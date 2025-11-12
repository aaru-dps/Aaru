using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.Images;

public sealed partial class AaruFormat
{
    // AARU_EXPORT int32_t AARU_CALL aaruf_read_media_tag(void *context, uint8_t *data, const int32_t tag, uint32_t *length)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_read_media_tag", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_read_media_tag(IntPtr context, byte[] data, MediaTagType tag, ref uint length);

    // AARU_EXPORT int32_t AARU_CALL aaruf_read_sector(void *context, const uint64_t sector_address, bool negative,
    // uint8_t *data, uint32_t *length, uint8_t *sector_status)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_read_sector", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_read_sector(IntPtr                             context,  ulong    sectorAddress,
                                                    [MarshalAs(UnmanagedType.I1)] bool negative, byte[]   data,
                                                    ref                           uint length,   out byte sectorStatus);

    // AARU_EXPORT int32_t AARU_CALL aaruf_read_track_sector(void *context, uint8_t *data, const uint64_t sector_address,
    // uint32_t *length, const uint8_t track, uint8_t *sector_status)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_read_track_sector", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_read_track_sector(IntPtr   context, byte[] data,  ulong    sectorAddress,
                                                          ref uint length,  byte   track, out byte sectorStatus);

    // AARU_EXPORT int32_t AARU_CALL aaruf_read_sector_long(void *context, const uint64_t sector_address, bool negative,
    // uint8_t *data, uint32_t *length, uint8_t *sector_status)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_read_sector_long", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_read_sector_long(IntPtr context, ulong sectorAddress,
                                                         [MarshalAs(UnmanagedType.I1)] bool negative, byte[] data,
                                                         ref uint length, out byte sectorStatus);

    // AARU_EXPORT int32_t AARU_CALL aaruf_read_sector_tag(const void *context, const uint64_t sector_address,
    // const bool negative, uint8_t *buffer, uint32_t *length,
    // const int32_t tag)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_read_sector_tag", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_read_sector_tag(IntPtr                             context, ulong sectorAddress,
                                                        [MarshalAs(UnmanagedType.I1)] bool negative, byte[] buffer,
                                                        ref                           uint length, SectorTagType tag);

#region IWritableOpticalImage Members

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        uint length = 0;

        Status res = aaruf_read_media_tag(_context, buffer, tag, ref length);

        if(res != Status.BufferTooSmall) return StatusToErrorNumber(res);

        buffer = new byte[length];

        res = aaruf_read_media_tag(_context, buffer, tag, ref length);

        // Totally legal, we need to cut buffer size then
        if(length < buffer.Length && length > 0) Array.Resize(ref buffer, (int)length);

        return StatusToErrorNumber(res);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, bool negative, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer = null;
        uint length = 0;
        sectorStatus = SectorStatus.NotDumped;

        Status res = aaruf_read_sector(_context, sectorAddress, negative, buffer, ref length, out _);

        // Sector not dumped
        if(res == Status.SectorNotDumped && length > 0)
        {
            buffer = new byte[length];

            return ErrorNumber.NoError;
        }

        if(res != Status.BufferTooSmall) return StatusToErrorNumber(res);

        buffer = new byte[length];

        res = aaruf_read_sector(_context, sectorAddress, negative, buffer, ref length, out byte libSectorStatus);

        sectorStatus = (SectorStatus)libSectorStatus;

        // Sector not dumped
        if(res == Status.SectorNotDumped && length > 0)
        {
            buffer = new byte[length];

            return ErrorNumber.NoError;
        }

        // Totally legal, we need to cut buffer size then
        if(length < buffer.Length && length > 0) Array.Resize(ref buffer, (int)length);

        return StatusToErrorNumber(res);
    }

    public ErrorNumber ReadDPM(out uint dpmStartSector, out uint dpmResolution, out uint numberOfDpmEntries, out ulong[] dpm)
    {
        dpmStartSector = 0;
        dpmResolution = 0;
        numberOfDpmEntries = 0;
        dpm = null;

        return ErrorNumber.NotSupported;
    }

    public ErrorNumber ReadSectorDPM(ulong sectorAddress, out ulong? dpm)
    {
        dpm = null;

        return ErrorNumber.NotSupported;
    }

    public ErrorNumber ReadDPM(out uint dpmStartSector, out uint dpmResolution, out uint numberOfDpmEntries, out ulong[] dpm)
    {
        dpmStartSector = 0;
        dpmResolution = 0;
        numberOfDpmEntries = 0;
        dpm = null;

        return ErrorNumber.NotSupported;
    }

    public ErrorNumber ReadSectorDPM(ulong sectorAddress, out ulong? dpm)
    {
        dpm = null;

        return ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer = null;
        uint length = 0;
        sectorStatus = SectorStatus.NotDumped;

        Status res = aaruf_read_track_sector(_context, buffer, sectorAddress, ref length, (byte)track, out _);

        // Sector not dumped
        if(res == Status.SectorNotDumped && length > 0)
        {
            buffer = new byte[length];

            return ErrorNumber.NoError;
        }

        if(res != Status.BufferTooSmall) return StatusToErrorNumber(res);

        buffer = new byte[length];

        res = aaruf_read_track_sector(_context,
                                      buffer,
                                      sectorAddress,
                                      ref length,
                                      (byte)track,
                                      out byte libSectorStatus);

        sectorStatus = (SectorStatus)libSectorStatus;

        // Sector not dumped
        if(res == Status.SectorNotDumped && length > 0)
        {
            buffer = new byte[length];

            return ErrorNumber.NoError;
        }

        // Totally legal, we need to cut buffer size then
        if(length < buffer.Length && length > 0) Array.Resize(ref buffer, (int)length);

        return StatusToErrorNumber(res);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong            sectorAddress, bool negative, out byte[] buffer,
                                      out SectorStatus sectorStatus)
    {
        buffer = null;
        uint length = 0;
        sectorStatus = SectorStatus.NotDumped;

        Status res = aaruf_read_sector_long(_context, sectorAddress, negative, buffer, ref length, out _);

        // Sector not dumped
        if(res == Status.SectorNotDumped && length > 0)
        {
            buffer = new byte[length];

            return ErrorNumber.NoError;
        }

        if(res != Status.BufferTooSmall) return StatusToErrorNumber(res);

        buffer = new byte[length];

        res = aaruf_read_sector_long(_context, sectorAddress, negative, buffer, ref length, out byte libSectorStatus);

        sectorStatus = (SectorStatus)libSectorStatus;

        // Sector not dumped
        if(res == Status.SectorNotDumped && length > 0)
        {
            buffer = new byte[length];

            return ErrorNumber.NoError;
        }

        // Totally legal, we need to cut buffer size then
        if(length < buffer.Length && length > 0) Array.Resize(ref buffer, (int)length);

        return StatusToErrorNumber(res);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, bool negative, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        switch(tag)
        {
            // Due to how we cache the tracks, we need to handle ISRC tags ourselves
            case SectorTagType.CdTrackIsrc when _trackIsrcs != null:
            {
                if(_trackIsrcs.TryGetValue((int)sectorAddress, out string isrc))
                {
                    buffer = new byte[13];
                    byte[] isrcBytes = Encoding.UTF8.GetBytes(isrc ?? "");
                    Array.Copy(isrcBytes, 0, buffer, 0, Math.Min(isrcBytes.Length, 13));

                    return ErrorNumber.NoError;
                }

                break;
            }

            // Due to how we cache the tracks, we need to handle Track Flags ourselves
            case SectorTagType.CdTrackFlags when _trackFlags != null:
            {
                if(_trackFlags.TryGetValue((int)sectorAddress, out byte flags))
                {
                    buffer    = new byte[1];
                    buffer[0] = flags;

                    return ErrorNumber.NoError;
                }

                break;
            }
        }

        uint length = 0;

        Status res = aaruf_read_sector_tag(_context, sectorAddress, negative, buffer, ref length, tag);

        if(res != Status.BufferTooSmall && res != Status.IncorrectDataSize) return StatusToErrorNumber(res);

        buffer = new byte[length];

        res = aaruf_read_sector_tag(_context, sectorAddress, negative, buffer, ref length, tag);

        // Totally legal, we need to cut buffer size then
        if(length < buffer.Length && length > 0) Array.Resize(ref buffer, (int)length);

        return StatusToErrorNumber(res);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        MemoryStream ms = new();
        sectorStatus = new SectorStatus[length];

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber res = ReadSector(sectorAddress + i,
                                         negative,
                                         out byte[] sectorBuffer,
                                         out SectorStatus singleSectorStatus);

            sectorStatus[i] = singleSectorStatus;

            if(res != ErrorNumber.NoError)
            {
                buffer = ms.ToArray();

                return res;
            }

            ms.Write(sectorBuffer, 0, sectorBuffer.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, bool negative, uint length, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        MemoryStream ms = new();
        sectorStatus = new SectorStatus[length];

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber res = ReadSectorLong(sectorAddress + i,
                                             negative,
                                             out byte[] sectorBuffer,
                                             out SectorStatus singleSectorStatus);

            sectorStatus[i] = singleSectorStatus;

            if(res != ErrorNumber.NoError)
            {
                buffer = ms.ToArray();

                return res;
            }

            ms.Write(sectorBuffer, 0, sectorBuffer.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, bool negative, uint length, SectorTagType tag,
                                      out byte[] buffer)
    {
        MemoryStream ms = new();

        for(uint i = 0; i < length; i++)
        {
            ErrorNumber res = ReadSectorTag(sectorAddress + i, negative, tag, out byte[] sectorBuffer);

            if(res != ErrorNumber.NoError)
            {
                buffer = ms.ToArray();

                return res;
            }

            ms.Write(sectorBuffer, 0, sectorBuffer.Length);
        }

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc) return ErrorNumber.NotSupported;

        if(Tracks == null) return ErrorNumber.SectorNotFound;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        return trk?.Sequence != track
                   ? ErrorNumber.SectorNotFound
                   : ReadSectorTag(trk.StartSector + sectorAddress, false, tag, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                   out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc) return ErrorNumber.NotSupported;

        if(Tracks == null) return ErrorNumber.SectorNotFound;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        if(trk?.Sequence != track) return ErrorNumber.SectorNotFound;

        return trk.StartSector + sectorAddress + length > trk.EndSector + 1
                   ? ErrorNumber.OutOfRange
                   : ReadSectors(trk.StartSector + sectorAddress, false, length, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, uint length, uint track, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc) return ErrorNumber.NotSupported;

        if(Tracks == null) return ErrorNumber.SectorNotFound;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        return trk?.Sequence != track
                   ? ErrorNumber.SectorNotFound
                   : trk.StartSector + sectorAddress + length > trk.EndSector + 1
                       ? ErrorNumber.OutOfRange
                       : ReadSectorsTag(trk.StartSector + sectorAddress, false, length, tag, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer, out SectorStatus sectorStatus)
    {
        buffer       = null;
        sectorStatus = SectorStatus.NotDumped;

        if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc) return ErrorNumber.NotSupported;

        if(Tracks == null) return ErrorNumber.SectorNotFound;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        return trk?.Sequence != track
                   ? ErrorNumber.SectorNotFound
                   : ReadSectorLong(trk.StartSector + sectorAddress, false, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong              sectorAddress, uint length, uint track, out byte[] buffer,
                                       out SectorStatus[] sectorStatus)
    {
        buffer       = null;
        sectorStatus = null;

        if(_imageInfo.MetadataMediaType != MetadataMediaType.OpticalDisc) return ErrorNumber.NotSupported;

        if(Tracks == null) return ErrorNumber.SectorNotFound;

        Track trk = Tracks.FirstOrDefault(t => t.Sequence == track);

        return trk?.Sequence != track
                   ? ErrorNumber.SectorNotFound
                   : trk.StartSector + sectorAddress + length > trk.EndSector + 1
                       ? ErrorNumber.OutOfRange
                       : ReadSectorsLong(trk.StartSector + sectorAddress, false, length, out buffer, out sectorStatus);
    }

#endregion
}