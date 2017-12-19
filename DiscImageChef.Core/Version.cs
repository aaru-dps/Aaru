// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Version.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Returns DiscImageChef version.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.Interop;
using Schemas;

namespace DiscImageChef.Core
{
    public static class Version
    {
        public static SoftwareType GetSoftwareType(PlatformID platform)
        {
            return new SoftwareType
            {
                Name = "DiscImageChef",
                OperatingSystem = platform.ToString(),
                Version = typeof(Version).Assembly.GetName().Version.ToString()
            };
        }

        public static string GetVersion()
        {
            return typeof(Version).Assembly.GetName().Version.ToString();
        }
    }
}