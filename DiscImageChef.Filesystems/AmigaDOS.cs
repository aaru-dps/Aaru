/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : AmigaDOS.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies PC-Engine CDs.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Text;
using DiscImageChef;
using DiscImageChef.PartPlugins;
using System.Collections.Generic;
using DiscImageChef.Console;

namespace DiscImageChef.Plugins
{
    class AmigaDOSPlugin : Plugin
    {
        public AmigaDOSPlugin()
        {
            Name = "Amiga DOS filesystem";
            PluginUUID = new Guid("3c882400-208c-427d-a086-9119852a1bc7");
        }

        /// <summary>
        /// Boot block, first 2 sectors
        /// </summary>
        struct BootBlock
        {
            /// <summary>
            /// Offset 0x00, "DOSx" disk type
            /// </summary>
            public UInt32 diskType;
            /// <summary>
            /// Offset 0x04, Checksum
            /// </summary>
            public UInt32 checksum;
            /// <summary>
            /// Offset 0x08, Pointer to root block, mostly invalid
            /// </summary>
            public UInt32 root_ptr;
            /// <summary>
            /// Offset 0x0C, Boot code, til completion
            /// </summary>
            public byte[] bootCode;
        }

        struct RootBlock
        {
            /// <summary>
            /// Offset 0x00, block type, value = T_HEADER (2)
            /// </summary>
            public UInt32 type;
            /// <summary>
            /// Offset 0x04, unused
            /// </summary>
            public UInt32 headerKey;
            /// <summary>
            /// Offset 0x08, unused
            /// </summary>
            public UInt32 highSeq;
            /// <summary>
            /// Offset 0x0C, longs used by hash table
            /// </summary>
            public UInt32 hashTableSize;
            /// <summary>
            /// Offset 0x10, unused
            /// </summary>
            public UInt32 firstData;
            /// <summary>
            /// Offset 0x14, Rootblock checksum
            /// </summary>
            public UInt32 checksum;
            /// <summary>
            /// Offset 0x18, Hashtable, size = (block size / 4) - 56 or size = hashTableSize
            /// </summary>
            public UInt32[] hashTable;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+0, bitmap flag, 0xFFFFFFFF if valid
            /// </summary>
            public UInt32 bitmapFlag;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+4, bitmap pages, 25 entries
            /// </summary>
            public UInt32[] bitmapPages;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+104, pointer to bitmap extension block
            /// </summary>
            public UInt32 bitmapExtensionBlock;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+108, last root alteration days since 1978/01/01
            /// </summary>
            public UInt32 rDays;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+112, last root alteration minutes past midnight
            /// </summary>
            public UInt32 rMins;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+116, last root alteration ticks (1/50 secs)
            /// </summary>
            public UInt32 rTicks;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+120, disk name, pascal string, 32 bytes
            /// </summary>
            public string diskName;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+152, unused
            /// </summary>
            public UInt32 reserved1;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+156, unused
            /// </summary>
            public UInt32 reserved2;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+160, last disk alteration days since 1978/01/01
            /// </summary>
            public UInt32 vDays;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+164, last disk alteration minutes past midnight
            /// </summary>
            public UInt32 vMins;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+168, last disk alteration ticks (1/50 secs)
            /// </summary>
            public UInt32 vTicks;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+172, filesystem creation days since 1978/01/01
            /// </summary>
            public UInt32 cDays;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+176, filesystem creation minutes since 1978/01/01
            /// </summary>
            public UInt32 cMins;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+180, filesystem creation ticks since 1978/01/01
            /// </summary>
            public UInt32 cTicks;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+184, unused
            /// </summary>
            public UInt32 nextHash;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+188, unused
            /// </summary>
            public UInt32 parentDir;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+192, first directory cache block
            /// </summary>
            public UInt32 extension;
            /// <summary>
            /// Offset 0x18+hashTableSize*4+196, block secondary type = ST_ROOT (1)
            /// </summary>
            public UInt32 sec_type;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if (partitionStart >= imagePlugin.GetSectors())
                return false;

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] sector = imagePlugin.ReadSector(0 + partitionStart);

            UInt32 magic = BigEndianBitConverter.ToUInt32(sector, 0x00);

            if ((magic & 0x6D754600) != 0x6D754600 &&
                (magic & 0x444F5300) != 0x444F5300)
                return false;

            ulong root_ptr = BigEndianBitConverter.ToUInt32(sector, 0x08);
            DicConsole.DebugWriteLine("AmigaDOS plugin", "Bootblock points to {0} as Rootblock", root_ptr);

            root_ptr = (partitionEnd - partitionStart) / 2 + partitionStart + 1;

            DicConsole.DebugWriteLine("AmigaDOS plugin", "Nonetheless, going to block {0} for Rootblock", root_ptr);

            if (root_ptr >= imagePlugin.GetSectors())
                return false;

            sector = imagePlugin.ReadSector(root_ptr);

            UInt32 type = BigEndianBitConverter.ToUInt32(sector, 0x00);
            UInt32 hashTableSize = BigEndianBitConverter.ToUInt32(sector, 0x0C);

            if ((0x18 + hashTableSize * 4 + 196) > sector.Length)
                return false;

            UInt32 sec_type = BigEndianBitConverter.ToUInt32(sector, (int)(0x18 + hashTableSize * 4 + 196));

            return type == 2 && sec_type == 1;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            StringBuilder sbInformation = new StringBuilder();

            byte[] BootBlockSectors = imagePlugin.ReadSectors(0 + partitionStart, 2);
            byte[] RootBlockSector = imagePlugin.ReadSector((partitionEnd - partitionStart) / 2 + partitionStart + 1);
            byte[] diskName = new byte[32];

            BootBlock bootBlk = new BootBlock();
            RootBlock rootBlk = new RootBlock();
            xmlFSType = new Schemas.FileSystemType();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            bootBlk.diskType = BigEndianBitConverter.ToUInt32(BootBlockSectors, 0x00);
            bootBlk.checksum = BigEndianBitConverter.ToUInt32(BootBlockSectors, 0x04);
            bootBlk.root_ptr = BigEndianBitConverter.ToUInt32(BootBlockSectors, 0x08);
            bootBlk.bootCode = new byte[BootBlockSectors.Length - 0x0C];
            Array.Copy(BootBlockSectors, 0x0C, bootBlk.bootCode, 0, BootBlockSectors.Length - 0x0C);

            DicConsole.DebugWriteLine("AmigaDOS plugin", "Stored boot blocks checksum is 0x{0:X8}", bootBlk.checksum);
            DicConsole.DebugWriteLine("AmigaDOS plugin", "Probably incorrect calculated boot blocks checksum is 0x{0:X8}", AmigaChecksum(RootBlockSector));
            Checksums.SHA1Context sha1Ctx = new Checksums.SHA1Context();
            sha1Ctx.Init();
            sha1Ctx.Update(bootBlk.bootCode);
            DicConsole.DebugWriteLine("AmigaDOS plugin", "Boot code SHA1 is {0}", sha1Ctx.End());

            rootBlk.type = BigEndianBitConverter.ToUInt32(RootBlockSector, 0x00);
            rootBlk.headerKey = BigEndianBitConverter.ToUInt32(RootBlockSector, 0x04);
            rootBlk.highSeq = BigEndianBitConverter.ToUInt32(RootBlockSector, 0x08);
            rootBlk.hashTableSize = BigEndianBitConverter.ToUInt32(RootBlockSector, 0x0C);
            rootBlk.firstData = BigEndianBitConverter.ToUInt32(RootBlockSector, 0x10);
            rootBlk.checksum = BigEndianBitConverter.ToUInt32(RootBlockSector, 0x14);
            rootBlk.hashTable = new uint[rootBlk.hashTableSize];

            for (int i = 0; i < rootBlk.hashTableSize; i++)
                rootBlk.hashTable[i] = BigEndianBitConverter.ToUInt32(RootBlockSector, 0x18 + i * 4);

            rootBlk.bitmapFlag = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 0));
            rootBlk.bitmapPages = new uint[25];

            for (int i = 0; i < 25; i++)
                rootBlk.bitmapPages[i] = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 4 + i * 4));

            rootBlk.bitmapExtensionBlock = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 104));
            rootBlk.rDays = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 108));
            rootBlk.rMins = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 112));
            rootBlk.rTicks = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 116));

            Array.Copy(RootBlockSector, 0x18 + rootBlk.hashTableSize * 4 + 120, diskName, 0, 32);
            rootBlk.diskName = StringHandlers.PascalToString(diskName);

            rootBlk.reserved1 = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 152));
            rootBlk.reserved2 = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 156));
            rootBlk.vDays = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 160));
            rootBlk.vMins = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 164));
            rootBlk.vTicks = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 168));
            rootBlk.cDays = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 172));
            rootBlk.cMins = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 176));
            rootBlk.cTicks = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 180));
            rootBlk.nextHash = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 184));
            rootBlk.parentDir = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 188));
            rootBlk.extension = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 192));
            rootBlk.sec_type = BigEndianBitConverter.ToUInt32(RootBlockSector, (int)(0x18 + rootBlk.hashTableSize * 4 + 196));

            DicConsole.DebugWriteLine("AmigaDOS plugin", "Stored root block checksum is 0x{0:X8}", rootBlk.checksum);
            DicConsole.DebugWriteLine("AmigaDOS plugin", "Probably incorrect calculated root block checksum is 0x{0:X8}", AmigaChecksum(RootBlockSector));

            switch (bootBlk.diskType & 0xFF)
            {
                case 0:
                    sbInformation.Append("Amiga Original File System");
                    xmlFSType.Type = "Amiga OFS";
                    break;
                case 1:
                    sbInformation.Append("Amiga Fast File System");
                    xmlFSType.Type = "Amiga FFS";
                    break;
                case 2:
                    sbInformation.Append("Amiga Original File System with international characters");
                    xmlFSType.Type = "Amiga OFS";
                    break;
                case 3:
                    sbInformation.Append("Amiga Fast File System with international characters");
                    xmlFSType.Type = "Amiga FFS";
                    break;
                case 4:
                    sbInformation.Append("Amiga Original File System with directory cache");
                    xmlFSType.Type = "Amiga OFS";
                    break;
                case 5:
                    sbInformation.Append("Amiga Fast File System with directory cache");
                    xmlFSType.Type = "Amiga FFS";
                    break;
                case 6:
                    sbInformation.Append("Amiga Original File System with long filenames");
                    xmlFSType.Type = "Amiga OFS";
                    break;
                case 7:
                    sbInformation.Append("Amiga Fast File System with long filenames");
                    xmlFSType.Type = "Amiga FFS";
                    break;
            }

            if ((bootBlk.diskType & 0x6D754600) == 0x6D754600)
                sbInformation.Append(", with multi-user patches");

            sbInformation.AppendLine();

            if ((bootBlk.diskType & 0xFF) == 6 || (bootBlk.diskType & 0xFF) == 7)
            {
                sbInformation.AppendLine("AFFS v2, following information may be completely incorrect or garbage.");
                xmlFSType.Type = "Amiga FFS2";
            }

            sbInformation.AppendFormat("Volume name: {0}", rootBlk.diskName).AppendLine();

            if (rootBlk.bitmapFlag == 0xFFFFFFFF)
                sbInformation.AppendLine("Volume bitmap is valid");

            if (rootBlk.bitmapExtensionBlock != 0x00000000 && rootBlk.bitmapExtensionBlock != 0xFFFFFFFF)
                sbInformation.AppendFormat("Bitmap extension at block {0}", rootBlk.bitmapExtensionBlock).AppendLine();

            if ((bootBlk.diskType & 0xFF) == 4 || (bootBlk.diskType & 0xFF) == 5)
                sbInformation.AppendFormat("Directory cache starts at block {0}", rootBlk.extension).AppendLine();

            sbInformation.AppendFormat("Volume created on {0}", DateHandlers.AmigaToDateTime(rootBlk.cDays, rootBlk.cMins, rootBlk.cTicks)).AppendLine();
            sbInformation.AppendFormat("Volume last modified on {0}", DateHandlers.AmigaToDateTime(rootBlk.vDays, rootBlk.vMins, rootBlk.vTicks)).AppendLine();
            sbInformation.AppendFormat("Volume root directory last modified on on {0}", DateHandlers.AmigaToDateTime(rootBlk.rDays, rootBlk.rMins, rootBlk.rTicks)).AppendLine();

            information = sbInformation.ToString();

            xmlFSType.CreationDate = DateHandlers.AmigaToDateTime(rootBlk.cDays, rootBlk.cMins, rootBlk.cTicks);
            xmlFSType.ModificationDate = DateHandlers.AmigaToDateTime(rootBlk.vDays, rootBlk.vMins, rootBlk.vTicks);
            xmlFSType.Dirty = rootBlk.bitmapFlag != 0xFFFFFFFF;
        }

        static UInt32 AmigaChecksum(byte[] data)
        {
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            UInt32 sum = 0;

            for (int i = 0; i < data.Length; i += 4)
                sum += BigEndianBitConverter.ToUInt32(data, i);

            return sum;
        }
    }
}