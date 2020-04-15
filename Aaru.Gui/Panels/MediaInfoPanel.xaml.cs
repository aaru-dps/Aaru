using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Panels
{
    public class MediaInfoPanel : UserControl
    {
        public MediaInfoPanel() => InitializeComponent();

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}