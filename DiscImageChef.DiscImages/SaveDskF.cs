// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SaveDskF.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages IBM SaveDskF disk images.
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
    public class SaveDskF : IWritableImage
    {
        const ushort SDF_MAGIC_OLD        = 0x58AA;
        const ushort SDF_MAGIC            = 0x59AA;
        const ushort SDF_MAGIC_COMPRESSED = 0x5AAA;
        uint         calculatedChk;
        byte[]       decodedDisk;

        SaveDskFHeader header;
        ImageInfo      imageInfo;
        FileStream     writingStream;

        public SaveDskF()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Version               = null,
                Application           = null,
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

        public string    Name => "IBM SaveDskF";
        public Guid      Id   => new Guid("288CE058-1A51-4034-8C45-5A256CAE1461");
        public ImageInfo Info => imageInfo;

        public string Format => "IBM SaveDskF";

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
            if(stream.Length < 41) return false;

            byte[] hdr = new byte[40];
            stream.Read(hdr, 0, 40);

            header        = new SaveDskFHeader();
            IntPtr hdrPtr = Marshal.AllocHGlobal(40);
            Marshal.Copy(hdr, 0, hdrPtr, 40);
            header = (SaveDskFHeader)Marshal.PtrToStructure(hdrPtr, typeof(SaveDskFHeader));
            Marshal.FreeHGlobal(hdrPtr);

            return (header.magic        == SDF_MAGIC || header.magic           == SDF_MAGIC_COMPRESSED ||
                    header.magic        == SDF_MAGIC_OLD) && header.fatCopies  <= 2 && header.padding == 0 &&
                   header.commentOffset < stream.Length   && header.dataOffset < stream.Length;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdr = new byte[40];

            stream.Read(hdr, 0, 40);
            header        = new SaveDskFHeader();
            IntPtr hdrPtr = Marshal.AllocHGlobal(40);
            Marshal.Copy(hdr, 0, hdrPtr, 40);
            header = (SaveDskFHeader)Marshal.PtrToStructure(hdrPtr, typeof(SaveDskFHeader));
            Marshal.FreeHGlobal(hdrPtr);

            DicConsole.DebugWriteLine("SaveDskF plugin", "header.magic = 0x{0:X4}",      header.magic);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.mediaType = 0x{0:X2}",  header.mediaType);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.sectorSize = {0}",      header.sectorSize);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.clusterMask = {0}",     header.clusterMask);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.clusterShift = {0}",    header.clusterShift);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.reservedSectors = {0}", header.reservedSectors);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.fatCopies = {0}",       header.fatCopies);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.rootEntries = {0}",     header.rootEntries);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.firstCluster = {0}",    header.firstCluster);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.clustersCopied = {0}",  header.clustersCopied);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.sectorsPerFat = {0}",   header.sectorsPerFat);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.checksum = 0x{0:X8}",   header.checksum);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.cylinders = {0}",       header.cylinders);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.heads = {0}",           header.heads);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.sectorsPerTrack = {0}", header.sectorsPerTrack);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.padding = {0}",         header.padding);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.sectorsCopied = {0}",   header.sectorsCopied);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.commentOffset = {0}",   header.commentOffset);
            DicConsole.DebugWriteLine("SaveDskF plugin", "header.dataOffset = {0}",      header.dataOffset);

            if(header.dataOffset == 0 && header.magic == SDF_MAGIC_OLD) header.dataOffset = 512;

            byte[] cmt = new byte[header.dataOffset - header.commentOffset];
            stream.Seek(header.commentOffset, SeekOrigin.Begin);
            stream.Read(cmt, 0, cmt.Length);
            if(cmt.Length > 1) imageInfo.Comments = StringHandlers.CToString(cmt, Encoding.GetEncoding("ibm437"));

            calculatedChk = 0;
            stream.Seek(0, SeekOrigin.Begin);

            int b;
            do
            {
                b                        =  stream.ReadByte();
                if(b >= 0) calculatedChk += (uint)b;
            }
            while(b >= 0);

            DicConsole.DebugWriteLine("SaveDskF plugin", "Calculated checksum = 0x{0:X8}, {1}", calculatedChk,
                                      calculatedChk == header.checksum);

            imageInfo.Application          = "SaveDskF";
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = imageFilter.GetFilename();
            imageInfo.ImageSize            = (ulong)(stream.Length - header.dataOffset);
            imageInfo.Sectors              = (ulong)(header.sectorsPerTrack * header.heads * header.cylinders);
            imageInfo.SectorSize           = header.sectorSize;

            imageInfo.MediaType =
                Geometry.GetMediaType((header.cylinders, (byte)header.heads, header.sectorsPerTrack, header.sectorSize,
                                      MediaEncoding.MFM, false));

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            DicConsole.VerboseWriteLine("SaveDskF image contains a disk of type {0}", imageInfo.MediaType);
            if(!string.IsNullOrEmpty(imageInfo.Comments))
                DicConsole.VerboseWriteLine("SaveDskF comments: {0}", imageInfo.Comments);

            // TODO: Support compressed images
            if(header.magic == SDF_MAGIC_COMPRESSED)
                throw new
                    FeatureSupportedButNotImplementedImageException("Compressed SaveDskF images are not supported.");

            // SaveDskF only ommits ending clusters, leaving no gaps behind, so reading all data we have...
            stream.Seek(header.dataOffset, SeekOrigin.Begin);
            decodedDisk = new byte[imageInfo.Sectors * imageInfo.SectorSize];
            stream.Read(decodedDisk, 0, (int)(stream.Length - header.dataOffset));

            imageInfo.Cylinders       = header.cylinders;
            imageInfo.Heads           = header.heads;
            imageInfo.SectorsPerTrack = header.sectorsPerTrack;

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
            return calculatedChk == header.checksum;
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

            Array.Copy(decodedDisk, (int)sectorAddress * imageInfo.SectorSize, buffer, 0,
                       length                          * imageInfo.SectorSize);

            return buffer;
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
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
                MediaType.ACORN_35_DS_DD, MediaType.ACORN_35_DS_HD, MediaType.Apricot_35, MediaType.ATARI_35_DS_DD,
                MediaType.ATARI_35_DS_DD_11, MediaType.ATARI_35_SS_DD, MediaType.ATARI_35_SS_DD_11, MediaType.DMF,
                MediaType.DMF_82, MediaType.DOS_35_DS_DD_8, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED,
                MediaType.DOS_35_HD, MediaType.DOS_35_SS_DD_8, MediaType.DOS_35_SS_DD_9, MediaType.DOS_525_DS_DD_8,
                MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_525_SS_DD_8, MediaType.DOS_525_SS_DD_9,
                MediaType.FDFORMAT_35_DD, MediaType.FDFORMAT_35_HD, MediaType.FDFORMAT_525_DD,
                MediaType.FDFORMAT_525_HD, MediaType.RX50, MediaType.XDF_35, MediaType.XDF_525
            };
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new(string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".dsk"};

        public bool   IsWriting    { get; private set; }
        public string ErrorMessage { get; private set; }

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

            if(data.Length != imageInfo.SectorSize)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress >= imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            writingStream.Seek((long)(512 + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
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

            if(data.Length % imageInfo.SectorSize != 0)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress + length > imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            writingStream.Seek((long)(512 + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
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

            if(!string.IsNullOrWhiteSpace(imageInfo.Comments))
            {
                byte[] commentsBytes = Encoding.GetEncoding("ibm437").GetBytes(imageInfo.Comments);
                header.commentOffset = (ushort)Marshal.SizeOf(header);
                writingStream.Seek(header.commentOffset, SeekOrigin.Begin);
                writingStream.Write(commentsBytes, 0,
                                    commentsBytes.Length >= 512 - header.commentOffset
                                        ? 512                   - header.commentOffset
                                        : commentsBytes.Length);
            }

            byte[] hdr    = new byte[Marshal.SizeOf(header)];
            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            Marshal.StructureToPtr(header, hdrPtr, true);
            Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
            Marshal.FreeHGlobal(hdrPtr);

            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(hdr, 0, hdr.Length);

            header.checksum = 0;
            writingStream.Seek(0, SeekOrigin.Begin);

            int b;
            do
            {
                b                          =  writingStream.ReadByte();
                if(b >= 0) header.checksum += (uint)b;
            }
            while(b >= 0);

            hdr    = new byte[Marshal.SizeOf(header)];
            hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
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

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(sectorSize == 0)
            {
                ErrorMessage = "Unsupported sector size";
                return false;
            }

            if(sectors > ushort.MaxValue)
            {
                ErrorMessage = $"Too many sectors";
                return false;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

            try { writingStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None); }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            (ushort cylinders, byte heads, ushort sectorsPerTrack, uint bytesPerSector, MediaEncoding encoding, bool
                variableSectorsPerTrack, MediaType type) geometry = Geometry.GetGeometry(mediaType);

            header = new SaveDskFHeader
            {
                cylinders       = geometry.cylinders,
                dataOffset      = 512,
                heads           = geometry.heads,
                magic           = SDF_MAGIC,
                sectorsCopied   = (ushort)sectors,
                sectorsPerTrack = geometry.sectorsPerTrack,
                sectorSize      = (ushort)sectorSize
            };

            IsWriting    = true;
            ErrorMessage = null;
            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            // Geometry is set by media type
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SaveDskFHeader
        {
            /// <summary>0x00 magic number</summary>
            public ushort magic;
            /// <summary>0x02 media type from FAT</summary>
            public ushort mediaType;
            /// <summary>0x04 bytes per sector</summary>
            public ushort sectorSize;
            /// <summary>0x06 sectors per cluster - 1</summary>
            public byte clusterMask;
            /// <summary>0x07 log2(cluster / sector)</summary>
            public byte clusterShift;
            /// <summary>0x08 reserved sectors</summary>
            public ushort reservedSectors;
            /// <summary>0x0A copies of FAT</summary>
            public byte fatCopies;
            /// <summary>0x0B entries in root directory</summary>
            public ushort rootEntries;
            /// <summary>0x0D first cluster</summary>
            public ushort firstCluster;
            /// <summary>0x0F clusters present in image</summary>
            public ushort clustersCopied;
            /// <summary>0x11 sectors per FAT</summary>
            public byte sectorsPerFat;
            /// <summary>0x12 sector number of root directory</summary>
            public ushort rootDirectorySector;
            /// <summary>0x14 sum of all image bytes</summary>
            public uint checksum;
            /// <summary>0x18 cylinders</summary>
            public ushort cylinders;
            /// <summary>0x1A heads</summary>
            public ushort heads;
            /// <summary>0x1C sectors per track</summary>
            public ushort sectorsPerTrack;
            /// <summary>0x1E always zero</summary>
            public uint padding;
            /// <summary>0x22 sectors present in image</summary>
            public ushort sectorsCopied;
            /// <summary>0x24 offset to comment</summary>
            public ushort commentOffset;
            /// <summary>0x26 offset to data</summary>
            public ushort dataOffset;
        }
    }
}