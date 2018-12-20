using System.Data.Entity.Migrations;

namespace DiscImageChef.Server.Migrations
{
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable("dbo.Devices",
                        c => new
                        {
                            Id                = c.Int(false, true),
                            AddedWhen         = c.DateTime(false, 0),
                            CompactFlash      = c.Boolean(false),
                            Manufacturer      = c.String(unicode: false),
                            Model             = c.String(unicode: false),
                            Revision          = c.String(unicode: false),
                            Type              = c.Int(false),
                            ATA_Id            = c.Int(),
                            ATAPI_Id          = c.Int(),
                            FireWire_Id       = c.Int(),
                            MultiMediaCard_Id = c.Int(),
                            PCMCIA_Id         = c.Int(),
                            SCSI_Id           = c.Int(),
                            SecureDigital_Id  = c.Int(),
                            USB_Id            = c.Int()
                        }).PrimaryKey(t => t.Id).ForeignKey("dbo.Atas", t => t.ATA_Id)
                          .ForeignKey("dbo.Atas",    t => t.ATAPI_Id).ForeignKey("dbo.FireWires", t => t.FireWire_Id)
                          .ForeignKey("dbo.MmcSds",  t => t.MultiMediaCard_Id)
                          .ForeignKey("dbo.Pcmcias", t => t.PCMCIA_Id).ForeignKey("dbo.Scsis", t => t.SCSI_Id)
                          .ForeignKey("dbo.MmcSds",  t => t.SecureDigital_Id).ForeignKey("dbo.Usbs", t => t.USB_Id)
                          .Index(t => t.ATA_Id).Index(t => t.ATAPI_Id).Index(t => t.FireWire_Id)
                          .Index(t => t.MultiMediaCard_Id).Index(t => t.PCMCIA_Id).Index(t => t.SCSI_Id)
                          .Index(t => t.SecureDigital_Id).Index(t => t.USB_Id);

            CreateTable("dbo.Atas",
                        c => new {Id = c.Int(false, true), Identify = c.Binary(), ReadCapabilities_Id = c.Int()})
               .PrimaryKey(t => t.Id).ForeignKey("dbo.TestedMedias", t => t.ReadCapabilities_Id)
               .Index(t => t.ReadCapabilities_Id);

            CreateTable("dbo.TestedMedias",
                        c => new
                        {
                            Id                               = c.Int(false, true),
                            IdentifyData                     = c.Binary(),
                            CanReadAACS                      = c.Boolean(),
                            CanReadADIP                      = c.Boolean(),
                            CanReadATIP                      = c.Boolean(),
                            CanReadBCA                       = c.Boolean(),
                            CanReadC2Pointers                = c.Boolean(),
                            CanReadCMI                       = c.Boolean(),
                            CanReadCorrectedSubchannel       = c.Boolean(),
                            CanReadCorrectedSubchannelWithC2 = c.Boolean(),
                            CanReadDCB                       = c.Boolean(),
                            CanReadDDS                       = c.Boolean(),
                            CanReadDMI                       = c.Boolean(),
                            CanReadDiscInformation           = c.Boolean(),
                            CanReadFullTOC                   = c.Boolean(),
                            CanReadHDCMI                     = c.Boolean(),
                            CanReadLayerCapacity             = c.Boolean(),
                            CanReadFirstTrackPreGap          = c.Boolean(),
                            CanReadLeadIn                    = c.Boolean(),
                            CanReadLeadOut                   = c.Boolean(),
                            CanReadMediaID                   = c.Boolean(),
                            CanReadMediaSerial               = c.Boolean(),
                            CanReadPAC                       = c.Boolean(),
                            CanReadPFI                       = c.Boolean(),
                            CanReadPMA                       = c.Boolean(),
                            CanReadPQSubchannel              = c.Boolean(),
                            CanReadPQSubchannelWithC2        = c.Boolean(),
                            CanReadPRI                       = c.Boolean(),
                            CanReadRWSubchannel              = c.Boolean(),
                            CanReadRWSubchannelWithC2        = c.Boolean(),
                            CanReadRecordablePFI             = c.Boolean(),
                            CanReadSpareAreaInformation      = c.Boolean(),
                            CanReadTOC                       = c.Boolean(),
                            Density                          = c.Byte(),
                            Manufacturer                     = c.String(unicode: false),
                            MediaIsRecognized                = c.Boolean(false),
                            MediumType                       = c.Byte(),
                            MediumTypeName                   = c.String(unicode: false),
                            Model                            = c.String(unicode: false),
                            SupportsHLDTSTReadRawDVD         = c.Boolean(),
                            SupportsNECReadCDDA              = c.Boolean(),
                            SupportsPioneerReadCDDA          = c.Boolean(),
                            SupportsPioneerReadCDDAMSF       = c.Boolean(),
                            SupportsPlextorReadCDDA          = c.Boolean(),
                            SupportsPlextorReadRawDVD        = c.Boolean(),
                            SupportsRead10                   = c.Boolean(),
                            SupportsRead12                   = c.Boolean(),
                            SupportsRead16                   = c.Boolean(),
                            SupportsRead6                    = c.Boolean(),
                            SupportsReadCapacity16           = c.Boolean(),
                            SupportsReadCapacity             = c.Boolean(),
                            SupportsReadCd                   = c.Boolean(),
                            SupportsReadCdMsf                = c.Boolean(),
                            SupportsReadCdRaw                = c.Boolean(),
                            SupportsReadCdMsfRaw             = c.Boolean(),
                            SupportsReadLong16               = c.Boolean(),
                            SupportsReadLong                 = c.Boolean(),
                            ModeSense6Data                   = c.Binary(),
                            ModeSense10Data                  = c.Binary(),
                            SolidStateDevice                 = c.Boolean(),
                            SupportsReadDmaLba               = c.Boolean(),
                            SupportsReadDmaRetryLba          = c.Boolean(),
                            SupportsReadLba                  = c.Boolean(),
                            SupportsReadRetryLba             = c.Boolean(),
                            SupportsReadLongLba              = c.Boolean(),
                            SupportsReadLongRetryLba         = c.Boolean(),
                            SupportsSeekLba                  = c.Boolean(),
                            SupportsReadDmaLba48             = c.Boolean(),
                            SupportsReadLba48                = c.Boolean(),
                            SupportsReadDma                  = c.Boolean(),
                            SupportsReadDmaRetry             = c.Boolean(),
                            SupportsReadRetry                = c.Boolean(),
                            SupportsReadSectors              = c.Boolean(),
                            SupportsReadLongRetry            = c.Boolean(),
                            SupportsSeek                     = c.Boolean(),
                            CHS_Id                           = c.Int(),
                            CurrentCHS_Id                    = c.Int(),
                            Ata_Id                           = c.Int(),
                            Mmc_Id                           = c.Int(),
                            Scsi_Id                          = c.Int()
                        }).PrimaryKey(t => t.Id).ForeignKey("dbo.Chs", t => t.CHS_Id)
                          .ForeignKey("dbo.Chs",  t => t.CurrentCHS_Id).ForeignKey("dbo.Atas", t => t.Ata_Id)
                          .ForeignKey("dbo.Mmcs", t => t.Mmc_Id).ForeignKey("dbo.Scsis", t => t.Scsi_Id)
                          .Index(t => t.CHS_Id).Index(t => t.CurrentCHS_Id).Index(t => t.Ata_Id)
                          .Index(t => t.Mmc_Id).Index(t => t.Scsi_Id);

            CreateTable("dbo.Chs", c => new {Id = c.Int(false, true)}).PrimaryKey(t => t.Id);

            CreateTable("dbo.FireWires",
                        c => new
                        {
                            Id             = c.Int(false, true),
                            Manufacturer   = c.String(unicode: false),
                            Product        = c.String(unicode: false),
                            RemovableMedia = c.Boolean(false)
                        }).PrimaryKey(t => t.Id);

            CreateTable("dbo.MmcSds",
                        c => new
                        {
                            Id          = c.Int(false, true),
                            CID         = c.Binary(),
                            CSD         = c.Binary(),
                            OCR         = c.Binary(),
                            SCR         = c.Binary(),
                            ExtendedCSD = c.Binary()
                        }).PrimaryKey(t => t.Id);

            CreateTable("dbo.Pcmcias",
                        c => new
                        {
                            Id           = c.Int(false, true),
                            CIS          = c.Binary(),
                            Compliance   = c.String(unicode: false),
                            Manufacturer = c.String(unicode: false),
                            ProductName  = c.String(unicode: false)
                        }).PrimaryKey(t => t.Id);

            CreateTable("dbo.Scsis",
                        c => new
                        {
                            Id                   = c.Int(false, true),
                            InquiryData          = c.Binary(),
                            SupportsModeSense6   = c.Boolean(false),
                            SupportsModeSense10  = c.Boolean(false),
                            SupportsModeSubpages = c.Boolean(false),
                            ModeSense6Data       = c.Binary(),
                            ModeSense10Data      = c.Binary(),
                            ModeSense_Id         = c.Int(),
                            MultiMediaDevice_Id  = c.Int(),
                            ReadCapabilities_Id  = c.Int(),
                            SequentialDevice_Id  = c.Int()
                        }).PrimaryKey(t => t.Id).ForeignKey("dbo.ScsiModes", t => t.ModeSense_Id)
                          .ForeignKey("dbo.Mmcs",         t => t.MultiMediaDevice_Id)
                          .ForeignKey("dbo.TestedMedias", t => t.ReadCapabilities_Id)
                          .ForeignKey("dbo.Sscs",         t => t.SequentialDevice_Id).Index(t => t.ModeSense_Id)
                          .Index(t => t.MultiMediaDevice_Id).Index(t => t.ReadCapabilities_Id)
                          .Index(t => t.SequentialDevice_Id);

            CreateTable("dbo.ScsiPages",
                        c => new
                        {
                            Id          = c.Int(false, true),
                            page        = c.Byte(false),
                            subpage     = c.Byte(),
                            value       = c.Binary(),
                            Scsi_Id     = c.Int(),
                            ScsiMode_Id = c.Int()
                        }).PrimaryKey(t => t.Id).ForeignKey("dbo.Scsis", t => t.Scsi_Id)
                          .ForeignKey("dbo.ScsiModes", t => t.ScsiMode_Id).Index(t => t.Scsi_Id)
                          .Index(t => t.ScsiMode_Id);

            CreateTable("dbo.ScsiModes",
                        c => new
                        {
                            Id                = c.Int(false, true),
                            MediumType        = c.Byte(),
                            WriteProtected    = c.Boolean(false),
                            Speed             = c.Byte(),
                            BufferedMode      = c.Byte(),
                            BlankCheckEnabled = c.Boolean(false),
                            DPOandFUA         = c.Boolean(false)
                        }).PrimaryKey(t => t.Id);

            CreateTable("dbo.BlockDescriptors",
                        c => new {Id = c.Int(false, true), Density = c.Byte(false), ScsiMode_Id = c.Int()})
               .PrimaryKey(t => t.Id).ForeignKey("dbo.ScsiModes", t => t.ScsiMode_Id).Index(t => t.ScsiMode_Id);

            CreateTable("dbo.Mmcs", c => new {Id = c.Int(false, true), Features_Id = c.Int(), ModeSense2A_Id = c.Int()})
               .PrimaryKey(t => t.Id).ForeignKey("dbo.MmcFeatures", t => t.Features_Id)
               .ForeignKey("dbo.ModePage_2A", t => t.ModeSense2A_Id).Index(t => t.Features_Id)
               .Index(t => t.ModeSense2A_Id);

            CreateTable("dbo.MmcFeatures",
                        c => new
                        {
                            Id                            = c.Int(false, true),
                            AACSVersion                   = c.Byte(),
                            AGIDs                         = c.Byte(),
                            BindingNonceBlocks            = c.Byte(),
                            BufferUnderrunFreeInDVD       = c.Boolean(false),
                            BufferUnderrunFreeInSAO       = c.Boolean(false),
                            BufferUnderrunFreeInTAO       = c.Boolean(false),
                            CanAudioScan                  = c.Boolean(false),
                            CanEject                      = c.Boolean(false),
                            CanEraseSector                = c.Boolean(false),
                            CanExpandBDRESpareArea        = c.Boolean(false),
                            CanFormat                     = c.Boolean(false),
                            CanFormatBDREWithoutSpare     = c.Boolean(false),
                            CanFormatCert                 = c.Boolean(false),
                            CanFormatFRF                  = c.Boolean(false),
                            CanFormatQCert                = c.Boolean(false),
                            CanFormatRRM                  = c.Boolean(false),
                            CanGenerateBindingNonce       = c.Boolean(false),
                            CanLoad                       = c.Boolean(false),
                            CanMuteSeparateChannels       = c.Boolean(false),
                            CanOverwriteSAOTrack          = c.Boolean(false),
                            CanOverwriteTAOTrack          = c.Boolean(false),
                            CanPlayCDAudio                = c.Boolean(false),
                            CanPseudoOverwriteBDR         = c.Boolean(false),
                            CanReadAllDualR               = c.Boolean(false),
                            CanReadAllDualRW              = c.Boolean(false),
                            CanReadBD                     = c.Boolean(false),
                            CanReadBDR                    = c.Boolean(false),
                            CanReadBDRE1                  = c.Boolean(false),
                            CanReadBDRE2                  = c.Boolean(false),
                            CanReadBDROM                  = c.Boolean(false),
                            CanReadBluBCA                 = c.Boolean(false),
                            CanReadCD                     = c.Boolean(false),
                            CanReadCDMRW                  = c.Boolean(false),
                            CanReadCPRM_MKB               = c.Boolean(false),
                            CanReadDDCD                   = c.Boolean(false),
                            CanReadDVD                    = c.Boolean(false),
                            CanReadDVDPlusMRW             = c.Boolean(false),
                            CanReadDVDPlusR               = c.Boolean(false),
                            CanReadDVDPlusRDL             = c.Boolean(false),
                            CanReadDVDPlusRW              = c.Boolean(false),
                            CanReadDVDPlusRWDL            = c.Boolean(false),
                            CanReadDriveAACSCertificate   = c.Boolean(false),
                            CanReadHDDVD                  = c.Boolean(false),
                            CanReadHDDVDR                 = c.Boolean(false),
                            CanReadHDDVDRAM               = c.Boolean(false),
                            CanReadLeadInCDText           = c.Boolean(false),
                            CanReadOldBDR                 = c.Boolean(false),
                            CanReadOldBDRE                = c.Boolean(false),
                            CanReadOldBDROM               = c.Boolean(false),
                            CanReadSpareAreaInformation   = c.Boolean(false),
                            CanReportDriveSerial          = c.Boolean(false),
                            CanReportMediaSerial          = c.Boolean(false),
                            CanTestWriteDDCDR             = c.Boolean(false),
                            CanTestWriteDVD               = c.Boolean(false),
                            CanTestWriteInSAO             = c.Boolean(false),
                            CanTestWriteInTAO             = c.Boolean(false),
                            CanUpgradeFirmware            = c.Boolean(false),
                            CanWriteBD                    = c.Boolean(false),
                            CanWriteBDR                   = c.Boolean(false),
                            CanWriteBDRE1                 = c.Boolean(false),
                            CanWriteBDRE2                 = c.Boolean(false),
                            CanWriteBusEncryptedBlocks    = c.Boolean(false),
                            CanWriteCDMRW                 = c.Boolean(false),
                            CanWriteCDRW                  = c.Boolean(false),
                            CanWriteCDRWCAV               = c.Boolean(false),
                            CanWriteCDSAO                 = c.Boolean(false),
                            CanWriteCDTAO                 = c.Boolean(false),
                            CanWriteCSSManagedDVD         = c.Boolean(false),
                            CanWriteDDCDR                 = c.Boolean(false),
                            CanWriteDDCDRW                = c.Boolean(false),
                            CanWriteDVDPlusMRW            = c.Boolean(false),
                            CanWriteDVDPlusR              = c.Boolean(false),
                            CanWriteDVDPlusRDL            = c.Boolean(false),
                            CanWriteDVDPlusRW             = c.Boolean(false),
                            CanWriteDVDPlusRWDL           = c.Boolean(false),
                            CanWriteDVDR                  = c.Boolean(false),
                            CanWriteDVDRDL                = c.Boolean(false),
                            CanWriteDVDRW                 = c.Boolean(false),
                            CanWriteHDDVDR                = c.Boolean(false),
                            CanWriteHDDVDRAM              = c.Boolean(false),
                            CanWriteOldBDR                = c.Boolean(false),
                            CanWriteOldBDRE               = c.Boolean(false),
                            CanWritePackedSubchannelInTAO = c.Boolean(false),
                            CanWriteRWSubchannelInSAO     = c.Boolean(false),
                            CanWriteRWSubchannelInTAO     = c.Boolean(false),
                            CanWriteRaw                   = c.Boolean(false),
                            CanWriteRawMultiSession       = c.Boolean(false),
                            CanWriteRawSubchannelInTAO    = c.Boolean(false),
                            ChangerIsSideChangeCapable    = c.Boolean(false),
                            ChangerSlots                  = c.Byte(false),
                            ChangerSupportsDiscPresent    = c.Boolean(false),
                            CPRMVersion                   = c.Byte(),
                            CSSVersion                    = c.Byte(),
                            DBML                          = c.Boolean(false),
                            DVDMultiRead                  = c.Boolean(false),
                            EmbeddedChanger               = c.Boolean(false),
                            ErrorRecoveryPage             = c.Boolean(false),
                            FirmwareDate                  = c.DateTime(precision: 0),
                            LoadingMechanismType          = c.Byte(),
                            Locked                        = c.Boolean(false),
                            MultiRead                     = c.Boolean(false),
                            PreventJumper                 = c.Boolean(false),
                            SupportsAACS                  = c.Boolean(false),
                            SupportsBusEncryption         = c.Boolean(false),
                            SupportsC2                    = c.Boolean(false),
                            SupportsCPRM                  = c.Boolean(false),
                            SupportsCSS                   = c.Boolean(false),
                            SupportsDAP                   = c.Boolean(false),
                            SupportsDeviceBusyEvent       = c.Boolean(false),
                            SupportsHybridDiscs           = c.Boolean(false),
                            SupportsModePage1Ch           = c.Boolean(false),
                            SupportsOSSC                  = c.Boolean(false),
                            SupportsPWP                   = c.Boolean(false),
                            SupportsSWPP                  = c.Boolean(false),
                            SupportsSecurDisc             = c.Boolean(false),
                            SupportsSeparateVolume        = c.Boolean(false),
                            SupportsVCPS                  = c.Boolean(false),
                            SupportsWriteInhibitDCB       = c.Boolean(false),
                            SupportsWriteProtectPAC       = c.Boolean(false)
                        }).PrimaryKey(t => t.Id);

            CreateTable("dbo.ModePage_2A",
                        c => new
                        {
                            Id                      = c.Int(false, true),
                            PS                      = c.Boolean(false),
                            MultiSession            = c.Boolean(false),
                            Mode2Form2              = c.Boolean(false),
                            Mode2Form1              = c.Boolean(false),
                            AudioPlay               = c.Boolean(false),
                            ISRC                    = c.Boolean(false),
                            UPC                     = c.Boolean(false),
                            C2Pointer               = c.Boolean(false),
                            DeinterlaveSubchannel   = c.Boolean(false),
                            Subchannel              = c.Boolean(false),
                            AccurateCDDA            = c.Boolean(false),
                            CDDACommand             = c.Boolean(false),
                            LoadingMechanism        = c.Byte(false),
                            Eject                   = c.Boolean(false),
                            PreventJumper           = c.Boolean(false),
                            LockState               = c.Boolean(false),
                            Lock                    = c.Boolean(false),
                            SeparateChannelMute     = c.Boolean(false),
                            SeparateChannelVolume   = c.Boolean(false),
                            Method2                 = c.Boolean(false),
                            ReadCDRW                = c.Boolean(false),
                            ReadCDR                 = c.Boolean(false),
                            WriteCDRW               = c.Boolean(false),
                            WriteCDR                = c.Boolean(false),
                            DigitalPort2            = c.Boolean(false),
                            DigitalPort1            = c.Boolean(false),
                            Composite               = c.Boolean(false),
                            SSS                     = c.Boolean(false),
                            SDP                     = c.Boolean(false),
                            Length                  = c.Byte(false),
                            LSBF                    = c.Boolean(false),
                            RCK                     = c.Boolean(false),
                            BCK                     = c.Boolean(false),
                            TestWrite               = c.Boolean(false),
                            ReadBarcode             = c.Boolean(false),
                            ReadDVDRAM              = c.Boolean(false),
                            ReadDVDR                = c.Boolean(false),
                            ReadDVDROM              = c.Boolean(false),
                            WriteDVDRAM             = c.Boolean(false),
                            WriteDVDR               = c.Boolean(false),
                            LeadInPW                = c.Boolean(false),
                            SCC                     = c.Boolean(false),
                            BUF                     = c.Boolean(false),
                            RotationControlSelected = c.Byte(false)
                        }).PrimaryKey(t => t.Id);

            CreateTable("dbo.Sscs", c => new {Id = c.Int(false, true), BlockSizeGranularity = c.Byte()})
               .PrimaryKey(t => t.Id);

            CreateTable("dbo.SupportedDensities",
                        c => new
                        {
                            Id                       = c.Int(false, true),
                            PrimaryCode              = c.Byte(false),
                            SecondaryCode            = c.Byte(false),
                            Writable                 = c.Boolean(false),
                            Duplicate                = c.Boolean(false),
                            DefaultDensity           = c.Boolean(false),
                            Organization             = c.String(unicode: false),
                            Name                     = c.String(unicode: false),
                            Description              = c.String(unicode: false),
                            Ssc_Id                   = c.Int(),
                            TestedSequentialMedia_Id = c.Int()
                        }).PrimaryKey(t => t.Id).ForeignKey("dbo.Sscs", t => t.Ssc_Id)
                          .ForeignKey("dbo.TestedSequentialMedias", t => t.TestedSequentialMedia_Id)
                          .Index(t => t.Ssc_Id).Index(t => t.TestedSequentialMedia_Id);

            CreateTable("dbo.SscSupportedMedias",
                        c => new
                        {
                            Id                       = c.Int(false, true),
                            MediumType               = c.Byte(false),
                            Organization             = c.String(unicode: false),
                            Name                     = c.String(unicode: false),
                            Description              = c.String(unicode: false),
                            Ssc_Id                   = c.Int(),
                            TestedSequentialMedia_Id = c.Int()
                        }).PrimaryKey(t => t.Id).ForeignKey("dbo.Sscs", t => t.Ssc_Id)
                          .ForeignKey("dbo.TestedSequentialMedias", t => t.TestedSequentialMedia_Id)
                          .Index(t => t.Ssc_Id).Index(t => t.TestedSequentialMedia_Id);

            CreateTable("dbo.DensityCodes", c => new {Code = c.Int(false, true), SscSupportedMedia_Id = c.Int()})
               .PrimaryKey(t => t.Code).ForeignKey("dbo.SscSupportedMedias", t => t.SscSupportedMedia_Id)
               .Index(t => t.SscSupportedMedia_Id);

            CreateTable("dbo.TestedSequentialMedias",
                        c => new
                        {
                            Id                 = c.Int(false, true),
                            CanReadMediaSerial = c.Boolean(),
                            Density            = c.Byte(),
                            Manufacturer       = c.String(unicode: false),
                            MediaIsRecognized  = c.Boolean(false),
                            MediumType         = c.Byte(),
                            MediumTypeName     = c.String(unicode: false),
                            Model              = c.String(unicode: false),
                            ModeSense6Data     = c.Binary(),
                            ModeSense10Data    = c.Binary(),
                            Ssc_Id             = c.Int()
                        }).PrimaryKey(t => t.Id).ForeignKey("dbo.Sscs", t => t.Ssc_Id).Index(t => t.Ssc_Id);

            CreateTable("dbo.Usbs",
                        c => new
                        {
                            Id             = c.Int(false, true),
                            Manufacturer   = c.String(unicode: false),
                            Product        = c.String(unicode: false),
                            RemovableMedia = c.Boolean(false),
                            Descriptors    = c.Binary()
                        }).PrimaryKey(t => t.Id);

            CreateTable("dbo.UploadedReports",
                        c => new
                        {
                            Id                = c.Int(false, true),
                            UploadedWhen      = c.DateTime(false, 0),
                            CompactFlash      = c.Boolean(false),
                            Manufacturer      = c.String(unicode: false),
                            Model             = c.String(unicode: false),
                            Revision          = c.String(unicode: false),
                            Type              = c.Int(false),
                            ATA_Id            = c.Int(),
                            ATAPI_Id          = c.Int(),
                            FireWire_Id       = c.Int(),
                            MultiMediaCard_Id = c.Int(),
                            PCMCIA_Id         = c.Int(),
                            SCSI_Id           = c.Int(),
                            SecureDigital_Id  = c.Int(),
                            USB_Id            = c.Int()
                        }).PrimaryKey(t => t.Id).ForeignKey("dbo.Atas", t => t.ATA_Id)
                          .ForeignKey("dbo.Atas",    t => t.ATAPI_Id).ForeignKey("dbo.FireWires", t => t.FireWire_Id)
                          .ForeignKey("dbo.MmcSds",  t => t.MultiMediaCard_Id)
                          .ForeignKey("dbo.Pcmcias", t => t.PCMCIA_Id).ForeignKey("dbo.Scsis", t => t.SCSI_Id)
                          .ForeignKey("dbo.MmcSds",  t => t.SecureDigital_Id).ForeignKey("dbo.Usbs", t => t.USB_Id)
                          .Index(t => t.ATA_Id).Index(t => t.ATAPI_Id).Index(t => t.FireWire_Id)
                          .Index(t => t.MultiMediaCard_Id).Index(t => t.PCMCIA_Id).Index(t => t.SCSI_Id)
                          .Index(t => t.SecureDigital_Id).Index(t => t.USB_Id);
        }

        public override void Down()
        {
            DropForeignKey("dbo.UploadedReports",        "USB_Id",                   "dbo.Usbs");
            DropForeignKey("dbo.UploadedReports",        "SecureDigital_Id",         "dbo.MmcSds");
            DropForeignKey("dbo.UploadedReports",        "SCSI_Id",                  "dbo.Scsis");
            DropForeignKey("dbo.UploadedReports",        "PCMCIA_Id",                "dbo.Pcmcias");
            DropForeignKey("dbo.UploadedReports",        "MultiMediaCard_Id",        "dbo.MmcSds");
            DropForeignKey("dbo.UploadedReports",        "FireWire_Id",              "dbo.FireWires");
            DropForeignKey("dbo.UploadedReports",        "ATAPI_Id",                 "dbo.Atas");
            DropForeignKey("dbo.UploadedReports",        "ATA_Id",                   "dbo.Atas");
            DropForeignKey("dbo.Devices",                "USB_Id",                   "dbo.Usbs");
            DropForeignKey("dbo.Devices",                "SecureDigital_Id",         "dbo.MmcSds");
            DropForeignKey("dbo.Devices",                "SCSI_Id",                  "dbo.Scsis");
            DropForeignKey("dbo.Scsis",                  "SequentialDevice_Id",      "dbo.Sscs");
            DropForeignKey("dbo.TestedSequentialMedias", "Ssc_Id",                   "dbo.Sscs");
            DropForeignKey("dbo.SscSupportedMedias",     "TestedSequentialMedia_Id", "dbo.TestedSequentialMedias");
            DropForeignKey("dbo.SupportedDensities",     "TestedSequentialMedia_Id", "dbo.TestedSequentialMedias");
            DropForeignKey("dbo.SscSupportedMedias",     "Ssc_Id",                   "dbo.Sscs");
            DropForeignKey("dbo.DensityCodes",           "SscSupportedMedia_Id",     "dbo.SscSupportedMedias");
            DropForeignKey("dbo.SupportedDensities",     "Ssc_Id",                   "dbo.Sscs");
            DropForeignKey("dbo.TestedMedias",           "Scsi_Id",                  "dbo.Scsis");
            DropForeignKey("dbo.Scsis",                  "ReadCapabilities_Id",      "dbo.TestedMedias");
            DropForeignKey("dbo.Scsis",                  "MultiMediaDevice_Id",      "dbo.Mmcs");
            DropForeignKey("dbo.TestedMedias",           "Mmc_Id",                   "dbo.Mmcs");
            DropForeignKey("dbo.Mmcs",                   "ModeSense2A_Id",           "dbo.ModePage_2A");
            DropForeignKey("dbo.Mmcs",                   "Features_Id",              "dbo.MmcFeatures");
            DropForeignKey("dbo.Scsis",                  "ModeSense_Id",             "dbo.ScsiModes");
            DropForeignKey("dbo.ScsiPages",              "ScsiMode_Id",              "dbo.ScsiModes");
            DropForeignKey("dbo.BlockDescriptors",       "ScsiMode_Id",              "dbo.ScsiModes");
            DropForeignKey("dbo.ScsiPages",              "Scsi_Id",                  "dbo.Scsis");
            DropForeignKey("dbo.Devices",                "PCMCIA_Id",                "dbo.Pcmcias");
            DropForeignKey("dbo.Devices",                "MultiMediaCard_Id",        "dbo.MmcSds");
            DropForeignKey("dbo.Devices",                "FireWire_Id",              "dbo.FireWires");
            DropForeignKey("dbo.Devices",                "ATAPI_Id",                 "dbo.Atas");
            DropForeignKey("dbo.Devices",                "ATA_Id",                   "dbo.Atas");
            DropForeignKey("dbo.TestedMedias",           "Ata_Id",                   "dbo.Atas");
            DropForeignKey("dbo.Atas",                   "ReadCapabilities_Id",      "dbo.TestedMedias");
            DropForeignKey("dbo.TestedMedias",           "CurrentCHS_Id",            "dbo.Chs");
            DropForeignKey("dbo.TestedMedias",           "CHS_Id",                   "dbo.Chs");
            DropIndex("dbo.UploadedReports",        new[] {"USB_Id"});
            DropIndex("dbo.UploadedReports",        new[] {"SecureDigital_Id"});
            DropIndex("dbo.UploadedReports",        new[] {"SCSI_Id"});
            DropIndex("dbo.UploadedReports",        new[] {"PCMCIA_Id"});
            DropIndex("dbo.UploadedReports",        new[] {"MultiMediaCard_Id"});
            DropIndex("dbo.UploadedReports",        new[] {"FireWire_Id"});
            DropIndex("dbo.UploadedReports",        new[] {"ATAPI_Id"});
            DropIndex("dbo.UploadedReports",        new[] {"ATA_Id"});
            DropIndex("dbo.TestedSequentialMedias", new[] {"Ssc_Id"});
            DropIndex("dbo.DensityCodes",           new[] {"SscSupportedMedia_Id"});
            DropIndex("dbo.SscSupportedMedias",     new[] {"TestedSequentialMedia_Id"});
            DropIndex("dbo.SscSupportedMedias",     new[] {"Ssc_Id"});
            DropIndex("dbo.SupportedDensities",     new[] {"TestedSequentialMedia_Id"});
            DropIndex("dbo.SupportedDensities",     new[] {"Ssc_Id"});
            DropIndex("dbo.Mmcs",                   new[] {"ModeSense2A_Id"});
            DropIndex("dbo.Mmcs",                   new[] {"Features_Id"});
            DropIndex("dbo.BlockDescriptors",       new[] {"ScsiMode_Id"});
            DropIndex("dbo.ScsiPages",              new[] {"ScsiMode_Id"});
            DropIndex("dbo.ScsiPages",              new[] {"Scsi_Id"});
            DropIndex("dbo.Scsis",                  new[] {"SequentialDevice_Id"});
            DropIndex("dbo.Scsis",                  new[] {"ReadCapabilities_Id"});
            DropIndex("dbo.Scsis",                  new[] {"MultiMediaDevice_Id"});
            DropIndex("dbo.Scsis",                  new[] {"ModeSense_Id"});
            DropIndex("dbo.TestedMedias",           new[] {"Scsi_Id"});
            DropIndex("dbo.TestedMedias",           new[] {"Mmc_Id"});
            DropIndex("dbo.TestedMedias",           new[] {"Ata_Id"});
            DropIndex("dbo.TestedMedias",           new[] {"CurrentCHS_Id"});
            DropIndex("dbo.TestedMedias",           new[] {"CHS_Id"});
            DropIndex("dbo.Atas",                   new[] {"ReadCapabilities_Id"});
            DropIndex("dbo.Devices",                new[] {"USB_Id"});
            DropIndex("dbo.Devices",                new[] {"SecureDigital_Id"});
            DropIndex("dbo.Devices",                new[] {"SCSI_Id"});
            DropIndex("dbo.Devices",                new[] {"PCMCIA_Id"});
            DropIndex("dbo.Devices",                new[] {"MultiMediaCard_Id"});
            DropIndex("dbo.Devices",                new[] {"FireWire_Id"});
            DropIndex("dbo.Devices",                new[] {"ATAPI_Id"});
            DropIndex("dbo.Devices",                new[] {"ATA_Id"});
            DropTable("dbo.UploadedReports");
            DropTable("dbo.Usbs");
            DropTable("dbo.TestedSequentialMedias");
            DropTable("dbo.DensityCodes");
            DropTable("dbo.SscSupportedMedias");
            DropTable("dbo.SupportedDensities");
            DropTable("dbo.Sscs");
            DropTable("dbo.ModePage_2A");
            DropTable("dbo.MmcFeatures");
            DropTable("dbo.Mmcs");
            DropTable("dbo.BlockDescriptors");
            DropTable("dbo.ScsiModes");
            DropTable("dbo.ScsiPages");
            DropTable("dbo.Scsis");
            DropTable("dbo.Pcmcias");
            DropTable("dbo.MmcSds");
            DropTable("dbo.FireWires");
            DropTable("dbo.Chs");
            DropTable("dbo.TestedMedias");
            DropTable("dbo.Atas");
            DropTable("dbo.Devices");
        }
    }
}