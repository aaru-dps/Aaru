// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;

namespace DiscImageChef.Decoders.SCSI.MMC
{
    public enum FormatLayerTypeCodes : ushort
    {
        CDLayer = 0x0008,
        DVDLayer = 0x0010,
        BDLayer = 0x0040,
        HDDVDLayer = 0x0050
    }

    public enum SessionStatusCodes : byte
    {
        Empty = 0x00,
        Incomplete = 0x01,
        ReservedOrDamaged = 0x02,
        Complete = 0x03
    }

    public enum DiscStatusCodes : byte
    {
        Empty = 0x00,
        Incomplete = 0x01,
        Finalized = 0x02,
        Others = 0x03
    }

    public enum BGFormatStatusCodes : byte
    {
        NoFormattable = 0x00,
        IncompleteBackgroundFormat = 0x01,
        BackgroundFormatInProgress = 0x02,
        FormatComplete = 0x03
    }

    public enum DiscTypeCodes : byte
    {
        /// <summary>
        /// Also valid for CD-DA, DVD and BD
        /// </summary>
        CDROM = 0x00,
        CDi = 0x10,
        CDROMXA = 0x20,
        Undefined = 0xFF
    }
}

