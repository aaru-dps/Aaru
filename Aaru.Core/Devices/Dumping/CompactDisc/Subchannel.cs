// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Subchannel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles CompactDisc subchannel data.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/



// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping;

using Aaru.CommonTypes;
using Aaru.Core.Logging;
using Aaru.Devices;

partial class Dump
{
    /// <summary>Check if the drive can read RW raw subchannel</summary>
    /// <param name="dev">Device</param>
    /// <param name="dumpLog">Dumping log</param>
    /// <param name="updateStatus">Progress update callback</param>
    /// <param name="lba">LBA to try</param>
    /// <returns><c>true</c> if read correctly, <c>false</c> otherwise</returns>
    public static bool SupportsRwSubchannel(Device dev, DumpLog dumpLog, UpdateStatusHandler updateStatus, uint lba)
    {
        dumpLog?.WriteLine("Checking if drive supports full raw subchannel reading...");
        updateStatus?.Invoke("Checking if drive supports full raw subchannel reading...");

        return !dev.ReadCd(out _, out _, lba, 2352 + 96, 1, MmcSectorTypes.AllTypes, false, false, true,
                           MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout,
                           out _);
    }

    /// <summary>Check if the drive can read RW raw subchannel</summary>
    /// <param name="dev">Device</param>
    /// <param name="dumpLog">Dumping log</param>
    /// <param name="updateStatus">Progress update callback</param>
    /// <param name="lba">LBA to try</param>
    /// <returns><c>true</c> if read correctly, <c>false</c> otherwise</returns>
    public static bool SupportsPqSubchannel(Device dev, DumpLog dumpLog, UpdateStatusHandler updateStatus, uint lba)
    {
        dumpLog?.WriteLine("Checking if drive supports PQ subchannel reading...");
        updateStatus?.Invoke("Checking if drive supports PQ subchannel reading...");

        return !dev.ReadCd(out _, out _, lba, 2352 + 16, 1, MmcSectorTypes.AllTypes, false, false, true,
                           MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Q16, dev.Timeout,
                           out _);
    }
}