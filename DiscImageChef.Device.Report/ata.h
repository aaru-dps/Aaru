//
// Created by claunia on 11/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_ATA_H
#define DISCIMAGECHEF_DEVICE_REPORT_ATA_H

#include <stdint.h>

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
}AtaRegistersLBA28;

typedef struct
{
    uint16_t feature;
    uint16_t sectorCount;
    uint16_t lbaLow;
    uint16_t lbaMid;
    uint16_t lbaHigh;
    uint8_t deviceHead;
    uint8_t command;
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
}AtaErrorRegistersCHS;

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
    uint8_t status;
    uint8_t error;
    uint16_t sectorCount;
    uint16_t lbaLow;
    uint16_t lbaMid;
    uint16_t lbaHigh;
    uint8_t deviceHead;
    uint8_t command;
} AtaErrorRegistersLBA48;

typedef enum
{
    ATA_TRANSFER_NONE = 0,
    ATA_TRANSFER_FEATURE,
    ATA_TRANSFER_SECTORCOUNT,
    ATA_TRANSFTER_SPTSIU
} AtaTransferRegister;

typedef enum {
    ATA_PROTOCOL_HARD_RESET = 0,
    ATA_PROTOCOL_SOFT_RESET = 1,
    ATA_PROTOCOL_NO_DATA = 3,
    ATA_PROTOCOL_PIO_IN = 4,
    ATA_PROTOCOL_PIO_OUT = 5,
    ATA_PROTOCOL_DMA = 6,
    ATA_PROTOCOL_DMA_QUEUED = 7,
    ATA_PROTOCOL_DEVICE_DIAGNOSTICS = 8,
    ATA_PROTOCOL_DEVICE_RESET = 9,
    ATA_PROTOCOL_UDMA_IN = 10,
    ATA_PROTOCOL_UDMA_OUT = 11,
    ATA_PROTOCOL_FPDMA = 12,
    ATA_PROTOCOL_RETURN_RESPONSE = 15
} AtaProtocol;

typedef enum
{
    ATA_IDENTIFY_PACKET_DEVICE = 0xA1,
    ATA_IDENTIFY_DEVICE = 0xEC
} AtaCommands;

unsigned char *AtaToCString(unsigned char* string, int len);
int SendAtaCommandChs(int fd, AtaRegistersCHS registers, AtaErrorRegistersCHS **errorRegisters, int protocol, int transferRegister, unsigned char *buffer, unsigned int buffer_len, int transferBlocks);
int SendAtaCommandLba28(int fd, AtaRegistersLBA28 registers, AtaErrorRegistersLBA28 **errorRegisters, int protocol, int transferRegister, unsigned char *buffer, unsigned int buffer_len, int transferBlocks);
int SendAtaCommandLba48(int fd, AtaRegistersLBA48 registers, AtaErrorRegistersLBA48 **errorRegisters, int protocol, int transferRegister, unsigned char *buffer, unsigned int buffer_len, int transferBlocks);
int Identify(int fd, unsigned char **buffer, AtaErrorRegistersCHS **errorRegisters);
#endif //DISCIMAGECHEF_DEVICE_REPORT_ATA_H
