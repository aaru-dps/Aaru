/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------

Filename       : atapi.c
Author(s)      : Natalia Portillo

Component      : DiscImageChef.Device.Report

--[ Description ] ----------------------------------------------------------

Contains ATAPI commands.

--[ License ] --------------------------------------------------------------

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright © 2011-2018 Natalia Portillo
****************************************************************************/

#include <malloc.h>
#include <string.h>
#include <stdint.h>
#include "ata.h"
#include "atapi.h"

int IdentifyPacket(int fd, unsigned char **buffer, AtaErrorRegistersCHS **errorRegisters)
{
    *buffer = malloc(512);
    memset(*buffer, 0, 512);
    AtaRegistersCHS registers;
    memset(&registers, 0, sizeof(AtaRegistersCHS));

    registers.command = ATA_IDENTIFY_PACKET_DEVICE;

    int error = SendAtaCommandChs(fd, registers, errorRegisters, ATA_PROTOCOL_PIO_IN, ATA_TRANSFER_NONE, *buffer, 512,
                                  0);

    return error;
}