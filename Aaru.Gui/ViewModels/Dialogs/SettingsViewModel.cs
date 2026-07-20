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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Windows.Input;
using Aaru.Gui.Views.Dialogs;
using Aaru.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aaru.Gui.ViewModels.Dialogs;

public sealed partial class SettingsViewModel : ViewModelBase
{
    readonly SettingsDialog _view;
    [ObservableProperty]
    bool _commandStatsChecked;
    [ObservableProperty]
    bool _deviceStatsChecked;
    [ObservableProperty]
    bool _filesystemStatsChecked;
    [ObservableProperty]
    bool _filterStatsChecked;
    [ObservableProperty]
    bool _gdprVisible;
    [ObservableProperty]
    bool _mediaImageStatsChecked;
    [ObservableProperty]
    bool _mediaScanStatsChecked;
    [ObservableProperty]
    bool _mediaStatsChecked;
    [ObservableProperty]
    bool _partitionStatsChecked;
    [ObservableProperty]
    bool _saveReportsGloballyChecked;
    [ObservableProperty]
    bool _saveStatsChecked;
    [ObservableProperty]
    bool _shareCrashReportsChecked;
    [ObservableProperty]
    bool _shareReportsChecked;
    [ObservableProperty]
    bool _shareStatsChecked;
    [ObservableProperty]
    int _tabControlSelectedIndex;
    [ObservableProperty]
    bool _verifyStatsChecked;

    public SettingsViewModel(SettingsDialog view, bool gdprChange)
    {
        _view                      = view;
        GdprVisible                = gdprChange;
        SaveReportsGloballyChecked = Settings.Settings.Current.SaveReportsGlobally;
        ShareReportsChecked        = Settings.Settings.Current.ShareReports;
        ShareCrashReportsChecked   = Settings.Settings.Current.ShareCrashReports;

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

        CancelCommand = new RelayCommand(Cancel);
        SaveCommand   = new RelayCommand(Save);

        if(!_gdprVisible) _tabControlSelectedIndex = 1;
    }

    // TODO: Show Preferences in macOS
    public ICommand CancelCommand { get; }
    public ICommand SaveCommand   { get; }

    void Save()
    {
        Settings.Settings.Current.SaveReportsGlobally = SaveReportsGloballyChecked;
        Settings.Settings.Current.ShareReports        = ShareReportsChecked;
        Settings.Settings.Current.ShareCrashReports   = ShareCrashReportsChecked;

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

    void Cancel() => _view.Close();
}