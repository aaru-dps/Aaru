// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Scanning.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines format for metadata.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Schemas;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Aaru.CommonTypes.AaruMetadata;

public class Scan
{
    public File                 File           { get; set; }
    public List<Checksum>       Checksums      { get; set; }
    public List<Scanner>        Scanner        { get; set; }
    public List<ScanProcessing> ScanProcessing { get; set; }
    public List<OCR>            OCR            { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Scan(ScanType cicm)
    {
        if(cicm is null) return null;

        var scan = new Scan
        {
            File = cicm.File
        };

        if(cicm.Checksums is not null)
        {
            scan.Checksums = [];

            foreach(Schemas.ChecksumType chk in cicm.Checksums) scan.Checksums.Add(chk);
        }

        if(cicm.Scanner is not null)
        {
            scan.Scanner = [];

            foreach(ScannerType scanner in cicm.Scanner) scan.Scanner.Add(scanner);
        }

        if(cicm.ScanProcessing is not null)
        {
            scan.ScanProcessing = [];

            foreach(ScanProcessingType processing in cicm.ScanProcessing) scan.ScanProcessing.Add(processing);
        }

        if(cicm.OCR is null) return scan;

        scan.OCR = [];

        foreach(OCRType ocr in cicm.OCR) scan.OCR.Add(ocr);

        return scan;
    }
}

public class Cover
{
    public File           File      { get; set; }
    public List<Checksum> Checksums { get; set; }
    public byte[]         Thumbnail { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Cover(CoverType cicm)
    {
        if(cicm is null) return null;

        var cover = new Cover
        {
            File      = cicm.File,
            Thumbnail = cicm.Thumbnail
        };

        if(cicm.Checksums is null) return cover;

        cover.Checksums = [];

        foreach(Schemas.ChecksumType chk in cicm.Checksums) cover.Checksums.Add(chk);

        return cover;
    }
}

public class Case
{
    public CaseType Type  { get; set; }
    public Scans    Scans { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Case(Schemas.CaseType cicm) => cicm is null
                                                                       ? null
                                                                       : new Case
                                                                       {
                                                                           Type  = (CaseType)cicm.CaseType1,
                                                                           Scans = cicm.Scans
                                                                       };
}

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum CaseType
{
    Jewel,
    BigJewel,
    SlimJewel,
    Sleeve,
    Qpack,
    Digisleeve,
    DiscboxSlider,
    CompacPlus,
    KeepCase,
    SnapCase,
    SoftCase,
    EcoPack,
    Liftlock,
    Spindle,
    Ps2Case,
    Ps3Case,
    BlurayKeepCase,
    PsCase,
    DcCase,
    SaturnCase,
    XboxCase,
    Xbox360Case,
    XboxOneCase,
    SaturnBigCase,
    GcCase,
    WiiCase,
    Unknown
}

public class Scans
{
    public CaseScan  Case  { get; set; }
    public MediaScan Media { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Scans(ScansType cicm) => cicm is null
                                                                 ? null
                                                                 : new Scans
                                                                 {
                                                                     Case  = cicm.CaseScan,
                                                                     Media = cicm.Scan
                                                                 };
}

public class CaseScan
{
    public CaseScanElement Element { get; set; }
    public Scan            Scan    { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator CaseScan(CaseScanType cicm) => cicm is null
                                                                       ? null
                                                                       : new CaseScan
                                                                       {
                                                                           Element = (CaseScanElement)cicm
                                                                              .CaseScanElement,
                                                                           Scan = cicm.Scan
                                                                       };
}

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum CaseScanElement
{
    Sleeve,
    Inner,
    Inlay,
    FrontBack,
    FrontFull,
    BoxFront,
    BoxBack,
    BoxSpine,
    External
}

public class MediaScan
{
    public MediaScanElement Element { get; set; }
    public Scan             Scan    { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator MediaScan(MediaScanType cicm) => cicm is null
                                                                         ? null
                                                                         : new MediaScan
                                                                         {
                                                                             Element = (MediaScanElement)cicm
                                                                                .MediaScanElement,
                                                                             Scan = cicm.Scan
                                                                         };
}

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum MediaScanElement
{
    Up,
    Down,
    Front,
    Back,
    Left,
    Right
}

public class Scanner
{
    public string Author          { get; set; }
    public string Manufacturer    { get; set; }
    public string Model           { get; set; }
    public string Serial          { get; set; }
    public string Software        { get; set; }
    public string SoftwareVersion { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Scanner(ScannerType cicm) => cicm is null
                                                                     ? null
                                                                     : new Scanner
                                                                     {
                                                                         Author          = cicm.Author,
                                                                         Manufacturer    = cicm.Manufacturer,
                                                                         Model           = cicm.Model,
                                                                         Serial          = cicm.Serial,
                                                                         Software        = cicm.Software,
                                                                         SoftwareVersion = cicm.SoftwareVersion
                                                                     };
}

public class ScanProcessing
{
    public string Author          { get; set; }
    public string Software        { get; set; }
    public string SoftwareVersion { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator ScanProcessing(ScanProcessingType cicm) => cicm is null
                                                                                   ? null
                                                                                   : new ScanProcessing
                                                                                   {
                                                                                       Author   = cicm.Author,
                                                                                       Software = cicm.Software,
                                                                                       SoftwareVersion =
                                                                                           cicm.SoftwareVersion
                                                                                   };
}

public class OCR
{
    public string         Author          { get; set; }
    public string         Software        { get; set; }
    public string         SoftwareVersion { get; set; }
    public List<Language> Language        { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator OCR(OCRType cicm)
    {
        if(cicm is null) return null;

        var ocr = new OCR
        {
            Author          = cicm.Author,
            Software        = cicm.Software,
            SoftwareVersion = cicm.SoftwareVersion
        };

        if(cicm.Language is null) return ocr;

        ocr.Language = [];

        foreach(Language lng in cicm.Language) ocr.Language.Add(lng);

        return ocr;
    }
}