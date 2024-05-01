// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PCMCIA.cs
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

using System;
using System.Collections.Generic;
using Schemas;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Aaru.CommonTypes.AaruMetadata;

public class Pcmcia
{
    public Dump         Cis                   { get; set; }
    public string       Compliance            { get; set; }
    public ushort?      ManufacturerCode      { get; set; }
    public ushort?      CardCode              { get; set; }
    public string       Manufacturer          { get; set; }
    public string       ProductName           { get; set; }
    public List<string> AdditionalInformation { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Pcmcia(PCMCIAType cicm) => cicm is null
                                                                   ? null
                                                                   : new Pcmcia
                                                                   {
                                                                       Cis        = cicm.CIS,
                                                                       Compliance = cicm.Compliance,
                                                                       ManufacturerCode =
                                                                           cicm.ManufacturerCodeSpecified
                                                                               ? cicm.ManufacturerCode
                                                                               : null,
                                                                       CardCode =
                                                                           cicm.CardCodeSpecified
                                                                               ? cicm.CardCode
                                                                               : null,
                                                                       Manufacturer = cicm.Manufacturer,
                                                                       ProductName  = cicm.ProductName,
                                                                       AdditionalInformation =
                                                                           cicm.AdditionalInformation is null
                                                                               ? null
                                                                               : new List<string>(cicm
                                                                                  .AdditionalInformation)
                                                                   };
}