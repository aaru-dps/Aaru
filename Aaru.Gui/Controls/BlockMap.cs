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
//     Draws a block map.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Aaru.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Visuals.Media.Imaging;
using JetBrains.Annotations;

namespace Aaru.Gui.Controls;

// TODO: Partially fill clusters
// TODO: React to size changes
// TODO: Optimize block size to viewport
// TODO: Writing one more than it should
public sealed class BlockMap : ItemsControl
{
    const int BLOCK_SIZE = 15;
    public static readonly StyledProperty<ulong> BlocksProperty =
        AvaloniaProperty.Register<Border, ulong>(nameof(Blocks));

    public static readonly StyledProperty<IBrush> SuperFastColorProperty =
        AvaloniaProperty.Register<Border, IBrush>(nameof(SuperFastColor), Brushes.LightGreen);

    public static readonly StyledProperty<IBrush> FastColorProperty =
        AvaloniaProperty.Register<Border, IBrush>(nameof(FastColor), Brushes.Green);

    public static readonly StyledProperty<IBrush> AverageColorProperty =
        AvaloniaProperty.Register<Border, IBrush>(nameof(AverageColor), Brushes.DarkGreen);

    public static readonly StyledProperty<IBrush> SlowColorProperty =
        AvaloniaProperty.Register<Border, IBrush>(nameof(SlowColor), Brushes.Yellow);

    public static readonly StyledProperty<IBrush> SuperSlowColorProperty =
        AvaloniaProperty.Register<Border, IBrush>(nameof(SuperSlowColor), Brushes.Orange);

    public static readonly StyledProperty<IBrush> ProblematicColorProperty =
        AvaloniaProperty.Register<Border, IBrush>(nameof(ProblematicColor), Brushes.Red);

    public static readonly StyledProperty<double> SuperFastMaxTimeProperty =
        AvaloniaProperty.Register<Border, double>(nameof(SuperFastMaxTime), 3);

    public static readonly StyledProperty<double> FastMaxTimeProperty =
        AvaloniaProperty.Register<Border, double>(nameof(FastMaxTime), 10);

    public static readonly StyledProperty<double> AverageMaxTimeProperty =
        AvaloniaProperty.Register<Border, double>(nameof(AverageMaxTime), 50);

    public static readonly StyledProperty<double> SlowMaxTimeProperty =
        AvaloniaProperty.Register<Border, double>(nameof(SlowMaxTime), 150);

    public static readonly StyledProperty<double> SuperSlowMaxTimeProperty =
        AvaloniaProperty.Register<Border, double>(nameof(SuperSlowMaxTime), 500);
    RenderTargetBitmap _bitmap;
    ulong              _clusterSize;
    ulong              _maxBlocks;

    public double SuperFastMaxTime
    {
        get => GetValue(SuperFastMaxTimeProperty);
        set => SetValue(SuperFastMaxTimeProperty, value);
    }

    public double FastMaxTime
    {
        get => GetValue(FastMaxTimeProperty);
        set => SetValue(FastMaxTimeProperty, value);
    }

    public double AverageMaxTime
    {
        get => GetValue(AverageMaxTimeProperty);
        set => SetValue(AverageMaxTimeProperty, value);
    }

    public double SlowMaxTime
    {
        get => GetValue(SlowMaxTimeProperty);
        set => SetValue(SlowMaxTimeProperty, value);
    }

    public double SuperSlowMaxTime
    {
        get => GetValue(SuperSlowMaxTimeProperty);
        set => SetValue(SuperSlowMaxTimeProperty, value);
    }

    public IBrush SuperFastColor
    {
        get => GetValue(SuperFastColorProperty);
        set => SetValue(SuperFastColorProperty, value);
    }

    public IBrush FastColor
    {
        get => GetValue(FastColorProperty);
        set => SetValue(FastColorProperty, value);
    }

    public IBrush AverageColor
    {
        get => GetValue(AverageColorProperty);
        set => SetValue(AverageColorProperty, value);
    }

    public IBrush SlowColor
    {
        get => GetValue(SlowColorProperty);
        set => SetValue(SlowColorProperty, value);
    }

    public IBrush SuperSlowColor
    {
        get => GetValue(SuperSlowColorProperty);
        set => SetValue(SuperSlowColorProperty, value);
    }

    public IBrush ProblematicColor
    {
        get => GetValue(ProblematicColorProperty);
        set => SetValue(ProblematicColorProperty, value);
    }

    public ulong Blocks
    {
        get => GetValue(BlocksProperty);
        set => SetValue(BlocksProperty, value);
    }

    protected override void OnPropertyChanged<T>([NotNull] AvaloniaPropertyChangedEventArgs<T> e)
    {
        base.OnPropertyChanged(e);

        switch(e.Property.Name)
        {
            case nameof(Blocks):
                if(_maxBlocks == 0)
                    _maxBlocks = (ulong)(Width / BLOCK_SIZE * (Height / BLOCK_SIZE));

                if(Blocks > _maxBlocks)
                {
                    _clusterSize = Blocks / _maxBlocks;

                    if(Blocks % _maxBlocks > 0)
                        _clusterSize++;

                    if(Blocks / _clusterSize < _maxBlocks)
                    {
                        _maxBlocks = Blocks / _clusterSize;

                        if(Blocks % _clusterSize > 0)
                            _maxBlocks++;
                    }
                }
                else
                {
                    _clusterSize = 1;
                    _maxBlocks   = Blocks;
                }

                CreateBitmap();
                DrawGrid();
                RedrawAll();
                Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);

                break;
            case nameof(SuperFastMaxTime):
            case nameof(FastMaxTime):
            case nameof(AverageMaxTime):
            case nameof(SlowMaxTime):
            case nameof(SuperSlowMaxTime):
            case nameof(SuperFastColor):
            case nameof(FastColor):
            case nameof(AverageColor):
            case nameof(SlowColor):
            case nameof(SuperSlowColor):
            case nameof(ProblematicColor):

                CreateBitmap();
                DrawGrid();
                RedrawAll();
                Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);

                break;
        }
    }

    public override void Render(DrawingContext context)
    {
        if((int?)_bitmap?.Size.Height != (int)Height ||
           (int?)_bitmap?.Size.Width  != (int)Width)
        {
            _maxBlocks = (ulong)(Width / BLOCK_SIZE * (Height / BLOCK_SIZE));
            CreateBitmap();
        }

        context.DrawImage(_bitmap, new Rect(0, 0, Width, Height), new Rect(0, 0, Width, Height),
                          BitmapInterpolationMode.HighQuality);

        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
        base.Render(context);
    }

    protected override void ItemsCollectionChanged(object sender, [NotNull] NotifyCollectionChangedEventArgs e)
    {
        base.ItemsCollectionChanged(sender, e);

        switch(e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
            {
                if(e.NewItems is not {} items)
                    throw new ArgumentException(UI.Invalid_list_of_items);

                using IDrawingContextImpl ctxi = _bitmap.CreateDrawingContext(null);
                using var                 ctx  = new DrawingContext(ctxi, false);

                foreach(object item in items)
                {
                    if(item is not ValueTuple<ulong, double> block)
                        throw new ArgumentException(UI.Invalid_item_in_list, nameof(Items));

                    DrawCluster(block.Item1, block.Item2, false, ctx);
                }

                Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);

                break;
            }
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Move:
            {
                if(e.NewItems is not {} newItems ||
                   e.OldItems is not {} oldItems)
                    throw new ArgumentException(UI.Invalid_list_of_items);

                using IDrawingContextImpl ctxi = _bitmap.CreateDrawingContext(null);
                using var                 ctx  = new DrawingContext(ctxi, false);

                foreach(object item in oldItems)
                {
                    if(item is not ValueTuple<ulong, double> block)
                        throw new ArgumentException(UI.Invalid_item_in_list, nameof(Items));

                    DrawCluster(block.Item1, block.Item2, false, ctx);
                }

                foreach(object item in newItems)
                {
                    if(item is not ValueTuple<ulong, double> block)
                        throw new ArgumentException(UI.Invalid_item_in_list, nameof(Items));

                    DrawCluster(block.Item1, block.Item2, false, ctx);
                }

                Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);

                break;
            }
            case NotifyCollectionChangedAction.Reset:
                CreateBitmap();
                DrawGrid();
                Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);

                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    void RedrawAll()
    {
        if(Items is null)
            return;

        using IDrawingContextImpl ctxi = _bitmap.CreateDrawingContext(null);
        using var                 ctx  = new DrawingContext(ctxi, false);

        foreach(object item in Items)
        {
            if(item is not ValueTuple<ulong, double> block)
                throw new ArgumentException(UI.Invalid_item_in_list, nameof(Items));

            DrawCluster(block.Item1, block.Item2, false, ctx);
        }

        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    void DrawCluster(ulong block, double duration, bool clear = false, DrawingContext ctx = null)
    {
        if(double.IsNegative(duration) ||
           double.IsInfinity(duration))
            throw new ArgumentException(UI.Duration_cannot_be_negative_or_infinite, nameof(duration));

        bool  newContext     = ctx is null;
        ulong clustersPerRow = (ulong)Width / BLOCK_SIZE;
        ulong cluster        = block        / _clusterSize;
        ulong row            = cluster      / clustersPerRow;
        ulong column         = cluster      % clustersPerRow;
        ulong x              = column       * BLOCK_SIZE;
        ulong y              = row          * BLOCK_SIZE;
        var   pen            = new Pen(Foreground);

        IBrush brush;

        if(clear)
            brush = Background;
        else if(duration < SuperFastMaxTime)
            brush = SuperFastColor;
        else if(duration >= SuperFastMaxTime &&
                duration < FastMaxTime)
            brush = FastColor;
        else if(duration >= FastMaxTime &&
                duration < AverageMaxTime)
            brush = AverageColor;
        else if(duration >= AverageMaxTime &&
                duration < SlowMaxTime)
            brush = SlowColor;
        else if(duration >= SlowMaxTime &&
                duration < SuperSlowMaxTime)
            brush = SuperSlowColor;
        else if(duration >= SuperSlowMaxTime ||
                double.IsNaN(duration))
            brush = ProblematicColor;
        else
            brush = Background;

        if(newContext)
        {
            using IDrawingContextImpl ctxi = _bitmap.CreateDrawingContext(null);
            ctx = new DrawingContext(ctxi, false);
        }

        ctx.FillRectangle(brush, new Rect(x, y, BLOCK_SIZE, BLOCK_SIZE));
        ctx.DrawRectangle(pen, new Rect(x, y, BLOCK_SIZE, BLOCK_SIZE));

        if(double.IsNaN(duration))
        {
            ctx.DrawLine(pen, new Point(x, y), new Point(x + BLOCK_SIZE, y            + BLOCK_SIZE));
            ctx.DrawLine(pen, new Point(x, y               + BLOCK_SIZE), new Point(x + BLOCK_SIZE, y));
        }

        if(newContext)
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    protected override void ItemsChanged([NotNull] AvaloniaPropertyChangedEventArgs e)
    {
        if(e.NewValue != null &&
           e.NewValue is not IList<(ulong, double)>)
            throw new ArgumentException(UI.Items_must_be_a_IList_ulong_double);

        base.ItemsChanged(e);

        CreateBitmap();
        DrawGrid();
        RedrawAll();
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    void CreateBitmap()
    {
        if(_maxBlocks == 0)
            _maxBlocks = (ulong)(Width / BLOCK_SIZE * (Height / BLOCK_SIZE));

        _bitmap?.Dispose();

        _bitmap = new RenderTargetBitmap(new PixelSize((int)Width, (int)Height), new Vector(96, 96));

        using IDrawingContextImpl ctxi = _bitmap.CreateDrawingContext(null);
        using var                 ctx  = new DrawingContext(ctxi, false);

        ctx.FillRectangle(Background, new Rect(0, 0, Width, Height));
    }

    void DrawGrid()
    {
        using IDrawingContextImpl ctxi = _bitmap.CreateDrawingContext(null);
        using var                 ctx  = new DrawingContext(ctxi, false);

        ulong clustersPerRow = (ulong)Width / BLOCK_SIZE;

        bool allBlocksDrawn = false;

        for(ulong y = 0; y < Height && !allBlocksDrawn; y += BLOCK_SIZE)
        {
            for(ulong x = 0; x < Width; x += BLOCK_SIZE)
            {
                ulong currentBlockValue = (y * clustersPerRow / BLOCK_SIZE) + (x / BLOCK_SIZE);

                if(currentBlockValue >= _maxBlocks ||
                   currentBlockValue >= Blocks)
                {
                    allBlocksDrawn = true;

                    break;
                }

                ctx.DrawRectangle(new Pen(Foreground), new Rect(x, y, BLOCK_SIZE, BLOCK_SIZE));
            }
        }
    }

    void DrawSquares(Color[] colors, int borderWidth, int sideLength)
    {
        using IDrawingContextImpl ctxi = _bitmap.CreateDrawingContext(null);
        using var                 ctx  = new DrawingContext(ctxi, false);

        int squareWidth = (sideLength - (2 * borderWidth)) / colors.Length;
        int x           = 0;
        int y           = 0;

        foreach(Color color in colors)
        {
            ctx.FillRectangle(new SolidColorBrush(color), new Rect(x, y, squareWidth, squareWidth));
            x += squareWidth + (2 * borderWidth);

            if(x < sideLength)
                continue;

            x =  0;
            y += squareWidth + (2 * borderWidth);
        }
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if(Width  < 1          ||
           Height < 1          ||
           double.IsNaN(Width) ||
           double.IsNaN(Height))
        {
            base.OnAttachedToLogicalTree(e);

            return;
        }

        CreateBitmap();
        DrawGrid();

        base.OnAttachedToLogicalTree(e);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _bitmap.Dispose();
        _bitmap = null;
        base.OnDetachedFromLogicalTree(e);
    }
}