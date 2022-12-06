// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Options.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Lists all options supported by writable media image formats.
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
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using JetBrains.Annotations;

namespace Aaru.Commands.Image
{
    internal sealed class ListOptionsCommand : Command
    {
        public ListOptionsCommand() : base("options", "Lists all options supported by writable media images.") =>
            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));

        public static int Invoke(bool debug, bool verbose)
        {
            MainClass.PrintCopyright();

            if(debug)
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            AaruConsole.DebugWriteLine("List-Options command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("List-Options command", "--verbose={0}", verbose);
            Statistics.AddCommand("list-options");

            PluginBase plugins = GetPluginBase.Instance;

            AaruConsole.WriteLine("Read/Write media images options:");

            foreach(KeyValuePair<string, IWritableImage> kvp in plugins.WritableImages)
            {
                List<(string name, Type type, string description, object @default)> options =
                    kvp.Value.SupportedOptions.ToList();

                if(options.Count == 0)
                    continue;

                AaruConsole.WriteLine("\tOptions for {0}:", kvp.Value.Name);
                AaruConsole.WriteLine("\t\t{0,-20} {1,-10} {2,-12} {3,-8}", "Name", "Type", "Default", "Description");

                foreach((string name, Type type, string description, object @default) option in
                    options.OrderBy(t => t.name))
                    AaruConsole.WriteLine("\t\t{0,-20} {1,-10} {2,-12} {3,-8}", option.name, TypeToString(option.type),
                                          option.@default, option.description);

                AaruConsole.WriteLine();
            }

            return (int)ErrorNumber.NoError;
        }

        [NotNull]
        static string TypeToString([NotNull] Type type)
        {
            if(type == typeof(bool))
                return "boolean";

            if(type == typeof(sbyte) ||
               type == typeof(short) ||
               type == typeof(int)   ||
               type == typeof(long))
                return "signed number";

            if(type == typeof(byte)   ||
               type == typeof(ushort) ||
               type == typeof(uint)   ||
               type == typeof(ulong))
                return "number";

            if(type == typeof(float) ||
               type == typeof(double))
                return "float number";

            if(type == typeof(Guid))
                return "uuid";

            return type == typeof(string) ? "string" : type.ToString();
        }
    }
}