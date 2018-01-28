// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Partimage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages partimage disk images.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using Extents;
using Schemas;

#pragma warning disable 649

namespace DiscImageChef.DiscImages
{
    public class Partimage : IMediaImage
    {
        const int    MAX_DESCRIPTION         = 4096;
        const int    MAX_HOSTNAMESIZE        = 128;
        const int    MAX_DEVICENAMELEN       = 512;
        const int    MAX_UNAMEINFOLEN        = 65; //SYS_NMLN
        const int    MBR_SIZE_WHOLE          = 512;
        const int    MAX_DESC_MODEL          = 128;
        const int    MAX_DESC_GEOMETRY       = 1024;
        const int    MAX_DESC_IDENTIFY       = 4096;
        const int    CHECK_FREQUENCY         = 65536;
        const string MAGIC_BEGIN_LOCALHEADER = "MAGIC-BEGIN-LOCALHEADER";
        const string MAGIC_BEGIN_DATABLOCKS  = "MAGIC-BEGIN-DATABLOCKS";
        const string MAGIC_BEGIN_BITMAP      = "MAGIC-BEGIN-BITMAP";
        const string MAGIC_BEGIN_MBRBACKUP   = "MAGIC-BEGIN-MBRBACKUP";
        const string MAGIC_BEGIN_TAIL        = "MAGIC-BEGIN-TAIL";
        const string MAGIC_BEGIN_INFO        = "MAGIC-BEGIN-INFO";
        const string MAGIC_BEGIN_EXT000      = "MAGIC-BEGIN-EXT000"; // reserved for future use
        const string MAGIC_BEGIN_EXT001      = "MAGIC-BEGIN-EXT001"; // reserved for future use
        const string MAGIC_BEGIN_EXT002      = "MAGIC-BEGIN-EXT002"; // reserved for future use
        const string MAGIC_BEGIN_EXT003      = "MAGIC-BEGIN-EXT003"; // reserved for future use
        const string MAGIC_BEGIN_EXT004      = "MAGIC-BEGIN-EXT004"; // reserved for future use
        const string MAGIC_BEGIN_EXT005      = "MAGIC-BEGIN-EXT005"; // reserved for future use
        const string MAGIC_BEGIN_EXT006      = "MAGIC-BEGIN-EXT006"; // reserved for future use
        const string MAGIC_BEGIN_EXT007      = "MAGIC-BEGIN-EXT007"; // reserved for future use
        const string MAGIC_BEGIN_EXT008      = "MAGIC-BEGIN-EXT008"; // reserved for future use
        const string MAGIC_BEGIN_EXT009      = "MAGIC-BEGIN-EXT009"; // reserved for future use
        const string MAGIC_BEGIN_VOLUME      = "PaRtImAgE-VoLuMe";

        const    uint   MAX_CACHE_SIZE     = 16777216;
        const    uint   MAX_CACHED_SECTORS = MAX_CACHE_SIZE / 512;
        readonly byte[] partimageMagic     =
        {
            0x50, 0x61, 0x52, 0x74, 0x49, 0x6D, 0x41, 0x67, 0x45, 0x2D, 0x56, 0x6F, 0x4C, 0x75, 0x4D, 0x65, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        byte[]              bitmap;
        PartimageMainHeader cMainHeader;

        PartimageHeader cVolumeHeader;
        long            dataOff;

        ExtentsULong             extents;
        Dictionary<ulong, ulong> extentsOff;
        ImageInfo                imageInfo;
        Stream                   imageStream;

        Dictionary<ulong, byte[]> sectorCache;

        public Partimage()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Application           = "Partimage",
                ApplicationVersion    = null,
                Creator               = null,
                Comments              = null,
                MediaManufacturer     = null,
                MediaModel            = null,
                MediaSerialNumber     = null,
                MediaBarcode          = null,
                MediaPartNumber       = null,
                MediaSequence         = 0,
                LastMediaSequence     = 0,
                DriveManufacturer     = null,
                DriveModel            = null,
                DriveSerialNumber     = null,
                DriveFirmwareRevision = null
            };
        }

        public ImageInfo Info => imageInfo;

        public string Name => "Partimage disk image";
        public Guid   Id   => new Guid("AAFDB99D-2B77-49EA-831C-C9BB58C68C95");

        public string Format => "Partimage";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] pHdrB = new byte[Marshal.SizeOf(cVolumeHeader)];
            stream.Read(pHdrB, 0, Marshal.SizeOf(cVolumeHeader));
            cVolumeHeader    = new PartimageHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(cVolumeHeader));
            Marshal.Copy(pHdrB, 0, headerPtr, Marshal.SizeOf(cVolumeHeader));
            cVolumeHeader = (PartimageHeader)Marshal.PtrToStructure(headerPtr, typeof(PartimageHeader));
            Marshal.FreeHGlobal(headerPtr);

            return partimageMagic.SequenceEqual(cVolumeHeader.magic);
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] hdrB = new byte[Marshal.SizeOf(cVolumeHeader)];
            stream.Read(hdrB, 0, Marshal.SizeOf(cVolumeHeader));
            cVolumeHeader    = new PartimageHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(cVolumeHeader));
            Marshal.Copy(hdrB, 0, headerPtr, Marshal.SizeOf(cVolumeHeader));
            cVolumeHeader = (PartimageHeader)Marshal.PtrToStructure(headerPtr, typeof(PartimageHeader));
            Marshal.FreeHGlobal(headerPtr);

            DicConsole.DebugWriteLine("Partimage plugin", "CVolumeHeader.magic = {0}",
                                      StringHandlers.CToString(cVolumeHeader.magic));
            DicConsole.DebugWriteLine("Partimage plugin", "CVolumeHeader.version = {0}",
                                      StringHandlers.CToString(cVolumeHeader.version));
            DicConsole.DebugWriteLine("Partimage plugin", "CVolumeHeader.volumeNumber = {0}",
                                      cVolumeHeader.volumeNumber);
            DicConsole.DebugWriteLine("Partimage plugin", "CVolumeHeader.identificator = {0:X16}",
                                      cVolumeHeader.identificator);

            // TODO: Support multifile volumes
            if(cVolumeHeader.volumeNumber > 0)
                throw new FeatureSupportedButNotImplementedImageException("Support for multiple volumes not supported");

            hdrB = new byte[Marshal.SizeOf(cMainHeader)];
            stream.Read(hdrB, 0, Marshal.SizeOf(cMainHeader));
            cMainHeader = new PartimageMainHeader();
            headerPtr   = Marshal.AllocHGlobal(Marshal.SizeOf(cMainHeader));
            Marshal.Copy(hdrB, 0, headerPtr, Marshal.SizeOf(cMainHeader));
            cMainHeader = (PartimageMainHeader)Marshal.PtrToStructure(headerPtr, typeof(PartimageMainHeader));
            Marshal.FreeHGlobal(headerPtr);

            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szFileSystem = {0}",
                                      StringHandlers.CToString(cMainHeader.szFileSystem));
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szPartDescription = {0}",
                                      StringHandlers.CToString(cMainHeader.szPartDescription));
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szOriginalDevice = {0}",
                                      StringHandlers.CToString(cMainHeader.szOriginalDevice));
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szFirstImageFilepath = {0}",
                                      StringHandlers.CToString(cMainHeader.szFirstImageFilepath));
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szUnameSysname = {0}",
                                      StringHandlers.CToString(cMainHeader.szUnameSysname));
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szUnameNodename = {0}",
                                      StringHandlers.CToString(cMainHeader.szUnameNodename));
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szUnameRelease = {0}",
                                      StringHandlers.CToString(cMainHeader.szUnameRelease));
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szUnameVersion = {0}",
                                      StringHandlers.CToString(cMainHeader.szUnameVersion));
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szUnameMachine = {0}",
                                      StringHandlers.CToString(cMainHeader.szUnameMachine));
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwCompression = {0} ({1})",
                                      cMainHeader.dwCompression, (uint)cMainHeader.dwCompression);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwMainFlags = {0}", cMainHeader.dwMainFlags);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_sec = {0}",
                                      cMainHeader.dateCreate.Second);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_min = {0}",
                                      cMainHeader.dateCreate.Minute);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_hour = {0}",
                                      cMainHeader.dateCreate.Hour);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_mday = {0}",
                                      cMainHeader.dateCreate.DayOfMonth);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_mon = {0}",
                                      cMainHeader.dateCreate.Month);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_year = {0}",
                                      cMainHeader.dateCreate.Year);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_wday = {0}",
                                      cMainHeader.dateCreate.DayOfWeek);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_yday = {0}",
                                      cMainHeader.dateCreate.DayOfYear);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_isdst = {0}",
                                      cMainHeader.dateCreate.IsDst);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_gmtoffsec = {0}",
                                      cMainHeader.dateCreate.GmtOff);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate.tm_zone = {0}",
                                      cMainHeader.dateCreate.Timezone);

            DateTime dateCreate = new DateTime(1900                              + (int)cMainHeader.dateCreate.Year,
                                               (int)cMainHeader.dateCreate.Month + 1,
                                               (int)cMainHeader.dateCreate.DayOfMonth, (int)cMainHeader.dateCreate.Hour,
                                               (int)cMainHeader.dateCreate.Minute, (int)cMainHeader.dateCreate.Second);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dateCreate = {0}", dateCreate);

            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.qwPartSize = {0}", cMainHeader.qwPartSize);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szHostname = {0}",
                                      StringHandlers.CToString(cMainHeader.szHostname));
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.szVersion = {0}",
                                      StringHandlers.CToString(cMainHeader.szVersion));
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwMbrCount = {0}", cMainHeader.dwMbrCount);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwMbrSize = {0}",  cMainHeader.dwMbrSize);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwEncryptAlgo = {0} ({1})",
                                      cMainHeader.dwEncryptAlgo, (uint)cMainHeader.dwEncryptAlgo);
            DicConsole.DebugWriteLine("Partimage plugin", "ArrayIsNullOrEmpty(CMainHeader.cHashTestKey) = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(cMainHeader.cHashTestKey));
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture000 = {0}",
                                      cMainHeader.dwReservedFuture000);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture001 = {0}",
                                      cMainHeader.dwReservedFuture001);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture002 = {0}",
                                      cMainHeader.dwReservedFuture002);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture003 = {0}",
                                      cMainHeader.dwReservedFuture003);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture004 = {0}",
                                      cMainHeader.dwReservedFuture004);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture005 = {0}",
                                      cMainHeader.dwReservedFuture005);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture006 = {0}",
                                      cMainHeader.dwReservedFuture006);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture007 = {0}",
                                      cMainHeader.dwReservedFuture007);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture008 = {0}",
                                      cMainHeader.dwReservedFuture008);
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.dwReservedFuture009 = {0}",
                                      cMainHeader.dwReservedFuture009);
            DicConsole.DebugWriteLine("Partimage plugin", "ArrayIsNullOrEmpty(CMainHeader.cReserved) = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(cMainHeader.cReserved));
            DicConsole.DebugWriteLine("Partimage plugin", "CMainHeader.crc = 0x{0:X8}", cMainHeader.crc);

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
                if(!magic.Equals(MAGIC_BEGIN_MBRBACKUP)) throw new ImageNotSupportedException("Cannot find MBRs");

                stream.Seek(cMainHeader.dwMbrSize * cMainHeader.dwMbrCount, SeekOrigin.Current);
            }

            // Skip extended headers and their CRC fields
            stream.Seek((MAGIC_BEGIN_EXT000.Length + 4) * 10, SeekOrigin.Current);

            hdrB = new byte[MAGIC_BEGIN_LOCALHEADER.Length];
            stream.Read(hdrB, 0, MAGIC_BEGIN_LOCALHEADER.Length);
            magic = StringHandlers.CToString(hdrB);
            if(!magic.Equals(MAGIC_BEGIN_LOCALHEADER)) throw new ImageNotSupportedException("Cannot find local header");

            hdrB = new byte[Marshal.SizeOf(typeof(CLocalHeader))];
            stream.Read(hdrB, 0, Marshal.SizeOf(typeof(CLocalHeader)));
            headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CLocalHeader)));
            Marshal.Copy(hdrB, 0, headerPtr, Marshal.SizeOf(typeof(CLocalHeader)));
            CLocalHeader localHeader = (CLocalHeader)Marshal.PtrToStructure(headerPtr, typeof(CLocalHeader));
            Marshal.FreeHGlobal(headerPtr);

            DicConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.qwBlockSize = {0}",  localHeader.qwBlockSize);
            DicConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.qwUsedBlocks = {0}", localHeader.qwUsedBlocks);
            DicConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.qwBlocksCount = {0}",
                                      localHeader.qwBlocksCount);
            DicConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.qwBitmapSize = {0}", localHeader.qwBitmapSize);
            DicConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.qwBadBlocksCount = {0}",
                                      localHeader.qwBadBlocksCount);
            DicConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.szLabel = {0}",
                                      StringHandlers.CToString(localHeader.szLabel));
            DicConsole.DebugWriteLine("Partimage plugin", "ArrayIsNullOrEmpty(CLocalHeader.cReserved) = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(localHeader.cReserved));
            DicConsole.DebugWriteLine("Partimage plugin", "CLocalHeader.crc = 0x{0:X8}", localHeader.crc);

            hdrB = new byte[MAGIC_BEGIN_BITMAP.Length];
            stream.Read(hdrB, 0, MAGIC_BEGIN_BITMAP.Length);
            magic = StringHandlers.CToString(hdrB);
            if(!magic.Equals(MAGIC_BEGIN_BITMAP)) throw new ImageNotSupportedException("Cannot find bitmap");

            bitmap = new byte[localHeader.qwBitmapSize];
            stream.Read(bitmap, 0, (int)localHeader.qwBitmapSize);

            hdrB = new byte[MAGIC_BEGIN_INFO.Length];
            stream.Read(hdrB, 0, MAGIC_BEGIN_INFO.Length);
            magic = StringHandlers.CToString(hdrB);
            if(!magic.Equals(MAGIC_BEGIN_INFO)) throw new ImageNotSupportedException("Cannot find info block");

            // Skip info block and its checksum
            stream.Seek(16384 + 4, SeekOrigin.Current);

            hdrB = new byte[MAGIC_BEGIN_DATABLOCKS.Length];
            stream.Read(hdrB, 0, MAGIC_BEGIN_DATABLOCKS.Length);
            magic = StringHandlers.CToString(hdrB);
            if(!magic.Equals(MAGIC_BEGIN_DATABLOCKS)) throw new ImageNotSupportedException("Cannot find data blocks");

            dataOff = stream.Position;

            DicConsole.DebugWriteLine("Partimage plugin", "dataOff = {0}", dataOff);

            // Seek to tail
            stream.Seek(-(Marshal.SizeOf(typeof(CMainTail)) + MAGIC_BEGIN_TAIL.Length), SeekOrigin.End);

            hdrB = new byte[MAGIC_BEGIN_TAIL.Length];
            stream.Read(hdrB, 0, MAGIC_BEGIN_TAIL.Length);
            magic = StringHandlers.CToString(hdrB);
            if(!magic.Equals(MAGIC_BEGIN_TAIL))
                throw new
                    ImageNotSupportedException("Cannot find tail. Multiple volumes are not supported or image is corrupt.");

            DicConsole.DebugWriteLine("Partimage plugin", "Filling extents");
            DateTime start    = DateTime.Now;
            extents           = new ExtentsULong();
            extentsOff        = new Dictionary<ulong, ulong>();
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

                if(next && current) blockOff++;

                current = next;
            }

            DateTime end = DateTime.Now;
            DicConsole.DebugWriteLine("Partimage plugin", "Took {0} seconds to fill extents",
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
            imageInfo.ImageSize            =
                (ulong)(stream.Length - (dataOff + Marshal.SizeOf(typeof(CMainTail)) + MAGIC_BEGIN_TAIL.Length));
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

            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector)) return sector;

            ulong blockOff = BlockOffset(sectorAddress);

            // Offset of requested sector is:
            // Start of data +
            long imageOff = dataOff +
                            // How many stored bytes to skip
                            (long)(blockOff * imageInfo.SectorSize) +
                            // How many bytes of CRC blocks to skip
                            (long)(blockOff / (CHECK_FREQUENCY / imageInfo.SectorSize)) *
                            Marshal.SizeOf(typeof(CCheck));

            sector = new byte[imageInfo.SectorSize];
            imageStream.Seek(imageOff, SeekOrigin.Begin);
            imageStream.Read(sector, 0, (int)imageInfo.SectorSize);

            if(sectorCache.Count > MAX_CACHED_SECTORS)
            {
                System.Console.WriteLine("Cache cleared");
                sectorCache.Clear();
            }

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

            MemoryStream ms = new MemoryStream();

            bool allEmpty = true;
            for(uint i = 0; i                                                    < length; i++)
                if((bitmap[sectorAddress / 8] & (1 << (int)(sectorAddress % 8))) != 0)
                {
                    allEmpty = false;
                    break;
                }

            if(allEmpty) return new byte[imageInfo.SectorSize * length];

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                   out                                   List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < imageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out                                               List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        // TODO: All blocks contain a CRC32 that's incompatible with current implementation. Need to check for compatibility.
        public bool? VerifyMediaImage()
        {
            return null;
        }

        public List<DumpHardwareType> DumpHardware => null;
        public CICMMetadataType       CicmMetadata => null;

        ulong BlockOffset(ulong sectorAddress)
        {
            extents.GetStart(sectorAddress, out ulong extentStart);
            extentsOff.TryGetValue(extentStart, out ulong extentStartingOffset);
            return extentStartingOffset + (sectorAddress - extentStart);
        }

        /// <summary>
        ///     Partimage disk image header, little-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PartimageHeader
        {
            /// <summary>
            ///     Magic, <see cref="Partimage.partimageMagic" />
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] magic;
            /// <summary>
            ///     Source filesystem
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] version;
            /// <summary>
            ///     Volume number
            /// </summary>
            public uint volumeNumber;
            /// <summary>
            ///     Image identifier
            /// </summary>
            public ulong identificator;
            /// <summary>
            ///     Empty space
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 404)]
            public byte[] reserved;
        }

        struct PortableTm
        {
            public uint Second;
            public uint Minute;
            public uint Hour;
            public uint DayOfMonth;
            public uint Month;
            public uint Year;
            public uint DayOfWeek;
            public uint DayOfYear;
            public uint IsDst;

            public uint GmtOff;
            public uint Timezone;
        }

        enum PCompression : uint
        {
            None  = 0,
            Gzip  = 1,
            Bzip2 = 2,
            Lzo   = 3
        }

        enum PEncryption : uint
        {
            None = 0
        }

        /// <summary>
        ///     Partimage CMainHeader
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PartimageMainHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] szFileSystem; // ext2fs, ntfs, reiserfs, ...
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DESCRIPTION)]
            public byte[] szPartDescription; // user description of the partition
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DEVICENAMELEN)]
            public byte[] szOriginalDevice; // original partition name
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4095)]
            public byte[] szFirstImageFilepath; //MAXPATHLEN]; // for splitted image files

            // system and hardware infos
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
            public byte[] szUnameSysname;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
            public byte[] szUnameNodename;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
            public byte[] szUnameRelease;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
            public byte[] szUnameVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_UNAMEINFOLEN)]
            public byte[] szUnameMachine;

            public PCompression dwCompression; // COMPRESS_XXXXXX
            public uint         dwMainFlags;
            public PortableTm   dateCreate; // date of image creation
            public ulong        qwPartSize; // size of the partition in bytes
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_HOSTNAMESIZE)]
            public byte[] szHostname;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szVersion; // version of the image file

            // MBR backup
            public uint dwMbrCount; // how many MBR are saved in the image file
            public uint dwMbrSize;  // size of a MBR record (allow to change the size in the next versions)

            // future encryption support
            public PEncryption dwEncryptAlgo; // algo used to encrypt data
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] cHashTestKey; // used to test the password without giving it

            // reserved for future use (save DiskLabel, Extended partitions, ...)
            public uint dwReservedFuture000;
            public uint dwReservedFuture001;
            public uint dwReservedFuture002;
            public uint dwReservedFuture003;
            public uint dwReservedFuture004;
            public uint dwReservedFuture005;
            public uint dwReservedFuture006;
            public uint dwReservedFuture007;
            public uint dwReservedFuture008;
            public uint dwReservedFuture009;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6524)]
            public byte[] cReserved; // Adjust to fit with total header size

            public uint crc;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CMbr // must be 1024
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MBR_SIZE_WHOLE)]
            public byte[] cData;
            public uint   dwDataCRC;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DEVICENAMELEN)]
            public byte[] szDevice; // ex: "hda"

            // disk identificators
            ulong qwBlocksCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DESC_MODEL)]
            public byte[] szDescModel;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 884)]
            public byte[] cReserved; // for future use

            //public byte[] szDescGeometry[MAX_DESC_GEOMETRY];
            //public byte[] szDescIdentify[MAX_DESC_IDENTIFY];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CCheck
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            byte[]       cMagic; // must be 'C','H','K'
            public uint  dwCRC;  // CRC of the CHECK_FREQUENCY blocks
            public ulong qwPos;  // number of the last block written
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CLocalHeader // size must be 16384 (adjust the reserved data)
        {
            public ulong qwBlockSize;
            public ulong qwUsedBlocks;
            public ulong qwBlocksCount;
            public ulong qwBitmapSize; // bytes in the bitmap
            public ulong qwBadBlocksCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szLabel;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16280)]
            public byte[] cReserved; // Adjust to fit with total header size

            public uint crc;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CMainTail // size must be 16384 (adjust the reserved data)
        {
            public ulong qwCRC;
            public uint  dwVolumeNumber;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16372)]
            public byte[] cReserved; // Adjust to fit with total header size
        }
    }
}