// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : clmul.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
// Compute the CRC64 using a parallelized folding approach with the PCLMULQDQ
// instruction.
//
// --[ License ] --------------------------------------------------------------
//
//     This file is under the public domain:
//     https://github.com/rawrunprotected/crc
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Aaru.Checksums.CRC64;

static class Clmul
{
    static readonly byte[] _shuffleMasks =
    {
        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x8f, 0x8e,
        0x8d, 0x8c, 0x8b, 0x8a, 0x89, 0x88, 0x87, 0x86, 0x85, 0x84, 0x83, 0x82, 0x81, 0x80
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void ShiftRight128(Vector128<ulong>     initial, uint n, out Vector128<ulong> outLeft,
                              out Vector128<ulong> outRight)
    {
        uint maskPos = 16 - n;

        var maskA = Vector128.Create(_shuffleMasks[maskPos],
                                     _shuffleMasks[maskPos + 1],
                                     _shuffleMasks[maskPos + 2],
                                     _shuffleMasks[maskPos + 3],
                                     _shuffleMasks[maskPos + 4],
                                     _shuffleMasks[maskPos + 5],
                                     _shuffleMasks[maskPos + 6],
                                     _shuffleMasks[maskPos + 7],
                                     _shuffleMasks[maskPos + 8],
                                     _shuffleMasks[maskPos + 9],
                                     _shuffleMasks[maskPos + 10],
                                     _shuffleMasks[maskPos + 11],
                                     _shuffleMasks[maskPos + 12],
                                     _shuffleMasks[maskPos + 13],
                                     _shuffleMasks[maskPos + 14],
                                     _shuffleMasks[maskPos + 15]);

        Vector128<byte> maskB = Sse2.Xor(maskA, Sse2.CompareEqual(Vector128<byte>.Zero, Vector128<byte>.Zero));

        outLeft  = Ssse3.Shuffle(initial.AsByte(), maskB).AsUInt64();
        outRight = Ssse3.Shuffle(initial.AsByte(), maskA).AsUInt64();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Vector128<ulong> Fold(Vector128<ulong> input, Vector128<ulong> foldConstants) =>
        Sse2.Xor(Pclmulqdq.CarrylessMultiply(input, foldConstants, 0x00),
                 Pclmulqdq.CarrylessMultiply(input, foldConstants, 0x11));

    internal static ulong Step(ulong crc, byte[] data, uint length)
    {
        var         bufPos         = 16;
        const ulong k1             = 0xe05dd497ca393ae4;
        const ulong k2             = 0xdabe95afc7875f40;
        const ulong mu             = 0x9c3e466c172963d5;
        const ulong pol            = 0x92d8af2baf0e1e85;
        var         foldConstants1 = Vector128.Create(k1,   k2);
        var         foldConstants2 = Vector128.Create(mu,   pol);
        var         initialCrc     = Vector128.Create(~crc, 0);
        length -= 16;

        // Initial CRC can simply be added to data
        ShiftRight128(initialCrc, 0, out Vector128<ulong> crc0, out Vector128<ulong> crc1);

        Vector128<ulong> accumulator =
            Sse2.Xor(Fold(Sse2.Xor(crc0,
                                   Vector128.Create(BitConverter.ToUInt64(data, 0), BitConverter.ToUInt64(data, 8))),
                          foldConstants1),
                     crc1);

        while(length >= 32)
        {
            accumulator =
                Fold(Sse2.Xor(Vector128.Create(BitConverter.ToUInt64(data, bufPos),
                                               BitConverter.ToUInt64(data, bufPos + 8)),
                              accumulator),
                     foldConstants1);

            length -= 16;
            bufPos += 16;
        }

        Vector128<ulong> p = Sse2.Xor(accumulator,
                                      Vector128.Create(BitConverter.ToUInt64(data, bufPos),
                                                       BitConverter.ToUInt64(data, bufPos + 8)));

        Vector128<ulong> r = Sse2.Xor(Pclmulqdq.CarrylessMultiply(p, foldConstants1, 0x10),
                                      Sse2.ShiftRightLogical128BitLane(p, 8));

        // Final Barrett reduction
        Vector128<ulong> t1 = Pclmulqdq.CarrylessMultiply(r, foldConstants2, 0x00);

        Vector128<ulong> t2 = Sse2.Xor(Sse2.Xor(Pclmulqdq.CarrylessMultiply(t1, foldConstants2, 0x10),
                                                Sse2.ShiftLeftLogical128BitLane(t1, 8)),
                                       r);

        return ~((ulong)Sse41.Extract(t2.AsUInt32(), 3) << 32 | Sse41.Extract(t2.AsUInt32(), 2));
    }
}