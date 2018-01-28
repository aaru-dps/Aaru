// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MaxiDisk.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages MaxiDisk images.
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
using DiscImageChef.Helpers;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public class MaxiDisk : IWritableImage
    {
        /// <summary>Disk image file</summary>
        IFilter    hdkImageFilter;
        ImageInfo  imageInfo;
        FileStream writingStream;

        public MaxiDisk()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Application           = "MAXI Disk",
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

        public ImageInfo Info => imageInfo;

        public string Name => "MAXI Disk image";
        public Guid   Id   => new Guid("D27D924A-7034-466E-ADE1-B81EF37E469E");

        public string Format => "MAXI Disk";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 8) return false;

            byte[] buffer = new byte[8];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            HdkHeader tmpHeader = (HdkHeader)Marshal.PtrToStructure(ftrPtr, typeof(HdkHeader));
            Marshal.FreeHGlobal(ftrPtr);

            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.unknown = {0}",        tmpHeader.unknown);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.diskType = {0}",       tmpHeader.diskType);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.heads = {0}",          tmpHeader.heads);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.cylinders = {0}",      tmpHeader.cylinders);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.bytesPerSector = {0}", tmpHeader.bytesPerSector);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.sectorsPerTrack = {0}",
                                      tmpHeader.sectorsPerTrack);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.unknown2 = {0}", tmpHeader.unknown2);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.unknown3 = {0}", tmpHeader.unknown3);

            // This is hardcoded
            // But its possible values are unknown...
            //if(tmp_header.diskType > 11)
            //    return false;

            // Only floppies supported
            if(tmpHeader.heads == 0 || tmpHeader.heads > 2) return false;

            // No floppies with more than this?
            if(tmpHeader.cylinders > 90) return false;

            // Maximum supported bps is 16384
            if(tmpHeader.bytesPerSector > 7) return false;

            int expectedFileSize = tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack *
                                   (128 << tmpHeader.bytesPerSector) + 8;

            return expectedFileSize == stream.Length;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 8) return false;

            byte[] buffer = new byte[8];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            HdkHeader tmpHeader = (HdkHeader)Marshal.PtrToStructure(ftrPtr, typeof(HdkHeader));
            Marshal.FreeHGlobal(ftrPtr);

            // This is hardcoded
            // But its possible values are unknown...
            //if(tmp_header.diskType > 11)
            //    return false;

            // Only floppies supported
            if(tmpHeader.heads == 0 || tmpHeader.heads > 2) return false;

            // No floppies with more than this?
            if(tmpHeader.cylinders > 90) return false;

            // Maximum supported bps is 16384
            if(tmpHeader.bytesPerSector > 7) return false;

            int expectedFileSize = tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack *
                                   (128 << tmpHeader.bytesPerSector) + 8;

            if(expectedFileSize != stream.Length) return false;

            imageInfo.Cylinders       = tmpHeader.cylinders;
            imageInfo.Heads           = tmpHeader.heads;
            imageInfo.SectorsPerTrack = tmpHeader.sectorsPerTrack;
            imageInfo.Sectors         = (ulong)(tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack);
            imageInfo.SectorSize      = (uint)(128 << tmpHeader.bytesPerSector);

            hdkImageFilter = imageFilter;

            imageInfo.ImageSize            = (ulong)(stream.Length - 8);
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();

            imageInfo.MediaType =
                Geometry.GetMediaType(((ushort)imageInfo.Cylinders, (byte)imageInfo.Heads,
                                      (ushort)imageInfo.SectorsPerTrack, imageInfo.SectorSize, MediaEncoding.MFM, false
                                      ));

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

        public List<DumpHardwareType> DumpHardware => null;
        public CICMMetadataType       CicmMetadata => null;

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

            Stream stream = hdkImageFilter.GetDataForkStream();
            stream.Seek((long)(8 + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
            stream.Read(buffer, 0, (int)(length  * imageInfo.SectorSize));

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
        public IEnumerable<string> KnownExtensions => new[] {".hdk"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(CountBits.Count(sectorSize) != 1 || sectorSize > 16384)
            {
                ErrorMessage = "Unsupported sector size";
                return false;
            }

            if(sectors > 90 * 2 * 255)
            {
                ErrorMessage = "Too many sectors";
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

            if(imageInfo.Cylinders > 90)
            {
                ErrorMessage = "Too many cylinders";
                return false;
            }

            if(imageInfo.Heads > 2)
            {
                ErrorMessage = "Too many heads";
                return false;
            }

            try { writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
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

            writingStream.Seek((long)((ulong)Marshal.SizeOf(typeof(HdkHeader)) + sectorAddress * imageInfo.SectorSize),
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

            writingStream.Seek((long)((ulong)Marshal.SizeOf(typeof(HdkHeader)) + sectorAddress * imageInfo.SectorSize),
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

            HdkHeader header = new HdkHeader
            {
                diskType        = (byte)HdkDiskTypes.Dos2880,
                cylinders       = (byte)imageInfo.Cylinders,
                heads           = (byte)imageInfo.Heads,
                sectorsPerTrack = (byte)imageInfo.SectorsPerTrack
            };

            for(uint i = imageInfo.SectorSize / 128; i > 1;)
            {
                header.bytesPerSector++;
                i >>= 1;
            }

            byte[] hdr    = new byte[Marshal.SizeOf(header)];
            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            Marshal.StructureToPtr(header, hdrPtr, true);
            Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
            Marshal.FreeHGlobal(hdrPtr);

            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(hdr, 0, hdr.Length);

            writingStream.Flush();
            writingStream.Close();

            IsWriting    = false;
            ErrorMessage = "";
            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            if(cylinders > 90)
            {
                ErrorMessage = "Too many cylinders.";
                return false;
            }

            if(heads > 2)
            {
                ErrorMessage = "Too many heads.";
                return false;
            }

            if(sectorsPerTrack > byte.MaxValue)
            {
                ErrorMessage = "Too many sectors per track.";
                return false;
            }

            imageInfo.SectorsPerTrack = sectorsPerTrack;
            imageInfo.Heads           = heads;
            imageInfo.Cylinders       = cylinders;

            return true;
        }

        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            ErrorMessage = "Unsupported feature";
            return false;
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            ErrorMessage = "Unsupported feature";
            return false;
        }

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware)
        {
            // Not supported
            return false;
        }

        public bool SetCicmMetadata(CICMMetadataType metadata)
        {
            // Not supported
            return false;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HdkHeader
        {
            public byte unknown;
            public byte diskType;
            public byte heads;
            public byte cylinders;
            public byte bytesPerSector;
            public byte sectorsPerTrack;
            public byte unknown2;
            public byte unknown3;
        }

        enum HdkDiskTypes : byte
        {
            Dos360   = 0,
            Maxi420  = 1,
            Dos720   = 2,
            Maxi800  = 3,
            Dos1200  = 4,
            Maxi1400 = 5,
            Dos1440  = 6,
            Mac1440  = 7,
            Maxi1600 = 8,
            Dmf      = 9,
            Dos2880  = 10,
            Maxi3200 = 11
        }
    }
}