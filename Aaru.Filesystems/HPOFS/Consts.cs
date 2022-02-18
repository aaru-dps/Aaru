// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     High Performance Optical File System constants.
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

namespace Aaru.Filesystems
{
    public sealed partial class HPOFS
    {
        readonly byte[] _type =
        {
            0x48, 0x50, 0x4F, 0x46, 0x53, 0x00, 0x00, 0x00
        };
        readonly byte[] _medinfoSignature =
        {
            0x4D, 0x45, 0x44, 0x49, 0x4E, 0x46, 0x4F, 0x20
        };
        readonly byte[] _volinfoSignature =
        {
            0x56, 0x4F, 0x4C, 0x49, 0x4E, 0x46, 0x4F, 0x20
        };
    }
}