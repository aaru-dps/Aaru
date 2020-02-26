// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies Sydex TeleDisk disk images.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;

namespace DiscImageChef.DiscImages
{
    public partial class TeleDisk
    {
        public bool Identify(IFilter imageFilter)
        {
            header = new TeleDiskHeader();
            byte[] headerBytes = new byte[12];
            Stream stream      = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            stream.Read(headerBytes, 0, 12);

            header.Signature = BitConverter.ToUInt16(headerBytes, 0);

            if(header.Signature != TD_MAGIC && header.Signature != TD_ADV_COMP_MAGIC) return false;

            header.Sequence      = headerBytes[2];
            header.DiskSet       = headerBytes[3];
            header.Version       = headerBytes[4];
            header.DataRate      = headerBytes[5];
            header.DriveType     = headerBytes[6];
            header.Stepping      = headerBytes[7];
            header.DosAllocation = headerBytes[8];
            header.Sides         = headerBytes[9];
            header.Crc           = BitConverter.ToUInt16(headerBytes, 10);

            byte[] headerBytesForCrc = new byte[10];
            Array.Copy(headerBytes, headerBytesForCrc, 10);
            ushort calculatedHeaderCrc = TeleDiskCrc(0x0000, headerBytesForCrc);

            DicConsole.DebugWriteLine("TeleDisk plugin", "header.signature = 0x{0:X4}",      header.Signature);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.sequence = 0x{0:X2}",       header.Sequence);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.diskSet = 0x{0:X2}",        header.DiskSet);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.version = 0x{0:X2}",        header.Version);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.dataRate = 0x{0:X2}",       header.DataRate);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.driveType = 0x{0:X2}",      header.DriveType);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.stepping = 0x{0:X2}",       header.Stepping);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.dosAllocation = 0x{0:X2}",  header.DosAllocation);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.sides = 0x{0:X2}",          header.Sides);
            DicConsole.DebugWriteLine("TeleDisk plugin", "header.crc = 0x{0:X4}",            header.Crc);
            DicConsole.DebugWriteLine("TeleDisk plugin", "calculated header crc = 0x{0:X4}", calculatedHeaderCrc);

            // We need more checks as the magic is too simply.
            // This may deny legal images

            // That would be much of a coincidence
            if(header.Crc == calculatedHeaderCrc) return true;

            if(header.Sequence != 0x00) return false;

            if(header.DataRate != DATA_RATE_250KBPS && header.DataRate != DATA_RATE_300KBPS &&
               header.DataRate != DATA_RATE_500KBPS) return false;

            return header.DriveType == DRIVE_TYPE_35_DD  || header.DriveType == DRIVE_TYPE_35_ED          ||
                   header.DriveType == DRIVE_TYPE_35_HD  || header.DriveType == DRIVE_TYPE_525_DD         ||
                   header.DriveType == DRIVE_TYPE_525_HD || header.DriveType == DRIVE_TYPE_525_HD_DD_DISK ||
                   header.DriveType == DRIVE_TYPE_8_INCH;
        }
    }
}