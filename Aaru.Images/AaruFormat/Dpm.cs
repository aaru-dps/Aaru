using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Aaru.Checksums;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class AaruFormat
{
    /// <summary>Size of the DPM block header</summary>
    const int  DPM_HEADER_SIZE              = 40;
    /// <summary>Size of a DPM entry</summary>
    const int  DPM_ENTRY_SIZE               = 17;
    /// <summary>Size of a DPM calibration</summary>
    const int  DPM_CALIBRATION_SIZE         = 24;
    /// <summary>DPM header flag set when the layers use opposite track path</summary>
    const byte DPM_FLAG_OPPOSITE_TRACK_PATH = 0x01;

    // AARU_EXPORT int32_t AARU_CALL aaruf_get_dpm(const void *context, uint8_t *buffer, size_t *length)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_get_dpm", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_get_dpm(IntPtr context, byte[] buffer, ref nuint length);

    // AARU_EXPORT int32_t AARU_CALL aaruf_set_dpm(void *context, const uint8_t *data, size_t length)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_set_dpm", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_set_dpm(IntPtr context, [In] byte[] data, nuint length);

    // AARU_EXPORT int32_t AARU_CALL aaruf_clear_dpm(void *context)
    [LibraryImport("libaaruformat", EntryPoint = "aaruf_clear_dpm", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static partial Status aaruf_clear_dpm(IntPtr context);

    /// <inheritdoc />
    public ErrorNumber ReadDpm(out DataPositionMeasurement dpm)
    {
        dpm = default(DataPositionMeasurement);

        nuint  length = 0;
        Status res    = aaruf_get_dpm(_context, null, ref length);

        if(res == Status.MetadataNotPresent) return ErrorNumber.NoData;

        if(res != Status.BufferTooSmall) return StatusToErrorNumber(res);

        var block = new byte[length];
        res = aaruf_get_dpm(_context, block, ref length);

        if(res != Status.Ok) return StatusToErrorNumber(res);

        return DecodeDpm(block, out dpm) ? ErrorNumber.NoError : ErrorNumber.InvalidArgument;
    }

    /// <inheritdoc />
    public bool SetDpm(DataPositionMeasurement dpm)
    {
        if(!Dpm.Validate(dpm))
        {
            ErrorMessage = Localization.Invalid_DPM;

            return false;
        }

        byte[] block = EncodeDpm(dpm);
        Status res   = aaruf_set_dpm(_context, block, (nuint)block.Length);

        ErrorMessage = StatusToErrorMessage(res);

        return res == Status.Ok;
    }

    /// <summary>Removes the Data Position Measurement from an image being written or resumed</summary>
    /// <returns><c>true</c> if operating completed successfully, <c>false</c> otherwise</returns>
    public bool ClearDpm()
    {
        Status res = aaruf_clear_dpm(_context);

        ErrorMessage = StatusToErrorMessage(res);

        return res == Status.Ok;
    }

    /// <summary>Serializes a DPM as a libaaruformat DPM block, header included</summary>
    /// <param name="dpm">DPM</param>
    /// <returns>Serialized block</returns>
    static byte[] EncodeDpm(DataPositionMeasurement dpm)
    {
        ulong[]          layerEnds    = dpm.LayerEnds    ?? [];
        DpmCalibration[] calibrations = dpm.Calibrations ?? [];

        int payloadLength = layerEnds.Length    * sizeof(ulong)  +
                            dpm.Entries.Length  * DPM_ENTRY_SIZE +
                            calibrations.Length * DPM_CALIBRATION_SIZE;

        var        block   = new byte[DPM_HEADER_SIZE + payloadLength];
        Span<byte> payload = block.AsSpan(DPM_HEADER_SIZE);
        int        offset  = 0;

        foreach(ulong layerEnd in layerEnds)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(payload[offset..], layerEnd);
            offset += sizeof(ulong);
        }

        foreach(DpmEntry entry in dpm.Entries)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(payload[offset..],       entry.Lba);
            BinaryPrimitives.WriteUInt64LittleEndian(payload[(offset + 8)..], entry.Angle);
            payload[offset + 16] =  (byte)entry.Status;
            offset               += DPM_ENTRY_SIZE;
        }

        foreach(DpmCalibration calibration in calibrations)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(payload[offset..],        calibration.Lba);
            BinaryPrimitives.WriteUInt64LittleEndian(payload[(offset + 8)..],  calibration.RotationPeriod);
            BinaryPrimitives.WriteUInt64LittleEndian(payload[(offset + 16)..], calibration.SectorsPerTurn);
            offset += DPM_CALIBRATION_SIZE;
        }

        Crc64Context.Data(payload.ToArray(), out byte[] crc);

        Span<byte> header = block.AsSpan(0, DPM_HEADER_SIZE);
        BinaryPrimitives.WriteUInt32LittleEndian(header,       (uint)BlockType.DataPositionMeasurementBlock);
        BinaryPrimitives.WriteUInt32LittleEndian(header[4..],  (uint)dpm.Entries.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(header[8..],  (uint)calibrations.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(header[12..], dpm.NominalSpacing);
        BinaryPrimitives.WriteUInt16LittleEndian(header[16..], dpm.TimingUnit);
        BinaryPrimitives.WriteUInt16LittleEndian(header[18..], dpm.Speed);
        header[20] = (byte)layerEnds.Length;
        header[21] = dpm.OppositeTrackPath ? DPM_FLAG_OPPOSITE_TRACK_PATH : (byte)0;
        BinaryPrimitives.WriteUInt64LittleEndian(header[24..], (ulong)payloadLength);
        BinaryPrimitives.WriteUInt64LittleEndian(header[32..], Swapping.Swap(BitConverter.ToUInt64(crc, 0)));

        return block;
    }

    /// <summary>Deserializes a libaaruformat DPM block, header included</summary>
    /// <param name="block">Serialized block</param>
    /// <param name="dpm">DPM</param>
    /// <returns><c>true</c> if the block is consistent, <c>false</c> otherwise</returns>
    static bool DecodeDpm(byte[] block, out DataPositionMeasurement dpm)
    {
        dpm = default(DataPositionMeasurement);

        if(block.Length < DPM_HEADER_SIZE) return false;

        ReadOnlySpan<byte> header = block.AsSpan(0, DPM_HEADER_SIZE);

        if(BinaryPrimitives.ReadUInt32LittleEndian(header) != (uint)BlockType.DataPositionMeasurementBlock)
            return false;

        uint  entries       = BinaryPrimitives.ReadUInt32LittleEndian(header[4..]);
        uint  calibrations  = BinaryPrimitives.ReadUInt32LittleEndian(header[8..]);
        byte  layerEnds     = header[20];
        ulong payloadLength = BinaryPrimitives.ReadUInt64LittleEndian(header[24..]);

        if(payloadLength !=
           (ulong)layerEnds    * sizeof(ulong)  +
           (ulong)entries      * DPM_ENTRY_SIZE +
           (ulong)calibrations * DPM_CALIBRATION_SIZE ||
           payloadLength != (ulong)(block.Length - DPM_HEADER_SIZE))
            return false;

        ReadOnlySpan<byte> payload = block.AsSpan(DPM_HEADER_SIZE);
        int                offset  = 0;

        dpm = new DataPositionMeasurement
        {
            NominalSpacing    = BinaryPrimitives.ReadUInt32LittleEndian(header[12..]),
            TimingUnit        = BinaryPrimitives.ReadUInt16LittleEndian(header[16..]),
            Speed             = BinaryPrimitives.ReadUInt16LittleEndian(header[18..]),
            OppositeTrackPath = (header[21] & DPM_FLAG_OPPOSITE_TRACK_PATH) != 0,
            LayerEnds         = new ulong[layerEnds],
            Entries           = new DpmEntry[entries],
            Calibrations      = new DpmCalibration[calibrations]
        };

        for(int i = 0; i < layerEnds; i++)
        {
            dpm.LayerEnds[i] =  BinaryPrimitives.ReadUInt64LittleEndian(payload[offset..]);
            offset           += sizeof(ulong);
        }

        for(int i = 0; i < entries; i++)
        {
            dpm.Entries[i] = new DpmEntry
            {
                Lba    = BinaryPrimitives.ReadUInt64LittleEndian(payload[offset..]),
                Angle  = BinaryPrimitives.ReadUInt64LittleEndian(payload[(offset + 8)..]),
                Status = (DpmEntryStatus)payload[offset + 16]
            };

            offset += DPM_ENTRY_SIZE;
        }

        for(int i = 0; i < calibrations; i++)
        {
            dpm.Calibrations[i] = new DpmCalibration
            {
                Lba            = BinaryPrimitives.ReadUInt64LittleEndian(payload[offset..]),
                RotationPeriod = BinaryPrimitives.ReadUInt64LittleEndian(payload[(offset + 8)..]),
                SectorsPerTurn = BinaryPrimitives.ReadUInt64LittleEndian(payload[(offset + 16)..])
            };

            offset += DPM_CALIBRATION_SIZE;
        }

        return true;
    }
}