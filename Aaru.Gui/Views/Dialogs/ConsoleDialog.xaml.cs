using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views.Dialogs
{
    public class ConsoleWindow : Window
    {
        public ConsoleWindow()
        {
            InitializeComponent();
        #if DEBUG
            this.AttachDevTools();
        #endif
        }

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();

            base.OnClosing(e);
        }
    }
}