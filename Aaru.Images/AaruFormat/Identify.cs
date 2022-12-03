// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies Aaru Format disk images.
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

using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class AaruFormat
{
    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        _imageStream = imageFilter.GetDataForkStream();
        _imageStream.Seek(0, SeekOrigin.Begin);

        if(_imageStream.Length < Marshal.SizeOf<AaruHeader>())
            return false;

        _structureBytes = new byte[Marshal.SizeOf<AaruHeader>()];
        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
        _header = Marshal.ByteArrayToStructureLittleEndian<AaruHeader>(_structureBytes);

        return _header.identifier is DIC_MAGIC or AARU_MAGIC && _header.imageMajorVersion <= AARUFMT_VERSION;
    }
}