// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ArchiveFormat.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Detects archive format.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;

namespace Aaru.Core;

/// <summary>Core archive format operations</summary>
public static class ArchiveFormat
{
    const string MODULE_NAME = "Format detection";

    /// <summary>Detects the archive plugin that recognizes the data inside a filter</summary>
    /// <param name="archiveFilter">Filter</param>
    /// <returns>Detected archive plugin</returns>
    public static IArchive Detect(IFilter archiveFilter)
    {
        try
        {
            PluginRegister plugins = PluginRegister.Singleton;

            IArchive format = null;

            // Check all but RAW plugin
            foreach(IArchive plugin in plugins.Archives.Values)
            {
                if(plugin is null) continue;

                try
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Trying_plugin_0, plugin.Name);

                    if(!plugin.Identify(archiveFilter)) continue;

                    format = plugin;

                    break;
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
                {
                    // ignored
                }
            }

            // Not recognized
            return format;
        }
        catch
        {
            return null;
        }
    }
}