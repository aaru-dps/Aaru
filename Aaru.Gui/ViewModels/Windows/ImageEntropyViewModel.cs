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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reactive;
using System.Threading;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Gui.Models;
using Aaru.Localization;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Windows;

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

    public string DuplicatedSectorsLabel => UI.Calculates_how_many_sectors_are_duplicated;
    public string SeparatedTracksLabel   => UI.Calculates_entropy_for_each_track_separately;
    public string WholeDiscLabel         => UI.Calculates_entropy_for_the_whole_disc;
    public string TrackEntropyLabel      => UI.Title_Track_entropy;
    public string TrackLabel             => Localization.Core.Title_Track;
    public string EntropyLabel           => UI.Title_Entropy;
    public string UniqueSectorsLabel     => UI.Title_Unique_sectors;
    public string StartLabel             => UI.ButtonLabel_Start;
    public string CloseLabel             => UI.ButtonLabel_Close;
    public string StopLabel              => UI.ButtonLabel_Stop;

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

    [JetBrains.Annotations.NotNull]
    public string Title => UI.Title_Calculating_entropy;
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

        if(WholeDiscChecked && _inputFormat is IOpticalMediaImage { Sessions.Count: > 1 })
        {
            AaruConsole.ErrorWriteLine(UI.Calculating_disc_entropy_of_multisession_images_is_not_yet_implemented);
            WholeDiscChecked = false;
        }

        // ReSharper disable once AsyncVoidLambda
        var thread = new Thread(async () =>
        {
            if(SeparatedTracksChecked)
            {
                _tracksEntropy = entropyCalculator.CalculateTracksEntropy(DuplicatedSectorsChecked);

                foreach(EntropyResults trackEntropy in _tracksEntropy)
                {
                    AaruConsole.WriteLine(UI.Entropy_for_track_0_is_1, trackEntropy.Track, trackEntropy.Entropy);

                    if(trackEntropy.UniqueSectors != null)
                        AaruConsole.WriteLine(UI.Track_0_has_1_unique_sectors_2, trackEntropy.Track,
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
                    Track   = trackEntropy.Track.ToString(),
                    Entropy = trackEntropy.Entropy.ToString(CultureInfo.CurrentUICulture),
                    UniqueSectors = $"{trackEntropy.UniqueSectors} ({
                        (trackEntropy.UniqueSectors ?? 0) / (double)trackEntropy.Sectors:P3})"
                });

        if(WholeDiscChecked != true)
            return;

        MediaEntropyText    = string.Format(UI.Entropy_for_disk_is_0, _entropy.Entropy);
        MediaEntropyVisible = true;

        if(_entropy.UniqueSectors == null)
            return;

        MediaUniqueSectorsText = string.Format(UI.Disk_has_0_unique_sectors_1, _entropy.UniqueSectors,
                                               (double)_entropy.UniqueSectors / _entropy.Sectors);

        MediaUniqueSectorsVisible = true;
    }

    void ExecuteCloseCommand() => _view.Close();

    internal void ExecuteStopCommand() => throw new NotImplementedException();

    void InitProgress() => Progress1Visible = true;

    void EndProgress() => Progress1Visible = false;

    void InitProgress2() => Progress2Visible = true;

    void EndProgress2() => Progress2Visible = false;

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
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

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
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