// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads CPCEMU disk images.
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
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Decoders.Floppy;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public partial class Cpcdsk
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;

            byte[] headerB = new byte[256];
            stream.Read(headerB, 0, 256);
            CpcDiskInfo header = Marshal.ByteArrayToStructureLittleEndian<CpcDiskInfo>(headerB);

            if(!cpcdskId.SequenceEqual(header.magic.Take(cpcdskId.Length)) &&
               !edskId.SequenceEqual(header.magic)   &&
               !du54Id.SequenceEqual(header.magic))
                return false;

            extended = edskId.SequenceEqual(header.magic);
            DicConsole.DebugWriteLine("CPCDSK plugin", "Extended = {0}", extended);

            DicConsole.DebugWriteLine("CPCDSK plugin", "header.magic = \"{0}\"",
                                      StringHandlers.CToString(header.magic));

            DicConsole.DebugWriteLine("CPCDSK plugin", "header.magic2 = \"{0}\"",
                                      StringHandlers.CToString(header.magic2));

            DicConsole.DebugWriteLine("CPCDSK plugin", "header.creator = \"{0}\"",
                                      StringHandlers.CToString(header.creator));

            DicConsole.DebugWriteLine("CPCDSK plugin", "header.tracks = {0}", header.tracks);
            DicConsole.DebugWriteLine("CPCDSK plugin", "header.sides = {0}", header.sides);

            if(!extended)
                DicConsole.DebugWriteLine("CPCDSK plugin", "header.tracksize = {0}", header.tracksize);
            else
                for(int i = 0; i < header.tracks; i++)
                {
                    for(int j = 0; j < header.sides; j++)
                        DicConsole.DebugWriteLine("CPCDSK plugin", "Track {0} Side {1} size = {2}", i, j,
                                                  header.tracksizeTable[(i * header.sides) + j] * 256);
                }

            ulong currentSector = 0;
            sectors      = new Dictionary<ulong, byte[]>();
            addressMarks = new Dictionary<ulong, byte[]>();
            ulong readtracks        = 0;
            bool  allTracksSameSize = true;
            ulong sectorsPerTrack   = 0;

            // Seek to first track descriptor
            stream.Seek(256, SeekOrigin.Begin);

            for(int i = 0; i < header.tracks; i++)
            {
                for(int j = 0; j < header.sides; j++)
                {
                    // Track not stored in image
                    if(extended && header.tracksizeTable[(i * header.sides) + j] == 0)
                        continue;

                    long trackPos = stream.Position;

                    byte[] trackB = new byte[256];
                    stream.Read(trackB, 0, 256);
                    CpcTrackInfo trackInfo = Marshal.ByteArrayToStructureLittleEndian<CpcTrackInfo>(trackB);

                    if(!trackId.SequenceEqual(trackInfo.magic))
                    {
                        DicConsole.ErrorWriteLine("Not the expected track info.");

                        return false;
                    }

                    DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].magic = \"{0}\"",
                                              StringHandlers.CToString(trackInfo.magic), i, j);

                    DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].bps = {0}",
                                              SizeCodeToBytes(trackInfo.bps), i, j);

                    DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].dataRate = {0}", trackInfo.dataRate,
                                              i, j);

                    DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].filler = 0x{0:X2}", trackInfo.filler,
                                              i, j);

                    DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].gap3 = 0x{0:X2}", trackInfo.gap3, i,
                                              j);

                    DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].padding = {0}", trackInfo.padding, i,
                                              j);

                    DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].recordingMode = {0}",
                                              trackInfo.recordingMode, i, j);

                    DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].sectors = {0}", trackInfo.sectors, i,
                                              j);

                    DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].side = {0}", trackInfo.side, i, j);
                    DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].track = {0}", trackInfo.track, i, j);

                    if(trackInfo.sectors != sectorsPerTrack)
                        if(sectorsPerTrack == 0)
                            sectorsPerTrack = trackInfo.sectors;
                        else
                            allTracksSameSize = false;

                    byte[][] thisTrackSectors      = new byte[trackInfo.sectors][];
                    byte[][] thisTrackAddressMarks = new byte[trackInfo.sectors][];

                    for(int k = 1; k <= trackInfo.sectors; k++)
                    {
                        DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].sector[{3}].id = 0x{0:X2}",
                                                  trackInfo.sectorsInfo[k - 1].id, i, j, k);

                        DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].sector[{3}].len = {0}",
                                                  trackInfo.sectorsInfo[k - 1].len, i, j, k);

                        DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].sector[{3}].side = {0}",
                                                  trackInfo.sectorsInfo[k - 1].side, i, j, k);

                        DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].sector[{3}].size = {0}",
                                                  SizeCodeToBytes(trackInfo.sectorsInfo[k - 1].size), i, j, k);

                        DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].sector[{3}].st1 = 0x{0:X2}",
                                                  trackInfo.sectorsInfo[k - 1].st1, i, j, k);

                        DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].sector[{3}].st2 = 0x{0:X2}",
                                                  trackInfo.sectorsInfo[k - 1].st2, i, j, k);

                        DicConsole.DebugWriteLine("CPCDSK plugin", "trackInfo[{1}:{2}].sector[{3}].track = {0}",
                                                  trackInfo.sectorsInfo[k - 1].track, i, j, k);

                        int sectLen = extended ? trackInfo.sectorsInfo[k - 1].len
                                          : SizeCodeToBytes(trackInfo.sectorsInfo[k - 1].size);

                        byte[] sector = new byte[sectLen];
                        stream.Read(sector, 0, sectLen);

                        if(sectLen < SizeCodeToBytes(trackInfo.sectorsInfo[k - 1].size))
                        {
                            byte[] temp = new byte[SizeCodeToBytes(trackInfo.sectorsInfo[k - 1].size)];
                            Array.Copy(sector, 0, temp, 0, sector.Length);
                            sector = temp;
                        }
                        else if(sectLen > SizeCodeToBytes(trackInfo.sectorsInfo[k - 1].size))
                        {
                            byte[] temp = new byte[SizeCodeToBytes(trackInfo.sectorsInfo[k - 1].size)];
                            Array.Copy(sector, 0, temp, 0, temp.Length);
                            sector = temp;
                        }

                        thisTrackSectors[(trackInfo.sectorsInfo[k - 1].id & 0x3F) - 1] = sector;

                        byte[] amForCrc = new byte[8];
                        amForCrc[0] = 0xA1;
                        amForCrc[1] = 0xA1;
                        amForCrc[2] = 0xA1;
                        amForCrc[3] = (byte)IBMIdType.AddressMark;
                        amForCrc[4] = trackInfo.sectorsInfo[k - 1].track;
                        amForCrc[5] = trackInfo.sectorsInfo[k - 1].side;
                        amForCrc[6] = trackInfo.sectorsInfo[k - 1].id;
                        amForCrc[7] = (byte)trackInfo.sectorsInfo[k - 1].size;

                        CRC16IBMContext.Data(amForCrc, 8, out byte[] amCrc);

                        byte[] addressMark = new byte[22];
                        Array.Copy(amForCrc, 0, addressMark, 12, 8);
                        Array.Copy(amCrc, 0, addressMark, 20, 2);

                        thisTrackAddressMarks[(trackInfo.sectorsInfo[k - 1].id & 0x3F) - 1] = addressMark;
                    }

                    for(int s = 0; s < thisTrackSectors.Length; s++)
                    {
                        sectors.Add(currentSector, thisTrackSectors[s]);
                        addressMarks.Add(currentSector, thisTrackAddressMarks[s]);
                        currentSector++;

                        if(thisTrackSectors[s].Length > imageInfo.SectorSize)
                            imageInfo.SectorSize = (uint)thisTrackSectors[s].Length;
                    }

                    stream.Seek(trackPos, SeekOrigin.Begin);

                    if(extended)
                    {
                        stream.Seek(header.tracksizeTable[(i * header.sides) + j] * 256, SeekOrigin.Current);
                        imageInfo.ImageSize += (ulong)(header.tracksizeTable[(i * header.sides) + j] * 256) - 256;
                    }
                    else
                    {
                        stream.Seek(header.tracksize, SeekOrigin.Current);
                        imageInfo.ImageSize += (ulong)header.tracksize - 256;
                    }

                    readtracks++;
                }
            }

            DicConsole.DebugWriteLine("CPCDSK plugin", "Read {0} sectors", sectors.Count);
            DicConsole.DebugWriteLine("CPCDSK plugin", "Read {0} tracks", readtracks);
            DicConsole.DebugWriteLine("CPCDSK plugin", "All tracks are same size? {0}", allTracksSameSize);

            imageInfo.Application          = StringHandlers.CToString(header.creator);
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = (ulong)sectors.Count;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.MediaType            = MediaType.CompactFloppy;
            imageInfo.ReadableSectorTags.Add(SectorTagType.FloppyAddressMark);

            // Debug writing full disk as raw
            /*
            FileStream foo = new FileStream(Path.GetFileNameWithoutExtension(imageFilter.GetFilename()) + ".bin", FileMode.Create);
            for(ulong i = 0; i < (ulong)sectors.Count; i++)
            {
                byte[] foob;
                sectors.TryGetValue(i, out foob);
                foo.Write(foob, 0, foob.Length);
            }
            foo.Close();
            */

            imageInfo.Cylinders       = header.tracks;
            imageInfo.Heads           = header.sides;
            imageInfo.SectorsPerTrack = (uint)(imageInfo.Sectors / (imageInfo.Cylinders * imageInfo.Heads));

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectors.TryGetValue(sectorAddress, out byte[] sector))
                return sector;

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            if(tag != SectorTagType.FloppyAddressMark)
                throw new FeatureUnsupportedImageException($"Tag {tag} not supported by image format");

            if(addressMarks.TryGetValue(sectorAddress, out byte[] addressMark))
                return addressMark;

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(tag != SectorTagType.FloppyAddressMark)
                throw new FeatureUnsupportedImageException($"Tag {tag} not supported by image format");

            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] adddressMark = ReadSector(sectorAddress + i);
                ms.Write(adddressMark, 0, adddressMark.Length);
            }

            return ms.ToArray();
        }
    }
}