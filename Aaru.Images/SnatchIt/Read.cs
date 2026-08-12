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
//     Reads Software Pirates SNATCH-IT disk images.
//
//     Based on the work of Michal Necasek (fdimg).
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Images;

public sealed partial class SnatchIt
{
    /// <summary>Size in bytes of the on-disk track header block.</summary>
    const int CP2_TRACK_HEADER_SIZE = 3 + CP2_MAX_SPT * 16;

    /// <summary>Convert size code to bytes (128 &lt;&lt; code).</summary>
    static uint SectorSizeFromCode(byte code) => (uint)(128 << code);

    /// <summary>Walk all segments from <paramref name="startOfs" /> and populate <see cref="_trackMap" />.</summary>
    ErrorNumber BuildTrackMap(long startOfs, out byte heads, out byte cyls)
    {
        heads = 1;
        cyls  = 0;

        long currOfs     = startOfs;
        var  cylLast     = 0;
        var  seen2ndSide = false;
        var  blkSzBuf    = new byte[2];
        var  trailerBuf  = new byte[1];
        var  thdrBuf     = new byte[CP2_TRACK_HEADER_SIZE];

        _stream.Seek(currOfs, SeekOrigin.Begin);

        for(;;)
        {
            var firstInSeg = true;

            if(_stream.EnsureRead(blkSzBuf, 0, 2) != 2) break;

            var blkSz = (ushort)(blkSzBuf[0] | blkSzBuf[1] << 8);

            if(blkSz == 0 || blkSz >= 0x8000) return ErrorNumber.InvalidArgument;

            if((blkSz - 1) % CP2_TRACK_HEADER_SIZE != 0) return ErrorNumber.InvalidArgument;

            currOfs += 2;

            long trkDataOfs = currOfs + blkSz + 2;
            int  nThdrs     = (blkSz - 1) / CP2_TRACK_HEADER_SIZE;

            for(var i = 0; i < nThdrs; i++)
            {
                if(_stream.EnsureRead(thdrBuf, 0, CP2_TRACK_HEADER_SIZE) != CP2_TRACK_HEADER_SIZE)
                    return ErrorNumber.InOutError;

                currOfs += CP2_TRACK_HEADER_SIZE;

                byte cyl     = thdrBuf[0];
                byte head    = thdrBuf[1];
                byte numSect = thdrBuf[2];
                var  phantom = false;

                if(numSect == 1)
                {
                    // First sector header starts at offset 3; fields inside: st0..st2 at 1..3, hdr_cyl at 4, hdr_sec at 6, size_code at 7
                    byte pcyl  = thdrBuf[3 + 4];
                    byte psec  = thdrBuf[3 + 6];
                    byte pcode = thdrBuf[3 + 7];

                    if(pcyl == 6 && psec == 6 && pcode == 6) phantom = true;
                }

                if(phantom)
                {
                    firstInSeg = false;

                    continue;
                }

                ErrorNumber rc = MapTrack(thdrBuf, cyl, head, numSect, trkDataOfs, firstInSeg);

                if(rc != ErrorNumber.NoError) return rc;

                if(cyl > cylLast) cylLast = cyl;

                if(head != 0) seen2ndSide = true;

                firstInSeg = false;
            }

            if(_stream.EnsureRead(trailerBuf, 0, 1) != 1) return ErrorNumber.InOutError;

            byte trailer = trailerBuf[0];

            currOfs += 1;

            if(_stream.EnsureRead(blkSzBuf, 0, 2) != 2) return ErrorNumber.InOutError;

            var dataBlkSz = (ushort)(blkSzBuf[0] | blkSzBuf[1] << 8);

            currOfs += 2;

            currOfs += dataBlkSz;

            _stream.Seek(currOfs, SeekOrigin.Begin);

            if(trailer == CP2_TRAILER_LAST) break;
        }

        heads = (byte)(seen2ndSide ? 2 : 1);
        cyls  = (byte)(cylLast + 1);

        return ErrorNumber.NoError;
    }

    /// <summary>Parse sector headers from a track header block and store descriptors into <see cref="_trackMap" />.</summary>
    ErrorNumber MapTrack(byte[] thdrBuf, byte cyl, byte head, byte numSect, long trkDataOfs, bool firstInSeg)
    {
        if(cyl >= NCYL_MAX) return ErrorNumber.InvalidArgument;

        if(head > 1) return ErrorNumber.InvalidArgument;

        int trkIdx = cyl * 2 + head;

        if(trkIdx >= _trackMap.Length) return ErrorNumber.InvalidArgument;

        if(_trackMap[trkIdx] is not null) return ErrorNumber.InvalidArgument;

        // The track header only describes up to CP2_MAX_SPT sectors
        if(numSect > CP2_MAX_SPT) return ErrorNumber.InvalidArgument;

        var sectors = new SectorDesc[numSect];

        for(var ps = 0; ps < numSect; ps++)
        {
            int  off     = 3 + ps * 16;
            byte result  = thdrBuf[off + 0];
            byte st0     = thdrBuf[off + 1];
            byte st1     = thdrBuf[off + 2];
            byte st2     = thdrBuf[off + 3];
            byte hdrCyl  = thdrBuf[off + 4];
            byte hdrHead = thdrBuf[off + 5];
            byte hdrSec  = thdrBuf[off + 6];
            byte sizCode = thdrBuf[off + 7];
            byte ofsLo   = thdrBuf[off + 8];
            byte ofsHi   = thdrBuf[off + 9];

            var secOfs = (ushort)(ofsHi << 8 | ofsLo);

            if(ps == 0 && firstInSeg && secOfs != CP2_SEC_OFS_MAGIC) return ErrorNumber.InvalidArgument;

            if(secOfs < CP2_SEC_OFS_MAGIC) return ErrorNumber.InvalidArgument;

            // ST0 bits 0:1 = drive select, bit 2 = head; other bits indicate abnormal termination or error.
            // ST1/ST2 nonzero generally indicate errors. The ST2 Control Mark bit marks a Deleted Data
            // Address Mark, which is not an error per se but is tracked separately. Any other abnormal bit
            // set leaves the sector flagged as errored rather than rejecting the whole image.
            bool errored = (st0 & 0xF8) != 0 || st1 != 0 || (st2 & ~CP2_ST2_DELETED_DAM) != 0;

            if(hdrCyl != cyl) return ErrorNumber.InvalidArgument;

            if(hdrHead != head) return ErrorNumber.InvalidArgument;

            uint secSize = SectorSizeFromCode(sizCode);

            sectors[ps] = new SectorDesc
            {
                FileOffset = trkDataOfs + secOfs - CP2_SEC_OFS_MAGIC,
                Size       = secSize,
                SectorId   = hdrSec,
                Deleted    = (st2 & CP2_ST2_DELETED_DAM) != 0,
                Errored    = errored
            };

            _ = result;
        }

        _trackMap[trkIdx] = new TrackDesc
        {
            Sectors = sectors
        };

        return ErrorNumber.NoError;
    }

#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        _stream = imageFilter.GetDataForkStream();
        _stream.Seek(0, SeekOrigin.Begin);

        var hdrBuf = new byte[30];

        if(_stream.EnsureRead(hdrBuf, 0, 30) != 30) return ErrorNumber.InvalidArgument;

        string sig = Encoding.ASCII.GetString(hdrBuf, 0, 16);

        if(sig != SOFTWARE_PIRATES) return ErrorNumber.InvalidArgument;

        string ver = Encoding.ASCII.GetString(hdrBuf, 16, 7);

        if(ver != RELEASE_PREFIX) return ErrorNumber.InvalidArgument;

        if(hdrBuf[28] != (byte)'$' || hdrBuf[29] != (byte)'0') return ErrorNumber.NotSupported;

        string version = Encoding.ASCII.GetString(hdrBuf, 16, 12).TrimEnd('\0', ' ');

        AaruLogging.Debug(MODULE_NAME, "header.version = {0}", version);

        _trackMap = new TrackDesc[NCYL_MAX * 2];

        ErrorNumber rc = BuildTrackMap(30, out byte heads, out byte cyls);

        if(rc != ErrorNumber.NoError) return rc;

        if(cyls == 0) return ErrorNumber.InvalidArgument;

        // First recorded track is always trk_ofs[0] (cyl 0, head 0) for a sane image.
        TrackDesc firstTrack = _trackMap[0];

        if(firstTrack?.Sectors is null || firstTrack.Sectors.Length == 0) return ErrorNumber.InvalidArgument;

        byte firstId = 255;

        foreach(SectorDesc sd in firstTrack.Sectors.Where(sd => sd.SectorId < firstId)) firstId = sd.SectorId;

        var spt = (byte)firstTrack.Sectors.Length;

        AaruLogging.Debug(MODULE_NAME, "cyls = {0}, heads = {1}, spt = {2}, firstId = {3}", cyls, heads, spt, firstId);

        ulong totalSectors = (ulong)cyls * heads * spt;

        _sectorMap = new SectorLoc[totalSectors];

        uint maxSectorSize = 0;

        for(ulong lba = 0; lba < totalSectors; lba++)
        {
            var track = (int)(lba / spt);

            if(heads == 1) track *= 2;

            int logicalId = (int)(lba % spt) + firstId;

            TrackDesc td = track < _trackMap.Length ? _trackMap[track] : null;

            if(td?.Sectors is null)
            {
                _sectorMap[lba] = new SectorLoc
                {
                    TrackPresent  = false,
                    SectorPresent = false,
                    Size          = 0,
                    FileOffset    = -1
                };

                continue;
            }

            SectorDesc found = td.Sectors.FirstOrDefault(sd => sd.SectorId == logicalId);

            if(found is null)
            {
                _sectorMap[lba] = new SectorLoc
                {
                    TrackPresent  = true,
                    SectorPresent = false,
                    Size          = 0,
                    FileOffset    = -1
                };

                continue;
            }

            if(found.Size > maxSectorSize) maxSectorSize = found.Size;

            _sectorMap[lba] = new SectorLoc
            {
                TrackPresent  = true,
                SectorPresent = true,
                FileOffset    = found.FileOffset,
                Size          = found.Size,
                Deleted       = found.Deleted,
                Errored       = found.Errored
            };
        }

        if(maxSectorSize == 0) maxSectorSize = 512;

        _imageInfo.MetadataMediaType    = MetadataMediaType.BlockMedia;
        _imageInfo.Sectors              = totalSectors;
        _imageInfo.SectorSize           = maxSectorSize;
        _imageInfo.ImageSize            = (ulong)_stream.Length - 30;
        _imageInfo.Heads                = heads;
        _imageInfo.Cylinders            = cyls;
        _imageInfo.SectorsPerTrack      = spt;
        _imageInfo.Application          = "SNATCH-IT";
        _imageInfo.Version              = version;
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;

        _imageInfo.MediaType = Geometry.GetMediaType((cyls, heads, spt, maxSectorSize, MediaEncoding.MFM, false));

        AaruLogging.Verbose(Localization.SnatchIt_image_contains_a_disk_of_type_0, _imageInfo.MediaType);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        ErrorNumber rc = ReadSectors(sectorAddress, negative, 1, out buffer, out SectorStatus[] statuses);

        sectorStatus = statuses is { Length: > 0 } ? statuses[0] : SectorStatus.NotDumped;

        return rc;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.NotSupported;

        if(sectorAddress >= _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        // Pass 1: compute the concatenated output length (sum of each sector's real size).
        long totalLen = 0;

        for(uint i = 0; i < length; i++)
        {
            SectorLoc loc = _sectorMap[sectorAddress + i];

            totalLen += loc.SectorPresent ? loc.Size : _imageInfo.SectorSize;
        }

        buffer       = new byte[totalLen];
        sectorStatus = new SectorStatus[length];

        long offset = 0;

        for(uint i = 0; i < length; i++)
        {
            SectorLoc loc = _sectorMap[sectorAddress + i];

            if(!loc.SectorPresent)
            {
                string placeholder = !loc.TrackPresent ? CP2_TRACK_MISSING_TEXT : CP2_SECTOR_MISSING_TEXT;

                byte[] txt = Encoding.ASCII.GetBytes(placeholder);

                Array.Copy(txt, 0, buffer, offset, Math.Min(txt.Length, _imageInfo.SectorSize));

                offset          += _imageInfo.SectorSize;
                sectorStatus[i] =  SectorStatus.NotDumped;

                continue;
            }

            _stream.Seek(loc.FileOffset, SeekOrigin.Begin);

            if(_stream.EnsureRead(buffer, (int)offset, (int)loc.Size) != (int)loc.Size) return ErrorNumber.InOutError;

            offset += loc.Size;

            // SectorStatus has no dedicated "deleted DAM" value; map both deleted-DAM and FDC error
            // bits to Errored. Otherwise the sector was read cleanly.
            sectorStatus[i] = loc.Deleted || loc.Errored ? SectorStatus.Errored : SectorStatus.Dumped;
        }

        return ErrorNumber.NoError;
    }

#endregion
}