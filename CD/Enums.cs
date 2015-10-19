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

namespace DiscImageChef.Decoders.CD
{
    public enum TOC_ADR : byte
    {
        /// <summary>
        /// Q Sub-channel mode information not supplied
        /// </summary>
        NoInformation = 0x00,
        /// <summary>
        /// Q Sub-channel encodes current position data
        /// </summary>
        CurrentPosition = 0x01,
        /// <summary>
        /// Q Sub-channel encodes the media catalog number
        /// </summary>
        MediaCatalogNumber = 0x02,
        /// <summary>
        /// Q Sub-channel encodes the ISRC
        /// </summary>
        ISRC = 0x03
    }

    public enum TOC_CONTROL : byte
    {
        /// <summary>
        /// Stereo audio, no pre-emphasis
        /// </summary>
        TwoChanNoPreEmph = 0x00,
        /// <summary>
        /// Stereo audio with pre-emphasis
        /// </summary>
        TwoChanPreEmph = 0x01,
        /// <summary>
        /// If mask applied, track can be copied
        /// </summary>
        CopyPermissionMask = 0x02,
        /// <summary>
        /// Data track, recorded uninterrumpted
        /// </summary>
        DataTrack = 0x04,
        /// <summary>
        /// Data track, recorded incrementally
        /// </summary>
        DataTrackIncremental = 0x05,
        /// <summary>
        /// Quadraphonic audio, no pre-emphasis
        /// </summary>
        FourChanNoPreEmph = 0x08,
        /// <summary>
        /// Quadraphonic audio with pre-emphasis
        /// </summary>
        FourChanPreEmph = 0x09,
        /// <summary>
        /// Reserved mask
        /// </summary>
        ReservedMask = 0x0C
    }
}

