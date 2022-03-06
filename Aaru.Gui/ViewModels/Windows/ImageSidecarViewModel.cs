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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using Schemas;

namespace Aaru.Gui.ViewModels.Windows;

public sealed class ImageSidecarViewModel : ViewModelBase
{
    readonly Encoding    _encoding;
    readonly Guid        _filterId;
    readonly string      _imageSource;
    readonly IMediaImage _inputFormat;
    readonly Window      _view;
    bool                 _closeVisible;
    bool                 _destinationEnabled;
    string               _destinationText;
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
    Sidecar              _sidecarClass;
    bool                 _startVisible;
    string               _statusText;
    bool                 _statusVisible;
    bool                 _stopEnabled;
    bool                 _stopVisible;

    public ImageSidecarViewModel(IMediaImage inputFormat, string imageSource, Guid filterId, Encoding encoding,
                                 Window view)
    {
        _view        = view;
        _inputFormat = inputFormat;
        _imageSource = imageSource;
        _filterId    = filterId;
        _encoding    = encoding;

        DestinationText = Path.Combine(Path.GetDirectoryName(imageSource) ?? "",
                                       Path.GetFileNameWithoutExtension(imageSource) + ".cicm.xml");

        DestinationEnabled = true;
        StartVisible       = true;
        CloseVisible       = true;
        DestinationCommand = ReactiveCommand.Create(ExecuteDestinationCommand);
        StartCommand       = ReactiveCommand.Create(ExecuteStartCommand);
        CloseCommand       = ReactiveCommand.Create(ExecuteCloseCommand);
        StopCommand        = ReactiveCommand.Create(ExecuteStopCommand);
    }

    public string                      Title              { get; }
    public ReactiveCommand<Unit, Unit> DestinationCommand { get; }
    public ReactiveCommand<Unit, Unit> StartCommand       { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand       { get; }
    public ReactiveCommand<Unit, Unit> StopCommand        { get; }

    public bool ProgressIndeterminate
    {
        get => _progressIndeterminate;
        set => this.RaiseAndSetIfChanged(ref _progressIndeterminate, value);
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

    public bool Progress2Visible
    {
        get => _progress2Visible;
        set => this.RaiseAndSetIfChanged(ref _progress2Visible, value);
    }

    public bool Progress2Indeterminate
    {
        get => _progress2Indeterminate;
        set => this.RaiseAndSetIfChanged(ref _progress2Indeterminate, value);
    }

    public double ProgressMaxValue
    {
        get => _progressMaxValue;
        set => this.RaiseAndSetIfChanged(ref _progressMaxValue, value);
    }

    public double Progress2MaxValue
    {
        get => _progress2MaxValue;
        set => this.RaiseAndSetIfChanged(ref _progress2MaxValue, value);
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

    public double Progress2Value
    {
        get => _progress2Value;
        set => this.RaiseAndSetIfChanged(ref _progress2Value, value);
    }

    public string Progress2Text
    {
        get => _progress2Text;
        set => this.RaiseAndSetIfChanged(ref _progress2Text, value);
    }

    public string DestinationText
    {
        get => _destinationText;
        set => this.RaiseAndSetIfChanged(ref _destinationText, value);
    }

    public bool DestinationEnabled
    {
        get => _destinationEnabled;
        set => this.RaiseAndSetIfChanged(ref _destinationEnabled, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    public bool StatusVisible
    {
        get => _statusVisible;
        set => this.RaiseAndSetIfChanged(ref _statusVisible, value);
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

    public bool StopEnabled
    {
        get => _stopEnabled;
        set => this.RaiseAndSetIfChanged(ref _stopEnabled, value);
    }

    public bool StopVisible
    {
        get => _stopVisible;
        set => this.RaiseAndSetIfChanged(ref _stopVisible, value);
    }

    void ExecuteStartCommand() => new Thread(DoWork).Start();

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

        _sidecarClass                      =  new Sidecar(_inputFormat, _imageSource, _filterId, _encoding);
        _sidecarClass.UpdateStatusEvent    += UpdateStatus;
        _sidecarClass.InitProgressEvent    += InitProgress;
        _sidecarClass.UpdateProgressEvent  += UpdateProgress;
        _sidecarClass.EndProgressEvent     += EndProgress;
        _sidecarClass.InitProgressEvent2   += InitProgress2;
        _sidecarClass.UpdateProgressEvent2 += UpdateProgress2;
        _sidecarClass.EndProgressEvent2    += EndProgress2;
        CICMMetadataType sidecar = _sidecarClass.Create();

        AaruConsole.WriteLine("Writing metadata sidecar");

        var xmlFs = new FileStream(DestinationText, FileMode.Create);

        var xmlSer = new XmlSerializer(typeof(CICMMetadataType));
        xmlSer.Serialize(xmlFs, sidecar);
        xmlFs.Close();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            CloseVisible    = true;
            StopVisible     = false;
            ProgressVisible = false;
            StatusVisible   = false;
        });

        Statistics.AddCommand("create-sidecar");
    }

    async void EndProgress2() => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        Progress2Visible = false;
    });

    async void UpdateProgress2(string text, long current, long maximum) =>
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Progress2Text          = text;
            Progress2Indeterminate = false;

            Progress2MaxValue = maximum;
            Progress2Value    = current;
        });

    async void InitProgress2() => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        Progress2Visible = true;
    });

    async void EndProgress() => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        Progress1Visible = false;
    });

    async void UpdateProgress(string text, long current, long maximum) =>
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProgressText          = text;
            ProgressIndeterminate = false;

            ProgressMaxValue = maximum;
            ProgressValue    = current;
        });

    async void InitProgress() => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        Progress1Visible = true;
    });

    async void UpdateStatus(string text) => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        StatusText = text;
    });

    void ExecuteCloseCommand() => _view.Close();

    void ExecuteStopCommand()
    {
        ProgressText = "Aborting...";
        StopEnabled  = false;
        _sidecarClass.Abort();
    }

    async void ExecuteDestinationCommand()
    {
        var dlgDestination = new SaveFileDialog
        {
            Title = "Choose destination file"
        };

        dlgDestination.Filters.Add(new FileDialogFilter
        {
            Name = "CICM XML metadata",
            Extensions = new List<string>(new[]
            {
                "*.xml"
            })
        });

        string result = await dlgDestination.ShowAsync(_view);

        if(result is null)
        {
            DestinationText = "";

            return;
        }

        if(string.IsNullOrEmpty(Path.GetExtension(result)))
            result += ".xml";

        DestinationText = result;
    }
}