// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DeviceViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model for device.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Core;
using Aaru.Decoders.SCSI.SSC;
using Aaru.Devices;
using Aaru.Gui.ViewModels.Tabs;
using Aaru.Gui.Views.Tabs;
using Aaru.Gui.Views.Windows;
using Aaru.Localization;
using Aaru.Logging;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Svg;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;
using DeviceInfo = Aaru.Core.Devices.Info.DeviceInfo;
using ScsiInfo = Aaru.Core.Media.Info.ScsiInfo;

namespace Aaru.Gui.ViewModels.Windows;

public partial class DeviceViewModel : ViewModelBase
{
    readonly DeviceView _window;
    [ObservableProperty]
    AtaInfo _ataInfo;
    [ObservableProperty]
    bool _blockLimits;
    [ObservableProperty]
    string _blockSizeGranularity;
    [ObservableProperty]
    BlurayInfo _blurayInfo;
    [ObservableProperty]
    CompactDiscInfo _compactDiscInfo;
    [ObservableProperty]
    string _densities;
    Device _dev;
    [ObservableProperty]
    string _devicePath;
    [ObservableProperty]
    string _deviceType;
    DeviceInfo _devInfo;
    [ObservableProperty]
    DvdInfo _dvdInfo;
    [ObservableProperty]
    DvdWritableInfo _dvdWritableInfo;
    [ObservableProperty]
    string _firewireGuid;
    [ObservableProperty]
    string _firewireManufacturer;
    [ObservableProperty]
    string _firewireModel;
    [ObservableProperty]
    string _firewireModelId;
    [ObservableProperty]
    string _firewireVendorId;
    [ObservableProperty]
    bool _firewireVisible;
    [ObservableProperty]
    bool _kreon;
    [ObservableProperty]
    bool _kreonChallengeResponse;
    [ObservableProperty]
    bool _kreonChallengeResponse360;
    [ObservableProperty]
    bool _kreonDecryptSs;
    [ObservableProperty]
    bool _kreonDecryptSs360;
    [ObservableProperty]
    bool _kreonErrorSkipping;
    [ObservableProperty]
    bool _kreonLock;
    [ObservableProperty]
    bool _kreonWxripperUnlock;
    [ObservableProperty]
    bool _kreonWxripperUnlock360;
    [ObservableProperty]
    bool _kreonXtremeUnlock;
    [ObservableProperty]
    bool _kreonXtremeUnlock360;
    [ObservableProperty]
    string _manufacturer;
    [ObservableProperty]
    string _maxBlockSize;
    [ObservableProperty]
    bool _mediaHasInformation;
    ScsiInfo _mediaInfo;
    [ObservableProperty]
    bool _mediaIsInserted;
    [ObservableProperty]
    IImage _mediaLogo;
    [ObservableProperty]
    string _mediaSerial;
    [ObservableProperty]
    string _mediaSize;
    [ObservableProperty]
    string _mediaType;
    [ObservableProperty]
    string _mediumDensity;
    [ObservableProperty]
    string _mediumTypes;
    [ObservableProperty]
    string _minBlockSize;
    [ObservableProperty]
    bool _mmcVisible;
    [ObservableProperty]
    string _model;
    [ObservableProperty]
    PcmciaInfo _pcmciaInfo;
    [ObservableProperty]
    bool _plextorBitSetting;
    [ObservableProperty]
    bool _plextorBitSettingDl;
    [ObservableProperty]
    string _plextorCdReadTime;
    [ObservableProperty]
    string _plextorCdWriteTime;
    [ObservableProperty]
    string _plextorDiscs;
    [ObservableProperty]
    bool _plextorDvd;
    [ObservableProperty]
    bool _plextorDvdPlusWriteTest;
    [ObservableProperty]
    string _plextorDvdReadTime;
    [ObservableProperty]
    bool _plextorDvdTimesVisible;
    [ObservableProperty]
    string _plextorDvdWriteTime;
    [ObservableProperty]
    bool _plextorEepromVisible;
    [ObservableProperty]
    bool _plextorGigaRec;
    [ObservableProperty]
    bool _plextorHidesRecordables;
    [ObservableProperty]
    bool _plextorHidesSessions;
    [ObservableProperty]
    bool _plextorHiding;
    [ObservableProperty]
    bool _plextorPoweRec;
    [ObservableProperty]
    bool _plextorPoweRecEnabled;
    [ObservableProperty]
    string _plextorPoweRecLast;
    [ObservableProperty]
    bool _plextorPoweRecLastVisible;
    [ObservableProperty]
    string _plextorPoweRecMax;
    [ObservableProperty]
    bool _plextorPoweRecMaxVisible;
    [ObservableProperty]
    string _plextorPoweRecRecommended;
    [ObservableProperty]
    bool _plextorPoweRecRecommendedVisible;
    [ObservableProperty]
    string _plextorPoweRecSelected;
    [ObservableProperty]
    bool _plextorPoweRecSelectedVisible;
    [ObservableProperty]
    bool _plextorSecuRec;
    [ObservableProperty]
    bool _plextorSilentMode;
    [ObservableProperty]
    string _plextorSilentModeAccessTime;
    [ObservableProperty]
    string _plextorSilentModeCdReadSpeedLimit;
    [ObservableProperty]
    string _plextorSilentModeCdWriteSpeedLimit;
    [ObservableProperty]
    string _plextorSilentModeDvdReadSpeedLimit;
    [ObservableProperty]
    bool _plextorSilentModeDvdReadSpeedLimitVisible;
    [ObservableProperty]
    bool _plextorSilentModeEnabled;
    [ObservableProperty]
    bool _plextorSpeedEnabled;
    [ObservableProperty]
    bool _plextorSpeedRead;
    [ObservableProperty]
    bool _plextorVariRec;
    [ObservableProperty]
    bool _plextorVariRecDvd;
    [ObservableProperty]
    bool _plextorVisible;
    [ObservableProperty]
    bool _removableChecked;
    [ObservableProperty]
    string _revision;
    [ObservableProperty]
    bool _saveGetConfigurationVisible;
    [ObservableProperty]
    bool _saveReadCapacity16Visible;
    [ObservableProperty]
    bool _saveReadCapacityVisible;
    [ObservableProperty]
    bool _saveReadMediaSerialVisible;
    [ObservableProperty]
    bool _saveRecognizedFormatLayersVisible;
    [ObservableProperty]
    bool _saveUsbDescriptorsEnabled;
    [ObservableProperty]
    bool _saveWriteProtectionStatusVisible;
    [ObservableProperty]
    Views.Tabs.ScsiInfo _scsiInfo;
    [ObservableProperty]
    string _scsiType;
    [ObservableProperty]
    SdMmcInfo _sdMmcInfo;
    [ObservableProperty]
    string _serial;
    [ObservableProperty]
    bool _ssc;
    [ObservableProperty]
    string _statusMessage;
    [ObservableProperty]
    bool _statusMessageVisible;
    [ObservableProperty]
    bool _usbConnected;
    byte[] _usbDescriptors;
    [ObservableProperty]
    string _usbManufacturer;
    [ObservableProperty]
    string _usbProduct;
    [ObservableProperty]
    string _usbProductId;
    [ObservableProperty]
    string _usbSerial;
    [ObservableProperty]
    string _usbVendorId;
    [ObservableProperty]
    bool _usbVisible;
    [ObservableProperty]
    XboxInfo _xboxInfo;

    public DeviceViewModel(DeviceView window, string devicePath)
    {
        _window    = window;
        DevicePath = devicePath;

        SaveUsbDescriptorsCommand         = new AsyncRelayCommand(SaveUsbDescriptorsAsync);
        SaveReadMediaSerialCommand        = new AsyncRelayCommand(SaveReadMediaSerialAsync);
        SaveReadCapacityCommand           = new AsyncRelayCommand(SaveReadCapacityAsync);
        SaveReadCapacity16Command         = new AsyncRelayCommand(SaveReadCapacity16Async);
        SaveGetConfigurationCommand       = new AsyncRelayCommand(SaveGetConfigurationAsync);
        SaveRecognizedFormatLayersCommand = new AsyncRelayCommand(SaveRecognizedFormatLayersAsync);
        SaveWriteProtectionStatusCommand  = new AsyncRelayCommand(SaveWriteProtectionStatusAsync);
        DumpCommand                       = new AsyncRelayCommand(DumpAsync);
        ScanCommand                       = new AsyncRelayCommand(ScanAsync);
    }

    public ICommand SaveUsbDescriptorsCommand         { get; }
    public ICommand SaveReadMediaSerialCommand        { get; }
    public ICommand SaveReadCapacityCommand           { get; }
    public ICommand SaveReadCapacity16Command         { get; }
    public ICommand SaveGetConfigurationCommand       { get; }
    public ICommand SaveRecognizedFormatLayersCommand { get; }
    public ICommand SaveWriteProtectionStatusCommand  { get; }
    public ICommand DumpCommand                       { get; }
    public ICommand ScanCommand                       { get; }

    Task SaveReadMediaSerialAsync() => SaveElementAsync(_mediaInfo.MediaSerialNumber);

    Task SaveReadCapacityAsync() => SaveElementAsync(_mediaInfo.ReadCapacity);

    Task SaveReadCapacity16Async() => SaveElementAsync(_mediaInfo.ReadCapacity16);

    Task SaveGetConfigurationAsync() => SaveElementAsync(_mediaInfo.MmcConfiguration);

    Task SaveRecognizedFormatLayersAsync() => SaveElementAsync(_mediaInfo.RecognizedFormatLayers);

    Task SaveWriteProtectionStatusAsync() => SaveElementAsync(_mediaInfo.WriteProtectionStatus);

    async Task SaveElementAsync(byte[] data)
    {
        IStorageFile result = await _window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = [FilePickerFileTypes.Binary]
        });

        if(result is null) return;

        var saveFs = new FileStream(result.Path.AbsolutePath, FileMode.Create);
        await saveFs.WriteAsync(data, 0, data.Length);

        saveFs.Close();
    }

    public void LoadData()
    {
        _ = Task.Run(Worker);
    }

    async Task SaveUsbDescriptorsAsync()
    {
        IStorageFile result = await _window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = [FilePickerFileTypes.Binary]
        });

        if(result is null) return;

        var saveFs = new FileStream(result.Path.AbsolutePath, FileMode.Create);
        await saveFs.WriteAsync(_usbDescriptors, 0, _usbDescriptors.Length);

        saveFs.Close();
    }

    void Worker()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            StatusMessageVisible = true;
            StatusMessage        = UI.Opening_device;
        });

        var dev = Device.Create(DevicePath, out ErrorNumber devErrno);

        switch(dev)
        {
            case null:
                if(OperatingSystem.IsWindows() && (int)devErrno == 5)
                {
                    AaruLogging.Error(UI.Insufficient_permissions_trying_to_open_device);

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        IMsBox<ButtonResult> msbox = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                            UI.You_do_not_have_enough_privileges_to_access_the_device,
                            ButtonEnum.Ok,
                            Icon.Error);

                        _ = msbox.ShowAsync();

                        _window.Close();
                    });
                }
                else
                {
                    AaruLogging.Error(string.Format(UI.Could_not_open_device_error_0, devErrno));

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        IMsBox<ButtonResult> msbox = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                            string.Format(UI.Could_not_open_device_error_0, devErrno),
                            ButtonEnum.Ok,
                            Icon.Error);

                        _ = msbox.ShowAsync();

                        _window.Close();
                    });
                }

                return;
            case Devices.Remote.Device remoteDev:
                Statistics.AddRemote(remoteDev.RemoteApplication,
                                     remoteDev.RemoteVersion,
                                     remoteDev.RemoteOperatingSystem,
                                     remoteDev.RemoteOperatingSystemVersion,
                                     remoteDev.RemoteArchitecture);

                break;
        }

        if(dev.Error)
        {
            AaruLogging.Error(Error.Print(dev.LastError));

            Dispatcher.UIThread.Invoke(() =>
            {
                IMsBox<ButtonResult> msbox = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                    Error.Print(dev.LastError),
                    ButtonEnum.Ok,
                    Icon.Error);

                _ = msbox.ShowAsync();

                _window.Close();
            });

            return;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            DeviceType       = $"[rosybrown]{dev.Type.Humanize()}[/]";
            Manufacturer     = (dev.Manufacturer     != null ? $"[blue]{dev.Manufacturer}[/]" : null)!;
            Model            = (dev.Model            != null ? $"[purple]{dev.Model}[/]" : null)!;
            Revision         = (dev.FirmwareRevision != null ? $"[teal]{dev.FirmwareRevision}[/]" : null)!;
            Serial           = (dev.Serial           != null ? $"[fuchsia]{dev.Serial}[/]" : null)!;
            ScsiType         = $"[orange]{dev.ScsiType.Humanize()}[/]";
            RemovableChecked = dev.IsRemovable;
            UsbConnected     = dev.IsUsb;
            StatusMessage    = UI.Device_opened_successfully;
        });

        _dev = dev;

        Statistics.AddDevice(dev);

        Dispatcher.UIThread.Invoke(() => StatusMessage = UI.Querying_device_information);

        var devInfo = new DeviceInfo(dev);
        Statistics.AddCommand("device-info");

        _devInfo = devInfo;

        Dispatcher.UIThread.Invoke(() => StatusMessage = UI.Device_information_queryied_successfully);

        if(devInfo.IsUsb)
        {
            _usbDescriptors = devInfo.UsbDescriptors;

            Dispatcher.UIThread.Invoke(() =>
            {
                UsbVisible                = true;
                SaveUsbDescriptorsEnabled = devInfo.UsbDescriptors != null;
                UsbVendorId               = $"[cyan]{devInfo.UsbVendorId:X4}[/]";
                UsbProductId              = $"[cyan]{devInfo.UsbProductId:X4}[/]";
                UsbManufacturer           = $"[blue]{devInfo.UsbManufacturerString}[/]";
                UsbProduct                = $"[purple]{devInfo.UsbProductString}[/]";
                UsbSerial                 = $"[fuchsia]{devInfo.UsbSerialString}[/]";
            });
        }

        if(devInfo.IsFireWire)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                FirewireVisible      = true;
                FirewireVendorId     = $"[cyan]{devInfo.FireWireVendor:X4}[/]";
                FirewireModelId      = $"[cyan]{devInfo.FireWireModel:X4}[/]";
                FirewireManufacturer = $"[blue]{devInfo.FireWireVendorName}[/]";
                FirewireModel        = $"[purple]{devInfo.FireWireModelName}[/]";
                FirewireGuid         = $"[fuchsia]{devInfo.FireWireGuid:X16}[/]";
            });
        }

        if(devInfo.IsPcmcia)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                PcmciaInfo = new PcmciaInfo
                {
                    DataContext = new PcmciaInfoViewModel(devInfo.Cis, _window)
                };
            });
        }

        if(devInfo.AtaIdentify != null || devInfo.AtapiIdentify != null)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                AtaInfo = new AtaInfo
                {
                    DataContext = new AtaInfoViewModel(devInfo.AtaIdentify,
                                                       devInfo.AtapiIdentify,
                                                       devInfo.AtaMcptError,
                                                       _window)
                };

                if(devInfo.ScsiInquiryData is null) MediaIsInserted = true;
            });
        }

        if(devInfo.ScsiInquiryData != null)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                ScsiInfo = new Views.Tabs.ScsiInfo
                {
                    DataContext = new ScsiInfoViewModel(devInfo.ScsiInquiryData,
                                                        devInfo.ScsiInquiry,
                                                        devInfo.ScsiEvpdPages,
                                                        devInfo.ScsiMode,
                                                        devInfo.ScsiType,
                                                        devInfo.ScsiModeSense6,
                                                        devInfo.ScsiModeSense10,
                                                        devInfo.MmcConfiguration,
                                                        _window)
                };
            });

            if(devInfo.PlextorFeatures != null)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    PlextorVisible = true;

                    if(devInfo.PlextorFeatures.Eeprom != null)
                    {
                        PlextorEepromVisible = true;
                        PlextorDiscs         = $"{devInfo.PlextorFeatures.Discs}";

                        PlextorCdReadTime = devInfo.PlextorFeatures.CdReadTime.Seconds()
                                                   .Humanize(minUnit: TimeUnit.Second);

                        PlextorCdWriteTime = devInfo.PlextorFeatures.CdWriteTime.Seconds()
                                                    .Humanize(minUnit: TimeUnit.Second);

                        if(devInfo.PlextorFeatures.IsDvd)
                        {
                            PlextorDvdTimesVisible = true;

                            PlextorDvdReadTime = devInfo.PlextorFeatures.DvdReadTime.Seconds()
                                                        .Humanize(minUnit: TimeUnit.Second);

                            PlextorDvdWriteTime = devInfo.PlextorFeatures.DvdWriteTime.Seconds()
                                                         .Humanize(minUnit: TimeUnit.Second);
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

                                PlextorPoweRecRecommended =
                                    string.Format(UI._0_Kb_sec, devInfo.PlextorFeatures.PoweRecRecommendedSpeed);
                            }

                            if(devInfo.PlextorFeatures.PoweRecSelected > 0)
                            {
                                PlextorPoweRecSelectedVisible = true;

                                PlextorPoweRecSelected =
                                    string.Format(UI._0_Kb_sec, devInfo.PlextorFeatures.PoweRecSelected);
                            }

                            if(devInfo.PlextorFeatures.PoweRecMax > 0)
                            {
                                PlextorPoweRecMaxVisible = true;

                                PlextorPoweRecMax = string.Format(UI._0_Kb_sec, devInfo.PlextorFeatures.PoweRecMax);
                            }

                            if(devInfo.PlextorFeatures.PoweRecLast > 0)
                            {
                                PlextorPoweRecLastVisible = true;

                                PlextorPoweRecLast = string.Format(UI._0_Kb_sec, devInfo.PlextorFeatures.PoweRecLast);
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
                                                              ? Aaru.Localization.Core.Access_time_is_slow
                                                              : Aaru.Localization.Core.Access_time_is_fast;

                            PlextorSilentModeCdReadSpeedLimit =
                                devInfo.PlextorFeatures.CdReadSpeedLimit > 0
                                    ? $"{devInfo.PlextorFeatures.CdReadSpeedLimit}x"
                                    : UI.unlimited_as_in_speed;

                            PlextorSilentModeCdWriteSpeedLimit =
                                devInfo.PlextorFeatures.CdWriteSpeedLimit > 0
                                    ? $"{devInfo.PlextorFeatures.CdWriteSpeedLimit}x"
                                    : UI.unlimited_as_in_speed;

                            if(devInfo.PlextorFeatures.IsDvd)
                            {
                                PlextorSilentModeDvdReadSpeedLimitVisible = true;

                                PlextorSilentModeDvdReadSpeedLimit =
                                    devInfo.PlextorFeatures.DvdReadSpeedLimit > 0
                                        ? $"{devInfo.PlextorFeatures.DvdReadSpeedLimit}x"
                                        : UI.unlimited_as_in_speed;
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

                    if(!devInfo.PlextorFeatures.IsDvd) return;

                    PlextorVariRecDvd       = devInfo.PlextorFeatures.VariRecDvd;
                    PlextorBitSetting       = devInfo.PlextorFeatures.BitSetting;
                    PlextorBitSettingDl     = devInfo.PlextorFeatures.BitSettingDl;
                    PlextorDvdPlusWriteTest = devInfo.PlextorFeatures.DvdPlusWriteTest;
                });
            }

            if(devInfo.ScsiInquiry?.KreonPresent == true)
            {
                Dispatcher.UIThread.Invoke(() =>
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
                });
            }

            if(devInfo.BlockLimits != null)
            {
                BlockLimits.BlockLimitsData? blockLimits = Decoders.SCSI.SSC.BlockLimits.Decode(devInfo.BlockLimits);

                if(blockLimits.HasValue)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        Ssc = true;

                        if(blockLimits.Value.minBlockLen == blockLimits.Value.maxBlockLen)
                        {
                            MinBlockSize = string.Format(Aaru.Localization.Core.Device_block_size_is_fixed_at_0_bytes,
                                                         blockLimits.Value.minBlockLen);
                        }
                        else
                        {
                            MaxBlockSize = blockLimits.Value.maxBlockLen > 0
                                               ? string.Format(Aaru.Localization.Core
                                                                   .Device_maximum_block_size_is_0_bytes,
                                                               blockLimits.Value.maxBlockLen)
                                               : Aaru.Localization.Core.Device_does_not_specify_a_maximum_block_size;

                            MinBlockSize = string.Format(Aaru.Localization.Core.Device_minimum_block_size_is_0_bytes,
                                                         blockLimits.Value.minBlockLen);

                            if(blockLimits.Value.granularity > 0)
                            {
                                BlockSizeGranularity =
                                    string.Format(Aaru.Localization.Core
                                                      .Device_needs_a_block_size_granularity_of_pow_0_1_bytes,
                                                  blockLimits.Value.granularity,
                                                  Math.Pow(2, blockLimits.Value.granularity));
                            }
                        }
                    });
                }
            }

            if(devInfo.DensitySupport != null)
            {
                if(devInfo.DensitySupportHeader.HasValue)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        Densities =
                            DensitySupport.PrettifyDensity(devInfo.DensitySupportHeader);
                    });
                }
            }

            if(devInfo.MediumDensitySupport != null)
            {
                if(devInfo.MediaTypeSupportHeader.HasValue)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        MediumTypes =
                            DensitySupport
                               .PrettifyMediumType(devInfo.MediaTypeSupportHeader);
                    });
                }

                Dispatcher.UIThread.Invoke(() =>
                {
                    MediumDensity =
                        DensitySupport.PrettifyMediumType(devInfo.MediumDensitySupport);
                });
            }

            Dispatcher.UIThread.Invoke(() => StatusMessage = UI.Querying_media_information);

            var mediaInfo = new ScsiInfo(dev);
            Statistics.AddCommand("media-info");

            if(!mediaInfo.MediaInserted)
            {
                Dispatcher.UIThread.Invoke(() => StatusMessageVisible = false);

                return;
            }

            MediaIsInserted = true;

            Dispatcher.UIThread.Invoke(() =>
            {
                var genericHddIcon = new SvgImage
                {
                    Source =
                        SvgSource.Load(AssetLoader
                                          .Open(new Uri("avares://Aaru.Gui/Assets/Icons/oxygen/drive-harddisk.svg")))
                };

                var genericOpticalIcon = new SvgImage
                {
                    Source =
                        SvgSource.Load(AssetLoader
                                          .Open(new Uri("avares://Aaru.Gui/Assets/Icons/oxygen/drive-optical.svg")))
                };

                var genericFolderIcon = new SvgImage
                {
                    Source =
                        SvgSource.Load(AssetLoader
                                          .Open(new Uri("avares://Aaru.Gui/Assets/Icons/oxygen/inode-directory.svg")))
                };

                // Check if we're using dark theme
                bool isDarkTheme = _window.ActualThemeVariant == ThemeVariant.Dark;

                // Build the appropriate SVG path based on theme
                string svgPath = isDarkTheme
                                     ? $"avares://Aaru.Gui/Assets/Logos/Media/Dark/{mediaInfo.MediaType}.svg"
                                     : $"avares://Aaru.Gui/Assets/Logos/Media/{mediaInfo.MediaType}.svg";

                var mediaResource = new Uri(svgPath);

                // Fallback to light theme version if dark version doesn't exist
                if(isDarkTheme && !AssetLoader.Exists(mediaResource))
                    mediaResource = new Uri($"avares://Aaru.Gui/Assets/Logos/Media/{mediaInfo.MediaType}.svg");

                MediaLogo = AssetLoader.Exists(mediaResource)
                                ? new SvgImage
                                {
                                    Source = SvgSource.Load(AssetLoader.Open(mediaResource))
                                }
                                : dev.ScsiType == PeripheralDeviceTypes.DirectAccess
                                    ? genericHddIcon
                                    : dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice
                                        ? genericOpticalIcon
                                        : genericFolderIcon;

                MediaType = mediaInfo.MediaType.Humanize();
            });

            if(mediaInfo.Blocks != 0 && mediaInfo.BlockSize != 0)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    MediaSize =
                        string.Format(Aaru.Localization.Core
                                          .Media_has_0_blocks_of_1_bytes_each_for_a_total_of_2,
                                      mediaInfo.Blocks,
                                      mediaInfo.BlockSize,
                                      ByteSize.FromBytes(mediaInfo.Blocks *
                                                         mediaInfo.BlockSize)
                                              .ToString("0.000"));
                });
            }

            if(mediaInfo.MediaSerialNumber != null)
            {
                var sbSerial = new StringBuilder();

                for(var i = 4; i < mediaInfo.MediaSerialNumber.Length; i++)
                    sbSerial.Append($"{mediaInfo.MediaSerialNumber[i]:X2}");

                Dispatcher.UIThread.Invoke(() => MediaSerial = sbSerial.ToString());
            }

            Dispatcher.UIThread.Invoke(() =>
            {
                MediaHasInformation = true;

                SaveReadMediaSerialVisible = mediaInfo.MediaSerialNumber != null;
                SaveReadCapacityVisible    = mediaInfo.ReadCapacity      != null;
                SaveReadCapacity16Visible  = mediaInfo.ReadCapacity16    != null;

                SaveGetConfigurationVisible = mediaInfo.MmcConfiguration != null;

                SaveRecognizedFormatLayersVisible = mediaInfo.RecognizedFormatLayers != null;

                SaveWriteProtectionStatusVisible = mediaInfo.WriteProtectionStatus != null;

                MmcVisible = SaveGetConfigurationVisible       ||
                             SaveRecognizedFormatLayersVisible ||
                             SaveWriteProtectionStatusVisible;
            });

            _mediaInfo = mediaInfo;

            Dispatcher.UIThread.Invoke(() =>
            {
                if(_mediaInfo.Toc                    != null ||
                   _mediaInfo.Atip                   != null ||
                   _mediaInfo.DiscInformation        != null ||
                   _mediaInfo.Session                != null ||
                   _mediaInfo.RawToc                 != null ||
                   _mediaInfo.Pma                    != null ||
                   _mediaInfo.CdTextLeadIn           != null ||
                   _mediaInfo.DecodedToc             != null ||
                   _mediaInfo.DecodedAtip            != null ||
                   _mediaInfo.DecodedSession         != null ||
                   _mediaInfo.FullToc                != null ||
                   _mediaInfo.DecodedCdTextLeadIn    != null ||
                   _mediaInfo.DecodedDiscInformation != null ||
                   _mediaInfo.Mcn                    != null ||
                   _mediaInfo.Isrcs                  != null)
                {
                    CompactDiscInfo = new CompactDiscInfo
                    {
                        DataContext = new CompactDiscInfoViewModel(_mediaInfo.Toc,
                                                                   _mediaInfo.Atip,
                                                                   _mediaInfo.DiscInformation,
                                                                   _mediaInfo.Session,
                                                                   _mediaInfo.RawToc,
                                                                   _mediaInfo.Pma,
                                                                   _mediaInfo.CdTextLeadIn,
                                                                   _mediaInfo.DecodedToc,
                                                                   _mediaInfo.DecodedAtip,
                                                                   _mediaInfo.DecodedSession,
                                                                   _mediaInfo.FullToc,
                                                                   _mediaInfo.DecodedCdTextLeadIn,
                                                                   _mediaInfo.DecodedDiscInformation,
                                                                   _mediaInfo.Mcn,
                                                                   _mediaInfo.Isrcs,
                                                                   _window)
                    };
                }

                if(_mediaInfo.DvdPfi                    != null ||
                   _mediaInfo.DvdDmi                    != null ||
                   _mediaInfo.DvdCmi                    != null ||
                   _mediaInfo.HddvdCopyrightInformation != null ||
                   _mediaInfo.DvdBca                    != null ||
                   _mediaInfo.DvdAacs                   != null ||
                   _mediaInfo.DecodedPfi                != null)
                {
                    DvdInfo = new DvdInfo
                    {
                        DataContext = new DvdInfoViewModel(_mediaInfo.DvdPfi,
                                                           _mediaInfo.DvdDmi,
                                                           _mediaInfo.DvdCmi,
                                                           _mediaInfo.HddvdCopyrightInformation,
                                                           _mediaInfo.DvdBca,
                                                           _mediaInfo.DvdAacs,
                                                           _mediaInfo.DecodedPfi,
                                                           _window)
                    };
                }

                if(_mediaInfo.XgdInfo                   != null ||
                   _mediaInfo.XboxSecuritySector        != null ||
                   _mediaInfo.DecodedXboxSecuritySector != null)
                {
                    XboxInfo = new XboxInfo
                    {
                        DataContext = new XboxInfoViewModel(_mediaInfo.XgdInfo,
                                                            _mediaInfo.DvdDmi,
                                                            _mediaInfo.XboxSecuritySector,
                                                            _mediaInfo.DecodedXboxSecuritySector,
                                                            _window)
                    };
                }

                if(_mediaInfo.DvdRamDds                     != null ||
                   _mediaInfo.DvdRamCartridgeStatus         != null ||
                   _mediaInfo.DvdRamSpareArea               != null ||
                   _mediaInfo.LastBorderOutRmd              != null ||
                   _mediaInfo.DvdPreRecordedInfo            != null ||
                   _mediaInfo.DvdrMediaIdentifier           != null ||
                   _mediaInfo.DvdrPhysicalInformation       != null ||
                   _mediaInfo.HddvdrMediumStatus            != null ||
                   _mediaInfo.HddvdrLastRmd                 != null ||
                   _mediaInfo.DvdrLayerCapacity             != null ||
                   _mediaInfo.DvdrDlMiddleZoneStart         != null ||
                   _mediaInfo.DvdrDlJumpIntervalSize        != null ||
                   _mediaInfo.DvdrDlManualLayerJumpStartLba != null ||
                   _mediaInfo.DvdrDlRemapAnchorPoint        != null ||
                   _mediaInfo.DvdPlusAdip                   != null ||
                   _mediaInfo.DvdPlusDcb                    != null)
                {
                    DvdWritableInfo = new DvdWritableInfo
                    {
                        DataContext = new DvdWritableInfoViewModel(_mediaInfo.DvdRamDds,
                                                                   _mediaInfo.DvdRamCartridgeStatus,
                                                                   _mediaInfo.DvdRamSpareArea,
                                                                   _mediaInfo.LastBorderOutRmd,
                                                                   _mediaInfo.DvdPreRecordedInfo,
                                                                   _mediaInfo.DvdrMediaIdentifier,
                                                                   _mediaInfo.DvdrPhysicalInformation,
                                                                   _mediaInfo.HddvdrMediumStatus,
                                                                   _mediaInfo.HddvdrLastRmd,
                                                                   _mediaInfo.DvdrLayerCapacity,
                                                                   _mediaInfo.DvdrDlMiddleZoneStart,
                                                                   _mediaInfo.DvdrDlJumpIntervalSize,
                                                                   _mediaInfo.DvdrDlManualLayerJumpStartLba,
                                                                   _mediaInfo.DvdrDlRemapAnchorPoint,
                                                                   _mediaInfo.DvdPlusAdip,
                                                                   _mediaInfo.DvdPlusDcb,
                                                                   _window)
                    };
                }

                if(_mediaInfo.BlurayDiscInformation      != null ||
                   _mediaInfo.BlurayBurstCuttingArea     != null ||
                   _mediaInfo.BlurayDds                  != null ||
                   _mediaInfo.BlurayCartridgeStatus      != null ||
                   _mediaInfo.BluraySpareAreaInformation != null ||
                   _mediaInfo.BlurayPowResources         != null ||
                   _mediaInfo.BlurayTrackResources       != null ||
                   _mediaInfo.BlurayRawDfl               != null ||
                   _mediaInfo.BlurayPac                  != null)
                {
                    BlurayInfo = new BlurayInfo
                    {
                        DataContext = new BlurayInfoViewModel(_mediaInfo.BlurayDiscInformation,
                                                              _mediaInfo.BlurayBurstCuttingArea,
                                                              _mediaInfo.BlurayDds,
                                                              _mediaInfo.BlurayCartridgeStatus,
                                                              _mediaInfo.BluraySpareAreaInformation,
                                                              _mediaInfo.BlurayPowResources,
                                                              _mediaInfo.BlurayTrackResources,
                                                              _mediaInfo.BlurayRawDfl,
                                                              _mediaInfo.BlurayPac,
                                                              _window)
                    };
                }
            });
        }

        if(devInfo.CID         != null ||
           devInfo.CSD         != null ||
           devInfo.OCR         != null ||
           devInfo.ExtendedCSD != null ||
           devInfo.SCR         != null)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                SdMmcInfo = new SdMmcInfo
                {
                    DataContext = new SdMmcInfoViewModel(devInfo.Type,
                                                         devInfo.CID,
                                                         devInfo.CSD,
                                                         devInfo.OCR,
                                                         devInfo.ExtendedCSD,
                                                         devInfo.SCR)
                };

                MediaIsInserted = true;
            });
        }

        Dispatcher.UIThread.Invoke(() => StatusMessageVisible = false);
    }

    public void Closed()
    {
        _dev?.Close();
    }

    async Task DumpAsync()
    {
        switch(_mediaInfo?.MediaType)
        {
            case CommonTypes.MediaType.GDR or CommonTypes.MediaType.GDROM:
                await MessageBoxManager
                     .GetMessageBoxStandard(UI.Title_Error,
                                            Aaru.Localization.Core.GD_ROM_dump_support_is_not_yet_implemented,
                                            ButtonEnum.Ok,
                                            Icon.Error)
                     .ShowWindowDialogAsync(_window);

                return;
            case CommonTypes.MediaType.XGD or CommonTypes.MediaType.XGD2 or CommonTypes.MediaType.XGD3
                when _mediaInfo.DeviceInfo.ScsiInquiry?.KreonPresent != true:
                await MessageBoxManager
                     .GetMessageBoxStandard(UI.Title_Error,
                                            Aaru.Localization.Core
                                                .Dumping_Xbox_Game_Discs_requires_a_drive_with_Kreon_firmware,
                                            ButtonEnum.Ok,
                                            Icon.Error)
                     .ShowWindowDialogAsync(_window);

                return;
        }

        var mediaDumpWindow = new MediaDump();

        mediaDumpWindow.DataContext = new MediaDumpViewModel(_dev, DevicePath, _devInfo, mediaDumpWindow, _mediaInfo);

        await mediaDumpWindow.ShowDialog(_window);
    }

    async Task ScanAsync()
    {
        switch(_mediaInfo?.MediaType)
        {
            // TODO: GD-ROM
            case CommonTypes.MediaType.GDR:
            case CommonTypes.MediaType.GDROM:
                await MessageBoxManager
                     .GetMessageBoxStandard(UI.Title_Error,
                                            Aaru.Localization.Core.GD_ROM_scan_support_is_not_yet_implemented,
                                            ButtonEnum.Ok,
                                            Icon.Error)
                     .ShowWindowDialogAsync(_window);

                return;

            // TODO: Xbox
            case CommonTypes.MediaType.XGD:
            case CommonTypes.MediaType.XGD2:
            case CommonTypes.MediaType.XGD3:
                await MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                                                              Aaru.Localization.Core
                                                                  .Scanning_Xbox_discs_is_not_yet_supported,
                                                              ButtonEnum.Ok,
                                                              Icon.Error)
                                       .ShowWindowDialogAsync(_window);

                return;
        }

        var mediaScanWindow = new MediaScan();

        mediaScanWindow.DataContext = new MediaScanViewModel(_dev, DevicePath, mediaScanWindow);

        await mediaScanWindow.ShowDialog(_window);
    }
}