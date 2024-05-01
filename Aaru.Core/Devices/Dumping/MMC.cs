// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps media from SCSI MultiMedia devices.
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
// Copyright © 2011-2024 Natalia Portillo
// Copyright © 2020-2024 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.Bluray;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Decryption;
using Aaru.Decryption.DVD;
using Aaru.Devices;
using DDS = Aaru.Decoders.DVD.DDS;
using DMI = Aaru.Decoders.Xbox.DMI;
using DVDDecryption = Aaru.Decryption.DVD.Dump;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;
using Spare = Aaru.Decoders.DVD.Spare;

// ReSharper disable JoinDeclarationAndInitializer

namespace Aaru.Core.Devices.Dumping;

/// <summary>Implement dumping optical discs from MultiMedia devices</summary>
partial class Dump
{
    /// <summary>Dumps an optical disc</summary>
    void Mmc()
    {
        MediaType     dskType = MediaType.Unknown;
        bool          sense;
        byte[]        tmpBuf;
        var           compactDisc      = true;
        var           gotConfiguration = false;
        var           isXbox           = false;
        DVDDecryption dvdDecrypt       = null;
        _speedMultiplier = 1;

        // TODO: Log not only what is it reading, but if it was read correctly or not.
        sense = _dev.GetConfiguration(out byte[] cmdBuf, out _, 0, MmcGetConfigurationRt.Current, _dev.Timeout, out _);

        if(!sense)
        {
            gotConfiguration = true;
            Features.SeparatedFeatures ftr = Features.Separate(cmdBuf);
            _dumpLog.WriteLine(Localization.Core.Device_reports_current_profile_is_0, ftr.CurrentProfile);

            switch(ftr.CurrentProfile)
            {
                case 0x0001:
                    dskType          = MediaType.GENERIC_HDD;
                    _speedMultiplier = -1;
                    goto default;
                case 0x0002:
                    dskType          = MediaType.PD650;
                    _speedMultiplier = -1;
                    goto default;
                case 0x0005:
                    dskType = MediaType.CDMO;

                    break;
                case 0x0008:
                    dskType = MediaType.CD;

                    break;
                case 0x0009:
                    dskType = MediaType.CDR;

                    break;
                case 0x000A:
                    dskType = MediaType.CDRW;

                    break;
                case 0x0010:
                    dskType          = MediaType.DVDROM;
                    _speedMultiplier = 9;
                    goto default;
                case 0x0011:
                    dskType          = MediaType.DVDR;
                    _speedMultiplier = 9;
                    goto default;
                case 0x0012:
                    dskType          = MediaType.DVDRAM;
                    _speedMultiplier = 9;
                    goto default;
                case 0x0013:
                case 0x0014:
                    dskType          = MediaType.DVDRW;
                    _speedMultiplier = 9;
                    goto default;
                case 0x0015:
                case 0x0016:
                    dskType          = MediaType.DVDRDL;
                    _speedMultiplier = 9;
                    goto default;
                case 0x0017:
                    dskType          = MediaType.DVDRWDL;
                    _speedMultiplier = 9;
                    goto default;
                case 0x0018:
                    dskType          = MediaType.DVDDownload;
                    _speedMultiplier = 9;
                    goto default;
                case 0x001A:
                    dskType          = MediaType.DVDPRW;
                    _speedMultiplier = 9;
                    goto default;
                case 0x001B:
                    dskType          = MediaType.DVDPR;
                    _speedMultiplier = 9;
                    goto default;
                case 0x0020:
                    dskType = MediaType.DDCD;
                    goto default;
                case 0x0021:
                    dskType = MediaType.DDCDR;
                    goto default;
                case 0x0022:
                    dskType = MediaType.DDCDRW;
                    goto default;
                case 0x002A:
                    dskType          = MediaType.DVDPRWDL;
                    _speedMultiplier = 9;
                    goto default;
                case 0x002B:
                    dskType          = MediaType.DVDPRDL;
                    _speedMultiplier = 9;
                    goto default;
                case 0x0040:
                    dskType          = MediaType.BDROM;
                    _speedMultiplier = 30;
                    goto default;
                case 0x0041:
                case 0x0042:
                    dskType          = MediaType.BDR;
                    _speedMultiplier = 30;
                    goto default;
                case 0x0043:
                    dskType          = MediaType.BDRE;
                    _speedMultiplier = 30;
                    goto default;
                case 0x0050:
                    dskType          = MediaType.HDDVDROM;
                    _speedMultiplier = 30;
                    goto default;
                case 0x0051:
                    dskType          = MediaType.HDDVDR;
                    _speedMultiplier = 30;
                    goto default;
                case 0x0052:
                    dskType          = MediaType.HDDVDRAM;
                    _speedMultiplier = 30;
                    goto default;
                case 0x0053:
                    dskType          = MediaType.HDDVDRW;
                    _speedMultiplier = 30;
                    goto default;
                case 0x0058:
                    dskType          = MediaType.HDDVDRDL;
                    _speedMultiplier = 30;
                    goto default;
                case 0x005A:
                    dskType          = MediaType.HDDVDRWDL;
                    _speedMultiplier = 30;
                    goto default;
                default:
                    compactDisc = false;

                    break;
            }
        }

        Modes.DecodedMode? decMode = null;

        sense = _dev.ModeSense6(out cmdBuf, out _, true, ScsiModeSensePageControl.Current, 0x00, _dev.Timeout, out _);

        if(sense || _dev.Error)
        {
            sense = _dev.ModeSense6(out cmdBuf,
                                    out _,
                                    false,
                                    ScsiModeSensePageControl.Current,
                                    0x00,
                                    _dev.Timeout,
                                    out _);

            if(!sense && !_dev.Error) decMode = Modes.DecodeMode6(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);
        }
        else
            decMode = Modes.DecodeMode6(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);

        if(decMode is null)
        {
            sense = _dev.ModeSense10(out cmdBuf,
                                     out _,
                                     false,
                                     true,
                                     ScsiModeSensePageControl.Current,
                                     0x3F,
                                     0x00,
                                     _dev.Timeout,
                                     out _);

            if(sense || _dev.Error)
            {
                sense = _dev.ModeSense10(out cmdBuf,
                                         out _,
                                         false,
                                         false,
                                         ScsiModeSensePageControl.Current,
                                         0x3F,
                                         0x00,
                                         _dev.Timeout,
                                         out _);

                if(sense || _dev.Error)
                {
                    sense = _dev.ModeSense10(out cmdBuf,
                                             out _,
                                             false,
                                             true,
                                             ScsiModeSensePageControl.Current,
                                             0x00,
                                             0x00,
                                             _dev.Timeout,
                                             out _);

                    if(sense || _dev.Error)
                    {
                        sense = _dev.ModeSense10(out cmdBuf,
                                                 out _,
                                                 false,
                                                 false,
                                                 ScsiModeSensePageControl.Current,
                                                 0x00,
                                                 0x00,
                                                 _dev.Timeout,
                                                 out _);

                        if(!sense && !_dev.Error)
                            decMode = Modes.DecodeMode10(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);
                    }
                    else
                        decMode = Modes.DecodeMode10(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);
                }
                else
                    decMode = Modes.DecodeMode10(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);
            }
            else
                decMode = Modes.DecodeMode10(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);
        }

        if(decMode.HasValue  &&
           _dev.IsUsb        &&
           !gotConfiguration &&
           decMode.Value.Header.MediumType is MediumTypes.UnknownBlockDevice
                                           or MediumTypes.ReadOnlyBlockDevice
                                           or MediumTypes.ReadWriteBlockDevice)
        {
            _speedMultiplier = -1;
            Sbc(null, MediaType.Unknown, false);

            return;
        }

        if(compactDisc)
        {
            _speedMultiplier *= 177;
            CompactDisc();

            return;
        }

        _speedMultiplier *= 150;

        var   scsiReader = new Reader(_dev, _dev.Timeout, null, _errorLog, _dumpRaw);
        ulong blocks     = scsiReader.GetDeviceBlocks();
        _dumpLog.WriteLine(Localization.Core.Device_reports_disc_has_0_blocks, blocks);
        Dictionary<MediaTagType, byte[]> mediaTags = new();

        if(dskType == MediaType.PD650)
        {
            dskType = (blocks + 1) switch
                      {
                          1281856  => MediaType.PD650_WORM,
                          58620544 => MediaType.REV120,
                          17090880 => MediaType.REV35,
                          34185728 => MediaType.REV70,
                          _        => dskType
                      };
        }

#region Nintendo

        switch(dskType)
        {
            case MediaType.Unknown when blocks > 0:
                _dumpLog.WriteLine(Localization.Core.Reading_Physical_Format_Information);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Dvd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.PhysicalInformation,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    PFI.PhysicalFormatInformation? nintendoPfi = PFI.Decode(cmdBuf, dskType);

                    if(nintendoPfi is { DiskCategory: DiskCategory.Nintendo, PartVersion: 15 })
                    {
                        _dumpLog.WriteLine(Localization.Core
                                                       .Dumping_Nintendo_GameCube_or_Wii_discs_is_not_yet_implemented);

                        StoppingErrorMessage?.Invoke(Localization.Core
                                                                 .Dumping_Nintendo_GameCube_or_Wii_discs_is_not_yet_implemented);

                        return;
                    }
                }

                break;
            case MediaType.DVDDownload:
            case MediaType.DVDPR:
            case MediaType.DVDPRDL:
            case MediaType.DVDPRW:
            case MediaType.DVDPRWDL:
            case MediaType.DVDR:
            case MediaType.DVDRAM:
            case MediaType.DVDRDL:
            case MediaType.DVDROM:
            case MediaType.DVDRW:
            case MediaType.DVDRWDL:
            case MediaType.HDDVDR:
            case MediaType.HDDVDRAM:
            case MediaType.HDDVDRDL:
            case MediaType.HDDVDROM:
            case MediaType.HDDVDRW:
            case MediaType.HDDVDRWDL:
                _dumpLog.WriteLine(Localization.Core.Reading_Physical_Format_Information);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Dvd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.PhysicalInformation,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    PFI.PhysicalFormatInformation? nullablePfi = PFI.Decode(cmdBuf, dskType);

                    if(nullablePfi.HasValue)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DVD_PFI, tmpBuf);

                        PFI.PhysicalFormatInformation decPfi = nullablePfi.Value;
                        UpdateStatus?.Invoke($"PFI:\n{PFI.Prettify(decPfi)}");

                        // False book types
                        dskType = decPfi.DiskCategory switch
                                  {
                                      DiskCategory.DVDPR => MediaType.DVDPR,
                                      DiskCategory.DVDPRDL => MediaType.DVDPRDL,
                                      DiskCategory.DVDPRW => MediaType.DVDPRW,
                                      DiskCategory.DVDPRWDL => MediaType.DVDPRWDL,
                                      DiskCategory.DVDR => decPfi.PartVersion >= 6 ? MediaType.DVDRDL : MediaType.DVDR,
                                      DiskCategory.DVDRAM => MediaType.DVDRAM,
                                      DiskCategory.DVDRW => decPfi.PartVersion >= 15
                                                                ? MediaType.DVDRWDL
                                                                : MediaType.DVDRW,
                                      DiskCategory.HDDVDR   => MediaType.HDDVDR,
                                      DiskCategory.HDDVDRAM => MediaType.HDDVDRAM,
                                      DiskCategory.HDDVDROM => MediaType.HDDVDROM,
                                      DiskCategory.HDDVDRW  => MediaType.HDDVDRW,
                                      DiskCategory.Nintendo => decPfi.DiscSize == DVDSize.Eighty
                                                                   ? MediaType.GOD
                                                                   : MediaType.WOD,
                                      DiskCategory.UMD => MediaType.UMD,
                                      _                => MediaType.DVDROM
                                  };
                    }
                }

                _dumpLog.WriteLine(Localization.Core.Reading_Disc_Manufacturing_Information);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Dvd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.DiscManufacturingInformation,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    if(DMI.IsXbox(cmdBuf) || DMI.IsXbox360(cmdBuf))
                    {
                        if(DMI.IsXbox(cmdBuf))
                            dskType = MediaType.XGD;
                        else if(DMI.IsXbox360(cmdBuf))
                        {
                            dskType = MediaType.XGD2;

                            // All XGD3 all have the same number of blocks
                            if(blocks + 1 == 25063   || // Locked (or non compatible drive)
                               blocks + 1 == 4229664 || // Xtreme unlock
                               blocks + 1 == 4246304)   // Wxripper unlock
                                dskType = MediaType.XGD3;
                        }

                        isXbox = true;

                        sense = _dev.ScsiInquiry(out byte[] inqBuf, out _);

                        if(sense || Inquiry.Decode(inqBuf)?.KreonPresent != true)
                        {
                            _dumpLog.WriteLine(Localization.Core
                                                           .Dumping_Xbox_Game_Discs_requires_a_drive_with_Kreon_firmware);

                            StoppingErrorMessage?.Invoke(Localization.Core
                                                                     .Dumping_Xbox_Game_Discs_requires_a_drive_with_Kreon_firmware);

                            if(!_force) return;

                            isXbox = false;
                        }

                        if(_dumpRaw && !_force)
                        {
                            StoppingErrorMessage?.Invoke(Localization.Core
                                                                     .If_you_want_to_continue_reading_cooked_data_when_raw_is_not_available_use_the_force_option);

                            // TODO: Exit more gracefully
                            return;
                        }
                    }

                    if(cmdBuf.Length == 2052)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DVD_DMI, tmpBuf);
                    }
                }

                break;
        }

#endregion Nintendo

#region All DVD and HD DVD types

#endregion All DVD and HD DVD types

#region DVD-ROM

        if(dskType is MediaType.DVDDownload or MediaType.DVDROM)
        {
            _dumpLog.WriteLine(Localization.Core.Reading_Lead_in_Copyright_Information);

            sense = _dev.ReadDiscStructure(out cmdBuf,
                                           out _,
                                           MmcDiscStructureMediaType.Dvd,
                                           0,
                                           0,
                                           MmcDiscStructureFormat.CopyrightInformation,
                                           0,
                                           _dev.Timeout,
                                           out _);

            if(!sense)
            {
                if(CSS_CPRM.DecodeLeadInCopyright(cmdBuf).HasValue)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    mediaTags.Add(MediaTagType.DVD_CMI, tmpBuf);

                    CSS_CPRM.LeadInCopyright? cmi = CSS_CPRM.DecodeLeadInCopyright(cmdBuf);

                    if(cmi?.CopyrightType == CopyrightType.NoProtection)
                        UpdateStatus?.Invoke(Localization.Core.Drive_reports_no_copy_protection_on_disc);
                    else
                    {
                        if(!Settings.Settings.Current.EnableDecryption)
                        {
                            UpdateStatus?.Invoke(Localization.Core
                                                             .Drive_reports_the_disc_uses_copy_protection_The_dump_will_be_incorrect_unless_decryption_is_enabled);
                        }
                        else
                        {
                            if(cmi?.CopyrightType == CopyrightType.CSS)
                            {
                                UpdateStatus?.Invoke(Localization.Core.Drive_reports_disc_uses_CSS_copy_protection);

                                dvdDecrypt = new DVDDecryption(_dev);

                                sense = dvdDecrypt.ReadBusKey(out cmdBuf,
                                                              out _,
                                                              CSS_CPRM.DecodeLeadInCopyright(cmdBuf)?.CopyrightType ??
                                                              CopyrightType.NoProtection,
                                                              _dev.Timeout,
                                                              out _);

                                if(!sense)
                                {
                                    byte[] busKey = cmdBuf;

                                    UpdateStatus?.Invoke(Localization.Core.Reading_disc_key);
                                    sense = dvdDecrypt.ReadDiscKey(out cmdBuf, out _, _dev.Timeout, out _);

                                    if(!sense)
                                    {
                                        CSS_CPRM.DiscKey? decodedDiscKey = CSS.DecodeDiscKey(cmdBuf, busKey);

                                        sense = dvdDecrypt.ReadAsf(out cmdBuf,
                                                                   out _,
                                                                   DvdCssKeyClass.DvdCssCppmOrCprm,
                                                                   _dev.Timeout,
                                                                   out _);

                                        if(!sense)
                                        {
                                            if(cmdBuf[7] == 1)
                                            {
                                                UpdateStatus?.Invoke(Localization.Core
                                                                        .Disc_and_drive_authentication_succeeded);

                                                sense = dvdDecrypt.ReadRpc(out cmdBuf,
                                                                           out _,
                                                                           DvdCssKeyClass.DvdCssCppmOrCprm,
                                                                           _dev.Timeout,
                                                                           out _);

                                                if(!sense)
                                                {
                                                    CSS_CPRM.RegionalPlaybackControlState? rpc =
                                                        CSS_CPRM.DecodeRegionalPlaybackControlState(cmdBuf);

                                                    if(rpc.HasValue)
                                                    {
                                                        UpdateStatus?.Invoke(CSS.CheckRegion(rpc.Value, cmi.Value)
                                                                                 ? Localization.Core
                                                                                    .Disc_and_drive_regions_match
                                                                                 : Localization.Core
                                                                                    .Disc_and_drive_regions_do_not_match_The_dump_will_be_incorrect);
                                                    }
                                                }

                                                if(decodedDiscKey.HasValue)
                                                {
                                                    mediaTags.Add(MediaTagType.DVD_DiscKey, decodedDiscKey.Value.Key);

                                                    UpdateStatus?.Invoke(Localization.Core.Decrypting_disc_key);

                                                    CSS.DecryptDiscKey(decodedDiscKey.Value.Key, out byte[] discKey);

                                                    if(discKey != null)
                                                    {
                                                        UpdateStatus?.Invoke(Localization.Core
                                                                                .Decryption_of_disc_key_succeeded);

                                                        mediaTags.Add(MediaTagType.DVD_DiscKey_Decrypted, discKey);
                                                    }
                                                    else
                                                    {
                                                        UpdateStatus?.Invoke(Localization.Core
                                                                                .Decryption_of_disc_key_failed);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                UpdateStatus?.Invoke(string.Format(Localization.Core
                                                                      .Drive_reports_0_copy_protection_not_yet_supported_dump_incorrect,
                                                                   (CSS_CPRM.DecodeLeadInCopyright(cmdBuf)
                                                                           ?.CopyrightType ??
                                                                    CopyrightType.NoProtection).ToString()));
                            }
                        }
                    }
                }
            }
        }

#endregion DVD-ROM

        switch(dskType)
        {
#region DVD-ROM and HD DVD-ROM

            case MediaType.DVDDownload:
            case MediaType.DVDROM:
            case MediaType.HDDVDROM:
                _dumpLog.WriteLine(Localization.Core.Reading_Burst_Cutting_Area);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Dvd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.BurstCuttingArea,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    mediaTags.Add(MediaTagType.DVD_BCA, tmpBuf);
                }

                break;

#endregion DVD-ROM and HD DVD-ROM

#region DVD-RAM and HD DVD-RAM

            case MediaType.DVDRAM:
            case MediaType.HDDVDRAM:
                _dumpLog.WriteLine(Localization.Core.Reading_Disc_Description_Structure);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Dvd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.DvdramDds,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    if(DDS.Decode(cmdBuf).HasValue)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DVDRAM_DDS, tmpBuf);
                    }
                }

                _dumpLog.WriteLine(Localization.Core.Reading_Spare_Area_Information);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Dvd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.DvdramSpareAreaInformation,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    if(Spare.Decode(cmdBuf).HasValue)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DVDRAM_SpareArea, tmpBuf);
                    }
                }

                break;

#endregion DVD-RAM and HD DVD-RAM

#region DVD-R and DVD-RW

            case MediaType.DVDR:
            case MediaType.DVDRW:
                _dumpLog.WriteLine(Localization.Core.Reading_Pre_Recorded_Information);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Dvd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.PreRecordedInfo,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    mediaTags.Add(MediaTagType.DVDR_PreRecordedInfo, tmpBuf);
                }

                break;

#endregion DVD-R and DVD-RW
        }

        switch(dskType)
        {
#region DVD-R, DVD-RW and HD DVD-R

            case MediaType.DVDR:
            case MediaType.DVDRW:
            case MediaType.HDDVDR:
                _dumpLog.WriteLine(Localization.Core.Reading_Media_Identifier);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Dvd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.DvdrMediaIdentifier,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    mediaTags.Add(MediaTagType.DVDR_MediaIdentifier, tmpBuf);
                }

                _dumpLog.WriteLine(Localization.Core.Reading_Recordable_Physical_Information);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Dvd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.DvdrPhysicalInformation,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    mediaTags.Add(MediaTagType.DVDR_PFI, tmpBuf);
                }

                break;

#endregion DVD-R, DVD-RW and HD DVD-R

#region All DVD+

            case MediaType.DVDPR:
            case MediaType.DVDPRDL:
            case MediaType.DVDPRW:
            case MediaType.DVDPRWDL:
                _dumpLog.WriteLine(Localization.Core.Reading_ADdress_In_Pregroove);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Dvd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.Adip,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    mediaTags.Add(MediaTagType.DVD_ADIP, tmpBuf);
                }

                _dumpLog.WriteLine(Localization.Core.Reading_Disc_Control_Blocks);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Dvd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.Dcb,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    mediaTags.Add(MediaTagType.DCB, tmpBuf);
                }

                break;

#endregion All DVD+

#region HD DVD-ROM

            case MediaType.HDDVDROM:
                _dumpLog.WriteLine(Localization.Core.Reading_Lead_in_Copyright_Information);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Dvd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.HddvdCopyrightInformation,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    mediaTags.Add(MediaTagType.HDDVD_CPI, tmpBuf);
                }

                break;

#endregion HD DVD-ROM

#region All Blu-ray

            case MediaType.BDR:
            case MediaType.BDRE:
            case MediaType.BDROM:
            case MediaType.BDRXL:
            case MediaType.BDREXL:
            case MediaType.UHDBD:
                _dumpLog.WriteLine(Localization.Core.Reading_Disc_Information);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Bd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.DiscInformation,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    if(DI.Decode(cmdBuf).HasValue)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.BD_DI, tmpBuf);
                    }
                }

                // TODO: PAC
                /*
                dumpLog.WriteLine("Reading PAC.");
                sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Bd, 0, 0,
                                              MmcDiscStructureFormat.Pac, 0, dev.Timeout, out _);
                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    mediaTags.Add(MediaTagType.PAC, tmpBuf);
                }*/
                break;

#endregion All Blu-ray
        }

        switch(dskType)
        {
#region BD-ROM only

            case MediaType.BDROM:
            case MediaType.UHDBD:
                _dumpLog.WriteLine(Localization.Core.Reading_Burst_Cutting_Area);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Bd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.BdBurstCuttingArea,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    mediaTags.Add(MediaTagType.BD_BCA, tmpBuf);
                }

                break;

#endregion BD-ROM only

#region Writable Blu-ray only

            case MediaType.BDR:
            case MediaType.BDRE:
            case MediaType.BDRXL:
            case MediaType.BDREXL:
                _dumpLog.WriteLine(Localization.Core.Reading_Disc_Definition_Structure);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Bd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.BdDds,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    mediaTags.Add(MediaTagType.BD_DDS, tmpBuf);
                }

                _dumpLog.WriteLine(Localization.Core.Reading_Spare_Area_Information);

                sense = _dev.ReadDiscStructure(out cmdBuf,
                                               out _,
                                               MmcDiscStructureMediaType.Bd,
                                               0,
                                               0,
                                               MmcDiscStructureFormat.BdSpareAreaInformation,
                                               0,
                                               _dev.Timeout,
                                               out _);

                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    mediaTags.Add(MediaTagType.BD_SpareArea, tmpBuf);
                }

                break;

#endregion Writable Blu-ray only
        }

        if(isXbox)
        {
            Xgd(mediaTags, dskType);

            return;
        }

        Sbc(mediaTags, dskType, true, dvdDecrypt);
    }

    // TODO: Move somewhere else
    internal static void AddMediaTagToSidecar(string outputPath, MediaTagType tagType, byte[] tag, ref Metadata sidecar)
    {
        switch(tagType)
        {
            case MediaTagType.DVD_PFI:
                sidecar.OpticalDiscs[0].Pfi = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.DVD_DMI:
                sidecar.OpticalDiscs[0].Dmi = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.DVD_CMI:
            case MediaTagType.HDDVD_CPI:
                sidecar.OpticalDiscs[0].Cmi = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                var tmp = new byte[tag.Length + 4];
                Array.Copy(tag, 0, tmp, 4, tag.Length);
                tmp[0] = (byte)((tag.Length & 0xFF00) >> 8);
                tmp[1] = (byte)(tag.Length & 0xFF);

                CSS_CPRM.LeadInCopyright? cpy = CSS_CPRM.DecodeLeadInCopyright(tmp);

                if(cpy.HasValue && cpy.Value.CopyrightType != CopyrightType.NoProtection)
                    sidecar.OpticalDiscs[0].CopyProtection = cpy.Value.CopyrightType.ToString();

                break;
            case MediaTagType.DVD_BCA:
            case MediaTagType.BD_BCA:
                sidecar.OpticalDiscs[0].Bca = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.BD_DDS:
            case MediaTagType.DVDRAM_DDS:
                sidecar.OpticalDiscs[0].Dds = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.DVDRAM_SpareArea:
            case MediaTagType.BD_SpareArea:
                sidecar.OpticalDiscs[0].Sai = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.DVDR_PreRecordedInfo:
                sidecar.OpticalDiscs[0].Pri = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.DVD_MediaIdentifier:
                sidecar.OpticalDiscs[0].MediaID = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.DVDR_PFI:
                sidecar.OpticalDiscs[0].Pfir = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.DVD_ADIP:
                sidecar.OpticalDiscs[0].Adip = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.DCB:
                sidecar.OpticalDiscs[0].Dcb = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.BD_DI:
                sidecar.OpticalDiscs[0].Di = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.Xbox_SecuritySector:
                sidecar.OpticalDiscs[0].Xbox ??= new Xbox();

                sidecar.OpticalDiscs[0].Xbox.SecuritySectors = new List<XboxSecuritySector>
                {
                    new()
                    {
                        RequestNumber  = 0,
                        RequestVersion = 1,
                        SecuritySectors = new CommonTypes.AaruMetadata.Dump
                        {
                            Image     = outputPath,
                            Size      = (ulong)tag.Length,
                            Checksums = Checksum.GetChecksums(tag)
                        }
                    }
                };

                break;
            case MediaTagType.Xbox_PFI:
                sidecar.OpticalDiscs[0].Xbox ??= new Xbox();

                sidecar.OpticalDiscs[0].Xbox.Pfi = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.Xbox_DMI:
                sidecar.OpticalDiscs[0].Xbox ??= new Xbox();

                sidecar.OpticalDiscs[0].Xbox.Dmi = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.CD_FullTOC:
                sidecar.OpticalDiscs[0].Toc = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.CD_ATIP:
                sidecar.OpticalDiscs[0].Atip = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.CD_PMA:
                sidecar.OpticalDiscs[0].Pma = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.CD_TEXT:
                sidecar.OpticalDiscs[0].LeadInCdText = new CommonTypes.AaruMetadata.Dump
                {
                    Image     = outputPath,
                    Size      = (ulong)tag.Length,
                    Checksums = Checksum.GetChecksums(tag)
                };

                break;
            case MediaTagType.CD_FirstTrackPregap:
                sidecar.OpticalDiscs[0].FirstTrackPregrap = new List<Border>
                {
                    new()
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag)
                    }
                };

                break;
            case MediaTagType.CD_LeadIn:
                sidecar.OpticalDiscs[0].LeadIn = new List<Border>
                {
                    new()
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag)
                    }
                };

                break;
        }
    }
}