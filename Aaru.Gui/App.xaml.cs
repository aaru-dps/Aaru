using Aaru.Gui.ViewModels;
using Aaru.Gui.Views;
using Avalonia;
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
                desktop.MainWindow = new SplashWindow
                {
                    DataContext = new SplashWindowViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}