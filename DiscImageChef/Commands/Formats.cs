/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Formats.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Verbs.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Implements the 'formats' verb.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System;
using System.Collections.Generic;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using DiscImageChef.Filesystems;
using DiscImageChef.Console;

namespace DiscImageChef.Commands
{
    public static class Formats
    {
        public static void ListFormats(FormatsOptions FormatsOptions)
        {
            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();

            DicConsole.WriteLine("Supported disc image formats:");
            if(FormatsOptions.Verbose)
                DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, ImagePlugin> kvp in plugins.ImagePluginsList)
            {
                if(FormatsOptions.Verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.PluginUUID, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);
            }
            DicConsole.WriteLine();
            DicConsole.WriteLine("Supported filesystems:");
            if(FormatsOptions.Verbose)
                DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, Filesystem> kvp in plugins.PluginsList)
            {
                if(FormatsOptions.Verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.PluginUUID, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);
            }
            DicConsole.WriteLine();
            DicConsole.WriteLine("Supported partitioning schemes:");
            if(FormatsOptions.Verbose)
                DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, PartPlugin> kvp in plugins.PartPluginsList)
            {
                if(FormatsOptions.Verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.PluginUUID, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);
            }

            Core.Statistics.AddCommand("formats");
        }
    }
}

