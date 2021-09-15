// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads Apple DiskCopy 4.2 disk images.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Claunia.Encoding;
using Claunia.RsrcFork;
using Version = Resources.Version;

namespace Aaru.DiscImages
{
    public sealed partial class DiskCopy42
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer  = new byte[0x58];
            byte[] pString = new byte[64];
            stream.Read(buffer, 0, 0x58);
            IsWriting = false;

            // Incorrect pascal string length, not DC42
            if(buffer[0] > 63)
                return false;

            header = new Header();

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

            AaruConsole.DebugWriteLine("DC42 plugin", "header.diskName = \"{0}\"", header.DiskName);
            AaruConsole.DebugWriteLine("DC42 plugin", "header.dataSize = {0} bytes", header.DataSize);
            AaruConsole.DebugWriteLine("DC42 plugin", "header.tagSize = {0} bytes", header.TagSize);
            AaruConsole.DebugWriteLine("DC42 plugin", "header.dataChecksum = 0x{0:X8}", header.DataChecksum);
            AaruConsole.DebugWriteLine("DC42 plugin", "header.tagChecksum = 0x{0:X8}", header.TagChecksum);
            AaruConsole.DebugWriteLine("DC42 plugin", "header.format = 0x{0:X2}", header.Format);
            AaruConsole.DebugWriteLine("DC42 plugin", "header.fmtByte = 0x{0:X2}", header.FmtByte);
            AaruConsole.DebugWriteLine("DC42 plugin", "header.valid = {0}", header.Valid);
            AaruConsole.DebugWriteLine("DC42 plugin", "header.reserved = {0}", header.Reserved);

            if(header.Valid    != 1 ||
               header.Reserved != 0)
                return false;

            // Some versions seem to incorrectly create little endian fields
            if(header.DataSize + header.TagSize + 0x54 != imageFilter.DataForkLength &&
               header.Format                           != kSigmaFormatTwiggy)
            {
                header.DataSize     = BitConverter.ToUInt32(buffer, 0x40);
                header.TagSize      = BitConverter.ToUInt32(buffer, 0x44);
                header.DataChecksum = BitConverter.ToUInt32(buffer, 0x48);
                header.TagChecksum  = BitConverter.ToUInt32(buffer, 0x4C);

                if(header.DataSize + header.TagSize + 0x54 != imageFilter.DataForkLength &&
                   header.Format                           != kSigmaFormatTwiggy)
                    return false;
            }

            if(header.Format != kSonyFormat400K    &&
               header.Format != kSonyFormat800K    &&
               header.Format != kSonyFormat720K    &&
               header.Format != kSonyFormat1440K   &&
               header.Format != kSonyFormat1680K   &&
               header.Format != kSigmaFormatTwiggy &&
               header.Format != kNotStandardFormat)
            {
                AaruConsole.DebugWriteLine("DC42 plugin", "Unknown header.format = 0x{0:X2} value", header.Format);

                return false;
            }

            if(header.FmtByte != kSonyFmtByte400K          &&
               header.FmtByte != kSonyFmtByte800K          &&
               header.FmtByte != kSonyFmtByte800KIncorrect &&
               header.FmtByte != kSonyFmtByteProDos        &&
               header.FmtByte != kInvalidFmtByte           &&
               header.FmtByte != kSigmaFmtByteTwiggy       &&
               header.FmtByte != kFmtNotStandard           &&
               header.FmtByte != kMacOSXFmtByte)
            {
                AaruConsole.DebugWriteLine("DC42 plugin", "Unknown tmp_header.fmtByte = 0x{0:X2} value",
                                           header.FmtByte);

                return false;
            }

            if(header.FmtByte == kInvalidFmtByte)
            {
                AaruConsole.DebugWriteLine("DC42 plugin", "Image says it's unformatted");

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
                AaruConsole.DebugWriteLine("DC42 plugin", "bptag = {0} bytes", bptag);

                if(bptag != 12 &&
                   bptag != 20 &&
                   bptag != 24)
                {
                    AaruConsole.DebugWriteLine("DC42 plugin", "Unknown tag size");

                    return false;
                }

                imageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);
            }

            imageInfo.ImageSize            = (imageInfo.Sectors * imageInfo.SectorSize) + (imageInfo.Sectors * bptag);
            imageInfo.CreationTime         = imageFilter.CreationTime;
            imageInfo.LastModificationTime = imageFilter.LastWriteTime;
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

                Stream dataStream = imageFilter.GetDataForkStream();
                dataStream.Seek(dataOffset, SeekOrigin.Begin);
                dataStream.Read(data, 0, (int)header.DataSize);

                Stream tagStream = imageFilter.GetDataForkStream();
                tagStream.Seek(tagOffset, SeekOrigin.Begin);
                tagStream.Read(tags, 0, (int)header.TagSize);

                ushort mfsMagic     = BigEndianBitConverter.ToUInt16(data, (data.Length / 2) + 0x400);
                ushort mfsAllBlocks = BigEndianBitConverter.ToUInt16(data, (data.Length / 2) + 0x412);

                // Detect a Macintosh Twiggy
                if(mfsMagic     == 0xD2D7 &&
                   mfsAllBlocks == 422)
                {
                    AaruConsole.DebugWriteLine("DC42 plugin", "Macintosh Twiggy detected, reversing disk sides");
                    Array.Copy(data, header.DataSize                    / 2, twiggyCache, 0, header.DataSize    / 2);
                    Array.Copy(tags, header.TagSize                     / 2, twiggyCacheTags, 0, header.TagSize / 2);
                    Array.Copy(data, 0, twiggyCache, header.DataSize    / 2, header.DataSize                    / 2);
                    Array.Copy(tags, 0, twiggyCacheTags, header.TagSize / 2, header.TagSize                     / 2);
                }
                else
                {
                    AaruConsole.DebugWriteLine("DC42 plugin", "Lisa Twiggy detected, reversing second half of disk");
                    Array.Copy(data, 0, twiggyCache, 0, header.DataSize    / 2);
                    Array.Copy(tags, 0, twiggyCacheTags, 0, header.TagSize / 2);

                    int copiedSectors = 0;
                    int sectorsToCopy = 0;

                    for(int i = 0; i < 46; i++)
                    {
                        if(i >= 0 &&
                           i <= 3)
                            sectorsToCopy = 22;

                        if(i >= 4 &&
                           i <= 10)
                            sectorsToCopy = 21;

                        if(i >= 11 &&
                           i <= 16)
                            sectorsToCopy = 20;

                        if(i >= 17 &&
                           i <= 22)
                            sectorsToCopy = 19;

                        if(i >= 23 &&
                           i <= 28)
                            sectorsToCopy = 18;

                        if(i >= 29 &&
                           i <= 34)
                            sectorsToCopy = 17;

                        if(i >= 35 &&
                           i <= 41)
                            sectorsToCopy = 16;

                        if(i >= 42 &&
                           i <= 45)
                            sectorsToCopy = 15;

                        Array.Copy(data, (header.DataSize / 2) + (copiedSectors * 512), twiggyCache,
                                   twiggyCache.Length          - (copiedSectors * 512) - (sectorsToCopy * 512),
                                   sectorsToCopy * 512);

                        Array.Copy(tags, (header.TagSize / 2) + (copiedSectors * bptag), twiggyCacheTags,
                                   twiggyCacheTags.Length     - (copiedSectors * bptag) - (sectorsToCopy * bptag),
                                   sectorsToCopy * bptag);

                        copiedSectors += sectorsToCopy;
                    }
                }
            }

            try
            {
                if(imageFilter.HasResourceFork)
                {
                    var rsrcFork = new ResourceFork(imageFilter.GetResourceForkStream());

                    if(rsrcFork.ContainsKey(0x76657273))
                    {
                        Resource versRsrc = rsrcFork.GetResource(0x76657273);

                        byte[] vers = versRsrc?.GetResource(versRsrc.GetIds()[0]);

                        if(vers != null)
                        {
                            var version = new Version(vers);

                            string release = null;
                            string dev     = null;
                            string pre     = null;

                            string major = $"{version.MajorVersion}";
                            string minor = $".{version.MinorVersion / 10}";

                            if(version.MinorVersion % 10 > 0)
                                release = $".{version.MinorVersion % 10}";

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

                            if(dev                       == null &&
                               version.PreReleaseVersion > 0)
                                dev = "f";

                            if(dev != null)
                                pre = $"{version.PreReleaseVersion}";

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

                            var   dCpyEx    = new Regex(REGEX_DCPY);
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
            catch(InvalidCastException) {}

            AaruConsole.DebugWriteLine("DC42 plugin", "Image application = {0} version {1}", imageInfo.Application,
                                       imageInfo.ApplicationVersion);

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            AaruConsole.VerboseWriteLine("DiskCopy 4.2 image contains a disk of type {0}", imageInfo.MediaType);

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

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) => ReadSectorsTag(sectorAddress, 1, tag);

        /// <inheritdoc />
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
                stream.Seek((long)(dataOffset + (sectorAddress * imageInfo.SectorSize)), SeekOrigin.Begin);
                stream.Read(buffer, 0, (int)(length * imageInfo.SectorSize));
            }

            return buffer;
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(tag != SectorTagType.AppleSectorTag)
                throw new FeatureUnsupportedImageException($"Tag {tag} not supported by image format");

            if(header.TagSize == 0)
                throw new FeatureNotPresentImageException("Disk image does not have tags");

            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * bptag];

            if(twiggy)
                Array.Copy(twiggyCacheTags, (int)sectorAddress * bptag, buffer, 0, length * bptag);
            else
            {
                Stream stream = dc42ImageFilter.GetDataForkStream();
                stream.Seek((long)(tagOffset + (sectorAddress * bptag)), SeekOrigin.Begin);
                stream.Read(buffer, 0, (int)(length * bptag));
            }

            return buffer;
        }

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectorsLong(sectorAddress, 1);

        /// <inheritdoc />
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

                Array.Copy(tags, i * bptag, buffer, (i * (imageInfo.SectorSize + bptag)) + imageInfo.SectorSize, bptag);
            }

            return buffer;
        }
    }
}