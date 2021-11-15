// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Nintendo64.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Byte addressable image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Nintendo 64 cartridge dumps.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages.ByteAddressable;

/// <inheritdoc />
/// <summary>Implements support for Nintendo 64 cartridge dumps</summary>
public class Nintendo64 : IByteAddressableImage
{
    byte[]    _data;
    Stream    _dataStream;
    ImageInfo _imageInfo;
    bool      _interleaved;
    bool      _littleEndian;
    bool      _opened;
    long      _position;
    /// <inheritdoc />
    public string Author => "Natalia Portillo";
    /// <inheritdoc />
    public CICMMetadataType CicmMetadata => null;
    /// <inheritdoc />
    public List<DumpHardwareType> DumpHardware => null;
    /// <inheritdoc />
    public string Format => !_opened
                                ? "Nintendo 64 cartridge dump"
                                : _interleaved
                                    ? "Doctor V64"
                                    : "Mr. Backup Z64";
    /// <inheritdoc />
    public Guid Id => new("EF1B4319-48A0-4EEC-B8E8-D0EA36F8CC92");
    /// <inheritdoc />
    public ImageInfo Info => _imageInfo;
    /// <inheritdoc />
    public string Name => "Nintendo 64";

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        if(imageFilter == null)
            return false;

        Stream stream = imageFilter.GetDataForkStream();

        // Not sure but seems to be a multiple of at least this, maybe more
        if(stream.Length % 512 != 0)
            return false;

        stream.Position = 0;
        byte[] magicBytes = new byte[4];
        stream.Read(magicBytes, 0, 4);
        uint magic = BitConverter.ToUInt32(magicBytes, 0);

        switch(magic)
        {
            case 0x80371240:
            case 0x80371241:
            case 0x40123780:
            case 0x41123780:
            case 0x12408037:
            case 0x12418037:
            case 0x37804012:
            case 0x37804112: return true;
            default: return false;
        }
    }

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        if(imageFilter == null)
            return ErrorNumber.NoSuchFile;

        Stream stream = imageFilter.GetDataForkStream();

        // Not sure but seems to be a multiple of at least this, maybe more
        if(stream.Length % 512 != 0)
            return ErrorNumber.InvalidArgument;

        stream.Position = 0;
        byte[] magicBytes = new byte[4];
        stream.Read(magicBytes, 0, 4);
        uint magic = BitConverter.ToUInt32(magicBytes, 0);

        switch(magic)
        {
            case 0x80371240:
            case 0x80371241:
                _interleaved  = false;
                _littleEndian = true;

                break;
            case 0x40123780:
            case 0x41123780:
                _interleaved  = false;
                _littleEndian = false;

                break;
            case 0x12408037:
            case 0x12418037:
                _interleaved  = true;
                _littleEndian = false;

                break;
            case 0x37804012:
            case 0x37804112:
                _interleaved  = true;
                _littleEndian = false;

                break;
            default: return ErrorNumber.InvalidArgument;
        }

        _data           = new byte[imageFilter.DataForkLength];
        stream.Position = 0;
        stream.Read(_data, 0, (int)imageFilter.DataForkLength);

        _imageInfo = new ImageInfo
        {
            Application          = _interleaved ? "Doctor V64" : "Mr. Backup Z64",
            CreationTime         = imageFilter.CreationTime,
            ImageSize            = (ulong)imageFilter.DataForkLength,
            MediaType            = MediaType.N64GamePak,
            LastModificationTime = imageFilter.LastWriteTime,
            Sectors              = (ulong)imageFilter.DataForkLength,
            XmlMediaType         = XmlMediaType.LinearMedia
        };

        if(_littleEndian)
        {
            byte[] tmp = new byte[_data.Length];

            for(int i = 0; i < _data.Length; i += 4)
            {
                tmp[i] = _data[i + 3];
                tmp[i            + 1] = _data[i + 2];
                tmp[i            + 2] = _data[i + 1];
                tmp[i            + 3] = _data[i];
            }

            _data = tmp;
        }

        if(_interleaved)
        {
            byte[] tmp = new byte[_data.Length];

            for(int i = 0; i < _data.Length; i += 2)
            {
                tmp[i] = _data[i + 1];
                tmp[i            + 1] = _data[i];
            }

            _data = tmp;
        }

        Header   header = Marshal.ByteArrayToStructureBigEndian<Header>(_data, 0, Marshal.SizeOf<Header>());
        Encoding encoding;

        try
        {
            encoding = Encoding.GetEncoding("shift_jis");
        }
        catch
        {
            encoding = Encoding.ASCII;
        }

        _imageInfo.MediaPartNumber = StringHandlers.SpacePaddedToString(header.CartridgeId, encoding);
        _imageInfo.MediaTitle      = StringHandlers.SpacePaddedToString(header.Name, encoding);

        var sb = new StringBuilder();

        sb.AppendFormat("Name: {0}", _imageInfo.MediaTitle).AppendLine();
        sb.AppendFormat("Region: {0}", DecodeCountryCode(header.CountryCode)).AppendLine();
        sb.AppendFormat("Cartridge ID: {0}", _imageInfo.MediaPartNumber).AppendLine();
        sb.AppendFormat("Cartridge type: {0}", (char)header.CartridgeType).AppendLine();
        sb.AppendFormat("Version: {0}.{1}", (header.Version / 10) + 1, header.Version % 10).AppendLine();
        sb.AppendFormat("CRC1: 0x{0:X8}", header.Crc1).AppendLine();
        sb.AppendFormat("CRC2: 0x{0:X8}", header.Crc2).AppendLine();

        _imageInfo.Comments = sb.ToString();
        _opened             = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public long Position { get; set; }

    /// <inheritdoc />
    public ErrorNumber Create(string path, MediaType mediaType, Dictionary<string, string> options, long maximumSize)
    {
        if(_opened)
        {
            ErrorMessage = "Cannot create an opened image";

            return ErrorNumber.InvalidArgument;
        }

        if(mediaType != MediaType.N64GamePak)
        {
            ErrorMessage = $"Unsupported media format {mediaType}";

            return ErrorNumber.NotSupported;
        }

        _imageInfo = new ImageInfo
        {
            MediaType = mediaType,
            Sectors   = (ulong)maximumSize
        };

        string extension = Path.GetExtension(path).ToLowerInvariant();

        if(extension == ".v64")
            _interleaved = true;

        try
        {
            _dataStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch(IOException e)
        {
            ErrorMessage = $"Could not create new image file, exception {e.Message}";

            return ErrorNumber.InOutError;
        }

        _imageInfo.MediaType = mediaType;
        IsWriting            = true;
        _opened              = true;
        _data                = new byte[maximumSize];

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetHeader(out byte[] header)
    {
        header = null;

        return !_opened ? ErrorNumber.NotOpened : ErrorNumber.NoData;
    }

    /// <inheritdoc />
    public ErrorNumber GetMappings(out object mappings)
    {
        mappings = null;

        // TODO: Implement
        if(_opened)
            return ErrorNumber.NotImplemented;

        ErrorMessage = "Not image has been opened.";

        return ErrorNumber.NotOpened;
    }

    /// <inheritdoc />
    public ErrorNumber ReadByte(out byte b, bool advance = true) => ReadByteAt(_position, out b, advance);

    /// <inheritdoc />
    public ErrorNumber ReadByteAt(long position, out byte b, bool advance = true)
    {
        b = 0;

        if(!_opened)
        {
            ErrorMessage = "Not image has been opened.";

            return ErrorNumber.NotOpened;
        }

        if(position >= _data.Length)
        {
            ErrorMessage = "The requested position is out of range.";

            return ErrorNumber.OutOfRange;
        }

        b = _data[position];

        if(advance)
            _position = position + 1;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadBytes(byte[] buffer, int offset, int bytesToRead, out int bytesRead, bool advance = true) =>
        ReadBytesAt(_position, buffer, offset, bytesToRead, out bytesRead, advance);

    /// <inheritdoc />
    public ErrorNumber ReadBytesAt(long position, byte[] buffer, int offset, int bytesToRead, out int bytesRead,
                                   bool advance = true)
    {
        bytesRead = 0;

        if(!_opened)
        {
            ErrorMessage = "Not image has been opened.";

            return ErrorNumber.NotOpened;
        }

        if(position >= _data.Length)
        {
            ErrorMessage = "The requested position is out of range.";

            return ErrorNumber.OutOfRange;
        }

        if(buffer is null)
        {
            ErrorMessage = "Buffer must not be null.";

            return ErrorNumber.InvalidArgument;
        }

        if(offset + bytesToRead > buffer.Length)
            bytesRead = buffer.Length - offset;

        if(position + bytesToRead > _data.Length)
            bytesToRead = (int)(_data.Length - position);

        Array.Copy(_data, position, buffer, offset, bytesToRead);

        if(advance)
            _position = position + bytesToRead;

        bytesRead = bytesToRead;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber SetHeader(byte[] header)
    {
        if(!_opened)
        {
            ErrorMessage = "Not image has been opened.";

            return ErrorNumber.NotOpened;
        }

        if(IsWriting)
            return ErrorNumber.NotSupported;

        ErrorMessage = "Image is not opened for writing.";

        return ErrorNumber.ReadOnly;
    }

    /// <inheritdoc />
    public ErrorNumber SetMappings(object mappings)
    {
        if(!_opened)
        {
            ErrorMessage = "Not image has been opened.";

            return ErrorNumber.NotOpened;
        }

        // TODO: Implement
        if(IsWriting)
            return ErrorNumber.NotImplemented;

        ErrorMessage = "Image is not opened for writing.";

        return ErrorNumber.ReadOnly;
    }

    /// <inheritdoc />
    public ErrorNumber WriteByte(byte b, bool advance = true) => WriteByteAt(_position, b, advance);

    /// <inheritdoc />
    public ErrorNumber WriteByteAt(long position, byte b, bool advance = true)
    {
        if(!_opened)
        {
            ErrorMessage = "Not image has been opened.";

            return ErrorNumber.NotOpened;
        }

        if(!IsWriting)
        {
            ErrorMessage = "Image is not opened for writing.";

            return ErrorNumber.ReadOnly;
        }

        if(position >= _data.Length)
        {
            ErrorMessage = "The requested position is out of range.";

            return ErrorNumber.OutOfRange;
        }

        _data[position] = b;

        if(advance)
            _position = position + 1;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber WriteBytes(byte[] buffer, int offset, int bytesToWrite, out int bytesWritten,
                                  bool advance = true) =>
        WriteBytesAt(_position, buffer, offset, bytesToWrite, out bytesWritten, advance);

    /// <inheritdoc />
    public ErrorNumber WriteBytesAt(long position, byte[] buffer, int offset, int bytesToWrite, out int bytesWritten,
                                    bool advance = true)
    {
        bytesWritten = 0;

        if(!_opened)
        {
            ErrorMessage = "Not image has been opened.";

            return ErrorNumber.NotOpened;
        }

        if(!IsWriting)
        {
            ErrorMessage = "Image is not opened for writing.";

            return ErrorNumber.ReadOnly;
        }

        if(position >= _data.Length)
        {
            ErrorMessage = "The requested position is out of range.";

            return ErrorNumber.OutOfRange;
        }

        if(buffer is null)
        {
            ErrorMessage = "Buffer must not be null.";

            return ErrorNumber.InvalidArgument;
        }

        if(offset + bytesToWrite > buffer.Length)
            bytesToWrite = buffer.Length - offset;

        if(position + bytesToWrite > _data.Length)
            bytesToWrite = (int)(_data.Length - position);

        Array.Copy(buffer, offset, _data, position, bytesToWrite);

        if(advance)
            _position = position + bytesToWrite;

        bytesWritten = bytesToWrite;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public string ErrorMessage { get; private set; }
    /// <inheritdoc />
    public bool IsWriting { get; private set; }
    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[]
    {
        ".n64", ".v64", ".z64"
    };
    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Array.Empty<MediaTagType>();
    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        MediaType.N64GamePak
    };
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
        Array.Empty<(string name, Type type, string description, object @default)>();
    /// <inheritdoc />
    public IEnumerable<SectorTagType> SupportedSectorTags => Array.Empty<SectorTagType>();

    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint sectorSize) => Create(path, mediaType, options, (long)sectors) == ErrorNumber.NoError;

    /// <inheritdoc />
    public bool Close()
    {
        if(!_opened)
        {
            ErrorMessage = "Not image has been opened.";

            return false;
        }

        if(!IsWriting)
        {
            ErrorMessage = "Image is not opened for writing.";

            return false;
        }

        if(_interleaved)
        {
            byte[] tmp = new byte[_data.Length];

            for(int i = 0; i < _data.Length; i += 2)
            {
                tmp[i] = _data[i + 1];
                tmp[i            + 1] = _data[i];
            }

            _data = tmp;
        }

        _dataStream.Position = 0;
        _dataStream.Write(_data, 0, _data.Length);
        _dataStream.Close();

        return true;
    }

    /// <inheritdoc />
    public bool SetCicmMetadata(CICMMetadataType metadata) => false;

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

    /// <inheritdoc />
    public bool SetMetadata(ImageInfo metadata) => true;

    static string DecodeCountryCode(byte countryCode) => countryCode switch
    {
        0x37 => "Beta",
        0x41 => "Asia (NTSC)",
        0x42 => "Brazil",
        0x43 => "China",
        0x44 => "Germany",
        0x45 => "North America",
        0x46 => "France",
        0x47 => "Gateway 64 (NTSC)",
        0x48 => "Netherlands",
        0x49 => "Italy",
        0x4A => "Japan",
        0x4B => "Korea",
        0x4C => "Gateway 64 (PAL)",
        0x4E => "Canada",
        0x50 => "Europe",
        0x53 => "Spain",
        0x55 => "Australia",
        0x57 => "Scandinavia",
        0x58 => "Europe",
        0x59 => "Europe",
        _    => "Unknown"
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1), SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    struct Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] Validation;
        public readonly byte Compression;
        public readonly byte Padding1;
        public readonly uint ClockRate;
        public readonly uint ProgramCounter;
        public readonly uint ReleaseAddress;
        public readonly uint Crc1;
        public readonly uint Crc2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] Padding2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] Name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public readonly byte[] Padding3;
        /// <summary>'N' for cart, 'D' for 64DD, 'C' for expandable cart, 'E' for 64DD expansion, 'Z' for Aleck64</summary>
        public readonly byte CartridgeType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] CartridgeId;
        public readonly byte CountryCode;
        public readonly byte Version;
    }
}