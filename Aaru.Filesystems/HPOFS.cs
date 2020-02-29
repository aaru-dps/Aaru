// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HPOFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the High Performance Optical File System and shows
//     information.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    // Information from test floppy images created with OS/2 HPOFS 2.0
    // Need to get IBM document GA32-0224 -> IBM 3995 Optical Library Dataserver Products: Optical Disk Format
    public class HPOFS : IFilesystem
    {
        readonly byte[] hpofsType =
        {
            0x48, 0x50, 0x4F, 0x46, 0x53, 0x00, 0x00, 0x00
        };
        readonly byte[] medinfoSignature =
        {
            0x4D, 0x45, 0x44, 0x49, 0x4E, 0x46, 0x4F, 0x20
        };
        readonly byte[] volinfoSignature =
        {
            0x56, 0x4F, 0x4C, 0x49, 0x4E, 0x46, 0x4F, 0x20
        };

        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "High Performance Optical File System";
        public Guid           Id        => new Guid("1b72dcd5-d031-4757-8a9f-8d2fb18c59e2");
        public string         Author    => "Natalia Portillo";

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(16 + partition.Start >= partition.End)
                return false;

            byte[] hpofsBpbSector =
                imagePlugin.ReadSector(0 + partition.Start); // Seek to BIOS parameter block, on logical sector 0

            if(hpofsBpbSector.Length < 512)
                return false;

            BiosParameterBlock bpb = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock>(hpofsBpbSector);

            return bpb.fs_type.SequenceEqual(hpofsType);
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("ibm850");
            information = "";

            var sb = new StringBuilder();

            byte[] hpofsBpbSector =
                imagePlugin.ReadSector(0 + partition.Start); // Seek to BIOS parameter block, on logical sector 0

            byte[] medInfoSector =
                imagePlugin.ReadSector(13 + partition.Start); // Seek to media information block, on logical sector 13

            byte[] volInfoSector =
                imagePlugin.ReadSector(14 + partition.Start); // Seek to volume information block, on logical sector 14

            BiosParameterBlock     bpb = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock>(hpofsBpbSector);
            MediaInformationBlock  mib = Marshal.ByteArrayToStructureBigEndian<MediaInformationBlock>(medInfoSector);
            VolumeInformationBlock vib = Marshal.ByteArrayToStructureBigEndian<VolumeInformationBlock>(volInfoSector);

            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.oem_name = \"{0}\"",
                                       StringHandlers.CToString(bpb.oem_name));

            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.bps = {0}", bpb.bps);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.spc = {0}", bpb.spc);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.rsectors = {0}", bpb.rsectors);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.fats_no = {0}", bpb.fats_no);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.root_ent = {0}", bpb.root_ent);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.sectors = {0}", bpb.sectors);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.media = 0x{0:X2}", bpb.media);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.spfat = {0}", bpb.spfat);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.sptrk = {0}", bpb.sptrk);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.heads = {0}", bpb.heads);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.hsectors = {0}", bpb.hsectors);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.big_sectors = {0}", bpb.big_sectors);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.drive_no = 0x{0:X2}", bpb.drive_no);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.nt_flags = {0}", bpb.nt_flags);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.signature = 0x{0:X2}", bpb.signature);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.serial_no = 0x{0:X8}", bpb.serial_no);

            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.volume_label = \"{0}\"",
                                       StringHandlers.SpacePaddedToString(bpb.volume_label));

            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.fs_type = \"{0}\"", StringHandlers.CToString(bpb.fs_type));

            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.boot_code is empty? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(bpb.boot_code));

            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.unknown = {0}", bpb.unknown);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.unknown2 = {0}", bpb.unknown2);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.signature2 = {0}", bpb.signature2);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.blockId = \"{0}\"", StringHandlers.CToString(mib.blockId));

            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.volumeLabel = \"{0}\"",
                                       StringHandlers.SpacePaddedToString(mib.volumeLabel));

            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.comment = \"{0}\"",
                                       StringHandlers.SpacePaddedToString(mib.comment));

            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.serial = 0x{0:X8}", mib.serial);

            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.creationTimestamp = {0}",
                                       DateHandlers.DosToDateTime(mib.creationDate, mib.creationTime));

            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.codepageType = {0}", mib.codepageType);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.codepage = {0}", mib.codepage);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.rps = {0}", mib.rps);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.bps = {0}", mib.bps);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.bpc = {0}", mib.bpc);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.unknown2 = {0}", mib.unknown2);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.sectors = {0}", mib.sectors);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.unknown3 = {0}", mib.unknown3);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.unknown4 = {0}", mib.unknown4);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.major = {0}", mib.major);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.minor = {0}", mib.minor);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.unknown5 = {0}", mib.unknown5);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.unknown6 = {0}", mib.unknown6);

            AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.filler is empty? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(mib.filler));

            AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.blockId = \"{0}\"", StringHandlers.CToString(vib.blockId));
            AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown = {0}", vib.unknown);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown2 = {0}", vib.unknown2);

            AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown3 is empty? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(vib.unknown3));

            AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown4 = \"{0}\"",
                                       StringHandlers.SpacePaddedToString(vib.unknown4));

            AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.owner = \"{0}\"",
                                       StringHandlers.SpacePaddedToString(vib.owner));

            AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown5 = \"{0}\"",
                                       StringHandlers.SpacePaddedToString(vib.unknown5));

            AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown6 = {0}", vib.unknown6);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.percentFull = {0}", vib.percentFull);
            AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown7 = {0}", vib.unknown7);

            AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.filler is empty? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(vib.filler));

            sb.AppendLine("High Performance Optical File System");
            sb.AppendFormat("OEM name: {0}", StringHandlers.SpacePaddedToString(bpb.oem_name)).AppendLine();
            sb.AppendFormat("{0} bytes per sector", bpb.bps).AppendLine();
            sb.AppendFormat("{0} sectors per cluster", bpb.spc).AppendLine();
            sb.AppendFormat("Media descriptor: 0x{0:X2}", bpb.media).AppendLine();
            sb.AppendFormat("{0} sectors per track", bpb.sptrk).AppendLine();
            sb.AppendFormat("{0} heads", bpb.heads).AppendLine();
            sb.AppendFormat("{0} sectors hidden before BPB", bpb.hsectors).AppendLine();
            sb.AppendFormat("{0} sectors on volume ({1} bytes)", mib.sectors, mib.sectors * bpb.bps).AppendLine();
            sb.AppendFormat("BIOS Drive Number: 0x{0:X2}", bpb.drive_no).AppendLine();
            sb.AppendFormat("Serial number: 0x{0:X8}", mib.serial).AppendLine();

            sb.AppendFormat("Volume label: {0}", StringHandlers.SpacePaddedToString(mib.volumeLabel, Encoding)).
               AppendLine();

            sb.AppendFormat("Volume comment: {0}", StringHandlers.SpacePaddedToString(mib.comment, Encoding)).
               AppendLine();

            sb.AppendFormat("Volume owner: {0}", StringHandlers.SpacePaddedToString(vib.owner, Encoding)).AppendLine();

            sb.AppendFormat("Volume created on {0}", DateHandlers.DosToDateTime(mib.creationDate, mib.creationTime)).
               AppendLine();

            sb.AppendFormat("Volume uses {0} codepage {1}", mib.codepageType > 0 && mib.codepageType < 3
                                                                ? mib.codepageType == 2
                                                                      ? "EBCDIC"
                                                                      : "ASCII" : "Unknown", mib.codepage).AppendLine();

            sb.AppendFormat("RPS level: {0}", mib.rps).AppendLine();
            sb.AppendFormat("Filesystem version: {0}.{1}", mib.major, mib.minor).AppendLine();
            sb.AppendFormat("Volume can be filled up to {0}%", vib.percentFull).AppendLine();

            XmlFsType = new FileSystemType
            {
                Clusters               = mib.sectors / bpb.spc, ClusterSize = (uint)(bpb.bps * bpb.spc),
                CreationDate           = DateHandlers.DosToDateTime(mib.creationDate, mib.creationTime),
                CreationDateSpecified  = true,
                DataPreparerIdentifier = StringHandlers.SpacePaddedToString(vib.owner, Encoding), Type = "HPOFS",
                VolumeName             = StringHandlers.SpacePaddedToString(mib.volumeLabel, Encoding),
                VolumeSerial           = $"{mib.serial:X8}",
                SystemIdentifier       = StringHandlers.SpacePaddedToString(bpb.oem_name)
            };

            information = sb.ToString();
        }

        /// <summary>BIOS Parameter Block, at sector 0, little-endian</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BiosParameterBlock
        {
            /// <summary>0x000, Jump to boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly byte[] jump;
            /// <summary>0x003, OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] oem_name;
            /// <summary>0x00B, Bytes per sector</summary>
            public readonly ushort bps;
            /// <summary>0x00D, Sectors per cluster</summary>
            public readonly byte spc;
            /// <summary>0x00E, Reserved sectors between BPB and... does it have sense in HPFS?</summary>
            public readonly ushort rsectors;
            /// <summary>0x010, Number of FATs... seriously?</summary>
            public readonly byte fats_no;
            /// <summary>0x011, Number of entries on root directory... ok</summary>
            public readonly ushort root_ent;
            /// <summary>0x013, Sectors in volume... doubt it</summary>
            public readonly ushort sectors;
            /// <summary>0x015, Media descriptor</summary>
            public readonly byte media;
            /// <summary>0x016, Sectors per FAT... again</summary>
            public readonly ushort spfat;
            /// <summary>0x018, Sectors per track... you're kidding</summary>
            public readonly ushort sptrk;
            /// <summary>0x01A, Heads... stop!</summary>
            public readonly ushort heads;
            /// <summary>0x01C, Hidden sectors before BPB</summary>
            public readonly uint hsectors;
            /// <summary>0x024, Sectors in volume if &gt; 65535...</summary>
            public readonly uint big_sectors;
            /// <summary>0x028, Drive number</summary>
            public readonly byte drive_no;
            /// <summary>0x029, Volume flags?</summary>
            public readonly byte nt_flags;
            /// <summary>0x02A, EPB signature, 0x29</summary>
            public readonly byte signature;
            /// <summary>0x02B, Volume serial number</summary>
            public readonly uint serial_no;
            /// <summary>0x02F, Volume label, 11 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public readonly byte[] volume_label;
            /// <summary>0x03A, Filesystem type, 8 bytes, space-padded ("HPFS    ")</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] fs_type;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 442)]
            public readonly byte[] boot_code;
            /// <summary>0x1F8, Unknown</summary>
            public readonly uint unknown;
            /// <summary>0x1FC, Unknown</summary>
            public readonly ushort unknown2;
            /// <summary>0x1FE, 0xAA55</summary>
            public readonly ushort signature2;
        }

        /// <summary>Media Information Block, at sector 13, big-endian</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MediaInformationBlock
        {
            /// <summary>Block identifier "MEDINFO "</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] blockId;
            /// <summary>Volume label</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] volumeLabel;
            /// <summary>Volume comment</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 160)]
            public readonly byte[] comment;
            /// <summary>Volume serial number</summary>
            public readonly uint serial;
            /// <summary>Volume creation date, DOS format</summary>
            public readonly ushort creationDate;
            /// <summary>Volume creation time, DOS format</summary>
            public readonly ushort creationTime;
            /// <summary>Codepage type: 1 ASCII, 2 EBCDIC</summary>
            public readonly ushort codepageType;
            /// <summary>Codepage</summary>
            public readonly ushort codepage;
            /// <summary>RPS level</summary>
            public readonly uint rps;
            /// <summary>Coincides with bytes per sector, and bytes per cluster, need more media</summary>
            public readonly ushort bps;
            /// <summary>Coincides with bytes per sector, and bytes per cluster, need more media</summary>
            public readonly ushort bpc;
            /// <summary>Unknown, empty</summary>
            public readonly uint unknown2;
            /// <summary>Sectors (or clusters)</summary>
            public readonly uint sectors;
            /// <summary>Unknown, coincides with bps but changing it makes nothing</summary>
            public readonly uint unknown3;
            /// <summary>Empty?</summary>
            public readonly ulong unknown4;
            /// <summary>Format major version</summary>
            public readonly ushort major;
            /// <summary>Format minor version</summary>
            public readonly ushort minor;
            /// <summary>Empty?</summary>
            public readonly uint unknown5;
            /// <summary>Unknown, non-empty</summary>
            public readonly uint unknown6;
            /// <summary>Empty</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
            public readonly byte[] filler;
        }

        /// <summary>Volume Information Block, at sector 14, big-endian</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VolumeInformationBlock
        {
            /// <summary>Block identifier "VOLINFO "</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] blockId;
            /// <summary>Unknown</summary>
            public readonly uint unknown;
            /// <summary>Unknown</summary>
            public readonly uint unknown2;
            /// <summary>Unknown</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            public readonly byte[] unknown3;
            /// <summary>Unknown, space-padded string</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] unknown4;
            /// <summary>Owner, space-padded string</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] owner;
            /// <summary>Unknown, space-padded string</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] unknown5;
            /// <summary>Unknown, empty?</summary>
            public readonly uint unknown6;
            /// <summary>Maximum percent full</summary>
            public readonly ushort percentFull;
            /// <summary>Unknown, empty?</summary>
            public readonly ushort unknown7;
            /// <summary>Empty</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 384)]
            public readonly byte[] filler;
        }
    }
}