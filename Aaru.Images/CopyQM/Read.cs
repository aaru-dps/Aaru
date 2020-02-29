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
//     Reads Sydex CopyQM disk images.
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
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public partial class CopyQm
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdr = new byte[133];

            stream.Read(hdr, 0, 133);
            header = Marshal.ByteArrayToStructureLittleEndian<CopyQmHeader>(hdr);

            AaruConsole.DebugWriteLine("CopyQM plugin", "header.magic = 0x{0:X4}", header.magic);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.mark = 0x{0:X2}", header.mark);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.sectorSize = {0}", header.sectorSize);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.sectorPerCluster = {0}", header.sectorPerCluster);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.reservedSectors = {0}", header.reservedSectors);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.fatCopy = {0}", header.fatCopy);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.rootEntries = {0}", header.rootEntries);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.sectors = {0}", header.sectors);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.mediaType = 0x{0:X2}", header.mediaType);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.sectorsPerFat = {0}", header.sectorsPerFat);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.sectorsPerTrack = {0}", header.sectorsPerTrack);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.heads = {0}", header.heads);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.hidden = {0}", header.hidden);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.sectorsBig = {0}", header.sectorsBig);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.description = {0}", header.description);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.blind = {0}", header.blind);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.density = {0}", header.density);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.imageCylinders = {0}", header.imageCylinders);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.totalCylinders = {0}", header.totalCylinders);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.crc = 0x{0:X8}", header.crc);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.volumeLabel = {0}", header.volumeLabel);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.time = 0x{0:X4}", header.time);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.date = 0x{0:X4}", header.date);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.commentLength = {0}", header.commentLength);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.secbs = {0}", header.secbs);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.unknown = 0x{0:X4}", header.unknown);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.interleave = {0}", header.interleave);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.skew = {0}", header.skew);
            AaruConsole.DebugWriteLine("CopyQM plugin", "header.drive = {0}", header.drive);

            byte[] cmt = new byte[header.commentLength];
            stream.Read(cmt, 0, header.commentLength);
            imageInfo.Comments = StringHandlers.CToString(cmt);
            decodedImage       = new MemoryStream();

            calculatedDataCrc = 0;

            while(stream.Position + 2 < stream.Length)
            {
                byte[] runLengthBytes = new byte[2];

                if(stream.Read(runLengthBytes, 0, 2) != 2)
                    break;

                short runLength = BitConverter.ToInt16(runLengthBytes, 0);

                if(runLength < 0)
                {
                    byte   repeatedByte  = (byte)stream.ReadByte();
                    byte[] repeatedArray = new byte[runLength * -1];
                    ArrayHelpers.ArrayFill(repeatedArray, repeatedByte);

                    for(int i = 0; i < runLength * -1; i++)
                    {
                        decodedImage.WriteByte(repeatedByte);

                        calculatedDataCrc = copyQmCrcTable[(repeatedByte ^ calculatedDataCrc) & 0x3F] ^
                                            (calculatedDataCrc >> 8);
                    }
                }
                else if(runLength > 0)
                {
                    byte[] nonRepeated = new byte[runLength];
                    stream.Read(nonRepeated, 0, runLength);
                    decodedImage.Write(nonRepeated, 0, runLength);

                    foreach(byte c in nonRepeated)
                        calculatedDataCrc = copyQmCrcTable[(c ^ calculatedDataCrc) & 0x3F] ^ (calculatedDataCrc >> 8);
                }
            }

            // In case there is omitted data
            long sectors = header.sectorsPerTrack * header.heads * header.totalCylinders;

            long fillingLen = (sectors * header.sectorSize) - decodedImage.Length;

            if(fillingLen > 0)
            {
                byte[] filling = new byte[fillingLen];
                ArrayHelpers.ArrayFill(filling, (byte)0xF6);
                decodedImage.Write(filling, 0, filling.Length);
            }

            int sum = 0;

            for(int i = 0; i < hdr.Length - 1; i++)
                sum += hdr[i];

            headerChecksumOk = ((-1 * sum) & 0xFF) == header.headerChecksum;

            AaruConsole.DebugWriteLine("CopyQM plugin", "Calculated header checksum = 0x{0:X2}, {1}", (-1 * sum) & 0xFF,
                                       headerChecksumOk);

            AaruConsole.DebugWriteLine("CopyQM plugin", "Calculated data CRC = 0x{0:X8}, {1}", calculatedDataCrc,
                                       calculatedDataCrc == header.crc);

            imageInfo.Application          = "CopyQM";
            imageInfo.CreationTime         = DateHandlers.DosToDateTime(header.date, header.time);
            imageInfo.LastModificationTime = imageInfo.CreationTime;
            imageInfo.MediaTitle           = header.volumeLabel;
            imageInfo.ImageSize            = (ulong)(stream.Length - 133 - header.commentLength);
            imageInfo.Sectors              = (ulong)sectors;
            imageInfo.SectorSize           = header.sectorSize;

            imageInfo.MediaType = Geometry.GetMediaType(((ushort)header.totalCylinders, (byte)header.heads,
                                                         header.sectorsPerTrack, (uint)header.sectorSize,
                                                         MediaEncoding.MFM, false));

            switch(imageInfo.MediaType)
            {
                case MediaType.NEC_525_HD when header.drive == COPYQM_35_HD || header.drive == COPYQM_35_ED:
                    imageInfo.MediaType = MediaType.NEC_35_HD_8;

                    break;
                case MediaType.DOS_525_HD when header.drive == COPYQM_35_HD || header.drive == COPYQM_35_ED:
                    imageInfo.MediaType = MediaType.NEC_35_HD_15;

                    break;
                case MediaType.RX50 when header.drive == COPYQM_525_DD || header.drive == COPYQM_525_HD:
                    imageInfo.MediaType = MediaType.ATARI_35_SS_DD;

                    break;
            }

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            decodedDisk            = decodedImage.ToArray();

            decodedImage.Close();

            AaruConsole.VerboseWriteLine("CopyQM image contains a disk of type {0}", imageInfo.MediaType);

            if(!string.IsNullOrEmpty(imageInfo.Comments))
                AaruConsole.VerboseWriteLine("CopyQM comments: {0}", imageInfo.Comments);

            imageInfo.Heads           = header.heads;
            imageInfo.Cylinders       = header.totalCylinders;
            imageInfo.SectorsPerTrack = header.sectorsPerTrack;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            Array.Copy(decodedDisk, (int)sectorAddress * imageInfo.SectorSize, buffer, 0,
                       length                          * imageInfo.SectorSize);

            return buffer;
        }

        public bool? VerifyMediaImage() => calculatedDataCrc == header.crc && headerChecksumOk;
    }
}