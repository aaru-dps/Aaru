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

public sealed partial class GoToSectorDialogViewModel : ViewModelBase
{
    readonly ulong         _maxSector;
    readonly ManagedWindow _dialog;
    [ObservableProperty]
    string _errorMessage = string.Empty;
    [ObservableProperty]
    bool _hasError;
    [ObservableProperty]
    string _sectorNumber = string.Empty;

    public GoToSectorDialogViewModel(ManagedWindow dialog, ulong maxSector)
    {
        _dialog       = dialog;
        _maxSector    = maxSector;
        OkCommand     = new RelayCommand(Ok);
        CancelCommand = new RelayCommand(Cancel);
    }

    public ulong? Result { get; private set; }

    public ICommand OkCommand     { get; }
    public ICommand CancelCommand { get; }

    void Ok()
    {
        if(!ulong.TryParse((string?)SectorNumber, out ulong sector))
        {
            ErrorMessage = Localization.Resources.Please_enter_a_valid_number;
            HasError     = true;

            return;
        }

        if(sector > _maxSector)
        {
            ErrorMessage = string.Format(Localization.Resources.Sector_number_must_be_less_than_or_equal_to_0,
                                         _maxSector);

            HasError = true;

            return;
        }

        Result = sector;
        _dialog.Close(true);
    }

    void Cancel()
    {
        Result = null;
        _dialog.Close(false);
    }
}