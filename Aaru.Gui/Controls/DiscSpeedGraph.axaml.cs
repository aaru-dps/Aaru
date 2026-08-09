// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DiscSpeedGraph.axaml.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI custom controls.
//
// --[ Description ] ----------------------------------------------------------
//
//     A disc speed graph control to visualize read/write speeds over sectors.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Aaru.Gui.Controls;

/// <summary>
///     Disc speed graph control that visualizes read/write speeds over sectors,
///     similar to Nero DiscSpeed and ImgBurn speed graphs.
/// </summary>
public partial class DiscSpeedGraph : UserControl
{
    const int    MARGIN_LEFT   = 60;   // Space for Y-axis labels (KB/s)
    const int    MARGIN_RIGHT  = 60;   // Space for Y-axis labels (speed rating)
    const int    MARGIN_TOP    = 20;   // Top margin
    const int    MARGIN_BOTTOM = 30;   // Space for X-axis labels
    const double MIN_ZOOM      = 0.1;  // 10% minimum zoom (zoomed out)
    const double MAX_ZOOM      = 50.0; // 5000% maximum zoom (zoomed in) increased from 10.0
    const double ZOOM_STEP     = 0.2;  // 20% zoom step per click/scroll

    public static readonly StyledProperty<ObservableCollection<(ulong sector, double speedKbps)>> SpeedDataProperty =
        AvaloniaProperty
           .Register<DiscSpeedGraph, ObservableCollection<(ulong sector, double speedKbps)>>(nameof(SpeedData));

    public static readonly StyledProperty<ulong> MaxSectorProperty =
        AvaloniaProperty.Register<DiscSpeedGraph, ulong>(nameof(MaxSector));

    public static readonly StyledProperty<double> MaxSpeedProperty =
        AvaloniaProperty.Register<DiscSpeedGraph, double>(nameof(MaxSpeed));

    public static readonly StyledProperty<int> MultiplierProperty =
        AvaloniaProperty.Register<DiscSpeedGraph, int>(nameof(Multiplier), 1353);

    readonly Canvas _canvas;
    readonly List<Line> _gridLines = [];
    readonly List<TextBlock> _labels = [];
    readonly List<(ulong sector, double speedKbps)> _processedData = [];
    readonly Polyline _speedLine;
    readonly List<double> _speedWindow = new(30); // Window of recent non-spike speeds
    int _consecutiveSpikeCount; // Counter for consecutive spike attenuation
    ObservableCollection<(ulong sector, double speedKbps)> _speedData;

    double _yZoomLevel = 1.0; // 1.0 = 100% (no zoom), higher = zoomed in

    public DiscSpeedGraph()
    {
        InitializeComponent();
        _canvas = this.FindControl<Canvas>("GraphCanvas");

        // Create the speed line (aqua colored)
        _speedLine = new Polyline
        {
            Stroke          = new SolidColorBrush(Color.FromRgb(0, 255, 255)), // Aqua
            StrokeThickness = 2,
            ZIndex          = 1 // Above the grid lines
        };

        _canvas?.Children.Add(_speedLine);

        // Wire up scroll wheel for zoom
        if(_canvas != null) _canvas.PointerWheelChanged += OnPointerWheelChanged;

        // Keyboard shortcuts: + to zoom in, - to zoom out
        KeyDown += OnKeyDown;
    }

    public ObservableCollection<(ulong sector, double speedKbps)> SpeedData
    {
        get => GetValue(SpeedDataProperty);
        set => SetValue(SpeedDataProperty, value);
    }

    public ulong MaxSector
    {
        get => GetValue(MaxSectorProperty);
        set => SetValue(MaxSectorProperty, value);
    }

    public double MaxSpeed
    {
        get => GetValue(MaxSpeedProperty);
        set => SetValue(MaxSpeedProperty, value);
    }

    public int Multiplier
    {
        get => GetValue(MultiplierProperty);
        set => SetValue(MultiplierProperty, value);
    }

    void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if(change.Property == SpeedDataProperty)
        {
            if(_speedData != null) _speedData.CollectionChanged -= OnSpeedDataChanged;

            _speedData = change.GetNewValue<ObservableCollection<(ulong sector, double speedKbps)>>();

            if(_speedData == null) return;

            _speedData.CollectionChanged += OnSpeedDataChanged;
            _processedData.Clear();
            RedrawAll();
        }
        else if(change.Property == MaxSectorProperty  ||
                change.Property == MaxSpeedProperty   ||
                change.Property == MultiplierProperty ||
                change.Property == BoundsProperty)
            RedrawAll();
    }

    void OnSpeedDataChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if(e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            _processedData.AddRange(from (ulong sector, double speedKbps) item in e.NewItems
                                    select ProcessNewDataPoint(item));

            DrawNewSegment();
        }
        else
        {
            _processedData.Clear();
            _speedWindow.Clear();
            _consecutiveSpikeCount = 0;
            RedrawAll();
        }
    }

    void ZoomIn()
    {
        double newZoom = Math.Min(_yZoomLevel + ZOOM_STEP, MAX_ZOOM);

        if(!(Math.Abs(newZoom - _yZoomLevel) > 0.001)) return;

        _yZoomLevel = newZoom;
        RedrawAll();
    }

    void ZoomOut()
    {
        double newZoom = Math.Max(_yZoomLevel - ZOOM_STEP, MIN_ZOOM);

        if(!(Math.Abs(newZoom - _yZoomLevel) > 0.001)) return;

        _yZoomLevel = newZoom;
        RedrawAll();
    }

    void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        // Scroll up = zoom in, scroll down = zoom out
        if(e.Delta.Y > 0)
            ZoomIn();
        else if(e.Delta.Y < 0) ZoomOut();

        e.Handled = true;
    }

    void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch(e.Key)
        {
            case Key.OemPlus: // Usually needs Shift
            case Key.Add:     // Numpad +
                ZoomIn();
                e.Handled = true;

                break;
            case Key.OemMinus:
            case Key.Subtract: // Numpad -
                ZoomOut();
                e.Handled = true;

                break;
        }
    }

    (ulong sector, double speedKbps) ProcessNewDataPoint((ulong sector, double speedKbps) newPoint)
    {
        (ulong sector, double speed) = newPoint;

        // Skip zero/negative speeds
        if(speed <= 0) return newPoint;

        // Build initial window before spike detection
        if(_speedWindow.Count < 15)
        {
            _speedWindow.Add(speed);
            _consecutiveSpikeCount = 0;

            return newPoint;
        }

        // Get quartiles of the speed window
        var    sortedWindow = _speedWindow.Order().ToList();
        double q1           = sortedWindow[sortedWindow.Count     / 4];
        double q3           = sortedWindow[sortedWindow.Count * 3 / 4];
        double iqr          = q3 - q1;

        // Outlier threshold: Q3 + 1.5*IQR (standard statistical definition)
        double outlierThreshold = q3 + 1.5 * iqr;

        double processedSpeed = speed;

        // Only attenuate EXTREME outliers: must be BOTH above threshold AND more than 3x Q3
        if(speed > outlierThreshold && speed > q3 * 3.0)
        {
            _consecutiveSpikeCount++;

            // Only attenuate up to 5 consecutive spikes
            if(_consecutiveSpikeCount <= 5)
            {
                // Cap the spike to 0.95 * Q3 (gentle smoothing)
                processedSpeed = q3 * 0.95;
            }

            // After 5 consecutive spikes, assume it's a real speed region and stop attenuating
        }
        else
        {
            // Speed is normal or part of a sustained high-speed region
            _consecutiveSpikeCount = 0;
        }

        // Always add the processed speed to window
        _speedWindow.Add(processedSpeed);

        // Keep window size at 30 samples
        if(_speedWindow.Count > 30) _speedWindow.RemoveAt(0);

        return (sector, processedSpeed);
    }

    void RedrawAll()
    {
        if(_canvas == null || Bounds.Width <= 0 || Bounds.Height <= 0) return;

        // Clear everything
        ClearGraph();

        // Process all data
        if(_speedData?.Count > 0)
        {
            _processedData.Clear();
            _speedWindow.Clear();
            _consecutiveSpikeCount = 0;

            _processedData.AddRange(_speedData.Select(ProcessNewDataPoint));
        }

        // Draw background grid
        DrawGrid();

        // Draw speed line
        DrawSpeedLine();
    }

    void ClearGraph()
    {
        // Remove all grid lines and labels
        foreach(Line line in _gridLines) _canvas.Children.Remove(line);

        foreach(TextBlock label in _labels) _canvas.Children.Remove(label);

        _gridLines.Clear();
        _labels.Clear();

        // Clear the speed line
        _speedLine.Points.Clear();
    }

    void DrawGrid()
    {
        if(MaxSpeed <= 0 || MaxSector <= 0) return;

        double graphWidth  = Bounds.Width  - MARGIN_LEFT - MARGIN_RIGHT;
        double graphHeight = Bounds.Height - MARGIN_TOP  - MARGIN_BOTTOM;

        if(graphWidth <= 0 || graphHeight <= 0) return;

        var                       grayBrush  = new SolidColorBrush(Color.FromRgb(64, 64, 64));
        IImmutableSolidColorBrush whiteBrush = Brushes.White;

        // Draw vertical grid lines every 10%
        for(var i = 0; i <= 10; i++)
        {
            double x = MARGIN_LEFT + graphWidth * i / 10.0;

            var line = new Line
            {
                StartPoint      = new Point(x, MARGIN_TOP),
                EndPoint        = new Point(x, MARGIN_TOP + graphHeight),
                Stroke          = grayBrush,
                StrokeThickness = 1
            };

            _canvas.Children.Add(line);
            _gridLines.Add(line);

            // Add X-axis labels (sector numbers)
            if(MaxSector <= 0) continue;

            ulong sector = MaxSector * (ulong)i / 10ul;

            var sectorText = new TextBlock
            {
                Text       = sector.ToString("N0"),
                Foreground = whiteBrush,
                FontSize   = 10
            };

            _labels.Add(sectorText);
            _canvas.Children.Add(sectorText);

            // Measure text to center it
            sectorText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(sectorText, x         - sectorText.DesiredSize.Width / 2);
            Canvas.SetTop(sectorText, MARGIN_TOP + graphHeight + 5);
        }

        // Draw horizontal grid lines and labels
        const int numHorizontalLines = 10;

        for(var i = 0; i <= numHorizontalLines; i++)
        {
            double y = MARGIN_TOP + graphHeight * (1 - (double)i / numHorizontalLines);

            var line = new Line
            {
                StartPoint      = new Point(MARGIN_LEFT,              y),
                EndPoint        = new Point(MARGIN_LEFT + graphWidth, y),
                Stroke          = grayBrush,
                StrokeThickness = 1
            };

            _canvas.Children.Add(line);
            _gridLines.Add(line);
        }

        // Add Y-axis labels (KB/s on left, speed rating on right)
        for(var i = 0; i <= numHorizontalLines; i++)
        {
            double y     = MARGIN_TOP + graphHeight * (1 - (double)i / numHorizontalLines);
            double speed = MaxSpeed / _yZoomLevel   * i / numHorizontalLines;

            // Left side: KB/s
            var kbpsText = new TextBlock
            {
                Text       = speed.ToString("F0"),
                Foreground = whiteBrush,
                FontSize   = 10
            };

            _labels.Add(kbpsText);
            _canvas.Children.Add(kbpsText);
            kbpsText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(kbpsText, MARGIN_LEFT - kbpsText.DesiredSize.Width - 5);
            Canvas.SetTop(kbpsText, y            - kbpsText.DesiredSize.Height / 2);

            // Right side: Speed rating (e.g., 48x)
            if(Multiplier <= 0) continue;

            double speedRating = speed / Multiplier;

            var ratingText = new TextBlock
            {
                Text       = $"{speedRating:F1}x",
                Foreground = whiteBrush,
                FontSize   = 10
            };

            _labels.Add(ratingText);
            _canvas.Children.Add(ratingText);
            ratingText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(ratingText, MARGIN_LEFT + graphWidth + 5);
            Canvas.SetTop(ratingText, y            - ratingText.DesiredSize.Height / 2);
        }
    }

    void DrawSpeedLine()
    {
        if(_processedData.Count == 0 || MaxSpeed <= 0 || MaxSector <= 0) return;

        double graphWidth  = Bounds.Width  - MARGIN_LEFT - MARGIN_RIGHT;
        double graphHeight = Bounds.Height - MARGIN_TOP  - MARGIN_BOTTOM;

        if(graphWidth <= 0 || graphHeight <= 0) return;

        _speedLine.Points.Clear();

        double effectiveMaxSpeed = MaxSpeed / _yZoomLevel;

        foreach((ulong sector, double speedKbps) in _processedData)
        {
            double x = MARGIN_LEFT + graphWidth * sector / MaxSector;
            double y = MARGIN_TOP  + graphHeight         * (1 - speedKbps / effectiveMaxSpeed);

            // Clamp to graph bounds
            x = Math.Max(MARGIN_LEFT, Math.Min(MARGIN_LEFT + graphWidth,  x));
            y = Math.Max(MARGIN_TOP,  Math.Min(MARGIN_TOP  + graphHeight, y));

            _speedLine.Points.Add(new Point(x, y));
        }
    }

    void DrawNewSegment()
    {
        if(_processedData.Count == 0 || MaxSpeed <= 0 || MaxSector <= 0) return;

        double graphWidth  = Bounds.Width  - MARGIN_LEFT - MARGIN_RIGHT;
        double graphHeight = Bounds.Height - MARGIN_TOP  - MARGIN_BOTTOM;

        if(graphWidth <= 0 || graphHeight <= 0) return;

        double effectiveMaxSpeed = MaxSpeed / _yZoomLevel;

        // Only add the new point(s) to the polyline
        int startIndex = _speedLine.Points.Count;

        for(int i = startIndex; i < _processedData.Count; i++)
        {
            (ulong sector, double speedKbps) = _processedData[i];
            double x = MARGIN_LEFT + graphWidth * sector / MaxSector;
            double y = MARGIN_TOP  + graphHeight         * (1 - speedKbps / effectiveMaxSpeed);

            // Clamp to graph bounds
            x = Math.Max(MARGIN_LEFT, Math.Min(MARGIN_LEFT + graphWidth,  x));
            y = Math.Max(MARGIN_TOP,  Math.Min(MARGIN_TOP  + graphHeight, y));

            _speedLine.Points.Add(new Point(x, y));
        }
    }
}