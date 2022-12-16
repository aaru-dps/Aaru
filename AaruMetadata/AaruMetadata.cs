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
using System.Text.Json.Serialization;
using Schemas;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Aaru.CommonTypes.AaruMetadata;

[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(MetadataJson))]
public partial class MetadataJsonContext : JsonSerializerContext {}

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
    public List<AudioMedia>              AudioMedias              { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Metadata(CICMMetadataType cicm)
    {
        if(cicm is null)
            return null;

        var metadata = new Metadata
        {
            Developers    = cicm.Developer is null ? null : new List<string>(cicm.Developer),
            Publishers    = cicm.Publisher is null ? null : new List<string>(cicm.Publisher),
            Authors       = cicm.Author is null ? null : new List<string>(cicm.Author),
            Performers    = cicm.Performer is null ? null : new List<string>(cicm.Performer),
            Name          = cicm.Name,
            Version       = cicm.Version,
            Release       = cicm.ReleaseTypeSpecified ? (ReleaseType)cicm.ReleaseType : null,
            ReleaseDate   = cicm.ReleaseDateSpecified ? cicm.ReleaseDate : null,
            PartNumber    = cicm.PartNumber,
            SerialNumber  = cicm.SerialNumber,
            Keywords      = cicm.Keywords is null ? null : new List<string>(cicm.Keywords),
            Categories    = cicm.Categories is null ? null : new List<string>(cicm.Categories),
            Subcategories = cicm.Subcategories is null ? null : new List<string>(cicm.Subcategories),
            Systems       = cicm.Systems is null ? null : new List<string>(cicm.Systems)
        };

        if(cicm.Barcodes is not null)
        {
            metadata.Barcodes = new List<Barcode>();

            foreach(Schemas.BarcodeType code in cicm.Barcodes)
                metadata.Barcodes.Add(code);
        }

        if(cicm.Magazine is not null)
        {
            metadata.Magazines = new List<Magazine>();

            foreach(MagazineType magazine in cicm.Magazine)
                metadata.Magazines.Add(magazine);
        }

        if(cicm.Book is not null)
        {
            metadata.Books = new List<Book>();

            foreach(BookType book in cicm.Book)
                metadata.Books.Add(book);
        }

        if(cicm.Languages is not null)
        {
            metadata.Languages = new List<Language>();

            foreach(LanguagesTypeLanguage lng in cicm.Languages)
                metadata.Languages.Add((Language)lng);
        }

        if(cicm.Architectures is not null)
        {
            metadata.Architectures = new List<Architecture>();

            foreach(ArchitecturesTypeArchitecture arch in cicm.Architectures)
                metadata.Architectures.Add((Architecture)arch);
        }

        if(cicm.RequiredOperatingSystems is not null)
        {
            metadata.RequiredOperatingSystems = new List<RequiredOperatingSystem>();

            foreach(RequiredOperatingSystemType os in cicm.RequiredOperatingSystems)
                metadata.RequiredOperatingSystems.Add(os);
        }

        if(cicm.UserManual is not null)
        {
            metadata.UserManuals = new List<UserManual>();

            foreach(UserManualType manual in cicm.UserManual)
                metadata.UserManuals.Add(manual);
        }

        if(cicm.OpticalDisc is not null)
        {
            metadata.OpticalDiscs = new List<OpticalDisc>();

            foreach(OpticalDiscType disc in cicm.OpticalDisc)
                metadata.OpticalDiscs.Add(disc);
        }

        if(cicm.Advertisement is not null)
        {
            metadata.Advertisements = new List<Advertisement>();

            foreach(AdvertisementType adv in cicm.Advertisement)
                metadata.Advertisements.Add(adv);
        }

        if(cicm.LinearMedia is not null)
        {
            metadata.LinearMedias = new List<LinearMedia>();

            foreach(LinearMediaType media in cicm.LinearMedia)
                metadata.LinearMedias.Add(media);
        }

        if(cicm.PCICard is not null)
        {
            metadata.PciCards = new List<Pci>();

            foreach(PCIType pci in cicm.PCICard)
                metadata.PciCards.Add(pci);
        }

        if(cicm.BlockMedia is not null)
        {
            metadata.BlockMedias = new List<BlockMedia>();

            foreach(BlockMediaType media in cicm.BlockMedia)
                metadata.BlockMedias.Add(media);
        }

        if(cicm.AudioMedia is not null)
        {
            metadata.AudioMedias = new List<AudioMedia>();

            foreach(AudioMediaType media in cicm.AudioMedia)
                metadata.AudioMedias.Add(media);
        }

        return metadata;
    }
}