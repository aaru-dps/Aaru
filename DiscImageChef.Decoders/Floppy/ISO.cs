// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ISO.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : ISO floppy decoder
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Methods and structures for ISO floppy decoding, also used by Atari ST and
// others
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

namespace DiscImageChef.Decoders.Floppy
{
    // Information from:
    // National Semiconductor PC87332VLJ datasheet
    // SMsC FDC37C78 datasheet
    // Intel 82078 datasheet
    // Intel 82077AA datasheet
    // Toshiba TC8566AF datasheet
    // Fujitsu MB8876A datasheet
    // ECMA-147
    // ECMA-100

    /// <summary>
    /// Methods and structures for ISO floppy decoding (also used by Atari ST and others)
    /// </summary>
    public static class ISO
    {
        /// <summary>
        /// ISO floppy track, also used by Atari ST and others
        /// </summary>
        public struct ISOFloppyTrack
        {
            /// <summary>
            /// Start of track, 32 bytes set to 0x4E
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] innerGap;
            /// <summary>
            /// Track sectors
            /// </summary>
            public IBMMFMSector[] sectors;
            /// <summary>
            /// Undefined size
            /// </summary>
            public byte[] gap;
        }
    }
}

