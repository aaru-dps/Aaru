// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Magazine.cs
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
using Schemas;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Aaru.CommonTypes.AaruMetadata;

public class Magazine
{
    public List<Barcode>  Barcodes        { get; set; }
    public Cover          Cover           { get; set; }
    public string         Name            { get; set; }
    public string         Editorial       { get; set; }
    public DateTime?      PublicationDate { get; set; }
    public uint?          Number          { get; set; }
    public List<Language> Languages       { get; set; }
    public uint?          Pages           { get; set; }
    public string         PageSize        { get; set; }
    public Scan           Scan            { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Magazine(MagazineType cicm)
    {
        if(cicm is null) return null;

        var magazine = new Magazine
        {
            Cover           = cicm.Cover,
            Name            = cicm.Name,
            Editorial       = cicm.Editorial,
            PublicationDate = cicm.PublicationDateSpecified ? cicm.PublicationDate : null,
            Number          = cicm.NumberSpecified ? cicm.Number : null,
            Pages           = cicm.PagesSpecified ? cicm.Pages : null,
            Scan            = cicm.Scan
        };

        if(cicm.Barcodes is not null)
        {
            magazine.Barcodes = new List<Barcode>();

            foreach(Schemas.BarcodeType code in cicm.Barcodes) magazine.Barcodes.Add(code);
        }

        if(cicm.Language is null) return magazine;

        foreach(LanguagesTypeLanguage lng in cicm.Language) magazine.Languages.Add((Language)lng);

        return magazine;
    }
}