// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Decoders.ATA;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AtaRegistersChs
{
    public byte Feature;
    public byte SectorCount;
    public byte Sector;
    public byte CylinderLow;
    public byte CylinderHigh;
    public byte DeviceHead;
    public byte Command;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AtaRegistersLba28
{
    public byte Feature;
    public byte SectorCount;
    public byte LbaLow;
    public byte LbaMid;
    public byte LbaHigh;
    public byte DeviceHead;
    public byte Command;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AtaRegistersLba48
{
    public ushort Feature;
    public ushort SectorCount;
    public byte   LbaLowPrevious;
    public byte   LbaLowCurrent;
    public byte   LbaMidPrevious;
    public byte   LbaMidCurrent;
    public byte   LbaHighPrevious;
    public byte   LbaHighCurrent;
    public byte   DeviceHead;
    public byte   Command;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AtaErrorRegistersChs
{
    public byte Status;
    public byte Error;
    public byte SectorCount;
    public byte Sector;
    public byte CylinderLow;
    public byte CylinderHigh;
    public byte DeviceHead;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AtaErrorRegistersLba28
{
    public byte Status;
    public byte Error;
    public byte SectorCount;
    public byte LbaLow;
    public byte LbaMid;
    public byte LbaHigh;
    public byte DeviceHead;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct AtaErrorRegistersLba48
{
    public byte   Status;
    public byte   Error;
    public ushort SectorCount;
    public byte   LbaLowPrevious;
    public byte   LbaLowCurrent;
    public byte   LbaMidPrevious;
    public byte   LbaMidCurrent;
    public byte   LbaHighPrevious;
    public byte   LbaHighCurrent;
    public byte   DeviceHead;
}