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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;

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
    public DimensionsNew      Dimensions      { get; set; }
    public Scans              Scans           { get; set; }
    public List<DumpHardware> DumpHardware    { get; set; }
    public Pcmcia             Pcmcia          { get; set; }
    public string             CopyProtection  { get; set; }
}
