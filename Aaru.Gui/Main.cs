using Avalonia;
using Avalonia.Dialogs;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;

namespace Aaru.Gui
{
    public static class Main
    {
        public static int Start(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.
                                                       Configure<App>().UsePlatformDetect().LogToDebug().
                                                       UseReactiveUI().UseManagedSystemDialogs();
    }
}