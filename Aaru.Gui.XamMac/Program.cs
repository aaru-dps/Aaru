// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Program.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Xamarin.macOS launcher.
//
// --[ Description ] ----------------------------------------------------------
//
//     Initializes Eto.Forms for Xamarin.macOS.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/
using System;
using System.Reflection;
using System.Threading.Tasks;
using DiscImageChef.Core;
using DiscImageChef.Database;
using DiscImageChef.Gui.Forms;
using Eto;
using Eto.Forms;
using Microsoft.EntityFrameworkCore;

namespace DiscImageChef.Gtk
{
    class Program
    {
        internal static bool Verbose;
        internal static bool Debug;
        internal static string AssemblyCopyright;
        internal static string AssemblyTitle;
        internal static AssemblyInformationalVersionAttribute AssemblyVersion;

        [STAThread]
        public static void Main(string[] args)
        {
            object[] attributes = typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            AssemblyTitle = ((AssemblyTitleAttribute)attributes[0]).Title;
            attributes = typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            AssemblyVersion =
                Attribute.GetCustomAttribute(typeof(Program).Assembly, typeof(AssemblyInformationalVersionAttribute)) as
                    AssemblyInformationalVersionAttribute;
            AssemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

            Settings.Settings.LoadSettings();

            DicContext ctx = DicContext.Create(Settings.Settings.LocalDbPath);
            ctx.Database.Migrate();
            ctx.SaveChanges();

            // TODO: Update database on GUI

            DicContext mctx = DicContext.Create(Settings.Settings.MasterDbPath);
            mctx.Database.Migrate();
            mctx.SaveChanges();

            Statistics.LoadStats();
            if (Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.ShareStats)
                Task.Run(() => { Statistics.SubmitStats(); });

            foreach (string arg in args)
                switch (arg.ToLowerInvariant())
                {
                    case "-v":
                    case "--verbose":
                        Verbose = true;
                        break;
                    case "-d":
                    case "--debug":
                        Debug = true;
                        break;
                }

            new Application(Platforms.XamMac2).Run(new frmMain(Debug, Verbose));

            Statistics.SaveStats();
        }
    }
}