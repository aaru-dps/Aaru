// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Lisa filesystem plugin.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Filesystems;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class LisaFS
{
    /// <summary>Lisa FS v1, from Lisa OS 1.0 (Workshop or Office) Never seen on Sony floppies.</summary>
    const byte LISA_V1 = 0x0E;
    /// <summary>
    ///     Lisa FS v2, from Lisa OS 2.0 (Workshop or Office) Contrary to what most information online says the only
    ///     difference with V1 is the Extents File size. Catalog format is the same
    /// </summary>
    const byte LISA_V2 = 0x0F;
    /// <summary>
    ///     Lisa FS v3, from Lisa OS 3.0 (Workshop or Office) Adds support for user catalogs (aka subdirectories), and
    ///     changes the catalog format from extents to double-linked list. Uses '-' as path separator (so people that created
    ///     Lisa/FILE.TEXT just created a file named like that :p)
    /// </summary>
    const byte LISA_V3 = 0x11;
    /// <summary>Maximum string size in LisaFS</summary>
    const uint E_NAME = 32;
    /// <summary>Unused file ID</summary>
    const ushort FILEID_FREE = 0x0000;
    /// <summary>Used by the boot blocks</summary>
    const ushort FILEID_BOOT = 0xAAAA;
    /// <summary>Used by the operating system loader blocks</summary>
    const ushort FILEID_LOADER = 0xBBBB;
    /// <summary>Used by the MDDF</summary>
    const ushort FILEID_MDDF = 0x0001;
    /// <summary>Used by the volume bitmap, sits between MDDF and S-Records file.</summary>
    const ushort FILEID_BITMAP = 0x0002;
    /// <summary>S-Records file</summary>
    const ushort FILEID_SRECORD = 0x0003;
    /// <summary>The root catalog</summary>
    const ushort FILEID_CATALOG = 0x0004;
    const short FILEID_BOOT_SIGNED   = -21846;
    const short FILEID_LOADER_SIGNED = -17477;
    /// <summary>A file that has been erased</summary>
    const ushort FILEID_ERASED = 0x7FFF;
    const ushort FILEID_MAX = FILEID_ERASED;

    /// <summary>Root directory ID</summary>
    const short DIRID_ROOT = 0;

    enum FileType : byte
    {
        /// <summary>Undefined file type</summary>
        Undefined = 0,
        /// <summary>MDDF</summary>
        MDDFile = 1,
        /// <summary>Root catalog</summary>
        RootCat = 2,
        /// <summary>Bitmap</summary>
        FreeList = 3,
        /// <summary>Unknown, maybe refers to the S-Records File?</summary>
        BadBlocks = 4,
        /// <summary>System data</summary>
        SysData = 5,
        /// <summary>Printer spool</summary>
        Spool = 6,
        /// <summary>Executable. Yet application files don't use it</summary>
        Exec = 7,
        /// <summary>User catalog</summary>
        UserCat = 8,
        /// <summary>Pipe. Not seen on disk.</summary>
        Pipe = 9,
        /// <summary>Boot file?</summary>
        BootFile = 10,
        /// <summary>Swap for data</summary>
        SwapData = 11,
        /// <summary>Swap for code</summary>
        SwapCode = 12,
        /// <summary>Unknown</summary>
        RamAP = 13,
        /// <summary>Any file</summary>
        UserFile = 14,
        /// <summary>Erased?</summary>
        KilledObject = 15
    }

    const string FS_TYPE = "lisafs";
}