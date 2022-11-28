// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using SharpCompress.Compressors.Xz;

namespace Aaru.Filters;

/// <inheritdoc />
/// <summary>Decompress xz files while reading</summary>
public sealed class XZ : IFilter
{
    Stream _dataStream;
    Stream _innerStream;

    /// <inheritdoc />
    public string Name => Localization.XZ_Name;
    /// <inheritdoc />
    public Guid Id => new("666A8617-0444-4C05-9F4F-DF0FD758D0D2");
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
    public bool Identify(byte[] buffer) => buffer[0]  == 0xFD && buffer[1]  == 0x37 && buffer[2] == 0x7A &&
                                           buffer[3]  == 0x58 && buffer[4]  == 0x5A && buffer[5] == 0x00 &&
                                           buffer[^2] == 0x59 && buffer[^1] == 0x5A;

    /// <inheritdoc />
    public bool Identify(Stream stream)
    {
        byte[] buffer = new byte[6];
        byte[] footer = new byte[2];

        if(stream.Length < 8)
            return false;

        stream.Seek(0, SeekOrigin.Begin);
        stream.EnsureRead(buffer, 0, 6);
        stream.Seek(-2, SeekOrigin.End);
        stream.EnsureRead(footer, 0, 2);
        stream.Seek(0, SeekOrigin.Begin);

        return buffer[0] == 0xFD && buffer[1] == 0x37 && buffer[2] == 0x7A && buffer[3] == 0x58 && buffer[4] == 0x5A &&
               buffer[5] == 0x00 && footer[0] == 0x59 && footer[1] == 0x5A;
    }

    /// <inheritdoc />
    public bool Identify(string path)
    {
        if(!File.Exists(path))
            return false;

        var    stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        byte[] buffer = new byte[6];
        byte[] footer = new byte[2];

        if(stream.Length < 8)
            return false;

        stream.Seek(0, SeekOrigin.Begin);
        stream.EnsureRead(buffer, 0, 6);
        stream.Seek(-2, SeekOrigin.End);
        stream.EnsureRead(footer, 0, 2);
        stream.Seek(0, SeekOrigin.Begin);

        return buffer[0] == 0xFD && buffer[1] == 0x37 && buffer[2] == 0x7A && buffer[3] == 0x58 && buffer[4] == 0x5A &&
               buffer[5] == 0x00 && footer[0] == 0x59 && footer[1] == 0x5A;
    }

    /// <inheritdoc />
    public ErrorNumber Open(byte[] buffer)
    {
        _dataStream   = new MemoryStream(buffer);
        BasePath      = null;
        CreationTime  = DateTime.UtcNow;
        LastWriteTime = CreationTime;
        GuessSize();
        _innerStream = new ForcedSeekStream<XZStream>(DataForkLength, _dataStream);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Open(Stream stream)
    {
        _dataStream   = stream;
        BasePath      = null;
        CreationTime  = DateTime.UtcNow;
        LastWriteTime = CreationTime;
        GuessSize();
        _innerStream = new ForcedSeekStream<XZStream>(DataForkLength, _dataStream);

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
        GuessSize();
        _innerStream = new ForcedSeekStream<XZStream>(DataForkLength, _dataStream);

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
            if(BasePath?.EndsWith(".xz", StringComparison.InvariantCultureIgnoreCase) == true)
                return BasePath[..^3];

            return BasePath?.EndsWith(".xzip", StringComparison.InvariantCultureIgnoreCase) == true ? BasePath[..^5]
                       : BasePath;
        }
    }

    /// <inheritdoc />
    public string ParentFolder => System.IO.Path.GetDirectoryName(BasePath);

    void GuessSize()
    {
        DataForkLength = 0;

        // Seek to footer backwards size field
        _dataStream.Seek(-8, SeekOrigin.End);
        byte[] tmp = new byte[4];
        _dataStream.EnsureRead(tmp, 0, 4);
        uint backwardSize = (BitConverter.ToUInt32(tmp, 0) + 1) * 4;

        // Seek to first indexed record
        _dataStream.Seek(-12 - (backwardSize - 2), SeekOrigin.End);

        // Skip compressed size
        tmp = new byte[backwardSize - 2];
        _dataStream.EnsureRead(tmp, 0, tmp.Length);
        ulong number = 0;
        int   ignore = Decode(tmp, tmp.Length, ref number);

        // Get compressed size
        _dataStream.Seek(-12 - (backwardSize - 2 - ignore), SeekOrigin.End);
        tmp = new byte[backwardSize - 2 - ignore];
        _dataStream.EnsureRead(tmp, 0, tmp.Length);
        Decode(tmp, tmp.Length, ref number);
        DataForkLength = (long)number;

        _dataStream.Seek(0, SeekOrigin.Begin);
    }

    static int Decode(byte[] buf, int sizeMax, ref ulong num)
    {
        switch(sizeMax)
        {
            case 0: return 0;
            case > 9:
                sizeMax = 9;

                break;
        }

        num = (ulong)(buf[0] & 0x7F);
        int i = 0;

        while((buf[i++] & 0x80) == 0x80)
        {
            if(i      >= sizeMax ||
               buf[i] == 0x00)
                return 0;

            num |= (ulong)(buf[i] & 0x7F) << (i * 7);
        }

        return i;
    }
}