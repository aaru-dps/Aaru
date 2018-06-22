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
using Extents;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public class PartClone : IMediaImage
    {
        const int CRC_SIZE = 4;

        const    uint   MAX_CACHE_SIZE     = 16777216;
        const    uint   MAX_CACHED_SECTORS = MAX_CACHE_SIZE / 512;
        readonly byte[] biTmAgIc           = {0x42, 0x69, 0x54, 0x6D, 0x41, 0x67, 0x49, 0x63};
        readonly byte[] partCloneMagic =
            {0x70, 0x61, 0x72, 0x74, 0x63, 0x6C, 0x6F, 0x6E, 0x65, 0x2D, 0x69, 0x6D, 0x61, 0x67, 0x65};
        // The used block "bitmap" uses one byte per block
        // TODO: Convert on-image bytemap to on-memory bitmap
        byte[] byteMap;
        long   dataOff;

        ExtentsULong             extents;
        Dictionary<ulong, ulong> extentsOff;
        ImageInfo                imageInfo;
        Stream                   imageStream;

        PartCloneHeader pHdr;

        Dictionary<ulong, byte[]> sectorCache;

        public PartClone()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Application           = "PartClone",
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

        public string    Name => "PartClone disk image";
        public Guid      Id   => new Guid("AB1D7518-B548-4099-A4E2-C29C53DDE0C3");
        public ImageInfo Info => imageInfo;

        public string Format => "PartClone";

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

        public bool Open(IFilter imageFilter)
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
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.version = {0}",
                                      StringHandlers.CToString(pHdr.version));
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.blockSize = {0}",   pHdr.blockSize);
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.deviceSize = {0}",  pHdr.deviceSize);
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.totalBlocks = {0}", pHdr.totalBlocks);
            DicConsole.DebugWriteLine("PartClone plugin", "pHdr.usedBlocks = {0}",  pHdr.usedBlocks);

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
            extents    = new ExtentsULong();
            extentsOff = new Dictionary<ulong, ulong>();
            bool  current     = byteMap[0] > 0;
            ulong blockOff    = 0;
            ulong extentStart = 0;

            for(ulong i = 1; i < pHdr.totalBlocks; i++)
            {
                bool next = byteMap[i] > 0;

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
                        extentsOff.TryGetValue(extentStart, out _);
                    }

                if(next && current) blockOff++;

                current = next;
            }

            DateTime end = DateTime.Now;
            DicConsole.DebugWriteLine("PartClone plugin", "Took {0} seconds to fill extents",
                                      (end - start).TotalSeconds);

            sectorCache = new Dictionary<ulong, byte[]>();

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = pHdr.totalBlocks;
            imageInfo.SectorSize           = pHdr.blockSize;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.ImageSize            = (ulong)(stream.Length - (4096 + 0x40 + (long)pHdr.totalBlocks));
            imageStream                    = stream;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(byteMap[sectorAddress] == 0) return new byte[pHdr.blockSize];

            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector)) return sector;

            long imageOff = dataOff + (long)(BlockOffset(sectorAddress) * (pHdr.blockSize + CRC_SIZE));

            sector = new byte[pHdr.blockSize];
            imageStream.Seek(imageOff, SeekOrigin.Begin);
            imageStream.Read(sector, 0, (int)pHdr.blockSize);

            if(sectorCache.Count > MAX_CACHED_SECTORS) sectorCache.Clear();

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
            for(uint i = 0; i < length; i++)
                if(byteMap[sectorAddress + i] != 0)
                {
                    allEmpty = false;
                    break;
                }

            if(allEmpty) return new byte[pHdr.blockSize * length];

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

        public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < imageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
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
        ///     PartClone disk image header, little-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PartCloneHeader
        {
            /// <summary>
            ///     Magic, <see cref="PartClone.partCloneMagic" />
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public byte[] magic;
            /// <summary>
            ///     Source filesystem
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public byte[] filesystem;
            /// <summary>
            ///     Version
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] version;
            /// <summary>
            ///     Padding
            /// </summary>
            public ushort padding;
            /// <summary>
            ///     Block (sector) size
            /// </summary>
            public uint blockSize;
            /// <summary>
            ///     Size of device containing the cloned partition
            /// </summary>
            public ulong deviceSize;
            /// <summary>
            ///     Total blocks in cloned partition
            /// </summary>
            public ulong totalBlocks;
            /// <summary>
            ///     Used blocks in cloned partition
            /// </summary>
            public ulong usedBlocks;
            /// <summary>
            ///     Empty space
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
            public byte[] buffer;
        }
    }
}