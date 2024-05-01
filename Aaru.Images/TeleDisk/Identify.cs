// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class TeleDisk
{
#region IMediaImage Members

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        _header = new Header();
        var    headerBytes = new byte[12];
        Stream stream      = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        stream.EnsureRead(headerBytes, 0, 12);

        _header.Signature = BitConverter.ToUInt16(headerBytes, 0);

        if(_header.Signature != TD_MAGIC && _header.Signature != TD_ADV_COMP_MAGIC) return false;

        _header.Sequence      = headerBytes[2];
        _header.DiskSet       = headerBytes[3];
        _header.Version       = headerBytes[4];
        _header.DataRate      = headerBytes[5];
        _header.DriveType     = headerBytes[6];
        _header.Stepping      = headerBytes[7];
        _header.DosAllocation = headerBytes[8];
        _header.Sides         = headerBytes[9];
        _header.Crc           = BitConverter.ToUInt16(headerBytes, 10);

        var headerBytesForCrc = new byte[10];
        Array.Copy(headerBytes, headerBytesForCrc, 10);
        ushort calculatedHeaderCrc = TeleDiskCrc(0x0000, headerBytesForCrc);

        AaruConsole.DebugWriteLine(MODULE_NAME, "header.signature = 0x{0:X4}",     _header.Signature);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.sequence = 0x{0:X2}",      _header.Sequence);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.diskSet = 0x{0:X2}",       _header.DiskSet);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.version = 0x{0:X2}",       _header.Version);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.dataRate = 0x{0:X2}",      _header.DataRate);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.driveType = 0x{0:X2}",     _header.DriveType);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.stepping = 0x{0:X2}",      _header.Stepping);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.dosAllocation = 0x{0:X2}", _header.DosAllocation);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.sides = 0x{0:X2}",         _header.Sides);
        AaruConsole.DebugWriteLine(MODULE_NAME, "header.crc = 0x{0:X4}",           _header.Crc);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.calculated_header_crc_equals_0_X4, calculatedHeaderCrc);

        // We need more checks as the magic is too simply.
        // This may deny legal images

        // That would be much of a coincidence
        if(_header.Crc == calculatedHeaderCrc) return true;

        if(_header.Sequence != 0x00) return false;

        if(_header.DataRate != DATA_RATE_250_KBPS &&
           _header.DataRate != DATA_RATE_300_KBPS &&
           _header.DataRate != DATA_RATE_500_KBPS)
            return false;

        return _header.DriveType is DRIVE_TYPE_35_DD
                                 or DRIVE_TYPE_35_ED
                                 or DRIVE_TYPE_35_HD
                                 or DRIVE_TYPE_525_DD
                                 or DRIVE_TYPE_525_HD
                                 or DRIVE_TYPE_525_HD_DD_DISK
                                 or DRIVE_TYPE_8_INCH;
    }

#endregion
}