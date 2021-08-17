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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
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
using Aaru.Settings;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Windows
{
    public sealed class SplashWindowViewModel : ViewModelBase
    {
        readonly SplashWindow _view;
        double                _currentProgress;
        double                _maxProgress;
        string                _message;

        public SplashWindowViewModel(SplashWindow view) => _view = view;

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
            Message         = "Welcome to Aaru!";
            MaxProgress     = 9;
            CurrentProgress = 0;

            Dispatcher.UIThread.Post(InitializeConsole);
        }

        void InitializeConsole()
        {
            CurrentProgress++;
            Message = "Initializing console...";

            Task.Run(() =>
            {
                ConsoleHandler.Init();
                AaruConsole.WriteLine("Aaru started!");

                Dispatcher.UIThread.Post(LoadSettings);
            });
        }

        void LoadSettings()
        {
            CurrentProgress++;
            Message = "Loading settings...";
            AaruConsole.WriteLine("Loading settings...");

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
            Message = "Migrating local database...";
            AaruConsole.WriteLine("Migrating local database...");

            Task.Run(() =>
            {
                AaruContext ctx;

                try
                {
                    ctx = AaruContext.Create(Settings.Settings.LocalDbPath);
                    ctx.Database.Migrate();
                }
                catch(NotSupportedException)
                {
                    File.Delete(Settings.Settings.LocalDbPath);
                    ctx = AaruContext.Create(Settings.Settings.LocalDbPath);
                    ctx.Database.EnsureCreated();

                    ctx.Database.
                        ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (\"MigrationId\" TEXT PRIMARY KEY, \"ProductVersion\" TEXT)");

                    foreach(string migration in ctx.Database.GetPendingMigrations())
                    {
                        ctx.Database.
                            ExecuteSqlRaw($"INSERT INTO \"__EFMigrationsHistory\" (MigrationId, ProductVersion) VALUES ('{migration}', '0.0.0')");
                    }

                    ctx.SaveChanges();
                }

                // Remove duplicates
                foreach(var duplicate in ctx.SeenDevices.AsEnumerable()!.GroupBy(a => new
                {
                    a.Manufacturer,
                    a.Model,
                    a.Revision,
                    a.Bus
                }).Where(a => a.Count() > 1).Distinct().Select(a => a.Key))
                    ctx.RemoveRange(ctx.SeenDevices!.
                                        Where(d => d.Manufacturer == duplicate.Manufacturer &&
                                                   d.Model == duplicate.Model && d.Revision == duplicate.Revision &&
                                                   d.Bus == duplicate.Bus).Skip(1));

                // Remove nulls
                ctx.RemoveRange(ctx.SeenDevices!.Where(d => d.Manufacturer == null && d.Model == null &&
                                                            d.Revision     == null));

                ctx.SaveChanges();

                Dispatcher.UIThread.Post(UpdateMainDatabase);
            });
        }

        void UpdateMainDatabase()
        {
            CurrentProgress++;
            Message = "Updating main database...";
            AaruConsole.WriteLine("Updating main database...");

            Task.Run(() =>
            {
                bool mainDbUpdate = false;

                if(!File.Exists(Settings.Settings.MainDbPath))
                {
                    mainDbUpdate = true;

                    // TODO: Update database
                }

                var mainContext = AaruContext.Create(Settings.Settings.MainDbPath);

                if(mainContext.Database.GetPendingMigrations().Any())
                {
                    AaruConsole.WriteLine("New database version, updating...");

                    try
                    {
                        File.Delete(Settings.Settings.MainDbPath);
                    }
                    catch(Exception)
                    {
                        AaruConsole.
                            ErrorWriteLine("Exception trying to remove old database version, cannot continue...");

                        AaruConsole.ErrorWriteLine("Please manually remove file at {0}", Settings.Settings.MainDbPath);
                    }

                    // TODO: Update database
                }

                Dispatcher.UIThread.Post(CheckGdprCompliance);
            });
        }

        async void CheckGdprCompliance()
        {
            CurrentProgress++;
            Message = "Checking GDPR compliance...";
            AaruConsole.WriteLine("Checking GDPR compliance...");

            if(Settings.Settings.Current.GdprCompliance < DicSettings.GDPR_LEVEL)
            {
                var settingsDialog          = new SettingsDialog();
                var settingsDialogViewModel = new SettingsViewModel(settingsDialog, true);
                settingsDialog.DataContext = settingsDialogViewModel;
                await settingsDialog.ShowDialog(_view);
            }

            LoadStatistics();
        }

        void LoadStatistics()
        {
            CurrentProgress++;
            Message = "Loading statistics...";
            AaruConsole.WriteLine("Loading statistics...");

            Task.Run(() =>
            {
                Statistics.LoadStats();

                Dispatcher.UIThread.Post(RegisterEncodings);
            });
        }

        void RegisterEncodings()
        {
            CurrentProgress++;
            Message = "Registering encodings...";
            AaruConsole.WriteLine("Registering encodings...");

            Task.Run(() =>
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                Dispatcher.UIThread.Post(SaveStatistics);
            });
        }

        void SaveStatistics()
        {
            CurrentProgress++;
            Message = "Saving statistics...";
            AaruConsole.WriteLine("Saving statistics...");

            Task.Run(() =>
            {
                Statistics.SaveStats();

                Dispatcher.UIThread.Post(LoadMainWindow);
            });
        }

        void LoadMainWindow()
        {
            CurrentProgress++;
            Message = "Loading main window...";
            AaruConsole.WriteLine("Loading main window...");
            WorkFinished?.Invoke(this, EventArgs.Empty);
        }

        internal event EventHandler WorkFinished;
    }
}