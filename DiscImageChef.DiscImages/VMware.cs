// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : VMware.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages VMware disk images.
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
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.ImagePlugins;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class VMware : ImagePlugin
    {
        #region Internal constants
        const uint VMwareExtentMagic = 0x564D444B;
        const uint VMwareCowMagic = 0x44574F43;

        const string VMTypeCustom = "custom";
        const string VMTypeMonoSparse = "monolithicSparse";
        const string VMTypeMonoFlat = "monolithicFlat";
        const string VMTypeSplitSparse = "twoGbMaxExtentSparse";
        const string VMTypeSplitFlat = "twoGbMaxExtentFlat";
        const string VMTypeFullDevice = "fullDevice";
        const string VMTypePartDevice = "partitionedDevice";
        const string VMFSTypeFlat = "vmfsPreallocated";
        const string VMFSTypeZero = "vmfsEagerZeroedThick";
        const string VMFSTypeThin = "vmfsThin";
        const string VMFSTypeSparse = "vmfsSparse";
        const string VMFSTypeRDM = "vmfsRDM";
        const string VMFSTypeRDMOld = "vmfsRawDeviceMap";
        const string VMFSTypeRDMP = "vmfsRDMP";
        const string VMFSTypeRDMPOld = "vmfsPassthroughRawDeviceMap";
        const string VMFSTypeRaw = "vmfsRaw";
        const string VMFSType = "vmfs";
        const string VMTypeStream = "streamOptimized";

        const string DDFMagic = "# Disk DescriptorFile";
        readonly byte[] DDFMagicBytes = { 0x23, 0x20, 0x44, 0x69, 0x73, 0x6B, 0x20, 0x44, 0x65, 0x73, 0x63, 0x72, 0x69, 0x70, 0x74, 0x6F, 0x72, 0x46, 0x69, 0x6C, 0x65 };

        const string VersionRegEx = "^\\s*version\\s*=\\s*(?<version>\\d+)$";
        const string CidRegEx = "^\\s*CID\\s*=\\s*(?<cid>[0123456789abcdef]{8})$";
        const string ParenCidRegEx = "^\\s*parentCID\\s*=\\s*(?<cid>[0123456789abcdef]{8})$";
        const string TypeRegEx = "^\\s*createType\\s*=\\s*\\\"(?<type>custom|monolithicSparse|monolithicFlat|twoGbMaxExtentSparse|twoGbMaxExtentFlat|fullDevice|partitionedDevice|vmfs|vmfsPreallocated|vmfsEagerZeroedThick|vmfsThin|vmfsSparse|vmfsRDM|vmfsRawDeviceMap|vmfsRDMP|vmfsPassthroughRawDeviceMap|vmfsRaw|streamOptimized)\\\"$";
        const string ExtentRegEx = "^\\s*(?<access>(RW|RDONLY|NOACCESS))\\s+(?<sectors>\\d+)\\s+(?<type>(FLAT|SPARSE|ZERO|VMFS|VMFSSPARSE|VMFSRDM|VMFSRAW))\\s+\\\"(?<filename>.+)\\\"(\\s*(?<offset>\\d+))?$";
        const string DDBTypeRegEx = "^\\s*ddb\\.adapterType\\s*=\\s*\\\"(?<type>ide|buslogic|lsilogic|legacyESX)\\\"$";
        const string DDBSectorsRegEx = "^\\s*ddb\\.geometry\\.sectors\\s*=\\s*\\\"(?<sectors>\\d+)\\\"$";
        const string DDBHeadsRegex = "^\\s*ddb\\.geometry\\.heads\\s*=\\s*\\\"(?<heads>\\d+)\\\"$";
        const string DDBCylindersRegEx = "^\\s*ddb\\.geometry\\.cylinders\\s*=\\s*\\\"(?<cylinders>\\d+)\\\"$";
        const string ParentRegEx = "^\\s*parentFileNameHint\\s*=\\s*\\\"(?<filename>.+)\\\"$";

        const uint FlagsValidNewLine = 0x01;
        const uint FlagsUseRedundantTable = 0x02;
        const uint FlagsZeroGrainGTE = 0x04;
        const uint FlagsCompression = 0x10000;
        const uint FlagsMarkers = 0x20000;

        const ushort CompressionNone = 0;
        const ushort CompressionDeflate = 1;
        #endregion

        #region Internal Structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VMwareExtentHeader
        {
            public uint magic;
            public uint version;
            public uint flags;
            public ulong capacity;
            public ulong grainSize;
            public ulong descriptorOffset;
            public ulong descriptorSize;
            public uint GTEsPerGT;
            public ulong rgdOffset;
            public ulong gdOffset;
            public ulong overhead;
            [MarshalAs(UnmanagedType.U1)]
            public bool uncleanShutdown;
            public byte singleEndLineChar;
            public byte nonEndLineChar;
            public byte doubleEndLineChar1;
            public byte doubleEndLineChar2;
            public ushort compression;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 433)]
            public byte[] padding;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VMwareCowHeader
        {
            public uint magic;
            public uint version;
            public uint flags;
            public uint sectors;
            public uint grainSize;
            public uint gdOffset;
            public uint numGDEntries;
            public uint freeSector;
            public uint cylinders;
            public uint heads;
            public uint spt;
            // It stats on cylinders, above, but, don't care
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024 - 12)]
            public byte[] parentFileName;
            public uint parentGeneration;
            public uint generation;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
            public byte[] name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] description;
            public uint savedGeneration;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] reserved;
            [MarshalAs(UnmanagedType.U1)]
            public bool uncleanShutdown;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 396)]
            public byte[] padding;
        }
        #endregion

        struct VMwareExtent
        {
            public string access;
            public uint sectors;
            public string type;
            public Filter filter;
            public string filename;
            public uint offset;
        }

        VMwareExtentHeader vmEHdr;
        VMwareCowHeader vmCHdr;

        Dictionary<ulong, byte[]> sectorCache;
        Dictionary<ulong, byte[]> grainCache;

        uint cid;
        uint parentCid;
        string imageType;
        uint version;
        Dictionary<ulong, VMwareExtent> extents;
        string parentName;

        const uint MaxCacheSize = 16777216;
        const uint sectorSize = 512;
        uint maxCachedSectors = MaxCacheSize / sectorSize;
        uint maxCachedGrains;

        ImagePlugin parentImage;
        bool hasParent;
        Filter gdFilter;
        uint[] gTable;

        ulong grainSize;

        public VMware()
        {
            Name = "VMware disk image";
            PluginUUID = new Guid("E314DE35-C103-48A3-AD36-990F68523C46");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = false;
            ImageInfo.imageHasSessions = false;
            ImageInfo.imageVersion = null;
            ImageInfo.imageApplication = "VMware";
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

        public override bool IdentifyImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            byte[] ddfMagic = new byte[0x15];

            if(stream.Length > Marshal.SizeOf(vmEHdr))
            {
                stream.Seek(0, SeekOrigin.Begin);
                byte[] vmEHdr_b = new byte[Marshal.SizeOf(vmEHdr)];
                stream.Read(vmEHdr_b, 0, Marshal.SizeOf(vmEHdr));
                vmEHdr = new VMwareExtentHeader();
                IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vmEHdr));
                Marshal.Copy(vmEHdr_b, 0, headerPtr, Marshal.SizeOf(vmEHdr));
                vmEHdr = (VMwareExtentHeader)Marshal.PtrToStructure(headerPtr, typeof(VMwareExtentHeader));
                Marshal.FreeHGlobal(headerPtr);

                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(ddfMagic, 0, 0x15);

                vmCHdr = new VMwareCowHeader();
                if(stream.Length > Marshal.SizeOf(vmCHdr))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    byte[] vmCHdr_b = new byte[Marshal.SizeOf(vmCHdr)];
                    stream.Read(vmCHdr_b, 0, Marshal.SizeOf(vmCHdr));
                    headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vmCHdr));
                    Marshal.Copy(vmCHdr_b, 0, headerPtr, Marshal.SizeOf(vmCHdr));
                    vmCHdr = (VMwareCowHeader)Marshal.PtrToStructure(headerPtr, typeof(VMwareCowHeader));
                    Marshal.FreeHGlobal(headerPtr);
                }

                return DDFMagicBytes.SequenceEqual(ddfMagic) || vmEHdr.magic == VMwareExtentMagic || vmCHdr.magic == VMwareCowMagic;
            }

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(ddfMagic, 0, 0x15);

            return DDFMagicBytes.SequenceEqual(ddfMagic);
        }

        public override bool OpenImage(Filter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            vmEHdr = new VMwareExtentHeader();
            vmCHdr = new VMwareCowHeader();
            bool embedded = false;

            if(stream.Length > Marshal.SizeOf(vmEHdr))
            {
                stream.Seek(0, SeekOrigin.Begin);
                byte[] vmEHdr_b = new byte[Marshal.SizeOf(vmEHdr)];
                stream.Read(vmEHdr_b, 0, Marshal.SizeOf(vmEHdr));
                IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vmEHdr));
                Marshal.Copy(vmEHdr_b, 0, headerPtr, Marshal.SizeOf(vmEHdr));
                vmEHdr = (VMwareExtentHeader)Marshal.PtrToStructure(headerPtr, typeof(VMwareExtentHeader));
                Marshal.FreeHGlobal(headerPtr);
            }

            if(stream.Length > Marshal.SizeOf(vmCHdr))
            {
                stream.Seek(0, SeekOrigin.Begin);
                byte[] vmCHdr_b = new byte[Marshal.SizeOf(vmCHdr)];
                stream.Read(vmCHdr_b, 0, Marshal.SizeOf(vmCHdr));
                IntPtr cowPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vmCHdr));
                Marshal.Copy(vmCHdr_b, 0, cowPtr, Marshal.SizeOf(vmCHdr));
                vmCHdr = (VMwareCowHeader)Marshal.PtrToStructure(cowPtr, typeof(VMwareCowHeader));
                Marshal.FreeHGlobal(cowPtr);
            }

            MemoryStream ddfStream = new MemoryStream();
            bool vmEHdrSet = false;
            bool cowD = false;

            if(vmEHdr.magic == VMwareExtentMagic)
            {
                vmEHdrSet = true;
                gdFilter = imageFilter;

                if(vmEHdr.descriptorOffset == 0 || vmEHdr.descriptorSize == 0)
                    throw new Exception("Please open VMDK descriptor.");

                byte[] ddfEmbed = new byte[vmEHdr.descriptorSize * sectorSize];

                stream.Seek((long)(vmEHdr.descriptorOffset * sectorSize), SeekOrigin.Begin);
                stream.Read(ddfEmbed, 0, ddfEmbed.Length);
                ddfStream.Write(ddfEmbed, 0, ddfEmbed.Length);

                embedded = true;
            }
            else if(vmCHdr.magic == VMwareCowMagic)
            {
                gdFilter = imageFilter;
                cowD = true;
            }
            else
            {
                byte[] ddfMagic = new byte[0x15];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(ddfMagic, 0, 0x15);

                if(!DDFMagicBytes.SequenceEqual(ddfMagic))
                    throw new Exception("Not a descriptor.");

                stream.Seek(0, SeekOrigin.Begin);
                byte[] ddfExternal = new byte[imageFilter.GetDataForkLength()];
                stream.Read(ddfExternal, 0, ddfExternal.Length);
                ddfStream.Write(ddfExternal, 0, ddfExternal.Length);
            }

            extents = new Dictionary<ulong, VMwareExtent>();
            ulong currentSector = 0;

            FiltersList filtersList = new FiltersList();

			bool matchedCyls = false, matchedHds = false, matchedSpt = false;

			if(cowD)
            {
                int cowCount = 1;
                string basePath = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath());

                while(true)
                {
                    string curPath;
                    if(cowCount == 1)
                        curPath = basePath + ".vmdk";
                    else
                        curPath = string.Format("{0}-{1:D2}.vmdk", basePath, cowCount);

                    if(!File.Exists(curPath))
                        break;

                    Filter extentFilter = filtersList.GetFilter(curPath);
                    Stream extentStream = extentFilter.GetDataForkStream();

                    if(stream.Length > Marshal.SizeOf(vmCHdr))
                    {
                        VMwareCowHeader extHdrCow = new VMwareCowHeader();
                        extentStream.Seek(0, SeekOrigin.Begin);
                        byte[] vmCHdr_b = new byte[Marshal.SizeOf(extHdrCow)];
                        extentStream.Read(vmCHdr_b, 0, Marshal.SizeOf(extHdrCow));
                        IntPtr cowPtr = Marshal.AllocHGlobal(Marshal.SizeOf(extHdrCow));
                        Marshal.Copy(vmCHdr_b, 0, cowPtr, Marshal.SizeOf(extHdrCow));
                        extHdrCow = (VMwareCowHeader)Marshal.PtrToStructure(cowPtr, typeof(VMwareCowHeader));
                        Marshal.FreeHGlobal(cowPtr);

                        if(extHdrCow.magic != VMwareCowMagic)
                            break;

                        VMwareExtent newExtent = new VMwareExtent();
                        newExtent.access = "RW";
                        newExtent.filter = extentFilter;
                        newExtent.filename = extentFilter.GetFilename();
                        newExtent.offset = 0;
                        newExtent.sectors = extHdrCow.sectors;
                        newExtent.type = "SPARSE";

                        DicConsole.DebugWriteLine("VMware plugin", "{0} {1} {2} \"{3}\" {4}", newExtent.access, newExtent.sectors, newExtent.type, newExtent.filter, newExtent.offset);

                        extents.Add(currentSector, newExtent);
                        currentSector += newExtent.sectors;
					}
                    else
                        break;

                    cowCount++;
                }

                imageType = VMTypeSplitSparse;
            }
            else
            {
                ddfStream.Seek(0, SeekOrigin.Begin);

                Regex RegexVersion = new Regex(VersionRegEx);
                Regex RegexCid = new Regex(CidRegEx);
                Regex RegexParentCid = new Regex(ParenCidRegEx);
                Regex RegexType = new Regex(TypeRegEx);
                Regex RegexExtent = new Regex(ExtentRegEx);
                Regex RegexParent = new Regex(ParentRegEx);
				Regex RegexCylinders = new Regex(DDBCylindersRegEx);
				Regex RegexHeads = new Regex(DDBHeadsRegex);
				Regex RegexSectors = new Regex(DDBSectorsRegEx);

				Match MatchVersion;
                Match MatchCid;
                Match MatchParentCid;
                Match MatchType;
                Match MatchExtent;
                Match MatchParent;
				Match MatchCylinders;
				Match MatchHeads;
				Match MatchSectors;

				StreamReader ddfStreamRdr = new StreamReader(ddfStream);

                while(ddfStreamRdr.Peek() >= 0)
                {
                    string _line = ddfStreamRdr.ReadLine();

                    MatchVersion = RegexVersion.Match(_line);
                    MatchCid = RegexCid.Match(_line);
                    MatchParentCid = RegexParentCid.Match(_line);
                    MatchType = RegexType.Match(_line);
                    MatchExtent = RegexExtent.Match(_line);
                    MatchParent = RegexParent.Match(_line);
					MatchCylinders = RegexCylinders.Match(_line);
					MatchHeads = RegexHeads.Match(_line);
					MatchSectors = RegexSectors.Match(_line);

					if(MatchVersion.Success)
					{
						uint.TryParse(MatchVersion.Groups["version"].Value, out version);
						DicConsole.DebugWriteLine("VMware plugin", "version = {0}", version);
					}
					else if(MatchCid.Success)
					{
						cid = Convert.ToUInt32(MatchCid.Groups["cid"].Value, 16);
						DicConsole.DebugWriteLine("VMware plugin", "cid = {0:x8}", cid);
					}
					else if(MatchParentCid.Success)
					{
						parentCid = Convert.ToUInt32(MatchParentCid.Groups["cid"].Value, 16);
						DicConsole.DebugWriteLine("VMware plugin", "parentCID = {0:x8}", parentCid);
					}
					else if(MatchType.Success)
					{
						imageType = MatchType.Groups["type"].Value;
						DicConsole.DebugWriteLine("VMware plugin", "createType = \"{0}\"", imageType);
					}
					else if(MatchExtent.Success)
					{
						VMwareExtent newExtent = new VMwareExtent();
						newExtent.access = MatchExtent.Groups["access"].Value;
						if(!embedded)
							newExtent.filter = filtersList.GetFilter(Path.Combine(Path.GetDirectoryName(imageFilter.GetBasePath()), MatchExtent.Groups["filename"].Value));
						else
							newExtent.filter = imageFilter;
						uint.TryParse(MatchExtent.Groups["offset"].Value, out newExtent.offset);
						uint.TryParse(MatchExtent.Groups["sectors"].Value, out newExtent.sectors);
						newExtent.type = MatchExtent.Groups["type"].Value;
						DicConsole.DebugWriteLine("VMware plugin", "{0} {1} {2} \"{3}\" {4}", newExtent.access, newExtent.sectors, newExtent.type, newExtent.filter, newExtent.offset);

						extents.Add(currentSector, newExtent);
						currentSector += newExtent.sectors;
					}
					else if(MatchParent.Success)
					{
						parentName = MatchParent.Groups["filename"].Value;
						DicConsole.DebugWriteLine("VMware plugin", "parentFileNameHint = \"{0}\"", parentName);
						hasParent = true;
					}
					else if(MatchCylinders.Success)
					{
						uint.TryParse(MatchCylinders.Groups["cylinders"].Value, out ImageInfo.cylinders);
						matchedCyls = true;
					}
					else if(MatchHeads.Success)
					{
						uint.TryParse(MatchHeads.Groups["heads"].Value, out ImageInfo.heads);
						matchedHds = true;
					}
					else if(MatchSectors.Success)
					{
						uint.TryParse(MatchSectors.Groups["sectors"].Value, out ImageInfo.sectorsPerTrack);
						matchedSpt = true;
					}
				}
            }

            if(extents.Count == 0)
                throw new Exception("Did not find any extent");

            switch(imageType)
            {
                case VMTypeMonoSparse://"monolithicSparse";
                case VMTypeMonoFlat://"monolithicFlat";
                case VMTypeSplitSparse://"twoGbMaxExtentSparse";
                case VMTypeSplitFlat://"twoGbMaxExtentFlat";
                case VMFSTypeFlat://"vmfsPreallocated";
                case VMFSTypeZero://"vmfsEagerZeroedThick";
                case VMFSTypeThin://"vmfsThin";
                case VMFSTypeSparse://"vmfsSparse";
                case VMFSType://"vmfs";
                case VMTypeStream://"streamOptimized";
                    break;
                case VMTypeFullDevice://"fullDevice";
                case VMTypePartDevice://"partitionedDevice";
                case VMFSTypeRDM://"vmfsRDM";
                case VMFSTypeRDMOld://"vmfsRawDeviceMap";
                case VMFSTypeRDMP://"vmfsRDMP";
                case VMFSTypeRDMPOld://"vmfsPassthroughRawDeviceMap";
                case VMFSTypeRaw://"vmfsRaw";
                    throw new ImageNotSupportedException("Raw device image files are not supported, try accessing the device directly.");
                default:
                    throw new ImageNotSupportedException(string.Format("Dunno how to handle \"{0}\" extents.", imageType));
            }

            bool oneNoFlat = false || cowD;

            foreach(VMwareExtent extent in extents.Values)
            {
                if(extent.filter == null)
                    throw new Exception(string.Format("Extent file {0} not found.", extent.filter));

                if(extent.access == "NOACCESS")
                    throw new Exception("Cannot access NOACCESS extents ;).");

                if(extent.type != "FLAT" &&
                   extent.type != "ZERO" &&
                   extent.type != "VMFS" &&
                   !cowD)
                {
                    Stream extentStream = extent.filter.GetDataForkStream();
                    extentStream.Seek(0, SeekOrigin.Begin);

                    if(extentStream.Length < sectorSize)
                        throw new Exception(string.Format("Extent {0} is too small.", extent.filter));

                    VMwareExtentHeader extentHdr = new VMwareExtentHeader();
                    byte[] extentHdr_b = new byte[Marshal.SizeOf(extentHdr)];
                    extentStream.Read(extentHdr_b, 0, Marshal.SizeOf(extentHdr));
                    IntPtr extentHdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(extentHdr));
                    Marshal.Copy(extentHdr_b, 0, extentHdrPtr, Marshal.SizeOf(extentHdr));
                    extentHdr = (VMwareExtentHeader)Marshal.PtrToStructure(extentHdrPtr, typeof(VMwareExtentHeader));
                    Marshal.FreeHGlobal(extentHdrPtr);

                    if(extentHdr.magic != VMwareExtentMagic)
                        throw new Exception(string.Format("{0} is not an VMware extent.", extent.filter));

                    if(extentHdr.capacity != extent.sectors)
                        throw new Exception(string.Format("Extent contains incorrect number of sectors, {0}. {1} were expected", extentHdr.capacity, extent.sectors));

                    // TODO: Support compressed extents
                    if(extentHdr.compression != CompressionNone)
                        throw new ImageNotSupportedException("Compressed extents are not yet supported.");

                    if(!vmEHdrSet)
                    {
                        vmEHdr = extentHdr;
                        gdFilter = extent.filter;
                        vmEHdrSet = true;
                    }

                    oneNoFlat = true;
                }
            }

            if(oneNoFlat && !vmEHdrSet && !cowD)
                throw new Exception("There are sparse extents but there is no header to find the grain tables, cannot proceed.");

            ImageInfo.sectors = currentSector;

            uint grains = 0;
            uint gdEntries = 0;
            long gdOffset = 0;
            uint GTEsPerGT = 0;

            if(oneNoFlat && !cowD)
            {
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.magic = 0x{0:X8}", vmEHdr.magic);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.version = {0}", vmEHdr.version);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.flags = 0x{0:X8}", vmEHdr.flags);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.capacity = {0}", vmEHdr.capacity);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.grainSize = {0}", vmEHdr.grainSize);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.descriptorOffset = {0}", vmEHdr.descriptorOffset);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.descriptorSize = {0}", vmEHdr.descriptorSize);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.GTEsPerGT = {0}", vmEHdr.GTEsPerGT);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.rgdOffset = {0}", vmEHdr.rgdOffset);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.gdOffset = {0}", vmEHdr.gdOffset);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.overhead = {0}", vmEHdr.overhead);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.uncleanShutdown = {0}", vmEHdr.uncleanShutdown);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.singleEndLineChar = 0x{0:X2}", vmEHdr.singleEndLineChar);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.nonEndLineChar = 0x{0:X2}", vmEHdr.nonEndLineChar);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.doubleEndLineChar1 = 0x{0:X2}", vmEHdr.doubleEndLineChar1);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.doubleEndLineChar2 = 0x{0:X2}", vmEHdr.doubleEndLineChar2);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.compression = 0x{0:X4}", vmEHdr.compression);

                grainSize = vmEHdr.grainSize;
                grains = (uint)(ImageInfo.sectors / vmEHdr.grainSize);
                gdEntries = grains / vmEHdr.GTEsPerGT;
                GTEsPerGT = vmEHdr.GTEsPerGT;

                if((vmEHdr.flags & FlagsUseRedundantTable) == FlagsUseRedundantTable)
                    gdOffset = (long)vmEHdr.rgdOffset;
                else
                    gdOffset = (long)vmEHdr.gdOffset;
            }
            else if(oneNoFlat && cowD)
            {
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.magic = 0x{0:X8}", vmCHdr.magic);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.version = {0}", vmCHdr.version);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.flags = 0x{0:X8}", vmCHdr.flags);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.sectors = {0}", vmCHdr.sectors);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.grainSize = {0}", vmCHdr.grainSize);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.gdOffset = {0}", vmCHdr.gdOffset);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.numGDEntries = {0}", vmCHdr.numGDEntries);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.freeSector = {0}", vmCHdr.freeSector);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.cylinders = {0}", vmCHdr.cylinders);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.heads = {0}", vmCHdr.heads);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.spt = {0}", vmCHdr.spt);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.generation = {0}", vmCHdr.generation);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.name = {0}", StringHandlers.CToString(vmCHdr.name));
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.description = {0}", StringHandlers.CToString(vmCHdr.description));
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.savedGeneration = {0}", vmCHdr.savedGeneration);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.uncleanShutdown = {0}", vmCHdr.uncleanShutdown);

                grainSize = vmCHdr.grainSize;
                grains = (uint)(ImageInfo.sectors / vmCHdr.grainSize);
                gdEntries = vmCHdr.numGDEntries;
                gdOffset = vmCHdr.gdOffset;
                GTEsPerGT = grains / gdEntries;
                ImageInfo.imageName = StringHandlers.CToString(vmCHdr.name);
                ImageInfo.imageComments = StringHandlers.CToString(vmCHdr.description);
                version = vmCHdr.version;
            }

            if(oneNoFlat)
            {
                if(grains == 0 || gdEntries == 0)
                    throw new Exception("Some error ocurred setting GD sizes");

                DicConsole.DebugWriteLine("VMware plugin", "{0} sectors in {1} grains in {2} tables", ImageInfo.sectors, grains, gdEntries);

                Stream gdStream = gdFilter.GetDataForkStream();

                gdStream.Seek(gdOffset * sectorSize, SeekOrigin.Begin);

                DicConsole.DebugWriteLine("VMware plugin", "Reading grain directory");
                uint[] gd = new uint[gdEntries];
                byte[] gdBytes = new byte[gdEntries * 4];
                gdStream.Read(gdBytes, 0, gdBytes.Length);
                for(int i = 0; i < gdEntries; i++)
                    gd[i] = BitConverter.ToUInt32(gdBytes, i * 4);

                DicConsole.DebugWriteLine("VMware plugin", "Reading grain tables");
                uint currentGrain = 0;
                gTable = new uint[grains];
                foreach(uint gtOff in gd)
                {
                    byte[] gtBytes = new byte[GTEsPerGT * 4];
                    gdStream.Seek(gtOff * sectorSize, SeekOrigin.Begin);
                    gdStream.Read(gtBytes, 0, gtBytes.Length);
                    for(int i = 0; i < GTEsPerGT; i++)
                    {
                        gTable[currentGrain] = BitConverter.ToUInt32(gtBytes, i * 4);
                        currentGrain++;
                    }
                }

                maxCachedGrains = (uint)(MaxCacheSize / (grainSize * sectorSize));

                grainCache = new Dictionary<ulong, byte[]>();
            }

            if(hasParent)
            {
                Filter parentFilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), parentName));
                if(parentFilter == null)
                    throw new Exception(string.Format("Cannot find parent \"{0}\".", parentName));

                parentImage = new VMware();

                if(!parentImage.OpenImage(parentFilter))
                    throw new Exception(string.Format("Cannot open parent \"{0}\".", parentName));
            }

            sectorCache = new Dictionary<ulong, byte[]>();

            ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
            ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
            ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            ImageInfo.sectorSize = sectorSize;
            ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
            ImageInfo.mediaType = MediaType.GENERIC_HDD;
            ImageInfo.imageSize = ImageInfo.sectors * sectorSize;
            // VMDK version 1 started on VMware 4, so there is a previous version, "COWD"
            if(cowD)
                ImageInfo.imageVersion = string.Format("{0}", version);
            else
                ImageInfo.imageVersion = string.Format("{0}", version + 3);

			if(cowD)
			{
				ImageInfo.cylinders = vmCHdr.cylinders;
				ImageInfo.heads = vmCHdr.heads;
				ImageInfo.sectorsPerTrack = vmCHdr.spt;
			}
			else if(!matchedCyls || !matchedHds || !matchedSpt)
			{
				ImageInfo.cylinders = (uint)((ImageInfo.sectors / 16) / 63);
				ImageInfo.heads = 16;
				ImageInfo.sectorsPerTrack = 63;
			}

			return true;
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

            byte[] sector;

            if(sectorCache.TryGetValue(sectorAddress, out sector))
                return sector;

            VMwareExtent currentExtent = new VMwareExtent();
            bool extentFound = false;
            ulong extentStartSector = 0;

            foreach(KeyValuePair<ulong, VMwareExtent> kvp in extents)
            {
                if(sectorAddress >= kvp.Key)
                {
                    currentExtent = kvp.Value;
                    extentFound = true;
                    extentStartSector = kvp.Key;
                }
            }

            if(!extentFound)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

            Stream dataStream;

            if(currentExtent.type == "ZERO")
            {
                sector = new byte[sectorSize];

                if(sectorCache.Count >= maxCachedSectors)
                    sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);
                return sector;
            }

            if(currentExtent.type == "FLAT" || currentExtent.type == "VMFS")
            {
                dataStream = currentExtent.filter.GetDataForkStream();
                dataStream.Seek((long)(currentExtent.offset + ((sectorAddress - extentStartSector) * sectorSize)), SeekOrigin.Begin);
                sector = new byte[sectorSize];
                dataStream.Read(sector, 0, sector.Length);

                if(sectorCache.Count >= maxCachedSectors)
                    sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);
                return sector;
            }

            ulong index = sectorAddress / grainSize;
            ulong secOff = (sectorAddress % grainSize) * sectorSize;

            uint grainOff = gTable[index];

            if(grainOff == 0 && hasParent)
                return parentImage.ReadSector(sectorAddress);

            if(grainOff == 0 || grainOff == 1)
            {
                sector = new byte[sectorSize];

                if(sectorCache.Count >= maxCachedSectors)
                    sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);
                return sector;
            }

            byte[] grain = new byte[sectorSize * grainSize];

            if(!grainCache.TryGetValue(grainOff, out grain))
            {
                grain = new byte[sectorSize * grainSize];
                dataStream = currentExtent.filter.GetDataForkStream();
                dataStream.Seek((long)(((grainOff - extentStartSector) * sectorSize)), SeekOrigin.Begin);
                dataStream.Read(grain, 0, grain.Length);

                if(grainCache.Count >= maxCachedGrains)
                    grainCache.Clear();

                grainCache.Add(grainOff, grain);
            }

            sector = new byte[sectorSize];
            Array.Copy(grain, (int)secOff, sector, 0, sectorSize);

            if(sectorCache.Count > maxCachedSectors)
                sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > ImageInfo.sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

            if(sectorAddress + length > ImageInfo.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
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
            return "VMware";
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

        public override string GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
        }

        #region Unsupported features

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
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

