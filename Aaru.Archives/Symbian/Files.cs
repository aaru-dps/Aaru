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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.IO.Compression;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Filters;
using Aaru.Helpers.IO;
using FileAttributes = System.IO.FileAttributes;

namespace Aaru.Archives;

public sealed partial class Symbian
{
#region IArchive Members

    /// <inheritdoc />
    public int NumberOfEntries => Opened ? _files.Count : -1;

    /// <inheritdoc />
    public ErrorNumber GetFilename(int entryNumber, out string fileName)
    {
        fileName = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        fileName = _files[entryNumber].destinationName;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetEntryNumber(string fileName, bool caseInsensitiveMatch, out int entryNumber)
    {
        entryNumber = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        if(string.IsNullOrEmpty(fileName)) return ErrorNumber.InvalidArgument;

        entryNumber = _files.FindIndex(x => caseInsensitiveMatch
                                                ? x.destinationName.Equals(fileName,
                                                                           StringComparison.CurrentCultureIgnoreCase)
                                                : x.destinationName.Equals(fileName, StringComparison.CurrentCulture));

        return entryNumber < 0 ? ErrorNumber.NoSuchFile : ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetCompressedSize(int entryNumber, out long length)
    {
        length = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        length = _files[entryNumber].length;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetUncompressedSize(int entryNumber, out long length)
    {
        length = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        length = _compressed ? _files[entryNumber].originalLength : _files[entryNumber].length;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetAttributes(int entryNumber, out FileAttributes attributes)
    {
        attributes = FileAttributes.None;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        attributes = FileAttributes.Normal;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(int entryNumber, out FileEntryInfo stat)
    {
        stat = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        stat = new FileEntryInfo
        {
            Length     = _compressed ? _files[entryNumber].originalLength : _files[entryNumber].length,
            Attributes = CommonTypes.Structs.FileAttributes.File,
            Inode      = (ulong)entryNumber
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetEntry(int entryNumber, out IFilter filter)
    {
        filter = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        Stream stream = new OffsetStream(new NonClosableStream(_stream),
                                         _files[entryNumber].pointer,
                                         _files[entryNumber].pointer + _files[entryNumber].length);

        ErrorNumber errno;

        if(_compressed)
        {
            if(_files[entryNumber].originalLength == 0)
                stream = new MemoryStream([]);
            else
            {
                stream = new ForcedSeekStream<ZLibStream>(_files[entryNumber].originalLength,
                                                          stream,
                                                          CompressionMode.Decompress);
            }
        }

        filter = new ZZZNoFilter();
        errno  = filter.Open(stream);

        if(errno == ErrorNumber.NoError) return ErrorNumber.NoError;

        stream.Close();

        return errno;
    }

#endregion
}