// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CID.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes MultiMediaCard CID.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Decoders.MMC;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "UnassignedField.Global")]
public class CID
{
    public byte   ApplicationID;
    public byte   CRC;
    public byte   DeviceType;
    public byte   Manufacturer;
    public byte   ManufacturingDate;
    public string ProductName;
    public byte   ProductRevision;
    public uint   ProductSerialNumber;
}

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Decoders
{
    public static CID DecodeCID(uint[] response)
    {
        if(response?.Length != 4)
            return null;

        byte[] data = new byte[16];

        byte[] tmp = BitConverter.GetBytes(response[0]);
        Array.Copy(tmp, 0, data, 0, 4);
        tmp = BitConverter.GetBytes(response[1]);
        Array.Copy(tmp, 0, data, 4, 4);
        tmp = BitConverter.GetBytes(response[2]);
        Array.Copy(tmp, 0, data, 8, 4);
        tmp = BitConverter.GetBytes(response[3]);
        Array.Copy(tmp, 0, data, 12, 4);

        return DecodeCID(data);
    }

    public static CID DecodeCID(byte[] response)
    {
        if(response?.Length != 16)
            return null;

        var cid = new CID
        {
            Manufacturer        = response[0],
            DeviceType          = (byte)(response[1] & 0x03),
            ProductRevision     = response[9],
            ProductSerialNumber = Swapping.Swap(BitConverter.ToUInt32(response, 10)),
            ManufacturingDate   = response[14],
            CRC                 = (byte)((response[15] & 0xFE) >> 1)
        };

        byte[] tmp = new byte[6];
        Array.Copy(response, 3, tmp, 0, 6);
        cid.ProductName = StringHandlers.CToString(tmp);

        return cid;
    }

    public static string PrettifyCID(CID cid)
    {
        if(cid == null)
            return null;

        var sb = new StringBuilder();

        sb.AppendLine(Localization.MultiMediaCard_Device_Identification_Register);
        sb.AppendFormat("\t" + Localization.Manufacturer_0, VendorString.Prettify(cid.Manufacturer)).AppendLine();

        switch(cid.DeviceType)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Removable_device);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.BGA_device);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.POP_device);

                break;
        }

        sb.AppendFormat("\t" + Localization.Application_ID_0, cid.ApplicationID).AppendLine();
        sb.AppendFormat("\t" + Localization.Product_name_0, cid.ProductName).AppendLine();

        sb.AppendFormat("\t" + Localization.Product_revision_0_1, (cid.ProductRevision & 0xF0) >> 4,
                        cid.ProductRevision & 0x0F).AppendLine();

        sb.AppendFormat("\t" + Localization.Product_serial_number_0, cid.ProductSerialNumber).AppendLine();

        string year = (cid.ManufacturingDate & 0x0F) switch
        {
            0  => Localization._1997_or_2013,
            1  => Localization._1998_or_2014,
            2  => Localization._1999_or_2015,
            3  => Localization._2000_or_2016,
            4  => Localization._2001_or_2017,
            5  => Localization._2002_or_2018,
            6  => Localization._2003_or_2019,
            7  => Localization._2004_or_2020,
            8  => Localization._2005_or_2021,
            9  => Localization._2006_or_2022,
            10 => Localization._2007_or_2023,
            11 => Localization._2008_or_2024,
            12 => Localization._2009_or_2025,
            13 => "2010",
            14 => "2011",
            15 => "2012",
            _  => ""
        };

        sb.AppendFormat("\t" + Localization.Device_manufactured_month_0_of_1, (cid.ManufacturingDate & 0xF0) >> 4,
                        year).AppendLine();

        sb.AppendFormat("\t" + Localization.CID_CRC_0, cid.CRC).AppendLine();

        return sb.ToString();
    }

    public static string PrettifyCID(uint[] response) => PrettifyCID(DecodeCID(response));

    public static string PrettifyCID(byte[] response) => PrettifyCID(DecodeCID(response));
}