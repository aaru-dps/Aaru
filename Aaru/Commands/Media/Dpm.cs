// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dpm.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'media dpm' command.
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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Core;
using Aaru.Core.Devices.Dpm;
using Aaru.Core.Logging;
using Aaru.Images;
using Aaru.Localization;
using Aaru.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using MediaType = Aaru.CommonTypes.MediaType;

namespace Aaru.Commands.Media;

sealed class MediaDpmCommand : Command<MediaDpmCommand.Settings>
{
    const  string       MODULE_NAME = "Media-DPM command";
    static ProgressTask _progressTask1;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        MainClass.PrintCopyright();

        Statistics.AddCommand("media-dpm");

        AaruLogging.Debug(MODULE_NAME, "--debug={0}",   settings.Debug);
        AaruLogging.Debug(MODULE_NAME, "--device={0}",  Markup.Escape(settings.DevicePath ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--image={0}",   Markup.Escape(settings.ImagePath  ?? ""));
        AaruLogging.Debug(MODULE_NAME, "--force={0}",   settings.Force);
        AaruLogging.Debug(MODULE_NAME, "--spacing={0}", settings.Spacing);
        AaruLogging.Debug(MODULE_NAME, "--speed={0}",   settings.Speed);
        AaruLogging.Debug(MODULE_NAME, "--start={0}",   settings.Start);
        AaruLogging.Debug(MODULE_NAME, "--end={0}",     settings.End);
        AaruLogging.Debug(MODULE_NAME, "--verbose={0}", settings.Verbose);

        // Open the image, which must be an AaruFormat image that can be resumed
        IFilter inputFilter = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_file_filter).IsIndeterminate();
            inputFilter = PluginRegister.Singleton.GetFilter(settings.ImagePath);
        });

        if(inputFilter == null)
        {
            AaruLogging.Error(UI.Cannot_open_specified_file);

            return (int)ErrorNumber.CannotOpenFile;
        }

        IBaseImage inputFormat = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_image_format).IsIndeterminate();
            inputFormat = ImageFormat.Detect(inputFilter);
        });

        if(inputFormat is not AaruFormat aif)
        {
            AaruLogging.Error(UI.File_is_not_an_AaruFormat_image);

            return (int)ErrorNumber.InvalidArgument;
        }

        ErrorNumber opened = ErrorNumber.NoData;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Invoke_Opening_image_file).IsIndeterminate();
            opened = aif.Open(inputFilter);
        });

        if(opened != ErrorNumber.NoError)
        {
            AaruLogging.Error(UI.Unable_to_open_image_format);
            AaruLogging.Error(Localization.Core.Error_0, opened);

            return (int)opened;
        }

        if(aif.Info.Version.StartsWith("1.", StringComparison.OrdinalIgnoreCase))
        {
            AaruLogging.Error(UI.AaruFormat_images_version_1_x_are_read_only);

            return (int)ErrorNumber.InvalidArgument;
        }

        if(aif.ReadDpm(out _) == ErrorNumber.NoError && !settings.Force)
        {
            AaruLogging.Error(UI.Image_already_has_DPM_use_force);

            return (int)ErrorNumber.InvalidArgument;
        }

        // Open the device, which must be a local optical drive, as network latency would ruin the timing
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

        if(dev is null)
        {
            AaruLogging.Error(string.Format(UI.Could_not_open_device_error_0, Error.Print(devErrno)));

            return (int)ErrorNumber.CannotOpenDevice;
        }

        if(dev.Error)
        {
            AaruLogging.Error(Error.Print(dev.LastError));

            return (int)ErrorNumber.CannotOpenDevice;
        }

        if(dev is Devices.Remote.Device)
        {
            AaruLogging.Error(UI.DPM_cannot_be_measured_on_remote_devices);
            dev.Close();

            return (int)ErrorNumber.NotSupported;
        }

        if(dev.ScsiType != PeripheralDeviceTypes.MultiMediaDevice)
        {
            AaruLogging.Error(UI.DPM_can_only_be_measured_on_optical_drives);
            dev.Close();

            return (int)ErrorNumber.NotSupported;
        }

        DeviceLog.StartLog(dev, false);
        Statistics.AddDevice(dev);

        var measurement = new DpmMeasurement(new DeviceDpmDrive(dev));
        measurement.ErrorMessage += static text => AaruLogging.Error(text);

        if(!measurement.Prepare(settings.Spacing))
        {
            dev.Close();

            return (int)ErrorNumber.NotSupported;
        }

        if(measurement.Kind == DpmMediumKind.Bd) AaruLogging.WriteLine(UI.DPM_on_Bluray_is_experimental);

        // Check the disc in the drive is the one in the image
        if(!MediumMatchesImage(dev, aif, measurement.LastLba) && !settings.Force)
        {
            AaruLogging.Error(UI.Medium_in_drive_does_not_match_image);
            dev.Close();

            return (int)ErrorNumber.InvalidArgument;
        }

        // Sectors the dump could not read are never read again
        string resumePath = ResumeSidecar.FindResumePath(settings.ImagePath, null);

        if(resumePath?.EndsWith(".json", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                using var fs = new FileStream(resumePath, FileMode.Open, FileAccess.Read);

                Resume resume =
                    (JsonSerializer.Deserialize(fs, typeof(ResumeJson), ResumeJsonContext.Default) as ResumeJson)
                  ?.Resume;

                if(resume?.BadBlocks is { Count: > 0 })
                {
                    measurement.SeedUnreadable(resume.BadBlocks);
                    AaruLogging.WriteLine(UI.Skipping_0_sectors_the_dump_could_not_read, resume.BadBlocks.Count);
                }
            }
            catch(Exception ex)
            {
                AaruLogging.Debug(MODULE_NAME, "Could not read resume file {0}: {1}", resumePath, ex.Message);
            }
        }

        uint end = settings.End ?? measurement.LastLba;
        DataPositionMeasurement? dpm = null;

        AnsiConsole.Progress()
                   .AutoClear(true)
                   .HideCompleted(true)
                   .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                   .Start(ctx =>
                    {
                        measurement.UpdateStatus += static text => AaruLogging.WriteLine(text);

                        measurement.InitProgress += () => _progressTask1 = ctx.AddTask(UI.Measuring_DPM);

                        measurement.UpdateProgress += (text, current, maximum) =>
                        {
                            _progressTask1             ??= ctx.AddTask(UI.Measuring_DPM);
                            _progressTask1.Description =   Markup.Escape(text);
                            _progressTask1.Value       =   current;
                            _progressTask1.MaxValue    =   maximum;
                        };

                        measurement.EndProgress += static () =>
                        {
                            _progressTask1?.StopTask();
                            _progressTask1 = null;
                        };

                        Console.CancelKeyPress += (_, e) =>
                        {
                            e.Cancel = true;
                            measurement.Abort();
                        };

                        dpm = measurement.Measure(settings.Start, end, settings.Speed);
                    });

        dev.Close();

        if(dpm is null) return (int)ErrorNumber.InOutError;

        // Resume the image to add the DPM
        ulong     sectors         = aif.Info.Sectors;
        MediaType mediaType       = aif.Info.MediaType;
        uint      negativeSectors = aif.Info.NegativeSectors;
        uint      overflowSectors = aif.Info.OverflowSectors;
        uint      sectorSize      = aif.Info.SectorSize;

        aif.Close();

        bool written = false;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Writing_DPM_to_image).IsIndeterminate();

            written = aif.Create(settings.ImagePath, mediaType, [], sectors, negativeSectors, overflowSectors,
                                 sectorSize) &&
                      aif.SetDpm(dpm.Value);

            written &= aif.Close();
        });

        if(!written)
        {
            AaruLogging.Error(UI.Error_reopening_image_for_writing);
            AaruLogging.Error(aif.ErrorMessage);

            return (int)ErrorNumber.WriteError;
        }

        string prefix = Path.Combine(Path.GetDirectoryName(settings.ImagePath) ?? "",
                                     Path.GetFileNameWithoutExtension(settings.ImagePath));

        DpmSidecar.Write(dpm.Value, prefix);

        AaruLogging.WriteLine(UI.Written_DPM_with_0_entries_to_image, dpm.Value.Entries.Length);

        return (int)ErrorNumber.NoError;
    }

    /// <summary>Checks the medium in the drive has the size of the image, and some of its sectors</summary>
    static bool MediumMatchesImage(Devices.Device dev, IMediaImage image, uint lastLba)
    {
        if(lastLba != uint.MaxValue && lastLba + 1UL != image.Info.Sectors)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Drive reports {0} sectors, image has {1}",
                              lastLba + 1UL,
                              image.Info.Sectors);

            return false;
        }

        ulong[] samples = [16, image.Info.Sectors / 2, image.Info.Sectors - 1];
        int     compared = 0;

        foreach(ulong sample in samples.Distinct())
        {
            if(sample >= image.Info.Sectors) continue;

            // Sectors the drive or the image can't give as 2048 bytes, like audio, are not compared
            if(dev.Read12(out byte[] fromDrive, out _, 0, false, false, false, false, (uint)sample, 2048, 0, 1, false,
                          dev.Timeout, out _) ||
               image.ReadSector(sample, false, out byte[] fromImage, out _) != ErrorNumber.NoError ||
               fromImage?.Length != 2048)
                continue;

            if(!fromDrive.AsSpan(0, 2048).SequenceEqual(fromImage))
            {
                AaruLogging.Debug(MODULE_NAME, "Sector {0} differs between the drive and the image", sample);

                return false;
            }

            compared++;
        }

        AaruLogging.Debug(MODULE_NAME, "{0} sectors match between the drive and the image", compared);

        return true;
    }

#region Nested type: Settings

    public class Settings : MediaFamily
    {
        [LocalizedDescription(nameof(UI.DPM_force_help))]
        [CommandOption("-f|--force")]
        [DefaultValue(false)]
        public bool Force { get; init; }
        [LocalizedDescription(nameof(UI.DPM_spacing_help))]
        [CommandOption("--spacing")]
        [DefaultValue(0u)]
        public uint Spacing { get; init; }
        [LocalizedDescription(nameof(UI.DPM_speed_help))]
        [CommandOption("--speed")]
        [DefaultValue((ushort)0)]
        public ushort Speed { get; init; }
        [LocalizedDescription(nameof(UI.DPM_start_help))]
        [CommandOption("--start")]
        [DefaultValue(0u)]
        public uint Start { get; init; }
        [LocalizedDescription(nameof(UI.DPM_end_help))]
        [CommandOption("--end")]
        [DefaultValue(null)]
        public uint? End { get; init; }
        [LocalizedDescription(nameof(UI.Device_path))]
        [CommandArgument(0, "<device-path>")]
        public string DevicePath { get; init; }
        [LocalizedDescription(nameof(UI.DPM_image_path_help))]
        [CommandArgument(1, "<image-path>")]
        public string ImagePath { get; init; }
    }

#endregion
}