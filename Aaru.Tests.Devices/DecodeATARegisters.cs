// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DecodeATARegisters.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru device testing.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using Aaru.Decoders.ATA;

namespace Aaru.Tests.Devices;

internal static partial class MainClass
{
    static string DecodeAtaStatus(byte status)
    {
        string ret = "";

        if((status & 0x80) == 0x80)
            ret += "BSY ";

        if((status & 0x40) == 0x40)
            ret += "DRDY ";

        if((status & 0x20) == 0x20)
            ret += "DWF ";

        if((status & 0x10) == 0x10)
            ret += "DSC ";

        if((status & 0x8) == 0x8)
            ret += "DRQ ";

        if((status & 0x4) == 0x4)
            ret += "CORR ";

        if((status & 0x2) == 0x2)
            ret += "IDX ";

        if((status & 0x1) == 0x1)
            ret += "ERR ";

        return ret;
    }

    static string DecodeAtaError(byte status)
    {
        string ret = "";

        if((status & 0x80) == 0x80)
            ret += "BBK ";

        if((status & 0x40) == 0x40)
            ret += "UNC ";

        if((status & 0x20) == 0x20)
            ret += "MC ";

        if((status & 0x10) == 0x10)
            ret += "IDNF ";

        if((status & 0x8) == 0x8)
            ret += "MCR ";

        if((status & 0x4) == 0x4)
            ret += "ABRT ";

        if((status & 0x2) == 0x2)
            ret += "TK0NF ";

        if((status & 0x1) == 0x1)
            ret += "AMNF ";

        return ret;
    }

    public static string DecodeAtaRegisters(AtaErrorRegistersChs registers)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("Status: {0}", DecodeAtaStatus(registers.Status)).AppendLine();
        sb.AppendFormat("Error: {0}", DecodeAtaStatus(registers.Error)).AppendLine();
        sb.AppendFormat("Device: {0}", (registers.DeviceHead >> 4) & 0x01).AppendLine();
        sb.AppendFormat("Cylinder: {0}", registers.CylinderHigh << (8 + registers.CylinderLow)).AppendLine();
        sb.AppendFormat("Head: {0}", registers.DeviceHead & 0xF).AppendLine();
        sb.AppendFormat("Sector: {0}", registers.Sector).AppendLine();
        sb.AppendFormat("Count: {0}", registers.SectorCount).AppendLine();
        sb.AppendFormat("LBA?: {0}", Convert.ToBoolean(registers.DeviceHead       & 0x40)).AppendLine();
        sb.AppendFormat("Bit 7 set?: {0}", Convert.ToBoolean(registers.DeviceHead & 0x80)).AppendLine();
        sb.AppendFormat("Bit 5 set?: {0}", Convert.ToBoolean(registers.DeviceHead & 0x20)).AppendLine();

        return sb.ToString();
    }

    public static string DecodeAtaRegisters(AtaErrorRegistersLba28 registers)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("Status: {0}", DecodeAtaStatus(registers.Status)).AppendLine();
        sb.AppendFormat("Error: {0}", DecodeAtaStatus(registers.Error)).AppendLine();
        sb.AppendFormat("Device: {0}", (registers.DeviceHead >> 4) & 0x01).AppendLine();

        sb.AppendFormat("LBA: {0}",
                        ((registers.DeviceHead & 0xF) << 24) + (registers.LbaHigh << 16) + (registers.LbaMid << 8) +
                        registers.LbaLow);

        sb.AppendFormat("Count: {0}", registers.SectorCount).AppendLine();
        sb.AppendFormat("LBA?: {0}", Convert.ToBoolean(registers.DeviceHead       & 0x40)).AppendLine();
        sb.AppendFormat("Bit 7 set?: {0}", Convert.ToBoolean(registers.DeviceHead & 0x80)).AppendLine();
        sb.AppendFormat("Bit 5 set?: {0}", Convert.ToBoolean(registers.DeviceHead & 0x20)).AppendLine();

        return sb.ToString();
    }

    public static string DecodeAtaRegisters(AtaErrorRegistersLba48 registers)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("Status: {0}", DecodeAtaStatus(registers.Status)).AppendLine();
        sb.AppendFormat("Error: {0}", DecodeAtaStatus(registers.Error)).AppendLine();
        sb.AppendFormat("Device: {0}", (registers.DeviceHead >> 4) & 0x01).AppendLine();

        ulong lba = registers.LbaHighPrevious * 0x10000000000UL;
        lba += registers.LbaMidPrevious * 0x100000000UL;
        lba += registers.LbaLowPrevious * 0x1000000UL;
        lba += registers.LbaHighCurrent * 0x10000UL;
        lba += registers.LbaMidCurrent  * 0x100UL;
        lba += registers.LbaLowCurrent;

        sb.AppendFormat("LBA: {0}", lba);

        sb.AppendFormat("Count: {0}", registers.SectorCount).AppendLine();
        sb.AppendFormat("LBA?: {0}", Convert.ToBoolean(registers.DeviceHead       & 0x40)).AppendLine();
        sb.AppendFormat("Bit 7 set?: {0}", Convert.ToBoolean(registers.DeviceHead & 0x80)).AppendLine();
        sb.AppendFormat("Bit 5 set?: {0}", Convert.ToBoolean(registers.DeviceHead & 0x20)).AppendLine();

        return sb.ToString();
    }
}