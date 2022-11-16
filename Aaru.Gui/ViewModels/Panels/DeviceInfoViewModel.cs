// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DeviceInfoViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the device information panel.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Aaru.Decoders.SCSI.SSC;
using Aaru.Devices;
using Aaru.Gui.ViewModels.Tabs;
using Aaru.Gui.Views.Tabs;
using Avalonia.Controls;
using ReactiveUI;
using DeviceInfo = Aaru.Core.Devices.Info.DeviceInfo;

namespace Aaru.Gui.ViewModels.Panels;

public sealed class DeviceInfoViewModel : ViewModelBase
{
    readonly DeviceInfo _devInfo;
    readonly Window     _view;
    AtaInfo             _ataInfo;
    string              _blockLimits;
    string              _blockSizeGranularity;
    string              _cid;
    string              _csd;
    string              _densities;
    string              _deviceType;
    string              _extendedCsd;
    string              _firewireGuid;
    string              _firewireManufacturer;
    string              _firewireModel;
    string              _firewireModelId;
    string              _firewireVendorId;
    bool                _firewireVisible;
    bool                _kreon;
    bool                _kreonChallengeResponse;
    bool                _kreonChallengeResponse360;
    bool                _kreonDecryptSs;
    bool                _kreonDecryptSs360;
    bool                _kreonErrorSkipping;
    bool                _kreonLock;
    bool                _kreonWxripperUnlock;
    bool                _kreonWxripperUnlock360;
    bool                _kreonXtremeUnlock;
    bool                _kreonXtremeUnlock360;
    string              _manufacturer;
    string              _maxBlockSize;
    string              _mediumDensity;
    string              _mediumTypes;
    string              _minBlockSize;
    string              _model;
    string              _ocr;
    PcmciaInfo          _pcmciaInfo;
    bool                _plextorBitSetting;
    bool                _plextorBitSettingDl;
    string              _plextorCdReadTime;
    string              _plextorCdWriteTime;
    string              _plextorDiscs;
    string              _plextorDvd;
    bool                _plextorDvdPlusWriteTest;
    string              _plextorDvdReadTime;
    bool                _plextorDvdTimesVisible;
    string              _plextorDvdWriteTime;
    bool                _plextorEepromVisible;
    bool                _plextorGigaRec;
    bool                _plextorHidesRecordables;
    bool                _plextorHidesSessions;
    bool                _plextorHiding;
    bool                _plextorPoweRec;
    bool                _plextorPoweRecEnabled;
    string              _plextorPoweRecLast;
    bool                _plextorPoweRecLastVisible;
    string              _plextorPoweRecMax;
    bool                _plextorPoweRecMaxVisible;
    string              _plextorPoweRecRecommended;
    bool                _plextorPoweRecRecommendedVisible;
    string              _plextorPoweRecSelected;
    bool                _plextorPoweRecSelectedVisible;
    bool                _plextorSecuRec;
    bool                _plextorSilentMode;
    string              _plextorSilentModeAccessTime;
    string              _plextorSilentModeCdReadSpeedLimit;
    string              _plextorSilentModeCdWriteSpeedLimit;
    string              _plextorSilentModeDvdReadSpeedLimit;
    bool                _plextorSilentModeDvdReadSpeedLimitVisible;
    bool                _plextorSilentModeEnabled;
    bool                _plextorSpeedEnabled;
    bool                _plextorSpeedRead;
    bool                _plextorVariRec;
    bool                _plextorVariRecDvd;
    bool                _plextorVisible;
    bool                _removable;
    string              _revision;
    bool                _saveUsbDescriptorsEnabled;
    string              _scr;
    ScsiInfo            _scsiInfo;
    string              _scsiType;
    string              _sdMm;
    SdMmcInfo           _sdMmcInfo;
    string              _secureDigital;
    string              _serial;
    bool                _ssc;
    string              _usbConnected;
    string              _usbManufacturer;
    string              _usbProduct;
    string              _usbProductId;
    string              _usbSerial;
    string              _usbVendorId;
    bool                _usbVisible;

    public DeviceInfoViewModel(DeviceInfo devInfo, Window view)
    {
        SaveUsbDescriptorsCommand = ReactiveCommand.Create(ExecuteSaveUsbDescriptorsCommand);
        _view                     = view;
        _devInfo                  = devInfo;

        DeviceType   = devInfo.Type.ToString();
        Manufacturer = devInfo.Manufacturer;
        Model        = devInfo.Model;
        Revision     = devInfo.FirmwareRevision;
        Serial       = devInfo.Serial;
        ScsiType     = devInfo.ScsiType.ToString();
        Removable    = devInfo.IsRemovable;
        UsbVisible   = devInfo.IsUsb;

        if(devInfo.IsUsb)
        {
            UsbVisible                = true;
            SaveUsbDescriptorsEnabled = devInfo.UsbDescriptors != null;
            UsbVendorId               = $"{devInfo.UsbVendorId:X4}";
            UsbProductId              = $"{devInfo.UsbProductId:X4}";
            UsbManufacturer           = devInfo.UsbManufacturerString;
            UsbProduct                = devInfo.UsbProductString;
            UsbSerial                 = devInfo.UsbSerialString;
        }

        if(devInfo.IsFireWire)
        {
            FirewireVisible      = true;
            FirewireVendorId     = $"{devInfo.FireWireVendor:X4}";
            FirewireModelId      = $"{devInfo.FireWireModel:X4}";
            FirewireManufacturer = devInfo.FireWireVendorName;
            FirewireModel        = devInfo.FireWireModelName;
            FirewireGuid         = $"{devInfo.FireWireGuid:X16}";
        }

        if(devInfo.IsPcmcia)
            PcmciaInfo = new PcmciaInfo
            {
                DataContext = new PcmciaInfoViewModel(devInfo.Cis, _view)
            };

        if(devInfo.AtaIdentify   != null ||
           devInfo.AtapiIdentify != null)
            AtaInfo = new AtaInfo
            {
                DataContext =
                    new AtaInfoViewModel(devInfo.AtaIdentify, devInfo.AtapiIdentify, devInfo.AtaMcptError, _view)
            };

        if(devInfo.ScsiInquiryData != null)
        {
            ScsiInfo = new ScsiInfo
            {
                DataContext = new ScsiInfoViewModel(devInfo.ScsiInquiryData, devInfo.ScsiInquiry, devInfo.ScsiEvpdPages,
                                                    devInfo.ScsiMode, devInfo.ScsiType, devInfo.ScsiModeSense6,
                                                    devInfo.ScsiModeSense10, devInfo.MmcConfiguration, _view)
            };

            if(devInfo.PlextorFeatures != null)
            {
                PlextorVisible = true;

                if(devInfo.PlextorFeatures.Eeprom != null)
                {
                    PlextorEepromVisible = true;
                    PlextorDiscs         = $"{devInfo.PlextorFeatures.Discs}";
                    PlextorCdReadTime    = TimeSpan.FromSeconds(devInfo.PlextorFeatures.CdReadTime).ToString();

                    PlextorCdWriteTime = TimeSpan.FromSeconds(devInfo.PlextorFeatures.CdWriteTime).ToString();

                    if(devInfo.PlextorFeatures.IsDvd)
                    {
                        PlextorDvdTimesVisible = true;

                        PlextorDvdReadTime = TimeSpan.FromSeconds(devInfo.PlextorFeatures.DvdReadTime).ToString();

                        PlextorDvdWriteTime = TimeSpan.FromSeconds(devInfo.PlextorFeatures.DvdWriteTime).ToString();
                    }
                }

                PlextorPoweRec = devInfo.PlextorFeatures.PoweRec;

                if(devInfo.PlextorFeatures.PoweRec)
                {
                    PlextorPoweRecEnabled = devInfo.PlextorFeatures.PoweRecEnabled;

                    if(devInfo.PlextorFeatures.PoweRecEnabled)
                    {
                        PlextorPoweRecEnabled = true;

                        if(devInfo.PlextorFeatures.PoweRecRecommendedSpeed > 0)
                        {
                            PlextorPoweRecRecommendedVisible = true;

                            PlextorPoweRecRecommended = $"{devInfo.PlextorFeatures.PoweRecRecommendedSpeed} Kb/sec.";
                        }

                        if(devInfo.PlextorFeatures.PoweRecSelected > 0)
                        {
                            PlextorPoweRecSelectedVisible = true;

                            PlextorPoweRecSelected = $"{devInfo.PlextorFeatures.PoweRecSelected} Kb/sec.";
                        }

                        if(devInfo.PlextorFeatures.PoweRecMax > 0)
                        {
                            PlextorPoweRecMaxVisible = true;
                            PlextorPoweRecMax        = $"{devInfo.PlextorFeatures.PoweRecMax} Kb/sec.";
                        }

                        if(devInfo.PlextorFeatures.PoweRecLast > 0)
                        {
                            PlextorPoweRecLastVisible = true;
                            PlextorPoweRecLast        = $"{devInfo.PlextorFeatures.PoweRecLast} Kb/sec.";
                        }
                    }
                }

                PlextorSilentMode = devInfo.PlextorFeatures.SilentMode;

                if(devInfo.PlextorFeatures.SilentMode)
                {
                    PlextorSilentModeEnabled = devInfo.PlextorFeatures.SilentModeEnabled;

                    if(devInfo.PlextorFeatures.SilentModeEnabled)
                    {
                        PlextorSilentModeAccessTime = devInfo.PlextorFeatures.AccessTimeLimit == 2
                                                          ? "\tAccess time is slow" : "\tAccess time is fast";

                        PlextorSilentModeCdReadSpeedLimit =
                            devInfo.PlextorFeatures.CdReadSpeedLimit > 0
                                ? $"{devInfo.PlextorFeatures.CdReadSpeedLimit}x" : "unlimited";

                        PlextorSilentModeCdWriteSpeedLimit =
                            devInfo.PlextorFeatures.CdWriteSpeedLimit > 0
                                ? $"{devInfo.PlextorFeatures.CdReadSpeedLimit}x" : "unlimited";

                        if(devInfo.PlextorFeatures.IsDvd)
                        {
                            PlextorSilentModeDvdReadSpeedLimitVisible = true;

                            PlextorSilentModeDvdReadSpeedLimit =
                                devInfo.PlextorFeatures.DvdReadSpeedLimit > 0
                                    ? $"{devInfo.PlextorFeatures.DvdReadSpeedLimit}x" : "unlimited";
                        }
                    }
                }

                PlextorGigaRec   = devInfo.PlextorFeatures.GigaRec;
                PlextorSecuRec   = devInfo.PlextorFeatures.SecuRec;
                PlextorSpeedRead = devInfo.PlextorFeatures.SpeedRead;

                if(devInfo.PlextorFeatures.SpeedRead)
                    PlextorSpeedEnabled = devInfo.PlextorFeatures.SpeedReadEnabled;

                PlextorHiding = devInfo.PlextorFeatures.Hiding;

                if(devInfo.PlextorFeatures.Hiding)
                {
                    PlextorHidesRecordables = devInfo.PlextorFeatures.HidesRecordables;
                    PlextorHidesSessions    = devInfo.PlextorFeatures.HidesSessions;
                }

                PlextorVariRec = devInfo.PlextorFeatures.VariRec;

                if(devInfo.PlextorFeatures.IsDvd)
                {
                    PlextorVariRecDvd       = devInfo.PlextorFeatures.VariRecDvd;
                    PlextorBitSetting       = devInfo.PlextorFeatures.BitSetting;
                    PlextorBitSettingDl     = devInfo.PlextorFeatures.BitSettingDl;
                    PlextorDvdPlusWriteTest = devInfo.PlextorFeatures.DvdPlusWriteTest;
                }
            }

            if(devInfo.ScsiInquiry?.KreonPresent == true)
            {
                Kreon                  = true;
                KreonChallengeResponse = devInfo.KreonFeatures.HasFlag(KreonFeatures.ChallengeResponse);
                KreonDecryptSs         = devInfo.KreonFeatures.HasFlag(KreonFeatures.DecryptSs);
                KreonXtremeUnlock      = devInfo.KreonFeatures.HasFlag(KreonFeatures.XtremeUnlock);
                KreonWxripperUnlock    = devInfo.KreonFeatures.HasFlag(KreonFeatures.WxripperUnlock);

                KreonChallengeResponse360 = devInfo.KreonFeatures.HasFlag(KreonFeatures.ChallengeResponse360);

                KreonDecryptSs360      = devInfo.KreonFeatures.HasFlag(KreonFeatures.DecryptSs360);
                KreonXtremeUnlock360   = devInfo.KreonFeatures.HasFlag(KreonFeatures.XtremeUnlock360);
                KreonWxripperUnlock360 = devInfo.KreonFeatures.HasFlag(KreonFeatures.WxripperUnlock360);
                KreonLock              = devInfo.KreonFeatures.HasFlag(KreonFeatures.Lock);
                KreonErrorSkipping     = devInfo.KreonFeatures.HasFlag(KreonFeatures.ErrorSkipping);
            }

            if(devInfo.BlockLimits != null)
            {
                BlockLimits.BlockLimitsData? blockLimits = Decoders.SCSI.SSC.BlockLimits.Decode(devInfo.BlockLimits);

                if(blockLimits.HasValue)
                {
                    Ssc = true;

                    if(blockLimits.Value.minBlockLen == blockLimits.Value.maxBlockLen)
                        MinBlockSize = $"Device's block size is fixed at {blockLimits.Value.minBlockLen} bytes";
                    else
                    {
                        MaxBlockSize = blockLimits.Value.maxBlockLen > 0
                                           ? $"Device's maximum block size is {blockLimits.Value.maxBlockLen} bytes"
                                           : "Device does not specify a maximum block size";

                        MinBlockSize = $"Device's minimum block size is {blockLimits.Value.minBlockLen} bytes";

                        if(blockLimits.Value.granularity > 0)
                            BlockSizeGranularity = $"Device's needs a block size granularity of 2^{
                                blockLimits.Value.granularity} ({Math.Pow(2, blockLimits.Value.granularity)}) bytes";
                    }
                }
            }

            if(devInfo.DensitySupport != null)
                if(devInfo.DensitySupportHeader.HasValue)
                    Densities = DensitySupport.PrettifyDensity(devInfo.DensitySupportHeader);

            if(devInfo.MediumDensitySupport != null)
            {
                if(devInfo.MediaTypeSupportHeader.HasValue)
                    MediumTypes = DensitySupport.PrettifyMediumType(devInfo.MediaTypeSupportHeader);

                MediumDensity = DensitySupport.PrettifyMediumType(devInfo.MediumDensitySupport);
            }
        }

        SdMmcInfo = new SdMmcInfo
        {
            DataContext = new SdMmcInfoViewModel(devInfo.Type, devInfo.CID, devInfo.CSD, devInfo.OCR,
                                                 devInfo.ExtendedCSD, devInfo.SCR)
        };
    }

    public ReactiveCommand<Unit, Task> SaveUsbDescriptorsCommand { get; }

    public string DeviceType
    {
        get => _deviceType;
        set => this.RaiseAndSetIfChanged(ref _deviceType, value);
    }

    public string Manufacturer
    {
        get => _manufacturer;
        set => this.RaiseAndSetIfChanged(ref _manufacturer, value);
    }

    public string Model
    {
        get => _model;
        set => this.RaiseAndSetIfChanged(ref _model, value);
    }

    public string Revision
    {
        get => _revision;
        set => this.RaiseAndSetIfChanged(ref _revision, value);
    }

    public string Serial
    {
        get => _serial;
        set => this.RaiseAndSetIfChanged(ref _serial, value);
    }

    public string ScsiType
    {
        get => _scsiType;
        set => this.RaiseAndSetIfChanged(ref _scsiType, value);
    }

    public bool Removable
    {
        get => _removable;
        set => this.RaiseAndSetIfChanged(ref _removable, value);
    }

    public string UsbConnected
    {
        get => _usbConnected;
        set => this.RaiseAndSetIfChanged(ref _usbConnected, value);
    }

    public bool UsbVisible
    {
        get => _usbVisible;
        set => this.RaiseAndSetIfChanged(ref _usbVisible, value);
    }

    public string UsbVendorId
    {
        get => _usbVendorId;
        set => this.RaiseAndSetIfChanged(ref _usbVendorId, value);
    }

    public string UsbProductId
    {
        get => _usbProductId;
        set => this.RaiseAndSetIfChanged(ref _usbProductId, value);
    }

    public string UsbManufacturer
    {
        get => _usbManufacturer;
        set => this.RaiseAndSetIfChanged(ref _usbManufacturer, value);
    }

    public string UsbProduct
    {
        get => _usbProduct;
        set => this.RaiseAndSetIfChanged(ref _usbProduct, value);
    }

    public string UsbSerial
    {
        get => _usbSerial;
        set => this.RaiseAndSetIfChanged(ref _usbSerial, value);
    }

    public bool SaveUsbDescriptorsEnabled
    {
        get => _saveUsbDescriptorsEnabled;
        set => this.RaiseAndSetIfChanged(ref _saveUsbDescriptorsEnabled, value);
    }

    public bool FirewireVisible
    {
        get => _firewireVisible;
        set => this.RaiseAndSetIfChanged(ref _firewireVisible, value);
    }

    public string FirewireVendorId
    {
        get => _firewireVendorId;
        set => this.RaiseAndSetIfChanged(ref _firewireVendorId, value);
    }

    public string FirewireModelId
    {
        get => _firewireModelId;
        set => this.RaiseAndSetIfChanged(ref _firewireModelId, value);
    }

    public string FirewireManufacturer
    {
        get => _firewireManufacturer;
        set => this.RaiseAndSetIfChanged(ref _firewireManufacturer, value);
    }

    public string FirewireModel
    {
        get => _firewireModel;
        set => this.RaiseAndSetIfChanged(ref _firewireModel, value);
    }

    public string FirewireGuid
    {
        get => _firewireGuid;
        set => this.RaiseAndSetIfChanged(ref _firewireGuid, value);
    }

    public bool PlextorVisible
    {
        get => _plextorVisible;
        set => this.RaiseAndSetIfChanged(ref _plextorVisible, value);
    }

    public bool PlextorEepromVisible
    {
        get => _plextorEepromVisible;
        set => this.RaiseAndSetIfChanged(ref _plextorEepromVisible, value);
    }

    public string PlextorDiscs
    {
        get => _plextorDiscs;
        set => this.RaiseAndSetIfChanged(ref _plextorDiscs, value);
    }

    public string PlextorCdReadTime
    {
        get => _plextorCdReadTime;
        set => this.RaiseAndSetIfChanged(ref _plextorCdReadTime, value);
    }

    public string PlextorCdWriteTime
    {
        get => _plextorCdWriteTime;
        set => this.RaiseAndSetIfChanged(ref _plextorCdWriteTime, value);
    }

    public bool PlextorDvdTimesVisible
    {
        get => _plextorDvdTimesVisible;
        set => this.RaiseAndSetIfChanged(ref _plextorDvdTimesVisible, value);
    }

    public string PlextorDvdReadTime
    {
        get => _plextorDvdReadTime;
        set => this.RaiseAndSetIfChanged(ref _plextorDvdReadTime, value);
    }

    public string PlextorDvdWriteTime
    {
        get => _plextorDvdWriteTime;
        set => this.RaiseAndSetIfChanged(ref _plextorDvdWriteTime, value);
    }

    public bool PlextorPoweRec
    {
        get => _plextorPoweRec;
        set => this.RaiseAndSetIfChanged(ref _plextorPoweRec, value);
    }

    public bool PlextorPoweRecEnabled
    {
        get => _plextorPoweRecEnabled;
        set => this.RaiseAndSetIfChanged(ref _plextorPoweRecEnabled, value);
    }

    public bool PlextorPoweRecRecommendedVisible
    {
        get => _plextorPoweRecRecommendedVisible;
        set => this.RaiseAndSetIfChanged(ref _plextorPoweRecRecommendedVisible, value);
    }

    public string PlextorPoweRecRecommended
    {
        get => _plextorPoweRecRecommended;
        set => this.RaiseAndSetIfChanged(ref _plextorPoweRecRecommended, value);
    }

    public bool PlextorPoweRecSelectedVisible
    {
        get => _plextorPoweRecSelectedVisible;
        set => this.RaiseAndSetIfChanged(ref _plextorPoweRecSelectedVisible, value);
    }

    public string PlextorPoweRecSelected
    {
        get => _plextorPoweRecSelected;
        set => this.RaiseAndSetIfChanged(ref _plextorPoweRecSelected, value);
    }

    public bool PlextorPoweRecMaxVisible
    {
        get => _plextorPoweRecMaxVisible;
        set => this.RaiseAndSetIfChanged(ref _plextorPoweRecMaxVisible, value);
    }

    public string PlextorPoweRecMax
    {
        get => _plextorPoweRecMax;
        set => this.RaiseAndSetIfChanged(ref _plextorPoweRecMax, value);
    }

    public bool PlextorPoweRecLastVisible
    {
        get => _plextorPoweRecLastVisible;
        set => this.RaiseAndSetIfChanged(ref _plextorPoweRecLastVisible, value);
    }

    public string PlextorPoweRecLast
    {
        get => _plextorPoweRecLast;
        set => this.RaiseAndSetIfChanged(ref _plextorPoweRecLast, value);
    }

    public bool PlextorSilentMode
    {
        get => _plextorSilentMode;
        set => this.RaiseAndSetIfChanged(ref _plextorSilentMode, value);
    }

    public bool PlextorSilentModeEnabled
    {
        get => _plextorSilentModeEnabled;
        set => this.RaiseAndSetIfChanged(ref _plextorSilentModeEnabled, value);
    }

    public string PlextorSilentModeAccessTime
    {
        get => _plextorSilentModeAccessTime;
        set => this.RaiseAndSetIfChanged(ref _plextorSilentModeAccessTime, value);
    }

    public string PlextorSilentModeCdReadSpeedLimit
    {
        get => _plextorSilentModeCdReadSpeedLimit;
        set => this.RaiseAndSetIfChanged(ref _plextorSilentModeCdReadSpeedLimit, value);
    }

    public string PlextorSilentModeCdWriteSpeedLimit
    {
        get => _plextorSilentModeCdWriteSpeedLimit;
        set => this.RaiseAndSetIfChanged(ref _plextorSilentModeCdWriteSpeedLimit, value);
    }

    public bool PlextorSilentModeDvdReadSpeedLimitVisible
    {
        get => _plextorSilentModeDvdReadSpeedLimitVisible;
        set => this.RaiseAndSetIfChanged(ref _plextorSilentModeDvdReadSpeedLimitVisible, value);
    }

    public string PlextorSilentModeDvdReadSpeedLimit
    {
        get => _plextorSilentModeDvdReadSpeedLimit;
        set => this.RaiseAndSetIfChanged(ref _plextorSilentModeDvdReadSpeedLimit, value);
    }

    public bool PlextorGigaRec
    {
        get => _plextorGigaRec;
        set => this.RaiseAndSetIfChanged(ref _plextorGigaRec, value);
    }

    public bool PlextorSecuRec
    {
        get => _plextorSecuRec;
        set => this.RaiseAndSetIfChanged(ref _plextorSecuRec, value);
    }

    public bool PlextorSpeedRead
    {
        get => _plextorSpeedRead;
        set => this.RaiseAndSetIfChanged(ref _plextorSpeedRead, value);
    }

    public bool PlextorSpeedEnabled
    {
        get => _plextorSpeedEnabled;
        set => this.RaiseAndSetIfChanged(ref _plextorSpeedEnabled, value);
    }

    public bool PlextorHiding
    {
        get => _plextorHiding;
        set => this.RaiseAndSetIfChanged(ref _plextorHiding, value);
    }

    public bool PlextorHidesRecordables
    {
        get => _plextorHidesRecordables;
        set => this.RaiseAndSetIfChanged(ref _plextorHidesRecordables, value);
    }

    public bool PlextorHidesSessions
    {
        get => _plextorHidesSessions;
        set => this.RaiseAndSetIfChanged(ref _plextorHidesSessions, value);
    }

    public bool PlextorVariRec
    {
        get => _plextorVariRec;
        set => this.RaiseAndSetIfChanged(ref _plextorVariRec, value);
    }

    public string PlextorDvd
    {
        get => _plextorDvd;
        set => this.RaiseAndSetIfChanged(ref _plextorDvd, value);
    }

    public bool PlextorVariRecDvd
    {
        get => _plextorVariRecDvd;
        set => this.RaiseAndSetIfChanged(ref _plextorVariRecDvd, value);
    }

    public bool PlextorBitSetting
    {
        get => _plextorBitSetting;
        set => this.RaiseAndSetIfChanged(ref _plextorBitSetting, value);
    }

    public bool PlextorBitSettingDl
    {
        get => _plextorBitSettingDl;
        set => this.RaiseAndSetIfChanged(ref _plextorBitSettingDl, value);
    }

    public bool PlextorDvdPlusWriteTest
    {
        get => _plextorDvdPlusWriteTest;
        set => this.RaiseAndSetIfChanged(ref _plextorDvdPlusWriteTest, value);
    }

    public bool Kreon
    {
        get => _kreon;
        set => this.RaiseAndSetIfChanged(ref _kreon, value);
    }

    public bool KreonChallengeResponse
    {
        get => _kreonChallengeResponse;
        set => this.RaiseAndSetIfChanged(ref _kreonChallengeResponse, value);
    }

    public bool KreonDecryptSs
    {
        get => _kreonDecryptSs;
        set => this.RaiseAndSetIfChanged(ref _kreonDecryptSs, value);
    }

    public bool KreonXtremeUnlock
    {
        get => _kreonXtremeUnlock;
        set => this.RaiseAndSetIfChanged(ref _kreonXtremeUnlock, value);
    }

    public bool KreonWxripperUnlock
    {
        get => _kreonWxripperUnlock;
        set => this.RaiseAndSetIfChanged(ref _kreonWxripperUnlock, value);
    }

    public bool KreonChallengeResponse360
    {
        get => _kreonChallengeResponse360;
        set => this.RaiseAndSetIfChanged(ref _kreonChallengeResponse360, value);
    }

    public bool KreonDecryptSs360
    {
        get => _kreonDecryptSs360;
        set => this.RaiseAndSetIfChanged(ref _kreonDecryptSs360, value);
    }

    public bool KreonXtremeUnlock360
    {
        get => _kreonXtremeUnlock360;
        set => this.RaiseAndSetIfChanged(ref _kreonXtremeUnlock360, value);
    }

    public bool KreonWxripperUnlock360
    {
        get => _kreonWxripperUnlock360;
        set => this.RaiseAndSetIfChanged(ref _kreonWxripperUnlock360, value);
    }

    public bool KreonLock
    {
        get => _kreonLock;
        set => this.RaiseAndSetIfChanged(ref _kreonLock, value);
    }

    public bool KreonErrorSkipping
    {
        get => _kreonErrorSkipping;
        set => this.RaiseAndSetIfChanged(ref _kreonErrorSkipping, value);
    }

    public bool Ssc
    {
        get => _ssc;
        set => this.RaiseAndSetIfChanged(ref _ssc, value);
    }

    public string BlockLimits
    {
        get => _blockLimits;
        set => this.RaiseAndSetIfChanged(ref _blockLimits, value);
    }

    public string MinBlockSize
    {
        get => _minBlockSize;
        set => this.RaiseAndSetIfChanged(ref _minBlockSize, value);
    }

    public string MaxBlockSize
    {
        get => _maxBlockSize;
        set => this.RaiseAndSetIfChanged(ref _maxBlockSize, value);
    }

    public string BlockSizeGranularity
    {
        get => _blockSizeGranularity;
        set => this.RaiseAndSetIfChanged(ref _blockSizeGranularity, value);
    }

    public string Densities
    {
        get => _densities;
        set => this.RaiseAndSetIfChanged(ref _densities, value);
    }

    public string MediumTypes
    {
        get => _mediumTypes;
        set => this.RaiseAndSetIfChanged(ref _mediumTypes, value);
    }

    public string MediumDensity
    {
        get => _mediumDensity;
        set => this.RaiseAndSetIfChanged(ref _mediumDensity, value);
    }

    public string SecureDigital
    {
        get => _secureDigital;
        set => this.RaiseAndSetIfChanged(ref _secureDigital, value);
    }

    public string SdMm
    {
        get => _sdMm;
        set => this.RaiseAndSetIfChanged(ref _sdMm, value);
    }

    public string Cid
    {
        get => _cid;
        set => this.RaiseAndSetIfChanged(ref _cid, value);
    }

    public string Csd
    {
        get => _csd;
        set => this.RaiseAndSetIfChanged(ref _csd, value);
    }

    public string Ocr
    {
        get => _ocr;
        set => this.RaiseAndSetIfChanged(ref _ocr, value);
    }

    public string ExtendedCsd
    {
        get => _extendedCsd;
        set => this.RaiseAndSetIfChanged(ref _extendedCsd, value);
    }

    public string Scr
    {
        get => _scr;
        set => this.RaiseAndSetIfChanged(ref _scr, value);
    }

    public PcmciaInfo PcmciaInfo
    {
        get => _pcmciaInfo;
        set => this.RaiseAndSetIfChanged(ref _pcmciaInfo, value);
    }

    public ScsiInfo ScsiInfo
    {
        get => _scsiInfo;
        set => this.RaiseAndSetIfChanged(ref _scsiInfo, value);
    }

    public AtaInfo AtaInfo
    {
        get => _ataInfo;
        set => this.RaiseAndSetIfChanged(ref _ataInfo, value);
    }

    public SdMmcInfo SdMmcInfo
    {
        get => _sdMmcInfo;
        set => this.RaiseAndSetIfChanged(ref _sdMmcInfo, value);
    }

    public string DeviceInformationLabel                  => "Device information";
    public string GeneralLabel                            => "General";
    public string DeviceTypeLabel                         => "Device type";
    public string ManufacturerLabel                       => "Manufacturer";
    public string ModelLabel                              => "Model";
    public string RevisionLabel                           => "Revision";
    public string SerialNumberLabel                       => "Serial number";
    public string ScsiTypeLabel                           => "Peripheral device type";
    public string RemovableMediaLabel                     => "Removable media";
    public string UsbConnectedLabel                       => "Connected by USB";
    public string USBLabel                                => "USB";
    public string VendorIDLabel                           => "Vendor ID";
    public string ProductIDLabel                          => "Product ID";
    public string ProductLabel                            => "Product";
    public string SaveUsbDescriptorsLabel                 => "Save descriptors to file";
    public string FireWireLabel                           => "FireWire";
    public string ModelIDLabel                            => "Model ID";
    public string GUIDLabel                               => "GUID";
    public string PlextorLabel                            => "Plextor";
    public string PlextorDiscsLabel                       => "Total loaded discs:";
    public string PlextorCdReadTimeLabel                  => "Time spent reading CDs";
    public string PlextorCdWriteTimeLabel                 => "Time spent writing CDs";
    public string PlextorDvdReadTimeLabel                 => "Time spent reading DVDs";
    public string PlextorDvdWriteTimeLabel                => "Time spent writing DVDs";
    public string PlextorPoweRecLabel                     => "Supports PoweRec";
    public string PlextorPoweRecEnabledLabel              => "PoweRec is enabled";
    public string PlextorPoweRecRecommendedLabel          => "Recommended speed";
    public string PlextorPoweRecSelectedLabel             => "Selected PoweRec speed for currently inserted media:";
    public string PlextorPoweRecMaxLabel                  => "Maximum PoweRec speed for currently inserted media:";
    public string PlextorPoweRecLastLabel                 => "Last PoweRec used speed";
    public string PlextorSilentModeLabel                  => "Supports SilentMode";
    public string PlextorSilentModeEnabledLabel           => "SilentMode is enabled";
    public string PlextorSilentModeCdReadSpeedLimitLabel  => "CD read speed limited to";
    public string PlextorSilentModeCdWriteSpeedLimitLabel => "CD write speed limited to";
    public string PlextorSilentModeDvdReadSpeedLimitLabel => "DVD read speed limited to";
    public string PlextorGigaRecLabel                     => "Supports GigaRec";
    public string PlextorSecuRecLabel                     => "Supports SecuRec";
    public string PlextorSpeedReadLabel                   => "Supports SpeedRead";
    public string PlextorSpeedEnabledLabel                => "SpeedRead is enabled";
    public string PlextorHidingLabel                      => "Supports hiding CD-Rs and sessions";
    public string PlextorHidesRecordablesLabel            => "Is hiding CD-Rs";
    public string PlextorHidesSessionsLabel               => "Is forcing only first session";
    public string PlextorVariRecLabel                     => "Supports VariRec";
    public string PlextorVariRecDvdLabel                  => "Supports VariRec on DVDs";
    public string PlextorBitSettingLabel                  => "Supports bitsetting DVD+R book type";
    public string PlextorBitSettingDlLabel                => "Supports bitsetting DVD+R DL book type";
    public string PlextorDvdPlusWriteTestLabel            => "Supports test writing DVD+";
    public string KreonLabel                              => "Kreon";
    public string KreonChallengeResponseLabel             => "Can do challenge/response with Xbox discs";
    public string KreonDecryptSsLabel                     => "Can read and decrypt SS from Xbox discs";
    public string KreonXtremeUnlockLabel                  => "Can set xtreme unlock state with Xbox discs";
    public string KreonWxripperUnlockLabel                => "Can set wxripper unlock state with Xbox discs";
    public string KreonChallengeResponse360Label          => "Can do challenge/response with Xbox 360 discs";
    public string KreonDecryptSs360Label                  => "Can read and decrypt SS from Xbox 360 discs";
    public string KreonXtremeUnlock360Label               => "Can set xtreme unlock state with Xbox 360 discs";
    public string KreonWxripperUnlock360Label             => "Can set wxripper unlock state with Xbox 360 discs";
    public string KreonSetLockedLabel                     => "Can set locked state";
    public string KreonErrorSkippingLabel                 => "Can skip read errors";
    public string DensitiesSupportedByDeviceLabel         => "Densities supported by device:";
    public string MediumTypesSupportedByDeviceLabel       => "Medium types supported by device:";
    public string CIDLabel                                => "CID";
    public string CSDLabel                                => "CSD";
    public string OCRLabel                                => "OCR";
    public string ExtendedCSDLabel                        => "Extended CSD";
    public string SCRLabel                                => "SCR";
    public string PCMCIALabel                             => "PCMCIA";
    public string ATA_ATAPILabel                          => "ATA/ATAPI";
    public string SCSILabel                               => "SCSI";
    public string SD_MMCLabel                             => "SD/MMC";

    async Task ExecuteSaveUsbDescriptorsCommand()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters?.Add(new FileDialogFilter
        {
            Extensions = new List<string>(new[]
            {
                "*.bin"
            }),
            Name = "Binary"
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null)
            return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(_devInfo.UsbDescriptors, 0, _devInfo.UsbDescriptors.Length);

        saveFs.Close();
    }
}