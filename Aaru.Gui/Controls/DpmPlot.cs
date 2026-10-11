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
using Aaru.Core.Graphics;
using Aaru.Core.Image;
using Aaru.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace Aaru.Gui.Controls;

/// <summary>Panels the DPM plot can show, stacked and sharing the sector axis</summary>
public enum DpmPlotPanel
{
    /// <summary>Density of every bin</summary>
    Density
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
    double                    _viewFirst;
    double                    _viewLast;

    static DpmPlot()
    {
        AffectsRender<DpmPlot>(AnalysisProperty);
    }

    public DpmPlot()
    {
        ClipToBounds = true;

        ActualThemeVariantChanged += (_, _) => InvalidateVisual();

        _panels =
        [
            new PanelState(DpmPlotPanel.Density, 3)
        ];
    }

    public DpmAnalysisResult? Analysis
    {
        get => GetValue(AnalysisProperty);
        set => SetValue(AnalysisProperty, value);
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
        if(Analysis is {} analysis)
        {
            _viewFirst = analysis.FirstLba;
            _viewLast  = analysis.LastLba;
        }

        foreach(PanelState panel in _panels) panel.AutoY = true;

        InvalidateVisual();
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        Palette palette = ActualThemeVariant == ThemeVariant.Dark ? DarkPalette : LightPalette;

        context.FillRectangle(palette.Surface, new Rect(Bounds.Size));

        if(Analysis is not { Bins.Length: > 0 } analysis) return;

        if(_viewLast <= _viewFirst) ResetView();

        Rect plot = new Rect(MARGIN_LEFT,
                             MARGIN_TOP,
                             Math.Max(1, Bounds.Width  - MARGIN_LEFT - MARGIN_RIGHT),
                             Math.Max(1, Bounds.Height - MARGIN_TOP  - MARGIN_BOTTOM));

        LayoutPanels(plot);

        (int first, int end) = VisibleBins(analysis);

        foreach(PanelState panel in _panels)
        {
            if(!panel.Visible) continue;

            DrawPanel(context, palette, analysis, panel, first, end);
        }

        DrawLayerBreaks(context, palette, analysis, plot);
        DrawSectorAxis(context, palette, plot);
    }

    /// <summary>Splits the plot area between the visible panels by their weight</summary>
    void LayoutPanels(Rect plot)
    {
        double weights = 0;
        int    visible = 0;

        foreach(PanelState panel in _panels)
        {
            if(!panel.Visible) continue;

            weights += panel.Weight;
            visible++;
        }

        if(visible == 0) return;

        double available = plot.Height - PANEL_GAP * (visible - 1);
        double top       = plot.Top;

        foreach(PanelState panel in _panels)
        {
            if(!panel.Visible) continue;

            double height = Math.Max(1, available * panel.Weight / weights);
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
                   DpmPlotPanel.Density => bin.Density,
                   _                    => double.NaN
               };
    }

    /// <summary>Title of a panel</summary>
    static string PanelTitle(DpmPlotPanel kind)
    {
        return kind switch
               {
                   DpmPlotPanel.Density => UI.Title_DPM_degrees_per_sector,
                   _                    => ""
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
        if(panel.AutoY) AutoRange(panel, analysis, first, end);

        Rect bounds = panel.Bounds;

        DrawValueAxis(context, palette, panel);

        using(context.PushClip(bounds))
        {
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
            context.DrawText(label, new Point(x + 4, plot.Top + 4));
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
            context.DrawText(label, new Point(x - label.Width / 2, plot.Bottom + 6));
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

    /// <summary>State of a panel</summary>
    sealed class PanelState(DpmPlotPanel kind, double weight)
    {
        public DpmPlotPanel Kind    { get; }      = kind;
        public double       Weight  { get; }      = weight;
        public bool         Visible { get; set; } = true;
        public bool         AutoY   { get; set; } = true;
        public double       Low     { get; set; }
        public double       High    { get; set; }
        public Rect         Bounds  { get; set; }
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
    }
}