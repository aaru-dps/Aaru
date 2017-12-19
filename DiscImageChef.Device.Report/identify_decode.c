/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------

Filename       : identify_decode.h
Author(s)      : Natalia Portillo

Component      : DiscImageChef.Device.Report

--[ Description ] ----------------------------------------------------------

Contains decoders for ATA IDENTIFY (PACKET) DEVICE structure.

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

#include <stdint.h>
#include <string.h>
#include <malloc.h>
#include "identify_decode.h"

#define MAX_STRING_SIZE 512

char *DecodeGeneralConfiguration(uint16_t configuration)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(configuration & 0x8000)
    {
        strcat(decoded, "NonMagnetic");
        set = 1;
    }

    if(configuration & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "FormatGapReq");
        set = 1;
    }

    if(configuration & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "TrackOffset");
        set = 1;
    }

    if(configuration & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "DataStrobeOffset");
        set = 1;
    }

    if(configuration & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "RotationalSpeedTolerance");
        set = 1;
    }

    if(configuration & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "UltraFastIDE");
        set = 1;
    }

    if(configuration & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "FastIDE");
        set = 1;
    }

    if(configuration & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SlowIDE");
        set = 1;
    }

    if(configuration & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Removable");
        set = 1;
    }

    if(configuration & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Fixed");
        set = 1;
    }

    if(configuration & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SpindleControl");
        set = 1;
    }

    if(configuration & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "HighHeadSwitch");
        set = 1;
    }

    if(configuration & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "NotMFM");
        set = 1;
    }

    if(configuration & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "IncompleteResponse");
        set = 1;
    }

    if(configuration & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "HardSector");
        set = 1;
    }

    if(configuration & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeTransferMode(uint16_t transferMode)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(transferMode & 0x80)
    {
        strcat(decoded, "Mode7");
        set = 1;
    }

    if(transferMode & 0x40)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Mode6");
        set = 1;
    }

    if(transferMode & 0x20)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Mode5");
        set = 1;
    }

    if(transferMode & 0x10)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Mode4");
        set = 1;
    }

    if(transferMode & 0x08)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Mode3");
        set = 1;
    }

    if(transferMode & 0x04)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Mode2");
        set = 1;
    }

    if(transferMode & 0x02)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Mode1");
        set = 1;
    }

    if(transferMode & 0x01)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Mode0");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeCapabilities(uint16_t capabilities)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(capabilities & 0x8000)
    {
        strcat(decoded, "InterleavedDMA");
        set = 1;
    }

    if(capabilities & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "CommandQueue");
        set = 1;
    }

    if(capabilities & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "StandardStanbyTimer");
        set = 1;
    }

    if(capabilities & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "RequiresATASoftReset");
        set = 1;
    }

    if(capabilities & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "IORDY");
        set = 1;
    }

    if(capabilities & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "CanDisableIORDY");
        set = 1;
    }

    if(capabilities & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "LBASupport");
        set = 1;
    }

    if(capabilities & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "DMASupport");
        set = 1;
    }

    if(capabilities & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "VendorBit7");
        set = 1;
    }

    if(capabilities & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "VendorBit6");
        set = 1;
    }

    if(capabilities & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "VendorBit5");
        set = 1;
    }

    if(capabilities & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "VendorBit4");
        set = 1;
    }

    if(capabilities & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "VendorBit3");
        set = 1;
    }

    if(capabilities & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "VendorBit2");
        set = 1;
    }

    if(capabilities & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "PhysicalAlignment1");
        set = 1;
    }

    if(capabilities & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "PhysicalAlignment0");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeCapabilities2(uint16_t capabilities)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(capabilities & 0x8000)
    {
        strcat(decoded, "MustBeClear");
        set = 1;
    }

    if(capabilities & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "MustBeSet");
        set = 1;
    }

    if(capabilities & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved13");
        set = 1;
    }

    if(capabilities & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved12");
        set = 1;
    }

    if(capabilities & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved11");
        set = 1;
    }

    if(capabilities & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved10");
        set = 1;
    }

    if(capabilities & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved09");
        set = 1;
    }

    if(capabilities & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved08");
        set = 1;
    }

    if(capabilities & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved07");
        set = 1;
    }

    if(capabilities & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved06");
        set = 1;
    }

    if(capabilities & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved05");
        set = 1;
    }

    if(capabilities & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved04");
        set = 1;
    }

    if(capabilities & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved03");
        set = 1;
    }

    if(capabilities & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved02");
        set = 1;
    }

    if(capabilities & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved01");
        set = 1;
    }

    if(capabilities & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SpecificStandbyTimer");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeCapabilities3(uint8_t capabilities)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(capabilities & 0x80)
    {
        strcat(decoded, "BlockErase");
        set = 1;
    }

    if(capabilities & 0x40)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Overwrite");
        set = 1;
    }

    if(capabilities & 0x20)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "CryptoScramble");
        set = 1;
    }

    if(capabilities & 0x10)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Sanitize");
        set = 1;
    }

    if(capabilities & 0x08)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SanitizeCommands");
        set = 1;
    }

    if(capabilities & 0x04)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SanitizeAntifreeze");
        set = 1;
    }

    if(capabilities & 0x02)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved01");
        set = 1;
    }

    if(capabilities & 0x01)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "MultipleValid");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeCommandSet(uint16_t commandset)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(commandset & 0x8000)
    {
        strcat(decoded, "Obsolete15");
        set = 1;
    }

    if(commandset & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Nop");
        set = 1;
    }

    if(commandset & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "ReadBuffer");
        set = 1;
    }

    if(commandset & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "WriteBuffer");
        set = 1;
    }

    if(commandset & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Obsolete11");
        set = 1;
    }

    if(commandset & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "HPA");
        set = 1;
    }

    if(commandset & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "DeviceReset");
        set = 1;
    }

    if(commandset & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Service");
        set = 1;
    }

    if(commandset & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Release");
        set = 1;
    }

    if(commandset & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "LookAhead");
        set = 1;
    }

    if(commandset & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "WriteCache");
        set = 1;
    }

    if(commandset & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Packet");
        set = 1;
    }

    if(commandset & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "PowerManagement");
        set = 1;
    }

    if(commandset & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "RemovableMedia");
        set = 1;
    }

    if(commandset & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SecurityMode");
        set = 1;
    }

    if(commandset & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SMART");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeCommandSet2(uint16_t commandset)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(commandset & 0x8000)
    {
        strcat(decoded, "MustBeClear");
        set = 1;
    }

    if(commandset & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "MustBeSet");
        set = 1;
    }

    if(commandset & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "FlushCacheExt");
        set = 1;
    }

    if(commandset & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "FlushCache");
        set = 1;
    }

    if(commandset & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "DCO");
        set = 1;
    }

    if(commandset & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "LBA48");
        set = 1;
    }

    if(commandset & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "AAM");
        set = 1;
    }

    if(commandset & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SetMax");
        set = 1;
    }

    if(commandset & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "AddressOffsetReservedAreaBoot");
        set = 1;
    }

    if(commandset & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SetFeaturesRequired");
        set = 1;
    }

    if(commandset & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "PowerUpInStandby");
        set = 1;
    }

    if(commandset & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "RemovableNotification");
        set = 1;
    }

    if(commandset & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "APM");
        set = 1;
    }

    if(commandset & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "CompactFlash");
        set = 1;
    }

    if(commandset & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "DownloadMicrocode");
        set = 1;
    }

    if(commandset & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SMART");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeCommandSet3(uint16_t commandset)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(commandset & 0x8000)
    {
        strcat(decoded, "MustBeClear");
        set = 1;
    }

    if(commandset & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "MustBeSet");
        set = 1;
    }

    if(commandset & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "IdleImmediate");
        set = 1;
    }

    if(commandset & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved12");
        set = 1;
    }

    if(commandset & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved11");
        set = 1;
    }

    if(commandset & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "WriteURG");
        set = 1;
    }

    if(commandset & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "ReadURG");
        set = 1;
    }

    if(commandset & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "WWN");
        set = 1;
    }

    if(commandset & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "FUAWriteQ");
        set = 1;
    }

    if(commandset & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "FUAWrite");
        set = 1;
    }

    if(commandset & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "GPL");
        set = 1;
    }

    if(commandset & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Streaming");
        set = 1;
    }

    if(commandset & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "MCPT");
        set = 1;
    }

    if(commandset & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "MediaSerial");
        set = 1;
    }

    if(commandset & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SMARTSelfTest");
        set = 1;
    }

    if(commandset & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SMARTLog");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeCommandSet4(uint16_t commandset)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(commandset & 0x8000)
    {
        strcat(decoded, "MustBeClear");
        set = 1;
    }

    if(commandset & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "MustBeSet");
        set = 1;
    }

    if(commandset & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved13");
        set = 1;
    }

    if(commandset & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved12");
        set = 1;
    }

    if(commandset & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved11");
        set = 1;
    }

    if(commandset & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved10");
        set = 1;
    }

    if(commandset & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "DSN");
        set = 1;
    }

    if(commandset & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "AMAC");
        set = 1;
    }

    if(commandset & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "ExtPowerCond");
        set = 1;
    }

    if(commandset & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "ExtStatusReport");
        set = 1;
    }

    if(commandset & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "FreeFallControl");
        set = 1;
    }

    if(commandset & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SegmentedDownloadMicrocode");
        set = 1;
    }

    if(commandset & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "RWDMAExtGpl");
        set = 1;
    }

    if(commandset & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "WriteUnc");
        set = 1;
    }

    if(commandset & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "WRV");
        set = 1;
    }

    if(commandset & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "DT1825");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeCommandSet5(uint16_t commandset)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(commandset & 0x8000)
    {
        strcat(decoded, "CFast");
        set = 1;
    }

    if(commandset & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "DeterministicTrim");
        set = 1;
    }

    if(commandset & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "LongPhysSectorAligError");
        set = 1;
    }

    if(commandset & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "DeviceConfDMA");
        set = 1;
    }

    if(commandset & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "ReadBufferDMA");
        set = 1;
    }

    if(commandset & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "WriteBufferDMA");
        set = 1;
    }

    if(commandset & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SetMaxDMA");
        set = 1;
    }

    if(commandset & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "DownloadMicroCodeDMA");
        set = 1;
    }

    if(commandset & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "IEEE1667");
        set = 1;
    }

    if(commandset & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Ata28");
        set = 1;
    }

    if(commandset & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "ReadZeroTrim");
        set = 1;
    }

    if(commandset & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Encrypted");
        set = 1;
    }

    if(commandset & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "ExtSectors");
        set = 1;
    }

    if(commandset & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "AllCacheNV");
        set = 1;
    }

    if(commandset & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "ZonedBit1");
        set = 1;
    }

    if(commandset & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "ZonedBit0");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeDataSetMgmt(uint16_t datasetmgmt)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(datasetmgmt & 0x8000)
    {
        strcat(decoded, "Reserved15");
        set = 1;
    }

    if(datasetmgmt & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved14");
        set = 1;
    }

    if(datasetmgmt & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved13");
        set = 1;
    }

    if(datasetmgmt & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved12");
        set = 1;
    }

    if(datasetmgmt & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved11");
        set = 1;
    }

    if(datasetmgmt & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved10");
        set = 1;
    }

    if(datasetmgmt & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved09");
        set = 1;
    }

    if(datasetmgmt & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved08");
        set = 1;
    }

    if(datasetmgmt & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved07");
        set = 1;
    }

    if(datasetmgmt & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved06");
        set = 1;
    }

    if(datasetmgmt & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved05");
        set = 1;
    }

    if(datasetmgmt & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved04");
        set = 1;
    }

    if(datasetmgmt & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved03");
        set = 1;
    }

    if(datasetmgmt & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved02");
        set = 1;
    }

    if(datasetmgmt & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved01");
        set = 1;
    }

    if(datasetmgmt & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Trim");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeDeviceFormFactor(uint16_t formfactor)
{
    switch(formfactor)
    {
        case 0:
            return "NotReported";
        case 1:
            return "FiveAndQuarter";
        case 2:
            return "ThreeAndHalf";
        case 3:
            return "TwoAndHalf";
        case 4:
            return "OnePointEight";
        case 5:
            return "LessThanOnePointEight";
        default:
            return NULL;
    }
}

char *DecodeSATAFeatures(uint16_t features)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(features & 0x8000)
    {
        strcat(decoded, "Reserved15");
        set = 1;
    }

    if(features & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved14");
        set = 1;
    }

    if(features & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved13");
        set = 1;
    }

    if(features & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved12");
        set = 1;
    }

    if(features & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved11");
        set = 1;
    }

    if(features & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved10");
        set = 1;
    }

    if(features & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved09");
        set = 1;
    }

    if(features & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved08");
        set = 1;
    }

    if(features & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "NCQAutoSense");
        set = 1;
    }

    if(features & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "SettingsPreserve");
        set = 1;
    }

    if(features & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "HardwareFeatureControl");
        set = 1;
    }

    if(features & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "InOrderData");
        set = 1;
    }

    if(features & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "InitPowerMgmt");
        set = 1;
    }

    if(features & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "DMASetup");
        set = 1;
    }

    if(features & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "NonZeroBufferOffset");
        set = 1;
    }

    if(features & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Clear");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeMajorVersion(uint16_t version)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(version & 0x8000)
    {
        strcat(decoded, "Reserved15");
        set = 1;
    }

    if(version & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved14");
        set = 1;
    }

    if(version & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved13");
        set = 1;
    }

    if(version & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved12");
        set = 1;
    }

    if(version & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "ACS4");
        set = 1;
    }

    if(version & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "ACS3");
        set = 1;
    }

    if(version & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "ACS2");
        set = 1;
    }

    if(version & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Ata8ACS");
        set = 1;
    }

    if(version & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "AtaAtapi7");
        set = 1;
    }

    if(version & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "AtaAtapi6");
        set = 1;
    }

    if(version & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "AtaAtapi5");
        set = 1;
    }

    if(version & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "AtaAtapi4");
        set = 1;
    }

    if(version & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Ata3");
        set = 1;
    }

    if(version & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Ata2");
        set = 1;
    }

    if(version & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Ata1");
        set = 1;
    }

    if(version & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved00");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeSATACapabilities(uint16_t capabilities)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(capabilities & 0x8000)
    {
        strcat(decoded, "ReadLogDMAExt");
        set = 1;
    }

    if(capabilities & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "DevSlumbTrans");
        set = 1;
    }

    if(capabilities & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "HostSlumbTrans");
        set = 1;
    }

    if(capabilities & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "NCQPriority");
        set = 1;
    }

    if(capabilities & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "UnloadNCQ");
        set = 1;
    }

    if(capabilities & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "PHYEventCounter");
        set = 1;
    }

    if(capabilities & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "PowerReceipt");
        set = 1;
    }

    if(capabilities & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "NCQ");
        set = 1;
    }

    if(capabilities & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved07");
        set = 1;
    }

    if(capabilities & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved06");
        set = 1;
    }

    if(capabilities & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved05");
        set = 1;
    }

    if(capabilities & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved04");
        set = 1;
    }

    if(capabilities & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Gen3Speed");
        set = 1;
    }

    if(capabilities & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Gen2Speed");
        set = 1;
    }

    if(capabilities & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Gen1Speed");
        set = 1;
    }

    if(capabilities & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Clear");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeSATACapabilities2(uint16_t capabilities)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(capabilities & 0x8000)
    {
        strcat(decoded, "Reserved15");
        set = 1;
    }

    if(capabilities & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved14");
        set = 1;
    }

    if(capabilities & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved13");
        set = 1;
    }

    if(capabilities & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved12");
        set = 1;
    }

    if(capabilities & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved11");
        set = 1;
    }

    if(capabilities & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved10");
        set = 1;
    }

    if(capabilities & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved09");
        set = 1;
    }

    if(capabilities & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved08");
        set = 1;
    }

    if(capabilities & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved07");
        set = 1;
    }

    if(capabilities & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "FPDMAQ");
        set = 1;
    }

    if(capabilities & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "NCQMgmt");
        set = 1;
    }

    if(capabilities & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "NCQStream");
        set = 1;
    }

    if(capabilities & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "CurrentSpeedBit2");
        set = 1;
    }

    if(capabilities & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "CurrentSpeedBit1");
        set = 1;
    }

    if(capabilities & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "CurrentSpeedBit0");
        set = 1;
    }

    if(capabilities & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Clear");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeSCTCommandTransport(uint16_t transport)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(transport & 0x8000)
    {
        strcat(decoded, "Vendor15");
        set = 1;
    }

    if(transport & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Vendor14");
        set = 1;
    }

    if(transport & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Vendor13");
        set = 1;
    }

    if(transport & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Vendor12");
        set = 1;
    }

    if(transport & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved11");
        set = 1;
    }

    if(transport & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved10");
        set = 1;
    }

    if(transport & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved09");
        set = 1;
    }

    if(transport & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved08");
        set = 1;
    }

    if(transport & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved07");
        set = 1;
    }

    if(transport & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved06");
        set = 1;
    }

    if(transport & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "DataTables");
        set = 1;
    }

    if(transport & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "FeaturesControl");
        set = 1;
    }

    if(transport & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "ErrorRecoveryControl");
        set = 1;
    }

    if(transport & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "WriteSame");
        set = 1;
    }

    if(transport & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "LongSectorAccess");
        set = 1;
    }

    if(transport & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Supported");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeSecurityStatus(uint16_t status)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(status & 0x8000)
    {
        strcat(decoded, "Reserved15");
        set = 1;
    }

    if(status & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved14");
        set = 1;
    }

    if(status & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved13");
        set = 1;
    }

    if(status & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved12");
        set = 1;
    }

    if(status & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved11");
        set = 1;
    }

    if(status & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved10");
        set = 1;
    }

    if(status & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved09");
        set = 1;
    }

    if(status & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Maximum");
        set = 1;
    }

    if(status & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved07");
        set = 1;
    }

    if(status & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved06");
        set = 1;
    }

    if(status & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Enhanced");
        set = 1;
    }

    if(status & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Expired");
        set = 1;
    }

    if(status & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Frozen");
        set = 1;
    }

    if(status & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Locked");
        set = 1;
    }

    if(status & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Enabled");
        set = 1;
    }

    if(status & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Supported");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}

char *DecodeSpecificConfiguration(uint16_t configuration)
{
    switch(configuration)
    {
        case 0x37C8:
            return "RequiresSetIncompleteResponse";
        case 0x738C:
            return "RequiresSetCompleteResponse";
        case 0x8C73:
            return "NotRequiresSetIncompleteResponse";
        case 0xC837:
            return "NotRequiresSetCompleteResponse";
        default:
            return NULL;
    }
}

char *DecodeTrustedComputing(uint16_t trutedcomputing)
{
    char *decoded = malloc(MAX_STRING_SIZE);
    memset(decoded, 0, MAX_STRING_SIZE);
    int set = 0;

    if(trutedcomputing & 0x8000)
    {
        strcat(decoded, "Clear");
        set = 1;
    }

    if(trutedcomputing & 0x4000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Set");
        set = 1;
    }

    if(trutedcomputing & 0x2000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved13");
        set = 1;
    }

    if(trutedcomputing & 0x1000)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved12");
        set = 1;
    }

    if(trutedcomputing & 0x0800)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved11");
        set = 1;
    }

    if(trutedcomputing & 0x0400)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved10");
        set = 1;
    }

    if(trutedcomputing & 0x0200)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved09");
        set = 1;
    }

    if(trutedcomputing & 0x0100)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved08");
        set = 1;
    }

    if(trutedcomputing & 0x0080)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved07");
        set = 1;
    }

    if(trutedcomputing & 0x0040)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved06");
        set = 1;
    }

    if(trutedcomputing & 0x0020)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved05");
        set = 1;
    }

    if(trutedcomputing & 0x0010)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved04");
        set = 1;
    }

    if(trutedcomputing & 0x0008)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved03");
        set = 1;
    }

    if(trutedcomputing & 0x0004)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved02");
        set = 1;
    }

    if(trutedcomputing & 0x0002)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "Reserved01");
        set = 1;
    }

    if(trutedcomputing & 0x0001)
    {
        if(set)
            strcat(decoded, " ");
        strcat(decoded, "TrustedComputing");
        set = 1;
    }

    if(set)
        return decoded;

    return NULL;
}