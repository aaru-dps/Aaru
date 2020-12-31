// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ZZZNoFilter.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides a filter to open single files.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filters
{
    /// <summary>No filter for reading files not recognized by any filter</summary>
    public sealed class ZZZNoFilter : IFilter
    {
        string   _basePath;
        DateTime _creationTime;
        Stream   _dataStream;
        DateTime _lastWriteTime;
        bool     _opened;

        public string Name   => "No filter";
        public Guid   Id     => new Guid("12345678-AAAA-BBBB-CCCC-123456789000");
        public string Author => "Natalia Portillo";

        public void Close()
        {
            _dataStream?.Close();
            _dataStream = null;
            _basePath   = null;
            _opened     = false;
        }

        public string GetBasePath() => _basePath;

        public Stream GetDataForkStream() => _dataStream;

        public string GetPath() => _basePath;

        public Stream GetResourceForkStream() => null;

        public bool HasResourceFork() => false;

        public bool Identify(byte[] buffer) => buffer != null && buffer.Length > 0;

        public bool Identify(Stream stream) => stream != null && stream.Length > 0;

        public bool Identify(string path) => File.Exists(path);

        public void Open(byte[] buffer)
        {
            _dataStream    = new MemoryStream(buffer);
            _basePath      = null;
            _creationTime  = DateTime.UtcNow;
            _lastWriteTime = _creationTime;
            _opened        = true;
        }

        public void Open(Stream stream)
        {
            _dataStream    = stream;
            _basePath      = null;
            _creationTime  = DateTime.UtcNow;
            _lastWriteTime = _creationTime;
            _opened        = true;
        }

        public void Open(string path)
        {
            _dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            _basePath   = Path.GetFullPath(path);
            var fi = new FileInfo(path);
            _creationTime  = fi.CreationTimeUtc;
            _lastWriteTime = fi.LastWriteTimeUtc;
            _opened        = true;
        }

        public DateTime GetCreationTime() => _creationTime;

        public long GetDataForkLength() => _dataStream.Length;

        public DateTime GetLastWriteTime() => _lastWriteTime;

        public long GetLength() => _dataStream.Length;

        public long GetResourceForkLength() => 0;

        public string GetFilename() => Path.GetFileName(_basePath);

        public string GetParentFolder() => Path.GetDirectoryName(_basePath);

        public bool IsOpened() => _opened;
    }
}