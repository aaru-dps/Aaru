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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;

namespace Aaru.Core;

/// <summary>Core media image format operations</summary>
public static class ImageFormat
{
    const string MODULE_NAME = "Format detection";

    /// <summary>Detects the image plugin that recognizes the data inside a filter</summary>
    /// <param name="imageFilter">Filter</param>
    /// <returns>Detected image plugin</returns>
    public static IBaseImage Detect(IFilter imageFilter)
    {
        try
        {
            PluginBase.Init();
            PluginRegister plugins = PluginRegister.Singleton;

            IBaseImage imageFormat = null;

            // Check all but RAW plugin
            foreach(Type pluginType in plugins.MediaImages.Values)
            {
                if(Activator.CreateInstance(pluginType) is not IMediaImage imagePlugin)
                    continue;

                if(imagePlugin.Id == new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                    continue;

                try
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Trying_plugin_0, imagePlugin.Name);

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
            }

            if(imageFormat != null)
                return imageFormat;

            // Check all but RAW plugin
            foreach(Type pluginType in plugins.ByteAddressableImages.Values)
            {
                try
                {
                    if(Activator.CreateInstance(pluginType) is not IByteAddressableImage imagePlugin)
                        continue;

                    if(imagePlugin.Id == new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                        continue;

                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Trying_plugin_0, imagePlugin.Name);

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
            }

            if(imageFormat != null)
                return imageFormat;

            // Check only RAW plugin
            foreach(Type pluginType in plugins.MediaImages.Values)
            {
                if(Activator.CreateInstance(pluginType) is not IMediaImage imagePlugin)
                    continue;

                if(imagePlugin.Id != new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                    continue;

                try
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Trying_plugin_0, imagePlugin.Name);

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