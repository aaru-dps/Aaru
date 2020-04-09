using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aaru.Console;
using Aaru.Core;
using Aaru.Database;
using Aaru.Settings;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;

namespace Aaru.Gui.ViewModels
{
    public class SplashWindowViewModel : ViewModelBase
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

                Dispatcher.UIThread.Post(LoadSettings);
            });
        }

        void LoadSettings()
        {
            CurrentProgress++;
            Message = "Loading settings...";

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

            Task.Run(() =>
            {
                var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);
                ctx.Database.Migrate();
                ctx.SaveChanges();

                Dispatcher.UIThread.Post(UpdateMasterDatabase);
            });
        }

        void UpdateMasterDatabase()
        {
            CurrentProgress++;
            Message = "Updating master database...";

            Task.Run(() =>
            {
                bool masterDbUpdate = false;

                if(!File.Exists(Settings.Settings.MasterDbPath))
                {
                    masterDbUpdate = true;

                    // TODO: Update database
                }

                var masterContext = AaruContext.Create(Settings.Settings.MasterDbPath);

                if(masterContext.Database.GetPendingMigrations().Any())
                {
                    AaruConsole.WriteLine("New database version, updating...");

                    try
                    {
                        File.Delete(Settings.Settings.MasterDbPath);
                    }
                    catch(Exception)
                    {
                        AaruConsole.
                            ErrorWriteLine("Exception trying to remove old database version, cannot continue...");

                        AaruConsole.ErrorWriteLine("Please manually remove file at {0}",
                                                   Settings.Settings.MasterDbPath);
                    }

                    // TODO: Update database
                }

                Dispatcher.UIThread.Post(CheckGdprCompliance);
            });
        }

        void CheckGdprCompliance()
        {
            CurrentProgress++;
            Message = "Checking GDPR compliance...";

            Task.Run(() =>
            {
                // TODO: Settings window
                if(Settings.Settings.Current.GdprCompliance < DicSettings.GdprLevel)
                    AaruConsole.ErrorWriteLine("Settings window not yet implemented");

                Dispatcher.UIThread.Post(LoadStatistics);
            });
        }

        void LoadStatistics()
        {
            CurrentProgress++;
            Message = "Loading statistics...";

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
            WorkFinished?.Invoke(this, EventArgs.Empty);
        }

        internal event EventHandler WorkFinished;
    }
}