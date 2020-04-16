using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Tabs
{
    public class XboxInfoTab : UserControl
    {
        public XboxInfoTab() => InitializeComponent();

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}