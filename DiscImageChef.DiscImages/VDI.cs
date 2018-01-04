// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : VDI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages VirtualBox disk images.
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
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    // TODO: Support version 0
    // TODO: Support fixed images
    // TODO: Support version 1.2 geometry
    public class Vdi : IMediaImage
    {
        const uint VDI_MAGIC = 0xBEDA107F;
        const uint VDI_EMPTY = 0xFFFFFFFF;

        const string ORACLE_VDI = "<<< Oracle VM VirtualBox Disk Image >>>\n";
        const string QEMUVDI = "<<< QEMU VM Virtual Disk Image >>>\n";
        const string SUN_OLD_VDI = "<<< Sun xVM VirtualBox Disk Image >>>\n";
        const string SUN_VDI = "<<< Sun VirtualBox Disk Image >>>\n";
        const string INNOTEK_VDI = "<<< innotek VirtualBox Disk Image >>>\n";
        const string INNOTEK_OLD_VDI = "<<< InnoTek VirtualBox Disk Image >>>\n";
        const string DIC_VDI = "<<< DiscImageChef VirtualBox Disk Image >>>\n";

        const uint MAX_CACHE_SIZE = 16777216;
        const uint MAX_CACHED_SECTORS = MAX_CACHE_SIZE / 512;
        uint[] ibm;
        ImageInfo imageInfo;
        Stream imageStream;

        Dictionary<ulong, byte[]> sectorCache;

        VdiHeader vHdr;

        public Vdi()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                HasPartitions = false,
                HasSessions = false,
                Version = null,
                Application = null,
                ApplicationVersion = null,
                Creator = null,
                Comments = null,
                MediaManufacturer = null,
                MediaModel = null,
                MediaSerialNumber = null,
                MediaBarcode = null,
                MediaPartNumber = null,
                MediaSequence = 0,
                LastMediaSequence = 0,
                DriveManufacturer = null,
                DriveModel = null,
                DriveSerialNumber = null,
                DriveFirmwareRevision = null
            };
        }

        public ImageInfo Info => imageInfo;

        public string Name => "VirtualBox Disk Image";
        public Guid Id => new Guid("E314DE35-C103-48A3-AD36-990F68523C46");

        public string Format => "VDI";

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

            byte[] vHdrB = new byte[Marshal.SizeOf(vHdr)];
            stream.Read(vHdrB, 0, Marshal.SizeOf(vHdr));
            vHdr = new VdiHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vHdr));
            Marshal.Copy(vHdrB, 0, headerPtr, Marshal.SizeOf(vHdr));
            vHdr = (VdiHeader)Marshal.PtrToStructure(headerPtr, typeof(VdiHeader));
            Marshal.FreeHGlobal(headerPtr);

            return vHdr.magic == VDI_MAGIC;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] vHdrB = new byte[Marshal.SizeOf(vHdr)];
            stream.Read(vHdrB, 0, Marshal.SizeOf(vHdr));
            vHdr = new VdiHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vHdr));
            Marshal.Copy(vHdrB, 0, headerPtr, Marshal.SizeOf(vHdr));
            vHdr = (VdiHeader)Marshal.PtrToStructure(headerPtr, typeof(VdiHeader));
            Marshal.FreeHGlobal(headerPtr);

            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.creator = {0}", vHdr.creator);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.magic = {0}", vHdr.magic);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.version = {0}.{1}", vHdr.majorVersion,
                                      vHdr.minorVersion);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.headerSize = {0}", vHdr.headerSize);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.imageType = {0}", vHdr.imageType);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.imageFlags = {0}", vHdr.imageFlags);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.description = {0}", vHdr.comments);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.offsetBlocks = {0}", vHdr.offsetBlocks);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.offsetData = {0}", vHdr.offsetData);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.cylinders = {0}", vHdr.cylinders);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.heads = {0}", vHdr.heads);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.spt = {0}", vHdr.spt);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.sectorSize = {0}", vHdr.sectorSize);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.size = {0}", vHdr.size);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.blockSize = {0}", vHdr.blockSize);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.blockExtraData = {0}", vHdr.blockExtraData);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.blocks = {0}", vHdr.blocks);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.allocatedBlocks = {0}", vHdr.allocatedBlocks);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.uuid = {0}", vHdr.uuid);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.snapshotUuid = {0}", vHdr.snapshotUuid);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.linkUuid = {0}", vHdr.linkUuid);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.parentUuid = {0}", vHdr.parentUuid);

            if(vHdr.imageType != VdiImageType.Normal)
                throw new FeatureSupportedButNotImplementedImageException($"Support for image type {vHdr.imageType} not yet implemented");
            
            DicConsole.DebugWriteLine("VirtualBox plugin", "Reading Image Block Map");
            stream.Seek(vHdr.offsetBlocks, SeekOrigin.Begin);
            ibm = new uint[vHdr.blocks];
            byte[] ibmB = new byte[vHdr.blocks * 4];
            stream.Read(ibmB, 0, ibmB.Length);
            for(int i = 0; i < ibm.Length; i++) ibm[i] = BitConverter.ToUInt32(ibmB, i * 4);

            sectorCache = new Dictionary<ulong, byte[]>();

            imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors = vHdr.size / vHdr.sectorSize;
            imageInfo.ImageSize = vHdr.size;
            imageInfo.SectorSize = vHdr.sectorSize;
            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            imageInfo.MediaType = MediaType.GENERIC_HDD;
            imageInfo.Comments = vHdr.comments;
            imageInfo.Version = $"{vHdr.majorVersion}.{vHdr.minorVersion}";

            switch(vHdr.creator)
            {
                case SUN_VDI:
                    imageInfo.Application = "Sun VirtualBox";
                    break;
                case SUN_OLD_VDI:
                    imageInfo.Application = "Sun xVM";
                    break;
                case ORACLE_VDI:
                    imageInfo.Application = "Oracle VirtualBox";
                    break;
                case QEMUVDI:
                    imageInfo.Application = "QEMU";
                    break;
                case INNOTEK_VDI:
                case INNOTEK_OLD_VDI:
                    imageInfo.Application = "innotek VirtualBox";
                    break;
                case DIC_VDI:
                    imageInfo.Application = "DiscImageChef";
                    break;
            }

            imageStream = stream;

            imageInfo.Cylinders = vHdr.cylinders;
            imageInfo.Heads = vHdr.heads;
            imageInfo.SectorsPerTrack = vHdr.spt;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector)) return sector;

            ulong index = sectorAddress * vHdr.sectorSize / vHdr.blockSize;
            ulong secOff = sectorAddress * vHdr.sectorSize % vHdr.blockSize;

            uint ibmOff = ibm[index];
            ulong imageOff;

            if(ibmOff == VDI_EMPTY) return new byte[vHdr.sectorSize];

            imageOff = vHdr.offsetData + ibmOff * vHdr.blockSize;

            byte[] cluster = new byte[vHdr.blockSize];
            imageStream.Seek((long)imageOff, SeekOrigin.Begin);
            imageStream.Read(cluster, 0, (int)vHdr.blockSize);
            sector = new byte[vHdr.sectorSize];
            Array.Copy(cluster, (int)secOff, sector, 0, vHdr.sectorSize);

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
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({sectorAddress} + {length}) than available ({imageInfo.Sectors})");

            MemoryStream ms = new MemoryStream();

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
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < imageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifyMediaImage()
        {
            return null;
        }

        /// <summary>
        ///     VDI disk image header, little-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VdiHeader
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string creator;
            /// <summary>
            ///     Magic, <see cref="Vdi.VDI_MAGIC" />
            /// </summary>
            public uint magic;
            /// <summary>
            ///     Version
            /// </summary>
            public ushort majorVersion;
            public ushort minorVersion;
            public int headerSize;
            public VdiImageType imageType;
            public VdiImageFlags imageFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string comments;
            public uint offsetBlocks;
            public uint offsetData;
            public uint cylinders;
            public uint heads;
            public uint spt;
            public uint sectorSize;
            public uint unused;
            public ulong size;
            public uint blockSize;
            public uint blockExtraData;
            public uint blocks;
            public uint allocatedBlocks;
            public Guid uuid;
            public Guid snapshotUuid;
            public Guid linkUuid;
            public Guid parentUuid;
        }

        
        enum VdiImageType : uint
        {
        /// <summary> Normal dynamically growing base image file.</summary>
            Normal = 1,
        /// <summary>Preallocated base image file of a fixed size.</summary>
            Fixed,
        /// <summary>Dynamically growing image file for undo/commit changes support.</summary>
            Undo,
        /// <summary>Dynamically growing image file for differencing support.</summary>
            Differential,

        /// <summary>First valid image type value.</summary>
            First = Normal,
        /// <summary>Last valid image type value.</summary>
            Last = Differential
        }

        enum VdiImageFlags : uint
        {
            /// <summary>
            /// Fill new blocks with zeroes while expanding image file. Only valid for newly created images, never set
            /// for opened existing images.</summary>
            ZeroExpand = 0x100
        }
    }
}