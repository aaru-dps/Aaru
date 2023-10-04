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

public class AtariLynx : IByteAddressableImage
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
    public string Format => "Atari Lynx cartridge dump";

    /// <inheritdoc />
    public Guid Id => new("809A6835-0486-4FD3-BD8B-2EF40C3EF97B");

    /// <inheritdoc />
    // ReSharper disable once ConvertToAutoProperty
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Name => Localization.AtariLynx_Name;

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        if(imageFilter == null)
            return false;

        Stream stream = imageFilter.GetDataForkStream();

        // Not sure but seems to be a multiple of at least this
        if((stream.Length - 64) % 65536 != 0)
            return false;

        stream.Position = 0;
        var magicBytes = new byte[4];
        stream.EnsureRead(magicBytes, 0, 4);
        var magic = BitConverter.ToUInt32(magicBytes, 0);

        // "LYNX"
        return magic == 0x584E594C;
    }

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        if(imageFilter == null)
            return ErrorNumber.NoSuchFile;

        Stream stream = imageFilter.GetDataForkStream();

        // Not sure but seems to be a multiple of at least this, maybe more
        if((stream.Length - 64) % 65536 != 0)
            return ErrorNumber.InvalidArgument;

        stream.Position = 0x0;
        var magicBytes = new byte[4];
        stream.EnsureRead(magicBytes, 0, 4);
        var magic = BitConverter.ToUInt32(magicBytes, 0);

        if(magic != 0x584E594C)
            return ErrorNumber.InvalidArgument;

        var headerBytes = new byte[64];
        stream.Position = 0;
        stream.EnsureRead(headerBytes, 0, 64);

        _data = new byte[imageFilter.DataForkLength - 64];
        stream.EnsureRead(_data, 0, (int)imageFilter.DataForkLength - 64);

        _imageInfo = new ImageInfo
        {
            Application          = "Handy",
            CreationTime         = imageFilter.CreationTime,
            ImageSize            = (ulong)imageFilter.DataForkLength,
            MediaType            = MediaType.AtariLynxCard,
            LastModificationTime = imageFilter.LastWriteTime,
            Sectors              = (ulong)imageFilter.DataForkLength,
            MetadataMediaType    = MetadataMediaType.LinearMedia
        };

        HandyHeader header = Marshal.ByteArrayToStructureBigEndian<HandyHeader>(headerBytes, 0, 64);

        if(header.Version != 256)
            return ErrorNumber.NotSupported;

        _imageInfo.MediaTitle        = StringHandlers.CToString(header.Name);
        _imageInfo.MediaManufacturer = StringHandlers.CToString(header.Manufacturer);

        var sb = new StringBuilder();

        sb.AppendFormat(Localization.Name_0,         _imageInfo.MediaTitle).AppendLine();
        sb.AppendFormat(Localization.Manufacturer_0, _imageInfo.MediaManufacturer).AppendLine();

        sb.AppendFormat(Localization.Bank_zero_size_0_pages_1_bytes, header.Bank0Length, header.Bank0Length * 65536).
           AppendLine();

        sb.AppendFormat(Localization.Bank_one_size_0_pages_1_bytes, header.Bank1Length, header.Bank1Length * 65536).
           AppendLine();

        sb.AppendFormat(Localization.Rotation_0, header.Rotation).AppendLine();

        _imageInfo.Comments = sb.ToString();
        _opened             = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public string ErrorMessage { get; private set; }

    /// <inheritdoc />
    public bool IsWriting { get; private set; }

    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[] { ".lnx" };

    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Array.Empty<MediaTagType>();

    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[] { MediaType.AtariLynxCard };

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

        HandyHeader header = new()
        {
            Bank0Length  = (short)(_data.Length > 4 * 131072 ? 4 * 131072                  / 256 : _data.Length / 256),
            Bank1Length  = (short)(_data.Length > 4 * 131072 ? (_data.Length - 4 * 131072) / 256 : 0),
            Magic        = 0x584E594C,
            Manufacturer = new byte[16],
            Name         = new byte[32],
            Spare        = new byte[5],
            Version      = 256
        };

        byte[] tmp = Encoding.ASCII.GetBytes(_imageInfo.MediaTitle[..32]);
        Array.Copy(tmp, 0, header.Name, 0, tmp.Length);
        tmp = Encoding.ASCII.GetBytes(_imageInfo.MediaManufacturer[..16]);
        Array.Copy(tmp, 0, header.Manufacturer, 0, tmp.Length);

        byte[] headerBytes = Marshal.StructureToByteArrayBigEndian(header);

        _dataStream.Write(headerBytes, 0, headerBytes.Length);
        _dataStream.Write(_data,       0, _data.Length);
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
    public bool SetImageInfo(ImageInfo imageInfo)
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

        if(!string.IsNullOrWhiteSpace(imageInfo.MediaTitle))
            _imageInfo.MediaTitle = imageInfo.MediaTitle[..32];

        if(!string.IsNullOrWhiteSpace(imageInfo.MediaManufacturer))
            _imageInfo.MediaManufacturer = imageInfo.MediaManufacturer[..16];

        return true;
    }

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

        if(mediaType != MediaType.AtariLynxCard)
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

        var foundRom = false;

        // Sanitize
        foreach(LinearMemoryDevice map in mappings.Devices)
        {
            switch(map.Type)
            {
                case LinearMemoryType.ROM when !foundRom:
                    foundRom = true;

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

#region Nested type: HandyHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    struct HandyHeader
    {
        public uint  Magic;
        public short Bank0Length;
        public short Bank1Length;
        public short Version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] Name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Manufacturer;
        public byte Rotation;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] Spare;
    }

#endregion
}