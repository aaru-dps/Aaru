// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : RRIP.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
using System.Runtime.InteropServices;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660 : Filesystem
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PosixAttributes
        {
            public ushort signature;
            public byte length;
            public byte version;
            public PosixMode st_mode;
            public PosixMode st_mode_be;
            public uint st_nlink;
            public uint st_nlink_be;
            public uint st_uid;
            public uint st_uid_be;
            public uint st_gid;
            public uint st_gid_be;
            public uint st_ino;
            public uint st_ino_be;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PosixDeviceNumber
        {
            public ushort signature;
            public byte length;
            public byte version;
            public uint dev_t_high;
            public uint dev_t_high_be;
            public uint dev_t_low;
            public uint dev_t_low_be;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SymbolicLink
        {
            public ushort signature;
            public byte length;
            public byte version;
            public SymlinkFlags flags;
            // Followed by SymbolicLinkComponent (link to /bar/foo uses at least two of these structs)
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SymbolicLinkComponent
        {
            public SymlinkComponentFlags flags;
            public byte length;
            // Followed by component content
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AlternateName
        {
            public ushort signature;
            public byte length;
            public byte version;
            public AlternateNameFlags flags;
            // Folowed by name, can be divided in pieces
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChildLink
        {
            public ushort signature;
            public byte length;
            public byte version;
            public uint child_dir_lba;
            public uint child_dir_lba_be;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ParentLink
        {
            public ushort signature;
            public byte length;
            public byte version;
            public uint parent_dir_lba;
            public uint parent_dir_lba_be;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RelocatedDirectory
        {
            public ushort signature;
            public byte length;
            public byte version;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Timestamps
        {
            public ushort signature;
            public byte length;
            public byte version;
            public TimestampFlags flags;
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
        struct SparseFile
        {
            public ushort signature;
            public byte length;
            public byte version;
            public uint virtual_size_high;
            public uint virtual_size_high_be;
            public uint virtual_size_low;
            public uint virtual_size_low_be;
            public byte table_depth;
        }
    }
}
