// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
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
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.ImagePlugins;
using System.CodeDom.Compiler;

namespace DiscImageChef.Filesystems.LisaFS
{
    partial class LisaFS : Filesystem
    {
        public override Errno Mount()
        {
            return Mount(false);
        }

        public override Errno Mount(bool debug)
        {
            try
            {
                if(device.ImageInfo.readableSectorTags == null ||
                   !device.ImageInfo.readableSectorTags.Contains(SectorTagType.AppleSectorTag))
                {
                    DicConsole.DebugWriteLine("LisaFS plugin", "Underlying device does not support Lisa tags");
                    return Errno.InOutError;
                }

                // LisaOS is big-endian
                BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

                // Minimal LisaOS disk is 3.5" single sided double density, 800 sectors
                if(device.GetSectors() < 800)
                {
                    DicConsole.DebugWriteLine("LisaFS plugin", "Device is too small");
                    return Errno.InOutError;
                }

                // LisaOS searches sectors until tag tells MDDF resides there, so we'll search 100 sectors
                for(int i = 0; i < 100; i++)
                {
                    byte[] tag = device.ReadSectorTag((ulong)i, SectorTagType.AppleSectorTag);
                    UInt16 fileid = BigEndianBitConverter.ToUInt16(tag, 0x04);

                    DicConsole.DebugWriteLine("LisaFS plugin", "Sector {0}, file ID 0x{1:X4}", i, fileid);

                    if(fileid == FILEID_MDDF)
                    {
                        byte[] sector = device.ReadSector((ulong)i);
                        mddf = new MDDF();
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
                        mddf.unknown8 = BigEndianBitConverter.ToUInt32(sector, 0x94);
                        mddf.unknown9 = BigEndianBitConverter.ToUInt32(sector, 0x98);
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

                        if(mddf.mddf_block != i ||
                            mddf.vol_size > device.GetSectors() ||
                            mddf.vol_size - 1 != mddf.volsize_minus_one ||
                            mddf.vol_size - i - 1 != mddf.volsize_minus_mddf_minus_one ||
                            mddf.datasize > mddf.blocksize ||
                            mddf.blocksize < device.GetSectorSize() ||
                            mddf.datasize != device.GetSectorSize())
                        {
                            DicConsole.DebugWriteLine("LisaFS plugin", "Incorrect MDDF found");
                            return Errno.InvalidArgument;
                        }

                        if(mddf.fsversion != LisaFSv3)
                        {
                            string version = mddf.fsversion.ToString();

                            switch(mddf.fsversion)
                            {
                                case LisaFSv1:
                                    version = "v1";
                                    break;
                                case LisaFSv2:
                                    version = "v2";
                                    break;
                            }

                            DicConsole.DebugWriteLine("LisaFS plugin", "Cannot mount LisaFS version {0}", version);
                            return Errno.NotSupported;
                        }

                        extentCache = new Dictionary<short, ExtentFile>();
                        systemFileCache = new Dictionary<short, byte[]>();
                        fileCache = new Dictionary<short, byte[]>();
                        catalogCache = new Dictionary<short, List<CatalogEntry>>();
                        fileSizeCache = new Dictionary<short, int>();

                        Errno error;

                        mounted = true;

                        List<CatalogEntry> tempCat;
                        error = ReadCatalog((short)FILEID_DIRECTORY, out tempCat);

                        if(error != Errno.NoError)
                        {
                            DicConsole.DebugWriteLine("LisaFS plugin", "Cannot read root catalog, error {0}", error.ToString());
                            mounted = false;
                            return error;
                        }

                        this.debug = debug;

                        if(debug)
                        {
                            byte[] temp;

                            error = ReadSystemFile(FILEID_BOOT_SIGNED, out temp);
                            if(error != Errno.NoError)
                            {
                                DicConsole.DebugWriteLine("LisaFS plugin", "Unable to read boot blocks");
                                mounted = false;
                                return error;
                            }

                            error = ReadSystemFile(FILEID_LOADER_SIGNED, out temp);
                            if(error != Errno.NoError)
                            {
                                DicConsole.DebugWriteLine("LisaFS plugin", "Unable to read boot loader");
                                mounted = false;
                                return error;
                            }

                            error = ReadSystemFile((short)FILEID_MDDF, out temp);
                            if(error != Errno.NoError)
                            {
                                DicConsole.DebugWriteLine("LisaFS plugin", "Unable to read MDDF");
                                mounted = false;
                                return error;
                            }

                            error = ReadSystemFile((short)FILEID_BITMAP, out temp);
                            if(error != Errno.NoError)
                            {
                                DicConsole.DebugWriteLine("LisaFS plugin", "Unable to read volume bitmap");
                                mounted = false;
                                return error;
                            }

                            error = ReadSystemFile((short)FILEID_SRECORD, out temp);
                            if(error != Errno.NoError)
                            {
                                DicConsole.DebugWriteLine("LisaFS plugin", "Unable to read S-Records file");
                                mounted = false;
                                return error;
                            }
                        }

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

                        return Errno.NoError;
                    }
                }

                DicConsole.DebugWriteLine("LisaFS plugin", "Not a Lisa filesystem");
                return Errno.NotSupported;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception {0}, {1}, {2}", ex.Message, ex.InnerException, ex.StackTrace);
                return Errno.InOutError;
            }
        }

        public override Errno Unmount()
        {
            mounted = false;
            extentCache = null;
            systemFileCache = null;
            fileCache = null;
            catalogCache = null;
            fileSizeCache = null;

            return Errno.NoError;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            if(!mounted)
                return Errno.AccessDenied;

            stat = new FileSystemInfo();
            stat.Blocks = mddf.vol_size;
            stat.FilenameLength = (ushort)E_NAME;
            stat.Files = mddf.filecount;
            stat.FreeBlocks = mddf.freecount;
            stat.FreeFiles = FILEID_MAX - stat.Files;
            stat.Id.Serial64 = mddf.volid;
            stat.Id.IsLong = true;
            stat.PluginId = PluginUUID;
            stat.Type = "LisaFS v3";

            return Errno.NoError;
        }
    }
}

