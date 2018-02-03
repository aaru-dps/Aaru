// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FletcherContext.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements Fletcher-32 and Fletcher-16 algorithms.
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

// Disabled because the speed is abnormally slow

using System;
using System.IO;
using System.Text;

namespace DiscImageChef.Checksums
{
    public class Fletcher32Context : IChecksum
    {
        bool   inodd;
        byte   oddValue;
        ushort sum1, sum2;

        /// <summary>
        ///     Initializes the Fletcher32 sums
        /// </summary>
        public Fletcher32Context()
        {
            sum1     = 0xFFFF;
            sum2     = 0xFFFF;
            oddValue = 0;
            inodd    = false;
        }

        /// <summary>
        ///     Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            ushort block;
            if(!inodd)
                if(len % 2 != 0)
                {
                    oddValue = data[len - 1];
                    inodd    = true;

                    for(int i = 0; i < len - 1; i += 2)
                    {
                        block = BigEndianBitConverter.ToUInt16(data, i);
                        sum1  = (ushort)((sum1 + block) % 0xFFFF);
                        sum2  = (ushort)((sum2 + sum1)  % 0xFFFF);
                    }
                }
                else
                {
                    inodd = false;
                    for(int i = 0; i < len; i += 2)
                    {
                        block = BigEndianBitConverter.ToUInt16(data, i);
                        sum1  = (ushort)((sum1 + block) % 0xFFFF);
                        sum2  = (ushort)((sum2 + sum1)  % 0xFFFF);
                    }
                }
            // Carrying odd
            else
            {
                byte[] oddData = new byte[2];
                oddData[0]     = oddValue;
                oddData[1]     = data[0];

                block = BigEndianBitConverter.ToUInt16(oddData, 0);
                sum1  = (ushort)((sum1 + block) % 0xFFFF);
                sum2  = (ushort)((sum2 + sum1)  % 0xFFFF);

                // Even size, carrying odd
                if(len % 2 == 0)
                {
                    oddValue = data[len - 1];
                    inodd    = true;

                    for(int i = 1; i < len - 1; i += 2)
                    {
                        block = BigEndianBitConverter.ToUInt16(data, i);
                        sum1  = (ushort)((sum1 + block) % 0xFFFF);
                        sum2  = (ushort)((sum2 + sum1)  % 0xFFFF);
                    }
                }
                else
                {
                    inodd = false;
                    for(int i = 1; i < len; i += 2)
                    {
                        block = BigEndianBitConverter.ToUInt16(data, i);
                        sum1  = (ushort)((sum1 + block) % 0xFFFF);
                        sum2  = (ushort)((sum2 + sum1)  % 0xFFFF);
                    }
                }
            }
        }

        /// <summary>
        ///     Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data)
        {
            Update(data, (uint)data.Length);
        }

        /// <summary>
        ///     Returns a byte array of the hash value.
        /// </summary>
        public byte[] Final()
        {
            uint finalSum = (uint)(sum1 + (sum2 << 16));
            return BigEndianBitConverter.GetBytes(finalSum);
        }

        /// <summary>
        ///     Returns a hexadecimal representation of the hash value.
        /// </summary>
        public string End()
        {
            uint          finalSum       = (uint)(sum1 + (sum2 << 16));
            StringBuilder fletcherOutput = new StringBuilder();

            for(int i = 0; i < BigEndianBitConverter.GetBytes(finalSum).Length; i++)
                fletcherOutput.Append(BigEndianBitConverter.GetBytes(finalSum)[i].ToString("x2"));

            return fletcherOutput.ToString();
        }

        /// <summary>
        ///     Gets the hash of a file
        /// </summary>
        /// <param name="filename">File path.</param>
        public static byte[] File(string filename)
        {
            File(filename, out byte[] hash);
            return hash;
        }

        /// <summary>
        ///     Gets the hash of a file in hexadecimal and as a byte array.
        /// </summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string File(string filename, out byte[] hash)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            ushort     localSum1, localSum2, block;
            uint       finalSum;
            byte[]     blockBytes;

            localSum1 = 0xFFFF;
            localSum2 = 0xFFFF;

            if(fileStream.Length % 2 == 0)
                for(int i = 0; i     < fileStream.Length; i += 2)
                {
                    blockBytes = new byte[2];
                    fileStream.Read(blockBytes, 0, 2);
                    block     = BigEndianBitConverter.ToUInt16(blockBytes, 0);
                    localSum1 = (ushort)((localSum1 + block)     % 0xFFFF);
                    localSum2 = (ushort)((localSum2 + localSum1) % 0xFFFF);
                }
            else
            {
                for(int i = 0; i < fileStream.Length - 1; i += 2)
                {
                    blockBytes = new byte[2];
                    fileStream.Read(blockBytes, 0, 2);
                    block     = BigEndianBitConverter.ToUInt16(blockBytes, 0);
                    localSum1 = (ushort)((localSum1 + block)     % 0xFFFF);
                    localSum2 = (ushort)((localSum2 + localSum1) % 0xFFFF);
                }

                byte[] oddData = new byte[2];
                oddData[0]     = (byte)fileStream.ReadByte();
                oddData[1]     = 0;

                block     = BigEndianBitConverter.ToUInt16(oddData, 0);
                localSum1 = (ushort)((localSum1 + block)     % 0xFFFF);
                localSum2 = (ushort)((localSum2 + localSum1) % 0xFFFF);
            }

            finalSum = (uint)(localSum1 + (localSum2 << 16));

            hash = BitConverter.GetBytes(finalSum);

            StringBuilder fletcherOutput = new StringBuilder();

            foreach(byte h in hash) fletcherOutput.Append(h.ToString("x2"));

            return fletcherOutput.ToString();
        }

        /// <summary>
        ///     Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash)
        {
            ushort localSum1, localSum2, block;
            uint   finalSum;

            localSum1 = 0xFFFF;
            localSum2 = 0xFFFF;

            if(len % 2           == 0)
                for(int i = 0; i < len; i += 2)
                {
                    block     = BigEndianBitConverter.ToUInt16(data, i);
                    localSum1 = (ushort)((localSum1 + block)     % 0xFFFF);
                    localSum2 = (ushort)((localSum2 + localSum1) % 0xFFFF);
                }
            else
            {
                for(int i = 0; i < len - 1; i += 2)
                {
                    block     = BigEndianBitConverter.ToUInt16(data, i);
                    localSum1 = (ushort)((localSum1 + block)     % 0xFFFF);
                    localSum2 = (ushort)((localSum2 + localSum1) % 0xFFFF);
                }

                byte[] oddData = new byte[2];
                oddData[0]     = data[len - 1];
                oddData[1]     = 0;

                block     = BigEndianBitConverter.ToUInt16(oddData, 0);
                localSum1 = (ushort)((localSum1 + block)     % 0xFFFF);
                localSum2 = (ushort)((localSum2 + localSum1) % 0xFFFF);
            }

            finalSum = (uint)(localSum1 + (localSum2 << 16));

            hash = BitConverter.GetBytes(finalSum);

            StringBuilder fletcherOutput = new StringBuilder();

            foreach(byte h in hash) fletcherOutput.Append(h.ToString("x2"));

            return fletcherOutput.ToString();
        }

        /// <summary>
        ///     Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, out byte[] hash)
        {
            return Data(data, (uint)data.Length, out hash);
        }
    }

    public class Fletcher16Context
    {
        byte sum1, sum2;

        /// <summary>
        ///     Initializes the Fletcher16 sums
        /// </summary>
        public void Init()
        {
            sum1 = 0xFF;
            sum2 = 0xFF;
        }

        /// <summary>
        ///     Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            for(int i = 0; i < len; i++)
            {
                sum1 = (byte)((sum1 + data[i]) % 0xFF);
                sum2 = (byte)((sum2 + sum1)    % 0xFF);
            }
        }

        /// <summary>
        ///     Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data)
        {
            Update(data, (uint)data.Length);
        }

        /// <summary>
        ///     Returns a byte array of the hash value.
        /// </summary>
        public byte[] Final()
        {
            ushort finalSum = (ushort)(sum1 + (sum2 << 8));
            return BigEndianBitConverter.GetBytes(finalSum);
        }

        /// <summary>
        ///     Returns a hexadecimal representation of the hash value.
        /// </summary>
        public string End()
        {
            ushort        finalSum       = (ushort)(sum1 + (sum2 << 8));
            StringBuilder fletcherOutput = new StringBuilder();

            for(int i = 0; i < BigEndianBitConverter.GetBytes(finalSum).Length; i++)
                fletcherOutput.Append(BigEndianBitConverter.GetBytes(finalSum)[i].ToString("x2"));

            return fletcherOutput.ToString();
        }

        /// <summary>
        ///     Gets the hash of a file
        /// </summary>
        /// <param name="filename">File path.</param>
        public static byte[] File(string filename)
        {
            File(filename, out byte[] hash);
            return hash;
        }

        /// <summary>
        ///     Gets the hash of a file in hexadecimal and as a byte array.
        /// </summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string File(string filename, out byte[] hash)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            byte       localSum1, localSum2, block;
            ushort     finalSum;

            localSum1 = 0xFF;
            localSum2 = 0xFF;

            for(int i = 0; i < fileStream.Length; i += 2)
            {
                block     = (byte)fileStream.ReadByte();
                localSum1 = (byte)((localSum1 + block)     % 0xFF);
                localSum2 = (byte)((localSum2 + localSum1) % 0xFF);
            }

            finalSum = (ushort)(localSum1 + (localSum2 << 8));

            hash = BitConverter.GetBytes(finalSum);

            StringBuilder fletcherOutput = new StringBuilder();

            foreach(byte h in hash) fletcherOutput.Append(h.ToString("x2"));

            return fletcherOutput.ToString();
        }

        /// <summary>
        ///     Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash)
        {
            byte   localSum1, localSum2;
            ushort finalSum;

            localSum1 = 0xFF;
            localSum2 = 0xFF;

            for(int i = 0; i < len; i++)
            {
                localSum1 = (byte)((localSum1 + data[i])   % 0xFF);
                localSum2 = (byte)((localSum2 + localSum1) % 0xFF);
            }

            finalSum = (ushort)(localSum1 + (localSum2 << 8));

            hash = BitConverter.GetBytes(finalSum);

            StringBuilder fletcherOutput = new StringBuilder();

            foreach(byte h in hash) fletcherOutput.Append(h.ToString("x2"));

            return fletcherOutput.ToString();
        }

        /// <summary>
        ///     Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, out byte[] hash)
        {
            return Data(data, (uint)data.Length, out hash);
        }
    }
}