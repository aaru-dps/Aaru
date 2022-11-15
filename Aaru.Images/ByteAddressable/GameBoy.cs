namespace Aaru.DiscImages.ByteAddressable;

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
        var magicBytes = new byte[8];
        stream.EnsureRead(magicBytes, 0, 8);
        var magic = BitConverter.ToUInt64(magicBytes, 0);

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
        var magicBytes = new byte[8];
        stream.EnsureRead(magicBytes, 0, 8);
        var magic = BitConverter.ToUInt64(magicBytes, 0);

        if(magic != 0x0B000DCC6666EDCE)
            return ErrorNumber.InvalidArgument;

        _data           = new byte[imageFilter.DataForkLength];
        stream.Position = 0;
        stream.EnsureRead(_data, 0, (int)imageFilter.DataForkLength);

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

        var name = new byte[(header.Name[^1] & 0x80) == 0x80 ? 15 : 16];
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
    public ErrorNumber GetMappings(out LinearMemoryMap mappings)
    {
        mappings = new LinearMemoryMap();

        if(!_opened)
        {
            ErrorMessage = "Not image has been opened.";

            return ErrorNumber.NotOpened;
        }

        Header header = Marshal.ByteArrayToStructureBigEndian<Header>(_data, 0x100, Marshal.SizeOf<Header>());

        var    hasMapper          = false;
        var    hasSaveRam         = false;
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

        var devices = 1;

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
            mappings.Devices[1] = new LinearMemoryDevice
            {
                Type = LinearMemoryType.SaveRAM,
                PhysicalAddress = new LinearMemoryAddressing
                {
                    Start  = mappings.Devices[0].PhysicalAddress.Length,
                    Length = DecodeSaveRamSize(header.SramSize)
                }
            };

        if(hasMapper)
            mappings.Devices[^1] = new LinearMemoryDevice
            {
                Type         = LinearMemoryType.Mapper,
                Manufacturer = mapperManufacturer,
                Model        = mapperName
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

        var foundRom     = false;
        var foundSaveRam = false;
        var foundMapper  = false;

        // Sanitize
        foreach(LinearMemoryDevice map in mappings.Devices)
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
    static string DecodeLicensee(byte headerLicensee, byte[] headerLicenseeNew)
    {
        if(headerLicensee != 0x33)
            return headerLicensee switch
                   {
                       0x00 => "none",
                       0x01 => "nintendo",
                       0x08 => "capcom",
                       0x09 => "hot-b",
                       0x0A => "jaleco",
                       0x0B => "coconuts",
                       0x0C => "elite systems",
                       0x13 => "electronic arts",
                       0x18 => "hudsonsoft",
                       0x19 => "itc entertainment",
                       0x1A => "yanoman",
                       0x1D => "clary",
                       0x1F => "virgin",
                       0x20 => "KSS",
                       0x24 => "pcm complete",
                       0x25 => "san-x",
                       0x28 => "kotobuki systems",
                       0x29 => "seta",
                       0x30 => "infogrames",
                       0x31 => "nintendo",
                       0x32 => "bandai",
                       0x33 => "'''GBC - see above'''",
                       0x34 => "konami",
                       0x35 => "hector",
                       0x38 => "Capcom",
                       0x39 => "Banpresto",
                       0x3C => "*entertainment i",
                       0x3E => "gremlin",
                       0x41 => "Ubisoft",
                       0x42 => "Atlus",
                       0x44 => "Malibu",
                       0x46 => "angel",
                       0x47 => "spectrum holoby",
                       0x49 => "irem",
                       0x4A => "virgin",
                       0x4D => "malibu",
                       0x4F => "u.s. gold",
                       0x50 => "absolute",
                       0x51 => "acclaim",
                       0x52 => "activision",
                       0x53 => "american sammy",
                       0x54 => "gametek",
                       0x55 => "park place",
                       0x56 => "ljn",
                       0x57 => "matchbox",
                       0x59 => "milton bradley",
                       0x5A => "mindscape",
                       0x5B => "romstar",
                       0x5C => "naxat soft",
                       0x5D => "tradewest",
                       0x60 => "titus",
                       0x61 => "virgin",
                       0x67 => "ocean",
                       0x69 => "electronic arts",
                       0x6E => "elite systems",
                       0x6F => "electro brain",
                       0x70 => "Infogrammes",
                       0x71 => "Interplay",
                       0x72 => "broderbund",
                       0x73 => "sculptered soft",
                       0x75 => "the sales curve",
                       0x78 => "t*hq",
                       0x79 => "accolade",
                       0x7A => "triffix entertainment",
                       0x7C => "microprose",
                       0x7F => "kemco",
                       0x80 => "misawa entertainment",
                       0x83 => "lozc",
                       0x86 => "tokuma shoten intermedia",
                       0x8B => "bullet-proof software",
                       0x8C => "vic tokai",
                       0x8E => "ape",
                       0x8F => "i'max",
                       0x91 => "chun soft",
                       0x92 => "video system",
                       0x93 => "tsuburava",
                       0x95 => "varie",
                       0x96 => "yonezawa/s'pal",
                       0x97 => "kaneko",
                       0x99 => "arc",
                       0x9A => "nihon bussan",
                       0x9B => "Tecmo",
                       0x9C => "imagineer",
                       0x9D => "Banpresto",
                       0x9F => "nova",
                       0xA1 => "Hori electric",
                       0xA2 => "Bandai",
                       0xA4 => "Konami",
                       0xA6 => "kawada",
                       0xA7 => "takara",
                       0xA9 => "technos japan",
                       0xAA => "broderbund",
                       0xAC => "Toei animation",
                       0xAD => "toho",
                       0xAF => "Namco",
                       0xB0 => "Acclaim",
                       0xB1 => "ascii or nexoft",
                       0xB2 => "Bandai",
                       0xB4 => "Enix",
                       0xB6 => "HAL",
                       0xB7 => "SNK",
                       0xB9 => "pony canyon",
                       0xBA => "*culture brain o",
                       0xBB => "Sunsoft",
                       0xBD => "Sony imagesoft",
                       0xBF => "sammy",
                       0xC0 => "Taito",
                       0xC2 => "Kemco",
                       0xC3 => "Squaresoft",
                       0xC4 => "tokuma shoten intermedia",
                       0xC5 => "data east",
                       0xC6 => "tonkin house",
                       0xC8 => "koei",
                       0xC9 => "ufl",
                       0xCA => "ultra",
                       0xCB => "vap",
                       0xCC => "use",
                       0xCD => "meldac",
                       0xCE => "*pony canyon or",
                       0xCF => "angel",
                       0xD0 => "Taito",
                       0xD1 => "sofel",
                       0xD2 => "quest",
                       0xD3 => "sigma enterprises",
                       0xD4 => "ask kodansha",
                       0xD6 => "naxat soft",
                       0xD7 => "copya systems",
                       0xD9 => "Banpresto",
                       0xDA => "tomy",
                       0xDB => "ljn",
                       0xDD => "ncs",
                       0xDE => "human",
                       0xDF => "altron",
                       0xE0 => "jaleco",
                       0xE1 => "towachiki",
                       0xE2 => "uutaka",
                       0xE3 => "varie",
                       0xE5 => "epoch",
                       0xE7 => "athena",
                       0xE8 => "asmik",
                       0xE9 => "natsume",
                       0xEA => "king records",
                       0xEB => "atlus",
                       0xEC => "Epic/Sony records",
                       0xEE => "igs",
                       0xF0 => "a wave",
                       0xF3 => "extreme entertainment",
                       0xFF => "ljn",
                       _    => "Unknown"
                   };

        string licenseeNew = StringHandlers.CToString(headerLicenseeNew);

        return licenseeNew switch
               {
                   "00" => "none",
                   "01" => "Nintendo R&D1",
                   "08" => "Capcom",
                   "13" => "Electronic Arts",
                   "18" => "Hudson Soft",
                   "19" => "b-ai",
                   "20" => "kss",
                   "22" => "pow",
                   "24" => "PCM Complete",
                   "25" => "san-x",
                   "28" => "Kemco Japan",
                   "29" => "seta",
                   "30" => "Viacom",
                   "31" => "Nintendo",
                   "32" => "Bandai",
                   "33" => "Ocean / Acclaim",
                   "34" => "Konami",
                   "35" => "Hector",
                   "37" => "Taito",
                   "38" => "Hudson",
                   "39" => "Banpresto",
                   "41" => "Ubi Soft",
                   "42" => "Atlus",
                   "44" => "Malibu",
                   "46" => "angel",
                   "47" => "Bullet -Proof",
                   "49" => "irem",
                   "50" => "Absolute",
                   "51" => "Acclaim",
                   "52" => "Activision",
                   "53" => "American sammy",
                   "54" => "Konami",
                   "55" => "Hi tech entertainment",
                   "56" => "LJN",
                   "57" => "Matchbox",
                   "58" => "Mattel",
                   "59" => "Milton Bradley",
                   "60" => "Titus",
                   "61" => "Virgin",
                   "64" => "LucasArts",
                   "67" => "Ocean",
                   "69" => "Electronic Arts",
                   "70" => "Infogrames",
                   "71" => "Interplay",
                   "72" => "Brøderbund",
                   "73" => "sculptured",
                   "75" => "sci",
                   "78" => "THQ",
                   "79" => "Accolade",
                   "80" => "misawa",
                   "83" => "lozc",
                   "86" => "tokuma shoten i",
                   "87" => "tsukuda ori",
                   "91" => "Chunsoft",
                   "92" => "Video  system",
                   "93" => "Ocean / Acclaim",
                   "95" => "Varie",
                   "96" => "Yonezawa / s'pal",
                   "97" => "Kaneko",
                   "99" => "Pack in soft",
                   "A4" => "Konami",
                   _    => "Unknown"
               };

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
                                                                 0x22 =>
                                                                     "ROM, MBC7, RAM, battery, light sensor and vibration motor",
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