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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using SharpCompress.Compressors;
using SharpCompress.Compressors.LZMA;

namespace DiscImageChef.Filters
{
    public class LZip : Filter
    {
        Stream dataStream;
        string basePath;
        DateTime lastWriteTime;
        DateTime creationTime;
        bool opened;
        long decompressedSize;
        Stream innerStream;

        public LZip()
        {
            Name = "LZip";
            UUID = new Guid("09D715E9-20C0-48B1-A8D9-D8897CEC57C9");
        }

        public override void Close()
        {
            if(dataStream != null)
                dataStream.Close();
            dataStream = null;
            basePath = null;
            opened = false;
        }

        public override string GetBasePath()
        {
            return basePath;
        }

        public override Stream GetDataForkStream()
        {
            return innerStream;
        }

        public override string GetPath()
        {
            return basePath;
        }

        public override Stream GetResourceForkStream()
        {
            return null;
        }

        public override bool HasResourceFork()
        {
            return false;
        }

        public override bool Identify(byte[] buffer)
        {
            return buffer[0] == 0x4C && buffer[1] == 0x5A && buffer[2] == 0x49 && buffer[3] == 0x50 && buffer[4] == 0x01;
        }

        public override bool Identify(Stream stream)
        {
            byte[] buffer = new byte[5];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 5);
            stream.Seek(0, SeekOrigin.Begin);

            return buffer[0] == 0x4C && buffer[1] == 0x5A && buffer[2] == 0x49 && buffer[3] == 0x50 && buffer[4] == 0x01;
        }

        public override bool Identify(string path)
        {
            if(File.Exists(path))
            {
                FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[5];

                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(buffer, 0, 5);
                stream.Seek(0, SeekOrigin.Begin);

                return buffer[0] == 0x4C && buffer[1] == 0x5A && buffer[2] == 0x49 && buffer[3] == 0x50 && buffer[4] == 0x01;
            }

            return false;
        }

        public override void Open(byte[] buffer)
        {
            dataStream = new MemoryStream(buffer);
            basePath = null;
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            decompressedSize = BitConverter.ToInt64(buffer, buffer.Length - 16);
            innerStream = new ForcedSeekStream<LZipStream>(decompressedSize, dataStream, CompressionMode.Decompress, false);
            opened = true;
        }

        public override void Open(Stream stream)
        {
            dataStream = stream;
            basePath = null;
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            byte[] tmp = new byte[8];
            dataStream.Seek(-16 ,SeekOrigin.End);
            dataStream.Read(tmp, 0, 8);
            decompressedSize = BitConverter.ToInt64(tmp, 0);
            dataStream.Seek(0, SeekOrigin.Begin);
            innerStream = new ForcedSeekStream<LZipStream>(decompressedSize, dataStream, CompressionMode.Decompress, false);
            opened = true;
        }

        public override void Open(string path)
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
            innerStream = new ForcedSeekStream<LZipStream>(decompressedSize, dataStream, CompressionMode.Decompress, false);
            opened = true;
        }

        public override DateTime GetCreationTime()
        {
            return creationTime;
        }

        public override long GetDataForkLength()
        {
            return decompressedSize;
        }

        public override DateTime GetLastWriteTime()
        {
            return lastWriteTime;
        }

        public override long GetLength()
        {
            return decompressedSize;
        }

        public override long GetResourceForkLength()
        {
            return 0;
        }

        public override string GetFilename()
        {
            if(basePath == null)
                return null;
            if(basePath.EndsWith(".lz", StringComparison.InvariantCultureIgnoreCase))
                return basePath.Substring(0, basePath.Length - 3);
            if(basePath.EndsWith(".lzip", StringComparison.InvariantCultureIgnoreCase))
                return basePath.Substring(0, basePath.Length - 5);
            return basePath;
        }

        public override string GetParentFolder()
        {
            return Path.GetDirectoryName(basePath);
        }

        public override bool IsOpened()
        {
            return opened;
        }
    }
}