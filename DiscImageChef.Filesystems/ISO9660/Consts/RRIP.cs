// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : RRIP.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     RRIP extensions constants and enumerations.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        const ushort RRIP_MAGIC            = 0x5252; // "RR"
        const ushort RRIP_POSIX_ATTRIBUTES = 0x5058; // "PX"
        const ushort RRIP_POSIX_DEV_NO     = 0x504E; // "PN"
        const ushort RRIP_SYMLINK          = 0x534C; // "SL"
        const ushort RRIP_NAME             = 0x4E4D; // "NM"
        const ushort RRIP_CHILDLINK        = 0x434C; // "CL"
        const ushort RRIP_PARENTLINK       = 0x504C; // "PL"
        const ushort RRIP_RELOCATED_DIR    = 0x5245; // "RE"
        const ushort RRIP_TIMESTAMPS       = 0x5446; // "TF"
        const ushort RRIP_SPARSE           = 0x5346; // "SF"

        [Flags]
        enum PosixMode : uint
        {
            OwnerRead    = 0x100,
            OwnerWrite   = 0x80,
            OwnerExecute = 0x40,
            GroupRead    = 0x20,
            GroupWrite   = 0x10,
            GroupExecute = 0x8,
            OtherRead    = 0x4,
            OtherWrite   = 0x2,
            OtherExecute = 0x1,
            SetUID       = 0x800,
            SetGid       = 0x400,
            IsVTX        = 0x200,
            Socket       = 0xC000,
            Symlink      = 0xA000,
            Regular      = 0x8000,
            Block        = 0x6000,
            Character    = 0x2000,
            Directory    = 0x4000,
            Pipe         = 0x1000
        }

        [Flags]
        enum SymlinkFlags : byte
        {
            Continue = 1
        }

        [Flags]
        enum SymlinkComponentFlags : byte
        {
            Continue    = 1,
            Current     = 2,
            Parent      = 4,
            Root        = 8,
            Mountpoint  = 16,
            Networkname = 32
        }

        [Flags]
        enum AlternateNameFlags : byte
        {
            Continue    = 1,
            Current     = 2,
            Parent      = 4,
            Networkname = 32
        }

        [Flags]
        enum TimestampFlags : byte
        {
            Creation        = 1 << 0,
            Modification    = 1 << 1,
            Access          = 1 << 2,
            AttributeChange = 1 << 3,
            Backup          = 1 << 4,
            Expiration      = 1 << 5,
            Effective       = 1 << 6,
            LongFormat      = 1 << 7
        }
    }
}