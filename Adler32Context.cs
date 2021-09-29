// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Adler32Context.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements an Adler-32 algorithm.
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

using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using Aaru.Checksums.Adler32;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Ssse3 = System.Runtime.Intrinsics.X86.Ssse3;

namespace Aaru.Checksums
{
    /// <inheritdoc />
    /// <summary>Implements the Adler-32 algorithm</summary>
    public sealed class Adler32Context : IChecksum
    {
        internal const ushort ADLER_MODULE = 65521;
        internal const uint   NMAX         = 5552;
        ushort                _sum1, _sum2;

        /// <summary>Initializes the Adler-32 sums</summary>
        public Adler32Context()
        {
            _sum1 = 1;
            _sum2 = 0;
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
            uint finalSum    = (uint)((_sum2 << 16) | _sum1);
            var  adlerOutput = new StringBuilder();

            for(int i = 0; i < BigEndianBitConverter.GetBytes(finalSum).Length; i++)
                adlerOutput.Append(BigEndianBitConverter.GetBytes(finalSum)[i].ToString("x2"));

            return adlerOutput.ToString();
        }

        static void Step(ref ushort preSum1, ref ushort preSum2, byte[] data, uint len)
        {
            if(Ssse3.IsSupported)
            {
                Adler32.Ssse3.Step(ref preSum1, ref preSum2, data, len);

                return;
            }

            if(AdvSimd.IsSupported)
            {
                neon.Step(ref preSum1, ref preSum2, data, len);

                return;
            }

            uint sum1 = preSum1;
            uint sum2 = preSum2;
            uint n;
            int  dataOff = 0;

            /* in case user likes doing a byte at a time, keep it fast */
            if(len == 1)
            {
                sum1 += data[dataOff];

                if(sum1 >= ADLER_MODULE)
                    sum1 -= ADLER_MODULE;

                sum2 += sum1;

                if(sum2 >= ADLER_MODULE)
                    sum2 -= ADLER_MODULE;

                preSum1 = (ushort)(sum1 & 0xFFFF);
                preSum2 = (ushort)(sum2 & 0xFFFF);

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

                if(sum1 >= ADLER_MODULE)
                    sum1 -= ADLER_MODULE;

                sum2    %= ADLER_MODULE; /* only added so many ADLER_MODULE's */
                preSum1 =  (ushort)(sum1 & 0xFFFF);
                preSum2 =  (ushort)(sum2 & 0xFFFF);

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

                sum1 %= ADLER_MODULE;
                sum2 %= ADLER_MODULE;
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

                sum1 %= ADLER_MODULE;
                sum2 %= ADLER_MODULE;
            }

            preSum1 = (ushort)(sum1 & 0xFFFF);
            preSum2 = (ushort)(sum2 & 0xFFFF);
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

            ushort localSum1 = 1;
            ushort localSum2 = 0;

            byte[] buffer = new byte[65536];
            int    read   = fileStream.Read(buffer, 0, 65536);

            while(read > 0)
            {
                Step(ref localSum1, ref localSum2, buffer, (uint)read);
                read = fileStream.Read(buffer, 0, 65536);
            }

            uint finalSum = (uint)((localSum2 << 16) | localSum1);

            hash = BigEndianBitConverter.GetBytes(finalSum);

            var adlerOutput = new StringBuilder();

            foreach(byte h in hash)
                adlerOutput.Append(h.ToString("x2"));

            fileStream.Close();

            return adlerOutput.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash)
        {
            ushort localSum1 = 1;
            ushort localSum2 = 0;

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
}