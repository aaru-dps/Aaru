// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : GZip.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Allow to open files that are compressed using gzip.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.IO.Compression;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Filters
{
    /// <summary>Decompress gzip files while reading</summary>
    public sealed class GZip : IFilter
    {
        string   _basePath;
        DateTime _creationTime;
        Stream   _dataStream;
        uint     _decompressedSize;
        DateTime _lastWriteTime;
        bool     _opened;
        Stream   _zStream;

        public string Name   => "GZip";
        public Guid   Id     => new Guid("F4996661-4A29-42C9-A2C7-3904EF40F3B0");
        public string Author => "Natalia Portillo";

        public void Close()
        {
            _dataStream?.Close();
            _dataStream = null;
            _basePath   = null;
            _opened     = false;
        }

        public string GetBasePath() => _basePath;

        public Stream GetDataForkStream() => _zStream;

        public string GetPath() => _basePath;

        public Stream GetResourceForkStream() => null;

        public bool HasResourceFork() => false;

        public bool Identify(byte[] buffer) => buffer[0] == 0x1F && buffer[1] == 0x8B && buffer[2] == 0x08;

        public bool Identify(Stream stream)
        {
            byte[] buffer = new byte[3];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 3);
            stream.Seek(0, SeekOrigin.Begin);

            return buffer[0] == 0x1F && buffer[1] == 0x8B && buffer[2] == 0x08;
        }

        public bool Identify(string path)
        {
            if(!File.Exists(path))
                return false;

            var    stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[3];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 3);
            stream.Seek(0, SeekOrigin.Begin);

            return buffer[0] == 0x1F && buffer[1] == 0x8B && buffer[2] == 0x08;
        }

        public void Open(byte[] buffer)
        {
            byte[] mtimeB = new byte[4];
            byte[] isizeB = new byte[4];

            _dataStream = new MemoryStream(buffer);
            _basePath   = null;

            _dataStream.Seek(4, SeekOrigin.Begin);
            _dataStream.Read(mtimeB, 0, 4);
            _dataStream.Seek(-4, SeekOrigin.End);
            _dataStream.Read(isizeB, 0, 4);
            _dataStream.Seek(0, SeekOrigin.Begin);

            uint mtime = BitConverter.ToUInt32(mtimeB, 0);
            uint isize = BitConverter.ToUInt32(isizeB, 0);

            _decompressedSize = isize;
            _creationTime     = DateHandlers.UnixUnsignedToDateTime(mtime);
            _lastWriteTime    = _creationTime;

            _zStream = new ForcedSeekStream<GZipStream>(_decompressedSize, _dataStream, CompressionMode.Decompress);

            _opened = true;
        }

        public void Open(Stream stream)
        {
            byte[] mtimeB = new byte[4];
            byte[] isizeB = new byte[4];

            _dataStream = stream;
            _basePath   = null;

            _dataStream.Seek(4, SeekOrigin.Begin);
            _dataStream.Read(mtimeB, 0, 4);
            _dataStream.Seek(-4, SeekOrigin.End);
            _dataStream.Read(isizeB, 0, 4);
            _dataStream.Seek(0, SeekOrigin.Begin);

            uint mtime = BitConverter.ToUInt32(mtimeB, 0);
            uint isize = BitConverter.ToUInt32(isizeB, 0);

            _decompressedSize = isize;
            _creationTime     = DateHandlers.UnixUnsignedToDateTime(mtime);
            _lastWriteTime    = _creationTime;

            _zStream = new ForcedSeekStream<GZipStream>(_decompressedSize, _dataStream, CompressionMode.Decompress);

            _opened = true;
        }

        public void Open(string path)
        {
            byte[] mtimeB = new byte[4];
            byte[] isizeB = new byte[4];

            _dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            _basePath   = Path.GetFullPath(path);

            _dataStream.Seek(4, SeekOrigin.Begin);
            _dataStream.Read(mtimeB, 0, 4);
            _dataStream.Seek(-4, SeekOrigin.End);
            _dataStream.Read(isizeB, 0, 4);
            _dataStream.Seek(0, SeekOrigin.Begin);

            uint mtime = BitConverter.ToUInt32(mtimeB, 0);
            uint isize = BitConverter.ToUInt32(isizeB, 0);

            _decompressedSize = isize;
            var fi = new FileInfo(path);
            _creationTime = fi.CreationTimeUtc;
            _lastWriteTime = DateHandlers.UnixUnsignedToDateTime(mtime);
            _zStream = new ForcedSeekStream<GZipStream>(_decompressedSize, _dataStream, CompressionMode.Decompress);
            _opened = true;
        }

        public DateTime GetCreationTime() => _creationTime;

        public long GetDataForkLength() => _decompressedSize;

        public DateTime GetLastWriteTime() => _lastWriteTime;

        public long GetLength() => _decompressedSize;

        public long GetResourceForkLength() => 0;

        public string GetFilename()
        {
            if(_basePath?.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase) == true)
                return _basePath.Substring(0, _basePath.Length - 3);

            return _basePath?.EndsWith(".gzip", StringComparison.InvariantCultureIgnoreCase) == true
                       ? _basePath.Substring(0, _basePath.Length - 5) : _basePath;
        }

        public string GetParentFolder() => Path.GetDirectoryName(_basePath);

        public bool IsOpened() => _opened;
    }
}