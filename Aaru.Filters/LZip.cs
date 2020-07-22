// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LZip.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Allow to open files that are compressed using lzip.
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
using Aaru.CommonTypes.Interfaces;
using SharpCompress.Compressors;
using SharpCompress.Compressors.LZMA;

namespace Aaru.Filters
{
    /// <summary>Decompress lzip files while reading</summary>
    public sealed class LZip : IFilter
    {
        string   _basePath;
        DateTime _creationTime;
        Stream   _dataStream;
        long     _decompressedSize;
        Stream   _innerStream;
        DateTime _lastWriteTime;
        bool     _opened;

        public string Name   => "LZip";
        public Guid   Id     => new Guid("09D715E9-20C0-48B1-A8D9-D8897CEC57C9");
        public string Author => "Natalia Portillo";

        public void Close()
        {
            _dataStream?.Close();
            _dataStream = null;
            _basePath   = null;
            _opened     = false;
        }

        public string GetBasePath() => _basePath;

        public Stream GetDataForkStream() => _innerStream;

        public string GetPath() => _basePath;

        public Stream GetResourceForkStream() => null;

        public bool HasResourceFork() => false;

        public bool Identify(byte[] buffer) => buffer[0] == 0x4C && buffer[1] == 0x5A && buffer[2] == 0x49 &&
                                               buffer[3] == 0x50 && buffer[4] == 0x01;

        public bool Identify(Stream stream)
        {
            byte[] buffer = new byte[5];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 5);
            stream.Seek(0, SeekOrigin.Begin);

            return buffer[0] == 0x4C && buffer[1] == 0x5A && buffer[2] == 0x49 && buffer[3] == 0x50 &&
                   buffer[4] == 0x01;
        }

        public bool Identify(string path)
        {
            if(!File.Exists(path))
                return false;

            var    stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[5];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 5);
            stream.Seek(0, SeekOrigin.Begin);

            return buffer[0] == 0x4C && buffer[1] == 0x5A && buffer[2] == 0x49 && buffer[3] == 0x50 &&
                   buffer[4] == 0x01;
        }

        public void Open(byte[] buffer)
        {
            _dataStream       = new MemoryStream(buffer);
            _basePath         = null;
            _creationTime     = DateTime.UtcNow;
            _lastWriteTime    = _creationTime;
            _decompressedSize = BitConverter.ToInt64(buffer, buffer.Length - 16);

            _innerStream = new ForcedSeekStream<LZipStream>(_decompressedSize, _dataStream, CompressionMode.Decompress);

            _opened = true;
        }

        public void Open(Stream stream)
        {
            _dataStream    = stream;
            _basePath      = null;
            _creationTime  = DateTime.UtcNow;
            _lastWriteTime = _creationTime;
            byte[] tmp = new byte[8];
            _dataStream.Seek(-16, SeekOrigin.End);
            _dataStream.Read(tmp, 0, 8);
            _decompressedSize = BitConverter.ToInt64(tmp, 0);
            _dataStream.Seek(0, SeekOrigin.Begin);
            _innerStream = new ForcedSeekStream<LZipStream>(_decompressedSize, _dataStream, CompressionMode.Decompress);
            _opened      = true;
        }

        public void Open(string path)
        {
            _dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            _basePath   = Path.GetFullPath(path);

            var fi = new FileInfo(path);
            _creationTime  = fi.CreationTimeUtc;
            _lastWriteTime = fi.LastWriteTimeUtc;
            byte[] tmp = new byte[8];
            _dataStream.Seek(-16, SeekOrigin.End);
            _dataStream.Read(tmp, 0, 8);
            _decompressedSize = BitConverter.ToInt64(tmp, 0);
            _dataStream.Seek(0, SeekOrigin.Begin);
            _innerStream = new ForcedSeekStream<LZipStream>(_decompressedSize, _dataStream, CompressionMode.Decompress);
            _opened      = true;
        }

        public DateTime GetCreationTime() => _creationTime;

        public long GetDataForkLength() => _decompressedSize;

        public DateTime GetLastWriteTime() => _lastWriteTime;

        public long GetLength() => _decompressedSize;

        public long GetResourceForkLength() => 0;

        public string GetFilename()
        {
            if(_basePath?.EndsWith(".lz", StringComparison.InvariantCultureIgnoreCase) == true)
                return _basePath.Substring(0, _basePath.Length - 3);

            return _basePath?.EndsWith(".lzip", StringComparison.InvariantCultureIgnoreCase) == true
                       ? _basePath.Substring(0, _basePath.Length - 5) : _basePath;
        }

        public string GetParentFolder() => Path.GetDirectoryName(_basePath);

        public bool IsOpened() => _opened;
    }
}