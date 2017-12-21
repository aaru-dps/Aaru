// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Formats.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'formats' verb.
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

using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.DiscImages;
using DiscImageChef.Partitions;

namespace DiscImageChef.Commands
{
    static class Formats
    {
        internal static void ListFormats(FormatsOptions formatsOptions)
        {
            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();
            FiltersList filtersList = new FiltersList();

            DicConsole.WriteLine("Supported filters:");
            if(formatsOptions.Verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tFilter");
            foreach(KeyValuePair<string, Filter> kvp in filtersList.filtersList)
                if(formatsOptions.Verbose) DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.UUID, kvp.Value.Name);
                else DicConsole.WriteLine(kvp.Value.Name);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Supported disc image formats:");
            if(formatsOptions.Verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, ImagePlugin> kvp in plugins.ImagePluginsList)
                if(formatsOptions.Verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.PluginUuid, kvp.Value.Name);
                else DicConsole.WriteLine(kvp.Value.Name);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Supported filesystems:");
            if(formatsOptions.Verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, Filesystem> kvp in plugins.PluginsList)
                if(formatsOptions.Verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.PluginUUID, kvp.Value.Name);
                else DicConsole.WriteLine(kvp.Value.Name);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Supported partitioning schemes:");
            if(formatsOptions.Verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, PartitionPlugin> kvp in plugins.PartPluginsList)
                if(formatsOptions.Verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.PluginUuid, kvp.Value.Name);
                else DicConsole.WriteLine(kvp.Value.Name);

            Core.Statistics.AddCommand("formats");
        }
    }
}