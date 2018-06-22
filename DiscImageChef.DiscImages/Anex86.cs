// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Anex86.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Anex86 disk images.
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
using Schemas;

namespace DiscImageChef.DiscImages
{
    public class Anex86 : IWritableImage
    {
        IFilter      anexImageFilter;
        Anex86Header fdihdr;
        ImageInfo    imageInfo;
        FileStream   writingStream;

        public Anex86()
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

        public string Name => "Anex86 Disk Image";
        public Guid   Id   => new Guid("0410003E-6E7B-40E6-9328-BA5651ADF6B7");

        public string Format => "Anex86 disk image";

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

            fdihdr = new Anex86Header();

            if(stream.Length < Marshal.SizeOf(fdihdr)) return false;

            byte[] hdrB = new byte[Marshal.SizeOf(fdihdr)];
            stream.Read(hdrB, 0, hdrB.Length);

            GCHandle handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
            fdihdr = (Anex86Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Anex86Header));
            handle.Free();

            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.unknown = {0}",   fdihdr.unknown);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.hddtype = {0}",   fdihdr.hddtype);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.hdrSize = {0}",   fdihdr.hdrSize);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.dskSize = {0}",   fdihdr.dskSize);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.bps = {0}",       fdihdr.bps);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.spt = {0}",       fdihdr.spt);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.heads = {0}",     fdihdr.heads);
            DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.cylinders = {0}", fdihdr.cylinders);

            return stream.Length  == fdihdr.hdrSize + fdihdr.dskSize &&
                   fdihdr.dskSize == fdihdr.bps * fdihdr.spt * fdihdr.heads * fdihdr.cylinders;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            fdihdr = new Anex86Header();

            if(stream.Length < Marshal.SizeOf(fdihdr)) return false;

            byte[] hdrB = new byte[Marshal.SizeOf(fdihdr)];
            stream.Read(hdrB, 0, hdrB.Length);

            GCHandle handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
            fdihdr = (Anex86Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Anex86Header));
            handle.Free();

            imageInfo.MediaType = Geometry.GetMediaType(((ushort)fdihdr.cylinders, (byte)fdihdr.heads,
                                                            (ushort)fdihdr.spt, (uint)fdihdr.bps, MediaEncoding.MFM,
                                                            false));
            if(imageInfo.MediaType == MediaType.Unknown) imageInfo.MediaType = MediaType.GENERIC_HDD;

            DicConsole.DebugWriteLine("Anex86 plugin", "MediaType: {0}", imageInfo.MediaType);

            imageInfo.ImageSize            = (ulong)fdihdr.dskSize;
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = (ulong)(fdihdr.cylinders * fdihdr.heads * fdihdr.spt);
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.SectorSize           = (uint)fdihdr.bps;
            imageInfo.Cylinders            = (uint)fdihdr.cylinders;
            imageInfo.Heads                = (uint)fdihdr.heads;
            imageInfo.SectorsPerTrack      = (uint)fdihdr.spt;

            anexImageFilter = imageFilter;

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

            Stream stream = anexImageFilter.GetDataForkStream();

            stream.Seek((long)((ulong)fdihdr.hdrSize + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);

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

        public IEnumerable<MediaTagType>  SupportedMediaTags  => new MediaTagType[] { };
        public IEnumerable<SectorTagType> SupportedSectorTags => new SectorTagType[] { };
        // TODO: Test with real hardware to see real supported media
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.IBM23FD, MediaType.ECMA_66, MediaType.DOS_525_SS_DD_8, MediaType.DOS_525_SS_DD_9,
                MediaType.ACORN_525_SS_SD_40, MediaType.ACORN_525_SS_DD_40, MediaType.ATARI_525_SD,
                MediaType.ATARI_525_DD, MediaType.ATARI_525_ED, MediaType.DOS_525_DS_DD_8, MediaType.DOS_525_DS_DD_9,
                MediaType.ECMA_70, MediaType.Apricot_35, MediaType.RX01, MediaType.RX02, MediaType.NEC_525_HD,
                MediaType.ECMA_99_15, MediaType.NEC_8_SD, MediaType.RX03, MediaType.DOS_35_SS_DD_8,
                MediaType.DOS_35_SS_DD_9, MediaType.ACORN_525_SS_SD_80, MediaType.RX50, MediaType.ATARI_35_SS_DD_11,
                MediaType.ACORN_525_SS_DD_80, MediaType.ACORN_35_DS_DD, MediaType.DOS_35_DS_DD_8,
                MediaType.DOS_35_DS_DD_9, MediaType.ACORN_35_DS_HD, MediaType.DOS_525_HD, MediaType.ACORN_525_DS_DD,
                MediaType.DOS_35_HD, MediaType.XDF_525, MediaType.DMF, MediaType.XDF_35, MediaType.DOS_35_ED,
                MediaType.FDFORMAT_35_DD, MediaType.FDFORMAT_525_HD, MediaType.FDFORMAT_35_HD, MediaType.NEC_35_TD,
                MediaType.Unknown, MediaType.GENERIC_HDD, MediaType.FlashDrive, MediaType.CompactFlash,
                MediaType.CompactFlashType2, MediaType.PCCardTypeI, MediaType.PCCardTypeII, MediaType.PCCardTypeIII,
                MediaType.PCCardTypeIV
            };
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".fdi", ".hdi"};

        public bool   IsWriting    { get; private set; }
        public string ErrorMessage { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(sectorSize == 0)
            {
                ErrorMessage = "Unsupported sector size";
                return false;
            }

            if(sectors * sectorSize > int.MaxValue || sectors > (long)int.MaxValue * 8 * 33)
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

            fdihdr = new Anex86Header {hdrSize = 4096, dskSize = (int)(sectors * sectorSize), bps = (int)sectorSize};

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

            writingStream.Seek((long)(4096 + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
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

            writingStream.Seek((long)(4096 + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
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

            if((imageInfo.MediaType == MediaType.Unknown           || imageInfo.MediaType == MediaType.GENERIC_HDD   ||
                imageInfo.MediaType == MediaType.FlashDrive        || imageInfo.MediaType == MediaType.CompactFlash  ||
                imageInfo.MediaType == MediaType.CompactFlashType2 || imageInfo.MediaType == MediaType.PCCardTypeI   ||
                imageInfo.MediaType == MediaType.PCCardTypeII      || imageInfo.MediaType == MediaType.PCCardTypeIII ||
                imageInfo.MediaType == MediaType.PCCardTypeIV) && fdihdr.cylinders == 0)
            {
                fdihdr.cylinders = (int)(imageInfo.Sectors / 8 / 33);
                fdihdr.heads     = 8;
                fdihdr.spt       = 33;

                while(fdihdr.cylinders == 0)
                {
                    fdihdr.heads--;

                    if(fdihdr.heads == 0)
                    {
                        fdihdr.spt--;
                        fdihdr.heads = 8;
                    }

                    fdihdr.cylinders = (int)imageInfo.Sectors / fdihdr.heads / fdihdr.spt;

                    if(fdihdr.cylinders == 0 && fdihdr.heads == 0 && fdihdr.spt == 0) break;
                }
            }

            byte[] hdr    = new byte[Marshal.SizeOf(fdihdr)];
            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(fdihdr));
            Marshal.StructureToPtr(fdihdr, hdrPtr, true);
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
            if(cylinders > int.MaxValue)
            {
                ErrorMessage = "Too many cylinders.";
                return false;
            }

            if(heads > int.MaxValue)
            {
                ErrorMessage = "Too many heads.";
                return false;
            }

            if(sectorsPerTrack > int.MaxValue)
            {
                ErrorMessage = "Too many sectors per track.";
                return false;
            }

            fdihdr.spt       = (int)sectorsPerTrack;
            fdihdr.heads     = (int)heads;
            fdihdr.cylinders = (int)cylinders;

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
        struct Anex86Header
        {
            public int unknown;
            public int hddtype;
            public int hdrSize;
            public int dskSize;
            public int bps;
            public int spt;
            public int heads;
            public int cylinders;
        }
    }
}