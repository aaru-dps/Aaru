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
//     Identifies Connectix and Microsoft Virtual PC disk images.
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

public sealed partial class Vhd
{
#region IWritableImage Members

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        Stream imageStream = imageFilter.GetDataForkStream();

        var headerCookieBytes = new byte[8];
        var footerCookieBytes = new byte[8];

        if(imageStream.Length % 2 == 0)
            imageStream.Seek(-512, SeekOrigin.End);
        else
            imageStream.Seek(-511, SeekOrigin.End);

        imageStream.EnsureRead(footerCookieBytes, 0, 8);
        imageStream.Seek(0, SeekOrigin.Begin);
        imageStream.EnsureRead(headerCookieBytes, 0, 8);

        var headerCookie = BigEndianBitConverter.ToUInt64(headerCookieBytes, 0);
        var footerCookie = BigEndianBitConverter.ToUInt64(footerCookieBytes, 0);

        return headerCookie == IMAGE_COOKIE || footerCookie == IMAGE_COOKIE;
    }

#endregion
}