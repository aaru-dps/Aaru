// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DataFile.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Abstracts writing to files.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using SkiaSharp;

namespace Aaru.Core.Graphics;

// TODO: HD DVD sectors are a guess
public sealed class Spiral
{
    static readonly DiscParameters _cdParameters = new(120, 15, 33, 46, 50, 116, 0, 0, 360000, SKColors.Silver);
    static readonly DiscParameters _cdRecordableParameters =
        new(120, 15, 33, 46, 50, 116, 45, 46, 360000, new SKColor(0xBD, 0xA0, 0x00));
    static readonly DiscParameters _cdRewritableParameters =
        new(120, 15, 33, 46, 50, 116, 45, 46, 360000, new SKColor(0x50, 0x50, 0x50));
    static readonly DiscParameters _ddcdParameters = new(120, 15, 33, 46, 50, 116, 0, 0, 720000, SKColors.Silver);
    static readonly DiscParameters _ddcdRecordableParameters =
        new(120, 15, 33, 46, 50, 116, 45, 46, 720000, new SKColor(0xBD, 0xA0, 0x00));
    static readonly DiscParameters _ddcdRewritableParameters =
        new(120, 15, 33, 46, 50, 116, 45, 46, 720000, new SKColor(0x50, 0x50, 0x50));
    static readonly DiscParameters _dvdPlusRParameters =
        new(120, 15, 33, 46.8f, 48, 116, 46.586f, 46.8f, 2295104, new SKColor(0x6f, 0x0A, 0xCA));
    static readonly DiscParameters _dvdPlusRParameters80 =
        new(80, 15, 33, 46.8f, 48, 76, 46.586f, 46.8f, 714544, new SKColor(0x6f, 0x0A, 0xCA));
    static readonly DiscParameters _dvdPlusRwParameters =
        new(120, 15, 33, 44, 48, 116, 47.792f, 48, 2295104, new SKColor(0x38, 0x38, 0x38));
    static readonly DiscParameters _dvdPlusRwParameters80 =
        new(80, 15, 33, 44, 48, 76, 47.792f, 48, 714544, new SKColor(0x38, 0x38, 0x38));
    static readonly DiscParameters _ps1CdParameters = new(120, 15, 33, 46, 50, 116, 0, 0, 360000, SKColors.Black);
    static readonly DiscParameters _ps2CdParameters =
        new(120, 15, 33, 46, 50, 116, 0, 0, 360000, new SKColor(0x0c, 0x08, 0xc3));
    static readonly DiscParameters _dvdParameters =
        new(120, 15, 33, 44, 48, 116, 0, 0, 2294922, new SKColor(0x6f, 0x0A, 0xCA));
    static readonly DiscParameters _dvdParameters80 =
        new(120, 15, 33, 44, 48, 76, 0, 0, 714544, new SKColor(0x6f, 0x0A, 0xCA));
    static readonly DiscParameters _dvdRParameters =
        new(120, 15, 33, 46, 48, 116, 44, 46, 2294922, new SKColor(0x6f, 0x0A, 0xCA));
    static readonly DiscParameters _dvdRParameters80 =
        new(80, 15, 33, 46, 48, 76, 44, 46, 712891, new SKColor(0x6f, 0x0A, 0xCA));
    static readonly DiscParameters _dvdRwParameters =
        new(120, 15, 33, 46, 48, 116, 44, 46, 2294922, new SKColor(0x38, 0x38, 0x38));
    static readonly DiscParameters _dvdRwParameters80 =
        new(80, 15, 33, 46, 48, 76, 44, 46, 712891, new SKColor(0x38, 0x38, 0x38));
    static readonly DiscParameters _bdParameters =
        new(120, 15, 33, 44, 48, 116, 0, 0, 12219392, new SKColor(0x80, 0x80, 0x80));
    static readonly DiscParameters _bdRParameters =
        new(120, 15, 33, 46, 48, 116, 44, 46, 12219392, new SKColor(0x40, 0x40, 0x40));
    static readonly DiscParameters _bdReParameters =
        new(120, 15, 33, 46, 48, 116, 44, 46, 11826176, new SKColor(0x20, 0x20, 0x20));
    static readonly DiscParameters _hddvdParameters =
        new(120, 15, 33, 44, 48, 116, 0, 0, 7864320, new SKColor(0x6f, 0x0A, 0xCA));
    static readonly DiscParameters _hddvdRParameters =
        new(120, 15, 33, 46, 48, 116, 44, 46, 7864320, new SKColor(0xff, 0x91, 0x00));
    static readonly DiscParameters _hddvdRwParameters =
        new(120, 15, 33, 46, 48, 116, 44, 46, 7864320, new SKColor(0x30, 0x30, 0x30));
    static readonly DiscParameters _umdParameters =
        new(60, 11.025f, 16.2f, 28, 32, 56, 0, 0, 471872, new SKColor(0x6f, 0x0A, 0xCA));
    static readonly DiscParameters _gdParameters = new(120, 15, 33, 46, 50, 116, 0, 0, 550000, SKColors.Silver);
    static readonly DiscParameters _gdRecordableParameters =
        new(120, 15, 33, 46, 50, 116, 45, 46, 550000, new SKColor(0xBD, 0xA0, 0x00));
    readonly SKCanvas      _canvas;
    readonly bool          _gdrom;
    readonly List<SKPoint> _leadInPoints;
    readonly long          _maxSector;
    readonly List<SKPoint> _points;
    readonly List<SKPoint> _pointsLowDensity;
    readonly List<SKPoint> _recordableInformationPoints;

    /// <summary>Initializes a spiral</summary>
    /// <param name="width">Width in pixels for the underlying bitmap</param>
    /// <param name="height">Height in pixels for the underlying bitmap</param>
    /// <param name="parameters">Disc parameters</param>
    /// <param name="lastSector">Last sector that will be drawn into the spiral</param>
    public Spiral(int width, int height, DiscParameters parameters, ulong lastSector)
    {
        if(parameters == _gdParameters ||
           parameters == _gdRecordableParameters)
            _gdrom = true;

        // GD-ROM LD area ends at 29mm, HD area starts at 30mm radius

        Bitmap  = new SKBitmap(width, height);
        _canvas = new SKCanvas(Bitmap);

        var center = new SKPoint(width / 2f, height / 2f);

        int smallerDimension = Math.Min(width, height) - 8;

        // Get other diameters
        float centerHoleDiameter = smallerDimension * parameters.CenterHole      / parameters.DiscDiameter;
        float clampingDiameter   = smallerDimension * parameters.ClampingMinimum / parameters.DiscDiameter;

        float informationAreaStartDiameter =
            smallerDimension * parameters.InformationAreaStart / parameters.DiscDiameter;

        float leadInEndDiameter          = smallerDimension * parameters.LeadInEnd          / parameters.DiscDiameter;
        float informationAreaEndDiameter = smallerDimension * parameters.InformationAreaEnd / parameters.DiscDiameter;

        float recordableAreaStartDiameter =
            smallerDimension * parameters.RecordableInformationStart / parameters.DiscDiameter;

        float recordableAreaEndDiameter =
            smallerDimension * parameters.RecordableInformationEnd / parameters.DiscDiameter;

        _maxSector = parameters.NominalMaxSectors;
        long lastSector1 = (long)lastSector;

        // If the dumped media is overburnt
        if(lastSector1 > _maxSector)
            _maxSector = lastSector1;

        // Ensure the disc hole is not painted over
        var clipPath = new SKPath();
        clipPath.AddCircle(center.X, center.Y, centerHoleDiameter / 2);
        _canvas.ClipPath(clipPath, SKClipOperation.Difference);

        // Paint CD
        _canvas.DrawCircle(center, smallerDimension / 2f, new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            Color = parameters.DiscColor
        });

        // Draw out border of disc
        _canvas.DrawCircle(center, smallerDimension / 2f, new SKPaint
        {
            Style       = SKPaintStyle.Stroke,
            Color       = SKColors.Black,
            StrokeWidth = 4
        });

        // Draw disc hole border
        _canvas.DrawCircle(center, centerHoleDiameter / 2f, new SKPaint
        {
            Style       = SKPaintStyle.Stroke,
            Color       = SKColors.Black,
            StrokeWidth = 4
        });

        // Draw clamping area
        _canvas.DrawCircle(center, clampingDiameter / 2f, new SKPaint
        {
            Style       = SKPaintStyle.Stroke,
            Color       = SKColors.Gray,
            StrokeWidth = 4
        });

        // Some trigonometry thing I do not understand fully but it controls the space between the spiral turns
        const float a = 1f;

        // Draw the Lead-In
        _leadInPoints = GetSpiralPoints(center, informationAreaStartDiameter / 2, leadInEndDiameter / 2,
                                        _gdrom ? a : a * 1.5f);

        var path = new SKPath();

        path.MoveTo(_leadInPoints[0]);

        foreach(SKPoint point in _leadInPoints)
            path.LineTo(point);

        _canvas.DrawPath(path, new SKPaint
        {
            Style       = SKPaintStyle.Stroke,
            Color       = SKColors.LightGray,
            StrokeWidth = 2
        });

        // If there's a recordable information area, get its points
        if(recordableAreaEndDiameter > 0 &&
           recordableAreaStartDiameter > 0)
            _recordableInformationPoints = GetSpiralPoints(center, recordableAreaStartDiameter / 2,
                                                           recordableAreaEndDiameter / 2, _gdrom ? a : a * 1.5f);

        if(_gdrom)
        {
            float lowDensityEndDiameter    = smallerDimension * 29 * 2 / parameters.DiscDiameter;
            float highDensityStartDiameter = smallerDimension * 30 * 2 / parameters.DiscDiameter;

            _pointsLowDensity = GetSpiralPoints(center, leadInEndDiameter / 2, lowDensityEndDiameter / 2, a * 1.5f);
            _points = GetSpiralPoints(center, highDensityStartDiameter / 2, informationAreaEndDiameter / 2, a);
        }
        else
            _points = GetSpiralPoints(center, leadInEndDiameter / 2, informationAreaEndDiameter / 2, a);

        path = new SKPath();

        if(_gdrom && _pointsLowDensity is not null)
        {
            path.MoveTo(_pointsLowDensity[0]);

            foreach(SKPoint point in _pointsLowDensity)
                path.LineTo(point);

            _canvas.DrawPath(path, new SKPaint
            {
                Style       = SKPaintStyle.Stroke,
                Color       = SKColors.Gray,
                StrokeWidth = 2
            });
        }

        path.MoveTo(_points[0]);

        long pointsPerSector;
        long sectorsPerPoint;

        if(_gdrom)
        {
            pointsPerSector = _points.Count / (_maxSector - 45000);
            sectorsPerPoint = (_maxSector                 - 45000) / _points.Count;

            if((_maxSector - 45000) % _points.Count > 0)
                sectorsPerPoint++;
        }
        else
        {
            pointsPerSector = _points.Count / _maxSector;
            sectorsPerPoint = _maxSector    / _points.Count;

            if(_maxSector % _points.Count > 0)
                sectorsPerPoint++;
        }

        long lastPoint;

        if(_gdrom)
            lastSector1 -= 45000;

        if(pointsPerSector > 0)
            lastPoint = lastSector1 * pointsPerSector;
        else
            lastPoint = lastSector1 / sectorsPerPoint;

        for(int index = 0; index < lastPoint; index++)
        {
            SKPoint point = _points[index];
            path.LineTo(point);
        }

        _canvas.DrawPath(path, new SKPaint
        {
            Style       = SKPaintStyle.Stroke,
            Color       = SKColors.Gray,
            StrokeWidth = 2
        });
    }

    public SKBitmap Bitmap { get; }

    public static DiscParameters DiscParametersFromMediaType(MediaType mediaType, bool smallDisc = false) =>
        mediaType switch
        {
            MediaType.CD          => _cdParameters,
            MediaType.CDDA        => _cdParameters,
            MediaType.CDG         => _cdParameters,
            MediaType.CDEG        => _cdParameters,
            MediaType.CDI         => _cdParameters,
            MediaType.CDIREADY    => _cdParameters,
            MediaType.CDROM       => _cdParameters,
            MediaType.CDROMXA     => _cdParameters,
            MediaType.CDPLUS      => _cdParameters,
            MediaType.CDMO        => _cdParameters,
            MediaType.VCD         => _cdParameters,
            MediaType.SVCD        => _cdParameters,
            MediaType.PCD         => _cdParameters,
            MediaType.DTSCD       => _cdParameters,
            MediaType.CDMIDI      => _cdParameters,
            MediaType.CDV         => _cdParameters,
            MediaType.CDR         => _cdRecordableParameters,
            MediaType.CDRW        => _cdRewritableParameters,
            MediaType.CDMRW       => _cdRewritableParameters,
            MediaType.SACD        => _dvdParameters,
            MediaType.DVDROM      => smallDisc ? _dvdParameters : _dvdParameters80,
            MediaType.DVDR        => smallDisc ? _dvdRParameters : _dvdRParameters80,
            MediaType.DVDRW       => smallDisc ? _dvdRwParameters : _dvdRwParameters80,
            MediaType.DVDPR       => smallDisc ? _dvdPlusRParameters : _dvdPlusRParameters80,
            MediaType.DVDPRW      => smallDisc ? _dvdPlusRwParameters : _dvdPlusRwParameters80,
            MediaType.DVDPRWDL    => smallDisc ? _dvdPlusRwParameters : _dvdPlusRwParameters80,
            MediaType.DVDRDL      => smallDisc ? _dvdRParameters : _dvdRParameters80,
            MediaType.DVDPRDL     => smallDisc ? _dvdPlusRParameters : _dvdPlusRParameters80,
            MediaType.DVDRWDL     => smallDisc ? _dvdRwParameters : _dvdRwParameters80,
            MediaType.PS1CD       => _ps1CdParameters,
            MediaType.PS2CD       => _ps2CdParameters,
            MediaType.PS2DVD      => _dvdParameters,
            MediaType.PS3DVD      => _dvdParameters,
            MediaType.XGD         => _dvdParameters,
            MediaType.XGD2        => _dvdParameters,
            MediaType.XGD3        => _dvdParameters,
            MediaType.XGD4        => _bdParameters,
            MediaType.MEGACD      => _cdParameters,
            MediaType.SATURNCD    => _cdParameters,
            MediaType.MilCD       => _cdParameters,
            MediaType.SuperCDROM2 => _cdParameters,
            MediaType.JaguarCD    => _cdParameters,
            MediaType.ThreeDO     => _cdParameters,
            MediaType.PCFX        => _cdParameters,
            MediaType.NeoGeoCD    => _cdParameters,
            MediaType.CDTV        => _cdParameters,
            MediaType.CD32        => _cdParameters,
            MediaType.Nuon        => _dvdParameters,
            MediaType.GOD         => _dvdParameters80,
            MediaType.WOD         => _dvdParameters,
            MediaType.Pippin      => _cdParameters,
            MediaType.DDCD        => _ddcdParameters,
            MediaType.DDCDR       => _ddcdRecordableParameters,
            MediaType.DDCDRW      => _ddcdRewritableParameters,
            MediaType.BDROM       => _bdParameters,
            MediaType.BDR         => _bdRParameters,
            MediaType.BDRE        => _bdReParameters,
            MediaType.PS3BD       => _bdParameters,
            MediaType.PS4BD       => _bdParameters,
            MediaType.PS5BD       => _bdParameters,
            MediaType.HDDVDROM    => _hddvdParameters,
            MediaType.HDDVDR      => _hddvdRParameters,
            MediaType.HDDVDRDL    => _hddvdRParameters,
            MediaType.HDDVDRW     => _hddvdRwParameters,
            MediaType.HDDVDRWDL   => _hddvdRwParameters,
            MediaType.CBHD        => _hddvdParameters,
            MediaType.FMTOWNS     => _cdParameters,
            MediaType.DVDDownload => _dvdParameters,
            MediaType.CVD         => _cdParameters,
            MediaType.Playdia     => _cdParameters,
            MediaType.WUOD        => _bdParameters,
            MediaType.UMD         => _umdParameters,
            MediaType.GDROM       => _gdParameters,
            MediaType.GDR         => _gdRecordableParameters,
            _                     => null
        };

    /// <summary>Paints the segment of the spiral that corresponds to the specified sector in green</summary>
    /// <param name="sector">Sector</param>
    public void PaintSectorGood(ulong sector) => PaintSector(sector, SKColors.Green);

    /// <summary>Paints the segment of the spiral that corresponds to the specified sector in red</summary>
    /// <param name="sector">Sector</param>
    public void PaintSectorBad(ulong sector) => PaintSector(sector, SKColors.Red);

    /// <summary>Paints the segment of the spiral that corresponds to the specified sector in yellow</summary>
    /// <param name="sector">Sector</param>
    public void PaintSectorUnknown(ulong sector) => PaintSector(sector, SKColors.Yellow);

    /// <summary>Paints the segment of the spiral that corresponds to the specified sector in gray</summary>
    /// <param name="sector">Sector</param>
    public void PaintSectorUndumped(ulong sector) => PaintSector(sector, SKColors.Gray);

    /// <summary>Paints the segment of the spiral that corresponds to the information specific to recordable discs in green</summary>
    public void PaintRecordableInformationGood()
    {
        if(_recordableInformationPoints is null)
            return;

        var path = new SKPath();

        path.MoveTo(_recordableInformationPoints[0]);

        foreach(SKPoint point in _recordableInformationPoints)
            path.LineTo(point);

        _canvas.DrawPath(path, new SKPaint
        {
            Style       = SKPaintStyle.Stroke,
            Color       = SKColors.Green,
            StrokeWidth = 2
        });
    }

    /// <summary>Paints the segment of the spiral that corresponds to the specified sector in the specified color</summary>
    /// <param name="sector">Sector</param>
    /// <param name="color">Color to paint the segment</param>
    public void PaintSector(ulong sector, SKColor color)
    {
        long          pointsPerSector;
        long          sectorsPerPoint;
        List<SKPoint> points = _gdrom && sector <= 45000 ? _pointsLowDensity : _points;

        if(_gdrom)
        {
            if(sector <= 45000)
            {
                pointsPerSector = points.Count / 45000;
                sectorsPerPoint = 45000        / points.Count;

                if(45000 % points.Count > 0)
                    sectorsPerPoint++;
            }
            else
            {
                sector          -= 45000;
                pointsPerSector =  points.Count / (_maxSector - 45000);
                sectorsPerPoint =  (_maxSector                - 45000) / points.Count;

                if((_maxSector - 45000) % points.Count > 0)
                    sectorsPerPoint++;
            }
        }
        else
        {
            pointsPerSector = points.Count / _maxSector;
            sectorsPerPoint = _maxSector   / points.Count;

            if(_maxSector % points.Count > 0)
                sectorsPerPoint++;
        }

        var paint = new SKPaint
        {
            Style       = SKPaintStyle.Stroke,
            Color       = color,
            StrokeWidth = 2
        };

        var path = new SKPath();

        if(pointsPerSector > 0)
        {
            long firstPoint = (long)sector * pointsPerSector;

            path.MoveTo(points[(int)firstPoint]);

            for(int i = (int)firstPoint; i < firstPoint + pointsPerSector; i++)
                path.LineTo(points[i]);

            _canvas.DrawPath(path, paint);

            return;
        }

        long point = (long)sector / sectorsPerPoint;

        if(point == 0)
        {
            path.MoveTo(points[0]);
            path.LineTo(points[1]);
        }
        else if(point >= points.Count - 1)
        {
            path.MoveTo(points[^2]);
            path.LineTo(points[^1]);
        }
        else
        {
            path.MoveTo(points[(int)point]);
            path.LineTo(points[(int)point + 1]);
        }

        _canvas.DrawPath(path, paint);
    }

    /// <summary>
    ///     Paints the segment of the spiral that corresponds to the specified sector of the Lead-In in the specified
    ///     color
    /// </summary>
    /// <param name="sector">Sector</param>
    /// <param name="color">Color to paint the segment in</param>
    /// <param name="leadInSize">Total size of the lead-in in sectors</param>
    public void PaintLeadInSector(ulong sector, SKColor color, int leadInSize)
    {
        long pointsPerSector = _leadInPoints.Count / leadInSize;
        long sectorsPerPoint = leadInSize          / _leadInPoints.Count;

        if(leadInSize % _leadInPoints.Count > 0)
            sectorsPerPoint++;

        var paint = new SKPaint
        {
            Style       = SKPaintStyle.Stroke,
            Color       = color,
            StrokeWidth = 2
        };

        var path = new SKPath();

        if(pointsPerSector > 0)
        {
            long firstPoint = (long)sector * pointsPerSector;

            path.MoveTo(_leadInPoints[(int)firstPoint]);

            for(int i = (int)firstPoint; i < firstPoint + pointsPerSector; i++)
                path.LineTo(_leadInPoints[i]);

            _canvas.DrawPath(path, paint);

            return;
        }

        long point = (long)sector / sectorsPerPoint;

        if(point == 0)
        {
            path.MoveTo(_leadInPoints[0]);
            path.LineTo(_leadInPoints[1]);
        }
        else if(point >= _leadInPoints.Count - 1)
        {
            path.MoveTo(_leadInPoints[^2]);
            path.LineTo(_leadInPoints[^1]);
        }
        else
        {
            path.MoveTo(_leadInPoints[(int)point]);
            path.LineTo(_leadInPoints[(int)point + 1]);
        }

        _canvas.DrawPath(path, paint);
    }

    /// <summary>Writes the spiral bitmap as a PNG into the specified stream</summary>
    /// <param name="stream">Stream that will receive the spiral bitmap</param>
    public void WriteToStream(Stream stream)
    {
        var    image = SKImage.FromBitmap(Bitmap);
        SKData data  = image.Encode();
        data.SaveTo(stream);
    }

    /// <summary>Gets all the points that are needed to draw a spiral with the specified parameters</summary>
    /// <param name="center">Center of the spiral start</param>
    /// <param name="minRadius">Minimum radius before which the spiral must have no points</param>
    /// <param name="maxRadius">Radius at which the spiral will end</param>
    /// <param name="A">TODO: Something trigonometry something something...</param>
    /// <returns>List of points to draw the specified spiral</returns>
    static List<SKPoint> GetSpiralPoints(SKPoint center, float minRadius, float maxRadius, float A)
    {
        // Get the points.
        List<SKPoint> points = new();
        const float   dtheta = (float)(0.5f * Math.PI / 180);

        for(float theta = 0;; theta += dtheta)
        {
            // Calculate r.
            float r = A * theta;

            if(r < minRadius)
                continue;

            // Convert to Cartesian coordinates.
            float x = (float)(r * Math.Cos(theta));
            float y = (float)(r * Math.Sin(theta));

            // Center.
            x += center.X;
            y += center.Y;

            // Create the point.
            points.Add(new SKPoint(x, y));

            // If we have gone far enough, stop.
            if(r > maxRadius)
                break;
        }

        return points;
    }

    /// <summary>Defines the physical disc parameters</summary>
    /// <param name="DiscDiameter">Diameter of the whole disc</param>
    /// <param name="CenterHole">Diameter of the hole at the center</param>
    /// <param name="ClampingMinimum">Diameter of the clamping area</param>
    /// <param name="InformationAreaStart">Diameter at which the information area starts</param>
    /// <param name="LeadInEnd">Diameter at which the Lead-In starts</param>
    /// <param name="InformationAreaEnd">Diameter at which the information area ends</param>
    /// <param name="RecordableInformationStart">Diameter at which the information specific to recordable media starts</param>
    /// <param name="RecordableInformationEnd">Diameter at which the information specific to recordable media starts</param>
    /// <param name="NominalMaxSectors">Number of maximum sectors, for discs following the specifications</param>
    /// <param name="DiscColor">Typical disc color</param>
    public sealed record DiscParameters(float DiscDiameter, float CenterHole, float ClampingMinimum,
                                        float InformationAreaStart, float LeadInEnd, float InformationAreaEnd,
                                        float RecordableInformationStart, float RecordableInformationEnd,
                                        int NominalMaxSectors, SKColor DiscColor);
}