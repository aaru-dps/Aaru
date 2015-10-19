// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Perpendicular.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Perpendicular MFM floppy decoder
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Methods and structures for perpendicular MFM floppy decoding
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
    /// Methods and structures for perpendicular MFM floppy decoding
    /// </summary>
    public static class Perpendicular
    {
        /// <summary>
        /// Perpendicular floppy track
        /// </summary>
        public struct PerpendicularFloppyTrack
        {
            /// <summary>
            /// Start of track
            /// </summary>
            public IBMMFMTrackPreamble trackStart;
            /// <summary>
            /// Track sectors
            /// </summary>
            public PerpendicularFloppySector[] sectors;
            /// <summary>
            /// Undefined size
            /// </summary>
            public byte[] gap;
        }

        /// <summary>
        /// Raw demodulated format for perpendicular floppies
        /// </summary>
        public struct PerpendicularFloppySector
        {
            /// <summary>
            /// Sector address mark
            /// </summary>
            public IBMMFMSectorAddressMark addressMark;
            /// <summary>
            /// 41 bytes set to 0x4E
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 41)]
            public byte[] innerGap;
            /// <summary>
            /// Sector data block
            /// </summary>
            public IBMMFMSectorDataBlock dataBlock;
            /// <summary>
            /// Variable-sized inter-sector gap, ECMA defines 83 bytes
            /// </summary>
            public byte[] outerGap;
        }
    }
}

