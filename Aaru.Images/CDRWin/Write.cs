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
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Track = Aaru.CommonTypes.Structs.Track;

namespace Aaru.Images;

public sealed partial class CdrWin
{
#region IWritableOpticalImage Members

    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint   sectorSize)
    {
        if(options != null)
        {
            if(options.TryGetValue("separate", out string tmpValue))
            {
                if(!bool.TryParse(tmpValue, out _separateTracksWriting))
                {
                    ErrorMessage = Localization.Invalid_value_for_split_option;

                    return false;
                }

                if(_separateTracksWriting)
                {
                    ErrorMessage = Localization.Separate_tracks_not_yet_implemented;

                    return false;
                }
            }
        }
        else
            _separateTracksWriting = false;

        if(!SupportedMediaTypes.Contains(mediaType))
        {
            ErrorMessage = string.Format(Localization.Unsupported_media_format_0, mediaType);

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
            _writingBaseName = Path.Combine(Path.GetDirectoryName(path) ?? "", Path.GetFileNameWithoutExtension(path));

            _descriptorStream = new StreamWriter(path, false, Encoding.ASCII);
        }
        catch(IOException ex)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, ex.Message);
            AaruConsole.WriteException(ex);

            return false;
        }

        _discImage = new CdrWinDisc
        {
            MediaType = mediaType,
            Sessions  = new List<Session>(),
            Tracks    = new List<CdrWinTrack>()
        };

        var mediaTypeAsInt = (int)_discImage.MediaType;

        _isCd = mediaTypeAsInt is >= 10 and <= 39 or 112 or 113 or >= 150 and <= 152 or 154 or 155 or >= 171 and <= 179
                               or >= 740 and <= 749;

        if(_isCd)
        {
            _trackFlags = new Dictionary<byte, byte>();
            _trackIsrcs = new Dictionary<byte, string>();
        }

        IsWriting    = true;
        ErrorMessage = null;

        return true;
    }

    /// <inheritdoc />
    public bool WriteMediaTag(byte[] data, MediaTagType tag)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        switch(tag)
        {
            case MediaTagType.CD_MCN when _isCd:
                _discImage.Mcn = Encoding.ASCII.GetString(data);

                return true;
            case MediaTagType.CD_TEXT when _isCd:
                var cdTextStream = new FileStream(_writingBaseName + "_cdtext.bin", FileMode.Create,
                                                  FileAccess.ReadWrite, FileShare.None);

                cdTextStream.Write(data, 0, data.Length);
                _discImage.CdTextFile = Path.GetFileName(cdTextStream.Name);
                cdTextStream.Close();

                return true;
            default:
                ErrorMessage = string.Format(Localization.Unsupported_media_tag_0, tag);

                return false;
        }
    }

    /// <inheritdoc />
    public bool WriteSector(byte[] data, ulong sectorAddress)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        Track track =
            _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

        if(track is null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_track_containing_0, sectorAddress);

            return false;
        }

        FileStream trackStream = _writingStreams.FirstOrDefault(kvp => kvp.Key == track.Sequence).Value;

        if(trackStream == null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_file_containing_0, sectorAddress);

            return false;
        }

        if(track.BytesPerSector != track.RawBytesPerSector)
        {
            ErrorMessage = Localization.Invalid_write_mode_for_this_sector;

            return false;
        }

        if(data.Length != track.RawBytesPerSector)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        trackStream.
            Seek((long)(track.FileOffset + (sectorAddress - track.StartSector) * (ulong)track.RawBytesPerSector),
                 SeekOrigin.Begin);

        trackStream.Write(data, 0, data.Length);

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        Track track =
            _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

        if(track is null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_track_containing_0, sectorAddress);

            return false;
        }

        FileStream trackStream = _writingStreams.FirstOrDefault(kvp => kvp.Key == track.Sequence).Value;

        if(trackStream == null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_file_containing_0, sectorAddress);

            return false;
        }

        if(track.BytesPerSector != track.RawBytesPerSector)
        {
            ErrorMessage = Localization.Invalid_write_mode_for_this_sector;

            return false;
        }

        if(sectorAddress + length > track.EndSector + 1)
        {
            ErrorMessage = Localization.Cant_cross_tracks;

            return false;
        }

        if(data.Length % track.RawBytesPerSector != 0)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        trackStream.
            Seek((long)(track.FileOffset + (sectorAddress - track.StartSector) * (ulong)track.RawBytesPerSector),
                 SeekOrigin.Begin);

        trackStream.Write(data, 0, data.Length);

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorLong(byte[] data, ulong sectorAddress)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        Track track =
            _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

        if(track is null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_track_containing_0, sectorAddress);

            return false;
        }

        FileStream trackStream = _writingStreams.FirstOrDefault(kvp => kvp.Key == track.Sequence).Value;

        if(trackStream == null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_file_containing_0, sectorAddress);

            return false;
        }

        if(data.Length != track.RawBytesPerSector)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        trackStream.
            Seek((long)(track.FileOffset + (sectorAddress - track.StartSector) * (ulong)track.RawBytesPerSector),
                 SeekOrigin.Begin);

        trackStream.Write(data, 0, data.Length);

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        Track track =
            _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

        if(track is null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_track_containing_0, sectorAddress);

            return false;
        }

        FileStream trackStream = _writingStreams.FirstOrDefault(kvp => kvp.Key == track.Sequence).Value;

        if(trackStream == null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_file_containing_0, sectorAddress);

            return false;
        }

        if(sectorAddress + length > track.EndSector + 1)
        {
            ErrorMessage = Localization.Cant_cross_tracks;

            return false;
        }

        if(data.Length % track.RawBytesPerSector != 0)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        trackStream.
            Seek((long)(track.FileOffset + (sectorAddress - track.StartSector) * (ulong)track.RawBytesPerSector),
                 SeekOrigin.Begin);

        trackStream.Write(data, 0, data.Length);

        return true;
    }

    /// <inheritdoc />
    public bool SetTracks(List<Track> tracks)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(tracks == null || tracks.Count == 0)
        {
            ErrorMessage = Localization.Invalid_tracks_sent;

            return false;
        }

        if(_writingTracks != null && _writingStreams != null)
        {
            foreach(FileStream oldTrack in _writingStreams.Select(t => t.Value).Distinct())
                oldTrack.Close();
        }

        _writingTracks = new List<Track>();

        foreach(Track track in tracks.OrderBy(t => t.Sequence))
        {
            track.File = _separateTracksWriting
                             ? _writingBaseName + $"_track{track.Sequence:D2}.bin"
                             : _writingBaseName + ".bin";

            track.FileOffset = _separateTracksWriting ? 0 : track.StartSector * 2352;
            _writingTracks.Add(track);
        }

        _writingStreams = new Dictionary<uint, FileStream>();

        if(_separateTracksWriting)
        {
            foreach(Track track in _writingTracks)
            {
                _writingStreams.Add(track.Sequence,
                                    new FileStream(track.File, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                                                   FileShare.None));
            }
        }
        else
        {
            var jointStream = new FileStream(_writingBaseName + ".bin", FileMode.OpenOrCreate, FileAccess.ReadWrite,
                                             FileShare.None);

            foreach(Track track in _writingTracks)
                _writingStreams.Add(track.Sequence, jointStream);
        }

        return true;
    }

    /// <inheritdoc />
    public bool Close()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return false;
        }

        if(_separateTracksWriting)
        {
            foreach(FileStream writingStream in _writingStreams.Values)
            {
                writingStream.Flush();
                writingStream.Close();
            }
        }
        else
        {
            _writingStreams.First().Value.Flush();
            _writingStreams.First().Value.Close();
        }

        var currentSession = 0;

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
            foreach(var dumpData in from dump in DumpHardware
                                    from extent in dump.Extents.OrderBy(e => e.Start)
                                    select new
                                    {
                                        dump.Manufacturer,
                                        dump.Model,
                                        dump.Firmware,
                                        dump.Serial,
                                        Application        = dump.Software.Name,
                                        ApplicationVersion = dump.Software.Version,
                                        dump.Software.OperatingSystem,
                                        extent.Start,
                                        extent.End
                                    })
            {
                _descriptorStream.WriteLine($"REM METADATA DUMP EXTENT: {dumpData.Application} | {
                    dumpData.ApplicationVersion} | {dumpData.OperatingSystem} | {dumpData.Manufacturer} | {
                        dumpData.Model} | {dumpData.Firmware} | {dumpData.Serial} | {dumpData.Start}:{dumpData.End}");
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
            _descriptorStream.WriteLine("FILE \"{0}\" BINARY", Path.GetFileName(_writingStreams.First().Value.Name));

        Track trackZero = null;

        foreach(Track track in _writingTracks)
        {
            switch(track.Sequence)
            {
                // You should not be able to write CD-i Ready as this format, but just in case
                case 0 when _imageInfo.MediaType != MediaType.CDIREADY:
                    trackZero = track;

                    continue;
                case 1 when trackZero != null && !track.Indexes.ContainsKey(0):
                    track.StartSector = trackZero.StartSector;
                    track.Pregap      = (ulong)track.Indexes[1] - track.StartSector;

                    break;
            }

            if(track.Session > currentSession)
                _descriptorStream.WriteLine("REM SESSION {0}", ++currentSession);

            if(_separateTracksWriting)
                _descriptorStream.WriteLine("FILE \"{0}\" BINARY", Path.GetFileName(track.File));

            (byte minute, byte second, byte frame) msf = LbaToMsf(track.StartSector);
            _descriptorStream.WriteLine("  TRACK {0:D2} {1}", track.Sequence, GetTrackMode(track));

            if(_isCd)
            {
                if(_trackFlags.TryGetValue((byte)track.Sequence, out byte flagsByte))
                {
                    if(flagsByte != 0 && flagsByte != (byte)CdFlags.DataTrack)
                    {
                        var flags = (CdFlags)flagsByte;

                        _descriptorStream.WriteLine("    FLAGS{0}{1}{2}",
                                                    flags.HasFlag(CdFlags.CopyPermitted) ? " DCP" : "",
                                                    flags.HasFlag(CdFlags.FourChannel) ? " 4CH" : "",
                                                    flags.HasFlag(CdFlags.PreEmphasis) ? " PRE" : "");
                    }
                }

                if(_trackIsrcs.TryGetValue((byte)track.Sequence, out string isrc) && !string.IsNullOrWhiteSpace(isrc))
                    _descriptorStream.WriteLine("    ISRC {0}", isrc);
            }

            if(track.Pregap > 0 && _isCd)
            {
                if(track.Sequence > _writingTracks.Where(t => t.Session == track.Session).Min(t => t.Sequence))
                {
                    _descriptorStream.WriteLine("    INDEX {0:D2} {1:D2}:{2:D2}:{3:D2}", 0, msf.minute, msf.second,
                                                msf.frame);
                }

                if(track.Sequence > 1)
                    msf = LbaToMsf(track.StartSector + track.Pregap);

                _descriptorStream.WriteLine("    INDEX {0:D2} {1:D2}:{2:D2}:{3:D2}", 1, msf.minute, msf.second,
                                            msf.frame);
            }
            else
            {
                _descriptorStream.WriteLine("    INDEX {0:D2} {1:D2}:{2:D2}:{3:D2}", 1, msf.minute, msf.second,
                                            msf.frame);
            }

            if(_isCd)
            {
                foreach(KeyValuePair<ushort, int> index in track.Indexes.Where(i => i.Key > 1))
                {
                    msf = LbaToMsf((ulong)index.Value);

                    _descriptorStream.WriteLine("    INDEX {0:D2} {1:D2}:{2:D2}:{3:D2}", index.Key, msf.minute,
                                                msf.second, msf.frame);
                }
            }

            ushort lastSession = _writingTracks.Max(t => t.Session);

            if(currentSession >= lastSession)
                continue;

            Track lastTrackInSession = _writingTracks.Where(t => t.Session == currentSession).MaxBy(t => t.Sequence);

            if(track.Sequence != lastTrackInSession.Sequence)
                continue;

            msf = LbaToMsf(track.EndSector + 1);
            _descriptorStream.WriteLine("REM LEAD-OUT {0:D2}:{1:D2}:{2:D2}", msf.minute, msf.second, msf.frame);
        }

        _descriptorStream.Flush();
        _descriptorStream.Close();

        IsWriting    = false;
        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
    {
        ErrorMessage = Localization.Unsupported_feature;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        Track track =
            _writingTracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

        if(track is null)
        {
            ErrorMessage = string.Format(Localization.Cant_find_track_containing_0, sectorAddress);

            return false;
        }

        switch(tag)
        {
            case SectorTagType.CdTrackFlags when _isCd:
            {
                if(data.Length != 1)
                {
                    ErrorMessage = Localization.Incorrect_data_size_for_track_flags;

                    return false;
                }

                _trackFlags[(byte)sectorAddress] = data[0];

                return true;
            }
            case SectorTagType.CdTrackIsrc when _isCd:
            {
                if(data != null)
                    _trackIsrcs[(byte)sectorAddress] = Encoding.UTF8.GetString(data);

                return true;
            }
            default:
                ErrorMessage = string.Format(Localization.Unsupported_tag_type_0, tag);

                return false;
        }
    }

    /// <inheritdoc />
    public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag) =>
        WriteSectorTag(data, sectorAddress, tag);

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardware> dumpHardware)
    {
        DumpHardware = dumpHardware;

        return true;
    }

    /// <inheritdoc />
    public bool SetMetadata(Metadata metadata) => false;

    /// <inheritdoc />
    public bool SetImageInfo(ImageInfo imageInfo)
    {
        _discImage.Barcode            = imageInfo.MediaBarcode;
        _discImage.Comment            = imageInfo.Comments;
        _discImage.Title              = imageInfo.MediaTitle;
        _imageInfo.Application        = imageInfo.Application;
        _imageInfo.ApplicationVersion = imageInfo.ApplicationVersion;

        return true;
    }

#endregion
}