/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------

Filename       : main.h
Author(s)      : Natalia Portillo

Component      : DiscImageChef.Device.Report

--[ Description ] ----------------------------------------------------------

Contains global definitions.

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

#ifndef DISCIMAGECHEF_DEVICE_REPORT_MAIN_H
#define DISCIMAGECHEF_DEVICE_REPORT_MAIN_H

#define DIC_VERSION "3.99.6.0"
#define DIC_COPYRIGHT "Copyright © 2011-2017 Natalia Portillo"
#define XML_ENCODING "UTF-8"
#define DIC_REPORT_ROOT "DicDeviceReport"

typedef enum
{
    DEVICE_TYPE_UNKNOWN,
    DEVICE_TYPE_SCSI,
    DEVICE_TYPE_ATA,
    DEVICE_TYPE_ATAPI,
    DEVICE_TYPE_USB,
    DEVICE_TYPE_FIREWIRE,
    DEVICE_TYPE_PCMCIA,
    DEVICE_TYPE_MMC,
    DEVICE_TYPE_SD
} DeviceTypes;

const char *DeviceType[] = {"Unknown", "SCSI", "ATA", "ATAPI", "USB", "FireWire", "PCMCIA", "MultiMediaCard",
                            "SecureDigital"};
#endif //DISCIMAGECHEF_DEVICE_REPORT_MAIN_H
