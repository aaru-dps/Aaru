// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filters;

/// <inheritdoc />
/// <summary>No filter for reading files not recognized by any filter</summary>
public sealed class ZZZNoFilter : IFilter
{
    Stream _dataStream;

    /// <inheritdoc />
    public string Name => Localization.ZZZNoFilter_Name;
    /// <inheritdoc />
    public Guid Id => new("12345678-AAAA-BBBB-CCCC-123456789000");
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
    public Stream GetDataForkStream() => _dataStream;

    /// <inheritdoc />
    public string Path => BasePath;

    /// <inheritdoc />
    public Stream GetResourceForkStream() => null;

    /// <inheritdoc />
    public bool HasResourceFork => false;

    /// <inheritdoc />
    public bool Identify(byte[] buffer) => buffer is { Length: > 0 };

    /// <inheritdoc />
    public bool Identify(Stream stream) => stream is { Length: > 0 };

    /// <inheritdoc />
    public bool Identify(string path) => File.Exists(path);

    /// <inheritdoc />
    public ErrorNumber Open(byte[] buffer)
    {
        _dataStream   = new MemoryStream(buffer);
        BasePath      = null;
        CreationTime  = DateTime.UtcNow;
        LastWriteTime = CreationTime;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Open(Stream stream)
    {
        _dataStream   = stream;
        BasePath      = null;
        CreationTime  = DateTime.UtcNow;
        LastWriteTime = CreationTime;

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

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public DateTime CreationTime { get; private set; }

    /// <inheritdoc />
    public long DataForkLength => _dataStream.Length;

    /// <inheritdoc />
    public DateTime LastWriteTime { get; private set; }

    /// <inheritdoc />
    public long Length => _dataStream.Length;

    /// <inheritdoc />
    public long ResourceForkLength => 0;

    /// <inheritdoc />
    public string Filename => System.IO.Path.GetFileName(BasePath);

    /// <inheritdoc />
    public string ParentFolder => System.IO.Path.GetDirectoryName(BasePath);
}