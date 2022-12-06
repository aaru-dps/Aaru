﻿// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Write.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Writes cdrdao cuesheets (toc/bin).
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.DiscImages
{
    public sealed partial class Cdrdao
    {
        /// <inheritdoc />
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
            if(options != null)
            {
                if(options.TryGetValue("separate", out string tmpValue))
                {
                    if(!bool.TryParse(tmpValue, out _separateTracksWriting))
                    {
                        ErrorMessage = "Invalid value for split option";

                        return false;
                    }

                    if(_separateTracksWriting)
                    {
                        ErrorMessage = "Separate tracks not yet implemented";

                        return false;
                    }
                }
            }
            else
                _separateTracksWriting = false;

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupported media format {mediaType}";

                return false;
            }

            _imageInfo = new ImageInfo
            {
                MediaType  = mediaType,
                SectorSize = sectorSize,
                Sectors    = sectors
            };

            // TODO: Separate tracks
            try
            {
                _writingBaseName =
                    Path.Combine(Path.GetDirectoryName(path) ?? "", Path.GetFileNameWithoutExtension(path));

                _descriptorStream = new StreamWriter(path, false, Encoding.ASCII);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";

                return false;
            }

            _discimage = new CdrdaoDisc
            {
                Disktype = mediaType,
                Tracks   = new List<CdrdaoTrack>()
            };

            _trackFlags = new Dictionary<byte, byte>();
            _trackIsrcs = new Dictionary<byte, string>();

            IsWriting    = true;
            ErrorMessage = null;

            return true;
        }

        /// <inheritdoc />
        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            switch(tag)
            {
                case MediaTagType.CD_MCN:
                    _discimage.Mcn = Encoding.ASCII.GetString(data);

                    return true;
                default:
                    ErrorMessage = $"Unsupported media tag {tag}";

                    return false;
            }
        }

        /// <inheritdoc />
        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            FileStream trackStream = _writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

            if(trackStream == null)
            {
                ErrorMessage = $"Can't found file containing {sectorAddress}";

                return false;
            }

            if(track.TrackBytesPerSector != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Invalid write mode for this sector";

                return false;
            }

            if(data.Length != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            // cdrdao audio tracks are endian swapped corresponding to Aaru
            if(track.TrackType == TrackType.Audio)
            {
                byte[] swapped = new byte[data.Length];

                for(long i = 0; i < swapped.Length; i += 2)
                {
                    swapped[i] = data[i + 1];
                    swapped[i           + 1] = data[i];
                }

                data = swapped;
            }

            trackStream.
                Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector)),
                     SeekOrigin.Begin);

            trackStream.Write(data, 0, data.Length);

            return true;
        }

        /// <inheritdoc />
        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            FileStream trackStream = _writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

            if(trackStream == null)
            {
                ErrorMessage = $"Can't found file containing {sectorAddress}";

                return false;
            }

            if(track.TrackBytesPerSector != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Invalid write mode for this sector";

                return false;
            }

            if(sectorAddress + length > track.TrackEndSector + 1)
            {
                ErrorMessage = "Can't cross tracks";

                return false;
            }

            if(data.Length % track.TrackRawBytesPerSector != 0)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            // cdrdao audio tracks are endian swapped corresponding to Aaru
            if(track.TrackType == TrackType.Audio)
            {
                byte[] swapped = new byte[data.Length];

                for(long i = 0; i < swapped.Length; i += 2)
                {
                    swapped[i] = data[i + 1];
                    swapped[i           + 1] = data[i];
                }

                data = swapped;
            }

            switch(track.TrackSubchannelType)
            {
                case TrackSubchannelType.None:
                    trackStream.
                        Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector)),
                             SeekOrigin.Begin);

                    trackStream.Write(data, 0, data.Length);

                    ErrorMessage = "";

                    return true;
                case TrackSubchannelType.Raw:
                case TrackSubchannelType.RawInterleaved:
                    trackStream.
                        Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + 96))),
                             SeekOrigin.Begin);

                    for(uint i = 0; i < length; i++)
                    {
                        trackStream.Write(data, (int)(i * track.TrackRawBytesPerSector), track.TrackRawBytesPerSector);
                        trackStream.Position += 96;
                    }

                    ErrorMessage = "";

                    return true;
                default:
                    ErrorMessage = "Invalid subchannel mode for this sector";

                    return false;
            }
        }

        /// <inheritdoc />
        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            FileStream trackStream = _writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

            if(trackStream == null)
            {
                ErrorMessage = $"Can't found file containing {sectorAddress}";

                return false;
            }

            if(data.Length != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            // cdrdao audio tracks are endian swapped corresponding to Aaru
            if(track.TrackType == TrackType.Audio)
            {
                byte[] swapped = new byte[data.Length];

                for(long i = 0; i < swapped.Length; i += 2)
                {
                    swapped[i] = data[i + 1];
                    swapped[i           + 1] = data[i];
                }

                data = swapped;
            }

            uint subchannelSize = (uint)(track.TrackSubchannelType != TrackSubchannelType.None ? 96 : 0);

            trackStream.
                Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + subchannelSize))),
                     SeekOrigin.Begin);

            trackStream.Write(data, 0, data.Length);

            return true;
        }

        /// <inheritdoc />
        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            FileStream trackStream = _writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

            if(trackStream == null)
            {
                ErrorMessage = $"Can't found file containing {sectorAddress}";

                return false;
            }

            if(sectorAddress + length > track.TrackEndSector + 1)
            {
                ErrorMessage = "Can't cross tracks";

                return false;
            }

            if(data.Length % track.TrackRawBytesPerSector != 0)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            // cdrdao audio tracks are endian swapped corresponding to Aaru
            if(track.TrackType == TrackType.Audio)
            {
                byte[] swapped = new byte[data.Length];

                for(long i = 0; i < swapped.Length; i += 2)
                {
                    swapped[i] = data[i + 1];
                    swapped[i           + 1] = data[i];
                }

                data = swapped;
            }

            uint subchannelSize = (uint)(track.TrackSubchannelType != TrackSubchannelType.None ? 96 : 0);

            for(uint i = 0; i < length; i++)
            {
                trackStream.
                    Seek((long)(track.TrackFileOffset + ((i + sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + subchannelSize))),
                         SeekOrigin.Begin);

                trackStream.Write(data, (int)(i * track.TrackRawBytesPerSector), track.TrackRawBytesPerSector);
            }

            return true;
        }

        /// <inheritdoc />
        public bool SetTracks(List<Track> tracks)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(tracks       == null ||
               tracks.Count == 0)
            {
                ErrorMessage = "Invalid tracks sent";

                return false;
            }

            if(_writingTracks  != null &&
               _writingStreams != null)
                foreach(FileStream oldTrack in _writingStreams.Select(t => t.Value).Distinct())
                    oldTrack.Close();

            ulong currentOffset = 0;
            _writingTracks = new List<Track>();

            foreach(Track track in tracks.OrderBy(t => t.TrackSequence))
            {
                if(track.TrackSubchannelType == TrackSubchannelType.Q16 ||
                   track.TrackSubchannelType == TrackSubchannelType.Q16Interleaved)
                {
                    ErrorMessage =
                        $"Unsupported subchannel type {track.TrackSubchannelType} for track {track.TrackSequence}";

                    return false;
                }

                Track newTrack = track;

                newTrack.TrackFile = _separateTracksWriting ? _writingBaseName + $"_track{track.TrackSequence:D2}.bin"
                                         : _writingBaseName                    + ".bin";

                newTrack.TrackFileOffset = _separateTracksWriting ? 0 : currentOffset;
                _writingTracks.Add(newTrack);

                currentOffset += (ulong)newTrack.TrackRawBytesPerSector *
                                 (newTrack.TrackEndSector - newTrack.TrackStartSector + 1);

                if(track.TrackSubchannelType != TrackSubchannelType.None)
                    currentOffset += 96 * (newTrack.TrackEndSector - newTrack.TrackStartSector + 1);
            }

            _writingStreams = new Dictionary<uint, FileStream>();

            if(_separateTracksWriting)
                foreach(Track track in _writingTracks)
                    _writingStreams.Add(track.TrackSequence,
                                        new FileStream(track.TrackFile, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                                                       FileShare.None));
            else
            {
                var jointStream = new FileStream(_writingBaseName + ".bin", FileMode.OpenOrCreate, FileAccess.ReadWrite,
                                                 FileShare.None);

                foreach(Track track in _writingTracks)
                    _writingStreams.Add(track.TrackSequence, jointStream);
            }

            return true;
        }

        /// <inheritdoc />
        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";

                return false;
            }

            if(_separateTracksWriting)
                foreach(FileStream writingStream in _writingStreams.Values)
                {
                    writingStream.Flush();
                    writingStream.Close();
                }
            else
            {
                _writingStreams.First().Value.Flush();
                _writingStreams.First().Value.Close();
            }

            bool data = _writingTracks.Count(t => t.TrackType != TrackType.Audio) > 0;

            bool mode2 = _writingTracks.Count(t => t.TrackType == TrackType.CdMode2Form1 ||
                                                   t.TrackType == TrackType.CdMode2Form2 ||
                                                   t.TrackType == TrackType.CdMode2Formless) > 0;

            if(mode2)
                _descriptorStream.WriteLine("CD_ROM_XA");
            else if(data)
                _descriptorStream.WriteLine("CD_ROM");
            else
                _descriptorStream.WriteLine("CD_DA");

            if(!string.IsNullOrWhiteSpace(_discimage.Comment))
            {
                string[] commentLines = _discimage.Comment.Split(new[]
                {
                    '\n'
                }, StringSplitOptions.RemoveEmptyEntries);

                foreach(string line in commentLines)
                    _descriptorStream.WriteLine("// {0}", line);
            }

            _descriptorStream.WriteLine();

            if(!string.IsNullOrEmpty(_discimage.Mcn))
                _descriptorStream.WriteLine("CATALOG {0}", _discimage.Mcn);

            foreach(Track track in _writingTracks)
            {
                _descriptorStream.WriteLine();
                _descriptorStream.WriteLine("// Track {0}", track.TrackSequence);

                string subchannelType;

                switch(track.TrackSubchannelType)
                {
                    case TrackSubchannelType.Packed:
                    case TrackSubchannelType.PackedInterleaved:
                        subchannelType = " RW";

                        break;
                    case TrackSubchannelType.Raw:
                    case TrackSubchannelType.RawInterleaved:
                        subchannelType = " RW_RAW";

                        break;
                    default:
                        subchannelType = "";

                        break;
                }

                _descriptorStream.WriteLine("TRACK {0}{1}", GetTrackMode(track), subchannelType);

                _trackFlags.TryGetValue((byte)track.TrackSequence, out byte flagsByte);

                var flags = (CdFlags)flagsByte;

                _descriptorStream.WriteLine("{0}COPY", flags.HasFlag(CdFlags.CopyPermitted) ? "" : "NO ");

                if(track.TrackType == TrackType.Audio)
                {
                    _descriptorStream.WriteLine("{0}PRE_EMPHASIS", flags.HasFlag(CdFlags.PreEmphasis) ? "" : "NO ");

                    _descriptorStream.WriteLine("{0}_CHANNEL_AUDIO",
                                                flags.HasFlag(CdFlags.FourChannel) ? "FOUR" : "TWO");
                }

                if(_trackIsrcs.TryGetValue((byte)track.TrackSequence, out string isrc) &&
                   !string.IsNullOrWhiteSpace(isrc))
                    _descriptorStream.WriteLine("ISRC {0}", isrc);

                (byte minute, byte second, byte frame) msf =
                    LbaToMsf(track.TrackEndSector - track.TrackStartSector + 1);

                _descriptorStream.WriteLine("DATAFILE \"{0}\" #{1} {2:D2}:{3:D2}:{4:D2} // length in bytes: {5}",
                                            Path.GetFileName(track.TrackFile), track.TrackFileOffset, msf.minute,
                                            msf.second, msf.frame,
                                            (track.TrackEndSector - track.TrackStartSector + 1) *
                                            (ulong)(track.TrackRawBytesPerSector +
                                                    (track.TrackSubchannelType != TrackSubchannelType.None ? 96 : 0)));

                foreach(KeyValuePair<ushort, int> index in track.Indexes.OrderBy(i => i.Key).Where(i => i.Key > 1))
                {
                    msf = LbaToMsf((ulong)index.Value - (track.TrackPregap + track.TrackStartSector));

                    _descriptorStream.WriteLine("INDEX {0:D2}:{1:D2}:{2:D2}", msf.minute, msf.second, msf.frame);
                }

                _descriptorStream.WriteLine();
            }

            _descriptorStream.Flush();
            _descriptorStream.Close();

            IsWriting    = false;
            ErrorMessage = "";

            return true;
        }

        /// <inheritdoc />
        public bool SetMetadata(ImageInfo metadata)
        {
            _discimage.Barcode = metadata.MediaBarcode;
            _discimage.Comment = metadata.Comments;

            return true;
        }

        /// <inheritdoc />
        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            ErrorMessage = "Unsupported feature";

            return false;
        }

        /// <inheritdoc />
        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            switch(tag)
            {
                case SectorTagType.CdTrackFlags:
                {
                    if(data.Length != 1)
                    {
                        ErrorMessage = "Incorrect data size for track flags";

                        return false;
                    }

                    _trackFlags[(byte)sectorAddress] = data[0];

                    return true;
                }
                case SectorTagType.CdTrackIsrc:
                {
                    if(data != null)
                        _trackIsrcs[(byte)sectorAddress] = Encoding.UTF8.GetString(data);

                    return true;
                }
                case SectorTagType.CdSectorSubchannel:
                {
                    if(track.TrackSubchannelType == 0)
                    {
                        ErrorMessage =
                            $"Trying to write subchannel to track {track.TrackSequence}, that does not have subchannel";

                        return false;
                    }

                    if(data.Length != 96)
                    {
                        ErrorMessage = "Incorrect data size for subchannel";

                        return false;
                    }

                    FileStream trackStream =
                        _writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

                    if(trackStream == null)
                    {
                        ErrorMessage = $"Can't found file containing {sectorAddress}";

                        return false;
                    }

                    trackStream.
                        Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + 96))) + track.TrackRawBytesPerSector,
                             SeekOrigin.Begin);

                    trackStream.Write(data, 0, data.Length);

                    return true;
                }
                default:
                    ErrorMessage = $"Unsupported tag type {tag}";

                    return false;
            }
        }

        /// <inheritdoc />
        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            switch(tag)
            {
                case SectorTagType.CdTrackFlags:
                case SectorTagType.CdTrackIsrc: return WriteSectorTag(data, sectorAddress, tag);
                case SectorTagType.CdSectorSubchannel:
                {
                    if(track.TrackSubchannelType == 0)
                    {
                        ErrorMessage =
                            $"Trying to write subchannel to track {track.TrackSequence}, that does not have subchannel";

                        return false;
                    }

                    if(data.Length % 96 != 0)
                    {
                        ErrorMessage = "Incorrect data size for subchannel";

                        return false;
                    }

                    FileStream trackStream =
                        _writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

                    if(trackStream == null)
                    {
                        ErrorMessage = $"Can't found file containing {sectorAddress}";

                        return false;
                    }

                    for(uint i = 0; i < length; i++)
                    {
                        trackStream.
                            Seek((long)(track.TrackFileOffset + ((i + sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + 96))) + track.TrackRawBytesPerSector,
                                 SeekOrigin.Begin);

                        trackStream.Write(data, (int)(i * 96), 96);
                    }

                    return true;
                }
                default:
                    ErrorMessage = $"Unsupported tag type {tag}";

                    return false;
            }
        }

        /// <inheritdoc />
        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

        /// <inheritdoc />
        public bool SetCicmMetadata(CICMMetadataType metadata) => false;
    }
}