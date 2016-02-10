// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DeviceReport.cs
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
using System.Xml.Serialization;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.SCSI;

namespace DiscImageChef.Metadata
{
    [SerializableAttribute()]
    [XmlRootAttribute("DicDeviceReport", Namespace="", IsNullable=false)]
    public class DeviceReport
    {
        public usbType USB;
        public firewireType FireWire;
        public ataType ATA;
        public ataType ATAPI;
        public scsiType SCSI;
        public bool CompactFlash;

        [XmlIgnoreAttribute()]
        public bool CompactFlashSpecified;
    }

    public class usbType
    {
        public ushort VendorID;
        public ushort ProductID;
        public string Manufacturer;
        public string Product;
        public bool RemovableMedia;
    }

    public class firewireType
    {
        public uint VendorID;
        public uint ProductID;
        public string Manufacturer;
        public string Product;
        public bool RemovableMedia;
    }

    public class ataType
    {

        public string AdditionalPID;
        public Identify.TransferMode APIOSupported;
        public ushort ATAPIByteCount;
        public ushort BufferType;
        public ushort BufferSize;
        public Identify.CapabilitiesBit Capabilities;
        public Identify.CapabilitiesBit2 Capabilities2;
        public Identify.CapabilitiesBit3 Capabilities3;
        public ushort CFAPowerMode;
        public Identify.CommandSetBit CommandSet;
        public Identify.CommandSetBit2 CommandSet2;
        public Identify.CommandSetBit3 CommandSet3;
        public Identify.CommandSetBit4 CommandSet4;
        public Identify.CommandSetBit5 CommandSet5;
        public byte CurrentAAM;
        public ushort CurrentAPM;
        public Identify.DataSetMgmtBit DataSetMgmt;
        public ushort DataSetMgmtSize;
        public Identify.DeviceFormFactorEnum DeviceFormFactor;
        public Identify.TransferMode DMAActive;
        public Identify.TransferMode DMASupported;
        public byte DMATransferTimingMode;
        public ushort EnhancedSecurityEraseTime;
        public Identify.CommandSetBit EnabledCommandSet;
        public Identify.CommandSetBit2 EnabledCommandSet2;
        public Identify.CommandSetBit3 EnabledCommandSet3;
        public Identify.CommandSetBit4 EnabledCommandSet4;
        public Identify.SATAFeaturesBit EnabledSATAFeatures;
        public ulong ExtendedUserSectors;
        public byte FreeFallSensitivity;
        public string FirmwareRevision;
        public Identify.GeneralConfigurationBit GeneralConfiguration;
        public ushort HardwareResetResult;
        public ushort InterseekDelay;
        public Identify.MajorVersionBit MajorVersion;
        public ushort MasterPasswordRevisionCode;
        public ushort MaxDownloadMicroMode3;
        public ushort MaxQueueDepth;
        public Identify.TransferMode MDMAActive;
        public Identify.TransferMode MDMASupported;
        public ushort MinDownloadMicroMode3;
        public ushort MinMDMACycleTime;
        public ushort MinorVersion;
        public ushort MinPIOCycleTimeNoFlow;
        public ushort MinPIOCycleTimeFlow;
        public string Model;
        public byte MultipleMaxSectors;
        public byte MultipleSectorNumber;
        public ushort NVCacheCaps;
        public uint NVCacheSize;
        public ushort NVCacheWriteSpeed;
        public byte NVEstimatedSpinUp;
        public ushort PacketBusRelease;
        public byte PIOTransferTimingMode;
        public byte RecommendedAAM;
        public ushort RecommendedMDMACycleTime;
        public ushort RemovableStatusSet;
        public Identify.SATACapabilitiesBit SATACapabilities;
        public Identify.SATACapabilitiesBit2 SATACapabilities2;
        public Identify.SATAFeaturesBit SATAFeatures;
        public Identify.SCTCommandTransportBit SCTCommandTransport;
        public uint SectorsPerCard;
        public ushort SecurityEraseTime;
        public Identify.SecurityStatusBit SecurityStatus;
        public ushort ServiceBusyClear;
        public Identify.SpecificConfigurationEnum SpecificConfiguration;
        public ushort StreamAccessLatency;
        public ushort StreamMinReqSize;
        public uint StreamPerformanceGranularity;
        public ushort StreamTransferTimeDMA;
        public ushort StreamTransferTimePIO;
        public ushort TransportMajorVersion;
        public ushort TransportMinorVersion;
        public Identify.TrustedComputingBit TrustedComputing;
        public Identify.TransferMode UDMAActive;
        public Identify.TransferMode UDMASupported;
        public byte WRVMode;
        public uint WRVSectorCountMode3;
        public uint WRVSectorCountMode2;

        public testedMediaType ReadCapabilities;
        public testedMediaType[] RemovableMedias;


        [XmlIgnoreAttribute()]
        public bool AdditionalPIDSpecified;
        [XmlIgnoreAttribute()]
        public bool APIOSupportedSpecified;
        [XmlIgnoreAttribute()]
        public bool ATAPIByteCountSpecified;
        [XmlIgnoreAttribute()]
        public bool BufferTypeSpecified;
        [XmlIgnoreAttribute()]
        public bool BufferSizeSpecified;
        [XmlIgnoreAttribute()]
        public bool CapabilitiesSpecified;
        [XmlIgnoreAttribute()]
        public bool Capabilities2Specified;
        [XmlIgnoreAttribute()]
        public bool Capabilities3Specified;
        [XmlIgnoreAttribute()]
        public bool CFAPowerModeSpecified;
        [XmlIgnoreAttribute()]
        public bool CommandSetSpecified;
        [XmlIgnoreAttribute()]
        public bool CommandSet2Specified;
        [XmlIgnoreAttribute()]
        public bool CommandSet3Specified;
        [XmlIgnoreAttribute()]
        public bool CommandSet4Specified;
        [XmlIgnoreAttribute()]
        public bool CommandSet5Specified;
        [XmlIgnoreAttribute()]
        public bool CurrentAAMSpecified;
        [XmlIgnoreAttribute()]
        public bool CurrentAPMSpecified;
        [XmlIgnoreAttribute()]
        public bool DataSetMgmtSpecified;
        [XmlIgnoreAttribute()]
        public bool DataSetMgmtSizeSpecified;
        [XmlIgnoreAttribute()]
        public bool DeviceFormFactorSpecified;
        [XmlIgnoreAttribute()]
        public bool DMAActiveSpecified;
        [XmlIgnoreAttribute()]
        public bool DMASupportedSpecified;
        [XmlIgnoreAttribute()]
        public bool DMATransferTimingModeSpecified;
        [XmlIgnoreAttribute()]
        public bool EnhancedSecurityEraseTimeSpecified;
        [XmlIgnoreAttribute()]
        public bool EnabledCommandSetSpecified;
        [XmlIgnoreAttribute()]
        public bool EnabledCommandSet2Specified;
        [XmlIgnoreAttribute()]
        public bool EnabledCommandSet3Specified;
        [XmlIgnoreAttribute()]
        public bool EnabledCommandSet4Specified;
        [XmlIgnoreAttribute()]
        public bool EnabledSATAFeaturesSpecified;
        [XmlIgnoreAttribute()]
        public bool ExtendedIdentifySpecified;
        [XmlIgnoreAttribute()]
        public bool ExtendedUserSectorsSpecified;
        [XmlIgnoreAttribute()]
        public bool FreeFallSensitivitySpecified;
        [XmlIgnoreAttribute()]
        public bool FirmwareRevisionSpecified;
        [XmlIgnoreAttribute()]
        public bool GeneralConfigurationSpecified;
        [XmlIgnoreAttribute()]
        public bool HardwareResetResultSpecified;
        [XmlIgnoreAttribute()]
        public bool InterseekDelaySpecified;
        [XmlIgnoreAttribute()]
        public bool MajorVersionSpecified;
        [XmlIgnoreAttribute()]
        public bool MasterPasswordRevisionCodeSpecified;
        [XmlIgnoreAttribute()]
        public bool MaxDownloadMicroMode3Specified;
        [XmlIgnoreAttribute()]
        public bool MaxQueueDepthSpecified;
        [XmlIgnoreAttribute()]
        public bool MDMAActiveSpecified;
        [XmlIgnoreAttribute()]
        public bool MDMASupportedSpecified;
        [XmlIgnoreAttribute()]
        public bool MinDownloadMicroMode3Specified;
        [XmlIgnoreAttribute()]
        public bool MinMDMACycleTimeSpecified;
        [XmlIgnoreAttribute()]
        public bool MinorVersionSpecified;
        [XmlIgnoreAttribute()]
        public bool MinPIOCycleTimeNoFlowSpecified;
        [XmlIgnoreAttribute()]
        public bool MinPIOCycleTimeFlowSpecified;
        [XmlIgnoreAttribute()]
        public bool ModelSpecified;
        [XmlIgnoreAttribute()]
        public bool MultipleMaxSectorsSpecified;
        [XmlIgnoreAttribute()]
        public bool MultipleSectorNumberSpecified;
        [XmlIgnoreAttribute()]
        public bool NVCacheCapsSpecified;
        [XmlIgnoreAttribute()]
        public bool NVCacheSizeSpecified;
        [XmlIgnoreAttribute()]
        public bool NVCacheWriteSpeedSpecified;
        [XmlIgnoreAttribute()]
        public bool NVEstimatedSpinUpSpecified;
        [XmlIgnoreAttribute()]
        public bool PacketBusReleaseSpecified;
        [XmlIgnoreAttribute()]
        public bool PIOTransferTimingModeSpecified;
        [XmlIgnoreAttribute()]
        public bool RecommendedAAMSpecified;
        [XmlIgnoreAttribute()]
        public bool RecommendedMDMACycleTimeSpecified;
        [XmlIgnoreAttribute()]
        public bool RemovableStatusSetSpecified;
        [XmlIgnoreAttribute()]
        public bool SATACapabilitiesSpecified;
        [XmlIgnoreAttribute()]
        public bool SATACapabilities2Specified;
        [XmlIgnoreAttribute()]
        public bool SATAFeaturesSpecified;
        [XmlIgnoreAttribute()]
        public bool SCTCommandTransportSpecified;
        [XmlIgnoreAttribute()]
        public bool SectorsPerCardSpecified;
        [XmlIgnoreAttribute()]
        public bool SecurityEraseTimeSpecified;
        [XmlIgnoreAttribute()]
        public bool SecurityStatusSpecified;
        [XmlIgnoreAttribute()]
        public bool ServiceBusyClearSpecified;
        [XmlIgnoreAttribute()]
        public bool SpecificConfigurationSpecified;
        [XmlIgnoreAttribute()]
        public bool StreamAccessLatencySpecified;
        [XmlIgnoreAttribute()]
        public bool StreamMinReqSizeSpecified;
        [XmlIgnoreAttribute()]
        public bool StreamPerformanceGranularitySpecified;
        [XmlIgnoreAttribute()]
        public bool StreamTransferTimeDMASpecified;
        [XmlIgnoreAttribute()]
        public bool StreamTransferTimePIOSpecified;
        [XmlIgnoreAttribute()]
        public bool TransportMajorVersionSpecified;
        [XmlIgnoreAttribute()]
        public bool TransportMinorVersionSpecified;
        [XmlIgnoreAttribute()]
        public bool TrustedComputingSpecified;
        [XmlIgnoreAttribute()]
        public bool UDMAActiveSpecified;
        [XmlIgnoreAttribute()]
        public bool UDMASupportedSpecified;
        [XmlIgnoreAttribute()]
        public bool WRVModeSpecified;
        [XmlIgnoreAttribute()]
        public bool WRVSectorCountMode3Specified;
        [XmlIgnoreAttribute()]
        public bool WRVSectorCountMode2Specified;
    }

    public class chsType
    {
        public ushort Cylinders;
        public ushort Heads;
        public ushort Sectors;
    }

    public class scsiType
    {
        public scsiInquiryType Inquiry;
        public pageType[] EVPDPages;
        public bool SupportsModeSense6;
        public bool SupportsModeSense10;
        public bool SupportsModeSubpages;
        public modeType ModeSense;
        public mmcType MultiMediaDevice;
        public testedMediaType ReadCapabilities;
        public testedMediaType[] RemovableMedias;
        public sscType SequentialDevice;

        [XmlIgnoreAttribute()]
        public bool ReadCapabilitiesSpecified;
    }

    public class scsiInquiryType
    {
        public bool AccessControlCoordinator;
        public bool ACKRequests;
        public bool AERCSupported;
        public bool Address16;
        public bool Address32;
        public byte ANSIVersion;
        public TGPSValues AsymmetricalLUNAccess;
        public bool BasicQueueing;
        public byte DeviceTypeModifier;
        public byte ECMAVersion;
        public bool EnclosureServices;
        public bool HierarchicalLUN;
        public bool IUS;
        public byte ISOVersion;
        public bool LinkedCommands;
        public bool MediumChanger;
        public bool MultiPortDevice;
        public bool NormalACA;
        public PeripheralDeviceTypes PeripheralDeviceType;
        public PeripheralQualifiers PeripheralQualifier;
        public string ProductIdentification;
        public string ProductRevisionLevel;
        public bool Protection;
        public bool QAS;
        public bool RelativeAddressing;
        public bool Removable;
        public byte ResponseDataFormat;
        public bool TaggedCommandQueue;
        public bool TerminateTaskSupported;
        public bool ThirdPartyCopy;
        public bool TranferDisable;
        public bool SoftReset;
        public SPIClocking SPIClocking;
        public bool StorageArrayController;
        public bool SyncTransfer;
        public string VendorIdentification;
        public ushort[] VersionDescriptors;
        public bool WideBus16;
        public bool WideBus32;

        [XmlIgnoreAttribute()]
        public bool ANSIVersionSpecified;
        [XmlIgnoreAttribute()]
        public bool ECMAVersionSpecified;
        [XmlIgnoreAttribute()]
        public bool DeviceTypeModifierSpecified;
        [XmlIgnoreAttribute()]
        public bool ISOVersionSpecified;
        [XmlIgnoreAttribute()]
        public bool ProductIdentificationSpecified;
        [XmlIgnoreAttribute()]
        public bool ProductRevisionLevelSpecified;
        [XmlIgnoreAttribute()]
        public bool ResponseDataFormatSpecified;
        [XmlIgnoreAttribute()]
        public bool VendorIdentificationSpecified;
    }

    [SerializableAttribute()]
    public class pageType
    {
        [XmlAttributeAttribute()]
        public byte page; 

        [XmlTextAttribute()]
        public byte[] value;
    }

    public class modeType
    {
        public byte MediumType;
        public bool WriteProtected;
        public blockDescriptorType[] BlockDescriptors;
        public byte Speed;
        public byte BufferedMode;
        public bool BlankCheckEnabled;
        public bool DPOandFUA;
        public modePageType[] ModePages;

        [XmlIgnoreAttribute()]
        public bool MediumTypeSpecified;
        [XmlIgnoreAttribute()]
        public bool SpeedSpecified;
        [XmlIgnoreAttribute()]
        public bool BufferedModeSpecified;
    }

    public class blockDescriptorType
    {
        public byte Density;
        public ulong Blocks;
        public uint BlockLength;

        [XmlIgnoreAttribute()]
        public bool BlocksSpecified;
        [XmlIgnoreAttribute()]
        public bool BlockLengthSpecified;
    }

    [SerializableAttribute()]
    public class modePageType
    {
        [XmlAttributeAttribute()]
        public byte page; 

        [XmlAttributeAttribute()]
        public byte subpage; 

        [XmlTextAttribute()]
        public byte[] value;
    }

    public class mmcType
    {
        public mmcModeType ModeSense2A;
        public mmcFeaturesType Features;
        public testedMediaType[] TestedMedia;
    }

    public class mmcModeType
    {
        public bool AccurateCDDA;
        public bool BCK;
        public ushort BufferSize;
        public bool BufferUnderRunProtection;
        public bool CanEject;
        public bool CanLockMedia;
        public bool CDDACommand;
        public bool CompositeAudioVideo;
        public bool CSSandCPPMSupported;
        public ushort CurrentSpeed;
        public ushort CurrentWriteSpeed;
        public ushort CurrentWriteSpeedSelected;
        public bool DeterministicSlotChanger;
        public bool DigitalPort1;
        public bool DigitalPort2;
        public bool LeadInPW;
        public byte LoadingMechanismType;
        public bool LockStatus;
        public bool LSBF;
        public ushort MaximumSpeed;
        public ushort MaximumWriteSpeed;
        public bool PlaysAudio;
        public bool PreventJumperStatus;
        public bool RCK;
        public bool ReadsBarcode;
        public bool ReadsBothSides;
        public bool ReadsCDR;
        public bool ReadsCDRW;
        public bool ReadsDeinterlavedSubchannel;
        public bool ReadsDVDR;
        public bool ReadsDVDRAM;
        public bool ReadsDVDROM;
        public bool ReadsISRC;
        public bool ReadsMode2Form2;
        public bool ReadsMode2Form1;
        public bool ReadsPacketCDR;
        public bool ReadsSubchannel;
        public bool ReadsUPC;
        public bool ReturnsC2Pointers;
        public byte RotationControlSelected;
        public bool SeparateChannelMute;
        public bool SeparateChannelVolume;
        public bool SSS;
        public bool SupportsMultiSession;
        public ushort SupportedVolumeLevels;
        public bool TestWrite;
        public bool WritesCDR;
        public bool WritesCDRW;
        public bool WritesDVDR;
        public bool WritesDVDRAM;
        public Modes.ModePage_2A_WriteDescriptor[] WriteSpeedPerformanceDescriptors;

        [XmlIgnoreAttribute()]
        public bool MaximumSpeedSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportedVolumeLevelsSpecified;
        [XmlIgnoreAttribute()]
        public bool BufferSizeSpecified;
        [XmlIgnoreAttribute()]
        public bool CurrentSpeedSpecified;
        [XmlIgnoreAttribute()]
        public bool MaximumWriteSpeedSpecified;
        [XmlIgnoreAttribute()]
        public bool CurrentWriteSpeedSpecified;
        [XmlIgnoreAttribute()]
        public bool RotationControlSelectedSpecified;
        [XmlIgnoreAttribute()]
        public bool CurrentWriteSpeedSelectedSpecified;
    }

    public class mmcFeaturesType
    {
        public byte AACSVersion;
        public byte AGIDs;
        public byte BindingNonceBlocks;
        public ushort BlocksPerReadableUnit;
        public bool BufferUnderrunFreeInDVD;
        public bool BufferUnderrunFreeInSAO;
        public bool BufferUnderrunFreeInTAO;
        public bool CanAudioScan;
        public bool CanEject;
        public bool CanEraseSector;
        public bool CanExpandBDRESpareArea;
        public bool CanFormat;
        public bool CanFormatBDREWithoutSpare;
        public bool CanFormatCert;
        public bool CanFormatFRF;
        public bool CanFormatQCert;
        public bool CanFormatRRM;
        public bool CanGenerateBindingNonce;
        public bool CanLoad;
        public bool CanMuteSeparateChannels;
        public bool CanOverwriteSAOTrack;
        public bool CanOverwriteTAOTrack;
        public bool CanPlayCDAudio;
        public bool CanPseudoOverwriteBDR;
        public bool CanReadAllDualR;
        public bool CanReadAllDualRW;
        public bool CanReadBD;
        public bool CanReadBDR;
        public bool CanReadBDRE1;
        public bool CanReadBDRE2;
        public bool CanReadBDROM;
        public bool CanReadBluBCA;
        public bool CanReadCD;
        public bool CanReadCDMRW;
        public bool CanReadCPRM_MKB;
        public bool CanReadDDCD;
        public bool CanReadDVD;
        public bool CanReadDVDPlusMRW;
        public bool CanReadDVDPlusR;
        public bool CanReadDVDPlusRDL;
        public bool CanReadDVDPlusRW;
        public bool CanReadDVDPlusRWDL;
        public bool CanReadDriveAACSCertificate;
        public bool CanReadHDDVD;
        public bool CanReadHDDVDR;
        public bool CanReadHDDVDRAM;
        public bool CanReadLeadInCDText;
        public bool CanReadOldBDR;
        public bool CanReadOldBDRE;
        public bool CanReadOldBDROM;
        public bool CanReadSpareAreaInformation;
        public bool CanReportDriveSerial;
        public bool CanReportMediaSerial;
        public bool CanTestWriteDDCDR;
        public bool CanTestWriteDVD;
        public bool CanTestWriteInSAO;
        public bool CanTestWriteInTAO;
        public bool CanUpgradeFirmware;
        public bool CanWriteBD;
        public bool CanWriteBDR;
        public bool CanWriteBDRE1;
        public bool CanWriteBDRE2;
        public bool CanWriteBusEncryptedBlocks;
        public bool CanWriteCDMRW;
        public bool CanWriteCDRW;
        public bool CanWriteCDRWCAV;
        public bool CanWriteCDSAO;
        public bool CanWriteCDTAO;
        public bool CanWriteCSSManagedDVD;
        public bool CanWriteDDCDR;
        public bool CanWriteDDCDRW;
        public bool CanWriteDVDPlusMRW;
        public bool CanWriteDVDPlusR;
        public bool CanWriteDVDPlusRDL;
        public bool CanWriteDVDPlusRW;
        public bool CanWriteDVDPlusRWDL;
        public bool CanWriteDVDR;
        public bool CanWriteDVDRDL;
        public bool CanWriteDVDRW;
        public bool CanWriteHDDVDR;
        public bool CanWriteHDDVDRAM;
        public bool CanWriteOldBDR;
        public bool CanWriteOldBDRE;
        public bool CanWritePackedSubchannelInTAO;
        public bool CanWriteRWSubchannelInSAO;
        public bool CanWriteRWSubchannelInTAO;
        public bool CanWriteRaw;
        public bool CanWriteRawMultiSession;
        public bool CanWriteRawSubchannelInTAO;
        public bool ChangerIsSideChangeCapable;
        public byte ChangerSlots;
        public bool ChangerSupportsDiscPresent;
        public byte CPRMVersion;
        public byte CSSVersion;
        public bool DBML;
        public bool DVDMultiRead;
        public bool EmbeddedChanger;
        public bool ErrorRecoveryPage;
        [XmlElementAttribute(DataType="date")]
        public DateTime FirmwareDate;
        public byte LoadingMechanismType;
        public bool Locked;
        public uint LogicalBlockSize;
        public bool MultiRead;
        public DiscImageChef.Decoders.SCSI.MMC.PhysicalInterfaces PhysicalInterfaceStandard;
        public bool PreventJumper;
        public bool SupportsAACS;
        public bool SupportsBusEncryption;
        public bool SupportsC2;
        public bool SupportsCPRM;
        public bool SupportsCSS;
        public bool SupportsDAP;
        public bool SupportsDeviceBusyEvent;
        public bool SupportsHybridDiscs;
        public bool SupportsModePage1Ch;
        public bool SupportsOSSC;
        public bool SupportsPWP;
        public bool SupportsSWPP;
        public bool SupportsSecurDisc;
        public bool SupportsSeparateVolume;
        public bool SupportsVCPS;
        public bool SupportsWriteInhibitDCB;
        public bool SupportsWriteProtectPAC;
        public ushort VolumeLevels;

        [XmlIgnoreAttribute()]
        public bool PhysicalInterfaceStandardSpecified;
        [XmlIgnoreAttribute()]
        public bool AACSVersionSpecified;
        [XmlIgnoreAttribute()]
        public bool AGIDsSpecified;
        [XmlIgnoreAttribute()]
        public bool BindingNonceBlocksSpecified;
        [XmlIgnoreAttribute()]
        public bool CPRMVersionSpecified;
        [XmlIgnoreAttribute()]
        public bool CSSVersionSpecified;
        [XmlIgnoreAttribute()]
        public bool ChangerHighestSlotNumberSpecified;
        [XmlIgnoreAttribute()]
        public bool LoadingMechanismTypeSpecified;
        [XmlIgnoreAttribute()]
        public bool LogicalBlockSizeSpecified;
        [XmlIgnoreAttribute()]
        public bool BlocksPerReadableUnitSpecified;
        [XmlIgnoreAttribute()]
        public bool FirmwareDateSpecified;
        [XmlIgnoreAttribute()]
        public bool VolumeLevelsSpecified;
    }

    public class testedMediaType
    {
        public ulong Blocks;
        public uint BlockSize;
        public bool CanReadAACS;
        public bool CanReadADIP;
        public bool CanReadATIP;
        public bool CanReadBCA;
        public bool CanReadC2Pointers;
        public bool CanReadCMI;
        public bool CanReadCorrectedSubchannel;
        public bool CanReadCorrectedSubchannelWithC2;
        public bool CanReadDCB;
        public bool CanReadDDS;
        public bool CanReadDMI;
        public bool CanReadDiscInformation;
        public bool CanReadFullTOC;
        public bool CanReadHDCMI;
        public bool CanReadLayerCapacity;
        public bool CanReadLeadIn;
        public bool CanReadLeadOut;
        public bool CanReadMediaID;
        public bool CanReadMediaSerial;
        public bool CanReadPAC;
        public bool CanReadPFI;
        public bool CanReadPMA;
        public bool CanReadPQSubchannel;
        public bool CanReadPQSubchannelWithC2;
        public bool CanReadPRI;
        public bool CanReadRWSubchannel;
        public bool CanReadRWSubchannelWithC2;
        public bool CanReadRecordablePFI;
        public bool CanReadSpareAreaInformation;
        public bool CanReadTOC;
        public byte Density;
        public uint LongBlockSize;
        public string Manufacturer;
        public bool MediaIsRecognized;
        public byte MediumType;
        public string MediumTypeName;
        public string Model;
        public bool SupportsHLDTSTReadRawDVD;
        public bool SupportsNECReadCDDA;
        public bool SupportsPioneerReadCDDA;
        public bool SupportsPioneerReadCDDAMSF;
        public bool SupportsPlextorReadCDDA;
        public bool SupportsPlextorReadRawDVD;
        public bool SupportsRead10;
        public bool SupportsRead12;
        public bool SupportsRead16;
        public bool SupportsRead;
        public bool SupportsReadCapacity16;
        public bool SupportsReadCapacity;
        public bool SupportsReadCd;
        public bool SupportsReadCdMsf;
        public bool SupportsReadCdRaw;
        public bool SupportsReadCdMsfRaw;
        public bool SupportsReadLong16;
        public bool SupportsReadLong;

        [XmlIgnoreAttribute()]
        public bool BlocksSpecified;
        [XmlIgnoreAttribute()]
        public bool BlockSizeSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadAACSSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadADIPSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadATIPSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadBCASpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadC2PointersSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadCMISpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadCorrectedSubchannelSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadCorrectedSubchannelWithC2Specified;
        [XmlIgnoreAttribute()]
        public bool CanReadDCBSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadDDSSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadDMISpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadDiscInformationSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadFullTOCSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadHDCMISpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadLayerCapacitySpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadLeadInSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadLeadOutSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadMediaIDSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadMediaSerialSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadPACSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadPFISpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadPMASpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadPQSubchannelSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadPQSubchannelWithC2Specified;
        [XmlIgnoreAttribute()]
        public bool CanReadPRISpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadRWSubchannelSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadRWSubchannelWithC2Specified;
        [XmlIgnoreAttribute()]
        public bool CanReadRecordablePFISpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadSpareAreaInformationSpecified;
        [XmlIgnoreAttribute()]
        public bool CanReadTOCSpecified;
        [XmlIgnoreAttribute()]
        public bool DensitySpecified;
        [XmlIgnoreAttribute()]
        public bool LongBlockSizeSpecified;
        [XmlIgnoreAttribute()]
        public bool ManufacturerSpecified;
        [XmlIgnoreAttribute()]
        public bool MediumTypeSpecified;
        [XmlIgnoreAttribute()]
        public bool ModelSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsHLDTSTReadRawDVDSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsNECReadCDDASpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsPioneerReadCDDASpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsPioneerReadCDDAMSFSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsPlextorReadCDDASpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsPlextorReadRawDVDSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsRead10Specified;
        [XmlIgnoreAttribute()]
        public bool SupportsRead12Specified;
        [XmlIgnoreAttribute()]
        public bool SupportsRead16Specified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadCapacity16Specified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadCapacitySpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadCdSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadCdMsfSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadCdRawSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadCdMsfRawSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadLong16Specified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadLongSpecified;

        public chsType CHS;
        public chsType CurrentCHS;
        public uint LBASectors;
        public ulong LBA48Sectors;
        public ushort LogicalAlignment;
        public ushort NominalRotationRate;
        public uint PhysicalBlockSize;
        public bool SolidStateDevice;
        public ushort UnformattedBPT;
        public ushort UnformattedBPS;

        [XmlIgnoreAttribute()]
        public bool LBASectorsSpecified;
        [XmlIgnoreAttribute()]
        public bool LBA48SectorsSpecified;
        [XmlIgnoreAttribute()]
        public bool LogicalAlignmentSpecified;
        [XmlIgnoreAttribute()]
        public bool NominalRotationRateSpecified;
        [XmlIgnoreAttribute()]
        public bool PhysicalBlockSizeSpecified;
        [XmlIgnoreAttribute()]
        public bool SolidStateDeviceSpecified;
        [XmlIgnoreAttribute()]
        public bool UnformattedBPTSpecified;
        [XmlIgnoreAttribute()]
        public bool UnformattedBPSSpecified;

        public bool SupportsReadDmaLba;
        public bool SupportsReadDmaRetryLba;
        public bool SupportsReadLba;
        public bool SupportsReadRetryLba;
        public bool SupportsReadLongLba;
        public bool SupportsReadLongRetryLba;
        public bool SupportsSeekLba;

        public bool SupportsReadDmaLba48;
        public bool SupportsReadLba48;

        public bool SupportsReadDma;
        public bool SupportsReadDmaRetry;
        public bool SupportsReadRetry;
        public bool SupportsReadLongRetry;
        public bool SupportsSeek;

        [XmlIgnoreAttribute()]
        public bool SupportsReadDmaLbaSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadDmaRetryLbaSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadLbaSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadRetryLbaSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadLongLbaSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadLongRetryLbaSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsSeekLbaSpecified;

        [XmlIgnoreAttribute()]
        public bool SupportsReadDmaLba48Specified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadLba48Specified;

        [XmlIgnoreAttribute()]
        public bool SupportsReadDmaSpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadDmaRetrySpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadRetrySpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsReadLongRetrySpecified;
        [XmlIgnoreAttribute()]
        public bool SupportsSeekSpecified;
    }

    public class sscType
    {
        public byte BlockSizeGranularity;
        public uint MaxBlockLength;
        public uint MinBlockLength;

        public SupportedDensity[] SupportedDensities;
        public SupportedMedia[] SupportedMediaTypes;
        public SequentialMedia[] TestedMedia;

        [XmlIgnoreAttribute()]
        public bool BlockSizeGranularitySpecified;
        [XmlIgnoreAttribute()]
        public bool MaxBlockLengthSpecified;
        [XmlIgnoreAttribute()]
        public bool MinBlockLengthSpecified;
    }

    public struct SupportedDensity
    {
        public byte PrimaryCode;
        public byte SecondaryCode;
        public bool Writable;
        public bool Duplicate;
        public bool DefaultDensity;
        public uint BitsPerMm;
        public ushort Width;
        public ushort Tracks;
        public uint Capacity;
        public string Organization;
        public string Name;
        public string Description;
    }

    public struct SupportedMedia
    {
        public byte MediumType;
        public int[] DensityCodes;
        public ushort Width;
        public ushort Length;
        public string Organization;
        public string Name;
        public string Description;
    }

    public struct SequentialMedia
    {
        public bool CanReadMediaSerial;
        public byte Density;
        public string Manufacturer;
        public bool MediaIsRecognized;
        public byte MediumType;
        public string MediumTypeName;
        public string Model;
        public SupportedDensity[] SupportedDensities;
        public SupportedMedia[] SupportedMediaTypes;

        [XmlIgnoreAttribute()]
        public bool CanReadMediaSerialSpecified;
        [XmlIgnoreAttribute()]
        public bool DensitySpecified;
        [XmlIgnoreAttribute()]
        public bool MediumTypeSpecified;
    }
}

