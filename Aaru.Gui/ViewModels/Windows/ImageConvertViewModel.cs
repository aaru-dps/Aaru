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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Core;
using Aaru.Gui.Models;
using Aaru.Localization;
using Aaru.Logging;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Sentry;
using Convert = Aaru.Core.Image.Convert;

namespace Aaru.Gui.ViewModels.Windows;

[SuppressMessage("ReSharper", "AsyncVoidLambda", Justification = "Event handler lambdas must be async void.")]
public sealed partial class ImageConvertViewModel : ViewModelBase
{
    readonly IMediaImage _inputFormat;
    readonly Window      _view;
    Metadata             _aaruMetadata;
    [ObservableProperty]
    bool _aaruMetadataFromImageVisible;
    volatile bool _cancel;
    [ObservableProperty]
    bool _closeVisible;
    [ObservableProperty]
    string _commentsText;
    [ObservableProperty]
    bool _commentsVisible;
    [ObservableProperty]
    string _creatorText;
    [ObservableProperty]
    bool _creatorVisible;
    [ObservableProperty]
    bool _destinationEnabled;
    [ObservableProperty]
    string _destinationText;
    [ObservableProperty]
    bool _destinationVisible;
    [ObservableProperty]
    string _driveFirmwareRevisionText;
    [ObservableProperty]
    bool _driveFirmwareRevisionVisible;
    [ObservableProperty]
    string _driveManufacturerText;
    [ObservableProperty]
    bool _driveManufacturerVisible;
    [ObservableProperty]
    string _driveModelText;
    [ObservableProperty]
    bool _driveModelVisible;
    [ObservableProperty]
    string _driveSerialNumberText;
    [ObservableProperty]
    bool _driveSerialNumberVisible;
    [ObservableProperty]
    bool _forceChecked;
    [ObservableProperty]
    bool _formatReadOnly;
    [ObservableProperty]
    double _lastMediaSequenceValue;
    [ObservableProperty]
    bool _lastMediaSequenceVisible;
    [ObservableProperty]
    string _mediaBarcodeText;
    [ObservableProperty]
    bool _mediaBarcodeVisible;
    [ObservableProperty]
    string _mediaManufacturerText;
    [ObservableProperty]
    bool _mediaManufacturerVisible;
    [ObservableProperty]
    string _mediaModelText;
    [ObservableProperty]
    bool _mediaModelVisible;
    [ObservableProperty]
    string _mediaPartNumberText;
    [ObservableProperty]
    bool _mediaPartNumberVisible;
    [ObservableProperty]
    double _mediaSequenceValue;
    [ObservableProperty]
    bool _mediaSequenceVisible;
    [ObservableProperty]
    string _mediaSerialNumberText;
    [ObservableProperty]
    bool _mediaSerialNumberVisible;
    [ObservableProperty]
    string _mediaTitleText;
    [ObservableProperty]
    bool _mediaTitleVisible;
    [ObservableProperty]
    string _metadataJsonText;
    [ObservableProperty]
    bool _optionsVisible;
    [ObservableProperty]
    bool _progress1Visible;
    [ObservableProperty]
    bool _progress2Indeterminate;
    [ObservableProperty]
    double _progress2MaxValue;
    [ObservableProperty]
    string _progress2Text;
    [ObservableProperty]
    double _progress2Value;
    [ObservableProperty]
    bool _progress2Visible;
    [ObservableProperty]
    bool _progressIndeterminate;
    [ObservableProperty]
    double _progressMaxValue;
    [ObservableProperty]
    string _progressText;
    [ObservableProperty]
    double _progressValue;
    [ObservableProperty]
    bool _progressVisible;
    [ObservableProperty]
    bool _resumeFileFromImageVisible;
    [ObservableProperty]
    string _resumeFileText;
    [ObservableProperty]
    double _sectorsValue;
    [ObservableProperty]
    ImagePluginModel _selectedPlugin;
    [ObservableProperty]
    string _sourceText;
    [ObservableProperty]
    bool _startVisible;
    [ObservableProperty]
    bool _stopEnabled;
    [ObservableProperty]
    bool _stopVisible;
    Resume  _resume;
    Convert _converter;

    public ImageConvertViewModel([JetBrains.Annotations.NotNull] IMediaImage inputFormat, string imageSource,
                                 Window                                      view)
    {
        _view                        = view;
        _inputFormat                 = inputFormat;
        _cancel                      = false;
        DestinationCommand           = new AsyncRelayCommand(DestinationAsync);
        CreatorCommand               = new RelayCommand(Creator);
        MediaTitleCommand            = new RelayCommand(MediaTitle);
        MediaManufacturerCommand     = new RelayCommand(MediaManufacturer);
        MediaModelCommand            = new RelayCommand(MediaModel);
        MediaSerialNumberCommand     = new RelayCommand(MediaSerialNumber);
        MediaBarcodeCommand          = new RelayCommand(MediaBarcode);
        MediaPartNumberCommand       = new RelayCommand(MediaPartNumber);
        MediaSequenceCommand         = new RelayCommand(MediaSequence);
        LastMediaSequenceCommand     = new RelayCommand(LastMediaSequence);
        DriveManufacturerCommand     = new RelayCommand(DriveManufacturer);
        DriveModelCommand            = new RelayCommand(DriveModel);
        DriveSerialNumberCommand     = new RelayCommand(DriveSerialNumber);
        DriveFirmwareRevisionCommand = new RelayCommand(DriveFirmwareRevision);
        CommentsCommand              = new RelayCommand(Comments);
        AaruMetadataFromImageCommand = new RelayCommand(AaruMetadataFromImage);
        AaruMetadataCommand          = new AsyncRelayCommand(AaruMetadataAsync);
        ResumeFileFromImageCommand   = new RelayCommand(ResumeFileFromImage);
        ResumeFileCommand            = new AsyncRelayCommand(ResumeFileAsync);
        StartCommand                 = new AsyncRelayCommand(StartAsync);
        CloseCommand                 = new RelayCommand(Close);
        StopCommand                  = new RelayCommand(Stop);
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
        PluginsList                  = [];

        PluginRegister plugins = PluginRegister.Singleton;

        foreach(IBaseWritableImage plugin in plugins.WritableImages.Values)
        {
            if(plugin is null) continue;

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

        List<DumpHardware> dumpHardware = inputFormat.DumpHardware?.Any() == true ? inputFormat.DumpHardware : null;

        MetadataJsonText = _aaruMetadata == null ? "" : UI._From_image_;
        ResumeFileText   = dumpHardware  == null ? "" : UI._From_image_;

        SelectedPlugin     = PluginsList[0];
        SectorsValue       = 512;
        DestinationVisible = true;
        DestinationEnabled = true;
        OptionsVisible     = true;
        StartVisible       = true;
        CloseVisible       = true;
    }

    public ObservableCollection<ImagePluginModel> PluginsList                  { get; }
    public ICommand                               DestinationCommand           { get; }
    public ICommand                               CreatorCommand               { get; }
    public ICommand                               MediaTitleCommand            { get; }
    public ICommand                               MediaManufacturerCommand     { get; }
    public ICommand                               MediaModelCommand            { get; }
    public ICommand                               MediaSerialNumberCommand     { get; }
    public ICommand                               MediaBarcodeCommand          { get; }
    public ICommand                               MediaPartNumberCommand       { get; }
    public ICommand                               MediaSequenceCommand         { get; }
    public ICommand                               LastMediaSequenceCommand     { get; }
    public ICommand                               DriveManufacturerCommand     { get; }
    public ICommand                               DriveModelCommand            { get; }
    public ICommand                               DriveSerialNumberCommand     { get; }
    public ICommand                               DriveFirmwareRevisionCommand { get; }
    public ICommand                               CommentsCommand              { get; }
    public ICommand                               AaruMetadataFromImageCommand { get; }
    public ICommand                               AaruMetadataCommand          { get; }
    public ICommand                               ResumeFileFromImageCommand   { get; }
    public ICommand                               ResumeFileCommand            { get; }
    public ICommand                               StartCommand                 { get; }
    public ICommand                               CloseCommand                 { get; }
    public ICommand                               StopCommand                  { get; }

    async Task StartAsync()
    {
        if(SelectedPlugin is null)
        {
            await MessageBoxManager
                 .GetMessageBoxStandard(UI.Title_Error, UI.Error_trying_to_find_selected_plugin, icon: Icon.Error)
                 .ShowWindowDialogAsync(_view);

            return;
        }

        object plugin = SelectedPlugin.Plugin;
        _ = Task.Run(() => DoWorkAsync(plugin));
    }

    async Task DoWorkAsync(object plugin)
    {
        var warning = false;

        if(plugin is not IWritableImage outputFormat)
        {
            await MessageBoxManager
                 .GetMessageBoxStandard(UI.Title_Error, UI.Error_trying_to_find_selected_plugin, icon: Icon.Error)
                 .ShowWindowDialogAsync(_view);

            return;
        }

        Dictionary<string, string> parsedOptions          = [];
        uint                       nominalNegativeSectors = _inputFormat.Info.NegativeSectors;
        uint                       nominalOverflowSectors = _inputFormat.Info.OverflowSectors;
        PluginRegister             plugins                = PluginRegister.Singleton;

        _converter = new Convert(_inputFormat,
                                 outputFormat,
                                 _inputFormat.Info.MediaType,
                                 ForceChecked,
                                 DestinationText,
                                 parsedOptions,
                                 nominalNegativeSectors,
                                 nominalOverflowSectors,
                                 CommentsText,
                                 CreatorText,
                                 DriveFirmwareRevisionText,
                                 DriveManufacturerText,
                                 DriveModelText,
                                 DriveSerialNumberText,
                                 (int)LastMediaSequenceValue,
                                 MediaBarcodeText,
                                 MediaManufacturerText,
                                 MediaModelText,
                                 MediaPartNumberText,
                                 (int)MediaSequenceValue,
                                 MediaSerialNumberText,
                                 MediaTitleText,
                                 false,
                                 (uint)SectorsValue,
                                 plugins,
                                 true,
                                 false,
                                 false,
                                 false,
                                 null,
                                 _resume,
                                 _aaruMetadata,
                                 false,
                                 false,
                                 false,
                                 SourceText,
                                 0);

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
        });

        _converter.UpdateStatus += static text => AaruLogging.WriteLine(text);

        _converter.ErrorMessage += text =>
        {
            warning = true;
            AaruLogging.Error(text);
        };

        _converter.StoppingErrorMessage += text =>
        {
            _ = Dispatcher.UIThread.InvokeAsync(() => MessageBoxManager
                                                     .GetMessageBoxStandard(UI.Title_Error, text, icon: Icon.Error)
                                                     .ShowWindowDialogAsync(_view));
        };

        _converter.UpdateProgress += (text, current, maximum) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                ProgressText          = text;
                ProgressValue         = current;
                ProgressMaxValue      = maximum;
                ProgressIndeterminate = false;
            });
        };

        _converter.PulseProgress += text =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                ProgressText          = text;
                ProgressIndeterminate = true;
            });
        };

        _converter.InitProgress += () => Dispatcher.UIThread.Invoke(() => Progress1Visible = true);

        _converter.EndProgress += () => Dispatcher.UIThread.Invoke(() => Progress1Visible = false);

        _converter.InitProgress2 += () => Dispatcher.UIThread.Invoke(() => Progress2Visible = true);

        _converter.EndProgress2 += () => Dispatcher.UIThread.Invoke(() => Progress2Visible = false);

        _converter.UpdateProgress2 += (text, current, maximum) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Progress2Text          = text;
                Progress2Value         = current;
                Progress2MaxValue      = maximum;
                Progress2Indeterminate = false;
            });
        };

        _converter.Start();

        if(_cancel)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MessageBoxManager
                     .GetMessageBoxStandard(UI.Title_Error,
                                            UI.Operation_canceled_the_output_file_is_not_correct,
                                            icon: Icon.Error)
                     .ShowWindowDialogAsync(_view);

                CloseVisible    = true;
                StopVisible     = false;
                ProgressVisible = false;
            });

            return;
        }

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await MessageBoxManager.GetMessageBoxStandard(warning ? UI.Title_Warning : UI.Title_Conversion_success,
                                                          warning
                                                              ? UI.Some_warnings_happened_Check_console
                                                              : UI.Image_converted_successfully,
                                                          icon: warning ? Icon.Warning : Icon.Info)
                                   .ShowWindowDialogAsync(_view);

            CloseVisible    = true;
            StopVisible     = false;
            ProgressVisible = false;
        });

        Statistics.AddCommand("convert-image");
    }

    void Close() => _view.Close();

    internal void Stop()
    {
        _cancel = true;
        _converter?.Abort();
        StopEnabled = false;
    }

    async Task DestinationAsync()
    {
        if(SelectedPlugin is null) return;

        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = UI.Dialog_Choose_destination_file,
            FileTypeChoices =
            [
                new FilePickerFileType(SelectedPlugin.Plugin.Name)
                {
                    Patterns = SelectedPlugin.Plugin.KnownExtensions.ToList()
                }
            ]
        });

        if(result is null)
        {
            DestinationText = "";

            return;
        }

        DestinationText = result.Path.LocalPath;

        if(string.IsNullOrEmpty(Path.GetExtension(DestinationText)))
            DestinationText += SelectedPlugin.Plugin.KnownExtensions.First();
    }

    void Creator() => CreatorText = _inputFormat.Info.Creator;

    void MediaTitle() => MediaTitleText = _inputFormat.Info.MediaTitle;

    void Comments() => CommentsText = _inputFormat.Info.Comments;

    void MediaManufacturer() => MediaManufacturerText = _inputFormat.Info.MediaManufacturer;

    void MediaModel() => MediaModelText = _inputFormat.Info.MediaModel;

    void MediaSerialNumber() => MediaSerialNumberText = _inputFormat.Info.MediaSerialNumber;

    void MediaBarcode() => MediaBarcodeText = _inputFormat.Info.MediaBarcode;

    void MediaPartNumber() => MediaPartNumberText = _inputFormat.Info.MediaPartNumber;

    void MediaSequence() => MediaSequenceValue = _inputFormat.Info.MediaSequence;

    void LastMediaSequence() => LastMediaSequenceValue = _inputFormat.Info.LastMediaSequence;

    void DriveManufacturer() => DriveManufacturerText = _inputFormat.Info.DriveManufacturer;

    void DriveModel() => DriveModelText = _inputFormat.Info.DriveModel;

    void DriveSerialNumber() => DriveSerialNumberText = _inputFormat.Info.DriveSerialNumber;

    void DriveFirmwareRevision() => DriveFirmwareRevisionText = _inputFormat.Info.DriveFirmwareRevision;

    void AaruMetadataFromImage()
    {
        MetadataJsonText = UI._From_image_;
        _aaruMetadata    = _inputFormat.AaruMetadata;
    }

    async Task AaruMetadataAsync()
    {
        _aaruMetadata    = null;
        MetadataJsonText = "";

        IReadOnlyList<IStorageFile> result = await _view.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title          = UI.Dialog_Choose_existing_metadata_sidecar,
            AllowMultiple  = false,
            FileTypeFilter = [FilePickerFileTypes.AaruMetadata]
        });

        if(result.Count != 1) return;

        try
        {
            var fs = new FileStream(result[0].Path.LocalPath, FileMode.Open);

            _aaruMetadata =
                (await JsonSerializer.DeserializeAsync(fs, typeof(MetadataJson), MetadataJsonContext.Default) as
                     MetadataJson)?.AaruMetadata;

            fs.Close();
            MetadataJsonText = result[0].Path.LocalPath;
        }
        catch(Exception ex)
        {
            SentrySdk.CaptureException(ex);

            await MessageBoxManager
                 .GetMessageBoxStandard(UI.Title_Error, UI.Incorrect_metadata_sidecar_file, icon: Icon.Error)
                 .ShowWindowDialogAsync(_view);
        }
    }

    void ResumeFileFromImage()
    {
        ResumeFileText = UI._From_image_;
    }

    async Task ResumeFileAsync()
    {
        _resume        = null;
        ResumeFileText = "";

        IReadOnlyList<IStorageFile> result = await _view.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title          = UI.Dialog_Choose_existing_resume_file,
            AllowMultiple  = false,
            FileTypeFilter = [FilePickerFileTypes.AaruResumeFile]
        });

        if(result.Count != 1) return;

        try
        {
            var fs = new FileStream(result[0].Path.LocalPath, FileMode.Open);

            Resume resume =
                (await JsonSerializer.DeserializeAsync(fs, typeof(ResumeJson), ResumeJsonContext.Default) as ResumeJson)
              ?.Resume;

            fs.Close();

            if(resume?.Tries?.Any() == true)
            {
                _resume        = resume;
                ResumeFileText = result[0].Path.LocalPath;
            }
            else
            {
                await MessageBoxManager
                     .GetMessageBoxStandard(UI.Title_Error,
                                            UI.Resume_file_does_not_contain_dump_hardware_information,
                                            icon: Icon.Error)
                     .ShowWindowDialogAsync(_view);
            }
        }
        catch(Exception ex)
        {
            SentrySdk.CaptureException(ex);

            await MessageBoxManager.GetMessageBoxStandard(UI.Title_Error, UI.Incorrect_resume_file, icon: Icon.Error)
                                   .ShowWindowDialogAsync(_view);
        }
    }
}