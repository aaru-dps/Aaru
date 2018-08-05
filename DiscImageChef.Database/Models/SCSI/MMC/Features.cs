// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Features.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for MMC features.
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
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Decoders.SCSI.MMC;

namespace DiscImageChef.Database.Models.SCSI.MMC
{
    public class Features : BaseEntity
    {
        public byte?               AACSVersion                     { get; set; }
        public byte?               AGIDs                           { get; set; }
        public byte?               BindingNonceBlocks              { get; set; }
        public ushort?             BlocksPerReadableUnit           { get; set; }
        public bool                BufferUnderrunFreeInDVD         { get; set; }
        public bool                BufferUnderrunFreeInSAO         { get; set; }
        public bool                BufferUnderrunFreeInTAO         { get; set; }
        public bool                CanAudioScan                    { get; set; }
        public bool                CanEject                        { get; set; }
        public bool                CanEraseSector                  { get; set; }
        public bool                CanExpandBDRESpareArea          { get; set; }
        public bool                CanFormat                       { get; set; }
        public bool                CanFormatBDREWithoutSpare       { get; set; }
        public bool                CanFormatCert                   { get; set; }
        public bool                CanFormatFRF                    { get; set; }
        public bool                CanFormatQCert                  { get; set; }
        public bool                CanFormatRRM                    { get; set; }
        public bool                CanGenerateBindingNonce         { get; set; }
        public bool                CanLoad                         { get; set; }
        public bool                CanMuteSeparateChannels         { get; set; }
        public bool                CanOverwriteSAOTrack            { get; set; }
        public bool                CanOverwriteTAOTrack            { get; set; }
        public bool                CanPlayCDAudio                  { get; set; }
        public bool                CanPseudoOverwriteBDR           { get; set; }
        public bool                CanReadAllDualR                 { get; set; }
        public bool                CanReadAllDualRW                { get; set; }
        public bool                CanReadBD                       { get; set; }
        public bool                CanReadBDR                      { get; set; }
        public bool                CanReadBDRE1                    { get; set; }
        public bool                CanReadBDRE2                    { get; set; }
        public bool                CanReadBDROM                    { get; set; }
        public bool                CanReadBluBCA                   { get; set; }
        public bool                CanReadCD                       { get; set; }
        public bool                CanReadCDMRW                    { get; set; }
        public bool                CanReadCPRM_MKB                 { get; set; }
        public bool                CanReadDDCD                     { get; set; }
        public bool                CanReadDVD                      { get; set; }
        public bool                CanReadDVDPlusMRW               { get; set; }
        public bool                CanReadDVDPlusR                 { get; set; }
        public bool                CanReadDVDPlusRDL               { get; set; }
        public bool                CanReadDVDPlusRW                { get; set; }
        public bool                CanReadDVDPlusRWDL              { get; set; }
        public bool                CanReadDriveAACSCertificate     { get; set; }
        public bool                CanReadHDDVD                    { get; set; }
        public bool                CanReadHDDVDR                   { get; set; }
        public bool                CanReadHDDVDRAM                 { get; set; }
        public bool                CanReadLeadInCDText             { get; set; }
        public bool                CanReadOldBDR                   { get; set; }
        public bool                CanReadOldBDRE                  { get; set; }
        public bool                CanReadOldBDROM                 { get; set; }
        public bool                CanReadSpareAreaInformation     { get; set; }
        public bool                CanReportDriveSerial            { get; set; }
        public bool                CanReportMediaSerial            { get; set; }
        public bool                CanTestWriteDDCDR               { get; set; }
        public bool                CanTestWriteDVD                 { get; set; }
        public bool                CanTestWriteInSAO               { get; set; }
        public bool                CanTestWriteInTAO               { get; set; }
        public bool                CanUpgradeFirmware              { get; set; }
        public bool                CanWriteBD                      { get; set; }
        public bool                CanWriteBDR                     { get; set; }
        public bool                CanWriteBDRE1                   { get; set; }
        public bool                CanWriteBDRE2                   { get; set; }
        public bool                CanWriteBusEncryptedBlocks      { get; set; }
        public bool                CanWriteCDMRW                   { get; set; }
        public bool                CanWriteCDRW                    { get; set; }
        public bool                CanWriteCDRWCAV                 { get; set; }
        public bool                CanWriteCDSAO                   { get; set; }
        public bool                CanWriteCDTAO                   { get; set; }
        public bool                CanWriteCSSManagedDVD           { get; set; }
        public bool                CanWriteDDCDR                   { get; set; }
        public bool                CanWriteDDCDRW                  { get; set; }
        public bool                CanWriteDVDPlusMRW              { get; set; }
        public bool                CanWriteDVDPlusR                { get; set; }
        public bool                CanWriteDVDPlusRDL              { get; set; }
        public bool                CanWriteDVDPlusRW               { get; set; }
        public bool                CanWriteDVDPlusRWDL             { get; set; }
        public bool                CanWriteDVDR                    { get; set; }
        public bool                CanWriteDVDRDL                  { get; set; }
        public bool                CanWriteDVDRW                   { get; set; }
        public bool                CanWriteHDDVDR                  { get; set; }
        public bool                CanWriteHDDVDRAM                { get; set; }
        public bool                CanWriteOldBDR                  { get; set; }
        public bool                CanWriteOldBDRE                 { get; set; }
        public bool                CanWritePackedSubchannelInTAO   { get; set; }
        public bool                CanWriteRWSubchannelInSAO       { get; set; }
        public bool                CanWriteRWSubchannelInTAO       { get; set; }
        public bool                CanWriteRaw                     { get; set; }
        public bool                CanWriteRawMultiSession         { get; set; }
        public bool                CanWriteRawSubchannelInTAO      { get; set; }
        public bool                ChangerIsSideChangeCapable      { get; set; }
        public byte                ChangerSlots                    { get; set; }
        public bool?               ChangerSupportsDiscPresent      { get; set; }
        public byte?               CPRMVersion                     { get; set; }
        public byte?               CSSVersion                      { get; set; }
        public bool                DBML                            { get; set; }
        public bool                DVDMultiRead                    { get; set; }
        public bool                EmbeddedChanger                 { get; set; }
        public bool                ErrorRecoveryPage               { get; set; }
        public DateTime?           FirmwareDate                    { get; set; }
        public byte?               LoadingMechanismType            { get; set; }
        public bool                Locked                          { get; set; }
        public uint?               LogicalBlockSize                { get; set; }
        public bool                MultiRead                       { get; set; }
        public PhysicalInterfaces? PhysicalInterfaceStandard       { get; set; }
        public uint?               PhysicalInterfaceStandardNumber { get; set; }
        public bool                PreventJumper                   { get; set; }
        public bool                SupportsAACS                    { get; set; }
        public bool                SupportsBusEncryption           { get; set; }
        public bool                SupportsC2                      { get; set; }
        public bool                SupportsCPRM                    { get; set; }
        public bool                SupportsCSS                     { get; set; }
        public bool                SupportsDAP                     { get; set; }
        public bool                SupportsDeviceBusyEvent         { get; set; }
        public bool                SupportsHybridDiscs             { get; set; }
        public bool                SupportsModePage1Ch             { get; set; }
        public bool                SupportsOSSC                    { get; set; }
        public bool                SupportsPWP                     { get; set; }
        public bool                SupportsSWPP                    { get; set; }
        public bool                SupportsSecurDisc               { get; set; }
        public bool                SupportsSeparateVolume          { get; set; }
        public bool                SupportsVCPS                    { get; set; }
        public bool                SupportsWriteInhibitDCB         { get; set; }
        public bool                SupportsWriteProtectPAC         { get; set; }
        public ushort?             VolumeLevels                    { get; set; }

        public static Features MapFeatures(mmcFeaturesType oldFeatures)
        {
            if(oldFeatures == null) return null;

            Features newFeatures = new Features
            {
                BufferUnderrunFreeInDVD       = oldFeatures.BufferUnderrunFreeInDVD,
                BufferUnderrunFreeInSAO       = oldFeatures.BufferUnderrunFreeInSAO,
                BufferUnderrunFreeInTAO       = oldFeatures.BufferUnderrunFreeInTAO,
                CanAudioScan                  = oldFeatures.CanAudioScan,
                CanEject                      = oldFeatures.CanEject,
                CanEraseSector                = oldFeatures.CanEraseSector,
                CanExpandBDRESpareArea        = oldFeatures.CanExpandBDRESpareArea,
                CanFormat                     = oldFeatures.CanFormat,
                CanFormatBDREWithoutSpare     = oldFeatures.CanFormatBDREWithoutSpare,
                CanFormatCert                 = oldFeatures.CanFormatCert,
                CanFormatFRF                  = oldFeatures.CanFormatFRF,
                CanFormatQCert                = oldFeatures.CanFormatQCert,
                CanFormatRRM                  = oldFeatures.CanFormatRRM,
                CanGenerateBindingNonce       = oldFeatures.CanGenerateBindingNonce,
                CanLoad                       = oldFeatures.CanLoad,
                CanMuteSeparateChannels       = oldFeatures.CanMuteSeparateChannels,
                CanOverwriteSAOTrack          = oldFeatures.CanOverwriteSAOTrack,
                CanOverwriteTAOTrack          = oldFeatures.CanOverwriteTAOTrack,
                CanPlayCDAudio                = oldFeatures.CanPlayCDAudio,
                CanPseudoOverwriteBDR         = oldFeatures.CanPseudoOverwriteBDR,
                CanReadAllDualR               = oldFeatures.CanReadAllDualR,
                CanReadAllDualRW              = oldFeatures.CanReadAllDualRW,
                CanReadBD                     = oldFeatures.CanReadBD,
                CanReadBDR                    = oldFeatures.CanReadBDR,
                CanReadBDRE1                  = oldFeatures.CanReadBDRE1,
                CanReadBDRE2                  = oldFeatures.CanReadBDRE2,
                CanReadBDROM                  = oldFeatures.CanReadBDROM,
                CanReadBluBCA                 = oldFeatures.CanReadBluBCA,
                CanReadCD                     = oldFeatures.CanReadCD,
                CanReadCDMRW                  = oldFeatures.CanReadCDMRW,
                CanReadCPRM_MKB               = oldFeatures.CanReadCPRM_MKB,
                CanReadDDCD                   = oldFeatures.CanReadDDCD,
                CanReadDVD                    = oldFeatures.CanReadDVD,
                CanReadDVDPlusMRW             = oldFeatures.CanReadDVDPlusMRW,
                CanReadDVDPlusR               = oldFeatures.CanReadDVDPlusR,
                CanReadDVDPlusRDL             = oldFeatures.CanReadDVDPlusRDL,
                CanReadDVDPlusRW              = oldFeatures.CanReadDVDPlusRW,
                CanReadDVDPlusRWDL            = oldFeatures.CanReadDVDPlusRWDL,
                CanReadDriveAACSCertificate   = oldFeatures.CanReadDriveAACSCertificate,
                CanReadHDDVD                  = oldFeatures.CanReadHDDVD,
                CanReadHDDVDR                 = oldFeatures.CanReadHDDVDR,
                CanReadHDDVDRAM               = oldFeatures.CanReadHDDVDRAM,
                CanReadLeadInCDText           = oldFeatures.CanReadLeadInCDText,
                CanReadOldBDR                 = oldFeatures.CanReadOldBDR,
                CanReadOldBDRE                = oldFeatures.CanReadOldBDRE,
                CanReadOldBDROM               = oldFeatures.CanReadOldBDROM,
                CanReadSpareAreaInformation   = oldFeatures.CanReadSpareAreaInformation,
                CanReportDriveSerial          = oldFeatures.CanReportDriveSerial,
                CanReportMediaSerial          = oldFeatures.CanReportMediaSerial,
                CanTestWriteDDCDR             = oldFeatures.CanTestWriteDDCDR,
                CanTestWriteDVD               = oldFeatures.CanTestWriteDVD,
                CanTestWriteInSAO             = oldFeatures.CanTestWriteInSAO,
                CanTestWriteInTAO             = oldFeatures.CanTestWriteInTAO,
                CanUpgradeFirmware            = oldFeatures.CanUpgradeFirmware,
                CanWriteBD                    = oldFeatures.CanWriteBD,
                CanWriteBDR                   = oldFeatures.CanWriteBDR,
                CanWriteBDRE1                 = oldFeatures.CanWriteBDRE1,
                CanWriteBDRE2                 = oldFeatures.CanWriteBDRE2,
                CanWriteBusEncryptedBlocks    = oldFeatures.CanWriteBusEncryptedBlocks,
                CanWriteCDMRW                 = oldFeatures.CanWriteCDMRW,
                CanWriteCDRW                  = oldFeatures.CanWriteCDRW,
                CanWriteCDRWCAV               = oldFeatures.CanWriteCDRWCAV,
                CanWriteCDSAO                 = oldFeatures.CanWriteCDSAO,
                CanWriteCDTAO                 = oldFeatures.CanWriteCDTAO,
                CanWriteCSSManagedDVD         = oldFeatures.CanWriteCSSManagedDVD,
                CanWriteDDCDR                 = oldFeatures.CanWriteDDCDR,
                CanWriteDDCDRW                = oldFeatures.CanWriteDDCDRW,
                CanWriteDVDPlusMRW            = oldFeatures.CanWriteDVDPlusMRW,
                CanWriteDVDPlusR              = oldFeatures.CanWriteDVDPlusR,
                CanWriteDVDPlusRDL            = oldFeatures.CanWriteDVDPlusRDL,
                CanWriteDVDPlusRW             = oldFeatures.CanWriteDVDPlusRW,
                CanWriteDVDPlusRWDL           = oldFeatures.CanWriteDVDPlusRWDL,
                CanWriteDVDR                  = oldFeatures.CanWriteDVDR,
                CanWriteDVDRDL                = oldFeatures.CanWriteDVDRDL,
                CanWriteDVDRW                 = oldFeatures.CanWriteDVDRW,
                CanWriteHDDVDR                = oldFeatures.CanWriteHDDVDR,
                CanWriteHDDVDRAM              = oldFeatures.CanWriteHDDVDRAM,
                CanWriteOldBDR                = oldFeatures.CanWriteOldBDR,
                CanWriteOldBDRE               = oldFeatures.CanWriteOldBDRE,
                CanWritePackedSubchannelInTAO = oldFeatures.CanWritePackedSubchannelInTAO,
                CanWriteRWSubchannelInSAO     = oldFeatures.CanWriteRWSubchannelInSAO,
                CanWriteRWSubchannelInTAO     = oldFeatures.CanWriteRWSubchannelInTAO,
                CanWriteRaw                   = oldFeatures.CanWriteRaw,
                CanWriteRawMultiSession       = oldFeatures.CanWriteRawMultiSession,
                CanWriteRawSubchannelInTAO    = oldFeatures.CanWriteRawSubchannelInTAO,
                ChangerIsSideChangeCapable    = oldFeatures.ChangerIsSideChangeCapable,
                ChangerSlots                  = oldFeatures.ChangerSlots,
                ChangerSupportsDiscPresent    = oldFeatures.ChangerSupportsDiscPresent,
                DBML                          = oldFeatures.DBML,
                DVDMultiRead                  = oldFeatures.DVDMultiRead,
                EmbeddedChanger               = oldFeatures.EmbeddedChanger,
                ErrorRecoveryPage             = oldFeatures.ErrorRecoveryPage,
                Locked                        = oldFeatures.Locked,
                MultiRead                     = oldFeatures.MultiRead,
                PreventJumper                 = oldFeatures.PreventJumper,
                SupportsAACS                  = oldFeatures.SupportsAACS,
                SupportsBusEncryption         = oldFeatures.SupportsBusEncryption,
                SupportsC2                    = oldFeatures.SupportsC2,
                SupportsCPRM                  = oldFeatures.SupportsCPRM,
                SupportsCSS                   = oldFeatures.SupportsCSS,
                SupportsDAP                   = oldFeatures.SupportsDAP,
                SupportsDeviceBusyEvent       = oldFeatures.SupportsDeviceBusyEvent,
                SupportsHybridDiscs           = oldFeatures.SupportsHybridDiscs,
                SupportsModePage1Ch           = oldFeatures.SupportsModePage1Ch,
                SupportsOSSC                  = oldFeatures.SupportsOSSC,
                SupportsPWP                   = oldFeatures.SupportsPWP,
                SupportsSWPP                  = oldFeatures.SupportsSWPP,
                SupportsSecurDisc             = oldFeatures.SupportsSecurDisc,
                SupportsSeparateVolume        = oldFeatures.SupportsSeparateVolume,
                SupportsVCPS                  = oldFeatures.SupportsVCPS,
                SupportsWriteInhibitDCB       = oldFeatures.SupportsWriteInhibitDCB,
                SupportsWriteProtectPAC       = oldFeatures.SupportsWriteProtectPAC
            };

            if(oldFeatures.AACSVersionSpecified) newFeatures.AACSVersion               = oldFeatures.AACSVersion;
            if(oldFeatures.AGIDsSpecified) newFeatures.AGIDs                           = oldFeatures.AGIDs;
            if(oldFeatures.BindingNonceBlocksSpecified) newFeatures.BindingNonceBlocks = oldFeatures.BindingNonceBlocks;
            if(oldFeatures.BlocksPerReadableUnitSpecified)
                newFeatures.BlocksPerReadableUnit = oldFeatures.BlocksPerReadableUnit;
            if(oldFeatures.CPRMVersionSpecified) newFeatures.CPRMVersion   = oldFeatures.CPRMVersion;
            if(oldFeatures.CSSVersionSpecified) newFeatures.CSSVersion     = oldFeatures.CSSVersion;
            if(oldFeatures.FirmwareDateSpecified) newFeatures.FirmwareDate = oldFeatures.FirmwareDate;
            if(oldFeatures.LoadingMechanismTypeSpecified)
                newFeatures.LoadingMechanismType = oldFeatures.LoadingMechanismType;
            if(oldFeatures.LogicalBlockSizeSpecified) newFeatures.LogicalBlockSize = oldFeatures.LogicalBlockSize;
            if(oldFeatures.PhysicalInterfaceStandardSpecified)
                newFeatures.PhysicalInterfaceStandard = oldFeatures.PhysicalInterfaceStandard;
            if(oldFeatures.PhysicalInterfaceStandardNumberSpecified)
                newFeatures.PhysicalInterfaceStandardNumber = oldFeatures.PhysicalInterfaceStandardNumber;
            if(oldFeatures.VolumeLevelsSpecified) newFeatures.VolumeLevels = oldFeatures.VolumeLevels;

            return newFeatures;
        }
    }
}