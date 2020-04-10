using System.Reactive;
using Aaru.Gui.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;

namespace Aaru.Gui.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        readonly MainWindow _view;

        public MainWindowViewModel(MainWindow view)
        {
            AboutCommand     = ReactiveCommand.Create(ExecuteAboutCommand);
            EncodingsCommand = ReactiveCommand.Create(ExecuteEncodingsCommand);
            _view            = view;
        }

        public string Greeting => "Welcome to Aaru!";

        public bool NativeMenuNotSupported =>
            !NativeMenu.GetIsNativeMenuExported((Application.Current.ApplicationLifetime as
                                                     IClassicDesktopStyleApplicationLifetime)?.MainWindow);

        public ReactiveCommand<Unit, Unit> AboutCommand     { get; }
        public ReactiveCommand<Unit, Unit> EncodingsCommand { get; }

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
    }
}