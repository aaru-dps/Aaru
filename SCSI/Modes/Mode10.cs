// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mode10.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes and encodines SCSI modes in MODE SENSE/SELECT (10) format.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Aaru.CommonTypes.Structs.Devices.SCSI;

namespace Aaru.Decoders.SCSI
{
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static partial class Modes
    {
        public static ModeHeader? DecodeModeHeader10(byte[] modeResponse, PeripheralDeviceTypes deviceType)
        {
            if(modeResponse        == null ||
               modeResponse.Length < 8)
                return null;

            ushort modeLength      = (ushort)((modeResponse[0] << 8) + modeResponse[1]);
            ushort blockDescLength = (ushort)((modeResponse[6] << 8) + modeResponse[7]);

            if(modeResponse.Length < modeLength)
                return null;

            var header = new ModeHeader
            {
                MediumType = (MediumTypes)modeResponse[2]
            };

            bool longLBA = (modeResponse[4] & 0x01) == 0x01;

            if(blockDescLength > 0)
                if(longLBA)
                {
                    header.BlockDescriptors = new BlockDescriptor[blockDescLength / 16];

                    for(int i = 0; i < header.BlockDescriptors.Length; i++)
                    {
                        if(12 + (i * 16) + 8 >= modeResponse.Length)
                            break;

                        header.BlockDescriptors[i] = new BlockDescriptor
                        {
                            Density = DensityType.Default
                        };

                        byte[] temp = new byte[8];
                        temp[0]                                =  modeResponse[7 + (i * 16) + 8];
                        temp[1]                                =  modeResponse[6 + (i * 16) + 8];
                        temp[2]                                =  modeResponse[5 + (i * 16) + 8];
                        temp[3]                                =  modeResponse[4 + (i * 16) + 8];
                        temp[4]                                =  modeResponse[3 + (i * 16) + 8];
                        temp[5]                                =  modeResponse[2 + (i * 16) + 8];
                        temp[6]                                =  modeResponse[1 + (i * 16) + 8];
                        temp[7]                                =  modeResponse[0 + (i * 16) + 8];
                        header.BlockDescriptors[i].Blocks      =  BitConverter.ToUInt64(temp, 0);
                        header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[15 + (i * 16) + 8] << 24);
                        header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[14 + (i * 16) + 8] << 16);
                        header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[13 + (i * 16) + 8] << 8);
                        header.BlockDescriptors[i].BlockLength += modeResponse[12 + (i * 16) + 8];
                    }
                }
                else
                {
                    header.BlockDescriptors = new BlockDescriptor[blockDescLength / 8];

                    for(int i = 0; i < header.BlockDescriptors.Length; i++)
                    {
                        if(7 + (i * 8) + 8 >= modeResponse.Length)
                            break;

                        header.BlockDescriptors[i] = new BlockDescriptor();

                        if(deviceType != PeripheralDeviceTypes.DirectAccess)
                            header.BlockDescriptors[i].Density = (DensityType)modeResponse[0 + (i * 8) + 8];
                        else
                        {
                            header.BlockDescriptors[i].Density =  DensityType.Default;
                            header.BlockDescriptors[i].Blocks  += (ulong)(modeResponse[0 + (i * 8) + 8] << 24);
                        }

                        header.BlockDescriptors[i].Blocks      += (ulong)(modeResponse[1 + (i * 8) + 8] << 16);
                        header.BlockDescriptors[i].Blocks      += (ulong)(modeResponse[2 + (i * 8) + 8] << 8);
                        header.BlockDescriptors[i].Blocks      += modeResponse[3 + (i * 8) + 8];
                        header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[5 + (i * 8) + 8] << 16);
                        header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[6 + (i * 8) + 8] << 8);
                        header.BlockDescriptors[i].BlockLength += modeResponse[7 + (i * 8) + 8];
                    }
                }

            switch(deviceType)
            {
                case PeripheralDeviceTypes.DirectAccess:
                case PeripheralDeviceTypes.MultiMediaDevice:
                    header.WriteProtected = (modeResponse[3] & 0x80) == 0x80;
                    header.DPOFUA         = (modeResponse[3] & 0x10) == 0x10;

                    break;
                case PeripheralDeviceTypes.SequentialAccess:
                    header.WriteProtected = (modeResponse[3]       & 0x80) == 0x80;
                    header.Speed          = (byte)(modeResponse[3] & 0x0F);
                    header.BufferedMode   = (byte)((modeResponse[3] & 0x70) >> 4);

                    break;
                case PeripheralDeviceTypes.PrinterDevice:
                    header.BufferedMode = (byte)((modeResponse[3] & 0x70) >> 4);

                    break;
                case PeripheralDeviceTypes.OpticalDevice:
                    header.WriteProtected = (modeResponse[3] & 0x80) == 0x80;
                    header.EBC            = (modeResponse[3] & 0x01) == 0x01;
                    header.DPOFUA         = (modeResponse[3] & 0x10) == 0x10;

                    break;
            }

            return header;
        }

        public static string PrettifyModeHeader10(byte[] modeResponse, PeripheralDeviceTypes deviceType) =>
            PrettifyModeHeader(DecodeModeHeader10(modeResponse, deviceType), deviceType);

        public static DecodedMode? DecodeMode10(byte[] modeResponse, PeripheralDeviceTypes deviceType)
        {
            ModeHeader? hdr = DecodeModeHeader10(modeResponse, deviceType);

            if(!hdr.HasValue)
                return null;

            var decoded = new DecodedMode
            {
                Header = hdr.Value
            };

            bool longlba = (modeResponse[4] & 0x01) == 0x01;
            int  offset;
            int  blkDrLength = 0;

            if(decoded.Header.BlockDescriptors != null)
                blkDrLength = decoded.Header.BlockDescriptors.Length;

            if(longlba)
                offset = 8 + (blkDrLength * 16);
            else
                offset = 8 + (blkDrLength * 8);

            int length = modeResponse[0] << 8;
            length += modeResponse[1];
            length += 2;

            if(length != modeResponse.Length)
                return decoded;

            List<ModePage> listpages = new();

            while(offset < modeResponse.Length)
            {
                bool isSubpage = (modeResponse[offset] & 0x40) == 0x40;
                var  pg        = new ModePage();
                byte pageNo    = (byte)(modeResponse[offset] & 0x3F);

                if(pageNo == 0)
                {
                    pg.PageResponse = new byte[modeResponse.Length - offset];
                    Array.Copy(modeResponse, offset, pg.PageResponse, 0, pg.PageResponse.Length);
                    pg.Page    =  0;
                    pg.Subpage =  0;
                    offset     += pg.PageResponse.Length;
                }
                else
                {
                    if(isSubpage && offset + 3 < modeResponse.Length)
                    {
                        pg.PageResponse = new byte[(modeResponse[offset + 2] << 8) + modeResponse[offset + 3] + 4];
                        int copyLen = pg.PageResponse.Length;

                        if(pg.PageResponse.Length + offset > modeResponse.Length)
                            copyLen = modeResponse.Length - offset;

                        Array.Copy(modeResponse, offset, pg.PageResponse, 0, copyLen);
                        pg.Page    =  (byte)(modeResponse[offset] & 0x3F);
                        pg.Subpage =  modeResponse[offset + 1];
                        offset     += pg.PageResponse.Length;
                    }
                    else if(isSubpage && offset + 1 < modeResponse.Length)
                    {
                        pg.PageResponse = new byte[modeResponse[offset + 1] + 2];
                        int copyLen = pg.PageResponse.Length;

                        if(pg.PageResponse.Length + offset > modeResponse.Length)
                            copyLen = modeResponse.Length - offset;

                        Array.Copy(modeResponse, offset, pg.PageResponse, 0, copyLen);
                        pg.Page    =  (byte)(modeResponse[offset] & 0x3F);
                        pg.Subpage =  0;
                        offset     += pg.PageResponse.Length;
                    }
                    else
                        offset = modeResponse.Length;
                }

                listpages.Add(pg);
            }

            decoded.Pages = listpages.ToArray();

            return decoded;
        }

        public static byte[] EncodeModeHeader10(ModeHeader header, PeripheralDeviceTypes deviceType,
                                                bool longLBA = false)
        {
            byte[] hdr;

            if(header.BlockDescriptors != null)
                hdr = longLBA ? new byte[8 + (header.BlockDescriptors.Length * 16)]
                          : new byte[8     + (header.BlockDescriptors.Length * 8)];
            else
                hdr = new byte[8];

            hdr[2] = (byte)header.MediumType;

            switch(deviceType)
            {
                case PeripheralDeviceTypes.DirectAccess:
                case PeripheralDeviceTypes.MultiMediaDevice:
                    if(header.WriteProtected)
                        hdr[3] += 0x80;

                    if(header.DPOFUA)
                        hdr[3] += 0x10;

                    break;
                case PeripheralDeviceTypes.SequentialAccess:
                    if(header.WriteProtected)
                        hdr[3] += 0x80;

                    hdr[3] += (byte)(header.Speed               & 0x0F);
                    hdr[3] += (byte)((header.BufferedMode << 4) & 0x70);

                    break;
                case PeripheralDeviceTypes.PrinterDevice:
                    hdr[3] += (byte)((header.BufferedMode << 4) & 0x70);

                    break;
                case PeripheralDeviceTypes.OpticalDevice:
                    if(header.WriteProtected)
                        hdr[3] += 0x80;

                    if(header.EBC)
                        hdr[3] += 0x01;

                    if(header.DPOFUA)
                        hdr[3] += 0x10;

                    break;
            }

            if(longLBA)
                hdr[4] += 0x01;

            if(header.BlockDescriptors == null)
                return hdr;

            if(longLBA)
                for(int i = 0; i < header.BlockDescriptors.Length; i++)
                {
                    byte[] temp = BitConverter.GetBytes(header.BlockDescriptors[i].Blocks);
                    hdr[7  + (i * 16) + 8] = temp[0];
                    hdr[6  + (i * 16) + 8] = temp[1];
                    hdr[5  + (i * 16) + 8] = temp[2];
                    hdr[4  + (i * 16) + 8] = temp[3];
                    hdr[3  + (i * 16) + 8] = temp[4];
                    hdr[2  + (i * 16) + 8] = temp[5];
                    hdr[1  + (i * 16) + 8] = temp[6];
                    hdr[0  + (i * 16) + 8] = temp[7];
                    hdr[12 + (i * 16) + 8] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF000000) >> 24);
                    hdr[13 + (i * 16) + 8] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF0000)   >> 16);
                    hdr[14 + (i * 16) + 8] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF00)     >> 8);
                    hdr[15 + (i * 16) + 8] = (byte)(header.BlockDescriptors[i].BlockLength & 0xFF);
                }
            else
                for(int i = 0; i < header.BlockDescriptors.Length; i++)
                {
                    if(deviceType != PeripheralDeviceTypes.DirectAccess)
                        hdr[0 + (i * 8) + 8] = (byte)header.BlockDescriptors[i].Density;
                    else
                        hdr[0 + (i * 8) + 8] = (byte)((header.BlockDescriptors[i].Blocks & 0xFF000000) >> 24);

                    hdr[1 + (i * 8) + 8] = (byte)((header.BlockDescriptors[i].Blocks & 0xFF0000) >> 16);
                    hdr[2 + (i * 8) + 8] = (byte)((header.BlockDescriptors[i].Blocks & 0xFF00)   >> 8);
                    hdr[3 + (i * 8) + 8] = (byte)(header.BlockDescriptors[i].Blocks & 0xFF);
                    hdr[5 + (i * 8) + 8] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF0000) >> 16);
                    hdr[6 + (i * 8) + 8] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF00)   >> 8);
                    hdr[7 + (i * 8) + 8] = (byte)(header.BlockDescriptors[i].BlockLength & 0xFF);
                }

            return hdr;
        }

        public static byte[] EncodeMode10(DecodedMode mode, PeripheralDeviceTypes deviceType)
        {
            int modeSize = 0;

            if(mode.Pages != null)
                modeSize += mode.Pages.Sum(page => page.PageResponse.Length);

            byte[] hdr = EncodeModeHeader10(mode.Header, deviceType);
            modeSize += hdr.Length;
            byte[] md = new byte[modeSize];

            Array.Copy(hdr, 0, md, 0, hdr.Length);

            if(mode.Pages == null)
                return md;

            {
                int offset = hdr.Length;

                foreach(ModePage page in mode.Pages)
                {
                    Array.Copy(page.PageResponse, 0, md, offset, page.PageResponse.Length);
                    offset += page.PageResponse.Length;
                }
            }

            return md;
        }
    }
}