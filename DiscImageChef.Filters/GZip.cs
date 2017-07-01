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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.IO.Compression;

namespace DiscImageChef.Filters
{
    public class GZip : Filter
    {
        Stream dataStream;
        string basePath;
        DateTime lastWriteTime;
        DateTime creationTime;
        bool opened;
        uint decompressedSize;
        Stream zStream;

        public GZip()
        {
            Name = "GZip";
            UUID = new Guid("F4996661-4A29-42C9-A2C7-3904EF40F3B0");
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
            return zStream;
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
            return buffer[0] == 0x1F && buffer[1] == 0x8B && buffer[2] == 0x08;
        }

        public override bool Identify(Stream stream)
        {
            byte[] buffer = new byte[3];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 3);
            stream.Seek(0, SeekOrigin.Begin);

            return buffer[0] == 0x1F && buffer[1] == 0x8B && buffer[2] == 0x08;
        }

        public override bool Identify(string path)
        {
            if(File.Exists(path))
            {
                FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[3];

                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(buffer, 0, 3);
                stream.Seek(0, SeekOrigin.Begin);

                return buffer[0] == 0x1F && buffer[1] == 0x8B && buffer[2] == 0x08;
            }

            return false;
        }

        public override void Open(byte[] buffer)
        {
            byte[] mtime_b = new byte[4];
            byte[] isize_b = new byte[4];
            uint mtime;
            uint isize;

            dataStream = new MemoryStream(buffer);
            basePath = null;

            dataStream.Seek(4, SeekOrigin.Begin);
            dataStream.Read(mtime_b, 0, 4);
            dataStream.Seek(-4, SeekOrigin.End);
            dataStream.Read(isize_b, 0, 4);
            dataStream.Seek(0, SeekOrigin.Begin);

            mtime = BitConverter.ToUInt32(mtime_b, 0);
            isize = BitConverter.ToUInt32(isize_b, 0);

            decompressedSize = isize;
            creationTime = DateHandlers.UNIXUnsignedToDateTime(mtime);
            lastWriteTime = creationTime;
            zStream = new ForcedSeekStream<GZipStream>(decompressedSize, dataStream, CompressionMode.Decompress);
            opened = true;
        }

        public override void Open(Stream stream)
        {
            byte[] mtime_b = new byte[4];
            byte[] isize_b = new byte[4];
            uint mtime;
            uint isize;

            dataStream = stream;
            basePath = null;

            dataStream.Seek(4, SeekOrigin.Begin);
            dataStream.Read(mtime_b, 0, 4);
            dataStream.Seek(-4, SeekOrigin.End);
            dataStream.Read(isize_b, 0, 4);
            dataStream.Seek(0, SeekOrigin.Begin);

            mtime = BitConverter.ToUInt32(mtime_b, 0);
            isize = BitConverter.ToUInt32(isize_b, 0);

            decompressedSize = isize;
            creationTime = DateHandlers.UNIXUnsignedToDateTime(mtime);
            lastWriteTime = creationTime;
            zStream = new ForcedSeekStream<GZipStream>(decompressedSize, dataStream, CompressionMode.Decompress);
            opened = true;
        }

        public override void Open(string path)
        {
            byte[] mtime_b = new byte[4];
            byte[] isize_b = new byte[4];
            uint mtime;
            uint isize;

            dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            basePath = Path.GetFullPath(path);

            dataStream.Seek(4, SeekOrigin.Begin);
            dataStream.Read(mtime_b, 0, 4);
            dataStream.Seek(-4, SeekOrigin.End);
            dataStream.Read(isize_b, 0, 4);
            dataStream.Seek(0, SeekOrigin.Begin);

            mtime = BitConverter.ToUInt32(mtime_b, 0);
            isize = BitConverter.ToUInt32(isize_b, 0);

            decompressedSize = isize;
            FileInfo fi = new FileInfo(path);
            creationTime = fi.CreationTimeUtc;
            lastWriteTime = DateHandlers.UNIXUnsignedToDateTime(mtime);
            zStream = new ForcedSeekStream<GZipStream>(decompressedSize, dataStream, CompressionMode.Decompress);
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
            if(basePath.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase))
                return basePath.Substring(0, basePath.Length - 3);
            if(basePath.EndsWith(".gzip", StringComparison.InvariantCultureIgnoreCase))
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