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
            MixedCD_80 = 0x07,

            #endregion Medium Types defined in SCSI-2 for MultiMedia devices

            #region Medium Types defined in SFF-8020i
            /// <summary>
            /// Unknown medium type
            /// </summary>
            Unknown_CD = 0x00,
            /// <summary>
            /// 120 mm Hybrid disc (Photo CD)
            /// </summary>
            HybridCD = 0x04,
            /// <summary>
            /// Unknown size CD-R
            /// </summary>
            Unknown_CDR = 0x10,
            /// <summary>
            /// 120 mm CD-R with data only
            /// </summary>
            CDR = 0x11,
            /// <summary>
            /// 120 mm CD-R with audio only
            /// </summary>
            CDR_DA = 0x12,
            /// <summary>
            /// 120 mm CD-R with data and audio
            /// </summary>
            CDR_Mixed = 0x13,
            /// <summary>
            /// 120 mm Hybrid CD-R (Photo CD)
            /// </summary>
            HybridCDR = 0x14,
            /// <summary>
            /// 80 mm CD-R with data only
            /// </summary>
            CDR_80 = 0x15,
            /// <summary>
            /// 80 mm CD-R with audio only
            /// </summary>
            CDR_DA_80 = 0x16,
            /// <summary>
            /// 80 mm CD-R with data and audio
            /// </summary>
            CDR_Mixed_80 = 0x17,
            /// <summary>
            /// 80 mm Hybrid CD-R (Photo CD)
            /// </summary>
            HybridCDR_80 = 0x18,
            /// <summary>
            /// Unknown size CD-RW
            /// </summary>
            Unknown_CDRW = 0x20,
            /// <summary>
            /// 120 mm CD-RW with data only
            /// </summary>
            CDRW = 0x21,
            /// <summary>
            /// 120 mm CD-RW with audio only
            /// </summary>
            CDRW_DA = 0x22,
            /// <summary>
            /// 120 mm CD-RW with data and audio
            /// </summary>
            CDRW_Mixed = 0x23,
            /// <summary>
            /// 120 mm Hybrid CD-RW (Photo CD)
            /// </summary>
            HybridCDRW = 0x24,
            /// <summary>
            /// 80 mm CD-RW with data only
            /// </summary>
            CDRW_80 = 0x25,
            /// <summary>
            /// 80 mm CD-RW with audio only
            /// </summary>
            CDRW_DA_80 = 0x26,
            /// <summary>
            /// 80 mm CD-RW with data and audio
            /// </summary>
            CDRW_Mixed_80 = 0x27,
            /// <summary>
            /// 80 mm Hybrid CD-RW (Photo CD)
            /// </summary>
            HybridCDRW_80 = 0x28,
            /// <summary>
            /// Unknown size HD disc
            /// </summary>
            Unknown_HD = 0x30,
            /// <summary>
            /// 120 mm HD disc
            /// </summary>
            HD = 0x31,
            /// <summary>
            /// 80 mm HD disc
            /// </summary>
            HD_80 = 0x35,
            /// <summary>
            /// No disc inserted, tray closed or caddy inserted
            /// </summary>
            NoDisc = 0x70,
            /// <summary>
            /// Tray open or no caddy inserted
            /// </summary>
            TrayOpen = 0x71,
            /// <summary>
            /// Tray closed or caddy inserted but medium error
            /// </summary>
            MediumError = 0x72,
            #endregion Medium Types defined in SFF-8020i

            #region Medium Types defined in USB Mass Storage Class - UFI Command Specification
            /// <summary>
            /// 3.5-inch, 135 tpi, 12362 bits/radian, double-sided MFM (aka 1.25Mb)
            /// </summary>
            Type3Floppy = 0x93,
            /// <summary>
            /// 3.5-inch, 135 tpi, 15916 bits/radian, double-sided MFM (aka 1.44Mb)
            /// </summary>
            HDFloppy = 0x94,
            #endregion Medium Types defined in USB Mass Storage Class - UFI Command Specification

            #region Medium Types defined in INF-8070
            /// <summary>
            /// Unknown type block device
            /// </summary>
            UnknownBlockDevice = 0x40,
            /// <summary>
            /// Read-only block device
            /// </summary>
            ReadOnlyBlockDevice = 0x41,
            /// <summary>
            /// Read/Write block device
            /// </summary>
            ReadWriteBlockDevice = 0x42
            #endregion Medium Types defined in INF-8070
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
                                case MediumTypes.Type3Floppy:
                                    sb.AppendLine("3.5-inch, 135 tpi, 12362 bits/radian, double-sided MFM (aka 1.25Mb)");
                                    break;
                                case MediumTypes.HDFloppy:
                                    sb.AppendLine("3.5-inch, 135 tpi, 15916 bits/radian, double-sided MFM (aka 1.44Mb)");
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
                            case MediumTypes.Unknown_CD:
                                sb.AppendLine("Unknown medium type");
                                break;
                            case MediumTypes.HybridCD:
                                sb.AppendLine("120 mm Hybrid disc (Photo CD)");
                                break;
                            case MediumTypes.Unknown_CDR:
                                sb.AppendLine("Unknown size CD-R");
                                break;
                            case MediumTypes.CDR:
                                sb.AppendLine("120 mm CD-R with data only");
                                break;
                            case MediumTypes.CDR_DA:
                                sb.AppendLine("120 mm CD-R with audio only");
                                break;
                            case MediumTypes.CDR_Mixed:
                                sb.AppendLine("120 mm CD-R with data and audio");
                                break;
                            case MediumTypes.HybridCDR:
                                sb.AppendLine("120 mm Hybrid CD-R (Photo CD)");
                                break;
                            case MediumTypes.CDR_80:
                                sb.AppendLine("80 mm CD-R with data only");
                                break;
                            case MediumTypes.CDR_DA_80:
                                sb.AppendLine("80 mm CD-R with audio only");
                                break;
                            case MediumTypes.CDR_Mixed_80:
                                sb.AppendLine("80 mm CD-R with data and audio");
                                break;
                            case MediumTypes.HybridCDR_80:
                                sb.AppendLine("80 mm Hybrid CD-R (Photo CD)");
                                break;
                            case MediumTypes.Unknown_CDRW:
                                sb.AppendLine("Unknown size CD-RW");
                                break;
                            case MediumTypes.CDRW:
                                sb.AppendLine("120 mm CD-RW with data only");
                                break;
                            case MediumTypes.CDRW_DA:
                                sb.AppendLine("120 mm CD-RW with audio only");
                                break;
                            case MediumTypes.CDRW_Mixed:
                                sb.AppendLine("120 mm CD-RW with data and audio");
                                break;
                            case MediumTypes.HybridCDRW:
                                sb.AppendLine("120 mm Hybrid CD-RW (Photo CD)");
                                break;
                            case MediumTypes.CDRW_80:
                                sb.AppendLine("80 mm CD-RW with data only");
                                break;
                            case MediumTypes.CDRW_DA_80:
                                sb.AppendLine("80 mm CD-RW with audio only");
                                break;
                            case MediumTypes.CDRW_Mixed_80:
                                sb.AppendLine("80 mm CD-RW with data and audio");
                                break;
                            case MediumTypes.HybridCDRW_80:
                                sb.AppendLine("80 mm Hybrid CD-RW (Photo CD)");
                                break;
                            case MediumTypes.Unknown_HD:
                                sb.AppendLine("Unknown size HD disc");
                                break;
                            case MediumTypes.HD:
                                sb.AppendLine("120 mm HD disc");
                                break;
                            case MediumTypes.HD_80:
                                sb.AppendLine("80 mm HD disc");
                                break;
                            case MediumTypes.NoDisc:
                                sb.AppendLine("No disc inserted, tray closed or caddy inserted");
                                break;
                            case MediumTypes.TrayOpen:
                                sb.AppendLine("Tray open or no caddy inserted");
                                break;
                            case MediumTypes.MediumError:
                                sb.AppendLine("Tray closed or caddy inserted but medium error");
                                break;
                            case MediumTypes.UnknownBlockDevice:
                                sb.AppendLine("Unknown block device");
                                break;
                            case MediumTypes.ReadOnlyBlockDevice:
                                sb.AppendLine("Read-only block device");
                                break;
                            case MediumTypes.ReadWriteBlockDevice:
                                sb.AppendLine("Read/Write block device");
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
        /// 12 bytes in SPC-1, SPC-2, SPC-3, SPC-4, SPC-5
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

            if ((pageResponse[0] & 0x40) == 0x40)
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
        /// 16 bytes in SCSI-2, SPC-1, SPC-2, SPC-3, SPC-4, SPC-5
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

            if ((pageResponse[0] & 0x40) == 0x40)
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

        #region Mode Page 0x08: Caching page
        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x08
        /// 12 bytes in SCSI-2
        /// 20 bytes in SBC-1, SBC-2, SBC-3
        /// </summary>
        public struct ModePage_08
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// <c>true</c> if write cache is enabled
            /// </summary>
            public bool WCE;
            /// <summary>
            /// Multiplication factor
            /// </summary>
            public bool MF;
            /// <summary>
            /// <c>true</c> if read cache is enabled
            /// </summary>
            public bool RCD;
            /// <summary>
            /// Advices on reading-cache retention priority
            /// </summary>
            public byte DemandReadRetentionPrio;
            /// <summary>
            /// Advices on writing-cache retention priority
            /// </summary>
            public byte WriteRetentionPriority;
            /// <summary>
            /// If requested read blocks are more than this, no pre-fetch is done
            /// </summary>
            public ushort DisablePreFetch;
            /// <summary>
            /// Minimum pre-fetch
            /// </summary>
            public ushort MinimumPreFetch;
            /// <summary>
            /// Maximum pre-fetch
            /// </summary>
            public ushort MaximumPreFetch;
            /// <summary>
            /// Upper limit on maximum pre-fetch value
            /// </summary>
            public ushort MaximumPreFetchCeiling;

            /// <summary>
            /// Manual cache controlling
            /// </summary>
            public bool IC;
            /// <summary>
            /// Abort pre-fetch
            /// </summary>
            public bool ABPF;
            /// <summary>
            /// Caching analysis permitted
            /// </summary>
            public bool CAP;
            /// <summary>
            /// Pre-fetch over discontinuities
            /// </summary>
            public bool Disc;
            /// <summary>
            /// <see cref="CacheSegmentSize"/> is to be used to control caching segmentation
            /// </summary>
            public bool Size;
            /// <summary>
            /// Force sequential write
            /// </summary>
            public bool FSW;
            /// <summary>
            /// Logical block cache segment size
            /// </summary>
            public bool LBCSS;
            /// <summary>
            /// Disable read-ahead
            /// </summary>
            public bool DRA;
            /// <summary>
            /// How many segments should the cache be divided upon
            /// </summary>
            public byte CacheSegments;
            /// <summary>
            /// How many bytes should the cache be divided upon
            /// </summary>
            public ushort CacheSegmentSize;
            /// <summary>
            /// How many bytes should be used as a buffer when all other cached data cannot be evicted
            /// </summary>
            public uint NonCacheSegmentSize;

            public bool NV_DIS;
        }

        public static ModePage_08? DecodeModePage_08(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x08)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 12)
                return null;

            ModePage_08 decoded = new ModePage_08();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.WCE |= (pageResponse[2] & 0x04) == 0x04;
            decoded.MF |= (pageResponse[2] & 0x02) == 0x02;
            decoded.RCD |= (pageResponse[2] & 0x01) == 0x01;

            decoded.DemandReadRetentionPrio = (byte)((pageResponse[3] & 0xF0) >> 4);
            decoded.WriteRetentionPriority = (byte)(pageResponse[3] & 0x0F);
            decoded.DisablePreFetch = (ushort)((pageResponse[4] << 8) + pageResponse[5]);
            decoded.MinimumPreFetch = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.MaximumPreFetch = (ushort)((pageResponse[8] << 8) + pageResponse[9]);
            decoded.MaximumPreFetchCeiling = (ushort)((pageResponse[10] << 8) + pageResponse[11]);

            if (pageResponse.Length < 20)
                return decoded;

            decoded.IC |= (pageResponse[2] & 0x80) == 0x80;
            decoded.ABPF |= (pageResponse[2] & 0x40) == 0x40;
            decoded.CAP |= (pageResponse[2] & 0x20) == 0x20;
            decoded.Disc |= (pageResponse[2] & 0x10) == 0x10;
            decoded.Size |= (pageResponse[2] & 0x08) == 0x08;

            decoded.FSW |= (pageResponse[12] & 0x80) == 0x80;
            decoded.LBCSS |= (pageResponse[12] & 0x40) == 0x40;
            decoded.DRA |= (pageResponse[12] & 0x20) == 0x20;

            decoded.CacheSegments = pageResponse[13];
            decoded.CacheSegmentSize = (ushort)((pageResponse[14] << 8) + pageResponse[15]);
            decoded.NonCacheSegmentSize = (uint)((pageResponse[17] << 16) + (pageResponse[18] << 8) + pageResponse[19]);

            decoded.NV_DIS |= (pageResponse[12] & 0x01) == 0x01;

            return decoded;
        }

        public static string PrettifyModePage_08(byte[] pageResponse)
        {
            return PrettifyModePage_08(DecodeModePage_08(pageResponse));
        }

        public static string PrettifyModePage_08(ModePage_08? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_08 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Caching mode page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");
            if (page.RCD)
                sb.AppendLine("\tRead-cache is enabled");
            if (page.WCE)
                sb.AppendLine("\tWrite-cache is enabled");
            
            switch (page.DemandReadRetentionPrio)
            {
                case 0:
                    sb.AppendLine("\tDrive does not distinguish between cached read data");
                    break;
                case 1:
                    sb.AppendLine("\tData put by READ commands should be evicted from cache sooner than data put in read cache by other means");
                    break;
                case 0xF:
                    sb.AppendLine("\tData put by READ commands should not be evicted if there is data cached by other means that can be evicted");
                    break;
                default:
                    sb.AppendFormat("\tUnknown demand read retention priority value {0}", page.DemandReadRetentionPrio).AppendLine();
                    break;
            }

            switch (page.WriteRetentionPriority)
            {
                case 0:
                    sb.AppendLine("\tDrive does not distinguish between cached write data");
                    break;
                case 1:
                    sb.AppendLine("\tData put by WRITE commands should be evicted from cache sooner than data put in write cache by other means");
                    break;
                case 0xF:
                    sb.AppendLine("\tData put by WRITE commands should not be evicted if there is data cached by other means that can be evicted");
                    break;
                default:
                    sb.AppendFormat("\tUnknown demand write retention priority value {0}", page.DemandReadRetentionPrio).AppendLine();
                    break;
            }

            if (page.DRA)
                sb.AppendLine("\tRead-ahead is disabled");
            else
            {
                if (page.MF)
                    sb.AppendLine("\tPre-fetch values indicate a block multiplier");

                if (page.DisablePreFetch == 0)
                    sb.AppendLine("\tNo pre-fetch will be done");
                else
                {
                    sb.AppendFormat("\tPre-fetch will be done for READ commands of {0} blocks or less", page.DisablePreFetch).AppendLine();

                    if (page.MinimumPreFetch > 0)
                        sb.AppendFormat("At least {0} blocks will be always pre-fetched", page.MinimumPreFetch).AppendLine();
                    if (page.MaximumPreFetch > 0)
                        sb.AppendFormat("\tA maximum of {0} blocks will be pre-fetched", page.MaximumPreFetch).AppendLine();
                    if (page.MaximumPreFetchCeiling > 0)
                        sb.AppendFormat("\tA maximum of {0} blocks will be pre-fetched even if it is commanded to pre-fetch more", page.MaximumPreFetchCeiling).AppendLine();

                    if (page.IC)
                        sb.AppendLine("\tDevice should use number of cache segments or cache segment size for caching");
                    if (page.ABPF)
                        sb.AppendLine("\tPre-fetch should be aborted upong receiving a new command");
                    if (page.CAP)
                        sb.AppendLine("\tCaching analysis is permitted");
                    if (page.Disc)
                        sb.AppendLine("\tPre-fetch can continue across discontinuities (such as cylinders or tracks)");
                }
            }

            if (page.FSW)
                sb.AppendLine("\tDrive should not reorder the sequence of write commands to be faster");

            if (page.Size)
            {
                if (page.CacheSegmentSize > 0)
                {
                    if (page.LBCSS)
                        sb.AppendFormat("\tDrive cache segments should be {0} blocks long", page.CacheSegmentSize).AppendLine();
                    else
                        sb.AppendFormat("\tDrive cache segments should be {0} bytes long", page.CacheSegmentSize).AppendLine();
                }
            }
            else
            {
                if (page.CacheSegments > 0)
                    sb.AppendFormat("\tDrive should have {0} cache segments", page.CacheSegments).AppendLine();
            }

            if (page.NonCacheSegmentSize > 0)
                sb.AppendFormat("\tDrive shall allocate {0} bytes to buffer even when all cached data cannot be evicted", page.NonCacheSegmentSize).AppendLine();

            if (page.NV_DIS)
                sb.AppendLine("\tNon-Volatile cache is disabled");

            return sb.ToString();
        }
        #endregion Mode Page 0x08: Caching page

        #region Mode Page 0x05: Flexible disk page
        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x05
        /// 32 bytes in SCSI-2, SBC-1
        /// </summary>
        public struct ModePage_05
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Data rate of peripheral device on kbit/s
            /// </summary>
            public ushort TransferRate;
            /// <summary>
            /// Heads for reading and/or writing
            /// </summary>
            public byte Heads;
            /// <summary>
            /// Sectors per revolution per head
            /// </summary>
            public byte SectorsPerTrack;
            /// <summary>
            /// Bytes of data per sector
            /// </summary>
            public ushort BytesPerSector;
            /// <summary>
            /// Cylinders used for data storage
            /// </summary>
            public ushort Cylinders;
            /// <summary>
            /// Cylinder where write precompensation starts
            /// </summary>
            public ushort WritePrecompCylinder;
            /// <summary>
            /// Cylinder where write current reduction starts
            /// </summary>
            public ushort WriteReduceCylinder;
            /// <summary>
            /// Step rate in 100 μs units
            /// </summary>
            public ushort DriveStepRate;
            /// <summary>
            /// Width of step pulse in μs
            /// </summary>
            public byte DriveStepPulse;
            /// <summary>
            /// Head settle time in 100 μs units
            /// </summary>
            public ushort HeadSettleDelay;
            /// <summary>
            /// If <see cref="TRDY"/> is <c>true</c>, specified in 1/10s of a
            /// second the time waiting for read status before aborting medium
            /// access. Otherwise, indicates time to way before medimum access
            /// after motor on signal is asserted.
            /// </summary>
            public byte MotorOnDelay;
            /// <summary>
            /// Time in 1/10s of a second to wait before releasing the motor on
            /// signal after an idle condition. 0xFF means to never release the
            /// signal
            /// </summary>
            public byte MotorOffDelay;
            /// <summary>
            /// Specifies if a signal indicates that the medium is ready to be accessed
            /// </summary>
            public bool TRDY;
            /// <summary>
            /// If <c>true</c> sectors start with one. Otherwise, they start with zero.
            /// </summary>
            public bool SSN;
            /// <summary>
            /// If <c>true</c> specifies that motor on shall remain released.
            /// </summary>
            public bool MO;
            /// <summary>
            /// Number of additional step pulses per cylinder.
            /// </summary>
            public byte SPC;
            /// <summary>
            /// Write compensation value
            /// </summary>
            public byte WriteCompensation;
            /// <summary>
            /// Head loading time in ms.
            /// </summary>
            public byte HeadLoadDelay;
            /// <summary>
            /// Head unloading time in ms.
            /// </summary>
            public byte HeadUnloadDelay;
            /// <summary>
            /// Description of shugart's bus pin 34 usage
            /// </summary>
            public byte Pin34;
            /// <summary>
            /// Description of shugart's bus pin 2 usage
            /// </summary>
            public byte Pin2;
            /// <summary>
            /// Description of shugart's bus pin 4 usage
            /// </summary>
            public byte Pin4;
            /// <summary>
            /// Description of shugart's bus pin 1 usage
            /// </summary>
            public byte Pin1;
            /// <summary>
            /// Medium speed in rpm
            /// </summary>
            public ushort MediumRotationRate;
        }

        public static ModePage_05? DecodeModePage_05(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x05)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 32)
                return null;

            ModePage_05 decoded = new ModePage_05();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.TransferRate = (ushort)((pageResponse[2] << 8) + pageResponse[3]);
            decoded.Heads = pageResponse[4];
            decoded.SectorsPerTrack = pageResponse[5];
            decoded.BytesPerSector = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.Cylinders = (ushort)((pageResponse[8] << 8) + pageResponse[9]);
            decoded.WritePrecompCylinder = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
            decoded.WriteReduceCylinder = (ushort)((pageResponse[12] << 8) + pageResponse[13]);
            decoded.DriveStepRate = (ushort)((pageResponse[14] << 8) + pageResponse[15]);
            decoded.DriveStepPulse = pageResponse[16];
            decoded.HeadSettleDelay = (ushort)((pageResponse[17] << 8) + pageResponse[18]);
            decoded.MotorOnDelay = pageResponse[19];
            decoded.MotorOffDelay = pageResponse[20];
            decoded.TRDY |= (pageResponse[21] & 0x80) == 0x80;
            decoded.SSN |= (pageResponse[21] & 0x40) == 0x40;
            decoded.MO |= (pageResponse[21] & 0x20) == 0x20;
            decoded.SPC = (byte)(pageResponse[22] & 0x0F);
            decoded.WriteCompensation = pageResponse[23];
            decoded.HeadLoadDelay = pageResponse[24];
            decoded.HeadUnloadDelay = pageResponse[25];
            decoded.Pin34 = (byte)((pageResponse[26] & 0xF0) >> 4);
            decoded.Pin2 = (byte)(pageResponse[26] & 0x0F);
            decoded.Pin4 = (byte)((pageResponse[27] & 0xF0) >> 4);
            decoded.Pin1 = (byte)(pageResponse[27] & 0x0F);
            decoded.MediumRotationRate = (ushort)((pageResponse[28] << 8) + pageResponse[29]);

            return decoded;
        }

        public static string PrettifyModePage_05(byte[] pageResponse)
        {
            return PrettifyModePage_05(DecodeModePage_05(pageResponse));
        }

        public static string PrettifyModePage_05(ModePage_05? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_05 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Flexible disk page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendFormat("\tTransfer rate: {0} kbit/s", page.TransferRate).AppendLine();
            sb.AppendFormat("\t{0} heads", page.Heads).AppendLine();
            sb.AppendFormat("\t{0} cylinders", page.Cylinders).AppendLine();
            sb.AppendFormat("\t{0} sectors per track", page.SectorsPerTrack).AppendLine();
            sb.AppendFormat("\t{0} bytes per sector", page.BytesPerSector).AppendLine();
            if(page.WritePrecompCylinder < page.Cylinders)
                sb.AppendFormat("\tWrite pre-compensation starts at cylinder {0}", page.WritePrecompCylinder).AppendLine();
            if(page.WriteReduceCylinder < page.Cylinders)
                sb.AppendFormat("\tWrite current reduction starts at cylinder {0}", page.WriteReduceCylinder).AppendLine();
            if (page.DriveStepRate > 0)
                sb.AppendFormat("\tDrive steps in {0} μs", (uint)page.DriveStepRate * 100).AppendLine();
            if (page.DriveStepPulse > 0)
                sb.AppendFormat("\tEach step pulse is {0} ms", page.DriveStepPulse).AppendLine();
            if (page.HeadSettleDelay > 0)
                sb.AppendFormat("\tHeads settles in {0} μs", (uint)page.HeadSettleDelay * 100).AppendLine();

            if(!page.TRDY)
                sb.AppendFormat("\tTarget shall wait {0} seconds before attempting to access the medium after motor on is asserted",
                    (double)page.MotorOnDelay * 10).AppendLine();
            else
                sb.AppendFormat("\tTarget shall wait {0} seconds after drive is ready before aborting medium access attemps",
                    (double)page.MotorOnDelay * 10).AppendLine();

            if (page.MotorOffDelay != 0xFF)
                sb.AppendFormat("\tTarget shall wait {0} seconds before releasing the motor on signal after becoming idle",
                    (double)page.MotorOffDelay * 10).AppendLine();
            else
                sb.AppendLine("\tTarget shall never release the motor on signal");

            if (page.TRDY)
                sb.AppendLine("\tThere is a drive ready signal");
            if (page.SSN)
                sb.AppendLine("\tSectors start at 1");
            if (page.MO)
                sb.AppendLine("\tThe motor on signal shall remain released");

            sb.AppendFormat("\tDrive needs to do {0} step pulses per cylinder", page.SPC + 1).AppendLine();

            if (page.WriteCompensation > 0)
                sb.AppendFormat("\tWrite pre-compensation is {0}", page.WriteCompensation).AppendLine();
            if (page.HeadLoadDelay > 0)
                sb.AppendFormat("\tHead takes {0} ms to load", page.HeadLoadDelay).AppendLine();
            if (page.HeadUnloadDelay > 0)
                sb.AppendFormat("\tHead takes {0} ms to unload", page.HeadUnloadDelay).AppendLine();

            if (page.MediumRotationRate > 0)
                sb.AppendFormat("\tMedium rotates at {0} rpm", page.MediumRotationRate).AppendLine();

            switch (page.Pin34 & 0x07)
            {
                case 0:
                    sb.AppendLine("\tPin 34 is unconnected");
                    break;
                case 1:
                    sb.Append("\tPin 34 indicates drive is ready when active ");
                    if ((page.Pin34 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
                case 2:
                    sb.Append("\tPin 34 indicates disk has changed when active ");
                    if ((page.Pin34 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
                default:
                    sb.AppendFormat("\tPin 34 indicates unknown function {0} when active ", page.Pin34 & 0x07);
                    if ((page.Pin34 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
            }

            switch (page.Pin4 & 0x07)
            {
                case 0:
                    sb.AppendLine("\tPin 4 is unconnected");
                    break;
                case 1:
                    sb.Append("\tPin 4 indicates drive is in use when active ");
                    if ((page.Pin4 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
                case 2:
                    sb.Append("\tPin 4 indicates eject when active ");
                    if ((page.Pin4 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
                case 3:
                    sb.Append("\tPin 4 indicates head load when active ");
                    if ((page.Pin4 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
                default:
                    sb.AppendFormat("\tPin 4 indicates unknown function {0} when active ", page.Pin4 & 0x07);
                    if ((page.Pin4 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
            }

            switch (page.Pin2 & 0x07)
            {
                case 0:
                    sb.AppendLine("\tPin 2 is unconnected");
                    break;
                default:
                    sb.AppendFormat("\tPin 2 indicates unknown function {0} when active ", page.Pin2 & 0x07);
                    if ((page.Pin2 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
            }

            switch (page.Pin1 & 0x07)
            {
                case 0:
                    sb.AppendLine("\tPin 1 is unconnected");
                    break;
                case 1:
                    sb.Append("\tPin 1 indicates disk change reset when active ");
                    if ((page.Pin1 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
                default:
                    sb.AppendFormat("\tPin 1 indicates unknown function {0} when active ", page.Pin1 & 0x07);
                    if ((page.Pin1 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
            }

            return sb.ToString();
        }
        #endregion Mode Page 0x05: Flexible disk page

        #region Mode Page 0x03: Format device page
        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x03
        /// 24 bytes in SCSI-2, SBC-1
        /// </summary>
        public struct ModePage_03
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Tracks per zone to use in dividing the capacity for the purpose of allocating alternate sectors
            /// </summary>
            public ushort TracksPerZone;
            /// <summary>
            /// Number of sectors per zone that shall be reserved for defect handling
            /// </summary>
            public ushort AltSectorsPerZone;
            /// <summary>
            /// Number of tracks per zone that shall be reserved for defect handling
            /// </summary>
            public ushort AltTracksPerZone;
            /// <summary>
            /// Number of tracks per LUN that shall be reserved for defect handling
            /// </summary>
            public ushort AltTracksPerLun;
            /// <summary>
            /// Number of physical sectors per track
            /// </summary>
            public ushort SectorsPerTrack;
            /// <summary>
            /// Bytes per physical sector
            /// </summary>
            public ushort BytesPerSector;
            /// <summary>
            /// Interleave value, target dependent
            /// </summary>
            public ushort Interleave;
            /// <summary>
            /// Sectors between last block of one track and first block of the next
            /// </summary>
            public ushort TrackSkew;
            /// <summary>
            /// Sectors between last block of a cylinder and first block of the next one
            /// </summary>
            public ushort CylinderSkew;
            /// <summary>
            /// Soft-sectored
            /// </summary>
            public bool SSEC;
            /// <summary>
            /// Hard-sectored
            /// </summary>
            public bool HSEC;
            /// <summary>
            /// Removable
            /// </summary>
            public bool RMB;
            /// <summary>
            /// If set, address are allocated progressively in a surface before going to the next.
            /// Otherwise, it goes by cylinders
            /// </summary>
            public bool SURF;
        }

        public static ModePage_03? DecodeModePage_03(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x03)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 24)
                return null;

            ModePage_03 decoded = new ModePage_03();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.TracksPerZone = (ushort)((pageResponse[2] << 8) + pageResponse[3]);
            decoded.AltSectorsPerZone = (ushort)((pageResponse[4] << 8) + pageResponse[5]);
            decoded.AltTracksPerZone = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.AltTracksPerLun = (ushort)((pageResponse[8] << 8) + pageResponse[9]);
            decoded.SectorsPerTrack = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
            decoded.BytesPerSector = (ushort)((pageResponse[12] << 8) + pageResponse[13]);
            decoded.Interleave = (ushort)((pageResponse[14] << 8) + pageResponse[15]);
            decoded.TrackSkew = (ushort)((pageResponse[16] << 8) + pageResponse[17]);
            decoded.CylinderSkew = (ushort)((pageResponse[18] << 8) + pageResponse[19]);
            decoded.SSEC |= (pageResponse[20] & 0x80) == 0x80;
            decoded.HSEC |= (pageResponse[20] & 0x40) == 0x40;
            decoded.RMB |= (pageResponse[20] & 0x20) == 0x20;
            decoded.SURF |= (pageResponse[20] & 0x10) == 0x10;

            return decoded;
        }

        public static string PrettifyModePage_03(byte[] pageResponse)
        {
            return PrettifyModePage_03(DecodeModePage_03(pageResponse));
        }

        public static string PrettifyModePage_03(ModePage_03? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_03 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Format device page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendFormat("\t{0} tracks per zone to use in dividing the capacity for the purpose of allocating alternate sectors", page.TracksPerZone).AppendLine();
            sb.AppendFormat("\t{0} sectors per zone that shall be reserved for defect handling", page.AltSectorsPerZone).AppendLine();
            sb.AppendFormat("\t{0} tracks per zone that shall be reserved for defect handling", page.AltTracksPerZone).AppendLine();
            sb.AppendFormat("\t{0} tracks per LUN that shall be reserved for defect handling", page.AltTracksPerLun).AppendLine();
            sb.AppendFormat("\t{0} physical sectors per track", page.SectorsPerTrack).AppendLine();
            sb.AppendFormat("\t{0} Bytes per physical sector", page.BytesPerSector).AppendLine();
            sb.AppendFormat("\tTarget-dependent interleave value is {0}", page.Interleave).AppendLine();
            sb.AppendFormat("\t{0} sectors between last block of one track and first block of the next", page.TrackSkew).AppendLine();
            sb.AppendFormat("\t{0} sectors between last block of a cylinder and first block of the next one", page.CylinderSkew).AppendLine();
            if (page.SSEC)
                sb.AppendLine("\tDrive supports soft-sectoring format");
            if (page.HSEC)
                sb.AppendLine("\tDrive supports hard-sectoring format");
            if (page.RMB)
                sb.AppendLine("\tDrive media is removable");
            if (page.SURF)
                sb.AppendLine("\tSector addressing is progressively incremented in one surface before going to the next");
            else
                sb.AppendLine("\tSector addressing is progressively incremented in one cylinder before going to the next");

            return sb.ToString();
        }
        #endregion Mode Page 0x03: Format device page

        #region Mode Page 0x0B: Medium types supported page
        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x0B
        /// 8 bytes in SCSI-2
        /// </summary>
        public struct ModePage_0B
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            public byte MediumType1;
            public byte MediumType2;
            public byte MediumType3;
            public byte MediumType4;
        }

        public static ModePage_0B? DecodeModePage_0B(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x0B)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 8)
                return null;

            ModePage_0B decoded = new ModePage_0B();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.MediumType1 = pageResponse[4];
            decoded.MediumType2 = pageResponse[5];
            decoded.MediumType3 = pageResponse[6];
            decoded.MediumType4 = pageResponse[7];

            return decoded;
        }

        public static string PrettifyModePage_0B(byte[] pageResponse)
        {
            return PrettifyModePage_0B(DecodeModePage_0B(pageResponse));
        }

        public static string PrettifyModePage_0B(ModePage_0B? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_0B page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Medium types supported page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            // TODO: Implement it when all known medium types are supported
            sb.AppendLine("Not yet implemented");

            return sb.ToString();
        }
        #endregion Mode Page 0x0B: Medium types supported page

        #region Mode Page 0x0C: Notch page
        // TODO: Implement this page
        #endregion Mode Page 0x0C: Notch page

        #region Mode Page 0x01: Read-write error recovery page
        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x01
        /// 12 bytes in SCSI-2, SBC-1, SBC-2
        /// </summary>
        public struct ModePage_01
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Automatic Write Reallocation Enabled
            /// </summary>
            public bool AWRE;
            /// <summary>
            /// Automatic Read Reallocation Enabled
            /// </summary>
            public bool ARRE;
            /// <summary>
            /// Transfer block
            /// </summary>
            public bool TB;
            /// <summary>
            /// Read continuous
            /// </summary>
            public bool RC;
            /// <summary>
            /// Enable early recovery
            /// </summary>
            public bool EER;
            /// <summary>
            /// Post error reporting
            /// </summary>
            public bool PER;
            /// <summary>
            /// Disable transfer on error
            /// </summary>
            public bool DTE;
            /// <summary>
            /// Disable correction
            /// </summary>
            public bool DCR;
            /// <summary>
            /// How many times to retry a read operation
            /// </summary>
            public byte ReadRetryCount;
            /// <summary>
            /// How many bits of largest data burst error is maximum to apply error correction on it
            /// </summary>
            public byte CorrectionSpan;
            /// <summary>
            /// Offset to move the heads
            /// </summary>
            public sbyte HeadOffsetCount;
            /// <summary>
            /// Incremental position to which the recovered data strobe shall be adjusted
            /// </summary>
            public sbyte DataStrobeOffsetCount;
            /// <summary>
            /// How many times to retry a write operation
            /// </summary>
            public byte WriteRetryCount;
            /// <summary>
            /// Maximum time in ms to use in data error recovery procedures
            /// </summary>
            public ushort RecoveryTimeLimit;

            /// <summary>
            /// Logical block provisioning error reporting is enabled
            /// </summary>
            public bool LBPERE;
        }

        public static ModePage_01? DecodeModePage_01(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x01)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 8)
                return null;

            ModePage_01 decoded = new ModePage_01();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.AWRE |= (pageResponse[2] & 0x80) == 0x80;
            decoded.ARRE |= (pageResponse[2] & 0x40) == 0x40;
            decoded.TB |= (pageResponse[2] & 0x20) == 0x20;
            decoded.RC |= (pageResponse[2] & 0x10) == 0x10;
            decoded.EER |= (pageResponse[2] & 0x08) == 0x08;
            decoded.PER |= (pageResponse[2] & 0x04) == 0x04;
            decoded.DTE |= (pageResponse[2] & 0x02) == 0x02;
            decoded.DCR |= (pageResponse[2] & 0x01) == 0x01;

            decoded.ReadRetryCount = pageResponse[3];
            decoded.CorrectionSpan = pageResponse[4];
            decoded.HeadOffsetCount = (sbyte)pageResponse[5];
            decoded.DataStrobeOffsetCount = (sbyte)pageResponse[6];

            if (pageResponse.Length < 12)
                return decoded;

            decoded.WriteRetryCount = pageResponse[8];
            decoded.RecoveryTimeLimit = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
            decoded.LBPERE |= (pageResponse[7] & 0x80) == 0x80;

            return decoded;
        }

        public static string PrettifyModePage_01(byte[] pageResponse)
        {
            return PrettifyModePage_01(DecodeModePage_01(pageResponse));
        }

        public static string PrettifyModePage_01(ModePage_01? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_01 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Read-write error recovery page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            if (page.AWRE)
                sb.AppendLine("\tAutomatic write reallocation is enabled");
            if (page.ARRE)
                sb.AppendLine("\tAutomatic read reallocation is enabled");
            if (page.TB)
                sb.AppendLine("\tData not recovered within limits shall be transferred back before a CHECK CONDITION");
            if (page.RC)
                sb.AppendLine("\tDrive will transfer the entire requested length without delaying to perform error recovery");
            if (page.EER)
                sb.AppendLine("\tDrive will use the most expedient form of error recovery first");
            if (page.PER)
                sb.AppendLine("\tDrive shall report recovered errors");
            if (page.DTE)
                sb.AppendLine("\tTransfer will be terminated upon error detection");
            if (page.DCR)
                sb.AppendLine("\tError correction is disabled");
            if (page.ReadRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat read operations {0} times", page.ReadRetryCount).AppendLine();
            if (page.WriteRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat write operations {0} times", page.WriteRetryCount).AppendLine();
            if (page.RecoveryTimeLimit > 0)
                sb.AppendFormat("\tDrive will employ a maximum of {0} ms to recover data", page.RecoveryTimeLimit).AppendLine();
            if (page.LBPERE)
                sb.AppendLine("Logical block provisioning error reporting is enabled");

            return sb.ToString();
        }
        #endregion Mode Page 0x01: Read-write error recovery page

        #region Mode Page 0x04: Rigid disk drive geometry page
        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x04
        /// 24 bytes in SCSI-2, SBC-1
        /// </summary>
        public struct ModePage_04
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Cylinders used for data storage
            /// </summary>
            public uint Cylinders;
            /// <summary>
            /// Heads for reading and/or writing
            /// </summary>
            public byte Heads;
            /// <summary>
            /// Cylinder where write precompensation starts
            /// </summary>
            public uint WritePrecompCylinder;
            /// <summary>
            /// Cylinder where write current reduction starts
            /// </summary>
            public uint WriteReduceCylinder;
            /// <summary>
            /// Step rate in 100 ns units
            /// </summary>
            public ushort DriveStepRate;
            /// <summary>
            /// Cylinder where the heads park
            /// </summary>
            public int LandingCylinder;
            /// <summary>
            /// Rotational position locking
            /// </summary>
            public byte RPL;
            /// <summary>
            /// Rotational skew to apply when synchronized
            /// </summary>
            public byte RotationalOffset;
            /// <summary>
            /// Medium speed in rpm
            /// </summary>
            public ushort MediumRotationRate;
        }

        public static ModePage_04? DecodeModePage_04(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x04)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 24)
                return null;

            ModePage_04 decoded = new ModePage_04();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.Cylinders = (uint)((pageResponse[2] << 16) + (pageResponse[3] << 8) + pageResponse[4]);
            decoded.Heads = pageResponse[5];
            decoded.WritePrecompCylinder = (uint)((pageResponse[6] << 16) + (pageResponse[7] << 8) + pageResponse[8]);
            decoded.WriteReduceCylinder = (uint)((pageResponse[9] << 16) + (pageResponse[10] << 8) + pageResponse[11]);
            decoded.DriveStepRate = (ushort)((pageResponse[12] << 8) + pageResponse[13]);
            decoded.LandingCylinder = ((pageResponse[14] << 16) + (pageResponse[15] << 8) + pageResponse[16]);
            decoded.RPL = (byte)(pageResponse[17] & 0x03);
            decoded.RotationalOffset = pageResponse[18];
            decoded.MediumRotationRate = (ushort)((pageResponse[20] << 8) + pageResponse[21]);

            return decoded;
        }

        public static string PrettifyModePage_04(byte[] pageResponse)
        {
            return PrettifyModePage_04(DecodeModePage_04(pageResponse));
        }

        public static string PrettifyModePage_04(ModePage_04? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_04 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Rigid disk drive geometry page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendFormat("\t{0} heads", page.Heads).AppendLine();
            sb.AppendFormat("\t{0} cylinders", page.Cylinders).AppendLine();
            if(page.WritePrecompCylinder < page.Cylinders)
                sb.AppendFormat("\tWrite pre-compensation starts at cylinder {0}", page.WritePrecompCylinder).AppendLine();
            if(page.WriteReduceCylinder < page.Cylinders)
                sb.AppendFormat("\tWrite current reduction starts at cylinder {0}", page.WriteReduceCylinder).AppendLine();
            if (page.DriveStepRate > 0)
                sb.AppendFormat("\tDrive steps in {0} ns", (uint)page.DriveStepRate * 100).AppendLine();

            sb.AppendFormat("\tHeads park in cylinder {0}", page.LandingCylinder).AppendLine();

            if (page.MediumRotationRate > 0)
                sb.AppendFormat("\tMedium rotates at {0} rpm", page.MediumRotationRate).AppendLine();

            switch (page.RPL)
            {
                case 0:
                    sb.AppendLine("\tSpindle synchronization is disable or unsupported");
                    break;
                case 1:
                    sb.AppendLine("\tTarget operates as a synchronized-spindle slave");
                    break;
                case 2:
                    sb.AppendLine("\tTarget operates as a synchronized-spindle master");
                    break;
                case 3:
                    sb.AppendLine("\tTarget operates as a synchronized-spindle master control");
                    break;
            }

            return sb.ToString();
        }
        #endregion Mode Page 0x04: Rigid disk drive geometry page

        #region Mode Page 0x07: Verify error recovery page
        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x07
        /// 12 bytes in SCSI-2, SBC-1, SBC-2
        /// </summary>
        public struct ModePage_07
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Enable early recovery
            /// </summary>
            public bool EER;
            /// <summary>
            /// Post error reporting
            /// </summary>
            public bool PER;
            /// <summary>
            /// Disable transfer on error
            /// </summary>
            public bool DTE;
            /// <summary>
            /// Disable correction
            /// </summary>
            public bool DCR;
            /// <summary>
            /// How many times to retry a verify operation
            /// </summary>
            public byte VerifyRetryCount;
            /// <summary>
            /// How many bits of largest data burst error is maximum to apply error correction on it
            /// </summary>
            public byte CorrectionSpan;
            /// <summary>
            /// Maximum time in ms to use in data error recovery procedures
            /// </summary>
            public ushort RecoveryTimeLimit;
        }

        public static ModePage_07? DecodeModePage_07(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x07)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 12)
                return null;

            ModePage_07 decoded = new ModePage_07();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.EER |= (pageResponse[2] & 0x08) == 0x08;
            decoded.PER |= (pageResponse[2] & 0x04) == 0x04;
            decoded.DTE |= (pageResponse[2] & 0x02) == 0x02;
            decoded.DCR |= (pageResponse[2] & 0x01) == 0x01;

            decoded.VerifyRetryCount = pageResponse[3];
            decoded.CorrectionSpan = pageResponse[4];
            decoded.RecoveryTimeLimit = (ushort)((pageResponse[10] << 8) + pageResponse[11]);

            return decoded;
        }

        public static string PrettifyModePage_07(byte[] pageResponse)
        {
            return PrettifyModePage_07(DecodeModePage_07(pageResponse));
        }

        public static string PrettifyModePage_07(ModePage_07? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_07 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Verify error recovery page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            if (page.EER)
                sb.AppendLine("\tDrive will use the most expedient form of error recovery first");
            if (page.PER)
                sb.AppendLine("\tDrive shall report recovered errors");
            if (page.DTE)
                sb.AppendLine("\tTransfer will be terminated upon error detection");
            if (page.DCR)
                sb.AppendLine("\tError correction is disabled");
            if (page.VerifyRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat verify operations {0} times", page.VerifyRetryCount).AppendLine();
            if (page.RecoveryTimeLimit > 0)
                sb.AppendFormat("\tDrive will employ a maximum of {0} ms to recover data", page.RecoveryTimeLimit).AppendLine();

            return sb.ToString();
        }
        #endregion Mode Page 0x07: Verify error recovery page

        #region Mode Page 0x10: Device configuration page
        /// <summary>
        /// Device configuration page
        /// Page code 0x10
        /// 16 bytes in SCSI-2, SSC-1, SSC-2, SSC-3
        /// </summary>
        public struct ModePage_10_SSC
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Used in mode select to change partition to one specified in <see cref="ActivePartition"/> 
            /// </summary>
            public bool CAP;
            /// <summary>
            /// Used in mode select to change format to one specified in <see cref="ActiveFormat"/> 
            /// </summary>
            public bool CAF;
            /// <summary>
            /// Active format, vendor-specific
            /// </summary>
            public byte ActiveFormat;
            /// <summary>
            /// Current logical partition
            /// </summary>
            public byte ActivePartition;
            /// <summary>
            /// How full the buffer shall be before writing to medium
            /// </summary>
            public byte WriteBufferFullRatio;
            /// <summary>
            /// How empty the buffer shall be before reading more data from the medium
            /// </summary>
            public byte ReadBufferEmptyRatio;
            /// <summary>
            /// Delay in 100 ms before buffered data is forcefully written to the medium even before buffer is full
            /// </summary>
            public ushort WriteDelayTime;
            /// <summary>
            /// Drive supports recovering data from buffer
            /// </summary>
            public bool DBR;
            /// <summary>
            /// Medium has block IDs
            /// </summary>
            public bool BIS;
            /// <summary>
            /// Drive recognizes and reports setmarks
            /// </summary>
            public bool RSmk;
            /// <summary>
            /// Drive selects best speed
            /// </summary>
            public bool AVC;
            /// <summary>
            /// If drive should stop pre-reading on filemarks
            /// </summary>
            public byte SOCF;
            /// <summary>
            /// If set, recovered buffer data is LIFO, otherwise, FIFO
            /// </summary>
            public bool RBO;
            /// <summary>
            /// Report early warnings
            /// </summary>
            public bool REW;
            /// <summary>
            /// Inter-block gap
            /// </summary>
            public byte GapSize;
            /// <summary>
            /// End-of-Data format
            /// </summary>
            public byte EODDefined;
            /// <summary>
            /// EOD generation enabled
            /// </summary>
            public bool EEG;
            /// <summary>
            /// Synchronize data to medium on early warning
            /// </summary>
            public bool SEW;
            /// <summary>
            /// Bytes to reduce buffer size on early warning
            /// </summary>
            public uint BufferSizeEarlyWarning;
            /// <summary>
            /// Selected data compression algorithm
            /// </summary>
            public byte SelectedCompression;

            /// <summary>
            /// Soft write protect
            /// </summary>
            public bool SWP;
            /// <summary>
            /// Associated write protect
            /// </summary>
            public bool ASOCWP;
            /// <summary>
            /// Persistent write protect
            /// </summary>
            public bool PERSWP;
            /// <summary>
            /// Permanent write protect
            /// </summary>
            public bool PRMWP;

            public bool BAML;
            public bool BAM;
            public byte RewindOnReset;

            /// <summary>
            /// How drive shall respond to detection of compromised WORM medium integrity
            /// </summary>
            public byte WTRE;
            /// <summary>
            /// Respond to commands only if a reservation exists
            /// </summary>
            public bool OIR;
        }

        public static ModePage_10_SSC? DecodeModePage_10_SSC(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x10)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 16)
                return null;

            ModePage_10_SSC decoded = new ModePage_10_SSC();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.CAP |= (pageResponse[2] & 0x40) == 0x40;
            decoded.CAF |= (pageResponse[2] & 0x20) == 0x20;
            decoded.ActiveFormat = (byte)(pageResponse[2] & 0x1F);
            decoded.ActivePartition = pageResponse[3];
            decoded.WriteBufferFullRatio = pageResponse[4];
            decoded.ReadBufferEmptyRatio = pageResponse[5];
            decoded.WriteDelayTime = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.DBR |= (pageResponse[8] & 0x80) == 0x80;
            decoded.BIS |= (pageResponse[8] & 0x40) == 0x40;
            decoded.RSmk |= (pageResponse[8] & 0x20) == 0x20;
            decoded.AVC |= (pageResponse[8] & 0x10) == 0x10;
            decoded.RBO |= (pageResponse[8] & 0x02) == 0x02;
            decoded.REW |= (pageResponse[8] & 0x01) == 0x01;
            decoded.EEG |= (pageResponse[10] & 0x10) == 0x10;
            decoded.SEW |= (pageResponse[10] & 0x08) == 0x08;
            decoded.SOCF = (byte)((pageResponse[8] & 0x0C) >> 2);
            decoded.BufferSizeEarlyWarning = (uint)((pageResponse[11] << 16) + (pageResponse[12] << 8) + pageResponse[13]);
            decoded.SelectedCompression = pageResponse[14];

            decoded.SWP |= (pageResponse[10] & 0x04) == 0x04;
            decoded.ASOCWP |= (pageResponse[15] & 0x04) == 0x04;
            decoded.PERSWP |= (pageResponse[15] & 0x02) == 0x02;
            decoded.PRMWP |= (pageResponse[15] & 0x01) == 0x01;

            decoded.BAML |= (pageResponse[10] & 0x02) == 0x02;
            decoded.BAM |= (pageResponse[10] & 0x01) == 0x01;

            decoded.RewindOnReset = (byte)((pageResponse[15] & 0x18) >> 3);

            decoded.OIR |= (pageResponse[15] & 0x20) == 0x20;
            decoded.WTRE = (byte)((pageResponse[15] & 0xC0) >> 6);

            return decoded;
        }

        public static string PrettifyModePage_10_SSC(byte[] pageResponse)
        {
            return PrettifyModePage_10_SSC(DecodeModePage_10_SSC(pageResponse));
        }

        public static string PrettifyModePage_10_SSC(ModePage_10_SSC? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_10_SSC page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Device configuration page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");
            
            sb.AppendFormat("\tActive format: {0}", page.ActiveFormat).AppendLine();
            sb.AppendFormat("\tActive partition: {0}", page.ActivePartition).AppendLine();
            sb.AppendFormat("\tWrite buffer shall have a full ratio of {0} before being flushed to medium", page.WriteBufferFullRatio).AppendLine();
            sb.AppendFormat("\tRead buffer shall have an empty ratio of {0} before more data is read from medium", page.ReadBufferEmptyRatio).AppendLine();
            sb.AppendFormat("\tDrive will delay {0} ms before buffered data is forcefully written to the medium even before buffer is full", (int)page.WriteDelayTime * 100).AppendLine();
            if (page.DBR)
            {
                sb.AppendLine("\tDrive supports recovering data from buffer");
                if (page.RBO)
                    sb.AppendLine("\tRecovered buffer data comes in LIFO order");
                else
                    sb.AppendLine("\tRecovered buffer data comes in FIFO order");
            }
            if (page.BIS)
                sb.AppendLine("\tMedium supports block IDs");
            if (page.RSmk)
                sb.AppendLine("\tDrive reports setmarks");
            switch (page.SOCF)
            {
                case 0:
                    sb.AppendLine("\tDrive will pre-read until buffer is full");
                    break;
                case 1:
                    sb.AppendLine("\tDrive will pre-read until one filemark is detected");
                    break;
                case 2:
                    sb.AppendLine("\tDrive will pre-read until two filemark is detected");
                    break;
                case 3:
                    sb.AppendLine("\tDrive will pre-read until three filemark is detected");
                    break;
            }

            if (page.REW)
            {
                sb.AppendLine("\tDrive reports early warnings");
                if (page.SEW)
                    sb.AppendLine("\tDrive will synchronize buffer to medium on early warnings");
            }

            switch (page.GapSize)
            {
                case 0:
                    break;
                case 1:
                    sb.AppendLine("\tInter-block gap is long enough to support update in place");
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                    sb.AppendFormat("\tInter-block gap is {0} times the device's defined gap size", page.GapSize).AppendLine();
                    break;
                default:
                    sb.AppendFormat("\tInter-block gap is unknown value {0}", page.GapSize).AppendLine();
                    break;
            }

            if (page.EEG)
                sb.AppendLine("\tDrive generates end-of-data");

            switch (page.SelectedCompression)
            {
                case 0:
                    sb.AppendLine("\tDrive does not use compression");
                    break;
                case 1:
                    sb.AppendLine("\tDrive uses default compression");
                    break;
                default:
                    sb.AppendFormat("\tDrive uses unknown compression {0}", page.SelectedCompression).AppendLine();
                    break;
            }

            if (page.SWP)
                sb.AppendLine("\tSoftware write protect is enabled");
            if (page.ASOCWP)
                sb.AppendLine("\tAssociated write protect is enabled");
            if (page.PERSWP)
                sb.AppendLine("\tPersistent write protect is enabled");
            if (page.PRMWP)
                sb.AppendLine("\tPermanent write protect is enabled");

            if(page.BAML)
            {
                if (page.BAM)
                    sb.AppendLine("\tDrive operates using explicit address mode");
                else
                    sb.AppendLine("\tDrive operates using implicit address mode");
            }

            switch (page.RewindOnReset)
            {
                case 1:
                    sb.AppendLine("\tDrive shall position to beginning of default data partition on reset");
                    break;
                case 2:
                    sb.AppendLine("\tDrive shall maintain its position on reset");
                    break;
            }

            switch (page.WTRE)
            {
                case 1:
                    sb.AppendLine("\tDrive will do nothing on WORM tampered medium");
                    break;
                case 2:
                    sb.AppendLine("\tDrive will return CHECK CONDITION on WORM tampered medium");
                    break;
            }

            if (page.OIR)
                sb.AppendLine("\tDrive will only respond to commands if it has received a reservation");

            return sb.ToString();
        }
        #endregion Mode Page 0x10: Device configuration page

        #region Mode Page 0x0E: CD-ROM audio control parameters page
        /// <summary>
        /// CD-ROM audio control parameters
        /// Page code 0x0E
        /// 16 bytes in SCSI-2, MMC-1, MMC-2, MMC-3
        /// </summary>
        public struct ModePage_0E
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Return status as soon as playback operation starts
            /// </summary>
            public bool Immed;
            /// <summary>
            /// Stop on track crossing
            /// </summary>
            public bool SOTC;
            /// <summary>
            /// Indicates <see cref="BlocksPerSecondOfAudio"/> is valid
            /// </summary>
            public bool APRVal;
            /// <summary>
            /// Multiplier for <see cref="BlocksPerSecondOfAudio"/>
            /// </summary>
            public byte LBAFormat;
            /// <summary>
            /// LBAs per second of audio
            /// </summary>
            public ushort BlocksPerSecondOfAudio;
            /// <summary>
            /// Channels output on this port
            /// </summary>
            public byte OutputPort0ChannelSelection;
            /// <summary>
            /// Volume level for this port
            /// </summary>
            public byte OutputPort0Volume;
            /// <summary>
            /// Channels output on this port
            /// </summary>
            public byte OutputPort1ChannelSelection;
            /// <summary>
            /// Volume level for this port
            /// </summary>
            public byte OutputPort1Volume;
            /// <summary>
            /// Channels output on this port
            /// </summary>
            public byte OutputPort2ChannelSelection;
            /// <summary>
            /// Volume level for this port
            /// </summary>
            public byte OutputPort2Volume;
            /// <summary>
            /// Channels output on this port
            /// </summary>
            public byte OutputPort3ChannelSelection;
            /// <summary>
            /// Volume level for this port
            /// </summary>
            public byte OutputPort3Volume;
        }

        public static ModePage_0E? DecodeModePage_0E(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x0E)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 16)
                return null;

            ModePage_0E decoded = new ModePage_0E();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.Immed |= (pageResponse[2] & 0x04) == 0x04;
            decoded.SOTC |= (pageResponse[2] & 0x02) == 0x02;
            decoded.APRVal |= (pageResponse[5] & 0x80) == 0x80;
            decoded.LBAFormat = (byte)(pageResponse[5] & 0x0F);
            decoded.BlocksPerSecondOfAudio = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.OutputPort0ChannelSelection = (byte)(pageResponse[8] & 0x0F);
            decoded.OutputPort0Volume = pageResponse[9];
            decoded.OutputPort1ChannelSelection = (byte)(pageResponse[10] & 0x0F);
            decoded.OutputPort1Volume = pageResponse[11];
            decoded.OutputPort2ChannelSelection = (byte)(pageResponse[12] & 0x0F);
            decoded.OutputPort2Volume = pageResponse[13];
            decoded.OutputPort3ChannelSelection = (byte)(pageResponse[14] & 0x0F);
            decoded.OutputPort3Volume = pageResponse[15];

            return decoded;
        }

        public static string PrettifyModePage_0E(byte[] pageResponse)
        {
            return PrettifyModePage_0E(DecodeModePage_0E(pageResponse));
        }

        public static string PrettifyModePage_0E(ModePage_0E? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_0E page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI CD-ROM audio control parameters page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");
            if (page.Immed)
                sb.AppendLine("\tDrive will return from playback command immediately");
            else
                sb.AppendLine("\tDrive will return from playback command when playback ends");
            if (page.SOTC)
                sb.AppendLine("\tDrive will stop playback on track end");

            if (page.APRVal)
            {
                double blocks;
                if (page.LBAFormat == 8)
                    blocks = page.BlocksPerSecondOfAudio * (1 / 256);
                else
                    blocks = page.BlocksPerSecondOfAudio;

                sb.AppendFormat("\tThere are {0} blocks per each second of audio", blocks).AppendLine();
            }

            if (page.OutputPort0ChannelSelection > 0)
            {
                sb.Append("Output port 0 has channels ");
                if ((page.OutputPort0ChannelSelection & 0x01) == 0x01)
                    sb.Append("0 ");
                if ((page.OutputPort0ChannelSelection & 0x02) == 0x02)
                    sb.Append("1 ");
                if ((page.OutputPort0ChannelSelection & 0x04) == 0x04)
                    sb.Append("2 ");
                if ((page.OutputPort0ChannelSelection & 0x08) == 0x08)
                    sb.Append("3 ");

                switch (page.OutputPort0Volume)
                {
                    case 0:
                        sb.AppendLine("muted");
                        break;
                    case 0xFF:
                        sb.AppendLine("at maximum volume");
                        break;
                    default:
                        sb.AppendFormat("at volume {0}", page.OutputPort0Volume).AppendLine();
                        break;
                }
            }

            if (page.OutputPort1ChannelSelection > 0)
            {
                sb.Append("Output port 1 has channels ");
                if ((page.OutputPort1ChannelSelection & 0x01) == 0x01)
                    sb.Append("0 ");
                if ((page.OutputPort1ChannelSelection & 0x02) == 0x02)
                    sb.Append("1 ");
                if ((page.OutputPort1ChannelSelection & 0x04) == 0x04)
                    sb.Append("2 ");
                if ((page.OutputPort1ChannelSelection & 0x08) == 0x08)
                    sb.Append("3 ");

                switch (page.OutputPort1Volume)
                {
                    case 0:
                        sb.AppendLine("muted");
                        break;
                    case 0xFF:
                        sb.AppendLine("at maximum volume");
                        break;
                    default:
                        sb.AppendFormat("at volume {0}", page.OutputPort1Volume).AppendLine();
                        break;
                }
            }

            if (page.OutputPort2ChannelSelection > 0)
            {
                sb.Append("Output port 2 has channels ");
                if ((page.OutputPort2ChannelSelection & 0x01) == 0x01)
                    sb.Append("0 ");
                if ((page.OutputPort2ChannelSelection & 0x02) == 0x02)
                    sb.Append("1 ");
                if ((page.OutputPort2ChannelSelection & 0x04) == 0x04)
                    sb.Append("2 ");
                if ((page.OutputPort2ChannelSelection & 0x08) == 0x08)
                    sb.Append("3 ");

                switch (page.OutputPort2Volume)
                {
                    case 0:
                        sb.AppendLine("muted");
                        break;
                    case 0xFF:
                        sb.AppendLine("at maximum volume");
                        break;
                    default:
                        sb.AppendFormat("at volume {0}", page.OutputPort2Volume).AppendLine();
                        break;
                }
            }

            if (page.OutputPort3ChannelSelection > 0)
            {
                sb.Append("Output port 3 has channels ");
                if ((page.OutputPort3ChannelSelection & 0x01) == 0x01)
                    sb.Append("0 ");
                if ((page.OutputPort3ChannelSelection & 0x02) == 0x02)
                    sb.Append("1 ");
                if ((page.OutputPort3ChannelSelection & 0x04) == 0x04)
                    sb.Append("2 ");
                if ((page.OutputPort3ChannelSelection & 0x08) == 0x08)
                    sb.Append("3 ");

                switch (page.OutputPort3Volume)
                {
                    case 0:
                        sb.AppendLine("muted");
                        break;
                    case 0xFF:
                        sb.AppendLine("at maximum volume");
                        break;
                    default:
                        sb.AppendFormat("at volume {0}", page.OutputPort3Volume).AppendLine();
                        break;
                }
            }

            return sb.ToString();
        }
        #endregion Mode Page 0x0E: CD-ROM audio control parameters page

        #region Mode Page 0x0D: CD-ROM parameteres page
        /// <summary>
        /// CD-ROM parameteres page
        /// Page code 0x0D
        /// 8 bytes in SCSI-2, MMC-1, MMC-2, MMC-3
        /// </summary>
        public struct ModePage_0D
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Time the drive shall remain in hold track state after seek or read
            /// </summary>
            public byte InactivityTimerMultiplier;
            /// <summary>
            /// Seconds per Minute
            /// </summary>
            public ushort SecondsPerMinute;
            /// <summary>
            /// Frames per Second
            /// </summary>
            public ushort FramesPerSecond;
        }

        public static ModePage_0D? DecodeModePage_0D(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x0D)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 8)
                return null;

            ModePage_0D decoded = new ModePage_0D();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.InactivityTimerMultiplier = (byte)(pageResponse[3] & 0xF);
            decoded.SecondsPerMinute = (ushort)((pageResponse[4] << 8) + pageResponse[5]);
            decoded.FramesPerSecond = (ushort)((pageResponse[6] << 8) + pageResponse[7]);

            return decoded;
        }

        public static string PrettifyModePage_0D(byte[] pageResponse)
        {
            return PrettifyModePage_0D(DecodeModePage_0D(pageResponse));
        }

        public static string PrettifyModePage_0D(ModePage_0D? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_0D page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI CD-ROM parameters page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");
            switch (page.InactivityTimerMultiplier)
            {
                case 0:
                    sb.AppendLine("Drive will remain in track hold state a vendor-specified time after a seek or read");
                    break;
                case 1:
                    sb.AppendLine("Drive will remain in track hold state 125 ms after a seek or read");
                    break;
                case 2:
                    sb.AppendLine("Drive will remain in track hold state 250 ms after a seek or read");
                    break;
                case 3:
                    sb.AppendLine("Drive will remain in track hold state 500 ms after a seek or read");
                    break;
                case 4:
                    sb.AppendLine("Drive will remain in track hold state 1 second after a seek or read");
                    break;
                case 5:
                    sb.AppendLine("Drive will remain in track hold state 2 seconds after a seek or read");
                    break;
                case 6:
                    sb.AppendLine("Drive will remain in track hold state 4 seconds after a seek or read");
                    break;
                case 7:
                    sb.AppendLine("Drive will remain in track hold state 8 seconds after a seek or read");
                    break;
                case 8:
                    sb.AppendLine("Drive will remain in track hold state 16 seconds after a seek or read");
                    break;
                case 9:
                    sb.AppendLine("Drive will remain in track hold state 32 seconds after a seek or read");
                    break;
                case 10:
                    sb.AppendLine("Drive will remain in track hold state 1 minute after a seek or read");
                    break;
                case 11:
                    sb.AppendLine("Drive will remain in track hold state 2 minutes after a seek or read");
                    break;
                case 12:
                    sb.AppendLine("Drive will remain in track hold state 4 minutes after a seek or read");
                    break;
                case 13:
                    sb.AppendLine("Drive will remain in track hold state 8 minutes after a seek or read");
                    break;
                case 14:
                    sb.AppendLine("Drive will remain in track hold state 16 minutes after a seek or read");
                    break;
                case 15:
                    sb.AppendLine("Drive will remain in track hold state 32 minutes after a seek or read");
                    break;
            }

            if (page.SecondsPerMinute > 0)
                sb.AppendFormat("Each minute has {0} seconds", page.SecondsPerMinute).AppendLine();
            if (page.FramesPerSecond > 0)
                sb.AppendFormat("Each second has {0} frames", page.FramesPerSecond).AppendLine();

            return sb.ToString();
        }
        #endregion Mode Page 0x0D: CD-ROM parameteres page

        #region Mode Page 0x01: Read error recovery page for MultiMedia Devices
        /// <summary>
        /// Read error recovery page for MultiMedia Devices
        /// Page code 0x01
        /// 8 bytes in SCSI-2, MMC-1
        /// 12 bytes in MMC-2, MMC-3
        /// </summary>
        public struct ModePage_01_MMC
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Error recovery parameter
            /// </summary>
            public byte Parameter;
            /// <summary>
            /// How many times to retry a read operation
            /// </summary>
            public byte ReadRetryCount;
            /// <summary>
            /// How many times to retry a write operation
            /// </summary>
            public byte WriteRetryCount;
            /// <summary>
            /// Maximum time in ms to use in data error recovery procedures
            /// </summary>
            public ushort RecoveryTimeLimit;
        }

        public static ModePage_01_MMC? DecodeModePage_01_MMC(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x01)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 8)
                return null;

            ModePage_01_MMC decoded = new ModePage_01_MMC();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.Parameter = pageResponse[2];
            decoded.ReadRetryCount = pageResponse[3];

            if (pageResponse.Length < 12)
                return decoded;

            decoded.WriteRetryCount = pageResponse[8];
            decoded.RecoveryTimeLimit = (ushort)((pageResponse[10] << 8) + pageResponse[11]);

            return decoded;
        }

        public static string PrettifyModePage_01_MMC(byte[] pageResponse)
        {
            return PrettifyModePage_01_MMC(DecodeModePage_01_MMC(pageResponse));
        }

        public static string PrettifyModePage_01_MMC(ModePage_01_MMC? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_01_MMC page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Read error recovery page for MultiMedia Devices:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");
            if (page.ReadRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat read operations {0} times", page.ReadRetryCount).AppendLine();
            
            string AllUsed = "\tAll available recovery procedures will be used.\n";
            string CIRCRetriesUsed = "\tOnly retries and CIRC are used.\n";
            string RetriesUsed = "\tOnly retries are used.\n";
            string RecoveredNotReported = "Recovered errors will not be reported.\n";
            string RecoveredReported = "Recovered errors will be reported.\n";
            string RecoveredAbort = "Recovered errors will be reported and aborted with CHECK CONDITION.\n";
            string UnrecECCAbort = "Unrecovered ECC errors will return CHECK CONDITION.";
            string UnrecCIRCAbort = "Unrecovered CIRC errors will return CHECK CONDITION.";
            string UnrecECCNotAbort = "Unrecovered ECC errors will not abort the transfer.";
            string UnrecCIRCNotAbort = "Unrecovered CIRC errors will not abort the transfer.";
            string UnrecECCAbortData = "Unrecovered ECC errors will return CHECK CONDITION and the uncorrected data.";
            string UnrecCIRCAbortData = "Unrecovered CIRC errors will return CHECK CONDITION and the uncorrected data.";

            switch (page.Parameter)
            {
                case 0x00:
                    sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCAbort);
                    break;
                case 0x01:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCAbort);
                    break;
                case 0x04:
                    sb.AppendLine(AllUsed + RecoveredReported + UnrecECCAbort);
                    break;
                case 0x05:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCAbort);
                    break;
                case 0x06:
                    sb.AppendLine(AllUsed + RecoveredAbort + UnrecECCAbort);
                    break;
                case 0x07:
                    sb.AppendLine(RetriesUsed + RecoveredAbort + UnrecCIRCAbort);
                    break;
                case 0x10:
                    sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCNotAbort);
                    break;
                case 0x11:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCNotAbort);
                    break;
                case 0x14:
                    sb.AppendLine(AllUsed + RecoveredReported + UnrecECCNotAbort);
                    break;
                case 0x15:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCNotAbort);
                    break;
                case 0x20:
                    sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCAbortData);
                    break;
                case 0x21:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCAbortData);
                    break;
                case 0x24:
                    sb.AppendLine(AllUsed + RecoveredReported + UnrecECCAbortData);
                    break;
                case 0x25:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCAbortData);
                    break;
                case 0x26:
                    sb.AppendLine(AllUsed + RecoveredAbort + UnrecECCAbortData);
                    break;
                case 0x27:
                    sb.AppendLine(RetriesUsed + RecoveredAbort + UnrecCIRCAbortData);
                    break;
                case 0x30:
                    goto case 0x10;
                case 0x31:
                    goto case 0x11;
                case 0x34:
                    goto case 0x14;
                case 0x35:
                    goto case 0x15;
                default:
                    sb.AppendFormat("Unknown recovery parameter 0x{0:X2}", page.Parameter).AppendLine();
                    break;
            }

            if (page.WriteRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat write operations {0} times", page.WriteRetryCount).AppendLine();
            if (page.RecoveryTimeLimit > 0)
                sb.AppendFormat("\tDrive will employ a maximum of {0} ms to recover data", page.RecoveryTimeLimit).AppendLine();
            
            return sb.ToString();
        }
        #endregion Mode Page 0x01: Read error recovery page for MultiMedia Devices

        #region Mode Page 0x07: Verify error recovery page for MultiMedia Devices
        /// <summary>
        /// Verify error recovery page for MultiMedia Devices
        /// Page code 0x07
        /// 8 bytes in SCSI-2, MMC-1
        /// </summary>
        public struct ModePage_07_MMC
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Error recovery parameter
            /// </summary>
            public byte Parameter;
            /// <summary>
            /// How many times to retry a verify operation
            /// </summary>
            public byte VerifyRetryCount;
        }

        public static ModePage_07_MMC? DecodeModePage_07_MMC(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x07)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 8)
                return null;

            ModePage_07_MMC decoded = new ModePage_07_MMC();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.Parameter = pageResponse[2];
            decoded.VerifyRetryCount = pageResponse[3];

            return decoded;
        }

        public static string PrettifyModePage_07_MMC(byte[] pageResponse)
        {
            return PrettifyModePage_07_MMC(DecodeModePage_07_MMC(pageResponse));
        }

        public static string PrettifyModePage_07_MMC(ModePage_07_MMC? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_07_MMC page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Verify error recovery page for MultiMedia Devices:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");
            if (page.VerifyRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat verify operations {0} times", page.VerifyRetryCount).AppendLine();

            string AllUsed = "\tAll available recovery procedures will be used.\n";
            string CIRCRetriesUsed = "\tOnly retries and CIRC are used.\n";
            string RetriesUsed = "\tOnly retries are used.\n";
            string RecoveredNotReported = "Recovered errors will not be reported.\n";
            string RecoveredReported = "Recovered errors will be reported.\n";
            string RecoveredAbort = "Recovered errors will be reported and aborted with CHECK CONDITION.\n";
            string UnrecECCAbort = "Unrecovered ECC errors will return CHECK CONDITION.";
            string UnrecCIRCAbort = "Unrecovered CIRC errors will return CHECK CONDITION.";
            string UnrecECCNotAbort = "Unrecovered ECC errors will not abort the transfer.";
            string UnrecCIRCNotAbort = "Unrecovered CIRC errors will not abort the transfer.";
            string UnrecECCAbortData = "Unrecovered ECC errors will return CHECK CONDITION and the uncorrected data.";
            string UnrecCIRCAbortData = "Unrecovered CIRC errors will return CHECK CONDITION and the uncorrected data.";

            switch (page.Parameter)
            {
                case 0x00:
                    sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCAbort);
                    break;
                case 0x01:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCAbort);
                    break;
                case 0x04:
                    sb.AppendLine(AllUsed + RecoveredReported + UnrecECCAbort);
                    break;
                case 0x05:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCAbort);
                    break;
                case 0x06:
                    sb.AppendLine(AllUsed + RecoveredAbort + UnrecECCAbort);
                    break;
                case 0x07:
                    sb.AppendLine(RetriesUsed + RecoveredAbort + UnrecCIRCAbort);
                    break;
                case 0x10:
                    sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCNotAbort);
                    break;
                case 0x11:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCNotAbort);
                    break;
                case 0x14:
                    sb.AppendLine(AllUsed + RecoveredReported + UnrecECCNotAbort);
                    break;
                case 0x15:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCNotAbort);
                    break;
                case 0x20:
                    sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCAbortData);
                    break;
                case 0x21:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCAbortData);
                    break;
                case 0x24:
                    sb.AppendLine(AllUsed + RecoveredReported + UnrecECCAbortData);
                    break;
                case 0x25:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCAbortData);
                    break;
                case 0x26:
                    sb.AppendLine(AllUsed + RecoveredAbort + UnrecECCAbortData);
                    break;
                case 0x27:
                    sb.AppendLine(RetriesUsed + RecoveredAbort + UnrecCIRCAbortData);
                    break;
                case 0x30:
                    goto case 0x10;
                case 0x31:
                    goto case 0x11;
                case 0x34:
                    goto case 0x14;
                case 0x35:
                    goto case 0x15;
                default:
                    sb.AppendFormat("Unknown recovery parameter 0x{0:X2}", page.Parameter).AppendLine();
                    break;
            }

            return sb.ToString();
        }
        #endregion Mode Page 0x07: Verify error recovery page for MultiMedia Devices

        #region Mode Page 0x06: Optical memory page
        /// <summary>
        /// Optical memory page
        /// Page code 0x06
        /// 4 bytes in SCSI-2
        /// </summary>
        public struct ModePage_06
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Report updated block read
            /// </summary>
            public bool RUBR;
        }

        public static ModePage_06? DecodeModePage_06(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x06)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 4)
                return null;

            ModePage_06 decoded = new ModePage_06();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.RUBR |= (pageResponse[2] & 0x01) == 0x01;

            return decoded;
        }

        public static string PrettifyModePage_06(byte[] pageResponse)
        {
            return PrettifyModePage_06(DecodeModePage_06(pageResponse));
        }

        public static string PrettifyModePage_06(ModePage_06? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_06 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI optical memory:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");
            if (page.RUBR)
                sb.AppendLine("\tOn reading an updated block drive will return RECOVERED ERROR");

            return sb.ToString();
        }
        #endregion Mode Page 0x06: Optical memory page

        #region Mode Page 0x2A: CD-ROM capabilities page
        /// <summary>
        /// CD-ROM capabilities page
        /// Page code 0x2A
        /// 16 bytes in OB-U0077C
        /// 20 bytes in SFF-8020i
        /// 22 bytes in MMC-1
        /// 26 bytes in MMC-2
        /// Variable bytes in MMC-3
        /// </summary>
        public struct ModePage_2A
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Drive supports multi-session and/or Photo-CD
            /// </summary>
            public bool MultiSession;
            /// <summary>
            /// Drive is capable of reading sectors in Mode 2 Form 2 format
            /// </summary>
            public bool Mode2Form2;
            /// <summary>
            /// Drive is capable of reading sectors in Mode 2 Form 1 format
            /// </summary>
            public bool Mode2Form1;
            /// <summary>
            /// Drive is capable of playing audio
            /// </summary>
            public bool AudioPlay;
            /// <summary>
            /// Drive can return the ISRC
            /// </summary>
            public bool ISRC;
            /// <summary>
            /// Drive can return the media catalogue number
            /// </summary>
            public bool UPC;
            /// <summary>
            /// Drive can return C2 pointers
            /// </summary>
            public bool C2Pointer;
            /// <summary>
            /// Drive can read, deinterlave and correct R-W subchannels
            /// </summary>
            public bool DeinterlaveSubchannel;
            /// <summary>
            /// Drive can read interleaved and uncorrected R-W subchannels
            /// </summary>
            public bool Subchannel;
            /// <summary>
            /// Drive can continue from a loss of streaming on audio reading
            /// </summary>
            public bool AccurateCDDA;
            /// <summary>
            /// Audio can be read as digital data
            /// </summary>
            public bool CDDACommand;
            /// <summary>
            /// Loading Mechanism Type
            /// </summary>
            public byte LoadingMechanism;
            /// <summary>
            /// Drive can eject discs
            /// </summary>
            public bool Eject;
            /// <summary>
            /// Drive's optional prevent jumper status
            /// </summary>
            public bool PreventJumper;
            /// <summary>
            /// Current lock status
            /// </summary>
            public bool LockState;
            /// <summary>
            /// Drive can lock media
            /// </summary>
            public bool Lock;
            /// <summary>
            /// Each channel can be muted independently
            /// </summary>
            public bool SeparateChannelMute;
            /// <summary>
            /// Each channel's volume can be controlled independently
            /// </summary>
            public bool SeparateChannelVolume;
            /// <summary>
            /// Maximum drive speed in Kbytes/second
            /// </summary>
            public ushort MaximumSpeed;
            /// <summary>
            /// Supported volume levels
            /// </summary>
            public ushort SupportedVolumeLevels;
            /// <summary>
            /// Buffer size in Kbytes
            /// </summary>
            public ushort BufferSize;
            /// <summary>
            /// Current drive speed in Kbytes/second
            /// </summary>
            public ushort CurrentSpeed;

            public bool Method2;
            public bool ReadCDRW;
            public bool ReadCDR;
            public bool WriteCDRW;
            public bool WriteCDR;
            public bool DigitalPort2;
            public bool DigitalPort1;
            public bool Composite;
            public bool SSS;
            public bool SDP;
            public byte Length;
            public bool LSBF;
            public bool RCK;
            public bool BCK;

            public bool TestWrite;
            public ushort MaxWriteSpeed;
            public ushort CurrentWriteSpeed;

            public bool ReadBarcode;

            public bool ReadDVDRAM;
            public bool ReadDVDR;
            public bool ReadDVDROM;
            public bool WriteDVDRAM;
            public bool WriteDVDR;
            public bool LeadInPW;
            public bool SCC;
            public ushort CMRSupported;

            public bool BUF;
            public byte RotationControlSelected;
            public ushort CurrentWriteSpeedSelected;
            public ModePage_2A_WriteDescriptor[] WriteSpeedPerformanceDescriptors;
        }

        public struct ModePage_2A_WriteDescriptor
        {
            public byte RotationControl;
            public ushort WriteSpeed;
        }

        public static ModePage_2A? DecodeModePage_2A(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x2A)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 16)
                return null;

            ModePage_2A decoded = new ModePage_2A();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.AudioPlay |= (pageResponse[4] & 0x01) == 0x01;
            decoded.Mode2Form1 |= (pageResponse[4] & 0x10) == 0x10;
            decoded.Mode2Form2 |= (pageResponse[4] & 0x20) == 0x20;
            decoded.MultiSession |= (pageResponse[4] & 0x40) == 0x40;

            decoded.CDDACommand |= (pageResponse[5] & 0x01) == 0x01;
            decoded.AccurateCDDA |= (pageResponse[5] & 0x02) == 0x02;
            decoded.Subchannel |= (pageResponse[5] & 0x04) == 0x04;
            decoded.DeinterlaveSubchannel |= (pageResponse[5] & 0x08) == 0x08;
            decoded.C2Pointer |= (pageResponse[5] & 0x10) == 0x10;
            decoded.UPC |= (pageResponse[5] & 0x20) == 0x20;
            decoded.ISRC |= (pageResponse[5] & 0x40) == 0x40;

            decoded.LoadingMechanism = (byte)((pageResponse[6] & 0xE0) >> 5);
            decoded.Lock |= (pageResponse[6] & 0x01) == 0x01;
            decoded.LockState |= (pageResponse[6] & 0x02) == 0x02;
            decoded.PreventJumper |= (pageResponse[6] & 0x04) == 0x04;
            decoded.Eject |= (pageResponse[6] & 0x08) == 0x08;

            decoded.SeparateChannelVolume |= (pageResponse[7] & 0x01) == 0x01;
            decoded.SeparateChannelMute |= (pageResponse[7] & 0x02) == 0x02;

            decoded.MaximumSpeed = (ushort)((pageResponse[8] << 8) + pageResponse[9]);
            decoded.SupportedVolumeLevels = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
            decoded.BufferSize = (ushort)((pageResponse[12] << 8) + pageResponse[13]);
            decoded.CurrentSpeed = (ushort)((pageResponse[14] << 8) + pageResponse[15]);

            if (pageResponse.Length < 20)
                return decoded;

            decoded.Method2 |= (pageResponse[2] & 0x04) == 0x04;
            decoded.ReadCDRW |= (pageResponse[2] & 0x02) == 0x02;
            decoded.ReadCDR |= (pageResponse[2] & 0x01) == 0x01;

            decoded.WriteCDRW |= (pageResponse[3] & 0x02) == 0x02;
            decoded.WriteCDR |= (pageResponse[3] & 0x01) == 0x01;

            decoded.Composite |= (pageResponse[4] & 0x02) == 0x02;
            decoded.DigitalPort1 |= (pageResponse[4] & 0x04) == 0x04;
            decoded.DigitalPort2 |= (pageResponse[4] & 0x08) == 0x08;

            decoded.SDP |= (pageResponse[7] & 0x04) == 0x04;
            decoded.SSS |= (pageResponse[7] & 0x08) == 0x08;

            decoded.Length = (byte)((pageResponse[17] & 0x30) >> 4);
            decoded.LSBF |= (pageResponse[17] & 0x08) == 0x08;
            decoded.RCK |= (pageResponse[17] & 0x04) == 0x04;
            decoded.BCK |= (pageResponse[17] & 0x02) == 0x02;

            if (pageResponse.Length < 22)
                return decoded;

            decoded.TestWrite |= (pageResponse[3] & 0x04) == 0x04;
            decoded.MaxWriteSpeed = (ushort)((pageResponse[18] << 8) + pageResponse[19]);
            decoded.CurrentWriteSpeed = (ushort)((pageResponse[20] << 8) + pageResponse[21]);

            decoded.ReadBarcode |= (pageResponse[5] & 0x80) == 0x80;

            if (pageResponse.Length < 26)
                return decoded;

            decoded.ReadDVDRAM |= (pageResponse[2] & 0x20) == 0x20;
            decoded.ReadDVDR |= (pageResponse[2] & 0x10) == 0x10;
            decoded.ReadDVDROM |= (pageResponse[2] & 0x08) == 0x08;

            decoded.WriteDVDRAM |= (pageResponse[3] & 0x20) == 0x20;
            decoded.WriteDVDR |= (pageResponse[3] & 0x10) == 0x10;

            decoded.LeadInPW |= (pageResponse[3] & 0x20) == 0x20;
            decoded.SCC |= (pageResponse[3] & 0x10) == 0x10;

            decoded.CMRSupported = (ushort)((pageResponse[22] << 8) + pageResponse[23]);

            if (pageResponse.Length < 32)
                return decoded;

            decoded.BUF |= (pageResponse[4] & 0x80) == 0x80;
            decoded.RotationControlSelected = (byte)(pageResponse[27] & 0x03);
            decoded.CurrentWriteSpeedSelected = (ushort)((pageResponse[28] << 8) + pageResponse[29]);

            ushort descriptors = (ushort)((pageResponse[30] << 8) + pageResponse[31]);
            decoded.WriteSpeedPerformanceDescriptors = new ModePage_2A_WriteDescriptor[descriptors];

            for (int i = 0; i < descriptors; i++)
            {
                decoded.WriteSpeedPerformanceDescriptors[i] = new ModePage_2A_WriteDescriptor();
                decoded.WriteSpeedPerformanceDescriptors[i].RotationControl = (byte)(pageResponse[1 + 32 + i * 4] & 0x07);
                decoded.WriteSpeedPerformanceDescriptors[i].WriteSpeed = (ushort)((pageResponse[2 + 32 + i * 4] << 8) + pageResponse[3 + 32 + i * 4]);
            }

            return decoded;
        }

        public static string PrettifyModePage_2A(byte[] pageResponse)
        {
            return PrettifyModePage_2A(DecodeModePage_2A(pageResponse));
        }

        public static string PrettifyModePage_2A(ModePage_2A? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_2A page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI CD-ROM capabilities page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            if (page.AudioPlay)
                sb.AppendLine("\tDrive can play audio");
            if (page.Mode2Form1)
                sb.AppendLine("\tDrive can read sectors in Mode 2 Form 1 format");
            if (page.Mode2Form2)
                sb.AppendLine("\tDrive can read sectors in Mode 2 Form 2 format");
            if (page.MultiSession)
                sb.AppendLine("\tDrive supports multi-session discs and/or Photo-CD");

            if (page.CDDACommand)
                sb.AppendLine("\tDrive can read digital audio");
            if (page.AccurateCDDA)
                sb.AppendLine("\tDrive can continue from streaming loss");
            if (page.Subchannel)
                sb.AppendLine("\tDrive can read uncorrected and interleaved R-W subchannels");
            if (page.DeinterlaveSubchannel)
                sb.AppendLine("\tDrive can read, deinterleave and correct R-W subchannels");
            if (page.C2Pointer)
                sb.AppendLine("\tDrive supports C2 pointers");
            if (page.UPC)
                sb.AppendLine("\tDrive can read Media Catalogue Number");
            if (page.ISRC)
                sb.AppendLine("\tDrive can read ISRC");

            switch (page.LoadingMechanism)
            {
                case 0:
                    sb.AppendLine("\tDrive uses media caddy");
                    break;
                case 1:
                    sb.AppendLine("\tDrive uses a tray");
                    break;
                case 2:
                    sb.AppendLine("\tDrive is pop-up");
                    break;
                case 4:
                    sb.AppendLine("\tDrive is a changer with individually changeable discs");
                    break;
                case 5:
                    sb.AppendLine("\tDrive is a changer using cartridges");
                    break;
                default:
                    sb.AppendFormat("\tDrive uses unknown loading mechanism type {0}", page.LoadingMechanism).AppendLine();
                    break;
            }

            if (page.Lock)
                sb.AppendLine("\tDrive can lock media");
            if (page.PreventJumper)
            {
                sb.AppendLine("\tDrive power ups locked");
                if (page.LockState)
                    sb.AppendLine("\tDrive is locked, media cannot be ejected or inserted");
                else
                    sb.AppendLine("\tDrive is not locked, media can be ejected and inserted");
            }
            else
            {
                if (page.LockState)
                    sb.AppendLine("\tDrive is locked, media cannot be ejected, but if empty, can be inserted");
                else
                    sb.AppendLine("\tDrive is not locked, media can be ejected and inserted");
            }
            if (page.Eject)
                sb.AppendLine("\tDrive can eject media");

            if (page.SeparateChannelMute)
                sb.AppendLine("\tEach channel can be muted independently");
            if (page.SeparateChannelVolume)
                sb.AppendLine("\tEach channel's volume can be controlled independently");

            if (page.SupportedVolumeLevels > 0)
                sb.AppendFormat("\tDrive supports {0} volume levels", page.SupportedVolumeLevels).AppendLine();
            if(page.BufferSize > 0)
                sb.AppendFormat("\tDrive has {0} Kbyte of buffer", page.BufferSize).AppendLine();
            if (page.MaximumSpeed > 0)
                sb.AppendFormat("\tDrive's maximum reading speed is {0} Kbyte/sec.", page.MaximumSpeed).AppendLine();
            if (page.CurrentSpeed > 0)
                sb.AppendFormat("\tDrive's current reading speed is {0} Kbyte/sec.", page.CurrentSpeed).AppendLine();

            if (page.ReadCDR)
            {
                if (page.WriteCDR)
                    sb.AppendLine("\tDrive can read and write CD-R");
                else
                    sb.AppendLine("\tDrive can read CD-R");

                if (page.Method2)
                    sb.AppendLine("\tDrive supports reading CD-R packet media");
            }

            if (page.ReadCDRW)
            {
                if (page.WriteCDRW)
                    sb.AppendLine("\tDrive can read and write CD-RW");
                else
                    sb.AppendLine("\tDrive can read CD-RW");
            }

            if (page.ReadDVDROM)
                sb.AppendLine("\tDrive can read DVD-ROM");
            if (page.ReadDVDR)
            {
                if (page.WriteDVDR)
                    sb.AppendLine("\tDrive can read and write DVD-R");
                else
                    sb.AppendLine("\tDrive can read DVD-R");
            }
            if (page.ReadDVDRAM)
            {
                if (page.WriteDVDRAM)
                    sb.AppendLine("\tDrive can read and write DVD-RAM");
                else
                    sb.AppendLine("\tDrive can read DVD-RAM");
            }

            if (page.Composite)
                sb.AppendLine("\tDrive can deliver a compositve audio and video data stream");
            if (page.DigitalPort1)
                sb.AppendLine("\tDrive supports IEC-958 digital output on port 1");
            if (page.DigitalPort2)
                sb.AppendLine("\tDrive supports IEC-958 digital output on port 2");

            if (page.SDP)
                sb.AppendLine("\tDrive contains a changer that can report the exact contents of the slots");
            if (page.CurrentWriteSpeedSelected > 0)
            {
                if (page.RotationControlSelected == 0)
                    sb.AppendFormat("\tDrive's current writing speed is {0} Kbyte/sec. in CLV mode", page.CurrentWriteSpeedSelected).AppendLine();
                else if (page.RotationControlSelected == 1)
                    sb.AppendFormat("\tDrive's current writing speed is {0} Kbyte/sec. in pure CAV mode", page.CurrentWriteSpeedSelected).AppendLine();
            }
            else
            {
                if (page.MaxWriteSpeed > 0)
                    sb.AppendFormat("\tDrive's maximum writing speed is {0} Kbyte/sec.", page.MaxWriteSpeed).AppendLine();
                if (page.CurrentWriteSpeed > 0)
                    sb.AppendFormat("\tDrive's current writing speed is {0} Kbyte/sec.", page.CurrentWriteSpeed).AppendLine();
            }

            foreach (ModePage_2A_WriteDescriptor descriptor in page.WriteSpeedPerformanceDescriptors)
            {
                if (descriptor.WriteSpeed > 0)
                {
                    if (descriptor.RotationControl == 0)
                        sb.AppendFormat("\tDrive supports writing at {0} Kbyte/sec. in CLV mode", descriptor.WriteSpeed).AppendLine();
                    else if (descriptor.RotationControl == 1)
                        sb.AppendFormat("\tDrive supports writing at is {0} Kbyte/sec. in pure CAV mode", descriptor.WriteSpeed).AppendLine();
                }
            }

            if (page.TestWrite)
                sb.AppendLine("\tDrive supports test writing");

            if (page.ReadBarcode)
                sb.AppendLine("\tDrive can read barcode");

            if (page.SCC)
                sb.AppendLine("\tDrive can read both sides of a disc");
            if (page.LeadInPW)
                sb.AppendLine("\tDrive an read raw R-W subchannel from the Lead-In");

            if (page.CMRSupported == 1)
                sb.AppendLine("\tDrive supports DVD CSS and/or DVD CPPM");

            if (page.BUF)
                sb.AppendLine("\tDrive supports buffer under-run free recording");

            return sb.ToString();
        }
        #endregion Mode Page 0x2A: CD-ROM capabilities page

        #region Mode Page 0x1C: Informational exceptions control page
        /// <summary>
        /// Informational exceptions control page
        /// Page code 0x1C
        /// 12 bytes in SPC-1, SPC-2, SPC-3, SPC-4
        /// </summary>
        public struct ModePage_1C
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Informational exception operations should not affect performance
            /// </summary>
            public bool Perf;
            /// <summary>
            /// Disable informational exception operations
            /// </summary>
            public bool DExcpt;
            /// <summary>
            /// Create a test device failure at next interval time
            /// </summary>
            public bool Test;
            /// <summary>
            /// Log informational exception conditions
            /// </summary>
            public bool LogErr;
            /// <summary>
            /// Method of reporting informational exceptions
            /// </summary>
            public byte MRIE;
            /// <summary>
            /// 100 ms period to report an informational exception condition
            /// </summary>
            public uint IntervalTimer;
            /// <summary>
            /// How many times to report informational exceptions
            /// </summary>
            public uint ReportCount;

            /// <summary>
            /// Enable background functions
            /// </summary>
            public bool EBF;
            /// <summary>
            /// Warning reporting enabled
            /// </summary>
            public bool EWasc;

            /// <summary>
            /// Enable reporting of background self-test errors
            /// </summary>
            public bool EBACKERR;
        }

        public static ModePage_1C? DecodeModePage_1C(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x1C)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 12)
                return null;

            ModePage_1C decoded = new ModePage_1C();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.Perf |= (pageResponse[2] & 0x80) == 0x80;
            decoded.DExcpt |= (pageResponse[2] & 0x08) == 0x08;
            decoded.Test |= (pageResponse[2] & 0x04) == 0x04;
            decoded.LogErr |= (pageResponse[2] & 0x01) == 0x01;

            decoded.MRIE = (byte)(pageResponse[3] & 0x0F);

            decoded.IntervalTimer = (uint)((pageResponse[4] << 24) + (pageResponse[5] << 16) + (pageResponse[6] << 8) + pageResponse[7]);
            decoded.ReportCount = (uint)((pageResponse[8] << 24) + (pageResponse[9] << 16) + (pageResponse[10] << 8) + pageResponse[11]);

            decoded.EBF |= (pageResponse[2] & 0x20) == 0x20;
            decoded.EWasc |= (pageResponse[2] & 0x10) == 0x10;

            decoded.EBACKERR |= (pageResponse[2] & 0x02) == 0x02;

            return decoded;
        }

        public static string PrettifyModePage_1C(byte[] pageResponse)
        {
            return PrettifyModePage_1C(DecodeModePage_1C(pageResponse));
        }

        public static string PrettifyModePage_1C(ModePage_1C? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_1C page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Informational exceptions control page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            if (page.DExcpt)
                sb.AppendLine("\tInformational exceptions are disabled");
            else
            {
                sb.AppendLine("\tInformational exceptions are enabled");

                switch (page.MRIE)
                {
                    case 0:
                        sb.AppendLine("\tNo reporting of informational exception condition");
                        break;
                    case 1:
                        sb.AppendLine("\tAsynchronous event reporting of informational exceptions");
                        break;
                    case 2:
                        sb.AppendLine("\tGenerate unit attention on informational exceptions");
                        break;
                    case 3:
                        sb.AppendLine("\tConditionally generate recovered error on informational exceptions");
                        break;
                    case 4:
                        sb.AppendLine("\tUnconditionally generate recovered error on informational exceptions");
                        break;
                    case 5:
                        sb.AppendLine("\tGenerate no sense on informational exceptions");
                        break;
                    case 6:
                        sb.AppendLine("\tOnly report informational exception condition on request");
                        break;
                    default:
                        sb.AppendFormat("\tUnknown method of reporting {0}", page.MRIE).AppendLine();
                        break;
                }

                if (page.Perf)
                    sb.AppendLine("\tInformational exceptions reporting should not affect drive performance");
                if (page.Test)
                    sb.AppendLine("\tA test informational exception will raise on next timer");
                if (page.LogErr)
                    sb.AppendLine("\tDrive shall log informational exception conditions");

                if (page.IntervalTimer > 0)
                {
                    if (page.IntervalTimer == 0xFFFFFFFF)
                        sb.AppendLine("\tTimer interval is vendor-specific");
                    else
                        sb.AppendFormat("\tTimer interval is {0} ms", page.IntervalTimer * 100).AppendLine();
                }

                if (page.ReportCount > 0)
                    sb.AppendFormat("\tInformational exception conditions will be reported a maximum of {0} times", page.ReportCount);
            }

            if (page.EWasc)
                sb.AppendLine("\tWarning reporting is enabled");
            if (page.EBF)
                sb.AppendLine("\tBackground functions are enabled");
            if (page.EBACKERR)
                sb.AppendLine("\tDrive will report background self-test errors");

            return sb.ToString();
        }
        #endregion Mode Page 0x1C: Informational exceptions control page

        #region Mode Page 0x1A: Power condition page
        /// <summary>
        /// Power condition page
        /// Page code 0x1A
        /// 12 bytes in SPC-1, SPC-2, SPC-3, SPC-4
        /// 40 bytes in SPC-5
        /// </summary>
        public struct ModePage_1A
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Idle timer activated
            /// </summary>
            public bool Idle;
            /// <summary>
            /// Standby timer activated
            /// </summary>
            public bool Standby;
            /// <summary>
            /// Idle timer
            /// </summary>
            public uint IdleTimer;
            /// <summary>
            /// Standby timer
            /// </summary>
            public uint StandbyTimer;

            /// <summary>
            /// Interactions between background functions and power management
            /// </summary>
            public byte PM_BG_Precedence;
            /// <summary>
            /// Standby timer Y activated
            /// </summary>
            public bool Standby_Y;
            /// <summary>
            /// Idle timer B activated
            /// </summary>
            public bool Idle_B;
            /// <summary>
            /// Idle timer C activated
            /// </summary>
            public bool Idle_C;
            /// <summary>
            /// Idle timer B
            /// </summary>
            public uint IdleTimer_B;
            /// <summary>
            /// Idle timer C
            /// </summary>
            public uint IdleTimer_C;
            /// <summary>
            /// Standby timer Y
            /// </summary>
            public uint StandbyTimer_Y;
            public byte CCF_Idle;
            public byte CCF_Standby;
            public byte CCF_Stopped;
        }

        public static ModePage_1A? DecodeModePage_1A(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x1A)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 12)
                return null;

            ModePage_1A decoded = new ModePage_1A();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.Standby |= (pageResponse[3] & 0x01) == 0x01;
            decoded.Idle |= (pageResponse[3] & 0x02) == 0x02;

            decoded.IdleTimer = (uint)((pageResponse[4] << 24) + (pageResponse[5] << 16) + (pageResponse[6] << 8) + pageResponse[7]);
            decoded.StandbyTimer = (uint)((pageResponse[8] << 24) + (pageResponse[9] << 16) + (pageResponse[10] << 8) + pageResponse[11]);

            if (pageResponse.Length < 40)
                return decoded;

            decoded.PM_BG_Precedence = (byte)((pageResponse[2] & 0xC0) >> 6);
            decoded.Standby_Y |= (pageResponse[2] & 0x01) == 0x01;
            decoded.Idle_B |= (pageResponse[3] & 0x04) == 0x04;
            decoded.Idle_C |= (pageResponse[3] & 0x08) == 0x08;

            decoded.IdleTimer_B = (uint)((pageResponse[12] << 24) + (pageResponse[13] << 16) + (pageResponse[14] << 8) + pageResponse[15]);
            decoded.IdleTimer_C = (uint)((pageResponse[16] << 24) + (pageResponse[17] << 16) + (pageResponse[18] << 8) + pageResponse[19]);
            decoded.StandbyTimer_Y = (uint)((pageResponse[20] << 24) + (pageResponse[21] << 16) + (pageResponse[22] << 8) + pageResponse[23]);

            decoded.CCF_Idle = (byte)((pageResponse[39] & 0xC0) >> 6);
            decoded.CCF_Standby = (byte)((pageResponse[39] & 0x30) >> 4);
            decoded.CCF_Stopped = (byte)((pageResponse[39] & 0x0C) >> 2);

            return decoded;
        }

        public static string PrettifyModePage_1A(byte[] pageResponse)
        {
            return PrettifyModePage_1A(DecodeModePage_1A(pageResponse));
        }

        public static string PrettifyModePage_1A(ModePage_1A? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_1A page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Power condition page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            if ((page.Standby && page.StandbyTimer > 0) ||
               (page.Standby_Y && page.StandbyTimer_Y > 0))
            {
                if(page.Standby && page.StandbyTimer > 0)
                    sb.AppendFormat("\tStandby timer Z is set to {0} ms", page.StandbyTimer * 100).AppendLine();
                if(page.Standby_Y && page.StandbyTimer_Y > 0)
                    sb.AppendFormat("\tStandby timer Y is set to {0} ms", page.StandbyTimer_Y * 100).AppendLine();
            }
            else
                sb.AppendLine("\tDrive will not enter standy mode");

            if ((page.Idle && page.IdleTimer > 0) ||
                (page.Idle_B && page.IdleTimer_B > 0) ||
                (page.Idle_C && page.IdleTimer_C > 0))
            {
                if(page.Idle && page.IdleTimer > 0)
                    sb.AppendFormat("\tIdle timer A is set to {0} ms", page.IdleTimer * 100).AppendLine();
                if(page.Idle_B && page.IdleTimer_B > 0)
                    sb.AppendFormat("\tIdle timer B is set to {0} ms", page.IdleTimer_B * 100).AppendLine();
                if(page.Idle_C && page.IdleTimer_C > 0)
                    sb.AppendFormat("\tIdle timer C is set to {0} ms", page.IdleTimer_C * 100).AppendLine();
            }
            else
                sb.AppendLine("\tDrive will not enter idle mode");

            switch (page.PM_BG_Precedence)
            {
                case 0:
                    break;
                case 1:
                    sb.AppendLine("\tPerforming background functions take precedence over maintaining low power conditions");
                    break;
                case 2:
                    sb.AppendLine("\tMaintaining low power conditions take precedence over performing background functions");
                    break;
            }

            return sb.ToString();
        }
        #endregion Mode Page 0x1A: Power condition page

        #region Mode Page 0x0A subpage 0x01: Control Extension mode page
        /// <summary>
        /// Control Extension mode page
        /// Page code 0x0A
        /// Subpage code 0x01
        /// 32 bytes in SPC-3, SPC-4, SPC-5
        /// </summary>
        public struct ModePage_0A_S01
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Timestamp outside this standard
            /// </summary>
            public bool TCMOS;
            /// <summary>
            /// SCSI precedence
            /// </summary>
            public bool SCSIP;
            /// <summary>
            /// Implicit Asymmetric Logical Unit Access Enabled
            /// </summary>
            public bool IALUAE;
            /// <summary>
            /// Initial task priority
            /// </summary>
            public byte InitialPriority;

            /// <summary>
            /// Device life control disabled
            /// </summary>
            public bool DLC;
            /// <summary>
            /// Maximum size of SENSE data in bytes
            /// </summary>
            public byte MaximumSenseLength;
        }

        public static ModePage_0A_S01? DecodeModePage_0A_S01(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) != 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x0A)
                return null;

            if (pageResponse[1] != 0x01)
                return null;

            if (((pageResponse[2] << 8) + pageResponse[3] + 4) != pageResponse.Length)
                return null;

            if (pageResponse.Length < 32)
                return null;

            ModePage_0A_S01 decoded = new ModePage_0A_S01();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.IALUAE |= (pageResponse[4] & 0x01) == 0x01;
            decoded.SCSIP |= (pageResponse[4] & 0x02) == 0x02;
            decoded.TCMOS |= (pageResponse[4] & 0x04) == 0x04;

            decoded.InitialPriority = (byte)(pageResponse[5] & 0x0F);

            return decoded;
        }

        public static string PrettifyModePage_0A_S01(byte[] pageResponse)
        {
            return PrettifyModePage_0A_S01(DecodeModePage_0A_S01(pageResponse));
        }

        public static string PrettifyModePage_0A_S01(ModePage_0A_S01? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_0A_S01 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Control extension page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");
            
            if(page.TCMOS)
            {
                sb.Append("\tTimestamp can be initialized by methods outside of the SCSI standards");

                if (page.SCSIP)
                    sb.Append(", but SCSI's SET TIMESTAMP shall take precedence over them");

                sb.AppendLine();
            }

            if (page.IALUAE)
                sb.AppendLine("\tImplicit Asymmetric Logical Unit Access is enabled");

            sb.AppendFormat("\tInitial priority is {0}", page.InitialPriority).AppendLine();

            if (page.DLC)
                sb.AppendLine("\tDevice will not degrade performance to extend its life");

            if (page.MaximumSenseLength > 0)
                sb.AppendFormat("\tMaximum sense data would be {0} bytes", page.MaximumSenseLength).AppendLine();

            return sb.ToString();
        }
        #endregion Mode Page 0x0A subpage 0x01: Control Extension mode page

        #region Mode Page 0x1A subpage 0x01: Power Consumption mode page
        /// <summary>
        /// Power Consumption mode page
        /// Page code 0x1A
        /// Subpage code 0x01
        /// 16 bytes in SPC-5
        /// </summary>
        public struct ModePage_1A_S01
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Active power level
            /// </summary>
            public byte ActiveLevel;
            /// <summary>
            /// Power Consumption VPD identifier in use
            /// </summary>
            public byte PowerConsumptionIdentifier;
        }

        public static ModePage_1A_S01? DecodeModePage_1A_S01(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) != 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x1A)
                return null;

            if (pageResponse[1] != 0x01)
                return null;

            if (((pageResponse[2] << 8) + pageResponse[3] + 4) != pageResponse.Length)
                return null;

            if (pageResponse.Length < 16)
                return null;

            ModePage_1A_S01 decoded = new ModePage_1A_S01();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.ActiveLevel = (byte)(pageResponse[6] & 0x03);
            decoded.PowerConsumptionIdentifier = pageResponse[7];

            return decoded;
        }

        public static string PrettifyModePage_1A_S01(byte[] pageResponse)
        {
            return PrettifyModePage_1A_S01(DecodeModePage_1A_S01(pageResponse));
        }

        public static string PrettifyModePage_1A_S01(ModePage_1A_S01? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_1A_S01 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Power Consumption page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            switch(page.ActiveLevel)
            {
                case 0:
                    sb.AppendFormat("\tDevice power consumption is dictated by identifier {0} of Power Consumption VPD", page.PowerConsumptionIdentifier).AppendLine();
                    break;
                case 1:
                    sb.AppendLine("\tDevice is in highest relative power consumption level");
                    break;
                case 2:
                    sb.AppendLine("\tDevice is in intermediate relative power consumption level");
                    break;
                case 3:
                    sb.AppendLine("\tDevice is in lowest relative power consumption level");
                    break;
            }

            return sb.ToString();
        }
        #endregion Mode Page 0x1A subpage 0x01: Power Consumption mode page

        #region Mode Page 0x10: XOR control mode page
        /// <summary>
        /// XOR control mode page
        /// Page code 0x10
        /// 24 bytes in SBC-1, SBC-2
        /// </summary>
        public struct ModePage_10
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Disables XOR operations
            /// </summary>
            public bool XORDIS;
            /// <summary>
            /// Maximum transfer length in blocks for a XOR command
            /// </summary>
            public uint MaxXorWrite;
            /// <summary>
            /// Maximum regenerate length in blocks
            /// </summary>
            public uint MaxRegenSize;
            /// <summary>
            /// Maximum transfer length in blocks for READ during a rebuild
            /// </summary>
            public uint MaxRebuildRead;
            /// <summary>
            /// Minimum time in ms between READs during a rebuild
            /// </summary>
            public ushort RebuildDelay;
        }

        public static ModePage_10? DecodeModePage_10(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x10)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 24)
                return null;

            ModePage_10 decoded = new ModePage_10();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.XORDIS |= (pageResponse[2] & 0x02) == 0x02;
            decoded.MaxXorWrite = (uint)((pageResponse[4] << 24) + (pageResponse[5] << 16) + (pageResponse[6] << 8) + pageResponse[7]);
            decoded.MaxRegenSize = (uint)((pageResponse[12] << 24) + (pageResponse[13] << 16) + (pageResponse[14] << 8) + pageResponse[15]);
            decoded.MaxRebuildRead = (uint)((pageResponse[16] << 24) + (pageResponse[17] << 16) + (pageResponse[18] << 8) + pageResponse[19]);
            decoded.RebuildDelay = (ushort)((pageResponse[22] << 8) + pageResponse[23]);

            return decoded;
        }

        public static string PrettifyModePage_10(byte[] pageResponse)
        {
            return PrettifyModePage_10(DecodeModePage_10(pageResponse));
        }

        public static string PrettifyModePage_10(ModePage_10? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_10 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI XOR control mode page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            if (page.XORDIS)
                sb.AppendLine("\tXOR operations are disabled");
            else
            {
                if (page.MaxXorWrite > 0)
                    sb.AppendFormat("\tDrive accepts a maximum of {0} blocks in a single XOR WRITE command", page.MaxXorWrite).AppendLine();
                if (page.MaxRegenSize > 0)
                    sb.AppendFormat("\tDrive accepts a maximum of {0} blocks in a REGENERATE command", page.MaxRegenSize).AppendLine();
                if (page.MaxRebuildRead > 0)
                    sb.AppendFormat("\tDrive accepts a maximum of {0} blocks in a READ command during rebuild", page.MaxRebuildRead).AppendLine();
                if (page.RebuildDelay > 0)
                    sb.AppendFormat("\tDrive needs a minimum of {0} ms between READ commands during rebuild", page.RebuildDelay).AppendLine();
            }

            return sb.ToString();
        }
        #endregion Mode Page 0x10: XOR control mode page

        #region Mode Page 0x1C subpage 0x01: Background Control mode page
        /// <summary>
        /// Background Control mode page
        /// Page code 0x1A
        /// Subpage code 0x01
        /// 16 bytes in SPC-5
        /// </summary>
        public struct ModePage_1C_S01
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Suspend on log full
            /// </summary>
            public bool S_L_Full;
            /// <summary>
            /// Log only when intervention required
            /// </summary>
            public bool LOWIR;
            /// <summary>
            /// Enable background medium scan
            /// </summary>
            public bool En_Bms;
            /// <summary>
            /// Enable background pre-scan
            /// </summary>
            public bool En_Ps;
            /// <summary>
            /// Time in hours between background medium scans
            /// </summary>
            public ushort BackgroundScanInterval;
            /// <summary>
            /// Maximum time in hours for a background pre-scan to complete
            /// </summary>
            public ushort BackgroundPrescanTimeLimit;
            /// <summary>
            /// Minimum time in ms being idle before resuming a background scan
            /// </summary>
            public ushort MinIdleBeforeBgScan;
            /// <summary>
            /// Maximum time in ms to start processing commands while performing a background scan
            /// </summary>
            public ushort MaxTimeSuspendBgScan;
        }

        public static ModePage_1C_S01? DecodeModePage_1C_S01(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) != 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x1C)
                return null;

            if (pageResponse[1] != 0x01)
                return null;

            if (((pageResponse[2] << 8) + pageResponse[3] + 4) != pageResponse.Length)
                return null;

            if (pageResponse.Length < 16)
                return null;

            ModePage_1C_S01 decoded = new ModePage_1C_S01();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.S_L_Full |= (pageResponse[4] & 0x04) == 0x04;
            decoded.LOWIR |= (pageResponse[4] & 0x02) == 0x02;
            decoded.En_Bms |= (pageResponse[4] & 0x01) == 0x01;
            decoded.En_Ps |= (pageResponse[5] & 0x01) == 0x01;

            decoded.BackgroundScanInterval = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.BackgroundPrescanTimeLimit = (ushort)((pageResponse[8] << 8) + pageResponse[9]);
            decoded.MinIdleBeforeBgScan = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
            decoded.MaxTimeSuspendBgScan = (ushort)((pageResponse[12] << 8) + pageResponse[13]);

            return decoded;
        }

        public static string PrettifyModePage_1C_S01(byte[] pageResponse)
        {
            return PrettifyModePage_1C_S01(DecodeModePage_1C_S01(pageResponse));
        }

        public static string PrettifyModePage_1C_S01(ModePage_1C_S01? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_1C_S01 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Background Control page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            if (page.S_L_Full)
                sb.AppendLine("\tBackground scans will be halted if log is full");
            if (page.LOWIR)
                sb.AppendLine("\tBackground scans will only be logged if they require intervention");
            if (page.En_Bms)
                sb.AppendLine("\tBackground medium scans are enabled");
            if (page.En_Ps)
                sb.AppendLine("\tBackground pre-scans are enabled");

            if (page.BackgroundScanInterval > 0)
                sb.AppendFormat("\t{0} hours shall be between the start of a background scan operation and the next", page.BackgroundScanInterval).AppendLine();

            if (page.BackgroundPrescanTimeLimit > 0)
                sb.AppendFormat("\tBackgroun pre-scan operations can take a maximum of {0} hours", page.BackgroundPrescanTimeLimit).AppendLine();

            if (page.MinIdleBeforeBgScan > 0)
                sb.AppendFormat("\tAt least {0} ms must be idle before resuming a suspended background scan operation", page.MinIdleBeforeBgScan).AppendLine();

            if (page.MaxTimeSuspendBgScan > 0)
                sb.AppendFormat("\tAt most {0} ms must be before suspending a background scan operation and processing received commands", page.MaxTimeSuspendBgScan).AppendLine();

            return sb.ToString();
        }
        #endregion Mode Page 0x1C subpage 0x01: Background Control mode page

        #region Mode Page 0x0F: Data compression page
        /// <summary>
        /// Data compression page
        /// Page code 0x0F
        /// 16 bytes in SSC-1, SSC-2, SSC-3
        /// </summary>
        public struct ModePage_0F
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Data compression enabled
            /// </summary>
            public bool DCE;
            /// <summary>
            /// Data compression capable
            /// </summary>
            public bool DCC;
            /// <summary>
            /// Data decompression enabled
            /// </summary>
            public bool DDE;
            /// <summary>
            /// Report exception on decompression
            /// </summary>
            public byte RED;
            /// <summary>
            /// Compression algorithm
            /// </summary>
            public uint CompressionAlgo;
            /// <summary>
            /// Decompression algorithm
            /// </summary>
            public uint DecompressionAlgo;
        }

        public static ModePage_0F? DecodeModePage_0F(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x0F)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 24)
                return null;

            ModePage_0F decoded = new ModePage_0F();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.DCE |= (pageResponse[2] & 0x80) == 0x80;
            decoded.DCC |= (pageResponse[2] & 0x40) == 0x40;
            decoded.DDE |= (pageResponse[3] & 0x80) == 0x80;
            decoded.RED = (byte)((pageResponse[3] & 0x60) >> 5);

            decoded.CompressionAlgo = (uint)((pageResponse[4] << 24) + (pageResponse[5] << 16) + (pageResponse[6] << 8) + pageResponse[7]);
            decoded.DecompressionAlgo = (uint)((pageResponse[8] << 24) + (pageResponse[9] << 16) + (pageResponse[10] << 8) + pageResponse[11]);

            return decoded;
        }

        public static string PrettifyModePage_0F(byte[] pageResponse)
        {
            return PrettifyModePage_0F(DecodeModePage_0F(pageResponse));
        }

        public static string PrettifyModePage_0F(ModePage_0F? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_0F page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Data compression page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            if (page.DCC)
            {
                sb.AppendLine("\tDrive supports data compression");
                if (page.DCE)
                {
                    sb.Append("\tData compression is enabled with ");
                    switch (page.CompressionAlgo)
                    {
                        case 3:
                            sb.AppendLine("IBM ALDC with 512 byte buffer");
                            break;
                        case 4:
                            sb.AppendLine("IBM ALDC with 1024 byte buffer");
                            break;
                        case 5:
                            sb.AppendLine("IBM ALDC with 2048 byte buffer");
                            break;
                        case 0x10:
                            sb.AppendLine("IBM IDRC");
                            break;
                        case 0x20:
                            sb.AppendLine("DCLZ");
                            break;
                        case 0xFF:
                            sb.AppendLine("an unregistered compression algorithm");
                            break;
                        default:
                            sb.AppendFormat("an unknown algorithm coded {0}", page.CompressionAlgo).AppendLine();
                            break;
                    }
                }
                if (page.DDE)
                {
                    sb.AppendLine("\tData decompression is enabled");
                    if (page.DecompressionAlgo == 0)
                        sb.AppendLine("\tLast data read was uncompressed");
                    else
                    {
                        sb.Append("\tLast data read was compressed with ");
                        switch (page.CompressionAlgo)
                        {
                            case 3:
                                sb.AppendLine("IBM ALDC with 512 byte buffer");
                                break;
                            case 4:
                                sb.AppendLine("IBM ALDC with 1024 byte buffer");
                                break;
                            case 5:
                                sb.AppendLine("IBM ALDC with 2048 byte buffer");
                                break;
                            case 0x10:
                                sb.AppendLine("IBM IDRC");
                                break;
                            case 0x20:
                                sb.AppendLine("DCLZ");
                                break;
                            case 0xFF:
                                sb.AppendLine("an unregistered compression algorithm");
                                break;
                            default:
                                sb.AppendFormat("an unknown algorithm coded {0}", page.CompressionAlgo).AppendLine();
                                break;
                        }
                    }
                }

                sb.AppendFormat("\tReport exception on compression is set to {0}", page.RED).AppendLine();
            }
            else
                sb.AppendLine("\tDrive does not support data compression");

            return sb.ToString();
        }
        #endregion Mode Page 0x0F: Data compression page

        #region Mode Page 0x1B: Removable Block Access Capabilities page
        /// <summary>
        /// Removable Block Access Capabilities page
        /// Page code 0x1B
        /// 12 bytes in INF-8070
        /// </summary>
        public struct ModePage_1B
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Supports reporting progress of format
            /// </summary>
            public bool SRFP;
            /// <summary>
            /// Non-CD Optical Device
            /// </summary>
            public bool NCD;
            /// <summary>
            /// Phase change dual device supporting a CD and a Non-CD Optical devices
            /// </summary>
            public bool SML;
            /// <summary>
            /// Total number of LUNs
            /// </summary>
            public byte TLUN;
            /// <summary>
            /// System Floppy Type device
            /// </summary>
            public bool SFLP;
        }

        public static ModePage_1B? DecodeModePage_1B(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x1B)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 12)
                return null;

            ModePage_1B decoded = new ModePage_1B();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.SFLP |= (pageResponse[2] & 0x80) == 0x80;
            decoded.SRFP |= (pageResponse[2] & 0x40) == 0x40;
            decoded.NCD |= (pageResponse[3] & 0x80) == 0x80;
            decoded.SML |= (pageResponse[3] & 0x40) == 0x40;

            decoded.TLUN = (byte)(pageResponse[3] & 0x07);

            return decoded;
        }

        public static string PrettifyModePage_1B(byte[] pageResponse)
        {
            return PrettifyModePage_1B(DecodeModePage_1B(pageResponse));
        }

        public static string PrettifyModePage_1B(ModePage_1B? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_1B page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Removable Block Access Capabilities page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            if (page.SFLP)
                sb.AppendLine("\tDrive can be used as a system floppy device");
            if (page.SRFP)
                sb.AppendLine("\tDrive supports reporting progress of format");
            if (page.NCD)
                sb.AppendLine("\tDrive is a Non-CD Optical Device");
            if (page.SML)
                sb.AppendLine("\tDevice is a dual device supporting CD and Non-CD Optical");
            if (page.TLUN > 0)
                sb.AppendFormat("\tDrive supports {0} LUNs", page.TLUN).AppendLine();

            return sb.ToString();
        }
        #endregion Mode Page 0x1B: Removable Block Access Capabilities page

        #region Mode Page 0x1C: Timer & Protect page
        /// <summary>
        /// Timer & Protect page
        /// Page code 0x1C
        /// 8 bytes in INF-8070
        /// </summary>
        public struct ModePage_1C_SFF
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Time the device shall remain in the current state after seek, read or write operation
            /// </summary>
            public byte InactivityTimeMultiplier;
            /// <summary>
            /// Disabled until power cycle
            /// </summary>
            public bool DISP;
            /// <summary>
            /// Software Write Protect until Power-down
            /// </summary>
            public bool SWPP;
        }

        public static ModePage_1C_SFF? DecodeModePage_1C_SFF(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x1C)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 8)
                return null;

            ModePage_1C_SFF decoded = new ModePage_1C_SFF();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.DISP |= (pageResponse[2] & 0x02) == 0x02;
            decoded.SWPP |= (pageResponse[3] & 0x01) == 0x01;

            decoded.InactivityTimeMultiplier = (byte)(pageResponse[3] & 0x0F);

            return decoded;
        }

        public static string PrettifyModePage_1C_SFF(byte[] pageResponse)
        {
            return PrettifyModePage_1C_SFF(DecodeModePage_1C_SFF(pageResponse));
        }

        public static string PrettifyModePage_1C_SFF(ModePage_1C_SFF? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_1C_SFF page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Timer & Protect page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            if (page.DISP)
                sb.AppendLine("\tDrive is disabled until power is cycled");
            if (page.SWPP)
                sb.AppendLine("\tDrive is software write-protected until powered down");

            switch (page.InactivityTimeMultiplier)
            {
                case 0:
                    sb.AppendLine("Drive will remain in same status a vendor-specified time after a seek, read or write operation");
                    break;
                case 1:
                    sb.AppendLine("Drive will remain in same status 125 ms after a seek, read or write operation");
                    break;
                case 2:
                    sb.AppendLine("Drive will remain in same status 250 ms after a seek, read or write operation");
                    break;
                case 3:
                    sb.AppendLine("Drive will remain in same status 500 ms after a seek, read or write operation");
                    break;
                case 4:
                    sb.AppendLine("Drive will remain in same status 1 second after a seek, read or write operation");
                    break;
                case 5:
                    sb.AppendLine("Drive will remain in same status 2 seconds after a seek, read or write operation");
                    break;
                case 6:
                    sb.AppendLine("Drive will remain in same status 4 seconds after a seek, read or write operation");
                    break;
                case 7:
                    sb.AppendLine("Drive will remain in same status 8 seconds after a seek, read or write operation");
                    break;
                case 8:
                    sb.AppendLine("Drive will remain in same status 16 seconds after a seek, read or write operation");
                    break;
                case 9:
                    sb.AppendLine("Drive will remain in same status 32 seconds after a seek, read or write operation");
                    break;
                case 10:
                    sb.AppendLine("Drive will remain in same status 1 minute after a seek, read or write operation");
                    break;
                case 11:
                    sb.AppendLine("Drive will remain in same status 2 minutes after a seek, read or write operation");
                    break;
                case 12:
                    sb.AppendLine("Drive will remain in same status 4 minutes after a seek, read or write operation");
                    break;
                case 13:
                    sb.AppendLine("Drive will remain in same status 8 minutes after a seek, read or write operation");
                    break;
                case 14:
                    sb.AppendLine("Drive will remain in same status 16 minutes after a seek, read or write operation");
                    break;
                case 15:
                    sb.AppendLine("Drive will remain in same status 32 minutes after a seek, read or write operation");
                    break;
            }

            return sb.ToString();
        }
        #endregion Mode Page 0x1C: Timer & Protect page

        #region Mode Page 0x00: Drive Operation Mode page
        /// <summary>
        /// Drive Operation Mode page
        /// Page code 0x00
        /// 4 bytes in INF-8070
        /// </summary>
        public struct ModePage_00_SFF
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Select LUN Mode
            /// </summary>
            public bool SLM;
            /// <summary>
            /// Select LUN for rewritable
            /// </summary>
            public bool SLR;
            /// <summary>
            /// Disable verify for WRITE
            /// </summary>
            public bool DVW;
            /// <summary>
            /// Disable deferred error
            /// </summary>
            public bool DDE;
        }

        public static ModePage_00_SFF? DecodeModePage_00_SFF(byte[] pageResponse)
        {
            if (pageResponse == null)
                return null;

            if ((pageResponse[0] & 0x40) == 0x40)
                return null;

            if ((pageResponse[0] & 0x3F) != 0x00)
                return null;

            if (pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if (pageResponse.Length < 4)
                return null;

            ModePage_00_SFF decoded = new ModePage_00_SFF();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.SLM |= (pageResponse[2] & 0x80) == 0x80;
            decoded.SLR |= (pageResponse[2] & 0x40) == 0x40;
            decoded.DVW |= (pageResponse[2] & 0x20) == 0x20;

            decoded.DDE |= (pageResponse[3] & 0x10) == 0x10;

            return decoded;
        }

        public static string PrettifyModePage_00_SFF(byte[] pageResponse)
        {
            return PrettifyModePage_00_SFF(DecodeModePage_00_SFF(pageResponse));
        }

        public static string PrettifyModePage_00_SFF(ModePage_00_SFF? modePage)
        {
            if (!modePage.HasValue)
                return null;

            ModePage_00_SFF page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Drive Operation Mode page:");

            if (page.PS)
                sb.AppendLine("\tParameters can be saved");

            if (page.DVW)
                sb.AppendLine("\tVerifying after writing is disabled");
            if (page.DDE)
                sb.AppendLine("\tDrive will abort when a writing error is detected");

            if (page.SLM)
            {
                sb.Append("\tDrive has two LUNs with rewritable being ");
                if (page.SLM)
                    sb.AppendLine("LUN 1");
                else
                    sb.AppendLine("LUN 0");
            }

            return sb.ToString();
        }
        #endregion Mode Page 0x00: Drive Operation Mode page
    }
}

