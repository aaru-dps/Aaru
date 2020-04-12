using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Tabs
{
    public class AtaInfoTab : UserControl
    {
        public AtaInfoTab() => InitializeComponent();

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}