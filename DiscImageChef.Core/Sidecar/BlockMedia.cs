// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : BlockMedia.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$

using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Filesystems;
using DiscImageChef.ImagePlugins;
using Schemas;

namespace DiscImageChef.Core
{
    public static partial class Sidecar
    {
        static void BlockMedia(ImagePlugin image, System.Guid filterId, string imagePath, FileInfo fi, PluginBase plugins, List<ChecksumType> imgChecksums, ref CICMMetadataType sidecar)
        {
            sidecar.BlockMedia = new BlockMediaType[1];
            sidecar.BlockMedia[0] = new BlockMediaType();
            sidecar.BlockMedia[0].Checksums = imgChecksums.ToArray();
            sidecar.BlockMedia[0].Image = new ImageType();
            sidecar.BlockMedia[0].Image.format = image.GetImageFormat();
            sidecar.BlockMedia[0].Image.offset = 0;
            sidecar.BlockMedia[0].Image.offsetSpecified = true;
            sidecar.BlockMedia[0].Image.Value = Path.GetFileName(imagePath);
            sidecar.BlockMedia[0].Size = fi.Length;
            sidecar.BlockMedia[0].Sequence = new SequenceType();
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
            sidecar.BlockMedia[0].Sequence.MediaTitle = image.GetImageName();

            foreach(MediaTagType tagType in image.ImageInfo.readableMediaTags)
            {
                switch(tagType)
                {
                    case MediaTagType.ATAPI_IDENTIFY:
                        sidecar.BlockMedia[0].ATA = new ATAType();
                        sidecar.BlockMedia[0].ATA.Identify = new DumpType();
                        sidecar.BlockMedia[0].ATA.Identify.Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.ATAPI_IDENTIFY)).ToArray();
                        sidecar.BlockMedia[0].ATA.Identify.Size = image.ReadDiskTag(MediaTagType.ATAPI_IDENTIFY).Length;
                        break;
                    case MediaTagType.ATA_IDENTIFY:
                        sidecar.BlockMedia[0].ATA = new ATAType();
                        sidecar.BlockMedia[0].ATA.Identify = new DumpType();
                        sidecar.BlockMedia[0].ATA.Identify.Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.ATA_IDENTIFY)).ToArray();
                        sidecar.BlockMedia[0].ATA.Identify.Size = image.ReadDiskTag(MediaTagType.ATA_IDENTIFY).Length;
                        break;
                    case MediaTagType.PCMCIA_CIS:
                        byte[] cis = image.ReadDiskTag(MediaTagType.PCMCIA_CIS);
                        sidecar.BlockMedia[0].PCMCIA = new PCMCIAType();
                        sidecar.BlockMedia[0].PCMCIA.CIS = new DumpType();
                        sidecar.BlockMedia[0].PCMCIA.CIS.Checksums = Checksum.GetChecksums(cis).ToArray();
                        sidecar.BlockMedia[0].PCMCIA.CIS.Size = cis.Length;
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
                        sidecar.BlockMedia[0].SCSI = new SCSIType();
                        sidecar.BlockMedia[0].SCSI.Inquiry = new DumpType();
                        sidecar.BlockMedia[0].SCSI.Inquiry.Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.SCSI_INQUIRY)).ToArray();
                        sidecar.BlockMedia[0].SCSI.Inquiry.Size = image.ReadDiskTag(MediaTagType.SCSI_INQUIRY).Length;
                        break;
                    case MediaTagType.SD_CID:
                        if(sidecar.BlockMedia[0].SecureDigital == null)
                            sidecar.BlockMedia[0].SecureDigital = new SecureDigitalType();
                        sidecar.BlockMedia[0].SecureDigital.CID = new DumpType();
                        sidecar.BlockMedia[0].SecureDigital.CID.Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.SD_CID)).ToArray();
                        sidecar.BlockMedia[0].SecureDigital.CID.Size = image.ReadDiskTag(MediaTagType.SD_CID).Length;
                        break;
                    case MediaTagType.SD_CSD:
                        if(sidecar.BlockMedia[0].SecureDigital == null)
                            sidecar.BlockMedia[0].SecureDigital = new SecureDigitalType();
                        sidecar.BlockMedia[0].SecureDigital.CSD = new DumpType();
                        sidecar.BlockMedia[0].SecureDigital.CSD.Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.SD_CSD)).ToArray();
                        sidecar.BlockMedia[0].SecureDigital.CSD.Size = image.ReadDiskTag(MediaTagType.SD_CSD).Length;
                        break;
                    case MediaTagType.SD_ExtendedCSD:
                        if(sidecar.BlockMedia[0].SecureDigital == null)
                            sidecar.BlockMedia[0].SecureDigital = new SecureDigitalType();
                        sidecar.BlockMedia[0].SecureDigital.ExtendedCSD = new DumpType();
                        sidecar.BlockMedia[0].SecureDigital.ExtendedCSD.Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.SD_ExtendedCSD)).ToArray();
                        sidecar.BlockMedia[0].SecureDigital.ExtendedCSD.Size = image.ReadDiskTag(MediaTagType.SD_ExtendedCSD).Length;
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

                List<ChecksumType> cntChecksums = contentChkWorker.End();

                sidecar.BlockMedia[0].ContentChecksums = cntChecksums.ToArray();

                EndProgress2();
            }

            string dskType, dskSubType;
            Metadata.MediaType.MediaTypeToString(image.ImageInfo.mediaType, out dskType, out dskSubType);
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
                    sidecar.BlockMedia[0].FileSystemInformation[i] = new PartitionType();
                    sidecar.BlockMedia[0].FileSystemInformation[i].Description = partitions[i].Description;
                    sidecar.BlockMedia[0].FileSystemInformation[i].EndSector = (int)(partitions[i].End);
                    sidecar.BlockMedia[0].FileSystemInformation[i].Name = partitions[i].Name;
                    sidecar.BlockMedia[0].FileSystemInformation[i].Sequence = (int)partitions[i].Sequence;
                    sidecar.BlockMedia[0].FileSystemInformation[i].StartSector = (int)partitions[i].Start;
                    sidecar.BlockMedia[0].FileSystemInformation[i].Type = partitions[i].Type;

                    List<FileSystemType> lstFs = new List<FileSystemType>();

                    foreach(Filesystem _plugin in plugins.PluginsList.Values)
                    {
                        try
                        {
                            if(_plugin.Identify(image, partitions[i]))
                            {
                                string foo;
                                _plugin.GetInformation(image, partitions[i], out foo);
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
                sidecar.BlockMedia[0].FileSystemInformation[0] = new PartitionType();
                sidecar.BlockMedia[0].FileSystemInformation[0].StartSector = 0;
                sidecar.BlockMedia[0].FileSystemInformation[0].EndSector = (int)(image.GetSectors() - 1);

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


            // TODO: Implement support for getting CHS
        }
    }
}
