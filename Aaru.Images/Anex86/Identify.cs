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
//     Identifies Anex86 disk images.
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
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public partial class Anex86
    {
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < Marshal.SizeOf<Anex86Header>())
                return false;

            byte[] hdrB = new byte[Marshal.SizeOf<Anex86Header>()];
            stream.Read(hdrB, 0, hdrB.Length);

            _fdihdr = Marshal.SpanToStructureLittleEndian<Anex86Header>(hdrB);

            AaruConsole.DebugWriteLine("Anex86 plugin", "fdihdr.unknown = {0}", _fdihdr.unknown);
            AaruConsole.DebugWriteLine("Anex86 plugin", "fdihdr.hddtype = {0}", _fdihdr.hddtype);
            AaruConsole.DebugWriteLine("Anex86 plugin", "fdihdr.hdrSize = {0}", _fdihdr.hdrSize);
            AaruConsole.DebugWriteLine("Anex86 plugin", "fdihdr.dskSize = {0}", _fdihdr.dskSize);
            AaruConsole.DebugWriteLine("Anex86 plugin", "fdihdr.bps = {0}", _fdihdr.bps);
            AaruConsole.DebugWriteLine("Anex86 plugin", "fdihdr.spt = {0}", _fdihdr.spt);
            AaruConsole.DebugWriteLine("Anex86 plugin", "fdihdr.heads = {0}", _fdihdr.heads);
            AaruConsole.DebugWriteLine("Anex86 plugin", "fdihdr.cylinders = {0}", _fdihdr.cylinders);

            return stream.Length   == _fdihdr.hdrSize + _fdihdr.dskSize &&
                   _fdihdr.dskSize == _fdihdr.bps * _fdihdr.spt * _fdihdr.heads * _fdihdr.cylinders;
        }
    }
}