// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MPEG.cs
// Author(s)      : Rebecca Wallander <sakcheen+github@gmail.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles MPEG packets functionality.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2023-2024 Rebecca Wallander
// ****************************************************************************/

// http://www.mpucoder.com/DVD/vobov.html

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace Aaru.Decryption.DVD;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Mpeg
{
#region Mpeg2StreamId enum

    public enum Mpeg2StreamId : byte
    {
        ProgramEnd                                                       = 0xB9,
        PackHeader                                                       = 0xBA,
        SystemHeader                                                     = 0xBB,
        ProgramStreamMap                                                 = 0xBC,
        PrivateStream1                                                   = 0xBD,
        PaddingStream                                                    = 0xBE,
        PrivateStream2                                                   = 0xBF,
        EcmStream                                                        = 0xF0,
        EmmStream                                                        = 0xF1,
        ItuTRecH222_0_Or_IsoIec13818_1AnnexA_Or_IsoIec13818_6DsmccStream = 0xF2,
        IsoIec13522Stream                                                = 0xF3,
        ItuTRecH222_1TypeA                                               = 0xF4,
        ItuTRecH222_1TypeB                                               = 0xF5,
        ItuTRecH222_1TypeC                                               = 0xF6,
        ItuTRecH222_1TypeD                                               = 0xF7,
        ItuTRecH222_1TypeE                                               = 0xF8,
        AncillaryStream                                                  = 0xF9,
        Reserved1                                                        = 0xFA,
        Reserved2                                                        = 0xFB,
        Reserved3                                                        = 0xFC,
        Reserved4                                                        = 0xFD,
        Reserved5                                                        = 0xFE,
        ProgramStreamDirectory                                           = 0xFF,

        // DVD Video can only hold 8 audio streams
        MpegAudioStream1  = 0xC0,
        MpegAudioStream2  = 0xC1,
        MpegAudioStream3  = 0xC2,
        MpegAudioStream4  = 0xC3,
        MpegAudioStream5  = 0xC4,
        MpegAudioStream6  = 0xC5,
        MpegAudioStream7  = 0xC6,
        MpegAudioStream8  = 0xC7,
        MpegAudioStream9  = 0xC8,
        MpegAudioStream10 = 0xC9,
        MpegAudioStream11 = 0xCA,
        MpegAudioStream12 = 0xCB,
        MpegAudioStream13 = 0xCC,
        MpegAudioStream14 = 0xCD,
        MpegAudioStream15 = 0xCE,
        MpegAudioStream16 = 0xCF,
        MpegAudioStream17 = 0xD0,
        MpegAudioStream18 = 0xD1,
        MpegAudioStream19 = 0xD2,
        MpegAudioStream20 = 0xD3,
        MpegAudioStream21 = 0xD4,
        MpegAudioStream22 = 0xD5,
        MpegAudioStream23 = 0xD6,
        MpegAudioStream24 = 0xD7,
        MpegAudioStream25 = 0xD8,
        MpegAudioStream26 = 0xD9,
        MpegAudioStream27 = 0xDA,
        MpegAudioStream28 = 0xDB,
        MpegAudioStream29 = 0xDC,
        MpegAudioStream30 = 0xDD,
        MpegAudioStream31 = 0xDE,
        MpegAudioStream32 = 0xDF,

        // DVD Video can only hold 1 video stream
        MpegVideStream1  = 0xE0,
        MpegVideStream2  = 0xE1,
        MpegVideStream3  = 0xE2,
        MpegVideStream4  = 0xE3,
        MpegVideStream5  = 0xE4,
        MpegVideStream6  = 0xE5,
        MpegVideStream7  = 0xE6,
        MpegVideStream8  = 0xE7,
        MpegVideStream9  = 0xE8,
        MpegVideStream10 = 0xE9,
        MpegVideStream11 = 0xEA,
        MpegVideStream12 = 0xEB,
        MpegVideStream13 = 0xEC,
        MpegVideStream14 = 0xED,
        MpegVideStream15 = 0xEE,
        MpegVideStream16 = 0xEF
    }

#endregion

    static readonly byte[] _mpeg2PackHeaderStartCode = [0x0, 0x0, 0x1];

    public static bool ContainsMpegPackets(byte[] sectorData, uint blocks = 1, uint blockSize = 2048)
    {
        for(uint i = 0; i < blocks; i++)
            if(IsMpegPacket(sectorData.Skip((int)(i * blockSize))))
                return true;

        return false;
    }

    public static bool IsMpegPacket(IEnumerable<byte> sector) =>
        sector.Take(3).ToArray().SequenceEqual(_mpeg2PackHeaderStartCode);

#region Nested type: MpegHeader

    public struct MpegHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] StartCode;
        public byte PackIdentifier;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] SCRBlock;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] ProgramMuxRateBlock;
        byte _packStuffingLengthBlock;
    }

#endregion
}