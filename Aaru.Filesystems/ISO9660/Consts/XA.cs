// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : XA.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     CD-ROM XA extensions constants and enumerations.
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

using System;

// ReSharper disable UnusedMember.Local

namespace Aaru.Filesystems
{
    public sealed partial class ISO9660
    {
        const ushort XA_MAGIC = 0x5841; // "XA"

        [Flags]
        enum XaAttributes : ushort
        {
            SystemRead   = 0x01, SystemExecute = 0x04, OwnerRead     = 0x10,
            OwnerExecute = 0x40, GroupRead     = 0x100, GroupExecute = 0x400,
            Mode2Form1   = 0x800, Mode2Form2   = 0x1000, Interleaved = 0x2000,
            Cdda         = 0x4000, Directory   = 0x8000
        }

        [Flags]
        enum Mode2Submode : byte
        {
            EndOfFile = 0x80, RealTime    = 0x40, Form2 = 0x20,
            Trigger   = 0x10, Data        = 0x08, Audio = 0x04,
            Video     = 0x02, EndOfRecord = 0x01
        }
    }
}