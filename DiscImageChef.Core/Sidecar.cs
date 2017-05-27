// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Sidecar.cs
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
using DiscImageChef.Console;
using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Filesystems;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using Schemas;

namespace DiscImageChef.Core
{
    public static class Sidecar
    {
        public static CICMMetadataType Create(ImagePlugin image, string imagePath)
        {
            CICMMetadataType sidecar = new CICMMetadataType();
            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();

            FileInfo fi = new FileInfo(imagePath);
            FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read);

            Checksum imgChkWorker = new Checksum();

            // For fast debugging, skip checksum
            //goto skipImageChecksum;

            byte[] data;
            long position = 0;
            while(position < (fi.Length - 1048576))
            {
                data = new byte[1048576];
                fs.Read(data, 0, 1048576);

                DicConsole.Write("\rHashing image file byte {0} of {1}", position, fi.Length);

                imgChkWorker.Update(data);

                position += 1048576;
            }

            data = new byte[fi.Length - position];
            fs.Read(data, 0, (int)(fi.Length - position));

            DicConsole.Write("\rHashing image file byte {0} of {1}", position, fi.Length);

            imgChkWorker.Update(data);

            // For fast debugging, skip checksum
            //skipImageChecksum:

            DicConsole.WriteLine();
            fs.Close();

            List<ChecksumType> imgChecksums = imgChkWorker.End();

            switch(image.ImageInfo.xmlMediaType)
            {
                case XmlMediaType.OpticalDisc:
                    {
                        sidecar.OpticalDisc = new OpticalDiscType[1];
                        sidecar.OpticalDisc[0] = new OpticalDiscType();
                        sidecar.OpticalDisc[0].Checksums = imgChecksums.ToArray();
                        sidecar.OpticalDisc[0].Image = new ImageType();
                        sidecar.OpticalDisc[0].Image.format = image.GetImageFormat();
                        sidecar.OpticalDisc[0].Image.offset = 0;
                        sidecar.OpticalDisc[0].Image.offsetSpecified = true;
                        sidecar.OpticalDisc[0].Image.Value = Path.GetFileName(imagePath);
                        sidecar.OpticalDisc[0].Size = fi.Length;
                        sidecar.OpticalDisc[0].Sequence = new SequenceType();
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
                        sidecar.OpticalDisc[0].Sequence.MediaTitle = image.GetImageName();

                        MediaType dskType = image.ImageInfo.mediaType;

                        foreach(MediaTagType tagType in image.ImageInfo.readableMediaTags)
                        {
                            switch(tagType)
                            {
                                case MediaTagType.CD_ATIP:
                                    sidecar.OpticalDisc[0].ATIP = new DumpType();
                                    sidecar.OpticalDisc[0].ATIP.Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.CD_ATIP)).ToArray();
                                    sidecar.OpticalDisc[0].ATIP.Size = image.ReadDiskTag(MediaTagType.CD_ATIP).Length;
                                    Decoders.CD.ATIP.CDATIP? atip = Decoders.CD.ATIP.Decode(image.ReadDiskTag(MediaTagType.CD_ATIP));
                                    if(atip.HasValue)
                                    {
                                        if(atip.Value.DDCD)
                                            dskType = atip.Value.DiscType ? MediaType.DDCDRW : MediaType.DDCDR;
                                        else
                                            dskType = atip.Value.DiscType ? MediaType.CDRW : MediaType.CDR;
                                    }
                                    break;
                                case MediaTagType.DVD_BCA:
                                    sidecar.OpticalDisc[0].BCA = new DumpType();
                                    sidecar.OpticalDisc[0].BCA.Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.DVD_BCA)).ToArray();
                                    sidecar.OpticalDisc[0].BCA.Size = image.ReadDiskTag(MediaTagType.DVD_BCA).Length;
                                    break;
                                case MediaTagType.BD_BCA:
                                    sidecar.OpticalDisc[0].BCA = new DumpType();
                                    sidecar.OpticalDisc[0].BCA.Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.BD_BCA)).ToArray();
                                    sidecar.OpticalDisc[0].BCA.Size = image.ReadDiskTag(MediaTagType.BD_BCA).Length;
                                    break;
                                case MediaTagType.DVD_CMI:
                                    sidecar.OpticalDisc[0].CMI = new DumpType();
                                    Decoders.DVD.CSS_CPRM.LeadInCopyright? cmi = Decoders.DVD.CSS_CPRM.DecodeLeadInCopyright(image.ReadDiskTag(MediaTagType.DVD_CMI));
                                    if(cmi.HasValue)
                                    {
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
                                    }
                                    sidecar.OpticalDisc[0].CMI.Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.DVD_CMI)).ToArray();
                                    sidecar.OpticalDisc[0].CMI.Size = image.ReadDiskTag(MediaTagType.DVD_CMI).Length;
                                    break;
                                case MediaTagType.DVD_DMI:
                                    sidecar.OpticalDisc[0].DMI = new DumpType();
                                    sidecar.OpticalDisc[0].DMI.Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.DVD_DMI)).ToArray();
                                    sidecar.OpticalDisc[0].DMI.Size = image.ReadDiskTag(MediaTagType.DVD_DMI).Length;
                                    if(Decoders.Xbox.DMI.IsXbox(image.ReadDiskTag(MediaTagType.DVD_DMI)))
                                    {
                                        dskType = MediaType.XGD;
                                        sidecar.OpticalDisc[0].Dimensions = new DimensionsType();
                                        sidecar.OpticalDisc[0].Dimensions.Diameter = 120;
                                    }
                                    else if(Decoders.Xbox.DMI.IsXbox360(image.ReadDiskTag(MediaTagType.DVD_DMI)))
                                    {
                                        dskType = MediaType.XGD2;
                                        sidecar.OpticalDisc[0].Dimensions = new DimensionsType();
                                        sidecar.OpticalDisc[0].Dimensions.Diameter = 120;
                                    }
                                    break;
                                case MediaTagType.DVD_PFI:
                                    sidecar.OpticalDisc[0].PFI = new DumpType();
                                    sidecar.OpticalDisc[0].PFI.Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.DVD_PFI)).ToArray();
                                    sidecar.OpticalDisc[0].PFI.Size = image.ReadDiskTag(MediaTagType.DVD_PFI).Length;
                                    Decoders.DVD.PFI.PhysicalFormatInformation? pfi = Decoders.DVD.PFI.Decode(image.ReadDiskTag(MediaTagType.DVD_PFI));
                                    if(pfi.HasValue)
                                    {
                                        if(dskType != MediaType.XGD &&
                                            dskType != MediaType.XGD2 &&
                                            dskType != MediaType.XGD3)
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

                                            if(dskType == MediaType.DVDR && pfi.Value.PartVersion == 6)
                                                dskType = MediaType.DVDRDL;
                                            if(dskType == MediaType.DVDRW && pfi.Value.PartVersion == 3)
                                                dskType = MediaType.DVDRWDL;
                                            if(dskType == MediaType.GOD && pfi.Value.DiscSize == Decoders.DVD.DVDSize.OneTwenty)
                                                dskType = MediaType.WOD;

                                            sidecar.OpticalDisc[0].Dimensions = new DimensionsType();
                                            if(dskType == MediaType.UMD)
                                                sidecar.OpticalDisc[0].Dimensions.Diameter = 60;
                                            else if(pfi.Value.DiscSize == Decoders.DVD.DVDSize.Eighty)
                                                sidecar.OpticalDisc[0].Dimensions.Diameter = 80;
                                            else if(pfi.Value.DiscSize == Decoders.DVD.DVDSize.OneTwenty)
                                                sidecar.OpticalDisc[0].Dimensions.Diameter = 120;
                                        }
                                    }
                                    break;
                                case MediaTagType.CD_PMA:
                                    sidecar.OpticalDisc[0].PMA = new DumpType();
                                    sidecar.OpticalDisc[0].PMA.Checksums = Checksum.GetChecksums(image.ReadDiskTag(MediaTagType.CD_PMA)).ToArray();
                                    sidecar.OpticalDisc[0].PMA.Size = image.ReadDiskTag(MediaTagType.CD_PMA).Length;
                                    break;
                            }
                        }

                        try
                        {
                            List<Session> sessions = image.GetSessions();
                            sidecar.OpticalDisc[0].Sessions = sessions != null ? sessions.Count : 1;
                        }
                        catch
                        {
                            sidecar.OpticalDisc[0].Sessions = 1;
                        }

                        List<Track> tracks = image.GetTracks();
                        List<Schemas.TrackType> trksLst = null;
                        if(tracks != null)
                        {
                            sidecar.OpticalDisc[0].Tracks = new int[1];
                            sidecar.OpticalDisc[0].Tracks[0] = tracks.Count;
                            trksLst = new List<Schemas.TrackType>();
                        }

                        foreach(Track trk in tracks)
                        {
                            Schemas.TrackType xmlTrk = new Schemas.TrackType();
                            switch(trk.TrackType)
                            {
                                case ImagePlugins.TrackType.Audio:
                                    xmlTrk.TrackType1 = TrackTypeTrackType.audio;
                                    break;
                                case ImagePlugins.TrackType.CDMode2Form2:
                                    xmlTrk.TrackType1 = TrackTypeTrackType.m2f2;
                                    break;
                                case ImagePlugins.TrackType.CDMode2Formless:
                                    xmlTrk.TrackType1 = TrackTypeTrackType.mode2;
                                    break;
                                case ImagePlugins.TrackType.CDMode2Form1:
                                    xmlTrk.TrackType1 = TrackTypeTrackType.m2f1;
                                    break;
                                case ImagePlugins.TrackType.CDMode1:
                                    xmlTrk.TrackType1 = TrackTypeTrackType.mode1;
                                    break;
                                case ImagePlugins.TrackType.Data:
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
                            xmlTrk.Sequence = new TrackSequenceType();
                            xmlTrk.Sequence.Session = trk.TrackSession;
                            xmlTrk.Sequence.TrackNumber = (int)trk.TrackSequence;
                            xmlTrk.StartSector = (long)trk.TrackStartSector;
                            xmlTrk.EndSector = (long)trk.TrackEndSector;

                            if(trk.Indexes != null && trk.Indexes.ContainsKey(0))
                            {
                                ulong idx0;
                                if(trk.Indexes.TryGetValue(0, out idx0))
                                    xmlTrk.StartSector = (long)idx0;
                            }

                            if(sidecar.OpticalDisc[0].DiscType == "CD" ||
                                sidecar.OpticalDisc[0].DiscType == "GD")
                            {
                                xmlTrk.StartMSF = LbaToMsf(xmlTrk.StartSector);
                                xmlTrk.EndMSF = LbaToMsf(xmlTrk.EndSector);
                            }
                            else if(sidecar.OpticalDisc[0].DiscType == "DDCD")
                            {
                                xmlTrk.StartMSF = DdcdLbaToMsf(xmlTrk.StartSector);
                                xmlTrk.EndMSF = DdcdLbaToMsf(xmlTrk.EndSector);
                            }

                            xmlTrk.Image = new ImageType();
                            xmlTrk.Image.Value = Path.GetFileName(trk.TrackFile);
                            if(trk.TrackFileOffset > 0)
                            {
                                xmlTrk.Image.offset = (long)trk.TrackFileOffset;
                                xmlTrk.Image.offsetSpecified = true;
                            }

                            xmlTrk.Image.format = trk.TrackFileType;
                            xmlTrk.Size = (xmlTrk.EndSector - xmlTrk.StartSector + 1) * trk.TrackRawBytesPerSector;
                            xmlTrk.BytesPerSector = trk.TrackBytesPerSector;

                            // For fast debugging, skip checksum
                            //goto skipChecksum;

                            uint sectorsToRead = 512;

                            Checksum trkChkWorker = new Checksum();

                            ulong sectors = (ulong)(xmlTrk.EndSector - xmlTrk.StartSector + 1);
                            ulong doneSectors = 0;

                            while(doneSectors < sectors)
                            {
                                byte[] sector;

                                if((sectors - doneSectors) >= sectorsToRead)
                                {
                                    sector = image.ReadSectorsLong(doneSectors, sectorsToRead, (uint)xmlTrk.Sequence.TrackNumber);
                                    DicConsole.Write("\rHashings sectors {0} to {2} of track {1} ({3} sectors)", doneSectors, xmlTrk.Sequence.TrackNumber, doneSectors + sectorsToRead, sectors);
                                    doneSectors += sectorsToRead;
                                }
                                else
                                {
                                    sector = image.ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors), (uint)xmlTrk.Sequence.TrackNumber);
                                    DicConsole.Write("\rHashings sectors {0} to {2} of track {1} ({3} sectors)", doneSectors, xmlTrk.Sequence.TrackNumber, doneSectors + (sectors - doneSectors), sectors);
                                    doneSectors += (sectors - doneSectors);
                                }

                                trkChkWorker.Update(sector);
                            }

                            List<ChecksumType> trkChecksums = trkChkWorker.End();

                            xmlTrk.Checksums = trkChecksums.ToArray();

                            DicConsole.WriteLine();

                            if(trk.TrackSubchannelType != TrackSubchannelType.None)
                            {
                                xmlTrk.SubChannel = new SubChannelType();
                                xmlTrk.SubChannel.Image = new ImageType();
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
                                xmlTrk.SubChannel.Image.Value = trk.TrackSubchannelFile;

                                // TODO: Packed subchannel has different size?
                                xmlTrk.SubChannel.Size = (xmlTrk.EndSector - xmlTrk.StartSector + 1) * 96;

                                Checksum subChkWorker = new Checksum();

                                sectors = (ulong)(xmlTrk.EndSector - xmlTrk.StartSector + 1);
                                doneSectors = 0;

                                while(doneSectors < sectors)
                                {
                                    byte[] sector;

                                    if((sectors - doneSectors) >= sectorsToRead)
                                    {
                                        sector = image.ReadSectorsTag(doneSectors, sectorsToRead, (uint)xmlTrk.Sequence.TrackNumber, SectorTagType.CDSectorSubchannel);
                                        DicConsole.Write("\rHashings subchannel sectors {0} to {2} of track {1} ({3} sectors)", doneSectors, xmlTrk.Sequence.TrackNumber, doneSectors + sectorsToRead, sectors);
                                        doneSectors += sectorsToRead;
                                    }
                                    else
                                    {
                                        sector = image.ReadSectorsTag(doneSectors, (uint)(sectors - doneSectors), (uint)xmlTrk.Sequence.TrackNumber, SectorTagType.CDSectorSubchannel);
                                        DicConsole.Write("\rHashings subchannel sectors {0} to {2} of track {1} ({3} sectors)", doneSectors, xmlTrk.Sequence.TrackNumber, doneSectors + (sectors - doneSectors), sectors);
                                        doneSectors += (sectors - doneSectors);
                                    }

                                    subChkWorker.Update(sector);
                                }

                                List<ChecksumType> subChecksums = subChkWorker.End();

                                xmlTrk.SubChannel.Checksums = subChecksums.ToArray();

                                DicConsole.WriteLine();
                            }

                            // For fast debugging, skip checksum
                            //skipChecksum:

                            DicConsole.WriteLine("Checking filesystems on track {0} from sector {1} to {2}", xmlTrk.Sequence.TrackNumber, xmlTrk.StartSector, xmlTrk.EndSector);

                            List<Partition> partitions = new List<Partition>();

                            foreach(PartPlugin _partplugin in plugins.PartPluginsList.Values)
                            {
                                List<Partition> _partitions;

                                if(_partplugin.GetInformation(image, out _partitions))
                                {
                                    partitions.AddRange(_partitions);
                                    Statistics.AddPartition(_partplugin.Name);
                                }
                            }

                            xmlTrk.FileSystemInformation = new PartitionType[1];
                            if(partitions.Count > 0)
                            {
                                xmlTrk.FileSystemInformation = new PartitionType[partitions.Count];
                                for(int i = 0; i < partitions.Count; i++)
                                {
                                    xmlTrk.FileSystemInformation[i] = new PartitionType();
                                    xmlTrk.FileSystemInformation[i].Description = partitions[i].PartitionDescription;
                                    xmlTrk.FileSystemInformation[i].EndSector = (int)(partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1);
                                    xmlTrk.FileSystemInformation[i].Name = partitions[i].PartitionName;
                                    xmlTrk.FileSystemInformation[i].Sequence = (int)partitions[i].PartitionSequence;
                                    xmlTrk.FileSystemInformation[i].StartSector = (int)partitions[i].PartitionStartSector;
                                    xmlTrk.FileSystemInformation[i].Type = partitions[i].PartitionType;

                                    List<FileSystemType> lstFs = new List<FileSystemType>();

                                    foreach(Filesystem _plugin in plugins.PluginsList.Values)
                                    {
                                        try
                                        {
                                            if(_plugin.Identify(image, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1))
                                            {
                                                string foo;
                                                _plugin.GetInformation(image, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1, out foo);
                                                lstFs.Add(_plugin.XmlFSType);
                                                Statistics.AddFilesystem(_plugin.XmlFSType.Type);

                                                if(_plugin.XmlFSType.Type == "Opera")
                                                    dskType = MediaType.ThreeDO;
                                                if(_plugin.XmlFSType.Type == "PC Engine filesystem")
                                                    dskType = MediaType.SuperCDROM2;
                                                if(_plugin.XmlFSType.Type == "Nintendo Wii filesystem")
                                                    dskType = MediaType.WOD;
                                                if(_plugin.XmlFSType.Type == "Nintendo Gamecube filesystem")
                                                    dskType = MediaType.GOD;
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
                                        xmlTrk.FileSystemInformation[i].FileSystems = lstFs.ToArray();
                                }
                            }
                            else
                            {
                                xmlTrk.FileSystemInformation[0] = new PartitionType();
                                xmlTrk.FileSystemInformation[0].EndSector = (int)xmlTrk.EndSector;
                                xmlTrk.FileSystemInformation[0].StartSector = (int)xmlTrk.StartSector;

                                List<FileSystemType> lstFs = new List<FileSystemType>();

                                foreach(Filesystem _plugin in plugins.PluginsList.Values)
                                {
                                    try
                                    {
                                        if(_plugin.Identify(image, (ulong)xmlTrk.StartSector, (ulong)xmlTrk.EndSector))
                                        {
                                            string foo;
                                            _plugin.GetInformation(image, (ulong)xmlTrk.StartSector, (ulong)xmlTrk.EndSector, out foo);
                                            lstFs.Add(_plugin.XmlFSType);
                                            Statistics.AddFilesystem(_plugin.XmlFSType.Type);

                                            if(_plugin.XmlFSType.Type == "Opera")
                                                dskType = MediaType.ThreeDO;
                                            if(_plugin.XmlFSType.Type == "PC Engine filesystem")
                                                dskType = MediaType.SuperCDROM2;
                                            if(_plugin.XmlFSType.Type == "Nintendo Wii filesystem")
                                                dskType = MediaType.WOD;
                                            if(_plugin.XmlFSType.Type == "Nintendo Gamecube filesystem")
                                                dskType = MediaType.GOD;
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
                                    xmlTrk.FileSystemInformation[0].FileSystems = lstFs.ToArray();
                            }

                            trksLst.Add(xmlTrk);
                        }

                        if(trksLst != null)
                            sidecar.OpticalDisc[0].Track = trksLst.ToArray();

                        // All XGD3 all have the same number of blocks
                        if(dskType == MediaType.XGD2 && sidecar.OpticalDisc[0].Track.Length == 1)
                        {
                            ulong blocks = (ulong)(sidecar.OpticalDisc[0].Track[0].EndSector - sidecar.OpticalDisc[0].Track[0].StartSector + 1);
                            if(blocks == 25063 || // Locked (or non compatible drive)
                               blocks == 4229664 || // Xtreme unlock
                               blocks == 4246304) // Wxripper unlock
                                dskType = MediaType.XGD3;
                        }


                        string dscType, dscSubType;
                        Metadata.MediaType.MediaTypeToString(dskType, out dscType, out dscSubType);
                        sidecar.OpticalDisc[0].DiscType = dscType;
                        sidecar.OpticalDisc[0].DiscSubType = dscSubType;
                        Statistics.AddMedia(dskType, false);

                        if(!string.IsNullOrEmpty(image.ImageInfo.driveManufacturer) ||
                           !string.IsNullOrEmpty(image.ImageInfo.driveModel) ||
                           !string.IsNullOrEmpty(image.ImageInfo.driveFirmwareRevision) ||
                           !string.IsNullOrEmpty(image.ImageInfo.driveSerialNumber))
                        {
                            sidecar.OpticalDisc[0].DumpHardwareArray = new DumpHardwareType[1];
                            sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents = new ExtentType[0];
                            sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents[0].Start = 0;
                            sidecar.OpticalDisc[0].DumpHardwareArray[0].Extents[0].End = (int)image.ImageInfo.sectors;
                            sidecar.OpticalDisc[0].DumpHardwareArray[0].Manufacturer = image.ImageInfo.driveManufacturer;
                            sidecar.OpticalDisc[0].DumpHardwareArray[0].Model = image.ImageInfo.driveModel;
                            sidecar.OpticalDisc[0].DumpHardwareArray[0].Firmware = image.ImageInfo.driveFirmwareRevision;
                            sidecar.OpticalDisc[0].DumpHardwareArray[0].Serial = image.ImageInfo.driveSerialNumber;
                            sidecar.OpticalDisc[0].DumpHardwareArray[0].Software = new SoftwareType();
                            sidecar.OpticalDisc[0].DumpHardwareArray[0].Software.Name = image.GetImageApplication();
                            sidecar.OpticalDisc[0].DumpHardwareArray[0].Software.Version = image.GetImageApplicationVersion();
                        }

                        break;
                    }
                case XmlMediaType.BlockMedia:
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

                        DicConsole.WriteLine("Checking filesystems...");

                        List<Partition> partitions = new List<Partition>();

                        foreach(PartPlugin _partplugin in plugins.PartPluginsList.Values)
                        {
                            List<Partition> _partitions;

                            if(_partplugin.GetInformation(image, out _partitions))
                            {
                                partitions = _partitions;
                                Statistics.AddPartition(_partplugin.Name);
                                break;
                            }
                        }

                        sidecar.BlockMedia[0].FileSystemInformation = new PartitionType[1];
                        if(partitions.Count > 0)
                        {
                            sidecar.BlockMedia[0].FileSystemInformation = new PartitionType[partitions.Count];
                            for(int i = 0; i < partitions.Count; i++)
                            {
                                sidecar.BlockMedia[0].FileSystemInformation[i] = new PartitionType();
                                sidecar.BlockMedia[0].FileSystemInformation[i].Description = partitions[i].PartitionDescription;
                                sidecar.BlockMedia[0].FileSystemInformation[i].EndSector = (int)(partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1);
                                sidecar.BlockMedia[0].FileSystemInformation[i].Name = partitions[i].PartitionName;
                                sidecar.BlockMedia[0].FileSystemInformation[i].Sequence = (int)partitions[i].PartitionSequence;
                                sidecar.BlockMedia[0].FileSystemInformation[i].StartSector = (int)partitions[i].PartitionStartSector;
                                sidecar.BlockMedia[0].FileSystemInformation[i].Type = partitions[i].PartitionType;

                                List<FileSystemType> lstFs = new List<FileSystemType>();

                                foreach(Filesystem _plugin in plugins.PluginsList.Values)
                                {
                                    try
                                    {
                                        if(_plugin.Identify(image, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1))
                                        {
                                            string foo;
                                            _plugin.GetInformation(image, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector + partitions[i].PartitionSectors - 1, out foo);
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

                            List<FileSystemType> lstFs = new List<FileSystemType>();

                            foreach(Filesystem _plugin in plugins.PluginsList.Values)
                            {
                                try
                                {
                                    if(_plugin.Identify(image, 0, image.GetSectors() - 1))
                                    {
                                        string foo;
                                        _plugin.GetInformation(image, 0, image.GetSectors() - 1, out foo);
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
                        break;
                    }
                case XmlMediaType.LinearMedia:
                    {
                        sidecar.LinearMedia = new LinearMediaType[1];
                        sidecar.LinearMedia[0] = new LinearMediaType();
                        sidecar.LinearMedia[0].Checksums = imgChecksums.ToArray();
                        sidecar.LinearMedia[0].Image = new ImageType();
                        sidecar.LinearMedia[0].Image.format = image.GetImageFormat();
                        sidecar.LinearMedia[0].Image.offset = 0;
                        sidecar.LinearMedia[0].Image.offsetSpecified = true;
                        sidecar.LinearMedia[0].Image.Value = Path.GetFileName(imagePath);
                        sidecar.LinearMedia[0].Size = fi.Length;

                        //MediaType dskType = image.ImageInfo.diskType;
                        // TODO: Complete it
                        break;
                    }
                case XmlMediaType.AudioMedia:
                    {
                        sidecar.AudioMedia = new AudioMediaType[1];
                        sidecar.AudioMedia[0] = new AudioMediaType();
                        sidecar.AudioMedia[0].Checksums = imgChecksums.ToArray();
                        sidecar.AudioMedia[0].Image = new ImageType();
                        sidecar.AudioMedia[0].Image.format = image.GetImageFormat();
                        sidecar.AudioMedia[0].Image.offset = 0;
                        sidecar.AudioMedia[0].Image.offsetSpecified = true;
                        sidecar.AudioMedia[0].Image.Value = Path.GetFileName(imagePath);
                        sidecar.AudioMedia[0].Size = fi.Length;
                        sidecar.AudioMedia[0].Sequence = new SequenceType();
                        if(image.GetMediaSequence() != 0 && image.GetLastDiskSequence() != 0)
                        {
                            sidecar.AudioMedia[0].Sequence.MediaSequence = image.GetMediaSequence();
                            sidecar.AudioMedia[0].Sequence.TotalMedia = image.GetMediaSequence();
                        }
                        else
                        {
                            sidecar.AudioMedia[0].Sequence.MediaSequence = 1;
                            sidecar.AudioMedia[0].Sequence.TotalMedia = 1;
                        }
                        sidecar.AudioMedia[0].Sequence.MediaTitle = image.GetImageName();

                        //MediaType dskType = image.ImageInfo.diskType;
                        // TODO: Complete it
                        break;
                    }

            }

            return sidecar;
        }

        static string LbaToMsf(long lba)
        {
            long m, s, f;
            if(lba >= -150)
            {
                m = (lba + 150) / (75 * 60);
                lba -= m * (75 * 60);
                s = (lba + 150) / 75;
                lba -= s * 75;
                f = lba + 150;
            }
            else
            {
                m = (lba + 450150) / (75 * 60);
                lba -= m * (75 * 60);
                s = (lba + 450150) / 75;
                lba -= s * 75;
                f = lba + 450150;
            }

            return string.Format("{0}:{1:D2}:{2:D2}", m, s, f);
        }

        static string DdcdLbaToMsf(long lba)
        {
            long h, m, s, f;
            if(lba >= -150)
            {
                h = (lba + 150) / (75 * 60 * 60);
                lba -= h * (75 * 60 * 60);
                m = (lba + 150) / (75 * 60);
                lba -= m * (75 * 60);
                s = (lba + 150) / 75;
                lba -= s * 75;
                f = lba + 150;
            }
            else
            {
                h = (lba + 450150 * 2) / (75 * 60 * 60);
                lba -= h * (75 * 60 * 60);
                m = (lba + 450150 * 2) / (75 * 60);
                lba -= m * (75 * 60);
                s = (lba + 450150 * 2) / 75;
                lba -= s * 75;
                f = lba + 450150 * 2;
            }

            return string.Format("{3}:{0:D2}:{1:D2}:{2:D2}", m, s, f, h);
        }
    }
}
