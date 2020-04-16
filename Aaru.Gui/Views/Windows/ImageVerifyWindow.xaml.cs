using System.ComponentModel;
using Aaru.Gui.ViewModels.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views.Windows
{
    public class ImageVerifyWindow : Window
    {
        public ImageVerifyWindow()
        {
            InitializeComponent();
        #if DEBUG
            this.AttachDevTools();
        #endif
        }

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        protected override void OnClosing(CancelEventArgs e)
        {
            (DataContext as ImageVerifyViewModel)?.ExecuteStopCommand();
            base.OnClosing(e);
        }
    }
}