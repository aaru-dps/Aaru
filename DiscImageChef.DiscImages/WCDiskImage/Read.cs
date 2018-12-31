// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads d2f disk images.
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
// Copyright © 2018-2019 Michael Drüing
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;

namespace DiscImageChef.DiscImages
{
    public partial class WCDiskImage
    {
        public bool Open(IFilter imageFilter)
        {
            string comments = string.Empty;
            Stream stream   = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] header = new byte[32];
            stream.Read(header, 0, 32);

            IntPtr hdrPtr = Marshal.AllocHGlobal(32);
            Marshal.Copy(header, 0, hdrPtr, 32);
            WCDiskImageFileHeader fheader =
                (WCDiskImageFileHeader)Marshal.PtrToStructure(hdrPtr, typeof(WCDiskImageFileHeader));
            Marshal.FreeHGlobal(hdrPtr);
            DicConsole.DebugWriteLine("d2f plugin",
                                      "Detected WC DISK IMAGE with {0} heads, {1} tracks and {2} sectors per track.",
                                      fheader.heads, fheader.cylinders, fheader.sectorsPerTrack);

            imageInfo.Cylinders       = fheader.cylinders;
            imageInfo.SectorsPerTrack = fheader.sectorsPerTrack;
            imageInfo.SectorSize      = 512; // only 512 bytes per sector supported
            imageInfo.Heads           = fheader.heads;
            imageInfo.Sectors         = imageInfo.Heads * imageInfo.Cylinders * imageInfo.SectorsPerTrack;
            imageInfo.ImageSize       = imageInfo.Sectors                     * imageInfo.SectorSize;

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.MediaType = Geometry.GetMediaType(((ushort)imageInfo.Cylinders, (byte)imageInfo.Heads,
                                                            (ushort)imageInfo.SectorsPerTrack, 512, MediaEncoding.MFM,
                                                            false));

            /* buffer the entire disk in memory */
            for(int cyl = 0; cyl < imageInfo.Cylinders; cyl++)
            {
                for(int head = 0; head < imageInfo.Heads; head++) ReadTrack(stream, cyl, head);
            }

            /* if there are extra tracks, read them as well */
            if(fheader.extraTracks[0] == 1)
            {
                DicConsole.DebugWriteLine("d2f plugin", "Extra track 1 (head 0) present, reading");
                ReadTrack(stream, (int)imageInfo.Cylinders, 0);
            }

            if(fheader.extraTracks[1] == 1)
            {
                DicConsole.DebugWriteLine("d2f plugin", "Extra track 1 (head 1) present, reading");
                ReadTrack(stream, (int)imageInfo.Cylinders, 1);
            }

            if(fheader.extraTracks[2] == 1)
            {
                DicConsole.DebugWriteLine("d2f plugin", "Extra track 2 (head 0) present, reading");
                ReadTrack(stream, (int)imageInfo.Cylinders + 1, 0);
            }

            if(fheader.extraTracks[3] == 1)
            {
                DicConsole.DebugWriteLine("d2f plugin", "Extra track 2 (head 1) present, reading");
                ReadTrack(stream, (int)imageInfo.Cylinders + 1, 1);
            }

            /* adjust number of cylinders */
            if(fheader.extraTracks[0] == 1 || fheader.extraTracks[1] == 1) imageInfo.Cylinders++;
            if(fheader.extraTracks[2] == 1 || fheader.extraTracks[3] == 1) imageInfo.Cylinders++;

            /* read the comment and directory data if present */
            if(fheader.extraFlags.HasFlag(ExtraFlag.Comment))
            {
                DicConsole.DebugWriteLine("d2f plugin", "Comment present, reading");
                byte[] sheaderBuffer = new byte[6];
                stream.Read(sheaderBuffer, 0, 6);
                IntPtr sectPtr = Marshal.AllocHGlobal(6);
                Marshal.Copy(sheaderBuffer, 0, sectPtr, 6);
                WCDiskImageSectorHeader sheader =
                    (WCDiskImageSectorHeader)Marshal.PtrToStructure(sectPtr, typeof(WCDiskImageSectorHeader));
                Marshal.FreeHGlobal(sectPtr);

                if(sheader.flag != SectorFlag.Comment)
                    throw new InvalidDataException(string.Format("Invalid sector type '{0}' encountered",
                                                                 sheader.flag.ToString()));

                byte[] comm = new byte[sheader.crc];
                stream.Read(comm, 0, sheader.crc);
                comments += Encoding.ASCII.GetString(comm) + Environment.NewLine;
            }

            if(fheader.extraFlags.HasFlag(ExtraFlag.Directory))
            {
                DicConsole.DebugWriteLine("d2f plugin", "Directory listing present, reading");
                byte[] sheaderBuffer = new byte[6];
                stream.Read(sheaderBuffer, 0, 6);
                IntPtr sectPtr = Marshal.AllocHGlobal(6);
                Marshal.Copy(sheaderBuffer, 0, sectPtr, 6);
                WCDiskImageSectorHeader sheader =
                    (WCDiskImageSectorHeader)Marshal.PtrToStructure(sectPtr, typeof(WCDiskImageSectorHeader));
                Marshal.FreeHGlobal(sectPtr);

                if(sheader.flag != SectorFlag.Directory)
                    throw new InvalidDataException(string.Format("Invalid sector type '{0}' encountered",
                                                                 sheader.flag.ToString()));

                byte[] dir = new byte[sheader.crc];
                stream.Read(dir, 0, sheader.crc);
                comments += Encoding.ASCII.GetString(dir);
            }

            if(comments.Length > 0) imageInfo.Comments = comments;

            // save some variables for later use
            fileHeader    = fheader;
            wcImageFilter = imageFilter;
            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            int sectorNumber   = (int)(sectorAddress % imageInfo.SectorsPerTrack) + 1;
            int trackNumber    = (int)(sectorAddress / imageInfo.SectorsPerTrack);
            int headNumber     = imageInfo.Heads > 1 ? trackNumber % 2 : 0;
            int cylinderNumber = imageInfo.Heads > 1 ? trackNumber / 2 : trackNumber;

            if(badSectors[(cylinderNumber, headNumber, sectorNumber)])
            {
                DicConsole.DebugWriteLine("d2f plugin", "reading bad sector {0} ({1},{2},{3})", sectorAddress,
                                          cylinderNumber, headNumber, sectorNumber);

                /* if we have sector data, return that */
                if(sectorCache.ContainsKey((cylinderNumber, headNumber, sectorNumber)))
                    return sectorCache[(cylinderNumber, headNumber, sectorNumber)];

                /* otherwise, return an empty sector */
                return new byte[512];
            }

            return sectorCache[(cylinderNumber, headNumber, sectorNumber)];
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            byte[] result = new byte[length * imageInfo.SectorSize];

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            for(int i = 0; i < length; i++)
                ReadSector(sectorAddress + (ulong)i).CopyTo(result, i * imageInfo.SectorSize);

            return result;
        }

        /// <summary>
        ///     Read a whole track and cache it
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="cyl">The cylinder number of the track being read.</param>
        /// <param name="head">The head number of the track being read.</param>
        void ReadTrack(Stream stream, int cyl, int head)
        {
            byte[] sectorData;
            byte[] crc;
            short  calculatedCRC;

            for(int sect = 1; sect < imageInfo.SectorsPerTrack + 1; sect++)
            {
                /* read the sector header */
                byte[] sheaderBuffer = new byte[6];
                stream.Read(sheaderBuffer, 0, 6);
                IntPtr sectPtr = Marshal.AllocHGlobal(6);
                Marshal.Copy(sheaderBuffer, 0, sectPtr, 6);
                WCDiskImageSectorHeader sheader =
                    (WCDiskImageSectorHeader)Marshal.PtrToStructure(sectPtr, typeof(WCDiskImageSectorHeader));
                Marshal.FreeHGlobal(sectPtr);

                /* validate the sector header */
                if(sheader.cylinder != cyl || sheader.head != head || sheader.sector != sect)
                    throw new
                        InvalidDataException(string.Format("Unexpected sector encountered. Found CHS {0},{1},{2} but expected {3},{4},{5}",
                                                           sheader.cylinder, sheader.head, sheader.sector, cyl, head,
                                                           sect));

                sectorData = new byte[512];
                /* read the sector data */
                switch(sheader.flag)
                {
                    case SectorFlag.Normal: /* read a normal sector and store it in cache */
                        stream.Read(sectorData, 0, 512);
                        sectorCache[(cyl, head, sect)] = sectorData;
                        Crc16Context.Data(sectorData, 512, out crc);
                        calculatedCRC = (short)((256 * crc[0]) | crc[1]);
                        /*
                        DicConsole.DebugWriteLine("d2f plugin", "CHS {0},{1},{2}: Regular sector, stored CRC=0x{3:x4}, calculated CRC=0x{4:x4}",
                            cyl, head, sect, sheader.crc, 256 * crc[0] + crc[1]);
                         */
                        badSectors[(cyl, head, sect)] = sheader.crc != calculatedCRC;
                        if(calculatedCRC != sheader.crc)
                            DicConsole.DebugWriteLine("d2f plugin",
                                                      "CHS {0},{1},{2}: CRC mismatch: stored CRC=0x{3:x4}, calculated CRC=0x{4:x4}",
                                                      cyl, head, sect, sheader.crc, calculatedCRC);

                        break;
                    case SectorFlag.BadSector:
                        /*
                        DicConsole.DebugWriteLine("d2f plugin", "CHS {0},{1},{2}: Bad sector",
                            cyl, head, sect);
                         */
                        badSectors[(cyl, head, sect)] = true;

                        break;
                    case SectorFlag.RepeatByte:
                        /*
                        DicConsole.DebugWriteLine("d2f plugin", "CHS {0},{1},{2}: RepeatByte sector, fill byte 0x{0:x2}",
                            cyl, head, sect, sheader.crc & 0xff);
                         */
                        for(int i = 0; i < 512; i++) sectorData[i] = (byte)(sheader.crc & 0xff);

                        sectorCache[(cyl, head, sect)] = sectorData;
                        badSectors[(cyl, head, sect)]  = false;

                        break;
                    default:
                        throw new InvalidDataException(string.Format("Invalid sector type '{0}' encountered",
                                                                     sheader.flag.ToString()));
                }
            }
        }
    }
}