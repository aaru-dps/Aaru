using System.ComponentModel;
using Aaru.Gui.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views
{
    public class ImageChecksumWindow : Window
    {
        public ImageChecksumWindow()
        {
            InitializeComponent();
        #if DEBUG
            this.AttachDevTools();
        #endif
        }

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        protected override void OnClosing(CancelEventArgs e)
        {
            (DataContext as ImageChecksumViewModel)?.ExecuteStopCommand();
            base.OnClosing(e);
        }
    }
}