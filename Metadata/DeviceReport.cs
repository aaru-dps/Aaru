// /***************************************************************************
// Aaru Data Preservation Suite
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
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.CommonTypes.Structs.Devices.SCSI.Modes;
using Newtonsoft.Json;

// This is obsolete
#pragma warning disable 1591

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aaru.CommonTypes.Metadata
{
    [Serializable, XmlRoot("DicDeviceReport", Namespace = "", IsNullable = false)]
    public class DeviceReport
    {
        public usbType      USB            { get; set; }
        public firewireType FireWire       { get; set; }
        public pcmciaType   PCMCIA         { get; set; }
        public bool         CompactFlash   { get; set; }
        public ataType      ATA            { get; set; }
        public ataType      ATAPI          { get; set; }
        public scsiType     SCSI           { get; set; }
        public mmcsdType    MultiMediaCard { get; set; }
        public mmcsdType    SecureDigital  { get; set; }

        [XmlIgnore]
        public bool CompactFlashSpecified { get; set; }
    }

    public class usbType
    {
        public ushort VendorID       { get; set; }
        public ushort ProductID      { get; set; }
        public string Manufacturer   { get; set; }
        public string Product        { get; set; }
        public bool   RemovableMedia { get; set; }
        public byte[] Descriptors    { get; set; }
    }

    public class firewireType
    {
        public uint   VendorID       { get; set; }
        public uint   ProductID      { get; set; }
        public string Manufacturer   { get; set; }
        public string Product        { get; set; }
        public bool   RemovableMedia { get; set; }
    }

    public class ataType
    {
        public string                             AdditionalPID                { get; set; }
        public Identify.TransferMode              APIOSupported                { get; set; }
        public ushort                             ATAPIByteCount               { get; set; }
        public ushort                             BufferType                   { get; set; }
        public ushort                             BufferSize                   { get; set; }
        public Identify.CapabilitiesBit           Capabilities                 { get; set; }
        public Identify.CapabilitiesBit2          Capabilities2                { get; set; }
        public Identify.CapabilitiesBit3          Capabilities3                { get; set; }
        public ushort                             CFAPowerMode                 { get; set; }
        public Identify.CommandSetBit             CommandSet                   { get; set; }
        public Identify.CommandSetBit2            CommandSet2                  { get; set; }
        public Identify.CommandSetBit3            CommandSet3                  { get; set; }
        public Identify.CommandSetBit4            CommandSet4                  { get; set; }
        public Identify.CommandSetBit5            CommandSet5                  { get; set; }
        public byte                               CurrentAAM                   { get; set; }
        public ushort                             CurrentAPM                   { get; set; }
        public Identify.DataSetMgmtBit            DataSetMgmt                  { get; set; }
        public ushort                             DataSetMgmtSize              { get; set; }
        public Identify.DeviceFormFactorEnum      DeviceFormFactor             { get; set; }
        public Identify.TransferMode              DMAActive                    { get; set; }
        public Identify.TransferMode              DMASupported                 { get; set; }
        public byte                               DMATransferTimingMode        { get; set; }
        public ushort                             EnhancedSecurityEraseTime    { get; set; }
        public Identify.CommandSetBit             EnabledCommandSet            { get; set; }
        public Identify.CommandSetBit2            EnabledCommandSet2           { get; set; }
        public Identify.CommandSetBit3            EnabledCommandSet3           { get; set; }
        public Identify.CommandSetBit4            EnabledCommandSet4           { get; set; }
        public Identify.SATAFeaturesBit           EnabledSATAFeatures          { get; set; }
        public ulong                              ExtendedUserSectors          { get; set; }
        public byte                               FreeFallSensitivity          { get; set; }
        public string                             FirmwareRevision             { get; set; }
        public Identify.GeneralConfigurationBit   GeneralConfiguration         { get; set; }
        public ushort                             HardwareResetResult          { get; set; }
        public ushort                             InterseekDelay               { get; set; }
        public Identify.MajorVersionBit           MajorVersion                 { get; set; }
        public ushort                             MasterPasswordRevisionCode   { get; set; }
        public ushort                             MaxDownloadMicroMode3        { get; set; }
        public ushort                             MaxQueueDepth                { get; set; }
        public Identify.TransferMode              MDMAActive                   { get; set; }
        public Identify.TransferMode              MDMASupported                { get; set; }
        public ushort                             MinDownloadMicroMode3        { get; set; }
        public ushort                             MinMDMACycleTime             { get; set; }
        public ushort                             MinorVersion                 { get; set; }
        public ushort                             MinPIOCycleTimeNoFlow        { get; set; }
        public ushort                             MinPIOCycleTimeFlow          { get; set; }
        public string                             Model                        { get; set; }
        public byte                               MultipleMaxSectors           { get; set; }
        public byte                               MultipleSectorNumber         { get; set; }
        public ushort                             NVCacheCaps                  { get; set; }
        public uint                               NVCacheSize                  { get; set; }
        public ushort                             NVCacheWriteSpeed            { get; set; }
        public byte                               NVEstimatedSpinUp            { get; set; }
        public ushort                             PacketBusRelease             { get; set; }
        public byte                               PIOTransferTimingMode        { get; set; }
        public byte                               RecommendedAAM               { get; set; }
        public ushort                             RecommendedMDMACycleTime     { get; set; }
        public ushort                             RemovableStatusSet           { get; set; }
        public Identify.SATACapabilitiesBit       SATACapabilities             { get; set; }
        public Identify.SATACapabilitiesBit2      SATACapabilities2            { get; set; }
        public Identify.SATAFeaturesBit           SATAFeatures                 { get; set; }
        public Identify.SCTCommandTransportBit    SCTCommandTransport          { get; set; }
        public uint                               SectorsPerCard               { get; set; }
        public ushort                             SecurityEraseTime            { get; set; }
        public Identify.SecurityStatusBit         SecurityStatus               { get; set; }
        public ushort                             ServiceBusyClear             { get; set; }
        public Identify.SpecificConfigurationEnum SpecificConfiguration        { get; set; }
        public ushort                             StreamAccessLatency          { get; set; }
        public ushort                             StreamMinReqSize             { get; set; }
        public uint                               StreamPerformanceGranularity { get; set; }
        public ushort                             StreamTransferTimeDMA        { get; set; }
        public ushort                             StreamTransferTimePIO        { get; set; }
        public ushort                             TransportMajorVersion        { get; set; }
        public ushort                             TransportMinorVersion        { get; set; }
        public Identify.TrustedComputingBit       TrustedComputing             { get; set; }
        public Identify.TransferMode              UDMAActive                   { get; set; }
        public Identify.TransferMode              UDMASupported                { get; set; }
        public byte                               WRVMode                      { get; set; }
        public uint                               WRVSectorCountMode3          { get; set; }
        public uint                               WRVSectorCountMode2          { get; set; }

        public byte[] Identify { get; set; }

        public testedMediaType   ReadCapabilities { get; set; }
        public testedMediaType[] RemovableMedias  { get; set; }

        [XmlIgnore]
        public bool AdditionalPIDSpecified { get; set; }
        [XmlIgnore]
        public bool APIOSupportedSpecified { get; set; }
        [XmlIgnore]
        public bool ATAPIByteCountSpecified { get; set; }
        [XmlIgnore]
        public bool BufferTypeSpecified { get; set; }
        [XmlIgnore]
        public bool BufferSizeSpecified { get; set; }
        [XmlIgnore]
        public bool CapabilitiesSpecified { get; set; }
        [XmlIgnore]
        public bool Capabilities2Specified { get; set; }
        [XmlIgnore]
        public bool Capabilities3Specified { get; set; }
        [XmlIgnore]
        public bool CFAPowerModeSpecified { get; set; }
        [XmlIgnore]
        public bool CommandSetSpecified { get; set; }
        [XmlIgnore]
        public bool CommandSet2Specified { get; set; }
        [XmlIgnore]
        public bool CommandSet3Specified { get; set; }
        [XmlIgnore]
        public bool CommandSet4Specified { get; set; }
        [XmlIgnore]
        public bool CommandSet5Specified { get; set; }
        [XmlIgnore]
        public bool CurrentAAMSpecified { get; set; }
        [XmlIgnore]
        public bool CurrentAPMSpecified { get; set; }
        [XmlIgnore]
        public bool DataSetMgmtSpecified { get; set; }
        [XmlIgnore]
        public bool DataSetMgmtSizeSpecified { get; set; }
        [XmlIgnore]
        public bool DeviceFormFactorSpecified { get; set; }
        [XmlIgnore]
        public bool DMAActiveSpecified { get; set; }
        [XmlIgnore]
        public bool DMASupportedSpecified { get; set; }
        [XmlIgnore]
        public bool DMATransferTimingModeSpecified { get; set; }
        [XmlIgnore]
        public bool EnhancedSecurityEraseTimeSpecified { get; set; }
        [XmlIgnore]
        public bool EnabledCommandSetSpecified { get; set; }
        [XmlIgnore]
        public bool EnabledCommandSet2Specified { get; set; }
        [XmlIgnore]
        public bool EnabledCommandSet3Specified { get; set; }
        [XmlIgnore]
        public bool EnabledCommandSet4Specified { get; set; }
        [XmlIgnore]
        public bool EnabledSATAFeaturesSpecified { get; set; }
        [XmlIgnore]
        public bool ExtendedIdentifySpecified { get; set; }
        [XmlIgnore]
        public bool ExtendedUserSectorsSpecified { get; set; }
        [XmlIgnore]
        public bool FreeFallSensitivitySpecified { get; set; }
        [XmlIgnore]
        public bool FirmwareRevisionSpecified { get; set; }
        [XmlIgnore]
        public bool GeneralConfigurationSpecified { get; set; }
        [XmlIgnore]
        public bool HardwareResetResultSpecified { get; set; }
        [XmlIgnore]
        public bool InterseekDelaySpecified { get; set; }
        [XmlIgnore]
        public bool MajorVersionSpecified { get; set; }
        [XmlIgnore]
        public bool MasterPasswordRevisionCodeSpecified { get; set; }
        [XmlIgnore]
        public bool MaxDownloadMicroMode3Specified { get; set; }
        [XmlIgnore]
        public bool MaxQueueDepthSpecified { get; set; }
        [XmlIgnore]
        public bool MDMAActiveSpecified { get; set; }
        [XmlIgnore]
        public bool MDMASupportedSpecified { get; set; }
        [XmlIgnore]
        public bool MinDownloadMicroMode3Specified { get; set; }
        [XmlIgnore]
        public bool MinMDMACycleTimeSpecified { get; set; }
        [XmlIgnore]
        public bool MinorVersionSpecified { get; set; }
        [XmlIgnore]
        public bool MinPIOCycleTimeNoFlowSpecified { get; set; }
        [XmlIgnore]
        public bool MinPIOCycleTimeFlowSpecified { get; set; }
        [XmlIgnore]
        public bool ModelSpecified { get; set; }
        [XmlIgnore]
        public bool MultipleMaxSectorsSpecified { get; set; }
        [XmlIgnore]
        public bool MultipleSectorNumberSpecified { get; set; }
        [XmlIgnore]
        public bool NVCacheCapsSpecified { get; set; }
        [XmlIgnore]
        public bool NVCacheSizeSpecified { get; set; }
        [XmlIgnore]
        public bool NVCacheWriteSpeedSpecified { get; set; }
        [XmlIgnore]
        public bool NVEstimatedSpinUpSpecified { get; set; }
        [XmlIgnore]
        public bool PacketBusReleaseSpecified { get; set; }
        [XmlIgnore]
        public bool PIOTransferTimingModeSpecified { get; set; }
        [XmlIgnore]
        public bool RecommendedAAMSpecified { get; set; }
        [XmlIgnore]
        public bool RecommendedMDMACycleTimeSpecified { get; set; }
        [XmlIgnore]
        public bool RemovableStatusSetSpecified { get; set; }
        [XmlIgnore]
        public bool SATACapabilitiesSpecified { get; set; }
        [XmlIgnore]
        public bool SATACapabilities2Specified { get; set; }
        [XmlIgnore]
        public bool SATAFeaturesSpecified { get; set; }
        [XmlIgnore]
        public bool SCTCommandTransportSpecified { get; set; }
        [XmlIgnore]
        public bool SectorsPerCardSpecified { get; set; }
        [XmlIgnore]
        public bool SecurityEraseTimeSpecified { get; set; }
        [XmlIgnore]
        public bool SecurityStatusSpecified { get; set; }
        [XmlIgnore]
        public bool ServiceBusyClearSpecified { get; set; }
        [XmlIgnore]
        public bool SpecificConfigurationSpecified { get; set; }
        [XmlIgnore]
        public bool StreamAccessLatencySpecified { get; set; }
        [XmlIgnore]
        public bool StreamMinReqSizeSpecified { get; set; }
        [XmlIgnore]
        public bool StreamPerformanceGranularitySpecified { get; set; }
        [XmlIgnore]
        public bool StreamTransferTimeDMASpecified { get; set; }
        [XmlIgnore]
        public bool StreamTransferTimePIOSpecified { get; set; }
        [XmlIgnore]
        public bool TransportMajorVersionSpecified { get; set; }
        [XmlIgnore]
        public bool TransportMinorVersionSpecified { get; set; }
        [XmlIgnore]
        public bool TrustedComputingSpecified { get; set; }
        [XmlIgnore]
        public bool UDMAActiveSpecified { get; set; }
        [XmlIgnore]
        public bool UDMASupportedSpecified { get; set; }
        [XmlIgnore]
        public bool WRVModeSpecified { get; set; }
        [XmlIgnore]
        public bool WRVSectorCountMode3Specified { get; set; }
        [XmlIgnore]
        public bool WRVSectorCountMode2Specified { get; set; }
    }

    public class chsType
    {
        public ushort Cylinders { get; set; }
        public ushort Heads     { get; set; }
        public ushort Sectors   { get; set; }
    }

    public class scsiType
    {
        public scsiInquiryType   Inquiry              { get; set; }
        public pageType[]        EVPDPages            { get; set; }
        public bool              SupportsModeSense6   { get; set; }
        public bool              SupportsModeSense10  { get; set; }
        public bool              SupportsModeSubpages { get; set; }
        public modeType          ModeSense            { get; set; }
        public mmcType           MultiMediaDevice     { get; set; }
        public testedMediaType   ReadCapabilities     { get; set; }
        public testedMediaType[] RemovableMedias      { get; set; }
        public sscType           SequentialDevice     { get; set; }
        public byte[]            ModeSense6Data       { get; set; }
        public byte[]            ModeSense10Data      { get; set; }

        [XmlIgnore]
        public bool ReadCapabilitiesSpecified { get; set; }
    }

    public class scsiInquiryType
    {
        public bool                  AccessControlCoordinator { get; set; }
        public bool                  ACKRequests              { get; set; }
        public bool                  AERCSupported            { get; set; }
        public bool                  Address16                { get; set; }
        public bool                  Address32                { get; set; }
        public byte                  ANSIVersion              { get; set; }
        public TGPSValues            AsymmetricalLUNAccess    { get; set; }
        public bool                  BasicQueueing            { get; set; }
        public byte                  DeviceTypeModifier       { get; set; }
        public byte                  ECMAVersion              { get; set; }
        public bool                  EnclosureServices        { get; set; }
        public bool                  HierarchicalLUN          { get; set; }
        public bool                  IUS                      { get; set; }
        public byte                  ISOVersion               { get; set; }
        public bool                  LinkedCommands           { get; set; }
        public bool                  MediumChanger            { get; set; }
        public bool                  MultiPortDevice          { get; set; }
        public bool                  NormalACA                { get; set; }
        public PeripheralDeviceTypes PeripheralDeviceType     { get; set; }
        public PeripheralQualifiers  PeripheralQualifier      { get; set; }
        public string                ProductIdentification    { get; set; }
        public string                ProductRevisionLevel     { get; set; }
        public bool                  Protection               { get; set; }
        public bool                  QAS                      { get; set; }
        public bool                  RelativeAddressing       { get; set; }
        public bool                  Removable                { get; set; }
        public byte                  ResponseDataFormat       { get; set; }
        public bool                  TaggedCommandQueue       { get; set; }
        public bool                  TerminateTaskSupported   { get; set; }
        public bool                  ThirdPartyCopy           { get; set; }
        public bool                  TranferDisable           { get; set; }
        public bool                  SoftReset                { get; set; }
        public SPIClocking           SPIClocking              { get; set; }
        public bool                  StorageArrayController   { get; set; }
        public bool                  SyncTransfer             { get; set; }
        public string                VendorIdentification     { get; set; }
        public ushort[]              VersionDescriptors       { get; set; }
        public bool                  WideBus16                { get; set; }
        public bool                  WideBus32                { get; set; }
        public byte[]                Data                     { get; set; }

        [XmlIgnore]
        public bool ANSIVersionSpecified { get; set; }
        [XmlIgnore]
        public bool ECMAVersionSpecified { get; set; }
        [XmlIgnore]
        public bool DeviceTypeModifierSpecified { get; set; }
        [XmlIgnore]
        public bool ISOVersionSpecified { get; set; }
        [XmlIgnore]
        public bool ProductIdentificationSpecified { get; set; }
        [XmlIgnore]
        public bool ProductRevisionLevelSpecified { get; set; }
        [XmlIgnore]
        public bool ResponseDataFormatSpecified { get; set; }
        [XmlIgnore]
        public bool VendorIdentificationSpecified { get; set; }
    }

    [Serializable]
    public class pageType
    {
        [XmlAttribute]
        public byte page { get; set; }

        [XmlText]
        public byte[] value { get; set; }
    }

    public class modeType
    {
        public byte                  MediumType        { get; set; }
        public bool                  WriteProtected    { get; set; }
        public blockDescriptorType[] BlockDescriptors  { get; set; }
        public byte                  Speed             { get; set; }
        public byte                  BufferedMode      { get; set; }
        public bool                  BlankCheckEnabled { get; set; }
        public bool                  DPOandFUA         { get; set; }
        public modePageType[]        ModePages         { get; set; }

        [XmlIgnore]
        public bool MediumTypeSpecified { get; set; }
        [XmlIgnore]
        public bool SpeedSpecified { get; set; }
        [XmlIgnore]
        public bool BufferedModeSpecified { get; set; }
    }

    public class blockDescriptorType
    {
        public byte  Density     { get; set; }
        public ulong Blocks      { get; set; }
        public uint  BlockLength { get; set; }

        [XmlIgnore]
        public bool BlocksSpecified { get; set; }
        [XmlIgnore]
        public bool BlockLengthSpecified { get; set; }
    }

    [Serializable]
    public class modePageType
    {
        [XmlAttribute]
        public byte page { get; set; }

        [XmlAttribute]
        public byte subpage { get; set; }

        [XmlText]
        public byte[] value { get; set; }
    }

    public class mmcType
    {
        public mmcModeType       ModeSense2A { get; set; }
        public mmcFeaturesType   Features    { get; set; }
        public testedMediaType[] TestedMedia { get; set; }
    }

    public class mmcModeType
    {
        public bool                          AccurateCDDA                     { get; set; }
        public bool                          BCK                              { get; set; }
        public ushort                        BufferSize                       { get; set; }
        public bool                          BufferUnderRunProtection         { get; set; }
        public bool                          CanEject                         { get; set; }
        public bool                          CanLockMedia                     { get; set; }
        public bool                          CDDACommand                      { get; set; }
        public bool                          CompositeAudioVideo              { get; set; }
        public bool                          CSSandCPPMSupported              { get; set; }
        public ushort                        CurrentSpeed                     { get; set; }
        public ushort                        CurrentWriteSpeed                { get; set; }
        public ushort                        CurrentWriteSpeedSelected        { get; set; }
        public bool                          DeterministicSlotChanger         { get; set; }
        public bool                          DigitalPort1                     { get; set; }
        public bool                          DigitalPort2                     { get; set; }
        public bool                          LeadInPW                         { get; set; }
        public byte                          LoadingMechanismType             { get; set; }
        public bool                          LockStatus                       { get; set; }
        public bool                          LSBF                             { get; set; }
        public ushort                        MaximumSpeed                     { get; set; }
        public ushort                        MaximumWriteSpeed                { get; set; }
        public bool                          PlaysAudio                       { get; set; }
        public bool                          PreventJumperStatus              { get; set; }
        public bool                          RCK                              { get; set; }
        public bool                          ReadsBarcode                     { get; set; }
        public bool                          ReadsBothSides                   { get; set; }
        public bool                          ReadsCDR                         { get; set; }
        public bool                          ReadsCDRW                        { get; set; }
        public bool                          ReadsDeinterlavedSubchannel      { get; set; }
        public bool                          ReadsDVDR                        { get; set; }
        public bool                          ReadsDVDRAM                      { get; set; }
        public bool                          ReadsDVDROM                      { get; set; }
        public bool                          ReadsISRC                        { get; set; }
        public bool                          ReadsMode2Form2                  { get; set; }
        public bool                          ReadsMode2Form1                  { get; set; }
        public bool                          ReadsPacketCDR                   { get; set; }
        public bool                          ReadsSubchannel                  { get; set; }
        public bool                          ReadsUPC                         { get; set; }
        public bool                          ReturnsC2Pointers                { get; set; }
        public byte                          RotationControlSelected          { get; set; }
        public bool                          SeparateChannelMute              { get; set; }
        public bool                          SeparateChannelVolume            { get; set; }
        public bool                          SSS                              { get; set; }
        public bool                          SupportsMultiSession             { get; set; }
        public ushort                        SupportedVolumeLevels            { get; set; }
        public bool                          TestWrite                        { get; set; }
        public bool                          WritesCDR                        { get; set; }
        public bool                          WritesCDRW                       { get; set; }
        public bool                          WritesDVDR                       { get; set; }
        public bool                          WritesDVDRAM                     { get; set; }
        public ModePage_2A_WriteDescriptor[] WriteSpeedPerformanceDescriptors { get; set; }

        [XmlIgnore]
        public bool MaximumSpeedSpecified { get; set; }
        [XmlIgnore]
        public bool SupportedVolumeLevelsSpecified { get; set; }
        [XmlIgnore]
        public bool BufferSizeSpecified { get; set; }
        [XmlIgnore]
        public bool CurrentSpeedSpecified { get; set; }
        [XmlIgnore]
        public bool MaximumWriteSpeedSpecified { get; set; }
        [XmlIgnore]
        public bool CurrentWriteSpeedSpecified { get; set; }
        [XmlIgnore]
        public bool RotationControlSelectedSpecified { get; set; }
        [XmlIgnore]
        public bool CurrentWriteSpeedSelectedSpecified { get; set; }
    }

    public class mmcFeaturesType
    {
        public byte   AACSVersion                   { get; set; }
        public byte   AGIDs                         { get; set; }
        public byte   BindingNonceBlocks            { get; set; }
        public ushort BlocksPerReadableUnit         { get; set; }
        public bool   BufferUnderrunFreeInDVD       { get; set; }
        public bool   BufferUnderrunFreeInSAO       { get; set; }
        public bool   BufferUnderrunFreeInTAO       { get; set; }
        public bool   CanAudioScan                  { get; set; }
        public bool   CanEject                      { get; set; }
        public bool   CanEraseSector                { get; set; }
        public bool   CanExpandBDRESpareArea        { get; set; }
        public bool   CanFormat                     { get; set; }
        public bool   CanFormatBDREWithoutSpare     { get; set; }
        public bool   CanFormatCert                 { get; set; }
        public bool   CanFormatFRF                  { get; set; }
        public bool   CanFormatQCert                { get; set; }
        public bool   CanFormatRRM                  { get; set; }
        public bool   CanGenerateBindingNonce       { get; set; }
        public bool   CanLoad                       { get; set; }
        public bool   CanMuteSeparateChannels       { get; set; }
        public bool   CanOverwriteSAOTrack          { get; set; }
        public bool   CanOverwriteTAOTrack          { get; set; }
        public bool   CanPlayCDAudio                { get; set; }
        public bool   CanPseudoOverwriteBDR         { get; set; }
        public bool   CanReadAllDualR               { get; set; }
        public bool   CanReadAllDualRW              { get; set; }
        public bool   CanReadBD                     { get; set; }
        public bool   CanReadBDR                    { get; set; }
        public bool   CanReadBDRE1                  { get; set; }
        public bool   CanReadBDRE2                  { get; set; }
        public bool   CanReadBDROM                  { get; set; }
        public bool   CanReadBluBCA                 { get; set; }
        public bool   CanReadCD                     { get; set; }
        public bool   CanReadCDMRW                  { get; set; }
        public bool   CanReadCPRM_MKB               { get; set; }
        public bool   CanReadDDCD                   { get; set; }
        public bool   CanReadDVD                    { get; set; }
        public bool   CanReadDVDPlusMRW             { get; set; }
        public bool   CanReadDVDPlusR               { get; set; }
        public bool   CanReadDVDPlusRDL             { get; set; }
        public bool   CanReadDVDPlusRW              { get; set; }
        public bool   CanReadDVDPlusRWDL            { get; set; }
        public bool   CanReadDriveAACSCertificate   { get; set; }
        public bool   CanReadHDDVD                  { get; set; }
        public bool   CanReadHDDVDR                 { get; set; }
        public bool   CanReadHDDVDRAM               { get; set; }
        public bool   CanReadLeadInCDText           { get; set; }
        public bool   CanReadOldBDR                 { get; set; }
        public bool   CanReadOldBDRE                { get; set; }
        public bool   CanReadOldBDROM               { get; set; }
        public bool   CanReadSpareAreaInformation   { get; set; }
        public bool   CanReportDriveSerial          { get; set; }
        public bool   CanReportMediaSerial          { get; set; }
        public bool   CanTestWriteDDCDR             { get; set; }
        public bool   CanTestWriteDVD               { get; set; }
        public bool   CanTestWriteInSAO             { get; set; }
        public bool   CanTestWriteInTAO             { get; set; }
        public bool   CanUpgradeFirmware            { get; set; }
        public bool   CanWriteBD                    { get; set; }
        public bool   CanWriteBDR                   { get; set; }
        public bool   CanWriteBDRE1                 { get; set; }
        public bool   CanWriteBDRE2                 { get; set; }
        public bool   CanWriteBusEncryptedBlocks    { get; set; }
        public bool   CanWriteCDMRW                 { get; set; }
        public bool   CanWriteCDRW                  { get; set; }
        public bool   CanWriteCDRWCAV               { get; set; }
        public bool   CanWriteCDSAO                 { get; set; }
        public bool   CanWriteCDTAO                 { get; set; }
        public bool   CanWriteCSSManagedDVD         { get; set; }
        public bool   CanWriteDDCDR                 { get; set; }
        public bool   CanWriteDDCDRW                { get; set; }
        public bool   CanWriteDVDPlusMRW            { get; set; }
        public bool   CanWriteDVDPlusR              { get; set; }
        public bool   CanWriteDVDPlusRDL            { get; set; }
        public bool   CanWriteDVDPlusRW             { get; set; }
        public bool   CanWriteDVDPlusRWDL           { get; set; }
        public bool   CanWriteDVDR                  { get; set; }
        public bool   CanWriteDVDRDL                { get; set; }
        public bool   CanWriteDVDRW                 { get; set; }
        public bool   CanWriteHDDVDR                { get; set; }
        public bool   CanWriteHDDVDRAM              { get; set; }
        public bool   CanWriteOldBDR                { get; set; }
        public bool   CanWriteOldBDRE               { get; set; }
        public bool   CanWritePackedSubchannelInTAO { get; set; }
        public bool   CanWriteRWSubchannelInSAO     { get; set; }
        public bool   CanWriteRWSubchannelInTAO     { get; set; }
        public bool   CanWriteRaw                   { get; set; }
        public bool   CanWriteRawMultiSession       { get; set; }
        public bool   CanWriteRawSubchannelInTAO    { get; set; }
        public bool   ChangerIsSideChangeCapable    { get; set; }
        public byte   ChangerSlots                  { get; set; }
        public bool   ChangerSupportsDiscPresent    { get; set; }
        public byte   CPRMVersion                   { get; set; }
        public byte   CSSVersion                    { get; set; }
        public bool   DBML                          { get; set; }
        public bool   DVDMultiRead                  { get; set; }
        public bool   EmbeddedChanger               { get; set; }
        public bool   ErrorRecoveryPage             { get; set; }
        [XmlElement(DataType = "date")]
        public DateTime FirmwareDate { get;                              set; }
        public byte               LoadingMechanismType            { get; set; }
        public bool               Locked                          { get; set; }
        public uint               LogicalBlockSize                { get; set; }
        public bool               MultiRead                       { get; set; }
        public PhysicalInterfaces PhysicalInterfaceStandard       { get; set; }
        public uint               PhysicalInterfaceStandardNumber { get; set; }
        public bool               PreventJumper                   { get; set; }
        public bool               SupportsAACS                    { get; set; }
        public bool               SupportsBusEncryption           { get; set; }
        public bool               SupportsC2                      { get; set; }
        public bool               SupportsCPRM                    { get; set; }
        public bool               SupportsCSS                     { get; set; }
        public bool               SupportsDAP                     { get; set; }
        public bool               SupportsDeviceBusyEvent         { get; set; }
        public bool               SupportsHybridDiscs             { get; set; }
        public bool               SupportsModePage1Ch             { get; set; }
        public bool               SupportsOSSC                    { get; set; }
        public bool               SupportsPWP                     { get; set; }
        public bool               SupportsSWPP                    { get; set; }
        public bool               SupportsSecurDisc               { get; set; }
        public bool               SupportsSeparateVolume          { get; set; }
        public bool               SupportsVCPS                    { get; set; }
        public bool               SupportsWriteInhibitDCB         { get; set; }
        public bool               SupportsWriteProtectPAC         { get; set; }
        public ushort             VolumeLevels                    { get; set; }

        [XmlIgnore]
        public bool PhysicalInterfaceStandardSpecified { get; set; }
        [XmlIgnore]
        public bool PhysicalInterfaceStandardNumberSpecified { get; set; }
        [XmlIgnore]
        public bool AACSVersionSpecified { get; set; }
        [XmlIgnore]
        public bool AGIDsSpecified { get; set; }
        [XmlIgnore]
        public bool BindingNonceBlocksSpecified { get; set; }
        [XmlIgnore]
        public bool CPRMVersionSpecified { get; set; }
        [XmlIgnore]
        public bool CSSVersionSpecified { get; set; }
        [XmlIgnore]
        public bool ChangerHighestSlotNumberSpecified { get; set; }
        [XmlIgnore]
        public bool LoadingMechanismTypeSpecified { get; set; }
        [XmlIgnore]
        public bool LogicalBlockSizeSpecified { get; set; }
        [XmlIgnore]
        public bool BlocksPerReadableUnitSpecified { get; set; }
        [XmlIgnore]
        public bool FirmwareDateSpecified { get; set; }
        [XmlIgnore]
        public bool VolumeLevelsSpecified { get; set; }
    }

    public class testedMediaType
    {
        public ulong  Blocks                           { get; set; }
        public uint   BlockSize                        { get; set; }
        public bool   CanReadAACS                      { get; set; }
        public bool   CanReadADIP                      { get; set; }
        public bool   CanReadATIP                      { get; set; }
        public bool   CanReadBCA                       { get; set; }
        public bool   CanReadC2Pointers                { get; set; }
        public bool   CanReadCMI                       { get; set; }
        public bool   CanReadCorrectedSubchannel       { get; set; }
        public bool   CanReadCorrectedSubchannelWithC2 { get; set; }
        public bool   CanReadDCB                       { get; set; }
        public bool   CanReadDDS                       { get; set; }
        public bool   CanReadDMI                       { get; set; }
        public bool   CanReadDiscInformation           { get; set; }
        public bool   CanReadFullTOC                   { get; set; }
        public bool   CanReadHDCMI                     { get; set; }
        public bool   CanReadLayerCapacity             { get; set; }
        public bool   CanReadLeadIn                    { get; set; }
        public bool   CanReadLeadOut                   { get; set; }
        public bool   CanReadMediaID                   { get; set; }
        public bool   CanReadMediaSerial               { get; set; }
        public bool   CanReadPAC                       { get; set; }
        public bool   CanReadPFI                       { get; set; }
        public bool   CanReadPMA                       { get; set; }
        public bool   CanReadPQSubchannel              { get; set; }
        public bool   CanReadPQSubchannelWithC2        { get; set; }
        public bool   CanReadPRI                       { get; set; }
        public bool   CanReadRWSubchannel              { get; set; }
        public bool   CanReadRWSubchannelWithC2        { get; set; }
        public bool   CanReadRecordablePFI             { get; set; }
        public bool   CanReadSpareAreaInformation      { get; set; }
        public bool   CanReadTOC                       { get; set; }
        public byte   Density                          { get; set; }
        public uint   LongBlockSize                    { get; set; }
        public string Manufacturer                     { get; set; }
        public bool   MediaIsRecognized                { get; set; }
        public byte   MediumType                       { get; set; }
        public string MediumTypeName                   { get; set; }
        public string Model                            { get; set; }
        public bool   SupportsHLDTSTReadRawDVD         { get; set; }
        public bool   SupportsNECReadCDDA              { get; set; }
        public bool   SupportsPioneerReadCDDA          { get; set; }
        public bool   SupportsPioneerReadCDDAMSF       { get; set; }
        public bool   SupportsPlextorReadCDDA          { get; set; }
        public bool   SupportsPlextorReadRawDVD        { get; set; }
        public bool   SupportsRead10                   { get; set; }
        public bool   SupportsRead12                   { get; set; }
        public bool   SupportsRead16                   { get; set; }
        public bool   SupportsRead                     { get; set; }
        public bool   SupportsReadCapacity16           { get; set; }
        public bool   SupportsReadCapacity             { get; set; }
        public bool   SupportsReadCd                   { get; set; }
        public bool   SupportsReadCdMsf                { get; set; }
        public bool   SupportsReadCdRaw                { get; set; }
        public bool   SupportsReadCdMsfRaw             { get; set; }
        public bool   SupportsReadLong16               { get; set; }
        public bool   SupportsReadLong                 { get; set; }

        public byte[] ModeSense6Data  { get; set; }
        public byte[] ModeSense10Data { get; set; }

        [XmlIgnore]
        public bool BlocksSpecified { get; set; }
        [XmlIgnore]
        public bool BlockSizeSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadAACSSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadADIPSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadATIPSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadBCASpecified { get; set; }
        [XmlIgnore]
        public bool CanReadC2PointersSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadCMISpecified { get; set; }
        [XmlIgnore]
        public bool CanReadCorrectedSubchannelSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadCorrectedSubchannelWithC2Specified { get; set; }
        [XmlIgnore]
        public bool CanReadDCBSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadDDSSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadDMISpecified { get; set; }
        [XmlIgnore]
        public bool CanReadDiscInformationSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadFullTOCSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadHDCMISpecified { get; set; }
        [XmlIgnore]
        public bool CanReadLayerCapacitySpecified { get; set; }
        [XmlIgnore]
        public bool CanReadLeadInSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadLeadOutSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadMediaIDSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadMediaSerialSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadPACSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadPFISpecified { get; set; }
        [XmlIgnore]
        public bool CanReadPMASpecified { get; set; }
        [XmlIgnore]
        public bool CanReadPQSubchannelSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadPQSubchannelWithC2Specified { get; set; }
        [XmlIgnore]
        public bool CanReadPRISpecified { get; set; }
        [XmlIgnore]
        public bool CanReadRWSubchannelSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadRWSubchannelWithC2Specified { get; set; }
        [XmlIgnore]
        public bool CanReadRecordablePFISpecified { get; set; }
        [XmlIgnore]
        public bool CanReadSpareAreaInformationSpecified { get; set; }
        [XmlIgnore]
        public bool CanReadTOCSpecified { get; set; }
        [XmlIgnore]
        public bool DensitySpecified { get; set; }
        [XmlIgnore]
        public bool LongBlockSizeSpecified { get; set; }
        [XmlIgnore]
        public bool ManufacturerSpecified { get; set; }
        [XmlIgnore]
        public bool MediumTypeSpecified { get; set; }
        [XmlIgnore]
        public bool ModelSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsHLDTSTReadRawDVDSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsNECReadCDDASpecified { get; set; }
        [XmlIgnore]
        public bool SupportsPioneerReadCDDASpecified { get; set; }
        [XmlIgnore]
        public bool SupportsPioneerReadCDDAMSFSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsPlextorReadCDDASpecified { get; set; }
        [XmlIgnore]
        public bool SupportsPlextorReadRawDVDSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsRead10Specified { get; set; }
        [XmlIgnore]
        public bool SupportsRead12Specified { get; set; }
        [XmlIgnore]
        public bool SupportsRead16Specified { get; set; }
        [XmlIgnore]
        public bool SupportsReadSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadCapacity16Specified { get; set; }
        [XmlIgnore]
        public bool SupportsReadCapacitySpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadCdSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadCdMsfSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadCdRawSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadCdMsfRawSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadLong16Specified { get; set; }
        [XmlIgnore]
        public bool SupportsReadLongSpecified { get; set; }

        public chsType CHS                 { get; set; }
        public chsType CurrentCHS          { get; set; }
        public uint    LBASectors          { get; set; }
        public ulong   LBA48Sectors        { get; set; }
        public ushort  LogicalAlignment    { get; set; }
        public ushort  NominalRotationRate { get; set; }
        public uint    PhysicalBlockSize   { get; set; }
        public bool    SolidStateDevice    { get; set; }
        public ushort  UnformattedBPT      { get; set; }
        public ushort  UnformattedBPS      { get; set; }

        [XmlIgnore]
        public bool LBASectorsSpecified { get; set; }
        [XmlIgnore]
        public bool LBA48SectorsSpecified { get; set; }
        [XmlIgnore]
        public bool LogicalAlignmentSpecified { get; set; }
        [XmlIgnore]
        public bool NominalRotationRateSpecified { get; set; }
        [XmlIgnore]
        public bool PhysicalBlockSizeSpecified { get; set; }
        [XmlIgnore]
        public bool SolidStateDeviceSpecified { get; set; }
        [XmlIgnore]
        public bool UnformattedBPTSpecified { get; set; }
        [XmlIgnore]
        public bool UnformattedBPSSpecified { get; set; }

        public bool SupportsReadDmaLba       { get; set; }
        public bool SupportsReadDmaRetryLba  { get; set; }
        public bool SupportsReadLba          { get; set; }
        public bool SupportsReadRetryLba     { get; set; }
        public bool SupportsReadLongLba      { get; set; }
        public bool SupportsReadLongRetryLba { get; set; }
        public bool SupportsSeekLba          { get; set; }

        public bool SupportsReadDmaLba48 { get; set; }
        public bool SupportsReadLba48    { get; set; }

        public bool SupportsReadDma       { get; set; }
        public bool SupportsReadDmaRetry  { get; set; }
        public bool SupportsReadRetry     { get; set; }
        public bool SupportsReadLongRetry { get; set; }
        public bool SupportsSeek          { get; set; }

        [XmlIgnore]
        public bool SupportsReadDmaLbaSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadDmaRetryLbaSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadLbaSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadRetryLbaSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadLongLbaSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadLongRetryLbaSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsSeekLbaSpecified { get; set; }

        [XmlIgnore]
        public bool SupportsReadDmaLba48Specified { get; set; }
        [XmlIgnore]
        public bool SupportsReadLba48Specified { get; set; }

        [XmlIgnore]
        public bool SupportsReadDmaSpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadDmaRetrySpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadRetrySpecified { get; set; }
        [XmlIgnore]
        public bool SupportsReadLongRetrySpecified { get; set; }
        [XmlIgnore]
        public bool SupportsSeekSpecified { get; set; }
    }

    public class sscType
    {
        public byte BlockSizeGranularity { get; set; }
        public uint MaxBlockLength       { get; set; }
        public uint MinBlockLength       { get; set; }

        public SupportedDensity[] SupportedDensities  { get; set; }
        public SupportedMedia[]   SupportedMediaTypes { get; set; }
        public SequentialMedia[]  TestedMedia         { get; set; }

        [XmlIgnore]
        public bool BlockSizeGranularitySpecified { get; set; }
        [XmlIgnore]
        public bool MaxBlockLengthSpecified { get; set; }
        [XmlIgnore]
        public bool MinBlockLengthSpecified { get; set; }
    }

    public class SupportedDensity
    {
        [XmlIgnore, JsonIgnore]
        public int Id { get; set; }
        [DisplayName("Primary density code")]
        public byte PrimaryCode { get; set; }
        [DisplayName("Secondary density code")]
        public byte SecondaryCode { get; set; }
        public bool Writable  { get;     set; }
        public bool Duplicate { get;     set; }
        [DisplayName("Default density code")]
        public bool DefaultDensity { get; set; }
        [DisplayName("Bits per mm")]
        public uint BitsPerMm { get; set; }
        public ushort Width  { get;  set; }
        public ushort Tracks { get;  set; }
        [DisplayName("Nominal capacity (MiB)")]
        public uint Capacity { get;       set; }
        public string Organization { get; set; }
        public string Name         { get; set; }
        public string Description  { get; set; }
    }

    public class SupportedMedia
    {
        [XmlIgnore, JsonIgnore]
        public int Id { get;              set; }
        public byte   MediumType   { get; set; }
        public int[]  DensityCodes { get; set; }
        public ushort Width        { get; set; }
        public ushort Length       { get; set; }
        public string Organization { get; set; }
        public string Name         { get; set; }
        public string Description  { get; set; }
    }

    public struct SequentialMedia
    {
        public bool               CanReadMediaSerial  { get; set; }
        public byte               Density             { get; set; }
        public string             Manufacturer        { get; set; }
        public bool               MediaIsRecognized   { get; set; }
        public byte               MediumType          { get; set; }
        public string             MediumTypeName      { get; set; }
        public string             Model               { get; set; }
        public SupportedDensity[] SupportedDensities  { get; set; }
        public SupportedMedia[]   SupportedMediaTypes { get; set; }

        public byte[] ModeSense6Data  { get; set; }
        public byte[] ModeSense10Data { get; set; }

        [XmlIgnore]
        public bool CanReadMediaSerialSpecified { get; set; }
        [XmlIgnore]
        public bool DensitySpecified { get; set; }
        [XmlIgnore]
        public bool MediumTypeSpecified { get; set; }
    }

    [Serializable]
    public class pcmciaType
    {
        public byte[]   CIS                   { get; set; }
        public string   Compliance            { get; set; }
        public ushort   ManufacturerCode      { get; set; }
        public ushort   CardCode              { get; set; }
        public string   Manufacturer          { get; set; }
        public string   ProductName           { get; set; }
        public string[] AdditionalInformation { get; set; }

        [XmlIgnore]
        public bool ManufacturerCodeSpecified { get; set; }
        [XmlIgnore]
        public bool CardCodeSpecified { get; set; }
    }

    [Serializable]
    public class mmcsdType
    {
        public byte[] CID         { get; set; }
        public byte[] CSD         { get; set; }
        public byte[] OCR         { get; set; }
        public byte[] SCR         { get; set; }
        public byte[] ExtendedCSD { get; set; }
    }
}