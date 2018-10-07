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
using System.IO;
using System.Linq;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Core.Media.Info;
using DiscImageChef.Devices;
using DiscImageChef.Gui.Dialogs;
using DiscImageChef.Gui.Panels;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Forms
{
    public class frmMain : Form
    {
        bool                   closing;
        GridView               grdFiles;
        Label                  lblError;
        TreeGridItem           placeholderItem;
        TreeGridView           treeImages;
        TreeGridItemCollection treeImagesItems;

        public frmMain(bool debug, bool verbose)
        {
            XamlReader.Load(this);

            lblError = new Label();
            grdFiles = new GridView();

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

            placeholderItem = new TreeGridItem {Values = new object[] {"You should not be seeing this"}};

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
            // TODO: Extensions
            OpenFileDialog dlgOpenImage = new OpenFileDialog {Title = "Choose image to open"};

            DialogResult result = dlgOpenImage.ShowDialog(this);
            if(result != DialogResult.Ok) return;

            FiltersList filtersList = new FiltersList();
            IFilter     inputFilter = filtersList.GetFilter(dlgOpenImage.FileName);

            if(inputFilter == null)
            {
                MessageBox.Show("Cannot open specified file.", MessageBoxType.Error);
                return;
            }

            try
            {
                IMediaImage imageFormat = ImageFormat.Detect(inputFilter);

                if(imageFormat == null)
                {
                    MessageBox.Show("Image format not identified.", MessageBoxType.Error);
                    return;
                }

                DicConsole.WriteLine("Image format identified by {0} ({1}).", imageFormat.Name, imageFormat.Id);

                try
                {
                    if(!imageFormat.Open(inputFilter))
                    {
                        MessageBox.Show("Unable to open image format", MessageBoxType.Error);
                        DicConsole.ErrorWriteLine("Unable to open image format");
                        DicConsole.ErrorWriteLine("No error given");
                        return;
                    }

                    imagesRoot.Children.Add(new TreeGridItem
                    {
                        Values = new object[]
                        {
                            $"{Path.GetFileName(dlgOpenImage.FileName)} ({imageFormat.Info.MediaType})",
                            dlgOpenImage.FileName,
                            new pnlImageInfo(dlgOpenImage.FileName, inputFilter,
                                             imageFormat),
                            inputFilter, imageFormat
                        }
                    });

                    treeImages.ReloadData();

                    Statistics.AddMediaFormat(imageFormat.Format);
                    Statistics.AddMedia(imageFormat.Info.MediaType, false);
                    Statistics.AddFilter(inputFilter.Name);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Unable to open image format", MessageBoxType.Error);
                    DicConsole.ErrorWriteLine("Unable to open image format");
                    DicConsole.ErrorWriteLine("Error: {0}", ex.Message);
                    DicConsole.DebugWriteLine("Image-info command", "Stack trace: {0}", ex.StackTrace);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Exception reading file", MessageBoxType.Error);
                DicConsole.ErrorWriteLine($"Error reading file: {ex.Message}");
                DicConsole.DebugWriteLine("Image-info command", ex.StackTrace);
            }

            Statistics.AddCommand("image-info");
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

                    TreeGridItem devItem = new TreeGridItem
                    {
                        Values = new object[] {$"{device.Vendor} {device.Model} ({device.Bus})", device.Path, null}
                    };
                    devItem.Children.Add(placeholderItem);
                    devicesRoot.Children.Add(devItem);
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

        protected void OnMenuEncodings(object sender, EventArgs e)
        {
            new dlgEncodings().ShowModal(this);
        }

        protected void OnTreeImagesSelectedItemChanged(object sender, EventArgs e)
        {
            if(!(sender is TreeGridView tree)) return;

            if(!(tree.SelectedItem is TreeGridItem selectedItem)) return;

            splMain.Panel2 = null;

            if(selectedItem.Values.Length >= 3 && selectedItem.Values[2] is Panel infoPanel)
            {
                splMain.Panel2 = infoPanel;
                return;
            }

            if(selectedItem.Parent != devicesRoot) return;

            switch(selectedItem.Values[2])
            {
                case null:
                    try
                    {
                        Device dev = new Device((string)selectedItem.Values[1]);
                        if(dev.Error)
                        {
                            selectedItem.Values[2] = $"Error {dev.LastError} opening device";
                            return;
                        }

                        Core.Devices.Info.DeviceInfo devInfo = new Core.Devices.Info.DeviceInfo(dev);

                        selectedItem.Values[2] = new pnlDeviceInfo(devInfo);
                        splMain.Panel2         = (Panel)selectedItem.Values[2];

                        dev.Close();
                    }
                    catch(SystemException ex)
                    {
                        selectedItem.Values[2] = ex.Message;
                        lblError.Text          = ex.Message;
                        splMain.Panel2         = lblError;
                        DicConsole.ErrorWriteLine(ex.Message);
                    }

                    break;
                case string devErrorMessage:
                    lblError.Text  = devErrorMessage;
                    splMain.Panel2 = lblError;
                    break;
            }
        }

        protected void OnTreeImagesItemExpanding(object sender, TreeGridViewItemCancelEventArgs e)
        {
            // First expansion of a device
            if((e.Item as TreeGridItem)?.Children?.Count == 1               &&
               ((TreeGridItem)e.Item).Children[0]        == placeholderItem &&
               ((TreeGridItem)e.Item).Parent             == devicesRoot)
            {
                TreeGridItem deviceItem = (TreeGridItem)e.Item;

                deviceItem.Children.Clear();
                Device dev;
                try
                {
                    dev = new Device((string)deviceItem.Values[1]);
                    if(dev.Error)
                    {
                        deviceItem.Values[2] = $"Error {dev.LastError} opening device";
                        e.Cancel             = true;
                        treeImages.ReloadData();
                        treeImages.SelectedItem = deviceItem;
                        return;
                    }
                }
                catch(SystemException ex)
                {
                    deviceItem.Values[2] = ex.Message;
                    e.Cancel             = true;
                    treeImages.ReloadData();
                    DicConsole.ErrorWriteLine(ex.Message);
                    treeImages.SelectedItem = deviceItem;
                    return;
                }

                if(!dev.IsRemovable)
                    deviceItem.Children.Add(new TreeGridItem
                    {
                        Values = new object[]
                        {
                            "Non-removable device commands not yet implemented"
                        }
                    });
                else
                {
                    // TODO: Removable non-SCSI?
                    ScsiInfo scsiInfo = new ScsiInfo(dev);

                    if(!scsiInfo.MediaInserted)
                        deviceItem.Children.Add(new TreeGridItem {Values = new object[] {"No media inserted"}});
                    else
                        deviceItem.Children.Add(new TreeGridItem
                        {
                            Values = new[]
                            {
                                scsiInfo.MediaType, deviceItem.Values[1],
                                new pnlScsiInfo(scsiInfo, (string)deviceItem.Values[1])
                            }
                        });
                }

                dev.Close();
            }
        }

        #region XAML IDs
        TreeGridItem devicesRoot;
        TreeGridItem imagesRoot;
        Splitter     splMain;
        #endregion
    }
}