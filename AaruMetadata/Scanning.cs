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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;

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
}

public class Cover
{
    public File           File      { get; set; }
    public List<Checksum> Checksums { get; set; }
    public byte[]         Thumbnail { get; set; }
}

public class Case
{
    public CaseType Type  { get; set; }
    public Scans    Scans { get; set; }
}

public enum CaseType
{
    Jewel, BigJewel, SlimJewel,
    Sleeve, Qpack, Digisleeve,
    DiscboxSlider, CompacPlus, KeepCase,
    SnapCase, SoftCase, EcoPack,
    Liftlock, Spindle, Ps2Case,
    Ps3Case, BlurayKeepCase, PsCase,
    DcCase, SaturnCase, XboxCase,
    Xbox360Case, XboxOneCase, SaturnBigCase,
    GcCase, WiiCase, Unknown
}

public class Scans
{
    public CaseScan  Case  { get; set; }
    public MediaScan Media { get; set; }
}

public class CaseScan
{
    public CaseScanElement Element { get; set; }
    public Scan            Scan    { get; set; }
}

public enum CaseScanElement
{
    Sleeve, Inner, Inlay,
    FrontBack, FrontFull, BoxFront,
    BoxBack, BoxSpine, External
}

public class MediaScan
{
    public MediaScanElement Element { get; set; }
    public Scan             Scan    { get; set; }
}

public enum MediaScanElement
{
    Up, Down, Front,
    Back, Left, Right
}

public class Scanner
{
    public string Author          { get; set; }
    public string Manufacturer    { get; set; }
    public string Model           { get; set; }
    public string Serial          { get; set; }
    public string Software        { get; set; }
    public string SoftwareVersion { get; set; }
}

public class ScanProcessing
{
    public string Author          { get; set; }
    public string Software        { get; set; }
    public string SoftwareVersion { get; set; }
}

public class OCR
{
    public string         Author          { get; set; }
    public string         Software        { get; set; }
    public string         SoftwareVersion { get; set; }
    public List<Language> Language        { get; set; }
}
