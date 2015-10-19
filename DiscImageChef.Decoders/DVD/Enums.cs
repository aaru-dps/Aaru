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

namespace DiscImageChef.Decoders.DVD
{
    #region Public enumerations
    public enum DiskCategory : byte
    {
        DVDROM = 0x00,
        DVDRAM = 0x01,
        DVDR = 0x02,
        DVDRW = 0x03,
        HDDVDROM = 0x04,
        HDDVDRAM = 0x05,
        HDDVDR = 0x06,
        Reserved1 = 0x07,
        UMD = 0x08,
        DVDPRW = 0x09,
        DVDPR = 0x0A,
        Reserved2 = 0x0B,
        Reserved3 = 0x0C,
        DVDPRWDL = 0x0D,
        DVDPRDL = 0x0E,
        Reserved4 = 0x0F
    }

    public enum MaximumRateField : byte
    {
        /// <summary>
        /// 2.52 Mbps
        /// </summary>
        TwoMbps = 0x00,
        /// <summary>
        /// 5.04 Mbps
        /// </summary>
        FiveMbps = 0x01,
        /// <summary>
        /// 10.08 Mbps
        /// </summary>
        TenMbps = 0x02,
        /// <summary>
        /// 20.16 Mbps
        /// </summary>
        TwentyMbps = 0x03,
        /// <summary>
        /// 30.24 Mbps
        /// </summary>
        ThirtyMbps = 0x04,
        Unspecified = 0x0F
    }

    public enum LayerTypeFieldMask : byte
    {
        Embossed = 0x01,
        Recordable = 0x02,
        Rewritable = 0x04,
        Reserved = 0x08
    }

    public enum LinearDensityField : byte
    {
        /// <summary>
        /// 0.267 μm/bit
        /// </summary>
        TwoSix = 0x00,
        /// <summary>
        /// 0.293 μm/bit
        /// </summary>
        TwoNine = 0x01,
        /// <summary>
        /// 0.409 to 0.435 μm/bit
        /// </summary>
        FourZero = 0x02,
        /// <summary>
        /// 0.280 to 0.291 μm/bit
        /// </summary>
        TwoEight = 0x04,
        /// <summary>
        /// 0.153 μm/bit
        /// </summary>
        OneFive = 0x05,
        /// <summary>
        /// 0.130 to 0.140 μm/bit
        /// </summary>
        OneThree = 0x06,
        /// <summary>
        /// 0.353 μm/bit
        /// </summary>
        ThreeFive = 0x08,
    }

    public enum TrackDensityField : byte
    {
        /// <summary>
        /// 0.74 μm/track
        /// </summary>
        Seven = 0x00,
        /// <summary>
        /// 0.80 μm/track
        /// </summary>
        Eight = 0x01,
        /// <summary>
        /// 0.615 μm/track
        /// </summary>
        Six = 0x02,
        /// <summary>
        /// 0.40 μm/track
        /// </summary>
        Four = 0x03,
        /// <summary>
        /// 0.34 μm/track
        /// </summary>
        Three = 0x04
    }

    public enum CopyrightType : byte
    {
        /// <summary>
        /// There is no copy protection
        /// </summary>
        NoProtection = 0x00,
        /// <summary>
        /// Copy protection is CSS/CPPM
        /// </summary>
        CSS = 0x01,
        /// <summary>
        /// Copy protection is CPRM
        /// </summary>
        CPRM = 0x02,
        /// <summary>
        /// Copy protection is AACS
        /// </summary>
        AACS = 0x10
    }

    public enum WPDiscTypes : byte
    {
        /// <summary>
        /// Should not write without a cartridge
        /// </summary>
        DoNotWrite = 0x00,
        /// <summary>
        /// Can write without a cartridge
        /// </summary>
        CanWrite = 0x01,
        Reserved1 = 0x02,
        Reserved2 = 0x03
    }
    #endregion
}

