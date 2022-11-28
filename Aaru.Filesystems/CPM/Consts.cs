// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CP/M filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     CP/M filesystem constants.
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

// ReSharper disable InconsistentNaming

namespace Aaru.Filesystems;

public sealed partial class CPM
{
    // Do not translate
    const string FS_TYPE = "cpmfs";

    /// <summary>Enumerates the format identification byte used by CP/M-86</summary>
    enum FormatByte : byte
    {
        /// <summary>5.25" double-density single-side 8 sectors/track</summary>
        k160 = 0,
        /// <summary>5.25" double-density double-side 8 sectors/track</summary>
        k320 = 1,
        /// <summary>5.25" double-density double-side 9 sectors/track</summary>
        k360 = 0x10,
        /// <summary>5.25" double-density double-side 9 sectors/track</summary>
        k360Alt = 0x40,
        /// <summary>3.5" double-density double-side 9 sectors/track</summary>
        k720 = 0x11,
        /// <summary>3.5" double-density double-side 9 sectors/track using FEAT144</summary>
        f720 = 0x48,
        /// <summary>5.25" high-density double-side 15 sectors/track using FEAT144</summary>
        f1200 = 0x0C,
        /// <summary>3.5" high-density double-side 18 sectors/track using FEAT144</summary>
        f1440 = 0x90,
        /// <summary>5.25" double-density double-side 9 sectors/track</summary>
        k360Alt2 = 0x26,
        /// <summary>3.5" double-density double-side 9 sectors/track</summary>
        k720Alt = 0x94
    }
}