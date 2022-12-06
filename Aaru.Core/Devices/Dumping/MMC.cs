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
// Copyright © 2011-2023 Natalia Portillo
// Copyright © 2020-2023 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.Bluray;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Decryption;
using Aaru.Decryption.DVD;
using Aaru.Devices;
using Schemas;
using DDS = Aaru.Decoders.DVD.DDS;
using DMI = Aaru.Decoders.Xbox.DMI;
using DVDDecryption = Aaru.Decryption.DVD.Dump;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;
using Spare = Aaru.Decoders.DVD.Spare;

// ReSharper disable JoinDeclarationAndInitializer

namespace Aaru.Core.Devices.Dumping
{
    /// <summary>Implement dumping optical discs from MultiMedia devices</summary>
    partial class Dump
    {
        /// <summary>Dumps an optical disc</summary>
        void Mmc()
        {
            MediaType     dskType = MediaType.Unknown;
            bool          sense;
            byte[]        tmpBuf;
            bool          compactDisc      = true;
            bool          gotConfiguration = false;
            bool          isXbox           = false;
            DVDDecryption dvdDecrypt       = null;
            _speedMultiplier = 1;

            // TODO: Log not only what is it reading, but if it was read correctly or not.
            sense = _dev.GetConfiguration(out byte[] cmdBuf, out _, 0, MmcGetConfigurationRt.Current, _dev.Timeout,
                                          out _);

            if(!sense)
            {
                gotConfiguration = true;
                Features.SeparatedFeatures ftr = Features.Separate(cmdBuf);
                _dumpLog.WriteLine("Device reports current profile is 0x{0:X4}", ftr.CurrentProfile);

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

            sense = _dev.ModeSense6(out cmdBuf, out _, true, ScsiModeSensePageControl.Current, 0x00, _dev.Timeout,
                                    out _);

            if(sense || _dev.Error)
            {
                sense = _dev.ModeSense6(out cmdBuf, out _, false, ScsiModeSensePageControl.Current, 0x00, _dev.Timeout,
                                        out _);

                if(!sense &&
                   !_dev.Error)
                    decMode = Modes.DecodeMode6(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);
            }
            else
                decMode = Modes.DecodeMode6(cmdBuf, PeripheralDeviceTypes.MultiMediaDevice);

            if(decMode is null)
            {
                sense = _dev.ModeSense10(out cmdBuf, out _, false, true, ScsiModeSensePageControl.Current, 0x3F, 0x00,
                                         _dev.Timeout, out _);

                if(sense || _dev.Error)
                {
                    sense = _dev.ModeSense10(out cmdBuf, out _, false, false, ScsiModeSensePageControl.Current, 0x3F,
                                             0x00, _dev.Timeout, out _);

                    if(sense || _dev.Error)
                    {
                        sense = _dev.ModeSense10(out cmdBuf, out _, false, true, ScsiModeSensePageControl.Current, 0x00,
                                                 0x00, _dev.Timeout, out _);

                        if(sense || _dev.Error)
                        {
                            sense = _dev.ModeSense10(out cmdBuf, out _, false, false, ScsiModeSensePageControl.Current,
                                                     0x00, 0x00, _dev.Timeout, out _);

                            if(!sense &&
                               !_dev.Error)
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
               (decMode.Value.Header.MediumType == MediumTypes.UnknownBlockDevice  ||
                decMode.Value.Header.MediumType == MediumTypes.ReadOnlyBlockDevice ||
                decMode.Value.Header.MediumType == MediumTypes.ReadWriteBlockDevice))
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
            _dumpLog.WriteLine("Device reports disc has {0} blocks", blocks);
            Dictionary<MediaTagType, byte[]> mediaTags = new Dictionary<MediaTagType, byte[]>();

            if(dskType == MediaType.PD650)
                switch(blocks + 1)
                {
                    case 1281856:
                        dskType = MediaType.PD650_WORM;

                        break;
                    case 58620544:
                        dskType = MediaType.REV120;

                        break;
                    case 17090880:
                        dskType = MediaType.REV35;

                        break;

                    case 34185728:
                        dskType = MediaType.REV70;

                        break;
                }

            #region Nintendo
            switch(dskType)
            {
                case MediaType.Unknown when blocks > 0:
                    _dumpLog.WriteLine("Reading Physical Format Information");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                   MmcDiscStructureFormat.PhysicalInformation, 0, _dev.Timeout, out _);

                    if(!sense)
                    {
                        PFI.PhysicalFormatInformation? nintendoPfi = PFI.Decode(cmdBuf, dskType);

                        if(nintendoPfi?.DiskCategory     == DiskCategory.Nintendo &&
                           nintendoPfi.Value.PartVersion == 15)
                        {
                            _dumpLog.WriteLine("Dumping Nintendo GameCube or Wii discs is not yet implemented.");

                            StoppingErrorMessage?.
                                Invoke("Dumping Nintendo GameCube or Wii discs is not yet implemented.");

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
                    _dumpLog.WriteLine("Reading Physical Format Information");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                   MmcDiscStructureFormat.PhysicalInformation, 0, _dev.Timeout, out _);

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
                            switch(decPfi.DiskCategory)
                            {
                                case DiskCategory.DVDPR:
                                    dskType = MediaType.DVDPR;

                                    break;
                                case DiskCategory.DVDPRDL:
                                    dskType = MediaType.DVDPRDL;

                                    break;
                                case DiskCategory.DVDPRW:
                                    dskType = MediaType.DVDPRW;

                                    break;
                                case DiskCategory.DVDPRWDL:
                                    dskType = MediaType.DVDPRWDL;

                                    break;
                                case DiskCategory.DVDR:
                                    dskType = decPfi.PartVersion >= 6 ? MediaType.DVDRDL : MediaType.DVDR;

                                    break;
                                case DiskCategory.DVDRAM:
                                    dskType = MediaType.DVDRAM;

                                    break;
                                default:
                                    dskType = MediaType.DVDROM;

                                    break;
                                case DiskCategory.DVDRW:
                                    dskType = decPfi.PartVersion >= 15 ? MediaType.DVDRWDL : MediaType.DVDRW;

                                    break;
                                case DiskCategory.HDDVDR:
                                    dskType = MediaType.HDDVDR;

                                    break;
                                case DiskCategory.HDDVDRAM:
                                    dskType = MediaType.HDDVDRAM;

                                    break;
                                case DiskCategory.HDDVDROM:
                                    dskType = MediaType.HDDVDROM;

                                    break;
                                case DiskCategory.HDDVDRW:
                                    dskType = MediaType.HDDVDRW;

                                    break;
                                case DiskCategory.Nintendo:
                                    dskType = decPfi.DiscSize == DVDSize.Eighty ? MediaType.GOD : MediaType.WOD;

                                    break;
                                case DiskCategory.UMD:
                                    dskType = MediaType.UMD;

                                    break;
                            }
                        }
                    }

                    _dumpLog.WriteLine("Reading Disc Manufacturing Information");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                   MmcDiscStructureFormat.DiscManufacturingInformation, 0, _dev.Timeout,
                                                   out _);

                    if(!sense)
                    {
                        if(DMI.IsXbox(cmdBuf) ||
                           DMI.IsXbox360(cmdBuf))
                        {
                            if(DMI.IsXbox(cmdBuf))
                            {
                                dskType = MediaType.XGD;
                            }
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
                                _dumpLog.WriteLine("Dumping Xbox Game Discs requires a drive with Kreon firmware.");

                                StoppingErrorMessage?.
                                    Invoke("Dumping Xbox Game Discs requires a drive with Kreon firmware.");

                                if(!_force)
                                    return;

                                isXbox = false;
                            }

                            if(_dumpRaw && !_force)
                            {
                                StoppingErrorMessage?.
                                    Invoke("Not continuing. If you want to continue reading cooked data when raw is not available use the force option.");

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
            if(dskType == MediaType.DVDDownload ||
               dskType == MediaType.DVDROM)
            {
                _dumpLog.WriteLine("Reading Lead-in Copyright Information.");

                sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                               MmcDiscStructureFormat.CopyrightInformation, 0, _dev.Timeout, out _);

                if(!sense)
                    if(CSS_CPRM.DecodeLeadInCopyright(cmdBuf).HasValue)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DVD_CMI, tmpBuf);

                        CSS_CPRM.LeadInCopyright? cmi = CSS_CPRM.DecodeLeadInCopyright(cmdBuf);

                        if(cmi?.CopyrightType == CopyrightType.NoProtection)
                            UpdateStatus?.Invoke("Drive reports no copy protection on disc.");
                        else
                        {
                            if(!Settings.Settings.Current.EnableDecryption)
                                UpdateStatus?.Invoke("Drive reports the disc uses copy protection. " +
                                                     "The dump will be incorrect unless decryption is enabled.");
                            else
                            {
                                if(cmi?.CopyrightType == CopyrightType.CSS)
                                {
                                    UpdateStatus?.Invoke("Drive reports disc uses CSS copy protection.");

                                    dvdDecrypt = new DVDDecryption(_dev);

                                    sense = dvdDecrypt.ReadBusKey(out cmdBuf, out _,
                                                                  CSS_CPRM.DecodeLeadInCopyright(cmdBuf)?.
                                                                           CopyrightType ?? CopyrightType.NoProtection,
                                                                  _dev.Timeout, out _);

                                    if(!sense)
                                    {
                                        byte[] busKey = cmdBuf;

                                        UpdateStatus?.Invoke("Reading disc key.");
                                        sense = dvdDecrypt.ReadDiscKey(out cmdBuf, out _, _dev.Timeout, out _);

                                        if(!sense)
                                        {
                                            CSS_CPRM.DiscKey? decodedDiscKey = CSS.DecodeDiscKey(cmdBuf, busKey);

                                            sense = dvdDecrypt.ReadAsf(out cmdBuf, out _,
                                                                       DvdCssKeyClass.DvdCssCppmOrCprm, _dev.Timeout,
                                                                       out _);

                                            if(!sense)
                                            {
                                                if(cmdBuf[7] == 1)
                                                {
                                                    UpdateStatus?.Invoke("Disc and drive authentication succeeded.");

                                                    sense = dvdDecrypt.ReadRpc(out cmdBuf, out _,
                                                                               DvdCssKeyClass.DvdCssCppmOrCprm,
                                                                               _dev.Timeout, out _);

                                                    if(!sense)
                                                    {
                                                        CSS_CPRM.RegionalPlaybackControlState? rpc =
                                                            CSS_CPRM.DecodeRegionalPlaybackControlState(cmdBuf);

                                                        if(rpc.HasValue)
                                                        {
                                                            UpdateStatus?.Invoke(CSS.CheckRegion(rpc.Value, cmi.Value)
                                                                ? "Disc and drive regions match."
                                                                : "Disc and drive regions do not match. The dump will be incorrect");
                                                        }
                                                    }

                                                    if(decodedDiscKey.HasValue)
                                                    {
                                                        mediaTags.Add(MediaTagType.DVD_DiscKey,
                                                                      decodedDiscKey.Value.Key);

                                                        UpdateStatus?.Invoke("Decrypting disc key.");

                                                        CSS.DecryptDiscKey(decodedDiscKey.Value.Key,
                                                                           out byte[] discKey);

                                                        if(discKey != null)
                                                        {
                                                            UpdateStatus?.Invoke("Decryption of disc key succeeded.");
                                                            mediaTags.Add(MediaTagType.DVD_DiscKey_Decrypted, discKey);
                                                        }
                                                        else
                                                            UpdateStatus?.Invoke("Decryption of disc key failed.");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    UpdateStatus?.
                                        Invoke($"Drive reports disc uses {(CSS_CPRM.DecodeLeadInCopyright(cmdBuf)?.CopyrightType ?? CopyrightType.NoProtection).ToString()} copy protection. " +
                                               "This is not yet supported and the dump will be incorrect.");
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
                    _dumpLog.WriteLine("Reading Burst Cutting Area.");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                   MmcDiscStructureFormat.BurstCuttingArea, 0, _dev.Timeout, out _);

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
                    _dumpLog.WriteLine("Reading Disc Description Structure.");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                   MmcDiscStructureFormat.DvdramDds, 0, _dev.Timeout, out _);

                    if(!sense)
                        if(DDS.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            mediaTags.Add(MediaTagType.DVDRAM_DDS, tmpBuf);
                        }

                    _dumpLog.WriteLine("Reading Spare Area Information.");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                   MmcDiscStructureFormat.DvdramSpareAreaInformation, 0, _dev.Timeout,
                                                   out _);

                    if(!sense)
                        if(Spare.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            mediaTags.Add(MediaTagType.DVDRAM_SpareArea, tmpBuf);
                        }

                    break;
                #endregion DVD-RAM and HD DVD-RAM

                #region DVD-R and DVD-RW
                case MediaType.DVDR:
                case MediaType.DVDRW:
                    _dumpLog.WriteLine("Reading Pre-Recorded Information.");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                   MmcDiscStructureFormat.PreRecordedInfo, 0, _dev.Timeout, out _);

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
                    _dumpLog.WriteLine("Reading Media Identifier.");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                   MmcDiscStructureFormat.DvdrMediaIdentifier, 0, _dev.Timeout, out _);

                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DVDR_MediaIdentifier, tmpBuf);
                    }

                    _dumpLog.WriteLine("Reading Recordable Physical Information.");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                   MmcDiscStructureFormat.DvdrPhysicalInformation, 0, _dev.Timeout,
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
                    _dumpLog.WriteLine("Reading ADdress In Pregroove.");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                   MmcDiscStructureFormat.Adip, 0, _dev.Timeout, out _);

                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DVD_ADIP, tmpBuf);
                    }

                    _dumpLog.WriteLine("Reading Disc Control Blocks.");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                   MmcDiscStructureFormat.Dcb, 0, _dev.Timeout, out _);

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
                    _dumpLog.WriteLine("Reading Lead-in Copyright Information.");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                   MmcDiscStructureFormat.HddvdCopyrightInformation, 0, _dev.Timeout,
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
                    _dumpLog.WriteLine("Reading Disc Information.");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Bd, 0, 0,
                                                   MmcDiscStructureFormat.DiscInformation, 0, _dev.Timeout, out _);

                    if(!sense)
                        if(DI.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            mediaTags.Add(MediaTagType.BD_DI, tmpBuf);
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
                    _dumpLog.WriteLine("Reading Burst Cutting Area.");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Bd, 0, 0,
                                                   MmcDiscStructureFormat.BdBurstCuttingArea, 0, _dev.Timeout, out _);

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
                    _dumpLog.WriteLine("Reading Disc Definition Structure.");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Bd, 0, 0,
                                                   MmcDiscStructureFormat.BdDds, 0, _dev.Timeout, out _);

                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.BD_DDS, tmpBuf);
                    }

                    _dumpLog.WriteLine("Reading Spare Area Information.");

                    sense = _dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Bd, 0, 0,
                                                   MmcDiscStructureFormat.BdSpareAreaInformation, 0, _dev.Timeout,
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
        internal static void AddMediaTagToSidecar(string outputPath, MediaTagType tagType, byte[] tag,
                                                  ref CICMMetadataType sidecar)
        {
            switch(tagType)
            {
                case MediaTagType.DVD_PFI:
                    sidecar.OpticalDisc[0].PFI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.DVD_DMI:
                    sidecar.OpticalDisc[0].DMI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.DVD_CMI:
                case MediaTagType.HDDVD_CPI:
                    sidecar.OpticalDisc[0].CMI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    byte[] tmp = new byte[tag.Length + 4];
                    Array.Copy(tag, 0, tmp, 4, tag.Length);
                    tmp[0] = (byte)((tag.Length & 0xFF00) >> 8);
                    tmp[1] = (byte)(tag.Length & 0xFF);

                    CSS_CPRM.LeadInCopyright? cpy = CSS_CPRM.DecodeLeadInCopyright(tmp);

                    if(cpy.HasValue &&
                       cpy.Value.CopyrightType != CopyrightType.NoProtection)
                        sidecar.OpticalDisc[0].CopyProtection = cpy.Value.CopyrightType.ToString();

                    break;
                case MediaTagType.DVD_BCA:
                case MediaTagType.BD_BCA:
                    sidecar.OpticalDisc[0].BCA = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.BD_DDS:
                case MediaTagType.DVDRAM_DDS:
                    sidecar.OpticalDisc[0].DDS = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.DVDRAM_SpareArea:
                case MediaTagType.BD_SpareArea:
                    sidecar.OpticalDisc[0].SAI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.DVDR_PreRecordedInfo:
                    sidecar.OpticalDisc[0].PRI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.DVD_MediaIdentifier:
                    sidecar.OpticalDisc[0].MediaID = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.DVDR_PFI:
                    sidecar.OpticalDisc[0].PFIR = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.DVD_ADIP:
                    sidecar.OpticalDisc[0].ADIP = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.DCB:
                    sidecar.OpticalDisc[0].DCB = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.BD_DI:
                    sidecar.OpticalDisc[0].DI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.Xbox_SecuritySector:
                    sidecar.OpticalDisc[0].Xbox ??= new XboxType();

                    sidecar.OpticalDisc[0].Xbox.SecuritySectors = new[]
                    {
                        new XboxSecuritySectorsType
                        {
                            RequestNumber  = 0,
                            RequestVersion = 1,
                            SecuritySectors = new DumpType
                            {
                                Image     = outputPath,
                                Size      = (ulong)tag.Length,
                                Checksums = Checksum.GetChecksums(tag).ToArray()
                            }
                        }
                    };

                    break;
                case MediaTagType.Xbox_PFI:
                    sidecar.OpticalDisc[0].Xbox ??= new XboxType();

                    sidecar.OpticalDisc[0].Xbox.PFI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.Xbox_DMI:
                    sidecar.OpticalDisc[0].Xbox ??= new XboxType();

                    sidecar.OpticalDisc[0].Xbox.DMI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.CD_FullTOC:
                    sidecar.OpticalDisc[0].TOC = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.CD_ATIP:
                    sidecar.OpticalDisc[0].ATIP = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.CD_PMA:
                    sidecar.OpticalDisc[0].PMA = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.CD_TEXT:
                    sidecar.OpticalDisc[0].LeadInCdText = new DumpType
                    {
                        Image     = outputPath,
                        Size      = (ulong)tag.Length,
                        Checksums = Checksum.GetChecksums(tag).ToArray()
                    };

                    break;
                case MediaTagType.CD_FirstTrackPregap:
                    sidecar.OpticalDisc[0].FirstTrackPregrap = new[]
                    {
                        new BorderType
                        {
                            Image     = outputPath,
                            Size      = (ulong)tag.Length,
                            Checksums = Checksum.GetChecksums(tag).ToArray()
                        }
                    };

                    break;
                case MediaTagType.CD_LeadIn:
                    sidecar.OpticalDisc[0].LeadIn = new[]
                    {
                        new BorderType
                        {
                            Image     = outputPath,
                            Size      = (ulong)tag.Length,
                            Checksums = Checksum.GetChecksums(tag).ToArray()
                        }
                    };

                    break;
            }
        }
    }
}