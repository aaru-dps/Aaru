// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : XZ.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Allow to open files that are compressed using xz.
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
using System.IO;
using Aaru.CommonTypes.Interfaces;
using SharpCompress.Compressors.Xz;

namespace Aaru.Filters
{
    /// <inheritdoc />
    /// <summary>Decompress xz files while reading</summary>
    public sealed class XZ : IFilter
    {
        string   _basePath;
        DateTime _creationTime;
        Stream   _dataStream;
        long     _decompressedSize;
        Stream   _innerStream;
        DateTime _lastWriteTime;
        bool     _opened;

        /// <inheritdoc />
        public string Name => "XZ";
        /// <inheritdoc />
        public Guid Id => new Guid("666A8617-0444-4C05-9F4F-DF0FD758D0D2");
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
        public bool Identify(byte[] buffer) => buffer[0]  == 0xFD && buffer[1]  == 0x37 && buffer[2] == 0x7A &&
                                               buffer[3]  == 0x58 && buffer[4]  == 0x5A && buffer[5] == 0x00 &&
                                               buffer[^2] == 0x59 && buffer[^1] == 0x5A;

        /// <inheritdoc />
        public bool Identify(Stream stream)
        {
            byte[] buffer = new byte[6];
            byte[] footer = new byte[2];

            if(stream.Length < 8)
                return false;

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 6);
            stream.Seek(-2, SeekOrigin.End);
            stream.Read(footer, 0, 2);
            stream.Seek(0, SeekOrigin.Begin);

            return buffer[0] == 0xFD && buffer[1] == 0x37 && buffer[2] == 0x7A && buffer[3] == 0x58 &&
                   buffer[4] == 0x5A && buffer[5] == 0x00 && footer[0] == 0x59 && footer[1] == 0x5A;
        }

        /// <inheritdoc />
        public bool Identify(string path)
        {
            if(!File.Exists(path))
                return false;

            var    stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[6];
            byte[] footer = new byte[2];

            if(stream.Length < 8)
                return false;

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 6);
            stream.Seek(-2, SeekOrigin.End);
            stream.Read(footer, 0, 2);
            stream.Seek(0, SeekOrigin.Begin);

            return buffer[0] == 0xFD && buffer[1] == 0x37 && buffer[2] == 0x7A && buffer[3] == 0x58 &&
                   buffer[4] == 0x5A && buffer[5] == 0x00 && footer[0] == 0x59 && footer[1] == 0x5A;
        }

        /// <inheritdoc />
        public void Open(byte[] buffer)
        {
            _dataStream    = new MemoryStream(buffer);
            _basePath      = null;
            _creationTime  = DateTime.UtcNow;
            _lastWriteTime = _creationTime;
            GuessSize();
            _innerStream = new ForcedSeekStream<XZStream>(_decompressedSize, _dataStream);
            _opened      = true;
        }

        /// <inheritdoc />
        public void Open(Stream stream)
        {
            _dataStream    = stream;
            _basePath      = null;
            _creationTime  = DateTime.UtcNow;
            _lastWriteTime = _creationTime;
            GuessSize();
            _innerStream = new ForcedSeekStream<XZStream>(_decompressedSize, _dataStream);
            _opened      = true;
        }

        /// <inheritdoc />
        public void Open(string path)
        {
            _dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            _basePath   = Path.GetFullPath(path);

            var fi = new FileInfo(path);
            _creationTime  = fi.CreationTimeUtc;
            _lastWriteTime = fi.LastWriteTimeUtc;
            GuessSize();
            _innerStream = new ForcedSeekStream<XZStream>(_decompressedSize, _dataStream);
            _opened      = true;
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
            if(_basePath?.EndsWith(".xz", StringComparison.InvariantCultureIgnoreCase) == true)
                return _basePath.Substring(0, _basePath.Length - 3);

            return _basePath?.EndsWith(".xzip", StringComparison.InvariantCultureIgnoreCase) == true
                       ? _basePath.Substring(0, _basePath.Length - 5) : _basePath;
        }

        /// <inheritdoc />
        public string GetParentFolder() => Path.GetDirectoryName(_basePath);

        /// <inheritdoc />
        public bool IsOpened() => _opened;

        void GuessSize()
        {
            _decompressedSize = 0;

            // Seek to footer backwards size field
            _dataStream.Seek(-8, SeekOrigin.End);
            byte[] tmp = new byte[4];
            _dataStream.Read(tmp, 0, 4);
            uint backwardSize = (BitConverter.ToUInt32(tmp, 0) + 1) * 4;

            // Seek to first indexed record
            _dataStream.Seek(-12 - (backwardSize - 2), SeekOrigin.End);

            // Skip compressed size
            tmp = new byte[backwardSize - 2];
            _dataStream.Read(tmp, 0, tmp.Length);
            ulong number = 0;
            int   ignore = Decode(tmp, tmp.Length, ref number);

            // Get compressed size
            _dataStream.Seek(-12 - (backwardSize - 2 - ignore), SeekOrigin.End);
            tmp = new byte[backwardSize - 2 - ignore];
            _dataStream.Read(tmp, 0, tmp.Length);
            Decode(tmp, tmp.Length, ref number);
            _decompressedSize = (long)number;

            _dataStream.Seek(0, SeekOrigin.Begin);
        }

        int Decode(byte[] buf, int sizeMax, ref ulong num)
        {
            if(sizeMax == 0)
                return 0;

            if(sizeMax > 9)
                sizeMax = 9;

            num = (ulong)(buf[0] & 0x7F);
            int i = 0;

            while((buf[i++] & 0x80) == 0x80)
            {
                if(i      >= sizeMax ||
                   buf[i] == 0x00)
                    return 0;

                num |= (ulong)(buf[i] & 0x7F) << (i * 7);
            }

            return i;
        }
    }
}