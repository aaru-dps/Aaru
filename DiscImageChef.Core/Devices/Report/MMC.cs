// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates reports from SCSI MultiMedia devices.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using System.Text;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices.Report
{
    public partial class DeviceReport
    {
        static byte[] ClearMmcFeatures(byte[] response)
        {
            uint offset = 8;

            while(offset + 4 < response.Length)
            {
                ushort code = (ushort)((response[offset + 0] << 8) + response[offset + 1]);
                byte[] data = new byte[response[offset + 3] + 4];

                if(code != 0x0108)
                {
                    offset += (uint)data.Length;
                    continue;
                }

                if(data.Length + offset > response.Length) data = new byte[response.Length - offset];
                Array.Copy(data, 4, response, offset                                       + 4, data.Length - 4);
                offset += (uint)data.Length;
            }

            return response;
        }

        public MmcFeatures ReportMmcFeatures()
        {
            DicConsole.WriteLine("Querying MMC GET CONFIGURATION...");
            bool sense = dev.GetConfiguration(out byte[] buffer, out _, dev.Timeout, out _);

            if(sense) return null;

            Features.SeparatedFeatures ftr = Features.Separate(buffer);
            if(ftr.Descriptors == null || ftr.Descriptors.Length <= 0) return null;

            MmcFeatures report = new MmcFeatures {BinaryData = ClearMmcFeatures(buffer)};
            foreach(Features.FeatureDescriptor desc in ftr.Descriptors)
                switch(desc.Code)
                {
                    case 0x0001:
                    {
                        Feature_0001? ftr0001 = Features.Decode_0001(desc.Data);
                        if(ftr0001.HasValue)
                        {
                            report.PhysicalInterfaceStandardNumber = (uint)ftr0001.Value.PhysicalInterfaceStandard;

                            report.SupportsDeviceBusyEvent = ftr0001.Value.DBE;
                        }
                    }
                        break;
                    case 0x0003:
                    {
                        Feature_0003? ftr0003 = Features.Decode_0003(desc.Data);
                        if(ftr0003.HasValue)
                        {
                            report.LoadingMechanismType = ftr0003.Value.LoadingMechanismType;
                            report.CanLoad              = ftr0003.Value.Load;
                            report.CanEject             = ftr0003.Value.Eject;
                            report.PreventJumper        = ftr0003.Value.PreventJumper;
                            report.DBML                 = ftr0003.Value.DBML;
                            report.Locked               = ftr0003.Value.Lock;
                        }
                    }
                        break;
                    case 0x0004:
                    {
                        Feature_0004? ftr0004 = Features.Decode_0004(desc.Data);
                        if(ftr0004.HasValue)
                        {
                            report.SupportsWriteProtectPAC = ftr0004.Value.DWP;
                            report.SupportsWriteInhibitDCB = ftr0004.Value.WDCB;
                            report.SupportsPWP             = ftr0004.Value.SPWP;
                            report.SupportsSWPP            = ftr0004.Value.SSWPP;
                        }
                    }
                        break;
                    case 0x0010:
                    {
                        Feature_0010? ftr0010 = Features.Decode_0010(desc.Data);
                        if(ftr0010.HasValue)
                        {
                            if(ftr0010.Value.LogicalBlockSize > 0)
                                report.LogicalBlockSize = ftr0010.Value.LogicalBlockSize;

                            if(ftr0010.Value.Blocking > 0) report.BlocksPerReadableUnit = ftr0010.Value.Blocking;

                            report.ErrorRecoveryPage = ftr0010.Value.PP;
                        }
                    }
                        break;
                    case 0x001D:
                        report.MultiRead = true;
                        break;
                    case 0x001E:
                    {
                        report.CanReadCD = true;
                        Feature_001E? ftr001E = Features.Decode_001E(desc.Data);
                        if(ftr001E.HasValue)
                        {
                            report.SupportsDAP         = ftr001E.Value.DAP;
                            report.SupportsC2          = ftr001E.Value.C2;
                            report.CanReadLeadInCDText = ftr001E.Value.CDText;
                        }
                    }
                        break;
                    case 0x001F:
                    {
                        report.CanReadDVD = true;
                        Feature_001F? ftr001F = Features.Decode_001F(desc.Data);
                        if(ftr001F.HasValue)
                        {
                            report.DVDMultiRead     = ftr001F.Value.MULTI110;
                            report.CanReadAllDualRW = ftr001F.Value.DualRW;
                            report.CanReadAllDualR  = ftr001F.Value.DualR;
                        }
                    }
                        break;
                    case 0x0022:
                        report.CanEraseSector = true;
                        break;
                    case 0x0023:
                    {
                        report.CanFormat = true;
                        Feature_0023? ftr0023 = Features.Decode_0023(desc.Data);
                        if(ftr0023.HasValue)
                        {
                            report.CanFormatBDREWithoutSpare = ftr0023.Value.RENoSA;
                            report.CanExpandBDRESpareArea    = ftr0023.Value.Expand;
                            report.CanFormatQCert            = ftr0023.Value.QCert;
                            report.CanFormatCert             = ftr0023.Value.Cert;
                            report.CanFormatFRF              = ftr0023.Value.FRF;
                            report.CanFormatRRM              = ftr0023.Value.RRM;
                        }
                    }
                        break;
                    case 0x0024:
                        report.CanReadSpareAreaInformation = true;
                        break;
                    case 0x0027:
                        report.CanWriteCDRWCAV = true;
                        break;
                    case 0x0028:
                    {
                        report.CanReadCDMRW = true;
                        Feature_0028? ftr0028 = Features.Decode_0028(desc.Data);
                        if(ftr0028.HasValue)
                        {
                            report.CanReadDVDPlusMRW  = ftr0028.Value.DVDPRead;
                            report.CanWriteDVDPlusMRW = ftr0028.Value.DVDPWrite;
                            report.CanWriteCDMRW      = ftr0028.Value.Write;
                        }
                    }
                        break;
                    case 0x002A:
                    {
                        report.CanReadDVDPlusRW = true;
                        Feature_002A? ftr002A                         = Features.Decode_002A(desc.Data);
                        if(ftr002A.HasValue) report.CanWriteDVDPlusRW = ftr002A.Value.Write;
                    }
                        break;
                    case 0x002B:
                    {
                        report.CanReadDVDPlusR = true;
                        Feature_002B? ftr002B                        = Features.Decode_002B(desc.Data);
                        if(ftr002B.HasValue) report.CanWriteDVDPlusR = ftr002B.Value.Write;
                    }
                        break;
                    case 0x002D:
                    {
                        report.CanWriteCDTAO = true;
                        Feature_002D? ftr002D = Features.Decode_002D(desc.Data);
                        if(ftr002D.HasValue)
                        {
                            report.BufferUnderrunFreeInTAO       = ftr002D.Value.BUF;
                            report.CanWriteRawSubchannelInTAO    = ftr002D.Value.RWRaw;
                            report.CanWritePackedSubchannelInTAO = ftr002D.Value.RWPack;
                            report.CanTestWriteInTAO             = ftr002D.Value.TestWrite;
                            report.CanOverwriteTAOTrack          = ftr002D.Value.CDRW;
                            report.CanWriteRWSubchannelInTAO     = ftr002D.Value.RWSubchannel;
                        }
                    }
                        break;
                    case 0x002E:
                    {
                        report.CanWriteCDSAO = true;
                        Feature_002E? ftr002E = Features.Decode_002E(desc.Data);
                        if(ftr002E.HasValue)
                        {
                            report.BufferUnderrunFreeInSAO   = ftr002E.Value.BUF;
                            report.CanWriteRawMultiSession   = ftr002E.Value.RAWMS;
                            report.CanWriteRaw               = ftr002E.Value.RAW;
                            report.CanTestWriteInSAO         = ftr002E.Value.TestWrite;
                            report.CanOverwriteSAOTrack      = ftr002E.Value.CDRW;
                            report.CanWriteRWSubchannelInSAO = ftr002E.Value.RW;
                        }
                    }
                        break;
                    case 0x002F:
                    {
                        report.CanWriteDVDR = true;
                        Feature_002F? ftr002F = Features.Decode_002F(desc.Data);
                        if(ftr002F.HasValue)
                        {
                            report.BufferUnderrunFreeInDVD = ftr002F.Value.BUF;
                            report.CanWriteDVDRDL          = ftr002F.Value.RDL;
                            report.CanTestWriteDVD         = ftr002F.Value.TestWrite;
                            report.CanWriteDVDRW           = ftr002F.Value.DVDRW;
                        }
                    }
                        break;
                    case 0x0030:
                        report.CanReadDDCD = true;
                        break;
                    case 0x0031:
                    {
                        report.CanWriteDDCDR = true;
                        Feature_0031? ftr0031                         = Features.Decode_0031(desc.Data);
                        if(ftr0031.HasValue) report.CanTestWriteDDCDR = ftr0031.Value.TestWrite;
                    }
                        break;
                    case 0x0032:
                        report.CanWriteDDCDRW = true;
                        break;
                    case 0x0037:
                        report.CanWriteCDRW = true;
                        break;
                    case 0x0038:
                        report.CanPseudoOverwriteBDR = true;
                        break;
                    case 0x003A:
                    {
                        report.CanReadDVDPlusRWDL = true;
                        Feature_003A? ftr003A                           = Features.Decode_003A(desc.Data);
                        if(ftr003A.HasValue) report.CanWriteDVDPlusRWDL = ftr003A.Value.Write;
                    }
                        break;
                    case 0x003B:
                    {
                        report.CanReadDVDPlusRDL = true;
                        Feature_003B? ftr003B                          = Features.Decode_003B(desc.Data);
                        if(ftr003B.HasValue) report.CanWriteDVDPlusRDL = ftr003B.Value.Write;
                    }
                        break;
                    case 0x0040:
                    {
                        report.CanReadBD = true;
                        Feature_0040? ftr0040 = Features.Decode_0040(desc.Data);
                        if(ftr0040.HasValue)
                        {
                            report.CanReadBluBCA   = ftr0040.Value.BCA;
                            report.CanReadBDRE2    = ftr0040.Value.RE2;
                            report.CanReadBDRE1    = ftr0040.Value.RE1;
                            report.CanReadOldBDRE  = ftr0040.Value.OldRE;
                            report.CanReadBDR      = ftr0040.Value.R;
                            report.CanReadOldBDR   = ftr0040.Value.OldR;
                            report.CanReadBDROM    = ftr0040.Value.ROM;
                            report.CanReadOldBDROM = ftr0040.Value.OldROM;
                        }
                    }
                        break;
                    case 0x0041:
                    {
                        report.CanWriteBD = true;
                        Feature_0041? ftr0041 = Features.Decode_0041(desc.Data);
                        if(ftr0041.HasValue)
                        {
                            report.CanWriteBDRE2   = ftr0041.Value.RE2;
                            report.CanWriteBDRE1   = ftr0041.Value.RE1;
                            report.CanWriteOldBDRE = ftr0041.Value.OldRE;
                            report.CanWriteBDR     = ftr0041.Value.R;
                            report.CanWriteOldBDR  = ftr0041.Value.OldR;
                        }
                    }
                        break;
                    case 0x0050:
                    {
                        report.CanReadHDDVD = true;
                        Feature_0050? ftr0050 = Features.Decode_0050(desc.Data);
                        if(ftr0050.HasValue)
                        {
                            report.CanReadHDDVDR   = ftr0050.Value.HDDVDR;
                            report.CanReadHDDVDRAM = ftr0050.Value.HDDVDRAM;
                        }
                    }
                        break;
                    case 0x0051:
                    {
                        // TODO: Write HD DVD-RW
                        Feature_0051? ftr0051 = Features.Decode_0051(desc.Data);
                        if(ftr0051.HasValue)
                        {
                            report.CanWriteHDDVDR   = ftr0051.Value.HDDVDR;
                            report.CanWriteHDDVDRAM = ftr0051.Value.HDDVDRAM;
                        }
                    }
                        break;
                    case 0x0080:
                        report.SupportsHybridDiscs = true;
                        break;
                    case 0x0101:
                        report.SupportsModePage1Ch = true;
                        break;
                    case 0x0102:
                    {
                        report.EmbeddedChanger = true;
                        Feature_0102? ftr0102 = Features.Decode_0102(desc.Data);
                        if(ftr0102.HasValue)
                        {
                            report.ChangerIsSideChangeCapable = ftr0102.Value.SCC;
                            report.ChangerSupportsDiscPresent = ftr0102.Value.SDP;
                            report.ChangerSlots               = (byte)(ftr0102.Value.HighestSlotNumber + 1);
                        }
                    }
                        break;
                    case 0x0103:
                    {
                        report.CanPlayCDAudio = true;
                        Feature_0103? ftr0103 = Features.Decode_0103(desc.Data);
                        if(ftr0103.HasValue)
                        {
                            report.CanAudioScan            = ftr0103.Value.Scan;
                            report.CanMuteSeparateChannels = ftr0103.Value.SCM;
                            report.SupportsSeparateVolume  = ftr0103.Value.SV;
                            if(ftr0103.Value.VolumeLevels > 0) report.VolumeLevels = ftr0103.Value.VolumeLevels;
                        }
                    }
                        break;
                    case 0x0104:
                        report.CanUpgradeFirmware = true;
                        break;
                    case 0x0106:
                    {
                        report.SupportsCSS = true;
                        Feature_0106? ftr0106 = Features.Decode_0106(desc.Data);
                        if(ftr0106.HasValue)
                            if(ftr0106.Value.CSSVersion > 0)
                                report.CSSVersion = ftr0106.Value.CSSVersion;
                    }
                        break;
                    case 0x0108:
                        report.CanReportDriveSerial = true;
                        break;
                    case 0x0109:
                        report.CanReportMediaSerial = true;
                        break;
                    case 0x010B:
                    {
                        report.SupportsCPRM = true;
                        Feature_010B? ftr010B = Features.Decode_010B(desc.Data);
                        if(ftr010B.HasValue)
                            if(ftr010B.Value.CPRMVersion > 0)
                                report.CPRMVersion = ftr010B.Value.CPRMVersion;
                    }
                        break;
                    case 0x010C:
                    {
                        Feature_010C? ftr010C = Features.Decode_010C(desc.Data);
                        if(ftr010C.HasValue)
                        {
                            byte[] temp = new byte[4];
                            temp[0] = (byte)((ftr010C.Value.Century & 0xFF00) >> 8);
                            temp[1] = (byte)(ftr010C.Value.Century & 0xFF);
                            temp[2] = (byte)((ftr010C.Value.Year & 0xFF00) >> 8);
                            temp[3] = (byte)(ftr010C.Value.Year & 0xFF);
                            string syear = Encoding.ASCII.GetString(temp);
                            temp    = new byte[2];
                            temp[0] = (byte)((ftr010C.Value.Month & 0xFF00) >> 8);
                            temp[1] = (byte)(ftr010C.Value.Month & 0xFF);
                            string smonth = Encoding.ASCII.GetString(temp);
                            temp    = new byte[2];
                            temp[0] = (byte)((ftr010C.Value.Day & 0xFF00) >> 8);
                            temp[1] = (byte)(ftr010C.Value.Day & 0xFF);
                            string sday = Encoding.ASCII.GetString(temp);
                            temp    = new byte[2];
                            temp[0] = (byte)((ftr010C.Value.Hour & 0xFF00) >> 8);
                            temp[1] = (byte)(ftr010C.Value.Hour & 0xFF);
                            string shour = Encoding.ASCII.GetString(temp);
                            temp    = new byte[2];
                            temp[0] = (byte)((ftr010C.Value.Minute & 0xFF00) >> 8);
                            temp[1] = (byte)(ftr010C.Value.Minute & 0xFF);
                            string sminute = Encoding.ASCII.GetString(temp);
                            temp    = new byte[2];
                            temp[0] = (byte)((ftr010C.Value.Second & 0xFF00) >> 8);
                            temp[1] = (byte)(ftr010C.Value.Second & 0xFF);
                            string ssecond = Encoding.ASCII.GetString(temp);

                            try
                            {
                                report.FirmwareDate = new DateTime(int.Parse(syear), int.Parse(smonth), int.Parse(sday),
                                                                   int.Parse(shour), int.Parse(sminute),
                                                                   int.Parse(ssecond), DateTimeKind.Utc);
                            }
                            #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                            catch
                            {
                                // ignored
                            }
                            #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                        }
                    }
                        break;
                    case 0x010D:
                    {
                        report.SupportsAACS = true;
                        Feature_010D? ftr010D = Features.Decode_010D(desc.Data);
                        if(ftr010D.HasValue)
                        {
                            report.CanReadDriveAACSCertificate = ftr010D.Value.RDC;
                            report.CanReadCPRM_MKB             = ftr010D.Value.RMC;
                            report.CanWriteBusEncryptedBlocks  = ftr010D.Value.WBE;
                            report.SupportsBusEncryption       = ftr010D.Value.BEC;
                            report.CanGenerateBindingNonce     = ftr010D.Value.BNG;

                            if(ftr010D.Value.BindNonceBlocks > 0)
                                report.BindingNonceBlocks = ftr010D.Value.BindNonceBlocks;

                            if(ftr010D.Value.AGIDs > 0) report.AGIDs = ftr010D.Value.AGIDs;

                            if(ftr010D.Value.AACSVersion > 0) report.AACSVersion = ftr010D.Value.AACSVersion;
                        }
                    }
                        break;
                    case 0x010E:
                        report.CanWriteCSSManagedDVD = true;
                        break;
                    case 0x0113:
                        report.SupportsSecurDisc = true;
                        break;
                    case 0x0142:
                        report.SupportsOSSC = true;
                        break;
                    case 0x0110:
                        report.SupportsVCPS = true;
                        break;
                }

            return report;
        }

        public TestedMedia ReportMmcMedia(string mediaType, bool tryPlextor, bool tryPioneer, bool tryNec,
                                          bool   tryHldtst)
        {
            TestedMedia mediaTest = new TestedMedia();

            DicConsole.WriteLine("Querying SCSI READ CAPACITY...");
            bool sense = dev.ReadCapacity(out byte[] buffer, out byte[] senseBuffer, dev.Timeout, out _);
            if(!sense && !dev.Error)
            {
                mediaTest.SupportsReadCapacity = true;
                mediaTest.Blocks =
                    (ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3]) + 1;
                mediaTest.BlockSize =
                    (uint)((buffer[5] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + buffer[7]);
            }

            DicConsole.WriteLine("Querying SCSI READ CAPACITY (16)...");
            sense = dev.ReadCapacity16(out buffer, out buffer, dev.Timeout, out _);
            if(!sense && !dev.Error)
            {
                mediaTest.SupportsReadCapacity16 = true;
                byte[] temp = new byte[8];
                Array.Copy(buffer, 0, temp, 0, 8);
                Array.Reverse(temp);
                mediaTest.Blocks    = BitConverter.ToUInt64(temp, 0) + 1;
                mediaTest.BlockSize = (uint)((buffer[5] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + buffer[7]);
            }

            Modes.DecodedMode? decMode = null;

            DicConsole.WriteLine("Querying SCSI MODE SENSE (10)...");
            sense = dev.ModeSense10(out buffer, out senseBuffer, false, true, ScsiModeSensePageControl.Current, 0x3F,
                                    0x00, dev.Timeout, out _);
            if(!sense && !dev.Error)
            {
                decMode = Modes.DecodeMode10(buffer, dev.ScsiType);
                if(debug) mediaTest.ModeSense10Data = buffer;
            }

            DicConsole.WriteLine("Querying SCSI MODE SENSE...");
            sense = dev.ModeSense(out buffer, out senseBuffer, dev.Timeout, out _);
            if(!sense && !dev.Error)
            {
                if(!decMode.HasValue) decMode      = Modes.DecodeMode6(buffer, dev.ScsiType);
                if(debug) mediaTest.ModeSense6Data = buffer;
            }

            if(decMode.HasValue)
            {
                mediaTest.MediumType = (byte)decMode.Value.Header.MediumType;
                if(decMode.Value.Header.BlockDescriptors != null && decMode.Value.Header.BlockDescriptors.Length > 0)
                    mediaTest.Density = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
            }

            if(mediaType.StartsWith("CD-",   StringComparison.Ordinal) ||
               mediaType.StartsWith("DDCD-", StringComparison.Ordinal) || mediaType == "Audio CD")
            {
                DicConsole.WriteLine("Querying CD TOC...");
                mediaTest.CanReadTOC =
                    !dev.ReadTocPmaAtip(out buffer, out senseBuffer, false, 0, 0, dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadTOC);
                if(debug) mediaTest.TocData = buffer;

                DicConsole.WriteLine("Querying CD Full TOC...");
                mediaTest.CanReadFullTOC = !dev.ReadRawToc(out buffer, out senseBuffer, 1, dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadFullTOC);
                if(debug) mediaTest.FullTocData = buffer;
            }

            if(mediaType.StartsWith("CD-R",   StringComparison.Ordinal) ||
               mediaType.StartsWith("DDCD-R", StringComparison.Ordinal))
            {
                DicConsole.WriteLine("Querying CD ATIP...");
                mediaTest.CanReadATIP = !dev.ReadAtip(out buffer, out senseBuffer, dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadATIP);
                if(debug) mediaTest.AtipData = buffer;
                DicConsole.WriteLine("Querying CD PMA...");
                mediaTest.CanReadPMA = !dev.ReadPma(out buffer, out senseBuffer, dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPMA);
                if(debug) mediaTest.PmaData = buffer;
            }

            if(mediaType.StartsWith("DVD-",    StringComparison.Ordinal) ||
               mediaType.StartsWith("HD DVD-", StringComparison.Ordinal))
            {
                DicConsole.WriteLine("Querying DVD PFI...");
                mediaTest.CanReadPFI = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                              MmcDiscStructureMediaType.Dvd, 0, 0,
                                                              MmcDiscStructureFormat.PhysicalInformation, 0,
                                                              dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPFI);
                if(debug) mediaTest.PfiData = buffer;
                DicConsole.WriteLine("Querying DVD DMI...");
                mediaTest.CanReadDMI = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                              MmcDiscStructureMediaType.Dvd, 0, 0,
                                                              MmcDiscStructureFormat.DiscManufacturingInformation, 0,
                                                              dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDMI);
                if(debug) mediaTest.DmiData = buffer;
            }

            if(mediaType == "DVD-ROM")
            {
                DicConsole.WriteLine("Querying DVD CMI...");
                mediaTest.CanReadCMI = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                              MmcDiscStructureMediaType.Dvd, 0, 0,
                                                              MmcDiscStructureFormat.CopyrightInformation, 0,
                                                              dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadCMI);
                if(debug) mediaTest.CmiData = buffer;
            }

            switch(mediaType)
            {
                case "DVD-ROM":
                case "HD DVD-ROM":
                    DicConsole.WriteLine("Querying DVD BCA...");
                    mediaTest.CanReadBCA = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                  MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                  MmcDiscStructureFormat.BurstCuttingArea, 0,
                                                                  dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadBCA);
                    if(debug) mediaTest.DvdBcaData = buffer;
                    DicConsole.WriteLine("Querying DVD AACS...");
                    mediaTest.CanReadAACS = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                   MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                   MmcDiscStructureFormat.DvdAacs, 0, dev.Timeout,
                                                                   out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadAACS);
                    if(debug) mediaTest.DvdAacsData = buffer;
                    break;
                case "BD-ROM":
                case "Ultra HD Blu-ray movie":
                case "PlayStation 3 game":
                case "PlayStation 4 game":
                case "Xbox One game":
                    DicConsole.WriteLine("Querying BD BCA...");
                    mediaTest.CanReadBCA = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                  MmcDiscStructureMediaType.Bd, 0, 0,
                                                                  MmcDiscStructureFormat.BdBurstCuttingArea, 0,
                                                                  dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadBCA);
                    if(debug) mediaTest.BluBcaData = buffer;
                    break;
                case "DVD-RAM (1st gen, marked 2.6Gb or 5.2Gb)":
                case "DVD-RAM (2nd gen, marked 4.7Gb or 9.4Gb)":
                case "HD DVD-RAM":
                    mediaTest.CanReadDDS = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                  MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                  MmcDiscStructureFormat.DvdramDds, 0, dev.Timeout,
                                                                  out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDDS);
                    if(debug) mediaTest.DvdDdsData = buffer;
                    mediaTest.CanReadSpareAreaInformation =
                        !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                               MmcDiscStructureFormat.DvdramSpareAreaInformation, 0, dev.Timeout,
                                               out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadSpareAreaInformation);
                    if(debug) mediaTest.DvdSaiData = buffer;
                    break;
            }

            if(mediaType.StartsWith("BD-R", StringComparison.Ordinal) && mediaType != "BD-ROM")
            {
                DicConsole.WriteLine("Querying BD DDS...");
                mediaTest.CanReadDDS = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Bd,
                                                              0, 0, MmcDiscStructureFormat.BdDds, 0, dev.Timeout,
                                                              out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDDS);
                if(debug) mediaTest.BluDdsData = buffer;
                DicConsole.WriteLine("Querying BD SAI...");
                mediaTest.CanReadSpareAreaInformation =
                    !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Bd, 0, 0,
                                           MmcDiscStructureFormat.BdSpareAreaInformation, 0, dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadSpareAreaInformation);
                if(debug) mediaTest.BluSaiData = buffer;
            }

            if(mediaType == "DVD-R" || mediaType == "DVD-RW")
            {
                DicConsole.WriteLine("Querying DVD PRI...");
                mediaTest.CanReadPRI = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                              MmcDiscStructureMediaType.Dvd, 0, 0,
                                                              MmcDiscStructureFormat.PreRecordedInfo, 0, dev.Timeout,
                                                              out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPRI);
                if(debug) mediaTest.PriData = buffer;
            }

            if(mediaType == "DVD-R" || mediaType == "DVD-RW" || mediaType == "HD DVD-R")
            {
                DicConsole.WriteLine("Querying DVD Media ID...");
                mediaTest.CanReadMediaID = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                  MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                  MmcDiscStructureFormat.DvdrMediaIdentifier, 0,
                                                                  dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadMediaID);
                DicConsole.WriteLine("Querying DVD Embossed PFI...");
                mediaTest.CanReadRecordablePFI = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                        MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                        MmcDiscStructureFormat.DvdrPhysicalInformation,
                                                                        0, dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRecordablePFI);
                if(debug) mediaTest.EmbossedPfiData = buffer;
            }

            if(mediaType.StartsWith("DVD+R", StringComparison.Ordinal) || mediaType == "DVD+MRW")
            {
                DicConsole.WriteLine("Querying DVD ADIP...");
                mediaTest.CanReadADIP = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                               MmcDiscStructureMediaType.Dvd, 0, 0,
                                                               MmcDiscStructureFormat.Adip, 0, dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadADIP);
                if(debug) mediaTest.AdipData = buffer;
                DicConsole.WriteLine("Querying DVD DCB...");
                mediaTest.CanReadDCB = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                              MmcDiscStructureMediaType.Dvd, 0, 0,
                                                              MmcDiscStructureFormat.Dcb, 0, dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDCB);
                if(debug) mediaTest.DcbData = buffer;
            }

            if(mediaType == "HD DVD-ROM")
            {
                DicConsole.WriteLine("Querying HD DVD CMI...");
                mediaTest.CanReadHDCMI = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                MmcDiscStructureFormat.HddvdCopyrightInformation, 0,
                                                                dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadHDCMI);
                if(debug) mediaTest.HdCmiData = buffer;
            }

            if(mediaType.EndsWith(" DL", StringComparison.Ordinal))
            {
                DicConsole.WriteLine("Querying DVD Layer Capacity...");
                mediaTest.CanReadLayerCapacity = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                        MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                        MmcDiscStructureFormat.DvdrLayerCapacity, 0,
                                                                        dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadLayerCapacity);
                if(debug) mediaTest.DvdLayerData = buffer;
            }

            if(mediaType.StartsWith("BD-R", StringComparison.Ordinal) || mediaType == "Ultra HD Blu-ray movie" ||
               mediaType                                                           == "PlayStation 3 game"     ||
               mediaType                                                           == "PlayStation 4 game"     || mediaType == "Xbox One game")
            {
                DicConsole.WriteLine("Querying BD Disc Information...");
                mediaTest.CanReadDiscInformation = !dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                          MmcDiscStructureMediaType.Bd, 0, 0,
                                                                          MmcDiscStructureFormat.DiscInformation, 0,
                                                                          dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDiscInformation);
                if(debug) mediaTest.BluDiData = buffer;
                DicConsole.WriteLine("Querying BD PAC...");
                mediaTest.CanReadPAC = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Bd,
                                                              0, 0, MmcDiscStructureFormat.Pac, 0, dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPAC);
                if(debug) mediaTest.BluPacData = buffer;
            }

            DicConsole.WriteLine("Trying SCSI READ (6)...");
            mediaTest.SupportsRead6 = !dev.Read6(out buffer, out senseBuffer, 0, 2048, dev.Timeout, out _);
            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead6);
            if(debug) mediaTest.Read6Data = buffer;
            DicConsole.WriteLine("Trying SCSI READ (10)...");
            mediaTest.SupportsRead10 = !dev.Read10(out buffer, out senseBuffer, 0, false, true, false, false, 0, 2048,
                                                   0, 1, dev.Timeout, out _);
            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead10);
            if(debug) mediaTest.Read10Data = buffer;
            DicConsole.WriteLine("Trying SCSI READ (12)...");
            mediaTest.SupportsRead12 = !dev.Read12(out buffer, out senseBuffer, 0, false, true, false, false, 0, 2048,
                                                   0, 1, false, dev.Timeout, out _);
            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead12);
            if(debug) mediaTest.Read12Data = buffer;
            DicConsole.WriteLine("Trying SCSI READ (16)...");
            mediaTest.SupportsRead16 = !dev.Read16(out buffer, out senseBuffer, 0, false, true, false, 0, 2048, 0, 1,
                                                   false, dev.Timeout, out _);
            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead16);
            if(debug) mediaTest.Read16Data = buffer;

            if(mediaType.StartsWith("CD-",   StringComparison.Ordinal) ||
               mediaType.StartsWith("DDCD-", StringComparison.Ordinal) || mediaType == "Audio CD")
            {
                if(mediaType == "Audio CD")
                {
                    DicConsole.WriteLine("Trying SCSI READ CD...");
                    mediaTest.SupportsReadCd = !dev.ReadCd(out buffer, out senseBuffer, 0, 2352, 1, MmcSectorTypes.Cdda,
                                                           false, false, false, MmcHeaderCodes.None, true, false,
                                                           MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCd);
                    if(debug) mediaTest.ReadCdFullData = buffer;
                    DicConsole.WriteLine("Trying SCSI READ CD MSF...");
                    mediaTest.SupportsReadCdMsf = !dev.ReadCdMsf(out buffer, out senseBuffer, 0x00000200, 0x00000201,
                                                                 2352, MmcSectorTypes.Cdda, false, false,
                                                                 MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                                 MmcSubchannel.None, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCdMsf);
                    if(debug) mediaTest.ReadCdMsfFullData = buffer;
                }
                else
                {
                    DicConsole.WriteLine("Trying SCSI READ CD...");
                    mediaTest.SupportsReadCd = !dev.ReadCd(out buffer, out senseBuffer, 0, 2048, 1,
                                                           MmcSectorTypes.AllTypes, false, false, false,
                                                           MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                           MmcSubchannel.None, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCd);
                    if(debug) mediaTest.ReadCdData = buffer;
                    DicConsole.WriteLine("Trying SCSI READ CD MSF...");
                    mediaTest.SupportsReadCdMsf = !dev.ReadCdMsf(out buffer, out senseBuffer, 0x00000200, 0x00000201,
                                                                 2048, MmcSectorTypes.AllTypes, false, false,
                                                                 MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                                 MmcSubchannel.None, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCdMsf);
                    if(debug) mediaTest.ReadCdMsfData = buffer;
                    DicConsole.WriteLine("Trying SCSI READ CD full sector...");
                    mediaTest.SupportsReadCdRaw = !dev.ReadCd(out buffer, out senseBuffer, 0, 2352, 1,
                                                              MmcSectorTypes.AllTypes, false, false, true,
                                                              MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                                              MmcSubchannel.None, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCdRaw);
                    if(debug) mediaTest.ReadCdFullData = buffer;
                    DicConsole.WriteLine("Trying SCSI READ CD MSF full sector...");
                    mediaTest.SupportsReadCdMsfRaw = !dev.ReadCdMsf(out buffer, out senseBuffer, 0x00000200, 0x00000201,
                                                                    2352, MmcSectorTypes.AllTypes, false, false,
                                                                    MmcHeaderCodes.AllHeaders, true, true,
                                                                    MmcErrorField.None, MmcSubchannel.None, dev.Timeout,
                                                                    out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCdMsfRaw);
                    if(debug) mediaTest.ReadCdMsfFullData = buffer;
                }

                if(mediaTest.SupportsReadCdRaw == true || mediaType == "Audio CD")
                {
                    DicConsole.WriteLine("Trying to read CD Track 1 pregap...");
                    for(int i = -150; i < 0; i++)
                    {
                        if(mediaType == "Audio CD")
                            sense = dev.ReadCd(out buffer, out senseBuffer, (uint)i, 2352, 1, MmcSectorTypes.Cdda,
                                               false, false, false, MmcHeaderCodes.None, true, false,
                                               MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);
                        else
                            sense = dev.ReadCd(out buffer, out senseBuffer, (uint)i, 2352, 1, MmcSectorTypes.AllTypes,
                                               false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                               MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);
                        DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", sense);
                        if(debug) mediaTest.Track1PregapData = buffer;
                        if(sense) continue;

                        mediaTest.CanReadFirstTrackPreGap = true;
                        break;
                    }

                    DicConsole.WriteLine("Trying to read CD Lead-In...");
                    foreach(int i in new[] {-5000, -4000, -3000, -2000, -1000, -500, -250})
                    {
                        if(mediaType == "Audio CD")
                            sense = dev.ReadCd(out buffer, out senseBuffer, (uint)i, 2352, 1, MmcSectorTypes.Cdda,
                                               false, false, false, MmcHeaderCodes.None, true, false,
                                               MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);
                        else
                            sense = dev.ReadCd(out buffer, out senseBuffer, (uint)i, 2352, 1, MmcSectorTypes.AllTypes,
                                               false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                               MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);
                        DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", sense);
                        if(debug) mediaTest.LeadInData = buffer;
                        if(sense) continue;

                        mediaTest.CanReadLeadIn = true;
                        break;
                    }

                    DicConsole.WriteLine("Trying to read CD Lead-Out...");
                    if(mediaType == "Audio CD")
                        mediaTest.CanReadLeadOut = !dev.ReadCd(out buffer, out senseBuffer,
                                                               (uint)(mediaTest.Blocks + 1), 2352, 1,
                                                               MmcSectorTypes.Cdda, false, false, false,
                                                               MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                               MmcSubchannel.None, dev.Timeout, out _);
                    else
                        mediaTest.CanReadLeadOut = !dev.ReadCd(out buffer, out senseBuffer,
                                                               (uint)(mediaTest.Blocks + 1), 2352, 1,
                                                               MmcSectorTypes.AllTypes, false, false, true,
                                                               MmcHeaderCodes.AllHeaders, true, true,
                                                               MmcErrorField.None, MmcSubchannel.None, dev.Timeout,
                                                               out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadLeadOut);
                    if(debug) mediaTest.LeadOutData = buffer;
                }

                if(mediaType == "Audio CD")
                {
                    DicConsole.WriteLine("Trying to read C2 Pointers...");
                    mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2646, 1,
                                                              MmcSectorTypes.Cdda, false, false, false,
                                                              MmcHeaderCodes.None, true, false,
                                                              MmcErrorField.C2Pointers, MmcSubchannel.None, dev.Timeout,
                                                              out _);
                    if(!mediaTest.CanReadC2Pointers == true)
                        mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2648, 1,
                                                                  MmcSectorTypes.Cdda, false, false, false,
                                                                  MmcHeaderCodes.None, true, false,
                                                                  MmcErrorField.C2PointersAndBlock, MmcSubchannel.None,
                                                                  dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadC2Pointers);
                    if(debug) mediaTest.C2PointersData = buffer;

                    DicConsole.WriteLine("Trying to read subchannels...");
                    mediaTest.CanReadPQSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2368, 1,
                                                                MmcSectorTypes.Cdda, false, false, false,
                                                                MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                                MmcSubchannel.Q16, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannel);
                    if(debug) mediaTest.PQSubchannelData = buffer;
                    mediaTest.CanReadRWSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2448, 1,
                                                                MmcSectorTypes.Cdda, false, false, false,
                                                                MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                                MmcSubchannel.Raw, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannel);
                    if(debug) mediaTest.RWSubchannelData = buffer;
                    mediaTest.CanReadCorrectedSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2448, 1,
                                                                       MmcSectorTypes.Cdda, false, false, false,
                                                                       MmcHeaderCodes.None, true, false,
                                                                       MmcErrorField.None, MmcSubchannel.Rw,
                                                                       dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadCorrectedSubchannel);
                    if(debug) mediaTest.CorrectedSubchannelData = buffer;

                    DicConsole.WriteLine("Trying to read subchannels with C2 Pointers...");
                    mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2662, 1,
                                                                      MmcSectorTypes.Cdda, false, false, false,
                                                                      MmcHeaderCodes.None, true, false,
                                                                      MmcErrorField.C2Pointers, MmcSubchannel.Q16,
                                                                      dev.Timeout, out _);
                    if(mediaTest.CanReadPQSubchannelWithC2 == false)
                        mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2664, 1,
                                                                          MmcSectorTypes.Cdda, false, false, false,
                                                                          MmcHeaderCodes.None, true, false,
                                                                          MmcErrorField.C2PointersAndBlock,
                                                                          MmcSubchannel.Q16, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannelWithC2);
                    if(debug) mediaTest.PQSubchannelWithC2Data = buffer;

                    mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2712, 1,
                                                                      MmcSectorTypes.Cdda, false, false, false,
                                                                      MmcHeaderCodes.None, true, false,
                                                                      MmcErrorField.C2Pointers, MmcSubchannel.Raw,
                                                                      dev.Timeout, out _);
                    if(mediaTest.CanReadRWSubchannelWithC2 == false)
                        mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2714, 1,
                                                                          MmcSectorTypes.Cdda, false, false, false,
                                                                          MmcHeaderCodes.None, true, false,
                                                                          MmcErrorField.C2PointersAndBlock,
                                                                          MmcSubchannel.Raw, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannelWithC2);
                    if(debug) mediaTest.RWSubchannelWithC2Data = buffer;

                    mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2712, 1,
                                                                             MmcSectorTypes.Cdda, false, false, false,
                                                                             MmcHeaderCodes.None, true, false,
                                                                             MmcErrorField.C2Pointers, MmcSubchannel.Rw,
                                                                             dev.Timeout, out _);
                    if(mediaTest.CanReadCorrectedSubchannelWithC2 == false)
                        mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2714,
                                                                                 1, MmcSectorTypes.Cdda, false, false,
                                                                                 false, MmcHeaderCodes.None, true,
                                                                                 false,
                                                                                 MmcErrorField.C2PointersAndBlock,
                                                                                 MmcSubchannel.Rw, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                              !mediaTest.CanReadCorrectedSubchannelWithC2);
                    if(debug) mediaTest.CorrectedSubchannelWithC2Data = buffer;
                }
                else if(mediaTest.SupportsReadCdRaw == true)
                {
                    DicConsole.WriteLine("Trying to read C2 Pointers...");
                    mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2646, 1,
                                                              MmcSectorTypes.AllTypes, false, false, true,
                                                              MmcHeaderCodes.AllHeaders, true, true,
                                                              MmcErrorField.C2Pointers, MmcSubchannel.None, dev.Timeout,
                                                              out _);
                    if(mediaTest.CanReadC2Pointers == false)
                        mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2648, 1,
                                                                  MmcSectorTypes.AllTypes, false, false, true,
                                                                  MmcHeaderCodes.AllHeaders, true, true,
                                                                  MmcErrorField.C2PointersAndBlock, MmcSubchannel.None,
                                                                  dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadC2Pointers);
                    if(debug) mediaTest.C2PointersData = buffer;

                    DicConsole.WriteLine("Trying to read subchannels...");
                    mediaTest.CanReadPQSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2368, 1,
                                                                MmcSectorTypes.AllTypes, false, false, true,
                                                                MmcHeaderCodes.AllHeaders, true, true,
                                                                MmcErrorField.None, MmcSubchannel.Q16, dev.Timeout,
                                                                out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannel);
                    if(debug) mediaTest.PQSubchannelData = buffer;
                    mediaTest.CanReadRWSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2448, 1,
                                                                MmcSectorTypes.AllTypes, false, false, true,
                                                                MmcHeaderCodes.AllHeaders, true, true,
                                                                MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout,
                                                                out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannel);
                    if(debug) mediaTest.RWSubchannelData = buffer;
                    mediaTest.CanReadCorrectedSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2448, 1,
                                                                       MmcSectorTypes.AllTypes, false, false, true,
                                                                       MmcHeaderCodes.AllHeaders, true, true,
                                                                       MmcErrorField.None, MmcSubchannel.Rw,
                                                                       dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadCorrectedSubchannel);
                    if(debug) mediaTest.CorrectedSubchannelData = buffer;
                    DicConsole.WriteLine("Trying to read subchannels with C2 Pointers...");
                    mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2662, 1,
                                                                      MmcSectorTypes.AllTypes, false, false, true,
                                                                      MmcHeaderCodes.AllHeaders, true, true,
                                                                      MmcErrorField.C2Pointers, MmcSubchannel.Q16,
                                                                      dev.Timeout, out _);
                    if(mediaTest.CanReadPQSubchannelWithC2 == false)
                        mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2664, 1,
                                                                          MmcSectorTypes.AllTypes, false, false, true,
                                                                          MmcHeaderCodes.AllHeaders, true, true,
                                                                          MmcErrorField.C2PointersAndBlock,
                                                                          MmcSubchannel.Q16, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannelWithC2);
                    if(debug) mediaTest.PQSubchannelWithC2Data = buffer;

                    mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2712, 1,
                                                                      MmcSectorTypes.AllTypes, false, false, true,
                                                                      MmcHeaderCodes.AllHeaders, true, true,
                                                                      MmcErrorField.C2Pointers, MmcSubchannel.Raw,
                                                                      dev.Timeout, out _);
                    if(mediaTest.CanReadRWSubchannelWithC2 == false)
                        mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2714, 1,
                                                                          MmcSectorTypes.AllTypes, false, false, true,
                                                                          MmcHeaderCodes.AllHeaders, true, true,
                                                                          MmcErrorField.C2PointersAndBlock,
                                                                          MmcSubchannel.Raw, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannelWithC2);
                    if(debug) mediaTest.RWSubchannelWithC2Data = buffer;

                    mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2712, 1,
                                                                             MmcSectorTypes.AllTypes, false, false,
                                                                             true, MmcHeaderCodes.AllHeaders, true,
                                                                             true, MmcErrorField.C2Pointers,
                                                                             MmcSubchannel.Rw, dev.Timeout, out _);
                    if(mediaTest.CanReadCorrectedSubchannelWithC2 == false)
                        mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2714,
                                                                                 1, MmcSectorTypes.AllTypes, false,
                                                                                 false, true, MmcHeaderCodes.AllHeaders,
                                                                                 true, true,
                                                                                 MmcErrorField.C2PointersAndBlock,
                                                                                 MmcSubchannel.Rw, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                              !mediaTest.CanReadCorrectedSubchannelWithC2);
                    if(debug) mediaTest.CorrectedSubchannelWithC2Data = buffer;
                }
                else
                {
                    DicConsole.WriteLine("Trying to read C2 Pointers...");
                    mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2342, 1,
                                                              MmcSectorTypes.AllTypes, false, false, false,
                                                              MmcHeaderCodes.None, true, false,
                                                              MmcErrorField.C2Pointers, MmcSubchannel.None, dev.Timeout,
                                                              out _);
                    if(mediaTest.CanReadC2Pointers == false)
                        mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2344, 1,
                                                                  MmcSectorTypes.AllTypes, false, false, false,
                                                                  MmcHeaderCodes.None, true, false,
                                                                  MmcErrorField.C2PointersAndBlock, MmcSubchannel.None,
                                                                  dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadC2Pointers);
                    if(debug) mediaTest.C2PointersData = buffer;

                    DicConsole.WriteLine("Trying to read subchannels...");
                    mediaTest.CanReadPQSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2064, 1,
                                                                MmcSectorTypes.AllTypes, false, false, false,
                                                                MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                                MmcSubchannel.Q16, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannel);
                    if(debug) mediaTest.PQSubchannelData = buffer;
                    mediaTest.CanReadRWSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2144, 1,
                                                                MmcSectorTypes.AllTypes, false, false, false,
                                                                MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                                MmcSubchannel.Raw, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannel);
                    if(debug) mediaTest.RWSubchannelData = buffer;
                    mediaTest.CanReadCorrectedSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2144, 1,
                                                                       MmcSectorTypes.AllTypes, false, false, false,
                                                                       MmcHeaderCodes.None, true, false,
                                                                       MmcErrorField.None, MmcSubchannel.Rw,
                                                                       dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadCorrectedSubchannel);
                    if(debug) mediaTest.CorrectedSubchannelData = buffer;

                    DicConsole.WriteLine("Trying to read subchannels with C2 Pointers...");
                    mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2358, 1,
                                                                      MmcSectorTypes.AllTypes, false, false, false,
                                                                      MmcHeaderCodes.None, true, false,
                                                                      MmcErrorField.C2Pointers, MmcSubchannel.Q16,
                                                                      dev.Timeout, out _);
                    if(mediaTest.CanReadPQSubchannelWithC2 == false)
                        mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2360, 1,
                                                                          MmcSectorTypes.AllTypes, false, false, false,
                                                                          MmcHeaderCodes.None, true, false,
                                                                          MmcErrorField.C2PointersAndBlock,
                                                                          MmcSubchannel.Q16, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannelWithC2);
                    if(debug) mediaTest.PQSubchannelWithC2Data = buffer;

                    mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2438, 1,
                                                                      MmcSectorTypes.AllTypes, false, false, false,
                                                                      MmcHeaderCodes.None, true, false,
                                                                      MmcErrorField.C2Pointers, MmcSubchannel.Raw,
                                                                      dev.Timeout, out _);
                    if(mediaTest.CanReadRWSubchannelWithC2 == false)
                        mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2440, 1,
                                                                          MmcSectorTypes.AllTypes, false, false, false,
                                                                          MmcHeaderCodes.None, true, false,
                                                                          MmcErrorField.C2PointersAndBlock,
                                                                          MmcSubchannel.Raw, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannelWithC2);
                    if(debug) mediaTest.RWSubchannelWithC2Data = buffer;

                    mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2438, 1,
                                                                             MmcSectorTypes.AllTypes, false, false,
                                                                             false, MmcHeaderCodes.None, true, false,
                                                                             MmcErrorField.C2Pointers, MmcSubchannel.Rw,
                                                                             dev.Timeout, out _);
                    if(mediaTest.CanReadCorrectedSubchannelWithC2 == false)
                        mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2440,
                                                                                 1, MmcSectorTypes.AllTypes, false,
                                                                                 false, false, MmcHeaderCodes.None,
                                                                                 true, false,
                                                                                 MmcErrorField.C2PointersAndBlock,
                                                                                 MmcSubchannel.Rw, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                              !mediaTest.CanReadCorrectedSubchannelWithC2);
                    if(debug) mediaTest.CorrectedSubchannelWithC2Data = buffer;
                }

                if(tryPlextor)
                {
                    DicConsole.WriteLine("Trying Plextor READ CD-DA...");
                    mediaTest.SupportsPlextorReadCDDA =
                        !dev.PlextorReadCdDa(out buffer, out senseBuffer, 0, 2352, 1, PlextorSubchannel.None,
                                             dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsPlextorReadCDDA);
                    if(debug) mediaTest.PlextorReadCddaData = buffer;
                }

                if(tryPioneer)
                {
                    DicConsole.WriteLine("Trying Pioneer READ CD-DA...");
                    mediaTest.SupportsPioneerReadCDDA =
                        !dev.PioneerReadCdDa(out buffer, out senseBuffer, 0, 2352, 1, PioneerSubchannel.None,
                                             dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsPioneerReadCDDA);
                    if(debug) mediaTest.PioneerReadCddaData = buffer;
                    DicConsole.WriteLine("Trying Pioneer READ CD-DA MSF...");
                    mediaTest.SupportsPioneerReadCDDAMSF =
                        !dev.PioneerReadCdDaMsf(out buffer, out senseBuffer, 0x00000200, 0x00000201, 2352,
                                                PioneerSubchannel.None, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsPioneerReadCDDAMSF);
                    if(debug) mediaTest.PioneerReadCddaMsfData = buffer;
                }

                if(tryNec)
                {
                    DicConsole.WriteLine("Trying NEC READ CD-DA...");
                    mediaTest.SupportsNECReadCDDA =
                        !dev.NecReadCdDa(out buffer, out senseBuffer, 0, 1, dev.Timeout, out _);
                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsNECReadCDDA);
                    if(debug) mediaTest.NecReadCddaData = buffer;
                }
            }

            mediaTest.LongBlockSize = mediaTest.BlockSize;
            DicConsole.WriteLine("Trying SCSI READ LONG (10)...");
            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 0xFFFF, dev.Timeout, out _);
            if(sense && !dev.Error)
            {
                FixedSense? decSense = Sense.DecodeFixed(senseBuffer);
                if(decSense.HasValue)
                    if(decSense.Value.SenseKey == SenseKeys.IllegalRequest && decSense.Value.ASC == 0x24 &&
                       decSense.Value.ASCQ     == 0x00)
                    {
                        mediaTest.SupportsReadLong = true;
                        if(decSense.Value.InformationValid && decSense.Value.ILI)
                            mediaTest.LongBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);
                    }
            }

            if(mediaTest.SupportsReadLong == true && mediaTest.LongBlockSize == mediaTest.BlockSize)
            {
                // DVDs
                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 37856, dev.Timeout, out _);
                if(!sense && !dev.Error)
                {
                    mediaTest.ReadLong10Data   = buffer;
                    mediaTest.SupportsReadLong = true;
                    mediaTest.LongBlockSize    = 37856;
                }
            }

            if(tryPlextor)
            {
                DicConsole.WriteLine("Trying Plextor trick to raw read DVDs...");
                mediaTest.SupportsPlextorReadRawDVD =
                    !dev.PlextorReadRawDvd(out buffer, out senseBuffer, 0, 1, dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsPlextorReadRawDVD);
                if(mediaTest.SupportsPlextorReadRawDVD == true)
                    mediaTest.SupportsPlextorReadRawDVD = !ArrayHelpers.ArrayIsNullOrEmpty(buffer);

                if(mediaTest.SupportsPlextorReadRawDVD == true && debug) mediaTest.PlextorReadRawDVDData = buffer;
            }

            if(!tryHldtst) return mediaTest;

            DicConsole.WriteLine("Trying HL-DT-ST (aka LG) trick to raw read DVDs...");
            mediaTest.SupportsHLDTSTReadRawDVD =
                !dev.HlDtStReadRawDvd(out buffer, out senseBuffer, 0, 1, dev.Timeout, out _);
            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsHLDTSTReadRawDVD);

            if(mediaTest.SupportsHLDTSTReadRawDVD == true)
                mediaTest.SupportsHLDTSTReadRawDVD = !ArrayHelpers.ArrayIsNullOrEmpty(buffer);

            if(mediaTest.SupportsHLDTSTReadRawDVD == true && debug) mediaTest.HLDTSTReadRawDVDData = buffer;

            // This is for checking multi-session support, and inter-session lead-in/out reading, as Enhanced CD are
            if(mediaType == "Enhanced CD (aka E-CD, CD-Plus or CD+)")
            {
                DicConsole.WriteLine("Querying CD Full TOC...");
                mediaTest.CanReadFullTOC = !dev.ReadRawToc(out buffer, out senseBuffer, 1, dev.Timeout, out _);
                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadFullTOC);
                if(debug) mediaTest.FullTocData = buffer;

                if(mediaTest.CanReadFullTOC == true)
                {
                    FullTOC.CDFullTOC? decodedTocNullable = FullTOC.Decode(buffer);

                    mediaTest.CanReadFullTOC = decodedTocNullable.HasValue;

                    if(mediaTest.CanReadFullTOC == true)
                    {
                        FullTOC.CDFullTOC decodedToc = decodedTocNullable.Value;

                        if(!decodedToc.TrackDescriptors.Any(t => t.SessionNumber > 1))
                        {
                            DicConsole
                               .ErrorWriteLine("Could not find second session. Have you inserted the correct type of disc?");
                            return null;
                        }

                        FullTOC.TrackDataDescriptor firstSessionLeadOutTrack =
                            decodedToc.TrackDescriptors.FirstOrDefault(t => t.SessionNumber == 1 && t.POINT == 0xA2);
                        FullTOC.TrackDataDescriptor secondSessionFirstTrack =
                            decodedToc.TrackDescriptors.FirstOrDefault(t => t.SessionNumber > 1 && t.POINT <= 99);

                        if(firstSessionLeadOutTrack.SessionNumber == 0 || secondSessionFirstTrack.SessionNumber == 0)
                        {
                            DicConsole
                               .ErrorWriteLine("Could not find second session. Have you inserted the correct type of disc?");
                            return null;
                        }

                        DicConsole.DebugWriteLine("SCSI Report",
                                                  "First session Lead-Out starts at {0:D2}:{1:D2}:{2:D2}",
                                                  firstSessionLeadOutTrack.PMIN, firstSessionLeadOutTrack.PSEC,
                                                  firstSessionLeadOutTrack.PFRAME);
                        DicConsole.DebugWriteLine("SCSI Report", "Second session starts at {0:D2}:{1:D2}:{2:D2}",
                                                  secondSessionFirstTrack.PMIN, secondSessionFirstTrack.PSEC,
                                                  secondSessionFirstTrack.PFRAME);

                        // Skip Lead-Out pre-gap
                        uint firstSessionLeadOutLba = (uint)(firstSessionLeadOutTrack.PMIN * 60 * 75 +
                                                             firstSessionLeadOutTrack.PSEC      * 75 +
                                                             firstSessionLeadOutTrack.PFRAME         + 150);

                        // Skip second session track pre-gap
                        uint secondSessionLeadInLba = (uint)(secondSessionFirstTrack.PMIN * 60 * 75 +
                                                             secondSessionFirstTrack.PSEC      * 75 +
                                                             secondSessionFirstTrack.PFRAME - 300);

                        DicConsole.WriteLine("Trying SCSI READ CD in first session Lead-Out...");
                        mediaTest.CanReadingIntersessionLeadOut = !dev.ReadCd(out buffer, out senseBuffer,
                                                                              firstSessionLeadOutLba, 2448, 1,
                                                                              MmcSectorTypes.AllTypes, false, false,
                                                                              false, MmcHeaderCodes.AllHeaders, true,
                                                                              false, MmcErrorField.None,
                                                                              MmcSubchannel.Raw, dev.Timeout, out _);

                        if(mediaTest.CanReadingIntersessionLeadOut == false)
                        {
                            mediaTest.CanReadingIntersessionLeadOut = !dev.ReadCd(out buffer, out senseBuffer,
                                                                                  firstSessionLeadOutLba, 2368, 1,
                                                                                  MmcSectorTypes.AllTypes, false, false,
                                                                                  false, MmcHeaderCodes.AllHeaders,
                                                                                  true, false, MmcErrorField.None,
                                                                                  MmcSubchannel.Q16, dev.Timeout,
                                                                                  out _);

                            if(mediaTest.CanReadingIntersessionLeadOut == false)
                                mediaTest.CanReadingIntersessionLeadOut = !dev.ReadCd(out buffer, out senseBuffer,
                                                                                      firstSessionLeadOutLba, 2352, 1,
                                                                                      MmcSectorTypes.AllTypes, false,
                                                                                      false, false,
                                                                                      MmcHeaderCodes.AllHeaders, true,
                                                                                      false, MmcErrorField.None,
                                                                                      MmcSubchannel.None, dev.Timeout,
                                                                                      out _);
                        }

                        DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                  !mediaTest.CanReadingIntersessionLeadOut);
                        if(debug) mediaTest.IntersessionLeadOutData = buffer;

                        DicConsole.WriteLine("Trying SCSI READ CD in second session Lead-In...");
                        mediaTest.CanReadingIntersessionLeadIn = !dev.ReadCd(out buffer, out senseBuffer,
                                                                             secondSessionLeadInLba, 2448, 1,
                                                                             MmcSectorTypes.AllTypes, false, false,
                                                                             false, MmcHeaderCodes.AllHeaders, true,
                                                                             false, MmcErrorField.None,
                                                                             MmcSubchannel.Raw, dev.Timeout, out _);

                        if(mediaTest.CanReadingIntersessionLeadIn == false)
                        {
                            mediaTest.CanReadingIntersessionLeadIn = !dev.ReadCd(out buffer, out senseBuffer,
                                                                                 secondSessionLeadInLba, 2368, 1,
                                                                                 MmcSectorTypes.AllTypes, false, false,
                                                                                 false, MmcHeaderCodes.AllHeaders, true,
                                                                                 false, MmcErrorField.None,
                                                                                 MmcSubchannel.Q16, dev.Timeout, out _);

                            if(mediaTest.CanReadingIntersessionLeadIn == false)
                                mediaTest.CanReadingIntersessionLeadIn = !dev.ReadCd(out buffer, out senseBuffer,
                                                                                     secondSessionLeadInLba, 2352, 1,
                                                                                     MmcSectorTypes.AllTypes, false,
                                                                                     false, false,
                                                                                     MmcHeaderCodes.AllHeaders, true,
                                                                                     false, MmcErrorField.None,
                                                                                     MmcSubchannel.None, dev.Timeout,
                                                                                     out _);
                        }

                        DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                  !mediaTest.CanReadingIntersessionLeadIn);
                        if(debug) mediaTest.IntersessionLeadInData = buffer;
                    }
                }
            }

            return mediaTest;
        }
    }
}