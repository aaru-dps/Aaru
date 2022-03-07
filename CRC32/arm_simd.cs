// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : arm_simd.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//                  The Chromium Authors
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
// Compute CRC32 checksum using ARM special instructions..
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

namespace Aaru.Checksums.CRC32;

using System;
using System.Runtime.Intrinsics.Arm;

static class ArmSimd
{
    internal static uint Step64(byte[] buf, long len, uint crc)
    {
        uint c = crc;

        var bufPos = 0;

        while(len >= 64)
        {
            c      =  Crc32.Arm64.ComputeCrc32(c, BitConverter.ToUInt64(buf, bufPos));
            bufPos += 8;
            c      =  Crc32.Arm64.ComputeCrc32(c, BitConverter.ToUInt64(buf, bufPos));
            bufPos += 8;
            c      =  Crc32.Arm64.ComputeCrc32(c, BitConverter.ToUInt64(buf, bufPos));
            bufPos += 8;
            c      =  Crc32.Arm64.ComputeCrc32(c, BitConverter.ToUInt64(buf, bufPos));
            bufPos += 8;
            c      =  Crc32.Arm64.ComputeCrc32(c, BitConverter.ToUInt64(buf, bufPos));
            bufPos += 8;
            c      =  Crc32.Arm64.ComputeCrc32(c, BitConverter.ToUInt64(buf, bufPos));
            bufPos += 8;
            c      =  Crc32.Arm64.ComputeCrc32(c, BitConverter.ToUInt64(buf, bufPos));
            bufPos += 8;
            c      =  Crc32.Arm64.ComputeCrc32(c, BitConverter.ToUInt64(buf, bufPos));
            bufPos += 8;
            len    -= 64;
        }

        while(len >= 8)
        {
            c      =  Crc32.Arm64.ComputeCrc32(c, BitConverter.ToUInt64(buf, bufPos));
            bufPos += 8;
            len    -= 8;
        }

        while(len-- > 0)
            c = Crc32.ComputeCrc32(c, buf[bufPos++]);

        return c;
    }

    internal static uint Step32(byte[] buf, long len, uint crc)
    {
        uint c = crc;

        var bufPos = 0;

        while(len >= 32)
        {
            c      =  Crc32.ComputeCrc32(c, BitConverter.ToUInt32(buf, bufPos));
            bufPos += 4;
            c      =  Crc32.ComputeCrc32(c, BitConverter.ToUInt32(buf, bufPos));
            bufPos += 4;
            c      =  Crc32.ComputeCrc32(c, BitConverter.ToUInt32(buf, bufPos));
            bufPos += 4;
            c      =  Crc32.ComputeCrc32(c, BitConverter.ToUInt32(buf, bufPos));
            bufPos += 4;
            c      =  Crc32.ComputeCrc32(c, BitConverter.ToUInt32(buf, bufPos));
            bufPos += 4;
            c      =  Crc32.ComputeCrc32(c, BitConverter.ToUInt32(buf, bufPos));
            bufPos += 4;
            c      =  Crc32.ComputeCrc32(c, BitConverter.ToUInt32(buf, bufPos));
            bufPos += 4;
            c      =  Crc32.ComputeCrc32(c, BitConverter.ToUInt32(buf, bufPos));
            bufPos += 4;
            len    -= 32;
        }

        while(len >= 4)
        {
            c      =  Crc32.ComputeCrc32(c, BitConverter.ToUInt32(buf, bufPos));
            bufPos += 4;
            len    -= 4;
        }

        while(len-- > 0)
            c = Crc32.ComputeCrc32(c, buf[bufPos++]);

        return c;
    }
}