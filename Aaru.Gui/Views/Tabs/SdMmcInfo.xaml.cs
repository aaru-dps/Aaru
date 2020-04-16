using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views.Tabs
{
    public class SdMmcInfoTab : UserControl
    {
        public SdMmcInfoTab() => InitializeComponent();

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}