// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ListDevices.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'media-info' verb.
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
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Devices;
using Mono.Options;

namespace DiscImageChef.Commands
{
    internal class ListDevicesCommand : Command
    {
        private bool showHelp;

        public ListDevicesCommand() : base("list-devices", "Lists all connected devices.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name} [dic-remote-host]",
                "",
                Help,
                {"help|h|?", "Show this message and exit.", v => showHelp = v != null}
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            var extra = Options.Parse(arguments);

            if (showHelp)
            {
                Options.WriteOptionDescriptions(CommandSet.Out);
                return (int) ErrorNumber.HelpRequested;
            }

            MainClass.PrintCopyright();
            if (MainClass.Debug) DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;
            if (MainClass.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
            Statistics.AddCommand("list-devices");

            string dicRemote = null;

            if (extra.Count > 1)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");
                return (int) ErrorNumber.UnexpectedArgumentCount;
            }

            if (extra.Count == 1) dicRemote = extra[0];

            DicConsole.DebugWriteLine("List-Devices command", "--debug={0}", MainClass.Debug);
            DicConsole.DebugWriteLine("List-Devices command", "--verbose={0}", MainClass.Verbose);

            var devices = Device.ListDevices(dicRemote);

            if (devices == null || devices.Length == 0)
            {
                DicConsole.WriteLine("No known devices attached.");
            }
            else
            {
                devices = devices.OrderBy(d => d.Path).ToArray();

                DicConsole.WriteLine("{0,-22}|{1,-16}|{2,-24}|{3,-24}|{4,-10}|{5,-10}", "Path", "Vendor", "Model",
                    "Serial", "Bus", "Supported?");
                DicConsole.WriteLine("{0,-22}+{1,-16}+{2,-24}+{3,-24}+{4,-10}+{5,-10}", "----------------------",
                    "----------------", "------------------------", "------------------------",
                    "----------", "----------");
                foreach (var dev in devices)
                    DicConsole.WriteLine("{0,-22}|{1,-16}|{2,-24}|{3,-24}|{4,-10}|{5,-10}", dev.Path, dev.Vendor,
                        dev.Model, dev.Serial, dev.Bus, dev.Supported);
            }

            return (int) ErrorNumber.NoError;
        }
    }
}