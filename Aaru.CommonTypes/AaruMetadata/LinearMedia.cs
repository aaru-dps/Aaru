// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LinearMedia.cs
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

public class LinearMedia
{
    public Image              Image           { get; set; }
    public ulong              Size            { get; set; }
    public List<Checksum>     ImageChecksums  { get; set; }
    public List<Checksum>     Checksums       { get; set; }
    public string             PartNumber      { get; set; }
    public string             SerialNumber    { get; set; }
    public string             Title           { get; set; }
    public uint?              Sequence        { get; set; }
    public uint?              ImageInterleave { get; set; }
    public uint?              Interleave      { get; set; }
    public string             Manufacturer    { get; set; }
    public string             Model           { get; set; }
    public string             Package         { get; set; }
    public string             Interface       { get; set; }
    public Dimensions         Dimensions      { get; set; }
    public Scans              Scans           { get; set; }
    public List<DumpHardware> DumpHardware    { get; set; }
    public Pcmcia             Pcmcia          { get; set; }
    public string             CopyProtection  { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator LinearMedia(LinearMediaType cicm)
    {
        if(cicm is null) return null;

        var linearMedia = new LinearMedia
        {
            Image           = cicm.Image,
            Size            = cicm.Size,
            PartNumber      = cicm.PartNumber,
            SerialNumber    = cicm.SerialNumber,
            Title           = cicm.Title,
            Sequence        = cicm.SequenceSpecified ? cicm.Sequence : null,
            ImageInterleave = cicm.ImageInterleaveSpecified ? cicm.ImageInterleave : null,
            Interleave      = cicm.InterleaveSpecified ? cicm.Interleave : null,
            Manufacturer    = cicm.Manufacturer,
            Model           = cicm.Model,
            Package         = cicm.Package,
            Interface       = cicm.Interface,
            Dimensions      = cicm.Dimensions,
            Scans           = cicm.Scans,
            Pcmcia          = cicm.PCMCIA,
            CopyProtection  = cicm.CopyProtection
        };

        if(cicm.ImageChecksums is not null)
        {
            linearMedia.ImageChecksums = [];

            foreach(Schemas.ChecksumType chk in cicm.ImageChecksums) linearMedia.ImageChecksums.Add(chk);
        }

        if(cicm.Checksums is not null)
        {
            linearMedia.Checksums = [];

            foreach(Schemas.ChecksumType chk in cicm.Checksums) linearMedia.Checksums.Add(chk);
        }

        if(cicm.DumpHardwareArray is null) return linearMedia;

        linearMedia.DumpHardware = [];

        foreach(DumpHardwareType hw in cicm.DumpHardwareArray) linearMedia.DumpHardware.Add(hw);

        return linearMedia;
    }
}