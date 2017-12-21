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
    /// ForcedSeekStream allows to seek a forward-readable stream (like System.IO.Compression streams)
    /// by doing the slow and known trick of rewinding and forward reading until arriving the desired position.
    /// </summary>
    public class ForcedSeekStream<T> : Stream where T : Stream
    {
        T baseStream;
        object[] parameters;
        long streamLength;
        const int bufferLen = 1048576;
        FileStream backStream;
        string backFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DiscImageChef.Filters.ForcedSeekStream`1"/> class.
        /// </summary>
        /// <param name="length">The real (uncompressed) length of the stream.</param>
        /// <param name="args">Parameters that are used to create the base stream.</param>
        public ForcedSeekStream(long length, params object[] args)
        {
            parameters = args;
            streamLength = length;
            baseStream = (T)Activator.CreateInstance(typeof(T), parameters);
            backFile = Path.GetTempFileName();
            backStream = new FileStream(backFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            if(length == 0) CalculateLength();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DiscImageChef.Filters.ForcedSeekStream`1"/> class.
        /// </summary>
        /// <param name="args">Parameters that are used to create the base stream.</param>
        public ForcedSeekStream(params object[] args)
        {
            parameters = args;
            baseStream = (T)Activator.CreateInstance(typeof(T), parameters);
            backFile = Path.GetTempFileName();
            backStream = new FileStream(backFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            CalculateLength();
        }

        /// <summary>
        /// Calculates the real (uncompressed) length of the stream.
        /// It basically reads (uncompresses) the whole stream to memory discarding its contents,
        /// so it should be used as a last resort.
        /// </summary>
        /// <returns>The length.</returns>
        public void CalculateLength()
        {
            int read;
            do
            {
                byte[] buffer = new byte[bufferLen];
                read = baseStream.Read(buffer, 0, bufferLen);
                backStream.Write(buffer, 0, read);
            }
            while(read == bufferLen);

            streamLength = backStream.Length;
            backStream.Position = 0;
        }

        public override bool CanRead
        {
            get { return baseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return streamLength; }
        }

        public override long Position
        {
            get { return backStream.Position; }

            set { SetPosition(value); }
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
            long toposition = position - backStream.Position;
            int fullBufferReads = (int)(toposition / bufferLen);
            int restToRead = (int)(toposition % bufferLen);
            byte[] buffer;

            for(int i = 0; i < fullBufferReads; i++)
            {
                buffer = new byte[bufferLen];
                baseStream.Read(buffer, 0, bufferLen);
                backStream.Write(buffer, 0, bufferLen);
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
            if(backStream != null) backStream.Close();
            File.Delete(backFile);
        }

        ~ForcedSeekStream()
        {
            if(backStream != null) backStream.Close();
            File.Delete(backFile);
        }
    }
}