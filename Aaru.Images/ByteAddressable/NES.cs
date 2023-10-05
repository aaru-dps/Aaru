using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Database;
using Aaru.Database.Models;
using Aaru.Helpers;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Nes : IByteAddressableImage
{
    int           _chrLen;
    int           _chrNvramLen;
    int           _chrRamLen;
    byte[]        _data;
    Stream        _dataStream;
    ImageInfo     _imageInfo;
    int           _instRomLen;
    ushort        _mapper;
    bool          _nes20;
    NesHeaderInfo _nesHeaderInfo;
    bool          _opened;
    int           _prgLen;
    int           _prgNvramLen;
    int           _prgRamLen;
    int           _promLen;
    byte          _submapper;
    bool          _trainer;

#region IByteAddressableImage Members

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public Metadata AaruMetadata => null;

    /// <inheritdoc />
    public List<DumpHardware> DumpHardware => null;

    /// <inheritdoc />
    public string Format => _nes20 ? "NES 2.0" : "iNES";

    /// <inheritdoc />
    public Guid Id => new("D597A3F4-2B1C-441C-8487-0BCABC509302");

    /// <inheritdoc />
    // ReSharper disable once ConvertToAutoProperty
    public ImageInfo Info => _imageInfo;

    /// <inheritdoc />
    public string Name => Localization.Nes_Name;

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        if(imageFilter == null)
            return false;

        Stream stream = imageFilter.GetDataForkStream();

        // Not sure but seems to be a multiple of at least this
        if(stream.Length < 16)
            return false;

        stream.Position = 0;
        var magicBytes = new byte[4];
        stream.EnsureRead(magicBytes, 0, 8);
        var magic = BitConverter.ToUInt32(magicBytes, 0);

        return magic == 0x1A53454E;
    }

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        if(imageFilter == null)
            return ErrorNumber.NoSuchFile;

        Stream stream = imageFilter.GetDataForkStream();

        // Not sure but seems to be a multiple of at least this, maybe more
        if(stream.Length < 16)
            return ErrorNumber.InvalidArgument;

        stream.Position = 0;
        var header = new byte[16];
        stream.EnsureRead(header, 0, 8);
        var magic = BitConverter.ToUInt32(header, 0);

        if(magic != 0x1A53454E)
            return ErrorNumber.InvalidArgument;

        if((header[7] & 0x0C) == 0x08)
            _nes20 = true;

        _prgLen     = header[4] * 16384;
        _chrLen     = header[5] * 8192;
        _trainer    = (header[6] & 0x4) != 0;
        _instRomLen = 0;
        _promLen    = 0;
        int trainerLen = _trainer ? 512 : 0;

        _nesHeaderInfo = new NesHeaderInfo
        {
            NametableMirroring = (header[6] & 0x1) != 0,
            BatteryPresent     = (header[6] & 0x2) != 0,
            FourScreenMode     = (header[6] & 0x8) != 0,
            Mapper             = (ushort)(header[6] >> 4 | header[7] & 0xF0)
        };

        if((header[7] & 0x1) != 0)
            _nesHeaderInfo.ConsoleType = NesConsoleType.Vs;
        else if((header[7] & 0x2) != 0)
            _nesHeaderInfo.ConsoleType = NesConsoleType.Playchoice;
        else
            _nesHeaderInfo.ConsoleType = NesConsoleType.Nes;

        if(_nes20)
        {
            _nesHeaderInfo.ConsoleType =  (NesConsoleType)(header[7] & 0x3);
            _nesHeaderInfo.Mapper      += (ushort)((header[8] & 0xF) << 8);
            _nesHeaderInfo.Submapper   =  (byte)(header[8]           >> 4);

            if((header[9] & 0xF) == 0xF)
                _prgLen = (1 << (header[4] >> 2)) * (header[4] & 0x3);
            else
                _prgLen += (header[9] & 0xF) * 16384;

            if(header[9] >> 4 == 0xF)
                _chrLen = (1 << (header[5] >> 2)) * (header[5] & 0x3);
            else
                _chrLen += (header[9] >> 4) * 8192;

            if((header[10] & 0xF) > 0)
                _prgRamLen = 64 << (header[10] & 0xF);

            if((header[10] & 0xF0) > 0)
                _prgNvramLen = 64 << ((header[10] & 0xF0) >> 4);

            if((header[11] & 0xF) > 0)
                _chrRamLen = 64 << (header[11] & 0xF);

            if((header[11] & 0xF0) > 0)
                _chrNvramLen = 64 << ((header[11] & 0xF0) >> 4);

            _nesHeaderInfo.TimingMode = (NesTimingMode)(header[12] & 0x3);

            switch(_nesHeaderInfo.ConsoleType)
            {
                case NesConsoleType.Vs:
                    _nesHeaderInfo.VsPpuType      = (NesVsPpuType)(header[13] & 0xF);
                    _nesHeaderInfo.VsHardwareType = (NesVsHardwareType)(header[13] >> 4);

                    break;
                case NesConsoleType.Extended:
                    _nesHeaderInfo.ExtendedConsoleType = (NesExtendedConsoleType)(header[13] & 0xF);

                    break;
            }

            _nesHeaderInfo.DefaultExpansionDevice = (NesDefaultExpansionDevice)(header[15] & 0x3F);
        }

        _data           = new byte[imageFilter.DataForkLength - 16];
        stream.Position = 16;
        stream.EnsureRead(_data, 0, _data.Length);

        _imageInfo = new ImageInfo
        {
            Application          = "iNES",
            CreationTime         = imageFilter.CreationTime,
            ImageSize            = (ulong)imageFilter.DataForkLength,
            LastModificationTime = imageFilter.LastWriteTime,
            Sectors              = (ulong)imageFilter.DataForkLength,
            MetadataMediaType    = MetadataMediaType.LinearMedia,
            MediaType            = MediaType.FamicomGamePak
        };

        StringBuilder sb = new();

        sb.AppendFormat(Localization.PRG_ROM_size_0_bytes, _prgLen).AppendLine();
        sb.AppendFormat(Localization.CHR_ROM_size_0_bytes, _chrLen).AppendLine();
        sb.AppendFormat(Localization.Trainer_size_0_bytes, trainerLen).AppendLine();
        sb.AppendFormat(Localization.Mapper_0,             _nesHeaderInfo.Mapper).AppendLine();

        if(_nesHeaderInfo.BatteryPresent)
            sb.AppendLine(Localization.Has_battery_backed_RAM);

        if(_nesHeaderInfo.FourScreenMode)
            sb.AppendLine(Localization.Uses_four_screen_VRAM);
        else if(_nesHeaderInfo.NametableMirroring)
            sb.AppendLine(Localization.Uses_vertical_mirroring);
        else
            sb.AppendLine(Localization.Uses_horizontal_mirroring);

        switch(_nesHeaderInfo.ConsoleType)
        {
            // TODO: Proper media types
            case NesConsoleType.Vs:
                sb.AppendLine(Localization.VS_Unisystem_game);

                break;
            case NesConsoleType.Playchoice:
                sb.AppendLine(Localization.PlayChoice_10_game);
                sb.AppendFormat(Localization.INST_ROM_size_0_bytes, _instRomLen & 0xF).AppendLine();
                sb.AppendFormat(Localization.PROM_size_0_bytes,     _promLen).AppendLine();

                break;

            case NesConsoleType.Nes:
                break;
            case NesConsoleType.Extended:
                switch(_nesHeaderInfo.ExtendedConsoleType)
                {
                    case NesExtendedConsoleType.Vs:
                        sb.AppendLine(Localization.VS_Unisystem_game);

                        break;

                    case NesExtendedConsoleType.Normal:
                        break;
                    case NesExtendedConsoleType.Playchoice:
                        sb.AppendLine(Localization.PlayChoice_10_game);
                        sb.AppendFormat(Localization.INST_ROM_size_0_bytes, _instRomLen & 0xF).AppendLine();
                        sb.AppendFormat(Localization.PROM_size_0_bytes,     _promLen).AppendLine();

                        break;
                    case NesExtendedConsoleType.VT01_Monochrome:
                    case NesExtendedConsoleType.VT01:
                        sb.AppendLine(Localization.VR_Technology_VT01);

                        break;
                    case NesExtendedConsoleType.VT02:
                        sb.AppendLine(Localization.VR_Technology_VT02);

                        break;
                    case NesExtendedConsoleType.VT03:
                        sb.AppendLine(Localization.VR_Technology_VT03);

                        break;
                    case NesExtendedConsoleType.VT09:
                        sb.AppendLine(Localization.VR_Technology_VT09);

                        break;
                    case NesExtendedConsoleType.VT32:
                        sb.AppendLine(Localization.VR_Technology_VT32);

                        break;
                    case NesExtendedConsoleType.VT369:
                        sb.AppendLine(Localization.VR_Technology_VT369);

                        break;
                }

                break;
        }

        _imageInfo.Comments = sb.ToString();

        _opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public string ErrorMessage { get; private set; }

    /// <inheritdoc />
    public bool IsWriting { get; private set; }

    /// <inheritdoc />
    public IEnumerable<string> KnownExtensions => new[]
    {
        ".nes"
    };

    /// <inheritdoc />
    public IEnumerable<MediaTagType> SupportedMediaTags => Array.Empty<MediaTagType>();

    /// <inheritdoc />
    public IEnumerable<MediaType> SupportedMediaTypes => new[]
    {
        MediaType.NESGamePak, MediaType.FamicomGamePak
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

        var header = new byte[16];

        if(_nesHeaderInfo is null)
        {
            string    hash = Sha256Context.Data(_data, out _).ToLowerInvariant();
            using var ctx  = AaruContext.Create(Settings.Settings.MainDbPath);
            _nesHeaderInfo = ctx.NesHeaders.FirstOrDefault(hdr => hdr.Sha256 == hash);
        }

        _nesHeaderInfo ??= new NesHeaderInfo
        {
            Mapper      = _mapper,
            ConsoleType = _instRomLen > 0 ? NesConsoleType.Playchoice : NesConsoleType.Nes,
            Submapper   = _submapper
        };

        header[0] = 0x4E;
        header[1] = 0x45;
        header[2] = 0x53;
        header[3] = 0x1A;
        header[4] = (byte)(_prgLen / 16384 & 0xFF);
        header[5] = (byte)(_chrLen / 8192  & 0xFF);
        header[6] = (byte)((_nesHeaderInfo.Mapper & 0xF) << 4);

        if(_nesHeaderInfo.FourScreenMode)
            header[6] |= 0x8;

        if(_trainer)
            header[6] |= 0x4;

        if(_nesHeaderInfo.BatteryPresent)
            header[6] |= 0x2;

        if(_nesHeaderInfo.NametableMirroring)
            header[6] |= 0x1;

        header[7] =  (byte)(_mapper & 0xF0 | 0x8);
        header[7] |= (byte)_nesHeaderInfo.ConsoleType;

        header[8] =  (byte)(_nesHeaderInfo.Submapper << 4 | (_nesHeaderInfo.Mapper & 0xF00) >> 4);
        header[9] =  (byte)(_prgLen / 16384 >> 8);
        header[9] |= (byte)(_chrLen / 8192 >> 4 & 0xF);

        // TODO: PRG-RAM, PRG-NVRAM, CHR-RAM and CHR-NVRAM sizes

        header[12] = (byte)_nesHeaderInfo.TimingMode;

        header[13] = _nesHeaderInfo.ConsoleType switch
                     {
                         NesConsoleType.Vs => (byte)((int)_nesHeaderInfo.VsHardwareType << 4 |
                                                     (int)_nesHeaderInfo.VsPpuType),
                         NesConsoleType.Extended => (byte)_nesHeaderInfo.ExtendedConsoleType,
                         _                       => header[13]
                     };

        header[14] = 0;

        if(_instRomLen > 0)
            header[14]++;

        if(_promLen > 0)
            header[14]++;

        if(_nesHeaderInfo.ExtendedConsoleType == NesExtendedConsoleType.VT369)
            header[14]++;

        switch(_nesHeaderInfo.Mapper)
        {
            case 86 when _nesHeaderInfo.Submapper == 1:
            case 355:
                header[14]++;

                break;
        }

        header[15] = (byte)_nesHeaderInfo.DefaultExpansionDevice;

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

        if(mediaType != MediaType.FamicomGamePak && mediaType != MediaType.NESGamePak)
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
        _nes20               = true;

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

        List<LinearMemoryDevice> devices = new()
        {
            new LinearMemoryDevice
            {
                Type = LinearMemoryType.ROM,
                PhysicalAddress = new LinearMemoryAddressing
                {
                    Length = (ulong)_prgLen
                }
            }
        };

        if(_chrLen > 0)
        {
            devices.Add(new LinearMemoryDevice
            {
                Type = LinearMemoryType.CharacterROM,
                PhysicalAddress = new LinearMemoryAddressing
                {
                    Length = (ulong)_chrLen
                }
            });
        }

        if(_trainer)
        {
            devices.Add(new LinearMemoryDevice
            {
                Type = LinearMemoryType.Trainer
            });
        }

        if(_instRomLen > 0)
        {
            devices.Add(new LinearMemoryDevice
            {
                Type = LinearMemoryType.ROM,
                PhysicalAddress = new LinearMemoryAddressing
                {
                    Length = (ulong)_instRomLen
                }
            });
        }

        if(_promLen > 0)
        {
            devices.Add(new LinearMemoryDevice
            {
                Type = LinearMemoryType.ROM,
                PhysicalAddress = new LinearMemoryAddressing
                {
                    Length = (ulong)_promLen
                }
            });
        }

        if(_prgRamLen > 0)
        {
            devices.Add(new LinearMemoryDevice
            {
                Type = LinearMemoryType.WorkRAM,
                PhysicalAddress = new LinearMemoryAddressing
                {
                    Length = (ulong)_prgRamLen
                }
            });
        }

        if(_chrRamLen > 0)
        {
            devices.Add(new LinearMemoryDevice
            {
                Type = LinearMemoryType.CharacterRAM,
                PhysicalAddress = new LinearMemoryAddressing
                {
                    Length = (ulong)_chrRamLen
                }
            });
        }

        if(_prgNvramLen > 0)
        {
            devices.Add(new LinearMemoryDevice
            {
                Type = LinearMemoryType.SaveRAM,
                PhysicalAddress = new LinearMemoryAddressing
                {
                    Length = (ulong)_prgNvramLen
                }
            });
        }

        if(_chrNvramLen > 0)
        {
            devices.Add(new LinearMemoryDevice
            {
                Type = LinearMemoryType.CharacterRAM,
                PhysicalAddress = new LinearMemoryAddressing
                {
                    Length = (ulong)_chrNvramLen
                }
            });
        }

        ushort mapper    = _nesHeaderInfo?.Mapper    ?? _mapper;
        byte   submapper = _nesHeaderInfo?.Submapper ?? _submapper;

        devices.Add(new LinearMemoryDevice
        {
            Type        = LinearMemoryType.Mapper,
            Description = $"NES Mapper {mapper}"
        });

        if(submapper != 0)
        {
            devices.Add(new LinearMemoryDevice
            {
                Type        = LinearMemoryType.Mapper,
                Description = $"NES Submapper {submapper}"
            });
        }

        mappings = new LinearMemoryMap
        {
            Devices = devices.ToArray()
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

        var foundRom       = false;
        var foundChrRom    = false;
        var foundInstRom   = false;
        var foundProm      = false;
        var foundRam       = false;
        var foundChrRam    = false;
        var foundNvram     = false;
        var foundChrNvram  = false;
        var foundMapper    = false;
        var foundSubMapper = false;

        Regex regex;
        Match match;

        // Sanitize
        foreach(LinearMemoryDevice map in mappings.Devices)
        {
            switch(map.Type)
            {
                case LinearMemoryType.ROM when !foundRom:
                    _prgLen  = (int)map.PhysicalAddress.Length;
                    foundRom = true;

                    break;
                case LinearMemoryType.CharacterROM when !foundChrRom:
                    _chrLen     = (int)map.PhysicalAddress.Length;
                    foundChrRom = true;

                    break;
                case LinearMemoryType.Trainer when !_trainer:
                    _trainer = true;

                    break;
                case LinearMemoryType.ROM when !foundInstRom:
                    _instRomLen  = (int)map.PhysicalAddress.Length;
                    foundInstRom = true;

                    break;
                case LinearMemoryType.ROM when !foundProm:
                    _promLen  = (int)map.PhysicalAddress.Length;
                    foundProm = true;

                    break;
                case LinearMemoryType.WorkRAM when !foundRam:
                    _prgRamLen = (int)map.PhysicalAddress.Length;
                    foundRam   = true;

                    break;
                case LinearMemoryType.CharacterRAM when !foundChrRam:
                    _chrRamLen  = (int)map.PhysicalAddress.Length;
                    foundChrRam = true;

                    break;
                case LinearMemoryType.SaveRAM when !foundNvram:
                    _prgNvramLen = (int)map.PhysicalAddress.Length;
                    foundNvram   = true;

                    break;
                case LinearMemoryType.CharacterRAM when !foundChrNvram:
                    _chrNvramLen  = (int)map.PhysicalAddress.Length;
                    foundChrNvram = true;

                    break;
                case LinearMemoryType.Mapper when !foundMapper:
                    regex = new Regex(@"NES Mapper ?(<mapper>\d+)");
                    match = regex.Match(map.Description);

                    if(match.Success)
                    {
                        if(ushort.TryParse(match.Groups["mapper"].Value, out ushort mapper))
                        {
                            if(_nesHeaderInfo is null)
                                _mapper = mapper;
                            else
                                _nesHeaderInfo.Mapper = mapper;

                            foundMapper = true;
                        }
                    }

                    break;
                case LinearMemoryType.Mapper when !foundSubMapper:
                    regex = new Regex(@"NES Sub-Mapper ?(<mapper>\d+)");
                    match = regex.Match(map.Description);

                    if(match.Success)
                    {
                        if(byte.TryParse(match.Groups["mapper"].Value, out byte mapper))
                        {
                            if(_nesHeaderInfo is null)
                                _submapper = mapper;
                            else
                                _nesHeaderInfo.Submapper = mapper;

                            foundSubMapper = true;
                        }
                    }

                    break;
                default:
                    return ErrorNumber.InvalidArgument;
            }
        }

        return foundRom && foundMapper ? ErrorNumber.NoError : ErrorNumber.InvalidArgument;
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
}