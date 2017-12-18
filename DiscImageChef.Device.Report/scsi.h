//
// Created by claunia on 11/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_SCSI_H
#define DISCIMAGECHEF_DEVICE_REPORT_SCSI_H

#include <stdint.h>

int SendScsiCommand(int fd, void *cdb, unsigned char cdb_len, unsigned char *buffer, unsigned int buffer_len, unsigned char **senseBuffer, int direction);
int Inquiry(int fd, unsigned char **buffer, unsigned char **senseBuffer);
int PreventMediumRemoval(int fd, unsigned char **senseBuffer);
int AllowMediumRemoval(int fd, unsigned char **senseBuffer);
int PreventAllowMediumRemoval(int fd, unsigned char **senseBuffer, int persistent, int prevent);
int LoadTray(int fd, unsigned char **senseBuffer);
int EjectTray(int fd, unsigned char **senseBuffer);
int StartUnit(int fd, unsigned char **senseBuffer);
int StopUnit(int fd, unsigned char **senseBuffer);
int StartStopUnit(int fd, unsigned char **senseBuffer, int immediate, uint8_t formatLayer, uint8_t powerConditions, int changeFormatLayer, int loadEject, int start);
int SpcPreventMediumRemoval(int fd, unsigned char **senseBuffer);
int SpcAllowMediumRemoval(int fd, unsigned char **senseBuffer);
int SpcPreventAllowMediumRemoval(int fd, unsigned char **senseBuffer, uint8_t preventMode);
int Load(int fd, unsigned char **senseBuffer);
int Unload(int fd, unsigned char **senseBuffer);
int LoadUnload(int fd, unsigned char **senseBuffer, int immediate, int load, int retense, int endOfTape, int hold);
int ModeSense6(int fd, unsigned char **buffer, unsigned char **senseBuffer, int DBD, uint8_t pageControl, uint8_t pageCode, uint8_t subPageCode);
int ModeSense10(int fd, unsigned char **buffer, unsigned char **senseBuffer, int LLBAA, int DBD, uint8_t pageContorl, uint8_t pageCode, uint8_t subPageCode);
int ReadCapacity(int fd, unsigned char **buffer, unsigned char **senseBuffer, int RelAddr, uint32_t address, int PMI);
int ReadCapacity16(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint64_t address, int PMI);
int Read6(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t blockSize, uint8_t transferLength);
int Read10(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint8_t rdprotect, int dpo, int fua, int fuaNv, int relAddr, uint32_t lba, uint32_t blockSize, uint8_t groupNumber, uint16_t transferLength);
int Read12(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint8_t rdprotect, int dpo, int fua, int fuaNv, int relAddr, uint32_t lba, uint32_t blockSize, uint8_t groupNumber, uint32_t transferLength, int streaming);
int Read16(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint8_t rdprotect, int dpo, int fua, int fuaNv, uint64_t lba, uint32_t blockSize, uint8_t groupNumber, uint32_t transferLength, int streaming);
int ReadLong10(int fd, unsigned char **buffer, unsigned char **senseBuffer, int correct, int relAddr, uint32_t lba, uint16_t transferBytes);
int ReadLong16(int fd, unsigned char **buffer, unsigned char **senseBuffer, int correct, uint64_t lba, uint32_t transferBytes);
int Seek6(int fd, unsigned char **senseBuffer, uint32_t lba);
int Seek10(int fd, unsigned char **senseBuffer, uint32_t lba);
int TestUnitReady(int fd, unsigned char **senseBuffer);
int GetConfiguration(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint16_t startingFeatureNumber, uint8_t RT);
int ReadTocPmaAtip(int fd, unsigned char **buffer, unsigned char **senseBuffer, int MSF, uint8_t format, uint8_t trackSessionNumber);
int ReadDiscStructure(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint8_t mediaType, uint32_t address, uint8_t layerNumber, uint8_t format, uint8_t AGID);
int ReadCd(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t blockSize, uint32_t transferLength, uint8_t expectedSectorType, int DAP, int relAddr, int sync, uint8_t headerCodes, int userData, int edcEcc, uint8_t C2Error, uint8_t subchannel);
int ReadCdMsf(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t startMsf, uint32_t endMsf, uint32_t blockSize, uint8_t expectedSectorType, int DAP, int sync, uint8_t headerCodes, int userData, int edcEcc, uint8_t C2Error, uint8_t subchannel);
int PlextorReadCdDa(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t blockSize, uint32_t transferLength, uint8_t subchannel);
int PlextorReadRawDvd(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t transferLength);
int PioneerReadCdDa(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t blockSize, uint32_t transferLength, uint8_t subchannel);
int PioneerReadCdDaMsf(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t startMsf, uint32_t endMsf, uint32_t blockSize, uint8_t subchannel);
int NecReadCdDa(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t transferLength);
int HlDtStReadRawDvd(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t transferLength);
int ReadBlockLimits(int fd, unsigned char **buffer, unsigned char **senseBuffer);
int ReportDensitySupport(int fd, unsigned char **buffer, unsigned char **senseBuffer, int mediumType, int currentMedia);
int ReadMediaSerialNumber(int fd, unsigned char **buffer, unsigned char **senseBuffer);

typedef enum
{
    SCSI_TEST_UNIT_READY = 0x00,
    SCSI_READ_BLOCK_LIMITS = 0x05,
    SCSI_READ = 0x08,
    SCSI_SEEK = 0x0B,
    SCSI_INQUIRY = 0x12,
    SCSI_START_STOP_UNIT = 0x1B,
    SCSI_LOAD_UNLOAD = SCSI_START_STOP_UNIT,
    SCSI_MODE_SENSE = 0x1A,
    SCSI_PREVENT_ALLOW_MEDIUM_REMOVAL = 0x1E,
    SCSI_READ_CAPACITY = 0x25,
    SCSI_READ_10 = 0x28,
    SCSI_READ_LONG = 0x3E,
    SCSI_SEEK_10 = 0x2B,
    SCSI_READ_BUFFER = 0x3C,
    MMC_READ_TOC_PMA_ATIP = 0x43,
    SCSI_REPORT_DENSITY_SUPPORT = 0x44,
    MMC_GET_CONFIGURATION = 0x46,
    SCSI_MODE_SENSE_10 = 0x5A,
    SCSI_ATA_PASSTHROUGH_16 = 0x85,
    SCSI_READ_16 = 0x88,
    SCSI_SERVICE_ACTION_IN = 0x9E,
    SCSI_READ_12 = 0xA8,
    SCSI_READ_MEDIA_SERIAL = 0xAB,
    MMC_READ_DISC_STRUCTURE = 0xAD,
    MMC_READ_CD_MSF = 0xB9,
    MMC_READ_CD = 0xBE,
    NEC_READ_CDDA = 0xD4,
    PIONEER_READ_CDDA = 0xD8,
    PIONEER_READ_CDDA_MSF = 0xD9,
    HLDTST_VENDOR = 0xE7,
} ScsiCommands;

typedef enum
{
    MODE_PAGE_CURRENT = 0x00,
    MODE_PAGE_CHANGEABLE = 0x40,
    MODE_PAGE_DEFAULT = 0x80,
    MODE_PAGE_SAVED = 0xC0
} ScsiModeSensePageControl;

typedef enum
{
    SCSI_READ_CAPACITY_16 = 0x10,
    SCSI_READ_LONG_16 = 0x11,
} ScsiServiceActionIn;

typedef enum
{
    DISC_STRUCTURE_DVD = 0x00,
    DISC_STRUCTURE_BD = 0x01,
} MmcDiscStructureMediaType;

// TODO: Stylize this
typedef enum
{
    // Generic Format Codes

    /// <summary>
    /// AACS Volume Identifier
    /// </summary>
            DISC_STRUCTURE_AACSVolId = 0x80,
    /// <summary>
    /// AACS Pre-recorded Media Serial Number
    /// </summary>
            DISC_STRUCTURE_AACSMediaSerial = 0x81,
    /// <summary>
    /// AACS Media Identifier
    /// </summary>
            DISC_STRUCTURE_AACSMediaId = 0x82,
    /// <summary>
    /// AACS Lead-in Media Key Block
    /// </summary>
            DISC_STRUCTURE_AACSMKB = 0x83,
    /// <summary>
    /// AACS Data Keys
    /// </summary>
            DISC_STRUCTURE_AACSDataKeys = 0x84,
    /// <summary>
    /// AACS LBA extents
    /// </summary>
            DISC_STRUCTURE_AACSLBAExtents = 0x85,
    /// <summary>
    /// CPRM Media Key Block specified by AACS
    /// </summary>
            DISC_STRUCTURE_AACSMKBCPRM = 0x86,
    /// <summary>
    /// Recognized format layers
    /// </summary>
            DISC_STRUCTURE_RecognizedFormatLayers = 0x90,
    /// <summary>
    /// Write protection status
    /// </summary>
            DISC_STRUCTURE_WriteProtectionStatus = 0xC0,
    /// <summary>
    /// READ/SEND DISC STRUCTURE capability list
    /// </summary>
            DISC_STRUCTURE_CapabilityList = 0xFF,

    // DVD Disc Structures
    /// <summary>
    /// DVD Lead-in Physical Information
    /// </summary>
            DISC_STRUCTURE_PhysicalInformation = 0x00,
    /// <summary>
    /// DVD Lead-in Copyright Information
    /// </summary>
            DISC_STRUCTURE_CopyrightInformation = 0x01,
    /// <summary>
    /// CSS/CPPM Disc key
    /// </summary>
            DISC_STRUCTURE_DiscKey = 0x02,
    /// <summary>
    /// DVD Burst Cutting Area
    /// </summary>
            DISC_STRUCTURE_BurstCuttingArea = 0x03,
    /// <summary>
    /// DVD Lead-in Disc Manufacturing Information
    /// </summary>
            DISC_STRUCTURE_DiscManufacturingInformation = 0x04,
    /// <summary>
    /// DVD Copyright Information from specified sector
    /// </summary>
            DISC_STRUCTURE_SectorCopyrightInformation = 0x05,
    /// <summary>
    /// CSS/CPPM Media Identifier
    /// </summary>
            DISC_STRUCTURE_MediaIdentifier = 0x06,
    /// <summary>
    /// CSS/CPPM Media Key Block
    /// </summary>
            DISC_STRUCTURE_MediaKeyBlock = 0x07,
    /// <summary>
    /// DDS from DVD-RAM
    /// </summary>
            DISC_STRUCTURE_DVDRAM_DDS = 0x08,
    /// <summary>
    /// DVD-RAM Medium Status
    /// </summary>
            DISC_STRUCTURE_DVDRAM_MediumStatus = 0x09,
    /// <summary>
    /// DVD-RAM Spare Area Information
    /// </summary>
            DISC_STRUCTURE_DVDRAM_SpareAreaInformation = 0x0A,
    /// <summary>
    /// DVD-RAM Recording Type Information
    /// </summary>
            DISC_STRUCTURE_DVDRAM_RecordingType = 0x0B,
    /// <summary>
    /// DVD-R/-RW RMD in last Border-out
    /// </summary>
            DISC_STRUCTURE_LastBorderOutRMD = 0x0C,
    /// <summary>
    /// Specified RMD from last recorded Border-out
    /// </summary>
            DISC_STRUCTURE_SpecifiedRMD = 0x0D,
    /// <summary>
    /// DVD-R/-RW Lead-in pre-recorded information
    /// </summary>
            DISC_STRUCTURE_PreRecordedInfo = 0x0E,
    /// <summary>
    /// DVD-R/-RW Media Identifier
    /// </summary>
            DISC_STRUCTURE_DVDR_MediaIdentifier = 0x0F,
    /// <summary>
    /// DVD-R/-RW Physical Format Information
    /// </summary>
            DISC_STRUCTURE_DVDR_PhysicalInformation = 0x10,
    /// <summary>
    /// ADIP
    /// </summary>
            DISC_STRUCTURE_ADIP = 0x11,
    /// <summary>
    /// HD DVD Lead-in Copyright Protection Information
    /// </summary>
            DISC_STRUCTURE_HDDVD_CopyrightInformation = 0x12,
    /// <summary>
    /// AACS Lead-in Copyright Data Section
    /// </summary>
            DISC_STRUCTURE_DVD_AACS = 0x15,
    /// <summary>
    /// HD DVD-R Medium Status
    /// </summary>
            DISC_STRUCTURE_HDDVDR_MediumStatus = 0x19,
    /// <summary>
    /// HD DVD-R Last recorded RMD in the latest RMZ
    /// </summary>
            DISC_STRUCTURE_HDDVDR_LastRMD = 0x1A,
    /// <summary>
    /// DVD+/-R DL and DVD-Download DL layer capacity
    /// </summary>
            DISC_STRUCTURE_DVDR_LayerCapacity = 0x20,
    /// <summary>
    /// DVD-R DL Middle Zone start address
    /// </summary>
            DISC_STRUCTURE_MiddleZoneStart = 0x21,
    /// <summary>
    /// DVD-R DL Jump Interval Size
    /// </summary>
            DISC_STRUCTURE_JumpIntervalSize = 0x22,
    /// <summary>
    /// DVD-R DL Start LBA of the manual layer jump
    /// </summary>
            DISC_STRUCTURE_ManualLayerJumpStartLBA = 0x23,
    /// <summary>
    /// DVD-R DL Remapping information of the specified Anchor Point
    /// </summary>
            DISC_STRUCTURE_RemapAnchorPoint = 0x24,
    /// <summary>
    /// Disc Control Block
    /// </summary>
            DISC_STRUCTURE_DCB = 0x30,

    // BD Disc Structures
    /// <summary>
    /// Blu-ray Disc Information
    /// </summary>
            DISC_STRUCTURE_DiscInformation = 0x00,
    /// <summary>
    /// Blu-ray Burst Cutting Area
    /// </summary>
            DISC_STRUCTURE_BD_BurstCuttingArea = 0x03,
    /// <summary>
    /// Blu-ray DDS
    /// </summary>
            DISC_STRUCTURE_BD_DDS = 0x08,
    /// <summary>
    /// Blu-ray Cartridge Status
    /// </summary>
            DISC_STRUCTURE_CartridgeStatus = 0x09,
    /// <summary>
    /// Blu-ray Spare Area Information
    /// </summary>
            DISC_STRUCTURE_BD_SpareAreaInformation = 0x0A,
    /// <summary>
    /// Unmodified DFL
    /// </summary>
            DISC_STRUCTURE_RawDFL = 0x12,
    /// <summary>
    /// Physical Access Control
    /// </summary>
            DISC_STRUCTURE_PAC = 0x30
} MmcDiscStructureFormat;

typedef enum
{
    MMC_SECTOR_ALL = 0,
    MMC_SECTOR_CDDA = 1,
    MMC_SECTOR_MODE1 = 2,
    MMC_SECTOR_MODE2 = 3,
    MMC_SECTOR_MODE2F1 = 4,
    MMC_SECTOR_MODE2F2 = 5
} MmcSectorTypes;

typedef enum
{
    MMC_HEADER_NONE = 0,
    MMC_HEADER_ONLY = 1,
    MMC_SUBHEADER_ONLY = 2,
    MMC_HEADER_ALL = 3
} MmcHeaderCodes;

typedef enum
{
    MMC_ERROR_NONE = 0,
    MMC_ERROR_C2 = 1,
    MMC_ERROR_C2_AND_BLOCK = 2
} MmcErrorField;

typedef enum
{
    MMC_SUBCHANNEL_NONE = 0,
    MMC_SUBCHANNEL_RAW = 1,
    MMC_SUBCHANNEL_Q16 = 2,
    MMC_SUBCHANNEL_RW = 4
} MmcSubchannel;

typedef enum
{
    PIONEER_SUBCHANNEL_NONE = 0,
    PIONEER_SUBCHANNEL_Q16 = 1,
    PIONEER_SUBCHANNEL_ALL = 2,
    PIONEER_SUBCHANNEL_ONLY = 3
} PioneerSubchannel;

typedef enum
{
    PLEXTOR_SUBCHANNEL_NONE = 0,
    PLEXTOR_SUBCHANNEL_Q16 = 1,
    PLEXTOR_SUBCHANNEL_PACK = 2,
    PLEXTOR_SUBCHANNEL_ALL = 3,
    PLEXTOR_SUBCHANNEL_RAW_C2 = 8
} PlextorSubchannel;

// SCSI INQUIRY command response
#pragma pack(push, 1)
typedef struct
{
    /// <summary>
    /// Peripheral device type
    /// Byte 0, bits 4 to 0
    /// </summary>
    uint8_t PeripheralDeviceType : 5;
    /// <summary>
    /// Peripheral qualifier
    /// Byte 0, bits 7 to 5
    /// </summary>
    uint8_t PeripheralQualifier : 3;
    /// <summary>
    /// SCSI-1 vendor-specific qualification codes
    /// Byte 1, bits 6 to 0
    /// </summary>
    uint8_t DeviceTypeModifier : 7;
    /// <summary>
    /// Removable device
    /// Byte 1, bit 7
    /// </summary>
    uint8_t RMB : 1;
    /// <summary>
    /// ANSI SCSI Standard Version
    /// Byte 2, bits 2 to 0, mask = 0x07
    /// </summary>
    uint8_t ANSIVersion : 3;
    /// <summary>
    /// ECMA SCSI Standard Version
    /// Byte 2, bits 5 to 3, mask = 0x38, >> 3
    /// </summary>
    uint8_t ECMAVersion : 3;
    /// <summary>
    /// ISO/IEC SCSI Standard Version
    /// Byte 2, bits 7 to 6, mask = 0xC0, >> 6
    /// </summary>
    uint8_t ISOVersion : 2;
    /// <summary>
    /// Responde data format
    /// Byte 3, bit 3 to 0
    /// </summary>
    uint8_t ResponseDataFormat : 4;
    /// <summary>
    /// Supports LUN hierarchical addressing
    /// Byte 3, bit 4
    /// </summary>
    uint8_t HiSup : 1;
    /// <summary>
    /// Supports setting Normal ACA
    /// Byte 3, bit 5
    /// </summary>
    uint8_t NormACA : 1;
    /// <summary>
    /// Device supports TERMINATE TASK command
    /// Byte 3, bit 6
    /// </summary>
    uint8_t TrmTsk : 1;
    /// <summary>
    /// Asynchronous Event Reporting Capability supported
    /// Byte 3, bit 7
    /// </summary>
    uint8_t AERC : 1;
    /// <summary>
    /// Lenght of total INQUIRY response minus 4
    /// Byte 4
    /// </summary>
    uint8_t AdditionalLength;
    /// <summary>
    /// Supports protection information
    /// Byte 5, bit 0
    /// </summary>
    uint8_t Protect : 1;
    /// <summary>
    /// Reserved
    /// Byte 5, bits 2 to 1
    /// </summary>
    uint8_t Reserved2 : 2;
    /// <summary>
    /// Supports third-party copy commands
    /// Byte 5, bit 3
    /// </summary>
    uint8_t ThreePC : 1;
    /// <summary>
    /// Supports asymetrical logical unit access
    /// Byte 5, bits 5 to 4
    /// </summary>
    uint8_t TPGS : 2;
    /// <summary>
    /// Device contains an Access Control Coordinator
    /// Byte 5, bit 6
    /// </summary>
    uint8_t ACC : 1;
    /// <summary>
    /// Device contains an embedded storage array controller
    /// Byte 5, bit 7
    /// </summary>
    uint8_t SCCS : 1;
    /// <summary>
    /// Supports 16-bit wide SCSI addresses
    /// Byte 6, bit 0
    /// </summary>
    uint8_t Addr16 : 1;
    /// <summary>
    /// Supports 32-bit wide SCSI addresses
    /// Byte 6, bit 1
    /// </summary>
    uint8_t Addr32 : 1;
    /// <summary>
    /// Device supports request and acknowledge handshakes
    /// Byte 6, bit 2
    /// </summary>
    uint8_t ACKREQQ : 1;
    /// <summary>
    /// Device contains or is attached to a medium changer
    /// Byte 6, bit 3
    /// </summary>
    uint8_t MChngr : 1;
    /// <summary>
    /// Multi-port device
    /// Byte 6, bit 4
    /// </summary>
    uint8_t MultiP : 1;
    /// <summary>
    /// Vendor-specific
    /// Byte 6, bit 5
    /// </summary>
    uint8_t VS1 : 1;
    /// <summary>
    /// Device contains an embedded enclosure services component
    /// Byte 6, bit 6
    /// </summary>
    uint8_t EncServ : 1;
    /// <summary>
    /// Supports basic queueing
    /// Byte 6, bit 7
    /// </summary>
    uint8_t BQue : 1;
    /// <summary>
    /// Indicates that the devices responds to RESET with soft reset
    /// Byte 7, bit 0
    /// </summary>
    uint8_t SftRe : 1;
    /// <summary>
    /// Supports TCQ queue
    /// Byte 7, bit 1
    /// </summary>
    uint8_t CmdQue : 1;
    /// <summary>
    /// Supports CONTINUE TASK and TARGET TRANSFER DISABLE commands
    /// Byte 7, bit 2
    /// </summary>
    uint8_t TranDis : 1;
    /// <summary>
    /// Supports linked commands
    /// Byte 7, bit 3
    /// </summary>
    uint8_t Linked : 1;
    /// <summary>
    /// Supports synchronous data transfer
    /// Byte 7, bit 4
    /// </summary>
    uint8_t Sync : 1;
    /// <summary>
    /// Supports 16-bit wide data transfers
    /// Byte 7, bit 5
    /// </summary>
    uint8_t WBus16 : 1;
    /// <summary>
    /// Supports 32-bit wide data transfers
    /// Byte 7, bit 6
    /// </summary>
    uint8_t WBus32 : 1;
    /// <summary>
    /// Device supports relative addressing
    /// Byte 7, bit 7
    /// </summary>
    uint8_t RelAddr : 1;
    /// <summary>
    /// Vendor identification
    /// Bytes 8 to 15
    /// </summary>
    uint8_t VendorIdentification[8];
    /// <summary>
    /// Product identification
    /// Bytes 16 to 31
    /// </summary>
    uint8_t ProductIdentification[16];
    /// <summary>
    /// Product revision level
    /// Bytes 32 to 35
    /// </summary>
    uint8_t ProductRevisionLevel[4];
    /// <summary>
    /// Vendor-specific data
    /// Bytes 36 to 55
    /// </summary>
    uint8_t VendorSpecific[20];
    /// <summary>
    /// Supports information unit transfers
    /// Byte 56, bit 0
    /// </summary>
    uint8_t IUS : 1;
    /// <summary>
    /// Device supports Quick Arbitration and Selection
    /// Byte 56, bit 1
    /// </summary>
    uint8_t QAS : 1;
    /// <summary>
    /// Supported SPI clocking
    /// Byte 56, bits 3 to 2
    /// </summary>
    uint8_t Clocking : 2;
    /// <summary>
    /// Byte 56, bits 7 to 4
    /// </summary>
    uint8_t Reserved3 : 4;
    /// <summary>
    /// Reserved
    /// Byte 57
    /// </summary>
    uint8_t Reserved4;
    /// <summary>
    /// Array of version descriptors
    /// Bytes 58 to 73
    /// </summary>
    uint16_t VersionDescriptors[8];
    /// <summary>
    /// Reserved
    /// Bytes 74 to 95
    /// </summary>
    uint8_t Reserved5[22];
    /// <summary>
    /// Reserved
    /// Bytes 96 to end
    /// </summary>
    uint8_t VendorSpecific2;
} ScsiInquiry;
#pragma pack(pop)

#endif //DISCIMAGECHEF_DEVICE_REPORT_SCSI_H
