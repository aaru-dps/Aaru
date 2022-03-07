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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Gui.ViewModels.Dialogs;

using System.Reactive;
using Aaru.Gui.Views.Dialogs;
using Aaru.Settings;
using JetBrains.Annotations;
using ReactiveUI;

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
        SaveReportsGloballyChecked = Settings.Current.SaveReportsGlobally;
        ShareReportsChecked        = Settings.Current.ShareReports;

        if(Settings.Current.Stats != null)
        {
            SaveStatsChecked       = true;
            ShareStatsChecked      = Settings.Current.Stats.ShareStats;
            CommandStatsChecked    = Settings.Current.Stats.CommandStats;
            DeviceStatsChecked     = Settings.Current.Stats.DeviceStats;
            FilesystemStatsChecked = Settings.Current.Stats.FilesystemStats;
            FilterStatsChecked     = Settings.Current.Stats.FilterStats;
            MediaImageStatsChecked = Settings.Current.Stats.MediaImageStats;
            MediaScanStatsChecked  = Settings.Current.Stats.MediaScanStats;
            PartitionStatsChecked  = Settings.Current.Stats.PartitionStats;
            MediaStatsChecked      = Settings.Current.Stats.MediaStats;
            VerifyStatsChecked     = Settings.Current.Stats.VerifyStats;
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
    public string Title => "Settings";
    [NotNull]
    public string GdprLabel => "GDPR";
    [NotNull]
    public string ReportsLabel => "Reports";
    [NotNull]
    public string StatisticsLabel => "Statistics";
    [NotNull]
    public string SaveLabel => "Save";
    [NotNull]
    public string CancelLabel => "Cancel";
    [NotNull]
    public string GdprText1 =>
        @"In compliance with the European Union General Data Protection Regulation 2016/679 (GDPR),
we must give you the following information about Aaru and ask if you want to opt-in
in some information sharing.";

    [NotNull]
    public string GdprText2 => @"Disclaimer: Because Aaru is an open source software this information, and therefore,
compliance with GDPR only holds true if you obtained a certificated copy from its original
authors. In case of doubt, close Aaru now and ask in our IRC support channel.";

    [NotNull]
    public string GdprText3 =>
        @"For any information sharing your IP address may be stored in our server, in a way that is not
possible for any person, manual, or automated process, to link with your identity, unless
specified otherwise.";
    [NotNull]
    public string ReportsGloballyText =>
        @"With the 'device-report' command, Aaru creates a report of a device, that includes its
manufacturer, model, firmware revision and/or version, attached bus, size, and supported commands.
The serial number of the device is not stored in the report. If used with the debug parameter,
extra information about the device will be stored in the report. This information is known to contain
the device serial number in non-standard places that prevent the automatic removal of it on a handful
of devices. A human-readable copy of the report in XML format is always created in the same directory
where Aaru is being run from.";

    [NotNull]
    public string SaveReportsGloballyText => "Save device reports in shared folder of your computer?";

    [NotNull]
    public string ReportsText =>
        @"Sharing a report with us will send it to our server, that's in the european union territory, where it
will be manually analyzed by an european union citizen to remove any trace of personal identification
from it. Once that is done, it will be shared in our stats website, https://www.aaru.app
These report will be used to improve Aaru support, and in some cases, to provide emulation of the
devices to other open-source projects. In any case, no information linking the report to you will be stored.";

    [NotNull]
    public string ShareReportsText => "Share your device reports with us?";
    [NotNull]
    public string StatisticsText =>
        @"Aaru can store some usage statistics. These statistics are limited to the number of times a
command is executed, a filesystem, partition, or device is used, the operating system version, and other.
In no case, any information besides pure statistical usage numbers is stored, and they're just joint to the
pool with no way of using them to identify you.";

    [NotNull]
    public string SaveStatsText => "Save stats about your Aaru usage?";
    [NotNull]
    public string ShareStatsText => "Share your stats (anonymously)?";
    [NotNull]
    public string CommandStatsText => "Gather statistics about command usage?";
    [NotNull]
    public string DeviceStatsText => "Gather statistics about found devices?";
    [NotNull]
    public string FilesystemStatsText => "Gather statistics about found filesystems?";
    [NotNull]
    public string FilterStatsText => "Gather statistics about found file filters?";
    [NotNull]
    public string MediaImageStatsText => "Gather statistics about found media image formats?";
    [NotNull]
    public string MediaScanStatsText => "Gather statistics about scanned media?";
    [NotNull]
    public string PartitionStatsText => "Gather statistics about found partitioning schemes?";
    [NotNull]
    public string MediaStatsText => "Gather statistics about media types?";
    [NotNull]
    public string VerifyStatsText => "Gather statistics about media image verifications?";

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
        Settings.Current.SaveReportsGlobally = SaveReportsGloballyChecked;
        Settings.Current.ShareReports        = ShareReportsChecked;

        if(SaveStatsChecked)
            Settings.Current.Stats = new StatsSettings
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
        else
            Settings.Current.Stats = null;

        Settings.Current.GdprCompliance = DicSettings.GDPR_LEVEL;
        Settings.SaveSettings();
        _view.Close();
    }

    void ExecuteCancelCommand() => _view.Close();
}