// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ForcedSeekStream.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides a seekable stream from a forward-readable stream.
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

namespace Aaru.Filters
{
    /// <summary>
    ///     ForcedSeekStream allows to seek a forward-readable stream (like System.IO.Compression streams) by doing the
    ///     slow and known trick of rewinding and forward reading until arriving the desired position.
    /// </summary>
    /// <inheritdoc />
    public sealed class ForcedSeekStream<T> : Stream where T : Stream
    {
        const    int        BUFFER_LEN = 1048576;
        readonly string     _backFile;
        readonly FileStream _backStream;
        readonly T          _baseStream;
        long                _streamLength;

        /// <summary>Initializes a new instance of the <see cref="T:Aaru.Filters.ForcedSeekStream`1" /> class.</summary>
        /// <param name="length">The real (uncompressed) length of the stream.</param>
        /// <param name="args">Parameters that are used to create the base stream.</param>
        /// <inheritdoc />
        public ForcedSeekStream(long length, params object[] args)
        {
            _streamLength = length;
            _baseStream   = (T)Activator.CreateInstance(typeof(T), args);
            _backFile     = Path.GetTempFileName();
            _backStream   = new FileStream(_backFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            if(length == 0)
                CalculateLength();
        }

        /// <summary>Initializes a new instance of the <see cref="T:Aaru.Filters.ForcedSeekStream`1" /> class.</summary>
        /// <param name="args">Parameters that are used to create the base stream.</param>
        /// <inheritdoc />
        public ForcedSeekStream(params object[] args)
        {
            _baseStream = (T)Activator.CreateInstance(typeof(T), args);
            _backFile   = Path.GetTempFileName();
            _backStream = new FileStream(_backFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            CalculateLength();
        }

        /// <inheritdoc />
        public override bool CanRead => _baseStream.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => true;

        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override long Length => _streamLength;

        /// <inheritdoc />
        public override long Position
        {
            get => _backStream.Position;

            set => SetPosition(value);
        }

        /// <summary>
        ///     Calculates the real (uncompressed) length of the stream. It basically reads (uncompresses) the whole stream to
        ///     memory discarding its contents, so it should be used as a last resort.
        /// </summary>
        /// <returns>The length.</returns>
        public void CalculateLength()
        {
            int read;

            do
            {
                byte[] buffer = new byte[BUFFER_LEN];
                read = _baseStream.Read(buffer, 0, BUFFER_LEN);
                _backStream.Write(buffer, 0, read);
            } while(read == BUFFER_LEN);

            _streamLength        = _backStream.Length;
            _backStream.Position = 0;
        }

        void SetPosition(long position)
        {
            if(position == _backStream.Position)
                return;

            if(position < _backStream.Length)
            {
                _backStream.Position = position;

                return;
            }

            if(position > _streamLength)
                position = _streamLength;

            _backStream.Position = _backStream.Length;
            long   toPosition      = position - _backStream.Position;
            int    fullBufferReads = (int)(toPosition / BUFFER_LEN);
            int    restToRead      = (int)(toPosition % BUFFER_LEN);
            byte[] buffer;

            for(int i = 0; i < fullBufferReads; i++)
            {
                buffer = new byte[BUFFER_LEN];
                _baseStream.Read(buffer, 0, BUFFER_LEN);
                _backStream.Write(buffer, 0, BUFFER_LEN);
            }

            buffer = new byte[restToRead];
            _baseStream.Read(buffer, 0, restToRead);
            _backStream.Write(buffer, 0, restToRead);
        }

        /// <inheritdoc />
        public override void Flush()
        {
            _baseStream.Flush();
            _backStream.Flush();
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            if(_backStream.Position + count > _streamLength)
                count = (int)(_streamLength - _backStream.Position);

            if(_backStream.Position + count <= _backStream.Length)
                return _backStream.Read(buffer, offset, count);

            SetPosition(_backStream.Position + count);
            SetPosition(_backStream.Position - count);

            return _backStream.Read(buffer, offset, count);
        }

        /// <inheritdoc />
        public override int ReadByte()
        {
            if(_backStream.Position + 1 > _streamLength)
                return -1;

            if(_backStream.Position + 1 <= _backStream.Length)
                return _backStream.ReadByte();

            SetPosition(_backStream.Position + 1);
            SetPosition(_backStream.Position - 1);

            return _backStream.ReadByte();
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch(origin)
            {
                case SeekOrigin.Begin:
                    if(offset < 0)
                        throw new IOException("Cannot seek before stream start.");

                    SetPosition(offset);

                    break;
                case SeekOrigin.End:
                    if(offset > 0)
                        throw new IOException("Cannot seek after stream end.");

                    if(_streamLength == 0)
                        CalculateLength();

                    SetPosition(_streamLength + offset);

                    break;
                default:
                    SetPosition(_backStream.Position + offset);

                    break;
            }

            return _backStream.Position;
        }

        /// <inheritdoc />
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc />
        public override void Close()
        {
            _backStream?.Close();
            File.Delete(_backFile);
        }

        ~ForcedSeekStream()
        {
            _backStream?.Close();
            File.Delete(_backFile);
        }
    }
}