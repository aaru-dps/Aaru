// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ListOptions.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Lists all options supported by read-only filesystems.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;

namespace Aaru.Commands.Image
{
    internal class ListOptionsCommand : Command
    {
        public ListOptionsCommand() : base("options",
                                           "Lists all options supported by writable media images.") =>
            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));

        public static int Invoke(bool debug, bool verbose)
        {
            MainClass.PrintCopyright();

            if(debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            DicConsole.DebugWriteLine("List-Options command", "--debug={0}", debug);
            DicConsole.DebugWriteLine("List-Options command", "--verbose={0}", verbose);
            Statistics.AddCommand("list-options");

            PluginBase plugins = GetPluginBase.Instance;

            DicConsole.WriteLine("Read/Write media images options:");

            foreach(KeyValuePair<string, IWritableImage> kvp in plugins.WritableImages)
            {
                List<(string name, Type type, string description, object @default)> options =
                    kvp.Value.SupportedOptions.ToList();

                if(options.Count == 0)
                    continue;

                DicConsole.WriteLine("\tOptions for {0}:", kvp.Value.Name);
                DicConsole.WriteLine("\t\t{0,-20} {1,-10} {2,-12} {3,-8}", "Name", "Type", "Default", "Description");

                foreach((string name, Type type, string description, object @default) option in
                    options.OrderBy(t => t.name))
                    DicConsole.WriteLine("\t\t{0,-20} {1,-10} {2,-12} {3,-8}", option.name, TypeToString(option.type),
                                         option.@default, option.description);

                DicConsole.WriteLine();
            }

            return(int)ErrorNumber.NoError;
        }

        static string TypeToString(Type type)
        {
            if(type == typeof(bool))
                return"boolean";

            if(type == typeof(sbyte) ||
               type == typeof(short) ||
               type == typeof(int)   ||
               type == typeof(long))
                return"signed number";

            if(type == typeof(byte)   ||
               type == typeof(ushort) ||
               type == typeof(uint)   ||
               type == typeof(ulong))
                return"number";

            if(type == typeof(float) ||
               type == typeof(double))
                return"float number";

            if(type == typeof(Guid))
                return"uuid";

            return type == typeof(string) ? "string" : type.ToString();
        }
    }
}