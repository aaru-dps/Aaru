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
            new AboutDialog().ShowDialog(this);
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
            DicConsole.WriteLine("Refreshing devices");
            devicesRoot.Children.Clear();

            foreach(DeviceInfo device in Device.ListDevices().Where(d => d.Supported).OrderBy(d => d.Vendor)
                                               .ThenBy(d => d.Model))
            {
                DicConsole.DebugWriteLine("Main window",
                                          "Found support device model {0} by manufacturer {1} on bus {2} and path {3}",
                                          device.Model, device.Vendor, device.Bus, device.Path);
                devicesRoot.Children.Add(new TreeGridItem
                {
                    Values = new object[] {$"{device.Vendor} {device.Model} ({device.Bus})", device.Path}
                });
            }

            treeImages.ReloadData();
        }

        protected void OnMenuConsole(object sender, EventArgs e)
        {
            new frmConsole().Show();
        }

        #region XAML IDs
        TreeGridItem devicesRoot;
        GridView     grdFiles;
        TreeGridItem imagesRoot;
        #endregion
    }
}