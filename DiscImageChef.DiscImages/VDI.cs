// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : VDI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.DiscImages
{
    public class VDI : ImagePlugin
    {
        #region Internal constants
        const uint VDIMagic = 0xBEDA107F;
        const uint VDIEmpty = 0xFFFFFFFF;

        const string OracleVDI = "<<< Oracle VM VirtualBox Disk Image >>>\n";
        const string QEMUVDI = "<<< QEMU VM Virtual Disk Image >>>\n";
        const string SunOldVDI = "<<< Sun xVM VirtualBox Disk Image >>>\n";
        const string SunVDI = "<<< Sun VirtualBox Disk Image >>>\n";
        const string InnotekVDI = "<<< innotek VirtualBox Disk Image >>>\n";
        const string InnotekOldVDI = "<<< InnoTek VirtualBox Disk Image >>>\n";
        #endregion

        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        /// <summary>
        /// VDI disk image header, little-endian
        /// </summary>
        struct VDIHeader
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string creator;
            /// <summary>
            /// Magic, <see cref="VDIMagic"/>
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
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string description;
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
            public byte[] garbage;
        }
        #endregion

        VDIHeader vHdr;
        uint[] IBM;
        FileStream imageStream;

        Dictionary<ulong, byte[]> sectorCache;

        const uint MaxCacheSize = 16777216;
        uint maxCachedSectors = MaxCacheSize / 512;

        public VDI()
        {
            Name = "VirtualBox Disk Image";
            PluginUUID = new Guid("E314DE35-C103-48A3-AD36-990F68523C46");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageVersion = null;
            ImageInfo.imageApplication = null;
            ImageInfo.imageApplicationVersion = null;
            ImageInfo.imageCreator = null;
            ImageInfo.imageComments = null;
            ImageInfo.mediaManufacturer = null;
            ImageInfo.mediaModel = null;
            ImageInfo.mediaSerialNumber = null;
            ImageInfo.mediaBarcode = null;
            ImageInfo.mediaPartNumber = null;
            ImageInfo.mediaSequence = 0;
            ImageInfo.lastMediaSequence = 0;
            ImageInfo.driveManufacturer = null;
            ImageInfo.driveModel = null;
            ImageInfo.driveSerialNumber = null;
            ImageInfo.driveFirmwareRevision = null;
        }

        public override bool IdentifyImage(string imagePath)
        {
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;

            byte[] vHdr_b = new byte[Marshal.SizeOf(vHdr)];
            stream.Read(vHdr_b, 0, Marshal.SizeOf(vHdr));
            vHdr = new VDIHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vHdr));
            Marshal.Copy(vHdr_b, 0, headerPtr, Marshal.SizeOf(vHdr));
            vHdr = (VDIHeader)Marshal.PtrToStructure(headerPtr, typeof(VDIHeader));
            Marshal.FreeHGlobal(headerPtr);

            return vHdr.magic == VDIMagic;
        }

        public override bool OpenImage(string imagePath)
        {
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;

            byte[] vHdr_b = new byte[Marshal.SizeOf(vHdr)];
            stream.Read(vHdr_b, 0, Marshal.SizeOf(vHdr));
            vHdr = new VDIHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vHdr));
            Marshal.Copy(vHdr_b, 0, headerPtr, Marshal.SizeOf(vHdr));
            vHdr = (VDIHeader)Marshal.PtrToStructure(headerPtr, typeof(VDIHeader));
            Marshal.FreeHGlobal(headerPtr);

            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.creator = {0}", vHdr.creator);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.magic = {0}", vHdr.magic);
            DicConsole.DebugWriteLine("VirtualBox plugin", "vHdr.version = {0}.{1}", vHdr.majorVersion, vHdr.minorVersion);
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
            IBM = new uint[vHdr.blocks];
            byte[] IBM_b = new byte[vHdr.blocks * 4];
            stream.Read(IBM_b, 0, IBM_b.Length);
            for(int i = 0; i < IBM.Length; i++)
                IBM[i] = BitConverter.ToUInt32(IBM_b, i * 4);

            sectorCache = new Dictionary<ulong, byte[]>();

            FileInfo fi = new FileInfo(imagePath);
            ImageInfo.imageCreationTime = fi.CreationTimeUtc;
            ImageInfo.imageLastModificationTime = fi.LastWriteTimeUtc;
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imagePath);
            ImageInfo.sectors = vHdr.size / vHdr.sectorSize;
            ImageInfo.imageSize = vHdr.size;
            ImageInfo.sectorSize = vHdr.sectorSize;
            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.mediaType = MediaType.GENERIC_HDD;
            ImageInfo.imageComments = vHdr.description;
            ImageInfo.imageVersion = string.Format("{0}.{1}", vHdr.majorVersion, vHdr.minorVersion);

            switch(vHdr.creator)
            {
                case SunVDI:
                    ImageInfo.imageApplication = "Sun VirtualBox";
                    break;
                case SunOldVDI:
                    ImageInfo.imageApplication = "Sun xVM";
                    break;
                case OracleVDI:
                    ImageInfo.imageApplication = "Oracle VirtualBox";
                    break;
                case QEMUVDI:
                    ImageInfo.imageApplication = "QEMU";
                    break;
                case InnotekVDI:
                case InnotekOldVDI:
                    ImageInfo.imageApplication = "innotek VirtualBox";
                    break;
            }
            imageStream = stream;

            return true;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

            byte[] sector;

            if(sectorCache.TryGetValue(sectorAddress, out sector))
                return sector;

            ulong index = (sectorAddress * vHdr.sectorSize) / vHdr.blockSize;
            ulong secOff = (sectorAddress * vHdr.sectorSize) % vHdr.blockSize;

            uint ibmOff = IBM[index];
            ulong imageOff;

            if(ibmOff == VDIEmpty)
                return new byte[vHdr.sectorSize];

            imageOff = vHdr.offsetData + (ibmOff * vHdr.blockSize);

            byte[] cluster = new byte[vHdr.blockSize];
            imageStream.Seek((long)imageOff, SeekOrigin.Begin);
            imageStream.Read(cluster, 0, (int)vHdr.blockSize);
            sector = new byte[vHdr.sectorSize];
            Array.Copy(cluster, (int)secOff, sector, 0, vHdr.sectorSize);

            if(sectorCache.Count > maxCachedSectors)
                sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0} + {1}) than available ({2})", sectorAddress, length, ImageInfo.sectors));

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
            return ImageInfo.imageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.sectorSize;
        }

        public override string GetImageFormat()
        {
            return "VDI";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.imageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.imageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.imageApplicationVersion;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.imageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.imageLastModificationTime;
        }

        public override string GetImageName()
        {
            return ImageInfo.imageName;
        }

        public override string GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
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

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();
            for(ulong i = 0; i < ImageInfo.sectors; i++)
                UnknownLBAs.Add(i);
            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
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
