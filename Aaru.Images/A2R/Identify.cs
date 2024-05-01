// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Rebecca Wallander <sakcheen@gmail.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies A2R flux images.
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
// Copyright © 2011-2024 Rebecca Wallander
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class A2R
{
#region IFluxImage Members

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 8) return false;

        var hdr = new byte[4];

        stream.EnsureRead(hdr, 0, 4);

        return _a2Rv2Signature.SequenceEqual(hdr) || _a2Rv3Signature.SequenceEqual(hdr);
    }

#endregion
}