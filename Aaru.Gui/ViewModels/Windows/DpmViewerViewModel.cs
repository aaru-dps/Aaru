// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmViewerViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
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

using Aaru.CommonTypes.Structs;
using Aaru.Core.Image;
using Aaru.Gui.Views.Windows;
using Aaru.Localization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Aaru.Gui.ViewModels.Windows;

public sealed partial class DpmViewerViewModel : ViewModelBase
{
    [ObservableProperty]
    string _hoverText;
    [ObservableProperty]
    string _viewText;

    public DpmViewerViewModel() { }

    public DpmViewerViewModel(DpmViewer window, DataPositionMeasurement dpm, DpmAnalysisResult analysis,
                              string    imageName)
    {
        Dpm       = dpm;
        Analysis  = analysis;
        ImageName = imageName;
        Title     = string.Format(UI.Title_DPM_of_0, imageName);
    }

    public DataPositionMeasurement Dpm       { get; }
    public DpmAnalysisResult?      Analysis  { get; }
    public string                  ImageName { get; }
    public string                  Title     { get; }
}