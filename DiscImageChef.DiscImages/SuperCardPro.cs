// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SuperCardPro.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages SuperCardPro disk images.
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
// Copyright © 2011-2017 Natalia Portillo
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

namespace DiscImageChef.ImagePlugins
{
    public class SuperCardPro : ImagePlugin
    {
        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ScpHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] signature;
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 168)]
            public uint[] offsets;
        }

        public struct TrackHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] signature;
            public byte trackNumber;
            public TrackEntry[] entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TrackEntry
        {
            public uint indexTime;
            public uint trackLength;
            public uint dataOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ScpFooter
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
        #endregion Internal Structures

        #region Internal Constants
        /// <summary>
        /// SuperCardPro header signature: "SCP"
        /// </summary>
        readonly byte[] ScpSignature = { 0x53, 0x43, 0x50 };
        /// <summary>
        /// SuperCardPro track header signature: "TRK"
        /// </summary>
        readonly byte[] TrkSignature = { 0x54, 0x52, 0x4B };
        /// <summary>
        /// SuperCardPro footer signature: "FPCS"
        /// </summary>
        const uint FooterSignature = 0x53435046;

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
            /// If set flux starts at index pulse
            /// </summary>
            Index = 0x00,
            /// <summary>
            /// If set drive is 96tpi
            /// </summary>
            Tpi = 0x02,
            /// <summary>
            /// If set drive is 360rpm
            /// </summary>
            Rpm = 0x04,
            /// <summary>
            /// If set image contains normalized data
            /// </summary>
            Normalized = 0x08,
            /// <summary>
            /// If set image is read/write capable
            /// </summary>
            Writable = 0x10,
            /// <summary>
            /// If set, image has footer
            /// </summary>
            HasFooter = 0x20,
        }
        #endregion Internal Constants

        #region Internal variables
        // TODO: These variables have been made public so create-sidecar can access to this information until I define an API >4.0
        public ScpHeader header;
        public Dictionary<byte, TrackHeader> tracks;
        Stream scpStream;
        #endregion Internal variables

        public SuperCardPro()
        {
            Name = "SuperCardPro";
            PluginUUID = new Guid("C5D3182E-1D45-4767-A205-E6E5C83444DC");
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

        #region Public methods
        public override bool IdentifyImage(Filter imageFilter)
        {
            header = new ScpHeader();
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < Marshal.SizeOf(header))
                return false;

            byte[] hdr = new byte[Marshal.SizeOf(header)];
            stream.Read(hdr, 0, Marshal.SizeOf(header));

            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            Marshal.Copy(hdr, 0, hdrPtr, Marshal.SizeOf(header));
            header = (ScpHeader)Marshal.PtrToStructure(hdrPtr, typeof(ScpHeader));
            Marshal.FreeHGlobal(hdrPtr);

            return ScpSignature.SequenceEqual(header.signature);
        }

        public override bool OpenImage(Filter imageFilter)
        {
            header = new ScpHeader();
            scpStream = imageFilter.GetDataForkStream();
            scpStream.Seek(0, SeekOrigin.Begin);
            if(scpStream.Length < Marshal.SizeOf(header))
                return false;

            byte[] hdr = new byte[Marshal.SizeOf(header)];
            scpStream.Read(hdr, 0, Marshal.SizeOf(header));

            IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            Marshal.Copy(hdr, 0, hdrPtr, Marshal.SizeOf(header));
            header = (ScpHeader)Marshal.PtrToStructure(hdrPtr, typeof(ScpHeader));
            Marshal.FreeHGlobal(hdrPtr);

            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.signature = \"{0}\"", StringHandlers.CToString(header.signature));
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.version = {0}.{1}", (header.version & 0xF0) >> 4, header.version & 0xF);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.type = {0}", header.type);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.revolutions = {0}", header.revolutions);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.start = {0}", header.start);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.end = {0}", header.end);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.flags = {0}", header.flags);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.bitCellEncoding = {0}", header.bitCellEncoding);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.heads = {0}", header.heads);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.reserved = {0}", header.reserved);
            DicConsole.DebugWriteLine("SuperCardPro plugin", "header.checksum = 0x{0:X8}", header.checksum);

            if(!ScpSignature.SequenceEqual(header.signature))
                return false;

            tracks = new Dictionary<byte, TrackHeader>();

            for(byte t = header.start; t <= header.end; t++)
            {
                if(t >= header.offsets.Length)
                    break;

                scpStream.Position = header.offsets[t];
                TrackHeader trk = new TrackHeader();
                trk.signature = new byte[3];
                trk.entries = new TrackEntry[header.revolutions];
                scpStream.Read(trk.signature, 0, trk.signature.Length);
                trk.trackNumber = (byte)scpStream.ReadByte();

                if(!trk.signature.SequenceEqual(TrkSignature))
                {
                    DicConsole.DebugWriteLine("SuperCardPro plugin", "Track header at {0} contains incorrect signature.", header.offsets[t]);
                    continue;
                }

                if(trk.trackNumber != t)
                {
                    DicConsole.DebugWriteLine("SuperCardPro plugin", "Track number at {0} should be {1} but is {2}.", header.offsets[t], t, trk.trackNumber);
                    continue;
                }

                DicConsole.DebugWriteLine("SuperCardPro plugin", "Found track {0} at {1}.", t, header.offsets[t]);

                for(byte r = 0; r < header.revolutions; r++)
                {
                    byte[] rev = new byte[Marshal.SizeOf(typeof(TrackEntry))];
                    scpStream.Read(rev, 0, Marshal.SizeOf(typeof(TrackEntry)));

                    IntPtr revPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TrackEntry)));
                    Marshal.Copy(rev, 0, revPtr, Marshal.SizeOf(typeof(TrackEntry)));
                    trk.entries[r] = (TrackEntry)Marshal.PtrToStructure(revPtr, typeof(TrackEntry));
                    Marshal.FreeHGlobal(revPtr);
                    // De-relative offsets
                    trk.entries[r].dataOffset += header.offsets[t];
                }

                tracks.Add(t, trk);
            }

            if(header.flags.HasFlag(ScpFlags.HasFooter))
            {
                long position = scpStream.Position;
                scpStream.Seek(-4, SeekOrigin.End);
                
                while(scpStream.Position >= position)
                {
                    byte[] footerSig = new byte[4];
                    scpStream.Read(footerSig, 0, 4);
                    uint footerMagic = BitConverter.ToUInt32(footerSig, 0);

                    if(footerMagic == FooterSignature)
                    {
                        scpStream.Seek(-Marshal.SizeOf(typeof(ScpFooter)), SeekOrigin.Current);
                        
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "Found footer at {0}", scpStream.Position);
                        
                        byte[] ftr = new byte[Marshal.SizeOf(typeof(ScpFooter))];
                        scpStream.Read(ftr, 0, Marshal.SizeOf(typeof(ScpFooter)));

                        IntPtr ftrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ScpFooter)));
                        Marshal.Copy(ftr, 0, ftrPtr, Marshal.SizeOf(typeof(ScpFooter)));
                        ScpFooter footer = (ScpFooter)Marshal.PtrToStructure(ftrPtr, typeof(ScpFooter));
                        Marshal.FreeHGlobal(ftrPtr);

                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.manufacturerOffset = 0x{0:X8}", footer.manufacturerOffset);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.modelOffset = 0x{0:X8}", footer.modelOffset);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.serialOffset = 0x{0:X8}", footer.serialOffset);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.creatorOffset = 0x{0:X8}", footer.creatorOffset);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.applicationOffset = 0x{0:X8}", footer.applicationOffset);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.commentsOffset = 0x{0:X8}", footer.commentsOffset);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.creationTime = {0}", footer.creationTime);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.modificationTime = {0}", footer.modificationTime);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.applicationVersion = {0}.{1}", (footer.applicationVersion & 0xF0) >> 4, footer.applicationVersion & 0xF);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.hardwareVersion = {0}.{1}", (footer.hardwareVersion & 0xF0) >> 4, footer.hardwareVersion & 0xF);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.firmwareVersion = {0}.{1}", (footer.firmwareVersion & 0xF0) >> 4, footer.firmwareVersion & 0xF);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.imageVersion = {0}.{1}", (footer.imageVersion & 0xF0) >> 4, footer.imageVersion & 0xF);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "footer.signature = \"{0}\"", StringHandlers.CToString(BitConverter.GetBytes(footer.signature)));

                        ImageInfo.driveManufacturer = ReadPStringUTF8(scpStream, footer.manufacturerOffset);
                        ImageInfo.driveModel = ReadPStringUTF8(scpStream, footer.modelOffset);
                        ImageInfo.driveSerialNumber = ReadPStringUTF8(scpStream, footer.serialOffset);
                        ImageInfo.imageCreator = ReadPStringUTF8(scpStream, footer.creatorOffset);
                        ImageInfo.imageApplication = ReadPStringUTF8(scpStream, footer.applicationOffset);
                        ImageInfo.imageComments = ReadPStringUTF8(scpStream, footer.commentsOffset);

                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.driveManufacturer = \"{0}\"", ImageInfo.driveManufacturer);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.driveModel = \"{0}\"", ImageInfo.driveModel);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.driveSerialNumber = \"{0}\"", ImageInfo.driveSerialNumber);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageCreator = \"{0}\"", ImageInfo.imageCreator);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageApplication = \"{0}\"", ImageInfo.imageApplication);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageComments = \"{0}\"", ImageInfo.imageComments);

                        if(footer.creationTime != 0)
                            ImageInfo.imageCreationTime = DateHandlers.UNIXToDateTime(footer.creationTime);
                        else
                            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();

                        if(footer.modificationTime != 0)
                            ImageInfo.imageLastModificationTime = DateHandlers.UNIXToDateTime(footer.modificationTime);
                        else
                            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();

                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageCreationTime = {0}", ImageInfo.imageCreationTime);
                        DicConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageLastModificationTime = {0}", ImageInfo.imageLastModificationTime);

                        ImageInfo.imageApplicationVersion =
                            string.Format("{0}.{1}", (footer.applicationVersion & 0xF0) >> 4, footer.applicationVersion & 0xF);
                        ImageInfo.driveFirmwareRevision =
                            string.Format("{0}.{1}", (footer.firmwareVersion & 0xF0) >> 4, footer.firmwareVersion & 0xF);
                        ImageInfo.imageVersion =
                            string.Format("{0}.{1}", (footer.imageVersion & 0xF0) >> 4, footer.imageVersion & 0xF);
                        
                        break;
                    }
                    
                    scpStream.Seek(-8, SeekOrigin.Current);
                }
            }
            else
            {
                ImageInfo.imageApplication = "SuperCardPro";
                ImageInfo.imageApplicationVersion =
                    string.Format("{0}.{1}", (header.version & 0xF0) >> 4, header.version & 0xF);
                ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
                ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
                ImageInfo.imageVersion = "1.5";
            }

            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        string ReadPStringUTF8(Stream stream, uint position)
        {
            if(position == 0)
                return null;

            stream.Position = position;
            byte[] len_b = new byte[2];
            stream.Read(len_b, 0, 2);
            ushort len = BitConverter.ToUInt16(len_b, 0);

            if(len == 0 || len + stream.Position >= stream.Length)
                return null;

            byte[] str = new byte[len];
            stream.Read(str, 0, len);

            return Encoding.UTF8.GetString(str);
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.imageHasPartitions;
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

        // TODO: Check if it exists. If so, read it.
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

        public override string GetMediaManufacturer()
        {
            return ImageInfo.mediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.mediaModel;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.mediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.mediaBarcode;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.mediaPartNumber;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
        }

        public override int GetMediaSequence()
        {
            return ImageInfo.mediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.lastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.driveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.driveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.driveSerialNumber;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public override bool? VerifyMediaImage()
        {
            if(header.flags.HasFlag(ScpFlags.Writable))
                return null;

            byte[] wholeFile = new byte[scpStream.Length];
            uint sum = 0;

            scpStream.Position = 0;
            scpStream.Read(wholeFile, 0, wholeFile.Length);

            for(int i = 0x10; i < wholeFile.Length; i++)
                sum += wholeFile[i];

            return header.checksum == sum;
        }
        #endregion Public methods

        #region Unsupported features
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

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        #endregion Unsupported features
    }
}