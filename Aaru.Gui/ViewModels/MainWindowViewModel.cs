using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Aaru.CommonTypes.Interop;
using Aaru.Database;
using Aaru.Gui.Models;
using Aaru.Gui.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MessageBox.Avalonia;
using ReactiveUI;

namespace Aaru.Gui.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        readonly DevicesRootModel _devicesRoot;
        readonly ImagesRootModel  _imagesRoot;
        readonly MainWindow       _view;
        ConsoleWindow             _consoleWindow;
        bool                      _devicesSupported;

        public MainWindowViewModel(MainWindow view)
        {
            AboutCommand      = ReactiveCommand.Create(ExecuteAboutCommand);
            EncodingsCommand  = ReactiveCommand.Create(ExecuteEncodingsCommand);
            PluginsCommand    = ReactiveCommand.Create(ExecutePluginsCommand);
            StatisticsCommand = ReactiveCommand.Create(ExecuteStatisticsCommand);
            ExitCommand       = ReactiveCommand.Create(ExecuteExitCommand);
            SettingsCommand   = ReactiveCommand.Create(ExecuteSettingsCommand);
            ConsoleCommand    = ReactiveCommand.Create(ExecuteConsoleCommand);
            _view             = view;
            TreeRoot          = new ObservableCollection<RootModel>();

            _imagesRoot = new ImagesRootModel
            {
                Name = "Images"
            };

            TreeRoot.Add(_imagesRoot);

            switch(DetectOS.GetRealPlatformID())
            {
                case PlatformID.Win32NT:
                case PlatformID.Linux:
                case PlatformID.FreeBSD:
                    _devicesRoot = new DevicesRootModel
                    {
                        Name = "Devices"
                    };

                    TreeRoot.Add(_devicesRoot);
                    DevicesSupported = true;

                    break;
            }
        }

        public bool DevicesSupported
        {
            get => _devicesSupported;
            set => this.RaiseAndSetIfChanged(ref _devicesSupported, value);
        }

        public bool NativeMenuSupported =>
            NativeMenu.GetIsNativeMenuExported((Application.Current.ApplicationLifetime as
                                                    IClassicDesktopStyleApplicationLifetime)?.MainWindow);

        public string                          Greeting          => "Welcome to Aaru!";
        public ObservableCollection<RootModel> TreeRoot          { get; }
        public ReactiveCommand<Unit, Unit>     AboutCommand      { get; }
        public ReactiveCommand<Unit, Unit>     ConsoleCommand    { get; }
        public ReactiveCommand<Unit, Unit>     EncodingsCommand  { get; }
        public ReactiveCommand<Unit, Unit>     PluginsCommand    { get; }
        public ReactiveCommand<Unit, Unit>     StatisticsCommand { get; }
        public ReactiveCommand<Unit, Unit>     ExitCommand       { get; }
        public ReactiveCommand<Unit, Unit>     SettingsCommand   { get; }

        internal void ExecuteAboutCommand()
        {
            var dialog = new AboutDialog();
            dialog.DataContext = new AboutDialogViewModel(dialog);
            dialog.ShowDialog(_view);
        }

        internal void ExecuteEncodingsCommand()
        {
            var dialog = new EncodingsDialog();
            dialog.DataContext = new EncodingsDialogViewModel(dialog);
            dialog.ShowDialog(_view);
        }

        internal void ExecutePluginsCommand()
        {
            var dialog = new PluginsDialog();
            dialog.DataContext = new PluginsDialogViewModel(dialog);
            dialog.ShowDialog(_view);
        }

        internal void ExecuteStatisticsCommand()
        {
            using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

            if(!ctx.Commands.Any()     &&
               !ctx.Filesystems.Any()  &&
               !ctx.Filters.Any()      &&
               !ctx.MediaFormats.Any() &&
               !ctx.Medias.Any()       &&
               !ctx.Partitions.Any()   &&
               !ctx.SeenDevices.Any())
            {
                MessageBoxManager.GetMessageBoxStandardWindow("Warning", "There are no statistics.").ShowDialog(_view);

                return;
            }

            var dialog = new StatisticsDialog();
            dialog.DataContext = new StatisticsDialogViewModel(dialog);
            dialog.ShowDialog(_view);
        }

        internal async void ExecuteSettingsCommand()
        {
            var dialog = new SettingsDialog();
            dialog.DataContext = new SettingsDialogViewModel(dialog, false);
            await dialog.ShowDialog(_view);
        }

        internal void ExecuteExitCommand() =>
            (Application.Current.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)?.Shutdown();

        internal void ExecuteConsoleCommand()
        {
            if(_consoleWindow is null)
            {
                _consoleWindow             = new ConsoleWindow();
                _consoleWindow.DataContext = new ConsoleWindowViewModel(_consoleWindow);
            }

            _consoleWindow.Show();
        }
    }
}