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
//     Contains enumerations for Dunfield's IMD disk images.
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

namespace Aaru.DiscImages;

public sealed partial class Imd
{
    enum TransferRate : byte
    {
        /// <summary>500 kbps in FM mode</summary>
        FiveHundred = 0,
        /// <summary>300 kbps in FM mode</summary>
        ThreeHundred = 1,
        /// <summary>250 kbps in FM mode</summary>
        TwoHundred = 2,
        /// <summary>500 kbps in MFM mode</summary>
        FiveHundredMfm = 3,
        /// <summary>300 kbps in MFM mode</summary>
        ThreeHundredMfm = 4,
        /// <summary>250 kbps in MFM mode</summary>
        TwoHundredMfm = 5
    }

    enum SectorType : byte
    {
        Unavailable            = 0,
        Normal                 = 1,
        Compressed             = 2,
        Deleted                = 3,
        CompressedDeleted      = 4,
        Error                  = 5,
        CompressedError        = 6,
        DeletedError           = 7,
        CompressedDeletedError = 8
    }
}