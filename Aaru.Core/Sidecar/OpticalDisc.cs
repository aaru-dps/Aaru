﻿// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Devices.Dumping;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Schemas;
using DMI = Aaru.Decoders.Xbox.DMI;
using MediaType = Aaru.CommonTypes.MediaType;
using Session = Aaru.CommonTypes.Structs.Session;
using TrackType = Schemas.TrackType;

namespace Aaru.Core
{
    public sealed partial class Sidecar
    {
        /// <summary>Creates a metadata sidecar for an optical disc (e.g. CD, DVD, GD, BD, XGD, GOD)</summary>
        /// <param name="image">Image</param>
        /// <param name="filterId">Filter uuid</param>
        /// <param name="imagePath">Image path</param>
        /// <param name="fi">Image file information</param>
        /// <param name="plugins">Image plugins</param>
        /// <param name="imgChecksums">List of image checksums</param>
        /// <param name="sidecar">Metadata sidecar</param>
        /// <param name="encoding">Encoding to be used for filesystem plugins</param>
        void OpticalDisc(IOpticalMediaImage image, Guid filterId, string imagePath, FileInfo fi, PluginBase plugins,
                         List<ChecksumType> imgChecksums, ref CICMMetadataType sidecar, Encoding encoding)
        {
            if(_aborted)
                return;

            sidecar.OpticalDisc = new[]
            {
                new OpticalDiscType
                {
                    Checksums = imgChecksums.ToArray(),
                    Image = new ImageType
                    {
                        format          = image.Format,
                        offset          = 0,
                        offsetSpecified = true,
                        Value           = Path.GetFileName(imagePath)
                    },
                    Size = (ulong)fi.Length,
                    Sequence = new SequenceType
                    {
                        MediaTitle = image.Info.MediaTitle
                    }
                }
            };

            if(image.Info.MediaSequence     != 0 &&
               image.Info.LastMediaSequence != 0)
            {
                sidecar.OpticalDisc[0].Sequence.MediaSequence = (uint)image.Info.MediaSequence;
                sidecar.OpticalDisc[0].Sequence.TotalMedia    = (uint)image.Info.LastMediaSequence;
            }
            else
            {
                sidecar.OpticalDisc[0].Sequence.MediaSequence = 1;
                sidecar.OpticalDisc[0].Sequence.TotalMedia    = 1;
            }

            MediaType dskType = image.Info.MediaType;

            UpdateStatus("Hashing media tags...");

            foreach(MediaTagType tagType in image.Info.ReadableMediaTags)
            {
                if(_aborted)
                    return;

                byte[] tag = image.ReadDiskTag(tagType);

                Dump.AddMediaTagToSidecar(imagePath, tagType, tag, ref sidecar);

                switch(tagType)
                {
                    case MediaTagType.CD_ATIP:
                        ATIP.CDATIP atip = ATIP.Decode(tag);

                        if(atip != null)
                            if(atip.DDCD)
                                dskType = atip.DiscType ? MediaType.DDCDRW : MediaType.DDCDR;
                            else
                                dskType = atip.DiscType ? MediaType.CDRW : MediaType.CDR;

                        break;
                    case MediaTagType.DVD_DMI:
                        if(DMI.IsXbox(tag))
                        {
                            dskType = MediaType.XGD;

                            sidecar.OpticalDisc[0].Dimensions = new DimensionsType
                            {
                                Diameter  = 120,
                                Thickness = 1.2
                            };
                        }
                        else if(DMI.IsXbox360(tag))
                        {
                            dskType = MediaType.XGD2;

                            sidecar.OpticalDisc[0].Dimensions = new DimensionsType
                            {
                                Diameter  = 120,
                                Thickness = 1.2
                            };
                        }

                        break;
                    case MediaTagType.DVD_PFI:
                        PFI.PhysicalFormatInformation? pfi = PFI.Decode(tag, dskType);

                        if(pfi.HasValue)
                            if(dskType != MediaType.XGD    &&
                               dskType != MediaType.XGD2   &&
                               dskType != MediaType.XGD3   &&
                               dskType != MediaType.PS2DVD &&
                               dskType != MediaType.PS3DVD &&
                               dskType != MediaType.Nuon)
                            {
                                switch(pfi.Value.DiskCategory)
                                {
                                    case DiskCategory.DVDPR:
                                        dskType = MediaType.DVDPR;

                                        break;
                                    case DiskCategory.DVDPRDL:
                                        dskType = MediaType.DVDPRDL;

                                        break;
                                    case DiskCategory.DVDPRW:
                                        dskType = MediaType.DVDPRW;

                                        break;
                                    case DiskCategory.DVDPRWDL:
                                        dskType = MediaType.DVDPRWDL;

                                        break;
                                    case DiskCategory.DVDR:
                                        dskType = MediaType.DVDR;

                                        break;
                                    case DiskCategory.DVDRAM:
                                        dskType = MediaType.DVDRAM;

                                        break;
                                    case DiskCategory.DVDROM:
                                        dskType = MediaType.DVDROM;

                                        break;
                                    case DiskCategory.DVDRW:
                                        dskType = MediaType.DVDRW;

                                        break;
                                    case DiskCategory.HDDVDR:
                                        dskType = MediaType.HDDVDR;

                                        break;
                                    case DiskCategory.HDDVDRAM:
                                        dskType = MediaType.HDDVDRAM;

                                        break;
                                    case DiskCategory.HDDVDROM:
                                        dskType = MediaType.HDDVDROM;

                                        break;
                                    case DiskCategory.HDDVDRW:
                                        dskType = MediaType.HDDVDRW;

                                        break;
                                    case DiskCategory.Nintendo:
                                        dskType = MediaType.GOD;

                                        break;
                                    case DiskCategory.UMD:
                                        dskType = MediaType.UMD;

                                        break;
                                }

                                if(dskType               == MediaType.DVDR &&
                                   pfi.Value.PartVersion >= 6)
                                    dskType = MediaType.DVDRDL;

                                if(dskType               == MediaType.DVDRW &&
                                   pfi.Value.PartVersion >= 15)
                                    dskType = MediaType.DVDRWDL;

                                if(dskType            == MediaType.GOD &&
                                   pfi.Value.DiscSize == DVDSize.OneTwenty)
                                    dskType = MediaType.WOD;

                                sidecar.OpticalDisc[0].Dimensions = new DimensionsType();

                                if(dskType == MediaType.UMD)
                                {
                                    sidecar.OpticalDisc[0].Dimensions.Height          = 64;
                                    sidecar.OpticalDisc[0].Dimensions.HeightSpecified = true;
                                    sidecar.OpticalDisc[0].Dimensions.Width           = 63;
                                    sidecar.OpticalDisc[0].Dimensions.WidthSpecified  = true;
                                    sidecar.OpticalDisc[0].Dimensions.Thickness       = 4;
                                }
                                else
                                    switch(pfi.Value.DiscSize)
                                    {
                                        case DVDSize.Eighty:
                                            sidecar.OpticalDisc[0].Dimensions.Diameter          = 80;
                                            sidecar.OpticalDisc[0].Dimensions.DiameterSpecified = true;
                                            sidecar.OpticalDisc[0].Dimensions.Thickness         = 1.2;

                                            break;
                                        case DVDSize.OneTwenty:
                                            sidecar.OpticalDisc[0].Dimensions.Diameter          = 120;
                                            sidecar.OpticalDisc[0].Dimensions.DiameterSpecified = true;
                                            sidecar.OpticalDisc[0].Dimensions.Thickness         = 1.2;

                                            break;
                                    }
                            }

                        break;
                }
            }

            try
            {
                List<Session> sessions = image.Sessions;
                sidecar.OpticalDisc[0].Sessions = (uint)(sessions?.Count ?? 1);
            }
            catch
            {
                sidecar.OpticalDisc[0].Sessions = 1;
            }

            List<Track>     tracks  = image.Tracks;
            List<TrackType> trksLst = null;

            if(tracks != null)
            {
                sidecar.OpticalDisc[0].Tracks    = new uint[1];
                sidecar.OpticalDisc[0].Tracks[0] = (uint)tracks.Count;
                trksLst                          = new List<TrackType>();
            }

            if(sidecar.OpticalDisc[0].Dimensions == null &&
               image.Info.MediaType              != MediaType.Unknown)
                sidecar.OpticalDisc[0].Dimensions = Dimensions.DimensionsFromMediaType(image.Info.MediaType);

            if(_aborted)
                return;

            InitProgress();

            UpdateStatus("Checking filesystems");
            List<Partition> partitions = Partitions.GetAll(image);
            Partitions.AddSchemesToStats(partitions);

            UpdateStatus("Hashing tracks...");

            foreach(Track trk in tracks)
            {
                if(_aborted)
                {
                    EndProgress();

                    return;
                }

                var xmlTrk = new TrackType();

                switch(trk.TrackType)
                {
                    case CommonTypes.Enums.TrackType.Audio:
                        xmlTrk.TrackType1 = TrackTypeTrackType.audio;

                        break;
                    case CommonTypes.Enums.TrackType.CdMode2Form2:
                        xmlTrk.TrackType1 = TrackTypeTrackType.m2f2;

                        break;
                    case CommonTypes.Enums.TrackType.CdMode2Formless:
                        xmlTrk.TrackType1 = TrackTypeTrackType.mode2;

                        break;
                    case CommonTypes.Enums.TrackType.CdMode2Form1:
                        xmlTrk.TrackType1 = TrackTypeTrackType.m2f1;

                        break;
                    case CommonTypes.Enums.TrackType.CdMode1:
                        xmlTrk.TrackType1 = TrackTypeTrackType.mode1;

                        break;
                    case CommonTypes.Enums.TrackType.Data:
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

                xmlTrk.Sequence = new TrackSequenceType
                {
                    Session     = trk.TrackSession,
                    TrackNumber = trk.TrackSequence
                };

                xmlTrk.StartSector = trk.TrackStartSector;
                xmlTrk.EndSector   = trk.TrackEndSector;

                int idx0 = -1;

                if(trk.Indexes?.TryGetValue(0, out idx0) == true &&
                   idx0                                  >= 0)
                    xmlTrk.StartSector = (ulong)idx0;

                switch(sidecar.OpticalDisc[0].DiscType)
                {
                    case "CD":
                    case "GD":
                        xmlTrk.StartMSF = LbaToMsf((long)xmlTrk.StartSector);
                        xmlTrk.EndMSF   = LbaToMsf((long)xmlTrk.EndSector);

                        break;
                    case "DDCD":
                        xmlTrk.StartMSF = DdcdLbaToMsf((long)xmlTrk.StartSector);
                        xmlTrk.EndMSF   = DdcdLbaToMsf((long)xmlTrk.EndSector);

                        break;
                }

                xmlTrk.Image = new ImageType
                {
                    Value  = Path.GetFileName(trk.TrackFile),
                    format = trk.TrackFileType
                };

                if(trk.TrackFileOffset > 0)
                {
                    xmlTrk.Image.offset          = trk.TrackFileOffset;
                    xmlTrk.Image.offsetSpecified = true;
                }

                xmlTrk.Size = (xmlTrk.EndSector - xmlTrk.StartSector + 1) * (ulong)trk.TrackRawBytesPerSector;

                xmlTrk.BytesPerSector = (uint)trk.TrackBytesPerSector;

                uint  sectorsToRead = 512;
                ulong sectors       = xmlTrk.EndSector - xmlTrk.StartSector + 1;
                ulong doneSectors   = 0;

                // If there is only one track, and it's the same as the image file (e.g. ".iso" files), don't re-checksum.
                if(image.Id == new Guid("12345678-AAAA-BBBB-CCCC-123456789000") &&

                   // Only if filter is none...
                   (filterId == new Guid("12345678-AAAA-BBBB-CCCC-123456789000") ||

                    // ...or AppleDouble
                    filterId == new Guid("1b2165ee-c9df-4b21-bbbb-9e5892b2df4d")))
                    xmlTrk.Checksums = sidecar.OpticalDisc[0].Checksums;
                else
                {
                    UpdateProgress("Track {0} of {1}", trk.TrackSequence, tracks.Count);

                    // For fast debugging, skip checksum
                    //goto skipChecksum;

                    var trkChkWorker = new Checksum();

                    InitProgress2();

                    while(doneSectors < sectors)
                    {
                        if(_aborted)
                        {
                            EndProgress();
                            EndProgress2();

                            return;
                        }

                        byte[] sector;

                        if(sectors - doneSectors >= sectorsToRead)
                        {
                            sector = image.ReadSectorsLong(doneSectors, sectorsToRead, xmlTrk.Sequence.TrackNumber);

                            UpdateProgress2("Hashing sector {0} of {1}", (long)doneSectors,
                                            (long)(trk.TrackEndSector - trk.TrackStartSector + 1));

                            doneSectors += sectorsToRead;
                        }
                        else
                        {
                            sector = image.ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors),
                                                           xmlTrk.Sequence.TrackNumber);

                            UpdateProgress2("Hashing sector {0} of {1}", (long)doneSectors,
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
                        Image = new ImageType
                        {
                            Value = trk.TrackSubchannelFile
                        },

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
                        xmlTrk.SubChannel.Image.offset          = trk.TrackSubchannelOffset;
                        xmlTrk.SubChannel.Image.offsetSpecified = true;
                    }

                    var subChkWorker = new Checksum();

                    sectors     = xmlTrk.EndSector - xmlTrk.StartSector + 1;
                    doneSectors = 0;

                    InitProgress2();

                    while(doneSectors < sectors)
                    {
                        if(_aborted)
                        {
                            EndProgress();
                            EndProgress2();

                            return;
                        }

                        byte[] sector;

                        if(sectors - doneSectors >= sectorsToRead)
                        {
                            sector = image.ReadSectorsTag(doneSectors, sectorsToRead, xmlTrk.Sequence.TrackNumber,
                                                          SectorTagType.CdSectorSubchannel);

                            UpdateProgress2("Hashing subchannel sector {0} of {1}", (long)doneSectors,
                                            (long)(trk.TrackEndSector - trk.TrackStartSector + 1));

                            doneSectors += sectorsToRead;
                        }
                        else
                        {
                            sector = image.ReadSectorsTag(doneSectors, (uint)(sectors - doneSectors),
                                                          xmlTrk.Sequence.TrackNumber,
                                                          SectorTagType.CdSectorSubchannel);

                            UpdateProgress2("Hashing subchannel sector {0} of {1}", (long)doneSectors,
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

                List<Partition> trkPartitions = partitions.
                                                Where(p => p.Start >= trk.TrackStartSector &&
                                                           p.End   <= trk.TrackEndSector).ToList();

                xmlTrk.FileSystemInformation = new PartitionType[1];

                if(trkPartitions.Count > 0)
                {
                    xmlTrk.FileSystemInformation = new PartitionType[trkPartitions.Count];

                    for(int i = 0; i < trkPartitions.Count; i++)
                    {
                        xmlTrk.FileSystemInformation[i] = new PartitionType
                        {
                            Description = trkPartitions[i].Description,
                            EndSector   = trkPartitions[i].End,
                            Name        = trkPartitions[i].Name,
                            Sequence    = (uint)trkPartitions[i].Sequence,
                            StartSector = trkPartitions[i].Start,
                            Type        = trkPartitions[i].Type
                        };

                        List<FileSystemType> lstFs = new List<FileSystemType>();

                        foreach(IFilesystem plugin in plugins.PluginsList.Values)
                            try
                            {
                                if(_aborted)
                                {
                                    EndProgress();

                                    return;
                                }

                                if(!plugin.Identify(image, trkPartitions[i]))
                                    continue;

                                plugin.GetInformation(image, trkPartitions[i], out _, encoding);
                                lstFs.Add(plugin.XmlFsType);
                                Statistics.AddFilesystem(plugin.XmlFsType.Type);

                                switch(plugin.XmlFsType.Type)
                                {
                                    case "Opera":
                                        dskType = MediaType.ThreeDO;

                                        break;
                                    case "PC Engine filesystem":
                                        dskType = MediaType.SuperCDROM2;

                                        break;
                                    case "Nintendo Wii filesystem":
                                        dskType = MediaType.WOD;

                                        break;
                                    case "Nintendo Gamecube filesystem":
                                        dskType = MediaType.GOD;

                                        break;
                                }
                            }
                            #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                            catch
                                #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                            {
                                //AaruConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                            }

                        if(lstFs.Count > 0)
                            xmlTrk.FileSystemInformation[i].FileSystems = lstFs.ToArray();
                    }
                }
                else
                {
                    xmlTrk.FileSystemInformation[0] = new PartitionType
                    {
                        EndSector   = xmlTrk.EndSector,
                        StartSector = xmlTrk.StartSector
                    };

                    List<FileSystemType> lstFs = new List<FileSystemType>();

                    var xmlPart = new Partition
                    {
                        Start    = xmlTrk.StartSector,
                        Length   = xmlTrk.EndSector - xmlTrk.StartSector + 1,
                        Type     = xmlTrk.TrackType1.ToString(),
                        Size     = xmlTrk.Size,
                        Sequence = xmlTrk.Sequence.TrackNumber
                    };

                    foreach(IFilesystem plugin in plugins.PluginsList.Values)
                        try
                        {
                            if(_aborted)
                            {
                                EndProgress();

                                return;
                            }

                            if(!plugin.Identify(image, xmlPart))
                                continue;

                            plugin.GetInformation(image, xmlPart, out _, encoding);
                            lstFs.Add(plugin.XmlFsType);
                            Statistics.AddFilesystem(plugin.XmlFsType.Type);

                            switch(plugin.XmlFsType.Type)
                            {
                                case "Opera":
                                    dskType = MediaType.ThreeDO;

                                    break;
                                case "PC Engine filesystem":
                                    dskType = MediaType.SuperCDROM2;

                                    break;
                                case "Nintendo Wii filesystem":
                                    dskType = MediaType.WOD;

                                    break;
                                case "Nintendo Gamecube filesystem":
                                    dskType = MediaType.GOD;

                                    break;
                            }
                        }
                        #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        catch
                            #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                        {
                            //AaruConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                        }

                    if(lstFs.Count > 0)
                        xmlTrk.FileSystemInformation[0].FileSystems = lstFs.ToArray();
                }

                try
                {
                    byte[] isrcData = image.ReadSectorTag(trk.TrackSequence, SectorTagType.CdTrackIsrc);

                    if(isrcData?.Length > 0)
                        xmlTrk.ISRC = Encoding.UTF8.GetString(isrcData);
                }
                catch(Exception)
                {
                    // Ignored
                }

                try
                {
                    byte[] flagsData = image.ReadSectorTag(trk.TrackSequence, SectorTagType.CdTrackFlags);

                    if(flagsData?.Length > 0)
                    {
                        var trackFlags = (CdFlags)flagsData[0];

                        xmlTrk.Flags = new TrackFlagsType
                        {
                            PreEmphasis   = trackFlags.HasFlag(CdFlags.PreEmphasis),
                            CopyPermitted = trackFlags.HasFlag(CdFlags.CopyPermitted),
                            Data          = trackFlags.HasFlag(CdFlags.DataTrack),
                            Quadraphonic  = trackFlags.HasFlag(CdFlags.FourChannel)
                        };
                    }
                }
                catch(Exception)
                {
                    // Ignored
                }

                if(trk.Indexes?.Count > 0)
                {
                    xmlTrk.Indexes = trk.Indexes?.OrderBy(i => i.Key).Select(i => new TrackIndexType
                    {
                        index = i.Key,
                        Value = i.Value
                    }).ToArray();
                }

                trksLst.Add(xmlTrk);
            }

            EndProgress();

            if(trksLst != null)
                sidecar.OpticalDisc[0].Track = trksLst.ToArray();

            // All XGD3 all have the same number of blocks
            if(dskType                             == MediaType.XGD2 &&
               sidecar.OpticalDisc[0].Track.Length == 1)
            {
                ulong blocks = sidecar.OpticalDisc[0].Track[0].EndSector - sidecar.OpticalDisc[0].Track[0].StartSector +
                               1;

                if(blocks == 25063   || // Locked (or non compatible drive)
                   blocks == 4229664 || // Xtreme unlock
                   blocks == 4246304)   // Wxripper unlock
                    dskType = MediaType.XGD3;
            }

            (string type, string subType) discType = CommonTypes.Metadata.MediaType.MediaTypeToString(dskType);
            sidecar.OpticalDisc[0].DiscType    = discType.type;
            sidecar.OpticalDisc[0].DiscSubType = discType.subType;
            Statistics.AddMedia(dskType, false);

            if(image.DumpHardware != null)
                sidecar.OpticalDisc[0].DumpHardwareArray = image.DumpHardware.ToArray();
            else if(!string.IsNullOrEmpty(image.Info.DriveManufacturer)     ||
                    !string.IsNullOrEmpty(image.Info.DriveModel)            ||
                    !string.IsNullOrEmpty(image.Info.DriveFirmwareRevision) ||
                    !string.IsNullOrEmpty(image.Info.DriveSerialNumber))
                sidecar.OpticalDisc[0].DumpHardwareArray = new[]
                {
                    new DumpHardwareType
                    {
                        Extents = new[]
                        {
                            new ExtentType
                            {
                                Start = 0,
                                End   = image.Info.Sectors
                            }
                        },
                        Manufacturer = image.Info.DriveManufacturer,
                        Model        = image.Info.DriveModel,
                        Firmware     = image.Info.DriveFirmwareRevision,
                        Serial       = image.Info.DriveSerialNumber,
                        Software = new SoftwareType
                        {
                            Name    = image.Info.Application,
                            Version = image.Info.ApplicationVersion
                        }
                    }
                };
        }
    }
}