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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Partitions;
using Mono.Options;

namespace DiscImageChef.Commands
{
    class FormatsCommand : Command
    {
        bool showHelp;

        public FormatsCommand() : base("formats",
                                       "Lists all supported disc images, partition schemes and file systems.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name}",
                "",
                Help,
                {"help|h|?", "Show this message and exit.", v => showHelp = v != null}
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            List<string> extra = Options.Parse(arguments);

            if(showHelp)
            {
                Options.WriteOptionDescriptions(CommandSet.Out);
                return (int)ErrorNumber.HelpRequested;
            }

            MainClass.PrintCopyright();
            if(MainClass.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
            if(MainClass.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            if(extra.Count > 0)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");
                return (int)ErrorNumber.UnexpectedArgumentCount;
            }

            DicConsole.DebugWriteLine("Formats command", "--debug={0}",   MainClass.Debug);
            DicConsole.DebugWriteLine("Formats command", "--verbose={0}", MainClass.Verbose);

            PluginBase  plugins     = GetPluginBase.Instance;
            FiltersList filtersList = new FiltersList();

            DicConsole.WriteLine("Supported filters ({0}):", filtersList.Filters.Count);
            if(MainClass.Verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tFilter");
            foreach(KeyValuePair<string, IFilter> kvp in filtersList.Filters)
                if(MainClass.Verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.Id, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Read-only media image formats ({0}):",
                                 plugins.ImagePluginsList.Count(t => !t.Value.GetType().GetInterfaces()
                                                                       .Contains(typeof(IWritableImage))));
            if(MainClass.Verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, IMediaImage> kvp in plugins.ImagePluginsList.Where(t => !t.Value.GetType()
                                                                                                   .GetInterfaces()
                                                                                                   .Contains(typeof(
                                                                                                                 IWritableImage
                                                                                                             ))))
                if(MainClass.Verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.Id, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Read/write media image formats ({0}):", plugins.WritableImages.Count);
            if(MainClass.Verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, IWritableImage> kvp in plugins.WritableImages)
                if(MainClass.Verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.Id, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Supported filesystems for identification and information only ({0}):",
                                 plugins.PluginsList.Count(t => !t.Value.GetType().GetInterfaces()
                                                                  .Contains(typeof(IReadOnlyFilesystem))));
            if(MainClass.Verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, IFilesystem> kvp in plugins.PluginsList.Where(t => !t.Value.GetType()
                                                                                              .GetInterfaces()
                                                                                              .Contains(typeof(
                                                                                                            IReadOnlyFilesystem
                                                                                                        ))))
                if(MainClass.Verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.Id, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Supported filesystems that can read their contents ({0}):",
                                 plugins.ReadOnlyFilesystems.Count);
            if(MainClass.Verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, IReadOnlyFilesystem> kvp in plugins.ReadOnlyFilesystems)
                if(MainClass.Verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.Id, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Supported partitioning schemes ({0}):", plugins.PartPluginsList.Count);
            if(MainClass.Verbose) DicConsole.VerboseWriteLine("GUID\t\t\t\t\tPlugin");
            foreach(KeyValuePair<string, IPartition> kvp in plugins.PartPluginsList)
                if(MainClass.Verbose)
                    DicConsole.VerboseWriteLine("{0}\t{1}", kvp.Value.Id, kvp.Value.Name);
                else
                    DicConsole.WriteLine(kvp.Value.Name);

            Statistics.AddCommand("formats");
            return (int)ErrorNumber.NoError;
        }
    }
}