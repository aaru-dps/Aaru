// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Lisa filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle Apple Lisa filesystem catalogs (aka directories).
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
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class LisaFS
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        // LisaFS does not support symbolic links (afaik)
        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;
        ErrorNumber error = LookupFileId(path, out short fileId, out bool isDir);

        if(error != ErrorNumber.NoError)
            return error;

        if(!isDir)
            return ErrorNumber.NotDirectory;

        /*List<CatalogEntry> catalog;
        error = ReadCatalog(fileId, out catalog);
        if(error != ErrorNumber.NoError)
            return error;*/

        ReadDir(fileId, out List<string> contents);

        // On debug add system files as readable files
        // Syntax similar to NTFS
        if(_debug && fileId == DIRID_ROOT)
        {
            contents.Add("$MDDF");
            contents.Add("$Boot");
            contents.Add("$Loader");
            contents.Add("$Bitmap");
            contents.Add("$S-Record");
            contents.Add("$");
        }

        contents.Sort();

        node = new LisaDirNode
        {
            Path     = path,
            Contents = contents.ToArray(),
            Position = 0
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(IDirNode node, out string filename)
    {
        filename = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(node is not LisaDirNode mynode)
            return ErrorNumber.InvalidArgument;

        if(mynode.Position < 0)
            return ErrorNumber.InvalidArgument;

        if(mynode.Position >= mynode.Contents.Length)
            return ErrorNumber.NoError;

        filename = mynode.Contents[mynode.Position++];

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseDir(IDirNode node)
    {
        if(node is not LisaDirNode mynode)
            return ErrorNumber.InvalidArgument;

        mynode.Position = -1;
        mynode.Contents = null;

        return ErrorNumber.NoError;
    }

#endregion

    void ReadDir(short dirId, out List<string> contents) =>

        // Do same trick as Mac OS X, replace filesystem '/' with '-',
        // as '-' is the path separator in Lisa OS
        contents = (from entry in _catalogCache
                    where entry.parentID == dirId
                    select StringHandlers.CToString(entry.filename, _encoding).Replace('/', '-')).ToList();

    /// <summary>Reads, interprets and caches the Catalog File</summary>
    ErrorNumber ReadCatalog()
    {
        ErrorNumber errno;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        _catalogCache = new List<CatalogEntry>();

        // Do differently for V1 and V2
        if(_mddf.fsversion is LISA_V2 or LISA_V1)
        {
            ErrorNumber error = ReadFile((short)FILEID_CATALOG, out byte[] buf);

            if(error != ErrorNumber.NoError)
                return error;

            var                  offset    = 0;
            List<CatalogEntryV2> catalogV2 = new();

            // For each entry on the catalog
            while(offset + 54 < buf.Length)
            {
                var entV2 = new CatalogEntryV2
                {
                    filenameLen = buf[offset],
                    filename    = new byte[E_NAME],
                    unknown1    = buf[offset                                + 0x21],
                    fileType    = buf[offset                                + 0x22],
                    unknown2    = buf[offset                                + 0x23],
                    fileID      = BigEndianBitConverter.ToInt16(buf, offset + 0x24),
                    unknown3    = new byte[16]
                };

                Array.Copy(buf, offset + 0x01, entV2.filename, 0, E_NAME);
                Array.Copy(buf, offset + 0x26, entV2.unknown3, 0, 16);

                offset += 54;

                // Check that the entry is correct, not empty or garbage
                if(entV2.filenameLen != 0 && entV2.filenameLen <= E_NAME && entV2.fileType != 0 && entV2.fileID > 0)
                    catalogV2.Add(entV2);
            }

            // Convert entries to V3 format
            foreach(CatalogEntryV2 entV2 in catalogV2)
            {
                error = ReadExtentsFile(entV2.fileID, out ExtentFile ext);

                if(error != ErrorNumber.NoError)
                    continue;

                var entV3 = new CatalogEntry
                {
                    fileID   = entV2.fileID,
                    filename = new byte[32],
                    fileType = entV2.fileType,
                    length   = (int)_srecords[entV2.fileID].filesize,
                    dtc      = ext.dtc,
                    dtm      = ext.dtm
                };

                Array.Copy(entV2.filename, 0, entV3.filename, 0, entV2.filenameLen);

                _catalogCache.Add(entV3);
            }

            return ErrorNumber.NoError;
        }

        byte[] firstCatalogBlock = null;

        // Search for the first sector describing the catalog
        // While root catalog is not stored in S-Records, probably rest are? (unchecked)
        // If root catalog is not pointed in MDDF (unchecked) maybe it's always following S-Records File?
        for(ulong i = 0; i < _device.Info.Sectors; i++)
        {
            errno = _device.ReadSectorTag(i, SectorTagType.AppleSectorTag, out byte[] tag);

            if(errno != ErrorNumber.NoError)
                continue;

            DecodeTag(tag, out LisaTag.PriamTag catTag);

            if(catTag.FileId != FILEID_CATALOG || catTag.RelPage != 0)
                continue;

            errno = _device.ReadSectors(i, 4, out firstCatalogBlock);

            if(errno != ErrorNumber.NoError)
                return errno;

            break;
        }

        // Catalog not found
        if(firstCatalogBlock == null)
            return ErrorNumber.NoSuchFile;

        ulong prevCatalogPointer = BigEndianBitConverter.ToUInt32(firstCatalogBlock, 0x7F6);

        // Traverse double-linked list until first catalog block
        while(prevCatalogPointer != 0xFFFFFFFF)
        {
            errno = _device.ReadSectorTag(prevCatalogPointer + _mddf.mddf_block + _volumePrefix,
                                          SectorTagType.AppleSectorTag, out byte[] tag);

            if(errno != ErrorNumber.NoError)
                return errno;

            DecodeTag(tag, out LisaTag.PriamTag prevTag);

            if(prevTag.FileId != FILEID_CATALOG)
                return ErrorNumber.InvalidArgument;

            errno = _device.ReadSectors(prevCatalogPointer + _mddf.mddf_block + _volumePrefix, 4,
                                        out firstCatalogBlock);

            if(errno != ErrorNumber.NoError)
                return errno;

            prevCatalogPointer = BigEndianBitConverter.ToUInt32(firstCatalogBlock, 0x7F6);
        }

        ulong nextCatalogPointer = BigEndianBitConverter.ToUInt32(firstCatalogBlock, 0x7FA);

        List<byte[]> catalogBlocks = new()
        {
            firstCatalogBlock
        };

        // Traverse double-linked list to read full catalog
        while(nextCatalogPointer != 0xFFFFFFFF)
        {
            errno = _device.ReadSectorTag(nextCatalogPointer + _mddf.mddf_block + _volumePrefix,
                                          SectorTagType.AppleSectorTag, out byte[] tag);

            if(errno != ErrorNumber.NoError)
                return errno;

            DecodeTag(tag, out LisaTag.PriamTag nextTag);

            if(nextTag.FileId != FILEID_CATALOG)
                return ErrorNumber.InvalidArgument;

            errno = _device.ReadSectors(nextCatalogPointer + _mddf.mddf_block + _volumePrefix, 4,
                                        out byte[] nextCatalogBlock);

            if(errno != ErrorNumber.NoError)
                return errno;

            nextCatalogPointer = BigEndianBitConverter.ToUInt32(nextCatalogBlock, 0x7FA);
            catalogBlocks.Add(nextCatalogBlock);
        }

        // Foreach catalog block
        foreach(byte[] buf in catalogBlocks)
        {
            var offset = 0;

            // Traverse all entries
            while(offset + 64 <= buf.Length)

                // Catalog block header
            {
                if(buf[offset + 0x24] == 0x08)
                    offset += 78;

                // Maybe just garbage? Found in more than 1 disk
                else if(buf[offset + 0x24] == 0x7C)
                    offset += 50;

                // Apparently reserved to indicate end of catalog?
                else if(buf[offset + 0x24] == 0xFF)
                    break;

                // Normal entry
                else if(buf[offset + 0x24] == 0x03 && buf[offset] == 0x24)
                {
                    var entry = new CatalogEntry
                    {
                        marker     = buf[offset],
                        parentID   = BigEndianBitConverter.ToUInt16(buf, offset + 0x01),
                        filename   = new byte[E_NAME],
                        terminator = buf[offset                                 + 0x23],
                        fileType   = buf[offset                                 + 0x24],
                        unknown    = buf[offset                                 + 0x25],
                        fileID     = BigEndianBitConverter.ToInt16(buf, offset  + 0x26),
                        dtc        = BigEndianBitConverter.ToUInt32(buf, offset + 0x28),
                        dtm        = BigEndianBitConverter.ToUInt32(buf, offset + 0x2C),
                        length     = BigEndianBitConverter.ToInt32(buf, offset  + 0x30),
                        wasted     = BigEndianBitConverter.ToInt32(buf, offset  + 0x34),
                        tail       = new byte[8]
                    };

                    Array.Copy(buf, offset + 0x03, entry.filename, 0, E_NAME);
                    Array.Copy(buf, offset + 0x38, entry.tail,     0, 8);

                    if(ReadExtentsFile(entry.fileID, out _) == ErrorNumber.NoError)
                    {
                        if(!_fileSizeCache.ContainsKey(entry.fileID))
                        {
                            _catalogCache.Add(entry);
                            _fileSizeCache.Add(entry.fileID, entry.length);
                        }
                    }

                    offset += 64;
                }

                // Subdirectory entry
                else if(buf[offset + 0x24] == 0x01 && buf[offset] == 0x24)
                {
                    var entry = new CatalogEntry
                    {
                        marker     = buf[offset],
                        parentID   = BigEndianBitConverter.ToUInt16(buf, offset + 0x01),
                        filename   = new byte[E_NAME],
                        terminator = buf[offset                                 + 0x23],
                        fileType   = buf[offset                                 + 0x24],
                        unknown    = buf[offset                                 + 0x25],
                        fileID     = BigEndianBitConverter.ToInt16(buf, offset  + 0x26),
                        dtc        = BigEndianBitConverter.ToUInt32(buf, offset + 0x28),
                        dtm        = BigEndianBitConverter.ToUInt32(buf, offset + 0x2C),
                        length     = 0,
                        wasted     = 0,
                        tail       = null
                    };

                    Array.Copy(buf, offset + 0x03, entry.filename, 0, E_NAME);

                    if(!_directoryDtcCache.ContainsKey(entry.fileID))
                        _directoryDtcCache.Add(entry.fileID, DateHandlers.LisaToDateTime(entry.dtc));

                    _catalogCache.Add(entry);

                    offset += 48;
                }
                else
                    break;
            }
        }

        return ErrorNumber.NoError;
    }

    ErrorNumber StatDir(short dirId, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        stat = new FileEntryInfo
        {
            Attributes = new FileAttributes(),
            Inode      = FILEID_CATALOG,
            Mode       = 0x16D,
            Links      = 0,
            UID        = 0,
            GID        = 0,
            DeviceNo   = 0,
            Length     = 0,
            BlockSize  = _mddf.datasize,
            Blocks     = 0
        };

        _directoryDtcCache.TryGetValue(dirId, out DateTime tmp);
        stat.CreationTime = tmp;

        return ErrorNumber.NoError;
    }
}