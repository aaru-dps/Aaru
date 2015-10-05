/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Adler32Context.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Checksums.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Implements an Adler-32 algorithm.
 
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
Copyright (C) 2011-2015 Claunia.com
****************************************************************************/
//$Id$
using System;
using System.Text;
using System.IO;

namespace DiscImageChef.Checksums
{
    public class Adler32Context
    {
        UInt16 sum1, sum2;
        const UInt16 AdlerModule = 65521;

        /// <summary>
        /// Initializes the Adler-32 sums
        /// </summary>
        public void Init()
        {
            sum1 = 1;
            sum2 = 0;
        }

        /// <summary>
        /// Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            for (int i = 0; i < len; i++)
            {
                sum1 = (ushort)((sum1 + data[i]) % AdlerModule);
                sum2 = (ushort)((sum2 + sum1) % AdlerModule);
            }
        }

        /// <summary>
        /// Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data)
        {
            Update(data, (uint)data.Length);
        }

        /// <summary>
        /// Returns a byte array of the hash value.
        /// </summary>
        public byte[] Final()
        {
            UInt32 finalSum = (uint)((sum2 << 16) | sum1);
            return BigEndianBitConverter.GetBytes(finalSum);
        }

        /// <summary>
        /// Returns a hexadecimal representation of the hash value.
        /// </summary>
        public string End()
        {
            UInt32 finalSum = (uint)((sum2 << 16) | sum1);
            StringBuilder adlerOutput = new StringBuilder();

            for (int i = 0; i < BigEndianBitConverter.GetBytes(finalSum).Length; i++)
            {
                adlerOutput.Append(BigEndianBitConverter.GetBytes(finalSum)[i].ToString("x2"));
            }

            return adlerOutput.ToString();
        }

        /// <summary>
        /// Gets the hash of a file
        /// </summary>
        /// <param name="filename">File path.</param>
        public static byte[] File(string filename)
        {
            byte[] hash;
            File(filename, out hash);
            return hash;
        }

        /// <summary>
        /// Gets the hash of a file in hexadecimal and as a byte array.
        /// </summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string File(string filename, out byte[] hash)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            UInt16 localSum1, localSum2;
            UInt32 finalSum;

            localSum1 = 1;
            localSum2 = 0;

            localSum1 = (ushort)((localSum1 + fileStream.ReadByte()) % AdlerModule);
            localSum2 = (ushort)((localSum2 + localSum1) % AdlerModule);

            finalSum = (uint)((localSum2 << 16) | localSum1);

            hash = BitConverter.GetBytes(finalSum);

            StringBuilder adlerOutput = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                adlerOutput.Append(hash[i].ToString("x2"));
            }

            return adlerOutput.ToString();
        }

        /// <summary>
        /// Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash)
        {
            UInt16 localSum1, localSum2;
            UInt32 finalSum;

            localSum1 = 1;
            localSum2 = 0;

            for (int i = 0; i < len; i++)
            {
                localSum1 = (ushort)((localSum1 + data[i]) % AdlerModule);
                localSum2 = (ushort)((localSum2 + localSum1) % AdlerModule);
            }

            finalSum = (uint)((localSum2 << 16) | localSum1);

            hash = BitConverter.GetBytes(finalSum);

            StringBuilder adlerOutput = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                adlerOutput.Append(hash[i].ToString("x2"));
            }

            return adlerOutput.ToString();
        }

        /// <summary>
        /// Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, out byte[] hash)
        {
            return Data(data, (uint)data.Length, out hash);
        }
    }
}

