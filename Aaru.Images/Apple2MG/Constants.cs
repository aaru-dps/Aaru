// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constants.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for XGS emulator disk images.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public sealed partial class Apple2Mg
    {
        /// <summary>Magic number, "2IMG"</summary>
        const uint MAGIC = 0x474D4932;
        /// <summary>Disk image created by ASIMOV2, "!nfc"</summary>
        const uint CREATOR_ASIMOV = 0x63666E21;
        /// <summary>Disk image created by Bernie ][ the Rescue, "B2TR"</summary>
        const uint CREATOR_BERNIE = 0x52543242;
        /// <summary>Disk image created by Catakig, "CTKG"</summary>
        const uint CREATOR_CATAKIG = 0x474B5443;
        /// <summary>Disk image created by Sheppy's ImageMaker, "ShIm"</summary>
        const uint CREATOR_SHEPPY = 0x6D496853;
        /// <summary>Disk image created by Sweet16, "WOOF"</summary>
        const uint CREATOR_SWEET = 0x464F4F57;
        /// <summary>Disk image created by XGS, "XGS!"</summary>
        const uint CREATOR_XGS = 0x21534758;
        /// <summary>Disk image created by CiderPress, "CdrP"</summary>
        const uint CREATOR_CIDER = 0x50726443;
        /// <summary>Disk image created by DiscImageChef, "dic "</summary>
        const uint CREATOR_DIC = 0x20636964;
        /// <summary>Disk image created by Aaru, "aaru"</summary>
        const uint CREATOR_AARU = 0x75726161;

        const uint LOCKED_DISK         = 0x80000000;
        const uint VALID_VOLUME_NUMBER = 0x00000100;
        const uint VOLUME_NUMBER_MASK  = 0x000000FF;
        readonly int[] _deinterleave =
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
        };
        readonly int[] _interleave =
        {
            0, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 15
        };
    }
}