// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ZFS filesystem plugin.
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Filesystems;

/*
 * The ZFS on-disk structure is quite undocumented, so this has been checked using several test images and reading the comments and headers (but not the code)
 * of ZFS-On-Linux.
 *
 * The most basic structure, the vdev label, is as follows:
 * 8KiB of blank space
 * 8KiB reserved for boot code, stored as a ZIO block with magic and checksum
 * 112KiB of nvlist, usually encoded using XDR
 * 128KiB of copies of the 1KiB uberblock
 *
 * Two vdev labels, L0 and L1 are stored at the start of the vdev.
 * Another two, L2 and L3 are stored at the end.
 *
 * The nvlist is nothing more than a double linked list of name/value pairs where name is a string and value is an arbitrary type (and can be an array of it).
 * On-disk they are stored sequentially (no pointers) and can be encoded in XDR (an old Sun serialization method that stores everything as 4 bytes chunks) or
 * natively (that is as the host natively stores that values, for example on Intel an extended float would be 10 bytes (80 bit).
 * It can also be encoded little or big endian.
 * Because of this variations, ZFS stored a header indicating the used encoding and endianess before the encoded nvlist.
 */
/// <inheritdoc />
/// <summary>Implements detection for the Zettabyte File System (ZFS)</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Local")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public sealed partial class ZFS
{
    static bool DecodeNvList(byte[] nvlist, out Dictionary<string, NVS_Item> decodedNvList)
    {
        var tmp = new byte[nvlist.Length - 4];
        Array.Copy(nvlist, 4, tmp, 0, nvlist.Length - 4);
        bool xdr          = nvlist[0] == 1;
        bool littleEndian = nvlist[1] == 1;

        return DecodeNvList(tmp, out decodedNvList, xdr, littleEndian);
    }

    // TODO: Decode native nvlist
    static bool DecodeNvList(byte[] nvlist, out Dictionary<string, NVS_Item> decodedNvList, bool xdr, bool littleEndian)
    {
        decodedNvList = new Dictionary<string, NVS_Item>();

        if(nvlist        == null ||
           nvlist.Length < 16)
            return false;

        if(!xdr)
            return false;

        var offset = 8;

        while(offset < nvlist.Length)
        {
            var item    = new NVS_Item();
            int currOff = offset;

            item.encodedSize = BigEndianBitConverter.ToUInt32(nvlist, offset);

            // Finished
            if(item.encodedSize == 0)
                break;

            offset           += 4;
            item.decodedSize =  BigEndianBitConverter.ToUInt32(nvlist, offset);
            offset           += 4;
            var nameLength = BigEndianBitConverter.ToUInt32(nvlist, offset);
            offset += 4;

            if(nameLength % 4 > 0)
                nameLength += 4 - nameLength % 4;

            var nameBytes = new byte[nameLength];
            Array.Copy(nvlist, offset, nameBytes, 0, nameLength);
            item.name     =  StringHandlers.CToString(nameBytes);
            offset        += (int)nameLength;
            item.dataType =  (NVS_DataTypes)BigEndianBitConverter.ToUInt32(nvlist, offset);
            offset        += 4;
            item.elements =  BigEndianBitConverter.ToUInt32(nvlist, offset);
            offset        += 4;

            if(item.elements == 0)
            {
                decodedNvList.Add(item.name, item);

                continue;
            }

            switch(item.dataType)
            {
                case NVS_DataTypes.DATA_TYPE_BOOLEAN:
                case NVS_DataTypes.DATA_TYPE_BOOLEAN_ARRAY:
                case NVS_DataTypes.DATA_TYPE_BOOLEAN_VALUE:
                    if(item.elements > 1)
                    {
                        var boolArray = new bool[item.elements];

                        for(var i = 0; i < item.elements; i++)
                        {
                            var temp = BigEndianBitConverter.ToUInt32(nvlist, offset);
                            boolArray[i] =  temp > 0;
                            offset       += 4;
                        }

                        item.value = boolArray;
                    }
                    else
                    {
                        var temp = BigEndianBitConverter.ToUInt32(nvlist, offset);
                        item.value =  temp > 0;
                        offset     += 4;
                    }

                    break;
                case NVS_DataTypes.DATA_TYPE_BYTE:
                case NVS_DataTypes.DATA_TYPE_BYTE_ARRAY:
                case NVS_DataTypes.DATA_TYPE_UINT8:
                case NVS_DataTypes.DATA_TYPE_UINT8_ARRAY:
                    if(item.elements > 1)
                    {
                        var byteArray = new byte[item.elements];
                        Array.Copy(nvlist, offset, byteArray, 0, item.elements);
                        offset += (int)item.elements;

                        if(item.elements % 4 > 0)
                            offset += 4 - (int)(item.elements % 4);

                        item.value = byteArray;
                    }
                    else
                    {
                        item.value =  nvlist[offset];
                        offset     += 4;
                    }

                    break;
                case NVS_DataTypes.DATA_TYPE_DOUBLE:
                    if(item.elements > 1)
                    {
                        var doubleArray = new double[item.elements];

                        for(var i = 0; i < item.elements; i++)
                        {
                            var temp = BigEndianBitConverter.ToDouble(nvlist, offset);
                            doubleArray[i] =  temp;
                            offset         += 8;
                        }

                        item.value = doubleArray;
                    }
                    else
                    {
                        item.value =  BigEndianBitConverter.ToDouble(nvlist, offset);
                        offset     += 8;
                    }

                    break;
                case NVS_DataTypes.DATA_TYPE_HRTIME:
                    if(item.elements > 1)
                    {
                        var hrtimeArray = new DateTime[item.elements];

                        for(var i = 0; i < item.elements; i++)
                        {
                            DateTime temp =
                                DateHandlers.UnixHrTimeToDateTime(BigEndianBitConverter.ToUInt64(nvlist, offset));

                            hrtimeArray[i] =  temp;
                            offset         += 8;
                        }

                        item.value = hrtimeArray;
                    }
                    else
                    {
                        item.value = DateHandlers.UnixHrTimeToDateTime(BigEndianBitConverter.ToUInt64(nvlist, offset));

                        offset += 8;
                    }

                    break;
                case NVS_DataTypes.DATA_TYPE_INT16:
                case NVS_DataTypes.DATA_TYPE_INT16_ARRAY:
                    if(item.elements > 1)
                    {
                        var shortArray = new short[item.elements];

                        for(var i = 0; i < item.elements; i++)
                        {
                            var temp = BigEndianBitConverter.ToInt16(nvlist, offset);
                            shortArray[i] =  temp;
                            offset        += 4;
                        }

                        item.value = shortArray;
                    }
                    else
                    {
                        item.value =  BigEndianBitConverter.ToInt16(nvlist, offset);
                        offset     += 4;
                    }

                    break;
                case NVS_DataTypes.DATA_TYPE_INT32:
                case NVS_DataTypes.DATA_TYPE_INT32_ARRAY:
                    if(item.elements > 1)
                    {
                        var intArray = new int[item.elements];

                        for(var i = 0; i < item.elements; i++)
                        {
                            var temp = BigEndianBitConverter.ToInt32(nvlist, offset);
                            intArray[i] =  temp;
                            offset      += 4;
                        }

                        item.value = intArray;
                    }
                    else
                    {
                        item.value =  BigEndianBitConverter.ToInt32(nvlist, offset);
                        offset     += 4;
                    }

                    break;
                case NVS_DataTypes.DATA_TYPE_INT64:
                case NVS_DataTypes.DATA_TYPE_INT64_ARRAY:
                    if(item.elements > 1)
                    {
                        var longArray = new long[item.elements];

                        for(var i = 0; i < item.elements; i++)
                        {
                            var temp = BigEndianBitConverter.ToInt64(nvlist, offset);
                            longArray[i] =  temp;
                            offset       += 8;
                        }

                        item.value = longArray;
                    }
                    else
                    {
                        item.value =  BigEndianBitConverter.ToInt64(nvlist, offset);
                        offset     += 8;
                    }

                    break;
                case NVS_DataTypes.DATA_TYPE_INT8:
                case NVS_DataTypes.DATA_TYPE_INT8_ARRAY:
                    if(item.elements > 1)
                    {
                        var sbyteArray = new sbyte[item.elements];

                        for(var i = 0; i < item.elements; i++)
                        {
                            var temp = (sbyte)nvlist[offset];
                            sbyteArray[i] = temp;
                            offset++;
                        }

                        item.value = sbyteArray;

                        if(sbyteArray.Length % 4 > 0)
                            offset += 4 - sbyteArray.Length % 4;
                    }
                    else
                    {
                        item.value =  BigEndianBitConverter.ToInt64(nvlist, offset);
                        offset     += 4;
                    }

                    break;
                case NVS_DataTypes.DATA_TYPE_STRING:
                case NVS_DataTypes.DATA_TYPE_STRING_ARRAY:
                    if(item.elements > 1)
                    {
                        var stringArray = new string[item.elements];

                        for(var i = 0; i < item.elements; i++)
                        {
                            var strLength = BigEndianBitConverter.ToUInt32(nvlist, offset);
                            offset += 4;
                            var strBytes = new byte[strLength];
                            Array.Copy(nvlist, offset, strBytes, 0, strLength);
                            stringArray[i] =  StringHandlers.CToString(strBytes);
                            offset         += (int)strLength;

                            if(strLength % 4 > 0)
                                offset += 4 - (int)(strLength % 4);
                        }

                        item.value = stringArray;
                    }
                    else
                    {
                        var strLength = BigEndianBitConverter.ToUInt32(nvlist, offset);
                        offset += 4;
                        var strBytes = new byte[strLength];
                        Array.Copy(nvlist, offset, strBytes, 0, strLength);
                        item.value =  StringHandlers.CToString(strBytes);
                        offset     += (int)strLength;

                        if(strLength % 4 > 0)
                            offset += 4 - (int)(strLength % 4);
                    }

                    break;
                case NVS_DataTypes.DATA_TYPE_UINT16:
                case NVS_DataTypes.DATA_TYPE_UINT16_ARRAY:
                    if(item.elements > 1)
                    {
                        var ushortArray = new ushort[item.elements];

                        for(var i = 0; i < item.elements; i++)
                        {
                            var temp = BigEndianBitConverter.ToUInt16(nvlist, offset);
                            ushortArray[i] =  temp;
                            offset         += 4;
                        }

                        item.value = ushortArray;
                    }
                    else
                    {
                        item.value =  BigEndianBitConverter.ToUInt16(nvlist, offset);
                        offset     += 4;
                    }

                    break;
                case NVS_DataTypes.DATA_TYPE_UINT32:
                case NVS_DataTypes.DATA_TYPE_UINT32_ARRAY:
                    if(item.elements > 1)
                    {
                        var uintArray = new uint[item.elements];

                        for(var i = 0; i < item.elements; i++)
                        {
                            var temp = BigEndianBitConverter.ToUInt32(nvlist, offset);
                            uintArray[i] =  temp;
                            offset       += 4;
                        }

                        item.value = uintArray;
                    }
                    else
                    {
                        item.value =  BigEndianBitConverter.ToUInt32(nvlist, offset);
                        offset     += 4;
                    }

                    break;
                case NVS_DataTypes.DATA_TYPE_UINT64:
                case NVS_DataTypes.DATA_TYPE_UINT64_ARRAY:
                    if(item.elements > 1)
                    {
                        var ulongArray = new ulong[item.elements];

                        for(var i = 0; i < item.elements; i++)
                        {
                            var temp = BigEndianBitConverter.ToUInt64(nvlist, offset);
                            ulongArray[i] =  temp;
                            offset        += 8;
                        }

                        item.value = ulongArray;
                    }
                    else
                    {
                        item.value =  BigEndianBitConverter.ToUInt64(nvlist, offset);
                        offset     += 8;
                    }

                    break;
                case NVS_DataTypes.DATA_TYPE_NVLIST:
                    if(item.elements > 1)
                        goto default;

                    var subListBytes = new byte[item.encodedSize - (offset - currOff)];
                    Array.Copy(nvlist, offset, subListBytes, 0, subListBytes.Length);

                    if(DecodeNvList(subListBytes, out Dictionary<string, NVS_Item> subList, true, littleEndian))
                        item.value = subList;
                    else
                        goto default;

                    offset = (int)(currOff + item.encodedSize);

                    break;
                default:
                    var unknown = new byte[item.encodedSize - (offset - currOff)];
                    Array.Copy(nvlist, offset, unknown, 0, unknown.Length);
                    item.value = unknown;
                    offset     = (int)(currOff + item.encodedSize);

                    break;
            }

            decodedNvList.Add(item.name, item);
        }

        return decodedNvList.Count > 0;
    }

    static string PrintNvList(Dictionary<string, NVS_Item> decodedNvList)
    {
        var sb = new StringBuilder();

        foreach(NVS_Item item in decodedNvList.Values)
        {
            if(item.elements == 0)
            {
                sb.AppendFormat(Localization._0_is_not_set, item.name).AppendLine();

                continue;
            }

            switch(item.dataType)
            {
                case NVS_DataTypes.DATA_TYPE_BOOLEAN:
                case NVS_DataTypes.DATA_TYPE_BOOLEAN_ARRAY:
                case NVS_DataTypes.DATA_TYPE_BOOLEAN_VALUE:
                    if(item.elements > 1)
                    {
                        for(var i = 0; i < item.elements; i++)
                            sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((bool[])item.value)[i]).AppendLine();
                    }
                    else
                        sb.AppendFormat("{0} = {1}", item.name, (bool)item.value).AppendLine();

                    break;
                case NVS_DataTypes.DATA_TYPE_BYTE:
                case NVS_DataTypes.DATA_TYPE_BYTE_ARRAY:
                case NVS_DataTypes.DATA_TYPE_UINT8:
                case NVS_DataTypes.DATA_TYPE_UINT8_ARRAY:
                    if(item.elements > 1)
                    {
                        for(var i = 0; i < item.elements; i++)
                            sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((byte[])item.value)[i]).AppendLine();
                    }
                    else
                        sb.AppendFormat("{0} = {1}", item.name, (byte)item.value).AppendLine();

                    break;
                case NVS_DataTypes.DATA_TYPE_DOUBLE:
                    if(item.elements > 1)
                    {
                        for(var i = 0; i < item.elements; i++)
                            sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((double[])item.value)[i]).AppendLine();
                    }
                    else
                        sb.AppendFormat("{0} = {1}", item.name, (double)item.value).AppendLine();

                    break;
                case NVS_DataTypes.DATA_TYPE_HRTIME:
                    if(item.elements > 1)
                    {
                        for(var i = 0; i < item.elements; i++)
                            sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((DateTime[])item.value)[i]).AppendLine();
                    }
                    else
                        sb.AppendFormat("{0} = {1}", item.name, (DateTime)item.value).AppendLine();

                    break;
                case NVS_DataTypes.DATA_TYPE_INT16:
                case NVS_DataTypes.DATA_TYPE_INT16_ARRAY:
                    if(item.elements > 1)
                    {
                        for(var i = 0; i < item.elements; i++)
                            sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((short[])item.value)[i]).AppendLine();
                    }
                    else
                        sb.AppendFormat("{0} = {1}", item.name, (short)item.value).AppendLine();

                    break;
                case NVS_DataTypes.DATA_TYPE_INT32:
                case NVS_DataTypes.DATA_TYPE_INT32_ARRAY:
                    if(item.elements > 1)
                    {
                        for(var i = 0; i < item.elements; i++)
                            sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((int[])item.value)[i]).AppendLine();
                    }
                    else
                        sb.AppendFormat("{0} = {1}", item.name, (int)item.value).AppendLine();

                    break;
                case NVS_DataTypes.DATA_TYPE_INT64:
                case NVS_DataTypes.DATA_TYPE_INT64_ARRAY:
                    if(item.elements > 1)
                    {
                        for(var i = 0; i < item.elements; i++)
                            sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((long[])item.value)[i]).AppendLine();
                    }
                    else
                        sb.AppendFormat("{0} = {1}", item.name, (long)item.value).AppendLine();

                    break;
                case NVS_DataTypes.DATA_TYPE_INT8:
                case NVS_DataTypes.DATA_TYPE_INT8_ARRAY:
                    if(item.elements > 1)
                    {
                        for(var i = 0; i < item.elements; i++)
                            sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((sbyte[])item.value)[i]).AppendLine();
                    }
                    else
                        sb.AppendFormat("{0} = {1}", item.name, (sbyte)item.value).AppendLine();

                    break;
                case NVS_DataTypes.DATA_TYPE_STRING:
                case NVS_DataTypes.DATA_TYPE_STRING_ARRAY:
                    if(item.elements > 1)
                    {
                        for(var i = 0; i < item.elements; i++)
                            sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((string[])item.value)[i]).AppendLine();
                    }
                    else
                        sb.AppendFormat("{0} = {1}", item.name, (string)item.value).AppendLine();

                    break;
                case NVS_DataTypes.DATA_TYPE_UINT16:
                case NVS_DataTypes.DATA_TYPE_UINT16_ARRAY:
                    if(item.elements > 1)
                    {
                        for(var i = 0; i < item.elements; i++)
                            sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((ushort[])item.value)[i]).AppendLine();
                    }
                    else
                        sb.AppendFormat("{0} = {1}", item.name, (ushort)item.value).AppendLine();

                    break;
                case NVS_DataTypes.DATA_TYPE_UINT32:
                case NVS_DataTypes.DATA_TYPE_UINT32_ARRAY:
                    if(item.elements > 1)
                    {
                        for(var i = 0; i < item.elements; i++)
                            sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((uint[])item.value)[i]).AppendLine();
                    }
                    else
                        sb.AppendFormat("{0} = {1}", item.name, (uint)item.value).AppendLine();

                    break;
                case NVS_DataTypes.DATA_TYPE_UINT64:
                case NVS_DataTypes.DATA_TYPE_UINT64_ARRAY:
                    if(item.elements > 1)
                    {
                        for(var i = 0; i < item.elements; i++)
                            sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((ulong[])item.value)[i]).AppendLine();
                    }
                    else
                        sb.AppendFormat("{0} = {1}", item.name, (ulong)item.value).AppendLine();

                    break;
                case NVS_DataTypes.DATA_TYPE_NVLIST:
                    if(item.elements == 1)
                    {
                        sb.AppendFormat("{0} =\n{1}", item.name, PrintNvList((Dictionary<string, NVS_Item>)item.value)).
                           AppendLine();
                    }
                    else
                    {
                        sb.AppendFormat(Localization._0_equals_1_elements_nvlist_array_unable_to_print, item.name,
                                        item.elements).AppendLine();
                    }

                    break;
                default:
                    if(item.elements > 1)
                    {
                        for(var i = 0; i < item.elements; i++)
                        {
                            sb.AppendFormat(Localization._0_1_equals_unknown_data_type_2, item.name, i, item.dataType).
                               AppendLine();
                        }
                    }
                    else
                    {
                        sb.AppendFormat(Localization._0_equals_unknown_data_type_1, item.name, item.dataType).
                           AppendLine();
                    }

                    break;
            }
        }

        return sb.ToString();
    }
}