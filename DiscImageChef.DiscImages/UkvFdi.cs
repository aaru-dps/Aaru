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

namespace DiscImageChef.DiscImages
{
    public class UkvFdi : ImagePlugin
    {
        readonly byte[] signature = {0x46, 0x44, 0x49};

        // Cylinder by head, sector data matrix
        byte[][][][] sectorsData;

        public UkvFdi()
        {
            Name = "Spectrum Floppy Disk Image";
            PluginUuid = new Guid("DADFC9B2-67C1-42A3-B124-825528163FC0");
            ImageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                HasPartitions = false,
                HasSessions = false,
                Version = null,
                Application = null,
                ApplicationVersion = null,
                Creator = null,
                Comments = null,
                MediaManufacturer = null,
                MediaModel = null,
                MediaSerialNumber = null,
                MediaBarcode = null,
                MediaPartNumber = null,
                MediaSequence = 0,
                LastMediaSequence = 0,
                DriveManufacturer = null,
                DriveModel = null,
                DriveSerialNumber = null,
                DriveFirmwareRevision = null
            };
        }

        public override string ImageFormat => "Spectrum floppy disk image";

        public override List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public override List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public override List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            FdiHeader hdr = new FdiHeader();

            if(stream.Length < Marshal.SizeOf(hdr)) return false;

            byte[] hdrB = new byte[Marshal.SizeOf(hdr)];
            stream.Read(hdrB, 0, hdrB.Length);

            GCHandle handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
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

            byte[] hdrB = new byte[Marshal.SizeOf(hdr)];
            stream.Read(hdrB, 0, hdrB.Length);

            GCHandle handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
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
            ImageInfo.Comments = StringHandlers.CToString(description);

            DicConsole.DebugWriteLine("UkvFdi plugin", "hdr.description = \"{0}\"", ImageInfo.Comments);

            stream.Seek(0xE + hdr.addInfoLen, SeekOrigin.Begin);

            long spt = long.MaxValue;
            uint[][][] sectorsOff = new uint[hdr.cylinders][][];
            sectorsData = new byte[hdr.cylinders][][][];

            ImageInfo.Cylinders = hdr.cylinders;
            ImageInfo.Heads = hdr.heads;

            // Read track descriptors
            for(ushort cyl = 0; cyl < hdr.cylinders; cyl++)
            {
                sectorsOff[cyl] = new uint[hdr.heads][];
                sectorsData[cyl] = new byte[hdr.heads][][];

                for(ushort head = 0; head < hdr.heads; head++)
                {
                    byte[] sctB = new byte[4];
                    stream.Read(sctB, 0, 4);
                    stream.Seek(2, SeekOrigin.Current);
                    byte sectors = (byte)stream.ReadByte();
                    uint trkOff = BitConverter.ToUInt32(sctB, 0);

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
                        byte[] offB = new byte[2];
                        stream.Read(offB, 0, 2);
                        ushort secOff = BitConverter.ToUInt16(offB, 0);

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

                        if(128 << n > ImageInfo.SectorSize) ImageInfo.SectorSize = (uint)(128 << n);
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
                    if(sectorsOff[cyl][head].Length != 0) continue;

                    if(cyl + 1 == hdr.cylinders ||
                       // Next cylinder is also empty
                       sectorsOff[cyl + 1][head].Length == 0) emptyCyl = true;
                    // Create empty sectors
                    else
                    {
                        sectorsData[cyl][head] = new byte[spt][];
                        for(int i = 0; i < spt; i++) sectorsData[cyl][head][i] = new byte[ImageInfo.SectorSize];
                    }
                }

                if(emptyCyl) ImageInfo.Cylinders--;
            }

            // TODO: What about double sided, half track pitch compact floppies?
            ImageInfo.MediaType = MediaType.CompactFloppy;
            ImageInfo.ImageSize = (ulong)stream.Length - hdr.dataOff;
            ImageInfo.CreationTime = imageFilter.GetCreationTime();
            ImageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.MediaTitle = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.SectorsPerTrack = (uint)spt;
            ImageInfo.Sectors = ImageInfo.Cylinders * ImageInfo.Heads * ImageInfo.SectorsPerTrack;
            ImageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            return true;
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
            if(sectorAddress > ImageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > ImageInfo.Sectors)
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
            ushort cylinder = (ushort)(lba / (ImageInfo.Heads * ImageInfo.SectorsPerTrack));
            byte head = (byte)(lba / ImageInfo.SectorsPerTrack % ImageInfo.Heads);
            byte sector = (byte)(lba % ImageInfo.SectorsPerTrack + 1);

            return (cylinder, head, sector);
        }

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

        public override List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override List<Track> GetSessionTracks(ushort session)
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

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < ImageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }

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
            Deleted = 0x80
        }

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
    }
}