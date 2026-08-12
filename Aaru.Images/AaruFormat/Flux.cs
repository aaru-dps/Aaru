using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Aaru.Images;

public sealed partial class AaruFormat
{
    List<FluxCapture> _fluxCaptures;

#region IWritableFluxImage Members

    /// <inheritdoc />
    public List<FluxCapture> FluxCaptures
    {
        get
        {
            if(_fluxCaptures is not null) return _fluxCaptures;

            nuint length = 0;

            Status res = aaruf_get_flux_captures(_context, null, ref length);

            if(res != Status.BufferTooSmall)
            {
                ErrorMessage = StatusToErrorMessage(res);

                return [];
            }

            byte[] buffer = new byte[length];

            res = aaruf_get_flux_captures(_context, buffer, ref length);

            if(res != Status.Ok)
            {
                ErrorMessage = StatusToErrorMessage(res);

                return [];
            }

            int fluxCaptureSize  = Marshal.SizeOf<FluxCaptureEntry>();
            int fluxCaptureCount = (int)length / fluxCaptureSize;

            _fluxCaptures = new List<FluxCapture>(fluxCaptureCount);

            IntPtr ptr = Marshal.AllocHGlobal(fluxCaptureCount * fluxCaptureSize);

            try
            {
                Marshal.Copy(buffer, 0, ptr, (int)length);

                for(int i = 0; i < fluxCaptureCount; i++)
                {
                    nint             fluxCapturePtr = IntPtr.Add(ptr, i * fluxCaptureSize);
                    FluxCaptureEntry entry          = Marshal.PtrToStructure<FluxCaptureEntry>(fluxCapturePtr);

                    var capture = new FluxCapture
                    {
                        Head            = entry.Head,
                        Track           = entry.Track,
                        SubTrack        = entry.SubTrack,
                        CaptureIndex    = entry.CaptureIndex,
                        IndexResolution = entry.IndexResolution,
                        DataResolution  = entry.DataResolution,
                    };

                    _fluxCaptures.Add(capture);
                }
            }
            catch
            {
                // Callers expect a list, never null
                _fluxCaptures = [];
#pragma warning disable ERP022
            }
#pragma warning restore ERP022
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return _fluxCaptures;
        }
    }

    /// <inheritdoc />
    public ErrorNumber CapturesLength(uint head, ushort track, byte subTrack, out uint length)
    {
        length = (uint)FluxCaptures
                      .FindAll(capture => capture.Head     == head  &&
                                          capture.Track    == track &&
                                          capture.SubTrack == subTrack)
                      .Count;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxIndexResolution(uint      head, ushort track, byte subTrack, uint captureIndex,
                                               out ulong resolution)
    {
        FluxCapture capture = FluxCaptures.Find(capture => capture.Head         == head     &&
                                                           capture.Track        == track    &&
                                                           capture.SubTrack     == subTrack &&
                                                           capture.CaptureIndex == captureIndex);

        resolution = 0;

        if(capture is null) return ErrorNumber.SectorNotFound;

        resolution = capture.IndexResolution;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxDataResolution(uint      head, ushort track, byte subTrack, uint captureIndex,
                                              out ulong resolution)
    {
        FluxCapture capture = FluxCaptures.Find(capture => capture.Head         == head     &&
                                                           capture.Track        == track    &&
                                                           capture.SubTrack     == subTrack &&
                                                           capture.CaptureIndex == captureIndex);

        resolution = 0;

        if(capture is null) return ErrorNumber.SectorNotFound;

        resolution = capture.DataResolution;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxResolution(uint      head,            ushort    track, byte subTrack, uint captureIndex,
                                          out ulong indexResolution, out ulong dataResolution)
    {
        FluxCapture capture = FluxCaptures.Find(capture => capture.Head         == head     &&
                                                           capture.Track        == track    &&
                                                           capture.SubTrack     == subTrack &&
                                                           capture.CaptureIndex == captureIndex);

        indexResolution = 0;
        dataResolution  = 0;

        if(capture is null) return ErrorNumber.SectorNotFound;

        indexResolution = capture.IndexResolution;
        dataResolution  = capture.DataResolution;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxCapture(uint       head,            ushort    track, byte subTrack, uint captureIndex,
                                       out ulong  indexResolution, out ulong dataResolution, out byte[] indexBuffer,
                                       out byte[] dataBuffer)
    {
        FluxCapture capture = FluxCaptures.Find(capture => capture.Head         == head     &&
                                                           capture.Track        == track    &&
                                                           capture.SubTrack     == subTrack &&
                                                           capture.CaptureIndex == captureIndex);

        if(capture is null)
        {
            indexResolution = 0;
            dataResolution  = 0;
            indexBuffer     = null;
            dataBuffer      = null;

            return ErrorNumber.SectorNotFound;
        }

        nuint indexLength = 0;
        nuint dataLength  = 0;

        Status res = aaruf_read_flux_capture(_context,
                                             head,
                                             track,
                                             subTrack,
                                             captureIndex,
                                             null,
                                             ref indexLength,
                                             null,
                                             ref dataLength);

        if(res != Status.BufferTooSmall)
        {
            indexResolution = 0;
            dataResolution  = 0;
            indexBuffer     = null;
            dataBuffer      = null;

            return StatusToErrorNumber(res);
        }

        indexBuffer = new byte[indexLength];
        dataBuffer  = new byte[dataLength];

        res = aaruf_read_flux_capture(_context,
                                      head,
                                      track,
                                      subTrack,
                                      captureIndex,
                                      indexBuffer,
                                      ref indexLength,
                                      dataBuffer,
                                      ref dataLength);

        if(res != Status.Ok)
        {
            indexResolution = 0;
            dataResolution  = 0;
            indexBuffer     = null;
            dataBuffer      = null;

            return StatusToErrorNumber(res);
        }

        indexResolution = capture.IndexResolution;
        dataResolution  = capture.DataResolution;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxIndexCapture(uint       head, ushort track, byte subTrack, uint captureIndex,
                                            out byte[] buffer)
    {
        return ReadFluxCapture(head, track, subTrack, captureIndex, out _, out _, out buffer, out _);
    }

    /// <inheritdoc />
    public ErrorNumber ReadFluxDataCapture(uint head, ushort track, byte subTrack, uint captureIndex, out byte[] buffer)
    {
        return ReadFluxCapture(head, track, subTrack, captureIndex, out _, out _, out _, out buffer);
    }

    /// <inheritdoc />
    public ErrorNumber SubTrackLength(uint head, ushort track, out byte length)
    {
        List<FluxCapture> found = FluxCaptures.FindAll(capture => capture.Head == head && capture.Track == track);

        length = 0;

        if(found.Count == 0) return ErrorNumber.SectorNotFound;

        length = (byte)(found.Max(capture => capture.SubTrack) + 1);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetAllFluxCaptures(out List<FluxCapture> captures)
    {
        captures = FluxCaptures;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber WriteFluxCapture(ulong  indexResolution, ulong dataResolution, byte[] indexBuffer,
                                        byte[] dataBuffer, uint head, ushort track, byte subTrack, uint captureIndex)
    {
        Status res = aaruf_write_flux_capture(_context,
                                              head,
                                              track,
                                              subTrack,
                                              captureIndex,
                                              dataResolution,
                                              indexResolution,
                                              dataBuffer,
                                              (uint)dataBuffer.Length,
                                              indexBuffer,
                                              (uint)indexBuffer.Length);

        if(res != Status.Ok) return StatusToErrorNumber(res);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber WriteFluxIndexCapture(ulong resolution, byte[] index, uint head, ushort track, byte subTrack,
                                             uint  captureIndex)
    {
        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber WriteFluxDataCapture(ulong resolution, byte[] data, uint head, ushort track, byte subTrack,
                                            uint  captureIndex)
    {
        return ErrorNumber.NotImplemented;
    }

#endregion

    // AARU_EXPORT int32_t AARU_CALL aaruf_get_flux_captures(void *context, uint8_t *buffer, size_t *length)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_get_flux_captures", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_get_flux_captures(IntPtr context, byte[] buffer, ref nuint length);

    // AARU_EXPORT int32_t AARU_CALL aaruf_read_flux_capture(void *context, uint32_t head, uint16_t track, uint8_t subtrack,
    //                                                       uint8_t capture_index, uint8_t *index_data,
    //                                                       uint32_t *index_length, uint8_t *data_data,
    //                                                       uint32_t *data_length)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_read_flux_capture", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_read_flux_capture(IntPtr context,      uint head, ushort track, byte subtrack,
                                                          uint   captureIndex, byte[] indexData, ref nuint indexLength,
                                                          byte[] dataData,     ref nuint dataLength);

    // AARU_EXPORT int32_t AARU_CALL aaruf_write_flux_capture(void *context, uint32_t head, uint16_t track, uint8_t subtrack,
    //                                                        uint16_t capture_index, uint64_t data_resolution,
    //                                                        uint64_t index_resolution, const uint8_t *data,
    //                                                        uint32_t data_length, const uint8_t *index,
    //                                                        uint32_t index_length);
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_write_flux_capture", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_write_flux_capture(IntPtr context, uint head, ushort track, byte subtrack,
                                                           uint   captureIndex, ulong dataResolution,
                                                           ulong  indexResolution, byte[] data, uint dataLength,
                                                           byte[] index, uint indexLength);
}