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
//     Identifies NHD r0 disk images.
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
    public partial class Nhdr0
    {
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            // Even if comment is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
            var shiftjis = Encoding.GetEncoding("shift_jis");

            if(stream.Length < Marshal.SizeOf<Nhdr0Header>())
                return false;

            byte[] hdrB = new byte[Marshal.SizeOf<Nhdr0Header>()];
            stream.Read(hdrB, 0, hdrB.Length);

            nhdhdr = Marshal.ByteArrayToStructureLittleEndian<Nhdr0Header>(hdrB);

            if(!nhdhdr.szFileID.SequenceEqual(signature))
                return false;

            AaruConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.szFileID = \"{0}\"",
                                       StringHandlers.CToString(nhdhdr.szFileID, shiftjis));

            AaruConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.reserved1 = {0}", nhdhdr.reserved1);

            AaruConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.szComment = \"{0}\"",
                                       StringHandlers.CToString(nhdhdr.szComment, shiftjis));

            AaruConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.dwHeadSize = {0}", nhdhdr.dwHeadSize);
            AaruConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.dwCylinder = {0}", nhdhdr.dwCylinder);
            AaruConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.wHead = {0}", nhdhdr.wHead);
            AaruConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.wSect = {0}", nhdhdr.wSect);
            AaruConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.wSectLen = {0}", nhdhdr.wSectLen);

            return true;
        }
    }
}