// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : D88.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Quasi88 disk images.
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
using DiscImageChef.Decoders.Floppy;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    // Information from Quasi88's FORMAT.TXT file
    // Japanese comments copied from there
    // TODO: Solve media types
    public class D88 : IMediaImage
    {
        const byte READ_ONLY = 0x10;

        readonly byte[] reservedEmpty = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        ImageInfo       imageInfo;

        List<byte[]> sectorsData;

        public D88()
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

        public string    Name => "D88 Disk Image";
        public Guid      Id   => new Guid("669EDC77-EC41-4720-A88C-49C38CFFBAA0");
        public ImageInfo Info => imageInfo;

        public string Format => "D88 disk image";

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
            // Even if disk name is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
            Encoding shiftjis = Encoding.GetEncoding("shift_jis");

            D88Header d88Hdr = new D88Header();

            if(stream.Length < Marshal.SizeOf(d88Hdr)) return false;

            byte[] hdrB = new byte[Marshal.SizeOf(d88Hdr)];
            stream.Read(hdrB, 0, hdrB.Length);

            GCHandle handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
            d88Hdr          = (D88Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(D88Header));
            handle.Free();

            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.name = \"{0}\"",
                                      StringHandlers.CToString(d88Hdr.name, shiftjis));
            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.reserved is empty? = {0}",
                                      d88Hdr.reserved.SequenceEqual(reservedEmpty));
            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.write_protect = 0x{0:X2}", d88Hdr.write_protect);
            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_type = {0} ({1})", d88Hdr.disk_type,
                                      (byte)d88Hdr.disk_type);
            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_size = {0}", d88Hdr.disk_size);

            if(d88Hdr.disk_size != stream.Length) return false;

            if(d88Hdr.disk_type != DiskType.D2 && d88Hdr.disk_type != DiskType.Dd2 &&
               d88Hdr.disk_type != DiskType.Hd2) return false;

            if(!d88Hdr.reserved.SequenceEqual(reservedEmpty)) return false;

            int counter = 0;
            foreach(int t in d88Hdr.track_table)
            {
                if(t > 0) counter++;

                if(t < 0 || t > stream.Length) return false;
            }

            DicConsole.DebugWriteLine("D88 plugin", "{0} tracks", counter);

            return counter > 0;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            // Even if disk name is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
            Encoding shiftjis = Encoding.GetEncoding("shift_jis");

            D88Header d88Hdr = new D88Header();

            if(stream.Length < Marshal.SizeOf(d88Hdr)) return false;

            byte[] hdrB = new byte[Marshal.SizeOf(d88Hdr)];
            stream.Read(hdrB, 0, hdrB.Length);

            GCHandle handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
            d88Hdr          = (D88Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(D88Header));
            handle.Free();

            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.name = \"{0}\"",
                                      StringHandlers.CToString(d88Hdr.name, shiftjis));
            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.reserved is empty? = {0}",
                                      d88Hdr.reserved.SequenceEqual(reservedEmpty));
            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.write_protect = 0x{0:X2}", d88Hdr.write_protect);
            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_type = {0} ({1})", d88Hdr.disk_type,
                                      (byte)d88Hdr.disk_type);
            DicConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_size = {0}", d88Hdr.disk_size);

            if(d88Hdr.disk_size != stream.Length) return false;

            if(d88Hdr.disk_type != DiskType.D2 && d88Hdr.disk_type != DiskType.Dd2 &&
               d88Hdr.disk_type != DiskType.Hd2) return false;

            if(!d88Hdr.reserved.SequenceEqual(reservedEmpty)) return false;

            int trkCounter = 0;
            foreach(int t in d88Hdr.track_table)
            {
                if(t > 0) trkCounter++;

                if(t < 0 || t > stream.Length) return false;
            }

            DicConsole.DebugWriteLine("D88 plugin", "{0} tracks", trkCounter);

            if(trkCounter == 0) return false;

            SectorHeader sechdr = new SectorHeader();
            hdrB                = new byte[Marshal.SizeOf(sechdr)];
            stream.Seek(d88Hdr.track_table[0], SeekOrigin.Begin);
            stream.Read(hdrB, 0, hdrB.Length);

            handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
            sechdr = (SectorHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(SectorHeader));
            handle.Free();

            DicConsole.DebugWriteLine("D88 plugin", "sechdr.c = {0}",            sechdr.c);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.h = {0}",            sechdr.h);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.r = {0}",            sechdr.r);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.n = {0}",            sechdr.n);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.spt = {0}",          sechdr.spt);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.density = {0}",      sechdr.density);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.deleted_mark = {0}", sechdr.deleted_mark);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.status = {0}",       sechdr.status);
            DicConsole.DebugWriteLine("D88 plugin", "sechdr.size_of_data = {0}", sechdr.size_of_data);

            short             spt      = sechdr.spt;
            IBMSectorSizeCode bps      = sechdr.n;
            bool              allEqual = true;
            sectorsData                = new List<byte[]>();

            for(int i = 0; i < trkCounter; i++)
            {
                stream.Seek(d88Hdr.track_table[i], SeekOrigin.Begin);
                stream.Read(hdrB, 0, hdrB.Length);
                SortedDictionary<byte, byte[]> sectors = new SortedDictionary<byte, byte[]>();

                handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
                sechdr = (SectorHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(SectorHeader));
                handle.Free();

                if(sechdr.spt != spt || sechdr.n != bps)
                {
                    DicConsole.DebugWriteLine("D88 plugin",
                                              "Disk tracks are not same size. spt = {0} (expected {1}), bps = {2} (expected {3}) at track {4} sector {5}",
                                              sechdr.spt, spt, sechdr.n, bps, i, 0);
                    allEqual = false;
                }

                short  maxJ = sechdr.spt;
                byte[] secB;
                for(short j = 1; j < maxJ; j++)
                {
                    secB = new byte[sechdr.size_of_data];
                    stream.Read(secB, 0, secB.Length);
                    sectors.Add(sechdr.r, secB);
                    stream.Read(hdrB, 0, hdrB.Length);

                    handle = GCHandle.Alloc(hdrB, GCHandleType.Pinned);
                    sechdr = (SectorHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(SectorHeader));
                    handle.Free();

                    if(sechdr.spt == spt && sechdr.n == bps) continue;

                    DicConsole.DebugWriteLine("D88 plugin",
                                              "Disk tracks are not same size. spt = {0} (expected {1}), bps = {2} (expected {3}) at track {4} sector {5}",
                                              sechdr.spt, spt, sechdr.n, bps, i, j, sechdr.deleted_mark);
                    allEqual = false;
                }

                secB = new byte[sechdr.size_of_data];
                stream.Read(secB, 0, secB.Length);
                sectors.Add(sechdr.r, secB);

                foreach(KeyValuePair<byte, byte[]> kvp in sectors) sectorsData.Add(kvp.Value);
            }

            DicConsole.DebugWriteLine("D88 plugin", "{0} sectors", sectorsData.Count);

            /*
            FileStream debugStream = new FileStream("debug.img", FileMode.CreateNew, FileAccess.ReadWrite);
            for(int i = 0; i < sectorsData.Count; i++)
                debugStream.Write(sectorsData[i], 0, sectorsData[i].Length);
            debugStream.Close();
            */

            imageInfo.MediaType = MediaType.Unknown;
            if(allEqual)
                if(trkCounter == 154 && spt == 26 && bps == IBMSectorSizeCode.EighthKilo)
                    imageInfo.MediaType = MediaType.NEC_8_SD;
                else if(bps == IBMSectorSizeCode.QuarterKilo)
                    switch(trkCounter)
                    {
                        case 80 when spt == 16:
                            imageInfo.MediaType = MediaType.NEC_525_SS;
                            break;
                        case 154 when spt == 26:
                            imageInfo.MediaType = MediaType.NEC_8_DD;
                            break;
                        case 160 when spt == 16:
                            imageInfo.MediaType = MediaType.NEC_525_DS;
                            break;
                    }
                else if(trkCounter == 154 && spt == 8 && bps == IBMSectorSizeCode.Kilo)
                    imageInfo.MediaType = MediaType.NEC_525_HD;
                else if(bps == IBMSectorSizeCode.HalfKilo)
                    switch(d88Hdr.track_table.Length)
                    {
                        case 40:
                        {
                            switch(spt)
                            {
                                case 8:
                                    imageInfo.MediaType = MediaType.DOS_525_SS_DD_8;
                                    break;
                                case 9:
                                    imageInfo.MediaType = MediaType.DOS_525_SS_DD_9;
                                    break;
                            }
                        }

                            break;
                        case 80:
                        {
                            switch(spt)
                            {
                                case 8:
                                    imageInfo.MediaType = MediaType.DOS_525_DS_DD_8;
                                    break;
                                case 9:
                                    imageInfo.MediaType = MediaType.DOS_525_DS_DD_9;
                                    break;
                            }
                        }

                            break;
                        case 160:
                        {
                            switch(spt)
                            {
                                case 15:
                                    imageInfo.MediaType = MediaType.NEC_35_HD_15;
                                    break;
                                case 9:
                                    imageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                                    break;
                                case 18:
                                    imageInfo.MediaType = MediaType.DOS_35_HD;
                                    break;
                                case 36:
                                    imageInfo.MediaType = MediaType.DOS_35_ED;
                                    break;
                            }
                        }

                            break;
                        case 480:
                            if(spt == 38) imageInfo.MediaType = MediaType.NEC_35_TD;
                            break;
                    }

            DicConsole.DebugWriteLine("D88 plugin", "MediaType: {0}", imageInfo.MediaType);

            imageInfo.ImageSize            = (ulong)d88Hdr.disk_size;
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.Sectors              = (ulong)sectorsData.Count;
            imageInfo.Comments             = StringHandlers.CToString(d88Hdr.name, shiftjis);
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.SectorSize           = (uint)(128 << (int)bps);

            switch(imageInfo.MediaType)
            {
                case MediaType.NEC_525_SS:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 1;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.NEC_8_SD:
                case MediaType.NEC_8_DD:
                    imageInfo.Cylinders       = 77;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 26;
                    break;
                case MediaType.NEC_525_DS:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.NEC_525_HD:
                    imageInfo.Cylinders       = 77;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.DOS_525_SS_DD_8:
                    imageInfo.Cylinders       = 40;
                    imageInfo.Heads           = 1;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.DOS_525_SS_DD_9:
                    imageInfo.Cylinders       = 40;
                    imageInfo.Heads           = 1;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_525_DS_DD_8:
                    imageInfo.Cylinders       = 40;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 8;
                    break;
                case MediaType.DOS_525_DS_DD_9:
                    imageInfo.Cylinders       = 40;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.NEC_35_HD_15:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 15;
                    break;
                case MediaType.DOS_35_DS_DD_9:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 9;
                    break;
                case MediaType.DOS_35_HD:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 18;
                    break;
                case MediaType.DOS_35_ED:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 36;
                    break;
                case MediaType.NEC_35_TD:
                    imageInfo.Cylinders       = 240;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 38;
                    break;
            }

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

            MemoryStream buffer = new MemoryStream();
            for(int i = 0; i < length; i++)
                buffer.Write(sectorsData[(int)sectorAddress + i], 0, sectorsData[(int)sectorAddress + i].Length);

            return buffer.ToArray();
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

        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                   out                                   List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            for(ulong i = 0; i < imageInfo.Sectors; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out                                               List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public bool? VerifyMediaImage()
        {
            return null;
        }

        enum DiskType : byte
        {
            D2  = 0x00,
            Dd2 = 0x10,
            Hd2 = 0x20
        }

        enum DensityType : byte
        {
            Mfm = 0x00,
            Fm  = 0x40
        }

        /// <summary>
        ///     Status as returned by PC-98 BIOS
        ///     ステータスは、PC-98x1 のBIOS が返してくるステータスで、
        /// </summary>
        enum StatusType : byte
        {
            /// <summary>
            ///     Normal
            ///     正常
            /// </summary>
            Normal = 0x00,
            /// <summary>
            ///     Deleted
            ///     正常(DELETED DATA)
            /// </summary>
            Deleted = 0x10,
            /// <summary>
            ///     CRC error in address fields
            ///     ID CRC エラー
            /// </summary>
            IdError = 0xA0,
            /// <summary>
            ///     CRC error in data block
            ///     データ CRC エラー
            /// </summary>
            DataError = 0xB0,
            /// <summary>
            ///     Address mark not found
            ///     アドレスマークなし
            /// </summary>
            AddressMarkNotFound = 0xE0,
            /// <summary>
            ///     Data mark not found
            ///     データマークなし
            /// </summary>
            DataMarkNotFound = 0xF0
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct D88Header
        {
            /// <summary>
            ///     Disk name, nul-terminated ASCII
            ///     ディスクの名前(ASCII + '\0')
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] name;
            /// <summary>
            ///     Reserved
            ///     ディスクの名前(ASCII + '\0')
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public byte[] reserved;
            /// <summary>
            ///     Write protect status
            ///     ライトプロテクト： 0x00 なし、0x10 あり
            /// </summary>
            public byte write_protect;
            /// <summary>
            ///     Disk type
            ///     ディスクの種類： 0x00 2D、 0x10 2DD、 0x20 2HD
            /// </summary>
            public DiskType disk_type;
            /// <summary>
            ///     Disk image size
            ///     ディスクのサイズ
            /// </summary>
            public int disk_size;
            /// <summary>
            ///     Track pointers
            ///     トラック部のオフセットテーブル 0 Track ～ 163 Track
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 164)]
            public int[] track_table;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SectorHeader
        {
            /// <summary>
            ///     Cylinder
            ///     ID の C
            /// </summary>
            public byte c;
            /// <summary>
            ///     Head
            ///     ID の H
            /// </summary>
            public byte h;
            /// <summary>
            ///     Sector number
            ///     ID の R
            /// </summary>
            public byte r;
            /// <summary>
            ///     Sector size
            ///     ID の N
            /// </summary>
            public IBMSectorSizeCode n;
            /// <summary>
            ///     Number of sectors in this track
            ///     このトラック内に存在するセクタの数
            /// </summary>
            public short spt;
            /// <summary>
            ///     Density: 0x00 MFM, 0x40 FM
            ///     記録密度： 0x00 倍密度、0x40 単密度
            /// </summary>
            public DensityType density;
            /// <summary>
            ///     Deleted sector, 0x00 not deleted, 0x10 deleted
            ///     DELETED MARK： 0x00 ノーマル、 0x10 DELETED
            /// </summary>
            public byte deleted_mark;
            /// <summary>
            ///     Sector status
            ///     ステータス
            /// </summary>
            public byte status;
            /// <summary>
            ///     Reserved
            ///     リザーブ
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] reserved;
            /// <summary>
            ///     Size of data following this field
            ///     このセクタ部のデータサイズ
            /// </summary>
            public short size_of_data;
        }
    }
}