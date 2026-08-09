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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using Aaru.Gui.Models;
using Aaru.Localization;
using Aaru.Logging;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aaru.Gui.ViewModels.Windows;

public sealed partial class ImageEntropyViewModel : ViewModelBase
{
    readonly IMediaImage _inputFormat;
    readonly Window      _view;
    [ObservableProperty]
    bool _closeVisible;
    [ObservableProperty]
    bool _duplicatedSectorsChecked;
    [ObservableProperty]
    bool _duplicatedSectorsEnabled;
    EntropyResults _entropy;
    [ObservableProperty]
    string _mediaEntropyText;
    [ObservableProperty]
    bool _mediaEntropyVisible;
    [ObservableProperty]
    string _mediaUniqueSectorsText;
    [ObservableProperty]
    bool _mediaUniqueSectorsVisible;
    [ObservableProperty]
    bool _optionsVisible;
    [ObservableProperty]
    bool _progress1Visible;
    [ObservableProperty]
    bool _progress2Indeterminate;
    [ObservableProperty]
    double _progress2Max;
    [ObservableProperty]
    string _progress2Text;
    [ObservableProperty]
    double _progress2Value;
    [ObservableProperty]
    bool _progress2Visible;
    [ObservableProperty]
    bool _progressIndeterminate;
    [ObservableProperty]
    double _progressMax;
    [ObservableProperty]
    string _progressText;
    [ObservableProperty]
    double _progressValue;
    [ObservableProperty]
    bool _progressVisible;
    [ObservableProperty]
    bool _resultsVisible;
    [ObservableProperty]
    bool _separatedTracksChecked;
    [ObservableProperty]
    bool _separatedTracksEnabled;
    [ObservableProperty]
    bool _separatedTracksVisible;
    [ObservableProperty]
    bool _startVisible;
    [ObservableProperty]
    bool _stopVisible;
    EntropyResults[] _tracksEntropy;
    [ObservableProperty]
    bool _wholeDiscChecked;
    [ObservableProperty]
    bool _wholeDiscEnabled;
    [ObservableProperty]
    bool _wholeDiscVisible;

    public ImageEntropyViewModel(IMediaImage inputFormat, Window view)
    {
        _inputFormat             = inputFormat;
        _view                    = view;
        TrackEntropy             = [];
        StartCommand             = new RelayCommand(Start);
        CloseCommand             = new RelayCommand(Close);
        StopCommand              = new RelayCommand(Stop);
        OptionsVisible           = true;
        DuplicatedSectorsChecked = true;
        SeparatedTracksChecked   = true;
        WholeDiscChecked         = true;
        StartVisible             = true;

        var inputOptical = inputFormat as IOpticalMediaImage;

        bool hasTracks;

        try
        {
            hasTracks = inputOptical?.Tracks?.Count > 0;
        }
        catch(Exception ex)
        {
            AaruLogging.Exception(ex, UI.Title_Error);
            hasTracks = false;
        }

        if(hasTracks)
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

    public ObservableCollection<TrackEntropyModel> TrackEntropy { get; }
    public ICommand                                StartCommand { get; }
    public ICommand                                CloseCommand { get; }
    public ICommand                                StopCommand  { get; }

    void Start()
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
            AaruLogging.Error(UI.Calculating_disc_entropy_of_multisession_images_is_not_yet_implemented);
            WholeDiscChecked = false;
        }

        Statistics.AddCommand("entropy");

        _ = Task.Run(async () =>
        {
            try
            {
                await CalculateAsync(entropyCalculator);
            }
            catch(Exception ex)
            {
                AaruLogging.Exception(ex, UI.Title_Error);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressVisible = false;
                    CloseVisible    = true;
                });
            }
        });
    }

    async Task CalculateAsync(Entropy entropyCalculator)
    {
        if(SeparatedTracksChecked)
        {
            _tracksEntropy = entropyCalculator.CalculateTracksEntropy(DuplicatedSectorsChecked);

            foreach(EntropyResults trackEntropy in _tracksEntropy)
            {
                AaruLogging.WriteLine(UI.Entropy_for_track_0_is_1, trackEntropy.Track, trackEntropy.Entropy);

                if(trackEntropy.UniqueSectors != null)
                {
                    AaruLogging.WriteLine(UI.Track_0_has_1_unique_sectors_2,
                                          trackEntropy.Track,
                                          trackEntropy.UniqueSectors,
                                          (double)trackEntropy.UniqueSectors / trackEntropy.Sectors);
                }
            }
        }

        if(WholeDiscChecked) _entropy = entropyCalculator.CalculateMediaEntropy(DuplicatedSectorsChecked);

        await Dispatcher.UIThread.InvokeAsync(Finish);
    }

    void Finish()
    {
        OptionsVisible  = false;
        CloseVisible    = true;
        ProgressVisible = false;
        ResultsVisible  = true;

        if(SeparatedTracksChecked)
        {
            foreach(EntropyResults trackEntropy in _tracksEntropy)
            {
                TrackEntropy.Add(new TrackEntropyModel
                {
                    Track   = trackEntropy.Track.ToString(),
                    Entropy = trackEntropy.Entropy.ToString(CultureInfo.CurrentUICulture),
                    UniqueSectors = $"{trackEntropy.UniqueSectors} ({
                        (trackEntropy.UniqueSectors ?? 0) / (double)trackEntropy.Sectors:P3})"
                });
            }
        }

        if(!WholeDiscChecked) return;

        MediaEntropyText    = string.Format(UI.Entropy_for_disk_is_0, _entropy.Entropy);
        MediaEntropyVisible = true;

        if(_entropy.UniqueSectors == null) return;

        MediaUniqueSectorsText = string.Format(UI.Disk_has_0_unique_sectors_1,
                                               _entropy.UniqueSectors,
                                               (double)_entropy.UniqueSectors / _entropy.Sectors);

        MediaUniqueSectorsVisible = true;
    }

    void Close() => _view.Close();

    internal void Stop()
    {
        // Not implemented
    }

    void InitProgress() => Dispatcher.UIThread.Post(() => Progress1Visible = true);

    void EndProgress() => Dispatcher.UIThread.Post(() => Progress1Visible = false);

    void InitProgress2() => Dispatcher.UIThread.Post(() => Progress2Visible = true);

    void EndProgress2() => Dispatcher.UIThread.Post(() => Progress2Visible = false);

    void UpdateProgress(string text, long current, long maximum) => Dispatcher.UIThread.Post(() =>
    {
        ProgressText = text;

        if(maximum == 0)
        {
            ProgressIndeterminate = true;

            return;
        }

        if(ProgressIndeterminate) ProgressIndeterminate = false;

        ProgressMax   = maximum;
        ProgressValue = current;
    });

    void UpdateProgress2(string text, long current, long maximum) => Dispatcher.UIThread.Post(() =>
    {
        Progress2Text = text;

        if(maximum == 0)
        {
            Progress2Indeterminate = true;

            return;
        }

        if(Progress2Indeterminate) Progress2Indeterminate = false;

        Progress2Max   = maximum;
        Progress2Value = current;
    });
}