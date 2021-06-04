// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : OffsetStream.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides a stream that's a subset of another stream.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

#if !NETSTANDARD2_0

#endif

namespace Aaru.Filters
{
    /// <summary>Creates a stream that is a subset of another stream.</summary>
    public sealed class OffsetStream : Stream
    {
        readonly Stream _baseStream;
        readonly long   _streamEnd;
        readonly long   _streamStart;

        public OffsetStream(Stream stream, long start, long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = stream;

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize,
                            FileOptions options, long start, long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new FileStream(path, mode, access, share, bufferSize, options);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(SafeFileHandle handle, FileAccess access, long start, long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new FileStream(handle, access);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(SafeFileHandle handle, FileAccess access, int bufferSize, long start, long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new FileStream(handle, access, bufferSize);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync, long start,
                            long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new FileStream(handle, access, bufferSize, isAsync);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize,
                            bool useAsync, long start, long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new FileStream(path, mode, access, share, bufferSize, useAsync);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, long start,
                            long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new FileStream(path, mode, access, share, bufferSize);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(string path, FileMode mode, FileAccess access, FileShare share, long start, long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new FileStream(path, mode, access, share);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(string path, FileMode mode, FileAccess access, long start, long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new FileStream(path, mode, access);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(string path, FileMode mode, long start, long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new FileStream(path, mode);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(byte[] buffer, int index, int count, bool writable, bool publiclyVisible, long start,
                            long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new MemoryStream(buffer, index, count, writable, publiclyVisible);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(byte[] buffer, int index, int count, bool writable, long start, long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new MemoryStream(buffer, index, count, writable);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(byte[] buffer, int index, int count, long start, long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new MemoryStream(buffer, index, count);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(byte[] buffer, bool writable, long start, long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new MemoryStream(buffer, writable);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public OffsetStream(byte[] buffer, long start, long end)
        {
            if(start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0)
                throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            _streamStart = start;
            _streamEnd   = end;

            _baseStream = new MemoryStream(buffer);

            if(end > _baseStream.Length)
                throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");

            _baseStream.Position = start;
        }

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _streamEnd - _streamStart + 1;

        public override long Position
        {
            get => _baseStream.Position - _streamStart;

            set
            {
                if(value + _streamStart > _streamEnd)
                    throw new IOException("Cannot set position past stream end.");

                _baseStream.Position = value + _streamStart;
            }
        }

        ~OffsetStream()
        {
            _baseStream.Close();
            _baseStream.Dispose();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
                                               object state)
        {
            if(_baseStream.Position + count > _streamEnd)
                throw new IOException("Cannot read past stream end.");

            return _baseStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback,
                                                object state)
        {
            if(_baseStream.Position + count > _streamEnd)
                throw new IOException("Cannot write past stream end.");

            return _baseStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close() => _baseStream.Close();

        new void Dispose()
        {
            _baseStream.Dispose();
            base.Dispose();
        }

        public override int EndRead(IAsyncResult asyncResult) => _baseStream.EndRead(asyncResult);

        public override void EndWrite(IAsyncResult asyncResult) => _baseStream.EndWrite(asyncResult);

        public override int ReadByte() => _baseStream.Position == _streamEnd + 1 ? -1 : _baseStream.ReadByte();

        public override void WriteByte(byte value)
        {
            if(_baseStream.Position + 1 > _streamEnd)
                throw new IOException("Cannot write past stream end.");

            _baseStream.WriteByte(value);
        }

        public override void Flush() => _baseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(_baseStream.Position + count > _streamEnd + 1)
                throw new IOException("Cannot read past stream end.");

            return _baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch(origin)
            {
                case SeekOrigin.Begin:
                    if(offset + _streamStart > _streamEnd)
                        throw new IOException("Cannot seek past stream end.");

                    return _baseStream.Seek(offset + _streamStart, SeekOrigin.Begin) - _streamStart;
                case SeekOrigin.End:
                    if(offset - (_baseStream.Length - _streamEnd) < _streamStart)
                        throw new IOException("Cannot seek before stream start.");

                    return _baseStream.Seek(offset - (_baseStream.Length - _streamEnd), SeekOrigin.End) - _streamStart;
                default:
                    if(offset + _baseStream.Position > _streamEnd)
                        throw new IOException("Cannot seek past stream end.");

                    return _baseStream.Seek(offset, SeekOrigin.Current) - _streamStart;
            }
        }

        public override void SetLength(long value) =>
            throw new NotSupportedException("Growing OffsetStream is not supported.");

        public override void Write(byte[] buffer, int offset, int count)
        {
            if(_baseStream.Position + count > _streamEnd)
                throw new IOException("Cannot write past stream end.");

            _baseStream.Write(buffer, offset, count);
        }
    }
}