// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MediaScanViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the media scan window.
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Aaru.Core;
using Aaru.Core.Devices.Scanning;
using Aaru.Devices;
using Aaru.Localization;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

//using OxyPlot;

namespace Aaru.Gui.ViewModels.Windows;

[SuppressMessage("Usage",                   "VSTHRD100:Avoid async void methods", Justification = "Event handlers")]
[SuppressMessage("Philips Maintainability", "PH2096:Avoid async void",            Justification = "Event handlers")]
[SuppressMessage("AsyncUsage",
                 "AsyncFixer03:Fire-and-forget async-void methods or delegates",
                 Justification = "Event handlers")]
[SuppressMessage("ReSharper", "AsyncVoidMethod", Justification = "Event handlers")]
public sealed partial class MediaScanViewModel : ViewModelBase
{
    readonly Device                                        _device;
    readonly string                                        _devicePath;
    readonly List<(ulong startingSector, double duration)> _pendingSectorData     = [];
    readonly object                                        _pendingSectorDataLock = new();
    readonly Window                                        _view;
    [ObservableProperty]
    string _a;
    [ObservableProperty]
    string _avgSpeed;
    [ObservableProperty]
    string _b;
    [ObservableProperty]
    ObservableCollection<(ulong startingSector, double duration)> _blockMapSectorData;
    [ObservableProperty]
    ulong _blocks;
    ulong _blocksToRead;
    [ObservableProperty]
    string _c;
    [ObservableProperty]
    bool _closeVisible;
    [ObservableProperty]
    string _d;
    [ObservableProperty]
    string _e;
    [ObservableProperty]
    string _f;
    ScanResults _localResults;
    [ObservableProperty]
    double _maxGraphSpeed;
    [ObservableProperty]
    ulong _maxSector;
    [ObservableProperty]
    string _maxSpeed;
    [ObservableProperty]
    double _maxX;
    [ObservableProperty]
    double _maxY;
    [ObservableProperty]
    string _minSpeed;
    [ObservableProperty]
    double _minX;
    [ObservableProperty]
    double _minY;
    [ObservableProperty]
    bool _progress1Visible;
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
    bool _resultsVisible;
    [ObservableProperty]
    uint _scanBlockSize;
    MediaScan _scanner;
    [ObservableProperty]
    ObservableCollection<(ulong sector, double speedKbps)> _speedData;
    [ObservableProperty]
    int _speedMultiplier;
    [ObservableProperty]
    bool _startVisible;
    [ObservableProperty]
    double _stepsX;
    [ObservableProperty]
    double _stepsY;
    [ObservableProperty]
    bool _stopEnabled;
    [ObservableProperty]
    bool _stopVisible;
    [ObservableProperty]
    string _totalTime;
    [ObservableProperty]
    string _unreadableSectors;

    public MediaScanViewModel(Device device, string devicePath, Window view)
    {
        _device      = device;
        _devicePath  = devicePath;
        _view        = view;
        StopVisible  = false;
        StopEnabled  = true;
        StartCommand = new RelayCommand(Start);
        CloseCommand = new RelayCommand(Close);
        StopCommand  = new RelayCommand(Stop);
        StartVisible = true;
        CloseVisible = true;
    }

    public string Title { get; }

    public ICommand StartCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand StopCommand  { get; }

    void Close() => _view.Close();

    internal void Stop()
    {
        StopEnabled = false;
        _scanner?.Abort();
    }

    void Start()
    {
        StopEnabled     = true;
        StopVisible     = true;
        StartVisible    = false;
        CloseVisible    = false;
        ProgressVisible = true;
        ResultsVisible  = true;

        new Thread(DoWork).Start();
    }

    // TODO: Allow to save MHDD and ImgBurn log files
    async void DoWork()
    {
        _localResults                 =  new ScanResults();
        _scanner                      =  new MediaScan(null, null, _devicePath, _device, false);
        _scanner.ScanTime             += OnScanTime;
        _scanner.ScanUnreadable       += OnScanUnreadable;
        _scanner.UpdateStatus         += UpdateStatus;
        _scanner.StoppingErrorMessage += StoppingErrorMessage;
        _scanner.PulseProgress        += PulseProgress;
        _scanner.InitProgress         += InitProgress;
        _scanner.UpdateProgress       += UpdateProgress;
        _scanner.EndProgress          += EndProgress;
        _scanner.InitBlockMap         += InitBlockMap;
        _scanner.ScanSpeed            += ScanSpeed;

        ScanResults results = _scanner.Scan();

        // Flush any remaining pending sector data
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            lock(_pendingSectorDataLock)
            {
                if(BlockMapSectorData != null)
                {
                    foreach((ulong startingSector, double duration) item in _pendingSectorData)
                        BlockMapSectorData.Add(item);
                }

                _pendingSectorData.Clear();
            }

            TotalTime = string.Format(Aaru.Localization.Core.Took_a_total_of_0_1_processing_commands,
                                      results.TotalTime.Seconds().Humanize(minUnit: TimeUnit.Second),
                                      results.ProcessingTime.Seconds().Humanize(minUnit: TimeUnit.Second));

            AvgSpeed = string.Format(Aaru.Localization.Core.Average_speed_0,
                                     ByteSize.FromMegabytes(results.AvgSpeed).Per(1.Seconds()).Humanize());

            MaxSpeed = string.Format(Aaru.Localization.Core.Fastest_speed_burst_0,
                                     ByteSize.FromMegabytes(results.MaxSpeed).Per(1.Seconds()).Humanize());

            MinSpeed = string.Format(Aaru.Localization.Core.Slowest_speed_burst_0,
                                     ByteSize.FromMegabytes(results.MinSpeed).Per(1.Seconds()).Humanize());

            A = string.Format(Aaru.Localization.Core._0_sectors_took_less_than_3_ms,                        results.A);
            B = string.Format(Aaru.Localization.Core._0_sectors_took_less_than_10_ms_but_more_than_3_ms,    results.B);
            C = string.Format(Aaru.Localization.Core._0_sectors_took_less_than_50_ms_but_more_than_10_ms,   results.C);
            D = string.Format(Aaru.Localization.Core._0_sectors_took_less_than_150_ms_but_more_than_50_ms,  results.D);
            E = string.Format(Aaru.Localization.Core._0_sectors_took_less_than_500_ms_but_more_than_150_ms, results.E);
            F = string.Format(Aaru.Localization.Core._0_sectors_took_more_than_500_ms,                      results.F);

            UnreadableSectors = string.Format(Aaru.Localization.Core._0_sectors_could_not_be_read,
                                              results.UnreadableSectors?.Count ?? 0);
        });

        // TODO: Show list of unreadable sectors
        /*
        if(results.UnreadableSectors.Count > 0)
            foreach(ulong bad in results.UnreadableSectors)
                string.Format("Sector {0} could not be read", bad);
*/

        // TODO: Show results
        /*

        if(results.SeekTotal != 0 || results.SeekMin != double.MaxValue || results.SeekMax != double.MinValue)

            string.Format("Testing {0} seeks, longest seek took {1:F3} ms, fastest one took {2:F3} ms. ({3:F3} ms average)",
                                 results.SeekTimes, results.SeekMax, results.SeekMin, results.SeekTotal / 1000);
                                 */

        Statistics.AddCommand("media-scan");

        await WorkFinished();
    }

    async void ScanSpeed(ulong sector, double currentSpeed) => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        SpeedData?.Add((sector, currentSpeed));

        if(currentSpeed > MaxY) MaxY = currentSpeed + currentSpeed / 10d;
    });

    async void InitBlockMap(ulong blocks, ulong blockSize, ulong blocksToRead, ushort currentProfile) =>
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ScanBlockSize      = (uint)blocksToRead;
            BlockMapSectorData = [];
            MaxSector          = blocks;
            SpeedData          = [];
            _blocksToRead      = blocksToRead;

            switch(currentProfile)
            {
                case 0x0005: // CD and DDCD
                case 0x0008:
                case 0x0009:
                case 0x000A:
                case 0x0020:
                case 0x0021:
                case 0x0022:
                    SpeedMultiplier = 150;
                    MaxGraphSpeed   = 11250;

                    break;
                case 0x0010: // DVD SL
                case 0x0011:
                case 0x0012:
                case 0x0013:
                case 0x0014:
                case 0x0018:
                case 0x001A:
                case 0x001B:
                case 0x0015: // DVD DL
                case 0x0016:
                case 0x0017:
                case 0x002A:
                case 0x002B:
                    SpeedMultiplier = 1353;
                    MaxGraphSpeed   = 32472; // 24x DVD-ROM cap for graph scaling

                    break;
                case 0x0041:
                case 0x0042:
                case 0x0043:
                case 0x0040: // BD
                    SpeedMultiplier = 4500;
                    MaxGraphSpeed   = 108000; // 24x BD-ROM cap for graph scaling

                    break;
                case 0x0050: // HD DVD
                case 0x0051:
                case 0x0052:
                case 0x0053:
                case 0x0058:
                case 0x005A:
                    SpeedMultiplier = 4500;
                    MaxGraphSpeed   = 36550; // 8x HD-DVD cap for graph scaling

                    break;
                default:
                    SpeedMultiplier = 1353;
                    MaxGraphSpeed   = 1500000; // 1500 MB/s cap for graph scaling

                    break;
            }
        });

    async Task WorkFinished() => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        StopVisible     = false;
        StartVisible    = true;
        CloseVisible    = true;
        ProgressVisible = false;
    });

    async void EndProgress() => await Dispatcher.UIThread.InvokeAsync(() => { Progress1Visible = false; });

    async void UpdateProgress(string text, long current, long maximum) => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        ProgressText          = text;
        ProgressIndeterminate = false;

        ProgressMaxValue = maximum;
        ProgressValue    = current;
    });

    async void InitProgress() => await Dispatcher.UIThread.InvokeAsync(() => { Progress1Visible = true; });

    async void PulseProgress(string text) => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        ProgressText          = text;
        ProgressIndeterminate = true;
    });

#pragma warning disable AsyncFixer01
    async void StoppingErrorMessage(string text) => await Dispatcher.UIThread.InvokeAsync(async () =>
    {
        ProgressText = text;

        await MessageBoxManager.GetMessageBoxStandard(UI.Title_Error, $"{text}", ButtonEnum.Ok, Icon.Error)
                               .ShowWindowDialogAsync(_view);

        await WorkFinished();
    });
#pragma warning restore AsyncFixer01

    async void UpdateStatus(string text) => await Dispatcher.UIThread.InvokeAsync(() => { ProgressText = text; });

    async void OnScanUnreadable(ulong sector) => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        _localResults.Errored += _blocksToRead;
        UnreadableSectors = string.Format(Aaru.Localization.Core._0_sectors_could_not_be_read, _localResults.Errored);
    });

    async void OnScanTime(ulong sector, double duration)
    {
        // Update local results counters (thread-safe, no UI dispatch needed)
        switch(duration)
        {
            case < 3:
                _localResults.A += _blocksToRead;

                break;
            case >= 3 and < 10:
                _localResults.B += _blocksToRead;

                break;
            case >= 10 and < 50:
                _localResults.C += _blocksToRead;

                break;
            case >= 50 and < 150:
                _localResults.D += _blocksToRead;

                break;
            case >= 150 and < 500:
                _localResults.E += _blocksToRead;

                break;
            case >= 500:
                _localResults.F += _blocksToRead;

                break;
        }

        // Batch sector data updates
        List<(ulong sector, double duration)> itemsToAdd = null;

        lock(_pendingSectorDataLock)
        {
            _pendingSectorData.Add((sector, duration));

            // Only dispatch to UI thread every 50 items to reduce overhead
            if(_pendingSectorData.Count >= 50)
            {
                itemsToAdd = _pendingSectorData.ToList();
                _pendingSectorData.Clear();
            }
        }

        // Dispatch outside the lock
        if(itemsToAdd != null)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
                                                  {
                                                      foreach((ulong sector, double duration) item in itemsToAdd)
                                                          BlockMapSectorData.Add(item);

                                                      // Update text labels
                                                      A = string.Format(Aaru.Localization.Core
                                                                            ._0_sectors_took_less_than_3_ms,
                                                                        _localResults.A);

                                                      B = string.Format(Aaru.Localization.Core
                                                                            ._0_sectors_took_less_than_10_ms_but_more_than_3_ms,
                                                                        _localResults.B);

                                                      C = string.Format(Aaru.Localization.Core
                                                                            ._0_sectors_took_less_than_50_ms_but_more_than_10_ms,
                                                                        _localResults.C);

                                                      D = string.Format(Aaru.Localization.Core
                                                                            ._0_sectors_took_less_than_150_ms_but_more_than_50_ms,
                                                                        _localResults.D);

                                                      E = string.Format(Aaru.Localization.Core
                                                                            ._0_sectors_took_less_than_500_ms_but_more_than_150_ms,
                                                                        _localResults.E);

                                                      F = string.Format(Aaru.Localization.Core
                                                                            ._0_sectors_took_more_than_500_ms,
                                                                        _localResults.F);
                                                  },
                                                  DispatcherPriority.Background);
        }
    }
}