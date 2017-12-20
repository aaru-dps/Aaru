// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : OpticalDisc.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains logic to create sidecar from an optical media dump.
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

using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.Filesystems;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Core
{
    public static partial class Sidecar
    {
        static void OpticalDisc(ImagePlugin image, System.Guid filterId, string imagePath, FileInfo fi,
                                PluginBase plugins, List<ChecksumType> imgChecksums, ref CICMMetadataType sidecar)
        {
            sidecar.OpticalDisc = new[]
            {
                new OpticalDiscType
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
                    Sequence = new SequenceType {MediaTitle = image.GetImageName()}
                }
            };

            if(image.GetMediaSequence() != 0 && image.GetLastDiskSequence() != 0)
            {
                sidecar.OpticalDisc[0].Sequence.MediaSequence = image.GetMediaSequence();
                sidecar.OpticalDisc[0].Sequence.TotalMedia = image.GetMediaSequence();
            }
            else
            {
                sidecar.OpticalDisc[0].Sequence.MediaSequence = 1;
                sidecar.OpticalDisc[0].Sequence.TotalMedia = 1;
            }

            MediaType dskType = image.ImageInfo.MediaType;

            foreach(MediaTagType tagType in image.ImageInfo.ReadableMediaTags)
                switch(tagType)
                {
                    case MediaTagType.CD_ATIP:
                        sidecar.OpticalDisc[0].ATIP = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.CD_ATIP)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.CD_ATIP).Length
                        };
                        Decoders.CD.ATIP.CDATIP?
                            atip = Decoders.CD.ATIP.Decode(image.ReadDiskTag(MediaTagType.CD_ATIP));
                        if(atip.HasValue)
                            if(atip.Value.DDCD) dskType = atip.Value.DiscType ? MediaType.DDCDRW : MediaType.DDCDR;
                            else dskType = atip.Value.DiscType ? MediaType.CDRW : MediaType.CDR;
                        break;
                    case MediaTagType.DVD_BCA:
                        sidecar.OpticalDisc[0].BCA = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.DVD_BCA)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.DVD_BCA).Length
                        };
                        break;
                    case MediaTagType.BD_BCA:
                        sidecar.OpticalDisc[0].BCA = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.BD_BCA)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.BD_BCA).Length
                        };
                        break;
                    case MediaTagType.DVD_CMI:
                        sidecar.OpticalDisc[0].CMI = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.DVD_CMI)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.DVD_CMI).Length
                        };
                        Decoders.DVD.CSS_CPRM.LeadInCopyright? cmi =
                            Decoders.DVD.CSS_CPRM.DecodeLeadInCopyright(image.ReadDiskTag(MediaTagType.DVD_CMI));
                        if(cmi.HasValue)
                            switch(cmi.Value.CopyrightType)
                            {
                                case Decoders.DVD.CopyrightType.AACS:
                                    sidecar.OpticalDisc[0].CopyProtection = "AACS";
                                    break;
                                case Decoders.DVD.CopyrightType.CSS:
                                    sidecar.OpticalDisc[0].CopyProtection = "CSS";
                                    break;
                                case Decoders.DVD.CopyrightType.CPRM:
                                    sidecar.OpticalDisc[0].CopyProtection = "CPRM";
                                    break;
                            }

                        break;
                    case MediaTagType.DVD_DMI:
                        sidecar.OpticalDisc[0].DMI = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.DVD_DMI)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.DVD_DMI).Length
                        };
                        if(Decoders.Xbox.DMI.IsXbox(image.ReadDiskTag(MediaTagType.DVD_DMI)))
                        {
                            dskType = MediaType.XGD;
                            sidecar.OpticalDisc[0].Dimensions = new DimensionsType {Diameter = 120};
                        }
                        else if(Decoders.Xbox.DMI.IsXbox360(image.ReadDiskTag(MediaTagType.DVD_DMI)))
                        {
                            dskType = MediaType.XGD2;
                            sidecar.OpticalDisc[0].Dimensions = new DimensionsType {Diameter = 120};
                        }
                        break;
                    case MediaTagType.DVD_PFI:
                        sidecar.OpticalDisc[0].PFI = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.DVD_PFI)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.DVD_PFI).Length
                        };
                        Decoders.DVD.PFI.PhysicalFormatInformation? pfi =
                            Decoders.DVD.PFI.Decode(image.ReadDiskTag(MediaTagType.DVD_PFI));
                        if(pfi.HasValue)
                            if(dskType != MediaType.XGD && dskType != MediaType.XGD2 && dskType != MediaType.XGD3)
                            {
                                switch(pfi.Value.DiskCategory)
                                {
                                    case Decoders.DVD.DiskCategory.DVDPR:
                                        dskType = MediaType.DVDPR;
                                        break;
                                    case Decoders.DVD.DiskCategory.DVDPRDL:
                                        dskType = MediaType.DVDPRDL;
                                        break;
                                    case Decoders.DVD.DiskCategory.DVDPRW:
                                        dskType = MediaType.DVDPRW;
                                        break;
                                    case Decoders.DVD.DiskCategory.DVDPRWDL:
                                        dskType = MediaType.DVDPRWDL;
                                        break;
                                    case Decoders.DVD.DiskCategory.DVDR:
                                        dskType = MediaType.DVDR;
                                        break;
                                    case Decoders.DVD.DiskCategory.DVDRAM:
                                        dskType = MediaType.DVDRAM;
                                        break;
                                    case Decoders.DVD.DiskCategory.DVDROM:
                                        dskType = MediaType.DVDROM;
                                        break;
                                    case Decoders.DVD.DiskCategory.DVDRW:
                                        dskType = MediaType.DVDRW;
                                        break;
                                    case Decoders.DVD.DiskCategory.HDDVDR:
                                        dskType = MediaType.HDDVDR;
                                        break;
                                    case Decoders.DVD.DiskCategory.HDDVDRAM:
                                        dskType = MediaType.HDDVDRAM;
                                        break;
                                    case Decoders.DVD.DiskCategory.HDDVDROM:
                                        dskType = MediaType.HDDVDROM;
                                        break;
                                    case Decoders.DVD.DiskCategory.HDDVDRW:
                                        dskType = MediaType.HDDVDRW;
                                        break;
                                    case Decoders.DVD.DiskCategory.Nintendo:
                                        dskType = MediaType.GOD;
                                        break;
                                    case Decoders.DVD.DiskCategory.UMD:
                                        dskType = MediaType.UMD;
                                        break;
                                }

                                if(dskType == MediaType.DVDR && pfi.Value.PartVersion == 6) dskType = MediaType.DVDRDL;
                                if(dskType == MediaType.DVDRW && pfi.Value.PartVersion == 3)
                                    dskType = MediaType.DVDRWDL;
                                if(dskType == MediaType.GOD && pfi.Value.DiscSize == Decoders.DVD.DVDSize.OneTwenty)
                                    dskType = MediaType.WOD;

                                sidecar.OpticalDisc[0].Dimensions = new DimensionsType();
                                if(dskType == MediaType.UMD) sidecar.OpticalDisc[0].Dimensions.Diameter = 60;
                                else if(pfi.Value.DiscSize == Decoders.DVD.DVDSize.Eighty)
                                    sidecar.OpticalDisc[0].Dimensions.Diameter = 80;
                                else if(pfi.Value.DiscSize == Decoders.DVD.DVDSize.OneTwenty)
                                    sidecar.OpticalDisc[0].Dimensions.Diameter = 120;
                            }

                        break;
                    case MediaTagType.CD_PMA:
                        sidecar.OpticalDisc[0].PMA = new DumpType
                        {
                            Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.CD_PMA)).ToArray(),
                            Size = image.ReadDiskTag(MediaTagType.CD_PMA).Length
                        };
                        break;
                }

            try
            {
                List<Session> sessions = image.GetSessions();
                sidecar.OpticalDisc[0].Sessions = sessions != null ? sessions.Count : 1;
            }
            catch { sidecar.OpticalDisc[0].Sessions = 1; }

            List<Track> tracks = image.GetTracks();
            List<Schemas.TrackType> trksLst = null;
            if(tracks != null)
            {
                sidecar.OpticalDisc[0].Tracks = new int[1];
                sidecar.OpticalDisc[0].Tracks[0] = tracks.Count;
                trksLst = new List<Schemas.TrackType>();
            }

            InitProgress();
            foreach(Track trk in tracks)
            {
                Schemas.TrackType xmlTrk = new Schemas.TrackType();
                switch(trk.TrackType)
                {
                    case DiscImages.TrackType.Audio:
                        xmlTrk.TrackType1 = TrackTypeTrackType.audio;
                        break;
                    case DiscImages.TrackType.CdMode2Form2:
                        xmlTrk.TrackType1 = TrackTypeTrackType.m2f2;
                        break;
                    case DiscImages.TrackType.CdMode2Formless:
                        xmlTrk.TrackType1 = TrackTypeTrackType.mode2;
                        break;
                    case DiscImages.TrackType.CdMode2Form1:
                        xmlTrk.TrackType1 = TrackTypeTrackType.m2f1;
                        break;
                    case DiscImages.TrackType.CdMode1:
                        xmlTrk.TrackType1 = TrackTypeTrackType.mode1;
                        break;
                    case DiscImages.TrackType.Data:
                        switch(sidecar.OpticalDisc[0].DiscType)
                        {
                            case "BD":
                                xmlTrk.TrackType1 = TrackTypeTrackType.bluray;
                                break;
                            case "DDCD":
                                xmlTrk.TrackType1 = TrackTypeTrackType.ddcd;
                                break;
                            case "DVD":
                                xmlTrk.TrackType1 = TrackTypeTrackType.dvd;
                                break;
                            case "HD DVD":
                                xmlTrk.TrackType1 = TrackTypeTrackType.hddvd;
                                break;
                            default:
                                xmlTrk.TrackType1 = TrackTypeTrackType.mode1;
                                break;
                        }

                        break;
                }

                xmlTrk.Sequence =
                    new TrackSequenceType {Session = trk.TrackSession, TrackNumber = (int)trk.TrackSequence};
                xmlTrk.StartSector = (long)trk.TrackStartSector;
                xmlTrk.EndSector = (long)trk.TrackEndSector;

                if(trk.Indexes != null && trk.Indexes.ContainsKey(0)) if(trk.Indexes.TryGetValue(0, out ulong idx0)) xmlTrk.StartSector = (long)idx0;

                if(sidecar.OpticalDisc[0].DiscType == "CD" || sidecar.OpticalDisc[0].DiscType == "GD")
                {
                    xmlTrk.StartMSF = LbaToMsf(xmlTrk.StartSector);
                    xmlTrk.EndMSF = LbaToMsf(xmlTrk.EndSector);
                }
                else if(sidecar.OpticalDisc[0].DiscType == "DDCD")
                {
                    xmlTrk.StartMSF = DdcdLbaToMsf(xmlTrk.StartSector);
                    xmlTrk.EndMSF = DdcdLbaToMsf(xmlTrk.EndSector);
                }

                xmlTrk.Image = new ImageType {Value = Path.GetFileName(trk.TrackFile), format = trk.TrackFileType};

                if(trk.TrackFileOffset > 0)
                {
                    xmlTrk.Image.offset = (long)trk.TrackFileOffset;
                    xmlTrk.Image.offsetSpecified = true;
                }

                xmlTrk.Size = (xmlTrk.EndSector - xmlTrk.StartSector + 1) * trk.TrackRawBytesPerSector;
                xmlTrk.BytesPerSector = trk.TrackBytesPerSector;

                uint sectorsToRead = 512;
                ulong sectors = (ulong)(xmlTrk.EndSector - xmlTrk.StartSector + 1);
                ulong doneSectors = 0;

                // If there is only one track, and it's the same as the image file (e.g. ".iso" files), don't re-checksum.
                if(image.PluginUuid == new System.Guid("12345678-AAAA-BBBB-CCCC-123456789000") &&
                   // Only if filter is none...
                   (filterId == new System.Guid("12345678-AAAA-BBBB-CCCC-123456789000") ||
                    // ...or AppleDouble
                    filterId == new System.Guid("1b2165ee-c9df-4b21-bbbb-9e5892b2df4d"))) xmlTrk.Checksums = sidecar.OpticalDisc[0].Checksums;
                else
                {
                    UpdateProgress("Track {0} of {1}", trk.TrackSequence, tracks.Count);

                    // For fast debugging, skip checksum
                    //goto skipChecksum;

                    Checksum trkChkWorker = new Checksum();

                    InitProgress2();
                    while(doneSectors < sectors)
                    {
                        byte[] sector;

                        if(sectors - doneSectors >= sectorsToRead)
                        {
                            sector = image.ReadSectorsLong(doneSectors, sectorsToRead,
                                                           (uint)xmlTrk.Sequence.TrackNumber);
                            UpdateProgress2("Hashings sector {0} of {1}", (long)doneSectors,
                                            (long)(trk.TrackEndSector - trk.TrackStartSector + 1));
                            doneSectors += sectorsToRead;
                        }
                        else
                        {
                            sector = image.ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors),
                                                           (uint)xmlTrk.Sequence.TrackNumber);
                            UpdateProgress2("Hashings sector {0} of {1}", (long)doneSectors,
                                            (long)(trk.TrackEndSector - trk.TrackStartSector + 1));
                            doneSectors += sectors - doneSectors;
                        }

                        trkChkWorker.Update(sector);
                    }

                    List<ChecksumType> trkChecksums = trkChkWorker.End();

                    xmlTrk.Checksums = trkChecksums.ToArray();

                    EndProgress2();
                }

                if(trk.TrackSubchannelType != TrackSubchannelType.None)
                {
                    xmlTrk.SubChannel = new SubChannelType
                    {
                        Image = new ImageType {Value = trk.TrackSubchannelFile},
                        // TODO: Packed subchannel has different size?
                        Size = (xmlTrk.EndSector - xmlTrk.StartSector + 1) * 96
                    };

                    switch(trk.TrackSubchannelType)
                    {
                        case TrackSubchannelType.Packed:
                        case TrackSubchannelType.PackedInterleaved:
                            xmlTrk.SubChannel.Image.format = "rw";
                            break;
                        case TrackSubchannelType.Raw:
                        case TrackSubchannelType.RawInterleaved:
                            xmlTrk.SubChannel.Image.format = "rw_raw";
                            break;
                        case TrackSubchannelType.Q16:
                        case TrackSubchannelType.Q16Interleaved:
                            xmlTrk.SubChannel.Image.format = "q16";
                            break;
                    }

                    if(trk.TrackFileOffset > 0)
                    {
                        xmlTrk.SubChannel.Image.offset = (long)trk.TrackSubchannelOffset;
                        xmlTrk.SubChannel.Image.offsetSpecified = true;
                    }

                    Checksum subChkWorker = new Checksum();

                    sectors = (ulong)(xmlTrk.EndSector - xmlTrk.StartSector + 1);
                    doneSectors = 0;

                    InitProgress2();
                    while(doneSectors < sectors)
                    {
                        byte[] sector;

                        if(sectors - doneSectors >= sectorsToRead)
                        {
                            sector = image.ReadSectorsTag(doneSectors, sectorsToRead, (uint)xmlTrk.Sequence.TrackNumber,
                                                          SectorTagType.CdSectorSubchannel);
                            UpdateProgress2("Hashings subchannel sector {0} of {1}", (long)doneSectors,
                                            (long)(trk.TrackEndSector - trk.TrackStartSector + 1));
                            doneSectors += sectorsToRead;
                        }
                        else
                        {
                            sector = image.ReadSectorsTag(doneSectors, (uint)(sectors - doneSectors),
                                                          (uint)xmlTrk.Sequence.TrackNumber,
                                                          SectorTagType.CdSectorSubchannel);
                            UpdateProgress2("Hashings subchannel sector {0} of {1}", (long)doneSectors,
                                            (long)(trk.TrackEndSector - trk.TrackStartSector + 1));
                            doneSectors += sectors - doneSectors;
                        }

                        subChkWorker.Update(sector);
                    }

                    List<ChecksumType> subChecksums = subChkWorker.End();

                    xmlTrk.SubChannel.Checksums = subChecksums.ToArray();

                    EndProgress2();
                }

                // For fast debugging, skip checksum
                //skipChecksum:

                UpdateStatus("Checking filesystems on track {0} from sector {1} to {2}", xmlTrk.Sequence.TrackNumber,
                             xmlTrk.StartSector, xmlTrk.EndSector);

                List<Partition> partitions = Partitions.GetAll(image);
                Partitions.AddSchemesToStats(partitions);

                xmlTrk.FileSystemInformation = new PartitionType[1];
                if(partitions.Count > 0)
                {
                    xmlTrk.FileSystemInformation = new PartitionType[partitions.Count];
                    for(int i = 0; i < partitions.Count; i++)
                    {
                        xmlTrk.FileSystemInformation[i] = new PartitionType
                        {
                            Description = partitions[i].Description,
                            EndSector = (int)partitions[i].End,
                            Name = partitions[i].Name,
                            Sequence = (int)partitions[i].Sequence,
                            StartSector = (int)partitions[i].Start,
                            Type = partitions[i].Type
                        };
                        List<FileSystemType> lstFs = new List<FileSystemType>();

                        foreach(Filesystem plugin in plugins.PluginsList.Values)
                            try
                            {
                                if(plugin.Identify(image, partitions[i]))
                                {
                                    plugin.GetInformation(image, partitions[i], out string foo);
                                    lstFs.Add(plugin.XmlFSType);
                                    Statistics.AddFilesystem(plugin.XmlFSType.Type);

                                    if(plugin.XmlFSType.Type == "Opera") dskType = MediaType.ThreeDO;
                                    if(plugin.XmlFSType.Type == "PC Engine filesystem")
                                        dskType = MediaType.SuperCDROM2;
                                    if(plugin.XmlFSType.Type == "Nintendo Wii filesystem") dskType = MediaType.WOD;
                                    if(plugin.XmlFSType.Type == "Nintendo Gamecube filesystem")
                                        dskType = MediaType.GOD;
                                }
                            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                            catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                            {
                                //DicConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                            }

                        if(lstFs.Count > 0) xmlTrk.FileSystemInformation[i].FileSystems = lstFs.ToArray();
                    }
                }
                else
                {
                    xmlTrk.FileSystemInformation[0] = new PartitionType
                    {
                        EndSector = (int)xmlTrk.EndSector,
                        StartSector = (int)xmlTrk.StartSector
                    };
                    List<FileSystemType> lstFs = new List<FileSystemType>();

                    Partition xmlPart = new Partition
                    {
                        Start = (ulong)xmlTrk.StartSector,
                        Length = (ulong)(xmlTrk.EndSector - xmlTrk.StartSector + 1),
                        Type = xmlTrk.TrackType1.ToString(),
                        Size = (ulong)xmlTrk.Size,
                        Sequence = (ulong)xmlTrk.Sequence.TrackNumber
                    };
                    foreach(Filesystem plugin in plugins.PluginsList.Values)
                        try
                        {
                            if(plugin.Identify(image, xmlPart))
                            {
                                plugin.GetInformation(image, xmlPart, out string foo);
                                lstFs.Add(plugin.XmlFSType);
                                Statistics.AddFilesystem(plugin.XmlFSType.Type);

                                if(plugin.XmlFSType.Type == "Opera") dskType = MediaType.ThreeDO;
                                if(plugin.XmlFSType.Type == "PC Engine filesystem") dskType = MediaType.SuperCDROM2;
                                if(plugin.XmlFSType.Type == "Nintendo Wii filesystem") dskType = MediaType.WOD;
                                if(plugin.XmlFSType.Type == "Nintendo Gamecube filesystem") dskType = MediaType.GOD;
                            }
                        }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                        {
                            //DicConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                        }

                    if(lstFs.Count > 0) xmlTrk.FileSystemInformation[0].FileSystems = lstFs.ToArray();
                }

                trksLst.Add(xmlTrk);
            }

            EndProgress();

            if(trksLst != null) sidecar.OpticalDisc[0].Track = trksLst.ToArray();

            // All XGD3 all have the same number of blocks
            if(dskType == MediaType.XGD2 && sidecar.OpticalDisc[0].Track.Length == 1)
            {
                ulong blocks = (ulong)(sidecar.OpticalDisc[0].Track[0].EndSector -
                                       sidecar.OpticalDisc[0].Track[0].StartSector + 1);
                if(blocks == 25063 || // Locked (or non compatible drive)
                   blocks == 4229664 || // Xtreme unlock
                   blocks == 4246304) // Wxripper unlock
                    dskType = MediaType.XGD3;
            }

            Metadata.MediaType.MediaTypeToString(dskType, out string dscType, out string dscSubType);
            sidecar.OpticalDisc[0].DiscType = dscType;
            sidecar.OpticalDisc[0].DiscSubType = dscSubType;
            Statistics.AddMedia(dskType, false);

            if(!string.IsNullOrEmpty(image.ImageInfo.DriveManufacturer) ||
               !string.IsNullOrEmpty(image.ImageInfo.DriveModel) ||
               !string.IsNullOrEmpty(image.ImageInfo.DriveFirmwareRevision) ||
               !string.IsNullOrEmpty(image.ImageInfo.DriveSerialNumber))
                sidecar.OpticalDisc[0].DumpHardwareArray = new[]
                {
                    new DumpHardwareType
                    {
                        Extents = new[] {new ExtentType {Start = 0, End = image.ImageInfo.Sectors}},
                        Manufacturer = image.ImageInfo.DriveManufacturer,
                        Model = image.ImageInfo.DriveModel,
                        Firmware = image.ImageInfo.DriveFirmwareRevision,
                        Serial = image.ImageInfo.DriveSerialNumber,
                        Software = new SoftwareType
                        {
                            Name = image.GetImageApplication(),
                            Version = image.GetImageApplicationVersion()
                        }
                    }
                };
        }
    }
}