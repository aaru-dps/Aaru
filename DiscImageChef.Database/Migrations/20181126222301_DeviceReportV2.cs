using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscImageChef.Database.Migrations
{
    public partial class DeviceReportV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Chs",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             Cylinders = table.Column<ushort>(nullable: false),
                                             Heads     = table.Column<ushort>(nullable: false),
                                             Sectors   = table.Column<ushort>(nullable: false)
                                         }, constraints: table => { table.PrimaryKey("PK_Chs", x => x.Id); });

            migrationBuilder.CreateTable("FireWire",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             VendorID       = table.Column<uint>(nullable: false),
                                             ProductID      = table.Column<uint>(nullable: false),
                                             Manufacturer   = table.Column<string>(nullable: true),
                                             Product        = table.Column<string>(nullable: true),
                                             RemovableMedia = table.Column<bool>(nullable: false)
                                         }, constraints: table => { table.PrimaryKey("PK_FireWire", x => x.Id); });

            migrationBuilder.CreateTable("MmcFeatures",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<int>(nullable: false)
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
                                             ChangerSupportsDiscPresent      = table.Column<bool>(nullable: false),
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
                                         },
                                         constraints: table => { table.PrimaryKey("PK_MmcFeatures", x => x.Id); });

            migrationBuilder.CreateTable("MmcSd",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             CID         = table.Column<byte[]>(nullable: true),
                                             CSD         = table.Column<byte[]>(nullable: true),
                                             OCR         = table.Column<byte[]>(nullable: true),
                                             SCR         = table.Column<byte[]>(nullable: true),
                                             ExtendedCSD = table.Column<byte[]>(nullable: true)
                                         }, constraints: table => { table.PrimaryKey("PK_MmcSd", x => x.Id); });

            migrationBuilder.CreateTable("ModePage_2A",
                                         table => new
                                         {
                                             PS                        = table.Column<bool>(nullable: false),
                                             MultiSession              = table.Column<bool>(nullable: false),
                                             Mode2Form2                = table.Column<bool>(nullable: false),
                                             Mode2Form1                = table.Column<bool>(nullable: false),
                                             AudioPlay                 = table.Column<bool>(nullable: false),
                                             ISRC                      = table.Column<bool>(nullable: false),
                                             UPC                       = table.Column<bool>(nullable: false),
                                             C2Pointer                 = table.Column<bool>(nullable: false),
                                             DeinterlaveSubchannel     = table.Column<bool>(nullable: false),
                                             Subchannel                = table.Column<bool>(nullable: false),
                                             AccurateCDDA              = table.Column<bool>(nullable: false),
                                             CDDACommand               = table.Column<bool>(nullable: false),
                                             LoadingMechanism          = table.Column<byte>(nullable: false),
                                             Eject                     = table.Column<bool>(nullable: false),
                                             PreventJumper             = table.Column<bool>(nullable: false),
                                             LockState                 = table.Column<bool>(nullable: false),
                                             Lock                      = table.Column<bool>(nullable: false),
                                             SeparateChannelMute       = table.Column<bool>(nullable: false),
                                             SeparateChannelVolume     = table.Column<bool>(nullable: false),
                                             MaximumSpeed              = table.Column<ushort>(nullable: false),
                                             SupportedVolumeLevels     = table.Column<ushort>(nullable: false),
                                             BufferSize                = table.Column<ushort>(nullable: false),
                                             CurrentSpeed              = table.Column<ushort>(nullable: false),
                                             Method2                   = table.Column<bool>(nullable: false),
                                             ReadCDRW                  = table.Column<bool>(nullable: false),
                                             ReadCDR                   = table.Column<bool>(nullable: false),
                                             WriteCDRW                 = table.Column<bool>(nullable: false),
                                             WriteCDR                  = table.Column<bool>(nullable: false),
                                             DigitalPort2              = table.Column<bool>(nullable: false),
                                             DigitalPort1              = table.Column<bool>(nullable: false),
                                             Composite                 = table.Column<bool>(nullable: false),
                                             SSS                       = table.Column<bool>(nullable: false),
                                             SDP                       = table.Column<bool>(nullable: false),
                                             Length                    = table.Column<byte>(nullable: false),
                                             LSBF                      = table.Column<bool>(nullable: false),
                                             RCK                       = table.Column<bool>(nullable: false),
                                             BCK                       = table.Column<bool>(nullable: false),
                                             TestWrite                 = table.Column<bool>(nullable: false),
                                             MaxWriteSpeed             = table.Column<ushort>(nullable: false),
                                             CurrentWriteSpeed         = table.Column<ushort>(nullable: false),
                                             ReadBarcode               = table.Column<bool>(nullable: false),
                                             ReadDVDRAM                = table.Column<bool>(nullable: false),
                                             ReadDVDR                  = table.Column<bool>(nullable: false),
                                             ReadDVDROM                = table.Column<bool>(nullable: false),
                                             WriteDVDRAM               = table.Column<bool>(nullable: false),
                                             WriteDVDR                 = table.Column<bool>(nullable: false),
                                             LeadInPW                  = table.Column<bool>(nullable: false),
                                             SCC                       = table.Column<bool>(nullable: false),
                                             CMRSupported              = table.Column<ushort>(nullable: false),
                                             BUF                       = table.Column<bool>(nullable: false),
                                             RotationControlSelected   = table.Column<byte>(nullable: false),
                                             CurrentWriteSpeedSelected = table.Column<ushort>(nullable: false),
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true)
                                         },
                                         constraints: table => { table.PrimaryKey("PK_ModePage_2A", x => x.Id); });

            migrationBuilder.CreateTable("Pcmcia",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             CIS              = table.Column<byte[]>(nullable: true),
                                             Compliance       = table.Column<string>(nullable: true),
                                             ManufacturerCode = table.Column<ushort>(nullable: true),
                                             CardCode         = table.Column<ushort>(nullable: true),
                                             Manufacturer     = table.Column<string>(nullable: true),
                                             ProductName      = table.Column<string>(nullable: true)
                                         }, constraints: table => { table.PrimaryKey("PK_Pcmcia", x => x.Id); });

            migrationBuilder.CreateTable("ScsiMode",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             MediumType        = table.Column<byte>(nullable: true),
                                             WriteProtected    = table.Column<bool>(nullable: false),
                                             Speed             = table.Column<byte>(nullable: true),
                                             BufferedMode      = table.Column<byte>(nullable: true),
                                             BlankCheckEnabled = table.Column<bool>(nullable: false),
                                             DPOandFUA         = table.Column<bool>(nullable: false)
                                         }, constraints: table => { table.PrimaryKey("PK_ScsiMode", x => x.Id); });

            migrationBuilder.CreateTable("Ssc",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             BlockSizeGranularity = table.Column<byte>(nullable: true),
                                             MaxBlockLength       = table.Column<uint>(nullable: true),
                                             MinBlockLength       = table.Column<uint>(nullable: true)
                                         }, constraints: table => { table.PrimaryKey("PK_Ssc", x => x.Id); });

            migrationBuilder.CreateTable("Usb",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             VendorID       = table.Column<ushort>(nullable: false),
                                             ProductID      = table.Column<ushort>(nullable: false),
                                             Manufacturer   = table.Column<string>(nullable: true),
                                             Product        = table.Column<string>(nullable: true),
                                             RemovableMedia = table.Column<bool>(nullable: false),
                                             Descriptors    = table.Column<byte[]>(nullable: true)
                                         }, constraints: table => { table.PrimaryKey("PK_Usb", x => x.Id); });

            migrationBuilder.CreateTable("Mmc",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             ModeSense2AId = table.Column<int>(nullable: true),
                                             FeaturesId    = table.Column<int>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_Mmc", x => x.Id);
                                             table.ForeignKey("FK_Mmc_MmcFeatures_FeaturesId", x => x.FeaturesId,
                                                              "MmcFeatures", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Mmc_ModePage_2A_ModeSense2AId", x => x.ModeSense2AId,
                                                              "ModePage_2A", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("BlockDescriptor",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             Density     = table.Column<byte>(nullable: false),
                                             Blocks      = table.Column<ulong>(nullable: true),
                                             BlockLength = table.Column<uint>(nullable: true),
                                             ScsiModeId  = table.Column<int>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_BlockDescriptor", x => x.Id);
                                             table.ForeignKey("FK_BlockDescriptor_ScsiMode_ScsiModeId",
                                                              x => x.ScsiModeId, "ScsiMode", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("TestedSequentialMedia",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<int>(nullable: false)
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
                                             SscId              = table.Column<int>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_TestedSequentialMedia", x => x.Id);
                                             table.ForeignKey("FK_TestedSequentialMedia_Ssc_SscId", x => x.SscId, "Ssc",
                                                              "Id", onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("SscSupportedMedia",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<int>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             MediumType              = table.Column<byte>(nullable: false),
                                             Width                   = table.Column<ushort>(nullable: false),
                                             Length                  = table.Column<ushort>(nullable: false),
                                             Organization            = table.Column<string>(nullable: true),
                                             Name                    = table.Column<string>(nullable: true),
                                             Description             = table.Column<string>(nullable: true),
                                             SscId                   = table.Column<int>(nullable: true),
                                             TestedSequentialMediaId = table.Column<int>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_SscSupportedMedia", x => x.Id);
                                             table.ForeignKey("FK_SscSupportedMedia_Ssc_SscId", x => x.SscId, "Ssc",
                                                              "Id", onDelete: ReferentialAction.Restrict);
                                             table
                                                .ForeignKey("FK_SscSupportedMedia_TestedSequentialMedia_TestedSequentialMediaId",
                                                            x => x.TestedSequentialMediaId, "TestedSequentialMedia",
                                                            "Id",
                                                            onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("SupportedDensity",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<int>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             PrimaryCode             = table.Column<byte>(nullable: false),
                                             SecondaryCode           = table.Column<byte>(nullable: false),
                                             Writable                = table.Column<bool>(nullable: false),
                                             Duplicate               = table.Column<bool>(nullable: false),
                                             DefaultDensity          = table.Column<bool>(nullable: false),
                                             BitsPerMm               = table.Column<uint>(nullable: false),
                                             Width                   = table.Column<ushort>(nullable: false),
                                             Tracks                  = table.Column<ushort>(nullable: false),
                                             Capacity                = table.Column<uint>(nullable: false),
                                             Organization            = table.Column<string>(nullable: true),
                                             Name                    = table.Column<string>(nullable: true),
                                             Description             = table.Column<string>(nullable: true),
                                             SscId                   = table.Column<int>(nullable: true),
                                             TestedSequentialMediaId = table.Column<int>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_SupportedDensity", x => x.Id);
                                             table.ForeignKey("FK_SupportedDensity_Ssc_SscId", x => x.SscId, "Ssc",
                                                              "Id", onDelete: ReferentialAction.Restrict);
                                             table
                                                .ForeignKey("FK_SupportedDensity_TestedSequentialMedia_TestedSequentialMediaId",
                                                            x => x.TestedSequentialMediaId, "TestedSequentialMedia",
                                                            "Id",
                                                            onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("DensityCode",
                                         table => new
                                         {
                                             Code = table.Column<int>(nullable: false)
                                                         .Annotation("Sqlite:Autoincrement", true),
                                             SscSupportedMediaId = table.Column<int>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_DensityCode", x => x.Code);
                                             table.ForeignKey("FK_DensityCode_SscSupportedMedia_SscSupportedMediaId",
                                                              x => x.SscSupportedMediaId, "SscSupportedMedia", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("Reports",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<int>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             USBId            = table.Column<int>(nullable: true),
                                             FireWireId       = table.Column<int>(nullable: true),
                                             PCMCIAId         = table.Column<int>(nullable: true),
                                             CompactFlash     = table.Column<bool>(nullable: false),
                                             ATAId            = table.Column<int>(nullable: true),
                                             ATAPIId          = table.Column<int>(nullable: true),
                                             SCSIId           = table.Column<int>(nullable: true),
                                             MultiMediaCardId = table.Column<int>(nullable: true),
                                             SecureDigitalId  = table.Column<int>(nullable: true),
                                             Discriminator    = table.Column<string>(nullable: false),
                                             LastSynchronized = table.Column<DateTime>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_Reports", x => x.Id);
                                             table.ForeignKey("FK_Reports_FireWire_FireWireId", x => x.FireWireId,
                                                              "FireWire", "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Reports_MmcSd_MultiMediaCardId",
                                                              x => x.MultiMediaCardId, "MmcSd", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Reports_Pcmcia_PCMCIAId", x => x.PCMCIAId, "Pcmcia",
                                                              "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Reports_MmcSd_SecureDigitalId",
                                                              x => x.SecureDigitalId, "MmcSd", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Reports_Usb_USBId", x => x.USBId, "Usb", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("TestedMedia",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<int>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             IdentifyData                     = table.Column<byte[]>(nullable: true),
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
                                             CanReadFirstTrackPreGap          = table.Column<bool>(nullable: true),
                                             CanReadLeadIn                    = table.Column<bool>(nullable: true),
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
                                             SupportsRead6                    = table.Column<bool>(nullable: true),
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
                                             CHSId                            = table.Column<int>(nullable: true),
                                             CurrentCHSId                     = table.Column<int>(nullable: true),
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
                                             SupportsReadSectors              = table.Column<bool>(nullable: true),
                                             SupportsReadLongRetry            = table.Column<bool>(nullable: true),
                                             SupportsSeek                     = table.Column<bool>(nullable: true),
                                             AtaId                            = table.Column<int>(nullable: true),
                                             MmcId                            = table.Column<int>(nullable: true),
                                             ScsiId                           = table.Column<int>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_TestedMedia", x => x.Id);
                                             table.ForeignKey("FK_TestedMedia_Chs_CHSId", x => x.CHSId, "Chs", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_TestedMedia_Chs_CurrentCHSId", x => x.CurrentCHSId,
                                                              "Chs", "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_TestedMedia_Mmc_MmcId", x => x.MmcId, "Mmc", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("Ata",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             Identify           = table.Column<byte[]>(nullable: true),
                                             ReadCapabilitiesId = table.Column<int>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_Ata", x => x.Id);
                                             table.ForeignKey("FK_Ata_TestedMedia_ReadCapabilitiesId",
                                                              x => x.ReadCapabilitiesId, "TestedMedia", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("Scsi",
                                         table => new
                                         {
                                             Id =
                                                 table.Column<int>(nullable: false)
                                                      .Annotation("Sqlite:Autoincrement", true),
                                             InquiryData          = table.Column<byte[]>(nullable: true),
                                             SupportsModeSense6   = table.Column<bool>(nullable: false),
                                             SupportsModeSense10  = table.Column<bool>(nullable: false),
                                             SupportsModeSubpages = table.Column<bool>(nullable: false),
                                             ModeSenseId          = table.Column<int>(nullable: true),
                                             MultiMediaDeviceId   = table.Column<int>(nullable: true),
                                             ReadCapabilitiesId   = table.Column<int>(nullable: true),
                                             SequentialDeviceId   = table.Column<int>(nullable: true),
                                             ModeSense6Data       = table.Column<byte[]>(nullable: true),
                                             ModeSense10Data      = table.Column<byte[]>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_Scsi", x => x.Id);
                                             table.ForeignKey("FK_Scsi_ScsiMode_ModeSenseId", x => x.ModeSenseId,
                                                              "ScsiMode", "Id", onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Scsi_Mmc_MultiMediaDeviceId",
                                                              x => x.MultiMediaDeviceId, "Mmc", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Scsi_TestedMedia_ReadCapabilitiesId",
                                                              x => x.ReadCapabilitiesId, "TestedMedia", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_Scsi_Ssc_SequentialDeviceId",
                                                              x => x.SequentialDeviceId, "Ssc", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable("ScsiPage",
                                         table => new
                                         {
                                             Id = table.Column<int>(nullable: false)
                                                       .Annotation("Sqlite:Autoincrement", true),
                                             page       = table.Column<byte>(nullable: false),
                                             subpage    = table.Column<byte>(nullable: true),
                                             value      = table.Column<byte[]>(nullable: true),
                                             ScsiId     = table.Column<int>(nullable: true),
                                             ScsiModeId = table.Column<int>(nullable: true)
                                         }, constraints: table =>
                                         {
                                             table.PrimaryKey("PK_ScsiPage", x => x.Id);
                                             table.ForeignKey("FK_ScsiPage_Scsi_ScsiId", x => x.ScsiId, "Scsi", "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_ScsiPage_ScsiMode_ScsiModeId", x => x.ScsiModeId,
                                                              "ScsiMode", "Id", onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateIndex("IX_Ata_ReadCapabilitiesId", "Ata", "ReadCapabilitiesId");

            migrationBuilder.CreateIndex("IX_BlockDescriptor_ScsiModeId", "BlockDescriptor", "ScsiModeId");

            migrationBuilder.CreateIndex("IX_DensityCode_SscSupportedMediaId", "DensityCode", "SscSupportedMediaId");

            migrationBuilder.CreateIndex("IX_Mmc_FeaturesId", "Mmc", "FeaturesId");

            migrationBuilder.CreateIndex("IX_Mmc_ModeSense2AId", "Mmc", "ModeSense2AId");

            migrationBuilder.CreateIndex("IX_Reports_ATAId", "Reports", "ATAId");

            migrationBuilder.CreateIndex("IX_Reports_ATAPIId", "Reports", "ATAPIId");

            migrationBuilder.CreateIndex("IX_Reports_FireWireId", "Reports", "FireWireId");

            migrationBuilder.CreateIndex("IX_Reports_MultiMediaCardId", "Reports", "MultiMediaCardId");

            migrationBuilder.CreateIndex("IX_Reports_PCMCIAId", "Reports", "PCMCIAId");

            migrationBuilder.CreateIndex("IX_Reports_SCSIId", "Reports", "SCSIId");

            migrationBuilder.CreateIndex("IX_Reports_SecureDigitalId", "Reports", "SecureDigitalId");

            migrationBuilder.CreateIndex("IX_Reports_USBId", "Reports", "USBId");

            migrationBuilder.CreateIndex("IX_Scsi_ModeSenseId", "Scsi", "ModeSenseId");

            migrationBuilder.CreateIndex("IX_Scsi_MultiMediaDeviceId", "Scsi", "MultiMediaDeviceId");

            migrationBuilder.CreateIndex("IX_Scsi_ReadCapabilitiesId", "Scsi", "ReadCapabilitiesId");

            migrationBuilder.CreateIndex("IX_Scsi_SequentialDeviceId", "Scsi", "SequentialDeviceId");

            migrationBuilder.CreateIndex("IX_ScsiPage_ScsiId", "ScsiPage", "ScsiId");

            migrationBuilder.CreateIndex("IX_ScsiPage_ScsiModeId", "ScsiPage", "ScsiModeId");

            migrationBuilder.CreateIndex("IX_SscSupportedMedia_SscId", "SscSupportedMedia", "SscId");

            migrationBuilder.CreateIndex("IX_SscSupportedMedia_TestedSequentialMediaId", "SscSupportedMedia",
                                         "TestedSequentialMediaId");

            migrationBuilder.CreateIndex("IX_SupportedDensity_SscId", "SupportedDensity", "SscId");

            migrationBuilder.CreateIndex("IX_SupportedDensity_TestedSequentialMediaId", "SupportedDensity",
                                         "TestedSequentialMediaId");

            migrationBuilder.CreateIndex("IX_TestedMedia_AtaId", "TestedMedia", "AtaId");

            migrationBuilder.CreateIndex("IX_TestedMedia_CHSId", "TestedMedia", "CHSId");

            migrationBuilder.CreateIndex("IX_TestedMedia_CurrentCHSId", "TestedMedia", "CurrentCHSId");

            migrationBuilder.CreateIndex("IX_TestedMedia_MmcId", "TestedMedia", "MmcId");

            migrationBuilder.CreateIndex("IX_TestedMedia_ScsiId", "TestedMedia", "ScsiId");

            migrationBuilder.CreateIndex("IX_TestedSequentialMedia_SscId", "TestedSequentialMedia", "SscId");

            migrationBuilder.AddForeignKey("FK_Reports_Ata_ATAId", "Reports", "ATAId", "Ata", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Reports_Ata_ATAPIId", "Reports", "ATAPIId", "Ata", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Reports_Scsi_SCSIId", "Reports", "SCSIId", "Scsi", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_TestedMedia_Ata_AtaId", "TestedMedia", "AtaId", "Ata",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_TestedMedia_Scsi_ScsiId", "TestedMedia", "ScsiId", "Scsi",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_Ata_TestedMedia_ReadCapabilitiesId", "Ata");

            migrationBuilder.DropForeignKey("FK_Scsi_TestedMedia_ReadCapabilitiesId", "Scsi");

            migrationBuilder.DropTable("BlockDescriptor");

            migrationBuilder.DropTable("DensityCode");

            migrationBuilder.DropTable("Reports");

            migrationBuilder.DropTable("ScsiPage");

            migrationBuilder.DropTable("SupportedDensity");

            migrationBuilder.DropTable("SscSupportedMedia");

            migrationBuilder.DropTable("FireWire");

            migrationBuilder.DropTable("MmcSd");

            migrationBuilder.DropTable("Pcmcia");

            migrationBuilder.DropTable("Usb");

            migrationBuilder.DropTable("TestedSequentialMedia");

            migrationBuilder.DropTable("TestedMedia");

            migrationBuilder.DropTable("Ata");

            migrationBuilder.DropTable("Chs");

            migrationBuilder.DropTable("Scsi");

            migrationBuilder.DropTable("ScsiMode");

            migrationBuilder.DropTable("Mmc");

            migrationBuilder.DropTable("Ssc");

            migrationBuilder.DropTable("MmcFeatures");

            migrationBuilder.DropTable("ModePage_2A");
        }
    }
}