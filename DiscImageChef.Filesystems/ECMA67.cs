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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    public class ECMA67 : IFilesystem
    {
        readonly byte[] ECMA67_Magic = {0x56, 0x4F, 0x4C};

        Encoding currentEncoding;
        public virtual Encoding Encoding => currentEncoding;
        public virtual string Name => "ECMA-67";
        public virtual Guid Id => new Guid("62A2D44A-CBC1-4377-B4B6-28C5C92034A1");
        FileSystemType xmlFsType;
        public virtual FileSystemType XmlFsType => xmlFsType;

        public virtual bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(partition.Start > 0) return false;

            if(partition.End < 8) return false;

            byte[] sector = imagePlugin.ReadSector(6);

            if(sector.Length != 128) return false;

            VolumeLabel vol = new VolumeLabel();
            IntPtr volPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vol));
            Marshal.Copy(sector, 0, volPtr, Marshal.SizeOf(vol));
            vol = (VolumeLabel)Marshal.PtrToStructure(volPtr, typeof(VolumeLabel));
            Marshal.FreeHGlobal(volPtr);

            return ECMA67_Magic.SequenceEqual(vol.labelIdentifier) && vol.labelNumber == 1 && vol.recordLength == 0x31;
        }

        public virtual void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                            Encoding encoding)
        {
            currentEncoding = encoding ?? Encoding.GetEncoding("iso-8859-1");
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

            xmlFsType = new FileSystemType
            {
                Type = "ECMA-67",
                ClusterSize = 256,
                Clusters = (long)(partition.End - partition.Start + 1),
                VolumeName = Encoding.ASCII.GetString(vol.volumeIdentifier)
            };

            information = sbInformation.ToString();
        }

        public virtual Errno Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding, bool debug)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public virtual Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VolumeLabel
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] labelIdentifier;
            public byte labelNumber;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public byte[] volumeIdentifier;
            public byte volumeAccessibility;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)] public byte[] reserved1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)] public byte[] owner;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] reserved2;
            public byte surface;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] reserved3;
            public byte recordLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] reserved4;
            public byte fileLabelAllocation;
            public byte labelStandardVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)] public byte[] reserved5;
        }
    }
}