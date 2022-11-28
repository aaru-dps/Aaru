// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Nintendo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Nintendo optical filesystems plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Nintendo optical filesystems and shows information.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the filesystem used by Nintendo Gamecube and Wii discs</summary>
public sealed class NintendoPlugin : IFilesystem
{
    const string FS_TYPE_NGC = "ngcfs";
    const string FS_TYPE_WII = "wiifs";
    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.NintendoPlugin_Name;
    /// <inheritdoc />
    public Guid Id => new("4675fcb4-4418-4288-9e4a-33d6a4ac1126");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start != 0)
            return false;

        if(imagePlugin.Info.Sectors * imagePlugin.Info.SectorSize < 0x50000)
            return false;

        ErrorNumber errno = imagePlugin.ReadSectors(0, 0x50000 / imagePlugin.Info.SectorSize, out byte[] header);

        if(errno != ErrorNumber.NoError)
            return false;

        uint magicGc  = BigEndianBitConverter.ToUInt32(header, 0x1C);
        uint magicWii = BigEndianBitConverter.ToUInt32(header, 0x18);

        return magicGc == 0xC2339F3D || magicWii == 0x5D1C9EA3;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding = encoding ?? Encoding.GetEncoding("shift_jis");
        var sbInformation = new StringBuilder();
        information = "";
        XmlFsType   = new FileSystemType();

        var fields = new NintendoFields();

        ErrorNumber errno = imagePlugin.ReadSectors(0, 0x50000 / imagePlugin.Info.SectorSize, out byte[] header);

        if(errno != ErrorNumber.NoError)
            return;

        bool wii = false;

        uint magicGc  = BigEndianBitConverter.ToUInt32(header, 0x1C);
        uint magicWii = BigEndianBitConverter.ToUInt32(header, 0x18);

        if(magicWii == 0x5D1C9EA3)
            wii = true;
        else if(magicGc != 0xC2339F3D)
            return;

        fields.DiscType         =  Encoding.ASCII.GetString(header, 0, 1);
        fields.GameCode         =  Encoding.ASCII.GetString(header, 1, 2);
        fields.RegionCode       =  Encoding.ASCII.GetString(header, 3, 1);
        fields.PublisherCode    =  Encoding.ASCII.GetString(header, 4, 2);
        fields.DiscId           =  Encoding.ASCII.GetString(header, 0, 6);
        fields.DiscNumber       =  header[6];
        fields.DiscVersion      =  header[7];
        fields.Streaming        |= header[8] > 0;
        fields.StreamBufferSize =  header[9];
        byte[] temp = new byte[64];
        Array.Copy(header, 0x20, temp, 0, 64);
        fields.Title = StringHandlers.CToString(temp, Encoding);

        if(!wii)
        {
            fields.DebugOff  = BigEndianBitConverter.ToUInt32(header, 0x0400);
            fields.DebugAddr = BigEndianBitConverter.ToUInt32(header, 0x0404);
            fields.DolOff    = BigEndianBitConverter.ToUInt32(header, 0x0420);
            fields.FstOff    = BigEndianBitConverter.ToUInt32(header, 0x0424);
            fields.FstSize   = BigEndianBitConverter.ToUInt32(header, 0x0428);
            fields.FstMax    = BigEndianBitConverter.ToUInt32(header, 0x042C);
        }

        if(wii)
        {
            uint offset1 = BigEndianBitConverter.ToUInt32(header, 0x40004) << 2;
            uint offset2 = BigEndianBitConverter.ToUInt32(header, 0x4000C) << 2;
            uint offset3 = BigEndianBitConverter.ToUInt32(header, 0x40014) << 2;
            uint offset4 = BigEndianBitConverter.ToUInt32(header, 0x4001C) << 2;

            fields.FirstPartitions  = new NintendoPartition[BigEndianBitConverter.ToUInt32(header, 0x40000)];
            fields.SecondPartitions = new NintendoPartition[BigEndianBitConverter.ToUInt32(header, 0x40008)];
            fields.ThirdPartitions  = new NintendoPartition[BigEndianBitConverter.ToUInt32(header, 0x40010)];
            fields.FourthPartitions = new NintendoPartition[BigEndianBitConverter.ToUInt32(header, 0x40018)];

            for(int i = 0; i < fields.FirstPartitions.Length; i++)
                if(offset1 + (i * 8) + 8 < 0x50000)
                {
                    fields.FirstPartitions[i].Offset =
                        BigEndianBitConverter.ToUInt32(header, (int)(offset1 + (i * 8) + 0)) << 2;

                    fields.FirstPartitions[i].Type =
                        BigEndianBitConverter.ToUInt32(header, (int)(offset1 + (i * 8) + 4));
                }

            for(int i = 0; i < fields.SecondPartitions.Length; i++)
                if(offset1 + (i * 8) + 8 < 0x50000)
                {
                    fields.FirstPartitions[i].Offset =
                        BigEndianBitConverter.ToUInt32(header, (int)(offset2 + (i * 8) + 0)) << 2;

                    fields.FirstPartitions[i].Type =
                        BigEndianBitConverter.ToUInt32(header, (int)(offset2 + (i * 8) + 4));
                }

            for(int i = 0; i < fields.ThirdPartitions.Length; i++)
                if(offset1 + (i * 8) + 8 < 0x50000)
                {
                    fields.FirstPartitions[i].Offset =
                        BigEndianBitConverter.ToUInt32(header, (int)(offset3 + (i * 8) + 0)) << 2;

                    fields.FirstPartitions[i].Type =
                        BigEndianBitConverter.ToUInt32(header, (int)(offset3 + (i * 8) + 4));
                }

            for(int i = 0; i < fields.FourthPartitions.Length; i++)
                if(offset1 + (i * 8) + 8 < 0x50000)
                {
                    fields.FirstPartitions[i].Offset =
                        BigEndianBitConverter.ToUInt32(header, (int)(offset4 + (i * 8) + 0)) << 2;

                    fields.FirstPartitions[i].Type =
                        BigEndianBitConverter.ToUInt32(header, (int)(offset4 + (i * 8) + 4));
                }

            fields.Region       = header[0x4E000];
            fields.JapanAge     = header[0x4E010];
            fields.UsaAge       = header[0x4E011];
            fields.GermanAge    = header[0x4E013];
            fields.PegiAge      = header[0x4E014];
            fields.FinlandAge   = header[0x4E015];
            fields.PortugalAge  = header[0x4E016];
            fields.UkAge        = header[0x4E017];
            fields.AustraliaAge = header[0x4E018];
            fields.KoreaAge     = header[0x4E019];
        }
        else
        {
            fields.FirstPartitions  = Array.Empty<NintendoPartition>();
            fields.SecondPartitions = Array.Empty<NintendoPartition>();
            fields.ThirdPartitions  = Array.Empty<NintendoPartition>();
            fields.FourthPartitions = Array.Empty<NintendoPartition>();
        }

        AaruConsole.DebugWriteLine("Nintendo plugin", "discType = {0}", fields.DiscType);
        AaruConsole.DebugWriteLine("Nintendo plugin", "gameCode = {0}", fields.GameCode);
        AaruConsole.DebugWriteLine("Nintendo plugin", "regionCode = {0}", fields.RegionCode);
        AaruConsole.DebugWriteLine("Nintendo plugin", "publisherCode = {0}", fields.PublisherCode);
        AaruConsole.DebugWriteLine("Nintendo plugin", "discID = {0}", fields.DiscId);
        AaruConsole.DebugWriteLine("Nintendo plugin", "discNumber = {0}", fields.DiscNumber);
        AaruConsole.DebugWriteLine("Nintendo plugin", "discVersion = {0}", fields.DiscVersion);
        AaruConsole.DebugWriteLine("Nintendo plugin", "streaming = {0}", fields.Streaming);
        AaruConsole.DebugWriteLine("Nintendo plugin", "streamBufferSize = {0}", fields.StreamBufferSize);
        AaruConsole.DebugWriteLine("Nintendo plugin", "title = \"{0}\"", fields.Title);
        AaruConsole.DebugWriteLine("Nintendo plugin", "debugOff = 0x{0:X8}", fields.DebugOff);
        AaruConsole.DebugWriteLine("Nintendo plugin", "debugAddr = 0x{0:X8}", fields.DebugAddr);
        AaruConsole.DebugWriteLine("Nintendo plugin", "dolOff = 0x{0:X8}", fields.DolOff);
        AaruConsole.DebugWriteLine("Nintendo plugin", "fstOff = 0x{0:X8}", fields.FstOff);
        AaruConsole.DebugWriteLine("Nintendo plugin", "fstSize = {0}", fields.FstSize);
        AaruConsole.DebugWriteLine("Nintendo plugin", "fstMax = {0}", fields.FstMax);

        for(int i = 0; i < fields.FirstPartitions.Length; i++)
        {
            AaruConsole.DebugWriteLine("Nintendo plugin", "firstPartitions[{1}].offset = {0}",
                                       fields.FirstPartitions[i].Offset, i);

            AaruConsole.DebugWriteLine("Nintendo plugin", "firstPartitions[{1}].type = {0}",
                                       fields.FirstPartitions[i].Type, i);
        }

        for(int i = 0; i < fields.SecondPartitions.Length; i++)
        {
            AaruConsole.DebugWriteLine("Nintendo plugin", "secondPartitions[{1}].offset = {0}",
                                       fields.SecondPartitions[i].Offset, i);

            AaruConsole.DebugWriteLine("Nintendo plugin", "secondPartitions[{1}].type = {0}",
                                       fields.SecondPartitions[i].Type, i);
        }

        for(int i = 0; i < fields.ThirdPartitions.Length; i++)
        {
            AaruConsole.DebugWriteLine("Nintendo plugin", "thirdPartitions[{1}].offset = {0}",
                                       fields.ThirdPartitions[i].Offset, i);

            AaruConsole.DebugWriteLine("Nintendo plugin", "thirdPartitions[{1}].type = {0}",
                                       fields.ThirdPartitions[i].Type, i);
        }

        for(int i = 0; i < fields.FourthPartitions.Length; i++)
        {
            AaruConsole.DebugWriteLine("Nintendo plugin", "fourthPartitions[{1}].offset = {0}",
                                       fields.FourthPartitions[i].Offset, i);

            AaruConsole.DebugWriteLine("Nintendo plugin", "fourthPartitions[{1}].type = {0}",
                                       fields.FourthPartitions[i].Type, i);
        }

        AaruConsole.DebugWriteLine("Nintendo plugin", "region = {0}", fields.Region);
        AaruConsole.DebugWriteLine("Nintendo plugin", "japanAge = {0}", fields.JapanAge);
        AaruConsole.DebugWriteLine("Nintendo plugin", "usaAge = {0}", fields.UsaAge);
        AaruConsole.DebugWriteLine("Nintendo plugin", "germanAge = {0}", fields.GermanAge);
        AaruConsole.DebugWriteLine("Nintendo plugin", "pegiAge = {0}", fields.PegiAge);
        AaruConsole.DebugWriteLine("Nintendo plugin", "finlandAge = {0}", fields.FinlandAge);
        AaruConsole.DebugWriteLine("Nintendo plugin", "portugalAge = {0}", fields.PortugalAge);
        AaruConsole.DebugWriteLine("Nintendo plugin", "ukAge = {0}", fields.UkAge);
        AaruConsole.DebugWriteLine("Nintendo plugin", "australiaAge = {0}", fields.AustraliaAge);
        AaruConsole.DebugWriteLine("Nintendo plugin", "koreaAge = {0}", fields.KoreaAge);

        sbInformation.AppendLine(Localization.Nintendo_optical_filesystem);

        sbInformation.AppendLine(wii ? Localization.Nintendo_Wii_Optical_Disc
                                     : Localization.Nintendo_GameCube_Optical_Disc);

        sbInformation.AppendFormat(Localization.Disc_ID_is_0, fields.DiscId).AppendLine();
        sbInformation.AppendFormat(Localization.Disc_is_a_0_disc, DiscTypeToString(fields.DiscType)).AppendLine();
        sbInformation.AppendFormat(Localization.Disc_region_is_0, RegionCodeToString(fields.RegionCode)).AppendLine();

        sbInformation.AppendFormat(Localization.Published_by_0, PublisherCodeToString(fields.PublisherCode)).
                      AppendLine();

        if(fields.DiscNumber > 0)
            sbInformation.AppendFormat(Localization.Disc_number_0_of_a_multi_disc_set, fields.DiscNumber + 1).
                          AppendLine();

        if(fields.Streaming)
            sbInformation.AppendLine(Localization.Disc_is_prepared_for_audio_streaming);

        if(fields.StreamBufferSize > 0)
            sbInformation.AppendFormat(Localization.Audio_streaming_buffer_size_is_0_bytes, fields.StreamBufferSize).
                          AppendLine();

        sbInformation.AppendFormat(Localization.Title_0, fields.Title).AppendLine();

        if(wii)
        {
            for(int i = 0; i < fields.FirstPartitions.Length; i++)
                sbInformation.AppendFormat(Localization.First_0_partition_starts_at_sector_1,
                                           PartitionTypeToString(fields.FirstPartitions[i].Type),
                                           fields.FirstPartitions[i].Offset / 2048).AppendLine();

            for(int i = 0; i < fields.SecondPartitions.Length; i++)
                sbInformation.AppendFormat(Localization.Second_0_partition_starts_at_sector_1,
                                           PartitionTypeToString(fields.SecondPartitions[i].Type),
                                           fields.SecondPartitions[i].Offset / 2048).AppendLine();

            for(int i = 0; i < fields.ThirdPartitions.Length; i++)
                sbInformation.AppendFormat(Localization.Third_0_partition_starts_at_sector_1,
                                           PartitionTypeToString(fields.ThirdPartitions[i].Type),
                                           fields.ThirdPartitions[i].Offset / 2048).AppendLine();

            for(int i = 0; i < fields.FourthPartitions.Length; i++)
                sbInformation.AppendFormat(Localization.Fourth_0_partition_starts_at_sector_1,
                                           PartitionTypeToString(fields.FourthPartitions[i].Type),
                                           fields.FourthPartitions[i].Offset / 2048).AppendLine();

            //                sbInformation.AppendFormat("Region byte is {0}", fields.region).AppendLine();
            if((fields.JapanAge & 0x80) != 0x80)
                sbInformation.AppendFormat(Localization.Japan_age_rating_is_0, fields.JapanAge).AppendLine();

            if((fields.UsaAge & 0x80) != 0x80)
                sbInformation.AppendFormat(Localization.ESRB_age_rating_is_0, fields.UsaAge).AppendLine();

            if((fields.GermanAge & 0x80) != 0x80)
                sbInformation.AppendFormat(Localization.German_age_rating_is_0, fields.GermanAge).AppendLine();

            if((fields.PegiAge & 0x80) != 0x80)
                sbInformation.AppendFormat(Localization.PEGI_age_rating_is_0, fields.PegiAge).AppendLine();

            if((fields.FinlandAge & 0x80) != 0x80)
                sbInformation.AppendFormat(Localization.Finland_age_rating_is_0, fields.FinlandAge).AppendLine();

            if((fields.PortugalAge & 0x80) != 0x80)
                sbInformation.AppendFormat(Localization.Portugal_age_rating_is_0, fields.PortugalAge).AppendLine();

            if((fields.UkAge & 0x80) != 0x80)
                sbInformation.AppendFormat(Localization.UK_age_rating_is_0, fields.UkAge).AppendLine();

            if((fields.AustraliaAge & 0x80) != 0x80)
                sbInformation.AppendFormat(Localization.Australia_age_rating_is_0, fields.AustraliaAge).AppendLine();

            if((fields.KoreaAge & 0x80) != 0x80)
                sbInformation.AppendFormat(Localization.Korea_age_rating_is_0, fields.KoreaAge).AppendLine();
        }
        else
            sbInformation.AppendFormat(Localization.FST_starts_at_0_and_has_1_bytes, fields.FstOff, fields.FstSize).
                          AppendLine();

        information            = sbInformation.ToString();
        XmlFsType.Bootable     = true;
        XmlFsType.Clusters     = imagePlugin.Info.Sectors * imagePlugin.Info.SectorSize / 2048;
        XmlFsType.ClusterSize  = 2048;
        XmlFsType.Type         = wii ? FS_TYPE_WII : FS_TYPE_NGC;
        XmlFsType.VolumeName   = fields.Title;
        XmlFsType.VolumeSerial = fields.DiscId;
    }

    static string DiscTypeToString(string discType) => discType switch
    {
        "C" => Localization.Commodore_64_Virtual_Console,
        "D" => Localization.Demo,
        "E" => Localization.Neo_Geo_Virtual_Console,
        "F" => Localization.NES_Virtual_Console,
        "G" => Localization.Gamecube,
        "H" => Localization.Wii_channel,
        "J" => Localization.Super_Nintendo_Virtual_Console,
        "L" => Localization.Master_System_Virtual_Console,
        "M" => Localization.Megadrive_Virtual_Console,
        "N" => Localization.Nintendo_64_Virtual_Console,
        "P" => Localization.Promotional_or_TurboGrafx_Virtual_Console,
        "Q" => Localization.TurboGrafx_CD_Virtual_Console,
        "R" => Localization.Wii,
        "S" => Localization.Wii,
        "U" => Localization.Utility,
        "W" => Localization.WiiWare,
        "X" => Localization.MSX_Virtual_Console_or_WiiWare_demo,
        "0" => Localization.Diagnostic,
        "1" => Localization.Diagnostic,
        "4" => Localization.Wii_Backup,
        "_" => Localization.WiiFit,
        _   => string.Format(Localization.unknown_type_0, discType)
    };

    static string RegionCodeToString(string regionCode) => regionCode switch
    {
        "A" => Localization.NintendoPlugin_RegionCodeToString_any_region,
        "D" => Localization.NintendoPlugin_RegionCodeToString_Germany,
        "N" => Localization.NintendoPlugin_RegionCodeToString_USA,
        "E" => Localization.NintendoPlugin_RegionCodeToString_USA,
        "F" => Localization.NintendoPlugin_RegionCodeToString_France,
        "I" => Localization.NintendoPlugin_RegionCodeToString_Italy,
        "J" => Localization.NintendoPlugin_RegionCodeToString_Japan,
        "K" => Localization.NintendoPlugin_RegionCodeToString_Korea,
        "Q" => Localization.NintendoPlugin_RegionCodeToString_Korea,
        "L" => Localization.NintendoPlugin_RegionCodeToString_PAL,
        "M" => Localization.NintendoPlugin_RegionCodeToString_PAL,
        "P" => Localization.NintendoPlugin_RegionCodeToString_PAL,
        "R" => Localization.NintendoPlugin_RegionCodeToString_Russia,
        "S" => Localization.NintendoPlugin_RegionCodeToString_Spain,
        "T" => Localization.NintendoPlugin_RegionCodeToString_Taiwan,
        "U" => Localization.NintendoPlugin_RegionCodeToString_Australia,
        _   => string.Format(Localization.NintendoPlugin_RegionCodeToString_unknown_region_code_0, regionCode)
    };

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    static string PublisherCodeToString(string publisherCode) => publisherCode switch
    {
        "01" => "Nintendo",
        "08" => "CAPCOM",
        "41" => "Ubisoft",
        "4F" => "Eidos",
        "51" => "Acclaim",
        "52" => "Activision",
        "5D" => "Midway",
        "5G" => "Hudson",
        "64" => "LucasArts",
        "69" => "Electronic Arts",
        "6S" => "TDK Mediactive",
        "8P" => "SEGA",
        "A4" => "Mirage Studios",
        "AF" => "Namco",
        "B2" => "Bandai",
        "DA" => "Tomy",
        "EM" => "Konami",
        "70" => "Atari",
        "4Q" => "Disney Interactive",
        "GD" => "Square Enix",
        "7D" => "Sierra",
        _    => string.Format(Localization.Unknown_publisher_0, publisherCode)
    };

    static string PartitionTypeToString(uint type) => type switch
    {
        0 => Localization.data,
        1 => Localization.update,
        2 => Localization.channel,
        _ => string.Format(Localization.unknown_partition_type_0, type)
    };

    struct NintendoFields
    {
        public string              DiscType;
        public string              GameCode;
        public string              RegionCode;
        public string              PublisherCode;
        public string              DiscId;
        public byte                DiscNumber;
        public byte                DiscVersion;
        public bool                Streaming;
        public byte                StreamBufferSize;
        public string              Title;
        public uint                DebugOff;
        public uint                DebugAddr;
        public uint                DolOff;
        public uint                FstOff;
        public uint                FstSize;
        public uint                FstMax;
        public NintendoPartition[] FirstPartitions;
        public NintendoPartition[] SecondPartitions;
        public NintendoPartition[] ThirdPartitions;
        public NintendoPartition[] FourthPartitions;
        public byte                Region;
        public byte                JapanAge;
        public byte                UsaAge;
        public byte                GermanAge;
        public byte                PegiAge;
        public byte                FinlandAge;
        public byte                PortugalAge;
        public byte                UkAge;
        public byte                AustraliaAge;
        public byte                KoreaAge;
    }

    struct NintendoPartition
    {
        public uint Offset;
        public uint Type;
    }
}