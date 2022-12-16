// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlockMedia.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains logic to create sidecar from a block media dump.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.Console;
using Aaru.Decoders.PCMCIA;
using Aaru.DiscImages;
using Aaru.Filters;
using Aaru.Helpers;
using Directory = System.IO.Directory;
using File = System.IO.File;
using MediaType = Aaru.CommonTypes.Metadata.MediaType;
using Partition = Aaru.CommonTypes.Partition;
using Pcmcia = Aaru.CommonTypes.AaruMetadata.Pcmcia;
using Tuple = Aaru.Decoders.PCMCIA.Tuple;
using Usb = Aaru.CommonTypes.AaruMetadata.Usb;

namespace Aaru.Core;

public sealed partial class Sidecar
{
    /// <summary>Creates a metadata sidecar for a block media (e.g. floppy, hard disk, flash card, usb stick)</summary>
    /// <param name="image">Image</param>
    /// <param name="filterId">Filter uuid</param>
    /// <param name="imagePath">Image path</param>
    /// <param name="fi">Image file information</param>
    /// <param name="plugins">Image plugins</param>
    /// <param name="imgChecksums">List of image checksums</param>
    /// <param name="sidecar">Metadata sidecar</param>
    /// <param name="encoding">Encoding to be used for filesystem plugins</param>
    void BlockMedia(IMediaImage image, Guid filterId, string imagePath, FileInfo fi, PluginBase plugins,
                    List<CommonTypes.AaruMetadata.Checksum> imgChecksums, ref Metadata sidecar, Encoding encoding)
    {
        if(_aborted)
            return;

        sidecar.BlockMedias = new List<BlockMedia>
        {
            new()
            {
                Checksums = imgChecksums,
                Image = new Image
                {
                    Format = image.Format,
                    Offset = 0,
                    Value  = Path.GetFileName(imagePath)
                },
                Size = (ulong)fi.Length,
                Sequence = new Sequence
                {
                    Title = image.Info.MediaTitle
                }
            }
        };

        if(image.Info.MediaSequence     != 0 &&
           image.Info.LastMediaSequence != 0)
        {
            sidecar.BlockMedias[0].Sequence.MediaSequence = (uint)image.Info.MediaSequence;
            sidecar.BlockMedias[0].Sequence.TotalMedia    = (uint)image.Info.LastMediaSequence;
        }
        else
        {
            sidecar.BlockMedias[0].Sequence.MediaSequence = 1;
            sidecar.BlockMedias[0].Sequence.TotalMedia    = 1;
        }

        UpdateStatus(Localization.Core.Hashing_media_tags);
        ErrorNumber errno;
        byte[]      buffer;

        foreach(MediaTagType tagType in image.Info.ReadableMediaTags)
        {
            if(_aborted)
                return;

            switch(tagType)
            {
                case MediaTagType.ATAPI_IDENTIFY:
                    errno = image.ReadMediaTag(MediaTagType.ATAPI_IDENTIFY, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].ATA = new ATA
                    {
                        Identify = new Dump
                        {
                            Checksums = Checksum.GetChecksums(buffer),
                            Size      = (ulong)buffer.Length
                        }
                    };

                    break;
                case MediaTagType.ATA_IDENTIFY:
                    errno = image.ReadMediaTag(MediaTagType.ATA_IDENTIFY, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].ATA = new ATA
                    {
                        Identify = new Dump
                        {
                            Checksums = Checksum.GetChecksums(buffer),
                            Size      = (ulong)buffer.Length
                        }
                    };

                    break;
                case MediaTagType.PCMCIA_CIS:
                    errno = image.ReadMediaTag(MediaTagType.PCMCIA_CIS, out byte[] cis);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].Pcmcia = new Pcmcia
                    {
                        Cis = new Dump
                        {
                            Checksums = Checksum.GetChecksums(cis),
                            Size      = (ulong)cis.Length
                        }
                    };

                    Tuple[] tuples = CIS.GetTuples(cis);

                    if(tuples != null)
                        foreach(Tuple tuple in tuples)
                            switch(tuple.Code)
                            {
                                case TupleCodes.CISTPL_MANFID:
                                    ManufacturerIdentificationTuple manfid =
                                        CIS.DecodeManufacturerIdentificationTuple(tuple);

                                    if(manfid != null)
                                    {
                                        sidecar.BlockMedias[0].Pcmcia.ManufacturerCode = manfid.ManufacturerID;

                                        sidecar.BlockMedias[0].Pcmcia.CardCode = manfid.CardID;
                                    }

                                    break;
                                case TupleCodes.CISTPL_VERS_1:
                                    Level1VersionTuple vers = CIS.DecodeLevel1VersionTuple(tuple);

                                    if(vers != null)
                                    {
                                        sidecar.BlockMedias[0].Pcmcia.Manufacturer = vers.Manufacturer;
                                        sidecar.BlockMedias[0].Pcmcia.ProductName  = vers.Product;

                                        sidecar.BlockMedias[0].Pcmcia.Compliance =
                                            $"{vers.MajorVersion}.{vers.MinorVersion}";

                                        sidecar.BlockMedias[0].Pcmcia.AdditionalInformation =
                                            new List<string>(vers.AdditionalInformation);
                                    }

                                    break;
                            }

                    break;
                case MediaTagType.SCSI_INQUIRY:
                    errno = image.ReadMediaTag(MediaTagType.SCSI_INQUIRY, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].SCSI = new SCSI
                    {
                        Inquiry = new Dump
                        {
                            Checksums = Checksum.GetChecksums(buffer),
                            Size      = (ulong)buffer.Length
                        }
                    };

                    break;
                case MediaTagType.SD_CID:
                    errno = image.ReadMediaTag(MediaTagType.SD_CID, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].SecureDigital ??= new SecureDigital();

                    sidecar.BlockMedias[0].SecureDigital.CID = new Dump
                    {
                        Checksums = Checksum.GetChecksums(buffer),
                        Size      = (ulong)buffer.Length
                    };

                    break;
                case MediaTagType.SD_CSD:
                    errno = image.ReadMediaTag(MediaTagType.SD_CSD, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].SecureDigital ??= new SecureDigital();

                    sidecar.BlockMedias[0].SecureDigital.CSD = new Dump
                    {
                        Checksums = Checksum.GetChecksums(buffer),
                        Size      = (ulong)buffer.Length
                    };

                    break;
                case MediaTagType.SD_SCR:
                    errno = image.ReadMediaTag(MediaTagType.SD_SCR, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].SecureDigital ??= new SecureDigital();

                    sidecar.BlockMedias[0].SecureDigital.SCR = new Dump
                    {
                        Checksums = Checksum.GetChecksums(buffer),
                        Size      = (ulong)buffer.Length
                    };

                    break;
                case MediaTagType.SD_OCR:
                    errno = image.ReadMediaTag(MediaTagType.SD_OCR, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].SecureDigital ??= new SecureDigital();

                    sidecar.BlockMedias[0].SecureDigital.OCR = new Dump
                    {
                        Checksums = Checksum.GetChecksums(buffer),
                        Size      = (ulong)buffer.Length
                    };

                    break;
                case MediaTagType.MMC_CID:
                    errno = image.ReadMediaTag(MediaTagType.MMC_CID, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].MultiMediaCard ??= new MultiMediaCard();

                    sidecar.BlockMedias[0].MultiMediaCard.CID = new Dump
                    {
                        Checksums = Checksum.GetChecksums(buffer),
                        Size      = (ulong)buffer.Length
                    };

                    break;
                case MediaTagType.MMC_CSD:
                    errno = image.ReadMediaTag(MediaTagType.MMC_CSD, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].MultiMediaCard ??= new MultiMediaCard();

                    sidecar.BlockMedias[0].MultiMediaCard.CSD = new Dump
                    {
                        Checksums = Checksum.GetChecksums(buffer),
                        Size      = (ulong)buffer.Length
                    };

                    break;
                case MediaTagType.MMC_OCR:
                    errno = image.ReadMediaTag(MediaTagType.MMC_OCR, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].MultiMediaCard ??= new MultiMediaCard();

                    sidecar.BlockMedias[0].MultiMediaCard.OCR = new Dump
                    {
                        Checksums = Checksum.GetChecksums(buffer),
                        Size      = (ulong)buffer.Length
                    };

                    break;
                case MediaTagType.MMC_ExtendedCSD:
                    errno = image.ReadMediaTag(MediaTagType.MMC_ExtendedCSD, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].MultiMediaCard ??= new MultiMediaCard();

                    sidecar.BlockMedias[0].MultiMediaCard.ExtendedCSD = new Dump
                    {
                        Checksums = Checksum.GetChecksums(buffer),
                        Size      = (ulong)buffer.Length
                    };

                    break;
                case MediaTagType.USB_Descriptors:
                    errno = image.ReadMediaTag(MediaTagType.USB_Descriptors, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].Usb ??= new Usb();

                    sidecar.BlockMedias[0].Usb.Descriptors = new Dump
                    {
                        Checksums = Checksum.GetChecksums(buffer),
                        Size      = (ulong)buffer.Length
                    };

                    break;
                case MediaTagType.SCSI_MODESENSE_6:
                    errno = image.ReadMediaTag(MediaTagType.SCSI_MODESENSE_6, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].SCSI ??= new SCSI();

                    sidecar.BlockMedias[0].SCSI.ModeSense = new Dump
                    {
                        Checksums = Checksum.GetChecksums(buffer),
                        Size      = (ulong)buffer.Length
                    };

                    break;
                case MediaTagType.SCSI_MODESENSE_10:
                    errno = image.ReadMediaTag(MediaTagType.SCSI_MODESENSE_10, out buffer);

                    if(errno != ErrorNumber.NoError)
                        break;

                    sidecar.BlockMedias[0].SCSI ??= new SCSI();

                    sidecar.BlockMedias[0].SCSI.ModeSense10 = new Dump
                    {
                        Checksums = Checksum.GetChecksums(buffer),
                        Size      = (ulong)buffer.Length
                    };

                    break;
            }
        }

        // If there is only one track, and it's the same as the image file (e.g. ".iso" files), don't re-checksum.
        if(image.Id == new Guid("12345678-AAAA-BBBB-CCCC-123456789000") &&
           filterId == new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
            sidecar.BlockMedias[0].ContentChecksums = sidecar.BlockMedias[0].Checksums;
        else
        {
            UpdateStatus(Localization.Core.Hashing_sectors);

            var contentChkWorker = new Checksum();

            // For fast debugging, skip checksum
            //goto skipImageChecksum;

            const uint sectorsToRead = 64;
            ulong      sectors       = image.Info.Sectors;
            ulong      doneSectors   = 0;

            InitProgress2();

            while(doneSectors < sectors)
            {
                if(_aborted)
                {
                    EndProgress2();

                    return;
                }

                byte[] sector;

                if(sectors - doneSectors >= sectorsToRead)
                {
                    errno = image.ReadSectors(doneSectors, sectorsToRead, out sector);

                    if(errno != ErrorNumber.NoError)
                    {
                        UpdateStatus(string.Format(Localization.Core.Error_0_reading_sector_1, errno, doneSectors));
                        EndProgress2();

                        return;
                    }

                    UpdateProgress2(Localization.Core.Hashing_sector_0_of_1, (long)doneSectors, (long)sectors);
                    doneSectors += sectorsToRead;
                }
                else
                {
                    errno = image.ReadSectors(doneSectors, (uint)(sectors - doneSectors), out sector);

                    if(errno != ErrorNumber.NoError)
                    {
                        UpdateStatus(string.Format(Localization.Core.Error_0_reading_sector_1, errno, doneSectors));
                        EndProgress2();

                        return;
                    }

                    UpdateProgress2(Localization.Core.Hashing_sector_0_of_1, (long)doneSectors, (long)sectors);
                    doneSectors += sectors - doneSectors;
                }

                contentChkWorker.Update(sector);
            }

            // For fast debugging, skip checksum
            //skipImageChecksum:

            sidecar.BlockMedias[0].ContentChecksums = contentChkWorker.End();

            EndProgress2();
        }

        (string type, string subType) diskType = MediaType.MediaTypeToString(image.Info.MediaType);
        sidecar.BlockMedias[0].MediaType    = diskType.type;
        sidecar.BlockMedias[0].MediaSubType = diskType.subType;
        Statistics.AddMedia(image.Info.MediaType, false);

        sidecar.BlockMedias[0].Dimensions = Dimensions.DimensionsFromMediaType(image.Info.MediaType);

        sidecar.BlockMedias[0].LogicalBlocks    = image.Info.Sectors;
        sidecar.BlockMedias[0].LogicalBlockSize = image.Info.SectorSize;

        // TODO: Detect it
        sidecar.BlockMedias[0].PhysicalBlockSize = image.Info.SectorSize;

        if(image is ITapeImage { IsTape: true } tapeImage)
        {
            List<TapePartition> tapePartitions = new();

            foreach(CommonTypes.Structs.TapePartition tapePartition in tapeImage.TapePartitions)
            {
                var thisPartition = new TapePartition
                {
                    Image      = sidecar.BlockMedias[0].Image,
                    Sequence   = tapePartition.Number,
                    StartBlock = tapePartition.FirstBlock,
                    EndBlock   = tapePartition.LastBlock
                };

                if(tapeImage.TapePartitions.Count == 1)
                    thisPartition.Checksums = sidecar.BlockMedias[0].ContentChecksums;
                else
                {
                    UpdateStatus(string.Format(Localization.Core.Hashing_partition_0, tapePartition.Number));

                    if(_aborted)
                        return;

                    var tapePartitionChk = new Checksum();

                    // For fast debugging, skip checksum
                    //goto skipImageChecksum;

                    const uint sectorsToRead = 64;
                    ulong      sectors       = tapePartition.LastBlock - tapePartition.FirstBlock + 1;
                    ulong      doneSectors   = 0;

                    InitProgress2();

                    while(doneSectors < sectors)
                    {
                        if(_aborted)
                        {
                            EndProgress2();

                            return;
                        }

                        byte[] sector;

                        if(sectors - doneSectors >= sectorsToRead)
                        {
                            errno = image.ReadSectors(tapePartition.FirstBlock + doneSectors, sectorsToRead,
                                                      out sector);

                            if(errno != ErrorNumber.NoError)
                            {
                                AaruConsole.ErrorWriteLine(string.Format(Localization.Core.Error_0_reading_sector_1,
                                                                         errno,
                                                                         tapePartition.FirstBlock + doneSectors));

                                EndProgress2();

                                return;
                            }

                            UpdateProgress2(Localization.Core.Hashing_blocks_0_of_1, (long)doneSectors, (long)sectors);
                            doneSectors += sectorsToRead;
                        }
                        else
                        {
                            errno = image.ReadSectors(tapePartition.FirstBlock + doneSectors,
                                                      (uint)(sectors - doneSectors), out sector);

                            if(errno != ErrorNumber.NoError)
                            {
                                AaruConsole.ErrorWriteLine(string.Format(Localization.Core.Error_0_reading_sector_1,
                                                                         errno,
                                                                         tapePartition.FirstBlock + doneSectors));

                                EndProgress2();

                                return;
                            }

                            UpdateProgress2(Localization.Core.Hashing_blocks_0_of_1, (long)doneSectors, (long)sectors);
                            doneSectors += sectors - doneSectors;
                        }

                        thisPartition.Size += (ulong)sector.LongLength;

                        tapePartitionChk.Update(sector);
                    }

                    // For fast debugging, skip checksum
                    //skipImageChecksum:

                    thisPartition.Checksums = tapePartitionChk.End();

                    EndProgress2();
                }

                List<TapeFile> filesInPartition = new();

                foreach(CommonTypes.Structs.TapeFile tapeFile in
                        tapeImage.Files.Where(f => f.Partition == tapePartition.Number))
                {
                    var thisFile = new TapeFile
                    {
                        Sequence   = tapeFile.File,
                        StartBlock = tapeFile.FirstBlock,
                        EndBlock   = tapeFile.LastBlock,
                        Image      = sidecar.BlockMedias[0].Image,
                        Size       = 0,
                        BlockSize  = 0
                    };

                    if(tapeImage.Files.Count(f => f.Partition == tapePartition.Number) == 1)
                    {
                        thisFile.Checksums = thisPartition.Checksums;
                        thisFile.Size      = thisPartition.Size;
                    }
                    else
                    {
                        UpdateStatus(string.Format(Localization.Core.Hashing_file_0, tapeFile.File));

                        if(_aborted)
                            return;

                        var tapeFileChk = new Checksum();

                        // For fast debugging, skip checksum
                        //goto skipImageChecksum;

                        const uint sectorsToRead = 64;
                        ulong      sectors       = tapeFile.LastBlock - tapeFile.FirstBlock + 1;
                        ulong      doneSectors   = 0;

                        InitProgress2();

                        while(doneSectors < sectors)
                        {
                            if(_aborted)
                            {
                                EndProgress2();

                                return;
                            }

                            byte[] sector;

                            if(sectors - doneSectors >= sectorsToRead)
                            {
                                errno = image.ReadSectors(tapeFile.FirstBlock + doneSectors, sectorsToRead, out sector);

                                if(errno != ErrorNumber.NoError)
                                {
                                    AaruConsole.ErrorWriteLine(string.Format(Localization.Core.Error_0_reading_sector_1,
                                                                             errno, tapeFile.FirstBlock + doneSectors));

                                    EndProgress2();

                                    return;
                                }

                                UpdateProgress2(Localization.Core.Hashing_blocks_0_of_1, (long)doneSectors,
                                                (long)sectors);

                                doneSectors += sectorsToRead;
                            }
                            else
                            {
                                errno = image.ReadSectors(tapeFile.FirstBlock + doneSectors,
                                                          (uint)(sectors - doneSectors), out sector);

                                if(errno != ErrorNumber.NoError)
                                {
                                    AaruConsole.ErrorWriteLine(string.Format(Localization.Core.Error_0_reading_sector_1,
                                                                             errno, tapeFile.FirstBlock + doneSectors));

                                    EndProgress2();

                                    return;
                                }

                                UpdateProgress2(Localization.Core.Hashing_blocks_0_of_1, (long)doneSectors,
                                                (long)sectors);

                                doneSectors += sectors - doneSectors;
                            }

                            if((ulong)sector.LongLength > thisFile.BlockSize)
                                thisFile.BlockSize = (ulong)sector.LongLength;

                            thisFile.Size += (ulong)sector.LongLength;

                            tapeFileChk.Update(sector);
                        }

                        // For fast debugging, skip checksum
                        //skipImageChecksum:

                        thisFile.Checksums = tapeFileChk.End();

                        EndProgress2();
                    }

                    filesInPartition.Add(thisFile);
                }

                thisPartition.Files = filesInPartition;
                tapePartitions.Add(thisPartition);
            }

            sidecar.BlockMedias[0].TapeInformation = tapePartitions;
        }

        UpdateStatus(Localization.Core.Checking_filesystems);

        if(_aborted)
            return;

        List<Partition> partitions = Partitions.GetAll(image);
        Partitions.AddSchemesToStats(partitions);

        sidecar.BlockMedias[0].FileSystemInformation = new List<CommonTypes.AaruMetadata.Partition>();

        if(partitions.Count > 0)
        {
            foreach(Partition partition in partitions)
            {
                if(_aborted)
                    return;

                var fsInfo = new CommonTypes.AaruMetadata.Partition
                {
                    Description = partition.Description,
                    EndSector   = partition.End,
                    Name        = partition.Name,
                    Sequence    = (uint)partition.Sequence,
                    StartSector = partition.Start,
                    Type        = partition.Type
                };

                List<FileSystem> lstFs = new();

                foreach(IFilesystem plugin in plugins.PluginsList.Values)
                    try
                    {
                        if(_aborted)
                            return;

                        if(!plugin.Identify(image, partition))
                            continue;

                        if(plugin is IReadOnlyFilesystem fsPlugin &&
                           fsPlugin.Mount(image, partition, encoding, null, null) == ErrorNumber.NoError)
                        {
                            UpdateStatus(string.Format(Localization.Core.Mounting_0, fsPlugin.Metadata.Type));

                            fsPlugin.Metadata.Contents = Files(fsPlugin);

                            fsPlugin.Unmount();
                        }
                        else
                            plugin.GetInformation(image, partition, out _, encoding);

                        lstFs.Add(plugin.Metadata);
                        Statistics.AddFilesystem(plugin.Metadata.Type);
                    }
                    #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                    catch
                        #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                    {
                        //AaruConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                    }

                if(lstFs.Count > 0)
                    fsInfo.FileSystems = lstFs;

                sidecar.BlockMedias[0].FileSystemInformation.Add(fsInfo);
            }
        }
        else
        {
            if(_aborted)
                return;

            var fsInfo = new CommonTypes.AaruMetadata.Partition
            {
                StartSector = 0,
                EndSector   = image.Info.Sectors - 1
            };

            var wholePart = new Partition
            {
                Name   = Localization.Core.Whole_device,
                Length = image.Info.Sectors,
                Size   = image.Info.Sectors * image.Info.SectorSize
            };

            List<FileSystem> lstFs = new();

            foreach(IFilesystem plugin in plugins.PluginsList.Values)
                try
                {
                    if(_aborted)
                        return;

                    if(!plugin.Identify(image, wholePart))
                        continue;

                    if(plugin is IReadOnlyFilesystem fsPlugin &&
                       fsPlugin.Mount(image, wholePart, encoding, null, null) == ErrorNumber.NoError)
                    {
                        UpdateStatus(string.Format(Localization.Core.Mounting_0, fsPlugin.Metadata.Type));

                        fsPlugin.Metadata.Contents = Files(fsPlugin);

                        fsPlugin.Unmount();
                    }
                    else
                        plugin.GetInformation(image, wholePart, out _, encoding);

                    lstFs.Add(plugin.Metadata);
                    Statistics.AddFilesystem(plugin.Metadata.Type);
                }
                #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
                    #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                {
                    //AaruConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                }

            if(lstFs.Count > 0)
                fsInfo.FileSystems = lstFs;

            sidecar.BlockMedias[0].FileSystemInformation.Add(fsInfo);
        }

        UpdateStatus(Localization.Core.Saving_metadata);

        if(image.Info.Cylinders > 0 &&
           image.Info is { Heads: > 0, SectorsPerTrack: > 0 })
        {
            sidecar.BlockMedias[0].Cylinders       = image.Info.Cylinders;
            sidecar.BlockMedias[0].Heads           = (ushort)image.Info.Heads;
            sidecar.BlockMedias[0].SectorsPerTrack = image.Info.SectorsPerTrack;
        }

        if(image.Info.ReadableMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
        {
            Identify.IdentifyDevice? ataId = null;
            errno = image.ReadMediaTag(MediaTagType.ATA_IDENTIFY, out buffer);

            if(errno == ErrorNumber.NoError)
                ataId = Identify.Decode(buffer);

            if(ataId.HasValue)
                if(ataId.Value.CurrentCylinders       > 0 &&
                   ataId.Value.CurrentHeads           > 0 &&
                   ataId.Value.CurrentSectorsPerTrack > 0)
                {
                    sidecar.BlockMedias[0].Cylinders       = ataId.Value.CurrentCylinders;
                    sidecar.BlockMedias[0].Heads           = ataId.Value.CurrentHeads;
                    sidecar.BlockMedias[0].SectorsPerTrack = ataId.Value.CurrentSectorsPerTrack;
                }
                else if(ataId.Value.Cylinders       > 0 &&
                        ataId.Value.Heads           > 0 &&
                        ataId.Value.SectorsPerTrack > 0)
                {
                    sidecar.BlockMedias[0].Cylinders       = ataId.Value.Cylinders;
                    sidecar.BlockMedias[0].Heads           = ataId.Value.Heads;
                    sidecar.BlockMedias[0].SectorsPerTrack = ataId.Value.SectorsPerTrack;
                }
        }

        sidecar.BlockMedias[0].DumpHardware = image.DumpHardware;

        // TODO: This is more of a hack, redo it planned for >4.0
        string trkFormat = null;

        switch(image.Info.MediaType)
        {
            case CommonTypes.MediaType.Apple32SS:
            case CommonTypes.MediaType.Apple32DS:
                trkFormat = "Apple GCR (DOS 3.2)";

                break;
            case CommonTypes.MediaType.Apple33SS:
            case CommonTypes.MediaType.Apple33DS:
                trkFormat = "Apple GCR (DOS 3.3)";

                break;
            case CommonTypes.MediaType.AppleSonySS:
            case CommonTypes.MediaType.AppleSonyDS:
                trkFormat = "Apple GCR (Sony)";

                break;
            case CommonTypes.MediaType.AppleFileWare:
                trkFormat = "Apple GCR (Twiggy)";

                break;
            case CommonTypes.MediaType.DOS_525_SS_DD_9:
            case CommonTypes.MediaType.DOS_525_DS_DD_8:
            case CommonTypes.MediaType.DOS_525_DS_DD_9:
            case CommonTypes.MediaType.DOS_525_HD:
            case CommonTypes.MediaType.DOS_35_SS_DD_8:
            case CommonTypes.MediaType.DOS_35_SS_DD_9:
            case CommonTypes.MediaType.DOS_35_DS_DD_8:
            case CommonTypes.MediaType.DOS_35_DS_DD_9:
            case CommonTypes.MediaType.DOS_35_HD:
            case CommonTypes.MediaType.DOS_35_ED:
            case CommonTypes.MediaType.DMF:
            case CommonTypes.MediaType.DMF_82:
            case CommonTypes.MediaType.XDF_525:
            case CommonTypes.MediaType.XDF_35:
            case CommonTypes.MediaType.IBM53FD_256:
            case CommonTypes.MediaType.IBM53FD_512:
            case CommonTypes.MediaType.IBM53FD_1024:
            case CommonTypes.MediaType.RX02:
            case CommonTypes.MediaType.RX03:
            case CommonTypes.MediaType.RX50:
            case CommonTypes.MediaType.ACORN_525_SS_DD_40:
            case CommonTypes.MediaType.ACORN_525_SS_DD_80:
            case CommonTypes.MediaType.ACORN_525_DS_DD:
            case CommonTypes.MediaType.ACORN_35_DS_DD:
            case CommonTypes.MediaType.ACORN_35_DS_HD:
            case CommonTypes.MediaType.ATARI_525_ED:
            case CommonTypes.MediaType.ATARI_525_DD:
            case CommonTypes.MediaType.ATARI_35_SS_DD:
            case CommonTypes.MediaType.ATARI_35_DS_DD:
            case CommonTypes.MediaType.ATARI_35_SS_DD_11:
            case CommonTypes.MediaType.ATARI_35_DS_DD_11:
            case CommonTypes.MediaType.DOS_525_SS_DD_8:
            case CommonTypes.MediaType.NEC_8_DD:
            case CommonTypes.MediaType.NEC_525_SS:
            case CommonTypes.MediaType.NEC_525_DS:
            case CommonTypes.MediaType.NEC_525_HD:
            case CommonTypes.MediaType.NEC_35_HD_8:
            case CommonTypes.MediaType.NEC_35_HD_15:
            case CommonTypes.MediaType.NEC_35_TD:
            case CommonTypes.MediaType.FDFORMAT_525_DD:
            case CommonTypes.MediaType.FDFORMAT_525_HD:
            case CommonTypes.MediaType.FDFORMAT_35_DD:
            case CommonTypes.MediaType.FDFORMAT_35_HD:
            case CommonTypes.MediaType.Apricot_35:
            case CommonTypes.MediaType.CompactFloppy:
            case CommonTypes.MediaType.MetaFloppy_Mod_I:
            case CommonTypes.MediaType.MetaFloppy_Mod_II:
                trkFormat = "IBM MFM";

                break;
            case CommonTypes.MediaType.ATARI_525_SD:
            case CommonTypes.MediaType.NEC_8_SD:
            case CommonTypes.MediaType.ACORN_525_SS_SD_40:
            case CommonTypes.MediaType.ACORN_525_SS_SD_80:
            case CommonTypes.MediaType.RX01:
            case CommonTypes.MediaType.IBM23FD:
            case CommonTypes.MediaType.IBM33FD_128:
            case CommonTypes.MediaType.IBM33FD_256:
            case CommonTypes.MediaType.IBM33FD_512:
            case CommonTypes.MediaType.IBM43FD_128:
            case CommonTypes.MediaType.IBM43FD_256:
                trkFormat = "IBM FM";

                break;
            case CommonTypes.MediaType.CBM_35_DD:
                trkFormat = "Commodore MFM";

                break;
            case CommonTypes.MediaType.CBM_AMIGA_35_HD:
            case CommonTypes.MediaType.CBM_AMIGA_35_DD:
                trkFormat = "Amiga MFM";

                break;
            case CommonTypes.MediaType.CBM_1540:
            case CommonTypes.MediaType.CBM_1540_Ext:
            case CommonTypes.MediaType.CBM_1571:
                trkFormat = "Commodore GCR";

                break;
            case CommonTypes.MediaType.SHARP_525_9:
            case CommonTypes.MediaType.SHARP_35_9: break;
            case CommonTypes.MediaType.ECMA_99_15:
            case CommonTypes.MediaType.ECMA_99_26:
            case CommonTypes.MediaType.ECMA_99_8:
                trkFormat = "ISO MFM";

                break;
            case CommonTypes.MediaType.ECMA_54:
            case CommonTypes.MediaType.ECMA_59:
            case CommonTypes.MediaType.ECMA_66:
            case CommonTypes.MediaType.ECMA_69_8:
            case CommonTypes.MediaType.ECMA_69_15:
            case CommonTypes.MediaType.ECMA_69_26:
            case CommonTypes.MediaType.ECMA_70:
            case CommonTypes.MediaType.ECMA_78:
            case CommonTypes.MediaType.ECMA_78_2:
                trkFormat = "ISO FM";

                break;
            default:
                trkFormat = "Unknown";

                break;
        }

        #region SuperCardPro
        string scpFilePath = Path.Combine(Path.GetDirectoryName(imagePath),
                                          Path.GetFileNameWithoutExtension(imagePath) + ".scp");

        if(_aborted)
            return;

        if(File.Exists(scpFilePath))
        {
            UpdateStatus(Localization.Core.Hashing_SuperCardPro_image);
            var scpImage  = new SuperCardPro();
            var scpFilter = new ZZZNoFilter();
            scpFilter.Open(scpFilePath);

            if(image.Info.Heads <= 2 &&
               scpImage.Identify(scpFilter))
            {
                try
                {
                    scpImage.Open(scpFilter);
                }
                catch(NotImplementedException) {}

                if((image.Info.Heads == 2 && scpImage.Header.heads == 0) ||
                   (image.Info.Heads == 1 && scpImage.Header.heads is 1 or 2))
                    if(scpImage.Header.end + 1 >= image.Info.Cylinders)
                    {
                        List<BlockTrack> scpBlockTrackTypes = new();
                        ulong            currentSector      = 0;
                        Stream           scpStream          = scpFilter.GetDataForkStream();

                        for(byte t = scpImage.Header.start; t <= scpImage.Header.end; t++)
                        {
                            if(_aborted)
                                return;

                            var scpBlockTrackType = new BlockTrack
                            {
                                Cylinder = t / image.Info.Heads,
                                Head     = (ushort)(t % image.Info.Heads),
                                Image = new Image
                                {
                                    Format = scpImage.Format,
                                    Value  = Path.GetFileName(scpFilePath),
                                    Offset = scpImage.Header.offsets[t]
                                }
                            };

                            if(scpBlockTrackType.Cylinder < image.Info.Cylinders)
                            {
                                scpBlockTrackType.StartSector    =  currentSector;
                                currentSector                    += image.Info.SectorsPerTrack;
                                scpBlockTrackType.EndSector      =  currentSector - 1;
                                scpBlockTrackType.Sectors        =  image.Info.SectorsPerTrack;
                                scpBlockTrackType.BytesPerSector =  image.Info.SectorSize;
                                scpBlockTrackType.Format         =  trkFormat;
                            }

                            if(scpImage.ScpTracks.TryGetValue(t, out SuperCardPro.TrackHeader scpTrack))
                            {
                                byte[] trackContents =
                                    new byte[scpTrack.Entries.Last().dataOffset + scpTrack.Entries.Last().trackLength -
                                             scpImage.Header.offsets[t] + 1];

                                scpStream.Position = scpImage.Header.offsets[t];
                                scpStream.EnsureRead(trackContents, 0, trackContents.Length);
                                scpBlockTrackType.Size      = (ulong)trackContents.Length;
                                scpBlockTrackType.Checksums = Checksum.GetChecksums(trackContents);
                            }

                            scpBlockTrackTypes.Add(scpBlockTrackType);
                        }

                        sidecar.BlockMedias[0].Track = scpBlockTrackTypes.OrderBy(t => t.Cylinder).
                                                                          ThenBy(t => t.Head).ToList();
                    }
                    else
                        AaruConsole.
                            ErrorWriteLine(Localization.Core.SCP_image_do_not_same_number_tracks_0_disk_image_1_ignoring,
                                           scpImage.Header.end + 1, image.Info.Cylinders);
                else
                    AaruConsole.
                        ErrorWriteLine(Localization.Core.SCP_image_do_not_same_number_heads_0_disk_image_1_ignoring, 2,
                                       image.Info.Heads);
            }
        }
        #endregion

        #region KryoFlux
        string kfFile = null;

        string basename = Path.Combine(Path.GetDirectoryName(imagePath), Path.GetFileNameWithoutExtension(imagePath));

        bool kfDir = false;

        if(_aborted)
            return;

        if(Directory.Exists(basename))
        {
            string[] possibleKfStarts = Directory.GetFiles(basename, "*.raw", SearchOption.TopDirectoryOnly);

            if(possibleKfStarts.Length > 0)
            {
                kfFile = possibleKfStarts[0];
                kfDir  = true;
            }
        }
        else if(File.Exists(basename + "00.0.raw"))
            kfFile = basename + "00.0.raw";
        else if(File.Exists(basename + "00.1.raw"))
            kfFile = basename + "00.1.raw";

        if(kfFile != null)
        {
            UpdateStatus(Localization.Core.Hashing_KryoFlux_images);

            var kfImage  = new KryoFlux();
            var kfFilter = new ZZZNoFilter();
            kfFilter.Open(kfFile);

            if(image.Info.Heads <= 2 &&
               kfImage.Identify(kfFilter))
            {
                try
                {
                    kfImage.Open(kfFilter);
                }
                catch(NotImplementedException) {}

                if(kfImage.Info.Heads == image.Info.Heads)
                    if(kfImage.Info.Cylinders >= image.Info.Cylinders)
                    {
                        List<BlockTrack> kfBlockTrackTypes = new();

                        ulong currentSector = 0;

                        foreach(KeyValuePair<byte, IFilter> kvp in kfImage.tracks)
                        {
                            if(_aborted)
                                return;

                            var kfBlockTrackType = new BlockTrack
                            {
                                Cylinder = kvp.Key / image.Info.Heads,
                                Head     = (ushort)(kvp.Key % image.Info.Heads),
                                Image = new Image
                                {
                                    Format = kfImage.Format,
                                    Value = kfDir
                                                ? Path.
                                                    Combine(Path.GetFileName(Path.GetDirectoryName(kvp.Value.BasePath)),
                                                            kvp.Value.Filename) : kvp.Value.Filename,
                                    Offset = 0
                                }
                            };

                            if(kfBlockTrackType.Cylinder < image.Info.Cylinders)
                            {
                                kfBlockTrackType.StartSector    =  currentSector;
                                currentSector                   += image.Info.SectorsPerTrack;
                                kfBlockTrackType.EndSector      =  currentSector - 1;
                                kfBlockTrackType.Sectors        =  image.Info.SectorsPerTrack;
                                kfBlockTrackType.BytesPerSector =  image.Info.SectorSize;
                                kfBlockTrackType.Format         =  trkFormat;
                            }

                            Stream kfStream      = kvp.Value.GetDataForkStream();
                            byte[] trackContents = new byte[kfStream.Length];
                            kfStream.Position = 0;
                            kfStream.EnsureRead(trackContents, 0, trackContents.Length);
                            kfBlockTrackType.Size      = (ulong)trackContents.Length;
                            kfBlockTrackType.Checksums = Checksum.GetChecksums(trackContents);

                            kfBlockTrackTypes.Add(kfBlockTrackType);
                        }

                        sidecar.BlockMedias[0].Track = kfBlockTrackTypes.OrderBy(t => t.Cylinder).
                                                                         ThenBy(t => t.Head).ToList();
                    }
                    else
                        AaruConsole.
                            ErrorWriteLine(Localization.Core.KryoFlux_image_do_not_same_number_tracks_0_disk_image_1_ignoring,
                                           kfImage.Info.Cylinders, image.Info.Cylinders);
                else
                    AaruConsole.
                        ErrorWriteLine(Localization.Core.KryoFlux_image_do_not_same_number_heads_0_disk_image_1_ignoring,
                                       kfImage.Info.Heads, image.Info.Heads);
            }
        }
        #endregion

        #region DiscFerret
        string dfiFilePath = Path.Combine(Path.GetDirectoryName(imagePath),
                                          Path.GetFileNameWithoutExtension(imagePath) + ".dfi");

        if(_aborted)
            return;

        if(!File.Exists(dfiFilePath))
            return;

        var dfiImage  = new DiscFerret();
        var dfiFilter = new ZZZNoFilter();
        dfiFilter.Open(dfiFilePath);

        if(!dfiImage.Identify(dfiFilter))
            return;

        try
        {
            dfiImage.Open(dfiFilter);
        }
        catch(NotImplementedException) {}

        UpdateStatus(Localization.Core.Hashing_DiscFerret_image);

        if(image.Info.Heads == dfiImage.Info.Heads)
            if(dfiImage.Info.Cylinders >= image.Info.Cylinders)
            {
                List<BlockTrack> dfiBlockTrackTypes = new();
                ulong            currentSector      = 0;
                Stream           dfiStream          = dfiFilter.GetDataForkStream();

                foreach(int t in dfiImage.TrackOffsets.Keys)
                {
                    if(_aborted)
                        return;

                    var dfiBlockTrackType = new BlockTrack
                    {
                        Cylinder = (uint)(t   / image.Info.Heads),
                        Head     = (ushort)(t % image.Info.Heads),
                        Image = new Image
                        {
                            Format = dfiImage.Format,
                            Value  = Path.GetFileName(dfiFilePath)
                        }
                    };

                    if(dfiBlockTrackType.Cylinder < image.Info.Cylinders)
                    {
                        dfiBlockTrackType.StartSector    =  currentSector;
                        currentSector                    += image.Info.SectorsPerTrack;
                        dfiBlockTrackType.EndSector      =  currentSector - 1;
                        dfiBlockTrackType.Sectors        =  image.Info.SectorsPerTrack;
                        dfiBlockTrackType.BytesPerSector =  image.Info.SectorSize;
                        dfiBlockTrackType.Format         =  trkFormat;
                    }

                    if(dfiImage.TrackOffsets.TryGetValue(t, out long offset) &&
                       dfiImage.TrackLengths.TryGetValue(t, out long length))
                    {
                        dfiBlockTrackType.Image.Offset = (ulong)offset;
                        byte[] trackContents = new byte[length];
                        dfiStream.Position = offset;
                        dfiStream.EnsureRead(trackContents, 0, trackContents.Length);
                        dfiBlockTrackType.Size      = (ulong)trackContents.Length;
                        dfiBlockTrackType.Checksums = Checksum.GetChecksums(trackContents);
                    }

                    dfiBlockTrackTypes.Add(dfiBlockTrackType);
                }

                sidecar.BlockMedias[0].Track = dfiBlockTrackTypes.OrderBy(t => t.Cylinder).ThenBy(t => t.Head).ToList();
            }
            else
                AaruConsole.
                    ErrorWriteLine(Localization.Core.DiscFerret_image_do_not_same_number_tracks_0_disk_image_1_ignoring,
                                   dfiImage.Info.Cylinders, image.Info.Cylinders);
        else
            AaruConsole.
                ErrorWriteLine(Localization.Core.DiscFerret_image_do_not_same_number_heads_0_disk_image_1_ignoring,
                               dfiImage.Info.Heads, image.Info.Heads);
        #endregion

        // TODO: Implement support for getting CHS from SCSI mode pages
    }
}