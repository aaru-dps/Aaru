using System;
using System.Linq;
using DiscImageChef.Devices;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui
{
    public class frmMain : Form
    {
        Splitter               splMain;
        TreeGridView           treeImages;
        TreeGridItemCollection treeImagesItems;

        public frmMain()
        {
            XamlReader.Load(this);

            treeImagesItems = new TreeGridItemCollection();

            treeImages.Columns.Add(new GridColumn {HeaderText = "Name", DataCell = new TextBoxCell(0)});

            treeImages.AllowMultipleSelection = false;
            treeImages.ShowHeader             = false;
            treeImages.DataStore              = treeImagesItems;

            imagesRoot = new TreeGridItem {Values = new object[] {"Images"}};
            devicesRoot = new TreeGridItem {Values = new object[] {"Devices"}};

            treeImagesItems.Add(imagesRoot);
            treeImagesItems.Add(devicesRoot);
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
            devicesRoot.Children.Clear();

            foreach(DeviceInfo device in Device.ListDevices().Where(d => d.Supported).OrderBy(d => d.Vendor)
                                               .ThenBy(d => d.Model))
            {
                devicesRoot.Children.Add(new TreeGridItem
                {
                    Values = new object[] {$"{device.Vendor} {device.Model} ({device.Bus})", device.Path}
                });
            }

            treeImages.ReloadData();
        }

        #region XAML IDs
        TreeGridItem devicesRoot;
        GridView     grdFiles;
        TreeGridItem imagesRoot;
        #endregion
    }
}