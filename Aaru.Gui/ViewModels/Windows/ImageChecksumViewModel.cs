// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ImageChecksumViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the image checksum window.
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
using System.Reactive;
using System.Threading;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Aaru.Gui.Models;
using Aaru.Localization;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Windows;

public sealed class ImageChecksumViewModel : ViewModelBase
{
    // How many sectors to read at once
    const    uint        SECTORS_TO_READ = 256;
    const    string      MODULE_NAME     = "Image Checksum ViewModel";
    readonly IMediaImage _inputFormat;
    readonly Window      _view;
    bool                 _adler32Checked;
    bool                 _cancel;
    bool                 _checksumMediaChecked;
    bool                 _checksumTracksChecked;
    bool                 _checksumTracksVisible;
    bool                 _closeCommandEnabled;
    bool                 _closeCommandVisible;
    bool                 _crc16Checked;
    bool                 _crc32Checked;
    bool                 _crc64Checked;
    bool                 _fletcher16Checked;
    bool                 _fletcher32Checked;
    bool                 _md5Checked;
    bool                 _mediaChecksumsVisible;
    bool                 _optionsEnabled;
    bool                 _progress1Visible;
    double               _progress2Max;
    string               _progress2Text;
    double               _progress2Value;
    bool                 _progress2Visible;
    double               _progressMax;
    string               _progressText;
    double               _progressValue;
    bool                 _progressVisible;
    bool                 _resultsVisible;
    bool                 _sha1Checked;
    bool                 _sha256Checked;
    bool                 _sha384Checked;
    bool                 _sha512Checked;
    bool                 _spamsumChecked;
    bool                 _startCommandEnabled;
    bool                 _startCommandVisible;
    bool                 _stopCommandEnabled;
    bool                 _stopCommandVisible;
    string               _title;
    bool                 _trackChecksumsVisible;

    public ImageChecksumViewModel(IMediaImage inputFormat, Window view)
    {
        _view                 = view;
        _cancel               = false;
        _inputFormat          = inputFormat;
        ChecksumTracksChecked = ChecksumTracksVisible;
        OptionsEnabled        = true;
        ChecksumMediaChecked  = true;
        ChecksumTracksChecked = true;
        Adler32Checked        = true;
        Crc16Checked          = true;
        Crc32Checked          = true;
        Md5Checked            = true;
        Sha1Checked           = true;
        SpamsumChecked        = true;
        TrackChecksums        = new ObservableCollection<ChecksumModel>();
        MediaChecksums        = new ObservableCollection<ChecksumModel>();
        StartCommand          = ReactiveCommand.Create(ExecuteStartCommand);
        CloseCommand          = ReactiveCommand.Create(ExecuteCloseCommand);
        StopCommand           = ReactiveCommand.Create(ExecuteStopCommand);
        StopCommandVisible    = false;
        StartCommandVisible   = true;
        CloseCommandVisible   = true;
        StopCommandEnabled    = true;
        StartCommandEnabled   = true;
        CloseCommandEnabled   = true;

        try
        {
            ChecksumTracksVisible = (inputFormat as IOpticalMediaImage)?.Tracks?.Count > 0;
        }
        catch
        {
            ChecksumTracksVisible = false;
        }
    }

    public string ChecksumMediaLabel  => UI.Checksums_the_whole_disc;
    public string ChecksumTracksLabel => UI.Checksums_each_track_separately;
    public string Adler32Label        => UI.Calculates_Adler_32;
    public string Crc16Label          => UI.Calculates_CRC16;
    public string Crc32Label          => UI.Calculates_CRC32;
    public string Crc64Label          => UI.Calculates_CRC64_ECMA;
    public string Fletcher16Label     => UI.Calculates_Fletcher_16;
    public string Fletcher32Label     => UI.Calculates_Fletcher_32;
    public string Md5Label            => UI.Calculates_MD5;
    public string Sha1Label           => UI.Calculates_SHA1;
    public string Sha256Label         => UI.Calculates_SHA256;
    public string Sha384Label         => UI.Calculates_SHA384;
    public string Sha512Label         => UI.Calculates_SHA512;
    public string SpamSumLabel        => UI.Calculates_SpamSum_fuzzy_hash;
    public string TrackChecksumsLabel => UI.Title_Track_checksums;
    public string TrackLabel          => Localization.Core.Title_Track;
    public string AlgorithmsLabel     => UI.Title_Algorithms;
    public string HashLabel           => UI.Title_Hash;
    public string MediaChecksumsLabel => UI.Title_Media_checksums;
    public string StartLabel          => UI.ButtonLabel_Start;
    public string CloseLabel          => UI.ButtonLabel_Close;
    public string StopLabel           => UI.ButtonLabel_Stop;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public bool OptionsEnabled
    {
        get => _optionsEnabled;
        set => this.RaiseAndSetIfChanged(ref _optionsEnabled, value);
    }

    public bool ChecksumMediaChecked
    {
        get => _checksumMediaChecked;
        set => this.RaiseAndSetIfChanged(ref _checksumMediaChecked, value);
    }

    public bool ChecksumTracksChecked
    {
        get => _checksumTracksChecked;
        set => this.RaiseAndSetIfChanged(ref _checksumTracksChecked, value);
    }

    public bool Adler32Checked
    {
        get => _adler32Checked;
        set => this.RaiseAndSetIfChanged(ref _adler32Checked, value);
    }

    public bool Crc16Checked
    {
        get => _crc16Checked;
        set => this.RaiseAndSetIfChanged(ref _crc16Checked, value);
    }

    public bool Crc32Checked
    {
        get => _crc32Checked;
        set => this.RaiseAndSetIfChanged(ref _crc32Checked, value);
    }

    public bool Crc64Checked
    {
        get => _crc64Checked;
        set => this.RaiseAndSetIfChanged(ref _crc64Checked, value);
    }

    public bool Fletcher16Checked
    {
        get => _fletcher16Checked;
        set => this.RaiseAndSetIfChanged(ref _fletcher16Checked, value);
    }

    public bool Fletcher32Checked
    {
        get => _fletcher32Checked;
        set => this.RaiseAndSetIfChanged(ref _fletcher32Checked, value);
    }

    public bool Md5Checked
    {
        get => _md5Checked;
        set => this.RaiseAndSetIfChanged(ref _md5Checked, value);
    }

    public bool Sha1Checked
    {
        get => _sha1Checked;
        set => this.RaiseAndSetIfChanged(ref _sha1Checked, value);
    }

    public bool Sha256Checked
    {
        get => _sha256Checked;
        set => this.RaiseAndSetIfChanged(ref _sha256Checked, value);
    }

    public bool Sha384Checked
    {
        get => _sha384Checked;
        set => this.RaiseAndSetIfChanged(ref _sha384Checked, value);
    }

    public bool Sha512Checked
    {
        get => _sha512Checked;
        set => this.RaiseAndSetIfChanged(ref _sha512Checked, value);
    }

    public bool SpamsumChecked
    {
        get => _spamsumChecked;
        set => this.RaiseAndSetIfChanged(ref _spamsumChecked, value);
    }

    public bool ResultsVisible
    {
        get => _resultsVisible;
        set => this.RaiseAndSetIfChanged(ref _resultsVisible, value);
    }

    public bool TrackChecksumsVisible
    {
        get => _trackChecksumsVisible;
        set => this.RaiseAndSetIfChanged(ref _trackChecksumsVisible, value);
    }

    public bool MediaChecksumsVisible
    {
        get => _mediaChecksumsVisible;
        set => this.RaiseAndSetIfChanged(ref _mediaChecksumsVisible, value);
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

    public bool StartCommandEnabled
    {
        get => _startCommandEnabled;
        set => this.RaiseAndSetIfChanged(ref _startCommandEnabled, value);
    }

    public bool StartCommandVisible
    {
        get => _startCommandVisible;
        set => this.RaiseAndSetIfChanged(ref _startCommandVisible, value);
    }

    public bool CloseCommandEnabled
    {
        get => _closeCommandEnabled;
        set => this.RaiseAndSetIfChanged(ref _closeCommandEnabled, value);
    }

    public bool CloseCommandVisible
    {
        get => _closeCommandVisible;
        set => this.RaiseAndSetIfChanged(ref _closeCommandVisible, value);
    }

    public bool StopCommandEnabled
    {
        get => _stopCommandEnabled;
        set => this.RaiseAndSetIfChanged(ref _stopCommandEnabled, value);
    }

    public bool StopCommandVisible
    {
        get => _stopCommandVisible;
        set => this.RaiseAndSetIfChanged(ref _stopCommandVisible, value);
    }

    public bool ChecksumTracksVisible
    {
        get => _checksumTracksVisible;
        set => this.RaiseAndSetIfChanged(ref _stopCommandVisible, value);
    }

    public ObservableCollection<ChecksumModel> TrackChecksums { get; }
    public ObservableCollection<ChecksumModel> MediaChecksums { get; }
    public ReactiveCommand<Unit, Unit>         StartCommand   { get; }
    public ReactiveCommand<Unit, Unit>         CloseCommand   { get; }
    public ReactiveCommand<Unit, Unit>         StopCommand    { get; }

    void ExecuteStartCommand()
    {
        OptionsEnabled      = false;
        CloseCommandVisible = false;
        StartCommandVisible = false;
        StopCommandVisible  = true;
        ProgressVisible     = true;
        Progress1Visible    = true;
        Progress2Visible    = false;

        new Thread(DoWork)
        {
            Priority = ThreadPriority.BelowNormal
        }.Start();
    }

    void ExecuteCloseCommand() => _view.Close();

    internal void ExecuteStopCommand()
    {
        _cancel            = true;
        StopCommandEnabled = false;
    }

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    async void DoWork()
    {
        var opticalMediaImage = _inputFormat as IOpticalMediaImage;
        var formatHasTracks   = false;

        if(opticalMediaImage != null)
        {
            try
            {
                formatHasTracks = opticalMediaImage.Tracks?.Count > 0;
            }
            catch
            {
                formatHasTracks = false;
            }
        }

        // Setup progress bars
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProgressVisible  = true;
            Progress1Visible = true;
            Progress2Visible = true;
            ProgressMax      = 1;
            Progress2Max     = (int)(_inputFormat.Info.Sectors / SECTORS_TO_READ);

            if(formatHasTracks && ChecksumTracksChecked && opticalMediaImage != null)
                ProgressMax += opticalMediaImage.Tracks.Count;
            else
            {
                ProgressMax      = 2;
                Progress2Visible = false;
            }
        });

        var enabledChecksums = new EnableChecksum();

        if(Adler32Checked) enabledChecksums |= EnableChecksum.Adler32;

        if(Crc16Checked) enabledChecksums |= EnableChecksum.Crc16;

        if(Crc32Checked) enabledChecksums |= EnableChecksum.Crc32;

        if(Crc64Checked) enabledChecksums |= EnableChecksum.Crc64;

        if(Md5Checked) enabledChecksums |= EnableChecksum.Md5;

        if(Sha1Checked) enabledChecksums |= EnableChecksum.Sha1;

        if(Sha256Checked) enabledChecksums |= EnableChecksum.Sha256;

        if(Sha384Checked) enabledChecksums |= EnableChecksum.Sha384;

        if(Sha512Checked) enabledChecksums |= EnableChecksum.Sha512;

        if(SpamsumChecked) enabledChecksums |= EnableChecksum.SpamSum;

        if(Fletcher16Checked) enabledChecksums |= EnableChecksum.Fletcher16;

        if(Fletcher32Checked) enabledChecksums |= EnableChecksum.Fletcher32;

        Checksum    mediaChecksum = null;
        ErrorNumber errno;

        if(opticalMediaImage?.Tracks != null)
        {
            try
            {
                Checksum trackChecksum = null;

                if(ChecksumMediaChecked) mediaChecksum = new Checksum(enabledChecksums);

                ulong previousTrackEnd = 0;

                foreach(Track currentTrack in opticalMediaImage.Tracks)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ProgressText = string.Format(UI.Hashing_track_0_of_1,
                                                     currentTrack.Sequence,
                                                     opticalMediaImage.Tracks.Count);

                        ProgressValue++;
                    });

                    if(currentTrack.StartSector - previousTrackEnd != 0 && ChecksumMediaChecked)
                    {
                        for(ulong i = previousTrackEnd + 1; i < currentTrack.StartSector; i++)
                        {
                            ulong sector = i;

                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Progress2Value = (int)(sector / SECTORS_TO_READ);
                                Progress2Text  = $"Hashing track-less sector {sector}";
                            });

                            errno = opticalMediaImage.ReadSector(i, out byte[] hiddenSector);

                            if(errno != ErrorNumber.NoError)
                            {
                                AaruConsole.ErrorWriteLine(string.Format(Localization.Core.Error_0_reading_sector_1,
                                                                         errno,
                                                                         i));

                                _cancel = true;

                                break;
                            }

                            mediaChecksum?.Update(hiddenSector);
                        }
                    }

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               UI.Track_0_starts_at_sector_1_and_ends_at_sector_2,
                                               currentTrack.Sequence,
                                               currentTrack.StartSector,
                                               currentTrack.EndSector);

                    if(ChecksumTracksChecked) trackChecksum = new Checksum(enabledChecksums);

                    ulong sectors     = currentTrack.EndSector - currentTrack.StartSector + 1;
                    ulong doneSectors = 0;

                    while(doneSectors < sectors)
                    {
                        if(_cancel)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                CloseCommandVisible = true;
                                StartCommandVisible = false;
                                StopCommandVisible  = false;
                            });

                            return;
                        }

                        byte[] sector;

                        if(sectors - doneSectors >= SECTORS_TO_READ)
                        {
                            errno = opticalMediaImage.ReadSectors(doneSectors,
                                                                  SECTORS_TO_READ,
                                                                  currentTrack.Sequence,
                                                                  out sector);

                            if(errno != ErrorNumber.NoError)
                            {
                                AaruConsole.ErrorWriteLine(string.Format(Localization.Core.Error_0_reading_sector_1,
                                                                         errno,
                                                                         doneSectors));

                                _cancel = true;

                                continue;
                            }

                            ulong doneSectorsToInvoke = doneSectors;

                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Progress2Value = (int)(doneSectorsToInvoke / SECTORS_TO_READ);

                                Progress2Text = $"Hashing sectors {doneSectorsToInvoke} to {
                                    doneSectorsToInvoke + SECTORS_TO_READ} of track {currentTrack.Sequence}";
                            });

                            doneSectors += SECTORS_TO_READ;
                        }
                        else
                        {
                            errno = opticalMediaImage.ReadSectors(doneSectors,
                                                                  (uint)(sectors - doneSectors),
                                                                  currentTrack.Sequence,
                                                                  out sector);

                            if(errno != ErrorNumber.NoError)
                            {
                                AaruConsole.ErrorWriteLine(string.Format(Localization.Core.Error_0_reading_sector_1,
                                                                         errno,
                                                                         doneSectors));

                                _cancel = true;

                                continue;
                            }

                            ulong doneSectorsToInvoke = doneSectors;

                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Progress2Value = (int)(doneSectorsToInvoke / SECTORS_TO_READ);

                                Progress2Text = $"Hashing sectors {doneSectorsToInvoke} to {
                                    doneSectorsToInvoke + (sectors - doneSectorsToInvoke)} of track {
                                        currentTrack.Sequence}";
                            });

                            doneSectors += sectors - doneSectors;
                        }

                        if(ChecksumMediaChecked) mediaChecksum?.Update(sector);

                        if(ChecksumTracksChecked) trackChecksum?.Update(sector);
                    }

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if(ChecksumTracksChecked != true) return;

                        if(trackChecksum == null) return;

                        foreach(CommonTypes.AaruMetadata.Checksum chk in trackChecksum.End())
                        {
                            TrackChecksums.Add(new ChecksumModel
                            {
                                Track     = currentTrack.Sequence.ToString(),
                                Algorithm = chk.Type.ToString(),
                                Hash      = chk.Value
                            });
                        }
                    });

                    previousTrackEnd = currentTrack.EndSector;
                }

                if(opticalMediaImage.Info.Sectors - previousTrackEnd != 0 && ChecksumMediaChecked)
                {
                    for(ulong i = previousTrackEnd + 1; i < opticalMediaImage.Info.Sectors; i++)
                    {
                        ulong sector = i;

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Progress2Value = (int)(sector / SECTORS_TO_READ);
                            Progress2Text  = $"Hashing track-less sector {sector}";
                        });

                        errno = opticalMediaImage.ReadSector(i, out byte[] hiddenSector);

                        if(errno != ErrorNumber.NoError)
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization.Core.Error_0_reading_sector_1,
                                                                     errno,
                                                                     i));

                            _cancel = true;

                            break;
                        }

                        mediaChecksum?.Update(hiddenSector);
                    }
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if(mediaChecksum == null) return;

                    foreach(CommonTypes.AaruMetadata.Checksum chk in mediaChecksum.End())
                    {
                        MediaChecksums.Add(new ChecksumModel
                        {
                            Algorithm = chk.Type.ToString(),
                            Hash      = chk.Value
                        });
                    }
                });
            }
            catch(Exception ex)
            {
                AaruConsole.DebugWriteLine(Localization.Core.Could_not_get_tracks_because_0, ex.Message);
                AaruConsole.WriteLine("Unable to get separate tracks, not checksumming them");
                AaruConsole.WriteException(ex);
            }
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() => { Progress1Visible = false; });

            mediaChecksum = new Checksum(enabledChecksums);

            ulong doneSectors = 0;

            while(doneSectors < _inputFormat.Info.Sectors)
            {
                if(_cancel)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        CloseCommandVisible = true;
                        StartCommandVisible = false;
                        StopCommandVisible  = false;
                    });

                    return;
                }

                byte[] sector;

                if(_inputFormat.Info.Sectors - doneSectors >= SECTORS_TO_READ)
                {
                    errno = _inputFormat.ReadSectors(doneSectors, SECTORS_TO_READ, out sector);

                    if(errno != ErrorNumber.NoError)
                    {
                        AaruConsole.ErrorWriteLine(string.Format(Localization.Core.Error_0_reading_sector_1,
                                                                 errno,
                                                                 doneSectors));

                        _cancel = true;

                        continue;
                    }

                    ulong doneSectorsToInvoke = doneSectors;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Progress2Value = (int)(doneSectorsToInvoke / SECTORS_TO_READ);

                        Progress2Text = string.Format(UI.Hashing_sectors_0_to_1,
                                                      doneSectorsToInvoke,
                                                      doneSectorsToInvoke + SECTORS_TO_READ);
                    });

                    doneSectors += SECTORS_TO_READ;
                }
                else
                {
                    errno = _inputFormat.ReadSectors(doneSectors,
                                                     (uint)(_inputFormat.Info.Sectors - doneSectors),
                                                     out sector);

                    if(errno != ErrorNumber.NoError)
                    {
                        AaruConsole.ErrorWriteLine(string.Format(Localization.Core.Error_0_reading_sector_1,
                                                                 errno,
                                                                 doneSectors));

                        _cancel = true;

                        continue;
                    }

                    ulong doneSectorsToInvoke = doneSectors;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Progress2Value = (int)(doneSectorsToInvoke / SECTORS_TO_READ);

                        Progress2Text = string.Format(UI.Hashing_sectors_0_to_1,
                                                      doneSectorsToInvoke,
                                                      doneSectorsToInvoke +
                                                      (_inputFormat.Info.Sectors - doneSectorsToInvoke));
                    });

                    doneSectors += _inputFormat.Info.Sectors - doneSectors;
                }

                mediaChecksum.Update(sector);
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach(CommonTypes.AaruMetadata.Checksum chk in mediaChecksum.End())
                {
                    MediaChecksums.Add(new ChecksumModel
                    {
                        Algorithm = chk.Type.ToString(),
                        Hash      = chk.Value
                    });
                }
            });
        }

        if(ChecksumTracksChecked) await Dispatcher.UIThread.InvokeAsync(() => { TrackChecksumsVisible = true; });

        if(ChecksumMediaChecked) await Dispatcher.UIThread.InvokeAsync(() => { MediaChecksumsVisible = true; });

        Statistics.AddCommand("checksum");

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            OptionsEnabled      = false;
            ResultsVisible      = true;
            ProgressVisible     = false;
            StartCommandVisible = false;
            StopCommandVisible  = false;
            CloseCommandVisible = true;
        });
    }
}