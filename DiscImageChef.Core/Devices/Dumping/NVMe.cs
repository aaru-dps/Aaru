// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : NVMe.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps storage using NVMe protocol.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using DiscImageChef.Core.Logging;
using DiscImageChef.Devices;
using DiscImageChef.DiscImages;
using DiscImageChef.Metadata;
using Schemas;

namespace DiscImageChef.Core.Devices.Dumping
{
    public static class NvMe
    {
        public static void Dump(Device     dev, string devicePath, IWritableImage outputPlugin, ushort retryPasses,
                                bool       force, bool dumpRaw, bool              persistent, bool     stopOnError,
                                ref Resume resume,
                                ref
                                    DumpLog dumpLog, Encoding encoding, string outputPrefix,
                                string      outputPath,
                                Dictionary<string, string>
                                    formatOptions, CICMMetadataType preSidecar)
        {
            throw new NotImplementedException("NVMe devices not yet supported.");
        }
    }
}