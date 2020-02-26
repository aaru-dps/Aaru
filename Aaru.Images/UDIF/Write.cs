// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Write.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Writes Apple Universal Disk Image Format.
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
using System.Linq;
using System.Text;
using Claunia.PropertyList;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public partial class Udif
    {
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
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

            try { writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            chunks           = new Dictionary<ulong, BlockChunk>();
            currentChunk     = new BlockChunk();
            currentSector    = 0;
            dataForkChecksum = new Crc32Context();
            masterChecksum   = new Crc32Context();

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

            if(data.Length != imageInfo.SectorSize)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress >= imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            if(sectorAddress < currentSector)
            {
                ErrorMessage = "Tried to rewind, this format rewinded on writing";
                return false;
            }

            masterChecksum.Update(data);

            bool isEmpty = ArrayHelpers.ArrayIsNullOrEmpty(data);

            switch(currentChunk.type)
            {
                case CHUNK_TYPE_ZERO:
                    currentChunk.type = isEmpty ? CHUNK_TYPE_NOCOPY : CHUNK_TYPE_COPY;
                    break;
                case CHUNK_TYPE_NOCOPY when !isEmpty:
                case CHUNK_TYPE_COPY when isEmpty:
                    chunks.Add(currentChunk.sector, currentChunk);
                    currentChunk = new BlockChunk
                    {
                        type   = isEmpty ? CHUNK_TYPE_NOCOPY : CHUNK_TYPE_COPY,
                        sector = currentSector,
                        offset = (ulong)(isEmpty ? 0 : writingStream.Position)
                    };
                    break;
            }

            currentChunk.sectors++;
            currentChunk.length += (ulong)(isEmpty ? 0 : 512);
            currentSector++;

            if(!isEmpty)
            {
                dataForkChecksum.Update(data);
                writingStream.Write(data, 0, data.Length);
            }

            ErrorMessage = "";
            return true;
        }

        // TODO: This can be optimized
        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(data.Length % imageInfo.SectorSize != 0)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(sectorAddress + length > imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            // Ignore empty sectors
            if(ArrayHelpers.ArrayIsNullOrEmpty(data))
            {
                if(currentChunk.type == CHUNK_TYPE_COPY)
                {
                    chunks.Add(currentChunk.sector, currentChunk);
                    currentChunk = new BlockChunk {type = CHUNK_TYPE_NOCOPY, sector = currentSector};
                }

                currentChunk.sectors += (ulong)(data.Length / imageInfo.SectorSize);
                currentSector        += (ulong)(data.Length / imageInfo.SectorSize);
                masterChecksum.Update(data);

                ErrorMessage = "";
                return true;
            }

            for(uint i = 0; i < length; i++)
            {
                byte[] tmp = new byte[imageInfo.SectorSize];
                Array.Copy(data, i * imageInfo.SectorSize, tmp, 0, imageInfo.SectorSize);
                if(!WriteSector(tmp, sectorAddress + i)) return false;
            }

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

        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";
                return false;
            }

            if(currentChunk.type != CHUNK_TYPE_NOCOPY) currentChunk.length = currentChunk.sectors * 512;
            chunks.Add(currentChunk.sector, currentChunk);

            chunks.Add(imageInfo.Sectors, new BlockChunk {type = CHUNK_TYPE_END, sector = imageInfo.Sectors});

            BlockHeader bHdr = new BlockHeader
            {
                signature    = CHUNK_SIGNATURE,
                version      = 1,
                sectorCount  = imageInfo.Sectors,
                checksumType = UDIF_CHECKSUM_TYPE_CRC32,
                checksumLen  = 32,
                checksum     = BitConverter.ToUInt32(dataForkChecksum.Final().Reverse().ToArray(), 0),
                chunks       = (uint)chunks.Count
            };

            MemoryStream chunkMs = new MemoryStream();
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.signature),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.version),      0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.sectorStart),  0, 8);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.sectorCount),  0, 8);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.dataOffset),   0, 8);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.buffers),      0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.descriptor),   0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved1),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved2),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved3),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved4),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved5),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved6),    0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.checksumType), 0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.checksumLen),  0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.checksum),     0, 4);
            chunkMs.Write(new byte[124],                                     0, 124);
            chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.chunks),       0, 4);

            foreach(BlockChunk chunk in chunks.Values)
            {
                chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.type),    0, 4);
                chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.comment), 0, 4);
                chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.sector),  0, 8);
                chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.sectors), 0, 8);
                chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.offset),  0, 8);
                chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.length),  0, 8);
            }

            byte[] plist = Encoding.UTF8.GetBytes(new NSDictionary
            {
                {
                    "resource-fork",
                    new NSDictionary
                    {
                        {
                            "blkx",
                            new NSArray
                            {
                                new NSDictionary
                                {
                                    {"Attributes", "0x0050"},
                                    {
                                        "CFName",
                                        "whole disk (DiscImageChef : 0)"
                                    },
                                    {"Data", chunkMs.ToArray()},
                                    {"ID", "0"},
                                    {
                                        "Name",
                                        "whole disk (DiscImageChef : 0)"
                                    }
                                }
                            }
                        }
                    }
                }
            }.ToXmlPropertyList());

            footer = new UdifFooter
            {
                signature       = UDIF_SIGNATURE,
                version         = 4,
                headerSize      = 512,
                flags           = 1,
                dataForkLen     = (ulong)writingStream.Length,
                segmentNumber   = 1,
                segmentCount    = 1,
                segmentId       = Guid.NewGuid(),
                dataForkChkType = UDIF_CHECKSUM_TYPE_CRC32,
                dataForkChkLen  = 32,
                dataForkChk     = BitConverter.ToUInt32(dataForkChecksum.Final().Reverse().ToArray(), 0),
                plistOff        = (ulong)writingStream.Length,
                plistLen        = (ulong)plist.Length,
                // TODO: Find how is this calculated
                /*masterChkType   = 2,
                masterChkLen    = 32,
                masterChk       = BitConverter.ToUInt32(masterChecksum.Final().Reverse().ToArray(), 0),*/
                imageVariant = 2,
                sectorCount  = imageInfo.Sectors
            };

            writingStream.Seek(0, SeekOrigin.End);
            writingStream.Write(plist,                                                     0, plist.Length);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.signature),          0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.version),            0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.headerSize),         0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.flags),              0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.runningDataForkOff), 0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.dataForkOff),        0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.dataForkLen),        0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.rsrcForkOff),        0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.rsrcForkLen),        0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.segmentNumber),      0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.segmentCount),       0, 4);
            writingStream.Write(footer.segmentId.ToByteArray(),                            0, 16);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.dataForkChkType),    0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.dataForkChkLen),     0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.dataForkChk),        0, 4);
            writingStream.Write(new byte[124],                                             0, 124);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.plistOff),           0, 8);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.plistLen),           0, 8);
            writingStream.Write(new byte[120],                                             0, 120);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.masterChkType),      0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.masterChkLen),       0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.masterChk),          0, 4);
            writingStream.Write(new byte[124],                                             0, 124);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.imageVariant),       0, 4);
            writingStream.Write(BigEndianBitConverter.GetBytes(footer.sectorCount),        0, 8);
            writingStream.Write(new byte[12],                                              0, 12);

            writingStream.Flush();
            writingStream.Close();

            IsWriting    = false;
            ErrorMessage = "";
            return true;
        }

        // TODO: Comments
        public bool SetMetadata(ImageInfo metadata) => true;

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";
            return false;
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";
            return false;
        }

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

        public bool SetCicmMetadata(CICMMetadataType metadata) => false;
    }
}