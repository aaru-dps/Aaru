// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Core.Devices.Report;

using System;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Metadata;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Devices;
using Aaru.Helpers;
using global::Spectre.Console;

public sealed partial class DeviceReport
{
    static byte[] ClearMmcFeatures(byte[] response)
    {
        uint offset = 8;

        while(offset + 4 < response.Length)
        {
            var code = (ushort)((response[offset + 0] << 8) + response[offset + 1]);
            var data = new byte[response[offset + 3] + 4];

            if(code != 0x0108)
            {
                offset += (uint)data.Length;

                continue;
            }

            if(data.Length + offset > response.Length)
                data = new byte[response.Length - offset];

            Array.Copy(data, 4, response, offset + 4, data.Length - 4);
            offset += (uint)data.Length;
        }

        return response;
    }

    /// <summary>Creates a report for the GET CONFIGURATION response of an MMC device</summary>
    /// <returns>MMC features report</returns>
    public MmcFeatures ReportMmcFeatures()
    {
        var    sense  = true;
        byte[] buffer = Array.Empty<byte>();

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Querying MMC GET CONFIGURATION...").IsIndeterminate();
            sense = _dev.GetConfiguration(out buffer, out _, _dev.Timeout, out _);
        });

        if(sense)
            return null;

        Features.SeparatedFeatures ftr = Features.Separate(buffer);

        if(ftr.Descriptors        == null ||
           ftr.Descriptors.Length <= 0)
            return null;

        var report = new MmcFeatures
        {
            BinaryData = ClearMmcFeatures(buffer)
        };

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

                        if(ftr0010.Value.Blocking > 0)
                            report.BlocksPerReadableUnit = ftr0010.Value.Blocking;

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
                    Feature_002A? ftr002A = Features.Decode_002A(desc.Data);

                    if(ftr002A.HasValue)
                        report.CanWriteDVDPlusRW = ftr002A.Value.Write;
                }

                    break;
                case 0x002B:
                {
                    report.CanReadDVDPlusR = true;
                    Feature_002B? ftr002B = Features.Decode_002B(desc.Data);

                    if(ftr002B.HasValue)
                        report.CanWriteDVDPlusR = ftr002B.Value.Write;
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
                    Feature_0031? ftr0031 = Features.Decode_0031(desc.Data);

                    if(ftr0031.HasValue)
                        report.CanTestWriteDDCDR = ftr0031.Value.TestWrite;
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
                    Feature_003A? ftr003A = Features.Decode_003A(desc.Data);

                    if(ftr003A.HasValue)
                        report.CanWriteDVDPlusRWDL = ftr003A.Value.Write;
                }

                    break;
                case 0x003B:
                {
                    report.CanReadDVDPlusRDL = true;
                    Feature_003B? ftr003B = Features.Decode_003B(desc.Data);

                    if(ftr003B.HasValue)
                        report.CanWriteDVDPlusRDL = ftr003B.Value.Write;
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

                        if(ftr0103.Value.VolumeLevels > 0)
                            report.VolumeLevels = ftr0103.Value.VolumeLevels;
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

                    if(ftr0106?.CSSVersion > 0)
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

                    if(ftr010B?.CPRMVersion > 0)
                        report.CPRMVersion = ftr010B.Value.CPRMVersion;
                }

                    break;
                case 0x010C:
                {
                    Feature_010C? ftr010C = Features.Decode_010C(desc.Data);

                    if(ftr010C.HasValue)
                    {
                        var temp = new byte[4];
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
                                                               int.Parse(shour), int.Parse(sminute), int.Parse(ssecond),
                                                               DateTimeKind.Utc);
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

                        if(ftr010D.Value.AGIDs > 0)
                            report.AGIDs = ftr010D.Value.AGIDs;

                        if(ftr010D.Value.AACSVersion > 0)
                            report.AACSVersion = ftr010D.Value.AACSVersion;
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

    /// <summary>Creates a report for media inserted into an MMC device</summary>
    /// <param name="mediaType">Expected media type name</param>
    /// <param name="tryPlextor">Try Plextor vendor commands</param>
    /// <param name="tryPioneer">Try Pioneer vendor commands</param>
    /// <param name="tryNec">Try NEC vendor commands</param>
    /// <param name="tryHldtst">Try HL-DT-ST vendor commands</param>
    /// <param name="tryMediaTekF106">Try MediaTek vendor commands</param>
    /// <returns></returns>
    public TestedMedia ReportMmcMedia(string mediaType, bool tryPlextor, bool tryPioneer, bool tryNec, bool tryHldtst,
                                      bool tryMediaTekF106)
    {
        var    sense       = true;
        byte[] buffer      = Array.Empty<byte>();
        byte[] senseBuffer = Array.Empty<byte>();
        var    mediaTest   = new TestedMedia();

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Querying SCSI READ CAPACITY...").IsIndeterminate();
            sense = _dev.ReadCapacity(out buffer, out senseBuffer, _dev.Timeout, out _);
        });

        if(!sense &&
           !_dev.Error)
        {
            mediaTest.SupportsReadCapacity = true;

            mediaTest.Blocks = ((ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3]) &
                                0xFFFFFFFF) + 1;

            mediaTest.BlockSize = (uint)((buffer[5] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + buffer[7]);
        }

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Querying SCSI READ CAPACITY (16)...").IsIndeterminate();
            sense = _dev.ReadCapacity16(out buffer, out buffer, _dev.Timeout, out _);
        });

        if(!sense &&
           !_dev.Error)
        {
            mediaTest.SupportsReadCapacity16 = true;
            var temp = new byte[8];
            Array.Copy(buffer, 0, temp, 0, 8);
            Array.Reverse(temp);
            mediaTest.Blocks    = BitConverter.ToUInt64(temp, 0) + 1;
            mediaTest.BlockSize = (uint)((buffer[5] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + buffer[7]);
        }

        Modes.DecodedMode? decMode = null;

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Querying SCSI MODE SENSE (10)...").IsIndeterminate();

            sense = _dev.ModeSense10(out buffer, out senseBuffer, false, true, ScsiModeSensePageControl.Current, 0x3F,
                                     0x00, _dev.Timeout, out _);
        });

        if(!sense &&
           !_dev.Error)
        {
            decMode = Modes.DecodeMode10(buffer, _dev.ScsiType);

            mediaTest.ModeSense10Data = buffer;
        }

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Querying SCSI MODE SENSE...").IsIndeterminate();
            sense = _dev.ModeSense(out buffer, out senseBuffer, _dev.Timeout, out _);
        });

        if(!sense &&
           !_dev.Error)
        {
            decMode ??= Modes.DecodeMode6(buffer, _dev.ScsiType);

            mediaTest.ModeSense6Data = buffer;
        }

        if(decMode != null)
        {
            mediaTest.MediumType = (byte?)decMode?.Header.MediumType;

            if(decMode?.Header.BlockDescriptors?.Length > 0)
                mediaTest.Density = (byte?)decMode?.Header.BlockDescriptors?[0].Density;
        }

        if(mediaType.StartsWith("CD-", StringComparison.Ordinal)   ||
           mediaType.StartsWith("DDCD-", StringComparison.Ordinal) ||
           mediaType == "Audio CD")
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying CD TOC...").IsIndeterminate();

                mediaTest.CanReadTOC =
                    !_dev.ReadTocPmaAtip(out buffer, out senseBuffer, false, 0, 0, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadTOC);
            mediaTest.TocData = buffer;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying CD Full TOC...").IsIndeterminate();
                mediaTest.CanReadFullTOC = !_dev.ReadRawToc(out buffer, out senseBuffer, 1, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadFullTOC);
            mediaTest.FullTocData = buffer;
        }

        if(mediaType.StartsWith("CD-R", StringComparison.Ordinal) ||
           mediaType.StartsWith("DDCD-R", StringComparison.Ordinal))
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying CD ATIP...").IsIndeterminate();
                mediaTest.CanReadATIP = !_dev.ReadAtip(out buffer, out senseBuffer, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadATIP);

            mediaTest.AtipData = buffer;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying CD PMA...").IsIndeterminate();
                mediaTest.CanReadPMA = !_dev.ReadPma(out buffer, out senseBuffer, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPMA);

            mediaTest.PmaData = buffer;
        }

        if(mediaType.StartsWith("DVD-", StringComparison.Ordinal)    ||
           mediaType.StartsWith("DVD+", StringComparison.Ordinal)    ||
           mediaType.StartsWith("HD DVD-", StringComparison.Ordinal) ||
           mediaType.StartsWith("PD-", StringComparison.Ordinal))
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying DVD PFI...").IsIndeterminate();

                mediaTest.CanReadPFI = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                               MmcDiscStructureMediaType.Dvd, 0, 0,
                                                               MmcDiscStructureFormat.PhysicalInformation, 0,
                                                               _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPFI);

            mediaTest.PfiData = buffer;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying DVD DMI...").IsIndeterminate();

                mediaTest.CanReadDMI = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                               MmcDiscStructureMediaType.Dvd, 0, 0,
                                                               MmcDiscStructureFormat.DiscManufacturingInformation, 0,
                                                               _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDMI);

            mediaTest.DmiData = buffer;
        }

        if(mediaType == "DVD-ROM")
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying DVD CMI...").IsIndeterminate();

                mediaTest.CanReadCMI = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                               MmcDiscStructureMediaType.Dvd, 0, 0,
                                                               MmcDiscStructureFormat.CopyrightInformation, 0,
                                                               _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadCMI);

            mediaTest.CmiData = buffer;
        }

        switch(mediaType)
        {
            case "DVD-ROM":
            case "HD DVD-ROM":
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Querying DVD BCA...").IsIndeterminate();

                    mediaTest.CanReadBCA = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                   MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                   MmcDiscStructureFormat.BurstCuttingArea, 0,
                                                                   _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadBCA);

                mediaTest.DvdBcaData = buffer;

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Querying DVD AACS...").IsIndeterminate();

                    mediaTest.CanReadAACS = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                    MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                    MmcDiscStructureFormat.DvdAacs, 0, _dev.Timeout,
                                                                    out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadAACS);

                mediaTest.DvdAacsData = buffer;

                break;
            case "Nintendo GameCube game":
            case "Nintendo Wii game":
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Querying DVD BCA...").IsIndeterminate();

                    mediaTest.CanReadBCA = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                   MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                   MmcDiscStructureFormat.BurstCuttingArea, 0,
                                                                   _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadBCA);

                mediaTest.DvdBcaData = buffer;

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Querying DVD PFI...").IsIndeterminate();

                    mediaTest.CanReadPFI = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                   MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                   MmcDiscStructureFormat.PhysicalInformation, 0,
                                                                   _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPFI);

                mediaTest.PfiData = buffer;

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Querying DVD DMI...").IsIndeterminate();

                    mediaTest.CanReadDMI = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                   MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                   MmcDiscStructureFormat.DiscManufacturingInformation,
                                                                   0, _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDMI);

                mediaTest.DmiData = buffer;

                break;
            case "BD-ROM":
            case "Ultra HD Blu-ray movie":
            case "PlayStation 3 game":
            case "PlayStation 4 game":
            case "PlayStation 5 game":
            case "Xbox One game":
            case "Nintendo Wii U game":
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Querying BD BCA...").IsIndeterminate();

                    mediaTest.CanReadBCA = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                   MmcDiscStructureMediaType.Bd, 0, 0,
                                                                   MmcDiscStructureFormat.BdBurstCuttingArea, 0,
                                                                   _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadBCA);

                mediaTest.BluBcaData = buffer;

                break;
            case "DVD-RAM (1st gen, marked 2.6Gb or 5.2Gb)":
            case "DVD-RAM (2nd gen, marked 4.7Gb or 9.4Gb)":
            case "HD DVD-RAM":
            case "PD-650":
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Querying Disc Definition Structure...").IsIndeterminate();

                    mediaTest.CanReadDDS = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                   MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                   MmcDiscStructureFormat.DvdramDds, 0, _dev.Timeout,
                                                                   out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDDS);

                mediaTest.DvdDdsData = buffer;

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Querying Spare Area Information...").IsIndeterminate();

                    mediaTest.CanReadSpareAreaInformation =
                        !_dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                MmcDiscStructureFormat.DvdramSpareAreaInformation, 0, _dev.Timeout,
                                                out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadSpareAreaInformation);

                mediaTest.DvdSaiData = buffer;

                break;
        }

        if(mediaType.StartsWith("BD-R", StringComparison.Ordinal) &&
           mediaType != "BD-ROM")
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying BD DDS...").IsIndeterminate();

                mediaTest.CanReadDDS = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                               MmcDiscStructureMediaType.Bd, 0, 0,
                                                               MmcDiscStructureFormat.BdDds, 0, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDDS);

            mediaTest.BluDdsData = buffer;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying BD SAI...").IsIndeterminate();

                mediaTest.CanReadSpareAreaInformation =
                    !_dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.Bd, 0, 0,
                                            MmcDiscStructureFormat.BdSpareAreaInformation, 0, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadSpareAreaInformation);

            mediaTest.BluSaiData = buffer;
        }

        if(mediaType == "DVD-R" ||
           mediaType == "DVD-RW")
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying DVD PRI...").IsIndeterminate();

                mediaTest.CanReadPRI = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                               MmcDiscStructureMediaType.Dvd, 0, 0,
                                                               MmcDiscStructureFormat.PreRecordedInfo, 0, _dev.Timeout,
                                                               out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPRI);

            mediaTest.PriData = buffer;
        }

        if(mediaType == "DVD-R"  ||
           mediaType == "DVD-RW" ||
           mediaType == "HD DVD-R")
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying DVD Media ID...").IsIndeterminate();

                mediaTest.CanReadMediaID = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                   MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                   MmcDiscStructureFormat.DvdrMediaIdentifier, 0,
                                                                   _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadMediaID);

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying DVD Embossed PFI...").IsIndeterminate();

                mediaTest.CanReadRecordablePFI = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                         MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                         MmcDiscStructureFormat.DvdrPhysicalInformation,
                                                                         0, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRecordablePFI);

            mediaTest.EmbossedPfiData = buffer;
        }

        if(mediaType.StartsWith("DVD+R", StringComparison.Ordinal) ||
           mediaType == "DVD+MRW")
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying DVD ADIP...").IsIndeterminate();

                mediaTest.CanReadADIP = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                MmcDiscStructureFormat.Adip, 0, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadADIP);

            mediaTest.AdipData = buffer;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying DVD DCB...").IsIndeterminate();

                mediaTest.CanReadDCB = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                               MmcDiscStructureMediaType.Dvd, 0, 0,
                                                               MmcDiscStructureFormat.Dcb, 0, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDCB);

            mediaTest.DcbData = buffer;
        }

        if(mediaType == "HD DVD-ROM")
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying HD DVD CMI...").IsIndeterminate();

                mediaTest.CanReadHDCMI = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                 MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                 MmcDiscStructureFormat.HddvdCopyrightInformation, 0,
                                                                 _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadHDCMI);

            mediaTest.HdCmiData = buffer;
        }

        if(mediaType.EndsWith(" DL", StringComparison.Ordinal))
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying DVD Layer Capacity...").IsIndeterminate();

                mediaTest.CanReadLayerCapacity = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                         MmcDiscStructureMediaType.Dvd, 0, 0,
                                                                         MmcDiscStructureFormat.DvdrLayerCapacity, 0,
                                                                         _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadLayerCapacity);

            mediaTest.DvdLayerData = buffer;
        }

        if(mediaType.StartsWith("BD-R", StringComparison.Ordinal) ||
           mediaType == "Ultra HD Blu-ray movie"                  ||
           mediaType == "PlayStation 3 game"                      ||
           mediaType == "PlayStation 4 game"                      ||
           mediaType == "PlayStation 5 game"                      ||
           mediaType == "Xbox One game"                           ||
           mediaType == "Nintendo Wii game")
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying BD Disc Information...").IsIndeterminate();

                mediaTest.CanReadDiscInformation = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                                           MmcDiscStructureMediaType.Bd, 0, 0,
                                                                           MmcDiscStructureFormat.DiscInformation, 0,
                                                                           _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadDiscInformation);

            mediaTest.BluDiData = buffer;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying BD PAC...").IsIndeterminate();

                mediaTest.CanReadPAC = !_dev.ReadDiscStructure(out buffer, out senseBuffer,
                                                               MmcDiscStructureMediaType.Bd, 0, 0,
                                                               MmcDiscStructureFormat.Pac, 0, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPAC);

            mediaTest.BluPacData = buffer;
        }

        if(mediaType.StartsWith("CD-", StringComparison.Ordinal))
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying SCSI READ CD scrambled...").IsIndeterminate();

                mediaTest.CanReadCdScrambled = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2352, 1,
                                                            MmcSectorTypes.Cdda, false, false, false,
                                                            MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                            MmcSubchannel.None, _dev.Timeout, out _);
            });

            mediaTest.ReadCdScrambledData = buffer;
        }

        if(mediaType.StartsWith("PD-", StringComparison.Ordinal))
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying SCSI READ (6)...").IsIndeterminate();
                mediaTest.SupportsRead6 = !_dev.Read6(out buffer, out senseBuffer, 16, 512, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead6);

            mediaTest.Read6Data = buffer;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying SCSI READ (10)...").IsIndeterminate();

                mediaTest.SupportsRead10 = !_dev.Read10(out buffer, out senseBuffer, 0, false, true, false, false, 16,
                                                        512, 0, 1, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead10);

            mediaTest.Read10Data = buffer;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying SCSI READ (12)...").IsIndeterminate();

                mediaTest.SupportsRead12 = !_dev.Read12(out buffer, out senseBuffer, 0, false, true, false, false, 16,
                                                        512, 0, 1, false, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead12);

            mediaTest.Read12Data = buffer;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying SCSI READ (16)...").IsIndeterminate();

                mediaTest.SupportsRead16 = !_dev.Read16(out buffer, out senseBuffer, 0, false, true, false, 16, 512, 0,
                                                        1, false, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead16);

            mediaTest.Read16Data = buffer;
        }
        else
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying SCSI READ (6)...").IsIndeterminate();
                mediaTest.SupportsRead6 = !_dev.Read6(out buffer, out senseBuffer, 16, 2048, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead6);

            mediaTest.Read6Data = buffer;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying SCSI READ (10)...").IsIndeterminate();

                mediaTest.SupportsRead10 = !_dev.Read10(out buffer, out senseBuffer, 0, false, true, false, false, 16,
                                                        2048, 0, 1, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead10);

            mediaTest.Read10Data = buffer;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying SCSI READ (12)...").IsIndeterminate();

                mediaTest.SupportsRead12 = !_dev.Read12(out buffer, out senseBuffer, 0, false, true, false, false, 16,
                                                        2048, 0, 1, false, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead12);

            mediaTest.Read12Data = buffer;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying SCSI READ (16)...").IsIndeterminate();

                mediaTest.SupportsRead16 = !_dev.Read16(out buffer, out senseBuffer, 0, false, true, false, 16, 2048, 0,
                                                        1, false, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead16);

            mediaTest.Read16Data = buffer;
        }

        if(mediaType.StartsWith("CD-", StringComparison.Ordinal)   ||
           mediaType.StartsWith("DDCD-", StringComparison.Ordinal) ||
           mediaType == "Audio CD")
        {
            if(mediaType == "Audio CD")
            {
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying SCSI READ CD...").IsIndeterminate();

                    mediaTest.SupportsReadCd = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2352, 1,
                                                            MmcSectorTypes.Cdda, false, false, false,
                                                            MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                            MmcSubchannel.None, _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCd);

                mediaTest.ReadCdFullData = buffer;

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying SCSI READ CD MSF...").IsIndeterminate();

                    mediaTest.SupportsReadCdMsf = !_dev.ReadCdMsf(out buffer, out senseBuffer, 0x00000210, 0x00000211,
                                                                  2352, MmcSectorTypes.Cdda, false, false,
                                                                  MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                                  MmcSubchannel.None, _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCdMsf);

                mediaTest.ReadCdMsfFullData = buffer;
            }
            else
            {
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying SCSI READ CD...").IsIndeterminate();

                    mediaTest.SupportsReadCd = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2048, 1,
                                                            MmcSectorTypes.AllTypes, false, false, false,
                                                            MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                            MmcSubchannel.None, _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCd);

                mediaTest.ReadCdData = buffer;

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying SCSI READ CD MSF...").IsIndeterminate();

                    mediaTest.SupportsReadCdMsf = !_dev.ReadCdMsf(out buffer, out senseBuffer, 0x00000210, 0x00000211,
                                                                  2048, MmcSectorTypes.AllTypes, false, false,
                                                                  MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                                  MmcSubchannel.None, _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCdMsf);

                mediaTest.ReadCdMsfData = buffer;

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying SCSI READ CD full sector...").IsIndeterminate();

                    mediaTest.SupportsReadCdRaw = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2352, 1,
                                                               MmcSectorTypes.AllTypes, false, false, true,
                                                               MmcHeaderCodes.AllHeaders, true, true,
                                                               MmcErrorField.None, MmcSubchannel.None, _dev.Timeout,
                                                               out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCdRaw);

                mediaTest.ReadCdFullData = buffer;

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying SCSI READ CD MSF full sector...").IsIndeterminate();

                    mediaTest.SupportsReadCdMsfRaw = !_dev.ReadCdMsf(out buffer, out senseBuffer, 0x00000210,
                                                                     0x00000211, 2352, MmcSectorTypes.AllTypes, false,
                                                                     false, MmcHeaderCodes.AllHeaders, true, true,
                                                                     MmcErrorField.None, MmcSubchannel.None,
                                                                     _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsReadCdMsfRaw);

                mediaTest.ReadCdMsfFullData = buffer;
            }

            if(mediaTest.SupportsReadCdRaw == true ||
               mediaType                   == "Audio CD")
            {
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying to read CD Track 1 pre-gap...").IsIndeterminate();

                    for(int i = -10; i < 0; i++)
                    {
                        // ReSharper disable IntVariableOverflowInUncheckedContext
                        if(mediaType == "Audio CD")
                            sense = _dev.ReadCd(out buffer, out senseBuffer, (uint)i, 2352, 1, MmcSectorTypes.Cdda,
                                                false, false, false, MmcHeaderCodes.None, true, false,
                                                MmcErrorField.None, MmcSubchannel.None, _dev.Timeout, out _);
                        else
                            sense = _dev.ReadCd(out buffer, out senseBuffer, (uint)i, 2352, 1, MmcSectorTypes.AllTypes,
                                                false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                                MmcErrorField.None, MmcSubchannel.None, _dev.Timeout, out _);

                        // ReSharper restore IntVariableOverflowInUncheckedContext

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", sense);

                        mediaTest.Track1PregapData = buffer;

                        if(sense)
                            continue;

                        mediaTest.CanReadFirstTrackPreGap = true;

                        break;
                    }
                });

                mediaTest.CanReadFirstTrackPreGap ??= false;

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying to read CD Lead-In...").IsIndeterminate();

                    foreach(int i in new[]
                            {
                                -5000, -4000, -3000, -2000, -1000, -500, -250
                            })
                    {
                        if(mediaType == "Audio CD")
                            sense = _dev.ReadCd(out buffer, out senseBuffer, (uint)i, 2352, 1, MmcSectorTypes.Cdda,
                                                false, false, false, MmcHeaderCodes.None, true, false,
                                                MmcErrorField.None, MmcSubchannel.None, _dev.Timeout, out _);
                        else
                            sense = _dev.ReadCd(out buffer, out senseBuffer, (uint)i, 2352, 1, MmcSectorTypes.AllTypes,
                                                false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                                MmcErrorField.None, MmcSubchannel.None, _dev.Timeout, out _);

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", sense);

                        mediaTest.LeadInData = buffer;

                        if(sense)
                            continue;

                        mediaTest.CanReadLeadIn = true;

                        break;
                    }
                });

                mediaTest.CanReadLeadIn ??= false;

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying to read CD Lead-Out...").IsIndeterminate();

                    if(mediaType == "Audio CD")
                        mediaTest.CanReadLeadOut = !_dev.ReadCd(out buffer, out senseBuffer,
                                                                (uint)(mediaTest.Blocks + 1), 2352, 1,
                                                                MmcSectorTypes.Cdda, false, false, false,
                                                                MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                                MmcSubchannel.None, _dev.Timeout, out _);
                    else
                        mediaTest.CanReadLeadOut = !_dev.ReadCd(out buffer, out senseBuffer,
                                                                (uint)(mediaTest.Blocks + 1), 2352, 1,
                                                                MmcSectorTypes.AllTypes, false, false, true,
                                                                MmcHeaderCodes.AllHeaders, true, true,
                                                                MmcErrorField.None, MmcSubchannel.None, _dev.Timeout,
                                                                out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadLeadOut);

                mediaTest.LeadOutData = buffer;
            }

            if(mediaType == "Audio CD")
            {
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying to read C2 Pointers...").IsIndeterminate();
                    mediaTest.CanReadPMA = !_dev.ReadPma(out buffer, out senseBuffer, _dev.Timeout, out _);

                    // They return OK, but then all following commands make the drive fail miserably.
                    if(_dev.Model.StartsWith("iHOS104", StringComparison.Ordinal))
                        mediaTest.CanReadC2Pointers = false;
                    else
                    {
                        mediaTest.CanReadC2Pointers = !_dev.ReadCd(out buffer, out senseBuffer, 11, 2646, 1,
                                                                   MmcSectorTypes.Cdda, false, false, false,
                                                                   MmcHeaderCodes.None, true, false,
                                                                   MmcErrorField.C2Pointers, MmcSubchannel.None,
                                                                   _dev.Timeout, out _);

                        if(!mediaTest.CanReadC2Pointers == true)
                            mediaTest.CanReadC2Pointers = !_dev.ReadCd(out buffer, out senseBuffer, 11, 2648, 1,
                                                                       MmcSectorTypes.Cdda, false, false, false,
                                                                       MmcHeaderCodes.None, true, false,
                                                                       MmcErrorField.C2PointersAndBlock,
                                                                       MmcSubchannel.None, _dev.Timeout, out _);

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadC2Pointers);

                        mediaTest.C2PointersData = buffer;
                    }
                });

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying to read subchannels...").IsIndeterminate();

                    mediaTest.CanReadPQSubchannel = !_dev.ReadCd(out buffer, out senseBuffer, 11, 2368, 1,
                                                                 MmcSectorTypes.Cdda, false, false, false,
                                                                 MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                                 MmcSubchannel.Q16, _dev.Timeout, out _);

                    AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannel);

                    mediaTest.PQSubchannelData = buffer;

                    mediaTest.CanReadRWSubchannel = !_dev.ReadCd(out buffer, out senseBuffer, 11, 2448, 1,
                                                                 MmcSectorTypes.Cdda, false, false, false,
                                                                 MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                                 MmcSubchannel.Raw, _dev.Timeout, out _);

                    AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannel);

                    mediaTest.RWSubchannelData = buffer;

                    mediaTest.CanReadCorrectedSubchannel = !_dev.ReadCd(out buffer, out senseBuffer, 11, 2448, 1,
                                                                        MmcSectorTypes.Cdda, false, false, false,
                                                                        MmcHeaderCodes.None, true, false,
                                                                        MmcErrorField.None, MmcSubchannel.Rw,
                                                                        _dev.Timeout, out _);

                    AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadCorrectedSubchannel);

                    mediaTest.CorrectedSubchannelData = buffer;
                });

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying to read subchannels with C2 Pointers...").IsIndeterminate();
                    mediaTest.CanReadPMA = !_dev.ReadPma(out buffer, out senseBuffer, _dev.Timeout, out _);

                    // They return OK, but then all following commands make the drive fail miserably.
                    if(_dev.Model.StartsWith("iHOS104", StringComparison.Ordinal))
                    {
                        mediaTest.CanReadPQSubchannelWithC2        = false;
                        mediaTest.CanReadRWSubchannelWithC2        = false;
                        mediaTest.CanReadCorrectedSubchannelWithC2 = false;
                    }
                    else
                    {
                        mediaTest.CanReadPQSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 11, 2662, 1,
                                                                           MmcSectorTypes.Cdda, false, false, false,
                                                                           MmcHeaderCodes.None, true, false,
                                                                           MmcErrorField.C2Pointers, MmcSubchannel.Q16,
                                                                           _dev.Timeout, out _);

                        if(mediaTest.CanReadPQSubchannelWithC2 == false)
                            mediaTest.CanReadPQSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 11, 2664, 1,
                                                                               MmcSectorTypes.Cdda, false, false, false,
                                                                               MmcHeaderCodes.None, true, false,
                                                                               MmcErrorField.C2PointersAndBlock,
                                                                               MmcSubchannel.Q16, _dev.Timeout, out _);

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannelWithC2);

                        mediaTest.PQSubchannelWithC2Data = buffer;

                        mediaTest.CanReadRWSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 11, 2712, 1,
                                                                           MmcSectorTypes.Cdda, false, false, false,
                                                                           MmcHeaderCodes.None, true, false,
                                                                           MmcErrorField.C2Pointers, MmcSubchannel.Raw,
                                                                           _dev.Timeout, out _);

                        if(mediaTest.CanReadRWSubchannelWithC2 == false)
                            mediaTest.CanReadRWSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 11, 2714, 1,
                                                                               MmcSectorTypes.Cdda, false, false, false,
                                                                               MmcHeaderCodes.None, true, false,
                                                                               MmcErrorField.C2PointersAndBlock,
                                                                               MmcSubchannel.Raw, _dev.Timeout, out _);

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannelWithC2);

                        mediaTest.RWSubchannelWithC2Data = buffer;

                        mediaTest.CanReadCorrectedSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 11, 2712,
                                                                         1, MmcSectorTypes.Cdda, false, false,
                                                                         false, MmcHeaderCodes.None, true, false,
                                                                         MmcErrorField.C2Pointers,
                                                                         MmcSubchannel.Rw, _dev.Timeout, out _);

                        if(mediaTest.CanReadCorrectedSubchannelWithC2 == false)
                            mediaTest.CanReadCorrectedSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 11,
                                                                             2714, 1, MmcSectorTypes.Cdda, false,
                                                                             false, false, MmcHeaderCodes.None,
                                                                             true, false,
                                                                             MmcErrorField.C2PointersAndBlock,
                                                                             MmcSubchannel.Rw, _dev.Timeout,
                                                                             out _);

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                   !mediaTest.CanReadCorrectedSubchannelWithC2);

                        mediaTest.CorrectedSubchannelWithC2Data = buffer;
                    }
                });
            }
            else if(mediaTest.SupportsReadCdRaw == true)
            {
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying to read C2 Pointers...").IsIndeterminate();

                    // They return OK, but then all following commands make the drive fail miserably.
                    if(_dev.Model.StartsWith("iHOS104", StringComparison.Ordinal))
                        mediaTest.CanReadC2Pointers = false;
                    else
                    {
                        mediaTest.CanReadC2Pointers = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2646, 1,
                                                                   MmcSectorTypes.AllTypes, false, false, true,
                                                                   MmcHeaderCodes.AllHeaders, true, true,
                                                                   MmcErrorField.C2Pointers, MmcSubchannel.None,
                                                                   _dev.Timeout, out _);

                        if(mediaTest.CanReadC2Pointers == false)
                            mediaTest.CanReadC2Pointers = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2648, 1,
                                                                       MmcSectorTypes.AllTypes, false, false, true,
                                                                       MmcHeaderCodes.AllHeaders, true, true,
                                                                       MmcErrorField.C2PointersAndBlock,
                                                                       MmcSubchannel.None, _dev.Timeout, out _);

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadC2Pointers);

                        mediaTest.C2PointersData = buffer;
                    }
                });

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying to read subchannels...").IsIndeterminate();

                    mediaTest.CanReadPQSubchannel = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2368, 1,
                                                                 MmcSectorTypes.AllTypes, false, false, true,
                                                                 MmcHeaderCodes.AllHeaders, true, true,
                                                                 MmcErrorField.None, MmcSubchannel.Q16, _dev.Timeout,
                                                                 out _);

                    AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannel);

                    mediaTest.PQSubchannelData = buffer;

                    mediaTest.CanReadRWSubchannel = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2448, 1,
                                                                 MmcSectorTypes.AllTypes, false, false, true,
                                                                 MmcHeaderCodes.AllHeaders, true, true,
                                                                 MmcErrorField.None, MmcSubchannel.Raw, _dev.Timeout,
                                                                 out _);

                    AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannel);

                    mediaTest.RWSubchannelData = buffer;

                    mediaTest.CanReadCorrectedSubchannel = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2448, 1,
                                                                        MmcSectorTypes.AllTypes, false, false, true,
                                                                        MmcHeaderCodes.AllHeaders, true, true,
                                                                        MmcErrorField.None, MmcSubchannel.Rw,
                                                                        _dev.Timeout, out _);

                    AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadCorrectedSubchannel);

                    mediaTest.CorrectedSubchannelData = buffer;
                });

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying to read subchannels with C2 Pointers...").IsIndeterminate();

                    if(_dev.Model.StartsWith("iHOS104", StringComparison.Ordinal))
                    {
                        mediaTest.CanReadPQSubchannelWithC2        = false;
                        mediaTest.CanReadRWSubchannelWithC2        = false;
                        mediaTest.CanReadCorrectedSubchannelWithC2 = false;
                    }
                    else
                    {
                        mediaTest.CanReadPQSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2662, 1,
                                                                           MmcSectorTypes.AllTypes, false, false, true,
                                                                           MmcHeaderCodes.AllHeaders, true, true,
                                                                           MmcErrorField.C2Pointers, MmcSubchannel.Q16,
                                                                           _dev.Timeout, out _);

                        if(mediaTest.CanReadPQSubchannelWithC2 == false)
                            mediaTest.CanReadPQSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2664, 1,
                                                                               MmcSectorTypes.AllTypes, false, false,
                                                                               true, MmcHeaderCodes.AllHeaders, true,
                                                                               true, MmcErrorField.C2PointersAndBlock,
                                                                               MmcSubchannel.Q16, _dev.Timeout, out _);

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannelWithC2);

                        mediaTest.PQSubchannelWithC2Data = buffer;

                        mediaTest.CanReadRWSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2712, 1,
                                                                           MmcSectorTypes.AllTypes, false, false, true,
                                                                           MmcHeaderCodes.AllHeaders, true, true,
                                                                           MmcErrorField.C2Pointers, MmcSubchannel.Raw,
                                                                           _dev.Timeout, out _);

                        if(mediaTest.CanReadRWSubchannelWithC2 == false)
                            mediaTest.CanReadRWSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2714, 1,
                                                                               MmcSectorTypes.AllTypes, false, false,
                                                                               true, MmcHeaderCodes.AllHeaders, true,
                                                                               true, MmcErrorField.C2PointersAndBlock,
                                                                               MmcSubchannel.Raw, _dev.Timeout, out _);

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannelWithC2);

                        mediaTest.RWSubchannelWithC2Data = buffer;

                        mediaTest.CanReadCorrectedSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2712,
                                                                         1, MmcSectorTypes.AllTypes, false, false,
                                                                         true, MmcHeaderCodes.AllHeaders, true,
                                                                         true, MmcErrorField.C2Pointers,
                                                                         MmcSubchannel.Rw, _dev.Timeout, out _);

                        if(mediaTest.CanReadCorrectedSubchannelWithC2 == false)
                            mediaTest.CanReadCorrectedSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 16,
                                                                             2714, 1, MmcSectorTypes.AllTypes,
                                                                             false, false, true,
                                                                             MmcHeaderCodes.AllHeaders, true, true,
                                                                             MmcErrorField.C2PointersAndBlock,
                                                                             MmcSubchannel.Rw, _dev.Timeout,
                                                                             out _);

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                   !mediaTest.CanReadCorrectedSubchannelWithC2);

                        mediaTest.CorrectedSubchannelWithC2Data = buffer;
                    }
                });
            }
            else
            {
                if(_dev.Model.StartsWith("iHOS104", StringComparison.Ordinal))
                    mediaTest.CanReadC2Pointers = false;
                else
                {
                    Spectre.ProgressSingleSpinner(ctx =>
                    {
                        ctx.AddTask("Trying to read C2 Pointers...").IsIndeterminate();

                        mediaTest.CanReadC2Pointers = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2342, 1,
                                                                   MmcSectorTypes.AllTypes, false, false, false,
                                                                   MmcHeaderCodes.None, true, false,
                                                                   MmcErrorField.C2Pointers, MmcSubchannel.None,
                                                                   _dev.Timeout, out _);

                        if(mediaTest.CanReadC2Pointers == false)
                            mediaTest.CanReadC2Pointers = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2344, 1,
                                                                       MmcSectorTypes.AllTypes, false, false, false,
                                                                       MmcHeaderCodes.None, true, false,
                                                                       MmcErrorField.C2PointersAndBlock,
                                                                       MmcSubchannel.None, _dev.Timeout, out _);
                    });

                    AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadC2Pointers);

                    mediaTest.C2PointersData = buffer;
                }

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying to read subchannels...").IsIndeterminate();
                    mediaTest.CanReadPMA = !_dev.ReadPma(out buffer, out senseBuffer, _dev.Timeout, out _);

                    mediaTest.CanReadPQSubchannel = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2064, 1,
                                                                 MmcSectorTypes.AllTypes, false, false, false,
                                                                 MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                                 MmcSubchannel.Q16, _dev.Timeout, out _);

                    AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannel);

                    mediaTest.PQSubchannelData = buffer;

                    mediaTest.CanReadRWSubchannel = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2144, 1,
                                                                 MmcSectorTypes.AllTypes, false, false, false,
                                                                 MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                                 MmcSubchannel.Raw, _dev.Timeout, out _);

                    AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannel);

                    mediaTest.RWSubchannelData = buffer;

                    mediaTest.CanReadCorrectedSubchannel = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2144, 1,
                                                                        MmcSectorTypes.AllTypes, false, false, false,
                                                                        MmcHeaderCodes.None, true, false,
                                                                        MmcErrorField.None, MmcSubchannel.Rw,
                                                                        _dev.Timeout, out _);

                    AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadCorrectedSubchannel);

                    mediaTest.CorrectedSubchannelData = buffer;
                });

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying to read subchannels with C2 Pointers...").IsIndeterminate();

                    if(_dev.Model.StartsWith("iHOS104", StringComparison.Ordinal))
                    {
                        mediaTest.CanReadPQSubchannelWithC2        = false;
                        mediaTest.CanReadRWSubchannelWithC2        = false;
                        mediaTest.CanReadCorrectedSubchannelWithC2 = false;
                    }
                    else
                    {
                        mediaTest.CanReadPQSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2358, 1,
                                                                           MmcSectorTypes.AllTypes, false, false, false,
                                                                           MmcHeaderCodes.None, true, false,
                                                                           MmcErrorField.C2Pointers, MmcSubchannel.Q16,
                                                                           _dev.Timeout, out _);

                        if(mediaTest.CanReadPQSubchannelWithC2 == false)
                            mediaTest.CanReadPQSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2360, 1,
                                                                               MmcSectorTypes.AllTypes, false, false,
                                                                               false, MmcHeaderCodes.None, true, false,
                                                                               MmcErrorField.C2PointersAndBlock,
                                                                               MmcSubchannel.Q16, _dev.Timeout, out _);

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadPQSubchannelWithC2);

                        mediaTest.PQSubchannelWithC2Data = buffer;

                        mediaTest.CanReadRWSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2438, 1,
                                                                           MmcSectorTypes.AllTypes, false, false, false,
                                                                           MmcHeaderCodes.None, true, false,
                                                                           MmcErrorField.C2Pointers, MmcSubchannel.Raw,
                                                                           _dev.Timeout, out _);

                        if(mediaTest.CanReadRWSubchannelWithC2 == false)
                            mediaTest.CanReadRWSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2440, 1,
                                                                               MmcSectorTypes.AllTypes, false, false,
                                                                               false, MmcHeaderCodes.None, true, false,
                                                                               MmcErrorField.C2PointersAndBlock,
                                                                               MmcSubchannel.Raw, _dev.Timeout, out _);

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadRWSubchannelWithC2);

                        mediaTest.RWSubchannelWithC2Data = buffer;

                        mediaTest.CanReadCorrectedSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 16, 2438,
                                                                         1, MmcSectorTypes.AllTypes, false, false,
                                                                         false, MmcHeaderCodes.None, true, false,
                                                                         MmcErrorField.C2Pointers,
                                                                         MmcSubchannel.Rw, _dev.Timeout, out _);

                        if(mediaTest.CanReadCorrectedSubchannelWithC2 == false)
                            mediaTest.CanReadCorrectedSubchannelWithC2 = !_dev.ReadCd(out buffer, out senseBuffer, 16,
                                                                             2440, 1, MmcSectorTypes.AllTypes,
                                                                             false, false, false,
                                                                             MmcHeaderCodes.None, true, false,
                                                                             MmcErrorField.C2PointersAndBlock,
                                                                             MmcSubchannel.Rw, _dev.Timeout,
                                                                             out _);

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}",
                                                   !mediaTest.CanReadCorrectedSubchannelWithC2);

                        mediaTest.CorrectedSubchannelWithC2Data = buffer;
                    }
                });
            }

            if(tryPlextor)
            {
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying Plextor READ CD-DA...").IsIndeterminate();

                    mediaTest.SupportsPlextorReadCDDA =
                        !_dev.PlextorReadCdDa(out buffer, out senseBuffer, 16, 2352, 1, PlextorSubchannel.None,
                                              _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsPlextorReadCDDA);

                mediaTest.PlextorReadCddaData = buffer;
            }

            if(tryPioneer)
            {
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying Pioneer READ CD-DA...").IsIndeterminate();

                    mediaTest.SupportsPioneerReadCDDA =
                        !_dev.PioneerReadCdDa(out buffer, out senseBuffer, 16, 2352, 1, PioneerSubchannel.None,
                                              _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsPioneerReadCDDA);

                mediaTest.PioneerReadCddaData = buffer;

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying Pioneer READ CD-DA MSF...").IsIndeterminate();

                    mediaTest.SupportsPioneerReadCDDAMSF =
                        !_dev.PioneerReadCdDaMsf(out buffer, out senseBuffer, 0x00000210, 0x00000211, 2352,
                                                 PioneerSubchannel.None, _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsPioneerReadCDDAMSF);

                mediaTest.PioneerReadCddaMsfData = buffer;
            }

            if(tryNec)
            {
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Trying NEC READ CD-DA...").IsIndeterminate();

                    mediaTest.SupportsNECReadCDDA =
                        !_dev.NecReadCdDa(out buffer, out senseBuffer, 16, 1, _dev.Timeout, out _);
                });

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsNECReadCDDA);

                mediaTest.NecReadCddaData = buffer;
            }
        }

        mediaTest.LongBlockSize = mediaTest.BlockSize;

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Trying SCSI READ LONG (10)...").IsIndeterminate();
            sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 16, 0xFFFF, _dev.Timeout, out _);
        });

        if(sense && !_dev.Error)
        {
            DecodedSense? decSense = Sense.Decode(senseBuffer);

            if(decSense?.SenseKey  == SenseKeys.IllegalRequest &&
               decSense.Value.ASC  == 0x24                     &&
               decSense.Value.ASCQ == 0x00)
            {
                mediaTest.SupportsReadLong = true;

                bool valid       = decSense?.Fixed?.InformationValid == true;
                bool ili         = decSense?.Fixed?.ILI              == true;
                uint information = decSense?.Fixed?.Information ?? 0;

                if(decSense?.Descriptor.HasValue == true &&
                   decSense.Value.Descriptor.Value.Descriptors.TryGetValue(0, out byte[] desc00))
                {
                    valid       = true;
                    ili         = true;
                    information = (uint)Sense.DecodeDescriptor00(desc00);
                }

                if(valid && ili)
                    mediaTest.LongBlockSize = 0xFFFF - (information & 0xFFFF);
            }
        }

        if(mediaTest.SupportsReadLong == true &&
           mediaTest.LongBlockSize    == mediaTest.BlockSize)
        {
            // DVDs
            sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 16, 37856, _dev.Timeout, out _);

            if(!sense &&
               !_dev.Error)
            {
                mediaTest.ReadLong10Data   = buffer;
                mediaTest.SupportsReadLong = true;
                mediaTest.LongBlockSize    = 37856;
            }
        }

        if(tryPlextor)
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying Plextor trick to raw read DVDs...").IsIndeterminate();

                mediaTest.SupportsPlextorReadRawDVD =
                    !_dev.PlextorReadRawDvd(out buffer, out senseBuffer, 16, 1, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsPlextorReadRawDVD);

            if(mediaTest.SupportsPlextorReadRawDVD == true)
                mediaTest.SupportsPlextorReadRawDVD = !ArrayHelpers.ArrayIsNullOrEmpty(buffer);

            if(mediaTest.SupportsPlextorReadRawDVD == true)
                mediaTest.PlextorReadRawDVDData = buffer;
        }

        if(tryHldtst)
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying HL-DT-ST (aka LG) trick to raw read DVDs...").IsIndeterminate();

                mediaTest.SupportsHLDTSTReadRawDVD =
                    !_dev.HlDtStReadRawDvd(out buffer, out senseBuffer, 16, 1, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsHLDTSTReadRawDVD);

            if(mediaTest.SupportsHLDTSTReadRawDVD == true)
                mediaTest.SupportsHLDTSTReadRawDVD = !ArrayHelpers.ArrayIsNullOrEmpty(buffer);

            if(mediaTest.SupportsHLDTSTReadRawDVD == true)
                mediaTest.HLDTSTReadRawDVDData = buffer;
        }

        if(tryMediaTekF106)
        {
            var triedLba0    = false;
            var triedLeadOut = false;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying MediaTek READ DRAM command...").IsIndeterminate();

                if(mediaType                == "Audio CD" &&
                   mediaTest.SupportsReadCd == true)
                {
                    _dev.ReadCd(out _, out _, 0, 2352, 1, MmcSectorTypes.Cdda, false, false, false, MmcHeaderCodes.None,
                                true, false, MmcErrorField.None, MmcSubchannel.None, _dev.Timeout, out _);

                    triedLba0 = true;
                }
                else if((mediaType.StartsWith("CD", StringComparison.OrdinalIgnoreCase) ||
                         mediaType == "Enhanced CD (aka E-CD, CD-Plus or CD+)") &&
                        mediaTest.SupportsReadCdRaw == true)
                {
                    _dev.ReadCd(out _, out _, 0, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                                MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None,
                                _dev.Timeout, out _);

                    triedLba0 = true;
                }
                else if((mediaType.StartsWith("CD", StringComparison.OrdinalIgnoreCase) ||
                         mediaType == "Enhanced CD (aka E-CD, CD-Plus or CD+)") &&
                        mediaTest.SupportsReadCd == true)
                {
                    _dev.ReadCd(out _, out _, 0, 2048, 1, MmcSectorTypes.AllTypes, false, false, false,
                                MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None, _dev.Timeout,
                                out _);

                    triedLba0 = true;
                }
                else if(mediaTest.SupportsRead6 == true)
                {
                    _dev.Read6(out _, out _, 0, 2048, _dev.Timeout, out _);
                    triedLba0 = true;
                }
                else if(mediaTest.SupportsRead10 == true)
                {
                    _dev.Read10(out _, out _, 0, false, true, false, false, 0, 2048, 0, 1, _dev.Timeout, out _);
                    triedLba0 = true;
                }
                else if(mediaTest.SupportsRead12 == true)
                {
                    _dev.Read12(out _, out _, 0, false, true, false, false, 0, 2048, 0, 1, false, _dev.Timeout, out _);

                    triedLba0 = true;
                }
                else if(mediaTest.SupportsRead16 == true)
                {
                    _dev.Read16(out _, out _, 0, false, true, false, 0, 2048, 0, 1, false, _dev.Timeout, out _);
                    triedLba0 = true;
                }

                if(!triedLba0)
                    return;

                mediaTest.CanReadF1_06 =
                    !_dev.MediaTekReadDram(out buffer, out senseBuffer, 0, 0xB00, _dev.Timeout, out _);

                mediaTest.ReadF1_06Data = mediaTest.CanReadF1_06 == true ? buffer : senseBuffer;

                AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadF1_06);
            });

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Trying MediaTek READ DRAM command for Lead-Out...").IsIndeterminate();

                if(mediaTest.Blocks > 0)
                {
                    if(mediaType                == "Audio CD" &&
                       mediaTest.SupportsReadCd == true)
                    {
                        _dev.ReadCd(out _, out _, (uint)(mediaTest.Blocks + 1), 2352, 1, MmcSectorTypes.Cdda, false,
                                    false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                    MmcSubchannel.None, _dev.Timeout, out _);

                        triedLeadOut = true;
                    }
                    else if((mediaType.StartsWith("CD", StringComparison.OrdinalIgnoreCase) ||
                             mediaType == "Enhanced CD (aka E-CD, CD-Plus or CD+)") &&
                            mediaTest.SupportsReadCdRaw == true)
                    {
                        _dev.ReadCd(out _, out _, (uint)(mediaTest.Blocks + 1), 2352, 1, MmcSectorTypes.AllTypes, false,
                                    false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                    MmcSubchannel.None, _dev.Timeout, out _);

                        triedLeadOut = true;
                    }
                    else if((mediaType.StartsWith("CD", StringComparison.OrdinalIgnoreCase) ||
                             mediaType == "Enhanced CD (aka E-CD, CD-Plus or CD+)") &&
                            mediaTest.SupportsReadCd == true)
                    {
                        _dev.ReadCd(out _, out _, (uint)(mediaTest.Blocks + 1), 2048, 1, MmcSectorTypes.AllTypes, false,
                                    false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                    MmcSubchannel.None, _dev.Timeout, out _);

                        triedLeadOut = true;
                    }
                    else if(mediaTest.SupportsRead6 == true)
                    {
                        _dev.Read6(out _, out _, (uint)(mediaTest.Blocks + 1), 2048, _dev.Timeout, out _);
                        triedLeadOut = true;
                    }
                    else if(mediaTest.SupportsRead10 == true)
                    {
                        _dev.Read10(out _, out _, 0, false, true, false, false, (uint)(mediaTest.Blocks + 1), 2048, 0,
                                    1, _dev.Timeout, out _);

                        triedLeadOut = true;
                    }
                    else if(mediaTest.SupportsRead12 == true)
                    {
                        _dev.Read12(out _, out _, 0, false, true, false, false, (uint)(mediaTest.Blocks + 1), 2048, 0,
                                    1, false, _dev.Timeout, out _);

                        triedLeadOut = true;
                    }
                    else if(mediaTest.SupportsRead16 == true)
                    {
                        _dev.Read16(out _, out _, 0, false, true, false, (ulong)(mediaTest.Blocks + 1), 2048, 0, 1,
                                    false, _dev.Timeout, out _);

                        triedLeadOut = true;
                    }

                    if(triedLeadOut)
                    {
                        mediaTest.CanReadF1_06LeadOut =
                            !_dev.MediaTekReadDram(out buffer, out senseBuffer, 0, 0xB00, _dev.Timeout, out _);

                        mediaTest.ReadF1_06LeadOutData = mediaTest.CanReadF1_06LeadOut == true ? buffer : senseBuffer;

                        // This means it has returned the same as previous read, so not really lead-out.
                        if(mediaTest.CanReadF1_06        == true &&
                           mediaTest.CanReadF1_06LeadOut == true &&
                           mediaTest.ReadF1_06Data.SequenceEqual(mediaTest.ReadF1_06LeadOutData))
                        {
                            mediaTest.CanReadF1_06LeadOut  = false;
                            mediaTest.ReadF1_06LeadOutData = senseBuffer;
                        }

                        AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadF1_06LeadOut);
                    }
                }
            });
        }

        // This is for checking multi-session support, and inter-session lead-in/out reading, as Enhanced CD are
        if(mediaType == "Enhanced CD (aka E-CD, CD-Plus or CD+)")
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Querying CD Full TOC...").IsIndeterminate();
                mediaTest.CanReadFullTOC = !_dev.ReadRawToc(out buffer, out senseBuffer, 1, _dev.Timeout, out _);
            });

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadFullTOC);

            mediaTest.FullTocData = buffer;

            if(mediaTest.CanReadFullTOC == true)
            {
                FullTOC.CDFullTOC? decodedTocNullable = FullTOC.Decode(buffer);

                mediaTest.CanReadFullTOC = decodedTocNullable.HasValue;

                if(mediaTest.CanReadFullTOC == true)
                {
                    FullTOC.CDFullTOC decodedToc = decodedTocNullable.Value;

                    if(!decodedToc.TrackDescriptors.Any(t => t.SessionNumber > 1))
                    {
                        AaruConsole.
                            ErrorWriteLine("Could not find second session. Have you inserted the correct type of disc?");

                        return null;
                    }

                    FullTOC.TrackDataDescriptor firstSessionLeadOutTrack =
                        decodedToc.TrackDescriptors.FirstOrDefault(t => t.SessionNumber == 1 && t.POINT == 0xA2);

                    FullTOC.TrackDataDescriptor secondSessionFirstTrack =
                        decodedToc.TrackDescriptors.FirstOrDefault(t => t.SessionNumber > 1 && t.POINT <= 99);

                    if(firstSessionLeadOutTrack.SessionNumber == 0 ||
                       secondSessionFirstTrack.SessionNumber  == 0)
                    {
                        AaruConsole.
                            ErrorWriteLine("Could not find second session. Have you inserted the correct type of disc?");

                        return null;
                    }

                    AaruConsole.DebugWriteLine("SCSI Report", "First session Lead-Out starts at {0:D2}:{1:D2}:{2:D2}",
                                               firstSessionLeadOutTrack.PMIN, firstSessionLeadOutTrack.PSEC,
                                               firstSessionLeadOutTrack.PFRAME);

                    AaruConsole.DebugWriteLine("SCSI Report", "Second session starts at {0:D2}:{1:D2}:{2:D2}",
                                               secondSessionFirstTrack.PMIN, secondSessionFirstTrack.PSEC,
                                               secondSessionFirstTrack.PFRAME);

                    // Skip Lead-Out pre-gap
                    var firstSessionLeadOutLba = (uint)(firstSessionLeadOutTrack.PMIN * 60 * 75 +
                                                        firstSessionLeadOutTrack.PSEC * 75      +
                                                        firstSessionLeadOutTrack.PFRAME         + 150);

                    // Skip second session track pre-gap
                    var secondSessionLeadInLba = (uint)(secondSessionFirstTrack.PMIN * 60 * 75 +
                                                        secondSessionFirstTrack.PSEC * 75      +
                                                        secondSessionFirstTrack.PFRAME - 300);

                    Spectre.ProgressSingleSpinner(ctx =>
                    {
                        ctx.AddTask("Trying SCSI READ CD in first session Lead-Out...").IsIndeterminate();

                        mediaTest.CanReadingIntersessionLeadOut = !_dev.ReadCd(out buffer, out senseBuffer,
                                                                               firstSessionLeadOutLba, 2448, 1,
                                                                               MmcSectorTypes.AllTypes, false, false,
                                                                               false, MmcHeaderCodes.AllHeaders, true,
                                                                               false, MmcErrorField.None,
                                                                               MmcSubchannel.Raw, _dev.Timeout, out _);

                        if(mediaTest.CanReadingIntersessionLeadOut == false)
                        {
                            mediaTest.CanReadingIntersessionLeadOut = !_dev.ReadCd(out buffer, out senseBuffer,
                                                                          firstSessionLeadOutLba, 2368, 1,
                                                                          MmcSectorTypes.AllTypes, false, false,
                                                                          false, MmcHeaderCodes.AllHeaders, true,
                                                                          false, MmcErrorField.None,
                                                                          MmcSubchannel.Q16, _dev.Timeout, out _);

                            if(mediaTest.CanReadingIntersessionLeadOut == false)
                                mediaTest.CanReadingIntersessionLeadOut = !_dev.ReadCd(out buffer, out senseBuffer,
                                                                              firstSessionLeadOutLba, 2352, 1,
                                                                              MmcSectorTypes.AllTypes, false,
                                                                              false, false,
                                                                              MmcHeaderCodes.AllHeaders, true,
                                                                              false, MmcErrorField.None,
                                                                              MmcSubchannel.None, _dev.Timeout,
                                                                              out _);
                        }
                    });

                    AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadingIntersessionLeadOut);

                    mediaTest.IntersessionLeadOutData = buffer;

                    Spectre.ProgressSingleSpinner(ctx =>
                    {
                        ctx.AddTask("Trying SCSI READ CD in second session Lead-In...").IsIndeterminate();

                        mediaTest.CanReadingIntersessionLeadIn = !_dev.ReadCd(out buffer, out senseBuffer,
                                                                              secondSessionLeadInLba, 2448, 1,
                                                                              MmcSectorTypes.AllTypes, false, false,
                                                                              false, MmcHeaderCodes.AllHeaders, true,
                                                                              false, MmcErrorField.None,
                                                                              MmcSubchannel.Raw, _dev.Timeout, out _);

                        if(mediaTest.CanReadingIntersessionLeadIn == false)
                        {
                            mediaTest.CanReadingIntersessionLeadIn = !_dev.ReadCd(out buffer, out senseBuffer,
                                                                         secondSessionLeadInLba, 2368, 1,
                                                                         MmcSectorTypes.AllTypes, false, false,
                                                                         false, MmcHeaderCodes.AllHeaders, true,
                                                                         false, MmcErrorField.None,
                                                                         MmcSubchannel.Q16, _dev.Timeout, out _);

                            if(mediaTest.CanReadingIntersessionLeadIn == false)
                                mediaTest.CanReadingIntersessionLeadIn = !_dev.ReadCd(out buffer, out senseBuffer,
                                                                             secondSessionLeadInLba, 2352, 1,
                                                                             MmcSectorTypes.AllTypes, false, false,
                                                                             false, MmcHeaderCodes.AllHeaders,
                                                                             true, false, MmcErrorField.None,
                                                                             MmcSubchannel.None, _dev.Timeout,
                                                                             out _);
                        }
                    });

                    AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.CanReadingIntersessionLeadIn);

                    mediaTest.IntersessionLeadInData = buffer;
                }
            }
        }

        mediaTest.Blocks = null;

        return mediaTest;
    }
}