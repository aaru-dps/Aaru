// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Checksum.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines format for metadata.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

using System;
using System.Text.Json.Serialization;

namespace Aaru.CommonTypes.AaruMetadata;

public class Checksum
{
    public ChecksumType Type  { get; set; }
    public string       Value { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Checksum(Schemas.ChecksumType cicm) => cicm is null
                                                                               ? null
                                                                               : new Checksum
                                                                               {
                                                                                   Value = cicm.Value,
                                                                                   Type  = (ChecksumType)cicm.type
                                                                               };
}

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum ChecksumType
{
    Fletcher16,
    Fletcher32,
    Adler32,
    CRC16,
    CRC16Ccitt,
    CRC32,
    CRC64,
    Md4,
    Md5,
    Dm6,
    Ripemd128,
    Ripemd160,
    Ripemed320,
    Sha1,
    Sha224,
    Sha256,
    Sha384,
    Sha512,
    Sha3,
    Skein,
    Snefru,
    Blake256,
    Blake512,
    Tiger,
    Whirlpool,
    SpamSum
}