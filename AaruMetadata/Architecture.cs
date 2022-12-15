// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Architecture.cs
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

using System.Text.Json.Serialization;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Aaru.CommonTypes.AaruMetadata;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum Architecture
{
    [JsonPropertyName("4004")]
    _4004, [JsonPropertyName("4040")]
    _4040, [JsonPropertyName("6502")]
    _6502, [JsonPropertyName("65816")]
    _65816, [JsonPropertyName("8008")]
    _8008, [JsonPropertyName("8051")]
    _8051, [JsonPropertyName("8080")]
    _8080, [JsonPropertyName("8085")]
    _8085, Aarch64, Am29000, Amd64,
    Apx432, Arm, Avr,
    Avr32, Axp, Clipper,
    Cray, Esa390, Hobbit,
    I86, I860, I960,
    Ia32, Ia64, M56K,
    M6800, M6801, M6805,
    M6809, M68K, M88K,
    Mcs41, Mcs48, Mips32,
    Mips64, Msp430, Nios2,
    Openrisc, Parisc, PDP1,
    PDP10, PDP11, PDP7,
    PDP8, Pic, Power,
    Ppc, Ppc64, Prism,
    Renesasrx, Riscv, S360,
    S370, Sh, Sh1,
    Sh2, Sh3, Sh4,
    Sh5, Sh64, Sparc,
    Sparc64, Transputer, Vax,
    We32000, X32, Z80,
    Z800, Z8000, Z80000,
    Zarch
}
