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
//     Reads Sydex CopyQM disk images.
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
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class CopyQm
{
    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        byte[] hdr = new byte[133];

        stream.EnsureRead(hdr, 0, 133);
        _header = Marshal.ByteArrayToStructureLittleEndian<Header>(hdr);

        AaruConsole.DebugWriteLine("CopyQM plugin", "header.magic = 0x{0:X4}", _header.magic);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.mark = 0x{0:X2}", _header.mark);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.sectorSize = {0}", _header.sectorSize);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.sectorPerCluster = {0}", _header.sectorPerCluster);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.reservedSectors = {0}", _header.reservedSectors);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.fatCopy = {0}", _header.fatCopy);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.rootEntries = {0}", _header.rootEntries);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.sectors = {0}", _header.sectors);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.mediaType = 0x{0:X2}", _header.mediaType);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.sectorsPerFat = {0}", _header.sectorsPerFat);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.sectorsPerTrack = {0}", _header.sectorsPerTrack);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.heads = {0}", _header.heads);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.hidden = {0}", _header.hidden);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.sectorsBig = {0}", _header.sectorsBig);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.description = {0}", _header.description);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.blind = {0}", _header.blind);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.density = {0}", _header.density);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.imageCylinders = {0}", _header.imageCylinders);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.totalCylinders = {0}", _header.totalCylinders);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.crc = 0x{0:X8}", _header.crc);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.volumeLabel = {0}", _header.volumeLabel);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.time = 0x{0:X4}", _header.time);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.date = 0x{0:X4}", _header.date);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.commentLength = {0}", _header.commentLength);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.secbs = {0}", _header.secbs);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.unknown = 0x{0:X4}", _header.unknown);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.interleave = {0}", _header.interleave);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.skew = {0}", _header.skew);
        AaruConsole.DebugWriteLine("CopyQM plugin", "header.drive = {0}", _header.drive);

        byte[] cmt = new byte[_header.commentLength];
        stream.EnsureRead(cmt, 0, _header.commentLength);
        _imageInfo.Comments = StringHandlers.CToString(cmt);
        _decodedImage       = new MemoryStream();

        _calculatedDataCrc = 0;

        while(stream.Position + 2 < stream.Length)
        {
            byte[] runLengthBytes = new byte[2];

            if(stream.EnsureRead(runLengthBytes, 0, 2) != 2)
                break;

            short runLength = BitConverter.ToInt16(runLengthBytes, 0);

            switch(runLength)
            {
                case < 0:
                {
                    byte   repeatedByte  = (byte)stream.ReadByte();
                    byte[] repeatedArray = new byte[runLength * -1];
                    ArrayHelpers.ArrayFill(repeatedArray, repeatedByte);

                    for(int i = 0; i < runLength * -1; i++)
                    {
                        _decodedImage.WriteByte(repeatedByte);

                        _calculatedDataCrc = _copyQmCrcTable[(repeatedByte ^ _calculatedDataCrc) & 0x3F] ^
                                             (_calculatedDataCrc >> 8);
                    }

                    break;
                }
                case > 0:
                {
                    byte[] nonRepeated = new byte[runLength];
                    stream.EnsureRead(nonRepeated, 0, runLength);
                    _decodedImage.Write(nonRepeated, 0, runLength);

                    foreach(byte c in nonRepeated)
                        _calculatedDataCrc =
                            _copyQmCrcTable[(c ^ _calculatedDataCrc) & 0x3F] ^ (_calculatedDataCrc >> 8);

                    break;
                }
            }
        }

        // In case there is omitted data
        long sectors = _header.sectorsPerTrack * _header.heads * _header.totalCylinders;

        long fillingLen = (sectors * _header.sectorSize) - _decodedImage.Length;

        if(fillingLen > 0)
        {
            byte[] filling = new byte[fillingLen];
            ArrayHelpers.ArrayFill(filling, (byte)0xF6);
            _decodedImage.Write(filling, 0, filling.Length);
        }

        int sum = 0;

        for(int i = 0; i < hdr.Length - 1; i++)
            sum += hdr[i];

        _headerChecksumOk = ((-1 * sum) & 0xFF) == _header.headerChecksum;

        AaruConsole.DebugWriteLine("CopyQM plugin", Localization.Calculated_header_checksum_equals_0_X2_1,
                                   (-1 * sum) & 0xFF, _headerChecksumOk);

        AaruConsole.DebugWriteLine("CopyQM plugin", Localization.Calculated_data_CRC_equals_0_X8_1, _calculatedDataCrc,
                                   _calculatedDataCrc == _header.crc);

        _imageInfo.Application          = "CopyQM";
        _imageInfo.CreationTime         = DateHandlers.DosToDateTime(_header.date, _header.time);
        _imageInfo.LastModificationTime = _imageInfo.CreationTime;
        _imageInfo.MediaTitle           = _header.volumeLabel;
        _imageInfo.ImageSize            = (ulong)(stream.Length - 133 - _header.commentLength);
        _imageInfo.Sectors              = (ulong)sectors;
        _imageInfo.SectorSize           = _header.sectorSize;

        _imageInfo.MediaType = Geometry.GetMediaType((_header.totalCylinders, (byte)_header.heads,
                                                      _header.sectorsPerTrack, _header.sectorSize, MediaEncoding.MFM,
                                                      false));

        switch(_imageInfo.MediaType)
        {
            case MediaType.NEC_525_HD when _header.drive is COPYQM_35_HD or COPYQM_35_ED:
                _imageInfo.MediaType = MediaType.NEC_35_HD_8;

                break;
            case MediaType.DOS_525_HD when _header.drive is COPYQM_35_HD or COPYQM_35_ED:
                _imageInfo.MediaType = MediaType.NEC_35_HD_15;

                break;
            case MediaType.RX50 when _header.drive is COPYQM_525_DD or COPYQM_525_HD:
                _imageInfo.MediaType = MediaType.ATARI_35_SS_DD;

                break;
        }

        _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;
        _decodedDisk                 = _decodedImage.ToArray();

        _decodedImage.Close();

        AaruConsole.VerboseWriteLine(Localization.CopyQM_image_contains_a_disk_of_type_0, _imageInfo.MediaType);

        if(!string.IsNullOrEmpty(_imageInfo.Comments))
            AaruConsole.VerboseWriteLine(Localization.CopyQM_comments_0, _imageInfo.Comments);

        _imageInfo.Heads           = _header.heads;
        _imageInfo.Cylinders       = _header.totalCylinders;
        _imageInfo.SectorsPerTrack = _header.sectorsPerTrack;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        buffer = new byte[length * _imageInfo.SectorSize];

        Array.Copy(_decodedDisk, (int)sectorAddress * _imageInfo.SectorSize, buffer, 0, length * _imageInfo.SectorSize);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public bool? VerifyMediaImage() => _calculatedDataCrc == _header.crc && _headerChecksumOk;
}