// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : BLU.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Basic Lisa Utility disk images.
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
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Decoders;
using Schemas;
using Version = DiscImageChef.CommonTypes.Interop.Version;

namespace DiscImageChef.DiscImages
{
    public class Blu : IWritableImage
    {
        const string PROFILE_NAME   = "PROFILE      ";
        const string PROFILE10_NAME = "PROFILE 10   ";
        const string WIDGET_NAME    = "WIDGET-10    ";
        const string PRIAM_NAME     = "PRIAMDTATOWER";
        IFilter      bluImageFilter;
        int          bptag;

        BluHeader  imageHeader;
        ImageInfo  imageInfo;
        FileStream writingStream;

        public Blu()
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

        public ImageInfo Info => imageInfo;

        public string Name => "Basic Lisa Utility";
        public Guid   Id   => new Guid("A153E2F8-4235-432D-9A7F-20807B0BCD74");

        public string Format => "Basic Lisa Utility";

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

            if(stream.Length < 0x200) return false;

            byte[] header = new byte[0x17];
            stream.Read(header, 0, 0x17);

            BluHeader tmpHdr = new BluHeader {DeviceName = new byte[0x0D]};
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            Array.Copy(header, 0, tmpHdr.DeviceName, 0, 0x0D);
            tmpHdr.DeviceType    = BigEndianBitConverter.ToUInt32(header, 0x0C) & 0x00FFFFFF;
            tmpHdr.DeviceBlocks  = BigEndianBitConverter.ToUInt32(header, 0x11) & 0x00FFFFFF;
            tmpHdr.BytesPerBlock = BigEndianBitConverter.ToUInt16(header, 0x15);

            for(int i = 0; i < 0xD; i++)
                if(tmpHdr.DeviceName[i] < 0x20)
                    return false;

            return (tmpHdr.BytesPerBlock & 0xFE00) == 0x200;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            imageHeader                          = new BluHeader {DeviceName = new byte[0x0D]};
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] header = new byte[0x17];
            stream.Read(header, 0, 0x17);
            Array.Copy(header, 0, imageHeader.DeviceName, 0, 0x0D);
            imageHeader.DeviceType    = BigEndianBitConverter.ToUInt32(header, 0x0C) & 0x00FFFFFF;
            imageHeader.DeviceBlocks  = BigEndianBitConverter.ToUInt32(header, 0x11) & 0x00FFFFFF;
            imageHeader.BytesPerBlock = BigEndianBitConverter.ToUInt16(header, 0x15);

            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceName = \"{0}\"",
                                      StringHandlers.CToString(imageHeader.DeviceName));
            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceType = {0}",    imageHeader.DeviceType);
            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.deviceBlock = {0}",   imageHeader.DeviceBlocks);
            DicConsole.DebugWriteLine("BLU plugin", "ImageHeader.bytesPerBlock = {0}", imageHeader.BytesPerBlock);

            for(int i = 0; i < 0xD; i++)
                if(imageHeader.DeviceName[i] < 0x20)
                    return false;

            if((imageHeader.BytesPerBlock & 0xFE00) != 0x200) return false;

            stream.Seek(0, SeekOrigin.Begin);
            header = new byte[imageHeader.BytesPerBlock];
            stream.Read(header, 0, imageHeader.BytesPerBlock);

            imageInfo.SectorSize = 0x200;

            imageInfo.Sectors   = imageHeader.DeviceBlocks;
            imageInfo.ImageSize = imageHeader.DeviceBlocks * imageHeader.BytesPerBlock;
            bptag               = imageHeader.BytesPerBlock - 0x200;
            byte[] hdrTag = new byte[bptag];
            Array.Copy(header, 0x200, hdrTag, 0, bptag);

            switch(StringHandlers.CToString(imageHeader.DeviceName))
            {
                case PROFILE_NAME:
                    imageInfo.MediaType =
                        imageInfo.Sectors == 0x2600 ? MediaType.AppleProfile : MediaType.GENERIC_HDD;
                    imageInfo.Cylinders       = 152;
                    imageInfo.Heads           = 4;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case PROFILE10_NAME:
                    imageInfo.MediaType =
                        imageInfo.Sectors == 0x4C00 ? MediaType.AppleProfile : MediaType.GENERIC_HDD;
                    imageInfo.Cylinders       = 304;
                    imageInfo.Heads           = 4;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case WIDGET_NAME:
                    imageInfo.MediaType =
                        imageInfo.Sectors == 0x4C00 ? MediaType.AppleWidget : MediaType.GENERIC_HDD;
                    imageInfo.Cylinders       = 304;
                    imageInfo.Heads           = 4;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case PRIAM_NAME:
                    imageInfo.MediaType =
                        imageInfo.Sectors == 0x022C7C ? MediaType.PriamDataTower : MediaType.GENERIC_HDD;
                    // This values are invented...
                    imageInfo.Cylinders       = 419;
                    imageInfo.Heads           = 4;
                    imageInfo.SectorsPerTrack = 85;
                    break;
                default:
                    imageInfo.MediaType       = MediaType.GENERIC_HDD;
                    imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
                    imageInfo.Heads           = 16;
                    imageInfo.SectorsPerTrack = 63;
                    break;
            }

            imageInfo.Application = StringHandlers.CToString(hdrTag);

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());

            bluImageFilter = imageFilter;

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            if(bptag > 0) imageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);

            DicConsole.VerboseWriteLine("BLU image contains a disk of type {0}", imageInfo.MediaType);

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream buffer = new MemoryStream();
            int          seek   = 0;
            int          read   = 0x200;
            int          skip   = bptag;

            Stream stream = bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * imageHeader.BytesPerBlock), SeekOrigin.Begin);

            for(int i = 0; i < length; i++)
            {
                stream.Seek(seek, SeekOrigin.Current);
                byte[] sector = new byte[read];
                stream.Read(sector, 0, read);
                buffer.Write(sector, 0, read);
                stream.Seek(skip, SeekOrigin.Current);
            }

            return buffer.ToArray();
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(tag != SectorTagType.AppleSectorTag)
                throw new FeatureUnsupportedImageException($"Tag {tag} not supported by image format");

            if(bptag == 0) throw new FeatureNotPresentImageException("Disk image does not have tags");

            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream buffer = new MemoryStream();
            int          seek   = 0x200;
            int          read   = bptag;
            int          skip   = 0;

            Stream stream = bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * imageHeader.BytesPerBlock), SeekOrigin.Begin);

            for(int i = 0; i < length; i++)
            {
                stream.Seek(seek, SeekOrigin.Current);
                byte[] sector = new byte[read];
                stream.Read(sector, 0, read);
                buffer.Write(sector, 0, read);
                stream.Seek(skip, SeekOrigin.Current);
            }

            return buffer.ToArray();
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageHeader.BytesPerBlock];
            Stream stream = bluImageFilter.GetDataForkStream();
            stream.Seek((long)((sectorAddress + 1) * imageHeader.BytesPerBlock), SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            return buffer;
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

        // TODO: Check tag checkums
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

        public IEnumerable<MediaTagType>  SupportedMediaTags  => new MediaTagType[] { };
        public IEnumerable<SectorTagType> SupportedSectorTags => new[] {SectorTagType.AppleSectorTag};
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.AppleProfile, MediaType.AppleWidget, MediaType.PriamDataTower, MediaType.GENERIC_HDD,
                MediaType.Unknown, MediaType.FlashDrive, MediaType.CompactFlash, MediaType.CompactFlashType2,
                MediaType.PCCardTypeI, MediaType.PCCardTypeII, MediaType.PCCardTypeIII, MediaType.PCCardTypeIV
            };
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".blu"}; // Just invented
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(sectorSize != 512)
            {
                ErrorMessage = "Unsupported sector size";
                return false;
            }

            if(sectors > 0xFFFFFF)
            {
                ErrorMessage = "Too many sectors";
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
            int longSectorSize = imageInfo.MediaType == MediaType.PriamDataTower ? 536 : 532;

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

            writingStream.Seek(longSectorSize + (long)sectorAddress * longSectorSize, SeekOrigin.Begin);
            writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";
            return true;
        }

        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            int longSectorSize = imageInfo.MediaType == MediaType.PriamDataTower ? 536 : 532;

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

            writingStream.Seek(longSectorSize + (long)sectorAddress * longSectorSize, SeekOrigin.Begin);
            writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";
            return true;
        }

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(sectorAddress >= imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            int longSectorSize = imageInfo.MediaType == MediaType.PriamDataTower ? 536 : 532;

            byte[] oldTag;
            byte[] newTag;

            switch(data.Length - 512)
            {
                // Sony tag, convert to Profile
                case 12 when longSectorSize == 532:
                    oldTag = new byte[12];
                    Array.Copy(data, 512, oldTag, 0, 12);
                    newTag = LisaTag.DecodeSonyTag(oldTag)?.ToProfile().GetBytes();
                    break;
                // Sony tag, convert to Priam
                case 12 when longSectorSize == 536:
                    oldTag = new byte[12];
                    Array.Copy(data, 512, oldTag, 0, 12);
                    newTag = LisaTag.DecodeSonyTag(oldTag)?.ToPriam().GetBytes();
                    break;
                // Profile tag, copy to Profile
                case 20 when longSectorSize == 532:
                    newTag = new byte[20];
                    Array.Copy(data, 512, newTag, 0, 20);
                    break;
                // Profile tag, convert to Priam
                case 20 when longSectorSize == 536:
                    oldTag = new byte[20];
                    Array.Copy(data, 512, oldTag, 0, 20);
                    newTag = LisaTag.DecodeProfileTag(oldTag)?.ToPriam().GetBytes();
                    break;
                // Priam tag, convert to Profile
                case 24 when longSectorSize == 532:
                    oldTag = new byte[24];
                    Array.Copy(data, 512, oldTag, 0, 24);
                    newTag = LisaTag.DecodePriamTag(oldTag)?.ToProfile().GetBytes();
                    break;
                // Priam tag, copy to Priam
                case 12 when longSectorSize == 536:
                    newTag = new byte[24];
                    Array.Copy(data, 512, newTag, 0, 24);
                    break;
                case 0:
                    newTag = null;
                    break;
                default:
                    ErrorMessage = "Incorrect data size";
                    return false;
            }

            if(newTag == null) newTag = new byte[longSectorSize - 512];

            writingStream.Seek(longSectorSize + (long)sectorAddress * longSectorSize, SeekOrigin.Begin);
            writingStream.Write(data,   0, 512);
            writingStream.Write(newTag, 0, newTag.Length);

            ErrorMessage = "";
            return true;
        }

        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(sectorAddress + length > imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            int  longSectorSize  = imageInfo.MediaType == MediaType.PriamDataTower ? 536 : 532;
            long givenSectorSize = data.Length / length;

            switch(givenSectorSize)
            {
                case 536:
                case 532:
                case 524:
                case 512: break;
                default:
                    ErrorMessage = "Incorrect data size";
                    return false;
            }

            for(uint i = 0; i < length; i++)
            {
                byte[] oldTag;
                byte[] newTag;

                switch(givenSectorSize - 512)
                {
                    // Sony tag, convert to Profile
                    case 12 when longSectorSize == 532:
                        oldTag = new byte[12];
                        Array.Copy(data, givenSectorSize * i + 512, oldTag, 0, 12);
                        newTag = LisaTag.DecodeSonyTag(oldTag)?.ToProfile().GetBytes();
                        break;
                    // Sony tag, convert to Priam
                    case 12 when longSectorSize == 536:
                        oldTag = new byte[12];
                        Array.Copy(data, givenSectorSize * i + 512, oldTag, 0, 12);
                        newTag = LisaTag.DecodeSonyTag(oldTag)?.ToPriam().GetBytes();
                        break;
                    // Profile tag, copy to Profile
                    case 20 when longSectorSize == 532:
                        newTag = new byte[20];
                        Array.Copy(data, givenSectorSize * i + 512, newTag, 0, 20);
                        break;
                    // Profile tag, convert to Priam
                    case 20 when longSectorSize == 536:
                        oldTag = new byte[20];
                        Array.Copy(data, givenSectorSize * i + 512, oldTag, 0, 20);
                        newTag = LisaTag.DecodeProfileTag(oldTag)?.ToPriam().GetBytes();
                        break;
                    // Priam tag, convert to Profile
                    case 24 when longSectorSize == 532:
                        oldTag = new byte[24];
                        Array.Copy(data, givenSectorSize * i + 512, oldTag, 0, 24);
                        newTag = LisaTag.DecodePriamTag(oldTag)?.ToProfile().GetBytes();
                        break;
                    // Priam tag, copy to Priam
                    case 12 when longSectorSize == 536:
                        newTag = new byte[24];
                        Array.Copy(data, givenSectorSize * i + 512, newTag, 0, 24);
                        break;
                    case 0:
                        newTag = null;
                        break;
                    default:
                        ErrorMessage = "Incorrect data size";
                        return false;
                }

                if(newTag == null) newTag = new byte[longSectorSize - 512];

                writingStream.Seek(longSectorSize + (long)sectorAddress * longSectorSize, SeekOrigin.Begin);
                writingStream.Write(data,   (int)(givenSectorSize * i), 512);
                writingStream.Write(newTag, 0,                          newTag.Length);
            }

            ErrorMessage = "";
            return true;
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

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            byte[] markerTag = Encoding.UTF8.GetBytes("DiscImageChef " + Version.GetVersion());
            byte[] driveName;
            byte[] driveType      = new byte[3];
            byte[] driveBlocks    = BigEndianBitConverter.GetBytes((uint)imageInfo.Sectors);
            int    longSectorSize = imageInfo.MediaType == MediaType.PriamDataTower ? 536 : 532;
            byte[] blockSize      = BigEndianBitConverter.GetBytes((ushort)longSectorSize);

            switch(imageInfo.MediaType)
            {
                case MediaType.AppleProfile when imageInfo.Sectors == 0x4C00:
                    driveName = Encoding.ASCII.GetBytes(PROFILE10_NAME);
                    break;
                case MediaType.AppleWidget when imageInfo.Sectors == 0x4C00:
                    driveType[1] = 0x01;
                    driveName    = Encoding.ASCII.GetBytes(PROFILE10_NAME);
                    break;
                case MediaType.PriamDataTower when imageInfo.Sectors == 0x22C7C:
                    driveType[1] = 0xFF;
                    driveName    = Encoding.ASCII.GetBytes(PRIAM_NAME);
                    break;
                default:
                    driveName = Encoding.ASCII.GetBytes(PROFILE_NAME);
                    break;
            }

            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(driveName, 0, driveName.Length >= 0xD ? 0xD : driveName.Length);
            writingStream.Seek(0xD, SeekOrigin.Begin);
            writingStream.Write(driveType, 0, 3);
            writingStream.Seek(0x12, SeekOrigin.Begin);
            writingStream.Write(driveBlocks, 1, 3);
            writingStream.Seek(0x15, SeekOrigin.Begin);
            writingStream.Write(blockSize, 1, 2);
            writingStream.Seek(512, SeekOrigin.Begin);
            writingStream.Write(markerTag, 0,
                                markerTag.Length >= longSectorSize - 512 ? longSectorSize - 512 : markerTag.Length);

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

        struct BluHeader
        {
            public byte[] DeviceName;
            public uint   DeviceType;
            public uint   DeviceBlocks;
            public ushort BytesPerBlock;
        }
    }
}