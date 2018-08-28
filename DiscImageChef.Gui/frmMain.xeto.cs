// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : frmMain.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Main window.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements main GUI window.
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
using System.ComponentModel;
using System.Linq;
using DiscImageChef.Console;
using DiscImageChef.Devices;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui
{
    public class frmMain : Form
    {
        bool                   closing;
        Splitter               splMain;
        TreeGridView           treeImages;
        TreeGridItemCollection treeImagesItems;

        public frmMain(bool debug, bool verbose)
        {
            XamlReader.Load(this);

            ConsoleHandler.Init();
            ConsoleHandler.Debug   = debug;
            ConsoleHandler.Verbose = verbose;

            treeImagesItems = new TreeGridItemCollection();

            treeImages.Columns.Add(new GridColumn {HeaderText = "Name", DataCell = new TextBoxCell(0)});

            treeImages.AllowMultipleSelection = false;
            treeImages.ShowHeader             = false;
            treeImages.DataStore              = treeImagesItems;

            imagesRoot  = new TreeGridItem {Values = new object[] {"Images"}};
            devicesRoot = new TreeGridItem {Values = new object[] {"Devices"}};

            treeImagesItems.Add(imagesRoot);
            treeImagesItems.Add(devicesRoot);

            Closing += OnClosing;
        }

        void OnClosing(object sender, CancelEventArgs e)
        {
            // This prevents an infinite loop of crashes :p
            if(closing) return;

            closing = true;
            Application.Instance.Quit();
        }

        protected void OnMenuOpen(object sender, EventArgs e)
        {
            MessageBox.Show("Not yet implemented");
        }

        protected void OnMenuAbout(object sender, EventArgs e)
        {
            AboutDialog dlgAbout = new AboutDialog
            {
                Developers = new[] {"Natalia Portillo", "Michael Drüing"},
                License = "This program is free software: you can redistribute it and/or modify\n" +
                          "it under the terms of the GNU General public License as\n"              +
                          "published by the Free Software Foundation, either version 3 of the\n"   +
                          "License, or (at your option) any later version.\n\n"                    +
                          "This program is distributed in the hope that it will be useful,\n"      +
                          "but WITHOUT ANY WARRANTY; without even the implied warranty of\n"       +
                          "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the\n"        +
                          "GNU General public License for more details.\n\n"                       +
                          "You should have received a copy of the GNU General public License\n"    +
                          "along with this program.  If not, see <http://www.gnu.org/licenses/>.",
                ProgramName  = "The Disc Image Chef",
                Website      = new Uri("https://github.com/claunia"),
                WebsiteLabel = "Source code on..."
            };
            dlgAbout.ShowDialog(this);
        }

        protected void OnMenuQuit(object sender, EventArgs e)
        {
            Application.Instance.Quit();
        }

        protected void OnDeviceRefresh(object sender, EventArgs e)
        {
            RefreshDevices();
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            RefreshDevices();
        }

        void RefreshDevices()
        {
            try
            {
                DicConsole.WriteLine("Refreshing devices");
                devicesRoot.Children.Clear();

                foreach(DeviceInfo device in Device.ListDevices().Where(d => d.Supported).OrderBy(d => d.Vendor)
                                                   .ThenBy(d => d.Model))
                {
                    DicConsole.DebugWriteLine("Main window",
                                              "Found supported device model {0} by manufacturer {1} on bus {2} and path {3}",
                                              device.Model, device.Vendor, device.Bus, device.Path);
                    devicesRoot.Children.Add(new TreeGridItem
                    {
                        Values = new object[] {$"{device.Vendor} {device.Model} ({device.Bus})", device.Path}
                    });
                }

                treeImages.ReloadData();
            }
            catch(InvalidOperationException ex) { DicConsole.ErrorWriteLine(ex.Message); }
        }

        protected void OnMenuConsole(object sender, EventArgs e)
        {
            new frmConsole().Show();
        }

        protected void OnMenuPlugins(object sender, EventArgs e)
        {
            new dlgPlugins().ShowModal(this);
        }

        #region XAML IDs
        TreeGridItem devicesRoot;
        GridView     grdFiles;
        TreeGridItem imagesRoot;
        #endregion
    }
}