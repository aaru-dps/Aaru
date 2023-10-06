// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for Apridisk disk images.
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

namespace Aaru.Images;

public sealed partial class Apridisk
{
    (ushort cylinder, byte head, byte sector) LbaToChs(ulong lba)
    {
        var cylinder = (ushort)(lba                            / (_imageInfo.Heads * _imageInfo.SectorsPerTrack));
        var head     = (byte)(lba / _imageInfo.SectorsPerTrack % _imageInfo.Heads);
        var sector   = (byte)(lba % _imageInfo.SectorsPerTrack + 1);

        return (cylinder, head, sector);
    }
}