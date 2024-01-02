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
//     Reads Connectix and Microsoft Virtual PC disk images.
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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class Vhd
{
#region IWritableImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream imageStream = imageFilter.GetDataForkStream();
        var    header      = new byte[512];
        byte[] footer;

        imageStream.Seek(0, SeekOrigin.Begin);
        imageStream.EnsureRead(header, 0, 512);

        if(imageStream.Length % 2 == 0)
        {
            footer = new byte[512];
            imageStream.Seek(-512, SeekOrigin.End);
            imageStream.EnsureRead(footer, 0, 512);
        }
        else
        {
            footer = new byte[511];
            imageStream.Seek(-511, SeekOrigin.End);
            imageStream.EnsureRead(footer, 0, 511);
        }

        var headerChecksum = BigEndianBitConverter.ToUInt32(header, 0x40);
        var footerChecksum = BigEndianBitConverter.ToUInt32(footer, 0x40);
        var headerCookie   = BigEndianBitConverter.ToUInt64(header, 0);
        var footerCookie   = BigEndianBitConverter.ToUInt64(footer, 0);

        header[0x40] = 0;
        header[0x41] = 0;
        header[0x42] = 0;
        header[0x43] = 0;
        footer[0x40] = 0;
        footer[0x41] = 0;
        footer[0x42] = 0;
        footer[0x43] = 0;

        uint headerCalculatedChecksum = VhdChecksum(header);
        uint footerCalculatedChecksum = VhdChecksum(footer);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Header_checksum_equals_0_X8_calculated_equals_1_X8,
                                   headerChecksum, headerCalculatedChecksum);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Header_checksum_equals_0_X8_calculated_equals_1_X8,
                                   footerChecksum, footerCalculatedChecksum);

        byte[] usableHeader;
        uint   usableChecksum;

        if(headerCookie == IMAGE_COOKIE && headerChecksum == headerCalculatedChecksum)
        {
            usableHeader   = header;
            usableChecksum = headerChecksum;
        }
        else if(footerCookie == IMAGE_COOKIE && footerChecksum == footerCalculatedChecksum)
        {
            usableHeader   = footer;
            usableChecksum = footerChecksum;
        }
        else
        {
            AaruConsole.ErrorWriteLine(Localization.
                                           VirtualPC_plugin_Both_header_and_footer_are_corrupt_image_cannot_be_opened);

            return ErrorNumber.InvalidArgument;
        }

        _thisFooter = new HardDiskFooter
        {
            Cookie             = BigEndianBitConverter.ToUInt64(usableHeader, 0x00),
            Features           = BigEndianBitConverter.ToUInt32(usableHeader, 0x08),
            Version            = BigEndianBitConverter.ToUInt32(usableHeader, 0x0C),
            Offset             = BigEndianBitConverter.ToUInt64(usableHeader, 0x10),
            Timestamp          = BigEndianBitConverter.ToUInt32(usableHeader, 0x18),
            CreatorApplication = BigEndianBitConverter.ToUInt32(usableHeader, 0x1C),
            CreatorVersion     = BigEndianBitConverter.ToUInt32(usableHeader, 0x20),
            CreatorHostOs      = BigEndianBitConverter.ToUInt32(usableHeader, 0x24),
            OriginalSize       = BigEndianBitConverter.ToUInt64(usableHeader, 0x28),
            CurrentSize        = BigEndianBitConverter.ToUInt64(usableHeader, 0x30),
            DiskGeometry       = BigEndianBitConverter.ToUInt32(usableHeader, 0x38),
            DiskType           = BigEndianBitConverter.ToUInt32(usableHeader, 0x3C),
            Checksum           = usableChecksum,
            UniqueId           = BigEndianBitConverter.ToGuid(usableHeader, 0x44),
            SavedState         = usableHeader[0x54],
            Reserved           = new byte[usableHeader.Length - 0x55]
        };

        Array.Copy(usableHeader, 0x55, _thisFooter.Reserved, 0, usableHeader.Length - 0x55);

        _thisDateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _thisDateTime = _thisDateTime.AddSeconds(_thisFooter.Timestamp);

        var sha1Ctx = new Sha1Context();
        sha1Ctx.Update(_thisFooter.Reserved);

        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.cookie = 0x{0:X8}",   _thisFooter.Cookie);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.features = 0x{0:X8}", _thisFooter.Features);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.version = 0x{0:X8}",  _thisFooter.Version);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.offset = {0}",        _thisFooter.Offset);

        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.timestamp = 0x{0:X8} ({1})", _thisFooter.Timestamp,
                                   _thisDateTime);

        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.creatorApplication = 0x{0:X8} (\"{1}\")",
                                   _thisFooter.CreatorApplication,
                                   Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(_thisFooter.
                                                                CreatorApplication)));

        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.creatorVersion = 0x{0:X8}", _thisFooter.CreatorVersion);

        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.creatorHostOS = 0x{0:X8} (\"{1}\")", _thisFooter.CreatorHostOs,
                                   Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(_thisFooter.CreatorHostOs)));

        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.originalSize = {0}", _thisFooter.OriginalSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.currentSize = {0}",  _thisFooter.CurrentSize);

        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.diskGeometry = 0x{0:X8} (C/H/S: {1}/{2}/{3})",
                                   _thisFooter.DiskGeometry, (_thisFooter.DiskGeometry & 0xFFFF0000) >> 16,
                                   (_thisFooter.DiskGeometry & 0xFF00) >> 8, _thisFooter.DiskGeometry & 0xFF);

        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.diskType = 0x{0:X8}",     _thisFooter.DiskType);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.checksum = 0x{0:X8}",     _thisFooter.Checksum);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.uniqueId = {0}",          _thisFooter.UniqueId);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.savedState = 0x{0:X2}",   _thisFooter.SavedState);
        AaruConsole.DebugWriteLine(MODULE_NAME, "footer.reserved's SHA1 = 0x{0}", sha1Ctx.End());

        if(_thisFooter.Version == VERSION1)
            _imageInfo.Version = "1.0";
        else
        {
            AaruConsole.ErrorWriteLine(string.Format(Localization.VirtualPC_plugin_Unknown_image_type_0_found,
                                                     _thisFooter.DiskType));

            return ErrorNumber.InvalidArgument;
        }

        switch(_thisFooter.CreatorApplication)
        {
            case CREATOR_QEMU:
            {
                _imageInfo.Application = "QEMU";

                // QEMU always set same version
                _imageInfo.ApplicationVersion = "Unknown";

                break;
            }

            case CREATOR_VIRTUAL_BOX:
            {
                _imageInfo.ApplicationVersion = $"{(_thisFooter.CreatorVersion & 0xFFFF0000) >> 16}.{
                    _thisFooter.CreatorVersion & 0x0000FFFF:D2}";

                switch(_thisFooter.CreatorHostOs)
                {
                    case CREATOR_MACINTOSH:
                    case CREATOR_MACINTOSH_OLD:
                        _imageInfo.Application = "VirtualBox for Mac";

                        break;
                    case CREATOR_WINDOWS:
                        // VirtualBox uses Windows creator for any other OS
                        _imageInfo.Application = "VirtualBox";

                        break;
                    default:
                        _imageInfo.Application = string.Format(Localization.VirtualBox_for_unknown_OS_0,
                                                               Encoding.ASCII.
                                                                        GetString(BigEndianBitConverter.
                                                                            GetBytes(_thisFooter.
                                                                                CreatorHostOs)));

                        break;
                }

                break;
            }

            case CREATOR_VIRTUAL_SERVER:
            {
                _imageInfo.Application = "Microsoft Virtual Server";

                _imageInfo.ApplicationVersion = _thisFooter.CreatorVersion switch
                                                {
                                                    VERSION_VIRTUAL_SERVER2004 => "2004",
                                                    _ => string.Format(Localization.Unknown_version_0_X8,
                                                                       _thisFooter.CreatorVersion)
                                                };

                break;
            }

            case CREATOR_VIRTUAL_PC:
            {
                switch(_thisFooter.CreatorHostOs)
                {
                    case CREATOR_MACINTOSH:
                    case CREATOR_MACINTOSH_OLD:
                        switch(_thisFooter.CreatorVersion)
                        {
                            case VERSION_VIRTUAL_PC_MAC:
                                _imageInfo.Application        = "Connectix Virtual PC";
                                _imageInfo.ApplicationVersion = Localization._5_6_or_7;

                                break;
                            default:
                                _imageInfo.ApplicationVersion =
                                    string.Format(Localization.Unknown_version_0_X8, _thisFooter.CreatorVersion);

                                break;
                        }

                        break;
                    case CREATOR_WINDOWS:
                        switch(_thisFooter.CreatorVersion)
                        {
                            case VERSION_VIRTUAL_PC_MAC:
                                _imageInfo.Application        = "Connectix Virtual PC";
                                _imageInfo.ApplicationVersion = Localization._5_6_or_7;

                                break;
                            case VERSION_VIRTUAL_PC2004:
                                _imageInfo.Application        = "Microsoft Virtual PC";
                                _imageInfo.ApplicationVersion = "2004";

                                break;
                            case VERSION_VIRTUAL_PC2007:
                                _imageInfo.Application        = "Microsoft Virtual PC";
                                _imageInfo.ApplicationVersion = "2007";

                                break;
                            default:
                                _imageInfo.ApplicationVersion =
                                    string.Format(Localization.Unknown_version_0_X8, _thisFooter.CreatorVersion);

                                break;
                        }

                        break;
                    default:
                        _imageInfo.Application = string.Format(Localization.Virtual_PC_for_unknown_OS_0,
                                                               Encoding.ASCII.
                                                                        GetString(BigEndianBitConverter.
                                                                            GetBytes(_thisFooter.
                                                                                CreatorHostOs)));

                        _imageInfo.ApplicationVersion =
                            string.Format(Localization.Unknown_version_0_X8, _thisFooter.CreatorVersion);

                        break;
                }

                break;
            }

            case CREATOR_DISCIMAGECHEF:
            {
                _imageInfo.Application = "DiscImageChef";

                _imageInfo.ApplicationVersion = $"{(_thisFooter.CreatorVersion & 0xFF000000) >> 24}.{
                    (_thisFooter.CreatorVersion & 0xFF0000) >> 16}.{(_thisFooter.CreatorVersion & 0xFF00) >> 8}.{
                        _thisFooter.CreatorVersion & 0xFF}";
            }

                break;

            case CREATOR_AARU:
            {
                _imageInfo.Application = "Aaru";

                _imageInfo.ApplicationVersion = $"{(_thisFooter.CreatorVersion & 0xFF000000) >> 24}.{
                    (_thisFooter.CreatorVersion & 0xFF0000) >> 16}.{(_thisFooter.CreatorVersion & 0xFF00) >> 8}.{
                        _thisFooter.CreatorVersion & 0xFF}";
            }

                break;
            default:
            {
                _imageInfo.Application = string.Format(Localization.Unknown_application_0,
                                                       Encoding.ASCII.GetString(BigEndianBitConverter.
                                                                                    GetBytes(_thisFooter.
                                                                                        CreatorHostOs)));

                _imageInfo.ApplicationVersion =
                    string.Format(Localization.Unknown_version_0_X8, _thisFooter.CreatorVersion);

                break;
            }
        }

        _thisFilter           = imageFilter;
        _imageInfo.ImageSize  = _thisFooter.CurrentSize;
        _imageInfo.Sectors    = _thisFooter.CurrentSize / 512;
        _imageInfo.SectorSize = 512;

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = _thisDateTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);

        _imageInfo.Cylinders       = (_thisFooter.DiskGeometry & 0xFFFF0000) >> 16;
        _imageInfo.Heads           = (_thisFooter.DiskGeometry & 0xFF00)     >> 8;
        _imageInfo.SectorsPerTrack = _thisFooter.DiskGeometry & 0xFF;

        if(_thisFooter.DiskType is TYPE_DYNAMIC or TYPE_DIFFERENCING)
        {
            imageStream.Seek((long)_thisFooter.Offset, SeekOrigin.Begin);
            var dynamicBytes = new byte[1024];
            imageStream.EnsureRead(dynamicBytes, 0, 1024);

            var dynamicChecksum = BigEndianBitConverter.ToUInt32(dynamicBytes, 0x24);

            dynamicBytes[0x24] = 0;
            dynamicBytes[0x25] = 0;
            dynamicBytes[0x26] = 0;
            dynamicBytes[0x27] = 0;

            uint dynamicChecksumCalculated = VhdChecksum(dynamicBytes);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Dynamic_header_checksum_equals_0_X8_calculated_1_X8,
                                       dynamicChecksum, dynamicChecksumCalculated);

            if(dynamicChecksum != dynamicChecksumCalculated)
            {
                AaruConsole.ErrorWriteLine(Localization.
                                               VirtualPC_plugin_Both_header_and_footer_are_corrupt_image_cannot_be_opened);

                return ErrorNumber.InvalidArgument;
            }

            _thisDynamic = new DynamicDiskHeader
            {
                LocatorEntries = new ParentLocatorEntry[8], Reserved2 = new byte[256]
            };

            for(var i = 0; i < 8; i++)
                _thisDynamic.LocatorEntries[i] = new ParentLocatorEntry();

            _thisDynamic.Cookie          = BigEndianBitConverter.ToUInt64(dynamicBytes, 0x00);
            _thisDynamic.DataOffset      = BigEndianBitConverter.ToUInt64(dynamicBytes, 0x08);
            _thisDynamic.TableOffset     = BigEndianBitConverter.ToUInt64(dynamicBytes, 0x10);
            _thisDynamic.HeaderVersion   = BigEndianBitConverter.ToUInt32(dynamicBytes, 0x18);
            _thisDynamic.MaxTableEntries = BigEndianBitConverter.ToUInt32(dynamicBytes, 0x1C);
            _thisDynamic.BlockSize       = BigEndianBitConverter.ToUInt32(dynamicBytes, 0x20);
            _thisDynamic.Checksum        = dynamicChecksum;
            _thisDynamic.ParentId        = BigEndianBitConverter.ToGuid(dynamicBytes, 0x28);
            _thisDynamic.ParentTimestamp = BigEndianBitConverter.ToUInt32(dynamicBytes, 0x38);
            _thisDynamic.Reserved        = BigEndianBitConverter.ToUInt32(dynamicBytes, 0x3C);
            _thisDynamic.ParentName      = Encoding.BigEndianUnicode.GetString(dynamicBytes, 0x40, 512);

            for(var i = 0; i < 8; i++)
            {
                _thisDynamic.LocatorEntries[i].PlatformCode =
                    BigEndianBitConverter.ToUInt32(dynamicBytes, 0x240 + 0x00 + 24 * i);

                _thisDynamic.LocatorEntries[i].PlatformDataSpace =
                    BigEndianBitConverter.ToUInt32(dynamicBytes, 0x240 + 0x04 + 24 * i);

                _thisDynamic.LocatorEntries[i].PlatformDataLength =
                    BigEndianBitConverter.ToUInt32(dynamicBytes, 0x240 + 0x08 + 24 * i);

                _thisDynamic.LocatorEntries[i].Reserved =
                    BigEndianBitConverter.ToUInt32(dynamicBytes, 0x240 + 0x0C + 24 * i);

                _thisDynamic.LocatorEntries[i].PlatformDataOffset =
                    BigEndianBitConverter.ToUInt64(dynamicBytes, 0x240 + 0x10 + 24 * i);
            }

            Array.Copy(dynamicBytes, 0x300, _thisDynamic.Reserved2, 0, 256);

            _parentDateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _parentDateTime = _parentDateTime.AddSeconds(_thisDynamic.ParentTimestamp);

            sha1Ctx = new Sha1Context();
            sha1Ctx.Update(_thisDynamic.Reserved2);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.cookie = 0x{0:X8}", _thisDynamic.Cookie);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.dataOffset = {0}",  _thisDynamic.DataOffset);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.tableOffset = {0}", _thisDynamic.TableOffset);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.headerVersion = 0x{0:X8}", _thisDynamic.HeaderVersion);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.maxTableEntries = {0}", _thisDynamic.MaxTableEntries);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.blockSize = {0}",     _thisDynamic.BlockSize);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.checksum = 0x{0:X8}", _thisDynamic.Checksum);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.parentID = {0}",      _thisDynamic.ParentId);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.parentTimestamp = 0x{0:X8} ({1})",
                                       _thisDynamic.ParentTimestamp, _parentDateTime);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.reserved = 0x{0:X8}", _thisDynamic.Reserved);

            for(var i = 0; i < 8; i++)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.locatorEntries[{0}].platformCode = 0x{1:X8} (\"{2}\")",
                                           i, _thisDynamic.LocatorEntries[i].PlatformCode,
                                           Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(_thisDynamic.
                                                                        LocatorEntries[i].PlatformCode)));

                AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.locatorEntries[{0}].platformDataSpace = {1}", i,
                                           _thisDynamic.LocatorEntries[i].PlatformDataSpace);

                AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.locatorEntries[{0}].platformDataLength = {1}", i,
                                           _thisDynamic.LocatorEntries[i].PlatformDataLength);

                AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.locatorEntries[{0}].reserved = 0x{1:X8}", i,
                                           _thisDynamic.LocatorEntries[i].Reserved);

                AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.locatorEntries[{0}].platformDataOffset = {1}", i,
                                           _thisDynamic.LocatorEntries[i].PlatformDataOffset);
            }

            AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.parentName = \"{0}\"",     _thisDynamic.ParentName);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.reserved2's SHA1 = 0x{0}", sha1Ctx.End());

            if(_thisDynamic.HeaderVersion != VERSION1)
            {
                AaruConsole.ErrorWriteLine(string.Format(Localization.VirtualPC_plugin_Unknown_image_type_0_found,
                                                         _thisFooter.DiskType));

                return ErrorNumber.InvalidArgument;
            }

            var batStopwatch = new Stopwatch();
            batStopwatch.Start();

            var bat = new byte[_thisDynamic.MaxTableEntries * 4];
            imageStream.Seek((long)_thisDynamic.TableOffset, SeekOrigin.Begin);
            imageStream.EnsureRead(bat, 0, bat.Length);

            ReadOnlySpan<byte> span = bat;

            _blockAllocationTable = MemoryMarshal.Cast<byte, uint>(span)[..(int)_thisDynamic.MaxTableEntries].ToArray();

            batStopwatch.Stop();

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Filling_the_BAT_took_0_seconds,
                                       batStopwatch.Elapsed.TotalSeconds);

            _bitmapSize = (uint)Math.Ceiling((double)_thisDynamic.BlockSize /
                                             512

                                             // 1 bit per sector on the bitmap
                                            /
                                             8

                                             // and aligned to 512 byte boundary
                                            /
                                             512);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Bitmap_is_0_sectors, _bitmapSize);
        }

        _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;

        switch(_thisFooter.DiskType)
        {
            case TYPE_FIXED:
            case TYPE_DYNAMIC:
            {
                // Nothing to do here, really.
                return ErrorNumber.NoError;
            }

            case TYPE_DIFFERENCING:
            {
                _locatorEntriesData = new byte[8][];

                for(var i = 0; i < 8; i++)
                {
                    if(_thisDynamic.LocatorEntries[i].PlatformCode != 0x00000000)
                    {
                        _locatorEntriesData[i] = new byte[_thisDynamic.LocatorEntries[i].PlatformDataLength];
                        imageStream.Seek((long)_thisDynamic.LocatorEntries[i].PlatformDataOffset, SeekOrigin.Begin);

                        imageStream.EnsureRead(_locatorEntriesData[i], 0,
                                               (int)_thisDynamic.LocatorEntries[i].PlatformDataLength);

                        switch(_thisDynamic.LocatorEntries[i].PlatformCode)
                        {
                            case PLATFORM_CODE_WINDOWS_ABSOLUTE:
                            case PLATFORM_CODE_WINDOWS_RELATIVE:
                                AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.locatorEntries[{0}] = \"{1}\"", i,
                                                           Encoding.ASCII.GetString(_locatorEntriesData[i]));

                                break;
                            case PLATFORM_CODE_WINDOWS_ABSOLUTE_U:
                            case PLATFORM_CODE_WINDOWS_RELATIVE_U:
                                AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.locatorEntries[{0}] = \"{1}\"", i,
                                                           Encoding.BigEndianUnicode.GetString(_locatorEntriesData[i]));

                                break;
                            case PLATFORM_CODE_MACINTOSH_URI:
                                AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.locatorEntries[{0}] = \"{1}\"", i,
                                                           Encoding.UTF8.GetString(_locatorEntriesData[i]));

                                break;
                            default:
                                AaruConsole.DebugWriteLine(MODULE_NAME, "dynamic.locatorEntries[{0}] =", i);
                                PrintHex.PrintHexArray(_locatorEntriesData[i], 64);

                                break;
                        }
                    }
                }

                var    currentLocator = 0;
                var    locatorFound   = false;
                string parentPath     = null;

                while(!locatorFound && currentLocator < 8)
                {
                    switch(_thisDynamic.LocatorEntries[currentLocator].PlatformCode)
                    {
                        case PLATFORM_CODE_WINDOWS_ABSOLUTE:
                        case PLATFORM_CODE_WINDOWS_RELATIVE:
                            parentPath = Encoding.ASCII.GetString(_locatorEntriesData[currentLocator]);

                            break;
                        case PLATFORM_CODE_WINDOWS_ABSOLUTE_U:
                        case PLATFORM_CODE_WINDOWS_RELATIVE_U:
                            parentPath = Encoding.BigEndianUnicode.GetString(_locatorEntriesData[currentLocator]);

                            break;
                        case PLATFORM_CODE_MACINTOSH_URI:
                            parentPath =
                                Uri.UnescapeDataString(Encoding.UTF8.GetString(_locatorEntriesData[currentLocator]));

                            if(parentPath.StartsWith("file://localhost", StringComparison.InvariantCulture))
                                parentPath = parentPath.Remove(0, 16);
                            else
                            {
                                AaruConsole.DebugWriteLine(MODULE_NAME,
                                                           Localization.
                                                               Unsupported_protocol_classified_found_in_URI_parent_path_0,
                                                           parentPath);

                                parentPath = null;
                            }

                            break;
                    }

                    if(parentPath != null)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Possible_parent_path_0, parentPath);

                        IFilter parentFilter =
                            PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, parentPath));

                        if(parentFilter != null)
                            locatorFound = true;

                        if(!locatorFound)
                            parentPath = null;
                    }

                    currentLocator++;
                }

                if(!locatorFound)
                {
                    AaruConsole.ErrorWriteLine(Localization.
                                                   VirtualPC_plugin_Cannot_find_parent_file_for_differencing_disk_image);

                    return ErrorNumber.NoSuchFile;
                }

                {
                    _parentImage = new Vhd();

                    IFilter parentFilter =
                        PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, parentPath));

                    if(parentFilter == null)
                    {
                        AaruConsole.ErrorWriteLine(Localization.VirtualPC_plugin_Cannot_find_parent_image_filter);

                        return ErrorNumber.NoSuchFile;
                    }
                    /*                            PluginBase plugins = new PluginBase();
                                                plugins.RegisterAllPlugins();
                                                if (!plugins.ImagePluginsList.TryGetValue(Name.ToLower(), out parentImage))
                                                {
                                                    AaruConsole.ErrorWriteLine("(VirtualPC plugin): Unable to open myself");
                                                    return ErrorNumber.InvalidArgument;
                                                }*/

                    if(!_parentImage.Identify(parentFilter))
                    {
                        AaruConsole.ErrorWriteLine(Localization.
                                                       VirtualPC_plugin_Parent_image_is_not_a_Virtual_PC_disk_image);

                        return ErrorNumber.InvalidArgument;
                    }

                    ErrorNumber parentError = _parentImage.Open(parentFilter);

                    if(parentError != ErrorNumber.NoError)
                    {
                        AaruConsole.
                            ErrorWriteLine(string.
                                               Format(Localization.VirtualPC_plugin_Error_0_opening_parent_disk_image,
                                                      parentError));

                        return parentError;
                    }

                    // While specification says that parent and child disk images should contain UUID relationship
                    // in reality it seems that old differencing disk images stored a parent UUID that, nonetheless
                    // the parent never stored itself. So the only real way to know that images are related is
                    // because the parent IS found and SAME SIZE. Ugly...
                    // More funny even, tested parent images show an empty host OS, and child images a correct one.
                    if(_parentImage.Info.Sectors == _imageInfo.Sectors)
                        return ErrorNumber.NoError;

                    AaruConsole.ErrorWriteLine(Localization.VirtualPC_plugin_Parent_image_is_of_different_size);

                    return ErrorNumber.InvalidArgument;
                }
            }

            case TYPE_DEPRECATED1:
            case TYPE_DEPRECATED2:
            case TYPE_DEPRECATED3:
                AaruConsole.ErrorWriteLine(Localization.VirtualPC_plugin_Deprecated_image_type_found);

                return ErrorNumber.NotImplemented;

            default:
                AaruConsole.ErrorWriteLine(string.Format(Localization.VirtualPC_plugin_Unknown_image_type_0_found,
                                                         _thisFooter.DiskType));

                return ErrorNumber.InvalidArgument;
        }
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        switch(_thisFooter.DiskType)
        {
            case TYPE_DIFFERENCING:
            {
                // Block number for BAT searching
                var blockNumber = (uint)Math.Floor(sectorAddress / (_thisDynamic.BlockSize / 512.0));

                // Sector number inside of block
                var sectorInBlock = (uint)(sectorAddress % (_thisDynamic.BlockSize / 512));

                uint blockPosition = Swapping.Swap(_blockAllocationTable[blockNumber]);

                if(blockPosition == 0xFFFFFFFF)
                {
                    buffer = new byte[512];

                    return ErrorNumber.NoError;
                }

                var bitmap = new byte[_bitmapSize * 512];

                // Offset of block in file
                long blockOffset = blockPosition * 512L;

                var bitmapByte = (int)Math.Floor((double)sectorInBlock / 8);
                var bitmapBit  = (int)(sectorInBlock % 8);

                Stream thisStream = _thisFilter.GetDataForkStream();

                thisStream.Seek(blockOffset, SeekOrigin.Begin);
                thisStream.EnsureRead(bitmap, 0, (int)_bitmapSize * 512);

                var  mask  = (byte)(1 << 7 - bitmapBit);
                bool dirty = (bitmap[bitmapByte] & mask) == mask;

                /*
                AaruConsole.DebugWriteLine(MODULE_NAME, "bitmapSize = {0}", bitmapSize);
                AaruConsole.DebugWriteLine(MODULE_NAME, "blockNumber = {0}", blockNumber);
                AaruConsole.DebugWriteLine(MODULE_NAME, "sectorInBlock = {0}", sectorInBlock);
                AaruConsole.DebugWriteLine(MODULE_NAME, "blockOffset = {0}", blockOffset);
                AaruConsole.DebugWriteLine(MODULE_NAME, "bitmapByte = {0}", bitmapByte);
                AaruConsole.DebugWriteLine(MODULE_NAME, "bitmapBit = {0}", bitmapBit);
                AaruConsole.DebugWriteLine(MODULE_NAME, "mask = 0x{0:X2}", mask);
                AaruConsole.DebugWriteLine(MODULE_NAME, "dirty = 0x{0}", dirty);
                AaruConsole.DebugWriteLine(MODULE_NAME, "bitmap = ");
                PrintHex.PrintHexArray(bitmap, 64);
                */

                // Sector has been written, read from child image
                if(!dirty)
                    return _parentImage.ReadSector(sectorAddress, out buffer);
                /* Too noisy
                    AaruConsole.DebugWriteLine(MODULE_NAME, "Sector {0} is dirty", sectorAddress);
                    */

                buffer = new byte[512];
                uint sectorOffset = blockPosition + _bitmapSize + sectorInBlock;
                thisStream = _thisFilter.GetDataForkStream();

                thisStream.Seek(sectorOffset * 512, SeekOrigin.Begin);
                thisStream.EnsureRead(buffer, 0, 512);

                return ErrorNumber.NoError;

                /* Too noisy
                AaruConsole.DebugWriteLine(MODULE_NAME, "Sector {0} is clean", sectorAddress);
                */

                // Read sector from parent image
            }

            default:
                return ReadSectors(sectorAddress, 1, out buffer);
        }
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        switch(_thisFooter.DiskType)
        {
            case TYPE_FIXED:
            {
                buffer = new byte[512 * length];
                Stream thisStream = _thisFilter.GetDataForkStream();

                thisStream.Seek((long)(sectorAddress       * 512), SeekOrigin.Begin);
                thisStream.EnsureRead(buffer, 0, (int)(512 * length));

                return ErrorNumber.NoError;
            }

            // Contrary to Microsoft's specifications that tell us to check the bitmap
            // and in case of unused sector just return zeros, as blocks are allocated
            // as a whole, this would waste time and miss cache, so we read any sector
            // as long as it is in the block.
            case TYPE_DYNAMIC:
            {
                // Block number for BAT searching
                var blockNumber = (uint)Math.Floor(sectorAddress / (_thisDynamic.BlockSize / 512.0));

                // Sector number inside of block
                var sectorInBlock = (uint)(sectorAddress % (_thisDynamic.BlockSize / 512));

                // How many sectors before reaching end of block
                uint remainingInBlock = _thisDynamic.BlockSize / 512 - sectorInBlock;

                // Data that needs to be read from another block
                byte[] suffix = null;

                // How many sectors to read from this block
                uint sectorsToReadHere;

                // Asked to read more sectors than are remaining in block
                if(length > remainingInBlock)
                {
                    ErrorNumber errno = ReadSectors(sectorAddress + remainingInBlock, length - remainingInBlock,
                                                    out suffix);

                    if(errno != ErrorNumber.NoError)
                        return errno;

                    sectorsToReadHere = remainingInBlock;
                }
                else
                    sectorsToReadHere = length;

                // Keep the BAT in memory in big endian
                uint blockPosition = Swapping.Swap(_blockAllocationTable[blockNumber]);

                // Offset of sector in file
                uint sectorOffset = blockPosition + _bitmapSize + sectorInBlock;

                // Data that can be read in this block
                var prefix = new byte[sectorsToReadHere * 512];

                // 0xFFFFFFFF means unallocated
                if(blockPosition != 0xFFFFFFFF)
                {
                    Stream thisStream = _thisFilter.GetDataForkStream();
                    thisStream.Seek(sectorOffset * 512, SeekOrigin.Begin);
                    thisStream.EnsureRead(prefix, 0, (int)(512 * sectorsToReadHere));
                }

                // If it is unallocated, just fill with zeroes
                else
                    Array.Clear(prefix, 0, prefix.Length);

                // If we needed to read from another block, join all the data
                if(suffix == null)
                {
                    buffer = prefix;

                    return ErrorNumber.NoError;
                }

                buffer = new byte[512 * length];
                Array.Copy(prefix, 0, buffer, 0,             prefix.Length);
                Array.Copy(suffix, 0, buffer, prefix.Length, suffix.Length);

                return ErrorNumber.NoError;
            }

            case TYPE_DIFFERENCING:
            {
                // As on differencing images, each independent sector can be read from child or parent
                // image, we must read sector one by one
                buffer = new byte[512 * length];

                for(ulong i = 0; i < length; i++)
                {
                    ErrorNumber errno = ReadSector(sectorAddress + i, out byte[] oneSector);

                    if(errno != ErrorNumber.NoError)
                        return errno;

                    Array.Copy(oneSector, 0, buffer, (int)(i * 512), 512);
                }

                return ErrorNumber.NoError;
            }

            case TYPE_DEPRECATED1:
            case TYPE_DEPRECATED2:
            case TYPE_DEPRECATED3:
                AaruConsole.ErrorWriteLine(Localization.VirtualPC_plugin_Deprecated_image_type_found);

                return ErrorNumber.NotImplemented;

            default:
                AaruConsole.ErrorWriteLine(string.Format(Localization.VirtualPC_plugin_Unknown_image_type_0_found,
                                                         _thisFooter.DiskType));

                return ErrorNumber.InvalidArgument;
        }
    }

#endregion
}