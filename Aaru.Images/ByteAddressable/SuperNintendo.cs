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
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class SuperNintendo : IByteAddressableImage
{
    byte[]    _data;
    Stream    _dataStream;
    Header    _header;
    ImageInfo _imageInfo;
    bool      _opened;

#region IByteAddressableImage Members

    /// <inheritdoc />
    // ReSharper disable once ConvertToAutoProperty
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Name => Localization.SuperNintendo_Name;

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        if(imageFilter == null)
            return false;

        Stream stream = imageFilter.GetDataForkStream();

        // Not sure but seems to be a multiple of at least this
        if(stream.Length % 32768 != 0)
            return false;

        // Too many false positives at bigger sizes
        if(stream.Length > 16 * 1048576)
            return false;

        // Check exact sizes, too many positives otherwise
        if(stream.Length != 262144  &&
           stream.Length != 524288  &&
           stream.Length != 1048576 &&
           stream.Length != 1572864 &&
           stream.Length != 2097152 &&
           stream.Length != 2621440 &&
           stream.Length != 3145728 &&
           stream.Length != 4194304 &&
           stream.Length != 6291456 &&
           stream.Length != 8388608)
            return false;

        Header header;
        var    headerBytes = new byte[48];

        switch(stream.Length)
        {
            case > 0x40FFFF:
            {
                stream.Position = 0x40FFB0;

                stream.EnsureRead(headerBytes, 0, 48);
                header = Marshal.ByteArrayToStructureLittleEndian<Header>(headerBytes);

                if((header.Mode & 0xF) == 0x5 || (header.Mode & 0xF) == 0xA)
                    return true;

                break;
            }
            case > 0xFFFF:
            {
                stream.Position = 0xFFB0;

                stream.EnsureRead(headerBytes, 0, 48);
                header = Marshal.ByteArrayToStructureLittleEndian<Header>(headerBytes);

                if((header.Mode & 0xF) == 0x1 || (header.Mode & 0xF) == 0xA)
                    return true;

                break;
            }
            case > 0x7FFF:
                stream.Position = 0x7FB0;

                stream.EnsureRead(headerBytes, 0, 48);
                header = Marshal.ByteArrayToStructureLittleEndian<Header>(headerBytes);

                return (header.Mode & 0xF) == 0x0 || (header.Mode & 0xF) == 0x2 || (header.Mode & 0xF) == 0x3;
        }

        return false;
    }

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        if(imageFilter == null)
            return ErrorNumber.NoSuchFile;

        Stream stream = imageFilter.GetDataForkStream();

        // Not sure but seems to be a multiple of at least this
        if(stream.Length % 32768 != 0)
            return ErrorNumber.InvalidArgument;

        var found       = false;
        var headerBytes = new byte[48];

        switch(stream.Length)
        {
            case > 0x40FFFF:
            {
                stream.Position = 0x40FFB0;

                stream.EnsureRead(headerBytes, 0, 48);
                _header = Marshal.ByteArrayToStructureLittleEndian<Header>(headerBytes);

                if((_header.Mode & 0xF) == 0x5 || (_header.Mode & 0xF) == 0xA)
                    found = true;

                break;
            }
            case > 0xFFFF:
            {
                stream.Position = 0xFFB0;

                stream.EnsureRead(headerBytes, 0, 48);
                _header = Marshal.ByteArrayToStructureLittleEndian<Header>(headerBytes);

                if((_header.Mode & 0xF) == 0x1 || (_header.Mode & 0xF) == 0xA)
                    found = true;

                break;
            }
            case > 0x7FFF:
            {
                stream.Position = 0x7FB0;

                stream.EnsureRead(headerBytes, 0, 48);
                _header = Marshal.ByteArrayToStructureLittleEndian<Header>(headerBytes);

                if((_header.Mode & 0xF) == 0x0 || (_header.Mode & 0xF) == 0x2 || (_header.Mode & 0xF) == 0x3)
                    found = true;

                break;
            }
        }

        if(!found)
            return ErrorNumber.InvalidArgument;

        _data           = new byte[imageFilter.DataForkLength];
        stream.Position = 0;
        stream.EnsureRead(_data, 0, (int)imageFilter.DataForkLength);

        Encoding encoding;

        try
        {
            encoding = Encoding.GetEncoding("shift_jis");
        }
        catch(Exception)
        {
            encoding = Encoding.ASCII;
        }

        _imageInfo = new ImageInfo
        {
            Application          = "Multi Game Doctor 2",
            CreationTime         = imageFilter.CreationTime,
            ImageSize            = (ulong)imageFilter.DataForkLength,
            MediaType            = _header.Region == 1 ? MediaType.SNESGamePakUS : MediaType.SNESGamePak,
            LastModificationTime = imageFilter.LastWriteTime,
            Sectors              = (ulong)imageFilter.DataForkLength,
            MetadataMediaType    = MetadataMediaType.LinearMedia,
            MediaTitle           = StringHandlers.SpacePaddedToString(_header.Title, encoding),
            MediaManufacturer    = DecodeManufacturer(_header.OldMakerCode, _header.MakerCode)
        };

        var sb = new StringBuilder();

        sb.AppendFormat(Localization.Name_0,         _imageInfo.MediaTitle).AppendLine();
        sb.AppendFormat(Localization.Manufacturer_0, _imageInfo.MediaManufacturer).AppendLine();
        sb.AppendFormat(Localization.Region_0,       DecodeRegion(_header.Region)).AppendLine();

        if(_header.OldMakerCode == 0x33)
            sb.AppendFormat(Localization.Game_code_0, _header.GameCode).AppendLine();

        sb.AppendFormat(Localization.Revision_0, _header.Revision).AppendLine();

        if(_header.OldMakerCode == 0x33)
            sb.AppendFormat(Localization.Special_revision_0, _header.SpecialVersion).AppendLine();

        sb.AppendFormat(Localization.Header_checksum_0_X4,         _header.Checksum).AppendLine();
        sb.AppendFormat(Localization.Header_checksum_complement_0, _header.ChecksumComplement).AppendLine();

        sb.AppendFormat(Localization.ROM_size_0_bytes, (1 << _header.RomSize) * 1024).AppendLine();

        if(_header.RamSize > 0)
            sb.AppendFormat(Localization.RAM_size_0_bytes, (1 << _header.RamSize) * 1024).AppendLine();

        if(_header.OldMakerCode == 0x33)
        {
            if(_header.ExpansionFlashSize > 0)
                sb.AppendFormat(Localization.Flash_size_0_bytes, (1 << _header.ExpansionFlashSize) * 1024).AppendLine();

            if(_header.ExpansionRamSize > 0)
            {
                sb.AppendFormat(Localization.Expansion_RAM_size_0_bytes, (1 << _header.ExpansionRamSize) * 1024).
                   AppendLine();
            }
        }

        sb.AppendFormat(Localization.Cartridge_type_0, DecodeCartType(_header.Mode)).AppendLine();
        sb.AppendFormat(Localization.ROM_speed_0, DecodeRomSpeed(_header.Mode)).AppendLine();
        sb.AppendFormat(Localization.Bank_size_0_bytes, DecodeBankSize(_header.Mode)).AppendLine();
        sb.AppendFormat(Localization.Cartridge_chip_set_0, DecodeChipset(_header.Chipset)).AppendLine();
        sb.AppendFormat(Localization.Coprocessor_0, DecodeCoprocessor(_header.Chipset, _header.Subtype)).AppendLine();

        _imageInfo.Comments = sb.ToString();
        _opened             = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public Guid Id => new("DF861EB0-8B9B-4E3F-BF39-9F2E75668F80");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public string Format => "Super Nintendo Cartridge Dump";

    /// <inheritdoc />
    public List<DumpHardware> DumpHardware => null;

    /// <inheritdoc />
    public Metadata AaruMetadata => null;

    /// <inheritdoc />
    public string ErrorMessage { get; private set; }

    /// <inheritdoc />
    public bool IsWriting { get; private set; }

    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[]
    {
        ".sfc"
    };

    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Array.Empty<MediaTagType>();

    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        MediaType.SNESGamePak, MediaType.SNESGamePakUS
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

        if(mediaType != MediaType.SNESGamePak && mediaType != MediaType.SNESGamePakUS)
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
        catch(IOException ex)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, ex.Message);
            AaruConsole.WriteException(ex);

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

        int chipset = _header.Chipset & 0xF;

        bool hasRam      = _header.RamSize > 0;
        bool hasExtraRam = _header is { OldMakerCode: 0x33, ExpansionRamSize  : > 0 };
        bool hasFlash    = _header is { OldMakerCode: 0x33, ExpansionFlashSize: > 0 };
        bool hasBattery  = chipset is 2 or 5 or 6 or 9 or 0xA;

        var devices = 1;

        if(hasRam)
            devices++;

        if(hasExtraRam)
            devices++;

        if(hasFlash)
            devices++;

        mappings = new LinearMemoryMap
        {
            Devices = new LinearMemoryDevice[devices]
        };

        mappings.Devices[0] = new LinearMemoryDevice
        {
            Type = LinearMemoryType.ROM,
            PhysicalAddress = new LinearMemoryAddressing
            {
                Start  = 0,
                Length = (ulong)_data.Length
            }
        };

        var pos  = 1;
        var addr = (ulong)_data.Length;

        if(hasRam)
        {
            mappings.Devices[pos] = new LinearMemoryDevice
            {
                Type = hasBattery ? LinearMemoryType.SaveRAM : LinearMemoryType.WorkRAM,
                PhysicalAddress = new LinearMemoryAddressing
                {
                    Start  = addr,
                    Length = (ulong)(1 << _header.RamSize) * 1024
                }
            };

            addr += (ulong)(1 << _header.RamSize) * 1024;
            pos++;
        }

        if(hasExtraRam)
        {
            mappings.Devices[pos] = new LinearMemoryDevice
            {
                Type = hasBattery && !hasRam ? LinearMemoryType.SaveRAM : LinearMemoryType.WorkRAM,
                PhysicalAddress = new LinearMemoryAddressing
                {
                    Start  = addr,
                    Length = (ulong)(1 << _header.ExpansionRamSize) * 1024
                }
            };

            addr += (ulong)(1 << _header.ExpansionRamSize) * 1024;
            pos++;
        }

        if(hasFlash)
        {
            mappings.Devices[pos] = new LinearMemoryDevice
            {
                Type = LinearMemoryType.NOR,
                PhysicalAddress = new LinearMemoryAddressing
                {
                    Start  = addr,
                    Length = (ulong)(1 << _header.ExpansionRamSize) * 1024
                }
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

        var foundRom      = false;
        var foundRam      = false;
        var foundExtraRam = false;
        var foundFlash    = false;

        // Sanitize
        foreach(LinearMemoryDevice map in mappings.Devices)
        {
            switch(map.Type)
            {
                case LinearMemoryType.ROM when !foundRom:
                    foundRom = true;

                    break;
                case LinearMemoryType.SaveRAM when !foundRam:
                    foundRam = true;

                    break;
                case LinearMemoryType.SaveRAM when !foundExtraRam:
                    foundExtraRam = true;

                    break;
                case LinearMemoryType.WorkRAM when !foundRam:
                    foundRam = true;

                    break;
                case LinearMemoryType.WorkRAM when !foundExtraRam:
                    foundExtraRam = true;

                    break;
                case LinearMemoryType.NOR when !foundFlash:
                case LinearMemoryType.NAND when !foundFlash:
                    foundFlash = true;

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

    static string DecodeCoprocessor(byte chipset, byte subtype)
    {
        if((chipset & 0xF) < 3)
            return Localization.None_coprocessor;

        return ((chipset & 0xF0) >> 4) switch
               {
                   0   => "DSP",
                   1   => "GSU",
                   2   => "OBC1",
                   3   => "SA-1",
                   4   => "S-DD1",
                   5   => "S-RTC",
                   0xE => Localization.Other_coprocessor,
                   0xF => subtype switch
                          {
                              0    => "SPC7110",
                              1    => "ST010/ST011",
                              2    => "ST018",
                              0x10 => "CX4",
                              _    => Localization.Unknown_coprocessor
                          },
                   _ => Localization.Unknown_coprocessor
               };
    }

    static string DecodeChipset(byte chipset)
    {
        switch(chipset & 0xF)
        {
            case 0:
                return Localization.ROM;
            case 1:
                return Localization.ROM_and_RAM;
            case 2 when (chipset & 0xF0) == 0:
                return Localization.ROM_RAM_and_battery;
            case 3:
                return Localization.ROM_and_coprocessor;
            case 4:
                return Localization.ROM_RAM_and_coprocessor;
            case 2:
            case 5:
                return Localization.ROM_RAM_battery_and_coprocessor;
            case 6:
                return Localization.ROM_battery_and_coprocessor;
            case 9:
                return Localization.ROM_RAM_battery_coprocessor_and_RTC;
            case 0xA:
                return Localization.ROM_RAM_battery_and_coprocessor;
            default:
                return Localization.Unknown_chipset;
        }
    }

    static int DecodeBankSize(byte mode)
    {
        switch(mode & 0xF)
        {
            case 0:
            case 2:
            case 3:
                return 32768;
            case 1:
            case 5:
            case 0xA:
                return 65536;
            default:
                return 0;
        }
    }

    static string DecodeRomSpeed(byte mode) => (mode & 0x10) == 0x10 ? "Fast (120ns)" : "Slow (200ns)";

    static string DecodeCartType(byte mode)
    {
        switch(mode & 0xF)
        {
            case 0:
            case 2:
            case 3:
                return "LoROM";
            case 1:
            case 0xA:
                return "HiROM";
            case 5:
                return "ExHiROM";
            default:
                return Localization.Unknown_licensee;
        }
    }

    static string DecodeRegion(byte headerRegion) => headerRegion switch
                                                     {
                                                         0  => Localization.Japan,
                                                         1  => Localization.USA_and_Canada,
                                                         2  => Localization.Europe_Oceania_Asia,
                                                         3  => Localization.Sweden_Scandinavia,
                                                         4  => Localization.Finland,
                                                         5  => Localization.Denmark,
                                                         6  => Localization.France,
                                                         7  => Localization.Netherlands,
                                                         8  => Localization.Spain,
                                                         9  => Localization.Germany_Austria_Switzerland,
                                                         10 => Localization.Italy,
                                                         11 => Localization.China_Hong_Kong,
                                                         12 => Localization.Indonesia,
                                                         13 => Localization.South_Korea,
                                                         15 => Localization.Canada,
                                                         16 => Localization.Brazil,
                                                         17 => Localization.Australia,
                                                         _  => Localization.Unknown_licensee
                                                     };

    static string DecodeManufacturer(byte oldMakerCode, string makerCode)
    {
        // TODO: Add full table
        if(oldMakerCode != 0x33)
            makerCode = $"{(oldMakerCode >> 4) * 36 + (oldMakerCode & 0x0f)}";

        return makerCode switch
               {
                   "01" => "Nintendo",
                   _    => Localization.Unknown_manufacturer
               };
    }

#region Nested type: Header

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    struct Header
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2)]
        public string MakerCode;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string GameCode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] Reserved;
        public byte ExpansionFlashSize;
        public byte ExpansionRamSize;
        public byte SpecialVersion;
        public byte Subtype;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
        public byte[] Title;
        public byte   Mode;
        public byte   Chipset;
        public byte   RomSize;
        public byte   RamSize;
        public byte   Region;
        public byte   OldMakerCode;
        public byte   Revision;
        public ushort ChecksumComplement;
        public ushort Checksum;
    }

#endregion
}