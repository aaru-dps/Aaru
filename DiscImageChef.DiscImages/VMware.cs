// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : VMware.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    public class VMware : IWritableImage
    {
        const uint VMWARE_EXTENT_MAGIC = 0x564D444B;
        const uint VMWARE_COW_MAGIC    = 0x44574F43;

        const string VM_TYPE_CUSTOM       = "custom";
        const string VM_TYPE_MONO_SPARSE  = "monolithicSparse";
        const string VM_TYPE_MONO_FLAT    = "monolithicFlat";
        const string VM_TYPE_SPLIT_SPARSE = "twoGbMaxExtentSparse";
        const string VM_TYPE_SPLIT_FLAT   = "twoGbMaxExtentFlat";
        const string VM_TYPE_FULL_DEVICE  = "fullDevice";
        const string VM_TYPE_PART_DEVICE  = "partitionedDevice";
        const string VMFS_TYPE_FLAT       = "vmfsPreallocated";
        const string VMFS_TYPE_ZERO       = "vmfsEagerZeroedThick";
        const string VMFS_TYPE_THIN       = "vmfsThin";
        const string VMFS_TYPE_SPARSE     = "vmfsSparse";
        const string VMFS_TYPE_RDM        = "vmfsRDM";
        const string VMFS_TYPE_RDM_OLD    = "vmfsRawDeviceMap";
        const string VMFS_TYPE_RDMP       = "vmfsRDMP";
        const string VMFS_TYPE_RDMP_OLD   = "vmfsPassthroughRawDeviceMap";
        const string VMFS_TYPE_RAW        = "vmfsRaw";
        const string VMFS_TYPE            = "vmfs";
        const string VM_TYPE_STREAM       = "streamOptimized";

        const string DDF_MAGIC = "# Disk DescriptorFile";

        const string REGEX_VERSION    = @"^\s*version\s*=\s*(?<version>\d+)$";
        const string REGEX_CID        = @"^\s*CID\s*=\s*(?<cid>[0123456789abcdef]{8})$";
        const string REGEX_CID_PARENT = @"^\s*parentCID\s*=\s*(?<cid>[0123456789abcdef]{8})$";
        const string REGEX_TYPE       =
            @"^\s*createType\s*=\s*\""(?<type>custom|monolithicSparse|monolithicFlat|twoGbMaxExtentSparse|twoGbMaxExtentFlat|fullDevice|partitionedDevice|vmfs|vmfsPreallocated|vmfsEagerZeroedThick|vmfsThin|vmfsSparse|vmfsRDM|vmfsRawDeviceMap|vmfsRDMP|vmfsPassthroughRawDeviceMap|vmfsRaw|streamOptimized)\""$";
        const string REGEX_EXTENT =
            @"^\s*(?<access>(RW|RDONLY|NOACCESS))\s+(?<sectors>\d+)\s+(?<type>(FLAT|SPARSE|ZERO|VMFS|VMFSSPARSE|VMFSRDM|VMFSRAW))\s+\""(?<filename>.+)\""(\s*(?<offset>\d+))?$";
        const string REGEX_DDB_TYPE =
            @"^\s*ddb\.adapterType\s*=\s*\""(?<type>ide|buslogic|lsilogic|legacyESX)\""$";
        const string REGEX_DDB_SECTORS   = @"^\s*ddb\.geometry\.sectors\s*=\s*\""(?<sectors>\d+)\""$";
        const string REGEX_DDB_HEADS     = @"^\s*ddb\.geometry\.heads\s*=\s*\""(?<heads>\d+)\""$";
        const string REGEX_DDB_CYLINDERS = @"^\s*ddb\.geometry\.cylinders\s*=\s*\""(?<cylinders>\d+)\""$";
        const string PARENT_REGEX        = @"^\s*parentFileNameHint\s*=\s*\""(?<filename>.+)\""$";

        const uint FLAGS_VALID_NEW_LINE      = 0x01;
        const uint FLAGS_USE_REDUNDANT_TABLE = 0x02;
        const uint FLAGS_ZERO_GRAIN_GTE      = 0x04;
        const uint FLAGS_COMPRESSION         = 0x10000;
        const uint FLAGS_MARKERS             = 0x20000;

        const ushort COMPRESSION_NONE    = 0;
        const ushort COMPRESSION_DEFLATE = 1;

        const    uint   MAX_CACHE_SIZE     = 16777216;
        const    uint   SECTOR_SIZE        = 512;
        const    uint   MAX_CACHED_SECTORS = MAX_CACHE_SIZE / SECTOR_SIZE;
        readonly byte[] ddfMagicBytes      =
        {
            0x23, 0x20, 0x44, 0x69, 0x73, 0x6B, 0x20, 0x44, 0x65, 0x73, 0x63, 0x72, 0x69, 0x70, 0x74, 0x6F, 0x72, 0x46,
            0x69, 0x6C, 0x65
        };
        string adapter_type;

        uint                            cid;
        StreamWriter                    descriptorStream;
        Dictionary<ulong, VMwareExtent> extents;
        IFilter                         gdFilter;
        Dictionary<ulong, byte[]>       grainCache;

        ulong     grainSize;
        uint[]    gTable;
        bool      hasParent;
        int       hwversion;
        ImageInfo imageInfo;
        string    imageType;
        uint      maxCachedGrains;
        uint      parentCid;

        IMediaImage parentImage;
        string      parentName;

        Dictionary<ulong, byte[]> sectorCache;
        uint                      version;
        VMwareCowHeader           vmCHdr;

        VMwareExtentHeader vmEHdr;
        string             writingBaseName;
        FileStream         writingStream;

        public VMware()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = false,
                HasSessions           = false,
                Version               = null,
                Application           = "VMware",
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

        public string Name => "VMware disk image";
        public Guid   Id   => new Guid("E314DE35-C103-48A3-AD36-990F68523C46");

        public string Format => "VMware";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            byte[] ddfMagic = new byte[0x15];

            if(stream.Length > Marshal.SizeOf(vmEHdr))
            {
                stream.Seek(0, SeekOrigin.Begin);
                byte[] vmEHdrB = new byte[Marshal.SizeOf(vmEHdr)];
                stream.Read(vmEHdrB, 0, Marshal.SizeOf(vmEHdr));
                vmEHdr           = new VMwareExtentHeader();
                IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vmEHdr));
                Marshal.Copy(vmEHdrB, 0, headerPtr, Marshal.SizeOf(vmEHdr));
                vmEHdr = (VMwareExtentHeader)Marshal.PtrToStructure(headerPtr, typeof(VMwareExtentHeader));
                Marshal.FreeHGlobal(headerPtr);

                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(ddfMagic, 0, 0x15);

                vmCHdr = new VMwareCowHeader();
                if(stream.Length                                                 <= Marshal.SizeOf(vmCHdr))
                    return ddfMagicBytes.SequenceEqual(ddfMagic) || vmEHdr.magic == VMWARE_EXTENT_MAGIC ||
                           vmCHdr.magic                                          == VMWARE_COW_MAGIC;

                stream.Seek(0, SeekOrigin.Begin);
                byte[] vmCHdrB = new byte[Marshal.SizeOf(vmCHdr)];
                stream.Read(vmCHdrB, 0, Marshal.SizeOf(vmCHdr));
                headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vmCHdr));
                Marshal.Copy(vmCHdrB, 0, headerPtr, Marshal.SizeOf(vmCHdr));
                vmCHdr = (VMwareCowHeader)Marshal.PtrToStructure(headerPtr, typeof(VMwareCowHeader));
                Marshal.FreeHGlobal(headerPtr);

                return ddfMagicBytes.SequenceEqual(ddfMagic) || vmEHdr.magic == VMWARE_EXTENT_MAGIC ||
                       vmCHdr.magic                                          == VMWARE_COW_MAGIC;
            }

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(ddfMagic, 0, 0x15);

            return ddfMagicBytes.SequenceEqual(ddfMagic);
        }

        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            vmEHdr        = new VMwareExtentHeader();
            vmCHdr        = new VMwareCowHeader();
            bool embedded = false;

            if(stream.Length > Marshal.SizeOf(vmEHdr))
            {
                stream.Seek(0, SeekOrigin.Begin);
                byte[] vmEHdrB = new byte[Marshal.SizeOf(vmEHdr)];
                stream.Read(vmEHdrB, 0, Marshal.SizeOf(vmEHdr));
                IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vmEHdr));
                Marshal.Copy(vmEHdrB, 0, headerPtr, Marshal.SizeOf(vmEHdr));
                vmEHdr = (VMwareExtentHeader)Marshal.PtrToStructure(headerPtr, typeof(VMwareExtentHeader));
                Marshal.FreeHGlobal(headerPtr);
            }

            if(stream.Length > Marshal.SizeOf(vmCHdr))
            {
                stream.Seek(0, SeekOrigin.Begin);
                byte[] vmCHdrB = new byte[Marshal.SizeOf(vmCHdr)];
                stream.Read(vmCHdrB, 0, Marshal.SizeOf(vmCHdr));
                IntPtr cowPtr = Marshal.AllocHGlobal(Marshal.SizeOf(vmCHdr));
                Marshal.Copy(vmCHdrB, 0, cowPtr, Marshal.SizeOf(vmCHdr));
                vmCHdr = (VMwareCowHeader)Marshal.PtrToStructure(cowPtr, typeof(VMwareCowHeader));
                Marshal.FreeHGlobal(cowPtr);
            }

            MemoryStream ddfStream = new MemoryStream();
            bool         vmEHdrSet = false;
            bool         cowD      = false;

            if(vmEHdr.magic == VMWARE_EXTENT_MAGIC)
            {
                vmEHdrSet = true;
                gdFilter  = imageFilter;

                if(vmEHdr.descriptorOffset == 0 || vmEHdr.descriptorSize == 0)
                    throw new Exception("Please open VMDK descriptor.");

                byte[] ddfEmbed = new byte[vmEHdr.descriptorSize * SECTOR_SIZE];

                stream.Seek((long)(vmEHdr.descriptorOffset * SECTOR_SIZE), SeekOrigin.Begin);
                stream.Read(ddfEmbed, 0, ddfEmbed.Length);
                ddfStream.Write(ddfEmbed, 0, ddfEmbed.Length);

                embedded = true;
            }
            else if(vmCHdr.magic == VMWARE_COW_MAGIC)
            {
                gdFilter = imageFilter;
                cowD     = true;
            }
            else
            {
                byte[] ddfMagic = new byte[0x15];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(ddfMagic, 0, 0x15);

                if(!ddfMagicBytes.SequenceEqual(ddfMagic)) throw new Exception("Not a descriptor.");

                stream.Seek(0, SeekOrigin.Begin);
                byte[] ddfExternal = new byte[imageFilter.GetDataForkLength()];
                stream.Read(ddfExternal, 0, ddfExternal.Length);
                ddfStream.Write(ddfExternal, 0, ddfExternal.Length);
            }

            extents             = new Dictionary<ulong, VMwareExtent>();
            ulong currentSector = 0;

            bool matchedCyls = false, matchedHds = false, matchedSpt = false;

            if(cowD)
            {
                int    cowCount = 1;
                string basePath = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath());

                while(true)
                {
                    string curPath;
                    if(cowCount == 1) curPath = basePath + ".vmdk";
                    else curPath              = $"{basePath}-{cowCount:D2}.vmdk";

                    if(!File.Exists(curPath)) break;

                    IFilter extentFilter = new FiltersList().GetFilter(curPath);
                    Stream  extentStream = extentFilter.GetDataForkStream();

                    if(stream.Length > Marshal.SizeOf(vmCHdr))
                    {
                        VMwareCowHeader extHdrCow = new VMwareCowHeader();
                        extentStream.Seek(0, SeekOrigin.Begin);
                        byte[] vmCHdrB = new byte[Marshal.SizeOf(extHdrCow)];
                        extentStream.Read(vmCHdrB, 0, Marshal.SizeOf(extHdrCow));
                        IntPtr cowPtr = Marshal.AllocHGlobal(Marshal.SizeOf(extHdrCow));
                        Marshal.Copy(vmCHdrB, 0, cowPtr, Marshal.SizeOf(extHdrCow));
                        extHdrCow = (VMwareCowHeader)Marshal.PtrToStructure(cowPtr, typeof(VMwareCowHeader));
                        Marshal.FreeHGlobal(cowPtr);

                        if(extHdrCow.magic != VMWARE_COW_MAGIC) break;

                        VMwareExtent newExtent = new VMwareExtent
                        {
                            Access   = "RW",
                            Filter   = extentFilter,
                            Filename = extentFilter.GetFilename(),
                            Offset   = 0,
                            Sectors  = extHdrCow.sectors,
                            Type     = "SPARSE"
                        };

                        DicConsole.DebugWriteLine("VMware plugin", "{0} {1} {2} \"{3}\" {4}", newExtent.Access,
                                                  newExtent.Sectors, newExtent.Type, newExtent.Filename,
                                                  newExtent.Offset);

                        extents.Add(currentSector, newExtent);
                        currentSector += newExtent.Sectors;
                    }
                    else break;

                    cowCount++;
                }

                imageType = VM_TYPE_SPLIT_SPARSE;
            }
            else
            {
                ddfStream.Seek(0, SeekOrigin.Begin);

                Regex regexVersion   = new Regex(REGEX_VERSION);
                Regex regexCid       = new Regex(REGEX_CID);
                Regex regexParentCid = new Regex(REGEX_CID_PARENT);
                Regex regexType      = new Regex(REGEX_TYPE);
                Regex regexExtent    = new Regex(REGEX_EXTENT);
                Regex regexParent    = new Regex(PARENT_REGEX);
                Regex regexCylinders = new Regex(REGEX_DDB_CYLINDERS);
                Regex regexHeads     = new Regex(REGEX_DDB_HEADS);
                Regex regexSectors   = new Regex(REGEX_DDB_SECTORS);

                StreamReader ddfStreamRdr = new StreamReader(ddfStream);

                while(ddfStreamRdr.Peek() >= 0)
                {
                    string line = ddfStreamRdr.ReadLine();

                    Match matchVersion   = regexVersion.Match(line);
                    Match matchCid       = regexCid.Match(line);
                    Match matchParentCid = regexParentCid.Match(line);
                    Match matchType      = regexType.Match(line);
                    Match matchExtent    = regexExtent.Match(line);
                    Match matchParent    = regexParent.Match(line);
                    Match matchCylinders = regexCylinders.Match(line);
                    Match matchHeads     = regexHeads.Match(line);
                    Match matchSectors   = regexSectors.Match(line);

                    if(matchVersion.Success)
                    {
                        uint.TryParse(matchVersion.Groups["version"].Value, out version);
                        DicConsole.DebugWriteLine("VMware plugin", "version = {0}", version);
                    }
                    else if(matchCid.Success)
                    {
                        cid = Convert.ToUInt32(matchCid.Groups["cid"].Value, 16);
                        DicConsole.DebugWriteLine("VMware plugin", "cid = {0:x8}", cid);
                    }
                    else if(matchParentCid.Success)
                    {
                        parentCid = Convert.ToUInt32(matchParentCid.Groups["cid"].Value, 16);
                        DicConsole.DebugWriteLine("VMware plugin", "parentCID = {0:x8}", parentCid);
                    }
                    else if(matchType.Success)
                    {
                        imageType = matchType.Groups["type"].Value;
                        DicConsole.DebugWriteLine("VMware plugin", "createType = \"{0}\"", imageType);
                    }
                    else if(matchExtent.Success)
                    {
                        VMwareExtent newExtent = new VMwareExtent {Access = matchExtent.Groups["access"].Value};
                        if(!embedded)
                            newExtent.Filter =
                                new FiltersList()
                                   .GetFilter(Path.Combine(Path.GetDirectoryName(imageFilter.GetBasePath()),
                                                           matchExtent.Groups["filename"].Value));
                        else newExtent.Filter = imageFilter;
                        uint.TryParse(matchExtent.Groups["offset"].Value,  out newExtent.Offset);
                        uint.TryParse(matchExtent.Groups["sectors"].Value, out newExtent.Sectors);
                        newExtent.Type = matchExtent.Groups["type"].Value;
                        DicConsole.DebugWriteLine("VMware plugin", "{0} {1} {2} \"{3}\" {4}", newExtent.Access,
                                                  newExtent.Sectors, newExtent.Type, newExtent.Filename,
                                                  newExtent.Offset);

                        extents.Add(currentSector, newExtent);
                        currentSector += newExtent.Sectors;
                    }
                    else if(matchParent.Success)
                    {
                        parentName = matchParent.Groups["filename"].Value;
                        DicConsole.DebugWriteLine("VMware plugin", "parentFileNameHint = \"{0}\"", parentName);
                        hasParent = true;
                    }
                    else if(matchCylinders.Success)
                    {
                        uint.TryParse(matchCylinders.Groups["cylinders"].Value, out imageInfo.Cylinders);
                        matchedCyls = true;
                    }
                    else if(matchHeads.Success)
                    {
                        uint.TryParse(matchHeads.Groups["heads"].Value, out imageInfo.Heads);
                        matchedHds = true;
                    }
                    else if(matchSectors.Success)
                    {
                        uint.TryParse(matchSectors.Groups["sectors"].Value, out imageInfo.SectorsPerTrack);
                        matchedSpt = true;
                    }
                }
            }

            if(extents.Count == 0) throw new Exception("Did not find any extent");

            switch(imageType)
            {
                case VM_TYPE_MONO_SPARSE:  //"monolithicSparse";
                case VM_TYPE_MONO_FLAT:    //"monolithicFlat";
                case VM_TYPE_SPLIT_SPARSE: //"twoGbMaxExtentSparse";
                case VM_TYPE_SPLIT_FLAT:   //"twoGbMaxExtentFlat";
                case VMFS_TYPE_FLAT:       //"vmfsPreallocated";
                case VMFS_TYPE_ZERO:       //"vmfsEagerZeroedThick";
                case VMFS_TYPE_THIN:       //"vmfsThin";
                case VMFS_TYPE_SPARSE:     //"vmfsSparse";
                case VMFS_TYPE:            //"vmfs";
                case VM_TYPE_STREAM:       //"streamOptimized";
                    break;
                case VM_TYPE_FULL_DEVICE: //"fullDevice";
                case VM_TYPE_PART_DEVICE: //"partitionedDevice";
                case VMFS_TYPE_RDM:       //"vmfsRDM";
                case VMFS_TYPE_RDM_OLD:   //"vmfsRawDeviceMap";
                case VMFS_TYPE_RDMP:      //"vmfsRDMP";
                case VMFS_TYPE_RDMP_OLD:  //"vmfsPassthroughRawDeviceMap";
                case VMFS_TYPE_RAW:       //"vmfsRaw";
                    throw new
                        ImageNotSupportedException("Raw device image files are not supported, try accessing the device directly.");
                default: throw new ImageNotSupportedException($"Dunno how to handle \"{imageType}\" extents.");
            }

            bool oneNoFlat = cowD;

            foreach(VMwareExtent extent in extents.Values)
            {
                if(extent.Filter == null) throw new Exception($"Extent file {extent.Filename} not found.");

                if(extent.Access == "NOACCESS") throw new Exception("Cannot access NOACCESS extents ;).");

                if(extent.Type == "FLAT" || extent.Type == "ZERO" || extent.Type == "VMFS" || cowD) continue;

                Stream extentStream = extent.Filter.GetDataForkStream();
                extentStream.Seek(0, SeekOrigin.Begin);

                if(extentStream.Length < SECTOR_SIZE) throw new Exception($"Extent {extent.Filename} is too small.");

                VMwareExtentHeader extentHdr  = new VMwareExtentHeader();
                byte[]             extentHdrB = new byte[Marshal.SizeOf(extentHdr)];
                extentStream.Read(extentHdrB, 0, Marshal.SizeOf(extentHdr));
                IntPtr extentHdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(extentHdr));
                Marshal.Copy(extentHdrB, 0, extentHdrPtr, Marshal.SizeOf(extentHdr));
                extentHdr = (VMwareExtentHeader)Marshal.PtrToStructure(extentHdrPtr, typeof(VMwareExtentHeader));
                Marshal.FreeHGlobal(extentHdrPtr);

                if(extentHdr.magic != VMWARE_EXTENT_MAGIC)
                    throw new Exception($"{extent.Filter} is not an VMware extent.");

                if(extentHdr.capacity < extent.Sectors)
                    throw new
                        Exception($"Extent contains incorrect number of sectors, {extentHdr.capacity}. {extent.Sectors} were expected");

                // TODO: Support compressed extents
                if(extentHdr.compression != COMPRESSION_NONE)
                    throw new ImageNotSupportedException("Compressed extents are not yet supported.");

                if(!vmEHdrSet)
                {
                    vmEHdr    = extentHdr;
                    gdFilter  = extent.Filter;
                    vmEHdrSet = true;
                }

                oneNoFlat = true;
            }

            if(oneNoFlat && !vmEHdrSet && !cowD)
                throw new
                    Exception("There are sparse extents but there is no header to find the grain tables, cannot proceed.");

            imageInfo.Sectors = currentSector;

            uint grains    = 0;
            uint gdEntries = 0;
            long gdOffset  = 0;
            uint gtEsPerGt = 0;

            if(oneNoFlat && !cowD)
            {
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.magic = 0x{0:X8}",       vmEHdr.magic);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.version = {0}",          vmEHdr.version);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.flags = 0x{0:X8}",       vmEHdr.flags);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.capacity = {0}",         vmEHdr.capacity);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.grainSize = {0}",        vmEHdr.grainSize);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.descriptorOffset = {0}", vmEHdr.descriptorOffset);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.descriptorSize = {0}",   vmEHdr.descriptorSize);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.GTEsPerGT = {0}",        vmEHdr.GTEsPerGT);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.rgdOffset = {0}",        vmEHdr.rgdOffset);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.gdOffset = {0}",         vmEHdr.gdOffset);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.overhead = {0}",         vmEHdr.overhead);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.uncleanShutdown = {0}",  vmEHdr.uncleanShutdown);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.singleEndLineChar = 0x{0:X2}",
                                          vmEHdr.singleEndLineChar);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.nonEndLineChar = 0x{0:X2}", vmEHdr.nonEndLineChar);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.doubleEndLineChar1 = 0x{0:X2}",
                                          vmEHdr.doubleEndLineChar1);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.doubleEndLineChar2 = 0x{0:X2}",
                                          vmEHdr.doubleEndLineChar2);
                DicConsole.DebugWriteLine("VMware plugin", "vmEHdr.compression = 0x{0:X4}", vmEHdr.compression);

                grainSize = vmEHdr.grainSize;
                grains    = (uint)(imageInfo.Sectors / vmEHdr.grainSize) + 1;
                gdEntries = grains                   / vmEHdr.GTEsPerGT;
                gtEsPerGt = vmEHdr.GTEsPerGT;

                if((vmEHdr.flags & FLAGS_USE_REDUNDANT_TABLE) == FLAGS_USE_REDUNDANT_TABLE)
                    gdOffset  = (long)vmEHdr.rgdOffset;
                else gdOffset = (long)vmEHdr.gdOffset;
            }
            else if(oneNoFlat && cowD)
            {
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.magic = 0x{0:X8}",   vmCHdr.magic);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.version = {0}",      vmCHdr.version);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.flags = 0x{0:X8}",   vmCHdr.flags);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.sectors = {0}",      vmCHdr.sectors);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.grainSize = {0}",    vmCHdr.grainSize);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.gdOffset = {0}",     vmCHdr.gdOffset);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.numGDEntries = {0}", vmCHdr.numGDEntries);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.freeSector = {0}",   vmCHdr.freeSector);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.cylinders = {0}",    vmCHdr.cylinders);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.heads = {0}",        vmCHdr.heads);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.spt = {0}",          vmCHdr.spt);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.generation = {0}",   vmCHdr.generation);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.name = {0}",
                                          StringHandlers.CToString(vmCHdr.name));
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.description = {0}",
                                          StringHandlers.CToString(vmCHdr.description));
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.savedGeneration = {0}", vmCHdr.savedGeneration);
                DicConsole.DebugWriteLine("VMware plugin", "vmCHdr.uncleanShutdown = {0}", vmCHdr.uncleanShutdown);

                grainSize            = vmCHdr.grainSize;
                grains               = (uint)(imageInfo.Sectors / vmCHdr.grainSize) + 1;
                gdEntries            = vmCHdr.numGDEntries;
                gdOffset             = vmCHdr.gdOffset;
                gtEsPerGt            = grains / gdEntries;
                imageInfo.MediaTitle = StringHandlers.CToString(vmCHdr.name);
                imageInfo.Comments   = StringHandlers.CToString(vmCHdr.description);
                version              = vmCHdr.version;
            }

            if(oneNoFlat)
            {
                if(grains == 0 || gdEntries == 0) throw new Exception("Some error ocurred setting GD sizes");

                DicConsole.DebugWriteLine("VMware plugin", "{0} sectors in {1} grains in {2} tables", imageInfo.Sectors,
                                          grains, gdEntries);

                Stream gdStream = gdFilter.GetDataForkStream();

                gdStream.Seek(gdOffset * SECTOR_SIZE, SeekOrigin.Begin);

                DicConsole.DebugWriteLine("VMware plugin", "Reading grain directory");
                uint[] gd      = new uint[gdEntries];
                byte[] gdBytes = new byte[gdEntries * 4];
                gdStream.Read(gdBytes, 0, gdBytes.Length);
                for(int i = 0; i < gdEntries; i++) gd[i] = BitConverter.ToUInt32(gdBytes, i * 4);

                DicConsole.DebugWriteLine("VMware plugin", "Reading grain tables");
                uint currentGrain = 0;
                gTable            = new uint[grains];
                foreach(uint gtOff in gd)
                {
                    byte[] gtBytes = new byte[gtEsPerGt * 4];
                    gdStream.Seek(gtOff                 * SECTOR_SIZE, SeekOrigin.Begin);
                    gdStream.Read(gtBytes, 0, gtBytes.Length);
                    for(int i = 0; i < gtEsPerGt; i++)
                    {
                        gTable[currentGrain] = BitConverter.ToUInt32(gtBytes, i * 4);
                        currentGrain++;
                    }
                }

                maxCachedGrains = (uint)(MAX_CACHE_SIZE / (grainSize * SECTOR_SIZE));

                grainCache = new Dictionary<ulong, byte[]>();
            }

            if(hasParent)
            {
                IFilter parentFilter =
                    new FiltersList().GetFilter(Path.Combine(imageFilter.GetParentFolder(), parentName));
                if(parentFilter == null) throw new Exception($"Cannot find parent \"{parentName}\".");

                parentImage = new VMware();

                if(!parentImage.Open(parentFilter)) throw new Exception($"Cannot open parent \"{parentName}\".");
            }

            sectorCache = new Dictionary<ulong, byte[]>();

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.SectorSize           = SECTOR_SIZE;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.ImageSize            = imageInfo.Sectors * SECTOR_SIZE;
            // VMDK version 1 started on VMware 4, so there is a previous version, "COWD"
            imageInfo.Version = cowD ? $"{version}" : $"{version + 3}";

            if(cowD)
            {
                imageInfo.Cylinders       = vmCHdr.cylinders;
                imageInfo.Heads           = vmCHdr.heads;
                imageInfo.SectorsPerTrack = vmCHdr.spt;
            }
            else if(!matchedCyls || !matchedHds || !matchedSpt)
            {
                imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
                imageInfo.Heads           = 16;
                imageInfo.SectorsPerTrack = 63;
            }

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector)) return sector;

            VMwareExtent currentExtent     = new VMwareExtent();
            bool         extentFound       = false;
            ulong        extentStartSector = 0;

            foreach(KeyValuePair<ulong, VMwareExtent> kvp in extents.Where(kvp => sectorAddress >= kvp.Key))
            {
                currentExtent     = kvp.Value;
                extentFound       = true;
                extentStartSector = kvp.Key;
            }

            if(!extentFound)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            Stream dataStream;

            switch(currentExtent.Type)
            {
                case "ZERO":
                    sector = new byte[SECTOR_SIZE];

                    if(sectorCache.Count >= MAX_CACHED_SECTORS) sectorCache.Clear();

                    sectorCache.Add(sectorAddress, sector);
                    return sector;
                case "FLAT":
                case "VMFS":
                    dataStream = currentExtent.Filter.GetDataForkStream();
                    dataStream.Seek((long)((currentExtent.Offset + (sectorAddress - extentStartSector)) * SECTOR_SIZE),
                                    SeekOrigin.Begin);
                    sector = new byte[SECTOR_SIZE];
                    dataStream.Read(sector, 0, sector.Length);

                    if(sectorCache.Count >= MAX_CACHED_SECTORS) sectorCache.Clear();

                    sectorCache.Add(sectorAddress, sector);
                    return sector;
            }

            ulong index  = sectorAddress / grainSize;
            ulong secOff = sectorAddress % grainSize * SECTOR_SIZE;

            uint grainOff = gTable[index];

            if(grainOff == 0 && hasParent) return parentImage.ReadSector(sectorAddress);

            if(grainOff == 0 || grainOff == 1)
            {
                sector = new byte[SECTOR_SIZE];

                if(sectorCache.Count >= MAX_CACHED_SECTORS) sectorCache.Clear();

                sectorCache.Add(sectorAddress, sector);
                return sector;
            }

            if(!grainCache.TryGetValue(grainOff, out byte[] grain))
            {
                grain      = new byte[SECTOR_SIZE * grainSize];
                dataStream = currentExtent.Filter.GetDataForkStream();
                dataStream.Seek((long)((grainOff - extentStartSector) * SECTOR_SIZE), SeekOrigin.Begin);
                dataStream.Read(grain, 0, grain.Length);

                if(grainCache.Count >= maxCachedGrains) grainCache.Clear();

                grainCache.Add(grainOff, grain);
            }

            sector = new byte[SECTOR_SIZE];
            Array.Copy(grain, (int)secOff, sector, 0, SECTOR_SIZE);

            if(sectorCache.Count > MAX_CACHED_SECTORS) sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public byte[] ReadDiskTag(MediaTagType tag)
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
            new[] {MediaType.GENERIC_HDD, MediaType.Unknown};
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new[]
            {
                ("adapter_type", typeof(string),
                "Type of adapter type. Possible values: ide, lsilogic, buslogic, legacyESX."),
                ("hwversion", typeof(int), "VDMK hardware version."), ("sparse", typeof(bool), "Use sparse extents."),
                ("split", typeof(bool), "Split data file at 2GiB.")
            };
        public IEnumerable<string> KnownExtensions => new[] {".vmdk"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(options != null)
            {
                if(options.TryGetValue("adapter", out adapter_type))
                    switch(adapter_type.ToLowerInvariant())
                    {
                        case "ide":
                        case "lsilogic":
                        case "buslogic":
                            adapter_type = adapter_type.ToLowerInvariant();
                            break;
                        case "legacyesx":
                            adapter_type = "legacyESX";
                            break;
                        default:
                            ErrorMessage = $"Invalid adapter type {adapter_type}";
                            return false;
                    }
                else adapter_type = "ide";

                if(options.TryGetValue("hwversion", out string tmpValue))
                {
                    if(!int.TryParse(tmpValue, out hwversion))
                    {
                        ErrorMessage = "Invalid value for hwversion option";
                        return false;
                    }
                }
                else hwversion = 4;

                if(options.TryGetValue("split", out tmpValue))
                {
                    if(!bool.TryParse(tmpValue, out bool tmpBool))
                    {
                        ErrorMessage = "Invalid value for split option";
                        return false;
                    }

                    if(tmpBool)
                    {
                        ErrorMessage = "Splitted images not yet implemented";
                        return false;
                    }
                }

                if(options.TryGetValue("sparse", out tmpValue))
                {
                    if(!bool.TryParse(tmpValue, out bool tmpBool))
                    {
                        ErrorMessage = "Invalid value for sparse option";
                        return false;
                    }

                    if(tmpBool)
                    {
                        ErrorMessage = "Sparse images not yet implemented";
                        return false;
                    }
                }
            }
            else
            {
                adapter_type = "ide";
                hwversion    = 4;
            }

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

            try
            {
                writingBaseName  = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                descriptorStream = new StreamWriter(path, false, Encoding.ASCII);
                // TODO: Support split
                writingStream = new FileStream(writingBaseName + "-flat.vmdk", FileMode.CreateNew, FileAccess.ReadWrite,
                                               FileShare.None);
            }
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

            writingStream.Seek((long)(sectorAddress * 512), SeekOrigin.Begin);
            writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";
            return true;
        }

        // TODO: Implement sparse and split
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

            writingStream.Seek((long)(sectorAddress * 512), SeekOrigin.Begin);
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

        // TODO: Implement sparse and split
        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";
                return false;
            }

            writingStream.Flush();
            writingStream.Close();

            // TODO: Interface to set geometry
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

            descriptorStream.WriteLine("# Disk DescriptorFile");
            descriptorStream.WriteLine("version=1");
            descriptorStream.WriteLine($"CID={new Random().Next(int.MinValue, int.MaxValue):x8}");
            descriptorStream.WriteLine("parentCID=ffffffff");
            descriptorStream.WriteLine("createType=\"monolithicFlat\"");
            descriptorStream.WriteLine();
            descriptorStream.WriteLine("# Extent description");
            descriptorStream.WriteLine($"RW {imageInfo.Sectors} FLAT \"{writingBaseName + "-flat.vmdk"}\" 0");
            descriptorStream.WriteLine();
            descriptorStream.WriteLine("# The Disk Data Base");
            descriptorStream.WriteLine("#DDB");
            descriptorStream.WriteLine();
            descriptorStream.WriteLine($"ddb.virtualHWVersion = \"{hwversion}\"");
            descriptorStream.WriteLine($"ddb.geometry.cylinders = \"{imageInfo.Cylinders}\"");
            descriptorStream.WriteLine($"ddb.geometry.heads = \"{imageInfo.Heads}\"");
            descriptorStream.WriteLine($"ddb.geometry.sectors = \"{imageInfo.SectorsPerTrack}\"");
            descriptorStream.WriteLine($"ddb.adapterType = \"{adapter_type}\"");

            descriptorStream.Flush();
            descriptorStream.Close();
            IsWriting = false;

            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VMwareExtentHeader
        {
            public                               uint   magic;
            public                               uint   version;
            public                               uint   flags;
            public                               ulong  capacity;
            public                               ulong  grainSize;
            public                               ulong  descriptorOffset;
            public                               ulong  descriptorSize;
            public                               uint   GTEsPerGT;
            public                               ulong  rgdOffset;
            public                               ulong  gdOffset;
            public                               ulong  overhead;
            [MarshalAs(UnmanagedType.U1)] public bool   uncleanShutdown;
            public                               byte   singleEndLineChar;
            public                               byte   nonEndLineChar;
            public                               byte   doubleEndLineChar1;
            public                               byte   doubleEndLineChar2;
            public                               ushort compression;
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
            public uint   parentGeneration;
            public uint   generation;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
            public byte[] name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] description;
            public uint   savedGeneration;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public                               byte[] reserved;
            [MarshalAs(UnmanagedType.U1)] public bool   uncleanShutdown;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 396)]
            public byte[] padding;
        }

        struct VMwareExtent
        {
            public string  Access;
            public uint    Sectors;
            public string  Type;
            public IFilter Filter;
            public string  Filename;
            public uint    Offset;
        }
    }
}