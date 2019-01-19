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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DiscImageChef.Commands;
using DiscImageChef.CommonTypes.Interop;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Database;
using DiscImageChef.Settings;
using Microsoft.EntityFrameworkCore;
using Mono.Options;
using PlatformID = DiscImageChef.CommonTypes.Interop.PlatformID;

namespace DiscImageChef
{
    class MainClass
    {
        internal static bool                                  Verbose;
        internal static bool                                  Debug;
        internal static string                                AssemblyCopyright;
        internal static string                                AssemblyTitle;
        internal static AssemblyInformationalVersionAttribute AssemblyVersion;

        [STAThread]
        public static int Main(string[] args)
        {
            object[] attributes = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            AssemblyTitle = ((AssemblyTitleAttribute)attributes[0]).Title;
            attributes    = typeof(MainClass).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            AssemblyVersion =
                Attribute.GetCustomAttribute(typeof(MainClass).Assembly, typeof(AssemblyInformationalVersionAttribute))
                    as AssemblyInformationalVersionAttribute;
            AssemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

            DicConsole.WriteLineEvent      += System.Console.WriteLine;
            DicConsole.WriteEvent          += System.Console.Write;
            DicConsole.ErrorWriteLineEvent += System.Console.Error.WriteLine;

            Settings.Settings.LoadSettings();

            DicContext ctx = DicContext.Create(Settings.Settings.LocalDbPath);
            ctx.Database.Migrate();
            ctx.SaveChanges();

            bool masterDbUpdate = false;
            if(!File.Exists(Settings.Settings.MasterDbPath))
            {
                masterDbUpdate = true;
                UpdateCommand.DoUpdate(true);
            }

            DicContext mctx = DicContext.Create(Settings.Settings.MasterDbPath);
            mctx.Database.Migrate();
            mctx.SaveChanges();

            if((args.Length < 1 || args[0].ToLowerInvariant() != "gui") &&
               Settings.Settings.Current.GdprCompliance < DicSettings.GdprLevel)
                new ConfigureCommand(true, true).Invoke(args);
            Statistics.LoadStats();
            if(Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.ShareStats)
                Task.Run(() => { Statistics.SubmitStats(); });

            PlatformID currentPlatform = DetectOS.GetRealPlatformID();

            CommandSet commands = new CommandSet("DiscImageChef")
            {
                $"{AssemblyTitle} {AssemblyVersion?.InformationalVersion}",
                $"{AssemblyCopyright}",
                "",
                "usage: DiscImageChef COMMAND [OPTIONS]",
                "",
                "Global options:",
                {"verbose|v", "Shows verbose output.", b => Verbose = b        != null},
                {"debug|d", "Shows debug output from plugins.", b => Debug = b != null},
                "",
                "Available commands:",
                new AnalyzeCommand(),
                new BenchmarkCommand(),
                new ChecksumCommand(),
                new CompareCommand(),
                new ConfigureCommand(false, false),
                new ConvertImageCommand(),
                new CreateSidecarCommand(),
                new DecodeCommand()
            };

            if(currentPlatform == PlatformID.FreeBSD || currentPlatform == PlatformID.Linux ||
               currentPlatform == PlatformID.Win32NT)
            {
                commands.Add(new DeviceInfoCommand());
                commands.Add(new DeviceReportCommand());
                commands.Add(new DumpMediaCommand());
            }

            commands.Add(new EntropyCommand());
            commands.Add(new ExtractFilesCommand());
            commands.Add(new FormatsCommand());
            commands.Add(new GuiCommand());
            commands.Add(new ImageInfoCommand());

            if(currentPlatform == PlatformID.FreeBSD || currentPlatform == PlatformID.Linux ||
               currentPlatform == PlatformID.Win32NT) commands.Add(new ListDevicesCommand());

            commands.Add(new ListEncodingsCommand());
            commands.Add(new ListOptionsCommand());
            commands.Add(new LsCommand());

            if(currentPlatform == PlatformID.FreeBSD || currentPlatform == PlatformID.Linux ||
               currentPlatform == PlatformID.Win32NT)
            {
                commands.Add(new MediaInfoCommand());
                commands.Add(new MediaScanCommand());
            }

            commands.Add(new PrintHexCommand());
            commands.Add(new StatisticsCommand());
            commands.Add(new UpdateCommand(masterDbUpdate));
            commands.Add(new VerifyCommand());

            int ret = commands.Run(args);

            Statistics.SaveStats();

            return ret;
        }

        internal static void PrintCopyright()
        {
            DicConsole.WriteLine("{0} {1}", AssemblyTitle, AssemblyVersion?.InformationalVersion);
            DicConsole.WriteLine("{0}",     AssemblyCopyright);
            DicConsole.WriteLine();
        }
    }
}