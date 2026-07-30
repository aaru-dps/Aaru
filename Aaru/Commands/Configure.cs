// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Configure.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'configure' command.
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

using System.Threading;
using Aaru.CommonTypes.Enums;
using Aaru.Localization;
using Aaru.Logging;
using Aaru.Settings;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Aaru.Commands;

sealed class ConfigureCommand : Command<ConfigureCommand.Settings>
{
    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        MainClass.PrintCopyright();

        return DoConfigure(false);
    }

    internal int DoConfigure(bool gdprChange)
    {
        if(gdprChange)
        {
            AaruLogging.WriteLine(UI.GDPR_Compliance);

            AaruLogging.WriteLine();

            AaruLogging.WriteLine(UI.GDPR_Open_Source_Disclaimer);

            AaruLogging.WriteLine();

            AaruLogging.WriteLine(UI.GDPR_Information_sharing);
        }

        AaruLogging.WriteLine();

        AaruLogging.WriteLine(UI.Configure_enable_decryption_disclaimer);

        Aaru.Settings.Settings.Current.EnableDecryption =
            AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_enable_decryption_of_copy_protected_media_Q}[/]");

#region Device reports

        AaruLogging.WriteLine();

        AaruLogging.WriteLine(UI.Configure_Device_Report_information_disclaimer);

        Aaru.Settings.Settings.Current.SaveReportsGlobally = AnsiConsole.Confirm($"[italic]{UI.
            Configure_Do_you_want_to_save_device_reports_in_shared_folder_of_your_computer_Q}[/]");

        AaruLogging.WriteLine();

        AaruLogging.WriteLine(UI.Configure_share_report_disclaimer);

        Aaru.Settings.Settings.Current.ShareReports =
            AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_share_your_device_reports_with_us_Q}[/]");

#endregion Device reports

#region Crash reports

        AskCrashReportConsent();

#endregion Crash reports

#region Statistics

        AaruLogging.WriteLine();

        AaruLogging.WriteLine(UI.Statistics_disclaimer);

        if(AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_save_stats_about_your_Aaru_usage_Q}[/]"))
        {
            Aaru.Settings.Settings.Current.Stats = new StatsSettings
            {
                ShareStats = AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_share_your_stats__anonymously_Q}[/]"),
                CommandStats =
                    AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_gather_statistics_about_command_usage_Q}[/]"),
                DeviceStats =
                    AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_gather_statistics_about_found_devices_Q}[/]"),
                FilesystemStats =
                    AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_gather_statistics_about_found_filesystems_Q}[/]"),
                FilterStats =
                    AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_gather_statistics_about_found_file_filters_Q}[/]"),
                MediaImageStats =
                    AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_gather_statistics_about_found_media_image_formats_Q
                    }[/]"),
                MediaScanStats =
                    AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_gather_statistics_about_scanned_media_Q}[/]"),
                PartitionStats = AnsiConsole.Confirm($"[italic]{UI.
                    Do_you_want_to_gather_statistics_about_found_partitioning_schemes_Q}[/]"),
                MediaStats =
                    AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_gather_statistics_about_media_types_Q}[/]"),
                VerifyStats =
                    AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_gather_statistics_about_media_image_verifications_Q
                    }[/]")
            };
        }
        else
            Aaru.Settings.Settings.Current.Stats = null;

#endregion Statistics

        Aaru.Settings.Settings.Current.GdprCompliance = DicSettings.GDPR_LEVEL;
        Aaru.Settings.Settings.SaveSettings();

        return (int)ErrorNumber.NoError;
    }

    /// <summary>Asks the user whether to share crash reports and records that the question has now been asked.</summary>
    /// <remarks>
    ///     Shared by the full <see cref="DoConfigure" /> wizard and by the one-off consent prompt at startup, so the
    ///     wording and the flag live in a single place. The caller is responsible for persisting the settings.
    /// </remarks>
    internal void AskCrashReportConsent()
    {
        AaruLogging.WriteLine();

        AaruLogging.WriteLine(UI.Configure_crash_report_disclaimer);

        Aaru.Settings.Settings.Current.ShareCrashReports =
            AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_share_crash_reports_with_us_Q}[/]");

        Aaru.Settings.Settings.Current.HasConsentBeenAsked = true;
    }

#region Nested type: Settings

    public class Settings : BaseSettings {}

#endregion
}