/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : VHD.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Disc image plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Manages Connectix and Microsoft Virtual PC disk images.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2015 Claunia.com
****************************************************************************/
//$Id$
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiscImageChef.ImagePlugins
{
    /// <summary>
    /// Supports Connectix/Microsoft Virtual PC hard disk image format
    /// Until Virtual PC 5 there existed no format, and the hard disk image was
    /// merely a sector by sector (RAW) image with a resource fork giving
    /// information to Virtual PC itself.
    /// </summary>
    public class VHD : ImagePlugin
    {
        #region Internal Structures
        struct HardDiskFooter
        {
            /// <summary>
            /// Offset 0x00, File magic number, <see cref="ImageCookie"/> 
            /// </summary>
            public UInt64 cookie;
            /// <summary>
            /// Offset 0x08, Specific feature support
            /// </summary>
            public UInt32 features;
            /// <summary>
            /// Offset 0x0C, File format version
            /// </summary>
            public UInt32 version;
            /// <summary>
            /// Offset 0x10, Offset from beginning of file to next structure
            /// </summary>
            public UInt64 offset;
            /// <summary>
            /// Offset 0x18, Creation date seconds since 2000/01/01 00:00:00 UTC
            /// </summary>
            public UInt32 timestamp;
            /// <summary>
            /// Offset 0x1C, Application that created this disk image
            /// </summary>
            public UInt32 creatorApplication;
            /// <summary>
            /// Offset 0x20, Version of the application that created this disk image
            /// </summary>
            public UInt32 creatorVersion;
            /// <summary>
            /// Offset 0x24, Host operating system of the application that created this disk image
            /// </summary>
            public UInt32 creatorHostOS;
            /// <summary>
            /// Offset 0x28, Original hard disk size, in bytes
            /// </summary>
            public UInt64 originalSize;
            /// <summary>
            /// Offset 0x30, Current hard disk size, in bytes
            /// </summary>
            public UInt64 currentSize;
            /// <summary>
            /// Offset 0x38, CHS geometry
            /// Cylinder mask = 0xFFFF0000
            /// Heads mask = 0x0000FF00
            /// Sectors mask = 0x000000FF
            /// </summary>
            public UInt32 diskGeometry;
            /// <summary>
            /// Offset 0x3C, Disk image type
            /// </summary>
            public UInt32 diskType;
            /// <summary>
            /// Offset 0x40, Checksum for this structure
            /// </summary>
            public UInt32 checksum;
            /// <summary>
            /// Offset 0x44, UUID, used to associate parent with differencing disk images
            /// </summary>
            public Guid uniqueId;
            /// <summary>
            /// Offset 0x54, If set, system is saved, so compaction and expansion cannot be performed
            /// </summary>
            public byte savedState;
            /// <summary>
            /// Offset 0x55, 427 bytes reserved, should contain zeros.
            /// </summary>
            public byte[] reserved;
        }

        struct ParentLocatorEntry
        {
            /// <summary>
            /// Describes the platform specific type this entry belongs to
            /// </summary>
            public UInt32 platformCode;
            /// <summary>
            /// Describes the number of 512 bytes sectors used by this entry
            /// </summary>
            public UInt32 platformDataSpace;
            /// <summary>
            /// Describes this entry's size in bytes
            /// </summary>
            public UInt32 platformDataLength;
            /// <summary>
            /// Reserved
            /// </summary>
            public UInt32 reserved;
            /// <summary>
            /// Offset on disk image this entry resides on
            /// </summary>
            public UInt64 platformDataOffset;
        }

        struct DynamicDiskHeader
        {
            /// <summary>
            /// Header magic, <see cref="DynamicCookie"/> 
            /// </summary>
            public UInt64 cookie;
            /// <summary>
            /// Offset to next structure on disk image.
            /// Currently unused, 0xFFFFFFFF
            /// </summary>
            public UInt64 dataOffset;
            /// <summary>
            /// Offset of the Block Allocation Table (BAT)
            /// </summary>
            public UInt64 tableOffset;
            /// <summary>
            /// Version of this header
            /// </summary>
            public UInt32 headerVersion;
            /// <summary>
            /// Maximum entries present in the BAT
            /// </summary>
            public UInt32 maxTableEntries;
            /// <summary>
            /// Size of a block in bytes
            /// Should always be a power of two of 512
            /// </summary>
            public UInt32 blockSize;
            /// <summary>
            /// Checksum of this header
            /// </summary>
            public UInt32 checksum;
            /// <summary>
            /// UUID of parent disk image for differencing type
            /// </summary>
            public Guid parentID;
            /// <summary>
            /// Timestamp of parent disk image
            /// </summary>
            public UInt32 parentTimestamp;
            /// <summary>
            /// Reserved
            /// </summary>
            public UInt32 reserved;
            /// <summary>
            /// 512 bytes UTF-16 of paren disk image filename
            /// </summary>
            public string parentName;
            /// <summary>
            /// Parent disk image locator entry, <see cref="ParentLocatorEntry"/> 
            /// </summary>
            public ParentLocatorEntry locatorEntry1;
            /// <summary>
            /// Parent disk image locator entry, <see cref="ParentLocatorEntry"/> 
            /// </summary>
            public ParentLocatorEntry locatorEntry2;
            /// <summary>
            /// Parent disk image locator entry, <see cref="ParentLocatorEntry"/> 
            /// </summary>
            public ParentLocatorEntry locatorEntry3;
            /// <summary>
            /// Parent disk image locator entry, <see cref="ParentLocatorEntry"/> 
            /// </summary>
            public ParentLocatorEntry locatorEntry4;
            /// <summary>
            /// Parent disk image locator entry, <see cref="ParentLocatorEntry"/> 
            /// </summary>
            public ParentLocatorEntry locatorEntry5;
            /// <summary>
            /// Parent disk image locator entry, <see cref="ParentLocatorEntry"/> 
            /// </summary>
            public ParentLocatorEntry locatorEntry6;
            /// <summary>
            /// Parent disk image locator entry, <see cref="ParentLocatorEntry"/> 
            /// </summary>
            public ParentLocatorEntry locatorEntry7;
            /// <summary>
            /// Parent disk image locator entry, <see cref="ParentLocatorEntry"/> 
            /// </summary>
            public ParentLocatorEntry locatorEntry8;
            /// <summary>
            /// 256 reserved bytes
            /// </summary>
            public byte[] reserved2;
        }
        #endregion

        #region Internal Constants
        /// <summary>
        /// File magic number, "conectix"
        /// </summary>
        const UInt64 ImageCookie = 0x636F6E6563746978;
        /// <summary>
        /// Dynamic disk header magic, "cxsparse"
        /// </summary>
        const UInt64 DynamicCookie = 0x6378737061727365;

        /// <summary>
        /// Disk image is candidate for deletion on shutdown
        /// </summary>
        const UInt32 FeaturesTemporary = 0x00000001;
        /// <summary>
        /// Unknown, set from Virtual PC for Mac 7 onwards
        /// </summary>
        const UInt32 FeaturesReserved = 0x00000002;
        /// <summary>
        /// Unknown
        /// </summary>
        const UInt32 FeaturesUnknown = 0x00000100;

        /// <summary>
        /// Only known version
        /// </summary>
        const UInt32 Version1 = 0x00010000;

        /// <summary>
        /// Created by Virtual PC, "vpc "
        /// </summary>
        const UInt32 CreatorVirtualPC = 0x76706320;
        /// <summary>
        /// Created by Virtual Server, "vs  "
        /// </summary>
        const UInt32 CreatorVirtualServer = 0x76732020;
        /// <summary>
        /// Created by QEMU, "qemu"
        /// </summary>
        const UInt32 CreatorQEMU = 0x71656D75;
        /// <summary>
        /// Created by VirtualBox, "vbox"
        /// </summary>
        const UInt32 CreatorVirtualBox = 0x76626F78;

        /// <summary>
        /// Disk image created by Virtual Server 2004
        /// </summary>
        const UInt32 VersionVirtualServer2004 = 0x00010000;
        /// <summary>
        /// Disk image created by Virtual PC 2004
        /// </summary>
        const UInt32 VersionVirtualPC2004 = 0x00050000;
        /// <summary>
        /// Disk image created by Virtual PC 2007
        /// </summary>
        const UInt32 VersionVirtualPC2007 = 0x00050003;
        /// <summary>
        /// Disk image created by Virtual PC for Mac 5, 6 or 7
        /// </summary>
        const UInt32 VersionVirtualPCMac = 0x00040000;

        /// <summary>
        /// Disk image created in Windows, "Wi2k"
        /// </summary>
        const UInt32 CreatorWindows = 0x5769326B;
        /// <summary>
        /// Disk image created in Macintosh, "Mac "
        /// </summary>
        const UInt32 CreatorMacintosh = 0x4D616320;
        /// <summary>
        /// Seen in Virtual PC for Mac for dynamic and fixed images
        /// </summary>
        const UInt32 CreatorMacintoshOld = 0x00000000;

        /// <summary>
        /// Disk image type is none, useless?
        /// </summary>
        const UInt32 typeNone = 0;
        /// <summary>
        /// Deprecated disk image type
        /// </summary>
        const UInt32 typeDeprecated1 = 1;
        /// <summary>
        /// Fixed disk image type
        /// </summary>
        const UInt32 typeFixed = 2;
        /// <summary>
        /// Dynamic disk image type
        /// </summary>
        const UInt32 typeDynamic = 3;
        /// <summary>
        /// Differencing disk image type
        /// </summary>
        const UInt32 typeDifferencing = 4;
        /// <summary>
        /// Deprecated disk image type
        /// </summary>
        const UInt32 typeDeprecated2 = 5;
        /// <summary>
        /// Deprecated disk image type
        /// </summary>
        const UInt32 typeDeprecated3 = 6;

        /// <summary>
        /// Means platform locator is unused
        /// </summary>
        const UInt32 platformCodeUnused = 0x00000000;
        /// <summary>
        /// Stores a relative path string for Windows, unknown locale used, deprecated, "Wi2r"
        /// </summary>
        const UInt32 platformCodeWindowsRelative = 0x57693272;
        /// <summary>
        /// Stores an absolute path string for Windows, unknown locale used, deprecated, "Wi2k"
        /// </summary>
        const UInt32 platformCodeWindowsAbsolute = 0x5769326B;
        /// <summary>
        /// Stores a relative path string for Windows in UTF-16, "W2ru"
        /// </summary>
        const UInt32 platformCodeWindowsRelativeU = 0x57327275;
        /// <summary>
        /// Stores an absolute path string for Windows in UTF-16, "W2ku"
        /// </summary>
        const UInt32 platformCodeWindowsAbsoluteU = 0x57326B75;
        /// <summary>
        /// Stores a Mac OS alias as a blob, "Mac "
        /// </summary>
        const UInt32 platformCodeMacintoshAlias = 0x4D616320;
        /// <summary>
        /// Stores a Mac OS X URI (RFC-2396) absolute path in UTF-8, "MacX"
        /// </summary>
        const UInt32 platformCodeMacintoshURI = 0x4D616358;
        #endregion

        #region Internal variables
        HardDiskFooter thisFooter;
        HardDiskFooter parentFooter;
        DynamicDiskHeader thisDynamic;
        DynamicDiskHeader parentDynamic;
        DateTime thisDateTime;
        DateTime parentDateTime;
        string thisPath;
        string parentPath;
        #endregion

        public VHD(PluginBase Core)
        {
            Name = "VirtualPC";
            PluginUUID = new Guid("8014d88f-64cd-4484-9441-7635c632958a");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableDiskTags = new List<DiskTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageVersion = null;
            ImageInfo.imageApplication = null;
            ImageInfo.imageApplicationVersion = null;
            ImageInfo.imageCreator = null;
            ImageInfo.imageComments = null;
            ImageInfo.diskManufacturer = null;
            ImageInfo.diskModel = null;
            ImageInfo.diskSerialNumber = null;
            ImageInfo.diskBarcode = null;
            ImageInfo.diskPartNumber = null;
            ImageInfo.diskSequence = 0;
            ImageInfo.lastDiskSequence = 0;
            ImageInfo.driveManufacturer = null;
            ImageInfo.driveModel = null;
            ImageInfo.driveSerialNumber = null;
        }

        #region public methods
        public override bool IdentifyImage(string imagePath)
        {
            FileStream imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            UInt64 headerCookie;
            UInt64 footerCookie;

            byte[] headerCookieBytes = new byte[8];
            byte[] footerCookieBytes = new byte[8];

            if ((imageStream.Length % 2) == 0)
                imageStream.Seek(-512, SeekOrigin.End);
            else
                imageStream.Seek(-511, SeekOrigin.End);

            imageStream.Read(footerCookieBytes, 0, 8);
            imageStream.Seek(0, SeekOrigin.Begin);
            imageStream.Read(headerCookieBytes, 0, 8);

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            headerCookie = BigEndianBitConverter.ToUInt64(headerCookieBytes, 0);
            footerCookie = BigEndianBitConverter.ToUInt64(footerCookieBytes, 0);

            return (headerCookie == ImageCookie || footerCookie == ImageCookie);
        }

        public override bool OpenImage(string imagePath)
        {
            FileStream imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            byte[] header = new byte[512];
            byte[] footer;

            imageStream.Seek(0, SeekOrigin.Begin);
            imageStream.Read(header, 0, 512);

            if ((imageStream.Length % 2) == 0)
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

            UInt32 headerChecksum = BigEndianBitConverter.ToUInt32(header, 0x40);
            UInt32 footerChecksum = BigEndianBitConverter.ToUInt32(footer, 0x40);
            UInt64 headerCookie = BigEndianBitConverter.ToUInt64(header, 0);
            UInt64 footerCookie = BigEndianBitConverter.ToUInt64(footer, 0);

            header[0x40] = 0;
            header[0x41] = 0;
            header[0x42] = 0;
            header[0x43] = 0;
            footer[0x40] = 0;
            footer[0x41] = 0;
            footer[0x42] = 0;
            footer[0x43] = 0;

            UInt32 headerCalculatedChecksum = VHDChecksum(header);
            UInt32 footerCalculatedChecksum = VHDChecksum(footer);

            if (MainClass.isDebug)
            {
                Console.WriteLine("DEBUG (VirtualPC plugin): Header checksum = 0x{0:X8}, calculated = 0x{1:X8}", headerChecksum, headerCalculatedChecksum);
                Console.WriteLine("DEBUG (VirtualPC plugin): Header checksum = 0x{0:X8}, calculated = 0x{1:X8}", footerChecksum, footerCalculatedChecksum);
            }

            byte[] usableHeader;
            UInt32 usableChecksum;

            if (headerCookie == ImageCookie && headerChecksum == headerCalculatedChecksum)
            {
                usableHeader = header;
                usableChecksum = headerChecksum;
            }
            else if (footerCookie == ImageCookie && footerChecksum == footerCalculatedChecksum)
            {
                usableHeader = footer;
                usableChecksum = footerChecksum;
            }
            else
                throw new ImageNotSupportedException("(VirtualPC plugin): Both header and footer are corrupt, image cannot be opened.");

            thisFooter = new HardDiskFooter();
            thisFooter.cookie = BigEndianBitConverter.ToUInt64(usableHeader, 0x00);
            thisFooter.features = BigEndianBitConverter.ToUInt32(usableHeader, 0x08);
            thisFooter.version = BigEndianBitConverter.ToUInt32(usableHeader, 0x0C);
            thisFooter.offset = BigEndianBitConverter.ToUInt64(usableHeader, 0x10);
            thisFooter.timestamp = BigEndianBitConverter.ToUInt32(usableHeader, 0x18);
            thisFooter.creatorApplication = BigEndianBitConverter.ToUInt32(usableHeader, 0x1C);
            thisFooter.creatorVersion = BigEndianBitConverter.ToUInt32(usableHeader, 0x20);
            thisFooter.creatorHostOS = BigEndianBitConverter.ToUInt32(usableHeader, 0x24);
            thisFooter.originalSize = BigEndianBitConverter.ToUInt64(usableHeader, 0x28);
            thisFooter.currentSize = BigEndianBitConverter.ToUInt64(usableHeader, 0x30);
            thisFooter.diskGeometry = BigEndianBitConverter.ToUInt32(usableHeader, 0x38);
            thisFooter.diskType = BigEndianBitConverter.ToUInt32(usableHeader, 0x3C);
            thisFooter.checksum = usableChecksum;
            thisFooter.uniqueId = BigEndianBitConverter.ToGuid(usableHeader, 0x44);
            thisFooter.savedState = usableHeader[0x54];
            thisFooter.reserved = new byte[usableHeader.Length - 0x55];
            Array.Copy(usableHeader, 0x55, thisFooter.reserved, 0, (usableHeader.Length - 0x55));

            thisDateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            thisDateTime = thisDateTime.AddSeconds(thisFooter.timestamp);

            if (MainClass.isDebug)
            {
                Checksums.SHA1Context sha1Ctx = new Checksums.SHA1Context();
                sha1Ctx.Init();
                sha1Ctx.Update(thisFooter.reserved);

                Console.WriteLine("DEBUG (VirtualPC plugin): footer.cookie = 0x{0:X8}", thisFooter.cookie);
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.features = 0x{0:X8}", thisFooter.features);
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.version = 0x{0:X8}", thisFooter.version);
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.offset = {0}", thisFooter.offset);
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.timestamp = 0x{0:X8} ({1})", thisFooter.timestamp, thisDateTime);
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.creatorApplication = 0x{0:X8} (\"{1}\")", thisFooter.creatorApplication,
                    Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(thisFooter.creatorApplication)));
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.creatorVersion = 0x{0:X8}", thisFooter.creatorVersion);
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.creatorHostOS = 0x{0:X8} (\"{1}\")", thisFooter.creatorHostOS,
                    Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(thisFooter.creatorHostOS)));
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.originalSize = {0}", thisFooter.originalSize);
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.currentSize = {0}", thisFooter.currentSize);
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.diskGeometry = 0x{0:X8} (C/H/S: {1}/{2}/{3})", thisFooter.diskGeometry,
                    (thisFooter.diskGeometry&0xFFFF0000)>>16, (thisFooter.diskGeometry&0xFF00)>>8, (thisFooter.diskGeometry&0xFF));
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.diskType = 0x{0:X8}", thisFooter.diskType);
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.checksum = 0x{0:X8}", thisFooter.checksum);
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.uniqueId = {0}", thisFooter.uniqueId);
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.savedState = 0x{0:X2}", thisFooter.savedState);
                Console.WriteLine("DEBUG (VirtualPC plugin): footer.reserved's SHA1 = 0x{0}", sha1Ctx.End());
            }

            if(thisFooter.version == Version1)
                ImageInfo.imageVersion = "1.0";
            else
                throw new ImageNotSupportedException(String.Format("(VirtualPC plugin): Unknown image type {0} found. Please submit a bug with an example image.", thisFooter.diskType));

            switch (thisFooter.creatorApplication)
            {
                case CreatorQEMU:
                    {
                        ImageInfo.imageApplication = "QEMU";
                        // QEMU always set same version
                        ImageInfo.imageApplicationVersion = "Unknown";

                        break;
                    }
                case CreatorVirtualBox:
                    {
                        ImageInfo.imageApplicationVersion = String.Format("{0}.{1:D2}", (thisFooter.creatorVersion & 0xFFFF0000) >> 16, (thisFooter.creatorVersion & 0x0000FFFF));
                        switch (thisFooter.creatorHostOS)
                        {
                            case CreatorMacintosh:
                            case CreatorMacintoshOld:
                                ImageInfo.imageApplication = "VirtualBox for Mac";
                                break;
                            case CreatorWindows:
                                // VirtualBox uses Windows creator for any other OS
                                ImageInfo.imageApplication = "VirtualBox";
                                break;
                            default:
                                ImageInfo.imageApplication = String.Format("VirtualBox for unknown OS \"{0}\"", Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(thisFooter.creatorHostOS)));
                                break;
                        }
                        break;
                    }
                case CreatorVirtualServer:
                    {
                        ImageInfo.imageApplication = "Microsoft Virtual Server";
                        switch(thisFooter.creatorVersion)
                        {
                            case VersionVirtualServer2004:
                                ImageInfo.imageApplicationVersion = "2004";
                                break;
                            default:
                                ImageInfo.imageApplicationVersion = String.Format("Unknown version 0x{0:X8}", thisFooter.creatorVersion);
                                break;
                        }
                        break;
                    }
                case CreatorVirtualPC:
                    {
                        switch (thisFooter.creatorHostOS)
                        {
                            case CreatorMacintosh:
                            case CreatorMacintoshOld:
                                switch(thisFooter.creatorVersion)
                                {
                                    case VersionVirtualPCMac:
                                        ImageInfo.imageApplication = "Connectix Virtual PC";
                                        ImageInfo.imageApplicationVersion = "5, 6 or 7";
                                        break;
                                    default:
                                        ImageInfo.imageApplicationVersion = String.Format("Unknown version 0x{0:X8}", thisFooter.creatorVersion);
                                        break;
                                }
                                break;
                            case CreatorWindows:
                                switch(thisFooter.creatorVersion)
                                {
                                    case VersionVirtualPCMac:
                                        ImageInfo.imageApplication = "Connectix Virtual PC";
                                        ImageInfo.imageApplicationVersion = "5, 6 or 7";
                                        break;
                                    case VersionVirtualPC2004:
                                        ImageInfo.imageApplication = "Microsoft Virtual PC";
                                        ImageInfo.imageApplicationVersion = "2004";
                                        break;
                                    case VersionVirtualPC2007:
                                        ImageInfo.imageApplication = "Microsoft Virtual PC";
                                        ImageInfo.imageApplicationVersion = "2007";
                                        break;
                                    default:
                                        ImageInfo.imageApplicationVersion = String.Format("Unknown version 0x{0:X8}", thisFooter.creatorVersion);
                                        break;
                                }
                                break;
                            default:
                                ImageInfo.imageApplication = String.Format("Virtual PC for unknown OS \"{0}\"", Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(thisFooter.creatorHostOS)));
                                ImageInfo.imageApplicationVersion = String.Format("Unknown version 0x{0:X8}", thisFooter.creatorVersion);
                                break;
                        }
                        break;
                    }
                default:
                    {
                        ImageInfo.imageApplication = String.Format("Unknown application \"{0}\"", Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(thisFooter.creatorHostOS)));
                        ImageInfo.imageApplicationVersion = String.Format("Unknown version 0x{0:X8}", thisFooter.creatorVersion);
                        break;
                    }
            }

            thisPath = imagePath;
            ImageInfo.imageSize = thisFooter.currentSize;
            ImageInfo.sectors = thisFooter.currentSize/512;
            ImageInfo.sectorSize = 512;

            FileInfo fi = new FileInfo(imagePath);
            ImageInfo.imageCreationTime = fi.CreationTimeUtc;
            ImageInfo.imageLastModificationTime = thisDateTime;
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imagePath);
           
            switch (thisFooter.diskType)
            {
                case typeFixed:
                    {
                        // Nothing to do here, really.
                        imageStream.Close();
                        return true;
                    }
                case typeDynamic:
                    {
                        throw new NotImplementedException("(VirtualPC plugin): Dynamic disk images not yet supported");
                    }
                case typeDifferencing:
                    {
                        throw new NotImplementedException("(VirtualPC plugin): Differencing disk images not yet supported");
                    }
                case typeDeprecated1:
                case typeDeprecated2:
                case typeDeprecated3:
                    {
                        throw new ImageNotSupportedException("(VirtualPC plugin): Deprecated image type found. Please submit a bug with an example image.");
                    }
                default:
                    {
                        throw new ImageNotSupportedException(String.Format("(VirtualPC plugin): Unknown image type {0} found. Please submit a bug with an example image.", thisFooter.diskType));
                    }
            }
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
            switch(thisFooter.diskType)
            {
                case typeFixed:
                    return "Virtual PC fixed size disk image";
                case typeDynamic:
                    return "Virtual PC dynamic size disk image";
                case typeDifferencing:
                    return "Virtual PC differencing disk image";
                default:
                    return "Virtual PC disk image";
            }
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

        public override DiskType GetDiskType()
        {
            return DiskType.GENERIC_HDD;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            FileStream thisStream;

            switch (thisFooter.diskType)
            {
                case typeFixed:
                    {
                        byte[] data = new byte[512 * length];
                        thisStream = new FileStream(thisPath, FileMode.Open, FileAccess.Read);

                        thisStream.Seek((long)(sectorAddress * 512), SeekOrigin.Begin);
                        thisStream.Read(data, 0, (int)(512 * length));

                        thisStream.Close();
                        return data;
                    }
                case typeDynamic:
                    {
                        throw new NotImplementedException("(VirtualPC plugin): Dynamic disk images not yet supported");
                    }
                case typeDifferencing:
                    {
                        throw new NotImplementedException("(VirtualPC plugin): Differencing disk images not yet supported");
                    }
                case typeDeprecated1:
                case typeDeprecated2:
                case typeDeprecated3:
                    {
                        throw new ImageNotSupportedException("(VirtualPC plugin): Deprecated image type found. Please submit a bug with an example image.");
                    }
                default:
                    {
                        throw new ImageNotSupportedException(String.Format("(VirtualPC plugin): Unknown image type {0} found. Please submit a bug with an example image.", thisFooter.diskType));
                    }
            }
        }
        #endregion

        #region private methods
        UInt32 VHDChecksum(byte[] data)
        {
            UInt32 checksum = 0;
            foreach (byte b in data)
                checksum += b;
            return ~checksum;
        }
        #endregion

        #region Unsupported features
        public override string GetImageComments()
        {
            return null;
        }
        public override byte[] ReadDiskTag(DiskTagType tag)
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
        public override string GetDiskManufacturer()
        {
            return null;
        }
        public override string GetDiskModel()
        {
            return null;
        }
        public override string GetDiskSerialNumber()
        {
            return null;
        }
        public override string GetDiskBarcode()
        {
            return null;
        }
        public override string GetDiskPartNumber()
        {
            return null;
        }
        public override int GetDiskSequence()
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
        public override System.Collections.Generic.List<DiscImageChef.PartPlugins.Partition> GetPartitions()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        public override System.Collections.Generic.List<Track> GetTracks()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        public override System.Collections.Generic.List<Track> GetSessionTracks(Session session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        public override System.Collections.Generic.List<Track> GetSessionTracks(ushort session)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        public override System.Collections.Generic.List<Session> GetSessions()
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
        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();
            for (ulong i = 0; i < ImageInfo.sectors; i++)
                UnknownLBAs.Add(i);
            return null;
        }
        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }
        public override bool? VerifyDiskImage()
        {
            return null;
        }
        #endregion
    }
}

