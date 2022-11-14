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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

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
        var magicBytes = new byte[4];
        stream.EnsureRead(magicBytes, 0, 4);
        var magic = BitConverter.ToUInt32(magicBytes, 0);

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
        var magicBytes = new byte[4];
        stream.EnsureRead(magicBytes, 0, 4);
        var magic = BitConverter.ToUInt32(magicBytes, 0);

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
        stream.EnsureRead(_data, 0, (int)imageFilter.DataForkLength);

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
            var tmp = new byte[_data.Length];

            for(var i = 0; i < _data.Length; i += 4)
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
            var tmp = new byte[_data.Length];

            for(var i = 0; i < _data.Length; i += 2)
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
        sb.AppendFormat("Version: {0}.{1}", header.Version / 10 + 1, header.Version % 10).AppendLine();
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
    [SuppressMessage("ReSharper", "CommentTypo")]
    public ErrorNumber GetMappings(out LinearMemoryMap mappings)
    {
        mappings = new LinearMemoryMap();

        if(!_opened)
        {
            ErrorMessage = "Not image has been opened.";

            return ErrorNumber.NotOpened;
        }

        mappings = new LinearMemoryMap();
        LinearMemoryType saveType   = LinearMemoryType.Unknown;
        ulong            saveLength = 0;

        Header header = Marshal.ByteArrayToStructureBigEndian<Header>(_data, 0, Marshal.SizeOf<Header>());

        switch((char)header.CartridgeType)
        {
            case 'N':
                switch(StringHandlers.SpacePaddedToString(header.CartridgeId))
                {
                    //Special case for first Japanese revisions of Kirby 64, overrides later entry
                    case "K4" when (char)header.CountryCode == 'J' && header.Version < 2:
                        saveType   = LinearMemoryType.SaveRAM;
                        saveLength = 32768;

                        break;
                    case "TW": //64 de Hakken!! Tamagotchi
                    case "HF": //64 Hanafuda: Tenshi no Yakusoku
                    case "OS": //64 Oozumou
                    case "TC": //64 Trump Collection
                    case "ER": //AeroFighters Assault
                    case "AG": //AeroGauge
                    case "AB": //Air Boarder 64
                    case "S3": //AI Shougi 3
                    case "TN": //All Star Tennis '99
                    case "BN": //Bakuretsu Muteki Bangaioh
                    case "BK": //Banjo-Kazooie
                    case "FH": //Bass Hunter 64
                    case "MU": //Big Mountain 2000
                    case "BC": //Blast Corps
                    case "BH": //Body Harvest
                    case "HA": //Bomberman 64: Arcade Edition (J)
                    case "BM": //Bomberman 64
                    case "BV": //Bomberman 64: The Second Attack!
                    case "BD": //Bomberman Hero
                    case "CT": //Chameleon Twist
                    case "CH": //Chopper Attack
                    case "CG": //Choro Q 64 II - Hacha Mecha Grand Prix Race (J)
                    case "P2": //Chou Kuukan Night Pro Yakyuu King 2 (J)
                    case "XO": //Cruis'n Exotica
                    case "CU": //Cruis'n USA
                    case "CX": //Custom Robo
                    case "DY": //Diddy Kong Racing
                    case "DQ": //Disney's Donald Duck - Goin' Quackers [Quack Attack (E)]
                    case "DR": //Doraemon: Nobita to 3tsu no Seireiseki
                    case "N6": //Dr. Mario 64
                    case "DU": //Duck Dodgers starring Daffy Duck
                    case "JM": //Earthworm Jim 3D
                    case "FW": //F-1 World Grand Prix
                    case "F2": //F-1 World Grand Prix II
                    case "KA": //Fighters Destiny
                    case "FG": //Fighter Destiny 2
                    case "GL": //Getter Love!!
                    case "GV": //Glover
                    case "GE": //GoldenEye 007
                    case "HP": //Heiwa Pachinko World 64
                    case "PG": //Hey You, Pikachu!
                    case "IJ": //Indiana Jones and the Infernal Machine
                    case "IC": //Indy Racing 2000
                    case "FY": //Kakutou Denshou: F-Cup Maniax
                    case "KI": //Killer Instinct Gold
                    case "LL": //Last Legion UX
                    case "LR": //Lode Runner 3-D
                    case "KT": //Mario Kart 64
                    case "LB": //Mario Party (PAL)
                    case "MW": //Mario Party 2
                    case "ML": //Mickey's Speedway USA
                    case "TM": //Mischief Makers [Yuke Yuke!! Trouble Makers (J)]
                    case "MI": //Mission: Impossible
                    case "MG": //Monaco Grand Prix [Racing Simulation 2 (G)]
                    case "MO": //Monopoly
                    case "MS": //Morita Shougi 64
                    case "MR": //Multi-Racing Championship
                    case "CR": //Penny Racers [Choro Q 64 (J)]
                    case "EA": //PGA European Tour
                    case "PW": //Pilotwings 64
                    case "PM": //Premier Manager 64 (E)
                    case "PY": //Puyo Puyo Sun 64
                    case "PT": //Puyo Puyon Party
                    case "RA": //Rally '99 (J)
                    case "WQ": //Rally Challenge 2000
                    case "SU": //Rocket: Robot on Wheels
                    case "SN": //Snow Speeder (J)
                    case "K2": //Snowboard Kids 2 [Chou Snobow Kids (J)]
                    case "SV": //Space Station Silicon Valley
                    case "FX": //Lylat Wars (E)
                    case "FP": //Star Fox 64 (U)
                    case "S6": //Star Soldier: Vanishing Earth
                    case "NA": //Star Wars Episode I: Battle for Naboo
                    case "RS": //Star Wars: Rogue Squadron
                    case "SW": //Star Wars: Shadows of the Empire
                    case "SC": //Starshot: Space Circus Fever
                    case "SA": //Sonic Wings Assault (J)
                    case "B6": //Super B-Daman: Battle Phoenix 64
                    case "SM": //Super Mario 64
                    case "SS": //Super Robot Spirits
                    case "TX": //Taz Express
                    case "T6": //Tetris 64
                    case "TP": //Tetrisphere
                    case "TJ": //Tom & Jerry in Fists of Fury
                    case "RC": //Top Gear Overdrive
                    case "TR": //Top Gear Rally (J + E)
                    case "TB": //Transformers: Beast Wars Metals 64
                    case "GU": //Tsumi to Batsu: Hoshi no Keishousha (Sin and Punishment)
                    case "IR": //Utchan Nanchan no Hono no Challenger: Denryuu Ira Ira Bou
                    case "VL": //V-Rally Edition '99
                    case "VY": //V-Rally Edition '99 (J)
                    case "WR": //Wave Race 64: Kawasaki Jet Ski
                    case "WC": //Wild Choppers
                    case "AD": //Worms Armageddon (U)
                    case "WU": //Worms Armageddon (E)
                    case "YK": //Yakouchuu II: Satsujin Kouro
                    case "MZ": //Zool - Majou Tsukai Densetsu (J)
                    case "DK" when (char)header.CountryCode == 'J': //Dark Rift aka Space Dynamites
                    case "WT" when (char)header.CountryCode == 'J': //Wetrix (J)
                        saveType   = LinearMemoryType.EEPROM;
                        saveLength = 512;

                        break;

                    //2KB EEPROM
                    case "B7": //Banjo-Tooie
                    case "GT": //City-Tour GP: Zen-Nihon GT Senshuken
                    case "FU": //Conker's Bad Fur Day
                    case "CW": //Cruis'n World
                    case "CZ": //Custom Robo V2
                    case "D6": //Densha de Go! 64
                    case "DO": //Donkey Kong 64
                    case "D2": //Doraemon 2: Nobita to Hikari no Shinden
                    case "3D": //Doraemon 3: Nobita no Machi SOS!
                    case "MX": //Excitebike 64
                    case "GC": //GT 64: Championship Edition
                    case "IM": //Ide Yosuke no Mahjong Juku
                    case "K4": //Kirby 64: The Crystal Shards
                    case "NB": //Kobe Bryant in NBA Courtside
                    case "MV": //Mario Party 3
                    case "M8": //Mario Tennis
                    case "EV": //Neon Genesis Evangelion
                    case "PP": //Parlor! Pro 64: Pachinko Jikki Simulation Game
                    case "UB": //PD Ultraman Battle Collection 64
                    case "PD": //Perfect Dark
                    case "RZ": //Ridge Racer 64
                    case "R7": //Robot Poncots 64: 7tsu no Umi no Caramel
                    case "EP": //Star Wars Episode I: Racer
                    case "YS": //Yoshi's Story

                    //Special cases for Japanese versions of Castlevania
                    case "D3" when (char)header.CountryCode == 'J': //Akumajou Dracula Mokushiroku (J)
                    case "D4"
                        when (char)header.CountryCode == 'J'
                        : //Akumajou Dracula Mokushiroku Gaiden: Legend of Cornell (J)
                        saveType   = LinearMemoryType.EEPROM;
                        saveLength = 2048;

                        break;

                    //32KB SRAM
                    case "TE": //1080 Snowboarding
                    case "VB": //Bass Rush - ECOGEAR PowerWorm Championship (J)
                    case "FZ": //F-Zero X (U + E)
                    case "SI": //Fushigi no Dungeon: Fuurai no Shiren 2
                    case "G6": //Ganmare Goemon: Dero Dero Douchuu Obake Tenkomori
                    case "3H": //Ganbare! Nippon! Olympics 2000
                    case "GP": //Goemon: Mononoke Sugoroku
                    case "YW": //Harvest Moon 64
                    case "HY": //Hybrid Heaven (J)
                    case "IB": //Itoi Shigesato no Bass Tsuri No. 1 Kettei Ban!
                    case "PS": //Jikkyou J.League 1999: Perfect Striker 2
                    case "PA": //Jikkyou Powerful Pro Yakyuu 2000
                    case "P4": //Jikkyou Powerful Pro Yakyuu 4
                    case "J5": //Jikkyou Powerful Pro Yakyuu 5
                    case "P6": //Jikkyou Powerful Pro Yakyuu 6
                    case "PE": //Jikkyou Powerful Pro Yakyuu Basic Ban 2001
                    case "JG": //Jinsei Game 64
                    case "ZL": //Legend of Zelda: Ocarina of Time (E)
                    case "KG": //Major League Baseball featuring Ken Griffey Jr.
                    case "MF": //Mario Golf 64
                    case "RI": //New Tetris, The
                    case "UT": //Nushi Zuri 64
                    case "UM": //Nushi Zuri 64: Shiokaze ni Notte
                    case "OB": //Ogre Battle 64: Person of Lordly Caliber
                    case "B5": //Resident Evil 2 (Japan) aka Biohazard 2
                    case "RE": //Resident Evil 2
                    case "AL": //Super Smash Bros. [Nintendo All-Star! Dairantou Smash Brothers (J)]
                    case "T3": //Shin Nihon Pro Wrestling - Toukon Road 2 - The Next Generation (J)
                    case "S4": //Super Robot Taisen 64
                    case "A2": //Virtual Pro Wrestling 2
                    case "VP": //Virtual Pro Wrestling 64
                    case "WL": //Waialae Country Club: True Golf Classics
                    case "W2": //WCW-nWo Revenge
                    case "WX": //WWF WrestleMania 2000
                        saveType   = LinearMemoryType.SaveRAM;
                        saveLength = 32768;

                        break;

                    //128KB Flash
                    case "CC": //Command & Conquer
                    case "DA": //Derby Stallion 64
                    case "AF": //Doubutsu no Mori
                    case "JF": //Jet Force Gemini
                    case "KJ": //Ken Griffey Jr.'s Slugfest
                    case "ZS": //Legend of Zelda: Majora's Mask [Zelda no Densetsu - Mujura no Kamen (J)]
                    case "M6": //Mega Man 64
                    case "CK": //NBA Courtside 2 featuring Kobe Bryant
                    case "MQ": //Paper Mario
                    case "PN": //Pokemon Puzzle League
                    case "PF": //Pokemon Snap [Pocket Monsters Snap (J)]
                    case "PO": //Pokemon Stadium
                    case "P3": //Pokemon Stadium 2 [Pocket Monsters Stadium - Kin Gin (J)]
                    case "RH": //Rockman Dash (J)
                    case "SQ": //StarCraft 64
                    case "T9": //Tigger's Honey Hunt
                    case "W4": //WWF No Mercy
                    case "DP": //Dinosaur Planet
                        saveType   = LinearMemoryType.NOR;
                        saveLength = 131072;

                        break;
                }

                break;
            case 'C':
                switch(StringHandlers.SpacePaddedToString(header.CartridgeId))
                {
                    case "LB": //Mario Party (NTSC)
                        saveType   = LinearMemoryType.EEPROM;
                        saveLength = 512;

                        break;

                    //32KB SRAM
                    case "FZ": //F-Zero X (J)
                    case "ZL": //Legend of Zelda: Ocarina of Time [Zelda no Densetsu - Toki no Ocarina (J)]
                    case "PS": //Pocket Monsters Stadium (J)
                        saveType   = LinearMemoryType.SaveRAM;
                        saveLength = 32768;

                        break;

                    //96KB SRAM
                    case "DZ": //Dezaemon 3D
                        saveType   = LinearMemoryType.SaveRAM;
                        saveLength = 98304;

                        break;

                    //128KB Flash
                    case "P2": //Pocket Monsters Stadium 2 (J)
                        saveType   = LinearMemoryType.NOR;
                        saveLength = 131072;

                        break;
                }

                break;
        }

        mappings = new LinearMemoryMap
        {
            Devices = saveLength > 0 ? new LinearMemoryDevice[2] : new LinearMemoryDevice[1]
        };

        mappings.Devices[0].Type = LinearMemoryType.ROM;

        mappings.Devices[0].PhysicalAddress = new LinearMemoryAddressing
        {
            Start  = 0,
            Length = (ulong)_data.Length
        };

        if(saveLength <= 0)
            return ErrorNumber.NoError;

        mappings.Devices[1].Type = saveType;

        mappings.Devices[1].PhysicalAddress = new LinearMemoryAddressing
        {
            Start  = (ulong)_data.Length,
            Length = saveLength
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

        // Sanitize
        foreach(LinearMemoryDevice map in mappings.Devices)
            switch(map.Type)
            {
                case LinearMemoryType.ROM when !foundRom:
                    foundRom = true;

                    break;
                case LinearMemoryType.SaveRAM when !foundSaveRam:
                case LinearMemoryType.NOR when !foundSaveRam:
                case LinearMemoryType.EEPROM when !foundSaveRam:
                    foundSaveRam = true;

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
            var tmp = new byte[_data.Length];

            for(var i = 0; i < _data.Length; i += 2)
            {
                tmp[i] = _data[i + 1];
                tmp[i            + 1] = _data[i];
            }

            _data = tmp;
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