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
//     Identifies Ray Arachelian's disk images.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Text.RegularExpressions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class RayDim
    {
        /// <inheritdoc />
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < Marshal.SizeOf<Header>())
                return false;

            byte[] buffer = new byte[Marshal.SizeOf<Header>()];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            Header header = Marshal.ByteArrayToStructureLittleEndian<Header>(buffer);

            string signature = StringHandlers.CToString(header.signature);

            AaruConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.signature = {0}", signature);
            AaruConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.diskType = {0}", header.diskType);
            AaruConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.heads = {0}", header.heads);

            AaruConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.cylinders = {0}",
                                       header.cylinders);

            AaruConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.sectorsPerTrack = {0}",
                                       header.sectorsPerTrack);

            var   sx = new Regex(REGEX_SIGNATURE);
            Match sm = sx.Match(signature);

            AaruConsole.DebugWriteLine("Ray Arachelian's Disk IMage plugin", "header.signature matches? = {0}",
                                       sm.Success);

            return sm.Success;
        }
    }
}