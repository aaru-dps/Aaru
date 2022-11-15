// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ssse3.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//                  The Chromium Authors
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
// Compute Adler32 checksum using SSSE3 vectorization.
//
// --[ License ] --------------------------------------------------------------
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//    * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
//    * Neither the name of Google Inc. nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// Copyright 2017 The Chromium Authors. All rights reserved.
// ****************************************************************************/

namespace Aaru.Checksums.Adler32;

using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

static class Ssse3
{
    internal static void Step(ref ushort sum1, ref ushort sum2, byte[] buf, uint len)
    {
        uint s1     = sum1;
        uint s2     = sum2;
        var  bufPos = 0;

        /*
         * Process the data in blocks.
         */
        const uint blockSize = 1 << 5;
        uint       blocks    = len / blockSize;
        len -= blocks * blockSize;

        while(blocks != 0)
        {
            uint n = Adler32Context.NMAX / blockSize; /* The NMAX constraint. */

            if(n > blocks)
                n = blocks;

            blocks -= n;

            Vector128<byte> tap1 = Vector128.Create(32, 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17).
                                             AsByte();

            Vector128<byte> tap2 = Vector128.Create(16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1).AsByte();
            Vector128<byte> zero = Vector128.Create(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0).AsByte();
            var             ones = Vector128.Create(1, 1, 1, 1, 1, 1, 1, 1);
            /*
             * Process n blocks of data. At most NMAX data bytes can be
             * processed before s2 must be reduced modulo BASE.
             */
            var vPs = Vector128.Create(s1 * n, 0, 0, 0);
            var vS2 = Vector128.Create(s2, 0, 0, 0);
            var vS1 = Vector128.Create(0u, 0, 0, 0);

            do
            {
                /*
                 * Load 32 input bytes.
                 */
                var bytes1 = Vector128.Create(BitConverter.ToUInt32(buf, bufPos),
                                              BitConverter.ToUInt32(buf, bufPos + 4),
                                              BitConverter.ToUInt32(buf, bufPos + 8),
                                              BitConverter.ToUInt32(buf, bufPos + 12));

                bufPos += 16;

                var bytes2 = Vector128.Create(BitConverter.ToUInt32(buf, bufPos),
                                              BitConverter.ToUInt32(buf, bufPos + 4),
                                              BitConverter.ToUInt32(buf, bufPos + 8),
                                              BitConverter.ToUInt32(buf, bufPos + 12));

                bufPos += 16;

                /*
                 * Add previous block byte sum to v_ps.
                 */
                vPs = Sse2.Add(vPs, vS1);
                /*
                 * Horizontally add the bytes for s1, multiply-adds the
                 * bytes by [ 32, 31, 30, ... ] for s2.
                 */
                vS1 = Sse2.Add(vS1, Sse2.SumAbsoluteDifferences(bytes1.AsByte(), zero).AsUInt32());

                Vector128<short> mad1 =
                    System.Runtime.Intrinsics.X86.Ssse3.MultiplyAddAdjacent(bytes1.AsByte(), tap1.AsSByte());

                vS2 = Sse2.Add(vS2, Sse2.MultiplyAddAdjacent(mad1.AsInt16(), ones.AsInt16()).AsUInt32());
                vS1 = Sse2.Add(vS1, Sse2.SumAbsoluteDifferences(bytes2.AsByte(), zero).AsUInt32());

                Vector128<short> mad2 =
                    System.Runtime.Intrinsics.X86.Ssse3.MultiplyAddAdjacent(bytes2.AsByte(), tap2.AsSByte());

                vS2 = Sse2.Add(vS2, Sse2.MultiplyAddAdjacent(mad2.AsInt16(), ones.AsInt16()).AsUInt32());
            } while(--n != 0);

            vS2 = Sse2.Add(vS2, Sse2.ShiftLeftLogical(vPs, 5));
            /*
             * Sum epi32 ints v_s1(s2) and accumulate in s1(s2).
             */
            vS1 =  Sse2.Add(vS1, Sse2.Shuffle(vS1, 177));
            vS1 =  Sse2.Add(vS1, Sse2.Shuffle(vS1, 78));
            s1  += (uint)Sse2.ConvertToInt32(vS1.AsInt32());
            vS2 =  Sse2.Add(vS2, Sse2.Shuffle(vS2, 177));
            vS2 =  Sse2.Add(vS2, Sse2.Shuffle(vS2, 78));
            s2  =  (uint)Sse2.ConvertToInt32(vS2.AsInt32());
            /*
             * Reduce.
             */
            s1 %= Adler32Context.ADLER_MODULE;
            s2 %= Adler32Context.ADLER_MODULE;
        }

        /*
         * Handle leftover data.
         */
        if(len != 0)
        {
            if(len >= 16)
            {
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                s2  += s1 += buf[bufPos++];
                len -= 16;
            }

            while(len-- != 0)
                s2 += s1 += buf[bufPos++];

            if(s1 >= Adler32Context.ADLER_MODULE)
                s1 -= Adler32Context.ADLER_MODULE;

            s2 %= Adler32Context.ADLER_MODULE;
        }

        /*
         * Return the recombined sums.
         */
        sum1 = (ushort)(s1 & 0xFFFF);
        sum2 = (ushort)(s2 & 0xFFFF);
    }
}