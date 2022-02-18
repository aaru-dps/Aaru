// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CIS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes PCMCIA Card Information Structure.
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Decoders.PCMCIA
{
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class CIS
    {
        // TODO: Handle links? Or are they removed in lower layers of the operating system drivers?
        public static Tuple[] GetTuples(byte[] data)
        {
            List<Tuple> tuples   = new List<Tuple>();
            int         position = 0;

            while(position < data.Length)
            {
                var tuple = new Tuple
                {
                    Code = (TupleCodes)data[position]
                };

                if(tuple.Code == TupleCodes.CISTPL_NULL)
                    continue;

                if(tuple.Code == TupleCodes.CISTPL_END)
                    break;

                tuple.Link = data[position + 1];

                if(position + 2 + tuple.Link > data.Length)
                    break;

                tuple.Data = new byte[tuple.Link + 2];
                Array.Copy(data, position, tuple.Data, 0, tuple.Link + 2);

                tuples.Add(tuple);
                position += tuple.Link + 2;
            }

            return tuples.ToArray();
        }

        public static DeviceGeometryTuple DecodeDeviceGeometryTuple(Tuple tuple)
        {
            if(tuple == null)
                return null;

            if(tuple.Code != TupleCodes.CISTPL_DEVICEGEO &&
               tuple.Code != TupleCodes.CISTPL_DEVICEGEO_A)
                return null;

            return tuple.Data == null ? null : DecodeDeviceGeometryTuple(tuple.Data);
        }

        public static DeviceGeometryTuple DecodeDeviceGeometryTuple(byte[] data)
        {
            if((data?.Length - 2) % 6 != 0)
                return null;

            var                  tuple      = new DeviceGeometryTuple();
            List<DeviceGeometry> geometries = new List<DeviceGeometry>();

            for(int position = 2; position < data.Length; position += 6)
            {
                var geometry = new DeviceGeometry
                {
                    CardInterface  = data[position],
                    EraseBlockSize = data[position + 1],
                    ReadBlockSize  = data[position + 2],
                    WriteBlockSize = data[position + 3],
                    Partitions     = data[position + 4],
                    Interleaving   = data[position + 5]
                };

                geometries.Add(geometry);
            }

            tuple.Code       = (TupleCodes)data[0];
            tuple.Link       = data[1];
            tuple.Geometries = geometries.ToArray();

            return tuple;
        }

        public static string PrettifyDeviceGeometryTuple(DeviceGeometryTuple tuple)
        {
            if(tuple == null)
                return null;

            if(tuple.Code != TupleCodes.CISTPL_DEVICEGEO &&
               tuple.Code != TupleCodes.CISTPL_DEVICEGEO_A)
                return null;

            var sb = new StringBuilder();
            sb.AppendLine("PCMCIA Device Geometry Tuples:");

            foreach(DeviceGeometry geometry in tuple.Geometries)
            {
                sb.AppendLine("\tGeometry:");
                sb.AppendFormat("\t\tDevice width: {0} bits", (1 << (geometry.CardInterface - 1)) * 8).AppendLine();

                sb.AppendFormat("\t\tErase block = {0} bytes",
                                (1 << (geometry.EraseBlockSize - 1)) * (1 << (geometry.Interleaving - 1))).AppendLine();

                sb.AppendFormat("\t\tRead block = {0} bytes",
                                (1 << (geometry.ReadBlockSize - 1)) * (1 << (geometry.Interleaving - 1))).AppendLine();

                sb.AppendFormat("\t\tWrite block = {0} bytes",
                                (1 << (geometry.WriteBlockSize - 1)) * (1 << (geometry.Interleaving - 1))).AppendLine();

                sb.AppendFormat("\t\tPartition alignment = {0} bytes",
                                (1 << (geometry.EraseBlockSize - 1)) * (1 << (geometry.Interleaving - 1)) *
                                (1 << (geometry.Partitions     - 1))).AppendLine();
            }

            return sb.ToString();
        }

        public static string PrettifyDeviceGeometryTuple(Tuple tuple) =>
            PrettifyDeviceGeometryTuple(DecodeDeviceGeometryTuple(tuple));

        public static string PrettifyDeviceGeometryTuple(byte[] data) =>
            PrettifyDeviceGeometryTuple(DecodeDeviceGeometryTuple(data));

        public static ManufacturerIdentificationTuple DecodeManufacturerIdentificationTuple(Tuple tuple)
        {
            if(tuple?.Code != TupleCodes.CISTPL_MANFID)
                return null;

            return tuple.Data == null ? null : DecodeManufacturerIdentificationTuple(tuple.Data);
        }

        public static ManufacturerIdentificationTuple DecodeManufacturerIdentificationTuple(byte[] data)
        {
            if(data == null)
                return null;

            if(data.Length < 6)
                return null;

            return new ManufacturerIdentificationTuple
            {
                Code           = (TupleCodes)data[0],
                Link           = data[1],
                ManufacturerID = BitConverter.ToUInt16(data, 2),
                CardID         = BitConverter.ToUInt16(data, 4)
            };
        }

        public static string PrettifyManufacturerIdentificationTuple(ManufacturerIdentificationTuple tuple)
        {
            if(tuple?.Code != TupleCodes.CISTPL_MANFID)
                return null;

            var sb = new StringBuilder();
            sb.AppendLine("PCMCIA Manufacturer Identification Tuple:");
            sb.AppendFormat("\tManufacturer ID: {0}", VendorCode.Prettify(tuple.ManufacturerID)).AppendLine();
            sb.AppendFormat("\tCard ID:D 0x{0:X4}", tuple.CardID).AppendLine();

            return sb.ToString();
        }

        public static string PrettifyManufacturerIdentificationTuple(Tuple tuple) =>
            PrettifyManufacturerIdentificationTuple(DecodeManufacturerIdentificationTuple(tuple));

        public static string PrettifyManufacturerIdentificationTuple(byte[] data) =>
            PrettifyManufacturerIdentificationTuple(DecodeManufacturerIdentificationTuple(data));

        public static Level1VersionTuple DecodeLevel1VersionTuple(Tuple tuple)
        {
            if(tuple?.Code != TupleCodes.CISTPL_VERS_1)
                return null;

            return tuple.Data == null ? null : DecodeLevel1VersionTuple(tuple.Data);
        }

        public static Level1VersionTuple DecodeLevel1VersionTuple(byte[] data)
        {
            if(data == null)
                return null;

            if(data.Length < 4)
                return null;

            List<byte>   buffer       = new List<byte>();
            List<string> strings      = null;
            bool         firstString  = false;
            bool         secondString = false;

            var tuple = new Level1VersionTuple
            {
                Code         = (TupleCodes)data[0],
                Link         = data[1],
                MajorVersion = data[2],
                MinorVersion = data[3]
            };

            for(int position = 4; position < data.Length; position++)
            {
                if(data[position] == 0xFF)
                    break;

                buffer.Add(data[position]);

                if(data[position] != 0x00)
                    continue;

                if(!firstString)
                {
                    tuple.Manufacturer = StringHandlers.CToString(buffer.ToArray());
                    buffer             = new List<byte>();
                    firstString        = true;

                    continue;
                }

                // TODO: Check this
                if(!secondString)
                {
                    tuple.Product = StringHandlers.CToString(buffer.ToArray());
                    buffer        = new List<byte>();
                    firstString   = true;

                    continue;
                }

                if(strings == null)
                    strings = new List<string>();

                strings.Add(StringHandlers.CToString(buffer.ToArray()));
                buffer = new List<byte>();
            }

            if(strings != null)
                tuple.AdditionalInformation = strings.ToArray();

            return tuple;
        }

        public static string PrettifyLevel1VersionTuple(Level1VersionTuple tuple)
        {
            if(tuple?.Code != TupleCodes.CISTPL_VERS_1)
                return null;

            var sb = new StringBuilder();
            sb.AppendLine("PCMCIA Level 1 Version / Product Information Tuple:");

            sb.AppendFormat("\tCard indicates compliance with PC Card Standard Release {0}.{1}", tuple.MajorVersion,
                            tuple.MinorVersion).AppendLine();

            if(string.IsNullOrEmpty(tuple.Manufacturer))
                sb.AppendLine("\tNo manufacturer information string.");
            else
                sb.AppendFormat("\tManufacturer: {0}", tuple.Manufacturer).AppendLine();

            if(string.IsNullOrEmpty(tuple.Product))
                sb.AppendLine("\tNo product name string.");
            else
                sb.AppendFormat("\tProduct name: {0}", tuple.Product).AppendLine();

            if(tuple.AdditionalInformation        == null ||
               tuple.AdditionalInformation.Length == 0)
                sb.AppendLine("\tNo additional information.");
            else
            {
                sb.AppendLine("\tAdditional information:");

                foreach(string info in tuple.AdditionalInformation.Where(info => !string.IsNullOrEmpty(info)))
                    sb.AppendFormat("\t\t{0}", info).AppendLine();
            }

            return sb.ToString();
        }

        public static string PrettifyLevel1VersionTuple(Tuple tuple) =>
            PrettifyLevel1VersionTuple(DecodeLevel1VersionTuple(tuple));

        public static string PrettifyLevel1VersionTuple(byte[] data) =>
            PrettifyLevel1VersionTuple(DecodeLevel1VersionTuple(data));
    }
}