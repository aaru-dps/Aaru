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
//     Reads Apple nibbelized disk images.
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Decoders.Floppy;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class AppleNib
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512)
            return ErrorNumber.InvalidArgument;

        var buffer = new byte[stream.Length];
        stream.EnsureRead(buffer, 0, buffer.Length);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Decoding_whole_image);
        List<Apple2.RawTrack> tracks = Apple2.MarshalDisk(buffer);
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Got_0_tracks, tracks.Count);

        Dictionary<ulong, Apple2.RawSector> rawSectors = new();

        var spt            = 0;
        var allTracksEqual = true;

        for(var i = 1; i < tracks.Count; i++)
            allTracksEqual &= tracks[i - 1].sectors.Length == tracks[i].sectors.Length;

        if(allTracksEqual)
            spt = tracks[0].sectors.Length;

        bool    skewed  = spt == 16;
        ulong[] skewing = _proDosSkewing;

        // Detect ProDOS skewed disks
        if(skewed)
        {
            foreach(bool isDos in from sector in tracks[17].sectors
                                  where sector.addressField.sector.SequenceEqual(new byte[] { 170, 170 })
                                  select Apple2.DecodeSector(sector)
                                  into sector0
                                  where sector0 != null
                                  select sector0[0x01] == 17 && sector0[0x02] < 16  && sector0[0x27] <= 122 &&
                                         sector0[0x34] == 35 && sector0[0x35] == 16 && sector0[0x36] == 0   &&
                                         sector0[0x37] == 1)
            {
                if(isDos)
                    skewing = _dosSkewing;

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           skewing.SequenceEqual(_dosSkewing)
                                               ? Localization.Using_DOS_skewing
                                               : Localization.Using_ProDOS_skewing);
            }
        }

        for(var i = 0; i < tracks.Count; i++)
        {
            foreach(Apple2.RawSector sector in tracks[i].sectors)
            {
                if(skewed && spt != 0)
                {
                    var sectorNo = (ulong)(((sector.addressField.sector[0] & 0x55) << 1 |
                                            sector.addressField.sector[1] & 0x55) & 0xFF);

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Hardware_sector_0_of_track_1_goes_to_logical_sector_2,
                                               sectorNo, i, skewing[sectorNo] + (ulong)(i * spt));

                    rawSectors.Add(skewing[sectorNo] + (ulong)(i * spt), sector);
                    _imageInfo.Sectors++;
                }
                else
                {
                    rawSectors.Add(_imageInfo.Sectors, sector);
                    _imageInfo.Sectors++;
                }
            }
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Got_0_sectors, _imageInfo.Sectors);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Cooking_sectors);

        _longSectors   = new Dictionary<ulong, byte[]>();
        _cookedSectors = new Dictionary<ulong, byte[]>();
        _addressFields = new Dictionary<ulong, byte[]>();

        foreach(KeyValuePair<ulong, Apple2.RawSector> kvp in rawSectors)
        {
            byte[] cooked = Apple2.DecodeSector(kvp.Value);
            byte[] raw    = Apple2.MarshalSector(kvp.Value);
            byte[] addr   = Apple2.MarshalAddressField(kvp.Value.addressField);
            _longSectors.Add(kvp.Key, raw);
            _cookedSectors.Add(kvp.Key, cooked);
            _addressFields.Add(kvp.Key, addr);
        }

        _imageInfo.ImageSize            = (ulong)imageFilter.DataForkLength;
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);

        _imageInfo.MediaType = _imageInfo.Sectors switch
                               {
                                   455 => MediaType.Apple32SS,
                                   560 => MediaType.Apple33SS,
                                   _   => MediaType.Unknown
                               };

        _imageInfo.SectorSize        = 256;
        _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;
        _imageInfo.ReadableSectorTags.Add(SectorTagType.FloppyAddressMark);

        switch(_imageInfo.MediaType)
        {
            case MediaType.Apple32SS:
                _imageInfo.Cylinders       = 35;
                _imageInfo.Heads           = 1;
                _imageInfo.SectorsPerTrack = 13;

                break;
            case MediaType.Apple33SS:
                _imageInfo.Cylinders       = 35;
                _imageInfo.Heads           = 1;
                _imageInfo.SectorsPerTrack = 16;

                break;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        return _cookedSectors.TryGetValue(sectorAddress, out buffer) ? ErrorNumber.NoError : ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return errno;

            ms.Write(sector, 0, sector.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(tag != SectorTagType.FloppyAddressMark)
            return ErrorNumber.NotSupported;

        return _addressFields.TryGetValue(sectorAddress, out buffer) ? ErrorNumber.NoError : ErrorNumber.NoData;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        if(tag != SectorTagType.FloppyAddressMark)
            return ErrorNumber.NotSupported;

        var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSectorTag(sectorAddress + i, tag, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return errno;

            ms.Write(sector, 0, sector.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        return _longSectors.TryGetValue(sectorAddress, out buffer) ? ErrorNumber.NoError : ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors)
            return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSectorLong(sectorAddress + i, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return errno;

            ms.Write(sector, 0, sector.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

#endregion
}