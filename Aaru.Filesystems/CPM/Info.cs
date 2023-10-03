// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class CPM
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        // This will only continue on devices with a chance to have ever been used by CP/M while failing on all others
        // It's ugly, but will stop a lot of false positives
        switch(imagePlugin.Info.MediaType)
        {
            case MediaType.Unknown:
            case MediaType.Apple32SS:
            case MediaType.Apple32DS:
            case MediaType.Apple33SS:
            case MediaType.Apple33DS:
            case MediaType.DOS_525_SS_DD_8:
            case MediaType.DOS_525_SS_DD_9:
            case MediaType.DOS_525_DS_DD_8:
            case MediaType.DOS_525_DS_DD_9:
            case MediaType.DOS_525_HD:
            case MediaType.DOS_35_SS_DD_8:
            case MediaType.DOS_35_SS_DD_9:
            case MediaType.DOS_35_DS_DD_8:
            case MediaType.DOS_35_DS_DD_9:
            case MediaType.DOS_35_HD:
            case MediaType.DOS_35_ED:
            case MediaType.IBM23FD:
            case MediaType.IBM33FD_128:
            case MediaType.IBM33FD_256:
            case MediaType.IBM33FD_512:
            case MediaType.IBM43FD_128:
            case MediaType.IBM43FD_256:
            case MediaType.IBM53FD_256:
            case MediaType.IBM53FD_512:
            case MediaType.IBM53FD_1024:
            case MediaType.RX01:
            case MediaType.RX02:
            case MediaType.RX03:
            case MediaType.RX50:
            case MediaType.ACORN_525_SS_SD_40:
            case MediaType.ACORN_525_SS_SD_80:
            case MediaType.ACORN_525_SS_DD_40:
            case MediaType.ACORN_525_SS_DD_80:
            case MediaType.ACORN_525_DS_DD:
            case MediaType.ATARI_525_SD:
            case MediaType.ATARI_525_ED:
            case MediaType.ATARI_525_DD:
            case MediaType.CBM_35_DD:
            case MediaType.CBM_1540:
            case MediaType.CBM_1540_Ext:
            case MediaType.CBM_1571:
            case MediaType.NEC_8_SD:
            case MediaType.NEC_8_DD:
            case MediaType.NEC_525_SS:
            case MediaType.NEC_525_DS:
            case MediaType.NEC_525_HD:
            case MediaType.NEC_35_HD_8:
            case MediaType.NEC_35_HD_15:
            case MediaType.SHARP_525_9:
            case MediaType.SHARP_35_9:
            case MediaType.ECMA_99_8:
            case MediaType.ECMA_99_15:
            case MediaType.ECMA_99_26:
            case MediaType.ECMA_54:
            case MediaType.ECMA_59:
            case MediaType.ECMA_66:
            case MediaType.ECMA_69_8:
            case MediaType.ECMA_69_15:
            case MediaType.ECMA_69_26:
            case MediaType.ECMA_70:
            case MediaType.ECMA_78:
            case MediaType.ECMA_78_2:
            case MediaType.Apricot_35:
            case MediaType.CompactFloppy:
            case MediaType.DemiDiskette:
            case MediaType.QuickDisk:
            case MediaType.Wafer:
            case MediaType.ZXMicrodrive:
            case MediaType.AppleProfile:
            case MediaType.AppleWidget:
            case MediaType.AppleHD20:
            case MediaType.RA60:
            case MediaType.RA80:
            case MediaType.RA81:
            case MediaType.RC25:
            case MediaType.RD31:
            case MediaType.RD32:
            case MediaType.RD51:
            case MediaType.RD52:
            case MediaType.RD53:
            case MediaType.RD54:
            case MediaType.RK06:
            case MediaType.RK06_18:
            case MediaType.RK07:
            case MediaType.RK07_18:
            case MediaType.RM02:
            case MediaType.RM03:
            case MediaType.RM05:
            case MediaType.RP02:
            case MediaType.RP02_18:
            case MediaType.RP03:
            case MediaType.RP03_18:
            case MediaType.RP04:
            case MediaType.RP04_18:
            case MediaType.RP05:
            case MediaType.RP05_18:
            case MediaType.RP06:
            case MediaType.RP06_18:
            case MediaType.GENERIC_HDD:
            case MediaType.FlashDrive:
            case MediaType.MetaFloppy_Mod_I:
            case MediaType.MetaFloppy_Mod_II: break;
            default: return false;
        }

        // This will try to identify a CP/M filesystem
        // However as it contains no identification marks whatsoever it's more something of trial-and-error
        // As anything can happen, better try{}catch{} than sorry ;)
        try
        {
            byte[] sector;
            ulong  sectorSize;
            ulong  firstDirectorySector;
            byte[] directory = null;
            _workingDefinition = null;
            _label             = null;

            // Try Amstrad superblock
            ErrorNumber errno;

            if(!_cpmFound)
            {
                // Read CHS = {0,0,1}
                errno = imagePlugin.ReadSector(0 + partition.Start, out sector);

                if(errno == ErrorNumber.NoError)
                {
                    int amsSbOffset = 0;

                    uint sig1 = BitConverter.ToUInt32(sector, 0x2B);
                    uint sig2 = BitConverter.ToUInt32(sector, 0x33) & 0x00FFFFFF;
                    uint sig3 = BitConverter.ToUInt32(sector, 0x7C);

                    // PCW16 extended boot record
                    if(sig1 == 0x4D2F5043 &&
                       sig2 == 0x004B5344 &&
                       sig3 == sig1)
                        amsSbOffset = 0x80;

                    // Read the superblock
                    AmstradSuperBlock amsSb =
                        Marshal.ByteArrayToStructureLittleEndian<AmstradSuperBlock>(sector, amsSbOffset, 16);

                    // Check that format byte and sidedness indicate the same number of sizes
                    if((amsSb.format == 0 && (amsSb.sidedness & 0x02) == 0) ||
                       (amsSb.format == 2 && (amsSb.sidedness & 0x02) == 1) ||
                       (amsSb.format == 2 && (amsSb.sidedness & 0x02) == 2))
                    {
                        // Calculate device limits
                        ulong sides       = (ulong)(amsSb.format == 0 ? 1 : 2);
                        ulong sectorCount = (ulong)(amsSb.tps * amsSb.spt * (byte)sides);
                        sectorSize = (ulong)(128 << amsSb.psh);

                        // Compare device limits from superblock to real limits
                        if(sectorSize  == imagePlugin.Info.SectorSize &&
                           sectorCount == imagePlugin.Info.Sectors)
                        {
                            _cpmFound            = true;
                            firstDirectorySector = (ulong)(amsSb.off * amsSb.spt);

                            // Build a DiscParameterBlock
                            _dpb = new DiscParameterBlock
                            {
                                al0 = sectorCount == 1440 ? (byte)0xF0 : (byte)0xC0,
                                spt = amsSb.spt,
                                bsh = amsSb.bsh
                            };

                            for(int i = 0; i < _dpb.bsh; i++)
                                _dpb.blm += (byte)Math.Pow(2, i);

                            if(sectorCount >= 1440)
                            {
                                _dpb.cks = 0x40;
                                _dpb.drm = 0xFF;
                            }
                            else
                            {
                                _dpb.cks = 0x10;
                                _dpb.drm = 0x3F;
                            }

                            _dpb.dsm = 0; // I don't care
                            _dpb.exm = sectorCount == 2880 ? (byte)1 : (byte)0;
                            _dpb.off = amsSb.off;
                            _dpb.psh = amsSb.psh;

                            for(int i = 0; i < _dpb.psh; i++)
                                _dpb.phm += (byte)Math.Pow(2, i);

                            _dpb.spt = (ushort)(amsSb.spt * (sectorSize              / 128));
                            uint directoryLength = (uint)(((ulong)_dpb.drm + 1) * 32 / sectorSize);

                            imagePlugin.ReadSectors(firstDirectorySector + partition.Start, directoryLength,
                                                    out directory);

                            // Build a CP/M disk definition
                            _workingDefinition = new CpmDefinition
                            {
                                al0             = _dpb.al0,
                                al1             = _dpb.al1,
                                bitrate         = "LOW",
                                blm             = _dpb.blm,
                                bsh             = _dpb.bsh,
                                bytesPerSector  = 512,
                                cylinders       = amsSb.tps,
                                drm             = _dpb.drm,
                                dsm             = _dpb.dsm,
                                encoding        = "MFM",
                                evenOdd         = false,
                                exm             = _dpb.exm,
                                label           = null,
                                comment         = "Amstrad PCW superblock",
                                ofs             = _dpb.off,
                                sectorsPerTrack = amsSb.spt,
                                side1 = new Side
                                {
                                    sideId    = 0,
                                    sectorIds = new int[amsSb.spt]
                                }
                            };

                            for(int si = 0; si < amsSb.spt; si++)
                                _workingDefinition.side1.sectorIds[si] = si + 1;

                            if(amsSb.format == 2)
                            {
                                _workingDefinition.order = (amsSb.sidedness & 0x02) switch
                                {
                                    1 => "SIDES",
                                    2 => "CYLINDERS",
                                    _ => null
                                };

                                _workingDefinition.side2 = new Side
                                {
                                    sideId    = 1,
                                    sectorIds = new int[amsSb.spt]
                                };

                                for(int si = 0; si < amsSb.spt; si++)
                                    _workingDefinition.side2.sectorIds[si] = si + 1;
                            }
                            else
                                _workingDefinition.order = null;

                            _workingDefinition.skew = 2;
                            _workingDefinition.sofs = 0;

                            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_Amstrad_superblock);
                        }
                    }
                }
            }

            // Try CP/M-86 superblock for hard disks
            if(!_cpmFound)
            {
                // Read CHS = {0,0,4}
                errno = imagePlugin.ReadSector(3 + partition.Start, out sector);

                if(errno == ErrorNumber.NoError)
                {
                    ushort sum = 0;

                    // Sum of all 16-bit words that make this sector must be 0
                    for(int i = 0; i < sector.Length; i += 2)
                        sum += BitConverter.ToUInt16(sector, i);

                    // It may happen that there is a corrupted superblock
                    // Better to ignore corrupted than to false positive the rest
                    if(sum == 0)
                    {
                        // Read the superblock
                        HardDiskSuperBlock hddSb = Marshal.ByteArrayToStructureLittleEndian<HardDiskSuperBlock>(sector);

                        // Calculate volume size
                        sectorSize = (ulong)(hddSb.recordsPerSector * 128);
                        ulong sectorsInPartition = (ulong)(hddSb.cylinders * hddSb.heads * hddSb.sectorsPerTrack);

                        ulong startingSector =
                            (ulong)(((hddSb.firstCylinder * hddSb.heads) + hddSb.heads) * hddSb.sectorsPerTrack);

                        // If volume size corresponds with working partition (this variant will be inside MBR partitioning)
                        if(sectorSize                           == imagePlugin.Info.SectorSize &&
                           startingSector                       == partition.Start             &&
                           sectorsInPartition + partition.Start <= partition.End)
                        {
                            _cpmFound            = true;
                            firstDirectorySector = (ulong)(hddSb.off * hddSb.sectorsPerTrack);

                            // Build a DiscParameterBlock
                            _dpb = new DiscParameterBlock
                            {
                                al0 = (byte)hddSb.al0,
                                al1 = (byte)hddSb.al1,
                                blm = hddSb.blm,
                                bsh = hddSb.bsh,
                                cks = hddSb.cks,
                                drm = hddSb.drm,
                                dsm = hddSb.dsm,
                                exm = hddSb.exm,
                                off = hddSb.off,

                                // Needed?
                                phm = 0,

                                // Needed?
                                psh = 0,
                                spt = hddSb.spt
                            };

                            uint directoryLength = (uint)(((ulong)_dpb.drm + 1) * 32 / sectorSize);

                            imagePlugin.ReadSectors(firstDirectorySector + partition.Start, directoryLength,
                                                    out directory);

                            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_CPM_86_hard_disk_superblock);

                            // Build a CP/M disk definition
                            _workingDefinition = new CpmDefinition
                            {
                                al0             = _dpb.al0,
                                al1             = _dpb.al1,
                                bitrate         = "HIGH",
                                blm             = _dpb.blm,
                                bsh             = _dpb.bsh,
                                bytesPerSector  = 512,
                                cylinders       = hddSb.cylinders,
                                drm             = _dpb.drm,
                                dsm             = _dpb.dsm,
                                encoding        = "MFM",
                                evenOdd         = false,
                                exm             = _dpb.exm,
                                label           = null,
                                comment         = "CP/M-86 hard disk superblock",
                                ofs             = _dpb.off,
                                sectorsPerTrack = hddSb.sectorsPerTrack,
                                side1 = new Side
                                {
                                    sideId    = 0,
                                    sectorIds = new int[hddSb.sectorsPerTrack]
                                },
                                order = "SIDES",
                                side2 = new Side
                                {
                                    sideId    = 1,
                                    sectorIds = new int[hddSb.sectorsPerTrack]
                                },
                                skew = 0,
                                sofs = 0
                            };

                            for(int si = 0; si < hddSb.sectorsPerTrack; si++)
                                _workingDefinition.side1.sectorIds[si] = si + 1;

                            for(int si = 0; si < hddSb.spt; si++)
                                _workingDefinition.side2.sectorIds[si] = si + 1;
                        }
                    }
                }
            }

            // Try CP/M-86 format ID for floppies
            if(!_cpmFound)
            {
                // Read CHS = {0,0,1}
                errno = imagePlugin.ReadSector(0 + partition.Start, out sector);

                if(errno == ErrorNumber.NoError)
                {
                    byte formatByte;

                    // Check for alternate location of format ID
                    if(sector.Last() == 0x00 ||
                       sector.Last() == 0xFF)
                        if(sector[0x40] == 0x94 ||
                           sector[0x40] == 0x26)
                            formatByte = sector[0x40];
                        else
                            formatByte = sector.Last();
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
                            if(imagePlugin.Info is { SectorSize: 512, Sectors: 320 })
                            {
                                _cpmFound              = true;
                                firstDirectorySector86 = 8;

                                _dpb = new DiscParameterBlock
                                {
                                    al0 = 0xC0,
                                    al1 = 0,
                                    blm = 7,
                                    bsh = 3,
                                    cks = 0x10,
                                    drm = 0x3F,
                                    dsm = 0x9B,
                                    exm = 0,
                                    off = 1,
                                    phm = 3,
                                    psh = 2,
                                    spt = 8 * 4
                                };

                                _workingDefinition = new CpmDefinition
                                {
                                    al0             = _dpb.al0,
                                    al1             = _dpb.al1,
                                    bitrate         = "LOW",
                                    blm             = _dpb.blm,
                                    bsh             = _dpb.bsh,
                                    bytesPerSector  = 512,
                                    cylinders       = 40,
                                    drm             = _dpb.drm,
                                    dsm             = _dpb.dsm,
                                    encoding        = "MFM",
                                    evenOdd         = false,
                                    exm             = _dpb.exm,
                                    label           = null,
                                    comment         = "CP/M-86 floppy identifier",
                                    ofs             = _dpb.off,
                                    sectorsPerTrack = 8,
                                    side1 = new Side
                                    {
                                        sideId    = 0,
                                        sectorIds = new int[8]
                                    },
                                    skew = 0,
                                    sofs = 0
                                };

                                for(int si = 0; si < 8; si++)
                                    _workingDefinition.side1.sectorIds[si] = si + 1;
                            }

                            break;
                        case FormatByte.k320:
                            if(imagePlugin.Info is { SectorSize: 512, Sectors: 640 })
                            {
                                _cpmFound              = true;
                                firstDirectorySector86 = 16;

                                _dpb = new DiscParameterBlock
                                {
                                    al0 = 0x80,
                                    al1 = 0,
                                    blm = 0x0F,
                                    bsh = 4,
                                    cks = 0x10,
                                    drm = 0x3F,
                                    dsm = 0x9D,
                                    exm = 1,
                                    off = 2,
                                    phm = 3,
                                    psh = 2,
                                    spt = 8 * 4
                                };

                                _workingDefinition = new CpmDefinition
                                {
                                    al0             = _dpb.al0,
                                    al1             = _dpb.al1,
                                    bitrate         = "LOW",
                                    blm             = _dpb.blm,
                                    bsh             = _dpb.bsh,
                                    bytesPerSector  = 512,
                                    cylinders       = 40,
                                    drm             = _dpb.drm,
                                    dsm             = _dpb.dsm,
                                    encoding        = "MFM",
                                    evenOdd         = false,
                                    exm             = _dpb.exm,
                                    label           = null,
                                    comment         = "CP/M-86 floppy identifier",
                                    ofs             = _dpb.off,
                                    sectorsPerTrack = 8,
                                    side1 = new Side
                                    {
                                        sideId    = 0,
                                        sectorIds = new int[8]
                                    },
                                    order = "SIDES",
                                    side2 = new Side
                                    {
                                        sideId    = 1,
                                        sectorIds = new int[8]
                                    },
                                    skew = 0,
                                    sofs = 0
                                };

                                for(int si = 0; si < 8; si++)
                                    _workingDefinition.side1.sectorIds[si] = si + 1;

                                for(int si = 0; si < 8; si++)
                                    _workingDefinition.side2.sectorIds[si] = si + 1;
                            }

                            break;
                        case FormatByte.k360:
                        case FormatByte.k360Alt:
                        case FormatByte.k360Alt2:
                            if(imagePlugin.Info is { SectorSize: 512, Sectors: 720 })
                            {
                                _cpmFound              = true;
                                firstDirectorySector86 = 36;

                                _dpb = new DiscParameterBlock
                                {
                                    al0 = 0x80,
                                    al1 = 0,
                                    blm = 0x0F,
                                    bsh = 4,
                                    cks = 0x10,
                                    drm = 0x3F,
                                    dsm = 0, // Unknown. Needed?
                                    exm = 1,
                                    off = 4,
                                    phm = 3,
                                    psh = 2,
                                    spt = 9 * 4
                                };

                                _workingDefinition = new CpmDefinition
                                {
                                    al0             = _dpb.al0,
                                    al1             = _dpb.al1,
                                    bitrate         = "LOW",
                                    blm             = _dpb.blm,
                                    bsh             = _dpb.bsh,
                                    bytesPerSector  = 512,
                                    cylinders       = 40,
                                    drm             = _dpb.drm,
                                    dsm             = _dpb.dsm,
                                    encoding        = "MFM",
                                    evenOdd         = false,
                                    exm             = _dpb.exm,
                                    label           = null,
                                    comment         = "CP/M-86 floppy identifier",
                                    ofs             = _dpb.off,
                                    sectorsPerTrack = 9,
                                    side1 = new Side
                                    {
                                        sideId    = 0,
                                        sectorIds = new int[9]
                                    },
                                    order = "SIDES",
                                    side2 = new Side
                                    {
                                        sideId    = 1,
                                        sectorIds = new int[9]
                                    },
                                    skew = 0,
                                    sofs = 0
                                };

                                for(int si = 0; si < 9; si++)
                                    _workingDefinition.side1.sectorIds[si] = si + 1;

                                for(int si = 0; si < 9; si++)
                                    _workingDefinition.side2.sectorIds[si] = si + 1;
                            }

                            break;
                        case FormatByte.k720:
                        case FormatByte.k720Alt:
                            if(imagePlugin.Info is { SectorSize: 512, Sectors: 1440 })
                            {
                                _cpmFound              = true;
                                firstDirectorySector86 = 36;

                                _dpb = new DiscParameterBlock
                                {
                                    al0 = 0xF0,
                                    al1 = 0,
                                    blm = 0x0F,
                                    bsh = 4,
                                    cks = 0x40,
                                    drm = 0xFF,
                                    dsm = 0x15E,
                                    exm = 0,
                                    off = 4,
                                    phm = 3,
                                    psh = 2,
                                    spt = 9 * 4
                                };

                                _workingDefinition = new CpmDefinition
                                {
                                    al0             = _dpb.al0,
                                    al1             = _dpb.al1,
                                    bitrate         = "LOW",
                                    blm             = _dpb.blm,
                                    bsh             = _dpb.bsh,
                                    bytesPerSector  = 512,
                                    cylinders       = 80,
                                    drm             = _dpb.drm,
                                    dsm             = _dpb.dsm,
                                    encoding        = "MFM",
                                    evenOdd         = false,
                                    exm             = _dpb.exm,
                                    label           = null,
                                    comment         = "CP/M-86 floppy identifier",
                                    ofs             = _dpb.off,
                                    sectorsPerTrack = 9,
                                    side1 = new Side
                                    {
                                        sideId    = 0,
                                        sectorIds = new int[9]
                                    },
                                    order = "SIDES",
                                    side2 = new Side
                                    {
                                        sideId    = 1,
                                        sectorIds = new int[9]
                                    },
                                    skew = 0,
                                    sofs = 0
                                };

                                for(int si = 0; si < 9; si++)
                                    _workingDefinition.side1.sectorIds[si] = si + 1;

                                for(int si = 0; si < 9; si++)
                                    _workingDefinition.side2.sectorIds[si] = si + 1;
                            }

                            break;
                        case FormatByte.f720:
                            if(imagePlugin.Info is { SectorSize: 512, Sectors: 1440 })
                            {
                                _cpmFound              = true;
                                firstDirectorySector86 = 18;

                                _dpb = new DiscParameterBlock
                                {
                                    al0 = 0xF0,
                                    al1 = 0,
                                    blm = 0x0F,
                                    bsh = 4,
                                    cks = 0x40,
                                    drm = 0xFF,
                                    dsm = 0x162,
                                    exm = 0,
                                    off = 2,
                                    phm = 3,
                                    psh = 2,
                                    spt = 9 * 4
                                };

                                _workingDefinition = new CpmDefinition
                                {
                                    al0             = _dpb.al0,
                                    al1             = _dpb.al1,
                                    bitrate         = "LOW",
                                    blm             = _dpb.blm,
                                    bsh             = _dpb.bsh,
                                    bytesPerSector  = 512,
                                    cylinders       = 80,
                                    drm             = _dpb.drm,
                                    dsm             = _dpb.dsm,
                                    encoding        = "MFM",
                                    evenOdd         = false,
                                    exm             = _dpb.exm,
                                    label           = null,
                                    comment         = "CP/M-86 floppy identifier",
                                    ofs             = _dpb.off,
                                    sectorsPerTrack = 9,
                                    side1 = new Side
                                    {
                                        sideId    = 0,
                                        sectorIds = new int[9]
                                    },
                                    order = "CYLINDERS",
                                    side2 = new Side
                                    {
                                        sideId    = 1,
                                        sectorIds = new int[9]
                                    },
                                    skew = 0,
                                    sofs = 0
                                };

                                for(int si = 0; si < 9; si++)
                                    _workingDefinition.side1.sectorIds[si] = si + 1;

                                for(int si = 0; si < 9; si++)
                                    _workingDefinition.side2.sectorIds[si] = si + 1;
                            }

                            break;
                        case FormatByte.f1200:
                            if(imagePlugin.Info is { SectorSize: 512, Sectors: 2400 })
                            {
                                _cpmFound              = true;
                                firstDirectorySector86 = 30;

                                _dpb = new DiscParameterBlock
                                {
                                    al0 = 0xC0,
                                    al1 = 0,
                                    blm = 0x1F,
                                    bsh = 5,
                                    cks = 0x40,
                                    drm = 0xFF,
                                    dsm = 0x127,
                                    exm = 1,
                                    off = 2,
                                    phm = 3,
                                    psh = 2,
                                    spt = 15 * 4
                                };

                                _workingDefinition = new CpmDefinition
                                {
                                    al0             = _dpb.al0,
                                    al1             = _dpb.al1,
                                    bitrate         = "HIGH",
                                    blm             = _dpb.blm,
                                    bsh             = _dpb.bsh,
                                    bytesPerSector  = 512,
                                    cylinders       = 80,
                                    drm             = _dpb.drm,
                                    dsm             = _dpb.dsm,
                                    encoding        = "MFM",
                                    evenOdd         = false,
                                    exm             = _dpb.exm,
                                    label           = null,
                                    comment         = "CP/M-86 floppy identifier",
                                    ofs             = _dpb.off,
                                    sectorsPerTrack = 15,
                                    side1 = new Side
                                    {
                                        sideId    = 0,
                                        sectorIds = new int[15]
                                    },
                                    order = "CYLINDERS",
                                    side2 = new Side
                                    {
                                        sideId    = 1,
                                        sectorIds = new int[15]
                                    },
                                    skew = 0,
                                    sofs = 0
                                };

                                for(int si = 0; si < 15; si++)
                                    _workingDefinition.side1.sectorIds[si] = si + 1;

                                for(int si = 0; si < 15; si++)
                                    _workingDefinition.side2.sectorIds[si] = si + 1;
                            }

                            break;
                        case FormatByte.f1440:
                            if(imagePlugin.Info is { SectorSize: 512, Sectors: 2880 })
                            {
                                _cpmFound              = true;
                                firstDirectorySector86 = 36;

                                _dpb = new DiscParameterBlock
                                {
                                    al0 = 0xC0,
                                    al1 = 0,
                                    blm = 0x1F,
                                    bsh = 5,
                                    cks = 0x40,
                                    drm = 0xFF,
                                    dsm = 0x162,
                                    exm = 1,
                                    off = 2,
                                    phm = 3,
                                    psh = 2,
                                    spt = 18 * 4
                                };

                                _workingDefinition = new CpmDefinition
                                {
                                    al0             = _dpb.al0,
                                    al1             = _dpb.al1,
                                    bitrate         = "LOW",
                                    blm             = _dpb.blm,
                                    bsh             = _dpb.bsh,
                                    bytesPerSector  = 512,
                                    cylinders       = 80,
                                    drm             = _dpb.drm,
                                    dsm             = _dpb.dsm,
                                    encoding        = "MFM",
                                    evenOdd         = false,
                                    exm             = _dpb.exm,
                                    label           = null,
                                    comment         = "CP/M-86 floppy identifier",
                                    ofs             = _dpb.off,
                                    sectorsPerTrack = 18,
                                    side1 = new Side
                                    {
                                        sideId    = 0,
                                        sectorIds = new int[18]
                                    },
                                    order = "CYLINDERS",
                                    side2 = new Side
                                    {
                                        sideId    = 1,
                                        sectorIds = new int[18]
                                    },
                                    skew = 0,
                                    sofs = 0
                                };

                                for(int si = 0; si < 18; si++)
                                    _workingDefinition.side1.sectorIds[si] = si + 1;

                                for(int si = 0; si < 18; si++)
                                    _workingDefinition.side2.sectorIds[si] = si + 1;
                            }

                            break;
                    }

                    if(_cpmFound)
                    {
                        uint directoryLength = (uint)(((ulong)_dpb.drm + 1) * 32 / imagePlugin.Info.SectorSize);

                        imagePlugin.ReadSectors(firstDirectorySector86 + partition.Start, directoryLength,
                                                out directory);

                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_CPM_86_floppy_identifier);
                    }
                }
            }

            // One of the few CP/M filesystem marks has been found, try for correcteness checking the whole directory
            if(_cpmFound)
            {
                if(CheckDir(directory))
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.First_directory_block_seems_correct);

                    return true;
                }

                _cpmFound = false;
            }

            // Try all definitions
            if(!_cpmFound)
            {
                // Load all definitions
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Trying_to_load_definitions);

                if(LoadDefinitions()                      &&
                   _definitions?.definitions      != null &&
                   _definitions.definitions.Count > 0)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Trying_all_known_definitions);

                    foreach(CpmDefinition def in from def in _definitions.definitions let sectors =
                                                     (ulong)(def.cylinders * def.sides * def.sectorsPerTrack)
                                                 where sectors            == imagePlugin.Info.Sectors &&
                                                       def.bytesPerSector == imagePlugin.Info.SectorSize select def)
                    {
                        // Definition seems to describe current disk, at least, same number of volume sectors and bytes per sector
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Trying_definition_0, def.comment);
                        ulong offset;

                        if(def.sofs != 0)
                            offset = (ulong)def.sofs;
                        else
                            offset = (ulong)(def.ofs * def.sectorsPerTrack);

                        int dirLen = (def.drm + 1) * 32 / def.bytesPerSector;

                        if(def.sides == 1)
                        {
                            _sectorMask = new int[def.side1.sectorIds.Length];

                            for(int m = 0; m < _sectorMask.Length; m++)
                                _sectorMask[m] = def.side1.sectorIds[m] - def.side1.sectorIds[0];
                        }
                        else
                        {
                            // Head changes after every track
                            if(string.Compare(def.order, "SIDES", StringComparison.InvariantCultureIgnoreCase) == 0)
                            {
                                _sectorMask = new int[def.side1.sectorIds.Length + def.side2.sectorIds.Length];

                                for(int m = 0; m < def.side1.sectorIds.Length; m++)
                                    _sectorMask[m] = def.side1.sectorIds[m] - def.side1.sectorIds[0];

                                // Skip first track (first side)
                                for(int m = 0; m < def.side2.sectorIds.Length; m++)
                                    _sectorMask[m + def.side1.sectorIds.Length] =
                                        def.side2.sectorIds[m] - def.side2.sectorIds[0] + def.side1.sectorIds.Length;
                            }

                            // Head changes after whole side
                            else if(string.Compare(def.order, "CYLINDERS",
                                                   StringComparison.InvariantCultureIgnoreCase) == 0)
                            {
                                for(int m = 0; m < def.side1.sectorIds.Length; m++)
                                    _sectorMask[m] = def.side1.sectorIds[m] - def.side1.sectorIds[0];

                                // Skip first track (first side) and first track (second side)
                                for(int m = 0; m < def.side1.sectorIds.Length; m++)
                                    _sectorMask[m + def.side1.sectorIds.Length] =
                                        def.side1.sectorIds[m] - def.side1.sectorIds[0] + def.side1.sectorIds.Length +
                                        def.side2.sectorIds.Length;
                            }

                            // TODO: Implement COLUMBIA ordering
                            else if(string.Compare(def.order, "COLUMBIA",
                                                   StringComparison.InvariantCultureIgnoreCase) == 0)
                            {
                                AaruConsole.DebugWriteLine(MODULE_NAME,
                                                           Localization.
                                                               Dont_know_how_to_handle_COLUMBIA_ordering_not_proceeding_with_this_definition);

                                continue;
                            }

                            // TODO: Implement EAGLE ordering
                            else if(string.Compare(def.order, "EAGLE", StringComparison.InvariantCultureIgnoreCase) ==
                                    0)
                            {
                                AaruConsole.DebugWriteLine(MODULE_NAME,
                                                           Localization.
                                                               Don_know_how_to_handle_EAGLE_ordering_not_proceeding_with_this_definition);

                                continue;
                            }
                            else
                            {
                                AaruConsole.DebugWriteLine(MODULE_NAME,
                                                           Localization.
                                                               Unknown_order_type_0_not_proceeding_with_this_definition,
                                                           def.order);

                                continue;
                            }
                        }

                        // Read the directory marked by this definition
                        var ms = new MemoryStream();

                        for(int p = 0; p < dirLen; p++)
                        {
                            errno =
                                imagePlugin.
                                    ReadSector((ulong)((int)offset + (int)partition.Start + (p / _sectorMask.Length * _sectorMask.Length) + _sectorMask[p % _sectorMask.Length]),
                                               out byte[] dirSector);

                            if(errno != ErrorNumber.NoError)
                                break;

                            ms.Write(dirSector, 0, dirSector.Length);
                        }

                        directory = ms.ToArray();

                        if(def.evenOdd)
                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                       Localization.
                                                           Definition_contains_EVEN_ODD_field_with_unknown_meaning_detection_may_be_wrong);

                        // Complement of the directory bytes if needed
                        if(def.complement)
                            for(int b = 0; b < directory.Length; b++)
                                directory[b] = (byte)(~directory[b] & 0xFF);

                        // Check the directory
                        if(CheckDir(directory))
                        {
                            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Definition_0_has_a_correct_directory,
                                                       def.comment);

                            // Build a Disc Parameter Block
                            _workingDefinition = def;

                            _dpb = new DiscParameterBlock
                            {
                                al0 = (byte)def.al0,
                                al1 = (byte)def.al1,
                                blm = (byte)def.blm,
                                bsh = (byte)def.bsh,

                                // Needed?
                                cks = 0,
                                drm = (ushort)def.drm,
                                dsm = (ushort)def.dsm,
                                exm = (byte)def.exm,
                                off = (ushort)def.ofs,
                                spt = (ushort)(def.sectorsPerTrack * def.bytesPerSector / 128)
                            };

                            switch(def.bytesPerSector)
                            {
                                case 128:
                                    _dpb.psh = 0;
                                    _dpb.phm = 0;

                                    break;
                                case 256:
                                    _dpb.psh = 1;
                                    _dpb.phm = 1;

                                    break;
                                case 512:
                                    _dpb.psh = 2;
                                    _dpb.phm = 3;

                                    break;
                                case 1024:
                                    _dpb.psh = 3;
                                    _dpb.phm = 7;

                                    break;
                                case 2048:
                                    _dpb.psh = 4;
                                    _dpb.phm = 15;

                                    break;
                                case 4096:
                                    _dpb.psh = 5;
                                    _dpb.phm = 31;

                                    break;
                                case 8192:
                                    _dpb.psh = 6;
                                    _dpb.phm = 63;

                                    break;
                                case 16384:
                                    _dpb.psh = 7;
                                    _dpb.phm = 127;

                                    break;
                                case 32768:
                                    _dpb.psh = 8;
                                    _dpb.phm = 255;

                                    break;
                            }

                            _cpmFound          = true;
                            _workingDefinition = def;

                            return true;
                        }

                        _label             = null;
                        _labelCreationDate = null;
                        _labelUpdateDate   = null;
                    }
                }
            }

            // Clear class variables
            _cpmFound             = false;
            _workingDefinition    = null;
            _dpb                  = null;
            _label                = null;
            _standardTimestamps   = false;
            _thirdPartyTimestamps = false;

            return false;
        }
        catch
        {
            //throw ex;
            return false;
        }
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        metadata    = new FileSystem();

        // As the identification is so complex, just call Identify() and relay on its findings
        if(!Identify(imagePlugin, partition) ||
           !_cpmFound                        ||
           _workingDefinition == null        ||
           _dpb               == null)
            return;

        var sb = new StringBuilder();
        sb.AppendLine(Localization.CPM_filesystem);

        if(!string.IsNullOrEmpty(_workingDefinition.comment))
            sb.AppendFormat(Localization.Identified_as_0, _workingDefinition.comment).AppendLine();

        sb.AppendFormat(Localization.Volume_block_is_0_bytes, 128 << _dpb.bsh).AppendLine();

        if(_dpb.dsm > 0)
            sb.AppendFormat(Localization.Volume_contains_0_blocks_1_bytes, _dpb.dsm, _dpb.dsm * (128 << _dpb.bsh)).
               AppendLine();

        sb.AppendFormat(Localization.Volume_contains_0_directory_entries, _dpb.drm + 1).AppendLine();

        if(_workingDefinition.sofs > 0)
            sb.AppendFormat(Localization.Volume_reserves_0_sectors_for_system, _workingDefinition.sofs).AppendLine();
        else
            sb.AppendFormat(Localization.Volume_reserves_1_tracks_0_sectors_for_system,
                            _workingDefinition.ofs * _workingDefinition.sectorsPerTrack, _workingDefinition.ofs).
               AppendLine();

        if(_workingDefinition.side1.sectorIds.Length >= 2)
        {
            int interleaveSide1 = _workingDefinition.side1.sectorIds[1] - _workingDefinition.side1.sectorIds[0];

            if(interleaveSide1 > 1)
                sb.AppendFormat(Localization.Side_zero_uses_0_one_software_interleaving, interleaveSide1).AppendLine();
        }

        if(_workingDefinition.sides == 2)
        {
            if(_workingDefinition.side2.sectorIds.Length >= 2)
            {
                int interleaveSide2 = _workingDefinition.side2.sectorIds[1] - _workingDefinition.side2.sectorIds[0];

                if(interleaveSide2 > 1)
                    sb.AppendFormat(Localization.Side_one_uses_0_one_software_interleaving, interleaveSide2).
                       AppendLine();
            }

            switch(_workingDefinition.order)
            {
                case "SIDES":
                    sb.AppendLine(Localization.Head_changes_after_each_whole_track);

                    break;
                case "CYLINDERS":
                    sb.AppendLine(Localization.Head_changes_after_whole_side);

                    break;
                default:
                    sb.AppendFormat(Localization.Unknown_how_0_side_ordering_works, _workingDefinition.order).
                       AppendLine();

                    break;
            }
        }

        if(_workingDefinition.skew > 0)
            sb.AppendFormat(Localization.Device_uses_0_one_hardware_interleaving, _workingDefinition.skew).AppendLine();

        if(_workingDefinition.sofs > 0)
            sb.AppendLine($"BSH {_dpb.bsh} BLM {_dpb.blm} EXM {_dpb.exm} DSM {_dpb.dsm} DRM {_dpb.drm} AL0 {_dpb.al0
                :X2}H AL1 {_dpb.al1:X2}H SOFS {_workingDefinition.sofs}");
        else
            sb.AppendLine($"BSH {_dpb.bsh} BLM {_dpb.blm} EXM {_dpb.exm} DSM {_dpb.dsm} DRM {_dpb.drm} AL0 {_dpb.al0
                :X2}H AL1 {_dpb.al1:X2}H OFS {_workingDefinition.ofs}");

        if(_label != null)
            sb.AppendFormat(Localization.Volume_label_0, _label).AppendLine();

        if(_standardTimestamps)
            sb.AppendLine(Localization.Volume_uses_standard_CPM_timestamps);

        if(_thirdPartyTimestamps)
            sb.AppendLine(Localization.Volume_uses_third_party_timestamps);

        if(_labelCreationDate != null)
            sb.AppendFormat(Localization.Volume_created_on_0, DateHandlers.CpmToDateTime(_labelCreationDate)).
               AppendLine();

        if(_labelUpdateDate != null)
            sb.AppendFormat(Localization.Volume_updated_on_0, DateHandlers.CpmToDateTime(_labelUpdateDate)).
               AppendLine();

        metadata             =  new FileSystem();
        metadata.Bootable    |= _workingDefinition.sofs > 0 || _workingDefinition.ofs > 0;
        metadata.ClusterSize =  (uint)(128 << _dpb.bsh);

        if(_dpb.dsm > 0)
            metadata.Clusters = _dpb.dsm;
        else
            metadata.Clusters = partition.End - partition.Start;

        if(_labelCreationDate != null)
        {
            metadata.CreationDate = DateHandlers.CpmToDateTime(_labelCreationDate);
        }

        if(_labelUpdateDate != null)
        {
            metadata.ModificationDate = DateHandlers.CpmToDateTime(_labelUpdateDate);
        }

        metadata.Type       = FS_TYPE;
        metadata.VolumeName = _label;

        information = sb.ToString();
    }
}