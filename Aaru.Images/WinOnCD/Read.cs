// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads WinOnCD disc images.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.Images;

public sealed partial class WinOnCD
{
    /// <inheritdoc />
    public ErrorNumber ReadDPM(out uint dpmStartSector, out uint dpmResolution, out uint numberOfDpmEntries, out ulong[] dpm)
    {
        dpmStartSector     = 0;
        dpmResolution      = 0;
        numberOfDpmEntries = 0;
        dpm                = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorDPM(ulong sectorAddress, out ulong? dpm)
    {
        dpm = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        if(trk is null) return ErrorNumber.SectorNotFound;

        if(sectorAddress < trk.StartSector || sectorAddress > trk.EndSector) return ErrorNumber.OutOfRange;

        ulong offsetInTrack = sectorAddress         - trk.StartSector;
        int   stride        = trk.RawBytesPerSector + (trk.SubchannelType != TrackSubchannelType.None ? 96 : 0);
        var   fileOffset    = (long)(trk.FileOffset + offsetInTrack * (ulong)stride);

        if(trk.RawBytesPerSector == trk.BytesPerSector)
        {
            buffer = new byte[trk.BytesPerSector];
            _imageStream.Seek(fileOffset, SeekOrigin.Begin);
            _imageStream.EnsureRead(buffer, 0, trk.BytesPerSector);
        }
        else
        {
            var rawSector = new byte[trk.RawBytesPerSector];
            _imageStream.Seek(fileOffset, SeekOrigin.Begin);
            _imageStream.EnsureRead(rawSector, 0, trk.RawBytesPerSector);

            buffer = new byte[trk.BytesPerSector];

            switch(trk.Type)
            {
                case TrackType.CdMode1 when trk.RawBytesPerSector == 2352:
                    Array.Copy(rawSector, 16, buffer, 0, trk.BytesPerSector);

                    break;
                case TrackType.CdMode2Formless when trk.RawBytesPerSector == 2352:
                    Array.Copy(rawSector, 16, buffer, 0, trk.BytesPerSector);

                    break;
                default:
                    Array.Copy(rawSector, 0, buffer, 0, trk.BytesPerSector);

                    break;
            }
        }

        sectorStatus = SectorStatus.Dumped;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        if(trk is null) return ErrorNumber.SectorNotFound;

        if(sectorAddress + length - 1 > trk.EndSector) return ErrorNumber.OutOfRange;

        using var ms = new MemoryStream();
        sectorStatus = new SectorStatus[length];

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSector(sectorAddress + i, track, out byte[] sectorData, out SectorStatus status);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(sectorData, 0, sectorData.Length);
            sectorStatus[i] = status;
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(negative) return ErrorNumber.NotSupported;

        Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.StartSector && sectorAddress <= t.EndSector);

        if(trk is null) return ErrorNumber.SectorNotFound;

        return ReadSector(sectorAddress, trk.Sequence, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.NotSupported;

        Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.StartSector && sectorAddress <= t.EndSector);

        if(trk is null) return ErrorNumber.SectorNotFound;

        return ReadSectors(sectorAddress, length, trk.Sequence, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        if(trk is null) return ErrorNumber.SectorNotFound;

        if(sectorAddress < trk.StartSector || sectorAddress > trk.EndSector) return ErrorNumber.OutOfRange;

        ulong offsetInTrack = sectorAddress         - trk.StartSector;
        int   stride        = trk.RawBytesPerSector + (trk.SubchannelType != TrackSubchannelType.None ? 96 : 0);
        var   fileOffset    = (long)(trk.FileOffset + offsetInTrack * (ulong)stride);

        switch(trk.Type)
        {
            case TrackType.Audio:
            {
                buffer = new byte[2352];
                _imageStream.Seek(fileOffset, SeekOrigin.Begin);
                _imageStream.EnsureRead(buffer, 0, 2352);

                break;
            }
            case TrackType.CdMode1:
            {
                if(trk.RawBytesPerSector >= 2352)
                {
                    buffer = new byte[2352];
                    _imageStream.Seek(fileOffset, SeekOrigin.Begin);
                    _imageStream.EnsureRead(buffer, 0, 2352);
                }
                else
                {
                    var userData = new byte[trk.RawBytesPerSector];
                    _imageStream.Seek(fileOffset, SeekOrigin.Begin);
                    _imageStream.EnsureRead(userData, 0, trk.RawBytesPerSector);

                    buffer = new byte[2352];
                    Array.Copy(userData, 0, buffer, 16, trk.RawBytesPerSector);
                    _sectorBuilder.ReconstructPrefix(ref buffer, TrackType.CdMode1, (long)sectorAddress);
                    _sectorBuilder.ReconstructEcc(ref buffer, TrackType.CdMode1);
                }

                break;
            }
            case TrackType.CdMode2Form1:
            {
                var userData = new byte[trk.RawBytesPerSector];
                _imageStream.Seek(fileOffset, SeekOrigin.Begin);
                _imageStream.EnsureRead(userData, 0, trk.RawBytesPerSector);

                buffer = new byte[2352];
                Array.Copy(userData, 0, buffer, 24, trk.RawBytesPerSector);
                _sectorBuilder.ReconstructPrefix(ref buffer, TrackType.CdMode2Form1, (long)sectorAddress);
                _sectorBuilder.ReconstructEcc(ref buffer, TrackType.CdMode2Form1);

                break;
            }
            case TrackType.CdMode2Form2:
            {
                var userData = new byte[trk.RawBytesPerSector];
                _imageStream.Seek(fileOffset, SeekOrigin.Begin);
                _imageStream.EnsureRead(userData, 0, trk.RawBytesPerSector);

                buffer = new byte[2352];
                Array.Copy(userData, 0, buffer, 24, trk.RawBytesPerSector);
                _sectorBuilder.ReconstructPrefix(ref buffer, TrackType.CdMode2Form2, (long)sectorAddress);
                _sectorBuilder.ReconstructEcc(ref buffer, TrackType.CdMode2Form2);

                break;
            }
            case TrackType.CdMode2Formless:
            {
                if(trk.RawBytesPerSector >= 2352)
                {
                    buffer = new byte[2352];
                    _imageStream.Seek(fileOffset, SeekOrigin.Begin);
                    _imageStream.EnsureRead(buffer, 0, 2352);
                }
                else
                {
                    buffer = new byte[2352];
                    _sectorBuilder.ReconstructPrefix(ref buffer, TrackType.CdMode2Formless, (long)sectorAddress);

                    _imageStream.Seek(fileOffset, SeekOrigin.Begin);
                    _imageStream.EnsureRead(buffer, 16, trk.RawBytesPerSector);
                }

                break;
            }
            default:
                return ErrorNumber.NotSupported;
        }

        sectorStatus = SectorStatus.Dumped;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        if(trk is null) return ErrorNumber.SectorNotFound;

        if(sectorAddress + length - 1 > trk.EndSector) return ErrorNumber.OutOfRange;

        using var ms = new MemoryStream();
        sectorStatus = new SectorStatus[length];

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno =
                ReadSectorLong(sectorAddress + i, track, out byte[] sectorData, out SectorStatus status);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(sectorData, 0, sectorData.Length);
            sectorStatus[i] = status;
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong            sectorAddress, bool negative, out byte[] buffer,
                                      out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(negative) return ErrorNumber.NotSupported;

        Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.StartSector && sectorAddress <= t.EndSector);

        if(trk is null) return ErrorNumber.SectorNotFound;

        return ReadSectorLong(sectorAddress, trk.Sequence, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(negative) return ErrorNumber.NotSupported;

        Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.StartSector && sectorAddress <= t.EndSector);

        if(trk is null) return ErrorNumber.SectorNotFound;

        return ReadSectorsLong(sectorAddress, length, trk.Sequence, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        if(trk is null) return ErrorNumber.SectorNotFound;

        if(sectorAddress < trk.StartSector || sectorAddress > trk.EndSector) return ErrorNumber.OutOfRange;

        switch(tag)
        {
            case SectorTagType.CdTrackFlags:
            {
                var cdFlags = (CdFlags)0;

                if(_trackFlags.TryGetValue(trk.Sequence, out byte c2dFlags))
                {
                    if((c2dFlags & (byte)C2dFlag.PreEmphasis) != 0) cdFlags |= CdFlags.PreEmphasis;
                    if((c2dFlags & (byte)C2dFlag.Copyright)   != 0) cdFlags |= CdFlags.CopyPermitted;
                    if((c2dFlags & (byte)C2dFlag.Data)        != 0) cdFlags |= CdFlags.DataTrack;
                }
                else if(trk.Type != TrackType.Audio) cdFlags = CdFlags.DataTrack;

                buffer = [(byte)cdFlags];

                return ErrorNumber.NoError;
            }
            case SectorTagType.CdSectorSync:
            case SectorTagType.CdSectorHeader:
            case SectorTagType.CdSectorEdc:
            case SectorTagType.CdSectorEccP:
            case SectorTagType.CdSectorEccQ:
            case SectorTagType.CdSectorEcc:
                if(trk.Type == TrackType.Audio) return ErrorNumber.NotSupported;

                break;
            case SectorTagType.CdSectorSubHeader:
                if(trk.Type != TrackType.CdMode2Form1 &&
                   trk.Type != TrackType.CdMode2Form2 &&
                   trk.Type != TrackType.CdMode2Formless)
                    return ErrorNumber.NotSupported;

                break;
            case SectorTagType.CdTrackIsrc:
                if(!_trackIsrcs.TryGetValue(trk.Sequence, out string isrc)) return ErrorNumber.NoData;

                buffer = Encoding.ASCII.GetBytes(isrc);

                return ErrorNumber.NoError;
            case SectorTagType.CdSectorSubchannel:
                if(trk.SubchannelType == TrackSubchannelType.None) return ErrorNumber.NotSupported;

            {
                ulong offsetInTrack = sectorAddress         - trk.StartSector;
                int   stride        = trk.RawBytesPerSector + 96;
                var   fileOffset    = (long)(trk.FileOffset + offsetInTrack * (ulong)stride);

                buffer = new byte[96];
                _imageStream.Seek(fileOffset + trk.RawBytesPerSector, SeekOrigin.Begin);
                _imageStream.EnsureRead(buffer, 0, 96);
            }

                return ErrorNumber.NoError;
            default:
                return ErrorNumber.NotSupported;
        }

        ErrorNumber errno = ReadSectorLong(sectorAddress, track, out byte[] fullSector, out SectorStatus _);

        if(errno != ErrorNumber.NoError) return errno;

        switch(tag)
        {
            case SectorTagType.CdSectorSync:
                buffer = new byte[12];
                Array.Copy(fullSector, 0, buffer, 0, 12);

                break;
            case SectorTagType.CdSectorHeader:
                buffer = new byte[4];
                Array.Copy(fullSector, 12, buffer, 0, 4);

                break;
            case SectorTagType.CdSectorSubHeader:
                buffer = new byte[8];
                Array.Copy(fullSector, 16, buffer, 0, 8);

                break;
            case SectorTagType.CdSectorEdc:
                buffer = new byte[4];

                if(trk.Type == TrackType.CdMode1)
                    Array.Copy(fullSector, 2064, buffer, 0, 4);
                else if(trk.Type == TrackType.CdMode2Form1)
                    Array.Copy(fullSector, 2072, buffer, 0, 4);
                else
                    return ErrorNumber.NotSupported;

                break;
            case SectorTagType.CdSectorEccP:
                if(trk.Type == TrackType.CdMode2Formless) return ErrorNumber.NotSupported;

                buffer = new byte[172];
                Array.Copy(fullSector, 2076, buffer, 0, 172);

                break;
            case SectorTagType.CdSectorEccQ:
                if(trk.Type == TrackType.CdMode2Formless) return ErrorNumber.NotSupported;

                buffer = new byte[104];
                Array.Copy(fullSector, 2248, buffer, 0, 104);

                break;
            case SectorTagType.CdSectorEcc:
                if(trk.Type == TrackType.CdMode2Formless) return ErrorNumber.NotSupported;

                buffer = new byte[276];
                Array.Copy(fullSector, 2076, buffer, 0, 276);

                break;
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, uint length, uint track, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        if(trk is null) return ErrorNumber.SectorNotFound;

        if(sectorAddress + length - 1 > trk.EndSector) return ErrorNumber.OutOfRange;

        if(tag == SectorTagType.CdTrackFlags) return ReadSectorTag(sectorAddress, track, tag, out buffer);

        using var ms = new MemoryStream();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber errno = ReadSectorTag(sectorAddress + i, track, tag, out byte[] tagData);

            if(errno != ErrorNumber.NoError) return errno;

            ms.Write(tagData, 0, tagData.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, bool negative, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(negative) return ErrorNumber.NotSupported;

        Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.StartSector && sectorAddress <= t.EndSector);

        if(trk is null) return ErrorNumber.SectorNotFound;

        return ReadSectorTag(sectorAddress, trk.Sequence, tag, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, bool negative, uint length, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if(negative) return ErrorNumber.NotSupported;

        Track trk = Tracks.FirstOrDefault(t => sectorAddress >= t.StartSector && sectorAddress <= t.EndSector);

        if(trk is null) return ErrorNumber.SectorNotFound;

        return ReadSectorsTag(sectorAddress, length, trk.Sequence, tag, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        switch(tag)
        {
            case MediaTagType.CD_MCN:
                if(_mcn == null) return ErrorNumber.NoData;

                buffer = Encoding.ASCII.GetBytes(_mcn);

                return ErrorNumber.NoError;
            case MediaTagType.CD_TEXT:
                if(_cdtext == null) return ErrorNumber.NoData;

                buffer = _cdtext;

                return ErrorNumber.NoError;
            default:
                return ErrorNumber.NotSupported;
        }
    }

    /// <inheritdoc />
    public List<Track> GetSessionTracks(Session session) => Tracks.Where(t => t.Session == session.Sequence).ToList();

    /// <inheritdoc />
    public List<Track> GetSessionTracks(ushort session) => Tracks.Where(t => t.Session == session).ToList();
}