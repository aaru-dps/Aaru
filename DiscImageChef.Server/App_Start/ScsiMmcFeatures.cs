// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ScsiMmcFeatures.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MMC features from reports.
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

using System.Collections.Generic;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Metadata;

namespace DiscImageChef.Server.App_Start
{
    public static class ScsiMmcFeatures
    {
        public static void Report(mmcFeaturesType ftr, ref List<string> mmcOneValue)
        {
            if(ftr.SupportsAACS && ftr.AACSVersionSpecified)
                mmcOneValue.Add($"Drive supports AACS version {ftr.AACSVersion}");
            else if(ftr.SupportsAACS) mmcOneValue.Add("Drive supports AACS");
            if(ftr.AGIDsSpecified) mmcOneValue.Add($"Drive supports {ftr.AGIDs} AGIDs concurrently");
            if(ftr.CanGenerateBindingNonce)
            {
                mmcOneValue.Add("Drive supports generating the binding nonce");
                if(ftr.BindingNonceBlocksSpecified)
                    mmcOneValue.Add($"{ftr.BindingNonceBlocks} media blocks are required for the binding nonce");
            }
            if(ftr.BlocksPerReadableUnit > 1)
                mmcOneValue.Add($"{ftr.BlocksPerReadableUnit} logical blocks per media writable unit");
            if(ftr.BufferUnderrunFreeInDVD) mmcOneValue.Add("Drive supports zero loss linking writing DVDs");
            if(ftr.BufferUnderrunFreeInSAO) mmcOneValue.Add("Drive supports zero loss linking in Session at Once Mode");
            if(ftr.BufferUnderrunFreeInTAO) mmcOneValue.Add("Drive supports zero loss linking in Track at Once Mode");
            if(ftr.CanAudioScan) mmcOneValue.Add("Drive supports the SCAN command");
            if(ftr.CanEject) mmcOneValue.Add("Drive can eject media");
            if(ftr.CanEraseSector) mmcOneValue.Add("Drive supports media that require erasing before writing");
            if(ftr.CanExpandBDRESpareArea) mmcOneValue.Add("Drive can expand the spare area on a formatted BD-RE disc");
            if(ftr.CanFormat) mmcOneValue.Add("Drive can format media into logical blocks");
            if(ftr.CanFormatBDREWithoutSpare) mmcOneValue.Add("Drive can format BD-RE with no spares allocated");
            if(ftr.CanFormatQCert) mmcOneValue.Add("Drive can format BD-RE discs with quick certification");
            if(ftr.CanFormatCert) mmcOneValue.Add("Drive can format BD-RE discs with full certification");
            if(ftr.CanFormatFRF) mmcOneValue.Add("Drive can fast re-format BD-RE discs");
            if(ftr.CanFormatRRM) mmcOneValue.Add("Drive can format BD-R discs with RRM format");
            if(ftr.CanLoad) mmcOneValue.Add("Drive can load media");
            if(ftr.CanMuteSeparateChannels) mmcOneValue.Add("Drive is able to mute channels separately");
            if(ftr.CanOverwriteSAOTrack) mmcOneValue.Add("Drive can overwrite a SAO track with another in CD-RWs");
            if(ftr.CanOverwriteTAOTrack) mmcOneValue.Add("Drive can overwrite a TAO track with another in CD-RWs");
            if(ftr.CanPlayCDAudio) mmcOneValue.Add("Drive has an analogue audio output");
            if(ftr.CanPseudoOverwriteBDR) mmcOneValue.Add("Drive can write BD-R on Pseudo-OVerwrite SRM mode");
            if(ftr.CanReadAllDualR) mmcOneValue.Add("Drive can read DVD-R DL from all recording modes");
            if(ftr.CanReadAllDualRW) mmcOneValue.Add("Drive can read DVD-RW DL from all recording modes");
            if(ftr.CanReadBD) mmcOneValue.Add("Drive can read BD-ROM");
            if(ftr.CanReadBDR) mmcOneValue.Add("Drive can read BD-R Ver.1");
            if(ftr.CanReadBDRE1) mmcOneValue.Add("Drive can read BD-RE Ver.1");
            if(ftr.CanReadBDRE2) mmcOneValue.Add("Drive can read BD-RE Ver.2");
            if(ftr.CanReadBDROM) mmcOneValue.Add("Drive can read BD-ROM Ver.1");
            if(ftr.CanReadBluBCA) mmcOneValue.Add("Drive can read BD's Burst Cutting Area");
            if(ftr.CanReadCD) mmcOneValue.Add("Drive can read CD-ROM");
            if(ftr.CanWriteCDMRW && ftr.CanReadDVDPlusMRW && ftr.CanWriteDVDPlusMRW)
                mmcOneValue.Add("Drive can read and write CD-MRW and DVD+MRW");
            else if(ftr.CanReadDVDPlusMRW && ftr.CanWriteDVDPlusMRW)
                mmcOneValue.Add("Drive can read and write DVD+MRW");
            else if(ftr.CanWriteCDMRW && ftr.CanReadDVDPlusMRW)
                mmcOneValue.Add("Drive and read DVD+MRW and read and write CD-MRW");
            else if(ftr.CanWriteCDMRW) mmcOneValue.Add("Drive can read and write CD-MRW");
            else if(ftr.CanReadDVDPlusMRW) mmcOneValue.Add("Drive can read CD-MRW and DVD+MRW");
            else if(ftr.CanReadCDMRW) mmcOneValue.Add("Drive can read CD-MRW");
            if(ftr.CanReadCPRM_MKB) mmcOneValue.Add("Drive supports reading Media Key Block of CPRM");
            if(ftr.CanReadDDCD) mmcOneValue.Add("Drive can read DDCDs");
            if(ftr.CanReadDVD) mmcOneValue.Add("Drive can read DVD");
            if(ftr.CanWriteDVDPlusRW) mmcOneValue.Add("Drive can read and write DVD+RW");
            else if(ftr.CanReadDVDPlusRW) mmcOneValue.Add("Drive can read DVD+RW");
            if(ftr.CanWriteDVDPlusR) mmcOneValue.Add("Drive can read and write DVD+R");
            else if(ftr.CanReadDVDPlusR) mmcOneValue.Add("Drive can read DVD+R");
            if(ftr.CanWriteDVDPlusRDL) mmcOneValue.Add("Drive can read and write DVD+R DL");
            else if(ftr.CanReadDVDPlusRDL) mmcOneValue.Add("Drive can read DVD+R DL");
            if(ftr.CanReadDriveAACSCertificate) mmcOneValue.Add("Drive supports reading the Drive Certificate");
            if(ftr.CanReadHDDVD && ftr.CanReadHDDVDR && ftr.CanReadHDDVDRAM)
                mmcOneValue.Add("Drive can read HD DVD-ROM, HD DVD-RW, HD DVD-R and HD DVD-RAM");
            else if(ftr.CanReadHDDVD && ftr.CanReadHDDVDR)
                mmcOneValue.Add("Drive can read HD DVD-ROM, HD DVD-RW and HD DVD-R");
            else if(ftr.CanReadHDDVD && ftr.CanReadHDDVDRAM)
                mmcOneValue.Add("Drive can read HD DVD-ROM, HD DVD-RW and HD DVD-RAM");
            else if(ftr.CanReadHDDVD) mmcOneValue.Add("Drive can read HD DVD-ROM and HD DVD-RW");
            if(ftr.CanReadLeadInCDText) mmcOneValue.Add("Drive can return CD-Text from Lead-In");
            if(ftr.CanReadOldBDR) mmcOneValue.Add("Drive can read BD-R pre-1.0");
            if(ftr.CanReadOldBDRE) mmcOneValue.Add("Drive can read BD-RE pre-1.0");
            if(ftr.CanReadOldBDROM) mmcOneValue.Add("Drive can read BD-ROM pre-1.0");
            if(ftr.CanReadSpareAreaInformation) mmcOneValue.Add("Drive can return Spare Area Information");
            if(ftr.CanReportDriveSerial) mmcOneValue.Add("Drive is to report drive serial number");
            if(ftr.CanReportMediaSerial) mmcOneValue.Add("Drive is to read media serial number");
            if(ftr.CanTestWriteDDCDR) mmcOneValue.Add("Drive can do a test writing with DDCD-R");
            if(ftr.CanTestWriteDVD) mmcOneValue.Add("Drive can do a test writing with DVDs");
            if(ftr.CanTestWriteInSAO) mmcOneValue.Add("Drive can do a test writing in Session at Once Mode");
            if(ftr.CanTestWriteInTAO) mmcOneValue.Add("Drive can do a test writing in Track at Once Mode");
            if(ftr.CanUpgradeFirmware) mmcOneValue.Add("Drive supports Microcode Upgrade");
            if(ftr.ErrorRecoveryPage) mmcOneValue.Add("Drive shall report Read/Write Error Recovery mode page");
            if(ftr.Locked) mmcOneValue.Add("Drive can lock media");
            if(ftr.LogicalBlockSize > 0)
                mmcOneValue.Add($"{ftr.LogicalBlockSize} bytes per logical block");
            if(ftr.MultiRead)
                mmcOneValue.Add("Drive claims capability to read all CD formats according to OSTA Multi-Read Specification");

            switch(ftr.PhysicalInterfaceStandard)
            {
                case PhysicalInterfaces.Unspecified:
                    mmcOneValue.Add("Drive uses an unspecified physical interface");
                    break;
                case PhysicalInterfaces.SCSI:
                    mmcOneValue.Add("Drive uses SCSI interface");
                    break;
                case PhysicalInterfaces.ATAPI:
                    mmcOneValue.Add("Drive uses ATAPI interface");
                    break;
                case PhysicalInterfaces.IEEE1394:
                    mmcOneValue.Add("Drive uses IEEE-1394 interface");
                    break;
                case PhysicalInterfaces.IEEE1394A:
                    mmcOneValue.Add("Drive uses IEEE-1394A interface");
                    break;
                case PhysicalInterfaces.FC:
                    mmcOneValue.Add("Drive uses Fibre Channel interface");
                    break;
                case PhysicalInterfaces.IEEE1394B:
                    mmcOneValue.Add("Drive uses IEEE-1394B interface");
                    break;
                case PhysicalInterfaces.SerialATAPI:
                    mmcOneValue.Add("Drive uses Serial ATAPI interface");
                    break;
                case PhysicalInterfaces.USB:
                    mmcOneValue.Add("Drive uses USB interface");
                    break;
                case PhysicalInterfaces.Vendor:
                    mmcOneValue.Add("Drive uses a vendor unique interface");
                    break;
                default:
                    mmcOneValue.Add($"Drive uses an unknown interface with code {(uint)ftr.PhysicalInterfaceStandard}");
                    break;
            }

            if(ftr.PreventJumper) mmcOneValue.Add("Drive power ups locked");
            if(ftr.SupportsBusEncryption) mmcOneValue.Add("Drive supports bus encryption");
            if(ftr.CanWriteBD) mmcOneValue.Add("Drive can write BD-R or BD-RE");
            if(ftr.CanWriteBDR) mmcOneValue.Add("Drive can write BD-R Ver.1");
            if(ftr.CanWriteBDRE1) mmcOneValue.Add("Drive can write BD-RE Ver.1");
            if(ftr.CanWriteBDRE2) mmcOneValue.Add("Drive can write BD-RE Ver.2");
            if(ftr.CanWriteBusEncryptedBlocks) mmcOneValue.Add("Drive supports writing with bus encryption");
            if(ftr.CanWriteCDRW) mmcOneValue.Add("Drive can write CD-RW");
            if(ftr.CanWriteCDRWCAV) mmcOneValue.Add("Drive can write High-Speed CD-RW");
            if(ftr.CanWriteCDSAO && !ftr.CanWriteRaw) mmcOneValue.Add("Drive can write CDs in Session at Once Mode:");
            else if(!ftr.CanWriteCDSAO && ftr.CanWriteRaw) mmcOneValue.Add("Drive can write CDs in raw Mode:");
            else if(ftr.CanWriteCDSAO && ftr.CanWriteRaw)
                mmcOneValue.Add("Drive can write CDs in Session at Once and in Raw Modes:");
            if(ftr.CanWriteCDTAO) mmcOneValue.Add("Drive can write CDs in Track at Once Mode:");
            if(ftr.CanWriteCSSManagedDVD) mmcOneValue.Add("Drive can write CSS managed DVDs");
            if(ftr.CanWriteDDCDR) mmcOneValue.Add("Drive supports writing DDCD-R");
            if(ftr.CanWriteDDCDRW) mmcOneValue.Add("Drive supports writing DDCD-RW");
            if(ftr.CanWriteDVDPlusRWDL) mmcOneValue.Add("Drive can read and write DVD+RW DL");
            else if(ftr.CanReadDVDPlusRWDL) mmcOneValue.Add("Drive can read DVD+RW DL");
            if(ftr.CanWriteDVDR && ftr.CanWriteDVDRW && ftr.CanWriteDVDRDL)
                mmcOneValue.Add("Drive supports writing DVD-R, DVD-RW and DVD-R DL");
            else if(ftr.CanWriteDVDR && ftr.CanWriteDVDRDL)
                mmcOneValue.Add("Drive supports writing DVD-R and DVD-R DL");
            else if(ftr.CanWriteDVDR && ftr.CanWriteDVDRW) mmcOneValue.Add("Drive supports writing DVD-R and DVD-RW");
            else if(ftr.CanWriteDVDR) mmcOneValue.Add("Drive supports writing DVD-R");
            if(ftr.CanWriteHDDVDR && ftr.CanWriteHDDVDRAM)
                mmcOneValue.Add("Drive can write HD DVD-RW, HD DVD-R and HD DVD-RAM");
            else if(ftr.CanWriteHDDVDR) mmcOneValue.Add("Drive can write HD DVD-RW and HD DVD-R");
            else if(ftr.CanWriteHDDVDRAM) mmcOneValue.Add("Drive can write HD DVD-RW and HD DVD-RAM");
            // TODO: Write HD DVD-RW
            /*
            else
                mmcOneValue.Add("Drive can write HD DVD-RW");
            */
            if(ftr.CanWriteOldBDR) mmcOneValue.Add("Drive can write BD-R pre-1.0");
            if(ftr.CanWriteOldBDRE) mmcOneValue.Add("Drive can write BD-RE pre-1.0");
            if(ftr.CanWriteRWSubchannelInTAO)
            {
                mmcOneValue.Add("Drive can write user provided data in the R-W subchannels in Track at Once Mode");
                if(ftr.CanWriteRawSubchannelInTAO)
                    mmcOneValue.Add("Drive accepts RAW R-W subchannel data in Track at Once Mode");
                if(ftr.CanWritePackedSubchannelInTAO)
                    mmcOneValue.Add("Drive accepts Packed R-W subchannel data in Track at Once Mode");
            }
            if(ftr.CanWriteRWSubchannelInSAO)
                mmcOneValue.Add("Drive can write user provided data in the R-W subchannels in Session at Once Mode");
            if(ftr.CanWriteRaw && ftr.CanWriteRawMultiSession)
                mmcOneValue.Add("Drive can write multi-session CDs in raw mode");
            if(ftr.EmbeddedChanger)
            {
                mmcOneValue.Add("Drive contains an embedded changer");

                if(ftr.ChangerIsSideChangeCapable) mmcOneValue.Add("Drive can change disc side");
                if(ftr.ChangerSupportsDiscPresent)
                    mmcOneValue.Add("Drive is able to report slots contents after a reset or change");

                mmcOneValue.Add($"Drive has {ftr.ChangerSlots + 1} slots");
            }
            if(ftr.SupportsCSS && ftr.CSSVersionSpecified)
                mmcOneValue.Add($"Drive supports DVD CSS/CPPM version {ftr.CSSVersion}");
            else if(ftr.SupportsCSS) mmcOneValue.Add("Drive supports DVD CSS/CPRM");
            if(ftr.SupportsCPRM && ftr.CPRMVersionSpecified)
                mmcOneValue.Add($"Drive supports DVD CPPM version {ftr.CPRMVersion}");
            else if(ftr.SupportsCPRM) mmcOneValue.Add("Drive supports DVD CPRM");
            if(ftr.DBML) mmcOneValue.Add("Drive reports Device Busy Class events during medium loading/unloading");
            if(ftr.DVDMultiRead) mmcOneValue.Add("Drive conforms to DVD Multi Drive Read-only Specifications");
            if(ftr.FirmwareDateSpecified)
                mmcOneValue.Add($"Drive firmware is dated {ftr.FirmwareDate}");
            if(ftr.SupportsC2) mmcOneValue.Add("Drive supports C2 Error Pointers");
            if(ftr.SupportsDAP) mmcOneValue.Add("Drive supports the DAP bit in the READ CD and READ CD MSF commands");
            if(ftr.SupportsDeviceBusyEvent) mmcOneValue.Add("Drive supports Device Busy events");

            switch(ftr.LoadingMechanismType)
            {
                case 0:
                    mmcOneValue.Add("Drive uses media caddy");
                    break;
                case 1:
                    mmcOneValue.Add("Drive uses a tray");
                    break;
                case 2:
                    mmcOneValue.Add("Drive is pop-up");
                    break;
                case 4:
                    mmcOneValue.Add("Drive is a changer with individually changeable discs");
                    break;
                case 5:
                    mmcOneValue.Add("Drive is a changer using cartridges");
                    break;
                default:
                    mmcOneValue.Add($"Drive uses unknown loading mechanism type {ftr.LoadingMechanismType}");
                    break;
            }

            if(ftr.SupportsHybridDiscs) mmcOneValue.Add("Drive is able to access Hybrid discs");
            if(ftr.SupportsModePage1Ch)
                mmcOneValue.Add("Drive supports the Informational Exceptions Control mode page 1Ch");
            if(ftr.SupportsOSSC)
                mmcOneValue.Add("Drive supports the Trusted Computing Group Optical Security Subsystem Class");
            if(ftr.SupportsPWP) mmcOneValue.Add("Drive supports set/release of PWP status");
            if(ftr.SupportsSWPP) mmcOneValue.Add("Drive supports the SWPP bit of the Timeout and Protect mode page");
            if(ftr.SupportsSecurDisc) mmcOneValue.Add("Drive supports SecurDisc");
            if(ftr.SupportsSeparateVolume) mmcOneValue.Add("Drive supports separate volume per channel");
            if(ftr.SupportsVCPS) mmcOneValue.Add("Drive supports VCPS");
            if(ftr.VolumeLevelsSpecified)
                mmcOneValue.Add($"Drive has {ftr.VolumeLevels + 1} volume levels");
            if(ftr.SupportsWriteProtectPAC)
                mmcOneValue.Add("Drive supports reading/writing the Disc Write Protect PAC on BD-R/-RE media");
            if(ftr.SupportsWriteInhibitDCB)
                mmcOneValue.Add("Drive supports writing the Write Inhibit DCB on DVD+RW media");

            mmcOneValue.Sort();
            mmcOneValue.Add("");
        }
    }
}