using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views.Panels
{
    public class DeviceInfoPanel : UserControl
    {
        public DeviceInfoPanel() => InitializeComponent();

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}