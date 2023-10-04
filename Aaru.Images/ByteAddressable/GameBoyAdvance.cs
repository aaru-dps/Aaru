using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages;

public class GameBoyAdvance : IByteAddressableImage
{
    byte[]    _data;
    Stream    _dataStream;
    ImageInfo _imageInfo;
    bool      _opened;

#region IByteAddressableImage Members

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public Metadata AaruMetadata => null;

    /// <inheritdoc />
    public List<DumpHardware> DumpHardware => null;

    /// <inheritdoc />
    public string Format => "Nintendo Game Boy Advance cartridge dump";

    /// <inheritdoc />
    public Guid Id => new("0040DDEB-3902-4402-9028-62915C5AA81F");

    /// <inheritdoc />
    // ReSharper disable once ConvertToAutoProperty
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Name => Localization.GameBoyAdvance_Name;

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        if(imageFilter == null)
            return false;

        Stream stream = imageFilter.GetDataForkStream();

        // Not sure but seems to be a multiple of at least this
        if(stream.Length % 32768 != 0)
            return false;

        stream.Position = 4;
        var magicBytes = new byte[8];
        stream.EnsureRead(magicBytes, 0, 8);
        var magic = BitConverter.ToUInt64(magicBytes, 0);

        return magic == 0x21A29A6951AEFF24;
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

        stream.Position = 4;
        var magicBytes = new byte[8];
        stream.EnsureRead(magicBytes, 0, 8);
        var magic = BitConverter.ToUInt64(magicBytes, 0);

        if(magic != 0x21A29A6951AEFF24)
            return ErrorNumber.InvalidArgument;

        _data           = new byte[imageFilter.DataForkLength];
        stream.Position = 0;
        stream.EnsureRead(_data, 0, (int)imageFilter.DataForkLength);

        _imageInfo = new ImageInfo
        {
            CreationTime         = imageFilter.CreationTime,
            ImageSize            = (ulong)imageFilter.DataForkLength,
            MediaType            = MediaType.GameBoyAdvanceGamePak,
            LastModificationTime = imageFilter.LastWriteTime,
            Sectors              = (ulong)imageFilter.DataForkLength,
            MetadataMediaType    = MetadataMediaType.LinearMedia
        };

        Header header = Marshal.ByteArrayToStructureBigEndian<Header>(_data, 0, Marshal.SizeOf<Header>());

        _imageInfo.MediaTitle = StringHandlers.CToString(header.Name);

        var sb = new StringBuilder();

        sb.AppendFormat(Localization.Name_0, _imageInfo.MediaTitle).AppendLine();

        sb.AppendFormat(Localization.Device_type_0,      header.DeviceType).AppendLine();
        sb.AppendFormat(Localization.Console_type_0,     header.UnitCode).AppendLine();
        sb.AppendFormat(Localization.Product_code_AGB_0, StringHandlers.CToString(header.Code)).AppendLine();
        sb.AppendFormat(Localization.Maker_code_0,       StringHandlers.CToString(header.Maker)).AppendLine();
        sb.AppendFormat(Localization.Revision_0,         header.Revision).AppendLine();
        sb.AppendFormat(Localization.Header_checksum_0,  header.ComplementCheck).AppendLine();

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
        ".gba"
    };

    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Array.Empty<MediaTagType>();

    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        MediaType.GameBoyAdvanceGamePak
    };

    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
        Array.Empty<(string name, Type type, string description, object @default)>();

    /// <inheritdoc />
    public IEnumerable<SectorTagType> SupportedSectorTags => Array.Empty<SectorTagType>();

    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint   sectorSize) => Create(path, mediaType, options, (long)sectors) == ErrorNumber.NoError;

    /// <inheritdoc />
    public bool Close()
    {
        if(!_opened)
        {
            ErrorMessage = Localization.No_image_has_been_opened;

            return false;
        }

        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return false;
        }

        _dataStream.Position = 0;
        _dataStream.Write(_data, 0, _data.Length);
        _dataStream.Close();

        IsWriting = false;
        _opened   = false;

        return true;
    }

    /// <inheritdoc />
    public bool SetMetadata(Metadata metadata) => false;

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardware> dumpHardware) => false;

    /// <inheritdoc />
    public bool SetImageInfo(ImageInfo imageInfo) => true;

    /// <inheritdoc />
    public long Position { get; set; }

    /// <inheritdoc />
    public ErrorNumber Create(string path, MediaType mediaType, Dictionary<string, string> options, long maximumSize)
    {
        if(_opened)
        {
            ErrorMessage = Localization.Cannot_create_an_opened_image;

            return ErrorNumber.InvalidArgument;
        }

        if(mediaType != MediaType.GameBoyAdvanceGamePak)
        {
            ErrorMessage = string.Format(Localization.Unsupported_media_format_0, mediaType);

            return ErrorNumber.NotSupported;
        }

        _imageInfo = new ImageInfo
        {
            MediaType = mediaType,
            Sectors   = (ulong)maximumSize
        };

        try
        {
            _dataStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch(IOException e)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, e.Message);

            return ErrorNumber.InOutError;
        }

        _imageInfo.MediaType = mediaType;
        IsWriting            = true;
        _opened              = true;
        _data                = new byte[maximumSize];

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetMappings(out LinearMemoryMap mappings)
    {
        mappings = new LinearMemoryMap();

        if(!_opened)
        {
            ErrorMessage = Localization.No_image_has_been_opened;

            return ErrorNumber.NotOpened;
        }

        mappings = new LinearMemoryMap
        {
            Devices = new[]
            {
                new LinearMemoryDevice
                {
                    Type = LinearMemoryType.ROM,
                    PhysicalAddress = new LinearMemoryAddressing
                    {
                        Start  = 0,
                        Length = (ulong)_data.Length
                    }
                }
            }
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadByte(out byte b, bool advance = true) => ReadByteAt(Position, out b, advance);

    /// <inheritdoc />
    public ErrorNumber ReadByteAt(long position, out byte b, bool advance = true)
    {
        b = 0;

        if(!_opened)
        {
            ErrorMessage = Localization.No_image_has_been_opened;

            return ErrorNumber.NotOpened;
        }

        if(position >= _data.Length)
        {
            ErrorMessage = Localization.The_requested_position_is_out_of_range;

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
            ErrorMessage = Localization.No_image_has_been_opened;

            return ErrorNumber.NotOpened;
        }

        if(position >= _data.Length)
        {
            ErrorMessage = Localization.The_requested_position_is_out_of_range;

            return ErrorNumber.OutOfRange;
        }

        if(buffer is null)
        {
            ErrorMessage = Localization.Buffer_must_not_be_null;

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
    public ErrorNumber SetMappings(LinearMemoryMap mappings)
    {
        if(!_opened)
        {
            ErrorMessage = Localization.No_image_has_been_opened;

            return ErrorNumber.NotOpened;
        }

        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return ErrorNumber.ReadOnly;
        }

        var foundRom     = false;
        var foundSaveRam = false;

        // Sanitize
        foreach(LinearMemoryDevice map in mappings.Devices)
        {
            switch(map.Type)
            {
                case LinearMemoryType.ROM when !foundRom:
                    foundRom = true;

                    break;
                case LinearMemoryType.SaveRAM when !foundSaveRam:
                    foundSaveRam = true;

                    break;
                default:
                    return ErrorNumber.InvalidArgument;
            }
        }

        // Cannot save in this image format anyway
        return foundRom ? ErrorNumber.NoError : ErrorNumber.InvalidArgument;
    }

    /// <inheritdoc />
    public ErrorNumber WriteByte(byte b, bool advance = true) => WriteByteAt(Position, b, advance);

    /// <inheritdoc />
    public ErrorNumber WriteByteAt(long position, byte b, bool advance = true)
    {
        if(!_opened)
        {
            ErrorMessage = Localization.No_image_has_been_opened;

            return ErrorNumber.NotOpened;
        }

        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return ErrorNumber.ReadOnly;
        }

        if(position >= _data.Length)
        {
            ErrorMessage = Localization.The_requested_position_is_out_of_range;

            return ErrorNumber.OutOfRange;
        }

        _data[position] = b;

        if(advance)
            Position = position + 1;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber WriteBytes(byte[] buffer, int offset, int bytesToWrite, out int bytesWritten,
                                  bool   advance = true) =>
        WriteBytesAt(Position, buffer, offset, bytesToWrite, out bytesWritten, advance);

    /// <inheritdoc />
    public ErrorNumber WriteBytesAt(long position, byte[] buffer, int offset, int bytesToWrite, out int bytesWritten,
                                    bool advance = true)
    {
        bytesWritten = 0;

        if(!_opened)
        {
            ErrorMessage = Localization.No_image_has_been_opened;

            return ErrorNumber.NotOpened;
        }

        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return ErrorNumber.ReadOnly;
        }

        if(position >= _data.Length)
        {
            ErrorMessage = Localization.The_requested_position_is_out_of_range;

            return ErrorNumber.OutOfRange;
        }

        if(buffer is null)
        {
            ErrorMessage = Localization.Buffer_must_not_be_null;

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

#endregion

#region Nested type: Header

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    struct Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] EntryPoint;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 156)]
        public byte[] Logo;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] Name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Code;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] Maker;
        public byte Fixed;
        public byte UnitCode;
        public byte DeviceType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] Reserved;
        public byte Revision;
        public byte ComplementCheck;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] Reserved2;
    }

#endregion
}