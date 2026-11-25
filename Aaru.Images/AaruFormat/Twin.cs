using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.Images;

public sealed partial class AaruFormat
{
    // AARU_EXPORT int32_t AARU_CALL aaruf_get_sector_twin_count(const void *context, uint64_t sector_address,
    // bool negative, uint32_t *count)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_get_sector_twin_count", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_get_sector_twin_count(IntPtr context, ulong sectorAddress,
                                                              [MarshalAs(UnmanagedType.I1)] bool negative,
                                                              out                           uint count);

    // AARU_EXPORT int32_t AARU_CALL aaruf_read_sector_twin(void *context, uint64_t sector_address, bool negative,
    // uint32_t twin_index, uint8_t *data, uint32_t *length, uint8_t *sector_status)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_read_sector_twin", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_read_sector_twin(IntPtr context, ulong sectorAddress,
                                                         [MarshalAs(UnmanagedType.I1)] bool negative, uint twinIndex,
                                                         byte[] data, ref uint length, out byte sectorStatus);

    // AARU_EXPORT int32_t AARU_CALL aaruf_read_sector_twin_long(void *context, uint64_t sector_address,
    // bool negative, uint32_t twin_index, uint8_t *data, uint32_t *length, uint8_t *sector_status)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_read_sector_twin_long", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_read_sector_twin_long(IntPtr context, ulong sectorAddress,
                                                              [MarshalAs(UnmanagedType.I1)] bool negative,
                                                              uint twinIndex, byte[] data, ref uint length,
                                                              out byte sectorStatus);

    // AARU_EXPORT int32_t AARU_CALL aaruf_read_sector_twin_tag(const void *context, uint64_t sector_address,
    // bool negative, uint32_t twin_index, uint8_t *buffer, uint32_t *length, int32_t tag)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_read_sector_twin_tag", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_read_sector_twin_tag(IntPtr context, ulong sectorAddress,
                                                             [MarshalAs(UnmanagedType.I1)] bool negative,
                                                             uint twinIndex, byte[] buffer, ref uint length,
                                                             SectorTagType tag);

    // AARU_EXPORT int32_t AARU_CALL aaruf_write_sector_twin(void *context, uint64_t sector_address, bool negative,
    // uint32_t twin_index, const uint8_t *data, uint8_t sector_status, uint32_t length)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_write_sector_twin", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_write_sector_twin(IntPtr context, ulong sectorAddress,
                                                          [MarshalAs(UnmanagedType.I1)] bool negative, uint twinIndex,
                                                          [In] byte[] data, SectorStatus sectorStatus, uint length);

    // AARU_EXPORT int32_t AARU_CALL aaruf_write_sector_twin_long(void *context, uint64_t sector_address,
    // bool negative, uint32_t twin_index, const uint8_t *data, uint8_t sector_status, uint32_t length)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_write_sector_twin_long", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_write_sector_twin_long(IntPtr context, ulong sectorAddress,
                                                               [MarshalAs(UnmanagedType.I1)] bool negative,
                                                               uint twinIndex, [In] byte[] data,
                                                               SectorStatus sectorStatus, uint length);

    // AARU_EXPORT int32_t AARU_CALL aaruf_write_sector_twin_tag(void *context, uint64_t sector_address,
    // bool negative, uint32_t twin_index, const uint8_t *data, size_t length, int32_t tag)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_write_sector_twin_tag", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_write_sector_twin_tag(IntPtr context, ulong sectorAddress,
                                                              [MarshalAs(UnmanagedType.I1)] bool negative,
                                                              uint twinIndex, [In] byte[] data, nuint length,
                                                              SectorTagType tag);

    // AARU_EXPORT int32_t AARU_CALL aaruf_clear_sector_twins(void *context, uint64_t sector_address, bool negative)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_clear_sector_twins", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_clear_sector_twins(IntPtr context, ulong sectorAddress,
                                                           [MarshalAs(UnmanagedType.I1)] bool negative);

    // AARU_EXPORT int32_t AARU_CALL aaruf_get_twin_sectors(const void *context, TwinSectorInfo *buffer,
    // size_t *length)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_get_twin_sectors", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_get_twin_sectors(IntPtr context, IntPtr buffer, ref nuint length);

    /// <summary>
    ///     Reads one physical instance of a sector, in user data or long form.
    /// </summary>
    /// <param name="sectorAddress">Sector address (LBA).</param>
    /// <param name="negative">If the sector address is a negative LBA.</param>
    /// <param name="index">Index of the twin sector (0 or higher).</param>
    /// <param name="raw">True to read the sector in long (raw) form.</param>
    /// <param name="buffer">The sector data.</param>
    /// <param name="sectorStatus">The status of the sector.</param>
    /// <returns>Error status</returns>
    ErrorNumber ReadSectorTwin(ulong            sectorAddress, bool negative, uint index, bool raw, out byte[] buffer,
                               out SectorStatus sectorStatus)
    {
        buffer = null;
        uint length = 0;
        sectorStatus = SectorStatus.NotDumped;

        Status res = raw
                         ? aaruf_read_sector_twin_long(_context,
                                                       sectorAddress,
                                                       negative,
                                                       index,
                                                       buffer,
                                                       ref length,
                                                       out _)
                         : aaruf_read_sector_twin(_context, sectorAddress, negative, index, buffer, ref length, out _);

        // Sector not dumped
        if(res == Status.SectorNotDumped && length > 0)
        {
            buffer = new byte[length];

            return ErrorNumber.NoError;
        }

        if(res != Status.BufferTooSmall) return StatusToErrorNumber(res);

        buffer = new byte[length];
        byte libSectorStatus;

        res = raw
                  ? aaruf_read_sector_twin_long(_context,
                                                sectorAddress,
                                                negative,
                                                index,
                                                buffer,
                                                ref length,
                                                out libSectorStatus)
                  : aaruf_read_sector_twin(_context,
                                           sectorAddress,
                                           negative,
                                           index,
                                           buffer,
                                           ref length,
                                           out libSectorStatus);

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

#region ITwinSectorsImage Members

    /// <inheritdoc />
    public ErrorNumber GetSectorTwinCount(ulong sectorAddress, bool negative, out uint count)
    {
        Status res = aaruf_get_sector_twin_count(_context, sectorAddress, negative, out count);

        return StatusToErrorNumber(res);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTwin(ulong            sectorAddress, bool negative, uint index, out byte[] buffer,
                                      out SectorStatus sectorStatus)
    {
        return ReadSectorTwin(sectorAddress, negative, index, false, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTwinLong(ulong            sectorAddress, bool negative, uint index, out byte[] buffer,
                                          out SectorStatus sectorStatus)
    {
        return ReadSectorTwin(sectorAddress, negative, index, true, out buffer, out sectorStatus);
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorTwinTag(ulong      sectorAddress, bool negative, uint index, SectorTagType tag,
                                         out byte[] buffer)
    {
        // Track level tags are shared by all instances, and we cache them ourselves
        if(tag is SectorTagType.CdTrackIsrc or SectorTagType.CdTrackFlags)
            return ReadSectorTag(sectorAddress, negative, tag, out buffer);

        buffer = null;
        uint length = 0;

        Status res = aaruf_read_sector_twin_tag(_context, sectorAddress, negative, index, buffer, ref length, tag);

        if(res != Status.BufferTooSmall && res != Status.IncorrectDataSize) return StatusToErrorNumber(res);

        buffer = new byte[length];

        res = aaruf_read_sector_twin_tag(_context, sectorAddress, negative, index, buffer, ref length, tag);

        // Totally legal, we need to cut buffer size then
        if(length < buffer.Length && length > 0) Array.Resize(ref buffer, (int)length);

        return StatusToErrorNumber(res);
    }

    /// <inheritdoc />
    public ErrorNumber WriteSectorTwin(ulong        sectorAddress, bool negative, uint index, byte[] buffer,
                                       SectorStatus sectorStatus)
    {
        Status res = aaruf_write_sector_twin(_context,
                                             sectorAddress,
                                             negative,
                                             index,
                                             buffer,
                                             sectorStatus,
                                             (uint)buffer.Length);

        if(res != Status.Ok) ErrorMessage = StatusToErrorMessage(res);

        return StatusToErrorNumber(res);
    }

    /// <inheritdoc />
    public ErrorNumber WriteSectorTwinLong(ulong        sectorAddress, bool negative, uint index, byte[] buffer,
                                           SectorStatus sectorStatus)
    {
        Status res = aaruf_write_sector_twin_long(_context,
                                                  sectorAddress,
                                                  negative,
                                                  index,
                                                  buffer,
                                                  sectorStatus,
                                                  (uint)buffer.Length);

        if(res != Status.Ok) ErrorMessage = StatusToErrorMessage(res);

        return StatusToErrorNumber(res);
    }

    /// <inheritdoc />
    public ErrorNumber WriteSectorTwinTag(ulong         sectorAddress, bool negative, uint index, byte[] buffer,
                                          SectorTagType tag)
    {
        // Track level tags are shared by all instances, and we cache them ourselves
        if(tag is SectorTagType.CdTrackIsrc or SectorTagType.CdTrackFlags)
        {
            return WriteSectorTag(buffer, sectorAddress, negative, tag)
                       ? ErrorNumber.NoError
                       : ErrorNumber.InvalidArgument;
        }

        Status res = aaruf_write_sector_twin_tag(_context,
                                                 sectorAddress,
                                                 negative,
                                                 index,
                                                 buffer,
                                                 (nuint)buffer.Length,
                                                 tag);

        if(res != Status.Ok) ErrorMessage = StatusToErrorMessage(res);

        return StatusToErrorNumber(res);
    }

    /// <inheritdoc />
    public ErrorNumber ClearSectorTwins(ulong sectorAddress, bool negative)
    {
        Status res = aaruf_clear_sector_twins(_context, sectorAddress, negative);

        if(res != Status.Ok) ErrorMessage = StatusToErrorMessage(res);

        return StatusToErrorNumber(res);
    }

    /// <inheritdoc />
    public ErrorNumber GetTwinSectors(out List<TwinSectorInfo> twinSectors)
    {
        twinSectors = null;

        nuint  length = 0;
        Status res    = aaruf_get_twin_sectors(_context, IntPtr.Zero, ref length);

        if(res == Status.Ok && length == 0)
        {
            twinSectors = [];

            return ErrorNumber.NoError;
        }

        if(res != Status.BufferTooSmall) return StatusToErrorNumber(res);

        IntPtr buffer = Marshal.AllocHGlobal((int)length);

        try
        {
            res = aaruf_get_twin_sectors(_context, buffer, ref length);

            if(res != Status.Ok) return StatusToErrorNumber(res);

            int structSize = Marshal.SizeOf<TwinSectorInfoEntry>();
            int count      = (int)length / structSize;

            twinSectors = new List<TwinSectorInfo>(count);

            for(int i = 0; i < count; i++)
            {
                TwinSectorInfoEntry entry =
                    Marshal.PtrToStructure<TwinSectorInfoEntry>(IntPtr.Add(buffer, i * structSize));

                twinSectors.Add(new TwinSectorInfo
                {
                    sectorAddress = entry.SectorAddress,
                    count         = entry.Count,
                    negative      = entry.Negative != 0
                });
            }

            return ErrorNumber.NoError;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

#endregion
}