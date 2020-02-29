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
//     Reads partimage disk images.
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public partial class Partimage
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;

            byte[] hdrB = new byte[Marshal.SizeOf<PartimageHeader>()];
            stream.Read(hdrB, 0, Marshal.SizeOf<PartimageHeader>());
            cVolumeHeader = Marshal.ByteArrayToStructureLittleEndian<PartimageHeader>(hdrB);

            AaruConsole.DebugWriteLine("Partimage plugin", "CVolumeHeader.magic = {0}",
                                       StringHandlers.CToString(cVolumeHeader.magic));

            AaruConsole.DebugWriteLine("Partimage plugin", "CVolumeHeader.version = {0}",
                                       StringHandlers.CToString(cVolumeHeader.version));

            AaruConsole.DebugWriteLine("Partimage plugin", "CVolumeHeader.volumeNumber = {0}",
                                       cVolumeHeader.volumeNumber);

            AaruConsole.DebugWriteLine("Partimage plugin", "CVolumeHeader.identificator = {0:X16}",
                                       cVolumeHeader.identificator);

            // TODO: Support multifile volumes
            if(cVolumeHeader.volumeNumber > 0)
                throw new FeatureSupportedButNotImplementedImageException("Support for multiple volumes not supported");

            hdrB = new byte[Marshal.SizeOf<PartimageMainHeader>()];
            stream.Read(hdrB, 0, Marshal.SizeOf<PartimageMainHeader>());
            cMainHeader = Marshal.ByteArrayToStructureLittleEndian<PartimageMainHeader>(hdrB);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szFileSystem = {0}",
                                       StringHandlers.CToString(cMainHeader.szFileSystem));

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szPartDescription = {0}",
                                       StringHandlers.CToString(cMainHeader.szPartDescription));

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szOriginalDevice = {0}",
                                       StringHandlers.CToString(cMainHeader.szOriginalDevice));

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szFirstImageFilepath = {0}",
                                       StringHandlers.CToString(cMainHeader.szFirstImageFilepath));

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szUnameSysname = {0}",
                                       StringHandlers.CToString(cMainHeader.szUnameSysname));

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szUnameNodename = {0}",
                                       StringHandlers.CToString(cMainHeader.szUnameNodename));

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szUnameRelease = {0}",
                                       StringHandlers.CToString(cMainHeader.szUnameRelease));

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szUnameVersion = {0}",
                                       StringHandlers.CToString(cMainHeader.szUnameVersion));

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szUnameMachine = {0}",
                                       StringHandlers.CToString(cMainHeader.szUnameMachine));

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwCompression = {0} ({1})",
                                       cMainHeader.dwCompression, (uint)cMainHeader.dwCompression);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwMainFlags = {0}", cMainHeader.dwMainFlags);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_sec = {0}",
                                       cMainHeader.dateCreate.Second);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_min = {0}",
                                       cMainHeader.dateCreate.Minute);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_hour = {0}",
                                       cMainHeader.dateCreate.Hour);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_mday = {0}",
                                       cMainHeader.dateCreate.DayOfMonth);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_mon = {0}",
                                       cMainHeader.dateCreate.Month);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_year = {0}",
                                       cMainHeader.dateCreate.Year);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_wday = {0}",
                                       cMainHeader.dateCreate.DayOfWeek);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_yday = {0}",
                                       cMainHeader.dateCreate.DayOfYear);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_isdst = {0}",
                                       cMainHeader.dateCreate.IsDst);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_gmtoffsec = {0}",
                                       cMainHeader.dateCreate.GmtOff);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_zone = {0}",
                                       cMainHeader.dateCreate.Timezone);

            var dateCreate = new DateTime(1900                              + (int)cMainHeader.dateCreate.Year,
                                          (int)cMainHeader.dateCreate.Month + 1, (int)cMainHeader.dateCreate.DayOfMonth,
                                          (int)cMainHeader.dateCreate.Hour, (int)cMainHeader.dateCreate.Minute,
                                          (int)cMainHeader.dateCreate.Second);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate = {0}", dateCreate);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.qwPartSize = {0}", cMainHeader.qwPartSize);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szHostname = {0}",
                                       StringHandlers.CToString(cMainHeader.szHostname));

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szVersion = {0}",
                                       StringHandlers.CToString(cMainHeader.szVersion));

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwMbrCount = {0}", cMainHeader.dwMbrCount);
            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwMbrSize = {0}", cMainHeader.dwMbrSize);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwEncryptAlgo = {0} ({1})",
                                       cMainHeader.dwEncryptAlgo, (uint)cMainHeader.dwEncryptAlgo);

            AaruConsole.DebugWriteLine("Partimage plugin", "ArrayIsNullOrEmpty(CMainHeader.cHashTestKey) = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(cMainHeader.cHashTestKey));

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture000 = {0}",
                                       cMainHeader.dwReservedFuture000);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture001 = {0}",
                                       cMainHeader.dwReservedFuture001);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture002 = {0}",
                                       cMainHeader.dwReservedFuture002);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture003 = {0}",
                                       cMainHeader.dwReservedFuture003);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture004 = {0}",
                                       cMainHeader.dwReservedFuture004);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture005 = {0}",
                                       cMainHeader.dwReservedFuture005);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture006 = {0}",
                                       cMainHeader.dwReservedFuture006);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture007 = {0}",
                                       cMainHeader.dwReservedFuture007);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture008 = {0}",
                                       cMainHeader.dwReservedFuture008);

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture009 = {0}",
                                       cMainHeader.dwReservedFuture009);

            AaruConsole.DebugWriteLine("Partimage plugin", "ArrayIsNullOrEmpty(CMainHeader.cReserved) = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(cMainHeader.cReserved));

            AaruConsole.DebugWriteLine("Partimage plugin", "CMainHeader.crc = 0x{0:X8}", cMainHeader.crc);

            // partimage 0.6.1 does not support them either
            if(cMainHeader.dwEncryptAlgo != PEncryption.None)
                throw new ImageNotSupportedException("Encrypted images are currently not supported.");

            string magic;

            // Skip MBRs
            if(cMainHeader.dwMbrCount > 0)
            {
                hdrB = new byte[MAGIC_BEGIN_MBRBACKUP.Length];
                stream.Read(hdrB, 0, MAGIC_BEGIN_MBRBACKUP.Length);
                magic = StringHandlers.CToString(hdrB);

                if(!magic.Equals(MAGIC_BEGIN_MBRBACKUP))
                    throw new ImageNotSupportedException("Cannot find MBRs");

                stream.Seek(cMainHeader.dwMbrSize * cMainHeader.dwMbrCount, SeekOrigin.Current);
            }

            // Skip extended headers and their CRC fields
            stream.Seek((MAGIC_BEGIN_EXT000.Length + 4) * 10, SeekOrigin.Current);

            hdrB = new byte[MAGIC_BEGIN_LOCALHEADER.Length];
            stream.Read(hdrB, 0, MAGIC_BEGIN_LOCALHEADER.Length);
            magic = StringHandlers.CToString(hdrB);

            if(!magic.Equals(MAGIC_BEGIN_LOCALHEADER))
                throw new ImageNotSupportedException("Cannot find local header");

            hdrB = new byte[Marshal.SizeOf<CLocalHeader>()];
            stream.Read(hdrB, 0, Marshal.SizeOf<CLocalHeader>());
            CLocalHeader localHeader = Marshal.ByteArrayToStructureLittleEndian<CLocalHeader>(hdrB);

            AaruConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.qwBlockSize = {0}", localHeader.qwBlockSize);
            AaruConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.qwUsedBlocks = {0}", localHeader.qwUsedBlocks);

            AaruConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.qwBlocksCount = {0}",
                                       localHeader.qwBlocksCount);

            AaruConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.qwBitmapSize = {0}", localHeader.qwBitmapSize);

            AaruConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.qwBadBlocksCount = {0}",
                                       localHeader.qwBadBlocksCount);

            AaruConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.szLabel = {0}",
                                       StringHandlers.CToString(localHeader.szLabel));

            AaruConsole.DebugWriteLine("Partimage plugin", "ArrayIsNullOrEmpty(CLocalHeader.cReserved) = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(localHeader.cReserved));

            AaruConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.crc = 0x{0:X8}", localHeader.crc);

            hdrB = new byte[MAGIC_BEGIN_BITMAP.Length];
            stream.Read(hdrB, 0, MAGIC_BEGIN_BITMAP.Length);
            magic = StringHandlers.CToString(hdrB);

            if(!magic.Equals(MAGIC_BEGIN_BITMAP))
                throw new ImageNotSupportedException("Cannot find bitmap");

            bitmap = new byte[localHeader.qwBitmapSize];
            stream.Read(bitmap, 0, (int)localHeader.qwBitmapSize);

            hdrB = new byte[MAGIC_BEGIN_INFO.Length];
            stream.Read(hdrB, 0, MAGIC_BEGIN_INFO.Length);
            magic = StringHandlers.CToString(hdrB);

            if(!magic.Equals(MAGIC_BEGIN_INFO))
                throw new ImageNotSupportedException("Cannot find info block");

            // Skip info block and its checksum
            stream.Seek(16384 + 4, SeekOrigin.Current);

            hdrB = new byte[MAGIC_BEGIN_DATABLOCKS.Length];
            stream.Read(hdrB, 0, MAGIC_BEGIN_DATABLOCKS.Length);
            magic = StringHandlers.CToString(hdrB);

            if(!magic.Equals(MAGIC_BEGIN_DATABLOCKS))
                throw new ImageNotSupportedException("Cannot find data blocks");

            dataOff = stream.Position;

            AaruConsole.DebugWriteLine("Partimage plugin", "dataOff = {0}", dataOff);

            // Seek to tail
            stream.Seek(-(Marshal.SizeOf<CMainTail>() + MAGIC_BEGIN_TAIL.Length), SeekOrigin.End);

            hdrB = new byte[MAGIC_BEGIN_TAIL.Length];
            stream.Read(hdrB, 0, MAGIC_BEGIN_TAIL.Length);
            magic = StringHandlers.CToString(hdrB);

            if(!magic.Equals(MAGIC_BEGIN_TAIL))
                throw new
                    ImageNotSupportedException("Cannot find tail. Multiple volumes are not supported or image is corrupt.");

            AaruConsole.DebugWriteLine("Partimage plugin", "Filling extents");
            DateTime start = DateTime.Now;
            extents    = new ExtentsULong();
            extentsOff = new Dictionary<ulong, ulong>();
            bool  current     = (bitmap[0] & (1 << (0 % 8))) != 0;
            ulong blockOff    = 0;
            ulong extentStart = 0;

            for(ulong i = 1; i <= localHeader.qwBlocksCount; i++)
            {
                bool next = (bitmap[i / 8] & (1 << (int)(i % 8))) != 0;

                // Flux
                if(next != current)
                    if(next)
                    {
                        extentStart = i;
                        extentsOff.Add(i, ++blockOff);
                    }
                    else
                    {
                        extents.Add(extentStart, i);
                        extentsOff.TryGetValue(extentStart, out ulong _);
                    }

                if(next && current)
                    blockOff++;

                current = next;
            }

            DateTime end = DateTime.Now;

            AaruConsole.DebugWriteLine("Partimage plugin", "Took {0} seconds to fill extents",
                                       (end - start).TotalSeconds);

            sectorCache = new Dictionary<ulong, byte[]>();

            imageInfo.CreationTime         = dateCreate;
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = localHeader.qwBlocksCount + 1;
            imageInfo.SectorSize           = (uint)localHeader.qwBlockSize;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.Version              = StringHandlers.CToString(cMainHeader.szVersion);
            imageInfo.Comments             = StringHandlers.CToString(cMainHeader.szPartDescription);

            imageInfo.ImageSize =
                (ulong)(stream.Length - (dataOff + Marshal.SizeOf<CMainTail>() + MAGIC_BEGIN_TAIL.Length));

            imageStream = stream;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if((bitmap[sectorAddress / 8] & (1 << (int)(sectorAddress % 8))) == 0)
                return new byte[imageInfo.SectorSize];

            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector))
                return sector;

            ulong blockOff = BlockOffset(sectorAddress);

            // Offset of requested sector is:
            // Start of data +
            long imageOff = dataOff +

                            // How many stored bytes to skip
                            (long)(blockOff * imageInfo.SectorSize) +

                            // How many bytes of CRC blocks to skip
                            ((long)(blockOff / (CHECK_FREQUENCY / imageInfo.SectorSize)) * Marshal.SizeOf<CCheck>());

            sector = new byte[imageInfo.SectorSize];
            imageStream.Seek(imageOff, SeekOrigin.Begin);
            imageStream.Read(sector, 0, (int)imageInfo.SectorSize);

            if(sectorCache.Count > MAX_CACHED_SECTORS)
                sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            var ms = new MemoryStream();

            bool allEmpty = true;

            for(uint i = 0; i < length; i++)
                if((bitmap[sectorAddress / 8] & (1 << (int)(sectorAddress % 8))) != 0)
                {
                    allEmpty = false;

                    break;
                }

            if(allEmpty)
                return new byte[imageInfo.SectorSize * length];

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }
    }
}