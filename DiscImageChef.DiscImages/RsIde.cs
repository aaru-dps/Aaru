// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : RsIde.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages RS-IDE disk images.
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
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using Schemas;
using static DiscImageChef.Decoders.ATA.Identify;
using Version = DiscImageChef.CommonTypes.Interop.Version;

namespace DiscImageChef.DiscImages
{
    public class RsIde : IWritableImage
    {
        readonly byte[] signature = {0x52, 0x53, 0x2D, 0x49, 0x44, 0x45, 0x1A};
        ushort          dataOff;
        byte[]          identify;
        ImageInfo       imageInfo;

        IFilter    rsIdeImageFilter;
        FileStream writingStream;

        public RsIde()
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

        public string    Name => "RS-IDE Hard Disk Image";
        public Guid      Id   => new Guid("47C3E78D-2BE2-4BA5-AA6B-FEE27C86FC65");
        public ImageInfo Info => imageInfo;

        public string Format => "RS-IDE disk image";

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

            byte[] magic = new byte[7];
            stream.Read(magic, 0, magic.Length);

            return magic.SequenceEqual(signature);
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] hdrB = new byte[Marshal.SizeOf(typeof(RsIdeHeader))];
            stream.Read(hdrB, 0, hdrB.Length);

            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RsIdeHeader)));
            Marshal.Copy(hdrB, 0, hdrPtr, Marshal.SizeOf(typeof(RsIdeHeader)));
            RsIdeHeader hdr = (RsIdeHeader)Marshal.PtrToStructure(hdrPtr, typeof(RsIdeHeader));
            Marshal.FreeHGlobal(hdrPtr);

            if(!hdr.magic.SequenceEqual(signature)) return false;

            dataOff = hdr.dataOff;

            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.SectorSize           = (uint)(hdr.flags.HasFlag(RsIdeFlags.HalfSectors) ? 256 : 512);
            imageInfo.ImageSize            = (ulong)(stream.Length - dataOff);
            imageInfo.Sectors              = imageInfo.ImageSize / imageInfo.SectorSize;
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.Version              = $"{hdr.revision >> 8}.{hdr.revision & 0x0F}";

            if(!ArrayHelpers.ArrayIsNullOrEmpty(hdr.identify))
            {
                identify = new byte[512];
                Array.Copy(hdr.identify, 0, identify, 0, hdr.identify.Length);
                IdentifyDevice? ataId = Decode(identify);

                if(ataId.HasValue)
                {
                    imageInfo.ReadableMediaTags.Add(MediaTagType.ATA_IDENTIFY);
                    imageInfo.Cylinders             = ataId.Value.Cylinders;
                    imageInfo.Heads                 = ataId.Value.Heads;
                    imageInfo.SectorsPerTrack       = ataId.Value.SectorsPerCard;
                    imageInfo.DriveFirmwareRevision = ataId.Value.FirmwareRevision;
                    imageInfo.DriveModel            = ataId.Value.Model;
                    imageInfo.DriveSerialNumber     = ataId.Value.SerialNumber;
                    imageInfo.MediaSerialNumber     = ataId.Value.MediaSerial;
                    imageInfo.MediaManufacturer     = ataId.Value.MediaManufacturer;
                }
            }

            if(imageInfo.Cylinders == 0 || imageInfo.Heads == 0 || imageInfo.SectorsPerTrack == 0)
            {
                imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
                imageInfo.Heads           = 16;
                imageInfo.SectorsPerTrack = 63;
            }

            rsIdeImageFilter = imageFilter;

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

            Stream stream = rsIdeImageFilter.GetDataForkStream();

            stream.Seek((long)(dataOff + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);

            stream.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

            return buffer;
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            if(!imageInfo.ReadableMediaTags.Contains(tag) || tag != MediaTagType.ATA_IDENTIFY)
                throw new FeatureUnsupportedImageException("Feature not supported by image format");

            byte[] buffer = new byte[512];
            Array.Copy(identify, 0, buffer, 0, 512);
            return buffer;
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

        public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < imageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifyMediaImage()
        {
            return null;
        }

        public List<DumpHardwareType> DumpHardware => null;
        public CICMMetadataType       CicmMetadata => null;

        public IEnumerable<MediaTagType>  SupportedMediaTags  => new[] {MediaTagType.ATA_IDENTIFY};
        public IEnumerable<SectorTagType> SupportedSectorTags => new SectorTagType[] { };
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.GENERIC_HDD, MediaType.Unknown, MediaType.FlashDrive, MediaType.CompactFlash,
                MediaType.CompactFlashType2, MediaType.PCCardTypeI, MediaType.PCCardTypeII, MediaType.PCCardTypeIII,
                MediaType.PCCardTypeIV
            };
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".ide"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(sectorSize != 256 && sectorSize != 512)
            {
                ErrorMessage = "Unsupported sector size";
                return false;
            }

            if(sectors > 63 * 16 * 1024)
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
            if(tag != MediaTagType.ATA_IDENTIFY)
            {
                ErrorMessage = $"Unsupported media tag {tag}.";
                return false;
            }

            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            identify = new byte[106];
            Array.Copy(data, 0, identify, 0, 106);
            return true;
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

            writingStream
               .Seek((long)((ulong)Marshal.SizeOf(typeof(RsIdeHeader)) + sectorAddress * imageInfo.SectorSize),
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

            writingStream
               .Seek((long)((ulong)Marshal.SizeOf(typeof(RsIdeHeader)) + sectorAddress * imageInfo.SectorSize),
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

            if(imageInfo.Cylinders == 0)
            {
                imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
                imageInfo.Heads           = 16;
                imageInfo.SectorsPerTrack = 63;

                while(imageInfo.Cylinders == 0)
                {
                    imageInfo.Heads--;

                    if(imageInfo.Heads == 0)
                    {
                        imageInfo.SectorsPerTrack--;
                        imageInfo.Heads = 16;
                    }

                    imageInfo.Cylinders = (uint)(imageInfo.Sectors / imageInfo.Heads / imageInfo.SectorsPerTrack);

                    if(imageInfo.Cylinders == 0 && imageInfo.Heads == 0 && imageInfo.SectorsPerTrack == 0) break;
                }
            }

            RsIdeHeader header = new RsIdeHeader
            {
                magic    = signature,
                identify = new byte[106],
                dataOff  = (ushort)Marshal.SizeOf(typeof(RsIdeHeader)),
                revision = 1,
                reserved = new byte[11]
            };
            if(imageInfo.SectorSize == 256) header.flags = RsIdeFlags.HalfSectors;

            if(identify == null)
            {
                IdentifyDevice ataId = new IdentifyDevice
                {
                    GeneralConfiguration =
                        GeneralConfigurationBit.UltraFastIDE | GeneralConfigurationBit.Fixed |
                        GeneralConfigurationBit.NotMFM       | GeneralConfigurationBit.SoftSector,
                    Cylinders       = (ushort)imageInfo.Cylinders,
                    Heads           = (ushort)imageInfo.Heads,
                    SectorsPerTrack = (ushort)imageInfo.SectorsPerTrack,
                    VendorWord47    = 0x80,
                    Capabilities =
                        CapabilitiesBit.DMASupport | CapabilitiesBit.IORDY | CapabilitiesBit.LBASupport,
                    ExtendedIdentify       = ExtendedIdentifyBit.Words54to58Valid,
                    CurrentCylinders       = (ushort)imageInfo.Cylinders,
                    CurrentHeads           = (ushort)imageInfo.Heads,
                    CurrentSectorsPerTrack = (ushort)imageInfo.SectorsPerTrack,
                    CurrentSectors         = (uint)imageInfo.Sectors,
                    LBASectors             = (uint)imageInfo.Sectors,
                    DMASupported           = TransferMode.Mode0,
                    DMAActive              = TransferMode.Mode0
                };

                if(string.IsNullOrEmpty(imageInfo.DriveManufacturer)) imageInfo.DriveManufacturer = "DiscImageChef";

                if(string.IsNullOrEmpty(imageInfo.DriveModel)) imageInfo.DriveModel = "";

                if(string.IsNullOrEmpty(imageInfo.DriveFirmwareRevision)) Version.GetVersion();

                if(string.IsNullOrEmpty(imageInfo.DriveSerialNumber))
                    imageInfo.DriveSerialNumber = $"{new Random().NextDouble():16X}";

                byte[] ataIdBytes = new byte[Marshal.SizeOf(ataId)];
                IntPtr ptr        = Marshal.AllocHGlobal(512);
                Marshal.StructureToPtr(ataId, ptr, true);
                Marshal.Copy(ptr, ataIdBytes, 0, Marshal.SizeOf(ataId));
                Marshal.FreeHGlobal(ptr);

                Array.Copy(ScrambleAtaString(imageInfo.DriveManufacturer + " " + imageInfo.DriveModel, 40), 0,
                           ataIdBytes, 27 * 2, 40);
                Array.Copy(ScrambleAtaString(imageInfo.DriveFirmwareRevision, 8),  0, ataIdBytes,      23 * 2, 8);
                Array.Copy(ScrambleAtaString(imageInfo.DriveSerialNumber,     20), 0, ataIdBytes,      10 * 2, 20);
                Array.Copy(ataIdBytes,                                             0, header.identify, 0,      106);
            }
            else Array.Copy(identify, 0, header.identify, 0, 106);

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
            imageInfo.DriveManufacturer     = metadata.DriveManufacturer;
            imageInfo.DriveModel            = metadata.DriveModel;
            imageInfo.DriveFirmwareRevision = metadata.DriveFirmwareRevision;
            imageInfo.DriveSerialNumber     = metadata.DriveSerialNumber;

            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            if(cylinders > ushort.MaxValue)
            {
                ErrorMessage = "Too many cylinders.";
                return false;
            }

            if(heads > ushort.MaxValue)
            {
                ErrorMessage = "Too many heads.";
                return false;
            }

            if(sectorsPerTrack > ushort.MaxValue)
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

        static byte[] ScrambleAtaString(string text, int length)
        {
            byte[] inbuf = Encoding.ASCII.GetBytes(text);
            if(inbuf.Length % 2 != 0)
            {
                byte[] tmpbuf = new byte[inbuf.Length + 1];
                Array.Copy(inbuf, 0, tmpbuf, 0, inbuf.Length);
                tmpbuf[tmpbuf.Length - 1] = 0x20;
                inbuf                     = tmpbuf;
            }

            byte[] outbuf = new byte[inbuf.Length];

            for(int i = 0; i < length; i += 2)
            {
                outbuf[i] = inbuf[i + 1];
                outbuf[i            + 1] = inbuf[i];
            }

            byte[] retBuf                             = new byte[length];
            for(int i = 0; i < length; i++) retBuf[i] = 0x20;

            Array.Copy(outbuf, 0, retBuf, 0, outbuf.Length >= length ? length : outbuf.Length);
            return retBuf;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RsIdeHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public byte[] magic;
            public byte       revision;
            public RsIdeFlags flags;
            public ushort     dataOff;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 106)]
            public byte[] identify;
        }

        [Flags]
        enum RsIdeFlags : byte
        {
            HalfSectors = 1
        }
    }
}