using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Text;
using Aaru.Core.Media.Info;
using Aaru.Gui.Tabs;
using Aaru.Gui.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using ReactiveUI;

namespace Aaru.Gui.ViewModels
{
    public class MediaInfoViewModel : ViewModelBase
    {
        readonly string    _devicePath;
        readonly ScsiInfo  _scsiInfo;
        readonly Window    _view;
        BlurayInfoTab      _blurayInfo;
        CompactDiscInfoTab _compactDiscInfo;
        string             _densitySupport;
        DvdInfoTab         _dvdInfo;
        DvdWritableInfoTab _dvdWritableInfo;
        string             _generalVisible;
        Bitmap             _mediaLogo;
        string             _mediaSerial;
        string             _mediaSize;
        string             _mediaType;
        string             _mediumSupport;
        bool               _mmcVisible;
        bool               _saveDensitySupportVisible;
        bool               _saveGetConfigurationVisible;
        bool               _saveMediumSupportVisible;
        bool               _saveReadCapacity16Visible;
        bool               _saveReadCapacityVisible;
        bool               _saveReadMediaSerialVisible;
        bool               _saveRecognizedFormatLayersVisible;
        bool               _saveWriteProtectionStatusVisible;
        bool               _sscVisible;
        XboxInfoTab        _xboxInfo;

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

            MediaLogo = assets.Exists(mediaResource) ? new Bitmap(assets.Open(mediaResource)) : null;

            MediaType = scsiInfo.MediaType.ToString();

            if(scsiInfo.Blocks    != 0 &&
               scsiInfo.BlockSize != 0)
                MediaSize =
                    $"Media has {scsiInfo.Blocks} blocks of {scsiInfo.BlockSize} bytes/each. (for a total of {scsiInfo.Blocks * scsiInfo.BlockSize} bytes)";

            if(scsiInfo.MediaSerialNumber != null)
            {
                var sbSerial = new StringBuilder();

                for(int i = 4; i < scsiInfo.MediaSerialNumber.Length; i++)
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

            CompactDiscInfo = new CompactDiscInfoTab
            {
                DataContext = new CompactDiscInfoViewModel(scsiInfo.Toc, scsiInfo.Atip, scsiInfo.CompactDiscInformation,
                                                           scsiInfo.Session, scsiInfo.RawToc, scsiInfo.Pma,
                                                           scsiInfo.CdTextLeadIn, scsiInfo.DecodedToc,
                                                           scsiInfo.DecodedAtip, scsiInfo.DecodedSession,
                                                           scsiInfo.FullToc, scsiInfo.DecodedCdTextLeadIn,
                                                           scsiInfo.DecodedCompactDiscInformation, scsiInfo.Mcn,
                                                           scsiInfo.Isrcs, _view)
            };

            DvdInfo = new DvdInfoTab
            {
                DataContext = new DvdInfoViewModel(scsiInfo.MediaType, scsiInfo.DvdPfi, scsiInfo.DvdDmi,
                                                   scsiInfo.DvdCmi, scsiInfo.HddvdCopyrightInformation, scsiInfo.DvdBca,
                                                   scsiInfo.DvdAacs, scsiInfo.DecodedPfi, _view)
            };

            XboxInfo = new XboxInfoTab
            {
                DataContext = new XboxInfoViewModel(scsiInfo.XgdInfo, scsiInfo.DvdDmi, scsiInfo.XboxSecuritySector,
                                                    scsiInfo.DecodedXboxSecuritySector, _view)
            };

            DvdWritableInfo = new DvdWritableInfoTab
            {
                DataContext = new DvdWritableInfoViewModel(scsiInfo.MediaType, scsiInfo.DvdRamDds,
                                                           scsiInfo.DvdRamCartridgeStatus, scsiInfo.DvdRamSpareArea,
                                                           scsiInfo.LastBorderOutRmd, scsiInfo.DvdPreRecordedInfo,
                                                           scsiInfo.DvdrMediaIdentifier,
                                                           scsiInfo.DvdrPhysicalInformation,
                                                           scsiInfo.HddvdrMediumStatus, scsiInfo.HddvdrLastRmd,
                                                           scsiInfo.DvdrLayerCapacity, scsiInfo.DvdrDlMiddleZoneStart,
                                                           scsiInfo.DvdrDlJumpIntervalSize,
                                                           scsiInfo.DvdrDlManualLayerJumpStartLba,
                                                           scsiInfo.DvdrDlRemapAnchorPoint, scsiInfo.DvdPlusAdip,
                                                           scsiInfo.DvdPlusDcb, _view)
            };

            BlurayInfo = new BlurayInfoTab
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

        public CompactDiscInfoTab CompactDiscInfo
        {
            get => _compactDiscInfo;
            set => this.RaiseAndSetIfChanged(ref _compactDiscInfo, value);
        }

        public DvdInfoTab DvdInfo
        {
            get => _dvdInfo;
            set => this.RaiseAndSetIfChanged(ref _dvdInfo, value);
        }

        public DvdWritableInfoTab DvdWritableInfo
        {
            get => _dvdWritableInfo;
            set => this.RaiseAndSetIfChanged(ref _dvdWritableInfo, value);
        }

        public XboxInfoTab XboxInfo
        {
            get => _xboxInfo;
            set => this.RaiseAndSetIfChanged(ref _xboxInfo, value);
        }

        public BlurayInfoTab BlurayInfo
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
            if(_scsiInfo.MediaType == CommonTypes.MediaType.GDR ||
               _scsiInfo.MediaType == CommonTypes.MediaType.GDROM)
            {
                await MessageBoxManager.
                      GetMessageBoxStandardWindow("Error", "GD-ROM dump support is not yet implemented.", ButtonEnum.Ok,
                                                  Icon.Error).ShowDialog(_view);

                return;
            }

            if((_scsiInfo.MediaType == CommonTypes.MediaType.XGD || _scsiInfo.MediaType == CommonTypes.MediaType.XGD2 ||
                _scsiInfo.MediaType == CommonTypes.MediaType.XGD3) &&
               _scsiInfo.DeviceInfo.ScsiInquiry?.KreonPresent != true)
            {
                await MessageBoxManager.
                      GetMessageBoxStandardWindow("Error", "Dumping Xbox discs require a Kreon drive.", ButtonEnum.Ok,
                                                  Icon.Error).ShowDialog(_view);

                return;
            }

            var mediaDumpWindow = new MediaDumpWindow();

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
                          GetMessageBoxStandardWindow("Error", "GD-ROM scan support is not yet implemented.",
                                                      ButtonEnum.Ok, Icon.Error).ShowDialog(_view);

                    return;

                // TODO: Xbox
                case CommonTypes.MediaType.XGD:
                case CommonTypes.MediaType.XGD2:
                case CommonTypes.MediaType.XGD3:
                    await MessageBoxManager.
                          GetMessageBoxStandardWindow("Error", "Scanning Xbox discs is not yet supported.",
                                                      ButtonEnum.Ok, Icon.Error).ShowDialog(_view);

                    return;
            }

            var mediaScanWindow = new MediaScanWindow();

            mediaScanWindow.DataContext =
                new MediaScanViewModel(_devicePath, _scsiInfo.DeviceInfo, mediaScanWindow, _scsiInfo);

            mediaScanWindow.Show();
        }
    }
}