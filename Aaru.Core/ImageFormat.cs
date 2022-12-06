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
//     Detects disc image format.
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

using System;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;

namespace Aaru.Core
{
    /// <summary>Core media image format operations</summary>
    public static class ImageFormat
    {
        /// <summary>Detects the image plugin that recognizes the data inside a filter</summary>
        /// <param name="imageFilter">Filter</param>
        /// <returns>Detected image plugin</returns>
        public static IMediaImage Detect(IFilter imageFilter)
        {
            try
            {
                PluginBase plugins = GetPluginBase.Instance;

                IMediaImage imageFormat = null;

                // Check all but RAW plugin
                foreach(IMediaImage imagePlugin in plugins.ImagePluginsList.Values.Where(imagePlugin =>
                    imagePlugin.Id != new Guid("12345678-AAAA-BBBB-CCCC-123456789000")))
                    try
                    {
                        AaruConsole.DebugWriteLine("Format detection", "Trying plugin {0}", imagePlugin.Name);

                        if(!imagePlugin.Identify(imageFilter))
                            continue;

                        imageFormat = imagePlugin;

                        break;
                    }
                    #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                    catch
                    {
                        // ignored
                    }

                if(imageFormat != null)
                    return imageFormat;

                // Check only RAW plugin
                foreach(IMediaImage imagePlugin in plugins.ImagePluginsList.Values.Where(imagePlugin =>
                    imagePlugin.Id == new Guid("12345678-AAAA-BBBB-CCCC-123456789000")))
                    try
                    {
                        AaruConsole.DebugWriteLine("Format detection", "Trying plugin {0}", imagePlugin.Name);

                        if(!imagePlugin.Identify(imageFilter))
                            continue;

                        imageFormat = imagePlugin;

                        break;
                    }
                    #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                    catch
                    {
                        // ignored
                    }

                // Still not recognized
                return imageFormat;
            }
            catch
            {
                return null;
            }
        }
    }
}