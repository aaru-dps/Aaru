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
            /// ECMA-59 &amp; ANSI X3.121-1984: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on Both Sides
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
            /// ECMA-70 &amp; ANSI X3.125-1985: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 1,9 Tracks per mm
            /// </summary>
            ECMA70 = 0x12,
            /// <summary>
            /// ECMA-78 &amp; ANSI X3.126-1986: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 3,8 Tracks per mm
            /// </summary>
            ECMA78 = 0x16,
            /// <summary>
            /// ECMA-99 &amp; ISO 8630-1985: 130 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides; 3,8 Tracks per mm
            /// </summary>
            ECMA99 = 0x1A,
            /// <summary>
            /// ECMA-100 &amp; ANSI X3.137: 90 mm Flexible Disk Cartridge using MFM Recording at 7859 ftprad on Both Sides; 5,3 Tracks per mm
            /// </summary>
            ECMA100 = 0x1E,
            #endregion Medium Types defined in ECMA-111 for Direct-Access devices

            #region Medium Types defined in SCSI-2 for Direct-Access devices
            /// <summary>
            /// Unspecified single sided flexible disk
            /// </summary>
            Unspecified_SS = 0x01,
            /// <summary>
            /// Unspecified double sided flexible disk
            /// </summary>
            Unspecified_DS = 0x02,
            /// <summary>
            /// ANSI X3.73-1980: 200 mm, 6631 ftprad, 1,9 Tracks per mm, 1 side
            /// </summary>
            X3_73 = 0x05,
            /// <summary>
            /// ANSI X3.73-1980: 200 mm, 6631 ftprad, 1,9 Tracks per mm, 2 sides
            /// </summary>
            X3_73_DS = 0x06,
            /// <summary>
            /// ANSI X3.80-1980: 130 mm, 3979 ftprad, 1,9 Tracks per mm, 1 side
            /// </summary>
            X3_82 = 0x0D,
            /// <summary>
            /// 6,3 mm tape with 12 tracks at 394 ftpmm
            /// </summary>
            Tape12 = 0x40,
            /// <summary>
            /// 6,3 mm tape with 24 tracks at 394 ftpmm
            /// </summary>
            Tape24 = 0x44,
            #endregion Medium Types defined in SCSI-2 for Direct-Access devices

            #region Medium Types defined in SCSI-3 SBC-1 for Optical devices
            /// <summary>
            /// Read-only medium
            /// </summary>
            ReadOnly = 0x01,
            /// <summary>
            /// Write-once Read-many medium
            /// </summary>
            WORM = 0x02,
            /// <summary>
            /// Erasable medium
            /// </summary>
            Erasable = 0x03,
            /// <summary>
            /// Combination of read-only and write-once medium
            /// </summary>
            RO_WORM = 0x04,
            /// <summary>
            /// Combination of read-only and erasable medium
            /// </summary>
            RO_RW = 0x05,
            /// <summary>
            /// Combination of write-once and erasable medium
            /// </summary>
            WORM_RW = 0x06,
            #endregion Medium Types defined in SCSI-3 SBC-1 for Optical devices

            #region Medium Types defined in SCSI-2 for MultiMedia devices
            /// <summary>
            /// 120 mm CD-ROM
            /// </summary>
            CDROM = 0x01,
            /// <summary>
            /// 120 mm Compact Disc Digital Audio
            /// </summary>
            CDDA = 0x02,
            /// <summary>
            /// 120 mm Compact Disc with data and audio
            /// </summary>
            MixedCD = 0x03,
            /// <summary>
            /// 80 mm CD-ROM
            /// </summary>
            CDROM_80 = 0x05,
            /// <summary>
            /// 80 mm Compact Disc Digital Audio
            /// </summary>
            CDDA_80 = 0x06,
            /// <summary>
            /// 80 mm Compact Disc with data and audio
            /// </summary>
            MixedCD_80 = 0x07
            #endregion Medium Types defined in SCSI-2 for MultiMedia devices
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

            #region Density Types defined in ECMA-111 for Sequential-Access devices
            /// <summary>
            /// ECMA-62 &amp; ANSI X3.22-1983: 12,7 mm 9-Track Magnetic Tape, 32 ftpmm, NRZI, 32 cpmm
            /// </summary>
            ECMA62 = 0x01,
            /// <summary>
            /// ECMA-62 &amp; ANSI X3.39-1986: 12,7 mm 9-Track Magnetic Tape, 126 ftpmm, Phase Encoding, 63 cpmm
            /// </summary>
            ECMA62_Phase = 0x02,
            /// <summary>
            /// ECMA-62 &amp; ANSI X3.54-1986: 12,7 mm 9-Track Magnetic Tape, 356 ftpmm, NRZI, 245 cpmm GCR
            /// </summary>
            ECMA62_GCR = 0x03,
            /// <summary>
            /// ECMA-79 &amp; ANSI X3.116-1986: 6,30 mm Magnetic Tape Cartridge using MFM Recording at 252 ftpmm
            /// </summary>
            ECMA79 = 0x07,
            /// <summary>
            /// Draft ECMA &amp; ANSI X3B5/87-099: 12,7 mm Magnetic Tape Cartridge using IFM Recording on 18 Tracks at 1944 ftpmm, GCR
            /// </summary>
            ECMADraft = 0x09,
            /// <summary>
            /// ECMA-46 &amp; ANSI X3.56-1986: 6,30 mm Magnetic Tape Cartridge, Phase Encoding, 63 bpmm
            /// </summary>
            ECMA46 = 0x0B,
            /// <summary>
            /// ECMA-98: 6,30 mm Magnetic Tape Cartridge, NRZI Recording, 394 ftpmm
            /// </summary>
            ECMA98 = 0x0E,
            #endregion Density Types defined in ECMA-111 for Sequential-Access devices

            #region Density Types defined in SCSI-2 for Sequential-Access devices
            /// <summary>
            /// ANXI X3.136-1986: 6,3 mm 4 or 9-Track Magnetic Tape Cartridge, 315 bpmm, GCR
            /// </summary>
            X3_136 = 0x05,
            /// <summary>
            /// ANXI X3.157-1987: 12,7 mm 9-Track Magnetic Tape, 126 bpmm, Phase Encoding
            /// </summary>
            X3_157 = 0x06,
            /// <summary>
            /// ANXI X3.158-1987: 3,81 mm 4-Track Magnetic Tape Cassette, 315 bpmm, GCR
            /// </summary>
            X3_158 = 0x08,
            /// <summary>
            /// ANXI X3B5/86-199: 12,7 mm 22-Track Magnetic Tape Cartridge, 262 bpmm, MFM
            /// </summary>
            X3B5_86 = 0x0A,
            /// <summary>
            /// HI-TC1: 12,7 mm 24-Track Magnetic Tape Cartridge, 500 bpmm, GCR
            /// </summary>
            HiTC1 = 0x0C,
            /// <summary>
            /// HI-TC2: 12,7 mm 24-Track Magnetic Tape Cartridge, 999 bpmm, GCR
            /// </summary>
            HiTC2 = 0x0D,
            /// <summary>
            /// QIC-120: 6,3 mm 15-Track Magnetic Tape Cartridge, 394 bpmm, GCR
            /// </summary>
            QIC120 = 0x0F,
            /// <summary>
            /// QIC-150: 6,3 mm 18-Track Magnetic Tape Cartridge, 394 bpmm, GCR
            /// </summary>
            QIC150 = 0x10,
            /// <summary>
            /// QIC-320: 6,3 mm 26-Track Magnetic Tape Cartridge, 630 bpmm, GCR
            /// </summary>
            QIC320 = 0x11,
            /// <summary>
            /// QIC-1350: 6,3 mm 30-Track Magnetic Tape Cartridge, 2034 bpmm, RLL
            /// </summary>
            QIC1350 = 0x12,
            /// <summary>
            /// ANXI X3B5/88-185A: 3,81 mm Magnetic Tape Cassette, 2400 bpmm, DDS
            /// </summary>
            X3B5_88 = 0x13,
            /// <summary>
            /// ANXI X3.202-1991: 8 mm Magnetic Tape Cassette, 1703 bpmm, RLL
            /// </summary>
            X3_202 = 0x14,
            /// <summary>
            /// ECMA TC17: 8 mm Magnetic Tape Cassette, 1789 bpmm, RLL
            /// </summary>
            ECMA_TC17 = 0x15,
            /// <summary>
            /// ANXI X3.193-1990: 12,7 mm 48-Track Magnetic Tape Cartridge, 394 bpmm, MFM
            /// </summary>
            X3_193 = 0x16,
            /// <summary>
            /// ANXI X3B5/97-174: 12,7 mm 48-Track Magnetic Tape Cartridge, 1673 bpmm, MFM
            /// </summary>
            X3B5_91 = 0x17,
            #endregion Density Types defined in SCSI-2 for Sequential-Access devices

            #region Density Types defined in SCSI-2 for MultiMedia devices
            /// <summary>
            /// User data only
            /// </summary>
            User = 0x01,
            /// <summary>
            /// User data plus auxiliary data field
            /// </summary>
            UserAuxiliary = 0x02,
            /// <summary>
            /// 4-byt tag field, user data plus auxiliary data
            /// </summary>
            UserAuxiliaryTag = 0x03,
            /// <summary>
            /// Audio information only
            /// </summary>
            Audio = 0x04,
            #endregion Density Types defined in SCSI-2 for MultiMedia devices

            #region Density Types defined in SCSI-2 for Optical devices
            /// <summary>
            /// ISO/IEC 10090: 86 mm Read/Write single-sided optical disc with 12500 tracks
            /// </summary>
            ISO10090 = 0x01,
            /// <summary>
            /// 89 mm Read/Write double-sided optical disc with 12500 tracks
            /// </summary>
            D581 = 0x02,
            /// <summary>
            /// ANSI X3.212: 130 mm Read/Write double-sided optical disc with 18750 tracks
            /// </summary>
            X3_212 = 0x03,
            /// <summary>
            /// ANSI X3.191: 130 mm Write-Once double-sided optical disc with 30000 tracks
            /// </summary>
            X3_191 = 0x04,
            /// <summary>
            /// ANSI X3.214: 130 mm Write-Once double-sided optical disc with 20000 tracks
            /// </summary>
            X3_214 = 0x05,
            /// <summary>
            /// ANSI X3.211: 130 mm Write-Once double-sided optical disc with 18750 tracks
            /// </summary>
            X3_211 = 0x06,
            /// <summary>
            /// 200 mm optical disc
            /// </summary>
            D407 = 0x07,
            /// <summary>
            /// ISO/IEC 13614: 300 mm double-sided optical disc
            /// </summary>
            ISO13614 = 0x08,
            /// <summary>
            /// ANSI X3.200: 356 mm double-sided optical disc with 56350 tracks
            /// </summary>
            X3_200 = 0x09
            #endregion Density Types defined in SCSI-2 for Optical devices
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
            public bool DPOFUA;
        }

        public static ModeHeader? DecodeModeHeader6(byte[] modeResponse, PeripheralDeviceTypes deviceType)
        {
            if (modeResponse == null || modeResponse.Length < 4 || modeResponse.Length < modeResponse[0] + 1)
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

            if (deviceType == PeripheralDeviceTypes.DirectAccess || deviceType == PeripheralDeviceTypes.MultiMediaDevice)
            {
                header.WriteProtected = ((modeResponse[2] & 0x80) == 0x80);
                header.DPOFUA = ((modeResponse[2] & 0x10) == 0x10);
            }

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
                header.DPOFUA = ((modeResponse[2] & 0x10) == 0x10);
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
                        if (header.Value.MediumType != MediumTypes.Default)
                        {
                            sb.Append("Medium is ");

                            switch (header.Value.MediumType)
                            {
                                case MediumTypes.ECMA54:
                                    sb.AppendLine("ECMA-54: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on One Side");
                                    break;
                                case MediumTypes.ECMA59:
                                    sb.AppendLine("ECMA-59 & ANSI X3.121-1984: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on Both Sides");
                                    break;
                                case MediumTypes.ECMA69:
                                    sb.AppendLine("ECMA-69: 200 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides");
                                    break;
                                case MediumTypes.ECMA66:
                                    sb.AppendLine("ECMA-66: 130 mm Flexible Disk Cartridge using Two-Frequency Recording at 7958 ftprad on One Side");
                                    break;
                                case MediumTypes.ECMA70:
                                    sb.AppendLine("ECMA-70 & ANSI X3.125-1985: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 1,9 Tracks per mm");
                                    break;
                                case MediumTypes.ECMA78:
                                    sb.AppendLine("ECMA-78 & ANSI X3.126-1986: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 3,8 Tracks per mm");
                                    break;
                                case MediumTypes.ECMA99:
                                    sb.AppendLine("ECMA-99 & ISO 8630-1985: 130 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides; 3,8 Tracks per mm");
                                    break;
                                case MediumTypes.ECMA100:
                                    sb.AppendLine("ECMA-100 & ANSI X3.137: 90 mm Flexible Disk Cartridge using MFM Recording at 7859 ftprad on Both Sides; 5,3 Tracks per mm");
                                    break;
                                case MediumTypes.Unspecified_SS:
                                    sb.AppendLine("Unspecified single sided flexible disk");
                                    break;
                                case MediumTypes.Unspecified_DS:
                                    sb.AppendLine("Unspecified double sided flexible disk");
                                    break;
                                case MediumTypes.X3_73:
                                    sb.AppendLine("ANSI X3.73-1980: 200 mm, 6631 ftprad, 1,9 Tracks per mm, 1 side");
                                    break;
                                case MediumTypes.X3_73_DS:
                                    sb.AppendLine("ANSI X3.73-1980: 200 mm, 6631 ftprad, 1,9 Tracks per mm, 2 sides");
                                    break;
                                case MediumTypes.X3_82:
                                    sb.AppendLine("ANSI X3.80-1980: 130 mm, 3979 ftprad, 1,9 Tracks per mm, 1 side");
                                    break;
                                case MediumTypes.Tape12:
                                    sb.AppendLine("6,3 mm tape with 12 tracks at 394 ftpmm");
                                    break;
                                case MediumTypes.Tape24:
                                    sb.AppendLine("6,3 mm tape with 24 tracks at 394 ftpmm");
                                    break;
                                default:
                                    sb.AppendFormat("Unknown medium type 0x{0:X2}", header.Value.MediumType).AppendLine();
                                    break;
                            }
                        }

                        if (header.Value.WriteProtected)
                            sb.AppendLine("Medium is write protected");

                        if (header.Value.DPOFUA)
                            sb.AppendLine("Drive supports DPO and FUA bits");

                        foreach (BlockDescriptor descriptor in header.Value.BlockDescriptors)
                        {
                            string density = "";
                            switch (descriptor.Density)
                            {
                                case DensityType.Default:
                                    break;
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

                            if (density != "")
                            {
                                if (descriptor.Blocks == 0)
                                    sb.AppendFormat("All remaining blocks have {0} and are {1} bytes each", density, descriptor.BlockLength).AppendLine();
                                else
                                    sb.AppendFormat("{0} blocks have {1} and are {2} bytes each", descriptor.Blocks, density, descriptor.BlockLength).AppendLine();
                            }
                            else
                            {
                                if (descriptor.Blocks == 0)
                                    sb.AppendFormat("All remaining blocks are {0} bytes each", descriptor.BlockLength).AppendLine();
                                else
                                    sb.AppendFormat("{0} blocks are {1} bytes each", descriptor.Blocks, descriptor.BlockLength).AppendLine();
                            }
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
                            case 2:
                                sb.AppendLine("Device uses a write cache but doesn't return until cache is flushed");
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
                            string density = "";
                            switch (descriptor.Density)
                            {
                                case DensityType.Default:
                                    break;
                                case DensityType.ECMA62:
                                    density = "ECMA-62 & ANSI X3.22-1983: 12,7 mm 9-Track Magnetic Tape, 32 ftpmm, NRZI, 32 cpmm";
                                    break;
                                case DensityType.ECMA62_Phase:
                                    density = "ECMA-62 & ANSI X3.39-1986: 12,7 mm 9-Track Magnetic Tape, 126 ftpmm, Phase Encoding, 63 cpmm";
                                    break;
                                case DensityType.ECMA62_GCR:
                                    density = "ECMA-62 & ANSI X3.54-1986: 12,7 mm 9-Track Magnetic Tape, 356 ftpmm, NRZI, 245 cpmm GCR";
                                    break;
                                case DensityType.ECMA79:
                                    density = "ECMA-79 & ANSI X3.116-1986: 6,30 mm Magnetic Tape Cartridge, 252 ftpmm, MFM";
                                    break;
                                case DensityType.ECMADraft:
                                    density = "Draft ECMA & ANSI X3B5/87-099: 12,7 mm 18-Track Magnetic Tape Cartridge, 1944 ftpmm, IFM, GCR";
                                    break;
                                case DensityType.ECMA46:
                                    density = "ECMA-46 & ANSI X3.56-1986: 6,30 mm Magnetic Tape Cartridge, Phase Encoding, 63 bpmm";
                                    break;
                                case DensityType.ECMA98:
                                    density = "ECMA-98: 6,30 mm Magnetic Tape Cartridge, NRZI, 394 ftpmm";
                                    break;
                                case DensityType.X3_136:
                                    density = "ANXI X3.136-1986: 6,3 mm 4 or 9-Track Magnetic Tape Cartridge, 315 bpmm, GCR";
                                    break;
                                case DensityType.X3_157:
                                    density = "ANXI X3.157-1987: 12,7 mm 9-Track Magnetic Tape, 126 bpmm, Phase Encoding";
                                    break;
                                case DensityType.X3_158:
                                    density = "ANXI X3.158-1987: 3,81 mm 4-Track Magnetic Tape Cassette, 315 bpmm, GCR";
                                    break;
                                case DensityType.X3B5_86:
                                    density = "ANXI X3B5/86-199: 12,7 mm 22-Track Magnetic Tape Cartridge, 262 bpmm, MFM";
                                    break;
                                case DensityType.HiTC1:
                                    density = "HI-TC1: 12,7 mm 24-Track Magnetic Tape Cartridge, 500 bpmm, GCR";
                                    break;
                                case DensityType.HiTC2:
                                    density = "HI-TC2: 12,7 mm 24-Track Magnetic Tape Cartridge, 999 bpmm, GCR";
                                    break;
                                case DensityType.QIC120:
                                    density = "QIC-120: 6,3 mm 15-Track Magnetic Tape Cartridge, 394 bpmm, GCR";
                                    break;
                                case DensityType.QIC150:
                                    density = "QIC-150: 6,3 mm 18-Track Magnetic Tape Cartridge, 394 bpmm, GCR";
                                    break;
                                case DensityType.QIC320:
                                    density = "QIC-320: 6,3 mm 26-Track Magnetic Tape Cartridge, 630 bpmm, GCR";
                                    break;
                                case DensityType.QIC1350:
                                    density = "QIC-1350: 6,3 mm 30-Track Magnetic Tape Cartridge, 2034 bpmm, RLL";
                                    break;
                                case DensityType.X3B5_88:
                                    density = "ANXI X3B5/88-185A: 3,81 mm Magnetic Tape Cassette, 2400 bpmm, DDS";
                                    break;
                                case DensityType.X3_202:
                                    density = "ANXI X3.202-1991: 8 mm Magnetic Tape Cassette, 1703 bpmm, RLL";
                                    break;
                                case DensityType.ECMA_TC17:
                                    density = "ECMA TC17: 8 mm Magnetic Tape Cassette, 1789 bpmm, RLL";
                                    break;
                                case DensityType.X3_193:
                                    density = "ANXI X3.193-1990: 12,7 mm 48-Track Magnetic Tape Cartridge, 394 bpmm, MFM";
                                    break;
                                case DensityType.X3B5_91:
                                    density = "ANXI X3B5/97-174: 12,7 mm 48-Track Magnetic Tape Cartridge, 1673 bpmm, MFM";
                                    break;
                                default:
                                    density = String.Format("Unknown density code 0x{0:X2}", descriptor.Density);
                                    break;
                            }

                            if (density != "")
                            {
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
                            else
                            {
                                if (descriptor.Blocks == 0)
                                {
                                    if (descriptor.BlockLength == 0)
                                        sb.AppendFormat("All remaining blocks have a variable length").AppendLine();
                                    else
                                        sb.AppendFormat("All remaining blocks are {0} bytes each", descriptor.BlockLength).AppendLine();
                                }
                                else
                                {
                                    if (descriptor.BlockLength == 0)
                                        sb.AppendFormat("{0} blocks have a variable length", descriptor.Blocks).AppendLine();
                                    else
                                        sb.AppendFormat("{0} blocks are {1} bytes each", descriptor.Blocks, descriptor.BlockLength).AppendLine();
                                }
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
                        if (header.Value.MediumType != MediumTypes.Default)
                        {
                            sb.Append("Medium is ");

                            switch (header.Value.MediumType)
                            {
                                case MediumTypes.ReadOnly:
                                    sb.AppendLine("a Read-only optical");
                                    break;
                                case MediumTypes.WORM:
                                    sb.AppendLine("a Write-once Read-many optical");
                                    break;
                                case MediumTypes.Erasable:
                                    sb.AppendLine("a Erasable optical");
                                    break;
                                case MediumTypes.RO_WORM:
                                    sb.AppendLine("a combination of read-only and write-once optical");
                                    break;
                                case MediumTypes.RO_RW:
                                    sb.AppendLine("a combination of read-only and erasable optical");
                                    break;
                                case MediumTypes.WORM_RW:
                                    sb.AppendLine("a combination of write-once and erasable optical");
                                    break;
                                default:
                                    sb.AppendFormat("an unknown medium type 0x{0:X2}", header.Value.MediumType).AppendLine();
                                    break;
                            }
                        }

                        if (header.Value.WriteProtected)
                            sb.AppendLine("Medium is write protected");
                        if (header.Value.EBC)
                            sb.AppendLine("Blank checking during write is enabled");
                        if (header.Value.DPOFUA)
                            sb.AppendLine("Drive supports DPO and FUA bits");

                        foreach (BlockDescriptor descriptor in header.Value.BlockDescriptors)
                        {
                            string density = "";
                            switch (descriptor.Density)
                            {
                                case DensityType.Default:
                                    break;
                                case DensityType.ISO10090:
                                    density = "ISO/IEC 10090: 86 mm Read/Write single-sided optical disc with 12500 tracks";
                                    break;
                                case DensityType.D581:
                                    density = "89 mm Read/Write double-sided optical disc with 12500 tracks";
                                    break;
                                case DensityType.X3_212:
                                    density = "ANSI X3.212: 130 mm Read/Write double-sided optical disc with 18750 tracks";
                                    break;
                                case DensityType.X3_191:
                                    density = "ANSI X3.191: 130 mm Write-Once double-sided optical disc with 30000 tracks";
                                    break;
                                case DensityType.X3_214:
                                    density = "ANSI X3.214: 130 mm Write-Once double-sided optical disc with 20000 tracks";
                                    break;
                                case DensityType.X3_211:
                                    density = "ANSI X3.211: 130 mm Write-Once double-sided optical disc with 18750 tracks";
                                    break;
                                case DensityType.D407:
                                    density = "200 mm optical disc";
                                    break;
                                case DensityType.ISO13614:
                                    density = "ISO/IEC 13614: 300 mm double-sided optical disc";
                                    break;
                                case DensityType.X3_200:
                                    density = "ANSI X3.200: 356 mm double-sided optical disc with 56350 tracks";
                                    break;
                                default:
                                    density = String.Format("Unknown density code 0x{0:X2}", descriptor.Density);
                                    break;
                            }

                            if (density != "")
                            {
                                if (descriptor.Blocks == 0)
                                {
                                    if (descriptor.BlockLength == 0)
                                        sb.AppendFormat("All remaining blocks are {0} and have a variable length", density).AppendLine();
                                    else
                                        sb.AppendFormat("All remaining blocks are {0} and are {1} bytes each", density, descriptor.BlockLength).AppendLine();
                                }
                                else
                                {
                                    if (descriptor.BlockLength == 0)
                                        sb.AppendFormat("{0} blocks are {1} and have a variable length", descriptor.Blocks, density).AppendLine();
                                    else
                                        sb.AppendFormat("{0} blocks are {1} and are {2} bytes each", descriptor.Blocks, density, descriptor.BlockLength).AppendLine();
                                }
                            }
                            else
                            {
                                if (descriptor.Blocks == 0)
                                {
                                    if (descriptor.BlockLength == 0)
                                        sb.AppendFormat("All remaining blocks have a variable length").AppendLine();
                                    else
                                        sb.AppendFormat("All remaining blocks are {0} bytes each", descriptor.BlockLength).AppendLine();
                                }
                                else
                                {
                                    if (descriptor.BlockLength == 0)
                                        sb.AppendFormat("{0} blocks have a variable length", descriptor.Blocks).AppendLine();
                                    else
                                        sb.AppendFormat("{0} blocks are {1} bytes each", descriptor.Blocks, descriptor.BlockLength).AppendLine();
                                }
                            }
                        }

                        break;
                    }
                case PeripheralDeviceTypes.MultiMediaDevice:
                    {
                        sb.Append("Medium is ");

                        switch (header.Value.MediumType)
                        {
                            case MediumTypes.CDROM:
                                sb.AppendLine("120 mm CD-ROM");
                                break;
                            case MediumTypes.CDDA:
                                sb.AppendLine("120 mm Compact Disc Digital Audio");
                                break;
                            case MediumTypes.MixedCD:
                                sb.AppendLine("120 mm Compact Disc with data and audio");
                                break;
                            case MediumTypes.CDROM_80:
                                sb.AppendLine("80 mm CD-ROM");
                                break;
                            case MediumTypes.CDDA_80:
                                sb.AppendLine("80 mm Compact Disc Digital Audio");
                                break;
                            case MediumTypes.MixedCD_80:
                                sb.AppendLine("80 mm Compact Disc with data and audio");
                                break;
                            default:
                                sb.AppendFormat("Unknown medium type 0x{0:X2}", header.Value.MediumType).AppendLine();
                                break;
                        }

                        if (header.Value.WriteProtected)
                            sb.AppendLine("Medium is write protected");

                        if (header.Value.DPOFUA)
                            sb.AppendLine("Drive supports DPO and FUA bits");

                        foreach (BlockDescriptor descriptor in header.Value.BlockDescriptors)
                        {
                            string density = "";
                            switch (descriptor.Density)
                            {
                                case DensityType.Default:
                                    break;
                                case DensityType.User:
                                    density = "user data only";
                                    break;
                                case DensityType.UserAuxiliary:
                                    density = "user data plus auxiliary data";
                                    break;
                                case DensityType.UserAuxiliaryTag:
                                    density = "4-byte tag, user data plus auxiliary data";
                                    break;
                                case DensityType.Audio:
                                    density = "audio information only";
                                    break;
                                default:
                                    density = String.Format("with unknown density code 0x{0:X2}", descriptor.Density);
                                    break;
                            }

                            if (density != "")
                            {
                                if (descriptor.Blocks == 0)
                                    sb.AppendFormat("All remaining blocks have {0} and are {1} bytes each", density, descriptor.BlockLength).AppendLine();
                                else
                                    sb.AppendFormat("{0} blocks have {1} and are {2} bytes each", descriptor.Blocks, density, descriptor.BlockLength).AppendLine();
                            }
                            else
                            {
                                if (descriptor.Blocks == 0)
                                    sb.AppendFormat("All remaining blocks are {0} bytes each", descriptor.BlockLength).AppendLine();
                                else
                                    sb.AppendFormat("{0} blocks are {1} bytes each", descriptor.Blocks, descriptor.BlockLength).AppendLine();
                            }
                        }

                        break;
                    }
                default:
                    break;
            }

            return sb.ToString();
        }

        public static ModeHeader? DecodeModeHeader10(byte[] modeResponse, PeripheralDeviceTypes deviceType)
        {
            if (modeResponse == null || modeResponse.Length < 8)
                return null;

            ushort modeLength;
            ushort blockDescLength;

            modeLength = (ushort)((modeResponse[0] << 8) + modeResponse[1]);
            blockDescLength = (ushort)((modeResponse[6] << 8) + modeResponse[7]);

            if (modeResponse.Length < modeLength)
                return null;

            ModeHeader header = new ModeHeader();
            header.MediumType = (MediumTypes)modeResponse[2];

            if (blockDescLength > 0)
            {
                header.BlockDescriptors = new BlockDescriptor[blockDescLength/8];
                for (int i = 0; i < header.BlockDescriptors.Length; i++)
                {
                    header.BlockDescriptors[i].Density = (DensityType)modeResponse[0 + i * 8 + 8];
                    header.BlockDescriptors[i].Blocks += (ulong)(modeResponse[1 + i * 8 + 8] << 16);
                    header.BlockDescriptors[i].Blocks += (ulong)(modeResponse[2 + i * 8 + 8] << 8);
                    header.BlockDescriptors[i].Blocks += modeResponse[3 + i * 8 + 8];
                    header.BlockDescriptors[i].BlockLength += (ulong)(modeResponse[5 + i * 8 + 8] << 16);
                    header.BlockDescriptors[i].BlockLength += (ulong)(modeResponse[6 + i * 8 + 8] << 8);
                    header.BlockDescriptors[i].BlockLength += modeResponse[7 + i * 8 + 8];
                }
            }

            if (deviceType == PeripheralDeviceTypes.DirectAccess || deviceType == PeripheralDeviceTypes.MultiMediaDevice)
            {
                header.WriteProtected = ((modeResponse[3] & 0x80) == 0x80);
                header.DPOFUA = ((modeResponse[3] & 0x10) == 0x10);
            }

            if (deviceType == PeripheralDeviceTypes.SequentialAccess)
            {
                header.WriteProtected = ((modeResponse[3] & 0x80) == 0x80);
                header.Speed = (byte)(modeResponse[3] & 0x0F);
                header.BufferedMode = (byte)((modeResponse[3] & 0x70) >> 4);
            }

            if (deviceType == PeripheralDeviceTypes.PrinterDevice)
                header.BufferedMode = (byte)((modeResponse[3] & 0x70) >> 4);

            if (deviceType == PeripheralDeviceTypes.OpticalDevice)
            {
                header.WriteProtected = ((modeResponse[3] & 0x80) == 0x80);
                header.EBC = ((modeResponse[3] & 0x01) == 0x01);
                header.DPOFUA = ((modeResponse[3] & 0x10) == 0x10);
            }

            return header;
        }

        public static string PrettifyModeHeader10(byte[] modeResponse, PeripheralDeviceTypes deviceType)
        {
            return PrettifyModeHeader(DecodeModeHeader10(modeResponse, deviceType), deviceType);
        }
    }
}

