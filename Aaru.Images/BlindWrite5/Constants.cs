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
//     Contains constants for BlindWrite 5 disc images.
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

namespace Aaru.DiscImages
{
    public sealed partial class BlindWrite5
    {
        /// <summary>"BWT5 STREAM FOOT"</summary>
        readonly byte[] _bw5Footer =
        {
            0x42, 0x57, 0x54, 0x35, 0x20, 0x53, 0x54, 0x52, 0x45, 0x41, 0x4D, 0x20, 0x46, 0x4F, 0x4F, 0x54
        };
        /// <summary>"BWT5 STREAM SIGN"</summary>
        readonly byte[] _bw5Signature =
        {
            0x42, 0x57, 0x54, 0x35, 0x20, 0x53, 0x54, 0x52, 0x45, 0x41, 0x4D, 0x20, 0x53, 0x49, 0x47, 0x4E
        };
    }
}