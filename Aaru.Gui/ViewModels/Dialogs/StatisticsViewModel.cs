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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Aaru.Database;
using Aaru.Database.Models;
using Aaru.Gui.Models;
using Aaru.Gui.Views.Dialogs;
using Aaru.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NameCountModel = Aaru.Gui.Models.NameCountModel;

namespace Aaru.Gui.ViewModels.Dialogs;

public sealed partial class StatisticsViewModel : ViewModelBase
{
    readonly StatisticsDialog _view;
    [ObservableProperty]
    string _checksumText;
    [ObservableProperty]
    bool _checksumVisible;
    [ObservableProperty]
    bool _commandsVisible;
    [ObservableProperty]
    string _compareText;
    [ObservableProperty]
    bool _compareVisible;
    [ObservableProperty]
    string _convertImageText;
    [ObservableProperty]
    bool _convertImageVisible;
    [ObservableProperty]
    string _createSidecarText;
    [ObservableProperty]
    bool _createSidecarVisible;
    [ObservableProperty]
    string _decodeText;
    [ObservableProperty]
    bool _decodeVisible;
    [ObservableProperty]
    string _deviceInfoText;
    [ObservableProperty]
    bool _deviceInfoVisible;
    [ObservableProperty]
    string _deviceReportText;
    [ObservableProperty]
    bool _deviceReportVisible;
    [ObservableProperty]
    bool _devicesVisible;
    [ObservableProperty]
    string _dumpMediaText;
    [ObservableProperty]
    bool _dumpMediaVisible;
    [ObservableProperty]
    string _entropyText;
    [ObservableProperty]
    bool _entropyVisible;
    [ObservableProperty]
    bool _filesystemsVisible;
    [ObservableProperty]
    bool _filtersVisible;
    [ObservableProperty]
    string _formatsCommandText;
    [ObservableProperty]
    bool _formatsCommandVisible;
    [ObservableProperty]
    bool _formatsVisible;
    [ObservableProperty]
    string _fsInfoText;
    [ObservableProperty]
    bool _fsInfoVisible;
    [ObservableProperty]
    string _imageInfoText;
    [ObservableProperty]
    bool _imageInfoVisible;
    [ObservableProperty]
    string _mediaInfoText;
    [ObservableProperty]
    bool _mediaInfoVisible;
    [ObservableProperty]
    string _mediaScanText;
    [ObservableProperty]
    bool _mediaScanVisible;
    [ObservableProperty]
    bool _mediasVisible;
    [ObservableProperty]
    bool _partitionsVisible;
    [ObservableProperty]
    string _printHexText;
    [ObservableProperty]
    bool _printHexVisible;
    [ObservableProperty]
    string _verifyText;
    [ObservableProperty]
    bool _verifyVisible;

    public StatisticsViewModel(StatisticsDialog view)
    {
        _view        = view;
        Filters      = [];
        Formats      = [];
        Partitions   = [];
        Filesystems  = [];
        Devices      = [];
        Medias       = [];
        CloseCommand = new RelayCommand(Close);
        using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

        if(ctx.Commands.Any())
        {
            if(ctx.Commands.Any(static c => c.Name == "analyze"))
            {
                foreach(Command oldAnalyze in ctx.Commands.Where(static c => c.Name == "analyze"))
                {
                    oldAnalyze.Name = "fs-info";
                    ctx.Commands.Update(oldAnalyze);
                }

                ulong count = 0;

                foreach(Command fsInfo in ctx.Commands.Where(static c => c.Name == "fs-info" && c.Synchronized))
                {
                    count += fsInfo.Count;
                    ctx.Remove(fsInfo);
                }

                if(count > 0)
                {
                    ctx.Commands.Add(new Command
                    {
                        Count        = count,
                        Name         = "fs-info",
                        Synchronized = true
                    });
                }

                ctx.SaveChanges();
            }

            if(ctx.Commands.Any(static c => c.Name == "fs-info"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "fs-info" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "fs-info" && !c.Synchronized);

                FsInfoVisible = true;
                FsInfoText    = string.Format(UI.You_have_called_the_Filesystem_Info_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "checksum"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "checksum" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "checksum" && !c.Synchronized);

                ChecksumVisible = true;
                ChecksumText    = string.Format(UI.You_have_called_the_Checksum_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "compare"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "compare" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "compare" && !c.Synchronized);

                CompareVisible = true;
                CompareText    = string.Format(UI.You_have_called_the_Compare_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "convert-image"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "convert-image" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "convert-image" && !c.Synchronized);

                ConvertImageVisible = true;
                ConvertImageText    = string.Format(UI.You_have_called_the_Convert_Image_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "create-sidecar"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "create-sidecar" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "create-sidecar" && !c.Synchronized);

                CreateSidecarVisible = true;
                CreateSidecarText    = string.Format(UI.You_have_called_the_Create_Sidecar_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "decode"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "decode" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "decode" && !c.Synchronized);

                DecodeVisible = true;
                DecodeText    = string.Format(UI.You_have_called_the_Decode_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "device-info"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "device-info" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "device-info" && !c.Synchronized);

                DeviceInfoVisible = true;
                DeviceInfoText    = string.Format(UI.You_have_called_the_Device_Info_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "device-report"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "device-report" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "device-report" && !c.Synchronized);

                DeviceReportVisible = true;
                DeviceReportText    = string.Format(UI.You_have_called_the_Device_Report_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "dump-media"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "dump-media" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "dump-media" && !c.Synchronized);

                DumpMediaVisible = true;
                DumpMediaText    = string.Format(UI.You_have_called_the_Dump_Media_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "entropy"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "entropy" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "entropy" && !c.Synchronized);

                EntropyVisible = true;
                EntropyText    = string.Format(UI.You_have_called_the_Entropy_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "formats"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "formats" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "formats" && !c.Synchronized);

                FormatsCommandVisible = true;
                FormatsCommandText    = string.Format(UI.You_have_called_the_Formats_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "image-info"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "image-info" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "image-info" && !c.Synchronized);

                ImageInfoVisible = true;
                ImageInfoText    = string.Format(UI.You_have_called_the_Image_Info_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "media-info"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "media-info" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "media-info" && !c.Synchronized);

                MediaInfoVisible = true;
                MediaInfoText    = string.Format(UI.You_have_called_the_Media_Info_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "media-scan"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "media-scan" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "media-scan" && !c.Synchronized);

                MediaScanVisible = true;
                MediaScanText    = string.Format(UI.You_have_called_the_Media_Scan_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "printhex"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "printhex" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "printhex" && !c.Synchronized);

                PrintHexVisible = true;
                PrintHexText    = string.Format(UI.You_have_called_the_Print_Hex_command_0_times, count);
            }

            if(ctx.Commands.Any(static c => c.Name == "verify"))
            {
                ulong count = ctx.Commands.Where(static c => c.Name == "verify" && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

                count += (ulong)ctx.Commands.LongCount(static c => c.Name == "verify" && !c.Synchronized);

                VerifyVisible = true;
                VerifyText    = string.Format(UI.You_have_called_the_Verify_command_0_times, count);
            }

            CommandsVisible = FsInfoVisible         ||
                              ChecksumVisible       ||
                              CompareVisible        ||
                              ConvertImageVisible   ||
                              CreateSidecarVisible  ||
                              DecodeVisible         ||
                              DeviceInfoVisible     ||
                              DeviceReportVisible   ||
                              DumpMediaVisible      ||
                              EntropyVisible        ||
                              FormatsCommandVisible ||
                              ImageInfoVisible      ||
                              MediaInfoVisible      ||
                              MediaScanVisible      ||
                              PrintHexVisible       ||
                              VerifyVisible;
        }

        if(ctx.Filters.Any())
        {
            FiltersVisible = true;

            foreach(string nvs in ctx.Filters.Select(static n => n.Name).Distinct())
            {
                ulong count = ctx.Filters.Where(c => c.Name == nvs && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

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

            foreach(string nvs in ctx.MediaFormats.Select(static n => n.Name).Distinct())
            {
                ulong count = ctx.MediaFormats.Where(c => c.Name == nvs && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

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

            foreach(string nvs in ctx.Partitions.Select(static n => n.Name).Distinct())
            {
                ulong count = ctx.Partitions.Where(c => c.Name == nvs && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

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

            foreach(string nvs in ctx.Filesystems.Select(static n => n.Name).Distinct())
            {
                ulong count = ctx.Filesystems.Where(c => c.Name == nvs && c.Synchronized)
                                 .Select(static c => c.Count)
                                 .FirstOrDefault();

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

            foreach(DeviceStat ds in ctx.SeenDevices.OrderBy(static n => n.Manufacturer)
                                        .ThenBy(static n => n.Model)
                                        .ThenBy(static n => n.Revision)
                                        .ThenBy(static n => n.Bus))
            {
                Devices.Add(new DeviceStatsModel
                {
                    Model        = ds.Model,
                    Manufacturer = ds.Manufacturer,
                    Revision     = ds.Revision,
                    Bus          = ds.Bus
                });
            }
        }

        if(!ctx.Medias.Any()) return;

        MediasVisible = true;

        foreach(string media in ctx.Medias.OrderBy(static ms => ms.Type).Select(static ms => ms.Type).Distinct())
        {
            ulong count = ctx.Medias.Where(c => c.Type == media && c.Synchronized && c.Real)
                             .Select(static c => c.Count)
                             .FirstOrDefault();

            count += (ulong)ctx.Medias.LongCount(c => c.Type == media && !c.Synchronized && c.Real);

            if(count > 0)
            {
                Medias.Add(new MediaStatsModel
                {
                    Name  = media,
                    Count = count,
                    Type  = UI.Media_found_type_real
                });
            }

            count = ctx.Medias.Where(c => c.Type == media && c.Synchronized && !c.Real)
                       .Select(static c => c.Count)
                       .FirstOrDefault();

            count += (ulong)ctx.Medias.LongCount(c => c.Type == media && !c.Synchronized && !c.Real);

            if(count == 0) continue;

            Medias.Add(new MediaStatsModel
            {
                Name  = media,
                Count = count,
                Type  = UI.Media_found_type_image
            });
        }
    }

    public ICommand                               CloseCommand { get; }
    public ObservableCollection<NameCountModel>   Filters      { get; }
    public ObservableCollection<NameCountModel>   Formats      { get; }
    public ObservableCollection<NameCountModel>   Partitions   { get; }
    public ObservableCollection<NameCountModel>   Filesystems  { get; }
    public ObservableCollection<DeviceStatsModel> Devices      { get; }
    public ObservableCollection<MediaStatsModel>  Medias       { get; }

    void Close() => _view.Close();
}