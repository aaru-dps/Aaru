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

namespace DiscImageChef.DiscImages
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
            PluginUuid = new Guid("C0CDE13D-04D0-4913-8740-AFAA44D0A107");
            ImageInfo = new ImageInfo()
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                ImageHasPartitions = false,
                ImageHasSessions = false,
                ImageVersion = null,
                ImageApplication = null,
                ImageApplicationVersion = null,
                ImageCreator = null,
                ImageComments = null,
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

        Virtual98Header v98Hdr;
        Filter nhdImageFilter;

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            // Even if comment is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
            Encoding shiftjis = Encoding.GetEncoding("shift_jis");

            v98Hdr = new Virtual98Header();

            if(stream.Length < Marshal.SizeOf(v98Hdr)) return false;

            byte[] hdrB = new byte[Marshal.SizeOf(v98Hdr)];
            stream.Read(hdrB, 0, hdrB.Length);

            GCHandle handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
            v98Hdr = (Virtual98Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Virtual98Header));
            handle.Free();

            if(!v98Hdr.signature.SequenceEqual(signature)) return false;

            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.signature = \"{0}\"",
                                      StringHandlers.CToString(v98Hdr.signature, shiftjis));
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.comment = \"{0}\"",
                                      StringHandlers.CToString(v98Hdr.comment, shiftjis));
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.padding = {0}", v98Hdr.padding);
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.mbsize = {0}", v98Hdr.mbsize);
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.sectorsize = {0}", v98Hdr.sectorsize);
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.sectors = {0}", v98Hdr.sectors);
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.surfaces = {0}", v98Hdr.surfaces);
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.cylinders = {0}", v98Hdr.cylinders);
            DicConsole.DebugWriteLine("Virtual98 plugin", "v98hdr.totals = {0}", v98Hdr.totals);

            return true;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            // Even if comment is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
            Encoding shiftjis = Encoding.GetEncoding("shift_jis");

            v98Hdr = new Virtual98Header();

            if(stream.Length < Marshal.SizeOf(v98Hdr)) return false;

            byte[] hdrB = new byte[Marshal.SizeOf(v98Hdr)];
            stream.Read(hdrB, 0, hdrB.Length);

            GCHandle handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
            v98Hdr = (Virtual98Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Virtual98Header));
            handle.Free();

            ImageInfo.MediaType = MediaType.GENERIC_HDD;

            ImageInfo.ImageSize = (ulong)(stream.Length - 0xDC);
            ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.ImageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.Sectors = v98Hdr.totals;
            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.SectorSize = v98Hdr.sectorsize;
            ImageInfo.Cylinders = v98Hdr.cylinders;
            ImageInfo.Heads = v98Hdr.surfaces;
            ImageInfo.SectorsPerTrack = v98Hdr.sectors;
            ImageInfo.ImageComments = StringHandlers.CToString(v98Hdr.comment, shiftjis);

            nhdImageFilter = imageFilter;

            return true;
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
            return "Virtual98 disk image";
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

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * ImageInfo.SectorSize];

            Stream stream = nhdImageFilter.GetDataForkStream();

            // V98 are lazy allocated
            if((long)(0xDC + sectorAddress * ImageInfo.SectorSize) >= stream.Length) return buffer;

            stream.Seek((long)(0xDC + sectorAddress * ImageInfo.SectorSize), SeekOrigin.Begin);

            int toRead = (int)(length * ImageInfo.SectorSize);

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