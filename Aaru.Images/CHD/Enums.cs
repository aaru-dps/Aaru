// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains enumerations for MAME Compressed Hunks of Data disk images.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class Chd
{
    enum Compression : uint
    {
        None = 0, Zlib = 1, ZlibPlus = 2,
        Av   = 3
    }

    enum Flags : uint
    {
        HasParent = 1, Writable = 2
    }

    enum EntryFlagsV3 : byte
    {
        /// <summary>Invalid</summary>
        Invalid = 0,
        /// <summary>Compressed with primary codec</summary>
        Compressed = 1,
        /// <summary>Uncompressed</summary>
        Uncompressed = 2,
        /// <summary>Use offset as data</summary>
        Mini = 3,
        /// <summary>Same as another hunk in file</summary>
        SelfHunk = 4,
        /// <summary>Same as another hunk in parent</summary>
        ParentHunk = 5,
        /// <summary>Compressed with secondary codec (FLAC)</summary>
        SecondCompressed = 6
    }

    enum TrackTypeOld : uint
    {
        Mode1 = 0, Mode1Raw, Mode2,
        Mode2Form1, Mode2Form2, Mode2FormMix,
        Mode2Raw, Audio
    }

    enum SubTypeOld : uint
    {
        Cooked = 0, Raw, None
    }
}