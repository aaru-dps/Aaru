/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------

Filename       : identify_decode.h
Author(s)      : Natalia Portillo

Component      : DiscImageChef.Device.Report

--[ Description ] ----------------------------------------------------------

Contains definitions for ATA IDENTIFY (PACKET) DEVICE structure.

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
Copyright © 2011-2019 Natalia Portillo
****************************************************************************/

#ifndef DISCIMAGECHEF_DEVICE_REPORT_IDENTIFY_DECODE_H
#define DISCIMAGECHEF_DEVICE_REPORT_IDENTIFY_DECODE_H

char *DecodeGeneralConfiguration(uint16_t configuration);

char *DecodeTransferMode(uint16_t transferMode);

char *DecodeCapabilities(uint16_t capabilities);

char *DecodeCapabilities2(uint16_t capabilities);

char *DecodeCapabilities3(uint8_t capabilities);

char *DecodeCommandSet(uint16_t commandset);

char *DecodeCommandSet2(uint16_t commandset);

char *DecodeCommandSet3(uint16_t commandset);

char *DecodeCommandSet4(uint16_t commandset);

char *DecodeCommandSet5(uint16_t commandset);

char *DecodeDataSetMgmt(uint16_t datasetmgmt);

char *DecodeDeviceFormFactor(uint16_t formfactor);

char *DecodeSATAFeatures(uint16_t features);

char *DecodeMajorVersion(uint16_t capabilities);

char *DecodeSATACapabilities(uint16_t capabilities);

char *DecodeSATACapabilities2(uint16_t transport);

char *DecodeSCTCommandTransport(uint16_t transport);

char *DecodeSecurityStatus(uint16_t status);

char *DecodeSpecificConfiguration(uint16_t configuration);

char *DecodeTrustedComputing(uint16_t trutedcomputing);

#endif //DISCIMAGECHEF_DEVICE_REPORT_IDENTIFY_DECODE_H
