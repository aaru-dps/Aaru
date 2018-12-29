// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Properties.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains properties for Anex86 disk images.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public partial class Anex86
    {
        public ImageInfo Info => imageInfo;

        public string Name   => "Anex86 Disk Image";
        public Guid   Id     => new Guid("0410003E-6E7B-40E6-9328-BA5651ADF6B7");
        public string Author => "Natalia Portillo";
        public string Format => "Anex86 disk image";

        public List<Partition> Partitions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Track> Tracks =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");

        public List<Session> Sessions =>
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        public List<DumpHardwareType> DumpHardware => null;
        public CICMMetadataType       CicmMetadata => null;

        public IEnumerable<MediaTagType>  SupportedMediaTags  => new MediaTagType[] { };
        public IEnumerable<SectorTagType> SupportedSectorTags => new SectorTagType[] { };
        // TODO: Test with real hardware to see real supported media
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.IBM23FD, MediaType.ECMA_66, MediaType.DOS_525_SS_DD_8, MediaType.DOS_525_SS_DD_9,
                MediaType.ACORN_525_SS_SD_40, MediaType.ACORN_525_SS_DD_40, MediaType.ATARI_525_SD,
                MediaType.ATARI_525_DD, MediaType.ATARI_525_ED, MediaType.DOS_525_DS_DD_8,
                MediaType.DOS_525_DS_DD_9, MediaType.ECMA_70, MediaType.Apricot_35, MediaType.RX01, MediaType.RX02,
                MediaType.NEC_525_HD, MediaType.ECMA_99_15, MediaType.NEC_8_SD, MediaType.RX03,
                MediaType.DOS_35_SS_DD_8, MediaType.DOS_35_SS_DD_9, MediaType.ACORN_525_SS_SD_80, MediaType.RX50,
                MediaType.ATARI_35_SS_DD_11, MediaType.ACORN_525_SS_DD_80, MediaType.ACORN_35_DS_DD,
                MediaType.DOS_35_DS_DD_8, MediaType.DOS_35_DS_DD_9, MediaType.ACORN_35_DS_HD, MediaType.DOS_525_HD,
                MediaType.ACORN_525_DS_DD, MediaType.DOS_35_HD, MediaType.XDF_525, MediaType.DMF, MediaType.XDF_35,
                MediaType.DOS_35_ED, MediaType.FDFORMAT_35_DD, MediaType.FDFORMAT_525_HD, MediaType.FDFORMAT_35_HD,
                MediaType.NEC_35_TD, MediaType.Unknown, MediaType.GENERIC_HDD, MediaType.FlashDrive,
                MediaType.CompactFlash, MediaType.CompactFlashType2, MediaType.PCCardTypeI, MediaType.PCCardTypeII,
                MediaType.PCCardTypeIII, MediaType.PCCardTypeIV
            };
        public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
            new (string name, Type type, string description, object @default)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".fdi", ".hdi"};

        public bool   IsWriting    { get; private set; }
        public string ErrorMessage { get; private set; }
    }
}