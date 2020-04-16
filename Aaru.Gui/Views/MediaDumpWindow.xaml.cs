using System.ComponentModel;
using Aaru.Gui.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views
{
    public class MediaDumpWindow : Window
    {
        public MediaDumpWindow()
        {
            InitializeComponent();
        #if DEBUG
            this.AttachDevTools();
        #endif
        }

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        protected override void OnClosing(CancelEventArgs e)
        {
            (DataContext as MediaDumpViewModel)?.ExecuteStopCommand();
            base.OnClosing(e);
        }
    }
}