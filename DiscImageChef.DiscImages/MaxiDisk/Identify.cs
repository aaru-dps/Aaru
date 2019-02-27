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
//     Identifies MaxiDisk disk images.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.IO;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Helpers;

namespace DiscImageChef.DiscImages
{
    public partial class MaxiDisk
    {
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if(stream.Length < 8) return false;

            byte[] buffer = new byte[8];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            HdkHeader tmpHeader = Marshal.ByteArrayToStructureLittleEndian<HdkHeader>(buffer);

            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.unknown = {0}",        tmpHeader.unknown);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.diskType = {0}",       tmpHeader.diskType);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.heads = {0}",          tmpHeader.heads);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.cylinders = {0}",      tmpHeader.cylinders);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.bytesPerSector = {0}", tmpHeader.bytesPerSector);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.sectorsPerTrack = {0}",
                                      tmpHeader.sectorsPerTrack);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.unknown2 = {0}", tmpHeader.unknown2);
            DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.unknown3 = {0}", tmpHeader.unknown3);

            // This is hardcoded
            // But its possible values are unknown...
            //if(tmp_header.diskType > 11)
            //    return false;

            // Only floppies supported
            if(tmpHeader.heads == 0 || tmpHeader.heads > 2) return false;

            // No floppies with more than this?
            if(tmpHeader.cylinders > 90) return false;

            // Maximum supported bps is 16384
            if(tmpHeader.bytesPerSector > 7) return false;

            int expectedFileSize = tmpHeader.heads * tmpHeader.cylinders * tmpHeader.sectorsPerTrack *
                                   (128 << tmpHeader.bytesPerSector) + 8;

            return expectedFileSize == stream.Length;
        }
    }
}