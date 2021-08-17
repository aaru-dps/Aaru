// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : List.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'list' command.
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

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Core;
using Aaru.Devices;
using JetBrains.Annotations;

namespace Aaru.Commands.Device
{
    internal sealed class ListDevicesCommand : Command
    {
        public ListDevicesCommand() : base("list", "Lists all connected devices.")
        {
            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ZeroOrOne,
                Description = "aaruremote host",
                Name        = "aaru-remote-host"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, [CanBeNull] string aaruRemoteHost)
        {
            MainClass.PrintCopyright();

            if(debug)
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("list-devices");

            AaruConsole.DebugWriteLine("List-Devices command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("List-Devices command", "--verbose={0}", verbose);

            DeviceInfo[] devices = Devices.Device.ListDevices(out bool isRemote, out string serverApplication,
                                                              out string serverVersion,
                                                              out string serverOperatingSystem,
                                                              out string serverOperatingSystemVersion,
                                                              out string serverArchitecture, aaruRemoteHost);

            if(isRemote)
            {
                Statistics.AddRemote(serverApplication, serverVersion, serverOperatingSystem,
                                     serverOperatingSystemVersion, serverArchitecture);
            }

            if(devices        == null ||
               devices.Length == 0)
            {
                AaruConsole.WriteLine("No known devices attached.");
            }
            else
            {
                devices = devices.OrderBy(d => d.Path).ToArray();

                if(aaruRemoteHost is null)
                {
                    AaruConsole.WriteLine("{0,-22}|{1,-16}|{2,-24}|{3,-24}|{4,-10}|{5,-10}", "Path", "Vendor", "Model",
                                          "Serial", "Bus", "Supported?");

                    AaruConsole.WriteLine("{0,-22}+{1,-16}+{2,-24}+{3,-24}+{4,-10}+{5,-10}", "----------------------",
                                          "----------------", "------------------------", "------------------------",
                                          "----------", "----------");

                    foreach(DeviceInfo dev in devices)
                        AaruConsole.WriteLine("{0,-22}|{1,-16}|{2,-24}|{3,-24}|{4,-10}|{5,-10}", dev.Path, dev.Vendor,
                                              dev.Model, dev.Serial, dev.Bus, dev.Supported);
                }
                else
                {
                    AaruConsole.WriteLine("{0,-48}|{1,-16}|{2,-24}|{3,-24}|{4,-10}|{5,-10}", "Path", "Vendor", "Model",
                                          "Serial", "Bus", "Supported?");

                    AaruConsole.WriteLine("{0,-48}+{1,-16}+{2,-24}+{3,-24}+{4,-10}+{5,-10}",
                                          "------------------------------------------------", "----------------",
                                          "------------------------", "------------------------", "----------",
                                          "----------");

                    foreach(DeviceInfo dev in devices)
                        AaruConsole.WriteLine("{0,-48}|{1,-16}|{2,-24}|{3,-24}|{4,-10}|{5,-10}", dev.Path, dev.Vendor,
                                              dev.Model, dev.Serial, dev.Bus, dev.Supported);
                }
            }

            return (int)ErrorNumber.NoError;
        }
    }
}