// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PartPlugin.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines methods to be used by partitioning scheme plugins and several
//     constants.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;

namespace DiscImageChef.PartPlugins
{
    /// <summary>
    /// Abstract class to implement partitioning schemes interpreting plugins.
    /// </summary>
    public abstract class PartPlugin
    {
        /// <summary>Plugin name.</summary>
        public string Name;
        /// <summary>Plugin UUID.</summary>
        public Guid PluginUUID;

        protected PartPlugin()
        {
        }

        /// <summary>
        /// Interprets a partitioning scheme.
        /// </summary>
        /// <returns><c>true</c>, if partitioning scheme is recognized, <c>false</c> otherwise.</returns>
        /// <param name="imagePlugin">Disk image.</param>
        /// <param name="partitions">Returns list of partitions.</param>
        public abstract bool GetInformation(ImagePlugins.ImagePlugin imagePlugin, out List<CommonTypes.Partition> partitions);
    }
}