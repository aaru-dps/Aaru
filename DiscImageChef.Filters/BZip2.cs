// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;

namespace DiscImageChef.Filters
{
    /// <summary>
    ///     Decompress bz2 files while reading
    /// </summary>
    public class BZip2 : IFilter
    {
        string basePath;
        DateTime creationTime;
        Stream dataStream;
        long decompressedSize;
        Stream innerStream;
        DateTime lastWriteTime;
        bool opened;

        public virtual string Name => "BZip2";
        public virtual Guid Id => new Guid("FCCFB0C3-32EF-40D8-9714-2333F6AC72A9");

        public virtual void Close()
        {
            dataStream?.Close();
            dataStream = null;
            basePath = null;
            opened = false;
        }

        public virtual string GetBasePath()
        {
            return basePath;
        }

        public virtual Stream GetDataForkStream()
        {
            return innerStream;
        }

        public virtual string GetPath()
        {
            return basePath;
        }

        public virtual Stream GetResourceForkStream()
        {
            return null;
        }

        public virtual bool HasResourceFork()
        {
            return false;
        }

        public virtual bool Identify(byte[] buffer)
        {
            if(buffer[0] != 0x42 || buffer[1] != 0x5A || buffer[2] != 0x68 || buffer[3] < 0x31 ||
               buffer[3] > 0x39) return false;

            if(buffer.Length <= 512) return true;

            return buffer[buffer.Length - 512] != 0x6B || buffer[buffer.Length - 511] != 0x6F ||
                   buffer[buffer.Length - 510] != 0x6C || buffer[buffer.Length - 509] != 0x79;
        }

        public virtual bool Identify(Stream stream)
        {
            byte[] buffer = new byte[4];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 4);
            stream.Seek(0, SeekOrigin.Begin);

            if(buffer[0] != 0x42 || buffer[1] != 0x5A || buffer[2] != 0x68 || buffer[3] < 0x31 ||
               buffer[3] > 0x39) return false;

            if(stream.Length <= 512) return true;

            stream.Seek(-512, SeekOrigin.End);
            stream.Read(buffer, 0, 4);
            stream.Seek(0, SeekOrigin.Begin);
            // Check it is not an UDIF
            return buffer[0] != 0x6B || buffer[1] != 0x6F || buffer[2] != 0x6C || buffer[3] != 0x79;
        }

        public virtual bool Identify(string path)
        {
            if(!File.Exists(path)) return false;

            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 4);
            stream.Seek(0, SeekOrigin.Begin);

            if(buffer[0] != 0x42 || buffer[1] != 0x5A || buffer[2] != 0x68 || buffer[3] < 0x31 ||
               buffer[3] > 0x39) return false;

            if(stream.Length <= 512) return true;

            stream.Seek(-512, SeekOrigin.End);
            stream.Read(buffer, 0, 4);
            stream.Seek(0, SeekOrigin.Begin);
            // Check it is not an UDIF
            if(buffer[0] == 0x6B && buffer[1] == 0x6F && buffer[2] == 0x6C && buffer[3] == 0x79) return false;

            return true;
        }

        public virtual void Open(byte[] buffer)
        {
            dataStream = new MemoryStream(buffer);
            basePath = null;
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            innerStream = new ForcedSeekStream<BZip2Stream>(dataStream, CompressionMode.Decompress, false, false);
            decompressedSize = innerStream.Length;
            opened = true;
        }

        public virtual void Open(Stream stream)
        {
            dataStream = stream;
            basePath = null;
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            innerStream = new ForcedSeekStream<BZip2Stream>(dataStream, CompressionMode.Decompress, false, false);
            decompressedSize = innerStream.Length;
            opened = true;
        }

        public virtual void Open(string path)
        {
            dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            basePath = Path.GetFullPath(path);

            DateTime start = DateTime.UtcNow;
            DateTime end = DateTime.UtcNow;

            FileInfo fi = new FileInfo(path);
            creationTime = fi.CreationTimeUtc;
            lastWriteTime = fi.LastWriteTimeUtc;
            innerStream = new ForcedSeekStream<BZip2Stream>(dataStream, CompressionMode.Decompress, false, false);
            decompressedSize = innerStream.Length;
            opened = true;
        }

        public virtual DateTime GetCreationTime()
        {
            return creationTime;
        }

        public virtual long GetDataForkLength()
        {
            return decompressedSize;
        }

        public virtual DateTime GetLastWriteTime()
        {
            return lastWriteTime;
        }

        public virtual long GetLength()
        {
            return decompressedSize;
        }

        public virtual long GetResourceForkLength()
        {
            return 0;
        }

        public virtual string GetFilename()
        {
            if(basePath?.EndsWith(".bz2", StringComparison.InvariantCultureIgnoreCase) == true)
                return basePath.Substring(0, basePath.Length - 4);
            if(basePath?.EndsWith(".bzip2", StringComparison.InvariantCultureIgnoreCase) == true)
                return basePath.Substring(0, basePath.Length - 6);

            return basePath;
        }

        public virtual string GetParentFolder()
        {
            return Path.GetDirectoryName(basePath);
        }

        public virtual bool IsOpened()
        {
            return opened;
        }
    }
}