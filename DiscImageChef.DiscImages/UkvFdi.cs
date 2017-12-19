// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UkvFdi.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Spectrum FDI disk images.
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

namespace DiscImageChef.ImagePlugins
{
    public class UkvFdi : ImagePlugin
    {
        #region Internal enumerations
        [Flags]
        enum DiskFlags : byte
        {
            WriteProtected = 1
        }

        [Flags]
        enum SectorFlags : byte
        {
            CrcOk128 = 0x01,
            CrcOk256 = 0x02,
            CrcOk512 = 0x04,
            CrcOk1024 = 0x08,
            CrcOk2048 = 0x10,
            CrcOk4096 = 0x20,
            Deleted = 0x80,
        }
        #endregion

        #region Internal constants
        readonly byte[] signature = {0x46, 0x44, 0x49};
        #endregion

        #region Internal structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FdiHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] magic;
            public DiskFlags flags;
            public ushort cylinders;
            public ushort heads;
            public ushort descOff;
            public ushort dataOff;
            public ushort addInfoLen;
        }
        #endregion

        // Cylinder by head, sector data matrix
        byte[][][][] sectorsData;

        public UkvFdi()
        {
            Name = "Spectrum Floppy Disk Image";
            PluginUUID = new Guid("DADFC9B2-67C1-42A3-B124-825528163FC0");
            ImageInfo = new ImageInfo()
            {
                readableSectorTags = new List<SectorTagType>(),
                readableMediaTags = new List<MediaTagType>(),
                imageHasPartitions = false,
                imageHasSessions = false,
                imageVersion = null,
                imageApplication = null,
                imageApplicationVersion = null,
                imageCreator = null,
                imageComments = null,
                mediaManufacturer = null,
                mediaModel = null,
                mediaSerialNumber = null,
                mediaBarcode = null,
                mediaPartNumber = null,
                mediaSequence = 0,
                lastMediaSequence = 0,
                driveManufacturer = null,
                driveModel = null,
                driveSerialNumber = null,
                driveFirmwareRevision = null
            };
        }

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            FdiHeader hdr = new FdiHeader();

            if(stream.Length < Marshal.SizeOf(hdr)) return false;

            byte[] hdr_b = new byte[Marshal.SizeOf(hdr)];
            stream.Read(hdr_b, 0, hdr_b.Length);

            GCHandle handle = GCHandle.Alloc(hdr_b, GCHandleType.Pinned);
            hdr = (FdiHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FdiHeader));
            handle.Free();

            return hdr.magic.SequenceEqual(signature);
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            FdiHeader hdr = new FdiHeader();

            if(stream.Length < Marshal.SizeOf(hdr)) return false;

            byte[] hdr_b = new byte[Marshal.SizeOf(hdr)];
            stream.Read(hdr_b, 0, hdr_b.Length);

            GCHandle handle = GCHandle.Alloc(hdr_b, GCHandleType.Pinned);
            hdr = (FdiHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FdiHeader));
            handle.Free();

            DicConsole.DebugWriteLine("UkvFdi plugin", "hdr.addInfoLen = {0}", hdr.addInfoLen);
            DicConsole.DebugWriteLine("UkvFdi plugin", "hdr.cylinders = {0}", hdr.cylinders);
            DicConsole.DebugWriteLine("UkvFdi plugin", "hdr.dataOff = {0}", hdr.dataOff);
            DicConsole.DebugWriteLine("UkvFdi plugin", "hdr.descOff = {0}", hdr.descOff);
            DicConsole.DebugWriteLine("UkvFdi plugin", "hdr.flags = {0}", hdr.flags);
            DicConsole.DebugWriteLine("UkvFdi plugin", "hdr.heads = {0}", hdr.heads);

            stream.Seek(hdr.descOff, SeekOrigin.Begin);
            byte[] description = new byte[hdr.dataOff - hdr.descOff];
            stream.Read(description, 0, description.Length);
            ImageInfo.imageComments = StringHandlers.CToString(description);

            DicConsole.DebugWriteLine("UkvFdi plugin", "hdr.description = \"{0}\"", ImageInfo.imageComments);

            stream.Seek(0xE + hdr.addInfoLen, SeekOrigin.Begin);

            long spt = long.MaxValue;
            uint[][][] sectorsOff = new uint[hdr.cylinders][][];
            sectorsData = new byte[hdr.cylinders][][][];

            ImageInfo.cylinders = hdr.cylinders;
            ImageInfo.heads = hdr.heads;

            // Read track descriptors
            for(ushort cyl = 0; cyl < hdr.cylinders; cyl++)
            {
                sectorsOff[cyl] = new uint[hdr.heads][];
                sectorsData[cyl] = new byte[hdr.heads][][];

                for(ushort head = 0; head < hdr.heads; head++)
                {
                    byte[] sct_b = new byte[4];
                    stream.Read(sct_b, 0, 4);
                    stream.Seek(2, SeekOrigin.Current);
                    byte sectors = (byte)stream.ReadByte();
                    uint trkOff = BitConverter.ToUInt32(sct_b, 0);

                    DicConsole.DebugWriteLine("UkvFdi plugin", "trkhdr.c = {0}", cyl);
                    DicConsole.DebugWriteLine("UkvFdi plugin", "trkhdr.h = {0}", head);
                    DicConsole.DebugWriteLine("UkvFdi plugin", "trkhdr.sectors = {0}", sectors);
                    DicConsole.DebugWriteLine("UkvFdi plugin", "trkhdr.off = {0}", trkOff);

                    sectorsOff[cyl][head] = new uint[sectors];
                    sectorsData[cyl][head] = new byte[sectors][];

                    if(sectors < spt && sectors > 0) spt = sectors;

                    for(ushort sec = 0; sec < sectors; sec++)
                    {
                        byte c = (byte)stream.ReadByte();
                        byte h = (byte)stream.ReadByte();
                        byte r = (byte)stream.ReadByte();
                        byte n = (byte)stream.ReadByte();
                        SectorFlags f = (SectorFlags)stream.ReadByte();
                        byte[] off_b = new byte[2];
                        stream.Read(off_b, 0, 2);
                        ushort secOff = BitConverter.ToUInt16(off_b, 0);

                        DicConsole.DebugWriteLine("UkvFdi plugin", "sechdr.c = {0}", c);
                        DicConsole.DebugWriteLine("UkvFdi plugin", "sechdr.h = {0}", h);
                        DicConsole.DebugWriteLine("UkvFdi plugin", "sechdr.r = {0}", r);
                        DicConsole.DebugWriteLine("UkvFdi plugin", "sechdr.n = {0} ({1})", n, 128 << n);
                        DicConsole.DebugWriteLine("UkvFdi plugin", "sechdr.f = {0}", f);
                        DicConsole.DebugWriteLine("UkvFdi plugin", "sechdr.off = {0} ({1})", secOff,
                                                  secOff + trkOff + hdr.dataOff);

                        // TODO: This assumes sequential sectors.
                        sectorsOff[cyl][head][sec] = secOff + trkOff + hdr.dataOff;
                        sectorsData[cyl][head][sec] = new byte[128 << n];

                        if(128 << n > ImageInfo.sectorSize) ImageInfo.sectorSize = (uint)(128 << n);
                    }
                }
            }

            // Read sectors
            for(ushort cyl = 0; cyl < hdr.cylinders; cyl++)
            {
                bool emptyCyl = false;

                for(ushort head = 0; head < hdr.heads; head++)
                {
                    for(ushort sec = 0; sec < sectorsOff[cyl][head].Length; sec++)
                    {
                        stream.Seek(sectorsOff[cyl][head][sec], SeekOrigin.Begin);
                        stream.Read(sectorsData[cyl][head][sec], 0, sectorsData[cyl][head][sec].Length);
                    }

                    // For empty cylinders
                    if(sectorsOff[cyl][head].Length == 0)
                    {
                        // Last cylinder
                        if(cyl + 1 == hdr.cylinders ||
                           // Next cylinder is also empty
                           sectorsOff[cyl + 1][head].Length == 0) emptyCyl = true;
                        // Create empty sectors
                        else
                        {
                            sectorsData[cyl][head] = new byte[spt][];
                            for(int i = 0; i < spt; i++) sectorsData[cyl][head][i] = new byte[ImageInfo.sectorSize];
                        }
                    }
                }

                if(emptyCyl) ImageInfo.cylinders--;
            }

            // TODO: What about double sided, half track pitch compact floppies?
            ImageInfo.mediaType = MediaType.CompactFloppy;
            ImageInfo.imageSize = (ulong)stream.Length - hdr.dataOff;
            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.sectorsPerTrack = (uint)spt;
            ImageInfo.sectors = ImageInfo.cylinders * ImageInfo.heads * ImageInfo.sectorsPerTrack;
            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;

            return true;
        }

        public override bool ImageHasPartitions()
        {
            return false;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.imageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.sectorSize;
        }

        public override string GetImageFormat()
        {
            return "Spectrum floppy disk image";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.imageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.imageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.imageApplicationVersion;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.imageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.imageLastModificationTime;
        }

        public override string GetImageName()
        {
            return ImageInfo.imageName;
        }

        public override string GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            (ushort cylinder, byte head, byte sector) = LbaToChs(sectorAddress);

            if(cylinder >= sectorsData.Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(head >= sectorsData[cylinder].Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sector > sectorsData[cylinder][head].Length)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            return sectorsData[cylinder][head][sector - 1];
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream buffer = new MemoryStream();
            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                buffer.Write(sector, 0, sector.Length);
            }

            return buffer.ToArray();
        }

        (ushort cylinder, byte head, byte sector) LbaToChs(ulong lba)
        {
            ushort cylinder = (ushort)(lba / (ImageInfo.heads * ImageInfo.sectorsPerTrack));
            byte head = (byte)((lba / ImageInfo.sectorsPerTrack) % ImageInfo.heads);
            byte sector = (byte)((lba % ImageInfo.sectorsPerTrack) + 1);

            return (cylinder, head, sector);
        }

        #region Unsupported features
        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetMediaManufacturer()
        {
            return null;
        }

        public override string GetMediaModel()
        {
            return null;
        }

        public override string GetMediaSerialNumber()
        {
            return null;
        }

        public override string GetMediaBarcode()
        {
            return null;
        }

        public override string GetMediaPartNumber()
        {
            return null;
        }

        public override int GetMediaSequence()
        {
            return 0;
        }

        public override int GetLastDiskSequence()
        {
            return 0;
        }

        public override string GetDriveManufacturer()
        {
            return null;
        }

        public override string GetDriveModel()
        {
            return null;
        }

        public override string GetDriveSerialNumber()
        {
            return null;
        }

        public override List<Partition> GetPartitions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetTracks()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Session> GetSessions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            return null;
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs,
                                            out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();
            for(ulong i = 0; i < ImageInfo.sectors; i++) UnknownLBAs.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs,
                                            out List<ulong> UnknownLBAs)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }
        #endregion
    }
}