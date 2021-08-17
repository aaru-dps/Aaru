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
// Copyright © 2011-2021 Natalia Portillo
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

namespace Aaru.Commands.Filesystem
{
    internal sealed class ListOptionsCommand : Command
    {
        public ListOptionsCommand() : base("options", "Lists all options supported by read-only filesystems.") =>
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

            AaruConsole.WriteLine("Read-only filesystems options:");

            foreach(KeyValuePair<string, IReadOnlyFilesystem> kvp in plugins.ReadOnlyFilesystems)
            {
                List<(string name, Type type, string description)> options = kvp.Value.SupportedOptions.ToList();

                if(options.Count == 0)
                    continue;

                AaruConsole.WriteLine("\tOptions for {0}:", kvp.Value.Name);
                AaruConsole.WriteLine("\t\t{0,-16} {1,-16} {2,-8}", "Name", "Type", "Description");

                foreach((string name, Type type, string description) option in options.OrderBy(t => t.name))
                    AaruConsole.WriteLine("\t\t{0,-16} {1,-16} {2,-8}", option.name, TypeToString(option.type),
                                          option.description);

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