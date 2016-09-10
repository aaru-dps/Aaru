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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using SharpCompress.Compressor;
using SharpCompress.Compressor.BZip2;

namespace DiscImageChef.Filters
{
    public class BZip2 : Filter
    {
        Stream dataStream;
        string basePath;
        DateTime lastWriteTime;
        DateTime creationTime;
        bool opened;
        long decompressedSize;

        public BZip2()
        {
            Name = "BZip2";
            UUID = new Guid("FCCFB0C3-32EF-40D8-9714-2333F6AC72A9");
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
            // Otherwise base stream is not at correct position
            dataStream.Seek(0, SeekOrigin.Begin);
            return new ForcedSeekStream<BZip2Stream>(decompressedSize, dataStream, CompressionMode.Decompress, true, false);
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
            if(buffer[0] == 0x42 && buffer[1] == 0x5A && buffer[2] == 0x68 && buffer[3] >= 0x31 && buffer[3] <= 0x39)
            {
                if(buffer.Length > 512)
                {
                    // Check it is not an UDIF
                    if(buffer[buffer.Length - 512] == 0x6B && buffer[buffer.Length - 511] == 0x6F && buffer[buffer.Length - 510] == 0x6C && buffer[buffer.Length - 509] == 0x79)
                        return false;
                }
                return true;
            }
            return false;
        }

        public override bool Identify(Stream stream)
        {
            byte[] buffer = new byte[4];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 4);
            stream.Seek(0, SeekOrigin.Begin);

            if(buffer[0] == 0x42 && buffer[1] == 0x5A && buffer[2] == 0x68 && buffer[3] >= 0x31 && buffer[3] <= 0x39)
            {
                if(stream.Length > 512)
                {
                    stream.Seek(-512, SeekOrigin.End);
                    stream.Read(buffer, 0, 4);
                    stream.Seek(0, SeekOrigin.Begin);
                    // Check it is not an UDIF
                    if(buffer[0] == 0x6B && buffer[1] == 0x6F && buffer[2] == 0x6C && buffer[3] == 0x79)
                        return false;
                }
                return true;
            }
            return false;
        }

        public override bool Identify(string path)
        {
            if(File.Exists(path))
            {
                FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[4];

                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(buffer, 0, 4);
                stream.Seek(0, SeekOrigin.Begin);

                if(buffer[0] == 0x42 && buffer[1] == 0x5A && buffer[2] == 0x68 && buffer[3] >= 0x31 && buffer[3] <= 0x39)
                {
                    if(stream.Length > 512)
                    {
                        stream.Seek(-512, SeekOrigin.End);
                        stream.Read(buffer, 0, 4);
                        stream.Seek(0, SeekOrigin.Begin);
                        // Check it is not an UDIF
                        if(buffer[0] == 0x6B && buffer[1] == 0x6F && buffer[2] == 0x6C && buffer[3] == 0x79)
                            return false;
                    }
                    return true;
                }
            }

            return false;
        }

        // BZIP2 does not store the uncompressed size so we must read the whole file. Whole. Whatever time it takes. Or we couldn't seek on it...
        long GuessSize(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            BZip2Stream bzStream = new BZip2Stream(stream, CompressionMode.Decompress, true);
            long size = 0;
            int read = 0;
            byte[] buffer = new byte[1048576];
            do
            {
                read = bzStream.Read(buffer, 0, buffer.Length);
                size += read;
            }
            while(read > 0);
            stream.Seek(0, SeekOrigin.Begin);
            return size;
        }

        public override void Open(byte[] buffer)
        {
            dataStream = new MemoryStream(buffer);
            basePath = null;
            decompressedSize = GuessSize(dataStream);
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            opened = true;
        }

        public override void Open(Stream stream)
        {
            dataStream = stream;
            basePath = null;
            decompressedSize = GuessSize(dataStream);
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            opened = true;
        }

        public override void Open(string path)
        {
            dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            basePath = Path.GetFullPath(path);

            DateTime start = DateTime.UtcNow;
            decompressedSize = GuessSize(dataStream);
            DateTime end = DateTime.UtcNow;
            System.Console.WriteLine("Took {0} seconds to guess size is {1}", (end - start).TotalSeconds, decompressedSize);

            FileInfo fi = new FileInfo(path);
            creationTime = fi.CreationTimeUtc;
            lastWriteTime = fi.LastWriteTimeUtc;
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
            return basePath != null ? Path.GetFileName(basePath) : null;
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