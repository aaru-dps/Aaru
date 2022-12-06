// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common Apple file systems.
//
// --[ Description ] ----------------------------------------------------------
//
//     Common Apple file systems constants.
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

namespace Aaru.Filesystems
{
    // Information from Inside Macintosh
    // https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
    internal static partial class AppleCommon
    {
        /// <summary>"LK", HFS bootblock magic</summary>
        internal const ushort BB_MAGIC = 0x4C4B;
        /// <summary>"BD", HFS magic</summary>
        internal const ushort HFS_MAGIC = 0x4244;
        /// <summary>"H+", HFS+ magic</summary>
        internal const ushort HFSP_MAGIC = 0x482B;
        /// <summary>"HX", HFSX magic</summary>
        internal const ushort HFSX_MAGIC = 0x4858;
    }
}