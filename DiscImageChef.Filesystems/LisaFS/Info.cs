// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Lisa filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Apple Lisa filesystem and shows information.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using DiscImageChef.Console;
using DiscImageChef.Decoders;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.Filesystems.LisaFS
{
    partial class LisaFS : Filesystem
    {
        public override bool Identify(ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            try
            {
                if(imagePlugin.ImageInfo.readableSectorTags == null)
                    return false;

                if(!imagePlugin.ImageInfo.readableSectorTags.Contains(SectorTagType.AppleSectorTag))
                    return false;

                // LisaOS is big-endian
                BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

                // Minimal LisaOS disk is 3.5" single sided double density, 800 sectors
                if(imagePlugin.GetSectors() < 800)
                    return false;

                int before_mddf = -1;

                // LisaOS searches sectors until tag tells MDDF resides there, so we'll search 100 sectors
                for(int i = 0; i < 100; i++)
                {
                    LisaTag.PriamTag searchTag;
                    DecodeTag(imagePlugin.ReadSectorTag((ulong)i, SectorTagType.AppleSectorTag), out searchTag);

                    DicConsole.DebugWriteLine("LisaFS plugin", "Sector {0}, file ID 0x{1:X4}", i, searchTag.fileID);

                    if(before_mddf == -1 && searchTag.fileID == FILEID_LOADER_SIGNED)
                        before_mddf = i - 1;

                    if(searchTag.fileID == FILEID_MDDF)
                    {
                        byte[] sector = imagePlugin.ReadSector((ulong)i);
                        MDDF info_mddf = new MDDF();

                        info_mddf.mddf_block = BigEndianBitConverter.ToUInt32(sector, 0x6C);
                        info_mddf.volsize_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x70);
                        info_mddf.volsize_minus_mddf_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x74);
                        info_mddf.vol_size = BigEndianBitConverter.ToUInt32(sector, 0x78);
                        info_mddf.blocksize = BigEndianBitConverter.ToUInt16(sector, 0x7C);
                        info_mddf.datasize = BigEndianBitConverter.ToUInt16(sector, 0x7E);

                        DicConsole.DebugWriteLine("LisaFS plugin", "Current sector = {0}", i);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.mddf_block = {0}", info_mddf.mddf_block);
                        DicConsole.DebugWriteLine("LisaFS plugin", "Disk size = {0} sectors", imagePlugin.GetSectors());
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.vol_size = {0} sectors", info_mddf.vol_size);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.vol_size - 1 = {0}", info_mddf.volsize_minus_one);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.vol_size - mddf.mddf_block -1 = {0}", info_mddf.volsize_minus_mddf_minus_one);
                        DicConsole.DebugWriteLine("LisaFS plugin", "Disk sector = {0} bytes", imagePlugin.GetSectorSize());
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.blocksize = {0} bytes", info_mddf.blocksize);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.datasize = {0} bytes", info_mddf.datasize);

                        if(info_mddf.mddf_block != i - before_mddf)
                            return false;

                        if(info_mddf.vol_size > imagePlugin.GetSectors())
                            return false;

                        if(info_mddf.vol_size - 1 != info_mddf.volsize_minus_one)
                            return false;

                        if(info_mddf.vol_size - i - 1 != info_mddf.volsize_minus_mddf_minus_one - before_mddf)
                            return false;

                        if(info_mddf.datasize > info_mddf.blocksize)
                            return false;

                        if(info_mddf.blocksize < imagePlugin.GetSectorSize())
                            return false;

                        if(info_mddf.datasize != imagePlugin.GetSectorSize())
                            return false;

                        return true;
                    }
                }

                return false;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception {0}, {1}, {2}", ex.Message, ex.InnerException, ex.StackTrace);
                return false;
            }
        }

        public override void GetInformation(ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";
            StringBuilder sb = new StringBuilder();

            try
            {
                if(imagePlugin.ImageInfo.readableSectorTags == null)
                    return;

                if(!imagePlugin.ImageInfo.readableSectorTags.Contains(SectorTagType.AppleSectorTag))
                    return;

                // LisaOS is big-endian
                BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

                // Minimal LisaOS disk is 3.5" single sided double density, 800 sectors
                if(imagePlugin.GetSectors() < 800)
                    return;

                int before_mddf = -1;

                // LisaOS searches sectors until tag tells MDDF resides there, so we'll search 100 sectors
                for(int i = 0; i < 100; i++)
                {
                    LisaTag.PriamTag searchTag;
                    DecodeTag(imagePlugin.ReadSectorTag((ulong)i, SectorTagType.AppleSectorTag), out searchTag);

                    DicConsole.DebugWriteLine("LisaFS plugin", "Sector {0}, file ID 0x{1:X4}", i, searchTag.fileID);

                    if(before_mddf == -1 && searchTag.fileID == FILEID_LOADER_SIGNED)
                        before_mddf = i - 1;

                    if(searchTag.fileID == FILEID_MDDF)
                    {
                        byte[] sector = imagePlugin.ReadSector((ulong)i);
                        MDDF info_mddf = new MDDF();
                        byte[] pString = new byte[33];
                        uint lisa_time;

                        info_mddf.fsversion = BigEndianBitConverter.ToUInt16(sector, 0x00);
                        info_mddf.volid = BigEndianBitConverter.ToUInt64(sector, 0x02);
                        info_mddf.volnum = BigEndianBitConverter.ToUInt16(sector, 0x0A);
                        Array.Copy(sector, 0x0C, pString, 0, 33);
                        info_mddf.volname = GetStringFromPascal(pString);
                        info_mddf.unknown1 = sector[0x2D];
                        Array.Copy(sector, 0x2E, pString, 0, 33);
                        // Prevent garbage
                        if(pString[0] <= 32)
                            info_mddf.password = GetStringFromPascal(pString);
                        else
                            info_mddf.password = "";
                        info_mddf.unknown2 = sector[0x4F];
                        info_mddf.machine_id = BigEndianBitConverter.ToUInt32(sector, 0x50);
                        info_mddf.master_copy_id = BigEndianBitConverter.ToUInt32(sector, 0x54);
                        lisa_time = BigEndianBitConverter.ToUInt32(sector, 0x58);
                        info_mddf.dtvc = DateHandlers.LisaToDateTime(lisa_time);
                        lisa_time = BigEndianBitConverter.ToUInt32(sector, 0x5C);
                        info_mddf.dtcc = DateHandlers.LisaToDateTime(lisa_time);
                        lisa_time = BigEndianBitConverter.ToUInt32(sector, 0x60);
                        info_mddf.dtvb = DateHandlers.LisaToDateTime(lisa_time);
                        lisa_time = BigEndianBitConverter.ToUInt32(sector, 0x64);
                        info_mddf.dtvs = DateHandlers.LisaToDateTime(lisa_time);
                        info_mddf.unknown3 = BigEndianBitConverter.ToUInt32(sector, 0x68);
                        info_mddf.mddf_block = BigEndianBitConverter.ToUInt32(sector, 0x6C);
                        info_mddf.volsize_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x70);
                        info_mddf.volsize_minus_mddf_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x74);
                        info_mddf.vol_size = BigEndianBitConverter.ToUInt32(sector, 0x78);
                        info_mddf.blocksize = BigEndianBitConverter.ToUInt16(sector, 0x7C);
                        info_mddf.datasize = BigEndianBitConverter.ToUInt16(sector, 0x7E);
                        info_mddf.unknown4 = BigEndianBitConverter.ToUInt16(sector, 0x80);
                        info_mddf.unknown5 = BigEndianBitConverter.ToUInt32(sector, 0x82);
                        info_mddf.unknown6 = BigEndianBitConverter.ToUInt32(sector, 0x86);
                        info_mddf.clustersize = BigEndianBitConverter.ToUInt16(sector, 0x8A);
                        info_mddf.fs_size = BigEndianBitConverter.ToUInt32(sector, 0x8C);
                        info_mddf.unknown7 = BigEndianBitConverter.ToUInt32(sector, 0x90);
                        info_mddf.srec_ptr = BigEndianBitConverter.ToUInt32(sector, 0x94);
                        info_mddf.unknown9 = BigEndianBitConverter.ToUInt16(sector, 0x98);
                        info_mddf.srec_len = BigEndianBitConverter.ToUInt16(sector, 0x9A);
                        info_mddf.unknown10 = BigEndianBitConverter.ToUInt32(sector, 0x9C);
                        info_mddf.unknown11 = BigEndianBitConverter.ToUInt32(sector, 0xA0);
                        info_mddf.unknown12 = BigEndianBitConverter.ToUInt32(sector, 0xA4);
                        info_mddf.unknown13 = BigEndianBitConverter.ToUInt32(sector, 0xA8);
                        info_mddf.unknown14 = BigEndianBitConverter.ToUInt32(sector, 0xAC);
                        info_mddf.filecount = BigEndianBitConverter.ToUInt16(sector, 0xB0);
                        info_mddf.unknown15 = BigEndianBitConverter.ToUInt32(sector, 0xB2);
                        info_mddf.unknown16 = BigEndianBitConverter.ToUInt32(sector, 0xB6);
                        info_mddf.freecount = BigEndianBitConverter.ToUInt32(sector, 0xBA);
                        info_mddf.unknown17 = BigEndianBitConverter.ToUInt16(sector, 0xBE);
                        info_mddf.unknown18 = BigEndianBitConverter.ToUInt32(sector, 0xC0);
                        info_mddf.overmount_stamp = BigEndianBitConverter.ToUInt64(sector, 0xC4);
                        info_mddf.serialization = BigEndianBitConverter.ToUInt32(sector, 0xCC);
                        info_mddf.unknown19 = BigEndianBitConverter.ToUInt32(sector, 0xD0);
                        info_mddf.unknown_timestamp = BigEndianBitConverter.ToUInt32(sector, 0xD4);
                        info_mddf.unknown20 = BigEndianBitConverter.ToUInt32(sector, 0xD8);
                        info_mddf.unknown21 = BigEndianBitConverter.ToUInt32(sector, 0xDC);
                        info_mddf.unknown22 = BigEndianBitConverter.ToUInt32(sector, 0xE0);
                        info_mddf.unknown23 = BigEndianBitConverter.ToUInt32(sector, 0xE4);
                        info_mddf.unknown24 = BigEndianBitConverter.ToUInt32(sector, 0xE8);
                        info_mddf.unknown25 = BigEndianBitConverter.ToUInt32(sector, 0xEC);
                        info_mddf.unknown26 = BigEndianBitConverter.ToUInt32(sector, 0xF0);
                        info_mddf.unknown27 = BigEndianBitConverter.ToUInt32(sector, 0xF4);
                        info_mddf.unknown28 = BigEndianBitConverter.ToUInt32(sector, 0xF8);
                        info_mddf.unknown29 = BigEndianBitConverter.ToUInt32(sector, 0xFC);
                        info_mddf.unknown30 = BigEndianBitConverter.ToUInt32(sector, 0x100);
                        info_mddf.unknown31 = BigEndianBitConverter.ToUInt32(sector, 0x104);
                        info_mddf.unknown32 = BigEndianBitConverter.ToUInt32(sector, 0x108);
                        info_mddf.unknown33 = BigEndianBitConverter.ToUInt32(sector, 0x10C);
                        info_mddf.unknown34 = BigEndianBitConverter.ToUInt32(sector, 0x110);
                        info_mddf.unknown35 = BigEndianBitConverter.ToUInt32(sector, 0x114);
                        info_mddf.backup_volid = BigEndianBitConverter.ToUInt64(sector, 0x118);
                        info_mddf.label_size = BigEndianBitConverter.ToUInt16(sector, 0x120);
                        info_mddf.fs_overhead = BigEndianBitConverter.ToUInt16(sector, 0x122);
                        info_mddf.result_scavenge = BigEndianBitConverter.ToUInt16(sector, 0x124);
                        info_mddf.boot_code = BigEndianBitConverter.ToUInt16(sector, 0x126);
                        info_mddf.boot_environ = BigEndianBitConverter.ToUInt16(sector, 0x6C);
                        info_mddf.unknown36 = BigEndianBitConverter.ToUInt32(sector, 0x12A);
                        info_mddf.unknown37 = BigEndianBitConverter.ToUInt32(sector, 0x12E);
                        info_mddf.unknown38 = BigEndianBitConverter.ToUInt32(sector, 0x132);
                        info_mddf.vol_sequence = BigEndianBitConverter.ToUInt16(sector, 0x136);
                        info_mddf.vol_left_mounted = sector[0x138];

                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown1 = 0x{0:X2} ({0})", info_mddf.unknown1);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown2 = 0x{0:X2} ({0})", info_mddf.unknown2);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown3 = 0x{0:X8} ({0})", info_mddf.unknown3);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown4 = 0x{0:X4} ({0})", info_mddf.unknown4);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown5 = 0x{0:X8} ({0})", info_mddf.unknown5);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown6 = 0x{0:X8} ({0})", info_mddf.unknown6);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown7 = 0x{0:X8} ({0})", info_mddf.unknown7);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown9 = 0x{0:X4} ({0})", info_mddf.unknown9);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown10 = 0x{0:X8} ({0})", info_mddf.unknown10);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown11 = 0x{0:X8} ({0})", info_mddf.unknown11);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown12 = 0x{0:X8} ({0})", info_mddf.unknown12);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown13 = 0x{0:X8} ({0})", info_mddf.unknown13);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown14 = 0x{0:X8} ({0})", info_mddf.unknown14);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown15 = 0x{0:X8} ({0})", info_mddf.unknown15);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown16 = 0x{0:X8} ({0})", info_mddf.unknown16);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown17 = 0x{0:X4} ({0})", info_mddf.unknown17);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown18 = 0x{0:X8} ({0})", info_mddf.unknown18);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown19 = 0x{0:X8} ({0})", info_mddf.unknown19);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown20 = 0x{0:X8} ({0})", info_mddf.unknown20);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown21 = 0x{0:X8} ({0})", info_mddf.unknown21);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown22 = 0x{0:X8} ({0})", info_mddf.unknown22);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown23 = 0x{0:X8} ({0})", info_mddf.unknown23);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown24 = 0x{0:X8} ({0})", info_mddf.unknown24);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown25 = 0x{0:X8} ({0})", info_mddf.unknown25);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown26 = 0x{0:X8} ({0})", info_mddf.unknown26);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown27 = 0x{0:X8} ({0})", info_mddf.unknown27);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown28 = 0x{0:X8} ({0})", info_mddf.unknown28);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown29 = 0x{0:X8} ({0})", info_mddf.unknown29);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown30 = 0x{0:X8} ({0})", info_mddf.unknown30);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown31 = 0x{0:X8} ({0})", info_mddf.unknown31);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown32 = 0x{0:X8} ({0})", info_mddf.unknown32);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown33 = 0x{0:X8} ({0})", info_mddf.unknown33);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown34 = 0x{0:X8} ({0})", info_mddf.unknown34);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown35 = 0x{0:X8} ({0})", info_mddf.unknown35);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown36 = 0x{0:X8} ({0})", info_mddf.unknown36);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown37 = 0x{0:X8} ({0})", info_mddf.unknown37);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown38 = 0x{0:X8} ({0})", info_mddf.unknown38);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown_timestamp = 0x{0:X8} ({0}, {1})", info_mddf.unknown_timestamp, DateHandlers.LisaToDateTime(info_mddf.unknown_timestamp));

                        if(info_mddf.mddf_block != i - before_mddf)
                            return;

                        if(info_mddf.vol_size > imagePlugin.GetSectors())
                            return;

                        if(info_mddf.vol_size - 1 != info_mddf.volsize_minus_one)
                            return;

                        if(info_mddf.vol_size - i - 1 != info_mddf.volsize_minus_mddf_minus_one - before_mddf)
                            return;

                        if(info_mddf.datasize > info_mddf.blocksize)
                            return;

                        if(info_mddf.blocksize < imagePlugin.GetSectorSize())
                            return;

                        if(info_mddf.datasize != imagePlugin.GetSectorSize())
                            return;

                        switch(info_mddf.fsversion)
                        {
                            case LisaFSv1:
                                sb.AppendLine("LisaFS v1");
                                break;
                            case LisaFSv2:
                                sb.AppendLine("LisaFS v2");
                                break;
                            case LisaFSv3:
                                sb.AppendLine("LisaFS v3");
                                break;
                            default:
                                sb.AppendFormat("Uknown LisaFS version {0}", info_mddf.fsversion).AppendLine();
                                break;
                        }

                        sb.AppendFormat("Volume name: \"{0}\"", info_mddf.volname).AppendLine();
                        sb.AppendFormat("Volume password: \"{0}\"", info_mddf.password).AppendLine();
                        sb.AppendFormat("Volume ID: 0x{0:X16}", info_mddf.volid).AppendLine();
                        sb.AppendFormat("Backup volume ID: 0x{0:X16}", info_mddf.backup_volid).AppendLine();

                        sb.AppendFormat("Master copy ID: 0x{0:X8}", info_mddf.master_copy_id).AppendLine();

                        sb.AppendFormat("Volume is number {0} of {1}", info_mddf.volnum, info_mddf.vol_sequence).AppendLine();

                        sb.AppendFormat("Serial number of Lisa computer that created this volume: {0}", info_mddf.machine_id).AppendLine();
                        sb.AppendFormat("Serial number of Lisa computer that can use this volume's software {0}", info_mddf.serialization).AppendLine();

                        sb.AppendFormat("Volume created on {0}", info_mddf.dtvc).AppendLine();
                        sb.AppendFormat("Some timestamp, says {0}", info_mddf.dtcc).AppendLine();
                        sb.AppendFormat("Volume backed up on {0}", info_mddf.dtvb).AppendLine();
                        sb.AppendFormat("Volume scavenged on {0}", info_mddf.dtvs).AppendLine();
                        sb.AppendFormat("MDDF is in block {0}", info_mddf.mddf_block + before_mddf).AppendLine();
                        sb.AppendFormat("There are {0} reserved blocks before volume", before_mddf).AppendLine();
                        sb.AppendFormat("{0} blocks minus one", info_mddf.volsize_minus_one).AppendLine();
                        sb.AppendFormat("{0} blocks minus one minus MDDF offset", info_mddf.volsize_minus_mddf_minus_one).AppendLine();
                        sb.AppendFormat("{0} blocks in volume", info_mddf.vol_size).AppendLine();
                        sb.AppendFormat("{0} bytes per sector (uncooked)", info_mddf.blocksize).AppendLine();
                        sb.AppendFormat("{0} bytes per sector", info_mddf.datasize).AppendLine();
                        sb.AppendFormat("{0} blocks per cluster", info_mddf.clustersize).AppendLine();
                        sb.AppendFormat("{0} blocks in filesystem", info_mddf.fs_size).AppendLine();
                        sb.AppendFormat("{0} files in volume", info_mddf.filecount).AppendLine();
                        sb.AppendFormat("{0} blocks free", info_mddf.freecount).AppendLine();
                        sb.AppendFormat("{0} bytes in LisaInfo", info_mddf.label_size).AppendLine();
                        sb.AppendFormat("Filesystem overhead: {0}", info_mddf.fs_overhead).AppendLine();
                        sb.AppendFormat("Scanvenger result code: 0x{0:X8}", info_mddf.result_scavenge).AppendLine();
                        sb.AppendFormat("Boot code: 0x{0:X8}", info_mddf.boot_code).AppendLine();
                        sb.AppendFormat("Boot environment:  0x{0:X8}", info_mddf.boot_environ).AppendLine();
                        sb.AppendFormat("Overmount stamp: 0x{0:X16}", info_mddf.overmount_stamp).AppendLine();
                        sb.AppendFormat("S-Records start at {0} and spans for {1} blocks", info_mddf.srec_ptr + info_mddf.mddf_block + before_mddf, info_mddf.srec_len).AppendLine();

                        if(info_mddf.vol_left_mounted == 0)
                            sb.AppendLine("Volume is clean");
                        else
                            sb.AppendLine("Volume is dirty");

                        information = sb.ToString();

                        xmlFSType = new Schemas.FileSystemType();
                        if(DateTime.Compare(info_mddf.dtvb, DateHandlers.LisaToDateTime(0)) > 0)
                        {
                            xmlFSType.BackupDate = info_mddf.dtvb;
                            xmlFSType.BackupDateSpecified = true;
                        }
                        xmlFSType.Clusters = info_mddf.vol_size;
                        xmlFSType.ClusterSize = info_mddf.clustersize * info_mddf.datasize;
                        if(DateTime.Compare(info_mddf.dtvc, DateHandlers.LisaToDateTime(0)) > 0)
                        {
                            xmlFSType.CreationDate = info_mddf.dtvc;
                            xmlFSType.CreationDateSpecified = true;
                        }
                        xmlFSType.Dirty = info_mddf.vol_left_mounted != 0;
                        xmlFSType.Files = info_mddf.filecount;
                        xmlFSType.FilesSpecified = true;
                        xmlFSType.FreeClusters = info_mddf.freecount;
                        xmlFSType.FreeClustersSpecified = true;
                        xmlFSType.Type = "LisaFS";
                        xmlFSType.VolumeName = info_mddf.volname;
                        xmlFSType.VolumeSerial = string.Format("{0:X16}", info_mddf.volid);

                        return;
                    }
                }

                return;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception {0}, {1}, {2}", ex.Message, ex.InnerException, ex.StackTrace);
                return;
            }
        }
    }
}

