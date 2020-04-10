using System;
using Aaru.Gui.ViewModels;
using Aaru.Gui.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui
{
    public class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var swvm = new SplashWindowViewModel();
                swvm.WorkFinished += OnSplashFinished;

                desktop.MainWindow = new SplashWindow
                {
                    DataContext = swvm
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        void OnSplashFinished(object sender, EventArgs e)
        {
            if(!(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop))
                return;

            // Ensure not exit
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Close splash window
            desktop.MainWindow.Close();

            // Create and show main window
            desktop.MainWindow             = new MainWindow();
            desktop.MainWindow.DataContext = new MainWindowViewModel(desktop.MainWindow as MainWindow);
            desktop.MainWindow.Show();

            // Now can close when all windows are closed
            desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;
        }

        void OnAboutClicked(object sender, EventArgs args)
        {
            if(!(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) ||
               !(desktop.MainWindow is MainWindow mainWindow)                            ||
               !(mainWindow.DataContext is MainWindowViewModel mainWindowViewModel))
                return;

            mainWindowViewModel.ExecuteAboutCommand();
        }
    }
}