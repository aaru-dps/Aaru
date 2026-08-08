// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ReadBuffer.cs
// Author(s)      : Rebecca Wallander <sakcheen@gmail.com>
//
// Component      : SCSI READ BUFFER command.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains generic SCSI READ BUFFER command implementation.
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
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Devices;

public partial class Device
{
    // ReadBuffer 3C detection fields
    private byte _detectedReadBufferMode; // Detected buffer mode (0x00, 0x01, 0x02)
    private byte _detectedReadBufferId;   // Detected buffer ID (0x00, 0x01, 0x02)
    private uint _detectedBufferStride;   // Detected stride in bytes per sector
    private bool _readBuffer3CDetected;   // Flag to track if detection has been performed

    // Buffer format and management fields
    private enum BufferFormat
    {
        FullEccInterleaved,
        PoOnly,
        SectorDataOnly,
        FullEccWithPadding
    }

    private uint _bufferOffset;
    private uint _bufferCapacityInSectors = 714; // Default capacity, will be refined dynamically when offset is lost
    private BufferFormat _bufferFormat;
    private uint _totalSectorsRead; // Tracks cumulative sectors read successfully since last buffer reset

    /// <summary>Reads from device buffer using SCSI READ BUFFER command with specified variant</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the ReadBuffer response will be stored</param>
    /// <param name="senseBuffer">Sense buffer</param>
    /// <param name="bufferOffset">The offset to read the buffer at</param>
    /// <param name="transferLength">Number of bytes to read</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="duration">Duration in milliseconds</param>
    /// <param name="mode">Buffer mode (CDB byte 1, e.g., 0x01 for data, 0x02 for descriptors)</param>
    /// <param name="bufferId">Buffer ID (CDB byte 2, e.g., 0x01, 0x00)</param>
    public bool ScsiReadBuffer(out byte[] buffer,         out ReadOnlySpan<byte> senseBuffer, uint bufferOffset,
                               uint       transferLength, uint timeout, out double duration, byte mode, byte bufferId)
    {
        senseBuffer = SenseBuffer;
        Span<byte> cdb = CdbBuffer[..10];
        cdb.Clear();

        buffer = new byte[transferLength];

        cdb[0] = (byte)ScsiCommands.ReadBuffer;
        cdb[1] = mode;
        cdb[2] = bufferId;
        cdb[3] = (byte)((bufferOffset & 0xFF0000) >> 16);
        cdb[4] = (byte)((bufferOffset & 0xFF00)   >> 8);
        cdb[5] = (byte)(bufferOffset & 0xFF);
        cdb[6] = (byte)((buffer.Length & 0xFF0000) >> 16);
        cdb[7] = (byte)((buffer.Length & 0xFF00)   >> 8);
        cdb[8] = (byte)(buffer.Length & 0xFF);

        LastError = SendScsiCommand(cdb, ref buffer, timeout, ScsiDirection.In, out duration, out bool sense);

        Error = LastError != 0;

        return sense;
    }

    /// <summary>
    ///     Makes sure the data's sector number is the one expected.
    /// </summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="firstLba">First consecutive LBA of the buffer</param>
    /// <param name="transferLength">How many blocks in buffer</param>
    /// <param name="layerbreak">The address in which the layerbreak occur</param>
    /// <param name="otp">Set to <c>true</c> if disk is Opposite Track Path (OTP)</param>
    /// <returns><c>false</c> if any sector is not matching expected value, else <c>true</c></returns>
    static bool CheckSectorNumber(IReadOnlyList<byte> buffer, uint firstLba, uint transferLength, uint layerbreak,
                                  bool                otp)
    {
        for(var i = 0; i < transferLength; i++)
        {
            var    layer        = (byte)(buffer[0 + 2064 * i] & 0x1);
            byte[] sectorBuffer = [0x0, buffer[1 + 2064 * i], buffer[2 + 2064 * i], buffer[3 + 2064 * i]];

            uint sectorNumber = BigEndianBitConverter.ToUInt32(sectorBuffer, 0);

            if(otp)
            {
                if(!IsCorrectDlOtpPsn(sectorNumber, (ulong)(firstLba + i), layer, layerbreak)) return false;
            }
            else
            {
                if(!IsCorrectDlPtpPsn(sectorNumber, (ulong)(firstLba + i), layer, layerbreak)) return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Checks if the PSN for a raw sector matches the expected LBA for a single layer DVD
    /// </summary>
    /// <param name="sectorNumber">The Sector Number from Identification Data (ID)</param>
    /// <param name="lba">The expected LBA</param>
    /// <returns><c>false</c> if the sector is not matching expected value, else <c>true</c></returns>
    static bool IsCorrectSlPsn(uint sectorNumber, ulong lba) => sectorNumber == lba + 0x30000;

    /// <summary>
    ///     Checks if the PSN for a raw sector matches the expected LBA for a dual layer DVD with parallel track path
    /// </summary>
    /// <param name="sectorNumber">The Sector Number from Identification Data (ID)</param>
    /// <param name="lba">The expected LBA</param>
    /// <param name="layer">Layer number</param>
    /// <param name="layerbreak">The address in which the layerbreak occur</param>
    /// <returns><c>false</c> if the sector is not matching expected value, else <c>true</c></returns>
    static bool IsCorrectDlPtpPsn(uint sectorNumber, ulong lba, byte layer, uint layerbreak)
    {
        if(layer == 0) return IsCorrectSlPsn(sectorNumber, lba);

        return sectorNumber == lba + 0x30000 - (layerbreak + 1 - 0x30000);
    }

    /// <summary>
    ///     Checks if the PSN for a raw sector matches the expected LBA for a dual layer DVD with opposite track path
    /// </summary>
    /// <param name="sectorNumber">The Sector Number from Identification Data (ID)</param>
    /// <param name="lba">The expected LBA</param>
    /// <param name="layer">Layer number</param>
    /// <param name="layerbreak">The address in which the layerbreak occur</param>
    /// <returns><c>false</c> if the sector is not matching expected value, else <c>true</c></returns>
    static bool IsCorrectDlOtpPsn(uint sectorNumber, ulong lba, byte layer, uint layerbreak)
    {
        if(layer != 1) return IsCorrectSlPsn(sectorNumber, lba);

        ulong n = ~(layerbreak + 1 + (layerbreak - (lba + 0x30000))) & 0x00ffffff;

        return sectorNumber == n;
    }

    /// <summary>
    ///     Deinterleave full ECC block with interleaved PI (e.g., 2384 bytes)
    /// </summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="transferLength">How many blocks in buffer</param>
    /// <param name="stride">Bytes per sector in buffer</param>
    /// <returns>The deinterleaved sectors</returns>
    static byte[] DeinterleaveFullEccInterleaved(byte[] buffer, uint transferLength, uint stride)
    {
        // TODO: Save ECC instead of just throwing it away

        var deinterleaved = new byte[2064 * transferLength];

        for(var j = 0; j < transferLength; j++)
        {
            for(var i = 0; i < 12; i++)
                Array.Copy(buffer, j * stride + i * 182, deinterleaved, j * 2064 + i * 172, 172);
        }

        return deinterleaved;
    }

    /// <summary>
    ///     Extract sector data from PO-only format (e.g., 2236 bytes)
    /// </summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="transferLength">How many blocks in buffer</param>
    /// <param name="stride">Bytes per sector in buffer</param>
    /// <returns>The extracted sector data</returns>
    static byte[] DeinterleavePoOnly(byte[] buffer, uint transferLength, uint stride)
    {
        var deinterleaved = new byte[2064 * transferLength];

        for(var j = 0; j < transferLength; j++) Array.Copy(buffer, j * stride, deinterleaved, j * 2064, 2064);

        return deinterleaved;
    }

    /// <summary>
    ///     Deinterleave full ECC block with padding (e.g., 2816 bytes)
    /// </summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="transferLength">How many blocks in buffer</param>
    /// <param name="stride">Bytes per sector in buffer</param>
    /// <returns>The deinterleaved sectors</returns>
    static byte[] DeinterleaveFullEccWithPadding(byte[] buffer, uint transferLength, uint stride) =>

        // Same as FullEccInterleaved, padding is ignored
        DeinterleaveFullEccInterleaved(buffer, transferLength, stride);

    /// <summary>
    ///     Detects which ReadBuffer 3C command variant works for this drive
    ///     Tries variants in order: 3c0000, 3c0100, 3c0101, 3c0102, 3c0200
    /// </summary>
    /// <param name="lba">LBA to use for filling the buffer</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="duration">Duration in milliseconds</param>
    /// <returns>Tuple with (mode, bufferId) if a working variant is found, null otherwise</returns>
    private (byte mode, byte bufferId)? DetectReadBufferVariant(uint lba, uint timeout, out double duration)
    {
        duration = 0;

        // Variants to try in order: 3c0000, 3c0100, 3c0101, 3c0102, 3c0200
        (byte mode, byte bufferId)[] variants =
        [
            (0x00, 0x00), // 3c0000
            (0x01, 0x00), // 3c0100
            (0x01, 0x01), // 3c0101
            (0x01, 0x02), // 3c0102
            (0x02, 0x00)  // 3c0200
        ];

        // Fill buffer with sectors 0-16
        Read12(out _, out _, 0, false, false, false, false, lba, 2048, 0, 16, false, timeout, out double readDuration);
        duration += readDuration;

        foreach((byte mode, byte bufferId) variant in variants)
        {
            // Try to read buffer with this variant
            // Read enough for at least 3 sectors (minimum needed for stride detection)
            const uint readSize = 3 * 3000; // Enough for 3 sectors even with large stride

            bool sense = ScsiReadBuffer(out byte[] buffer,
                                        out _,
                                        0,
                                        readSize,
                                        timeout,
                                        out double readBufferDuration,
                                        variant.mode,
                                        variant.bufferId);

            duration += readBufferDuration;

            // Check if command succeeded, returned valid data, and has correct sector header (00 03 00)
            if(sense || buffer is not { Length: >= 2236 * 3 }) continue;

            // Validate that the data starts with the expected DVD sector header pattern
            if(buffer.Length >= 4 && buffer[1] == 0x03 && buffer[2] == 0x00 && buffer[3] == 0x00)
            {
                AaruLogging.Debug(SCSI_MODULE_NAME,
                                  "ReadBuffer 3C variant {0:x2}{1:x2} detected",
                                  variant.mode,
                                  variant.bufferId);

                return (variant.mode, variant.bufferId);
            }

            AaruLogging.Debug(SCSI_MODULE_NAME,
                              "ReadBuffer 3C variant {0:x2}{1:x2} returned data but header pattern incorrect",
                              variant.mode,
                              variant.bufferId);
        }

        AaruLogging.Debug(SCSI_MODULE_NAME, "No working ReadBuffer 3C variant found");

        return null;
    }

    /// <summary>
    ///     Detects the stride (bytes per sector) in the buffer by searching for
    ///     the 00 03 00 pattern that appears at the start of each sector
    /// </summary>
    /// <param name="lba">LBA to use for filling the buffer (sectors 0-16)</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="mode">Buffer mode to use</param>
    /// <param name="bufferId">Buffer ID to use</param>
    /// <param name="duration">Duration in milliseconds</param>
    /// <returns>Detected stride in bytes, or 0 if detection failed</returns>
    private uint DetectBufferStride(uint lba, uint timeout, byte mode, byte bufferId, out double duration)
    {
        // Fill buffer with sectors 0-16
        Read12(out _, out _, 0, false, false, false, false, lba, 2048, 0, 16, false, timeout, out duration);

        // Read a large buffer chunk (enough for 16+ sectors)
        const uint readSize = 16 * 3000; // Enough for 16 sectors even with large stride

        bool sense = ScsiReadBuffer(out byte[] buffer,
                                    out _,
                                    0,
                                    readSize,
                                    timeout,
                                    out double readDuration,
                                    mode,
                                    bufferId);

        duration += readDuration;

        if(sense || buffer == null || buffer.Length < 2236 * 3) // Need at least 3 sectors worth
        {
            AaruLogging.Debug(SCSI_MODULE_NAME,
                              "ReadBuffer stride detection failed, sense or buffer too small, using default");

            return 0; // Detection failed
        }

        // Search for pattern 03 00 00 starting from beginning + 1 byte
        // Find first occurrence
        int firstOffset = -1;

        for(var i = 0; i < buffer.Length - 3; i++)
        {
            if(buffer[i + 1] != 0x03 || buffer[i + 2] != 0x00 || buffer[i + 3] != 0x00) continue;

            firstOffset = i;

            break;
        }

        if(firstOffset != 0)
        {
            AaruLogging.Debug(SCSI_MODULE_NAME,
                              "ReadBuffer stride detection failed, pattern not at start, using default");

            return 0; // Pattern not at start
        }

        // Find second occurrence to calculate stride
        int secondOffset = -1;

        // firstOffset is always 0 here, the pattern was required to be at the start
        for(var i = 2064; i < Math.Min(2500, buffer.Length - 3); i++)
        {
            if(buffer[i + 1] != 0x03 || buffer[i + 2] != 0x00 || buffer[i + 3] != 0x01) continue;

            secondOffset = i;

            break;
        }

        if(secondOffset == -1)
        {
            AaruLogging.Debug(SCSI_MODULE_NAME,
                              "ReadBuffer stride detection failed, couldn't find second sector, using default");

            return 0; // Couldn't find second sector
        }

        uint stride = (uint)(secondOffset - firstOffset);

        // Verify stride by checking 3rd and 4th sectors
        for(int sectorNum = 2; sectorNum <= 3; sectorNum++)
        {
            int expectedOffset = (int)(firstOffset + stride * sectorNum);

            if(expectedOffset + 3 >= buffer.Length) break;

            if(buffer[expectedOffset + 1] != 0x03 ||
               buffer[expectedOffset + 2] != 0x00 ||
               buffer[expectedOffset + 3] != 0x02 && buffer[expectedOffset + 3] != 0x03)
                return 0; // Verification failed
        }

        AaruLogging.Debug(SCSI_MODULE_NAME, "ReadBuffer stride detection succeeded, stride: {0}", stride);

        return stride;
    }

    /// <summary>
    ///     Detects ReadBuffer 3C support: finds working variant and stride
    /// </summary>
    /// <param name="lba">LBA to use for detection</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="duration">Duration in milliseconds</param>
    /// <returns><c>true</c> if detection succeeded, <c>false</c> otherwise</returns>
    private bool DetectReadBuffer3C(uint lba, uint timeout, out double duration)
    {
        duration = 0;

        // If already detected, return success
        if(_readBuffer3CDetected) return _detectedBufferStride != 0;

        // Try to detect variant
        (byte mode, byte bufferId)? variant = DetectReadBufferVariant(lba, timeout, out double variantDuration);
        duration += variantDuration;

        if(!variant.HasValue)
        {
            _readBuffer3CDetected = true; // Mark as attempted even if failed

            return false;
        }

        _detectedReadBufferMode = variant.Value.mode;
        _detectedReadBufferId   = variant.Value.bufferId;

        // Detect stride using the found variant
        uint stride = DetectBufferStride(lba,
                                         timeout,
                                         _detectedReadBufferMode,
                                         _detectedReadBufferId,
                                         out double strideDuration);

        duration += strideDuration;

        if(stride is 0 or < 2064 or > 10000)
        {
            AaruLogging.Debug(SCSI_MODULE_NAME,
                              "ReadBuffer 3C stride detection failed or invalid stride: {0}, using default",
                              stride);

            _detectedBufferStride = 2384; // Default to known value
            _readBuffer3CDetected = true;

            return false; // Detection partially succeeded but stride failed
        }

        _detectedBufferStride = stride;
        _readBuffer3CDetected = true;

        AaruLogging.Debug(SCSI_MODULE_NAME,
                          "ReadBuffer 3C detection succeeded, variant: {0:x2}{1:x2}, stride: {2}",
                          _detectedReadBufferMode,
                          _detectedReadBufferId,
                          _detectedBufferStride);

        return true;
    }

    /// <summary>Reads a "raw" sector from DVD using ReadBuffer 3C command.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the ReadBuffer (RAW) response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="lba">Start block address.</param>
    /// <param name="transferLength">How many blocks to read.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="layerbreak">The address in which the layerbreak occur</param>
    /// <param name="otp">Set to <c>true</c> if disk is Opposite Track Path (OTP)</param>
    public bool ReadBuffer3CRawDvd(out byte[] buffer, out ReadOnlySpan<byte> senseBuffer, uint lba, uint transferLength,
                                   uint       timeout, out double duration, uint layerbreak, bool otp)
    {
        // Detect ReadBuffer 3C variant and stride on first call
        if(!_readBuffer3CDetected)
        {
            bool detected = DetectReadBuffer3C(lba, timeout, out double detectDuration);

            if(!detected || _detectedBufferStride == 0 || _detectedBufferStride < 2064 || _detectedBufferStride > 10000)
            {
                // Detection failed - raw reading is not supported on this drive
                AaruLogging.Debug(SCSI_MODULE_NAME,
                                  "ReadBuffer 3C detection failed - raw reading is not supported on this drive");

                buffer                = Array.Empty<byte>();
                senseBuffer           = SenseBuffer;
                duration              = detectDuration;
                Error                 = true;
                _readBuffer3CDetected = true;

                return true; // Return failure - raw reading not supported
            }

            // Detect format based on stride
            _bufferFormat = _detectedBufferStride switch
                            {
                                2064   => BufferFormat.SectorDataOnly,
                                2236   => BufferFormat.PoOnly,
                                2384   => BufferFormat.FullEccInterleaved,
                                > 2384 => BufferFormat.FullEccWithPadding,
                                _      => BufferFormat.FullEccInterleaved // Default for backward compatibility
                            };

            AaruLogging.Debug(SCSI_MODULE_NAME,
                              "ReadBuffer 3C buffer format detected based on stride: {0}, format: {1}",
                              _detectedBufferStride,
                              _bufferFormat);

            // Initialize buffer capacity with default value (will be refined dynamically when offset is lost)
            _bufferCapacityInSectors = 714;
            _totalSectorsRead        = 0; // Initialize tracking for dynamic capacity detection
            _readBuffer3CDetected    = true;
        }

        _bufferOffset %= _bufferCapacityInSectors;

        bool sense;

        if(layerbreak > 0 && transferLength > 1 && lba + 0x30000 + 256 > layerbreak && lba + 0x30000 < layerbreak + 256)
        {
            buffer      = new byte[transferLength * 2064];
            duration    = 0;
            senseBuffer = SenseBuffer;

            return true;
        }

        if(_bufferCapacityInSectors - _bufferOffset < transferLength)
        {
            sense = ReadSectorsAcrossBufferBorder(out buffer,
                                                  out senseBuffer,
                                                  lba,
                                                  transferLength,
                                                  timeout,
                                                  out duration,
                                                  layerbreak,
                                                  otp);
        }
        else
        {
            sense = ReadSectorsFromBuffer(out buffer,
                                          out senseBuffer,
                                          lba,
                                          transferLength,
                                          timeout,
                                          out duration,
                                          layerbreak,
                                          otp);
        }

        Error = LastError != 0;

        AaruLogging.Debug(SCSI_MODULE_NAME, "ReadBuffer 3C READ DVD RAW took {0} ms", duration);

        return sense;
    }

    /// <summary>
    ///     Reads the device's memory buffer and returns raw sector data
    /// </summary>
    /// <param name="buffer">Buffer where the ReadBuffer (RAW) response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="bufferOffset">The offset to read the buffer at</param>
    /// <param name="transferLength">How many blocks to read.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="lba">Start block address.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    private bool ReadBuffer3CInternal(out byte[] buffer,         out ReadOnlySpan<byte> senseBuffer, uint bufferOffset,
                                      uint       transferLength, uint timeout, out double duration, uint lba)
    {
        // We need to fill the buffer before reading it with the ReadBuffer command. We don't care about sense,
        // because the data can be wrong anyway, so we check the buffer data later instead.
        Read12(out _, out _, 0, false, false, false, false, lba, 2048, 0, 16, false, timeout, out duration);

        // Use generic ReadBuffer method with detected variant
        return ScsiReadBuffer(out buffer,
                              out senseBuffer,
                              bufferOffset,
                              transferLength,
                              timeout,
                              out duration,
                              _detectedReadBufferMode,
                              _detectedReadBufferId);
    }

    /// <summary>
    ///     Reads raw sectors from the device's memory
    /// </summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the ReadBuffer (RAW) response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="lba">Start block address.</param>
    /// <param name="transferLength">How many blocks to read.</param>
    /// <param name="layerbreak">The address in which the layerbreak occur</param>
    /// <param name="otp">Set to <c>true</c> if disk is Opposite Track Path (OTP)</param>
    private bool ReadSectorsFromBuffer(out byte[] buffer,         out ReadOnlySpan<byte> senseBuffer, uint lba,
                                       uint       transferLength, uint timeout, out double duration, uint layerbreak,
                                       bool       otp)
    {
        bool sense = ReadBuffer3CInternal(out buffer,
                                          out senseBuffer,
                                          _bufferOffset  * _detectedBufferStride,
                                          transferLength * _detectedBufferStride,
                                          timeout,
                                          out duration,
                                          lba);

        byte[] deinterleaved = DeinterleaveEccBlock(buffer, transferLength, _detectedBufferStride, _bufferFormat);

        if(!CheckSectorNumber(deinterleaved, lba, transferLength, layerbreak, otp))
        {
            // Buffer offset lost - this means we've wrapped around
            // Use the number of sectors read to detect buffer capacity
            if(_totalSectorsRead is > 0 and >= 16 and <= 2000)
            {
                uint detectedCapacity = _totalSectorsRead;
                uint oldCapacity      = _bufferCapacityInSectors;

                // If we already have a capacity, verify new detection is consistent
                if(_bufferCapacityInSectors == 714 || // Update if using default
                   (detectedCapacity >= _bufferCapacityInSectors * 9  / 10 &&
                    detectedCapacity <= _bufferCapacityInSectors * 11 / 10)) // Or within 10%
                {
                    _bufferCapacityInSectors = detectedCapacity;

                    AaruLogging.Debug(SCSI_MODULE_NAME,
                                      "Buffer capacity dynamically detected: {0} sectors (was {1})",
                                      detectedCapacity,
                                      oldCapacity);
                }
            }

            // Reset tracking for next cycle
            _totalSectorsRead = 0;

            // Buffer offset lost, try to find it again
            int offset = FindBufferOffset(lba, timeout, layerbreak, otp);

            if(offset == -1) return true;

            _bufferOffset = (uint)offset;

            sense = ReadBuffer3CInternal(out buffer,
                                         out senseBuffer,
                                         _bufferOffset  * _detectedBufferStride,
                                         transferLength * _detectedBufferStride,
                                         timeout,
                                         out duration,
                                         lba);

            deinterleaved = DeinterleaveEccBlock(buffer, transferLength, _detectedBufferStride, _bufferFormat);

            if(!CheckSectorNumber(deinterleaved, lba, transferLength, layerbreak, otp)) return true;
        }

        if(_decoding.Scramble(deinterleaved, transferLength, out byte[] scrambledBuffer) != ErrorNumber.NoError)
            return true;

        buffer = scrambledBuffer;

        _bufferOffset     += transferLength;
        _totalSectorsRead += transferLength; // Track successful read for capacity detection

        return sense;
    }

    /// <summary>
    ///     Reads raw sectors when they cross the device's memory border
    /// </summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the ReadBuffer (RAW) response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="lba">Start block address.</param>
    /// <param name="transferLength">How many blocks to read.</param>
    /// <param name="layerbreak">The address in which the layerbreak occur</param>
    /// <param name="otp">Set to <c>true</c> if disk is Opposite Track Path (OTP)</param>
    private bool ReadSectorsAcrossBufferBorder(out byte[] buffer, out ReadOnlySpan<byte> senseBuffer, uint lba,
                                               uint transferLength, uint timeout, out double duration, uint layerbreak,
                                               bool otp)
    {
        uint newTransferLength1 = _bufferCapacityInSectors - _bufferOffset;
        uint newTransferLength2 = transferLength           - newTransferLength1;

        bool sense1 = ReadBuffer3CInternal(out byte[] buffer1,
                                           out _,
                                           _bufferOffset      * _detectedBufferStride,
                                           newTransferLength1 * _detectedBufferStride,
                                           timeout,
                                           out double duration1,
                                           lba);

        bool sense2 = ReadBuffer3CInternal(out byte[] buffer2,
                                           out _,
                                           0,
                                           newTransferLength2 * _detectedBufferStride,
                                           timeout,
                                           out double duration2,
                                           lba);

        senseBuffer = SenseBuffer; // TODO

        buffer = new byte[_detectedBufferStride * transferLength];
        Array.Copy(buffer1, buffer, buffer1.Length);
        Array.Copy(buffer2, 0,      buffer, buffer1.Length, buffer2.Length);

        duration = duration1 + duration2;

        byte[] deinterleaved = DeinterleaveEccBlock(buffer, transferLength, _detectedBufferStride, _bufferFormat);

        if(!CheckSectorNumber(deinterleaved, lba, transferLength, layerbreak, otp)) return true;

        if(_decoding.Scramble(deinterleaved, transferLength, out byte[] scrambledBuffer) != ErrorNumber.NoError)
            return true;

        buffer = scrambledBuffer;

        _bufferOffset     =  newTransferLength2;
        _totalSectorsRead += transferLength; // Track successful read for capacity detection

        return sense1 || sense2;
    }

    /// <summary>
    ///     Sometimes the offset on the drive memory can get lost. This tries to find it again.
    /// </summary>
    /// <param name="lba">The expected LBA</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="layerbreak">The address in which the layerbreak occur</param>
    /// <param name="otp">Set to <c>true</c> if disk is Opposite Track Path (OTP)</param>
    /// <returns>The offset on the device memory, or -1 if not found</returns>
    private int FindBufferOffset(uint lba, uint timeout, uint layerbreak, bool otp)
    {
        for(uint i = 0; i < _bufferCapacityInSectors; i++)
        {
            ReadBuffer3CInternal(out byte[] buffer,
                                 out _,
                                 i * _detectedBufferStride,
                                 _detectedBufferStride,
                                 timeout,
                                 out double _,
                                 lba);

            byte[] deinterleaved = DeinterleaveEccBlock(buffer, 1, _detectedBufferStride, _bufferFormat);

            if(CheckSectorNumber(deinterleaved, lba, 1, layerbreak, otp)) return (int)i;
        }

        return -1;
    }

    /// <summary>
    ///     Deinterleave the ECC block based on detected format
    /// </summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="transferLength">How many blocks in buffer</param>
    /// <param name="stride">Bytes per sector in buffer</param>
    /// <param name="format">Buffer format type</param>
    /// <returns>The deinterleaved sectors</returns>
    private static byte[] DeinterleaveEccBlock(byte[] buffer, uint transferLength, uint stride, BufferFormat format)
    {
        return format switch
               {
                   BufferFormat.FullEccInterleaved => DeinterleaveFullEccInterleaved(buffer, transferLength, stride),
                   BufferFormat.PoOnly => DeinterleavePoOnly(buffer, transferLength, stride),
                   BufferFormat.SectorDataOnly => buffer, // No deinterleaving needed for sector-data-only format
                   BufferFormat.FullEccWithPadding => DeinterleaveFullEccWithPadding(buffer, transferLength, stride),
                   _ => DeinterleaveFullEccInterleaved(buffer, transferLength, stride) // Default fallback
               };
    }
}