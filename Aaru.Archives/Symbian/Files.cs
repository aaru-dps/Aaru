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

using System;

namespace Aaru.Archives;

public sealed partial class Symbian
{
#region IArchive Members

    /// <inheritdoc />
    public int GetNumberOfEntries() => _opened ? _files.Count : -1;

    /// <inheritdoc />
    public string GetFilename(int entryNumber)
    {
        if(!_opened)
            return null;

        if(entryNumber < 0 || entryNumber >= _files.Count)
            return null;

        return _files[entryNumber].destinationName;
    }

    /// <inheritdoc />
    public int GetEntryNumber(string fileName, bool caseInsensitiveMatch)
    {
        if(!_opened)
            return -1;

        if(string.IsNullOrEmpty(fileName))
            return -1;

        return _files.FindIndex(x => caseInsensitiveMatch
                                         ? x.destinationName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)
                                         : x.destinationName.Equals(fileName, StringComparison.CurrentCulture));
    }

#endregion
}