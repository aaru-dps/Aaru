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

namespace DiscImageChef.Filesystems.LisaFS
{
    partial class LisaFS : Filesystem
    {
        public override Errno ReadLink(string path, ref string dest)
        {
            // LisaFS does not support symbolic links (afaik)
            return Errno.NotSupported;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            Int16 fileId;
            bool isDir;
            Errno error = LookupFileId(path, out fileId, out isDir);
            if(error != Errno.NoError)
                return error;

            if(!isDir)
                return Errno.NotDirectory;

            List<CatalogEntry> catalog;
            ReadCatalog(fileId, out catalog);

            foreach(CatalogEntry entry in catalog)
                contents.Add(GetString(entry.filename).Replace('/',':'));

            if(debug && fileId == FILEID_DIRECTORY)
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

        Errno ReadCatalog(Int16 fileId, out List<CatalogEntry> catalog)
        {
            catalog = null;

            if(!mounted)
                return Errno.AccessDenied;

            if(fileId < 4)
                return Errno.InvalidArgument;

            if(catalogCache.TryGetValue(fileId, out catalog))
                return Errno.NoError;

            Errno error;

            if(mddf.fsversion == LisaFSv2 || mddf.fsversion == LisaFSv1)
            {
                if(fileId != FILEID_DIRECTORY)
                {
                    ExtentFile ext;
                    error = ReadExtentsFile(fileId, out ext);
                    if(error == Errno.NoError)
                        return Errno.NotDirectory;
                }

                byte[] buf;
                error = ReadFile(4, out buf);

                int offset = 0;
                List<CatalogEntryV2> catalogV2 = new List<CatalogEntryV2>();

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

                    if(entV2.filenameLen != 0 && entV2.filenameLen <= E_NAME && entV2.fileType != 0 && entV2.fileID > 0)
                        catalogV2.Add(entV2);
                }

                catalog = new List<CatalogEntry>();

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

                        catalog.Add(entV3);
                    }
                }

                catalogCache.Add(fileId, catalog);
                return Errno.NoError;
            }

            byte[] firstCatalogBlock = null;

            for(ulong i = 0; i < device.GetSectors(); i++)
            {
                Tag catTag;
                DecodeTag(device.ReadSectorTag(i, SectorTagType.AppleSectorTag), out catTag);

                if(catTag.fileID == fileId && catTag.relBlock == 0)
                {
                    firstCatalogBlock = device.ReadSectors(i, 4);
                    break;
                }

                if(catTag.fileID == -fileId)
                    return Errno.NotDirectory;
            }

            if(firstCatalogBlock == null)
                return Errno.NoSuchFile;

            ulong prevCatalogPointer;
            prevCatalogPointer = BigEndianBitConverter.ToUInt32(firstCatalogBlock, 0x7F6);

            while(prevCatalogPointer != 0xFFFFFFFF)
            {
                Tag prevTag;
                DecodeTag(device.ReadSectorTag(prevCatalogPointer + mddf.mddf_block + volumePrefix, SectorTagType.AppleSectorTag), out prevTag);

                if(prevTag.fileID != fileId)
                    return Errno.InvalidArgument;

                firstCatalogBlock = device.ReadSectors(prevCatalogPointer + mddf.mddf_block + volumePrefix, 4);
                prevCatalogPointer = BigEndianBitConverter.ToUInt32(firstCatalogBlock, 0x7F6);
            }

            ulong nextCatalogPointer;
            nextCatalogPointer = BigEndianBitConverter.ToUInt32(firstCatalogBlock, 0x7FA);

            List<byte[]> catalogBlocks = new List<byte[]>();
            catalogBlocks.Add(firstCatalogBlock);

            while(nextCatalogPointer != 0xFFFFFFFF)
            {
                Tag nextTag;
                DecodeTag(device.ReadSectorTag(nextCatalogPointer + mddf.mddf_block + volumePrefix, SectorTagType.AppleSectorTag), out nextTag);

                if(nextTag.fileID != fileId)
                    return Errno.InvalidArgument;

                byte[] nextCatalogBlock = device.ReadSectors(nextCatalogPointer + mddf.mddf_block + volumePrefix, 4);
                nextCatalogPointer = BigEndianBitConverter.ToUInt32(nextCatalogBlock, 0x7FA);
                catalogBlocks.Add(nextCatalogBlock);
            }

            catalog = new List<CatalogEntry>();

            foreach(byte[] buf in catalogBlocks)
            {
                int offset = 0;

                while((offset + 64) <= buf.Length)
                {
                    if(buf[offset + 0x24] == 0x08)
                        offset += 78;
                    else if(buf[offset + 0x24] == 0x7C)
                        offset += 50;
                    else if(buf[offset + 0x24] == 0xFF)
                        break;
                    else if(buf[offset + 0x24] == 0x03 && buf[offset] == 0x24)
                    {
                        CatalogEntry entry = new CatalogEntry();
                        entry.marker = buf[offset];
                        entry.zero = BigEndianBitConverter.ToUInt16(buf, offset + 0x01);
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

                        // This is as Pascal Workshop does, if there is no extents file it simply ignores it.
                        ExtentFile ext;
                        if(ReadExtentsFile(entry.fileID, out ext) == Errno.NoError)
                        {
                            if(!fileSizeCache.ContainsKey(entry.fileID))
                            {
                                catalog.Add(entry);
                                fileSizeCache.Add(entry.fileID, entry.length);
                            }
                        }

                        offset += 64;
                    }
                    else
                        break;
                }
            }

            catalogCache.Add(fileId, catalog);
            return Errno.NoError;
        }
    }
}

