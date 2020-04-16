using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views.Dialogs
{
    public class StatisticsDialog : Window
    {
        public StatisticsDialog()
        {
            InitializeComponent();
        #if DEBUG
            this.AttachDevTools();
        #endif
        }

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}