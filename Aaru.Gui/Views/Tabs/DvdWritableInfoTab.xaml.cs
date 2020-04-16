using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views.Tabs
{
    public class DvdWritableInfoTab : UserControl
    {
        public DvdWritableInfoTab() => InitializeComponent();

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}