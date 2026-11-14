// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Sectors.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CP/M filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Maps CP/M logical sectors to image sectors and checks definitions
//     against the disk.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Decoders.Floppy;
using Aaru.Logging;

namespace Aaru.Filesystems;

public sealed partial class CPM
{
    /// <summary>Gets how many allocation blocks fit after the reserved area of a volume</summary>
    /// <param name="sectors">Sectors in the volume</param>
    /// <param name="reservedSectors">Sectors reserved for the system at the start of the volume</param>
    /// <param name="bytesPerSector">Bytes per sector</param>
    /// <param name="bsh">Block shift, the block is 128 bytes shifted left by this</param>
    /// <returns>Number of allocation blocks, 0 if none fit</returns>
    static int GetCapacityInBlocks(ulong sectors, ulong reservedSectors, int bytesPerSector, int bsh)
    {
        if(sectors <= reservedSectors || bytesPerSector <= 0 || bsh is < 0 or > 16) return 0;

        return (int)((sectors - reservedSectors) * (ulong)bytesPerSector / (ulong)(128 << bsh));
    }

    /// <summary>Gets how many allocation blocks fit in the volume a definition describes</summary>
    /// <param name="def">Disk definition</param>
    /// <returns>Number of allocation blocks, 0 if none fit</returns>
    static int GetCapacityInBlocks(CpmDefinition def)
    {
        var sectors = (ulong)(def.cylinders * Math.Max(def.sides, 1) * def.sectorsPerTrack);

        return GetCapacityInBlocks(sectors, GetReservedSectors(def), def.bytesPerSector, def.bsh);
    }

    /// <summary>Gets the sectors a definition reserves for the system at the start of the volume</summary>
    /// <param name="def">Disk definition</param>
    /// <returns>Reserved sectors</returns>
    static ulong GetReservedSectors(CpmDefinition def)
    {
        if(def.sofs > 0) return (ulong)def.sofs;

        return (ulong)(def.ofs * def.sectorsPerTrack);
    }

    /// <summary>Converts a CP/M logical sector into the sector of the volume that holds it</summary>
    /// <remarks>
    ///     Images store sectors by cylinder, then head, then sector ID. SIDES uses both sides of a cylinder before
    ///     moving to the next one. CYLINDERS and EAGLE use all the cylinders of the first side and then all the
    ///     cylinders of the second side, in the same direction, as the Otrona Attaché EAGLE disks show. Inside a
    ///     track the definition's sector IDs give the software interleaving.
    /// </remarks>
    /// <param name="def">Disk definition</param>
    /// <param name="logicalSector">CP/M logical sector</param>
    /// <param name="physicalSector">Sector of the volume, relative to its start</param>
    /// <returns><c>false</c> if the definition uses a side ordering that is not supported</returns>
    static bool TryGetPhysicalSector(CpmDefinition def, ulong logicalSector, out ulong physicalSector)
    {
        physicalSector = 0;

        if(def.sectorsPerTrack <= 0 || def.cylinders <= 0) return false;

        var   sectorsPerTrack = (ulong)def.sectorsPerTrack;
        ulong track           = logicalSector / sectorsPerTrack;
        var   sector          = (int)(logicalSector % sectorsPerTrack);
        ulong cylinder;
        ulong head;

        if(def.sides <= 1)
        {
            cylinder = track;
            head     = 0;
        }
        else if(string.Equals(def.order, "SIDES", StringComparison.InvariantCultureIgnoreCase))
        {
            cylinder = track / 2;
            head     = track % 2;
        }
        else if(string.Equals(def.order, "CYLINDERS", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(def.order, "EAGLE",     StringComparison.InvariantCultureIgnoreCase))
        {
            head     = track / (ulong)def.cylinders;
            cylinder = track % (ulong)def.cylinders;
        }
        else
            return false;

        Side side = head == 0 ? def.side1 : def.side2;

        if(side?.sectorIds is null || sector >= side.sectorIds.Length) return false;

        int index = side.sectorIds[sector] - side.sectorIds.Min();

        if(index < 0 || index >= def.sectorsPerTrack) return false;

        physicalSector = (cylinder * (ulong)Math.Max(def.sides, 1) + head) * sectorsPerTrack + (ulong)index;

        return true;
    }

    /// <summary>Logs why a definition with an unsupported side ordering is skipped</summary>
    /// <param name="def">Disk definition</param>
    static void LogUnsupportedOrder(CpmDefinition def)
    {
        if(string.Equals(def.order, "COLUMBIA", StringComparison.InvariantCultureIgnoreCase))
        {
            AaruLogging.Debug(MODULE_NAME,
                              Localization.Dont_know_how_to_handle_COLUMBIA_ordering_not_proceeding_with_this_definition);
        }
        else
        {
            AaruLogging.Debug(MODULE_NAME,
                              Localization.Unknown_order_type_0_not_proceeding_with_this_definition,
                              def.order);
        }
    }

    /// <summary>
    ///     Checks the sector IDs a definition expects against the address marks stored in the image. Disks with the
    ///     same geometry are often told apart only by their sector IDs, like the Amstrad CPC formats.
    /// </summary>
    /// <param name="imagePlugin">Image</param>
    /// <param name="partition">Partition</param>
    /// <param name="def">Disk definition</param>
    /// <returns><c>false</c> if the image has address marks and their sector IDs are not the definition's ones</returns>
    static bool SectorIdsMatch(IMediaImage imagePlugin, Partition partition, CpmDefinition def)
    {
        if(imagePlugin.Info.ReadableSectorTags?.Contains(SectorTagType.FloppyAddressMark) != true) return true;

        for(var head = 0; head < Math.Max(def.sides, 1); head++)
        {
            Side side = head == 0 ? def.side1 : def.side2;

            if(side?.sectorIds is not { Length: > 0 }) continue;

            // Sectors are stored by sector ID, so the first one of the track has the lowest ID
            ulong sector = partition.Start + (ulong)(head * def.sectorsPerTrack);

            ErrorNumber errno = imagePlugin.ReadSectorTag(sector,
                                                          false,
                                                          SectorTagType.FloppyAddressMark,
                                                          out byte[] addressMark);

            // Only IBM style address marks are understood: A1 A1 A1 FE C H R N
            if(errno != ErrorNumber.NoError         ||
               addressMark is not { Length: >= 20 } ||
               addressMark[12] != 0xA1              ||
               addressMark[13] != 0xA1              ||
               addressMark[14] != 0xA1              ||
               addressMark[15] != (byte)IBMIdType.AddressMark)
                return true;

            if(addressMark[18] != side.sectorIds.Min()) return false;
        }

        return true;
    }

    /// <summary>
    ///     Checks that every block allocated by the files in a directory exists in the volume and is not one of the
    ///     blocks of the directory
    /// </summary>
    /// <param name="directory">Directory</param>
    /// <param name="dsm">Highest block number of the volume</param>
    /// <param name="al0">Directory blocks bitmap, first byte, bit 7 is block 0</param>
    /// <param name="al1">Directory blocks bitmap, second byte, bit 0 is block 15</param>
    /// <returns><c>false</c> if a file allocates a block after the end of the volume or inside the directory</returns>
    static bool CheckAllocations(byte[] directory, int dsm, int al0, int al1)
    {
        if(directory is null || dsm <= 0) return true;

        int directoryBlocks = (al0 & 0xFF) << 8 | al1 & 0xFF;

        for(var off = 0; off + 32 <= directory.Length; off += 32)
        {
            // Only file entries allocate blocks
            if((directory[off] & 0x7F) >= 0x10) continue;

            for(var a = 16; a < 32; a += dsm > 255 ? 2 : 1)
            {
                int block = dsm > 255 ? BitConverter.ToUInt16(directory, off + a) : directory[off + a];

                if(block > dsm) return false;

                // Block 0 marks an unused pointer
                if(block is > 0 and < 16 && (directoryBlocks & 0x8000 >> block) != 0) return false;
            }
        }

        return true;
    }

    /// <summary>Counts the directory entries that cannot exist in a CP/M directory</summary>
    /// <remarks>
    ///     A directory only contains files and passwords (status 0 to 31), the volume label (32), timestamps (33)
    ///     and free entries (E5h). Anything else is data that was read as if it were the directory.
    /// </remarks>
    /// <param name="directory">Directory</param>
    /// <returns>Number of impossible entries</returns>
    static int CountInvalidEntries(byte[] directory)
    {
        var invalid = 0;

        for(var off = 0; off + 32 <= directory.Length; off += 32)
        {
            int status = directory[off];

            if(status != 0xE5 && (status & 0x7F) > 0x21) invalid++;
        }

        return invalid;
    }

    /// <summary>Counts the distinct file entries in a directory</summary>
    /// <param name="directory">Directory</param>
    /// <returns>Number of distinct file entries</returns>
    static int CountFileEntries(byte[] directory)
    {
        HashSet<string> entries = [];

        for(var off = 0; off + 32 <= directory.Length; off += 32)
        {
            if((directory[off] & 0x7F) >= 0x10) continue;

            // User, filename, extension and extent numbers identify an entry
            string key = Convert.ToHexString(directory, off, 16);

            if(directory.Skip(off + 1).Take(11).All(static c => (c & 0x7F) is 0x20 or 0x00)) continue;

            entries.Add(key);
        }

        return entries.Count;
    }

    /// <summary>Uses a disk definition for the volume, building its disc parameter block</summary>
    /// <param name="def">Disk definition</param>
    void UseDefinition(CpmDefinition def)
    {
        int recordsPerSector = def.bytesPerSector / 128;

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
            phm = (byte)(recordsPerSector - 1),
            psh = (byte)Math.Log2(recordsPerSector),
            spt = (ushort)(def.sectorsPerTrack * recordsPerSector)
        };

        _cpmFound          = true;
        _workingDefinition = def;
    }
}