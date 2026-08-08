// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Subchannel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles CompactDisc subchannel data.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

using System;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Decoders.CD;
using Aaru.Devices;
using Aaru.Logging;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>Check if the drive can read RW raw subchannel</summary>
    /// <param name="dev">Device</param>
    /// <param name="dumpLog">Dumping log</param>
    /// <param name="updateStatus">Progress update callback</param>
    /// <param name="lba">LBA to try</param>
    /// <returns><c>true</c> if read correctly, <c>false</c> otherwise</returns>
    public static bool SupportsRwSubchannel(Device dev, UpdateStatusHandler updateStatus, uint lba)
    {
        updateStatus?.Invoke(Localization.Core.Checking_if_drive_supports_full_raw_subchannel_reading);

        return !dev.ReadCd(out _,
                           out _,
                           lba,
                           2352 + 96,
                           1,
                           MmcSectorTypes.AllTypes,
                           false,
                           false,
                           true,
                           MmcHeaderCodes.AllHeaders,
                           true,
                           true,
                           MmcErrorField.None,
                           MmcSubchannel.Raw,
                           dev.Timeout,
                           out _);
    }

    /// <summary>Check if the drive can read RW raw subchannel</summary>
    /// <param name="dev">Device</param>
    /// <param name="dumpLog">Dumping log</param>
    /// <param name="updateStatus">Progress update callback</param>
    /// <param name="lba">LBA to try</param>
    /// <returns><c>true</c> if read correctly, <c>false</c> otherwise</returns>
    public static bool SupportsPqSubchannel(Device dev, UpdateStatusHandler updateStatus, uint lba)
    {
        updateStatus?.Invoke(Localization.Core.Checking_if_drive_supports_PQ_subchannel_reading);

        return !dev.ReadCd(out _,
                           out _,
                           lba,
                           2352 + 16,
                           1,
                           MmcSectorTypes.AllTypes,
                           false,
                           false,
                           true,
                           MmcHeaderCodes.AllHeaders,
                           true,
                           true,
                           MmcErrorField.None,
                           MmcSubchannel.Q16,
                           dev.Timeout,
                           out _);
    }

    bool CheckSubchannelSupport(uint firstLba, IWritableOpticalImage outputOptical, out uint subSize,
                                out TrackSubchannelType subType, out bool supportsPqSubchannel,
                                out bool supportsRwSubchannel, out MmcSubchannel desiredSubchannel,
                                out MmcSubchannel supportedSubchannel, out bool read6, out bool read10, out bool read12,
                                out bool read16, out bool readcd, out bool bcdSubchannel)
    {
        uint   blockSize;                          // Size of the read sector in bytes
        byte[] cmdBuf;                             // Data buffer
        desiredSubchannel    = MmcSubchannel.None; // User requested subchannel
        supportsPqSubchannel = false;              // Supports reading PQ subchannel
        supportsRwSubchannel = false;              // Supports reading RW subchannel
        supportedSubchannel  = MmcSubchannel.None; // Drive's maximum supported subchannel
        read6                = false;              // Device supports READ(6)
        read10               = false;              // Device supports READ(10)
        read12               = false;              // Device supports READ(12)
        read16               = false;              // Device supports READ(16)
        readcd               = true;               // Device supports READ CD
        const uint sectorSize = 2352;              // Full sector size
        subSize = 0;                               // Subchannel size in bytes
        subType = TrackSubchannelType.None;        // Track subchannel type
        var    sense = true;                       // Sense indicator
        byte[] tmpBuf;                             // Temporary buffer
        bcdSubchannel = false;                     // Subchannel positioning is in BCD

        // Check subchannels support
        supportsPqSubchannel = SupportsPqSubchannel(_dev, UpdateStatus, firstLba) ||
                               SupportsPqSubchannel(_dev, UpdateStatus, firstLba + 5);

        supportsRwSubchannel = SupportsRwSubchannel(_dev, UpdateStatus, firstLba) ||
                               SupportsRwSubchannel(_dev, UpdateStatus, firstLba + 5);

        if(supportsRwSubchannel)
            supportedSubchannel = MmcSubchannel.Raw;
        else if(supportsPqSubchannel)
            supportedSubchannel = MmcSubchannel.Q16;
        else
            supportedSubchannel = MmcSubchannel.None;

        switch(_subchannel)
        {
            case DumpSubchannel.Any:
                if(supportsRwSubchannel)
                    desiredSubchannel = MmcSubchannel.Raw;
                else if(supportsPqSubchannel)
                    desiredSubchannel = MmcSubchannel.Q16;
                else
                    desiredSubchannel = MmcSubchannel.None;

                break;
            case DumpSubchannel.Rw:
                if(supportsRwSubchannel)
                    desiredSubchannel = MmcSubchannel.Raw;
                else
                {
                    AaruLogging.WriteLine(Localization.Core
                                                      .Drive_does_not_support_the_requested_subchannel_format_not_continuing);

                    StoppingErrorMessage?.Invoke(Localization.Core
                                                             .Drive_does_not_support_the_requested_subchannel_format_not_continuing);

                    return true;
                }

                break;
            case DumpSubchannel.RwOrPq:
                if(supportsRwSubchannel)
                    desiredSubchannel = MmcSubchannel.Raw;
                else if(supportsPqSubchannel)
                    desiredSubchannel = MmcSubchannel.Q16;
                else
                {
                    AaruLogging.WriteLine(Localization.Core
                                                      .Drive_does_not_support_the_requested_subchannel_format_not_continuing);

                    StoppingErrorMessage?.Invoke(Localization.Core
                                                             .Drive_does_not_support_the_requested_subchannel_format_not_continuing);

                    return true;
                }

                break;
            case DumpSubchannel.Pq:
                if(supportsPqSubchannel)
                    desiredSubchannel = MmcSubchannel.Q16;
                else
                {
                    AaruLogging.WriteLine(Localization.Core
                                                      .Drive_does_not_support_the_requested_subchannel_format_not_continuing);

                    StoppingErrorMessage?.Invoke(Localization.Core
                                                             .Drive_does_not_support_the_requested_subchannel_format_not_continuing);

                    return true;
                }

                break;
            case DumpSubchannel.None:
                desiredSubchannel = MmcSubchannel.None;

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if(desiredSubchannel == MmcSubchannel.Q16 && supportsPqSubchannel) supportedSubchannel = MmcSubchannel.Q16;

        // Check if output format supports subchannels
        if(!outputOptical.SupportedSectorTags.Contains(SectorTagType.CdSectorSubchannel) &&
           desiredSubchannel != MmcSubchannel.None)
        {
            if(_force || _subchannel == DumpSubchannel.None)
            {
                AaruLogging.WriteLine(Localization.Core.Output_format_does_not_support_subchannels_continuing);
                UpdateStatus?.Invoke(Localization.Core.Output_format_does_not_support_subchannels_continuing);
            }
            else
            {
                AaruLogging.WriteLine(Localization.Core.Output_format_does_not_support_subchannels_not_continuing);

                StoppingErrorMessage?.Invoke(Localization.Core
                                                         .Output_format_does_not_support_subchannels_not_continuing);

                return true;
            }

            desiredSubchannel = MmcSubchannel.None;
        }

        switch(supportedSubchannel)
        {
            case MmcSubchannel.None:
                UpdateStatus?.Invoke(Localization.Core.Checking_if_drive_supports_reading_without_subchannel);

                readcd = !_dev.ReadCd(out cmdBuf,
                                      out _,
                                      firstLba,
                                      sectorSize,
                                      1,
                                      MmcSectorTypes.AllTypes,
                                      false,
                                      false,
                                      true,
                                      MmcHeaderCodes.AllHeaders,
                                      true,
                                      true,
                                      MmcErrorField.None,
                                      supportedSubchannel,
                                      _dev.Timeout,
                                      out _) ||
                         !_dev.ReadCd(out cmdBuf,
                                      out _,
                                      firstLba + 5,
                                      sectorSize,
                                      1,
                                      MmcSectorTypes.AllTypes,
                                      false,
                                      false,
                                      true,
                                      MmcHeaderCodes.AllHeaders,
                                      true,
                                      true,
                                      MmcErrorField.None,
                                      supportedSubchannel,
                                      _dev.Timeout,
                                      out _);

                if(!readcd)
                {
                    AaruLogging.WriteLine(Localization.Core.Drive_does_not_support_READ_CD_trying_SCSI_READ_commands);
                    ErrorMessage?.Invoke(Localization.Core.Drive_does_not_support_READ_CD_trying_SCSI_READ_commands);

                    UpdateStatus?.Invoke(Localization.Core.Checking_if_drive_supports_READ_6);

                    read6 = !_dev.Read6(out cmdBuf, out _, firstLba,     2048, 1, _dev.Timeout, out _) ||
                            !_dev.Read6(out cmdBuf, out _, firstLba + 5, 2048, 1, _dev.Timeout, out _);

                    UpdateStatus?.Invoke(Localization.Core.Checking_if_drive_supports_READ_10);

                    read10 =
                        !_dev.Read10(out cmdBuf,
                                     out _,
                                     0,
                                     false,
                                     true,
                                     false,
                                     false,
                                     firstLba,
                                     2048,
                                     0,
                                     1,
                                     _dev.Timeout,
                                     out _) ||
                        !_dev.Read10(out cmdBuf,
                                     out _,
                                     0,
                                     false,
                                     true,
                                     false,
                                     false,
                                     firstLba + 5,
                                     2048,
                                     0,
                                     1,
                                     _dev.Timeout,
                                     out _);

                    UpdateStatus?.Invoke(Localization.Core.Checking_if_drive_supports_READ_12);

                    read12 =
                        !_dev.Read12(out cmdBuf,
                                     out _,
                                     0,
                                     false,
                                     true,
                                     false,
                                     false,
                                     firstLba,
                                     2048,
                                     0,
                                     1,
                                     false,
                                     _dev.Timeout,
                                     out _) ||
                        !_dev.Read12(out cmdBuf,
                                     out _,
                                     0,
                                     false,
                                     true,
                                     false,
                                     false,
                                     firstLba + 5,
                                     2048,
                                     0,
                                     1,
                                     false,
                                     _dev.Timeout,
                                     out _);

                    UpdateStatus?.Invoke(Localization.Core.Checking_if_drive_supports_READ_16);

                    read16 =
                        !_dev.Read16(out cmdBuf,
                                     out _,
                                     0,
                                     false,
                                     true,
                                     false,
                                     firstLba,
                                     2048,
                                     0,
                                     1,
                                     false,
                                     _dev.Timeout,
                                     out _) ||
                        !_dev.Read16(out cmdBuf,
                                     out _,
                                     0,
                                     false,
                                     true,
                                     false,
                                     firstLba + 5,
                                     2048,
                                     0,
                                     1,
                                     false,
                                     _dev.Timeout,
                                     out _);

                    switch(read6)
                    {
                        case false when !read10 && !read12 && !read16:
                            AaruLogging.WriteLine(Localization.Core.Cannot_read_from_disc_not_continuing);
                            StoppingErrorMessage?.Invoke(Localization.Core.Cannot_read_from_disc_not_continuing);

                            return true;
                        case true:
                            UpdateStatus?.Invoke(Localization.Core.Drive_supports_READ_6);

                            break;
                    }

                    if(read10) UpdateStatus?.Invoke(Localization.Core.Drive_supports_READ_10);

                    if(read12) UpdateStatus?.Invoke(Localization.Core.Drive_supports_READ_12);

                    if(read16) UpdateStatus?.Invoke(Localization.Core.Drive_supports_READ_16);
                }

                UpdateStatus?.Invoke(Localization.Core.Drive_can_read_without_subchannel);

                subSize = 0;

                break;
            case MmcSubchannel.Raw:
                UpdateStatus?.Invoke(Localization.Core.Full_raw_subchannel_reading_supported);
                subSize = 96;

                break;
            case MmcSubchannel.Q16:
                UpdateStatus?.Invoke(Localization.Core.PQ_subchannel_reading_supported);

                UpdateStatus?.Invoke(Localization.Core.WARNING_If_disc_says_CDG_CDEG_CDMIDI_dump_will_be_incorrect);

                subSize = 16;

                break;
        }

        subType = desiredSubchannel switch
                  {
                      MmcSubchannel.None                     => TrackSubchannelType.None,
                      MmcSubchannel.Raw or MmcSubchannel.Q16 => TrackSubchannelType.Raw,
                      _                                      => throw new ArgumentOutOfRangeException()
                  };

        blockSize = sectorSize + subSize;

        // Check if subchannel is BCD
        if(supportedSubchannel == MmcSubchannel.None) return false;

        sense = _dev.ReadCd(out cmdBuf,
                            out _,
                            (firstLba / 75 + 1) * 75 + 35,
                            blockSize,
                            1,
                            MmcSectorTypes.AllTypes,
                            false,
                            false,
                            true,
                            MmcHeaderCodes.AllHeaders,
                            true,
                            true,
                            MmcErrorField.None,
                            supportedSubchannel,
                            _dev.Timeout,
                            out _);

        if(sense) return false;

        tmpBuf = new byte[subSize];
        Array.Copy(cmdBuf, sectorSize, tmpBuf, 0, subSize);

        if(supportedSubchannel == MmcSubchannel.Q16) tmpBuf = Subchannel.ConvertQToRaw(tmpBuf);

        tmpBuf = Subchannel.Deinterleave(tmpBuf);

        // 9th Q subchannel is always FRAME when in user data area
        // LBA 35 => MSF 00:02:35 => FRAME 35 (in hexadecimal 0x23)
        // Sometimes drive returns a pregap here but MSF 00:02:3x => FRAME 3x (hexadecimal 0x1E to 0x27)
        // In BCD the frame reads 0x30-0x39 (high nibble 3); in binary it reads 0x1E-0x27 (high nibble 1 or 2)
        bcdSubchannel = (tmpBuf[21] & 0xF0) == 0x30;

        UpdateStatus?.Invoke(bcdSubchannel
                                 ? Localization.Core.Drive_returns_subchannel_in_BCD
                                 : Localization.Core.Drive_does_not_returns_subchannel_in_BCD);

        return false;
    }
}