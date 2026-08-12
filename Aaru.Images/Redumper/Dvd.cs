// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dvd.cs
// Author(s)      : Rebecca Wallander <sakcheen@gmail.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads Redumper raw DVD (.sdram + .state) images.
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
// Copyright © 2011-2026 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.DVD;
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Images;

public sealed partial class Redumper
{
    ErrorNumber OpenDvd(IFilter imageFilter, string basePath, string sdramPath)
    {
        _isBluRay   = false;
        _lbaStart   = DVD_LBA_START;
        _bdNintendo = false;

        long stateLength = imageFilter.DataForkLength;
        long sdramLength = new FileInfo(sdramPath).Length;

        if(sdramLength % RECORDING_FRAME_SIZE != 0) return ErrorNumber.InvalidArgument;

        _totalFrames = sdramLength / RECORDING_FRAME_SIZE;

        if(stateLength != _totalFrames) return ErrorNumber.InvalidArgument;

        _imageFilter = imageFilter;

        Stream stateStream = imageFilter.GetDataForkStream();
        _stateData = new byte[stateLength];
        stateStream.Seek(0, SeekOrigin.Begin);
        stateStream.EnsureRead(_stateData, 0, (int)stateLength);

        _ramFilter = PluginRegister.Singleton.GetFilter(sdramPath);

        if(_ramFilter is null) return ErrorNumber.NoSuchFile;

        _ramPath = sdramPath;

        long negativeLbaCount = Math.Min(-_lbaStart, _totalFrames);
        long positiveLbaCount = Math.Max(0, _totalFrames + _lbaStart);

        _imageInfo.NegativeSectors = (uint)negativeLbaCount;
        _imageInfo.Sectors         = (ulong)positiveLbaCount;
        _imageInfo.SectorSize      = USER_DATA_SIZE;
        _imageInfo.ImageSize       = _imageInfo.Sectors * USER_DATA_SIZE;

        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.MetadataMediaType    = MetadataMediaType.OpticalDisc;
        _imageInfo.HasPartitions        = true;
        _imageInfo.HasSessions          = true;

        _mediaTags = new Dictionary<MediaTagType, byte[]>();
        LoadDvdMediaTagSidecars(basePath);

        _imageInfo.MediaType = MediaType.DVDROM;

        if(_mediaTags.TryGetValue(MediaTagType.DVD_PFI, out byte[] pfi))
        {
            PFI.PhysicalFormatInformation? decodedPfi = PFI.Decode(pfi, _imageInfo.MediaType);

            if(decodedPfi.HasValue)
            {
                _imageInfo.MediaType = decodedPfi.Value.DiskCategory switch
                                       {
                                           DiskCategory.DVDPR    => MediaType.DVDPR,
                                           DiskCategory.DVDPRDL  => MediaType.DVDPRDL,
                                           DiskCategory.DVDPRW   => MediaType.DVDPRW,
                                           DiskCategory.DVDPRWDL => MediaType.DVDPRWDL,
                                           DiskCategory.DVDR => decodedPfi.Value.PartVersion >= 6
                                                                    ? MediaType.DVDRDL
                                                                    : MediaType.DVDR,
                                           DiskCategory.DVDRAM => MediaType.DVDRAM,
                                           DiskCategory.DVDRW => decodedPfi.Value.PartVersion >= 15
                                                                     ? MediaType.DVDRWDL
                                                                     : MediaType.DVDRW,
                                           DiskCategory.Nintendo => decodedPfi.Value.DiscSize == DVDSize.Eighty
                                                                        ? MediaType.GOD
                                                                        : MediaType.WOD,
                                           _ => MediaType.DVDROM
                                       };

                if(decodedPfi.Value.DataAreaEndPSN >= decodedPfi.Value.DataAreaStartPSN)
                    _ngcwRegularDataSectors =
                        (ulong)(decodedPfi.Value.DataAreaEndPSN - decodedPfi.Value.DataAreaStartPSN) + 1;
            }
        }

        TryInitializeNgcwAfterOpen();

        _imageInfo.ReadableMediaTags = [.. _mediaTags.Keys];

        _imageInfo.ReadableSectorTags =
        [
            SectorTagType.DvdSectorInformation, SectorTagType.DvdSectorNumber, SectorTagType.DvdSectorIed,
            SectorTagType.DvdSectorCmi, SectorTagType.DvdSectorTitleKey, SectorTagType.DvdSectorEdc
        ];

        Tracks =
        [
            new Track
            {
                Sequence          = 1,
                Session           = 1,
                Type              = TrackType.Data,
                StartSector       = 0,
                EndSector         = _imageInfo.Sectors > 0 ? _imageInfo.Sectors - 1 : 0,
                Pregap            = 0,
                FileType          = "BINARY",
                Filter            = _ramFilter,
                File              = sdramPath,
                BytesPerSector    = USER_DATA_SIZE,
                RawBytesPerSector = DVD_SECTOR_SIZE
            }
        ];

        Sessions =
        [
            new Session
            {
                Sequence    = 1,
                StartSector = 0,
                EndSector   = _imageInfo.Sectors > 0 ? _imageInfo.Sectors - 1 : 0,
                StartTrack  = 1,
                EndTrack    = 1
            }
        ];

        Partitions =
        [
            new Partition
            {
                Sequence = 0,
                Start    = 0,
                Length   = _imageInfo.Sectors,
                Size     = _imageInfo.Sectors * _imageInfo.SectorSize,
                Offset   = 0,
                Type     = "DVD Data"
            }
        ];

        return ErrorNumber.NoError;
    }

    ErrorNumber ReadSectorForDvd(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer = new byte[USER_DATA_SIZE];
        ErrorNumber err = ReadSectorLongForDvd(sectorAddress, negative, out byte[] long_buffer, out sectorStatus);

        if(err != ErrorNumber.NoError) return err;

        Array.Copy(long_buffer, Aaru.Decoders.Nintendo.Sector.NintendoMainDataOffset, buffer, 0, USER_DATA_SIZE);

        return ErrorNumber.NoError;
    }

    ErrorNumber ReadSectorLongForDvd(ulong            sectorAddress, bool negative, out byte[] buffer,
                                     out SectorStatus sectorStatus)
    {
        sectorStatus = SectorStatus.Dumped;

        // The NGCW logic for DVDs are the same as for Nintendo
        return ReadSectorLongForNgcw(sectorAddress, negative, out buffer, out sectorStatus);
    }

    ErrorNumber ReadSectorsTagForDvd(ulong      sectorAddress, bool negative, uint length, SectorTagType tag,
                                     out byte[] buffer)
    {
        buffer = null;

        uint sectorOffset;
        uint sectorSize;

        switch(tag)
        {
            case SectorTagType.DvdSectorInformation:
                sectorOffset = 0;
                sectorSize   = 1;

                break;
            case SectorTagType.DvdSectorNumber:
                sectorOffset = 1;
                sectorSize   = 3;

                break;
            case SectorTagType.DvdSectorIed:
                sectorOffset = 4;
                sectorSize   = 2;

                break;
            case SectorTagType.DvdSectorCmi:
                sectorOffset = 6;
                sectorSize   = 1;

                break;
            case SectorTagType.DvdSectorTitleKey:
                sectorOffset = 7;
                sectorSize   = 5;

                break;
            case SectorTagType.DvdSectorEdc:
                sectorOffset = 2060;
                sectorSize   = 4;

                break;
            default:
                return ErrorNumber.NotSupported;
        }

        buffer = new byte[sectorSize * length];

        for(uint i = 0; i < length; i++)
        {
            ulong addr = negative ? sectorAddress - i : sectorAddress + i;

            ErrorNumber errno = ReadSectorLong(addr, negative, out byte[] sector, out _);

            if(errno != ErrorNumber.NoError) return errno;

            Array.Copy(sector, sectorOffset, buffer, i * sectorSize, sectorSize);
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Reads one RecordingFrame from the .sdram file and flattens it into a 2064-byte DVD sector
    ///     by extracting only the 172-byte main_data portion from each of the 12 rows (discarding PI/PO parity).
    /// </summary>
    byte[] ReadAndFlattenFrame(long frameIndex)
    {
        Stream stream = _ramFilter.GetDataForkStream();
        long   offset = frameIndex * RECORDING_FRAME_SIZE;

        if(offset + RECORDING_FRAME_SIZE > stream.Length) return null;

        var frame = new byte[RECORDING_FRAME_SIZE];
        stream.Seek(offset, SeekOrigin.Begin);
        stream.EnsureRead(frame, 0, RECORDING_FRAME_SIZE);

        var dvdSector = new byte[DVD_SECTOR_SIZE];
        int rowStride = ROW_MAIN_DATA_SIZE + ROW_PARITY_INNER_SIZE;

        for(int row = 0; row < RECORDING_FRAME_ROWS; row++)
            Array.Copy(frame, row * rowStride, dvdSector, row * ROW_MAIN_DATA_SIZE, ROW_MAIN_DATA_SIZE);

        return dvdSector;
    }

    /// <summary>Loads DVD sidecar files (.physical, .manufacturer, .bca).</summary>
    void LoadDvdMediaTagSidecars(string basePath)
    {
        LoadScsiSidecar(basePath, ".physical", ".0.physical", MediaTagType.DVD_PFI);

        LoadScsiSidecar(basePath, ".1.physical", null, MediaTagType.DVD_PFI_2ndLayer);

        LoadScsiSidecar(basePath, ".manufacturer", ".0.manufacturer", MediaTagType.DVD_DMI);

        string bcaPath = basePath + ".bca";

        if(File.Exists(bcaPath))
        {
            byte[] bcaData = File.ReadAllBytes(bcaPath);

            if(bcaData.Length > 0)
            {
                _mediaTags[MediaTagType.DVD_BCA] = bcaData;
                AaruLogging.Debug(MODULE_NAME, Localization.Found_media_tag_0, MediaTagType.DVD_BCA);
            }
        }
    }
}