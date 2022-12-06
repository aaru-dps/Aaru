// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filters
{
    /// <inheritdoc />
    /// <summary>Decodes AppleSingle files</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public sealed class AppleSingle : IFilter
    {
        const uint MAGIC    = 0x00051600;
        const uint VERSION  = 0x00010000;
        const uint VERSION2 = 0x00020000;
        readonly byte[] _dosHome =
        {
            0x4D, 0x53, 0x2D, 0x44, 0x4F, 0x53, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
        };

        readonly byte[] _macintoshHome =
        {
            0x4D, 0x61, 0x63, 0x69, 0x6E, 0x74, 0x6F, 0x73, 0x68, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
        };
        readonly byte[] _osxHome =
        {
            0x4D, 0x61, 0x63, 0x20, 0x4F, 0x53, 0x20, 0x58, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
        };
        readonly byte[] _proDosHome =
        {
            0x50, 0x72, 0x6F, 0x44, 0x4F, 0x53, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
        };
        readonly byte[] _unixHome =
        {
            0x55, 0x6E, 0x69, 0x78, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
        };
        readonly byte[] _vmsHome =
        {
            0x56, 0x41, 0x58, 0x20, 0x56, 0x4D, 0x53, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
        };
        string   _basePath;
        byte[]   _bytes;
        DateTime _creationTime;
        Entry    _dataFork;
        Header   _header;
        bool     _isBytes, _isStream, _isPath, _opened;
        DateTime _lastWriteTime;
        Entry    _rsrcFork;
        Stream   _stream;

        /// <inheritdoc />
        public string Name => "AppleSingle";
        /// <inheritdoc />
        public Guid Id => new Guid("A69B20E8-F4D3-42BB-BD2B-4A7263394A05");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public void Close()
        {
            _bytes = null;
            _stream?.Close();
            _isBytes  = false;
            _isStream = false;
            _isPath   = false;
            _opened   = false;
        }

        /// <inheritdoc />
        public string GetBasePath() => _basePath;

        /// <inheritdoc />
        public DateTime GetCreationTime() => _creationTime;

        /// <inheritdoc />
        public long GetDataForkLength() => _dataFork.length;

        /// <inheritdoc />
        public Stream GetDataForkStream()
        {
            if(_dataFork.length == 0)
                return null;

            if(_isBytes)
                return new OffsetStream(_bytes, _dataFork.offset, _dataFork.offset + _dataFork.length - 1);

            if(_isStream)
                return new OffsetStream(_stream, _dataFork.offset, _dataFork.offset + _dataFork.length - 1);

            if(_isPath)
                return new OffsetStream(_basePath, FileMode.Open, FileAccess.Read, _dataFork.offset,
                                        _dataFork.offset + _dataFork.length - 1);

            return null;
        }

        /// <inheritdoc />
        public string GetFilename() => Path.GetFileName(_basePath);

        /// <inheritdoc />
        public DateTime GetLastWriteTime() => _lastWriteTime;

        /// <inheritdoc />
        public long GetLength() => _dataFork.length + _rsrcFork.length;

        /// <inheritdoc />
        public string GetParentFolder() => Path.GetDirectoryName(_basePath);

        /// <inheritdoc />
        public string GetPath() => _basePath;

        /// <inheritdoc />
        public long GetResourceForkLength() => _rsrcFork.length;

        /// <inheritdoc />
        public Stream GetResourceForkStream()
        {
            if(_rsrcFork.length == 0)
                return null;

            if(_isBytes)
                return new OffsetStream(_bytes, _rsrcFork.offset, _rsrcFork.offset + _rsrcFork.length - 1);

            if(_isStream)
                return new OffsetStream(_stream, _rsrcFork.offset, _rsrcFork.offset + _rsrcFork.length - 1);

            if(_isPath)
                return new OffsetStream(_basePath, FileMode.Open, FileAccess.Read, _rsrcFork.offset,
                                        _rsrcFork.offset + _rsrcFork.length - 1);

            return null;
        }

        /// <inheritdoc />
        public bool HasResourceFork() => _rsrcFork.length > 0;

        /// <inheritdoc />
        public bool Identify(byte[] buffer)
        {
            if(buffer        == null ||
               buffer.Length < 26)
                return false;

            byte[] hdrB = new byte[26];
            Array.Copy(buffer, 0, hdrB, 0, 26);
            _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

            return _header.magic == MAGIC && (_header.version == VERSION || _header.version == VERSION2);
        }

        /// <inheritdoc />
        public bool Identify(Stream stream)
        {
            if(stream        == null ||
               stream.Length < 26)
                return false;

            byte[] hdrB = new byte[26];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(hdrB, 0, 26);
            _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

            return _header.magic == MAGIC && (_header.version == VERSION || _header.version == VERSION2);
        }

        /// <inheritdoc />
        public bool Identify(string path)
        {
            if(!File.Exists(path))
                return false;

            var fstream = new FileStream(path, FileMode.Open, FileAccess.Read);

            if(fstream.Length < 26)
                return false;

            byte[] hdrB = new byte[26];
            fstream.Read(hdrB, 0, 26);
            _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

            fstream.Close();

            return _header.magic == MAGIC && (_header.version == VERSION || _header.version == VERSION2);
        }

        /// <inheritdoc />
        public bool IsOpened() => _opened;

        /// <inheritdoc />
        public void Open(byte[] buffer)
        {
            var ms = new MemoryStream(buffer);
            ms.Seek(0, SeekOrigin.Begin);

            byte[] hdrB = new byte[26];
            ms.Read(hdrB, 0, 26);
            _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

            Entry[] entries = new Entry[_header.entries];

            for(int i = 0; i < _header.entries; i++)
            {
                byte[] entry = new byte[12];
                ms.Read(entry, 0, 12);
                entries[i] = Marshal.ByteArrayToStructureBigEndian<Entry>(entry);
            }

            _creationTime  = DateTime.UtcNow;
            _lastWriteTime = _creationTime;

            foreach(Entry entry in entries)
                switch((AppleSingleEntryID)entry.id)
                {
                    case AppleSingleEntryID.DataFork:
                        _dataFork = entry;

                        break;
                    case AppleSingleEntryID.FileDates:
                        ms.Seek(entry.offset, SeekOrigin.Begin);
                        byte[] datesB = new byte[16];
                        ms.Read(datesB, 0, 16);

                        FileDates dates = Marshal.ByteArrayToStructureBigEndian<FileDates>(datesB);

                        _creationTime  = DateHandlers.UnixUnsignedToDateTime(dates.creationDate);
                        _lastWriteTime = DateHandlers.UnixUnsignedToDateTime(dates.modificationDate);

                        break;
                    case AppleSingleEntryID.FileInfo:
                        ms.Seek(entry.offset, SeekOrigin.Begin);
                        byte[] finfo = new byte[entry.length];
                        ms.Read(finfo, 0, finfo.Length);

                        if(_macintoshHome.SequenceEqual(_header.homeFilesystem))
                        {
                            MacFileInfo macinfo = Marshal.ByteArrayToStructureBigEndian<MacFileInfo>(finfo);

                            _creationTime  = DateHandlers.MacToDateTime(macinfo.creationDate);
                            _lastWriteTime = DateHandlers.MacToDateTime(macinfo.modificationDate);
                        }
                        else if(_proDosHome.SequenceEqual(_header.homeFilesystem))
                        {
                            ProDOSFileInfo prodosinfo = Marshal.ByteArrayToStructureBigEndian<ProDOSFileInfo>(finfo);

                            _creationTime  = DateHandlers.MacToDateTime(prodosinfo.creationDate);
                            _lastWriteTime = DateHandlers.MacToDateTime(prodosinfo.modificationDate);
                        }
                        else if(_unixHome.SequenceEqual(_header.homeFilesystem))
                        {
                            UnixFileInfo unixinfo = Marshal.ByteArrayToStructureBigEndian<UnixFileInfo>(finfo);

                            _creationTime  = DateHandlers.UnixUnsignedToDateTime(unixinfo.creationDate);
                            _lastWriteTime = DateHandlers.UnixUnsignedToDateTime(unixinfo.modificationDate);
                        }
                        else if(_dosHome.SequenceEqual(_header.homeFilesystem))
                        {
                            DOSFileInfo dosinfo = Marshal.ByteArrayToStructureBigEndian<DOSFileInfo>(finfo);

                            _lastWriteTime =
                                DateHandlers.DosToDateTime(dosinfo.modificationDate, dosinfo.modificationTime);
                        }

                        break;
                    case AppleSingleEntryID.ResourceFork:
                        _rsrcFork = entry;

                        break;
                }

            ms.Close();
            _opened  = true;
            _isBytes = true;
            _bytes   = buffer;
        }

        /// <inheritdoc />
        public void Open(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdrB = new byte[26];
            stream.Read(hdrB, 0, 26);
            _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

            Entry[] entries = new Entry[_header.entries];

            for(int i = 0; i < _header.entries; i++)
            {
                byte[] entry = new byte[12];
                stream.Read(entry, 0, 12);
                entries[i] = Marshal.ByteArrayToStructureBigEndian<Entry>(entry);
            }

            _creationTime  = DateTime.UtcNow;
            _lastWriteTime = _creationTime;

            foreach(Entry entry in entries)
                switch((AppleSingleEntryID)entry.id)
                {
                    case AppleSingleEntryID.DataFork:
                        _dataFork = entry;

                        break;
                    case AppleSingleEntryID.FileDates:
                        stream.Seek(entry.offset, SeekOrigin.Begin);
                        byte[] datesB = new byte[16];
                        stream.Read(datesB, 0, 16);

                        FileDates dates = Marshal.ByteArrayToStructureBigEndian<FileDates>(datesB);

                        _creationTime  = DateHandlers.MacToDateTime(dates.creationDate);
                        _lastWriteTime = DateHandlers.MacToDateTime(dates.modificationDate);

                        break;
                    case AppleSingleEntryID.FileInfo:
                        stream.Seek(entry.offset, SeekOrigin.Begin);
                        byte[] finfo = new byte[entry.length];
                        stream.Read(finfo, 0, finfo.Length);

                        if(_macintoshHome.SequenceEqual(_header.homeFilesystem))
                        {
                            MacFileInfo macinfo = Marshal.ByteArrayToStructureBigEndian<MacFileInfo>(finfo);

                            _creationTime  = DateHandlers.MacToDateTime(macinfo.creationDate);
                            _lastWriteTime = DateHandlers.MacToDateTime(macinfo.modificationDate);
                        }
                        else if(_proDosHome.SequenceEqual(_header.homeFilesystem))
                        {
                            ProDOSFileInfo prodosinfo = Marshal.ByteArrayToStructureBigEndian<ProDOSFileInfo>(finfo);

                            _creationTime  = DateHandlers.MacToDateTime(prodosinfo.creationDate);
                            _lastWriteTime = DateHandlers.MacToDateTime(prodosinfo.modificationDate);
                        }
                        else if(_unixHome.SequenceEqual(_header.homeFilesystem))
                        {
                            UnixFileInfo unixinfo = Marshal.ByteArrayToStructureBigEndian<UnixFileInfo>(finfo);

                            _creationTime  = DateHandlers.UnixUnsignedToDateTime(unixinfo.creationDate);
                            _lastWriteTime = DateHandlers.UnixUnsignedToDateTime(unixinfo.modificationDate);
                        }
                        else if(_dosHome.SequenceEqual(_header.homeFilesystem))
                        {
                            DOSFileInfo dosinfo = Marshal.ByteArrayToStructureBigEndian<DOSFileInfo>(finfo);

                            _lastWriteTime =
                                DateHandlers.DosToDateTime(dosinfo.modificationDate, dosinfo.modificationTime);
                        }

                        break;
                    case AppleSingleEntryID.ResourceFork:
                        _rsrcFork = entry;

                        break;
                }

            stream.Seek(0, SeekOrigin.Begin);
            _opened   = true;
            _isStream = true;
            _stream   = stream;
        }

        /// <inheritdoc />
        public void Open(string path)
        {
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.Begin);

            byte[] hdrB = new byte[26];
            fs.Read(hdrB, 0, 26);
            _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

            Entry[] entries = new Entry[_header.entries];

            for(int i = 0; i < _header.entries; i++)
            {
                byte[] entry = new byte[12];
                fs.Read(entry, 0, 12);
                entries[i] = Marshal.ByteArrayToStructureBigEndian<Entry>(entry);
            }

            _creationTime  = DateTime.UtcNow;
            _lastWriteTime = _creationTime;

            foreach(Entry entry in entries)
                switch((AppleSingleEntryID)entry.id)
                {
                    case AppleSingleEntryID.DataFork:
                        _dataFork = entry;

                        break;
                    case AppleSingleEntryID.FileDates:
                        fs.Seek(entry.offset, SeekOrigin.Begin);
                        byte[] datesB = new byte[16];
                        fs.Read(datesB, 0, 16);

                        FileDates dates = Marshal.ByteArrayToStructureBigEndian<FileDates>(datesB);

                        _creationTime  = DateHandlers.MacToDateTime(dates.creationDate);
                        _lastWriteTime = DateHandlers.MacToDateTime(dates.modificationDate);

                        break;
                    case AppleSingleEntryID.FileInfo:
                        fs.Seek(entry.offset, SeekOrigin.Begin);
                        byte[] finfo = new byte[entry.length];
                        fs.Read(finfo, 0, finfo.Length);

                        if(_macintoshHome.SequenceEqual(_header.homeFilesystem))
                        {
                            MacFileInfo macinfo = Marshal.ByteArrayToStructureBigEndian<MacFileInfo>(finfo);

                            _creationTime  = DateHandlers.MacToDateTime(macinfo.creationDate);
                            _lastWriteTime = DateHandlers.MacToDateTime(macinfo.modificationDate);
                        }
                        else if(_proDosHome.SequenceEqual(_header.homeFilesystem))
                        {
                            ProDOSFileInfo prodosinfo = Marshal.ByteArrayToStructureBigEndian<ProDOSFileInfo>(finfo);

                            _creationTime  = DateHandlers.MacToDateTime(prodosinfo.creationDate);
                            _lastWriteTime = DateHandlers.MacToDateTime(prodosinfo.modificationDate);
                        }
                        else if(_unixHome.SequenceEqual(_header.homeFilesystem))
                        {
                            UnixFileInfo unixinfo = Marshal.ByteArrayToStructureBigEndian<UnixFileInfo>(finfo);

                            _creationTime  = DateHandlers.UnixUnsignedToDateTime(unixinfo.creationDate);
                            _lastWriteTime = DateHandlers.UnixUnsignedToDateTime(unixinfo.modificationDate);
                        }
                        else if(_dosHome.SequenceEqual(_header.homeFilesystem))
                        {
                            DOSFileInfo dosinfo = Marshal.ByteArrayToStructureBigEndian<DOSFileInfo>(finfo);

                            _lastWriteTime =
                                DateHandlers.DosToDateTime(dosinfo.modificationDate, dosinfo.modificationTime);
                        }

                        break;
                    case AppleSingleEntryID.ResourceFork:
                        _rsrcFork = entry;

                        break;
                }

            fs.Close();
            _opened   = true;
            _isPath   = true;
            _basePath = path;
        }

        enum AppleSingleEntryID : uint
        {
            Invalid     = 0, DataFork    = 1, ResourceFork    = 2,
            RealName    = 3, Comment     = 4, Icon            = 5,
            ColorIcon   = 6, FileInfo    = 7, FileDates       = 8,
            FinderInfo  = 9, MacFileInfo = 10, ProDOSFileInfo = 11,
            DOSFileInfo = 12, ShortName  = 13, AfpFileInfo    = 14,
            DirectoryID = 15
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Header
        {
            public readonly uint magic;
            public readonly uint version;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] homeFilesystem;
            public readonly ushort entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Entry
        {
            public readonly uint id;
            public readonly uint offset;
            public readonly uint length;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct FileDates
        {
            public readonly uint creationDate;
            public readonly uint modificationDate;
            public readonly uint backupDate;
            public readonly uint accessDate;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct MacFileInfo
        {
            public readonly uint creationDate;
            public readonly uint modificationDate;
            public readonly uint backupDate;
            public readonly uint accessDate;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct UnixFileInfo
        {
            public readonly uint creationDate;
            public readonly uint accessDate;
            public readonly uint modificationDate;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct DOSFileInfo
        {
            public readonly ushort modificationDate;
            public readonly ushort modificationTime;
            public readonly ushort attributes;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ProDOSFileInfo
        {
            public readonly uint   creationDate;
            public readonly uint   modificationDate;
            public readonly uint   backupDate;
            public readonly ushort access;
            public readonly ushort fileType;
            public readonly uint   auxType;
        }
    }
}