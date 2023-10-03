// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SettingsViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the settings dialog.
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

using System.Reactive;
using Aaru.Gui.Views.Dialogs;
using Aaru.Localization;
using Aaru.Settings;
using JetBrains.Annotations;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Dialogs;

public sealed class SettingsViewModel : ViewModelBase
{
    readonly SettingsDialog _view;
    bool                    _commandStatsChecked;
    bool                    _deviceStatsChecked;
    bool                    _filesystemStatsChecked;
    bool                    _filterStatsChecked;
    bool                    _gdprVisible;
    bool                    _mediaImageStatsChecked;
    bool                    _mediaScanStatsChecked;
    bool                    _mediaStatsChecked;
    bool                    _partitionStatsChecked;
    bool                    _saveReportsGloballyChecked;
    bool                    _saveStatsChecked;
    bool                    _shareReportsChecked;
    bool                    _shareStatsChecked;
    int                     _tabControlSelectedIndex;
    bool                    _verifyStatsChecked;

    public SettingsViewModel(SettingsDialog view, bool gdprChange)
    {
        _view                      = view;
        GdprVisible                = gdprChange;
        SaveReportsGloballyChecked = Settings.Settings.Current.SaveReportsGlobally;
        ShareReportsChecked        = Settings.Settings.Current.ShareReports;

        if(Settings.Settings.Current.Stats != null)
        {
            SaveStatsChecked       = true;
            ShareStatsChecked      = Settings.Settings.Current.Stats.ShareStats;
            CommandStatsChecked    = Settings.Settings.Current.Stats.CommandStats;
            DeviceStatsChecked     = Settings.Settings.Current.Stats.DeviceStats;
            FilesystemStatsChecked = Settings.Settings.Current.Stats.FilesystemStats;
            FilterStatsChecked     = Settings.Settings.Current.Stats.FilterStats;
            MediaImageStatsChecked = Settings.Settings.Current.Stats.MediaImageStats;
            MediaScanStatsChecked  = Settings.Settings.Current.Stats.MediaScanStats;
            PartitionStatsChecked  = Settings.Settings.Current.Stats.PartitionStats;
            MediaStatsChecked      = Settings.Settings.Current.Stats.MediaStats;
            VerifyStatsChecked     = Settings.Settings.Current.Stats.VerifyStats;
        }
        else
            SaveStatsChecked = false;

        CancelCommand = ReactiveCommand.Create(ExecuteCancelCommand);
        SaveCommand   = ReactiveCommand.Create(ExecuteSaveCommand);

        if(!_gdprVisible)
            _tabControlSelectedIndex = 1;
    }

    // TODO: Show Preferences in macOS
    [NotNull]
    public string Title => UI.Title_Settings;

    [NotNull]
    public string GdprLabel => UI.Title_GDPR;

    [NotNull]
    public string ReportsLabel => UI.Title_Reports;

    [NotNull]
    public string StatisticsLabel => UI.Title_Statistics;

    [NotNull]
    public string SaveLabel => UI.ButtonLabel_Save;

    [NotNull]
    public string CancelLabel => UI.ButtonLabel_Cancel;

    [NotNull]
    public string GdprText1 => UI.GDPR_Compliance;

    [NotNull]
    public string GdprText2 => UI.GDPR_Open_Source_Disclaimer;

    [NotNull]
    public string GdprText3 => UI.GDPR_Information_sharing;

    [NotNull]
    public string ReportsGloballyText => UI.Configure_Device_Report_information_disclaimer;

    [NotNull]
    public string SaveReportsGloballyText => UI.Save_device_reports_in_shared_folder_of_your_computer_Q;

    [NotNull]
    public string ReportsText => UI.Configure_share_report_disclaimer;

    [NotNull]
    public string ShareReportsText => UI.Share_your_device_reports_with_us_Q;

    [NotNull]
    public string StatisticsText => UI.Statistics_disclaimer;

    [NotNull]
    public string SaveStatsText => UI.Save_stats_about_your_Aaru_usage_Q;

    [NotNull]
    public string ShareStatsText => UI.Share_your_stats_anonymously_Q;

    [NotNull]
    public string CommandStatsText => UI.Gather_statistics_about_command_usage_Q;

    [NotNull]
    public string DeviceStatsText => UI.Gather_statistics_about_found_devices_Q;

    [NotNull]
    public string FilesystemStatsText => UI.Gather_statistics_about_found_filesystems_Q;

    [NotNull]
    public string FilterStatsText => UI.Gather_statistics_about_found_file_filters_Q;

    [NotNull]
    public string MediaImageStatsText => UI.Gather_statistics_about_found_media_image_formats_Q;

    [NotNull]
    public string MediaScanStatsText => UI.Gather_statistics_about_scanned_media_Q;

    [NotNull]
    public string PartitionStatsText => UI.Gather_statistics_about_found_partitioning_schemes_Q;

    [NotNull]
    public string MediaStatsText => UI.Gather_statistics_about_media_types_Q;

    [NotNull]
    public string VerifyStatsText => UI.Gather_statistics_about_media_image_verifications_Q;

    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand   { get; }

    public bool GdprVisible
    {
        get => _gdprVisible;
        set => this.RaiseAndSetIfChanged(ref _gdprVisible, value);
    }

    public bool SaveReportsGloballyChecked
    {
        get => _saveReportsGloballyChecked;
        set => this.RaiseAndSetIfChanged(ref _saveReportsGloballyChecked, value);
    }

    public bool ShareReportsChecked
    {
        get => _shareReportsChecked;
        set => this.RaiseAndSetIfChanged(ref _shareReportsChecked, value);
    }

    public bool SaveStatsChecked
    {
        get => _saveStatsChecked;
        set => this.RaiseAndSetIfChanged(ref _saveStatsChecked, value);
    }

    public bool ShareStatsChecked
    {
        get => _shareStatsChecked;
        set => this.RaiseAndSetIfChanged(ref _shareStatsChecked, value);
    }

    public bool CommandStatsChecked
    {
        get => _commandStatsChecked;
        set => this.RaiseAndSetIfChanged(ref _commandStatsChecked, value);
    }

    public bool DeviceStatsChecked
    {
        get => _deviceStatsChecked;
        set => this.RaiseAndSetIfChanged(ref _deviceStatsChecked, value);
    }

    public bool FilesystemStatsChecked
    {
        get => _filesystemStatsChecked;
        set => this.RaiseAndSetIfChanged(ref _filesystemStatsChecked, value);
    }

    public bool FilterStatsChecked
    {
        get => _filterStatsChecked;
        set => this.RaiseAndSetIfChanged(ref _filterStatsChecked, value);
    }

    public bool MediaImageStatsChecked
    {
        get => _mediaImageStatsChecked;
        set => this.RaiseAndSetIfChanged(ref _mediaImageStatsChecked, value);
    }

    public bool MediaScanStatsChecked
    {
        get => _mediaScanStatsChecked;
        set => this.RaiseAndSetIfChanged(ref _mediaScanStatsChecked, value);
    }

    public bool PartitionStatsChecked
    {
        get => _partitionStatsChecked;
        set => this.RaiseAndSetIfChanged(ref _partitionStatsChecked, value);
    }

    public bool MediaStatsChecked
    {
        get => _mediaStatsChecked;
        set => this.RaiseAndSetIfChanged(ref _mediaStatsChecked, value);
    }

    public bool VerifyStatsChecked
    {
        get => _verifyStatsChecked;
        set => this.RaiseAndSetIfChanged(ref _verifyStatsChecked, value);
    }

    public int TabControlSelectedIndex
    {
        get => _tabControlSelectedIndex;
        set => this.RaiseAndSetIfChanged(ref _tabControlSelectedIndex, value);
    }

    void ExecuteSaveCommand()
    {
        Settings.Settings.Current.SaveReportsGlobally = SaveReportsGloballyChecked;
        Settings.Settings.Current.ShareReports        = ShareReportsChecked;

        if(SaveStatsChecked)
        {
            Settings.Settings.Current.Stats = new StatsSettings
            {
                ShareStats      = ShareStatsChecked,
                CommandStats    = CommandStatsChecked,
                DeviceStats     = DeviceStatsChecked,
                FilesystemStats = FilesystemStatsChecked,
                FilterStats     = FilterStatsChecked,
                MediaImageStats = MediaImageStatsChecked,
                MediaScanStats  = MediaScanStatsChecked,
                PartitionStats  = PartitionStatsChecked,
                MediaStats      = MediaStatsChecked,
                VerifyStats     = VerifyStatsChecked
            };
        }
        else
            Settings.Settings.Current.Stats = null;

        Settings.Settings.Current.GdprCompliance = DicSettings.GDPR_LEVEL;
        Settings.Settings.SaveSettings();
        _view.Close();
    }

    void ExecuteCancelCommand() => _view.Close();
}