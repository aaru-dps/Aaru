// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : XFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XFS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the XFS filesystem and shows information.
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
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    public class XFS : IFilesystem
    {
        const uint XFS_MAGIC = 0x58465342;

        Encoding currentEncoding;
        FileSystemType xmlFsType;
        public virtual FileSystemType XmlFsType => xmlFsType;

        public virtual Encoding Encoding => currentEncoding;
        public virtual string Name => "XFS Filesystem Plugin";
        public virtual Guid Id => new Guid("1D8CD8B8-27E6-410F-9973-D16409225FBA");

        public virtual bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 512) return false;

            // Misaligned
            if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                XFS_Superblock xfsSb = new XFS_Superblock();

                uint sbSize = (uint)((Marshal.SizeOf(xfsSb) + 0x400) / imagePlugin.Info.SectorSize);
                if((Marshal.SizeOf(xfsSb) + 0x400) % imagePlugin.Info.SectorSize != 0) sbSize++;

                byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);
                if(sector.Length < Marshal.SizeOf(xfsSb)) return false;

                byte[] sbpiece = new byte[Marshal.SizeOf(xfsSb)];

                foreach(int location in new[] {0, 0x200, 0x400})
                {
                    Array.Copy(sector, location, sbpiece, 0, Marshal.SizeOf(xfsSb));

                    xfsSb = BigEndianMarshal.ByteArrayToStructureBigEndian<XFS_Superblock>(sbpiece);

                    DicConsole.DebugWriteLine("XFS plugin", "magic at 0x{0:X3} = 0x{1:X8} (expected 0x{2:X8})",
                                              location, xfsSb.magicnum, XFS_MAGIC);

                    if(xfsSb.magicnum == XFS_MAGIC) return true;
                }
            }
            else
                foreach(ulong location in new[] {0, 1, 2})
                {
                    XFS_Superblock xfsSb = new XFS_Superblock();

                    uint sbSize = (uint)(Marshal.SizeOf(xfsSb) / imagePlugin.Info.SectorSize);
                    if(Marshal.SizeOf(xfsSb) % imagePlugin.Info.SectorSize != 0) sbSize++;

                    byte[] sector = imagePlugin.ReadSectors(partition.Start + location, sbSize);
                    if(sector.Length < Marshal.SizeOf(xfsSb)) return false;

                    xfsSb = BigEndianMarshal.ByteArrayToStructureBigEndian<XFS_Superblock>(sector);

                    DicConsole.DebugWriteLine("XFS plugin", "magic at {0} = 0x{1:X8} (expected 0x{2:X8})", location,
                                              xfsSb.magicnum, XFS_MAGIC);

                    if(xfsSb.magicnum == XFS_MAGIC) return true;
                }

            return false;
        }

        public virtual void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
        {
            currentEncoding = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";
            if(imagePlugin.Info.SectorSize < 512) return;

            XFS_Superblock xfsSb = new XFS_Superblock();

            // Misaligned
            if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                uint sbSize = (uint)((Marshal.SizeOf(xfsSb) + 0x400) / imagePlugin.Info.SectorSize);
                if((Marshal.SizeOf(xfsSb) + 0x400) % imagePlugin.Info.SectorSize != 0) sbSize++;

                byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);
                if(sector.Length < Marshal.SizeOf(xfsSb)) return;

                byte[] sbpiece = new byte[Marshal.SizeOf(xfsSb)];

                foreach(int location in new[] {0, 0x200, 0x400})
                {
                    Array.Copy(sector, location, sbpiece, 0, Marshal.SizeOf(xfsSb));

                    xfsSb = BigEndianMarshal.ByteArrayToStructureBigEndian<XFS_Superblock>(sbpiece);

                    DicConsole.DebugWriteLine("XFS plugin", "magic at 0x{0:X3} = 0x{1:X8} (expected 0x{2:X8})",
                                              location, xfsSb.magicnum, XFS_MAGIC);

                    if(xfsSb.magicnum == XFS_MAGIC) break;
                }
            }
            else
                foreach(ulong location in new[] {0, 1, 2})
                {
                    uint sbSize = (uint)(Marshal.SizeOf(xfsSb) / imagePlugin.Info.SectorSize);
                    if(Marshal.SizeOf(xfsSb) % imagePlugin.Info.SectorSize != 0) sbSize++;

                    byte[] sector = imagePlugin.ReadSectors(partition.Start + location, sbSize);
                    if(sector.Length < Marshal.SizeOf(xfsSb)) return;

                    xfsSb = BigEndianMarshal.ByteArrayToStructureBigEndian<XFS_Superblock>(sector);

                    DicConsole.DebugWriteLine("XFS plugin", "magic at {0} = 0x{1:X8} (expected 0x{2:X8})", location,
                                              xfsSb.magicnum, XFS_MAGIC);

                    if(xfsSb.magicnum == XFS_MAGIC) break;
                }

            if(xfsSb.magicnum != XFS_MAGIC) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("XFS filesystem");
            sb.AppendFormat("Filesystem version {0}", xfsSb.version & 0xF).AppendLine();
            sb.AppendFormat("{0} bytes per sector", xfsSb.sectsize).AppendLine();
            sb.AppendFormat("{0} bytes per block", xfsSb.blocksize).AppendLine();
            sb.AppendFormat("{0} bytes per inode", xfsSb.inodesize).AppendLine();
            sb.AppendFormat("{0} data blocks in volume, {1} free", xfsSb.dblocks, xfsSb.fdblocks).AppendLine();
            sb.AppendFormat("{0} blocks per allocation group", xfsSb.agblocks).AppendLine();
            sb.AppendFormat("{0} allocation groups in volume", xfsSb.agcount).AppendLine();
            sb.AppendFormat("{0} inodes in volume, {1} free", xfsSb.icount, xfsSb.ifree).AppendLine();
            if(xfsSb.inprogress > 0) sb.AppendLine("fsck in progress");
            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(xfsSb.fname, currentEncoding)).AppendLine();
            sb.AppendFormat("Volume UUID: {0}", xfsSb.uuid).AppendLine();

            information = sb.ToString();

            xmlFsType = new FileSystemType
            {
                Type = "XFS filesystem",
                ClusterSize = (int)xfsSb.blocksize,
                Clusters = (long)xfsSb.dblocks,
                FreeClusters = (long)xfsSb.fdblocks,
                FreeClustersSpecified = true,
                Files = (long)(xfsSb.icount - xfsSb.ifree),
                FilesSpecified = true,
                Dirty = xfsSb.inprogress > 0,
                VolumeName = StringHandlers.CToString(xfsSb.fname, currentEncoding),
                VolumeSerial = xfsSb.uuid.ToString()
            };
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
        struct XFS_Superblock
        {
            public uint magicnum;
            public uint blocksize;
            public ulong dblocks;
            public ulong rblocks;
            public ulong rextents;
            public Guid uuid;
            public ulong logstat;
            public ulong rootino;
            public ulong rbmino;
            public ulong rsumino;
            public uint rextsize;
            public uint agblocks;
            public uint agcount;
            public uint rbmblocks;
            public uint logblocks;
            public ushort version;
            public ushort sectsize;
            public ushort inodesize;
            public ushort inopblock;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] fname;
            public byte blocklog;
            public byte sectlog;
            public byte inodelog;
            public byte inopblog;
            public byte agblklog;
            public byte rextslog;
            public byte inprogress;
            public byte imax_pct;
            public ulong icount;
            public ulong ifree;
            public ulong fdblocks;
            public ulong frextents;
            public ulong uquotino;
            public ulong gquotino;
            public ushort qflags;
            public byte flags;
            public byte shared_vn;
            public ulong inoalignmt;
            public ulong unit;
            public ulong width;
            public byte dirblklog;
            public byte logsectlog;
            public ushort logsectsize;
            public uint logsunit;
            public uint features2;
            public uint bad_features2;
            public uint features_compat;
            public uint features_ro_compat;
            public uint features_incompat;
            public uint features_log_incompat;
            // This field is little-endian while rest of superblock is big-endian
            public uint crc;
            public uint spino_align;
            public ulong pquotino;
            public ulong lsn;
            public Guid meta_uuid;
        }
    }
}