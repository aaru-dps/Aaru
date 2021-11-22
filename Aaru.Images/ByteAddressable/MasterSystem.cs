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
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages.ByteAddressable;

public class MasterSystem : IByteAddressableImage
{
    byte[]    _data;
    Stream    _dataStream;
    bool      _gameGear;
    ImageInfo _imageInfo;
    bool      _opened;
    int       _romSize;
    /// <inheritdoc />
    public string Author => "Natalia Portillo";
    /// <inheritdoc />
    public CICMMetadataType CicmMetadata => null;
    /// <inheritdoc />
    public List<DumpHardwareType> DumpHardware => null;
    /// <inheritdoc />
    public string Format => _gameGear ? "Sega Game Gear cartridge dump" : "Sega Master System cartridge dump";
    /// <inheritdoc />
    public Guid Id => new("B0C02927-890D-41D0-8E95-C5D9A2A74131");
    /// <inheritdoc />
    public ImageInfo Info => _imageInfo;
    /// <inheritdoc />
    public string Name => "Sega Game Gear / Master System";

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        if(imageFilter == null)
            return false;

        Stream stream = imageFilter.GetDataForkStream();

        // Not sure but seems to be a multiple of at least this
        if(stream.Length % 8192 != 0)
            return false;

        stream.Position = 0x7ff0;
        byte[] magicBytes = new byte[8];
        stream.Read(magicBytes, 0, 8);
        ulong magic = BitConverter.ToUInt64(magicBytes, 0);

        if(magic == 0x4147455320524D54)
            return true;

        stream.Position = 0x3ff0;
        magicBytes      = new byte[8];
        stream.Read(magicBytes, 0, 8);
        magic = BitConverter.ToUInt64(magicBytes, 0);

        if(magic == 0x4147455320524D54)
            return true;

        stream.Position = 0x1ff0;
        magicBytes      = new byte[8];
        stream.Read(magicBytes, 0, 8);
        magic = BitConverter.ToUInt64(magicBytes, 0);

        return magic == 0x4147455320524D54;
    }

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        if(imageFilter == null)
            return ErrorNumber.NoSuchFile;

        Stream stream = imageFilter.GetDataForkStream();

        // Not sure but seems to be a multiple of at least this, maybe more
        if(stream.Length % 8192 != 0)
            return ErrorNumber.InvalidArgument;

        int headerPosition = 0;

        stream.Position = 0x7ff0;
        byte[] magicBytes = new byte[8];
        stream.Read(magicBytes, 0, 8);
        ulong magic = BitConverter.ToUInt64(magicBytes, 0);

        if(magic != 0x0B000DCC6666EDCE)
            headerPosition = 0x7ff0;
        else
        {
            stream.Position = 0x3ff0;
            magicBytes      = new byte[8];
            stream.Read(magicBytes, 0, 8);
            magic = BitConverter.ToUInt64(magicBytes, 0);

            if(magic != 0x0B000DCC6666EDCE)
                headerPosition = 0x3ff0;
            else
            {
                stream.Position = 0x1ff0;
                magicBytes      = new byte[8];
                stream.Read(magicBytes, 0, 8);
                magic = BitConverter.ToUInt64(magicBytes, 0);

                if(magic != 0x0B000DCC6666EDCE)
                    headerPosition = 0x1ff0;
                else
                    return ErrorNumber.InvalidArgument;
            }
        }

        _data           = new byte[imageFilter.DataForkLength];
        stream.Position = 0;
        stream.Read(_data, 0, (int)imageFilter.DataForkLength);

        _imageInfo = new ImageInfo
        {
            Application          = "Multi Game Doctor 2",
            CreationTime         = imageFilter.CreationTime,
            ImageSize            = (ulong)imageFilter.DataForkLength,
            LastModificationTime = imageFilter.LastWriteTime,
            Sectors              = (ulong)imageFilter.DataForkLength,
            XmlMediaType         = XmlMediaType.LinearMedia
        };

        Header header = Marshal.ByteArrayToStructureBigEndian<Header>(_data, headerPosition, Marshal.SizeOf<Header>());

        var sb = new StringBuilder();

        int productCode = (header.ProductCode[0] & 0xF) + ((header.ProductCode[0] & 0xF0) * 10) +
                          ((header.ProductCode[1] & 0xF) * 100) + ((header.ProductCode[1] & 0xF0) * 1000) +
                          (((header.VersionAndProduct & 0xF0) >> 4) * 10000);

        sb.AppendFormat("Product code: {0}", productCode).AppendLine();

        int    regionCode = (header.SizeAndRegion & 0xF0) >> 4;
        string region;
        string cartType;

        switch(regionCode)
        {
            case 3:
                region   = "Japan";
                cartType = "Master System";

                break;
            case 4:
                region   = "Export";
                cartType = "Master System";

                break;
            case 5:
                region    = "Japan";
                cartType  = "Game Gear";
                _gameGear = true;

                break;
            case 6:
                region    = "Export";
                cartType  = "Game Gear";
                _gameGear = true;

                break;
            case 7:
                region    = "International";
                cartType  = "Game Gear";
                _gameGear = true;

                break;
            default:
                region   = "Unknown";
                cartType = "Unknown";

                break;
        }

        _imageInfo.MediaType = _gameGear ? MediaType.GameGearCartridge : MediaType.MasterSystemCartridge;

        int sizeCode = header.SizeAndRegion & 0xF;

        _romSize = sizeCode switch
        {
            0   => 262144,
            1   => 524288,
            2   => 1048576,
            0xA => 8192,
            0xB => 16384,
            0xC => 32768,
            0xD => 49152,
            0xE => 65536,
            0xF => 131072,
            _   => 0
        };

        sb.AppendFormat("Region: {0}", region).AppendLine();
        sb.AppendFormat("Cartridge type: {0}", cartType).AppendLine();
        sb.AppendFormat("ROM size: {0} bytes", _romSize).AppendLine();
        sb.AppendFormat("Revision: {0}", header.VersionAndProduct & 0xF).AppendLine();
        sb.AppendFormat("Checksum: 0x{0:X4}", header.Checksum).AppendLine();

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
        ".sms", ".gg"
    };
    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Array.Empty<MediaTagType>();
    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        MediaType.MasterSystemCartridge, MediaType.GameGearCartridge
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

        if(mediaType != MediaType.GameBoyGamePak)
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
                        Length = (ulong)_romSize
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

    [StructLayout(LayoutKind.Sequential, Pack = 1), SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    struct Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Signature;
        public ushort Reserved;
        public ushort Checksum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] ProductCode;
        public byte VersionAndProduct;
        public byte SizeAndRegion;
    }
}