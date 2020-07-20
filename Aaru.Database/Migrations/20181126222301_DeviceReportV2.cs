using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class DeviceReportV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Chs", table => new
            {
                Id        = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                Cylinders = table.Column<ushort>(),
                Heads     = table.Column<ushort>(),
                Sectors   = table.Column<ushort>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Chs", x => x.Id);
            });

            migrationBuilder.CreateTable("FireWire", table => new
            {
                Id             = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                VendorID       = table.Column<uint>(),
                ProductID      = table.Column<uint>(),
                Manufacturer   = table.Column<string>(nullable: true),
                Product        = table.Column<string>(nullable: true),
                RemovableMedia = table.Column<bool>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_FireWire", x => x.Id);
            });

            migrationBuilder.CreateTable("MmcFeatures", table => new
            {
                Id                              = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                AACSVersion                     = table.Column<byte>(nullable: true),
                AGIDs                           = table.Column<byte>(nullable: true),
                BindingNonceBlocks              = table.Column<byte>(nullable: true),
                BlocksPerReadableUnit           = table.Column<ushort>(nullable: true),
                BufferUnderrunFreeInDVD         = table.Column<bool>(),
                BufferUnderrunFreeInSAO         = table.Column<bool>(),
                BufferUnderrunFreeInTAO         = table.Column<bool>(),
                CanAudioScan                    = table.Column<bool>(),
                CanEject                        = table.Column<bool>(),
                CanEraseSector                  = table.Column<bool>(),
                CanExpandBDRESpareArea          = table.Column<bool>(),
                CanFormat                       = table.Column<bool>(),
                CanFormatBDREWithoutSpare       = table.Column<bool>(),
                CanFormatCert                   = table.Column<bool>(),
                CanFormatFRF                    = table.Column<bool>(),
                CanFormatQCert                  = table.Column<bool>(),
                CanFormatRRM                    = table.Column<bool>(),
                CanGenerateBindingNonce         = table.Column<bool>(),
                CanLoad                         = table.Column<bool>(),
                CanMuteSeparateChannels         = table.Column<bool>(),
                CanOverwriteSAOTrack            = table.Column<bool>(),
                CanOverwriteTAOTrack            = table.Column<bool>(),
                CanPlayCDAudio                  = table.Column<bool>(),
                CanPseudoOverwriteBDR           = table.Column<bool>(),
                CanReadAllDualR                 = table.Column<bool>(),
                CanReadAllDualRW                = table.Column<bool>(),
                CanReadBD                       = table.Column<bool>(),
                CanReadBDR                      = table.Column<bool>(),
                CanReadBDRE1                    = table.Column<bool>(),
                CanReadBDRE2                    = table.Column<bool>(),
                CanReadBDROM                    = table.Column<bool>(),
                CanReadBluBCA                   = table.Column<bool>(),
                CanReadCD                       = table.Column<bool>(),
                CanReadCDMRW                    = table.Column<bool>(),
                CanReadCPRM_MKB                 = table.Column<bool>(),
                CanReadDDCD                     = table.Column<bool>(),
                CanReadDVD                      = table.Column<bool>(),
                CanReadDVDPlusMRW               = table.Column<bool>(),
                CanReadDVDPlusR                 = table.Column<bool>(),
                CanReadDVDPlusRDL               = table.Column<bool>(),
                CanReadDVDPlusRW                = table.Column<bool>(),
                CanReadDVDPlusRWDL              = table.Column<bool>(),
                CanReadDriveAACSCertificate     = table.Column<bool>(),
                CanReadHDDVD                    = table.Column<bool>(),
                CanReadHDDVDR                   = table.Column<bool>(),
                CanReadHDDVDRAM                 = table.Column<bool>(),
                CanReadLeadInCDText             = table.Column<bool>(),
                CanReadOldBDR                   = table.Column<bool>(),
                CanReadOldBDRE                  = table.Column<bool>(),
                CanReadOldBDROM                 = table.Column<bool>(),
                CanReadSpareAreaInformation     = table.Column<bool>(),
                CanReportDriveSerial            = table.Column<bool>(),
                CanReportMediaSerial            = table.Column<bool>(),
                CanTestWriteDDCDR               = table.Column<bool>(),
                CanTestWriteDVD                 = table.Column<bool>(),
                CanTestWriteInSAO               = table.Column<bool>(),
                CanTestWriteInTAO               = table.Column<bool>(),
                CanUpgradeFirmware              = table.Column<bool>(),
                CanWriteBD                      = table.Column<bool>(),
                CanWriteBDR                     = table.Column<bool>(),
                CanWriteBDRE1                   = table.Column<bool>(),
                CanWriteBDRE2                   = table.Column<bool>(),
                CanWriteBusEncryptedBlocks      = table.Column<bool>(),
                CanWriteCDMRW                   = table.Column<bool>(),
                CanWriteCDRW                    = table.Column<bool>(),
                CanWriteCDRWCAV                 = table.Column<bool>(),
                CanWriteCDSAO                   = table.Column<bool>(),
                CanWriteCDTAO                   = table.Column<bool>(),
                CanWriteCSSManagedDVD           = table.Column<bool>(),
                CanWriteDDCDR                   = table.Column<bool>(),
                CanWriteDDCDRW                  = table.Column<bool>(),
                CanWriteDVDPlusMRW              = table.Column<bool>(),
                CanWriteDVDPlusR                = table.Column<bool>(),
                CanWriteDVDPlusRDL              = table.Column<bool>(),
                CanWriteDVDPlusRW               = table.Column<bool>(),
                CanWriteDVDPlusRWDL             = table.Column<bool>(),
                CanWriteDVDR                    = table.Column<bool>(),
                CanWriteDVDRDL                  = table.Column<bool>(),
                CanWriteDVDRW                   = table.Column<bool>(),
                CanWriteHDDVDR                  = table.Column<bool>(),
                CanWriteHDDVDRAM                = table.Column<bool>(),
                CanWriteOldBDR                  = table.Column<bool>(),
                CanWriteOldBDRE                 = table.Column<bool>(),
                CanWritePackedSubchannelInTAO   = table.Column<bool>(),
                CanWriteRWSubchannelInSAO       = table.Column<bool>(),
                CanWriteRWSubchannelInTAO       = table.Column<bool>(),
                CanWriteRaw                     = table.Column<bool>(),
                CanWriteRawMultiSession         = table.Column<bool>(),
                CanWriteRawSubchannelInTAO      = table.Column<bool>(),
                ChangerIsSideChangeCapable      = table.Column<bool>(),
                ChangerSlots                    = table.Column<byte>(),
                ChangerSupportsDiscPresent      = table.Column<bool>(),
                CPRMVersion                     = table.Column<byte>(nullable: true),
                CSSVersion                      = table.Column<byte>(nullable: true),
                DBML                            = table.Column<bool>(),
                DVDMultiRead                    = table.Column<bool>(),
                EmbeddedChanger                 = table.Column<bool>(),
                ErrorRecoveryPage               = table.Column<bool>(),
                FirmwareDate                    = table.Column<DateTime>(nullable: true),
                LoadingMechanismType            = table.Column<byte>(nullable: true),
                Locked                          = table.Column<bool>(),
                LogicalBlockSize                = table.Column<uint>(nullable: true),
                MultiRead                       = table.Column<bool>(),
                PhysicalInterfaceStandardNumber = table.Column<uint>(nullable: true),
                PreventJumper                   = table.Column<bool>(),
                SupportsAACS                    = table.Column<bool>(),
                SupportsBusEncryption           = table.Column<bool>(),
                SupportsC2                      = table.Column<bool>(),
                SupportsCPRM                    = table.Column<bool>(),
                SupportsCSS                     = table.Column<bool>(),
                SupportsDAP                     = table.Column<bool>(),
                SupportsDeviceBusyEvent         = table.Column<bool>(),
                SupportsHybridDiscs             = table.Column<bool>(),
                SupportsModePage1Ch             = table.Column<bool>(),
                SupportsOSSC                    = table.Column<bool>(),
                SupportsPWP                     = table.Column<bool>(),
                SupportsSWPP                    = table.Column<bool>(),
                SupportsSecurDisc               = table.Column<bool>(),
                SupportsSeparateVolume          = table.Column<bool>(),
                SupportsVCPS                    = table.Column<bool>(),
                SupportsWriteInhibitDCB         = table.Column<bool>(),
                SupportsWriteProtectPAC         = table.Column<bool>(),
                VolumeLevels                    = table.Column<ushort>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MmcFeatures", x => x.Id);
            });

            migrationBuilder.CreateTable("MmcSd", table => new
            {
                Id          = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                CID         = table.Column<byte[]>(nullable: true),
                CSD         = table.Column<byte[]>(nullable: true),
                OCR         = table.Column<byte[]>(nullable: true),
                SCR         = table.Column<byte[]>(nullable: true),
                ExtendedCSD = table.Column<byte[]>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MmcSd", x => x.Id);
            });

            migrationBuilder.CreateTable("ModePage_2A", table => new
            {
                PS                        = table.Column<bool>(),
                MultiSession              = table.Column<bool>(),
                Mode2Form2                = table.Column<bool>(),
                Mode2Form1                = table.Column<bool>(),
                AudioPlay                 = table.Column<bool>(),
                ISRC                      = table.Column<bool>(),
                UPC                       = table.Column<bool>(),
                C2Pointer                 = table.Column<bool>(),
                DeinterlaveSubchannel     = table.Column<bool>(),
                Subchannel                = table.Column<bool>(),
                AccurateCDDA              = table.Column<bool>(),
                CDDACommand               = table.Column<bool>(),
                LoadingMechanism          = table.Column<byte>(),
                Eject                     = table.Column<bool>(),
                PreventJumper             = table.Column<bool>(),
                LockState                 = table.Column<bool>(),
                Lock                      = table.Column<bool>(),
                SeparateChannelMute       = table.Column<bool>(),
                SeparateChannelVolume     = table.Column<bool>(),
                MaximumSpeed              = table.Column<ushort>(),
                SupportedVolumeLevels     = table.Column<ushort>(),
                BufferSize                = table.Column<ushort>(),
                CurrentSpeed              = table.Column<ushort>(),
                Method2                   = table.Column<bool>(),
                ReadCDRW                  = table.Column<bool>(),
                ReadCDR                   = table.Column<bool>(),
                WriteCDRW                 = table.Column<bool>(),
                WriteCDR                  = table.Column<bool>(),
                DigitalPort2              = table.Column<bool>(),
                DigitalPort1              = table.Column<bool>(),
                Composite                 = table.Column<bool>(),
                SSS                       = table.Column<bool>(),
                SDP                       = table.Column<bool>(),
                Length                    = table.Column<byte>(),
                LSBF                      = table.Column<bool>(),
                RCK                       = table.Column<bool>(),
                BCK                       = table.Column<bool>(),
                TestWrite                 = table.Column<bool>(),
                MaxWriteSpeed             = table.Column<ushort>(),
                CurrentWriteSpeed         = table.Column<ushort>(),
                ReadBarcode               = table.Column<bool>(),
                ReadDVDRAM                = table.Column<bool>(),
                ReadDVDR                  = table.Column<bool>(),
                ReadDVDROM                = table.Column<bool>(),
                WriteDVDRAM               = table.Column<bool>(),
                WriteDVDR                 = table.Column<bool>(),
                LeadInPW                  = table.Column<bool>(),
                SCC                       = table.Column<bool>(),
                CMRSupported              = table.Column<ushort>(),
                BUF                       = table.Column<bool>(),
                RotationControlSelected   = table.Column<byte>(),
                CurrentWriteSpeedSelected = table.Column<ushort>(),
                Id                        = table.Column<int>().Annotation("Sqlite:Autoincrement", true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ModePage_2A", x => x.Id);
            });

            migrationBuilder.CreateTable("Pcmcia", table => new
            {
                Id               = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                CIS              = table.Column<byte[]>(nullable: true),
                Compliance       = table.Column<string>(nullable: true),
                ManufacturerCode = table.Column<ushort>(nullable: true),
                CardCode         = table.Column<ushort>(nullable: true),
                Manufacturer     = table.Column<string>(nullable: true),
                ProductName      = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Pcmcia", x => x.Id);
            });

            migrationBuilder.CreateTable("ScsiMode", table => new
            {
                Id                = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                MediumType        = table.Column<byte>(nullable: true),
                WriteProtected    = table.Column<bool>(),
                Speed             = table.Column<byte>(nullable: true),
                BufferedMode      = table.Column<byte>(nullable: true),
                BlankCheckEnabled = table.Column<bool>(),
                DPOandFUA         = table.Column<bool>()
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ScsiMode", x => x.Id);
            });

            migrationBuilder.CreateTable("Ssc", table => new
            {
                Id                   = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                BlockSizeGranularity = table.Column<byte>(nullable: true),
                MaxBlockLength       = table.Column<uint>(nullable: true),
                MinBlockLength       = table.Column<uint>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Ssc", x => x.Id);
            });

            migrationBuilder.CreateTable("Usb", table => new
            {
                Id             = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                VendorID       = table.Column<ushort>(),
                ProductID      = table.Column<ushort>(),
                Manufacturer   = table.Column<string>(nullable: true),
                Product        = table.Column<string>(nullable: true),
                RemovableMedia = table.Column<bool>(),
                Descriptors    = table.Column<byte[]>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Usb", x => x.Id);
            });

            migrationBuilder.CreateTable("Mmc", table => new
            {
                Id         = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                FeaturesId = table.Column<int>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Mmc", x => x.Id);

                table.ForeignKey("FK_Mmc_MmcFeatures_FeaturesId", x => x.FeaturesId, "MmcFeatures", "Id",
                                 onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateTable("BlockDescriptor", table => new
            {
                Id          = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                Density     = table.Column<byte>(),
                Blocks      = table.Column<ulong>(nullable: true),
                BlockLength = table.Column<uint>(nullable: true),
                ScsiModeId  = table.Column<int>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_BlockDescriptor", x => x.Id);

                table.ForeignKey("FK_BlockDescriptor_ScsiMode_ScsiModeId", x => x.ScsiModeId, "ScsiMode", "Id",
                                 onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateTable("TestedSequentialMedia", table => new
            {
                Id                 = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                CanReadMediaSerial = table.Column<bool>(nullable: true),
                Density            = table.Column<byte>(nullable: true),
                Manufacturer       = table.Column<string>(nullable: true),
                MediaIsRecognized  = table.Column<bool>(),
                MediumType         = table.Column<byte>(nullable: true),
                MediumTypeName     = table.Column<string>(nullable: true),
                Model              = table.Column<string>(nullable: true),
                ModeSense6Data     = table.Column<byte[]>(nullable: true),
                ModeSense10Data    = table.Column<byte[]>(nullable: true),
                SscId              = table.Column<int>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_TestedSequentialMedia", x => x.Id);

                table.ForeignKey("FK_TestedSequentialMedia_Ssc_SscId", x => x.SscId, "Ssc", "Id",
                                 onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateTable("SscSupportedMedia", table => new
            {
                Id                      = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                MediumType              = table.Column<byte>(),
                Width                   = table.Column<ushort>(),
                Length                  = table.Column<ushort>(),
                Organization            = table.Column<string>(nullable: true),
                Name                    = table.Column<string>(nullable: true),
                Description             = table.Column<string>(nullable: true),
                SscId                   = table.Column<int>(nullable: true),
                TestedSequentialMediaId = table.Column<int>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_SscSupportedMedia", x => x.Id);

                table.ForeignKey("FK_SscSupportedMedia_Ssc_SscId", x => x.SscId, "Ssc", "Id",
                                 onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_SscSupportedMedia_TestedSequentialMedia_TestedSequentialMediaId",
                                 x => x.TestedSequentialMediaId, "TestedSequentialMedia", "Id",
                                 onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateTable("SupportedDensity", table => new
            {
                Id                      = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                PrimaryCode             = table.Column<byte>(),
                SecondaryCode           = table.Column<byte>(),
                Writable                = table.Column<bool>(),
                Duplicate               = table.Column<bool>(),
                DefaultDensity          = table.Column<bool>(),
                BitsPerMm               = table.Column<uint>(),
                Width                   = table.Column<ushort>(),
                Tracks                  = table.Column<ushort>(),
                Capacity                = table.Column<uint>(),
                Organization            = table.Column<string>(nullable: true),
                Name                    = table.Column<string>(nullable: true),
                Description             = table.Column<string>(nullable: true),
                SscId                   = table.Column<int>(nullable: true),
                TestedSequentialMediaId = table.Column<int>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_SupportedDensity", x => x.Id);

                table.ForeignKey("FK_SupportedDensity_Ssc_SscId", x => x.SscId, "Ssc", "Id",
                                 onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_SupportedDensity_TestedSequentialMedia_TestedSequentialMediaId",
                                 x => x.TestedSequentialMediaId, "TestedSequentialMedia", "Id",
                                 onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateTable("DensityCode", table => new
            {
                Code                = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                SscSupportedMediaId = table.Column<int>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_DensityCode", x => x.Code);

                table.ForeignKey("FK_DensityCode_SscSupportedMedia_SscSupportedMediaId", x => x.SscSupportedMediaId,
                                 "SscSupportedMedia", "Id", onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateTable("Reports", table => new
            {
                Id               = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                USBId            = table.Column<int>(nullable: true),
                FireWireId       = table.Column<int>(nullable: true),
                PCMCIAId         = table.Column<int>(nullable: true),
                CompactFlash     = table.Column<bool>(),
                ATAId            = table.Column<int>(nullable: true),
                ATAPIId          = table.Column<int>(nullable: true),
                SCSIId           = table.Column<int>(nullable: true),
                MultiMediaCardId = table.Column<int>(nullable: true),
                SecureDigitalId  = table.Column<int>(nullable: true),
                Discriminator    = table.Column<string>(),
                LastSynchronized = table.Column<DateTime>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Reports", x => x.Id);

                table.ForeignKey("FK_Reports_FireWire_FireWireId", x => x.FireWireId, "FireWire", "Id",
                                 onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_Reports_MmcSd_MultiMediaCardId", x => x.MultiMediaCardId, "MmcSd", "Id",
                                 onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_Reports_Pcmcia_PCMCIAId", x => x.PCMCIAId, "Pcmcia", "Id",
                                 onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_Reports_MmcSd_SecureDigitalId", x => x.SecureDigitalId, "MmcSd", "Id",
                                 onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_Reports_Usb_USBId", x => x.USBId, "Usb", "Id",
                                 onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateTable("TestedMedia", table => new
            {
                Id                               = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
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
                MediaIsRecognized                = table.Column<bool>(),
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

                table.ForeignKey("FK_TestedMedia_Chs_CurrentCHSId", x => x.CurrentCHSId, "Chs", "Id",
                                 onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_TestedMedia_Mmc_MmcId", x => x.MmcId, "Mmc", "Id",
                                 onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateTable("Ata", table => new
            {
                Id                 = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                Identify           = table.Column<byte[]>(nullable: true),
                ReadCapabilitiesId = table.Column<int>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Ata", x => x.Id);

                table.ForeignKey("FK_Ata_TestedMedia_ReadCapabilitiesId", x => x.ReadCapabilitiesId, "TestedMedia",
                                 "Id", onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateTable("Scsi", table => new
            {
                Id                   = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                InquiryData          = table.Column<byte[]>(nullable: true),
                SupportsModeSense6   = table.Column<bool>(),
                SupportsModeSense10  = table.Column<bool>(),
                SupportsModeSubpages = table.Column<bool>(),
                ModeSenseId          = table.Column<int>(nullable: true),
                MultiMediaDeviceId   = table.Column<int>(nullable: true),
                ReadCapabilitiesId   = table.Column<int>(nullable: true),
                SequentialDeviceId   = table.Column<int>(nullable: true),
                ModeSense6Data       = table.Column<byte[]>(nullable: true),
                ModeSense10Data      = table.Column<byte[]>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Scsi", x => x.Id);

                table.ForeignKey("FK_Scsi_ScsiMode_ModeSenseId", x => x.ModeSenseId, "ScsiMode", "Id",
                                 onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_Scsi_Mmc_MultiMediaDeviceId", x => x.MultiMediaDeviceId, "Mmc", "Id",
                                 onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_Scsi_TestedMedia_ReadCapabilitiesId", x => x.ReadCapabilitiesId, "TestedMedia",
                                 "Id", onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_Scsi_Ssc_SequentialDeviceId", x => x.SequentialDeviceId, "Ssc", "Id",
                                 onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateTable("ScsiPage", table => new
            {
                Id         = table.Column<int>().Annotation("Sqlite:Autoincrement", true),
                page       = table.Column<byte>(),
                subpage    = table.Column<byte>(nullable: true),
                value      = table.Column<byte[]>(nullable: true),
                ScsiId     = table.Column<int>(nullable: true),
                ScsiModeId = table.Column<int>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ScsiPage", x => x.Id);

                table.ForeignKey("FK_ScsiPage_Scsi_ScsiId", x => x.ScsiId, "Scsi", "Id",
                                 onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_ScsiPage_ScsiMode_ScsiModeId", x => x.ScsiModeId, "ScsiMode", "Id",
                                 onDelete: ReferentialAction.Restrict);
            });

            migrationBuilder.CreateIndex("IX_Ata_ReadCapabilitiesId", "Ata", "ReadCapabilitiesId");

            migrationBuilder.CreateIndex("IX_BlockDescriptor_ScsiModeId", "BlockDescriptor", "ScsiModeId");

            migrationBuilder.CreateIndex("IX_DensityCode_SscSupportedMediaId", "DensityCode", "SscSupportedMediaId");

            migrationBuilder.CreateIndex("IX_Mmc_FeaturesId", "Mmc", "FeaturesId");

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