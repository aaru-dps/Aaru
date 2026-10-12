// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmViewer.axaml.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI windows.
//
// --[ Description ] ----------------------------------------------------------
//
//     Data Position Measurement viewer window.
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
using Aaru.Gui.Controls;
using Aaru.Gui.ViewModels.Windows;
using Aaru.Localization;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Aaru.Gui.Views.Windows;

public sealed partial class DpmViewer : Window
{
    public DpmViewer()
    {
        InitializeComponent();

        Plot.ViewChanged       += (_, _) => UpdateStatus();
        Plot.HoveredBinChanged += (_, _) => UpdateStatus();
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if(DataContext is DpmViewerViewModel vm)
            vm.ZoomRequested += (_, range) => Plot.SetView(range.first, range.last);

        UpdateStatus();
    }

    void OnResetZoom(object sender, RoutedEventArgs e)
    {
        Plot.ResetView();
    }

    void OnFitValues(object sender, RoutedEventArgs e)
    {
        Plot.FitValues();
    }

    /// <summary>Shows the sectors in view and the bin under the pointer</summary>
    void UpdateStatus()
    {
        if(DataContext is not DpmViewerViewModel vm || vm.Analysis is not {} analysis) return;

        vm.ViewText = string.Format(UI.DPM_view_0_to_1, (ulong)Plot.ViewFirst, (ulong)Plot.ViewLast);

        vm.HoverText = Plot.HoveredBin >= 0
                           ? string.Join(" · ",
                                         DpmPlot.Describe(analysis,
                                                          Plot.HoveredBin,
                                                          Plot.HoveredSector,
                                                          vm.Comparison ?? default,
                                                          vm.ComparisonName))
                           : "";
    }
}