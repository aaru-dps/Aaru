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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Gui.ViewModels.Panels;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Text;
using Aaru.Gui.ViewModels.Tabs;
using Aaru.Gui.ViewModels.Windows;
using Aaru.Gui.Views.Tabs;
using Aaru.Gui.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using ScsiInfo = Aaru.Core.Media.Info.ScsiInfo;

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
        IAssetLoader assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        _devicePath = devicePath;
        _scsiInfo   = scsiInfo;

        var mediaResource = new Uri($"avares://Aaru.Gui/Assets/Logos/Media/{scsiInfo.MediaType}.png");

        MediaLogo = assets?.Exists(mediaResource) == true ? new Bitmap(assets.Open(mediaResource)) : null;

        MediaType = scsiInfo.MediaType.ToString();

        if(scsiInfo.Blocks    != 0 &&
           scsiInfo.BlockSize != 0)
        {
            ulong totalSize = scsiInfo.Blocks * scsiInfo.BlockSize;

            if(totalSize > 1099511627776)
                MediaSize =
                    $"Media has {scsiInfo.Blocks} blocks of {scsiInfo.BlockSize} bytes/each. (for a total of {totalSize / 1099511627776d:F3} TiB)";
            else if(totalSize > 1073741824)
                MediaSize =
                    $"Media has {scsiInfo.Blocks} blocks of {scsiInfo.BlockSize} bytes/each. (for a total of {totalSize / 1073741824d:F3} GiB)";
            else if(totalSize > 1048576)
                MediaSize =
                    $"Media has {scsiInfo.Blocks} blocks of {scsiInfo.BlockSize} bytes/each. (for a total of {totalSize / 1048576d:F3} MiB)";
            else if(totalSize > 1024)
                MediaSize =
                    $"Media has {scsiInfo.Blocks} blocks of {scsiInfo.BlockSize} bytes/each. (for a total of {totalSize / 1024d:F3} KiB)";
            else
                MediaSize =
                    $"Media has {scsiInfo.Blocks} blocks of {scsiInfo.BlockSize} bytes/each. (for a total of {totalSize} bytes)";
        }

        if(scsiInfo.MediaSerialNumber != null)
        {
            var sbSerial = new StringBuilder();

            for(var i = 4; i < scsiInfo.MediaSerialNumber.Length; i++)
                sbSerial.AppendFormat("{0:X2}", scsiInfo.MediaSerialNumber[i]);

            MediaSerial = sbSerial.ToString();
        }

        SaveReadMediaSerialVisible = scsiInfo.MediaSerialNumber != null;
        SaveReadCapacityVisible    = scsiInfo.ReadCapacity      != null;
        SaveReadCapacity16Visible  = scsiInfo.ReadCapacity16    != null;

        SaveGetConfigurationVisible       = scsiInfo.MmcConfiguration       != null;
        SaveRecognizedFormatLayersVisible = scsiInfo.RecognizedFormatLayers != null;
        SaveWriteProtectionStatusVisible  = scsiInfo.WriteProtectionStatus  != null;

        MmcVisible = SaveGetConfigurationVisible || SaveRecognizedFormatLayersVisible ||
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
            DataContext = new CompactDiscInfoViewModel(scsiInfo.Toc, scsiInfo.Atip, scsiInfo.DiscInformation,
                                                       scsiInfo.Session, scsiInfo.RawToc, scsiInfo.Pma,
                                                       scsiInfo.CdTextLeadIn, scsiInfo.DecodedToc, scsiInfo.DecodedAtip,
                                                       scsiInfo.DecodedSession, scsiInfo.FullToc,
                                                       scsiInfo.DecodedCdTextLeadIn, scsiInfo.DecodedDiscInformation,
                                                       scsiInfo.Mcn, scsiInfo.Isrcs, _view)
        };

        DvdInfo = new DvdInfo
        {
            DataContext = new DvdInfoViewModel(scsiInfo.MediaType, scsiInfo.DvdPfi, scsiInfo.DvdDmi, scsiInfo.DvdCmi,
                                               scsiInfo.HddvdCopyrightInformation, scsiInfo.DvdBca, scsiInfo.DvdAacs,
                                               scsiInfo.DecodedPfi, _view)
        };

        XboxInfo = new XboxInfo
        {
            DataContext = new XboxInfoViewModel(scsiInfo.XgdInfo, scsiInfo.DvdDmi, scsiInfo.XboxSecuritySector,
                                                scsiInfo.DecodedXboxSecuritySector, _view)
        };

        DvdWritableInfo = new DvdWritableInfo
        {
            DataContext = new DvdWritableInfoViewModel(scsiInfo.MediaType, scsiInfo.DvdRamDds,
                                                       scsiInfo.DvdRamCartridgeStatus, scsiInfo.DvdRamSpareArea,
                                                       scsiInfo.LastBorderOutRmd, scsiInfo.DvdPreRecordedInfo,
                                                       scsiInfo.DvdrMediaIdentifier, scsiInfo.DvdrPhysicalInformation,
                                                       scsiInfo.HddvdrMediumStatus, scsiInfo.HddvdrLastRmd,
                                                       scsiInfo.DvdrLayerCapacity, scsiInfo.DvdrDlMiddleZoneStart,
                                                       scsiInfo.DvdrDlJumpIntervalSize,
                                                       scsiInfo.DvdrDlManualLayerJumpStartLba,
                                                       scsiInfo.DvdrDlRemapAnchorPoint, scsiInfo.DvdPlusAdip,
                                                       scsiInfo.DvdPlusDcb, _view)
        };

        BlurayInfo = new BlurayInfo
        {
            DataContext = new BlurayInfoViewModel(scsiInfo.BlurayDiscInformation, scsiInfo.BlurayBurstCuttingArea,
                                                  scsiInfo.BlurayDds, scsiInfo.BlurayCartridgeStatus,
                                                  scsiInfo.BluraySpareAreaInformation, scsiInfo.BlurayPowResources,
                                                  scsiInfo.BlurayTrackResources, scsiInfo.BlurayRawDfl,
                                                  scsiInfo.BlurayPac, _view)
        };
    }

    public ReactiveCommand<Unit, Unit> SaveReadMediaSerialCommand        { get; }
    public ReactiveCommand<Unit, Unit> SaveReadCapacityCommand           { get; }
    public ReactiveCommand<Unit, Unit> SaveReadCapacity16Command         { get; }
    public ReactiveCommand<Unit, Unit> SaveGetConfigurationCommand       { get; }
    public ReactiveCommand<Unit, Unit> SaveRecognizedFormatLayersCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveWriteProtectionStatusCommand  { get; }
    public ReactiveCommand<Unit, Unit> SaveDensitySupportCommand         { get; }
    public ReactiveCommand<Unit, Unit> SaveMediumSupportCommand          { get; }
    public ReactiveCommand<Unit, Unit> DumpCommand                       { get; }
    public ReactiveCommand<Unit, Unit> ScanCommand                       { get; }

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

    async void SaveElement(byte[] data)
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters.Add(new FileDialogFilter
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
        saveFs.Write(data, 0, data.Length);

        saveFs.Close();
    }

    void ExecuteSaveReadMediaSerialCommand() => SaveElement(_scsiInfo.MediaSerialNumber);

    void ExecuteSaveReadCapacityCommand() => SaveElement(_scsiInfo.ReadCapacity);

    void ExecuteSaveReadCapacity16Command() => SaveElement(_scsiInfo.ReadCapacity16);

    void ExecuteSaveGetConfigurationCommand() => SaveElement(_scsiInfo.MmcConfiguration);

    void ExecuteSaveRecognizedFormatLayersCommand() => SaveElement(_scsiInfo.RecognizedFormatLayers);

    void ExecuteSaveWriteProtectionStatusCommand() => SaveElement(_scsiInfo.WriteProtectionStatus);

    void ExecuteSaveDensitySupportCommand() => SaveElement(_scsiInfo.DensitySupport);

    void ExecuteSaveMediumSupportCommand() => SaveElement(_scsiInfo.MediaTypeSupport);

    async void ExecuteDumpCommand()
    {
        if(_scsiInfo.MediaType is CommonTypes.MediaType.GDR or CommonTypes.MediaType.GDROM)
        {
            await MessageBoxManager.
                  GetMessageBoxStandardWindow("Error", "GD-ROM dump support is not yet implemented.", ButtonEnum.Ok,
                                              Icon.Error).ShowDialog(_view);

            return;
        }

        if(_scsiInfo.MediaType is CommonTypes.MediaType.XGD or CommonTypes.MediaType.XGD2
                                                            or CommonTypes.MediaType.XGD3 &&
           _scsiInfo.DeviceInfo.ScsiInquiry?.KreonPresent != true)
        {
            await MessageBoxManager.
                  GetMessageBoxStandardWindow("Error", "Dumping Xbox discs require a Kreon drive.", ButtonEnum.Ok,
                                              Icon.Error).ShowDialog(_view);

            return;
        }

        var mediaDumpWindow = new MediaDump();

        mediaDumpWindow.DataContext =
            new MediaDumpViewModel(_devicePath, _scsiInfo.DeviceInfo, mediaDumpWindow, _scsiInfo);

        mediaDumpWindow.Show();
    }

    async void ExecuteScanCommand()
    {
        switch(_scsiInfo.MediaType)
        {
            // TODO: GD-ROM
            case CommonTypes.MediaType.GDR:
            case CommonTypes.MediaType.GDROM:
                await MessageBoxManager.
                      GetMessageBoxStandardWindow("Error", "GD-ROM scan support is not yet implemented.", ButtonEnum.Ok,
                                                  Icon.Error).ShowDialog(_view);

                return;

            // TODO: Xbox
            case CommonTypes.MediaType.XGD:
            case CommonTypes.MediaType.XGD2:
            case CommonTypes.MediaType.XGD3:
                await MessageBoxManager.
                      GetMessageBoxStandardWindow("Error", "Scanning Xbox discs is not yet supported.", ButtonEnum.Ok,
                                                  Icon.Error).ShowDialog(_view);

                return;
        }

        var mediaScanWindow = new MediaScan();

        mediaScanWindow.DataContext = new MediaScanViewModel(_devicePath, mediaScanWindow);

        mediaScanWindow.Show();
    }
}