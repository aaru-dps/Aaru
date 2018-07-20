// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AppleSingle.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides a filter to open AppleSingle files.
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
using System.Linq;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes.Interfaces;

namespace DiscImageChef.Filters
{
    /// <summary>
    ///     Decodes AppleSingle files
    /// </summary>
    public class AppleSingle : IFilter
    {
        const uint AppleSingleMagic    = 0x00051600;
        const uint AppleSingleVersion  = 0x00010000;
        const uint AppleSingleVersion2 = 0x00020000;
        readonly byte[] DOSHome =
            {0x4D, 0x53, 0x2D, 0x44, 0x4F, 0x53, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20};

        readonly byte[] MacintoshHome =
            {0x4D, 0x61, 0x63, 0x69, 0x6E, 0x74, 0x6F, 0x73, 0x68, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20};
        readonly byte[] OSXHome =
            {0x4D, 0x61, 0x63, 0x20, 0x4F, 0x53, 0x20, 0x58, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20};
        readonly byte[] ProDOSHome =
            {0x50, 0x72, 0x6F, 0x44, 0x4F, 0x53, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20};
        readonly byte[] UNIXHome =
            {0x55, 0x6E, 0x69, 0x78, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20};
        readonly byte[] VMSHome =
            {0x56, 0x41, 0x58, 0x20, 0x56, 0x4D, 0x53, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20};
        string   basePath;
        byte[]   bytes;
        DateTime creationTime;

        AppleSingleEntry  dataFork;
        AppleSingleHeader header;
        bool              isBytes, isStream, isPath, opened;
        DateTime          lastWriteTime;
        AppleSingleEntry  rsrcFork;
        Stream            stream;

        public string Name => "AppleSingle";
        public Guid   Id   => new Guid("A69B20E8-F4D3-42BB-BD2B-4A7263394A05");

        public void Close()
        {
            bytes = null;
            stream?.Close();
            isBytes  = false;
            isStream = false;
            isPath   = false;
            opened   = false;
        }

        public string GetBasePath()
        {
            return basePath;
        }

        public DateTime GetCreationTime()
        {
            return creationTime;
        }

        public long GetDataForkLength()
        {
            return dataFork.length;
        }

        public Stream GetDataForkStream()
        {
            if(dataFork.length == 0) return null;

            if(isBytes) return new OffsetStream(bytes,   dataFork.offset, dataFork.offset + dataFork.length - 1);
            if(isStream) return new OffsetStream(stream, dataFork.offset, dataFork.offset + dataFork.length - 1);
            if(isPath)
                return new OffsetStream(basePath, FileMode.Open, FileAccess.Read, dataFork.offset,
                                        dataFork.offset + dataFork.length - 1);

            return null;
        }

        public string GetFilename()
        {
            return Path.GetFileName(basePath);
        }

        public DateTime GetLastWriteTime()
        {
            return lastWriteTime;
        }

        public long GetLength()
        {
            return dataFork.length + rsrcFork.length;
        }

        public string GetParentFolder()
        {
            return Path.GetDirectoryName(basePath);
        }

        public string GetPath()
        {
            return basePath;
        }

        public long GetResourceForkLength()
        {
            return rsrcFork.length;
        }

        public Stream GetResourceForkStream()
        {
            if(rsrcFork.length == 0) return null;

            if(isBytes) return new OffsetStream(bytes,   rsrcFork.offset, rsrcFork.offset + rsrcFork.length - 1);
            if(isStream) return new OffsetStream(stream, rsrcFork.offset, rsrcFork.offset + rsrcFork.length - 1);
            if(isPath)
                return new OffsetStream(basePath, FileMode.Open, FileAccess.Read, rsrcFork.offset,
                                        rsrcFork.offset + rsrcFork.length - 1);

            return null;
        }

        public bool HasResourceFork()
        {
            return rsrcFork.length > 0;
        }

        public bool Identify(byte[] buffer)
        {
            if(buffer == null || buffer.Length < 26) return false;

            byte[] hdr_b = new byte[26];
            Array.Copy(buffer, 0, hdr_b, 0, 26);
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleHeader>(hdr_b);

            return header.magic == AppleSingleMagic &&
                   (header.version == AppleSingleVersion || header.version == AppleSingleVersion2);
        }

        public bool Identify(Stream stream)
        {
            if(stream == null || stream.Length < 26) return false;

            byte[] hdr_b = new byte[26];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(hdr_b, 0, 26);
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleHeader>(hdr_b);

            return header.magic == AppleSingleMagic &&
                   (header.version == AppleSingleVersion || header.version == AppleSingleVersion2);
        }

        public bool Identify(string path)
        {
            FileStream fstream = new FileStream(path, FileMode.Open, FileAccess.Read);
            if(fstream.Length < 26) return false;

            byte[] hdr_b = new byte[26];
            fstream.Read(hdr_b, 0, 26);
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleHeader>(hdr_b);

            fstream.Close();
            return header.magic == AppleSingleMagic &&
                   (header.version == AppleSingleVersion || header.version == AppleSingleVersion2);
        }

        public bool IsOpened()
        {
            return opened;
        }

        public void Open(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            ms.Seek(0, SeekOrigin.Begin);

            byte[] hdr_b = new byte[26];
            ms.Read(hdr_b, 0, 26);
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleHeader>(hdr_b);

            AppleSingleEntry[] entries = new AppleSingleEntry[header.entries];
            for(int i = 0; i < header.entries; i++)
            {
                byte[] entry = new byte[12];
                ms.Read(entry, 0, 12);
                entries[i] = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleEntry>(entry);
            }

            creationTime  = DateTime.UtcNow;
            lastWriteTime = creationTime;
            foreach(AppleSingleEntry entry in entries)
                switch((AppleSingleEntryID)entry.id)
                {
                    case AppleSingleEntryID.DataFork:
                        dataFork = entry;
                        break;
                    case AppleSingleEntryID.FileDates:
                        ms.Seek(entry.offset, SeekOrigin.Begin);
                        byte[] dates_b = new byte[16];
                        ms.Read(dates_b, 0, 16);
                        AppleSingleFileDates dates =
                            BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleFileDates>(dates_b);
                        creationTime  = DateHandlers.UnixUnsignedToDateTime(dates.creationDate);
                        lastWriteTime = DateHandlers.UnixUnsignedToDateTime(dates.modificationDate);
                        break;
                    case AppleSingleEntryID.FileInfo:
                        ms.Seek(entry.offset, SeekOrigin.Begin);
                        byte[] finfo = new byte[entry.length];
                        ms.Read(finfo, 0, finfo.Length);
                        if(MacintoshHome.SequenceEqual(header.homeFilesystem))
                        {
                            AppleSingleMacFileInfo macinfo =
                                BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleMacFileInfo>(finfo);
                            creationTime  = DateHandlers.MacToDateTime(macinfo.creationDate);
                            lastWriteTime = DateHandlers.MacToDateTime(macinfo.modificationDate);
                        }
                        else if(ProDOSHome.SequenceEqual(header.homeFilesystem))
                        {
                            AppleSingleProDOSFileInfo prodosinfo =
                                BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleProDOSFileInfo>(finfo);
                            creationTime  = DateHandlers.MacToDateTime(prodosinfo.creationDate);
                            lastWriteTime = DateHandlers.MacToDateTime(prodosinfo.modificationDate);
                        }
                        else if(UNIXHome.SequenceEqual(header.homeFilesystem))
                        {
                            AppleSingleUNIXFileInfo unixinfo =
                                BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleUNIXFileInfo>(finfo);
                            creationTime  = DateHandlers.UnixUnsignedToDateTime(unixinfo.creationDate);
                            lastWriteTime = DateHandlers.UnixUnsignedToDateTime(unixinfo.modificationDate);
                        }
                        else if(DOSHome.SequenceEqual(header.homeFilesystem))
                        {
                            AppleSingleDOSFileInfo dosinfo =
                                BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleDOSFileInfo>(finfo);
                            lastWriteTime =
                                DateHandlers.DosToDateTime(dosinfo.modificationDate, dosinfo.modificationTime);
                        }

                        break;
                    case AppleSingleEntryID.ResourceFork:
                        rsrcFork = entry;
                        break;
                }

            ms.Close();
            opened  = true;
            isBytes = true;
            bytes   = buffer;
        }

        public void Open(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdr_b = new byte[26];
            stream.Read(hdr_b, 0, 26);
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleHeader>(hdr_b);

            AppleSingleEntry[] entries = new AppleSingleEntry[header.entries];
            for(int i = 0; i < header.entries; i++)
            {
                byte[] entry = new byte[12];
                stream.Read(entry, 0, 12);
                entries[i] = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleEntry>(entry);
            }

            creationTime  = DateTime.UtcNow;
            lastWriteTime = creationTime;
            foreach(AppleSingleEntry entry in entries)
                switch((AppleSingleEntryID)entry.id)
                {
                    case AppleSingleEntryID.DataFork:
                        dataFork = entry;
                        break;
                    case AppleSingleEntryID.FileDates:
                        stream.Seek(entry.offset, SeekOrigin.Begin);
                        byte[] dates_b = new byte[16];
                        stream.Read(dates_b, 0, 16);
                        AppleSingleFileDates dates =
                            BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleFileDates>(dates_b);
                        creationTime  = DateHandlers.MacToDateTime(dates.creationDate);
                        lastWriteTime = DateHandlers.MacToDateTime(dates.modificationDate);
                        break;
                    case AppleSingleEntryID.FileInfo:
                        stream.Seek(entry.offset, SeekOrigin.Begin);
                        byte[] finfo = new byte[entry.length];
                        stream.Read(finfo, 0, finfo.Length);
                        if(MacintoshHome.SequenceEqual(header.homeFilesystem))
                        {
                            AppleSingleMacFileInfo macinfo =
                                BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleMacFileInfo>(finfo);
                            creationTime  = DateHandlers.MacToDateTime(macinfo.creationDate);
                            lastWriteTime = DateHandlers.MacToDateTime(macinfo.modificationDate);
                        }
                        else if(ProDOSHome.SequenceEqual(header.homeFilesystem))
                        {
                            AppleSingleProDOSFileInfo prodosinfo =
                                BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleProDOSFileInfo>(finfo);
                            creationTime  = DateHandlers.MacToDateTime(prodosinfo.creationDate);
                            lastWriteTime = DateHandlers.MacToDateTime(prodosinfo.modificationDate);
                        }
                        else if(UNIXHome.SequenceEqual(header.homeFilesystem))
                        {
                            AppleSingleUNIXFileInfo unixinfo =
                                BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleUNIXFileInfo>(finfo);
                            creationTime  = DateHandlers.UnixUnsignedToDateTime(unixinfo.creationDate);
                            lastWriteTime = DateHandlers.UnixUnsignedToDateTime(unixinfo.modificationDate);
                        }
                        else if(DOSHome.SequenceEqual(header.homeFilesystem))
                        {
                            AppleSingleDOSFileInfo dosinfo =
                                BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleDOSFileInfo>(finfo);
                            lastWriteTime =
                                DateHandlers.DosToDateTime(dosinfo.modificationDate, dosinfo.modificationTime);
                        }

                        break;
                    case AppleSingleEntryID.ResourceFork:
                        rsrcFork = entry;
                        break;
                }

            stream.Seek(0, SeekOrigin.Begin);
            opened      = true;
            isStream    = true;
            this.stream = stream;
        }

        public void Open(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.Begin);

            byte[] hdr_b = new byte[26];
            fs.Read(hdr_b, 0, 26);
            header = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleHeader>(hdr_b);

            AppleSingleEntry[] entries = new AppleSingleEntry[header.entries];
            for(int i = 0; i < header.entries; i++)
            {
                byte[] entry = new byte[12];
                fs.Read(entry, 0, 12);
                entries[i] = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleEntry>(entry);
            }

            creationTime  = DateTime.UtcNow;
            lastWriteTime = creationTime;
            foreach(AppleSingleEntry entry in entries)
                switch((AppleSingleEntryID)entry.id)
                {
                    case AppleSingleEntryID.DataFork:
                        dataFork = entry;
                        break;
                    case AppleSingleEntryID.FileDates:
                        fs.Seek(entry.offset, SeekOrigin.Begin);
                        byte[] dates_b = new byte[16];
                        fs.Read(dates_b, 0, 16);
                        AppleSingleFileDates dates =
                            BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleFileDates>(dates_b);
                        creationTime  = DateHandlers.MacToDateTime(dates.creationDate);
                        lastWriteTime = DateHandlers.MacToDateTime(dates.modificationDate);
                        break;
                    case AppleSingleEntryID.FileInfo:
                        fs.Seek(entry.offset, SeekOrigin.Begin);
                        byte[] finfo = new byte[entry.length];
                        fs.Read(finfo, 0, finfo.Length);
                        if(MacintoshHome.SequenceEqual(header.homeFilesystem))
                        {
                            AppleSingleMacFileInfo macinfo =
                                BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleMacFileInfo>(finfo);
                            creationTime  = DateHandlers.MacToDateTime(macinfo.creationDate);
                            lastWriteTime = DateHandlers.MacToDateTime(macinfo.modificationDate);
                        }
                        else if(ProDOSHome.SequenceEqual(header.homeFilesystem))
                        {
                            AppleSingleProDOSFileInfo prodosinfo =
                                BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleProDOSFileInfo>(finfo);
                            creationTime  = DateHandlers.MacToDateTime(prodosinfo.creationDate);
                            lastWriteTime = DateHandlers.MacToDateTime(prodosinfo.modificationDate);
                        }
                        else if(UNIXHome.SequenceEqual(header.homeFilesystem))
                        {
                            AppleSingleUNIXFileInfo unixinfo =
                                BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleUNIXFileInfo>(finfo);
                            creationTime  = DateHandlers.UnixUnsignedToDateTime(unixinfo.creationDate);
                            lastWriteTime = DateHandlers.UnixUnsignedToDateTime(unixinfo.modificationDate);
                        }
                        else if(DOSHome.SequenceEqual(header.homeFilesystem))
                        {
                            AppleSingleDOSFileInfo dosinfo =
                                BigEndianMarshal.ByteArrayToStructureBigEndian<AppleSingleDOSFileInfo>(finfo);
                            lastWriteTime =
                                DateHandlers.DosToDateTime(dosinfo.modificationDate, dosinfo.modificationTime);
                        }

                        break;
                    case AppleSingleEntryID.ResourceFork:
                        rsrcFork = entry;
                        break;
                }

            fs.Close();
            opened   = true;
            isPath   = true;
            basePath = path;
        }

        enum AppleSingleEntryID : uint
        {
            Invalid        = 0,
            DataFork       = 1,
            ResourceFork   = 2,
            RealName       = 3,
            Comment        = 4,
            Icon           = 5,
            ColorIcon      = 6,
            FileInfo       = 7,
            FileDates      = 8,
            FinderInfo     = 9,
            MacFileInfo    = 10,
            ProDOSFileInfo = 11,
            DOSFileInfo    = 12,
            ShortName      = 13,
            AFPFileInfo    = 14,
            DirectoryID    = 15
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleSingleHeader
        {
            public uint magic;
            public uint version;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] homeFilesystem;
            public ushort entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleSingleEntry
        {
            public uint id;
            public uint offset;
            public uint length;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleSingleFileDates
        {
            public uint creationDate;
            public uint modificationDate;
            public uint backupDate;
            public uint accessDate;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleSingleMacFileInfo
        {
            public uint creationDate;
            public uint modificationDate;
            public uint backupDate;
            public uint accessDate;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleSingleUNIXFileInfo
        {
            public uint creationDate;
            public uint accessDate;
            public uint modificationDate;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleSingleDOSFileInfo
        {
            public ushort modificationDate;
            public ushort modificationTime;
            public ushort attributes;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AppleSingleProDOSFileInfo
        {
            public uint   creationDate;
            public uint   modificationDate;
            public uint   backupDate;
            public ushort access;
            public ushort fileType;
            public uint   auxType;
        }
    }
}