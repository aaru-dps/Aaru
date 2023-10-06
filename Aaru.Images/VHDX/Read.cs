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
//     Reads Microsoft Hyper-V disk images.
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
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class Vhdx
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512)
            return ErrorNumber.InvalidArgument;

        var vhdxIdB = new byte[Marshal.SizeOf<Identifier>()];
        stream.EnsureRead(vhdxIdB, 0, Marshal.SizeOf<Identifier>());
        _id = Marshal.ByteArrayToStructureLittleEndian<Identifier>(vhdxIdB);

        if(_id.signature != VHDX_SIGNATURE)
            return ErrorNumber.InvalidArgument;

        _imageInfo.Application = Encoding.Unicode.GetString(_id.creator);

        stream.Seek(64 * 1024, SeekOrigin.Begin);
        var vHdrB = new byte[Marshal.SizeOf<Header>()];
        stream.EnsureRead(vHdrB, 0, Marshal.SizeOf<Header>());
        _vHdr = Marshal.ByteArrayToStructureLittleEndian<Header>(vHdrB);

        if(_vHdr.Signature != VHDX_HEADER_SIG)
        {
            stream.Seek(128 * 1024, SeekOrigin.Begin);
            vHdrB = new byte[Marshal.SizeOf<Header>()];
            stream.EnsureRead(vHdrB, 0, Marshal.SizeOf<Header>());
            _vHdr = Marshal.ByteArrayToStructureLittleEndian<Header>(vHdrB);

            if(_vHdr.Signature != VHDX_HEADER_SIG)
            {
                AaruConsole.ErrorWriteLine(Localization.VHDX_header_not_found);

                return ErrorNumber.InvalidArgument;
            }
        }

        stream.Seek(192 * 1024, SeekOrigin.Begin);
        var vRegTableB = new byte[Marshal.SizeOf<RegionTableHeader>()];
        stream.EnsureRead(vRegTableB, 0, Marshal.SizeOf<RegionTableHeader>());
        _vRegHdr = Marshal.ByteArrayToStructureLittleEndian<RegionTableHeader>(vRegTableB);

        if(_vRegHdr.signature != VHDX_REGION_SIG)
        {
            stream.Seek(256 * 1024, SeekOrigin.Begin);
            vRegTableB = new byte[Marshal.SizeOf<RegionTableHeader>()];
            stream.EnsureRead(vRegTableB, 0, Marshal.SizeOf<RegionTableHeader>());
            _vRegHdr = Marshal.ByteArrayToStructureLittleEndian<RegionTableHeader>(vRegTableB);

            if(_vRegHdr.signature != VHDX_REGION_SIG)
            {
                AaruConsole.ErrorWriteLine(Localization.VHDX_region_table_not_found);

                return ErrorNumber.InvalidArgument;
            }
        }

        _vRegs = new RegionTableEntry[_vRegHdr.entries];

        for(var i = 0; i < _vRegs.Length; i++)
        {
            var vRegB = new byte[System.Runtime.InteropServices.Marshal.SizeOf(_vRegs[i])];
            stream.EnsureRead(vRegB, 0, System.Runtime.InteropServices.Marshal.SizeOf(_vRegs[i]));
            _vRegs[i] = Marshal.ByteArrayToStructureLittleEndian<RegionTableEntry>(vRegB);

            if(_vRegs[i].guid == _batGuid)
                _batOffset = (long)_vRegs[i].offset;
            else if(_vRegs[i].guid == _metadataGuid)
                _metadataOffset = (long)_vRegs[i].offset;
            else if((_vRegs[i].flags & REGION_FLAGS_REQUIRED) == REGION_FLAGS_REQUIRED)
            {
                AaruConsole.
                    ErrorWriteLine(string.Format(Localization.Found_unsupported_and_required_region_Guid_0_not_proceeding_with_image,
                                                 _vRegs[i].guid));

                return ErrorNumber.InvalidArgument;
            }
        }

        if(_batOffset == 0)
        {
            AaruConsole.ErrorWriteLine(Localization.BAT_not_found_cannot_continue);

            return ErrorNumber.InvalidArgument;
        }

        if(_metadataOffset == 0)
        {
            AaruConsole.ErrorWriteLine(Localization.Metadata_not_found_cannot_continue);

            return ErrorNumber.InvalidArgument;
        }

        uint fileParamsOff = 0, vdSizeOff = 0, p83Off = 0, logOff = 0, physOff = 0, parentOff = 0;

        stream.Seek(_metadataOffset, SeekOrigin.Begin);
        var metTableB = new byte[Marshal.SizeOf<MetadataTableHeader>()];
        stream.EnsureRead(metTableB, 0, Marshal.SizeOf<MetadataTableHeader>());
        _vMetHdr = Marshal.ByteArrayToStructureLittleEndian<MetadataTableHeader>(metTableB);

        _vMets = new MetadataTableEntry[_vMetHdr.entries];

        for(var i = 0; i < _vMets.Length; i++)
        {
            var vMetB = new byte[System.Runtime.InteropServices.Marshal.SizeOf(_vMets[i])];
            stream.EnsureRead(vMetB, 0, System.Runtime.InteropServices.Marshal.SizeOf(_vMets[i]));
            _vMets[i] = Marshal.ByteArrayToStructureLittleEndian<MetadataTableEntry>(vMetB);

            if(_vMets[i].itemId == _fileParametersGuid)
                fileParamsOff = _vMets[i].offset;
            else if(_vMets[i].itemId == _virtualDiskSizeGuid)
                vdSizeOff = _vMets[i].offset;
            else if(_vMets[i].itemId == _page83DataGuid)
                p83Off = _vMets[i].offset;
            else if(_vMets[i].itemId == _logicalSectorSizeGuid)
                logOff = _vMets[i].offset;
            else if(_vMets[i].itemId == _physicalSectorSizeGuid)
                physOff = _vMets[i].offset;
            else if(_vMets[i].itemId == _parentLocatorGuid)
                parentOff = _vMets[i].offset;
            else if((_vMets[i].flags & METADATA_FLAGS_REQUIRED) == METADATA_FLAGS_REQUIRED)
            {
                AaruConsole.
                    ErrorWriteLine(string.Format(Localization.Found_unsupported_and_required_metadata_Guid_0_not_proceeding_with_image,
                                                 _vMets[i].itemId));

                return ErrorNumber.InvalidArgument;
            }
        }

        byte[] tmp;

        if(fileParamsOff != 0)
        {
            stream.Seek(fileParamsOff + _metadataOffset, SeekOrigin.Begin);
            tmp = new byte[8];
            stream.EnsureRead(tmp, 0, 8);

            _vFileParms = new FileParameters
            {
                blockSize = BitConverter.ToUInt32(tmp, 0),
                flags     = BitConverter.ToUInt32(tmp, 4)
            };
        }
        else
        {
            AaruConsole.ErrorWriteLine(Localization.File_parameters_not_found);

            return ErrorNumber.InvalidArgument;
        }

        if(vdSizeOff != 0)
        {
            stream.Seek(vdSizeOff + _metadataOffset, SeekOrigin.Begin);
            tmp = new byte[8];
            stream.EnsureRead(tmp, 0, 8);
            _virtualDiskSize = BitConverter.ToUInt64(tmp, 0);
        }
        else
        {
            AaruConsole.ErrorWriteLine(Localization.Virtual_disk_size_not_found);

            return ErrorNumber.InvalidArgument;
        }

        if(p83Off != 0)
        {
            stream.Seek(p83Off + _metadataOffset, SeekOrigin.Begin);
            tmp = new byte[16];
            stream.EnsureRead(tmp, 0, 16);
            _page83Data = new Guid(tmp);
        }

        if(logOff != 0)
        {
            stream.Seek(logOff + _metadataOffset, SeekOrigin.Begin);
            tmp = new byte[4];
            stream.EnsureRead(tmp, 0, 4);
            _logicalSectorSize = BitConverter.ToUInt32(tmp, 0);
        }
        else
        {
            AaruConsole.ErrorWriteLine(Localization.Logical_sector_size_not_found);

            return ErrorNumber.InvalidArgument;
        }

        if(physOff != 0)
        {
            stream.Seek(physOff + _metadataOffset, SeekOrigin.Begin);
            tmp = new byte[4];
            stream.EnsureRead(tmp, 0, 4);
            _physicalSectorSize = BitConverter.ToUInt32(tmp, 0);
        }
        else
        {
            AaruConsole.ErrorWriteLine(Localization.Physical_sector_size_not_found);

            return ErrorNumber.InvalidArgument;
        }

        if(parentOff != 0 && (_vFileParms.flags & FILE_FLAGS_HAS_PARENT) == FILE_FLAGS_HAS_PARENT)
        {
            stream.Seek(parentOff + _metadataOffset, SeekOrigin.Begin);
            var vParHdrB = new byte[Marshal.SizeOf<ParentLocatorHeader>()];
            stream.EnsureRead(vParHdrB, 0, Marshal.SizeOf<ParentLocatorHeader>());
            _vParHdr = Marshal.ByteArrayToStructureLittleEndian<ParentLocatorHeader>(vParHdrB);

            if(_vParHdr.locatorType != _parentTypeVhdxGuid)
            {
                AaruConsole.
                    ErrorWriteLine(string.Format(Localization.Found_unsupported_and_required_parent_locator_type_0_not_proceeding_with_image,
                                                 _vParHdr.locatorType));

                return ErrorNumber.NotSupported;
            }

            _vPars = new ParentLocatorEntry[_vParHdr.keyValueCount];

            for(var i = 0; i < _vPars.Length; i++)
            {
                var vParB = new byte[System.Runtime.InteropServices.Marshal.SizeOf(_vPars[i])];
                stream.EnsureRead(vParB, 0, System.Runtime.InteropServices.Marshal.SizeOf(_vPars[i]));
                _vPars[i] = Marshal.ByteArrayToStructureLittleEndian<ParentLocatorEntry>(vParB);
            }
        }
        else if((_vFileParms.flags & FILE_FLAGS_HAS_PARENT) == FILE_FLAGS_HAS_PARENT)
        {
            AaruConsole.ErrorWriteLine(Localization.Parent_locator_not_found);

            return ErrorNumber.NoSuchFile;
        }

        if((_vFileParms.flags & FILE_FLAGS_HAS_PARENT) == FILE_FLAGS_HAS_PARENT &&
           _vParHdr.locatorType                        == _parentTypeVhdxGuid)
        {
            _parentImage = new Vhdx();
            var parentWorks = false;

            foreach(ParentLocatorEntry parentEntry in _vPars)
            {
                stream.Seek(parentEntry.keyOffset + _metadataOffset, SeekOrigin.Begin);
                var tmpKey = new byte[parentEntry.keyLength];
                stream.EnsureRead(tmpKey, 0, tmpKey.Length);
                string entryType = Encoding.Unicode.GetString(tmpKey);

                IFilter parentFilter;

                if(string.Compare(entryType, RELATIVE_PATH_KEY, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    stream.Seek(parentEntry.valueOffset + _metadataOffset, SeekOrigin.Begin);
                    var tmpVal = new byte[parentEntry.valueLength];
                    stream.EnsureRead(tmpVal, 0, tmpVal.Length);
                    string entryValue = Encoding.Unicode.GetString(tmpVal);

                    try
                    {
                        parentFilter =
                            PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, entryValue));

                        if(parentFilter != null && _parentImage.Open(parentFilter) == ErrorNumber.NoError)
                        {
                            parentWorks = true;

                            break;
                        }
                    }
                    catch
                    {
                        parentWorks = false;
                    }

                    string relEntry = Path.Combine(Path.GetDirectoryName(imageFilter.Path), entryValue);

                    try
                    {
                        parentFilter =
                            PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, relEntry));

                        if(parentFilter == null || _parentImage.Open(parentFilter) != ErrorNumber.NoError)
                            continue;

                        parentWorks = true;

                        break;
                    }
                    catch
                    {
                        // ignored
                    }
                }
                else if(string.Compare(entryType, VOLUME_PATH_KEY,         StringComparison.OrdinalIgnoreCase) == 0 ||
                        string.Compare(entryType, ABSOLUTE_WIN32_PATH_KEY, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    stream.Seek(parentEntry.valueOffset + _metadataOffset, SeekOrigin.Begin);
                    var tmpVal = new byte[parentEntry.valueLength];
                    stream.EnsureRead(tmpVal, 0, tmpVal.Length);
                    string entryValue = Encoding.Unicode.GetString(tmpVal);

                    try
                    {
                        parentFilter =
                            PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder, entryValue));

                        if(parentFilter == null || _parentImage.Open(parentFilter) != ErrorNumber.NoError)
                            continue;

                        parentWorks = true;

                        break;
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            if(!parentWorks)
            {
                AaruConsole.ErrorWriteLine(Localization.Image_is_differential_but_parent_cannot_be_opened);

                return ErrorNumber.InOutError;
            }

            _hasParent = true;
        }

        _chunkRatio = (long)(Math.Pow(2, 23) * _logicalSectorSize / _vFileParms.blockSize);
        _dataBlocks = _virtualDiskSize / _vFileParms.blockSize;

        if(_virtualDiskSize % _vFileParms.blockSize > 0)
            _dataBlocks++;

        long batEntries;

        if(_hasParent)
        {
            long sectorBitmapBlocks = (long)_dataBlocks / _chunkRatio;

            if(_dataBlocks % (ulong)_chunkRatio > 0)
                sectorBitmapBlocks++;

            _sectorBitmapPointers = new ulong[sectorBitmapBlocks];

            batEntries = sectorBitmapBlocks * (_chunkRatio - 1);
        }
        else
            batEntries = (long)(_dataBlocks + (_dataBlocks - 1) / (ulong)_chunkRatio);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_BAT);

        long readChunks = 0;
        _blockAllocationTable = new ulong[_dataBlocks];
        var batB = new byte[batEntries * 8];
        stream.Seek(_batOffset, SeekOrigin.Begin);
        stream.EnsureRead(batB, 0, batB.Length);

        ulong skipSize = 0;

        for(ulong i = 0; i < _dataBlocks; i++)
        {
            if(readChunks == _chunkRatio)
            {
                if(_hasParent)
                    _sectorBitmapPointers[skipSize / 8] = BitConverter.ToUInt64(batB, (int)(i * 8 + skipSize));

                readChunks =  0;
                skipSize   += 8;
            }
            else
            {
                _blockAllocationTable[i] = BitConverter.ToUInt64(batB, (int)(i * 8 + skipSize));
                readChunks++;
            }
        }

        if(_hasParent)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_Sector_Bitmap);

            var sectorBmpMs = new MemoryStream();

            foreach(ulong pt in _sectorBitmapPointers)
            {
                switch(pt & BAT_FLAGS_MASK)
                {
                    case SECTOR_BITMAP_NOT_PRESENT:
                        sectorBmpMs.Write(new byte[1048576], 0, 1048576);

                        break;
                    case SECTOR_BITMAP_PRESENT:
                        stream.Seek((long)((pt & BAT_FILE_OFFSET_MASK) * 1048576), SeekOrigin.Begin);
                        var bmp = new byte[1048576];
                        stream.EnsureRead(bmp, 0, bmp.Length);
                        sectorBmpMs.Write(bmp, 0, bmp.Length);

                        break;
                    default:
                        if((pt & BAT_FLAGS_MASK) != 0)
                        {
                            AaruConsole.
                                ErrorWriteLine(string.
                                                   Format(Localization.Unsupported_sector_bitmap_block_flags_0_found_not_proceeding,
                                                          pt & BAT_FLAGS_MASK));

                            return ErrorNumber.InvalidArgument;
                        }

                        break;
                }
            }

            _sectorBitmap = sectorBmpMs.ToArray();
            sectorBmpMs.Close();
        }

        _maxBlockCache  = (int)(MAX_CACHE_SIZE / _vFileParms.blockSize);
        _maxSectorCache = (int)(MAX_CACHE_SIZE / _logicalSectorSize);

        _imageStream = stream;

        _sectorCache = new Dictionary<ulong, byte[]>();
        _blockCache  = new Dictionary<ulong, byte[]>();

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.SectorSize           = _logicalSectorSize;
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.MediaType            = MediaType.GENERIC_HDD;
        _imageInfo.ImageSize            = _virtualDiskSize;
        _imageInfo.Sectors              = _imageInfo.ImageSize / _imageInfo.SectorSize;
        _imageInfo.DriveSerialNumber    = _page83Data.ToString();

        // TODO: Separate image application from version, need several samples.

        _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 16 / 63);
        _imageInfo.Heads           = 16;
        _imageInfo.SectorsPerTrack = 63;

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

        ulong index  = sectorAddress * _logicalSectorSize / _vFileParms.blockSize;
        ulong secOff = sectorAddress * _logicalSectorSize % _vFileParms.blockSize;

        ulong blkPtr   = _blockAllocationTable[index];
        ulong blkFlags = blkPtr & BAT_FLAGS_MASK;

        if((blkPtr & BAT_RESERVED_MASK) != 0)
        {
            AaruConsole.ErrorWriteLine($"Unknown flags (0x{blkPtr & BAT_RESERVED_MASK:X16}) set in block pointer");

            return ErrorNumber.InvalidArgument;
        }

        switch(blkFlags & BAT_FLAGS_MASK)
        {
            case PAYLOAD_BLOCK_NOT_PRESENT:
                if(_hasParent)
                    return _parentImage.ReadSector(sectorAddress, out buffer);

                buffer = new byte[_logicalSectorSize];

                return ErrorNumber.NoError;
            case PAYLOAD_BLOCK_UNDEFINED:
            case PAYLOAD_BLOCK_ZERO:
            case PAYLOAD_BLOCK_UNMAPPER:
                buffer = new byte[_logicalSectorSize];

                return ErrorNumber.NoError;
        }

        bool partialBlock = (blkFlags & BAT_FLAGS_MASK) == PAYLOAD_BLOCK_PARTIALLY_PRESENT;

        if(partialBlock && _hasParent && !CheckBitmap(sectorAddress))
            return _parentImage.ReadSector(sectorAddress, out buffer);

        if(!_blockCache.TryGetValue(blkPtr & BAT_FILE_OFFSET_MASK, out byte[] block))
        {
            block = new byte[_vFileParms.blockSize];
            _imageStream.Seek((long)(blkPtr & BAT_FILE_OFFSET_MASK), SeekOrigin.Begin);
            _imageStream.EnsureRead(block, 0, block.Length);

            if(_blockCache.Count >= _maxBlockCache)
                _blockCache.Clear();

            _blockCache.Add(blkPtr & BAT_FILE_OFFSET_MASK, block);
        }

        buffer = new byte[_logicalSectorSize];
        Array.Copy(block, (int)secOff, buffer, 0, buffer.Length);

        if(_sectorCache.Count >= _maxSectorCache)
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