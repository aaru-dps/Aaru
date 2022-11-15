// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : NonClosableStream.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Compression.
//
// --[ Description ] ----------------------------------------------------------
//
//     Overrides MemoryStream to ignore standard close requests.
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

using System.IO;
using Aaru.Helpers;

namespace Aaru.Compression;

/// <inheritdoc />
/// <summary>Creates a MemoryStream that ignores close commands</summary>
sealed class NonClosableStream : Stream
{
    readonly MemoryStream _baseStream;

    internal NonClosableStream(byte[] buffer) => _baseStream = new MemoryStream(buffer);

    internal NonClosableStream() => _baseStream = new MemoryStream();

    public override bool CanRead  => _baseStream.CanRead;
    public override bool CanSeek  => _baseStream.CanSeek;
    public override bool CanWrite => _baseStream.CanWrite;
    public override long Length   => _baseStream.Length;

    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }

    public override void Flush() => _baseStream.Flush();

    public override int Read(byte[] buffer, int offset, int count) => _baseStream.EnsureRead(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);

    public override void SetLength(long value) => _baseStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _baseStream.Write(buffer, offset, count);

    public override void Close()
    {
        // Do nothing
    }

    internal byte[] GetBuffer() => _baseStream.GetBuffer();

    internal byte[] ToArray() => _baseStream.ToArray();

    internal void ReallyClose() => _baseStream.Close();
}