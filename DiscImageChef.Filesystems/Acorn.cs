// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Acorn.cs
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
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems
{
    class AcornADFS : Filesystem
    {
        const ulong ADFS_SB_POS = 0xC00;

        public AcornADFS()
        {
            Name = "Acorn Advanced Disc Filing System";
            PluginUUID = new Guid("BAFC1E50-9C64-4CD3-8400-80628CC27AFA");
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DiscRecord
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1C0)]
            public byte[] spare;
            public byte log2secsize;
            public byte spt;
            public byte heads;
            public byte density;
            public byte idlen;
            public byte log2bpmb;
            public byte skew;
            public byte bootoption;
            public byte lowsector;
            public byte nzones;
            public ushort zone_spare;
            public uint root;
            public uint disc_size;
            public ushort disc_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] disc_name;
            public uint disc_type;
            public uint disc_size_high;
            public byte flags;
            public byte nzones_high;
            public uint format_version;
            public uint root_size;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] reserved;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if(partitionStart >= imagePlugin.GetSectors())
                return false;

            ulong sbSector;

            if(imagePlugin.GetSectorSize() > ADFS_SB_POS)
                sbSector = 0;
            else
                sbSector = ADFS_SB_POS / imagePlugin.GetSectorSize();

            byte[] sector = imagePlugin.ReadSector(sbSector + partitionStart);
            DiscRecord drSb;

            try
            {
                GCHandle handle = GCHandle.Alloc(sector, GCHandleType.Pinned);
                drSb = (DiscRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DiscRecord));
                handle.Free();
            }
            catch
            {
                return false;
            }

            if(drSb.log2secsize < 8 || drSb.log2secsize > 10)
                return false;

            if(drSb.idlen < (drSb.log2secsize + 3) || drSb.idlen > 19)
                return false;

            if((drSb.disc_size_high >> drSb.log2secsize) != 0)
                return false;

            if(!ArrayHelpers.ArrayIsNullOrEmpty(drSb.reserved))
                return false;

            ulong bytes = drSb.disc_size_high;
            bytes *= 0x100000000;
            bytes += drSb.disc_size;

            if(bytes > (imagePlugin.GetSectors() * (ulong)imagePlugin.GetSectorSize()))
                return false;

            return true;
        }
        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            StringBuilder sbInformation = new StringBuilder();
            xmlFSType = new Schemas.FileSystemType();
            information = "";

            ulong sbSector;

            if(imagePlugin.GetSectorSize() > ADFS_SB_POS)
                sbSector = 0;
            else
                sbSector = ADFS_SB_POS / imagePlugin.GetSectorSize();

            byte[] sector = imagePlugin.ReadSector(sbSector + partitionStart);

            GCHandle handle = GCHandle.Alloc(sector, GCHandleType.Pinned);
            DiscRecord drSb = (DiscRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DiscRecord));
            handle.Free();

            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.log2secsize = {0}", drSb.log2secsize);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.spt = {0}", drSb.spt);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.heads = {0}", drSb.heads);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.density = {0}", drSb.density);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.idlen = {0}", drSb.idlen);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.log2bpmb = {0}", drSb.log2bpmb);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.skew = {0}", drSb.skew);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.bootoption = {0}", drSb.bootoption);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.lowsector = {0}", drSb.lowsector);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.nzones = {0}", drSb.nzones);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.zone_spare = {0}", drSb.zone_spare);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.root = {0}", drSb.root);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_size = {0}", drSb.disc_size);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_id = {0}", drSb.disc_id);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_name = {0}", StringHandlers.CToString(drSb.disc_name));
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_type = {0}", drSb.disc_type);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.disc_size_high = {0}", drSb.disc_size_high);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.flags = {0}", drSb.flags);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.nzones_high = {0}", drSb.nzones_high);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.format_version = {0}", drSb.format_version);
            DicConsole.DebugWriteLine("ADFS Plugin", "drSb.root_size = {0}", drSb.root_size);

            if(drSb.log2secsize < 8 || drSb.log2secsize > 10)
                return;

            if(drSb.idlen < (drSb.log2secsize + 3) || drSb.idlen > 19)
                return;

            if((drSb.disc_size_high >> drSb.log2secsize) != 0)
                return;

            if(!ArrayHelpers.ArrayIsNullOrEmpty(drSb.reserved))
                return;

            ulong bytes = drSb.disc_size_high;
            bytes *= 0x100000000;
            bytes += drSb.disc_size;

            ulong zones = drSb.nzones_high;
            zones *= 0x100000000;
            zones += drSb.nzones;

            if(bytes > (imagePlugin.GetSectors() * (ulong)imagePlugin.GetSectorSize()))
                return;

            string discname = StringHandlers.CToString(drSb.disc_name);

            sbInformation.AppendLine("Acorn Advanced Disc Filing System");
            sbInformation.AppendLine();
            sbInformation.AppendFormat("Version {0}", drSb.format_version).AppendLine();
            sbInformation.AppendFormat("{0} bytes per sector", 1 << drSb.log2secsize).AppendLine();
            sbInformation.AppendFormat("{0} sectors per track", drSb.spt).AppendLine();
            sbInformation.AppendFormat("{0} heads", drSb.heads).AppendLine();
            sbInformation.AppendFormat("Density code: {0}", drSb.density).AppendLine();
            sbInformation.AppendFormat("Skew: {0}", drSb.skew).AppendLine();
            sbInformation.AppendFormat("Boot option: {0}", drSb.bootoption).AppendLine();
            sbInformation.AppendFormat("Root starts at byte {0}", drSb.root).AppendLine();
            sbInformation.AppendFormat("Root is {0} bytes long", drSb.root_size).AppendLine();
            sbInformation.AppendFormat("Volume has {0} bytes in {1} zones", bytes, zones).AppendLine();
            sbInformation.AppendFormat("Volume flags: 0x{0:X4}", drSb.flags).AppendLine();
            sbInformation.AppendFormat("Volume ID: {0}", drSb.disc_id).AppendLine();
            sbInformation.AppendFormat("Volume name: {0}", discname).AppendLine();

            information = sbInformation.ToString();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Bootable |= drSb.bootoption != 0; // Or not?
            xmlFSType.Clusters = (long)(bytes / (ulong)(1 << drSb.log2secsize));
            xmlFSType.ClusterSize = (1 << drSb.log2secsize);
            xmlFSType.Type = "Acorn Advanced Disc Filing System";
            xmlFSType.VolumeName = discname;
            xmlFSType.VolumeSerial = string.Format("{0}", drSb.disc_id);
        }
    }
}

