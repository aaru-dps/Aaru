// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using Schemas;
using Tuple = DiscImageChef.Decoders.PCMCIA.Tuple;

namespace DiscImageChef.Core
{
    public static partial class Sidecar
    {
        static void BlockMedia(ImagePlugin image, System.Guid filterId, string imagePath, FileInfo fi, PluginBase plugins, List<ChecksumType> imgChecksums, ref CICMMetadataType sidecar)
        {
            sidecar.BlockMedia = new[]
            {
	            new BlockMediaType
	            {
	                Checksums = imgChecksums.ToArray(),
	                Image = new ImageType
	                {
	                    format = image.GetImageFormat(),
	                    offset = 0,
	                    offsetSpecified = true,
	                    Value = Path.GetFileName(imagePath)
	                },
	                Size = fi.Length,
	                Sequence = new SequenceType
	                {
	                    MediaTitle = image.GetImageName()
	                }
                }
            };

            if(image.GetMediaSequence() != 0 && image.GetLastDiskSequence() != 0)
            {
                sidecar.BlockMedia[0].Sequence.MediaSequence = image.GetMediaSequence();
                sidecar.BlockMedia[0].Sequence.TotalMedia = image.GetMediaSequence();
            }
            else
            {
                sidecar.BlockMedia[0].Sequence.MediaSequence = 1;
                sidecar.BlockMedia[0].Sequence.TotalMedia = 1;
            }

            foreach(MediaTagType tagType in image.ImageInfo.readableMediaTags)
            {
                switch(tagType)
                {
                    case MediaTagType.ATAPI_IDENTIFY:
                        sidecar.BlockMedia[0].ATA = new ATAType
                        {
                            Identify = new DumpType
                            {
                                Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.ATAPI_IDENTIFY)).ToArray(),
                                Size = image.ReadDiskTag(MediaTagType.ATAPI_IDENTIFY).Length
                            }
                        };
                        break;
                    case MediaTagType.ATA_IDENTIFY:
                        sidecar.BlockMedia[0].ATA = new ATAType
                        {
                            Identify = new DumpType
                            {
                                Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.ATA_IDENTIFY)).ToArray(),
                                Size = image.ReadDiskTag(MediaTagType.ATA_IDENTIFY).Length
                            }
                        };
                        break;
                    case MediaTagType.PCMCIA_CIS:
                        byte[] cis = image.ReadDiskTag(MediaTagType.PCMCIA_CIS);
                        sidecar.BlockMedia[0].PCMCIA = new PCMCIAType
                        {
                            CIS = new DumpType
                            {
                                Checksums = Checksum.GetChecksums(cis).ToArray(),
                                Size = cis.Length
                            }
                        };
                        Tuple[] tuples = CIS.GetTuples(cis);
                        if(tuples != null)
                        {
                            foreach(Tuple tuple in tuples)
                            {
                                if(tuple.Code == TupleCodes.CISTPL_MANFID)
                                {
                                    ManufacturerIdentificationTuple manfid = CIS.DecodeManufacturerIdentificationTuple(tuple);

                                    if(manfid != null)
                                    {
                                        sidecar.BlockMedia[0].PCMCIA.ManufacturerCode = manfid.ManufacturerID;
                                        sidecar.BlockMedia[0].PCMCIA.CardCode = manfid.CardID;
                                        sidecar.BlockMedia[0].PCMCIA.ManufacturerCodeSpecified = true;
                                        sidecar.BlockMedia[0].PCMCIA.CardCodeSpecified = true;
                                    }
                                }
                                else if(tuple.Code == TupleCodes.CISTPL_VERS_1)
                                {
                                    Level1VersionTuple vers = CIS.DecodeLevel1VersionTuple(tuple);

                                    if(vers != null)
                                    {
                                        sidecar.BlockMedia[0].PCMCIA.Manufacturer = vers.Manufacturer;
                                        sidecar.BlockMedia[0].PCMCIA.ProductName = vers.Product;
                                        sidecar.BlockMedia[0].PCMCIA.Compliance = string.Format("{0}.{1}", vers.MajorVersion, vers.MinorVersion);
                                        sidecar.BlockMedia[0].PCMCIA.AdditionalInformation = vers.AdditionalInformation;
                                    }
                                }
                            }
                        }
                        break;
                    case MediaTagType.SCSI_INQUIRY:
                        sidecar.BlockMedia[0].SCSI = new SCSIType
                        {
                            Inquiry = new DumpType
                            {
                                Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.SCSI_INQUIRY)).ToArray(),
                                Size = image.ReadDiskTag(MediaTagType.SCSI_INQUIRY).Length
                            }
                        };
                        break;
                    case MediaTagType.SD_CID:
                        if(sidecar.BlockMedia[0].SecureDigital == null)
                            sidecar.BlockMedia[0].SecureDigital = new SecureDigitalType();
                        sidecar.BlockMedia[0].SecureDigital.CID = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.SD_CID)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.SD_CID).Length
                        };
                        break;
                    case MediaTagType.SD_CSD:
                        if(sidecar.BlockMedia[0].SecureDigital == null)
                            sidecar.BlockMedia[0].SecureDigital = new SecureDigitalType();
                        sidecar.BlockMedia[0].SecureDigital.CSD = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.SD_CSD)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.SD_CSD).Length
                        };
                        break;
                    case MediaTagType.SD_SCR:
                        if(sidecar.BlockMedia[0].SecureDigital == null)
                            sidecar.BlockMedia[0].SecureDigital = new SecureDigitalType();
                        sidecar.BlockMedia[0].SecureDigital.SCR = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.SD_SCR)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.SD_SCR).Length
                        };
                        break;
                    case MediaTagType.SD_OCR:
                        if(sidecar.BlockMedia[0].SecureDigital == null)
                            sidecar.BlockMedia[0].SecureDigital = new SecureDigitalType();
                        sidecar.BlockMedia[0].SecureDigital.OCR = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.SD_OCR)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.SD_OCR).Length
                        };
                        break;
                    case MediaTagType.MMC_CID:
                        if(sidecar.BlockMedia[0].MultiMediaCard == null)
                            sidecar.BlockMedia[0].MultiMediaCard = new MultiMediaCardType();
                        sidecar.BlockMedia[0].MultiMediaCard.CID = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.SD_CID)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.SD_CID).Length
                        };
                        break;
                    case MediaTagType.MMC_CSD:
                        if(sidecar.BlockMedia[0].MultiMediaCard == null)
                            sidecar.BlockMedia[0].MultiMediaCard = new MultiMediaCardType();
                        sidecar.BlockMedia[0].MultiMediaCard.CSD = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.SD_CSD)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.SD_CSD).Length
                        };
                        break;
                    case MediaTagType.MMC_OCR:
                        if(sidecar.BlockMedia[0].MultiMediaCard == null)
                            sidecar.BlockMedia[0].MultiMediaCard = new MultiMediaCardType();
                        sidecar.BlockMedia[0].MultiMediaCard.OCR = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.SD_OCR)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.SD_OCR).Length
                        };
                        break;
                    case MediaTagType.MMC_ExtendedCSD:
                        if(sidecar.BlockMedia[0].MultiMediaCard == null)
                            sidecar.BlockMedia[0].MultiMediaCard = new MultiMediaCardType();
                        sidecar.BlockMedia[0].MultiMediaCard.ExtendedCSD = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.MMC_ExtendedCSD)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.MMC_ExtendedCSD).Length
                        };
                        break;
                }
            }

            // If there is only one track, and it's the same as the image file (e.g. ".iso" files), don't re-checksum.
            if(image.PluginUUID == new System.Guid("12345678-AAAA-BBBB-CCCC-123456789000") &&
               filterId == new System.Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
            {
                sidecar.BlockMedia[0].ContentChecksums = sidecar.BlockMedia[0].Checksums;
            }
            else
            {
                Checksum contentChkWorker = new Checksum();

                // For fast debugging, skip checksum
                //goto skipImageChecksum;

                uint sectorsToRead = 512;
                ulong sectors = image.GetSectors();
                ulong doneSectors = 0;

                InitProgress2();
                while(doneSectors < sectors)
                {
                    byte[] sector;

                    if((sectors - doneSectors) >= sectorsToRead)
                    {
                        sector = image.ReadSectors(doneSectors, sectorsToRead);
                        UpdateProgress2("Hashings sector {0} of {1}", (long)doneSectors, (long)sectors);
                        doneSectors += sectorsToRead;
                    }
                    else
                    {
                        sector = image.ReadSectors(doneSectors, (uint)(sectors - doneSectors));
                        UpdateProgress2("Hashings sector {0} of {1}", (long)doneSectors, (long)sectors);
                        doneSectors += (sectors - doneSectors);
                    }

                    contentChkWorker.Update(sector);
                }

                // For fast debugging, skip checksum
                //skipImageChecksum:

                List<ChecksumType> cntChecksums = contentChkWorker.End();

                sidecar.BlockMedia[0].ContentChecksums = cntChecksums.ToArray();

                EndProgress2();
            }

            Metadata.MediaType.MediaTypeToString(image.ImageInfo.mediaType, out string dskType, out string dskSubType);
            sidecar.BlockMedia[0].DiskType = dskType;
            sidecar.BlockMedia[0].DiskSubType = dskSubType;
            Statistics.AddMedia(image.ImageInfo.mediaType, false);

            sidecar.BlockMedia[0].Dimensions = Metadata.Dimensions.DimensionsFromMediaType(image.ImageInfo.mediaType);

            sidecar.BlockMedia[0].LogicalBlocks = (long)image.GetSectors();
            sidecar.BlockMedia[0].LogicalBlockSize = (int)image.GetSectorSize();
            // TODO: Detect it
            sidecar.BlockMedia[0].PhysicalBlockSize = (int)image.GetSectorSize();

            UpdateStatus("Checking filesystems...");

            List<Partition> partitions = Partitions.GetAll(image);
            Partitions.AddSchemesToStats(partitions);

            sidecar.BlockMedia[0].FileSystemInformation = new PartitionType[1];
            if(partitions.Count > 0)
            {
                sidecar.BlockMedia[0].FileSystemInformation = new PartitionType[partitions.Count];
                for(int i = 0; i < partitions.Count; i++)
                {
                    sidecar.BlockMedia[0].FileSystemInformation[i] = new PartitionType
                    {
                        Description = partitions[i].Description,
                        EndSector = (int)(partitions[i].End),
                        Name = partitions[i].Name,
                        Sequence = (int)partitions[i].Sequence,
                        StartSector = (int)partitions[i].Start,
                        Type = partitions[i].Type
                    };
                    List<FileSystemType> lstFs = new List<FileSystemType>();

                    foreach(Filesystem _plugin in plugins.PluginsList.Values)
                    {
                        try
                        {
                            if(_plugin.Identify(image, partitions[i]))
                            {
                                _plugin.GetInformation(image, partitions[i], out string foo);
                                lstFs.Add(_plugin.XmlFSType);
                                Statistics.AddFilesystem(_plugin.XmlFSType.Type);
                            }
                        }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                        {
                            //DicConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                        }
                    }

                    if(lstFs.Count > 0)
                        sidecar.BlockMedia[0].FileSystemInformation[i].FileSystems = lstFs.ToArray();
                }
            }
            else
            {
                sidecar.BlockMedia[0].FileSystemInformation[0] = new PartitionType
                {
                    StartSector = 0,
                    EndSector = (int)(image.GetSectors() - 1)
                };

                Partition wholePart = new Partition
                {
                    Name = "Whole device",
                    Length = image.GetSectors(),
                    Size = image.GetSectors() * image.GetSectorSize()
                };

                List<FileSystemType> lstFs = new List<FileSystemType>();

                foreach(Filesystem _plugin in plugins.PluginsList.Values)
                {
                    try
                    {
                        if(_plugin.Identify(image, wholePart))
                        {
                            _plugin.GetInformation(image, wholePart, out string foo);
                            lstFs.Add(_plugin.XmlFSType);
                            Statistics.AddFilesystem(_plugin.XmlFSType.Type);
                        }
                    }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                    catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                    {
                        //DicConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                    }
                }

                if(lstFs.Count > 0)
                    sidecar.BlockMedia[0].FileSystemInformation[0].FileSystems = lstFs.ToArray();
            }

            if(image.ImageInfo.cylinders > 0 && image.ImageInfo.heads > 0 && image.ImageInfo.sectorsPerTrack > 0)
            {
                sidecar.BlockMedia[0].CylindersSpecified = true;
                sidecar.BlockMedia[0].HeadsSpecified = true;
                sidecar.BlockMedia[0].SectorsPerTrackSpecified = true;
                sidecar.BlockMedia[0].Cylinders = image.ImageInfo.cylinders;
                sidecar.BlockMedia[0].Heads = image.ImageInfo.heads;
                sidecar.BlockMedia[0].SectorsPerTrack = image.ImageInfo.sectorsPerTrack;
            }

            if(image.ImageInfo.readableMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
            {
                Decoders.ATA.Identify.IdentifyDevice? ataId = Decoders.ATA.Identify.Decode(image.ReadDiskTag(MediaTagType.ATA_IDENTIFY));
                if(ataId.HasValue)
                {
                    if(ataId.Value.CurrentCylinders > 0 && ataId.Value.CurrentHeads > 0 && ataId.Value.CurrentSectorsPerTrack > 0)
                    {
                        sidecar.BlockMedia[0].CylindersSpecified = true;
                        sidecar.BlockMedia[0].HeadsSpecified = true;
                        sidecar.BlockMedia[0].SectorsPerTrackSpecified = true;
                        sidecar.BlockMedia[0].Cylinders = ataId.Value.CurrentCylinders;
                        sidecar.BlockMedia[0].Heads = ataId.Value.CurrentHeads;
                        sidecar.BlockMedia[0].SectorsPerTrack = ataId.Value.CurrentSectorsPerTrack;
                    }
                    else if(ataId.Value.Cylinders > 0 && ataId.Value.Heads > 0 && ataId.Value.SectorsPerTrack > 0)
                    {
                        sidecar.BlockMedia[0].CylindersSpecified = true;
                        sidecar.BlockMedia[0].HeadsSpecified = true;
                        sidecar.BlockMedia[0].SectorsPerTrackSpecified = true;
                        sidecar.BlockMedia[0].Cylinders = ataId.Value.Cylinders;
                        sidecar.BlockMedia[0].Heads = ataId.Value.Heads;
                        sidecar.BlockMedia[0].SectorsPerTrack = ataId.Value.SectorsPerTrack;
                    }
                }
            }

            // TODO: This is more of a hack, redo it planned for >4.0
            string trkFormat = null;
            
            switch(image.ImageInfo.mediaType)
            {
                case MediaType.Apple32SS:
                case MediaType.Apple32DS:
                    trkFormat = "Apple GCR (DOS 3.2)";
                    break;
                case MediaType.Apple33SS:
                case MediaType.Apple33DS:
                    trkFormat = "Apple GCR (DOS 3.3)";
                    break;
                case MediaType.AppleSonySS:
                case MediaType.AppleSonyDS:
                    trkFormat = "Apple GCR (Sony)";
                    break;
                case MediaType.AppleFileWare:
                    trkFormat = "Apple GCR (Twiggy)";
                    break;
                case MediaType.DOS_525_SS_DD_9:
                case MediaType.DOS_525_DS_DD_8:
                case MediaType.DOS_525_DS_DD_9:
                case MediaType.DOS_525_HD:
                case MediaType.DOS_35_SS_DD_8:
                case MediaType.DOS_35_SS_DD_9:
                case MediaType.DOS_35_DS_DD_8:
                case MediaType.DOS_35_DS_DD_9:
                case MediaType.DOS_35_HD:
                case MediaType.DOS_35_ED:
                case MediaType.DMF:
                case MediaType.DMF_82:
                case MediaType.XDF_525:
                case MediaType.XDF_35:
                case MediaType.IBM53FD_256:
                case MediaType.IBM53FD_512:
                case MediaType.IBM53FD_1024:
                case MediaType.RX02:
                case MediaType.RX03:
                case MediaType.RX50:
                case MediaType.ACORN_525_SS_DD_40:
                case MediaType.ACORN_525_SS_DD_80:
                case MediaType.ACORN_525_DS_DD:
                case MediaType.ACORN_35_DS_DD:
                case MediaType.ACORN_35_DS_HD:
                case MediaType.ATARI_525_ED:
                case MediaType.ATARI_525_DD:
                case MediaType.ATARI_35_SS_DD:
                case MediaType.ATARI_35_DS_DD:
                case MediaType.ATARI_35_SS_DD_11:
                case MediaType.ATARI_35_DS_DD_11:
                case MediaType.DOS_525_SS_DD_8:
                case MediaType.NEC_8_DD:
                case MediaType.NEC_525_SS:
                case MediaType.NEC_525_DS:
                case MediaType.NEC_525_HD:
                case MediaType.NEC_35_HD_8:
                case MediaType.NEC_35_HD_15:
                case MediaType.NEC_35_TD:
                case MediaType.FDFORMAT_525_DD:
                case MediaType.FDFORMAT_525_HD:
                case MediaType.FDFORMAT_35_DD:
                case MediaType.FDFORMAT_35_HD:
                case MediaType.Apricot_35:
                case MediaType.CompactFloppy:
                    trkFormat = "IBM MFM";
                    break;
                case MediaType.ATARI_525_SD:
                case MediaType.NEC_8_SD:
                case MediaType.ACORN_525_SS_SD_40:
                case MediaType.ACORN_525_SS_SD_80:
                case MediaType.RX01:
                case MediaType.IBM23FD:
                case MediaType.IBM33FD_128:
                case MediaType.IBM33FD_256:
                case MediaType.IBM33FD_512:
                case MediaType.IBM43FD_128:
                case MediaType.IBM43FD_256:
                    trkFormat = "IBM FM";
                    break;
                case MediaType.CBM_35_DD:
                    trkFormat = "Commodore MFM";
                    break;
                case MediaType.CBM_AMIGA_35_HD:
                case MediaType.CBM_AMIGA_35_DD:
                    trkFormat = "Amiga MFM";
                    break;
                case MediaType.CBM_1540:
                case MediaType.CBM_1540_Ext:
                case MediaType.CBM_1571:
                    trkFormat = "Commodore GCR";
                    break;
                case MediaType.SHARP_525:
                case MediaType.SHARP_525_9:
                case MediaType.SHARP_35:
                    break;
                case MediaType.SHARP_35_9:
                    break;
                case MediaType.ECMA_99_15:
                case MediaType.ECMA_99_26:
                case MediaType.ECMA_100:
                case MediaType.ECMA_125:
                case MediaType.ECMA_147:
                case MediaType.ECMA_99_8:
                    trkFormat = "ISO MFM";
                    break;
                case MediaType.ECMA_54:
                case MediaType.ECMA_59:
                case MediaType.ECMA_66:
                case MediaType.ECMA_69_8:
                case MediaType.ECMA_69_15:
                case MediaType.ECMA_69_26:
                case MediaType.ECMA_70:
                case MediaType.ECMA_78:
                case MediaType.ECMA_78_2:
                    trkFormat = "ISO FM";
                    break;
                default:
                    trkFormat = "Unknown";
                    break;
            }

            #region SuperCardPro
            string scpFilePath = Path.Combine(Path.GetDirectoryName(imagePath),
                Path.GetFileNameWithoutExtension(imagePath) + ".scp");

            if(File.Exists(scpFilePath))
            {
                ImagePlugins.SuperCardPro scpImage = new SuperCardPro();
                Filters.ZZZNoFilter scpFilter = new ZZZNoFilter();
                scpFilter.Open(scpFilePath);

                if(image.ImageInfo.heads <= 2 && scpImage.IdentifyImage(scpFilter))
                {

                    try
                    {
                        scpImage.OpenImage(scpFilter);
                    }
                    catch(NotImplementedException)
                    {
                    }

                    if((image.ImageInfo.heads == 2 && scpImage.header.heads == 0) ||
                       (image.ImageInfo.heads == 1 && (scpImage.header.heads == 1 || scpImage.header.heads == 2)))
                    {
                        if(scpImage.header.end + 1 >= image.ImageInfo.cylinders)
                        {
                            List<BlockTrackType> scpBlockTrackTypes = new List<BlockTrackType>();
                            long currentSector = 0;
                            Stream scpStream = scpFilter.GetDataForkStream();

                            for(byte t = scpImage.header.start; t <= scpImage.header.end; t++)
                            {
                                BlockTrackType scpBlockTrackType = new BlockTrackType();
                                scpBlockTrackType.Cylinder = t / image.ImageInfo.heads;
                                scpBlockTrackType.Head = t % image.ImageInfo.heads;
                                scpBlockTrackType.Image = new ImageType();
                                scpBlockTrackType.Image.format = scpImage.GetImageFormat();
                                scpBlockTrackType.Image.Value = Path.GetFileName(scpFilePath);
                                scpBlockTrackType.Image.offset = scpImage.header.offsets[t];

                                if(scpBlockTrackType.Cylinder < image.ImageInfo.cylinders)
                                {
                                    scpBlockTrackType.StartSector = currentSector;
                                    currentSector += image.ImageInfo.sectorsPerTrack;
                                    scpBlockTrackType.EndSector = currentSector - 1;
                                    scpBlockTrackType.Sectors = image.ImageInfo.sectorsPerTrack;
                                    scpBlockTrackType.BytesPerSector = (int)image.ImageInfo.sectorSize;
                                    scpBlockTrackType.Format = trkFormat;
                                }

                                if(scpImage.tracks.TryGetValue(t, out SuperCardPro.TrackHeader scpTrack))
                                {
                                    byte[] trackContents =
                                        new byte[(scpTrack.entries.Last().dataOffset +
                                                  scpTrack.entries.Last().trackLength) - scpImage.header.offsets[t] +
                                                 1];
                                    scpStream.Position = scpImage.header.offsets[t];
                                    scpStream.Read(trackContents, 0, trackContents.Length);
                                    scpBlockTrackType.Size = trackContents.Length;
                                    scpBlockTrackType.Checksums = Checksum.GetChecksums(trackContents).ToArray();
                                }

                                scpBlockTrackTypes.Add(scpBlockTrackType);
                            }

                            sidecar.BlockMedia[0].Track =
                                scpBlockTrackTypes.OrderBy(t => t.Cylinder).ThenBy(t => t.Head).ToArray();
                        }
                        else
                            DicConsole.ErrorWriteLine(
                                "SuperCardPro image do not contain same number of tracks ({0}) than disk image ({1}), ignoring...",
                                scpImage.header.end + 1, image.ImageInfo.cylinders);
                    }
                    else
                        DicConsole.ErrorWriteLine(
                            "SuperCardPro image do not contain same number of heads ({0}) than disk image ({1}), ignoring...",
                            2, image.ImageInfo.heads);
                }
            }
            #endregion
            
            #region KryoFlux
            string kfFile = null;
            string basename = Path.Combine(Path.GetDirectoryName(imagePath),
                Path.GetFileNameWithoutExtension(imagePath));
            bool kfDir = false;

            if(Directory.Exists(basename))
            {
                string[] possibleKfStarts = Directory.GetFiles(basename, "*.raw", SearchOption.TopDirectoryOnly);
                if(possibleKfStarts.Length > 0)
                {
                    kfFile = possibleKfStarts[0];
                    kfDir = true;
                }
            }
            else if(File.Exists(basename + "00.0.raw"))
                kfFile = basename + "00.0.raw";
            else if(File.Exists(basename + "00.1.raw"))
                kfFile = basename + "00.1.raw";

            if(kfFile != null)
            {
                ImagePlugins.KryoFlux kfImage = new KryoFlux();
                Filters.ZZZNoFilter kfFilter = new ZZZNoFilter();
                kfFilter.Open(kfFile);
                if(image.ImageInfo.heads <= 2 && kfImage.IdentifyImage(kfFilter))
                {
                    try
                    {
                        kfImage.OpenImage(kfFilter);
                    }
                    catch(NotImplementedException)
                    {
                    }

                    if(kfImage.ImageInfo.heads == image.ImageInfo.heads)
                    {
                        if(kfImage.ImageInfo.cylinders >= image.ImageInfo.cylinders)
                        {
                            List<BlockTrackType> kfBlockTrackTypes = new List<BlockTrackType>();
                            
                            long currentSector = 0;

                            foreach(KeyValuePair<byte, Filter> kvp in kfImage.tracks)
                            {
                                BlockTrackType kfBlockTrackType = new BlockTrackType();
                                kfBlockTrackType.Cylinder = kvp.Key / image.ImageInfo.heads;
                                kfBlockTrackType.Head = kvp.Key % image.ImageInfo.heads;
                                kfBlockTrackType.Image = new ImageType();
                                kfBlockTrackType.Image.format = kfImage.GetImageFormat();
                                kfBlockTrackType.Image.Value =
                                    kfDir
                                        ? Path.Combine(Path.GetFileName(Path.GetDirectoryName(kvp.Value.GetBasePath())), kvp.Value.GetFilename())
                                        : kvp.Value.GetFilename();
                                kfBlockTrackType.Image.offset = 0;

                                if(kfBlockTrackType.Cylinder < image.ImageInfo.cylinders)
                                {
                                    kfBlockTrackType.StartSector = currentSector;
                                    currentSector += image.ImageInfo.sectorsPerTrack;
                                    kfBlockTrackType.EndSector = currentSector - 1;
                                    kfBlockTrackType.Sectors = image.ImageInfo.sectorsPerTrack;
                                    kfBlockTrackType.BytesPerSector = (int)image.ImageInfo.sectorSize;
                                    kfBlockTrackType.Format = trkFormat;
                                }

                                Stream kfStream = kvp.Value.GetDataForkStream();
                                byte[] trackContents = new byte[kfStream.Length];
                                kfStream.Position = 0;
                                kfStream.Read(trackContents, 0, trackContents.Length);
                                kfBlockTrackType.Size = trackContents.Length;
                                kfBlockTrackType.Checksums = Checksum.GetChecksums(trackContents).ToArray();

                                kfBlockTrackTypes.Add(kfBlockTrackType);
                            }

                            sidecar.BlockMedia[0].Track =
                                kfBlockTrackTypes.OrderBy(t => t.Cylinder).ThenBy(t => t.Head).ToArray();
                        }
                        else
                            DicConsole.ErrorWriteLine(
                                "KryoFlux image do not contain same number of tracks ({0}) than disk image ({1}), ignoring...",
                                kfImage.ImageInfo.cylinders, image.ImageInfo.cylinders);
                    }
                    else
                        DicConsole.ErrorWriteLine(
                            "KryoFluximage do not contain same number of heads ({0}) than disk image ({1}), ignoring...",
                            kfImage.ImageInfo.heads, image.ImageInfo.heads);
                }
            }
            #endregion

            #region DiscFerret
            string dfiFilePath = Path.Combine(Path.GetDirectoryName(imagePath),
                Path.GetFileNameWithoutExtension(imagePath) + ".dfi");

            if(File.Exists(dfiFilePath))
            {
                ImagePlugins.DiscFerret dfiImage = new DiscFerret();
                Filters.ZZZNoFilter dfiFilter = new ZZZNoFilter();
                dfiFilter.Open(dfiFilePath);

                if(dfiImage.IdentifyImage(dfiFilter))
                {

                    try
                    {
                        dfiImage.OpenImage(dfiFilter);
                    }
                    catch(NotImplementedException)
                    {
                    }

                    if(image.ImageInfo.heads == dfiImage.ImageInfo.heads)
                    {
                        if(dfiImage.ImageInfo.cylinders >= image.ImageInfo.cylinders)
                        {
                            List<BlockTrackType> dfiBlockTrackTypes = new List<BlockTrackType>();
                            long currentSector = 0;
                            Stream dfiStream = dfiFilter.GetDataForkStream();

                            foreach(int t in dfiImage.trackOffsets.Keys)
                            {
                                BlockTrackType dfiBlockTrackType = new BlockTrackType();
                                dfiBlockTrackType.Cylinder = t / image.ImageInfo.heads;
                                dfiBlockTrackType.Head = t % image.ImageInfo.heads;
                                dfiBlockTrackType.Image = new ImageType();
                                dfiBlockTrackType.Image.format = dfiImage.GetImageFormat();
                                dfiBlockTrackType.Image.Value = Path.GetFileName(dfiFilePath);

                                if(dfiBlockTrackType.Cylinder < image.ImageInfo.cylinders)
                                {
                                    dfiBlockTrackType.StartSector = currentSector;
                                    currentSector += image.ImageInfo.sectorsPerTrack;
                                    dfiBlockTrackType.EndSector = currentSector - 1;
                                    dfiBlockTrackType.Sectors = image.ImageInfo.sectorsPerTrack;
                                    dfiBlockTrackType.BytesPerSector = (int)image.ImageInfo.sectorSize;
                                    dfiBlockTrackType.Format = trkFormat;
                                }

                                if(dfiImage.trackOffsets.TryGetValue(t, out long offset) && dfiImage.trackLengths.TryGetValue(t, out long length))
                                {
                                    dfiBlockTrackType.Image.offset = offset;
                                    byte[] trackContents = new byte[length];
                                    dfiStream.Position = offset;
                                    dfiStream.Read(trackContents, 0, trackContents.Length);
                                    dfiBlockTrackType.Size = trackContents.Length;
                                    dfiBlockTrackType.Checksums = Checksum.GetChecksums(trackContents).ToArray();
                                }

                                dfiBlockTrackTypes.Add(dfiBlockTrackType);
                            }

                            sidecar.BlockMedia[0].Track =
                                dfiBlockTrackTypes.OrderBy(t => t.Cylinder).ThenBy(t => t.Head).ToArray();
                        }
                        else
                            DicConsole.ErrorWriteLine(
                                "DiscFerret image do not contain same number of tracks ({0}) than disk image ({1}), ignoring...",
                                dfiImage.ImageInfo.cylinders, image.ImageInfo.cylinders);
                    }
                    else
                        DicConsole.ErrorWriteLine(
                            "DiscFerret image do not contain same number of heads ({0}) than disk image ({1}), ignoring...",
                            dfiImage.ImageInfo.heads, image.ImageInfo.heads);
                }
            }
            #endregion
            // TODO: Implement support for getting CHS from SCSI mode pages
        }
    }
}
