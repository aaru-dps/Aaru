// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Modes.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Text;

namespace DiscImageChef.Decoders.SCSI
{
    public static class Modes
    {
        public enum MediumTypes : byte
        {
            Default = 0x00,
            #region Medium Types defined in ECMA-111 for Direct-Access devices
            /// <summary>
            /// ECMA-54: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on One Side
            /// </summary>
            ECMA54 = 0x09,
            /// <summary>
            /// ECMA-59: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on Both Sides
            /// </summary>
            ECMA59 = 0x0A,
            /// <summary>
            /// ECMA-69: 200 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides
            /// </summary>
            ECMA69 = 0x0B,
            /// <summary>
            /// ECMA-66: 130 mm Flexible Disk Cartridge using Two-Frequency Recording at 7958 ftprad on One Side
            /// </summary>
            ECMA66 = 0x0E,
            /// <summary>
            /// ECMA-70: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 1,9 Tracks per mm
            /// </summary>
            ECMA70 = 0x12,
            /// <summary>
            /// ECMA-78: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 3,8 Tracks per mm
            /// </summary>
            ECMA78 = 0x16,
            /// <summary>
            /// ECMA-99: 130 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides; 3,8 Tracks per mm
            /// </summary>
            ECMA99 = 0x1A,
            /// <summary>
            /// ECMA-100: 90 mm Flexible Disk Cartridge using MFM Recording at 7859 ftprad on Both Sides; 5,3 Tracks per mm
            /// </summary>
            ECMA100 = 0x1E
            #endregion Medium Types defined in ECMA-111 for Direct-Access devices
        }

        public enum DensityType : byte
        {
            Default = 0x00,
            #region Density Types defined in ECMA-111 for Direct-Access devices
            /// <summary>
            /// 7958 flux transitions per radian
            /// </summary>
            Flux7958 = 0x01,
            /// <summary>
            /// 13262 flux transitions per radian
            /// </summary>
            Flux13262 = 0x02,
            /// <summary>
            /// 15916 flux transitions per radian
            /// </summary>
            Flux15916 = 0x03,
            #endregion Density Types defined in ECMA-111 for Direct-Access devices

            #region Medium Types defined in ECMA-111 for Sequential-Access devices
            /// <summary>
            /// ECMA-62: 12,7 mm 9-Track Magnetic Tape, 32 ftpmm, NRZ1, 32 cpmm
            /// </summary>
            ECMA62 = 0x01,
            /// <summary>
            /// ECMA-62: 12,7 mm 9-Track Magnetic Tape, 126 ftpmm, Phase Encoding, 63 cpmm
            /// </summary>
            ECMA62_Phase = 0x02,
            /// <summary>
            /// ECMA-62: 12,7 mm 9-Track Magnetic Tape, 356 ftpmm, NRZ1, 245 cpmm GCR
            /// </summary>
            ECMA62_GCR = 0x03,
            /// <summary>
            /// ECMA-79: 6,30 mm Magnetic Tape Cartridge using MFM Recording at 252 ftpmm
            /// </summary>
            ECMA79 = 0x07,
            /// <summary>
            /// Draft ECMA: 12,7 mm Magnetic Tape Cartridge using IFM Recording on 18 Tracks at 1944 ftpmm, GCR
            /// </summary>
            ECMADraft = 0x09,
            /// <summary>
            /// ECMA-46: 6,30 mm Magnetic Tape Cartridge, Phase Encoding, 63 bpmm
            /// </summary>
            ECMA46 = 0x0B,
            /// <summary>
            /// ECMA-98: 6,30 mm Magnetic Tape Cartridge, NRZ1 Recording, 394 ftpmm
            /// </summary>
            ECMA98 = 0x0E
            #endregion Medium Types defined in ECMA-111 for Sequential-Access devices
        }

        public struct BlockDescriptor
        {
            public DensityType Density;
            public ulong Blocks;
            public ulong BlockLength;
        }

        public struct ModeHeader
        {
            public MediumTypes MediumType;
            public bool WriteProtected;
            public BlockDescriptor[] BlockDescriptors;
            public byte Speed;
            public byte BufferedMode;
            public bool EBC;
        }

        public static ModeHeader? DecodeModeHeader6(byte[] modeResponse, PeripheralDeviceTypes deviceType)
        {
            if (modeResponse.Length < modeResponse[0] + 1)
                return null;

            ModeHeader header = new ModeHeader();
            header.MediumType = (MediumTypes)modeResponse[1];

            if (modeResponse[3] > 0)
            {
                header.BlockDescriptors = new BlockDescriptor[modeResponse[3]/8];
                for (int i = 0; i < header.BlockDescriptors.Length; i++)
                {
                    header.BlockDescriptors[i].Density = (DensityType)modeResponse[0 + i * 8 + 4];
                    header.BlockDescriptors[i].Blocks += (ulong)(modeResponse[1 + i * 8 + 4] << 16);
                    header.BlockDescriptors[i].Blocks += (ulong)(modeResponse[2 + i * 8 + 4] << 8);
                    header.BlockDescriptors[i].Blocks += modeResponse[3 + i * 8 + 4];
                    header.BlockDescriptors[i].BlockLength += (ulong)(modeResponse[5 + i * 8 + 4] << 16);
                    header.BlockDescriptors[i].BlockLength += (ulong)(modeResponse[6 + i * 8 + 4] << 8);
                    header.BlockDescriptors[i].BlockLength += modeResponse[7 + i * 8 + 4];
                }
            }

            if (deviceType == PeripheralDeviceTypes.DirectAccess)
                header.WriteProtected = ((modeResponse[2] & 0x80) == 0x80);

            if (deviceType == PeripheralDeviceTypes.SequentialAccess)
            {
                header.WriteProtected = ((modeResponse[2] & 0x80) == 0x80);
                header.Speed = (byte)(modeResponse[2] & 0x0F);
                header.BufferedMode = (byte)((modeResponse[2] & 0x70) >> 4);
            }

            if (deviceType == PeripheralDeviceTypes.PrinterDevice)
                header.BufferedMode = (byte)((modeResponse[2] & 0x70) >> 4);

            if (deviceType == PeripheralDeviceTypes.OpticalDevice)
            {
                header.WriteProtected = ((modeResponse[2] & 0x80) == 0x80);
                header.EBC = ((modeResponse[2] & 0x01) == 0x01);
            }

            return header;
        }

        public static string PrettifyModeHeader6(byte[] modeResponse, PeripheralDeviceTypes deviceType)
        {
            return PrettifyModeHeader(DecodeModeHeader6(modeResponse, deviceType), deviceType);
        }

        public static string PrettifyModeHeader(ModeHeader? header, PeripheralDeviceTypes deviceType)
        {
            if (!header.HasValue)
                return null;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Mode Page 0:");

            switch (deviceType)
            {
                case PeripheralDeviceTypes.DirectAccess:
                    {
                        sb.Append("Medium is ");

                        switch (header.Value.MediumType)
                        {
                            case MediumTypes.ECMA54:
                                sb.AppendLine("ECMA-54: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on One Side");
                                break;
                            case MediumTypes.ECMA59:
                                sb.AppendLine("ECMA-59: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on Both Sides");
                                break;
                            case MediumTypes.ECMA69:
                                sb.AppendLine("ECMA-69: 200 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides");
                                break;
                            case MediumTypes.ECMA66:
                                sb.AppendLine("ECMA-66: 130 mm Flexible Disk Cartridge using Two-Frequency Recording at 7958 ftprad on One Side");
                                break;
                            case MediumTypes.ECMA70:
                                sb.AppendLine("ECMA-70: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 1,9 Tracks per mm");
                                break;
                            case MediumTypes.ECMA78:
                                sb.AppendLine("ECMA-78: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 3,8 Tracks per mm");
                                break;
                            case MediumTypes.ECMA99:
                                sb.AppendLine("ECMA-99: 130 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides; 3,8 Tracks per mm");
                                break;
                            case MediumTypes.ECMA100:
                                sb.AppendLine("ECMA-100: 90 mm Flexible Disk Cartridge using MFM Recording at 7859 ftprad on Both Sides; 5,3 Tracks per mm");
                                break;
                            default:
                                sb.AppendFormat("Unknown medium type 0x{0:X2}", header.Value.MediumType).AppendLine();
                                break;
                        }

                        if (header.Value.WriteProtected)
                            sb.AppendLine("Medium is write protected");
                        
                        foreach (BlockDescriptor descriptor in header.Value.BlockDescriptors)
                        {
                            string density;
                            switch (descriptor.Density)
                            {
                                case DensityType.Flux7958:
                                    density = "7958 flux transitions per radian";
                                    break;
                                case DensityType.Flux13262:
                                    density = "13262 flux transitions per radian";
                                    break;
                                case DensityType.Flux15916:
                                    density = "15916 flux transitions per radian";
                                    break;
                                default:
                                    density = String.Format("with unknown density code 0x{0:X2}", descriptor.Density);
                                    break;
                            }

                            if (descriptor.Blocks == 0)
                                sb.AppendFormat("All remaining blocks have {0} and are {1} bytes each", density, descriptor.BlockLength).AppendLine();
                            else
                                sb.AppendFormat("{0} blocks have {1} and are {2} bytes each", descriptor.Blocks, density, descriptor.BlockLength).AppendLine();
                        }

                        break;
                    }
                case PeripheralDeviceTypes.SequentialAccess:
                    {
                        switch (header.Value.BufferedMode)
                        {
                            case 0:
                                sb.AppendLine("Device writes directly to media");
                                break;
                            case 1:
                                sb.AppendLine("Device uses a write cache");
                                break;
                            default:
                                sb.AppendFormat("Unknown buffered mode code 0x{0:X2}", header.Value.BufferedMode).AppendLine();
                                break;
                        }

                        if (header.Value.Speed == 0)
                            sb.AppendLine("Device uses default speed");
                        else
                            sb.AppendFormat("Device uses speed {0}", header.Value.Speed).AppendLine();

                        if (header.Value.WriteProtected)
                            sb.AppendLine("Medium is write protected");

                        foreach (BlockDescriptor descriptor in header.Value.BlockDescriptors)
                        {
                            string density;
                            switch (descriptor.Density)
                            {
                                case DensityType.ECMA62:
                                    density = "ECMA-62: 12,7 mm 9-Track Magnetic Tape, 32 ftpmm, NRZ1, 32 cpmm";
                                    break;
                                case DensityType.ECMA62_Phase:
                                    density = "ECMA-62: 12,7 mm 9-Track Magnetic Tape, 126 ftpmm, Phase Encoding, 63 cpmm";
                                    break;
                                case DensityType.ECMA62_GCR:
                                    density = "ECMA-62: 12,7 mm 9-Track Magnetic Tape, 356 ftpmm, NRZ1, 245 cpmm GCR";
                                    break;
                                case DensityType.ECMA79:
                                    density = "ECMA-79: 6,30 mm Magnetic Tape Cartridge, 252 ftpmm, MFM";
                                    break;
                                case DensityType.ECMADraft:
                                    density = "Draft ECMA: 12,7 mm 18-Track Magnetic Tape Cartridge, 1944 ftpmm, IFM, GCR";
                                    break;
                                case DensityType.ECMA46:
                                    density = "ECMA-46: 6,30 mm Magnetic Tape Cartridge, Phase Encoding, 63 bpmm";
                                    break;
                                case DensityType.ECMA98:
                                    density = "ECMA-98: 6,30 mm Magnetic Tape Cartridge, NRZ1, 394 ftpmm";
                                    break;
                                default:
                                    density = String.Format("Unknown density code 0x{0:X2}", descriptor.Density);
                                    break;
                            }

                            if (descriptor.Blocks == 0)
                            {
                                if (descriptor.BlockLength == 0)
                                    sb.AppendFormat("All remaining blocks conform to {0} and have a variable length", density).AppendLine();
                                else
                                    sb.AppendFormat("All remaining blocks conform to {0} and are {1} bytes each", density, descriptor.BlockLength).AppendLine();
                            }
                            else
                            {
                                if (descriptor.BlockLength == 0)
                                    sb.AppendFormat("{0} blocks conform to {1} and have a variable length", descriptor.Blocks, density).AppendLine();
                                else
                                    sb.AppendFormat("{0} blocks conform to {1} and are {2} bytes each", descriptor.Blocks, density, descriptor.BlockLength).AppendLine();
                            }
                        }

                        break;
                    }
                case PeripheralDeviceTypes.PrinterDevice:
                    {
                        switch (header.Value.BufferedMode)
                        {
                            case 0:
                                sb.AppendLine("Device prints directly");
                                break;
                            case 1:
                                sb.AppendLine("Device uses a print cache");
                                break;
                            default:
                                sb.AppendFormat("Unknown buffered mode code 0x{0:X2}", header.Value.BufferedMode).AppendLine();
                                break;
                        }
                        break;
                    }
                case PeripheralDeviceTypes.OpticalDevice:
                    {
                        if (header.Value.WriteProtected)
                            sb.AppendLine("Medium is write protected");
                        if (header.Value.EBC)
                            sb.AppendLine("Blank checking during write is enabled");

                        break;
                    }
                default:
                    break;
            }

            return sb.ToString();
        }
    }
}

