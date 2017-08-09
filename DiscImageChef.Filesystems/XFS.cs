// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : XFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems
{
    public class XFS : Filesystem
    {
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] fname;
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

        const uint XFS_Magic = 0x58465342;

        public XFS()
        {
            Name = "XFS Filesystem Plugin";
            PluginUUID = new Guid("1D8CD8B8-27E6-410F-9973-D16409225FBA");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public XFS(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "XFS Filesystem Plugin";
            PluginUUID = new Guid("1D8CD8B8-27E6-410F-9973-D16409225FBA");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else
                CurrentEncoding = encoding;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if(imagePlugin.GetSectorSize() < 512)
                return false;

            // Misaligned
            if(imagePlugin.ImageInfo.xmlMediaType == ImagePlugins.XmlMediaType.OpticalDisc)
            {
                XFS_Superblock xfsSb = new XFS_Superblock();

                uint sbSize = (uint)((Marshal.SizeOf(xfsSb) + 0x400) / imagePlugin.GetSectorSize());
                if((Marshal.SizeOf(xfsSb) + 0x400) % imagePlugin.GetSectorSize() != 0)
                    sbSize++;

                byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);
                if(sector.Length < Marshal.SizeOf(xfsSb))
                    return false;

                byte[] sbpiece = new byte[Marshal.SizeOf(xfsSb)];

                foreach(int location in new[] { 0, 0x200, 0x400 })
                {
                    Array.Copy(sector, location, sbpiece, 0, Marshal.SizeOf(xfsSb));

                    xfsSb = BigEndianMarshal.ByteArrayToStructureBigEndian<XFS_Superblock>(sbpiece);

                    DicConsole.DebugWriteLine("XFS plugin", "magic at 0x{0:X3} = 0x{1:X8} (expected 0x{2:X8})", location, xfsSb.magicnum, XFS_Magic);

                    if(xfsSb.magicnum == XFS_Magic)
                        return true;
                }
            }
            else
            {
                foreach(ulong location in new[] { 0, 1, 2 })
                {
                    XFS_Superblock xfsSb = new XFS_Superblock();

                    uint sbSize = (uint)(Marshal.SizeOf(xfsSb) / imagePlugin.GetSectorSize());
                    if(Marshal.SizeOf(xfsSb) % imagePlugin.GetSectorSize() != 0)
                        sbSize++;

                    byte[] sector = imagePlugin.ReadSectors(partition.Start + location, sbSize);
                    if(sector.Length < Marshal.SizeOf(xfsSb))
                        return false;

                    xfsSb = BigEndianMarshal.ByteArrayToStructureBigEndian<XFS_Superblock>(sector);

                    DicConsole.DebugWriteLine("XFS plugin", "magic at {0} = 0x{1:X8} (expected 0x{2:X8})", location, xfsSb.magicnum, XFS_Magic);

                    if(xfsSb.magicnum == XFS_Magic)
                        return true;
                }
            }

            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";
            if(imagePlugin.GetSectorSize() < 512)
                return;

            XFS_Superblock xfsSb = new XFS_Superblock();

            // Misaligned
            if(imagePlugin.ImageInfo.xmlMediaType == ImagePlugins.XmlMediaType.OpticalDisc)
            {
                uint sbSize = (uint)((Marshal.SizeOf(xfsSb) + 0x400) / imagePlugin.GetSectorSize());
                if((Marshal.SizeOf(xfsSb) + 0x400) % imagePlugin.GetSectorSize() != 0)
                    sbSize++;

                byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);
                if(sector.Length < Marshal.SizeOf(xfsSb))
                    return;

                byte[] sbpiece = new byte[Marshal.SizeOf(xfsSb)];

                foreach(int location in new[] { 0, 0x200, 0x400 })
                {
                    Array.Copy(sector, location, sbpiece, 0, Marshal.SizeOf(xfsSb));

                    xfsSb = BigEndianMarshal.ByteArrayToStructureBigEndian<XFS_Superblock>(sbpiece);

                    DicConsole.DebugWriteLine("XFS plugin", "magic at 0x{0:X3} = 0x{1:X8} (expected 0x{2:X8})", location, xfsSb.magicnum, XFS_Magic);

                    if(xfsSb.magicnum == XFS_Magic)
                        break;
                }
            }
            else
            {
                foreach(ulong location in new[] { 0, 1, 2 })
                {
                    uint sbSize = (uint)(Marshal.SizeOf(xfsSb) / imagePlugin.GetSectorSize());
                    if(Marshal.SizeOf(xfsSb) % imagePlugin.GetSectorSize() != 0)
                        sbSize++;

                    byte[] sector = imagePlugin.ReadSectors(partition.Start + location, sbSize);
                    if(sector.Length < Marshal.SizeOf(xfsSb))
                        return;

                    xfsSb = BigEndianMarshal.ByteArrayToStructureBigEndian<XFS_Superblock>(sector);

                    DicConsole.DebugWriteLine("XFS plugin", "magic at {0} = 0x{1:X8} (expected 0x{2:X8})", location, xfsSb.magicnum, XFS_Magic);

                    if(xfsSb.magicnum == XFS_Magic)
                        break;
                }
            }

            if(xfsSb.magicnum != XFS_Magic)
                return;

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
            if(xfsSb.inprogress > 0)
                sb.AppendLine("fsck in progress");
            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(xfsSb.fname, CurrentEncoding)).AppendLine();
            sb.AppendFormat("Volume UUID: {0}", xfsSb.uuid).AppendLine();

            information = sb.ToString();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Type = "XFS filesystem";
            xmlFSType.ClusterSize = (int)xfsSb.blocksize;
            xmlFSType.Clusters = (long)xfsSb.dblocks;
            xmlFSType.FreeClusters = (long)xfsSb.fdblocks;
            xmlFSType.FreeClustersSpecified = true;
            xmlFSType.Files = (long)(xfsSb.icount - xfsSb.ifree);
            xmlFSType.FilesSpecified = true;
            xmlFSType.Dirty |= xfsSb.inprogress > 0;
            xmlFSType.VolumeName = StringHandlers.CToString(xfsSb.fname, CurrentEncoding);
            xmlFSType.VolumeSerial = xfsSb.uuid.ToString();
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