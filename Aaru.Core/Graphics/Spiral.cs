// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Spiral.cs
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Decoders.Bluray;
using Aaru.Decoders.DVD;
using SkiaSharp;

namespace Aaru.Core.Graphics;

// TODO: HD DVD sectors are a guess
public sealed class Spiral : IMediaGraph
{
    static readonly DiscParameters _cdParameters = new(120, 15, 33, 46, 50, 116, 0, 0, 360000, SKColors.Silver);
    static readonly DiscParameters _cdRecordableParameters =
        new(120, 15, 33, 46, 50, 116, 45, 46, 360000, new SKColor(0xBD, 0xA0, 0x00));
    static readonly DiscParameters _cdRewritableParameters =
        new(120, 15, 33, 46, 50, 116, 45, 46, 360000, new SKColor(0x50, 0x50, 0x50));
    static readonly DiscParameters _ddcdParameters = new(120, 15, 33, 46, 50, 116, 0, 0, 666000, SKColors.Silver);

    // VideoNow discs are CDs of a smaller diameter: same hole, clamping area, information area start and lead-in, the
    // same margin at the rim, and a capacity proportional to the recorded area, (end² - 50²) / (116² - 50²) of a CD's
    static readonly DiscParameters _videoNowParameters = new(85, 15, 33, 46, 50, 81, 0, 0, 133439, SKColors.Silver);
    static readonly DiscParameters _videoNowColorParameters =
        new(100, 15, 33, 46, 50, 96, 0, 0, 220679, SKColors.Silver);
    static readonly DiscParameters _ddcdRecordableParameters =
        new(120, 15, 33, 46, 50, 116, 45, 46, 666000, new SKColor(0xBD, 0xA0, 0x00));
    static readonly DiscParameters _ddcdRewritableParameters =
        new(120, 15, 33, 46, 50, 116, 45, 46, 666000, new SKColor(0x50, 0x50, 0x50));
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
    static readonly DiscParameters _dvdParameters   = new(120, 15, 33, 44, 48, 116, 0, 0, 2294922, SKColors.Silver);
    static readonly DiscParameters _dvdParameters80 = new(80, 15, 33, 44, 48, 76, 0, 0, 714544, SKColors.Silver);
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
    static readonly DiscParameters _bdRxlParameters =
        new(120, 15, 33, 46, 48, 116, 44, 46, 48878592, new SKColor(0x20, 0x20, 0x20));
    static readonly DiscParameters _hddvdParameters =
        new(120, 15, 33, 44, 48, 116, 0, 0, 7864320, new SKColor(0x6f, 0x0A, 0xCA));
    static readonly DiscParameters _hddvdRParameters =
        new(120, 15, 33, 46, 48, 116, 44, 46, 7864320, new SKColor(0xff, 0x91, 0x00));
    static readonly DiscParameters _hddvdRwParameters =
        new(120, 15, 33, 46, 48, 116, 44, 46, 7864320, new SKColor(0x30, 0x30, 0x30));
    static readonly DiscParameters _umdParameters = new(60, 11.025f, 16.2f, 28, 32, 56, 0, 0, 471872, SKColors.Silver);
    static readonly DiscParameters _gdParameters  = new(120, 15, 33, 46, 50, 116, 0, 0, 550000, SKColors.Silver);
    static readonly DiscParameters _gdRecordableParameters =
        new(120, 15, 33, 46, 50, 116, 45, 46, 550000, new SKColor(0xBD, 0xA0, 0x00));
    readonly SKCanvas        _canvas;
    readonly float[]         _dataMaxRadius;
    readonly float[]         _dataMinRadius;
    readonly bool            _gdrom;
    readonly long            _layerBreak; // -1 when single-layer
    readonly long[]          _layerMaxSector;
    readonly List<SKPoint>[] _leadInPoints;
    readonly int             _numLayers;
    readonly List<SKPoint>[] _points;
    readonly List<SKPoint>[] _recordableInformationPoints;
    float                    _lowDensityMaxRadius;
    float                    _lowDensityMinRadius;
    List<SKPoint>            _pointsLowDensity;

    /// <summary>Initializes a spiral</summary>
    /// <param name="width">Width in pixels for the underlying bitmap (per disc; doubled internally when dual-layer)</param>
    /// <param name="height">Height in pixels for the underlying bitmap</param>
    /// <param name="parameters">Disc parameters</param>
    /// <param name="lastSector">Last sector that will be drawn into the spiral</param>
    /// <param name="layerBreak">
    ///     When not <see langword="null" />, enables dual-layer mode: layer 0 holds sectors
    ///     <c>0..layerBreak</c> and layer 1 holds <c>layerBreak+1..lastSector</c>. Each layer's full spiral spans
    ///     <c>0..NominalMaxSectors</c>.
    /// </param>
    /// <param name="oppositeTrackPath">
    ///     When <see langword="true" /> (default, OTP per ECMA-267), layer 1 is drawn outside-in
    ///     (sector 0 at the outer radius). When <see langword="false" /> (PTP), layer 1 is drawn inside-out like layer 0.
    /// </param>
    public Spiral(int  width, int height, DiscParameters parameters, ulong lastSector, ulong? layerBreak = null,
                  bool oppositeTrackPath = true)
    {
        if(parameters == _gdParameters || parameters == _gdRecordableParameters) _gdrom = true;

        // GD-ROM is physically single-layer; reject dual-layer combo
        if(_gdrom) layerBreak = null;

        _numLayers  = layerBreak is not null ? 2 : 1;
        _layerBreak = layerBreak is not null ? (long)(ulong)layerBreak : -1;

        int bitmapWidth = _numLayers == 2 ? width * 2 : width;
        Bitmap  = new SKBitmap(bitmapWidth, height);
        _canvas = new SKCanvas(Bitmap);

        _points                      = new List<SKPoint>[_numLayers];
        _leadInPoints                = new List<SKPoint>[_numLayers];
        _recordableInformationPoints = new List<SKPoint>[_numLayers];
        _dataMinRadius               = new float[_numLayers];
        _dataMaxRadius               = new float[_numLayers];
        _layerMaxSector              = new long[_numLayers];

        // Compute per-layer last sector (each layer's spiral spans 0..NominalMaxSectors)
        long layer0Last = _numLayers == 2 ? Math.Min((long)lastSector, _layerBreak) : (long)lastSector;
        long layer1Last = _numLayers == 2 ? (long)lastSector - _layerBreak - 1 : -1;

        var leftCenter  = new SKPoint(width / 2f,         height / 2f);
        var rightCenter = new SKPoint(width + width / 2f, height / 2f);

        DrawLayer(0, leftCenter, width, height, parameters, layer0Last, false);

        if(_numLayers == 2) DrawLayer(1, rightCenter, width, height, parameters, layer1Last, oppositeTrackPath);
    }

    public SKBitmap Bitmap { get; }

    /// <summary>Draws the background, lead-in (layer 0 only), and undumped spiral for the specified layer.</summary>
    void DrawLayer(int layerIdx, SKPoint center, int width, int height, DiscParameters parameters, long layerLastSector,
                   bool reverse)
    {
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

        long layerMaxSector = parameters.NominalMaxSectors;

        // Per-layer overburn
        if(layerLastSector > layerMaxSector) layerMaxSector = layerLastSector;

        _layerMaxSector[layerIdx] = layerMaxSector;

        _canvas.Save();

        // Clip drawing to this disc's half of the bitmap so the other layer is untouched
        var halfRect = new SKRect(center.X - width  / 2f,
                                  center.Y - height / 2f,
                                  center.X + width  / 2f,
                                  center.Y + height / 2f);

        var halfClip = new SKPath();
        halfClip.AddRect(halfRect);
        _canvas.ClipPath(halfClip);

        // Ensure the disc hole is not painted over
        var clipPath = new SKPath();
        clipPath.AddCircle(center.X, center.Y, centerHoleDiameter / 2);
        _canvas.ClipPath(clipPath, SKClipOperation.Difference);

        // Paint disc body
        _canvas.DrawCircle(center,
                           smallerDimension / 2f,
                           new SKPaint
                           {
                               Style = SKPaintStyle.StrokeAndFill,
                               Color = parameters.DiscColor
                           });

        // Draw outer border of disc
        _canvas.DrawCircle(center,
                           smallerDimension / 2f,
                           new SKPaint
                           {
                               Style       = SKPaintStyle.Stroke,
                               Color       = SKColors.Black,
                               StrokeWidth = 4
                           });

        // Draw disc hole border
        _canvas.DrawCircle(center,
                           centerHoleDiameter / 2f,
                           new SKPaint
                           {
                               Style       = SKPaintStyle.Stroke,
                               Color       = SKColors.Black,
                               StrokeWidth = 4
                           });

        // Draw clamping area
        _canvas.DrawCircle(center,
                           clampingDiameter / 2f,
                           new SKPaint
                           {
                               Style       = SKPaintStyle.Stroke,
                               Color       = SKColors.Gray,
                               StrokeWidth = 4
                           });

        // Controls distance between spiral turns. Smaller values = tighter spiral with more revolutions,
        // which makes ring-shaped error patterns (e.g. ring protections) visible as rings.
        // At 0.53 with 1px stroke, drawn track pitch ~3.3px with ~2.3px gap between lines,
        // giving ~82 revolutions for a CD on a 1000px image. A typical ring protection
        // (~3072 consecutive bad sectors) spans ~0.70 drawn revolutions = a visible ring.
        const float a = 0.53f;

        // Draw the Lead-In (layer 0 only)
        if(layerIdx == 0)
        {
            _leadInPoints[0] = GetSpiralPoints(center,
                                               informationAreaStartDiameter / 2,
                                               leadInEndDiameter            / 2,
                                               _gdrom ? a * 1.5f : a);

            var leadInPath = new SKPath();
            leadInPath.MoveTo(_leadInPoints[0][0]);

            foreach(SKPoint point in _leadInPoints[0]) leadInPath.LineTo(point);
            _leadInPoints[0].Reverse();

            _canvas.DrawPath(leadInPath,
                             new SKPaint
                             {
                                 Style       = SKPaintStyle.Stroke,
                                 Color       = SKColors.LightGray,
                                 StrokeWidth = 1
                             });

            // If there's a recordable information area, get its points (layer 0 only)
            if(recordableAreaEndDiameter > 0 && recordableAreaStartDiameter > 0)
            {
                _recordableInformationPoints[0] = GetSpiralPoints(center,
                                                                  recordableAreaStartDiameter / 2,
                                                                  recordableAreaEndDiameter   / 2,
                                                                  _gdrom ? a : a * 1.5f);
            }
        }

        if(_gdrom && layerIdx == 0)
        {
            float lowDensityEndDiameter    = smallerDimension * 29 * 2 / parameters.DiscDiameter;
            float highDensityStartDiameter = smallerDimension * 30 * 2 / parameters.DiscDiameter;

            _lowDensityMinRadius = leadInEndDiameter          / 2;
            _lowDensityMaxRadius = lowDensityEndDiameter      / 2;
            _dataMinRadius[0]    = highDensityStartDiameter   / 2;
            _dataMaxRadius[0]    = informationAreaEndDiameter / 2;

            _pointsLowDensity = GetSpiralPoints(center, _lowDensityMinRadius, _lowDensityMaxRadius, a * 1.5f);
            _points[0]        = GetSpiralPoints(center, _dataMinRadius[0],    _dataMaxRadius[0],    a);
        }
        else
        {
            _dataMinRadius[layerIdx] = leadInEndDiameter          / 2;
            _dataMaxRadius[layerIdx] = informationAreaEndDiameter / 2;

            _points[layerIdx] = GetSpiralPoints(center, _dataMinRadius[layerIdx], _dataMaxRadius[layerIdx], a);

            // For OTP layer 1, reverse point list so index 0 sits at the outer radius
            // (sector 0 of layer 1 lies at the outer radius where layer 0 ended)
            if(reverse) _points[layerIdx].Reverse();
        }

        // Draw GD-ROM low-density undumped spiral (layer 0 only, single-layer mode)
        if(_gdrom && layerIdx == 0 && _pointsLowDensity is not null)
        {
            var ldPath = new SKPath();
            ldPath.MoveTo(_pointsLowDensity[0]);

            foreach(SKPoint point in _pointsLowDensity) ldPath.LineTo(point);

            _canvas.DrawPath(ldPath,
                             new SKPaint
                             {
                                 Style       = SKPaintStyle.Stroke,
                                 Color       = SKColors.Gray,
                                 StrokeWidth = 1
                             });
        }

        // Draw undumped data spiral up to layerLastSector
        long layerLast    = layerLastSector;
        long effectiveMax = layerMaxSector;

        if(_gdrom)
        {
            layerLast    -= 45000;
            effectiveMax -= 45000;
        }

        if(layerLast >= 0 && effectiveMax > 0 && _points[layerIdx].Count > 0)
        {
            long lastPoint = ClvSectorToPoint(layerLast,
                                              effectiveMax,
                                              _points[layerIdx].Count,
                                              _dataMinRadius[layerIdx],
                                              _dataMaxRadius[layerIdx]) +
                             1;

            if(lastPoint > _points[layerIdx].Count) lastPoint = _points[layerIdx].Count;

            var dataPath = new SKPath();
            dataPath.MoveTo(_points[layerIdx][0]);

            for(var index = 0; index < lastPoint; index++)
            {
                SKPoint point = _points[layerIdx][index];
                dataPath.LineTo(point);
            }

            _canvas.DrawPath(dataPath,
                             new SKPaint
                             {
                                 Style       = SKPaintStyle.Stroke,
                                 Color       = SKColors.Gray,
                                 StrokeWidth = 1
                             });
        }

        _canvas.Restore();
    }

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
            MediaType.DVDROM      => smallDisc ? _dvdParameters80 : _dvdParameters,
            MediaType.DVDR        => smallDisc ? _dvdRParameters80 : _dvdRParameters,
            MediaType.DVDRW       => smallDisc ? _dvdRwParameters80 : _dvdRwParameters,
            MediaType.DVDPR       => smallDisc ? _dvdPlusRParameters80 : _dvdPlusRParameters,
            MediaType.DVDPRW      => smallDisc ? _dvdPlusRwParameters80 : _dvdPlusRwParameters,
            MediaType.DVDPRWDL    => smallDisc ? _dvdPlusRwParameters80 : _dvdPlusRwParameters,
            MediaType.DVDRDL      => smallDisc ? _dvdRParameters80 : _dvdRParameters,
            MediaType.DVDPRDL     => smallDisc ? _dvdPlusRParameters80 : _dvdPlusRParameters,
            MediaType.DVDRWDL     => smallDisc ? _dvdRwParameters80 : _dvdRwParameters,
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
            MediaType.VideoNow      => _videoNowParameters,
            MediaType.VideoNowColor => _videoNowColorParameters,
            MediaType.VideoNowXp    => _videoNowColorParameters,
            MediaType.DDCD        => _ddcdParameters,
            MediaType.DDCDR       => _ddcdRecordableParameters,
            MediaType.DDCDRW      => _ddcdRewritableParameters,
            MediaType.BDROM       => _bdParameters,
            MediaType.BDR         => _bdRParameters,
            MediaType.BDRE        => _bdReParameters,
            MediaType.BDRXL       => _bdRxlParameters,
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

    /// <summary>
    ///     Extracts the dual-layer layer-break and track-path from a DVD PFI structure. Returns
    ///     <c>(null, true)</c> for single-layer discs. The track-path flag (PFI byte 6 bit 4, per
    ///     ECMA-267 §13.2) distinguishes OTP (<c>TrackPath = 1</c>) from PTP (<c>TrackPath = 0</c>) and is mapped
    ///     directly onto the returned <c>oppositeTrackPath</c> boolean.
    /// </summary>
    /// <param name="pfi">Decoded DVD PFI, or <see langword="null" /> if unavailable</param>
    /// <returns>
    ///     <c>layerBreak</c>: sector (relative to LBA 0) where layer 0 ends; layer 1 starts at
    ///     <c>layerBreak + 1</c>. <c>oppositeTrackPath</c>: <see langword="true" /> when PFI reports OTP,
    ///     <see langword="false" /> when PFI reports PTP.
    /// </returns>
    public static (ulong? layerBreak, bool oppositeTrackPath) LayerBreakFromPfi(PFI.PhysicalFormatInformation? pfi)
    {
        // Default for single-layer discs: no layer break, OTP assumed (unused when layerBreak is null)
        if(pfi is null) return (null, true);

        var decoded = (PFI.PhysicalFormatInformation)pfi;

        // PFI.Layers encodes (NumberOfLayers - 1), so Layers == 1 means a dual-layer disc.
        // Single- and triple-plus-layer discs are treated as single-layer by this visualization.
        if(decoded.Layers != 1) return (null, decoded.TrackPath);

        // Layer 0 end PSN and data area start PSN are physical sector numbers. Sector 0 of the image
        // corresponds to DataAreaStartPSN; layer break in image LBAs is the number of sectors in layer 0 minus one.
        if(decoded.Layer0EndPSN == 0 || decoded.Layer0EndPSN < decoded.DataAreaStartPSN)
            return (null, decoded.TrackPath);

        ulong layerBreak = decoded.Layer0EndPSN - decoded.DataAreaStartPSN;

        // ECMA-267 §13.2: PFI TrackPath field — 1b = Opposite Track Path, 0b = Parallel Track Path.
        bool oppositeTrackPath = decoded.TrackPath;

        return (layerBreak, oppositeTrackPath);
    }

    /// <summary>
    ///     Extracts the dual-layer layer-break from a Blu-ray DI structure. Blu-ray layered discs are always
    ///     OTP-like from the reading head's perspective, so <c>oppositeTrackPath</c> is always <see langword="true" />.
    /// </summary>
    /// <param name="di">Decoded BD DI, or <see langword="null" /> if unavailable</param>
    /// <param name="totalBlocks">Total blocks in the image; used to compute a midpoint layer-break fallback</param>
    /// <returns>Layer break LBA and track path, or <c>(null, true)</c> for single-layer</returns>
    public static (ulong? layerBreak, bool oppositeTrackPath) LayerBreakFromDi(
        DI.DiscInformation? di, ulong totalBlocks)
    {
        if(di?.Units is null || ((DI.DiscInformation)di).Units.Length == 0) return (null, true);

        byte layers = ((DI.DiscInformation)di).Units[0].Layers;

        if(layers <= 1) return (null, true);

        // BD DI does not expose per-layer sector count in an easily parsable way across BD-ROM/-R/-RE,
        // so we split the total block count evenly between layers as a visualization approximation.
        if(totalBlocks == 0) return (null, true);

        ulong layerSize  = totalBlocks / layers;
        ulong layerBreak = layerSize > 0 ? layerSize - 1 : 0;

        return (layerBreak, true);
    }

    void PaintSectors(ulong startingSector, uint length, SKColor color)
    {
        for(uint i = 0; i < length; i++) PaintSector(startingSector + i, color);
    }

    void PaintSectors(IEnumerable<ulong> sectors, SKColor color)
    {
        foreach(ulong sector in sectors) PaintSector(sector, color);
    }

    /// <summary>Paints the segment of the spiral that corresponds to the specified sector in the specified color</summary>
    /// <param name="sector">Sector</param>
    /// <param name="color">Color to paint the segment</param>
    void PaintSector(ulong sector, SKColor color)
    {
        List<SKPoint> points;
        float         minRadius, maxRadius;
        long          effectiveMaxSector;
        double        effectiveSector;

        if(_numLayers == 2)
        {
            int layerIdx;

            if((long)sector <= _layerBreak)
            {
                layerIdx        = 0;
                effectiveSector = sector;
            }
            else
            {
                layerIdx        = 1;
                effectiveSector = (double)sector - _layerBreak - 1;
            }

            points             = _points[layerIdx];
            minRadius          = _dataMinRadius[layerIdx];
            maxRadius          = _dataMaxRadius[layerIdx];
            effectiveMaxSector = _layerMaxSector[layerIdx];
        }
        else if(_gdrom && sector <= 45000)
        {
            points             = _pointsLowDensity;
            minRadius          = _lowDensityMinRadius;
            maxRadius          = _lowDensityMaxRadius;
            effectiveMaxSector = 45000;
            effectiveSector    = sector;
        }
        else if(_gdrom)
        {
            points             = _points[0];
            minRadius          = _dataMinRadius[0];
            maxRadius          = _dataMaxRadius[0];
            effectiveMaxSector = _layerMaxSector[0] - 45000;
            effectiveSector    = (double)sector     - 45000;
        }
        else
        {
            points             = _points[0];
            minRadius          = _dataMinRadius[0];
            maxRadius          = _dataMaxRadius[0];
            effectiveMaxSector = _layerMaxSector[0];
            effectiveSector    = sector;
        }

        if(points is null || points.Count == 0) return;

        long firstPoint = ClvSectorToPoint(effectiveSector, effectiveMaxSector, points.Count, minRadius, maxRadius);

        long lastPoint = ClvSectorToPoint(effectiveSector + 1, effectiveMaxSector, points.Count, minRadius, maxRadius);

        if(firstPoint >= points.Count - 1) return;

        // Ensure we draw at least one visible line segment (from firstPoint to firstPoint+1)
        long endPoint = Math.Clamp(Math.Max(lastPoint, firstPoint + 1), firstPoint + 1, points.Count - 1);

        var paint = new SKPaint
        {
            Style       = SKPaintStyle.Stroke,
            Color       = color,
            StrokeWidth = 1
        };

        var path = new SKPath();

        path.MoveTo(points[(int)firstPoint]);

        for(long i = firstPoint + 1; i <= endPoint; i++) path.LineTo(points[(int)i]);

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
        // Lead-In is always painted on layer 0
        List<SKPoint> leadIn = _leadInPoints[0];

        if(leadIn is null || leadIn.Count == 0) return;

        long pointsPerSector = leadIn.Count / leadInSize;
        long sectorsPerPoint = leadInSize   / leadIn.Count;

        if(leadInSize % leadIn.Count > 0) sectorsPerPoint++;

        var paint = new SKPaint
        {
            Style       = SKPaintStyle.Stroke,
            Color       = color,
            StrokeWidth = 1
        };

        var path = new SKPath();

        if(pointsPerSector > 0)
        {
            long firstPoint = (long)sector * pointsPerSector;
            long lastPoint  = Math.Min(firstPoint + pointsPerSector, leadIn.Count);

            if(firstPoint >= leadIn.Count) return;

            path.MoveTo(leadIn[(int)firstPoint]);

            for(var i = (int)firstPoint; i < lastPoint; i++) path.LineTo(leadIn[i]);

            _canvas.DrawPath(path, paint);

            return;
        }

        long point = (long)sector / sectorsPerPoint;

        if(point == 0)
        {
            path.MoveTo(leadIn[0]);
            path.LineTo(leadIn[1]);
        }
        else if(point >= leadIn.Count - 1)
        {
            path.MoveTo(leadIn[^2]);
            path.LineTo(leadIn[^1]);
        }
        else
        {
            path.MoveTo(leadIn[(int)point]);
            path.LineTo(leadIn[(int)point + 1]);
        }

        _canvas.DrawPath(path, paint);
    }

    /// <summary>
    ///     Paints the segment of the spiral that corresponds to the specified sector of a standard CD lead-in in the
    ///     specified color. Uses the standard CD lead-in size of approximately 2,500-3,000 sectors
    ///     (ECMA-130 specification: 46mm to 50mm radial distance with 1.6µm track pitch).
    /// </summary>
    /// <param name="sector">Sector within the lead-in (0-based, where 0 is LBA -150 equivalent)</param>
    /// <param name="color">Color to paint the segment in</param>
    public void PaintCdLeadInSector(long sector, SKColor color)
    {
        const int cdLeadInSize = 2750; // Approximate CD lead-in sectors (46-50mm at 1.6µm pitch, ~75 sectors/sec)
        PaintLeadInSector((ulong)(sector * -1), color, cdLeadInSize);
    }

    /// <summary>Maps a sector number to its spiral point index using CLV (Constant Linear Velocity) mapping</summary>
    /// <remarks>
    ///     On CLV optical discs, the relationship between sector number and radius is quadratic:
    ///     r² = r_min² + (sector/maxSector) × (r_max² - r_min²). This correctly positions sectors
    ///     at their physical radial location on the drawn spiral.
    /// </remarks>
    /// <param name="sector">Sector number to map</param>
    /// <param name="maxSector">Maximum sector number</param>
    /// <param name="numPoints">Total number of points in the spiral</param>
    /// <param name="minRadius">Minimum radius of the spiral (in pixels)</param>
    /// <param name="maxRadius">Maximum radius of the spiral (in pixels)</param>
    /// <returns>Point index corresponding to the sector</returns>
    static long ClvSectorToPoint(double sector, long maxSector, int numPoints, float minRadius, float maxRadius)
    {
        double fraction    = sector            / maxSector;
        double rSquaredMin = (double)minRadius * minRadius;
        double rSquaredMax = (double)maxRadius * maxRadius;
        double rTarget     = Math.Sqrt(rSquaredMin + fraction * (rSquaredMax - rSquaredMin));
        double pointFrac   = (rTarget - minRadius) / (maxRadius - minRadius);

        return Math.Clamp((long)(pointFrac * numPoints), 0, numPoints - 1);
    }

    /// <summary>Gets all the points that are needed to draw a spiral with the specified parameters</summary>
    /// <param name="center">Center of the spiral start</param>
    /// <param name="minRadius">Minimum radius before which the spiral must have no points</param>
    /// <param name="maxRadius">Radius at which the spiral will end</param>
    /// <param name="a">A constant that decides the position of a point on the spiral</param>
    /// <returns>List of points to draw the specified spiral</returns>
    static List<SKPoint> GetSpiralPoints(SKPoint center, float minRadius, float maxRadius, float a)
    {
        // Initialize a list to store the points of the spiral.
        List<SKPoint> points = [];
        const float   dtheta = (float)(0.5f * Math.PI / 180);

        for(float theta = 0;; theta += dtheta)
        {
            // Calculate r.
            float r = a * theta;

            if(r < minRadius) continue;

            // Converts polar coordinates (r,theta) to Cartesian (x,y)
            var x = (float)(r * Math.Cos(theta));
            var y = (float)(r * Math.Sin(theta));

            // Adjusts x and y by center coordinates
            x += center.X;
            y += center.Y;

            // Adds the newly calculated point to the list
            points.Add(new SKPoint(x, y));

            // Terminate the loop if we have reached the end of the spiral
            if(r > maxRadius) break;
        }

        return points;
    }

#region Nested type: DiscParameters

    /// <summary>Defines the physical disc parameters</summary>
    /// <param name="DiscDiameter">Diameter of the whole disc</param>
    /// <param name="CenterHole">Diameter of the hole at the center</param>
    /// <param name="ClampingMinimum">Diameter of the clamping area</param>
    /// <param name="InformationAreaStart">Diameter at which the information area starts</param>
    /// <param name="LeadInEnd">Diameter at which the Lead-In ends</param>
    /// <param name="InformationAreaEnd">Diameter at which the information area ends</param>
    /// <param name="RecordableInformationStart">Diameter at which the information specific to recordable media starts</param>
    /// <param name="RecordableInformationEnd">Diameter at which the information specific to recordable media starts</param>
    /// <param name="NominalMaxSectors">Number of maximum sectors, for discs following the specifications</param>
    /// <param name="DiscColor">Typical disc color</param>
    public sealed record DiscParameters
    (
        float   DiscDiameter,
        float   CenterHole,
        float   ClampingMinimum,
        float   InformationAreaStart,
        float   LeadInEnd,
        float   InformationAreaEnd,
        float   RecordableInformationStart,
        float   RecordableInformationEnd,
        int     NominalMaxSectors,
        SKColor DiscColor
    );

#endregion

#region IMediaGraph Members

    /// <inheritdoc />
    /// <summary>Paints the segment of the spiral that corresponds to the specified sector in green</summary>
    /// <param name="sector">Sector</param>
    public void PaintSectorGood(ulong sector) => PaintSector(sector, SKColors.Green);

    /// <inheritdoc />
    /// <summary>Paints the segment of the spiral that corresponds to the specified sector in red</summary>
    /// <param name="sector">Sector</param>
    public void PaintSectorBad(ulong sector) => PaintSector(sector, SKColors.Red);

    /// <inheritdoc />
    /// <summary>Paints the segment of the spiral that corresponds to the specified sector in yellow</summary>
    /// <param name="sector">Sector</param>
    public void PaintSectorUnknown(ulong sector) => PaintSector(sector, SKColors.Yellow);

    /// <inheritdoc />
    /// <summary>Paints the segment of the spiral that corresponds to the specified sector in gray</summary>
    /// <param name="sector">Sector</param>
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
    /// <summary>Paints the segment of the spiral that corresponds to the information specific to recordable discs in green</summary>
    public void PaintRecordableInformationGood()
    {
        // Layer 0 only
        List<SKPoint> recordable = _recordableInformationPoints[0];

        if(recordable is null) return;

        var path = new SKPath();

        path.MoveTo(recordable[0]);

        foreach(SKPoint point in recordable) path.LineTo(point);

        _canvas.DrawPath(path,
                         new SKPaint
                         {
                             Style       = SKPaintStyle.Stroke,
                             Color       = SKColors.Green,
                             StrokeWidth = 1
                         });
    }

    /// <inheritdoc />
    public bool TryLoadExisting(string path)
    {
        if(!File.Exists(path)) return false;

        using var existing = SKBitmap.Decode(path);

        if(existing is null || existing.Width != Bitmap.Width || existing.Height != Bitmap.Height) return false;

        _canvas.DrawBitmap(existing, 0, 0);

        return true;
    }

    /// <inheritdoc />
    public void WriteTo(string path)
    {
        using var fs = new FileStream(path, FileMode.Create);
        WriteTo(fs);
        fs.Close();
    }

    /// <inheritdoc />
    /// <summary>Writes the spiral bitmap as a PNG into the specified stream</summary>
    /// <param name="stream">Stream that will receive the spiral bitmap</param>
    public void WriteTo(Stream stream)
    {
        var    image = SKImage.FromBitmap(Bitmap);
        SKData data  = image.Encode();
        data.SaveTo(stream);
    }

#endregion
}