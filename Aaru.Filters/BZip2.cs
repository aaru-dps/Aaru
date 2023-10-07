// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Helpers.IO;
using Ionic.BZip2;

namespace Aaru.Filters;

/// <inheritdoc />
/// <summary>Decompress bz2 files while reading</summary>
public class BZip2 : IFilter
{
    Stream _dataStream;
    Stream _innerStream;

#region IFilter Members

    /// <inheritdoc />
    public string Name => Localization.BZip2_Name;

    /// <inheritdoc />
    public Guid Id => new("FCCFB0C3-32EF-40D8-9714-2333F6AC72A9");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

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
    public bool Identify(byte[] buffer)
    {
        if(buffer[0] != 0x42 || buffer[1] != 0x5A || buffer[2] != 0x68 || buffer[3] < 0x31 || buffer[3] > 0x39)
            return false;

        if(buffer.Length <= 512)
            return true;

        return buffer[^512] != 0x6B || buffer[^511] != 0x6F || buffer[^510] != 0x6C || buffer[^509] != 0x79;
    }

    /// <inheritdoc />
    public bool Identify(Stream stream)
    {
        var buffer = new byte[4];

        stream.Seek(0, SeekOrigin.Begin);
        stream.EnsureRead(buffer, 0, 4);
        stream.Seek(0, SeekOrigin.Begin);

        if(buffer[0] != 0x42 || buffer[1] != 0x5A || buffer[2] != 0x68 || buffer[3] < 0x31 || buffer[3] > 0x39)
            return false;

        if(stream.Length <= 512)
            return true;

        stream.Seek(-512, SeekOrigin.End);
        stream.EnsureRead(buffer, 0, 4);
        stream.Seek(0, SeekOrigin.Begin);

        // Check it is not an UDIF
        return buffer[0] != 0x6B || buffer[1] != 0x6F || buffer[2] != 0x6C || buffer[3] != 0x79;
    }

    /// <inheritdoc />
    public bool Identify(string path)
    {
        if(!File.Exists(path))
            return false;

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var buffer = new byte[4];

        stream.Seek(0, SeekOrigin.Begin);
        stream.EnsureRead(buffer, 0, 4);
        stream.Seek(0, SeekOrigin.Begin);

        if(buffer[0] != 0x42 || buffer[1] != 0x5A || buffer[2] != 0x68 || buffer[3] < 0x31 || buffer[3] > 0x39)
            return false;

        if(stream.Length <= 512)
            return true;

        stream.Seek(-512, SeekOrigin.End);
        stream.EnsureRead(buffer, 0, 4);
        stream.Seek(0, SeekOrigin.Begin);

        // Check it is not an UDIF
        return buffer[0] != 0x6B || buffer[1] != 0x6F || buffer[2] != 0x6C || buffer[3] != 0x79;
    }

    /// <inheritdoc />
    public ErrorNumber Open(byte[] buffer)
    {
        _dataStream    = new MemoryStream(buffer);
        BasePath       = null;
        CreationTime   = DateTime.UtcNow;
        LastWriteTime  = CreationTime;
        _innerStream   = new ForcedSeekStream<BZip2InputStream>(_dataStream, false);
        DataForkLength = _innerStream.Length;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Open(Stream stream)
    {
        _dataStream    = stream;
        BasePath       = null;
        CreationTime   = DateTime.UtcNow;
        LastWriteTime  = CreationTime;
        _innerStream   = new ForcedSeekStream<BZip2InputStream>(_dataStream, false);
        DataForkLength = _innerStream.Length;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Open(string path)
    {
        _dataStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BasePath    = System.IO.Path.GetFullPath(path);

        var fi = new FileInfo(path);
        CreationTime   = fi.CreationTimeUtc;
        LastWriteTime  = fi.LastWriteTimeUtc;
        _innerStream   = new ForcedSeekStream<BZip2InputStream>(_dataStream, false);
        DataForkLength = _innerStream.Length;

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
            if(BasePath?.EndsWith(".bz2", StringComparison.InvariantCultureIgnoreCase) == true)
                return BasePath[..^4];

            return BasePath?.EndsWith(".bzip2", StringComparison.InvariantCultureIgnoreCase) == true
                       ? BasePath[..^6]
                       : BasePath;
        }
    }

    /// <inheritdoc />
    public string ParentFolder => System.IO.Path.GetDirectoryName(BasePath);

#endregion
}