using System.ComponentModel;
using Aaru.Gui.ViewModels.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views.Windows
{
    public class ImageConvertWindow : Window
    {
        public ImageConvertWindow()
        {
            InitializeComponent();
        #if DEBUG
            this.AttachDevTools();
        #endif
        }

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        protected override void OnClosing(CancelEventArgs e)
        {
            (DataContext as ImageConvertViewModel)?.ExecuteStopCommand();
            base.OnClosing(e);
        }
    }
}