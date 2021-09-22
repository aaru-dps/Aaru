// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

// Disabled because the speed is abnormally slow

using System.IO;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Checksums
{
    /// <summary>Implements the Fletcher-32 algorithm</summary>
    public sealed class Fletcher32Context : IChecksum
    {
        const ushort FLETCHER_MODULE = 0xFFFF;
        const uint   NMAX            = 5552;
        ushort       _sum1, _sum2;

        /// <summary>Initializes the Fletcher-32 sums</summary>
        public Fletcher32Context()
        {
            _sum1 = 0xFFFF;
            _sum2 = 0xFFFF;
        }

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len) => Step(ref _sum1, ref _sum2, data, len);

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data) => Update(data, (uint)data.Length);

        /// <inheritdoc />
        /// <summary>Returns a byte array of the hash value.</summary>
        public byte[] Final()
        {
            uint finalSum = (uint)((_sum2 << 16) | _sum1);

            return BigEndianBitConverter.GetBytes(finalSum);
        }

        /// <inheritdoc />
        /// <summary>Returns a hexadecimal representation of the hash value.</summary>
        public string End()
        {
            uint finalSum       = (uint)((_sum2 << 16) | _sum1);
            var  fletcherOutput = new StringBuilder();

            for(int i = 0; i < BigEndianBitConverter.GetBytes(finalSum).Length; i++)
                fletcherOutput.Append(BigEndianBitConverter.GetBytes(finalSum)[i].ToString("x2"));

            return fletcherOutput.ToString();
        }

        static void Step(ref ushort previousSum1, ref ushort previousSum2, byte[] data, uint len)
        {
            uint sum1 = previousSum1;
            uint sum2 = previousSum2;
            uint n;
            int  dataOff = 0;

            /* in case user likes doing a byte at a time, keep it fast */
            if(len == 1)
            {
                sum1 += data[dataOff];

                if(sum1 >= FLETCHER_MODULE)
                    sum1 -= FLETCHER_MODULE;

                sum2 += sum1;

                if(sum2 >= FLETCHER_MODULE)
                    sum2 -= FLETCHER_MODULE;

                previousSum1 = (ushort)(sum1 & 0xFFFF);
                previousSum2 = (ushort)(sum2 & 0xFFFF);

                return;
            }

            /* in case short lengths are provided, keep it somewhat fast */
            if(len < 16)
            {
                while(len-- > 0)
                {
                    sum1 += data[dataOff++];
                    sum2 += sum1;
                }

                if(sum1 >= FLETCHER_MODULE)
                    sum1 -= FLETCHER_MODULE;

                sum2         %= FLETCHER_MODULE; /* only added so many FLETCHER_MODULE's */
                previousSum1 =  (ushort)(sum1 & 0xFFFF);
                previousSum2 =  (ushort)(sum2 & 0xFFFF);

                return;
            }

            /* do length NMAX blocks -- requires just one modulo operation */
            while(len >= NMAX)
            {
                len -= NMAX;
                n   =  NMAX / 16; /* NMAX is divisible by 16 */

                do
                {
                    sum1 += data[dataOff];
                    sum2 += sum1;
                    sum1 += data[dataOff + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 2];
                    sum2 += sum1;
                    sum1 += data[dataOff + 2 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4 + 2];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4 + 2 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 2];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 2 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 4];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 4 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 4 + 2];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 4 + 2 + 1];
                    sum2 += sum1;

                    /* 16 sums unrolled */
                    dataOff += 16;
                } while(--n != 0);

                sum1 %= FLETCHER_MODULE;
                sum2 %= FLETCHER_MODULE;
            }

            /* do remaining bytes (less than NMAX, still just one modulo) */
            if(len != 0)
            {
                /* avoid modulos if none remaining */
                while(len >= 16)
                {
                    len  -= 16;
                    sum1 += data[dataOff];
                    sum2 += sum1;
                    sum1 += data[dataOff + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 2];
                    sum2 += sum1;
                    sum1 += data[dataOff + 2 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4 + 2];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4 + 2 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 2];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 2 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 4];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 4 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 4 + 2];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 4 + 2 + 1];
                    sum2 += sum1;

                    dataOff += 16;
                }

                while(len-- != 0)
                {
                    sum1 += data[dataOff++];
                    sum2 += sum1;
                }

                sum1 %= FLETCHER_MODULE;
                sum2 %= FLETCHER_MODULE;
            }

            previousSum1 = (ushort)(sum1 & 0xFFFF);
            previousSum2 = (ushort)(sum2 & 0xFFFF);
        }

        /// <summary>Gets the hash of a file</summary>
        /// <param name="filename">File path.</param>
        public static byte[] File(string filename)
        {
            File(filename, out byte[] hash);

            return hash;
        }

        /// <summary>Gets the hash of a file in hexadecimal and as a byte array.</summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string File(string filename, out byte[] hash)
        {
            var fileStream = new FileStream(filename, FileMode.Open);

            ushort localSum1 = 0xFFFF;
            ushort localSum2 = 0xFFFF;

            byte[] buffer = new byte[65536];
            int    read   = fileStream.Read(buffer, 0, 65536);

            while(read > 0)
            {
                Step(ref localSum1, ref localSum2, buffer, (uint)read);

                read = fileStream.Read(buffer, 0, 65536);
            }

            uint finalSum = (uint)((localSum2 << 16) | localSum1);

            hash = BigEndianBitConverter.GetBytes(finalSum);

            var fletcherOutput = new StringBuilder();

            foreach(byte h in hash)
                fletcherOutput.Append(h.ToString("x2"));

            fileStream.Close();

            return fletcherOutput.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash)
        {
            ushort localSum1 = 0xFFFF;
            ushort localSum2 = 0xFFFF;

            Step(ref localSum1, ref localSum2, data, len);

            uint finalSum = (uint)((localSum2 << 16) | localSum1);

            hash = BigEndianBitConverter.GetBytes(finalSum);

            var adlerOutput = new StringBuilder();

            foreach(byte h in hash)
                adlerOutput.Append(h.ToString("x2"));

            return adlerOutput.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, out byte[] hash) => Data(data, (uint)data.Length, out hash);
    }

    /// <inheritdoc />
    /// <summary>Implements the Fletcher-16 algorithm</summary>
    public sealed class Fletcher16Context : IChecksum
    {
        const byte FLETCHER_MODULE = 0xFF;
        const byte NMAX            = 22;
        byte       _sum1, _sum2;

        /// <summary>Initializes the Fletcher-16 sums</summary>
        public Fletcher16Context()
        {
            _sum1 = 0xFF;
            _sum2 = 0xFF;
        }

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len) => Step(ref _sum1, ref _sum2, data, len);

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data) => Update(data, (uint)data.Length);

        /// <inheritdoc />
        /// <summary>Returns a byte array of the hash value.</summary>
        public byte[] Final()
        {
            ushort finalSum = (ushort)((_sum2 << 8) | _sum1);

            return BigEndianBitConverter.GetBytes(finalSum);
        }

        /// <inheritdoc />
        /// <summary>Returns a hexadecimal representation of the hash value.</summary>
        public string End()
        {
            ushort finalSum       = (ushort)((_sum2 << 8) | _sum1);
            var    fletcherOutput = new StringBuilder();

            for(int i = 0; i < BigEndianBitConverter.GetBytes(finalSum).Length; i++)
                fletcherOutput.Append(BigEndianBitConverter.GetBytes(finalSum)[i].ToString("x2"));

            return fletcherOutput.ToString();
        }

        static void Step(ref byte previousSum1, ref byte previousSum2, byte[] data, uint len)
        {
            uint sum1 = previousSum1;
            uint sum2 = previousSum2;
            uint n;
            int  dataOff = 0;

            /* in case user likes doing a byte at a time, keep it fast */
            if(len == 1)
            {
                sum1 += data[dataOff];

                if(sum1 >= FLETCHER_MODULE)
                    sum1 -= FLETCHER_MODULE;

                sum2 += sum1;

                if(sum2 >= FLETCHER_MODULE)
                    sum2 -= FLETCHER_MODULE;

                previousSum1 = (byte)(sum1 & 0xFF);
                previousSum2 = (byte)(sum2 & 0xFF);

                return;
            }

            /* in case short lengths are provided, keep it somewhat fast */
            if(len < 11)
            {
                while(len-- > 0)
                {
                    sum1 += data[dataOff++];
                    sum2 += sum1;
                }

                if(sum1 >= FLETCHER_MODULE)
                    sum1 -= FLETCHER_MODULE;

                sum2         %= FLETCHER_MODULE; /* only added so many FLETCHER_MODULE's */
                previousSum1 =  (byte)(sum1 & 0xFF);
                previousSum2 =  (byte)(sum2 & 0xFF);

                return;
            }

            /* do length NMAX blocks -- requires just one modulo operation */
            while(len >= NMAX)
            {
                len -= NMAX;
                n   =  NMAX / 11; /* NMAX is divisible by 16 */

                do
                {
                    sum1 += data[dataOff];
                    sum2 += sum1;
                    sum1 += data[dataOff + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 2];
                    sum2 += sum1;
                    sum1 += data[dataOff + 2 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4 + 2];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4 + 2 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 2];
                    sum2 += sum1;

                    /* 11 sums unrolled */
                    dataOff += 11;
                } while(--n != 0);

                sum1 %= FLETCHER_MODULE;
                sum2 %= FLETCHER_MODULE;
            }

            /* do remaining bytes (less than NMAX, still just one modulo) */
            if(len != 0)
            {
                /* avoid modulos if none remaining */
                while(len >= 11)
                {
                    len  -= 11;
                    sum1 += data[dataOff];
                    sum2 += sum1;
                    sum1 += data[dataOff + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 2];
                    sum2 += sum1;
                    sum1 += data[dataOff + 2 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4 + 2];
                    sum2 += sum1;
                    sum1 += data[dataOff + 4 + 2 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 1];
                    sum2 += sum1;
                    sum1 += data[dataOff + 8 + 2];
                    sum2 += sum1;

                    dataOff += 11;
                }

                while(len-- != 0)
                {
                    sum1 += data[dataOff++];
                    sum2 += sum1;
                }

                sum1 %= FLETCHER_MODULE;
                sum2 %= FLETCHER_MODULE;
            }

            previousSum1 = (byte)(sum1 & 0xFF);
            previousSum2 = (byte)(sum2 & 0xFF);
        }

        /// <summary>Gets the hash of a file</summary>
        /// <param name="filename">File path.</param>
        public static byte[] File(string filename)
        {
            File(filename, out byte[] hash);

            return hash;
        }

        /// <summary>Gets the hash of a file in hexadecimal and as a byte array.</summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string File(string filename, out byte[] hash)
        {
            var fileStream = new FileStream(filename, FileMode.Open);

            byte localSum1 = 0xFF;
            byte localSum2 = 0xFF;

            byte[] buffer = new byte[65536];
            int    read   = fileStream.Read(buffer, 0, 65536);

            while(read > 0)
            {
                Step(ref localSum1, ref localSum2, buffer, (uint)read);

                read = fileStream.Read(buffer, 0, 65536);
            }

            ushort finalSum = (ushort)((localSum2 << 8) | localSum1);

            hash = BigEndianBitConverter.GetBytes(finalSum);

            var fletcherOutput = new StringBuilder();

            foreach(byte h in hash)
                fletcherOutput.Append(h.ToString("x2"));

            fileStream.Close();

            return fletcherOutput.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash)
        {
            byte localSum1 = 0xFF;
            byte localSum2 = 0xFF;

            Step(ref localSum1, ref localSum2, data, len);

            ushort finalSum = (ushort)((localSum2 << 8) | localSum1);

            hash = BigEndianBitConverter.GetBytes(finalSum);

            var adlerOutput = new StringBuilder();

            foreach(byte h in hash)
                adlerOutput.Append(h.ToString("x2"));

            return adlerOutput.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, out byte[] hash) => Data(data, (uint)data.Length, out hash);
    }
}