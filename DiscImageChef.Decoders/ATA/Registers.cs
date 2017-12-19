// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Errors.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes ATA error registers.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

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