// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads Dreamcast GDI disc images.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Decoders.CD;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.DiscImages
{
    public sealed partial class Gdi
    {
        /// <inheritdoc />
        public ErrorNumber Open(IFilter imageFilter)
        {
            if(imageFilter == null)
                return ErrorNumber.NoSuchFile;

            try
            {
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                _gdiStream = new StreamReader(imageFilter.GetDataForkStream());
                int  lineNumber  = 0;
                bool highDensity = false;

                // Initialize all RegExs
                var regexTrack = new Regex(REGEX_TRACK);

                // Initialize all RegEx matches

                // Initialize disc
                _discImage = new GdiDisc
                {
                    Sessions = new List<Session>(),
                    Tracks   = new List<GdiTrack>()
                };

                ulong currentStart = 0;
                _offsetMap                = new Dictionary<uint, ulong>();
                _densitySeparationSectors = 0;

                while(_gdiStream.Peek() >= 0)
                {
                    lineNumber++;
                    string line = _gdiStream.ReadLine();

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

                        AaruConsole.DebugWriteLine("GDI plugin",
                                                   "Found track {0} starts at {1} flags {2} type {3} file {4} offset {5} at line {6}",
                                                   trackMatch.Groups["track"].Value, trackMatch.Groups["start"].Value,
                                                   trackMatch.Groups["flags"].Value, trackMatch.Groups["type"].Value,
                                                   trackMatch.Groups["filename"].Value,
                                                   trackMatch.Groups["offset"].Value, lineNumber);

                        var filtersList = new FiltersList();

                        var currentTrack = new GdiTrack
                        {
                            Bps         = ushort.Parse(trackMatch.Groups["type"].Value),
                            Flags       = byte.Parse(trackMatch.Groups["flags"].Value),
                            Offset      = long.Parse(trackMatch.Groups["offset"].Value),
                            Sequence    = uint.Parse(trackMatch.Groups["track"].Value),
                            StartSector = ulong.Parse(trackMatch.Groups["start"].Value),
                            TrackFilter = filtersList.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                             trackMatch.Groups["filename"].Value.
                                                                                 Replace("\\\"", "\"").Trim('"')))
                        };

                        currentTrack.TrackFile = currentTrack.TrackFilter.Filename;

                        if(currentTrack.StartSector - currentStart > 0)
                            if(currentTrack.StartSector == 45000)
                            {
                                highDensity = true;
                                _offsetMap.Add(0, currentStart);
                                _densitySeparationSectors = currentTrack.StartSector - currentStart;
                                currentStart              = currentTrack.StartSector;
                            }
                            else
                            {
                                currentTrack.Pregap      =  currentTrack.StartSector - currentStart;
                                currentTrack.StartSector -= currentTrack.StartSector - currentStart;
                            }

                        if((currentTrack.TrackFilter.DataForkLength - currentTrack.Offset) % currentTrack.Bps != 0)
                            throw new ImageNotSupportedException("Track size not a multiple of sector size");

                        currentTrack.Sectors = (ulong)((currentTrack.TrackFilter.DataForkLength - currentTrack.Offset) /
                                                       currentTrack.Bps);

                        currentTrack.Sectors     += currentTrack.Pregap;
                        currentStart             += currentTrack.Sectors;
                        currentTrack.HighDensity =  highDensity;

                        currentTrack.TrackType =
                            (currentTrack.Flags & 0x4) == 0x4 ? TrackType.CdMode1 : TrackType.Audio;

                        _discImage.Tracks.Add(currentTrack);
                    }
                }

                Session[] sessions = new Session[2];

                for(int s = 0; s < sessions.Length; s++)
                    if(s == 0)
                    {
                        sessions[s].Sequence = 1;

                        foreach(GdiTrack trk in _discImage.Tracks.Where(trk => !trk.HighDensity))
                        {
                            if(sessions[s].StartTrack == 0)
                                sessions[s].StartTrack = trk.Sequence;
                            else if(sessions[s].StartTrack > trk.Sequence)
                                sessions[s].StartTrack = trk.Sequence;

                            if(sessions[s].EndTrack < trk.Sequence)
                                sessions[s].EndTrack = trk.Sequence;

                            if(sessions[s].StartSector > trk.StartSector)
                                sessions[s].StartSector = trk.StartSector;

                            if(sessions[s].EndSector < trk.Sectors                    + trk.StartSector - 1)
                                sessions[s].EndSector = trk.Sectors + trk.StartSector - 1;
                        }
                    }
                    else
                    {
                        sessions[s].Sequence = 2;

                        foreach(GdiTrack trk in _discImage.Tracks.Where(trk => trk.HighDensity))
                        {
                            if(sessions[s].StartTrack == 0)
                                sessions[s].StartTrack = trk.Sequence;
                            else if(sessions[s].StartTrack > trk.Sequence)
                                sessions[s].StartTrack = trk.Sequence;

                            if(sessions[s].EndTrack < trk.Sequence)
                                sessions[s].EndTrack = trk.Sequence;

                            if(sessions[s].StartSector > trk.StartSector)
                                sessions[s].StartSector = trk.StartSector;

                            if(sessions[s].EndSector < trk.Sectors                    + trk.StartSector - 1)
                                sessions[s].EndSector = trk.Sectors + trk.StartSector - 1;
                        }
                    }

                _discImage.Sessions.Add(sessions[0]);
                _discImage.Sessions.Add(sessions[1]);

                _discImage.Disktype = MediaType.GDROM;

                // DEBUG information
                AaruConsole.DebugWriteLine("GDI plugin", "Disc image parsing results");

                AaruConsole.DebugWriteLine("GDI plugin", "Session information:");
                AaruConsole.DebugWriteLine("GDI plugin", "\tDisc contains {0} sessions", _discImage.Sessions.Count);

                for(int i = 0; i < _discImage.Sessions.Count; i++)
                {
                    AaruConsole.DebugWriteLine("GDI plugin", "\tSession {0} information:", i + 1);

                    AaruConsole.DebugWriteLine("GDI plugin", "\t\tStarting track: {0}",
                                               _discImage.Sessions[i].StartTrack);

                    AaruConsole.DebugWriteLine("GDI plugin", "\t\tStarting sector: {0}",
                                               _discImage.Sessions[i].StartSector);

                    AaruConsole.DebugWriteLine("GDI plugin", "\t\tEnding track: {0}", _discImage.Sessions[i].EndTrack);

                    AaruConsole.DebugWriteLine("GDI plugin", "\t\tEnding sector: {0}",
                                               _discImage.Sessions[i].EndSector);
                }

                AaruConsole.DebugWriteLine("GDI plugin", "Track information:");
                AaruConsole.DebugWriteLine("GDI plugin", "\tDisc contains {0} tracks", _discImage.Tracks.Count);

                for(int i = 0; i < _discImage.Tracks.Count; i++)
                {
                    AaruConsole.DebugWriteLine("GDI plugin", "\tTrack {0} information:", _discImage.Tracks[i].Sequence);
                    AaruConsole.DebugWriteLine("GDI plugin", "\t\t{0} bytes per sector", _discImage.Tracks[i].Bps);
                    AaruConsole.DebugWriteLine("GDI plugin", "\t\tPregap: {0} sectors", _discImage.Tracks[i].Pregap);

                    if((_discImage.Tracks[i].Flags & 0x8) == 0x8)
                        AaruConsole.DebugWriteLine("GDI plugin", "\t\tTrack is flagged as quadraphonic");

                    if((_discImage.Tracks[i].Flags & 0x4) == 0x4)
                        AaruConsole.DebugWriteLine("GDI plugin", "\t\tTrack is data");

                    if((_discImage.Tracks[i].Flags & 0x2) == 0x2)
                        AaruConsole.DebugWriteLine("GDI plugin", "\t\tTrack allows digital copy");

                    if((_discImage.Tracks[i].Flags & 0x1) == 0x1)
                        AaruConsole.DebugWriteLine("GDI plugin", "\t\tTrack has pre-emphasis applied");

                    AaruConsole.DebugWriteLine("GDI plugin",
                                               "\t\tTrack resides in file {0}, type defined as {1}, starting at byte {2}",
                                               _discImage.Tracks[i].TrackFilter, _discImage.Tracks[i].TrackType,
                                               _discImage.Tracks[i].Offset);
                }

                AaruConsole.DebugWriteLine("GDI plugin", "Building offset map");

                Partitions = new List<Partition>();
                ulong byteOffset = 0;

                for(int i = 0; i < _discImage.Tracks.Count; i++)
                {
                    if(_discImage.Tracks[i].Sequence == 1 &&
                       i                             != 0)
                        throw new ImageNotSupportedException("Unordered tracks");

                    // Index 01
                    var partition = new Partition
                    {
                        Description = $"Track {_discImage.Tracks[i].Sequence}.",
                        Name        = null,
                        Start       = _discImage.Tracks[i].StartSector,
                        Size        = _discImage.Tracks[i].Sectors * _discImage.Tracks[i].Bps,
                        Length      = _discImage.Tracks[i].Sectors,
                        Sequence    = _discImage.Tracks[i].Sequence,
                        Offset      = byteOffset,
                        Type        = _discImage.Tracks[i].TrackType.ToString()
                    };

                    byteOffset += partition.Size;
                    _offsetMap.Add(_discImage.Tracks[i].Sequence, partition.Start);
                    Partitions.Add(partition);
                }

                foreach(GdiTrack track in _discImage.Tracks)
                    _imageInfo.ImageSize += track.Bps * track.Sectors;

                foreach(GdiTrack track in _discImage.Tracks)
                    _imageInfo.Sectors += track.Sectors;

                _imageInfo.Sectors += _densitySeparationSectors;

                _imageInfo.SectorSize = 2352; // All others

                foreach(GdiTrack unused in _discImage.Tracks.Where(track => (track.Flags & 0x4) == 0x4 &&
                                                                            track.Bps           == 2352))
                {
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                }

                _imageInfo.CreationTime         = imageFilter.CreationTime;
                _imageInfo.LastModificationTime = imageFilter.LastWriteTime;

                _imageInfo.MediaType = _discImage.Disktype;

                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

                _imageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                AaruConsole.VerboseWriteLine("GDI image describes a disc of type {0}", _imageInfo.MediaType);

                _sectorBuilder = new SectorBuilder();

                return ErrorNumber.NoError;
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine("Exception trying to identify image file {0}", imageFilter.BasePath);
                AaruConsole.ErrorWriteLine("Exception: {0}", ex);

                return ErrorNumber.UnexpectedException;
            }
        }

        /// <inheritdoc />
        public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) =>
            ReadSectors(sectorAddress, 1, out buffer);

        /// <inheritdoc />
        public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer) =>
            ReadSectorsTag(sectorAddress, 1, tag, out buffer);

        /// <inheritdoc />
        public ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer) =>
            ReadSectors(sectorAddress, 1, track, out buffer);

        /// <inheritdoc />
        public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer) =>
            ReadSectorsTag(sectorAddress, 1, track, tag, out buffer);

        /// <inheritdoc />
        public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
        {
            buffer = null;

            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress >= kvp.Value
                                                     from gdiTrack in _discImage.Tracks
                                                     where gdiTrack.Sequence         == kvp.Key
                                                     where sectorAddress - kvp.Value < gdiTrack.Sectors select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key, out buffer);

            _offsetMap.TryGetValue(0, out ulong transitionStart);

            if(sectorAddress < transitionStart ||
               sectorAddress >= _densitySeparationSectors + transitionStart)
                return ErrorNumber.SectorNotFound;

            return ReadSectors(sectorAddress - transitionStart, length, 0, out buffer);
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer)
        {
            buffer = null;

            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress >= kvp.Value
                                                     from gdiTrack in _discImage.Tracks
                                                     where gdiTrack.Sequence         == kvp.Key
                                                     where sectorAddress - kvp.Value < gdiTrack.Sectors select kvp)
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag, out buffer);

            _offsetMap.TryGetValue(0, out ulong transitionStart);

            if(sectorAddress < transitionStart ||
               sectorAddress >= _densitySeparationSectors + transitionStart)
                return ErrorNumber.SectorNotFound;

            return ReadSectorsTag(sectorAddress - transitionStart, length, 0, tag, out buffer);
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectors(ulong sectorAddress, uint length, uint track, out byte[] buffer)
        {
            buffer = null;

            if(track == 0)
            {
                if(sectorAddress + length > _densitySeparationSectors)
                    return ErrorNumber.OutOfRange;

                buffer = new byte[length * 2352];

                return ErrorNumber.NoError;
            }

            var aaruTrack = new GdiTrack
            {
                Sequence = 0
            };

            foreach(GdiTrack gdiTrack in _discImage.Tracks.Where(gdiTrack => gdiTrack.Sequence == track))
            {
                aaruTrack = gdiTrack;

                break;
            }

            if(aaruTrack.Sequence == 0)
                return ErrorNumber.SectorNotFound;

            if(sectorAddress + length > aaruTrack.Sectors)
                return ErrorNumber.OutOfRange;

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(aaruTrack.TrackType)
            {
                case TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                case TrackType.CdMode1:
                {
                    if(aaruTrack.Bps == 2352)
                    {
                        sectorOffset = 16;
                        sectorSize   = 2048;
                        sectorSkip   = 288;
                    }
                    else
                    {
                        sectorOffset = 0;
                        sectorSize   = 2048;
                        sectorSkip   = 0;
                    }

                    break;
                }
                default: return ErrorNumber.NotSupported;
            }

            buffer = new byte[sectorSize * length];

            ulong remainingSectors = length;

            if(aaruTrack.Pregap > 0 &&
               sectorAddress    < aaruTrack.Pregap)
            {
                ulong remainingPregap = aaruTrack.Pregap - sectorAddress;

                remainingSectors -= length > remainingPregap ? remainingPregap : length;
            }

            if(remainingSectors == 0)
                return ErrorNumber.NoError;

            _imageStream = aaruTrack.TrackFilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

            long pos = aaruTrack.Offset + (long)((sectorAddress    * (sectorOffset + sectorSize + sectorSkip)) -
                                                 (aaruTrack.Pregap * aaruTrack.Bps));

            if(pos < 0)
                pos = 0;

            br.BaseStream.Seek(pos, SeekOrigin.Begin);

            if(sectorOffset     == 0 &&
               sectorSkip       == 0 &&
               remainingSectors == length)
                buffer = br.ReadBytes((int)(sectorSize * remainingSectors));
            else if(sectorOffset == 0 &&
                    sectorSkip   == 0)
            {
                byte[] tmp = br.ReadBytes((int)(sectorSize * remainingSectors));
                Array.Copy(tmp, 0, buffer, (int)((length - remainingSectors) * sectorSize), tmp.Length);
            }
            else
            {
                int bufferPos = (int)((length - remainingSectors) * sectorSize);

                for(ulong i = 0; i < remainingSectors; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, bufferPos, sectorSize);
                    bufferPos += (int)sectorSize;
                }
            }

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag,
                                          out byte[] buffer)
        {
            buffer = null;

            if(tag == SectorTagType.CdTrackFlags)
                track = (uint)sectorAddress;

            if(track == 0)
            {
                if(sectorAddress + length > _densitySeparationSectors)
                    return ErrorNumber.OutOfRange;

                if(tag != SectorTagType.CdTrackFlags)
                    return ErrorNumber.NotSupported;

                buffer = new byte[]
                {
                    0x00
                };

                return ErrorNumber.NoError;
            }

            var aaruTrack = new GdiTrack
            {
                Sequence = 0
            };

            foreach(GdiTrack gdiTrack in _discImage.Tracks.Where(gdiTrack => gdiTrack.Sequence == track))
            {
                aaruTrack = gdiTrack;

                break;
            }

            if(aaruTrack.Sequence == 0)
                return ErrorNumber.SectorNotFound;

            if(length > aaruTrack.Sectors)
                return ErrorNumber.OutOfRange;

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
                    buffer = new byte[1];

                    buffer[0] += aaruTrack.Flags;

                    return ErrorNumber.NoError;
                }
                default: return ErrorNumber.NotSupported;
            }

            switch(aaruTrack.TrackType)
            {
                case TrackType.Audio: return ErrorNumber.NoData;
                case TrackType.CdMode1:
                {
                    // TODO: Build
                    if(aaruTrack.Bps != 2352)
                        return ErrorNumber.NoData;

                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        {
                            sectorOffset = 0;
                            sectorSize   = 12;
                            sectorSkip   = 2340;

                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize   = 4;
                            sectorSkip   = 2336;

                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                        case SectorTagType.CdSectorSubHeader: return ErrorNumber.NotSupported;
                        case SectorTagType.CdSectorEcc:
                        {
                            sectorOffset = 2076;
                            sectorSize   = 276;
                            sectorSkip   = 0;

                            break;
                        }
                        case SectorTagType.CdSectorEccP:
                        {
                            sectorOffset = 2076;
                            sectorSize   = 172;
                            sectorSkip   = 104;

                            break;
                        }
                        case SectorTagType.CdSectorEccQ:
                        {
                            sectorOffset = 2248;
                            sectorSize   = 104;
                            sectorSkip   = 0;

                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2064;
                            sectorSize   = 4;
                            sectorSkip   = 284;

                            break;
                        }
                        default: return ErrorNumber.NotSupported;
                    }

                    break;
                }
                default: return ErrorNumber.NotSupported;
            }

            buffer = new byte[sectorSize * length];

            ulong remainingSectors = length;

            if(aaruTrack.Pregap > 0 &&
               sectorAddress    < aaruTrack.Pregap)
            {
                ulong remainingPregap = aaruTrack.Pregap - sectorAddress;

                remainingSectors -= length > remainingPregap ? remainingPregap : length;
            }

            if(remainingSectors == 0)
                return ErrorNumber.NoError;

            _imageStream = aaruTrack.TrackFilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

            long pos = aaruTrack.Offset + (long)((sectorAddress    * (sectorOffset + sectorSize + sectorSkip)) -
                                                 (aaruTrack.Pregap * aaruTrack.Bps));

            if(pos < 0)
                pos = 0;

            br.BaseStream.Seek(pos, SeekOrigin.Begin);

            if(sectorOffset     == 0 &&
               sectorSkip       == 0 &&
               remainingSectors == length)
                buffer = br.ReadBytes((int)(sectorSize * remainingSectors));
            else if(sectorOffset == 0 &&
                    sectorSkip   == 0)
            {
                byte[] tmp = br.ReadBytes((int)(sectorSize * remainingSectors));
                Array.Copy(tmp, 0, buffer, (int)((length - remainingSectors) * sectorSize), tmp.Length);
            }
            else
            {
                int bufferPos = (int)((length - remainingSectors) * sectorSize);

                for(ulong i = 0; i < remainingSectors; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, bufferPos, sectorSize);
                    bufferPos += (int)sectorSize;
                }
            }

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer) =>
            ReadSectorsLong(sectorAddress, 1, out buffer);

        /// <inheritdoc />
        public ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer) =>
            ReadSectorsLong(sectorAddress, 1, track, out buffer);

        /// <inheritdoc />
        public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer)
        {
            buffer = null;

            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress >= kvp.Value
                                                     from gdiTrack in _discImage.Tracks
                                                     where gdiTrack.Sequence         == kvp.Key
                                                     where sectorAddress - kvp.Value < gdiTrack.Sectors select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key, out buffer);

            return ErrorNumber.SectorNotFound;
        }

        /// <inheritdoc />
        public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, uint track, out byte[] buffer)
        {
            buffer = null;

            if(track == 0)
            {
                if(sectorAddress + length > _densitySeparationSectors)
                    return ErrorNumber.OutOfRange;

                buffer = new byte[length * 2352];

                return ErrorNumber.NoError;
            }

            var aaruTrack = new GdiTrack
            {
                Sequence = 0
            };

            foreach(GdiTrack gdiTrack in _discImage.Tracks.Where(gdiTrack => gdiTrack.Sequence == track))
            {
                aaruTrack = gdiTrack;

                break;
            }

            if(aaruTrack.Sequence == 0)
                return ErrorNumber.SectorNotFound;

            if(sectorAddress + length > aaruTrack.Sectors)
                return ErrorNumber.OutOfRange;

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(aaruTrack.TrackType)
            {
                case TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                case TrackType.CdMode1:
                {
                    if(aaruTrack.Bps == 2352)
                    {
                        sectorOffset = 0;
                        sectorSize   = 2352;
                        sectorSkip   = 0;
                    }
                    else
                    {
                        sectorOffset = 0;
                        sectorSize   = 2048;
                        sectorSkip   = 0;
                    }

                    break;
                }
                default: return ErrorNumber.NotSupported;
            }

            buffer = new byte[sectorSize * length];

            ulong remainingSectors = length;

            if(aaruTrack.Pregap > 0 &&
               sectorAddress    < aaruTrack.Pregap)
            {
                ulong remainingPregap = aaruTrack.Pregap - sectorAddress;

                remainingSectors -= length > remainingPregap ? remainingPregap : length;
            }

            if(remainingSectors == 0)
                return ErrorNumber.NoError;

            _imageStream = aaruTrack.TrackFilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

            long pos = aaruTrack.Offset + (long)((sectorAddress    * (sectorOffset + sectorSize + sectorSkip)) -
                                                 (aaruTrack.Pregap * aaruTrack.Bps));

            if(pos < 0)
                pos = 0;

            br.BaseStream.Seek(pos, SeekOrigin.Begin);

            if(sectorOffset     == 0 &&
               sectorSkip       == 0 &&
               remainingSectors == length)
                buffer = br.ReadBytes((int)(sectorSize * remainingSectors));
            else if(sectorOffset == 0 &&
                    sectorSkip   == 0)
            {
                byte[] tmp = br.ReadBytes((int)(sectorSize * remainingSectors));
                Array.Copy(tmp, 0, buffer, (int)((length - remainingSectors) * sectorSize), tmp.Length);
            }
            else
            {
                int bufferPos = (int)((length - remainingSectors) * sectorSize);

                for(ulong i = 0; i < remainingSectors; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, bufferPos, sectorSize);
                    bufferPos += (int)sectorSize;
                }
            }

            switch(aaruTrack.TrackType)
            {
                case TrackType.CdMode1 when aaruTrack.Bps == 2048:
                {
                    byte[] fullSector = new byte[2352];
                    byte[] fullBuffer = new byte[2352 * length];

                    for(uint i = 0; i < length; i++)
                    {
                        Array.Copy(buffer, i * 2048, fullSector, 16, 2048);
                        _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode1, (long)(sectorAddress + i));
                        _sectorBuilder.ReconstructEcc(ref fullSector, TrackType.CdMode1);
                        Array.Copy(fullSector, 0, fullBuffer, i * 2352, 2352);
                    }

                    buffer = fullBuffer;

                    break;
                }
            }

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(Session session)
        {
            if(_discImage.Sessions.Contains(session))
                return GetSessionTracks(session.Sequence);

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(ushort session)
        {
            List<Track> tracks = new();
            bool        expectedDensity;

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

            foreach(GdiTrack gdiTrack in _discImage.Tracks)
                if(gdiTrack.HighDensity == expectedDensity)
                {
                    var track = new Track
                    {
                        Description       = null,
                        StartSector       = gdiTrack.StartSector,
                        Pregap            = gdiTrack.Pregap,
                        Session           = (ushort)(gdiTrack.HighDensity ? 2 : 1),
                        Sequence          = gdiTrack.Sequence,
                        Type              = gdiTrack.TrackType,
                        Filter            = gdiTrack.TrackFilter,
                        File              = gdiTrack.TrackFile,
                        FileOffset        = (ulong)gdiTrack.Offset,
                        FileType          = "BINARY",
                        RawBytesPerSector = gdiTrack.Bps,
                        BytesPerSector    = gdiTrack.TrackType == TrackType.Data ? 2048 : 2352,
                        SubchannelType    = TrackSubchannelType.None
                    };

                    track.EndSector = track.StartSector + gdiTrack.Sectors - 1;

                    tracks.Add(track);
                }

            return tracks;
        }
    }
}