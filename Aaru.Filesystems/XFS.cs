// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    /// <inheritdoc />
    /// <summary>Implements detection of SGI's XFS</summary>
    public sealed class XFS : IFilesystem
    {
        const uint XFS_MAGIC = 0x58465342;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Name => "XFS Filesystem Plugin";
        /// <inheritdoc />
        public Guid Id => new Guid("1D8CD8B8-27E6-410F-9973-D16409225FBA");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 512)
                return false;

            // Misaligned
            if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                uint sbSize = (uint)((Marshal.SizeOf<Superblock>() + 0x400) / imagePlugin.Info.SectorSize);

                if((Marshal.SizeOf<Superblock>() + 0x400) % imagePlugin.Info.SectorSize != 0)
                    sbSize++;

                byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);

                if(sector.Length < Marshal.SizeOf<Superblock>())
                    return false;

                byte[] sbpiece = new byte[Marshal.SizeOf<Superblock>()];

                foreach(int location in new[]
                {
                    0, 0x200, 0x400
                })
                {
                    Array.Copy(sector, location, sbpiece, 0, Marshal.SizeOf<Superblock>());

                    Superblock xfsSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sbpiece);

                    AaruConsole.DebugWriteLine("XFS plugin", "magic at 0x{0:X3} = 0x{1:X8} (expected 0x{2:X8})",
                                               location, xfsSb.magicnum, XFS_MAGIC);

                    if(xfsSb.magicnum == XFS_MAGIC)
                        return true;
                }
            }
            else
                foreach(int i in new[]
                {
                    0, 1, 2
                })
                {
                    ulong location = (ulong)i;

                    uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

                    if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                        sbSize++;

                    byte[] sector = imagePlugin.ReadSectors(partition.Start + location, sbSize);

                    if(sector.Length < Marshal.SizeOf<Superblock>())
                        return false;

                    Superblock xfsSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

                    AaruConsole.DebugWriteLine("XFS plugin", "magic at {0} = 0x{1:X8} (expected 0x{2:X8})", location,
                                               xfsSb.magicnum, XFS_MAGIC);

                    if(xfsSb.magicnum == XFS_MAGIC)
                        return true;
                }

            return false;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";

            if(imagePlugin.Info.SectorSize < 512)
                return;

            var xfsSb = new Superblock();

            // Misaligned
            if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                uint sbSize = (uint)((Marshal.SizeOf<Superblock>() + 0x400) / imagePlugin.Info.SectorSize);

                if((Marshal.SizeOf<Superblock>() + 0x400) % imagePlugin.Info.SectorSize != 0)
                    sbSize++;

                byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);

                if(sector.Length < Marshal.SizeOf<Superblock>())
                    return;

                byte[] sbpiece = new byte[Marshal.SizeOf<Superblock>()];

                foreach(int location in new[]
                {
                    0, 0x200, 0x400
                })
                {
                    Array.Copy(sector, location, sbpiece, 0, Marshal.SizeOf<Superblock>());

                    xfsSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sbpiece);

                    AaruConsole.DebugWriteLine("XFS plugin", "magic at 0x{0:X3} = 0x{1:X8} (expected 0x{2:X8})",
                                               location, xfsSb.magicnum, XFS_MAGIC);

                    if(xfsSb.magicnum == XFS_MAGIC)
                        break;
                }
            }
            else
                foreach(int i in new[]
                {
                    0, 1, 2
                })
                {
                    ulong location = (ulong)i;
                    uint  sbSize   = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

                    if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                        sbSize++;

                    byte[] sector = imagePlugin.ReadSectors(partition.Start + location, sbSize);

                    if(sector.Length < Marshal.SizeOf<Superblock>())
                        return;

                    xfsSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

                    AaruConsole.DebugWriteLine("XFS plugin", "magic at {0} = 0x{1:X8} (expected 0x{2:X8})", location,
                                               xfsSb.magicnum, XFS_MAGIC);

                    if(xfsSb.magicnum == XFS_MAGIC)
                        break;
                }

            if(xfsSb.magicnum != XFS_MAGIC)
                return;

            var sb = new StringBuilder();

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

            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(xfsSb.fname, Encoding)).AppendLine();
            sb.AppendFormat("Volume UUID: {0}", xfsSb.uuid).AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type                  = "XFS filesystem",
                ClusterSize           = xfsSb.blocksize,
                Clusters              = xfsSb.dblocks,
                FreeClusters          = xfsSb.fdblocks,
                FreeClustersSpecified = true,
                Files                 = xfsSb.icount - xfsSb.ifree,
                FilesSpecified        = true,
                Dirty                 = xfsSb.inprogress > 0,
                VolumeName            = StringHandlers.CToString(xfsSb.fname, Encoding),
                VolumeSerial          = xfsSb.uuid.ToString()
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Superblock
        {
            public readonly uint   magicnum;
            public readonly uint   blocksize;
            public readonly ulong  dblocks;
            public readonly ulong  rblocks;
            public readonly ulong  rextents;
            public readonly Guid   uuid;
            public readonly ulong  logstat;
            public readonly ulong  rootino;
            public readonly ulong  rbmino;
            public readonly ulong  rsumino;
            public readonly uint   rextsize;
            public readonly uint   agblocks;
            public readonly uint   agcount;
            public readonly uint   rbmblocks;
            public readonly uint   logblocks;
            public readonly ushort version;
            public readonly ushort sectsize;
            public readonly ushort inodesize;
            public readonly ushort inopblock;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public readonly byte[] fname;
            public readonly byte   blocklog;
            public readonly byte   sectlog;
            public readonly byte   inodelog;
            public readonly byte   inopblog;
            public readonly byte   agblklog;
            public readonly byte   rextslog;
            public readonly byte   inprogress;
            public readonly byte   imax_pct;
            public readonly ulong  icount;
            public readonly ulong  ifree;
            public readonly ulong  fdblocks;
            public readonly ulong  frextents;
            public readonly ulong  uquotino;
            public readonly ulong  gquotino;
            public readonly ushort qflags;
            public readonly byte   flags;
            public readonly byte   shared_vn;
            public readonly ulong  inoalignmt;
            public readonly ulong  unit;
            public readonly ulong  width;
            public readonly byte   dirblklog;
            public readonly byte   logsectlog;
            public readonly ushort logsectsize;
            public readonly uint   logsunit;
            public readonly uint   features2;
            public readonly uint   bad_features2;
            public readonly uint   features_compat;
            public readonly uint   features_ro_compat;
            public readonly uint   features_incompat;
            public readonly uint   features_log_incompat;

            // This field is little-endian while rest of superblock is big-endian
            public readonly uint  crc;
            public readonly uint  spino_align;
            public readonly ulong pquotino;
            public readonly ulong lsn;
            public readonly Guid  meta_uuid;
        }
    }
}