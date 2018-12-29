// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core.
//
// --[ Description ] ----------------------------------------------------------
//
//     Detects media types in MultiMediaCommand devices
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;

namespace DiscImageChef.Core.Media.Detection
{
    public static class MMC
    {
        /// <summary>
        ///     Checks if the media corresponds to CD-i.
        /// </summary>
        /// <param name="sector0">Contents of LBA 0, with all headers.</param>
        /// <param name="sector16">Contents of LBA 0, with all headers.</param>
        /// <returns><c>true</c> if it corresponds to a CD-i, <c>false</c>otherwise.</returns>
        public static bool IsCdi(byte[] sector0, byte[] sector16)
        {
            if(sector0?.Length != 2352 || sector16?.Length != 2352) return false;

            byte[] syncMark = {0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00};
            byte[] cdiMark  = {0x01, 0x43, 0x44, 0x2D};
            byte[] testMark = new byte[12];
            Array.Copy(sector0, 0, testMark, 0, 12);

            bool hiddenData = syncMark.SequenceEqual(testMark) &&
                              (sector0[0xF] == 0 || sector0[0xF] == 1 || sector0[0xF] == 2);

            if(!hiddenData || sector0[0xF] != 2) return false;

            testMark = new byte[4];
            Array.Copy(sector16, 24, testMark, 0, 4);
            return cdiMark.SequenceEqual(testMark);
        }
    }
}