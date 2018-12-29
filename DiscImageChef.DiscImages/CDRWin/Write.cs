// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Write.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Writes CDRWin cuesheets (cue/bin).
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public partial class CdrWin
    {
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(options != null)
            {
                if(options.TryGetValue("separate", out string tmpValue))
                {
                    if(!bool.TryParse(tmpValue, out separateTracksWriting))
                    {
                        ErrorMessage = "Invalid value for split option";
                        return false;
                    }

                    if(separateTracksWriting)
                    {
                        ErrorMessage = "Separate tracksnot yet implemented";
                        return false;
                    }
                }
            }
            else separateTracksWriting = false;

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

            // TODO: Separate tracks
            try
            {
                writingBaseName  = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                descriptorStream = new StreamWriter(path, false, Encoding.ASCII);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            discimage = new CdrWinDisc
            {
                Disktype = mediaType,
                Sessions = new List<Session>(),
                Tracks   = new List<CdrWinTrack>()
            };

            trackFlags = new Dictionary<byte, byte>();
            trackIsrcs = new Dictionary<byte, string>();

            IsWriting    = true;
            ErrorMessage = null;
            return true;
        }

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
                    discimage.Mcn = Encoding.ASCII.GetString(data);
                    return true;
                case MediaTagType.CD_TEXT:
                    FileStream cdTextStream = new FileStream(writingBaseName + "_cdtext.bin", FileMode.Create,
                                                             FileAccess.ReadWrite, FileShare.None);
                    cdTextStream.Write(data, 0, data.Length);
                    discimage.Cdtextfile = Path.GetFileName(cdTextStream.Name);
                    cdTextStream.Close();
                    return true;
                default:
                    ErrorMessage = $"Unsupported media tag {tag}";
                    return false;
            }
        }

        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            Track track =
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";
                return false;
            }

            FileStream trackStream = writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

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

            trackStream.Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector),
                             SeekOrigin.Begin);
            trackStream.Write(data, 0, data.Length);

            return true;
        }

        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            Track track =
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";
                return false;
            }

            FileStream trackStream = writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

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

            trackStream.Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector),
                             SeekOrigin.Begin);
            trackStream.Write(data, 0, data.Length);

            return true;
        }

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            Track track =
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";
                return false;
            }

            FileStream trackStream = writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

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

            trackStream.Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector),
                             SeekOrigin.Begin);
            trackStream.Write(data, 0, data.Length);

            return true;
        }

        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            Track track =
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";
                return false;
            }

            FileStream trackStream = writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

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

            trackStream.Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector),
                             SeekOrigin.Begin);
            trackStream.Write(data, 0, data.Length);

            return true;
        }

        public bool SetTracks(List<Track> tracks)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(tracks == null || tracks.Count == 0)
            {
                ErrorMessage = "Invalid tracks sent";
                return false;
            }

            if(writingTracks != null && writingStreams != null)
                foreach(FileStream oldTrack in writingStreams.Select(t => t.Value).Distinct())
                    oldTrack.Close();

            ulong currentOffset = 0;
            writingTracks = new List<Track>();
            foreach(Track track in tracks.OrderBy(t => t.TrackSequence))
            {
                Track newTrack = track;
                newTrack.TrackFile = separateTracksWriting
                                         ? writingBaseName + $"_track{track.TrackSequence:D2}.bin"
                                         : writingBaseName + ".bin";
                newTrack.TrackFileOffset = separateTracksWriting ? 0 : currentOffset;
                writingTracks.Add(newTrack);
                currentOffset += (ulong)newTrack.TrackRawBytesPerSector *
                                 (newTrack.TrackEndSector - newTrack.TrackStartSector + 1);
            }

            writingStreams = new Dictionary<uint, FileStream>();
            if(separateTracksWriting)
                foreach(Track track in writingTracks)
                    writingStreams.Add(track.TrackSequence,
                                       new FileStream(track.TrackFile, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                                                      FileShare.None));
            else
            {
                FileStream jointstream = new FileStream(writingBaseName + ".bin", FileMode.OpenOrCreate,
                                                        FileAccess.ReadWrite, FileShare.None);
                foreach(Track track in writingTracks) writingStreams.Add(track.TrackSequence, jointstream);
            }

            return true;
        }

        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";
                return false;
            }

            if(separateTracksWriting)
                foreach(FileStream writingStream in writingStreams.Values)
                {
                    writingStream.Flush();
                    writingStream.Close();
                }
            else
            {
                writingStreams.First().Value.Flush();
                writingStreams.First().Value.Close();
            }

            int currentSession = 0;

            if(!string.IsNullOrWhiteSpace(discimage.Comment))
            {
                string[] commentLines = discimage.Comment.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
                foreach(string line in commentLines) descriptorStream.WriteLine("REM {0}", line);
            }

            descriptorStream.WriteLine("REM ORIGINAL MEDIA-TYPE {0}", MediaTypeToCdrwinType(imageInfo.MediaType));

            if(!string.IsNullOrEmpty(discimage.Cdtextfile))
                descriptorStream.WriteLine("CDTEXTFILE \"{0}\"", Path.GetFileName(discimage.Cdtextfile));

            if(!string.IsNullOrEmpty(discimage.Title)) descriptorStream.WriteLine("TITLE {0}", discimage.Title);

            if(!string.IsNullOrEmpty(discimage.Mcn)) descriptorStream.WriteLine("CATALOG {0}", discimage.Mcn);

            if(!string.IsNullOrEmpty(discimage.Barcode)) descriptorStream.WriteLine("UPC_EAN {0}", discimage.Barcode);

            if(!separateTracksWriting)
                descriptorStream.WriteLine("FILE \"{0}\" BINARY", Path.GetFileName(writingStreams.First().Value.Name));

            foreach(Track track in writingTracks)
            {
                if(track.TrackSession > currentSession) descriptorStream.WriteLine("REM SESSION {0}", ++currentSession);

                if(separateTracksWriting)
                    descriptorStream.WriteLine("FILE \"{0}\" BINARY", Path.GetFileName(track.TrackFile));

                (byte minute, byte second, byte frame) msf = LbaToMsf(track.TrackStartSector);
                descriptorStream.WriteLine("  TRACK {0:D2} {1}", track.TrackSequence, GetTrackMode(track));

                if(trackFlags.TryGetValue((byte)track.TrackSequence, out byte flagsByte))
                    if(flagsByte != 0 && flagsByte != (byte)CdFlags.DataTrack)
                    {
                        CdFlags flags = (CdFlags)flagsByte;
                        descriptorStream.WriteLine("    FLAGS{0}{1}{2}",
                                                   flags.HasFlag(CdFlags.CopyPermitted) ? " DCP" : "",
                                                   flags.HasFlag(CdFlags.FourChannel) ? " 4CH" : "",
                                                   flags.HasFlag(CdFlags.PreEmphasis) ? " PRE" : "");
                    }

                if(trackIsrcs.TryGetValue((byte)track.TrackSequence, out string isrc))
                    descriptorStream.WriteLine("    ISRC {0}", isrc);

                descriptorStream.WriteLine("    INDEX {0:D2} {1:D2}:{2:D2}:{3:D2}", 1, msf.minute, msf.second,
                                           msf.frame);
            }

            descriptorStream.Flush();
            descriptorStream.Close();

            IsWriting    = false;
            ErrorMessage = "";
            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            ErrorMessage = "Unsupported feature";
            return false;
        }

        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            Track track =
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
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

                    trackFlags.Add((byte)track.TrackSequence, data[0]);
                    return true;
                }
                case SectorTagType.CdTrackIsrc:
                {
                    if(data != null) trackIsrcs.Add((byte)track.TrackSequence, Encoding.UTF8.GetString(data));
                    return true;
                }
                default:
                    ErrorMessage = $"Unsupported tag type {tag}";
                    return false;
            }
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            return WriteSectorTag(data, sectorAddress, tag);
        }

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware)
        {
            // Not supported
            return false;
        }

        public bool SetCicmMetadata(CICMMetadataType metadata)
        {
            // Not supported
            return false;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            discimage.Barcode = metadata.MediaBarcode;
            discimage.Comment = metadata.Comments;
            discimage.Title   = metadata.MediaTitle;
            return true;
        }
    }
}