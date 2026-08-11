// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : XENIX.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages XENIX partitions.
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
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of XENIX partitions</summary>
public sealed class XENIX : IPartition
{
    const ushort PAMAGIC     = 0x1234;
    const ushort BAMAGIC     = 0x4321;
    const int    MAXPARTS    = 16;
    const uint   XENIX_BSIZE = 1024;

    // Sectors reserved at the start of the fdisk partition (boot0 + boot1 + division table + bad track table),
    // GDSECS in SCO's <sys/dio.h>
    const uint GDSECS = 52;

    // Fallback base offset (in 1 KiB blocks) matching the 20 MiB XENIX 2 disks this plugin was written against,
    // used when the image carries no geometry to compute the real cylinder-aligned base from
    const uint   XENIX_OFFSET = 977;
    const string MODULE_NAME  = "XENIX partitions plugin";

#region Nested type: Partable

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Partable
    {
        public readonly ushort p_magic; /* magic number validity indicator */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXPARTS)]
        public readonly Partition[] p; /*partition headers*/
    }

#endregion

#region Nested type: Partition

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Partition
    {
        public readonly int p_off;  /*start 1K block no of partition*/
        public readonly int p_size; /*# of 1K blocks in partition*/
    }

#endregion

#region IPartition Members

    /// <inheritdoc />
    public string Name => Localization.XENIX_Name;

    /// <inheritdoc />
    public Guid Id => new("53BE01DE-E68B-469F-A17F-EC2E4BD61CD9");

    /// <inheritdoc />
    public string Author => Authors.NATALIA_PORTILLO;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<CommonTypes.Partition> partitions, ulong sectorOffset)
    {
        partitions = [];

        if(44 + sectorOffset >= imagePlugin.Info.Sectors) return false;

        ErrorNumber errno = imagePlugin.ReadSector(42 + sectorOffset, false, out byte[] tblsector, out _);

        if(errno != ErrorNumber.NoError) return false;

        Partable xnxtbl = Marshal.ByteArrayToStructureLittleEndian<Partable>(tblsector);

        AaruLogging.Debug(MODULE_NAME, "xnxtbl.p_magic = 0x{0:X4} (should be 0x{1:X4})", xnxtbl.p_magic, PAMAGIC);

        if(xnxtbl.p_magic != PAMAGIC) return false;

        // Division offsets are relative to a cylinder-aligned filesystem base: the reserved area (GDSECS plus the
        // bad track alternates) rounded up to the next cylinder boundary
        ushort maxBad = 0;
        errno = imagePlugin.ReadSector(44 + sectorOffset, false, out byte[] badSector, out _);

        if(errno == ErrorNumber.NoError && badSector.Length >= 4 && BitConverter.ToUInt16(badSector, 0) == BAMAGIC)
            maxBad = BitConverter.ToUInt16(badSector,                                                2);

        AaruLogging.Debug(MODULE_NAME, "b_maxbad = {0}", maxBad);

        uint heads = imagePlugin.Info.Heads;
        uint spt   = imagePlugin.Info.SectorsPerTrack;

        if(heads == 0 || spt == 0) GetMbrGeometry(imagePlugin, sectorOffset, out heads, out spt);

        ulong legacyBase = XENIX_OFFSET * XENIX_BSIZE / imagePlugin.Info.SectorSize + sectorOffset;
        ulong fsBase     = legacyBase;

        if(heads > 0 && spt > 0)
        {
            ulong spc      = (ulong)heads * spt;
            ulong reserved = sectorOffset + GDSECS + (ulong)maxBad * spt;
            ulong cylBase  = (reserved + spc - 1)                  / spc * spc;

            AaruLogging.Debug(MODULE_NAME, "heads = {0}, spt = {1}, computed base = {2}", heads, spt, cylBase);

            // Keep the legacy base only if the computed one shows no superblock where division 0 should have it
            // but the legacy one does
            if(HasSuperblock(imagePlugin, cylBase) || !HasSuperblock(imagePlugin, legacyBase)) fsBase = cylBase;
        }

        AaruLogging.Debug(MODULE_NAME, "fsBase = {0}", fsBase);

        for(var i = 0; i < MAXPARTS; i++)
        {
            AaruLogging.Debug(MODULE_NAME, "xnxtbl.p[{0}].p_off = {1}",  i, xnxtbl.p[i].p_off);
            AaruLogging.Debug(MODULE_NAME, "xnxtbl.p[{0}].p_size = {1}", i, xnxtbl.p[i].p_size);

            if(xnxtbl.p[i].p_size <= 0) continue;

            var part = new CommonTypes.Partition
            {
                Start    = fsBase + (ulong)xnxtbl.p[i].p_off * XENIX_BSIZE / imagePlugin.Info.SectorSize,
                Length   = (ulong)xnxtbl.p[i].p_size * XENIX_BSIZE / imagePlugin.Info.SectorSize,
                Offset   = fsBase * imagePlugin.Info.SectorSize + (ulong)xnxtbl.p[i].p_off * XENIX_BSIZE,
                Size     = (ulong)xnxtbl.p[i].p_size * XENIX_BSIZE,
                Sequence = (ulong)i,
                Type     = "XENIX",
                Scheme   = Name
            };

            if(part.End < imagePlugin.Info.Sectors) partitions.Add(part);
        }

        return partitions.Count > 0;
    }

#endregion

    /// <summary>Derives heads and sectors per track from the MBR entry that starts at <paramref name="sectorOffset" /></summary>
    static void GetMbrGeometry(IMediaImage imagePlugin, ulong sectorOffset, out uint heads, out uint spt)
    {
        heads = 0;
        spt   = 0;

        if(imagePlugin.ReadSector(0, false, out byte[] mbr, out _) != ErrorNumber.NoError) return;

        if(mbr.Length < 512 || mbr[510] != 0x55 || mbr[511] != 0xAA) return;

        for(var i = 0; i < 4; i++)
        {
            int entry = 446 + 16 * i;

            if(BitConverter.ToUInt32(mbr, entry + 8) != sectorOffset) continue;

            heads = (uint)mbr[entry + 5] + 1;
            spt   = (uint)mbr[entry + 6] & 0x3F;

            return;
        }
    }

    /// <summary>Checks whether a XENIX or SysV superblock is found where a division starting at
    /// <paramref name="startSector" /> should have one</summary>
    static bool HasSuperblock(IMediaImage imagePlugin, ulong startSector)
    {
        uint sectorsPerBlock = XENIX_BSIZE / imagePlugin.Info.SectorSize;

        if(sectorsPerBlock == 0) sectorsPerBlock = 1;

        // The superblock lives at 1 KiB block 1 of the division, but tolerate the one-sector misalignment the
        // legacy base produces
        for(ulong i = 1; i <= sectorsPerBlock + 1; i++)
        {
            if(imagePlugin.ReadSectors(startSector + i, false, sectorsPerBlock, out byte[] sb, out _) !=
               ErrorNumber.NoError)
                continue;

            if(sb.Length < 0x400) continue;

            uint magic = BitConverter.ToUInt32(sb, 0x3F8);

            switch(magic)
            {
                case 0x002B5544 or 0x44552B00: // XENIX
                case 0xFD187E20 or 0x207E18FD: // SysV
                case 0xFD187E21 or 0x217E18FD: // EAFS
                    return true;
            }
        }

        return false;
    }
}