// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Verify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Verifies Aaru Format disk images.
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
using Aaru.Checksums;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class AaruFormat
{
#region IVerifiableImage Members

    /// <inheritdoc />
    public bool? VerifyMediaImage()
    {
        // This will traverse all blocks and check their CRC64 without uncompressing them
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Checking_index_integrity_at_0, _header.indexOffset);

        _imageStream.Position = (long)_header.indexOffset;

        _structureBytes = new byte[Marshal.SizeOf<IndexHeader>()];
        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
        IndexHeader idxHeader = Marshal.SpanToStructureLittleEndian<IndexHeader>(_structureBytes);

        if(idxHeader.identifier != BlockType.Index)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Incorrect_index_identifier);

            return false;
        }

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   Localization.Index_at_0_contains_1_entries,
                                   _header.indexOffset,
                                   idxHeader.entries);

        _structureBytes = new byte[Marshal.SizeOf<IndexEntry>() * idxHeader.entries];
        _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
        Crc64Context.Data(_structureBytes, out byte[] verifyCrc);

        if(BitConverter.ToUInt64(verifyCrc, 0) != idxHeader.crc64)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       Localization.Expected_index_CRC_0_X16_but_got_1_X16,
                                       idxHeader.crc64,
                                       BitConverter.ToUInt64(verifyCrc, 0));

            return false;
        }

        _imageStream.Position -= _structureBytes.Length;

        List<IndexEntry> vrIndex = new();

        for(ushort i = 0; i < idxHeader.entries; i++)
        {
            _structureBytes = new byte[Marshal.SizeOf<IndexEntry>()];
            _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
            IndexEntry entry = Marshal.SpanToStructureLittleEndian<IndexEntry>(_structureBytes);

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       Localization.Block_type_0_with_data_type_1_is_indexed_to_be_at_2,
                                       entry.blockType,
                                       entry.dataType,
                                       entry.offset);

            vrIndex.Add(entry);
        }

        // Read up to 1MiB at a time for verification
        const int verifySize = 1024 * 1024;

        foreach(IndexEntry entry in vrIndex)
        {
            _imageStream.Position = (long)entry.offset;
            Crc64Context crcVerify;
            ulong        readBytes;
            byte[]       verifyBytes;

            switch(entry.blockType)
            {
                case BlockType.DataBlock:
                    _structureBytes = new byte[Marshal.SizeOf<BlockHeader>()];
                    _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
                    BlockHeader blockHeader = Marshal.SpanToStructureLittleEndian<BlockHeader>(_structureBytes);

                    crcVerify = new Crc64Context();
                    readBytes = 0;

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Verifying_data_block_type_0_at_position_1,
                                               entry.dataType,
                                               entry.offset);

                    while(readBytes + verifySize < blockHeader.cmpLength)
                    {
                        verifyBytes = new byte[verifySize];
                        _imageStream.EnsureRead(verifyBytes, 0, verifyBytes.Length);
                        crcVerify.Update(verifyBytes);
                        readBytes += (ulong)verifyBytes.LongLength;
                    }

                    verifyBytes = new byte[blockHeader.cmpLength - readBytes];
                    _imageStream.EnsureRead(verifyBytes, 0, verifyBytes.Length);
                    crcVerify.Update(verifyBytes);

                    verifyCrc = crcVerify.Final();

                    if(BitConverter.ToUInt64(verifyCrc, 0) != blockHeader.cmpCrc64)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Expected_block_CRC_0_X16_but_got_1_X16,
                                                   blockHeader.cmpCrc64,
                                                   BitConverter.ToUInt64(verifyCrc, 0));

                        return false;
                    }

                    break;
                case BlockType.DeDuplicationTable:
                    _structureBytes = new byte[Marshal.SizeOf<DdtHeader>()];
                    _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
                    DdtHeader ddtHeader = Marshal.SpanToStructureLittleEndian<DdtHeader>(_structureBytes);

                    crcVerify = new Crc64Context();
                    readBytes = 0;

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Verifying_deduplication_table_type_0_at_position_1,
                                               entry.dataType,
                                               entry.offset);

                    while(readBytes + verifySize < ddtHeader.cmpLength)
                    {
                        verifyBytes = new byte[verifySize];
                        _imageStream.EnsureRead(verifyBytes, 0, verifyBytes.Length);
                        crcVerify.Update(verifyBytes);
                        readBytes += (ulong)verifyBytes.LongLength;
                    }

                    verifyBytes = new byte[ddtHeader.cmpLength - readBytes];
                    _imageStream.EnsureRead(verifyBytes, 0, verifyBytes.Length);
                    crcVerify.Update(verifyBytes);

                    verifyCrc = crcVerify.Final();

                    if(BitConverter.ToUInt64(verifyCrc, 0) != ddtHeader.cmpCrc64)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Expected_DDT_CRC_0_but_got_1,
                                                   ddtHeader.cmpCrc64,
                                                   BitConverter.ToUInt64(verifyCrc, 0));

                        return false;
                    }

                    break;
                case BlockType.TracksBlock:
                    _structureBytes = new byte[Marshal.SizeOf<TracksHeader>()];
                    _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
                    TracksHeader trkHeader = Marshal.SpanToStructureLittleEndian<TracksHeader>(_structureBytes);

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Track_block_at_0_contains_1_entries,
                                               _header.indexOffset,
                                               trkHeader.entries);

                    _structureBytes = new byte[Marshal.SizeOf<TrackEntry>() * trkHeader.entries];
                    _imageStream.EnsureRead(_structureBytes, 0, _structureBytes.Length);
                    Crc64Context.Data(_structureBytes, out verifyCrc);

                    if(BitConverter.ToUInt64(verifyCrc, 0) != trkHeader.crc64)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Expected_index_CRC_0_X16_but_got_1_X16,
                                                   trkHeader.crc64,
                                                   BitConverter.ToUInt64(verifyCrc, 0));

                        return false;
                    }

                    break;
                default:
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Ignored_field_type_0, entry.blockType);

                    break;
            }
        }

        return true;
    }

#endregion

#region IWritableOpticalImage Members

    /// <inheritdoc />
    public bool? VerifySector(ulong sectorAddress)
    {
        if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc) return null;

        ErrorNumber errno = ReadSectorLong(sectorAddress, out byte[] buffer);

        return errno != ErrorNumber.NoError ? null : CdChecksums.CheckCdSector(buffer);
    }

    /// <inheritdoc />
    public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                               out List<ulong> unknownLbas)
    {
        failingLbas = new List<ulong>();
        unknownLbas = new List<ulong>();

        // Right now only CompactDisc sectors are verifiable
        if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc)
        {
            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        ErrorNumber errno = ReadSectorsLong(sectorAddress, length, out byte[] buffer);

        if(errno != ErrorNumber.NoError) return null;

        var bps    = (int)(buffer.Length / length);
        var sector = new byte[bps];
        failingLbas = new List<ulong>();
        unknownLbas = new List<ulong>();

        for(var i = 0; i < length; i++)
        {
            Array.Copy(buffer, i * bps, sector, 0, bps);
            bool? sectorStatus = CdChecksums.CheckCdSector(sector);

            switch(sectorStatus)
            {
                case null:
                    unknownLbas.Add((ulong)i + sectorAddress);

                    break;
                case false:
                    failingLbas.Add((ulong)i + sectorAddress);

                    break;
            }
        }

        if(unknownLbas.Count > 0) return null;

        return failingLbas.Count <= 0;
    }

    /// <inheritdoc />
    public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                               out List<ulong> unknownLbas)
    {
        // Right now only CompactDisc sectors are verifiable
        if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(ulong i = sectorAddress; i < sectorAddress + length; i++) unknownLbas.Add(i);

            return null;
        }

        failingLbas = new List<ulong>();
        unknownLbas = new List<ulong>();

        ErrorNumber errno = ReadSectorsLong(sectorAddress, length, track, out byte[] buffer);

        if(errno != ErrorNumber.NoError) return null;

        var bps    = (int)(buffer.Length / length);
        var sector = new byte[bps];

        for(var i = 0; i < length; i++)
        {
            Array.Copy(buffer, i * bps, sector, 0, bps);
            bool? sectorStatus = CdChecksums.CheckCdSector(sector);

            switch(sectorStatus)
            {
                case null:
                    unknownLbas.Add((ulong)i + sectorAddress);

                    break;
                case false:
                    failingLbas.Add((ulong)i + sectorAddress);

                    break;
            }
        }

        if(unknownLbas.Count > 0) return null;

        return failingLbas.Count <= 0;
    }

#endregion
}