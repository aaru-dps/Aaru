// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DiskCopy42.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Apple DiskCopy 4.2 disc images, including unofficial modifications.
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
using System.Text.RegularExpressions;
using Claunia.Encoding;
using Claunia.RsrcFork;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using Encoding = System.Text.Encoding;
using Version = Resources.Version;

namespace DiscImageChef.DiscImages
{
    // Checked using several images and strings inside Apple's DiskImages.framework
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class DiskCopy42 : IWritableImage
    {
        // format byte
        /// <summary>3.5", single side, double density, GCR</summary>
        const byte kSonyFormat400K = 0x00;
        /// <summary>3.5", double side, double density, GCR</summary>
        const byte kSonyFormat800K = 0x01;
        /// <summary>3.5", double side, double density, MFM</summary>
        const byte kSonyFormat720K = 0x02;
        /// <summary>3.5", double side, high density, MFM</summary>
        const byte kSonyFormat1440K = 0x03;
        /// <summary>3.5", double side, high density, MFM, 21 sectors/track (aka, Microsoft DMF)</summary>
        // Unchecked value</summary>
        const byte kSonyFormat1680K = 0x04;
        /// <summary>Defined by Sigma Seven's BLU</summary>
        const byte kSigmaFormatTwiggy = 0x54;
        /// <summary>Defined by LisaEm</summary>
        const byte kNotStandardFormat = 0x5D;
        // There should be a value for Apple HD20 hard disks, unknown...
        // fmyByte byte
        // Based on GCR nibble
        // Always 0x02 for MFM disks
        // Unknown for Apple HD20
        /// <summary>Defined by Sigma Seven's BLU</summary>
        const byte kSigmaFmtByteTwiggy = 0x01;
        /// <summary>3.5" single side double density GCR and MFM all use same code</summary>
        const byte kSonyFmtByte400K  = 0x02;
        const byte kSonyFmtByte720K  = kSonyFmtByte400K;
        const byte kSonyFmtByte1440K = kSonyFmtByte400K;
        const byte kSonyFmtByte1680K = kSonyFmtByte400K;
        /// <summary>3.5" double side double density GCR, 512 bytes/sector, interleave 2:1</summary>
        const byte kSonyFmtByte800K = 0x22;
        /// <summary>
        ///     3.5" double side double density GCR, 512 bytes/sector, interleave 2:1, incorrect value (but appears on
        ///     official documentation)
        /// </summary>
        const byte kSonyFmtByte800KIncorrect = 0x12;
        /// <summary>3.5" double side double density GCR, ProDOS format, interleave 4:1</summary>
        const byte kSonyFmtByteProDos = 0x24;
        /// <summary>Unformatted sectors</summary>
        const byte kInvalidFmtByte = 0x96;
        /// <summary>Defined by LisaEm</summary>
        const byte kFmtNotStandard = 0x93;
        /// <summary>Used incorrectly by Mac OS X with certaing disk images</summary>
        const byte   kMacOSXFmtByte = 0x00;
        const string REGEX_DCPY     = @"(?<application>\S+)\s(?<version>\S+)\rData checksum=\$(?<checksum>\S+)$";
        /// <summary>Bytes per tag, should be 12</summary>
        uint bptag;

        /// <summary>Start of data sectors in disk image, should be 0x58</summary>
        uint dataOffset;
        /// <summary>Disk image file</summary>
        IFilter dc42ImageFilter;
        /// <summary>Header of opened image</summary>
        Dc42Header header;
        ImageInfo  imageInfo;
        ulong      sectorsToWrite;
        /// <summary>Start of tags in disk image, after data sectors</summary>
        uint         tagOffset;
        bool         twiggy;
        byte[]       twiggyCache;
        byte[]       twiggyCacheTags;
        MemoryStream twiggyDataCache;
        MemoryStream twiggyTagCache;

        FileStream writingStream;

        public DiskCopy42()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Version               = "4.2",
                Application           = "Apple DiskCopy",
                ApplicationVersion    = "4.2",
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
        public string    Name => "Apple DiskCopy 4.2";
        public Guid      Id   => new Guid("0240B7B1-E959-4CDC-B0BD-386D6E467B88");

        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer  = new byte[0x58];
            byte[] pString = new byte[64];
            stream.Read(buffer, 0, 0x58);

            // Incorrect pascal string length, not DC42
            if(buffer[0] > 63) return false;

            Dc42Header tmpHeader = new Dc42Header();

            Array.Copy(buffer, 0, pString, 0, 64);

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            tmpHeader.DiskName     = StringHandlers.PascalToString(pString, Encoding.GetEncoding("macintosh"));
            tmpHeader.DataSize     = BigEndianBitConverter.ToUInt32(buffer, 0x40);
            tmpHeader.TagSize      = BigEndianBitConverter.ToUInt32(buffer, 0x44);
            tmpHeader.DataChecksum = BigEndianBitConverter.ToUInt32(buffer, 0x48);
            tmpHeader.TagChecksum  = BigEndianBitConverter.ToUInt32(buffer, 0x4C);
            tmpHeader.Format       = buffer[0x50];
            tmpHeader.FmtByte      = buffer[0x51];
            tmpHeader.Valid        = buffer[0x52];
            tmpHeader.Reserved     = buffer[0x53];

            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.diskName = \"{0}\"",      tmpHeader.DiskName);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.dataSize = {0} bytes",    tmpHeader.DataSize);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.tagSize = {0} bytes",     tmpHeader.TagSize);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.dataChecksum = 0x{0:X8}", tmpHeader.DataChecksum);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.tagChecksum = 0x{0:X8}",  tmpHeader.TagChecksum);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.format = 0x{0:X2}",       tmpHeader.Format);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.fmtByte = 0x{0:X2}",      tmpHeader.FmtByte);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.valid = {0}",             tmpHeader.Valid);
            DicConsole.DebugWriteLine("DC42 plugin", "tmp_header.reserved = {0}",          tmpHeader.Reserved);

            if(tmpHeader.Valid != 1 || tmpHeader.Reserved != 0) return false;

            // Some versions seem to incorrectly create little endian fields
            if(tmpHeader.DataSize + tmpHeader.TagSize + 0x54 != imageFilter.GetDataForkLength() &&
               tmpHeader.Format                              != kSigmaFormatTwiggy)
            {
                tmpHeader.DataSize     = BitConverter.ToUInt32(buffer, 0x40);
                tmpHeader.TagSize      = BitConverter.ToUInt32(buffer, 0x44);
                tmpHeader.DataChecksum = BitConverter.ToUInt32(buffer, 0x48);
                tmpHeader.TagChecksum  = BitConverter.ToUInt32(buffer, 0x4C);

                if(tmpHeader.DataSize + tmpHeader.TagSize + 0x54 != imageFilter.GetDataForkLength() &&
                   tmpHeader.Format                              != kSigmaFormatTwiggy) return false;
            }

            if(tmpHeader.Format != kSonyFormat400K  && tmpHeader.Format != kSonyFormat800K    &&
               tmpHeader.Format != kSonyFormat720K  && tmpHeader.Format != kSonyFormat1440K   &&
               tmpHeader.Format != kSonyFormat1680K && tmpHeader.Format != kSigmaFormatTwiggy &&
               tmpHeader.Format != kNotStandardFormat)
            {
                DicConsole.DebugWriteLine("DC42 plugin", "Unknown tmp_header.format = 0x{0:X2} value",
                                          tmpHeader.Format);

                return false;
            }

            if(tmpHeader.FmtByte != kSonyFmtByte400K          && tmpHeader.FmtByte != kSonyFmtByte800K    &&
               tmpHeader.FmtByte != kSonyFmtByte800KIncorrect && tmpHeader.FmtByte != kSonyFmtByteProDos  &&
               tmpHeader.FmtByte != kInvalidFmtByte           && tmpHeader.FmtByte != kSigmaFmtByteTwiggy &&
               tmpHeader.FmtByte != kFmtNotStandard           && tmpHeader.FmtByte != kMacOSXFmtByte)
            {
                DicConsole.DebugWriteLine("DC42 plugin", "Unknown tmp_header.fmtByte = 0x{0:X2} value",
                                          tmpHeader.FmtByte);

                return false;
            }

            if(tmpHeader.FmtByte != kInvalidFmtByte) return true;

            DicConsole.DebugWriteLine("DC42 plugin", "Image says it's unformatted");

            return false;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer  = new byte[0x58];
            byte[] pString = new byte[64];
            stream.Read(buffer, 0, 0x58);
            IsWriting = false;

            // Incorrect pascal string length, not DC42
            if(buffer[0] > 63) return false;

            header                               = new Dc42Header();
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            Array.Copy(buffer, 0, pString, 0, 64);
            header.DiskName     = StringHandlers.PascalToString(pString, Encoding.GetEncoding("macintosh"));
            header.DataSize     = BigEndianBitConverter.ToUInt32(buffer, 0x40);
            header.TagSize      = BigEndianBitConverter.ToUInt32(buffer, 0x44);
            header.DataChecksum = BigEndianBitConverter.ToUInt32(buffer, 0x48);
            header.TagChecksum  = BigEndianBitConverter.ToUInt32(buffer, 0x4C);
            header.Format       = buffer[0x50];
            header.FmtByte      = buffer[0x51];
            header.Valid        = buffer[0x52];
            header.Reserved     = buffer[0x53];

            DicConsole.DebugWriteLine("DC42 plugin", "header.diskName = \"{0}\"",      header.DiskName);
            DicConsole.DebugWriteLine("DC42 plugin", "header.dataSize = {0} bytes",    header.DataSize);
            DicConsole.DebugWriteLine("DC42 plugin", "header.tagSize = {0} bytes",     header.TagSize);
            DicConsole.DebugWriteLine("DC42 plugin", "header.dataChecksum = 0x{0:X8}", header.DataChecksum);
            DicConsole.DebugWriteLine("DC42 plugin", "header.tagChecksum = 0x{0:X8}",  header.TagChecksum);
            DicConsole.DebugWriteLine("DC42 plugin", "header.format = 0x{0:X2}",       header.Format);
            DicConsole.DebugWriteLine("DC42 plugin", "header.fmtByte = 0x{0:X2}",      header.FmtByte);
            DicConsole.DebugWriteLine("DC42 plugin", "header.valid = {0}",             header.Valid);
            DicConsole.DebugWriteLine("DC42 plugin", "header.reserved = {0}",          header.Reserved);

            if(header.Valid != 1 || header.Reserved != 0) return false;

            // Some versions seem to incorrectly create little endian fields
            if(header.DataSize + header.TagSize + 0x54 != imageFilter.GetDataForkLength() &&
               header.Format                           != kSigmaFormatTwiggy)
            {
                header.DataSize     = BitConverter.ToUInt32(buffer, 0x40);
                header.TagSize      = BitConverter.ToUInt32(buffer, 0x44);
                header.DataChecksum = BitConverter.ToUInt32(buffer, 0x48);
                header.TagChecksum  = BitConverter.ToUInt32(buffer, 0x4C);

                if(header.DataSize + header.TagSize + 0x54 != imageFilter.GetDataForkLength() &&
                   header.Format                           != kSigmaFormatTwiggy) return false;
            }

            if(header.Format != kSonyFormat400K  && header.Format != kSonyFormat800K    &&
               header.Format != kSonyFormat720K  && header.Format != kSonyFormat1440K   &&
               header.Format != kSonyFormat1680K && header.Format != kSigmaFormatTwiggy &&
               header.Format != kNotStandardFormat)
            {
                DicConsole.DebugWriteLine("DC42 plugin", "Unknown header.format = 0x{0:X2} value", header.Format);

                return false;
            }

            if(header.FmtByte != kSonyFmtByte400K          && header.FmtByte != kSonyFmtByte800K    &&
               header.FmtByte != kSonyFmtByte800KIncorrect && header.FmtByte != kSonyFmtByteProDos  &&
               header.FmtByte != kInvalidFmtByte           && header.FmtByte != kSigmaFmtByteTwiggy &&
               header.FmtByte != kFmtNotStandard           && header.FmtByte != kMacOSXFmtByte)
            {
                DicConsole.DebugWriteLine("DC42 plugin", "Unknown tmp_header.fmtByte = 0x{0:X2} value", header.FmtByte);

                return false;
            }

            if(header.FmtByte == kInvalidFmtByte)
            {
                DicConsole.DebugWriteLine("DC42 plugin", "Image says it's unformatted");

                return false;
            }

            dataOffset           = 0x54;
            tagOffset            = header.TagSize != 0 ? 0x54 + header.DataSize : 0;
            imageInfo.SectorSize = 512;
            bptag                = (uint)(header.TagSize != 0 ? 12 : 0);
            dc42ImageFilter      = imageFilter;

            imageInfo.Sectors = header.DataSize / 512;

            if(header.TagSize != 0)
            {
                bptag = (uint)(header.TagSize / imageInfo.Sectors);
                DicConsole.DebugWriteLine("DC42 plugin", "bptag = {0} bytes", bptag);

                if(bptag != 12 && bptag != 20 && bptag != 24)
                {
                    DicConsole.DebugWriteLine("DC42 plugin", "Unknown tag size");
                    return false;
                }

                imageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);
            }

            imageInfo.ImageSize            = imageInfo.Sectors * imageInfo.SectorSize + imageInfo.Sectors * bptag;
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = header.DiskName;

            switch(header.Format)
            {
                case kSonyFormat400K:
                    imageInfo.MediaType = imageInfo.Sectors == 1600 ? MediaType.AppleSonyDS : MediaType.AppleSonySS;
                    break;
                case kSonyFormat800K:
                    imageInfo.MediaType = MediaType.AppleSonyDS;
                    break;
                case kSonyFormat720K:
                    imageInfo.MediaType = MediaType.DOS_35_DS_DD_9;
                    break;
                case kSonyFormat1440K:
                    imageInfo.MediaType = MediaType.DOS_35_HD;
                    break;
                case kSonyFormat1680K:
                    imageInfo.MediaType = MediaType.DMF;
                    break;
                case kSigmaFormatTwiggy:
                    imageInfo.MediaType = MediaType.AppleFileWare;
                    break;
                case kNotStandardFormat:
                    switch(imageInfo.Sectors)
                    {
                        case 9728:
                            imageInfo.MediaType = MediaType.AppleProfile;
                            break;
                        case 19456:
                            imageInfo.MediaType = MediaType.AppleProfile;
                            break;
                        case 38912:
                            imageInfo.MediaType = MediaType.AppleWidget;
                            break;
                        case 39040:
                            imageInfo.MediaType = MediaType.AppleHD20;
                            break;
                        default:
                            imageInfo.MediaType = MediaType.Unknown;
                            break;
                    }

                    break;
                default:
                    imageInfo.MediaType = MediaType.Unknown;
                    break;
            }

            if(imageInfo.MediaType == MediaType.AppleFileWare)
            {
                byte[] data = new byte[header.DataSize];
                byte[] tags = new byte[header.TagSize];

                twiggyCache     = new byte[header.DataSize];
                twiggyCacheTags = new byte[header.TagSize];
                twiggy          = true;

                Stream datastream = imageFilter.GetDataForkStream();
                datastream.Seek(dataOffset, SeekOrigin.Begin);
                datastream.Read(data, 0, (int)header.DataSize);

                Stream tagstream = imageFilter.GetDataForkStream();
                tagstream.Seek(tagOffset, SeekOrigin.Begin);
                tagstream.Read(tags, 0, (int)header.TagSize);

                ushort mfsMagic     = BigEndianBitConverter.ToUInt16(data, data.Length / 2 + 0x400);
                ushort mfsAllBlocks = BigEndianBitConverter.ToUInt16(data, data.Length / 2 + 0x412);

                // Detect a Macintosh Twiggy
                if(mfsMagic == 0xD2D7 && mfsAllBlocks == 422)
                {
                    DicConsole.DebugWriteLine("DC42 plugin", "Macintosh Twiggy detected, reversing disk sides");
                    Array.Copy(data, header.DataSize / 2, twiggyCache,     0, header.DataSize / 2);
                    Array.Copy(tags, header.TagSize  / 2, twiggyCacheTags, 0, header.TagSize  / 2);
                    Array.Copy(data, 0,                   twiggyCache,     header.DataSize    / 2, header.DataSize / 2);
                    Array.Copy(tags, 0,                   twiggyCacheTags, header.TagSize     / 2, header.TagSize  / 2);
                }
                else
                {
                    DicConsole.DebugWriteLine("DC42 plugin", "Lisa Twiggy detected, reversing second half of disk");
                    Array.Copy(data, 0, twiggyCache,     0, header.DataSize / 2);
                    Array.Copy(tags, 0, twiggyCacheTags, 0, header.TagSize  / 2);

                    int copiedSectors = 0;
                    int sectorsToCopy = 0;

                    for(int i = 0; i < 46; i++)
                    {
                        if(i >= 0  && i <= 3) sectorsToCopy  = 22;
                        if(i >= 4  && i <= 10) sectorsToCopy = 21;
                        if(i >= 11 && i <= 16) sectorsToCopy = 20;
                        if(i >= 17 && i <= 22) sectorsToCopy = 19;
                        if(i >= 23 && i <= 28) sectorsToCopy = 18;
                        if(i >= 29 && i <= 34) sectorsToCopy = 17;
                        if(i >= 35 && i <= 41) sectorsToCopy = 16;
                        if(i >= 42 && i <= 45) sectorsToCopy = 15;

                        Array.Copy(data, header.DataSize / 2 + copiedSectors * 512, twiggyCache,
                                   twiggyCache.Length        - copiedSectors * 512 - sectorsToCopy * 512,
                                   sectorsToCopy                             * 512);
                        Array.Copy(tags, header.TagSize                      / 2 + copiedSectors * bptag,
                                   twiggyCacheTags,
                                   twiggyCacheTags.Length - copiedSectors * bptag -
                                   sectorsToCopy                          * bptag,
                                   sectorsToCopy                          * bptag);

                        copiedSectors += sectorsToCopy;
                    }
                }
            }

            try
            {
                if(imageFilter.HasResourceFork())
                {
                    ResourceFork rsrcFork = new ResourceFork(imageFilter.GetResourceForkStream());
                    if(rsrcFork.ContainsKey(0x76657273))
                    {
                        Resource versRsrc = rsrcFork.GetResource(0x76657273);

                        byte[] vers = versRsrc?.GetResource(versRsrc.GetIds()[0]);

                        if(vers != null)
                        {
                            Version version = new Version(vers);

                            string release = null;
                            string dev     = null;
                            string pre     = null;

                            string major = $"{version.MajorVersion}";
                            string minor = $".{version.MinorVersion / 10}";
                            if(version.MinorVersion                 % 10 > 0) release = $".{version.MinorVersion % 10}";
                            switch(version.DevStage)
                            {
                                case Version.DevelopmentStage.Alpha:
                                    dev = "a";
                                    break;
                                case Version.DevelopmentStage.Beta:
                                    dev = "b";
                                    break;
                                case Version.DevelopmentStage.PreAlpha:
                                    dev = "d";
                                    break;
                            }

                            if(dev == null && version.PreReleaseVersion > 0) dev = "f";

                            if(dev != null) pre = $"{version.PreReleaseVersion}";

                            imageInfo.ApplicationVersion = $"{major}{minor}{release}{dev}{pre}";
                            imageInfo.Application        = version.VersionString;
                            imageInfo.Comments           = version.VersionMessage;
                        }
                    }

                    if(rsrcFork.ContainsKey(0x64437079))
                    {
                        Resource dCpyRsrc = rsrcFork.GetResource(0x64437079);
                        if(dCpyRsrc != null)
                        {
                            string dCpy = StringHandlers.PascalToString(dCpyRsrc.GetResource(dCpyRsrc.GetIds()[0]),
                                                                        Encoding.GetEncoding("macintosh"));
                            Regex dCpyEx    = new Regex(REGEX_DCPY);
                            Match dCpyMatch = dCpyEx.Match(dCpy);

                            if(dCpyMatch.Success)
                            {
                                imageInfo.Application        = dCpyMatch.Groups["application"].Value;
                                imageInfo.ApplicationVersion = dCpyMatch.Groups["version"].Value;
                            }
                        }
                    }
                }
            }
            catch(InvalidCastException) { }

            DicConsole.DebugWriteLine("DC42 plugin", "Image application = {0} version {1}", imageInfo.Application,
                                      imageInfo.ApplicationVersion);

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            DicConsole.VerboseWriteLine("DiskCopy 4.2 image contains a disk of type {0}", imageInfo.MediaType);

            switch(imageInfo.MediaType)
            {
                case MediaType.AppleSonySS:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 1;
                    imageInfo.SectorsPerTrack = 10;
                    break;
                case MediaType.AppleSonyDS:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 10;
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
                case MediaType.DMF:
                    imageInfo.Cylinders       = 80;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 21;
                    break;
                case MediaType.AppleProfile:
                    switch(imageInfo.Sectors)
                    {
                        case 9728:
                            imageInfo.Cylinders = 152;
                            break;
                        case 19456:
                            imageInfo.Cylinders = 304;
                            break;
                    }

                    imageInfo.Heads           = 4;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.AppleWidget:
                    imageInfo.Cylinders       = 608;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                case MediaType.AppleHD20:
                    imageInfo.Cylinders       = 610;
                    imageInfo.Heads           = 2;
                    imageInfo.SectorsPerTrack = 16;
                    break;
                default:
                    imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
                    imageInfo.Heads           = 16;
                    imageInfo.SectorsPerTrack = 63;
                    break;
            }

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

        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                   out                                   List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out                                               List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        public bool? VerifyMediaImage()
        {
            byte[] data    = new byte[header.DataSize];
            byte[] tags    = new byte[header.TagSize];
            uint   tagsChk = 0;

            DicConsole.DebugWriteLine("DC42 plugin", "Reading data");
            Stream datastream = dc42ImageFilter.GetDataForkStream();
            datastream.Seek(dataOffset, SeekOrigin.Begin);
            datastream.Read(data, 0, (int)header.DataSize);

            DicConsole.DebugWriteLine("DC42 plugin", "Calculating data checksum");
            uint dataChk = DC42CheckSum(data);
            DicConsole.DebugWriteLine("DC42 plugin", "Calculated data checksum = 0x{0:X8}", dataChk);
            DicConsole.DebugWriteLine("DC42 plugin", "Stored data checksum = 0x{0:X8}",     header.DataChecksum);

            if(header.TagSize <= 0) return dataChk == header.DataChecksum && tagsChk == header.TagChecksum;

            DicConsole.DebugWriteLine("DC42 plugin", "Reading tags");
            Stream tagstream = dc42ImageFilter.GetDataForkStream();
            tagstream.Seek(tagOffset, SeekOrigin.Begin);
            tagstream.Read(tags, 0, (int)header.TagSize);

            DicConsole.DebugWriteLine("DC42 plugin", "Calculating tag checksum");
            tagsChk = DC42CheckSum(tags);
            DicConsole.DebugWriteLine("DC42 plugin", "Calculated tag checksum = 0x{0:X8}", tagsChk);
            DicConsole.DebugWriteLine("DC42 plugin", "Stored tag checksum = 0x{0:X8}",     header.TagChecksum);

            return dataChk == header.DataChecksum && tagsChk == header.TagChecksum;
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

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            if(twiggy)
                Array.Copy(twiggyCache, (int)sectorAddress * imageInfo.SectorSize, buffer, 0,
                           length                          * imageInfo.SectorSize);
            else
            {
                Stream stream = dc42ImageFilter.GetDataForkStream();
                stream.Seek((long)(dataOffset + sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
                stream.Read(buffer, 0, (int)(length           * imageInfo.SectorSize));
            }

            return buffer;
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(tag != SectorTagType.AppleSectorTag)
                throw new FeatureUnsupportedImageException($"Tag {tag} not supported by image format");

            if(header.TagSize == 0) throw new FeatureNotPresentImageException("Disk image does not have tags");

            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * bptag];

            if(twiggy) Array.Copy(twiggyCacheTags, (int)sectorAddress * bptag, buffer, 0, length * bptag);
            else
            {
                Stream stream = dc42ImageFilter.GetDataForkStream();
                stream.Seek((long)(tagOffset + sectorAddress * bptag), SeekOrigin.Begin);
                stream.Read(buffer, 0, (int)(length          * bptag));
            }

            return buffer;
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

            byte[] data   = ReadSectors(sectorAddress, length);
            byte[] tags   = ReadSectorsTag(sectorAddress, length, SectorTagType.AppleSectorTag);
            byte[] buffer = new byte[data.Length + tags.Length];

            for(uint i = 0; i < length; i++)
            {
                Array.Copy(data, i * imageInfo.SectorSize, buffer, i * (imageInfo.SectorSize + bptag),
                           imageInfo.SectorSize);
                Array.Copy(tags, i * bptag, buffer, i * (imageInfo.SectorSize + bptag) + imageInfo.SectorSize, bptag);
            }

            return buffer;
        }

        public string Format => "Apple DiskCopy 4.2";

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

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
        public IEnumerable<SectorTagType> SupportedSectorTags => new[] {SectorTagType.AppleSectorTag};
        public IEnumerable<MediaType>     SupportedMediaTypes =>
            new[]
            {
                MediaType.AppleFileWare, MediaType.AppleHD20, MediaType.AppleProfile, MediaType.AppleSonyDS,
                MediaType.AppleSonySS, MediaType.AppleWidget, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,
                MediaType.DMF
            };
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new[] {("macosx", typeof(bool), "Use Mac OS X format byte")};
        public IEnumerable<string> KnownExtensions => new[] {".dc42", ".diskcopy42", ".image"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors)
        {
            header    = new Dc42Header();
            bool tags = false;
            bool macosx = false;

            if(options != null && options.TryGetValue("macosx", out string tmpOption))
                bool.TryParse(tmpOption, out macosx);

            switch(mediaType)
            {
                case MediaType.AppleFileWare:
                    header.FmtByte  = kSigmaFmtByteTwiggy;
                    header.Format   = kSigmaFormatTwiggy;
                    twiggy          = true;
                    tags            = true;
                    twiggyDataCache = new MemoryStream();
                    twiggyTagCache  = new MemoryStream();

                    // TODO
                    ErrorMessage = "Twiggy write support not yet implemented";
                    return false;
                case MediaType.AppleHD20:
                    if(sectors != 39040)
                    {
                        ErrorMessage = "Incorrect number of sectors for Apple HD20 image";
                        return false;
                    }

                    header.FmtByte = kFmtNotStandard;
                    header.Format  = kNotStandardFormat;
                    tags           = true;
                    break;
                case MediaType.AppleProfile:
                    if(sectors != 9728 && sectors != 19456)
                    {
                        ErrorMessage = "Incorrect number of sectors for Apple Profile image";
                        return false;
                    }

                    header.FmtByte = kFmtNotStandard;
                    header.Format  = kNotStandardFormat;
                    tags           = true;
                    break;
                case MediaType.AppleSonyDS:
                    if(sectors != 1600)
                    {
                        ErrorMessage = "Incorrect number of sectors for Apple MF2DD image";
                        return false;
                    }

                    header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte800K;
                    header.Format  = kSonyFormat800K;
                    tags           = true;
                    break;
                case MediaType.AppleSonySS:
                    if(sectors != 800)
                    {
                        ErrorMessage = "Incorrect number of sectors for Apple MF1DD image";
                        return false;
                    }

                    header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte400K;
                    header.Format  = kSonyFormat400K;
                    tags           = true;
                    break;
                case MediaType.AppleWidget:
                    if(sectors != 39040)
                    {
                        ErrorMessage = "Incorrect number of sectors for Apple Widget image";
                        return false;
                    }

                    header.FmtByte = kFmtNotStandard;
                    header.Format  = kNotStandardFormat;
                    tags           = true;
                    break;
                case MediaType.DOS_35_DS_DD_9:
                    if(sectors != 1440)
                    {
                        ErrorMessage = "Incorrect number of sectors for MF2DD image";
                        return false;
                    }

                    header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte720K;
                    header.Format  = kSonyFormat720K;
                    break;
                case MediaType.DOS_35_HD:
                    if(sectors != 2880)
                    {
                        ErrorMessage = "Incorrect number of sectors for MF2HD image";
                        return false;
                    }

                    header.Format  = kSonyFmtByte1440K;
                    header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte1440K;
                    break;
                case MediaType.DMF:
                    if(sectors != 3360)
                    {
                        ErrorMessage = "Incorrect number of sectors for DMF image";
                        return false;
                    }

                    header.FmtByte = macosx ? kMacOSXFmtByte : kSonyFmtByte1680K;
                    header.Format  = kSonyFormat1680K;
                    break;
                default:
                    ErrorMessage = $"Unsupport media format {mediaType}";
                    return false;
            }

            dataOffset              = 0x54;
            tagOffset               = header.TagSize != 0 ? 0x54 + header.DataSize : 0;
            header.DiskName         = "-DiscImageChef converted image-";
            header.Valid            = 1;
            header.DataSize         = (uint)(sectors * 512);
            if(tags) header.TagSize = (uint)(sectors * 12);
            sectorsToWrite          = sectors;

            try { writingStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None); }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            ErrorMessage = null;
            return true;
        }

        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            ErrorMessage = "Unsupported feature";
            return false;
        }

        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(sectorAddress >= sectorsToWrite)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            writingStream.Seek((long)(dataOffset + sectorAddress * 512), SeekOrigin.Begin);
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

            if(data.Length != 512)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(data.Length % 512 != 0)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress + length >= sectorsToWrite)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            writingStream.Seek((long)(dataOffset + sectorAddress * 512), SeekOrigin.Begin);
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

            if(header.TagSize == 0)
            {
                ErrorMessage = "Image does not support tags";
                return false;
            }

            if(data.Length != 524)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress >= sectorsToWrite)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            writingStream.Seek((long)(dataOffset + sectorAddress * 512), SeekOrigin.Begin);
            writingStream.Write(data, 0, 512);
            writingStream.Seek((long)(tagOffset + sectorAddress * 12), SeekOrigin.Begin);
            writingStream.Write(data, 512, 12);

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

            if(header.TagSize == 0)
            {
                ErrorMessage = "Image does not support tags";
                return false;
            }

            if(data.Length % 524 != 0)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress + length >= sectorsToWrite)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            for(uint i = 0; i < length; i++)
            {
                writingStream.Seek((long)(dataOffset + (sectorAddress + i) * 512), SeekOrigin.Begin);
                writingStream.Write(data, (int)(i                          * 524 + 0), 512);
                writingStream.Seek((long)(tagOffset                              + (sectorAddress + i) * 12),
                                   SeekOrigin.Begin);
                writingStream.Write(data, (int)(i * 524 + 512),
                                    12);
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

            writingStream.Seek(0x54, SeekOrigin.Begin);
            byte[] data = new byte[header.DataSize];
            writingStream.Read(data, 0, (int)header.DataSize);
            header.DataChecksum = DC42CheckSum(data);
            writingStream.Seek(0x54 + header.DataSize, SeekOrigin.Begin);
            data = new byte[header.TagSize];
            writingStream.Read(data, 0, (int)header.TagSize);
            header.TagChecksum = DC42CheckSum(data);

            writingStream.Seek(0, SeekOrigin.Begin);
            if(header.DiskName.Length > 63) header.DiskName = header.DiskName.Substring(0, 63);
            writingStream.WriteByte((byte)header.DiskName.Length);
            Encoding macRoman = new MacRoman();
            writingStream.Write(macRoman.GetBytes(header.DiskName), 0, header.DiskName.Length);

            writingStream.Seek(64, SeekOrigin.Begin);
            writingStream.Write(BigEndianBitConverter.GetBytes(header.DataSize),     0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(header.TagSize),      0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(header.DataChecksum), 0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(header.TagChecksum),  0, 4);
            writingStream.WriteByte(header.Format);
            writingStream.WriteByte(header.FmtByte);
            writingStream.WriteByte(1);
            writingStream.WriteByte(0);

            writingStream.Flush();
            writingStream.Close();
            IsWriting = false;

            return true;
        }

        static uint DC42CheckSum(byte[] buffer)
        {
            uint dc42Chk = 0;
            if((buffer.Length & 0x01) == 0x01) return 0xFFFFFFFF;

            for(uint i = 0; i < buffer.Length; i += 2)
            {
                dc42Chk += (uint)(buffer[i] << 8);
                dc42Chk += buffer[i + 1];
                dc42Chk =  (dc42Chk >> 1) | (dc42Chk << 31);
            }

            return dc42Chk;
        }

        ~DiskCopy42()
        {
            Close();
        }

        // DiskCopy 4.2 header, big-endian, data-fork, start of file, 84 bytes
        struct Dc42Header
        {
            /// <summary>0x00, 64 bytes, pascal string, disk name or "-not a Macintosh disk-", filled with garbage</summary>
            public string DiskName;
            /// <summary>0x40, size of data in bytes (usually sectors*512)</summary>
            public uint DataSize;
            /// <summary>0x44, size of tags in bytes (usually sectors*12)</summary>
            public uint TagSize;
            /// <summary>0x48, checksum of data bytes</summary>
            public uint DataChecksum;
            /// <summary>0x4C, checksum of tag bytes</summary>
            public uint TagChecksum;
            /// <summary>0x50, format of disk, see constants</summary>
            public byte Format;
            /// <summary>0x51, format of sectors, see constants</summary>
            public byte FmtByte;
            /// <summary>0x52, is disk image valid? always 0x01</summary>
            public byte Valid;
            /// <summary>0x53, reserved, always 0x00</summary>
            public byte Reserved;
        }
    }
}