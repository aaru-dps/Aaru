// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ECMA67.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ECMA-67 plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the ECMA-67 file system and shows information.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.Filesystems
{
    class ECMA67 : Filesystem
    {
        public ECMA67()
        {
            Name = "ECMA-67";
            PluginUUID = new Guid("62A2D44A-CBC1-4377-B4B6-28C5C92034A1");
        }

        public ECMA67(ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            Name = "ECMA-67";
            PluginUUID = new Guid("62A2D44A-CBC1-4377-B4B6-28C5C92034A1");
        }

        readonly byte[] ECMA67_Magic = { 0x56, 0x4F, 0x4C };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VolumeLabel
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] labelIdentifier;
            public byte labelNumber;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] volumeIdentifier;
            public byte volumeAccessibility;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public byte[] reserved1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public byte[] owner;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] reserved2;
            public byte surface;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] reserved3;
            public byte recordLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] reserved4;
            public byte fileLabelAllocation;
            public byte labelStandardVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] reserved5;
        }

        public override bool Identify(ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if(partitionStart > 0)
                return false;

            if(partitionEnd < 8)
                return false;

            byte[] sector = imagePlugin.ReadSector(6);

            if(sector.Length != 128)
                return false;

            VolumeLabel vol = new VolumeLabel();
            IntPtr volPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vol));
            Marshal.Copy(sector, 0, volPtr, Marshal.SizeOf(vol));
            vol = (VolumeLabel)Marshal.PtrToStructure(volPtr, typeof(VolumeLabel));
            Marshal.FreeHGlobal(volPtr);

            return ECMA67_Magic.SequenceEqual(vol.labelIdentifier) && vol.labelNumber == 1 && vol.recordLength == 0x31;
        }

        public override void GetInformation(ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            byte[] sector = imagePlugin.ReadSector(6);

            StringBuilder sbInformation = new StringBuilder();

            VolumeLabel vol = new VolumeLabel();
            IntPtr volPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vol));
            Marshal.Copy(sector, 0, volPtr, Marshal.SizeOf(vol));
            vol = (VolumeLabel)Marshal.PtrToStructure(volPtr, typeof(VolumeLabel));
            Marshal.FreeHGlobal(volPtr);

            sbInformation.AppendLine("ECMA-67");

            sbInformation.AppendFormat("Volume name: {0}", Encoding.ASCII.GetString(vol.volumeIdentifier)).AppendLine();
            sbInformation.AppendFormat("Volume owner: {0}", Encoding.ASCII.GetString(vol.owner)).AppendLine();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Type = "ECMA-67";
            xmlFSType.ClusterSize = 256;
            xmlFSType.Clusters = (long)(partitionEnd - partitionStart + 1);
            xmlFSType.VolumeName = Encoding.ASCII.GetString(vol.volumeIdentifier);

            information = sbInformation.ToString();
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}