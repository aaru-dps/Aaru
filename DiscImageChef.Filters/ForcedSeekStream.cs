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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;

namespace DiscImageChef.Filters
{
    /// <summary>
    ///     ForcedSeekStream allows to seek a forward-readable stream (like System.IO.Compression streams)
    ///     by doing the slow and known trick of rewinding and forward reading until arriving the desired position.
    /// </summary>
    public class ForcedSeekStream<T> : Stream where T : Stream
    {
        const int  BUFFER_LEN = 1048576;
        string     backFile;
        FileStream backStream;
        T          baseStream;
        long       streamLength;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:DiscImageChef.Filters.ForcedSeekStream`1" /> class.
        /// </summary>
        /// <param name="length">The real (uncompressed) length of the stream.</param>
        /// <param name="args">Parameters that are used to create the base stream.</param>
        public ForcedSeekStream(long length, params object[] args)
        {
            streamLength = length;
            baseStream   = (T)Activator.CreateInstance(typeof(T), args);
            backFile     = Path.GetTempFileName();
            backStream   = new FileStream(backFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            if(length == 0) CalculateLength();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:DiscImageChef.Filters.ForcedSeekStream`1" /> class.
        /// </summary>
        /// <param name="args">Parameters that are used to create the base stream.</param>
        public ForcedSeekStream(params object[] args)
        {
            baseStream = (T)Activator.CreateInstance(typeof(T), args);
            backFile   = Path.GetTempFileName();
            backStream = new FileStream(backFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            CalculateLength();
        }

        public override bool CanRead => baseStream.CanRead;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => streamLength;

        public override long Position
        {
            get => backStream.Position;

            set => SetPosition(value);
        }

        /// <summary>
        ///     Calculates the real (uncompressed) length of the stream.
        ///     It basically reads (uncompresses) the whole stream to memory discarding its contents,
        ///     so it should be used as a last resort.
        /// </summary>
        /// <returns>The length.</returns>
        public void CalculateLength()
        {
            int read;
            do
            {
                byte[] buffer = new byte[BUFFER_LEN];
                read = baseStream.Read(buffer, 0, BUFFER_LEN);
                backStream.Write(buffer, 0, read);
            }
            while(read == BUFFER_LEN);

            streamLength        = backStream.Length;
            backStream.Position = 0;
        }

        void SetPosition(long position)
        {
            if(position == backStream.Position) return;

            if(position < backStream.Length)
            {
                backStream.Position = position;
                return;
            }

            backStream.Position = backStream.Length;
            long   toposition      = position - backStream.Position;
            int    fullBufferReads = (int)(toposition / BUFFER_LEN);
            int    restToRead      = (int)(toposition % BUFFER_LEN);
            byte[] buffer;

            for(int i = 0; i < fullBufferReads; i++)
            {
                buffer = new byte[BUFFER_LEN];
                baseStream.Read(buffer, 0, BUFFER_LEN);
                backStream.Write(buffer, 0, BUFFER_LEN);
            }

            buffer = new byte[restToRead];
            baseStream.Read(buffer, 0, restToRead);
            backStream.Write(buffer, 0, restToRead);
        }

        public override void Flush()
        {
            baseStream.Flush();
            backStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(backStream.Position + count <= backStream.Length) return backStream.Read(buffer, offset, count);

            SetPosition(backStream.Position + count);
            SetPosition(backStream.Position - count);

            return backStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            if(backStream.Position + 1 <= backStream.Length) return backStream.ReadByte();

            SetPosition(backStream.Position + 1);
            SetPosition(backStream.Position - 1);

            return backStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch(origin)
            {
                case SeekOrigin.Begin:
                    if(offset < 0) throw new IOException("Cannot seek before stream start.");

                    SetPosition(offset);
                    break;
                case SeekOrigin.End:
                    if(offset > 0) throw new IOException("Cannot seek after stream end.");

                    if(streamLength == 0) CalculateLength();
                    SetPosition(streamLength + offset);
                    break;
                default:
                    SetPosition(backStream.Position + offset);
                    break;
            }

            return backStream.Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            backStream?.Close();
            File.Delete(backFile);
        }

        ~ForcedSeekStream()
        {
            backStream?.Close();
            File.Delete(backFile);
        }
    }
}