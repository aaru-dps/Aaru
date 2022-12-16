// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Tape.cs
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

public class TapePartition
{
    public Image          Image      { get; set; }
    public ulong          Size       { get; set; }
    public ulong          Sequence   { get; set; }
    public ulong          StartBlock { get; set; }
    public ulong          EndBlock   { get; set; }
    public List<Checksum> Checksums  { get; set; }
    public List<TapeFile> Files      { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator TapePartition(TapePartitionType cicm)
    {
        if(cicm is null)
            return null;

        TapePartition partition = new()
        {
            Image      = cicm.Image,
            Size       = cicm.Size,
            Sequence   = cicm.Sequence,
            StartBlock = cicm.StartBlock,
            EndBlock   = cicm.EndBlock
        };

        if(cicm.Checksums is not null)
        {
            partition.Checksums = new List<Checksum>();

            foreach(Schemas.ChecksumType chk in cicm.Checksums)
                partition.Checksums.Add(chk);
        }

        if(cicm.File is null)
            return partition;

        partition.Files = new List<TapeFile>();

        foreach(TapeFileType file in cicm.File)
            partition.Files.Add(file);

        return partition;
    }
}

public class TapeFile
{
    public Image          Image      { get; set; }
    public ulong          Size       { get; set; }
    public ulong          Sequence   { get; set; }
    public ulong          BlockSize  { get; set; }
    public ulong          StartBlock { get; set; }
    public ulong          EndBlock   { get; set; }
    public List<Checksum> Checksums  { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator TapeFile(TapeFileType cicm)
    {
        if(cicm is null)
            return null;

        var file = new TapeFile
        {
            Image      = cicm.Image,
            Size       = cicm.Size,
            Sequence   = cicm.Sequence,
            BlockSize  = cicm.BlockSize,
            StartBlock = cicm.StartBlock,
            EndBlock   = cicm.EndBlock
        };

        if(cicm.Checksums is null)
            return file;

        file.Checksums = new List<Checksum>();

        foreach(Schemas.ChecksumType chk in cicm.Checksums)
            file.Checksums.Add(chk);

        return file;
    }
}
