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
//     Contains enumerations for SuperCardPro flux images.
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

namespace Aaru.DiscImages
{
    public sealed partial class SuperCardPro
    {
        public enum ScpDiskType : byte
        {
            Commodore64 = 0x00, CommodoreAmiga = 0x04, AtariFMSS  = 0x10,
            AtariFMDS   = 0x11, AtariFSEx      = 0x12, AtariSTSS  = 0x14,
            AtariSTDS   = 0x15, AppleII        = 0x20, AppleIIPro = 0x21,
            Apple400K   = 0x24, Apple800K      = 0x25, Apple144   = 0x26,
            PC360K      = 0x30, PC720K         = 0x31, PC12M      = 0x32,
            PC144M      = 0x33, TandySSSD      = 0x40, TandySSDD  = 0x41,
            TandyDSSD   = 0x42, TandyDSDD      = 0x43, Ti994A     = 0x50,
            RolandD20   = 0x60
        }

        [Flags]
        public enum ScpFlags : byte
        {
            /// <summary>If set flux starts at index pulse</summary>
            Index = 0x00,
            /// <summary>If set drive is 96tpi</summary>
            Tpi = 0x02,
            /// <summary>If set drive is 360rpm</summary>
            Rpm = 0x04,
            /// <summary>If set image contains normalized data</summary>
            Normalized = 0x08,
            /// <summary>If set image is read/write capable</summary>
            Writable = 0x10,
            /// <summary>If set, image has footer</summary>
            HasFooter = 0x20
        }
    }
}