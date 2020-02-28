// /***************************************************************************
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
// Copyright © 2011-2020 Natalia Portillo
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

namespace Aaru.DiscImages
{
    public partial class CdrWin
    {
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
                MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors
            };

            // TODO: Separate tracks
            try
            {
                _writingBaseName  = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                _descriptorStream = new StreamWriter(path, false, Encoding.ASCII);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";

                return false;
            }

            _discImage = new CdrWinDisc
            {
                MediaType = mediaType, Sessions = new List<Session>(), Tracks = new List<CdrWinTrack>()
            };

            _trackFlags = new Dictionary<byte, byte>();
            _trackIsrcs = new Dictionary<byte, string>();

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
                    _discImage.Mcn = Encoding.ASCII.GetString(data);

                    return true;
                case MediaTagType.CD_TEXT:
                    var cdTextStream = new FileStream(_writingBaseName + "_cdtext.bin", FileMode.Create,
                                                      FileAccess.ReadWrite, FileShare.None);

                    cdTextStream.Write(data, 0, data.Length);
                    _discImage.CdTextFile = Path.GetFileName(cdTextStream.Name);
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
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
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

            trackStream.
                Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector)),
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
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
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

            trackStream.
                Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector)),
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
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
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

            trackStream.
                Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector)),
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
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                     sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
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

            trackStream.
                Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector)),
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
                Track newTrack = track;

                newTrack.TrackFile = _separateTracksWriting ? _writingBaseName + $"_track{track.TrackSequence:D2}.bin"
                                         : _writingBaseName                    + ".bin";

                newTrack.TrackFileOffset = _separateTracksWriting ? 0 : currentOffset;
                _writingTracks.Add(newTrack);

                currentOffset += (ulong)newTrack.TrackRawBytesPerSector *
                                 ((newTrack.TrackEndSector - newTrack.TrackStartSector) + 1);
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

            int currentSession = 0;

            if(!string.IsNullOrWhiteSpace(_discImage.Comment))
            {
                string[] commentLines = _discImage.Comment.Split(new[]
                {
                    '\n'
                }, StringSplitOptions.RemoveEmptyEntries);

                foreach(string line in commentLines)
                    _descriptorStream.WriteLine("REM {0}", line);
            }

            _descriptorStream.WriteLine("REM ORIGINAL MEDIA-TYPE: {0}", MediaTypeToCdrwinType(_imageInfo.MediaType));

            _descriptorStream.WriteLine("REM METADATA AARU MEDIA-TYPE: {0}", _imageInfo.MediaType);

            if(!string.IsNullOrEmpty(_imageInfo.Application))
            {
                _descriptorStream.WriteLine("REM Ripping Tool: {0}", _imageInfo.Application);

                if(!string.IsNullOrEmpty(_imageInfo.ApplicationVersion))
                    _descriptorStream.WriteLine("REM Ripping Tool Version: {0}", _imageInfo.ApplicationVersion);
            }

            if(DumpHardware != null)
            {
                foreach(var dumpData in from dump in DumpHardware from extent in dump.Extents.OrderBy(e => e.Start)
                                        select new
                                        {
                                            dump.Manufacturer, dump.Model, dump.Firmware, dump.Serial,
                                            Application        = dump.Software.Name,
                                            ApplicationVersion = dump.Software.Version, dump.Software.OperatingSystem,
                                            extent.Start, extent.End
                                        })
                {
                    _descriptorStream.
                        WriteLine($"REM METADATA DUMP EXTENT: {dumpData.Application} | {dumpData.ApplicationVersion} | {dumpData.OperatingSystem} | {dumpData.Manufacturer} | {dumpData.Model} | {dumpData.Firmware} | {dumpData.Serial} | {dumpData.Start}:{dumpData.End}");
                }
            }

            if(!string.IsNullOrEmpty(_discImage.CdTextFile))
                _descriptorStream.WriteLine("CDTEXTFILE \"{0}\"", Path.GetFileName(_discImage.CdTextFile));

            if(!string.IsNullOrEmpty(_discImage.Title))
                _descriptorStream.WriteLine("TITLE {0}", _discImage.Title);

            if(!string.IsNullOrEmpty(_discImage.Mcn))
                _descriptorStream.WriteLine("CATALOG {0}", _discImage.Mcn);

            if(!string.IsNullOrEmpty(_discImage.Barcode))
                _descriptorStream.WriteLine("UPC_EAN {0}", _discImage.Barcode);

            if(!_separateTracksWriting)
                _descriptorStream.WriteLine("FILE \"{0}\" BINARY",
                                            Path.GetFileName(_writingStreams.First().Value.Name));

            foreach(Track track in _writingTracks)
            {
                if(track.TrackSession > currentSession)
                    _descriptorStream.WriteLine("REM SESSION {0}", ++currentSession);

                if(_separateTracksWriting)
                    _descriptorStream.WriteLine("FILE \"{0}\" BINARY", Path.GetFileName(track.TrackFile));

                (byte minute, byte second, byte frame) msf = LbaToMsf(track.TrackStartSector);
                _descriptorStream.WriteLine("  TRACK {0:D2} {1}", track.TrackSequence, GetTrackMode(track));

                if(_trackFlags.TryGetValue((byte)track.TrackSequence, out byte flagsByte))
                    if(flagsByte != 0 &&
                       flagsByte != (byte)CdFlags.DataTrack)
                    {
                        var flags = (CdFlags)flagsByte;

                        _descriptorStream.WriteLine("    FLAGS{0}{1}{2}",
                                                    flags.HasFlag(CdFlags.CopyPermitted) ? " DCP" : "",
                                                    flags.HasFlag(CdFlags.FourChannel) ? " 4CH" : "",
                                                    flags.HasFlag(CdFlags.PreEmphasis) ? " PRE" : "");
                    }

                if(_trackIsrcs.TryGetValue((byte)track.TrackSequence, out string isrc))
                    _descriptorStream.WriteLine("    ISRC {0}", isrc);

                if(track.TrackPregap > 0)
                {
                    _descriptorStream.WriteLine("    INDEX {0:D2} {1:D2}:{2:D2}:{3:D2}", 0, msf.minute, msf.second,
                                                msf.frame);

                    msf = LbaToMsf(track.TrackStartSector + track.TrackPregap);

                    _descriptorStream.WriteLine("    INDEX {0:D2} {1:D2}:{2:D2}:{3:D2}", 1, msf.minute, msf.second,
                                                msf.frame);
                }
                else
                    _descriptorStream.WriteLine("    INDEX {0:D2} {1:D2}:{2:D2}:{3:D2}", 1, msf.minute, msf.second,
                                                msf.frame);
            }

            _descriptorStream.Flush();
            _descriptorStream.Close();

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
                _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
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

                    _trackFlags.Add((byte)track.TrackSequence, data[0]);

                    return true;
                }
                case SectorTagType.CdTrackIsrc:
                {
                    if(data != null)
                        _trackIsrcs.Add((byte)track.TrackSequence, Encoding.UTF8.GetString(data));

                    return true;
                }
                default:
                    ErrorMessage = $"Unsupported tag type {tag}";

                    return false;
            }
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag) =>
            WriteSectorTag(data, sectorAddress, tag);

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware)
        {
            DumpHardware = dumpHardware;

            return true;
        }

        public bool SetCicmMetadata(CICMMetadataType metadata) => false;

        public bool SetMetadata(ImageInfo metadata)
        {
            _discImage.Barcode            = metadata.MediaBarcode;
            _discImage.Comment            = metadata.Comments;
            _discImage.Title              = metadata.MediaTitle;
            _imageInfo.Application        = metadata.Application;
            _imageInfo.ApplicationVersion = metadata.ApplicationVersion;

            return true;
        }
    }
}