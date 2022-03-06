// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for Apple DiskCopy 4.2 disk images.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages;

public sealed partial class DiskCopy42
{
    static uint CheckSum(byte[] buffer)
    {
        uint dc42Chk = 0;

        if((buffer.Length & 0x01) == 0x01)
            return 0xFFFFFFFF;

        for(uint i = 0; i < buffer.Length; i += 2)
        {
            dc42Chk += (uint)(buffer[i] << 8);
            dc42Chk += buffer[i + 1];
            dc42Chk =  (dc42Chk >> 1) | (dc42Chk << 31);
        }

        return dc42Chk;
    }
}