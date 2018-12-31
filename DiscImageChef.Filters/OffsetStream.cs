// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Microsoft.Win32.SafeHandles;
#if !NETSTANDARD2_0
using System.Security.AccessControl;
#endif

namespace DiscImageChef.Filters
{
    /// <summary>
    ///     Creates a stream that is a subset of another stream.
    /// </summary>
    public class OffsetStream : Stream
    {
        readonly Stream baseStream;
        readonly long   streamEnd;
        readonly long   streamStart;

        public OffsetStream(Stream stream, long start, long end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = stream;

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(string      path,    FileMode mode,  FileAccess access, FileShare share, int bufferSize,
                            FileOptions options, long     start, long       end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new FileStream(path, mode, access, share, bufferSize, options);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(SafeFileHandle handle, FileAccess access, long start, long end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new FileStream(handle, access);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(SafeFileHandle handle, FileAccess access, int bufferSize, long start, long end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new FileStream(handle, access, bufferSize);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync, long start,
                            long           end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new FileStream(handle, access, bufferSize, isAsync);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(string path,     FileMode mode,  FileAccess access, FileShare share, int bufferSize,
                            bool   useAsync, long     start, long       end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new FileStream(path, mode, access, share, bufferSize, useAsync);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, long start,
                            long   end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new FileStream(path, mode, access, share, bufferSize);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(string path, FileMode mode, FileAccess access, FileShare share, long start, long end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new FileStream(path, mode, access, share);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(string path, FileMode mode, FileAccess access, long start, long end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new FileStream(path, mode, access);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(string path, FileMode mode, long start, long end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new FileStream(path, mode);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(byte[] buffer, int index, int count, bool writable, bool publiclyVisible, long start,
                            long   end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new MemoryStream(buffer, index, count, writable, publiclyVisible);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(byte[] buffer, int index, int count, bool writable, long start, long end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new MemoryStream(buffer, index, count, writable);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(byte[] buffer, int index, int count, long start, long end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new MemoryStream(buffer, index, count);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(byte[] buffer, bool writable, long start, long end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new MemoryStream(buffer, writable);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(byte[] buffer, long start, long end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new MemoryStream(buffer);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public override bool CanRead => baseStream.CanRead;

        public override bool CanSeek => baseStream.CanSeek;

        public override bool CanWrite => baseStream.CanWrite;

        public override long Length => streamEnd - streamStart + 1;

        public override long Position
        {
            get => baseStream.Position - streamStart;

            set
            {
                if(value + streamStart > streamEnd) throw new IOException("Cannot set position past stream end.");

                baseStream.Position = value;
            }
        }

        ~OffsetStream()
        {
            baseStream.Close();
            baseStream.Dispose();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
                                               object state)
        {
            if(baseStream.Position + count > streamEnd) throw new IOException("Cannot read past stream end.");

            return baseStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback,
                                                object state)
        {
            if(baseStream.Position + count > streamEnd) throw new IOException("Cannot write past stream end.");

            return baseStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            baseStream.Close();
        }

        protected new void Dispose()
        {
            baseStream.Dispose();
            base.Dispose();
        }

        public override int EndRead(IAsyncResult asyncResult) => baseStream.EndRead(asyncResult);

        public override void EndWrite(IAsyncResult asyncResult)
        {
            baseStream.EndWrite(asyncResult);
        }

        public override int ReadByte() => baseStream.Position == streamEnd + 1 ? -1 : baseStream.ReadByte();

        public override void WriteByte(byte value)
        {
            if(baseStream.Position + 1 > streamEnd) throw new IOException("Cannot write past stream end.");

            baseStream.WriteByte(value);
        }

        public override void Flush()
        {
            baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(baseStream.Position + count > streamEnd + 1) throw new IOException("Cannot read past stream end.");

            return baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch(origin)
            {
                case SeekOrigin.Begin:
                    if(offset + streamStart > streamEnd) throw new IOException("Cannot seek past stream end.");

                    return baseStream.Seek(offset + streamStart, SeekOrigin.Begin) - streamStart;
                case SeekOrigin.End:
                    if(offset - (baseStream.Length - streamEnd) < streamStart)
                        throw new IOException("Cannot seek before stream start.");

                    return baseStream.Seek(offset - (baseStream.Length - streamEnd), SeekOrigin.End) - streamStart;
                default:
                    if(offset + baseStream.Position > streamEnd) throw new IOException("Cannot seek past stream end.");

                    return baseStream.Seek(offset, SeekOrigin.Current) - streamStart;
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("Growing OffsetStream is not supported.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if(baseStream.Position + count > streamEnd) throw new IOException("Cannot write past stream end.");

            baseStream.Write(buffer, offset, count);
        }

        #if !NETSTANDARD2_0
        public OffsetStream(string      path, FileMode mode, FileSystemRights rights, FileShare share,
                            int         bufferSize,
                            FileOptions options, long start, long end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new FileStream(path, mode, rights, share, bufferSize, options);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }

        public OffsetStream(string      path, FileMode mode, FileSystemRights rights, FileShare share,
                            int         bufferSize,
                            FileOptions options, FileSecurity fileSecurity, long start, long end)
        {
            if(start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Start can't be a negative number.");

            if(end < 0) throw new ArgumentOutOfRangeException(nameof(end), "End can't be a negative number.");

            streamStart = start;
            streamEnd   = end;

            baseStream = new FileStream(path, mode, rights, share, bufferSize, options, fileSecurity);

            if(end > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(end), "End is after stream end.");
        }
        #endif
    }
}