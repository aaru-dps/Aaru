using System;
using Aaru.Gui.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        #if DEBUG
            this.AttachDevTools();
        #endif
        }

        void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            (DataContext as MainWindowViewModel)?.LoadComplete();
        }
    }
}