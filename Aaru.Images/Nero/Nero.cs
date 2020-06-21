// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Nero.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Nero Burning ROM disc images.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.CD;
using Session = Aaru.CommonTypes.Structs.Session;

#pragma warning disable 414
#pragma warning disable 169

namespace Aaru.DiscImages
{
    [SuppressMessage("ReSharper", "NotAccessedField.Local"),
     SuppressMessage("ReSharper", "CollectionNeverQueried.Local")]
    public partial class Nero : IOpticalMediaImage
    {
        bool                                 imageNewFormat;
        Stream                               imageStream;
        ImageInfo                            imageInfo;
        NeroCdText                           neroCdtxt;
        NeroV1Cuesheet                       neroCuesheetV1;
        NeroV2Cuesheet                       neroCuesheetV2;
        NeroV1Dao                            neroDaov1;
        NeroV2Dao                            neroDaov2;
        NeroDiscInformation                  neroDiscInfo;
        IFilter                              neroFilter;
        NeroMediaType                        neroMediaTyp;
        NeroReloChunk                        neroRelo;
        readonly Dictionary<ushort, uint>    neroSessions;
        NeroV1Tao                            neroTaov1;
        NeroV2Tao                            neroTaov2;
        NeroTocChunk                         neroToc;
        readonly Dictionary<uint, NeroTrack> neroTracks;
        readonly Dictionary<uint, ulong>     offsetmap;
        Dictionary<uint, byte[]>             trackIsrCs;
        byte[]                               upc;
        SectorBuilder                        _sectorBuilder;

        public Nero()
        {
            imageNewFormat = false;

            imageInfo = new ImageInfo
            {
                ReadableSectorTags = new List<SectorTagType>(), ReadableMediaTags = new List<MediaTagType>()
            };

            neroSessions = new Dictionary<ushort, uint>();
            neroTracks   = new Dictionary<uint, NeroTrack>();
            offsetmap    = new Dictionary<uint, ulong>();
            Sessions     = new List<Session>();
            Partitions   = new List<Partition>();
        }
    }
}