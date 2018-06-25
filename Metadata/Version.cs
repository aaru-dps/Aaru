// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Statistics.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XML metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Returns DiscImageChef version in XML software type format.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.CommonTypes.Interop;
using Schemas;

namespace DiscImageChef.CommonTypes.Metadata
{
    public static class Version
    {
        /// <summary>
        ///     Gets XML software type for the running version
        /// </summary>
        /// <returns>XML software type</returns>
        public static SoftwareType GetSoftwareType()
        {
            return new SoftwareType
            {
                Name            = "DiscImageChef",
                OperatingSystem = DetectOS.GetRealPlatformID().ToString(),
                Version         = typeof(Version).Assembly.GetName().Version.ToString()
            };
        }
    }
}