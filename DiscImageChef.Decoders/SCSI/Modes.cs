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
                header.BlockDescriptors = new BlockDescriptor[modeResponse[3] / 8];
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
                header.BlockDescriptors = new BlockDescriptor[blockDescLength / 8];
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

        #region Mode Page 0x0A: Control mode page
        /// <summary>
        /// Control mode page
        /// Page code 0x0A
        /// 8 bytes in SCSI-2
        /// 12 bytes in SPC-1, SPC-2
        /// </summary>
        public struct ModePage_0A
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// If set, target shall report log exception conditions
            /// </summary>
            public bool RLEC;
            /// <summary>
            /// Queue algorithm modifier
            /// </summary>
            public byte QueueAlgorithm;
            /// <summary>
            /// If set all remaining suspended I/O processes shall be aborted after the contingent allegiance condition or extended contingent allegiance condition
            /// </summary>
            public byte QErr;
            /// <summary>
            /// Tagged queuing is disabled
            /// </summary>
            public bool DQue;
            /// <summary>
            /// Extended Contingent Allegiance is enabled
            /// </summary>
            public bool EECA;
            /// <summary>
            /// Target may issue an asynchronous event notification upon completing its initialization
            /// </summary>
            public bool RAENP;
            /// <summary>
            /// Target may issue an asynchronous event notification instead of a unit attention condition
            /// </summary>
            public bool UAAENP;
            /// <summary>
            /// Target may issue an asynchronous event notification instead of a deferred error
            /// </summary>
            public bool EAENP;
            /// <summary>
            /// Minimum time in ms after initialization before attempting asynchronous event notifications
            /// </summary>
            public ushort ReadyAENHoldOffPeriod;

            /// <summary>
            /// Global logging target save disabled
            /// </summary>
            public bool GLTSD;
            /// <summary>
            /// CHECK CONDITION should be reported rather than a long busy condition
            /// </summary>
            public bool RAC;
            /// <summary>
            /// Software write protect is active
            /// </summary>
            public bool SWP;
            /// <summary>
            /// Maximum time in 100 ms units allowed to remain busy. 0xFFFF == unlimited.
            /// </summary>
            public ushort BusyTimeoutPeriod;

            /// <summary>
            /// Task set type
            /// </summary>
            public byte TST;
            /// <summary>
            /// Tasks aborted by other initiator's actions should be terminated with TASK ABORTED
            /// </summary>
            public bool TAS;
            /// <summary>
            /// Action to be taken when a medium is inserted
            /// </summary>
            public byte AutoloadMode;
            /// <summary>
            /// Time in seconds to complete an extended self-test
            /// </summary>
            public byte ExtendedSelfTestCompletionTime;

            /// <summary>
            /// All tasks received in nexus with ACA ACTIVE is set and an ACA condition is established shall terminate
            /// </summary>
            public bool TMF_ONLY;
            /// <summary>
            /// Device shall return descriptor format sense data when returning sense data in the same transactions as a CHECK CONDITION
            /// </summary>
            public bool D_SENSE;
            /// <summary>
            /// Unit attention interlocks control
            /// </summary>
            public byte UA_INTLCK_CTRL;
            /// <summary>
            /// LOGICAL BLOCK APPLICATION TAG should not be modified
            /// </summary>
            public bool ATO;

            /// <summary>
            /// Protector information checking is disabled
            /// </summary>
            public bool DPICZ;
            /// <summary>
            /// No unit attention on release
            /// </summary>
            public bool NUAR;
            /// <summary>
            /// Application Tag mode page is enabled
            /// </summary>
            public bool ATMPE;
            /// <summary>
            /// Abort any write command without protection information
            /// </summary>
            public bool RWWP;
            /// <summary>
            /// Supportes block lengths and protection information
            /// </summary>
            public bool SBLP;
        }

        public static ModePage_0A? DecodeModePage_0A(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x0A)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 8)
                return null;

            ModePage_0A decoded = new ModePage_0A();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.RLEC |= (pageResponse[2] & 0x01) == 0x01;

            decoded.QueueAlgorithm = (byte)((pageResponse[3] & 0xF0) >> 4);
            decoded.QErr = (byte)((pageResponse[3] & 0x06) >> 1);

            decoded.DQue |= (pageResponse[3] & 0x01) == 0x01;
            decoded.EECA |= (pageResponse[4] & 0x80) == 0x80;
            decoded.RAENP |= (pageResponse[4] & 0x04) == 0x04;
            decoded.UAAENP |= (pageResponse[4] & 0x02) == 0x02;
            decoded.EAENP |= (pageResponse[4] & 0x01) == 0x01;

            decoded.ReadyAENHoldOffPeriod = (ushort)((pageResponse[6] << 8) + pageResponse[7]);

            if (pageResponse.Length < 10)
                return decoded;

            // SPC-1
            decoded.GLTSD |= (pageResponse[2] & 0x02) == 0x02;
            decoded.RAC |= (pageResponse[4] & 0x40) == 0x40;
            decoded.SWP |= (pageResponse[4] & 0x08) == 0x08;

            decoded.BusyTimeoutPeriod = (ushort)((pageResponse[8] << 8) + pageResponse[9]);

            // SPC-2
            decoded.TST = (byte)((pageResponse[2] & 0xE0) >> 5);
            decoded.TAS |= (pageResponse[4] & 0x80) == 0x80;
            decoded.AutoloadMode = (byte)(pageResponse[5] & 0x07);
            decoded.BusyTimeoutPeriod = (ushort)((pageResponse[10] << 8) + pageResponse[11]);

            // SPC-3
            decoded.TMF_ONLY |= (pageResponse[2] & 0x10) == 0x10;
            decoded.D_SENSE |= (pageResponse[2] & 0x04) == 0x04;
            decoded.UA_INTLCK_CTRL = (byte)((pageResponse[4] & 0x30) >> 4);
            decoded.TAS |= (pageResponse[5] & 0x40) == 0x40;
            decoded.ATO |= (pageResponse[5] & 0x80) == 0x80;

            // SPC-5
            decoded.DPICZ |= (pageResponse[2] & 0x08) == 0x08;
            decoded.NUAR |= (pageResponse[3] & 0x08) == 0x08;
            decoded.ATMPE |= (pageResponse[5] & 0x20) == 0x20;
            decoded.RWWP |= (pageResponse[5] & 0x10) == 0x10;
            decoded.SBLP |= (pageResponse[5] & 0x08) == 0x08;

            return decoded;
        }

        public static string PrettifyModePage_0A(byte[] pageResponse)
        {
            return PrettifyModePage_0A(DecodeModePage_0A(pageResponse));
        }

        public static string PrettifyModePage_0A(ModePage_0A? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_0A page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Control mode page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");
            if (page.RLEC)
                sb.AppendLine("\tIf set, target shall report log exception conditions");
            if (page.DQue)
                sb.AppendLine("\tTagged queuing is disabled");
            if (page.EECA)
                sb.AppendLine("\tExtended Contingent Allegiance is enabled");
            if (page.RAENP)
                sb.AppendLine("\tTarget may issue an asynchronous event notification upon completing its initialization");
            if (page.UAAENP)
                sb.AppendLine("\tTarget may issue an asynchronous event notification instead of a unit attention condition");
            if (page.EAENP)
                sb.AppendLine("\tTarget may issue an asynchronous event notification instead of a deferred error");
            if (page.GLTSD)
                sb.AppendLine("\tGlobal logging target save disabled");
            if (page.RAC)
                sb.AppendLine("\tCHECK CONDITION should be reported rather than a long busy condition");
            if (page.SWP)
                sb.AppendLine("\tSoftware write protect is active");
            if (page.TAS)
                sb.AppendLine("\tTasks aborted by other initiator's actions should be terminated with TASK ABORTED");
            if (page.TMF_ONLY)
                sb.AppendLine("\tAll tasks received in nexus with ACA ACTIVE is set and an ACA condition is established shall terminate");
            if (page.D_SENSE)
                sb.AppendLine("\tDevice shall return descriptor format sense data when returning sense data in the same transactions as a CHECK CONDITION");
            if (page.ATO)
                sb.AppendLine("\tLOGICAL BLOCK APPLICATION TAG should not be modified");
            if (page.DPICZ)
                sb.AppendLine("\tProtector information checking is disabled");
            if (page.NUAR)
                sb.AppendLine("\tNo unit attention on release");
            if (page.ATMPE)
                sb.AppendLine("\tApplication Tag mode page is enabled");
            if (page.RWWP)
                sb.AppendLine("\tAbort any write command without protection information");
            if (page.SBLP)
                sb.AppendLine("\tSupportes block lengths and protection information");

            switch (page.TST)
            {
                case 0:
                    sb.AppendLine("\tThe logical unit maintains one task set for all nexuses");
                    break;
                case 1:
                    sb.AppendLine("\tThe logical unit maintains separate task sets for each nexus");
                    break;
                default:
                    sb.AppendFormat("\tUnknown Task set type {0}", page.TST).AppendLine();
                    break;
            }

            switch (page.QueueAlgorithm)
            {
                case 0:
                    sb.AppendLine("\tCommands should be sent strictly ordered");
                    break;
                case 1:
                    sb.AppendLine("\tCommands can be reordered in any manner");
                    break;
                default:
                    sb.AppendFormat("\tUnknown Queue Algorithm Modifier {0}", page.QueueAlgorithm).AppendLine();
                    break;
            }

            switch (page.QErr)
            {
                case 0:
                    sb.AppendLine("\tIf ACA is established, the task set commands shall resume after it is cleared, otherwise they shall terminate with CHECK CONDITION");
                    break;
                case 1:
                    sb.AppendLine("\tAll the affected commands in the task set shall be aborted when CHECK CONDITION is returned");
                    break;
                case 3:
                    sb.AppendLine("\tAffected commands in the task set belonging with the CHECK CONDITION nexus shall be aborted");
                    break;
                default:
                    sb.AppendLine("\tReserved QErr value 2 is set");
                    break;
            }

            switch (page.UA_INTLCK_CTRL)
            {
                case 0:
                    sb.AppendLine("\tLUN shall clear unit attention condition reported in the same nexus");
                    break;
                case 2:
                    sb.AppendLine("\tLUN shall not clear unit attention condition reported in the same nexus");
                    break;
                case 3:
                    sb.AppendLine("\tLUN shall not clear unit attention condition reported in the same nexus and shall establish a unit attention condition for the initiator");
                    break;
                default:
                    sb.AppendLine("\tReserved UA_INTLCK_CTRL value 1 is set");
                    break;
            }

            switch (page.AutoloadMode)
            {
                case 0:
                    sb.AppendLine("\tOn medium insertion, it shall be loaded for full access");
                    break;
                case 1:
                    sb.AppendLine("\tOn medium insertion, it shall be loaded for auxiliary memory access only");
                    break;
                case 2:
                    sb.AppendLine("\tOn medium insertion, it shall not be loaded");
                    break;
                default:
                    sb.AppendFormat("\tReserved autoload mode {0} set", page.AutoloadMode).AppendLine();
                    break;
            }

            if (page.ReadyAENHoldOffPeriod > 0)
                sb.AppendFormat("\t{0} ms before attempting asynchronous event notifications after initialization", page.ReadyAENHoldOffPeriod).AppendLine();

            if (page.BusyTimeoutPeriod > 0)
            {
                if (page.BusyTimeoutPeriod == 0xFFFF)
                    sb.AppendLine("\tThere is no limit on the maximum time that is allowed to remain busy");
                else
                    sb.AppendFormat("\tA maximum of {0} ms are allowed to remain busy", (int)page.BusyTimeoutPeriod * 100).AppendLine();
            }

            if (page.ExtendedSelfTestCompletionTime > 0)
                sb.AppendFormat("\t{0} seconds to complete extended self-test", page.ExtendedSelfTestCompletionTime);

            return sb.ToString();
        }
        #endregion Mode Page 0x0A: Control mode page

        #region Mode Page 0x02: Disconnect-reconnect page
        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x02
        /// 16 bytes in SCSI-2
        /// </summary>
        public struct ModePage_02
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// How full should be the buffer prior to attempting a reselection
            /// </summary>
            public byte BufferFullRatio;
            /// <summary>
            /// How empty should be the buffer prior to attempting a reselection
            /// </summary>
            public byte BufferEmptyRatio;
            /// <summary>
            /// Max. time in 100 µs increments that the target is permitted to assert BSY without a REQ/ACK
            /// </summary>
            public ushort BusInactivityLimit;
            /// <summary>
            /// Min. time in 100 µs increments to wait after releasing the bus before attempting reselection
            /// </summary>
            public ushort DisconnectTimeLimit;
            /// <summary>
            /// Max. time in 100 µs increments allowed to use the bus before disconnecting, if granted the privilege and not restricted by <see cref="DTDC"/> 
            /// </summary>
            public ushort ConnectTimeLimit;
            /// <summary>
            /// Maximum amount of data before disconnecting in 512 bytes increments
            /// </summary>
            public ushort MaxBurstSize;
            /// <summary>
            /// Data transfer disconnect control
            /// </summary>
            public byte DTDC;

            /// <summary>
            /// Target shall not transfer data for a command during the same interconnect tenancy
            /// </summary>
            public bool DIMM;
            /// <summary>
            /// Wether to use fair or unfair arbitration when requesting an interconnect tenancy
            /// </summary>
            public byte FairArbitration;
            /// <summary>
            /// Max. ammount of data in 512 bytes increments that may be transferred for a command along with the command
            /// </summary>
            public ushort FirstBurstSize;
            /// <summary>
            /// Target is allowed to re-order the data transfer
            /// </summary>
            public bool EMDP;
        }

        public static ModePage_02? DecodeModePage_02(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x02)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 16)
                return null;

            ModePage_02 decoded = new ModePage_02();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.BufferFullRatio = pageResponse[2];
            decoded.BufferEmptyRatio = pageResponse[3];
            decoded.BusInactivityLimit = (ushort)((pageResponse[4] << 8) + pageResponse[5]);
            decoded.DisconnectTimeLimit = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.ConnectTimeLimit = (ushort)((pageResponse[8] << 8) + pageResponse[9]);
            decoded.MaxBurstSize = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
            decoded.FirstBurstSize = (ushort)((pageResponse[14] << 8) + pageResponse[15]);
            decoded.EMDP |= (pageResponse[12] & 0x80) == 0x80;
            decoded.DIMM |= (pageResponse[12] & 0x08) == 0x08;
            decoded.FairArbitration = (byte)((pageResponse[12] & 0x70) >> 4);
            decoded.DTDC = (byte)(pageResponse[12] & 0x07);

            return decoded;
        }

        public static string PrettifyModePage_02(byte[] pageResponse)
        {
            return PrettifyModePage_02(DecodeModePage_02(pageResponse));
        }

        public static string PrettifyModePage_02(ModePage_02? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_02 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Disconnect-Reconnect mode page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");
            if (page.BufferFullRatio > 0)
                sb.AppendFormat("\t{0} ratio of buffer that shall be full prior to attempting a reselection", page.BufferFullRatio).AppendLine();
            if (page.BufferEmptyRatio > 0)
                sb.AppendFormat("\t{0} ratio of buffer that shall be empty prior to attempting a reselection", page.BufferEmptyRatio).AppendLine();
            if (page.BusInactivityLimit > 0)
                sb.AppendFormat("\t{0} µs maximum permitted to assert BSY without a REQ/ACK handshake", (int)page.BusInactivityLimit * 100).AppendLine();
            if (page.DisconnectTimeLimit > 0)
                sb.AppendFormat("\t{0} µs maximum permitted wait after releasing the bus before attempting reselection", (int)page.DisconnectTimeLimit * 100).AppendLine();
            if (page.ConnectTimeLimit > 0)
                sb.AppendFormat("\t{0} µs allowed to use the bus before disconnecting, if granted the privilege and not restricted", (int)page.ConnectTimeLimit * 100).AppendLine();
            if (page.MaxBurstSize > 0)
                sb.AppendFormat("\t{0} bytes maximum can be transferred before disconnecting", (int)page.MaxBurstSize * 512).AppendLine();
            if (page.FirstBurstSize > 0)
                sb.AppendFormat("\t{0} bytes maximum can be transferred for a command along with the disconnect command", (int)page.FirstBurstSize * 512).AppendLine();

            if (page.DIMM)
                sb.AppendLine("\tTarget shall not transfer data for a command during the same interconnect tenancy");
            if (page.EMDP)
                sb.AppendLine("\tTarget is allowed to re-order the data transfer");

            switch (page.DTDC)
            {
                case 0:
                    sb.AppendLine("\tData transfer disconnect control is not used");
                    break;
                case 1:
                    sb.AppendLine("\tAll data for a command shall be transferred within a single interconnect tenancy");
                    break;
                case 3:
                    sb.AppendLine("\tAll data and the response for a command shall be transferred within a single interconnect tenancy");
                    break;
                default:
                    sb.AppendFormat("\tReserved data transfer disconnect control value {0}", page.DTDC).AppendLine();
                    break;
            }

            return sb.ToString();
        }
        #endregion Mode Page 0x02: Disconnect-reconnect page
    }
}

