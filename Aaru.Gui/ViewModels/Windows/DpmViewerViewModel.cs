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

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Image;
using Aaru.Gui.Controls;
using Aaru.Gui.Models;
using Aaru.Gui.ViewModels.Tabs;
using Aaru.Gui.Views.Windows;
using Aaru.Localization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Aaru.Gui.ViewModels.Windows;

public sealed partial class DpmViewerViewModel : ViewModelBase
{
    [ObservableProperty]
    string _hoverText;
    [ObservableProperty]
    DpmBandModel _selectedBand;
    [ObservableProperty]
    DpmCalibrationModel _selectedCalibration;
    [ObservableProperty]
    DpmLayerModel _selectedLayer;
    [ObservableProperty]
    bool _showDensity = true;
    [ObservableProperty]
    bool _showDeviation = true;
    [ObservableProperty]
    bool _showRpm = true;
    [ObservableProperty]
    bool _showStatus = true;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DensityUnit))]
    int _unitIndex;
    [ObservableProperty]
    string _viewText;

    public DpmViewerViewModel()
    {
        Summary      = [];
        Layers       = [];
        Calibrations = [];
        Bands        = [];
    }

    public DpmViewerViewModel(DpmViewer window, DataPositionMeasurement dpm, DpmAnalysisResult analysis,
                              string    imageName)
    {
        Dpm       = dpm;
        Analysis  = analysis;
        ImageName = imageName;
        Title     = string.Format(UI.Title_DPM_of_0, imageName);

        Summary = new ObservableCollection<DpmSummaryModel>(DpmInfoViewModel.BuildSummary(analysis));

        Layers = new ObservableCollection<DpmLayerModel>(analysis.Layers.Select(static l => new DpmLayerModel
        {
            Layer          = l.Layer,
            FirstLba       = l.FirstLba,
            LastLba        = l.LastLba,
            Bins           = l.Bins,
            Turns          = l.Turns.ToString("F2"),
            Density        = string.Format(UI.DPM_0_to_1_range, l.MinimumDensity.ToString("F4"),
                                           l.MaximumDensity.ToString("F4")),
            MedianDensity  = l.MedianDensity.ToString("F4"),
            SectorsPerTurn = string.Format(UI.DPM_0_to_1_range, l.FirstSectorsPerTurn.ToString("F3"),
                                           l.LastSectorsPerTurn.ToString("F3")),
            Noise          = l.Fitted ? l.DeviationSigma.ToString("P3") : ""
        }));

        Calibrations = new ObservableCollection<DpmCalibrationModel>(analysis.Calibrations.Select(static c =>
            new DpmCalibrationModel
            {
                Lba            = c.Lba,
                Layer          = c.Layer,
                Rpm            = c.Rpm.ToString("F1"),
                Period         = c.RotationPeriod.ToString("F3"),
                SectorsPerTurn = c.SectorsPerTurn.ToString("F3")
            }));

        Bands = new ObservableCollection<DpmBandModel>(analysis.Bands.Select(static b => new DpmBandModel
        {
            FirstLba         = b.FirstLba,
            LastLba          = b.LastLba,
            Layer            = b.Layer,
            MeanDeviation    = b.MeanDeviation.ToString("+0.00%;-0.00%"),
            MaximumDeviation = b.MaximumDeviation.ToString("+0.00%;-0.00%")
        }));
    }

    /// <summary>Raised to show a range of sectors in the graph</summary>
    public event EventHandler<(double first, double last)> ZoomRequested;

    public ObservableCollection<DpmSummaryModel>     Summary      { get; }
    public ObservableCollection<DpmLayerModel>       Layers       { get; }
    public ObservableCollection<DpmCalibrationModel> Calibrations { get; }
    public ObservableCollection<DpmBandModel>        Bands        { get; }

    partial void OnSelectedLayerChanged(DpmLayerModel value)
    {
        if(value != null) ZoomRequested?.Invoke(this, (value.FirstLba, value.LastLba));
    }

    partial void OnSelectedBandChanged(DpmBandModel value)
    {
        if(value != null) ShowRegion(value.FirstLba, value.LastLba);
    }

    partial void OnSelectedCalibrationChanged(DpmCalibrationModel value)
    {
        if(value is null || Analysis is not {} analysis) return;

        // Some bins each side of the calibration
        double half = 25.0 * analysis.NominalSpacing;

        ZoomRequested?.Invoke(this, (value.Lba - half, value.Lba + half));
    }

    /// <summary>Shows a region of sectors with as many sectors again around it</summary>
    void ShowRegion(ulong first, ulong last)
    {
        double margin = Math.Max((last - first + 1) / 2.0, 4.0 * (Analysis?.NominalSpacing ?? 1));

        ZoomRequested?.Invoke(this, (first - margin, last + 1 + margin));
    }

    public DataPositionMeasurement Dpm         { get; }
    public DpmAnalysisResult?      Analysis    { get; }
    public string                  ImageName   { get; }
    public string                  Title       { get; }
    public bool                    HasRpm      => Analysis?.Calibrations.Length > 0;
    public DpmDensityUnit          DensityUnit => (DpmDensityUnit)UnitIndex;
}