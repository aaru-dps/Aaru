//
// Created by claunia on 11/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_ATA_H
#define DISCIMAGECHEF_DEVICE_REPORT_ATA_H

#pragma pack(1)
typedef struct
{
    uint8_t feature;
    uint8_t sectorCount;
    uint8_t sector;
    uint8_t cylinderLow;
    uint8_t cylinderHigh;
    uint8_t deviceHead;
    uint8_t command;
} AtaRegistersCHS;

typedef struct
{
    uint8_t feature;
    uint8_t sectorCount;
    uint8_t lbaLow;
    uint8_t lbaMid;
    uint8_t lbaHigh;
    uint8_t deviceHead;
    uint8_t command;
} AtaRegistersLBA28;

typedef struct
{
    uint16_t feature;
    uint16_t sectorCount;
    uint16_t lbaLow;
    uint16_t lbaMid;
    uint16_t lbaHigh;
    uint8_t  deviceHead;
    uint8_t  command;
} AtaRegistersLBA48;

typedef struct
{
    uint8_t status;
    uint8_t error;
    uint8_t sectorCount;
    uint8_t sector;
    uint8_t cylinderLow;
    uint8_t cylinderHigh;
    uint8_t deviceHead;
    uint8_t command;
} AtaErrorRegistersCHS;

typedef struct
{
    uint8_t status;
    uint8_t error;
    uint8_t sectorCount;
    uint8_t lbaLow;
    uint8_t lbaMid;
    uint8_t lbaHigh;
    uint8_t deviceHead;
    uint8_t command;
} AtaErrorRegistersLBA28;

typedef struct
{
    uint8_t  status;
    uint8_t  error;
    uint16_t sectorCount;
    uint16_t lbaLow;
    uint16_t lbaMid;
    uint16_t lbaHigh;
    uint8_t  deviceHead;
    uint8_t  command;
} AtaErrorRegistersLBA48;

typedef enum
{
    ATA_TRANSFER_NONE = 0, ATA_TRANSFER_FEATURE, ATA_TRANSFER_SECTORCOUNT, ATA_TRANSFTER_SPTSIU
} AtaTransferRegister;

typedef enum
{
    ATA_PROTOCOL_HARD_RESET         = 0,
    ATA_PROTOCOL_SOFT_RESET         = 1,
    ATA_PROTOCOL_NO_DATA            = 3,
    ATA_PROTOCOL_PIO_IN             = 4,
    ATA_PROTOCOL_PIO_OUT            = 5,
    ATA_PROTOCOL_DMA                = 6,
    ATA_PROTOCOL_DMA_QUEUED         = 7,
    ATA_PROTOCOL_DEVICE_DIAGNOSTICS = 8,
    ATA_PROTOCOL_DEVICE_RESET       = 9,
    ATA_PROTOCOL_UDMA_IN            = 10,
    ATA_PROTOCOL_UDMA_OUT           = 11,
    ATA_PROTOCOL_FPDMA              = 12,
    ATA_PROTOCOL_RETURN_RESPONSE    = 15
} AtaProtocol;

typedef enum
{
    ATA_READ_RETRY             = 0x20,
    ATA_READ_SECTORS           = 0x21,
    ATA_READ_LONG_RETRY        = 0x22,
    ATA_READ_LONG              = 0x23,
    ATA_READ_EXT               = 0x24,
    ATA_READ_DMA_EXT           = 0x25,
    ATA_SEEK                   = 0x70,
    ATA_READ_DMA_RETRY         = 0xC8,
    ATA_READ_DMA               = 0xC9,
    ATA_IDENTIFY_PACKET_DEVICE = 0xA1,
    ATA_IDENTIFY_DEVICE        = 0xEC
} AtaCommands;

typedef struct
{
    /*
       Word 0
       General device configuration
       On ATAPI devices:
       Bits 12 to 8 indicate device type as SCSI defined
       Bits 6 to 5:
       0 = Device shall set DRQ within 3 ms of receiving PACKET
       1 = Device shall assert INTRQ when DRQ is set to one
       2 = Device shall set DRQ within 50 µs of receiving PACKET
       Bits 1 to 0:
       0 = 12 byte command packet
       1 = 16 byte command packet
       CompactFlash is 0x848A (non magnetic, removable, not MFM, hardsector, and UltraFAST)
    */
    uint16_t GeneralConfiguration;
    /*
       Word 1
       Cylinders in default translation mode
       Obsoleted in ATA/ATAPI-6
    */
    uint16_t Cylinders;
    /*
       Word 2
       Specific configuration
    */
    uint16_t SpecificConfiguration;
    /*
       Word 3
       Heads in default translation mode
       Obsoleted in ATA/ATAPI-6
    */
    uint16_t Heads;
    /*
       Word 4
       Unformatted bytes per track in default translation mode
       Obsoleted in ATA-2
    */
    uint16_t UnformattedBPT;
    /*
       Word 5
       Unformatted bytes per sector in default translation mode
       Obsoleted in ATA-2
    */
    uint16_t UnformattedBPS;
    /*
       Word 6
       Sectors per track in default translation mode
       Obsoleted in ATA/ATAPI-6
    */
    uint16_t SectorsPerTrack;
    /*
       Words 7 to 8
       CFA: Number of sectors per card
    */
    uint32_t SectorsPerCard;
    /*
       Word 9
       Vendor unique
       Obsoleted in ATA/ATAPI-4
    */
    uint16_t VendorWord9;
    /*
       Words 10 to 19
       Device serial number, right justified, padded with spaces
    */
    uint8_t  SerialNumber[20];
    /*
       Word 20
       Manufacturer defined
       Obsoleted in ATA-2
       0x0001 = single ported single sector buffer
       0x0002 = dual ported multi sector buffer
       0x0003 = dual ported multi sector buffer with reading
    */
    uint16_t BufferType;
    /*
       Word 21
       Size of buffer in 512 byte increments
       Obsoleted in ATA-2
    */
    uint16_t BufferSize;
    /*
       Word 22
       Bytes of ECC available in READ/WRITE LONG commands
       Obsoleted in ATA/ATAPI-4
    */
    uint16_t EccBytes;
    /*
       Words 23 to 26
       Firmware revision, left justified, padded with spaces
    */
    uint8_t  FirmwareRevision[8];
    /*
       Words 27 to 46
       Model number, left justified, padded with spaces
    */
    uint8_t  Model[40];
    /*
       Word 47 bits 7 to 0
       Maximum number of sectors that can be transferred per
       interrupt on read and write multiple commands
    */
    uint8_t  MultipleMaxSectors;
    /*
       Word 47 bits 15 to 8
       Vendor unique
       ATA/ATAPI-4 says it must be 0x80
    */
    uint8_t  VendorWord47;
    /*
       Word 48
       ATA-1: Set to 1 if it can perform doubleword I/O
       ATA-2 to ATA/ATAPI-7: Reserved
       ATA8-ACS: Trusted Computing feature set
    */
    uint16_t TrustedComputing;
    /*
       Word 49
       Capabilities
    */
    uint16_t Capabilities;
    /*
       Word 50
       Capabilities
    */
    uint16_t Capabilities2;
    /*
       Word 51 bits 7 to 0
       Vendor unique
       Obsoleted in ATA/ATAPI-4
    */
    uint8_t  VendorWord51;
    /*
       Word 51 bits 15 to 8
       Transfer timing mode in PIO
       Obsoleted in ATA/ATAPI-4
    */
    uint8_t  PIOTransferTimingMode;
    /*
       Word 52 bits 7 to 0
       Vendor unique
       Obsoleted in ATA/ATAPI-4
    */
    uint8_t  VendorWord52;
    /*
       Word 52 bits 15 to 8
       Transfer timing mode in DMA
       Obsoleted in ATA/ATAPI-4
    */
    uint8_t  DMATransferTimingMode;
    /*
       Word 53 bits 7 to 0
       Reports if words 54 to 58 are valid
    */
    uint8_t  ExtendedIdentify;
    /*
       Word 53 bits 15 to 8
       Free-fall Control Sensitivity
    */
    uint8_t  FreeFallSensitivity;
    /*
       Word 54
       Cylinders in current translation mode
       Obsoleted in ATA/ATAPI-6
    */
    uint16_t CurrentCylinders;
    /*
       Word 55
       Heads in current translation mode
       Obsoleted in ATA/ATAPI-6
    */
    uint16_t CurrentHeads;
    /*
       Word 56
       Sectors per track in current translation mode
       Obsoleted in ATA/ATAPI-6
    */
    uint16_t CurrentSectorsPerTrack;
    /*
       Words 57 to 58
       Total sectors currently user-addressable
       Obsoleted in ATA/ATAPI-6
    */
    uint32_t CurrentSectors;
    /*
       Word 59 bits 7 to 0
       Number of sectors currently set to transfer on a READ/WRITE MULTIPLE command
    */
    uint8_t  MultipleSectorNumber;
    /*
       Word 59 bits 15 to 8
       Indicates if <see cref="MultipleSectorNumber"/> is valid
    */
    uint8_t  Capabilities3;
    /*
       Words 60 to 61
       If drive supports LBA, how many sectors are addressable using LBA
    */
    uint32_t LBASectors;
    /*
       Word 62 bits 7 to 0
       Single word DMA modes available
       Obsoleted in ATA/ATAPI-4
       In ATAPI it's not obsolete, indicates UDMA mode (UDMA7 is instead MDMA0)
    */
    uint8_t  DMASupported;
    /*
       Word 62 bits 15 to 8
       Single word DMA mode currently active
       Obsoleted in ATA/ATAPI-4
       In ATAPI it's not obsolete, bits 0 and 1 indicate MDMA mode+1,
       bit 10 indicates DMA is supported and bit 15 indicates DMADIR bit
       in PACKET is required for DMA transfers
    */
    uint8_t  DMAActive;
    /*
       Word 63 bits 7 to 0
       Multiword DMA modes available
    */
    uint8_t  MDMASupported;
    /*
       Word 63 bits 15 to 8
       Multiword DMA mode currently active
    */
    uint8_t  MDMAActive;

    /*
       Word 64 bits 7 to 0
       Supported Advanced PIO transfer modes
    */
    uint8_t  APIOSupported;
    /*
       Word 64 bits 15 to 8
       Reserved
    */
    uint8_t  ReservedWord64;
    /*
       Word 65
       Minimum MDMA transfer cycle time per word in nanoseconds
    */
    uint16_t MinMDMACycleTime;
    /*
       Word 66
       Recommended MDMA transfer cycle time per word in nanoseconds
    */
    uint16_t RecMDMACycleTime;
    /*
       Word 67
       Minimum PIO transfer cycle time without flow control in nanoseconds
    */
    uint16_t MinPIOCycleTimeNoFlow;
    /*
       Word 68
       Minimum PIO transfer cycle time with IORDY flow control in nanoseconds
    */
    uint16_t MinPIOCycleTimeFlow;

    /*
       Word 69
       Additional supported
    */
    uint16_t CommandSet5;
    /*
       Word 70
       Reserved
    */
    uint16_t ReservedWord70;
    /*
       Word 71
       ATAPI: Typical time in ns from receipt of PACKET to release bus
    */
    uint16_t PacketBusRelease;
    /*
       Word 72
       ATAPI: Typical time in ns from receipt of SERVICE to clear BSY
    */
    uint16_t ServiceBusyClear;
    /*
       Word 73
       Reserved
    */
    uint16_t ReservedWord73;
    /*
       Word 74
       Reserved
    */
    uint16_t ReservedWord74;

    /*
       Word 75
       Maximum Queue depth
    */
    uint16_t MaxQueueDepth;

    /*
       Word 76
       Serial ATA Capabilities
    */
    uint16_t SATACapabilities;
    /*
       Word 77
       Serial ATA Additional Capabilities
    */
    uint16_t SATACapabilities2;

    /*
       Word 78
       Supported Serial ATA features
    */
    uint16_t SATAFeatures;
    /*
       Word 79
       Enabled Serial ATA features
    */
    uint16_t EnabledSATAFeatures;

    /*
       Word 80
       Major version of ATA/ATAPI standard supported
    */
    uint16_t MajorVersion;
    /*
       Word 81
       Minimum version of ATA/ATAPI standard supported
    */
    uint16_t MinorVersion;

    /*
       Word 82
       Supported command/feature sets
    */
    uint16_t CommandSet;
    /*
       Word 83
       Supported command/feature sets
    */
    uint16_t CommandSet2;
    /*
       Word 84
       Supported command/feature sets
    */
    uint16_t CommandSet3;

    /*
       Word 85
       Enabled command/feature sets
    */
    uint16_t EnabledCommandSet;
    /*
       Word 86
       Enabled command/feature sets
    */
    uint16_t EnabledCommandSet2;
    /*
       Word 87
       Enabled command/feature sets
    */
    uint16_t EnabledCommandSet3;

    /*
       Word 88 bits 7 to 0
       Supported Ultra DMA transfer modes
    */
    uint8_t UDMASupported;
    /*
       Word 88 bits 15 to 8
       Selected Ultra DMA transfer modes
    */
    uint8_t UDMAActive;

    /*
       Word 89
       Time required for security erase completion
    */
    uint16_t SecurityEraseTime;
    /*
       Word 90
       Time required for enhanced security erase completion
    */
    uint16_t EnhancedSecurityEraseTime;
    /*
       Word 91
       Current advanced power management value
    */
    uint16_t CurrentAPM;

    /*
       Word 92
       Master password revision code
    */
    uint16_t MasterPasswordRevisionCode;
    /*
       Word 93
       Hardware reset result
    */
    uint16_t HardwareResetResult;

    /*
       Word 94 bits 7 to 0
       Current AAM value
    */
    uint8_t CurrentAAM;
    /*
       Word 94 bits 15 to 8
       Vendor's recommended AAM value
    */
    uint8_t RecommendedAAM;

    /*
       Word 95
       Stream minimum request size
    */
    uint16_t StreamMinReqSize;
    /*
       Word 96
       Streaming transfer time in DMA
    */
    uint16_t StreamTransferTimeDMA;
    /*
       Word 97
       Streaming access latency in DMA and PIO
    */
    uint16_t StreamAccessLatency;
    /*
       Words 98 to 99
       Streaming performance granularity
    */
    uint32_t StreamPerformanceGranularity;

    /*
       Words 100 to 103
       48-bit LBA addressable sectors
    */
    uint64_t LBA48Sectors;

    /*
       Word 104
       Streaming transfer time in PIO
    */
    uint16_t StreamTransferTimePIO;

    /*
       Word 105
       Maximum number of 512-byte block per DATA SET MANAGEMENT command
    */
    uint16_t DataSetMgmtSize;

    /*
       Word 106
       Bit 15 should be zero
       Bit 14 should be one
       Bit 13 set indicates device has multiple logical sectors per physical sector
       Bit 12 set indicates logical sector has more than 256 words (512 bytes)
       Bits 11 to 4 are reserved
       Bits 3 to 0 indicate power of two of logical sectors per physical sector
    */
    uint16_t PhysLogSectorSize;

    /*
       Word 107
       Interseek delay for ISO-7779 acoustic testing, in microseconds
    */
    uint16_t InterseekDelay;

    /*
       Words 108 to 111
       World Wide Name
    */
    uint64_t WWN;

    /*
       Words 112 to 115
       Reserved for WWN extension to 128 bit
    */
    uint64_t WWNExtension;

    /*
       Word 116
       Reserved for technical report
    */
    uint16_t ReservedWord116;

    /*
       Words 117 to 118
       Words per logical sector
    */
    uint32_t LogicalSectorWords;

    /*
       Word 119
       Supported command/feature sets
    */
    uint16_t CommandSet4;
    /*
       Word 120
       Supported command/feature sets
    */
    uint16_t EnabledCommandSet4;

    /*
       Words 121 to 125
       Reserved
    */
    uint16_t ReservedWords121[5];

    /*
       Word 126
       ATAPI byte count limit
    */
    uint16_t ATAPIByteCount;

    /*
       Word 127
       Removable Media Status Notification feature set support
       Bits 15 to 2 are reserved
       Bits 1 to 0 must be 0 for not supported or 1 for supported. 2 and 3 are reserved.
       Obsoleted in ATA8-ACS
    */
    uint16_t RemovableStatusSet;

    /*
       Word 128
       Security status
    */
    uint16_t SecurityStatus;

    /*
       Words 129 to 159
    */
    uint16_t ReservedWords129[31];

    /*
       Word 160
       CFA power mode
       Bit 15 must be set
       Bit 13 indicates mode 1 is required for one or more commands
       Bit 12 indicates mode 1 is disabled
       Bits 11 to 0 indicates maximum current in mA
    */
    uint16_t CFAPowerMode;

    /*
       Words 161 to 167
       Reserved for CFA
    */
    uint16_t ReservedCFA[7];

    /*
       Word 168
       Bits 15 to 4, reserved
       Bits 3 to 0, device nominal form factor
    */
    uint16_t DeviceFormFactor;
    /*
       Word 169
       DATA SET MANAGEMENT support
    */
    uint16_t DataSetMgmt;
    /*
       Words 170 to 173
       Additional product identifier
    */
    uint8_t  AdditionalPID[8];

    /*
       Word 174
       Reserved
    */
    uint16_t ReservedWord174;
    /*
       Word 175
       Reserved
    */
    uint16_t ReservedWord175;

    /*
       Words 176 to 195
       Current media serial number
    */
    uint8_t MediaSerial[40];
    /*
       Words 196 to 205
       Current media manufacturer
    */
    uint8_t MediaManufacturer[20];

    /*
       Word 206
       SCT Command Transport features
    */
    uint16_t SCTCommandTransport;

    /*
       Word 207
       Reserved for CE-ATA
    */
    uint16_t ReservedCEATAWord207;
    /*
       Word 208
       Reserved for CE-ATA
    */
    uint16_t ReservedCEATAWord208;

    /*
       Word 209
       Alignment of logical block within a larger physical block
       Bit 15 shall be cleared to zero
       Bit 14 shall be set to one
       Bits 13 to 0 indicate logical sector offset within the first physical sector
    */
    uint16_t LogicalAlignment;

    /*
       Words 210 to 211
       Write/Read/Verify sector count mode 3 only
    */
    uint32_t WRVSectorCountMode3;
    /*
       Words 212 to 213
       Write/Read/Verify sector count mode 2 only
    */
    uint32_t WRVSectorCountMode2;

    /*
       Word 214
       NV Cache capabilities
       Bits 15 to 12 feature set version
       Bits 11 to 18 power mode feature set version
       Bits 7 to 5 reserved
       Bit 4 feature set enabled
       Bits 3 to 2 reserved
       Bit 1 power mode feature set enabled
       Bit 0 power mode feature set supported
    */
    uint16_t NVCacheCaps;
    /*
       Words 215 to 216
       NV Cache Size in Logical BLocks
    */
    uint32_t NVCacheSize;
    /*
       Word 217
       Nominal media rotation rate
       In ACS-1 meant NV Cache read speed in MB/s
    */
    uint16_t NominalRotationRate;
    /*
       Word 218
       NV Cache write speed in MB/s
       Reserved since ACS-2
    */
    uint16_t NVCacheWriteSpeed;
    /*
       Word 219 bits 7 to 0
       Estimated device spin up in seconds
    */
    uint8_t  NVEstimatedSpinUp;
    /*
       Word 219 bits 15 to 8
       NV Cache reserved
    */
    uint8_t  NVReserved;

    /*
       Word 220 bits 7 to 0
       Write/Read/Verify feature set current mode
    */
    uint8_t WRVMode;
    /*
       Word 220 bits 15 to 8
       Reserved
    */
    uint8_t WRVReserved;

    /*
       Word 221
       Reserved
    */
    uint16_t ReservedWord221;

    /*
       Word 222
       Transport major revision number
       Bits 15 to 12 indicate transport type. 0 parallel, 1 serial, 0xE PCIe.
       Bits 11 to 0 indicate revision
    */
    uint16_t TransportMajorVersion;
    /*
       Word 223
       Transport minor revision number
    */
    uint16_t TransportMinorVersion;

    /*
       Words 224 to 229
       Reserved for CE-ATA
    */
    uint16_t ReservedCEATA224[6];

    /*
       Words 230 to 233
       48-bit LBA if Word 69 bit 3 is set
    */
    uint64_t ExtendedUserSectors;

    /*
       Word 234
       Minimum number of 512 byte units per DOWNLOAD MICROCODE mode 3
    */
    uint16_t MinDownloadMicroMode3;
    /*
       Word 235
       Maximum number of 512 byte units per DOWNLOAD MICROCODE mode 3
    */
    uint16_t MaxDownloadMicroMode3;

    /*
       Words 236 to 254
    */
    uint16_t ReservedWords[19];

    /*
       Word 255 bits 7 to 0
       Should be 0xA5
    */
    uint8_t Signature;
    /*
       Word 255 bits 15 to 8
       Checksum
    */
    uint8_t Checksum;
} IdentifyDevice;

unsigned char *AtaToCString (unsigned char *string, int len);

int SendAtaCommandChs (int fd, AtaRegistersCHS registers, AtaErrorRegistersCHS **errorRegisters, int protocol,
                       int transferRegister, unsigned char *buffer, unsigned int buffer_len, int transferBlocks);

int SendAtaCommandLba28 (int fd, AtaRegistersLBA28 registers, AtaErrorRegistersLBA28 **errorRegisters, int protocol,
                         int transferRegister, unsigned char *buffer, unsigned int buffer_len, int transferBlocks);

int SendAtaCommandLba48 (int fd, AtaRegistersLBA48 registers, AtaErrorRegistersLBA48 **errorRegisters, int protocol,
                         int transferRegister, unsigned char *buffer, unsigned int buffer_len, int transferBlocks);

int Identify (int fd, unsigned char **buffer, AtaErrorRegistersCHS **errorRegisters);

int Read (int fd, unsigned char **buffer, AtaErrorRegistersCHS **statusRegisters, int retry, uint16_t cylinder,
          uint8_t head, uint8_t sector, uint8_t count);

int ReadLong (int fd, unsigned char **buffer, AtaErrorRegistersCHS **statusRegisters, int retry, uint16_t cylinder,
              uint8_t head, uint8_t sector, uint32_t blockSize);

int Seek (int fd, AtaErrorRegistersCHS **statusRegisters, uint16_t cylinder, uint8_t head, uint8_t sector);

int ReadDma (int fd, unsigned char **buffer, AtaErrorRegistersCHS **statusRegisters, int retry, uint16_t cylinder,
             uint8_t head, uint8_t sector, uint8_t count);

int ReadDmaLba (int fd, unsigned char **buffer, AtaErrorRegistersLBA28 **statusRegisters, int retry, uint32_t lba,
                uint8_t count);

int ReadLba (int fd, unsigned char **buffer, AtaErrorRegistersLBA28 **statusRegisters, int retry, uint32_t lba,
             uint8_t count);

int ReadLongLba (int fd, unsigned char **buffer, AtaErrorRegistersLBA28 **statusRegisters, int retry, uint32_t lba,
                 uint32_t blockSize);

int SeekLba (int fd, AtaErrorRegistersLBA28 **statusRegisters, uint32_t lba);

int
ReadDmaLba48 (int fd, unsigned char **buffer, AtaErrorRegistersLBA48 **statusRegisters, uint64_t lba, uint16_t count);

int ReadLba48 (int fd, unsigned char **buffer, AtaErrorRegistersLBA48 **statusRegisters, uint64_t lba, uint16_t count);

#endif //DISCIMAGECHEF_DEVICE_REPORT_ATA_H
