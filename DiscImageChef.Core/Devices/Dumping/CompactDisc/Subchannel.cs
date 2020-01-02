// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CompactDisc.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps CDs and DDCDs.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Extents;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Core.Media.Detection;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Devices;
using Schemas;
using CdOffset = DiscImageChef.Database.Models.CdOffset;
using MediaType = DiscImageChef.CommonTypes.MediaType;
using PlatformID = DiscImageChef.CommonTypes.Interop.PlatformID;
using Session = DiscImageChef.Decoders.CD.Session;
using TrackType = DiscImageChef.CommonTypes.Enums.TrackType;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace DiscImageChef.Core.Devices.Dumping
{
    partial class Dump
    {
        bool SupportsRwSubchannel()
        {
            _dumpLog.WriteLine("Checking if drive supports full raw subchannel reading...");
            UpdateStatus?.Invoke("Checking if drive supports full raw subchannel reading...");

            return!_dev.ReadCd(out _, out _, 0, 2352 + 96, 1, MmcSectorTypes.AllTypes, false, false, true,
                               MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Raw,
                               _dev.Timeout, out _);
        }

        bool SupportsPqSubchannel()
        {
            _dumpLog.WriteLine("Checking if drive supports PQ subchannel reading...");
            UpdateStatus?.Invoke("Checking if drive supports PQ subchannel reading...");

            return!_dev.ReadCd(out _, out _, 0, 2352 + 16, 1, MmcSectorTypes.AllTypes, false, false, true,
                               MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Q16,
                               _dev.Timeout, out _);
        }
    }
}