// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : NHDr0.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages NHD r0 disk images.
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
    // Info from http://www.geocities.jp/t98next/nhdr0.txt
    public class Nhdr0 : IMediaImage
    {
        readonly byte[] signature =
            {0x54, 0x39, 0x38, 0x48, 0x44, 0x44, 0x49, 0x4D, 0x41, 0x47, 0x45, 0x2E, 0x52, 0x30, 0x00};
        ImageInfo imageInfo;

        Nhdr0Header nhdhdr;
        IFilter nhdImageFilter;

        public Nhdr0()
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

        public string Name => "T98-Next NHD r0 Disk Image";
        public Guid Id => new Guid("6ECACD0A-8F4D-4465-8815-AEA000D370E3");
        public ImageInfo Info => imageInfo;

        public string Format => "NHDr0 disk image";

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
            // Even if comment is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
            Encoding shiftjis = Encoding.GetEncoding("shift_jis");

            nhdhdr = new Nhdr0Header();

            if(stream.Length < Marshal.SizeOf(nhdhdr)) return false;

            byte[] hdrB = new byte[Marshal.SizeOf(nhdhdr)];
            stream.Read(hdrB, 0, hdrB.Length);

            GCHandle handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
            nhdhdr = (Nhdr0Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Nhdr0Header));
            handle.Free();

            if(!nhdhdr.szFileID.SequenceEqual(signature)) return false;

            DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.szFileID = \"{0}\"",
                                      StringHandlers.CToString(nhdhdr.szFileID, shiftjis));
            DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.reserved1 = {0}", nhdhdr.reserved1);
            DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.szComment = \"{0}\"",
                                      StringHandlers.CToString(nhdhdr.szComment, shiftjis));
            DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.dwHeadSize = {0}", nhdhdr.dwHeadSize);
            DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.dwCylinder = {0}", nhdhdr.dwCylinder);
            DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.wHead = {0}", nhdhdr.wHead);
            DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.wSect = {0}", nhdhdr.wSect);
            DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.wSectLen = {0}", nhdhdr.wSectLen);

            return true;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            // Even if comment is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
            Encoding shiftjis = Encoding.GetEncoding("shift_jis");

            nhdhdr = new Nhdr0Header();

            if(stream.Length < Marshal.SizeOf(nhdhdr)) return false;

            byte[] hdrB = new byte[Marshal.SizeOf(nhdhdr)];
            stream.Read(hdrB, 0, hdrB.Length);

            GCHandle handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
            nhdhdr = (Nhdr0Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Nhdr0Header));
            handle.Free();

            imageInfo.MediaType = MediaType.GENERIC_HDD;

            imageInfo.ImageSize = (ulong)(stream.Length - nhdhdr.dwHeadSize);
            imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors = (ulong)(nhdhdr.dwCylinder * nhdhdr.wHead * nhdhdr.wSect);
            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            imageInfo.SectorSize = (uint)nhdhdr.wSectLen;
            imageInfo.Cylinders = (uint)nhdhdr.dwCylinder;
            imageInfo.Heads = (uint)nhdhdr.wHead;
            imageInfo.SectorsPerTrack = (uint)nhdhdr.wSect;
            imageInfo.Comments = StringHandlers.CToString(nhdhdr.szComment, shiftjis);

            nhdImageFilter = imageFilter;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            Stream stream = nhdImageFilter.GetDataForkStream();

            stream.Seek((long)((ulong)nhdhdr.dwHeadSize + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

            return buffer;
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
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

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Nhdr0Header
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)] public byte[] szFileID;
            public byte reserved1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)] public byte[] szComment;
            public int dwHeadSize;
            public int dwCylinder;
            public short wHead;
            public short wSect;
            public short wSectLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] reserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xE0)] public byte[] reserved3;
        }
    }
}