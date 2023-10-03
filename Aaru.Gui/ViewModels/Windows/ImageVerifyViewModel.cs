// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ImageVerifyViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the image verification window.
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Aaru.Gui.Models;
using Aaru.Localization;
using Avalonia.Controls;
using Avalonia.Threading;
using Humanizer;
using Humanizer.Localisation;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Windows;

public sealed class ImageVerifyViewModel : ViewModelBase
{
    readonly IMediaImage _inputFormat;
    readonly Window      _view;
    bool                 _cancel;
    bool                 _closeVisible;
    string               _imageResultText;
    bool                 _imageResultVisible;
    bool                 _optionsVisible;
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
    bool                 _resultsVisible;
    string               _sectorErrorsText;
    bool                 _sectorErrorsVisible;
    string               _sectorsErrorsAllText;
    bool                 _sectorsErrorsAllVisible;
    bool                 _sectorSummaryVisible;
    string               _sectorsUnknownAllText;
    bool                 _sectorsUnknownAllVisible;
    string               _sectorsUnknownsText;
    bool                 _sectorsUnknownsVisible;
    bool                 _startVisible;
    bool                 _stopEnabled;
    bool                 _stopVisible;
    string               _totalSectorErrorsText;
    string               _totalSectorErrorsUnknownsText;
    string               _totalSectorsText;
    string               _totalSectorUnknownsText;
    bool                 _verifyImageChecked;
    bool                 _verifyImageEnabled;
    bool                 _verifySectorsChecked;
    bool                 _verifySectorsEnabled;
    bool                 _verifySectorsVisible;

    public ImageVerifyViewModel(IMediaImage inputFormat, Window view)
    {
        _view                = view;
        StartCommand         = ReactiveCommand.Create(ExecuteStartCommand);
        CloseCommand         = ReactiveCommand.Create(ExecuteCloseCommand);
        StopCommand          = ReactiveCommand.Create(ExecuteStopCommand);
        _inputFormat         = inputFormat;
        _cancel              = false;
        ErrorList            = new ObservableCollection<LbaModel>();
        UnknownList          = new ObservableCollection<LbaModel>();
        VerifyImageEnabled   = true;
        VerifySectorsEnabled = true;
        CloseVisible         = true;
        StartVisible         = true;
        OptionsVisible       = true;
    }

    public string VerifyImageLabel   => UI.Verify_media_image_if_supported;
    public string VerifySectorsLabel => UI.Verify_all_sectors_if_supported;
    public string LBALabel           => UI.Title_LBA;
    public string StartLabel         => UI.ButtonLabel_Start;
    public string CloseLabel         => UI.ButtonLabel_Close;
    public string StopLabel          => UI.ButtonLabel_Stop;

    public ObservableCollection<LbaModel> ErrorList    { get; }
    public ObservableCollection<LbaModel> UnknownList  { get; }
    public ReactiveCommand<Unit, Unit>    StartCommand { get; }
    public ReactiveCommand<Unit, Unit>    CloseCommand { get; }
    public ReactiveCommand<Unit, Unit>    StopCommand  { get; }

    public bool VerifyImageEnabled
    {
        get => _verifyImageEnabled;
        set => this.RaiseAndSetIfChanged(ref _verifyImageEnabled, value);
    }

    public bool VerifySectorsEnabled
    {
        get => _verifySectorsEnabled;
        set => this.RaiseAndSetIfChanged(ref _verifySectorsEnabled, value);
    }

    public bool VerifySectorsVisible
    {
        get => _verifySectorsVisible;
        set => this.RaiseAndSetIfChanged(ref _verifySectorsVisible, value);
    }

    public double ProgressMaxValue
    {
        get => _progressMaxValue;
        set => this.RaiseAndSetIfChanged(ref _progressMaxValue, value);
    }

    public bool VerifyImageChecked
    {
        get => _verifyImageChecked;
        set => this.RaiseAndSetIfChanged(ref _verifyImageChecked, value);
    }

    public bool ProgressIndeterminate
    {
        get => _progressIndeterminate;
        set => this.RaiseAndSetIfChanged(ref _progressIndeterminate, value);
    }

    public bool ImageResultVisible
    {
        get => _imageResultVisible;
        set => this.RaiseAndSetIfChanged(ref _imageResultVisible, value);
    }

    public string ImageResultText
    {
        get => _imageResultText;
        set => this.RaiseAndSetIfChanged(ref _imageResultText, value);
    }

    public bool VerifySectorsChecked
    {
        get => _verifySectorsChecked;
        set => this.RaiseAndSetIfChanged(ref _verifySectorsChecked, value);
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

    public bool SectorsErrorsAllVisible
    {
        get => _sectorsErrorsAllVisible;
        set => this.RaiseAndSetIfChanged(ref _sectorsErrorsAllVisible, value);
    }

    public string SectorsErrorsAllText
    {
        get => _sectorsErrorsAllText;
        set => this.RaiseAndSetIfChanged(ref _sectorsErrorsAllText, value);
    }

    public bool SectorsUnknownAllVisible
    {
        get => _sectorsUnknownAllVisible;
        set => this.RaiseAndSetIfChanged(ref _sectorsUnknownAllVisible, value);
    }

    public string SectorsUnknownAllText
    {
        get => _sectorsUnknownAllText;
        set => this.RaiseAndSetIfChanged(ref _sectorsUnknownAllText, value);
    }

    public string SectorErrorsText
    {
        get => _sectorErrorsText;
        set => this.RaiseAndSetIfChanged(ref _sectorErrorsText, value);
    }

    public bool SectorErrorsVisible
    {
        get => _sectorErrorsVisible;
        set => this.RaiseAndSetIfChanged(ref _sectorErrorsVisible, value);
    }

    public bool SectorsUnknownsVisible
    {
        get => _sectorsUnknownsVisible;
        set => this.RaiseAndSetIfChanged(ref _sectorsUnknownsVisible, value);
    }

    public string SectorsUnknownsText
    {
        get => _sectorsUnknownsText;
        set => this.RaiseAndSetIfChanged(ref _sectorsUnknownsText, value);
    }

    public bool SectorSummaryVisible
    {
        get => _sectorSummaryVisible;
        set => this.RaiseAndSetIfChanged(ref _sectorSummaryVisible, value);
    }

    public string TotalSectorsText
    {
        get => _totalSectorsText;
        set => this.RaiseAndSetIfChanged(ref _totalSectorsText, value);
    }

    public string TotalSectorErrorsText
    {
        get => _totalSectorErrorsText;
        set => this.RaiseAndSetIfChanged(ref _totalSectorErrorsText, value);
    }

    public string TotalSectorUnknownsText
    {
        get => _totalSectorUnknownsText;
        set => this.RaiseAndSetIfChanged(ref _totalSectorUnknownsText, value);
    }

    public string TotalSectorErrorsUnknownsText
    {
        get => _totalSectorErrorsUnknownsText;
        set => this.RaiseAndSetIfChanged(ref _totalSectorErrorsUnknownsText, value);
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

    public bool ProgressVisible
    {
        get => _progressVisible;
        set => this.RaiseAndSetIfChanged(ref _progressVisible, value);
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

    void ExecuteStartCommand()
    {
        VerifyImageEnabled   = false;
        VerifySectorsEnabled = false;
        CloseVisible         = false;
        StartVisible         = false;
        StopVisible          = true;
        ProgressVisible      = true;
        Progress2Visible     = false;

        VerifySectorsVisible = _inputFormat is IOpticalMediaImage or IVerifiableSectorsImage;

        // TODO: Do not offer the option to use this form if the image does not support any kind of verification
        new Thread(DoWork).Start();
    }

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    async void DoWork()
    {
        bool formatHasTracks;
        var  inputOptical           = _inputFormat as IOpticalMediaImage;
        var  verifiableSectorsImage = _inputFormat as IVerifiableSectorsImage;

        try
        {
            formatHasTracks = inputOptical?.Tracks?.Count > 0;
        }
        catch
        {
            formatHasTracks = false;
        }

        // Setup progress bars
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProgressVisible  = true;
            ProgressMaxValue = 0;

            if(VerifyImageChecked || VerifySectorsChecked)
                ProgressMaxValue = 1;

            if(formatHasTracks && inputOptical != null)
                ProgressMaxValue += inputOptical.Tracks.Count;
            else
            {
                if(VerifySectorsChecked)
                {
                    ProgressMaxValue = 2;
                    Progress2Visible = false;
                    Progress2Visible = false;
                }
                else
                {
                    Progress2Visible = true;
                    Progress2Visible = true;
                }
            }

            ProgressMaxValue++;
        });

        if(VerifyImageChecked)
        {
            if(_inputFormat is not IVerifiableImage verifiableImage)
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ImageResultVisible = true;
                    ImageResultText    = UI.Disc_image_does_not_support_verification;
                });
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressText = UI.Checking_media_image;

                    if(VerifySectorsChecked)
                        ProgressValue = 1;
                    else
                        ProgressIndeterminate = true;

                    Progress2Indeterminate = true;
                });

                var chkStopwatch = new Stopwatch();
                chkStopwatch.Start();
                bool?    discCheckStatus = verifiableImage.VerifyMediaImage();
                chkStopwatch.Stop();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ImageResultVisible = true;

                    ImageResultText = discCheckStatus switch
                    {
                        true  => UI.Disc_image_checksums_are_correct,
                        false => UI.Disc_image_checksums_are_incorrect,
                        null  => UI.Disc_image_does_not_contain_checksums
                    };
                });

                AaruConsole.VerboseWriteLine(UI.Checking_disc_image_checksums_took_0,
                                             chkStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second));
            }
        }

        if(VerifySectorsChecked)
        {
            var         chkStopwatch = new Stopwatch();
            List<ulong> failingLbas  = new();
            List<ulong> unknownLbas  = new();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Progress2Visible       = true;
                Progress2Indeterminate = false;
                Progress2MaxValue      = _inputFormat.Info.Sectors / 512d;
                StopEnabled            = true;
            });

            if(formatHasTracks)
            {
                ulong currentSectorAll = 0;

                chkStopwatch.Restart();

                foreach(Track currentTrack in inputOptical.Tracks)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ProgressText = string.Format(UI.Verifying_track_0_of_1, currentTrack.Sequence,
                                                     inputOptical.Tracks.Count);

                        ProgressValue++;
                    });

                    ulong remainingSectors = currentTrack.EndSector - currentTrack.StartSector;
                    ulong currentSector    = 0;

                    while(remainingSectors > 0)
                    {
                        if(_cancel)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                CloseVisible = true;
                                StartVisible = false;
                                StopVisible  = false;
                            });

                            return;
                        }

                        ulong all = currentSectorAll;

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Progress2Value = all / 512d;

                            Progress2Text = string.Format(UI.Checking_sector_0_of_1_on_track_2, all,
                                                          _inputFormat.Info.Sectors, currentTrack.Sequence);
                        });

                        List<ulong> tempFailingLbas;
                        List<ulong> tempUnknownLbas;

                        if(remainingSectors < 512)
                            inputOptical.VerifySectors(currentSector, (uint)remainingSectors, currentTrack.Sequence,
                                                       out tempFailingLbas, out tempUnknownLbas);
                        else
                            inputOptical.VerifySectors(currentSector, 512, currentTrack.Sequence, out tempFailingLbas,
                                                       out tempUnknownLbas);

                        failingLbas.AddRange(tempFailingLbas);

                        unknownLbas.AddRange(tempUnknownLbas);

                        if(remainingSectors < 512)
                        {
                            currentSector    += remainingSectors;
                            currentSectorAll += remainingSectors;
                            remainingSectors =  0;
                        }
                        else
                        {
                            currentSector    += 512;
                            currentSectorAll += 512;
                            remainingSectors -= 512;
                        }
                    }
                }

                chkStopwatch.Stop();
            }
            else if(verifiableSectorsImage is not null)
            {
                ulong remainingSectors = _inputFormat.Info.Sectors;
                ulong currentSector    = 0;

                chkStopwatch.Restart();

                while(remainingSectors > 0)
                {
                    if(_cancel)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            CloseVisible = true;
                            StartVisible = false;
                            StopVisible  = false;
                        });

                        return;
                    }

                    ulong sector = currentSector;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Progress2Value = (int)(sector / 512);
                        Progress2Text  = string.Format(UI.Checking_sector_0_of_1, sector, _inputFormat.Info.Sectors);
                    });

                    List<ulong> tempFailingLbas;
                    List<ulong> tempUnknownLbas;

                    if(remainingSectors < 512)
                        verifiableSectorsImage.VerifySectors(currentSector, (uint)remainingSectors, out tempFailingLbas,
                                                             out tempUnknownLbas);
                    else
                        verifiableSectorsImage.VerifySectors(currentSector, 512, out tempFailingLbas,
                                                             out tempUnknownLbas);

                    failingLbas.AddRange(tempFailingLbas);

                    unknownLbas.AddRange(tempUnknownLbas);

                    if(remainingSectors < 512)
                    {
                        currentSector    += remainingSectors;
                        remainingSectors =  0;
                    }
                    else
                    {
                        currentSector    += 512;
                        remainingSectors -= 512;
                    }
                }

                chkStopwatch.Stop();
            }

            AaruConsole.VerboseWriteLine(UI.Checking_sector_checksums_took_0,
                                         chkStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second));

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if(failingLbas.Count > 0)
                {
                    if(failingLbas.Count == (int)_inputFormat.Info.Sectors)
                    {
                        SectorsErrorsAllVisible = true;
                        SectorsErrorsAllText    = UI.All_sectors_contain_errors;
                    }
                    else
                    {
                        SectorErrorsText    = UI.LBAs_with_error;
                        SectorErrorsVisible = true;

                        foreach(ulong t in failingLbas)
                            ErrorList.Add(new LbaModel
                            {
                                Lba = t.ToString()
                            });
                    }
                }

                if(unknownLbas.Count > 0)
                {
                    if(unknownLbas.Count == (int)_inputFormat.Info.Sectors)
                    {
                        SectorsUnknownAllVisible = true;
                        SectorsUnknownAllText    = UI.All_sectors_are_unknown;
                    }
                    else
                    {
                        SectorsUnknownsText    = UI.Unknown_LBAs;
                        SectorsUnknownsVisible = true;

                        foreach(ulong t in unknownLbas)
                            UnknownList.Add(new LbaModel
                            {
                                Lba = t.ToString()
                            });
                    }
                }

                SectorSummaryVisible    = true;
                TotalSectorsText        = string.Format(UI.Total_sectors, _inputFormat.Info.Sectors);
                TotalSectorErrorsText   = string.Format(UI.Total_errors, failingLbas.Count);
                TotalSectorUnknownsText = string.Format(UI.Total_unknowns, unknownLbas.Count);

                TotalSectorErrorsUnknownsText =
                    string.Format(UI.Total_errors_plus_unknowns, failingLbas.Count + unknownLbas.Count);
            });
        }

        Statistics.AddCommand("verify");

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            OptionsVisible  = false;
            ResultsVisible  = true;
            ProgressVisible = false;
            StartVisible    = false;
            StopVisible     = false;
            CloseVisible    = true;
        });
    }

    void ExecuteCloseCommand() => _view.Close();

    internal void ExecuteStopCommand()
    {
        _cancel     = true;
        StopEnabled = false;
    }
}