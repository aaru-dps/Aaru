// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Barcode.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aaru.CommonTypes.AaruMetadata;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum BarcodeType
{
    Aztec,
    Codabar,
    Code11,
    Code128,
    Code39,
    Code93,
    CPC_Binary,
    EZcode,
    FIM,
    ITF,
    ITF14,
    EAN13,
    EAN8,
    MaxiCode,
    ISBN,
    ISRC,
    MSI,
    ShotCode,
    RM4SCC,
    QR,
    EAN5,
    EAN2,
    POSTNET,
    PostBar,
    Plessey,
    Pharmacode,
    PDF417,
    PatchCode
}

public class Barcode
{
    public BarcodeType Type  { get; set; }
    public string      Value { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Barcode(Schemas.BarcodeType cicm) => cicm is null
                                                                             ? null
                                                                             : new Barcode
                                                                             {
                                                                                 Type  = (BarcodeType)cicm.type,
                                                                                 Value = cicm.Value
                                                                             };
}