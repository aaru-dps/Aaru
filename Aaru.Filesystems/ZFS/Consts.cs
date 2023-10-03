// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ZFS filesystem plugin.
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

/*
 * The ZFS on-disk structure is quite undocumented, so this has been checked using several test images and reading the comments and headers (but not the code)
 * of ZFS-On-Linux.
 *
 * The most basic structure, the vdev label, is as follows:
 * 8KiB of blank space
 * 8KiB reserved for boot code, stored as a ZIO block with magic and checksum
 * 112KiB of nvlist, usually encoded using XDR
 * 128KiB of copies of the 1KiB uberblock
 *
 * Two vdev labels, L0 and L1 are stored at the start of the vdev.
 * Another two, L2 and L3 are stored at the end.
 *
 * The nvlist is nothing more than a double linked list of name/value pairs where name is a string and value is an arbitrary type (and can be an array of it).
 * On-disk they are stored sequentially (no pointers) and can be encoded in XDR (an old Sun serialization method that stores everything as 4 bytes chunks) or
 * natively (that is as the host natively stores that values, for example on Intel an extended float would be 10 bytes (80 bit).
 * It can also be encoded little or big endian.
 * Because of this variations, ZFS stored a header indicating the used encoding and endianess before the encoded nvlist.
 */
/// <inheritdoc />
/// <summary>Implements detection for the Zettabyte File System (ZFS)</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Local")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public sealed partial class ZFS
{
    const ulong ZEC_MAGIC = 0x0210DA7AB10C7A11;
    const ulong ZEC_CIGAM = 0x117A0CB17ADA1002;

    // These parameters define how the nvlist is stored
    const byte NVS_LITTLE_ENDIAN = 1;
    const byte NVS_BIG_ENDIAN    = 0;
    const byte NVS_NATIVE        = 0;
    const byte NVS_XDR           = 1;

    const ulong UBERBLOCK_MAGIC = 0x00BAB10C;

    const uint ZFS_MAGIC = 0x58465342;

    const string FS_TYPE = "zfs";
}