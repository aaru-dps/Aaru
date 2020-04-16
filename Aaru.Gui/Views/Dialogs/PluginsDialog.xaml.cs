using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views.Dialogs
{
    public class PluginsDialog : Window
    {
        public PluginsDialog()
        {
            InitializeComponent();
        #if DEBUG
            this.AttachDevTools();
        #endif
        }

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}