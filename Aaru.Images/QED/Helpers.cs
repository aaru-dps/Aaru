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
//     Contains helpers for QEMU Enhanced Disk images.
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

namespace Aaru.DiscImages;

public sealed partial class Qed
{
    static bool IsPowerOfTwo(uint x)
    {
        while((x & 1) == 0 &&
              x       > 1)
            x >>= 1;

        return x == 1;
    }

    static int Ctz32(uint val)
    {
        int cnt = 0;

        if((val & 0xFFFF) == 0)
        {
            cnt +=  16;
            val >>= 16;
        }

        if((val & 0xFF) == 0)
        {
            cnt +=  8;
            val >>= 8;
        }

        if((val & 0xF) == 0)
        {
            cnt +=  4;
            val >>= 4;
        }

        if((val & 0x3) == 0)
        {
            cnt +=  2;
            val >>= 2;
        }

        if((val & 0x1) == 0)
        {
            cnt++;
            val >>= 1;
        }

        if((val & 0x1) == 0)
            cnt++;

        return cnt;
    }
}