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
using DiscImageChef.DiscImages;

namespace DiscImageChef.DiscImages
{
    public class Vdi : ImagePlugin
    {
        #region Internal constants
        const uint VDI_MAGIC = 0xBEDA107F;
        const uint VDI_EMPTY = 0xFFFFFFFF;

        const string ORACLE_VDI = "<<< Oracle VM VirtualBox Disk Image >>>\n";
        const string QEMUVDI = "<<< QEMU VM Virtual Disk Image >>>\n";
        const string SUN_OLD_VDI = "<<< Sun xVM VirtualBox Disk Image >>>\n";
        const string SUN_VDI = "<<< Sun VirtualBox Disk Image >>>\n";
        const string INNOTEK_VDI = "<<< innotek VirtualBox Disk Image >>>\n";
        const string INNOTEK_OLD_VDI = "<<< InnoTek VirtualBox Disk Image >>>\n";
        #endregion

        #region Internal Structures
        /// <summary>
        /// VDI disk image header, little-endian
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VdiHeader
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string creator;
            /// <summary>
            /// Magic, <see cref="Vdi.VDI_MAGIC"/>
            /// </summary>
            public uint magic;
            /// <summary>
            /// Version
            /// </summary>
            public ushort majorVersion;
            public ushort minorVersion;
            public uint headerSize;
            public uint imageType;
            public uint imageFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string description;
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)] public byte[] garbage;
        }
        #endregion

        VdiHeader vHdr;
        uint[] ibm;
        Stream imageStream;

        Dictionary<ulong, byte[]> sectorCache;

        const uint MAX_CACHE_SIZE = 16777216;
        const uint MAX_CACHED_SECTORS = MAX_CACHE_SIZE / 512;

        public Vdi()
        {
            Name = "VirtualBox Disk Image";
            PluginUuid = new Guid("E314DE35-C103-48A3-AD36-990F68523C46");
            ImageInfo = new ImageInfo();
            ImageInfo.ReadableSectorTags = new List<SectorTagType>();
            ImageInfo.ReadableMediaTags = new List<MediaTagType>();
            ImageInfo.ImageHasPartitions = false;
            ImageInfo.ImageHasSessions = false;
            ImageInfo.ImageVersion = null;
            ImageInfo.ImageApplication = null;
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

            byte[] vHdrB = new byte[Marshal.SizeOf(vHdr)];
            stream.Read(vHdrB, 0, Marshal.SizeOf(vHdr));
            vHdr = new VdiHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vHdr));
            Marshal.Copy(vHdrB, 0, headerPtr, Marshal.SizeOf(vHdr));
            vHdr = (VdiHeader)Marshal.PtrToStructure(headerPtr, typeof(VdiHeader));
            Marshal.FreeHGlobal(headerPtr);

            return vHdr.magic == VDI_MAGIC;
        }

        public override bool OpenImage(Filter imageFilter)
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
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.description = {0}", vHdr.description);
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

            DicConsole.DebugWriteLine("VirtualBox plugin", "Reading Image Block Map");
            stream.Seek(vHdr.offsetBlocks, SeekOrigin.Begin);
            ibm = new uint[vHdr.blocks];
            byte[] ibmB = new byte[vHdr.blocks * 4];
            stream.Read(ibmB, 0, ibmB.Length);
            for(int i = 0; i < ibm.Length; i++) ibm[i] = BitConverter.ToUInt32(ibmB, i * 4);

            sectorCache = new Dictionary<ulong, byte[]>();

            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.Sectors = vHdr.size / vHdr.sectorSize;
            ImageInfo.ImageSize = vHdr.size;
            ImageInfo.SectorSize = vHdr.sectorSize;
            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.MediaType = MediaType.GENERIC_HDD;
            ImageInfo.ImageComments = vHdr.description;
            ImageInfo.ImageVersion = string.Format("{0}.{1}", vHdr.majorVersion, vHdr.minorVersion);

            switch(vHdr.creator)
            {
                case SUN_VDI:
                    ImageInfo.ImageApplication = "Sun VirtualBox";
                    break;
                case SUN_OLD_VDI:
                    ImageInfo.ImageApplication = "Sun xVM";
                    break;
                case ORACLE_VDI:
                    ImageInfo.ImageApplication = "Oracle VirtualBox";
                    break;
                case QEMUVDI:
                    ImageInfo.ImageApplication = "QEMU";
                    break;
                case INNOTEK_VDI:
                case INNOTEK_OLD_VDI:
                    ImageInfo.ImageApplication = "innotek VirtualBox";
                    break;
            }

            imageStream = stream;

            ImageInfo.Cylinders = vHdr.cylinders;
            ImageInfo.Heads = vHdr.heads;
            ImageInfo.SectorsPerTrack = vHdr.spt;

            return true;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            byte[] sector;

            if(sectorCache.TryGetValue(sectorAddress, out sector)) return sector;

            ulong index = (sectorAddress * vHdr.sectorSize) / vHdr.blockSize;
            ulong secOff = (sectorAddress * vHdr.sectorSize) % vHdr.blockSize;

            uint ibmOff = ibm[index];
            ulong imageOff;

            if(ibmOff == VDI_EMPTY) return new byte[vHdr.sectorSize];

            imageOff = vHdr.offsetData + (ibmOff * vHdr.blockSize);

            byte[] cluster = new byte[vHdr.blockSize];
            imageStream.Seek((long)imageOff, SeekOrigin.Begin);
            imageStream.Read(cluster, 0, (int)vHdr.blockSize);
            sector = new byte[vHdr.sectorSize];
            Array.Copy(cluster, (int)secOff, sector, 0, vHdr.sectorSize);

            if(sectorCache.Count > MAX_CACHED_SECTORS) sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      string.Format("Sector address {0} not found", sectorAddress));

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string
                                                          .Format("Requested more sectors ({0} + {1}) than available ({2})",
                                                                  sectorAddress, length, ImageInfo.Sectors));

            MemoryStream ms = new MemoryStream();

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
            return "VDI";
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

        public override bool? VerifyMediaImage()
        {
            return null;
        }
        #endregion
    }
}