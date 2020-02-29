using System;
using System.Reflection;
using System.Threading.Tasks;
using Aaru.Core;
using Aaru.Database;
using Aaru.Gui.Forms;
using Eto;
using Eto.Forms;
using Microsoft.EntityFrameworkCore;

namespace Aaru.Wpf
{
    internal class Program
    {
        internal static bool                                  Verbose;
        internal static bool                                  Debug;
        internal static string                                AssemblyCopyright;
        internal static string                                AssemblyTitle;
        internal static AssemblyInformationalVersionAttribute AssemblyVersion;

        [STAThread]
        public static void Main(string[] args)
        {
            object[] attributes = typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            AssemblyTitle = ((AssemblyTitleAttribute)attributes[0]).Title;
            attributes    = typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

            AssemblyVersion =
                Attribute.GetCustomAttribute(typeof(Program).Assembly, typeof(AssemblyInformationalVersionAttribute)) as
                    AssemblyInformationalVersionAttribute;

            AssemblyCopyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

            Settings.Settings.LoadSettings();

            var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);
            ctx.Database.Migrate();
            ctx.SaveChanges();

            // TODO: Update database on GUI

            var mctx = AaruContext.Create(Settings.Settings.MasterDbPath);
            mctx.Database.Migrate();
            mctx.SaveChanges();

            Statistics.LoadStats();

            if(Settings.Settings.Current.Stats != null &&
               Settings.Settings.Current.Stats.ShareStats)
                Task.Run(Statistics.SubmitStats);

            foreach(string arg in args)
                switch(arg.ToLowerInvariant())
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

            new Application(Platforms.Wpf).Run(new frmMain(Debug, Verbose));

            Statistics.SaveStats();
        }
    }
}