// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helper logic used on sidecar creation.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Core
{
    public sealed partial class Sidecar
    {
        /// <summary>Converts a LBA to MM:SS:FF string for CDs</summary>
        /// <param name="lba">LBA</param>
        /// <returns>MM:SS:FF</returns>
        static string LbaToMsf(long lba)
        {
            long m, s, f;

            if(lba >= -150)
            {
                m   =  (lba + 150) / (75 * 60);
                lba -= m           * (75 * 60);
                s   =  (lba + 150) / 75;
                lba -= s           * 75;
                f   =  lba + 150;
            }
            else
            {
                m   =  (lba + 450150) / (75 * 60);
                lba -= m              * (75 * 60);
                s   =  (lba + 450150) / 75;
                lba -= s              * 75;
                f   =  lba + 450150;
            }

            return $"{m}:{s:D2}:{f:D2}";
        }

        /// <summary>Converts a LBA to MM:SS:FF string for DDCDs</summary>
        /// <param name="lba">LBA</param>
        /// <returns>MM:SS:FF</returns>
        static string DdcdLbaToMsf(long lba)
        {
            long h, m, s, f;

            if(lba >= -150)
            {
                h   =  (lba + 150) / (75 * 60 * 60);
                lba -= h           * (75 * 60 * 60);
                m   =  (lba + 150) / (75 * 60);
                lba -= m           * (75 * 60);
                s   =  (lba + 150) / 75;
                lba -= s           * 75;
                f   =  lba + 150;
            }
            else
            {
                h   =  (lba + (450150 * 2)) / (75 * 60 * 60);
                lba -= h                    * (75 * 60 * 60);
                m   =  (lba + (450150 * 2)) / (75 * 60);
                lba -= m                    * (75 * 60);
                s   =  (lba + (450150 * 2)) / 75;
                lba -= s                    * 75;
                f   =  lba + (450150 * 2);
            }

            return string.Format("{3}:{0:D2}:{1:D2}:{2:D2}", m, s, f, h);
        }
    }
}