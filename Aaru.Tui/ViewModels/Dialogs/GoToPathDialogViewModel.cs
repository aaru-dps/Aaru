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

using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Iciclecreek.Avalonia.WindowManager;

namespace Aaru.Tui.ViewModels.Dialogs;

public sealed partial class GoToPathDialogViewModel : ViewModelBase
{
    readonly ManagedWindow _dialog;
    [ObservableProperty]
    string _errorMessage = string.Empty;
    [ObservableProperty]
    bool _hasError;
    [ObservableProperty]
    string? _path = string.Empty;

    public GoToPathDialogViewModel(ManagedWindow dialog)
    {
        _dialog       = dialog;
        OkCommand     = new RelayCommand(Ok);
        CancelCommand = new RelayCommand(Cancel);
    }

    public ICommand OkCommand     { get; }
    public ICommand CancelCommand { get; }

    void Ok()
    {
        if(string.IsNullOrWhiteSpace(Path))
        {
            HasError     = true;
            ErrorMessage = Localization.Resources.Path_cannot_be_empty;

            return;
        }

        if(!Directory.Exists(Path))
        {
            HasError     = true;
            ErrorMessage = Localization.Resources.Path_does_not_exist;

            return;
        }

        _dialog.Close(true);
    }

    void Cancel()
    {
        _dialog.Close(false);
    }
}