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
//     Contains various SCSI MMC enumerations.
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


// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace Aaru.Decoders.SCSI.MMC;

public enum FormatLayerTypeCodes : ushort
{
    CDLayer    = 0x0008,
    DVDLayer   = 0x0010,
    BDLayer    = 0x0040,
    HDDVDLayer = 0x0050
}

public enum SessionStatusCodes : byte
{
    Empty             = 0x00,
    Incomplete        = 0x01,
    ReservedOrDamaged = 0x02,
    Complete          = 0x03
}

public enum DiscStatusCodes : byte
{
    Empty      = 0x00,
    Incomplete = 0x01,
    Finalized  = 0x02,
    Others     = 0x03
}

public enum BGFormatStatusCodes : byte
{
    NoFormattable              = 0x00,
    IncompleteBackgroundFormat = 0x01,
    BackgroundFormatInProgress = 0x02,
    FormatComplete             = 0x03
}

public enum DiscTypeCodes : byte
{
    /// <summary>Also valid for CD-DA, DVD and BD</summary>
    CDROM = 0x00,
    CDi       = 0x10,
    CDROMXA   = 0x20,
    Undefined = 0xFF
}

public enum LayerJumpRecordingStatus : byte
{
    Incremental     = 0,
    Unspecified     = 1,
    Manual          = 2,
    RegularInterval = 3
}