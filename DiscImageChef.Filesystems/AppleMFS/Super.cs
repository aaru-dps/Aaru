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
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems.AppleMFS
{
    // Information from Inside Macintosh Volume II
    public partial class AppleMFS
    {
        public override Errno Mount(bool debug)
        {
            this.debug = debug;
            volMDB = new MFS_MasterDirectoryBlock();

            byte[] variable_size;

            mdbBlocks = device.ReadSector(2 + partitionStart);
            bootBlocks = device.ReadSector(0 + partitionStart);

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            volMDB.drSigWord = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x000);
            if(volMDB.drSigWord != MFS_MAGIC) return Errno.InvalidArgument;

            volMDB.drCrDate = BigEndianBitConverter.ToUInt32(mdbBlocks, 0x002);
            volMDB.drLsBkUp = BigEndianBitConverter.ToUInt32(mdbBlocks, 0x006);
            volMDB.drAtrb = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x00A);
            volMDB.drNmFls = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x00C);
            volMDB.drDirSt = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x00E);
            volMDB.drBlLen = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x010);
            volMDB.drNmAlBlks = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x012);
            volMDB.drAlBlkSiz = BigEndianBitConverter.ToUInt32(mdbBlocks, 0x014);
            volMDB.drClpSiz = BigEndianBitConverter.ToUInt32(mdbBlocks, 0x018);
            volMDB.drAlBlSt = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x01C);
            volMDB.drNxtFNum = BigEndianBitConverter.ToUInt32(mdbBlocks, 0x01E);
            volMDB.drFreeBks = BigEndianBitConverter.ToUInt16(mdbBlocks, 0x022);
            volMDB.drVNSiz = mdbBlocks[0x024];
            variable_size = new byte[volMDB.drVNSiz + 1];
            Array.Copy(mdbBlocks, 0x024, variable_size, 0, volMDB.drVNSiz + 1);
            volMDB.drVN = StringHandlers.PascalToString(variable_size, CurrentEncoding);

            directoryBlocks = device.ReadSectors(volMDB.drDirSt + partitionStart, volMDB.drBlLen);
            int bytesInBlockMap = volMDB.drNmAlBlks * 12 / 8 + volMDB.drNmAlBlks * 12 % 8;
            int bytesBeforeBlockMap = 64;
            int bytesInWholeMDB = bytesInBlockMap + bytesBeforeBlockMap;
            int sectorsInWholeMDB = bytesInWholeMDB / (int)device.ImageInfo.SectorSize +
                                    bytesInWholeMDB % (int)device.ImageInfo.SectorSize;
            byte[] wholeMDB = device.ReadSectors(partitionStart + 2, (uint)sectorsInWholeMDB);
            blockMapBytes = new byte[bytesInBlockMap];
            Array.Copy(wholeMDB, bytesBeforeBlockMap, blockMapBytes, 0, blockMapBytes.Length);

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

                if(i < blockMap.Length) blockMap[i] = (tmp1 & 0xFFF00000) >> 20;
                if(i + 2 < blockMap.Length) blockMap[i + 1] = (tmp1 & 0xFFF00) >> 8;
                if(i + 3 < blockMap.Length) blockMap[i + 2] = ((tmp1 & 0xFF) << 4) + ((tmp2 & 0xF0000000) >> 28);
                if(i + 4 < blockMap.Length) blockMap[i + 3] = (tmp2 & 0xFFF0000) >> 16;
                if(i + 5 < blockMap.Length) blockMap[i + 4] = (tmp2 & 0xFFF0) >> 4;
                if(i + 6 < blockMap.Length) blockMap[i + 5] = ((tmp2 & 0xF) << 8) + ((tmp3 & 0xFF000000) >> 24);
                if(i + 7 < blockMap.Length) blockMap[i + 6] = (tmp3 & 0xFFF000) >> 12;
                if(i + 8 < blockMap.Length) blockMap[i + 7] = tmp3 & 0xFFF;

                offset += 12;
            }

            if(device.ImageInfo.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag))
            {
                mdbTags = device.ReadSectorTag(2 + partitionStart, SectorTagType.AppleSectorTag);
                bootTags = device.ReadSectorTag(0 + partitionStart, SectorTagType.AppleSectorTag);
                directoryTags = device.ReadSectorsTag(volMDB.drDirSt + partitionStart, volMDB.drBlLen,
                                                      SectorTagType.AppleSectorTag);
                bitmapTags = device.ReadSectorsTag(partitionStart + 2, (uint)sectorsInWholeMDB,
                                                   SectorTagType.AppleSectorTag);
            }

            sectorsPerBlock = (int)(volMDB.drAlBlkSiz / device.ImageInfo.SectorSize);

            if(!FillDirectory()) return Errno.InvalidArgument;

            mounted = true;

            ushort bbSig = BigEndianBitConverter.ToUInt16(bootBlocks, 0x000);

            if(bbSig != MFSBB_MAGIC) bootBlocks = null;

            xmlFSType = new FileSystemType();
            if(volMDB.drLsBkUp > 0)
            {
                xmlFSType.BackupDate = DateHandlers.MacToDateTime(volMDB.drLsBkUp);
                xmlFSType.BackupDateSpecified = true;
            }
            xmlFSType.Bootable = bbSig == MFSBB_MAGIC;
            xmlFSType.Clusters = volMDB.drNmAlBlks;
            xmlFSType.ClusterSize = (int)volMDB.drAlBlkSiz;
            if(volMDB.drCrDate > 0)
            {
                xmlFSType.CreationDate = DateHandlers.MacToDateTime(volMDB.drCrDate);
                xmlFSType.CreationDateSpecified = true;
            }
            xmlFSType.Files = volMDB.drNmFls;
            xmlFSType.FilesSpecified = true;
            xmlFSType.FreeClusters = volMDB.drFreeBks;
            xmlFSType.FreeClustersSpecified = true;
            xmlFSType.Type = "MFS";
            xmlFSType.VolumeName = volMDB.drVN;

            return Errno.NoError;
        }

        public override Errno Mount()
        {
            return Mount(false);
        }

        public override Errno Unmount()
        {
            mounted = false;
            idToFilename = null;
            idToEntry = null;
            filenameToId = null;
            bootBlocks = null;

            return Errno.NoError;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            stat = new FileSystemInfo();
            stat.Blocks = volMDB.drNmAlBlks;
            stat.FilenameLength = 255;
            stat.Files = volMDB.drNmFls;
            stat.FreeBlocks = volMDB.drFreeBks;
            stat.FreeFiles = uint.MaxValue - stat.Files;
            stat.PluginId = PluginUUID;
            stat.Type = "Apple MFS";

            return Errno.NoError;
        }
    }
}