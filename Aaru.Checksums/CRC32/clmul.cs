// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : clmul.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//                  Wajdi Feghali    <wajdi.k.feghali@intel.com>
//                  Jim Guilford     <james.guilford@intel.com>
//                  Vinodh Gopal     <vinodh.gopal@intel.com>
//                  Erdinc Ozturk    <erdinc.ozturk@intel.com>
//                  Jim Kukunas      <james.t.kukunas@linux.intel.com>
//                  Marian Beermann
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
// Compute the CRC32 using a parallelized folding approach with the PCLMULQDQ
// instruction.
//
// A white paper describing this algorithm can be found at:
// http://www.intel.com/content/dam/www/public/us/en/documents/white-papers/fast-crc-computation-generic-polynomials-pclmulqdq-paper.pdf
//
// --[ License ] --------------------------------------------------------------
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the authors be held liable for any damages arising from
// the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
//   1. The origin of this software must not be misrepresented; you must not
//      claim that you wrote the original software. If you use this software
//      in a product, an acknowledgment in the product documentation would be
//      appreciated but is not required.
//
//   2. Altered source versions must be plainly marked as such, and must not be
//      misrepresented as being the original software.
//
//   3. This notice may not be removed or altered from any source distribution.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2024 Natalia Portillo
// Copyright (c) 2016 Marian Beermann (add support for initial value, restructuring)
// Copyright (C) 2013 Intel Corporation. All rights reserved.
// ****************************************************************************/

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Aaru.Checksums.CRC32;

static class Clmul
{
    static readonly uint[] _crcK =
    {
        0xccaa009e, 0x00000000, /* rk1 */ 0x751997d0, 0x00000001, /* rk2 */ 0xccaa009e, 0x00000000, /* rk5 */
        0x63cd6124, 0x00000001, /* rk6 */ 0xf7011640, 0x00000001, /* rk7 */ 0xdb710640, 0x00000001  /* rk8 */
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void Fold4(ref Vector128<uint> xmmCRC0, ref Vector128<uint> xmmCRC1, ref Vector128<uint> xmmCRC2,
                      ref Vector128<uint> xmmCRC3)
    {
        var xmmFold4 = Vector128.Create(0xc6e41596, 0x00000001, 0x54442bd4, 0x00000001);

        Vector128<uint> xTmp0 = xmmCRC0;
        Vector128<uint> xTmp1 = xmmCRC1;
        Vector128<uint> xTmp2 = xmmCRC2;
        Vector128<uint> xTmp3 = xmmCRC3;

        xmmCRC0 = Pclmulqdq.CarrylessMultiply(xmmCRC0.AsUInt64(), xmmFold4.AsUInt64(), 0x01).AsUInt32();
        xTmp0   = Pclmulqdq.CarrylessMultiply(xTmp0.AsUInt64(),   xmmFold4.AsUInt64(), 0x10).AsUInt32();
        Vector128<float> psCRC0 = xmmCRC0.AsSingle();
        Vector128<float> psT0   = xTmp0.AsSingle();
        Vector128<float> psRes0 = Sse.Xor(psCRC0, psT0);

        xmmCRC1 = Pclmulqdq.CarrylessMultiply(xmmCRC1.AsUInt64(), xmmFold4.AsUInt64(), 0x01).AsUInt32();
        xTmp1   = Pclmulqdq.CarrylessMultiply(xTmp1.AsUInt64(),   xmmFold4.AsUInt64(), 0x10).AsUInt32();
        Vector128<float> psCRC1 = xmmCRC1.AsSingle();
        Vector128<float> psT1   = xTmp1.AsSingle();
        Vector128<float> psRes1 = Sse.Xor(psCRC1, psT1);

        xmmCRC2 = Pclmulqdq.CarrylessMultiply(xmmCRC2.AsUInt64(), xmmFold4.AsUInt64(), 0x01).AsUInt32();
        xTmp2   = Pclmulqdq.CarrylessMultiply(xTmp2.AsUInt64(),   xmmFold4.AsUInt64(), 0x10).AsUInt32();
        Vector128<float> psCRC2 = xmmCRC2.AsSingle();
        Vector128<float> psT2   = xTmp2.AsSingle();
        Vector128<float> psRes2 = Sse.Xor(psCRC2, psT2);

        xmmCRC3 = Pclmulqdq.CarrylessMultiply(xmmCRC3.AsUInt64(), xmmFold4.AsUInt64(), 0x01).AsUInt32();
        xTmp3   = Pclmulqdq.CarrylessMultiply(xTmp3.AsUInt64(),   xmmFold4.AsUInt64(), 0x10).AsUInt32();
        Vector128<float> psCRC3 = xmmCRC3.AsSingle();
        Vector128<float> psT3   = xTmp3.AsSingle();
        Vector128<float> psRes3 = Sse.Xor(psCRC3, psT3);

        xmmCRC0 = psRes0.AsUInt32();
        xmmCRC1 = psRes1.AsUInt32();
        xmmCRC2 = psRes2.AsUInt32();
        xmmCRC3 = psRes3.AsUInt32();
    }

    internal static uint Step(byte[] src, long len, uint initialCRC)
    {
        Vector128<uint> xmmInitial = Sse2.ConvertScalarToVector128UInt32(initialCRC);
        Vector128<uint> xmmCRC0    = Sse2.ConvertScalarToVector128UInt32(0x9db42487);
        Vector128<uint> xmmCRC1    = Vector128<uint>.Zero;
        Vector128<uint> xmmCRC2    = Vector128<uint>.Zero;
        Vector128<uint> xmmCRC3    = Vector128<uint>.Zero;
        var             bufPos     = 0;

        var first = true;

        /* fold 512 to 32 step variable declarations for ISO-C90 compat. */
        var xmmMask  = Vector128.Create(0xFFFFFFFF, 0xFFFFFFFF, 0x00000000, 0x00000000);
        var xmmMask2 = Vector128.Create(0x00000000, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF);

        while((len -= 64) >= 0)
        {
            var xmmT0 = Vector128.Create(BitConverter.ToUInt32(src, bufPos),
                                         BitConverter.ToUInt32(src, bufPos + 4),
                                         BitConverter.ToUInt32(src, bufPos + 8),
                                         BitConverter.ToUInt32(src, bufPos + 12));

            bufPos += 16;

            var xmmT1 = Vector128.Create(BitConverter.ToUInt32(src, bufPos),
                                         BitConverter.ToUInt32(src, bufPos + 4),
                                         BitConverter.ToUInt32(src, bufPos + 8),
                                         BitConverter.ToUInt32(src, bufPos + 12));

            bufPos += 16;

            var xmmT2 = Vector128.Create(BitConverter.ToUInt32(src, bufPos),
                                         BitConverter.ToUInt32(src, bufPos + 4),
                                         BitConverter.ToUInt32(src, bufPos + 8),
                                         BitConverter.ToUInt32(src, bufPos + 12));

            bufPos += 16;

            var xmmT3 = Vector128.Create(BitConverter.ToUInt32(src, bufPos),
                                         BitConverter.ToUInt32(src, bufPos + 4),
                                         BitConverter.ToUInt32(src, bufPos + 8),
                                         BitConverter.ToUInt32(src, bufPos + 12));

            bufPos += 16;

            if(first)
            {
                first = false;
                xmmT0 = Sse2.Xor(xmmT0, xmmInitial);
            }

            Fold4(ref xmmCRC0, ref xmmCRC1, ref xmmCRC2, ref xmmCRC3);

            xmmCRC0 = Sse2.Xor(xmmCRC0, xmmT0);
            xmmCRC1 = Sse2.Xor(xmmCRC1, xmmT1);
            xmmCRC2 = Sse2.Xor(xmmCRC2, xmmT2);
            xmmCRC3 = Sse2.Xor(xmmCRC3, xmmT3);
        }

        /* fold 512 to 32 */

        /*
         * k1
         */
        var crcFold = Vector128.Create(_crcK[0], _crcK[1], _crcK[2], _crcK[3]);

        Vector128<uint> xTmp0 = Pclmulqdq.CarrylessMultiply(xmmCRC0.AsUInt64(), crcFold.AsUInt64(), 0x10).AsUInt32();

        xmmCRC0 = Pclmulqdq.CarrylessMultiply(xmmCRC0.AsUInt64(), crcFold.AsUInt64(), 0x01).AsUInt32();
        xmmCRC1 = Sse2.Xor(xmmCRC1, xTmp0);
        xmmCRC1 = Sse2.Xor(xmmCRC1, xmmCRC0);

        Vector128<uint> xTmp1 = Pclmulqdq.CarrylessMultiply(xmmCRC1.AsUInt64(), crcFold.AsUInt64(), 0x10).AsUInt32();

        xmmCRC1 = Pclmulqdq.CarrylessMultiply(xmmCRC1.AsUInt64(), crcFold.AsUInt64(), 0x01).AsUInt32();
        xmmCRC2 = Sse2.Xor(xmmCRC2, xTmp1);
        xmmCRC2 = Sse2.Xor(xmmCRC2, xmmCRC1);

        Vector128<uint> xTmp2 = Pclmulqdq.CarrylessMultiply(xmmCRC2.AsUInt64(), crcFold.AsUInt64(), 0x10).AsUInt32();

        xmmCRC2 = Pclmulqdq.CarrylessMultiply(xmmCRC2.AsUInt64(), crcFold.AsUInt64(), 0x01).AsUInt32();
        xmmCRC3 = Sse2.Xor(xmmCRC3, xTmp2);
        xmmCRC3 = Sse2.Xor(xmmCRC3, xmmCRC2);

        /*
         * k5
         */
        crcFold = Vector128.Create(_crcK[4], _crcK[5], _crcK[6], _crcK[7]);

        xmmCRC0 = xmmCRC3;
        xmmCRC3 = Pclmulqdq.CarrylessMultiply(xmmCRC3.AsUInt64(), crcFold.AsUInt64(), 0).AsUInt32();
        xmmCRC0 = Sse2.ShiftRightLogical128BitLane(xmmCRC0, 8);
        xmmCRC3 = Sse2.Xor(xmmCRC3, xmmCRC0);

        xmmCRC0 = xmmCRC3;
        xmmCRC3 = Sse2.ShiftLeftLogical128BitLane(xmmCRC3, 4);
        xmmCRC3 = Pclmulqdq.CarrylessMultiply(xmmCRC3.AsUInt64(), crcFold.AsUInt64(), 0x10).AsUInt32();
        xmmCRC3 = Sse2.Xor(xmmCRC3, xmmCRC0);
        xmmCRC3 = Sse2.And(xmmCRC3, xmmMask2);

        /*
         * k7
         */
        xmmCRC1 = xmmCRC3;
        xmmCRC2 = xmmCRC3;
        crcFold = Vector128.Create(_crcK[8], _crcK[9], _crcK[10], _crcK[11]);

        xmmCRC3 = Pclmulqdq.CarrylessMultiply(xmmCRC3.AsUInt64(), crcFold.AsUInt64(), 0).AsUInt32();
        xmmCRC3 = Sse2.Xor(xmmCRC3, xmmCRC2);
        xmmCRC3 = Sse2.And(xmmCRC3, xmmMask);

        xmmCRC2 = xmmCRC3;
        xmmCRC3 = Pclmulqdq.CarrylessMultiply(xmmCRC3.AsUInt64(), crcFold.AsUInt64(), 0x10).AsUInt32();
        xmmCRC3 = Sse2.Xor(xmmCRC3, xmmCRC2);
        xmmCRC3 = Sse2.Xor(xmmCRC3, xmmCRC1);

        /*
         * could just as well write xmm_crc3[2], doing a movaps and truncating, but
         * no real advantage - it's a tiny bit slower per call, while no additional CPUs
         * would be supported by only requiring SSSE3 and CLMUL instead of SSE4.1 + CLMUL
         */
        return ~Sse41.Extract(xmmCRC3, 2);
    }
}