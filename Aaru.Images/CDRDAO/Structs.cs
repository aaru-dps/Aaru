// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for cdrdao cuesheets (toc/bin).
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.DiscImages;

public sealed partial class Cdrdao
{
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    struct CdrdaoTrackFile
    {
        /// <summary>Track #</summary>
        public uint Sequence;
        /// <summary>Filter of file containing track</summary>
        public IFilter Datafilter;
        /// <summary>Path of file containing track</summary>
        public string Datafile;
        /// <summary>Offset of track start in file</summary>
        public ulong Offset;
        /// <summary>Type of file</summary>
        public string Filetype;
    }

    #pragma warning disable 169
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    struct CdrdaoTrack
    {
        /// <summary>Track #</summary>
        public uint Sequence;
        /// <summary>Track title (from CD-Text)</summary>
        public string Title;
        /// <summary>Track genre (from CD-Text)</summary>
        public string Genre;
        /// <summary>Track arranger (from CD-Text)</summary>
        public string Arranger;
        /// <summary>Track composer (from CD-Text)</summary>
        public string Composer;
        /// <summary>Track performer (from CD-Text)</summary>
        public string Performer;
        /// <summary>Track song writer (from CD-Text)</summary>
        public string Songwriter;
        /// <summary>Track ISRC</summary>
        public string Isrc;
        /// <summary>Disk provider's message (from CD-Text)</summary>
        public string Message;
        /// <summary>File struct for this track</summary>
        public CdrdaoTrackFile Trackfile;
        /// <summary>Indexes on this track</summary>
        public Dictionary<int, ulong> Indexes;
        /// <summary>Track pre-gap in sectors</summary>
        public ulong Pregap;
        /// <summary>Track post-gap in sectors</summary>
        public ulong Postgap;
        /// <summary>Digical Copy Permitted</summary>
        public bool FlagDcp;
        /// <summary>Track is quadraphonic</summary>
        public bool Flag4Ch;
        /// <summary>Track has preemphasis</summary>
        public bool FlagPre;
        /// <summary>Bytes per sector</summary>
        public ushort Bps;
        /// <summary>Sectors in track</summary>
        public ulong Sectors;
        /// <summary>Starting sector in track</summary>
        public ulong StartSector;
        /// <summary>Track type</summary>
        public string Tracktype;
        public bool Subchannel;
        public bool Packedsubchannel;
    }

    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    struct CdrdaoDisc
    {
        /// <summary>Disk title (from CD-Text)</summary>
        public string Title;
        /// <summary>Disk genre (from CD-Text)</summary>
        public string Genre;
        /// <summary>Disk arranger (from CD-Text)</summary>
        public string Arranger;
        /// <summary>Disk composer (from CD-Text)</summary>
        public string Composer;
        /// <summary>Disk performer (from CD-Text)</summary>
        public string Performer;
        /// <summary>Disk song writer (from CD-Text)</summary>
        public string Songwriter;
        /// <summary>Disk provider's message (from CD-Text)</summary>
        public string Message;
        /// <summary>Media catalog number</summary>
        public string Mcn;
        /// <summary>Disk type</summary>
        public MediaType Disktype;
        /// <summary>Disk type string</summary>
        public string Disktypestr;
        /// <summary>Disk CDDB ID</summary>
        public string DiskId;
        /// <summary>Disk UPC/EAN</summary>
        public string Barcode;
        /// <summary>Tracks</summary>
        public List<CdrdaoTrack> Tracks;
        /// <summary>Disk comment</summary>
        public string Comment;
    }
    #pragma warning restore 169
}