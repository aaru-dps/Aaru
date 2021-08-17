// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BZip2.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Allow to open files that are compressed using bzip2.
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
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;

namespace Aaru.Filters
{
    /// <inheritdoc />
    /// <summary>Decompress bz2 files while reading</summary>
    public class BZip2 : IFilter
    {
        string   _basePath;
        DateTime _creationTime;
        Stream   _dataStream;
        long     _decompressedSize;
        Stream   _innerStream;
        DateTime _lastWriteTime;
        bool     _opened;

        /// <inheritdoc />
        public string Name => "BZip2";
        /// <inheritdoc />
        public Guid Id => new Guid("FCCFB0C3-32EF-40D8-9714-2333F6AC72A9");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public void Close()
        {
            _dataStream?.Close();
            _dataStream = null;
            _basePath   = null;
            _opened     = false;
        }

        /// <inheritdoc />
        public string GetBasePath() => _basePath;

        /// <inheritdoc />
        public Stream GetDataForkStream() => _innerStream;

        /// <inheritdoc />
        public string GetPath() => _basePath;

        /// <inheritdoc />
        public Stream GetResourceForkStream() => null;

        /// <inheritdoc />
        public bool HasResourceFork() => false;

        /// <inheritdoc />
        public bool Identify(byte[] buffer)
        {
            if(buffer[0] != 0x42 ||
               buffer[1] != 0x5A ||
               buffer[2] != 0x68 ||
               buffer[3] < 0x31  ||
               buffer[3] > 0x39)
                return false;

            if(buffer.Length <= 512)
                return true;

            return buffer[^512] != 0x6B || buffer[^511] != 0x6F || buffer[^510] != 0x6C || buffer[^509] != 0x79;
        }

        /// <inheritdoc />
        public bool Identify(Stream stream)
        {
            byte[] buffer = new byte[4];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 4);
            stream.Seek(0, SeekOrigin.Begin);

            if(buffer[0] != 0x42 ||
               buffer[1] != 0x5A ||
               buffer[2] != 0x68 ||
               buffer[3] < 0x31  ||
               buffer[3] > 0x39)
                return false;

            if(stream.Length <= 512)
                return true;

            stream.Seek(-512, SeekOrigin.End);
            stream.Read(buffer, 0, 4);
            stream.Seek(0, SeekOrigin.Begin);

            // Check it is not an UDIF
            return buffer[0] != 0x6B || buffer[1] != 0x6F || buffer[2] != 0x6C || buffer[3] != 0x79;
        }

        /// <inheritdoc />
        public bool Identify(string path)
        {
            if(!File.Exists(path))
                return false;

            var    stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 4);
            stream.Seek(0, SeekOrigin.Begin);

            if(buffer[0] != 0x42 ||
               buffer[1] != 0x5A ||
               buffer[2] != 0x68 ||
               buffer[3] < 0x31  ||
               buffer[3] > 0x39)
                return false;

            if(stream.Length <= 512)
                return true;

            stream.Seek(-512, SeekOrigin.End);
            stream.Read(buffer, 0, 4);
            stream.Seek(0, SeekOrigin.Begin);

            // Check it is not an UDIF
            return buffer[0] != 0x6B || buffer[1] != 0x6F || buffer[2] != 0x6C || buffer[3] != 0x79;
        }

        /// <inheritdoc />
        public void Open(byte[] buffer)
        {
            _dataStream       = new MemoryStream(buffer);
            _basePath         = null;
            _creationTime     = DateTime.UtcNow;
            _lastWriteTime    = _creationTime;
            _innerStream      = new ForcedSeekStream<BZip2Stream>(_dataStream, CompressionMode.Decompress, false);
            _decompressedSize = _innerStream.Length;
            _opened           = true;
        }

        /// <inheritdoc />
        public void Open(Stream stream)
        {
            _dataStream       = stream;
            _basePath         = null;
            _creationTime     = DateTime.UtcNow;
            _lastWriteTime    = _creationTime;
            _innerStream      = new ForcedSeekStream<BZip2Stream>(_dataStream, CompressionMode.Decompress, false);
            _decompressedSize = _innerStream.Length;
            _opened           = true;
        }

        /// <inheritdoc />
        public void Open(string path)
        {
            _dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            _basePath   = Path.GetFullPath(path);

            var fi = new FileInfo(path);
            _creationTime     = fi.CreationTimeUtc;
            _lastWriteTime    = fi.LastWriteTimeUtc;
            _innerStream      = new ForcedSeekStream<BZip2Stream>(_dataStream, CompressionMode.Decompress, false);
            _decompressedSize = _innerStream.Length;
            _opened           = true;
        }

        /// <inheritdoc />
        public DateTime GetCreationTime() => _creationTime;

        /// <inheritdoc />
        public long GetDataForkLength() => _decompressedSize;

        /// <inheritdoc />
        public DateTime GetLastWriteTime() => _lastWriteTime;

        /// <inheritdoc />
        public long GetLength() => _decompressedSize;

        /// <inheritdoc />
        public long GetResourceForkLength() => 0;

        /// <inheritdoc />
        public string GetFilename()
        {
            if(_basePath?.EndsWith(".bz2", StringComparison.InvariantCultureIgnoreCase) == true)
                return _basePath.Substring(0, _basePath.Length - 4);

            return _basePath?.EndsWith(".bzip2", StringComparison.InvariantCultureIgnoreCase) == true
                       ? _basePath.Substring(0, _basePath.Length - 6) : _basePath;
        }

        /// <inheritdoc />
        public string GetParentFolder() => Path.GetDirectoryName(_basePath);

        /// <inheritdoc />
        public bool IsOpened() => _opened;
    }
}