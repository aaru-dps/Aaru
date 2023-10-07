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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Archives;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class Symbian
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding) => throw new NotImplementedException();

    /// <inheritdoc />
    public void Close()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public int GetNumberOfEntries() => throw new NotImplementedException();

    /// <inheritdoc />
    public string GetFilename(int entryNumber) => throw new NotImplementedException();

    /// <inheritdoc />
    public int GetEntryNumber(string fileName, bool caseInsensitiveMatch) => throw new NotImplementedException();

    /// <inheritdoc />
    public long GetCompressedSize(int entryNumber) => throw new NotImplementedException();

    /// <inheritdoc />
    public long GetUncompressedSize(int entryNumber) => throw new NotImplementedException();

    /// <inheritdoc />
    public FileAttributes GetAttributes(int entryNumber) => throw new NotImplementedException();

    /// <inheritdoc />
    public List<string> GetXAttrs(int entryNumber) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber GetXattr(int entryNumber, string xattr, out byte[] buffer) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public FileSystemInfo Stat(int entryNumber) => throw new NotImplementedException();

    /// <inheritdoc />
    public IFilter GetEntry(int entryNumber) => throw new NotImplementedException();

#endregion
}