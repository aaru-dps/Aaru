using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Panels
{
    public class SubdirectoryPanel : UserControl
    {
        public SubdirectoryPanel() => InitializeComponent();

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}