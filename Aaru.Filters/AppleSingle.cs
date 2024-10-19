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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Helpers.IO;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filters;

/// <inheritdoc />
/// <summary>Decodes AppleSingle files</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class AppleSingle : IFilter
{
    const    uint   MAGIC          = 0x00051600;
    const    uint   VERSION        = 0x00010000;
    const    uint   VERSION2       = 0x00020000;
    readonly byte[] _dosHome       = "MS-DOS          "u8.ToArray();
    readonly byte[] _macintoshHome = "Macintosh       "u8.ToArray();
    readonly byte[] _osxHome       = "Mac OS X        "u8.ToArray();
    readonly byte[] _proDosHome    = "ProDOS          "u8.ToArray();
    readonly byte[] _unixHome      = "Unix            "u8.ToArray();
    readonly byte[] _vmsHome       = "VAX VMS         "u8.ToArray();
    byte[]          _bytes;
    Entry           _dataFork;
    Header          _header;
    bool            _isBytes, _isStream, _isPath;
    Entry           _rsrcFork;
    Stream          _stream;

#region IFilter Members

    /// <inheritdoc />
    public string Name => Localization.AppleSingle_Name;

    /// <inheritdoc />
    public Guid Id => new("A69B20E8-F4D3-42BB-BD2B-4A7263394A05");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public void Close()
    {
        _bytes = null;
        _stream?.Close();
        _isBytes  = false;
        _isStream = false;
        _isPath   = false;
    }

    /// <inheritdoc />
    public string BasePath { get; private set; }

    /// <inheritdoc />
    public DateTime CreationTime { get; private set; }

    /// <inheritdoc />
    public long DataForkLength => _dataFork.length;

    /// <inheritdoc />
    public Stream GetDataForkStream()
    {
        if(_dataFork.length == 0) return null;

        if(_isBytes) return new OffsetStream(_bytes, _dataFork.offset, _dataFork.offset + _dataFork.length - 1);

        if(_isStream) return new OffsetStream(_stream, _dataFork.offset, _dataFork.offset + _dataFork.length - 1);

        if(_isPath)
        {
            return new OffsetStream(BasePath,
                                    FileMode.Open,
                                    FileAccess.Read,
                                    _dataFork.offset,
                                    _dataFork.offset + _dataFork.length - 1);
        }

        return null;
    }

    /// <inheritdoc />
    public string Filename => System.IO.Path.GetFileName(BasePath);

    /// <inheritdoc />
    public DateTime LastWriteTime { get; private set; }

    /// <inheritdoc />
    public long Length => _dataFork.length + _rsrcFork.length;

    /// <inheritdoc />
    public string ParentFolder => System.IO.Path.GetDirectoryName(BasePath);

    /// <inheritdoc />
    public string Path => BasePath;

    /// <inheritdoc />
    public long ResourceForkLength => _rsrcFork.length;

    /// <inheritdoc />
    public Stream GetResourceForkStream()
    {
        if(_rsrcFork.length == 0) return null;

        if(_isBytes) return new OffsetStream(_bytes, _rsrcFork.offset, _rsrcFork.offset + _rsrcFork.length - 1);

        if(_isStream) return new OffsetStream(_stream, _rsrcFork.offset, _rsrcFork.offset + _rsrcFork.length - 1);

        if(_isPath)
        {
            return new OffsetStream(BasePath,
                                    FileMode.Open,
                                    FileAccess.Read,
                                    _rsrcFork.offset,
                                    _rsrcFork.offset + _rsrcFork.length - 1);
        }

        return null;
    }

    /// <inheritdoc />
    public bool HasResourceFork => _rsrcFork.length > 0;

    /// <inheritdoc />
    public bool Identify(byte[] buffer)
    {
        if(buffer == null || buffer.Length < 26) return false;

        var hdrB = new byte[26];
        Array.Copy(buffer, 0, hdrB, 0, 26);
        _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

        return _header is { magic: MAGIC, version: VERSION or VERSION2 };
    }

    /// <inheritdoc />
    public bool Identify(Stream stream)
    {
        if(stream == null || stream.Length < 26) return false;

        var hdrB = new byte[26];
        stream.Seek(0, SeekOrigin.Begin);
        stream.EnsureRead(hdrB, 0, 26);
        _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

        return _header is { magic: MAGIC, version: VERSION or VERSION2 };
    }

    /// <inheritdoc />
    public bool Identify(string path)
    {
        if(!File.Exists(path)) return false;

        var fstream = new FileStream(path, FileMode.Open, FileAccess.Read);

        if(fstream.Length < 26) return false;

        var hdrB = new byte[26];
        fstream.EnsureRead(hdrB, 0, 26);
        _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

        fstream.Close();

        return _header is { magic: MAGIC, version: VERSION or VERSION2 };
    }

    /// <inheritdoc />
    public ErrorNumber Open(byte[] buffer)
    {
        var ms = new MemoryStream(buffer);
        ms.Seek(0, SeekOrigin.Begin);

        var hdrB = new byte[26];
        ms.EnsureRead(hdrB, 0, 26);
        _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

        var entries = new Entry[_header.entries];

        for(var i = 0; i < _header.entries; i++)
        {
            var entry = new byte[12];
            ms.EnsureRead(entry, 0, 12);
            entries[i] = Marshal.ByteArrayToStructureBigEndian<Entry>(entry);
        }

        CreationTime  = DateTime.UtcNow;
        LastWriteTime = CreationTime;

        foreach(Entry entry in entries)
        {
            switch((AppleSingleEntryID)entry.id)
            {
                case AppleSingleEntryID.DataFork:
                    _dataFork = entry;

                    break;
                case AppleSingleEntryID.FileDates:
                    ms.Seek(entry.offset, SeekOrigin.Begin);
                    var datesB = new byte[16];
                    ms.EnsureRead(datesB, 0, 16);

                    FileDates dates = Marshal.ByteArrayToStructureBigEndian<FileDates>(datesB);

                    CreationTime  = DateHandlers.UnixUnsignedToDateTime(dates.creationDate);
                    LastWriteTime = DateHandlers.UnixUnsignedToDateTime(dates.modificationDate);

                    break;
                case AppleSingleEntryID.FileInfo:
                    ms.Seek(entry.offset, SeekOrigin.Begin);
                    var finfo = new byte[entry.length];
                    ms.EnsureRead(finfo, 0, finfo.Length);

                    if(_macintoshHome.SequenceEqual(_header.homeFilesystem))
                    {
                        MacFileInfo macinfo = Marshal.ByteArrayToStructureBigEndian<MacFileInfo>(finfo);

                        CreationTime  = DateHandlers.MacToDateTime(macinfo.creationDate);
                        LastWriteTime = DateHandlers.MacToDateTime(macinfo.modificationDate);
                    }
                    else if(_proDosHome.SequenceEqual(_header.homeFilesystem))
                    {
                        ProDOSFileInfo prodosinfo = Marshal.ByteArrayToStructureBigEndian<ProDOSFileInfo>(finfo);

                        CreationTime  = DateHandlers.MacToDateTime(prodosinfo.creationDate);
                        LastWriteTime = DateHandlers.MacToDateTime(prodosinfo.modificationDate);
                    }
                    else if(_unixHome.SequenceEqual(_header.homeFilesystem))
                    {
                        UnixFileInfo unixinfo = Marshal.ByteArrayToStructureBigEndian<UnixFileInfo>(finfo);

                        CreationTime  = DateHandlers.UnixUnsignedToDateTime(unixinfo.creationDate);
                        LastWriteTime = DateHandlers.UnixUnsignedToDateTime(unixinfo.modificationDate);
                    }
                    else if(_dosHome.SequenceEqual(_header.homeFilesystem))
                    {
                        DOSFileInfo dosinfo = Marshal.ByteArrayToStructureBigEndian<DOSFileInfo>(finfo);

                        LastWriteTime = DateHandlers.DosToDateTime(dosinfo.modificationDate, dosinfo.modificationTime);
                    }

                    break;
                case AppleSingleEntryID.ResourceFork:
                    _rsrcFork = entry;

                    break;
            }
        }

        ms.Close();
        _isBytes = true;
        _bytes   = buffer;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Open(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);

        var hdrB = new byte[26];
        stream.EnsureRead(hdrB, 0, 26);
        _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

        var entries = new Entry[_header.entries];

        for(var i = 0; i < _header.entries; i++)
        {
            var entry = new byte[12];
            stream.EnsureRead(entry, 0, 12);
            entries[i] = Marshal.ByteArrayToStructureBigEndian<Entry>(entry);
        }

        CreationTime  = DateTime.UtcNow;
        LastWriteTime = CreationTime;

        foreach(Entry entry in entries)
        {
            switch((AppleSingleEntryID)entry.id)
            {
                case AppleSingleEntryID.DataFork:
                    _dataFork = entry;

                    break;
                case AppleSingleEntryID.FileDates:
                    stream.Seek(entry.offset, SeekOrigin.Begin);
                    var datesB = new byte[16];
                    stream.EnsureRead(datesB, 0, 16);

                    FileDates dates = Marshal.ByteArrayToStructureBigEndian<FileDates>(datesB);

                    CreationTime  = DateHandlers.MacToDateTime(dates.creationDate);
                    LastWriteTime = DateHandlers.MacToDateTime(dates.modificationDate);

                    break;
                case AppleSingleEntryID.FileInfo:
                    stream.Seek(entry.offset, SeekOrigin.Begin);
                    var finfo = new byte[entry.length];
                    stream.EnsureRead(finfo, 0, finfo.Length);

                    if(_macintoshHome.SequenceEqual(_header.homeFilesystem))
                    {
                        MacFileInfo macinfo = Marshal.ByteArrayToStructureBigEndian<MacFileInfo>(finfo);

                        CreationTime  = DateHandlers.MacToDateTime(macinfo.creationDate);
                        LastWriteTime = DateHandlers.MacToDateTime(macinfo.modificationDate);
                    }
                    else if(_proDosHome.SequenceEqual(_header.homeFilesystem))
                    {
                        ProDOSFileInfo prodosinfo = Marshal.ByteArrayToStructureBigEndian<ProDOSFileInfo>(finfo);

                        CreationTime  = DateHandlers.MacToDateTime(prodosinfo.creationDate);
                        LastWriteTime = DateHandlers.MacToDateTime(prodosinfo.modificationDate);
                    }
                    else if(_unixHome.SequenceEqual(_header.homeFilesystem))
                    {
                        UnixFileInfo unixinfo = Marshal.ByteArrayToStructureBigEndian<UnixFileInfo>(finfo);

                        CreationTime  = DateHandlers.UnixUnsignedToDateTime(unixinfo.creationDate);
                        LastWriteTime = DateHandlers.UnixUnsignedToDateTime(unixinfo.modificationDate);
                    }
                    else if(_dosHome.SequenceEqual(_header.homeFilesystem))
                    {
                        DOSFileInfo dosinfo = Marshal.ByteArrayToStructureBigEndian<DOSFileInfo>(finfo);

                        LastWriteTime = DateHandlers.DosToDateTime(dosinfo.modificationDate, dosinfo.modificationTime);
                    }

                    break;
                case AppleSingleEntryID.ResourceFork:
                    _rsrcFork = entry;

                    break;
            }
        }

        stream.Seek(0, SeekOrigin.Begin);
        _isStream = true;
        _stream   = stream;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Open(string path)
    {
        var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        fs.Seek(0, SeekOrigin.Begin);

        var hdrB = new byte[26];
        fs.EnsureRead(hdrB, 0, 26);
        _header = Marshal.ByteArrayToStructureBigEndian<Header>(hdrB);

        var entries = new Entry[_header.entries];

        for(var i = 0; i < _header.entries; i++)
        {
            var entry = new byte[12];
            fs.EnsureRead(entry, 0, 12);
            entries[i] = Marshal.ByteArrayToStructureBigEndian<Entry>(entry);
        }

        CreationTime  = DateTime.UtcNow;
        LastWriteTime = CreationTime;

        foreach(Entry entry in entries)
        {
            switch((AppleSingleEntryID)entry.id)
            {
                case AppleSingleEntryID.DataFork:
                    _dataFork = entry;

                    break;
                case AppleSingleEntryID.FileDates:
                    fs.Seek(entry.offset, SeekOrigin.Begin);
                    var datesB = new byte[16];
                    fs.EnsureRead(datesB, 0, 16);

                    FileDates dates = Marshal.ByteArrayToStructureBigEndian<FileDates>(datesB);

                    CreationTime  = DateHandlers.MacToDateTime(dates.creationDate);
                    LastWriteTime = DateHandlers.MacToDateTime(dates.modificationDate);

                    break;
                case AppleSingleEntryID.FileInfo:
                    fs.Seek(entry.offset, SeekOrigin.Begin);
                    var finfo = new byte[entry.length];
                    fs.EnsureRead(finfo, 0, finfo.Length);

                    if(_macintoshHome.SequenceEqual(_header.homeFilesystem))
                    {
                        MacFileInfo macinfo = Marshal.ByteArrayToStructureBigEndian<MacFileInfo>(finfo);

                        CreationTime  = DateHandlers.MacToDateTime(macinfo.creationDate);
                        LastWriteTime = DateHandlers.MacToDateTime(macinfo.modificationDate);
                    }
                    else if(_proDosHome.SequenceEqual(_header.homeFilesystem))
                    {
                        ProDOSFileInfo prodosinfo = Marshal.ByteArrayToStructureBigEndian<ProDOSFileInfo>(finfo);

                        CreationTime  = DateHandlers.MacToDateTime(prodosinfo.creationDate);
                        LastWriteTime = DateHandlers.MacToDateTime(prodosinfo.modificationDate);
                    }
                    else if(_unixHome.SequenceEqual(_header.homeFilesystem))
                    {
                        UnixFileInfo unixinfo = Marshal.ByteArrayToStructureBigEndian<UnixFileInfo>(finfo);

                        CreationTime  = DateHandlers.UnixUnsignedToDateTime(unixinfo.creationDate);
                        LastWriteTime = DateHandlers.UnixUnsignedToDateTime(unixinfo.modificationDate);
                    }
                    else if(_dosHome.SequenceEqual(_header.homeFilesystem))
                    {
                        DOSFileInfo dosinfo = Marshal.ByteArrayToStructureBigEndian<DOSFileInfo>(finfo);

                        LastWriteTime = DateHandlers.DosToDateTime(dosinfo.modificationDate, dosinfo.modificationTime);
                    }

                    break;
                case AppleSingleEntryID.ResourceFork:
                    _rsrcFork = entry;

                    break;
            }
        }

        fs.Close();
        _isPath  = true;
        BasePath = path;

        return ErrorNumber.NoError;
    }

#endregion

#region Nested type: AppleSingleEntryID

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
        AfpFileInfo    = 14,
        DirectoryID    = 15
    }

#endregion

#region Nested type: DOSFileInfo

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DOSFileInfo
    {
        public readonly ushort modificationDate;
        public readonly ushort modificationTime;
        public readonly ushort attributes;
    }

#endregion

#region Nested type: Entry

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Entry
    {
        public readonly uint id;
        public readonly uint offset;
        public readonly uint length;
    }

#endregion

#region Nested type: FileDates

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct FileDates
    {
        public readonly uint creationDate;
        public readonly uint modificationDate;
        public readonly uint backupDate;
        public readonly uint accessDate;
    }

#endregion

#region Nested type: Header

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Header
    {
        public readonly uint magic;
        public readonly uint version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] homeFilesystem;
        public readonly ushort entries;
    }

#endregion

#region Nested type: MacFileInfo

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct MacFileInfo
    {
        public readonly uint creationDate;
        public readonly uint modificationDate;
        public readonly uint backupDate;
        public readonly uint accessDate;
    }

#endregion

#region Nested type: ProDOSFileInfo

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

#endregion

#region Nested type: UnixFileInfo

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct UnixFileInfo
    {
        public readonly uint creationDate;
        public readonly uint accessDate;
        public readonly uint modificationDate;
    }

#endregion
}