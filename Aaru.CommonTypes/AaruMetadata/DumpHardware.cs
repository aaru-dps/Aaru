// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DumpHardware.cs
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Schemas;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Aaru.CommonTypes.AaruMetadata;

public class DumpHardware
{
    public string       Manufacturer { get; set; }
    public string       Model        { get; set; }
    public string       Revision     { get; set; }
    public string       Firmware     { get; set; }
    public string       Serial       { get; set; }
    public List<Extent> Extents      { get; set; }
    public Software     Software     { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator DumpHardware(DumpHardwareType cicm)
    {
        if(cicm is null) return null;

        var hw = new DumpHardware
        {
            Manufacturer = cicm.Manufacturer,
            Model        = cicm.Model,
            Revision     = cicm.Revision,
            Firmware     = cicm.Firmware,
            Serial       = cicm.Serial,
            Software     = cicm.Software
        };

        if(cicm.Extents is null) return hw;

        hw.Extents = new List<Extent>();

        foreach(ExtentType ext in cicm.Extents) hw.Extents.Add(ext);

        return hw;
    }
}

public class Extent
{
    public ulong Start { get; set; }
    public ulong End   { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Extent(ExtentType cicm) => cicm is null
                                                                   ? null
                                                                   : new Extent
                                                                   {
                                                                       Start = cicm.Start,
                                                                       End   = cicm.End
                                                                   };
}

public class Software
{
    public string Name            { get; set; }
    public string Version         { get; set; }
    public string OperatingSystem { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Software(SoftwareType cicm) => cicm is null
                                                                       ? null
                                                                       : new Software
                                                                       {
                                                                           Name            = cicm.Name,
                                                                           Version         = cicm.Version,
                                                                           OperatingSystem = cicm.OperatingSystem
                                                                       };
}