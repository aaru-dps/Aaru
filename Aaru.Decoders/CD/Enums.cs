// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains various CD enumerations.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Decoders.CD;

[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public enum TocAdr : byte
{
    /// <summary>Q Sub-channel mode information not supplied</summary>
    NoInformation = 0x00,
    /// <summary>Q Sub-channel encodes current position data</summary>
    CurrentPosition = 0x01,
    /// <summary>Q Sub-channel encodes the media catalog number</summary>
    MediaCatalogNumber = 0x02,
    /// <summary>Q Sub-channel encodes the ISRC</summary>
    ISRC = 0x03,
    /// <summary>Q Sub-channel encodes the start of an audio/data track (if found in TOC)</summary>
    TrackPointer = 0x01,
    /// <summary>Q Sub-channel encodes the start of a video track (if found in TOC) for CD-V</summary>
    VideoTrackPointer = 0x04
}

public enum TocControl : byte
{
    /// <summary>Stereo audio, no pre-emphasis</summary>
    TwoChanNoPreEmph = 0x00,
    /// <summary>Stereo audio with pre-emphasis</summary>
    TwoChanPreEmph = 0x01,
    /// <summary>If mask applied, track can be copied</summary>
    CopyPermissionMask = 0x02,
    /// <summary>Data track, recorded uninterrumpted</summary>
    DataTrack = 0x04,
    /// <summary>Data track, recorded incrementally</summary>
    DataTrackIncremental = 0x05,
    /// <summary>Quadraphonic audio, no pre-emphasis</summary>
    FourChanNoPreEmph = 0x08,
    /// <summary>Quadraphonic audio with pre-emphasis</summary>
    FourChanPreEmph = 0x09,
    /// <summary>Reserved mask</summary>
    ReservedMask = 0x0C
}