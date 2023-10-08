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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Localization;
using Aaru.Settings;
using Spectre.Console;

namespace Aaru.Commands;

sealed class ConfigureCommand : Command
{
    public ConfigureCommand() : base("configure", UI.Configure_Command_Description) =>
        Handler = CommandHandler.Create((Func<bool, bool, int>)Invoke);

    int Invoke(bool debug, bool verbose)
    {
        MainClass.PrintCopyright();

        if(debug)
        {
            IAnsiConsole stderrConsole = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(System.Console.Error)
            });

            AaruConsole.DebugWriteLineEvent += (format, objects) =>
            {
                if(objects is null)
                    stderrConsole.MarkupLine(format);
                else
                    stderrConsole.MarkupLine(format, objects);
            };

            AaruConsole.WriteExceptionEvent += ex => { stderrConsole.WriteException(ex); };
        }

        if(verbose)
        {
            AaruConsole.WriteEvent += (format, objects) =>
            {
                if(objects is null)
                    AnsiConsole.Markup(format);
                else
                    AnsiConsole.Markup(format, objects);
            };
        }

        return DoConfigure(false);
    }

    internal int DoConfigure(bool gdprChange)
    {
        if(gdprChange)
        {
            AaruConsole.WriteLine(UI.GDPR_Compliance);

            AaruConsole.WriteLine();

            AaruConsole.WriteLine(UI.GDPR_Open_Source_Disclaimer);

            AaruConsole.WriteLine();

            AaruConsole.WriteLine(UI.GDPR_Information_sharing);
        }

        AaruConsole.WriteLine();

        AaruConsole.WriteLine(UI.Configure_enable_decryption_disclaimer);

        Settings.Settings.Current.EnableDecryption =
            AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_enable_decryption_of_copy_protected_media_Q}[/]");

    #region Device reports

        AaruConsole.WriteLine();

        AaruConsole.WriteLine(UI.Configure_Device_Report_information_disclaimer);

        Settings.Settings.Current.SaveReportsGlobally = AnsiConsole.Confirm($"[italic]{UI.
            Configure_Do_you_want_to_save_device_reports_in_shared_folder_of_your_computer_Q}[/]");

        AaruConsole.WriteLine();

        AaruConsole.WriteLine(UI.Configure_share_report_disclaimer);

        Settings.Settings.Current.ShareReports =
            AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_share_your_device_reports_with_us_Q}[/]");

    #endregion Device reports

    #region Statistics

        AaruConsole.WriteLine();

        AaruConsole.WriteLine(UI.Statistics_disclaimer);

        if(AnsiConsole.Confirm($"[italic]{UI.Do_you_want_to_save_stats_about_your_Aaru_usage_Q}[/]"))
        {
            Settings.Settings.Current.Stats = new StatsSettings
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
            Settings.Settings.Current.Stats = null;

    #endregion Statistics

        Settings.Settings.Current.GdprCompliance = DicSettings.GDPR_LEVEL;
        Settings.Settings.SaveSettings();

        return (int)ErrorNumber.NoError;
    }
}