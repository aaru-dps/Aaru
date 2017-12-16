//
// Created by claunia on 11/12/17.
//

#include <scsi/sg.h>
#include <malloc.h>
#include <string.h>
#include "ata.h"
#include "scsi.h"

int AtaProtocolToScsiDirection(int protocol)
{
    switch(protocol)
    {
        case ATA_PROTOCOL_DEVICE_DIAGNOSTICS:
        case ATA_PROTOCOL_DEVICE_RESET:
        case ATA_PROTOCOL_HARD_RESET:
        case ATA_PROTOCOL_NO_DATA:
        case ATA_PROTOCOL_SOFT_RESET:
        case ATA_PROTOCOL_RETURN_RESPONSE:
            return SG_DXFER_NONE;
        case ATA_PROTOCOL_PIO_IN:
        case ATA_PROTOCOL_UDMA_IN:
            return SG_DXFER_FROM_DEV;
        case ATA_PROTOCOL_PIO_OUT:
        case ATA_PROTOCOL_UDMA_OUT:
            return SG_DXFER_TO_DEV;
        default:
            return SG_DXFER_TO_FROM_DEV;
    }
}

unsigned char *AtaToCString(unsigned char* string, int len)
{
    unsigned char* buffer = malloc(len + 1);
    unsigned char* ptr = buffer;
    int i;

    for(i = 0; i < len; i+=2)
    {
        *ptr++ = *(string + i + 1);
        *ptr++ = *(string + i);
    }

    buffer[len] = 0x00;
    *ptr = *(buffer + len);

    for(i = len; i >= 0; i--, *ptr--)
    {
        if(*ptr == 0x20 || *ptr == 0x00)
            *ptr = 0;
        else
            break;
    }

    return buffer;
}

int SendAtaCommandChs(int fd, AtaRegistersCHS registers, AtaErrorRegistersCHS **errorRegisters, int protocol, int transferRegister, unsigned char *buffer, unsigned int buffer_len, int transferBlocks)
{
    unsigned char cdb[16];
    memset(&cdb, 0, 16);
    cdb[0] = SCSI_ATA_PASSTHROUGH_16;
    cdb[1] = (unsigned char)((protocol << 1) & 0x1E);
    if(transferRegister != ATA_TRANSFER_NONE && protocol != ATA_PROTOCOL_NO_DATA)
    {
        switch(protocol)
        {
            case ATA_PROTOCOL_PIO_IN:
            case ATA_PROTOCOL_UDMA_IN:
                cdb[2] = 0x08;
                break;
            default:
                cdb[2] = 0x00;
                break;
        }

        if(transferBlocks)
            cdb[2] |= 0x04;

        cdb[2] |= (transferRegister & 0x03);
    }

    cdb[4] = registers.feature;
    cdb[6] = registers.sectorCount;
    cdb[8] = registers.sector;
    cdb[10] = registers.cylinderLow;
    cdb[12] = registers.cylinderHigh;
    cdb[13] = registers.deviceHead;
    cdb[14] = registers.command;

    unsigned char *sense_buf;
    int error = SendScsiCommand(fd, &cdb, 16, buffer, buffer_len, &sense_buf, AtaProtocolToScsiDirection(protocol));

    *errorRegisters = malloc(sizeof(AtaErrorRegistersCHS));
    memset(*errorRegisters, 0, sizeof(AtaErrorRegistersCHS));
    (*errorRegisters)->error = sense_buf[11];
    (*errorRegisters)->sectorCount = sense_buf[13];
    (*errorRegisters)->sector = sense_buf[15];
    (*errorRegisters)->cylinderLow = sense_buf[17];
    (*errorRegisters)->cylinderHigh = sense_buf[19];
    (*errorRegisters)->deviceHead = sense_buf[20];
    (*errorRegisters)->status = sense_buf[21];

    if(error != 0)
        return error;

    return (*errorRegisters)->error;
}

int SendAtaCommandLba28(int fd, AtaRegistersLBA28 registers, AtaErrorRegistersLBA28 **errorRegisters, int protocol, int transferRegister, unsigned char *buffer, unsigned int buffer_len, int transferBlocks)
{
    unsigned char cdb[16];
    memset(&cdb, 0, 16);
    cdb[0] = SCSI_ATA_PASSTHROUGH_16;
    cdb[1] = (unsigned char)((protocol << 1) & 0x1E);
    if(transferRegister != ATA_TRANSFER_NONE && protocol != ATA_PROTOCOL_NO_DATA)
    {
        switch(protocol)
        {
            case ATA_PROTOCOL_PIO_IN:
            case ATA_PROTOCOL_UDMA_IN:
                cdb[2] = 0x08;
                break;
            default:
                cdb[2] = 0x00;
                break;
        }

        if(transferBlocks)
            cdb[2] |= 0x04;

        cdb[2] |= (transferRegister & 0x03);
    }

    cdb[2] |= 0x20;

    cdb[4] = registers.feature;
    cdb[6] = registers.sectorCount;
    cdb[8] = registers.lbaLow;
    cdb[10] = registers.lbaMid;
    cdb[12] = registers.lbaHigh;
    cdb[13] = registers.deviceHead;
    cdb[14] = registers.command;

    unsigned char *sense_buf;
    int error = SendScsiCommand(fd, &cdb, 16, buffer, buffer_len, &sense_buf, AtaProtocolToScsiDirection(protocol));

    *errorRegisters = malloc(sizeof(AtaErrorRegistersLBA28));
    memset(*errorRegisters, 0, sizeof(AtaErrorRegistersLBA28));
    (*errorRegisters)->error = sense_buf[11];
    (*errorRegisters)->sectorCount = sense_buf[13];
    (*errorRegisters)->lbaLow = sense_buf[15];
    (*errorRegisters)->lbaMid= sense_buf[17];
    (*errorRegisters)->lbaHigh = sense_buf[19];
    (*errorRegisters)->deviceHead = sense_buf[20];
    (*errorRegisters)->status = sense_buf[21];

    if(error != 0)
        return error;

    return (*errorRegisters)->error;
}

int SendAtaCommandLba48(int fd, AtaRegistersLBA48 registers, AtaErrorRegistersLBA48 **errorRegisters, int protocol, int transferRegister, unsigned char *buffer, unsigned int buffer_len, int transferBlocks)
{
    unsigned char cdb[16];
    memset(&cdb, 0, 16);
    cdb[0] = SCSI_ATA_PASSTHROUGH_16;
    cdb[1] = (unsigned char)((protocol << 1) & 0x1E);
    cdb[1] |= 0x01;
    if(transferRegister != ATA_TRANSFER_NONE && protocol != ATA_PROTOCOL_NO_DATA)
    {
        switch(protocol)
        {
            case ATA_PROTOCOL_PIO_IN:
            case ATA_PROTOCOL_UDMA_IN:
                cdb[2] = 0x08;
                break;
            default:
                cdb[2] = 0x00;
                break;
        }

        if(transferBlocks)
            cdb[2] |= 0x04;

        cdb[2] |= (transferRegister & 0x03);
    }

    cdb[2] |= 0x20;

    cdb[3] = (uint8_t)((registers.feature & 0xFF00) >> 8);
    cdb[4] = (uint8_t)(registers.feature & 0xFF);
    cdb[5] = (uint8_t)((registers.sectorCount & 0xFF00) >> 8);
    cdb[6] = (uint8_t)(registers.sectorCount & 0xFF);
    cdb[7] = (uint8_t)((registers.lbaLow & 0xFF00) >> 8);
    cdb[8] = (uint8_t)(registers.lbaLow & 0xFF);
    cdb[9] = (uint8_t)((registers.lbaMid & 0xFF00) >> 8);
    cdb[10] = (uint8_t)(registers.lbaMid & 0xFF);
    cdb[11] = (uint8_t)((registers.lbaHigh & 0xFF00) >> 8);
    cdb[12] = (uint8_t)(registers.lbaHigh & 0xFF);
    cdb[13] = registers.deviceHead;
    cdb[14] = registers.command;

    unsigned char *sense_buf;
    int error = SendScsiCommand(fd, &cdb, 16, buffer, buffer_len, &sense_buf, AtaProtocolToScsiDirection(protocol));

    *errorRegisters = malloc(sizeof(AtaErrorRegistersLBA48));
    memset(*errorRegisters, 0, sizeof(AtaErrorRegistersLBA48));
    (*errorRegisters)->error = sense_buf[11];
    (*errorRegisters)->sectorCount = (uint16_t)((sense_buf[12] << 8) + sense_buf[13]);
    (*errorRegisters)->lbaLow = (uint16_t)((sense_buf[14] << 8) + sense_buf[15]);
    (*errorRegisters)->lbaMid = (uint16_t)((sense_buf[16] << 8) + sense_buf[17]);
    (*errorRegisters)->lbaHigh = (uint16_t)((sense_buf[18] << 8) + sense_buf[19]);
    (*errorRegisters)->deviceHead = sense_buf[20];
    (*errorRegisters)->status = sense_buf[21];

    if(error != 0)
        return error;

    return (*errorRegisters)->error;
}

int Identify(int fd, unsigned char **buffer, AtaErrorRegistersCHS **errorRegisters)
{
    *buffer = malloc(512);
    memset(*buffer, 0, 512);
    AtaRegistersCHS registers;
    memset(&registers, 0, sizeof(AtaRegistersCHS));

    registers.command = ATA_IDENTIFY_DEVICE;

    int error = SendAtaCommandChs(fd, registers, errorRegisters, ATA_PROTOCOL_PIO_IN, ATA_TRANSFER_NONE, *buffer, 512, 0);

    return error;
}