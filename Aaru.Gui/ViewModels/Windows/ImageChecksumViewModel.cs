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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Input;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using Aaru.Gui.Models;
using Aaru.Localization;
using Aaru.Logging;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sentry;

namespace Aaru.Gui.ViewModels.Windows;

public sealed partial class ImageChecksumViewModel : ViewModelBase
{
    // How many sectors to read at once
    const    uint        SECTORS_TO_READ = 256;
    const    string      MODULE_NAME     = "Image Checksum ViewModel";
    readonly IMediaImage _inputFormat;
    readonly Window      _view;
    [ObservableProperty]
    bool _adler32Checked;
    bool _cancel;
    [ObservableProperty]
    bool _checksumMediaChecked;
    [ObservableProperty]
    bool _checksumTracksChecked;
    [ObservableProperty]
    bool _checksumTracksVisible;
    [ObservableProperty]
    bool _closeCommandEnabled;
    [ObservableProperty]
    bool _closeCommandVisible;
    [ObservableProperty]
    bool _crc16Checked;
    [ObservableProperty]
    bool _crc32Checked;
    [ObservableProperty]
    bool _crc64Checked;
    [ObservableProperty]
    bool _fletcher16Checked;
    [ObservableProperty]
    bool _fletcher32Checked;
    [ObservableProperty]
    bool _md5Checked;
    [ObservableProperty]
    bool _mediaChecksumsVisible;
    [ObservableProperty]
    bool _optionsEnabled;
    [ObservableProperty]
    bool _optionsVisible;
    [ObservableProperty]
    bool _progress1Visible;
    [ObservableProperty]
    double _progress2Max;
    [ObservableProperty]
    string _progress2Text;
    [ObservableProperty]
    double _progress2Value;
    [ObservableProperty]
    bool _progress2Visible;
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
    bool _sha1Checked;
    [ObservableProperty]
    bool _sha256Checked;
    [ObservableProperty]
    bool _sha384Checked;
    [ObservableProperty]
    bool _sha512Checked;
    [ObservableProperty]
    bool _spamsumChecked;
    [ObservableProperty]
    bool _startCommandEnabled;
    [ObservableProperty]
    bool _startCommandVisible;
    [ObservableProperty]
    bool _stopCommandEnabled;
    [ObservableProperty]
    bool _stopCommandVisible;
    [ObservableProperty]
    string _title;
    [ObservableProperty]
    bool _trackChecksumsVisible;

    public ImageChecksumViewModel(IMediaImage inputFormat, Window view)
    {
        _view                 = view;
        _cancel               = false;
        _inputFormat          = inputFormat;
        ChecksumTracksChecked = ChecksumTracksVisible;
        OptionsVisible        = true;
        OptionsEnabled        = true;
        ChecksumMediaChecked  = true;
        ChecksumTracksChecked = true;
        Adler32Checked        = true;
        Crc16Checked          = true;
        Crc32Checked          = true;
        Md5Checked            = true;
        Sha1Checked           = true;
        SpamsumChecked        = true;
        TrackChecksums        = [];
        MediaChecksums        = [];
        StartCommand          = new RelayCommand(Start);
        CloseCommand          = new RelayCommand(Close);
        StopCommand           = new RelayCommand(Stop);
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
        catch(Exception ex)
        {
            ChecksumTracksVisible = false;
            SentrySdk.CaptureException(ex);
        }
    }

    public ObservableCollection<ChecksumModel> TrackChecksums { get; }
    public ObservableCollection<ChecksumModel> MediaChecksums { get; }
    public ICommand                            StartCommand   { get; }
    public ICommand                            CloseCommand   { get; }
    public ICommand                            StopCommand    { get; }

    void Start()
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

    void Close() => _view.Close();

    internal void Stop()
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
            catch(Exception ex)
            {
                SentrySdk.CaptureException(ex);

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
            Progress2Max     = _inputFormat.Info.Sectors / (double)SECTORS_TO_READ;

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

                        Progress2Max = currentTrack.EndSector - currentTrack.StartSector + 1;

                        ProgressValue++;
                    });

                    /*
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

                            errno = opticalMediaImage.ReadSector(i, false, out byte[] hiddenSector, out _);

                            if(errno != ErrorNumber.NoError)
                            {
                                AaruLogging.Error(string.Format(Aaru.Localization.Core.Error_0_reading_sector_1, errno, i));

                                _cancel = true;

                                break;
                            }

                            mediaChecksum?.Update(hiddenSector);
                        }
                    }
*/

                    AaruLogging.Debug(MODULE_NAME,
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
                                                                  out sector,
                                                                  out _);

                            if(errno != ErrorNumber.NoError)
                            {
                                AaruLogging.Error(string.Format(Aaru.Localization.Core.Error_0_reading_sector_1,
                                                                errno,
                                                                doneSectors));

                                _cancel = true;

                                continue;
                            }

                            ulong doneSectorsToInvoke = doneSectors;

                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Progress2Value = doneSectorsToInvoke;

                                Progress2Text = string.Format(UI.Hashing_sectors_0_to_2_of_track_1,
                                                              doneSectorsToInvoke,
                                                              currentTrack.Sequence,
                                                              doneSectorsToInvoke + SECTORS_TO_READ);
                            });

                            doneSectors += SECTORS_TO_READ;
                        }
                        else
                        {
                            errno = opticalMediaImage.ReadSectors(doneSectors,
                                                                  (uint)(sectors - doneSectors),
                                                                  currentTrack.Sequence,
                                                                  out sector,
                                                                  out _);

                            if(errno != ErrorNumber.NoError)
                            {
                                AaruLogging.Error(string.Format(Aaru.Localization.Core.Error_0_reading_sector_1,
                                                                errno,
                                                                doneSectors));

                                _cancel = true;

                                continue;
                            }

                            ulong doneSectorsToInvoke = doneSectors;

                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Progress2Value = (int)doneSectorsToInvoke;

                                Progress2Text = string.Format(UI.Hashing_sectors_0_to_2_of_track_1,
                                                              doneSectorsToInvoke,
                                                              currentTrack.Sequence,
                                                              doneSectorsToInvoke + (sectors - doneSectorsToInvoke));
                            });

                            doneSectors += sectors - doneSectors;
                        }

                        if(ChecksumMediaChecked) mediaChecksum?.Update(sector);

                        if(ChecksumTracksChecked) trackChecksum?.Update(sector);
                    }

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if(!ChecksumTracksChecked) return;

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

                /*
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

                        errno = opticalMediaImage.ReadSector(i, false, out byte[] hiddenSector, out _);

                        if(errno != ErrorNumber.NoError)
                        {
                            AaruLogging.Error(string.Format(Aaru.Localization.Core.Error_0_reading_sector_1, errno, i));

                            _cancel = true;

                            break;
                        }

                        mediaChecksum?.Update(hiddenSector);
                    }
                }
                */

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
                AaruLogging.Debug(Aaru.Localization.Core.Could_not_get_tracks_because_0, ex.Message);
                AaruLogging.WriteLine("Unable to get separate tracks, not checksumming them");
                AaruLogging.Exception(ex, Aaru.Localization.Core.Could_not_get_tracks_because_0, ex.Message);
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
                    errno = _inputFormat.ReadSectors(doneSectors, false, SECTORS_TO_READ, out sector, out _);

                    if(errno != ErrorNumber.NoError)
                    {
                        AaruLogging.Error(string.Format(Aaru.Localization.Core.Error_0_reading_sector_1,
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
                                                     false,
                                                     (uint)(_inputFormat.Info.Sectors - doneSectors),
                                                     out sector,
                                                     out _);

                    if(errno != ErrorNumber.NoError)
                    {
                        AaruLogging.Error(string.Format(Aaru.Localization.Core.Error_0_reading_sector_1,
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
            OptionsVisible      = false;
            ResultsVisible      = true;
            ProgressVisible     = false;
            StartCommandVisible = false;
            StopCommandVisible  = false;
            CloseCommandVisible = true;
        });
    }
}