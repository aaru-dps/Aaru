// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : RRIP.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     RRIP extensions structures.
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
// Copyright © 2011-2021 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems
{
    [SuppressMessage("ReSharper", "UnusedType.Local")]
    public sealed partial class ISO9660
    {
        // RRIP 1.10
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct PosixAttributesOld
        {
            public readonly ushort    signature;
            public readonly byte      length;
            public readonly byte      version;
            public readonly PosixMode st_mode;
            public readonly PosixMode st_mode_be;
            public readonly uint      st_nlink;
            public readonly uint      st_nlink_be;
            public readonly uint      st_uid;
            public readonly uint      st_uid_be;
            public readonly uint      st_gid;
            public readonly uint      st_gid_be;
        }

        // RRIP 1.12
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct PosixAttributes
        {
            public readonly ushort    signature;
            public readonly byte      length;
            public readonly byte      version;
            public readonly PosixMode st_mode;
            public readonly PosixMode st_mode_be;
            public readonly uint      st_nlink;
            public readonly uint      st_nlink_be;
            public readonly uint      st_uid;
            public readonly uint      st_uid_be;
            public readonly uint      st_gid;
            public readonly uint      st_gid_be;
            public readonly uint      st_ino;
            public readonly uint      st_ino_be;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct PosixDeviceNumber
        {
            public readonly ushort signature;
            public readonly byte   length;
            public readonly byte   version;
            public readonly uint   dev_t_high;
            public readonly uint   dev_t_high_be;
            public readonly uint   dev_t_low;
            public readonly uint   dev_t_low_be;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct SymbolicLink
        {
            public readonly ushort       signature;
            public readonly byte         length;
            public readonly byte         version;
            public readonly SymlinkFlags flags;

            // Followed by SymbolicLinkComponent (link to /bar/foo uses at least two of these structs)
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct SymbolicLinkComponent
        {
            public readonly SymlinkComponentFlags flags;
            public readonly byte                  length;

            // Followed by component content
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct AlternateName
        {
            public readonly ushort             signature;
            public readonly byte               length;
            public readonly byte               version;
            public readonly AlternateNameFlags flags;

            // Folowed by name, can be divided in pieces
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ChildLink
        {
            public readonly ushort signature;
            public readonly byte   length;
            public readonly byte   version;
            public readonly uint   child_dir_lba;
            public readonly uint   child_dir_lba_be;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ParentLink
        {
            public readonly ushort signature;
            public readonly byte   length;
            public readonly byte   version;
            public readonly uint   parent_dir_lba;
            public readonly uint   parent_dir_lba_be;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct RelocatedDirectory
        {
            public readonly ushort signature;
            public readonly byte   length;
            public readonly byte   version;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Timestamps
        {
            public readonly ushort         signature;
            public readonly byte           length;
            public readonly byte           version;
            public readonly TimestampFlags flags;

            // If flags indicate long format, timestamps are 17 bytes, if not, 7 bytes
            // Followed by creation time if present
            // Followed by modification time if present
            // Followed by access time if present
            // Followed by attribute change time if present
            // Followed by backup time if present
            // Followed by expiration time if present
            // Followed by effective time if present
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct SparseFile
        {
            public readonly ushort signature;
            public readonly byte   length;
            public readonly byte   version;
            public readonly uint   virtual_size_high;
            public readonly uint   virtual_size_high_be;
            public readonly uint   virtual_size_low;
            public readonly uint   virtual_size_low_be;
            public readonly byte   table_depth;
        }
    }
}