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
    public class Virtual98 : IMediaImage
    {
        readonly byte[] signature = {0x56, 0x48, 0x44, 0x31, 0x2E, 0x30, 0x30, 0x00};
        ImageInfo imageInfo;
        IFilter nhdImageFilter;

        Virtual98Header v98Hdr;

        public Virtual98()
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

        public string Name => "Virtual98 Disk Image";
        public Guid Id => new Guid("C0CDE13D-04D0-4913-8740-AFAA44D0A107");

        public string Format => "Virtual98 disk image";

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

        public bool Open(IFilter imageFilter)
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

            imageInfo.MediaType = MediaType.GENERIC_HDD;

            imageInfo.ImageSize = (ulong)(stream.Length - 0xDC);
            imageInfo.CreationTime = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors = v98Hdr.totals;
            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            imageInfo.SectorSize = v98Hdr.sectorsize;
            imageInfo.Cylinders = v98Hdr.cylinders;
            imageInfo.Heads = v98Hdr.surfaces;
            imageInfo.SectorsPerTrack = v98Hdr.sectors;
            imageInfo.Comments = StringHandlers.CToString(v98Hdr.comment, shiftjis);

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

            // V98 are lazy allocated
            if((long)(0xDC + sectorAddress * imageInfo.SectorSize) >= stream.Length) return buffer;

            stream.Seek((long)(0xDC + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);

            int toRead = (int)(length * imageInfo.SectorSize);

            if(toRead + stream.Position > stream.Length) toRead = (int)(stream.Length - stream.Position);

            stream.Read(buffer, 0, toRead);

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
    }
}