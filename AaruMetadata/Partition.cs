// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Partition.cs
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

public class Partition
{
    public uint             Sequence    { get; set; }
    public string           Name        { get; set; }
    public string           Type        { get; set; }
    public ulong            StartSector { get; set; }
    public ulong            EndSector   { get; set; }
    public string           Description { get; set; }
    public List<FileSystem> FileSystems { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Partition(PartitionType cicm)
    {
        if(cicm is null)
            return null;

        var part = new Partition
        {
            Sequence    = cicm.Sequence,
            Name        = cicm.Name,
            Type        = cicm.Type,
            StartSector = cicm.StartSector,
            EndSector   = cicm.EndSector,
            Description = cicm.Description
        };

        if(cicm.FileSystems is null)
            return part;

        part.FileSystems = new List<FileSystem>();

        foreach(FileSystemType fs in cicm.FileSystems)
            part.FileSystems.Add(fs);

        return part;
    }
}
