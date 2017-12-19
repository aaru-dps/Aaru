// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ModesEncoders.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Encodes SCSI modes.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;

namespace DiscImageChef.Decoders.SCSI
{
    public static partial class Modes
    {
        public static byte[] EncodeModeHeader6(ModeHeader header, PeripheralDeviceTypes deviceType)
        {
            byte[] hdr;

            if(header.BlockDescriptors != null)
                hdr = new byte[4 + header.BlockDescriptors.Length * 8];
            else
                hdr = new byte[4];

            hdr[1] = (byte)header.MediumType;

            if(deviceType == PeripheralDeviceTypes.DirectAccess || deviceType == PeripheralDeviceTypes.MultiMediaDevice)
            {
                if(header.WriteProtected)
                    hdr[2] += 0x80;
                if(header.DPOFUA)
                    hdr[2] += 0x10;
            }

            if(deviceType == PeripheralDeviceTypes.SequentialAccess)
            {
                if(header.WriteProtected)
                    hdr[2] += 0x80;
                hdr[2] += (byte)(header.Speed & 0x0F);
                hdr[2] += (byte)((header.BufferedMode << 4) & 0x70);
            }

            if(deviceType == PeripheralDeviceTypes.PrinterDevice)
                hdr[2] += (byte)((header.BufferedMode << 4) & 0x70);

            if(deviceType == PeripheralDeviceTypes.OpticalDevice)
            {
                if(header.WriteProtected)
                    hdr[2] += 0x80;
                if(header.EBC)
                    hdr[2] += 0x01;
                if(header.DPOFUA)
                    hdr[2] += 0x10;
            }

            if(header.BlockDescriptors != null)
            {
                hdr[3] = (byte)(header.BlockDescriptors.Length * 8);

                for(int i = 0; i < header.BlockDescriptors.Length; i++)
                {
                    hdr[0 + i * 8 + 4] = (byte)header.BlockDescriptors[i].Density;
                    hdr[1 + i * 8 + 4] = (byte)((header.BlockDescriptors[i].Blocks & 0xFF0000) >> 16);
                    hdr[2 + i * 8 + 4] = (byte)((header.BlockDescriptors[i].Blocks & 0xFF00) >> 8);
                    hdr[3 + i * 8 + 4] = (byte)(header.BlockDescriptors[i].Blocks & 0xFF);
                    hdr[5 + i * 8 + 4] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF0000) >> 16);
                    hdr[6 + i * 8 + 4] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF00) >> 8);
                    hdr[7 + i * 8 + 4] = (byte)(header.BlockDescriptors[i].BlockLength & 0xFF);
                }
            }

            return hdr;
        }

        public static byte[] EncodeMode6(DecodedMode mode, PeripheralDeviceTypes deviceType)
        {
            int modeSize = 0;
            if(mode.Pages != null)
            {
                foreach(ModePage page in mode.Pages)
                    modeSize += page.PageResponse.Length;
            }

            byte[] hdr = EncodeModeHeader6(mode.Header, deviceType);
            modeSize += hdr.Length;
            byte[] md = new byte[modeSize];

            Array.Copy(hdr, 0, md, 0, hdr.Length);

            if(mode.Pages != null)
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


        public static byte[] EncodeMode10(DecodedMode mode, PeripheralDeviceTypes deviceType)
        {
            int modeSize = 0;
            if(mode.Pages != null)
            {
                foreach(ModePage page in mode.Pages)
                    modeSize += page.PageResponse.Length;
            }

            byte[] hdr = EncodeModeHeader10(mode.Header, deviceType);
            modeSize += hdr.Length;
            byte[] md = new byte[modeSize];

            Array.Copy(hdr, 0, md, 0, hdr.Length);

            if(mode.Pages != null)
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

        public static byte[] EncodeModeHeader10(ModeHeader header, PeripheralDeviceTypes deviceType)
        {
            return EncodeModeHeader10(header, deviceType, false);
        }

        public static byte[] EncodeModeHeader10(ModeHeader header, PeripheralDeviceTypes deviceType, bool longLBA)
        {
            byte[] hdr;

            if(header.BlockDescriptors != null)
            {
                if(longLBA)
                    hdr = new byte[8 + header.BlockDescriptors.Length * 16];
                else
                    hdr = new byte[8 + header.BlockDescriptors.Length * 8];
            }
            else
                hdr = new byte[8];

            hdr[2] = (byte)header.MediumType;

            if(deviceType == PeripheralDeviceTypes.DirectAccess || deviceType == PeripheralDeviceTypes.MultiMediaDevice)
            {
                if(header.WriteProtected)
                    hdr[3] += 0x80;
                if(header.DPOFUA)
                    hdr[3] += 0x10;
            }

            if(deviceType == PeripheralDeviceTypes.SequentialAccess)
            {
                if(header.WriteProtected)
                    hdr[3] += 0x80;
                hdr[3] += (byte)(header.Speed & 0x0F);
                hdr[3] += (byte)((header.BufferedMode << 4) & 0x70);
            }

            if(deviceType == PeripheralDeviceTypes.PrinterDevice)
                hdr[3] += (byte)((header.BufferedMode << 4) & 0x70);

            if(deviceType == PeripheralDeviceTypes.OpticalDevice)
            {
                if(header.WriteProtected)
                    hdr[3] += 0x80;
                if(header.EBC)
                    hdr[3] += 0x01;
                if(header.DPOFUA)
                    hdr[3] += 0x10;
            }

            if(longLBA)
                hdr[4] += 0x01;

            if(header.BlockDescriptors != null)
            {
                if(longLBA)
                {
                    for(int i = 0; i < header.BlockDescriptors.Length; i++)
                    {
                        byte[] temp = BitConverter.GetBytes(header.BlockDescriptors[i].Blocks);
                        hdr[7 + i * 16 + 8] = temp[0];
                        hdr[6 + i * 16 + 8] = temp[1];
                        hdr[5 + i * 16 + 8] = temp[2];
                        hdr[4 + i * 16 + 8] = temp[3];
                        hdr[3 + i * 16 + 8] = temp[4];
                        hdr[2 + i * 16 + 8] = temp[5];
                        hdr[1 + i * 16 + 8] = temp[6];
                        hdr[0 + i * 16 + 8] = temp[7];
                        hdr[12 + i * 16 + 8] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF000000) >> 24);
                        hdr[13 + i * 16 + 8] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF0000) >> 16);
                        hdr[14 + i * 16 + 8] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF00) >> 8);
                        hdr[15 + i * 16 + 8] = (byte)(header.BlockDescriptors[i].BlockLength & 0xFF);
                    }
                }
                else
                {
                    for(int i = 0; i < header.BlockDescriptors.Length; i++)
                    {
                        if(deviceType != PeripheralDeviceTypes.DirectAccess)
                            hdr[0 + i * 8 + 8] = (byte)header.BlockDescriptors[i].Density;
                        else
                            hdr[0 + i * 8 + 8] = (byte)((header.BlockDescriptors[i].Blocks & 0xFF000000) >> 24);
                        hdr[1 + i * 8 + 8] = (byte)((header.BlockDescriptors[i].Blocks & 0xFF0000) >> 16);
                        hdr[2 + i * 8 + 8] = (byte)((header.BlockDescriptors[i].Blocks & 0xFF00) >> 8);
                        hdr[3 + i * 8 + 8] = (byte)(header.BlockDescriptors[i].Blocks & 0xFF);
                        hdr[5 + i * 8 + 8] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF0000) >> 16);
                        hdr[6 + i * 8 + 8] = (byte)((header.BlockDescriptors[i].BlockLength & 0xFF00) >> 8);
                        hdr[7 + i * 8 + 8] = (byte)(header.BlockDescriptors[i].BlockLength & 0xFF);
                    }
                }
            }

            return hdr;
        }

        public static byte[] EncodeModePage_01(ModePage_01 page)
        {
            byte[] pg = new byte[8];

            pg[0] = 0x01;
            pg[1] = 6;

            if(page.PS)
                pg[0] += 0x80;
            if(page.AWRE)
                pg[2] += 0x80;
            if(page.ARRE)
                pg[2] += 0x40;
            if(page.TB)
                pg[2] += 0x20;
            if(page.RC)
                pg[2] += 0x10;
            if(page.EER)
                pg[2] += 0x08;
            if(page.PER)
                pg[2] += 0x04;
            if(page.DTE)
                pg[2] += 0x02;
            if(page.DCR)
                pg[2] += 0x01;

            pg[3] = page.ReadRetryCount;
            pg[4] = page.CorrectionSpan;
            pg[5] = (byte)page.HeadOffsetCount;
            pg[6] = (byte)page.DataStrobeOffsetCount;

            // This is from a newer version of SCSI unknown what happen for drives expecting an 8 byte page
            /*
            pg[8] = page.WriteRetryCount;
            if (page.LBPERE)
                pg[7] += 0x80;
            pg[10] = (byte)((page.RecoveryTimeLimit & 0xFF00) << 8);
            pg[11] = (byte)(page.RecoveryTimeLimit & 0xFF);*/

            return pg;
        }

        public static byte[] EncodeModePage_01_MMC(ModePage_01_MMC page)
        {
            byte[] pg = new byte[12];

            pg[0] = 0x01;
            pg[1] = 10;

            if(page.PS)
                pg[0] += 0x80;
            pg[2] = page.Parameter;
            pg[3] = page.ReadRetryCount;

            // This is from a newer version of SCSI unknown what happen for drives expecting an 8 byte page

            pg[8] = page.WriteRetryCount;
            pg[10] = (byte)((page.RecoveryTimeLimit & 0xFF00) << 8);
            pg[11] = (byte)(page.RecoveryTimeLimit & 0xFF);

            return pg;
        }
    }
}

