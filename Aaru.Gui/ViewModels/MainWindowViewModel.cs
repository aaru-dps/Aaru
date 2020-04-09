using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Aaru.Gui.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Aaru!";

        public bool NativeMenuNotSupported => !NativeMenu.GetIsNativeMenuExported((Application.Current.ApplicationLifetime as
                                                                                       IClassicDesktopStyleApplicationLifetime)?.MainWindow);
    }
}