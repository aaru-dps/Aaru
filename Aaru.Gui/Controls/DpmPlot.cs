// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmPlot.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI custom controls.
//
// --[ Description ] ----------------------------------------------------------
//
//     Draws a Data Position Measurement with independently zoomable axes.
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
using System.Globalization;
using System.Linq;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Graphics;
using Aaru.Core.Image;
using Aaru.Gui.ViewModels.Tabs;
using Aaru.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace Aaru.Gui.Controls;

/// <summary>Panels the DPM plot can show, stacked and sharing the sector axis</summary>
public enum DpmPlotPanel
{
    /// <summary>Density of every bin</summary>
    Density,
    /// <summary>Deviation of every bin from the spiral fitted to its layer</summary>
    Deviation,
    /// <summary>Rotation speed at every calibration</summary>
    Rpm,
    /// <summary>Status of every bin, as a strip of colours</summary>
    Status
}

/// <summary>Draws a Data Position Measurement, with independently zoomable axes</summary>
/// <remarks>
///     Every panel is redrawn from the data at the current ranges, so zooming changes the proportions of the axes and
///     their ticks instead of scaling an image. When there are more bins than pixels, every pixel column is drawn as
///     the span between its lowest and highest bin, so bin to bin changes are never averaged away.
/// </remarks>
public sealed class DpmPlot : Control
{
    const double MARGIN_LEFT   = 76;
    const double MARGIN_RIGHT  = 16;
    const double MARGIN_TOP    = 8;
    const double MARGIN_BOTTOM = 40;
    const double PANEL_GAP     = 14;
    const double FONT_SIZE     = 11;
    const double STRIP_HEIGHT  = 14;

    public static readonly StyledProperty<DpmAnalysisResult?> AnalysisProperty =
        AvaloniaProperty.Register<DpmPlot, DpmAnalysisResult?>(nameof(Analysis));

    static readonly Palette DarkPalette = new("#1a1a19",
                                              "#ffffff",
                                              "#c3c2b7",
                                              "#898781",
                                              "#2c2c2a",
                                              "#3987e5",
                                              "#d95926",
                                              "#ec835a",
                                              "#d03b3b");

    static readonly Palette LightPalette = new("#fcfcfb",
                                               "#1f1f1e",
                                               "#4d4c47",
                                               "#7a7974",
                                               "#e4e3df",
                                               "#2a6fc4",
                                               "#c24e1f",
                                               "#d9733f",
                                               "#c23030");

    readonly List<PanelState> _panels;
    Drag                      _drag;
    Point                     _dragCurrent;
    PanelState                _dragPanel;
    double                    _dragPanelHigh;
    double                    _dragPanelLow;
    Point                     _dragStart;
    double                    _dragViewFirst;
    double                    _dragViewLast;
    int                       _hoverBin = -1;
    Point?                    _hoverPoint;
    Rect                      _plot;
    double                    _viewFirst;
    double                    _viewLast;

    static DpmPlot()
    {
        AffectsRender<DpmPlot>(AnalysisProperty);
        FocusableProperty.OverrideDefaultValue<DpmPlot>(true);
    }

    public DpmPlot()
    {
        ClipToBounds = true;

        ActualThemeVariantChanged += (_, _) => InvalidateVisual();

        _panels =
        [
            new PanelState(DpmPlotPanel.Density,   3),
            new PanelState(DpmPlotPanel.Deviation, 2),
            new PanelState(DpmPlotPanel.Rpm,       1.2),
            new PanelState(DpmPlotPanel.Status,  0, STRIP_HEIGHT)
        ];
    }

    public DpmAnalysisResult? Analysis
    {
        get => GetValue(AnalysisProperty);
        set => SetValue(AnalysisProperty, value);
    }

    /// <summary>First sector in view</summary>
    public double ViewFirst => _viewFirst;

    /// <summary>Last sector in view</summary>
    public double ViewLast => _viewLast;

    /// <summary>Bin under the pointer, -1 if none</summary>
    public int HoveredBin => _hoverBin;

    /// <summary>Raised when the sectors in view change</summary>
    public event EventHandler ViewChanged;

    /// <summary>Raised when the bin under the pointer changes</summary>
    public event EventHandler HoveredBinChanged;

    /// <summary>Describes a bin, one line per value</summary>
    /// <param name="analysis">Analysis of the DPM</param>
    /// <param name="index">Bin</param>
    /// <param name="sector">Sector inside the bin</param>
    public static List<string> Describe(DpmAnalysisResult analysis, int index, double sector)
    {
        DpmBin bin = analysis.Bins[index];

        List<string> lines =
        [
            string.Format(UI.DPM_hover_sector_0, (ulong)Math.Max(0, sector)),
            string.Format(UI.DPM_hover_bin_0_1_layer_2, bin.FirstLba, bin.EndLba - 1, bin.Layer),
            DpmInfoViewModel.StatusName(bin.Status),
            string.Format(UI.DPM_hover_turn_0, bin.Turn),
            string.Format(UI.DPM_hover_density_0, bin.Density),
            string.Format(UI.DPM_hover_sectors_per_turn_0, bin.SectorsPerTurn)
        ];

        if(!double.IsNaN(bin.Deviation)) lines.Add(string.Format(UI.DPM_hover_deviation_0, bin.Deviation));

        if(NearestCalibration(analysis, sector) is {} calibration)
            lines.Add(string.Format(UI.DPM_hover_rpm_0_1, calibration.Rpm, calibration.Lba));

        return lines;
    }

    /// <summary>Calibration nearest to a sector</summary>
    static DpmCalibrationPoint? NearestCalibration(DpmAnalysisResult analysis, double sector)
    {
        DpmCalibrationPoint? nearest  = null;
        double               distance = double.MaxValue;

        foreach(DpmCalibrationPoint calibration in analysis.Calibrations)
        {
            double d = Math.Abs(calibration.Lba - sector);

            if(d >= distance) continue;

            distance = d;
            nearest  = calibration;
        }

        return nearest;
    }

    /// <summary>Bin that contains a sector, -1 if none</summary>
    static int BinAt(DpmAnalysisResult analysis, double sector)
    {
        DpmBin[] bins = analysis.Bins;
        int      low  = 0, high = bins.Length;

        while(low < high)
        {
            int middle = low + (high - low) / 2;

            if(bins[middle].EndLba <= sector)
                low = middle + 1;
            else
                high = middle;
        }

        return low < bins.Length && bins[low].FirstLba <= sector ? low : -1;
    }

    /// <summary>Updates the bin under the pointer</summary>
    void Hover(Point point)
    {
        if(!_plot.Contains(point) || Analysis is not {} analysis)
        {
            ClearHover();

            return;
        }

        _hoverPoint = point;
        InvalidateVisual();
        SetHoveredBin(BinAt(analysis, XToSector(point.X)));
    }

    /// <summary>Forgets the bin under the pointer</summary>
    void ClearHover()
    {
        _hoverPoint = null;
        InvalidateVisual();
        SetHoveredBin(-1);
    }

    void SetHoveredBin(int bin)
    {
        if(bin == _hoverBin) return;

        _hoverBin = bin;
        HoveredBinChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Sector under the pointer, NaN if none</summary>
    public double HoveredSector => _hoverPoint is {} point ? XToSector(point.X) : double.NaN;

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        ClearHover();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if(change.Property == AnalysisProperty) ResetView();
    }

    /// <summary>Shows the whole DPM, with every vertical range automatic</summary>
    public void ResetView()
    {
        if(Analysis is {} analysis) SetView(analysis.FirstLba, analysis.LastLba);

        FitValues();
    }

    /// <summary>Whether a panel is shown</summary>
    public bool IsPanelVisible(DpmPlotPanel kind)
    {
        return _panels.Find(p => p.Kind == kind)?.Visible == true;
    }

    /// <summary>Shows or hides a panel</summary>
    public void SetPanelVisible(DpmPlotPanel kind, bool visible)
    {
        if(_panels.Find(p => p.Kind == kind) is not {} panel || panel.Visible == visible) return;

        panel.Visible = visible;
        InvalidateVisual();
    }

    /// <summary>Whether a panel is drawn, it must be visible and have data</summary>
    bool Shown(PanelState panel)
    {
        return panel.Visible && (panel.Kind != DpmPlotPanel.Rpm || Analysis?.Calibrations.Length > 0);
    }

    /// <summary>Fits the vertical range of every panel to the bins in view</summary>
    public void FitValues()
    {
        foreach(PanelState panel in _panels) panel.AutoY = true;

        InvalidateVisual();
    }

    /// <summary>Shows a range of sectors, keeping it inside the DPM and at least a few bins wide</summary>
    /// <param name="first">First sector</param>
    /// <param name="last">Last sector</param>
    public void SetView(double first, double last)
    {
        if(Analysis is not {} analysis) return;

        double minimum = analysis.FirstLba;
        double maximum = analysis.LastLba;
        double width   = Math.Max(last - first, Math.Min(4.0 * analysis.NominalSpacing, maximum - minimum));

        width = Math.Min(width, maximum - minimum);

        double middle = (first + last) / 2;
        first = Math.Clamp(middle - width / 2, minimum, maximum - width);

        _viewFirst = first;
        _viewLast  = first + width;

        InvalidateVisual();
        ViewChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Zooms the sectors in view around a sector</summary>
    /// <param name="factor">Less than 1 to zoom in, more than 1 to zoom out</param>
    /// <param name="sector">Sector that stays in place</param>
    void ZoomSectors(double factor, double sector)
    {
        SetView(sector - (sector - _viewFirst) * factor, sector + (_viewLast - sector) * factor);
    }

    /// <summary>Moves the sectors in view</summary>
    /// <param name="fraction">Fraction of the sectors in view to move, positive to the right</param>
    void PanSectors(double fraction)
    {
        double offset = (_viewLast - _viewFirst) * fraction;

        SetView(_viewFirst + offset, _viewLast + offset);
    }

    /// <summary>Panel at a point, if any</summary>
    PanelState PanelAt(Point point)
    {
        foreach(PanelState panel in _panels)
            if(Shown(panel) && panel.Bounds.Contains(point))
                return panel;

        return null;
    }

    /// <summary>Sector at a horizontal position</summary>
    double XToSector(double x)
    {
        return _viewFirst + (x - _plot.Left) / _plot.Width * (_viewLast - _viewFirst);
    }

    /// <summary>Value at a vertical position of a panel</summary>
    static double YToValue(double y, PanelState panel)
    {
        return panel.Low + (panel.Bounds.Bottom - y) / panel.Bounds.Height * (panel.High - panel.Low);
    }

    /// <inheritdoc />
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if(Analysis is null) return;

        Point  point  = e.GetPosition(this);
        double factor = Math.Pow(1.25, -e.Delta.Y);

        if(e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            // Zoom the values of the panel under the pointer, keeping the value under it in place
            if(PanelAt(point) is not {} panel) return;

            double value = YToValue(point.Y, panel);

            panel.AutoY = false;
            panel.Low   = value - (value - panel.Low)  * factor;
            panel.High  = value + (panel.High - value) * factor;

            InvalidateVisual();
        }
        else if(e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            PanSectors(-(e.Delta.Y + e.Delta.X) * 0.1);
        else
            ZoomSectors(factor, XToSector(Math.Clamp(point.X, _plot.Left, _plot.Right)));

        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if(Analysis is null) return;

        Focus();

        Point                 point      = e.GetPosition(this);
        PointerPointProperties properties = e.GetCurrentPoint(this).Properties;

        if(properties.IsLeftButtonPressed && e.ClickCount == 2)
        {
            // Double click fits the values of a panel, or everything outside them
            if(PanelAt(point) is {} panel)
            {
                panel.AutoY = true;
                InvalidateVisual();
            }
            else
                ResetView();

            e.Handled = true;

            return;
        }

        if(properties.IsLeftButtonPressed)
            _drag = Drag.Pan;
        else if(properties.IsRightButtonPressed)
            _drag = Drag.Zoom;
        else
            return;

        _dragStart     = point;
        _dragCurrent   = point;
        _dragViewFirst = _viewFirst;
        _dragViewLast  = _viewLast;
        _dragPanel     = PanelAt(point);

        if(_dragPanel != null)
        {
            _dragPanelLow  = _dragPanel.Low;
            _dragPanelHigh = _dragPanel.High;
        }

        e.Pointer.Capture(this);
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if(_drag == Drag.None)
        {
            Hover(e.GetPosition(this));

            return;
        }

        _dragCurrent = e.GetPosition(this);

        if(_drag == Drag.Zoom)
        {
            InvalidateVisual();

            return;
        }

        double sectors = (_dragCurrent.X - _dragStart.X) / _plot.Width * (_dragViewLast - _dragViewFirst);

        SetView(_dragViewFirst - sectors, _dragViewLast - sectors);

        // Control also moves the values of the panel where the drag started
        if(!e.KeyModifiers.HasFlag(KeyModifiers.Control) || _dragPanel is null) return;

        double values = (_dragCurrent.Y - _dragStart.Y) / _dragPanel.Bounds.Height * (_dragPanelHigh - _dragPanelLow);

        _dragPanel.AutoY = false;
        _dragPanel.Low   = _dragPanelLow  + values;
        _dragPanel.High  = _dragPanelHigh + values;

        InvalidateVisual();
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if(_drag == Drag.None) return;

        Drag drag = _drag;
        _drag = Drag.None;
        e.Pointer.Capture(null);

        if(drag != Drag.Zoom) return;

        Rect band = new Rect(_dragStart, _dragCurrent).Normalize();

        // Zoom to the selected sectors, and to the selected values of the panel where it started
        if(band.Width >= 4)
            SetView(XToSector(Math.Max(band.Left, _plot.Left)), XToSector(Math.Min(band.Right, _plot.Right)));

        if(band.Height >= 4 && _dragPanel != null)
        {
            double top    = Math.Max(band.Top,    _dragPanel.Bounds.Top);
            double bottom = Math.Min(band.Bottom, _dragPanel.Bounds.Bottom);

            if(bottom > top)
            {
                double low  = YToValue(bottom, _dragPanel);
                double high = YToValue(top,    _dragPanel);

                _dragPanel.AutoY = false;
                _dragPanel.Low   = low;
                _dragPanel.High  = high;
            }
        }

        InvalidateVisual();
    }

    /// <inheritdoc />
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        if(_drag == Drag.None) return;

        _drag = Drag.None;
        InvalidateVisual();
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if(Analysis is null) return;

        double middle = (_viewFirst + _viewLast) / 2;

        switch(e.Key)
        {
            case Key.Home:
                ResetView();

                break;
            case Key.Add or Key.OemPlus:
                ZoomSectors(0.8, middle);

                break;
            case Key.Subtract or Key.OemMinus:
                ZoomSectors(1.25, middle);

                break;
            case Key.Left:
                PanSectors(-0.1);

                break;
            case Key.Right:
                PanSectors(0.1);

                break;
            default:
                return;
        }

        e.Handled = true;
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        Palette palette = ActualThemeVariant == ThemeVariant.Dark ? DarkPalette : LightPalette;

        context.FillRectangle(palette.Surface, new Rect(Bounds.Size));

        if(Analysis is not { Bins.Length: > 0 } analysis) return;

        if(_viewLast <= _viewFirst)
        {
            _viewFirst = analysis.FirstLba;
            _viewLast  = analysis.LastLba;
        }

        _plot = new Rect(MARGIN_LEFT,
                         MARGIN_TOP,
                         Math.Max(1, Bounds.Width  - MARGIN_LEFT - MARGIN_RIGHT),
                         Math.Max(1, Bounds.Height - MARGIN_TOP  - MARGIN_BOTTOM));

        Rect plot = _plot;

        LayoutPanels(plot);

        (int first, int end) = VisibleBins(analysis);

        foreach(PanelState panel in _panels)
        {
            if(!Shown(panel)) continue;

            DrawPanel(context, palette, analysis, panel, first, end);
        }

        DrawLayerBreaks(context, palette, analysis, plot);
        DrawLegend(context, palette, analysis);
        DrawSectorAxis(context, palette, plot);

        if(_drag == Drag.None && _hoverBin >= 0 && _hoverPoint is {} hover)
            DrawHover(context, palette, analysis, hover);

        if(_drag != Drag.Zoom) return;

        Rect band = new Rect(_dragStart, _dragCurrent).Normalize().Intersect(plot);

        context.DrawRectangle(palette.Selection, palette.SelectionPen, band);
    }

    /// <summary>Draws a line at the pointer, the value of the bin in every panel and a box describing it</summary>
    void DrawHover(DrawingContext context, Palette palette, DpmAnalysisResult analysis, Point hover)
    {
        context.DrawLine(palette.CrosshairPen, new Point(hover.X, _plot.Top), new Point(hover.X, _plot.Bottom));

        DpmBin bin = analysis.Bins[_hoverBin];
        double x   = SectorToX((bin.FirstLba + bin.EndLba) / 2.0, _plot);

        foreach(PanelState panel in _panels)
        {
            if(!Shown(panel)) continue;

            double value = Value(panel.Kind, bin);

            if(double.IsNaN(value) || double.IsInfinity(value)) continue;

            double y = ValueToY(value, panel.Low, panel.High, panel.Bounds);

            if(y < panel.Bounds.Top || y > panel.Bounds.Bottom) continue;

            context.DrawEllipse(palette.Surface, palette.SeriesPen, new Point(x, y), 4, 4);
        }

        List<FormattedText> lines = Describe(analysis, _hoverBin, XToSector(hover.X))
                                   .Select(line => Text(line, palette.PrimaryInk))
                                   .ToList();

        double width  = lines.Max(static l => l.Width)  + 16;
        double height = lines.Sum(static l => l.Height) + 12;
        double left   = hover.X + 16;
        double top    = hover.Y + 16;

        if(left + width > Bounds.Width) left = hover.X - 16 - width;

        if(top + height > Bounds.Height) top = Math.Max(0, hover.Y - 16 - height);

        context.DrawRectangle(palette.Overlay, palette.FramePen, new Rect(left, top, width, height), 4, 4);

        double lineTop = top + 6;

        foreach(FormattedText line in lines)
        {
            context.DrawText(line, new Point(left + 8, lineTop));
            lineTop += line.Height;
        }
    }

    /// <summary>Splits the plot area between the visible panels by their weight</summary>
    void LayoutPanels(Rect plot)
    {
        double weights = 0;
        double fixedHeight = 0;
        int    visible = 0;

        foreach(PanelState panel in _panels)
        {
            if(!Shown(panel)) continue;

            weights     += panel.Weight;
            fixedHeight += panel.FixedHeight;
            visible++;
        }

        if(visible == 0) return;

        double available = Math.Max(0, plot.Height - PANEL_GAP * (visible - 1) - fixedHeight);
        double top       = plot.Top;

        foreach(PanelState panel in _panels)
        {
            if(!Shown(panel)) continue;

            double height = panel.FixedHeight > 0
                                ? panel.FixedHeight
                                : Math.Max(1, weights > 0 ? available * panel.Weight / weights : 1);
            panel.Bounds =  new Rect(plot.Left, top, plot.Width, height);
            top          += height + PANEL_GAP;
        }
    }

    /// <summary>Gets the range of bins that overlap the visible sectors</summary>
    /// <returns>First bin and the one after the last</returns>
    (int first, int end) VisibleBins(DpmAnalysisResult analysis)
    {
        DpmBin[] bins = analysis.Bins;

        // First bin that ends after the start of the view
        int low = 0, high = bins.Length;

        while(low < high)
        {
            int middle = low + (high - low) / 2;

            if(bins[middle].EndLba <= _viewFirst)
                low = middle + 1;
            else
                high = middle;
        }

        int first = low;

        // First bin that starts at or after the end of the view
        high = bins.Length;

        while(low < high)
        {
            int middle = low + (high - low) / 2;

            if(bins[middle].FirstLba < _viewLast)
                low = middle + 1;
            else
                high = middle;
        }

        return (first, low);
    }

    /// <summary>Value a panel plots for a bin, NaN if it has none</summary>
    static double Value(DpmPlotPanel kind, DpmBin bin)
    {
        return kind switch
               {
                   DpmPlotPanel.Density   => bin.Density,
                   DpmPlotPanel.Deviation => bin.Deviation * 100,
                   _                      => double.NaN
               };
    }

    /// <summary>Title of a panel</summary>
    static string PanelTitle(DpmPlotPanel kind)
    {
        return kind switch
               {
                   DpmPlotPanel.Density   => UI.Title_DPM_degrees_per_sector,
                   DpmPlotPanel.Deviation => UI.Title_DPM_deviation_from_spiral,
                   DpmPlotPanel.Rpm       => UI.Title_DPM_rotation_speed,
                   _                      => ""
               };
    }

    /// <summary>Fits the vertical range of a panel to the measured bins in view</summary>
    static void AutoRange(PanelState panel, DpmAnalysisResult analysis, int first, int end)
    {
        double minimum = double.MaxValue, maximum = double.MinValue;

        for(int i = first; i < end; i++)
        {
            DpmBin bin = analysis.Bins[i];

            if(!bin.Measured) continue;

            double value = Value(panel.Kind, bin);

            if(double.IsNaN(value) || double.IsInfinity(value)) continue;

            minimum = Math.Min(minimum, value);
            maximum = Math.Max(maximum, value);
        }

        if(minimum > maximum)
        {
            minimum = 0;
            maximum = 1;
        }

        double padding = Math.Max((maximum - minimum) * 0.05, Math.Max(Math.Abs(maximum) * 0.001, 1e-9));

        panel.Low  = minimum - padding;
        panel.High = maximum + padding;
    }

    void DrawPanel(DrawingContext context, Palette palette, DpmAnalysisResult analysis, PanelState panel, int first,
                   int            end)
    {
        Rect bounds = panel.Bounds;

        if(panel.Kind == DpmPlotPanel.Status)
        {
            DrawStatusStrip(context, palette, analysis, bounds, first, end);

            return;
        }

        if(panel.AutoY)
        {
            if(panel.Kind == DpmPlotPanel.Rpm)
                AutoRangeRpm(panel, analysis);
            else
                AutoRange(panel, analysis, first, end);
        }

        using(context.PushClip(bounds)) DrawUnmeasured(context, palette, analysis, bounds, first, end);

        DrawValueAxis(context, palette, panel);

        using(context.PushClip(bounds))
        {
            switch(panel.Kind)
            {
                case DpmPlotPanel.Density:
                    DrawSeries(context,
                               analysis,
                               bounds,
                               first,
                               end,
                               static bin => bin.Fit,
                               panel.Low,
                               panel.High,
                               palette.FitPen);

                    break;
                case DpmPlotPanel.Deviation:
                    DrawBands(context, palette, analysis, panel);

                    break;
                case DpmPlotPanel.Rpm:
                    DrawCalibrations(context, palette, analysis, panel);

                    break;
            }

            DrawSeries(context,
                       analysis,
                       bounds,
                       first,
                       end,
                       bin => Value(panel.Kind, bin),
                       panel.Low,
                       panel.High,
                       palette.SeriesPen);
        }

        context.DrawRectangle(null, palette.FramePen, bounds);

        FormattedText title = Text(PanelTitle(panel.Kind), palette.SecondaryInk);
        context.DrawText(title, new Point(bounds.Left + 6, bounds.Top + 4));
    }

    /// <summary>Fits the vertical range of the rotation speed to the calibrations in view, or all of them</summary>
    void AutoRangeRpm(PanelState panel, DpmAnalysisResult analysis)
    {
        double minimum = double.MaxValue, maximum = double.MinValue;

        foreach(bool all in new[] { false, true })
        {
            foreach(DpmCalibrationPoint calibration in analysis.Calibrations)
            {
                if(!all && (calibration.Lba < _viewFirst || calibration.Lba > _viewLast)) continue;

                minimum = Math.Min(minimum, calibration.Rpm);
                maximum = Math.Max(maximum, calibration.Rpm);
            }

            if(minimum <= maximum) break;
        }

        if(minimum > maximum)
        {
            minimum = 0;
            maximum = 1;
        }

        double padding = Math.Max((maximum - minimum) * 0.1, 50);

        panel.Low  = Math.Max(0, minimum - padding);
        panel.High = maximum + padding;
    }

    /// <summary>Draws the rotation speed at every calibration as dots joined by a thin line</summary>
    void DrawCalibrations(DrawingContext context, Palette palette, DpmAnalysisResult analysis, PanelState panel)
    {
        Rect bounds   = panel.Bounds;
        var  geometry = new StreamGeometry();
        var  points   = new List<Point>();

        using(StreamGeometryContext path = geometry.Open())
        {
            foreach(DpmCalibrationPoint calibration in analysis.Calibrations)
            {
                var point = new Point(SectorToX(calibration.Lba, bounds),
                                      ValueToY(calibration.Rpm, panel.Low, panel.High, bounds));

                if(points.Count == 0)
                    path.BeginFigure(point, false);
                else
                    path.LineTo(point);

                points.Add(point);
            }

            if(points.Count > 0) path.EndFigure(false);
        }

        context.DrawGeometry(null, palette.CalibrationPen, geometry);

        foreach(Point point in points)
        {
            if(point.X < bounds.Left - 4 || point.X > bounds.Right + 4) continue;

            context.DrawEllipse(palette.Series, palette.SurfacePen, point, 4, 4);
        }
    }

    /// <summary>Draws the bands found by the analysis, the zero line and the band threshold of every layer</summary>
    void DrawBands(DrawingContext context, Palette palette, DpmAnalysisResult analysis, PanelState panel)
    {
        Rect bounds = panel.Bounds;

        foreach(DpmBand band in analysis.Bands)
        {
            if(band.LastLba + 1 < _viewFirst || band.FirstLba > _viewLast) continue;

            double left  = SectorToX(band.FirstLba,    bounds);
            double right = SectorToX(band.LastLba + 1, bounds);

            context.FillRectangle(palette.Band, new Rect(left, bounds.Top, Math.Max(right - left, 2), bounds.Height));
        }

        double zero = ValueToY(0, panel.Low, panel.High, bounds);
        context.DrawLine(palette.ZeroPen, new Point(bounds.Left, zero), new Point(bounds.Right, zero));

        foreach(DpmLayerSummary layer in analysis.Layers)
        {
            if(!layer.Fitted) continue;

            double left  = Math.Max(SectorToX(layer.FirstLba, bounds), bounds.Left);
            double right = Math.Min(SectorToX(layer.LastLba,  bounds), bounds.Right);

            if(right <= left) continue;

            foreach(double threshold in new[] { layer.BandThreshold * 100, -layer.BandThreshold * 100 })
            {
                double y = ValueToY(threshold, panel.Low, panel.High, bounds);

                context.DrawLine(palette.ThresholdPen, new Point(left, y), new Point(right, y));
            }
        }
    }

    /// <summary>Colour of a bin status in the strip, null for measured bins</summary>
    static Color? StatusColor(Palette palette, DpmEntryStatus status)
    {
        return status switch
               {
                   DpmEntryStatus.Remeasured   => palette.Series.Color,
                   DpmEntryStatus.Interpolated => palette.Serious,
                   DpmEntryStatus.Unreadable   => palette.Critical,
                   DpmEntryStatus.LayerBreak   => palette.MutedInk.Color,
                   _                           => null
               };
    }

    /// <summary>Calls an action for every run of bins with the same status, by horizontal position</summary>
    void StatusRuns(DpmAnalysisResult analysis, Rect bounds, int first, int end,
                    Action<DpmEntryStatus, double, double> run)
    {
        int i = first;

        while(i < end)
        {
            DpmEntryStatus status = analysis.Bins[i].Status;
            int            j      = i + 1;

            while(j < end && analysis.Bins[j].Status == status) j++;

            double left  = SectorToX(analysis.Bins[i].FirstLba, bounds);
            double right = SectorToX(analysis.Bins[j - 1].EndLba, bounds);

            run(status, left, Math.Max(right, left + 1));

            i = j;
        }
    }

    /// <summary>Draws the status of every bin as a strip of colours, measured bins left blank</summary>
    void DrawStatusStrip(DrawingContext context, Palette palette, DpmAnalysisResult analysis, Rect bounds, int first,
                         int            end)
    {
        context.FillRectangle(palette.Strip, bounds);

        using(context.PushClip(bounds))
        {
            StatusRuns(analysis,
                       bounds,
                       first,
                       end,
                       (status, left, right) =>
                       {
                           if(StatusColor(palette, status) is not {} color) return;

                           context.FillRectangle(new ImmutableSolidColorBrush(color),
                                                 new Rect(left, bounds.Top, right - left, bounds.Height));
                       });
        }

        context.DrawRectangle(null, palette.FramePen, bounds);

        FormattedText title = Text(UI.Title_DPM_status, palette.MutedInk);
        context.DrawText(title, new Point(bounds.Left - 6 - title.Width, bounds.Center.Y - title.Height / 2));
    }

    /// <summary>Shades the bins that were not measured, as their values are interpolated</summary>
    void DrawUnmeasured(DrawingContext context, Palette palette, DpmAnalysisResult analysis, Rect bounds, int first,
                        int            end)
    {
        StatusRuns(analysis,
                   bounds,
                   first,
                   end,
                   (status, left, right) =>
                   {
                       if(status is not (DpmEntryStatus.Interpolated or DpmEntryStatus.Unreadable)) return;

                       Color color = status == DpmEntryStatus.Unreadable ? palette.Critical : palette.Serious;

                       context.FillRectangle(new ImmutableSolidColorBrush(color, 0.25),
                                             new Rect(left, bounds.Top, right - left, bounds.Height));
                   });
    }

    /// <summary>Draws the legend of the statuses in the DPM, over the top right of the first panel</summary>
    void DrawLegend(DrawingContext context, Palette palette, DpmAnalysisResult analysis)
    {
        PanelState panel = _panels.Find(Shown);

        if(panel is null) return;

        double right = panel.Bounds.Right - 8;
        double top   = panel.Bounds.Top   + 4;

        foreach(DpmEntryStatus status in new[]
                {
                    DpmEntryStatus.LayerBreak, DpmEntryStatus.Remeasured, DpmEntryStatus.Interpolated,
                    DpmEntryStatus.Unreadable
                })
        {
            if(analysis.StatusCounts[(int)status] == 0 || StatusColor(palette, status) is not {} color) continue;

            FormattedText label = Text(DpmInfoViewModel.StatusName(status), palette.SecondaryInk);

            right -= label.Width;
            context.DrawText(label, new Point(right, top));
            right -= 16;
            context.FillRectangle(new ImmutableSolidColorBrush(color),
                                  new Rect(right, top + (label.Height - 10) / 2, 10, 10),
                                  2);
            right -= 14;
        }
    }

    /// <summary>Draws the horizontal grid lines and labels of a panel</summary>
    static void DrawValueAxis(DrawingContext context, Palette palette, PanelState panel)
    {
        Rect     bounds = panel.Bounds;
        double[] ticks  = DpmGraph.NiceTicks(panel.Low, panel.High, Math.Max(2, (int)(bounds.Height / 36)));

        if(ticks.Length == 0) return;

        string format = TickFormat(ticks.Length > 1 ? ticks[1] - ticks[0] : Math.Abs(ticks[0]));

        foreach(double tick in ticks)
        {
            double y = ValueToY(tick, panel.Low, panel.High, bounds);

            context.DrawLine(palette.GridPen, new Point(bounds.Left, y), new Point(bounds.Right, y));

            FormattedText label = Text(tick.ToString(format, CultureInfo.CurrentCulture), palette.MutedInk);
            context.DrawText(label, new Point(bounds.Left - 6 - label.Width, y - label.Height / 2));
        }
    }

    /// <summary>Draws the dashed lines where layers end</summary>
    void DrawLayerBreaks(DrawingContext context, Palette palette, DpmAnalysisResult analysis, Rect plot)
    {
        for(int i = 0; i < analysis.LayerEnds.Length; i++)
        {
            double x = SectorToX(analysis.LayerEnds[i] + 1, plot);

            if(x < plot.Left || x > plot.Right) continue;

            context.DrawLine(palette.LayerPen, new Point(x, plot.Top), new Point(x, plot.Bottom));

            FormattedText label = Text(string.Format(UI.DPM_Layer_0, i + 1), palette.SecondaryInk);
            context.DrawText(label, new Point(x + 4, plot.Top + 6 + label.Height));
        }
    }

    /// <summary>Draws the sector ticks under the plot</summary>
    void DrawSectorAxis(DrawingContext context, Palette palette, Rect plot)
    {
        double[] ticks = DpmGraph.NiceTicks(_viewFirst, _viewLast, Math.Max(2, (int)(plot.Width / 110)));

        foreach(double tick in ticks)
        {
            double x = SectorToX(tick, plot);

            context.DrawLine(palette.GridPen, new Point(x, plot.Bottom), new Point(x, plot.Bottom + 4));

            FormattedText label = Text(tick.ToString("N0", CultureInfo.CurrentCulture), palette.MutedInk);
            context.DrawText(label,
                             new Point(Math.Clamp(x - label.Width / 2, 0, Math.Max(0, Bounds.Width - label.Width)),
                                       plot.Bottom + 6));
        }

        FormattedText title = Text(UI.Title_DPM_sector, palette.SecondaryInk);
        context.DrawText(title, new Point(plot.Center.X - title.Width / 2, plot.Bottom + 22));
    }

    /// <summary>Draws a value of every bin as a line through the middle of the bins</summary>
    /// <remarks>
    ///     The line breaks at layer changes and at bins without value. With more bins than pixels every pixel column is
    ///     drawn from its lowest to its highest value.
    /// </remarks>
    void DrawSeries(DrawingContext     context, DpmAnalysisResult analysis, Rect bounds, int first, int end,
                    Func<DpmBin, double> value, double low, double high, IPen pen)
    {
        if(end <= first) return;

        bool dense = end - first > bounds.Width;

        var geometry = new StreamGeometry();

        using(StreamGeometryContext path = geometry.Open())
        {
            bool   open      = false;
            int    layer     = -1;
            int    column    = int.MinValue;
            double minimum   = 0, maximum = 0;
            double lastY     = 0;

            void Point(double x, double y)
            {
                var point = new Point(x, y);

                if(open)
                    path.LineTo(point);
                else
                {
                    path.BeginFigure(point, false);
                    open = true;
                }

                lastY = y;
            }

            void Flush()
            {
                if(column == int.MinValue) return;

                double top    = ValueToY(maximum, low, high, bounds);
                double bottom = ValueToY(minimum, low, high, bounds);

                // Start on the end nearer to where the line comes from, so columns join without crossing
                if(open && Math.Abs(lastY - bottom) < Math.Abs(lastY - top))
                {
                    Point(column + 0.5, bottom);
                    Point(column + 0.5, top);
                }
                else
                {
                    Point(column + 0.5, top);
                    Point(column + 0.5, bottom);
                }

                column = int.MinValue;
            }

            void Break()
            {
                Flush();

                if(open) path.EndFigure(false);

                open = false;
            }

            for(int i = first; i < end; i++)
            {
                DpmBin bin = analysis.Bins[i];
                double v   = value(bin);

                if(double.IsNaN(v) || double.IsInfinity(v))
                {
                    Break();

                    continue;
                }

                if(bin.Layer != layer)
                {
                    Break();
                    layer = bin.Layer;
                }

                double x = SectorToX((bin.FirstLba + bin.EndLba) / 2.0, bounds);

                if(!dense)
                {
                    Point(x, ValueToY(v, low, high, bounds));

                    continue;
                }

                int c = (int)Math.Floor(x);

                if(c != column)
                {
                    Flush();
                    column  = c;
                    minimum = v;
                    maximum = v;

                    continue;
                }

                minimum = Math.Min(minimum, v);
                maximum = Math.Max(maximum, v);
            }

            Break();
        }

        context.DrawGeometry(null, pen, geometry);
    }

    /// <summary>Horizontal position of a sector</summary>
    double SectorToX(double sector, Rect bounds)
    {
        return bounds.Left + (sector - _viewFirst) / (_viewLast - _viewFirst) * bounds.Width;
    }

    /// <summary>Vertical position of a value in a panel</summary>
    static double ValueToY(double value, double low, double high, Rect bounds)
    {
        return bounds.Bottom - (value - low) / (high - low) * bounds.Height;
    }

    /// <summary>Format with enough decimals to tell apart ticks a step apart</summary>
    static string TickFormat(double step)
    {
        if(step <= 0 || double.IsNaN(step)) return "G4";

        int decimals = Math.Clamp((int)Math.Ceiling(-Math.Log10(step)), 0, 9);

        return "N" + decimals.ToString(CultureInfo.InvariantCulture);
    }

    static FormattedText Text(string text, IBrush brush)
    {
        return new FormattedText(text,
                                 CultureInfo.CurrentCulture,
                                 FlowDirection.LeftToRight,
                                 Typeface.Default,
                                 FONT_SIZE,
                                 brush);
    }

    /// <summary>What dragging the pointer does</summary>
    enum Drag
    {
        None,
        Pan,
        Zoom
    }

    /// <summary>State of a panel</summary>
    /// <param name="kind">Panel</param>
    /// <param name="weight">Share of the height left by the fixed height panels</param>
    /// <param name="fixedHeight">Fixed height, 0 to use the weight</param>
    sealed class PanelState(DpmPlotPanel kind, double weight, double fixedHeight = 0)
    {
        public DpmPlotPanel Kind        { get; }      = kind;
        public double       Weight      { get; }      = weight;
        public double       FixedHeight { get; }      = fixedHeight;
        public bool         Visible     { get; set; } = true;
        public bool         AutoY       { get; set; } = true;
        public double       Low         { get; set; }
        public double       High        { get; set; }
        public Rect         Bounds      { get; set; }
    }

    /// <summary>Brushes and pens of a theme</summary>
    sealed class Palette
    {
        public Palette(string surface, string primary, string secondary, string muted, string grid, string series,
                       string secondSeries, string serious, string critical)
        {
            Surface      = new ImmutableSolidColorBrush(Color.Parse(surface));
            PrimaryInk   = new ImmutableSolidColorBrush(Color.Parse(primary));
            SecondaryInk = new ImmutableSolidColorBrush(Color.Parse(secondary));
            MutedInk     = new ImmutableSolidColorBrush(Color.Parse(muted));
            Series       = new ImmutableSolidColorBrush(Color.Parse(series));
            SecondSeries = new ImmutableSolidColorBrush(Color.Parse(secondSeries));
            Serious      = Color.Parse(serious);
            Critical     = Color.Parse(critical);
            GridPen      = new ImmutablePen(new ImmutableSolidColorBrush(Color.Parse(grid)));
            FramePen     = new ImmutablePen(new ImmutableSolidColorBrush(Color.Parse(grid)), 1.5);
            SeriesPen    = new ImmutablePen(Series, 1.5, null, PenLineCap.Round, PenLineJoin.Round);
            LayerPen     = new ImmutablePen(MutedInk, 1, new ImmutableDashStyle([6, 6], 0));
            Selection    = new ImmutableSolidColorBrush(Color.Parse(series), 0.15);
            SelectionPen = new ImmutablePen(Series);
            CrosshairPen = new ImmutablePen(MutedInk, 1, new ImmutableDashStyle([2, 3], 0));
            Overlay      = new ImmutableSolidColorBrush(Color.Parse(surface), 0.92);
            Strip        = new ImmutableSolidColorBrush(Color.Parse(grid), 0.5);
            Band         = new ImmutableSolidColorBrush(Color.Parse(secondSeries), 0.18);
            FitPen       = new ImmutablePen(MutedInk, 1, new ImmutableDashStyle([4, 4], 0));
            ZeroPen      = new ImmutablePen(MutedInk);
            ThresholdPen   = new ImmutablePen(SecondSeries, 1, new ImmutableDashStyle([4, 4], 0));
            CalibrationPen = new ImmutablePen(new ImmutableSolidColorBrush(Color.Parse(series), 0.5));
            SurfacePen     = new ImmutablePen(Surface, 2);
        }

        public ImmutableSolidColorBrush Surface      { get; }
        public ImmutableSolidColorBrush PrimaryInk   { get; }
        public ImmutableSolidColorBrush SecondaryInk { get; }
        public ImmutableSolidColorBrush MutedInk     { get; }
        public ImmutableSolidColorBrush Series       { get; }
        public ImmutableSolidColorBrush SecondSeries { get; }
        public Color                    Serious      { get; }
        public Color                    Critical     { get; }
        public IPen                     GridPen      { get; }
        public IPen                     FramePen     { get; }
        public IPen                     SeriesPen    { get; }
        public IPen                     LayerPen     { get; }
        public ImmutableSolidColorBrush Selection    { get; }
        public IPen                     SelectionPen { get; }
        public IPen                     CrosshairPen { get; }
        public ImmutableSolidColorBrush Overlay      { get; }
        public ImmutableSolidColorBrush Strip        { get; }
        public ImmutableSolidColorBrush Band         { get; }
        public IPen                     FitPen       { get; }
        public IPen                     ZeroPen      { get; }
        public IPen                     ThresholdPen   { get; }
        public IPen                     CalibrationPen { get; }
        public IPen                     SurfacePen     { get; }
    }
}