// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.ComponentModel;
using System.IO;
using Aaru.Console;
using Aaru.Core;
using Aaru.Core.Media.Info;
using Aaru.Devices;
using Aaru.Gui.Panels;
using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;
using DeviceInfo = Aaru.Core.Devices.Info.DeviceInfo;

namespace Aaru.Gui.Forms
{
    public class frmMain : Form
    {
        readonly Bitmap devicesIcon;
        readonly Bitmap ejectIcon;
        readonly Bitmap hardDiskIcon;
        readonly Bitmap imagesIcon;
        readonly Label  lblError;
        /// <summary>This is to remember that column is an image to be set in future</summary>
        readonly Image nullImage;
        readonly Bitmap                 opticalIcon;
        readonly TreeGridItem           placeholderItem;
        readonly Bitmap                 removableIcon;
        readonly Bitmap                 sdIcon;
        readonly Bitmap                 tapeIcon;
        readonly TreeGridItemCollection treeImagesItems;
        readonly ContextMenu            treeImagesMenu;
        readonly Bitmap                 usbIcon;
        bool                            closing;
        GridView                        grdFiles;
        TreeGridView                    treeImages;

        public frmMain(bool debug, bool verbose)
        {
            XamlReader.Load(this);

            lblError  = new Label();
            grdFiles  = new GridView();
            nullImage = null;

            ConsoleHandler.Init();
            ConsoleHandler.Debug   = debug;
            ConsoleHandler.Verbose = verbose;

            treeImagesItems = new TreeGridItemCollection();

            treeImages.Columns.Add(new GridColumn
            {
                HeaderText = "Name", DataCell = new ImageTextCell(0, 1)
            });

            treeImages.AllowMultipleSelection = false;
            treeImages.ShowHeader             = false;
            treeImages.DataStore              = treeImagesItems;

            // TODO: SVG
            imagesIcon =
                new Bitmap(ResourceHandler.
                               GetResourceStream("Aaru.Gui.Assets.Icons.oxygen._32x32.inode-directory.png"));

            devicesIcon =
                new Bitmap(ResourceHandler.GetResourceStream("Aaru.Gui.Assets.Icons.oxygen._32x32.computer.png"));

            hardDiskIcon =
                new Bitmap(ResourceHandler.GetResourceStream("Aaru.Gui.Assets.Icons.oxygen._32x32.drive-harddisk.png"));

            opticalIcon =
                new Bitmap(ResourceHandler.GetResourceStream("Aaru.Gui.Assets.Icons.oxygen._32x32.drive-optical.png"));

            usbIcon =
                new Bitmap(ResourceHandler.
                               GetResourceStream("Aaru.Gui.Assets.Icons.oxygen._32x32.drive-removable-media-usb.png"));

            removableIcon =
                new Bitmap(ResourceHandler.
                               GetResourceStream("Aaru.Gui.Assets.Icons.oxygen._32x32.drive-removable-media.png"));

            sdIcon =
                new Bitmap(ResourceHandler.
                               GetResourceStream("Aaru.Gui.Assets.Icons.oxygen._32x32.media-flash-sd-mmc.png"));

            tapeIcon =
                new Bitmap(ResourceHandler.GetResourceStream("Aaru.Gui.Assets.Icons.oxygen._32x32.media-tape.png"));

            ejectIcon =
                new Bitmap(ResourceHandler.GetResourceStream("Aaru.Gui.Assets.Icons.oxygen._32x32.media-eject.png"));

            imagesRoot = new TreeGridItem
            {
                Values = new object[]
                {
                    imagesIcon, "Images"
                }
            };

            devicesRoot = new TreeGridItem
            {
                Values = new object[]
                {
                    devicesIcon, "Devices"
                }
            };

            treeImagesItems.Add(imagesRoot);
            treeImagesItems.Add(devicesRoot);

            placeholderItem = new TreeGridItem
            {
                Values = new object[]
                {
                    nullImage, "You should not be seeing this"
                }
            };

            Closing += OnClosing;

            treeImagesMenu         =  new ContextMenu();
            treeImagesMenu.Opening += OnTreeImagesMenuOpening;
            treeImages.ContextMenu =  treeImagesMenu;
        }

        void OnTreeImagesMenuOpening(object sender, EventArgs e)
        {
            OnTreeImagesSelectedItemChanged(treeImages, e);

            treeImagesMenu.Items.Clear();

            var menuItem = new ButtonMenuItem
            {
                Text = "Close all images"
            };

            menuItem.Click += CloseAllImages;
            treeImagesMenu.Items.Add(menuItem);

            menuItem = new ButtonMenuItem
            {
                Text = "Refresh devices"
            };

            //            menuItem.Click += OnDeviceRefresh;
            treeImagesMenu.Items.Add(menuItem);

            if(!(treeImages.SelectedItem is TreeGridItem selectedItem))
                return;

            if(selectedItem.Values.Length < 4)
                return;
        }

        // TODO
        void CloseAllImages(object sender, EventArgs eventArgs) => Eto.Forms.MessageBox.Show("Not yet implemented");

        void OnClosing(object sender, CancelEventArgs e)
        {
            // This prevents an infinite loop of crashes :p
            if(closing)
                return;

            closing = true;
            Application.Instance.Quit();
        }

        protected void OnTreeImagesSelectedItemChanged(object sender, EventArgs e)
        {
            if(!(sender is TreeGridView tree))
                return;

            if(!(tree.SelectedItem is TreeGridItem selectedItem))
                return;

            splMain.Panel2 = null;

            if(selectedItem.Values.Length >= 4 &&
               selectedItem.Values[3] is Panel infoPanel)
            {
                splMain.Panel2 = infoPanel;

                return;
            }

            if(selectedItem.Values.Length < 4)
                return;

            switch(selectedItem.Values[3])
            {
                case null when selectedItem.Parent == devicesRoot:
                    try
                    {
                        var dev = new Device((string)selectedItem.Values[2]);

                        if(dev.IsRemote)
                            Statistics.AddRemote(dev.RemoteApplication, dev.RemoteVersion, dev.RemoteOperatingSystem,
                                                 dev.RemoteOperatingSystemVersion, dev.RemoteArchitecture);

                        if(dev.Error)
                        {
                            selectedItem.Values[3] = $"Error {dev.LastError} opening device";

                            return;
                        }

                        var devInfo = new DeviceInfo(dev);

                        selectedItem.Values[3] = new pnlDeviceInfo(devInfo);
                        splMain.Panel2         = (Panel)selectedItem.Values[3];

                        dev.Close();
                    }
                    catch(SystemException ex)
                    {
                        selectedItem.Values[3] = ex.Message;
                        lblError.Text          = ex.Message;
                        splMain.Panel2         = lblError;
                        AaruConsole.ErrorWriteLine(ex.Message);
                    }

                    break;
                case string devErrorMessage when selectedItem.Parent == devicesRoot:
                    lblError.Text  = devErrorMessage;
                    splMain.Panel2 = lblError;

                    break;
            }
        }

        protected void OnTreeImagesItemExpanding(object sender, TreeGridViewItemCancelEventArgs e)
        {
            // First expansion of a device
            if((e.Item as TreeGridItem)?.Children?.Count != 1 ||
               ((TreeGridItem)e.Item).Children[0]        != placeholderItem)
                return;

            if(((TreeGridItem)e.Item).Parent == devicesRoot)
            {
                var deviceItem = (TreeGridItem)e.Item;

                deviceItem.Children.Clear();
                Device dev;

                try
                {
                    dev = new Device((string)deviceItem.Values[2]);

                    if(dev.IsRemote)
                        Statistics.AddRemote(dev.RemoteApplication, dev.RemoteVersion, dev.RemoteOperatingSystem,
                                             dev.RemoteOperatingSystemVersion, dev.RemoteArchitecture);

                    if(dev.Error)
                    {
                        deviceItem.Values[3] = $"Error {dev.LastError} opening device";
                        e.Cancel             = true;
                        treeImages.ReloadData();
                        treeImages.SelectedItem = deviceItem;

                        return;
                    }
                }
                catch(SystemException ex)
                {
                    deviceItem.Values[3] = ex.Message;
                    e.Cancel             = true;
                    treeImages.ReloadData();
                    AaruConsole.ErrorWriteLine(ex.Message);
                    treeImages.SelectedItem = deviceItem;

                    return;
                }

                if(!dev.IsRemovable)
                    deviceItem.Children.Add(new TreeGridItem
                    {
                        Values = new object[]
                        {
                            nullImage, "Non-removable device commands not yet implemented"
                        }
                    });
                else
                {
                    // TODO: Removable non-SCSI?
                    var scsiInfo = new ScsiInfo(dev);

                    if(!scsiInfo.MediaInserted)
                        deviceItem.Children.Add(new TreeGridItem
                        {
                            Values = new object[]
                            {
                                ejectIcon, "No media inserted"
                            }
                        });
                    else
                    {
                        // TODO: SVG
                        Stream logo =
                            ResourceHandler.GetResourceStream($"Aaru.Gui.Assets.Logos.Media.{scsiInfo.MediaType}.png");

                        deviceItem.Children.Add(new TreeGridItem
                        {
                            Values = new[]
                            {
                                logo == null ? null : new Bitmap(logo), scsiInfo.MediaType, deviceItem.Values[2],
                                new pnlScsiInfo(scsiInfo, (string)deviceItem.Values[2])
                            }
                        });
                    }
                }

                dev.Close();
            }
        }

        #region XAML IDs
        readonly TreeGridItem devicesRoot;
        readonly TreeGridItem imagesRoot;
        Splitter              splMain;
        #endregion
    }
}