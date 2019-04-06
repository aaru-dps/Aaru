// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     FATX filesystem constants.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;

namespace DiscImageChef.Filesystems.FATX
{
    public partial class XboxFatPlugin
    {
        const uint FATX_MAGIC       = 0x58544146;
        const byte UNUSED_DIRENTRY  = 0x00;
        const byte DELETED_DIRENTRY = 0xE5;
        const byte MAX_FILENAME     = 42;

        [Flags]
        enum Attributes : byte
        {
            ReadOnly  = 0x01,
            Hidden    = 0x02,
            System    = 0x04,
            Directory = 0x10,
            Archive   = 0x20
        }
    }
}