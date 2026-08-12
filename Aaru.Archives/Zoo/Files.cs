// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Files.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Zoo plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Compression;
using Aaru.Filters;
using Aaru.Helpers;
using Aaru.Helpers.IO;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Archives;

public sealed partial class Zoo
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber GetFilename(int entryNumber, out string fileName)
    {
        fileName = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        Direntry entry = _files[entryNumber];

        fileName = StringHandlers.CToString(entry.lfname ?? entry.fname, _encoding);

        if(entry.dirname is null) return ErrorNumber.NoError;

        string directoryName = StringHandlers.CToString(entry.dirname, _encoding);

        // Path separators are UNIX in archive, change them
        if(entry.system_id != 1 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            directoryName = directoryName.Replace('/', '\\');
        else
            directoryName = directoryName.Replace('\\', '/');

        fileName = Path.Combine(directoryName, fileName);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetCompressedSize(int entryNumber, out long length)
    {
        length = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        Direntry entry = _files[entryNumber];

        length = entry.size_now;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetUncompressedSize(int entryNumber, out long length)
    {
        length = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        Direntry entry = _files[entryNumber];

        length = entry.org_size;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetEntryNumber(string fileName, bool caseInsensitiveMatch, out int entryNumber)
    {
        // This can be done faster, it's 7am, gimme a break
        entryNumber = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        StringComparison comparison = caseInsensitiveMatch
                                          ? StringComparison.CurrentCultureIgnoreCase
                                          : StringComparison.CurrentCulture;

        for(int i = 0, count = _files.Count; i < count; i++)
        {
            if(GetFilename(i, out string name) != ErrorNumber.NoError || !name.Equals(fileName, comparison)) continue;

            entryNumber = i;

            return ErrorNumber.NoError;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(int entryNumber, out FileEntryInfo stat)
    {
        stat = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        Direntry entry = _files[entryNumber];

        stat = new FileEntryInfo
        {
            Length           = entry.org_size,
            Attributes       = FileAttributes.File,
            Blocks           = entry.org_size / 512,
            BlockSize        = 512,
            LastWriteTime    = DateHandlers.DosToDateTime(entry.date, entry.time),
            LastWriteTimeUtc = DateHandlers.DosToDateTime(entry.date, entry.time) // TODO: Handle tz, when not 127
        };

        if(entry.org_size % 512 != 0) stat.Blocks++;

        // POSIX permissions, DOS version of ZOO basically ignored the attributes
        if((entry.fattr & 1 << 22) > 0) stat.Mode = entry.fattr & 0x1ff;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetEntry(int entryNumber, out IFilter filter)
    {
        filter = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _files.Count) return ErrorNumber.OutOfRange;

        Direntry entry = _files[entryNumber];

        if(entry.packing_method > 2) return ErrorNumber.InvalidArgument;

        Stream stream;

        if(entry.org_size == 0)
            stream = new MemoryStream([]);
        else
        {
            // Validate against the file size, OffsetStream throws on invalid bounds
            if(entry.size_now <= 0 || entry.offset < 0 || entry.offset + entry.size_now > _stream.Length)
                return ErrorNumber.InvalidArgument;

            stream = new OffsetStream(new NonClosableStream(_stream), entry.offset, entry.offset + entry.size_now - 1);
        }

        stream = entry.packing_method switch
                 {
                     1 => new ForcedSeekStream<LzdStream>(entry.org_size, stream),
                     2 => new Lh5Stream(stream, entry.org_size),
                     _ => stream
                 };

        filter = new ZZZNoFilter();
        ErrorNumber errno = filter.Open(stream);

        if(errno == ErrorNumber.NoError) return ErrorNumber.NoError;

        stream.Close();

        return errno;
    }

#endregion
}