// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : neon.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//                  The Chromium Authors
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
// Compute Fletcher32 checksum using NEON vectorization.
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
// Copyright © 2011-2024 Natalia Portillo
// Copyright 2017 The Chromium Authors. All rights reserved.
// ****************************************************************************/

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

namespace Aaru.Checksums.Fletcher32;

static class Neon
{
    internal static void Step(ref ushort preSum1, ref ushort preSum2, byte[] buf, uint len)
    {
        /*
         * Split Fletcher-32 into component sums.
         */
        uint s1 = preSum1;
        uint s2 = preSum2;

        var bufPos = 0;

        /*
         * Process the data in blocks.
         */
        const uint block_Size = 1 << 5;
        uint       blocks     = len / block_Size;
        len -= blocks * block_Size;

        while(blocks != 0)
        {
            uint n = Fletcher32Context.Nmax / block_Size; /* The NMAX constraint. */

            if(n > blocks) n = blocks;

            blocks -= n;
            /*
             * Process n blocks of data. At most NMAX data bytes can be
             * processed before s2 must be reduced modulo FLETCHER_MODULE.
             */
            var               v_S2           = Vector128.Create(s1 * n, 0, 0, 0);
            var               v_S1           = Vector128.Create(0u,     0, 0, 0);
            Vector128<ushort> v_Column_Sum_1 = AdvSimd.DuplicateToVector128((ushort)0);
            Vector128<ushort> v_Column_Sum_2 = AdvSimd.DuplicateToVector128((ushort)0);
            Vector128<ushort> v_Column_Sum_3 = AdvSimd.DuplicateToVector128((ushort)0);
            Vector128<ushort> v_Column_Sum_4 = AdvSimd.DuplicateToVector128((ushort)0);

            do
            {
                /*
                 * Load 32 input bytes.
                 */
                var bytes1 = Vector128.Create(buf[bufPos],
                                              buf[bufPos + 1],
                                              buf[bufPos + 2],
                                              buf[bufPos + 3],
                                              buf[bufPos + 4],
                                              buf[bufPos + 5],
                                              buf[bufPos + 6],
                                              buf[bufPos + 7],
                                              buf[bufPos + 8],
                                              buf[bufPos + 9],
                                              buf[bufPos + 10],
                                              buf[bufPos + 11],
                                              buf[bufPos + 12],
                                              buf[bufPos + 13],
                                              buf[bufPos + 14],
                                              buf[bufPos + 15]);

                bufPos += 16;

                var bytes2 = Vector128.Create(buf[bufPos],
                                              buf[bufPos + 1],
                                              buf[bufPos + 2],
                                              buf[bufPos + 3],
                                              buf[bufPos + 4],
                                              buf[bufPos + 5],
                                              buf[bufPos + 6],
                                              buf[bufPos + 7],
                                              buf[bufPos + 8],
                                              buf[bufPos + 9],
                                              buf[bufPos + 10],
                                              buf[bufPos + 11],
                                              buf[bufPos + 12],
                                              buf[bufPos + 13],
                                              buf[bufPos + 14],
                                              buf[bufPos + 15]);

                bufPos += 16;
                /*
                 * Add previous block byte sum to v_s2.
                 */
                v_S2 = AdvSimd.Add(v_S2, v_S1);

                /*
                 * Horizontally add the bytes for s1.
                 */
                v_S1 = AdvSimd.AddPairwiseWideningAndAdd(v_S1,
                                                         AdvSimd
                                                            .AddPairwiseWideningAndAdd(AdvSimd
                                                                    .AddPairwiseWidening(bytes1),
                                                                 bytes2));

                /*
                 * Vertically add the bytes for s2.
                 */
                v_Column_Sum_1 = AdvSimd.AddWideningLower(v_Column_Sum_1, bytes1.GetLower());
                v_Column_Sum_2 = AdvSimd.AddWideningLower(v_Column_Sum_2, bytes1.GetUpper());
                v_Column_Sum_3 = AdvSimd.AddWideningLower(v_Column_Sum_3, bytes2.GetLower());
                v_Column_Sum_4 = AdvSimd.AddWideningLower(v_Column_Sum_4, bytes2.GetUpper());
            } while(--n != 0);

            v_S2 = AdvSimd.ShiftLeftLogical(v_S2, 5);

            /*
             * Multiply-add bytes by [ 32, 31, 30, ... ] for s2.
             */
            v_S2 = AdvSimd.MultiplyWideningLowerAndAdd(v_S2,
                                                       v_Column_Sum_1.GetLower(),
                                                       Vector64.Create((ushort)32, 31, 30, 29));

            v_S2 = AdvSimd.MultiplyWideningLowerAndAdd(v_S2,
                                                       v_Column_Sum_1.GetUpper(),
                                                       Vector64.Create((ushort)28, 27, 26, 25));

            v_S2 = AdvSimd.MultiplyWideningLowerAndAdd(v_S2,
                                                       v_Column_Sum_2.GetLower(),
                                                       Vector64.Create((ushort)24, 23, 22, 21));

            v_S2 = AdvSimd.MultiplyWideningLowerAndAdd(v_S2,
                                                       v_Column_Sum_2.GetUpper(),
                                                       Vector64.Create((ushort)20, 19, 18, 17));

            v_S2 = AdvSimd.MultiplyWideningLowerAndAdd(v_S2,
                                                       v_Column_Sum_3.GetLower(),
                                                       Vector64.Create((ushort)16, 15, 14, 13));

            v_S2 = AdvSimd.MultiplyWideningLowerAndAdd(v_S2,
                                                       v_Column_Sum_3.GetUpper(),
                                                       Vector64.Create((ushort)12, 11, 10, 9));

            v_S2 = AdvSimd.MultiplyWideningLowerAndAdd(v_S2,
                                                       v_Column_Sum_4.GetLower(),
                                                       Vector64.Create((ushort)8, 7, 6, 5));

            v_S2 = AdvSimd.MultiplyWideningLowerAndAdd(v_S2,
                                                       v_Column_Sum_4.GetUpper(),
                                                       Vector64.Create((ushort)4, 3, 2, 1));

            /*
             * Sum epi32 ints v_s1(s2) and accumulate in s1(s2).
             */
            Vector64<uint> sum1 = AdvSimd.AddPairwise(v_S1.GetLower(), v_S1.GetUpper());
            Vector64<uint> sum2 = AdvSimd.AddPairwise(v_S2.GetLower(), v_S2.GetUpper());
            Vector64<uint> s1S2 = AdvSimd.AddPairwise(sum1,            sum2);
            s1 += AdvSimd.Extract(s1S2, 0);
            s2 += AdvSimd.Extract(s1S2, 1);
            /*
             * Reduce.
             */
            s1 %= Fletcher32Context.FletcherModule;
            s2 %= Fletcher32Context.FletcherModule;
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

            while(len-- != 0) s2 += s1 += buf[bufPos++];

            if(s1 >= Fletcher32Context.FletcherModule) s1 -= Fletcher32Context.FletcherModule;

            s2 %= Fletcher32Context.FletcherModule;
        }

        /*
         * Return the recombined sums.
         */
        preSum1 = (ushort)(s1 & 0xFFFF);
        preSum2 = (ushort)(s2 & 0xFFFF);
    }
}