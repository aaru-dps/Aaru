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
//     Writes CloneCD disc images.
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
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using Schemas;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.DiscImages
{
    public sealed partial class CloneCd
    {
        /// <inheritdoc />
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
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

            try
            {
                _writingBaseName =
                    Path.Combine(Path.GetDirectoryName(path) ?? "", Path.GetFileNameWithoutExtension(path));

                _descriptorStream = new StreamWriter(path, false, Encoding.ASCII);

                _dataStream = new FileStream(_writingBaseName + ".img", FileMode.OpenOrCreate, FileAccess.ReadWrite,
                                             FileShare.None);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";

                return false;
            }

            _imageInfo.MediaType = mediaType;

            _trackFlags = new Dictionary<byte, byte>();

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
                    _catalog = Encoding.ASCII.GetString(data);

                    return true;
                case MediaTagType.CD_FullTOC:
                    _fullToc = data;

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

            // TODO: Implement ECC generation
            ErrorMessage = "This format requires sectors to be raw. Generating ECC is not yet implemented";

            return false;
        }

        /// <inheritdoc />
        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            // TODO: Implement ECC generation
            ErrorMessage = "This format requires sectors to be raw. Generating ECC is not yet implemented";

            return false;
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
                Tracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            if(data.Length != track.RawBytesPerSector)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            _dataStream.
                Seek((long)(track.FileOffset + ((sectorAddress - track.StartSector) * (ulong)track.RawBytesPerSector)),
                     SeekOrigin.Begin);

            _dataStream.Write(data, 0, data.Length);

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
                Tracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

            if(track is null)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            if(sectorAddress + length > track.EndSector + 1)
            {
                ErrorMessage = "Can't cross tracks";

                return false;
            }

            if(data.Length % track.RawBytesPerSector != 0)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            _dataStream.
                Seek((long)(track.FileOffset + ((sectorAddress - track.StartSector) * (ulong)track.RawBytesPerSector)),
                     SeekOrigin.Begin);

            _dataStream.Write(data, 0, data.Length);

            return true;
        }

        /// <inheritdoc />
        public bool SetTracks(List<Track> tracks)
        {
            ulong currentDataOffset       = 0;
            ulong currentSubchannelOffset = 0;

            Tracks = new List<Track>();

            foreach(Track track in tracks.OrderBy(t => t.Sequence))
            {
                Track newTrack = track;

                if(newTrack.Session > 1)
                {
                    Track firstSessionTrack = tracks.FirstOrDefault(t => t.Session == newTrack.Session);

                    if(firstSessionTrack?.Sequence == newTrack.Sequence &&
                       newTrack.Pregap             >= 150)
                    {
                        newTrack.Pregap      -= 150;
                        newTrack.StartSector += 150;
                    }
                }

                uint subchannelSize;

                switch(newTrack.SubchannelType)
                {
                    case TrackSubchannelType.None:
                        subchannelSize = 0;

                        break;
                    case TrackSubchannelType.Raw:
                    case TrackSubchannelType.RawInterleaved:
                        subchannelSize = 96;

                        break;
                    default:
                        ErrorMessage = $"Unsupported subchannel type {newTrack.SubchannelType}";

                        return false;
                }

                newTrack.FileOffset       = currentDataOffset;
                newTrack.SubchannelOffset = currentSubchannelOffset;

                currentDataOffset += (ulong)newTrack.RawBytesPerSector *
                                     (newTrack.EndSector - newTrack.StartSector + 1);

                currentSubchannelOffset += subchannelSize * (newTrack.EndSector - newTrack.StartSector + 1);

                Tracks.Add(newTrack);
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

            _dataStream.Flush();
            _dataStream.Close();

            _subStream?.Flush();
            _subStream?.Close();

            FullTOC.CDFullTOC? nullableToc = null;
            FullTOC.CDFullTOC  toc;

            // Easy, just decode the real toc
            if(_fullToc != null)
            {
                byte[] tmp = new byte[_fullToc.Length + 2];
                Array.Copy(BigEndianBitConverter.GetBytes((ushort)_fullToc.Length), 0, tmp, 0, 2);
                Array.Copy(_fullToc, 0, tmp, 2, _fullToc.Length);
                nullableToc = FullTOC.Decode(tmp);
            }

            // Not easy, create a toc from scratch
            toc = nullableToc ?? FullTOC.Create(Tracks, _trackFlags, true);

            _descriptorStream.WriteLine("[CloneCD]");
            _descriptorStream.WriteLine("Version=2");
            _descriptorStream.WriteLine("[Disc]");
            _descriptorStream.WriteLine("TocEntries={0}", toc.TrackDescriptors.Length);
            _descriptorStream.WriteLine("Sessions={0}", toc.LastCompleteSession);
            _descriptorStream.WriteLine("DataTracksScrambled=0");
            _descriptorStream.WriteLine("CDTextLength=0");

            if(!string.IsNullOrEmpty(_catalog))
                _descriptorStream.WriteLine("CATALOG={0}", _catalog);

            for(int i = 1; i <= toc.LastCompleteSession; i++)
            {
                _descriptorStream.WriteLine("[Session {0}]", i);

                Track firstSessionTrack = Tracks.FirstOrDefault(t => t.Session == i);

                switch(firstSessionTrack?.Type)
                {
                    case TrackType.Audio:
                        // CloneCD always writes this value for first track in disc, however the Rainbow Books
                        // say the first track pregap is no different from other session pregaps, same mode as
                        // the track they belong to.
                        _descriptorStream.WriteLine("PreGapMode=0");

                        break;
                    case TrackType.Data:
                    case TrackType.CdMode1:
                        _descriptorStream.WriteLine("PreGapMode=1");

                        break;
                    case TrackType.CdMode2Formless:
                    case TrackType.CdMode2Form1:
                    case TrackType.CdMode2Form2:
                        _descriptorStream.WriteLine("PreGapMode=2");

                        break;
                    default:
                        ErrorMessage =
                            $"Unexpected first session track type {firstSessionTrack?.Type.ToString() ?? "null"}";

                        return false;
                }

                _descriptorStream.WriteLine("PreGapSubC=0");
            }

            for(int i = 0; i < toc.TrackDescriptors.Length; i++)
            {
                long alba = MsfToLba((toc.TrackDescriptors[i].Min, toc.TrackDescriptors[i].Sec,
                                      toc.TrackDescriptors[i].Frame));

                long plba = MsfToLba((toc.TrackDescriptors[i].PMIN, toc.TrackDescriptors[i].PSEC,
                                      toc.TrackDescriptors[i].PFRAME));

                if(alba > 405000)
                    alba = (alba - 405000 + 300) * -1;

                if(plba > 405000)
                    plba = (plba - 405000 + 300) * -1;

                _descriptorStream.WriteLine("[Entry {0}]", i);
                _descriptorStream.WriteLine("Session={0}", toc.TrackDescriptors[i].SessionNumber);
                _descriptorStream.WriteLine("Point=0x{0:x2}", toc.TrackDescriptors[i].POINT);
                _descriptorStream.WriteLine("ADR=0x{0:x2}", toc.TrackDescriptors[i].ADR);
                _descriptorStream.WriteLine("Control=0x{0:x2}", toc.TrackDescriptors[i].CONTROL);
                _descriptorStream.WriteLine("TrackNo={0}", toc.TrackDescriptors[i].TNO);
                _descriptorStream.WriteLine("AMin={0}", toc.TrackDescriptors[i].Min);
                _descriptorStream.WriteLine("ASec={0}", toc.TrackDescriptors[i].Sec);
                _descriptorStream.WriteLine("AFrame={0}", toc.TrackDescriptors[i].Frame);
                _descriptorStream.WriteLine("ALBA={0}", alba);

                _descriptorStream.WriteLine("Zero={0}",
                                            ((toc.TrackDescriptors[i].HOUR & 0x0F) << 4) +
                                            (toc.TrackDescriptors[i].PHOUR & 0x0F));

                _descriptorStream.WriteLine("PMin={0}", toc.TrackDescriptors[i].PMIN);
                _descriptorStream.WriteLine("PSec={0}", toc.TrackDescriptors[i].PSEC);
                _descriptorStream.WriteLine("PFrame={0}", toc.TrackDescriptors[i].PFRAME);
                _descriptorStream.WriteLine("PLBA={0}", plba);
            }

            _descriptorStream.Flush();
            _descriptorStream.Close();

            IsWriting    = false;
            ErrorMessage = "";

            return true;
        }

        /// <inheritdoc />
        public bool SetMetadata(ImageInfo metadata) => true;

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
                Tracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

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
                case SectorTagType.CdSectorSubchannel:
                {
                    if(track.SubchannelType == 0)
                    {
                        ErrorMessage =
                            $"Trying to write subchannel to track {track.Sequence}, that does not have subchannel";

                        return false;
                    }

                    if(data.Length != 96)
                    {
                        ErrorMessage = "Incorrect data size for subchannel";

                        return false;
                    }

                    if(_subStream == null)
                        try
                        {
                            _subStream = new FileStream(_writingBaseName + ".sub", FileMode.OpenOrCreate,
                                                        FileAccess.ReadWrite, FileShare.None);
                        }
                        catch(IOException e)
                        {
                            ErrorMessage = $"Could not create subchannel file, exception {e.Message}";

                            return false;
                        }

                    _subStream.Seek((long)(track.SubchannelOffset + ((sectorAddress - track.StartSector) * 96)),
                                    SeekOrigin.Begin);

                    _subStream.Write(Subchannel.Deinterleave(data), 0, data.Length);

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
                Tracks.FirstOrDefault(trk => sectorAddress >= trk.StartSector && sectorAddress <= trk.EndSector);

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
                    if(track.SubchannelType == 0)
                    {
                        ErrorMessage =
                            $"Trying to write subchannel to track {track.Sequence}, that does not have subchannel";

                        return false;
                    }

                    if(data.Length % 96 != 0)
                    {
                        ErrorMessage = "Incorrect data size for subchannel";

                        return false;
                    }

                    if(_subStream == null)
                        try
                        {
                            _subStream = new FileStream(_writingBaseName + ".sub", FileMode.OpenOrCreate,
                                                        FileAccess.ReadWrite, FileShare.None);
                        }
                        catch(IOException e)
                        {
                            ErrorMessage = $"Could not create subchannel file, exception {e.Message}";

                            return false;
                        }

                    _subStream.Seek((long)(track.SubchannelOffset + ((sectorAddress - track.StartSector) * 96)),
                                    SeekOrigin.Begin);

                    _subStream.Write(Subchannel.Deinterleave(data), 0, data.Length);

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