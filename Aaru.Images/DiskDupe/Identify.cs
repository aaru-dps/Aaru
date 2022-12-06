// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies DiskDupe DDI disk images.
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
// Copyright © 2021-2023 Michael Drüing
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.DiscImages
{
    public sealed partial class DiskDupe
    {
        /// <inheritdoc />
        public bool Identify(IFilter imageFilter)
        {
            Stream      stream       = imageFilter.GetDataForkStream();
            var         fHeader      = new FileHeader();
            TrackInfo[] trackMap     = null;
            long[]      trackOffsets = null;

            // TODO: validate the tracks
            // For now, having a valid header should be sufficient.
            return TryReadHeader(stream, ref fHeader, ref trackMap, ref trackOffsets);
        }
    }
}