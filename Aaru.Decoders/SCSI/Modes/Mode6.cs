// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Modes.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes and encodines SCSI modes in MODE SENSE/SELECT (6) format.
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
using System.Linq;
using Aaru.CommonTypes.Structs.Devices.SCSI;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
    public static ModeHeader? DecodeModeHeader6(byte[] modeResponse, PeripheralDeviceTypes deviceType)
    {
        if(modeResponse == null || modeResponse.Length < 4 || modeResponse.Length < modeResponse[0] + 1)
            return null;

        var header = new ModeHeader
        {
            MediumType = (MediumTypes)modeResponse[1]
        };

        if(modeResponse[3] > 0)
        {
            // An incorrect size field, we cannot know if the following bytes are really the pages (probably not),
            // so consider the MODE SENSE(6) response as invalid
            if(modeResponse[3] + 4 > modeResponse.Length)
                return null;

            header.BlockDescriptors = new BlockDescriptor[modeResponse[3] / 8];

            for(var i = 0; i < header.BlockDescriptors.Length; i++)
            {
                header.BlockDescriptors[i].Density     =  (DensityType)modeResponse[0 + i * 8 + 4];
                header.BlockDescriptors[i].Blocks      += (ulong)(modeResponse[1 + i * 8 + 4] << 16);
                header.BlockDescriptors[i].Blocks      += (ulong)(modeResponse[2 + i * 8 + 4] << 8);
                header.BlockDescriptors[i].Blocks      += modeResponse[3 + i * 8 + 4];
                header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[5 + i * 8 + 4] << 16);
                header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[6 + i * 8 + 4] << 8);
                header.BlockDescriptors[i].BlockLength += modeResponse[7 + i * 8 + 4];
            }
        }

        switch(deviceType)
        {
            case PeripheralDeviceTypes.DirectAccess:
            case PeripheralDeviceTypes.MultiMediaDevice:
                header.WriteProtected = (modeResponse[2] & 0x80) == 0x80;
                header.DPOFUA         = (modeResponse[2] & 0x10) == 0x10;

                break;
            case PeripheralDeviceTypes.SequentialAccess:
                header.WriteProtected = (modeResponse[2]       & 0x80) == 0x80;
                header.Speed          = (byte)(modeResponse[2] & 0x0F);
                header.BufferedMode   = (byte)((modeResponse[2] & 0x70) >> 4);

                break;
            case PeripheralDeviceTypes.PrinterDevice:
                header.BufferedMode = (byte)((modeResponse[2] & 0x70) >> 4);

                break;
            case PeripheralDeviceTypes.OpticalDevice:
                header.WriteProtected = (modeResponse[2] & 0x80) == 0x80;
                header.EBC            = (modeResponse[2] & 0x01) == 0x01;
                header.DPOFUA         = (modeResponse[2] & 0x10) == 0x10;

                break;
        }

        return header;
    }

    public static string PrettifyModeHeader6(byte[] modeResponse, PeripheralDeviceTypes deviceType) =>
        PrettifyModeHeader(DecodeModeHeader6(modeResponse, deviceType), deviceType);

    public static DecodedMode? DecodeMode6(byte[] modeResponse, PeripheralDeviceTypes deviceType)
    {
        ModeHeader? hdr = DecodeModeHeader6(modeResponse, deviceType);

        if(!hdr.HasValue)
            return null;

        var decoded = new DecodedMode
        {
            Header = hdr.Value
        };

        var blkDrLength = 0;

        if(decoded.Header.BlockDescriptors != null)
            blkDrLength = decoded.Header.BlockDescriptors.Length;

        int offset = 4               + blkDrLength * 8;
        int length = modeResponse[0] + 1;

        if(length != modeResponse.Length)
            return decoded;

        List<ModePage> listpages = new();

        while(offset < modeResponse.Length)
        {
            bool isSubpage = (modeResponse[offset] & 0x40) == 0x40;
            var  pg        = new ModePage();
            var  pageNo    = (byte)(modeResponse[offset] & 0x3F);

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
                if(isSubpage)
                {
                    if(offset + 3 >= modeResponse.Length)
                        break;

                    pg.PageResponse = new byte[(modeResponse[offset + 2] << 8) + modeResponse[offset + 3] + 4];
                    int copyLen = pg.PageResponse.Length;

                    if(pg.PageResponse.Length + offset > modeResponse.Length)
                        copyLen = modeResponse.Length - offset;

                    Array.Copy(modeResponse, offset, pg.PageResponse, 0, copyLen);
                    pg.Page    =  (byte)(modeResponse[offset] & 0x3F);
                    pg.Subpage =  modeResponse[offset + 1];
                    offset     += pg.PageResponse.Length;
                }
                else
                {
                    if(offset + 1 >= modeResponse.Length)
                        break;

                    pg.PageResponse = new byte[modeResponse[offset + 1] + 2];
                    int copyLen = pg.PageResponse.Length;

                    if(pg.PageResponse.Length + offset > modeResponse.Length)
                        copyLen = modeResponse.Length - offset;

                    Array.Copy(modeResponse, offset, pg.PageResponse, 0, copyLen);
                    pg.Page    =  (byte)(modeResponse[offset] & 0x3F);
                    pg.Subpage =  0;
                    offset     += pg.PageResponse.Length;
                }
            }

            listpages.Add(pg);
        }

        decoded.Pages = listpages.ToArray();

        return decoded;
    }

    public static byte[] EncodeModeHeader6(ModeHeader header, PeripheralDeviceTypes deviceType)
    {
        byte[] hdr = header.BlockDescriptors != null ? new byte[4 + header.BlockDescriptors.Length * 8] : new byte[4];

        hdr[1] = (byte)header.MediumType;

        switch(deviceType)
        {
            case PeripheralDeviceTypes.DirectAccess:
            case PeripheralDeviceTypes.MultiMediaDevice:
                if(header.WriteProtected)
                    hdr[2] += 0x80;

                if(header.DPOFUA)
                    hdr[2] += 0x10;

                break;
            case PeripheralDeviceTypes.SequentialAccess:
                if(header.WriteProtected)
                    hdr[2] += 0x80;

                hdr[2] += (byte)(header.Speed             & 0x0F);
                hdr[2] += (byte)(header.BufferedMode << 4 & 0x70);

                break;
            case PeripheralDeviceTypes.PrinterDevice:
                hdr[2] += (byte)(header.BufferedMode << 4 & 0x70);

                break;
            case PeripheralDeviceTypes.OpticalDevice:
                if(header.WriteProtected)
                    hdr[2] += 0x80;

                if(header.EBC)
                    hdr[2] += 0x01;

                if(header.DPOFUA)
                    hdr[2] += 0x10;

                break;
        }

        if(header.BlockDescriptors == null)
            return hdr;

        hdr[3] = (byte)(header.BlockDescriptors.Length * 8);

        for(var i = 0; i < header.BlockDescriptors.Length; i++)
        {
            hdr[0 + i * 8 + 4] = (byte)header.BlockDescriptors[i].Density;
            hdr[1 + i * 8 + 4] = (byte)((header.BlockDescriptors[i].Blocks & 0xFF0000) >> 16);
            hdr[2 + i * 8 + 4] = (byte)((header.BlockDescriptors[i].Blocks & 0xFF00)   >> 8);
            hdr[3 + i * 8 + 4] = (byte)(header.BlockDescriptors[i].Blocks & 0xFF);
            hdr[5 + i * 8 + 4] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF0000) >> 16);
            hdr[6 + i * 8 + 4] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF00)   >> 8);
            hdr[7 + i * 8 + 4] = (byte)(header.BlockDescriptors[i].BlockLength & 0xFF);
        }

        return hdr;
    }

    public static byte[] EncodeMode6(DecodedMode mode, PeripheralDeviceTypes deviceType)
    {
        var modeSize = 0;

        if(mode.Pages != null)
            modeSize += mode.Pages.Sum(page => page.PageResponse.Length);

        byte[] hdr = EncodeModeHeader6(mode.Header, deviceType);
        modeSize += hdr.Length;
        var md = new byte[modeSize];

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