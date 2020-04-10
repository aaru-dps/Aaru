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
            AboutCommand = ReactiveCommand.Create(ExecuteAboutCommand);
            _view        = view;
        }

        public string Greeting => "Welcome to Aaru!";

        public bool NativeMenuNotSupported =>
            !NativeMenu.GetIsNativeMenuExported((Application.Current.ApplicationLifetime as
                                                     IClassicDesktopStyleApplicationLifetime)?.MainWindow);

        public ReactiveCommand<Unit, Unit> AboutCommand { get; }

        internal void ExecuteAboutCommand()
        {
            var dialog = new AboutDialog();
            dialog.DataContext = new AboutDialogViewModel(dialog);
            dialog.ShowDialog(_view);
        }
    }
}