// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MediaInfoViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the media information panel.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Aaru.Gui.ViewModels.Tabs;
using Aaru.Gui.ViewModels.Windows;
using Aaru.Gui.Views.Tabs;
using Aaru.Gui.Views.Windows;
using Aaru.Localization;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Humanizer.Bytes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using ScsiInfo = Aaru.Core.Media.Info.ScsiInfo;

namespace Aaru.Gui.ViewModels.Panels;

public sealed class MediaInfoViewModel : ViewModelBase
{
    readonly string   _devicePath;
    readonly ScsiInfo _scsiInfo;
    readonly Window   _view;
    BlurayInfo        _blurayInfo;
    CompactDiscInfo   _compactDiscInfo;
    string            _densitySupport;
    DvdInfo           _dvdInfo;
    DvdWritableInfo   _dvdWritableInfo;
    string            _generalVisible;
    Bitmap            _mediaLogo;
    string            _mediaSerial;
    string            _mediaSize;
    string            _mediaType;
    string            _mediumSupport;
    bool              _mmcVisible;
    bool              _saveDensitySupportVisible;
    bool              _saveGetConfigurationVisible;
    bool              _saveMediumSupportVisible;
    bool              _saveReadCapacity16Visible;
    bool              _saveReadCapacityVisible;
    bool              _saveReadMediaSerialVisible;
    bool              _saveRecognizedFormatLayersVisible;
    bool              _saveWriteProtectionStatusVisible;
    bool              _sscVisible;
    XboxInfo          _xboxInfo;

    public MediaInfoViewModel(ScsiInfo scsiInfo, string devicePath, Window view)
    {
        _view                             = view;
        SaveReadMediaSerialCommand        = ReactiveCommand.Create(ExecuteSaveReadMediaSerialCommand);
        SaveReadCapacityCommand           = ReactiveCommand.Create(ExecuteSaveReadCapacityCommand);
        SaveReadCapacity16Command         = ReactiveCommand.Create(ExecuteSaveReadCapacity16Command);
        SaveGetConfigurationCommand       = ReactiveCommand.Create(ExecuteSaveGetConfigurationCommand);
        SaveRecognizedFormatLayersCommand = ReactiveCommand.Create(ExecuteSaveRecognizedFormatLayersCommand);
        SaveWriteProtectionStatusCommand  = ReactiveCommand.Create(ExecuteSaveWriteProtectionStatusCommand);
        SaveDensitySupportCommand         = ReactiveCommand.Create(ExecuteSaveDensitySupportCommand);
        SaveMediumSupportCommand          = ReactiveCommand.Create(ExecuteSaveMediumSupportCommand);
        DumpCommand                       = ReactiveCommand.Create(ExecuteDumpCommand);
        ScanCommand                       = ReactiveCommand.Create(ExecuteScanCommand);
        _devicePath                       = devicePath;
        _scsiInfo                         = scsiInfo;

        var mediaResource = new Uri($"avares://Aaru.Gui/Assets/Logos/Media/{scsiInfo.MediaType}.png");

        MediaLogo = AssetLoader.Exists(mediaResource) ? new Bitmap(AssetLoader.Open(mediaResource)) : null;

        MediaType = scsiInfo.MediaType.ToString();

        if(scsiInfo.Blocks != 0 && scsiInfo.BlockSize != 0)
        {
            MediaSize = string.Format(Localization.Core.Media_has_0_blocks_of_1_bytes_each_for_a_total_of_2,
                                      scsiInfo.Blocks,
                                      scsiInfo.BlockSize,
                                      ByteSize.FromBytes(scsiInfo.Blocks * scsiInfo.BlockSize).ToString("0.000"));
        }

        if(scsiInfo.MediaSerialNumber != null)
        {
            var sbSerial = new StringBuilder();

            for(var i = 4; i < scsiInfo.MediaSerialNumber.Length; i++)
                sbSerial.Append($"{scsiInfo.MediaSerialNumber[i]:X2}");

            MediaSerial = sbSerial.ToString();
        }

        SaveReadMediaSerialVisible = scsiInfo.MediaSerialNumber != null;
        SaveReadCapacityVisible    = scsiInfo.ReadCapacity      != null;
        SaveReadCapacity16Visible  = scsiInfo.ReadCapacity16    != null;

        SaveGetConfigurationVisible       = scsiInfo.MmcConfiguration       != null;
        SaveRecognizedFormatLayersVisible = scsiInfo.RecognizedFormatLayers != null;
        SaveWriteProtectionStatusVisible  = scsiInfo.WriteProtectionStatus  != null;

        MmcVisible = SaveGetConfigurationVisible       ||
                     SaveRecognizedFormatLayersVisible ||
                     SaveWriteProtectionStatusVisible;

        if(scsiInfo.DensitySupportHeader.HasValue)
            DensitySupport = Decoders.SCSI.SSC.DensitySupport.PrettifyDensity(scsiInfo.DensitySupportHeader);

        if(scsiInfo.MediaTypeSupportHeader.HasValue)
            MediumSupport = Decoders.SCSI.SSC.DensitySupport.PrettifyMediumType(scsiInfo.MediaTypeSupportHeader);

        SaveDensitySupportVisible = scsiInfo.DensitySupport   != null;
        SaveMediumSupportVisible  = scsiInfo.MediaTypeSupport != null;

        SscVisible = SaveDensitySupportVisible || SaveMediumSupportVisible;

        CompactDiscInfo = new CompactDiscInfo
        {
            DataContext = new CompactDiscInfoViewModel(scsiInfo.Toc,
                                                       scsiInfo.Atip,
                                                       scsiInfo.DiscInformation,
                                                       scsiInfo.Session,
                                                       scsiInfo.RawToc,
                                                       scsiInfo.Pma,
                                                       scsiInfo.CdTextLeadIn,
                                                       scsiInfo.DecodedToc,
                                                       scsiInfo.DecodedAtip,
                                                       scsiInfo.DecodedSession,
                                                       scsiInfo.FullToc,
                                                       scsiInfo.DecodedCdTextLeadIn,
                                                       scsiInfo.DecodedDiscInformation,
                                                       scsiInfo.Mcn,
                                                       scsiInfo.Isrcs,
                                                       _view)
        };

        DvdInfo = new DvdInfo
        {
            DataContext = new DvdInfoViewModel(scsiInfo.DvdPfi,
                                               scsiInfo.DvdDmi,
                                               scsiInfo.DvdCmi,
                                               scsiInfo.HddvdCopyrightInformation,
                                               scsiInfo.DvdBca,
                                               scsiInfo.DvdAacs,
                                               scsiInfo.DecodedPfi,
                                               _view)
        };

        XboxInfo = new XboxInfo
        {
            DataContext = new XboxInfoViewModel(scsiInfo.XgdInfo,
                                                scsiInfo.DvdDmi,
                                                scsiInfo.XboxSecuritySector,
                                                scsiInfo.DecodedXboxSecuritySector,
                                                _view)
        };

        DvdWritableInfo = new DvdWritableInfo
        {
            DataContext = new DvdWritableInfoViewModel(scsiInfo.DvdRamDds,
                                                       scsiInfo.DvdRamCartridgeStatus,
                                                       scsiInfo.DvdRamSpareArea,
                                                       scsiInfo.LastBorderOutRmd,
                                                       scsiInfo.DvdPreRecordedInfo,
                                                       scsiInfo.DvdrMediaIdentifier,
                                                       scsiInfo.DvdrPhysicalInformation,
                                                       scsiInfo.HddvdrMediumStatus,
                                                       scsiInfo.HddvdrLastRmd,
                                                       scsiInfo.DvdrLayerCapacity,
                                                       scsiInfo.DvdrDlMiddleZoneStart,
                                                       scsiInfo.DvdrDlJumpIntervalSize,
                                                       scsiInfo.DvdrDlManualLayerJumpStartLba,
                                                       scsiInfo.DvdrDlRemapAnchorPoint,
                                                       scsiInfo.DvdPlusAdip,
                                                       scsiInfo.DvdPlusDcb,
                                                       _view)
        };

        BlurayInfo = new BlurayInfo
        {
            DataContext = new BlurayInfoViewModel(scsiInfo.BlurayDiscInformation,
                                                  scsiInfo.BlurayBurstCuttingArea,
                                                  scsiInfo.BlurayDds,
                                                  scsiInfo.BlurayCartridgeStatus,
                                                  scsiInfo.BluraySpareAreaInformation,
                                                  scsiInfo.BlurayPowResources,
                                                  scsiInfo.BlurayTrackResources,
                                                  scsiInfo.BlurayRawDfl,
                                                  scsiInfo.BlurayPac,
                                                  _view)
        };
    }

    public ReactiveCommand<Unit, Task> SaveReadMediaSerialCommand        { get; }
    public ReactiveCommand<Unit, Task> SaveReadCapacityCommand           { get; }
    public ReactiveCommand<Unit, Task> SaveReadCapacity16Command         { get; }
    public ReactiveCommand<Unit, Task> SaveGetConfigurationCommand       { get; }
    public ReactiveCommand<Unit, Task> SaveRecognizedFormatLayersCommand { get; }
    public ReactiveCommand<Unit, Task> SaveWriteProtectionStatusCommand  { get; }
    public ReactiveCommand<Unit, Task> SaveDensitySupportCommand         { get; }
    public ReactiveCommand<Unit, Task> SaveMediumSupportCommand          { get; }
    public ReactiveCommand<Unit, Task> DumpCommand                       { get; }
    public ReactiveCommand<Unit, Task> ScanCommand                       { get; }

    public Bitmap MediaLogo
    {
        get => _mediaLogo;
        set => this.RaiseAndSetIfChanged(ref _mediaLogo, value);
    }

    public string GeneralVisible
    {
        get => _generalVisible;
        set => this.RaiseAndSetIfChanged(ref _generalVisible, value);
    }

    public string MediaType
    {
        get => _mediaType;
        set => this.RaiseAndSetIfChanged(ref _mediaType, value);
    }

    public string MediaSize
    {
        get => _mediaSize;
        set => this.RaiseAndSetIfChanged(ref _mediaSize, value);
    }

    public string MediaSerial
    {
        get => _mediaSerial;
        set => this.RaiseAndSetIfChanged(ref _mediaSerial, value);
    }

    public bool SaveReadMediaSerialVisible
    {
        get => _saveReadMediaSerialVisible;
        set => this.RaiseAndSetIfChanged(ref _saveReadMediaSerialVisible, value);
    }

    public bool SaveReadCapacityVisible
    {
        get => _saveReadCapacityVisible;
        set => this.RaiseAndSetIfChanged(ref _saveReadCapacityVisible, value);
    }

    public bool SaveReadCapacity16Visible
    {
        get => _saveReadCapacity16Visible;
        set => this.RaiseAndSetIfChanged(ref _saveReadCapacity16Visible, value);
    }

    public bool MmcVisible
    {
        get => _mmcVisible;
        set => this.RaiseAndSetIfChanged(ref _mmcVisible, value);
    }

    public bool SaveGetConfigurationVisible
    {
        get => _saveGetConfigurationVisible;
        set => this.RaiseAndSetIfChanged(ref _saveGetConfigurationVisible, value);
    }

    public bool SaveRecognizedFormatLayersVisible
    {
        get => _saveRecognizedFormatLayersVisible;
        set => this.RaiseAndSetIfChanged(ref _saveRecognizedFormatLayersVisible, value);
    }

    public bool SaveWriteProtectionStatusVisible
    {
        get => _saveWriteProtectionStatusVisible;
        set => this.RaiseAndSetIfChanged(ref _saveWriteProtectionStatusVisible, value);
    }

    public bool SscVisible
    {
        get => _sscVisible;
        set => this.RaiseAndSetIfChanged(ref _sscVisible, value);
    }

    public string DensitySupport
    {
        get => _densitySupport;
        set => this.RaiseAndSetIfChanged(ref _densitySupport, value);
    }

    public string MediumSupport
    {
        get => _mediumSupport;
        set => this.RaiseAndSetIfChanged(ref _mediumSupport, value);
    }

    public bool SaveDensitySupportVisible
    {
        get => _saveDensitySupportVisible;
        set => this.RaiseAndSetIfChanged(ref _saveDensitySupportVisible, value);
    }

    public bool SaveMediumSupportVisible
    {
        get => _saveMediumSupportVisible;
        set => this.RaiseAndSetIfChanged(ref _saveMediumSupportVisible, value);
    }

    public CompactDiscInfo CompactDiscInfo
    {
        get => _compactDiscInfo;
        set => this.RaiseAndSetIfChanged(ref _compactDiscInfo, value);
    }

    public DvdInfo DvdInfo
    {
        get => _dvdInfo;
        set => this.RaiseAndSetIfChanged(ref _dvdInfo, value);
    }

    public DvdWritableInfo DvdWritableInfo
    {
        get => _dvdWritableInfo;
        set => this.RaiseAndSetIfChanged(ref _dvdWritableInfo, value);
    }

    public XboxInfo XboxInfo
    {
        get => _xboxInfo;
        set => this.RaiseAndSetIfChanged(ref _xboxInfo, value);
    }

    public BlurayInfo BlurayInfo
    {
        get => _blurayInfo;
        set => this.RaiseAndSetIfChanged(ref _blurayInfo, value);
    }

    public string MediaInformationLabel           => UI.Title_Media_information;
    public string GeneralLabel                    => UI.Title_General;
    public string MediaTypeLabel                  => UI.Title_Media_type;
    public string MediaSerialNumberLabel          => UI.Title_Media_serial_number;
    public string SaveReadMediaSerialLabel        => UI.ButtonLabel_Save_READ_MEDIA_SERIAL_NUMBER_response;
    public string SaveReadCapacityLabel           => UI.ButtonLabel_Save_READ_CAPACITY_response;
    public string SaveReadCapacity16Label         => UI.ButtonLabel_Save_READ_CAPACITY_16_response;
    public string MMCLabel                        => Localization.Core.Title_MMC;
    public string SaveGetConfigurationLabel       => UI.ButtonLabel_Save_GET_CONFIGURATION_response;
    public string SaveRecognizedFormatLayersLabel => UI.ButtonLabel_Save_RECOGNIZED_FORMAT_LAYERS_response;
    public string SaveWriteProtectionStatusLabel  => UI.ButtonLabel_Save_WRITE_PROTECTION_STATUS_response;
    public string SscLabel                        => Localization.Core.Title_SSC;
    public string DensitySupportLabel             => UI.Densities_supported_by_currently_inserted_media;
    public string MediumSupportLabel              => UI.Medium_types_currently_inserted_in_device;
    public string SaveDensitySupportLabel         => UI.ButtonLabel_Save_REPORT_DENSITY_SUPPORT_MEDIA_response;
    public string SaveMediumSupportLabel          => UI.ButtonLabel_Save_REPORT_DENSITY_SUPPORT_MEDIUM_MEDIA_response;
    public string CompactDiscLabel                => Localization.Core.Title_CompactDisc;
    public string DvdLabel                        => Localization.Core.Title_DVD;
    public string Dvd_R_WLabel                    => Localization.Core.Title_DVD_Plus_Dash_R_W;
    public string XboxLabel                       => Localization.Core.Title_Xbox;
    public string BluRayLabel                     => Localization.Core.Title_Blu_ray;
    public string DumpLabel                       => UI.ButtonLabel_Dump_media_to_image;
    public string ScanLabel                       => UI.ButtonLabel_Scan_media_surface;

    async Task SaveElement(byte[] data)
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters?.Add(new FileDialogFilter
        {
            Extensions =
            [
                ..new[]
                {
                    "*.bin"
                }
            ],
            Name = UI.Dialog_Binary_files
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null) return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(data, 0, data.Length);

        saveFs.Close();
    }

    async Task ExecuteSaveReadMediaSerialCommand() => await SaveElement(_scsiInfo.MediaSerialNumber);

    async Task ExecuteSaveReadCapacityCommand() => await SaveElement(_scsiInfo.ReadCapacity);

    async Task ExecuteSaveReadCapacity16Command() => await SaveElement(_scsiInfo.ReadCapacity16);

    async Task ExecuteSaveGetConfigurationCommand() => await SaveElement(_scsiInfo.MmcConfiguration);

    async Task ExecuteSaveRecognizedFormatLayersCommand() => await SaveElement(_scsiInfo.RecognizedFormatLayers);

    async Task ExecuteSaveWriteProtectionStatusCommand() => await SaveElement(_scsiInfo.WriteProtectionStatus);

    async Task ExecuteSaveDensitySupportCommand() => await SaveElement(_scsiInfo.DensitySupport);

    async Task ExecuteSaveMediumSupportCommand() => await SaveElement(_scsiInfo.MediaTypeSupport);

    async Task ExecuteDumpCommand()
    {
        switch(_scsiInfo.MediaType)
        {
            case CommonTypes.MediaType.GDR or CommonTypes.MediaType.GDROM:
                await MessageBoxManager
                     .GetMessageBoxStandard(UI.Title_Error,
                                            Localization.Core.GD_ROM_dump_support_is_not_yet_implemented,
                                            ButtonEnum.Ok,
                                            Icon.Error)
                     .ShowWindowDialogAsync(_view);

                return;
            case CommonTypes.MediaType.XGD or CommonTypes.MediaType.XGD2 or CommonTypes.MediaType.XGD3
                when _scsiInfo.DeviceInfo.ScsiInquiry?.KreonPresent != true:
                await MessageBoxManager
                     .GetMessageBoxStandard(UI.Title_Error,
                                            Localization.Core
                                                        .Dumping_Xbox_Game_Discs_requires_a_drive_with_Kreon_firmware,
                                            ButtonEnum.Ok,
                                            Icon.Error)
                     .ShowWindowDialogAsync(_view);

                return;
        }

        var mediaDumpWindow = new MediaDump();

        mediaDumpWindow.DataContext =
            new MediaDumpViewModel(_devicePath, _scsiInfo.DeviceInfo, mediaDumpWindow, _scsiInfo);

        mediaDumpWindow.Show();
    }

    async Task ExecuteScanCommand()
    {
        switch(_scsiInfo.MediaType)
        {
            // TODO: GD-ROM
            case CommonTypes.MediaType.GDR:
            case CommonTypes.MediaType.GDROM:
                await MessageBoxManager
                     .GetMessageBoxStandard(UI.Title_Error,
                                            Localization.Core.GD_ROM_scan_support_is_not_yet_implemented,
                                            ButtonEnum.Ok,
                                            Icon.Error)
                     .ShowWindowDialogAsync(_view);

                return;

            // TODO: Xbox
            case CommonTypes.MediaType.XGD:
            case CommonTypes.MediaType.XGD2:
            case CommonTypes.MediaType.XGD3:
                await MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                                                              Localization.Core
                                                                          .Scanning_Xbox_discs_is_not_yet_supported,
                                                              ButtonEnum.Ok,
                                                              Icon.Error)
                                       .ShowWindowDialogAsync(_view);

                return;
        }

        var mediaScanWindow = new MediaScan();

        mediaScanWindow.DataContext = new MediaScanViewModel(_devicePath, mediaScanWindow);

        mediaScanWindow.Show();
    }
}