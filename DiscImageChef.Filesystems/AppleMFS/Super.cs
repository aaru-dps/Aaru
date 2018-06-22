// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Macintosh File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles mounting and umounting the Apple Macintosh File System.
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
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems.AppleMFS
{
    // Information from Inside Macintosh Volume II
    public partial class AppleMFS
    {
        public Errno Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                           Dictionary<string, string> options)
        {
            device         = imagePlugin;
            partitionStart = partition.Start;
            Encoding       = encoding ?? Encoding.GetEncoding("macintosh");
            if(options == null) options = GetDefaultOptions();
            if(options.TryGetValue("debug", out string debugString)) bool.TryParse(debugString, out debug);
            volMDB = new MFS_MasterDirectoryBlock();

            mdbBlocks  = device.ReadSector(2 + partitionStart);
            bootBlocks = device.ReadSector(0 + partitionStart);

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            volMDB.drSigWord = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x000);
            if(volMDB.drSigWord != MFS_MAGIC) return Errno.InvalidArgument;

            volMDB.drCrDate   = BigEndianBitConverter.ToUInt32(mdbBlocks, 0x002);
            volMDB.drLsBkUp   = BigEndianBitConverter.ToUInt32(mdbBlocks, 0x006);
            volMDB.drAtrb     = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x00A);
            volMDB.drNmFls    = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x00C);
            volMDB.drDirSt    = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x00E);
            volMDB.drBlLen    = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x010);
            volMDB.drNmAlBlks = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x012);
            volMDB.drAlBlkSiz = BigEndianBitConverter.ToUInt32(mdbBlocks, 0x014);
            volMDB.drClpSiz   = BigEndianBitConverter.ToUInt32(mdbBlocks, 0x018);
            volMDB.drAlBlSt   = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x01C);
            volMDB.drNxtFNum  = BigEndianBitConverter.ToUInt32(mdbBlocks, 0x01E);
            volMDB.drFreeBks  = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x022);
            volMDB.drVNSiz    = mdbBlocks[0x024];
            byte[] variableSize = new byte[volMDB.drVNSiz + 1];
            Array.Copy(mdbBlocks, 0x024, variableSize, 0, volMDB.drVNSiz + 1);
            volMDB.drVN = StringHandlers.PascalToString(variableSize, Encoding);

            directoryBlocks = device.ReadSectors(volMDB.drDirSt + partitionStart, volMDB.drBlLen);
            int       bytesInBlockMap        = volMDB.drNmAlBlks * 12 / 8 + volMDB.drNmAlBlks * 12 % 8;
            const int BYTES_BEFORE_BLOCK_MAP = 64;
            int       bytesInWholeMdb        = bytesInBlockMap + BYTES_BEFORE_BLOCK_MAP;
            int sectorsInWholeMdb = bytesInWholeMdb / (int)device.Info.SectorSize +
                                    bytesInWholeMdb % (int)device.Info.SectorSize;
            byte[] wholeMdb = device.ReadSectors(partitionStart + 2, (uint)sectorsInWholeMdb);
            blockMapBytes = new byte[bytesInBlockMap];
            Array.Copy(wholeMdb, BYTES_BEFORE_BLOCK_MAP, blockMapBytes, 0, blockMapBytes.Length);

            int offset = 0;
            blockMap = new uint[volMDB.drNmAlBlks + 2 + 1];
            for(int i = 2; i < volMDB.drNmAlBlks + 2; i += 8)
            {
                uint tmp1 = 0;
                uint tmp2 = 0;
                uint tmp3 = 0;

                if(offset + 4 <= blockMapBytes.Length) tmp1 = BigEndianBitConverter.ToUInt32(blockMapBytes, offset);
                if(offset + 4 + 4 <= blockMapBytes.Length)
                    tmp2 = BigEndianBitConverter.ToUInt32(blockMapBytes, offset + 4);
                if(offset + 8 + 4 <= blockMapBytes.Length)
                    tmp3 = BigEndianBitConverter.ToUInt32(blockMapBytes, offset + 8);

                if(i     < blockMap.Length) blockMap[i]     = (tmp1 & 0xFFF00000) >> 20;
                if(i + 2 < blockMap.Length) blockMap[i + 1] = (tmp1 & 0xFFF00)    >> 8;
                if(i + 3 < blockMap.Length) blockMap[i + 2] = ((tmp1 & 0xFF) << 4) + ((tmp2 & 0xF0000000) >> 28);
                if(i + 4 < blockMap.Length) blockMap[i + 3] = (tmp2 & 0xFFF0000) >> 16;
                if(i + 5 < blockMap.Length) blockMap[i + 4] = (tmp2 & 0xFFF0)    >> 4;
                if(i + 6 < blockMap.Length) blockMap[i + 5] = ((tmp2 & 0xF) << 8) + ((tmp3 & 0xFF000000) >> 24);
                if(i + 7 < blockMap.Length) blockMap[i + 6] = (tmp3 & 0xFFF000) >> 12;
                if(i + 8 < blockMap.Length) blockMap[i + 7] = tmp3 & 0xFFF;

                offset += 12;
            }

            if(device.Info.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag))
            {
                mdbTags  = device.ReadSectorTag(2 + partitionStart, SectorTagType.AppleSectorTag);
                bootTags = device.ReadSectorTag(0 + partitionStart, SectorTagType.AppleSectorTag);
                directoryTags = device.ReadSectorsTag(volMDB.drDirSt + partitionStart, volMDB.drBlLen,
                                                      SectorTagType.AppleSectorTag);
                bitmapTags = device.ReadSectorsTag(partitionStart + 2, (uint)sectorsInWholeMdb,
                                                   SectorTagType.AppleSectorTag);
            }

            sectorsPerBlock = (int)(volMDB.drAlBlkSiz / device.Info.SectorSize);

            if(!FillDirectory()) return Errno.InvalidArgument;

            mounted = true;

            ushort bbSig = BigEndianBitConverter.ToUInt16(bootBlocks, 0x000);

            if(bbSig != MFSBB_MAGIC) bootBlocks = null;

            XmlFsType = new FileSystemType();
            if(volMDB.drLsBkUp > 0)
            {
                XmlFsType.BackupDate          = DateHandlers.MacToDateTime(volMDB.drLsBkUp);
                XmlFsType.BackupDateSpecified = true;
            }

            XmlFsType.Bootable    = bbSig == MFSBB_MAGIC;
            XmlFsType.Clusters    = volMDB.drNmAlBlks;
            XmlFsType.ClusterSize = (int)volMDB.drAlBlkSiz;
            if(volMDB.drCrDate > 0)
            {
                XmlFsType.CreationDate          = DateHandlers.MacToDateTime(volMDB.drCrDate);
                XmlFsType.CreationDateSpecified = true;
            }

            XmlFsType.Files                 = volMDB.drNmFls;
            XmlFsType.FilesSpecified        = true;
            XmlFsType.FreeClusters          = volMDB.drFreeBks;
            XmlFsType.FreeClustersSpecified = true;
            XmlFsType.Type                  = "MFS";
            XmlFsType.VolumeName            = volMDB.drVN;

            return Errno.NoError;
        }

        public Errno Unmount()
        {
            mounted      = false;
            idToFilename = null;
            idToEntry    = null;
            filenameToId = null;
            bootBlocks   = null;

            return Errno.NoError;
        }

        public Errno StatFs(out FileSystemInfo stat)
        {
            stat = new FileSystemInfo
            {
                Blocks         = volMDB.drNmAlBlks,
                FilenameLength = 255,
                Files          = volMDB.drNmFls,
                FreeBlocks     = volMDB.drFreeBks,
                PluginId       = Id,
                Type           = "Apple MFS"
            };
            stat.FreeFiles = uint.MaxValue - stat.Files;

            return Errno.NoError;
        }
    }
}