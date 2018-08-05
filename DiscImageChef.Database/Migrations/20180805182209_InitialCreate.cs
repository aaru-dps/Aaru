// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : 20180805163101_InitialCreate.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Initial database status.
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
using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscImageChef.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("CHS",
                                         table => new
                                         {
                                             Id = table.Column<ulong>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             Cylinders = table.Column<ushort>(nullable: false),
                                             Heads     = table.Column<ushort>(nullable: false),
                                             Sectors   = table.Column<ushort>(nullable: false)
                                         }, constraints: table => { table.PrimaryKey("PK_CHS", x => x.Id); });

            migrationBuilder.CreateTable("Features",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             AACSVersion                     = table.Column<byte>(nullable: true),
                                             AGIDs                           = table.Column<byte>(nullable: true),
                                             BindingNonceBlocks              = table.Column<byte>(nullable: true),
                                             BlocksPerReadableUnit           = table.Column<ushort>(nullable: true),
                                             BufferUnderrunFreeInDVD         = table.Column<bool>(nullable: false),
                                             BufferUnderrunFreeInSAO         = table.Column<bool>(nullable: false),
                                             BufferUnderrunFreeInTAO         = table.Column<bool>(nullable: false),
                                             CanAudioScan                    = table.Column<bool>(nullable: false),
                                             CanEject                        = table.Column<bool>(nullable: false),
                                             CanEraseSector                  = table.Column<bool>(nullable: false),
                                             CanExpandBDRESpareArea          = table.Column<bool>(nullable: false),
                                             CanFormat                       = table.Column<bool>(nullable: false),
                                             CanFormatBDREWithoutSpare       = table.Column<bool>(nullable: false),
                                             CanFormatCert                   = table.Column<bool>(nullable: false),
                                             CanFormatFRF                    = table.Column<bool>(nullable: false),
                                             CanFormatQCert                  = table.Column<bool>(nullable: false),
                                             CanFormatRRM                    = table.Column<bool>(nullable: false),
                                             CanGenerateBindingNonce         = table.Column<bool>(nullable: false),
                                             CanLoad                         = table.Column<bool>(nullable: false),
                                             CanMuteSeparateChannels         = table.Column<bool>(nullable: false),
                                             CanOverwriteSAOTrack            = table.Column<bool>(nullable: false),
                                             CanOverwriteTAOTrack            = table.Column<bool>(nullable: false),
                                             CanPlayCDAudio                  = table.Column<bool>(nullable: false),
                                             CanPseudoOverwriteBDR           = table.Column<bool>(nullable: false),
                                             CanReadAllDualR                 = table.Column<bool>(nullable: false),
                                             CanReadAllDualRW                = table.Column<bool>(nullable: false),
                                             CanReadBD                       = table.Column<bool>(nullable: false),
                                             CanReadBDR                      = table.Column<bool>(nullable: false),
                                             CanReadBDRE1                    = table.Column<bool>(nullable: false),
                                             CanReadBDRE2                    = table.Column<bool>(nullable: false),
                                             CanReadBDROM                    = table.Column<bool>(nullable: false),
                                             CanReadBluBCA                   = table.Column<bool>(nullable: false),
                                             CanReadCD                       = table.Column<bool>(nullable: false),
                                             CanReadCDMRW                    = table.Column<bool>(nullable: false),
                                             CanReadCPRM_MKB                 = table.Column<bool>(nullable: false),
                                             CanReadDDCD                     = table.Column<bool>(nullable: false),
                                             CanReadDVD                      = table.Column<bool>(nullable: false),
                                             CanReadDVDPlusMRW               = table.Column<bool>(nullable: false),
                                             CanReadDVDPlusR                 = table.Column<bool>(nullable: false),
                                             CanReadDVDPlusRDL               = table.Column<bool>(nullable: false),
                                             CanReadDVDPlusRW                = table.Column<bool>(nullable: false),
                                             CanReadDVDPlusRWDL              = table.Column<bool>(nullable: false),
                                             CanReadDriveAACSCertificate     = table.Column<bool>(nullable: false),
                                             CanReadHDDVD                    = table.Column<bool>(nullable: false),
                                             CanReadHDDVDR                   = table.Column<bool>(nullable: false),
                                             CanReadHDDVDRAM                 = table.Column<bool>(nullable: false),
                                             CanReadLeadInCDText             = table.Column<bool>(nullable: false),
                                             CanReadOldBDR                   = table.Column<bool>(nullable: false),
                                             CanReadOldBDRE                  = table.Column<bool>(nullable: false),
                                             CanReadOldBDROM                 = table.Column<bool>(nullable: false),
                                             CanReadSpareAreaInformation     = table.Column<bool>(nullable: false),
                                             CanReportDriveSerial            = table.Column<bool>(nullable: false),
                                             CanReportMediaSerial            = table.Column<bool>(nullable: false),
                                             CanTestWriteDDCDR               = table.Column<bool>(nullable: false),
                                             CanTestWriteDVD                 = table.Column<bool>(nullable: false),
                                             CanTestWriteInSAO               = table.Column<bool>(nullable: false),
                                             CanTestWriteInTAO               = table.Column<bool>(nullable: false),
                                             CanUpgradeFirmware              = table.Column<bool>(nullable: false),
                                             CanWriteBD                      = table.Column<bool>(nullable: false),
                                             CanWriteBDR                     = table.Column<bool>(nullable: false),
                                             CanWriteBDRE1                   = table.Column<bool>(nullable: false),
                                             CanWriteBDRE2                   = table.Column<bool>(nullable: false),
                                             CanWriteBusEncryptedBlocks      = table.Column<bool>(nullable: false),
                                             CanWriteCDMRW                   = table.Column<bool>(nullable: false),
                                             CanWriteCDRW                    = table.Column<bool>(nullable: false),
                                             CanWriteCDRWCAV                 = table.Column<bool>(nullable: false),
                                             CanWriteCDSAO                   = table.Column<bool>(nullable: false),
                                             CanWriteCDTAO                   = table.Column<bool>(nullable: false),
                                             CanWriteCSSManagedDVD           = table.Column<bool>(nullable: false),
                                             CanWriteDDCDR                   = table.Column<bool>(nullable: false),
                                             CanWriteDDCDRW                  = table.Column<bool>(nullable: false),
                                             CanWriteDVDPlusMRW              = table.Column<bool>(nullable: false),
                                             CanWriteDVDPlusR                = table.Column<bool>(nullable: false),
                                             CanWriteDVDPlusRDL              = table.Column<bool>(nullable: false),
                                             CanWriteDVDPlusRW               = table.Column<bool>(nullable: false),
                                             CanWriteDVDPlusRWDL             = table.Column<bool>(nullable: false),
                                             CanWriteDVDR                    = table.Column<bool>(nullable: false),
                                             CanWriteDVDRDL                  = table.Column<bool>(nullable: false),
                                             CanWriteDVDRW                   = table.Column<bool>(nullable: false),
                                             CanWriteHDDVDR                  = table.Column<bool>(nullable: false),
                                             CanWriteHDDVDRAM                = table.Column<bool>(nullable: false),
                                             CanWriteOldBDR                  = table.Column<bool>(nullable: false),
                                             CanWriteOldBDRE                 = table.Column<bool>(nullable: false),
                                             CanWritePackedSubchannelInTAO   = table.Column<bool>(nullable: false),
                                             CanWriteRWSubchannelInSAO       = table.Column<bool>(nullable: false),
                                             CanWriteRWSubchannelInTAO       = table.Column<bool>(nullable: false),
                                             CanWriteRaw                     = table.Column<bool>(nullable: false),
                                             CanWriteRawMultiSession         = table.Column<bool>(nullable: false),
                                             CanWriteRawSubchannelInTAO      = table.Column<bool>(nullable: false),
                                             ChangerIsSideChangeCapable      = table.Column<bool>(nullable: false),
                                             ChangerSlots                    = table.Column<byte>(nullable: false),
                                             ChangerSupportsDiscPresent      = table.Column<bool>(nullable: true),
                                             CPRMVersion                     = table.Column<byte>(nullable: true),
                                             CSSVersion                      = table.Column<byte>(nullable: true),
                                             DBML                            = table.Column<bool>(nullable: false),
                                             DVDMultiRead                    = table.Column<bool>(nullable: false),
                                             EmbeddedChanger                 = table.Column<bool>(nullable: false),
                                             ErrorRecoveryPage               = table.Column<bool>(nullable: false),
                                             FirmwareDate                    = table.Column<DateTime>(nullable: true),
                                             LoadingMechanismType            = table.Column<byte>(nullable: true),
                                             Locked                          = table.Column<bool>(nullable: false),
                                             LogicalBlockSize                = table.Column<uint>(nullable: true),
                                             MultiRead                       = table.Column<bool>(nullable: false),
                                             PhysicalInterfaceStandard       = table.Column<uint>(nullable: true),
                                             PhysicalInterfaceStandardNumber = table.Column<uint>(nullable: true),
                                             PreventJumper                   = table.Column<bool>(nullable: false),
                                             SupportsAACS                    = table.Column<bool>(nullable: false),
                                             SupportsBusEncryption           = table.Column<bool>(nullable: false),
                                             SupportsC2                      = table.Column<bool>(nullable: false),
                                             SupportsCPRM                    = table.Column<bool>(nullable: false),
                                             SupportsCSS                     = table.Column<bool>(nullable: false),
                                             SupportsDAP                     = table.Column<bool>(nullable: false),
                                             SupportsDeviceBusyEvent         = table.Column<bool>(nullable: false),
                                             SupportsHybridDiscs             = table.Column<bool>(nullable: false),
                                             SupportsModePage1Ch             = table.Column<bool>(nullable: false),
                                             SupportsOSSC                    = table.Column<bool>(nullable: false),
                                             SupportsPWP                     = table.Column<bool>(nullable: false),
                                             SupportsSWPP                    = table.Column<bool>(nullable: false),
                                             SupportsSecurDisc               = table.Column<bool>(nullable: false),
                                             SupportsSeparateVolume          = table.Column<bool>(nullable: false),
                                             SupportsVCPS                    = table.Column<bool>(nullable: false),
                                             SupportsWriteInhibitDCB         = table.Column<bool>(nullable: false),
                                             SupportsWriteProtectPAC         = table.Column<bool>(nullable: false),
                                             VolumeLevels                    = table.Column<ushort>(nullable: true)
                                         }, constraints: table => { table.PrimaryKey("PK_Features", x => x.Id); });

            migrationBuilder.CreateTable("FireWire",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             VendorID       = table.Column<uint>(nullable: false),
                                             ProductID      = table.Column<uint>(nullable: false),
                                             Manufacturer   = table.Column<string>(nullable: true),
                                             Product        = table.Column<string>(nullable: true),
                                             RemovableMedia = table.Column<bool>(nullable: false)
                                         }, constraints: table => { table.PrimaryKey("PK_FireWire", x => x.Id); });

            migrationBuilder.CreateTable("Inquiry",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             AccessControlCoordinator = table.Column<bool>(nullable: false),
                                             ACKRequests              = table.Column<bool>(nullable: false),
                                             AERCSupported            = table.Column<bool>(nullable: false),
                                             Address16                = table.Column<bool>(nullable: false),
                                             Address32                = table.Column<bool>(nullable: false),
                                             ANSIVersion              = table.Column<byte>(nullable: true),
                                             AsymmetricalLUNAccess    = table.Column<byte>(nullable: false),
                                             BasicQueueing            = table.Column<bool>(nullable: false),
                                             DeviceTypeModifier       = table.Column<byte>(nullable: true),
                                             ECMAVersion              = table.Column<byte>(nullable: true),
                                             EnclosureServices        = table.Column<bool>(nullable: false),
                                             HierarchicalLUN          = table.Column<bool>(nullable: false),
                                             IUS                      = table.Column<bool>(nullable: false),
                                             ISOVersion               = table.Column<byte>(nullable: true),
                                             LinkedCommands           = table.Column<bool>(nullable: false),
                                             MediumChanger            = table.Column<bool>(nullable: false),
                                             MultiPortDevice          = table.Column<bool>(nullable: false),
                                             NormalACA                = table.Column<bool>(nullable: false),
                                             PeripheralDeviceType     = table.Column<byte>(nullable: false),
                                             PeripheralQualifier      = table.Column<byte>(nullable: false),
                                             ProductIdentification    = table.Column<string>(nullable: true),
                                             ProductRevisionLevel     = table.Column<string>(nullable: true),
                                             Protection               = table.Column<bool>(nullable: false),
                                             QAS                      = table.Column<bool>(nullable: false),
                                             RelativeAddressing       = table.Column<bool>(nullable: false),
                                             Removable                = table.Column<bool>(nullable: false),
                                             ResponseDataFormat       = table.Column<byte>(nullable: true),
                                             TaggedCommandQueue       = table.Column<bool>(nullable: false),
                                             TerminateTaskSupported   = table.Column<bool>(nullable: false),
                                             ThirdPartyCopy           = table.Column<bool>(nullable: false),
                                             TranferDisable           = table.Column<bool>(nullable: false),
                                             SoftReset                = table.Column<bool>(nullable: false),
                                             SPIClocking              = table.Column<byte>(nullable: false),
                                             StorageArrayController   = table.Column<bool>(nullable: false),
                                             SyncTransfer             = table.Column<bool>(nullable: false),
                                             VendorIdentification     = table.Column<string>(nullable: true),
                                             WideBus16                = table.Column<bool>(nullable: false),
                                             WideBus32                = table.Column<bool>(nullable: false),
                                             Data                     = table.Column<byte[]>(nullable: true)
                                         }, constraints: table => { table.PrimaryKey("PK_Inquiry", x => x.Id); });

            migrationBuilder.CreateTable("Mode",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             MediumType        = table.Column<byte>(nullable: true),
                                             WriteProtected    = table.Column<bool>(nullable: false),
                                             Speed             = table.Column<byte>(nullable: true),
                                             BufferedMode      = table.Column<byte>(nullable: true),
                                             BlankCheckEnabled = table.Column<bool>(nullable: false),
                                             DPOandFUA         = table.Column<bool>(nullable: false)
                                         }, constraints: table => { table.PrimaryKey("PK_Mode", x => x.Id); });

            migrationBuilder.CreateTable("Mode2A",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             AccurateCDDA                = table.Column<bool>(nullable: false),
                                             BCK                         = table.Column<bool>(nullable: false),
                                             BufferSize                  = table.Column<ushort>(nullable: false),
                                             BufferUnderRunProtection    = table.Column<bool>(nullable: true),
                                             CanEject                    = table.Column<bool>(nullable: false),
                                             CanLockMedia                = table.Column<bool>(nullable: false),
                                             CDDACommand                 = table.Column<bool>(nullable: false),
                                             CompositeAudioVideo         = table.Column<bool>(nullable: false),
                                             CSSandCPPMSupported         = table.Column<bool>(nullable: false),
                                             CurrentSpeed                = table.Column<ushort>(nullable: true),
                                             CurrentWriteSpeed           = table.Column<ushort>(nullable: true),
                                             CurrentWriteSpeedSelected   = table.Column<ushort>(nullable: true),
                                             DeterministicSlotChanger    = table.Column<bool>(nullable: false),
                                             DigitalPort1                = table.Column<bool>(nullable: false),
                                             DigitalPort2                = table.Column<bool>(nullable: false),
                                             LeadInPW                    = table.Column<bool>(nullable: false),
                                             LoadingMechanismType        = table.Column<byte>(nullable: false),
                                             LockStatus                  = table.Column<bool>(nullable: false),
                                             LSBF                        = table.Column<bool>(nullable: false),
                                             MaximumSpeed                = table.Column<ushort>(nullable: true),
                                             MaximumWriteSpeed           = table.Column<ushort>(nullable: true),
                                             PlaysAudio                  = table.Column<bool>(nullable: false),
                                             PreventJumperStatus         = table.Column<bool>(nullable: false),
                                             RCK                         = table.Column<bool>(nullable: false),
                                             ReadsBarcode                = table.Column<bool>(nullable: false),
                                             ReadsBothSides              = table.Column<bool>(nullable: false),
                                             ReadsCDR                    = table.Column<bool>(nullable: false),
                                             ReadsCDRW                   = table.Column<bool>(nullable: false),
                                             ReadsDeinterlavedSubchannel = table.Column<bool>(nullable: false),
                                             ReadsDVDR                   = table.Column<bool>(nullable: false),
                                             ReadsDVDRAM                 = table.Column<bool>(nullable: false),
                                             ReadsDVDROM                 = table.Column<bool>(nullable: false),
                                             ReadsISRC                   = table.Column<bool>(nullable: false),
                                             ReadsMode2Form2             = table.Column<bool>(nullable: false),
                                             ReadsMode2Form1             = table.Column<bool>(nullable: false),
                                             ReadsPacketCDR              = table.Column<bool>(nullable: false),
                                             ReadsSubchannel             = table.Column<bool>(nullable: false),
                                             ReadsUPC                    = table.Column<bool>(nullable: false),
                                             ReturnsC2Pointers           = table.Column<bool>(nullable: false),
                                             RotationControlSelected     = table.Column<byte>(nullable: true),
                                             SeparateChannelMute         = table.Column<bool>(nullable: false),
                                             SeparateChannelVolume       = table.Column<bool>(nullable: false),
                                             SSS                         = table.Column<bool>(nullable: false),
                                             SupportsMultiSession        = table.Column<bool>(nullable: false),
                                             SupportedVolumeLevels       = table.Column<ushort>(nullable: true),
                                             TestWrite                   = table.Column<bool>(nullable: false),
                                             WritesCDR                   = table.Column<bool>(nullable: false),
                                             WritesCDRW                  = table.Column<bool>(nullable: false),
                                             WritesDVDR                  = table.Column<bool>(nullable: false),
                                             WritesDVDRAM                = table.Column<bool>(nullable: false)
                                         }, constraints: table => { table.PrimaryKey("PK_Mode2A", x => x.Id); });

            migrationBuilder.CreateTable("PCMCIA",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             CIS              = table.Column<byte[]>(nullable: true),
                                             Compliance       = table.Column<string>(nullable: true),
                                             ManufacturerCode = table.Column<ushort>(nullable: false),
                                             CardCode         = table.Column<ushort>(nullable: false),
                                             Manufacturer     = table.Column<string>(nullable: true),
                                             ProductName      = table.Column<string>(nullable: true)
                                         }, constraints: table => { table.PrimaryKey("PK_PCMCIA", x => x.Id); });

            migrationBuilder.CreateTable("SecureDigital",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             CID         = table.Column<byte[]>(nullable: true),
                                             CSD         = table.Column<byte[]>(nullable: true),
                                             OCR         = table.Column<byte[]>(nullable: true),
                                             SCR         = table.Column<byte[]>(nullable: true),
                                             ExtendedCSD = table.Column<byte[]>(nullable: true)
                                         }, constraints: table => { table.PrimaryKey("PK_SecureDigital", x => x.Id); });

            migrationBuilder.CreateTable("SSC",
                                         table => new
                                         {
                                             Id = table.Column<ulong>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             BlockSizeGranularity = table.Column<byte>(nullable: true),
                                             MaxBlockLength       = table.Column<uint>(nullable: true),
                                             MinBlockLength       = table.Column<uint>(nullable: true)
                                         }, constraints: table => { table.PrimaryKey("PK_SSC", x => x.Id); });

            migrationBuilder.CreateTable("USB",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             VendorID       = table.Column<ushort>(nullable: false),
                                             ProductID      = table.Column<ushort>(nullable: false),
                                             Manufacturer   = table.Column<string>(nullable: true),
                                             Product        = table.Column<string>(nullable: true),
                                             RemovableMedia = table.Column<bool>(nullable: false),
                                             Descriptors    = table.Column<byte[]>(nullable: true)
                                         }, constraints: table => { table.PrimaryKey("PK_USB", x => x.Id); });

            migrationBuilder.CreateTable("UshortClass",
                                         table => new
                                         {
                                             Id = table.Column<ulong>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             Value     = table.Column<ushort>(nullable: false),
                                             InquiryId = table.Column<ulong>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_UshortClass", x => x.Id);
                                             table.ForeignKey("FK_UshortClass_Inquiry_InquiryId", x => x.InquiryId,
                                                              "Inquiry", "Id", onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("BlockDescriptor",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             Density     = table.Column<byte>(nullable: false),
                                             Blocks      = table.Column<ulong>(nullable: true),
                                             BlockLength = table.Column<uint>(nullable: true),
                                             ModeId      = table.Column<ulong>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_BlockDescriptor", x => x.Id);
                                             table.ForeignKey("FK_BlockDescriptor_Mode_ModeId", x => x.ModeId, "Mode",
                                                              "Id", onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("ModePage",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             page    = table.Column<byte>(nullable: false),
                                             subpage = table.Column<byte>(nullable: false),
                                             value   = table.Column<byte[]>(nullable: true),
                                             ModeId  = table.Column<ulong>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_ModePage", x => x.Id);
                                             table.ForeignKey("FK_ModePage_Mode_ModeId", x => x.ModeId, "Mode", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("MMC",
                                         table => new
                                         {
                                             Id = table.Column<ulong>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             ModeSense2AId = table.Column<ulong>(nullable: true),
                                             FeaturesId    = table.Column<ulong>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_MMC", x => x.Id);
                                             table.ForeignKey("FK_MMC_Features_FeaturesId", x => x.FeaturesId,
                                                              "Features", "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_MMC_Mode2A_ModeSense2AId", x => x.ModeSense2AId,
                                                              "Mode2A", "Id", onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("WriteDescriptor",
                                         table => new
                                         {
                                             Id = table.Column<ulong>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             RotationControl = table.Column<byte>(nullable: false),
                                             WriteSpeed      = table.Column<ushort>(nullable: false),
                                             Mode2AId        = table.Column<ulong>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_WriteDescriptor", x => x.Id);
                                             table.ForeignKey("FK_WriteDescriptor_Mode2A_Mode2AId", x => x.Mode2AId,
                                                              "Mode2A", "Id", onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("StringClass",
                                         table => new
                                         {
                                             Id = table.Column<ulong>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             Value    = table.Column<string>(nullable: false),
                                             PCMCIAId = table.Column<ulong>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_StringClass", x => x.Id);
                                             table.ForeignKey("FK_StringClass_PCMCIA_PCMCIAId", x => x.PCMCIAId,
                                                              "PCMCIA", "Id", onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("SequentialMedia",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             CanReadMediaSerial = table.Column<bool>(nullable: true),
                                             Density            = table.Column<byte>(nullable: true),
                                             Manufacturer       = table.Column<string>(nullable: true),
                                             MediaIsRecognized  = table.Column<bool>(nullable: false),
                                             MediumType         = table.Column<byte>(nullable: true),
                                             MediumTypeName     = table.Column<string>(nullable: true),
                                             Model              = table.Column<string>(nullable: true),
                                             ModeSense6Data     = table.Column<byte[]>(nullable: true),
                                             ModeSense10Data    = table.Column<byte[]>(nullable: true),
                                             SSCId              = table.Column<ulong>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_SequentialMedia", x => x.Id);
                                             table.ForeignKey("FK_SequentialMedia_SSC_SSCId", x => x.SSCId, "SSC", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("SupportedDensity",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             PrimaryCode       = table.Column<byte>(nullable: false),
                                             SecondaryCode     = table.Column<byte>(nullable: false),
                                             Writable          = table.Column<bool>(nullable: false),
                                             Duplicate         = table.Column<bool>(nullable: false),
                                             DefaultDensity    = table.Column<bool>(nullable: false),
                                             BitsPerMm         = table.Column<uint>(nullable: false),
                                             Width             = table.Column<ushort>(nullable: false),
                                             Tracks            = table.Column<ushort>(nullable: false),
                                             Capacity          = table.Column<uint>(nullable: false),
                                             Organization      = table.Column<string>(nullable: true),
                                             Name              = table.Column<string>(nullable: true),
                                             Description       = table.Column<string>(nullable: true),
                                             SSCId             = table.Column<ulong>(nullable: true),
                                             SequentialMediaId = table.Column<ulong>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_SupportedDensity", x => x.Id);
                                             table.ForeignKey("FK_SupportedDensity_SSC_SSCId", x => x.SSCId, "SSC",
                                                              "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_SupportedDensity_SequentialMedia_SequentialMediaId",
                                                              x => x.SequentialMediaId, "SequentialMedia", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("SupportedMedia",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             MediumType        = table.Column<byte>(nullable: false),
                                             Width             = table.Column<ushort>(nullable: false),
                                             Length            = table.Column<ushort>(nullable: false),
                                             Organization      = table.Column<string>(nullable: true),
                                             Name              = table.Column<string>(nullable: true),
                                             Description       = table.Column<string>(nullable: true),
                                             SSCId             = table.Column<ulong>(nullable: true),
                                             SequentialMediaId = table.Column<ulong>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_SupportedMedia", x => x.Id);
                                             table.ForeignKey("FK_SupportedMedia_SSC_SSCId", x => x.SSCId, "SSC", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_SupportedMedia_SequentialMedia_SequentialMediaId",
                                                              x => x.SequentialMediaId, "SequentialMedia", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("IntClass",
                                         table => new
                                         {
                                             Id = table.Column<ulong>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             Value            = table.Column<int>(nullable: false),
                                             SupportedMediaId = table.Column<ulong>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_IntClass", x => x.Id);
                                             table.ForeignKey("FK_IntClass_SupportedMedia_SupportedMediaId",
                                                              x => x.SupportedMediaId, "SupportedMedia", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("Device",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             WhenAdded        = table.Column<DateTime>(nullable: false),
                                             WhenModified     = table.Column<DateTime>(nullable: true),
                                             Manufacturer     = table.Column<string>(nullable: true),
                                             Model            = table.Column<string>(nullable: true),
                                             Revision         = table.Column<string>(nullable: true),
                                             Type             = table.Column<int>(nullable: false),
                                             USBId            = table.Column<ulong>(nullable: true),
                                             FireWireId       = table.Column<ulong>(nullable: true),
                                             PCMCIAId         = table.Column<ulong>(nullable: true),
                                             ATAId            = table.Column<ulong>(nullable: true),
                                             ATAPIId          = table.Column<ulong>(nullable: true),
                                             SCSIId           = table.Column<ulong>(nullable: true),
                                             MultiMediaCardId = table.Column<ulong>(nullable: true),
                                             SecureDigitalId  = table.Column<ulong>(nullable: true),
                                             IsValid          = table.Column<bool>(nullable: false),
                                             TimesSeen        = table.Column<ulong>(nullable: false)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_Device", x => x.Id);
                                             table.ForeignKey("FK_Device_FireWire_FireWireId", x => x.FireWireId,
                                                              "FireWire", "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Device_SecureDigital_MultiMediaCardId",
                                                              x => x.MultiMediaCardId, "SecureDigital", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Device_PCMCIA_PCMCIAId", x => x.PCMCIAId, "PCMCIA",
                                                              "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Device_SecureDigital_SecureDigitalId",
                                                              x => x.SecureDigitalId, "SecureDigital", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Device_USB_USBId", x => x.USBId, "USB", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("TestedMedia",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             Blocks                           = table.Column<ulong>(nullable: true),
                                             BlockSize                        = table.Column<uint>(nullable: true),
                                             CanReadAACS                      = table.Column<bool>(nullable: true),
                                             CanReadADIP                      = table.Column<bool>(nullable: true),
                                             CanReadATIP                      = table.Column<bool>(nullable: true),
                                             CanReadBCA                       = table.Column<bool>(nullable: true),
                                             CanReadC2Pointers                = table.Column<bool>(nullable: true),
                                             CanReadCMI                       = table.Column<bool>(nullable: true),
                                             CanReadCorrectedSubchannel       = table.Column<bool>(nullable: true),
                                             CanReadCorrectedSubchannelWithC2 = table.Column<bool>(nullable: true),
                                             CanReadDCB                       = table.Column<bool>(nullable: true),
                                             CanReadDDS                       = table.Column<bool>(nullable: true),
                                             CanReadDMI                       = table.Column<bool>(nullable: true),
                                             CanReadDiscInformation           = table.Column<bool>(nullable: true),
                                             CanReadFullTOC                   = table.Column<bool>(nullable: true),
                                             CanReadHDCMI                     = table.Column<bool>(nullable: true),
                                             CanReadLayerCapacity             = table.Column<bool>(nullable: true),
                                             CanReadLeadIn                    = table.Column<bool>(nullable: true),
                                             CanReadLeadInPostgap             = table.Column<bool>(nullable: true),
                                             CanReadLeadOut                   = table.Column<bool>(nullable: true),
                                             CanReadMediaID                   = table.Column<bool>(nullable: true),
                                             CanReadMediaSerial               = table.Column<bool>(nullable: true),
                                             CanReadPAC                       = table.Column<bool>(nullable: true),
                                             CanReadPFI                       = table.Column<bool>(nullable: true),
                                             CanReadPMA                       = table.Column<bool>(nullable: true),
                                             CanReadPQSubchannel              = table.Column<bool>(nullable: true),
                                             CanReadPQSubchannelWithC2        = table.Column<bool>(nullable: true),
                                             CanReadPRI                       = table.Column<bool>(nullable: true),
                                             CanReadRWSubchannel              = table.Column<bool>(nullable: true),
                                             CanReadRWSubchannelWithC2        = table.Column<bool>(nullable: true),
                                             CanReadRecordablePFI             = table.Column<bool>(nullable: true),
                                             CanReadSpareAreaInformation      = table.Column<bool>(nullable: true),
                                             CanReadTOC                       = table.Column<bool>(nullable: true),
                                             Density                          = table.Column<byte>(nullable: true),
                                             LongBlockSize                    = table.Column<uint>(nullable: true),
                                             Manufacturer                     = table.Column<string>(nullable: true),
                                             MediaIsRecognized                = table.Column<bool>(nullable: false),
                                             MediumType                       = table.Column<byte>(nullable: true),
                                             MediumTypeName                   = table.Column<string>(nullable: true),
                                             Model                            = table.Column<string>(nullable: true),
                                             SupportsHLDTSTReadRawDVD         = table.Column<bool>(nullable: true),
                                             SupportsNECReadCDDA              = table.Column<bool>(nullable: true),
                                             SupportsPioneerReadCDDA          = table.Column<bool>(nullable: true),
                                             SupportsPioneerReadCDDAMSF       = table.Column<bool>(nullable: true),
                                             SupportsPlextorReadCDDA          = table.Column<bool>(nullable: true),
                                             SupportsPlextorReadRawDVD        = table.Column<bool>(nullable: true),
                                             SupportsRead10                   = table.Column<bool>(nullable: true),
                                             SupportsRead12                   = table.Column<bool>(nullable: true),
                                             SupportsRead16                   = table.Column<bool>(nullable: true),
                                             SupportsRead                     = table.Column<bool>(nullable: true),
                                             SupportsReadCapacity16           = table.Column<bool>(nullable: true),
                                             SupportsReadCapacity             = table.Column<bool>(nullable: true),
                                             SupportsReadCd                   = table.Column<bool>(nullable: true),
                                             SupportsReadCdMsf                = table.Column<bool>(nullable: true),
                                             SupportsReadCdRaw                = table.Column<bool>(nullable: true),
                                             SupportsReadCdMsfRaw             = table.Column<bool>(nullable: true),
                                             SupportsReadLong16               = table.Column<bool>(nullable: true),
                                             SupportsReadLong                 = table.Column<bool>(nullable: true),
                                             ModeSense6Data                   = table.Column<byte[]>(nullable: true),
                                             ModeSense10Data                  = table.Column<byte[]>(nullable: true),
                                             CHSId                            = table.Column<ulong>(nullable: true),
                                             CurrentCHSId                     = table.Column<ulong>(nullable: true),
                                             LBASectors                       = table.Column<uint>(nullable: true),
                                             LBA48Sectors                     = table.Column<ulong>(nullable: true),
                                             LogicalAlignment                 = table.Column<ushort>(nullable: true),
                                             NominalRotationRate              = table.Column<ushort>(nullable: true),
                                             PhysicalBlockSize                = table.Column<uint>(nullable: true),
                                             SolidStateDevice                 = table.Column<bool>(nullable: true),
                                             UnformattedBPT                   = table.Column<ushort>(nullable: true),
                                             UnformattedBPS                   = table.Column<ushort>(nullable: true),
                                             SupportsReadDmaLba               = table.Column<bool>(nullable: true),
                                             SupportsReadDmaRetryLba          = table.Column<bool>(nullable: true),
                                             SupportsReadLba                  = table.Column<bool>(nullable: true),
                                             SupportsReadRetryLba             = table.Column<bool>(nullable: true),
                                             SupportsReadLongLba              = table.Column<bool>(nullable: true),
                                             SupportsReadLongRetryLba         = table.Column<bool>(nullable: true),
                                             SupportsSeekLba                  = table.Column<bool>(nullable: true),
                                             SupportsReadDmaLba48             = table.Column<bool>(nullable: true),
                                             SupportsReadLba48                = table.Column<bool>(nullable: true),
                                             SupportsReadDma                  = table.Column<bool>(nullable: true),
                                             SupportsReadDmaRetry             = table.Column<bool>(nullable: true),
                                             SupportsReadRetry                = table.Column<bool>(nullable: true),
                                             SupportsReadLongRetry            = table.Column<bool>(nullable: true),
                                             SupportsSeek                     = table.Column<bool>(nullable: true),
                                             ATAId                            = table.Column<ulong>(nullable: true),
                                             MMCId                            = table.Column<ulong>(nullable: true),
                                             SCSIId                           = table.Column<ulong>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_TestedMedia", x => x.Id);
                                             table.ForeignKey("FK_TestedMedia_CHS_CHSId", x => x.CHSId, "CHS", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_TestedMedia_CHS_CurrentCHSId", x => x.CurrentCHSId,
                                                              "CHS", "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_TestedMedia_MMC_MMCId", x => x.MMCId, "MMC", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("ATA",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             AdditionalPID                = table.Column<string>(nullable: true),
                                             APIOSupported                = table.Column<byte>(nullable: true),
                                             ATAPIByteCount               = table.Column<ushort>(nullable: true),
                                             BufferType                   = table.Column<ushort>(nullable: true),
                                             BufferSize                   = table.Column<ushort>(nullable: true),
                                             Capabilities                 = table.Column<ushort>(nullable: true),
                                             Capabilities2                = table.Column<ushort>(nullable: true),
                                             Capabilities3                = table.Column<byte>(nullable: true),
                                             CFAPowerMode                 = table.Column<ushort>(nullable: true),
                                             CommandSet                   = table.Column<ushort>(nullable: true),
                                             CommandSet2                  = table.Column<ushort>(nullable: true),
                                             CommandSet3                  = table.Column<ushort>(nullable: true),
                                             CommandSet4                  = table.Column<ushort>(nullable: true),
                                             CommandSet5                  = table.Column<ushort>(nullable: true),
                                             CurrentAAM                   = table.Column<byte>(nullable: true),
                                             CurrentAPM                   = table.Column<ushort>(nullable: true),
                                             DataSetMgmt                  = table.Column<ushort>(nullable: true),
                                             DataSetMgmtSize              = table.Column<ushort>(nullable: true),
                                             DeviceFormFactor             = table.Column<ushort>(nullable: true),
                                             DMAActive                    = table.Column<byte>(nullable: true),
                                             DMASupported                 = table.Column<byte>(nullable: true),
                                             DMATransferTimingMode        = table.Column<byte>(nullable: true),
                                             EnhancedSecurityEraseTime    = table.Column<ushort>(nullable: true),
                                             EnabledCommandSet            = table.Column<ushort>(nullable: true),
                                             EnabledCommandSet2           = table.Column<ushort>(nullable: true),
                                             EnabledCommandSet3           = table.Column<ushort>(nullable: true),
                                             EnabledCommandSet4           = table.Column<ushort>(nullable: true),
                                             EnabledSATAFeatures          = table.Column<ushort>(nullable: true),
                                             ExtendedUserSectors          = table.Column<ulong>(nullable: true),
                                             FreeFallSensitivity          = table.Column<byte>(nullable: true),
                                             FirmwareRevision             = table.Column<string>(nullable: true),
                                             GeneralConfiguration         = table.Column<ushort>(nullable: true),
                                             HardwareResetResult          = table.Column<ushort>(nullable: true),
                                             InterseekDelay               = table.Column<ushort>(nullable: true),
                                             MajorVersion                 = table.Column<ushort>(nullable: true),
                                             MasterPasswordRevisionCode   = table.Column<ushort>(nullable: true),
                                             MaxDownloadMicroMode3        = table.Column<ushort>(nullable: true),
                                             MaxQueueDepth                = table.Column<ushort>(nullable: true),
                                             MDMAActive                   = table.Column<byte>(nullable: true),
                                             MDMASupported                = table.Column<byte>(nullable: true),
                                             MinDownloadMicroMode3        = table.Column<ushort>(nullable: true),
                                             MinMDMACycleTime             = table.Column<ushort>(nullable: true),
                                             MinorVersion                 = table.Column<ushort>(nullable: true),
                                             MinPIOCycleTimeNoFlow        = table.Column<ushort>(nullable: true),
                                             MinPIOCycleTimeFlow          = table.Column<ushort>(nullable: true),
                                             Model                        = table.Column<string>(nullable: true),
                                             MultipleMaxSectors           = table.Column<byte>(nullable: true),
                                             MultipleSectorNumber         = table.Column<byte>(nullable: true),
                                             NVCacheCaps                  = table.Column<ushort>(nullable: true),
                                             NVCacheSize                  = table.Column<uint>(nullable: true),
                                             NVCacheWriteSpeed            = table.Column<ushort>(nullable: true),
                                             NVEstimatedSpinUp            = table.Column<byte>(nullable: true),
                                             PacketBusRelease             = table.Column<ushort>(nullable: true),
                                             PIOTransferTimingMode        = table.Column<byte>(nullable: true),
                                             RecommendedAAM               = table.Column<byte>(nullable: true),
                                             RecommendedMDMACycleTime     = table.Column<ushort>(nullable: true),
                                             RemovableStatusSet           = table.Column<ushort>(nullable: true),
                                             SATACapabilities             = table.Column<ushort>(nullable: true),
                                             SATACapabilities2            = table.Column<ushort>(nullable: true),
                                             SATAFeatures                 = table.Column<ushort>(nullable: true),
                                             SCTCommandTransport          = table.Column<ushort>(nullable: true),
                                             SectorsPerCard               = table.Column<uint>(nullable: true),
                                             SecurityEraseTime            = table.Column<ushort>(nullable: true),
                                             SecurityStatus               = table.Column<ushort>(nullable: true),
                                             ServiceBusyClear             = table.Column<ushort>(nullable: true),
                                             SpecificConfiguration        = table.Column<ushort>(nullable: true),
                                             StreamAccessLatency          = table.Column<ushort>(nullable: true),
                                             StreamMinReqSize             = table.Column<ushort>(nullable: true),
                                             StreamPerformanceGranularity = table.Column<uint>(nullable: true),
                                             StreamTransferTimeDMA        = table.Column<ushort>(nullable: true),
                                             StreamTransferTimePIO        = table.Column<ushort>(nullable: true),
                                             TransportMajorVersion        = table.Column<ushort>(nullable: true),
                                             TransportMinorVersion        = table.Column<ushort>(nullable: true),
                                             TrustedComputing             = table.Column<ushort>(nullable: true),
                                             UDMAActive                   = table.Column<byte>(nullable: true),
                                             UDMASupported                = table.Column<byte>(nullable: true),
                                             WRVMode                      = table.Column<byte>(nullable: true),
                                             WRVSectorCountMode3          = table.Column<uint>(nullable: true),
                                             WRVSectorCountMode2          = table.Column<uint>(nullable: true),
                                             Identify                     = table.Column<byte[]>(nullable: true),
                                             ReadCapabilitiesId           = table.Column<ulong>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_ATA", x => x.Id);
                                             table.ForeignKey("FK_ATA_TestedMedia_ReadCapabilitiesId",
                                                              x => x.ReadCapabilitiesId, "TestedMedia", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("SCSI",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<ulong>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             InquiryId            = table.Column<ulong>(nullable: true),
                                             SupportsModeSense6   = table.Column<bool>(nullable: false),
                                             SupportsModeSense10  = table.Column<bool>(nullable: false),
                                             SupportsModeSubpages = table.Column<bool>(nullable: false),
                                             ModeSenseId          = table.Column<ulong>(nullable: true),
                                             MultiMediaDeviceId   = table.Column<ulong>(nullable: true),
                                             ReadCapabilitiesId   = table.Column<ulong>(nullable: true),
                                             SequentialDeviceId   = table.Column<ulong>(nullable: true),
                                             ModeSense6Data       = table.Column<byte[]>(nullable: true),
                                             ModeSense10Data      = table.Column<byte[]>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_SCSI", x => x.Id);
                                             table.ForeignKey("FK_SCSI_Inquiry_InquiryId", x => x.InquiryId, "Inquiry",
                                                              "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_SCSI_Mode_ModeSenseId", x => x.ModeSenseId, "Mode",
                                                              "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_SCSI_MMC_MultiMediaDeviceId",
                                                              x => x.MultiMediaDeviceId, "MMC", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_SCSI_TestedMedia_ReadCapabilitiesId",
                                                              x => x.ReadCapabilitiesId, "TestedMedia", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_SCSI_SSC_SequentialDeviceId",
                                                              x => x.SequentialDeviceId, "SSC", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("Page",
                                         table => new
                                         {
                                             Id = table.Column<ulong>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             page   = table.Column<byte>(nullable: false),
                                             value  = table.Column<byte[]>(nullable: true),
                                             SCSIId = table.Column<ulong>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_Page", x => x.Id);
                                             table.ForeignKey("FK_Page_SCSI_SCSIId", x => x.SCSIId, "SCSI", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateIndex("IX_ATA_ReadCapabilitiesId", "ATA", "ReadCapabilitiesId");

            migrationBuilder.CreateIndex("IX_BlockDescriptor_ModeId", "BlockDescriptor", "ModeId");

            migrationBuilder.CreateIndex("IX_Device_ATAId", "Device", "ATAId");

            migrationBuilder.CreateIndex("IX_Device_ATAPIId", "Device", "ATAPIId");

            migrationBuilder.CreateIndex("IX_Device_FireWireId", "Device", "FireWireId");

            migrationBuilder.CreateIndex("IX_Device_MultiMediaCardId", "Device", "MultiMediaCardId");

            migrationBuilder.CreateIndex("IX_Device_PCMCIAId", "Device", "PCMCIAId");

            migrationBuilder.CreateIndex("IX_Device_SCSIId", "Device", "SCSIId");

            migrationBuilder.CreateIndex("IX_Device_SecureDigitalId", "Device", "SecureDigitalId");

            migrationBuilder.CreateIndex("IX_Device_USBId", "Device", "USBId");

            migrationBuilder.CreateIndex("IX_IntClass_SupportedMediaId", "IntClass", "SupportedMediaId");

            migrationBuilder.CreateIndex("IX_MMC_FeaturesId", "MMC", "FeaturesId");

            migrationBuilder.CreateIndex("IX_MMC_ModeSense2AId", "MMC", "ModeSense2AId");

            migrationBuilder.CreateIndex("IX_ModePage_ModeId", "ModePage", "ModeId");

            migrationBuilder.CreateIndex("IX_Page_SCSIId", "Page", "SCSIId");

            migrationBuilder.CreateIndex("IX_SCSI_InquiryId", "SCSI", "InquiryId");

            migrationBuilder.CreateIndex("IX_SCSI_ModeSenseId", "SCSI", "ModeSenseId");

            migrationBuilder.CreateIndex("IX_SCSI_MultiMediaDeviceId", "SCSI", "MultiMediaDeviceId");

            migrationBuilder.CreateIndex("IX_SCSI_ReadCapabilitiesId", "SCSI", "ReadCapabilitiesId");

            migrationBuilder.CreateIndex("IX_SCSI_SequentialDeviceId", "SCSI", "SequentialDeviceId");

            migrationBuilder.CreateIndex("IX_SequentialMedia_SSCId", "SequentialMedia", "SSCId");

            migrationBuilder.CreateIndex("IX_StringClass_PCMCIAId", "StringClass", "PCMCIAId");

            migrationBuilder.CreateIndex("IX_SupportedDensity_SSCId", "SupportedDensity", "SSCId");

            migrationBuilder.CreateIndex("IX_SupportedDensity_SequentialMediaId", "SupportedDensity",
                                         "SequentialMediaId");

            migrationBuilder.CreateIndex("IX_SupportedMedia_SSCId", "SupportedMedia", "SSCId");

            migrationBuilder.CreateIndex("IX_SupportedMedia_SequentialMediaId", "SupportedMedia", "SequentialMediaId");

            migrationBuilder.CreateIndex("IX_TestedMedia_ATAId", "TestedMedia", "ATAId");

            migrationBuilder.CreateIndex("IX_TestedMedia_CHSId", "TestedMedia", "CHSId");

            migrationBuilder.CreateIndex("IX_TestedMedia_CurrentCHSId", "TestedMedia", "CurrentCHSId");

            migrationBuilder.CreateIndex("IX_TestedMedia_MMCId", "TestedMedia", "MMCId");

            migrationBuilder.CreateIndex("IX_TestedMedia_SCSIId", "TestedMedia", "SCSIId");

            migrationBuilder.CreateIndex("IX_UshortClass_InquiryId", "UshortClass", "InquiryId");

            migrationBuilder.CreateIndex("IX_WriteDescriptor_Mode2AId", "WriteDescriptor", "Mode2AId");

            migrationBuilder.AddForeignKey("FK_Device_ATA_ATAId", "Device", "ATAId", "ATA", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Device_ATA_ATAPIId", "Device", "ATAPIId", "ATA", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Device_SCSI_SCSIId", "Device", "SCSIId", "SCSI", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_TestedMedia_ATA_ATAId", "TestedMedia", "ATAId", "ATA",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_TestedMedia_SCSI_SCSIId", "TestedMedia", "SCSIId", "SCSI",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_ATA_TestedMedia_ReadCapabilitiesId", "ATA");

            migrationBuilder.DropForeignKey("FK_SCSI_TestedMedia_ReadCapabilitiesId", "SCSI");

            migrationBuilder.DropTable("BlockDescriptor");

            migrationBuilder.DropTable("Device");

            migrationBuilder.DropTable("IntClass");

            migrationBuilder.DropTable("ModePage");

            migrationBuilder.DropTable("Page");

            migrationBuilder.DropTable("StringClass");

            migrationBuilder.DropTable("SupportedDensity");

            migrationBuilder.DropTable("UshortClass");

            migrationBuilder.DropTable("WriteDescriptor");

            migrationBuilder.DropTable("FireWire");

            migrationBuilder.DropTable("SecureDigital");

            migrationBuilder.DropTable("USB");

            migrationBuilder.DropTable("SupportedMedia");

            migrationBuilder.DropTable("PCMCIA");

            migrationBuilder.DropTable("SequentialMedia");

            migrationBuilder.DropTable("TestedMedia");

            migrationBuilder.DropTable("ATA");

            migrationBuilder.DropTable("CHS");

            migrationBuilder.DropTable("SCSI");

            migrationBuilder.DropTable("Inquiry");

            migrationBuilder.DropTable("Mode");

            migrationBuilder.DropTable("MMC");

            migrationBuilder.DropTable("SSC");

            migrationBuilder.DropTable("Features");

            migrationBuilder.DropTable("Mode2A");
        }
    }
}