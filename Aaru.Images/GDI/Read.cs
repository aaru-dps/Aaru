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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.CD;
using Aaru.Logging;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class Gdi
{
#region IOpticalMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        if(imageFilter == null) return ErrorNumber.NoSuchFile;

        try
        {
            imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
            _gdiStream = new StreamReader(imageFilter.GetDataForkStream());
            var lineNumber  = 0;
            var highDensity = false;

            // Initialize all RegExs
            Regex regexTrack = TrackRegex();

            // Initialize all RegEx matches

            // Initialize disc
            _discImage = new GdiDisc
            {
                Sessions = [],
                Tracks   = []
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
                    if(int.TryParse(line, out _)) continue;

                    AaruLogging.Error(Localization.Not_a_correct_Dreamcast_GDI_image);

                    return ErrorNumber.InvalidArgument;
                }

                Match trackMatch = regexTrack.Match(line ?? "");

                if(!trackMatch.Success)
                {
                    AaruLogging.Error(string.Format(Localization.Unknown_line_0_at_line_1, line, lineNumber));

                    return ErrorNumber.InvalidArgument;
                }

                AaruLogging.Debug(MODULE_NAME,
                                  Localization.Found_track_0_starts_at_1_flags_2_type_3_file_4_offset_5_at_line_6,
                                  trackMatch.Groups["track"].Value,
                                  trackMatch.Groups["start"].Value,
                                  trackMatch.Groups["flags"].Value,
                                  trackMatch.Groups["type"].Value,
                                  trackMatch.Groups["filename"].Value,
                                  trackMatch.Groups["offset"].Value,
                                  lineNumber);


                var currentTrack = new GdiTrack
                {
                    Bps         = ushort.Parse(trackMatch.Groups["type"].Value),
                    Flags       = byte.Parse(trackMatch.Groups["flags"].Value),
                    Offset      = long.Parse(trackMatch.Groups["offset"].Value),
                    Sequence    = uint.Parse(trackMatch.Groups["track"].Value),
                    StartSector = ulong.Parse(trackMatch.Groups["start"].Value),
                    TrackFilter = ResolveDataFile(trackMatch.Groups["filename"].Value.Replace("\\\"", "\"").Trim('"'),
                                                  imageFilter.ParentFolder)
                };

                if(currentTrack.TrackFilter is null)
                {
                    AaruLogging.Error(Localization.Cannot_find_data_file_0, trackMatch.Groups["filename"].Value);

                    return ErrorNumber.NoSuchFile;
                }

                currentTrack.TrackFile = currentTrack.TrackFilter.Filename;

                if(currentTrack.StartSector - currentStart > 0)
                {
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
                }

                if((currentTrack.TrackFilter.DataForkLength - currentTrack.Offset) % currentTrack.Bps != 0)
                {
                    AaruLogging.Error(Localization.Track_size_not_a_multiple_of_sector_size);

                    return ErrorNumber.InvalidArgument;
                }

                currentTrack.Sectors = (ulong)((currentTrack.TrackFilter.DataForkLength - currentTrack.Offset) /
                                               currentTrack.Bps);

                currentTrack.Sectors     += currentTrack.Pregap;
                currentStart             += currentTrack.Sectors;
                currentTrack.HighDensity =  highDensity;

                currentTrack.TrackType = (currentTrack.Flags & 0x4) == 0x4 ? TrackType.CdMode1 : TrackType.Audio;

                _discImage.Tracks.Add(currentTrack);
            }

            var sessions = new Session[2];

            for(var s = 0; s < sessions.Length; s++)
            {
                if(s == 0)
                {
                    sessions[s].Sequence = 1;

                    foreach(GdiTrack trk in _discImage.Tracks.Where(static trk => !trk.HighDensity))
                    {
                        if(sessions[s].StartTrack == 0)
                            sessions[s].StartTrack                                            = trk.Sequence;
                        else if(sessions[s].StartTrack > trk.Sequence) sessions[s].StartTrack = trk.Sequence;

                        if(sessions[s].EndTrack < trk.Sequence) sessions[s].EndTrack = trk.Sequence;

                        if(sessions[s].StartSector > trk.StartSector) sessions[s].StartSector = trk.StartSector;

                        if(sessions[s].EndSector < trk.Sectors                    + trk.StartSector - 1)
                            sessions[s].EndSector = trk.Sectors + trk.StartSector - 1;
                    }
                }
                else
                {
                    sessions[s].Sequence = 2;

                    foreach(GdiTrack trk in _discImage.Tracks.Where(static trk => trk.HighDensity))
                    {
                        if(sessions[s].StartTrack == 0)
                            sessions[s].StartTrack                                            = trk.Sequence;
                        else if(sessions[s].StartTrack > trk.Sequence) sessions[s].StartTrack = trk.Sequence;

                        if(sessions[s].EndTrack < trk.Sequence) sessions[s].EndTrack = trk.Sequence;

                        if(sessions[s].StartSector > trk.StartSector) sessions[s].StartSector = trk.StartSector;

                        if(sessions[s].EndSector < trk.Sectors                    + trk.StartSector - 1)
                            sessions[s].EndSector = trk.Sectors + trk.StartSector - 1;
                    }
                }
            }

            _discImage.Sessions.Add(sessions[0]);
            _discImage.Sessions.Add(sessions[1]);

            _discImage.Disktype = MediaType.GDROM;

            // DEBUG information
            AaruLogging.Debug(MODULE_NAME, Localization.Disc_image_parsing_results);

            AaruLogging.Debug(MODULE_NAME, Localization.Session_information);

            AaruLogging.Debug(MODULE_NAME, "\t" + Localization.Disc_contains_0_sessions, _discImage.Sessions.Count);

            for(var i = 0; i < _discImage.Sessions.Count; i++)
            {
                AaruLogging.Debug(MODULE_NAME, "\t" + Localization.Session_0_information, i + 1);

                AaruLogging.Debug(MODULE_NAME,
                                  "\t\t" + Localization.Starting_track_0,
                                  _discImage.Sessions[i].StartTrack);

                AaruLogging.Debug(MODULE_NAME,
                                  "\t\t" + Localization.Starting_sector_0,
                                  _discImage.Sessions[i].StartSector);

                AaruLogging.Debug(MODULE_NAME, "\t\t" + Localization.Ending_track_0, _discImage.Sessions[i].EndTrack);

                AaruLogging.Debug(MODULE_NAME, "\t\t" + Localization.Ending_sector_0, _discImage.Sessions[i].EndSector);
            }

            AaruLogging.Debug(MODULE_NAME, Localization.Track_information);

            AaruLogging.Debug(MODULE_NAME, "\t" + Localization.Disc_contains_0_tracks, _discImage.Tracks.Count);

            for(var i = 0; i < _discImage.Tracks.Count; i++)
            {
                AaruLogging.Debug(MODULE_NAME, "\t" + Localization.Track_0_information, _discImage.Tracks[i].Sequence);

                AaruLogging.Debug(MODULE_NAME, "\t\t" + Localization._0_bytes_per_sector, _discImage.Tracks[i].Bps);

                AaruLogging.Debug(MODULE_NAME, "\t\t" + Localization.Pregap_0_sectors, _discImage.Tracks[i].Pregap);

                if((_discImage.Tracks[i].Flags & 0x8) == 0x8)
                    AaruLogging.Debug(MODULE_NAME, "\t\t" + Localization.Track_is_flagged_as_quadraphonic);

                if((_discImage.Tracks[i].Flags & 0x4) == 0x4)
                    AaruLogging.Debug(MODULE_NAME, "\t\t" + Localization.Track_is_data);

                if((_discImage.Tracks[i].Flags & 0x2) == 0x2)
                    AaruLogging.Debug(MODULE_NAME, "\t\t" + Localization.Track_allows_digital_copy);

                if((_discImage.Tracks[i].Flags & 0x1) == 0x1)
                    AaruLogging.Debug(MODULE_NAME, "\t\t" + Localization.Track_has_pre_emphasis_applied);

                AaruLogging.Debug(MODULE_NAME,
                                  "\t\t" + Localization.Track_resides_in_file_0_type_defined_as_1_starting_at_byte_2,
                                  _discImage.Tracks[i].TrackFilter,
                                  _discImage.Tracks[i].TrackType,
                                  _discImage.Tracks[i].Offset);
            }

            AaruLogging.Debug(MODULE_NAME, Localization.Building_offset_map);

            Partitions = [];
            ulong byteOffset = 0;

            for(var i = 0; i < _discImage.Tracks.Count; i++)
            {
                if(_discImage.Tracks[i].Sequence == 1 && i != 0)
                {
                    AaruLogging.Error(Localization.Unordered_tracks);

                    return ErrorNumber.InvalidArgument;
                }

                // Index 01
                var partition = new Partition
                {
                    Description = string.Format(Localization.Track_0, _discImage.Tracks[i].Sequence),
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

            foreach(GdiTrack track in _discImage.Tracks) _imageInfo.ImageSize += track.Bps * track.Sectors;

            foreach(GdiTrack track in _discImage.Tracks) _imageInfo.Sectors += track.Sectors;

            _imageInfo.Sectors += _densitySeparationSectors;

            _imageInfo.SectorSize = 2352; // All others

            foreach(GdiTrack unused in _discImage.Tracks.Where(static track =>
                                                                   (track.Flags & 0x4) == 0x4 && track.Bps == 2352))
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

            _imageInfo.MetadataMediaType = MetadataMediaType.OpticalDisc;

            AaruLogging.Verbose(Localization.GDI_image_describes_a_disc_of_type_0, _imageInfo.MediaType);

            _sectorBuilder = new SectorBuilder();

            return ErrorNumber.NoError;
        }
        catch(Exception ex)
        {
            AaruLogging.Error(Localization.Exception_trying_to_identify_image_file_0, imageFilter.BasePath);
            AaruLogging.Exception(ex, Localization.Exception_trying_to_identify_image_file_0, imageFilter.BasePath);

            return ErrorNumber.UnexpectedException;
        }
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        sectorStatus = SectorStatus.Dumped;

        return ReadSectors(sectorAddress, negative, 1, out buffer, out _);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, bool negative, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, negative, 1, tag, out buffer);

    public ErrorNumber ReadDPM(out uint    dpmStartSector, out uint dpmResolution, out uint numberOfDpmEntries,
                               out ulong[] dpm)
    {
        dpmStartSector     = 0;
        dpmResolution      = 0;
        numberOfDpmEntries = 0;
        dpm                = null;

        return ErrorNumber.NotSupported;
    }

    public ErrorNumber ReadSectorDPM(ulong sectorAddress, out ulong? dpm)
    {
        dpm = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus)
    {
        sectorStatus = SectorStatus.Dumped;

        return ReadSectors(sectorAddress, 1, track, out buffer, out _);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, 1, track, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.NotSupported;

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap
                                                 where sectorAddress >= kvp.Value
                                                 from gdiTrack in _discImage.Tracks
                                                 where gdiTrack.Sequence         == kvp.Key
                                                 where sectorAddress - kvp.Value < gdiTrack.Sectors
                                                 select kvp)
            return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key, out buffer, out sectorStatus);

        _offsetMap.TryGetValue(0, out ulong transitionStart);

        if(sectorAddress < transitionStart || sectorAddress >= _densitySeparationSectors + transitionStart)
            return ErrorNumber.SectorNotFound;

        return ReadSectors(sectorAddress - transitionStart, length, 0, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, bool negative, uint length, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if(negative) return ErrorNumber.NotSupported;

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap
                                                 where sectorAddress >= kvp.Value
                                                 from gdiTrack in _discImage.Tracks
                                                 where gdiTrack.Sequence         == kvp.Key
                                                 where sectorAddress - kvp.Value < gdiTrack.Sectors
                                                 select kvp)
            return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag, out buffer);

        _offsetMap.TryGetValue(0, out ulong transitionStart);

        if(sectorAddress < transitionStart || sectorAddress >= _densitySeparationSectors + transitionStart)
            return ErrorNumber.SectorNotFound;

        return ReadSectorsTag(sectorAddress - transitionStart, length, 0, tag, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(track == 0)
        {
            if(sectorAddress + length > _densitySeparationSectors) return ErrorNumber.OutOfRange;

            buffer       = new byte[length * 2352];
            sectorStatus = new SectorStatus[length];

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

        if(aaruTrack.Sequence == 0) return ErrorNumber.SectorNotFound;

        if(sectorAddress + length > aaruTrack.Sectors) return ErrorNumber.OutOfRange;
        sectorStatus = Enumerable.Repeat(SectorStatus.Dumped, (int)length).ToArray();

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
            default:
                return ErrorNumber.NotSupported;
        }

        buffer = new byte[sectorSize * length];

        ulong remainingSectors = length;

        if(aaruTrack.Pregap > 0 && sectorAddress < aaruTrack.Pregap)
        {
            ulong remainingPregap = aaruTrack.Pregap - sectorAddress;

            remainingSectors -= length > remainingPregap ? remainingPregap : length;
        }

        if(remainingSectors == 0) return ErrorNumber.NoError;

        _imageStream = aaruTrack.TrackFilter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        long pos = aaruTrack.Offset +
                   (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip) - aaruTrack.Pregap * aaruTrack.Bps);

        if(pos < 0) pos = 0;

        br.BaseStream.Seek(pos, SeekOrigin.Begin);

        switch(sectorOffset)
        {
            case 0 when sectorSkip == 0 && remainingSectors == length:
                buffer = br.ReadBytes((int)(sectorSize * remainingSectors));

                break;
            case 0 when sectorSkip == 0:
            {
                byte[] tmp = br.ReadBytes((int)(sectorSize * remainingSectors));
                Array.Copy(tmp, 0, buffer, (int)((length - remainingSectors) * sectorSize), tmp.Length);

                break;
            }
            default:
            {
                var bufferPos = (int)((length - remainingSectors) * sectorSize);

                for(ulong i = 0; i < remainingSectors; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, bufferPos, sectorSize);
                    bufferPos += (int)sectorSize;
                }

                break;
            }
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, uint length, uint track, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if(tag == SectorTagType.CdTrackFlags) track = (uint)sectorAddress;

        if(track == 0)
        {
            if(sectorAddress + length > _densitySeparationSectors) return ErrorNumber.OutOfRange;

            if(tag != SectorTagType.CdTrackFlags) return ErrorNumber.NotSupported;

            buffer = new byte[1];

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

        if(aaruTrack.Sequence == 0) return ErrorNumber.SectorNotFound;

        if(sectorAddress + length > aaruTrack.Sectors) return ErrorNumber.OutOfRange;

        uint sectorOffset = 0;
        uint sectorSize   = 0;
        uint sectorSkip   = 0;

        switch(tag)
        {
            case SectorTagType.CdSectorEcc:
            case SectorTagType.CdSectorEccP:
            case SectorTagType.CdSectorEccQ:
            case SectorTagType.CdSectorEdc:
            case SectorTagType.CdSectorHeader:
            case SectorTagType.CdSectorSync:
                break;
            case SectorTagType.CdTrackFlags:
            {
                buffer = new byte[1];

                buffer[0] += aaruTrack.Flags;

                return ErrorNumber.NoError;
            }
            default:
                return ErrorNumber.NotSupported;
        }

        switch(aaruTrack.TrackType)
        {
            case TrackType.Audio:
                return ErrorNumber.NoData;
            case TrackType.CdMode1:
            {
                // TODO: Build
                if(aaruTrack.Bps != 2352) return ErrorNumber.NoData;

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
                }

                break;
            }
            default:
                return ErrorNumber.NotSupported;
        }

        buffer = new byte[sectorSize * length];

        ulong remainingSectors = length;

        if(aaruTrack.Pregap > 0 && sectorAddress < aaruTrack.Pregap)
        {
            ulong remainingPregap = aaruTrack.Pregap - sectorAddress;

            remainingSectors -= length > remainingPregap ? remainingPregap : length;
        }

        if(remainingSectors == 0) return ErrorNumber.NoError;

        _imageStream = aaruTrack.TrackFilter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        long pos = aaruTrack.Offset +
                   (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip) - aaruTrack.Pregap * aaruTrack.Bps);

        if(pos < 0) pos = 0;

        br.BaseStream.Seek(pos, SeekOrigin.Begin);

        switch(sectorOffset)
        {
            case 0 when sectorSkip == 0 && remainingSectors == length:
                buffer = br.ReadBytes((int)(sectorSize * remainingSectors));

                break;
            case 0 when sectorSkip == 0:
            {
                byte[] tmp = br.ReadBytes((int)(sectorSize * remainingSectors));
                Array.Copy(tmp, 0, buffer, (int)((length - remainingSectors) * sectorSize), tmp.Length);

                break;
            }
            default:
            {
                var bufferPos = (int)((length - remainingSectors) * sectorSize);

                for(ulong i = 0; i < remainingSectors; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, bufferPos, sectorSize);
                    bufferPos += (int)sectorSize;
                }

                break;
            }
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong            sectorAddress, bool negative, out byte[] buffer,
                                      out SectorStatus sectorStatus)
    {
        sectorStatus = SectorStatus.Dumped;

        return ReadSectorsLong(sectorAddress, negative, 1, out buffer, out _);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus)
    {
        sectorStatus = SectorStatus.Dumped;

        return ReadSectorsLong(sectorAddress, 1, track, out buffer, out _);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.NotSupported;

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap
                                                 where sectorAddress >= kvp.Value
                                                 from gdiTrack in _discImage.Tracks
                                                 where gdiTrack.Sequence         == kvp.Key
                                                 where sectorAddress - kvp.Value < gdiTrack.Sectors
                                                 select kvp)
            return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key, out buffer, out sectorStatus);

        return ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(track == 0)
        {
            if(sectorAddress + length > _densitySeparationSectors) return ErrorNumber.OutOfRange;

            buffer       = new byte[length * 2352];
            sectorStatus = new SectorStatus[length];

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

        if(aaruTrack.Sequence == 0) return ErrorNumber.SectorNotFound;

        if(sectorAddress + length > aaruTrack.Sectors) return ErrorNumber.OutOfRange;
        sectorStatus = Enumerable.Repeat(SectorStatus.Dumped, (int)length).ToArray();

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
            default:
                return ErrorNumber.NotSupported;
        }

        buffer = new byte[sectorSize * length];

        ulong remainingSectors = length;

        if(aaruTrack.Pregap > 0 && sectorAddress < aaruTrack.Pregap)
        {
            ulong remainingPregap = aaruTrack.Pregap - sectorAddress;

            remainingSectors -= length > remainingPregap ? remainingPregap : length;
        }

        if(remainingSectors == 0) return ErrorNumber.NoError;

        _imageStream = aaruTrack.TrackFilter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        long pos = aaruTrack.Offset +
                   (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip) - aaruTrack.Pregap * aaruTrack.Bps);

        if(pos < 0) pos = 0;

        br.BaseStream.Seek(pos, SeekOrigin.Begin);

        switch(sectorOffset)
        {
            case 0 when sectorSkip == 0 && remainingSectors == length:
                buffer = br.ReadBytes((int)(sectorSize * remainingSectors));

                break;
            case 0 when sectorSkip == 0:
            {
                byte[] tmp = br.ReadBytes((int)(sectorSize * remainingSectors));
                Array.Copy(tmp, 0, buffer, (int)((length - remainingSectors) * sectorSize), tmp.Length);

                break;
            }
            default:
            {
                var bufferPos = (int)((length - remainingSectors) * sectorSize);

                for(ulong i = 0; i < remainingSectors; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, bufferPos, sectorSize);
                    bufferPos += (int)sectorSize;
                }

                break;
            }
        }

        switch(aaruTrack.TrackType)
        {
            case TrackType.CdMode1 when aaruTrack.Bps == 2048:
            {
                var fullSector = new byte[2352];
                var fullBuffer = new byte[2352 * length];

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

    /// <summary>Resolves the path of a file referenced by a GDI</summary>
    /// <remarks>
    ///     GDI files are commonly moved away from the machine that created them, so the path they contain is often
    ///     meaningless, or written for another operating system. Files sitting next to the GDI therefore take
    ///     precedence over anything the path itself points to, including the current working directory.
    /// </remarks>
    /// <param name="datafile">Path, as written in the GDI</param>
    /// <param name="parentFolder">Folder containing the GDI</param>
    /// <returns>Filter for the referenced file, <c>null</c> if it cannot be found</returns>
    static IFilter ResolveDataFile(string datafile, string parentFolder)
    {
        IFilter filter = null;

        // Path as written in the GDI, relative to the GDI itself, with the separators of this operating
        // system, so paths written in another one still resolve
        if(!Path.IsPathRooted(datafile))
        {
            string relative = datafile.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar);

            filter = PluginRegister.Singleton.GetFilter(Path.Combine(parentFolder, relative));
        }

        // Filename alone, next to the GDI, which is where it is found when the GDI has been moved
        string filename = datafile[(datafile.LastIndexOfAny(['/', '\\']) + 1)..];

        if(filter is null && filename.Length > 0)
            filter = PluginRegister.Singleton.GetFilter(Path.Combine(parentFolder, filename));

        // Path as written in the GDI, verbatim, be it absolute or relative to the working directory
        return filter ?? PluginRegister.Singleton.GetFilter(datafile);
    }

    /// <inheritdoc />
    public List<Track> GetSessionTracks(Session session) =>
        _discImage.Sessions.Contains(session) ? GetSessionTracks(session.Sequence) : null;

    /// <inheritdoc />
    public List<Track> GetSessionTracks(ushort session)
    {
        List<Track> tracks = [];
        bool        expectedDensity;

        switch(session)
        {
            case 1:
                expectedDensity = false;

                break;
            case 2:
                expectedDensity = true;

                break;
            default:
                return null;
        }

        foreach(GdiTrack gdiTrack in _discImage.Tracks)
        {
            if(gdiTrack.HighDensity != expectedDensity) continue;

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
                BytesPerSector    = gdiTrack.TrackType == TrackType.Audio ? 2352 : 2048,
                SubchannelType    = TrackSubchannelType.None
            };

            track.EndSector = track.StartSector + gdiTrack.Sectors - 1;

            tracks.Add(track);
        }

        return tracks;
    }

#endregion
}