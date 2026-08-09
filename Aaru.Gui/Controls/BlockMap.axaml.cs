// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlockMap.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI custom controls.
//
// --[ Description ] ----------------------------------------------------------
//
//     A block map control to visualize sector access times.
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Aaru.Gui.Controls;

public partial class BlockMap : UserControl
{
    const int    BlockSize    = 4;     // Size of each block in pixels
    const int    BlockSpacing = 1;     // Spacing between blocks
    const double MinDuration  = 1.0;   // Green threshold (ms)
    const double MaxDuration  = 500.0; // Red threshold (ms)

    public static readonly StyledProperty<ObservableCollection<(ulong startingSector, double duration)>>
        SectorDataProperty =
            AvaloniaProperty
               .Register<BlockMap, ObservableCollection<(ulong startingSector, double duration)>>(nameof(SectorData));

    public static readonly StyledProperty<uint> ScanBlockSizeProperty =
        AvaloniaProperty.Register<BlockMap, uint>(nameof(ScanBlockSize), 1u);
    readonly Image _image;

    WriteableBitmap _bitmap;
    int             _blocksPerRow;
    int             _rows;

    uint                                                          _scanBlockSize = 1;
    ObservableCollection<(ulong startingSector, double duration)> _sectorData;

    public BlockMap()
    {
        InitializeComponent();
        _image              =  this.FindControl<Image>("BlockImage");
        _image.PointerMoved += OnPointerMoved;
    }

    public ObservableCollection<(ulong startingSector, double duration)> SectorData
    {
        get => GetValue(SectorDataProperty);
        set => SetValue(SectorDataProperty, value);
    }

    public uint ScanBlockSize
    {
        get => GetValue(ScanBlockSizeProperty);
        set => SetValue(ScanBlockSizeProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if(change.Property == SectorDataProperty)
        {
            if(_sectorData != null) _sectorData.CollectionChanged -= OnSectorDataChanged;

            _sectorData = change.GetNewValue<ObservableCollection<(ulong startingSector, double duration)>>();

            if(_sectorData == null) return;

            _sectorData.CollectionChanged += OnSectorDataChanged;
            RedrawAll();
        }
        else if(change.Property == ScanBlockSizeProperty)
        {
            _scanBlockSize = change.GetNewValue<uint>();
            RedrawAll();
        }
        else if(change.Property == BoundsProperty)
        {
            CalculateBlocksPerRow();
            RedrawAll();
        }
    }

    private void CalculateBlocksPerRow()
    {
        if(Bounds.Width <= 0) return;
        int blockWithSpacing = BlockSize + BlockSpacing;
        _blocksPerRow = Math.Max(1, (int)Bounds.Width / blockWithSpacing);
        _rows         = _sectorData == null ? 0 : (_sectorData.Count + _blocksPerRow - 1) / _blocksPerRow;
        EnsureBitmap();
    }

    void EnsureBitmap()
    {
        if(_blocksPerRow <= 0 || _rows <= 0) return;

        int blockWithSpacing = BlockSize + BlockSpacing;
        int width            = _blocksPerRow * blockWithSpacing;
        int height           = _rows         * blockWithSpacing;

        if(_bitmap != null && _bitmap.PixelSize.Width == width && _bitmap.PixelSize.Height == height) return;

        // Sanity cap: a runaway row count would otherwise allocate a gigantic framebuffer
        if(width <= 0 || height <= 0 || (long)width * height > 64 * 1024 * 1024) return;

        var newBitmap = new WriteableBitmap(new PixelSize(width, height),
                                            new Vector(96, 96),
                                            PixelFormat.Bgra8888,
                                            AlphaFormat.Premul);

        _bitmap?.Dispose();
        _bitmap = newBitmap;

        if(_image == null) return;

        _image.Source = _bitmap;
        _image.Width  = width;
        _image.Height = height;
    }

    private void RedrawAll()
    {
        if(_sectorData == null || _sectorData.Count == 0)
        {
            if(_bitmap == null || _image == null) return;

            using(ILockedFramebuffer fb = _bitmap.Lock())
            {
                unsafe
                {
                    new Span<byte>((void*)fb.Address, fb.Size.Height * fb.RowBytes).Clear();
                }
            }

            _image.InvalidateVisual();

            return;
        }

        if(_blocksPerRow == 0) return;
        CalculateBlocksPerRow();

        if(_bitmap == null) return;

        using(ILockedFramebuffer fb = _bitmap.Lock())
        {
            unsafe
            {
                var data = new Span<byte>((void*)fb.Address, fb.Size.Height * fb.RowBytes);
                data.Clear();

                int blockWithSpacing = BlockSize + BlockSpacing;

                for(var i = 0; i < _sectorData.Count; i++)
                {
                    (ulong _, double duration) = _sectorData[i];
                    Color color = GetColorForDuration(duration);

                    int row = i       / _blocksPerRow;
                    int col = i       % _blocksPerRow;
                    DrawBlock(fb, col * blockWithSpacing, row * blockWithSpacing, color);
                }
            }
        }

        _image.InvalidateVisual();
    }

    static void DrawBlock(ILockedFramebuffer fb, int x, int y, Color color)
    {
        int stride = fb.RowBytes;

        if(x < 0 || y < 0 || (x + BlockSize) * 4 > stride || y + BlockSize > fb.Size.Height) return;

        unsafe
        {
            var basePtr = (byte*)fb.Address;

            for(var dy = 0; dy < BlockSize; dy++)
            {
                byte* rowPtr = basePtr + (y + dy) * stride + x * 4;

                for(var dx = 0; dx < BlockSize; dx++)
                {
                    rowPtr[0] =  color.B;
                    rowPtr[1] =  color.G;
                    rowPtr[2] =  color.R;
                    rowPtr[3] =  255;
                    rowPtr    += 4;
                }
            }
        }
    }

    private void OnSectorDataChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if(_sectorData == null) return;

        if(e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && e.NewItems.Count > 0)
        {
            // If not laid out yet, defer to full redraw
            if(_blocksPerRow == 0)
            {
                RedrawAll();

                return;
            }

            // Check if rows changed (only depends on count now)
            int newRows = (_sectorData.Count + _blocksPerRow - 1) / _blocksPerRow;

            if(newRows != _rows)
            {
                _rows = newRows;
                EnsureBitmap();
                RedrawAll();

                return;
            }

            if(_bitmap == null) EnsureBitmap();

            if(_bitmap == null) return;

            using(ILockedFramebuffer fb = _bitmap.Lock())
            {
                int blockWithSpacing = BlockSize + BlockSpacing;
                int startIndex       = e.NewStartingIndex;
                int count            = e.NewItems.Count;

                for(var i = 0; i < count; i++)
                {
                    int dataIndex = startIndex + i;

                    if(dataIndex >= _sectorData.Count) break;
                    (ulong _, double duration) = _sectorData[dataIndex];
                    Color color = GetColorForDuration(duration);
                    int   row   = dataIndex / _blocksPerRow;
                    int   col   = dataIndex % _blocksPerRow;
                    DrawBlock(fb, col       * blockWithSpacing, row * blockWithSpacing, color);
                }
            }

            _image.InvalidateVisual();
        }
        else
            RedrawAll();
    }

    void OnPointerMoved(object sender, PointerEventArgs e)
    {
        if(_sectorData == null || _sectorData.Count == 0 || _blocksPerRow == 0)
        {
            ToolTip.SetTip(_image, null);

            return;
        }

        Point p                = e.GetPosition(_image);
        int   blockWithSpacing = BlockSize + BlockSpacing;

        if(p.X < 0 || p.Y < 0)
        {
            ToolTip.SetTip(_image, null);

            return;
        }

        var col = (int)(p.X / blockWithSpacing);
        var row = (int)(p.Y / blockWithSpacing);

        if(col < 0 || row < 0)
        {
            ToolTip.SetTip(_image, null);

            return;
        }

        int index = row * _blocksPerRow + col;

        if(index < 0 || index >= _sectorData.Count)
        {
            ToolTip.SetTip(_image, null);

            return;
        }

        (ulong startSector, double duration) = _sectorData[index];
        ulong endSector = startSector + (_scanBlockSize > 0 ? _scanBlockSize - 1u : 0u);

        string tooltipText = _scanBlockSize > 1
                                 ? $"Sectors {startSector} - {endSector}\nBlock size: {_scanBlockSize} sectors\nDuration: {duration:F2} ms"
                                 : $"Sector {startSector}\nDuration: {duration:F2} ms";

        ToolTip.SetTip(_image, tooltipText);
    }

    private static Color GetColorForDuration(double duration)
    {
        // Clamp duration between min and max
        double clampedDuration = Math.Max(MinDuration, Math.Min(MaxDuration, duration));

        // Color gradient thresholds with more intermediate steps:
        // 1ms -> Green (0, 255, 0)
        // 3ms -> Bright Green (32, 255, 0)
        // 5ms -> Green-Lime (64, 255, 0)
        // 10ms -> Lime (128, 255, 0)
        // 20ms -> Lime-Yellow (192, 255, 0)
        // 35ms -> Light Yellow (224, 255, 0)
        // 50ms -> Yellow (255, 255, 0)
        // 75ms -> Yellow-Orange (255, 220, 0)
        // 100ms -> Light Orange (255, 192, 0)
        // 150ms -> Orange (255, 128, 0)
        // 225ms -> Red-Orange (255, 64, 0)
        // 350ms -> Dark Red-Orange (255, 32, 0)
        // 500ms -> Red (255, 0, 0)

        if(clampedDuration <= 3.0) // 1ms to 3ms: Green to Bright Green
        {
            double t = (clampedDuration - MinDuration) / (3.0 - MinDuration);

            return Color.FromRgb((byte)(0 + t * 32), // R: 0 -> 32
                                 255,                // G: stays 255
                                 0                   // B: stays 0
                                );
        }

        if(clampedDuration <= 5.0) // 3ms to 5ms: Bright Green to Green-Lime
        {
            double t = (clampedDuration - 3.0) / (5.0 - 3.0);

            return Color.FromRgb((byte)(32 + t * 32), // R: 32 -> 64
                                 255,                 // G: stays 255
                                 0                    // B: stays 0
                                );
        }

        if(clampedDuration <= 10.0) // 5ms to 10ms: Green-Lime to Lime
        {
            double t = (clampedDuration - 5.0) / (10.0 - 5.0);

            return Color.FromRgb((byte)(64 + t * 64), // R: 64 -> 128
                                 255,                 // G: stays 255
                                 0                    // B: stays 0
                                );
        }

        if(clampedDuration <= 20.0) // 10ms to 20ms: Lime to Lime-Yellow
        {
            double t = (clampedDuration - 10.0) / (20.0 - 10.0);

            return Color.FromRgb((byte)(128 + t * 64), // R: 128 -> 192
                                 255,                  // G: stays 255
                                 0                     // B: stays 0
                                );
        }

        if(clampedDuration <= 35.0) // 20ms to 35ms: Lime-Yellow to Light Yellow
        {
            double t = (clampedDuration - 20.0) / (35.0 - 20.0);

            return Color.FromRgb((byte)(192 + t * 32), // R: 192 -> 224
                                 255,                  // G: stays 255
                                 0                     // B: stays 0
                                );
        }

        if(clampedDuration <= 50.0) // 35ms to 50ms: Light Yellow to Yellow
        {
            double t = (clampedDuration - 35.0) / (50.0 - 35.0);

            return Color.FromRgb((byte)(224 + t * 31), // R: 224 -> 255
                                 255,                  // G: stays 255
                                 0                     // B: stays 0
                                );
        }

        if(clampedDuration <= 75.0) // 50ms to 75ms: Yellow to Yellow-Orange
        {
            double t = (clampedDuration - 50.0) / (75.0 - 50.0);

            return Color.FromRgb(255,                  // R: stays 255
                                 (byte)(255 - t * 35), // G: 255 -> 220
                                 0                     // B: stays 0
                                );
        }

        if(clampedDuration <= 100.0) // 75ms to 100ms: Yellow-Orange to Light Orange
        {
            double t = (clampedDuration - 75.0) / (100.0 - 75.0);

            return Color.FromRgb(255,                  // R: stays 255
                                 (byte)(220 - t * 28), // G: 220 -> 192
                                 0                     // B: stays 0
                                );
        }

        if(clampedDuration <= 150.0) // 100ms to 150ms: Light Orange to Orange
        {
            double t = (clampedDuration - 100.0) / (150.0 - 100.0);

            return Color.FromRgb(255,                  // R: stays 255
                                 (byte)(192 - t * 64), // G: 192 -> 128
                                 0                     // B: stays 0
                                );
        }

        if(clampedDuration <= 225.0) // 150ms to 225ms: Orange to Red-Orange
        {
            double t = (clampedDuration - 150.0) / (225.0 - 150.0);

            return Color.FromRgb(255,                  // R: stays 255
                                 (byte)(128 - t * 64), // G: 128 -> 64
                                 0                     // B: stays 0
                                );
        }

        if(clampedDuration <= 350.0) // 225ms to 350ms: Red-Orange to Dark Red-Orange
        {
            double t = (clampedDuration - 225.0) / (350.0 - 225.0);

            return Color.FromRgb(255,                 // R: stays 255
                                 (byte)(64 - t * 32), // G: 64 -> 32
                                 0                    // B: stays 0
                                );
        }
        else // 350ms to 500ms: Dark Red-Orange to Red
        {
            double t = (clampedDuration - 350.0) / (MaxDuration - 350.0);

            return Color.FromRgb(255,                 // R: stays 255
                                 (byte)(32 - t * 32), // G: 32 -> 0
                                 0                    // B: stays 0
                                );
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if(_sectorData != null) _sectorData.CollectionChanged -= OnSectorDataChanged;

        if(_image != null) _image.Source = null;

        _bitmap?.Dispose();
        _bitmap = null;
    }
}