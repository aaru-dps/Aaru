// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : dlgPlugins.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Plugins dialog.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the plugins dialog.
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
using System.Collections.ObjectModel;
using System.Reflection;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Core;
using DiscImageChef.Partitions;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui
{
    public class dlgPlugins : Dialog
    {
        ObservableCollection<PluginEntry> filesystems;
        ObservableCollection<PluginEntry> filters;
        ObservableCollection<PluginEntry> floppyImages;
        ObservableCollection<PluginEntry> mediaImages;
        ObservableCollection<PluginEntry> partitions;
        ObservableCollection<PluginEntry> readOnlyFilesystems;
        ObservableCollection<PluginEntry> writableFloppyImages;
        ObservableCollection<PluginEntry> writableImages;

        public dlgPlugins()
        {
            XamlReader.Load(this);

            DefaultButton = btnClose;
            DisplayMode   = DialogDisplayMode.Attached;

            filters              = new ObservableCollection<PluginEntry>();
            floppyImages         = new ObservableCollection<PluginEntry>();
            mediaImages          = new ObservableCollection<PluginEntry>();
            partitions           = new ObservableCollection<PluginEntry>();
            filesystems          = new ObservableCollection<PluginEntry>();
            readOnlyFilesystems  = new ObservableCollection<PluginEntry>();
            writableFloppyImages = new ObservableCollection<PluginEntry>();
            writableImages       = new ObservableCollection<PluginEntry>();

            grdFilters.DataStore = filters;
            grdFilters.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Name)},
                HeaderText = "Name",
                Sortable   = true
            });
            grdFilters.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => $"{r.Uuid}")},
                HeaderText = "UUID",
                Sortable   = true
            });
            grdFilters.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Version)},
                HeaderText = "Version",
                Sortable   = true
            });
            grdFilters.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Author)},
                HeaderText = "Author",
                Sortable   = true
            });
            grdFilters.AllowMultipleSelection = false;
            grdFilters.AllowColumnReordering  = true;

            grdReadableMediaImages.DataStore = mediaImages;
            grdReadableMediaImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Name)},
                HeaderText = "Name",
                Sortable   = true
            });
            grdReadableMediaImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => $"{r.Uuid}")},
                HeaderText = "UUID",
                Sortable   = true
            });
            grdReadableMediaImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Version)},
                HeaderText = "Version",
                Sortable   = true
            });
            grdReadableMediaImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Author)},
                HeaderText = "Author",
                Sortable   = true
            });
            grdReadableMediaImages.AllowMultipleSelection = false;
            grdReadableMediaImages.AllowColumnReordering  = true;

            grdPartitions.DataStore = partitions;
            grdPartitions.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Name)},
                HeaderText = "Name",
                Sortable   = true
            });
            grdPartitions.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => $"{r.Uuid}")},
                HeaderText = "UUID",
                Sortable   = true
            });
            grdPartitions.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Version)},
                HeaderText = "Version",
                Sortable   = true
            });
            grdPartitions.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Author)},
                HeaderText = "Author",
                Sortable   = true
            });
            grdPartitions.AllowMultipleSelection = false;
            grdPartitions.AllowColumnReordering  = true;

            grdFilesystem.DataStore = filesystems;
            grdFilesystem.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Name)},
                HeaderText = "Name",
                Sortable   = true
            });
            grdFilesystem.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => $"{r.Uuid}")},
                HeaderText = "UUID",
                Sortable   = true
            });
            grdFilesystem.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Version)},
                HeaderText = "Version",
                Sortable   = true
            });
            grdFilesystem.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Author)},
                HeaderText = "Author",
                Sortable   = true
            });
            grdFilesystem.AllowMultipleSelection = false;
            grdFilesystem.AllowColumnReordering  = true;

            grdReadOnlyFilesystem.DataStore = readOnlyFilesystems;
            grdReadOnlyFilesystem.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Name)},
                HeaderText = "Name",
                Sortable   = true
            });
            grdReadOnlyFilesystem.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => $"{r.Uuid}")},
                HeaderText = "UUID",
                Sortable   = true
            });
            grdReadOnlyFilesystem.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Version)},
                HeaderText = "Version",
                Sortable   = true
            });
            grdReadOnlyFilesystem.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Author)},
                HeaderText = "Author",
                Sortable   = true
            });
            grdReadOnlyFilesystem.AllowMultipleSelection = false;
            grdReadOnlyFilesystem.AllowColumnReordering  = true;

            grdReadableFloppyImages.DataStore = floppyImages;
            grdReadableFloppyImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Name)},
                HeaderText = "Name",
                Sortable   = true
            });
            grdReadableFloppyImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => $"{r.Uuid}")},
                HeaderText = "UUID",
                Sortable   = true
            });
            grdReadableFloppyImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Version)},
                HeaderText = "Version",
                Sortable   = true
            });
            grdReadableFloppyImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Author)},
                HeaderText = "Author",
                Sortable   = true
            });
            grdReadableFloppyImages.AllowMultipleSelection = false;
            grdReadableFloppyImages.AllowColumnReordering  = true;

            grdWritableFloppyImages.DataStore = writableFloppyImages;
            grdWritableFloppyImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Name)},
                HeaderText = "Name",
                Sortable   = true
            });
            grdWritableFloppyImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => $"{r.Uuid}")},
                HeaderText = "UUID",
                Sortable   = true
            });
            grdWritableFloppyImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Version)},
                HeaderText = "Version",
                Sortable   = true
            });
            grdWritableFloppyImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Author)},
                HeaderText = "Author",
                Sortable   = true
            });
            grdWritableFloppyImages.AllowMultipleSelection = false;
            grdWritableFloppyImages.AllowColumnReordering  = true;

            grdWritableMediaImages.DataStore = writableImages;
            grdWritableMediaImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Name)},
                HeaderText = "Name",
                Sortable   = true
            });
            grdWritableMediaImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => $"{r.Uuid}")},
                HeaderText = "UUID",
                Sortable   = true
            });
            grdWritableMediaImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Version)},
                HeaderText = "Version",
                Sortable   = true
            });
            grdWritableMediaImages.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PluginEntry, string>(r => r.Author)},
                HeaderText = "Author",
                Sortable   = true
            });
            grdWritableMediaImages.AllowMultipleSelection = false;
            grdWritableMediaImages.AllowColumnReordering  = true;
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            filters.Clear();
            floppyImages.Clear();
            mediaImages.Clear();
            partitions.Clear();
            filesystems.Clear();
            readOnlyFilesystems.Clear();
            writableFloppyImages.Clear();
            writableImages.Clear();

            foreach(IFilter filter in GetPluginBase.Instance.Filters.Values)
                filters.Add(new PluginEntry
                {
                    Name    = filter.Name,
                    Uuid    = filter.Id,
                    Version = Assembly.GetAssembly(filter.GetType())?.GetName().Version.ToString(),
                    Author =
                        ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly.GetAssembly(filter.GetType()),
                                                                                typeof(AssemblyCompanyAttribute),
                                                                                false)).Company
                });

            foreach(IFloppyImage floppyImage in GetPluginBase.Instance.FloppyImages.Values)
                floppyImages.Add(new PluginEntry
                {
                    Name    = floppyImage.Name,
                    Uuid    = floppyImage.Id,
                    Version = Assembly.GetAssembly(floppyImage.GetType())?.GetName().Version.ToString(),
                    Author =
                        ((AssemblyCompanyAttribute)
                            Attribute.GetCustomAttribute(Assembly.GetAssembly(floppyImage.GetType()),
                                                         typeof(AssemblyCompanyAttribute), false)).Company
                });

            foreach(IMediaImage mediaImage in GetPluginBase.Instance.ImagePluginsList.Values)
                mediaImages.Add(new PluginEntry
                {
                    Name    = mediaImage.Name,
                    Uuid    = mediaImage.Id,
                    Version = Assembly.GetAssembly(mediaImage.GetType())?.GetName().Version.ToString(),
                    Author =
                        ((AssemblyCompanyAttribute)
                            Attribute.GetCustomAttribute(Assembly.GetAssembly(mediaImage.GetType()),
                                                         typeof(AssemblyCompanyAttribute), false)).Company
                });

            foreach(IPartition partition in GetPluginBase.Instance.PartPluginsList.Values)
                partitions.Add(new PluginEntry
                {
                    Name    = partition.Name,
                    Uuid    = partition.Id,
                    Version = Assembly.GetAssembly(partition.GetType())?.GetName().Version.ToString(),
                    Author =
                        ((AssemblyCompanyAttribute)
                            Attribute.GetCustomAttribute(Assembly.GetAssembly(partition.GetType()),
                                                         typeof(AssemblyCompanyAttribute), false)).Company
                });

            foreach(IFilesystem filesystem in GetPluginBase.Instance.PluginsList.Values)
                filesystems.Add(new PluginEntry
                {
                    Name    = filesystem.Name,
                    Uuid    = filesystem.Id,
                    Version = Assembly.GetAssembly(filesystem.GetType())?.GetName().Version.ToString(),
                    Author =
                        ((AssemblyCompanyAttribute)
                            Attribute.GetCustomAttribute(Assembly.GetAssembly(filesystem.GetType()),
                                                         typeof(AssemblyCompanyAttribute), false)).Company
                });

            foreach(IReadOnlyFilesystem readOnlyFilesystem in GetPluginBase.Instance.ReadOnlyFilesystems.Values)
                readOnlyFilesystems.Add(new PluginEntry
                {
                    Name    = readOnlyFilesystem.Name,
                    Uuid    = readOnlyFilesystem.Id,
                    Version = Assembly.GetAssembly(readOnlyFilesystem.GetType())?.GetName().Version.ToString(),
                    Author =
                        ((AssemblyCompanyAttribute)
                            Attribute.GetCustomAttribute(Assembly.GetAssembly(readOnlyFilesystem.GetType()),
                                                         typeof(AssemblyCompanyAttribute), false)).Company
                });

            foreach(IWritableFloppyImage writableFloppyImage in GetPluginBase.Instance.WritableFloppyImages.Values)
                writableFloppyImages.Add(new PluginEntry
                {
                    Name    = writableFloppyImage.Name,
                    Uuid    = writableFloppyImage.Id,
                    Version = Assembly.GetAssembly(writableFloppyImage.GetType())?.GetName().Version.ToString(),
                    Author =
                        ((AssemblyCompanyAttribute)
                            Attribute.GetCustomAttribute(Assembly.GetAssembly(writableFloppyImage.GetType()),
                                                         typeof(AssemblyCompanyAttribute), false)).Company
                });

            foreach(IWritableImage writableImage in GetPluginBase.Instance.WritableImages.Values)
                writableImages.Add(new PluginEntry
                {
                    Name    = writableImage.Name,
                    Uuid    = writableImage.Id,
                    Version = Assembly.GetAssembly(writableImage.GetType())?.GetName().Version.ToString(),
                    Author =
                        ((AssemblyCompanyAttribute)
                            Attribute.GetCustomAttribute(Assembly.GetAssembly(writableImage.GetType()),
                                                         typeof(AssemblyCompanyAttribute), false)).Company
                });
        }

        protected void OnBtnClose(object sender, EventArgs e)
        {
            Close();
        }

        class PluginEntry
        {
            public string Name    { get; set; }
            public Guid   Uuid    { get; set; }
            public string Version { get; set; }
            public string Author  { get; set; }
        }

        #region XAML controls
        TabPage  tabFilters;
        GridView grdFilters;
        TabPage  tabPartitions;
        GridView grdPartitions;
        TabPage  tabFilesystems;
        GroupBox grpFilesystemIdentifyOnly;
        GridView grdFilesystem;
        GroupBox grpFilesystemReadable;
        GridView grdReadOnlyFilesystem;
        TabPage  tabMediaImages;
        GroupBox grpReadableMediaImages;
        GridView grdReadableMediaImages;
        GroupBox grpWritableMediaImages;
        GridView grdWritableMediaImages;
        Button   btnClose;
        TabPage  tabFloppyImages;
        GroupBox grpReadableFloppyImages;
        GridView grdReadableFloppyImages;
        GroupBox grpWritableFloppyImages;
        GridView grdWritableFloppyImages;
        #endregion
    }
}