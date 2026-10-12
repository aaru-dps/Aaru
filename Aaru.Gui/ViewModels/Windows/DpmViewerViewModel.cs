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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using Aaru.Core.Graphics;
using Aaru.Core.Image;
using Aaru.Gui.Controls;
using Aaru.Gui.Models;
using Aaru.Gui.ViewModels.Tabs;
using Aaru.Gui.Views.Windows;
using Aaru.Localization;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Sentry;

namespace Aaru.Gui.ViewModels.Windows;

public sealed partial class DpmViewerViewModel : ViewModelBase
{
    readonly DpmViewer _window;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsComparing))]
    DpmComparisonResult? _comparison;
    [ObservableProperty]
    string _comparisonName;
    [ObservableProperty]
    string _comparisonVerdict;
    [ObservableProperty]
    DpmBandModel _selectedDifference;
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
        Summary           = [];
        Layers            = [];
        Calibrations      = [];
        Bands             = [];
        ComparisonSummary = [];
        Differences       = [];
    }

    public DpmViewerViewModel(DpmViewer window, DataPositionMeasurement dpm, DpmAnalysisResult analysis,
                              string    imageName)
    {
        Dpm       = dpm;
        Analysis  = analysis;
        ImageName = imageName;
        Title     = string.Format(UI.Title_DPM_of_0, imageName);
        _window   = window;

        ComparisonSummary      = [];
        Differences            = [];
        CompareCommand         = new AsyncRelayCommand(CompareAsync);
        ClearComparisonCommand = new RelayCommand(ClearComparison);
        SaveGraphCommand       = new AsyncRelayCommand(SaveGraphAsync);
        SaveViewCommand        = new AsyncRelayCommand(SaveViewAsync);
        ExportCsvCommand       = new AsyncRelayCommand(ExportCsvAsync);

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

    public ObservableCollection<DpmSummaryModel>     Summary                { get; }
    public ObservableCollection<DpmLayerModel>       Layers                 { get; }
    public ObservableCollection<DpmCalibrationModel> Calibrations           { get; }
    public ObservableCollection<DpmBandModel>        Bands                  { get; }
    public ObservableCollection<DpmSummaryModel>     ComparisonSummary      { get; }
    public ObservableCollection<DpmBandModel>        Differences            { get; }
    public ICommand                                  CompareCommand         { get; }
    public ICommand                                  ClearComparisonCommand { get; }
    public ICommand                                  SaveGraphCommand       { get; }
    public ICommand                                  SaveViewCommand        { get; }
    public ICommand                                  ExportCsvCommand       { get; }
    public bool                                      IsComparing            => Comparison.HasValue;

    partial void OnSelectedDifferenceChanged(DpmBandModel value)
    {
        if(value != null) ShowRegion(value.FirstLba, value.LastLba);
    }

    /// <summary>Asks for a file to save to</summary>
    /// <returns>The path, or <c>null</c> if cancelled</returns>
    async Task<string> AskSavePathAsync(string title, string extension, string typeName)
    {
        IStorageFile result = await _window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title             = title,
            DefaultExtension  = extension,
            SuggestedFileName = Path.GetFileNameWithoutExtension(ImageName) + ".dpm." + extension,
            FileTypeChoices =
            [
                new FilePickerFileType(typeName)
                {
                    Patterns = ["*." + extension]
                }
            ]
        });

        return result?.Path.LocalPath;
    }

    /// <summary>Shows an error</summary>
    Task ShowErrorAsync(string message)
    {
        return MessageBoxManager.GetMessageBoxStandard(UI.Title_Error, message, ButtonEnum.Ok, Icon.Error)
                                .ShowWindowDialogAsync(_window);
    }

    /// <summary>Saves the whole DPM, or the comparison, as drawn by the command line</summary>
    async Task SaveGraphAsync()
    {
        string path = await AskSavePathAsync(UI.ButtonLabel_Save_DPM_graph_as_PNG, "png", "PNG");

        if(path is null) return;

        try
        {
            if(Comparison is {} comparison)
                await Task.Run(() => DpmGraph.WriteComparison(comparison, path, ImageName, ComparisonName));
            else
                await Task.Run(() => DpmGraph.Write(Dpm, path, ImageName));
        }
        catch(Exception ex)
        {
            SentrySdk.CaptureException(ex);
            await ShowErrorAsync(UI.Error_writing_DPM_graph);
        }
    }

    /// <summary>Saves the graph as shown, with its current zoom</summary>
    async Task SaveViewAsync()
    {
        string path = await AskSavePathAsync(UI.ButtonLabel_Save_DPM_view_as_PNG, "png", "PNG");

        if(path is null) return;

        try
        {
            DpmPlot plot    = _window.Plot;
            double  scaling = _window.RenderScaling;

            var size = new PixelSize(Math.Max(1, (int)(plot.Bounds.Width  * scaling)),
                                     Math.Max(1, (int)(plot.Bounds.Height * scaling)));

            using var bitmap = new RenderTargetBitmap(size, new Vector(96 * scaling, 96 * scaling));

            bitmap.Render(plot);
            bitmap.Save(path, new PngBitmapEncoderOptions());
        }
        catch(Exception ex)
        {
            SentrySdk.CaptureException(ex);
            await ShowErrorAsync(UI.Error_writing_DPM_graph);
        }
    }

    /// <summary>Exports the bins and calibrations as CSV, one table after the other</summary>
    async Task ExportCsvAsync()
    {
        string path = await AskSavePathAsync(UI.ButtonLabel_Export_DPM_as_CSV, "csv", "CSV");

        if(path is null || Analysis is not {} analysis) return;

        try
        {
            await Task.Run(() => WriteCsv(analysis, path));
        }
        catch(Exception ex)
        {
            SentrySdk.CaptureException(ex);
            await ShowErrorAsync(UI.Error_writing_DPM_CSV);
        }
    }

    /// <summary>Writes the bins and calibrations as CSV, with invariant number formats</summary>
    static void WriteCsv(DpmAnalysisResult analysis, string path)
    {
        CultureInfo invariant = CultureInfo.InvariantCulture;

        using var writer = new StreamWriter(path);

        writer.WriteLine("first_lba,end_lba,layer,status,degrees_per_sector,sectors_per_turn,turn,fit,deviation");

        foreach(DpmBin bin in analysis.Bins)
        {
            writer.WriteLine(string.Format(invariant,
                                           "{0},{1},{2},{3},{4:R},{5:R},{6:R},{7:R},{8:R}",
                                           bin.FirstLba,
                                           bin.EndLba,
                                           bin.Layer,
                                           bin.Status,
                                           bin.Density,
                                           bin.SectorsPerTurn,
                                           bin.Turn,
                                           bin.Fit,
                                           bin.Deviation));
        }

        if(analysis.Calibrations.Length == 0) return;

        writer.WriteLine();
        writer.WriteLine("calibration_lba,layer,rpm,rotation_period_ms,sectors_per_turn");

        foreach(DpmCalibrationPoint calibration in analysis.Calibrations)
        {
            writer.WriteLine(string.Format(invariant,
                                           "{0},{1},{2:R},{3:R},{4:R}",
                                           calibration.Lba,
                                           calibration.Layer,
                                           calibration.Rpm,
                                           calibration.RotationPeriod,
                                           calibration.SectorsPerTurn));
        }
    }

    /// <summary>Asks for another image and compares its DPM with this one</summary>
    async Task CompareAsync()
    {
        IReadOnlyList<IStorageFile> result = await _window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title          = UI.Dialog_Choose_file_to_open,
            AllowMultiple  = false,
            FileTypeFilter = [FilePickerFileTypes.All]
        });

        if(result.Count != 1) return;

        string path = result[0].Path.LocalPath;

        (string error, DataPositionMeasurement? other) = await Task.Run(() => ReadOtherDpm(path));

        if(error != null)
        {
            await ShowErrorAsync(error);

            return;
        }

        DpmComparisonResult comparison = DpmComparison.Compare(Dpm, other.Value);

        ComparisonName = Path.GetFileName(path);
        Comparison     = comparison;

        FillComparison(comparison);
    }

    /// <summary>Opens an image and reads its DPM</summary>
    /// <returns>An error message, or the DPM</returns>
    static (string error, DataPositionMeasurement? dpm) ReadOtherDpm(string path)
    {
        try
        {
            IFilter filter = PluginRegister.Singleton.GetFilter(path);

            if(filter == null) return (UI.Cannot_open_specified_file, null);

            if(ImageFormat.Detect(filter) is not IMediaImage image) return (UI.Image_format_not_identified, null);

            ErrorNumber opened = image.Open(filter);

            if(opened != ErrorNumber.NoError) return (string.Format(UI.Error_0_opening_image_format, opened), null);

            if(image is not IOpticalMediaImage optical ||
               optical.ReadDpm(out DataPositionMeasurement dpm) != ErrorNumber.NoError)
                return (UI.Image_has_no_DPM, null);

            return (null, dpm);
        }
        catch(Exception ex)
        {
            SentrySdk.CaptureException(ex);

            return (ex.Message, null);
        }
    }

    void ClearComparison()
    {
        Comparison         = null;
        ComparisonName     = null;
        ComparisonVerdict  = null;
        SelectedDifference = null;
        ComparisonSummary.Clear();
        Differences.Clear();
    }

    /// <summary>Fills the comparison pane, as the comparison of images does in the command line</summary>
    void FillComparison(DpmComparisonResult comparison)
    {
        ComparisonSummary.Clear();
        Differences.Clear();

        void Add(string label, string value)
        {
            ComparisonSummary.Add(new DpmSummaryModel
            {
                Label = label,
                Value = value
            });
        }

        void Note(string text)
        {
            ComparisonSummary.Add(new DpmSummaryModel
            {
                Value = text
            });
        }

        Note(string.Format(UI.DPM_compared_with_0, ComparisonName));

        if(comparison.TimingUnitDiffers) Note(UI.DPM_timing_units_differ);

        if(comparison.TrackPathDiffers) Note(UI.DPM_track_paths_differ);

        if(comparison.LayersDiffer) Note(UI.DPM_layer_boundaries_differ);

        if(comparison.SpeedDiffers) Note(UI.DPM_speeds_differ);

        if(comparison.SpacingDiffers)
            Note(string.Format(UI.DPM_spacings_differ_compared_every_0_sectors, comparison.Spacing));

        ComparisonVerdict = comparison.Verdict switch
                            {
                                DpmComparisonVerdict.Match => UI.DPM_match_WithMarkup,
                                DpmComparisonVerdict.Differ => string.Format(UI.DPM_differ_in_0_regions_WithMarkup,
                                                                             comparison.Differences.Length),
                                _ => UI.DPM_not_comparable_WithMarkup
                            };

        if(comparison.Verdict == DpmComparisonVerdict.NotComparable) return;

        Add(UI.Title_DPM_compared_range,
            string.Format(UI.DPM_sectors_0_to_1_every_2, comparison.FirstLba, comparison.LastLba, comparison.Spacing));

        Add(UI.Title_DPM_bins, string.Format(UI.DPM_0_bins_1_not_measured, comparison.Bins, comparison.ExcludedBins));

        string[] scales = new string[comparison.Scales.Length];

        for(int i = 0; i < scales.Length; i++)
        {
            scales[i] = string.Format(UI.DPM_scale_0_1_ppm, comparison.Scales[i], (comparison.Scales[i] - 1.0) * 1e6);

            if(scales.Length > 1) scales[i] = string.Format(UI.DPM_layer_0_1, i, scales[i]);
        }

        Add(UI.Title_DPM_calibration_scale, string.Join(", ", scales));
        Add(UI.Title_DPM_noise,             string.Format(UI.DPM_noise_0_1, comparison.Noise1, comparison.Noise2));
        Add(UI.Title_DPM_threshold,         comparison.Threshold.ToString("P2"));

        Add(UI.Title_DPM_deviation,
            string.Format(UI.DPM_deviation_0_1_2,
                          comparison.MedianDeviation,
                          comparison.Percentile95Deviation,
                          comparison.MaximumDeviation));

        Add(UI.Title_DPM_isolated_outliers, comparison.IsolatedOutliers.ToString());

        foreach(DpmDifference difference in comparison.Differences)
        {
            Differences.Add(new DpmBandModel
            {
                FirstLba         = difference.FirstLba,
                LastLba          = difference.LastLba,
                Layer            = Aaru.CommonTypes.Structs.Dpm.LayerOf(Dpm, difference.FirstLba),
                MeanDeviation    = difference.MeanDeviation.ToString("+0.00%;-0.00%"),
                MaximumDeviation = difference.MaximumDeviation.ToString("+0.00%;-0.00%")
            });
        }
    }

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