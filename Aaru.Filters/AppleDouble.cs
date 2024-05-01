// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AppleDouble.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides a filter to open AppleDouble files.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Helpers.IO;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filters;

/// <inheritdoc />
/// <summary>Decodes AppleDouble files</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class AppleDouble : IFilter
{
    const    uint   MAGIC          = 0x00051607;
    const    uint   VERSION        = 0x00010000;
    const    uint   VERSION2       = 0x00020000;
    readonly byte[] _dosHome       = "MS-DOS          "u8.ToArray();
    readonly byte[] _macintoshHome = "Macintosh       "u8.ToArray();
    readonly byte[] _osxHome       = "Mac OS X        "u8.ToArray();
    readonly byte[] _proDosHome    = "ProDOS          "u8.ToArray();
    readonly byte[] _unixHome      = "Unix            "u8.ToArray();
    readonly byte[] _vmsHome       = "VAX VMS         "u8.ToArray();
    Entry           _dataFork;
    Header          _header;
    string          _headerPath;
    Entry           _rsrcFork;

#region IFilter Members

    /// <inheritdoc />
    public string Name => Localization.AppleDouble_Name;

    /// <inheritdoc />
    public Guid Id => new("1B2165EE-C9DF-4B21-BBBB-9E5892B2DF4D");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public void Close() {}

    /// <inheritdoc />
    public string BasePath { get; private set; }

    /// <inheritdoc />
    public DateTime CreationTime { get; private set; }

    /// <inheritdoc />
    public long DataForkLength => _dataFork.length;

    /// <inheritdoc />
    public Stream GetDataForkStream() => new FileStream(BasePath, FileMode.Open, FileAccess.Read);

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

        return new OffsetStream(_headerPath,
                                FileMode.Open,
                                FileAccess.Read,
                                _rsrcFork.offset,
                                _rsrcFork.offset + _rsrcFork.length - 1);
    }

    /// <inheritdoc />
    public bool HasResourceFork => _rsrcFork.length > 0;

    /// <inheritdoc />
    public bool Identify(byte[] buffer) => false;

    /// <inheritdoc />
    public bool Identify(Stream stream) => false;

    /// <inheritdoc />
    public bool Identify(string path)
    {
        string filename      = System.IO.Path.GetFileName(path);
        string filenameNoExt = System.IO.Path.GetFileNameWithoutExtension(path);
        string parentFolder  = System.IO.Path.GetDirectoryName(path);

        parentFolder ??= "";

        if(filename is null || filenameNoExt is null) return false;

        // Prepend data fork name with "R."
        string proDosAppleDouble = System.IO.Path.Combine(parentFolder, "R." + filename);

        // Prepend data fork name with '%'
        string unixAppleDouble = System.IO.Path.Combine(parentFolder, "%" + filename);

        // Change file extension to ADF
        string dosAppleDouble = System.IO.Path.Combine(parentFolder, filenameNoExt + ".ADF");

        // Change file extension to adf
        string dosAppleDoubleLower = System.IO.Path.Combine(parentFolder, filenameNoExt + ".adf");

        // Store AppleDouble header file in ".AppleDouble" folder with same name
        string netatalkAppleDouble = System.IO.Path.Combine(parentFolder, ".AppleDouble", filename);

        // Store AppleDouble header file in "resource.frk" folder with same name
        string daveAppleDouble = System.IO.Path.Combine(parentFolder, "resource.frk", filename);

        // Prepend data fork name with "._"
        string osxAppleDouble = System.IO.Path.Combine(parentFolder, "._" + filename);

        // Adds ".rsrc" extension
        string unArAppleDouble = System.IO.Path.Combine(parentFolder, filename + ".rsrc");

        // Check AppleDouble created by A/UX in ProDOS filesystem
        if(File.Exists(proDosAppleDouble))
        {
            var prodosStream = new FileStream(proDosAppleDouble, FileMode.Open, FileAccess.Read);

            if(prodosStream.Length > 26)
            {
                var prodosB = new byte[26];
                prodosStream.EnsureRead(prodosB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(prodosB);
                prodosStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) return true;
            }
        }

        // Check AppleDouble created by A/UX in UFS filesystem
        if(File.Exists(unixAppleDouble))
        {
            var unixStream = new FileStream(unixAppleDouble, FileMode.Open, FileAccess.Read);

            if(unixStream.Length > 26)
            {
                var unixB = new byte[26];
                unixStream.EnsureRead(unixB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(unixB);
                unixStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) return true;
            }
        }

        // Check AppleDouble created by A/UX in FAT filesystem
        if(File.Exists(dosAppleDouble))
        {
            var dosStream = new FileStream(dosAppleDouble, FileMode.Open, FileAccess.Read);

            if(dosStream.Length > 26)
            {
                var dosB = new byte[26];
                dosStream.EnsureRead(dosB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(dosB);
                dosStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) return true;
            }
        }

        // Check AppleDouble created by A/UX in case preserving FAT filesystem
        if(File.Exists(dosAppleDoubleLower))
        {
            var doslStream = new FileStream(dosAppleDoubleLower, FileMode.Open, FileAccess.Read);

            if(doslStream.Length > 26)
            {
                var doslB = new byte[26];
                doslStream.EnsureRead(doslB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(doslB);
                doslStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) return true;
            }
        }

        // Check AppleDouble created by Netatalk
        if(File.Exists(netatalkAppleDouble))
        {
            var netatalkStream = new FileStream(netatalkAppleDouble, FileMode.Open, FileAccess.Read);

            if(netatalkStream.Length > 26)
            {
                var netatalkB = new byte[26];
                netatalkStream.EnsureRead(netatalkB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(netatalkB);
                netatalkStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) return true;
            }
        }

        // Check AppleDouble created by DAVE
        if(File.Exists(daveAppleDouble))
        {
            var daveStream = new FileStream(daveAppleDouble, FileMode.Open, FileAccess.Read);

            if(daveStream.Length > 26)
            {
                var daveB = new byte[26];
                daveStream.EnsureRead(daveB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(daveB);
                daveStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) return true;
            }
        }

        // Check AppleDouble created by Mac OS X
        if(File.Exists(osxAppleDouble))
        {
            var osxStream = new FileStream(osxAppleDouble, FileMode.Open, FileAccess.Read);

            if(osxStream.Length > 26)
            {
                var osxB = new byte[26];
                osxStream.EnsureRead(osxB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(osxB);
                osxStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) return true;
            }
        }

        // Check AppleDouble created by UnAr (from The Unarchiver)
        if(!File.Exists(unArAppleDouble)) return false;

        var unarStream = new FileStream(unArAppleDouble, FileMode.Open, FileAccess.Read);

        if(unarStream.Length <= 26) return false;

        var unarB = new byte[26];
        unarStream.EnsureRead(unarB, 0, 26);
        _header = Marshal.ByteArrayToStructureBigEndian<Header>(unarB);
        unarStream.Close();

        return _header is { magic: MAGIC, version: VERSION or VERSION2 };
    }

    // Now way to have two files in a single byte array
    /// <inheritdoc />
    public ErrorNumber Open(byte[] buffer) => ErrorNumber.NotSupported;

    // Now way to have two files in a single stream
    /// <inheritdoc />
    public ErrorNumber Open(Stream stream) => ErrorNumber.NotSupported;

    /// <inheritdoc />
    public ErrorNumber Open(string path)
    {
        string filename      = System.IO.Path.GetFileName(path);
        string filenameNoExt = System.IO.Path.GetFileNameWithoutExtension(path);
        string parentFolder  = System.IO.Path.GetDirectoryName(path);

        parentFolder ??= "";

        if(filename is null || filenameNoExt is null) return ErrorNumber.InvalidArgument;

        // Prepend data fork name with "R."
        string proDosAppleDouble = System.IO.Path.Combine(parentFolder, "R." + filename);

        // Prepend data fork name with '%'
        string unixAppleDouble = System.IO.Path.Combine(parentFolder, "%" + filename);

        // Change file extension to ADF
        string dosAppleDouble = System.IO.Path.Combine(parentFolder, filenameNoExt + ".ADF");

        // Change file extension to adf
        string dosAppleDoubleLower = System.IO.Path.Combine(parentFolder, filenameNoExt + ".adf");

        // Store AppleDouble header file in ".AppleDouble" folder with same name
        string netatalkAppleDouble = System.IO.Path.Combine(parentFolder, ".AppleDouble", filename);

        // Store AppleDouble header file in "resource.frk" folder with same name
        string daveAppleDouble = System.IO.Path.Combine(parentFolder, "resource.frk", filename);

        // Prepend data fork name with "._"
        string osxAppleDouble = System.IO.Path.Combine(parentFolder, "._" + filename);

        // Adds ".rsrc" extension
        string unArAppleDouble = System.IO.Path.Combine(parentFolder, filename + ".rsrc");

        // Check AppleDouble created by A/UX in ProDOS filesystem
        if(File.Exists(proDosAppleDouble))
        {
            var prodosStream = new FileStream(proDosAppleDouble, FileMode.Open, FileAccess.Read);

            if(prodosStream.Length > 26)
            {
                var prodosB = new byte[26];
                prodosStream.EnsureRead(prodosB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(prodosB);
                prodosStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) _headerPath = proDosAppleDouble;
            }
        }

        // Check AppleDouble created by A/UX in UFS filesystem
        if(File.Exists(unixAppleDouble))
        {
            var unixStream = new FileStream(unixAppleDouble, FileMode.Open, FileAccess.Read);

            if(unixStream.Length > 26)
            {
                var unixB = new byte[26];
                unixStream.EnsureRead(unixB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(unixB);
                unixStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) _headerPath = unixAppleDouble;
            }
        }

        // Check AppleDouble created by A/UX in FAT filesystem
        if(File.Exists(dosAppleDouble))
        {
            var dosStream = new FileStream(dosAppleDouble, FileMode.Open, FileAccess.Read);

            if(dosStream.Length > 26)
            {
                var dosB = new byte[26];
                dosStream.EnsureRead(dosB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(dosB);
                dosStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) _headerPath = dosAppleDouble;
            }
        }

        // Check AppleDouble created by A/UX in case preserving FAT filesystem
        if(File.Exists(dosAppleDoubleLower))
        {
            var doslStream = new FileStream(dosAppleDoubleLower, FileMode.Open, FileAccess.Read);

            if(doslStream.Length > 26)
            {
                var doslB = new byte[26];
                doslStream.EnsureRead(doslB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(doslB);
                doslStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) _headerPath = dosAppleDoubleLower;
            }
        }

        // Check AppleDouble created by Netatalk
        if(File.Exists(netatalkAppleDouble))
        {
            var netatalkStream = new FileStream(netatalkAppleDouble, FileMode.Open, FileAccess.Read);

            if(netatalkStream.Length > 26)
            {
                var netatalkB = new byte[26];
                netatalkStream.EnsureRead(netatalkB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(netatalkB);
                netatalkStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) _headerPath = netatalkAppleDouble;
            }
        }

        // Check AppleDouble created by DAVE
        if(File.Exists(daveAppleDouble))
        {
            var daveStream = new FileStream(daveAppleDouble, FileMode.Open, FileAccess.Read);

            if(daveStream.Length > 26)
            {
                var daveB = new byte[26];
                daveStream.EnsureRead(daveB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(daveB);
                daveStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) _headerPath = daveAppleDouble;
            }
        }

        // Check AppleDouble created by Mac OS X
        if(File.Exists(osxAppleDouble))
        {
            var osxStream = new FileStream(osxAppleDouble, FileMode.Open, FileAccess.Read);

            if(osxStream.Length > 26)
            {
                var osxB = new byte[26];
                osxStream.EnsureRead(osxB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(osxB);
                osxStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) _headerPath = osxAppleDouble;
            }
        }

        // Check AppleDouble created by UnAr (from The Unarchiver)
        if(File.Exists(unArAppleDouble))
        {
            var unarStream = new FileStream(unArAppleDouble, FileMode.Open, FileAccess.Read);

            if(unarStream.Length > 26)
            {
                var unarB = new byte[26];
                unarStream.EnsureRead(unarB, 0, 26);
                _header = Marshal.ByteArrayToStructureBigEndian<Header>(unarB);
                unarStream.Close();

                if(_header is { magic: MAGIC, version: VERSION or VERSION2 }) _headerPath = unArAppleDouble;
            }
        }

        // TODO: More appropriate error
        if(_headerPath is null) return ErrorNumber.NotSupported;

        var fs = new FileStream(_headerPath, FileMode.Open, FileAccess.Read);
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
            switch((EntryId)entry.id)
            {
                case EntryId.DataFork:
                    // AppleDouble have datafork in separated file
                    break;
                case EntryId.FileDates:
                    fs.Seek(entry.offset, SeekOrigin.Begin);
                    var datesB = new byte[16];
                    fs.EnsureRead(datesB, 0, 16);

                    FileDates dates = Marshal.ByteArrayToStructureBigEndian<FileDates>(datesB);

                    CreationTime  = DateHandlers.UnixUnsignedToDateTime(dates.creationDate);
                    LastWriteTime = DateHandlers.UnixUnsignedToDateTime(dates.modificationDate);

                    break;
                case EntryId.FileInfo:
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
                case EntryId.ResourceFork:
                    _rsrcFork = entry;

                    break;
            }
        }

        _dataFork = new Entry
        {
            id = (uint)EntryId.DataFork
        };

        if(File.Exists(path))
        {
            var dataFs = new FileStream(path, FileMode.Open, FileAccess.Read);
            _dataFork.length = (uint)dataFs.Length;
            dataFs.Close();
        }

        fs.Close();
        BasePath = path;

        return ErrorNumber.NoError;
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
    struct Entry
    {
        public          uint id;
        public readonly uint offset;
        public          uint length;
    }

#endregion

#region Nested type: EntryId

    enum EntryId : uint
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