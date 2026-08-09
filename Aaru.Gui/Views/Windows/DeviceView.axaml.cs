// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DeviceView.axaml.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI views.
//
// --[ Description ] ----------------------------------------------------------
//
//     View for device.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.Gui.ViewModels.Windows;
using Avalonia.Controls;

namespace Aaru.Gui.Views.Windows;

public partial class DeviceView : Window
{
    public DeviceView()
    {
        InitializeComponent();

        Closed += (_, _) => (DataContext as DeviceViewModel)?.Closed();
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if(DataContext is not DeviceViewModel vm) return;

        vm.LoadData();
    }
}