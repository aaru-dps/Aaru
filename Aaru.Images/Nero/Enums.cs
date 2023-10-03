// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains enumerations for Nero Burning ROM disc images.
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

using System;
using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class Nero
{
#region Nested type: DaoMode

    enum DaoMode : ushort
    {
        Data         = 0x0000,
        DataM2F1     = 0x0002,
        DataM2F2     = 0x0003,
        DataRaw      = 0x0005,
        DataM2Raw    = 0x0006,
        Audio        = 0x0007,
        AudioAlt     = 0x0008,
        DataRawSub   = 0x000F,
        AudioSub     = 0x0010,
        DataM2RawSub = 0x0011
    }

#endregion

#region Nested type: NeroMediaTypes

    [Flags]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    enum NeroMediaTypes : uint
    {
        /// <summary>No media</summary>
        NeroMtypNone = 0x00000,
        /// <summary>CD-R/RW</summary>
        NeroMtypCd = 0x00001,
        /// <summary>DDCD-R/RW</summary>
        NeroMtypDdcd = 0x00002,
        /// <summary>DVD-R/RW</summary>
        NeroMtypDvdM = 0x00004,
        /// <summary>DVD+RW</summary>
        NeroMtypDvdP = 0x00008,
        /// <summary>DVD-RAM</summary>
        NeroMtypDvdRam = 0x00010,
        /// <summary>Multi-level disc</summary>
        NeroMtypMl = 0x00020,
        /// <summary>Mount Rainier</summary>
        NeroMtypMrw = 0x00040,
        /// <summary>Exclude CD-R</summary>
        NeroMtypNoCdr = 0x00080,
        /// <summary>Exclude CD-RW</summary>
        NeroMtypNoCdrw = 0x00100,
        /// <summary>CD-RW</summary>
        NeroMtypCdrw = NeroMtypCd | NeroMtypNoCdr,
        /// <summary>CD-R</summary>
        NeroMtypCdr = NeroMtypCd | NeroMtypNoCdrw,
        /// <summary>DVD-ROM</summary>
        NeroMtypDvdRom = 0x00200,
        /// <summary>CD-ROM</summary>
        NeroMtypCdrom = 0x00400,
        /// <summary>Exclude DVD-RW</summary>
        NeroMtypNoDvdMRw = 0x00800,
        /// <summary>Exclude DVD-R</summary>
        NeroMtypNoDvdMR = 0x01000,
        /// <summary>Exclude DVD+RW</summary>
        NeroMtypNoDvdPRw = 0x02000,
        /// <summary>Exclude DVD+R</summary>
        NeroMtypNoDvdPR = 0x04000,
        /// <summary>DVD-R</summary>
        NeroMtypDvdMR = NeroMtypDvdM | NeroMtypNoDvdMRw,
        /// <summary>DVD-RW</summary>
        NeroMtypDvdMRw = NeroMtypDvdM | NeroMtypNoDvdMR,
        /// <summary>DVD+R</summary>
        NeroMtypDvdPR = NeroMtypDvdP | NeroMtypNoDvdPRw,
        /// <summary>DVD+RW</summary>
        NeroMtypDvdPRw = NeroMtypDvdP | NeroMtypNoDvdPR,
        /// <summary>Packet-writing (fixed)</summary>
        NeroMtypFpacket = 0x08000,
        /// <summary>Packet-writing (variable)</summary>
        NeroMtypVpacket = 0x10000,
        /// <summary>Packet-writing (any)</summary>
        NeroMtypPacketw = NeroMtypMrw | NeroMtypFpacket | NeroMtypVpacket,
        /// <summary>HD-Burn</summary>
        NeroMtypHdb = 0x20000,
        /// <summary>DVD+R DL</summary>
        NeroMtypDvdPR9 = 0x40000,
        /// <summary>DVD-R DL</summary>
        NeroMtypDvdMR9 = 0x80000,
        /// <summary>Any DVD double-layer</summary>
        NeroMtypDvdAnyR9 = NeroMtypDvdPR9 | NeroMtypDvdMR9,
        /// <summary>Any DVD</summary>
        NeroMtypDvdAny = NeroMtypDvdM | NeroMtypDvdP | NeroMtypDvdRam | NeroMtypDvdAnyR9,
        /// <summary>BD-ROM</summary>
        NeroMtypBdRom = 0x100000,
        /// <summary>BD-R</summary>
        NeroMtypBdR = 0x200000,
        /// <summary>BD-RE</summary>
        NeroMtypBdRe = 0x400000,
        /// <summary>BD-R/RE</summary>
        NeroMtypBd = NeroMtypBdR | NeroMtypBdRe,
        /// <summary>Any BD</summary>
        NeroMtypBdAny = NeroMtypBd | NeroMtypBdRom,
        /// <summary>HD DVD-ROM</summary>
        NeroMtypHdDvdRom = 0x0800000,
        /// <summary>HD DVD-R</summary>
        NeroMtypHdDvdR = 0x1000000,
        /// <summary>HD DVD-RW</summary>
        NeroMtypHdDvdRw = 0x2000000,
        /// <summary>HD DVD-R/RW</summary>
        NeroMtypHdDvd = NeroMtypHdDvdR | NeroMtypHdDvdRw,
        /// <summary>Any HD DVD</summary>
        NeroMtypHdDvdAny = NeroMtypHdDvd | NeroMtypHdDvdRom,
        /// <summary>Any DVD, old</summary>
        NeroMtypDvdAnyOld = NeroMtypDvdM | NeroMtypDvdP | NeroMtypDvdRam
    }

#endregion
}