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
//     Contains structures for CDRWin cuesheets (cue/bin).
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

// ReSharper disable NotAccessedField.Local

namespace Aaru.DiscImages
{
    public sealed partial class CdrWin
    {
        struct CdrWinTrackFile
        {
            /// <summary>Track #</summary>
            public uint Sequence;
            /// <summary>Filter of file containing track</summary>
            public IFilter DataFilter;
            /// <summary>Offset of track start in file</summary>
            public ulong Offset;
            /// <summary>Type of file</summary>
            public string FileType;
        }

        class CdrWinTrack
        {
            /// <summary>Track arranger (from CD-Text)</summary>
            public string Arranger;
            /// <summary>Bytes per sector</summary>
            public ushort Bps;
            /// <summary>Track composer (from CD-Text)</summary>
            public string Composer;
            /// <summary>Track is quadraphonic</summary>
            public bool Flag4Ch;
            /// <summary>Digital Copy Permitted</summary>
            public bool FlagDcp;
            /// <summary>Track has pre-emphasis</summary>
            public bool FlagPre;
            /// <summary>Track has SCMS</summary>
            public bool FlagScms;
            /// <summary>Track genre (from CD-Text)</summary>
            public string Genre;
            /// <summary>Indexes on this track</summary>
            public SortedDictionary<ushort, int> Indexes;
            /// <summary>Track ISRC</summary>
            public string Isrc;
            /// <summary>Track performer (from CD-Text)</summary>
            public string Performer;
            /// <summary>Track post-gap in sectors</summary>
            public int Postgap;
            /// <summary>Track pre-gap in sectors</summary>
            public int Pregap;
            /// <summary>Sectors in track</summary>
            public ulong Sectors;
            /// <summary>Track #</summary>
            public uint Sequence;
            /// <summary>Track session</summary>
            public ushort Session;
            /// <summary>Track song writer (from CD-Text)</summary>
            public string Songwriter;
            /// <summary>Track title (from CD-Text)</summary>
            public string Title;
            /// <summary>File struct for this track</summary>
            public CdrWinTrackFile TrackFile;
            /// <summary>Track type</summary>
            public string TrackType;
        }

        struct CdrWinDisc
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
            /// <summary>Media catalog number</summary>
            public string Mcn;
            /// <summary>Disk type</summary>
            public MediaType MediaType;
            /// <summary>Disk type string</summary>
            public string OriginalMediaType;
            /// <summary>Disk CDDB ID</summary>
            public string DiscId;
            /// <summary>Disk UPC/EAN</summary>
            public string Barcode;
            /// <summary>Sessions</summary>
            public List<Session> Sessions;
            /// <summary>Tracks</summary>
            public List<CdrWinTrack> Tracks;
            /// <summary>Disk comment</summary>
            public string Comment;
            /// <summary>File containing CD-Text</summary>
            public string CdTextFile;
            /// <summary>Has trurip extensions</summary>
            public bool IsTrurip;
            /// <summary>Disc image hashes</summary>
            public Dictionary<string, string> DiscHashes;
            /// <summary>Aaru media type</summary>
            public string AaruMediaType;
            /// <summary>Is a GDROM from Redump.org</summary>
            public bool IsRedumpGigadisc;
        }
    }
}