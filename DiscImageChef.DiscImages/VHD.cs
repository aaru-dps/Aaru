// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : VHD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Connectix and Microsoft Virtual PC disk images.
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
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using DiscImageChef.Interop;
using PlatformID = DiscImageChef.Interop.PlatformID;
using Version = System.Version;

namespace DiscImageChef.DiscImages
{
    /// <inheritdoc />
    /// <summary>
    ///     Supports Connectix/Microsoft Virtual PC hard disk image format
    ///     Until Virtual PC 5 there existed no format, and the hard disk image was
    ///     merely a sector by sector (RAW) image with a resource fork giving
    ///     information to Virtual PC itself.
    /// </summary>
    public class Vhd : IWritableImage
    {
        /// <summary>
        ///     File magic number, "conectix"
        /// </summary>
        const ulong IMAGE_COOKIE = 0x636F6E6563746978;
        /// <summary>
        ///     Dynamic disk header magic, "cxsparse"
        /// </summary>
        const ulong DYNAMIC_COOKIE = 0x6378737061727365;

        /// <summary>
        ///     Disk image is candidate for deletion on shutdown
        /// </summary>
        const uint FEATURES_TEMPORARY = 0x00000001;
        /// <summary>
        ///     Unknown, set from Virtual PC for Mac 7 onwards
        /// </summary>
        const uint FEATURES_RESERVED = 0x00000002;
        /// <summary>
        ///     Unknown
        /// </summary>
        const uint FEATURES_UNKNOWN = 0x00000100;

        /// <summary>
        ///     Only known version
        /// </summary>
        const uint VERSION1 = 0x00010000;

        /// <summary>
        ///     Created by Virtual PC, "vpc "
        /// </summary>
        const uint CREATOR_VIRTUAL_PC = 0x76706320;
        /// <summary>
        ///     Created by Virtual Server, "vs  "
        /// </summary>
        const uint CREATOR_VIRTUAL_SERVER = 0x76732020;
        /// <summary>
        ///     Created by QEMU, "qemu"
        /// </summary>
        const uint CREATOR_QEMU = 0x71656D75;
        /// <summary>
        ///     Created by VirtualBox, "vbox"
        /// </summary>
        const uint CREATOR_VIRTUAL_BOX = 0x76626F78;
        /// <summary>
        ///     Created by DiscImageChef, "dic "
        /// </summary>
        const uint CREATOR_DISCIMAGECHEF = 0x64696320;

        /// <summary>
        ///     Disk image created by Virtual Server 2004
        /// </summary>
        const uint VERSION_VIRTUAL_SERVER2004 = 0x00010000;
        /// <summary>
        ///     Disk image created by Virtual PC 2004
        /// </summary>
        const uint VERSION_VIRTUAL_PC2004 = 0x00050000;
        /// <summary>
        ///     Disk image created by Virtual PC 2007
        /// </summary>
        const uint VERSION_VIRTUAL_PC2007 = 0x00050003;
        /// <summary>
        ///     Disk image created by Virtual PC for Mac 5, 6 or 7
        /// </summary>
        const uint VERSION_VIRTUAL_PC_MAC = 0x00040000;

        /// <summary>
        ///     Disk image created in Windows, "Wi2k"
        /// </summary>
        const uint CREATOR_WINDOWS = 0x5769326B;
        /// <summary>
        ///     Disk image created in Macintosh, "Mac "
        /// </summary>
        const uint CREATOR_MACINTOSH = 0x4D616320;
        /// <summary>
        ///     Seen in Virtual PC for Mac for dynamic and fixed images
        /// </summary>
        const uint CREATOR_MACINTOSH_OLD = 0x00000000;

        /// <summary>
        ///     Disk image type is none, useless?
        /// </summary>
        const uint TYPE_NONE = 0;
        /// <summary>
        ///     Deprecated disk image type
        /// </summary>
        const uint TYPE_DEPRECATED1 = 1;
        /// <summary>
        ///     Fixed disk image type
        /// </summary>
        const uint TYPE_FIXED = 2;
        /// <summary>
        ///     Dynamic disk image type
        /// </summary>
        const uint TYPE_DYNAMIC = 3;
        /// <summary>
        ///     Differencing disk image type
        /// </summary>
        const uint TYPE_DIFFERENCING = 4;
        /// <summary>
        ///     Deprecated disk image type
        /// </summary>
        const uint TYPE_DEPRECATED2 = 5;
        /// <summary>
        ///     Deprecated disk image type
        /// </summary>
        const uint TYPE_DEPRECATED3 = 6;

        /// <summary>
        ///     Means platform locator is unused
        /// </summary>
        const uint PLATFORM_CODE_UNUSED = 0x00000000;
        /// <summary>
        ///     Stores a relative path string for Windows, unknown locale used, deprecated, "Wi2r"
        /// </summary>
        const uint PLATFORM_CODE_WINDOWS_RELATIVE = 0x57693272;
        /// <summary>
        ///     Stores an absolute path string for Windows, unknown locale used, deprecated, "Wi2k"
        /// </summary>
        const uint PLATFORM_CODE_WINDOWS_ABSOLUTE = 0x5769326B;
        /// <summary>
        ///     Stores a relative path string for Windows in UTF-16, "W2ru"
        /// </summary>
        const uint PLATFORM_CODE_WINDOWS_RELATIVE_U = 0x57327275;
        /// <summary>
        ///     Stores an absolute path string for Windows in UTF-16, "W2ku"
        /// </summary>
        const uint PLATFORM_CODE_WINDOWS_ABSOLUTE_U = 0x57326B75;
        /// <summary>
        ///     Stores a Mac OS alias as a blob, "Mac "
        /// </summary>
        const uint PLATFORM_CODE_MACINTOSH_ALIAS = 0x4D616320;
        /// <summary>
        ///     Stores a Mac OS X URI (RFC-2396) absolute path in UTF-8, "MacX"
        /// </summary>
        const uint        PLATFORM_CODE_MACINTOSH_URI = 0x4D616358;
        uint              bitmapSize;
        uint[]            blockAllocationTable;
        ImageInfo         imageInfo;
        byte[][]          locatorEntriesData;
        DateTime          parentDateTime;
        IMediaImage       parentImage;
        DateTime          thisDateTime;
        DynamicDiskHeader thisDynamic;
        IFilter           thisFilter;

        HardDiskFooter thisFooter;
        FileStream     writingStream;

        public Vhd()
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

        public ImageInfo Info => imageInfo;

        public string Name => "VirtualPC";
        public Guid   Id   => new Guid("8014d88f-64cd-4484-9441-7635c632958a");

        public string Format
        {
            get
            {
                switch(thisFooter.DiskType)
                {
                    case TYPE_FIXED:        return "Virtual PC fixed size disk image";
                    case TYPE_DYNAMIC:      return "Virtual PC dynamic size disk image";
                    case TYPE_DIFFERENCING: return "Virtual PC differencing disk image";
                    default:                return "Virtual PC disk image";
                }
            }
        }

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool Identify(IFilter imageFilter)
        {
            Stream imageStream = imageFilter.GetDataForkStream();
            ulong  headerCookie;
            ulong  footerCookie;

            byte[] headerCookieBytes = new byte[8];
            byte[] footerCookieBytes = new byte[8];

            if(imageStream.Length % 2 == 0) imageStream.Seek(-512, SeekOrigin.End);
            else imageStream.Seek(-511,                            SeekOrigin.End);

            imageStream.Read(footerCookieBytes, 0, 8);
            imageStream.Seek(0, SeekOrigin.Begin);
            imageStream.Read(headerCookieBytes, 0, 8);

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            headerCookie                         = BigEndianBitConverter.ToUInt64(headerCookieBytes, 0);
            footerCookie                         = BigEndianBitConverter.ToUInt64(footerCookieBytes, 0);

            return headerCookie == IMAGE_COOKIE || footerCookie == IMAGE_COOKIE;
        }

        public bool Open(IFilter imageFilter)
        {
            Stream imageStream = imageFilter.GetDataForkStream();
            byte[] header      = new byte[512];
            byte[] footer;

            imageStream.Seek(0, SeekOrigin.Begin);
            imageStream.Read(header, 0, 512);

            if(imageStream.Length % 2 == 0)
            {
                footer = new byte[512];
                imageStream.Seek(-512, SeekOrigin.End);
                imageStream.Read(footer, 0, 512);
            }
            else
            {
                footer = new byte[511];
                imageStream.Seek(-511, SeekOrigin.End);
                imageStream.Read(footer, 0, 511);
            }

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            uint  headerChecksum = BigEndianBitConverter.ToUInt32(header, 0x40);
            uint  footerChecksum = BigEndianBitConverter.ToUInt32(footer, 0x40);
            ulong headerCookie   = BigEndianBitConverter.ToUInt64(header, 0);
            ulong footerCookie   = BigEndianBitConverter.ToUInt64(footer, 0);

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

            DicConsole.DebugWriteLine("VirtualPC plugin", "Header checksum = 0x{0:X8}, calculated = 0x{1:X8}",
                                      headerChecksum, headerCalculatedChecksum);
            DicConsole.DebugWriteLine("VirtualPC plugin", "Header checksum = 0x{0:X8}, calculated = 0x{1:X8}",
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
                throw new
                    ImageNotSupportedException("(VirtualPC plugin): Both header and footer are corrupt, image cannot be opened.");

            thisFooter = new HardDiskFooter
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
            Array.Copy(usableHeader, 0x55, thisFooter.Reserved, 0, usableHeader.Length - 0x55);

            thisDateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            thisDateTime = thisDateTime.AddSeconds(thisFooter.Timestamp);

            Sha1Context sha1Ctx = new Sha1Context();
            sha1Ctx.Init();
            sha1Ctx.Update(thisFooter.Reserved);

            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.cookie = 0x{0:X8}",   thisFooter.Cookie);
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.features = 0x{0:X8}", thisFooter.Features);
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.version = 0x{0:X8}",  thisFooter.Version);
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.offset = {0}",        thisFooter.Offset);
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.timestamp = 0x{0:X8} ({1})", thisFooter.Timestamp,
                                      thisDateTime);
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.creatorApplication = 0x{0:X8} (\"{1}\")",
                                      thisFooter.CreatorApplication,
                                      Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(thisFooter
                                                                                                 .CreatorApplication)));
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.creatorVersion = 0x{0:X8}",
                                      thisFooter.CreatorVersion);
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.creatorHostOS = 0x{0:X8} (\"{1}\")",
                                      thisFooter.CreatorHostOs,
                                      Encoding.ASCII.GetString(BigEndianBitConverter
                                                                  .GetBytes(thisFooter.CreatorHostOs)));
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.originalSize = {0}", thisFooter.OriginalSize);
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.currentSize = {0}",  thisFooter.CurrentSize);
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.diskGeometry = 0x{0:X8} (C/H/S: {1}/{2}/{3})",
                                      thisFooter.DiskGeometry, (thisFooter.DiskGeometry & 0xFFFF0000) >> 16,
                                      (thisFooter.DiskGeometry                          & 0xFF00)     >> 8,
                                      thisFooter.DiskGeometry                           & 0xFF);
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.diskType = 0x{0:X8}",     thisFooter.DiskType);
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.checksum = 0x{0:X8}",     thisFooter.Checksum);
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.uniqueId = {0}",          thisFooter.UniqueId);
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.savedState = 0x{0:X2}",   thisFooter.SavedState);
            DicConsole.DebugWriteLine("VirtualPC plugin", "footer.reserved's SHA1 = 0x{0}", sha1Ctx.End());

            if(thisFooter.Version == VERSION1) imageInfo.Version = "1.0";
            else
                throw new
                    ImageNotSupportedException($"(VirtualPC plugin): Unknown image type {thisFooter.DiskType} found. Please submit a bug with an example image.");

            switch(thisFooter.CreatorApplication)
            {
                case CREATOR_QEMU:
                {
                    imageInfo.Application = "QEMU";
                    // QEMU always set same version
                    imageInfo.ApplicationVersion = "Unknown";

                    break;
                }
                case CREATOR_VIRTUAL_BOX:
                {
                    imageInfo.ApplicationVersion =
                        $"{(thisFooter.CreatorVersion & 0xFFFF0000) >> 16}.{thisFooter.CreatorVersion & 0x0000FFFF:D2}";
                    switch(thisFooter.CreatorHostOs)
                    {
                        case CREATOR_MACINTOSH:
                        case CREATOR_MACINTOSH_OLD:
                            imageInfo.Application = "VirtualBox for Mac";
                            break;
                        case CREATOR_WINDOWS:
                            // VirtualBox uses Windows creator for any other OS
                            imageInfo.Application = "VirtualBox";
                            break;
                        default:
                            imageInfo.Application =
                                $"VirtualBox for unknown OS \"{Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(thisFooter.CreatorHostOs))}\"";
                            break;
                    }

                    break;
                }
                case CREATOR_VIRTUAL_SERVER:
                {
                    imageInfo.Application = "Microsoft Virtual Server";
                    switch(thisFooter.CreatorVersion)
                    {
                        case VERSION_VIRTUAL_SERVER2004:
                            imageInfo.ApplicationVersion = "2004";
                            break;
                        default:
                            imageInfo.ApplicationVersion = $"Unknown version 0x{thisFooter.CreatorVersion:X8}";
                            break;
                    }

                    break;
                }
                case CREATOR_VIRTUAL_PC:
                {
                    switch(thisFooter.CreatorHostOs)
                    {
                        case CREATOR_MACINTOSH:
                        case CREATOR_MACINTOSH_OLD:
                            switch(thisFooter.CreatorVersion)
                            {
                                case VERSION_VIRTUAL_PC_MAC:
                                    imageInfo.Application        = "Connectix Virtual PC";
                                    imageInfo.ApplicationVersion = "5, 6 or 7";
                                    break;
                                default:
                                    imageInfo.ApplicationVersion = $"Unknown version 0x{thisFooter.CreatorVersion:X8}";
                                    break;
                            }

                            break;
                        case CREATOR_WINDOWS:
                            switch(thisFooter.CreatorVersion)
                            {
                                case VERSION_VIRTUAL_PC_MAC:
                                    imageInfo.Application        = "Connectix Virtual PC";
                                    imageInfo.ApplicationVersion = "5, 6 or 7";
                                    break;
                                case VERSION_VIRTUAL_PC2004:
                                    imageInfo.Application        = "Microsoft Virtual PC";
                                    imageInfo.ApplicationVersion = "2004";
                                    break;
                                case VERSION_VIRTUAL_PC2007:
                                    imageInfo.Application        = "Microsoft Virtual PC";
                                    imageInfo.ApplicationVersion = "2007";
                                    break;
                                default:
                                    imageInfo.ApplicationVersion = $"Unknown version 0x{thisFooter.CreatorVersion:X8}";
                                    break;
                            }

                            break;
                        default:
                            imageInfo.Application =
                                $"Virtual PC for unknown OS \"{Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(thisFooter.CreatorHostOs))}\"";
                            imageInfo.ApplicationVersion = $"Unknown version 0x{thisFooter.CreatorVersion:X8}";
                            break;
                    }

                    break;
                }
                case CREATOR_DISCIMAGECHEF:
                {
                    imageInfo.Application        = "DiscImageChef";
                    imageInfo.ApplicationVersion =
                        $"{(thisFooter.CreatorVersion & 0xFF000000) >> 24}.{(thisFooter.CreatorVersion & 0xFF0000) >> 16}.{(thisFooter.CreatorVersion & 0xFF00) >> 8}.{thisFooter.CreatorVersion & 0xFF}";
                }
                    break;
                default:
                {
                    imageInfo.Application =
                        $"Unknown application \"{Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(thisFooter.CreatorHostOs))}\"";
                    imageInfo.ApplicationVersion = $"Unknown version 0x{thisFooter.CreatorVersion:X8}";
                    break;
                }
            }

            thisFilter           = imageFilter;
            imageInfo.ImageSize  = thisFooter.CurrentSize;
            imageInfo.Sectors    = thisFooter.CurrentSize / 512;
            imageInfo.SectorSize = 512;

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = thisDateTime;
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());

            imageInfo.Cylinders       = (thisFooter.DiskGeometry & 0xFFFF0000) >> 16;
            imageInfo.Heads           = (thisFooter.DiskGeometry & 0xFF00)     >> 8;
            imageInfo.SectorsPerTrack = thisFooter.DiskGeometry  & 0xFF;

            if(thisFooter.DiskType == TYPE_DYNAMIC || thisFooter.DiskType == TYPE_DIFFERENCING)
            {
                imageStream.Seek((long)thisFooter.Offset, SeekOrigin.Begin);
                byte[] dynamicBytes = new byte[1024];
                imageStream.Read(dynamicBytes, 0, 1024);

                uint dynamicChecksum = BigEndianBitConverter.ToUInt32(dynamicBytes, 0x24);

                dynamicBytes[0x24] = 0;
                dynamicBytes[0x25] = 0;
                dynamicBytes[0x26] = 0;
                dynamicBytes[0x27] = 0;

                uint dynamicChecksumCalculated = VhdChecksum(dynamicBytes);

                DicConsole.DebugWriteLine("VirtualPC plugin",
                                          "Dynamic header checksum = 0x{0:X8}, calculated = 0x{1:X8}", dynamicChecksum,
                                          dynamicChecksumCalculated);

                if(dynamicChecksum != dynamicChecksumCalculated)
                    throw new
                        ImageNotSupportedException("(VirtualPC plugin): Both header and footer are corrupt, image cannot be opened.");

                thisDynamic                               =
                    new DynamicDiskHeader {LocatorEntries = new ParentLocatorEntry[8], Reserved2 = new byte[256]};

                for(int i = 0; i < 8; i++) thisDynamic.LocatorEntries[i] = new ParentLocatorEntry();

                thisDynamic.Cookie          = BigEndianBitConverter.ToUInt64(dynamicBytes, 0x00);
                thisDynamic.DataOffset      = BigEndianBitConverter.ToUInt64(dynamicBytes, 0x08);
                thisDynamic.TableOffset     = BigEndianBitConverter.ToUInt64(dynamicBytes, 0x10);
                thisDynamic.HeaderVersion   = BigEndianBitConverter.ToUInt32(dynamicBytes, 0x18);
                thisDynamic.MaxTableEntries = BigEndianBitConverter.ToUInt32(dynamicBytes, 0x1C);
                thisDynamic.BlockSize       = BigEndianBitConverter.ToUInt32(dynamicBytes, 0x20);
                thisDynamic.Checksum        = dynamicChecksum;
                thisDynamic.ParentId        = BigEndianBitConverter.ToGuid(dynamicBytes, 0x28);
                thisDynamic.ParentTimestamp = BigEndianBitConverter.ToUInt32(dynamicBytes, 0x38);
                thisDynamic.Reserved        = BigEndianBitConverter.ToUInt32(dynamicBytes, 0x3C);
                thisDynamic.ParentName      = Encoding.BigEndianUnicode.GetString(dynamicBytes, 0x40, 512);

                for(int i = 0; i < 8; i++)
                {
                    thisDynamic.LocatorEntries[i].PlatformCode =
                        BigEndianBitConverter.ToUInt32(dynamicBytes, 0x240 + 0x00 + 24 * i);
                    thisDynamic.LocatorEntries[i].PlatformDataSpace =
                        BigEndianBitConverter.ToUInt32(dynamicBytes, 0x240 + 0x04 + 24 * i);
                    thisDynamic.LocatorEntries[i].PlatformDataLength =
                        BigEndianBitConverter.ToUInt32(dynamicBytes, 0x240 + 0x08 + 24 * i);
                    thisDynamic.LocatorEntries[i].Reserved =
                        BigEndianBitConverter.ToUInt32(dynamicBytes, 0x240 + 0x0C + 24 * i);
                    thisDynamic.LocatorEntries[i].PlatformDataOffset =
                        BigEndianBitConverter.ToUInt64(dynamicBytes, 0x240 + 0x10 + 24 * i);
                }

                Array.Copy(dynamicBytes, 0x300, thisDynamic.Reserved2, 0, 256);

                parentDateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                parentDateTime = parentDateTime.AddSeconds(thisDynamic.ParentTimestamp);

                sha1Ctx = new Sha1Context();
                sha1Ctx.Init();
                sha1Ctx.Update(thisDynamic.Reserved2);

                DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.cookie = 0x{0:X8}", thisDynamic.Cookie);
                DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.dataOffset = {0}",  thisDynamic.DataOffset);
                DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.tableOffset = {0}", thisDynamic.TableOffset);
                DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.headerVersion = 0x{0:X8}",
                                          thisDynamic.HeaderVersion);
                DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.maxTableEntries = {0}",
                                          thisDynamic.MaxTableEntries);
                DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.blockSize = {0}",     thisDynamic.BlockSize);
                DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.checksum = 0x{0:X8}", thisDynamic.Checksum);
                DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.parentID = {0}",      thisDynamic.ParentId);
                DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.parentTimestamp = 0x{0:X8} ({1})",
                                          thisDynamic.ParentTimestamp, parentDateTime);
                DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.reserved = 0x{0:X8}", thisDynamic.Reserved);
                for(int i = 0; i < 8; i++)
                {
                    DicConsole.DebugWriteLine("VirtualPC plugin",
                                              "dynamic.locatorEntries[{0}].platformCode = 0x{1:X8} (\"{2}\")", i,
                                              thisDynamic.LocatorEntries[i].PlatformCode,
                                              Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(thisDynamic
                                                                                                     .LocatorEntries[i]
                                                                                                     .PlatformCode)));
                    DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.locatorEntries[{0}].platformDataSpace = {1}",
                                              i, thisDynamic.LocatorEntries[i].PlatformDataSpace);
                    DicConsole.DebugWriteLine("VirtualPC plugin",
                                              "dynamic.locatorEntries[{0}].platformDataLength = {1}", i,
                                              thisDynamic.LocatorEntries[i].PlatformDataLength);
                    DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.locatorEntries[{0}].reserved = 0x{1:X8}", i,
                                              thisDynamic.LocatorEntries[i].Reserved);
                    DicConsole.DebugWriteLine("VirtualPC plugin",
                                              "dynamic.locatorEntries[{0}].platformDataOffset = {1}", i,
                                              thisDynamic.LocatorEntries[i].PlatformDataOffset);
                }

                DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.parentName = \"{0}\"",
                                          thisDynamic.ParentName);
                DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.reserved2's SHA1 = 0x{0}", sha1Ctx.End());

                if(thisDynamic.HeaderVersion != VERSION1)
                    throw new
                        ImageNotSupportedException($"(VirtualPC plugin): Unknown image type {thisFooter.DiskType} found. Please submit a bug with an example image.");

                DateTime startTime = DateTime.UtcNow;

                blockAllocationTable = new uint[thisDynamic.MaxTableEntries];

                // Safe and slow code. It takes 76,572 ms to fill a 30720 entries BAT
                /*
                byte[] bat = new byte[thisDynamic.maxTableEntries * 4];
                imageStream.Seek((long)thisDynamic.tableOffset, SeekOrigin.Begin);
                imageStream.Read(bat, 0, (int)(thisDynamic.maxTableEntries * 4));
                for (int i = 0; i < thisDynamic.maxTableEntries; i++)
                    blockAllocationTable[i] = BigEndianBitConverter.ToUInt32(bat, 4 * i);

                DateTime endTime = DateTime.UtcNow;
                DicConsole.DebugWriteLine("VirtualPC plugin", "Filling the BAT took {0} seconds", (endTime-startTime).TotalSeconds);
                */

                // How many sectors uses the BAT
                uint batSectorCount = (uint)Math.Ceiling((double)thisDynamic.MaxTableEntries * 4 / 512);

                byte[] batSectorBytes = new byte[512];

                // Unsafe and fast code. It takes 4 ms to fill a 30720 entries BAT
                for(int i = 0; i < batSectorCount; i++)
                {
                    imageStream.Seek((long)thisDynamic.TableOffset + i * 512, SeekOrigin.Begin);
                    imageStream.Read(batSectorBytes, 0, 512);
                    // This does the big-endian trick but reverses the order of elements also
                    Array.Reverse(batSectorBytes);
                    GCHandle  handle    = GCHandle.Alloc(batSectorBytes, GCHandleType.Pinned);
                    BatSector batSector =
                        (BatSector)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(BatSector));
                    handle.Free();
                    // This restores the order of elements
                    Array.Reverse(batSector.blockPointer);
                    if(blockAllocationTable.Length >= i                               * 512 / 4 + 512 / 4)
                        Array.Copy(batSector.blockPointer, 0, blockAllocationTable, i * 512 / 4, 512  / 4);
                    else
                        Array.Copy(batSector.blockPointer, 0, blockAllocationTable, i * 512 / 4,
                                   blockAllocationTable.Length - i                    * 512 / 4);
                }

                DateTime endTime = DateTime.UtcNow;
                DicConsole.DebugWriteLine("VirtualPC plugin", "Filling the BAT took {0} seconds",
                                          (endTime - startTime).TotalSeconds);

                // Too noisy
                /*
                    for (int i = 0; i < thisDynamic.maxTableEntries; i++)
                        DicConsole.DebugWriteLine("VirtualPC plugin", "blockAllocationTable[{0}] = {1}", i, blockAllocationTable[i]);
                */

                // Get the roundest number of sectors needed to store the block bitmap
                bitmapSize = (uint)Math.Ceiling((double)thisDynamic.BlockSize / 512
                                                                                // 1 bit per sector on the bitmap
                                                                              / 8
                                                                                // and aligned to 512 byte boundary
                                                                              / 512);
                DicConsole.DebugWriteLine("VirtualPC plugin", "Bitmap is {0} sectors", bitmapSize);
            }

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            switch(thisFooter.DiskType)
            {
                case TYPE_FIXED:
                case TYPE_DYNAMIC:
                {
                    // Nothing to do here, really.
                    return true;
                }
                case TYPE_DIFFERENCING:
                {
                    locatorEntriesData = new byte[8][];
                    for(int i = 0; i                                  < 8; i++)
                        if(thisDynamic.LocatorEntries[i].PlatformCode != 0x00000000)
                        {
                            locatorEntriesData[i] = new byte[thisDynamic.LocatorEntries[i].PlatformDataLength];
                            imageStream.Seek((long)thisDynamic.LocatorEntries[i].PlatformDataOffset, SeekOrigin.Begin);
                            imageStream.Read(locatorEntriesData[i], 0,
                                             (int)thisDynamic.LocatorEntries[i].PlatformDataLength);

                            switch(thisDynamic.LocatorEntries[i].PlatformCode)
                            {
                                case PLATFORM_CODE_WINDOWS_ABSOLUTE:
                                case PLATFORM_CODE_WINDOWS_RELATIVE:
                                    DicConsole.DebugWriteLine("VirtualPC plugin",
                                                              "dynamic.locatorEntries[{0}] = \"{1}\"", i,
                                                              Encoding.ASCII.GetString(locatorEntriesData[i]));
                                    break;
                                case PLATFORM_CODE_WINDOWS_ABSOLUTE_U:
                                case PLATFORM_CODE_WINDOWS_RELATIVE_U:
                                    DicConsole.DebugWriteLine("VirtualPC plugin",
                                                              "dynamic.locatorEntries[{0}] = \"{1}\"", i,
                                                              Encoding.BigEndianUnicode
                                                                      .GetString(locatorEntriesData[i]));
                                    break;
                                case PLATFORM_CODE_MACINTOSH_URI:
                                    DicConsole.DebugWriteLine("VirtualPC plugin",
                                                              "dynamic.locatorEntries[{0}] = \"{1}\"", i,
                                                              Encoding.UTF8.GetString(locatorEntriesData[i]));
                                    break;
                                default:
                                    DicConsole.DebugWriteLine("VirtualPC plugin", "dynamic.locatorEntries[{0}] =", i);
                                    PrintHex.PrintHexArray(locatorEntriesData[i], 64);
                                    break;
                            }
                        }

                    int    currentLocator = 0;
                    bool   locatorFound   = false;
                    string parentPath     = null;

                    while(!locatorFound && currentLocator < 8)
                    {
                        switch(thisDynamic.LocatorEntries[currentLocator].PlatformCode)
                        {
                            case PLATFORM_CODE_WINDOWS_ABSOLUTE:
                            case PLATFORM_CODE_WINDOWS_RELATIVE:
                                parentPath = Encoding.ASCII.GetString(locatorEntriesData[currentLocator]);
                                break;
                            case PLATFORM_CODE_WINDOWS_ABSOLUTE_U:
                            case PLATFORM_CODE_WINDOWS_RELATIVE_U:
                                parentPath = Encoding.BigEndianUnicode.GetString(locatorEntriesData[currentLocator]);
                                break;
                            case PLATFORM_CODE_MACINTOSH_URI:
                                parentPath =
                                    Uri.UnescapeDataString(Encoding.UTF8.GetString(locatorEntriesData[currentLocator]));
                                if(parentPath.StartsWith("file://localhost", StringComparison.InvariantCulture))
                                    parentPath = parentPath.Remove(0, 16);
                                else
                                {
                                    DicConsole.DebugWriteLine("VirtualPC plugin",
                                                              "Unsupported protocol classified found in URI parent path: \"{0}\"",
                                                              parentPath);
                                    parentPath = null;
                                }

                                break;
                        }

                        if(parentPath != null)
                        {
                            DicConsole.DebugWriteLine("VirtualPC plugin", "Possible parent path: \"{0}\"", parentPath);
                            IFilter parentFilter =
                                new FiltersList().GetFilter(Path.Combine(imageFilter.GetParentFolder(), parentPath));

                            if(parentFilter != null) locatorFound = true;

                            if(!locatorFound) parentPath = null;
                        }

                        currentLocator++;
                    }

                    if(!locatorFound)
                        throw new
                            FileNotFoundException("(VirtualPC plugin): Cannot find parent file for differencing disk image");

                    {
                        parentImage          = new Vhd();
                        IFilter parentFilter =
                            new FiltersList().GetFilter(Path.Combine(imageFilter.GetParentFolder(), parentPath));

                        if(parentFilter == null)
                            throw new ImageNotSupportedException("(VirtualPC plugin): Cannot find parent image filter");
                        /*                            PluginBase plugins = new PluginBase();
                                                    plugins.RegisterAllPlugins();
                                                    if (!plugins.ImagePluginsList.TryGetValue(Name.ToLower(), out parentImage))
                                                        throw new SystemException("(VirtualPC plugin): Unable to open myself");*/

                        if(!parentImage.Identify(parentFilter))
                            throw new
                                ImageNotSupportedException("(VirtualPC plugin): Parent image is not a Virtual PC disk image");

                        if(!parentImage.Open(parentFilter))
                            throw new ImageNotSupportedException("(VirtualPC plugin): Cannot open parent disk image");

                        // While specification says that parent and child disk images should contain UUID relationship
                        // in reality it seems that old differencing disk images stored a parent UUID that, nonetheless
                        // the parent never stored itself. So the only real way to know that images are related is
                        // because the parent IS found and SAME SIZE. Ugly...
                        // More funny even, tested parent images show an empty host OS, and child images a correct one.
                        if(parentImage.Info.Sectors != imageInfo.Sectors)
                            throw new
                                ImageNotSupportedException("(VirtualPC plugin): Parent image is of different size");
                    }

                    return true;
                }
                case TYPE_DEPRECATED1:
                case TYPE_DEPRECATED2:
                case TYPE_DEPRECATED3:
                {
                    throw new
                        ImageNotSupportedException("(VirtualPC plugin): Deprecated image type found. Please submit a bug with an example image.");
                }
                default:
                {
                    throw new
                        ImageNotSupportedException($"(VirtualPC plugin): Unknown image type {thisFooter.DiskType} found. Please submit a bug with an example image.");
                }
            }
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            switch(thisFooter.DiskType)
            {
                case TYPE_DIFFERENCING:
                {
                    // Block number for BAT searching
                    uint blockNumber = (uint)Math.Floor(sectorAddress / (thisDynamic.BlockSize / 512.0));
                    // Sector number inside of block
                    uint sectorInBlock = (uint)(sectorAddress % (thisDynamic.BlockSize / 512));

                    if(blockAllocationTable[blockNumber] == 0xFFFFFFFF) return new byte[512];

                    byte[] bitmap = new byte[bitmapSize * 512];

                    // Offset of block in file
                    uint blockOffset = blockAllocationTable[blockNumber] * 512;

                    int bitmapByte = (int)Math.Floor((double)sectorInBlock / 8);
                    int bitmapBit  = (int)(sectorInBlock                   % 8);

                    Stream thisStream = thisFilter.GetDataForkStream();

                    thisStream.Seek(blockOffset, SeekOrigin.Begin);
                    thisStream.Read(bitmap, 0, (int)bitmapSize * 512);

                    byte mask  = (byte)(1 << (7 - bitmapBit));
                    bool dirty = (bitmap[bitmapByte] & mask) == mask;

                    /*
                    DicConsole.DebugWriteLine("VirtualPC plugin", "bitmapSize = {0}", bitmapSize);
                    DicConsole.DebugWriteLine("VirtualPC plugin", "blockNumber = {0}", blockNumber);
                    DicConsole.DebugWriteLine("VirtualPC plugin", "sectorInBlock = {0}", sectorInBlock);
                    DicConsole.DebugWriteLine("VirtualPC plugin", "blockOffset = {0}", blockOffset);
                    DicConsole.DebugWriteLine("VirtualPC plugin", "bitmapByte = {0}", bitmapByte);
                    DicConsole.DebugWriteLine("VirtualPC plugin", "bitmapBit = {0}", bitmapBit);
                    DicConsole.DebugWriteLine("VirtualPC plugin", "mask = 0x{0:X2}", mask);
                    DicConsole.DebugWriteLine("VirtualPC plugin", "dirty = 0x{0}", dirty);
                    DicConsole.DebugWriteLine("VirtualPC plugin", "bitmap = ");
                    PrintHex.PrintHexArray(bitmap, 64);
                    */

                    // Sector has been written, read from child image
                    if(!dirty) return parentImage.ReadSector(sectorAddress);
                    /* Too noisy
                        DicConsole.DebugWriteLine("VirtualPC plugin", "Sector {0} is dirty", sectorAddress);
                        */

                    byte[] data         = new byte[512];
                    uint   sectorOffset = blockAllocationTable[blockNumber] + bitmapSize + sectorInBlock;
                    thisStream          = thisFilter.GetDataForkStream();

                    thisStream.Seek(sectorOffset * 512, SeekOrigin.Begin);
                    thisStream.Read(data, 0, 512);

                    return data;

                    /* Too noisy
                    DicConsole.DebugWriteLine("VirtualPC plugin", "Sector {0} is clean", sectorAddress);
                    */

                    // Read sector from parent image
                }
                default: return ReadSectors(sectorAddress, 1);
            }
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            switch(thisFooter.DiskType)
            {
                case TYPE_FIXED:
                {
                    byte[] data       = new byte[512 * length];
                    Stream thisStream = thisFilter.GetDataForkStream();

                    thisStream.Seek((long)(sectorAddress * 512), SeekOrigin.Begin);
                    thisStream.Read(data, 0, (int)(512   * length));

                    return data;
                }
                // Contrary to Microsoft's specifications that tell us to check the bitmap
                // and in case of unused sector just return zeros, as blocks are allocated
                // as a whole, this would waste time and miss cache, so we read any sector
                // as long as it is in the block.
                case TYPE_DYNAMIC:
                {
                    // Block number for BAT searching
                    uint blockNumber = (uint)Math.Floor(sectorAddress / (thisDynamic.BlockSize / 512.0));
                    // Sector number inside of block
                    uint sectorInBlock = (uint)(sectorAddress % (thisDynamic.BlockSize / 512));
                    // How many sectors before reaching end of block
                    uint remainingInBlock = thisDynamic.BlockSize / 512 - sectorInBlock;

                    // Data that needs to be read from another block
                    byte[] suffix = null;

                    // How many sectors to read from this block
                    uint sectorsToReadHere;

                    // Asked to read more sectors than are remaining in block
                    if(length > remainingInBlock)
                    {
                        suffix            = ReadSectors(sectorAddress + remainingInBlock, length - remainingInBlock);
                        sectorsToReadHere = remainingInBlock;
                    }
                    else sectorsToReadHere = length;

                    // Offset of sector in file
                    uint sectorOffset = blockAllocationTable[blockNumber] + bitmapSize + sectorInBlock;
                    // Data that can be read in this block
                    byte[] prefix = new byte[sectorsToReadHere * 512];

                    // 0xFFFFFFFF means unallocated
                    if(blockAllocationTable[blockNumber] != 0xFFFFFFFF)
                    {
                        Stream thisStream = thisFilter.GetDataForkStream();
                        thisStream.Seek(sectorOffset         * 512, SeekOrigin.Begin);
                        thisStream.Read(prefix, 0, (int)(512 * sectorsToReadHere));
                    }
                    // If it is unallocated, just fill with zeroes
                    else Array.Clear(prefix, 0, prefix.Length);

                    // If we needed to read from another block, join all the data
                    if(suffix == null) return prefix;

                    byte[] data = new byte[512 * length];
                    Array.Copy(prefix, 0, data, 0,             prefix.Length);
                    Array.Copy(suffix, 0, data, prefix.Length, suffix.Length);
                    return data;
                }
                case TYPE_DIFFERENCING:
                {
                    // As on differencing images, each independent sector can be read from child or parent
                    // image, we must read sector one by one
                    byte[] fullData = new byte[512 * length];
                    for(ulong i = 0; i < length; i++)
                    {
                        byte[] oneSector = ReadSector(sectorAddress + i);
                        Array.Copy(oneSector, 0, fullData, (int)(i * 512), 512);
                    }

                    return fullData;
                }
                case TYPE_DEPRECATED1:
                case TYPE_DEPRECATED2:
                case TYPE_DEPRECATED3:
                {
                    throw new
                        ImageNotSupportedException("(VirtualPC plugin): Deprecated image type found. Please submit a bug with an example image.");
                }
                default:
                {
                    throw new
                        ImageNotSupportedException($"(VirtualPC plugin): Unknown image type {thisFooter.DiskType} found. Please submit a bug with an example image.");
                }
            }
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

        public IEnumerable<MediaTagType>  SupportedMediaTags  => new MediaTagType[] { };
        public IEnumerable<SectorTagType> SupportedSectorTags => new SectorTagType[] { };
        public IEnumerable<MediaType>     SupportedMediaTypes =>
            new[]
            {
                MediaType.GENERIC_HDD, MediaType.Unknown, MediaType.FlashDrive, MediaType.CompactFlash,
                MediaType.CompactFlashType2, MediaType.PCCardTypeI, MediaType.PCCardTypeII, MediaType.PCCardTypeIII,
                MediaType.PCCardTypeIV
            };
        // TODO: Support dynamic images
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".vhd"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(sectorSize != 512)
            {
                ErrorMessage = "Unsupported sector size";
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

            if(data.Length != 512)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress >= imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            writingStream.Seek((long)(0 + sectorAddress * 512), SeekOrigin.Begin);
            writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";
            return true;
        }

        // TODO: Implement dynamic
        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(data.Length % 512 != 0)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress + length > imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            writingStream.Seek((long)(0 + sectorAddress * 512), SeekOrigin.Begin);
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

            Version thisVersion = GetType().Assembly.GetName().Version;

            if(imageInfo.Cylinders == 0)
            {
                imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
                imageInfo.Heads           = 16;
                imageInfo.SectorsPerTrack = 63;

                while(imageInfo.Cylinders == 0)
                {
                    imageInfo.Heads--;

                    if(imageInfo.Heads == 0)
                    {
                        imageInfo.SectorsPerTrack--;
                        imageInfo.Heads = 16;
                    }

                    imageInfo.Cylinders = (uint)(imageInfo.Sectors / imageInfo.Heads / imageInfo.SectorsPerTrack);

                    if(imageInfo.Cylinders == 0 && imageInfo.Heads == 0 && imageInfo.SectorsPerTrack == 0) break;
                }
            }

            HardDiskFooter footer = new HardDiskFooter
            {
                Cookie    = IMAGE_COOKIE,
                Features  = FEATURES_RESERVED,
                Version   = VERSION1,
                Timestamp =
                    (uint)(DateTime.Now - new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                CreatorApplication = CREATOR_DISCIMAGECHEF,
                CreatorVersion     =
                    (uint)(((thisVersion.Major & 0xFF) << 24) + ((thisVersion.Minor   & 0xFF) << 16) +
                           ((thisVersion.Build & 0xFF) << 8)  + (thisVersion.Revision & 0xFF)),
                CreatorHostOs = DetectOS.GetRealPlatformID() == PlatformID.MacOSX ? CREATOR_MACINTOSH : CREATOR_WINDOWS,
                DiskType      = TYPE_FIXED,
                UniqueId      = Guid.NewGuid(),
                DiskGeometry  =
                    ((imageInfo.Cylinders      & 0xFFFF) << 16) + ((imageInfo.Heads & 0xFF) << 8) +
                    (imageInfo.SectorsPerTrack & 0xFF),
                OriginalSize = imageInfo.Sectors * 512,
                CurrentSize  = imageInfo.Sectors * 512
            };
            footer.Offset = footer.DiskType == TYPE_FIXED ? ulong.MaxValue : 512;

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            byte[] footerBytes                   = new byte[512];
            Array.Copy(BigEndianBitConverter.GetBytes(footer.Cookie),             0, footerBytes, 0x00, 8);
            Array.Copy(BigEndianBitConverter.GetBytes(footer.Features),           0, footerBytes, 0x08, 4);
            Array.Copy(BigEndianBitConverter.GetBytes(footer.Version),            0, footerBytes, 0x0C, 4);
            Array.Copy(BigEndianBitConverter.GetBytes(footer.Offset),             0, footerBytes, 0x10, 8);
            Array.Copy(BigEndianBitConverter.GetBytes(footer.Timestamp),          0, footerBytes, 0x18, 4);
            Array.Copy(BigEndianBitConverter.GetBytes(footer.CreatorApplication), 0, footerBytes, 0x1C, 4);
            Array.Copy(BigEndianBitConverter.GetBytes(footer.CreatorVersion),     0, footerBytes, 0x20, 4);
            Array.Copy(BigEndianBitConverter.GetBytes(footer.CreatorHostOs),      0, footerBytes, 0x24, 4);
            Array.Copy(BigEndianBitConverter.GetBytes(footer.OriginalSize),       0, footerBytes, 0x28, 8);
            Array.Copy(BigEndianBitConverter.GetBytes(footer.CurrentSize),        0, footerBytes, 0x30, 8);
            Array.Copy(BigEndianBitConverter.GetBytes(footer.DiskGeometry),       0, footerBytes, 0x38, 4);
            Array.Copy(BigEndianBitConverter.GetBytes(footer.DiskType),           0, footerBytes, 0x3C, 4);
            Array.Copy(footer.UniqueId.ToByteArray(),                             0, footerBytes, 0x44, 4);

            footer.Checksum = VhdChecksum(footerBytes);
            Array.Copy(BigEndianBitConverter.GetBytes(footer.Checksum), 0, footerBytes, 0x40, 4);

            writingStream.Seek((long)(footer.DiskType == TYPE_FIXED ? footer.OriginalSize : 0), SeekOrigin.Begin);
            writingStream.Write(footerBytes, 0, 512);

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
            if(cylinders > 0xFFFF)
            {
                ErrorMessage = "Too many cylinders.";
                return false;
            }

            if(heads > 0xFF)
            {
                ErrorMessage = "Too many heads.";
                return false;
            }

            if(sectorsPerTrack > 0xFF)
            {
                ErrorMessage = "Too many sectors per track.";
                return false;
            }

            imageInfo.SectorsPerTrack = sectorsPerTrack;
            imageInfo.Heads           = heads;
            imageInfo.Cylinders       = cylinders;

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

        static uint VhdChecksum(IEnumerable<byte> data)
        {
            uint checksum = data.Aggregate<byte, uint>(0, (current, b) => current + b);

            return ~checksum;
        }

        struct HardDiskFooter
        {
            /// <summary>
            ///     Offset 0x00, File magic number, <see cref="Vhd.IMAGE_COOKIE" />
            /// </summary>
            public ulong Cookie;
            /// <summary>
            ///     Offset 0x08, Specific feature support
            /// </summary>
            public uint Features;
            /// <summary>
            ///     Offset 0x0C, File format version
            /// </summary>
            public uint Version;
            /// <summary>
            ///     Offset 0x10, Offset from beginning of file to next structure
            /// </summary>
            public ulong Offset;
            /// <summary>
            ///     Offset 0x18, Creation date seconds since 2000/01/01 00:00:00 UTC
            /// </summary>
            public uint Timestamp;
            /// <summary>
            ///     Offset 0x1C, Application that created this disk image
            /// </summary>
            public uint CreatorApplication;
            /// <summary>
            ///     Offset 0x20, Version of the application that created this disk image
            /// </summary>
            public uint CreatorVersion;
            /// <summary>
            ///     Offset 0x24, Host operating system of the application that created this disk image
            /// </summary>
            public uint CreatorHostOs;
            /// <summary>
            ///     Offset 0x28, Original hard disk size, in bytes
            /// </summary>
            public ulong OriginalSize;
            /// <summary>
            ///     Offset 0x30, Current hard disk size, in bytes
            /// </summary>
            public ulong CurrentSize;
            /// <summary>
            ///     Offset 0x38, CHS geometry
            ///     Cylinder mask = 0xFFFF0000
            ///     Heads mask = 0x0000FF00
            ///     Sectors mask = 0x000000FF
            /// </summary>
            public uint DiskGeometry;
            /// <summary>
            ///     Offset 0x3C, Disk image type
            /// </summary>
            public uint DiskType;
            /// <summary>
            ///     Offset 0x40, Checksum for this structure
            /// </summary>
            public uint Checksum;
            /// <summary>
            ///     Offset 0x44, UUID, used to associate parent with differencing disk images
            /// </summary>
            public Guid UniqueId;
            /// <summary>
            ///     Offset 0x54, If set, system is saved, so compaction and expansion cannot be performed
            /// </summary>
            public byte SavedState;
            /// <summary>
            ///     Offset 0x55, 427 bytes reserved, should contain zeros.
            /// </summary>
            public byte[] Reserved;
        }

        struct ParentLocatorEntry
        {
            /// <summary>
            ///     Offset 0x00, Describes the platform specific type this entry belongs to
            /// </summary>
            public uint PlatformCode;
            /// <summary>
            ///     Offset 0x04, Describes the number of 512 bytes sectors used by this entry
            /// </summary>
            public uint PlatformDataSpace;
            /// <summary>
            ///     Offset 0x08, Describes this entry's size in bytes
            /// </summary>
            public uint PlatformDataLength;
            /// <summary>
            ///     Offset 0x0c, Reserved
            /// </summary>
            public uint Reserved;
            /// <summary>
            ///     Offset 0x10, Offset on disk image this entry resides on
            /// </summary>
            public ulong PlatformDataOffset;
        }

        struct DynamicDiskHeader
        {
            /// <summary>
            ///     Offset 0x00, Header magic, <see cref="Vhd.DYNAMIC_COOKIE" />
            /// </summary>
            public ulong Cookie;
            /// <summary>
            ///     Offset 0x08, Offset to next structure on disk image.
            ///     Currently unused, 0xFFFFFFFF
            /// </summary>
            public ulong DataOffset;
            /// <summary>
            ///     Offset 0x10, Offset of the Block Allocation Table (BAT)
            /// </summary>
            public ulong TableOffset;
            /// <summary>
            ///     Offset 0x18, Version of this header
            /// </summary>
            public uint HeaderVersion;
            /// <summary>
            ///     Offset 0x1C, Maximum entries present in the BAT
            /// </summary>
            public uint MaxTableEntries;
            /// <summary>
            ///     Offset 0x20, Size of a block in bytes
            ///     Should always be a power of two of 512
            /// </summary>
            public uint BlockSize;
            /// <summary>
            ///     Offset 0x24, Checksum of this header
            /// </summary>
            public uint Checksum;
            /// <summary>
            ///     Offset 0x28, UUID of parent disk image for differencing type
            /// </summary>
            public Guid ParentId;
            /// <summary>
            ///     Offset 0x38, Timestamp of parent disk image
            /// </summary>
            public uint ParentTimestamp;
            /// <summary>
            ///     Offset 0x3C, Reserved
            /// </summary>
            public uint Reserved;
            /// <summary>
            ///     Offset 0x40, 512 bytes UTF-16 of parent disk image filename
            /// </summary>
            public string ParentName;
            /// <summary>
            ///     Offset 0x240, Parent disk image locator entry, <see cref="ParentLocatorEntry" />
            /// </summary>
            public ParentLocatorEntry[] LocatorEntries;
            /// <summary>
            ///     Offset 0x300, 256 reserved bytes
            /// </summary>
            public byte[] Reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BatSector
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public uint[] blockPointer;
        }
    }
}