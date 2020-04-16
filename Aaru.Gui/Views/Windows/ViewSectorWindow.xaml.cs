using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views.Windows
{
    public class ViewSectorWindow : Window
    {
        public ViewSectorWindow()
        {
            InitializeComponent();
        #if DEBUG
            this.AttachDevTools();
        #endif
        }

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}