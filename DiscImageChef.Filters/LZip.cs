// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using SharpCompress.Compressors;
using SharpCompress.Compressors.LZMA;

namespace DiscImageChef.Filters
{
    /// <summary>
    ///     Decompress lzip files while reading
    /// </summary>
    public class LZip : IFilter
    {
        string basePath;
        DateTime creationTime;
        Stream dataStream;
        long decompressedSize;
        Stream innerStream;
        DateTime lastWriteTime;
        bool opened;

        public string Name => "LZip";
        public Guid Id => new Guid("09D715E9-20C0-48B1-A8D9-D8897CEC57C9");

        public void Close()
        {
            dataStream?.Close();
            dataStream = null;
            basePath = null;
            opened = false;
        }

        public string GetBasePath()
        {
            return basePath;
        }

        public Stream GetDataForkStream()
        {
            return innerStream;
        }

        public string GetPath()
        {
            return basePath;
        }

        public Stream GetResourceForkStream()
        {
            return null;
        }

        public bool HasResourceFork()
        {
            return false;
        }

        public bool Identify(byte[] buffer)
        {
            return buffer[0] == 0x4C && buffer[1] == 0x5A && buffer[2] == 0x49 && buffer[3] == 0x50 &&
                   buffer[4] == 0x01;
        }

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
            if(!File.Exists(path)) return false;

            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[5];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 5);
            stream.Seek(0, SeekOrigin.Begin);

            return buffer[0] == 0x4C && buffer[1] == 0x5A && buffer[2] == 0x49 && buffer[3] == 0x50 &&
                   buffer[4] == 0x01;
        }

        public void Open(byte[] buffer)
        {
            dataStream = new MemoryStream(buffer);
            basePath = null;
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            decompressedSize = BitConverter.ToInt64(buffer, buffer.Length - 16);
            innerStream =
                new ForcedSeekStream<LZipStream>(decompressedSize, dataStream, CompressionMode.Decompress, false);
            opened = true;
        }

        public void Open(Stream stream)
        {
            dataStream = stream;
            basePath = null;
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            byte[] tmp = new byte[8];
            dataStream.Seek(-16, SeekOrigin.End);
            dataStream.Read(tmp, 0, 8);
            decompressedSize = BitConverter.ToInt64(tmp, 0);
            dataStream.Seek(0, SeekOrigin.Begin);
            innerStream =
                new ForcedSeekStream<LZipStream>(decompressedSize, dataStream, CompressionMode.Decompress, false);
            opened = true;
        }

        public void Open(string path)
        {
            dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            basePath = Path.GetFullPath(path);

            DateTime start = DateTime.UtcNow;
            DateTime end = DateTime.UtcNow;

            FileInfo fi = new FileInfo(path);
            creationTime = fi.CreationTimeUtc;
            lastWriteTime = fi.LastWriteTimeUtc;
            byte[] tmp = new byte[8];
            dataStream.Seek(-16, SeekOrigin.End);
            dataStream.Read(tmp, 0, 8);
            decompressedSize = BitConverter.ToInt64(tmp, 0);
            dataStream.Seek(0, SeekOrigin.Begin);
            innerStream =
                new ForcedSeekStream<LZipStream>(decompressedSize, dataStream, CompressionMode.Decompress, false);
            opened = true;
        }

        public DateTime GetCreationTime()
        {
            return creationTime;
        }

        public long GetDataForkLength()
        {
            return decompressedSize;
        }

        public DateTime GetLastWriteTime()
        {
            return lastWriteTime;
        }

        public long GetLength()
        {
            return decompressedSize;
        }

        public long GetResourceForkLength()
        {
            return 0;
        }

        public string GetFilename()
        {
            if(basePath?.EndsWith(".lz", StringComparison.InvariantCultureIgnoreCase) == true)
                return basePath.Substring(0, basePath.Length - 3);
            if(basePath?.EndsWith(".lzip", StringComparison.InvariantCultureIgnoreCase) == true)
                return basePath.Substring(0, basePath.Length - 5);

            return basePath;
        }

        public string GetParentFolder()
        {
            return Path.GetDirectoryName(basePath);
        }

        public bool IsOpened()
        {
            return opened;
        }
    }
}