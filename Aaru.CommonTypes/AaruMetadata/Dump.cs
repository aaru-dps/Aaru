// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dump.cs
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

public class Image
{
    public string Format { get; set; }
    public ulong? Offset { get; set; }
    public string Value  { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Image(ImageType cicm) => cicm is null
                                                                 ? null
                                                                 : new Image
                                                                 {
                                                                     Format = cicm.format,
                                                                     Offset = cicm.offsetSpecified ? cicm.offset : null,
                                                                     Value  = cicm.Value
                                                                 };
}

public class Dump
{
    public string         Image     { get; set; }
    public ulong          Size      { get; set; }
    public List<Checksum> Checksums { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Dump(DumpType cicm)
    {
        if(cicm is null) return null;

        Dump dump = new()
        {
            Image = cicm.Image,
            Size  = cicm.Size
        };

        if(cicm.Checksums is null) return dump;

        dump.Checksums = [];

        foreach(Schemas.ChecksumType chk in cicm.Checksums) dump.Checksums.Add(chk);

        return dump;
    }
}

public class Border
{
    public string         Image     { get; set; }
    public ulong          Size      { get; set; }
    public List<Checksum> Checksums { get; set; }
    public uint?          Session   { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Border(BorderType cicm)
    {
        if(cicm is null) return null;

        var border = new Border
        {
            Image   = cicm.Image,
            Size    = cicm.Size,
            Session = cicm.sessionSpecified ? cicm.session : null
        };

        if(cicm.Checksums is null) return border;

        border.Checksums = [];

        foreach(Schemas.ChecksumType chk in cicm.Checksums) border.Checksums.Add(chk);

        return border;
    }
}

public class File
{
    public string Format { get; set; }
    public string Value  { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator File(FileType cicm) => cicm is null
                                                               ? null
                                                               : new File
                                                               {
                                                                   Format = cicm.format,
                                                                   Value  = cicm.Value
                                                               };
}

public class BlockSize
{
    public uint StartingBlock { get; set; }
    public uint Value         { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator BlockSize(BlockSizeType cicm) => cicm is null
                                                                         ? null
                                                                         : new BlockSize
                                                                         {
                                                                             StartingBlock = cicm.startingBlock,
                                                                             Value         = cicm.Value
                                                                         };
}