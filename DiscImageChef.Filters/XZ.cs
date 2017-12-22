// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using SharpCompress.Compressors.Xz;

namespace DiscImageChef.Filters
{
    public class XZ : Filter
    {
        Stream dataStream;
        string basePath;
        DateTime lastWriteTime;
        DateTime creationTime;
        bool opened;
        long decompressedSize;
        Stream innerStream;

        public XZ()
        {
            Name = "XZ";
            UUID = new Guid("666A8617-0444-4C05-9F4F-DF0FD758D0D2");
        }

        public override void Close()
        {
            dataStream?.Close();
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
            return buffer[0] == 0xFD && buffer[1] == 0x37 && buffer[2] == 0x7A && buffer[3] == 0x58 &&
                   buffer[4] == 0x5A && buffer[5] == 0x00 && buffer[buffer.Length - 2] == 0x59 &&
                   buffer[buffer.Length - 1] == 0x5A;
        }

        public override bool Identify(Stream stream)
        {
            byte[] buffer = new byte[6];
            byte[] footer = new byte[2];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 6);
            stream.Seek(-2, SeekOrigin.End);
            stream.Read(footer, 0, 2);
            stream.Seek(0, SeekOrigin.Begin);

            return buffer[0] == 0xFD && buffer[1] == 0x37 && buffer[2] == 0x7A && buffer[3] == 0x58 &&
                   buffer[4] == 0x5A && buffer[5] == 0x00 && footer[0] == 0x59 && footer[1] == 0x5A;
        }

        public override bool Identify(string path)
        {
            if(!File.Exists(path)) return false;

            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[6];
            byte[] footer = new byte[2];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 6);
            stream.Seek(0, SeekOrigin.Begin);
            stream.Seek(-2, SeekOrigin.End);
            stream.Read(footer, 0, 2);
            stream.Seek(0, SeekOrigin.Begin);

            return buffer[0] == 0xFD && buffer[1] == 0x37 && buffer[2] == 0x7A && buffer[3] == 0x58 &&
                   buffer[4] == 0x5A && buffer[5] == 0x00 && footer[0] == 0x59 && footer[1] == 0x5A;
        }

        void GuessSize()
        {
            decompressedSize = 0;
            // Seek to footer backwards size field
            dataStream.Seek(-8, SeekOrigin.End);
            byte[] tmp = new byte[4];
            dataStream.Read(tmp, 0, 4);
            uint backwardSize = (BitConverter.ToUInt32(tmp, 0) + 1) * 4;
            // Seek to first indexed record
            dataStream.Seek(-12 - (backwardSize - 2), SeekOrigin.End);

            // Skip compressed size
            tmp = new byte[backwardSize - 2];
            dataStream.Read(tmp, 0, tmp.Length);
            ulong number = 0;
            int ignore = Decode(tmp, tmp.Length, ref number);

            // Get compressed size
            dataStream.Seek(-12 - (backwardSize - 2 - ignore), SeekOrigin.End);
            tmp = new byte[backwardSize - 2 - ignore];
            dataStream.Read(tmp, 0, tmp.Length);
            Decode(tmp, tmp.Length, ref number);
            decompressedSize = (long)number;

            dataStream.Seek(0, SeekOrigin.Begin);
        }

        int Decode(byte[] buf, int sizeMax, ref ulong num)
        {
            if(sizeMax == 0) return 0;

            if(sizeMax > 9) sizeMax = 9;

            num = (ulong)(buf[0] & 0x7F);
            int i = 0;

            while((buf[i++] & 0x80) == 0x80)
            {
                if(i >= sizeMax || buf[i] == 0x00) return 0;

                num |= (ulong)(buf[i] & 0x7F) << (i * 7);
            }

            return i;
        }

        public override void Open(byte[] buffer)
        {
            dataStream = new MemoryStream(buffer);
            basePath = null;
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            GuessSize();
            innerStream = new ForcedSeekStream<XZStream>(decompressedSize, dataStream);
            opened = true;
        }

        public override void Open(Stream stream)
        {
            dataStream = stream;
            basePath = null;
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            GuessSize();
            innerStream = new ForcedSeekStream<XZStream>(decompressedSize, dataStream);
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
            GuessSize();
            innerStream = new ForcedSeekStream<XZStream>(decompressedSize, dataStream);
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
            if(basePath?.EndsWith(".xz", StringComparison.InvariantCultureIgnoreCase) == true)
                return basePath.Substring(0, basePath.Length - 3);
            return basePath?.EndsWith(".xzip", StringComparison.InvariantCultureIgnoreCase) == true ? basePath.Substring(0, basePath.Length - 5) : basePath;
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