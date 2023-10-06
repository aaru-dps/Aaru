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
//     Identifies Quasi88 disk images.
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

using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class D88
{
#region IMediaImage Members

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        // Even if disk name is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
        var shiftjis = Encoding.GetEncoding("shift_jis");

        if(stream.Length < Marshal.SizeOf<Header>())
            return false;

        var hdrB = new byte[Marshal.SizeOf<Header>()];
        stream.EnsureRead(hdrB, 0, hdrB.Length);

        Header hdr = Marshal.ByteArrayToStructureLittleEndian<Header>(hdrB);

        AaruConsole.DebugWriteLine(MODULE_NAME, "d88hdr.name = \"{0}\"", StringHandlers.CToString(hdr.name, shiftjis));

        AaruConsole.DebugWriteLine(MODULE_NAME, "d88hdr.reserved is empty? = {0}",
                                   hdr.reserved.SequenceEqual(_reservedEmpty));

        AaruConsole.DebugWriteLine(MODULE_NAME, "d88hdr.write_protect = 0x{0:X2}", hdr.write_protect);

        AaruConsole.DebugWriteLine(MODULE_NAME, "d88hdr.disk_type = {0} ({1})", hdr.disk_type, (byte)hdr.disk_type);

        AaruConsole.DebugWriteLine(MODULE_NAME, "d88hdr.disk_size = {0}", hdr.disk_size);

        if(hdr.disk_size != stream.Length)
            return false;

        if(hdr.disk_type != DiskType.D2 && hdr.disk_type != DiskType.Dd2 && hdr.disk_type != DiskType.Hd2)
            return false;

        if(!hdr.reserved.SequenceEqual(_reservedEmpty))
            return false;

        var counter = 0;

        foreach(int t in hdr.track_table)
        {
            if(t > 0)
                counter++;

            if(t < 0 || t > stream.Length)
                return false;
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization._0_tracks, counter);

        return counter > 0;
    }

#endregion
}