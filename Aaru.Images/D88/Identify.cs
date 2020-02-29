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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public partial class D88
    {
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            // Even if disk name is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
            var shiftjis = Encoding.GetEncoding("shift_jis");

            if(stream.Length < Marshal.SizeOf<D88Header>())
                return false;

            byte[] hdrB = new byte[Marshal.SizeOf<D88Header>()];
            stream.Read(hdrB, 0, hdrB.Length);

            D88Header d88Hdr = Marshal.ByteArrayToStructureLittleEndian<D88Header>(hdrB);

            AaruConsole.DebugWriteLine("D88 plugin", "d88hdr.name = \"{0}\"",
                                       StringHandlers.CToString(d88Hdr.name, shiftjis));

            AaruConsole.DebugWriteLine("D88 plugin", "d88hdr.reserved is empty? = {0}",
                                       d88Hdr.reserved.SequenceEqual(reservedEmpty));

            AaruConsole.DebugWriteLine("D88 plugin", "d88hdr.write_protect = 0x{0:X2}", d88Hdr.write_protect);

            AaruConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_type = {0} ({1})", d88Hdr.disk_type,
                                       (byte)d88Hdr.disk_type);

            AaruConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_size = {0}", d88Hdr.disk_size);

            if(d88Hdr.disk_size != stream.Length)
                return false;

            if(d88Hdr.disk_type != DiskType.D2  &&
               d88Hdr.disk_type != DiskType.Dd2 &&
               d88Hdr.disk_type != DiskType.Hd2)
                return false;

            if(!d88Hdr.reserved.SequenceEqual(reservedEmpty))
                return false;

            int counter = 0;

            foreach(int t in d88Hdr.track_table)
            {
                if(t > 0)
                    counter++;

                if(t < 0 ||
                   t > stream.Length)
                    return false;
            }

            AaruConsole.DebugWriteLine("D88 plugin", "{0} tracks", counter);

            return counter > 0;
        }
    }
}