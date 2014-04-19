/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : EndianAwareBinaryReader.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Program tools

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Override for System.IO.Binary.Reader that knows how to handle big-endian.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FileSystemIDandChk
{
    public class EndianAwareBinaryReader : BinaryReader
    {
        byte[] buffer = new byte[8];

        public EndianAwareBinaryReader(Stream input, Encoding encoding, bool isLittleEndian)
			: base(input, encoding)
        {
            IsLittleEndian = isLittleEndian;
        }

        public EndianAwareBinaryReader(Stream input, bool isLittleEndian)
			: this(input, Encoding.UTF8, isLittleEndian)
        {
        }

        public bool IsLittleEndian
        {
            get;
            set;
        }

        public override double ReadDouble()
        {
            if (IsLittleEndian)
                return base.ReadDouble();
            FillMyBuffer(8);
            return BitConverter.ToDouble(buffer.Take(8).Reverse().ToArray(), 0);
        }

        public override short ReadInt16()
        {
            if (IsLittleEndian)
                return base.ReadInt16();
            FillMyBuffer(2);
            return BitConverter.ToInt16(buffer.Take(2).Reverse().ToArray(), 0);
			
        }

        public override int ReadInt32()
        {
            if (IsLittleEndian)
                return base.ReadInt32();
            FillMyBuffer(4);
            return BitConverter.ToInt32(buffer.Take(4).Reverse().ToArray(), 0);
			
        }

        public override long ReadInt64()
        {
            if (IsLittleEndian)
                return base.ReadInt64();
            FillMyBuffer(8);
            return BitConverter.ToInt64(buffer.Take(8).Reverse().ToArray(), 0);
			
        }

        public override float ReadSingle()
        {
            if (IsLittleEndian)
                return base.ReadSingle();
            FillMyBuffer(4);
            return BitConverter.ToSingle(buffer.Take(4).Reverse().ToArray(), 0);
        }

        public override ushort ReadUInt16()
        {
            if (IsLittleEndian)
                return base.ReadUInt16();
            FillMyBuffer(2);
            return BitConverter.ToUInt16(buffer.Take(2).Reverse().ToArray(), 0);
        }

        public override uint ReadUInt32()
        {
            if (IsLittleEndian)
                return base.ReadUInt32();
            FillMyBuffer(4);
            return BitConverter.ToUInt32(buffer.Take(4).Reverse().ToArray(), 0);
        }

        public override ulong ReadUInt64()
        {
            if (IsLittleEndian)
                return base.ReadUInt64();
            FillMyBuffer(8);
            return BitConverter.ToUInt64(buffer.Take(8).Reverse().ToArray(), 0);
        }

        void FillMyBuffer(int numBytes)
        {
            int offset = 0;
            int num2;
            if (numBytes == 1)
            {
                num2 = BaseStream.ReadByte();
                if (num2 == -1)
                {
                    throw new EndOfStreamException("Attempted to read past the end of the stream.");
                }
                buffer[0] = (byte)num2;
            }
            else
            {
                do
                {
                    num2 = BaseStream.Read(buffer, offset, numBytes - offset);
                    if (num2 == 0)
                    {
                        throw new EndOfStreamException("Attempted to read past the end of the stream.");
                    }
                    offset += num2;
                }
                while (offset < numBytes);
            }
        }
    }
}