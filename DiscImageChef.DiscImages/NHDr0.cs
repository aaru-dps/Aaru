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
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using Schemas;

namespace DiscImageChef.DiscImages
{
    // Info from http://www.geocities.jp/t98next/nhdr0.txt
    public class Nhdr0 : IWritableImage
    {
        readonly byte[] signature =
            {0x54, 0x39, 0x38, 0x48, 0x44, 0x44, 0x49, 0x4D, 0x41, 0x47, 0x45, 0x2E, 0x52, 0x30, 0x00};
        ImageInfo imageInfo;

        Nhdr0Header nhdhdr;
        IFilter     nhdImageFilter;
        FileStream  writingStream;

        public Nhdr0()
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

        public string    Name => "T98-Next NHD r0 Disk Image";
        public Guid      Id   => new Guid("6ECACD0A-8F4D-4465-8815-AEA000D370E3");
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
            DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.wHead = {0}",      nhdhdr.wHead);
            DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.wSect = {0}",      nhdhdr.wSect);
            DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.wSectLen = {0}",   nhdhdr.wSectLen);

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

            imageInfo.ImageSize            = (ulong)(stream.Length - nhdhdr.dwHeadSize);
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = (ulong)(nhdhdr.dwCylinder * nhdhdr.wHead * nhdhdr.wSect);
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.SectorSize           = (uint)nhdhdr.wSectLen;
            imageInfo.Cylinders            = (uint)nhdhdr.dwCylinder;
            imageInfo.Heads                = (uint)nhdhdr.wHead;
            imageInfo.SectorsPerTrack      = (uint)nhdhdr.wSect;
            imageInfo.Comments             = StringHandlers.CToString(nhdhdr.szComment, shiftjis);

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
        public IEnumerable<MediaType>     SupportedMediaTypes => new[] {MediaType.GENERIC_HDD, MediaType.Unknown};
        // TODO: Support dynamic images
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".nhd"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(sectorSize != 0)
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
               .Seek((long)((ulong)Marshal.SizeOf(typeof(Nhdr0Header)) + sectorAddress * imageInfo.SectorSize),
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

            writingStream
               .Seek((long)((ulong)Marshal.SizeOf(typeof(Nhdr0Header)) + sectorAddress * imageInfo.SectorSize),
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
                imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 8 / 17);
                imageInfo.Heads           = 8;
                imageInfo.SectorsPerTrack = 17;

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

            Nhdr0Header header = new Nhdr0Header
            {
                szFileID   = signature,
                szComment  = new byte[0x100],
                dwHeadSize = Marshal.SizeOf(typeof(Nhdr0Header)),
                dwCylinder = (byte)imageInfo.Cylinders,
                wHead      = (byte)imageInfo.Heads,
                wSect      = (byte)imageInfo.SectorsPerTrack,
                wSectLen   = (byte)imageInfo.SectorSize,
                reserved2  = new byte[2],
                reserved3  = new byte[0xE0]
            };

            if(!string.IsNullOrEmpty(imageInfo.Comments))
            {
                byte[] commentBytes = Encoding.GetEncoding("shift_jis").GetBytes(imageInfo.Comments);
                Array.Copy(commentBytes, 0, header.szComment, 0,
                           commentBytes.Length >= 0x100 ? 0x100 : commentBytes.Length);
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
            IsWriting = false;

            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            imageInfo.Comments = metadata.Comments;
            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            if(cylinders > int.MaxValue)
            {
                ErrorMessage = "Too many cylinders.";
                return false;
            }

            if(heads > short.MaxValue)
            {
                ErrorMessage = "Too many heads.";
                return false;
            }

            if(sectorsPerTrack > short.MaxValue)
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
        struct Nhdr0Header
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public byte[] szFileID;
            public byte reserved1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
            public byte[] szComment;
            public int   dwHeadSize;
            public int   dwCylinder;
            public short wHead;
            public short wSect;
            public short wSectLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] reserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xE0)]
            public byte[] reserved3;
        }
    }
}