// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Claunia.PropertyList;

namespace Aaru.DiscImages;

public sealed partial class Udif
{
    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                       uint sectorSize)
    {
        if(sectorSize != 512)
        {
            ErrorMessage = Localization.Unsupported_sector_size;

            return false;
        }

        if(!SupportedMediaTypes.Contains(mediaType))
        {
            ErrorMessage = string.Format(Localization.Unsupported_media_format_0, mediaType);

            return false;
        }

        _imageInfo = new ImageInfo
        {
            MediaType  = mediaType,
            SectorSize = sectorSize,
            Sectors    = sectors
        };

        try
        {
            _writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch(IOException e)
        {
            ErrorMessage = string.Format(Localization.Could_not_create_new_image_file_exception_0, e.Message);

            return false;
        }

        _chunks           = new Dictionary<ulong, BlockChunk>();
        _currentChunk     = new BlockChunk();
        _currentSector    = 0;
        _dataForkChecksum = new Crc32Context();
        _masterChecksum   = new Crc32Context();

        IsWriting    = true;
        ErrorMessage = null;

        return true;
    }

    /// <inheritdoc />
    public bool WriteMediaTag(byte[] data, MediaTagType tag)
    {
        ErrorMessage = Localization.Writing_media_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSector(byte[] data, ulong sectorAddress)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(data.Length != _imageInfo.SectorSize)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        if(sectorAddress >= _imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        if(sectorAddress < _currentSector)
        {
            ErrorMessage = Localization.Tried_to_rewind_this_format_rewinded_on_writing;

            return false;
        }

        _masterChecksum.Update(data);

        bool isEmpty = ArrayHelpers.ArrayIsNullOrEmpty(data);

        switch(_currentChunk.type)
        {
            case CHUNK_TYPE_ZERO:
                _currentChunk.type = isEmpty ? CHUNK_TYPE_NOCOPY : CHUNK_TYPE_COPY;

                break;
            case CHUNK_TYPE_NOCOPY when !isEmpty:
            case CHUNK_TYPE_COPY when isEmpty:
                _chunks.Add(_currentChunk.sector, _currentChunk);

                _currentChunk = new BlockChunk
                {
                    type   = isEmpty ? CHUNK_TYPE_NOCOPY : CHUNK_TYPE_COPY,
                    sector = _currentSector,
                    offset = (ulong)(isEmpty ? 0 : _writingStream.Position)
                };

                break;
        }

        _currentChunk.sectors++;
        _currentChunk.length += (ulong)(isEmpty ? 0 : 512);
        _currentSector++;

        if(!isEmpty)
        {
            _dataForkChecksum.Update(data);
            _writingStream.Write(data, 0, data.Length);
        }

        ErrorMessage = "";

        return true;
    }

    // TODO: This can be optimized
    /// <inheritdoc />
    public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Tried_to_write_on_a_non_writable_image;

            return false;
        }

        if(data.Length % _imageInfo.SectorSize != 0)
        {
            ErrorMessage = Localization.Incorrect_data_size;

            return false;
        }

        if(sectorAddress + length > _imageInfo.Sectors)
        {
            ErrorMessage = Localization.Tried_to_write_past_image_size;

            return false;
        }

        // Ignore empty sectors
        if(ArrayHelpers.ArrayIsNullOrEmpty(data))
        {
            if(_currentChunk.type == CHUNK_TYPE_COPY)
            {
                _chunks.Add(_currentChunk.sector, _currentChunk);

                _currentChunk = new BlockChunk
                {
                    type   = CHUNK_TYPE_NOCOPY,
                    sector = _currentSector
                };
            }

            _currentChunk.sectors += (ulong)(data.Length / _imageInfo.SectorSize);
            _currentSector        += (ulong)(data.Length / _imageInfo.SectorSize);
            _masterChecksum.Update(data);

            ErrorMessage = "";

            return true;
        }

        for(uint i = 0; i < length; i++)
        {
            byte[] tmp = new byte[_imageInfo.SectorSize];
            Array.Copy(data, i * _imageInfo.SectorSize, tmp, 0, _imageInfo.SectorSize);

            if(!WriteSector(tmp, sectorAddress + i))
                return false;
        }

        ErrorMessage = "";

        return true;
    }

    /// <inheritdoc />
    public bool WriteSectorLong(byte[] data, ulong sectorAddress)
    {
        ErrorMessage = Localization.Writing_sectors_with_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
    {
        ErrorMessage = Localization.Writing_sectors_with_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool Close()
    {
        if(!IsWriting)
        {
            ErrorMessage = Localization.Image_is_not_opened_for_writing;

            return false;
        }

        if(_currentChunk.type != CHUNK_TYPE_NOCOPY)
            _currentChunk.length = _currentChunk.sectors * 512;

        _chunks.Add(_currentChunk.sector, _currentChunk);

        _chunks.Add(_imageInfo.Sectors, new BlockChunk
        {
            type   = CHUNK_TYPE_END,
            sector = _imageInfo.Sectors
        });

        var bHdr = new BlockHeader
        {
            signature    = CHUNK_SIGNATURE,
            version      = 1,
            sectorCount  = _imageInfo.Sectors,
            checksumType = UDIF_CHECKSUM_TYPE_CRC32,
            checksumLen  = 32,
            checksum     = BitConverter.ToUInt32(_dataForkChecksum.Final().Reverse().ToArray(), 0),
            chunks       = (uint)_chunks.Count
        };

        var chunkMs = new MemoryStream();
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.signature), 0, 4);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.version), 0, 4);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.sectorStart), 0, 8);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.sectorCount), 0, 8);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.dataOffset), 0, 8);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.buffers), 0, 4);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.descriptor), 0, 4);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved1), 0, 4);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved2), 0, 4);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved3), 0, 4);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved4), 0, 4);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved5), 0, 4);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.reserved6), 0, 4);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.checksumType), 0, 4);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.checksumLen), 0, 4);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.checksum), 0, 4);
        chunkMs.Write(new byte[124], 0, 124);
        chunkMs.Write(BigEndianBitConverter.GetBytes(bHdr.chunks), 0, 4);

        foreach(BlockChunk chunk in _chunks.Values)
        {
            chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.type), 0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.comment), 0, 4);
            chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.sector), 0, 8);
            chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.sectors), 0, 8);
            chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.offset), 0, 8);
            chunkMs.Write(BigEndianBitConverter.GetBytes(chunk.length), 0, 8);
        }

        byte[] plist = Encoding.UTF8.GetBytes(new NSDictionary
        {
            {
                "resource-fork", new NSDictionary
                {
                    {
                        "blkx", new NSArray
                        {
                            new NSDictionary
                            {
                                {
                                    "Attributes", "0x0050"
                                },
                                {
                                    "CFName", "whole disk (Aaru : 0)"
                                },
                                {
                                    "Data", chunkMs.ToArray()
                                },
                                {
                                    "ID", "0"
                                },
                                {
                                    "Name", "whole disk (Aaru : 0)"
                                }
                            }
                        }
                    }
                }
            }
        }.ToXmlPropertyList());

        _footer = new Footer
        {
            signature       = UDIF_SIGNATURE,
            version         = 4,
            headerSize      = 512,
            flags           = 1,
            dataForkLen     = (ulong)_writingStream.Length,
            segmentNumber   = 1,
            segmentCount    = 1,
            segmentId       = Guid.NewGuid(),
            dataForkChkType = UDIF_CHECKSUM_TYPE_CRC32,
            dataForkChkLen  = 32,
            dataForkChk     = BitConverter.ToUInt32(_dataForkChecksum.Final().Reverse().ToArray(), 0),
            plistOff        = (ulong)_writingStream.Length,
            plistLen        = (ulong)plist.Length,

            // TODO: Find how is this calculated
            /*masterChkType   = 2,
            masterChkLen    = 32,
            masterChk       = BitConverter.ToUInt32(masterChecksum.Final().Reverse().ToArray(), 0),*/
            imageVariant = 2,
            sectorCount  = _imageInfo.Sectors
        };

        _writingStream.Seek(0, SeekOrigin.End);
        _writingStream.Write(plist, 0, plist.Length);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.signature), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.version), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.headerSize), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.flags), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.runningDataForkOff), 0, 8);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.dataForkOff), 0, 8);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.dataForkLen), 0, 8);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.rsrcForkOff), 0, 8);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.rsrcForkLen), 0, 8);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.segmentNumber), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.segmentCount), 0, 4);
        _writingStream.Write(_footer.segmentId.ToByteArray(), 0, 16);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.dataForkChkType), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.dataForkChkLen), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.dataForkChk), 0, 4);
        _writingStream.Write(new byte[124], 0, 124);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.plistOff), 0, 8);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.plistLen), 0, 8);
        _writingStream.Write(new byte[120], 0, 120);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.masterChkType), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.masterChkLen), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.masterChk), 0, 4);
        _writingStream.Write(new byte[124], 0, 124);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.imageVariant), 0, 4);
        _writingStream.Write(BigEndianBitConverter.GetBytes(_footer.sectorCount), 0, 8);
        _writingStream.Write(new byte[12], 0, 12);

        _writingStream.Flush();
        _writingStream.Close();

        IsWriting    = false;
        ErrorMessage = "";

        return true;
    }

    // TODO: Comments
    /// <inheritdoc />
    public bool SetImageInfo(ImageInfo imageInfo) => true;

    /// <inheritdoc />
    public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

    /// <inheritdoc />
    public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
    {
        ErrorMessage = Localization.Writing_sectors_with_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
    {
        ErrorMessage = Localization.Writing_sectors_with_tags_is_not_supported;

        return false;
    }

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardware> dumpHardware) => false;

    /// <inheritdoc />
    public bool SetMetadata(Metadata metadata) => false;
}