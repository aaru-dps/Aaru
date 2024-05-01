// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Symbian.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Symbian plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies Symbian installer (.sis) packages and shows information.
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

using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;

namespace Aaru.Archives;

public sealed partial class Symbian
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber ListXAttr(int entryNumber, out List<string> xattrs)
    {
        xattrs = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        xattrs = new List<string>();

        if(_files[entryNumber].mime is not null) xattrs.Add("org.iana.mime_type");

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(int entryNumber, string xattr, ref byte[] buffer)
    {
        buffer = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        if(xattr != "org.iana.mime_type" || _files[entryNumber].mime is null)
            return ErrorNumber.NoSuchExtendedAttribute;

        buffer = Encoding.ASCII.GetBytes(_files[entryNumber].mime);

        return ErrorNumber.NoError;
    }

#endregion
}