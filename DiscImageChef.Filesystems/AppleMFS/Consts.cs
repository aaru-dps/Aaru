// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Macintosh File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Apple Macintosh File System constants.
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

namespace DiscImageChef.Filesystems.AppleMFS
{
    // Information from Inside Macintosh Volume II
    public partial class AppleMFS : Filesystem
    {
        const ushort MFS_MAGIC = 0xD2D7;
        // "LK"
        const ushort MFSBB_MAGIC = 0x4C4B;

        const short DIRID_TRASH = -3;
        const short DIRID_DESKTOP = -2;
        const short DIRID_TEMPLATE = -1;
        const short DIRID_ROOT = 0;

        const int BMAP_FREE = 0;
        const int BMAP_LAST = 1;
        const int BMAP_DIR = 0xFFF;
    }
}

