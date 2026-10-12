// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmGraph.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Draws a graph of Data Position Measurement.
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
using System.Globalization;
using System.IO;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Image;
using SkiaSharp;

namespace Aaru.Core.Graphics;

/// <summary>Draws a Data Position Measurement as a PNG graph, in dark mode</summary>
/// <remarks>
///     The top panel shows the density, in degrees per sector, of every bin along the disc, where copy protection
///     density changes show as steps. Interpolated and unreadable bins are shaded, and layer boundaries are marked. The
///     bottom panel, sharing the sector axis, shows the rotation speed at every calibration.
/// </remarks>
public static class DpmGraph
{
    /// <summary>Default width of the graph, in pixels</summary>
    public const int DEFAULT_WIDTH = 1920;
    /// <summary>Default height of the graph, in pixels</summary>
    public const int DEFAULT_HEIGHT = 1080;
    /// <summary>Smallest width or height of the graph, in pixels</summary>
    public const int MIN_SIZE = 320;
    /// <summary>Largest width or height of the graph, in pixels</summary>
    public const int MAX_SIZE = 16384;
    /// <summary>Height everything is laid out in, scaled to the height of the graph</summary>
    const int   HEIGHT        = 900;
    const float MARGIN_LEFT   = 110;
    const float MARGIN_RIGHT  = 40;
    const float TITLE_HEIGHT  = 90;
    const float PANEL_GAP     = 70;
    const float MARGIN_BOTTOM = 70;

    static readonly SKColor Surface      = SKColor.Parse("#1a1a19");
    static readonly SKColor PrimaryInk   = SKColor.Parse("#ffffff");
    static readonly SKColor SecondaryInk = SKColor.Parse("#c3c2b7");
    static readonly SKColor MutedInk     = SKColor.Parse("#898781");
    static readonly SKColor Gridline     = SKColor.Parse("#2c2c2a");
    static readonly SKColor Series       = SKColor.Parse("#3987e5");
    static readonly SKColor SecondSeries = SKColor.Parse("#d95926");
    static readonly SKColor Serious      = SKColor.Parse("#ec835a");
    static readonly SKColor Critical     = SKColor.Parse("#d03b3b");

    /// <summary>Writes the graph of a DPM as a PNG</summary>
    /// <param name="dpm">DPM</param>
    /// <param name="path">Path to the PNG file</param>
    /// <param name="name">Name of the image, shown as the title</param>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels, text and lines are scaled with it</param>
    public static void Write(DataPositionMeasurement dpm, string path, string name, int width = DEFAULT_WIDTH,
                             int                     height = DEFAULT_HEIGHT)
    {
        using var bitmap = new SKBitmap(CheckSize(width), CheckSize(height));
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(Surface);

        float layoutWidth = Layout(canvas, width, height);

        DpmEntry[] entries = dpm.Entries;
        int        nb      = entries.Length - 1;
        var        density = new double[nb];
        double     minimum = double.MaxValue, maximum = double.MinValue;

        for(int j = 0; j < nb; j++)
        {
            density[j] = (entries[j + 1].Angle - entries[j].Angle) *
                         360.0              /
                         Dpm.UNITS_PER_TURN /
                         (entries[j + 1].Lba - entries[j].Lba);

            if(entries[j + 1].Status is DpmEntryStatus.Interpolated or DpmEntryStatus.Unreadable) continue;

            minimum = Math.Min(minimum, density[j]);
            maximum = Math.Max(maximum, density[j]);
        }

        if(minimum > maximum)
        {
            minimum = 0;
            maximum = 1;
        }

        double padding = Math.Max((maximum - minimum) * 0.05, maximum * 0.001);
        minimum -= padding;
        maximum += padding;

        double firstLba = entries[0].Lba;
        double lastLba  = entries[^1].Lba;

        bool  hasCalibrations = dpm.Calibrations is { Length: > 0 };
        float plotRight = layoutWidth - MARGIN_RIGHT;
        float plotBottom = HEIGHT - MARGIN_BOTTOM;
        float densityTop = TITLE_HEIGHT;
        float densityBottom = hasCalibrations ? densityTop + (plotBottom - densityTop - PANEL_GAP) * 0.72f : plotBottom;
        float rpmTop = densityBottom + PANEL_GAP;

        float X(double lba) =>
            (float)(MARGIN_LEFT + (lba - firstLba) / Math.Max(1, lastLba - firstLba) * (plotRight - MARGIN_LEFT));

        float Y(double value, double low, double high, float top, float bottom) =>
            (float)(bottom - (value - low) / (high - low) * (bottom - top));

        using var titleFont = new SKFont(SKTypeface.Default, 26);
        using var labelFont = new SKFont(SKTypeface.Default, 16);

        using var ink = new SKPaint();
        ink.IsAntialias = true;

        ink.Color = PrimaryInk;

        canvas.DrawText(string.IsNullOrWhiteSpace(name) ? "Data Position Measurement" : name,
                        MARGIN_LEFT,
                        40,
                        SKTextAlign.Left,
                        titleFont,
                        ink);

        ink.Color = SecondaryInk;

        canvas.DrawText(string.Format(CultureInfo.InvariantCulture,
                                      "Data Position Measurement: {0} entries every {1} sectors, {2:F2} turns. Degrees per sector by sector.",
                                      entries.Length,
                                      dpm.NominalSpacing,
                                      (double)entries[^1].Angle / Dpm.UNITS_PER_TURN),
                        MARGIN_LEFT,
                        68,
                        SKTextAlign.Left,
                        labelFont,
                        ink);

        // Shaded bins that were not measured, behind everything else
        using var shade = new SKPaint();

        for(int j = 0; j < nb; j++)
        {
            DpmEntryStatus status = entries[j + 1].Status;

            if(status is not (DpmEntryStatus.Interpolated or DpmEntryStatus.Unreadable)) continue;

            shade.Color = (status == DpmEntryStatus.Unreadable ? Critical : Serious).WithAlpha(64);

            float left  = X(entries[j].Lba);
            float right = Math.Max(X(entries[j + 1].Lba), left + 1);

            canvas.DrawRect(left, densityTop, right - left, densityBottom - densityTop, shade);
        }

        DrawGrid(canvas, labelFont, layoutWidth, minimum, maximum, densityTop, densityBottom, "F3", Y);

        // Layer boundaries
        using var layerPaint = new SKPaint();
        layerPaint.IsAntialias = true;
        layerPaint.Color       = MutedInk;
        layerPaint.StrokeWidth = 1;
        layerPaint.PathEffect  = SKPathEffect.CreateDash([6, 6], 0);

        for(int i = 0; i < (dpm.LayerEnds?.Length ?? 0); i++)
        {
            float x = X(dpm.LayerEnds[i] + 0.5);

            if(x < MARGIN_LEFT || x > plotRight) continue;

            canvas.DrawLine(x, densityTop, x, hasCalibrations ? plotBottom : densityBottom, layerPaint);
            ink.Color = SecondaryInk;
            canvas.DrawText($"Layer {i + 1}", x + 6, densityTop + 18, SKTextAlign.Left, labelFont, ink);
        }

        // Density, a 2px line through the centre of every bin
        using var line = new SKPaint();
        line.IsAntialias = true;
        line.Color       = Series;
        line.StrokeWidth = 2;
        line.Style       = SKPaintStyle.Stroke;
        line.StrokeJoin  = SKStrokeJoin.Round;

        using(var densityPath = new SKPath())
        {
            for(int j = 0; j < nb; j++)
            {
                float x = X((entries[j].Lba + entries[j + 1].Lba) / 2.0);
                float y = Y(density[j], minimum, maximum, densityTop, densityBottom);

                if(j == 0 ||
                   dpm.LayerEnds is { Length: > 0 } &&
                   Dpm.LayerOf(dpm, entries[j].Lba) != Dpm.LayerOf(dpm, entries[j - 1].Lba))
                    densityPath.MoveTo(x, y);
                else
                    densityPath.LineTo(x, y);
            }

            canvas.DrawPath(densityPath, line);
        }

        // Legend, only for the shaded statuses that are present
        float legendX = plotRight;
        ink.Color = SecondaryInk;

        foreach((DpmEntryStatus status, SKColor color, string label) in new[]
                {
                    (DpmEntryStatus.Unreadable, Critical, "Unreadable, interpolated"),
                    (DpmEntryStatus.Interpolated, Serious, "Not measured, interpolated")
                })
        {
            if(Array.FindIndex(entries, e => e.Status == status) < 0) continue;

            float textWidth = labelFont.MeasureText(label);
            legendX -= textWidth;
            canvas.DrawText(label, legendX, 68, SKTextAlign.Left, labelFont, ink);
            legendX     -= 22;
            shade.Color =  color.WithAlpha(128);
            canvas.DrawRoundRect(legendX, 55, 14, 14, 2, 2, shade);
            legendX -= 24;
        }

        if(hasCalibrations)
        {
            double rpmMin = double.MaxValue, rpmMax = double.MinValue;

            foreach(DpmCalibration calibration in dpm.Calibrations)
            {
                double rpm = 60e9 / calibration.RotationPeriod;
                rpmMin = Math.Min(rpmMin, rpm);
                rpmMax = Math.Max(rpmMax, rpm);
            }

            double rpmPadding = Math.Max((rpmMax - rpmMin) * 0.1, 50);
            rpmMin =  Math.Max(0, rpmMin - rpmPadding);
            rpmMax += rpmPadding;

            ink.Color = SecondaryInk;

            canvas.DrawText("Rotation speed at every calibration, RPM",
                            MARGIN_LEFT,
                            rpmTop - 14,
                            SKTextAlign.Left,
                            labelFont,
                            ink);

            DrawGrid(canvas, labelFont, layoutWidth, rpmMin, rpmMax, rpmTop, plotBottom, "F0", Y);

            using var dot = new SKPaint();
            dot.IsAntialias = true;
            dot.Color       = Series;

            using var ring = new SKPaint();
            ring.IsAntialias = true;
            ring.Color       = Surface;

            foreach(DpmCalibration calibration in dpm.Calibrations)
            {
                float x = X(calibration.Lba);
                float y = Y(60e9 / calibration.RotationPeriod, rpmMin, rpmMax, rpmTop, plotBottom);

                canvas.DrawCircle(x, y, 6, ring);
                canvas.DrawCircle(x, y, 4, dot);
            }
        }

        // Sector axis, shared by both panels
        ink.Color = MutedInk;

        foreach(double tick in NiceTicks(firstLba, lastLba, Math.Max(2, (int)(layoutWidth / 200))))
            canvas.DrawText(tick.ToString("N0", CultureInfo.InvariantCulture),
                            X(tick),
                            plotBottom + 24,
                            SKTextAlign.Center,
                            labelFont,
                            ink);

        canvas.DrawText("Sector", (MARGIN_LEFT + plotRight) / 2, plotBottom + 52, SKTextAlign.Center, labelFont, ink);

        using var    image  = SKImage.FromBitmap(bitmap);
        using SKData data   = image.Encode(SKEncodedImageFormat.Png, 100);
        using var    stream = new FileStream(path, FileMode.Create);
        data.SaveTo(stream);
    }

    /// <summary>Writes a graph comparing two DPMs as a PNG</summary>
    /// <remarks>
    ///     The top panel overlays the density of both DPMs on the comparison grid, the second one divided by the
    ///     calibration scale. The bottom panel shows the relative deviation of the second against the first, with the
    ///     difference threshold as a band. Regions that differ are shaded on both.
    /// </remarks>
    /// <param name="comparison">Comparison of both DPMs</param>
    /// <param name="path">Path to the PNG file</param>
    /// <param name="name1">Name of the first image</param>
    /// <param name="name2">Name of the second image</param>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels, text and lines are scaled with it</param>
    public static void WriteComparison(DpmComparisonResult comparison, string path, string name1, string name2,
                                       int                 width = DEFAULT_WIDTH, int height = DEFAULT_HEIGHT)
    {
        using var bitmap = new SKBitmap(CheckSize(width), CheckSize(height));
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(Surface);

        float layoutWidth = Layout(canvas, width, height);

        DpmComparisonBin[] bins    = comparison.Grid ?? [];
        double             minimum = double.MaxValue, maximum = double.MinValue;
        double             devMax  = comparison.Threshold * 2;

        foreach(DpmComparisonBin bin in bins)
        {
            if(!bin.Valid) continue;

            double scaled = bin.Density2 / comparison.Scales[bin.Segment];
            minimum = Math.Min(minimum, Math.Min(bin.Density1, scaled));
            maximum = Math.Max(maximum, Math.Max(bin.Density1, scaled));
            devMax  = Math.Max(devMax,  Math.Abs(bin.Deviation));
        }

        if(minimum > maximum)
        {
            minimum = 0;
            maximum = 1;
        }

        double padding = Math.Max((maximum - minimum) * 0.05, maximum * 0.001);
        minimum -= padding;
        maximum += padding;
        devMax  *= 1.1;

        double firstLba = comparison.FirstLba;
        double lastLba  = comparison.LastLba;

        float plotRight     = layoutWidth - MARGIN_RIGHT;
        float plotBottom    = HEIGHT - MARGIN_BOTTOM;
        float densityTop    = TITLE_HEIGHT;
        float densityBottom = densityTop    + (plotBottom - densityTop - PANEL_GAP) * 0.6f;
        float deviationTop  = densityBottom + PANEL_GAP;

        float X(double lba) =>
            (float)(MARGIN_LEFT + (lba - firstLba) / Math.Max(1, lastLba - firstLba) * (plotRight - MARGIN_LEFT));

        float Y(double value, double low, double high, float top, float bottom) =>
            (float)(bottom - (value - low) / (high - low) * (bottom - top));

        using var titleFont = new SKFont(SKTypeface.Default, 26);
        using var labelFont = new SKFont(SKTypeface.Default, 16);

        using var ink = new SKPaint();
        ink.IsAntialias = true;

        ink.Color = PrimaryInk;
        canvas.DrawText($"{name1} vs. {name2}", MARGIN_LEFT, 40, SKTextAlign.Left, titleFont, ink);

        string scales = string.Join(", ",
                                    Array.ConvertAll(comparison.Scales,
                                                     s => s.ToString("F6", CultureInfo.InvariantCulture)));

        ink.Color = SecondaryInk;

        canvas.DrawText(string.Format(CultureInfo.InvariantCulture,
                                      "Degrees per sector by sector, every {0} sectors. Second divided by calibration scale {1}. {2} differing regions.",
                                      comparison.Spacing,
                                      scales,
                                      comparison.Differences?.Length ?? 0),
                        MARGIN_LEFT,
                        68,
                        SKTextAlign.Left,
                        labelFont,
                        ink);

        // Differing regions and unmeasured bins, behind everything else
        using var shade = new SKPaint();

        shade.Color = MutedInk.WithAlpha(40);

        foreach(DpmComparisonBin bin in bins)
        {
            if(bin.Valid) continue;

            float left  = X(bin.FirstLba);
            float right = Math.Max(X(bin.EndLba), left + 1);
            canvas.DrawRect(left, densityTop,   right - left, densityBottom - densityTop,   shade);
            canvas.DrawRect(left, deviationTop, right - left, plotBottom    - deviationTop, shade);
        }

        shade.Color = Critical.WithAlpha(64);

        foreach(DpmDifference difference in comparison.Differences ?? [])
        {
            float left  = X(difference.FirstLba);
            float right = Math.Max(X(difference.LastLba + 1), left + 2);
            canvas.DrawRect(left, densityTop,   right - left, densityBottom - densityTop,   shade);
            canvas.DrawRect(left, deviationTop, right - left, plotBottom    - deviationTop, shade);
        }

        DrawGrid(canvas, labelFont, layoutWidth, minimum, maximum, densityTop, densityBottom, "F3", Y);

        using var line = new SKPaint();
        line.IsAntialias = true;
        line.StrokeWidth = 2;
        line.Style       = SKPaintStyle.Stroke;
        line.StrokeJoin  = SKStrokeJoin.Round;

        void DrawSeries(Func<DpmComparisonBin, double> value, double low, double high, float top, float bottom,
                        SKColor                        color)
        {
            using var series = new SKPath();
            bool      broken = true;

            for(int j = 0; j < bins.Length; j++)
            {
                // Break the line at unmeasured bins and segment changes
                if(!bins[j].Valid || j > 0 && bins[j].Segment != bins[j - 1].Segment)
                {
                    broken = true;

                    if(!bins[j].Valid) continue;
                }

                float x = X((bins[j].FirstLba + bins[j].EndLba) / 2.0);
                float y = Y(value(bins[j]), low, high, top, bottom);

                if(broken)
                    series.MoveTo(x, y);
                else
                    series.LineTo(x, y);

                broken = false;
            }

            line.Color = color;
            canvas.DrawPath(series, line);
        }

        DrawSeries(static b => b.Density1, minimum, maximum, densityTop, densityBottom, Series);

        DrawSeries(b => b.Density2 / comparison.Scales[b.Segment],
                   minimum,
                   maximum,
                   densityTop,
                   densityBottom,
                   SecondSeries);

        // Legend, the two series and the differing regions
        float legendX = plotRight;

        foreach((SKColor color, string label, bool region) in new[]
                {
                    (Critical, "Differs", true), (SecondSeries, "Second", false), (Series, "First", false)
                })
        {
            if(region && comparison.Differences is not { Length: > 0 }) continue;

            float textWidth = labelFont.MeasureText(label);
            legendX   -= textWidth;
            ink.Color =  SecondaryInk;
            canvas.DrawText(label, legendX, 68, SKTextAlign.Left, labelFont, ink);
            legendX     -= 22;
            shade.Color =  region ? color.WithAlpha(128) : color;
            canvas.DrawRoundRect(legendX, 55, 14, 14, 2, 2, shade);
            legendX -= 24;
        }

        // Deviation panel, with the threshold band
        ink.Color = SecondaryInk;

        canvas.DrawText(string.Format(CultureInfo.InvariantCulture,
                                      "Relative deviation of the second against the first, %. Threshold ±{0:F2}%.",
                                      comparison.Threshold * 100),
                        MARGIN_LEFT,
                        deviationTop - 14,
                        SKTextAlign.Left,
                        labelFont,
                        ink);

        shade.Color = Gridline.WithAlpha(160);

        float bandTop    = Y(comparison.Threshold  * 100, -devMax * 100, devMax * 100, deviationTop, plotBottom);
        float bandBottom = Y(-comparison.Threshold * 100, -devMax * 100, devMax * 100, deviationTop, plotBottom);
        canvas.DrawRect(MARGIN_LEFT, bandTop, plotRight - MARGIN_LEFT, bandBottom - bandTop, shade);

        DrawGrid(canvas, labelFont, layoutWidth, -devMax * 100, devMax * 100, deviationTop, plotBottom, "F1", Y);

        DrawSeries(static b => b.Deviation * 100, -devMax * 100, devMax * 100, deviationTop, plotBottom, Series);

        // Sector axis, shared by both panels
        ink.Color = MutedInk;

        foreach(double tick in NiceTicks(firstLba, lastLba, Math.Max(2, (int)(layoutWidth / 200))))
        {
            canvas.DrawText(tick.ToString("N0", CultureInfo.InvariantCulture),
                            X(tick),
                            plotBottom + 24,
                            SKTextAlign.Center,
                            labelFont,
                            ink);
        }

        canvas.DrawText("Sector", (MARGIN_LEFT + plotRight) / 2, plotBottom + 52, SKTextAlign.Center, labelFont, ink);

        using var    image  = SKImage.FromBitmap(bitmap);
        using SKData data   = image.Encode(SKEncodedImageFormat.Png, 100);
        using var    stream = new FileStream(path, FileMode.Create);
        data.SaveTo(stream);
    }

    /// <summary>Checks a size in pixels of the graph</summary>
    static int CheckSize(int size)
    {
        if(size is < MIN_SIZE or > MAX_SIZE) throw new ArgumentOutOfRangeException(nameof(size));

        return size;
    }

    /// <summary>Scales the canvas so everything is laid out in a height of <see cref="HEIGHT" /></summary>
    /// <returns>Width to lay out in</returns>
    static float Layout(SKCanvas canvas, int width, int height)
    {
        float scale = height / (float)HEIGHT;

        canvas.Scale(scale);

        return width / scale;
    }

    /// <summary>Draws hairline gridlines and value labels for a panel</summary>
    static void DrawGrid(SKCanvas canvas, SKFont font, float layoutWidth, double low, double high, float top,
                         float    bottom, string format, Func<double, double, double, float, float, float> y)
    {
        using var grid = new SKPaint();
        grid.Color       = Gridline;
        grid.StrokeWidth = 1;

        using var ink = new SKPaint();
        ink.IsAntialias = true;
        ink.Color       = MutedInk;

        foreach(double tick in NiceTicks(low, high, 5))
        {
            float position = y(tick, low, high, top, bottom);

            canvas.DrawLine(MARGIN_LEFT, position, layoutWidth - MARGIN_RIGHT, position, grid);

            canvas.DrawText(tick.ToString(format, CultureInfo.InvariantCulture),
                            MARGIN_LEFT - 10,
                            position    + 5,
                            SKTextAlign.Right,
                            font,
                            ink);
        }
    }

    /// <summary>Round tick values covering a range</summary>
    /// <param name="low">Start of the range</param>
    /// <param name="high">End of the range</param>
    /// <param name="count">Approximate number of ticks</param>
    /// <returns>Ticks at multiples of 1, 2 or 5 times a power of ten inside the range</returns>
    public static double[] NiceTicks(double low, double high, int count)
    {
        double range = high - low;

        if(range <= 0) return [low];

        double rough     = range / count;
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rough)));
        double residual  = rough / magnitude;

        double step = residual switch
                      {
                          > 5 => 10 * magnitude,
                          > 2 => 5  * magnitude,
                          > 1 => 2  * magnitude,
                          _   => magnitude
                      };

        double first = Math.Ceiling(low / step) * step;
        int    n     = (int)Math.Floor((high - first) / step) + 1;
        var    ticks = new double[Math.Max(n, 0)];

        for(int i = 0; i < ticks.Length; i++) ticks[i] = first + i * step;

        return ticks;
    }
}