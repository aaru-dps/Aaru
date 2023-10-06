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
//     Identifies partclone disk images.
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
using System.Linq;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class PartClone
{
#region IMediaImage Members

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512)
            return false;

        var pHdrB = new byte[Marshal.SizeOf<Header>()];
        stream.EnsureRead(pHdrB, 0, Marshal.SizeOf<Header>());
        _pHdr = Marshal.ByteArrayToStructureLittleEndian<Header>(pHdrB);

        if(stream.Position + (long)_pHdr.totalBlocks > stream.Length)
            return false;

        stream.Seek((long)_pHdr.totalBlocks, SeekOrigin.Current);

        var bitmagic = new byte[8];
        stream.EnsureRead(bitmagic, 0, 8);

        return _partCloneMagic.SequenceEqual(_pHdr.magic) && _biTmAgIc.SequenceEqual(bitmagic);
    }

#endregion
}