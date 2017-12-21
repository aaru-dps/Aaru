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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using DiscImageChef.Console;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Devices;
using DiscImageChef.Metadata;

namespace DiscImageChef.Core.Devices.Report.SCSI
{
    static class Mmc
    {
        internal static void Report(Device dev, ref DeviceReport report, bool debug,
                                    ref Modes.ModePage_2A? cdromMode, ref List<string> mediaTypes)
        {
            if(report == null) return;

            byte[] senseBuffer;
            byte[] buffer;
            double duration;
            bool sense;
            uint timeout = 5;
            ConsoleKeyInfo pressedKey;
            Modes.DecodedMode? decMode;

            report.SCSI.MultiMediaDevice = new mmcType();

            if(cdromMode.HasValue)
            {
                report.SCSI.MultiMediaDevice.ModeSense2A = new mmcModeType();
                if(cdromMode.Value.BufferSize != 0)
                {
                    report.SCSI.MultiMediaDevice.ModeSense2A.BufferSize = cdromMode.Value.BufferSize;
                    report.SCSI.MultiMediaDevice.ModeSense2A.BufferSizeSpecified = true;
                }
                if(cdromMode.Value.CurrentSpeed != 0)
                {
                    report.SCSI.MultiMediaDevice.ModeSense2A.CurrentSpeed = cdromMode.Value.CurrentSpeed;
                    report.SCSI.MultiMediaDevice.ModeSense2A.CurrentSpeedSpecified = true;
                }
                if(cdromMode.Value.CurrentWriteSpeed != 0)
                {
                    report.SCSI.MultiMediaDevice.ModeSense2A.CurrentWriteSpeed = cdromMode.Value.CurrentWriteSpeed;
                    report.SCSI.MultiMediaDevice.ModeSense2A.CurrentWriteSpeedSpecified = true;
                }
                if(cdromMode.Value.CurrentWriteSpeedSelected != 0)
                {
                    report.SCSI.MultiMediaDevice.ModeSense2A.CurrentWriteSpeedSelected =
                        cdromMode.Value.CurrentWriteSpeedSelected;
                    report.SCSI.MultiMediaDevice.ModeSense2A.CurrentWriteSpeedSelectedSpecified = true;
                }
                if(cdromMode.Value.MaximumSpeed != 0)
                {
                    report.SCSI.MultiMediaDevice.ModeSense2A.MaximumSpeed = cdromMode.Value.MaximumSpeed;
                    report.SCSI.MultiMediaDevice.ModeSense2A.MaximumSpeedSpecified = true;
                }
                if(cdromMode.Value.MaxWriteSpeed != 0)
                {
                    report.SCSI.MultiMediaDevice.ModeSense2A.MaximumWriteSpeed = cdromMode.Value.MaxWriteSpeed;
                    report.SCSI.MultiMediaDevice.ModeSense2A.MaximumWriteSpeedSpecified = true;
                }
                if(cdromMode.Value.RotationControlSelected != 0)
                {
                    report.SCSI.MultiMediaDevice.ModeSense2A.RotationControlSelected =
                        cdromMode.Value.RotationControlSelected;
                    report.SCSI.MultiMediaDevice.ModeSense2A.RotationControlSelectedSpecified = true;
                }
                if(cdromMode.Value.SupportedVolumeLevels != 0)
                {
                    report.SCSI.MultiMediaDevice.ModeSense2A.SupportedVolumeLevels =
                        cdromMode.Value.SupportedVolumeLevels;
                    report.SCSI.MultiMediaDevice.ModeSense2A.SupportedVolumeLevelsSpecified = true;
                }

                report.SCSI.MultiMediaDevice.ModeSense2A.AccurateCDDA = cdromMode.Value.AccurateCDDA;
                report.SCSI.MultiMediaDevice.ModeSense2A.BCK = cdromMode.Value.BCK;
                report.SCSI.MultiMediaDevice.ModeSense2A.BufferUnderRunProtection = cdromMode.Value.BUF;
                report.SCSI.MultiMediaDevice.ModeSense2A.CanEject = cdromMode.Value.Eject;
                report.SCSI.MultiMediaDevice.ModeSense2A.CanLockMedia = cdromMode.Value.Lock;
                report.SCSI.MultiMediaDevice.ModeSense2A.CDDACommand = cdromMode.Value.CDDACommand;
                report.SCSI.MultiMediaDevice.ModeSense2A.CompositeAudioVideo = cdromMode.Value.Composite;
                report.SCSI.MultiMediaDevice.ModeSense2A.CSSandCPPMSupported = cdromMode.Value.CMRSupported == 1;
                report.SCSI.MultiMediaDevice.ModeSense2A.DeterministicSlotChanger = cdromMode.Value.SDP;
                report.SCSI.MultiMediaDevice.ModeSense2A.DigitalPort1 = cdromMode.Value.DigitalPort1;
                report.SCSI.MultiMediaDevice.ModeSense2A.DigitalPort2 = cdromMode.Value.DigitalPort2;
                report.SCSI.MultiMediaDevice.ModeSense2A.LeadInPW = cdromMode.Value.LeadInPW;
                report.SCSI.MultiMediaDevice.ModeSense2A.LoadingMechanismType = cdromMode.Value.LoadingMechanism;
                report.SCSI.MultiMediaDevice.ModeSense2A.LockStatus = cdromMode.Value.LockState;
                report.SCSI.MultiMediaDevice.ModeSense2A.LSBF = cdromMode.Value.LSBF;
                report.SCSI.MultiMediaDevice.ModeSense2A.PlaysAudio = cdromMode.Value.AudioPlay;
                report.SCSI.MultiMediaDevice.ModeSense2A.PreventJumperStatus = cdromMode.Value.PreventJumper;
                report.SCSI.MultiMediaDevice.ModeSense2A.RCK = cdromMode.Value.RCK;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsBarcode = cdromMode.Value.ReadBarcode;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsBothSides = cdromMode.Value.SCC;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsCDR = cdromMode.Value.ReadCDR;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsCDRW = cdromMode.Value.ReadCDRW;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsDeinterlavedSubchannel =
                    cdromMode.Value.DeinterlaveSubchannel;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsDVDR = cdromMode.Value.ReadDVDR;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsDVDRAM = cdromMode.Value.ReadDVDRAM;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsDVDROM = cdromMode.Value.ReadDVDROM;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsISRC = cdromMode.Value.ISRC;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsMode2Form2 = cdromMode.Value.Mode2Form2;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsMode2Form1 = cdromMode.Value.Mode2Form1;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsPacketCDR = cdromMode.Value.Method2;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsSubchannel = cdromMode.Value.Subchannel;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReadsUPC = cdromMode.Value.UPC;
                report.SCSI.MultiMediaDevice.ModeSense2A.ReturnsC2Pointers = cdromMode.Value.C2Pointer;
                report.SCSI.MultiMediaDevice.ModeSense2A.SeparateChannelMute = cdromMode.Value.SeparateChannelMute;
                report.SCSI.MultiMediaDevice.ModeSense2A.SeparateChannelVolume = cdromMode.Value.SeparateChannelVolume;
                report.SCSI.MultiMediaDevice.ModeSense2A.SSS = cdromMode.Value.SSS;
                report.SCSI.MultiMediaDevice.ModeSense2A.SupportsMultiSession = cdromMode.Value.MultiSession;
                report.SCSI.MultiMediaDevice.ModeSense2A.TestWrite = cdromMode.Value.TestWrite;
                report.SCSI.MultiMediaDevice.ModeSense2A.WritesCDR = cdromMode.Value.WriteCDR;
                report.SCSI.MultiMediaDevice.ModeSense2A.WritesCDRW = cdromMode.Value.WriteCDRW;
                report.SCSI.MultiMediaDevice.ModeSense2A.WritesDVDR = cdromMode.Value.WriteDVDR;
                report.SCSI.MultiMediaDevice.ModeSense2A.WritesDVDRAM = cdromMode.Value.WriteDVDRAM;
                report.SCSI.MultiMediaDevice.ModeSense2A.WriteSpeedPerformanceDescriptors =
                    cdromMode.Value.WriteSpeedPerformanceDescriptors;

                mediaTypes.Add("CD-ROM");
                mediaTypes.Add("Audio CD");
                if(cdromMode.Value.ReadCDR) mediaTypes.Add("CD-R");
                if(cdromMode.Value.ReadCDRW) mediaTypes.Add("CD-RW");
                if(cdromMode.Value.ReadDVDROM) mediaTypes.Add("DVD-ROM");
                if(cdromMode.Value.ReadDVDRAM) mediaTypes.Add("DVD-RAM");
                if(cdromMode.Value.ReadDVDR) mediaTypes.Add("DVD-R");
            }

            DicConsole.WriteLine("Querying MMC GET CONFIGURATION...");
            sense = dev.GetConfiguration(out buffer, out senseBuffer, timeout, out duration);

            if(!sense)
            {
                Features.SeparatedFeatures ftr = Features.Separate(buffer);
                if(ftr.Descriptors != null && ftr.Descriptors.Length > 0)
                {
                    report.SCSI.MultiMediaDevice.Features = new mmcFeaturesType();
                    foreach(Features.FeatureDescriptor desc in ftr.Descriptors)
                        switch(desc.Code)
                        {
                            case 0x0001:
                            {
                                Feature_0001? ftr0001 =
                                    Features.Decode_0001(desc.Data);
                                if(ftr0001.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.PhysicalInterfaceStandard =
                                        ftr0001.Value.PhysicalInterfaceStandard;
                                    report.SCSI.MultiMediaDevice.Features.PhysicalInterfaceStandardSpecified = true;
                                    if((uint)ftr0001.Value.PhysicalInterfaceStandard > 0x8)
                                    {
                                        report.SCSI.MultiMediaDevice.Features.PhysicalInterfaceStandardNumber =
                                            (uint)ftr0001.Value.PhysicalInterfaceStandard;
                                        report.SCSI.MultiMediaDevice.Features.PhysicalInterfaceStandardNumberSpecified =
                                            true;
                                        report.SCSI.MultiMediaDevice.Features.PhysicalInterfaceStandard =
                                            PhysicalInterfaces.Unspecified;
                                    }
                                    report.SCSI.MultiMediaDevice.Features.SupportsDeviceBusyEvent = ftr0001.Value.DBE;
                                }
                            }
                                break;
                            case 0x0003:
                            {
                                Feature_0003? ftr0003 =
                                    Features.Decode_0003(desc.Data);
                                if(ftr0003.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.LoadingMechanismType =
                                        ftr0003.Value.LoadingMechanismType;
                                    report.SCSI.MultiMediaDevice.Features.LoadingMechanismTypeSpecified = true;
                                    report.SCSI.MultiMediaDevice.Features.CanLoad = ftr0003.Value.Load;
                                    report.SCSI.MultiMediaDevice.Features.CanEject = ftr0003.Value.Eject;
                                    report.SCSI.MultiMediaDevice.Features.PreventJumper = ftr0003.Value.PreventJumper;
                                    report.SCSI.MultiMediaDevice.Features.DBML = ftr0003.Value.DBML;
                                    report.SCSI.MultiMediaDevice.Features.Locked = ftr0003.Value.Lock;
                                }
                            }
                                break;
                            case 0x0004:
                            {
                                Feature_0004? ftr0004 =
                                    Features.Decode_0004(desc.Data);
                                if(ftr0004.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.SupportsWriteProtectPAC = ftr0004.Value.DWP;
                                    report.SCSI.MultiMediaDevice.Features.SupportsWriteInhibitDCB = ftr0004.Value.WDCB;
                                    report.SCSI.MultiMediaDevice.Features.SupportsPWP = ftr0004.Value.SPWP;
                                    report.SCSI.MultiMediaDevice.Features.SupportsSWPP = ftr0004.Value.SSWPP;
                                }
                            }
                                break;
                            case 0x0010:
                            {
                                Feature_0010? ftr0010 =
                                    Features.Decode_0010(desc.Data);
                                if(ftr0010.HasValue)
                                {
                                    if(ftr0010.Value.LogicalBlockSize > 0)
                                    {
                                        report.SCSI.MultiMediaDevice.Features.LogicalBlockSize =
                                            ftr0010.Value.LogicalBlockSize;
                                        report.SCSI.MultiMediaDevice.Features.LogicalBlockSizeSpecified = true;
                                    }
                                    if(ftr0010.Value.Blocking > 0)
                                    {
                                        report.SCSI.MultiMediaDevice.Features.BlocksPerReadableUnit =
                                            ftr0010.Value.Blocking;
                                        report.SCSI.MultiMediaDevice.Features.BlocksPerReadableUnitSpecified = true;
                                    }
                                    report.SCSI.MultiMediaDevice.Features.ErrorRecoveryPage = ftr0010.Value.PP;
                                }
                            }
                                break;
                            case 0x001D:
                                report.SCSI.MultiMediaDevice.Features.MultiRead = true;
                                break;
                            case 0x001E:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanReadCD = true;
                                Feature_001E? ftr001E =
                                    Features.Decode_001E(desc.Data);
                                if(ftr001E.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.SupportsDAP = ftr001E.Value.DAP;
                                    report.SCSI.MultiMediaDevice.Features.SupportsC2 = ftr001E.Value.C2;
                                    report.SCSI.MultiMediaDevice.Features.CanReadLeadInCDText = ftr001E.Value.CDText;
                                }
                            }
                                break;
                            case 0x001F:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanReadDVD = true;
                                Feature_001F? ftr001F =
                                    Features.Decode_001F(desc.Data);
                                if(ftr001F.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.DVDMultiRead = ftr001F.Value.MULTI110;
                                    report.SCSI.MultiMediaDevice.Features.CanReadAllDualRW = ftr001F.Value.DualRW;
                                    report.SCSI.MultiMediaDevice.Features.CanReadAllDualR = ftr001F.Value.DualR;
                                }
                            }
                                break;
                            case 0x0022:
                                report.SCSI.MultiMediaDevice.Features.CanEraseSector = true;
                                break;
                            case 0x0023:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanFormat = true;
                                Feature_0023? ftr0023 =
                                    Features.Decode_0023(desc.Data);
                                if(ftr0023.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.CanFormatBDREWithoutSpare =
                                        ftr0023.Value.RENoSA;
                                    report.SCSI.MultiMediaDevice.Features.CanExpandBDRESpareArea = ftr0023.Value.Expand;
                                    report.SCSI.MultiMediaDevice.Features.CanFormatQCert = ftr0023.Value.QCert;
                                    report.SCSI.MultiMediaDevice.Features.CanFormatCert = ftr0023.Value.Cert;
                                    report.SCSI.MultiMediaDevice.Features.CanFormatFRF = ftr0023.Value.FRF;
                                    report.SCSI.MultiMediaDevice.Features.CanFormatRRM = ftr0023.Value.RRM;
                                }
                            }
                                break;
                            case 0x0024:
                                report.SCSI.MultiMediaDevice.Features.CanReadSpareAreaInformation = true;
                                break;
                            case 0x0027:
                                report.SCSI.MultiMediaDevice.Features.CanWriteCDRWCAV = true;
                                break;
                            case 0x0028:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanReadCDMRW = true;
                                Feature_0028? ftr0028 =
                                    Features.Decode_0028(desc.Data);
                                if(ftr0028.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusMRW = ftr0028.Value.DVDPRead;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteDVDPlusMRW = ftr0028.Value.DVDPWrite;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteCDMRW = ftr0028.Value.Write;
                                }
                            }
                                break;
                            case 0x002A:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRW = true;
                                Feature_002A? ftr002A =
                                    Features.Decode_002A(desc.Data);
                                if(ftr002A.HasValue) report.SCSI.MultiMediaDevice.Features.CanWriteDVDPlusRW = ftr002A.Value.Write;
                            }
                                break;
                            case 0x002B:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusR = true;
                                Feature_002B? ftr002B =
                                    Features.Decode_002B(desc.Data);
                                if(ftr002B.HasValue) report.SCSI.MultiMediaDevice.Features.CanWriteDVDPlusR = ftr002B.Value.Write;
                            }
                                break;
                            case 0x002D:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanWriteCDTAO = true;
                                Feature_002D? ftr002D =
                                    Features.Decode_002D(desc.Data);
                                if(ftr002D.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.BufferUnderrunFreeInTAO = ftr002D.Value.BUF;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteRawSubchannelInTAO =
                                        ftr002D.Value.RWRaw;
                                    report.SCSI.MultiMediaDevice.Features.CanWritePackedSubchannelInTAO =
                                        ftr002D.Value.RWPack;
                                    report.SCSI.MultiMediaDevice.Features.CanTestWriteInTAO = ftr002D.Value.TestWrite;
                                    report.SCSI.MultiMediaDevice.Features.CanOverwriteTAOTrack = ftr002D.Value.CDRW;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteRWSubchannelInTAO =
                                        ftr002D.Value.RWSubchannel;
                                }
                            }
                                break;
                            case 0x002E:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanWriteCDSAO = true;
                                Feature_002E? ftr002E =
                                    Features.Decode_002E(desc.Data);
                                if(ftr002E.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.BufferUnderrunFreeInSAO = ftr002E.Value.BUF;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteRawMultiSession = ftr002E.Value.RAWMS;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteRaw = ftr002E.Value.RAW;
                                    report.SCSI.MultiMediaDevice.Features.CanTestWriteInSAO = ftr002E.Value.TestWrite;
                                    report.SCSI.MultiMediaDevice.Features.CanOverwriteSAOTrack = ftr002E.Value.CDRW;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteRWSubchannelInSAO = ftr002E.Value.RW;
                                }
                            }
                                break;
                            case 0x002F:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanWriteDVDR = true;
                                Feature_002F? ftr002F =
                                    Features.Decode_002F(desc.Data);
                                if(ftr002F.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.BufferUnderrunFreeInDVD = ftr002F.Value.BUF;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteDVDRDL = ftr002F.Value.RDL;
                                    report.SCSI.MultiMediaDevice.Features.CanTestWriteDVD = ftr002F.Value.TestWrite;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteDVDRW = ftr002F.Value.DVDRW;
                                }
                            }
                                break;
                            case 0x0030:
                                report.SCSI.MultiMediaDevice.Features.CanReadDDCD = true;
                                break;
                            case 0x0031:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanWriteDDCDR = true;
                                Feature_0031? ftr0031 =
                                    Features.Decode_0031(desc.Data);
                                if(ftr0031.HasValue)
                                    report.SCSI.MultiMediaDevice.Features.CanTestWriteDDCDR = ftr0031.Value.TestWrite;
                            }
                                break;
                            case 0x0032:
                                report.SCSI.MultiMediaDevice.Features.CanWriteDDCDRW = true;
                                break;
                            case 0x0037:
                                report.SCSI.MultiMediaDevice.Features.CanWriteCDRW = true;
                                break;
                            case 0x0038:
                                report.SCSI.MultiMediaDevice.Features.CanPseudoOverwriteBDR = true;
                                break;
                            case 0x003A:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRWDL = true;
                                Feature_003A? ftr003A =
                                    Features.Decode_003A(desc.Data);
                                if(ftr003A.HasValue)
                                    report.SCSI.MultiMediaDevice.Features.CanWriteDVDPlusRWDL = ftr003A.Value.Write;
                            }
                                break;
                            case 0x003B:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRDL = true;
                                Feature_003B? ftr003B =
                                    Features.Decode_003B(desc.Data);
                                if(ftr003B.HasValue)
                                    report.SCSI.MultiMediaDevice.Features.CanWriteDVDPlusRDL = ftr003B.Value.Write;
                            }
                                break;
                            case 0x0040:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanReadBD = true;
                                Feature_0040? ftr0040 =
                                    Features.Decode_0040(desc.Data);
                                if(ftr0040.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.CanReadBluBCA = ftr0040.Value.BCA;
                                    report.SCSI.MultiMediaDevice.Features.CanReadBDRE2 = ftr0040.Value.RE2;
                                    report.SCSI.MultiMediaDevice.Features.CanReadBDRE1 = ftr0040.Value.RE1;
                                    report.SCSI.MultiMediaDevice.Features.CanReadOldBDRE = ftr0040.Value.OldRE;
                                    report.SCSI.MultiMediaDevice.Features.CanReadBDR = ftr0040.Value.R;
                                    report.SCSI.MultiMediaDevice.Features.CanReadOldBDR = ftr0040.Value.OldR;
                                    report.SCSI.MultiMediaDevice.Features.CanReadBDROM = ftr0040.Value.ROM;
                                    report.SCSI.MultiMediaDevice.Features.CanReadOldBDROM = ftr0040.Value.OldROM;
                                }
                            }
                                break;
                            case 0x0041:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanWriteBD = true;
                                Feature_0041? ftr0041 =
                                    Features.Decode_0041(desc.Data);
                                if(ftr0041.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.CanWriteBDRE2 = ftr0041.Value.RE2;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteBDRE1 = ftr0041.Value.RE1;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteOldBDRE = ftr0041.Value.OldRE;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteBDR = ftr0041.Value.R;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteOldBDR = ftr0041.Value.OldR;
                                }
                            }
                                break;
                            case 0x0050:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanReadHDDVD = true;
                                Feature_0050? ftr0050 =
                                    Features.Decode_0050(desc.Data);
                                if(ftr0050.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.CanReadHDDVDR = ftr0050.Value.HDDVDR;
                                    report.SCSI.MultiMediaDevice.Features.CanReadHDDVDRAM = ftr0050.Value.HDDVDRAM;
                                }
                            }
                                break;
                            case 0x0051:
                            {
                                // TODO: Write HD DVD-RW
                                Feature_0051? ftr0051 =
                                    Features.Decode_0051(desc.Data);
                                if(ftr0051.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.CanWriteHDDVDR = ftr0051.Value.HDDVDR;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteHDDVDRAM = ftr0051.Value.HDDVDRAM;
                                }
                            }
                                break;
                            case 0x0080:
                                report.SCSI.MultiMediaDevice.Features.SupportsHybridDiscs = true;
                                break;
                            case 0x0101:
                                report.SCSI.MultiMediaDevice.Features.SupportsModePage1Ch = true;
                                break;
                            case 0x0102:
                            {
                                report.SCSI.MultiMediaDevice.Features.EmbeddedChanger = true;
                                Feature_0102? ftr0102 =
                                    Features.Decode_0102(desc.Data);
                                if(ftr0102.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.ChangerIsSideChangeCapable =
                                        ftr0102.Value.SCC;
                                    report.SCSI.MultiMediaDevice.Features.ChangerSupportsDiscPresent =
                                        ftr0102.Value.SDP;
                                    report.SCSI.MultiMediaDevice.Features.ChangerSlots =
                                        (byte)(ftr0102.Value.HighestSlotNumber + 1);
                                }
                            }
                                break;
                            case 0x0103:
                            {
                                report.SCSI.MultiMediaDevice.Features.CanPlayCDAudio = true;
                                Feature_0103? ftr0103 =
                                    Features.Decode_0103(desc.Data);
                                if(ftr0103.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.CanAudioScan = ftr0103.Value.Scan;
                                    report.SCSI.MultiMediaDevice.Features.CanMuteSeparateChannels = ftr0103.Value.SCM;
                                    report.SCSI.MultiMediaDevice.Features.SupportsSeparateVolume = ftr0103.Value.SV;
                                    if(ftr0103.Value.VolumeLevels > 0)
                                    {
                                        report.SCSI.MultiMediaDevice.Features.VolumeLevelsSpecified = true;
                                        report.SCSI.MultiMediaDevice.Features.VolumeLevels = ftr0103.Value.VolumeLevels;
                                    }
                                }
                            }
                                break;
                            case 0x0104:
                                report.SCSI.MultiMediaDevice.Features.CanUpgradeFirmware = true;
                                break;
                            case 0x0106:
                            {
                                report.SCSI.MultiMediaDevice.Features.SupportsCSS = true;
                                Feature_0106? ftr0106 =
                                    Features.Decode_0106(desc.Data);
                                if(ftr0106.HasValue)
                                    if(ftr0106.Value.CSSVersion > 0)
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CSSVersionSpecified = true;
                                        report.SCSI.MultiMediaDevice.Features.CSSVersion = ftr0106.Value.CSSVersion;
                                    }
                            }
                                break;
                            case 0x0108:
                                report.SCSI.MultiMediaDevice.Features.CanReportDriveSerial = true;
                                break;
                            case 0x0109:
                                report.SCSI.MultiMediaDevice.Features.CanReportMediaSerial = true;
                                break;
                            case 0x010B:
                            {
                                report.SCSI.MultiMediaDevice.Features.SupportsCPRM = true;
                                Feature_010B? ftr010B =
                                    Features.Decode_010B(desc.Data);
                                if(ftr010B.HasValue)
                                    if(ftr010B.Value.CPRMVersion > 0)
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CPRMVersionSpecified = true;
                                        report.SCSI.MultiMediaDevice.Features.CPRMVersion = ftr010B.Value.CPRMVersion;
                                    }
                            }
                                break;
                            case 0x010C:
                            {
                                Feature_010C? ftr010C =
                                    Features.Decode_010C(desc.Data);
                                if(ftr010C.HasValue)
                                {
                                    string syear, smonth, sday, shour, sminute, ssecond;
                                    byte[] temp;

                                    temp = new byte[4];
                                    temp[0] = (byte)((ftr010C.Value.Century & 0xFF00) >> 8);
                                    temp[1] = (byte)(ftr010C.Value.Century & 0xFF);
                                    temp[2] = (byte)((ftr010C.Value.Year & 0xFF00) >> 8);
                                    temp[3] = (byte)(ftr010C.Value.Year & 0xFF);
                                    syear = Encoding.ASCII.GetString(temp);
                                    temp = new byte[2];
                                    temp[0] = (byte)((ftr010C.Value.Month & 0xFF00) >> 8);
                                    temp[1] = (byte)(ftr010C.Value.Month & 0xFF);
                                    smonth = Encoding.ASCII.GetString(temp);
                                    temp = new byte[2];
                                    temp[0] = (byte)((ftr010C.Value.Day & 0xFF00) >> 8);
                                    temp[1] = (byte)(ftr010C.Value.Day & 0xFF);
                                    sday = Encoding.ASCII.GetString(temp);
                                    temp = new byte[2];
                                    temp[0] = (byte)((ftr010C.Value.Hour & 0xFF00) >> 8);
                                    temp[1] = (byte)(ftr010C.Value.Hour & 0xFF);
                                    shour = Encoding.ASCII.GetString(temp);
                                    temp = new byte[2];
                                    temp[0] = (byte)((ftr010C.Value.Minute & 0xFF00) >> 8);
                                    temp[1] = (byte)(ftr010C.Value.Minute & 0xFF);
                                    sminute = Encoding.ASCII.GetString(temp);
                                    temp = new byte[2];
                                    temp[0] = (byte)((ftr010C.Value.Second & 0xFF00) >> 8);
                                    temp[1] = (byte)(ftr010C.Value.Second & 0xFF);
                                    ssecond = Encoding.ASCII.GetString(temp);

                                    try
                                    {
                                        report.SCSI.MultiMediaDevice.Features.FirmwareDate =
                                            new DateTime(int.Parse(syear), int.Parse(smonth), int.Parse(sday),
                                                         int.Parse(shour), int.Parse(sminute), int.Parse(ssecond),
                                                         DateTimeKind.Utc);

                                        report.SCSI.MultiMediaDevice.Features.FirmwareDateSpecified = true;
                                    }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                    catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                }
                            }
                                break;
                            case 0x010D:
                            {
                                report.SCSI.MultiMediaDevice.Features.SupportsAACS = true;
                                Feature_010D? ftr010D =
                                    Features.Decode_010D(desc.Data);
                                if(ftr010D.HasValue)
                                {
                                    report.SCSI.MultiMediaDevice.Features.CanReadDriveAACSCertificate =
                                        ftr010D.Value.RDC;
                                    report.SCSI.MultiMediaDevice.Features.CanReadCPRM_MKB = ftr010D.Value.RMC;
                                    report.SCSI.MultiMediaDevice.Features.CanWriteBusEncryptedBlocks =
                                        ftr010D.Value.WBE;
                                    report.SCSI.MultiMediaDevice.Features.SupportsBusEncryption = ftr010D.Value.BEC;
                                    report.SCSI.MultiMediaDevice.Features.CanGenerateBindingNonce = ftr010D.Value.BNG;

                                    if(ftr010D.Value.BindNonceBlocks > 0)
                                    {
                                        report.SCSI.MultiMediaDevice.Features.BindingNonceBlocksSpecified = true;
                                        report.SCSI.MultiMediaDevice.Features.BindingNonceBlocks =
                                            ftr010D.Value.BindNonceBlocks;
                                    }

                                    if(ftr010D.Value.AGIDs > 0)
                                    {
                                        report.SCSI.MultiMediaDevice.Features.AGIDsSpecified = true;
                                        report.SCSI.MultiMediaDevice.Features.AGIDs = ftr010D.Value.AGIDs;
                                    }

                                    if(ftr010D.Value.AACSVersion > 0)
                                    {
                                        report.SCSI.MultiMediaDevice.Features.AACSVersionSpecified = true;
                                        report.SCSI.MultiMediaDevice.Features.AACSVersion = ftr010D.Value.AACSVersion;
                                    }
                                }
                            }
                                break;
                            case 0x010E:
                                report.SCSI.MultiMediaDevice.Features.CanWriteCSSManagedDVD = true;
                                break;
                            case 0x0113:
                                report.SCSI.MultiMediaDevice.Features.SupportsSecurDisc = true;
                                break;
                            case 0x0142:
                                report.SCSI.MultiMediaDevice.Features.SupportsOSSC = true;
                                break;
                            case 0x0110:
                                report.SCSI.MultiMediaDevice.Features.SupportsVCPS = true;
                                break;
                        }
                }

                if(report.SCSI.MultiMediaDevice.Features.CanReadBD ||
                   report.SCSI.MultiMediaDevice.Features.CanReadBDR ||
                   report.SCSI.MultiMediaDevice.Features.CanReadBDRE1 ||
                   report.SCSI.MultiMediaDevice.Features.CanReadBDRE2 ||
                   report.SCSI.MultiMediaDevice.Features.CanReadBDROM ||
                   report.SCSI.MultiMediaDevice.Features.CanReadOldBDR ||
                   report.SCSI.MultiMediaDevice.Features.CanReadOldBDRE ||
                   report.SCSI.MultiMediaDevice.Features.CanReadOldBDROM)
                {
                    if(!mediaTypes.Contains("BD-ROM")) mediaTypes.Add("BD-ROM");
                    if(!mediaTypes.Contains("BD-R")) mediaTypes.Add("BD-R");
                    if(!mediaTypes.Contains("BD-RE")) mediaTypes.Add("BD-RE");
                    if(!mediaTypes.Contains("BD-R LTH")) mediaTypes.Add("BD-R LTH");
                    if(!mediaTypes.Contains("BD-R XL")) mediaTypes.Add("BD-R XL");
                }

                if(report.SCSI.MultiMediaDevice.Features.CanReadCD || report.SCSI.MultiMediaDevice.Features.MultiRead)
                {
                    if(!mediaTypes.Contains("CD-ROM")) mediaTypes.Add("CD-ROM");
                    if(!mediaTypes.Contains("Audio CD")) mediaTypes.Add("Audio CD");
                    if(!mediaTypes.Contains("CD-R")) mediaTypes.Add("CD-R");
                    if(!mediaTypes.Contains("CD-RW")) mediaTypes.Add("CD-RW");
                }

                if(report.SCSI.MultiMediaDevice.Features.CanReadCDMRW) if(!mediaTypes.Contains("CD-MRW")) mediaTypes.Add("CD-MRW");

                if(report.SCSI.MultiMediaDevice.Features.CanReadDDCD)
                {
                    if(!mediaTypes.Contains("DDCD-ROM")) mediaTypes.Add("DDCD-ROM");
                    if(!mediaTypes.Contains("DDCD-R")) mediaTypes.Add("DDCD-R");
                    if(!mediaTypes.Contains("DDCD-RW")) mediaTypes.Add("DDCD-RW");
                }

                if(report.SCSI.MultiMediaDevice.Features.CanReadDVD ||
                   report.SCSI.MultiMediaDevice.Features.DVDMultiRead ||
                   report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusR ||
                   report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRDL ||
                   report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRW ||
                   report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRWDL)
                {
                    if(!mediaTypes.Contains("DVD-ROM")) mediaTypes.Add("DVD-ROM");
                    if(!mediaTypes.Contains("DVD-R")) mediaTypes.Add("DVD-R");
                    if(!mediaTypes.Contains("DVD-RW")) mediaTypes.Add("DVD-RW");
                    if(!mediaTypes.Contains("DVD+R")) mediaTypes.Add("DVD+R");
                    if(!mediaTypes.Contains("DVD+RW")) mediaTypes.Add("DVD+RW");
                    if(!mediaTypes.Contains("DVD-R DL")) mediaTypes.Add("DVD-R DL");
                    if(!mediaTypes.Contains("DVD+R DL")) mediaTypes.Add("DVD+R DL");
                }

                if(report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusMRW) if(!mediaTypes.Contains("DVD+MRW")) mediaTypes.Add("DVD+MRW");

                if(report.SCSI.MultiMediaDevice.Features.CanReadHDDVD ||
                   report.SCSI.MultiMediaDevice.Features.CanReadHDDVDR)
                {
                    if(!mediaTypes.Contains("HD DVD-ROM")) mediaTypes.Add("HD DVD-ROM");
                    if(!mediaTypes.Contains("HD DVD-R")) mediaTypes.Add("HD DVD-R");
                    if(!mediaTypes.Contains("HD DVD-RW")) mediaTypes.Add("HD DVD-RW");
                }

                if(report.SCSI.MultiMediaDevice.Features.CanReadHDDVDRAM) if(!mediaTypes.Contains("HD DVD-RAM")) mediaTypes.Add("HD DVD-RAM");
            }

            bool tryPlextor = false, tryHldtst = false, tryPioneer = false, tryNec = false;

            tryPlextor |= dev.Manufacturer.ToLowerInvariant() == "plextor";
            tryHldtst |= dev.Manufacturer.ToLowerInvariant() == "hl-dt-st";
            tryPioneer |= dev.Manufacturer.ToLowerInvariant() == "pioneer";
            tryNec |= dev.Manufacturer.ToLowerInvariant() == "nec";

            // Very old CD drives do not contain mode page 2Ah neither GET CONFIGURATION, so just try all CDs on them
            // Also don't get confident, some drives didn't know CD-RW but are able to read them
            if(mediaTypes.Count == 0 || mediaTypes.Contains("CD-ROM"))
            {
                if(!mediaTypes.Contains("CD-ROM")) mediaTypes.Add("CD-ROM");
                if(!mediaTypes.Contains("Audio CD")) mediaTypes.Add("Audio CD");
                if(!mediaTypes.Contains("CD-R")) mediaTypes.Add("CD-R");
                if(!mediaTypes.Contains("CD-RW")) mediaTypes.Add("CD-RW");
            }

            mediaTypes.Sort();
            List<testedMediaType> mediaTests = new List<testedMediaType>();
            foreach(string mediaType in mediaTypes)
            {
                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you have a {0} disc that you can insert in the drive? (Y/N): ", mediaType);
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                if(pressedKey.Key == ConsoleKey.Y)
                {
                    dev.AllowMediumRemoval(out senseBuffer, timeout, out duration);
                    dev.EjectTray(out senseBuffer, timeout, out duration);
                    DicConsole.WriteLine("Please insert it in the drive and press any key when it is ready.");
                    System.Console.ReadKey(true);

                    testedMediaType mediaTest = new testedMediaType();
                    mediaTest.MediumTypeName = mediaType;
                    mediaTest.MediaIsRecognized = true;

                    sense = dev.ScsiTestUnitReady(out senseBuffer, timeout, out duration);
                    if(sense)
                    {
                        FixedSense? decSense = Sense.DecodeFixed(senseBuffer);
                        if(decSense.HasValue)
                            if(decSense.Value.ASC == 0x3A)
                            {
                                int leftRetries = 20;
                                while(leftRetries > 0)
                                {
                                    DicConsole.Write("\rWaiting for drive to become ready");
                                    Thread.Sleep(2000);
                                    sense = dev.ScsiTestUnitReady(out senseBuffer, timeout, out duration);
                                    if(!sense) break;

                                    leftRetries--;
                                }

                                mediaTest.MediaIsRecognized &= !sense;
                            }
                            else if(decSense.Value.ASC == 0x04 && decSense.Value.ASCQ == 0x01)
                            {
                                int leftRetries = 20;
                                while(leftRetries > 0)
                                {
                                    DicConsole.Write("\rWaiting for drive to become ready");
                                    Thread.Sleep(2000);
                                    sense = dev.ScsiTestUnitReady(out senseBuffer, timeout, out duration);
                                    if(!sense) break;

                                    leftRetries--;
                                }

                                mediaTest.MediaIsRecognized &= !sense;
                            }
                            // These should be trapped by the OS but seems in some cases they're not
                            else if(decSense.Value.ASC == 0x28)
                            {
                                int leftRetries = 20;
                                while(leftRetries > 0)
                                {
                                    DicConsole.Write("\rWaiting for drive to become ready");
                                    Thread.Sleep(2000);
                                    sense = dev.ScsiTestUnitReady(out senseBuffer, timeout, out duration);
                                    if(!sense) break;

                                    leftRetries--;
                                }

                                mediaTest.MediaIsRecognized &= !sense;
                            }
                            else mediaTest.MediaIsRecognized = false;
                        else mediaTest.MediaIsRecognized = false;
                    }

                    if(mediaTest.MediaIsRecognized)
                    {
                        mediaTest.SupportsReadCapacitySpecified = true;
                        mediaTest.SupportsReadCapacity16Specified = true;

                        DicConsole.WriteLine("Querying SCSI READ CAPACITY...");
                        sense = dev.ReadCapacity(out buffer, out senseBuffer, timeout, out duration);
                        if(!sense && !dev.Error)
                        {
                            mediaTest.SupportsReadCapacity = true;
                            mediaTest.Blocks =
                                (ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3]) + 1;
                            mediaTest.BlockSize =
                                (uint)((buffer[5] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + buffer[7]);
                            mediaTest.BlocksSpecified = true;
                            mediaTest.BlockSizeSpecified = true;
                        }

                        DicConsole.WriteLine("Querying SCSI READ CAPACITY (16)...");
                        sense = dev.ReadCapacity16(out buffer, out buffer, timeout, out duration);
                        if(!sense && !dev.Error)
                        {
                            mediaTest.SupportsReadCapacity16 = true;
                            byte[] temp = new byte[8];
                            Array.Copy(buffer, 0, temp, 0, 8);
                            Array.Reverse(temp);
                            mediaTest.Blocks = BitConverter.ToUInt64(temp, 0) + 1;
                            mediaTest.BlockSize =
                                (uint)((buffer[5] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + buffer[7]);
                            mediaTest.BlocksSpecified = true;
                            mediaTest.BlockSizeSpecified = true;
                        }

                        decMode = null;

                        DicConsole.WriteLine("Querying SCSI MODE SENSE (10)...");
                        sense = dev.ModeSense10(out buffer, out senseBuffer, false, true,
                                                ScsiModeSensePageControl.Current, 0x3F, 0x00, timeout, out duration);
                        if(!sense && !dev.Error)
                        {
                            report.SCSI.SupportsModeSense10 = true;
                            decMode = Modes.DecodeMode10(buffer, dev.ScsiType);
                            if(debug) mediaTest.ModeSense10Data = buffer;
                        }
                        DicConsole.WriteLine("Querying SCSI MODE SENSE...");
                        sense = dev.ModeSense(out buffer, out senseBuffer, timeout, out duration);
                        if(!sense && !dev.Error)
                        {
                            report.SCSI.SupportsModeSense6 = true;
                            if(!decMode.HasValue) decMode = Modes.DecodeMode6(buffer, dev.ScsiType);
                            if(debug) mediaTest.ModeSense6Data = buffer;
                        }

                        if(decMode.HasValue)
                        {
                            mediaTest.MediumType = (byte)decMode.Value.Header.MediumType;
                            mediaTest.MediumTypeSpecified = true;
                            if(decMode.Value.Header.BlockDescriptors != null &&
                               decMode.Value.Header.BlockDescriptors.Length > 0)
                            {
                                mediaTest.Density = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
                                mediaTest.DensitySpecified = true;
                            }
                        }

                        if(mediaType.StartsWith("CD-", StringComparison.Ordinal) ||
                           mediaType.StartsWith("DDCD-", StringComparison.Ordinal) || mediaType == "Audio CD")
                        {
                            mediaTest.CanReadTOCSpecified = true;
                            mediaTest.CanReadFullTOCSpecified = true;
                            DicConsole.WriteLine("Querying CD TOC...");
                            mediaTest.CanReadTOC =
                                !dev.ReadTocPmaAtip(out buffer, out senseBuffer, false, 0, 0, timeout, out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadTOC);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "readtoc",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);

                            DicConsole.WriteLine("Querying CD Full TOC...");
                            mediaTest.CanReadFullTOC =
                                !dev.ReadRawToc(out buffer, out senseBuffer, 1, timeout, out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadFullTOC);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "readfulltoc",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                        }

                        if(mediaType.StartsWith("CD-R", StringComparison.Ordinal) ||
                           mediaType.StartsWith("DDCD-R", StringComparison.Ordinal))
                        {
                            mediaTest.CanReadATIPSpecified = true;
                            mediaTest.CanReadPMASpecified = true;
                            DicConsole.WriteLine("Querying CD ATIP...");
                            mediaTest.CanReadATIP = !dev.ReadAtip(out buffer, out senseBuffer, timeout, out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadATIP);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "atip",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                            DicConsole.WriteLine("Querying CD PMA...");
                            mediaTest.CanReadPMA = !dev.ReadPma(out buffer, out senseBuffer, timeout, out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPMA);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "pma",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                        }

                        if(mediaType.StartsWith("DVD-", StringComparison.Ordinal) ||
                           mediaType.StartsWith("HD DVD-", StringComparison.Ordinal))
                        {
                            mediaTest.CanReadPFISpecified = true;
                            mediaTest.CanReadDMISpecified = true;
                            DicConsole.WriteLine("Querying DVD PFI...");
                            mediaTest.CanReadPFI =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                       MmcDiscStructureFormat.PhysicalInformation, 0, timeout,
                                                       out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPFI);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "pfi",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                            DicConsole.WriteLine("Querying DVD DMI...");
                            mediaTest.CanReadDMI =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                       MmcDiscStructureFormat.DiscManufacturingInformation, 0, timeout,
                                                       out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDMI);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "dmi",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                        }

                        if(mediaType == "DVD-ROM")
                        {
                            mediaTest.CanReadCMISpecified = true;
                            DicConsole.WriteLine("Querying DVD CMI...");
                            mediaTest.CanReadCMI =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                       MmcDiscStructureFormat.CopyrightInformation, 0, timeout,
                                                       out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadCMI);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "cmi",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                        }

                        switch(mediaType) {
                            case "DVD-ROM":
                            case "HD DVD-ROM":
                                mediaTest.CanReadBCASpecified = true;
                                DicConsole.WriteLine("Querying DVD BCA...");
                                mediaTest.CanReadBCA =
                                    !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                           MmcDiscStructureFormat.BurstCuttingArea, 0, timeout,
                                                           out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadBCA);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "bca",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                mediaTest.CanReadAACSSpecified = true;
                                DicConsole.WriteLine("Querying DVD AACS...");
                                mediaTest.CanReadAACS =
                                    !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                           MmcDiscStructureFormat.DvdAacs, 0, timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadAACS);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "aacs",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                break;
                            case "BD-ROM":
                                mediaTest.CanReadBCASpecified = true;
                                DicConsole.WriteLine("Querying BD BCA...");
                                mediaTest.CanReadBCA =
                                    !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Bd, 0, 0,
                                                           MmcDiscStructureFormat.BdBurstCuttingArea, 0, timeout,
                                                           out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadBCA);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "bdbca",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                break;
                            case "DVD-RAM":
                            case "HD DVD-RAM":
                                mediaTest.CanReadDDSSpecified = true;
                                mediaTest.CanReadSpareAreaInformationSpecified = true;
                                mediaTest.CanReadDDS =
                                    !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                           MmcDiscStructureFormat.DvdramDds, 0, timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDDS);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "dds",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                mediaTest.CanReadSpareAreaInformation =
                                    !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                           MmcDiscStructureFormat.DvdramSpareAreaInformation, 0, timeout,
                                                           out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.CanReadSpareAreaInformation);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "sai",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                break;
                        }

                        if(mediaType.StartsWith("BD-R", StringComparison.Ordinal) && mediaType != "BD-ROM")
                        {
                            mediaTest.CanReadDDSSpecified = true;
                            mediaTest.CanReadSpareAreaInformationSpecified = true;
                            DicConsole.WriteLine("Querying BD DDS...");
                            mediaTest.CanReadDDS =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Bd, 0, 0,
                                                       MmcDiscStructureFormat.BdDds, 0, timeout, out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDDS);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "bddds",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                            DicConsole.WriteLine("Querying BD SAI...");
                            mediaTest.CanReadSpareAreaInformation =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Bd, 0, 0,
                                                       MmcDiscStructureFormat.BdSpareAreaInformation, 0, timeout,
                                                       out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                      !mediaTest.CanReadSpareAreaInformation);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "bdsai",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                        }

                        if(mediaType == "DVD-R" || mediaType == "DVD-RW")
                        {
                            mediaTest.CanReadPRISpecified = true;
                            DicConsole.WriteLine("Querying DVD PRI...");
                            mediaTest.CanReadPRI =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                       MmcDiscStructureFormat.PreRecordedInfo, 0, timeout,
                                                       out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPRI);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "pri",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                        }

                        if(mediaType == "DVD-R" || mediaType == "DVD-RW" || mediaType == "HD DVD-R")
                        {
                            mediaTest.CanReadMediaIDSpecified = true;
                            mediaTest.CanReadRecordablePFISpecified = true;
                            DicConsole.WriteLine("Querying DVD Media ID...");
                            mediaTest.CanReadMediaID =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                       MmcDiscStructureFormat.DvdrMediaIdentifier, 0, timeout,
                                                       out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadMediaID);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "mediaid",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                            DicConsole.WriteLine("Querying DVD Embossed PFI...");
                            mediaTest.CanReadRecordablePFI =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                       MmcDiscStructureFormat.DvdrPhysicalInformation, 0, timeout,
                                                       out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRecordablePFI);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "epfi",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                        }

                        if(mediaType.StartsWith("DVD+R", StringComparison.Ordinal) || mediaType == "DVD+MRW")
                        {
                            mediaTest.CanReadADIPSpecified = true;
                            mediaTest.CanReadDCBSpecified = true;
                            DicConsole.WriteLine("Querying DVD ADIP...");
                            mediaTest.CanReadADIP =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                       MmcDiscStructureFormat.Adip, 0, timeout, out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadADIP);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "adip",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                            DicConsole.WriteLine("Querying DVD DCB...");
                            mediaTest.CanReadDCB =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                       MmcDiscStructureFormat.Dcb, 0, timeout, out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDCB);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "dcb",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                        }

                        if(mediaType == "HD DVD-ROM")
                        {
                            mediaTest.CanReadHDCMISpecified = true;
                            DicConsole.WriteLine("Querying HD DVD CMI...");
                            mediaTest.CanReadHDCMI =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                       MmcDiscStructureFormat.HddvdCopyrightInformation, 0, timeout,
                                                       out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadHDCMI);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "hdcmi",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                        }

                        if(mediaType.EndsWith(" DL", StringComparison.Ordinal))
                        {
                            mediaTest.CanReadLayerCapacitySpecified = true;
                            DicConsole.WriteLine("Querying DVD Layer Capacity...");
                            mediaTest.CanReadLayerCapacity =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                       MmcDiscStructureFormat.DvdrLayerCapacity, 0, timeout,
                                                       out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadLayerCapacity);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "layer",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                        }

                        if(mediaType.StartsWith("BD-R", StringComparison.Ordinal))
                        {
                            mediaTest.CanReadDiscInformationSpecified = true;
                            mediaTest.CanReadPACSpecified = true;
                            DicConsole.WriteLine("Querying BD Disc Information...");
                            mediaTest.CanReadDiscInformation =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Bd, 0, 0,
                                                       MmcDiscStructureFormat.DiscInformation, 0, timeout,
                                                       out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDiscInformation);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "di",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                            DicConsole.WriteLine("Querying BD PAC...");
                            mediaTest.CanReadPAC =
                                !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Bd, 0, 0,
                                                       MmcDiscStructureFormat.Pac, 0, timeout, out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPAC);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "pac",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                        }

                        mediaTest.SupportsReadSpecified = true;
                        mediaTest.SupportsRead10Specified = true;
                        mediaTest.SupportsRead12Specified = true;
                        mediaTest.SupportsRead16Specified = true;

                        DicConsole.WriteLine("Trying SCSI READ (6)...");
                        mediaTest.SupportsRead =
                            !dev.Read6(out buffer, out senseBuffer, 0, 2048, timeout, out duration);
                        DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead);
                        if(debug)
                            DataFile.WriteTo("SCSI Report", "read6",
                                             "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" + mediaType +
                                             ".bin", "read results", buffer);
                        DicConsole.WriteLine("Trying SCSI READ (10)...");
                        mediaTest.SupportsRead10 = !dev.Read10(out buffer, out senseBuffer, 0, false, true, false,
                                                               false, 0, 2048, 0, 1, timeout, out duration);
                        DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead10);
                        if(debug)
                            DataFile.WriteTo("SCSI Report", "read10",
                                             "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" + mediaType +
                                             ".bin", "read results", buffer);
                        DicConsole.WriteLine("Trying SCSI READ (12)...");
                        mediaTest.SupportsRead12 = !dev.Read12(out buffer, out senseBuffer, 0, false, true, false,
                                                               false, 0, 2048, 0, 1, false, timeout, out duration);
                        DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead12);
                        if(debug)
                            DataFile.WriteTo("SCSI Report", "read12",
                                             "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" + mediaType +
                                             ".bin", "read results", buffer);
                        DicConsole.WriteLine("Trying SCSI READ (16)...");
                        mediaTest.SupportsRead16 = !dev.Read16(out buffer, out senseBuffer, 0, false, true, false, 0,
                                                               2048, 0, 1, false, timeout, out duration);
                        DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead16);
                        if(debug)
                            DataFile.WriteTo("SCSI Report", "read16",
                                             "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" + mediaType +
                                             ".bin", "read results", buffer);

                        if(debug)
                            if(!tryPlextor)
                            {
                                pressedKey = new ConsoleKeyInfo();
                                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                                {
                                    DicConsole
                                        .Write("Do you have want to try Plextor vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");
                                    pressedKey = System.Console.ReadKey();
                                    DicConsole.WriteLine();
                                }

                                tryPlextor |= pressedKey.Key == ConsoleKey.Y;
                            }

                        if(mediaType.StartsWith("CD-", StringComparison.Ordinal) ||
                           mediaType.StartsWith("DDCD-", StringComparison.Ordinal) || mediaType == "Audio CD")
                        {
                            mediaTest.CanReadC2PointersSpecified = true;
                            mediaTest.CanReadCorrectedSubchannelSpecified = true;
                            mediaTest.CanReadCorrectedSubchannelWithC2Specified = true;
                            mediaTest.CanReadLeadInSpecified = true;
                            mediaTest.CanReadLeadOutSpecified = true;
                            mediaTest.CanReadPQSubchannelSpecified = true;
                            mediaTest.CanReadPQSubchannelWithC2Specified = true;
                            mediaTest.CanReadRWSubchannelSpecified = true;
                            mediaTest.CanReadRWSubchannelWithC2Specified = true;
                            mediaTest.SupportsReadCdMsfSpecified = true;
                            mediaTest.SupportsReadCdSpecified = true;
                            mediaTest.SupportsReadCdMsfRawSpecified = true;
                            mediaTest.SupportsReadCdRawSpecified = true;

                            if(mediaType == "Audio CD")
                            {
                                DicConsole.WriteLine("Trying SCSI READ CD...");
                                mediaTest.SupportsReadCd = !dev.ReadCd(out buffer, out senseBuffer, 0, 2352, 1,
                                                                       MmcSectorTypes.Cdda, false, false, false,
                                                                       MmcHeaderCodes.None, true, false,
                                                                       MmcErrorField.None, MmcSubchannel.None, timeout,
                                                                       out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCd);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcd",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                DicConsole.WriteLine("Trying SCSI READ CD MSF...");
                                mediaTest.SupportsReadCdMsf = !dev.ReadCdMsf(out buffer, out senseBuffer, 0x00000200,
                                                                             0x00000201, 2352, MmcSectorTypes.Cdda,
                                                                             false, false, MmcHeaderCodes.None, true,
                                                                             false, MmcErrorField.None,
                                                                             MmcSubchannel.None, timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCdMsf);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdmsf",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                            }
                            else
                            {
                                DicConsole.WriteLine("Trying SCSI READ CD...");
                                mediaTest.SupportsReadCd = !dev.ReadCd(out buffer, out senseBuffer, 0, 2048, 1,
                                                                       MmcSectorTypes.AllTypes, false, false, false,
                                                                       MmcHeaderCodes.None, true, false,
                                                                       MmcErrorField.None, MmcSubchannel.None, timeout,
                                                                       out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCd);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcd",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                DicConsole.WriteLine("Trying SCSI READ CD MSF...");
                                mediaTest.SupportsReadCdMsf = !dev.ReadCdMsf(out buffer, out senseBuffer, 0x00000200,
                                                                             0x00000201, 2048, MmcSectorTypes.AllTypes,
                                                                             false, false, MmcHeaderCodes.None, true,
                                                                             false, MmcErrorField.None,
                                                                             MmcSubchannel.None, timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCdMsf);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdmsf",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                DicConsole.WriteLine("Trying SCSI READ CD full sector...");
                                mediaTest.SupportsReadCdRaw = !dev.ReadCd(out buffer, out senseBuffer, 0, 2352, 1,
                                                                          MmcSectorTypes.AllTypes, false, false, true,
                                                                          MmcHeaderCodes.AllHeaders, true, true,
                                                                          MmcErrorField.None, MmcSubchannel.None,
                                                                          timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCdRaw);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdraw",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                DicConsole.WriteLine("Trying SCSI READ CD MSF full sector...");
                                mediaTest.SupportsReadCdMsfRaw = !dev.ReadCdMsf(out buffer, out senseBuffer, 0x00000200,
                                                                                0x00000201, 2352,
                                                                                MmcSectorTypes.AllTypes, false, false,
                                                                                MmcHeaderCodes.AllHeaders, true, true,
                                                                                MmcErrorField.None, MmcSubchannel.None,
                                                                                timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.SupportsReadCdMsfRaw);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdmsfraw",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                            }

                            if(mediaTest.SupportsReadCdRaw || mediaType == "Audio CD")
                            {
                                DicConsole.WriteLine("Trying to read CD Lead-In...");
                                for(int i = -150; i < 0; i++)
                                {
                                    if(mediaType == "Audio CD")
                                        sense = dev.ReadCd(out buffer, out senseBuffer, (uint)i, 2352, 1,
                                                           MmcSectorTypes.Cdda, false, false, false,
                                                           MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                           MmcSubchannel.None, timeout, out duration);
                                    else
                                        sense = dev.ReadCd(out buffer, out senseBuffer, (uint)i, 2352, 1,
                                                           MmcSectorTypes.AllTypes, false, false, true,
                                                           MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                                           MmcSubchannel.None, timeout, out duration);
                                    DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", sense);
                                    if(debug)
                                        DataFile.WriteTo("SCSI Report", "leadin",
                                                         "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                         mediaType + ".bin", "read results", buffer);
                                    if(sense) continue;

                                    mediaTest.CanReadLeadIn = true;
                                    break;
                                }

                                DicConsole.WriteLine("Trying to read CD Lead-Out...");
                                if(mediaType == "Audio CD")
                                    mediaTest.CanReadLeadOut = !dev.ReadCd(out buffer, out senseBuffer,
                                                                           (uint)(mediaTest.Blocks + 1), 2352, 1,
                                                                           MmcSectorTypes.Cdda, false, false, false,
                                                                           MmcHeaderCodes.None, true, false,
                                                                           MmcErrorField.None, MmcSubchannel.None,
                                                                           timeout, out duration);
                                else
                                    mediaTest.CanReadLeadOut = !dev.ReadCd(out buffer, out senseBuffer,
                                                                           (uint)(mediaTest.Blocks + 1), 2352, 1,
                                                                           MmcSectorTypes.AllTypes, false, false, true,
                                                                           MmcHeaderCodes.AllHeaders, true, true,
                                                                           MmcErrorField.None, MmcSubchannel.None,
                                                                           timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadLeadOut);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "leadout",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                            }

                            if(mediaType == "Audio CD")
                            {
                                DicConsole.WriteLine("Trying to read C2 Pointers...");
                                mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2646, 1,
                                                                          MmcSectorTypes.Cdda, false, false, false,
                                                                          MmcHeaderCodes.None, true, false,
                                                                          MmcErrorField.C2Pointers, MmcSubchannel.None,
                                                                          timeout, out duration);
                                if(!mediaTest.CanReadC2Pointers)
                                    mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2648, 1,
                                                                              MmcSectorTypes.Cdda, false, false, false,
                                                                              MmcHeaderCodes.None, true, false,
                                                                              MmcErrorField.C2PointersAndBlock,
                                                                              MmcSubchannel.None, timeout,
                                                                              out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadC2Pointers);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdc2",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);

                                DicConsole.WriteLine("Trying to read subchannels...");
                                mediaTest.CanReadPQSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2368, 1,
                                                                            MmcSectorTypes.Cdda, false, false, false,
                                                                            MmcHeaderCodes.None, true, false,
                                                                            MmcErrorField.None, MmcSubchannel.Q16,
                                                                            timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannel);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdpq",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                mediaTest.CanReadRWSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2448, 1,
                                                                            MmcSectorTypes.Cdda, false, false, false,
                                                                            MmcHeaderCodes.None, true, false,
                                                                            MmcErrorField.None, MmcSubchannel.Raw,
                                                                            timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannel);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdrw",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                mediaTest.CanReadCorrectedSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2448,
                                                                                   1, MmcSectorTypes.Cdda, false, false,
                                                                                   false, MmcHeaderCodes.None, true,
                                                                                   false, MmcErrorField.None,
                                                                                   MmcSubchannel.Rw, timeout,
                                                                                   out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.CanReadCorrectedSubchannel);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdsub",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);

                                DicConsole.WriteLine("Trying to read subchannels with C2 Pointers...");
                                mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2662,
                                                                                  1, MmcSectorTypes.Cdda, false, false,
                                                                                  false, MmcHeaderCodes.None, true,
                                                                                  false, MmcErrorField.C2Pointers,
                                                                                  MmcSubchannel.Q16, timeout,
                                                                                  out duration);
                                if(!mediaTest.CanReadPQSubchannelWithC2)
                                    mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0,
                                                                                      2664, 1, MmcSectorTypes.Cdda,
                                                                                      false, false, false,
                                                                                      MmcHeaderCodes.None, true, false,
                                                                                      MmcErrorField.C2PointersAndBlock,
                                                                                      MmcSubchannel.Q16, timeout,
                                                                                      out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.CanReadPQSubchannelWithC2);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdpqc2",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);

                                mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2712,
                                                                                  1, MmcSectorTypes.Cdda, false, false,
                                                                                  false, MmcHeaderCodes.None, true,
                                                                                  false, MmcErrorField.C2Pointers,
                                                                                  MmcSubchannel.Raw, timeout,
                                                                                  out duration);
                                if(!mediaTest.CanReadRWSubchannelWithC2)
                                    mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0,
                                                                                      2714, 1, MmcSectorTypes.Cdda,
                                                                                      false, false, false,
                                                                                      MmcHeaderCodes.None, true, false,
                                                                                      MmcErrorField.C2PointersAndBlock,
                                                                                      MmcSubchannel.Raw, timeout,
                                                                                      out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.CanReadRWSubchannelWithC2);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdrwc2",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);

                                mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0,
                                                                                         2712, 1, MmcSectorTypes.Cdda,
                                                                                         false, false, false,
                                                                                         MmcHeaderCodes.None, true,
                                                                                         false,
                                                                                         MmcErrorField.C2Pointers,
                                                                                         MmcSubchannel.Rw, timeout,
                                                                                         out duration);
                                if(!mediaTest.CanReadCorrectedSubchannelWithC2)
                                    mediaTest.CanReadCorrectedSubchannelWithC2 =
                                        !dev.ReadCd(out buffer, out senseBuffer, 0, 2714, 1, MmcSectorTypes.Cdda, false,
                                                    false, false, MmcHeaderCodes.None, true, false,
                                                    MmcErrorField.C2PointersAndBlock, MmcSubchannel.Rw, timeout,
                                                    out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.CanReadCorrectedSubchannelWithC2);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdsubc2",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                            }
                            else if(mediaTest.SupportsReadCdRaw)
                            {
                                DicConsole.WriteLine("Trying to read C2 Pointers...");
                                mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2646, 1,
                                                                          MmcSectorTypes.AllTypes, false, false, true,
                                                                          MmcHeaderCodes.AllHeaders, true, true,
                                                                          MmcErrorField.C2Pointers, MmcSubchannel.None,
                                                                          timeout, out duration);
                                if(!mediaTest.CanReadC2Pointers)
                                    mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2648, 1,
                                                                              MmcSectorTypes.AllTypes, false, false,
                                                                              true, MmcHeaderCodes.AllHeaders, true,
                                                                              true, MmcErrorField.C2PointersAndBlock,
                                                                              MmcSubchannel.None, timeout,
                                                                              out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadC2Pointers);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdc2",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);

                                DicConsole.WriteLine("Trying to read subchannels...");
                                mediaTest.CanReadPQSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2368, 1,
                                                                            MmcSectorTypes.AllTypes, false, false, true,
                                                                            MmcHeaderCodes.AllHeaders, true, true,
                                                                            MmcErrorField.None, MmcSubchannel.Q16,
                                                                            timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannel);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdpq",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                mediaTest.CanReadRWSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2448, 1,
                                                                            MmcSectorTypes.AllTypes, false, false, true,
                                                                            MmcHeaderCodes.AllHeaders, true, true,
                                                                            MmcErrorField.None, MmcSubchannel.Raw,
                                                                            timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannel);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdrw",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                mediaTest.CanReadCorrectedSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2448,
                                                                                   1, MmcSectorTypes.AllTypes, false,
                                                                                   false, true,
                                                                                   MmcHeaderCodes.AllHeaders, true,
                                                                                   true, MmcErrorField.None,
                                                                                   MmcSubchannel.Rw, timeout,
                                                                                   out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.CanReadCorrectedSubchannel);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdsub",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);

                                DicConsole.WriteLine("Trying to read subchannels with C2 Pointers...");
                                mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2662,
                                                                                  1, MmcSectorTypes.AllTypes, false,
                                                                                  false, true,
                                                                                  MmcHeaderCodes.AllHeaders, true, true,
                                                                                  MmcErrorField.C2Pointers,
                                                                                  MmcSubchannel.Q16, timeout,
                                                                                  out duration);
                                if(!mediaTest.CanReadPQSubchannelWithC2)
                                    mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0,
                                                                                      2664, 1, MmcSectorTypes.AllTypes,
                                                                                      false, false, true,
                                                                                      MmcHeaderCodes.AllHeaders, true,
                                                                                      true,
                                                                                      MmcErrorField.C2PointersAndBlock,
                                                                                      MmcSubchannel.Q16, timeout,
                                                                                      out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.CanReadPQSubchannelWithC2);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdpqc2",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);

                                mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2712,
                                                                                  1, MmcSectorTypes.AllTypes, false,
                                                                                  false, true,
                                                                                  MmcHeaderCodes.AllHeaders, true, true,
                                                                                  MmcErrorField.C2Pointers,
                                                                                  MmcSubchannel.Raw, timeout,
                                                                                  out duration);
                                if(!mediaTest.CanReadRWSubchannelWithC2)
                                    mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0,
                                                                                      2714, 1, MmcSectorTypes.AllTypes,
                                                                                      false, false, true,
                                                                                      MmcHeaderCodes.AllHeaders, true,
                                                                                      true,
                                                                                      MmcErrorField.C2PointersAndBlock,
                                                                                      MmcSubchannel.Raw, timeout,
                                                                                      out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.CanReadRWSubchannelWithC2);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdrwc2",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);

                                mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0,
                                                                                         2712, 1,
                                                                                         MmcSectorTypes.AllTypes, false,
                                                                                         false, true,
                                                                                         MmcHeaderCodes.AllHeaders,
                                                                                         true, true,
                                                                                         MmcErrorField.C2Pointers,
                                                                                         MmcSubchannel.Rw, timeout,
                                                                                         out duration);
                                if(!mediaTest.CanReadCorrectedSubchannelWithC2)
                                    mediaTest.CanReadCorrectedSubchannelWithC2 =
                                        !dev.ReadCd(out buffer, out senseBuffer, 0, 2714, 1, MmcSectorTypes.AllTypes,
                                                    false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                                    MmcErrorField.C2PointersAndBlock, MmcSubchannel.Rw, timeout,
                                                    out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.CanReadCorrectedSubchannelWithC2);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdsubc2",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                            }
                            else
                            {
                                DicConsole.WriteLine("Trying to read C2 Pointers...");
                                mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2342, 1,
                                                                          MmcSectorTypes.AllTypes, false, false, false,
                                                                          MmcHeaderCodes.None, true, false,
                                                                          MmcErrorField.C2Pointers, MmcSubchannel.None,
                                                                          timeout, out duration);
                                if(!mediaTest.CanReadC2Pointers)
                                    mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2344, 1,
                                                                              MmcSectorTypes.AllTypes, false, false,
                                                                              false, MmcHeaderCodes.None, true, false,
                                                                              MmcErrorField.C2PointersAndBlock,
                                                                              MmcSubchannel.None, timeout,
                                                                              out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadC2Pointers);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdc2",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);

                                DicConsole.WriteLine("Trying to read subchannels...");
                                mediaTest.CanReadPQSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2064, 1,
                                                                            MmcSectorTypes.AllTypes, false, false,
                                                                            false, MmcHeaderCodes.None, true, false,
                                                                            MmcErrorField.None, MmcSubchannel.Q16,
                                                                            timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannel);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdpq",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                mediaTest.CanReadRWSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2144, 1,
                                                                            MmcSectorTypes.AllTypes, false, false,
                                                                            false, MmcHeaderCodes.None, true, false,
                                                                            MmcErrorField.None, MmcSubchannel.Raw,
                                                                            timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannel);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdrw",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                mediaTest.CanReadCorrectedSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2144,
                                                                                   1, MmcSectorTypes.AllTypes, false,
                                                                                   false, false, MmcHeaderCodes.None,
                                                                                   true, false, MmcErrorField.None,
                                                                                   MmcSubchannel.Rw, timeout,
                                                                                   out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.CanReadCorrectedSubchannel);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdsub",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);

                                DicConsole.WriteLine("Trying to read subchannels with C2 Pointers...");
                                mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2358,
                                                                                  1, MmcSectorTypes.AllTypes, false,
                                                                                  false, false, MmcHeaderCodes.None,
                                                                                  true, false, MmcErrorField.C2Pointers,
                                                                                  MmcSubchannel.Q16, timeout,
                                                                                  out duration);
                                if(!mediaTest.CanReadPQSubchannelWithC2)
                                    mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0,
                                                                                      2360, 1, MmcSectorTypes.AllTypes,
                                                                                      false, false, false,
                                                                                      MmcHeaderCodes.None, true, false,
                                                                                      MmcErrorField.C2PointersAndBlock,
                                                                                      MmcSubchannel.Q16, timeout,
                                                                                      out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.CanReadPQSubchannelWithC2);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdpqc2",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);

                                mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2438,
                                                                                  1, MmcSectorTypes.AllTypes, false,
                                                                                  false, false, MmcHeaderCodes.None,
                                                                                  true, false, MmcErrorField.C2Pointers,
                                                                                  MmcSubchannel.Raw, timeout,
                                                                                  out duration);
                                if(!mediaTest.CanReadRWSubchannelWithC2)
                                    mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0,
                                                                                      2440, 1, MmcSectorTypes.AllTypes,
                                                                                      false, false, false,
                                                                                      MmcHeaderCodes.None, true, false,
                                                                                      MmcErrorField.C2PointersAndBlock,
                                                                                      MmcSubchannel.Raw, timeout,
                                                                                      out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.CanReadRWSubchannelWithC2);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdrwc2",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);

                                mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0,
                                                                                         2438, 1,
                                                                                         MmcSectorTypes.AllTypes, false,
                                                                                         false, false,
                                                                                         MmcHeaderCodes.None, true,
                                                                                         false,
                                                                                         MmcErrorField.C2Pointers,
                                                                                         MmcSubchannel.Rw, timeout,
                                                                                         out duration);
                                if(!mediaTest.CanReadCorrectedSubchannelWithC2)
                                    mediaTest.CanReadCorrectedSubchannelWithC2 =
                                        !dev.ReadCd(out buffer, out senseBuffer, 0, 2440, 1, MmcSectorTypes.AllTypes,
                                                    false, false, false, MmcHeaderCodes.None, true, false,
                                                    MmcErrorField.C2PointersAndBlock, MmcSubchannel.Rw, timeout,
                                                    out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.CanReadCorrectedSubchannelWithC2);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "readcdsubc2",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                            }

                            if(debug)
                            {
                                if(!tryNec)
                                {
                                    pressedKey = new ConsoleKeyInfo();
                                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                                    {
                                        DicConsole
                                            .Write("Do you have want to try NEC vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");
                                        pressedKey = System.Console.ReadKey();
                                        DicConsole.WriteLine();
                                    }

                                    tryNec |= pressedKey.Key == ConsoleKey.Y;
                                }

                                if(!tryPioneer)
                                {
                                    pressedKey = new ConsoleKeyInfo();
                                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                                    {
                                        DicConsole
                                            .Write("Do you have want to try Pioneer vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");
                                        pressedKey = System.Console.ReadKey();
                                        DicConsole.WriteLine();
                                    }

                                    tryPioneer |= pressedKey.Key == ConsoleKey.Y;
                                }
                            }

                            if(tryPlextor)
                            {
                                mediaTest.SupportsPlextorReadCDDASpecified = true;
                                DicConsole.WriteLine("Trying Plextor READ CD-DA...");
                                mediaTest.SupportsPlextorReadCDDA =
                                    !dev.PlextorReadCdDa(out buffer, out senseBuffer, 0, 2352, 1,
                                                         PlextorSubchannel.None, timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.SupportsPlextorReadCDDA);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "plextorreadcdda",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                            }

                            if(tryPioneer)
                            {
                                mediaTest.SupportsPioneerReadCDDASpecified = true;
                                mediaTest.SupportsPioneerReadCDDAMSFSpecified = true;
                                DicConsole.WriteLine("Trying Pioneer READ CD-DA...");
                                mediaTest.SupportsPioneerReadCDDA =
                                    !dev.PioneerReadCdDa(out buffer, out senseBuffer, 0, 2352, 1,
                                                         PioneerSubchannel.None, timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.SupportsPioneerReadCDDA);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "pioneerreadcdda",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                                DicConsole.WriteLine("Trying Pioneer READ CD-DA MSF...");
                                mediaTest.SupportsPioneerReadCDDAMSF =
                                    !dev.PioneerReadCdDaMsf(out buffer, out senseBuffer, 0x00000200, 0x00000201, 2352,
                                                            PioneerSubchannel.None, timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                          !mediaTest.SupportsPioneerReadCDDAMSF);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "pioneerreadcddamsf",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                            }

                            if(tryNec)
                            {
                                mediaTest.SupportsNECReadCDDASpecified = true;
                                DicConsole.WriteLine("Trying NEC READ CD-DA...");
                                mediaTest.SupportsNECReadCDDA =
                                    !dev.NecReadCdDa(out buffer, out senseBuffer, 0, 1, timeout, out duration);
                                DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsNECReadCDDA);
                                if(debug)
                                    DataFile.WriteTo("SCSI Report", "necreadcdda",
                                                     "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                     mediaType + ".bin", "read results", buffer);
                            }
                        }

                        mediaTest.LongBlockSize = mediaTest.BlockSize;
                        DicConsole.WriteLine("Trying SCSI READ LONG (10)...");
                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 0xFFFF, timeout,
                                               out duration);
                        if(sense && !dev.Error)
                        {
                            FixedSense? decSense = Sense.DecodeFixed(senseBuffer);
                            if(decSense.HasValue)
                                if(decSense.Value.SenseKey == SenseKeys.IllegalRequest &&
                                   decSense.Value.ASC == 0x24 && decSense.Value.ASCQ == 0x00)
                                {
                                    mediaTest.SupportsReadLong = true;
                                    if(decSense.Value.InformationValid && decSense.Value.ILI)
                                    {
                                        mediaTest.LongBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);
                                        mediaTest.LongBlockSizeSpecified = true;
                                    }
                                }
                        }

                        if(debug)
                            if(!tryHldtst)
                            {
                                pressedKey = new ConsoleKeyInfo();
                                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                                {
                                    DicConsole
                                        .Write("Do you have want to try HL-DT-ST (aka LG) vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");
                                    pressedKey = System.Console.ReadKey();
                                    DicConsole.WriteLine();
                                }

                                tryHldtst |= pressedKey.Key == ConsoleKey.Y;
                            }

                        if(mediaTest.SupportsReadLong && mediaTest.LongBlockSize == mediaTest.BlockSize)
                        {
                            // DVDs
                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 37856, timeout,
                                                   out duration);
                            if(!sense && !dev.Error)
                            {
                                mediaTest.SupportsReadLong = true;
                                mediaTest.LongBlockSize = 37856;
                                mediaTest.LongBlockSizeSpecified = true;
                            }
                        }

                        if(tryPlextor)
                        {
                            mediaTest.SupportsPlextorReadRawDVDSpecified = true;
                            DicConsole.WriteLine("Trying Plextor trick to raw read DVDs...");
                            mediaTest.SupportsPlextorReadRawDVD =
                                !dev.PlextorReadRawDvd(out buffer, out senseBuffer, 0, 1, timeout, out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                      !mediaTest.SupportsPlextorReadRawDVD);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "plextorrawdvd",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                            if(mediaTest.SupportsPlextorReadRawDVD)
                                mediaTest.SupportsPlextorReadRawDVD = !ArrayHelpers.ArrayIsNullOrEmpty(buffer);
                        }

                        if(tryHldtst)
                        {
                            mediaTest.SupportsHLDTSTReadRawDVDSpecified = true;
                            DicConsole.WriteLine("Trying HL-DT-ST (aka LG) trick to raw read DVDs...");
                            mediaTest.SupportsHLDTSTReadRawDVD =
                                !dev.HlDtStReadRawDvd(out buffer, out senseBuffer, 0, 1, timeout, out duration);
                            DicConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                      !mediaTest.SupportsHLDTSTReadRawDVD);
                            if(debug)
                                DataFile.WriteTo("SCSI Report", "hldtstrawdvd",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                        }

                        if(mediaTest.SupportsReadLong && mediaTest.LongBlockSize == mediaTest.BlockSize)
                        {
                            pressedKey = new ConsoleKeyInfo();
                            while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                            {
                                DicConsole
                                    .Write("Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours) (Y/N): ");
                                pressedKey = System.Console.ReadKey();
                                DicConsole.WriteLine();
                            }

                            if(pressedKey.Key == ConsoleKey.Y)
                            {
                                for(ushort i = (ushort)mediaTest.BlockSize; true; i++)
                                {
                                    DicConsole.Write("\rTrying to READ LONG with a size of {0} bytes...", i);
                                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, i, timeout,
                                                           out duration);
                                    if(!sense)
                                    {
                                        if(debug)
                                        {
                                            FileStream bingo =
                                                new FileStream($"{mediaType}_readlong.bin",
                                                               FileMode.Create);
                                            bingo.Write(buffer, 0, buffer.Length);
                                            bingo.Close();
                                        }
                                        mediaTest.LongBlockSize = i;
                                        mediaTest.LongBlockSizeSpecified = true;
                                        break;
                                    }

                                    if(i == ushort.MaxValue) break;
                                }

                                DicConsole.WriteLine();
                            }
                        }

                        if(debug && mediaTest.SupportsReadLong && mediaTest.LongBlockSizeSpecified &&
                           mediaTest.LongBlockSize != mediaTest.BlockSize)
                        {
                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0,
                                                   (ushort)mediaTest.LongBlockSize, timeout, out duration);
                            if(!sense)
                                DataFile.WriteTo("SCSI Report", "readlong10",
                                                 "_debug_" + report.SCSI.Inquiry.ProductIdentification + "_" +
                                                 mediaType + ".bin", "read results", buffer);
                        }
                    }

                    mediaTests.Add(mediaTest);
                }

                report.SCSI.MultiMediaDevice.TestedMedia = mediaTests.ToArray();
            }
        }
    }
}