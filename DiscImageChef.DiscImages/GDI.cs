// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : GDI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Dreamcast GDI disc images.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DiscImageChef.Console;
using DiscImageChef.CommonTypes;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
    // TODO: There seems no be no clear definition on how to treat pregaps that are not included in the file, so this is just appending it to start of track
    // TODO: This format doesn't support to specify pregaps that are included in the file (like Redump ones)
    public class GDI : ImagePlugin
    {
        #region Internal structures

        struct GDITrack
        {
            /// <summary>Track #</summary>
            public uint sequence;
            /// <summary>Track filter</summary>
            public Filter trackfilter;
            /// <summary>Track file</summary>
            public string trackfile;
            /// <summary>Track byte offset in file</summary>
            public long offset;
            /// <summary>Track flags</summary>
            public byte flags;
            /// <summary>Track starting sector</summary>
            public ulong startSector;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors in track</summary>
            public ulong sectors;
            /// <summary>Track type</summary>
            public TrackType tracktype;
            /// <summary>Track session</summary>
            public bool highDensity;
            /// <summary>Pregap sectors not stored in track file</summary>
            public ulong pregap;
        }

        struct GDIDisc
        {
            /// <summary>Sessions</summary>
            public List<Session> sessions;
            /// <summary>Tracks</summary>
            public List<GDITrack> tracks;
            /// <summary>Disk type</summary>
            public MediaType disktype;
        }

        #endregion Internal structures

        #region Internal variables

        StreamReader gdiStream;
        Stream imageStream;
        /// <summary>Dictionary, index is track #, value is track number, or 0 if a TOC</summary>
        Dictionary<uint, ulong> offsetmap;
        GDIDisc discimage;
        List<Partition> partitions;
        ulong densitySeparationSectors;

        #endregion Internal variables

        #region Parsing regexs

        const string TrackRegEx = "\\s?(?<track>\\d+)\\s+(?<start>\\d+)\\s(?<flags>\\d)\\s(?<type>2352|2048)\\s(?<filename>.+)\\s(?<offset>\\d+)$";

        #endregion Parsing regexs

        #region Public methods

        public GDI()
        {
            Name = "Dreamcast GDI image";
            PluginUUID = new Guid("281ECBF2-D2A7-414C-8497-1A33F6DCB2DD");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = true;
            ImageInfo.imageHasSessions = true;
            ImageInfo.imageVersion = null;
            ImageInfo.imageApplicationVersion = null;
            ImageInfo.imageName = null;
            ImageInfo.imageCreator = null;
            ImageInfo.mediaManufacturer = null;
            ImageInfo.mediaModel = null;
            ImageInfo.mediaPartNumber = null;
            ImageInfo.mediaSequence = 0;
            ImageInfo.lastMediaSequence = 0;
            ImageInfo.driveManufacturer = null;
            ImageInfo.driveModel = null;
            ImageInfo.driveSerialNumber = null;
            ImageInfo.driveFirmwareRevision = null;
        }

        // Due to .gdi format, this method must parse whole file, ignoring errors (those will be thrown by OpenImage()).
        public override bool IdentifyImage(Filter imageFilter)
        {
            try
            {
				imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
				byte[] testArray = new byte[512];
				imageFilter.GetDataForkStream().Read(testArray, 0, 512);
				imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
				// Check for unexpected control characters that shouldn't be present in a text file and can crash this plugin
				bool twoConsecutiveNulls = false;
				for(int i = 0; i < 512; i++)
				{
					if(i >= imageFilter.GetDataForkStream().Length)
						break;

					if(testArray[i] == 0)
					{
						if(twoConsecutiveNulls)
							return false;
						twoConsecutiveNulls = true;
					}
					else
						twoConsecutiveNulls = false;

					if(testArray[i] < 0x20 && testArray[i] != 0x0A && testArray[i] != 0x0D && testArray[i] != 0x00)
						return false;
				}
				gdiStream = new StreamReader(imageFilter.GetDataForkStream());
                int line = 0;
                int tracksFound = 0;
                int tracks = 0;

                while(gdiStream.Peek() >= 0)
                {
                    line++;
                    string _line = gdiStream.ReadLine();

                    if(line == 1)
                    {
                        if(!int.TryParse(_line, out tracks))
                            return false;
                    }
                    else
                    {
                        Regex RegexTrack = new Regex(TrackRegEx);

                        Match TrackMatch = RegexTrack.Match(_line);

                        if(!TrackMatch.Success)
                            return false;

                        tracksFound++;
                    }
                }

                if(tracks == 0)
                    return false;

                return tracks == tracksFound;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception trying to identify image file {0}", imageFilter.GetBasePath());
                DicConsole.ErrorWriteLine("Exception: {0}", ex.Message);
                DicConsole.ErrorWriteLine("Stack trace: {0}", ex.StackTrace);
                return false;
            }
        }

        public override bool OpenImage(Filter imageFilter)
        {
            if(imageFilter == null)
                return false;

            try
            {
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                gdiStream = new StreamReader(imageFilter.GetDataForkStream());
                int line = 0;
                int tracksFound = 0;
                int tracks = 0;
                bool highDensity = false;

                // Initialize all RegExs
                Regex RegexTrack = new Regex(TrackRegEx);

                // Initialize all RegEx matches
                Match TrackMatch;

                // Initialize disc
                discimage = new GDIDisc();
                discimage.sessions = new List<Session>();
                discimage.tracks = new List<GDITrack>();

                ulong currentStart = 0;
                offsetmap = new Dictionary<uint, ulong>();
                GDITrack currentTrack;
                densitySeparationSectors = 0;

				FiltersList filtersList;

                while(gdiStream.Peek() >= 0)
                {
                    line++;
                    string _line = gdiStream.ReadLine();

                    if(line == 1)
                    {
                        if(!int.TryParse(_line, out tracks))
                            throw new ImageNotSupportedException("Not a correct Dreamcast GDI image");
                    }
                    else
                    {
                        TrackMatch = RegexTrack.Match(_line);

                        if(!TrackMatch.Success)
                            throw new ImageNotSupportedException(string.Format("Unknown line \"{0}\" at line {1}", _line, line));

                        tracksFound++;

                        DicConsole.DebugWriteLine("GDI plugin", "Found track {0} starts at {1} flags {2} type {3} file {4} offset {5} at line {6}",
                            TrackMatch.Groups["track"].Value, TrackMatch.Groups["start"].Value, TrackMatch.Groups["flags"].Value,
                            TrackMatch.Groups["type"].Value, TrackMatch.Groups["filename"].Value, TrackMatch.Groups["offset"].Value, line);

						filtersList = new FiltersList();
                        currentTrack = new GDITrack();
                        currentTrack.bps = ushort.Parse(TrackMatch.Groups["type"].Value);
                        currentTrack.flags = (byte)(byte.Parse(TrackMatch.Groups["flags"].Value) * 0x10);
                        currentTrack.offset = long.Parse(TrackMatch.Groups["offset"].Value);
                        currentTrack.sequence = uint.Parse(TrackMatch.Groups["track"].Value);
                        currentTrack.startSector = ulong.Parse(TrackMatch.Groups["start"].Value);
                        currentTrack.trackfilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), TrackMatch.Groups["filename"].Value.Replace("\\\"", "\"").Trim(new[] { '"' })));
                        currentTrack.trackfile = currentTrack.trackfilter.GetFilename();

                        if((currentTrack.startSector - currentStart) > 0)
                        {
                            if(currentTrack.startSector == 45000)
                            {
                                highDensity = true;
                                offsetmap.Add(0, currentStart);
                                densitySeparationSectors = (currentTrack.startSector - currentStart);
                                currentStart = currentTrack.startSector;
                            }
                            else
                            {
                                currentTrack.pregap = (currentTrack.startSector - currentStart);
                                currentTrack.startSector -= (currentTrack.startSector - currentStart);
                            }
                        }

                        if(((currentTrack.trackfilter.GetDataForkLength() - currentTrack.offset) % currentTrack.bps) != 0)
                            throw new ImageNotSupportedException("Track size not a multiple of sector size");

                        currentTrack.sectors = (ulong)((currentTrack.trackfilter.GetDataForkLength() - currentTrack.offset) / currentTrack.bps);
                        currentTrack.sectors += currentTrack.pregap;
                        currentStart += currentTrack.sectors;
                        currentTrack.highDensity = highDensity;

                        if((currentTrack.flags & 0x40) == 0x40)
                            currentTrack.tracktype = TrackType.CDMode1;
                        else
                            currentTrack.tracktype = TrackType.Audio;

                        discimage.tracks.Add(currentTrack);
                    }
                }

                Session[] _sessions = new Session[2];
                for(int s = 0; s < _sessions.Length; s++)
                {
                    if(s == 0)
                    {
                        _sessions[s].SessionSequence = 1;

                        foreach(GDITrack trk in discimage.tracks)
                        {
                            if(!trk.highDensity)
                            {
                                if(_sessions[s].StartTrack == 0)
                                    _sessions[s].StartTrack = trk.sequence;
                                else if(_sessions[s].StartTrack > trk.sequence)
                                    _sessions[s].StartTrack = trk.sequence;

                                if(_sessions[s].EndTrack < trk.sequence)
                                    _sessions[s].EndTrack = trk.sequence;

                                if(_sessions[s].StartSector > trk.startSector)
                                    _sessions[s].StartSector = trk.startSector;

                                if(_sessions[s].EndSector < (trk.sectors + trk.startSector - 1))
                                    _sessions[s].EndSector = trk.sectors + trk.startSector - 1;
                            }
                        }
                    }
                    else
                    {
                        _sessions[s].SessionSequence = 2;

                        foreach(GDITrack trk in discimage.tracks)
                        {
                            if(trk.highDensity)
                            {
                                if(_sessions[s].StartTrack == 0)
                                    _sessions[s].StartTrack = trk.sequence;
                                else if(_sessions[s].StartTrack > trk.sequence)
                                    _sessions[s].StartTrack = trk.sequence;

                                if(_sessions[s].EndTrack < trk.sequence)
                                    _sessions[s].EndTrack = trk.sequence;

                                if(_sessions[s].StartSector > trk.startSector)
                                    _sessions[s].StartSector = trk.startSector;

                                if(_sessions[s].EndSector < (trk.sectors + trk.startSector - 1))
                                    _sessions[s].EndSector = trk.sectors + trk.startSector - 1;
                            }
                        }
                    }
                }

                discimage.sessions.Add(_sessions[0]);
                discimage.sessions.Add(_sessions[1]);

                discimage.disktype = MediaType.GDROM;

                // DEBUG information
                DicConsole.DebugWriteLine("GDI plugin", "Disc image parsing results");

                DicConsole.DebugWriteLine("GDI plugin", "Session information:");
                DicConsole.DebugWriteLine("GDI plugin", "\tDisc contains {0} sessions", discimage.sessions.Count);
                for(int i = 0; i < discimage.sessions.Count; i++)
                {
                    DicConsole.DebugWriteLine("GDI plugin", "\tSession {0} information:", i + 1);
                    DicConsole.DebugWriteLine("GDI plugin", "\t\tStarting track: {0}", discimage.sessions[i].StartTrack);
                    DicConsole.DebugWriteLine("GDI plugin", "\t\tStarting sector: {0}", discimage.sessions[i].StartSector);
                    DicConsole.DebugWriteLine("GDI plugin", "\t\tEnding track: {0}", discimage.sessions[i].EndTrack);
                    DicConsole.DebugWriteLine("GDI plugin", "\t\tEnding sector: {0}", discimage.sessions[i].EndSector);
                }
                DicConsole.DebugWriteLine("GDI plugin", "Track information:");
                DicConsole.DebugWriteLine("GDI plugin", "\tDisc contains {0} tracks", discimage.tracks.Count);
                for(int i = 0; i < discimage.tracks.Count; i++)
                {
                    DicConsole.DebugWriteLine("GDI plugin", "\tTrack {0} information:", discimage.tracks[i].sequence);
                    DicConsole.DebugWriteLine("GDI plugin", "\t\t{0} bytes per sector", discimage.tracks[i].bps);
                    DicConsole.DebugWriteLine("GDI plugin", "\t\tPregap: {0} sectors", discimage.tracks[i].pregap);

                    if((discimage.tracks[i].flags & 0x80) == 0x80)
                        DicConsole.DebugWriteLine("GDI plugin", "\t\tTrack is flagged as quadraphonic");
                    if((discimage.tracks[i].flags & 0x40) == 0x40)
                        DicConsole.DebugWriteLine("GDI plugin", "\t\tTrack is data");
                    if((discimage.tracks[i].flags & 0x20) == 0x20)
                        DicConsole.DebugWriteLine("GDI plugin", "\t\tTrack allows digital copy");
                    if((discimage.tracks[i].flags & 0x10) == 0x10)
                        DicConsole.DebugWriteLine("GDI plugin", "\t\tTrack has pre-emphasis applied");

                    DicConsole.DebugWriteLine("GDI plugin", "\t\tTrack resides in file {0}, type defined as {1}, starting at byte {2}",
                        discimage.tracks[i].trackfilter, discimage.tracks[i].tracktype, discimage.tracks[i].offset);
                }

                DicConsole.DebugWriteLine("GDI plugin", "Building offset map");

                partitions = new List<Partition>();
                ulong byte_offset = 0;

                for(int i = 0; i < discimage.tracks.Count; i++)
                {
                    if(discimage.tracks[i].sequence == 1 && i != 0)
                        throw new ImageNotSupportedException("Unordered tracks");

                    Partition partition = new Partition();

                    // Index 01
                    partition.Description = string.Format("Track {0}.", discimage.tracks[i].sequence);
                    partition.Name = null;
                    partition.Start = discimage.tracks[i].startSector;
                    partition.Size = discimage.tracks[i].sectors * discimage.tracks[i].bps;
                    partition.Length = discimage.tracks[i].sectors;
                    partition.Sequence = discimage.tracks[i].sequence;
                    partition.Offset = byte_offset;
                    partition.Type = discimage.tracks[i].tracktype.ToString();

                    byte_offset += partition.Size;
                    offsetmap.Add(discimage.tracks[i].sequence, partition.Start);
                    partitions.Add(partition);
                }

                foreach(GDITrack track in discimage.tracks)
                    ImageInfo.imageSize += track.bps * track.sectors;
                foreach(GDITrack track in discimage.tracks)
                    ImageInfo.sectors += track.sectors;

                ImageInfo.sectors += densitySeparationSectors;

                ImageInfo.sectorSize = 2352; // All others

                foreach(GDITrack track in discimage.tracks)
                {
                    if((track.flags & 0x40) == 0x40 && track.bps == 2352)
                    {
                        ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
                        ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
                        ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubHeader);
                        ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC);
                        ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_P);
                        ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_Q);
                        ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorEDC);
                    }
                }

                ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
                ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();

                ImageInfo.mediaType = discimage.disktype;

                ImageInfo.readableSectorTags.Add(SectorTagType.CDTrackFlags);

                ImageInfo.xmlMediaType = XmlMediaType.OpticalDisc;

                DicConsole.VerboseWriteLine("GDI image describes a disc of type {0}", ImageInfo.mediaType);

                return true;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception trying to identify image file {0}", imageFilter.GetBasePath());
                DicConsole.ErrorWriteLine("Exception: {0}", ex.Message);
                DicConsole.ErrorWriteLine("Stack trace: {0}", ex.StackTrace);
                return false;
            }
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.imageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.imageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.sectorSize;
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            return ReadSectors(sectorAddress, 1, track);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, track, tag);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(GDITrack gdi_track in discimage.tracks)
                    {
                        if(gdi_track.sequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < gdi_track.sectors)
                                return ReadSectors((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            ulong transitionStart;
            offsetmap.TryGetValue(0, out transitionStart);
            if(sectorAddress >= transitionStart && sectorAddress < (densitySeparationSectors + transitionStart))
                return ReadSectors((sectorAddress - transitionStart), length, 0);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(GDITrack gdi_track in discimage.tracks)
                    {
                        if(gdi_track.sequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < gdi_track.sectors)
                                return ReadSectorsTag((sectorAddress - kvp.Value), length, kvp.Key, tag);
                        }
                    }
                }
            }

            ulong transitionStart;
            offsetmap.TryGetValue(0, out transitionStart);
            if(sectorAddress >= transitionStart && sectorAddress < (densitySeparationSectors + transitionStart))
                return ReadSectorsTag((sectorAddress - transitionStart), length, 0, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            if(track == 0)
            {
                if((sectorAddress + length) > densitySeparationSectors)
                    throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than present in track, won't cross tracks");

                return new byte[length * 2352];
            }

            GDITrack _track = new GDITrack();

            _track.sequence = 0;

            foreach(GDITrack gdi_track in discimage.tracks)
            {
                if(gdi_track.sequence == track)
                {
                    _track = gdi_track;
                    break;
                }
            }

            if(_track.sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if((sectorAddress + length) > _track.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than present in track, won't cross tracks");

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch(_track.tracktype)
            {
                case TrackType.Audio:
                    {
                        sector_offset = 0;
                        sector_size = 2352;
                        sector_skip = 0;
                        break;
                    }
                case TrackType.CDMode1:
                    {
                        if(_track.bps == 2352)
                        {
                            sector_offset = 16;
                            sector_size = 2048;
                            sector_skip = 288;
                        }
                        else
                        {
                            sector_offset = 0;
                            sector_size = 2048;
                            sector_skip = 0;
                        }
                        break;
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sector_size * length];

            ulong remainingSectors = length;

            if(_track.pregap > 0 && sectorAddress < _track.pregap)
            {
                ulong remainingPregap = _track.pregap - sectorAddress;
                byte[] zero;
                if(length > remainingPregap)
                {
                    zero = new byte[remainingPregap * sector_size];
                    remainingSectors -= remainingPregap;
                }
                else
                {
                    zero = new byte[length * sector_size];
                    remainingSectors -= length;
                }

                Array.Copy(zero, 0, buffer, 0, zero.Length);
            }

            if(remainingSectors == 0)
                return buffer;

            imageStream = _track.trackfilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream.Seek(_track.offset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip) + _track.pregap * _track.bps), SeekOrigin.Begin);
            if(sector_offset == 0 && sector_skip == 0)
                buffer = br.ReadBytes((int)(sector_size * remainingSectors));
            else
            {
                for(ulong i = 0; i < remainingSectors; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sector_size);
                    br.BaseStream.Seek(sector_skip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, (int)(i * sector_size), sector_size);
                }
            }

            return buffer;
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            if(track == 0)
            {
                if((sectorAddress + length) > densitySeparationSectors)
                    throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than present in track, won't cross tracks");

                if(tag == SectorTagType.CDTrackFlags)
                    return new byte[] { 0x00 };

                throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
            }

            GDITrack _track = new GDITrack();

            _track.sequence = 0;

            foreach(GDITrack gdi_track in discimage.tracks)
            {
                if(gdi_track.sequence == track)
                {
                    _track = gdi_track;
                    break;
                }
            }

            if(_track.sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > _track.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than present in track, won't cross tracks");

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch(tag)
            {
                case SectorTagType.CDSectorECC:
                case SectorTagType.CDSectorECC_P:
                case SectorTagType.CDSectorECC_Q:
                case SectorTagType.CDSectorEDC:
                case SectorTagType.CDSectorHeader:
                case SectorTagType.CDSectorSync:
                    break;
                case SectorTagType.CDTrackFlags:
                    {
                        byte[] flags = new byte[1];

                        flags[0] += _track.flags;

                        return flags;
                    }
                default:
                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch(_track.tracktype)
            {
                case TrackType.Audio:
                    throw new ArgumentException("There are no tags on audio tracks", nameof(tag));
                case TrackType.CDMode1:
                    {
                        if(_track.bps != 2352)
                            throw new FeatureNotPresentImageException("Image does not include tags for mode 1 sectors");

                        switch(tag)
                        {
                            case SectorTagType.CDSectorSync:
                                {
                                    sector_offset = 0;
                                    sector_size = 12;
                                    sector_skip = 2340;
                                    break;
                                }
                            case SectorTagType.CDSectorHeader:
                                {
                                    sector_offset = 12;
                                    sector_size = 4;
                                    sector_skip = 2336;
                                    break;
                                }
                            case SectorTagType.CDSectorSubchannel:
                            case SectorTagType.CDSectorSubHeader:
                                throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                            case SectorTagType.CDSectorECC:
                                {
                                    sector_offset = 2076;
                                    sector_size = 276;
                                    sector_skip = 0;
                                    break;
                                }
                            case SectorTagType.CDSectorECC_P:
                                {
                                    sector_offset = 2076;
                                    sector_size = 172;
                                    sector_skip = 104;
                                    break;
                                }
                            case SectorTagType.CDSectorECC_Q:
                                {
                                    sector_offset = 2248;
                                    sector_size = 104;
                                    sector_skip = 0;
                                    break;
                                }
                            case SectorTagType.CDSectorEDC:
                                {
                                    sector_offset = 2064;
                                    sector_size = 4;
                                    sector_skip = 284;
                                    break;
                                }
                            default:
                                throw new ArgumentException("Unsupported tag requested", nameof(tag));
                        }
                        break;
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sector_size * length];

            ulong remainingSectors = length;

            if(_track.pregap > 0 && sectorAddress < _track.pregap)
            {
                ulong remainingPregap = _track.pregap - sectorAddress;
                byte[] zero;
                if(length > remainingPregap)
                {
                    zero = new byte[remainingPregap * sector_size];
                    remainingSectors -= remainingPregap;
                }
                else
                {
                    zero = new byte[length * sector_size];
                    remainingSectors -= length;
                }

                Array.Copy(zero, 0, buffer, 0, zero.Length);
            }

            if(remainingSectors == 0)
                return buffer;

            imageStream = _track.trackfilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream.Seek(_track.offset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip) + _track.pregap * _track.bps), SeekOrigin.Begin);
            if(sector_offset == 0 && sector_skip == 0)
                buffer = br.ReadBytes((int)(sector_size * remainingSectors));
            else
            {
                for(ulong i = 0; i < remainingSectors; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sector_size);
                    br.BaseStream.Seek(sector_skip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, (int)(i * sector_size), sector_size);
                }
            }

            return buffer;
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            return ReadSectorsLong(sectorAddress, 1, track);
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(GDITrack gdi_track in discimage.tracks)
                    {
                        if(gdi_track.sequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < gdi_track.sectors)
                                return ReadSectorsLong((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            if(track == 0)
            {
                if((sectorAddress + length) > densitySeparationSectors)
                    throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than present in track, won't cross tracks");

                return new byte[length * 2352];
            }

            GDITrack _track = new GDITrack();

            _track.sequence = 0;

            foreach(GDITrack gdi_track in discimage.tracks)
            {
                if(gdi_track.sequence == track)
                {
                    _track = gdi_track;
                    break;
                }
            }

            if(_track.sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if((sectorAddress + length) > _track.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than present in track, won't cross tracks");

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch(_track.tracktype)
            {
                case TrackType.Audio:
                    {
                        sector_offset = 0;
                        sector_size = 2352;
                        sector_skip = 0;
                        break;
                    }
                case TrackType.CDMode1:
                    {
                        if(_track.bps == 2352)
                        {
                            sector_offset = 0;
                            sector_size = 2352;
                            sector_skip = 0;
                        }
                        else
                        {
                            sector_offset = 0;
                            sector_size = 2048;
                            sector_skip = 0;
                        }
                        break;
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sector_size * length];

            ulong remainingSectors = length;

            if(_track.pregap > 0 && sectorAddress < _track.pregap)
            {
                ulong remainingPregap = _track.pregap - sectorAddress;
                byte[] zero;
                if(length > remainingPregap)
                {
                    zero = new byte[remainingPregap * sector_size];
                    remainingSectors -= remainingPregap;
                }
                else
                {
                    zero = new byte[length * sector_size];
                    remainingSectors -= length;
                }

                Array.Copy(zero, 0, buffer, 0, zero.Length);
            }

            if(remainingSectors == 0)
                return buffer;

            imageStream = _track.trackfilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream.Seek(_track.offset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip) + _track.pregap * _track.bps), SeekOrigin.Begin);
            if(sector_offset == 0 && sector_skip == 0)
                buffer = br.ReadBytes((int)(sector_size * remainingSectors));
            else
            {
                for(ulong i = 0; i < remainingSectors; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sector_size);
                    br.BaseStream.Seek(sector_skip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, (int)(i * sector_size), sector_size);
                }
            }

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "Dreamcast GDI image";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.imageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.imageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.imageApplicationVersion;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.imageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.imageLastModificationTime;
        }

        public override string GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.mediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.mediaBarcode;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
        }

        public override List<Partition> GetPartitions()
        {
            return partitions;
        }

        public override List<Track> GetTracks()
        {
            List<Track> tracks = new List<Track>();

            foreach(GDITrack gdi_track in discimage.tracks)
            {
                Track _track = new Track();

                _track.Indexes = new Dictionary<int, ulong>();
                _track.TrackDescription = null;
                _track.TrackStartSector = gdi_track.startSector;
                _track.TrackEndSector = _track.TrackStartSector + gdi_track.sectors - 1;
                _track.TrackPregap = gdi_track.pregap;
                if(gdi_track.highDensity)
                    _track.TrackSession = 2;
                else
                    _track.TrackSession = 1;
                _track.TrackSequence = gdi_track.sequence;
                _track.TrackType = gdi_track.tracktype;
                _track.TrackFilter = gdi_track.trackfilter;
                _track.TrackFile = gdi_track.trackfile;
                _track.TrackFileOffset = (ulong)gdi_track.offset;
                _track.TrackFileType = "BINARY";
                _track.TrackRawBytesPerSector = gdi_track.bps;
                if(gdi_track.tracktype == TrackType.Data)
                    _track.TrackBytesPerSector = 2048;
                else
                    _track.TrackBytesPerSector = 2352;
                _track.TrackSubchannelType = TrackSubchannelType.None;

                tracks.Add(_track);
            }

            return tracks;
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            if(discimage.sessions.Contains(session))
            {
                return GetSessionTracks(session.SessionSequence);
            }
            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            List<Track> tracks = new List<Track>();
            bool expectedDensity;

            switch(session)
            {
                case 1:
                    expectedDensity = false;
                    break;
                case 2:
                    expectedDensity = true;
                    break;
                default:
                    throw new ImageNotSupportedException("Session does not exist in disc image");
            }

            foreach(GDITrack gdi_track in discimage.tracks)
            {
                if(gdi_track.highDensity == expectedDensity)
                {
                    Track _track = new Track();

                    _track.Indexes = new Dictionary<int, ulong>();
                    _track.TrackDescription = null;
                    _track.TrackStartSector = gdi_track.startSector;
                    _track.TrackEndSector = _track.TrackStartSector + gdi_track.sectors - 1;
                    _track.TrackPregap = gdi_track.pregap;
                    if(gdi_track.highDensity)
                        _track.TrackSession = 2;
                    else
                        _track.TrackSession = 1;
                    _track.TrackSequence = gdi_track.sequence;
                    _track.TrackType = gdi_track.tracktype;
                    _track.TrackFilter = gdi_track.trackfilter;
                    _track.TrackFile = gdi_track.trackfile;
                    _track.TrackFileOffset = (ulong)gdi_track.offset;
                    _track.TrackFileType = "BINARY";
                    _track.TrackRawBytesPerSector = gdi_track.bps;
                    if(gdi_track.tracktype == TrackType.Data)
                        _track.TrackBytesPerSector = 2048;
                    else
                        _track.TrackBytesPerSector = 2352;
                    _track.TrackSubchannelType = TrackSubchannelType.None;

                    tracks.Add(_track);
                }
            }

            return tracks;
        }

        public override List<Session> GetSessions()
        {
            return discimage.sessions;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            byte[] buffer = ReadSectorLong(sectorAddress);
            return Checksums.CDChecksums.CheckCDSector(buffer);
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            byte[] buffer = ReadSectorLong(sectorAddress, track);
            return Checksums.CDChecksums.CheckCDSector(buffer);
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = Checksums.CDChecksums.CheckCDSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        UnknownLBAs.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        FailingLBAs.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(UnknownLBAs.Count > 0)
                return null;
            if(FailingLBAs.Count > 0)
                return false;
            return true;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = Checksums.CDChecksums.CheckCDSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        UnknownLBAs.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        FailingLBAs.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(UnknownLBAs.Count > 0)
                return null;
            if(FailingLBAs.Count > 0)
                return false;
            return true;
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }

        #endregion Public methods

        #region Unsupported features

        public override int GetMediaSequence()
        {
            return ImageInfo.mediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.lastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.driveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.driveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.driveSerialNumber;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.mediaPartNumber;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.mediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.mediaModel;
        }

        public override string GetImageName()
        {
            return ImageInfo.imageName;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
        }

        #endregion Unsupported features
    }
}

