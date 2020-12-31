// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ISO.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     ISO9660 filesystem constants and enumerations.
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
        const string ISO_MAGIC = "CD001";

        [Flags]
        enum FileFlags : byte
        {
            Hidden = 0x01, Directory = 0x02, Associated  = 0x04,
            Record = 0x08, Protected = 0x10, MultiExtent = 0x80
        }

        [Flags]
        enum Permissions : ushort
        {
            SystemRead   = 0x01, SystemExecute  = 0x04, OwnerRead     = 0x10,
            OwnerExecute = 0x40, GroupRead      = 0x100, GroupExecute = 0x400,
            OtherRead    = 0x1000, OtherExecute = 0x4000
        }

        enum RecordFormat : byte
        {
            Unspecified             = 0, FixedLength = 1, VariableLength = 2,
            VariableLengthAlternate = 3
        }

        enum RecordAttribute : byte
        {
            LFCR = 0, ISO1539 = 1, ControlContained = 2
        }
    }
}