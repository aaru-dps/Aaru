// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Main.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Main program loop.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains the main program loop.
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
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Aaru.Commands;
using Aaru.Commands.Device;
using Aaru.Commands.Filesystem;
using Aaru.Commands.Image;
using Aaru.Commands.Media;
using Aaru.Console;
using Aaru.Core;
using Aaru.Database;
using Aaru.Settings;
using Microsoft.EntityFrameworkCore;

namespace Aaru
{
    internal class MainClass
    {
        static string                                _assemblyCopyright;
        static string                                _assemblyTitle;
        static AssemblyInformationalVersionAttribute _assemblyVersion;

        [STAThread]
        public static int Main(string[] args)
        {
            object[] attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            _assemblyTitle = ((AssemblyTitleAttribute)attributes[0]).Title;
            attributes     = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

            _assemblyVersion =
                Attribute.GetCustomAttribute(typeof(MainClass).Assembly, typeof(AssemblyInformationalVersionAttribute))
                    as AssemblyInformationalVersionAttribute;

            _assemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

            DicConsole.WriteLineEvent      += System.Console.WriteLine;
            DicConsole.WriteEvent          += System.Console.Write;
            DicConsole.ErrorWriteLineEvent += System.Console.Error.WriteLine;

            Aaru.Settings.Settings.LoadSettings();

            var ctx = DicContext.Create(Aaru.Settings.Settings.LocalDbPath);
            ctx.Database.Migrate();
            ctx.SaveChanges();

            bool masterDbUpdate = false;

            if(!File.Exists(Aaru.Settings.Settings.MasterDbPath))
            {
                masterDbUpdate = true;
                UpdateCommand.DoUpdate(true);
            }

            var masterContext = DicContext.Create(Aaru.Settings.Settings.MasterDbPath);

            if(masterContext.Database.GetPendingMigrations().Any())
            {
                DicConsole.WriteLine("New database version, updating...");

                try
                {
                    File.Delete(Aaru.Settings.Settings.MasterDbPath);
                }
                catch(Exception)
                {
                    DicConsole.ErrorWriteLine("Exception trying to remove old database version, cannot continue...");
                    DicConsole.ErrorWriteLine("Please manually remove file at {0}", Aaru.Settings.Settings.MasterDbPath);
                }

                UpdateCommand.DoUpdate(true);
            }

            if((args.Length < 1 || args[0].ToLowerInvariant() != "gui") &&
               Aaru.Settings.Settings.Current.GdprCompliance < DicSettings.GdprLevel)
                new ConfigureCommand(true, true).Invoke(args);

            Statistics.LoadStats();

            if(Aaru.Settings.Settings.Current.Stats != null &&
               Aaru.Settings.Settings.Current.Stats.ShareStats)
                Task.Run(Statistics.SubmitStats);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Statistics.SaveStats();

            var rootCommand = new RootCommand
            {
                new Option(new[]
                {
                    "--verbose", "-v"
                }, "Shows verbose output.")
                {
                    Argument = new Argument<bool>(() => false)
                },
                new Option(new[]
                {
                    "--debug", "-d"
                }, "Shows debug output from plugins.")
                {
                    Argument = new Argument<bool>(() => false)
                }
            };

            rootCommand.Description =
                $"{_assemblyTitle} {_assemblyVersion?.InformationalVersion}\n{_assemblyCopyright}";

            rootCommand.AddCommand(new DatabaseFamily(masterDbUpdate));
            rootCommand.AddCommand(new DeviceFamily());
            rootCommand.AddCommand(new FilesystemFamily());
            rootCommand.AddCommand(new ImageFamily());
            rootCommand.AddCommand(new MediaFamily());

            rootCommand.AddCommand(new ConfigureCommand(false, false));
            rootCommand.AddCommand(new FormatsCommand());
            rootCommand.AddCommand(new ListEncodingsCommand());
            rootCommand.AddCommand(new ListNamespacesCommand());
            rootCommand.AddCommand(new RemoteCommand());

            return rootCommand.Invoke(args);
        }

        internal static void PrintCopyright()
        {
            DicConsole.WriteLine("{0} {1}", _assemblyTitle, _assemblyVersion?.InformationalVersion);
            DicConsole.WriteLine("{0}", _assemblyCopyright);
            DicConsole.WriteLine();
        }
    }
}