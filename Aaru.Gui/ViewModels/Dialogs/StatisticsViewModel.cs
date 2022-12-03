// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : StatisticsViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the statistics dialog.
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

using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Aaru.Database;
using Aaru.Database.Models;
using Aaru.Gui.Models;
using Aaru.Gui.Views.Dialogs;
using Aaru.Localization;
using JetBrains.Annotations;
using ReactiveUI;
using NameCountModel = Aaru.Gui.Models.NameCountModel;

namespace Aaru.Gui.ViewModels.Dialogs;

public sealed class StatisticsViewModel : ViewModelBase
{
    readonly StatisticsDialog _view;
    string                    _checksumText;
    bool                      _checksumVisible;
    bool                      _commandsVisible;
    string                    _compareText;
    bool                      _compareVisible;
    string                    _convertImageText;
    bool                      _convertImageVisible;
    string                    _createSidecarText;
    bool                      _createSidecarVisible;
    string                    _decodeText;
    bool                      _decodeVisible;
    string                    _deviceInfoText;
    bool                      _deviceInfoVisible;
    string                    _deviceReportText;
    bool                      _deviceReportVisible;
    bool                      _devicesVisible;
    string                    _dumpMediaText;
    bool                      _dumpMediaVisible;
    string                    _entropyText;
    bool                      _entropyVisible;
    bool                      _filesystemsVisible;
    bool                      _filtersVisible;
    bool                      _formatsCommandVisible;
    string                    _formatsText;
    bool                      _formatsVisible;
    string                    _fsinfoText;
    bool                      _fsinfoVisible;
    string                    _imageInfoText;
    bool                      _imageInfoVisible;
    string                    _mediaInfoText;
    bool                      _mediaInfoVisible;
    string                    _mediaScanText;
    bool                      _mediaScanVisible;
    bool                      _mediasVisible;
    bool                      _partitionsVisible;
    string                    _printHexText;
    bool                      _printHexVisible;
    string                    _verifyText;
    bool                      _verifyVisible;

    public StatisticsViewModel(StatisticsDialog view)
    {
        _view        = view;
        Filters      = new ObservableCollection<NameCountModel>();
        Formats      = new ObservableCollection<NameCountModel>();
        Partitions   = new ObservableCollection<NameCountModel>();
        Filesystems  = new ObservableCollection<NameCountModel>();
        Devices      = new ObservableCollection<DeviceStatsModel>();
        Medias       = new ObservableCollection<MediaStatsModel>();
        CloseCommand = ReactiveCommand.Create(ExecuteCloseCommand);
        using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

        if(ctx.Commands.Any())
        {
            if(ctx.Commands.Any(c => c.Name == "analyze"))
            {
                foreach(Command oldAnalyze in ctx.Commands.Where(c => c.Name == "analyze"))
                {
                    oldAnalyze.Name = "fs-info";
                    ctx.Commands.Update(oldAnalyze);
                }

                ulong count = 0;

                foreach(Command fsInfo in ctx.Commands.Where(c => c.Name == "fs-info" && c.Synchronized))
                {
                    count += fsInfo.Count;
                    ctx.Remove(fsInfo);
                }

                if(count > 0)
                    ctx.Commands.Add(new Command
                    {
                        Count        = count,
                        Name         = "fs-info",
                        Synchronized = true
                    });

                ctx.SaveChanges();
            }

            if(ctx.Commands.Any(c => c.Name == "fs-info"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "fs-info" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "fs-info" && !c.Synchronized);

                FsInfoVisible = true;
                FsInfoText    = string.Format(UI.You_have_called_the_Filesystem_Info_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "checksum"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "checksum" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "checksum" && !c.Synchronized);

                ChecksumVisible = true;
                ChecksumText    = string.Format(UI.You_have_called_the_Checksum_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "compare"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "compare" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "compare" && !c.Synchronized);

                CompareVisible = true;
                CompareText    = string.Format(UI.You_have_called_the_Compare_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "convert-image"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "convert-image" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "convert-image" && !c.Synchronized);

                ConvertImageVisible = true;
                ConvertImageText    = string.Format(UI.You_have_called_the_Convert_Image_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "create-sidecar"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "create-sidecar" && c.Synchronized).
                                  Select(c => c.Count).FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "create-sidecar" && !c.Synchronized);

                CreateSidecarVisible = true;
                CreateSidecarText    = string.Format(UI.You_have_called_the_Create_Sidecar_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "decode"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "decode" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "decode" && !c.Synchronized);

                DecodeVisible = true;
                DecodeText    = string.Format(UI.You_have_called_the_Decode_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "device-info"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "device-info" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "device-info" && !c.Synchronized);

                DeviceInfoVisible = true;
                DeviceInfoText    = string.Format(UI.You_have_called_the_Device_Info_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "device-report"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "device-report" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "device-report" && !c.Synchronized);

                DeviceReportVisible = true;
                DeviceReportText    = string.Format(UI.You_have_called_the_Device_Report_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "dump-media"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "dump-media" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "dump-media" && !c.Synchronized);

                DumpMediaVisible = true;
                DumpMediaText    = string.Format(UI.You_have_called_the_Dump_Media_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "entropy"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "entropy" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "entropy" && !c.Synchronized);

                EntropyVisible = true;
                EntropyText    = string.Format(UI.You_have_called_the_Entropy_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "formats"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "formats" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "formats" && !c.Synchronized);

                FormatsCommandVisible = true;
                FormatsCommandText    = string.Format(UI.You_have_called_the_Formats_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "image-info"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "image-info" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "image-info" && !c.Synchronized);

                ImageInfoVisible = true;
                ImageInfoText    = string.Format(UI.You_have_called_the_Image_Info_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "media-info"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "media-info" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "media-info" && !c.Synchronized);

                MediaInfoVisible = true;
                MediaInfoText    = string.Format(UI.You_have_called_the_Media_Info_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "media-scan"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "media-scan" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "media-scan" && !c.Synchronized);

                MediaScanVisible = true;
                MediaScanText    = string.Format(UI.You_have_called_the_Media_Scan_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "printhex"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "printhex" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "printhex" && !c.Synchronized);

                PrintHexVisible = true;
                PrintHexText    = string.Format(UI.You_have_called_the_Print_Hex_command_0_times, count);
            }

            if(ctx.Commands.Any(c => c.Name == "verify"))
            {
                ulong count = ctx.Commands.Where(c => c.Name == "verify" && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(c => c.Name == "verify" && !c.Synchronized);

                VerifyVisible = true;
                VerifyText    = string.Format(UI.You_have_called_the_Verify_command_0_times, count);
            }

            CommandsVisible = FsInfoVisible        || ChecksumVisible  || CompareVisible || ConvertImageVisible ||
                              CreateSidecarVisible || DecodeVisible    || DeviceInfoVisible || DeviceReportVisible ||
                              DumpMediaVisible     || EntropyVisible   || FormatsCommandVisible || ImageInfoVisible ||
                              MediaInfoVisible     || MediaScanVisible || PrintHexVisible || VerifyVisible;
        }

        if(ctx.Filters.Any())
        {
            FiltersVisible = true;

            foreach(string nvs in ctx.Filters.Select(n => n.Name).Distinct())
            {
                ulong count = ctx.Filters.Where(c => c.Name == nvs && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Filters.LongCount(c => c.Name == nvs && !c.Synchronized);

                Filters.Add(new NameCountModel
                {
                    Name  = nvs,
                    Count = count
                });
            }
        }

        if(ctx.MediaFormats.Any())
        {
            FormatsVisible = true;

            foreach(string nvs in ctx.MediaFormats.Select(n => n.Name).Distinct())
            {
                ulong count = ctx.MediaFormats.Where(c => c.Name == nvs && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.MediaFormats.LongCount(c => c.Name == nvs && !c.Synchronized);

                Formats.Add(new NameCountModel
                {
                    Name  = nvs,
                    Count = count
                });
            }
        }

        if(ctx.Partitions.Any())
        {
            PartitionsVisible = true;

            foreach(string nvs in ctx.Partitions.Select(n => n.Name).Distinct())
            {
                ulong count = ctx.Partitions.Where(c => c.Name == nvs && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Partitions.LongCount(c => c.Name == nvs && !c.Synchronized);

                Partitions.Add(new NameCountModel
                {
                    Name  = nvs,
                    Count = count
                });
            }
        }

        if(ctx.Filesystems.Any())
        {
            FilesystemsVisible = true;

            foreach(string nvs in ctx.Filesystems.Select(n => n.Name).Distinct())
            {
                ulong count = ctx.Filesystems.Where(c => c.Name == nvs && c.Synchronized).Select(c => c.Count).
                                  FirstOrDefault();

                count += (ulong)ctx.Filesystems.LongCount(c => c.Name == nvs && !c.Synchronized);

                Filesystems.Add(new NameCountModel
                {
                    Name  = nvs,
                    Count = count
                });
            }
        }

        if(ctx.SeenDevices.Any())
        {
            DevicesVisible = true;

            foreach(DeviceStat ds in ctx.SeenDevices.OrderBy(n => n.Manufacturer).ThenBy(n => n.Manufacturer).
                                         ThenBy(n => n.Revision).ThenBy(n => n.Bus))
                Devices.Add(new DeviceStatsModel
                {
                    Model        = ds.Model,
                    Manufacturer = ds.Manufacturer,
                    Revision     = ds.Revision,
                    Bus          = ds.Bus
                });
        }

        if(!ctx.Medias.Any())
            return;

        MediasVisible = true;

        foreach(string media in ctx.Medias.OrderBy(ms => ms.Type).Select(ms => ms.Type).Distinct())
        {
            ulong count = ctx.Medias.Where(c => c.Type == media && c.Synchronized && c.Real).Select(c => c.Count).
                              FirstOrDefault();

            count += (ulong)ctx.Medias.LongCount(c => c.Type == media && !c.Synchronized && c.Real);

            if(count > 0)
                Medias.Add(new MediaStatsModel
                {
                    Name  = media,
                    Count = count,
                    Type  = UI.Media_found_type_real
                });

            count = ctx.Medias.Where(c => c.Type == media && c.Synchronized && !c.Real).Select(c => c.Count).
                        FirstOrDefault();

            count += (ulong)ctx.Medias.LongCount(c => c.Type == media && !c.Synchronized && !c.Real);

            if(count == 0)
                continue;

            Medias.Add(new MediaStatsModel
            {
                Name  = media,
                Count = count,
                Type  = UI.Media_found_type_image
            });
        }
    }

    public string FsInfoText
    {
        get => _fsinfoText;
        set => this.RaiseAndSetIfChanged(ref _fsinfoText, value);
    }

    public bool FsInfoVisible
    {
        get => _fsinfoVisible;
        set => this.RaiseAndSetIfChanged(ref _fsinfoVisible, value);
    }

    public string ChecksumText
    {
        get => _checksumText;
        set => this.RaiseAndSetIfChanged(ref _checksumText, value);
    }

    public bool ChecksumVisible
    {
        get => _checksumVisible;
        set => this.RaiseAndSetIfChanged(ref _checksumVisible, value);
    }

    public string CompareText
    {
        get => _compareText;
        set => this.RaiseAndSetIfChanged(ref _compareText, value);
    }

    public bool CompareVisible
    {
        get => _compareVisible;
        set => this.RaiseAndSetIfChanged(ref _compareVisible, value);
    }

    public string ConvertImageText
    {
        get => _convertImageText;
        set => this.RaiseAndSetIfChanged(ref _convertImageText, value);
    }

    public bool ConvertImageVisible
    {
        get => _convertImageVisible;
        set => this.RaiseAndSetIfChanged(ref _convertImageVisible, value);
    }

    public string CreateSidecarText
    {
        get => _createSidecarText;
        set => this.RaiseAndSetIfChanged(ref _createSidecarText, value);
    }

    public bool CreateSidecarVisible
    {
        get => _createSidecarVisible;
        set => this.RaiseAndSetIfChanged(ref _createSidecarVisible, value);
    }

    public string DecodeText
    {
        get => _decodeText;
        set => this.RaiseAndSetIfChanged(ref _decodeText, value);
    }

    public bool DecodeVisible
    {
        get => _decodeVisible;
        set => this.RaiseAndSetIfChanged(ref _decodeVisible, value);
    }

    public string DeviceInfoText
    {
        get => _deviceInfoText;
        set => this.RaiseAndSetIfChanged(ref _deviceInfoText, value);
    }

    public bool DeviceInfoVisible
    {
        get => _deviceInfoVisible;
        set => this.RaiseAndSetIfChanged(ref _deviceInfoVisible, value);
    }

    public string DeviceReportText
    {
        get => _deviceReportText;
        set => this.RaiseAndSetIfChanged(ref _deviceReportText, value);
    }

    public bool DeviceReportVisible
    {
        get => _deviceReportVisible;
        set => this.RaiseAndSetIfChanged(ref _deviceReportVisible, value);
    }

    public string DumpMediaText
    {
        get => _dumpMediaText;
        set => this.RaiseAndSetIfChanged(ref _dumpMediaText, value);
    }

    public bool DumpMediaVisible
    {
        get => _dumpMediaVisible;
        set => this.RaiseAndSetIfChanged(ref _dumpMediaVisible, value);
    }

    public string EntropyText
    {
        get => _entropyText;
        set => this.RaiseAndSetIfChanged(ref _entropyText, value);
    }

    public bool EntropyVisible
    {
        get => _entropyVisible;
        set => this.RaiseAndSetIfChanged(ref _entropyVisible, value);
    }

    public string FormatsCommandText
    {
        get => _formatsText;
        set => this.RaiseAndSetIfChanged(ref _formatsText, value);
    }

    public bool FormatsCommandVisible
    {
        get => _formatsCommandVisible;
        set => this.RaiseAndSetIfChanged(ref _formatsCommandVisible, value);
    }

    public string ImageInfoText
    {
        get => _imageInfoText;
        set => this.RaiseAndSetIfChanged(ref _imageInfoText, value);
    }

    public bool ImageInfoVisible
    {
        get => _imageInfoVisible;
        set => this.RaiseAndSetIfChanged(ref _imageInfoVisible, value);
    }

    public string MediaInfoText
    {
        get => _mediaInfoText;
        set => this.RaiseAndSetIfChanged(ref _mediaInfoText, value);
    }

    public bool MediaInfoVisible
    {
        get => _mediaInfoVisible;
        set => this.RaiseAndSetIfChanged(ref _mediaInfoVisible, value);
    }

    public string MediaScanText
    {
        get => _mediaScanText;
        set => this.RaiseAndSetIfChanged(ref _mediaScanText, value);
    }

    public bool MediaScanVisible
    {
        get => _mediaScanVisible;
        set => this.RaiseAndSetIfChanged(ref _mediaScanVisible, value);
    }

    public string PrintHexText
    {
        get => _printHexText;
        set => this.RaiseAndSetIfChanged(ref _printHexText, value);
    }

    public bool PrintHexVisible
    {
        get => _printHexVisible;
        set => this.RaiseAndSetIfChanged(ref _printHexVisible, value);
    }

    public string VerifyText
    {
        get => _verifyText;
        set => this.RaiseAndSetIfChanged(ref _verifyText, value);
    }

    public bool VerifyVisible
    {
        get => _verifyVisible;
        set => this.RaiseAndSetIfChanged(ref _verifyVisible, value);
    }

    public bool CommandsVisible
    {
        get => _commandsVisible;
        set => this.RaiseAndSetIfChanged(ref _commandsVisible, value);
    }

    public bool FiltersVisible
    {
        get => _filtersVisible;
        set => this.RaiseAndSetIfChanged(ref _filtersVisible, value);
    }

    public bool PartitionsVisible
    {
        get => _partitionsVisible;
        set => this.RaiseAndSetIfChanged(ref _partitionsVisible, value);
    }

    public bool FormatsVisible
    {
        get => _formatsVisible;
        set => this.RaiseAndSetIfChanged(ref _formatsVisible, value);
    }

    public bool FilesystemsVisible
    {
        get => _filesystemsVisible;
        set => this.RaiseAndSetIfChanged(ref _filesystemsVisible, value);
    }

    public bool DevicesVisible
    {
        get => _devicesVisible;
        set => this.RaiseAndSetIfChanged(ref _devicesVisible, value);
    }

    public bool MediasVisible
    {
        get => _mediasVisible;
        set => this.RaiseAndSetIfChanged(ref _mediasVisible, value);
    }

    [NotNull]
    public string CommandsLabel => UI.Title_Commands;
    [NotNull]
    public string FilterLabel => UI.Title_Filter;
    [NotNull]
    public string PartitionLabel => UI.Title_Partition;
    [NotNull]
    public string PartitionsLabel => UI.Title_Partitions;
    [NotNull]
    public string FiltersLabel => UI.Title_Filters;
    [NotNull]
    public string FormatsLabel => UI.Title_Formats;
    [NotNull]
    public string FormatLabel => UI.Title_Format;
    [NotNull]
    public string FilesystemsLabel => UI.Title_Filesystems;
    [NotNull]
    public string FilesystemLabel => UI.Title_Filesystem;
    [NotNull]
    public string TimesFoundLabel => UI.Title_Times_used;
    [NotNull]
    public string DevicesLabel => UI.Title_Devices;
    [NotNull]
    public string DeviceLabel => UI.Title_Device;
    [NotNull]
    public string ManufacturerLabel => UI.Title_Manufacturer;
    [NotNull]
    public string RevisionLabel => UI.Title_Revision;
    [NotNull]
    public string BusLabel => UI.Title_Bus;
    [NotNull]
    public string MediasLabel => UI.Title_Medias;
    [NotNull]
    public string MediaLabel => UI.Title_Media;
    [NotNull]
    public string TypeLabel => Localization.Core.Title_Type_for_media;
    [NotNull]
    public string Title => UI.Encodings;
    [NotNull]
    public string CloseLabel => UI.ButtonLabel_Close;

    public ReactiveCommand<Unit, Unit>            CloseCommand { get; }
    public ObservableCollection<NameCountModel>   Filters      { get; }
    public ObservableCollection<NameCountModel>   Formats      { get; }
    public ObservableCollection<NameCountModel>   Partitions   { get; }
    public ObservableCollection<NameCountModel>   Filesystems  { get; }
    public ObservableCollection<DeviceStatsModel> Devices      { get; }
    public ObservableCollection<MediaStatsModel>  Medias       { get; }

    void ExecuteCloseCommand() => _view.Close();
}