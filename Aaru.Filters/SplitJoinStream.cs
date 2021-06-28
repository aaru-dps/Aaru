using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Interfaces;
using Microsoft.Win32.SafeHandles;

namespace Aaru.Filters
{
    public class SplitJoinStream : Stream
    {
        readonly Dictionary<long, Stream> _baseStreams;
        long                              _position;
        long                              _streamLength;

        public SplitJoinStream()
        {
            _baseStreams  = new Dictionary<long, Stream>();
            _streamLength = 0;
            _position     = 0;
            Filter        = new ZZZNoFilter();
            Filter.Open(this);
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => _streamLength;

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

        public IFilter Filter { get; }

        public void Add(Stream stream)
        {
            if(!stream.CanSeek)
                throw new ArgumentException("Non-seekable streams are not supported");

            if(!stream.CanRead)
                throw new ArgumentException("Non-readable streams are not supported");

            _baseStreams[_streamLength] =  stream;
            _streamLength               += stream.Length;
        }

        public void Add(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize,
                        FileOptions options) => Add(new FileStream(path, mode, access, share, bufferSize, options));

        public void Add(SafeFileHandle handle, FileAccess access) => Add(new FileStream(handle, access));

        public void Add(SafeFileHandle handle, FileAccess access, int bufferSize) =>
            Add(new FileStream(handle, access, bufferSize));

        public void Add(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync) =>
            Add(new FileStream(handle, access, bufferSize, isAsync));

        public void Add(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize,
                        bool useAsync) => Add(new FileStream(path, mode, access, share, bufferSize, useAsync));

        public void Add(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) =>
            Add(new FileStream(path, mode, access, share, bufferSize));

        public void Add(string path, FileMode mode, FileAccess access, FileShare share) =>
            Add(new FileStream(path, mode, access, share));

        public void Add(string path, FileMode mode, FileAccess access) => Add(new FileStream(path, mode, access));

        public void Add(string path, FileMode mode) => Add(new FileStream(path, mode));

        public void Add(byte[] buffer, int index, int count, bool writable, bool publiclyVisible) =>
            Add(new MemoryStream(buffer, index, count, writable, publiclyVisible));

        public void Add(byte[] buffer, int index, int count, bool writable, long start, long end) =>
            Add(new MemoryStream(buffer, index, count, writable));

        public void Add(byte[] buffer, int index, int count) => Add(new MemoryStream(buffer, index, count));

        public void Add(byte[] buffer, bool writable) => Add(new MemoryStream(buffer, writable));

        public void Add(byte[] buffer) => Add(new MemoryStream(buffer));

        public void Add(IFilter filter) => Add(filter.GetDataForkStream());

        public void AddRange(string basePath, string counterFormat = "{0:D3}", int counterStart = 0)
        {
            while(true)
            {
                string filePath = Path.Combine(basePath, string.Format(counterFormat, counterStart));

                if(!File.Exists(filePath))
                    break;

                Add(filePath, FileMode.Open);

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

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
                                               object state) =>
            throw new NotSupportedException("Asynchronous I/O is not supported.");

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback,
                                                object state) =>
            throw new NotSupportedException("Asynchronous I/O is not supported.");

        public override void Close()
        {
            foreach(Stream stream in _baseStreams.Values)
                stream.Close();

            _baseStreams.Clear();
            _position = 0;
        }

        new void Dispose()
        {
            foreach(Stream stream in _baseStreams.Values)
                stream.Dispose();

            _baseStreams.Clear();
            _position = 0;

            base.Dispose();
        }

        public override int EndRead(IAsyncResult asyncResult) =>
            throw new NotSupportedException("Asynchronous I/O is not supported.");

        public override void EndWrite(IAsyncResult asyncResult) =>
            throw new NotSupportedException("Asynchronous I/O is not supported.");

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

        public override void WriteByte(byte value) => throw new ReadOnlyException("This stream is read-only");

        public override void Flush() {}

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

        public override void SetLength(long value) => throw new ReadOnlyException("This stream is read-only");

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new ReadOnlyException("This stream is read-only");
    }
}