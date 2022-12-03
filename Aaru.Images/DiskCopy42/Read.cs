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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Claunia.Encoding;
using Claunia.RsrcFork;
using Version = Resources.Version;

namespace Aaru.DiscImages;

public sealed partial class DiskCopy42
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);
        byte[] buffer  = new byte[0x58];
        byte[] pString = new byte[64];
        stream.EnsureRead(buffer, 0, 0x58);
        IsWriting = false;

        // Incorrect pascal string length, not DC42
        if(buffer[0] > 63)
            return ErrorNumber.InvalidArgument;

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
            return ErrorNumber.InvalidArgument;

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
                return ErrorNumber.InvalidArgument;
        }

        if(header.Format != kSonyFormat400K    &&
           header.Format != kSonyFormat800K    &&
           header.Format != kSonyFormat720K    &&
           header.Format != kSonyFormat1440K   &&
           header.Format != kSonyFormat1680K   &&
           header.Format != kSigmaFormatTwiggy &&
           header.Format != kNotStandardFormat)
        {
            AaruConsole.DebugWriteLine("DC42 plugin", Localization.Unknown_header_format_equals_0_value, header.Format);

            return ErrorNumber.NotSupported;
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
            AaruConsole.DebugWriteLine("DC42 plugin", Localization.Unknown_tmp_header_fmtByte_equals_0_value,
                                       header.FmtByte);

            return ErrorNumber.NotSupported;
        }

        if(header.FmtByte == kInvalidFmtByte)
        {
            AaruConsole.DebugWriteLine("DC42 plugin", Localization.Image_says_its_unformatted);

            return ErrorNumber.InvalidArgument;
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
                AaruConsole.DebugWriteLine("DC42 plugin", Localization.Unknown_tag_size);

                return ErrorNumber.NotSupported;
            }

            imageInfo.ReadableSectorTags.Add(SectorTagType.AppleSectorTag);
        }

        imageInfo.ImageSize            = (imageInfo.Sectors * imageInfo.SectorSize) + (imageInfo.Sectors * bptag);
        imageInfo.CreationTime         = imageFilter.CreationTime;
        imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        imageInfo.MediaTitle           = header.DiskName;

        imageInfo.MediaType = header.Format switch
        {
            kSonyFormat400K    => imageInfo.Sectors == 1600 ? MediaType.AppleSonyDS : MediaType.AppleSonySS,
            kSonyFormat800K    => MediaType.AppleSonyDS,
            kSonyFormat720K    => MediaType.DOS_35_DS_DD_9,
            kSonyFormat1440K   => MediaType.DOS_35_HD,
            kSonyFormat1680K   => MediaType.DMF,
            kSigmaFormatTwiggy => MediaType.AppleFileWare,
            kNotStandardFormat => imageInfo.Sectors switch
            {
                9728  => MediaType.AppleProfile,
                19456 => MediaType.AppleProfile,
                38912 => MediaType.AppleWidget,
                39040 => MediaType.AppleHD20,
                _     => MediaType.Unknown
            },
            _ => MediaType.Unknown
        };

        if(imageInfo.MediaType == MediaType.AppleFileWare)
        {
            byte[] data = new byte[header.DataSize];
            byte[] tags = new byte[header.TagSize];

            twiggyCache     = new byte[header.DataSize];
            twiggyCacheTags = new byte[header.TagSize];
            twiggy          = true;

            Stream dataStream = imageFilter.GetDataForkStream();
            dataStream.Seek(dataOffset, SeekOrigin.Begin);
            dataStream.EnsureRead(data, 0, (int)header.DataSize);

            Stream tagStream = imageFilter.GetDataForkStream();
            tagStream.Seek(tagOffset, SeekOrigin.Begin);
            tagStream.EnsureRead(tags, 0, (int)header.TagSize);

            ushort mfsMagic     = BigEndianBitConverter.ToUInt16(data, (data.Length / 2) + 0x400);
            ushort mfsAllBlocks = BigEndianBitConverter.ToUInt16(data, (data.Length / 2) + 0x412);

            // Detect a Macintosh Twiggy
            if(mfsMagic     == 0xD2D7 &&
               mfsAllBlocks == 422)
            {
                AaruConsole.DebugWriteLine("DC42 plugin", Localization.Macintosh_Twiggy_detected_reversing_disk_sides);
                Array.Copy(data, header.DataSize                    / 2, twiggyCache, 0, header.DataSize    / 2);
                Array.Copy(tags, header.TagSize                     / 2, twiggyCacheTags, 0, header.TagSize / 2);
                Array.Copy(data, 0, twiggyCache, header.DataSize    / 2, header.DataSize                    / 2);
                Array.Copy(tags, 0, twiggyCacheTags, header.TagSize / 2, header.TagSize                     / 2);
            }
            else
            {
                AaruConsole.DebugWriteLine("DC42 plugin",
                                           Localization.Lisa_Twiggy_detected_reversing_second_half_of_disk);

                Array.Copy(data, 0, twiggyCache, 0, header.DataSize    / 2);
                Array.Copy(tags, 0, twiggyCacheTags, 0, header.TagSize / 2);

                int copiedSectors = 0;
                int sectorsToCopy = 0;

                for(int i = 0; i < 46; i++)
                {
                    sectorsToCopy = i switch
                    {
                        >= 0 and <= 3   => 22,
                        >= 4 and <= 10  => 21,
                        >= 11 and <= 16 => 20,
                        >= 17 and <= 22 => 19,
                        >= 23 and <= 28 => 18,
                        >= 29 and <= 34 => 17,
                        >= 35 and <= 41 => 16,
                        >= 42 and <= 45 => 15,
                        _               => sectorsToCopy
                    };

                    Array.Copy(data, (header.DataSize / 2) + (copiedSectors * 512), twiggyCache,
                               twiggyCache.Length - (copiedSectors * 512) - (sectorsToCopy * 512), sectorsToCopy * 512);

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
                        string pre     = null;

                        string major = $"{version.MajorVersion}";
                        string minor = $".{version.MinorVersion / 10}";

                        if(version.MinorVersion % 10 > 0)
                            release = $".{version.MinorVersion % 10}";

                        string dev = version.DevStage switch
                        {
                            Version.DevelopmentStage.Alpha    => "a",
                            Version.DevelopmentStage.Beta     => "b",
                            Version.DevelopmentStage.PreAlpha => "d",
                            _                                 => null
                        };

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

        AaruConsole.DebugWriteLine("DC42 plugin", Localization.Image_application_0_version_1, imageInfo.Application,
                                   imageInfo.ApplicationVersion);

        imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
        AaruConsole.VerboseWriteLine(Localization.DiskCopy_4_2_image_contains_a_disk_of_type_0, imageInfo.MediaType);

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
                imageInfo.Cylinders = imageInfo.Sectors switch
                {
                    9728  => 152,
                    19456 => 304,
                    _     => imageInfo.Cylinders
                };

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

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > imageInfo.Sectors - 1)
            return ErrorNumber.SectorNotFound;

        if(sectorAddress + length > imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        buffer = new byte[length * imageInfo.SectorSize];

        if(twiggy)
            Array.Copy(twiggyCache, (int)sectorAddress * imageInfo.SectorSize, buffer, 0,
                       length                          * imageInfo.SectorSize);
        else
        {
            Stream stream = dc42ImageFilter.GetDataForkStream();
            stream.Seek((long)(dataOffset + (sectorAddress * imageInfo.SectorSize)), SeekOrigin.Begin);
            stream.EnsureRead(buffer, 0, (int)(length * imageInfo.SectorSize));
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(tag != SectorTagType.AppleSectorTag)
            return ErrorNumber.NotSupported;

        if(header.TagSize == 0)
            return ErrorNumber.NoData;

        if(sectorAddress > imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        buffer = new byte[length * bptag];

        if(twiggy)
            Array.Copy(twiggyCacheTags, (int)sectorAddress * bptag, buffer, 0, length * bptag);
        else
        {
            Stream stream = dc42ImageFilter.GetDataForkStream();
            stream.Seek((long)(tagOffset + (sectorAddress * bptag)), SeekOrigin.Begin);
            stream.EnsureRead(buffer, 0, (int)(length * bptag));
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer) =>
        ReadSectorsLong(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        ErrorNumber errno = ReadSectors(sectorAddress, length, out byte[] data);

        if(errno != ErrorNumber.NoError)
            return errno;

        errno = ReadSectorsTag(sectorAddress, length, SectorTagType.AppleSectorTag, out byte[] tags);

        if(errno != ErrorNumber.NoError)
            return errno;

        buffer = new byte[data.Length + tags.Length];

        for(uint i = 0; i < length; i++)
        {
            Array.Copy(data, i * imageInfo.SectorSize, buffer, i * (imageInfo.SectorSize + bptag),
                       imageInfo.SectorSize);

            Array.Copy(tags, i * bptag, buffer, (i * (imageInfo.SectorSize + bptag)) + imageInfo.SectorSize, bptag);
        }

        return ErrorNumber.NoError;
    }
}