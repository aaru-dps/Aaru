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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.Helpers;
using Microsoft.Win32.SafeHandles;
#if !NETSTANDARD2_0

#endif

namespace Aaru.Filters;

/// <summary>Creates a stream that is a subset of another stream.</summary>
/// <inheritdoc />
public sealed class OffsetStream : Stream
{
    readonly Stream _baseStream;
    readonly long   _streamEnd;
    readonly long   _streamStart;

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified stream, both inclusive.
    /// </summary>
    /// <param name="stream">Base stream</param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(Stream stream, long start, long end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = stream;

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified file, both inclusive.
    /// </summary>
    /// <param name="path">A relative or absolute path for the file that the stream will encapsulate.</param>
    /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
    /// <param name="access">
    ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
    ///     <see cref="T:System.IO.FileStream" /> object.
    /// </param>
    /// <param name="share">
    ///     A bitwise combination of the enumeration values that determines how the file will be shared by
    ///     processes.
    /// </param>
    /// <param name="bufferSize">
    ///     A positive Int32 value greater than 0 indicating the buffer size. The default buffer size is
    ///     4096.
    /// </param>
    /// <param name="options">A bitwise combination of the enumeration values that specifies additional file options.</param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(string      path,    FileMode mode,  FileAccess access, FileShare share, int bufferSize,
                        FileOptions options, long     start, long       end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new FileStream(path, mode, access, share, bufferSize, options);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified file, both inclusive.
    /// </summary>
    /// <param name="handle">A file handle for the file that the stream will encapsulate.</param>
    /// <param name="access">
    ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
    ///     <see cref="T:System.IO.FileStream" /> object.
    /// </param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(SafeFileHandle handle, FileAccess access, long start, long end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new FileStream(handle, access);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified file, both inclusive.
    /// </summary>
    /// <param name="handle">A file handle for the file that the stream will encapsulate.</param>
    /// <param name="access">
    ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
    ///     <see cref="T:System.IO.FileStream" /> object.
    /// </param>
    /// <param name="bufferSize">
    ///     A positive Int32 value greater than 0 indicating the buffer size. The default buffer size is
    ///     4096.
    /// </param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(SafeFileHandle handle, FileAccess access, int bufferSize, long start, long end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new FileStream(handle, access, bufferSize);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified file, both inclusive.
    /// </summary>
    /// <param name="handle">A file handle for the file that the stream will encapsulate.</param>
    /// <param name="access">
    ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
    ///     <see cref="T:System.IO.FileStream" /> object.
    /// </param>
    /// <param name="bufferSize">
    ///     A positive Int32 value greater than 0 indicating the buffer size. The default buffer size is
    ///     4096.
    /// </param>
    /// <param name="isAsync">Specifies whether to use asynchronous I/O or synchronous I/O.</param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync, long start, long end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new FileStream(handle, access, bufferSize, isAsync);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified file, both inclusive.
    /// </summary>
    /// <param name="path">A relative or absolute path for the file that the stream will encapsulate.</param>
    /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
    /// <param name="access">
    ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
    ///     <see cref="T:System.IO.FileStream" /> object.
    /// </param>
    /// <param name="share">
    ///     A bitwise combination of the enumeration values that determines how the file will be shared by
    ///     processes.
    /// </param>
    /// <param name="bufferSize">
    ///     A positive Int32 value greater than 0 indicating the buffer size. The default buffer size is
    ///     4096.
    /// </param>
    /// <param name="useAsync">Specifies whether to use asynchronous I/O or synchronous I/O.</param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(string path,  FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync,
                        long   start, long     end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new FileStream(path, mode, access, share, bufferSize, useAsync);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified file, both inclusive.
    /// </summary>
    /// <param name="path">A relative or absolute path for the file that the stream will encapsulate.</param>
    /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
    /// <param name="access">
    ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
    ///     <see cref="T:System.IO.FileStream" /> object.
    /// </param>
    /// <param name="share">
    ///     A bitwise combination of the enumeration values that determines how the file will be shared by
    ///     processes.
    /// </param>
    /// <param name="bufferSize">
    ///     A positive Int32 value greater than 0 indicating the buffer size. The default buffer size is
    ///     4096.
    /// </param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, long start,
                        long   end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new FileStream(path, mode, access, share, bufferSize);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified file, both inclusive.
    /// </summary>
    /// <param name="path">A relative or absolute path for the file that the stream will encapsulate.</param>
    /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
    /// <param name="access">
    ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
    ///     <see cref="T:System.IO.FileStream" /> object.
    /// </param>
    /// <param name="share">
    ///     A bitwise combination of the enumeration values that determines how the file will be shared by
    ///     processes.
    /// </param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(string path, FileMode mode, FileAccess access, FileShare share, long start, long end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new FileStream(path, mode, access, share);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified file, both inclusive.
    /// </summary>
    /// <param name="path">A relative or absolute path for the file that the stream will encapsulate.</param>
    /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
    /// <param name="access">
    ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
    ///     <see cref="T:System.IO.FileStream" /> object.
    /// </param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(string path, FileMode mode, FileAccess access, long start, long end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new FileStream(path, mode, access);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified file, both inclusive.
    /// </summary>
    /// <param name="path">A relative or absolute path for the file that the stream will encapsulate.</param>
    /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(string path, FileMode mode, long start, long end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new FileStream(path, mode);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified byte array, both inclusive.
    /// </summary>
    /// <param name="buffer">The array of unsigned bytes to add at the end of this stream.</param>
    /// <param name="index">The index into <paramref name="buffer" /> at which the stream begins.</param>
    /// <param name="count">The length in bytes to add to the end of the current stream.</param>
    /// <param name="writable">The setting of the CanWrite property, currently ignored.</param>
    /// <param name="publiclyVisible">Currently ignored.</param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(byte[] buffer, int index, int count, bool writable, bool publiclyVisible, long start, long end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new MemoryStream(buffer, index, count, writable, publiclyVisible);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified byte array, both inclusive.
    /// </summary>
    /// <param name="buffer">The array of unsigned bytes to add at the end of this stream.</param>
    /// <param name="index">The index into <paramref name="buffer" /> at which the stream begins.</param>
    /// <param name="count">The length in bytes to add to the end of the current stream.</param>
    /// <param name="writable">The setting of the CanWrite property, currently ignored.</param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(byte[] buffer, int index, int count, bool writable, long start, long end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new MemoryStream(buffer, index, count, writable);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified byte array, both inclusive.
    /// </summary>
    /// <param name="buffer">The array of unsigned bytes to add at the end of this stream.</param>
    /// <param name="index">The index into <paramref name="buffer" /> at which the stream begins.</param>
    /// <param name="count">The length in bytes to add to the end of the current stream.</param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(byte[] buffer, int index, int count, long start, long end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new MemoryStream(buffer, index, count);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified byte array, both inclusive.
    /// </summary>
    /// <param name="buffer">The array of unsigned bytes to add at the end of this stream.</param>
    /// <param name="writable">The setting of the CanWrite property, currently ignored.</param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(byte[] buffer, bool writable, long start, long end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new MemoryStream(buffer, writable);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a stream that only allows reading from <paramref name="start" /> to <paramref name="end" /> of the
    ///     specified byte array, both inclusive.
    /// </summary>
    /// <param name="buffer">The array of unsigned bytes to add at the end of this stream.</param>
    /// <param name="start">Start position</param>
    /// <param name="end">Last readable position</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Invalid range</exception>
    public OffsetStream(byte[] buffer, long start, long end)
    {
        if(start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), Localization.Start_cant_be_a_negative_number);

        if(end < 0)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_cant_be_a_negative_number);

        _streamStart = start;
        _streamEnd   = end;

        _baseStream = new MemoryStream(buffer);

        if(end > _baseStream.Length)
            throw new ArgumentOutOfRangeException(nameof(end), Localization.End_is_after_stream_end);

        _baseStream.Position = start;
    }

    /// <inheritdoc />
    public override bool CanRead => _baseStream.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => _baseStream.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => _baseStream.CanWrite;

    /// <inheritdoc />
    public override long Length => _streamEnd - _streamStart + 1;

    /// <inheritdoc />
    public override long Position
    {
        get => _baseStream.Position - _streamStart;

        set
        {
            if(value + _streamStart > _streamEnd)
                throw new IOException(Localization.Cannot_set_position_past_stream_end);

            _baseStream.Position = value + _streamStart;
        }
    }

    ~OffsetStream()
    {
        _baseStream.Close();
        _baseStream.Dispose();
    }

    /// <inheritdoc />
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
        if(_baseStream.Position + count > _streamEnd)
            throw new IOException(Localization.Cannot_read_past_stream_end);

        return _baseStream.BeginRead(buffer, offset, count, callback, state);
    }

    /// <inheritdoc />
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
        if(_baseStream.Position + count > _streamEnd)
            throw new IOException(Localization.Cannot_write_past_stream_end);

        return _baseStream.BeginWrite(buffer, offset, count, callback, state);
    }

    /// <inheritdoc />
    public override void Close() => _baseStream.Close();

    /// <inheritdoc />
    public override int EndRead(IAsyncResult asyncResult) => _baseStream.EndRead(asyncResult);

    /// <inheritdoc />
    public override void EndWrite(IAsyncResult asyncResult) => _baseStream.EndWrite(asyncResult);

    /// <inheritdoc />
    public override int ReadByte() => _baseStream.Position == _streamEnd + 1 ? -1 : _baseStream.ReadByte();

    /// <inheritdoc />
    public override void WriteByte(byte value)
    {
        if(_baseStream.Position + 1 > _streamEnd)
            throw new IOException(Localization.Cannot_write_past_stream_end);

        _baseStream.WriteByte(value);
    }

    /// <inheritdoc />
    public override void Flush() => _baseStream.Flush();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        if(_baseStream.Position + count > _streamEnd + 1)
            throw new IOException(Localization.Cannot_read_past_stream_end);

        return _baseStream.EnsureRead(buffer, offset, count);
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        switch(origin)
        {
            case SeekOrigin.Begin:
                if(offset + _streamStart > _streamEnd)
                    throw new IOException(Localization.Cannot_seek_after_stream_end);

                return _baseStream.Seek(offset + _streamStart, SeekOrigin.Begin) - _streamStart;
            case SeekOrigin.End:
                if(offset - (_baseStream.Length - _streamEnd) < _streamStart)
                    throw new IOException(Localization.Cannot_seek_before_stream_start);

                return _baseStream.Seek(offset - (_baseStream.Length - _streamEnd), SeekOrigin.End) - _streamStart;
            default:
                if(offset + _baseStream.Position > _streamEnd)
                    throw new IOException(Localization.Cannot_seek_after_stream_end);

                return _baseStream.Seek(offset, SeekOrigin.Current) - _streamStart;
        }
    }

    /// <inheritdoc />
    public override void SetLength(long value) =>
        throw new NotSupportedException(Localization.Growing_OffsetStream_is_not_supported);

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        if(_baseStream.Position + count > _streamEnd)
            throw new IOException(Localization.Cannot_write_past_stream_end);

        _baseStream.Write(buffer, offset, count);
    }
}