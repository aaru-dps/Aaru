using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Tabs
{
    public class ScsiInfoTab : UserControl
    {
        public ScsiInfoTab() => InitializeComponent();

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}