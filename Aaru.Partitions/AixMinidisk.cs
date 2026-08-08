// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AixMinidisk.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages AIX minidisk partitions (VTOC).
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Attributes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of the AIX minidisk (VTOC) partitioning scheme</summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names match the original AIX headers")]
public sealed partial class AixMinidisk : IPartition
{
    const string MODULE_NAME = "AIX minidisk plugin";

    /// <summary>AIX VTOC magic string "VTOC"</summary>
    const uint VTOC_MAGIC = 0x56544F43; // "VTOC" in ASCII

    /// <summary>Block size for AIX minidisks</summary>
    const int BLOCKSIZE = 512;

    /// <summary>Maximum number of minidisks in VTOC</summary>
    const int MAX_MINIDISKS = 32;

    /// <inheritdoc />
    public string Name => "AIX minidisk";

    /// <inheritdoc />
    public Guid Id => new("25ED1C9A-0FF0-49FC-9F43-34844B8309B8");

    /// <inheritdoc />
    public string Author => Authors.NATALIA_PORTILLO;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        partitions = [];

        // VTOC is typically at sector 1 (after boot0)
        // But we need to find the AIX partition first if coming from MBR
        ulong vtocSector = sectorOffset + 1;

        ErrorNumber errno = imagePlugin.ReadSector(vtocSector, false, out byte[] sector, out _);

        if(errno != ErrorNumber.NoError || sector.Length < BLOCKSIZE)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading VTOC sector: {0}", errno);

            return false;
        }

        // Check for VTOC magic
        var magic = (uint)(sector[0] << 24 | sector[1] << 16 | sector[2] << 8 | sector[3]);

        AaruLogging.Debug(MODULE_NAME, "VTOC magic = 0x{0:X8}, expected 0x{1:X8}", magic, VTOC_MAGIC);

        if(magic != VTOC_MAGIC)
        {
            AaruLogging.Debug(MODULE_NAME, "VTOC magic not found at sector {0}", vtocSector);

            return false;
        }

        Vtoc1 vtoc = Marshal.ByteArrayToStructureBigEndian<Vtoc1>(sector);

        AaruLogging.Debug(MODULE_NAME,
                          "vtoc.magic_string = \"{0}\"",
                          Encoding.ASCII.GetString(vtoc.magic_string).TrimEnd('\0'));

        AaruLogging.Debug(MODULE_NAME, "vtoc.version = {0}",    vtoc.version);
        AaruLogging.Debug(MODULE_NAME, "vtoc.seq_num = {0}",    vtoc.seq_num);
        AaruLogging.Debug(MODULE_NAME, "vtoc.bootsize = {0}",   vtoc.bootsize);
        AaruLogging.Debug(MODULE_NAME, "vtoc.numbadblks = {0}", vtoc.numbadblks);
        AaruLogging.Debug(MODULE_NAME, "vtoc.badblkoff = {0}",  vtoc.badblkoff);

        // Read VTOC2 and VTOC3 for mount string table
        errno = imagePlugin.ReadSector(vtocSector + 1, false, out byte[] vtoc2Sector, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading VTOC2 sector: {0}", errno);

            return false;
        }

        errno = imagePlugin.ReadSector(vtocSector + 2, false, out byte[] vtoc3Sector, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading VTOC3 sector: {0}", errno);

            return false;
        }

        // Build the string table from VTOC2 and VTOC3
        // VTOC2: byte 0 is seq_num, bytes 1-511 are first part of string table
        // VTOC3: bytes 0-506 are second part, byte 507 is seq_num, bytes 508-511 are checksum
        var stringTable = new byte[511 + 507]; // V2_STRTABLEN + V3_STRTABLEN
        Array.Copy(vtoc2Sector, 1, stringTable, 0,   511);
        Array.Copy(vtoc3Sector, 0, stringTable, 511, 507);

        ulong counter = 0;

        for(var i = 0; i < MAX_MINIDISKS; i++)
        {
            Minidisk md = vtoc.mini[i];

            // Check for end marker
            if(md.type == MD_END)
            {
                AaruLogging.Debug(MODULE_NAME, "End of minidisk table at entry {0}", i);

                break;
            }

            AaruLogging.Debug(MODULE_NAME, "vtoc.mini[{0}].s_block = {1}",    i, md.s_block);
            AaruLogging.Debug(MODULE_NAME, "vtoc.mini[{0}].num_blks = {1}",   i, md.num_blks);
            AaruLogging.Debug(MODULE_NAME, "vtoc.mini[{0}].type = {1} ({2})", i, md.type, TypeToString(md.type));
            AaruLogging.Debug(MODULE_NAME, "vtoc.mini[{0}].mdisk_id = {1}",   i, md.mdisk_id);
            AaruLogging.Debug(MODULE_NAME, "vtoc.mini[{0}].str_index = {1}",  i, md.str_index);

            // Skip entries with no blocks
            if(md.num_blks == 0) continue;

            // Skip reserved and bad block entries
            if(md.type is MD_BAD_BLOCK or MD_UNALLOC) continue;

            // Get mount point string from string table
            var mountPoint = "";

            if(md.str_index > 0 && md.str_index < stringTable.Length)
            {
                int strEnd = Array.IndexOf<byte>(stringTable, 0, md.str_index);
                int strLen = strEnd >= 0 ? strEnd - md.str_index : stringTable.Length - md.str_index;
                mountPoint = Encoding.ASCII.GetString(stringTable, md.str_index, strLen);
            }

            var part = new Partition
            {
                Start    = (ulong)md.s_block * BLOCKSIZE / imagePlugin.Info.SectorSize + sectorOffset,
                Offset   = (ulong)md.s_block             * BLOCKSIZE + sectorOffset * imagePlugin.Info.SectorSize,
                Length   = (ulong)md.num_blks            * BLOCKSIZE / imagePlugin.Info.SectorSize,
                Size     = (ulong)md.num_blks                        * BLOCKSIZE,
                Type     = TypeToString(md.type),
                Sequence = counter,
                Scheme   = Name,
                Name     = string.IsNullOrEmpty(mountPoint) ? $"hd{md.mdisk_id & 0x1F}" : mountPoint
            };

            AaruLogging.Debug(MODULE_NAME,
                              "Partition {0}: start={1}, length={2}, type={3}, name={4}",
                              counter,
                              part.Start,
                              part.Length,
                              part.Type,
                              part.Name);

            partitions.Add(part);
            counter++;
        }

        return partitions.Count > 0;
    }

    static string TypeToString(byte type) => type switch
                                             {
                                                 MD_END        => "End marker",
                                                 MD_PAGE       => "AIX paging space",
                                                 MD_DUMP       => "AIX dump space",
                                                 MD_UNALLOC    => "Unallocated",
                                                 MD_AIX_BOOT   => "AIX boot",
                                                 MD_AIX_NOBOOT => "AIX filesystem",
                                                 MD_BAD_BLOCK  => "Bad block table",
                                                 _             => $"Unknown (0x{type:X2})"
                                             };

#region Constants

    /// <summary>End of VTOC table marker</summary>
    const byte MD_END = 0;

    /// <summary>Page space minidisk</summary>
    const byte MD_PAGE = 1;

    /// <summary>Dump space minidisk</summary>
    const byte MD_DUMP = 2;

    /// <summary>Free space for AIX</summary>
    const byte MD_UNALLOC = 3;

    /// <summary>AIX minidisk containing /unixtext</summary>
    const byte MD_AIX_BOOT = 8;

    /// <summary>Any other AIX minidisk</summary>
    const byte MD_AIX_NOBOOT = 9;

    /// <summary>Bad block table minidisk</summary>
    const byte MD_BAD_BLOCK = 0x0E;

#endregion

#region Nested type: Minidisk

    /// <inheritdoc />
    /// <summary>AIX minidisk entry structure</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct Minidisk
    {
        /// <summary>Starting block number</summary>
        public uint   s_block;
        /// <summary>Length of minidisk in blocks</summary>
        public uint   num_blks;
        /// <summary>Type of minidisk</summary>
        public byte   type;
        /// <summary>Minidisk ID number</summary>
        public byte   mdisk_id;
        /// <summary>Index into string table</summary>
        public ushort str_index;
    }

#endregion

#region Nested type: Vtoc1

    /// <inheritdoc />
    /// <summary>AIX VTOC sector 1 structure</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct Vtoc1
    {
        /// <summary>"VTOC" magic string</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] magic_string;
        /// <summary>Version number</summary>
        public byte   version;
        /// <summary>Sequence number (must match others)</summary>
        public byte   seq_num;
        /// <summary>Max length of boot 1 in disk blocks</summary>
        public ushort bootsize;
        /// <summary>Number of bad blocks (ST506 only)</summary>
        public ushort numbadblks;
        /// <summary>Offset within the bad block minidisk</summary>
        public ushort badblkoff;
        /// <summary>Spare (to put mini[] on 8 byte boundary)</summary>
        public int    vt_spare;
        /// <summary>Minidisk entries</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_MINIDISKS)]
        public Minidisk[] mini;
    }

#endregion
}