// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ListNamespaces.cs
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;

namespace Aaru.Commands
{
    internal sealed class ListNamespacesCommand : Command
    {
        public ListNamespacesCommand() : base("list-namespaces",
                                              "Lists all namespaces supported by read-only filesystems.") =>
            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));

        public static int Invoke(bool debug, bool verbose)
        {
            MainClass.PrintCopyright();

            if(debug)
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            AaruConsole.DebugWriteLine("List-Namespaces command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("List-Namespaces command", "--verbose={0}", verbose);
            Statistics.AddCommand("list-namespaces");

            PluginBase plugins = GetPluginBase.Instance;

            foreach(KeyValuePair<string, IReadOnlyFilesystem> kvp in
                plugins.ReadOnlyFilesystems.Where(kvp => !(kvp.Value.Namespaces is null)))
            {
                AaruConsole.WriteLine("\tNamespaces for {0}:", kvp.Value.Name);
                AaruConsole.WriteLine("\t\t{0,-16} {1,-16}", "Namespace", "Description");

                foreach(KeyValuePair<string, string> @namespace in kvp.Value.Namespaces.OrderBy(t => t.Key))
                    AaruConsole.WriteLine("\t\t{0,-16} {1,-16}", @namespace.Key, @namespace.Value);

                AaruConsole.WriteLine();
            }

            return (int)ErrorNumber.NoError;
        }
    }
}