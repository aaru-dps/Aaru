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
using Aaru.Decoders.CD;
using Schemas;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.DiscImages
{
    public partial class CloneCd
    {
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";

                return false;
            }

            imageInfo = new ImageInfo
            {
                MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors
            };

            try
            {
                writingBaseName  = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                descriptorStream = new StreamWriter(path, false, Encoding.ASCII);

                dataStream = new FileStream(writingBaseName + ".img", FileMode.OpenOrCreate, FileAccess.ReadWrite,
                                            FileShare.None);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";

                return false;
            }

            imageInfo.MediaType = mediaType;

            trackFlags = new Dictionary<byte, byte>();

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
                    catalog = Encoding.ASCII.GetString(data);

                    return true;
                case MediaTagType.CD_FullTOC:
                    fulltoc = data;

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

            // TODO: Implement ECC generation
            ErrorMessage = "This format requires sectors to be raw. Generating ECC is not yet implemented";

            return false;
        }

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

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                Tracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                             sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

                return false;
            }

            if(data.Length != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            dataStream.Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector)),
                            SeekOrigin.Begin);

            dataStream.Write(data, 0, data.Length);

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
                Tracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                             sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";

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

            dataStream.Seek((long)(track.TrackFileOffset + ((sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector)),
                            SeekOrigin.Begin);

            dataStream.Write(data, 0, data.Length);

            return true;
        }

        public bool SetTracks(List<Track> tracks)
        {
            ulong currentDataOffset       = 0;
            ulong currentSubchannelOffset = 0;

            Tracks = new List<Track>();

            foreach(Track track in tracks.OrderBy(t => t.TrackSequence))
            {
                Track newTrack = track;

                if(newTrack.TrackSession > 1)
                {
                    Track firstSessionTrack = tracks.FirstOrDefault(t => t.TrackSession == newTrack.TrackSession);

                    if(firstSessionTrack.TrackSequence == newTrack.TrackSequence &&
                       newTrack.TrackPregap            >= 150)
                    {
                        newTrack.TrackPregap      -= 150;
                        newTrack.TrackStartSector += 150;
                    }
                }

                uint subchannelSize;

                switch(newTrack.TrackSubchannelType)
                {
                    case TrackSubchannelType.None:
                        subchannelSize = 0;

                        break;
                    case TrackSubchannelType.Raw:
                    case TrackSubchannelType.RawInterleaved:
                        subchannelSize = 96;

                        break;
                    default:
                        ErrorMessage = $"Unsupported subchannel type {newTrack.TrackSubchannelType}";

                        return false;
                }

                newTrack.TrackFileOffset       = currentDataOffset;
                newTrack.TrackSubchannelOffset = currentSubchannelOffset;

                currentDataOffset += (ulong)newTrack.TrackRawBytesPerSector *
                                     ((newTrack.TrackEndSector - newTrack.TrackStartSector) + 1);

                currentSubchannelOffset += subchannelSize * ((newTrack.TrackEndSector - newTrack.TrackStartSector) + 1);

                Tracks.Add(newTrack);
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

            dataStream.Flush();
            dataStream.Close();

            subStream?.Flush();
            subStream?.Close();

            FullTOC.CDFullTOC? nullableToc = null;
            FullTOC.CDFullTOC  toc;

            // Easy, just decode the real toc
            if(fulltoc != null)
            {
                byte[] tmp = new byte[fulltoc.Length + 2];
                Array.Copy(BigEndianBitConverter.GetBytes((ushort)fulltoc.Length), 0, tmp, 0, 2);
                Array.Copy(fulltoc, 0, tmp, 2, fulltoc.Length);
                nullableToc = FullTOC.Decode(tmp);
            }

            // Not easy, create a toc from scratch
            if(nullableToc == null)
            {
                toc = new FullTOC.CDFullTOC();
                Dictionary<byte, byte> sessionEndingTrack = new Dictionary<byte, byte>();
                toc.FirstCompleteSession = byte.MaxValue;
                toc.LastCompleteSession  = byte.MinValue;
                List<FullTOC.TrackDataDescriptor> trackDescriptors = new List<FullTOC.TrackDataDescriptor>();
                byte                              currentTrack     = 0;

                foreach(Track track in Tracks.OrderBy(t => t.TrackSession).ThenBy(t => t.TrackSequence))
                {
                    if(track.TrackSession < toc.FirstCompleteSession)
                        toc.FirstCompleteSession = (byte)track.TrackSession;

                    if(track.TrackSession <= toc.LastCompleteSession)
                    {
                        currentTrack = (byte)track.TrackSequence;

                        continue;
                    }

                    if(toc.LastCompleteSession > 0)
                        sessionEndingTrack.Add(toc.LastCompleteSession, currentTrack);

                    toc.LastCompleteSession = (byte)track.TrackSession;
                }

                byte currentSession = 0;

                foreach(Track track in Tracks.OrderBy(t => t.TrackSession).ThenBy(t => t.TrackSequence))
                {
                    trackFlags.TryGetValue((byte)track.TrackSequence, out byte trackControl);

                    if(trackControl    == 0 &&
                       track.TrackType != TrackType.Audio)
                        trackControl = (byte)CdFlags.DataTrack;

                    // Lead-Out
                    if(track.TrackSession > currentSession &&
                       currentSession     != 0)
                    {
                        (byte minute, byte second, byte frame) leadoutAmsf = LbaToMsf(track.TrackStartSector - 150);

                        (byte minute, byte second, byte frame) leadoutPmsf =
                            LbaToMsf(Tracks.OrderBy(t => t.TrackSession).ThenBy(t => t.TrackSequence).Last().
                                            TrackStartSector);

                        // Lead-out
                        trackDescriptors.Add(new FullTOC.TrackDataDescriptor
                        {
                            SessionNumber = currentSession, POINT = 0xB0, ADR = 5, CONTROL = 0,
                            HOUR = 0, Min = leadoutAmsf.minute, Sec = leadoutAmsf.second, Frame = leadoutAmsf.frame,
                            PHOUR = 2, PMIN = leadoutPmsf.minute, PSEC = leadoutPmsf.second, PFRAME = leadoutPmsf.frame
                        });

                        // This seems to be constant? It should not exist on CD-ROM but CloneCD creates them anyway
                        // Format seems like ATIP, but ATIP should not be as 0xC0 in TOC...
                        trackDescriptors.Add(new FullTOC.TrackDataDescriptor
                        {
                            SessionNumber = currentSession, POINT = 0xC0, ADR = 5, CONTROL = 0,
                            Min           = 128, PMIN             = 97, PSEC  = 25
                        });
                    }

                    // Lead-in
                    if(track.TrackSession > currentSession)
                    {
                        currentSession = (byte)track.TrackSession;
                        sessionEndingTrack.TryGetValue(currentSession, out byte endingTrackNumber);

                        (byte minute, byte second, byte frame) leadinPmsf =
                            LbaToMsf(Tracks.FirstOrDefault(t => t.TrackSequence == endingTrackNumber).TrackEndSector +
                                     1);

                        // Starting track
                        trackDescriptors.Add(new FullTOC.TrackDataDescriptor
                        {
                            SessionNumber = currentSession, POINT = 0xA0, ADR = 1, CONTROL = trackControl,
                            PMIN          = (byte)track.TrackSequence
                        });

                        // Ending track
                        trackDescriptors.Add(new FullTOC.TrackDataDescriptor
                        {
                            SessionNumber = currentSession, POINT = 0xA1, ADR = 1, CONTROL = trackControl,
                            PMIN          = endingTrackNumber
                        });

                        // Lead-out start
                        trackDescriptors.Add(new FullTOC.TrackDataDescriptor
                        {
                            SessionNumber = currentSession, POINT = 0xA2, ADR = 1, CONTROL = trackControl,
                            PHOUR = 0, PMIN = leadinPmsf.minute, PSEC = leadinPmsf.second, PFRAME = leadinPmsf.frame
                        });
                    }

                    (byte minute, byte second, byte frame) pmsf = LbaToMsf(track.TrackStartSector);

                    // Track
                    trackDescriptors.Add(new FullTOC.TrackDataDescriptor
                    {
                        SessionNumber = (byte)track.TrackSession, POINT = (byte)track.TrackSequence, ADR = 1,
                        CONTROL       = trackControl, PHOUR             = 0, PMIN = pmsf.minute, PSEC = pmsf.second,
                        PFRAME        = pmsf.frame
                    });
                }

                toc.TrackDescriptors = trackDescriptors.ToArray();
            }
            else
                toc = nullableToc.Value;

            descriptorStream.WriteLine("[CloneCD]");
            descriptorStream.WriteLine("Version=2");
            descriptorStream.WriteLine("[Disc]");
            descriptorStream.WriteLine("TocEntries={0}", toc.TrackDescriptors.Length);
            descriptorStream.WriteLine("Sessions={0}", toc.LastCompleteSession);
            descriptorStream.WriteLine("DataTracksScrambled=0");
            descriptorStream.WriteLine("CDTextLength=0");

            if(!string.IsNullOrEmpty(catalog))
                descriptorStream.WriteLine("CATALOG={0}", catalog);

            for(int i = 1; i <= toc.LastCompleteSession; i++)
            {
                descriptorStream.WriteLine("[Session {0}]", i);

                Track firstSessionTrack = Tracks.FirstOrDefault(t => t.TrackSession == i);

                switch(firstSessionTrack.TrackType)
                {
                    case TrackType.Audio:
                        // CloneCD always writes this value for first track in disc, however the Rainbow Books
                        // say the first track pregap is no different from other session pregaps, same mode as
                        // the track they belong to.
                        descriptorStream.WriteLine("PreGapMode=0");

                        break;
                    case TrackType.Data:
                    case TrackType.CdMode1:
                        descriptorStream.WriteLine("PreGapMode=1");

                        break;
                    case TrackType.CdMode2Formless:
                    case TrackType.CdMode2Form1:
                    case TrackType.CdMode2Form2:
                        descriptorStream.WriteLine("PreGapMode=2");

                        break;
                    default: throw new ArgumentOutOfRangeException();
                }

                descriptorStream.WriteLine("PreGapSubC=0");
            }

            for(int i = 0; i < toc.TrackDescriptors.Length; i++)
            {
                long alba = MsfToLba((toc.TrackDescriptors[i].Min, toc.TrackDescriptors[i].Sec,
                                      toc.TrackDescriptors[i].Frame));

                long plba = MsfToLba((toc.TrackDescriptors[i].PMIN, toc.TrackDescriptors[i].PSEC,
                                      toc.TrackDescriptors[i].PFRAME));

                if(alba > 405000)
                    alba = ((alba - 405000) + 300) * -1;

                if(plba > 405000)
                    plba = ((plba - 405000) + 300) * -1;

                descriptorStream.WriteLine("[Entry {0}]", i);
                descriptorStream.WriteLine("Session={0}", toc.TrackDescriptors[i].SessionNumber);
                descriptorStream.WriteLine("Point=0x{0:x2}", toc.TrackDescriptors[i].POINT);
                descriptorStream.WriteLine("ADR=0x{0:x2}", toc.TrackDescriptors[i].ADR);
                descriptorStream.WriteLine("Control=0x{0:x2}", toc.TrackDescriptors[i].CONTROL);
                descriptorStream.WriteLine("TrackNo={0}", toc.TrackDescriptors[i].TNO);
                descriptorStream.WriteLine("AMin={0}", toc.TrackDescriptors[i].Min);
                descriptorStream.WriteLine("ASec={0}", toc.TrackDescriptors[i].Sec);
                descriptorStream.WriteLine("AFrame={0}", toc.TrackDescriptors[i].Frame);
                descriptorStream.WriteLine("ALBA={0}", alba);

                descriptorStream.WriteLine("Zero={0}",
                                           ((toc.TrackDescriptors[i].HOUR & 0x0F) << 4) +
                                           (toc.TrackDescriptors[i].PHOUR & 0x0F));

                descriptorStream.WriteLine("PMin={0}", toc.TrackDescriptors[i].PMIN);
                descriptorStream.WriteLine("PSec={0}", toc.TrackDescriptors[i].PSEC);
                descriptorStream.WriteLine("PFrame={0}", toc.TrackDescriptors[i].PFRAME);
                descriptorStream.WriteLine("PLBA={0}", plba);
            }

            descriptorStream.Flush();
            descriptorStream.Close();

            IsWriting    = false;
            ErrorMessage = "";

            return true;
        }

        public bool SetMetadata(ImageInfo metadata) => true;

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
                Tracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
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

                    trackFlags.Add((byte)sectorAddress, data[0]);

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

                    if(subStream == null)
                        try
                        {
                            subStream = new FileStream(writingBaseName + ".sub", FileMode.OpenOrCreate,
                                                       FileAccess.ReadWrite, FileShare.None);
                        }
                        catch(IOException e)
                        {
                            ErrorMessage = $"Could not create subchannel file, exception {e.Message}";

                            return false;
                        }

                    subStream.Seek((long)(track.TrackSubchannelOffset + ((sectorAddress - track.TrackStartSector) * 96)),
                                   SeekOrigin.Begin);

                    subStream.Write(Subchannel.Deinterleave(data), 0, data.Length);

                    return true;
                }
                default:
                    ErrorMessage = $"Unsupported tag type {tag}";

                    return false;
            }
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            Track track =
                Tracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                             sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
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

                    if(subStream == null)
                        try
                        {
                            subStream = new FileStream(writingBaseName + ".sub", FileMode.OpenOrCreate,
                                                       FileAccess.ReadWrite, FileShare.None);
                        }
                        catch(IOException e)
                        {
                            ErrorMessage = $"Could not create subchannel file, exception {e.Message}";

                            return false;
                        }

                    subStream.Seek((long)(track.TrackSubchannelOffset + ((sectorAddress - track.TrackStartSector) * 96)),
                                   SeekOrigin.Begin);

                    subStream.Write(Subchannel.Deinterleave(data), 0, data.Length);

                    return true;
                }
                default:
                    ErrorMessage = $"Unsupported tag type {tag}";

                    return false;
            }
        }

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

        public bool SetCicmMetadata(CICMMetadataType metadata) => false;
    }
}