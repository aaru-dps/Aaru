// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SuperCardPro.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages SuperCardPro flux images.
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
    public class SuperCardPro : ImagePlugin
    {
        public enum ScpDiskType : byte
        {
            Commodore64 = 0x00,
            CommodoreAmiga = 0x04,
            AtariFMSS = 0x10,
            AtariFMDS = 0x11,
            AtariFSEx = 0x12,
            AtariSTSS = 0x14,
            AtariSTDS = 0x15,
            AppleII = 0x20,
            AppleIIPro = 0x21,
            Apple400K = 0x24,
            Apple800K = 0x25,
            Apple144 = 0x26,
            PC360K = 0x30,
            PC720K = 0x31,
            PC12M = 0x32,
            PC144M = 0x33,
            TandySSSD = 0x40,
            TandySSDD = 0x41,
            TandyDSSD = 0x42,
            TandyDSDD = 0x43,
            Ti994A = 0x50,
            RolandD20 = 0x60
        }

        [Flags]
        public enum ScpFlags : byte
        {
            /// <summary>
            ///     If set flux starts at index pulse
            /// </summary>
            Index = 0x00,
            /// <summary>
            ///     If set drive is 96tpi
            /// </summary>
            Tpi = 0x02,
            /// <summary>
            ///     If set drive is 360rpm
            /// </summary>
            Rpm = 0x04,
            /// <summary>
            ///     If set image contains normalized data
            /// </summary>
            Normalized = 0x08,
            /// <summary>
            ///     If set image is read/write capable
            /// </summary>
            Writable = 0x10,
            /// <summary>
            ///     If set, image has footer
            /// </summary>
            HasFooter = 0x20
        }

        /// <summary>
        ///     SuperCardPro footer signature: "FPCS"
        /// </summary>
        const uint FOOTER_SIGNATURE = 0x53435046;

        /// <summary>
        ///     SuperCardPro header signature: "SCP"
        /// </summary>
        readonly byte[] scpSignature = {0x53, 0x43, 0x50};
        /// <summary>
        ///     SuperCardPro track header signature: "TRK"
        /// </summary>
        readonly byte[] trkSignature = {0x54, 0x52, 0x4B};

        // TODO: These variables have been made public so create-sidecar can access to this information until I define an API >4.0
        public ScpHeader Header;
        Stream scpStream;
        public Dictionary<byte, TrackHeader> Tracks;

        public SuperCardPro()
        {
            Name = "SuperCardPro";
            PluginUuid = new Guid("C5D3182E-1D45-4767-A205-E6E5C83444DC");
            ImageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(),
                ReadableMediaTags = new List<MediaTagType>(),
                ImageHasPartitions = false,
                ImageHasSessions = false,
                ImageVersion = null,
                ImageApplication = null,
                ImageApplicationVersion = null,
                ImageCreator = null,
                ImageComments = null,
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

        public override bool IdentifyImage(Filter imageFilter)
        {
            Header = new ScpHeader();
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < Marshal.SizeOf(Header)) return false;

            byte[] hdr = new byte[Marshal.SizeOf(Header)];
            stream.Read(hdr, 0, Marshal.SizeOf(Header));

            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(Header));
            Marshal.Copy(hdr, 0, hdrPtr, Marshal.SizeOf(Header));
            Header = (ScpHeader)Marshal.PtrToStructure(hdrPtr, typeof(ScpHeader));
            Marshal.FreeHGlobal(hdrPtr);

            return scpSignature.SequenceEqual(Header.signature);
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Header = new ScpHeader();
            scpStream = imageFilter.GetDataForkStream();
            scpStream.Seek(0, SeekOrigin.Begin);
            if(scpStream.Length < Marshal.SizeOf(Header)) return false;

            byte[] hdr = new byte[Marshal.SizeOf(Header)];
            scpStream.Read(hdr, 0, Marshal.SizeOf(Header));

            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(Header));
            Marshal.Copy(hdr, 0, hdrPtr, Marshal.SizeOf(Header));
            Header = (ScpHeader)Marshal.PtrToStructure(hdrPtr, typeof(ScpHeader));
            Marshal.FreeHGlobal(hdrPtr);

            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.signature = \"{0}\"",
                                      StringHandlers.CToString(Header.signature));
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.version = {0}.{1}", (Header.version & 0xF0) >> 4,
                                      Header.version & 0xF);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.type = {0}", Header.type);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.revolutions = {0}", Header.revolutions);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.start = {0}", Header.start);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.end = {0}", Header.end);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.flags = {0}", Header.flags);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.bitCellEncoding = {0}", Header.bitCellEncoding);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.heads = {0}", Header.heads);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.reserved = {0}", Header.reserved);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.checksum = 0x{0:X8}", Header.checksum);

            if(!scpSignature.SequenceEqual(Header.signature)) return false;

            Tracks = new Dictionary<byte, TrackHeader>();

            for(byte t = Header.start; t <= Header.end; t++)
            {
                if(t >= Header.offsets.Length) break;

                scpStream.Position = Header.offsets[t];
                TrackHeader trk =
                    new TrackHeader {Signature = new byte[3], Entries = new TrackEntry[Header.revolutions]};
                scpStream.Read(trk.Signature, 0, trk.Signature.Length);
                trk.TrackNumber = (byte)scpStream.ReadByte();

                if(!trk.Signature.SequenceEqual(trkSignature))
                {
                    DicConsole.DebugWriteLine("SuperCardPro plugin",
                                              "Track header at {0} contains incorrect signature.", Header.offsets[t]);
                    continue;
                }

                if(trk.TrackNumber != t)
                {
                    DicConsole.DebugWriteLine("SuperCardPro plugin", "Track number at {0} should be {1} but is {2}.",
                                              Header.offsets[t], t, trk.TrackNumber);
                    continue;
                }

                DicConsole.DebugWriteLine("SuperCardPro plugin", "Found track {0} at {1}.", t, Header.offsets[t]);

                for(byte r = 0; r < Header.revolutions; r++)
                {
                    byte[] rev = new byte[Marshal.SizeOf(typeof(TrackEntry))];
                    scpStream.Read(rev, 0, Marshal.SizeOf(typeof(TrackEntry)));

                    IntPtr revPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TrackEntry)));
                    Marshal.Copy(rev, 0, revPtr, Marshal.SizeOf(typeof(TrackEntry)));
                    trk.Entries[r] = (TrackEntry)Marshal.PtrToStructure(revPtr, typeof(TrackEntry));
                    Marshal.FreeHGlobal(revPtr);
                    // De-relative offsets
                    trk.Entries[r].dataOffset += Header.offsets[t];
                }

                Tracks.Add(t, trk);
            }

            if(Header.flags.HasFlag(ScpFlags.HasFooter))
            {
                long position = scpStream.Position;
                scpStream.Seek(-4, SeekOrigin.End);

                while(scpStream.Position >= position)
                {
                    byte[] footerSig = new byte[4];
                    scpStream.Read(footerSig, 0, 4);
                    uint footerMagic = BitConverter.ToUInt32(footerSig, 0);

                    if(footerMagic == FOOTER_SIGNATURE)
                    {
                        scpStream.Seek(-Marshal.SizeOf(typeof(ScpFooter)), SeekOrigin.Current);

                        DicConsole.DebugWriteLine("SuperCardPro plugin", "Found footer at {0}", scpStream.Position);

                        byte[] ftr = new byte[Marshal.SizeOf(typeof(ScpFooter))];
                        scpStream.Read(ftr, 0, Marshal.SizeOf(typeof(ScpFooter)));

                        IntPtr ftrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ScpFooter)));
                        Marshal.Copy(ftr, 0, ftrPtr, Marshal.SizeOf(typeof(ScpFooter)));
                        ScpFooter footer = (ScpFooter)Marshal.PtrToStructure(ftrPtr, typeof(ScpFooter));
                        Marshal.FreeHGlobal(ftrPtr);

                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.manufacturerOffset = 0x{0:X8}",
                                                  footer.manufacturerOffset);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.modelOffset = 0x{0:X8}",
                                                  footer.modelOffset);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.serialOffset = 0x{0:X8}",
                                                  footer.serialOffset);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.creatorOffset = 0x{0:X8}",
                                                  footer.creatorOffset);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.applicationOffset = 0x{0:X8}",
                                                  footer.applicationOffset);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.commentsOffset = 0x{0:X8}",
                                                  footer.commentsOffset);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.creationTime = {0}",
                                                  footer.creationTime);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.modificationTime = {0}",
                                                  footer.modificationTime);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.applicationVersion = {0}.{1}",
                                                  (footer.applicationVersion & 0xF0) >> 4,
                                                  footer.applicationVersion & 0xF);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.hardwareVersion = {0}.{1}",
                                                  (footer.hardwareVersion & 0xF0) >> 4, footer.hardwareVersion & 0xF);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.firmwareVersion = {0}.{1}",
                                                  (footer.firmwareVersion & 0xF0) >> 4, footer.firmwareVersion & 0xF);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.imageVersion = {0}.{1}",
                                                  (footer.imageVersion & 0xF0) >> 4, footer.imageVersion & 0xF);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.signature = \"{0}\"",
                                                  StringHandlers.CToString(BitConverter.GetBytes(footer.signature)));

                        ImageInfo.DriveManufacturer = ReadPStringUtf8(scpStream, footer.manufacturerOffset);
                        ImageInfo.DriveModel = ReadPStringUtf8(scpStream, footer.modelOffset);
                        ImageInfo.DriveSerialNumber = ReadPStringUtf8(scpStream, footer.serialOffset);
                        ImageInfo.ImageCreator = ReadPStringUtf8(scpStream, footer.creatorOffset);
                        ImageInfo.ImageApplication = ReadPStringUtf8(scpStream, footer.applicationOffset);
                        ImageInfo.ImageComments = ReadPStringUtf8(scpStream, footer.commentsOffset);

                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.driveManufacturer = \"{0}\"",
                                                  ImageInfo.DriveManufacturer);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.driveModel = \"{0}\"",
                                                  ImageInfo.DriveModel);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.driveSerialNumber = \"{0}\"",
                                                  ImageInfo.DriveSerialNumber);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageCreator = \"{0}\"",
                                                  ImageInfo.ImageCreator);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageApplication = \"{0}\"",
                                                  ImageInfo.ImageApplication);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageComments = \"{0}\"",
                                                  ImageInfo.ImageComments);

                        ImageInfo.ImageCreationTime = footer.creationTime != 0
                                                          ? DateHandlers.UnixToDateTime(footer.creationTime)
                                                          : imageFilter.GetCreationTime();

                        ImageInfo.ImageLastModificationTime =
                            footer.modificationTime != 0
                                ? DateHandlers.UnixToDateTime(footer.modificationTime)
                                : imageFilter.GetLastWriteTime();

                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageCreationTime = {0}",
                                                  ImageInfo.ImageCreationTime);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageLastModificationTime = {0}",
                                                  ImageInfo.ImageLastModificationTime);

                        ImageInfo.ImageApplicationVersion =
                            $"{(footer.applicationVersion & 0xF0) >> 4}.{footer.applicationVersion & 0xF}";
                        ImageInfo.DriveFirmwareRevision =
                            $"{(footer.firmwareVersion & 0xF0) >> 4}.{footer.firmwareVersion & 0xF}";
                        ImageInfo.ImageVersion = $"{(footer.imageVersion & 0xF0) >> 4}.{footer.imageVersion & 0xF}";

                        break;
                    }

                    scpStream.Seek(-8, SeekOrigin.Current);
                }
            }
            else
            {
                ImageInfo.ImageApplication = "SuperCardPro";
                ImageInfo.ImageApplicationVersion = $"{(Header.version & 0xF0) >> 4}.{Header.version & 0xF}";
                ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
                ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
                ImageInfo.ImageVersion = "1.5";
            }

            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        static string ReadPStringUtf8(Stream stream, uint position)
        {
            if(position == 0) return null;

            stream.Position = position;
            byte[] lenB = new byte[2];
            stream.Read(lenB, 0, 2);
            ushort len = BitConverter.ToUInt16(lenB, 0);

            if(len == 0 || len + stream.Position >= stream.Length) return null;

            byte[] str = new byte[len];
            stream.Read(str, 0, len);

            return Encoding.UTF8.GetString(str);
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.ImageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.ImageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.Sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.SectorSize;
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override string GetImageFormat()
        {
            return "SuperCardPro";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.ImageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.ImageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.ImageApplicationVersion;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.ImageCreator;
        }

        // TODO: Check if it exists. If so, read it.
        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.ImageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.ImageLastModificationTime;
        }

        public override string GetImageName()
        {
            return ImageInfo.ImageName;
        }

        public override string GetImageComments()
        {
            return ImageInfo.ImageComments;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.MediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.MediaModel;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.MediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.MediaBarcode;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.MediaPartNumber;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.MediaType;
        }

        public override int GetMediaSequence()
        {
            return ImageInfo.MediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.LastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.DriveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.DriveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.DriveSerialNumber;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override bool? VerifyMediaImage()
        {
            if(Header.flags.HasFlag(ScpFlags.Writable)) return null;

            byte[] wholeFile = new byte[scpStream.Length];
            uint sum = 0;

            scpStream.Position = 0;
            scpStream.Read(wholeFile, 0, wholeFile.Length);

            for(int i = 0x10; i < wholeFile.Length; i++) sum += wholeFile[i];

            return Header.checksum == sum;
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
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

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
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

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ScpHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] signature;
            public byte version;
            public ScpDiskType type;
            public byte revolutions;
            public byte start;
            public byte end;
            public ScpFlags flags;
            public byte bitCellEncoding;
            public byte heads;
            public byte reserved;
            public uint checksum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 168)] public uint[] offsets;
        }

        public struct TrackHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] Signature;
            public byte TrackNumber;
            public TrackEntry[] Entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TrackEntry
        {
            public uint indexTime;
            public uint trackLength;
            public uint dataOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ScpFooter
        {
            public uint manufacturerOffset;
            public uint modelOffset;
            public uint serialOffset;
            public uint creatorOffset;
            public uint applicationOffset;
            public uint commentsOffset;
            public long creationTime;
            public long modificationTime;
            public byte applicationVersion;
            public byte hardwareVersion;
            public byte firmwareVersion;
            public byte imageVersion;
            public uint signature;
        }
    }
}