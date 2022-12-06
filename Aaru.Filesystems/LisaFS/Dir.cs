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
using Aaru.CommonTypes.Structs;
using Aaru.Decoders;
using Aaru.Helpers;

namespace Aaru.Filesystems.LisaFS
{
    public sealed partial class LisaFS
    {
        /// <inheritdoc />
        public Errno ReadLink(string path, out string dest)
        {
            dest = null;

            // LisaFS does not support symbolic links (afaik)
            return Errno.NotSupported;
        }

        /// <inheritdoc />
        public Errno ReadDir(string path, out List<string> contents)
        {
            contents = null;
            Errno error = LookupFileId(path, out short fileId, out bool isDir);

            if(error != Errno.NoError)
                return error;

            if(!isDir)
                return Errno.NotDirectory;

            /*List<CatalogEntry> catalog;
            error = ReadCatalog(fileId, out catalog);
            if(error != Errno.NoError)
                return error;*/

            ReadDir(fileId, out contents);

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

            return Errno.NoError;
        }

        void ReadDir(short dirId, out List<string> contents) =>

            // Do same trick as Mac OS X, replace filesystem '/' with '-',
            // as '-' is the path separator in Lisa OS
            contents = (from entry in _catalogCache where entry.parentID == dirId
                        select StringHandlers.CToString(entry.filename, Encoding).Replace('/', '-')).ToList();

        /// <summary>Reads, interprets and caches the Catalog File</summary>
        Errno ReadCatalog()
        {
            if(!_mounted)
                return Errno.AccessDenied;

            _catalogCache = new List<CatalogEntry>();

            // Do differently for V1 and V2
            if(_mddf.fsversion == LISA_V2 ||
               _mddf.fsversion == LISA_V1)
            {
                Errno error = ReadFile((short)FILEID_CATALOG, out byte[] buf);

                if(error != Errno.NoError)
                    return error;

                int                  offset    = 0;
                List<CatalogEntryV2> catalogV2 = new List<CatalogEntryV2>();

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
                    if(entV2.filenameLen != 0      &&
                       entV2.filenameLen <= E_NAME &&
                       entV2.fileType    != 0      &&
                       entV2.fileID      > 0)
                        catalogV2.Add(entV2);
                }

                // Convert entries to V3 format
                foreach(CatalogEntryV2 entV2 in catalogV2)
                {
                    error = ReadExtentsFile(entV2.fileID, out ExtentFile ext);

                    if(error != Errno.NoError)
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

                return Errno.NoError;
            }

            byte[] firstCatalogBlock = null;

            // Search for the first sector describing the catalog
            // While root catalog is not stored in S-Records, probably rest are? (unchecked)
            // If root catalog is not pointed in MDDF (unchecked) maybe it's always following S-Records File?
            for(ulong i = 0; i < _device.Info.Sectors; i++)
            {
                DecodeTag(_device.ReadSectorTag(i, SectorTagType.AppleSectorTag), out LisaTag.PriamTag catTag);

                if(catTag.FileId  != FILEID_CATALOG ||
                   catTag.RelPage != 0)
                    continue;

                firstCatalogBlock = _device.ReadSectors(i, 4);

                break;
            }

            // Catalog not found
            if(firstCatalogBlock == null)
                return Errno.NoSuchFile;

            ulong prevCatalogPointer = BigEndianBitConverter.ToUInt32(firstCatalogBlock, 0x7F6);

            // Traverse double-linked list until first catalog block
            while(prevCatalogPointer != 0xFFFFFFFF)
            {
                DecodeTag(_device.ReadSectorTag(prevCatalogPointer + _mddf.mddf_block + _volumePrefix, SectorTagType.AppleSectorTag),
                          out LisaTag.PriamTag prevTag);

                if(prevTag.FileId != FILEID_CATALOG)
                    return Errno.InvalidArgument;

                firstCatalogBlock  = _device.ReadSectors(prevCatalogPointer + _mddf.mddf_block + _volumePrefix, 4);
                prevCatalogPointer = BigEndianBitConverter.ToUInt32(firstCatalogBlock, 0x7F6);
            }

            ulong nextCatalogPointer = BigEndianBitConverter.ToUInt32(firstCatalogBlock, 0x7FA);

            List<byte[]> catalogBlocks = new List<byte[]>
            {
                firstCatalogBlock
            };

            // Traverse double-linked list to read full catalog
            while(nextCatalogPointer != 0xFFFFFFFF)
            {
                DecodeTag(_device.ReadSectorTag(nextCatalogPointer + _mddf.mddf_block + _volumePrefix, SectorTagType.AppleSectorTag),
                          out LisaTag.PriamTag nextTag);

                if(nextTag.FileId != FILEID_CATALOG)
                    return Errno.InvalidArgument;

                byte[] nextCatalogBlock = _device.ReadSectors(nextCatalogPointer + _mddf.mddf_block + _volumePrefix, 4);
                nextCatalogPointer = BigEndianBitConverter.ToUInt32(nextCatalogBlock, 0x7FA);
                catalogBlocks.Add(nextCatalogBlock);
            }

            // Foreach catalog block
            foreach(byte[] buf in catalogBlocks)
            {
                int offset = 0;

                // Traverse all entries
                while(offset + 64 <= buf.Length)

                    // Catalog block header
                    if(buf[offset + 0x24] == 0x08)
                        offset += 78;

                    // Maybe just garbage? Found in more than 1 disk
                    else if(buf[offset + 0x24] == 0x7C)
                        offset += 50;

                    // Apparently reserved to indicate end of catalog?
                    else if(buf[offset + 0x24] == 0xFF)
                        break;

                    // Normal entry
                    else if(buf[offset + 0x24] == 0x03 &&
                            buf[offset]        == 0x24)
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
                        Array.Copy(buf, offset + 0x38, entry.tail, 0, 8);

                        if(ReadExtentsFile(entry.fileID, out _) == Errno.NoError)
                            if(!_fileSizeCache.ContainsKey(entry.fileID))
                            {
                                _catalogCache.Add(entry);
                                _fileSizeCache.Add(entry.fileID, entry.length);
                            }

                        offset += 64;
                    }

                    // Subdirectory entry
                    else if(buf[offset + 0x24] == 0x01 &&
                            buf[offset]        == 0x24)
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

            return Errno.NoError;
        }

        Errno StatDir(short dirId, out FileEntryInfo stat)
        {
            stat = null;

            if(!_mounted)
                return Errno.AccessDenied;

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

            return Errno.NoError;
        }
    }
}