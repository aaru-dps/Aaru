using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Interfaces;
using Microsoft.Win32.SafeHandles;

namespace Aaru.Filters
{
    /// <inheritdoc />
    /// <summary>Implements a stream that joins two or more files (sequentially) as a single stream</summary>
    public class SplitJoinStream : Stream
    {
        readonly Dictionary<long, Stream> _baseStreams;
        long                              _position;
        long                              _streamLength;

        /// <inheritdoc />
        public SplitJoinStream()
        {
            _baseStreams  = new Dictionary<long, Stream>();
            _streamLength = 0;
            _position     = 0;
            Filter        = new ZZZNoFilter();
            Filter.Open(this);
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override bool CanSeek => true;

        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override long Length => _streamLength;

        /// <inheritdoc />
        public override long Position
        {
            get => _position;

            set
            {
                if(value >= _streamLength)
                    throw new IOException("Cannot set position past stream end.");

                _position = value;
            }
        }

        /// <summary>Gets a filter from this stream</summary>
        public IFilter Filter { get; }

        /// <summary>Adds a stream at the end of the current stream</summary>
        /// <param name="stream">Stream to add</param>
        /// <exception cref="ArgumentException">The specified stream is non-readable or non-seekable</exception>
        public void Add(Stream stream)
        {
            if(!stream.CanSeek)
                throw new ArgumentException("Non-seekable streams are not supported");

            if(!stream.CanRead)
                throw new ArgumentException("Non-readable streams are not supported");

            _baseStreams[_streamLength] =  stream;
            _streamLength               += stream.Length;
        }

        /// <summary>Adds the specified file to the end of the current stream</summary>
        /// <param name="path">A relative or absolute path for the file that the stream will encapsulate.</param>
        /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
        /// <param name="access">
        ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
        ///     <see cref="FileStream" /> object.
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
        public void Add(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize,
                        FileOptions options) => Add(new FileStream(path, mode, access, share, bufferSize, options));

        /// <summary>Adds the specified file to the end of the current stream</summary>
        /// <param name="handle">A file handle for the file that the stream will encapsulate.</param>
        /// <param name="access">
        ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
        ///     <see cref="FileStream" /> object.
        /// </param>
        public void Add(SafeFileHandle handle, FileAccess access) => Add(new FileStream(handle, access));

        /// <summary>Adds the specified file to the end of the current stream</summary>
        /// <param name="handle">A file handle for the file that the stream will encapsulate.</param>
        /// <param name="access">
        ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
        ///     <see cref="FileStream" /> object.
        /// </param>
        /// <param name="bufferSize">
        ///     A positive Int32 value greater than 0 indicating the buffer size. The default buffer size is
        ///     4096.
        /// </param>
        public void Add(SafeFileHandle handle, FileAccess access, int bufferSize) =>
            Add(new FileStream(handle, access, bufferSize));

        /// <summary>Adds the specified file to the end of the current stream</summary>
        /// <param name="handle">A file handle for the file that the stream will encapsulate.</param>
        /// <param name="access">
        ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
        ///     <see cref="FileStream" /> object.
        /// </param>
        /// <param name="bufferSize">
        ///     A positive Int32 value greater than 0 indicating the buffer size. The default buffer size is
        ///     4096.
        /// </param>
        /// <param name="isAsync">Specifies whether to use asynchronous I/O or synchronous I/O.</param>
        public void Add(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync) =>
            Add(new FileStream(handle, access, bufferSize, isAsync));

        /// <summary>Adds the specified file to the end of the current stream</summary>
        /// <param name="path">A relative or absolute path for the file that the stream will encapsulate.</param>
        /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
        /// <param name="access">
        ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
        ///     <see cref="FileStream" /> object.
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
        public void Add(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize,
                        bool useAsync) => Add(new FileStream(path, mode, access, share, bufferSize, useAsync));

        /// <summary>Adds the specified file to the end of the current stream</summary>
        /// <param name="path">A relative or absolute path for the file that the stream will encapsulate.</param>
        /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
        /// <param name="access">
        ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
        ///     <see cref="FileStream" /> object.
        /// </param>
        /// <param name="share">
        ///     A bitwise combination of the enumeration values that determines how the file will be shared by
        ///     processes.
        /// </param>
        /// <param name="bufferSize">
        ///     A positive Int32 value greater than 0 indicating the buffer size. The default buffer size is
        ///     4096.
        /// </param>
        public void Add(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) =>
            Add(new FileStream(path, mode, access, share, bufferSize));

        /// <summary>Adds the specified file to the end of the current stream</summary>
        /// <param name="path">A relative or absolute path for the file that the stream will encapsulate.</param>
        /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
        /// <param name="access">
        ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
        ///     <see cref="FileStream" /> object.
        /// </param>
        /// <param name="share">
        ///     A bitwise combination of the enumeration values that determines how the file will be shared by
        ///     processes.
        /// </param>
        public void Add(string path, FileMode mode, FileAccess access, FileShare share) =>
            Add(new FileStream(path, mode, access, share));

        /// <summary>Adds the specified file to the end of the current stream</summary>
        /// <param name="path">A relative or absolute path for the file that the stream will encapsulate.</param>
        /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
        /// <param name="access">
        ///     A bitwise combination of the enumeration values that determines how the file can be accessed by a
        ///     <see cref="FileStream" /> object.
        /// </param>
        public void Add(string path, FileMode mode, FileAccess access) => Add(new FileStream(path, mode, access));

        /// <summary>Adds the specified file to the end of the current stream</summary>
        /// <param name="path">A relative or absolute path for the file that the stream will encapsulate.</param>
        /// <param name="mode">One of the enumeration values that determines how to open or create the file.</param>
        public void Add(string path, FileMode mode) => Add(new FileStream(path, mode));

        /// <summary>Adds the specified byte array to the end of the current stream</summary>
        /// <param name="buffer">The array of unsigned bytes to add at the end of this stream.</param>
        /// <param name="index">The index into <paramref name="buffer" /> at which the stream begins.</param>
        /// <param name="count">The length in bytes to add to the end of the current stream.</param>
        /// <param name="writable">The setting of the CanWrite property, currently ignored.</param>
        /// <param name="publiclyVisible">Currently ignored.</param>
        public void Add(byte[] buffer, int index, int count, bool writable, bool publiclyVisible) =>
            Add(new MemoryStream(buffer, index, count, writable, publiclyVisible));

        /// <summary>Adds the specified byte array to the end of the current stream</summary>
        /// <param name="buffer">The array of unsigned bytes to add at the end of this stream.</param>
        /// <param name="index">The index into <paramref name="buffer" /> at which the stream begins.</param>
        /// <param name="count">The length in bytes to add to the end of the current stream.</param>
        /// <param name="writable">The setting of the CanWrite property, currently ignored.</param>
        public void Add(byte[] buffer, int index, int count, bool writable) =>
            Add(new MemoryStream(buffer, index, count, writable));

        /// <summary>Adds the specified byte array to the end of the current stream</summary>
        /// <param name="buffer">The array of unsigned bytes to add at the end of this stream.</param>
        /// <param name="index">The index into <paramref name="buffer" /> at which the stream begins.</param>
        /// <param name="count">The length in bytes to add to the end of the current stream.</param>
        public void Add(byte[] buffer, int index, int count) => Add(new MemoryStream(buffer, index, count));

        /// <summary>Adds the specified byte array to the end of the current stream</summary>
        /// <param name="buffer">The array of unsigned bytes to add at the end of this stream.</param>
        /// <param name="writable">The setting of the CanWrite property, currently ignored.</param>
        public void Add(byte[] buffer, bool writable) => Add(new MemoryStream(buffer, writable));

        /// <summary>Adds the specified byte array to the end of the current stream</summary>
        /// <param name="buffer">The array of unsigned bytes to add at the end of this stream.</param>
        public void Add(byte[] buffer) => Add(new MemoryStream(buffer));

        /// <summary>Adds the data fork of the specified filter to the end of the current stream</summary>
        /// <param name="filter">Filter</param>
        public void Add(IFilter filter) => Add(filter.GetDataForkStream());

        /// <summary>Adds a range of files to the end of the current stream, alphabetically sorted</summary>
        /// <param name="basePath">Base file path, directory path only</param>
        /// <param name="counterFormat">Counter format, includes filename and a formatting string</param>
        /// <param name="counterStart">Counter start, defaults to 0</param>
        public void AddRange(string basePath, string counterFormat = "{0:D3}", int counterStart = 0,
                             FileAccess access = FileAccess.Read)
        {
            while(true)
            {
                string filePath = Path.Combine(basePath, string.Format(counterFormat, counterStart));

                if(!File.Exists(filePath))
                    break;

                Add(filePath, FileMode.Open, access);

                counterStart++;
            }
        }

        ~SplitJoinStream()
        {
            foreach(Stream stream in _baseStreams.Values)
            {
                stream.Close();
                stream.Dispose();
            }

            _baseStreams.Clear();
            _position = 0;
        }

        /// <inheritdoc />
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
                                               object state) =>
            throw new NotSupportedException("Asynchronous I/O is not supported.");

        /// <inheritdoc />
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback,
                                                object state) =>
            throw new NotSupportedException("Asynchronous I/O is not supported.");

        /// <inheritdoc />
        public override void Close()
        {
            foreach(Stream stream in _baseStreams.Values)
                stream.Close();

            _baseStreams.Clear();
            _position = 0;
        }

        /// <inheritdoc />
        public override int EndRead(IAsyncResult asyncResult) =>
            throw new NotSupportedException("Asynchronous I/O is not supported.");

        /// <inheritdoc />
        public override void EndWrite(IAsyncResult asyncResult) =>
            throw new NotSupportedException("Asynchronous I/O is not supported.");

        /// <inheritdoc />
        public override int ReadByte()
        {
            if(_position >= _streamLength)
                return -1;

            KeyValuePair<long, Stream> baseStream = _baseStreams.FirstOrDefault(s => s.Key >= _position);

            if(baseStream.Value == null)
                return -1;

            baseStream.Value.Position = _position - baseStream.Key;
            _position++;

            return baseStream.Value.ReadByte();
        }

        /// <inheritdoc />
        public override void WriteByte(byte value) => throw new ReadOnlyException("This stream is read-only");

        /// <inheritdoc />
        public override void Flush() {}

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;

            while(count > 0)
            {
                KeyValuePair<long, Stream> baseStream = _baseStreams.LastOrDefault(s => s.Key <= _position);

                if(baseStream.Value == null)
                    break;

                baseStream.Value.Position = _position - baseStream.Key;

                int currentCount = count;

                if(baseStream.Value.Position + currentCount > baseStream.Value.Length)
                    currentCount = (int)(baseStream.Value.Length - baseStream.Value.Position);

                read += baseStream.Value.Read(buffer, offset, currentCount);

                count  -= currentCount;
                offset += currentCount;
            }

            return read;
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch(origin)
            {
                case SeekOrigin.Begin:
                    if(offset >= _streamLength)
                        throw new IOException("Cannot seek past stream end.");

                    _position = offset;

                    break;
                case SeekOrigin.End:
                    if(_position - offset < 0)
                        throw new IOException("Cannot seek before stream start.");

                    _position -= offset;

                    break;
                default:
                    if(_position + offset >= _streamLength)
                        throw new IOException("Cannot seek past stream end.");

                    _position += offset;

                    break;
            }

            return _position;
        }

        /// <inheritdoc />
        public override void SetLength(long value) => throw new ReadOnlyException("This stream is read-only");

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count) =>
            throw new ReadOnlyException("This stream is read-only");
    }
}