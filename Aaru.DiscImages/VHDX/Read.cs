// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Helpers;

namespace DiscImageChef.DiscImages
{
    public partial class Vhdx
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < 512) return false;

            byte[] vhdxIdB = new byte[Marshal.SizeOf<VhdxIdentifier>()];
            stream.Read(vhdxIdB, 0, Marshal.SizeOf<VhdxIdentifier>());
            vhdxId = Marshal.ByteArrayToStructureLittleEndian<VhdxIdentifier>(vhdxIdB);

            if(vhdxId.signature != VHDX_SIGNATURE) return false;

            imageInfo.Application = Encoding.Unicode.GetString(vhdxId.creator);

            stream.Seek(64 * 1024, SeekOrigin.Begin);
            byte[] vHdrB = new byte[Marshal.SizeOf<VhdxHeader>()];
            stream.Read(vHdrB, 0, Marshal.SizeOf<VhdxHeader>());
            vHdr = Marshal.ByteArrayToStructureLittleEndian<VhdxHeader>(vHdrB);

            if(vHdr.Signature != VHDX_HEADER_SIG)
            {
                stream.Seek(128 * 1024, SeekOrigin.Begin);
                vHdrB = new byte[Marshal.SizeOf<VhdxHeader>()];
                stream.Read(vHdrB, 0, Marshal.SizeOf<VhdxHeader>());
                vHdr = Marshal.ByteArrayToStructureLittleEndian<VhdxHeader>(vHdrB);

                if(vHdr.Signature != VHDX_HEADER_SIG) throw new ImageNotSupportedException("VHDX header not found");
            }

            stream.Seek(192 * 1024, SeekOrigin.Begin);
            byte[] vRegTableB = new byte[Marshal.SizeOf<VhdxRegionTableHeader>()];
            stream.Read(vRegTableB, 0, Marshal.SizeOf<VhdxRegionTableHeader>());
            vRegHdr = Marshal.ByteArrayToStructureLittleEndian<VhdxRegionTableHeader>(vRegTableB);

            if(vRegHdr.signature != VHDX_REGION_SIG)
            {
                stream.Seek(256 * 1024, SeekOrigin.Begin);
                vRegTableB = new byte[Marshal.SizeOf<VhdxRegionTableHeader>()];
                stream.Read(vRegTableB, 0, Marshal.SizeOf<VhdxRegionTableHeader>());
                vRegHdr = Marshal.ByteArrayToStructureLittleEndian<VhdxRegionTableHeader>(vRegTableB);

                if(vRegHdr.signature != VHDX_REGION_SIG)
                    throw new ImageNotSupportedException("VHDX region table not found");
            }

            vRegs = new VhdxRegionTableEntry[vRegHdr.entries];
            for(int i = 0; i < vRegs.Length; i++)
            {
                byte[] vRegB = new byte[System.Runtime.InteropServices.Marshal.SizeOf(vRegs[i])];
                stream.Read(vRegB, 0, System.Runtime.InteropServices.Marshal.SizeOf(vRegs[i]));
                vRegs[i] = Marshal.ByteArrayToStructureLittleEndian<VhdxRegionTableEntry>(vRegB);

                if(vRegs[i].guid == batGuid)
                    batOffset = (long)vRegs[i].offset;
                else if(vRegs[i].guid == metadataGuid)
                    metadataOffset = (long)vRegs[i].offset;
                else if((vRegs[i].flags & REGION_FLAGS_REQUIRED) == REGION_FLAGS_REQUIRED)
                    throw new
                        ImageNotSupportedException($"Found unsupported and required region Guid {vRegs[i].guid}, not proceeding with image.");
            }

            if(batOffset == 0) throw new Exception("BAT not found, cannot continue.");

            if(metadataOffset == 0) throw new Exception("Metadata not found, cannot continue.");

            uint fileParamsOff = 0, vdSizeOff = 0, p83Off = 0, logOff = 0, physOff = 0, parentOff = 0;

            stream.Seek(metadataOffset, SeekOrigin.Begin);
            byte[] metTableB = new byte[Marshal.SizeOf<VhdxMetadataTableHeader>()];
            stream.Read(metTableB, 0, Marshal.SizeOf<VhdxMetadataTableHeader>());
            vMetHdr = Marshal.ByteArrayToStructureLittleEndian<VhdxMetadataTableHeader>(metTableB);

            vMets = new VhdxMetadataTableEntry[vMetHdr.entries];
            for(int i = 0; i < vMets.Length; i++)
            {
                byte[] vMetB = new byte[System.Runtime.InteropServices.Marshal.SizeOf(vMets[i])];
                stream.Read(vMetB, 0, System.Runtime.InteropServices.Marshal.SizeOf(vMets[i]));
                vMets[i] = Marshal.ByteArrayToStructureLittleEndian<VhdxMetadataTableEntry>(vMetB);

                if(vMets[i].itemId == fileParametersGuid)
                    fileParamsOff = vMets[i].offset;
                else if(vMets[i].itemId == virtualDiskSizeGuid)
                    vdSizeOff = vMets[i].offset;
                else if(vMets[i].itemId == page83DataGuid)
                    p83Off = vMets[i].offset;
                else if(vMets[i].itemId == logicalSectorSizeGuid)
                    logOff = vMets[i].offset;
                else if(vMets[i].itemId == physicalSectorSizeGuid)
                    physOff = vMets[i].offset;
                else if(vMets[i].itemId == parentLocatorGuid)
                    parentOff = vMets[i].offset;
                else if((vMets[i].flags & METADATA_FLAGS_REQUIRED) == METADATA_FLAGS_REQUIRED)
                    throw new
                        ImageNotSupportedException($"Found unsupported and required metadata Guid {vMets[i].itemId}, not proceeding with image.");
            }

            byte[] tmp;

            if(fileParamsOff != 0)
            {
                stream.Seek(fileParamsOff + metadataOffset, SeekOrigin.Begin);
                tmp = new byte[8];
                stream.Read(tmp, 0, 8);
                vFileParms = new VhdxFileParameters
                {
                    blockSize = BitConverter.ToUInt32(tmp, 0), flags = BitConverter.ToUInt32(tmp, 4)
                };
            }
            else throw new Exception("File parameters not found.");

            if(vdSizeOff != 0)
            {
                stream.Seek(vdSizeOff + metadataOffset, SeekOrigin.Begin);
                tmp = new byte[8];
                stream.Read(tmp, 0, 8);
                virtualDiskSize = BitConverter.ToUInt64(tmp, 0);
            }
            else throw new Exception("Virtual disk size not found.");

            if(p83Off != 0)
            {
                stream.Seek(p83Off + metadataOffset, SeekOrigin.Begin);
                tmp = new byte[16];
                stream.Read(tmp, 0, 16);
                page83Data = new Guid(tmp);
            }

            if(logOff != 0)
            {
                stream.Seek(logOff + metadataOffset, SeekOrigin.Begin);
                tmp = new byte[4];
                stream.Read(tmp, 0, 4);
                logicalSectorSize = BitConverter.ToUInt32(tmp, 0);
            }
            else throw new Exception("Logical sector size not found.");

            if(physOff != 0)
            {
                stream.Seek(physOff + metadataOffset, SeekOrigin.Begin);
                tmp = new byte[4];
                stream.Read(tmp, 0, 4);
                physicalSectorSize = BitConverter.ToUInt32(tmp, 0);
            }
            else throw new Exception("Physical sector size not found.");

            if(parentOff != 0 && (vFileParms.flags & FILE_FLAGS_HAS_PARENT) == FILE_FLAGS_HAS_PARENT)
            {
                stream.Seek(parentOff + metadataOffset, SeekOrigin.Begin);
                byte[] vParHdrB = new byte[Marshal.SizeOf<VhdxParentLocatorHeader>()];
                stream.Read(vParHdrB, 0, Marshal.SizeOf<VhdxParentLocatorHeader>());
                vParHdr = Marshal.ByteArrayToStructureLittleEndian<VhdxParentLocatorHeader>(vParHdrB);

                if(vParHdr.locatorType != parentTypeVhdxGuid)
                    throw new
                        ImageNotSupportedException($"Found unsupported and required parent locator type {vParHdr.locatorType}, not proceeding with image.");

                vPars = new VhdxParentLocatorEntry[vParHdr.keyValueCount];
                for(int i = 0; i < vPars.Length; i++)
                {
                    byte[] vParB = new byte[System.Runtime.InteropServices.Marshal.SizeOf(vPars[i])];
                    stream.Read(vParB, 0, System.Runtime.InteropServices.Marshal.SizeOf(vPars[i]));
                    vPars[i] = Marshal.ByteArrayToStructureLittleEndian<VhdxParentLocatorEntry>(vParB);
                }
            }
            else if((vFileParms.flags & FILE_FLAGS_HAS_PARENT) == FILE_FLAGS_HAS_PARENT)
                throw new Exception("Parent locator not found.");

            if((vFileParms.flags & FILE_FLAGS_HAS_PARENT) == FILE_FLAGS_HAS_PARENT &&
               vParHdr.locatorType                        == parentTypeVhdxGuid)
            {
                parentImage = new Vhdx();
                bool parentWorks = false;

                foreach(VhdxParentLocatorEntry parentEntry in vPars)
                {
                    stream.Seek(parentEntry.keyOffset + metadataOffset, SeekOrigin.Begin);
                    byte[] tmpKey = new byte[parentEntry.keyLength];
                    stream.Read(tmpKey, 0, tmpKey.Length);
                    string entryType = Encoding.Unicode.GetString(tmpKey);

                    IFilter parentFilter;
                    if(string.Compare(entryType, RELATIVE_PATH_KEY, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        stream.Seek(parentEntry.valueOffset + metadataOffset, SeekOrigin.Begin);
                        byte[] tmpVal = new byte[parentEntry.valueLength];
                        stream.Read(tmpVal, 0, tmpVal.Length);
                        string entryValue = Encoding.Unicode.GetString(tmpVal);

                        try
                        {
                            parentFilter =
                                new FiltersList().GetFilter(Path.Combine(imageFilter.GetParentFolder(), entryValue));
                            if(parentFilter != null && parentImage.Open(parentFilter))
                            {
                                parentWorks = true;
                                break;
                            }
                        }
                        catch { parentWorks = false; }

                        string relEntry = Path.Combine(Path.GetDirectoryName(imageFilter.GetPath()), entryValue);

                        try
                        {
                            parentFilter =
                                new FiltersList().GetFilter(Path.Combine(imageFilter.GetParentFolder(), relEntry));
                            if(parentFilter == null || !parentImage.Open(parentFilter)) continue;

                            parentWorks = true;
                            break;
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    else if(string.Compare(entryType, VOLUME_PATH_KEY, StringComparison.OrdinalIgnoreCase) ==
                            0 ||
                            string.Compare(entryType, ABSOLUTE_WIN32_PATH_KEY, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        stream.Seek(parentEntry.valueOffset + metadataOffset, SeekOrigin.Begin);
                        byte[] tmpVal = new byte[parentEntry.valueLength];
                        stream.Read(tmpVal, 0, tmpVal.Length);
                        string entryValue = Encoding.Unicode.GetString(tmpVal);

                        try
                        {
                            parentFilter =
                                new FiltersList().GetFilter(Path.Combine(imageFilter.GetParentFolder(), entryValue));
                            if(parentFilter == null || !parentImage.Open(parentFilter)) continue;

                            parentWorks = true;
                            break;
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                if(!parentWorks) throw new Exception("Image is differential but parent cannot be opened.");

                hasParent = true;
            }

            chunkRatio = (long)(Math.Pow(2, 23) * logicalSectorSize / vFileParms.blockSize);
            dataBlocks = virtualDiskSize / vFileParms.blockSize;
            if(virtualDiskSize           % vFileParms.blockSize > 0) dataBlocks++;

            long batEntries;
            if(hasParent)
            {
                long sectorBitmapBlocks = (long)dataBlocks / chunkRatio;
                if(dataBlocks % (ulong)chunkRatio > 0) sectorBitmapBlocks++;
                sectorBitmapPointers = new ulong[sectorBitmapBlocks];

                batEntries = sectorBitmapBlocks * (chunkRatio - 1);
            }
            else batEntries = (long)(dataBlocks + (dataBlocks - 1) / (ulong)chunkRatio);

            DicConsole.DebugWriteLine("VHDX plugin", "Reading BAT");

            long readChunks = 0;
            blockAllocationTable = new ulong[dataBlocks];
            byte[] batB = new byte[batEntries * 8];
            stream.Seek(batOffset, SeekOrigin.Begin);
            stream.Read(batB, 0, batB.Length);

            ulong skipSize = 0;
            for(ulong i = 0; i < dataBlocks; i++)
                if(readChunks == chunkRatio)
                {
                    if(hasParent)
                        sectorBitmapPointers[skipSize / 8] = BitConverter.ToUInt64(batB, (int)(i * 8 + skipSize));

                    readChunks =  0;
                    skipSize   += 8;
                }
                else
                {
                    blockAllocationTable[i] = BitConverter.ToUInt64(batB, (int)(i * 8 + skipSize));
                    readChunks++;
                }

            if(hasParent)
            {
                DicConsole.DebugWriteLine("VHDX plugin", "Reading Sector Bitmap");

                MemoryStream sectorBmpMs = new MemoryStream();
                foreach(ulong pt in sectorBitmapPointers)
                    switch(pt & BAT_FLAGS_MASK)
                    {
                        case SECTOR_BITMAP_NOT_PRESENT:
                            sectorBmpMs.Write(new byte[1048576], 0, 1048576);
                            break;
                        case SECTOR_BITMAP_PRESENT:
                            stream.Seek((long)((pt & BAT_FILE_OFFSET_MASK) * 1048576), SeekOrigin.Begin);
                            byte[] bmp = new byte[1048576];
                            stream.Read(bmp, 0, bmp.Length);
                            sectorBmpMs.Write(bmp, 0, bmp.Length);
                            break;
                        default:
                            if((pt & BAT_FLAGS_MASK) != 0)
                                throw new
                                    ImageNotSupportedException($"Unsupported sector bitmap block flags (0x{pt & BAT_FLAGS_MASK:X16}) found, not proceeding.");

                            break;
                    }

                sectorBitmap = sectorBmpMs.ToArray();
                sectorBmpMs.Close();
            }

            maxBlockCache  = (int)(MAX_CACHE_SIZE / vFileParms.blockSize);
            maxSectorCache = (int)(MAX_CACHE_SIZE / logicalSectorSize);

            imageStream = stream;

            sectorCache = new Dictionary<ulong, byte[]>();
            blockCache  = new Dictionary<ulong, byte[]>();

            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            imageInfo.SectorSize           = logicalSectorSize;
            imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            imageInfo.MediaType            = MediaType.GENERIC_HDD;
            imageInfo.ImageSize            = virtualDiskSize;
            imageInfo.Sectors              = imageInfo.ImageSize / imageInfo.SectorSize;
            imageInfo.DriveSerialNumber    = page83Data.ToString();

            // TODO: Separate image application from version, need several samples.

            imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
            imageInfo.Heads           = 16;
            imageInfo.SectorsPerTrack = 63;

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorCache.TryGetValue(sectorAddress, out byte[] sector)) return sector;

            ulong index  = sectorAddress * logicalSectorSize / vFileParms.blockSize;
            ulong secOff = sectorAddress * logicalSectorSize % vFileParms.blockSize;

            ulong blkPtr   = blockAllocationTable[index];
            ulong blkFlags = blkPtr & BAT_FLAGS_MASK;

            if((blkPtr & BAT_RESERVED_MASK) != 0)
                throw new
                    ImageNotSupportedException($"Unknown flags (0x{blkPtr & BAT_RESERVED_MASK:X16}) set in block pointer");

            switch(blkFlags & BAT_FLAGS_MASK)
            {
                case PAYLOAD_BLOCK_NOT_PRESENT:
                    return hasParent ? parentImage.ReadSector(sectorAddress) : new byte[logicalSectorSize];
                case PAYLOAD_BLOCK_UNDEFINED:
                case PAYLOAD_BLOCK_ZERO:
                case PAYLOAD_BLOCK_UNMAPPER: return new byte[logicalSectorSize];
            }

            bool partialBlock;
            partialBlock = (blkFlags & BAT_FLAGS_MASK) == PAYLOAD_BLOCK_PARTIALLY_PRESENT;

            if(partialBlock && hasParent && !CheckBitmap(sectorAddress)) return parentImage.ReadSector(sectorAddress);

            if(!blockCache.TryGetValue(blkPtr & BAT_FILE_OFFSET_MASK, out byte[] block))
            {
                block = new byte[vFileParms.blockSize];
                imageStream.Seek((long)(blkPtr & BAT_FILE_OFFSET_MASK), SeekOrigin.Begin);
                imageStream.Read(block, 0, block.Length);

                if(blockCache.Count >= maxBlockCache) blockCache.Clear();

                blockCache.Add(blkPtr & BAT_FILE_OFFSET_MASK, block);
            }

            sector = new byte[logicalSectorSize];
            Array.Copy(block, (int)secOff, sector, 0, sector.Length);

            if(sectorCache.Count >= maxSectorCache) sectorCache.Clear();

            sectorCache.Add(sectorAddress, sector);

            return sector;
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                      $"Sector address {sectorAddress} not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({sectorAddress + length}) than available ({imageInfo.Sectors})");

            MemoryStream ms = new MemoryStream();

            for(uint i = 0; i < length; i++)
            {
                byte[] sector = ReadSector(sectorAddress + i);
                ms.Write(sector, 0, sector.Length);
            }

            return ms.ToArray();
        }
    }
}