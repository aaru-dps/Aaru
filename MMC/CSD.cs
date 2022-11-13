// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CSD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes MultiMediaCard CSD.
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

namespace Aaru.Decoders.MMC;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
public class CSD
{
    public ushort Classes;
    public bool   ContentProtection;
    public bool   Copy;
    public byte   CRC;
    public byte   DefaultECC;
    public bool   DSRImplemented;
    public byte   ECC;
    public byte   EraseGroupSize;
    public byte   EraseGroupSizeMultiplier;
    public byte   FileFormat;
    public bool   FileFormatGroup;
    public byte   NSAC;
    public bool   PermanentWriteProtect;
    public byte   ReadBlockLength;
    public byte   ReadCurrentAtVddMax;
    public byte   ReadCurrentAtVddMin;
    public bool   ReadMisalignment;
    public bool   ReadsPartialBlocks;
    public ushort Size;
    public byte   SizeMultiplier;
    public byte   Speed;
    public byte   Structure;
    public byte   TAAC;
    public bool   TemporaryWriteProtect;
    public byte   Version;
    public byte   WriteBlockLength;
    public byte   WriteCurrentAtVddMax;
    public byte   WriteCurrentAtVddMin;
    public bool   WriteMisalignment;
    public bool   WriteProtectGroupEnable;
    public byte   WriteProtectGroupSize;
    public bool   WritesPartialBlocks;
    public byte   WriteSpeedFactor;
}

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Decoders
{
    public static CSD DecodeCSD(uint[] response)
    {
        if(response?.Length != 4)
            return null;

        var data = new byte[16];

        byte[] tmp = BitConverter.GetBytes(response[0]);
        Array.Copy(tmp, 0, data, 0, 4);
        tmp = BitConverter.GetBytes(response[1]);
        Array.Copy(tmp, 0, data, 4, 4);
        tmp = BitConverter.GetBytes(response[2]);
        Array.Copy(tmp, 0, data, 8, 4);
        tmp = BitConverter.GetBytes(response[3]);
        Array.Copy(tmp, 0, data, 12, 4);

        return DecodeCSD(data);
    }

    public static CSD DecodeCSD(byte[] response)
    {
        if(response?.Length != 16)
            return null;

        return new CSD
        {
            Structure = (byte)((response[0] & 0xC0) >> 6),
            Version = (byte)((response[0] & 0x3C) >> 2),
            TAAC = response[1],
            NSAC = response[2],
            Speed = response[3],
            Classes = (ushort)((response[4] << 4) + ((response[5] & 0xF0) >> 4)),
            ReadBlockLength = (byte)(response[5] & 0x0F),
            ReadsPartialBlocks = (response[6] & 0x80) == 0x80,
            WriteMisalignment = (response[6] & 0x40) == 0x40,
            ReadMisalignment = (response[6] & 0x20) == 0x20,
            DSRImplemented = (response[6] & 0x10) == 0x10,
            Size = (ushort)(((response[6] & 0x03) << 10) + (response[7] << 2) + ((response[8] & 0xC0) >> 6)),
            ReadCurrentAtVddMin = (byte)((response[8] & 0x38) >> 3),
            ReadCurrentAtVddMax = (byte)(response[8] & 0x07),
            WriteCurrentAtVddMin = (byte)((response[9] & 0xE0) >> 5),
            WriteCurrentAtVddMax = (byte)((response[9] & 0x1C) >> 2),
            SizeMultiplier = (byte)(((response[9] & 0x03) << 1) + ((response[10] & 0x80) >> 7)),
            EraseGroupSize = (byte)((response[10] & 0x7C) >> 2),
            EraseGroupSizeMultiplier = (byte)(((response[10] & 0x03) << 3) + ((response[11] & 0xE0) >> 5)),
            WriteProtectGroupSize = (byte)(response[11] & 0x1F),
            WriteProtectGroupEnable = (response[12] & 0x80) == 0x80,
            DefaultECC = (byte)((response[12] & 0x60) >> 5),
            WriteSpeedFactor = (byte)((response[12] & 0x1C) >> 2),
            WriteBlockLength = (byte)(((response[12] & 0x03) << 2) + ((response[13] & 0xC0) >> 6)),
            WritesPartialBlocks = (response[13] & 0x20) == 0x20,
            ContentProtection = (response[13] & 0x01) == 0x01,
            FileFormatGroup = (response[14] & 0x80) == 0x80,
            Copy = (response[14] & 0x40) == 0x40,
            PermanentWriteProtect = (response[14] & 0x20) == 0x20,
            TemporaryWriteProtect = (response[14] & 0x10) == 0x10,
            FileFormat = (byte)((response[14] & 0x0C) >> 2),
            ECC = (byte)(response[14] & 0x03),
            CRC = (byte)((response[15] & 0xFE) >> 1)
        };
    }

    public static string PrettifyCSD(CSD csd)
    {
        if(csd == null)
            return null;

        double unitFactor = 0;
        double multiplier = 0;
        var    unit       = "";

        var sb = new StringBuilder();
        sb.AppendLine("MultiMediaCard Device Specific Data Register:");

        switch(csd.Structure)
        {
            case 0:
                sb.AppendLine("\tRegister version 1.0");

                break;
            case 1:
                sb.AppendLine("\tRegister version 1.1");

                break;
            case 2:
                sb.AppendLine("\tRegister version 1.2");

                break;
            case 3:
                sb.AppendLine("\tRegister version is defined in Extended Device Specific Data Register");

                break;
        }

        switch(csd.TAAC & 0x07)
        {
            case 0:
                unit       = "ns";
                unitFactor = 1;

                break;
            case 1:
                unit       = "ns";
                unitFactor = 10;

                break;
            case 2:
                unit       = "ns";
                unitFactor = 100;

                break;
            case 3:
                unit       = "μs";
                unitFactor = 1;

                break;
            case 4:
                unit       = "μs";
                unitFactor = 10;

                break;
            case 5:
                unit       = "μs";
                unitFactor = 100;

                break;
            case 6:
                unit       = "ms";
                unitFactor = 1;

                break;
            case 7:
                unit       = "ms";
                unitFactor = 10;

                break;
        }

        switch((csd.TAAC & 0x78) >> 3)
        {
            case 0:
                multiplier = 0;

                break;
            case 1:
                multiplier = 1;

                break;
            case 2:
                multiplier = 1.2;

                break;
            case 3:
                multiplier = 1.3;

                break;
            case 4:
                multiplier = 1.5;

                break;
            case 5:
                multiplier = 2;

                break;
            case 6:
                multiplier = 2.5;

                break;
            case 7:
                multiplier = 3;

                break;
            case 8:
                multiplier = 3.5;

                break;
            case 9:
                multiplier = 4;

                break;
            case 10:
                multiplier = 4.5;

                break;
            case 11:
                multiplier = 5;

                break;
            case 12:
                multiplier = 5.5;

                break;
            case 13:
                multiplier = 6;

                break;
            case 14:
                multiplier = 7;

                break;
            case 15:
                multiplier = 8;

                break;
        }

        double result = unitFactor * multiplier;
        sb.AppendFormat("\tAsynchronous data access time is {0}{1}", result, unit).AppendLine();

        sb.AppendFormat("\tClock dependent part of data access is {0} clock cycles", csd.NSAC * 100).AppendLine();

        unit = "MHz";

        switch(csd.Speed & 0x07)
        {
            case 0:
                unitFactor = 0.1;

                break;
            case 1:
                unitFactor = 1;

                break;
            case 2:
                unitFactor = 10;

                break;
            case 3:
                unitFactor = 100;

                break;
            default:
                unit       = "unknown";
                unitFactor = 0;

                break;
        }

        switch((csd.Speed & 0x78) >> 3)
        {
            case 0:
                multiplier = 0;

                break;
            case 1:
                multiplier = 1;

                break;
            case 2:
                multiplier = 1.2;

                break;
            case 3:
                multiplier = 1.3;

                break;
            case 4:
                multiplier = 1.5;

                break;
            case 5:
                multiplier = 2;

                break;
            case 6:
                multiplier = 2.6;

                break;
            case 7:
                multiplier = 3;

                break;
            case 8:
                multiplier = 3.5;

                break;
            case 9:
                multiplier = 4;

                break;
            case 10:
                multiplier = 4.5;

                break;
            case 11:
                multiplier = 5.2;

                break;
            case 12:
                multiplier = 5.5;

                break;
            case 13:
                multiplier = 6;

                break;
            case 14:
                multiplier = 7;

                break;
            case 15:
                multiplier = 8;

                break;
        }

        result = unitFactor * multiplier;
        sb.AppendFormat("\tDevice's clock frequency: {0}{1}", result, unit).AppendLine();

        unit = "";

        for(int cl = 0, mask = 1; cl <= 11; cl++, mask <<= 1)
            if((csd.Classes & mask) == mask)
                unit += $" {cl}";

        sb.AppendFormat("\tDevice support command classes {0}", unit).AppendLine();

        if(csd.ReadBlockLength == 15)
            sb.AppendLine("\tRead block length size is defined in extended CSD");
        else
            sb.AppendFormat("\tRead block length is {0} bytes", Math.Pow(2, csd.ReadBlockLength)).AppendLine();

        if(csd.ReadsPartialBlocks)
            sb.AppendLine("\tDevice allows reading partial blocks");

        if(csd.WriteMisalignment)
            sb.AppendLine("\tWrite commands can cross physical block boundaries");

        if(csd.ReadMisalignment)
            sb.AppendLine("\tRead commands can cross physical block boundaries");

        if(csd.DSRImplemented)
            sb.AppendLine("\tDevice implements configurable driver stage");

        if(csd.Size == 0xFFF)
            sb.AppendLine("\tDevice may be bigger than 2GiB and have its real size defined in the extended CSD");

        result = (csd.Size + 1) * Math.Pow(2, csd.SizeMultiplier + 2);
        sb.AppendFormat("\tDevice has {0} blocks", (int)result).AppendLine();

        result = (csd.Size + 1) * Math.Pow(2, csd.SizeMultiplier + 2) * Math.Pow(2, csd.ReadBlockLength);

        switch(result)
        {
            case > 1073741824:
                sb.AppendFormat("\tDevice has {0} GiB", result / 1073741824.0).AppendLine();

                break;
            case > 1048576:
                sb.AppendFormat("\tDevice has {0} MiB", result / 1048576.0).AppendLine();

                break;
            case > 1024:
                sb.AppendFormat("\tDevice has {0} KiB", result / 1024.0).AppendLine();

                break;
            default:
                sb.AppendFormat("\tDevice has {0} bytes", result).AppendLine();

                break;
        }

        switch(csd.ReadCurrentAtVddMin & 0x07)
        {
            case 0:
                sb.AppendLine("\tDevice uses a maximum of 0.5mA for reading at minimum voltage");

                break;
            case 1:
                sb.AppendLine("\tDevice uses a maximum of 1mA for reading at minimum voltage");

                break;
            case 2:
                sb.AppendLine("\tDevice uses a maximum of 5mA for reading at minimum voltage");

                break;
            case 3:
                sb.AppendLine("\tDevice uses a maximum of 10mA for reading at minimum voltage");

                break;
            case 4:
                sb.AppendLine("\tDevice uses a maximum of 25mA for reading at minimum voltage");

                break;
            case 5:
                sb.AppendLine("\tDevice uses a maximum of 35mA for reading at minimum voltage");

                break;
            case 6:
                sb.AppendLine("\tDevice uses a maximum of 60mA for reading at minimum voltage");

                break;
            case 7:
                sb.AppendLine("\tDevice uses a maximum of 100mA for reading at minimum voltage");

                break;
        }

        switch(csd.ReadCurrentAtVddMax & 0x07)
        {
            case 0:
                sb.AppendLine("\tDevice uses a maximum of 1mA for reading at maximum voltage");

                break;
            case 1:
                sb.AppendLine("\tDevice uses a maximum of 5mA for reading at maximum voltage");

                break;
            case 2:
                sb.AppendLine("\tDevice uses a maximum of 10mA for reading at maximum voltage");

                break;
            case 3:
                sb.AppendLine("\tDevice uses a maximum of 25mA for reading at maximum voltage");

                break;
            case 4:
                sb.AppendLine("\tDevice uses a maximum of 35mA for reading at maximum voltage");

                break;
            case 5:
                sb.AppendLine("\tDevice uses a maximum of 45mA for reading at maximum voltage");

                break;
            case 6:
                sb.AppendLine("\tDevice uses a maximum of 80mA for reading at maximum voltage");

                break;
            case 7:
                sb.AppendLine("\tDevice uses a maximum of 200mA for reading at maximum voltage");

                break;
        }

        switch(csd.WriteCurrentAtVddMin & 0x07)
        {
            case 0:
                sb.AppendLine("\tDevice uses a maximum of 0.5mA for writing at minimum voltage");

                break;
            case 1:
                sb.AppendLine("\tDevice uses a maximum of 1mA for writing at minimum voltage");

                break;
            case 2:
                sb.AppendLine("\tDevice uses a maximum of 5mA for writing at minimum voltage");

                break;
            case 3:
                sb.AppendLine("\tDevice uses a maximum of 10mA for writing at minimum voltage");

                break;
            case 4:
                sb.AppendLine("\tDevice uses a maximum of 25mA for writing at minimum voltage");

                break;
            case 5:
                sb.AppendLine("\tDevice uses a maximum of 35mA for writing at minimum voltage");

                break;
            case 6:
                sb.AppendLine("\tDevice uses a maximum of 60mA for writing at minimum voltage");

                break;
            case 7:
                sb.AppendLine("\tDevice uses a maximum of 100mA for writing at minimum voltage");

                break;
        }

        switch(csd.WriteCurrentAtVddMax & 0x07)
        {
            case 0:
                sb.AppendLine("\tDevice uses a maximum of 1mA for writing at maximum voltage");

                break;
            case 1:
                sb.AppendLine("\tDevice uses a maximum of 5mA for writing at maximum voltage");

                break;
            case 2:
                sb.AppendLine("\tDevice uses a maximum of 10mA for writing at maximum voltage");

                break;
            case 3:
                sb.AppendLine("\tDevice uses a maximum of 25mA for writing at maximum voltage");

                break;
            case 4:
                sb.AppendLine("\tDevice uses a maximum of 35mA for writing at maximum voltage");

                break;
            case 5:
                sb.AppendLine("\tDevice uses a maximum of 45mA for writing at maximum voltage");

                break;
            case 6:
                sb.AppendLine("\tDevice uses a maximum of 80mA for writing at maximum voltage");

                break;
            case 7:
                sb.AppendLine("\tDevice uses a maximum of 200mA for writing at maximum voltage");

                break;
        }

        // TODO: Check specification
        unitFactor = Convert.ToDouble(csd.EraseGroupSize);
        multiplier = Convert.ToDouble(csd.EraseGroupSizeMultiplier);
        result     = (unitFactor + 1) * (multiplier + 1);
        sb.AppendFormat("\tDevice can erase a minimum of {0} blocks at a time", (int)result).AppendLine();

        if(csd.WriteProtectGroupEnable)
        {
            sb.AppendLine("\tDevice can write protect regions");

            // TODO: Check specification
            // unitFactor = Convert.ToDouble(csd.WriteProtectGroupSize);

            sb.AppendFormat("\tDevice can write protect a minimum of {0} blocks at a time", (int)(result + 1)).
               AppendLine();
        }
        else
            sb.AppendLine("\tDevice can't write protect regions");

        switch(csd.DefaultECC)
        {
            case 0:
                sb.AppendLine("\tDevice uses no ECC by default");

                break;
            case 1:
                sb.AppendLine("\tDevice uses BCH(542, 512) ECC by default");

                break;
            case 2:
                sb.AppendFormat("\tDevice uses unknown ECC code {0} by default", csd.DefaultECC).AppendLine();

                break;
        }

        sb.AppendFormat("\tWriting is {0} times slower than reading", Math.Pow(2, csd.WriteSpeedFactor)).AppendLine();

        if(csd.WriteBlockLength == 15)
            sb.AppendLine("\tWrite block length size is defined in extended CSD");
        else
            sb.AppendFormat("\tWrite block length is {0} bytes", Math.Pow(2, csd.WriteBlockLength)).AppendLine();

        if(csd.WritesPartialBlocks)
            sb.AppendLine("\tDevice allows writing partial blocks");

        if(csd.ContentProtection)
            sb.AppendLine("\tDevice supports content protection");

        if(!csd.Copy)
            sb.AppendLine("\tDevice contents are original");

        if(csd.PermanentWriteProtect)
            sb.AppendLine("\tDevice is permanently write protected");

        if(csd.TemporaryWriteProtect)
            sb.AppendLine("\tDevice is temporarily write protected");

        if(!csd.FileFormatGroup)
            switch(csd.FileFormat)
            {
                case 0:
                    sb.AppendLine("\tDevice is formatted like a hard disk");

                    break;
                case 1:
                    sb.AppendLine("\tDevice is formatted like a floppy disk using Microsoft FAT");

                    break;
                case 2:
                    sb.AppendLine("\tDevice uses Universal File Format");

                    break;
                default:
                    sb.AppendFormat("\tDevice uses unknown file format code {0}", csd.FileFormat).AppendLine();

                    break;
            }
        else
            sb.AppendFormat("\tDevice uses unknown file format code {0} and file format group 1", csd.FileFormat).
               AppendLine();

        switch(csd.ECC)
        {
            case 0:
                sb.AppendLine("\tDevice currently uses no ECC");

                break;
            case 1:
                sb.AppendLine("\tDevice currently uses BCH(542, 512) ECC by default");

                break;
            case 2:
                sb.AppendFormat("\tDevice currently uses unknown ECC code {0}", csd.DefaultECC).AppendLine();

                break;
        }

        sb.AppendFormat("\tCSD CRC: 0x{0:X2}", csd.CRC).AppendLine();

        return sb.ToString();
    }

    public static string PrettifyCSD(uint[] response) => PrettifyCSD(DecodeCSD(response));

    public static string PrettifyCSD(byte[] response) => PrettifyCSD(DecodeCSD(response));
}