// /***************************************************************************
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
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using DMI = Aaru.Decoders.Xbox.DMI;
using Dump = Aaru.Core.Devices.Dumping.Dump;
using Partition = Aaru.CommonTypes.Partition;
using Session = Aaru.CommonTypes.Structs.Session;
using Track = Aaru.CommonTypes.Structs.Track;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.Core;

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
                     List<CommonTypes.AaruMetadata.Checksum> imgChecksums, ref Metadata sidecar, Encoding encoding)
    {
        if(_aborted)
            return;

        sidecar.OpticalDiscs = new List<OpticalDisc>
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
            sidecar.OpticalDiscs[0].Sequence.MediaSequence = (uint)image.Info.MediaSequence;
            sidecar.OpticalDiscs[0].Sequence.TotalMedia    = (uint)image.Info.LastMediaSequence;
        }
        else
        {
            sidecar.OpticalDiscs[0].Sequence.MediaSequence = 1;
            sidecar.OpticalDiscs[0].Sequence.TotalMedia    = 1;
        }

        MediaType   dskType = image.Info.MediaType;
        ErrorNumber errno;

        UpdateStatus(Localization.Core.Hashing_media_tags);

        foreach(MediaTagType tagType in image.Info.ReadableMediaTags)
        {
            if(_aborted)
                return;

            errno = image.ReadMediaTag(tagType, out byte[] tag);

            if(errno != ErrorNumber.NoError)
                continue;

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

                        sidecar.OpticalDiscs[0].Dimensions = new Dimensions
                        {
                            Diameter  = 120,
                            Thickness = 1.2
                        };
                    }
                    else if(DMI.IsXbox360(tag))
                    {
                        dskType = MediaType.XGD2;

                        sidecar.OpticalDiscs[0].Dimensions = new Dimensions
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
                            dskType = pfi.Value.DiskCategory switch
                            {
                                DiskCategory.DVDPR    => MediaType.DVDPR,
                                DiskCategory.DVDPRDL  => MediaType.DVDPRDL,
                                DiskCategory.DVDPRW   => MediaType.DVDPRW,
                                DiskCategory.DVDPRWDL => MediaType.DVDPRWDL,
                                DiskCategory.DVDR     => MediaType.DVDR,
                                DiskCategory.DVDRAM   => MediaType.DVDRAM,
                                DiskCategory.DVDROM   => MediaType.DVDROM,
                                DiskCategory.DVDRW    => MediaType.DVDRW,
                                DiskCategory.HDDVDR   => MediaType.HDDVDR,
                                DiskCategory.HDDVDRAM => MediaType.HDDVDRAM,
                                DiskCategory.HDDVDROM => MediaType.HDDVDROM,
                                DiskCategory.HDDVDRW  => MediaType.HDDVDRW,
                                DiskCategory.Nintendo => MediaType.GOD,
                                DiskCategory.UMD      => MediaType.UMD,
                                _                     => dskType
                            };

                            if(dskType               == MediaType.DVDR &&
                               pfi.Value.PartVersion >= 6)
                                dskType = MediaType.DVDRDL;

                            if(dskType               == MediaType.DVDRW &&
                               pfi.Value.PartVersion >= 15)
                                dskType = MediaType.DVDRWDL;

                            if(dskType            == MediaType.GOD &&
                               pfi.Value.DiscSize == DVDSize.OneTwenty)
                                dskType = MediaType.WOD;

                            sidecar.OpticalDiscs[0].Dimensions = new Dimensions();

                            if(dskType == MediaType.UMD)
                            {
                                sidecar.OpticalDiscs[0].Dimensions.Height    = 64;
                                sidecar.OpticalDiscs[0].Dimensions.Width     = 63;
                                sidecar.OpticalDiscs[0].Dimensions.Thickness = 4;
                            }
                            else
                                switch(pfi.Value.DiscSize)
                                {
                                    case DVDSize.Eighty:
                                        sidecar.OpticalDiscs[0].Dimensions.Diameter  = 80;
                                        sidecar.OpticalDiscs[0].Dimensions.Thickness = 1.2;

                                        break;
                                    case DVDSize.OneTwenty:
                                        sidecar.OpticalDiscs[0].Dimensions.Diameter  = 120;
                                        sidecar.OpticalDiscs[0].Dimensions.Thickness = 1.2;

                                        break;
                                }
                        }

                    break;
            }
        }

        try
        {
            List<Session> sessions = image.Sessions;
            sidecar.OpticalDiscs[0].Sessions = (uint)(sessions?.Count ?? 1);
        }
        catch
        {
            sidecar.OpticalDiscs[0].Sessions = 1;
        }

        List<Track>                          tracks  = image.Tracks;
        List<CommonTypes.AaruMetadata.Track> trksLst = null;

        if(tracks != null)
        {
            sidecar.OpticalDiscs[0].Tracks    = new uint[1];
            sidecar.OpticalDiscs[0].Tracks[0] = (uint)tracks.Count;
            trksLst                           = new List<CommonTypes.AaruMetadata.Track>();
        }

        if(sidecar.OpticalDiscs[0].Dimensions == null &&
           image.Info.MediaType               != MediaType.Unknown)
            sidecar.OpticalDiscs[0].Dimensions = Dimensions.FromMediaType(image.Info.MediaType);

        if(_aborted)
            return;

        InitProgress();

        UpdateStatus(Localization.Core.Checking_filesystems);
        List<Partition> partitions = Partitions.GetAll(image);
        Partitions.AddSchemesToStats(partitions);

        UpdateStatus(Localization.Core.Hashing_tracks);

        foreach(Track trk in tracks)
        {
            if(_aborted)
            {
                EndProgress();

                return;
            }

            var xmlTrk = new CommonTypes.AaruMetadata.Track();

            xmlTrk.Type = trk.Type switch
            {
                TrackType.Audio           => CommonTypes.AaruMetadata.TrackType.Audio,
                TrackType.CdMode2Form2    => CommonTypes.AaruMetadata.TrackType.Mode2Form2,
                TrackType.CdMode2Formless => CommonTypes.AaruMetadata.TrackType.Mode2,
                TrackType.CdMode2Form1    => CommonTypes.AaruMetadata.TrackType.Mode2Form1,
                TrackType.CdMode1         => CommonTypes.AaruMetadata.TrackType.Mode1,
                TrackType.Data => sidecar.OpticalDiscs[0].DiscType switch
                {
                    "BD"     => CommonTypes.AaruMetadata.TrackType.Bluray,
                    "DDCD"   => CommonTypes.AaruMetadata.TrackType.Ddcd,
                    "DVD"    => CommonTypes.AaruMetadata.TrackType.Dvd,
                    "HD DVD" => CommonTypes.AaruMetadata.TrackType.HdDvd,
                    _        => CommonTypes.AaruMetadata.TrackType.Mode1
                },
                _ => xmlTrk.Type
            };

            xmlTrk.Sequence = new TrackSequence
            {
                Session = trk.Session,
                Number  = trk.Sequence
            };

            xmlTrk.StartSector = trk.StartSector;
            xmlTrk.EndSector   = trk.EndSector;

            if(trk.Indexes?.TryGetValue(0, out int idx0) == true &&
               idx0                                      >= 0)
                xmlTrk.StartSector = (ulong)idx0;

            switch(sidecar.OpticalDiscs[0].DiscType)
            {
                case "CD":
                case "GD":
                    xmlTrk.StartMsf = LbaToMsf((long)xmlTrk.StartSector);
                    xmlTrk.EndMsf   = LbaToMsf((long)xmlTrk.EndSector);

                    break;
                case "DDCD":
                    xmlTrk.StartMsf = DdcdLbaToMsf((long)xmlTrk.StartSector);
                    xmlTrk.EndMsf   = DdcdLbaToMsf((long)xmlTrk.EndSector);

                    break;
            }

            xmlTrk.Image = new Image
            {
                Value  = Path.GetFileName(trk.File),
                Format = trk.FileType
            };

            if(trk.FileOffset > 0)
                xmlTrk.Image.Offset = trk.FileOffset;

            xmlTrk.Size = (xmlTrk.EndSector - xmlTrk.StartSector + 1) * (ulong)trk.RawBytesPerSector;

            xmlTrk.BytesPerSector = (uint)trk.BytesPerSector;

            const uint sectorsToRead = 512;
            ulong      sectors       = xmlTrk.EndSector - xmlTrk.StartSector + 1;
            ulong      doneSectors   = 0;

            // If there is only one track, and it's the same as the image file (e.g. ".iso" files), don't re-checksum.
            if(image.Id == new Guid("12345678-AAAA-BBBB-CCCC-123456789000") &&

               // Only if filter is none...
               (filterId == new Guid("12345678-AAAA-BBBB-CCCC-123456789000") ||

                // ...or AppleDouble
                filterId == new Guid("1b2165ee-c9df-4b21-bbbb-9e5892b2df4d")))
                xmlTrk.Checksums = sidecar.OpticalDiscs[0].Checksums;
            else
            {
                UpdateProgress(Localization.Core.Track_0_of_1, trk.Sequence, tracks.Count);

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
                        errno = image.ReadSectorsLong(doneSectors, sectorsToRead, xmlTrk.Sequence.Number, out sector);

                        UpdateProgress2(Localization.Core.Hashing_sector_0_of_1, (long)doneSectors,
                                        (long)(trk.EndSector - trk.StartSector + 1));

                        if(errno != ErrorNumber.NoError)
                        {
                            UpdateStatus(string.Format(Localization.Core.Error_0_reading_sector_1, errno, doneSectors));
                            EndProgress2();

                            return;
                        }

                        doneSectors += sectorsToRead;
                    }
                    else
                    {
                        errno = image.ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors),
                                                      xmlTrk.Sequence.Number, out sector);

                        UpdateProgress2(Localization.Core.Hashing_sector_0_of_1, (long)doneSectors,
                                        (long)(trk.EndSector - trk.StartSector + 1));

                        if(errno != ErrorNumber.NoError)
                        {
                            UpdateStatus(string.Format(Localization.Core.Error_0_reading_sector_1, errno, doneSectors));
                            EndProgress2();

                            return;
                        }

                        doneSectors += sectors - doneSectors;
                    }

                    trkChkWorker.Update(sector);
                }

                xmlTrk.Checksums = trkChkWorker.End();

                EndProgress2();
            }

            if(trk.SubchannelType != TrackSubchannelType.None)
            {
                xmlTrk.SubChannel = new SubChannel
                {
                    Image = new Image
                    {
                        Value = trk.SubchannelFile
                    },

                    // TODO: Packed subchannel has different size?
                    Size = (xmlTrk.EndSector - xmlTrk.StartSector + 1) * 96
                };

                switch(trk.SubchannelType)
                {
                    case TrackSubchannelType.Packed:
                    case TrackSubchannelType.PackedInterleaved:
                        xmlTrk.SubChannel.Image.Format = "rw";

                        break;
                    case TrackSubchannelType.Raw:
                    case TrackSubchannelType.RawInterleaved:
                        xmlTrk.SubChannel.Image.Format = "rw_raw";

                        break;
                    case TrackSubchannelType.Q16:
                    case TrackSubchannelType.Q16Interleaved:
                        xmlTrk.SubChannel.Image.Format = "q16";

                        break;
                }

                if(trk.FileOffset > 0)
                    xmlTrk.SubChannel.Image.Offset = trk.SubchannelOffset;

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
                        errno = image.ReadSectorsTag(doneSectors, sectorsToRead, xmlTrk.Sequence.Number,
                                                     SectorTagType.CdSectorSubchannel, out sector);

                        UpdateProgress2(Localization.Core.Hashing_subchannel_sector_0_of_1, (long)doneSectors,
                                        (long)(trk.EndSector - trk.StartSector + 1));

                        if(errno != ErrorNumber.NoError)
                        {
                            UpdateStatus(string.Format(Localization.Core.Error_0_reading_sector_1, errno, doneSectors));
                            EndProgress2();

                            return;
                        }

                        doneSectors += sectorsToRead;
                    }
                    else
                    {
                        errno = image.ReadSectorsTag(doneSectors, (uint)(sectors - doneSectors), xmlTrk.Sequence.Number,
                                                     SectorTagType.CdSectorSubchannel, out sector);

                        UpdateProgress2(Localization.Core.Hashing_subchannel_sector_0_of_1, (long)doneSectors,
                                        (long)(trk.EndSector - trk.StartSector + 1));

                        if(errno != ErrorNumber.NoError)
                        {
                            UpdateStatus(string.Format(Localization.Core.Error_0_reading_sector_1, errno, doneSectors));
                            EndProgress2();

                            return;
                        }

                        doneSectors += sectors - doneSectors;
                    }

                    subChkWorker.Update(sector);
                }

                xmlTrk.SubChannel.Checksums = subChkWorker.End();

                EndProgress2();
            }

            // For fast debugging, skip checksum
            //skipChecksum:

            List<Partition> trkPartitions =
                partitions.Where(p => p.Start >= trk.StartSector && p.End <= trk.EndSector).ToList();

            xmlTrk.FileSystemInformation = new List<CommonTypes.AaruMetadata.Partition>();

            if(trkPartitions.Count > 0)
            {
                foreach(Partition partition in trkPartitions)
                {
                    var metadataPartition = new CommonTypes.AaruMetadata.Partition
                    {
                        Description = partition.Description,
                        EndSector   = partition.End,
                        Name        = partition.Name,
                        Sequence    = (uint)partition.Sequence,
                        StartSector = partition.Start,
                        Type        = partition.Type
                    };

                    List<FileSystem> lstFs = new();

                    foreach(Type pluginType in plugins.Filesystems.Values)
                        try
                        {
                            if(_aborted)
                            {
                                EndProgress();

                                return;
                            }

                            if(Activator.CreateInstance(pluginType) is not IFilesystem fs)
                                continue;

                            if(!fs.Identify(image, partition))
                                continue;

                            fs.GetInformation(image, partition, encoding, out _, out FileSystem fsMetadata);
                            lstFs.Add(fsMetadata);
                            Statistics.AddFilesystem(fsMetadata.Type);

                            dskType = fsMetadata.Type switch
                            {
                                "Opera"                        => MediaType.ThreeDO,
                                "PC Engine filesystem"         => MediaType.SuperCDROM2,
                                "Nintendo Wii filesystem"      => MediaType.WOD,
                                "Nintendo Gamecube filesystem" => MediaType.GOD,
                                _                              => dskType
                            };
                        }
                        #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        catch
                            #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                        {
                            //AaruConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                        }

                    if(lstFs.Count > 0)
                        metadataPartition.FileSystems = lstFs;

                    xmlTrk.FileSystemInformation.Add(metadataPartition);
                }
            }
            else
            {
                var metadataPartition = new CommonTypes.AaruMetadata.Partition
                {
                    EndSector   = xmlTrk.EndSector,
                    StartSector = xmlTrk.StartSector
                };

                List<FileSystem> lstFs = new();

                var xmlPart = new Partition
                {
                    Start    = xmlTrk.StartSector,
                    Length   = xmlTrk.EndSector - xmlTrk.StartSector + 1,
                    Type     = xmlTrk.Type.ToString(),
                    Size     = xmlTrk.Size,
                    Sequence = xmlTrk.Sequence.Number
                };

                foreach(Type pluginType in plugins.Filesystems.Values)
                    try
                    {
                        if(_aborted)
                        {
                            EndProgress();

                            return;
                        }

                        if(Activator.CreateInstance(pluginType) is not IFilesystem fs)
                            continue;

                        if(!fs.Identify(image, xmlPart))
                            continue;

                        fs.GetInformation(image, xmlPart, encoding, out _, out FileSystem fsMetadata);
                        lstFs.Add(fsMetadata);
                        Statistics.AddFilesystem(fsMetadata.Type);

                        dskType = fsMetadata.Type switch
                        {
                            "Opera"                        => MediaType.ThreeDO,
                            "PC Engine filesystem"         => MediaType.SuperCDROM2,
                            "Nintendo Wii filesystem"      => MediaType.WOD,
                            "Nintendo Gamecube filesystem" => MediaType.GOD,
                            _                              => dskType
                        };
                    }
                    #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                    catch
                        #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                    {
                        //AaruConsole.DebugWriteLine("Create-sidecar command", "Plugin {0} crashed", _plugin.Name);
                    }

                if(lstFs.Count > 0)
                    metadataPartition.FileSystems = lstFs;

                xmlTrk.FileSystemInformation.Add(metadataPartition);
            }

            errno = image.ReadSectorTag(trk.Sequence, SectorTagType.CdTrackIsrc, out byte[] isrcData);

            if(errno == ErrorNumber.NoError)
                xmlTrk.ISRC = Encoding.UTF8.GetString(isrcData);

            errno = image.ReadSectorTag(trk.Sequence, SectorTagType.CdTrackFlags, out byte[] flagsData);

            if(errno == ErrorNumber.NoError)
            {
                var trackFlags = (CdFlags)flagsData[0];

                xmlTrk.Flags = new TrackFlags
                {
                    PreEmphasis   = trackFlags.HasFlag(CdFlags.PreEmphasis),
                    CopyPermitted = trackFlags.HasFlag(CdFlags.CopyPermitted),
                    Data          = trackFlags.HasFlag(CdFlags.DataTrack),
                    Quadraphonic  = trackFlags.HasFlag(CdFlags.FourChannel)
                };
            }

            if(trk.Indexes?.Count > 0)
                xmlTrk.Indexes = trk.Indexes?.OrderBy(i => i.Key).Select(i => new TrackIndex
                {
                    Index = i.Key,
                    Value = i.Value
                }).ToList();

            trksLst.Add(xmlTrk);
        }

        EndProgress();

        if(trksLst != null)
            sidecar.OpticalDiscs[0].Track = trksLst;

        // All XGD3 all have the same number of blocks
        if(dskType                             == MediaType.XGD2 &&
           sidecar.OpticalDiscs[0].Track.Count == 1)
        {
            ulong blocks = sidecar.OpticalDiscs[0].Track[0].EndSector - sidecar.OpticalDiscs[0].Track[0].StartSector +
                           1;

            if(blocks is 25063 or 4229664 or 4246304) // Wxripper unlock
                dskType = MediaType.XGD3;
        }

        (string type, string subType) discType = CommonTypes.Metadata.MediaType.MediaTypeToString(dskType);
        sidecar.OpticalDiscs[0].DiscType    = discType.type;
        sidecar.OpticalDiscs[0].DiscSubType = discType.subType;
        Statistics.AddMedia(dskType, false);

        if(image.DumpHardware != null)
            sidecar.OpticalDiscs[0].DumpHardware = image.DumpHardware;
        else if(!string.IsNullOrEmpty(image.Info.DriveManufacturer)     ||
                !string.IsNullOrEmpty(image.Info.DriveModel)            ||
                !string.IsNullOrEmpty(image.Info.DriveFirmwareRevision) ||
                !string.IsNullOrEmpty(image.Info.DriveSerialNumber))
            sidecar.OpticalDiscs[0].DumpHardware = new List<DumpHardware>
            {
                new()
                {
                    Extents = new List<Extent>
                    {
                        new()
                        {
                            Start = 0,
                            End   = image.Info.Sectors
                        }
                    },
                    Manufacturer = image.Info.DriveManufacturer,
                    Model        = image.Info.DriveModel,
                    Firmware     = image.Info.DriveFirmwareRevision,
                    Serial       = image.Info.DriveSerialNumber,
                    Software = new Software
                    {
                        Name    = image.Info.Application,
                        Version = image.Info.ApplicationVersion
                    }
                }
            };
    }
}