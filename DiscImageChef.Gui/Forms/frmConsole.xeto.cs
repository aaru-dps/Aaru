// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : frmConsole.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Console window.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the console window and saving or clearing console log.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes.Interop;
using DiscImageChef.Console;
using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;
using PlatformID = DiscImageChef.CommonTypes.Interop.PlatformID;
using Version = DiscImageChef.CommonTypes.Interop.Version;

namespace DiscImageChef.Gui.Forms
{
    public class frmConsole : Form
    {
        public frmConsole()
        {
            XamlReader.Load(this);

            grdMessages.DataStore = ConsoleHandler.Entries;
            grdMessages.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<LogEntry, string>(r => $"{r.Timestamp}")
                },
                HeaderText = "Time",
                Sortable   = true
            });
            grdMessages.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<LogEntry, string>(r => r.Type)
                },
                HeaderText = "Type",
                Sortable   = true
            });
            grdMessages.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<LogEntry, string>(r => r.Module)
                },
                HeaderText = "Module",
                Sortable   = true
            });
            grdMessages.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<LogEntry, string>(r => r.Message)
                },
                HeaderText = "Message",
                Sortable   = true
            });

            grdMessages.AllowMultipleSelection = false;
            grdMessages.CellFormatting += (sender, e) =>
            {
                if(((LogEntry)e.Item).Type.ToLower() != "error") return;

                e.BackgroundColor = Colors.Red;
                e.ForegroundColor = Colors.Black;
            };
            grdMessages.AllowColumnReordering = true;

            chkDebug.Checked =  ConsoleHandler.Debug;
            Closing          += OnClosing;
        }

        void OnClosing(object sender, CancelEventArgs e)
        {
            // Otherwise if this closes it does not stop hearing events from collection, preventing console to keep working.
            grdMessages.DataStore = null;
        }

        protected void OnChkDebugChecked(object sender, EventArgs e)
        {
            ConsoleHandler.Debug = chkDebug.Checked.Value;
        }

        protected void OnBtnClearClicked(object sender, EventArgs e)
        {
            ConsoleHandler.Entries.Clear();
        }

        protected void OnBtnSaveClicked(object sender, EventArgs e)
        {
            SaveFileDialog dlgSave = new SaveFileDialog {CheckFileExists = true};
            dlgSave.Filters.Add(new FileFilter {Extensions               = new[] {"log"}, Name = "Log files"});
            DialogResult result = dlgSave.ShowDialog(this);
            if(result != DialogResult.Ok) return;

            try
            {
                FileStream   logFs = new FileStream(dlgSave.FileName, FileMode.Create, FileAccess.ReadWrite);
                StreamWriter logSw = new StreamWriter(logFs);

                logSw.WriteLine("Log saved at {0}", DateTime.Now);

                PlatformID platId  = DetectOS.GetRealPlatformID();
                string     platVer = DetectOS.GetVersion();
                AssemblyInformationalVersionAttribute assemblyVersion =
                    Attribute.GetCustomAttribute(typeof(DicConsole).Assembly,
                                                 typeof(AssemblyInformationalVersionAttribute)) as
                        AssemblyInformationalVersionAttribute;

                logSw.WriteLine("################# System information #################");
                logSw.WriteLine("{0} {1} ({2}-bit)", DetectOS.GetPlatformName(platId, platVer), platVer,
                                Environment.Is64BitOperatingSystem ? 64 : 32);
                if(DetectOS.IsMono) logSw.WriteLine("Mono {0}",              Version.GetMonoVersion());
                else if(DetectOS.IsNetCore) logSw.WriteLine(".NET Core {0}", Version.GetNetCoreVersion());
                else logSw.WriteLine(RuntimeInformation.FrameworkDescription);

                logSw.WriteLine();

                logSw.WriteLine("################# Program information ################");
                logSw.WriteLine("DiscImageChef {0}",          assemblyVersion?.InformationalVersion);
                logSw.WriteLine("Running in {0}-bit",         Environment.Is64BitProcess ? 64 : 32);
                logSw.WriteLine("Running GUI mode using {0}", Application.Instance.Platform.ID);
                #if DEBUG
                logSw.WriteLine("DEBUG version");
                #endif
                logSw.WriteLine("Command line: {0}", Environment.CommandLine);
                logSw.WriteLine();

                logSw.WriteLine("################# Console ################");
                foreach(LogEntry entry in ConsoleHandler.Entries)
                    if(entry.Type != "Info")
                        logSw.WriteLine("{0}: ({1}) {2}", entry.Timestamp, entry.Type.ToLower(), entry.Message);
                    else logSw.WriteLine("{0}: {1}",      entry.Timestamp, entry.Message);

                logSw.Close();
                logFs.Close();
            }
            catch(Exception exception)
            {
                MessageBox.Show("Exception {0} trying to save logfile, details has been sent to console.",
                                exception.Message);
                DicConsole.ErrorWriteLine("Console", exception.Message);
                DicConsole.ErrorWriteLine("Console", exception.StackTrace);
            }
        }

        #region XAML controls
        GridView grdMessages;
        CheckBox chkDebug;
        Button   btnClear;
        Button   btnSave;
        #endregion
    }
}