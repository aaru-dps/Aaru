// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CP/M filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the CP/M filesystem and shows information.
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.Filesystems.CPM
{
    partial class CPM : Filesystem
    {
        public override bool Identify(ImagePlugin imagePlugin, Partition partition)
        {
            // This will try to identify a CP/M filesystem
            // However as it contains no identification marks whatsoever it's more something of trial-and-error
            // As anything can happen, better try{}catch{} than sorry ;)
            try
            {
                byte[] sector;
                ulong sectorCount;
                ulong sectorSize;
                ulong sides;
                ulong firstDirectorySector;
                byte[] directory = null;
                workingDefinition = null;
                label = null;

                // Try Amstrad superblock
                if(!cpmFound)
                {
                    // Read CHS = {0,0,1}
                    sector = imagePlugin.ReadSector(0 + partition.Start);
                    int amsSbOffset = 0;

                    uint sig1, sig2, sig3;
                    sig1 = BitConverter.ToUInt32(sector, 0x2B);
                    sig2 = BitConverter.ToUInt32(sector, 0x33) & 0x00FFFFFF;
                    sig3 = BitConverter.ToUInt32(sector, 0x7C);

                    // PCW16 extended boot record
                    if(sig1 == 0x4D2F5043 && sig2 == 0x004B5344 && sig3 == sig1)
                        amsSbOffset = 0x80;

                    // Read the superblock
                    AmstradSuperBlock amsSb = new AmstradSuperBlock();
                    IntPtr amsPtr = Marshal.AllocHGlobal(16);
                    Marshal.Copy(sector, amsSbOffset, amsPtr, 16);
                    amsSb = (AmstradSuperBlock)Marshal.PtrToStructure(amsPtr, typeof(AmstradSuperBlock));
                    Marshal.FreeHGlobal(amsPtr);

                    // Check that format byte and sidedness indicate the same number of sizes
                    if((amsSb.format == 0 && (amsSb.sidedness & 0x02) == 0) ||
                       (amsSb.format == 2 && (amsSb.sidedness & 0x02) == 1) ||
                       (amsSb.format == 2 && (amsSb.sidedness & 0x02) == 2))
                    {
                        // Calculate device limits
                        sides = (ulong)(amsSb.format == 0 ? 1 : 2);
                        sectorCount = (ulong)(amsSb.tps * amsSb.spt * (byte)sides);
                        sectorSize = (ulong)(128 << amsSb.psh);

                        // Compare device limits from superblock to real limits
                        if(sectorSize == imagePlugin.GetSectorSize() &&
                           sectorCount == imagePlugin.GetSectors())
                        {
                            cpmFound = true;
                            firstDirectorySector = (ulong)((amsSb.off * amsSb.spt));

                            // Build a DiscParameterBlock
                            dpb = new DiscParameterBlock();
                            dpb.al0 = sectorCount == 1440 ? (byte)0xF0 : (byte)0xC0;
                            dpb.spt = amsSb.spt;
                            dpb.bsh = amsSb.bsh;
                            for(int i = 0; i < dpb.bsh; i++)
                                dpb.blm += (byte)Math.Pow(2, i);
                            if(sectorCount >= 1440)
                            {
                                dpb.cks = 0x40;
                                dpb.drm = 0xFF;
                            }
                            else
                            {
                                dpb.cks = 0x10;
                                dpb.drm = 0x3F;
                            }
                            dpb.dsm = 0; // I don't care
                            dpb.exm = sectorCount == 2880 ? (byte)1 : (byte)0;
                            dpb.off = amsSb.off;
                            dpb.psh = amsSb.psh;
                            for(int i = 0; i < dpb.psh; i++)
                                dpb.phm += (byte)Math.Pow(2, i);
                            dpb.spt = (ushort)(amsSb.spt * (sectorSize / 128));
                            uint directoryLength = (uint)((((ulong)dpb.drm + 1) * 32) / sectorSize);
                            directory = imagePlugin.ReadSectors(firstDirectorySector + partition.Start, directoryLength);

                            // Build a CP/M disk definition
                            workingDefinition = new CpmDefinition();
                            workingDefinition.al0 = dpb.al0;
                            workingDefinition.al1 = dpb.al1;
                            workingDefinition.bitrate = "LOW";
                            workingDefinition.blm = dpb.blm;
                            workingDefinition.bsh = dpb.bsh;
                            workingDefinition.bytesPerSector = 512;
                            workingDefinition.cylinders = amsSb.tps;
                            workingDefinition.drm = dpb.drm;
                            workingDefinition.dsm = dpb.dsm;
                            workingDefinition.encoding = "MFM";
                            workingDefinition.evenOdd = false;
                            workingDefinition.exm = dpb.exm;
                            workingDefinition.label = null;
                            workingDefinition.comment = "Amstrad PCW superblock";
                            workingDefinition.ofs = dpb.off;
                            workingDefinition.sectorsPerTrack = amsSb.spt;

                            workingDefinition.side1 = new Side();
                            workingDefinition.side1.sideId = 0;
                            workingDefinition.side1.sectorIds = new int[amsSb.spt];
                            for(int si = 0; si < amsSb.spt; si++)
                                workingDefinition.side1.sectorIds[si] = si + 1;

                            if(amsSb.format == 2)
                            {
                                if((amsSb.sidedness & 0x02) == 1)
                                    workingDefinition.order = "SIDES";
                                else if((amsSb.sidedness & 0x02) == 2)
                                    workingDefinition.order = "CYLINDERS";
                                else
                                    workingDefinition.order = null;

                                workingDefinition.side2 = new Side();
                                workingDefinition.side2.sideId = 1;
                                workingDefinition.side2.sectorIds = new int[amsSb.spt];
                                for(int si = 0; si < amsSb.spt; si++)
                                    workingDefinition.side2.sectorIds[si] = si + 1;
                            }
                            else
                                workingDefinition.order = null;

                            workingDefinition.skew = 2;
                            workingDefinition.sofs = 0;

                            DicConsole.DebugWriteLine("CP/M Plugin", "Found Amstrad superblock.");
                        }
                    }
                }

                // Try CP/M-86 superblock for hard disks
                if(!cpmFound)
                {
                    // Read CHS = {0,0,4}
                    sector = imagePlugin.ReadSector(3 + partition.Start);
                    ushort sum = 0;

                    // Sum of all 16-bit words that make this sector must be 0
                    for(int i = 0; i < sector.Length; i += 2)
                        sum += BitConverter.ToUInt16(sector, i);

                    // It may happen that there is a corrupted superblock
                    // Better to ignore corrupted than to false positive the rest
                    if(sum == 0)
                    {
                        // Read the superblock
                        HardDiskSuperBlock hddSb = new HardDiskSuperBlock();
                        IntPtr hddPtr = Marshal.AllocHGlobal(Marshal.SizeOf(hddSb));
                        Marshal.Copy(sector, 0, hddPtr, Marshal.SizeOf(hddSb));
                        hddSb = (HardDiskSuperBlock)Marshal.PtrToStructure(hddPtr, typeof(HardDiskSuperBlock));
                        Marshal.FreeHGlobal(hddPtr);

                        // Calculate volume size
                        sectorSize = (ulong)(hddSb.recordsPerSector * 128);
                        ulong sectorsInPartition = (ulong)(hddSb.cylinders * hddSb.heads * hddSb.sectorsPerTrack);
                        ulong startingSector = (ulong)((hddSb.firstCylinder * hddSb.heads + hddSb.heads) * hddSb.sectorsPerTrack);

                        // If volume size corresponds with working partition (this variant will be inside MBR partitioning)
                        if(sectorSize == imagePlugin.GetSectorSize() &&
                           startingSector == partition.Start &&
                           sectorsInPartition + partition.Start <= partition.End)
                        {
                            cpmFound = true;
                            firstDirectorySector = (ulong)((hddSb.off * hddSb.sectorsPerTrack));

                            // Build a DiscParameterBlock
                            dpb = new DiscParameterBlock();
                            dpb.al0 = (byte)hddSb.al0;
                            dpb.al1 = (byte)hddSb.al1;
                            dpb.blm = hddSb.blm;
                            dpb.bsh = hddSb.bsh;
                            dpb.cks = hddSb.cks;
                            dpb.drm = hddSb.drm;
                            dpb.dsm = hddSb.dsm;
                            dpb.exm = hddSb.exm;
                            dpb.off = hddSb.off;
                            dpb.phm = 0; // Needed?
                            dpb.psh = 0; // Needed?
                            dpb.spt = hddSb.spt;
                            uint directoryLength = (uint)((((ulong)dpb.drm + 1) * 32) / sectorSize);
                            directory = imagePlugin.ReadSectors(firstDirectorySector + partition.Start, directoryLength);
                            DicConsole.DebugWriteLine("CP/M Plugin", "Found CP/M-86 hard disk superblock.");

                            // Build a CP/M disk definition
                            workingDefinition = new CpmDefinition();
                            workingDefinition.al0 = dpb.al0;
                            workingDefinition.al1 = dpb.al1;
                            workingDefinition.bitrate = "HIGH";
                            workingDefinition.blm = dpb.blm;
                            workingDefinition.bsh = dpb.bsh;
                            workingDefinition.bytesPerSector = 512;
                            workingDefinition.cylinders = hddSb.cylinders;
                            workingDefinition.drm = dpb.drm;
                            workingDefinition.dsm = dpb.dsm;
                            workingDefinition.encoding = "MFM";
                            workingDefinition.evenOdd = false;
                            workingDefinition.exm = dpb.exm;
                            workingDefinition.label = null;
                            workingDefinition.comment = "CP/M-86 hard disk superblock";
                            workingDefinition.ofs = dpb.off;
                            workingDefinition.sectorsPerTrack = hddSb.sectorsPerTrack;
                            workingDefinition.side1 = new Side();
                            workingDefinition.side1.sideId = 0;
                            workingDefinition.side1.sectorIds = new int[hddSb.sectorsPerTrack];
                            for(int si = 0; si < hddSb.sectorsPerTrack; si++)
                                workingDefinition.side1.sectorIds[si] = si + 1;
                            workingDefinition.order = "SIDES";
                            workingDefinition.side2 = new Side();
                            workingDefinition.side2.sideId = 1;
                            workingDefinition.side2.sectorIds = new int[hddSb.sectorsPerTrack];
                            for(int si = 0; si < hddSb.spt; si++)
                                workingDefinition.side2.sectorIds[si] = si + 1;
                            workingDefinition.skew = 0;
                            workingDefinition.sofs = 0;
                        }
                    }
                }

                // Try CP/M-86 format ID for floppies
                if(!cpmFound)
                {
                    // Read CHS = {0,0,1}
                    sector = imagePlugin.ReadSector(0 + partition.Start);
                    byte formatByte;

                    // Check for alternate location of format ID
                    if(sector.Last() == 0x00 || sector.Last() == 0xFF)
                    {
                        if(sector[0x40] == 0x94 || sector[0x40] == 0x26)
                            formatByte = sector[0x40];
                        else
                            formatByte = sector.Last();
                    }
                    else
                        formatByte = sector.Last();

                    uint firstDirectorySector86 = 0;

                    // Check format ID
                    // If it is one of the known IDs, check disk size corresponds to the one we expect
                    // If so, build a DiscParameterBlock and a CP/M disk definition
                    // Will not work on over-formatted disks (40 cylinder volume on an 80 cylinder disk,
                    // something that happens a lot in IBM PC 5.25" disks)
                    switch((FormatByte)formatByte)
                    {
                        case FormatByte.k160:
                            if(imagePlugin.GetSectorSize() == 512 && imagePlugin.GetSectors() == 320)
                            {
                                cpmFound = true;
                                firstDirectorySector86 = 8;
                                dpb = new DiscParameterBlock();
                                dpb.al0 = 0xC0;
                                dpb.al1 = 0;
                                dpb.blm = 7;
                                dpb.bsh = 3;
                                dpb.cks = 0x10;
                                dpb.drm = 0x3F;
                                dpb.dsm = 0x9B;
                                dpb.exm = 0;
                                dpb.off = 1;
                                dpb.phm = 3;
                                dpb.psh = 2;
                                dpb.spt = 8 * 4;

                                workingDefinition = new CpmDefinition();
                                workingDefinition.al0 = dpb.al0;
                                workingDefinition.al1 = dpb.al1;
                                workingDefinition.bitrate = "LOW";
                                workingDefinition.blm = dpb.blm;
                                workingDefinition.bsh = dpb.bsh;
                                workingDefinition.bytesPerSector = 512;
                                workingDefinition.cylinders = 40;
                                workingDefinition.drm = dpb.drm;
                                workingDefinition.dsm = dpb.dsm;
                                workingDefinition.encoding = "MFM";
                                workingDefinition.evenOdd = false;
                                workingDefinition.exm = dpb.exm;
                                workingDefinition.label = null;
                                workingDefinition.comment = "CP/M-86 floppy identifier";
                                workingDefinition.ofs = dpb.off;
                                workingDefinition.sectorsPerTrack = 8;
                                workingDefinition.side1 = new Side();
                                workingDefinition.side1.sideId = 0;
                                workingDefinition.side1.sectorIds = new int[8];
                                for(int si = 0; si < 8; si++)
                                    workingDefinition.side1.sectorIds[si] = si + 1;
                                workingDefinition.skew = 0;
                                workingDefinition.sofs = 0;
                            }
                            break;
                        case FormatByte.k320:
                            if(imagePlugin.GetSectorSize() == 512 && imagePlugin.GetSectors() == 640)
                            {
                                cpmFound = true;
                                firstDirectorySector86 = 8;
                                dpb = new DiscParameterBlock();
                                dpb.al0 = 0x80;
                                dpb.al1 = 0;
                                dpb.blm = 0x0F;
                                dpb.bsh = 4;
                                dpb.cks = 0x10;
                                dpb.drm = 0x3F;
                                dpb.dsm = 0x9D;
                                dpb.exm = 1;
                                dpb.off = 1;
                                dpb.phm = 3;
                                dpb.psh = 2;
                                dpb.spt = 8 * 4;

                                workingDefinition = new CpmDefinition();
                                workingDefinition.al0 = dpb.al0;
                                workingDefinition.al1 = dpb.al1;
                                workingDefinition.bitrate = "LOW";
                                workingDefinition.blm = dpb.blm;
                                workingDefinition.bsh = dpb.bsh;
                                workingDefinition.bytesPerSector = 512;
                                workingDefinition.cylinders = 40;
                                workingDefinition.drm = dpb.drm;
                                workingDefinition.dsm = dpb.dsm;
                                workingDefinition.encoding = "MFM";
                                workingDefinition.evenOdd = false;
                                workingDefinition.exm = dpb.exm;
                                workingDefinition.label = null;
                                workingDefinition.comment = "CP/M-86 floppy identifier";
                                workingDefinition.ofs = dpb.off;
                                workingDefinition.sectorsPerTrack = 8;
                                workingDefinition.side1 = new Side();
                                workingDefinition.side1.sideId = 0;
                                workingDefinition.side1.sectorIds = new int[8];
                                for(int si = 0; si < 8; si++)
                                    workingDefinition.side1.sectorIds[si] = si + 1;
                                workingDefinition.order = "SIDES";
                                workingDefinition.side2 = new Side();
                                workingDefinition.side2.sideId = 1;
                                workingDefinition.side2.sectorIds = new int[8];
                                for(int si = 0; si < 8; si++)
                                    workingDefinition.side2.sectorIds[si] = si + 1;
                                workingDefinition.skew = 0;
                                workingDefinition.sofs = 0;
                            }
                            break;
                        case FormatByte.k360:
                        case FormatByte.k360Alt:
                        case FormatByte.k360Alt2:
                            if(imagePlugin.GetSectorSize() == 512 && imagePlugin.GetSectors() == 720)
                            {
                                cpmFound = true;
                                firstDirectorySector86 = 36;
                                dpb = new DiscParameterBlock();
                                dpb.al0 = 0x80;
                                dpb.al1 = 0;
                                dpb.blm = 0x0F;
                                dpb.bsh = 4;
                                dpb.cks = 0x10;
                                dpb.drm = 0x3F;
                                dpb.dsm = 0; // Unknown. Needed?
                                dpb.exm = 1;
                                dpb.off = 4;
                                dpb.phm = 3;
                                dpb.psh = 2;
                                dpb.spt = 9 * 4;

                                workingDefinition = new CpmDefinition();
                                workingDefinition.al0 = dpb.al0;
                                workingDefinition.al1 = dpb.al1;
                                workingDefinition.bitrate = "LOW";
                                workingDefinition.blm = dpb.blm;
                                workingDefinition.bsh = dpb.bsh;
                                workingDefinition.bytesPerSector = 512;
                                workingDefinition.cylinders = 40;
                                workingDefinition.drm = dpb.drm;
                                workingDefinition.dsm = dpb.dsm;
                                workingDefinition.encoding = "MFM";
                                workingDefinition.evenOdd = false;
                                workingDefinition.exm = dpb.exm;
                                workingDefinition.label = null;
                                workingDefinition.comment = "CP/M-86 floppy identifier";
                                workingDefinition.ofs = dpb.off;
                                workingDefinition.sectorsPerTrack = 9;
                                workingDefinition.side1 = new Side();
                                workingDefinition.side1.sideId = 0;
                                workingDefinition.side1.sectorIds = new int[9];
                                for(int si = 0; si < 9; si++)
                                    workingDefinition.side1.sectorIds[si] = si + 1;
                                workingDefinition.order = "SIDES";
                                workingDefinition.side2 = new Side();
                                workingDefinition.side2.sideId = 1;
                                workingDefinition.side2.sectorIds = new int[9];
                                for(int si = 0; si < 9; si++)
                                    workingDefinition.side2.sectorIds[si] = si + 1;
                                workingDefinition.skew = 0;
                                workingDefinition.sofs = 0;
                            }
                            break;
                        case FormatByte.k720:
                        case FormatByte.k720Alt:
                            if(imagePlugin.GetSectorSize() == 512 && imagePlugin.GetSectors() == 1440)
                            {
                                cpmFound = true;
                                firstDirectorySector86 = 36;
                                dpb = new DiscParameterBlock();
                                dpb.al0 = 0xF0;
                                dpb.al1 = 0;
                                dpb.blm = 0x0F;
                                dpb.bsh = 4;
                                dpb.cks = 0x40;
                                dpb.drm = 0xFF;
                                dpb.dsm = 0x15E;
                                dpb.exm = 0;
                                dpb.off = 4;
                                dpb.phm = 3;
                                dpb.psh = 2;
                                dpb.spt = 9 * 4;

                                workingDefinition = new CpmDefinition();
                                workingDefinition.al0 = dpb.al0;
                                workingDefinition.al1 = dpb.al1;
                                workingDefinition.bitrate = "LOW";
                                workingDefinition.blm = dpb.blm;
                                workingDefinition.bsh = dpb.bsh;
                                workingDefinition.bytesPerSector = 512;
                                workingDefinition.cylinders = 80;
                                workingDefinition.drm = dpb.drm;
                                workingDefinition.dsm = dpb.dsm;
                                workingDefinition.encoding = "MFM";
                                workingDefinition.evenOdd = false;
                                workingDefinition.exm = dpb.exm;
                                workingDefinition.label = null;
                                workingDefinition.comment = "CP/M-86 floppy identifier";
                                workingDefinition.ofs = dpb.off;
                                workingDefinition.sectorsPerTrack = 9;
                                workingDefinition.side1 = new Side();
                                workingDefinition.side1.sideId = 0;
                                workingDefinition.side1.sectorIds = new int[9];
                                for(int si = 0; si < 9; si++)
                                    workingDefinition.side1.sectorIds[si] = si + 1;
                                workingDefinition.order = "SIDES";
                                workingDefinition.side2 = new Side();
                                workingDefinition.side2.sideId = 1;
                                workingDefinition.side2.sectorIds = new int[9];
                                for(int si = 0; si < 9; si++)
                                    workingDefinition.side2.sectorIds[si] = si + 1;
                                workingDefinition.skew = 0;
                                workingDefinition.sofs = 0;
                            }
                            break;
                        case FormatByte.f720:
                            if(imagePlugin.GetSectorSize() == 512 && imagePlugin.GetSectors() == 1440)
                            {
                                cpmFound = true;
                                firstDirectorySector86 = 18;
                                dpb = new DiscParameterBlock();
                                dpb.al0 = 0xF0;
                                dpb.al1 = 0;
                                dpb.blm = 0x0F;
                                dpb.bsh = 4;
                                dpb.cks = 0x40;
                                dpb.drm = 0xFF;
                                dpb.dsm = 0x162;
                                dpb.exm = 0;
                                dpb.off = 2;
                                dpb.phm = 3;
                                dpb.psh = 2;
                                dpb.spt = 9 * 4;

                                workingDefinition = new CpmDefinition();
                                workingDefinition.al0 = dpb.al0;
                                workingDefinition.al1 = dpb.al1;
                                workingDefinition.bitrate = "LOW";
                                workingDefinition.blm = dpb.blm;
                                workingDefinition.bsh = dpb.bsh;
                                workingDefinition.bytesPerSector = 512;
                                workingDefinition.cylinders = 80;
                                workingDefinition.drm = dpb.drm;
                                workingDefinition.dsm = dpb.dsm;
                                workingDefinition.encoding = "MFM";
                                workingDefinition.evenOdd = false;
                                workingDefinition.exm = dpb.exm;
                                workingDefinition.label = null;
                                workingDefinition.comment = "CP/M-86 floppy identifier";
                                workingDefinition.ofs = dpb.off;
                                workingDefinition.sectorsPerTrack = 9;
                                workingDefinition.side1 = new Side();
                                workingDefinition.side1.sideId = 0;
                                workingDefinition.side1.sectorIds = new int[9];
                                for(int si = 0; si < 9; si++)
                                    workingDefinition.side1.sectorIds[si] = si + 1;
                                workingDefinition.order = "CYLINDERS";
                                workingDefinition.side2 = new Side();
                                workingDefinition.side2.sideId = 1;
                                workingDefinition.side2.sectorIds = new int[9];
                                for(int si = 0; si < 9; si++)
                                    workingDefinition.side2.sectorIds[si] = si + 1;
                                workingDefinition.skew = 0;
                                workingDefinition.sofs = 0;
                            }
                            break;
                        case FormatByte.f1200:
                            if(imagePlugin.GetSectorSize() == 512 && imagePlugin.GetSectors() == 2400)
                            {
                                cpmFound = true;
                                firstDirectorySector86 = 30;
                                dpb = new DiscParameterBlock();
                                dpb.al0 = 0xC0;
                                dpb.al1 = 0;
                                dpb.blm = 0x1F;
                                dpb.bsh = 5;
                                dpb.cks = 0x40;
                                dpb.drm = 0xFF;
                                dpb.dsm = 0x127;
                                dpb.exm = 1;
                                dpb.off = 2;
                                dpb.phm = 3;
                                dpb.psh = 2;
                                dpb.spt = 15 * 4;

                                workingDefinition = new CpmDefinition();
                                workingDefinition.al0 = dpb.al0;
                                workingDefinition.al1 = dpb.al1;
                                workingDefinition.bitrate = "HIGH";
                                workingDefinition.blm = dpb.blm;
                                workingDefinition.bsh = dpb.bsh;
                                workingDefinition.bytesPerSector = 512;
                                workingDefinition.cylinders = 80;
                                workingDefinition.drm = dpb.drm;
                                workingDefinition.dsm = dpb.dsm;
                                workingDefinition.encoding = "MFM";
                                workingDefinition.evenOdd = false;
                                workingDefinition.exm = dpb.exm;
                                workingDefinition.label = null;
                                workingDefinition.comment = "CP/M-86 floppy identifier";
                                workingDefinition.ofs = dpb.off;
                                workingDefinition.sectorsPerTrack = 15;
                                workingDefinition.side1 = new Side();
                                workingDefinition.side1.sideId = 0;
                                workingDefinition.side1.sectorIds = new int[15];
                                for(int si = 0; si < 15; si++)
                                    workingDefinition.side1.sectorIds[si] = si + 1;
                                workingDefinition.order = "CYLINDERS";
                                workingDefinition.side2 = new Side();
                                workingDefinition.side2.sideId = 1;
                                workingDefinition.side2.sectorIds = new int[15];
                                for(int si = 0; si < 15; si++)
                                    workingDefinition.side2.sectorIds[si] = si + 1;
                                workingDefinition.skew = 0;
                                workingDefinition.sofs = 0;
                            }
                            break;
                        case FormatByte.f1440:
                            if(imagePlugin.GetSectorSize() == 512 && imagePlugin.GetSectors() == 2880)
                            {
                                cpmFound = true;
                                firstDirectorySector86 = 36;
                                dpb = new DiscParameterBlock();
                                dpb.al0 = 0xC0;
                                dpb.al1 = 0;
                                dpb.blm = 0x1F;
                                dpb.bsh = 5;
                                dpb.cks = 0x40;
                                dpb.drm = 0xFF;
                                dpb.dsm = 0x162;
                                dpb.exm = 1;
                                dpb.off = 2;
                                dpb.phm = 3;
                                dpb.psh = 2;
                                dpb.spt = 18 * 4;

                                workingDefinition = new CpmDefinition();
                                workingDefinition.al0 = dpb.al0;
                                workingDefinition.al1 = dpb.al1;
                                workingDefinition.bitrate = "LOW";
                                workingDefinition.blm = dpb.blm;
                                workingDefinition.bsh = dpb.bsh;
                                workingDefinition.bytesPerSector = 512;
                                workingDefinition.cylinders = 80;
                                workingDefinition.drm = dpb.drm;
                                workingDefinition.dsm = dpb.dsm;
                                workingDefinition.encoding = "MFM";
                                workingDefinition.evenOdd = false;
                                workingDefinition.exm = dpb.exm;
                                workingDefinition.label = null;
                                workingDefinition.comment = "CP/M-86 floppy identifier";
                                workingDefinition.ofs = dpb.off;
                                workingDefinition.sectorsPerTrack = 18;
                                workingDefinition.side1 = new Side();
                                workingDefinition.side1.sideId = 0;
                                workingDefinition.side1.sectorIds = new int[18];
                                for(int si = 0; si < 18; si++)
                                    workingDefinition.side1.sectorIds[si] = si + 1;
                                workingDefinition.order = "CYLINDERS";
                                workingDefinition.side2 = new Side();
                                workingDefinition.side2.sideId = 1;
                                workingDefinition.side2.sectorIds = new int[18];
                                for(int si = 0; si < 18; si++)
                                    workingDefinition.side2.sectorIds[si] = si + 1;
                                workingDefinition.skew = 0;
                                workingDefinition.sofs = 0;
                            }
                            break;
                    }

                    if(cpmFound)
                    {
                        uint directoryLength = (uint)((((ulong)dpb.drm + 1) * 32) / imagePlugin.GetSectorSize());
                        directory = imagePlugin.ReadSectors(firstDirectorySector86 + partition.Start, directoryLength);
                        DicConsole.DebugWriteLine("CP/M Plugin", "Found CP/M-86 floppy identifier.");
                    }
                }

                // One of the few CP/M filesystem marks has been found, try for correcteness checking the whole directory
                if(cpmFound)
                {
                    if(CheckDir(directory))
                    {
                        DicConsole.DebugWriteLine("CP/M Plugin", "First directory block seems correct.");
                        return true;
                    }

                    cpmFound = false;
                }

                // Try all definitions
                if(!cpmFound)
                {
                    // Load all definitions
                    DicConsole.DebugWriteLine("CP/M Plugin", "Trying to load definitions.");
                    if(LoadDefinitions() && definitions != null && definitions.definitions != null && definitions.definitions.Count > 0)
                    {
                        DicConsole.DebugWriteLine("CP/M Plugin", "Trying all known definitions.");
                        foreach(CpmDefinition def in definitions.definitions)
                        {
                            ulong sectors = (ulong)(def.cylinders * def.sides * def.sectorsPerTrack);

                            // Definition seems to describe current disk, at least, same number of volume sectors and bytes per sector
                            if(sectors == imagePlugin.GetSectors() && def.bytesPerSector == imagePlugin.GetSectorSize())
                            {
                                DicConsole.DebugWriteLine("CP/M Plugin", "Trying definition \"{0}\"", def.comment);
                                ulong offset;
                                if(def.sofs != 0)
                                    offset = (ulong)def.sofs;
                                else
                                    offset = (ulong)(def.ofs * def.sectorsPerTrack);

                                int dirLen = ((def.drm + 1) * 32) / def.bytesPerSector;

                                if(def.sides == 1)
                                {
                                    sectorMask = new int[def.side1.sectorIds.Length];
                                    for(int m = 0; m < sectorMask.Length; m++)
                                        sectorMask[m] = def.side1.sectorIds[m] - def.side1.sectorIds[0];
                                }
                                else
                                {
                                    // Head changes after every track
                                    if(string.Compare(def.order, "SIDES", StringComparison.InvariantCultureIgnoreCase) == 0)
                                    {
                                        sectorMask = new int[def.side1.sectorIds.Length + def.side2.sectorIds.Length];
                                        for(int m = 0; m < def.side1.sectorIds.Length; m++)
                                            sectorMask[m] = def.side1.sectorIds[m] - def.side1.sectorIds[0];
                                        // Skip first track (first side)
                                        for(int m = 0; m < def.side2.sectorIds.Length; m++)
                                            sectorMask[m + def.side1.sectorIds.Length] = (def.side2.sectorIds[m] - def.side2.sectorIds[0]) + def.side1.sectorIds.Length;
                                    }
                                    // Head changes after whole side
                                    else if(string.Compare(def.order, "CYLINDERS", StringComparison.InvariantCultureIgnoreCase) == 0)
                                    {
                                        for(int m = 0; m < def.side1.sectorIds.Length; m++)
                                            sectorMask[m] = def.side1.sectorIds[m] - def.side1.sectorIds[0];
                                        // Skip first track (first side) and first track (second side)
                                        for(int m = 0; m < def.side1.sectorIds.Length; m++)
                                            sectorMask[m + def.side1.sectorIds.Length] = (def.side1.sectorIds[m] - def.side1.sectorIds[0]) + def.side1.sectorIds.Length + def.side2.sectorIds.Length;
                                    }
                                    // TODO: Implement COLUMBIA ordering
                                    else if(string.Compare(def.order, "COLUMBIA", StringComparison.InvariantCultureIgnoreCase) == 0)
                                    {
                                        DicConsole.DebugWriteLine("CP/M Plugin", "Don't know how to handle COLUMBIA ordering, not proceeding with this definition.");
                                        continue;
                                    }
                                    // TODO: Implement EAGLE ordering
                                    else if(string.Compare(def.order, "EAGLE", StringComparison.InvariantCultureIgnoreCase) == 0)
                                    {
                                        DicConsole.DebugWriteLine("CP/M Plugin", "Don't know how to handle EAGLE ordering, not proceeding with this definition.");
                                        continue;
                                    }
                                    else
                                    {
                                        DicConsole.DebugWriteLine("CP/M Plugin", "Unknown order type \"{0}\", not proceeding with this definition.", def.order);
                                        continue;
                                    }
                                }

                                // Read the directory marked by this definition
                                MemoryStream ms = new MemoryStream();
                                for(int p = 0; p < dirLen; p++)
                                {
                                    byte[] dirSector = imagePlugin.ReadSector((ulong)((int)offset + (int)partition.Start + (p / sectorMask.Length) * sectorMask.Length + sectorMask[p % sectorMask.Length]));
                                    ms.Write(dirSector, 0, dirSector.Length);
                                }
                                directory = ms.ToArray();

                                if(def.evenOdd)
                                    DicConsole.DebugWriteLine("CP/M Plugin", "Definition contains EVEN-ODD field, with unknown meaning, detection may be wrong.");

                                // Complement of the directory bytes if needed
                                if(def.complement)
                                {
                                    for(int b = 0; b < directory.Length; b++)
                                        directory[b] = (byte)(~directory[b] & 0xFF);
                                }

                                // Check the directory
                                if(CheckDir(directory))
                                {
                                    DicConsole.DebugWriteLine("CP/M Plugin", "Definition \"{0}\" has a correct directory", def.comment);

                                    // Build a Disc Parameter Block
                                    workingDefinition = def;
                                    dpb = new DiscParameterBlock();
                                    dpb.al0 = (byte)def.al0;
                                    dpb.al1 = (byte)def.al1;
                                    dpb.blm = (byte)def.blm;
                                    dpb.bsh = (byte)def.bsh;
                                    dpb.cks = 0; // Needed?
                                    dpb.drm = (ushort)def.drm;
                                    dpb.dsm = (ushort)def.dsm;
                                    dpb.exm = (byte)def.exm;
                                    dpb.off = (ushort)def.ofs;
                                    switch(def.bytesPerSector)
                                    {
                                        case 128:
                                            dpb.psh = 0;
                                            dpb.phm = 0;
                                            break;
                                        case 256:
                                            dpb.psh = 1;
                                            dpb.phm = 1;
                                            break;
                                        case 512:
                                            dpb.psh = 2;
                                            dpb.phm = 3;
                                            break;
                                        case 1024:
                                            dpb.psh = 3;
                                            dpb.phm = 7;
                                            break;
                                        case 2048:
                                            dpb.psh = 4;
                                            dpb.phm = 15;
                                            break;
                                        case 4096:
                                            dpb.psh = 5;
                                            dpb.phm = 31;
                                            break;
                                        case 8192:
                                            dpb.psh = 6;
                                            dpb.phm = 63;
                                            break;
                                        case 16384:
                                            dpb.psh = 7;
                                            dpb.phm = 127;
                                            break;
                                        case 32768:
                                            dpb.psh = 8;
                                            dpb.phm = 255;
                                            break;
                                    }
                                    dpb.spt = (ushort)((def.sectorsPerTrack * def.bytesPerSector) / 128);
                                    cpmFound = true;
                                    workingDefinition = def;

                                    return true;
                                }

                                label = null;
                                labelCreationDate = null;
                                labelUpdateDate = null;
                            }
                        }
                    }
                }

                // Clear class variables
                cpmFound = false;
                workingDefinition = null;
                dpb = null;
                label = null;
                standardTimestamps = false;
                thirdPartyTimestamps = false;
                return false;
            }
            catch
            {
                //throw ex;
                return false;
            }
        }

        public override void GetInformation(ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";
            // As the identification is so complex, just call Identify() and relay on its findings
            if(!Identify(imagePlugin, partition) || !cpmFound || workingDefinition == null || dpb == null)
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("CP/M filesystem");
            if(!string.IsNullOrEmpty(workingDefinition.comment))
                sb.AppendFormat("Identified as {0}", workingDefinition.comment).AppendLine();
            sb.AppendFormat("Volume block is {0} bytes", 128 << dpb.bsh).AppendLine();
            if(dpb.dsm > 0)
                sb.AppendFormat("Volume contains {0} blocks ({1} bytes)", dpb.dsm, (dpb.dsm) * (128 << dpb.bsh)).AppendLine();
            sb.AppendFormat("Volume contains {0} directory entries", dpb.drm + 1).AppendLine();
            if(workingDefinition.sofs > 0)
                sb.AppendFormat("Volume reserves {0} sectors for system", workingDefinition.sofs).AppendLine();
            else
                sb.AppendFormat("Volume reserves {1} tracks ({0} sectors) for system", workingDefinition.ofs * workingDefinition.sectorsPerTrack, workingDefinition.ofs).AppendLine();

            int interleaveSide1;
            int interleaveSide2 = 1;

            if(workingDefinition.side1.sectorIds.Length >= 2)
            {
                interleaveSide1 = workingDefinition.side1.sectorIds[1] - workingDefinition.side1.sectorIds[0];
                if(interleaveSide1 > 1)
                    sb.AppendFormat("Side 0 uses {0}:1 software interleaving", interleaveSide1).AppendLine();
            }
            
            if(workingDefinition.sides == 2)
            {
                if(workingDefinition.side2.sectorIds.Length >= 2)
                {
                    interleaveSide2 = workingDefinition.side2.sectorIds[1] - workingDefinition.side2.sectorIds[0];
                    if(interleaveSide2 > 1)
                        sb.AppendFormat("Side 1 uses {0}:1 software interleaving", interleaveSide2).AppendLine();
                }
                switch(workingDefinition.order)
                {
                    case "SIDES":
                        sb.AppendLine("Head changes after each whole track");
                        break;
                    case "CYLINDERS":
                        sb.AppendLine("Head changes after whole side");
                        break;
                    default:
                        sb.AppendFormat("Unknown how {0} side ordering works", workingDefinition.order).AppendLine();
                        break;
                }
            }

            if(workingDefinition.skew > 0)
                sb.AppendFormat("Device uses {0}:1 hardware interleaving", workingDefinition.skew).AppendLine();

            if(workingDefinition.sofs > 0)
                sb.AppendFormat("BSH {0} BLM {1} EXM {2} DSM {3} DRM {4} AL0 {5:X2}H AL1 {6:X2}H SOFS {7}", dpb.bsh, dpb.blm, dpb.exm, dpb.dsm, dpb.drm, dpb.al0, dpb.al1, workingDefinition.sofs).AppendLine();
            else
                sb.AppendFormat("BSH {0} BLM {1} EXM {2} DSM {3} DRM {4} AL0 {5:X2}H AL1 {6:X2}H OFS {7}", dpb.bsh, dpb.blm, dpb.exm, dpb.dsm, dpb.drm, dpb.al0, dpb.al1, workingDefinition.ofs).AppendLine();

            if(label != null)
                sb.AppendFormat("Volume label {0}", label).AppendLine();

            if(standardTimestamps)
                sb.AppendLine("Volume uses standard CP/M timestamps");

            if(thirdPartyTimestamps)
                sb.AppendLine("Volume uses third party timestamps");

            if(labelCreationDate != null)
                sb.AppendFormat("Volume created on {0}", DateHandlers.CPMToDateTime(labelCreationDate)).AppendLine();
            if(labelUpdateDate != null)
                sb.AppendFormat("Volume updated on {0}", DateHandlers.CPMToDateTime(labelUpdateDate)).AppendLine();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Bootable |= (workingDefinition.sofs > 0 || workingDefinition.ofs > 0);
            xmlFSType.ClusterSize = 128 << dpb.bsh;
            if(dpb.dsm > 0)
                xmlFSType.Clusters = ((dpb.dsm + 1) * 128) / (128 << dpb.bsh);
            else
                xmlFSType.Clusters = (long)(partition.End - partition.Start);
            if(labelCreationDate != null)
            {
                xmlFSType.CreationDate = DateHandlers.CPMToDateTime(labelCreationDate);
                xmlFSType.CreationDateSpecified = true;
            }
            if(labelUpdateDate != null)
            {
                xmlFSType.ModificationDate = DateHandlers.CPMToDateTime(labelUpdateDate);
                xmlFSType.ModificationDateSpecified = true;
            }
            xmlFSType.Type = "CP/M";
            xmlFSType.VolumeName = label;

            information = sb.ToString();
        }
    }
}

