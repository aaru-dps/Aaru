// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Layers.cs
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
using System.Diagnostics.CodeAnalysis;
using Schemas;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Aaru.CommonTypes.AaruMetadata;

public class Layers
{
    public List<Sectors> Sectors { get; set; }
    public LayerType?    Type    { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Layers(LayersType cicm)
    {
        if(cicm is null)
            return null;

        var layers = new Layers
        {
            Type = cicm.typeSpecified ? (LayerType)cicm.type : null
        };

        if(cicm.Sectors is null)
            return layers;

        layers.Sectors = new List<Sectors>();

        foreach(SectorsType sec in cicm.Sectors)
            layers.Sectors.Add(sec);

        return layers;
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum LayerType
{
    PTP, OTP
}

public class LayeredText
{
    public uint?  Layer { get; set; }
    public string Text  { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator LayeredText(LayeredTextType cicm) => cicm is null ? null : new LayeredText
    {
        Layer = cicm.layerSpecified ? cicm.layer : null,
        Text  = cicm.Value
    };
}

public class Sectors
{
    public uint? Layer { get; set; }
    public ulong Value { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Sectors(SectorsType cicm) => cicm is null ? null : new Sectors
    {
        Layer = cicm.layerSpecified ? cicm.layer : null,
        Value = cicm.Value
    };
}
