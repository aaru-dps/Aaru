using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Tabs
{
    public class DvdInfoTab : UserControl
    {
        public DvdInfoTab() => InitializeComponent();

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}