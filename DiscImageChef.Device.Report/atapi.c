//
// Created by claunia on 12/12/17.
//

#include <malloc.h>
#include <string.h>
#include "ata.h"
#include "atapi.h"

int IdentifyPacket(int fd, unsigned char **buffer, AtaErrorRegistersCHS **errorRegisters)
{
    *buffer = malloc(512);
    memset(*buffer, 0, 512);
    AtaRegistersCHS registers;
    memset(&registers, 0, sizeof(AtaRegistersCHS));

    registers.command = ATA_IDENTIFY_PACKET_DEVICE;

    int error = SendAtaCommandChs(fd, registers, errorRegisters, ATA_PROTOCOL_PIO_IN, ATA_TRANSFER_NONE, *buffer, 512, 0);

    return error;
}