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

public class GameBoy : IByteAddressableImage
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
    public string Format => "Nintendo Game Boy cartridge dump";
    /// <inheritdoc />
    public Guid Id => new("04AFDB93-587E-413B-9B52-10D4A92966CF");
    /// <inheritdoc />
    public ImageInfo Info => _imageInfo;
    /// <inheritdoc />
    public string Name => "Nintendo Game Boy";

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        if(imageFilter == null)
            return false;

        Stream stream = imageFilter.GetDataForkStream();

        // Not sure but seems to be a multiple of at least this
        if(stream.Length % 32768 != 0)
            return false;

        stream.Position = 0x104;
        byte[] magicBytes = new byte[8];
        stream.Read(magicBytes, 0, 8);
        ulong magic = BitConverter.ToUInt64(magicBytes, 0);

        return magic == 0x0B000DCC6666EDCE;
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

        stream.Position = 0x104;
        byte[] magicBytes = new byte[8];
        stream.Read(magicBytes, 0, 8);
        ulong magic = BitConverter.ToUInt64(magicBytes, 0);

        if(magic != 0x0B000DCC6666EDCE)
            return ErrorNumber.InvalidArgument;

        _data           = new byte[imageFilter.DataForkLength];
        stream.Position = 0;
        stream.Read(_data, 0, (int)imageFilter.DataForkLength);

        _imageInfo = new ImageInfo
        {
            Application          = "Multi Game Doctor 2",
            CreationTime         = imageFilter.CreationTime,
            ImageSize            = (ulong)imageFilter.DataForkLength,
            MediaType            = MediaType.GameBoyGamePak,
            LastModificationTime = imageFilter.LastWriteTime,
            Sectors              = (ulong)imageFilter.DataForkLength,
            XmlMediaType         = XmlMediaType.LinearMedia
        };

        Header header = Marshal.ByteArrayToStructureBigEndian<Header>(_data, 0x100, Marshal.SizeOf<Header>());

        byte[] name = new byte[(header.Name[^1] & 0x80) == 0x80 ? 15 : 16];
        Array.Copy(header.Name, 0, name, 0, name.Length);

        _imageInfo.MediaTitle = StringHandlers.CToString(name);

        var sb = new StringBuilder();

        sb.AppendFormat("Name: {0}", _imageInfo.MediaTitle).AppendLine();

        if((header.Name[^1] & 0xC0) == 0xC0)
            sb.AppendLine("Requires Game Boy Color");
        else
        {
            if((header.Name[^1] & 0xC0) == 0xC0)
                sb.AppendLine("Contains features for Game Boy Color");

            if(header.Sgb == 0x03)
                sb.AppendLine("Contains features for Super Game Boy");
        }

        sb.AppendFormat("Region: {0}", header.Country == 0 ? "Japan" : "World").AppendLine();
        sb.AppendFormat("Cartridge type: {0}", DecodeCartridgeType(header.RomType)).AppendLine();
        sb.AppendFormat("ROM size: {0} bytes", DecodeRomSize(header.RomSize)).AppendLine();

        if(header.SramSize > 0)
            sb.AppendFormat("Save RAM size: {0} bytes", DecodeSaveRamSize(header.SramSize)).AppendLine();

        sb.AppendFormat("Licensee: {0}", DecodeLicensee(header.Licensee, header.LicenseeNew)).AppendLine();
        sb.AppendFormat("Revision: {0}", header.Revision).AppendLine();
        sb.AppendFormat("Header checksum: 0x{0:X2}", header.HeaderChecksum).AppendLine();
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
        ".gb", ".gbc", ".sgb"
    };
    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Array.Empty<MediaTagType>();
    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        MediaType.GameBoyGamePak
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

        Header header = Marshal.ByteArrayToStructureBigEndian<Header>(_data, 0x100, Marshal.SizeOf<Header>());

        bool   hasMapper          = false;
        bool   hasSaveRam         = false;
        string mapperManufacturer = null;
        string mapperName         = null;

        switch(header.RomType)
        {
            case 0x01:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC1";

                break;
            case 0x02:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC1";
                hasSaveRam         = true;

                break;
            case 0x03:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC1";
                hasSaveRam         = true;

                break;
            case 0x05:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC2";

                break;
            case 0x06:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC2";

                break;
            case 0x08:
                hasSaveRam = true;

                break;
            case 0x09:
                hasSaveRam = true;

                break;
            case 0x0B:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MMM01";

                break;
            case 0x0C:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MMM01";
                hasSaveRam         = true;

                break;
            case 0x0D:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MMM01";
                hasSaveRam         = true;

                break;
            case 0x0F:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC3";

                break;
            case 0x10:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC3";
                hasSaveRam         = true;

                break;
            case 0x11:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC3";

                break;
            case 0x12:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC3";
                hasSaveRam         = true;

                break;
            case 0x13:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC3";
                hasSaveRam         = true;

                break;
            case 0x19:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC5";

                break;
            case 0x1A:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC5";
                hasSaveRam         = true;

                break;
            case 0x1B:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC5";
                hasSaveRam         = true;

                break;
            case 0x1C:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC5";

                break;
            case 0x1D:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC5";
                hasSaveRam         = true;

                break;
            case 0x1E:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC5";
                hasSaveRam         = true;

                break;
            case 0x20:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC6";

                break;
            case 0x22:
                hasMapper          = true;
                mapperManufacturer = "Nintendo";
                mapperName         = "MBC7";
                hasSaveRam         = true;

                break;
            case 0xFC: // Pocket Camera
                mappings = new LinearMemoryMap
                {
                    Devices = new[]
                    {
                        new LinearMemoryDevice
                        {
                            Location     = "U1",
                            Manufacturer = "Nintendo",
                            Model        = "MAC-GBD",
                            Package      = "LQFP-100",
                            Type         = LinearMemoryType.Processor
                        },
                        new LinearMemoryDevice
                        {
                            Location     = "U2",
                            Manufacturer = "Nintendo",
                            Model        = "GBD-PCAX-0",
                            Package      = "TSOP-32",
                            Type         = LinearMemoryType.ROM,
                            PhysicalAddress = new LinearMemoryAddressing
                            {
                                Start  = 0,
                                Length = DecodeRomSize(header.RomSize)
                            }
                        },
                        new LinearMemoryDevice
                        {
                            Location     = "U3",
                            Manufacturer = "Sharp",
                            Model        = "52CV1000SF85LL",
                            Package      = "TSOP-32",
                            Type         = LinearMemoryType.SaveRAM,
                            PhysicalAddress = new LinearMemoryAddressing
                            {
                                Start  = DecodeRomSize(header.RomSize),
                                Length = DecodeSaveRamSize(header.SramSize)
                            }
                        }
                    }
                };

                return ErrorNumber.NoError;
            case 0xFD:
                hasMapper          = true;
                mapperManufacturer = "Bandai";
                mapperName         = "TAMA5";

                break;
            case 0xFE:
                hasMapper          = true;
                mapperManufacturer = "Hudson";
                mapperName         = "HuC-3";

                break;
            case 0xFF:
                hasMapper          = true;
                mapperManufacturer = "Hudson";
                mapperName         = "HuC-1";

                break;
        }

        mappings = new LinearMemoryMap();

        if(header.SramSize > 0)
            hasSaveRam = true;

        int devices = 1;

        if(hasSaveRam)
            devices++;

        if(hasMapper)
            devices++;

        mappings.Devices = new LinearMemoryDevice[devices];

        mappings.Devices[0].Type = LinearMemoryType.ROM;

        mappings.Devices[0].PhysicalAddress = new LinearMemoryAddressing
        {
            Start  = 0,
            Length = DecodeRomSize(header.RomSize)
        };

        if(hasSaveRam)
        {
            mappings.Devices[1] = new LinearMemoryDevice
            {
                Type = LinearMemoryType.SaveRAM,
                PhysicalAddress = new LinearMemoryAddressing
                {
                    Start  = mappings.Devices[0].PhysicalAddress.Length,
                    Length = DecodeSaveRamSize(header.SramSize)
                }
            };
        }

        if(hasMapper)
        {
            mappings.Devices[^1] = new LinearMemoryDevice
            {
                Type         = LinearMemoryType.Mapper,
                Manufacturer = mapperManufacturer,
                Model        = mapperName
            };
        }

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

        bool foundRom     = false;
        bool foundSaveRam = false;
        bool foundMapper  = false;

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
                case LinearMemoryType.Mapper when !foundMapper:
                    foundMapper = true;

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

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    string DecodeLicensee(byte headerLicensee, byte[] headerLicenseeNew)
    {
        if(headerLicensee == 0x33)
        {
            string licenseeNew = StringHandlers.CToString(headerLicenseeNew);

            switch(licenseeNew)
            {
                case "00": return "none";
                case "01": return "Nintendo R&D1";
                case "08": return "Capcom";
                case "13": return "Electronic Arts";
                case "18": return "Hudson Soft";
                case "19": return "b-ai";
                case "20": return "kss";
                case "22": return "pow";
                case "24": return "PCM Complete";
                case "25": return "san-x";
                case "28": return "Kemco Japan";
                case "29": return "seta";
                case "30": return "Viacom";
                case "31": return "Nintendo";
                case "32": return "Bandai";
                case "33": return "Ocean / Acclaim";
                case "34": return "Konami";
                case "35": return "Hector";
                case "37": return "Taito";
                case "38": return "Hudson";
                case "39": return "Banpresto";
                case "41": return "Ubi Soft";
                case "42": return "Atlus";
                case "44": return "Malibu";
                case "46": return "angel";
                case "47": return "Bullet -Proof";
                case "49": return "irem";
                case "50": return "Absolute";
                case "51": return "Acclaim";
                case "52": return "Activision";
                case "53": return "American sammy";
                case "54": return "Konami";
                case "55": return "Hi tech entertainment";
                case "56": return "LJN";
                case "57": return "Matchbox";
                case "58": return "Mattel";
                case "59": return "Milton Bradley";
                case "60": return "Titus";
                case "61": return "Virgin";
                case "64": return "LucasArts";
                case "67": return "Ocean";
                case "69": return "Electronic Arts";
                case "70": return "Infogrames";
                case "71": return "Interplay";
                case "72": return "Brøderbund";
                case "73": return "sculptured";
                case "75": return "sci";
                case "78": return "THQ";
                case "79": return "Accolade";
                case "80": return "misawa";
                case "83": return "lozc";
                case "86": return "tokuma shoten i";
                case "87": return "tsukuda ori";
                case "91": return "Chunsoft";
                case "92": return "Video  system";
                case "93": return "Ocean / Acclaim";
                case "95": return "Varie";
                case "96": return "Yonezawa / s'pal";
                case "97": return "Kaneko";
                case "99": return "Pack in soft";
                case "A4": return "Konami";
                default:   return "Unknown";
            }
        }

        switch(headerLicensee)
        {
            case 0x00: return "none";
            case 0x01: return "nintendo";
            case 0x08: return "capcom";
            case 0x09: return "hot-b";
            case 0x0A: return "jaleco";
            case 0x0B: return "coconuts";
            case 0x0C: return "elite systems";
            case 0x13: return "electronic arts";
            case 0x18: return "hudsonsoft";
            case 0x19: return "itc entertainment";
            case 0x1A: return "yanoman";
            case 0x1D: return "clary";
            case 0x1F: return "virgin";
            case 0x20: return "KSS";
            case 0x24: return "pcm complete";
            case 0x25: return "san-x";
            case 0x28: return "kotobuki systems";
            case 0x29: return "seta";
            case 0x30: return "infogrames";
            case 0x31: return "nintendo";
            case 0x32: return "bandai";
            case 0x33: return "'''GBC - see above'''";
            case 0x34: return "konami";
            case 0x35: return "hector";
            case 0x38: return "Capcom";
            case 0x39: return "Banpresto";
            case 0x3C: return "*entertainment i";
            case 0x3E: return "gremlin";
            case 0x41: return "Ubisoft";
            case 0x42: return "Atlus";
            case 0x44: return "Malibu";
            case 0x46: return "angel";
            case 0x47: return "spectrum holoby";
            case 0x49: return "irem";
            case 0x4A: return "virgin";
            case 0x4D: return "malibu";
            case 0x4F: return "u.s. gold";
            case 0x50: return "absolute";
            case 0x51: return "acclaim";
            case 0x52: return "activision";
            case 0x53: return "american sammy";
            case 0x54: return "gametek";
            case 0x55: return "park place";
            case 0x56: return "ljn";
            case 0x57: return "matchbox";
            case 0x59: return "milton bradley";
            case 0x5A: return "mindscape";
            case 0x5B: return "romstar";
            case 0x5C: return "naxat soft";
            case 0x5D: return "tradewest";
            case 0x60: return "titus";
            case 0x61: return "virgin";
            case 0x67: return "ocean";
            case 0x69: return "electronic arts";
            case 0x6E: return "elite systems";
            case 0x6F: return "electro brain";
            case 0x70: return "Infogrammes";
            case 0x71: return "Interplay";
            case 0x72: return "broderbund";
            case 0x73: return "sculptered soft";
            case 0x75: return "the sales curve";
            case 0x78: return "t*hq";
            case 0x79: return "accolade";
            case 0x7A: return "triffix entertainment";
            case 0x7C: return "microprose";
            case 0x7F: return "kemco";
            case 0x80: return "misawa entertainment";
            case 0x83: return "lozc";
            case 0x86: return "tokuma shoten intermedia";
            case 0x8B: return "bullet-proof software";
            case 0x8C: return "vic tokai";
            case 0x8E: return "ape";
            case 0x8F: return "i'max";
            case 0x91: return "chun soft";
            case 0x92: return "video system";
            case 0x93: return "tsuburava";
            case 0x95: return "varie";
            case 0x96: return "yonezawa/s'pal";
            case 0x97: return "kaneko";
            case 0x99: return "arc";
            case 0x9A: return "nihon bussan";
            case 0x9B: return "Tecmo";
            case 0x9C: return "imagineer";
            case 0x9D: return "Banpresto";
            case 0x9F: return "nova";
            case 0xA1: return "Hori electric";
            case 0xA2: return "Bandai";
            case 0xA4: return "Konami";
            case 0xA6: return "kawada";
            case 0xA7: return "takara";
            case 0xA9: return "technos japan";
            case 0xAA: return "broderbund";
            case 0xAC: return "Toei animation";
            case 0xAD: return "toho";
            case 0xAF: return "Namco";
            case 0xB0: return "Acclaim";
            case 0xB1: return "ascii or nexoft";
            case 0xB2: return "Bandai";
            case 0xB4: return "Enix";
            case 0xB6: return "HAL";
            case 0xB7: return "SNK";
            case 0xB9: return "pony canyon";
            case 0xBA: return "*culture brain o";
            case 0xBB: return "Sunsoft";
            case 0xBD: return "Sony imagesoft";
            case 0xBF: return "sammy";
            case 0xC0: return "Taito";
            case 0xC2: return "Kemco";
            case 0xC3: return "Squaresoft";
            case 0xC4: return "tokuma shoten intermedia";
            case 0xC5: return "data east";
            case 0xC6: return "tonkin house";
            case 0xC8: return "koei";
            case 0xC9: return "ufl";
            case 0xCA: return "ultra";
            case 0xCB: return "vap";
            case 0xCC: return "use";
            case 0xCD: return "meldac";
            case 0xCE: return "*pony canyon or";
            case 0xCF: return "angel";
            case 0xD0: return "Taito";
            case 0xD1: return "sofel";
            case 0xD2: return "quest";
            case 0xD3: return "sigma enterprises";
            case 0xD4: return "ask kodansha";
            case 0xD6: return "naxat soft";
            case 0xD7: return "copya systems";
            case 0xD9: return "Banpresto";
            case 0xDA: return "tomy";
            case 0xDB: return "ljn";
            case 0xDD: return "ncs";
            case 0xDE: return "human";
            case 0xDF: return "altron";
            case 0xE0: return "jaleco";
            case 0xE1: return "towachiki";
            case 0xE2: return "uutaka";
            case 0xE3: return "varie";
            case 0xE5: return "epoch";
            case 0xE7: return "athena";
            case 0xE8: return "asmik";
            case 0xE9: return "natsume";
            case 0xEA: return "king records";
            case 0xEB: return "atlus";
            case 0xEC: return "Epic/Sony records";
            case 0xEE: return "igs";
            case 0xF0: return "a wave";
            case 0xF3: return "extreme entertainment";
            case 0xFF: return "ljn";
            default:   return "Unknown";
        }
    }

    static uint DecodeRomSize(byte headerRomType) => headerRomType switch
    {
        0    => 32768,
        1    => 65536,
        2    => 131072,
        3    => 262144,
        4    => 524288,
        5    => 1048576,
        6    => 2097152,
        7    => 4194304,
        8    => 8388608,
        0x52 => 1179648,
        0x53 => 1310720,
        0x54 => 1572864,
        _    => 0
    };

    static uint DecodeSaveRamSize(byte headerRamType) => headerRamType switch
    {
        0 => 0,
        1 => 2048,
        2 => 8192,
        3 => 32768,
        4 => 131072,
        5 => 65536,
        _ => 0
    };

    static string DecodeCartridgeType(byte headerRomType) => headerRomType switch
    {
        0x00 => "ROM only",
        0x01 => "ROM and MBC1",
        0x02 => "ROM, MBC1 and RAM",
        0x03 => "ROM, MBC1, RAM and battery",
        0x05 => "ROM and MBC2",
        0x06 => "ROM, MBC2 and battery",
        0x08 => "ROM and RAM",
        0x09 => "ROM, RAM and battery",
        0x0B => "ROM and MMM01",
        0x0C => "ROM, MMM01 and RAM",
        0x0D => "ROM, MMM01, RAM and battery",
        0x0F => "ROM, MBC3, timer and battery",
        0x10 => "ROM, MBC3, RAM, timer and battery",
        0x11 => "ROM and MBC3",
        0x12 => "ROM, MBC3 and RAM",
        0x13 => "ROM, MBC3, RAM and battery",
        0x19 => "ROM and MBC5",
        0x1A => "ROM, MBC5 and RAM",
        0x1B => "ROM, MBC5, RAM and battery",
        0x1C => "ROM, MBC5 and vibration motor",
        0x1D => "ROM, MBC5, RAM and vibration motor",
        0x1E => "ROM, MBC5, RAM, battery and vibration motor",
        0x20 => "ROM and MBC6",
        0x22 => "ROM, MBC7, RAM, battery, light sensor and vibration motor",
        0xFC => "Pocket Camera",
        0xFD => "ROM and TAMA5",
        0xFE => "ROM and HuC-3",
        0xFF => "ROM and HuC-1",
        _    => "Unknown"
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1), SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    struct Header
    {
        /// <summary>Usually 0x00 (NOP)</summary>
        public byte Opcode1;
        /// <summary>Usually 0xC3 (JP)</summary>
        public byte Opcode2;
        /// <summary>Jump destination</summary>
        public ushort Start;
        /// <summary>Boot logo, checked by boot ROM</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public byte[] Logo;
        /// <summary>Game name, ASCIIZ, if last byte has 7-bit set, that byte is Game Boy Color type</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Name;
        /// <summary>New licensee code</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] LicenseeNew;
        /// <summary>Super Game Boy Flag</summary>
        public byte Sgb;
        /// <summary>ROM type</summary>
        public byte RomType;
        /// <summary>ROM size</summary>
        public byte RomSize;
        /// <summary>SRAM size</summary>
        public byte SramSize;
        /// <summary>Country code</summary>
        public byte Country;
        /// <summary>Licensee code</summary>
        public byte Licensee;
        /// <summary>Game revision</summary>
        public byte Revision;
        /// <summary>Header checksum</summary>
        public byte HeaderChecksum;
        /// <summary>Cartridge checksum</summary>
        public ushort Checksum;
    }
}