// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ImageConvertViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the image conversion window.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Console;
using Aaru.Core;
using Aaru.Core.Media;
using Aaru.Devices;
using Aaru.Gui.Models;
using Aaru.Localization;
using Avalonia.Controls;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using ImageInfo = Aaru.CommonTypes.Structs.ImageInfo;
using Track = Aaru.CommonTypes.Structs.Track;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Gui.ViewModels.Windows;

[SuppressMessage("ReSharper", "AsyncVoidLambda")]
public sealed class ImageConvertViewModel : ViewModelBase
{
    readonly IMediaImage _inputFormat;
    readonly Window      _view;
    Metadata             _aaruMetadata;
    bool                 _aaruMetadataFromImageVisible;
    bool                 _cancel;
    bool                 _closeVisible;
    string               _commentsText;
    bool                 _commentsVisible;
    string               _creatorText;
    bool                 _creatorVisible;
    bool                 _destinationEnabled;
    string               _destinationText;
    bool                 _destinationVisible;
    string               _driveFirmwareRevisionText;
    bool                 _driveFirmwareRevisionVisible;
    string               _driveManufacturerText;
    bool                 _driveManufacturerVisible;
    string               _driveModelText;
    bool                 _driveModelVisible;
    string               _driveSerialNumberText;
    bool                 _driveSerialNumberVisible;
    List<DumpHardware>   _dumpHardware;
    bool                 _forceChecked;
    bool                 _formatReadOnly;
    double               _lastMediaSequenceValue;
    bool                 _lastMediaSequenceVisible;
    string               _mediaBarcodeText;
    bool                 _mediaBarcodeVisible;
    string               _mediaManufacturerText;
    bool                 _mediaManufacturerVisible;
    string               _mediaModelText;
    bool                 _mediaModelVisible;
    string               _mediaPartNumberText;
    bool                 _mediaPartNumberVisible;
    double               _mediaSequenceValue;
    bool                 _mediaSequenceVisible;
    string               _mediaSerialNumberText;
    bool                 _mediaSerialNumberVisible;
    string               _mediaTitleText;
    bool                 _mediaTitleVisible;
    string               _metadataJsonText;
    bool                 _optionsVisible;
    bool                 _progress1Visible;
    bool                 _progress2Indeterminate;
    double               _progress2MaxValue;
    string               _progress2Text;
    double               _progress2Value;
    bool                 _progress2Visible;
    bool                 _progressIndeterminate;
    double               _progressMaxValue;
    string               _progressText;
    double               _progressValue;
    bool                 _progressVisible;
    bool                 _resumeFileFromImageVisible;
    string               _resumeFileText;
    double               _sectorsValue;
    ImagePluginModel     _selectedPlugin;
    string               _sourceText;
    bool                 _startVisible;
    bool                 _stopEnabled;
    bool                 _stopVisible;
    string               _title;

    public ImageConvertViewModel([JetBrains.Annotations.NotNull] IMediaImage inputFormat, string imageSource,
                                 Window                                      view)
    {
        _view                        = view;
        _inputFormat                 = inputFormat;
        _cancel                      = false;
        DestinationCommand           = ReactiveCommand.Create(ExecuteDestinationCommand);
        CreatorCommand               = ReactiveCommand.Create(ExecuteCreatorCommand);
        MediaTitleCommand            = ReactiveCommand.Create(ExecuteMediaTitleCommand);
        MediaManufacturerCommand     = ReactiveCommand.Create(ExecuteMediaManufacturerCommand);
        MediaModelCommand            = ReactiveCommand.Create(ExecuteMediaModelCommand);
        MediaSerialNumberCommand     = ReactiveCommand.Create(ExecuteMediaSerialNumberCommand);
        MediaBarcodeCommand          = ReactiveCommand.Create(ExecuteMediaBarcodeCommand);
        MediaPartNumberCommand       = ReactiveCommand.Create(ExecuteMediaPartNumberCommand);
        MediaSequenceCommand         = ReactiveCommand.Create(ExecuteMediaSequenceCommand);
        LastMediaSequenceCommand     = ReactiveCommand.Create(ExecuteLastMediaSequenceCommand);
        DriveManufacturerCommand     = ReactiveCommand.Create(ExecuteDriveManufacturerCommand);
        DriveModelCommand            = ReactiveCommand.Create(ExecuteDriveModelCommand);
        DriveSerialNumberCommand     = ReactiveCommand.Create(ExecuteDriveSerialNumberCommand);
        DriveFirmwareRevisionCommand = ReactiveCommand.Create(ExecuteDriveFirmwareRevisionCommand);
        CommentsCommand              = ReactiveCommand.Create(ExecuteCommentsCommand);
        AaruMetadataFromImageCommand = ReactiveCommand.Create(ExecuteAaruMetadataFromImageCommand);
        AaruMetadataCommand          = ReactiveCommand.Create(ExecuteAaruMetadataCommand);
        ResumeFileFromImageCommand   = ReactiveCommand.Create(ExecuteResumeFileFromImageCommand);
        ResumeFileCommand            = ReactiveCommand.Create(ExecuteResumeFileCommand);
        StartCommand                 = ReactiveCommand.Create(ExecuteStartCommand);
        CloseCommand                 = ReactiveCommand.Create(ExecuteCloseCommand);
        StopCommand                  = ReactiveCommand.Create(ExecuteStopCommand);
        SourceText                   = imageSource;
        CreatorVisible               = !string.IsNullOrWhiteSpace(inputFormat.Info.Creator);
        MediaTitleVisible            = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaTitle);
        CommentsVisible              = !string.IsNullOrWhiteSpace(inputFormat.Info.Comments);
        MediaManufacturerVisible     = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaManufacturer);
        MediaModelVisible            = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaModel);
        MediaSerialNumberVisible     = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaSerialNumber);
        MediaBarcodeVisible          = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaBarcode);
        MediaPartNumberVisible       = !string.IsNullOrWhiteSpace(inputFormat.Info.MediaPartNumber);
        MediaSequenceVisible         = inputFormat.Info.MediaSequence != 0 && inputFormat.Info.LastMediaSequence != 0;
        LastMediaSequenceVisible     = inputFormat.Info.MediaSequence != 0 && inputFormat.Info.LastMediaSequence != 0;
        DriveManufacturerVisible     = !string.IsNullOrWhiteSpace(inputFormat.Info.DriveManufacturer);
        DriveModelVisible            = !string.IsNullOrWhiteSpace(inputFormat.Info.DriveModel);
        DriveSerialNumberVisible     = !string.IsNullOrWhiteSpace(inputFormat.Info.DriveSerialNumber);
        DriveFirmwareRevisionVisible = !string.IsNullOrWhiteSpace(inputFormat.Info.DriveFirmwareRevision);

        PluginRegister plugins = PluginRegister.Singleton;

        foreach(IBaseWritableImage plugin in plugins.WritableImages.Values)
        {
            if(plugin is null)
                continue;

            if(plugin.SupportedMediaTypes.Contains(inputFormat.Info.MediaType))
            {
                PluginsList.Add(new ImagePluginModel
                {
                    Plugin = plugin
                });
            }
        }

        AaruMetadataFromImageVisible = inputFormat.AaruMetadata        != null;
        ResumeFileFromImageVisible   = inputFormat.DumpHardware?.Any() == true;
        _aaruMetadata                = inputFormat.AaruMetadata;

        _dumpHardware = inputFormat.DumpHardware?.Any() == true ? inputFormat.DumpHardware : null;

        MetadataJsonText = _aaruMetadata == null ? "" : UI._From_image_;
        ResumeFileText   = _dumpHardware == null ? "" : UI._From_image_;
    }

    public string SourceImageLabel            => UI.Source_image;
    public string OutputFormatLabel           => UI.Output_format;
    public string ChooseLabel                 => UI.ButtonLabel_Choose;
    public string SectorsLabel                => UI.How_many_sectors_to_convert_at_once;
    public string ForceLabel                  => UI.Continue_conversion_even_if_data_lost;
    public string CreatorLabel                => UI.Who_person_created_the_image;
    public string GetFromSourceImageLabel     => UI.ButtonLabel_Get_from_source_image;
    public string MetadataLabel               => UI.Title_Metadata;
    public string MediaLabel                  => UI.Title_Media;
    public string TitleLabel                  => UI.Title_Title;
    public string ManufacturerLabel           => UI.Title_Manufacturer;
    public string ModelLabel                  => UI.Title_Model;
    public string SerialNumberLabel           => UI.Title_Serial_number;
    public string BarcodeLabel                => UI.Title_Barcode;
    public string PartNumberLabel             => UI.Title_Part_number;
    public string NumberInSequenceLabel       => UI.Title_Number_in_sequence;
    public string LastMediaOfTheSequenceLabel => UI.Title_Last_media_of_the_sequence;
    public string DriveLabel                  => UI.Title_Drive;
    public string FirmwareRevisionLabel       => UI.Title_Firmware_revision;
    public string CommentsLabel               => UI.Title_Comments;
    public string AaruMetadataLabel           => UI.Title_Existing_Aaru_Metadata_sidecar;
    public string FromImageLabel              => UI.Title_From_image;
    public string ResumeFileLabel             => UI.Title_Existing_resume_file;
    public string StartLabel                  => UI.ButtonLabel_Start;
    public string CloseLabel                  => UI.ButtonLabel_Close;
    public string StopLabel                   => UI.ButtonLabel_Stop;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string SourceText
    {
        get => _sourceText;
        set => this.RaiseAndSetIfChanged(ref _sourceText, value);
    }

    public ObservableCollection<ImagePluginModel> PluginsList { get; }

    public ImagePluginModel SelectedPlugin
    {
        get => _selectedPlugin;
        set => this.RaiseAndSetIfChanged(ref _selectedPlugin, value);
    }

    public string DestinationText
    {
        get => _destinationText;
        set => this.RaiseAndSetIfChanged(ref _destinationText, value);
    }

    public bool OptionsVisible
    {
        get => _optionsVisible;
        set => this.RaiseAndSetIfChanged(ref _optionsVisible, value);
    }

    public double SectorsValue
    {
        get => _sectorsValue;
        set => this.RaiseAndSetIfChanged(ref _sectorsValue, value);
    }

    public bool ForceChecked
    {
        get => _forceChecked;
        set => this.RaiseAndSetIfChanged(ref _forceChecked, value);
    }

    public string CreatorText
    {
        get => _creatorText;
        set => this.RaiseAndSetIfChanged(ref _creatorText, value);
    }

    public string MediaTitleText
    {
        get => _mediaTitleText;
        set => this.RaiseAndSetIfChanged(ref _mediaTitleText, value);
    }

    public string MediaManufacturerText
    {
        get => _mediaManufacturerText;
        set => this.RaiseAndSetIfChanged(ref _mediaManufacturerText, value);
    }

    public string MediaModelText
    {
        get => _mediaModelText;
        set => this.RaiseAndSetIfChanged(ref _mediaModelText, value);
    }

    public string MediaSerialNumberText
    {
        get => _mediaSerialNumberText;
        set => this.RaiseAndSetIfChanged(ref _mediaSerialNumberText, value);
    }

    public string MediaBarcodeText
    {
        get => _mediaBarcodeText;
        set => this.RaiseAndSetIfChanged(ref _mediaBarcodeText, value);
    }

    public string MediaPartNumberText
    {
        get => _mediaPartNumberText;
        set => this.RaiseAndSetIfChanged(ref _mediaPartNumberText, value);
    }

    public double MediaSequenceValue
    {
        get => _mediaSequenceValue;
        set => this.RaiseAndSetIfChanged(ref _mediaSequenceValue, value);
    }

    public double LastMediaSequenceValue
    {
        get => _lastMediaSequenceValue;
        set => this.RaiseAndSetIfChanged(ref _lastMediaSequenceValue, value);
    }

    public string DriveManufacturerText
    {
        get => _driveManufacturerText;
        set => this.RaiseAndSetIfChanged(ref _driveManufacturerText, value);
    }

    public string DriveModelText
    {
        get => _driveModelText;
        set => this.RaiseAndSetIfChanged(ref _driveModelText, value);
    }

    public string DriveSerialNumberText
    {
        get => _driveSerialNumberText;
        set => this.RaiseAndSetIfChanged(ref _driveSerialNumberText, value);
    }

    public string DriveFirmwareRevisionText
    {
        get => _driveFirmwareRevisionText;
        set => this.RaiseAndSetIfChanged(ref _driveFirmwareRevisionText, value);
    }

    public string CommentsText
    {
        get => _commentsText;
        set => this.RaiseAndSetIfChanged(ref _commentsText, value);
    }

    public string MetadataJsonText
    {
        get => _metadataJsonText;
        set => this.RaiseAndSetIfChanged(ref _metadataJsonText, value);
    }

    public string ResumeFileText
    {
        get => _resumeFileText;
        set => this.RaiseAndSetIfChanged(ref _resumeFileText, value);
    }

    public bool ProgressVisible
    {
        get => _progressVisible;
        set => this.RaiseAndSetIfChanged(ref _progressVisible, value);
    }

    public bool Progress1Visible
    {
        get => _progress1Visible;
        set => this.RaiseAndSetIfChanged(ref _progress1Visible, value);
    }

    public string ProgressText
    {
        get => _progressText;
        set => this.RaiseAndSetIfChanged(ref _progressText, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
    }

    public double ProgressMaxValue
    {
        get => _progressMaxValue;
        set => this.RaiseAndSetIfChanged(ref _progressMaxValue, value);
    }

    public bool ProgressIndeterminate
    {
        get => _progressIndeterminate;
        set => this.RaiseAndSetIfChanged(ref _progressIndeterminate, value);
    }

    public bool Progress2Visible
    {
        get => _progress2Visible;
        set => this.RaiseAndSetIfChanged(ref _progress2Visible, value);
    }

    public string Progress2Text
    {
        get => _progress2Text;
        set => this.RaiseAndSetIfChanged(ref _progress2Text, value);
    }

    public double Progress2Value
    {
        get => _progress2Value;
        set => this.RaiseAndSetIfChanged(ref _progress2Value, value);
    }

    public double Progress2MaxValue
    {
        get => _progress2MaxValue;
        set => this.RaiseAndSetIfChanged(ref _progress2MaxValue, value);
    }

    public bool Progress2Indeterminate
    {
        get => _progress2Indeterminate;
        set => this.RaiseAndSetIfChanged(ref _progress2Indeterminate, value);
    }

    public bool StartVisible
    {
        get => _startVisible;
        set => this.RaiseAndSetIfChanged(ref _startVisible, value);
    }

    public bool CloseVisible
    {
        get => _closeVisible;
        set => this.RaiseAndSetIfChanged(ref _closeVisible, value);
    }

    public bool StopVisible
    {
        get => _stopVisible;
        set => this.RaiseAndSetIfChanged(ref _stopVisible, value);
    }

    public bool StopEnabled
    {
        get => _stopEnabled;
        set => this.RaiseAndSetIfChanged(ref _stopEnabled, value);
    }

    public bool CreatorVisible
    {
        get => _creatorVisible;
        set => this.RaiseAndSetIfChanged(ref _creatorVisible, value);
    }

    public bool MediaTitleVisible
    {
        get => _mediaTitleVisible;
        set => this.RaiseAndSetIfChanged(ref _mediaTitleVisible, value);
    }

    public bool CommentsVisible
    {
        get => _commentsVisible;
        set => this.RaiseAndSetIfChanged(ref _commentsVisible, value);
    }

    public bool MediaManufacturerVisible
    {
        get => _mediaManufacturerVisible;
        set => this.RaiseAndSetIfChanged(ref _mediaManufacturerVisible, value);
    }

    public bool MediaModelVisible
    {
        get => _mediaModelVisible;
        set => this.RaiseAndSetIfChanged(ref _mediaModelVisible, value);
    }

    public bool MediaSerialNumberVisible
    {
        get => _mediaSerialNumberVisible;
        set => this.RaiseAndSetIfChanged(ref _mediaSerialNumberVisible, value);
    }

    public bool MediaBarcodeVisible
    {
        get => _mediaBarcodeVisible;
        set => this.RaiseAndSetIfChanged(ref _mediaBarcodeVisible, value);
    }

    public bool MediaPartNumberVisible
    {
        get => _mediaPartNumberVisible;
        set => this.RaiseAndSetIfChanged(ref _mediaPartNumberVisible, value);
    }

    public bool MediaSequenceVisible
    {
        get => _mediaSequenceVisible;
        set => this.RaiseAndSetIfChanged(ref _mediaSequenceVisible, value);
    }

    public bool LastMediaSequenceVisible
    {
        get => _lastMediaSequenceVisible;
        set => this.RaiseAndSetIfChanged(ref _lastMediaSequenceVisible, value);
    }

    public bool DriveManufacturerVisible
    {
        get => _driveManufacturerVisible;
        set => this.RaiseAndSetIfChanged(ref _driveManufacturerVisible, value);
    }

    public bool DriveModelVisible
    {
        get => _driveModelVisible;
        set => this.RaiseAndSetIfChanged(ref _driveModelVisible, value);
    }

    public bool DriveSerialNumberVisible
    {
        get => _driveSerialNumberVisible;
        set => this.RaiseAndSetIfChanged(ref _driveSerialNumberVisible, value);
    }

    public bool DriveFirmwareRevisionVisible
    {
        get => _driveFirmwareRevisionVisible;
        set => this.RaiseAndSetIfChanged(ref _driveFirmwareRevisionVisible, value);
    }

    public bool AaruMetadataFromImageVisible
    {
        get => _aaruMetadataFromImageVisible;
        set => this.RaiseAndSetIfChanged(ref _aaruMetadataFromImageVisible, value);
    }

    public bool ResumeFileFromImageVisible
    {
        get => _resumeFileFromImageVisible;
        set => this.RaiseAndSetIfChanged(ref _resumeFileFromImageVisible, value);
    }

    public bool DestinationEnabled
    {
        get => _destinationEnabled;
        set => this.RaiseAndSetIfChanged(ref _destinationEnabled, value);
    }

    public ReactiveCommand<Unit, Task> DestinationCommand           { get; }
    public ReactiveCommand<Unit, Unit> CreatorCommand               { get; }
    public ReactiveCommand<Unit, Unit> MediaTitleCommand            { get; }
    public ReactiveCommand<Unit, Unit> MediaManufacturerCommand     { get; }
    public ReactiveCommand<Unit, Unit> MediaModelCommand            { get; }
    public ReactiveCommand<Unit, Unit> MediaSerialNumberCommand     { get; }
    public ReactiveCommand<Unit, Unit> MediaBarcodeCommand          { get; }
    public ReactiveCommand<Unit, Unit> MediaPartNumberCommand       { get; }
    public ReactiveCommand<Unit, Unit> MediaSequenceCommand         { get; }
    public ReactiveCommand<Unit, Unit> LastMediaSequenceCommand     { get; }
    public ReactiveCommand<Unit, Unit> DriveManufacturerCommand     { get; }
    public ReactiveCommand<Unit, Unit> DriveModelCommand            { get; }
    public ReactiveCommand<Unit, Unit> DriveSerialNumberCommand     { get; }
    public ReactiveCommand<Unit, Unit> DriveFirmwareRevisionCommand { get; }
    public ReactiveCommand<Unit, Unit> CommentsCommand              { get; }
    public ReactiveCommand<Unit, Unit> AaruMetadataFromImageCommand { get; }
    public ReactiveCommand<Unit, Task> AaruMetadataCommand          { get; }
    public ReactiveCommand<Unit, Unit> ResumeFileFromImageCommand   { get; }
    public ReactiveCommand<Unit, Task> ResumeFileCommand            { get; }
    public ReactiveCommand<Unit, Task> StartCommand                 { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand                 { get; }
    public ReactiveCommand<Unit, Unit> StopCommand                  { get; }

    public bool FormatReadOnly
    {
        get => _formatReadOnly;
        set => this.RaiseAndSetIfChanged(ref _formatReadOnly, value);
    }

    public bool DestinationVisible
    {
        get => _destinationVisible;
        set => this.RaiseAndSetIfChanged(ref _destinationVisible, value);
    }

    async Task ExecuteStartCommand()
    {
        if(SelectedPlugin is null)
        {
            await MessageBoxManager.GetMessageBoxStandard(UI.Title_Error, UI.Error_trying_to_find_selected_plugin,
                                                          icon: Icon.Error).
                                    ShowWindowDialogAsync(_view);

            return;
        }

        new Thread(DoWork).Start(SelectedPlugin.Plugin);
    }

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    async void DoWork(object plugin)
    {
        var warning = false;

        if(plugin is not IWritableImage outputFormat)
        {
            await MessageBoxManager.GetMessageBoxStandard(UI.Title_Error, UI.Error_trying_to_find_selected_plugin,
                                                          icon: Icon.Error).
                                    ShowWindowDialogAsync(_view);

            return;
        }

        var inputOptical  = _inputFormat as IOpticalMediaImage;
        var outputOptical = outputFormat as IWritableOpticalImage;

        List<Track> tracks;

        try
        {
            tracks = inputOptical?.Tracks;
        }
        catch(Exception)
        {
            tracks = null;
        }

        // Prepare UI
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            CloseVisible       = false;
            StartVisible       = false;
            StopVisible        = true;
            ProgressVisible    = true;
            OptionsVisible     = false;
            StopEnabled        = true;
            FormatReadOnly     = true;
            DestinationVisible = false;

            ProgressMaxValue =  1d;
            ProgressMaxValue += _inputFormat.Info.ReadableMediaTags.Count;
            ProgressMaxValue++;

            if(tracks != null)
                ProgressMaxValue++;

            if(tracks == null)
            {
                ProgressMaxValue += 2;

                foreach(SectorTagType tag in _inputFormat.Info.ReadableSectorTags)
                {
                    switch(tag)
                    {
                        case SectorTagType.AppleSectorTag:
                        case SectorTagType.CdSectorSync:
                        case SectorTagType.CdSectorHeader:
                        case SectorTagType.CdSectorSubHeader:
                        case SectorTagType.CdSectorEdc:
                        case SectorTagType.CdSectorEccP:
                        case SectorTagType.CdSectorEccQ:
                        case SectorTagType.CdSectorEcc:
                            // This tags are inline in long sector
                            continue;
                    }

                    if(ForceChecked && !outputFormat.SupportedSectorTags.Contains(tag))
                        continue;

                    ProgressMaxValue++;
                }
            }
            else
            {
                ProgressMaxValue += tracks.Count;

                foreach(SectorTagType tag in _inputFormat.Info.ReadableSectorTags.OrderBy(t => t))
                {
                    switch(tag)
                    {
                        case SectorTagType.AppleSectorTag:
                        case SectorTagType.CdSectorSync:
                        case SectorTagType.CdSectorHeader:
                        case SectorTagType.CdSectorSubHeader:
                        case SectorTagType.CdSectorEdc:
                        case SectorTagType.CdSectorEccP:
                        case SectorTagType.CdSectorEccQ:
                        case SectorTagType.CdSectorEcc:
                            // This tags are inline in long sector
                            continue;
                    }

                    if(ForceChecked && !outputFormat.SupportedSectorTags.Contains(tag))
                        continue;

                    ProgressMaxValue += tracks.Count;
                }
            }

            if(_dumpHardware != null)
                ProgressMaxValue++;

            if(_aaruMetadata != null)
                ProgressMaxValue++;

            ProgressMaxValue++;
        });

        foreach(MediaTagType mediaTag in _inputFormat.Info.ReadableMediaTags.Where(mediaTag =>
            !outputFormat.SupportedMediaTags.Contains(mediaTag) && !ForceChecked))
        {
            await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                    GetMessageBoxStandard(UI.Title_Error,
                                                                        string.
                                                                            Format(UI.Converting_image_will_lose_media_tag_0,
                                                                                mediaTag), icon: Icon.Error).
                                                                    ShowWindowDialogAsync(_view));

            return;
        }

        bool useLong = _inputFormat.Info.ReadableSectorTags.Count != 0;

        foreach(SectorTagType sectorTag in _inputFormat.Info.ReadableSectorTags.Where(sectorTag =>
            !outputFormat.SupportedSectorTags.Contains(sectorTag)))
        {
            if(ForceChecked)
            {
                if(sectorTag != SectorTagType.CdTrackFlags &&
                   sectorTag != SectorTagType.CdTrackIsrc  &&
                   sectorTag != SectorTagType.CdSectorSubchannel)
                    useLong = false;

                continue;
            }

            await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                    GetMessageBoxStandard(UI.Title_Error,
                                                                        string.
                                                                            Format(UI.Converting_image_will_lose_sector_tag_0,
                                                                                sectorTag), icon: Icon.Error).
                                                                    ShowWindowDialogAsync(_view));

            return;
        }

        Dictionary<string, string> parsedOptions = new();

        /* TODO:
        if(grpOptions.Content is StackLayout stkImageOptions)
            foreach(Control option in stkImageOptions.Children)
            {
                if(cancel)
                    break;

                string value;

                switch(option)
                {
                    case CheckBox optBoolean:
                        value = optBooleanChecked?.ToString();

                        break;
                    case NumericStepper optNumber:
                        value = optNumber.Value.ToString(CultureInfo.CurrentCulture);

                        break;
                    case TextBox optString:
                        value = optString.Text;

                        break;
                    default: continue;
                }

                string key = option.ID.Substring(3);

                parsedOptions.Add(key, value);
            }
            */

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProgressText           = UI.Creating_output_image;
            Progress2Text          = "";
            Progress2Indeterminate = true;
        });

        if(!outputFormat.Create(DestinationText, _inputFormat.Info.MediaType, parsedOptions, _inputFormat.Info.Sectors,
                                _inputFormat.Info.SectorSize))
        {
            await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                    GetMessageBoxStandard(UI.Title_Error,
                                                                        string.
                                                                            Format(UI.Error_0_creating_output_image,
                                                                                outputFormat.ErrorMessage),
                                                                        icon: Icon.Error).
                                                                    ShowWindowDialogAsync(_view));

            AaruConsole.ErrorWriteLine(UI.Error_0_creating_output_image, outputFormat.ErrorMessage);

            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProgressText = UI.Setting_image_metadata;
            ProgressValue++;
            Progress2Text          = "";
            Progress2Indeterminate = true;
        });

        var metadata = new ImageInfo
        {
            Application           = "Aaru",
            ApplicationVersion    = Version.GetVersion(),
            Comments              = CommentsText,
            Creator               = CreatorText,
            DriveFirmwareRevision = DriveFirmwareRevisionText,
            DriveManufacturer     = DriveManufacturerText,
            DriveModel            = DriveModelText,
            DriveSerialNumber     = DriveSerialNumberText,
            LastMediaSequence     = (int)LastMediaSequenceValue,
            MediaBarcode          = MediaBarcodeText,
            MediaManufacturer     = MediaManufacturerText,
            MediaModel            = MediaModelText,
            MediaPartNumber       = MediaPartNumberText,
            MediaSequence         = (int)MediaSequenceValue,
            MediaSerialNumber     = MediaSerialNumberText,
            MediaTitle            = MediaTitleText
        };

        if(!_cancel)
        {
            if(!outputFormat.SetImageInfo(metadata))
            {
                if(ForceChecked != true)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                            GetMessageBoxStandard(UI.Title_Error,
                                                                                string.
                                                                                    Format(UI.Error_0_setting_metadata_not_continuing,
                                                                                        outputFormat.
                                                                                            ErrorMessage),
                                                                                icon: Icon.Error).
                                                                            ShowWindowDialogAsync(_view));

                    AaruConsole.ErrorWriteLine(UI.Error_0_setting_metadata_not_continuing, outputFormat.ErrorMessage);

                    return;
                }

                warning = true;
                AaruConsole.ErrorWriteLine(Localization.Core.Error_0_setting_metadata, outputFormat.ErrorMessage);
            }
        }

        if(tracks != null && !_cancel && outputOptical != null)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressText = UI.Setting_tracks_list;
                ProgressValue++;
                Progress2Text          = "";
                Progress2Indeterminate = true;
            });

            if(!outputOptical.SetTracks(tracks))
            {
                await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                        GetMessageBoxStandard(UI.Title_Error,
                                                                            string.
                                                                                Format(UI.Error_0_sending_tracks_list_to_output_image,
                                                                                    outputFormat.
                                                                                        ErrorMessage),
                                                                            icon: Icon.Error).
                                                                        ShowWindowDialogAsync(_view));

                AaruConsole.ErrorWriteLine(UI.Error_0_sending_tracks_list_to_output_image, outputFormat.ErrorMessage);

                return;
            }
        }

        ErrorNumber errno;

        foreach(MediaTagType mediaTag in _inputFormat.Info.ReadableMediaTags.TakeWhile(_ => !_cancel))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressText = string.Format(UI.Converting_media_tag_0, mediaTag);
                ProgressValue++;
                Progress2Text          = "";
                Progress2Indeterminate = true;
            });

            if(ForceChecked && !outputFormat.SupportedMediaTags.Contains(mediaTag))
                continue;

            errno = _inputFormat.ReadMediaTag(mediaTag, out byte[] tag);

            if(errno == ErrorNumber.NoError && outputFormat.WriteMediaTag(tag, mediaTag))
                continue;

            if(ForceChecked)
            {
                warning = true;

                if(errno == ErrorNumber.NoError)
                    AaruConsole.ErrorWriteLine(UI.Error_0_writing_media_tag, outputFormat.ErrorMessage);
                else
                    AaruConsole.ErrorWriteLine(UI.Error_0_reading_media_tag, errno);
            }
            else
            {
                if(errno == ErrorNumber.NoError)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                            GetMessageBoxStandard(UI.Title_Error,
                                                                                string.
                                                                                    Format(UI.Error_0_writing_media_tag_not_continuing,
                                                                                        outputFormat.
                                                                                            ErrorMessage),
                                                                                icon: Icon.Error).
                                                                            ShowWindowDialogAsync(_view));

                    AaruConsole.ErrorWriteLine(UI.Error_0_writing_media_tag_not_continuing, outputFormat.ErrorMessage);
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                            GetMessageBoxStandard(UI.Title_Error,
                                                                                string.
                                                                                    Format(UI.Error_0_reading_media_tag_not_continuing,
                                                                                        errno),
                                                                                icon: Icon.Error).
                                                                            ShowWindowDialogAsync(_view));

                    AaruConsole.ErrorWriteLine(UI.Error_0_reading_media_tag_not_continuing, errno);
                }

                return;
            }
        }

        ulong doneSectors = 0;

        if(tracks == null && !_cancel)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressText = string.Format(UI.Setting_geometry_to_0_cylinders_1_heads_and_2_sectors_per_track,
                                             _inputFormat.Info.Cylinders, _inputFormat.Info.Heads,
                                             _inputFormat.Info.SectorsPerTrack);

                ProgressValue++;
                Progress2Text          = "";
                Progress2Indeterminate = true;
            });

            if(!outputFormat.SetGeometry(_inputFormat.Info.Cylinders, _inputFormat.Info.Heads,
                                         _inputFormat.Info.SectorsPerTrack))
            {
                warning = true;

                AaruConsole.ErrorWriteLine(UI.Error_0_setting_geometry_image_may_be_incorrect_continuing,
                                           outputFormat.ErrorMessage);
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressText = UI.Converting_sectors;
                ProgressValue++;
                Progress2Text          = "";
                Progress2Indeterminate = false;
                Progress2MaxValue      = (int)(_inputFormat.Info.Sectors / SectorsValue);
            });

            while(doneSectors < _inputFormat.Info.Sectors)
            {
                if(_cancel)
                    break;

                byte[] sector;

                uint sectorsToDo;

                if(_inputFormat.Info.Sectors - doneSectors >= (ulong)SectorsValue)
                    sectorsToDo = (uint)SectorsValue;
                else
                    sectorsToDo = (uint)(_inputFormat.Info.Sectors - doneSectors);

                ulong sectors = doneSectors;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Progress2Text = string.Format(UI.Converting_sectors_0_to_1_2_done, sectors, sectors + sectorsToDo,
                                                  sectors / (double)_inputFormat.Info.Sectors);

                    Progress2Value = (int)(sectors / SectorsValue);
                });

                bool result;

                if(useLong)
                {
                    errno = sectorsToDo == 1
                                ? _inputFormat.ReadSectorLong(doneSectors, out sector)
                                : _inputFormat.ReadSectorsLong(doneSectors, sectorsToDo, out sector);

                    if(errno == ErrorNumber.NoError)
                    {
                        result = sectorsToDo == 1
                                     ? outputFormat.WriteSectorLong(sector, doneSectors)
                                     : outputFormat.WriteSectorsLong(sector, doneSectors, sectorsToDo);
                    }
                    else
                    {
                        result = true;

                        if(ForceChecked)
                        {
                            warning = true;

                            AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_continuing, errno, doneSectors);
                        }
                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                                  GetMessageBoxStandard(UI.Title_Error,
                                                                                      string.
                                                                                          Format(UI.Error_0_reading_sector_1_not_continuing,
                                                                                              errno,
                                                                                              doneSectors),
                                                                                      icon: Icon.Error).
                                                                                  ShowWindowDialogAsync(_view));

                            AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_not_continuing, errno, doneSectors);

                            return;
                        }
                    }
                }
                else
                {
                    errno = sectorsToDo == 1
                                ? _inputFormat.ReadSector(doneSectors, out sector)
                                : _inputFormat.ReadSectors(doneSectors, sectorsToDo, out sector);

                    if(errno == ErrorNumber.NoError)
                    {
                        result = sectorsToDo == 1
                                     ? outputFormat.WriteSector(sector, doneSectors)
                                     : outputFormat.WriteSectors(sector, doneSectors, sectorsToDo);
                    }
                    else
                    {
                        result = true;

                        if(ForceChecked)
                        {
                            warning = true;

                            AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_continuing, errno, doneSectors);
                        }
                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                                  GetMessageBoxStandard(UI.Title_Error,
                                                                                      string.
                                                                                          Format(UI.Error_0_reading_sector_1_not_continuing,
                                                                                              errno,
                                                                                              doneSectors),
                                                                                      icon: Icon.Error).
                                                                                  ShowWindowDialogAsync(_view));

                            AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_not_continuing, errno, doneSectors);

                            return;
                        }
                    }
                }

                if(!result)
                {
                    if(ForceChecked)
                    {
                        warning = true;

                        AaruConsole.ErrorWriteLine(UI.Error_0_writing_sector_1_continuing, outputFormat.ErrorMessage,
                                                   doneSectors);
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                              GetMessageBoxStandard(UI.Title_Error,
                                                                                  string.
                                                                                      Format(UI.Error_0_writing_sector_1_not_continuing,
                                                                                          outputFormat.
                                                                                              ErrorMessage,
                                                                                          doneSectors),
                                                                                  icon: Icon.Error).
                                                                              ShowWindowDialogAsync(_view));

                        AaruConsole.ErrorWriteLine(UI.Error_0_writing_sector_1_not_continuing,
                                                   outputFormat.ErrorMessage, doneSectors);

                        return;
                    }
                }

                doneSectors += sectorsToDo;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Progress2Text = string.Format(UI.Converting_sectors_0_to_1_2_done, _inputFormat.Info.Sectors,
                                              _inputFormat.Info.Sectors, 1.0);

                Progress2Value = Progress2MaxValue;
            });

            Dictionary<byte, string> isrcs                     = new();
            Dictionary<byte, byte>   trackFlags                = new();
            string                   mcn                       = null;
            HashSet<int>             subchannelExtents         = new();
            Dictionary<byte, int>    smallestPregapLbaPerTrack = new();

            foreach(SectorTagType tag in _inputFormat.Info.ReadableSectorTags.
                                                      Where(t => t == SectorTagType.CdTrackIsrc).
                                                      OrderBy(t => t))
            {
                foreach(Track track in inputOptical.Tracks)
                {
                    errno = _inputFormat.ReadSectorTag(track.Sequence, tag, out byte[] isrc);

                    if(errno != ErrorNumber.NoError)
                        continue;

                    isrcs[(byte)track.Sequence] = Encoding.UTF8.GetString(isrc);
                }
            }

            foreach(SectorTagType tag in _inputFormat.Info.ReadableSectorTags.
                                                      Where(t => t == SectorTagType.CdTrackFlags).
                                                      OrderBy(t => t))
            {
                foreach(Track track in inputOptical.Tracks)
                {
                    errno = _inputFormat.ReadSectorTag(track.Sequence, tag, out byte[] flags);

                    if(errno != ErrorNumber.NoError)
                        continue;

                    trackFlags[(byte)track.Sequence] = flags[0];
                }
            }

            for(ulong s = 0; s < _inputFormat.Info.Sectors; s++)
            {
                if(s > int.MaxValue)
                    break;

                subchannelExtents.Add((int)s);
            }

            foreach(SectorTagType tag in _inputFormat.Info.ReadableSectorTags.TakeWhile(_ => useLong && !_cancel))
            {
                switch(tag)
                {
                    case SectorTagType.AppleSectorTag:
                    case SectorTagType.CdSectorSync:
                    case SectorTagType.CdSectorHeader:
                    case SectorTagType.CdSectorSubHeader:
                    case SectorTagType.CdSectorEdc:
                    case SectorTagType.CdSectorEccP:
                    case SectorTagType.CdSectorEccQ:
                    case SectorTagType.CdSectorEcc:
                        // This tags are inline in long sector
                        continue;
                }

                if(ForceChecked && !outputFormat.SupportedSectorTags.Contains(tag))
                    continue;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressText = string.Format(UI.Converting_tag_0, tag);
                    ProgressValue++;
                    Progress2Text          = "";
                    Progress2Indeterminate = false;
                    Progress2MaxValue      = (int)(_inputFormat.Info.Sectors / SectorsValue);
                });

                doneSectors = 0;

                while(doneSectors < _inputFormat.Info.Sectors)
                {
                    if(_cancel)
                        break;

                    byte[] sector;

                    uint sectorsToDo;

                    if(_inputFormat.Info.Sectors - doneSectors >= (ulong)SectorsValue)
                        sectorsToDo = (uint)SectorsValue;
                    else
                        sectorsToDo = (uint)(_inputFormat.Info.Sectors - doneSectors);

                    ulong sectors = doneSectors;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Progress2Text = string.Format(UI.Converting_tag_0_for_sectors_1_to_2_3_done,
                                                      sectors / (double)_inputFormat.Info.Sectors, sectors,
                                                      sectors + sectorsToDo,
                                                      sectors / (double)_inputFormat.Info.Sectors);

                        Progress2Value = (int)(sectors / SectorsValue);
                    });

                    bool result;

                    if(sectorsToDo == 1)
                    {
                        errno = _inputFormat.ReadSectorTag(doneSectors, tag, out sector);

                        if(errno == ErrorNumber.NoError)
                        {
                            Track track = tracks.LastOrDefault(t => t.StartSector >= doneSectors);

                            if(tag == SectorTagType.CdSectorSubchannel && track != null)
                            {
                                bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                    MmcSubchannel.Raw, sector, doneSectors, 1, null, isrcs, (byte)track.Sequence,
                                    ref mcn, tracks.ToArray(), subchannelExtents, false,
                                    outputFormat as IWritableOpticalImage, false, false, null, null,
                                    smallestPregapLbaPerTrack, false, out _);

                                if(indexesChanged)
                                    outputOptical.SetTracks(tracks.ToList());

                                result = true;
                            }
                            else
                                result = outputFormat.WriteSectorTag(sector, doneSectors, tag);
                        }
                        else
                        {
                            result = true;

                            if(ForceChecked)
                            {
                                warning = true;

                                AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_continuing, errno, doneSectors);
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                          GetMessageBoxStandard(UI.Title_Error,
                                                                              string.
                                                                                  Format(UI.Error_0_reading_sector_1_not_continuing,
                                                                                      errno, doneSectors),
                                                                              icon: Icon.Error).
                                                                          ShowWindowDialogAsync(_view));

                                AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_not_continuing, errno,
                                                           doneSectors);

                                return;
                            }
                        }
                    }
                    else
                    {
                        errno = _inputFormat.ReadSectorsTag(doneSectors, sectorsToDo, tag, out sector);

                        if(errno == ErrorNumber.NoError)
                        {
                            Track track = tracks.LastOrDefault(t => t.StartSector >= doneSectors);

                            if(tag == SectorTagType.CdSectorSubchannel && track != null)

                            {
                                bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                    MmcSubchannel.Raw, sector, doneSectors, sectorsToDo, null, isrcs,
                                    (byte)track.Sequence, ref mcn, tracks.ToArray(), subchannelExtents, false,
                                    outputFormat as IWritableOpticalImage, false, false, null, null,
                                    smallestPregapLbaPerTrack, false, out _);

                                if(indexesChanged)
                                    outputOptical.SetTracks(tracks.ToList());

                                result = true;
                            }
                            else
                                result = outputFormat.WriteSectorsTag(sector, doneSectors, sectorsToDo, tag);
                        }
                        else
                        {
                            result = true;

                            if(ForceChecked)
                            {
                                warning = true;

                                AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_continuing, errno, doneSectors);
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                          GetMessageBoxStandard(UI.Title_Error,
                                                                              string.
                                                                                  Format(UI.Error_0_reading_sector_1_not_continuing,
                                                                                      errno, doneSectors),
                                                                              icon: Icon.Error).
                                                                          ShowWindowDialogAsync(_view));

                                AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_not_continuing, errno,
                                                           doneSectors);

                                return;
                            }
                        }
                    }

                    if(!result)
                    {
                        if(ForceChecked)
                        {
                            warning = true;

                            AaruConsole.ErrorWriteLine(UI.Error_0_writing_sector_1_continuing,
                                                       outputFormat.ErrorMessage, doneSectors);
                        }
                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                                  GetMessageBoxStandard(UI.Title_Error,
                                                                                      string.
                                                                                          Format(UI.Error_0_writing_sector_1_not_continuing,
                                                                                              outputFormat.
                                                                                                  ErrorMessage,
                                                                                              doneSectors),
                                                                                      icon: Icon.Error).
                                                                                  ShowWindowDialogAsync(_view));

                            AaruConsole.ErrorWriteLine(UI.Error_0_writing_sector_1_not_continuing,
                                                       outputFormat.ErrorMessage, doneSectors);

                            return;
                        }
                    }

                    doneSectors += sectorsToDo;
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Progress2Text = string.Format(UI.Converting_tag_0_for_sectors_1_to_2_3_done, tag,
                                                  _inputFormat.Info.Sectors, _inputFormat.Info.Sectors, 1.0);

                    Progress2Value = Progress2MaxValue;
                });

                if(isrcs.Count > 0)
                {
                    foreach(KeyValuePair<byte, string> isrc in isrcs)
                    {
                        outputOptical.WriteSectorTag(Encoding.UTF8.GetBytes(isrc.Value), isrc.Key,
                                                     SectorTagType.CdTrackIsrc);
                    }
                }

                if(trackFlags.Count > 0)
                {
                    foreach(KeyValuePair<byte, byte> flags in trackFlags)
                    {
                        outputOptical.WriteSectorTag(new[]
                        {
                            flags.Value
                        }, flags.Key, SectorTagType.CdTrackFlags);
                    }
                }

                if(mcn != null)
                    outputOptical.WriteMediaTag(Encoding.UTF8.GetBytes(mcn), MediaTagType.CD_MCN);
            }
        }
        else
        {
            foreach(Track track in tracks.TakeWhile(_ => !_cancel))
            {
                doneSectors = 0;
                ulong trackSectors = track.EndSector - track.StartSector + 1;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressText = string.Format(UI.Converting_sectors_in_track_0, track.Sequence);
                    ProgressValue++;
                    Progress2Text          = "";
                    Progress2Indeterminate = false;
                    Progress2MaxValue      = (int)(trackSectors / SectorsValue);
                });

                while(doneSectors < trackSectors)
                {
                    if(_cancel)
                        break;

                    byte[] sector;

                    uint sectorsToDo;

                    if(trackSectors - doneSectors >= (ulong)SectorsValue)
                        sectorsToDo = (uint)SectorsValue;
                    else
                        sectorsToDo = (uint)(trackSectors - doneSectors);

                    ulong sectors = doneSectors;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Progress2Text = string.Format(UI.Converting_sectors_0_to_1_in_track_2_3_done,
                                                      sectors + track.StartSector,
                                                      sectors + sectorsToDo + track.StartSector, track.Sequence,
                                                      (sectors + track.StartSector) /
                                                      (double)_inputFormat.Info.Sectors);

                        Progress2Value = (int)(sectors / SectorsValue);
                    });

                    bool result;

                    if(useLong)
                    {
                        errno = sectorsToDo == 1
                                    ? _inputFormat.ReadSectorLong(doneSectors + track.StartSector, out sector)
                                    : _inputFormat.ReadSectorsLong(doneSectors + track.StartSector, sectorsToDo,
                                                                   out sector);

                        if(errno == ErrorNumber.NoError)
                        {
                            result = sectorsToDo == 1
                                         ? outputFormat.WriteSectorLong(sector, doneSectors + track.StartSector)
                                         : outputFormat.WriteSectorsLong(sector, doneSectors + track.StartSector,
                                                                         sectorsToDo);
                        }
                        else
                        {
                            result = true;

                            if(ForceChecked)
                            {
                                warning = true;

                                AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_continuing, errno, doneSectors);
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                          GetMessageBoxStandard(UI.Title_Error,
                                                                              string.
                                                                                  Format(UI.Error_0_reading_sector_1_not_continuing,
                                                                                      errno, doneSectors),
                                                                              icon: Icon.Error).
                                                                          ShowWindowDialogAsync(_view));

                                return;
                            }
                        }
                    }
                    else
                    {
                        errno = sectorsToDo == 1
                                    ? _inputFormat.ReadSector(doneSectors + track.StartSector, out sector)
                                    : _inputFormat.ReadSectors(doneSectors + track.StartSector, sectorsToDo,
                                                               out sector);

                        if(errno == ErrorNumber.NoError)
                        {
                            result = sectorsToDo == 1
                                         ? outputFormat.WriteSector(sector, doneSectors + track.StartSector)
                                         : outputFormat.WriteSectors(sector, doneSectors + track.StartSector,
                                                                     sectorsToDo);
                        }
                        else
                        {
                            result = true;

                            if(ForceChecked)
                            {
                                warning = true;

                                AaruConsole.ErrorWriteLine(UI.Error_0_reading_sector_1_continuing, errno, doneSectors);
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                          GetMessageBoxStandard(UI.Title_Error,
                                                                              string.
                                                                                  Format(UI.Error_0_reading_sector_1_not_continuing,
                                                                                      errno, doneSectors),
                                                                              icon: Icon.Error).
                                                                          ShowWindowDialogAsync(_view));

                                return;
                            }
                        }
                    }

                    if(!result)
                    {
                        if(ForceChecked)
                        {
                            warning = true;

                            AaruConsole.ErrorWriteLine(UI.Error_0_writing_sector_1_continuing,
                                                       outputFormat.ErrorMessage, doneSectors);
                        }
                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                                  GetMessageBoxStandard(UI.Title_Error,
                                                                                      string.
                                                                                          Format(UI.Error_0_writing_sector_1_not_continuing,
                                                                                              outputFormat.
                                                                                                  ErrorMessage,
                                                                                              doneSectors),
                                                                                      icon: Icon.Error).
                                                                                  ShowWindowDialogAsync(_view));

                            return;
                        }
                    }

                    doneSectors += sectorsToDo;
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Progress2Text = string.Format(UI.Converting_sectors_0_to_1_in_track_2_3_done, _inputFormat.Info.Sectors,
                                              _inputFormat.Info.Sectors, tracks.Count, 1.0);

                Progress2Value = Progress2MaxValue;
            });

            foreach(SectorTagType tag in _inputFormat.Info.ReadableSectorTags.OrderBy(t => t).
                                                      TakeWhile(_ => useLong && !_cancel))
            {
                switch(tag)
                {
                    case SectorTagType.AppleSectorTag:
                    case SectorTagType.CdSectorSync:
                    case SectorTagType.CdSectorHeader:
                    case SectorTagType.CdSectorSubHeader:
                    case SectorTagType.CdSectorEdc:
                    case SectorTagType.CdSectorEccP:
                    case SectorTagType.CdSectorEccQ:
                    case SectorTagType.CdSectorEcc:
                        // This tags are inline in long sector
                        continue;
                }

                if(ForceChecked && !outputFormat.SupportedSectorTags.Contains(tag))
                    continue;

                foreach(Track track in tracks.TakeWhile(_ => !_cancel))
                {
                    doneSectors = 0;
                    ulong  trackSectors = track.EndSector - track.StartSector + 1;
                    byte[] sector;
                    bool   result;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ProgressText = $"Converting tag {tag} in track {track.Sequence}.";
                        ProgressValue++;
                        Progress2Text          = "";
                        Progress2Indeterminate = false;
                        Progress2MaxValue      = (int)(trackSectors / SectorsValue);
                    });

                    switch(tag)
                    {
                        case SectorTagType.CdTrackFlags:
                        case SectorTagType.CdTrackIsrc:

                            errno = _inputFormat.ReadSectorTag(track.Sequence, tag, out sector);

                            if(errno == ErrorNumber.NoError)
                                result = outputFormat.WriteSectorTag(sector, track.Sequence, tag);
                            else
                            {
                                if(ForceChecked)
                                {
                                    warning = true;

                                    AaruConsole.ErrorWriteLine(UI.Error_0_reading_media_tag, errno);
                                }
                                else
                                {
                                    await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                              GetMessageBoxStandard(UI.Title_Error,
                                                                                  string.
                                                                                      Format(UI.Error_0_reading_media_tag_not_continuing,
                                                                                          errno),
                                                                                  icon: Icon.Error).
                                                                              ShowWindowDialogAsync(_view));

                                    return;
                                }

                                continue;
                            }

                            if(!result)
                            {
                                if(ForceChecked)
                                {
                                    warning = true;

                                    AaruConsole.ErrorWriteLine(UI.Error_0_writing_tag_continuing,
                                                               outputFormat.ErrorMessage);
                                }
                                else
                                {
                                    await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                              GetMessageBoxStandard(UI.Title_Error,
                                                                                  string.
                                                                                      Format(UI.Error_0_writing_tag_not_continuing,
                                                                                          outputFormat.
                                                                                              ErrorMessage),
                                                                                  icon: Icon.Error).
                                                                              ShowWindowDialogAsync(_view));

                                    return;
                                }
                            }

                            continue;
                    }

                    while(doneSectors < trackSectors)
                    {
                        if(_cancel)
                            break;

                        uint sectorsToDo;

                        if(trackSectors - doneSectors >= (ulong)SectorsValue)
                            sectorsToDo = (uint)SectorsValue;
                        else
                            sectorsToDo = (uint)(trackSectors - doneSectors);

                        ulong sectors = doneSectors;

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Progress2Text = string.Format(UI.Converting_tag_0_for_sectors_1_to_2_in_track_3_4_done, tag,
                                                          sectors + track.StartSector,
                                                          sectors + sectorsToDo + track.StartSector, track.Sequence,
                                                          (sectors + track.StartSector) /
                                                          (double)_inputFormat.Info.Sectors);

                            Progress2Value = (int)(sectors / SectorsValue);
                        });

                        errno = sectorsToDo == 1
                                    ? _inputFormat.ReadSectorTag(doneSectors + track.StartSector, tag, out sector)
                                    : _inputFormat.ReadSectorsTag(doneSectors + track.StartSector, sectorsToDo, tag,
                                                                  out sector);

                        if(errno == ErrorNumber.NoError)
                        {
                            result = sectorsToDo == 1
                                         ? outputFormat.WriteSectorTag(sector, doneSectors + track.StartSector, tag)
                                         : outputFormat.WriteSectorsTag(sector, doneSectors + track.StartSector,
                                                                        sectorsToDo, tag);
                        }
                        else
                        {
                            result = true;

                            if(ForceChecked)
                            {
                                warning = true;

                                AaruConsole.ErrorWriteLine(UI.Error_0_reading_tag_for_sector_1_continuing, errno,
                                                           doneSectors);
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                          GetMessageBoxStandard(UI.Title_Error,
                                                                              string.
                                                                                  Format(UI.Error_0_reading_tag_for_sector_1_not_continuing,
                                                                                      errno, doneSectors),
                                                                              icon: Icon.Error).
                                                                          ShowWindowDialogAsync(_view));

                                return;
                            }
                        }

                        if(!result)
                        {
                            if(ForceChecked)
                            {
                                warning = true;

                                AaruConsole.ErrorWriteLine(UI.Error_0_writing_tag_for_sector_1_continuing,
                                                           outputFormat.ErrorMessage, doneSectors);
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                          GetMessageBoxStandard(UI.Title_Error,
                                                                              string.
                                                                                  Format(UI.Error_0_writing_tag_for_sector_1_not_continuing,
                                                                                      outputFormat.
                                                                                          ErrorMessage,
                                                                                      doneSectors),
                                                                              icon: Icon.Error).
                                                                          ShowWindowDialogAsync(_view));

                                return;
                            }
                        }

                        doneSectors += sectorsToDo;
                    }
                }
            }
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Progress2Visible = false;
            Progress2Visible = false;
        });

        bool ret;

        if(_dumpHardware != null && !_cancel)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressText = UI.Writing_dump_hardware_list;
                ProgressValue++;
            });

            ret = outputFormat.SetDumpHardware(_dumpHardware);

            if(!ret)
                AaruConsole.WriteLine(UI.Error_0_writing_dump_hardware_list_to_output_image, outputFormat.ErrorMessage);
        }

        ret = false;

        if(_aaruMetadata != null && !_cancel)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressText = UI.Writing_Aaru_Metadata_to_output_image;
                ProgressValue++;
            });

            outputFormat.SetMetadata(_aaruMetadata);

            if(!ret)
                AaruConsole.WriteLine(UI.Error_0_writing_Aaru_Metadata_to_output_image, outputFormat.ErrorMessage);
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProgressText          = UI.Closing_output_image;
            ProgressIndeterminate = true;
        });

        if(_cancel)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MessageBoxManager.
                      GetMessageBoxStandard(UI.Title_Error, UI.Operation_canceled_the_output_file_is_not_correct,
                                            icon: Icon.Error).
                      ShowWindowDialogAsync(_view);

                CloseVisible    = true;
                StopVisible     = false;
                ProgressVisible = false;
            });

            return;
        }

        if(!outputFormat.Close())
        {
            await Dispatcher.UIThread.InvokeAsync(async () => await MessageBoxManager.
                                                                    GetMessageBoxStandard(UI.Title_Error,
                                                                        string.
                                                                            Format(UI.Error_0_closing_output_image_Contents_are_not_correct,
                                                                                outputFormat.ErrorMessage),
                                                                        icon: Icon.Error).
                                                                    ShowWindowDialogAsync(_view));

            return;
        }

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await MessageBoxManager.GetMessageBoxStandard(warning ? UI.Title_Warning : UI.Title_Conversion_success,
                                                          warning
                                                              ? UI.Some_warnings_happened_Check_console
                                                              : UI.Image_converted_successfully,
                                                          icon: warning ? Icon.Warning : Icon.Info).
                                    ShowWindowDialogAsync(_view);

            CloseVisible    = true;
            StopVisible     = false;
            ProgressVisible = false;
        });

        Statistics.AddCommand("convert-image");
    }

    void ExecuteCloseCommand() => _view.Close();

    internal void ExecuteStopCommand()
    {
        _cancel     = true;
        StopEnabled = false;
    }

    /* TODO
            void OnCmbFormatSelectedIndexChanged()
            {
                txtDestination.Text = "";

                if(!(cmbFormat.SelectedValue is IWritableImage plugin))
                {
                    grpOptions.Visible     = false;
                    btnDestination.Enabled = false;

                    return;
                }

                btnDestination.Enabled = true;

                if(!plugin.SupportedOptions.Any())
                {
                    grpOptions.Content = null;
                    grpOptions.Visible = false;

                    return;
                }

                chkForce.Visible = false;

                foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags)
                {
                    if(plugin.SupportedMediaTags.Contains(mediaTag))
                        continue;

                    chkForce.Visible = true;
                    ForceChecked = true;

                    break;
                }

                foreach(SectorTagType sectorTag in inputFormat.Info.ReadableSectorTags)
                {
                    if(plugin.SupportedSectorTags.Contains(sectorTag))
                        continue;

                    chkForce.Visible = true;
                    ForceChecked = true;

                    break;
                }

                grpOptions.Visible = true;

                var stkImageOptions = new StackLayout
                {
                    Orientation = Orientation.Vertical
                };

                foreach((string name, Type type, string description, object @default) option in plugin.SupportedOptions)
                    switch(option.type.ToString())
                    {
                        case "System.Boolean":
                            var optBoolean = new CheckBox();
                            optBoolean.ID      = "opt" + option.name;
                            optBoolean.Text    = option.description;
                            optBooleanChecked = (bool)option.@default;
                            stkImageOptions.Items.Add(optBoolean);

                            break;
                        case "System.SByte":
                        case "System.Int16":
                        case "System.Int32":
                        case "System.Int64":
                            var stkNumber = new StackLayout();
                            stkNumber.Orientation = Orientation.Horizontal;
                            var optNumber = new NumericStepper();
                            optNumber.ID    = "opt" + option.name;
                            optNumber.Value = Convert.ToDouble(option.@default);
                            stkNumber.Items.Add(optNumber);
                            var lblNumber = new Label();
                            lblNumber.Text = option.description;
                            stkNumber.Items.Add(lblNumber);
                            stkImageOptions.Items.Add(stkNumber);

                            break;
                        case "System.Byte":
                        case "System.UInt16":
                        case "System.UInt32":
                        case "System.UInt64":
                            var stkUnsigned = new StackLayout();
                            stkUnsigned.Orientation = Orientation.Horizontal;
                            var optUnsigned = new NumericStepper();
                            optUnsigned.ID       = "opt" + option.name;
                            optUnsigned.MinValue = 0;
                            optUnsigned.Value    = Convert.ToDouble(option.@default);
                            stkUnsigned.Items.Add(optUnsigned);
                            var lblUnsigned = new Label();
                            lblUnsigned.Text = option.description;
                            stkUnsigned.Items.Add(lblUnsigned);
                            stkImageOptions.Items.Add(stkUnsigned);

                            break;
                        case "System.Single":
                        case "System.Double":
                            var stkFloat = new StackLayout();
                            stkFloat.Orientation = Orientation.Horizontal;
                            var optFloat = new NumericStepper();
                            optFloat.ID            = "opt" + option.name;
                            optFloat.DecimalPlaces = 2;
                            optFloat.Value         = Convert.ToDouble(option.@default);
                            stkFloat.Items.Add(optFloat);
                            var lblFloat = new Label();
                            lblFloat.Text = option.description;
                            stkFloat.Items.Add(lblFloat);
                            stkImageOptions.Items.Add(stkFloat);

                            break;
                        case "System.Guid":
                            // TODO
                            break;
                        case "System.String":
                            var stkString = new StackLayout();
                            stkString.Orientation = Orientation.Horizontal;
                            var lblString = new Label();
                            lblString.Text = option.description;
                            stkString.Items.Add(lblString);
                            var optString = new TextBox();
                            optString.ID   = "opt" + option.name;
                            optString.Text = (string)option.@default;
                            stkString.Items.Add(optString);
                            stkImageOptions.Items.Add(stkString);

                            break;
                    }

                grpOptions.Content = stkImageOptions;
            }
    */
    async Task ExecuteDestinationCommand()
    {
        if(SelectedPlugin is null)
            return;

        var dlgDestination = new SaveFileDialog
        {
            Title = UI.Dialog_Choose_destination_file
        };

        dlgDestination.Filters?.Add(new FileDialogFilter
        {
            Name       = SelectedPlugin.Plugin.Name,
            Extensions = SelectedPlugin.Plugin.KnownExtensions.ToList()
        });

        string result = await dlgDestination.ShowAsync(_view);

        if(result is null || result.Length != 1)
        {
            DestinationText = "";

            return;
        }

        if(string.IsNullOrEmpty(Path.GetExtension(result)))
            result += SelectedPlugin.Plugin.KnownExtensions.First();

        DestinationText = result;
    }

    void ExecuteCreatorCommand() => CreatorText = _inputFormat.Info.Creator;

    void ExecuteMediaTitleCommand() => MediaTitleText = _inputFormat.Info.MediaTitle;

    void ExecuteCommentsCommand() => CommentsText = _inputFormat.Info.Comments;

    void ExecuteMediaManufacturerCommand() => MediaManufacturerText = _inputFormat.Info.MediaManufacturer;

    void ExecuteMediaModelCommand() => MediaModelText = _inputFormat.Info.MediaModel;

    void ExecuteMediaSerialNumberCommand() => MediaSerialNumberText = _inputFormat.Info.MediaSerialNumber;

    void ExecuteMediaBarcodeCommand() => MediaBarcodeText = _inputFormat.Info.MediaBarcode;

    void ExecuteMediaPartNumberCommand() => MediaPartNumberText = _inputFormat.Info.MediaPartNumber;

    void ExecuteMediaSequenceCommand() => MediaSequenceValue = _inputFormat.Info.MediaSequence;

    void ExecuteLastMediaSequenceCommand() => LastMediaSequenceValue = _inputFormat.Info.LastMediaSequence;

    void ExecuteDriveManufacturerCommand() => DriveManufacturerText = _inputFormat.Info.DriveManufacturer;

    void ExecuteDriveModelCommand() => DriveModelText = _inputFormat.Info.DriveModel;

    void ExecuteDriveSerialNumberCommand() => DriveSerialNumberText = _inputFormat.Info.DriveSerialNumber;

    void ExecuteDriveFirmwareRevisionCommand() => DriveFirmwareRevisionText = _inputFormat.Info.DriveFirmwareRevision;

    void ExecuteAaruMetadataFromImageCommand()
    {
        MetadataJsonText = UI._From_image_;
        _aaruMetadata    = _inputFormat.AaruMetadata;
    }

    async Task ExecuteAaruMetadataCommand()
    {
        _aaruMetadata    = null;
        MetadataJsonText = "";

        var dlgMetadata = new OpenFileDialog
        {
            Title = UI.Dialog_Choose_existing_metadata_sidecar
        };

        dlgMetadata.Filters?.Add(new FileDialogFilter
        {
            Name = UI.Dialog_Aaru_Metadata,
            Extensions = new List<string>(new[]
            {
                ".json"
            })
        });

        string[] result = await dlgMetadata.ShowAsync(_view);

        if(result is null || result.Length != 1)
            return;

        try
        {
            var fs = new FileStream(result[0], FileMode.Open);

            _aaruMetadata =
                (await JsonSerializer.DeserializeAsync(fs, typeof(MetadataJson), MetadataJsonContext.Default) as
                     MetadataJson)?.AaruMetadata;

            fs.Close();
            MetadataJsonText = result[0];
        }
        catch
        {
            await MessageBoxManager.GetMessageBoxStandard(UI.Title_Error, UI.Incorrect_metadata_sidecar_file,
                                                          icon: Icon.Error).
                                    ShowWindowDialogAsync(_view);
        }
    }

    void ExecuteResumeFileFromImageCommand()
    {
        ResumeFileText = UI._From_image_;
        _dumpHardware  = _inputFormat.DumpHardware;
    }

    async Task ExecuteResumeFileCommand()
    {
        _dumpHardware  = null;
        ResumeFileText = "";

        var dlgMetadata = new OpenFileDialog
        {
            Title = UI.Dialog_Choose_existing_resume_file
        };

        dlgMetadata.Filters?.Add(new FileDialogFilter
        {
            Name = UI.Dialog_Choose_existing_resume_file,
            Extensions = new List<string>(new[]
            {
                ".json"
            })
        });

        string[] result = await dlgMetadata.ShowAsync(_view);

        if(result is null || result.Length != 1)
            return;

        try
        {
            var fs = new FileStream(result[0], FileMode.Open);

            Resume resume =
                (await JsonSerializer.DeserializeAsync(fs, typeof(ResumeJson),
                                                       ResumeJsonContext.Default) as ResumeJson)?.Resume;

            fs.Close();

            if(resume?.Tries?.Any() == false)
            {
                _dumpHardware  = resume.Tries;
                ResumeFileText = result[0];
            }
            else
            {
                await MessageBoxManager.
                      GetMessageBoxStandard(UI.Title_Error, UI.Resume_file_does_not_contain_dump_hardware_information,
                                            icon: Icon.Error).
                      ShowWindowDialogAsync(_view);
            }
        }
        catch
        {
            await MessageBoxManager.GetMessageBoxStandard(UI.Title_Error, UI.Incorrect_resume_file, icon: Icon.Error).
                                    ShowWindowDialogAsync(_view);
        }
    }
}