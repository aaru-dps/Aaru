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
//     Contains various floppy enumerations.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Decoders.Floppy;

/// <summary>In-sector code for sector size</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum IBMSectorSizeCode : byte
{
    /// <summary>128 bytes/sector</summary>
    EighthKilo = 0,
    /// <summary>256 bytes/sector</summary>
    QuarterKilo = 1,
    /// <summary>512 bytes/sector</summary>
    HalfKilo = 2,
    /// <summary>1024 bytes/sector</summary>
    Kilo = 3,
    /// <summary>2048 bytes/sector</summary>
    TwiceKilo = 4,
    /// <summary>4096 bytes/sector</summary>
    FriceKilo = 5,
    /// <summary>8192 bytes/sector</summary>
    TwiceFriceKilo = 6,
    /// <summary>16384 bytes/sector</summary>
    FricelyFriceKilo = 7
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum IBMIdType : byte
{
    IndexMark       = 0xFC,
    AddressMark     = 0xFE,
    DataMark        = 0xFB,
    DeletedDataMark = 0xF8
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum AppleEncodedFormat : byte
{
    /// <summary>Disk is an Apple II 3.5" disk</summary>
    AppleII = 0x96,
    /// <summary>Disk is an Apple Lisa 3.5" disk</summary>
    Lisa = 0x97,
    /// <summary>Disk is an Apple Macintosh single-sided 3.5" disk</summary>
    MacSingleSide = 0x9A,
    /// <summary>Disk is an Apple Macintosh double-sided 3.5" disk</summary>
    MacDoubleSide = 0xD9
}