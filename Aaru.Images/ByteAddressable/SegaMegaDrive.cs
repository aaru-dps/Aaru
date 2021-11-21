// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SegaMegaDrive.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Byte addressable image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Sega Mega Drive, 32X, Genesis and Pico cartridge dumps.
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
/// <summary>Implements support for Sega Mega Drive, 32X, Genesis and Pico cartridge dumps</summary>
public class SegaMegaDrive : IByteAddressableImage
{
    byte[]    _data;
    Stream    _dataStream;
    ImageInfo _imageInfo;
    bool      _interleaved;
    bool      _opened;
    bool      _smd;

    /// <inheritdoc />
    public string Author => "Natalia Portillo";
    /// <inheritdoc />
    public CICMMetadataType CicmMetadata => null;
    /// <inheritdoc />
    public List<DumpHardwareType> DumpHardware => null;
    /// <inheritdoc />
    public string Format => !_opened
                                ? "Mega Drive cartridge dump"
                                : _smd
                                    ? "Super Magic Drive"
                                    : _interleaved
                                        ? "Multi Game Doctor 2"
                                        : "Magicom";
    /// <inheritdoc />
    public Guid Id => new("7B1CE2E7-3BC4-4283-BFA4-F292D646DF15");
    /// <inheritdoc />
    public ImageInfo Info => _imageInfo;
    /// <inheritdoc />
    public string Name => "Sega Mega Drive / 32X / Pico";

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        if(imageFilter == null)
            return false;

        Stream stream = imageFilter.GetDataForkStream();
        stream.Position = 0;

        if(stream.Length % 512 != 0)
            return false;

        byte[] buffer = new byte[4];

        stream.Position = 256;
        stream.Read(buffer, 0, 4);

        // SEGA
        if(buffer[0] == 0x53 &&
           buffer[1] == 0x45 &&
           buffer[2] == 0x47 &&
           buffer[3] == 0x41)
            return true;

        // EA
        if(buffer[0] == 0x45 &&
           buffer[1] == 0x41)
        {
            stream.Position = (stream.Length / 2) + 256;
            stream.Read(buffer, 0, 2);

            // SG
            if(buffer[0] == 0x53 &&
               buffer[1] == 0x47)
                return true;
        }

        stream.Position = 512 + 128;
        stream.Read(buffer, 0, 4);

        // EA
        if(buffer[0] != 0x45 ||
           buffer[1] != 0x41)
            return false;

        stream.Position = 8832;
        stream.Read(buffer, 0, 2);

        // SG
        return buffer[0] == 0x53 && buffer[1] == 0x47;
    }

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        if(imageFilter == null)
            return ErrorNumber.NoSuchFile;

        Stream stream = imageFilter.GetDataForkStream();
        stream.Position = 0;

        if(stream.Length % 512 != 0)
            return ErrorNumber.InvalidArgument;

        byte[] buffer = new byte[4];

        stream.Position = 256;
        stream.Read(buffer, 0, 4);

        // SEGA
        bool found = buffer[0] == 0x53 && buffer[1] == 0x45 && buffer[2] == 0x47 && buffer[3] == 0x41;

        // EA
        if(buffer[0] == 0x45 &&
           buffer[1] == 0x41)
        {
            stream.Position = (stream.Length / 2) + 256;
            stream.Read(buffer, 0, 2);

            // SG
            if(buffer[0] == 0x53 &&
               buffer[1] == 0x47)
            {
                _interleaved = true;
                found        = true;
            }
        }

        stream.Position = 512 + 128;
        stream.Read(buffer, 0, 4);

        // EA
        if(buffer[0] == 0x45 &&
           buffer[1] == 0x41)
        {
            stream.Position = 8832;
            stream.Read(buffer, 0, 2);

            // SG
            if(buffer[0] == 0x53 &&
               buffer[1] == 0x47)
            {
                _smd  = true;
                found = true;
            }
        }

        if(!found)
            return ErrorNumber.InvalidArgument;

        _data           = new byte[_smd ? stream.Length - 512 : stream.Length];
        stream.Position = _smd ? 512 : 0;
        stream.Read(_data, 0, _data.Length);

        // Interleaves every 16KiB
        if(_smd)
        {
            byte[] tmp     = new byte[_data.Length];
            byte[] bankIn  = new byte[16384];
            byte[] bankOut = new byte[16384];

            for(int b = 0; b < _data.Length / 16384; b++)
            {
                Array.Copy(_data, b * 16384, bankIn, 0, 16384);

                for(int i = 0; i < 8192; i++)
                {
                    bankOut[(i * 2) + 1] = bankIn[i];
                    bankOut[i * 2]       = bankIn[i + 8192];
                }

                Array.Copy(bankOut, 0, tmp, b * 16384, 16384);
            }

            _data = tmp;
        }
        else if(_interleaved)
        {
            byte[] tmp  = new byte[_data.Length];
            int    half = _data.Length / 2;

            for(int i = 0; i < half; i++)
            {
                tmp[i * 2]       = _data[i];
                tmp[(i * 2) + 1] = _data[i + half];
            }

            _data = tmp;
        }

        SegaHeader header =
            Marshal.ByteArrayToStructureBigEndian<SegaHeader>(_data, 0x100, Marshal.SizeOf<SegaHeader>());

        Encoding encoding;

        try
        {
            encoding = Encoding.GetEncoding("shift_jis");
        }
        catch
        {
            encoding = Encoding.ASCII;
        }

        var sb = new StringBuilder();

        sb.AppendFormat("System type: {0}", StringHandlers.SpacePaddedToString(header.SystemType, encoding)).
           AppendLine();

        sb.AppendFormat("Copyright string: {0}", StringHandlers.SpacePaddedToString(header.Copyright, encoding)).
           AppendLine();

        sb.AppendFormat("Domestic title: {0}", StringHandlers.SpacePaddedToString(header.DomesticTitle, encoding)).
           AppendLine();

        sb.AppendFormat("Overseas title: {0}", StringHandlers.SpacePaddedToString(header.OverseasTitle, encoding)).
           AppendLine();

        sb.AppendFormat("Serial number: {0}", StringHandlers.SpacePaddedToString(header.SerialNumber, encoding)).
           AppendLine();

        sb.AppendFormat("Checksum: 0x{0:X4}", header.Checksum).AppendLine();

        sb.AppendFormat("Devices supported: {0}", StringHandlers.SpacePaddedToString(header.DeviceSupport, encoding)).
           AppendLine();

        sb.AppendFormat("ROM starts at 0x{0:X8} and ends at 0x{1:X8} ({2} bytes)", header.RomStart, header.RomEnd,
                        header.RomEnd - header.RomStart + 1).AppendLine();

        sb.AppendFormat("RAM starts at 0x{0:X8} and ends at 0x{1:X8} ({2} bytes)", header.RamStart, header.RamEnd,
                        header.RamEnd - header.RamStart + 1).AppendLine();

        if(header.ExtraRamPresent[0] == 0x52 &&
           header.ExtraRamPresent[1] == 0x41)
        {
            sb.AppendLine("Extra RAM present.");

            switch(header.ExtraRamType)
            {
                case 0xA0:
                    sb.AppendLine("Extra RAM uses 16-bit access.");

                    break;
                case 0xB0:
                    sb.AppendLine("Extra RAM uses 8-bit access (even addresses).");

                    break;
                case 0xB8:
                    sb.AppendLine("Extra RAM uses 8-bit access (odd addresses).");

                    break;
                case 0xE0:
                    sb.AppendLine("Extra RAM uses 16-bit access and persists when powered off.");

                    break;
                case 0xF0:
                    sb.AppendLine("Extra RAM uses 8-bit access (even addresses) and persists when powered off.");

                    break;
                case 0xF8:
                    sb.AppendLine("Extra RAM uses 8-bit access (odd addresses) and persists when powered off.");

                    break;
                default:
                    sb.AppendFormat("Extra RAM is of unknown type 0x{0:X2}", header.ExtraRamType);

                    break;
            }

            sb.AppendFormat("Extra RAM starts at 0x{0:X8} and ends at 0x{1:X8} ({2} bytes)", header.ExtraRamStart,
                            header.ExtraRamEnd,
                            (header.ExtraRamType & 0x10) == 0x10 ? (header.RomEnd - header.RomStart + 1) / 2
                                : header.RomEnd - header.RomStart + 1).AppendLine();
        }
        else
            sb.AppendLine("Extra RAM not present.");

        string modemSupport = StringHandlers.SpacePaddedToString(header.ModemSupport, encoding);

        if(!string.IsNullOrWhiteSpace(modemSupport))
            sb.AppendFormat("Modem support: {0}", modemSupport).AppendLine();

        sb.AppendFormat("Region support: {0}", StringHandlers.SpacePaddedToString(header.Region, encoding)).
           AppendLine();

        _imageInfo.ImageSize            = (ulong)stream.Length;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.MediaPartNumber      = StringHandlers.SpacePaddedToString(header.SerialNumber, encoding);
        _imageInfo.MediaTitle           = StringHandlers.SpacePaddedToString(header.DomesticTitle, encoding);

        _imageInfo.MediaType = StringHandlers.SpacePaddedToString(header.SystemType, encoding) switch
        {
            "SEGA 32X"  => MediaType._32XCartridge,
            "SEGA PICO" => MediaType.SegaPicoCartridge,
            _           => MediaType.MegaDriveCartridge
        };

        _imageInfo.Sectors      = (ulong)_data.Length;
        _imageInfo.XmlMediaType = XmlMediaType.LinearMedia;

        _imageInfo.Comments = sb.ToString();
        _opened             = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public string ErrorMessage { get; private set; }
    /// <inheritdoc />
    public bool IsWriting { get; private set; }
    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[]
    {
        ".smd", ".md", ".32x"
    };
    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Array.Empty<MediaTagType>();
    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        MediaType._32XCartridge, MediaType.MegaDriveCartridge, MediaType.SegaPicoCartridge
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
            byte[] tmp  = new byte[_data.Length];
            int    half = _data.Length / 2;

            for(int i = 0; i < half; i++)
            {
                tmp[i]        = _data[i * 2];
                tmp[i + half] = _data[(i * 2) + 1];
            }

            _data = tmp;
        }

        _dataStream.Position = 0;

        if(_smd)
        {
            byte[] smdHeader = Marshal.StructureToByteArrayLittleEndian(new SuperMagicDriveHeader
            {
                Empty     = new byte[501],
                FileType  = 6,
                ID0       = 3,
                ID1       = 0xAA,
                ID2       = 0xBB,
                PageCount = (byte)(_data.Length / 16384)
            });

            _dataStream.Write(smdHeader, 0, smdHeader.Length);

            byte[] tmp     = new byte[_data.Length];
            byte[] bankIn  = new byte[16384];
            byte[] bankOut = new byte[16384];

            for(int b = 0; b < _data.Length / 16384; b++)
            {
                Array.Copy(_data, b * 16384, bankIn, 0, 16384);

                for(int i = 0; i < 8192; i++)
                {
                    bankOut[i] = bankIn[(i * 2) + 1];
                    bankOut[i                   + 8192] = bankIn[i * 2];
                }

                Array.Copy(bankOut, 0, tmp, b * 16384, 16384);
            }

            _data = tmp;
        }

        _dataStream.Write(_data, 0, _data.Length);
        _dataStream.Close();

        IsWriting = false;
        _opened   = false;

        return true;
    }

    /// <inheritdoc />
    public bool SetCicmMetadata(CICMMetadataType metadata) => false;

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

    /// <inheritdoc />
    public bool SetMetadata(ImageInfo metadata) => true;

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

        if(mediaType != MediaType._32XCartridge      &&
           mediaType != MediaType.MegaDriveCartridge &&
           mediaType != MediaType.SegaPicoCartridge)
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

        if(extension == ".smd")
        {
            _interleaved = true;
            _smd         = true;
        }

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
    public ErrorNumber GetMappings(out LinearMemoryMap mappings)
    {
        mappings = new LinearMemoryMap();

        // TODO: Implement
        if(_opened)
            return ErrorNumber.NotImplemented;

        ErrorMessage = "Not image has been opened.";

        return ErrorNumber.NotOpened;
    }

    /// <inheritdoc />
    public ErrorNumber ReadByte(out byte b, bool advance = true) => ReadByteAt(Position, out b, advance);

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
            Position = position + 1;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadBytes(byte[] buffer, int offset, int bytesToRead, out int bytesRead, bool advance = true) =>
        ReadBytesAt(Position, buffer, offset, bytesToRead, out bytesRead, advance);

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
            Position = position + bytesToRead;

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
    public ErrorNumber SetMappings(LinearMemoryMap mappings)
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
    public ErrorNumber WriteByte(byte b, bool advance = true) => WriteByteAt(Position, b, advance);

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
            Position = position + 1;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber WriteBytes(byte[] buffer, int offset, int bytesToWrite, out int bytesWritten,
                                  bool advance = true) =>
        WriteBytesAt(Position, buffer, offset, bytesToWrite, out bytesWritten, advance);

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
            Position = position + bytesToWrite;

        bytesWritten = bytesToWrite;

        return ErrorNumber.NoError;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1), SuppressMessage("ReSharper", "MemberCanBePrivate.Local"),
     SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    struct SegaHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] SystemType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Copyright;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public byte[] DomesticTitle;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public byte[] OverseasTitle;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public byte[] SerialNumber;
        public ushort Checksum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] DeviceSupport;
        public uint RomStart;
        public uint RomEnd;
        public uint RamStart;
        public uint RamEnd;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] ExtraRamPresent;
        public byte ExtraRamType;
        public byte Padding;
        public uint ExtraRamStart;
        public uint ExtraRamEnd;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] ModemSupport;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] Padding2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Region;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
        public byte[] Padding3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1), SuppressMessage("ReSharper", "MemberCanBePrivate.Local"),
     SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    struct SuperMagicDriveHeader
    {
        /// <summary>16 KiB pages</summary>
        public byte PageCount;
        /// <summary>0x03 for Mega Drive</summary>
        public byte ID0;
        /// <summary>Not for Mega Drive</summary>
        public byte Unused;
        public byte Padding;
        public uint Reserved;
        /// <summary>0xAA</summary>
        public byte ID1;
        /// <summary>0xBB</summary>
        public byte ID2;
        /// <summary>0x06 for Mega Drive</summary>
        public byte FileType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 501)]
        public byte[] Empty;
    }
}