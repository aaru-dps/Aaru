// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : RayDIM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Ray Arachelian's disk images.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class RayDim : IWritableImage
    {
        const string REGEX_SIGNATURE =
            @"Disk IMage VER (?<major>\d).(?<minor>\d) Copyright \(C\) (?<year>\d{4}) Ray Arachelian, All Rights Reserved\.";

        MemoryStream disk;
        ImageInfo    imageInfo;
        FileStream   writingStream;

        public RayDim()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Application           = "Ray Arachelian's Disk IMage",
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

        public string    Name => "Ray Arachelian's Disk IMage";
        public Guid      Id   => new Guid("F541F4E7-C1E3-4A2D-B07F-D863E87AB961");
        public ImageInfo Info => imageInfo;

        public string Format => "Ray Arachelian's Disk IMage";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < Marshal.SizeOf(typeof(RayHdr))) return false;

            byte[] buffer = new byte[Marshal.SizeOf(typeof(RayHdr))];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            RayHdr header = (RayHdr)Marshal.PtrToStructure(ftrPtr, typeof(RayHdr));
            Marshal.FreeHGlobal(ftrPtr);

            string signature = StringHandlers.CToString(header.signature);

            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.signature = {0}", signature);
            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.diskType = {0}",  header.diskType);
            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.heads = {0}",     header.heads);
            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.cylinders = {0}", header.cylinders);
            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.sectorsPerTrack = {0}",
                                      header.sectorsPerTrack);

            Regex sx = new Regex(REGEX_SIGNATURE);
            Match sm = sx.Match(signature);

            DicConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.signature matches? = {0}",
                                      sm.Success);

            return sm.Success;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < Marshal.SizeOf(typeof(RayHdr))) return false;

            byte[] buffer = new byte[Marshal.SizeOf(typeof(RayHdr))];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            RayHdr header = (RayHdr)Marshal.PtrToStructure(ftrPtr, typeof(RayHdr));
            Marshal.FreeHGlobal(ftrPtr);

            string signature = StringHandlers.CToString(header.signature);

            Regex sx = new Regex(REGEX_SIGNATURE);
            Match sm = sx.Match(signature);

            if(!sm.Success) return false;

            imageInfo.ApplicationVersion = $"{sm.Groups["major"].Value}.{sm.Groups["minor"].Value}";

            imageInfo.Cylinders       = (uint)(header.cylinders + 1);
            imageInfo.Heads           = (uint)(header.heads     + 1);
            imageInfo.SectorsPerTrack = header.sectorsPerTrack;
            imageInfo.Sectors         = imageInfo.Cylinders * imageInfo.Heads * imageInfo.SectorsPerTrack;
            imageInfo.SectorSize      = 512;

            byte[] sectors = new byte[imageInfo.SectorsPerTrack * imageInfo.SectorSize];
            disk           = new MemoryStream();

            for(int i = 0; i < imageInfo.SectorsPerTrack * imageInfo.SectorSize; i++)
            {
                stream.Read(sectors, 0, sectors.Length);
                stream.Seek(imageInfo.SectorsPerTrack, SeekOrigin.Current);
                disk.Write(sectors, 0, sectors.Length);
            }

            imageInfo.MediaType =
                Geometry.GetMediaType(((ushort)imageInfo.Cylinders, (byte)imageInfo.Heads,
                                      (ushort)imageInfo.SectorsPerTrack, 512, MediaEncoding.MFM, false));

            switch(imageInfo.MediaType)
            {
                case MediaType.NEC_525_HD when header.diskType == RayDiskTypes.Mf2hd ||
                                               header.diskType == RayDiskTypes.Mf2ed:
                    imageInfo.MediaType = MediaType.NEC_35_HD_8;
                    break;
                case MediaType.DOS_525_HD when header.diskType == RayDiskTypes.Mf2hd ||
                                               header.diskType == RayDiskTypes.Mf2ed:
                    imageInfo.MediaType = MediaType.NEC_35_HD_15;
                    break;
                case MediaType.RX50 when header.diskType == RayDiskTypes.Md2dd || header.diskType == RayDiskTypes.Md2hd:
                    imageInfo.MediaType = MediaType.ATARI_35_SS_DD;
                    break;
            }

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            return true;
        }

        public bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            return null;
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                   out                                   List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out                                               List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifyMediaImage()
        {
            return null;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            disk.Seek((long)(sectorAddress    * imageInfo.SectorSize), SeekOrigin.Begin);
            disk.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

            return buffer;
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadDiskTag(MediaTagType tag)
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

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public IEnumerable<MediaTagType>  SupportedMediaTags  => new MediaTagType[] { };
        public IEnumerable<SectorTagType> SupportedSectorTags => new SectorTagType[] { };
        // TODO: Test with real hardware to see real supported media
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.Apricot_35, MediaType.ATARI_35_DS_DD, MediaType.ATARI_35_DS_DD_11, MediaType.ATARI_35_SS_DD,
                MediaType.ATARI_35_SS_DD_11, MediaType.DMF, MediaType.DMF_82, MediaType.DOS_35_DS_DD_8,
                MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED, MediaType.DOS_35_HD, MediaType.DOS_35_SS_DD_8,
                MediaType.DOS_35_SS_DD_9, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD,
                MediaType.DOS_525_SS_DD_8, MediaType.DOS_525_SS_DD_9, MediaType.FDFORMAT_35_DD,
                MediaType.FDFORMAT_35_HD, MediaType.FDFORMAT_525_DD, MediaType.FDFORMAT_525_HD, MediaType.RX50,
                MediaType.XDF_35, MediaType.XDF_525
            };
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new(string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".dim"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(sectorSize == 512)
            {
                ErrorMessage = "Unsupported sector size";
                return false;
            }

            if(sectors > 255 * 255 * 255)
            {
                ErrorMessage = $"Too many sectors";
                return false;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            (ushort cylinders, byte heads, ushort sectorsPerTrack, uint bytesPerSector, MediaEncoding encoding, bool
                variableSectorsPerTrack, MediaType type) geometry = Geometry.GetGeometry(mediaType);
            imageInfo                                             = new ImageInfo
            {
                MediaType       = mediaType,
                SectorSize      = sectorSize,
                Sectors         = sectors,
                Cylinders       = geometry.cylinders,
                Heads           = geometry.heads,
                SectorsPerTrack = geometry.sectorsPerTrack
            };

            try { writingStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None); }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            IsWriting    = true;
            ErrorMessage = null;
            return true;
        }

        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            ErrorMessage = "Writing media tags is not supported.";
            return false;
        }

        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(data.Length != 512)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress >= imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            writingStream.Seek((long)((ulong)Marshal.SizeOf(typeof(RayHdr)) + sectorAddress * imageInfo.SectorSize),
                               SeekOrigin.Begin);
            writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";
            return true;
        }

        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(data.Length % 512 != 0)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress + length > imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            writingStream.Seek((long)((ulong)Marshal.SizeOf(typeof(RayHdr)) + sectorAddress * imageInfo.SectorSize),
                               SeekOrigin.Begin);
            writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";
            return true;
        }

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";
            return false;
        }

        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";
            return false;
        }

        public bool SetTracks(List<Track> tracks)
        {
            ErrorMessage = "Unsupported feature";
            return false;
        }

        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";
                return false;
            }

            string headerSignature =
                $"Disk IMage VER 1.0 Copyright (C) ({DateTime.Now.Year:D4}) Ray Arachelian, All Rights Reserved.  DIC ";
            RayHdr header = new RayHdr
            {
                signature       = Encoding.ASCII.GetBytes(headerSignature),
                cylinders       = (byte)imageInfo.Cylinders,
                diskType        = RayDiskTypes.Mf2ed,
                heads           = (byte)imageInfo.Heads,
                sectorsPerTrack = (byte)imageInfo.SectorsPerTrack
            };
            header.signature[0x4A] = 0x00;

            byte[] hdr    = new byte[Marshal.SizeOf(header)];
            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            Marshal.StructureToPtr(header, hdrPtr, true);
            Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
            Marshal.FreeHGlobal(hdrPtr);

            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(hdr, 0, hdr.Length);

            writingStream.Flush();
            writingStream.Close();
            IsWriting = false;

            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            imageInfo.Comments = metadata.Comments;
            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RayHdr
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public byte[]       signature;
            public RayDiskTypes diskType;
            public byte         cylinders;
            public byte         sectorsPerTrack;
            public byte         heads;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        enum RayDiskTypes : byte
        {
            Md2dd = 1,
            Md2hd = 2,
            Mf2dd = 3,
            Mf2hd = 4,
            Mf2ed = 5
        }
    }
}