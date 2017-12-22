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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    // TODO: There seems no be no clear definition on how to treat pregaps that are not included in the file, so this is just appending it to start of track
    // TODO: This format doesn't support to specify pregaps that are included in the file (like Redump ones)
    public class Gdi : ImagePlugin
    {
        #region Internal structures
        struct GdiTrack
        {
            /// <summary>Track #</summary>
            public uint Sequence;
            /// <summary>Track filter</summary>
            public Filter Trackfilter;
            /// <summary>Track file</summary>
            public string Trackfile;
            /// <summary>Track byte offset in file</summary>
            public long Offset;
            /// <summary>Track flags</summary>
            public byte Flags;
            /// <summary>Track starting sector</summary>
            public ulong StartSector;
            /// <summary>Bytes per sector</summary>
            public ushort Bps;
            /// <summary>Sectors in track</summary>
            public ulong Sectors;
            /// <summary>Track type</summary>
            public TrackType Tracktype;
            /// <summary>Track session</summary>
            public bool HighDensity;
            /// <summary>Pregap sectors not stored in track file</summary>
            public ulong Pregap;
        }

        struct GdiDisc
        {
            /// <summary>Sessions</summary>
            public List<Session> Sessions;
            /// <summary>Tracks</summary>
            public List<GdiTrack> Tracks;
            /// <summary>Disk type</summary>
            public MediaType Disktype;
        }
        #endregion Internal structures

        #region Internal variables
        StreamReader gdiStream;
        Stream imageStream;
        /// <summary>Dictionary, index is track #, value is track number, or 0 if a TOC</summary>
        Dictionary<uint, ulong> offsetmap;
        GdiDisc discimage;
        List<Partition> partitions;
        ulong densitySeparationSectors;
        #endregion Internal variables

        #region Parsing regexs
        const string TRACK_REGEX =
                "\\s?(?<track>\\d+)\\s+(?<start>\\d+)\\s(?<flags>\\d)\\s(?<type>2352|2048)\\s(?<filename>.+)\\s(?<offset>\\d+)$"
            ;
        #endregion Parsing regexs

        #region Public methods
        public Gdi()
        {
            Name = "Dreamcast GDI image";
            PluginUuid = new Guid("281ECBF2-D2A7-414C-8497-1A33F6DCB2DD");
            ImageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                ImageHasPartitions = true,
                ImageHasSessions = true,
                ImageVersion = null,
                ImageApplicationVersion = null,
                ImageName = null,
                ImageCreator = null,
                MediaManufacturer = null,
                MediaModel = null,
                MediaPartNumber = null,
                MediaSequence = 0,
                LastMediaSequence = 0,
                DriveManufacturer = null,
                DriveModel = null,
                DriveSerialNumber = null,
                DriveFirmwareRevision = null
            };
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
                    if(i >= imageFilter.GetDataForkStream().Length) break;

                    if(testArray[i] == 0)
                    {
                        if(twoConsecutiveNulls) return false;

                        twoConsecutiveNulls = true;
                    }
                    else twoConsecutiveNulls = false;

                    if(testArray[i] < 0x20 && testArray[i] != 0x0A && testArray[i] != 0x0D && testArray[i] != 0x00)
                        return false;
                }

                gdiStream = new StreamReader(imageFilter.GetDataForkStream());
                int lineNumber = 0;
                int tracksFound = 0;
                int tracks = 0;

                while(gdiStream.Peek() >= 0)
                {
                    lineNumber++;
                    string line = gdiStream.ReadLine();

                    if(lineNumber == 1) { if(!int.TryParse(line, out tracks)) return false; }
                    else
                    {
                        Regex regexTrack = new Regex(TRACK_REGEX);

                        Match trackMatch = regexTrack.Match(line ?? throw new InvalidOperationException());

                        if(!trackMatch.Success) return false;

                        tracksFound++;
                    }
                }

                if(tracks == 0) return false;

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
            if(imageFilter == null) return false;

            try
            {
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                gdiStream = new StreamReader(imageFilter.GetDataForkStream());
                int lineNumber = 0;
                bool highDensity = false;

                // Initialize all RegExs
                Regex regexTrack = new Regex(TRACK_REGEX);

                // Initialize all RegEx matches

                // Initialize disc
                discimage = new GdiDisc {Sessions = new List<Session>(), Tracks = new List<GdiTrack>()};

                ulong currentStart = 0;
                offsetmap = new Dictionary<uint, ulong>();
                densitySeparationSectors = 0;

                while(gdiStream.Peek() >= 0)
                {
                    lineNumber++;
                    string line = gdiStream.ReadLine();

                    if(lineNumber == 1)
                    {
                        if(!int.TryParse(line, out _))
                            throw new ImageNotSupportedException("Not a correct Dreamcast GDI image");
                    }
                    else
                    {
                        Match trackMatch = regexTrack.Match(line ?? throw new InvalidOperationException());

                        if(!trackMatch.Success)
                            throw new ImageNotSupportedException($"Unknown line \"{line}\" at line {lineNumber}");

                        DicConsole.DebugWriteLine("GDI plugin",
                                                  "Found track {0} starts at {1} flags {2} type {3} file {4} offset {5} at line {6}",
                                                  trackMatch.Groups["track"].Value, trackMatch.Groups["start"].Value,
                                                  trackMatch.Groups["flags"].Value, trackMatch.Groups["type"].Value,
                                                  trackMatch.Groups["filename"].Value,
                                                  trackMatch.Groups["offset"].Value, lineNumber);

                        FiltersList filtersList = new FiltersList();
                        GdiTrack currentTrack = new GdiTrack
                        {
                            Bps = ushort.Parse(trackMatch.Groups["type"].Value),
                            Flags = (byte)(byte.Parse(trackMatch.Groups["flags"].Value) * 0x10),
                            Offset = long.Parse(trackMatch.Groups["offset"].Value),
                            Sequence = uint.Parse(trackMatch.Groups["track"].Value),
                            StartSector = ulong.Parse(trackMatch.Groups["start"].Value),
                            Trackfilter =
                                filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                   trackMatch.Groups["filename"].Value
                                                                             .Replace("\\\"", "\"").Trim('"')))
                        };
                        currentTrack.Trackfile = currentTrack.Trackfilter.GetFilename();

                        if(currentTrack.StartSector - currentStart > 0)
                            if(currentTrack.StartSector == 45000)
                            {
                                highDensity = true;
                                offsetmap.Add(0, currentStart);
                                densitySeparationSectors = currentTrack.StartSector - currentStart;
                                currentStart = currentTrack.StartSector;
                            }
                            else
                            {
                                currentTrack.Pregap = currentTrack.StartSector - currentStart;
                                currentTrack.StartSector -= currentTrack.StartSector - currentStart;
                            }

                        if((currentTrack.Trackfilter.GetDataForkLength() - currentTrack.Offset) % currentTrack.Bps !=
                           0) throw new ImageNotSupportedException("Track size not a multiple of sector size");

                        currentTrack.Sectors =
                            (ulong)((currentTrack.Trackfilter.GetDataForkLength() - currentTrack.Offset) /
                                    currentTrack.Bps);
                        currentTrack.Sectors += currentTrack.Pregap;
                        currentStart += currentTrack.Sectors;
                        currentTrack.HighDensity = highDensity;

                        currentTrack.Tracktype = (currentTrack.Flags & 0x40) == 0x40 ? TrackType.CdMode1 : TrackType.Audio;

                        discimage.Tracks.Add(currentTrack);
                    }
                }

                Session[] sessions = new Session[2];
                for(int s = 0; s < sessions.Length; s++)
                    if(s == 0)
                    {
                        sessions[s].SessionSequence = 1;

                        foreach(GdiTrack trk in discimage.Tracks.Where(trk => !trk.HighDensity)) {
                            if(sessions[s].StartTrack == 0) sessions[s].StartTrack = trk.Sequence;
                            else if(sessions[s].StartTrack > trk.Sequence) sessions[s].StartTrack = trk.Sequence;

                            if(sessions[s].EndTrack < trk.Sequence) sessions[s].EndTrack = trk.Sequence;

                            if(sessions[s].StartSector > trk.StartSector)
                                sessions[s].StartSector = trk.StartSector;

                            if(sessions[s].EndSector < trk.Sectors + trk.StartSector - 1)
                                sessions[s].EndSector = trk.Sectors + trk.StartSector - 1;
                        }
                    }
                    else
                    {
                        sessions[s].SessionSequence = 2;

                        foreach(GdiTrack trk in discimage.Tracks.Where(trk => trk.HighDensity)) {
                            if(sessions[s].StartTrack == 0) sessions[s].StartTrack = trk.Sequence;
                            else if(sessions[s].StartTrack > trk.Sequence) sessions[s].StartTrack = trk.Sequence;

                            if(sessions[s].EndTrack < trk.Sequence) sessions[s].EndTrack = trk.Sequence;

                            if(sessions[s].StartSector > trk.StartSector)
                                sessions[s].StartSector = trk.StartSector;

                            if(sessions[s].EndSector < trk.Sectors + trk.StartSector - 1)
                                sessions[s].EndSector = trk.Sectors + trk.StartSector - 1;
                        }
                    }

                discimage.Sessions.Add(sessions[0]);
                discimage.Sessions.Add(sessions[1]);

                discimage.Disktype = MediaType.GDROM;

                // DEBUG information
                DicConsole.DebugWriteLine("GDI plugin", "Disc image parsing results");

                DicConsole.DebugWriteLine("GDI plugin", "Session information:");
                DicConsole.DebugWriteLine("GDI plugin", "\tDisc contains {0} sessions", discimage.Sessions.Count);
                for(int i = 0; i < discimage.Sessions.Count; i++)
                {
                    DicConsole.DebugWriteLine("GDI plugin", "\tSession {0} information:", i + 1);
                    DicConsole.DebugWriteLine("GDI plugin", "\t\tStarting track: {0}",
                                              discimage.Sessions[i].StartTrack);
                    DicConsole.DebugWriteLine("GDI plugin", "\t\tStarting sector: {0}",
                                              discimage.Sessions[i].StartSector);
                    DicConsole.DebugWriteLine("GDI plugin", "\t\tEnding track: {0}", discimage.Sessions[i].EndTrack);
                    DicConsole.DebugWriteLine("GDI plugin", "\t\tEnding sector: {0}", discimage.Sessions[i].EndSector);
                }

                DicConsole.DebugWriteLine("GDI plugin", "Track information:");
                DicConsole.DebugWriteLine("GDI plugin", "\tDisc contains {0} tracks", discimage.Tracks.Count);
                for(int i = 0; i < discimage.Tracks.Count; i++)
                {
                    DicConsole.DebugWriteLine("GDI plugin", "\tTrack {0} information:", discimage.Tracks[i].Sequence);
                    DicConsole.DebugWriteLine("GDI plugin", "\t\t{0} bytes per sector", discimage.Tracks[i].Bps);
                    DicConsole.DebugWriteLine("GDI plugin", "\t\tPregap: {0} sectors", discimage.Tracks[i].Pregap);

                    if((discimage.Tracks[i].Flags & 0x80) == 0x80)
                        DicConsole.DebugWriteLine("GDI plugin", "\t\tTrack is flagged as quadraphonic");
                    if((discimage.Tracks[i].Flags & 0x40) == 0x40)
                        DicConsole.DebugWriteLine("GDI plugin", "\t\tTrack is data");
                    if((discimage.Tracks[i].Flags & 0x20) == 0x20)
                        DicConsole.DebugWriteLine("GDI plugin", "\t\tTrack allows digital copy");
                    if((discimage.Tracks[i].Flags & 0x10) == 0x10)
                        DicConsole.DebugWriteLine("GDI plugin", "\t\tTrack has pre-emphasis applied");

                    DicConsole.DebugWriteLine("GDI plugin",
                                              "\t\tTrack resides in file {0}, type defined as {1}, starting at byte {2}",
                                              discimage.Tracks[i].Trackfilter, discimage.Tracks[i].Tracktype,
                                              discimage.Tracks[i].Offset);
                }

                DicConsole.DebugWriteLine("GDI plugin", "Building offset map");

                partitions = new List<Partition>();
                ulong byteOffset = 0;

                for(int i = 0; i < discimage.Tracks.Count; i++)
                {
                    if(discimage.Tracks[i].Sequence == 1 && i != 0)
                        throw new ImageNotSupportedException("Unordered tracks");

                    // Index 01
                    Partition partition = new Partition
                    {
                        Description = $"Track {discimage.Tracks[i].Sequence}.",
                        Name = null,
                        Start = discimage.Tracks[i].StartSector,
                        Size = discimage.Tracks[i].Sectors * discimage.Tracks[i].Bps,
                        Length = discimage.Tracks[i].Sectors,
                        Sequence = discimage.Tracks[i].Sequence,
                        Offset = byteOffset,
                        Type = discimage.Tracks[i].Tracktype.ToString()
                    };

                    byteOffset += partition.Size;
                    offsetmap.Add(discimage.Tracks[i].Sequence, partition.Start);
                    partitions.Add(partition);
                }

                foreach(GdiTrack track in discimage.Tracks) ImageInfo.ImageSize += track.Bps * track.Sectors;
                foreach(GdiTrack track in discimage.Tracks) ImageInfo.Sectors += track.Sectors;

                ImageInfo.Sectors += densitySeparationSectors;

                ImageInfo.SectorSize = 2352; // All others

                foreach(GdiTrack track in discimage.Tracks.Where(track => (track.Flags & 0x40) == 0x40 && track.Bps == 2352)) {
                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);
                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);
                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);
                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                }

                ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
                ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();

                ImageInfo.MediaType = discimage.Disktype;

                ImageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

                ImageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                DicConsole.VerboseWriteLine("GDI image describes a disc of type {0}", ImageInfo.MediaType);

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
            return ImageInfo.ImageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.ImageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.Sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.SectorSize;
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
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap where sectorAddress >= kvp.Value from gdiTrack in discimage.Tracks where gdiTrack.Sequence == kvp.Key where sectorAddress - kvp.Value < gdiTrack.Sectors select kvp) return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            offsetmap.TryGetValue(0, out ulong transitionStart);
            if(sectorAddress >= transitionStart && sectorAddress < densitySeparationSectors + transitionStart)
                return ReadSectors(sectorAddress - transitionStart, length, 0);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap where sectorAddress >= kvp.Value from gdiTrack in discimage.Tracks where gdiTrack.Sequence == kvp.Key where sectorAddress - kvp.Value < gdiTrack.Sectors select kvp) return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            offsetmap.TryGetValue(0, out ulong transitionStart);
            if(sectorAddress >= transitionStart && sectorAddress < densitySeparationSectors + transitionStart)
                return ReadSectorsTag(sectorAddress - transitionStart, length, 0, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            if(track == 0)
            {
                if(sectorAddress + length > densitySeparationSectors)
                    throw new ArgumentOutOfRangeException(nameof(length),
                                                          "Requested more sectors than present in track, won't cross tracks");

                return new byte[length * 2352];
            }

            GdiTrack dicTrack = new GdiTrack {Sequence = 0};

            foreach(GdiTrack gdiTrack in discimage.Tracks.Where(gdiTrack => gdiTrack.Sequence == track)) {
                dicTrack = gdiTrack;
                break;
            }

            if(dicTrack.Sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(sectorAddress + length > dicTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      "Requested more sectors than present in track, won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(dicTrack.Tracktype)
            {
                case TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize = 2352;
                    sectorSkip = 0;
                    break;
                }
                case TrackType.CdMode1:
                {
                    if(dicTrack.Bps == 2352)
                    {
                        sectorOffset = 16;
                        sectorSize = 2048;
                        sectorSkip = 288;
                    }
                    else
                    {
                        sectorOffset = 0;
                        sectorSize = 2048;
                        sectorSkip = 0;
                    }
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            ulong remainingSectors = length;

            if(dicTrack.Pregap > 0 && sectorAddress < dicTrack.Pregap)
            {
                ulong remainingPregap = dicTrack.Pregap - sectorAddress;
                byte[] zero;
                if(length > remainingPregap)
                {
                    zero = new byte[remainingPregap * sectorSize];
                    remainingSectors -= remainingPregap;
                }
                else
                {
                    zero = new byte[length * sectorSize];
                    remainingSectors -= length;
                }

                Array.Copy(zero, 0, buffer, 0, zero.Length);
            }

            if(remainingSectors == 0) return buffer;

            imageStream = dicTrack.Trackfilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek(dicTrack.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip) + dicTrack.Pregap * dicTrack.Bps),
                    SeekOrigin.Begin);
            if(sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int)(sectorSize * remainingSectors));
            else
                for(ulong i = 0; i < remainingSectors; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, (int)(i * sectorSize), sectorSize);
                }

            return buffer;
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            if(track == 0)
            {
                if(sectorAddress + length > densitySeparationSectors)
                    throw new ArgumentOutOfRangeException(nameof(length),
                                                          "Requested more sectors than present in track, won't cross tracks");

                if(tag == SectorTagType.CdTrackFlags) return new byte[] {0x00};

                throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
            }

            GdiTrack dicTrack = new GdiTrack {Sequence = 0};

            foreach(GdiTrack gdiTrack in discimage.Tracks.Where(gdiTrack => gdiTrack.Sequence == track)) {
                dicTrack = gdiTrack;
                break;
            }

            if(dicTrack.Sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > dicTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      "Requested more sectors than present in track, won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(tag)
            {
                case SectorTagType.CdSectorEcc:
                case SectorTagType.CdSectorEccP:
                case SectorTagType.CdSectorEccQ:
                case SectorTagType.CdSectorEdc:
                case SectorTagType.CdSectorHeader:
                case SectorTagType.CdSectorSync: break;
                case SectorTagType.CdTrackFlags:
                {
                    byte[] flags = new byte[1];

                    flags[0] += dicTrack.Flags;

                    return flags;
                }
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch(dicTrack.Tracktype)
            {
                case TrackType.Audio: throw new ArgumentException("There are no tags on audio tracks", nameof(tag));
                case TrackType.CdMode1:
                {
                    if(dicTrack.Bps != 2352)
                        throw new FeatureNotPresentImageException("Image does not include tags for mode 1 sectors");

                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        {
                            sectorOffset = 0;
                            sectorSize = 12;
                            sectorSkip = 2340;
                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize = 4;
                            sectorSkip = 2336;
                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                        case SectorTagType.CdSectorSubHeader:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CdSectorEcc:
                        {
                            sectorOffset = 2076;
                            sectorSize = 276;
                            sectorSkip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEccP:
                        {
                            sectorOffset = 2076;
                            sectorSize = 172;
                            sectorSkip = 104;
                            break;
                        }
                        case SectorTagType.CdSectorEccQ:
                        {
                            sectorOffset = 2248;
                            sectorSize = 104;
                            sectorSkip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2064;
                            sectorSize = 4;
                            sectorSkip = 284;
                            break;
                        }
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            ulong remainingSectors = length;

            if(dicTrack.Pregap > 0 && sectorAddress < dicTrack.Pregap)
            {
                ulong remainingPregap = dicTrack.Pregap - sectorAddress;
                byte[] zero;
                if(length > remainingPregap)
                {
                    zero = new byte[remainingPregap * sectorSize];
                    remainingSectors -= remainingPregap;
                }
                else
                {
                    zero = new byte[length * sectorSize];
                    remainingSectors -= length;
                }

                Array.Copy(zero, 0, buffer, 0, zero.Length);
            }

            if(remainingSectors == 0) return buffer;

            imageStream = dicTrack.Trackfilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek(dicTrack.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip) + dicTrack.Pregap * dicTrack.Bps),
                    SeekOrigin.Begin);
            if(sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int)(sectorSize * remainingSectors));
            else
                for(ulong i = 0; i < remainingSectors; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, (int)(i * sectorSize), sectorSize);
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
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap where sectorAddress >= kvp.Value from gdiTrack in discimage.Tracks where gdiTrack.Sequence == kvp.Key where sectorAddress - kvp.Value < gdiTrack.Sectors select kvp) return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            if(track == 0)
            {
                if(sectorAddress + length > densitySeparationSectors)
                    throw new ArgumentOutOfRangeException(nameof(length),
                                                          "Requested more sectors than present in track, won't cross tracks");

                return new byte[length * 2352];
            }

            GdiTrack dicTrack = new GdiTrack {Sequence = 0};

            foreach(GdiTrack gdiTrack in discimage.Tracks.Where(gdiTrack => gdiTrack.Sequence == track)) {
                dicTrack = gdiTrack;
                break;
            }

            if(dicTrack.Sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(sectorAddress + length > dicTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      "Requested more sectors than present in track, won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(dicTrack.Tracktype)
            {
                case TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize = 2352;
                    sectorSkip = 0;
                    break;
                }
                case TrackType.CdMode1:
                {
                    if(dicTrack.Bps == 2352)
                    {
                        sectorOffset = 0;
                        sectorSize = 2352;
                        sectorSkip = 0;
                    }
                    else
                    {
                        sectorOffset = 0;
                        sectorSize = 2048;
                        sectorSkip = 0;
                    }
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            ulong remainingSectors = length;

            if(dicTrack.Pregap > 0 && sectorAddress < dicTrack.Pregap)
            {
                ulong remainingPregap = dicTrack.Pregap - sectorAddress;
                byte[] zero;
                if(length > remainingPregap)
                {
                    zero = new byte[remainingPregap * sectorSize];
                    remainingSectors -= remainingPregap;
                }
                else
                {
                    zero = new byte[length * sectorSize];
                    remainingSectors -= length;
                }

                Array.Copy(zero, 0, buffer, 0, zero.Length);
            }

            if(remainingSectors == 0) return buffer;

            imageStream = dicTrack.Trackfilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek(dicTrack.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip) + dicTrack.Pregap * dicTrack.Bps),
                    SeekOrigin.Begin);
            if(sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int)(sectorSize * remainingSectors));
            else
                for(ulong i = 0; i < remainingSectors; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, (int)(i * sectorSize), sectorSize);
                }

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "Dreamcast GDI image";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.ImageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.ImageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.ImageApplicationVersion;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.ImageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.ImageLastModificationTime;
        }

        public override string GetImageComments()
        {
            return ImageInfo.ImageComments;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.MediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.MediaBarcode;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.MediaType;
        }

        public override List<Partition> GetPartitions()
        {
            return partitions;
        }

        public override List<Track> GetTracks()
        {
            List<Track> tracks = new List<Track>();

            foreach(GdiTrack gdiTrack in discimage.Tracks)
            {
                Track track = new Track
                {
                    Indexes = new Dictionary<int, ulong>(),
                    TrackDescription = null,
                    TrackStartSector = gdiTrack.StartSector,
                    TrackPregap = gdiTrack.Pregap,
                    TrackSession = (ushort)(gdiTrack.HighDensity ? 2 : 1),
                    TrackSequence = gdiTrack.Sequence,
                    TrackType = gdiTrack.Tracktype,
                    TrackFilter = gdiTrack.Trackfilter,
                    TrackFile = gdiTrack.Trackfile,
                    TrackFileOffset = (ulong)gdiTrack.Offset,
                    TrackFileType = "BINARY",
                    TrackRawBytesPerSector = gdiTrack.Bps,
                    TrackBytesPerSector = gdiTrack.Tracktype == TrackType.Data ? 2048 : 2352,
                    TrackSubchannelType = TrackSubchannelType.None
                };

                track.TrackEndSector = track.TrackStartSector + gdiTrack.Sectors - 1;

                tracks.Add(track);
            }

            return tracks;
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            if(discimage.Sessions.Contains(session)) return GetSessionTracks(session.SessionSequence);

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
                default: throw new ImageNotSupportedException("Session does not exist in disc image");
            }

            foreach(GdiTrack gdiTrack in discimage.Tracks)
                if(gdiTrack.HighDensity == expectedDensity)
                {
                    Track track = new Track
                    {
                        Indexes = new Dictionary<int, ulong>(),
                        TrackDescription = null,
                        TrackStartSector = gdiTrack.StartSector,
                        TrackPregap = gdiTrack.Pregap,
                        TrackSession = (ushort)(gdiTrack.HighDensity ? 2 : 1),
                        TrackSequence = gdiTrack.Sequence,
                        TrackType = gdiTrack.Tracktype,
                        TrackFilter = gdiTrack.Trackfilter,
                        TrackFile = gdiTrack.Trackfile,
                        TrackFileOffset = (ulong)gdiTrack.Offset,
                        TrackFileType = "BINARY",
                        TrackRawBytesPerSector = gdiTrack.Bps,
                        TrackBytesPerSector = gdiTrack.Tracktype == TrackType.Data ? 2048 : 2352,
                        TrackSubchannelType = TrackSubchannelType.None
                    };

                    track.TrackEndSector = track.TrackStartSector + gdiTrack.Sectors - 1;

                    tracks.Add(track);
                }

            return tracks;
        }

        public override List<Session> GetSessions()
        {
            return discimage.Sessions;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            byte[] buffer = ReadSectorLong(sectorAddress);
            return CdChecksums.CheckCdSector(buffer);
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            byte[] buffer = ReadSectorLong(sectorAddress, track);
            return CdChecksums.CheckCdSector(buffer);
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = CdChecksums.CheckCdSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        unknownLbas.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        failingLbas.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(unknownLbas.Count > 0) return null;
            if(failingLbas.Count > 0) return false;

            return true;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = CdChecksums.CheckCdSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        unknownLbas.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        failingLbas.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(unknownLbas.Count > 0) return null;
            if(failingLbas.Count > 0) return false;

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
            return ImageInfo.MediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.LastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.DriveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.DriveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.DriveSerialNumber;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.MediaPartNumber;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.MediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.MediaModel;
        }

        public override string GetImageName()
        {
            return ImageInfo.ImageName;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.ImageCreator;
        }
        #endregion Unsupported features
    }
}