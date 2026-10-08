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
    const int   WIDTH         = 1600;
    const int   HEIGHT        = 900;
    const float MARGIN_LEFT   = 110;
    const float MARGIN_RIGHT  = 40;
    const float TITLE_HEIGHT  = 90;
    const float PANEL_GAP     = 70;
    const float MARGIN_BOTTOM = 70;

    static readonly SKColor Surface       = SKColor.Parse("#1a1a19");
    static readonly SKColor PrimaryInk    = SKColor.Parse("#ffffff");
    static readonly SKColor SecondaryInk  = SKColor.Parse("#c3c2b7");
    static readonly SKColor MutedInk      = SKColor.Parse("#898781");
    static readonly SKColor Gridline      = SKColor.Parse("#2c2c2a");
    static readonly SKColor Series        = SKColor.Parse("#3987e5");
    static readonly SKColor Serious       = SKColor.Parse("#ec835a");
    static readonly SKColor Critical      = SKColor.Parse("#d03b3b");

    /// <summary>Writes the graph of a DPM as a PNG</summary>
    /// <param name="dpm">DPM</param>
    /// <param name="path">Path to the PNG file</param>
    /// <param name="name">Name of the image, shown as the title</param>
    public static void Write(DataPositionMeasurement dpm, string path, string name)
    {
        using var bitmap = new SKBitmap(WIDTH, HEIGHT);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(Surface);

        DpmEntry[] entries = dpm.Entries;
        int        nb      = entries.Length - 1;
        var        density = new double[nb];
        double     minimum = double.MaxValue, maximum = double.MinValue;

        for(int j = 0; j < nb; j++)
        {
            density[j] = (entries[j + 1].Angle - entries[j].Angle) *
                         360.0                                       /
                         Dpm.UNITS_PER_TURN                          /
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
        float plotRight       = WIDTH - MARGIN_RIGHT;
        float plotBottom      = HEIGHT - MARGIN_BOTTOM;
        float densityTop      = TITLE_HEIGHT;
        float densityBottom   = hasCalibrations ? densityTop + (plotBottom - densityTop - PANEL_GAP) * 0.72f : plotBottom;
        float rpmTop          = densityBottom + PANEL_GAP;

        float X(double lba) => (float)(MARGIN_LEFT + (lba - firstLba) / Math.Max(1, lastLba - firstLba) *
                                       (plotRight - MARGIN_LEFT));

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

        DrawGrid(canvas, labelFont, minimum, maximum, densityTop, densityBottom, "F3", Y);

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

                if(j == 0 || dpm.LayerEnds is { Length: > 0 } && Dpm.LayerOf(dpm, entries[j].Lba) !=
                   Dpm.LayerOf(dpm, entries[j - 1].Lba))
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
            legendX -= 22;
            shade.Color = color.WithAlpha(128);
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
            rpmMin = Math.Max(0, rpmMin - rpmPadding);
            rpmMax += rpmPadding;

            ink.Color = SecondaryInk;
            canvas.DrawText("Rotation speed at every calibration, RPM", MARGIN_LEFT, rpmTop - 14, SKTextAlign.Left,
                            labelFont, ink);

            DrawGrid(canvas, labelFont, rpmMin, rpmMax, rpmTop, plotBottom, "F0", Y);

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

        foreach(double tick in NiceTicks(firstLba, lastLba, 8))
            canvas.DrawText(tick.ToString("N0", CultureInfo.InvariantCulture), X(tick), plotBottom + 24,
                            SKTextAlign.Center, labelFont, ink);

        canvas.DrawText("Sector", (MARGIN_LEFT + plotRight) / 2, plotBottom + 52, SKTextAlign.Center, labelFont, ink);

        using var image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new FileStream(path, FileMode.Create);
        data.SaveTo(stream);
    }

    /// <summary>Draws hairline gridlines and value labels for a panel</summary>
    static void DrawGrid(SKCanvas canvas, SKFont font, double low, double high, float top, float bottom,
                         string format, Func<double, double, double, float, float, float> y)
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

            canvas.DrawLine(MARGIN_LEFT, position, WIDTH - MARGIN_RIGHT, position, grid);

            canvas.DrawText(tick.ToString(format, CultureInfo.InvariantCulture), MARGIN_LEFT - 10, position + 5,
                            SKTextAlign.Right, font, ink);
        }
    }

    /// <summary>Round tick values covering a range</summary>
    static double[] NiceTicks(double low, double high, int count)
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