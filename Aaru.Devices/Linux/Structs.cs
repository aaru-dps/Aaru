// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures necessary for directly interfacing devices under
//     Linux.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Devices.Linux;

[StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
struct SgIoHdrT
{
    /// <summary>Always 'S' for SG v3</summary>
    public int interface_id;                   /* [i] 'S' (required) */
    public ScsiIoctlDirection dxfer_direction; /* [i] */
    public byte               cmd_len;         /* [i] */
    public byte               mx_sb_len;       /* [i] */
    public ushort             iovec_count;     /* [i] */
    public uint               dxfer_len;       /* [i] */
    public nint               dxferp;          /* [i], [*io] */
    public nint               cmdp;            /* [i], [*i]  */
    public nint               sbp;             /* [i], [*o]  */
    public uint               timeout;         /* [i] unit: millisecs */
    public uint               flags;           /* [i] */
    public int                pack_id;         /* [i->o] */
    public nint               usr_ptr;         /* [i->o] */
    public byte               status;          /* [o] */
    public byte               masked_status;   /* [o] */
    public byte               msg_status;      /* [o] */
    public byte               sb_len_wr;       /* [o] */
    public ushort             host_status;     /* [o] */
    public ushort             driver_status;   /* [o] */
    public int                resid;           /* [o] */
    public uint               duration;        /* [o] */
    public SgInfo             info;            /* [o] */
}

[StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
struct MmcIocCmd
{
    /// <summary>Implies direction of data. true = write, false = read</summary>
    public bool write_flag;
    /// <summary>Application-specific command. true = precede with CMD55</summary>
    public bool is_ascmd;
    public uint opcode;
    public uint arg;
    /// <summary>CMD response</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] response;
    public MmcFlags flags;
    public uint     blksz;
    public uint     blocks;
    /// <summary>
    ///     Sleep at least <see cref="postsleep_min_us" /> useconds, and at most <see cref="postsleep_max_us" /> useconds
    ///     *after* issuing command.Needed for some read commands for which cards have no other way of indicating they're ready
    ///     for the next command (i.e. there is no equivalent of a "busy" indicator for read operations).
    /// </summary>
    public uint postsleep_min_us;
    /// <summary>
    ///     Sleep at least <see cref="postsleep_min_us" /> useconds, and at most <see cref="postsleep_max_us" /> useconds
    ///     *after* issuing command.Needed for some read commands for which cards have no other way of indicating they're ready
    ///     for the next command (i.e. there is no equivalent of a "busy" indicator for read operations).
    /// </summary>
    public uint postsleep_max_us;
    /// <summary>Override driver-computed timeouts.</summary>
    public uint data_timeout_ns;
    /// <summary>Override driver-computed timeouts.</summary>
    public uint cmd_timeout_ms;
    /// <summary>
    ///     For 64-bit machines <see cref="data_ptr" /> , wants to be 8-byte aligned.Make sure this struct is the same
    ///     size when built for 32-bit.
    /// </summary>
    public uint __pad;
    /// <summary>DAT buffer</summary>
    public ulong data_ptr;
}