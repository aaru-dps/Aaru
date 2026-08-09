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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aaru.Core;
using Aaru.Database;
using Aaru.Gui.ViewModels.Dialogs;
using Aaru.Gui.Views.Dialogs;
using Aaru.Gui.Views.Windows;
using Aaru.Localization;
using Aaru.Logging;
using Aaru.Settings;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Sentry;

namespace Aaru.Gui.ViewModels.Windows;

public sealed partial class SplashWindowViewModel(SplashWindow view) : ViewModelBase
{
    [ObservableProperty]
    double _currentProgress;
    [ObservableProperty]
    double _maxProgress;
    [ObservableProperty]
    string _message;

    internal void OnOpened()
    {
        Message         = UI.Welcome_to_Aaru;
        MaxProgress     = 10;
        CurrentProgress = 0;

        Dispatcher.UIThread.Post(InitializeConsole);
    }

    void RunStage(Action work, Action next)
    {
        _ = Task.Run(() =>
        {
            try
            {
                work();
            }
            catch(Exception ex)
            {
                AaruLogging.Exception(ex, UI.Title_Error);
                SentrySdk.CaptureException(ex);
            }

            Dispatcher.UIThread.Post(next);
        });
    }

    void InitializeConsole()
    {
        CurrentProgress++;
        Message = UI.Initializing_console;

        RunStage(() =>
        {
            ConsoleHandler.Init();
            AaruLogging.WriteLine(UI.Aaru_started);
        }, LoadSettings);
    }

    void LoadSettings()
    {
        CurrentProgress++;
        Message = UI.Loading_settings;
        AaruLogging.WriteLine(UI.Loading_settings);

        // TODO: Detect there are no settings yet
        RunStage(Settings.Settings.LoadSettings, MigrateLocalDatabase);
    }

    void MigrateLocalDatabase()
    {
        CurrentProgress++;
        Message = UI.Migrating_local_database;
        AaruLogging.WriteLine(UI.Migrating_local_database);

        RunStage(() =>
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
                catch(Exception ex)
                {
                    // Should not ever arrive here, but if it does, keep trying to replace it anyway
                    SentrySdk.CaptureException(ex);
                }

                File.Delete(Settings.Settings.LocalDbPath);
                ctx = AaruContext.Create(Settings.Settings.LocalDbPath);
                ctx.Database.EnsureCreated();

                ctx.Database
                   .ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (\"MigrationId\" TEXT PRIMARY KEY, \"ProductVersion\" TEXT)");

                foreach(string migration in ctx.Database.GetPendingMigrations())
                {
#pragma warning disable EF1002
                    ctx.Database
                       .ExecuteSqlRaw($"INSERT INTO \"__EFMigrationsHistory\" (MigrationId, ProductVersion) VALUES ('{
                           migration}', '0.0.0')");
#pragma warning restore EF1002
                }

                ctx.SaveChanges();
            }

            // Remove duplicates
            foreach(var duplicate in ctx.SeenDevices.AsEnumerable()
                                        .GroupBy(static a => new
                                         {
                                             a.Manufacturer,
                                             a.Model,
                                             a.Revision,
                                             a.Bus
                                         })
                                        .Where(static a => a.Count() > 1)
                                        .Distinct()
                                        .Select(static a => a.Key))
            {
                ctx.RemoveRange(ctx.SeenDevices
                                   .Where(d => d.Manufacturer == duplicate.Manufacturer &&
                                               d.Model        == duplicate.Model        &&
                                               d.Revision     == duplicate.Revision     &&
                                               d.Bus          == duplicate.Bus)
                                   .Skip(1));
            }

            // Remove nulls
            ctx.RemoveRange(ctx.SeenDevices.Where(static d => d.Manufacturer == null &&
                                                              d.Model        == null &&
                                                              d.Revision     == null));

            ctx.SaveChanges();
        }, UpdateMainDatabase);
    }

    void UpdateMainDatabase()
    {
        CurrentProgress++;
        Message = UI.Updating_main_database;
        AaruLogging.WriteLine(UI.Updating_main_database);

        RunStage(() =>
        {
            bool mainDbUpdate = !File.Exists(Settings.Settings.MainDbPath);

            // TODO: Update database

            var mainContext = AaruContext.Create(Settings.Settings.MainDbPath, false);

            if(mainContext.Database.GetPendingMigrations().Any())
            {
                AaruLogging.WriteLine(UI.New_database_version_updating);

                try
                {
                    File.Delete(Settings.Settings.MainDbPath);
                }
                catch(Exception ex)
                {
                    AaruLogging.Error(UI.Exception_trying_to_remove_old_database_version);

                    AaruLogging.Error(UI.Please_manually_remove_file_at_0, Settings.Settings.MainDbPath);

                    SentrySdk.CaptureException(ex);

                    return;
                }

                // TODO: Update database
            }
        }, CheckGdprCompliance);
    }

    [SuppressMessage("ReSharper", "AsyncVoidMethod")]
    async void CheckGdprCompliance()
    {
        CurrentProgress++;
        Message = UI.Checking_GDPR_compliance;
        AaruLogging.WriteLine(UI.Checking_GDPR_compliance);

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
        AaruLogging.WriteLine(UI.Loading_statistics);

        RunStage(Statistics.LoadStats, RegisterEncodings);
    }

    void RegisterEncodings()
    {
        CurrentProgress++;
        Message = UI.Registering_encodings;
        AaruLogging.WriteLine(UI.Registering_encodings);

        RunStage(() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance), RegisterPlugins);
    }

    // There are too many places that depend on this being inited to be sure all are covered, so init it here.
    void RegisterPlugins()
    {
        CurrentProgress++;
        Message = UI.Registering_plugins;
        AaruLogging.WriteLine(UI.Registering_plugins);

        RunStage(PluginBase.Init, SaveStatistics);
    }

    void SaveStatistics()
    {
        CurrentProgress++;
        Message = UI.Saving_statistics;
        AaruLogging.WriteLine(UI.Saving_statistics);

        _ = Task.Run(async () =>
        {
            try
            {
                await Statistics.SaveStatsAsync();
            }
            catch(Exception ex)
            {
                AaruLogging.Exception(ex, UI.Title_Error);
                SentrySdk.CaptureException(ex);
            }

            Dispatcher.UIThread.Post(LoadMainWindow);
        });
    }

    void LoadMainWindow()
    {
        CurrentProgress++;
        Message = UI.Loading_main_window;
        AaruLogging.WriteLine(UI.Loading_main_window);
        WorkFinished?.Invoke(this, EventArgs.Empty);
    }

    internal event EventHandler WorkFinished;
}