using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views.Windows
{
    public class ImageSidecarWindow : Window
    {
        public ImageSidecarWindow()
        {
            InitializeComponent();
        #if DEBUG
            this.AttachDevTools();
        #endif
        }

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}