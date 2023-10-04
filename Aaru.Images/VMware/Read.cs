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
//     Reads VMware disk images.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages;

public sealed partial class VMware
{
#region IWritableImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();

        _vmEHdr = new ExtentHeader();
        _vmCHdr = new CowHeader();
        var embedded = false;

        if(stream.Length > Marshal.SizeOf<ExtentHeader>())
        {
            stream.Seek(0, SeekOrigin.Begin);
            var vmEHdrB = new byte[Marshal.SizeOf<ExtentHeader>()];
            stream.EnsureRead(vmEHdrB, 0, Marshal.SizeOf<ExtentHeader>());
            _vmEHdr = Marshal.ByteArrayToStructureLittleEndian<ExtentHeader>(vmEHdrB);
        }

        if(stream.Length > Marshal.SizeOf<CowHeader>())
        {
            stream.Seek(0, SeekOrigin.Begin);
            var vmCHdrB = new byte[Marshal.SizeOf<CowHeader>()];
            stream.EnsureRead(vmCHdrB, 0, Marshal.SizeOf<CowHeader>());
            _vmCHdr = Marshal.ByteArrayToStructureLittleEndian<CowHeader>(vmCHdrB);
        }

        var ddfStream = new MemoryStream();
        var vmEHdrSet = false;
        var cowD      = false;

        if(_vmEHdr.magic == VMWARE_EXTENT_MAGIC)
        {
            vmEHdrSet = true;
            _gdFilter = imageFilter;

            if(_vmEHdr.descriptorOffset == 0 || _vmEHdr.descriptorSize == 0)
            {
                AaruConsole.ErrorWriteLine(Localization.Please_open_VMDK_descriptor);

                return ErrorNumber.InvalidArgument;
            }

            var ddfEmbed = new byte[_vmEHdr.descriptorSize * SECTOR_SIZE];

            stream.Seek((long)(_vmEHdr.descriptorOffset * SECTOR_SIZE), SeekOrigin.Begin);
            stream.EnsureRead(ddfEmbed, 0, ddfEmbed.Length);
            ddfStream.Write(ddfEmbed, 0, ddfEmbed.Length);

            embedded = true;
        }
        else if(_vmCHdr.magic == VMWARE_COW_MAGIC)
        {
            _gdFilter = imageFilter;
            cowD      = true;
        }
        else
        {
            var ddfMagic = new byte[0x15];
            stream.Seek(0, SeekOrigin.Begin);
            stream.EnsureRead(ddfMagic, 0, 0x15);

            if(!_ddfMagicBytes.SequenceEqual(ddfMagic))
            {
                AaruConsole.ErrorWriteLine(Localization.Not_a_descriptor);

                return ErrorNumber.InvalidArgument;
            }

            stream.Seek(0, SeekOrigin.Begin);
            var ddfExternal = new byte[imageFilter.DataForkLength];
            stream.EnsureRead(ddfExternal, 0, ddfExternal.Length);
            ddfStream.Write(ddfExternal, 0, ddfExternal.Length);
        }

        _extents = new Dictionary<ulong, Extent>();
        ulong currentSector = 0;

        bool matchedCyls = false, matchedHds = false, matchedSpt = false;

        if(cowD)
        {
            var    cowCount = 1;
            string basePath = Path.GetFileNameWithoutExtension(imageFilter.BasePath);

            while(true)
            {
                string curPath;

                if(cowCount == 1)
                    curPath = basePath + ".vmdk";
                else
                    curPath = $"{basePath}-{cowCount:D2}.vmdk";

                if(!File.Exists(curPath))
                    break;

                IFilter extentFilter = new FiltersList().GetFilter(curPath);
                Stream  extentStream = extentFilter.GetDataForkStream();

                if(stream.Length > Marshal.SizeOf<CowHeader>())
                {
                    extentStream.Seek(0, SeekOrigin.Begin);
                    var vmCHdrB = new byte[Marshal.SizeOf<CowHeader>()];
                    extentStream.EnsureRead(vmCHdrB, 0, Marshal.SizeOf<CowHeader>());
                    CowHeader extHdrCow = Marshal.ByteArrayToStructureLittleEndian<CowHeader>(vmCHdrB);

                    if(extHdrCow.magic != VMWARE_COW_MAGIC)
                        break;

                    var newExtent = new Extent
                    {
                        Access   = "RW",
                        Filter   = extentFilter,
                        Filename = extentFilter.Filename,
                        Offset   = 0,
                        Sectors  = extHdrCow.sectors,
                        Type     = "SPARSE"
                    };

                    AaruConsole.DebugWriteLine(MODULE_NAME, "{0} {1} {2} \"{3}\" {4}", newExtent.Access,
                                               newExtent.Sectors, newExtent.Type, newExtent.Filename, newExtent.Offset);

                    _extents.Add(currentSector, newExtent);
                    currentSector += newExtent.Sectors;
                }
                else
                    break;

                cowCount++;
            }

            _imageType = VM_TYPE_SPLIT_SPARSE;
        }
        else
        {
            ddfStream.Seek(0, SeekOrigin.Begin);

            var regexVersion   = new Regex(REGEX_VERSION);
            var regexCid       = new Regex(REGEX_CID);
            var regexParentCid = new Regex(REGEX_CID_PARENT);
            var regexType      = new Regex(REGEX_TYPE);
            var regexExtent    = new Regex(REGEX_EXTENT);
            var regexParent    = new Regex(PARENT_REGEX);
            var regexCylinders = new Regex(REGEX_DDB_CYLINDERS);
            var regexHeads     = new Regex(REGEX_DDB_HEADS);
            var regexSectors   = new Regex(REGEX_DDB_SECTORS);

            var ddfStreamRdr = new StreamReader(ddfStream);

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
                    uint.TryParse(matchVersion.Groups["version"].Value, out _version);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "version = {0}", _version);
                }
                else if(matchCid.Success)
                {
                    _cid = Convert.ToUInt32(matchCid.Groups["cid"].Value, 16);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "cid = {0:x8}", _cid);
                }
                else if(matchParentCid.Success)
                {
                    _parentCid = Convert.ToUInt32(matchParentCid.Groups["cid"].Value, 16);
                    AaruConsole.DebugWriteLine(MODULE_NAME, "parentCID = {0:x8}", _parentCid);
                }
                else if(matchType.Success)
                {
                    _imageType = matchType.Groups["type"].Value;
                    AaruConsole.DebugWriteLine(MODULE_NAME, "createType = \"{0}\"", _imageType);
                }
                else if(matchExtent.Success)
                {
                    var newExtent = new Extent
                    {
                        Access = matchExtent.Groups["access"].Value
                    };

                    if(!embedded)
                    {
                        newExtent.Filter =
                            new FiltersList().GetFilter(Path.Combine(Path.GetDirectoryName(imageFilter.BasePath),
                                                                     matchExtent.Groups["filename"].Value));
                    }
                    else
                        newExtent.Filter = imageFilter;

                    uint.TryParse(matchExtent.Groups["offset"].Value,  out newExtent.Offset);
                    uint.TryParse(matchExtent.Groups["sectors"].Value, out newExtent.Sectors);
                    newExtent.Type = matchExtent.Groups["type"].Value;

                    AaruConsole.DebugWriteLine(MODULE_NAME, "{0} {1} {2} \"{3}\" {4}", newExtent.Access,
                                               newExtent.Sectors, newExtent.Type, newExtent.Filename, newExtent.Offset);

                    _extents.Add(currentSector, newExtent);
                    currentSector += newExtent.Sectors;
                }
                else if(matchParent.Success)
                {
                    _parentName = matchParent.Groups["filename"].Value;
                    AaruConsole.DebugWriteLine(MODULE_NAME, "parentFileNameHint = \"{0}\"", _parentName);
                    _hasParent = true;
                }
                else if(matchCylinders.Success)
                {
                    uint.TryParse(matchCylinders.Groups["cylinders"].Value, out _imageInfo.Cylinders);
                    matchedCyls = true;
                }
                else if(matchHeads.Success)
                {
                    uint.TryParse(matchHeads.Groups["heads"].Value, out _imageInfo.Heads);
                    matchedHds = true;
                }
                else if(matchSectors.Success)
                {
                    uint.TryParse(matchSectors.Groups["sectors"].Value, out _imageInfo.SectorsPerTrack);
                    matchedSpt = true;
                }
            }
        }

        if(_extents.Count == 0)
        {
            AaruConsole.ErrorWriteLine(Localization.Did_not_find_any_extent);

            return ErrorNumber.InvalidArgument;
        }

        switch(_imageType)
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
                AaruConsole.ErrorWriteLine(Localization.Raw_device_image_files_are_not_supported);

                return ErrorNumber.NotSupported;
            default:
                AaruConsole.ErrorWriteLine(string.Format(Localization.Dunno_how_to_handle_0_extents, _imageType));

                return ErrorNumber.InvalidArgument;
        }

        bool oneNoFlat = cowD;

        foreach(Extent extent in _extents.Values)
        {
            if(extent.Filter == null)
            {
                AaruConsole.ErrorWriteLine(string.Format(Localization.Extent_file_0_not_found, extent.Filename));

                return ErrorNumber.NoSuchFile;
            }

            if(extent.Access == "NOACCESS")
            {
                AaruConsole.ErrorWriteLine(Localization.Cannot_access_NOACCESS_extents);

                return ErrorNumber.InvalidArgument;
            }

            if(extent.Type is "FLAT" or "ZERO" or "VMFS" || cowD)
                continue;

            Stream extentStream = extent.Filter.GetDataForkStream();
            extentStream.Seek(0, SeekOrigin.Begin);

            if(extentStream.Length < SECTOR_SIZE)
            {
                AaruConsole.ErrorWriteLine(string.Format(Localization.Extent_0_is_too_small, extent.Filename));

                return ErrorNumber.InvalidArgument;
            }

            var extentHdrB = new byte[Marshal.SizeOf<ExtentHeader>()];
            extentStream.EnsureRead(extentHdrB, 0, Marshal.SizeOf<ExtentHeader>());
            ExtentHeader extentHdr = Marshal.ByteArrayToStructureLittleEndian<ExtentHeader>(extentHdrB);

            if(extentHdr.magic != VMWARE_EXTENT_MAGIC)
            {
                AaruConsole.ErrorWriteLine(string.Format(Localization._0_is_not_an_VMware_extent, extent.Filter));

                return ErrorNumber.InvalidArgument;
            }

            if(extentHdr.capacity < extent.Sectors)
            {
                AaruConsole.
                    ErrorWriteLine(string.Format(Localization.Extent_contains_incorrect_number_of_sectors_0_1_were_expected,
                                                 extentHdr.capacity, extent.Sectors));

                return ErrorNumber.InvalidArgument;
            }

            // TODO: Support compressed extents
            if(extentHdr.compression != COMPRESSION_NONE)
            {
                AaruConsole.ErrorWriteLine(Localization.Compressed_extents_are_not_yet_supported);

                return ErrorNumber.NotImplemented;
            }

            if(!vmEHdrSet)
            {
                _vmEHdr   = extentHdr;
                _gdFilter = extent.Filter;
                vmEHdrSet = true;
            }

            oneNoFlat = true;
        }

        if(oneNoFlat && !vmEHdrSet && !cowD)
        {
            AaruConsole.ErrorWriteLine(Localization.
                                           There_are_sparse_extents_but_there_is_no_header_to_find_the_grain_tables_cannot_proceed);

            return ErrorNumber.InvalidArgument;
        }

        _imageInfo.Sectors = currentSector;

        uint grains    = 0;
        uint gdEntries = 0;
        long gdOffset  = 0;
        uint gtEsPerGt = 0;

        switch(oneNoFlat)
        {
            case true when !cowD:
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.magic = 0x{0:X8}",       _vmEHdr.magic);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.version = {0}",          _vmEHdr.version);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.flags = 0x{0:X8}",       _vmEHdr.flags);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.capacity = {0}",         _vmEHdr.capacity);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.grainSize = {0}",        _vmEHdr.grainSize);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.descriptorOffset = {0}", _vmEHdr.descriptorOffset);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.descriptorSize = {0}",   _vmEHdr.descriptorSize);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.GTEsPerGT = {0}",        _vmEHdr.GTEsPerGT);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.rgdOffset = {0}",        _vmEHdr.rgdOffset);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.gdOffset = {0}",         _vmEHdr.gdOffset);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.overhead = {0}",         _vmEHdr.overhead);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.uncleanShutdown = {0}",  _vmEHdr.uncleanShutdown);

                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.singleEndLineChar = 0x{0:X2}",
                                           _vmEHdr.singleEndLineChar);

                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.nonEndLineChar = 0x{0:X2}", _vmEHdr.nonEndLineChar);

                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.doubleEndLineChar1 = 0x{0:X2}",
                                           _vmEHdr.doubleEndLineChar1);

                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.doubleEndLineChar2 = 0x{0:X2}",
                                           _vmEHdr.doubleEndLineChar2);

                AaruConsole.DebugWriteLine(MODULE_NAME, "vmEHdr.compression = 0x{0:X4}", _vmEHdr.compression);

                _grainSize = _vmEHdr.grainSize;
                grains     = (uint)(_imageInfo.Sectors / _vmEHdr.grainSize) + 1;
                gdEntries  = grains / _vmEHdr.GTEsPerGT;
                gtEsPerGt  = _vmEHdr.GTEsPerGT;

                if((_vmEHdr.flags & FLAGS_USE_REDUNDANT_TABLE) == FLAGS_USE_REDUNDANT_TABLE)
                    gdOffset = (long)_vmEHdr.rgdOffset;
                else
                    gdOffset = (long)_vmEHdr.gdOffset;

                break;
            }
            case true when cowD:
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.magic = 0x{0:X8}",   _vmCHdr.magic);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.version = {0}",      _vmCHdr.version);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.flags = 0x{0:X8}",   _vmCHdr.flags);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.sectors = {0}",      _vmCHdr.sectors);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.grainSize = {0}",    _vmCHdr.grainSize);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.gdOffset = {0}",     _vmCHdr.gdOffset);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.numGDEntries = {0}", _vmCHdr.numGDEntries);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.freeSector = {0}",   _vmCHdr.freeSector);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.cylinders = {0}",    _vmCHdr.cylinders);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.heads = {0}",        _vmCHdr.heads);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.spt = {0}",          _vmCHdr.spt);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.generation = {0}",   _vmCHdr.generation);

                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.name = {0}", StringHandlers.CToString(_vmCHdr.name));

                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.description = {0}",
                                           StringHandlers.CToString(_vmCHdr.description));

                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.savedGeneration = {0}", _vmCHdr.savedGeneration);
                AaruConsole.DebugWriteLine(MODULE_NAME, "vmCHdr.uncleanShutdown = {0}", _vmCHdr.uncleanShutdown);

                _grainSize            = _vmCHdr.grainSize;
                grains                = (uint)(_imageInfo.Sectors / _vmCHdr.grainSize) + 1;
                gdEntries             = _vmCHdr.numGDEntries;
                gdOffset              = _vmCHdr.gdOffset;
                gtEsPerGt             = grains / gdEntries;
                _imageInfo.MediaTitle = StringHandlers.CToString(_vmCHdr.name);
                _imageInfo.Comments   = StringHandlers.CToString(_vmCHdr.description);
                _version              = _vmCHdr.version;

                break;
        }

        if(oneNoFlat)
        {
            if(grains == 0 || gdEntries == 0)
            {
                AaruConsole.ErrorWriteLine(Localization.Some_error_occurred_setting_GD_sizes);

                return ErrorNumber.InOutError;
            }

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization._0_sectors_in_1_grains_in_2_tables, _imageInfo.Sectors,
                                       grains, gdEntries);

            Stream gdStream = _gdFilter.GetDataForkStream();

            gdStream.Seek(gdOffset * SECTOR_SIZE, SeekOrigin.Begin);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_grain_directory);
            var gdBytes = new byte[gdEntries * 4];
            gdStream.EnsureRead(gdBytes, 0, gdBytes.Length);
            Span<uint> gd = MemoryMarshal.Cast<byte, uint>(gdBytes);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_grain_tables);
            uint currentGrain = 0;
            _gTable = new uint[grains];

            foreach(uint gtOff in gd)
            {
                var gtBytes = new byte[gtEsPerGt * 4];
                gdStream.Seek(gtOff * SECTOR_SIZE, SeekOrigin.Begin);
                gdStream.EnsureRead(gtBytes, 0, gtBytes.Length);

                uint[] currentGt = MemoryMarshal.Cast<byte, uint>(gtBytes).ToArray();
                Array.Copy(currentGt, 0, _gTable, currentGrain, gtEsPerGt);
                currentGrain += gtEsPerGt;

                // TODO: Check speed here
                /*
                for(int i = 0; i < gtEsPerGt; i++)
                {
                    gTable[currentGrain] = BitConverter.ToUInt32(gtBytes, i * 4);
                    currentGrain++;
                }
            */
            }

            _maxCachedGrains = (uint)(MAX_CACHE_SIZE / (_grainSize * SECTOR_SIZE));

            _grainCache = new Dictionary<ulong, byte[]>();
        }

        if(_hasParent)
        {
            IFilter parentFilter = new FiltersList().GetFilter(Path.Combine(imageFilter.ParentFolder, _parentName));

            if(parentFilter == null)
            {
                AaruConsole.ErrorWriteLine(string.Format(Localization.Cannot_find_parent_0, _parentName));

                return ErrorNumber.NoSuchFile;
            }

            _parentImage = new VMware();
            ErrorNumber parentError = _parentImage.Open(parentFilter);

            if(parentError != ErrorNumber.NoError)
            {
                AaruConsole.ErrorWriteLine(string.Format(Localization.Error_0_opening_parent_1, parentError,
                                                         _parentName));

                return parentError;
            }
        }

        _sectorCache = new Dictionary<ulong, byte[]>();

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.SectorSize           = SECTOR_SIZE;
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.MediaType            = MediaType.GENERIC_HDD;
        _imageInfo.ImageSize            = _imageInfo.Sectors * SECTOR_SIZE;

        // VMDK version 1 started on VMware 4, so there is a previous version, "COWD"
        _imageInfo.Version = cowD ? $"{_version}" : $"{_version + 3}";

        if(cowD)
        {
            _imageInfo.Cylinders       = _vmCHdr.cylinders;
            _imageInfo.Heads           = _vmCHdr.heads;
            _imageInfo.SectorsPerTrack = _vmCHdr.spt;
        }
        else if(!matchedCyls || !matchedHds || !matchedSpt)
        {
            _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 16 / 63);
            _imageInfo.Heads           = 16;
            _imageInfo.SectorsPerTrack = 63;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(_sectorCache.TryGetValue(sectorAddress, out buffer))
            return ErrorNumber.NoError;

        var   currentExtent     = new Extent();
        var   extentFound       = false;
        ulong extentStartSector = 0;

        foreach(KeyValuePair<ulong, Extent> kvp in _extents.Where(kvp => sectorAddress >= kvp.Key))
        {
            currentExtent     = kvp.Value;
            extentFound       = true;
            extentStartSector = kvp.Key;
        }

        if(!extentFound)
            return ErrorNumber.SectorNotFound;

        Stream dataStream;

        switch(currentExtent.Type)
        {
            case "ZERO":
                buffer = new byte[SECTOR_SIZE];

                if(_sectorCache.Count >= MAX_CACHED_SECTORS)
                    _sectorCache.Clear();

                _sectorCache.Add(sectorAddress, buffer);

                return ErrorNumber.NoError;
            case "FLAT":
            case "VMFS":
                dataStream = currentExtent.Filter.GetDataForkStream();

                dataStream.Seek((long)((currentExtent.Offset + (sectorAddress - extentStartSector)) * SECTOR_SIZE),
                                SeekOrigin.Begin);

                buffer = new byte[SECTOR_SIZE];
                dataStream.EnsureRead(buffer, 0, buffer.Length);

                if(_sectorCache.Count >= MAX_CACHED_SECTORS)
                    _sectorCache.Clear();

                _sectorCache.Add(sectorAddress, buffer);

                return ErrorNumber.NoError;
        }

        ulong index  = sectorAddress              / _grainSize;
        ulong secOff = sectorAddress % _grainSize * SECTOR_SIZE;

        uint grainOff = _gTable[index];

        switch(grainOff)
        {
            case 0 when _hasParent:
                return _parentImage.ReadSector(sectorAddress, out buffer);
            case 0 or 1:
            {
                buffer = new byte[SECTOR_SIZE];

                if(_sectorCache.Count >= MAX_CACHED_SECTORS)
                    _sectorCache.Clear();

                _sectorCache.Add(sectorAddress, buffer);

                return ErrorNumber.NoError;
            }
        }

        if(!_grainCache.TryGetValue(grainOff, out byte[] grain))
        {
            grain      = new byte[SECTOR_SIZE * _grainSize];
            dataStream = currentExtent.Filter.GetDataForkStream();
            dataStream.Seek((long)((grainOff - extentStartSector) * SECTOR_SIZE), SeekOrigin.Begin);
            dataStream.EnsureRead(grain, 0, grain.Length);

            if(_grainCache.Count >= _maxCachedGrains)
                _grainCache.Clear();

            _grainCache.Add(grainOff, grain);
        }

        buffer = new byte[SECTOR_SIZE];
        Array.Copy(grain, (int)secOff, buffer, 0, SECTOR_SIZE);

        if(_sectorCache.Count > MAX_CACHED_SECTORS)
            _sectorCache.Clear();

        _sectorCache.Add(sectorAddress, buffer);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return errno;

            ms.Write(sector, 0, sector.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

#endregion
}