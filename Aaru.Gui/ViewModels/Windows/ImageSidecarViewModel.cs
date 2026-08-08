// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ImageSidecarViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the image sidecar creation window.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using Aaru.Localization;
using Aaru.Logging;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aaru.Gui.ViewModels.Windows;

public sealed partial class ImageSidecarViewModel : ViewModelBase
{
    readonly Encoding    _encoding;
    readonly Guid        _filterId;
    readonly string      _imageSource;
    readonly IMediaImage _inputFormat;
    readonly Window      _view;
    [ObservableProperty]
    bool _closeVisible;
    [ObservableProperty]
    bool _destinationEnabled;
    [ObservableProperty]
    string _destinationText;
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
    Sidecar _sidecarClass;
    [ObservableProperty]
    bool _startVisible;
    [ObservableProperty]
    string _statusText;
    [ObservableProperty]
    bool _statusVisible;
    [ObservableProperty]
    bool _stopEnabled;
    [ObservableProperty]
    bool _stopVisible;

    public ImageSidecarViewModel(IMediaImage inputFormat, string imageSource, Guid filterId, Encoding encoding,
                                 Window      view)
    {
        _view        = view;
        _inputFormat = inputFormat;
        _imageSource = imageSource;
        _filterId    = filterId;
        _encoding    = encoding;

        DestinationText = Path.Combine(Path.GetDirectoryName(imageSource) ?? "",
                                       Path.GetFileNameWithoutExtension(imageSource) + ".metadata.json");

        DestinationEnabled = true;
        StartVisible       = true;
        CloseVisible       = true;
        DestinationCommand = new AsyncRelayCommand(DestinationAsync);
        StartCommand       = new RelayCommand(Start);
        CloseCommand       = new RelayCommand(Close);
        StopCommand        = new RelayCommand(Stop);
    }

    public string   Title              { get; }
    public ICommand DestinationCommand { get; }
    public ICommand StartCommand       { get; }
    public ICommand CloseCommand       { get; }
    public ICommand StopCommand        { get; }

    void Start() => new Thread(DoWork).Start();

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    async void DoWork()
    {
        // Prepare UI
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            CloseVisible       = false;
            StartVisible       = false;
            StopVisible        = true;
            StopEnabled        = true;
            ProgressVisible    = true;
            DestinationEnabled = false;
            StatusVisible      = true;
        });

        _sidecarClass                      =  new Sidecar(_inputFormat, _imageSource, _filterId, _encoding, false);
        _sidecarClass.UpdateStatusEvent    += UpdateStatus;
        _sidecarClass.InitProgressEvent    += InitProgress;
        _sidecarClass.UpdateProgressEvent  += UpdateProgress;
        _sidecarClass.EndProgressEvent     += EndProgress;
        _sidecarClass.InitProgressEvent2   += InitProgress2;
        _sidecarClass.UpdateProgressEvent2 += UpdateProgress2;
        _sidecarClass.EndProgressEvent2    += EndProgress2;
        Metadata sidecar = _sidecarClass.Create();

        AaruLogging.WriteLine(Aaru.Localization.Core.Writing_metadata_sidecar);

        var jsonFs = new FileStream(DestinationText, FileMode.Create);

        await JsonSerializer.SerializeAsync(jsonFs,
                                            new MetadataJson
                                            {
                                                AaruMetadata = sidecar
                                            },
                                            typeof(MetadataJson),
                                            MetadataJsonContext.Default);

        jsonFs.Close();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            CloseVisible    = true;
            StopVisible     = false;
            ProgressVisible = false;
            StatusVisible   = false;
        });

        Statistics.AddCommand("create-sidecar");
    }

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    async void EndProgress2() => await Dispatcher.UIThread.InvokeAsync(() => { Progress2Visible = false; });

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    async void UpdateProgress2(string text, long current, long maximum) => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        Progress2Text          = text;
        Progress2Indeterminate = false;

        Progress2MaxValue = maximum;
        Progress2Value    = current;
    });

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    async void InitProgress2() => await Dispatcher.UIThread.InvokeAsync(() => { Progress2Visible = true; });

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    async void EndProgress() => await Dispatcher.UIThread.InvokeAsync(() => { Progress1Visible = false; });

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    async void UpdateProgress(string text, long current, long maximum) => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        ProgressText          = text;
        ProgressIndeterminate = false;

        ProgressMaxValue = maximum;
        ProgressValue    = current;
    });

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    async void InitProgress() => await Dispatcher.UIThread.InvokeAsync(() => { Progress1Visible = true; });

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    async void UpdateStatus(string text) => await Dispatcher.UIThread.InvokeAsync(() => { StatusText = text; });

    void Close() => _view.Close();

    void Stop()
    {
        ProgressText = Aaru.Localization.Core.Aborting;
        StopEnabled  = false;
        _sidecarClass.Abort();
    }

    async Task DestinationAsync()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = UI.Dialog_Choose_destination_file,
            FileTypeChoices = new List<FilePickerFileType>
            {
                FilePickerFileTypes.AaruMetadata
            }
        });

        if(result is null)
        {
            DestinationText = "";

            return;
        }

        DestinationText = result.Path.LocalPath;
        if(string.IsNullOrEmpty(Path.GetExtension(DestinationText))) DestinationText += ".json";
    }
}