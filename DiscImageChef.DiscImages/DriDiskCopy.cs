// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DriDiskCopy.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Digital Research's DISKCOPY disk images.
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
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public class DriDiskCopy : IWritableImage
    {
        const string REGEX_DRI = @"DiskImage\s(?<version>\d+.\d+)\s\(C\)\s\d+\,*\d*\s+Digital Research Inc";

        /// <summary>Disk image file</summary>
        IFilter driImageFilter;

        /// <summary>Footer of opened image</summary>
        DriFooter footer;
        ImageInfo  imageInfo;
        FileStream writingStream;

        public DriDiskCopy()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Application           = "DiskCopy",
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

        public string Name => "Digital Research DiskCopy";
        public Guid   Id   => new Guid("9F0BE551-8BAB-4038-8B5A-691F1BF5FFF3");

        public string Format => "Digital Research DiskCopy";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if((stream.Length - Marshal.SizeOf(typeof(DriFooter))) % 512 != 0) return false;

            byte[] buffer = new byte[Marshal.SizeOf(typeof(DriFooter))];
            stream.Seek(-buffer.Length, SeekOrigin.End);
            stream.Read(buffer, 0, buffer.Length);

            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            DriFooter tmpFooter = (DriFooter)Marshal.PtrToStructure(ftrPtr, typeof(DriFooter));
            Marshal.FreeHGlobal(ftrPtr);

            string sig = StringHandlers.CToString(tmpFooter.signature);

            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.signature = \"{0}\"", sig);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.five = {0}",      tmpFooter.bpb.five);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.driveCode = {0}", tmpFooter.bpb.driveCode);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.unknown = {0}",   tmpFooter.bpb.unknown);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.cylinders = {0}", tmpFooter.bpb.cylinders);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.unknown2 = {0}",  tmpFooter.bpb.unknown2);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.bps = {0}",       tmpFooter.bpb.bps);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.spc = {0}",       tmpFooter.bpb.spc);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.rsectors = {0}",  tmpFooter.bpb.rsectors);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.fats_no = {0}",   tmpFooter.bpb.fats_no);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.sectors = {0}",   tmpFooter.bpb.sectors);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.media_descriptor = {0}",
                                      tmpFooter.bpb.media_descriptor);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.spfat = {0}",    tmpFooter.bpb.spfat);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.sptrack = {0}",  tmpFooter.bpb.sptrack);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.heads = {0}",    tmpFooter.bpb.heads);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.hsectors = {0}", tmpFooter.bpb.hsectors);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.drive_no = {0}", tmpFooter.bpb.drive_no);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.unknown3 = {0}", tmpFooter.bpb.unknown3);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.unknown4 = {0}", tmpFooter.bpb.unknown4);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.sptrack2 = {0}", tmpFooter.bpb.sptrack2);
            DicConsole.DebugWriteLine("DRI DiskCopy plugin",
                                      "ArrayHelpers.ArrayIsNullOrEmpty(tmp_footer.bpb.unknown5) = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(tmpFooter.bpb.unknown5));

            Regex regexSignature = new Regex(REGEX_DRI);
            Match matchSignature = regexSignature.Match(sig);

            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "MatchSignature.Success? = {0}", matchSignature.Success);

            if(!matchSignature.Success) return false;

            if(tmpFooter.bpb.sptrack * tmpFooter.bpb.cylinders * tmpFooter.bpb.heads != tmpFooter.bpb.sectors)
                return false;

            return tmpFooter.bpb.sectors * tmpFooter.bpb.bps + Marshal.SizeOf(tmpFooter) == stream.Length;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if((stream.Length - Marshal.SizeOf(typeof(DriFooter))) % 512 != 0) return false;

            byte[] buffer = new byte[Marshal.SizeOf(typeof(DriFooter))];
            stream.Seek(-buffer.Length, SeekOrigin.End);
            stream.Read(buffer, 0, buffer.Length);

            footer = new DriFooter();
            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            footer = (DriFooter)Marshal.PtrToStructure(ftrPtr, typeof(DriFooter));
            Marshal.FreeHGlobal(ftrPtr);

            string sig = StringHandlers.CToString(footer.signature);

            Regex regexSignature = new Regex(REGEX_DRI);
            Match matchSignature = regexSignature.Match(sig);

            if(!matchSignature.Success) return false;

            if(footer.bpb.sptrack * footer.bpb.cylinders * footer.bpb.heads != footer.bpb.sectors) return false;

            if(footer.bpb.sectors * footer.bpb.bps + Marshal.SizeOf(footer) != stream.Length) return false;

            imageInfo.Cylinders          = footer.bpb.cylinders;
            imageInfo.Heads              = footer.bpb.heads;
            imageInfo.SectorsPerTrack    = footer.bpb.sptrack;
            imageInfo.Sectors            = footer.bpb.sectors;
            imageInfo.SectorSize         = footer.bpb.bps;
            imageInfo.ApplicationVersion = matchSignature.Groups["version"].Value;

            driImageFilter = imageFilter;

            imageInfo.ImageSize            = (ulong)(stream.Length - Marshal.SizeOf(footer));
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();

            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "Image application = {0} version {1}",
                                      imageInfo.Application, imageInfo.ApplicationVersion);

            // Correct some incorrect data in images of NEC 2HD disks
            if(imageInfo.Cylinders  == 77  && imageInfo.Heads == 2 && imageInfo.SectorsPerTrack == 16 &&
               imageInfo.SectorSize == 512 && (footer.bpb.driveCode == DriDriveCodes.md2hd ||
                                               footer.bpb.driveCode == DriDriveCodes.mf2hd))
            {
                imageInfo.SectorsPerTrack = 8;
                imageInfo.SectorSize      = 1024;
            }

            imageInfo.MediaType = Geometry.GetMediaType(((ushort)imageInfo.Cylinders, (byte)imageInfo.Heads,
                                                            (ushort)imageInfo.SectorsPerTrack, imageInfo.SectorSize,
                                                            MediaEncoding.MFM, false));

            switch(imageInfo.MediaType)
            {
                case MediaType.NEC_525_HD when footer.bpb.driveCode == DriDriveCodes.mf2hd ||
                                               footer.bpb.driveCode == DriDriveCodes.mf2ed:
                    imageInfo.MediaType = MediaType.NEC_35_HD_8;
                    break;
                case MediaType.DOS_525_HD when footer.bpb.driveCode == DriDriveCodes.mf2hd ||
                                               footer.bpb.driveCode == DriDriveCodes.mf2ed:
                    imageInfo.MediaType = MediaType.NEC_35_HD_15;
                    break;
                case MediaType.RX50 when footer.bpb.driveCode == DriDriveCodes.md2dd ||
                                         footer.bpb.driveCode == DriDriveCodes.md2hd:
                    imageInfo.MediaType = MediaType.ATARI_35_SS_DD;
                    break;
            }

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            DicConsole.VerboseWriteLine("Digital Research DiskCopy image contains a disk of type {0}",
                                        imageInfo.MediaType);

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

            Stream stream = driImageFilter.GetDataForkStream();
            stream.Seek((long)(sectorAddress    * imageInfo.SectorSize), SeekOrigin.Begin);
            stream.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

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
                MediaType.ACORN_35_DS_DD, MediaType.ACORN_35_DS_HD, MediaType.Apricot_35, MediaType.ATARI_35_DS_DD,
                MediaType.ATARI_35_DS_DD_11, MediaType.ATARI_35_SS_DD, MediaType.ATARI_35_SS_DD_11, MediaType.DMF,
                MediaType.DMF_82, MediaType.DOS_35_DS_DD_8, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED,
                MediaType.DOS_35_HD, MediaType.DOS_35_SS_DD_8, MediaType.DOS_35_SS_DD_9, MediaType.DOS_525_DS_DD_8,
                MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_525_SS_DD_8, MediaType.DOS_525_SS_DD_9,
                MediaType.FDFORMAT_35_DD, MediaType.FDFORMAT_35_HD, MediaType.FDFORMAT_525_DD,
                MediaType.FDFORMAT_525_HD, MediaType.RX50, MediaType.XDF_35, MediaType.XDF_525
            };
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".dsk"};

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

            if(sectors > ushort.MaxValue)
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

            // TODO: Check this
            (ushort cylinders, byte heads, ushort sectorsPerTrack, uint bytesPerSector, MediaEncoding encoding, bool
                variableSectorsPerTrack, MediaType type) geometry = Geometry.GetGeometry(mediaType);

            footer = new DriFooter
            {
                signature = new byte[51],
                bpb = new DriBpb
                {
                    five      = 5,
                    driveCode = DriDriveCodes.mf2ed,
                    cylinders = geometry.cylinders,
                    bps       = (ushort)imageInfo.SectorSize,
                    sectors   = (ushort)imageInfo.Sectors,
                    sptrack   = (ushort)imageInfo.SectorsPerTrack,
                    heads     = (ushort)imageInfo.Heads,
                    sptrack2  = (ushort)imageInfo.SectorsPerTrack,
                    unknown5  = new byte[144]
                }
            };
            Array.Copy(Encoding.ASCII.GetBytes("DiskImage 2.01 (C) 1990,1991 Digital Research Inc"), 0,
                       footer.signature, 0, 49);
            footer.bpbcopy = footer.bpb;

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

            writingStream.Seek((long)(sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
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

            writingStream.Seek((long)(sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
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

            byte[] hdr    = new byte[Marshal.SizeOf(footer)];
            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(footer));
            Marshal.StructureToPtr(footer, hdrPtr, true);
            Marshal.Copy(hdrPtr, hdr, 0, hdr.Length);
            Marshal.FreeHGlobal(hdrPtr);

            writingStream.Seek(0, SeekOrigin.End);
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
        struct DriFooter
        {
            /// <summary>Signature: "DiskImage 2.01 (C) 1990,1991 Digital Research Inc\0"</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 51)]
            public byte[] signature;
            /// <summary>Information about the disk image, mostly imitates FAT BPB</summary>
            public DriBpb bpb;
            /// <summary>Information about the disk image, mostly imitates FAT BPB, copy</summary>
            public DriBpb bpbcopy;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DriBpb
        {
            /// <summary>Seems to be always 0x05</summary>
            public byte five;
            /// <summary>A drive code that corresponds (but it not equal to) CMOS drive types</summary>
            public DriDriveCodes driveCode;
            /// <summary>Unknown seems to be always 2</summary>
            public ushort unknown;
            /// <summary>Cylinders</summary>
            public ushort cylinders;
            /// <summary>Seems to always be 0</summary>
            public byte unknown2;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>How many FATs</summary>
            public byte fats_no;
            /// <summary>Entries in root directory</summary>
            public ushort root_entries;
            /// <summary>Total sectors</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media_descriptor;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Sectors per track</summary>
            public ushort sptrack;
            /// <summary>Heads</summary>
            public ushort heads;
            /// <summary>Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>Drive number</summary>
            public byte drive_no;
            /// <summary>Seems to be 0</summary>
            public ulong unknown3;
            /// <summary>Seems to be 0</summary>
            public byte unknown4;
            /// <summary>Sectors per track (again?)</summary>
            public ushort sptrack2;
            /// <summary>Seems to be 0</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 144)]
            public byte[] unknown5;
        }

        /// <summary>
        ///     Drive codes change according to CMOS stored valued
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        enum DriDriveCodes : byte
        {
            /// <summary>5.25" 360k</summary>
            md2dd = 0,
            /// <summary>5.25" 1.2M</summary>
            md2hd = 1,
            /// <summary>3.5" 720k</summary>
            mf2dd = 2,
            /// <summary>3.5" 1.44M</summary>
            mf2hd = 7,
            /// <summary>3.5" 2.88M</summary>
            mf2ed = 9
        }
    }
}