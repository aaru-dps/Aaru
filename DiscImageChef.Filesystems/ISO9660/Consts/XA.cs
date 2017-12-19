// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660 : Filesystem
    {
        const ushort XaMagic = 0x5841; // "XA"

        [Flags]
        enum XaAttributes : ushort
        {
            SystemRead = 0x01,
            SystemExecute = 0x04,
            OwnerRead = 0x10,
            OwnerExecute = 0x40,
            GroupRead = 0x100,
            GroupExecute = 0x400,
            Mode2Form1 = 0x800,
            Mode2Form2 = 0x1000,
            Interleaved = 0x2000,
            Cdda = 0x4000,
            Directory = 0x8000,
        }
    }
}