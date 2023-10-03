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

using System;
using System.Collections.Generic;
using Schemas;

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
    public Dimensions          Dimensions            { get; set; }
    public List<Partition>     FileSystemInformation { get; set; }
    public List<DumpHardware>  DumpHardware          { get; set; }
    public string              MediaType             { get; set; }
    public string              MediaSubType          { get; set; }
    public string              Interface             { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator BlockMedia(BlockMediaType cicm)
    {
        if(cicm is null)
            return null;

        var media = new BlockMedia
        {
            Image             = cicm.Image,
            Size              = cicm.Size,
            Sequence          = cicm.Sequence,
            Manufacturer      = cicm.Manufacturer,
            Model             = cicm.Model,
            Serial            = cicm.Serial,
            Firmware          = cicm.Firmware,
            PartNumber        = cicm.PartNumber,
            SerialNumber      = cicm.SerialNumber,
            PhysicalBlockSize = cicm.PhysicalBlockSize,
            LogicalBlockSize  = cicm.LogicalBlockSize,
            LogicalBlocks     = cicm.LogicalBlocks,
            Scans             = cicm.Scans,
            ATA               = cicm.ATA,
            Pci               = cicm.PCI,
            Pcmcia            = cicm.PCMCIA,
            SecureDigital     = cicm.SecureDigital,
            MultiMediaCard    = cicm.MultiMediaCard,
            SCSI              = cicm.SCSI,
            Usb               = cicm.USB,
            Mam               = cicm.MAM,
            Heads             = cicm.HeadsSpecified ? cicm.Heads : null,
            Cylinders         = cicm.CylindersSpecified ? cicm.Cylinders : null,
            SectorsPerTrack   = cicm.SectorsPerTrackSpecified ? cicm.SectorsPerTrack : null,
            CopyProtection    = cicm.CopyProtection,
            Dimensions        = cicm.Dimensions,
            MediaType         = cicm.DiskType,
            MediaSubType      = cicm.DiskSubType,
            Interface         = cicm.Interface
        };

        if(cicm.Checksums is not null)
        {
            media.Checksums = new List<Checksum>();

            foreach(Schemas.ChecksumType chk in cicm.Checksums)
                media.Checksums.Add(chk);
        }

        if(cicm.ContentChecksums is not null)
        {
            media.ContentChecksums = new List<Checksum>();

            foreach(Schemas.ChecksumType chk in cicm.ContentChecksums)
                media.ContentChecksums.Add(chk);
        }

        if(cicm.VariableBlockSize is not null)
        {
            media.VariableBlockSize = new List<BlockSize>();

            foreach(BlockSizeType blkSize in cicm.VariableBlockSize)
                media.VariableBlockSize.Add(blkSize);
        }

        if(cicm.TapeInformation is not null)
        {
            media.TapeInformation = new List<TapePartition>();

            foreach(TapePartitionType tapeInformation in cicm.TapeInformation)
                media.TapeInformation.Add(tapeInformation);
        }

        if(cicm.FileSystemInformation is not null)
        {
            media.FileSystemInformation = new List<Partition>();

            foreach(PartitionType fsInfo in cicm.FileSystemInformation)
                media.FileSystemInformation.Add(fsInfo);
        }

        if(cicm.DumpHardwareArray is null)
            return media;

        media.DumpHardware = new List<DumpHardware>();

        foreach(DumpHardwareType hw in cicm.DumpHardwareArray)
            media.DumpHardware.Add(hw);

        return media;
    }
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

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator BlockTrack(BlockTrackType cicm)
    {
        if(cicm is null)
            return null;

        var trk = new BlockTrack
        {
            Image          = cicm.Image,
            Size           = cicm.Size,
            Head           = cicm.Head,
            Cylinder       = cicm.Cylinder,
            StartSector    = cicm.StartSector,
            EndSector      = cicm.EndSector,
            Sectors        = cicm.Sectors,
            BytesPerSector = cicm.BytesPerSector,
            Format         = cicm.Format
        };

        if(cicm.Checksums is null)
            return trk;

        trk.Checksums = new List<Checksum>();

        foreach(Schemas.ChecksumType chk in cicm.Checksums)
            trk.Checksums.Add(chk);

        return trk;
    }
}