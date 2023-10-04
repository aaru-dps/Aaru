// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SplashWindowViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the splash window.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aaru.Console;
using Aaru.Core;
using Aaru.Database;
using Aaru.Gui.ViewModels.Dialogs;
using Aaru.Gui.Views.Dialogs;
using Aaru.Gui.Views.Windows;
using Aaru.Localization;
using Aaru.Settings;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Windows;

public sealed class SplashWindowViewModel(SplashWindow view) : ViewModelBase
{
    double _currentProgress;
    double _maxProgress;
    string _message;

    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public double MaxProgress
    {
        get => _maxProgress;
        set => this.RaiseAndSetIfChanged(ref _maxProgress, value);
    }

    public double CurrentProgress
    {
        get => _currentProgress;
        set => this.RaiseAndSetIfChanged(ref _currentProgress, value);
    }

    internal void OnOpened()
    {
        Message         = UI.Welcome_to_Aaru;
        MaxProgress     = 9;
        CurrentProgress = 0;

        Dispatcher.UIThread.Post(InitializeConsole);
    }

    void InitializeConsole()
    {
        CurrentProgress++;
        Message = UI.Initializing_console;

        Task.Run(() =>
        {
            ConsoleHandler.Init();
            AaruConsole.WriteLine(UI.Aaru_started);

            Dispatcher.UIThread.Post(LoadSettings);
        });
    }

    void LoadSettings()
    {
        CurrentProgress++;
        Message = UI.Loading_settings;
        AaruConsole.WriteLine(UI.Loading_settings);

        Task.Run(() =>
        {
            // TODO: Detect there are no settings yet
            Settings.Settings.LoadSettings();

            Dispatcher.UIThread.Post(MigrateLocalDatabase);
        });
    }

    void MigrateLocalDatabase()
    {
        CurrentProgress++;
        Message = UI.Migrating_local_database;
        AaruConsole.WriteLine(UI.Migrating_local_database);

        Task.Run(() =>
        {
            AaruContext ctx = null;

            try
            {
                ctx = AaruContext.Create(Settings.Settings.LocalDbPath, false);
                ctx.Database.Migrate();
            }
            catch(NotSupportedException)
            {
                try
                {
                    ctx?.Database.CloseConnection();
                    ctx?.Dispose();
                }
                catch(Exception)
                {
                    // Should not ever arrive here, but if it does, keep trying to replace it anyway
                }

                File.Delete(Settings.Settings.LocalDbPath);
                ctx = AaruContext.Create(Settings.Settings.LocalDbPath);
                ctx.Database.EnsureCreated();

                ctx.Database.
                    ExecuteSqlRaw(
                        "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (\"MigrationId\" TEXT PRIMARY KEY, \"ProductVersion\" TEXT)");

                foreach(string migration in ctx.Database.GetPendingMigrations())
                {
                    ctx.Database.
                        ExecuteSqlRaw($"INSERT INTO \"__EFMigrationsHistory\" (MigrationId, ProductVersion) VALUES ('{
                            migration}', '0.0.0')");
                }

                ctx.SaveChanges();
            }

            // Remove duplicates
            foreach(var duplicate in ctx.SeenDevices.AsEnumerable().GroupBy(a => new
                    {
                        a.Manufacturer,
                        a.Model,
                        a.Revision,
                        a.Bus
                    }).Where(a => a.Count() > 1).Distinct().Select(a => a.Key))
            {
                ctx.RemoveRange(ctx.SeenDevices.
                                    Where(d => d.Manufacturer == duplicate.Manufacturer && d.Model == duplicate.Model &&
                                               d.Revision     == duplicate.Revision && d.Bus == duplicate.Bus).Skip(1));
            }

            // Remove nulls
            ctx.RemoveRange(ctx.SeenDevices.Where(d => d.Manufacturer == null && d.Model == null &&
                                                       d.Revision     == null));

            ctx.SaveChanges();

            Dispatcher.UIThread.Post(UpdateMainDatabase);
        });
    }

    void UpdateMainDatabase()
    {
        CurrentProgress++;
        Message = UI.Updating_main_database;
        AaruConsole.WriteLine(UI.Updating_main_database);

        Task.Run(() =>
        {
            bool mainDbUpdate = !File.Exists(Settings.Settings.MainDbPath);

            // TODO: Update database

            var mainContext = AaruContext.Create(Settings.Settings.MainDbPath, false);

            if(mainContext.Database.GetPendingMigrations().Any())
            {
                AaruConsole.WriteLine(UI.New_database_version_updating);

                try
                {
                    File.Delete(Settings.Settings.MainDbPath);
                }
                catch(Exception)
                {
                    AaruConsole.ErrorWriteLine(UI.Exception_trying_to_remove_old_database_version);

                    AaruConsole.ErrorWriteLine(UI.Please_manually_remove_file_at_0, Settings.Settings.MainDbPath);

                    return;
                }

                // TODO: Update database
            }

            Dispatcher.UIThread.Post(CheckGdprCompliance);
        });
    }

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    async void CheckGdprCompliance()
    {
        CurrentProgress++;
        Message = UI.Checking_GDPR_compliance;
        AaruConsole.WriteLine(UI.Checking_GDPR_compliance);

        if(Settings.Settings.Current.GdprCompliance < DicSettings.GDPR_LEVEL)
        {
            var settingsDialog          = new SettingsDialog();
            var settingsDialogViewModel = new SettingsViewModel(settingsDialog, true);
            settingsDialog.DataContext = settingsDialogViewModel;
            await settingsDialog.ShowDialog(view);
        }

        LoadStatistics();
    }

    void LoadStatistics()
    {
        CurrentProgress++;
        Message = UI.Loading_statistics;
        AaruConsole.WriteLine(UI.Loading_statistics);

        Task.Run(() =>
        {
            Statistics.LoadStats();

            Dispatcher.UIThread.Post(RegisterEncodings);
        });
    }

    void RegisterEncodings()
    {
        CurrentProgress++;
        Message = UI.Registering_encodings;
        AaruConsole.WriteLine(UI.Registering_encodings);

        Task.Run(() =>
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Dispatcher.UIThread.Post(SaveStatistics);
        });
    }

    void SaveStatistics()
    {
        CurrentProgress++;
        Message = UI.Saving_statistics;
        AaruConsole.WriteLine(UI.Saving_statistics);

        Task.Run(() =>
        {
            Statistics.SaveStats();

            Dispatcher.UIThread.Post(LoadMainWindow);
        });
    }

    void LoadMainWindow()
    {
        CurrentProgress++;
        Message = UI.Loading_main_window;
        AaruConsole.WriteLine(UI.Loading_main_window);
        WorkFinished?.Invoke(this, EventArgs.Empty);
    }

    internal event EventHandler WorkFinished;
}