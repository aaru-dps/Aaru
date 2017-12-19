// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Virtual98.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Virtual98 disk images.
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
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
    // Info from Neko Project II emulator
    public class Virtual98 : ImagePlugin
    {
        #region Internal structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Virtual98Header
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] signature;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] comment;
            public uint padding;
            public ushort mbsize;
            public ushort sectorsize;
            public byte sectors;
            public byte surfaces;
            public ushort cylinders;
            public uint totals;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x44)] public byte[] padding2;
        }
        #endregion

        readonly byte[] signature = {0x56, 0x48, 0x44, 0x31, 0x2E, 0x30, 0x30, 0x00};

        public Virtual98()
        {
            Name = "Virtual98 Disk Image";
            PluginUUID = new Guid("C0CDE13D-04D0-4913-8740-AFAA44D0A107");
            ImageInfo = new ImageInfo()
            {
                readableSectorTags = new List<SectorTagType>(),
                readableMediaTags = new List<MediaTagType>(),
                imageHasPartitions = false,
                imageHasSessions = false,
                imageVersion = null,
                imageApplication = null,
                imageApplicationVersion = null,
                imageCreator = null,
                imageComments = null,
                mediaManufacturer = null,
                mediaModel = null,
                mediaSerialNumber = null,
                mediaBarcode = null,
                mediaPartNumber = null,
                mediaSequence = 0,
                lastMediaSequence = 0,
                driveManufacturer = null,
                driveModel = null,
                driveSerialNumber = null,
                driveFirmwareRevision = null
            };
        }

        Virtual98Header v98hdr;
        Filter nhdImageFilter;

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            // Even if comment is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
            Encoding shiftjis = Encoding.GetEncoding("shift_jis");

            v98hdr = new Virtual98Header();

            if(stream.Length < Marshal.SizeOf(v98hdr)) return false;

            byte[] hdr_b = new byte[Marshal.SizeOf(v98hdr)];
            stream.Read(hdr_b, 0, hdr_b.Length);

            GCHandle handle = GCHandle.Alloc(hdr_b, GCHandleType.Pinned);
            v98hdr = (Virtual98Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Virtual98Header));
            handle.Free();

            if(!v98hdr.signature.SequenceEqual(signature)) return false;

            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.signature = \"{0}\"",
                                      StringHandlers.CToString(v98hdr.signature, shiftjis));
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.comment = \"{0}\"",
                                      StringHandlers.CToString(v98hdr.comment, shiftjis));
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.padding = {0}", v98hdr.padding);
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.mbsize = {0}", v98hdr.mbsize);
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.sectorsize = {0}", v98hdr.sectorsize);
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.sectors = {0}", v98hdr.sectors);
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.surfaces = {0}", v98hdr.surfaces);
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.cylinders = {0}", v98hdr.cylinders);
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.totals = {0}", v98hdr.totals);

            return true;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            // Even if comment is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
            Encoding shiftjis = Encoding.GetEncoding("shift_jis");

            v98hdr = new Virtual98Header();

            if(stream.Length < Marshal.SizeOf(v98hdr)) return false;

            byte[] hdr_b = new byte[Marshal.SizeOf(v98hdr)];
            stream.Read(hdr_b, 0, hdr_b.Length);

            GCHandle handle = GCHandle.Alloc(hdr_b, GCHandleType.Pinned);
            v98hdr = (Virtual98Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Virtual98Header));
            handle.Free();

            ImageInfo.mediaType = MediaType.GENERIC_HDD;

            ImageInfo.imageSize = (ulong)(stream.Length - 0xDC);
            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.sectors = v98hdr.totals;
            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.sectorSize = v98hdr.sectorsize;
            ImageInfo.cylinders = v98hdr.cylinders;
            ImageInfo.heads = v98hdr.surfaces;
            ImageInfo.sectorsPerTrack = v98hdr.sectors;
            ImageInfo.imageComments = StringHandlers.CToString(v98hdr.comment, shiftjis);

            nhdImageFilter = imageFilter;

            return true;
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
            return "Virtual98 disk image";
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

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * ImageInfo.sectorSize];

            Stream stream = nhdImageFilter.GetDataForkStream();

            // V98 are lazy allocated
            if((long)(0xDC + sectorAddress * ImageInfo.sectorSize) >= stream.Length) return buffer;

            stream.Seek((long)(0xDC + sectorAddress * ImageInfo.sectorSize), SeekOrigin.Begin);

            int toRead = (int)(length * ImageInfo.sectorSize);

            if(toRead + stream.Position > stream.Length) toRead = (int)(stream.Length - stream.Position);

            stream.Read(buffer, 0, toRead);

            return buffer;
        }

        #region Unsupported features
        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
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

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
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

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs,
                                            out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();
            for(ulong i = 0; i < ImageInfo.sectors; i++) UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs,
                                            out List<ulong> UnknownLBAs)
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