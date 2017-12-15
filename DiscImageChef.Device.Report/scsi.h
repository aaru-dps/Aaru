//
// Created by claunia on 11/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_SCSI_H
#define DISCIMAGECHEF_DEVICE_REPORT_SCSI_H

#include <stdint.h>

int SendScsiCommand(int fd, void *cdb, unsigned char cdb_len, unsigned char *buffer, unsigned int buffer_len, unsigned char **senseBuffer, int direction);
int Inquiry(int fd, unsigned char **buffer, unsigned char **senseBuffer);


typedef enum
{
    SCSI_INQUIRY = 0x12,
    SCSI_ATA_PASSTHROUGH_16 = 0x85
} ScsiCommands;

// SCSI INQUIRY command response
#pragma pack(1)
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

#endif //DISCIMAGECHEF_DEVICE_REPORT_SCSI_H
