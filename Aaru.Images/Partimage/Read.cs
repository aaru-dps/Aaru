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
//     Reads partimage disk images.
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
using System.Diagnostics;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class Partimage
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 512)
            return ErrorNumber.InvalidArgument;

        var hdrB = new byte[Marshal.SizeOf<Header>()];
        stream.EnsureRead(hdrB, 0, Marshal.SizeOf<Header>());
        _cVolumeHeader = Marshal.ByteArrayToStructureLittleEndian<Header>(hdrB);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CVolumeHeader.magic = {0}",
                                   StringHandlers.CToString(_cVolumeHeader.magic));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CVolumeHeader.version = {0}",
                                   StringHandlers.CToString(_cVolumeHeader.version));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CVolumeHeader.volumeNumber = {0}", _cVolumeHeader.volumeNumber);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CVolumeHeader.identificator = {0:X16}",
                                   _cVolumeHeader.identificator);

        // TODO: Support multifile volumes
        if(_cVolumeHeader.volumeNumber > 0)
        {
            AaruConsole.ErrorWriteLine(Localization.Support_for_multiple_volumes_not_supported);

            return ErrorNumber.NotImplemented;
        }

        hdrB = new byte[Marshal.SizeOf<MainHeader>()];
        stream.EnsureRead(hdrB, 0, Marshal.SizeOf<MainHeader>());
        _cMainHeader = Marshal.ByteArrayToStructureLittleEndian<MainHeader>(hdrB);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.szFileSystem = {0}",
                                   StringHandlers.CToString(_cMainHeader.szFileSystem));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.szPartDescription = {0}",
                                   StringHandlers.CToString(_cMainHeader.szPartDescription));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.szOriginalDevice = {0}",
                                   StringHandlers.CToString(_cMainHeader.szOriginalDevice));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.szFirstImageFilepath = {0}",
                                   StringHandlers.CToString(_cMainHeader.szFirstImageFilepath));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.szUnameSysname = {0}",
                                   StringHandlers.CToString(_cMainHeader.szUnameSysname));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.szUnameNodename = {0}",
                                   StringHandlers.CToString(_cMainHeader.szUnameNodename));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.szUnameRelease = {0}",
                                   StringHandlers.CToString(_cMainHeader.szUnameRelease));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.szUnameVersion = {0}",
                                   StringHandlers.CToString(_cMainHeader.szUnameVersion));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.szUnameMachine = {0}",
                                   StringHandlers.CToString(_cMainHeader.szUnameMachine));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwCompression = {0} ({1})",
                                   _cMainHeader.dwCompression, (uint)_cMainHeader.dwCompression);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwMainFlags = {0}", _cMainHeader.dwMainFlags);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dateCreate.tm_sec = {0}",
                                   _cMainHeader.dateCreate.Second);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dateCreate.tm_min = {0}",
                                   _cMainHeader.dateCreate.Minute);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dateCreate.tm_hour = {0}",
                                   _cMainHeader.dateCreate.Hour);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dateCreate.tm_mday = {0}",
                                   _cMainHeader.dateCreate.DayOfMonth);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dateCreate.tm_mon = {0}",
                                   _cMainHeader.dateCreate.Month);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dateCreate.tm_year = {0}",
                                   _cMainHeader.dateCreate.Year);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dateCreate.tm_wday = {0}",
                                   _cMainHeader.dateCreate.DayOfWeek);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dateCreate.tm_yday = {0}",
                                   _cMainHeader.dateCreate.DayOfYear);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dateCreate.tm_isdst = {0}",
                                   _cMainHeader.dateCreate.IsDst);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dateCreate.tm_gmtoffsec = {0}",
                                   _cMainHeader.dateCreate.GmtOff);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dateCreate.tm_zone = {0}",
                                   _cMainHeader.dateCreate.Timezone);

        var dateCreate = new DateTime(1900 + (int)_cMainHeader.dateCreate.Year, (int)_cMainHeader.dateCreate.Month + 1,
                                      (int)_cMainHeader.dateCreate.DayOfMonth, (int)_cMainHeader.dateCreate.Hour,
                                      (int)_cMainHeader.dateCreate.Minute, (int)_cMainHeader.dateCreate.Second);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dateCreate = {0}", dateCreate);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.qwPartSize = {0}", _cMainHeader.qwPartSize);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.szHostname = {0}",
                                   StringHandlers.CToString(_cMainHeader.szHostname));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.szVersion = {0}",
                                   StringHandlers.CToString(_cMainHeader.szVersion));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwMbrCount = {0}", _cMainHeader.dwMbrCount);
        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwMbrSize = {0}",  _cMainHeader.dwMbrSize);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwEncryptAlgo = {0} ({1})",
                                   _cMainHeader.dwEncryptAlgo, (uint)_cMainHeader.dwEncryptAlgo);

        AaruConsole.DebugWriteLine(MODULE_NAME, "ArrayIsNullOrEmpty(CMainHeader.cHashTestKey) = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(_cMainHeader.cHashTestKey));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwReservedFuture000 = {0}",
                                   _cMainHeader.dwReservedFuture000);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwReservedFuture001 = {0}",
                                   _cMainHeader.dwReservedFuture001);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwReservedFuture002 = {0}",
                                   _cMainHeader.dwReservedFuture002);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwReservedFuture003 = {0}",
                                   _cMainHeader.dwReservedFuture003);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwReservedFuture004 = {0}",
                                   _cMainHeader.dwReservedFuture004);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwReservedFuture005 = {0}",
                                   _cMainHeader.dwReservedFuture005);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwReservedFuture006 = {0}",
                                   _cMainHeader.dwReservedFuture006);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwReservedFuture007 = {0}",
                                   _cMainHeader.dwReservedFuture007);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwReservedFuture008 = {0}",
                                   _cMainHeader.dwReservedFuture008);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.dwReservedFuture009 = {0}",
                                   _cMainHeader.dwReservedFuture009);

        AaruConsole.DebugWriteLine(MODULE_NAME, "ArrayIsNullOrEmpty(CMainHeader.cReserved) = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(_cMainHeader.cReserved));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CMainHeader.crc = 0x{0:X8}", _cMainHeader.crc);

        // partimage 0.6.1 does not support them either
        if(_cMainHeader.dwEncryptAlgo != PEncryption.None)
        {
            AaruConsole.ErrorWriteLine(Localization.Encrypted_images_are_not_yet_supported);

            return ErrorNumber.NotImplemented;
        }

        string magic;

        // Skip MBRs
        if(_cMainHeader.dwMbrCount > 0)
        {
            hdrB = new byte[MAGIC_BEGIN_MBRBACKUP.Length];
            stream.EnsureRead(hdrB, 0, MAGIC_BEGIN_MBRBACKUP.Length);
            magic = StringHandlers.CToString(hdrB);

            if(!magic.Equals(MAGIC_BEGIN_MBRBACKUP))
            {
                AaruConsole.ErrorWriteLine(Localization.Cannot_find_MBRs);

                return ErrorNumber.InvalidArgument;
            }

            stream.Seek(_cMainHeader.dwMbrSize * _cMainHeader.dwMbrCount, SeekOrigin.Current);
        }

        // Skip extended headers and their CRC fields
        stream.Seek((MAGIC_BEGIN_EXT000.Length + 4) * 10, SeekOrigin.Current);

        hdrB = new byte[MAGIC_BEGIN_LOCALHEADER.Length];
        stream.EnsureRead(hdrB, 0, MAGIC_BEGIN_LOCALHEADER.Length);
        magic = StringHandlers.CToString(hdrB);

        if(!magic.Equals(MAGIC_BEGIN_LOCALHEADER))
        {
            AaruConsole.ErrorWriteLine(Localization.Cannot_find_local_header);

            return ErrorNumber.InvalidArgument;
        }

        hdrB = new byte[Marshal.SizeOf<CLocalHeader>()];
        stream.EnsureRead(hdrB, 0, Marshal.SizeOf<CLocalHeader>());
        CLocalHeader localHeader = Marshal.ByteArrayToStructureLittleEndian<CLocalHeader>(hdrB);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CLocalHeader.qwBlockSize = {0}",  localHeader.qwBlockSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "CLocalHeader.qwUsedBlocks = {0}", localHeader.qwUsedBlocks);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CLocalHeader.qwBlocksCount = {0}", localHeader.qwBlocksCount);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CLocalHeader.qwBitmapSize = {0}", localHeader.qwBitmapSize);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CLocalHeader.qwBadBlocksCount = {0}",
                                   localHeader.qwBadBlocksCount);

        AaruConsole.DebugWriteLine(MODULE_NAME, "CLocalHeader.szLabel = {0}",
                                   StringHandlers.CToString(localHeader.szLabel));

        AaruConsole.DebugWriteLine(MODULE_NAME, "ArrayIsNullOrEmpty(CLocalHeader.cReserved) = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(localHeader.cReserved));

        AaruConsole.DebugWriteLine(MODULE_NAME, "CLocalHeader.crc = 0x{0:X8}", localHeader.crc);

        hdrB = new byte[MAGIC_BEGIN_BITMAP.Length];
        stream.EnsureRead(hdrB, 0, MAGIC_BEGIN_BITMAP.Length);
        magic = StringHandlers.CToString(hdrB);

        if(!magic.Equals(MAGIC_BEGIN_BITMAP))
        {
            AaruConsole.ErrorWriteLine(Localization.Cannot_find_bitmap);

            return ErrorNumber.InvalidArgument;
        }

        _bitmap = new byte[localHeader.qwBitmapSize];
        stream.EnsureRead(_bitmap, 0, (int)localHeader.qwBitmapSize);

        hdrB = new byte[MAGIC_BEGIN_INFO.Length];
        stream.EnsureRead(hdrB, 0, MAGIC_BEGIN_INFO.Length);
        magic = StringHandlers.CToString(hdrB);

        if(!magic.Equals(MAGIC_BEGIN_INFO))
        {
            AaruConsole.ErrorWriteLine(Localization.Cannot_find_info_block);

            return ErrorNumber.InvalidArgument;
        }

        // Skip info block and its checksum
        stream.Seek(16384 + 4, SeekOrigin.Current);

        hdrB = new byte[MAGIC_BEGIN_DATABLOCKS.Length];
        stream.EnsureRead(hdrB, 0, MAGIC_BEGIN_DATABLOCKS.Length);
        magic = StringHandlers.CToString(hdrB);

        if(!magic.Equals(MAGIC_BEGIN_DATABLOCKS))
        {
            AaruConsole.ErrorWriteLine(Localization.Cannot_find_data_blocks);

            return ErrorNumber.InvalidArgument;
        }

        _dataOff = stream.Position;

        AaruConsole.DebugWriteLine(MODULE_NAME, "dataOff = {0}", _dataOff);

        // Seek to tail
        stream.Seek(-(Marshal.SizeOf<CMainTail>() + MAGIC_BEGIN_TAIL.Length), SeekOrigin.End);

        hdrB = new byte[MAGIC_BEGIN_TAIL.Length];
        stream.EnsureRead(hdrB, 0, MAGIC_BEGIN_TAIL.Length);
        magic = StringHandlers.CToString(hdrB);

        if(!magic.Equals(MAGIC_BEGIN_TAIL))
        {
            AaruConsole.ErrorWriteLine(Localization.
                                           Cannot_find_tail_Multiple_volumes_are_not_supported_or_image_is_corrupt);

            return ErrorNumber.InvalidArgument;
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Filling_extents);
        var extentsFillStopwatch = new Stopwatch();
        extentsFillStopwatch.Start();
        _extents    = new ExtentsULong();
        _extentsOff = new Dictionary<ulong, ulong>();
        bool  current     = (_bitmap[0] & 1 << 0 % 8) != 0;
        ulong blockOff    = 0;
        ulong extentStart = 0;

        for(ulong i = 1; i <= localHeader.qwBlocksCount; i++)
        {
            bool next = (_bitmap[i / 8] & 1 << (int)(i % 8)) != 0;

            // Flux
            if(next != current)
            {
                if(next)
                {
                    extentStart = i;
                    _extentsOff.Add(i, ++blockOff);
                }
                else
                {
                    _extents.Add(extentStart, i);
                    _extentsOff.TryGetValue(extentStart, out ulong _);
                }
            }

            if(next && current)
                blockOff++;

            current = next;
        }

        extentsFillStopwatch.Stop();

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Took_0_seconds_to_fill_extents,
                                   extentsFillStopwatch.Elapsed.TotalSeconds);

        _sectorCache = new Dictionary<ulong, byte[]>();

        _imageInfo.CreationTime         = dateCreate;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Sectors              = localHeader.qwBlocksCount + 1;
        _imageInfo.SectorSize           = (uint)localHeader.qwBlockSize;
        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.MediaType            = MediaType.GENERIC_HDD;
        _imageInfo.Version              = StringHandlers.CToString(_cMainHeader.szVersion);
        _imageInfo.Comments             = StringHandlers.CToString(_cMainHeader.szPartDescription);

        _imageInfo.ImageSize =
            (ulong)(stream.Length - (_dataOff + Marshal.SizeOf<CMainTail>() + MAGIC_BEGIN_TAIL.Length));

        _imageStream = stream;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1)
            return ErrorNumber.OutOfRange;

        if((_bitmap[sectorAddress / 8] & 1 << (int)(sectorAddress % 8)) == 0)
        {
            buffer = new byte[_imageInfo.SectorSize];

            return ErrorNumber.NoError;
        }

        if(_sectorCache.TryGetValue(sectorAddress, out buffer))
            return ErrorNumber.NoError;

        ulong blockOff = BlockOffset(sectorAddress);

        // Offset of requested sector is:
        // Start of data +
        long imageOff = _dataOff +

                        // How many stored bytes to skip
                        (long)(blockOff * _imageInfo.SectorSize) +

                        // How many bytes of CRC blocks to skip
                        (long)(blockOff / (CHECK_FREQUENCY / _imageInfo.SectorSize)) * Marshal.SizeOf<CCheck>();

        buffer = new byte[_imageInfo.SectorSize];
        _imageStream.Seek(imageOff, SeekOrigin.Begin);
        _imageStream.EnsureRead(buffer, 0, (int)_imageInfo.SectorSize);

        if(_sectorCache.Count > MAX_CACHED_SECTORS)
            _sectorCache.Clear();

        _sectorCache.Add(sectorAddress, buffer);

        return ErrorNumber.NoError;
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

        var allEmpty = true;

        for(uint i = 0; i < length; i++)
        {
            if((_bitmap[sectorAddress / 8] & 1 << (int)(sectorAddress % 8)) != 0)
            {
                allEmpty = false;

                break;
            }
        }

        if(allEmpty)
        {
            buffer = new byte[_imageInfo.SectorSize * length];

            return ErrorNumber.NoError;
        }

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

#endregion
}