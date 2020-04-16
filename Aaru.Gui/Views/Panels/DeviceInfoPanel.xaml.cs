using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Panels
{
    public class DeviceInfoPanel : UserControl
    {
        public DeviceInfoPanel() => InitializeComponent();

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}