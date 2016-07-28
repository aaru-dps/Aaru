// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Text;
using DiscImageChef.Console;
using DiscImageChef.ImagePlugins;
using System.Runtime.InteropServices;

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
                    Tag searchTag;
                    DecodeTag(imagePlugin.ReadSectorTag((ulong)i, SectorTagType.AppleSectorTag), out searchTag);

                    DicConsole.DebugWriteLine("LisaFS plugin", "Sector {0}, file ID 0x{1:X4}", i, searchTag.fileID);

                    if(before_mddf == -1 && searchTag.fileID == FILEID_LOADER_SIGNED)
                        before_mddf = i - 1;

                    if(searchTag.fileID == FILEID_MDDF)
                    {
                        byte[] sector = imagePlugin.ReadSector((ulong)i);
                        MDDF mddf = new MDDF();

                        mddf.mddf_block = BigEndianBitConverter.ToUInt32(sector, 0x6C);
                        mddf.volsize_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x70);
                        mddf.volsize_minus_mddf_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x74);
                        mddf.vol_size = BigEndianBitConverter.ToUInt32(sector, 0x78);
                        mddf.blocksize = BigEndianBitConverter.ToUInt16(sector, 0x7C);
                        mddf.datasize = BigEndianBitConverter.ToUInt16(sector, 0x7E);

                        DicConsole.DebugWriteLine("LisaFS plugin", "Current sector = {0}", i);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.mddf_block = {0}", mddf.mddf_block);
                        DicConsole.DebugWriteLine("LisaFS plugin", "Disk size = {0} sectors", imagePlugin.GetSectors());
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.vol_size = {0} sectors", mddf.vol_size);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.vol_size - 1 = {0}", mddf.volsize_minus_one);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.vol_size - mddf.mddf_block -1 = {0}", mddf.volsize_minus_mddf_minus_one);
                        DicConsole.DebugWriteLine("LisaFS plugin", "Disk sector = {0} bytes", imagePlugin.GetSectorSize());
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.blocksize = {0} bytes", mddf.blocksize);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.datasize = {0} bytes", mddf.datasize);

                        if(mddf.mddf_block != i - before_mddf)
                            return false;

                        if(mddf.vol_size > imagePlugin.GetSectors())
                            return false;

                        if(mddf.vol_size - 1 != mddf.volsize_minus_one)
                            return false;

                        if(mddf.vol_size - i - 1 != mddf.volsize_minus_mddf_minus_one - before_mddf)
                            return false;

                        if(mddf.datasize > mddf.blocksize)
                            return false;

                        if(mddf.blocksize < imagePlugin.GetSectorSize())
                            return false;

                        if(mddf.datasize != imagePlugin.GetSectorSize())
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
                    Tag searchTag;
                    DecodeTag(imagePlugin.ReadSectorTag((ulong)i, SectorTagType.AppleSectorTag), out searchTag);

                    DicConsole.DebugWriteLine("LisaFS plugin", "Sector {0}, file ID 0x{1:X4}", i, searchTag.fileID);

                    if(before_mddf == -1 && searchTag.fileID == FILEID_LOADER_SIGNED)
                        before_mddf = i - 1;

                    if(searchTag.fileID == FILEID_MDDF)
                    {
                        byte[] sector = imagePlugin.ReadSector((ulong)i);
                        MDDF mddf = new MDDF();
                        byte[] pString = new byte[33];
                        UInt32 lisa_time;

                        mddf.fsversion = BigEndianBitConverter.ToUInt16(sector, 0x00);
                        mddf.volid = BigEndianBitConverter.ToUInt64(sector, 0x02);
                        mddf.volnum = BigEndianBitConverter.ToUInt16(sector, 0x0A);
                        Array.Copy(sector, 0x0C, pString, 0, 33);
                        mddf.volname = StringHandlers.PascalToString(pString);
                        mddf.unknown1 = sector[0x2D];
                        Array.Copy(sector, 0x2E, pString, 0, 33);
                        // Prevent garbage
                        if(pString[0] <= 32)
                            mddf.password = StringHandlers.PascalToString(pString);
                        else
                            mddf.password = "";
                        mddf.unknown2 = sector[0x4F];
                        mddf.machine_id = BigEndianBitConverter.ToUInt32(sector, 0x50);
                        mddf.master_copy_id = BigEndianBitConverter.ToUInt32(sector, 0x54);
                        lisa_time = BigEndianBitConverter.ToUInt32(sector, 0x58);
                        mddf.dtvc = DateHandlers.LisaToDateTime(lisa_time);
                        lisa_time = BigEndianBitConverter.ToUInt32(sector, 0x5C);
                        mddf.dtcc = DateHandlers.LisaToDateTime(lisa_time);
                        lisa_time = BigEndianBitConverter.ToUInt32(sector, 0x60);
                        mddf.dtvb = DateHandlers.LisaToDateTime(lisa_time);
                        lisa_time = BigEndianBitConverter.ToUInt32(sector, 0x64);
                        mddf.dtvs = DateHandlers.LisaToDateTime(lisa_time);
                        mddf.unknown3 = BigEndianBitConverter.ToUInt32(sector, 0x68);
                        mddf.mddf_block = BigEndianBitConverter.ToUInt32(sector, 0x6C);
                        mddf.volsize_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x70);
                        mddf.volsize_minus_mddf_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x74);
                        mddf.vol_size = BigEndianBitConverter.ToUInt32(sector, 0x78);
                        mddf.blocksize = BigEndianBitConverter.ToUInt16(sector, 0x7C);
                        mddf.datasize = BigEndianBitConverter.ToUInt16(sector, 0x7E);
                        mddf.unknown4 = BigEndianBitConverter.ToUInt16(sector, 0x80);
                        mddf.unknown5 = BigEndianBitConverter.ToUInt32(sector, 0x82);
                        mddf.unknown6 = BigEndianBitConverter.ToUInt32(sector, 0x86);
                        mddf.clustersize = BigEndianBitConverter.ToUInt16(sector, 0x8A);
                        mddf.fs_size = BigEndianBitConverter.ToUInt32(sector, 0x8C);
                        mddf.unknown7 = BigEndianBitConverter.ToUInt32(sector, 0x90);
                        mddf.srec_ptr = BigEndianBitConverter.ToUInt32(sector, 0x94);
                        mddf.unknown9 = BigEndianBitConverter.ToUInt16(sector, 0x98);
                        mddf.srec_len = BigEndianBitConverter.ToUInt16(sector, 0x9A);
                        mddf.unknown10 = BigEndianBitConverter.ToUInt32(sector, 0x9C);
                        mddf.unknown11 = BigEndianBitConverter.ToUInt32(sector, 0xA0);
                        mddf.unknown12 = BigEndianBitConverter.ToUInt32(sector, 0xA4);
                        mddf.unknown13 = BigEndianBitConverter.ToUInt32(sector, 0xA8);
                        mddf.unknown14 = BigEndianBitConverter.ToUInt32(sector, 0xAC);
                        mddf.filecount = BigEndianBitConverter.ToUInt16(sector, 0xB0);
                        mddf.unknown15 = BigEndianBitConverter.ToUInt32(sector, 0xB2);
                        mddf.unknown16 = BigEndianBitConverter.ToUInt32(sector, 0xB6);
                        mddf.freecount = BigEndianBitConverter.ToUInt32(sector, 0xBA);
                        mddf.unknown17 = BigEndianBitConverter.ToUInt16(sector, 0xBE);
                        mddf.unknown18 = BigEndianBitConverter.ToUInt32(sector, 0xC0);
                        mddf.overmount_stamp = BigEndianBitConverter.ToUInt64(sector, 0xC4);
                        mddf.serialization = BigEndianBitConverter.ToUInt32(sector, 0xCC);
                        mddf.unknown19 = BigEndianBitConverter.ToUInt32(sector, 0xD0);
                        mddf.unknown_timestamp = BigEndianBitConverter.ToUInt32(sector, 0xD4);
                        mddf.unknown20 = BigEndianBitConverter.ToUInt32(sector, 0xD8);
                        mddf.unknown21 = BigEndianBitConverter.ToUInt32(sector, 0xDC);
                        mddf.unknown22 = BigEndianBitConverter.ToUInt32(sector, 0xE0);
                        mddf.unknown23 = BigEndianBitConverter.ToUInt32(sector, 0xE4);
                        mddf.unknown24 = BigEndianBitConverter.ToUInt32(sector, 0xE8);
                        mddf.unknown25 = BigEndianBitConverter.ToUInt32(sector, 0xEC);
                        mddf.unknown26 = BigEndianBitConverter.ToUInt32(sector, 0xF0);
                        mddf.unknown27 = BigEndianBitConverter.ToUInt32(sector, 0xF4);
                        mddf.unknown28 = BigEndianBitConverter.ToUInt32(sector, 0xF8);
                        mddf.unknown29 = BigEndianBitConverter.ToUInt32(sector, 0xFC);
                        mddf.unknown30 = BigEndianBitConverter.ToUInt32(sector, 0x100);
                        mddf.unknown31 = BigEndianBitConverter.ToUInt32(sector, 0x104);
                        mddf.unknown32 = BigEndianBitConverter.ToUInt32(sector, 0x108);
                        mddf.unknown33 = BigEndianBitConverter.ToUInt32(sector, 0x10C);
                        mddf.unknown34 = BigEndianBitConverter.ToUInt32(sector, 0x110);
                        mddf.unknown35 = BigEndianBitConverter.ToUInt32(sector, 0x114);
                        mddf.backup_volid = BigEndianBitConverter.ToUInt64(sector, 0x118);
                        mddf.label_size = BigEndianBitConverter.ToUInt16(sector, 0x120);
                        mddf.fs_overhead = BigEndianBitConverter.ToUInt16(sector, 0x122);
                        mddf.result_scavenge = BigEndianBitConverter.ToUInt16(sector, 0x124);
                        mddf.boot_code = BigEndianBitConverter.ToUInt16(sector, 0x126);
                        mddf.boot_environ = BigEndianBitConverter.ToUInt16(sector, 0x6C);
                        mddf.unknown36 = BigEndianBitConverter.ToUInt32(sector, 0x12A);
                        mddf.unknown37 = BigEndianBitConverter.ToUInt32(sector, 0x12E);
                        mddf.unknown38 = BigEndianBitConverter.ToUInt32(sector, 0x132);
                        mddf.vol_sequence = BigEndianBitConverter.ToUInt16(sector, 0x136);
                        mddf.vol_left_mounted = sector[0x138];

                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown1 = 0x{0:X2} ({0})", mddf.unknown1);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown2 = 0x{0:X2} ({0})", mddf.unknown2);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown3 = 0x{0:X8} ({0})", mddf.unknown3);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown4 = 0x{0:X4} ({0})", mddf.unknown4);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown5 = 0x{0:X8} ({0})", mddf.unknown5);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown6 = 0x{0:X8} ({0})", mddf.unknown6);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown7 = 0x{0:X8} ({0})", mddf.unknown7);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown9 = 0x{0:X4} ({0})", mddf.unknown9);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown10 = 0x{0:X8} ({0})", mddf.unknown10);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown11 = 0x{0:X8} ({0})", mddf.unknown11);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown12 = 0x{0:X8} ({0})", mddf.unknown12);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown13 = 0x{0:X8} ({0})", mddf.unknown13);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown14 = 0x{0:X8} ({0})", mddf.unknown14);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown15 = 0x{0:X8} ({0})", mddf.unknown15);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown16 = 0x{0:X8} ({0})", mddf.unknown16);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown17 = 0x{0:X4} ({0})", mddf.unknown17);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown18 = 0x{0:X8} ({0})", mddf.unknown18);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown19 = 0x{0:X8} ({0})", mddf.unknown19);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown20 = 0x{0:X8} ({0})", mddf.unknown20);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown21 = 0x{0:X8} ({0})", mddf.unknown21);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown22 = 0x{0:X8} ({0})", mddf.unknown22);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown23 = 0x{0:X8} ({0})", mddf.unknown23);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown24 = 0x{0:X8} ({0})", mddf.unknown24);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown25 = 0x{0:X8} ({0})", mddf.unknown25);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown26 = 0x{0:X8} ({0})", mddf.unknown26);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown27 = 0x{0:X8} ({0})", mddf.unknown27);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown28 = 0x{0:X8} ({0})", mddf.unknown28);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown29 = 0x{0:X8} ({0})", mddf.unknown29);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown30 = 0x{0:X8} ({0})", mddf.unknown30);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown31 = 0x{0:X8} ({0})", mddf.unknown31);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown32 = 0x{0:X8} ({0})", mddf.unknown32);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown33 = 0x{0:X8} ({0})", mddf.unknown33);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown34 = 0x{0:X8} ({0})", mddf.unknown34);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown35 = 0x{0:X8} ({0})", mddf.unknown35);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown36 = 0x{0:X8} ({0})", mddf.unknown36);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown37 = 0x{0:X8} ({0})", mddf.unknown37);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown38 = 0x{0:X8} ({0})", mddf.unknown38);
                        DicConsole.DebugWriteLine("LisaFS plugin", "mddf.unknown_timestamp = 0x{0:X8} ({0}, {1})", mddf.unknown_timestamp, DateHandlers.LisaToDateTime(mddf.unknown_timestamp));

                        if(mddf.mddf_block != i - before_mddf)
                            return;

                        if(mddf.vol_size > imagePlugin.GetSectors())
                            return;

                        if(mddf.vol_size - 1 != mddf.volsize_minus_one)
                            return;

                        if(mddf.vol_size - i - 1 != mddf.volsize_minus_mddf_minus_one - before_mddf)
                            return;

                        if(mddf.datasize > mddf.blocksize)
                            return;

                        if(mddf.blocksize < imagePlugin.GetSectorSize())
                            return;

                        if(mddf.datasize != imagePlugin.GetSectorSize())
                            return;

                        switch(mddf.fsversion)
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
                                sb.AppendFormat("Uknown LisaFS version {0}", mddf.fsversion).AppendLine();
                                break;
                        }

                        sb.AppendFormat("Volume name: \"{0}\"", mddf.volname).AppendLine();
                        sb.AppendFormat("Volume password: \"{0}\"", mddf.password).AppendLine();
                        sb.AppendFormat("Volume ID: 0x{0:X16}", mddf.volid).AppendLine();
                        sb.AppendFormat("Backup volume ID: 0x{0:X16}", mddf.backup_volid).AppendLine();

                        sb.AppendFormat("Master copy ID: 0x{0:X8}", mddf.master_copy_id).AppendLine();

                        sb.AppendFormat("Volume is number {0} of {1}", mddf.volnum, mddf.vol_sequence).AppendLine();

                        sb.AppendFormat("Serial number of Lisa computer that created this volume: {0}", mddf.machine_id).AppendLine();
                        sb.AppendFormat("Serial number of Lisa computer that can use this volume's software {0}", mddf.serialization).AppendLine();

                        sb.AppendFormat("Volume created on {0}", mddf.dtvc).AppendLine();
                        sb.AppendFormat("Some timestamp, says {0}", mddf.dtcc).AppendLine();
                        sb.AppendFormat("Volume backed up on {0}", mddf.dtvb).AppendLine();
                        sb.AppendFormat("Volume scavenged on {0}", mddf.dtvs).AppendLine();
                        sb.AppendFormat("MDDF is in block {0}", mddf.mddf_block + before_mddf).AppendLine();
                        sb.AppendFormat("There are {0} reserved blocks before volume", before_mddf).AppendLine();
                        sb.AppendFormat("{0} blocks minus one", mddf.volsize_minus_one).AppendLine();
                        sb.AppendFormat("{0} blocks minus one minus MDDF offset", mddf.volsize_minus_mddf_minus_one).AppendLine();
                        sb.AppendFormat("{0} blocks in volume", mddf.vol_size).AppendLine();
                        sb.AppendFormat("{0} bytes per sector (uncooked)", mddf.blocksize).AppendLine();
                        sb.AppendFormat("{0} bytes per sector", mddf.datasize).AppendLine();
                        sb.AppendFormat("{0} blocks per cluster", mddf.clustersize).AppendLine();
                        sb.AppendFormat("{0} blocks in filesystem", mddf.fs_size).AppendLine();
                        sb.AppendFormat("{0} files in volume", mddf.filecount).AppendLine();
                        sb.AppendFormat("{0} blocks free", mddf.freecount).AppendLine();
                        sb.AppendFormat("{0} bytes in LisaInfo", mddf.label_size).AppendLine();
                        sb.AppendFormat("Filesystem overhead: {0}", mddf.fs_overhead).AppendLine();
                        sb.AppendFormat("Scanvenger result code: 0x{0:X8}", mddf.result_scavenge).AppendLine();
                        sb.AppendFormat("Boot code: 0x{0:X8}", mddf.boot_code).AppendLine();
                        sb.AppendFormat("Boot environment:  0x{0:X8}", mddf.boot_environ).AppendLine();
                        sb.AppendFormat("Overmount stamp: 0x{0:X16}", mddf.overmount_stamp).AppendLine();
                        sb.AppendFormat("S-Records start at {0} and spans for {1} blocks", mddf.srec_ptr + mddf.mddf_block + before_mddf, mddf.srec_len).AppendLine();

                        if(mddf.vol_left_mounted == 0)
                            sb.AppendLine("Volume is clean");
                        else
                            sb.AppendLine("Volume is dirty");

                        information = sb.ToString();

                        xmlFSType = new Schemas.FileSystemType();
                        if(DateTime.Compare(mddf.dtvb, DateHandlers.LisaToDateTime(0)) > 0)
                        {
                            xmlFSType.BackupDate = mddf.dtvb;
                            xmlFSType.BackupDateSpecified = true;
                        }
                        xmlFSType.Clusters = mddf.vol_size;
                        xmlFSType.ClusterSize = mddf.clustersize * mddf.datasize;
                        if(DateTime.Compare(mddf.dtvc, DateHandlers.LisaToDateTime(0)) > 0)
                        {
                            xmlFSType.CreationDate = mddf.dtvc;
                            xmlFSType.CreationDateSpecified = true;
                        }
                        xmlFSType.Dirty = mddf.vol_left_mounted != 0;
                        xmlFSType.Files = mddf.filecount;
                        xmlFSType.FilesSpecified = true;
                        xmlFSType.FreeClusters = mddf.freecount;
                        xmlFSType.FreeClustersSpecified = true;
                        xmlFSType.Type = "LisaFS";
                        xmlFSType.VolumeName = mddf.volname;
                        xmlFSType.VolumeSerial = String.Format("{0:X16}", mddf.volid);

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

