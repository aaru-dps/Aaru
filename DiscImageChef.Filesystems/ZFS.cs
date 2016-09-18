// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ZFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the ZFS filesystem and shows information.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DiscImageChef.Filesystems
{
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
    class ZFS : Filesystem
    {
        const ulong ZEC_Magic = 0x0210DA7AB10C7A11;
        const ulong ZEC_Cigam = 0x117A0CB17ADA1002;

        struct ZIO_Checksum
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ulong[] word;
        }

        /// <summary>
        /// There is an empty ZIO at sector 16 or sector 31, with magic and checksum, to detect it is really ZFS I suppose.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ZIO_Empty
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 472)]
            public byte[] empty;
            public ulong magic;
            public ZIO_Checksum checksum;
        }

        // These parameters define how the nvlist is stored
        const byte NVS_LittleEndian = 1;
        const byte NVS_BigEndian = 0;
        const byte NVS_Native = 0;
        const byte NVS_XDR = 1;

        /// <summary>
        /// This structure indicates which encoding method and endianness is used to encode the nvlist
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct NVS_Method
        {
            public byte encoding;
            public byte endian;
            public byte reserved1;
            public byte reserved2;
        }

        /// <summary>
        /// This structure gives information about the encoded nvlist
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct NVS_XDR_Header
        {
            public NVS_Method encodingAndEndian;
            public uint version;
            public uint flags;
        }

        enum NVS_DataTypes : uint
        {
            DATA_TYPE_UNKNOWN = 0,
            DATA_TYPE_BOOLEAN,
            DATA_TYPE_BYTE,
            DATA_TYPE_INT16,
            DATA_TYPE_UINT16,
            DATA_TYPE_INT32,
            DATA_TYPE_UINT32,
            DATA_TYPE_INT64,
            DATA_TYPE_UINT64,
            DATA_TYPE_STRING,
            DATA_TYPE_BYTE_ARRAY,
            DATA_TYPE_INT16_ARRAY,
            DATA_TYPE_UINT16_ARRAY,
            DATA_TYPE_INT32_ARRAY,
            DATA_TYPE_UINT32_ARRAY,
            DATA_TYPE_INT64_ARRAY,
            DATA_TYPE_UINT64_ARRAY,
            DATA_TYPE_STRING_ARRAY,
            DATA_TYPE_HRTIME,
            DATA_TYPE_NVLIST,
            DATA_TYPE_NVLIST_ARRAY,
            DATA_TYPE_BOOLEAN_VALUE,
            DATA_TYPE_INT8,
            DATA_TYPE_UINT8,
            DATA_TYPE_BOOLEAN_ARRAY,
            DATA_TYPE_INT8_ARRAY,
            DATA_TYPE_UINT8_ARRAY,
            DATA_TYPE_DOUBLE
        }

        /// <summary>
        /// This represent an encoded nvpair (an item of an nvlist)
        /// </summary>
        struct NVS_Item
        {
            /// <summary>
            /// Size in bytes when encoded in XDR
            /// </summary>
            public uint encodedSize;
            /// <summary>
            /// Size in bytes when decoded
            /// </summary>
            public uint decodedSize;
            /// <summary>
            /// On disk, it is null-padded for alignment to 4 bytes and prepended by a 4 byte length indicator
            /// </summary>
            public string name;
            /// <summary>
            /// Data type
            /// </summary>
            public NVS_DataTypes dataType;
            /// <summary>
            /// How many elements are here
            /// </summary>
            public uint elements;
            /// <summary>
            /// On disk size is relative to <see cref="dataType"/> and <see cref="elements"/> always aligned to 4 bytes
            /// </summary>
            public object value;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DVA
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public ulong[] word;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SPA_BlockPointer
        {
            /// <summary>
            /// Data virtual address
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public DVA[] dataVirtualAddress;
            /// <summary>
            /// Block properties
            /// </summary>
            public ulong properties;
            /// <summary>
            /// Reserved for future expansion
            /// </summary>
            public ulong[] padding;
            /// <summary>
            /// TXG when block was allocated
            /// </summary>
            public ulong birthTxg;
            /// <summary>
            /// Transaction group at birth
            /// </summary>
            public ulong birth;
            /// <summary>
            /// Fill count
            /// </summary>
            public ulong fill;
            public ZIO_Checksum checksum;
        }

        const ulong Uberblock_Magic = 0x00BAB10C;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ZFS_Uberblock
        {
            public ulong magic;
            public ulong spaVersion;
            public ulong lastTxg;
            public ulong guidSum;
            public ulong timestamp;
            public SPA_BlockPointer mosPtr;
            public ulong softwareVersion;
        }

        const uint ZFS_Magic = 0x58465342;

        public ZFS()
        {
            Name = "ZFS Filesystem Plugin";
            PluginUUID = new Guid("0750014F-A714-4692-A369-E23F6EC3659C");
        }

        public ZFS(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            Name = "ZFS Filesystem Plugin";
            PluginUUID = new Guid("0750014F-A714-4692-A369-E23F6EC3659C");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if(imagePlugin.GetSectorSize() < 512)
                return false;

            byte[] sector;
            ulong magic;

            if(partitionStart + 31 < partitionEnd)
            {
                sector = imagePlugin.ReadSector(partitionStart + 31);
                magic = BitConverter.ToUInt64(sector, 0x1D8);
                if(magic == ZEC_Magic || magic == ZEC_Cigam)
                    return true;
            }

            if(partitionStart + 16 < partitionEnd)
            {
                sector = imagePlugin.ReadSector(partitionStart + 16);
                magic = BitConverter.ToUInt64(sector, 0x1D8);
                if(magic == ZEC_Magic || magic == ZEC_Cigam)
                    return true;
            }

            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";
            if(imagePlugin.GetSectorSize() < 512)
                return;

            byte[] sector;
            ulong magic;

            ulong nvlistOff = 32;
            uint nvlistLen = 114688 / imagePlugin.ImageInfo.sectorSize;
            byte[] nvlist;

            if(partitionStart + 31 < partitionEnd)
            {
                sector = imagePlugin.ReadSector(partitionStart + 31);
                magic = BitConverter.ToUInt64(sector, 0x1D8);
                if(magic == ZEC_Magic || magic == ZEC_Cigam)
                    nvlistOff = 32;
            }

            if(partitionStart + 16 < partitionEnd)
            {
                sector = imagePlugin.ReadSector(partitionStart + 16);
                magic = BitConverter.ToUInt64(sector, 0x1D8);
                if(magic == ZEC_Magic || magic == ZEC_Cigam)
                    nvlistOff = 17;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ZFS filesystem");

            nvlist = imagePlugin.ReadSectors(partitionStart + nvlistOff, nvlistLen);
            Dictionary<string, NVS_Item> decodedNvList;

            if(!DecodeNvList(nvlist, out decodedNvList))
                sb.AppendLine("Could not decode nvlist");
            else
                sb.AppendLine(PrintNvList(decodedNvList));

            information = sb.ToString();

            NVS_Item tmpObj;

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Type = "ZFS filesystem";
            if(decodedNvList.TryGetValue("name", out tmpObj))
                xmlFSType.VolumeName = (string)tmpObj.value;
            if(decodedNvList.TryGetValue("guid", out tmpObj))
                xmlFSType.VolumeSerial = string.Format("{0}", (ulong)tmpObj.value);
            if(decodedNvList.TryGetValue("pool_guid", out tmpObj))
                xmlFSType.VolumeSetIdentifier = string.Format("{0}", (ulong)tmpObj.value);
        }

        static bool DecodeNvList(byte[] nvlist, out Dictionary<string, NVS_Item> decodedNvList)
        {
            byte[] tmp = new byte[nvlist.Length - 4];
            Array.Copy(nvlist, 4, tmp, 0, nvlist.Length - 4);
            bool xdr = nvlist[0] == 1;
            bool littleEndian = nvlist[1] == 1;

            return DecodeNvList(tmp, out decodedNvList, xdr, littleEndian);
        }

        // TODO: Decode native nvlist
        static bool DecodeNvList(byte[] nvlist, out Dictionary<string, NVS_Item> decodedNvList, bool xdr, bool littleEndian)
        {
            decodedNvList = new Dictionary<string, NVS_Item>();

            if(nvlist == null || nvlist.Length < 16)
                return false;

            int offset = 0;
            if(!xdr)
                return false;

            BigEndianBitConverter.IsLittleEndian = littleEndian;

            offset = 8;
            while(offset < nvlist.Length)
            {
                uint nameLength;
                byte[] nameBytes;
                NVS_Item item = new NVS_Item();
                int currOff = offset;

                item.encodedSize = BigEndianBitConverter.ToUInt32(nvlist, offset);

                // Finished
                if(item.encodedSize == 0)
                    break;
                
                offset += 4;
                item.decodedSize = BigEndianBitConverter.ToUInt32(nvlist, offset);
                offset += 4;
                nameLength = BigEndianBitConverter.ToUInt32(nvlist, offset);
                offset += 4;
                if(nameLength % 4 > 0)
                    nameLength += 4 - (nameLength % 4);
                nameBytes = new byte[nameLength];
                Array.Copy(nvlist, offset, nameBytes, 0, nameLength);
                item.name = StringHandlers.CToString(nameBytes);
                offset += (int)nameLength;
                item.dataType = (NVS_DataTypes)BigEndianBitConverter.ToUInt32(nvlist, offset);
                offset += 4;
                item.elements = BigEndianBitConverter.ToUInt32(nvlist, offset);
                offset += 4;

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
                            bool[] boolArray = new bool[item.elements];
                            for(int i = 0; i < item.elements; i++)
                            {
                                uint temp = BigEndianBitConverter.ToUInt32(nvlist, offset);
                                boolArray[i] = temp > 0;
                                offset += 4;
                            }
                            item.value = boolArray;
                        }
                        else
                        {
                            uint temp = BigEndianBitConverter.ToUInt32(nvlist, offset);
                            item.value = (temp > 0);
                            offset += 4;
                        }
                        break;
                    case NVS_DataTypes.DATA_TYPE_BYTE:
                    case NVS_DataTypes.DATA_TYPE_BYTE_ARRAY:
                    case NVS_DataTypes.DATA_TYPE_UINT8:
                    case NVS_DataTypes.DATA_TYPE_UINT8_ARRAY:
                        if(item.elements > 1)
                        {
                            byte[] byteArray = new byte[item.elements];
                            Array.Copy(nvlist, offset, byteArray, 0, item.elements);
                            offset += (int)item.elements;
                            if(item.elements % 4 > 0)
                                offset += 4 - (int)(item.elements % 4);
                            item.value = byteArray;
                        }
                        else
                        {
                            item.value = nvlist[offset];
                            offset += 4;
                        }
                        break;
                    case NVS_DataTypes.DATA_TYPE_DOUBLE:
                        if(item.elements > 1)
                        {
                            double[] doubleArray = new double[item.elements];
                            for(int i = 0; i < item.elements; i++)
                            {
                                double temp = BigEndianBitConverter.ToDouble(nvlist, offset);
                                doubleArray[i] = temp;
                                offset += 8;
                            }
                            item.value = doubleArray;
                        }
                        else
                        {
                            item.value = BigEndianBitConverter.ToDouble(nvlist, offset);
                            offset += 8;
                        }
                        break;
                    case NVS_DataTypes.DATA_TYPE_HRTIME:
                        if(item.elements > 1)
                        {
                            DateTime[] hrtimeArray = new DateTime[item.elements];
                            for(int i = 0; i < item.elements; i++)
                            {
                                DateTime temp = DateHandlers.UNIXHrTimeToDateTime(BigEndianBitConverter.ToUInt64(nvlist, offset));
                                hrtimeArray[i] = temp;
                                offset += 8;
                            }
                            item.value = hrtimeArray;
                        }
                        else
                        {
                            item.value = DateHandlers.UNIXHrTimeToDateTime(BigEndianBitConverter.ToUInt64(nvlist, offset));
                            offset += 8;
                        }
                        break;
                    case NVS_DataTypes.DATA_TYPE_INT16:
                    case NVS_DataTypes.DATA_TYPE_INT16_ARRAY:
                        if(item.elements > 1)
                        {
                            short[] shortArray = new short[item.elements];
                            for(int i = 0; i < item.elements; i++)
                            {
                                short temp = BigEndianBitConverter.ToInt16(nvlist, offset);
                                shortArray[i] = temp;
                                offset += 4;
                            }
                            item.value = shortArray;
                        }
                        else
                        {
                            item.value = BigEndianBitConverter.ToInt16(nvlist, offset);
                            offset += 4;
                        }
                        break;
                    case NVS_DataTypes.DATA_TYPE_INT32:
                    case NVS_DataTypes.DATA_TYPE_INT32_ARRAY:
                        if(item.elements > 1)
                        {
                            int[] intArray = new int[item.elements];
                            for(int i = 0; i < item.elements; i++)
                            {
                                int temp = BigEndianBitConverter.ToInt32(nvlist, offset);
                                intArray[i] = temp;
                                offset += 4;
                            }
                            item.value = intArray;
                        }
                        else
                        {
                            item.value = BigEndianBitConverter.ToInt32(nvlist, offset);
                            offset += 4;
                        }
                        break;
                    case NVS_DataTypes.DATA_TYPE_INT64:
                    case NVS_DataTypes.DATA_TYPE_INT64_ARRAY:
                        if(item.elements > 1)
                        {
                            long[] longArray = new long[item.elements];
                            for(int i = 0; i < item.elements; i++)
                            {
                                long temp = BigEndianBitConverter.ToInt64(nvlist, offset);
                                longArray[i] = temp;
                                offset += 8;
                            }
                            item.value = longArray;
                        }
                        else
                        {
                            item.value = BigEndianBitConverter.ToInt64(nvlist, offset);
                            offset += 8;
                        }
                        break;
                    case NVS_DataTypes.DATA_TYPE_INT8:
                    case NVS_DataTypes.DATA_TYPE_INT8_ARRAY:
                        if(item.elements > 1)
                        {
                            sbyte[] sbyteArray = new sbyte[item.elements];
                            for(int i = 0; i < item.elements; i++)
                            {
                                sbyte temp = (sbyte)nvlist[offset];
                                sbyteArray[i] = temp;
                                offset++;
                            }
                            item.value = sbyteArray;
                            if(sbyteArray.Length % 4 > 0)
                                offset += 4 - sbyteArray.Length % 4;
                        }
                        else
                        {
                            item.value = BigEndianBitConverter.ToInt64(nvlist, offset);
                            offset += 4;
                        }
                        break;
                    case NVS_DataTypes.DATA_TYPE_STRING:
                    case NVS_DataTypes.DATA_TYPE_STRING_ARRAY:
                        if(item.elements > 1)
                        {
                            string[] stringArray = new string[item.elements];
                            for(int i = 0; i < item.elements; i++)
                            {
                                uint strLength = BigEndianBitConverter.ToUInt32(nvlist, offset);
                                offset += 4;
                                byte[] strBytes = new byte[strLength];
                                Array.Copy(nvlist, offset, strBytes, 0, strLength);
                                stringArray[i] = StringHandlers.CToString(strBytes);
                                offset += (int)strLength;
                                if(strLength % 4 > 0)
                                    offset += 4 - (int)(strLength % 4);
                            }
                            item.value = stringArray;
                        }
                        else
                        {
                            uint strLength = BigEndianBitConverter.ToUInt32(nvlist, offset);
                            offset += 4;
                            byte[] strBytes = new byte[strLength];
                            Array.Copy(nvlist, offset, strBytes, 0, strLength);
                            item.value = StringHandlers.CToString(strBytes);
                            offset += (int)strLength;
                            if(strLength % 4 > 0)
                                offset += 4 - (int)(strLength % 4);
                        }
                        break;
                    case NVS_DataTypes.DATA_TYPE_UINT16:
                    case NVS_DataTypes.DATA_TYPE_UINT16_ARRAY:
                        if(item.elements > 1)
                        {
                            ushort[] ushortArray = new ushort[item.elements];
                            for(int i = 0; i < item.elements; i++)
                            {
                                ushort temp = BigEndianBitConverter.ToUInt16(nvlist, offset);
                                ushortArray[i] = temp;
                                offset += 4;
                            }
                            item.value = ushortArray;
                        }
                        else
                        {
                            item.value = BigEndianBitConverter.ToUInt16(nvlist, offset);
                            offset += 4;
                        }
                        break;
                    case NVS_DataTypes.DATA_TYPE_UINT32:
                    case NVS_DataTypes.DATA_TYPE_UINT32_ARRAY:
                        if(item.elements > 1)
                        {
                            uint[] uintArray = new uint[item.elements];
                            for(int i = 0; i < item.elements; i++)
                            {
                                uint temp = BigEndianBitConverter.ToUInt32(nvlist, offset);
                                uintArray[i] = temp;
                                offset += 4;
                            }
                            item.value = uintArray;
                        }
                        else
                        {
                            item.value = BigEndianBitConverter.ToUInt32(nvlist, offset);
                            offset += 4;
                        }
                        break;
                    case NVS_DataTypes.DATA_TYPE_UINT64:
                    case NVS_DataTypes.DATA_TYPE_UINT64_ARRAY:
                        if(item.elements > 1)
                        {
                            ulong[] ulongArray = new ulong[item.elements];
                            for(int i = 0; i < item.elements; i++)
                            {
                                ulong temp = BigEndianBitConverter.ToUInt64(nvlist, offset);
                                ulongArray[i] = temp;
                                offset += 8;
                            }
                            item.value = ulongArray;
                        }
                        else
                        {
                            item.value = BigEndianBitConverter.ToUInt64(nvlist, offset);
                            offset += 8;
                        }
                        break;
                    case NVS_DataTypes.DATA_TYPE_NVLIST:
                        if(item.elements > 1)
                            goto default;

                        byte[] subListBytes = new byte[item.encodedSize - (offset - currOff)];
                        Array.Copy(nvlist, offset, subListBytes, 0, subListBytes.Length);
                        Dictionary<string, NVS_Item> subList;
                        if(DecodeNvList(subListBytes, out subList, xdr, littleEndian))
                            item.value = subList;
                        else
                            goto default;
                        offset = (int)(currOff + item.encodedSize);
                        break;
                    default:
                        byte[] unknown = new byte[item.encodedSize - (offset - currOff)];
                        Array.Copy(nvlist, offset, unknown, 0, unknown.Length);
                        item.value = unknown;
                        offset = (int)(currOff + item.encodedSize);
                        break;
                }

                decodedNvList.Add(item.name, item);
            }

            return decodedNvList.Count > 0;
        }

        static string PrintNvList(Dictionary<string, NVS_Item> decodedNvList)
        {
            StringBuilder sb = new StringBuilder();
            foreach(NVS_Item item in decodedNvList.Values)
            {
                if(item.elements == 0)
                {
                    sb.AppendFormat("{0} is not set", item.name).AppendLine();
                    continue;
                }

                switch(item.dataType)
                {
                    case NVS_DataTypes.DATA_TYPE_BOOLEAN:
                    case NVS_DataTypes.DATA_TYPE_BOOLEAN_ARRAY:
                    case NVS_DataTypes.DATA_TYPE_BOOLEAN_VALUE:
                        if(item.elements > 1)
                        {
                            for(int i = 0; i < item.elements; i++)
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
                            for(int i = 0; i < item.elements; i++)
                                sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((byte[])item.value)[i]).AppendLine();
                        }
                        else
                            sb.AppendFormat("{0} = {1}", item.name, (byte)item.value).AppendLine();
                        break;
                    case NVS_DataTypes.DATA_TYPE_DOUBLE:
                        if(item.elements > 1)
                        {
                            for(int i = 0; i < item.elements; i++)
                                sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((double[])item.value)[i]).AppendLine();
                        }
                        else
                            sb.AppendFormat("{0} = {1}", item.name, (double)item.value).AppendLine();
                        break;
                    case NVS_DataTypes.DATA_TYPE_HRTIME:
                        if(item.elements > 1)
                        {
                            for(int i = 0; i < item.elements; i++)
                                sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((DateTime[])item.value)[i]).AppendLine();
                        }
                        else
                            sb.AppendFormat("{0} = {1}", item.name, (DateTime)item.value).AppendLine();
                        break;
                    case NVS_DataTypes.DATA_TYPE_INT16:
                    case NVS_DataTypes.DATA_TYPE_INT16_ARRAY:
                        if(item.elements > 1)
                        {
                            for(int i = 0; i < item.elements; i++)
                                sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((short[])item.value)[i]).AppendLine();
                        }
                        else
                            sb.AppendFormat("{0} = {1}", item.name, (short)item.value).AppendLine();
                        break;
                    case NVS_DataTypes.DATA_TYPE_INT32:
                    case NVS_DataTypes.DATA_TYPE_INT32_ARRAY:
                        if(item.elements > 1)
                        {
                            for(int i = 0; i < item.elements; i++)
                                sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((int[])item.value)[i]).AppendLine();
                        }
                        else
                            sb.AppendFormat("{0} = {1}", item.name, (int)item.value).AppendLine();
                        break;
                    case NVS_DataTypes.DATA_TYPE_INT64:
                    case NVS_DataTypes.DATA_TYPE_INT64_ARRAY:
                        if(item.elements > 1)
                        {
                            for(int i = 0; i < item.elements; i++)
                                sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((long[])item.value)[i]).AppendLine();
                        }
                        else
                            sb.AppendFormat("{0} = {1}", item.name, (long)item.value).AppendLine();
                        break;
                    case NVS_DataTypes.DATA_TYPE_INT8:
                    case NVS_DataTypes.DATA_TYPE_INT8_ARRAY:
                        if(item.elements > 1)
                        {
                            for(int i = 0; i < item.elements; i++)
                                sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((sbyte[])item.value)[i]).AppendLine();
                        }
                        else
                            sb.AppendFormat("{0} = {1}", item.name, (sbyte)item.value).AppendLine();
                        break;
                    case NVS_DataTypes.DATA_TYPE_STRING:
                    case NVS_DataTypes.DATA_TYPE_STRING_ARRAY:
                        if(item.elements > 1)
                        {
                            for(int i = 0; i < item.elements; i++)
                                sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((string[])item.value)[i]).AppendLine();
                        }
                        else
                            sb.AppendFormat("{0} = {1}", item.name, (string)item.value).AppendLine();
                        break;
                    case NVS_DataTypes.DATA_TYPE_UINT16:
                    case NVS_DataTypes.DATA_TYPE_UINT16_ARRAY:
                        if(item.elements > 1)
                        {
                            for(int i = 0; i < item.elements; i++)
                                sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((ushort[])item.value)[i]).AppendLine();
                        }
                        else
                            sb.AppendFormat("{0} = {1}", item.name, (ushort)item.value).AppendLine();
                        break;
                    case NVS_DataTypes.DATA_TYPE_UINT32:
                    case NVS_DataTypes.DATA_TYPE_UINT32_ARRAY:
                        if(item.elements > 1)
                        {
                            for(int i = 0; i < item.elements; i++)
                                sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((uint[])item.value)[i]).AppendLine();
                        }
                        else
                            sb.AppendFormat("{0} = {1}", item.name, (uint)item.value).AppendLine();
                        break;
                    case NVS_DataTypes.DATA_TYPE_UINT64:
                    case NVS_DataTypes.DATA_TYPE_UINT64_ARRAY:
                        if(item.elements > 1)
                        {
                            for(int i = 0; i < item.elements; i++)
                                sb.AppendFormat("{0}[{1}] = {2}", item.name, i, ((ulong[])item.value)[i]).AppendLine();
                        }
                        else
                            sb.AppendFormat("{0} = {1}", item.name, (ulong)item.value).AppendLine();
                        break;
                    case NVS_DataTypes.DATA_TYPE_NVLIST:
                        if(item.elements == 1)
                            sb.AppendFormat("{0} =\n{1}", item.name, PrintNvList((Dictionary<string, NVS_Item>)item.value)).AppendLine();
                        else
                            sb.AppendFormat("{0} = {1} elements nvlist[], unable to print", item.name, item.elements).AppendLine();
                        break;
                    default:
                        if(item.elements > 1)
                        {
                            for(int i = 0; i < item.elements; i++)
                                sb.AppendFormat("{0}[{1}] = Unknown data type {2}", item.name, i, item.dataType).AppendLine();
                        }
                        else
                            sb.AppendFormat("{0} = Unknown data type {1}", item.name, item.dataType).AppendLine();
                        break;
                }
            }

            return sb.ToString();
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}