// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;

namespace DiscImageChef.Filters
{
    /// <summary>
    /// ForcedSeekStream allows to seek a forward-readable stream (like System.IO.Compression streams)
    /// by doing the slow and known trick of rewinding and forward reading until arriving the desired position.
    /// </summary>
    public class ForcedSeekStream<T> : Stream where T : Stream
    {
        T baseStream;
        long currentPosition;
        object[] parameters;
        long streamLength;
        const int bufferLen = 1048576;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DiscImageChef.Filters.ForcedSeekStream`1"/> class.
        /// </summary>
        /// <param name="length">The real (uncompressed) length of the stream.</param>
        /// <param name="args">Parameters that are used to create the base stream.</param>
        public ForcedSeekStream(long length, params object[] args)
        {
            parameters = args;
            Rewind();
            streamLength = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DiscImageChef.Filters.ForcedSeekStream`1"/> class.
        /// </summary>
        /// <param name="args">Parameters that are used to create the base stream.</param>
        public ForcedSeekStream(params object[] args)
        {
            parameters = args;
            Rewind();
            streamLength = baseStream.Length;
        }

        /// <summary>
        /// Rewinds the stream to start
        /// </summary>
        public void Rewind()
        {
            baseStream = (T)Activator.CreateInstance(typeof(T), parameters);
            currentPosition = 0;
        }

        /// <summary>
        /// Calculates the real (uncompressed) length of the stream.
        /// It basically reads (uncompresses) the whole stream to memory discarding its contents,
        /// so it should be used as a last resort.
        /// </summary>
        /// <returns>The length.</returns>
        public void CalculateLength()
        {
            long count = 0;
            int read;
            Rewind();
            do
            {
                byte[] buffer = new byte[bufferLen];
                read = baseStream.Read(buffer, 0, bufferLen);
                count += read;
            }
            while(read == bufferLen);

            streamLength = count;
        }

        public override bool CanRead
        {
            get
            {
                return baseStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return baseStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return currentPosition;
            }

            set
            {
                if(value == currentPosition)
                    return;
                
                if(value < currentPosition)
                    Rewind();

                int fullBufferReads = (int)(value / bufferLen);
                int restToRead = (int)(value % bufferLen);
                byte[] buffer;

                for(int i = 0; i < fullBufferReads; i++)
                {
                    buffer = new byte[bufferLen];
                    baseStream.Read(buffer, 0, bufferLen);
                }

                buffer = new byte[restToRead];
                baseStream.Read(buffer, 0, restToRead);

                currentPosition = value;
            }
        }

        public override void Flush()
        {
            baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = baseStream.Read(buffer, offset, count);

            currentPosition += read;

            return read;
        }

        public override int ReadByte()
        {
            int byt = baseStream.ReadByte();

            // Because -1 equals end of stream so we cannot go farther
            if(byt > 0)
                currentPosition++;
            
            return byt;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch(origin)
            {
                case SeekOrigin.Begin:
                    if(offset < 0)
                        throw new IOException("Cannot seek before stream start.");
                    Position = offset;
                    break;
                case SeekOrigin.End:
                    if(offset > 0)
                        throw new IOException("Cannot seek after stream end.");
                    if(streamLength == 0)
                        CalculateLength();
                    Position = streamLength + offset; 
                    break;
                default:
                    Position = currentPosition + offset;
                    break;
            }

            return currentPosition;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}

