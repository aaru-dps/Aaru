// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages
{
    public partial class Apridisk
    {
        (ushort cylinder, byte head, byte sector) LbaToChs(ulong lba)
        {
            ushort cylinder = (ushort)(lba                           / (imageInfo.Heads * imageInfo.SectorsPerTrack));
            byte   head     = (byte)(lba / imageInfo.SectorsPerTrack % imageInfo.Heads);
            byte   sector   = (byte)(lba % imageInfo.SectorsPerTrack + 1);

            return (cylinder, head, sector);
        }
    }
}