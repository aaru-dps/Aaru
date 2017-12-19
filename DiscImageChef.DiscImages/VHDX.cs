// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : VHDX.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Microsoft Hyper-V disk images.
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
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
    public class VHDX : ImagePlugin
    {
        #region Internal Structures

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VHDXIdentifier
        {
            /// <summary>
            /// Signature, <see cref="VHDXSignature"/> 
            /// </summary>
            public ulong signature;
            /// <summary>
            /// UTF-16 string containing creator
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] creator;
        }

        struct VHDXHeader
        {
            /// <summary>
            /// Signature, <see cref="VHDXHeaderSig"/> 
            /// </summary>
            public uint signature;
            /// <summary>
            /// CRC-32C of whole 4096 bytes header with this field set to 0
            /// </summary>
            public uint checksum;
            /// <summary>
            /// Sequence number
            /// </summary>
            public ulong sequence;
            /// <summary>
            /// Unique identifier for file contents, must be changed on first write to metadata
            /// </summary>
            public Guid fileWriteGuid;
            /// <summary>
            /// Unique identifier for disk contents, must be changed on first write to metadata or data
            /// </summary>
            public Guid dataWriteGuid;
            /// <summary>
            /// Unique identifier for log entries
            /// </summary>
            public Guid logGuid;
            /// <summary>
            /// Version of log format
            /// </summary>
            public ushort logVersion;
            /// <summary>
            /// Version of VHDX format
            /// </summary>
            public ushort version;
            /// <summary>
            /// Length in bytes of the log
            /// </summary>
            public uint logLength;
            /// <summary>
            /// Offset from image start to the log
            /// </summary>
            public ulong logOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4016)]
            public byte[] reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VHDXRegionTableHeader
        {
            /// <summary>
            /// Signature, <see cref="VHDXRegionSig"/> 
            /// </summary>
            public uint signature;
            /// <summary>
            /// CRC-32C of whole 64Kb table with this field set to 0
            /// </summary>
            public uint checksum;
            /// <summary>
            /// How many entries follow this table
            /// </summary>
            public uint entries;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VHDXRegionTableEntry
        {
            /// <summary>
            /// Object identifier
            /// </summary>
            public Guid guid;
            /// <summary>
            /// Offset in image of the object
            /// </summary>
            public ulong offset;
            /// <summary>
            /// Length in bytes of the object
            /// </summary>
            public uint length;
            /// <summary>
            /// Flags
            /// </summary>
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VHDXMetadataTableHeader
        {
            /// <summary>
            /// Signature
            /// </summary>
            public ulong signature;
            /// <summary>
            /// Reserved
            /// </summary>
            public ushort reserved;
            /// <summary>
            /// How many entries are in the table
            /// </summary>
            public ushort entries;
            /// <summary>
            /// Reserved
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public uint[] reserved2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VHDXMetadataTableEntry
        {
            /// <summary>
            /// Metadata ID
            /// </summary>
            public Guid itemId;
            /// <summary>
            /// Offset relative to start of metadata region
            /// </summary>
            public uint offset;
            /// <summary>
            /// Length in bytes
            /// </summary>
            public uint length;
            /// <summary>
            /// Flags
            /// </summary>
            public uint flags;
            /// <summary>
            /// Reserved
            /// </summary>
            public uint reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VHDXFileParameters
        {
            /// <summary>
            /// Block size in bytes
            /// </summary>
            public uint blockSize;
            /// <summary>
            /// Flags
            /// </summary>
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VHDXParentLocatorHeader
        {
            /// <summary>
            /// Type of parent virtual disk
            /// </summary>
            public Guid locatorType;
            public ushort reserved;
            /// <summary>
            /// How many KVPs are in this parent locator
            /// </summary>
            public ushort keyValueCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VHDXParentLocatorEntry
        {
            /// <summary>
            /// Offset from metadata to key
            /// </summary>
            public uint keyOffset;
            /// <summary>
            /// Offset from metadata to value
            /// </summary>
            public uint valueOffset;
            /// <summary>
            /// Size of key
            /// </summary>
            public ushort keyLength;
            /// <summary>
            /// Size of value
            /// </summary>
            public ushort valueLength;
        }

        #endregion

        #region Internal Constants

        const ulong VHDXSignature = 0x656C696678646876;
        const uint VHDXHeaderSig = 0x64616568;
        const uint VHDXRegionSig = 0x69676572;
        const ulong VHDXMetadataSig = 0x617461646174656D;
        readonly Guid BATGuid = new Guid("2DC27766-F623-4200-9D64-115E9BFD4A08");
        readonly Guid MetadataGuid = new Guid("8B7CA206-4790-4B9A-B8FE-575F050F886E");
        readonly Guid FileParametersGuid = new Guid("CAA16737-FA36-4D43-B3B6-33F0AA44E76B");
        readonly Guid VirtualDiskSizeGuid = new Guid("2FA54224-CD1B-4876-B211-5DBED83BF4B8");
        readonly Guid Page83DataGuid = new Guid("BECA12AB-B2E6-4523-93EF-C309E000C746");
        readonly Guid LogicalSectorSizeGuid = new Guid("8141BF1D-A96F-4709-BA47-F233A8FAAB5F");
        readonly Guid PhysicalSectorSizeGuid = new Guid("CDA348C7-445D-4471-9CC9-E9885251C556");
        readonly Guid ParentLocatorGuid = new Guid("A8D35F2D-B30B-454D-ABF7-D3D84834AB0C");
        readonly Guid ParentTypeVHDXGuid = new Guid("B04AEFB7-D19E-4A81-B789-25B8E9445913");

        const string ParentLinkageKey = "parent_linkage";
        const string ParentLinkage2Key = "parent_linkage2";
        const string RelativePathKey = "relative_path";
        const string VolumePathKey = "volume_path";
        const string AbsoluteWin32PathKey = "absolute_win32_path";

        const uint RegionFlagsRequired = 0x01;

        const uint MetadataFlagsUser = 0x01;
        const uint MetadataFlagsVirtual = 0x02;
        const uint MetadataFlagsRequired = 0x04;

        const uint FileFlagsLeaveAllocated = 0x01;
        const uint FileFlagsHasParent = 0x02;

        /// <summary>Block has never been stored on this image, check parent</summary>
        const ulong PayloadBlockNotPresent = 0x00;
        /// <summary>Block was stored on this image and is removed, return whatever data you wish</summary>
        const ulong PayloadBlockUndefined = 0x01;
        /// <summary>Block is filled with zeroes</summary>
        const ulong PayloadBlockZero = 0x02;
        /// <summary>All sectors in this block were UNMAPed/TRIMed, return zeroes</summary>
        const ulong PayloadBlockUnmapper = 0x03;
        /// <summary>Block is present on this image</summary>
        const ulong PayloadBlockFullyPresent = 0x06;
        /// <summary>Block is present on image but there may be sectors present on parent image</summary>
        const ulong PayloadBlockPartiallyPresent = 0x07;

        const ulong SectorBitmapNotPresent = 0x00;
        const ulong SectorBitmapPresent = 0x06;

        const ulong BATFileOffsetMask = 0xFFFFFFFFFFFC0000;
        const ulong BATFlagsMask      = 0x7;
        const ulong BATReservedMask   = 0x3FFF8;

        #endregion

        #region Internal variables

        ulong VirtualDiskSize;
        Guid Page83Data;
        uint LogicalSectorSize;
        uint PhysicalSectorSize;

        VHDXIdentifier vhdxId;
        VHDXHeader vHdr;
        VHDXRegionTableHeader vRegHdr;
        VHDXRegionTableEntry[] vRegs;
        VHDXMetadataTableHeader vMetHdr;
        VHDXMetadataTableEntry[] vMets;
        VHDXFileParameters vFileParms;
        VHDXParentLocatorHeader vParHdr;
        VHDXParentLocatorEntry[] vPars;

        long batOffset;
        long metadataOffset;

        long chunkRatio;
        ulong dataBlocks;

        ulong[] blockAllocationTable;
        ulong[] sectorBitmapPointers;
        byte[] sectorBitmap;
        ImagePlugin parentImage;
        bool hasParent;
        Stream imageStream;

        const int MaxCacheSize = 16777216;
        int maxBlockCache;
        int maxSectorCache;

        Dictionary<ulong, byte[]> sectorCache;
        Dictionary<ulong, byte[]> blockCache;

        #endregion

        public VHDX()
        {
            Name = "Microsoft VHDX";
            PluginUUID = new Guid("536B141B-D09C-4799-AB70-34631286EB9D");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageVersion = null;
            ImageInfo.imageApplication = null;
            ImageInfo.imageApplicationVersion = null;
            ImageInfo.imageCreator = null;
            ImageInfo.imageComments = null;
            ImageInfo.mediaManufacturer = null;
            ImageInfo.mediaModel = null;
            ImageInfo.mediaSerialNumber = null;
            ImageInfo.mediaBarcode = null;
            ImageInfo.mediaPartNumber = null;
            ImageInfo.mediaSequence = 0;
            ImageInfo.lastMediaSequence = 0;
            ImageInfo.driveManufacturer = null;
            ImageInfo.driveModel = null;
            ImageInfo.driveSerialNumber = null;
            ImageInfo.driveFirmwareRevision = null;
        }

        #region public methods

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;

            byte[] vhdxId_b = new byte[Marshal.SizeOf(vhdxId)];
            stream.Read(vhdxId_b, 0, Marshal.SizeOf(vhdxId));
            vhdxId = new VHDXIdentifier();
            IntPtr idPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vhdxId));
            Marshal.Copy(vhdxId_b, 0, idPtr, Marshal.SizeOf(vhdxId));
            vhdxId = (VHDXIdentifier)Marshal.PtrToStructure(idPtr, typeof(VHDXIdentifier));
            Marshal.FreeHGlobal(idPtr);

            return vhdxId.signature == VHDXSignature;
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512)
                return false;

            byte[] vhdxId_b = new byte[Marshal.SizeOf(vhdxId)];
            stream.Read(vhdxId_b, 0, Marshal.SizeOf(vhdxId));
            vhdxId = new VHDXIdentifier();
            IntPtr idPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vhdxId));
            Marshal.Copy(vhdxId_b, 0, idPtr, Marshal.SizeOf(vhdxId));
            vhdxId = (VHDXIdentifier)Marshal.PtrToStructure(idPtr, typeof(VHDXIdentifier));
            Marshal.FreeHGlobal(idPtr);

            if(vhdxId.signature != VHDXSignature)
               return false;

            ImageInfo.imageApplication = Encoding.Unicode.GetString(vhdxId.creator);

            stream.Seek(64 * 1024, SeekOrigin.Begin);
            byte[] vHdr_b = new byte[Marshal.SizeOf(vHdr)];
            stream.Read(vHdr_b, 0, Marshal.SizeOf(vHdr));
            vHdr = new VHDXHeader();
            IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vHdr));
            Marshal.Copy(vHdr_b, 0, headerPtr, Marshal.SizeOf(vHdr));
            vHdr = (VHDXHeader)Marshal.PtrToStructure(headerPtr, typeof(VHDXHeader));
            Marshal.FreeHGlobal(headerPtr);

            if(vHdr.signature != VHDXHeaderSig)
            {
                stream.Seek(128 * 1024, SeekOrigin.Begin);
                vHdr_b = new byte[Marshal.SizeOf(vHdr)];
                stream.Read(vHdr_b, 0, Marshal.SizeOf(vHdr));
                vHdr = new VHDXHeader();
                headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vHdr));
                Marshal.Copy(vHdr_b, 0, headerPtr, Marshal.SizeOf(vHdr));
                vHdr = (VHDXHeader)Marshal.PtrToStructure(headerPtr, typeof(VHDXHeader));
                Marshal.FreeHGlobal(headerPtr);

                if(vHdr.signature != VHDXHeaderSig)
                    throw new ImageNotSupportedException("VHDX header not found");
            }

            stream.Seek(192 * 1024, SeekOrigin.Begin);
            byte[] vRegTable_b = new byte[Marshal.SizeOf(vRegHdr)];
            stream.Read(vRegTable_b, 0, Marshal.SizeOf(vRegHdr));
            vRegHdr = new VHDXRegionTableHeader();
            IntPtr vRegTabPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vRegHdr));
            Marshal.Copy(vRegTable_b, 0, vRegTabPtr, Marshal.SizeOf(vRegHdr));
            vRegHdr = (VHDXRegionTableHeader)Marshal.PtrToStructure(vRegTabPtr, typeof(VHDXRegionTableHeader));
            Marshal.FreeHGlobal(vRegTabPtr);

            if(vRegHdr.signature != VHDXRegionSig)
            {
                stream.Seek(256 * 1024, SeekOrigin.Begin);
                vRegTable_b = new byte[Marshal.SizeOf(vRegHdr)];
                stream.Read(vRegTable_b, 0, Marshal.SizeOf(vRegHdr));
                vRegHdr = new VHDXRegionTableHeader();
                vRegTabPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vRegHdr));
                Marshal.Copy(vRegTable_b, 0, vRegTabPtr, Marshal.SizeOf(vRegHdr));
                vRegHdr = (VHDXRegionTableHeader)Marshal.PtrToStructure(vRegTabPtr, typeof(VHDXRegionTableHeader));
                Marshal.FreeHGlobal(vRegTabPtr);

                if(vRegHdr.signature != VHDXRegionSig)
                    throw new ImageNotSupportedException("VHDX region table not found");
            }

            vRegs = new VHDXRegionTableEntry[vRegHdr.entries];
            for(int i = 0; i < vRegs.Length; i++)
            {
                byte[] vReg_b = new byte[Marshal.SizeOf(vRegs[i])];
                stream.Read(vReg_b, 0, Marshal.SizeOf(vRegs[i]));
                vRegs[i] = new VHDXRegionTableEntry();
                IntPtr vRegPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vRegs[i]));
                Marshal.Copy(vReg_b, 0, vRegPtr, Marshal.SizeOf(vRegs[i]));
                vRegs[i] = (VHDXRegionTableEntry)Marshal.PtrToStructure(vRegPtr, typeof(VHDXRegionTableEntry));
                Marshal.FreeHGlobal(vRegPtr);

                if(vRegs[i].guid == BATGuid)
                    batOffset = (long)vRegs[i].offset;
                else if(vRegs[i].guid == MetadataGuid)
                    metadataOffset = (long)vRegs[i].offset;
                else if((vRegs[i].flags & RegionFlagsRequired) == RegionFlagsRequired)
                    throw new ImageNotSupportedException(string.Format("Found unsupported and required region Guid {0}, not proceeding with image.", vRegs[i].guid));
            }

            if(batOffset == 0)
                throw new Exception("BAT not found, cannot continue.");

            if(metadataOffset == 0)
                throw new Exception("Metadata not found, cannot continue.");

            uint fileParamsOff = 0, vdSizeOff = 0, p83Off = 0, logOff = 0, physOff = 0, parentOff = 0;

            stream.Seek(metadataOffset, SeekOrigin.Begin);
            byte[] metTable_b = new byte[Marshal.SizeOf(vMetHdr)];
            stream.Read(metTable_b, 0, Marshal.SizeOf(vMetHdr));
            vMetHdr = new VHDXMetadataTableHeader();
            IntPtr metTablePtr = Marshal.AllocHGlobal(Marshal.SizeOf(vMetHdr));
            Marshal.Copy(metTable_b, 0, metTablePtr, Marshal.SizeOf(vMetHdr));
            vMetHdr = (VHDXMetadataTableHeader)Marshal.PtrToStructure(metTablePtr, typeof(VHDXMetadataTableHeader));
            Marshal.FreeHGlobal(metTablePtr);

            vMets = new VHDXMetadataTableEntry[vMetHdr.entries];
            for(int i = 0; i < vMets.Length; i++)
            {
                byte[] vMet_b = new byte[Marshal.SizeOf(vMets[i])];
                stream.Read(vMet_b, 0, Marshal.SizeOf(vMets[i]));
                vMets[i] = new VHDXMetadataTableEntry();
                IntPtr vMetPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vMets[i]));
                Marshal.Copy(vMet_b, 0, vMetPtr, Marshal.SizeOf(vMets[i]));
                vMets[i] = (VHDXMetadataTableEntry)Marshal.PtrToStructure(vMetPtr, typeof(VHDXMetadataTableEntry));
                Marshal.FreeHGlobal(vMetPtr);

                if(vMets[i].itemId == FileParametersGuid)
                    fileParamsOff = vMets[i].offset;
                else if(vMets[i].itemId == VirtualDiskSizeGuid)
                    vdSizeOff = vMets[i].offset;
                else if(vMets[i].itemId == Page83DataGuid)
                    p83Off = vMets[i].offset;
                else if(vMets[i].itemId == LogicalSectorSizeGuid)
                    logOff = vMets[i].offset;
                else if(vMets[i].itemId == PhysicalSectorSizeGuid)
                    physOff = vMets[i].offset;
                else if(vMets[i].itemId == ParentLocatorGuid)
                    parentOff = vMets[i].offset;
                else if((vMets[i].flags & MetadataFlagsRequired) == MetadataFlagsRequired)
                    throw new ImageNotSupportedException(string.Format("Found unsupported and required metadata Guid {0}, not proceeding with image.", vMets[i].itemId));
            }

            byte[] tmp;

            if(fileParamsOff != 0)
            {
                stream.Seek(fileParamsOff + metadataOffset, SeekOrigin.Begin);
                tmp = new byte[8];
                stream.Read(tmp, 0, 8);
                vFileParms = new VHDXFileParameters();
                vFileParms.blockSize = BitConverter.ToUInt32(tmp, 0);
                vFileParms.flags = BitConverter.ToUInt32(tmp, 4);
            }
            else
                throw new Exception("File parameters not found.");

            if(vdSizeOff != 0)
            {
                stream.Seek(vdSizeOff + metadataOffset, SeekOrigin.Begin);
                tmp = new byte[8];
                stream.Read(tmp, 0, 8);
                VirtualDiskSize = BitConverter.ToUInt64(tmp, 0);
            }
            else
                throw new Exception("Virtual disk size not found.");

            if(p83Off != 0)
            {
                stream.Seek(p83Off + metadataOffset, SeekOrigin.Begin);
                tmp = new byte[16];
                stream.Read(tmp, 0, 16);
                Page83Data = new Guid(tmp);
            }

            if(logOff != 0)
            {
                stream.Seek(logOff + metadataOffset, SeekOrigin.Begin);
                tmp = new byte[4];
                stream.Read(tmp, 0, 4);
                LogicalSectorSize = BitConverter.ToUInt32(tmp, 0);
            }
            else
                throw new Exception("Logical sector size not found.");
            
            if(physOff != 0)
            {
                stream.Seek(physOff + metadataOffset, SeekOrigin.Begin);
                tmp = new byte[4];
                stream.Read(tmp, 0, 4);
                PhysicalSectorSize = BitConverter.ToUInt32(tmp, 0);
            }
            else
                throw new Exception("Physical sector size not found.");

            if(parentOff != 0 && (vFileParms.flags & FileFlagsHasParent) == FileFlagsHasParent)
            {
                stream.Seek(parentOff + metadataOffset, SeekOrigin.Begin);
                byte[] vParHdr_b = new byte[Marshal.SizeOf(vMetHdr)];
                stream.Read(vParHdr_b, 0, Marshal.SizeOf(vMetHdr));
                vParHdr = new VHDXParentLocatorHeader();
                IntPtr vParHdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vMetHdr));
                Marshal.Copy(vParHdr_b, 0, vParHdrPtr, Marshal.SizeOf(vMetHdr));
                vParHdr = (VHDXParentLocatorHeader)Marshal.PtrToStructure(vParHdrPtr, typeof(VHDXParentLocatorHeader));
                Marshal.FreeHGlobal(vParHdrPtr);

                if(vParHdr.locatorType != ParentTypeVHDXGuid)
                    throw new ImageNotSupportedException(string.Format("Found unsupported and required parent locator type {0}, not proceeding with image.", vParHdr.locatorType));

                vPars = new VHDXParentLocatorEntry[vParHdr.keyValueCount];
                for(int i = 0; i < vPars.Length; i++)
                {
                    byte[] vPar_b = new byte[Marshal.SizeOf(vPars[i])];
                    stream.Read(vPar_b, 0, Marshal.SizeOf(vPars[i]));
                    vPars[i] = new VHDXParentLocatorEntry();
                    IntPtr vParPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vPars[i]));
                    Marshal.Copy(vPar_b, 0, vParPtr, Marshal.SizeOf(vPars[i]));
                    vPars[i] = (VHDXParentLocatorEntry)Marshal.PtrToStructure(vParPtr, typeof(VHDXParentLocatorEntry));
                    Marshal.FreeHGlobal(vParPtr);

                }
            }
            else if((vFileParms.flags & FileFlagsHasParent) == FileFlagsHasParent)
                throw new Exception("Parent locator not found.");

            if((vFileParms.flags & FileFlagsHasParent) == FileFlagsHasParent && vParHdr.locatorType == ParentTypeVHDXGuid)
            {
                parentImage = new VHDX();
                bool parentWorks = false;
                Filter parentFilter;

                foreach(VHDXParentLocatorEntry parentEntry in vPars)
                {
                    stream.Seek(parentEntry.keyOffset + metadataOffset, SeekOrigin.Begin);
                    byte[] tmpKey = new byte[parentEntry.keyLength];
                    stream.Read(tmpKey, 0, tmpKey.Length);
                    string entryType = Encoding.Unicode.GetString(tmpKey);

                    if(string.Compare(entryType, RelativePathKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        stream.Seek(parentEntry.valueOffset + metadataOffset, SeekOrigin.Begin);
                        byte[] tmpVal = new byte[parentEntry.valueLength];
                        stream.Read(tmpVal, 0, tmpVal.Length);
                        string entryValue = Encoding.Unicode.GetString(tmpVal);

                        try
                        {
                            parentFilter = new FiltersList().GetFilter(Path.Combine(imageFilter.GetParentFolder(), entryValue));
                            if(parentFilter != null && parentImage.OpenImage(parentFilter))
                            {
                                parentWorks = true;
                                break;
                            }
                        }
                        catch { parentWorks = false; }

                        string relEntry = Path.Combine(Path.GetDirectoryName(imageFilter.GetPath()), entryValue);

                        try
                        {
                            parentFilter = new FiltersList().GetFilter(Path.Combine(imageFilter.GetParentFolder(), relEntry));
                            if(parentFilter != null && parentImage.OpenImage(parentFilter))
                            {
                                parentWorks = true;
                                break;
                            }
                        }
                        catch { continue; }
                    }
                    else if(string.Compare(entryType, VolumePathKey, StringComparison.OrdinalIgnoreCase) == 0 ||
                            string.Compare(entryType, AbsoluteWin32PathKey, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        stream.Seek(parentEntry.valueOffset + metadataOffset, SeekOrigin.Begin);
                        byte[] tmpVal = new byte[parentEntry.valueLength];
                        stream.Read(tmpVal, 0, tmpVal.Length);
                        string entryValue = Encoding.Unicode.GetString(tmpVal);

                        try
                        {
                            parentFilter = new FiltersList().GetFilter(Path.Combine(imageFilter.GetParentFolder(), entryValue));
                            if(parentFilter != null && parentImage.OpenImage(parentFilter))
                            {
                                parentWorks = true;
                                break;
                            }
                        }
                        catch { continue; }
                    }
                }

                if(!parentWorks)
                    throw new Exception("Image is differential but parent cannot be opened.");

                hasParent = true;
            }

            chunkRatio = (long)((Math.Pow(2, 23) * LogicalSectorSize) / vFileParms.blockSize);
            dataBlocks = VirtualDiskSize / vFileParms.blockSize;
            if((VirtualDiskSize % vFileParms.blockSize) > 0)
                dataBlocks++;

            long batEntries;
            if(hasParent)
            {
                long sectorBitmapBlocks = (long)dataBlocks / chunkRatio;
                if((dataBlocks % (ulong)chunkRatio) > 0)
                    sectorBitmapBlocks++;
                sectorBitmapPointers = new ulong[sectorBitmapBlocks];

                batEntries = sectorBitmapBlocks * (chunkRatio - 1);
            }
            else
                batEntries = (long)(dataBlocks + ((dataBlocks - 1) / (ulong)chunkRatio));

            DicConsole.DebugWriteLine("VHDX plugin", "Reading BAT");

            long readChunks = 0;
            blockAllocationTable = new ulong[dataBlocks];
            byte[] BAT_b = new byte[batEntries * 8];
            stream.Seek(batOffset, SeekOrigin.Begin);
            stream.Read(BAT_b, 0, BAT_b.Length);

            ulong skipSize = 0;
            for(ulong i = 0; i < dataBlocks; i++)
            {
                if(readChunks == chunkRatio)
                {
                    if(hasParent)
                        sectorBitmapPointers[skipSize / 8] = BitConverter.ToUInt64(BAT_b, (int)(i * 8 + skipSize));

                    readChunks = 0;
                    skipSize += 8;
                }
                else
                {
                    blockAllocationTable[i] = BitConverter.ToUInt64(BAT_b, (int)(i * 8 + skipSize));
                    readChunks++;
                }
            }

            if(hasParent)
            {
                DicConsole.DebugWriteLine("VHDX plugin", "Reading Sector Bitmap");

                MemoryStream sectorBmpMs = new MemoryStream();
                foreach(ulong pt in sectorBitmapPointers)
                {
                    if((pt & BATFlagsMask) == SectorBitmapNotPresent)
                    {
                        sectorBmpMs.Write(new byte[1048576], 0, 1048576);
                    }
                    else if((pt & BATFlagsMask) == SectorBitmapPresent)
                    {
                        stream.Seek((long)((pt & BATFileOffsetMask) * 1048576), SeekOrigin.Begin);
                        byte[] bmp = new byte[1048576];
                        stream.Read(bmp, 0, bmp.Length);
                        sectorBmpMs.Write(bmp, 0, bmp.Length);
                    }
                    else if((pt & BATFlagsMask) != 0)
                        throw new ImageNotSupportedException(string.Format("Unsupported sector bitmap block flags (0x{0:X16}) found, not proceeding.", pt & BATFlagsMask));
                }
                sectorBitmap = sectorBmpMs.ToArray();
                sectorBmpMs.Close();
            }

            maxBlockCache = (int)(MaxCacheSize / vFileParms.blockSize);
            maxSectorCache = (int)(MaxCacheSize / LogicalSectorSize);

            imageStream = stream;

            sectorCache = new Dictionary<ulong, byte[]>();
            blockCache = new Dictionary<ulong, byte[]>();

            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.sectorSize = LogicalSectorSize;
            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.mediaType = MediaType.GENERIC_HDD;
            ImageInfo.imageSize = VirtualDiskSize;
            ImageInfo.sectors = ImageInfo.imageSize / ImageInfo.sectorSize;
            ImageInfo.driveSerialNumber = Page83Data.ToString();

			// TODO: Separate image application from version, need several samples.

			ImageInfo.cylinders = (uint)((ImageInfo.sectors / 16) / 63);
			ImageInfo.heads = 16;
			ImageInfo.sectorsPerTrack = 63;

			return true;
        }

        bool CheckBitmap(ulong sectorAddress)
        {
            long index = (long)(sectorAddress / 8);
            int shift = (int)(sectorAddress % 8);
            byte val = (byte)(1 << shift);

            if(index > sectorBitmap.LongLength)
                return false;

            return ((sectorBitmap[index] & val) == val);
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
            return "VHDX";
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

        public override MediaType GetMediaType()
        {
            return MediaType.GENERIC_HDD;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

            byte[] sector;

            if(sectorCache.TryGetValue(sectorAddress, out sector))
                return sector;

            ulong index = (sectorAddress * LogicalSectorSize) / vFileParms.blockSize;
            ulong secOff = (sectorAddress * LogicalSectorSize) % vFileParms.blockSize;

            ulong blkPtr = blockAllocationTable[index];
            ulong blkFlags = blkPtr & BATFlagsMask;

            if((blkPtr & BATReservedMask) != 0)
                throw new ImageNotSupportedException(string.Format("Unknown flags (0x{0:X16}) set in block pointer", blkPtr & BATReservedMask));

            if((blkFlags & BATFlagsMask) == PayloadBlockNotPresent)
                return hasParent ? parentImage.ReadSector(sectorAddress) : new byte[LogicalSectorSize];

            if((blkFlags & BATFlagsMask) == PayloadBlockUndefined ||
               (blkFlags & BATFlagsMask) == PayloadBlockZero ||
               (blkFlags & BATFlagsMask) == PayloadBlockUnmapper)
                return new byte[LogicalSectorSize];

            bool partialBlock;
            partialBlock = !((blkFlags & BATFlagsMask) == PayloadBlockFullyPresent);
            partialBlock = (blkFlags & BATFlagsMask) == PayloadBlockPartiallyPresent;

            if(partialBlock && hasParent && !CheckBitmap(sectorAddress))
                return parentImage.ReadSector(sectorAddress);

            byte[] block;

            if(!blockCache.TryGetValue(blkPtr & BATFileOffsetMask, out block))
            {
                block = new byte[vFileParms.blockSize];
                imageStream.Seek((long)((blkPtr & BATFileOffsetMask)), SeekOrigin.Begin);
                imageStream.Read(block, 0, block.Length);

                if(blockCache.Count >= maxBlockCache)
                    blockCache.Clear();

                blockCache.Add(blkPtr & BATFileOffsetMask, block);
            }

            sector = new byte[LogicalSectorSize];
            Array.Copy(block, (int)secOff, sector, 0, sector.Length);

            if(sectorCache.Count >= maxSectorCache)
                sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than available ({1})", sectorAddress + length, ImageInfo.sectors));

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        #endregion

        #region private methods

        static uint VHDXChecksum(byte[] data)
        {
            uint checksum = 0;
            foreach(byte b in data)
                checksum += b;
            return ~checksum;
        }

        #endregion

        #region Unsupported features

        public override string GetImageComments()
        {
            return null;
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

        public override string GetMediaManufacturer()
        {
            return null;
        }

        public override string GetMediaModel()
        {
            return null;
        }

        public override string GetMediaSerialNumber()
        {
            return null;
        }

        public override string GetMediaBarcode()
        {
            return null;
        }

        public override string GetMediaPartNumber()
        {
            return null;
        }

        public override int GetMediaSequence()
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
            for(ulong i = 0; i < ImageInfo.sectors; i++)
                UnknownLBAs.Add(i);
            return null;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }

        #endregion
    }
}

