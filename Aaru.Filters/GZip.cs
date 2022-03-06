// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.IO.Compression;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Filters;

/// <inheritdoc />
/// <summary>Decompress gzip files while reading</summary>
public sealed class GZip : IFilter
{
    Stream _dataStream;
    uint   _decompressedSize;
    Stream _zStream;

    /// <inheritdoc />
    public string Name => "GZip";
    /// <inheritdoc />
    public Guid Id => new("F4996661-4A29-42C9-A2C7-3904EF40F3B0");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";

    /// <inheritdoc />
    public void Close()
    {
        _dataStream?.Close();
        _dataStream = null;
        BasePath    = null;
    }

    /// <inheritdoc />
    public string BasePath { get; private set; }

    /// <inheritdoc />
    public Stream GetDataForkStream() => _zStream;

    /// <inheritdoc />
    public string Path => BasePath;

    /// <inheritdoc />
    public Stream GetResourceForkStream() => null;

    /// <inheritdoc />
    public bool HasResourceFork => false;

    /// <inheritdoc />
    public bool Identify(byte[] buffer) => buffer[0] == 0x1F && buffer[1] == 0x8B && buffer[2] == 0x08;

    /// <inheritdoc />
    public bool Identify(Stream stream)
    {
        byte[] buffer = new byte[3];

        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(buffer, 0, 3);
        stream.Seek(0, SeekOrigin.Begin);

        return buffer[0] == 0x1F && buffer[1] == 0x8B && buffer[2] == 0x08;
    }

    /// <inheritdoc />
    public bool Identify(string path)
    {
        if(!File.Exists(path))
            return false;

        var    stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        byte[] buffer = new byte[3];

        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(buffer, 0, 3);
        stream.Seek(0, SeekOrigin.Begin);

        return buffer[0] == 0x1F && buffer[1] == 0x8B && buffer[2] == 0x08;
    }

    /// <inheritdoc />
    public ErrorNumber Open(byte[] buffer)
    {
        byte[] mtimeB = new byte[4];
        byte[] isizeB = new byte[4];

        _dataStream = new MemoryStream(buffer);
        BasePath    = null;

        _dataStream.Seek(4, SeekOrigin.Begin);
        _dataStream.Read(mtimeB, 0, 4);
        _dataStream.Seek(-4, SeekOrigin.End);
        _dataStream.Read(isizeB, 0, 4);
        _dataStream.Seek(0, SeekOrigin.Begin);

        uint mtime = BitConverter.ToUInt32(mtimeB, 0);
        uint isize = BitConverter.ToUInt32(isizeB, 0);

        _decompressedSize = isize;
        CreationTime      = DateHandlers.UnixUnsignedToDateTime(mtime);
        LastWriteTime     = CreationTime;

        _zStream = new ForcedSeekStream<GZipStream>(_decompressedSize, _dataStream, CompressionMode.Decompress);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Open(Stream stream)
    {
        byte[] mtimeB = new byte[4];
        byte[] isizeB = new byte[4];

        _dataStream = stream;
        BasePath    = null;

        _dataStream.Seek(4, SeekOrigin.Begin);
        _dataStream.Read(mtimeB, 0, 4);
        _dataStream.Seek(-4, SeekOrigin.End);
        _dataStream.Read(isizeB, 0, 4);
        _dataStream.Seek(0, SeekOrigin.Begin);

        uint mtime = BitConverter.ToUInt32(mtimeB, 0);
        uint isize = BitConverter.ToUInt32(isizeB, 0);

        _decompressedSize = isize;
        CreationTime      = DateHandlers.UnixUnsignedToDateTime(mtime);
        LastWriteTime     = CreationTime;

        _zStream = new ForcedSeekStream<GZipStream>(_decompressedSize, _dataStream, CompressionMode.Decompress);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Open(string path)
    {
        byte[] mtimeB = new byte[4];
        byte[] isizeB = new byte[4];

        _dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BasePath    = System.IO.Path.GetFullPath(path);

        _dataStream.Seek(4, SeekOrigin.Begin);
        _dataStream.Read(mtimeB, 0, 4);
        _dataStream.Seek(-4, SeekOrigin.End);
        _dataStream.Read(isizeB, 0, 4);
        _dataStream.Seek(0, SeekOrigin.Begin);

        uint mtime = BitConverter.ToUInt32(mtimeB, 0);
        uint isize = BitConverter.ToUInt32(isizeB, 0);

        _decompressedSize = isize;
        var fi = new FileInfo(path);
        CreationTime  = fi.CreationTimeUtc;
        LastWriteTime = DateHandlers.UnixUnsignedToDateTime(mtime);
        _zStream      = new ForcedSeekStream<GZipStream>(_decompressedSize, _dataStream, CompressionMode.Decompress);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public DateTime CreationTime { get; private set; }

    /// <inheritdoc />
    public long DataForkLength => _decompressedSize;

    /// <inheritdoc />
    public DateTime LastWriteTime { get; private set; }

    /// <inheritdoc />
    public long Length => _decompressedSize;

    /// <inheritdoc />
    public long ResourceForkLength => 0;

    /// <inheritdoc />
    public string Filename
    {
        get
        {
            if(BasePath?.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase) == true)
                return BasePath.Substring(0, BasePath.Length - 3);

            return BasePath?.EndsWith(".gzip", StringComparison.InvariantCultureIgnoreCase) == true
                       ? BasePath.Substring(0, BasePath.Length - 5) : BasePath;
        }
    }

    /// <inheritdoc />
    public string ParentFolder => System.IO.Path.GetDirectoryName(BasePath);
}