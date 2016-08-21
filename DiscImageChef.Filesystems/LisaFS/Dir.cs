// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.ImagePlugins;
using DiscImageChef.Decoders;

namespace DiscImageChef.Filesystems.LisaFS
{
    partial class LisaFS : Filesystem
    {
        /// <summary>
        /// Solves a symbolic link.
        /// </summary>
        /// <param name="path">Link path.</param>
        /// <param name="dest">Link destination.</param>
        public override Errno ReadLink(string path, ref string dest)
        {
            // LisaFS does not support symbolic links (afaik)
            return Errno.NotSupported;
        }

        /// <summary>
        /// Lists contents from a directory.
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <param name="contents">Directory contents.</param>
        public override Errno ReadDir(string path, ref List<string> contents)
        {
            short fileId;
            bool isDir;
            Errno error = LookupFileId(path, out fileId, out isDir);
            if(error != Errno.NoError)
                return error;

            if(!isDir)
                return Errno.NotDirectory;

            /*List<CatalogEntry> catalog;
            error = ReadCatalog(fileId, out catalog);
            if(error != Errno.NoError)
                return error;*/

            ReadDir(fileId, ref contents);

            // On debug add system files as readable files
            // Syntax similar to NTFS
            if(debug && fileId == DIRID_ROOT)
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

        Errno ReadDir(short dirId, ref List<string> contents)
        {
            contents = new List<string>();
            foreach(CatalogEntry entry in catalogCache)
            {
                if(entry.parentID == dirId)
                    // Do same trick as Mac OS X, replace filesystem '/' with '-',
                    // as '-' is the path separator in Lisa OS
                    contents.Add(GetString(entry.filename).Replace('/', '-'));
            }

            return Errno.NoError;
        }

        /// <summary>
        /// Reads, interprets and caches the Catalog File
        /// </summary>
        Errno ReadCatalog()
        {
            if(!mounted)
                return Errno.AccessDenied;

            catalogCache = new List<CatalogEntry>();

            Errno error;

            // Do differently for V1 and V2
            if(mddf.fsversion == LisaFSv2 || mddf.fsversion == LisaFSv1)
            {
                byte[] buf;
                error = ReadFile((short)FILEID_CATALOG, out buf);

                int offset = 0;
                List<CatalogEntryV2> catalogV2 = new List<CatalogEntryV2>();

                // For each entry on the catalog
                while(offset + 54 < buf.Length)
                {
                    CatalogEntryV2 entV2 = new CatalogEntryV2();
                    entV2.filenameLen = buf[offset];
                    entV2.filename = new byte[E_NAME];
                    Array.Copy(buf, offset + 0x01, entV2.filename, 0, E_NAME);
                    entV2.unknown1 = buf[offset + 0x21];
                    entV2.fileType = buf[offset + 0x22];
                    entV2.unknown2 = buf[offset + 0x23];
                    entV2.fileID = BigEndianBitConverter.ToInt16(buf, offset + 0x24);
                    entV2.unknown3 = new byte[16];
                    Array.Copy(buf, offset + 0x26, entV2.unknown3, 0, 16);

                    offset += 54;

                    // Check that the entry is correct, not empty or garbage
                    if(entV2.filenameLen != 0 && entV2.filenameLen <= E_NAME && entV2.fileType != 0 && entV2.fileID > 0)
                        catalogV2.Add(entV2);
                }

                // Convert entries to V3 format
                foreach(CatalogEntryV2 entV2 in catalogV2)
                {
                    ExtentFile ext;
                    error = ReadExtentsFile(entV2.fileID, out ext);
                    if(error == Errno.NoError)
                    {
                        CatalogEntry entV3 = new CatalogEntry();
                        entV3.fileID = entV2.fileID;
                        entV3.filename = new byte[32];
                        Array.Copy(entV2.filename, 0, entV3.filename, 0, entV2.filenameLen);
                        entV3.fileType = entV2.fileType;
                        entV3.length = (int)srecords[entV2.fileID].filesize;
                        entV3.dtc = ext.dtc;
                        entV3.dtm = ext.dtm;

                        catalogCache.Add(entV3);
                    }
                }

                return Errno.NoError;
            }

            byte[] firstCatalogBlock = null;

            // Search for the first sector describing the catalog
            // While root catalog is not stored in S-Records, probably rest are? (unchecked)
            // If root catalog is not pointed in MDDF (unchecked) maybe it's always following S-Records File?
            for(ulong i = 0; i < device.GetSectors(); i++)
            {
                LisaTag.PriamTag catTag;
                DecodeTag(device.ReadSectorTag(i, SectorTagType.AppleSectorTag), out catTag);

                if(catTag.fileID == FILEID_CATALOG && catTag.relPage == 0)
                {
                    firstCatalogBlock = device.ReadSectors(i, 4);
                    break;
                }
            }

            // Catalog not found
            if(firstCatalogBlock == null)
                return Errno.NoSuchFile;

            ulong prevCatalogPointer;
            prevCatalogPointer = BigEndianBitConverter.ToUInt32(firstCatalogBlock, 0x7F6);

            // Traverse double-linked list until first catalog block
            while(prevCatalogPointer != 0xFFFFFFFF)
            {
                LisaTag.PriamTag prevTag;
                DecodeTag(device.ReadSectorTag(prevCatalogPointer + mddf.mddf_block + volumePrefix, SectorTagType.AppleSectorTag), out prevTag);

                if(prevTag.fileID != FILEID_CATALOG)
                    return Errno.InvalidArgument;

                firstCatalogBlock = device.ReadSectors(prevCatalogPointer + mddf.mddf_block + volumePrefix, 4);
                prevCatalogPointer = BigEndianBitConverter.ToUInt32(firstCatalogBlock, 0x7F6);
            }

            ulong nextCatalogPointer;
            nextCatalogPointer = BigEndianBitConverter.ToUInt32(firstCatalogBlock, 0x7FA);

            List<byte[]> catalogBlocks = new List<byte[]>();
            catalogBlocks.Add(firstCatalogBlock);

            // Traverse double-linked list to read full catalog
            while(nextCatalogPointer != 0xFFFFFFFF)
            {
                LisaTag.PriamTag nextTag;
                DecodeTag(device.ReadSectorTag(nextCatalogPointer + mddf.mddf_block + volumePrefix, SectorTagType.AppleSectorTag), out nextTag);

                if(nextTag.fileID != FILEID_CATALOG)
                    return Errno.InvalidArgument;

                byte[] nextCatalogBlock = device.ReadSectors(nextCatalogPointer + mddf.mddf_block + volumePrefix, 4);
                nextCatalogPointer = BigEndianBitConverter.ToUInt32(nextCatalogBlock, 0x7FA);
                catalogBlocks.Add(nextCatalogBlock);
            }

            // Foreach catalog block
            foreach(byte[] buf in catalogBlocks)
            {
                int offset = 0;

                // Traverse all entries
                while((offset + 64) <= buf.Length)
                {
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
                    else if(buf[offset + 0x24] == 0x03 && buf[offset] == 0x24)
                    {
                        CatalogEntry entry = new CatalogEntry();
                        entry.marker = buf[offset];
                        entry.parentID = BigEndianBitConverter.ToUInt16(buf, offset + 0x01);
                        entry.filename = new byte[E_NAME];
                        Array.Copy(buf, offset + 0x03, entry.filename, 0, E_NAME);
                        entry.terminator = buf[offset + 0x23];
                        entry.fileType = buf[offset + 0x24];
                        entry.unknown = buf[offset + 0x25];
                        entry.fileID = BigEndianBitConverter.ToInt16(buf, offset + 0x26);
                        entry.dtc = BigEndianBitConverter.ToUInt32(buf, offset + 0x28);
                        entry.dtm = BigEndianBitConverter.ToUInt32(buf, offset + 0x2C);
                        entry.length = BigEndianBitConverter.ToInt32(buf, offset + 0x30);
                        entry.wasted = BigEndianBitConverter.ToInt32(buf, offset + 0x34);
                        entry.tail = new byte[8];
                        Array.Copy(buf, offset + 0x38, entry.tail, 0, 8);

                        ExtentFile ext;
                        if(ReadExtentsFile(entry.fileID, out ext) == Errno.NoError)
                        {
                            if(!fileSizeCache.ContainsKey(entry.fileID))
                            {
                                catalogCache.Add(entry);
                                fileSizeCache.Add(entry.fileID, entry.length);
                            }
                        }

                        offset += 64;
                    }
                    // Subdirectory entry
                    else if(buf[offset + 0x24] == 0x01 && buf[offset] == 0x24)
                    {
                        CatalogEntry entry = new CatalogEntry();
                        entry.marker = buf[offset];
                        entry.parentID = BigEndianBitConverter.ToUInt16(buf, offset + 0x01);
                        entry.filename = new byte[E_NAME];
                        Array.Copy(buf, offset + 0x03, entry.filename, 0, E_NAME);
                        entry.terminator = buf[offset + 0x23];
                        entry.fileType = buf[offset + 0x24];
                        entry.unknown = buf[offset + 0x25];
                        entry.fileID = BigEndianBitConverter.ToInt16(buf, offset + 0x26);
                        entry.dtc = BigEndianBitConverter.ToUInt32(buf, offset + 0x28);
                        entry.dtm = BigEndianBitConverter.ToUInt32(buf, offset + 0x2C);
                        entry.length = 0;
                        entry.wasted = 0;
                        entry.tail = null;

                        if(!directoryDTCCache.ContainsKey(entry.fileID))
                            directoryDTCCache.Add(entry.fileID, DateHandlers.LisaToDateTime(entry.dtc));

                        catalogCache.Add(entry);

                        offset += 48;
                    }
                    else
                        break;
                }
            }

            return Errno.NoError;
        }

        Errno StatDir(short dirId, out FileEntryInfo stat)
        {
            stat = null;

            if(!mounted)
                return Errno.AccessDenied;

            stat = new FileEntryInfo();
            stat.Attributes = new FileAttributes();
            DateTime tmp = new DateTime();

            directoryDTCCache.TryGetValue(dirId, out tmp);
            stat.CreationTime = tmp;
            stat.Inode = FILEID_CATALOG;
            stat.Mode = 0x16D;
            stat.Links = 0;
            stat.UID = 0;
            stat.GID = 0;
            stat.DeviceNo = 0;
            stat.Length = 0;
            stat.BlockSize = mddf.datasize;
            stat.Blocks = 0;

            return Errno.NoError;
        }
    }
}