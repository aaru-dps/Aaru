// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Text User Interface.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using Aaru.Tui.ViewModels.Windows;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Aaru.Tui.Views.Windows;

public partial class FileView : UserControl
{
    bool _focusPending;

    public FileView()
    {
        InitializeComponent();

        // Tunnel so we see Enter before the ListBox's own key handling marks it as handled
        FileList.AddHandler(KeyDownEvent, ListBox_OnKeyDown, RoutingStrategies.Tunnel);

        // Every directory change rebuilds the item list, so put the keyboard focus back on the first entry
        FileList.Items.CollectionChanged += (_, _) => FocusFileList();
    }

    void ListBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if(e.Key != Key.Enter) return;
        if(DataContext is not FileViewViewModel vm || !vm.OpenSelectedFileCommand.CanExecute(null)) return;

        vm.OpenSelectedFileCommand.Execute(null);
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        FocusFileList();
    }

    /// <summary>Selects the first file if nothing is selected and moves keyboard focus into the file list.</summary>
    void FocusFileList()
    {
        if(_focusPending) return;

        _focusPending = true;

        Dispatcher.UIThread.Post(() =>
        {
            _focusPending = false;

            if(FileList.ItemCount == 0) return;

            if(FileList.SelectedIndex < 0) FileList.SelectedIndex = 0;

            FileList.ContainerFromIndex(FileList.SelectedIndex)?.Focus();
        }, DispatcherPriority.Loaded);
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        (DataContext as FileViewViewModel)?.LoadComplete();
    }
}