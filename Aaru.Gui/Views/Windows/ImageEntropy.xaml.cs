using System.ComponentModel;
using Aaru.Gui.ViewModels.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views.Windows
{
    public class ImageEntropy : Window
    {
        public ImageEntropy()
        {
            InitializeComponent();
        #if DEBUG
            this.AttachDevTools();
        #endif
        }

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        protected override void OnClosing(CancelEventArgs e)
        {
            (DataContext as ImageEntropyViewModel)?.ExecuteStopCommand();
            base.OnClosing(e);
        }
    }
}