// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace DiscImageChef.Devices.Linux
{
    [StructLayout(LayoutKind.Sequential)]
    struct sg_io_hdr_t
    {
        /// <summary>
        /// Always 'S' for SG v3
        /// </summary>
        public int interface_id;           /* [i] 'S' (required) */
        public ScsiIoctlDirection dxfer_direction;        /* [i] */
        public byte cmd_len;      /* [i] */
        public byte mx_sb_len;    /* [i] */
        public ushort iovec_count; /* [i] */
        public uint dxfer_len;     /* [i] */
        public IntPtr dxferp;              /* [i], [*io] */
        public IntPtr cmdp;       /* [i], [*i]  */
        public IntPtr sbp;        /* [i], [*o]  */
        public uint timeout;       /* [i] unit: millisecs */
        public uint flags;         /* [i] */
        public int pack_id;                /* [i->o] */
        public IntPtr usr_ptr;             /* [i->o] */
        public byte status;       /* [o] */
        public byte masked_status;/* [o] */
        public byte msg_status;   /* [o] */
        public byte sb_len_wr;    /* [o] */
        public ushort host_status; /* [o] */
        public ushort driver_status;/* [o] */
        public int resid;                  /* [o] */
        public uint duration;      /* [o] */
        public SgInfo info;          /* [o] */
    }
}

