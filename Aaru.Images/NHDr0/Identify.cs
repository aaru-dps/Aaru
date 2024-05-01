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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class Nhdr0
{
#region IWritableImage Members

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        // Even if comment is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
        var shiftjis = Encoding.GetEncoding("shift_jis");

        if(stream.Length < Marshal.SizeOf<Header>()) return false;

        var hdrB = new byte[Marshal.SizeOf<Header>()];
        stream.EnsureRead(hdrB, 0, hdrB.Length);

        _nhdhdr = Marshal.ByteArrayToStructureLittleEndian<Header>(hdrB);

        if(!_nhdhdr.szFileID.SequenceEqual(_signature)) return false;

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "nhdhdr.szFileID = \"{0}\"",
                                   StringHandlers.CToString(_nhdhdr.szFileID, shiftjis));

        AaruConsole.DebugWriteLine(MODULE_NAME, "nhdhdr.reserved1 = {0}", _nhdhdr.reserved1);

        AaruConsole.DebugWriteLine(MODULE_NAME,
                                   "nhdhdr.szComment = \"{0}\"",
                                   StringHandlers.CToString(_nhdhdr.szComment, shiftjis));

        AaruConsole.DebugWriteLine(MODULE_NAME, "nhdhdr.dwHeadSize = {0}", _nhdhdr.dwHeadSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "nhdhdr.dwCylinder = {0}", _nhdhdr.dwCylinder);
        AaruConsole.DebugWriteLine(MODULE_NAME, "nhdhdr.wHead = {0}",      _nhdhdr.wHead);
        AaruConsole.DebugWriteLine(MODULE_NAME, "nhdhdr.wSect = {0}",      _nhdhdr.wSect);
        AaruConsole.DebugWriteLine(MODULE_NAME, "nhdhdr.wSectLen = {0}",   _nhdhdr.wSectLen);

        return true;
    }

#endregion
}