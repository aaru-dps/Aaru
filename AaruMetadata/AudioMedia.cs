// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AudioMedia.cs
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

public class AudioMedia
{
    public Image              Image          { get; set; }
    public ulong              Size           { get; set; }
    public List<Checksum>     Checksums      { get; set; }
    public Sequence           Sequence       { get; set; }
    public string             PartNumber     { get; set; }
    public string             SerialNumber   { get; set; }
    public string             Manufacturer   { get; set; }
    public string             Model          { get; set; }
    public string             AccoustID      { get; set; }
    public List<AudioBlock>   Blocks         { get; set; }
    public string             CopyProtection { get; set; }
    public DimensionsNew      Dimensions     { get; set; }
    public Scans              Scans          { get; set; }
    public List<DumpHardware> DumpHardware   { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator AudioMedia(AudioMediaType cicm)
    {
        if(cicm is null)
            return null;

        var media = new AudioMedia
        {
            Image          = cicm.Image,
            Size           = cicm.Size,
            Sequence       = cicm.Sequence,
            PartNumber     = cicm.PartNumber,
            SerialNumber   = cicm.SerialNumber,
            Manufacturer   = cicm.Manufacturer,
            Model          = cicm.Model,
            AccoustID      = cicm.AccoustID,
            CopyProtection = cicm.CopyProtection,
            Dimensions     = cicm.Dimensions,
            Scans          = cicm.Scans
        };

        if(cicm.Checksums is not null)
        {
            media.Checksums = new List<Checksum>();

            foreach(Schemas.ChecksumType chk in cicm.Checksums)
                media.Checksums.Add(chk);
        }

        if(cicm.Block is not null)
        {
            media.Blocks = new List<AudioBlock>();

            foreach(AudioBlockType blk in cicm.Block)
                media.Blocks.Add(blk);
        }

        if(cicm.DumpHardwareArray is null)
            return media;

        media.DumpHardware = new List<DumpHardware>();

        foreach(DumpHardwareType hw in cicm.DumpHardwareArray)
            media.DumpHardware.Add(hw);

        return media;
    }
}

public class AudioBlock
{
    public Image          Image     { get; set; }
    public ulong          Size      { get; set; }
    public string         AccoustID { get; set; }
    public List<Checksum> Checksums { get; set; }
    public string         Format    { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator AudioBlock(AudioBlockType cicm)
    {
        if(cicm is null)
            return null;

        var blk = new AudioBlock
        {
            Image     = cicm.Image,
            Size      = cicm.Size,
            AccoustID = cicm.AccoustID,
            Format    = cicm.Format
        };

        if(cicm.Checksums is null)
            return blk;

        blk.Checksums = new List<Checksum>();

        foreach(Schemas.ChecksumType chk in cicm.Checksums)
            blk.Checksums.Add(chk);

        return blk;
    }
}
