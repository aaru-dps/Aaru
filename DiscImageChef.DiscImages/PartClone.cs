// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PartClone.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages partclone disk images.
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
using DiscImageChef.DiscImages;
using Extents;

namespace DiscImageChef.DiscImages
{
    public class PartClone : ImagePlugin
    {
        #region Internal constants
        readonly byte[] partCloneMagic =
            {0x70, 0x61, 0x72, 0x74, 0x63, 0x6C, 0x6F, 0x6E, 0x65, 0x2D, 0x69, 0x6D, 0x61, 0x67, 0x65};
        readonly byte[] biTmAgIc = {0x42, 0x69, 0x54, 0x6D, 0x41, 0x67, 0x49, 0x63};
        const int CRC_SIZE = 4;
        #endregion

        #region Internal Structures
        /// <summary>
        /// PartClone disk image header, little-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PartCloneHeader
        {
            /// <summary>
            /// Magic, <see cref="PartClone.partCloneMagic"/>
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)] public byte[] magic;
            /// <summary>
            /// Source filesystem
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)] public byte[] filesystem;
            /// <summary>
            /// Version
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] version;
            /// <summary>
            /// Padding
            /// </summary>
            public ushort padding;
            /// <summary>
            /// Block (sector) size
            /// </summary>
            public uint blockSize;
            /// <summary>
            /// Size of device containing the cloned partition
            /// </summary>
            public ulong deviceSize;
            /// <summary>
            /// Total blocks in cloned partition
            /// </summary>
            public ulong totalBlocks;
            /// <summary>
            /// Used blocks in cloned partition
            /// </summary>
            public ulong usedBlocks;
            /// <summary>
            /// Empty space
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)] public byte[] buffer;
        }
        #endregion

        PartCloneHeader pHdr;
        // The used block "bitmap" uses one byte per block
        // TODO: Convert on-image bytemap to on-memory bitmap
        byte[] byteMap;
        Stream imageStream;
        long dataOff;

        Dictionary<ulong, byte[]> sectorCache;

        const uint MAX_CACHE_SIZE = 16777216;
        uint maxCachedSectors = MAX_CACHE_SIZE / 512;

        ExtentsULong extents;
        Dictionary<ulong, ulong> extentsOff;

        public PartClone()
        {
            Name = "PartClone disk image";
            PluginUuid = new Guid("AB1D7518-B548-4099-A4E2-C29C53DDE0C3");
            ImageInfo = new ImageInfo();
            ImageInfo.ReadableSectorTags = new List<SectorTagType>();
            ImageInfo.ReadableMediaTags = new List<MediaTagType>();
            ImageInfo.ImageHasPartitions = false;
            ImageInfo.ImageHasSessions = false;
            ImageInfo.ImageApplication = "PartClone";
            ImageInfo.ImageApplicationVersion = null;
            ImageInfo.ImageCreator = null;
            ImageInfo.ImageComments = null;
            ImageInfo.MediaManufacturer = null;
            ImageInfo.MediaModel = null;
            ImageInfo.MediaSerialNumber = null;
            ImageInfo.MediaBarcode = null;
            ImageInfo.MediaPartNumber = null;
            ImageInfo.MediaSequence = 0;
            ImageInfo.LastMediaSequence = 0;
            ImageInfo.DriveManufacturer = null;
            ImageInfo.DriveModel = null;
            ImageInfo.DriveSerialNumber = null;
            ImageInfo.DriveFirmwareRevision = null;
        }

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] pHdrB = new byte[Marshal.SizeOf(pHdr)];
            stream.Read(pHdrB, 0, Marshal.SizeOf(pHdr));
            pHdr = new PartCloneHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pHdr));
            Marshal.Copy(pHdrB, 0, headerPtr, Marshal.SizeOf(pHdr));
            pHdr = (PartCloneHeader)Marshal.PtrToStructure(headerPtr, typeof(PartCloneHeader));
            Marshal.FreeHGlobal(headerPtr);

            if(stream.Position + (long)pHdr.totalBlocks > stream.Length) return false;

            stream.Seek((long)pHdr.totalBlocks, SeekOrigin.Current);

            byte[] bitmagic = new byte[8];
            stream.Read(bitmagic, 0, 8);

            return partCloneMagic.SequenceEqual(pHdr.magic) && biTmAgIc.SequenceEqual(bitmagic);
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] pHdrB = new byte[Marshal.SizeOf(pHdr)];
            stream.Read(pHdrB, 0, Marshal.SizeOf(pHdr));
            pHdr = new PartCloneHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pHdr));
            Marshal.Copy(pHdrB, 0, headerPtr, Marshal.SizeOf(pHdr));
            pHdr = (PartCloneHeader)Marshal.PtrToStructure(headerPtr, typeof(PartCloneHeader));
            Marshal.FreeHGlobal(headerPtr);

            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.magic = {0}", StringHandlers.CToString(pHdr.magic));
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.filesystem = {0}",
                                      StringHandlers.CToString(pHdr.filesystem));
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.version = {0}", StringHandlers.CToString(pHdr.version));
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.blockSize = {0}", pHdr.blockSize);
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.deviceSize = {0}", pHdr.deviceSize);
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.totalBlocks = {0}", pHdr.totalBlocks);
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.usedBlocks = {0}", pHdr.usedBlocks);

            byteMap = new byte[pHdr.totalBlocks];
            DicConsole.DebugWriteLine("PartClone plugin", "Reading bytemap {0} bytes", byteMap.Length);
            stream.Read(byteMap, 0, byteMap.Length);

            byte[] bitmagic = new byte[8];
            stream.Read(bitmagic, 0, 8);

            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.bitmagic = {0}", StringHandlers.CToString(bitmagic));

            if(!biTmAgIc.SequenceEqual(bitmagic))
                throw new ImageNotSupportedException("Could not find partclone BiTmAgIc, not continuing...");

            dataOff = stream.Position;
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.dataOff = {0}", dataOff);

            DicConsole.DebugWriteLine("PartClone plugin", "Filling extents");
            DateTime start = DateTime.Now;
            extents = new ExtentsULong();
            extentsOff = new Dictionary<ulong, ulong>();
            bool current = byteMap[0] > 0;
            ulong blockOff = 0;
            ulong extentStart = 0;

            for(ulong i = 1; i < pHdr.totalBlocks; i++)
            {
                bool next = byteMap[i] > 0;

                // Flux
                if(next != current)
                {
                    // Next is used
                    if(next)
                    {
                        extentStart = i;
                        extentsOff.Add(i, ++blockOff);
                    }
                    else
                    {
                        extents.Add(extentStart, i);
                        extentsOff.TryGetValue(extentStart, out ulong foo);
                    }
                }

                if(next && current) blockOff++;

                current = next;
            }

            DateTime end = DateTime.Now;
            DicConsole.DebugWriteLine("PartClone plugin", "Took {0} seconds to fill extents",
                                      (end - start).TotalSeconds);

            sectorCache = new Dictionary<ulong, byte[]>();

            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.Sectors = pHdr.totalBlocks;
            ImageInfo.SectorSize = pHdr.blockSize;
            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.MediaType = MediaType.GENERIC_HDD;
            ImageInfo.ImageSize = (ulong)(stream.Length - (4096 + 0x40 + (long)pHdr.totalBlocks));
            imageStream = stream;

            return true;
        }

        ulong BlockOffset(ulong sectorAddress)
        {
            extents.GetStart(sectorAddress, out ulong extentStart);
            extentsOff.TryGetValue(extentStart, out ulong extentStartingOffset);
            return extentStartingOffset + (sectorAddress - extentStart);
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            if(byteMap[sectorAddress] == 0) return new byte[pHdr.blockSize];

            byte[] sector;

            if(sectorCache.TryGetValue(sectorAddress, out sector)) return sector;

            long imageOff = dataOff + (long)(BlockOffset(sectorAddress) * (pHdr.blockSize + CRC_SIZE));

            sector = new byte[pHdr.blockSize];
            imageStream.Seek(imageOff, SeekOrigin.Begin);
            imageStream.Read(sector, 0, (int)pHdr.blockSize);

            if(sectorCache.Count > maxCachedSectors) sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream ms = new MemoryStream();

            bool allEmpty = true;
            for(uint i = 0; i < length; i++)
            {
                if(byteMap[sectorAddress + i] != 0)
                {
                    allEmpty = false;
                    break;
                }
            }

            if(allEmpty) return new byte[pHdr.blockSize * length];

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public override bool ImageHasPartitions()
        {
            return false;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.ImageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.Sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.SectorSize;
        }

        public override string GetImageFormat()
        {
            return "PartClone";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.ImageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.ImageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.ImageApplicationVersion;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.ImageCreator;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.ImageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.ImageLastModificationTime;
        }

        public override string GetImageName()
        {
            return ImageInfo.ImageName;
        }

        public override string GetImageComments()
        {
            return ImageInfo.ImageComments;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.MediaType;
        }

        #region Unsupported features
        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetMediaManufacturer()
        {
            return null;
        }

        public override string GetMediaModel()
        {
            return null;
        }

        public override string GetMediaSerialNumber()
        {
            return null;
        }

        public override string GetMediaBarcode()
        {
            return null;
        }

        public override string GetMediaPartNumber()
        {
            return null;
        }

        public override int GetMediaSequence()
        {
            return 0;
        }

        public override int GetLastDiskSequence()
        {
            return 0;
        }

        public override string GetDriveManufacturer()
        {
            return null;
        }

        public override string GetDriveModel()
        {
            return null;
        }

        public override string GetDriveSerialNumber()
        {
            return null;
        }

        public override List<Partition> GetPartitions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetTracks()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Session> GetSessions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < ImageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        // TODO: All blocks contain a CRC32 that's incompatible with current implementation. Need to check for compatibility.
        public override bool? VerifyMediaImage()
        {
            return null;
        }
        #endregion
    }
}