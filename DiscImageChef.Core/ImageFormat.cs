// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;

namespace DiscImageChef.Core
{
    public static class ImageFormat
    {
        /// <summary>
        ///     Detects the image plugin that recognizes the data inside a filter
        /// </summary>
        /// <param name="imageFilter">Filter</param>
        /// <returns>Detected image plugin</returns>
        public static IMediaImage Detect(IFilter imageFilter)
        {
            try
            {
                PluginBase plugins = GetPluginBase.Instance;

                IMediaImage imageFormat = null;

                // Check all but RAW plugin
                foreach(IMediaImage imageplugin in plugins.ImagePluginsList.Values.Where(imageplugin =>
                                                                                             imageplugin.Id !=
                                                                                             new
                                                                                                 Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                )
                    try
                    {
                        DicConsole.DebugWriteLine("Format detection", "Trying plugin {0}", imageplugin.Name);
                        if(!imageplugin.Identify(imageFilter)) continue;

                        imageFormat = imageplugin;
                        break;
                    }
                    #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                    catch
                    {
                        // ignored
                    }

                if(imageFormat != null) return imageFormat;

                // Check only RAW plugin
                foreach(IMediaImage imageplugin in plugins.ImagePluginsList.Values.Where(imageplugin =>
                                                                                             imageplugin.Id ==
                                                                                             new
                                                                                                 Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                )
                    try
                    {
                        DicConsole.DebugWriteLine("Format detection", "Trying plugin {0}", imageplugin.Name);
                        if(!imageplugin.Identify(imageFilter)) continue;

                        imageFormat = imageplugin;
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
            catch { return null; }
        }
    }
}