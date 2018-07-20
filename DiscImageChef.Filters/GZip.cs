// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.IO.Compression;
using DiscImageChef.CommonTypes.Interfaces;

namespace DiscImageChef.Filters
{
    /// <summary>
    ///     Decompress gzip files while reading
    /// </summary>
    public class GZip : IFilter
    {
        string   basePath;
        DateTime creationTime;
        Stream   dataStream;
        uint     decompressedSize;
        DateTime lastWriteTime;
        bool     opened;
        Stream   zStream;

        public string Name => "GZip";
        public Guid   Id   => new Guid("F4996661-4A29-42C9-A2C7-3904EF40F3B0");

        public void Close()
        {
            dataStream?.Close();
            dataStream = null;
            basePath   = null;
            opened     = false;
        }

        public string GetBasePath()
        {
            return basePath;
        }

        public Stream GetDataForkStream()
        {
            return zStream;
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
            return buffer[0] == 0x1F && buffer[1] == 0x8B && buffer[2] == 0x08;
        }

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
            if(!File.Exists(path)) return false;

            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[]     buffer = new byte[3];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 3);
            stream.Seek(0, SeekOrigin.Begin);

            return buffer[0] == 0x1F && buffer[1] == 0x8B && buffer[2] == 0x08;
        }

        public void Open(byte[] buffer)
        {
            byte[] mtime_b = new byte[4];
            byte[] isize_b = new byte[4];

            dataStream = new MemoryStream(buffer);
            basePath   = null;

            dataStream.Seek(4, SeekOrigin.Begin);
            dataStream.Read(mtime_b, 0, 4);
            dataStream.Seek(-4, SeekOrigin.End);
            dataStream.Read(isize_b, 0, 4);
            dataStream.Seek(0, SeekOrigin.Begin);

            uint mtime = BitConverter.ToUInt32(mtime_b, 0);
            uint isize = BitConverter.ToUInt32(isize_b, 0);

            decompressedSize = isize;
            creationTime     = DateHandlers.UnixUnsignedToDateTime(mtime);
            lastWriteTime    = creationTime;
            zStream =
                new ForcedSeekStream<GZipStream>(decompressedSize, dataStream, CompressionMode.Decompress);
            opened = true;
        }

        public void Open(Stream stream)
        {
            byte[] mtime_b = new byte[4];
            byte[] isize_b = new byte[4];

            dataStream = stream;
            basePath   = null;

            dataStream.Seek(4, SeekOrigin.Begin);
            dataStream.Read(mtime_b, 0, 4);
            dataStream.Seek(-4, SeekOrigin.End);
            dataStream.Read(isize_b, 0, 4);
            dataStream.Seek(0, SeekOrigin.Begin);

            uint mtime = BitConverter.ToUInt32(mtime_b, 0);
            uint isize = BitConverter.ToUInt32(isize_b, 0);

            decompressedSize = isize;
            creationTime     = DateHandlers.UnixUnsignedToDateTime(mtime);
            lastWriteTime    = creationTime;
            zStream =
                new ForcedSeekStream<GZipStream>(decompressedSize, dataStream, CompressionMode.Decompress);
            opened = true;
        }

        public void Open(string path)
        {
            byte[] mtime_b = new byte[4];
            byte[] isize_b = new byte[4];

            dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            basePath   = Path.GetFullPath(path);

            dataStream.Seek(4, SeekOrigin.Begin);
            dataStream.Read(mtime_b, 0, 4);
            dataStream.Seek(-4, SeekOrigin.End);
            dataStream.Read(isize_b, 0, 4);
            dataStream.Seek(0, SeekOrigin.Begin);

            uint mtime = BitConverter.ToUInt32(mtime_b, 0);
            uint isize = BitConverter.ToUInt32(isize_b, 0);

            decompressedSize = isize;
            FileInfo fi = new FileInfo(path);
            creationTime  = fi.CreationTimeUtc;
            lastWriteTime = DateHandlers.UnixUnsignedToDateTime(mtime);
            zStream       = new ForcedSeekStream<GZipStream>(decompressedSize, dataStream, CompressionMode.Decompress);
            opened        = true;
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
            if(basePath?.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase) == true)
                return basePath.Substring(0, basePath.Length - 3);

            return basePath?.EndsWith(".gzip", StringComparison.InvariantCultureIgnoreCase) == true
                       ? basePath.Substring(0, basePath.Length - 5)
                       : basePath;
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