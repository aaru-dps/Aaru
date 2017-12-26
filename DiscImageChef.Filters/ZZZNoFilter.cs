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
    /// <summary>
    ///     No filter for reading files not recognized by any filter
    /// </summary>
    public class ZZZNoFilter : IFilter
    {
        string basePath;
        DateTime creationTime;
        Stream dataStream;
        DateTime lastWriteTime;
        bool opened;

        public virtual string Name => "No filter";
        public virtual Guid Id => new Guid("12345678-AAAA-BBBB-CCCC-123456789000");

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
            return dataStream;
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
            // TODO: Implement support for xattrs/ADS
            return false;
        }

        public virtual bool Identify(byte[] buffer)
        {
            return buffer != null && buffer.Length > 0;
        }

        public virtual bool Identify(Stream stream)
        {
            return stream != null && stream.Length > 0;
        }

        public virtual bool Identify(string path)
        {
            return File.Exists(path);
        }

        public virtual void Open(byte[] buffer)
        {
            dataStream = new MemoryStream(buffer);
            basePath = null;
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            opened = true;
        }

        public virtual void Open(Stream stream)
        {
            dataStream = stream;
            basePath = null;
            creationTime = DateTime.UtcNow;
            lastWriteTime = creationTime;
            opened = true;
        }

        public virtual void Open(string path)
        {
            dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            basePath = Path.GetFullPath(path);
            FileInfo fi = new FileInfo(path);
            creationTime = fi.CreationTimeUtc;
            lastWriteTime = fi.LastWriteTimeUtc;
            opened = true;
        }

        public virtual DateTime GetCreationTime()
        {
            return creationTime;
        }

        public virtual long GetDataForkLength()
        {
            return dataStream.Length;
        }

        public virtual DateTime GetLastWriteTime()
        {
            return lastWriteTime;
        }

        public virtual long GetLength()
        {
            return dataStream.Length;
        }

        public virtual long GetResourceForkLength()
        {
            return 0;
        }

        public virtual string GetFilename()
        {
            return Path.GetFileName(basePath);
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