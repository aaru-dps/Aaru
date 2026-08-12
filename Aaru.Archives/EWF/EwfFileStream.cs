// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : EwfFileStream.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : EWF logical evidence plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Stream wrapper for reading file data from EWF logical evidence chunks.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;

namespace Aaru.Archives;

/// <summary>Read-only stream that reads file data from EWF logical evidence chunks.</summary>
sealed class EwfFileStream(EwfArchive archive, long dataOffset, long dataSize) : Stream
{
    readonly EwfArchive _archive    = archive;
    readonly long       _dataOffset = dataOffset;
    long                _position;

    /// <inheritdoc />
    public override bool CanRead  => true;
    /// <inheritdoc />
    public override bool CanSeek  => true;
    /// <inheritdoc />
    public override bool CanWrite => false;
    /// <inheritdoc />
    public override long Length   { get; } = dataSize;

    /// <inheritdoc />
    public override long Position
    {
        get => _position;
        set => _position = Math.Max(0, Math.Min(value, Length));
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        if(_position >= Length) return 0;

        var toRead    = (int)Math.Min(count, Length - _position);
        var totalRead = 0;

        while(totalRead < toRead)
        {
            // Calculate which byte in the overall media data this maps to
            long mediaOffset = _dataOffset + _position;

            // Calculate chunk index and offset within chunk
            var chunkIndex    = (ulong)(mediaOffset / _archive._chunkSize);
            var offsetInChunk = (int)(mediaOffset   % _archive._chunkSize);

            byte[] chunkData = _archive.ReadChunk(chunkIndex);

            if(chunkData == null) break;

            int available = chunkData.Length - offsetInChunk;

            // A chunk shorter than expected would copy nothing and loop forever
            if(available <= 0) break;

            int toCopy = Math.Min(toRead - totalRead, available);

            Array.Copy(chunkData, offsetInChunk, buffer, offset + totalRead, toCopy);

            totalRead += toCopy;
            _position += toCopy;
        }

        return totalRead;
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        switch(origin)
        {
            case SeekOrigin.Begin:
                Position = offset;

                break;
            case SeekOrigin.Current:
                Position += offset;

                break;
            case SeekOrigin.End:
                Position = Length + offset;

                break;
        }

        return Position;
    }

    /// <inheritdoc />
    public override void Flush() {}

    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}