// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Scan.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'scan' command.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.ComponentModel;
using System.Threading;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.Core;
using Aaru.Core.Devices.Scanning;
using Aaru.Core.Logging;
using Aaru.Localization;
using Aaru.Logging;
using Humanizer;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Aaru.Commands.Media;

sealed class MediaScanCommand : Command<MediaScanCommand.Settings>
{
    const  string       MODULE_NAME = "Media-Scan command";
    static ProgressTask _progressTask1;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        MainClass.PrintCopyright();

        Statistics.AddCommand("media-scan");

        AaruLogging.Debug(MODULE_NAME, "--debug={0}",              settings.Debug);
        AaruLogging.Debug(MODULE_NAME, "--device={0}",             Markup.Escape(settings.DevicePath ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--ibg-log={0}",            Markup.Escape(settings.IbgLog     ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--mhdd-log={0}",           Markup.Escape(settings.MhddLog    ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--verbose={0}",            settings.Verbose);
        AaruLogging.Debug(MODULE_NAME, "--use-buffered-reads={0}", settings.UseBufferedReads);

        string devicePath = settings.DevicePath;

        if(devicePath.Length == 2 && devicePath[1] == ':' && devicePath[0] != '/' && char.IsLetter(devicePath[0]))
            devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

        Devices.Device dev      = null;
        ErrorNumber    devErrno = ErrorNumber.NoError;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Opening_device).IsIndeterminate();
            dev = Devices.Device.Create(devicePath, out devErrno);
        });

        switch(dev)
        {
            case null:
                AaruLogging.Error(string.Format(UI.Could_not_open_device_error_0, devErrno));

                return (int)devErrno;
            case Devices.Remote.Device remoteDev:
                Statistics.AddRemote(remoteDev.RemoteApplication,
                                     remoteDev.RemoteVersion,
                                     remoteDev.RemoteOperatingSystem,
                                     remoteDev.RemoteOperatingSystemVersion,
                                     remoteDev.RemoteArchitecture);

                break;
        }

        if(dev.Error)
        {
            AaruLogging.Error(Error.Print(dev.LastError));

            return (int)ErrorNumber.CannotOpenDevice;
        }

        DeviceLog.StartLog(dev, false);

        Statistics.AddDevice(dev);

        var scanner = new MediaScan(settings.MhddLog, settings.IbgLog, devicePath, dev, settings.UseBufferedReads);
        ScanResults results = new();

        AnsiConsole.Progress()
                   .AutoClear(true)
                   .HideCompleted(true)
                   .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                   .Start(ctx =>
                    {
                        scanner.UpdateStatus += static text => { AaruLogging.WriteLine(text); };

                        scanner.StoppingErrorMessage += static text => { AaruLogging.Error(text); };

                        scanner.UpdateProgress += (text, current, maximum) =>
                        {
                            _progressTask1             ??= ctx.AddTask("Progress");
                            _progressTask1.Description =   text;
                            _progressTask1.Value       =   current;
                            _progressTask1.MaxValue    =   maximum;
                        };

                        scanner.PulseProgress += text =>
                        {
                            if(_progressTask1 is null)
                                ctx.AddTask(text).IsIndeterminate();
                            else
                            {
                                _progressTask1.Description     = text;
                                _progressTask1.IsIndeterminate = true;
                            }
                        };

                        scanner.InitProgress += () => { _progressTask1 = ctx.AddTask("Progress"); };

                        scanner.EndProgress += static () =>
                        {
                            _progressTask1?.StopTask();
                            _progressTask1 = null;
                        };

                        Console.CancelKeyPress += (_, e) =>
                        {
                            e.Cancel = true;
                            scanner.Abort();
                        };

                        results = scanner.Scan();
                    });

        if(scanner.Aborted) AaruLogging.WriteLine(Localization.Core.Scan_aborted_partial_results);

        AaruLogging.WriteLine(Localization.Core.Took_a_total_of_0_1_processing_commands,
                              results.TotalTime.Seconds().Humanize(minUnit: TimeUnit.Second),
                              results.ProcessingTime.Seconds().Humanize(minUnit: TimeUnit.Second));

        AaruLogging.WriteLine(Localization.Core.Average_speed_0,
                              ByteSize.FromMegabytes(results.AvgSpeed).Per(1.Seconds()).Humanize());

        AaruLogging.WriteLine(Localization.Core.Fastest_speed_burst_0,
                              ByteSize.FromMegabytes(results.MaxSpeed).Per(1.Seconds()).Humanize());

        AaruLogging.WriteLine(Localization.Core.Slowest_speed_burst_0,
                              ByteSize.FromMegabytes(results.MinSpeed).Per(1.Seconds()).Humanize());

        AaruLogging.WriteLine();
        AaruLogging.WriteLine(Localization.Core.Summary);
        AaruLogging.WriteLine(Localization.Core._0_sectors_took_less_than_3_ms, results.A);

        AaruLogging.WriteLine(Localization.Core._0_sectors_took_less_than_10_ms_but_more_than_3_ms, results.B);

        AaruLogging.WriteLine(Localization.Core._0_sectors_took_less_than_50_ms_but_more_than_10_ms, results.C);

        AaruLogging.WriteLine(Localization.Core._0_sectors_took_less_than_150_ms_but_more_than_50_ms, results.D);

        AaruLogging.WriteLine(Localization.Core._0_sectors_took_less_than_500_ms_but_more_than_150_ms, results.E);

        AaruLogging.WriteLine(Localization.Core._0_sectors_took_more_than_500_ms, results.F);

        AaruLogging.WriteLine(Localization.Core._0_sectors_could_not_be_read, results.UnreadableSectors.Count);

        foreach(ulong bad in results.UnreadableSectors)
            AaruLogging.WriteLine(Localization.Core.Sector_0_could_not_be_read, bad);

        AaruLogging.WriteLine();

        if(results.SeekTotal > 0 || results.SeekMin < double.MaxValue || results.SeekMax > double.MinValue)

        {
            AaruLogging.WriteLine(Localization.Core
                                              .Testing_0_seeks_longest_seek_took_1_ms_fastest_one_took_2_ms_3_ms_average,
                                  results.SeekTimes,
                                  results.SeekMax,
                                  results.SeekMin,
                                  results.SeekTotal / 1000);
        }

        dev.Close();

        return (int)ErrorNumber.NoError;
    }

#region Nested type: Settings

    public class Settings : MediaFamily
    {
        [LocalizedDescription(nameof(UI.Write_a_log_of_the_scan_in_the_format_used_by_MHDD))]
        [DefaultValue(null)]
        [CommandOption("-m|--mhdd-log")]
        public string MhddLog { get; init; }
        [LocalizedDescription(nameof(UI.Write_a_log_of_the_scan_in_the_format_used_by_ImgBurn))]
        [DefaultValue(null)]
        [CommandOption("-b|--ibg-log")]
        public string IbgLog { get; init; }
        [LocalizedDescription(nameof(UI.OS_buffered_reads_help))]
        [DefaultValue(true)]
        [CommandOption("--use-buffered-reads")]
        public bool UseBufferedReads { get; init; }
        [LocalizedDescription(nameof(UI.Device_path))]
        [CommandArgument(0, "<device-path>")]
        public string DevicePath { get; init; }
    }

#endregion
}