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

public class AtariLynx : IByteAddressableImage
{
    byte[]    _data;
    Stream    _dataStream;
    ImageInfo _imageInfo;
    bool      _opened;
    /// <inheritdoc />
    public string Author => "Natalia Portillo";
    /// <inheritdoc />
    public CICMMetadataType CicmMetadata => null;
    /// <inheritdoc />
    public List<DumpHardwareType> DumpHardware => null;
    /// <inheritdoc />
    public string Format => "Atari Lynx cartridge dump";
    /// <inheritdoc />
    public Guid Id => new("809A6835-0486-4FD3-BD8B-2EF40C3EF97B");
    /// <inheritdoc />
    public ImageInfo Info => _imageInfo;
    /// <inheritdoc />
    public string Name => "Atari Lynx";

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
        byte[] magicBytes = new byte[4];
        stream.Read(magicBytes, 0, 4);
        uint magic = BitConverter.ToUInt32(magicBytes, 0);

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
        byte[] magicBytes = new byte[4];
        stream.Read(magicBytes, 0, 4);
        uint magic = BitConverter.ToUInt32(magicBytes, 0);

        if(magic != 0x584E594C)
            return ErrorNumber.InvalidArgument;

        byte[] headerBytes = new byte[64];
        stream.Position = 0;
        stream.Read(headerBytes, 0, 64);

        _data = new byte[imageFilter.DataForkLength - 64];
        stream.Read(_data, 0, (int)imageFilter.DataForkLength - 64);

        _imageInfo = new ImageInfo
        {
            Application          = "Handy",
            CreationTime         = imageFilter.CreationTime,
            ImageSize            = (ulong)imageFilter.DataForkLength,
            MediaType            = MediaType.AtariLynxCard,
            LastModificationTime = imageFilter.LastWriteTime,
            Sectors              = (ulong)imageFilter.DataForkLength,
            XmlMediaType         = XmlMediaType.LinearMedia
        };

        HandyHeader header = Marshal.ByteArrayToStructureBigEndian<HandyHeader>(headerBytes, 0, 64);

        if(header.Version != 256)
            return ErrorNumber.NotSupported;

        _imageInfo.MediaTitle        = StringHandlers.CToString(header.Name);
        _imageInfo.MediaManufacturer = StringHandlers.CToString(header.Manufacturer);

        var sb = new StringBuilder();

        sb.AppendFormat("Name: {0}", _imageInfo.MediaTitle).AppendLine();
        sb.AppendFormat("Manufacturer: {0}", _imageInfo.MediaManufacturer).AppendLine();

        sb.AppendFormat("Bank 0 size: {0} pages ({1} bytes)", header.Bank0Length, header.Bank0Length * 65536).
           AppendLine();

        sb.AppendFormat("Bank 1 size: {0} pages ({1} bytes)", header.Bank1Length, header.Bank1Length * 65536).
           AppendLine();

        sb.AppendFormat("Rotation: {0}", header.Rotation).AppendLine();

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
        ".lnx"
    };
    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Array.Empty<MediaTagType>();
    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        MediaType.AtariLynxCard
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

        _dataStream.Position = 0;

        HandyHeader header = new()
        {
            Bank0Length  = (short)(_data.Length > 4 * 131072 ? 4 * 131072 / 256 : _data.Length / 256),
            Bank1Length  = (short)(_data.Length > 4 * 131072 ? (_data.Length - (4 * 131072)) / 256 : 0),
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
    public bool SetMetadata(ImageInfo metadata)
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

        if(!string.IsNullOrWhiteSpace(metadata.MediaTitle))
            _imageInfo.MediaTitle = metadata.MediaTitle[..32];

        if(!string.IsNullOrWhiteSpace(metadata.MediaManufacturer))
            _imageInfo.MediaManufacturer = metadata.MediaManufacturer[..16];

        return true;
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

        if(mediaType != MediaType.AtariLynxCard)
        {
            ErrorMessage = $"Unsupported media format {mediaType}";

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

        if(!_opened)
        {
            ErrorMessage = "Not image has been opened.";

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

        if(!IsWriting)
        {
            ErrorMessage = "Image is not opened for writing.";

            return ErrorNumber.ReadOnly;
        }

        bool foundRom = false;

        // Sanitize
        foreach(LinearMemoryDevice map in mappings.Devices)
        {
            switch(map.Type)
            {
                case LinearMemoryType.ROM when !foundRom:
                    foundRom = true;

                    break;
                default: return ErrorNumber.InvalidArgument;
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
}