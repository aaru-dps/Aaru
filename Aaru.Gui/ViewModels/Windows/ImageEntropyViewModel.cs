// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ImageEntropyViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the image entropy calculation window.
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

namespace Aaru.Gui.ViewModels.Windows;

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using System.Threading;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Gui.Models;
using Avalonia.Controls;
using Avalonia.Threading;
using JetBrains.Annotations;
using ReactiveUI;

public sealed class ImageEntropyViewModel : ViewModelBase
{
    readonly IMediaImage _inputFormat;
    readonly Window      _view;
    bool                 _closeVisible;
    bool                 _duplicatedSectorsChecked;
    bool                 _duplicatedSectorsEnabled;
    EntropyResults       _entropy;
    string               _mediaEntropyText;
    bool                 _mediaEntropyVisible;
    string               _mediaUniqueSectorsText;
    bool                 _mediaUniqueSectorsVisible;
    bool                 _optionsVisible;
    bool                 _progress1Visible;
    bool                 _progress2Indeterminate;
    double               _progress2Max;
    string               _progress2Text;
    double               _progress2Value;
    bool                 _progress2Visible;
    bool                 _progressIndeterminate;
    double               _progressMax;
    string               _progressText;
    double               _progressValue;
    bool                 _progressVisible;
    bool                 _resultsVisible;
    bool                 _separatedTracksChecked;
    bool                 _separatedTracksEnabled;
    bool                 _separatedTracksVisible;
    bool                 _startVisible;
    bool                 _stopVisible;
    EntropyResults[]     _tracksEntropy;
    bool                 _wholeDiscChecked;
    bool                 _wholeDiscEnabled;
    bool                 _wholeDiscVisible;

    public ImageEntropyViewModel(IMediaImage inputFormat, Window view)
    {
        _inputFormat             = inputFormat;
        _view                    = view;
        TrackEntropy             = new ObservableCollection<TrackEntropyModel>();
        StartCommand             = ReactiveCommand.Create(ExecuteStartCommand);
        CloseCommand             = ReactiveCommand.Create(ExecuteCloseCommand);
        StopCommand              = ReactiveCommand.Create(ExecuteStopCommand);
        OptionsVisible           = true;
        DuplicatedSectorsChecked = true;
        SeparatedTracksChecked   = true;
        WholeDiscChecked         = true;
        StartVisible             = true;

        var inputOptical = inputFormat as IOpticalMediaImage;

        if(inputOptical?.Tracks.Count > 0)
        {
            SeparatedTracksVisible = true;
            WholeDiscVisible       = true;
        }
        else
        {
            SeparatedTracksChecked = false;
            WholeDiscChecked       = true;
        }
    }

    public bool SeparatedTracksVisible
    {
        get => _separatedTracksVisible;
        set => this.RaiseAndSetIfChanged(ref _separatedTracksVisible, value);
    }

    public bool WholeDiscVisible
    {
        get => _wholeDiscVisible;
        set => this.RaiseAndSetIfChanged(ref _wholeDiscVisible, value);
    }

    public bool SeparatedTracksChecked
    {
        get => _separatedTracksChecked;
        set => this.RaiseAndSetIfChanged(ref _separatedTracksChecked, value);
    }

    public bool WholeDiscChecked
    {
        get => _wholeDiscChecked;
        set => this.RaiseAndSetIfChanged(ref _wholeDiscChecked, value);
    }

    public bool DuplicatedSectorsEnabled
    {
        get => _duplicatedSectorsEnabled;
        set => this.RaiseAndSetIfChanged(ref _duplicatedSectorsEnabled, value);
    }

    public bool SeparatedTracksEnabled
    {
        get => _separatedTracksEnabled;
        set => this.RaiseAndSetIfChanged(ref _separatedTracksEnabled, value);
    }

    public bool WholeDiscEnabled
    {
        get => _wholeDiscEnabled;
        set => this.RaiseAndSetIfChanged(ref _wholeDiscEnabled, value);
    }

    public bool CloseVisible
    {
        get => _closeVisible;
        set => this.RaiseAndSetIfChanged(ref _closeVisible, value);
    }

    public bool StartVisible
    {
        get => _startVisible;
        set => this.RaiseAndSetIfChanged(ref _startVisible, value);
    }

    public bool StopVisible
    {
        get => _stopVisible;
        set => this.RaiseAndSetIfChanged(ref _stopVisible, value);
    }

    public bool ProgressVisible
    {
        get => _progressVisible;
        set => this.RaiseAndSetIfChanged(ref _progressVisible, value);
    }

    public bool DuplicatedSectorsChecked
    {
        get => _duplicatedSectorsChecked;
        set => this.RaiseAndSetIfChanged(ref _duplicatedSectorsChecked, value);
    }

    public bool OptionsVisible
    {
        get => _optionsVisible;
        set => this.RaiseAndSetIfChanged(ref _optionsVisible, value);
    }

    public bool ResultsVisible
    {
        get => _resultsVisible;
        set => this.RaiseAndSetIfChanged(ref _resultsVisible, value);
    }

    public string MediaEntropyText
    {
        get => _mediaEntropyText;
        set => this.RaiseAndSetIfChanged(ref _mediaEntropyText, value);
    }

    public bool MediaEntropyVisible
    {
        get => _mediaEntropyVisible;
        set => this.RaiseAndSetIfChanged(ref _mediaEntropyVisible, value);
    }

    public string MediaUniqueSectorsText
    {
        get => _mediaUniqueSectorsText;
        set => this.RaiseAndSetIfChanged(ref _mediaUniqueSectorsText, value);
    }

    public bool MediaUniqueSectorsVisible
    {
        get => _mediaUniqueSectorsVisible;
        set => this.RaiseAndSetIfChanged(ref _mediaUniqueSectorsVisible, value);
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

    public string ProgressText
    {
        get => _progressText;
        set => this.RaiseAndSetIfChanged(ref _progressText, value);
    }

    public bool ProgressIndeterminate
    {
        get => _progressIndeterminate;
        set => this.RaiseAndSetIfChanged(ref _progressIndeterminate, value);
    }

    public double ProgressMax
    {
        get => _progressMax;
        set => this.RaiseAndSetIfChanged(ref _progressMax, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
    }

    public string Progress2Text
    {
        get => _progress2Text;
        set => this.RaiseAndSetIfChanged(ref _progress2Text, value);
    }

    public bool Progress2Indeterminate
    {
        get => _progress2Indeterminate;
        set => this.RaiseAndSetIfChanged(ref _progress2Indeterminate, value);
    }

    public double Progress2Max
    {
        get => _progress2Max;
        set => this.RaiseAndSetIfChanged(ref _progress2Max, value);
    }

    public double Progress2Value
    {
        get => _progress2Value;
        set => this.RaiseAndSetIfChanged(ref _progress2Value, value);
    }

    [NotNull]
    public string Title => "Calculating entropy";
    public ObservableCollection<TrackEntropyModel> TrackEntropy { get; }
    public ReactiveCommand<Unit, Unit>             StartCommand { get; }
    public ReactiveCommand<Unit, Unit>             CloseCommand { get; }
    public ReactiveCommand<Unit, Unit>             StopCommand  { get; }

    void ExecuteStartCommand()
    {
        var entropyCalculator = new Entropy(false, _inputFormat);
        entropyCalculator.InitProgressEvent    += InitProgress;
        entropyCalculator.InitProgress2Event   += InitProgress2;
        entropyCalculator.UpdateProgressEvent  += UpdateProgress;
        entropyCalculator.UpdateProgress2Event += UpdateProgress2;
        entropyCalculator.EndProgressEvent     += EndProgress;
        entropyCalculator.EndProgress2Event    += EndProgress2;
        DuplicatedSectorsEnabled               =  false;
        SeparatedTracksEnabled                 =  false;
        WholeDiscEnabled                       =  false;
        CloseVisible                           =  false;
        StartVisible                           =  false;
        StopVisible                            =  false;
        ProgressVisible                        =  true;

        if(WholeDiscChecked                                 &&
           _inputFormat is IOpticalMediaImage opticalFormat &&
           opticalFormat.Sessions?.Count > 1)
        {
            AaruConsole.ErrorWriteLine("Calculating disc entropy of multisession images is not yet implemented.");
            WholeDiscChecked = false;
        }

        var thread = new Thread(async () =>
        {
            if(SeparatedTracksChecked)
            {
                _tracksEntropy = entropyCalculator.CalculateTracksEntropy(DuplicatedSectorsChecked);

                foreach(EntropyResults trackEntropy in _tracksEntropy)
                {
                    AaruConsole.WriteLine("Entropy for track {0} is {1:F4}.", trackEntropy.Track, trackEntropy.Entropy);

                    if(trackEntropy.UniqueSectors != null)
                        AaruConsole.WriteLine("Track {0} has {1} unique sectors ({2:P3})", trackEntropy.Track,
                                              trackEntropy.UniqueSectors,
                                              (double)trackEntropy.UniqueSectors / trackEntropy.Sectors);
                }
            }

            if(WholeDiscChecked != true)
                return;

            _entropy = entropyCalculator.CalculateMediaEntropy(DuplicatedSectorsChecked);

            await Dispatcher.UIThread.InvokeAsync(Finish);
        });

        Statistics.AddCommand("entropy");

        thread.Start();
    }

    void Finish()
    {
        OptionsVisible  = false;
        CloseVisible    = true;
        ProgressVisible = false;
        ResultsVisible  = true;

        if(SeparatedTracksChecked)
            foreach(EntropyResults trackEntropy in _tracksEntropy)
                TrackEntropy.Add(new TrackEntropyModel
                {
                    Track = trackEntropy.Track.ToString(),
                    Entropy = trackEntropy.Entropy.ToString(CultureInfo.CurrentUICulture),
                    UniqueSectors =
                        $"{trackEntropy.UniqueSectors} ({(trackEntropy.UniqueSectors ?? 0) / (double)trackEntropy.Sectors:P3})"
                });

        if(WholeDiscChecked != true)
            return;

        MediaEntropyText    = $"Entropy for disk is {_entropy.Entropy:F4}.";
        MediaEntropyVisible = true;

        if(_entropy.UniqueSectors == null)
            return;

        MediaUniqueSectorsText =
            $"Disk has {_entropy.UniqueSectors} unique sectors ({(double)_entropy.UniqueSectors / _entropy.Sectors:P3})";

        MediaUniqueSectorsVisible = true;
    }

    void ExecuteCloseCommand() => _view.Close();

    internal void ExecuteStopCommand() => throw new NotImplementedException();

    void InitProgress() => Progress1Visible = true;

    void EndProgress() => Progress1Visible = false;

    void InitProgress2() => Progress2Visible = true;

    void EndProgress2() => Progress2Visible = false;

    async void UpdateProgress(string text, long current, long maximum) => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        ProgressText = text;

        if(maximum == 0)
        {
            ProgressIndeterminate = true;

            return;
        }

        if(ProgressIndeterminate)
            ProgressIndeterminate = false;

        ProgressMax   = maximum;
        ProgressValue = current;
    });

    async void UpdateProgress2(string text, long current, long maximum) => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        Progress2Text = text;

        if(maximum == 0)
        {
            Progress2Indeterminate = true;

            return;
        }

        if(Progress2Indeterminate)
            Progress2Indeterminate = false;

        Progress2Max   = maximum;
        Progress2Value = current;
    });
}