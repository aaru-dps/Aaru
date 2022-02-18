// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ImageFormat.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Gets a new instance of all known plugins.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Core
{
    /// <summary>Plugin base operations</summary>
    public static class GetPluginBase
    {
        /// <summary>Gets an instance with all the known plugins</summary>
        public static PluginBase Instance
        {
            get
            {
                var instance = new PluginBase();

                IPluginRegister checksumRegister    = new Register();
                IPluginRegister imagesRegister      = new DiscImages.Register();
                IPluginRegister filesystemsRegister = new Aaru.Filesystems.Register();
                IPluginRegister filtersRegister     = new Filters.Register();
                IPluginRegister partitionsRegister  = new Aaru.Partitions.Register();

                instance.AddPlugins(checksumRegister);
                instance.AddPlugins(imagesRegister);
                instance.AddPlugins(filesystemsRegister);
                instance.AddPlugins(filtersRegister);
                instance.AddPlugins(partitionsRegister);

                return instance;
            }
        }
    }
}