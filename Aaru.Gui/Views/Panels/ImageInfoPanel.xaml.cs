using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Panels
{
    public class ImageInfoPanel : UserControl
    {
        public ImageInfoPanel() => InitializeComponent();

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}