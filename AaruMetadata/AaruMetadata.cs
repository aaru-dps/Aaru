// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AaruMetadata.cs
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

using System;
using System.Collections.Generic;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Aaru.CommonTypes.AaruMetadata;

public class MetadataJson
{
    public Metadata AaruMetadata { get; set; }
}

public class Metadata
{
    public List<string>                  Developers               { get; set; }
    public List<string>                  Publishers               { get; set; }
    public List<string>                  Authors                  { get; set; }
    public List<string>                  Performers               { get; set; }
    public string                        Name                     { get; set; }
    public string                        Version                  { get; set; }
    public ReleaseType?                  Release                  { get; set; }
    public DateTime?                     ReleaseDate              { get; set; }
    public List<Barcode>                 Barcodes                 { get; set; }
    public string                        PartNumber               { get; set; }
    public string                        SerialNumber             { get; set; }
    public List<string>                  Keywords                 { get; set; }
    public List<Magazine>                Magazines                { get; set; }
    public List<Book>                    Books                    { get; set; }
    public List<string>                  Categories               { get; set; }
    public List<string>                  Subcategories            { get; set; }
    public List<Language>                Languages                { get; set; }
    public List<string>                  Systems                  { get; set; }
    public List<Architecture>            Architectures            { get; set; }
    public List<RequiredOperatingSystem> RequiredOperatingSystems { get; set; }
    public List<UserManual>              UserManuals              { get; set; }
    public List<OpticalDisc>             OpticalDiscs             { get; set; }
    public List<Advertisement>           Advertisements           { get; set; }
    public List<LinearMedia>             LinearMedias             { get; set; }
    public List<Pci>                     PciCards                 { get; set; }
    public List<BlockMedia>              BlockMedias              { get; set; }
    public List<AudioMedia>              AudioMedia               { get; set; }
}