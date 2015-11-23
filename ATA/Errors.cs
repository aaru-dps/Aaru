// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Errors.cs
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

namespace DiscImageChef.Decoders.ATA
{
    public struct AtaRegistersCHS
    {
        public byte feature;
        public byte sectorCount;
        public byte sector;
        public byte cylinderLow;
        public byte cylinderHigh;
        public byte deviceHead;
        public byte command;
    }

    public struct AtaRegistersLBA28
    {
        public byte feature;
        public byte sectorCount;
        public byte lbaLow;
        public byte lbaMid;
        public byte lbaHigh;
        public byte deviceHead;
        public byte command;
    }

    public struct AtaRegistersLBA48
    {
        public ushort feature;
        public ushort sectorCount;
        public ushort lbaLow;
        public ushort lbaMid;
        public ushort lbaHigh;
        public byte deviceHead;
        public byte command;
    }

    public struct AtaErrorRegistersCHS
    {
        public byte status;
        public byte error;
        public byte sectorCount;
        public byte sector;
        public byte cylinderLow;
        public byte cylinderHigh;
        public byte deviceHead;
        public byte command;
    }

    public struct AtaErrorRegistersLBA28
    {
        public byte status;
        public byte error;
        public byte sectorCount;
        public byte lbaLow;
        public byte lbaMid;
        public byte lbaHigh;
        public byte deviceHead;
        public byte command;
    }

    public struct AtaErrorRegistersLBA48
    {
        public byte status;
        public byte error;
        public ushort sectorCount;
        public ushort lbaLow;
        public ushort lbaMid;
        public ushort lbaHigh;
        public byte deviceHead;
        public byte command;
    }
}

