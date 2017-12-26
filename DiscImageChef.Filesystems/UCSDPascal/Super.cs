// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : U.C.S.D. Pascal filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles mounting and umounting the U.C.S.D. Pascal filesystem.
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
using Claunia.Encoding;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;
using Encoding = System.Text.Encoding;

namespace DiscImageChef.Filesystems.UCSDPascal
{
    // Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
    public partial class PascalPlugin
    {
        public Errno Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding, bool debug)
        {
            device = imagePlugin;
            Encoding = encoding ?? new Apple2();
            this.debug = debug;
            if(device.Info.Sectors < 3) return Errno.InvalidArgument;

            // Blocks 0 and 1 are boot code
            catalogBlocks = device.ReadSector(2);

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            mountedVolEntry.firstBlock = BigEndianBitConverter.ToInt16(catalogBlocks, 0x00);
            mountedVolEntry.lastBlock = BigEndianBitConverter.ToInt16(catalogBlocks, 0x02);
            mountedVolEntry.entryType = (PascalFileKind)BigEndianBitConverter.ToInt16(catalogBlocks, 0x04);
            mountedVolEntry.volumeName = new byte[8];
            Array.Copy(catalogBlocks, 0x06, mountedVolEntry.volumeName, 0, 8);
            mountedVolEntry.blocks = BigEndianBitConverter.ToInt16(catalogBlocks, 0x0E);
            mountedVolEntry.files = BigEndianBitConverter.ToInt16(catalogBlocks, 0x10);
            mountedVolEntry.dummy = BigEndianBitConverter.ToInt16(catalogBlocks, 0x12);
            mountedVolEntry.lastBoot = BigEndianBitConverter.ToInt16(catalogBlocks, 0x14);
            mountedVolEntry.tail = BigEndianBitConverter.ToInt32(catalogBlocks, 0x16);

            if(mountedVolEntry.firstBlock != 0 || mountedVolEntry.lastBlock <= mountedVolEntry.firstBlock ||
               (ulong)mountedVolEntry.lastBlock > device.Info.Sectors - 2 ||
               mountedVolEntry.entryType != PascalFileKind.Volume &&
               mountedVolEntry.entryType != PascalFileKind.Secure || mountedVolEntry.volumeName[0] > 7 ||
               mountedVolEntry.blocks < 0 || (ulong)mountedVolEntry.blocks != device.Info.Sectors ||
               mountedVolEntry.files < 0) return Errno.InvalidArgument;

            catalogBlocks = device.ReadSectors(2, (uint)(mountedVolEntry.lastBlock - mountedVolEntry.firstBlock - 2));
            int offset = 26;

            fileEntries = new List<PascalFileEntry>();
            while(offset + 26 < catalogBlocks.Length)
            {
                PascalFileEntry entry = new PascalFileEntry
                {
                    filename = new byte[16],
                    firstBlock = BigEndianBitConverter.ToInt16(catalogBlocks, offset + 0x00),
                    lastBlock = BigEndianBitConverter.ToInt16(catalogBlocks, offset + 0x02),
                    entryType = (PascalFileKind)BigEndianBitConverter.ToInt16(catalogBlocks, offset + 0x04),
                    lastBytes = BigEndianBitConverter.ToInt16(catalogBlocks, offset + 0x16),
                    mtime = BigEndianBitConverter.ToInt16(catalogBlocks, offset + 0x18)
                };
                Array.Copy(catalogBlocks, offset + 0x06, entry.filename, 0, 16);

                if(entry.filename[0] <= 15 && entry.filename[0] > 0) fileEntries.Add(entry);

                offset += 26;
            }

            bootBlocks = device.ReadSectors(0, 2);

            XmlFsType = new FileSystemType
            {
                Bootable = !ArrayHelpers.ArrayIsNullOrEmpty(bootBlocks),
                Clusters = mountedVolEntry.blocks,
                ClusterSize = (int)device.Info.SectorSize,
                Files = mountedVolEntry.files,
                FilesSpecified = true,
                Type = "UCSD Pascal",
                VolumeName = StringHandlers.PascalToString(mountedVolEntry.volumeName, Encoding)
            };

            mounted = true;

            return Errno.NoError;
        }

        public Errno Unmount()
        {
            mounted = false;
            fileEntries = null;
            return Errno.NoError;
        }

        public Errno StatFs(out FileSystemInfo stat)
        {
            stat = new FileSystemInfo
            {
                Blocks = mountedVolEntry.blocks,
                FilenameLength = 16,
                Files = (ulong)mountedVolEntry.files,
                FreeBlocks = 0,
                PluginId = Id,
                Type = "UCSD Pascal"
            };

            stat.FreeBlocks = mountedVolEntry.blocks - (mountedVolEntry.lastBlock - mountedVolEntry.firstBlock);
            foreach(PascalFileEntry entry in fileEntries) stat.FreeBlocks -= entry.lastBlock - entry.firstBlock;

            return Errno.NotImplemented;
        }
    }
}