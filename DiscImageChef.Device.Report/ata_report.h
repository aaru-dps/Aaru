/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------

Filename       : ata_report.h
Author(s)      : Natalia Portillo

Component      : DiscImageChef.Device.Report

--[ Description ] ----------------------------------------------------------

Contains definitions used in ATA reports.

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

#ifndef DISCIMAGECHEF_DEVICE_REPORT_ATA_REPORT_H
#define DISCIMAGECHEF_DEVICE_REPORT_ATA_REPORT_H
#define DIC_ATA_REPORT_ELEMENT "ATA"

void AtaReport(int fd, xmlTextWriterPtr xmlWriter);

#endif //DISCIMAGECHEF_DEVICE_REPORT_ATA_REPORT_H
