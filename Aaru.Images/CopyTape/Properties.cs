// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Properties.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains properties for CopyTape tape images.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages
{
    public sealed partial class CopyTape
    {
        /// <inheritdoc />
        public ImageInfo Info => _imageInfo;
        /// <inheritdoc />
        public string Name => "CopyTape";
        /// <inheritdoc />
        public Guid Id => new Guid("C537D41E-D6A7-4922-9AA9-8E8442D0E340");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";
        /// <inheritdoc />
        public string Format => "CopyTape";
        /// <inheritdoc />
        public List<DumpHardwareType> DumpHardware => null;
        /// <inheritdoc />
        public CICMMetadataType CicmMetadata => null;
        /// <inheritdoc />
        public List<TapeFile> Files { get; private set; }
        /// <inheritdoc />
        public List<TapePartition> TapePartitions { get; private set; }
        /// <inheritdoc />
        public bool IsTape { get; set; }

        /// <inheritdoc />
        public IEnumerable<MediaTagType> SupportedMediaTags => new MediaTagType[]
            {};
        /// <inheritdoc />
        public IEnumerable<SectorTagType> SupportedSectorTags => new SectorTagType[]
            {};
        /// <inheritdoc />
        public IEnumerable<MediaType> SupportedMediaTypes => new[]
        {
            MediaType.UnknownTape, MediaType.ADR2120, MediaType.ADR260, MediaType.ADR30, MediaType.ADR50,
            MediaType.AIT1, MediaType.AIT1Turbo, MediaType.AIT2, MediaType.AIT2Turbo, MediaType.AIT3, MediaType.AIT3Ex,
            MediaType.AIT3Turbo, MediaType.AIT4, MediaType.AIT5, MediaType.AITETurbo, MediaType.SAIT1, MediaType.SAIT2,
            MediaType.Ditto, MediaType.DittoMax, MediaType.DigitalAudioTape, MediaType.DAT160, MediaType.DAT320,
            MediaType.DAT72, MediaType.DDS1, MediaType.DDS2, MediaType.DDS3, MediaType.DDS4, MediaType.CompactTapeI,
            MediaType.CompactTapeII, MediaType.DECtapeII, MediaType.DLTtapeIII, MediaType.DLTtapeIIIxt,
            MediaType.DLTtapeIV, MediaType.DLTtapeS4, MediaType.SDLT1, MediaType.SDLT2, MediaType.VStapeI,
            MediaType.Exatape15m, MediaType.Exatape22m, MediaType.Exatape22mAME, MediaType.Exatape28m,
            MediaType.Exatape40m, MediaType.Exatape45m, MediaType.Exatape54m, MediaType.Exatape75m,
            MediaType.Exatape76m, MediaType.Exatape80m, MediaType.Exatape106m, MediaType.Exatape160mXL,
            MediaType.Exatape112m, MediaType.Exatape125m, MediaType.Exatape150m, MediaType.Exatape170m,
            MediaType.Exatape225m, MediaType.IBM3470, MediaType.IBM3480, MediaType.IBM3490, MediaType.IBM3490E,
            MediaType.IBM3592, MediaType.LTO, MediaType.LTO2, MediaType.LTO3, MediaType.LTO3WORM, MediaType.LTO4,
            MediaType.LTO4WORM, MediaType.LTO5, MediaType.LTO5WORM, MediaType.LTO6, MediaType.LTO6WORM, MediaType.LTO7,
            MediaType.LTO7WORM, MediaType.MLR1, MediaType.MLR1SL, MediaType.MLR3, MediaType.SLR1, MediaType.SLR2,
            MediaType.SLR3, MediaType.SLR32, MediaType.SLR32SL, MediaType.SLR4, MediaType.SLR5, MediaType.SLR5SL,
            MediaType.SLR6, MediaType.SLRtape7, MediaType.SLRtape7SL, MediaType.SLRtape24, MediaType.SLRtape24SL,
            MediaType.SLRtape40, MediaType.SLRtape50, MediaType.SLRtape60, MediaType.SLRtape75, MediaType.SLRtape100,
            MediaType.SLRtape140, MediaType.QIC11, MediaType.QIC120, MediaType.QIC1350, MediaType.QIC150,
            MediaType.QIC24, MediaType.QIC3010, MediaType.QIC3020, MediaType.QIC3080, MediaType.QIC3095,
            MediaType.QIC320, MediaType.QIC40, MediaType.QIC525, MediaType.QIC80, MediaType.STK4480, MediaType.STK4490,
            MediaType.STK9490, MediaType.T9840A, MediaType.T9840B, MediaType.T9840C, MediaType.T9840D, MediaType.T9940A,
            MediaType.T9940B, MediaType.T10000A, MediaType.T10000B, MediaType.T10000C, MediaType.T10000D,
            MediaType.Travan, MediaType.Travan1Ex, MediaType.Travan3, MediaType.Travan3Ex, MediaType.Travan4,
            MediaType.Travan5, MediaType.Travan7, MediaType.VXA1, MediaType.VXA2, MediaType.VXA3
        };
        /// <inheritdoc />
        public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions =>
            new (string name, Type type, string description, object @default)[]
                {};
        /// <inheritdoc />
        public IEnumerable<string> KnownExtensions => new[]
        {
            ".cptp"
        };

        /// <inheritdoc />
        public bool IsWriting { get; private set; }
        /// <inheritdoc />
        public string ErrorMessage { get; private set; }
    }
}