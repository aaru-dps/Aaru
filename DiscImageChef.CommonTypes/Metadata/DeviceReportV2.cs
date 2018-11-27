// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DeviceReportV2.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : JSON metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains classes for a JSON device report.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DiscImageChef.CommonTypes.Metadata
{
    public class DeviceReportV2
    {
        [JsonIgnore]
        public int Id { get;                  set; }
        public Usb      USB            { get; set; }
        public FireWire FireWire       { get; set; }
        public Pcmcia   PCMCIA         { get; set; }
        public bool     CompactFlash   { get; set; }
        public Ata      ATA            { get; set; }
        public Ata      ATAPI          { get; set; }
        public Scsi     SCSI           { get; set; }
        public MmcSd    MultiMediaCard { get; set; }
        public MmcSd    SecureDigital  { get; set; }

        public string     Manufacturer { get; set; }
        public string     Model        { get; set; }
        public string     Revision     { get; set; }
        public DeviceType Type         { get; set; }
    }

    public class Usb
    {
        [JsonIgnore]
        public int Id { get;                set; }
        public ushort VendorID       { get; set; }
        public ushort ProductID      { get; set; }
        public string Manufacturer   { get; set; }
        public string Product        { get; set; }
        public bool   RemovableMedia { get; set; }
        public byte[] Descriptors    { get; set; }
    }

    public class FireWire
    {
        [JsonIgnore]
        public int Id { get;                set; }
        public uint   VendorID       { get; set; }
        public uint   ProductID      { get; set; }
        public string Manufacturer   { get; set; }
        public string Product        { get; set; }
        public bool   RemovableMedia { get; set; }
    }

    public class Ata
    {
        public Identify.IdentifyDevice? IdentifyDevice;
        [JsonIgnore]
        public int Id { get;                             set; }
        public byte[]            Identify         { get; set; }
        public TestedMedia       ReadCapabilities { get; set; }
        public List<TestedMedia> RemovableMedias  { get; set; }
    }

    public class Chs
    {
        [JsonIgnore]
        public int Id { get;           set; }
        public ushort Cylinders { get; set; }
        public ushort Heads     { get; set; }
        public ushort Sectors   { get; set; }
    }

    public class Scsi
    {
        public Inquiry.SCSIInquiry? Inquiry;
        [JsonIgnore]
        public int Id { get;                                 set; }
        public byte[]            InquiryData          { get; set; }
        public List<ScsiPage>    EVPDPages            { get; set; }
        public bool              SupportsModeSense6   { get; set; }
        public bool              SupportsModeSense10  { get; set; }
        public bool              SupportsModeSubpages { get; set; }
        public ScsiMode          ModeSense            { get; set; }
        public Mmc               MultiMediaDevice     { get; set; }
        public TestedMedia       ReadCapabilities     { get; set; }
        public List<TestedMedia> RemovableMedias      { get; set; }
        public Ssc               SequentialDevice     { get; set; }
        public byte[]            ModeSense6Data       { get; set; }
        public byte[]            ModeSense10Data      { get; set; }
    }

    public class ScsiMode
    {
        [JsonIgnore]
        public int Id { get;                                  set; }
        public byte?                 MediumType        { get; set; }
        public bool                  WriteProtected    { get; set; }
        public List<BlockDescriptor> BlockDescriptors  { get; set; }
        public byte?                 Speed             { get; set; }
        public byte?                 BufferedMode      { get; set; }
        public bool                  BlankCheckEnabled { get; set; }
        public bool                  DPOandFUA         { get; set; }
        public List<ScsiPage>        ModePages         { get; set; }
    }

    public class BlockDescriptor
    {
        [JsonIgnore]
        public int Id { get;             set; }
        public byte   Density     { get; set; }
        public ulong? Blocks      { get; set; }
        public uint?  BlockLength { get; set; }
    }

    public class ScsiPage
    {
        [JsonIgnore]
        public int Id { get;         set; }
        public byte   page    { get; set; }
        public byte?  subpage { get; set; }
        public byte[] value   { get; set; }
    }

    public class Mmc
    {
        [JsonIgnore]
        public int Id { get;                        set; }
        public Modes.ModePage_2A ModeSense2A { get; set; }
        public MmcFeatures       Features    { get; set; }
        public List<TestedMedia> TestedMedia { get; set; }
    }

    public class MmcFeatures
    {
        [JsonIgnore]
        public int Id { get;                                              set; }
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
        public bool                ChangerSupportsDiscPresent      { get; set; }
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
    }

    public class TestedMedia
    {
        public Identify.IdentifyDevice? IdentifyDevice;
        [JsonIgnore]
        public int Id { get;                                  set; }
        public byte[] IdentifyData                     { get; set; }
        public ulong? Blocks                           { get; set; }
        public uint?  BlockSize                        { get; set; }
        public bool?  CanReadAACS                      { get; set; }
        public bool?  CanReadADIP                      { get; set; }
        public bool?  CanReadATIP                      { get; set; }
        public bool?  CanReadBCA                       { get; set; }
        public bool?  CanReadC2Pointers                { get; set; }
        public bool?  CanReadCMI                       { get; set; }
        public bool?  CanReadCorrectedSubchannel       { get; set; }
        public bool?  CanReadCorrectedSubchannelWithC2 { get; set; }
        public bool?  CanReadDCB                       { get; set; }
        public bool?  CanReadDDS                       { get; set; }
        public bool?  CanReadDMI                       { get; set; }
        public bool?  CanReadDiscInformation           { get; set; }
        public bool?  CanReadFullTOC                   { get; set; }
        public bool?  CanReadHDCMI                     { get; set; }
        public bool?  CanReadLayerCapacity             { get; set; }
        public bool?  CanReadFirstTrackPreGap          { get; set; }
        public bool?  CanReadLeadIn                    { get; set; }
        public bool?  CanReadLeadOut                   { get; set; }
        public bool?  CanReadMediaID                   { get; set; }
        public bool?  CanReadMediaSerial               { get; set; }
        public bool?  CanReadPAC                       { get; set; }
        public bool?  CanReadPFI                       { get; set; }
        public bool?  CanReadPMA                       { get; set; }
        public bool?  CanReadPQSubchannel              { get; set; }
        public bool?  CanReadPQSubchannelWithC2        { get; set; }
        public bool?  CanReadPRI                       { get; set; }
        public bool?  CanReadRWSubchannel              { get; set; }
        public bool?  CanReadRWSubchannelWithC2        { get; set; }
        public bool?  CanReadRecordablePFI             { get; set; }
        public bool?  CanReadSpareAreaInformation      { get; set; }
        public bool?  CanReadTOC                       { get; set; }
        public byte?  Density                          { get; set; }
        public uint?  LongBlockSize                    { get; set; }
        public string Manufacturer                     { get; set; }
        public bool   MediaIsRecognized                { get; set; }
        public byte?  MediumType                       { get; set; }
        public string MediumTypeName                   { get; set; }
        public string Model                            { get; set; }
        public bool?  SupportsHLDTSTReadRawDVD         { get; set; }
        public bool?  SupportsNECReadCDDA              { get; set; }
        public bool?  SupportsPioneerReadCDDA          { get; set; }
        public bool?  SupportsPioneerReadCDDAMSF       { get; set; }
        public bool?  SupportsPlextorReadCDDA          { get; set; }
        public bool?  SupportsPlextorReadRawDVD        { get; set; }
        public bool?  SupportsRead10                   { get; set; }
        public bool?  SupportsRead12                   { get; set; }
        public bool?  SupportsRead16                   { get; set; }
        public bool?  SupportsRead6                    { get; set; }
        public bool?  SupportsReadCapacity16           { get; set; }
        public bool?  SupportsReadCapacity             { get; set; }
        public bool?  SupportsReadCd                   { get; set; }
        public bool?  SupportsReadCdMsf                { get; set; }
        public bool?  SupportsReadCdRaw                { get; set; }
        public bool?  SupportsReadCdMsfRaw             { get; set; }
        public bool?  SupportsReadLong16               { get; set; }
        public bool?  SupportsReadLong                 { get; set; }

        public byte[] ModeSense6Data  { get; set; }
        public byte[] ModeSense10Data { get; set; }

        public Chs     CHS                 { get; set; }
        public Chs     CurrentCHS          { get; set; }
        public uint?   LBASectors          { get; set; }
        public ulong?  LBA48Sectors        { get; set; }
        public ushort? LogicalAlignment    { get; set; }
        public ushort? NominalRotationRate { get; set; }
        public uint?   PhysicalBlockSize   { get; set; }
        public bool?   SolidStateDevice    { get; set; }
        public ushort? UnformattedBPT      { get; set; }
        public ushort? UnformattedBPS      { get; set; }

        public bool? SupportsReadDmaLba       { get; set; }
        public bool? SupportsReadDmaRetryLba  { get; set; }
        public bool? SupportsReadLba          { get; set; }
        public bool? SupportsReadRetryLba     { get; set; }
        public bool? SupportsReadLongLba      { get; set; }
        public bool? SupportsReadLongRetryLba { get; set; }
        public bool? SupportsSeekLba          { get; set; }

        public bool? SupportsReadDmaLba48 { get; set; }
        public bool? SupportsReadLba48    { get; set; }

        public bool? SupportsReadDma       { get; set; }
        public bool? SupportsReadDmaRetry  { get; set; }
        public bool? SupportsReadRetry     { get; set; }
        public bool? SupportsReadSectors   { get; set; }
        public bool? SupportsReadLongRetry { get; set; }
        public bool? SupportsSeek          { get; set; }
    }

    public class Ssc
    {
        [JsonIgnore]
        public int Id { get;                     set; }
        public byte? BlockSizeGranularity { get; set; }
        public uint? MaxBlockLength       { get; set; }
        public uint? MinBlockLength       { get; set; }

        public List<SupportedDensity>      SupportedDensities  { get; set; }
        public List<SscSupportedMedia>     SupportedMediaTypes { get; set; }
        public List<TestedSequentialMedia> TestedMedia         { get; set; }
    }

    public class TestedSequentialMedia
    {
        [JsonIgnore]
        public int Id { get;                                      set; }
        public bool?                   CanReadMediaSerial  { get; set; }
        public byte?                   Density             { get; set; }
        public string                  Manufacturer        { get; set; }
        public bool                    MediaIsRecognized   { get; set; }
        public byte?                   MediumType          { get; set; }
        public string                  MediumTypeName      { get; set; }
        public string                  Model               { get; set; }
        public List<SupportedDensity>  SupportedDensities  { get; set; }
        public List<SscSupportedMedia> SupportedMediaTypes { get; set; }

        public byte[] ModeSense6Data  { get; set; }
        public byte[] ModeSense10Data { get; set; }
    }

    public class Pcmcia
    {
        public string[] AdditionalInformation;
        [JsonIgnore]
        public int Id { get;                   set; }
        public byte[]  CIS              { get; set; }
        public string  Compliance       { get; set; }
        public ushort? ManufacturerCode { get; set; }
        public ushort? CardCode         { get; set; }
        public string  Manufacturer     { get; set; }
        public string  ProductName      { get; set; }
    }

    public class MmcSd
    {
        [JsonIgnore]
        public int Id { get;             set; }
        public byte[] CID         { get; set; }
        public byte[] CSD         { get; set; }
        public byte[] OCR         { get; set; }
        public byte[] SCR         { get; set; }
        public byte[] ExtendedCSD { get; set; }
    }

    public class SscSupportedMedia
    {
        [JsonIgnore]
        public int Id { get;                         set; }
        public byte              MediumType   { get; set; }
        public List<DensityCode> DensityCodes { get; set; }
        public ushort            Width        { get; set; }
        public ushort            Length       { get; set; }
        public string            Organization { get; set; }
        public string            Name         { get; set; }
        public string            Description  { get; set; }
    }

    public class DensityCode
    {
        [Key]
        public int Code { get; set; }
    }
}