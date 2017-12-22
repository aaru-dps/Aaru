// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ZZZNoFilter.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides a filter to open single files.
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

namespace DiscImageChef.Filters
{
    public class ZZZNoFilter : Filter
    {
        Stream dataStream;
        string basePath;
        DateTime lastWriteTime;
        DateTime creationTime;
        bool opened;

        public ZZZNoFilter()
        {
            Name = "No filter";
            UUID = new Guid("12345678-AAAA-BBBB-CCCC-123456789000");
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
            return dataStream;
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
            // TODO: Implement support for xattrs/ADS
            return false;
        }

        public override bool Identify(byte[] buffer)
        {
            return buffer != null && buffer.Length > 0;
        }

        public override bool Identify(Stream stream)
        {
            return stream != null && stream.Length > 0;
        }

        public override bool Identify(string path)
        {
            return File.Exists(path);
        }

        public override void Open(byte[] buffer)
        {
            dataStream = new MemoryStream(buffer);
            basePath = null;
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            opened = true;
        }

        public override void Open(Stream stream)
        {
            dataStream = stream;
            basePath = null;
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            opened = true;
        }

        public override void Open(string path)
        {
            dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            basePath = Path.GetFullPath(path);
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
            return dataStream.Length;
        }

        public override DateTime GetLastWriteTime()
        {
            return lastWriteTime;
        }

        public override long GetLength()
        {
            return dataStream.Length;
        }

        public override long GetResourceForkLength()
        {
            return 0;
        }

        public override string GetFilename()
        {
            return Path.GetFileName(basePath);
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