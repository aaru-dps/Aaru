// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlockMedia.cs
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

public class BlockMedia
{
    public Image               Image                 { get; set; }
    public ulong               Size                  { get; set; }
    public List<Checksum>      Checksums             { get; set; }
    public List<Checksum>      ContentChecksums      { get; set; }
    public Sequence            Sequence              { get; set; }
    public string              Manufacturer          { get; set; }
    public string              Model                 { get; set; }
    public string              Serial                { get; set; }
    public string              Firmware              { get; set; }
    public string              PartNumber            { get; set; }
    public string              SerialNumber          { get; set; }
    public uint                PhysicalBlockSize     { get; set; }
    public uint                LogicalBlockSize      { get; set; }
    public ulong               LogicalBlocks         { get; set; }
    public List<BlockSize>     VariableBlockSize     { get; set; }
    public List<TapePartition> TapeInformation       { get; set; }
    public Scans               Scans                 { get; set; }
    public ATA                 ATA                   { get; set; }
    public Pci                 Pci                   { get; set; }
    public Pcmcia              Pcmcia                { get; set; }
    public SecureDigital       SecureDigital         { get; set; }
    public MultiMediaCard      MultiMediaCard        { get; set; }
    public SCSI                SCSI                  { get; set; }
    public Usb                 Usb                   { get; set; }
    public Dump                Mam                   { get; set; }
    public ushort?             Heads                 { get; set; }
    public uint?               Cylinders             { get; set; }
    public ulong?              SectorsPerTrack       { get; set; }
    public List<BlockTrack>    Track                 { get; set; }
    public string              CopyProtection        { get; set; }
    public DimensionsNew       Dimensions            { get; set; }
    public List<Partition>     FileSystemInformation { get; set; }
    public List<DumpHardware>  DumpHardware          { get; set; }
    public string              DiskType              { get; set; }
    public string              DiskSubType           { get; set; }
    public string              Interface             { get; set; }
}

public class BlockTrack
{
    public Image          Image          { get; set; }
    public ulong          Size           { get; set; }
    public ushort         Head           { get; set; }
    public uint           Cylinder       { get; set; }
    public ulong          StartSector    { get; set; }
    public ulong          EndSector      { get; set; }
    public ulong          Sectors        { get; set; }
    public uint           BytesPerSector { get; set; }
    public List<Checksum> Checksums      { get; set; }
    public string         Format         { get; set; }
}
