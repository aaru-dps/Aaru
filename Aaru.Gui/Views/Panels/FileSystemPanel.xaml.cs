using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views.Panels
{
    public class FileSystemPanel : UserControl
    {
        public FileSystemPanel() => InitializeComponent();

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}