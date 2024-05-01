// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlockMap.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Interfaces;
using SkiaSharp;

namespace Aaru.Core.Graphics;

public class BlockMap : IMediaGraph
{
    readonly SKBitmap _bitmap;
    readonly SKCanvas _canvas;
    readonly int      _columns;
    readonly int      _sectorsPerSquare;
    readonly int      _squareSize;

    public BlockMap(int width, int height, ulong maxSectors)
    {
        _squareSize = 8;
        _columns    = (width - 1) / (_squareSize + 1);
        int rows    = (height - 1) / (_squareSize + 1);
        int squares = _columns     * rows;

        // Check if we can get bigger squares
        while(squares - _columns > (long)maxSectors)
        {
            _squareSize++;

            _columns = (width  - 1) / (_squareSize + 1);
            rows     = (height - 1) / (_squareSize + 1);
            squares  = _columns     * rows;
        }

        _sectorsPerSquare = (int)((long)maxSectors / squares);

        while(_sectorsPerSquare > 0 && _squareSize > 4)
        {
            _squareSize--;

            _columns = (width  - 1) / (_squareSize + 1);
            rows     = (height - 1) / (_squareSize + 1);
            squares  = _columns     * rows;

            _sectorsPerSquare = (int)((long)maxSectors / squares);
        }

        var removeSquaresAtLastRow = 0;
        var removeRows             = 0;

        // If we have spare squares, remove them
        if(squares > (long)maxSectors)
        {
            var removeSquares = (int)(squares - (long)maxSectors);
            removeRows             = removeSquares / _columns;
            removeSquaresAtLastRow = removeSquares % _columns;
        }

        float w = _columns * (_squareSize + 1) + 1;
        float h = rows     * (_squareSize + 1) + 1;

        _bitmap = new SKBitmap((int)w, (int)h);
        _canvas = new SKCanvas(_bitmap);

        // Paint background white
        _canvas.DrawRect(0,
                         0,
                         w,
                         h,
                         new SKPaint
                         {
                             Style = SKPaintStyle.StrokeAndFill,
                             Color = SKColors.White
                         });

        // Paint undumped sectors
        _canvas.DrawRect(0,
                         0,
                         w,
                         h - removeRows * (_squareSize + 1) - _squareSize - 2,
                         new SKPaint
                         {
                             Style = SKPaintStyle.StrokeAndFill,
                             Color = SKColors.Gray
                         });

        _canvas.DrawRect(0,
                         h - removeRows * (_squareSize + 1) - _squareSize - 2,
                         (_columns - removeSquaresAtLastRow) * (_squareSize + 1),
                         _squareSize + 2,
                         new SKPaint
                         {
                             Style = SKPaintStyle.StrokeAndFill,
                             Color = SKColors.Gray
                         });

        // Draw grid
        for(float y = 0; y < h - removeRows * (_squareSize + 1); y += _squareSize + 1)
        {
            if(y > h - removeRows * (_squareSize + 1) - (_squareSize + 2))
            {
                int cw = _columns - removeSquaresAtLastRow;

                _canvas.DrawLine(0f,
                                 y,
                                 cw * (_squareSize + 1),
                                 y,
                                 new SKPaint
                                 {
                                     StrokeWidth = 1f,
                                     Color       = SKColors.Black
                                 });
            }
            else
            {
                _canvas.DrawLine(0f,
                                 y,
                                 w,
                                 y,
                                 new SKPaint
                                 {
                                     StrokeWidth = 1f,
                                     Color       = SKColors.Black
                                 });
            }
        }

        for(float x = 0; x < w; x += _squareSize + 1)
        {
            float currentColumn = x / (_squareSize + 1);

            if(_columns - currentColumn + 1 > removeSquaresAtLastRow)
            {
                _canvas.DrawLine(x,
                                 0,
                                 x,
                                 h - removeRows * (_squareSize + 1),
                                 new SKPaint
                                 {
                                     StrokeWidth = 1f,
                                     Color       = SKColors.Black
                                 });
            }
            else
            {
                _canvas.DrawLine(x,
                                 0,
                                 x,
                                 h - removeRows * (_squareSize + 1) - _squareSize - 2,
                                 new SKPaint
                                 {
                                     StrokeWidth = 1f,
                                     Color       = SKColors.Black
                                 });
            }
        }
    }

#region IMediaGraph Members

    /// <inheritdoc />
    public void PaintSectorGood(ulong sector) => PaintSector(sector, SKColors.Green);

    /// <inheritdoc />
    public void PaintSectorBad(ulong sector) => PaintSector(sector, SKColors.Red);

    /// <inheritdoc />
    public void PaintSectorUnknown(ulong sector) => PaintSector(sector, SKColors.Yellow);

    /// <inheritdoc />
    public void PaintSectorUndumped(ulong sector) => PaintSector(sector, SKColors.Gray);

    /// <inheritdoc />
    public void PaintSector(ulong sector, byte red, byte green, byte blue, byte opacity = 255) =>
        PaintSector(sector, new SKColor(red, green, blue, opacity));

    /// <inheritdoc />
    public void PaintSectorsUndumped(ulong startingSector, uint length) =>
        PaintSectors(startingSector, length, SKColors.Gray);

    /// <inheritdoc />
    public void PaintSectorsGood(ulong startingSector, uint length) =>
        PaintSectors(startingSector, length, SKColors.Green);

    /// <inheritdoc />
    public void PaintSectorsBad(ulong startingSector, uint length) =>
        PaintSectors(startingSector, length, SKColors.Red);

    /// <inheritdoc />
    public void PaintSectorsUnknown(ulong startingSector, uint length) =>
        PaintSectors(startingSector, length, SKColors.Yellow);

    /// <inheritdoc />
    public void PaintSectors(ulong startingSector, uint length, byte red, byte green, byte blue, byte opacity = 255) =>
        PaintSectors(startingSector, length, new SKColor(red, green, blue, opacity));

    /// <inheritdoc />
    public void PaintSectorsUndumped(IEnumerable<ulong> sectors) => PaintSectors(sectors, SKColors.Gray);

    /// <inheritdoc />
    public void PaintSectorsGood(IEnumerable<ulong> sectors) => PaintSectors(sectors, SKColors.Green);

    /// <inheritdoc />
    public void PaintSectorsBad(IEnumerable<ulong> sectors) => PaintSectors(sectors, SKColors.Red);

    /// <inheritdoc />
    public void PaintSectorsUnknown(IEnumerable<ulong> sectors) => PaintSectors(sectors, SKColors.Yellow);

    /// <inheritdoc />
    public void PaintSectorsUnknown(IEnumerable<ulong> sectors, byte red, byte green, byte blue, byte opacity = 255) =>
        PaintSectors(sectors, new SKColor(red, green, blue, opacity));

    /// <inheritdoc />
    public void PaintRecordableInformationGood()
    {
        // Do nothing
    }

    /// <inheritdoc />
    public void WriteTo(Stream stream)
    {
        var    image = SKImage.FromBitmap(_bitmap);
        SKData data  = image.Encode();
        data.SaveTo(stream);
    }

    /// <inheritdoc />
    public void WriteTo(string path)
    {
        using var fs = new FileStream(path, FileMode.Create);
        WriteTo(fs);
        fs.Close();
    }

#endregion

    void PaintSector(ulong sector, SKColor color)
    {
        SKRect rect =
            GetSquareRectangle(_sectorsPerSquare == 0 ? (int)sector : (int)(sector / (ulong)_sectorsPerSquare));

        _canvas.DrawRect(rect,
                         new SKPaint
                         {
                             Style = SKPaintStyle.StrokeAndFill,
                             Color = color
                         });
    }

    void PaintSectors(ulong startingSector, uint length, SKColor color)
    {
        for(ulong sector = startingSector; sector < startingSector + length; sector++)
        {
            SKRect rect =
                GetSquareRectangle(_sectorsPerSquare == 0 ? (int)sector : (int)(sector / (ulong)_sectorsPerSquare));

            _canvas.DrawRect(rect,
                             new SKPaint
                             {
                                 Style = SKPaintStyle.StrokeAndFill,
                                 Color = color
                             });
        }
    }

    void PaintSectors(IEnumerable<ulong> sectors, SKColor color)
    {
        foreach(SKRect rect in sectors.Select(sector => GetSquareRectangle(_sectorsPerSquare == 0
                                                                               ? (int)sector
                                                                               : (int)(sector /
                                                                                           (ulong)_sectorsPerSquare))))
        {
            _canvas.DrawRect(rect,
                             new SKPaint
                             {
                                 Style = SKPaintStyle.StrokeAndFill,
                                 Color = color
                             });
        }
    }

    SKRect GetSquareRectangle(int square)
    {
        int row    = square / _columns;
        int column = square % _columns;

        float x  = 1 + column * (_squareSize + 1);
        float y  = 1 + row    * (_squareSize + 1);
        float xp = x + _squareSize;
        float yp = y + _squareSize;

        return new SKRect(x, y, xp, yp);
    }
}