// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DeviceReport.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XML metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains classes for an XML device report.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Xml.Serialization;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.SCSI;

namespace DiscImageChef.Metadata
{
    [Serializable]
    [XmlRoot("DicDeviceReport", Namespace = "", IsNullable = false)]
    public class DeviceReport
    {
        public usbType USB;
        public firewireType FireWire;
        public ataType ATA;
        public ataType ATAPI;
        public scsiType SCSI;
        public bool CompactFlash;
        public pcmciaType PCMCIA;

        [XmlIgnore]
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


        [XmlIgnore]
        public bool AdditionalPIDSpecified;
        [XmlIgnore]
        public bool APIOSupportedSpecified;
        [XmlIgnore]
        public bool ATAPIByteCountSpecified;
        [XmlIgnore]
        public bool BufferTypeSpecified;
        [XmlIgnore]
        public bool BufferSizeSpecified;
        [XmlIgnore]
        public bool CapabilitiesSpecified;
        [XmlIgnore]
        public bool Capabilities2Specified;
        [XmlIgnore]
        public bool Capabilities3Specified;
        [XmlIgnore]
        public bool CFAPowerModeSpecified;
        [XmlIgnore]
        public bool CommandSetSpecified;
        [XmlIgnore]
        public bool CommandSet2Specified;
        [XmlIgnore]
        public bool CommandSet3Specified;
        [XmlIgnore]
        public bool CommandSet4Specified;
        [XmlIgnore]
        public bool CommandSet5Specified;
        [XmlIgnore]
        public bool CurrentAAMSpecified;
        [XmlIgnore]
        public bool CurrentAPMSpecified;
        [XmlIgnore]
        public bool DataSetMgmtSpecified;
        [XmlIgnore]
        public bool DataSetMgmtSizeSpecified;
        [XmlIgnore]
        public bool DeviceFormFactorSpecified;
        [XmlIgnore]
        public bool DMAActiveSpecified;
        [XmlIgnore]
        public bool DMASupportedSpecified;
        [XmlIgnore]
        public bool DMATransferTimingModeSpecified;
        [XmlIgnore]
        public bool EnhancedSecurityEraseTimeSpecified;
        [XmlIgnore]
        public bool EnabledCommandSetSpecified;
        [XmlIgnore]
        public bool EnabledCommandSet2Specified;
        [XmlIgnore]
        public bool EnabledCommandSet3Specified;
        [XmlIgnore]
        public bool EnabledCommandSet4Specified;
        [XmlIgnore]
        public bool EnabledSATAFeaturesSpecified;
        [XmlIgnore]
        public bool ExtendedIdentifySpecified;
        [XmlIgnore]
        public bool ExtendedUserSectorsSpecified;
        [XmlIgnore]
        public bool FreeFallSensitivitySpecified;
        [XmlIgnore]
        public bool FirmwareRevisionSpecified;
        [XmlIgnore]
        public bool GeneralConfigurationSpecified;
        [XmlIgnore]
        public bool HardwareResetResultSpecified;
        [XmlIgnore]
        public bool InterseekDelaySpecified;
        [XmlIgnore]
        public bool MajorVersionSpecified;
        [XmlIgnore]
        public bool MasterPasswordRevisionCodeSpecified;
        [XmlIgnore]
        public bool MaxDownloadMicroMode3Specified;
        [XmlIgnore]
        public bool MaxQueueDepthSpecified;
        [XmlIgnore]
        public bool MDMAActiveSpecified;
        [XmlIgnore]
        public bool MDMASupportedSpecified;
        [XmlIgnore]
        public bool MinDownloadMicroMode3Specified;
        [XmlIgnore]
        public bool MinMDMACycleTimeSpecified;
        [XmlIgnore]
        public bool MinorVersionSpecified;
        [XmlIgnore]
        public bool MinPIOCycleTimeNoFlowSpecified;
        [XmlIgnore]
        public bool MinPIOCycleTimeFlowSpecified;
        [XmlIgnore]
        public bool ModelSpecified;
        [XmlIgnore]
        public bool MultipleMaxSectorsSpecified;
        [XmlIgnore]
        public bool MultipleSectorNumberSpecified;
        [XmlIgnore]
        public bool NVCacheCapsSpecified;
        [XmlIgnore]
        public bool NVCacheSizeSpecified;
        [XmlIgnore]
        public bool NVCacheWriteSpeedSpecified;
        [XmlIgnore]
        public bool NVEstimatedSpinUpSpecified;
        [XmlIgnore]
        public bool PacketBusReleaseSpecified;
        [XmlIgnore]
        public bool PIOTransferTimingModeSpecified;
        [XmlIgnore]
        public bool RecommendedAAMSpecified;
        [XmlIgnore]
        public bool RecommendedMDMACycleTimeSpecified;
        [XmlIgnore]
        public bool RemovableStatusSetSpecified;
        [XmlIgnore]
        public bool SATACapabilitiesSpecified;
        [XmlIgnore]
        public bool SATACapabilities2Specified;
        [XmlIgnore]
        public bool SATAFeaturesSpecified;
        [XmlIgnore]
        public bool SCTCommandTransportSpecified;
        [XmlIgnore]
        public bool SectorsPerCardSpecified;
        [XmlIgnore]
        public bool SecurityEraseTimeSpecified;
        [XmlIgnore]
        public bool SecurityStatusSpecified;
        [XmlIgnore]
        public bool ServiceBusyClearSpecified;
        [XmlIgnore]
        public bool SpecificConfigurationSpecified;
        [XmlIgnore]
        public bool StreamAccessLatencySpecified;
        [XmlIgnore]
        public bool StreamMinReqSizeSpecified;
        [XmlIgnore]
        public bool StreamPerformanceGranularitySpecified;
        [XmlIgnore]
        public bool StreamTransferTimeDMASpecified;
        [XmlIgnore]
        public bool StreamTransferTimePIOSpecified;
        [XmlIgnore]
        public bool TransportMajorVersionSpecified;
        [XmlIgnore]
        public bool TransportMinorVersionSpecified;
        [XmlIgnore]
        public bool TrustedComputingSpecified;
        [XmlIgnore]
        public bool UDMAActiveSpecified;
        [XmlIgnore]
        public bool UDMASupportedSpecified;
        [XmlIgnore]
        public bool WRVModeSpecified;
        [XmlIgnore]
        public bool WRVSectorCountMode3Specified;
        [XmlIgnore]
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

        [XmlIgnore]
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

        [XmlIgnore]
        public bool ANSIVersionSpecified;
        [XmlIgnore]
        public bool ECMAVersionSpecified;
        [XmlIgnore]
        public bool DeviceTypeModifierSpecified;
        [XmlIgnore]
        public bool ISOVersionSpecified;
        [XmlIgnore]
        public bool ProductIdentificationSpecified;
        [XmlIgnore]
        public bool ProductRevisionLevelSpecified;
        [XmlIgnore]
        public bool ResponseDataFormatSpecified;
        [XmlIgnore]
        public bool VendorIdentificationSpecified;
    }

    [Serializable]
    public class pageType
    {
        [XmlAttribute]
        public byte page;

        [XmlText]
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

        [XmlIgnore]
        public bool MediumTypeSpecified;
        [XmlIgnore]
        public bool SpeedSpecified;
        [XmlIgnore]
        public bool BufferedModeSpecified;
    }

    public class blockDescriptorType
    {
        public byte Density;
        public ulong Blocks;
        public uint BlockLength;

        [XmlIgnore]
        public bool BlocksSpecified;
        [XmlIgnore]
        public bool BlockLengthSpecified;
    }

    [Serializable]
    public class modePageType
    {
        [XmlAttribute]
        public byte page;

        [XmlAttribute]
        public byte subpage;

        [XmlText]
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

        [XmlIgnore]
        public bool MaximumSpeedSpecified;
        [XmlIgnore]
        public bool SupportedVolumeLevelsSpecified;
        [XmlIgnore]
        public bool BufferSizeSpecified;
        [XmlIgnore]
        public bool CurrentSpeedSpecified;
        [XmlIgnore]
        public bool MaximumWriteSpeedSpecified;
        [XmlIgnore]
        public bool CurrentWriteSpeedSpecified;
        [XmlIgnore]
        public bool RotationControlSelectedSpecified;
        [XmlIgnore]
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
        [XmlElement(DataType = "date")]
        public DateTime FirmwareDate;
        public byte LoadingMechanismType;
        public bool Locked;
        public uint LogicalBlockSize;
        public bool MultiRead;
        public Decoders.SCSI.MMC.PhysicalInterfaces PhysicalInterfaceStandard;
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

        [XmlIgnore]
        public bool PhysicalInterfaceStandardSpecified;
        [XmlIgnore]
        public bool AACSVersionSpecified;
        [XmlIgnore]
        public bool AGIDsSpecified;
        [XmlIgnore]
        public bool BindingNonceBlocksSpecified;
        [XmlIgnore]
        public bool CPRMVersionSpecified;
        [XmlIgnore]
        public bool CSSVersionSpecified;
        [XmlIgnore]
        public bool ChangerHighestSlotNumberSpecified;
        [XmlIgnore]
        public bool LoadingMechanismTypeSpecified;
        [XmlIgnore]
        public bool LogicalBlockSizeSpecified;
        [XmlIgnore]
        public bool BlocksPerReadableUnitSpecified;
        [XmlIgnore]
        public bool FirmwareDateSpecified;
        [XmlIgnore]
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

        [XmlIgnore]
        public bool BlocksSpecified;
        [XmlIgnore]
        public bool BlockSizeSpecified;
        [XmlIgnore]
        public bool CanReadAACSSpecified;
        [XmlIgnore]
        public bool CanReadADIPSpecified;
        [XmlIgnore]
        public bool CanReadATIPSpecified;
        [XmlIgnore]
        public bool CanReadBCASpecified;
        [XmlIgnore]
        public bool CanReadC2PointersSpecified;
        [XmlIgnore]
        public bool CanReadCMISpecified;
        [XmlIgnore]
        public bool CanReadCorrectedSubchannelSpecified;
        [XmlIgnore]
        public bool CanReadCorrectedSubchannelWithC2Specified;
        [XmlIgnore]
        public bool CanReadDCBSpecified;
        [XmlIgnore]
        public bool CanReadDDSSpecified;
        [XmlIgnore]
        public bool CanReadDMISpecified;
        [XmlIgnore]
        public bool CanReadDiscInformationSpecified;
        [XmlIgnore]
        public bool CanReadFullTOCSpecified;
        [XmlIgnore]
        public bool CanReadHDCMISpecified;
        [XmlIgnore]
        public bool CanReadLayerCapacitySpecified;
        [XmlIgnore]
        public bool CanReadLeadInSpecified;
        [XmlIgnore]
        public bool CanReadLeadOutSpecified;
        [XmlIgnore]
        public bool CanReadMediaIDSpecified;
        [XmlIgnore]
        public bool CanReadMediaSerialSpecified;
        [XmlIgnore]
        public bool CanReadPACSpecified;
        [XmlIgnore]
        public bool CanReadPFISpecified;
        [XmlIgnore]
        public bool CanReadPMASpecified;
        [XmlIgnore]
        public bool CanReadPQSubchannelSpecified;
        [XmlIgnore]
        public bool CanReadPQSubchannelWithC2Specified;
        [XmlIgnore]
        public bool CanReadPRISpecified;
        [XmlIgnore]
        public bool CanReadRWSubchannelSpecified;
        [XmlIgnore]
        public bool CanReadRWSubchannelWithC2Specified;
        [XmlIgnore]
        public bool CanReadRecordablePFISpecified;
        [XmlIgnore]
        public bool CanReadSpareAreaInformationSpecified;
        [XmlIgnore]
        public bool CanReadTOCSpecified;
        [XmlIgnore]
        public bool DensitySpecified;
        [XmlIgnore]
        public bool LongBlockSizeSpecified;
        [XmlIgnore]
        public bool ManufacturerSpecified;
        [XmlIgnore]
        public bool MediumTypeSpecified;
        [XmlIgnore]
        public bool ModelSpecified;
        [XmlIgnore]
        public bool SupportsHLDTSTReadRawDVDSpecified;
        [XmlIgnore]
        public bool SupportsNECReadCDDASpecified;
        [XmlIgnore]
        public bool SupportsPioneerReadCDDASpecified;
        [XmlIgnore]
        public bool SupportsPioneerReadCDDAMSFSpecified;
        [XmlIgnore]
        public bool SupportsPlextorReadCDDASpecified;
        [XmlIgnore]
        public bool SupportsPlextorReadRawDVDSpecified;
        [XmlIgnore]
        public bool SupportsRead10Specified;
        [XmlIgnore]
        public bool SupportsRead12Specified;
        [XmlIgnore]
        public bool SupportsRead16Specified;
        [XmlIgnore]
        public bool SupportsReadSpecified;
        [XmlIgnore]
        public bool SupportsReadCapacity16Specified;
        [XmlIgnore]
        public bool SupportsReadCapacitySpecified;
        [XmlIgnore]
        public bool SupportsReadCdSpecified;
        [XmlIgnore]
        public bool SupportsReadCdMsfSpecified;
        [XmlIgnore]
        public bool SupportsReadCdRawSpecified;
        [XmlIgnore]
        public bool SupportsReadCdMsfRawSpecified;
        [XmlIgnore]
        public bool SupportsReadLong16Specified;
        [XmlIgnore]
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

        [XmlIgnore]
        public bool LBASectorsSpecified;
        [XmlIgnore]
        public bool LBA48SectorsSpecified;
        [XmlIgnore]
        public bool LogicalAlignmentSpecified;
        [XmlIgnore]
        public bool NominalRotationRateSpecified;
        [XmlIgnore]
        public bool PhysicalBlockSizeSpecified;
        [XmlIgnore]
        public bool SolidStateDeviceSpecified;
        [XmlIgnore]
        public bool UnformattedBPTSpecified;
        [XmlIgnore]
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

        [XmlIgnore]
        public bool SupportsReadDmaLbaSpecified;
        [XmlIgnore]
        public bool SupportsReadDmaRetryLbaSpecified;
        [XmlIgnore]
        public bool SupportsReadLbaSpecified;
        [XmlIgnore]
        public bool SupportsReadRetryLbaSpecified;
        [XmlIgnore]
        public bool SupportsReadLongLbaSpecified;
        [XmlIgnore]
        public bool SupportsReadLongRetryLbaSpecified;
        [XmlIgnore]
        public bool SupportsSeekLbaSpecified;

        [XmlIgnore]
        public bool SupportsReadDmaLba48Specified;
        [XmlIgnore]
        public bool SupportsReadLba48Specified;

        [XmlIgnore]
        public bool SupportsReadDmaSpecified;
        [XmlIgnore]
        public bool SupportsReadDmaRetrySpecified;
        [XmlIgnore]
        public bool SupportsReadRetrySpecified;
        [XmlIgnore]
        public bool SupportsReadLongRetrySpecified;
        [XmlIgnore]
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

        [XmlIgnore]
        public bool BlockSizeGranularitySpecified;
        [XmlIgnore]
        public bool MaxBlockLengthSpecified;
        [XmlIgnore]
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

        [XmlIgnore]
        public bool CanReadMediaSerialSpecified;
        [XmlIgnore]
        public bool DensitySpecified;
        [XmlIgnore]
        public bool MediumTypeSpecified;
    }

    public class pcmciaType
    {
        public byte[] CIS;
        public string Compliance;
        public ushort ManufacturerCode;
        public ushort CardCode;
        public string Manufacturer;
        public string ProductName;
        public string[] AdditionalInformation;

        [XmlIgnore]
        public bool ManufacturerCodeSpecified;
        [XmlIgnore]
        public bool CardCodeSpecified;
    }
}

