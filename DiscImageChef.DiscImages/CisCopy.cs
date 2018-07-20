// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CisCopy.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages CisCopy disk images.
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
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using Schemas;

namespace DiscImageChef.DiscImages
{
    /* This is a very simple format created by a German application called CisCopy, aka CCOPY.EXE, with extension .DCF.
     * First byte indicates the floppy type, limited to standard formats.
     * Indeed if the floppy is not DOS formatted, user must choose from the list of supported formats manually.
     * Next 80 bytes (for 5.25" DD disks) or 160 bytes (for 5.25" HD and 3.5" disks) indicate if a track has been copied
     * or not.
     * It offers three copy methods:
     * a) All, copies all tracks
     * b) FAT, copies all tracks which contain sectors marked as sued by FAT
     * c) "Belelung" similarly to FAT. On some disk tests FAT cuts data, while belelung does not.
     * Finally, next byte indicates compression:
     * 0) No compression
     * 1) Normal compression, algorithm unknown
     * 2) High compression, algorithm unknown
     * Then the data for whole tracks follow.
     */
    public class CisCopy : IWritableImage
    {
        byte[]     decodedDisk;
        ImageInfo  imageInfo;
        long       writingOffset;
        FileStream writingStream;

        public CisCopy()
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

        public string Name => "CisCopy Disk Image (DC-File)";
        public Guid   Id   => new Guid("EDF20CC7-6012-49E2-9E92-663A53E42130");

        public string Format => "CisCopy";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        public ImageInfo Info => imageInfo;

        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            DiskType type = (DiskType)stream.ReadByte();
            byte     tracks;

            switch(type)
            {
                case DiskType.MD1DD8:
                case DiskType.MD1DD:
                case DiskType.MD2DD8:
                case DiskType.MD2DD:
                    tracks = 80;
                    break;
                case DiskType.MF2DD:
                case DiskType.MD2HD:
                case DiskType.MF2HD:
                    tracks = 160;
                    break;
                default: return false;
            }

            byte[] trackBytes = new byte[tracks];
            stream.Read(trackBytes, 0, tracks);

            for(int i = 0; i < tracks; i++)
                if(trackBytes[i] != (byte)TrackType.Copied && trackBytes[i] != (byte)TrackType.Omitted &&
                   trackBytes[i] != (byte)TrackType.OmittedAlternate)
                    return false;

            Compression cmpr = (Compression)stream.ReadByte();

            if(cmpr != Compression.None && cmpr != Compression.Normal && cmpr != Compression.High) return false;

            switch(type)
            {
                case DiskType.MD1DD8:
                    if(stream.Length > 40 * 1 * 8 * 512 + 82) return false;

                    break;
                case DiskType.MD1DD:
                    if(stream.Length > 40 * 1 * 9 * 512 + 82) return false;

                    break;
                case DiskType.MD2DD8:
                    if(stream.Length > 40 * 2 * 8 * 512 + 82) return false;

                    break;
                case DiskType.MD2DD:
                    if(stream.Length > 40 * 2 * 9 * 512 + 82) return false;

                    break;
                case DiskType.MF2DD:
                    if(stream.Length > 80 * 2 * 9 * 512 + 162) return false;

                    break;
                case DiskType.MD2HD:
                    if(stream.Length > 80 * 2 * 15 * 512 + 162) return false;

                    break;
                case DiskType.MF2HD:
                    if(stream.Length > 80 * 2 * 18 * 512 + 162) return false;

                    break;
            }

            return true;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            DiskType type = (DiskType)stream.ReadByte();
            byte     tracks;

            switch(type)
            {
                case DiskType.MD1DD8:
                case DiskType.MD1DD:
                case DiskType.MD2DD8:
                case DiskType.MD2DD:
                    tracks = 80;
                    break;
                case DiskType.MF2DD:
                case DiskType.MD2HD:
                case DiskType.MF2HD:
                    tracks = 160;
                    break;
                default: throw new ImageNotSupportedException($"Incorrect disk type {(byte)type}");
            }

            byte[] trackBytes = new byte[tracks];
            stream.Read(trackBytes, 0, tracks);

            Compression cmpr = (Compression)stream.ReadByte();

            if(cmpr != Compression.None)
                throw new FeatureSupportedButNotImplementedImageException("Compressed images are not supported.");

            int tracksize = 0;

            switch(type)
            {
                case DiskType.MD1DD8:
                case DiskType.MD2DD8:
                    tracksize = 8 * 512;
                    break;
                case DiskType.MD1DD:
                case DiskType.MD2DD:
                case DiskType.MF2DD:
                    tracksize = 9 * 512;
                    break;
                case DiskType.MD2HD:
                    tracksize = 15 * 512;
                    break;
                case DiskType.MF2HD:
                    tracksize = 18 * 512;
                    break;
            }

            int headstep                                                   = 1;
            if(type == DiskType.MD1DD || type == DiskType.MD1DD8) headstep = 2;

            MemoryStream decodedImage = new MemoryStream();

            for(int i = 0; i < tracks; i += headstep)
            {
                byte[] track = new byte[tracksize];

                if((TrackType)trackBytes[i] == TrackType.Copied) stream.Read(track, 0, tracksize);
                else ArrayHelpers.ArrayFill(track, (byte)0xF6);

                decodedImage.Write(track, 0, tracksize);
            }

            imageInfo.Application          = "CisCopy";
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = imageFilter.GetFilename();
            imageInfo.ImageSize            = (ulong)(stream.Length - 2 - trackBytes.Length);
            imageInfo.SectorSize           = 512;

            switch(type)
            {
                case DiskType.MD1DD8:
                    imageInfo.MediaType       = MediaType.DOS_525_SS_DD_8;
                    imageInfo.Sectors         = 40 * 1 * 8;
                    imageInfo.Heads           = 1;
                    imageInfo.Cylinders       = 40;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case DiskType.MD2DD8:
                    imageInfo.MediaType       = MediaType.DOS_525_DS_DD_8;
                    imageInfo.Sectors         = 40 * 2 * 8;
                    imageInfo.Heads           = 2;
                    imageInfo.Cylinders       = 40;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case DiskType.MD1DD:
                    imageInfo.MediaType       = MediaType.DOS_525_SS_DD_9;
                    imageInfo.Sectors         = 40 * 1 * 9;
                    imageInfo.Heads           = 1;
                    imageInfo.Cylinders       = 40;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case DiskType.MD2DD:
                    imageInfo.MediaType       = MediaType.DOS_525_DS_DD_9;
                    imageInfo.Sectors         = 40 * 2 * 9;
                    imageInfo.Heads           = 2;
                    imageInfo.Cylinders       = 40;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case DiskType.MF2DD:
                    imageInfo.MediaType       = MediaType.DOS_35_DS_DD_9;
                    imageInfo.Sectors         = 80 * 2 * 9;
                    imageInfo.Heads           = 2;
                    imageInfo.Cylinders       = 80;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case DiskType.MD2HD:
                    imageInfo.MediaType       = MediaType.DOS_525_HD;
                    imageInfo.Sectors         = 80 * 2 * 15;
                    imageInfo.Heads           = 2;
                    imageInfo.Cylinders       = 80;
                    imageInfo.SectorsPerTrack = 15;
                    break;
                case DiskType.MF2HD:
                    imageInfo.MediaType       = MediaType.DOS_35_HD;
                    imageInfo.Sectors         = 80 * 2 * 18;
                    imageInfo.Heads           = 2;
                    imageInfo.Cylinders       = 80;
                    imageInfo.SectorsPerTrack = 18;
                    break;
            }

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            decodedDisk            = decodedImage.ToArray();

            decodedImage.Close();

            DicConsole.VerboseWriteLine("CisCopy image contains a disk of type {0}", imageInfo.MediaType);

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

        public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
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
                MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_DS_DD_9,
                MediaType.DOS_525_HD, MediaType.DOS_525_SS_DD_8, MediaType.DOS_525_SS_DD_9
            };
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".dcf"};

        public bool   IsWriting    { get; private set; }
        public string ErrorMessage { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(sectorSize != 512)
            {
                ErrorMessage = "Unsupported sector size";
                return false;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

            try { writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            DiskType diskType;
            switch(mediaType)
            {
                case MediaType.DOS_35_DS_DD_9:
                    diskType = DiskType.MF2DD;
                    break;
                case MediaType.DOS_35_HD:
                    diskType = DiskType.MF2HD;
                    break;
                case MediaType.DOS_525_DS_DD_8:
                    diskType = DiskType.MD2DD8;
                    break;
                case MediaType.DOS_525_DS_DD_9:
                    diskType = DiskType.MD2DD;
                    break;
                case MediaType.DOS_525_HD:
                    diskType = DiskType.MD2HD;
                    break;
                case MediaType.DOS_525_SS_DD_8:
                    diskType = DiskType.MD1DD8;
                    break;
                case MediaType.DOS_525_SS_DD_9:
                    diskType = DiskType.MD1DD;
                    break;
                default:
                    ErrorMessage = $"Unsupport media format {mediaType}";
                    return false;
            }

            writingStream.WriteByte((byte)diskType);

            byte tracks = 0;

            switch(diskType)
            {
                case DiskType.MD1DD8:
                case DiskType.MD1DD:
                case DiskType.MD2DD8:
                case DiskType.MD2DD:
                    tracks = 80;
                    break;
                case DiskType.MF2DD:
                case DiskType.MD2HD:
                case DiskType.MF2HD:
                    tracks = 160;
                    break;
            }

            int headstep                                                           = 1;
            if(diskType == DiskType.MD1DD || diskType == DiskType.MD1DD8) headstep = 2;

            for(int i = 0; i < tracks; i += headstep)
            {
                writingStream.WriteByte((byte)TrackType.Copied);
                if(headstep == 2) writingStream.WriteByte(0);
            }

            writingStream.WriteByte((byte)Compression.None);
            writingOffset = writingStream.Position;

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

            writingStream.Seek(writingOffset + (long)(sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
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

            writingStream.Seek(writingOffset + (long)(sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
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
            // Geometry is not stored in image
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

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        enum DiskType : byte
        {
            MD1DD8 = 1,
            MD1DD  = 2,
            MD2DD8 = 3,
            MD2DD  = 4,
            MF2DD  = 5,
            MD2HD  = 6,
            MF2HD  = 7
        }

        enum Compression : byte
        {
            None   = 0,
            Normal = 1,
            High   = 2
        }

        enum TrackType : byte
        {
            Copied           = 0x4C,
            Omitted          = 0xFA,
            OmittedAlternate = 0xFE
        }
    }
}