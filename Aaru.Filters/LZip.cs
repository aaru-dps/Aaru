// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LZip.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Allow to open files that are compressed using lzip.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using SharpCompress.Compressors;
using SharpCompress.Compressors.LZMA;

namespace Aaru.Filters;

/// <inheritdoc />
/// <summary>Decompress lzip files while reading</summary>
public sealed class LZip : IFilter
{
    Stream _dataStream;
    Stream _innerStream;

    /// <inheritdoc />
    public string Name => "LZip";
    /// <inheritdoc />
    public Guid Id => new("09D715E9-20C0-48B1-A8D9-D8897CEC57C9");
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
    public Stream GetDataForkStream() => _innerStream;

    /// <inheritdoc />
    public string Path => BasePath;

    /// <inheritdoc />
    public Stream GetResourceForkStream() => null;

    /// <inheritdoc />
    public bool HasResourceFork => false;

    /// <inheritdoc />
    public bool Identify(byte[] buffer) => buffer[0] == 0x4C && buffer[1] == 0x5A && buffer[2] == 0x49 &&
                                           buffer[3] == 0x50 && buffer[4] == 0x01;

    /// <inheritdoc />
    public bool Identify(Stream stream)
    {
        byte[] buffer = new byte[5];

        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(buffer, 0, 5);
        stream.Seek(0, SeekOrigin.Begin);

        return buffer[0] == 0x4C && buffer[1] == 0x5A && buffer[2] == 0x49 && buffer[3] == 0x50 &&
               buffer[4] == 0x01;
    }

    /// <inheritdoc />
    public bool Identify(string path)
    {
        if(!File.Exists(path))
            return false;

        var    stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        byte[] buffer = new byte[5];

        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(buffer, 0, 5);
        stream.Seek(0, SeekOrigin.Begin);

        return buffer[0] == 0x4C && buffer[1] == 0x5A && buffer[2] == 0x49 && buffer[3] == 0x50 &&
               buffer[4] == 0x01;
    }

    /// <inheritdoc />
    public ErrorNumber Open(byte[] buffer)
    {
        _dataStream    = new MemoryStream(buffer);
        BasePath       = null;
        CreationTime   = DateTime.UtcNow;
        LastWriteTime  = CreationTime;
        DataForkLength = BitConverter.ToInt64(buffer, buffer.Length - 16);

        _innerStream = new ForcedSeekStream<LZipStream>(DataForkLength, _dataStream, CompressionMode.Decompress);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Open(Stream stream)
    {
        _dataStream   = stream;
        BasePath      = null;
        CreationTime  = DateTime.UtcNow;
        LastWriteTime = CreationTime;
        byte[] tmp = new byte[8];
        _dataStream.Seek(-16, SeekOrigin.End);
        _dataStream.Read(tmp, 0, 8);
        DataForkLength = BitConverter.ToInt64(tmp, 0);
        _dataStream.Seek(0, SeekOrigin.Begin);
        _innerStream = new ForcedSeekStream<LZipStream>(DataForkLength, _dataStream, CompressionMode.Decompress);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Open(string path)
    {
        _dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BasePath    = System.IO.Path.GetFullPath(path);

        var fi = new FileInfo(path);
        CreationTime  = fi.CreationTimeUtc;
        LastWriteTime = fi.LastWriteTimeUtc;
        byte[] tmp = new byte[8];
        _dataStream.Seek(-16, SeekOrigin.End);
        _dataStream.Read(tmp, 0, 8);
        DataForkLength = BitConverter.ToInt64(tmp, 0);
        _dataStream.Seek(0, SeekOrigin.Begin);
        _innerStream = new ForcedSeekStream<LZipStream>(DataForkLength, _dataStream, CompressionMode.Decompress);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public DateTime CreationTime { get; private set; }

    /// <inheritdoc />
    public long DataForkLength { get; private set; }

    /// <inheritdoc />
    public DateTime LastWriteTime { get; private set; }

    /// <inheritdoc />
    public long Length => DataForkLength;

    /// <inheritdoc />
    public long ResourceForkLength => 0;

    /// <inheritdoc />
    public string Filename
    {
        get
        {
            if(BasePath?.EndsWith(".lz", StringComparison.InvariantCultureIgnoreCase) == true)
                return BasePath.Substring(0, BasePath.Length - 3);

            return BasePath?.EndsWith(".lzip", StringComparison.InvariantCultureIgnoreCase) == true
                       ? BasePath.Substring(0, BasePath.Length - 5) : BasePath;
        }
    }

    /// <inheritdoc />
    public string ParentFolder => System.IO.Path.GetDirectoryName(BasePath);
}