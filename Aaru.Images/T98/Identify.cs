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
//     Identifies T98 disk images.
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

using System;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;

namespace Aaru.DiscImages
{
    public sealed partial class T98
    {
        /// <inheritdoc />
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length % 256 != 0)
                return false;

            byte[] hdrB = new byte[256];
            stream.Read(hdrB, 0, hdrB.Length);

            for(int i = 4; i < 256; i++)
                if(hdrB[i] != 0)
                    return false;

            int cylinders = BitConverter.ToInt32(hdrB, 0);

            AaruConsole.DebugWriteLine("T98 plugin", "cylinders = {0}", cylinders);

            // This format is expanding, so length can be smaller
            // Just grow it, I won't risk false positives...
            return stream.Length == (cylinders * 8 * 33 * 256) + 256;
        }
    }
}