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
// Copyright © 2011-2022 Natalia Portillo
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

        sb.AppendLine("MultiMediaCard Device Identification Register:");
        sb.AppendFormat("\tManufacturer: {0}", VendorString.Prettify(cid.Manufacturer)).AppendLine();

        switch(cid.DeviceType)
        {
            case 0:
                sb.AppendLine("\tRemovable device");

                break;
            case 1:
                sb.AppendLine("\tBGA device");

                break;
            case 2:
                sb.AppendLine("\tPOP device");

                break;
        }

        sb.AppendFormat("\tApplication ID: {0}", cid.ApplicationID).AppendLine();
        sb.AppendFormat("\tProduct name: {0}", cid.ProductName).AppendLine();

        sb.AppendFormat("\tProduct revision: {0:X2}.{1:X2}", (cid.ProductRevision & 0xF0) >> 4,
                        cid.ProductRevision & 0x0F).AppendLine();

        sb.AppendFormat("\tProduct serial number: {0}", cid.ProductSerialNumber).AppendLine();

        string year = (cid.ManufacturingDate & 0x0F) switch
        {
            0  => "1997 or 2013",
            1  => "1998 or 2014",
            2  => "1999 or 2015",
            3  => "2000 or 2016",
            4  => "2001 or 2017",
            5  => "2002 or 2018",
            6  => "2003 or 2019",
            7  => "2004 or 2020",
            8  => "2005 or 2021",
            9  => "2006 or 2022",
            10 => "2007 or 2023",
            11 => "2008 or 2024",
            12 => "2009 or 2025",
            13 => "2010",
            14 => "2011",
            15 => "2012",
            _  => ""
        };

        sb.AppendFormat("\tDevice manufactured month {0} of {1}", (cid.ManufacturingDate & 0xF0) >> 4, year).
           AppendLine();

        sb.AppendFormat("\tCID CRC: 0x{0:X2}", cid.CRC).AppendLine();

        return sb.ToString();
    }

    public static string PrettifyCID(uint[] response) => PrettifyCID(DecodeCID(response));

    public static string PrettifyCID(byte[] response) => PrettifyCID(DecodeCID(response));
}