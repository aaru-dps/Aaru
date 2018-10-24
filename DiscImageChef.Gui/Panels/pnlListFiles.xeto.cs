// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : pnlListFiles.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : List files.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the list files panel.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Panels
{
    // TODO: Resize columns
    // TODO: File icons?
    // TODO: Show xattrs
    public class pnlListFiles : Panel
    {
        GridColumn                         accessColumn;
        bool                               ascendingSort;
        GridColumn                         attributesColumn;
        GridColumn                         backupColumn;
        GridColumn                         changedColumn;
        GridColumn                         createdColumn;
        ObservableCollection<EntryForGrid> entries;
        IReadOnlyFilesystem                filesystem;
        GridColumn                         gidColumn;

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        GridView grdFiles;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion

        GridColumn inodeColumn;
        GridColumn linksColumn;
        GridColumn modeColumn;
        GridColumn nameColumn;
        GridColumn sizeColumn;
        GridColumn sortedColumn;
        GridColumn uidColumn;
        GridColumn writeColumn;

        public pnlListFiles(IReadOnlyFilesystem filesystem, Dictionary<string, FileEntryInfo> files, string parentPath)
        {
            this.filesystem = filesystem;
            XamlReader.Load(this);

            entries = new ObservableCollection<EntryForGrid>();

            nameColumn = new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<EntryForGrid, string>(r => r.Name)},
                HeaderText = "Name",
                Sortable   = true
            };
            sizeColumn = new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<EntryForGrid, string>(r => $"{r.Stat.Length}")
                },
                HeaderText = "Size",
                Sortable   = true
            };
            createdColumn = new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding =
                        Binding.Property<EntryForGrid, string>(r => r.Stat.CreationTime == default(DateTime)
                                                                        ? ""
                                                                        : $"{r.Stat.CreationTime:G}")
                },
                HeaderText = "Created",
                Sortable   = true
            };
            accessColumn = new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<EntryForGrid, string>(r => r.Stat.AccessTime == default(DateTime)
                                                                              ? ""
                                                                              : $"{r.Stat.AccessTime:G}")
                },
                HeaderText = "Last access",
                Sortable   = true
            };
            changedColumn = new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding =
                        Binding.Property<EntryForGrid, string>(r => r.Stat.StatusChangeTime == default(DateTime)
                                                                        ? ""
                                                                        : $"{r.Stat.StatusChangeTime:G}")
                },
                HeaderText = "Changed",
                Sortable   = true
            };
            backupColumn = new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<EntryForGrid, string>(r => r.Stat.BackupTime == default(DateTime)
                                                                              ? ""
                                                                              : $"{r.Stat.BackupTime:G}")
                },
                HeaderText = "Last backup",
                Sortable   = true
            };
            writeColumn = new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding =
                        Binding.Property<EntryForGrid, string>(r => r.Stat.LastWriteTime == default(DateTime)
                                                                        ? ""
                                                                        : $"{r.Stat.LastWriteTime:G}")
                },
                HeaderText = "Last write",
                Sortable   = true
            };
            attributesColumn = new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<EntryForGrid, string>(r => $"{r.Stat.Attributes}")
                },
                HeaderText = "Attributes",
                Sortable   = true
            };
            gidColumn = new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<EntryForGrid, string>(r => $"{r.Stat.GID}")},
                HeaderText = "GID",
                Sortable   = true
            };
            uidColumn = new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<EntryForGrid, string>(r => $"{r.Stat.UID}")},
                HeaderText = "UID",
                Sortable   = true
            };
            inodeColumn = new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<EntryForGrid, string>(r => $"{r.Stat.Inode}")
                },
                HeaderText = "Inode",
                Sortable   = true
            };
            linksColumn = new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<EntryForGrid, string>(r => $"{r.Stat.Links}")
                },
                HeaderText = "Links",
                Sortable   = true
            };
            modeColumn = new GridColumn
            {
                DataCell =
                    new TextBoxCell {Binding = Binding.Property<EntryForGrid, string>(r => $"{r.Stat.Mode}")},
                HeaderText = "Mode",
                Sortable   = true
            };

            grdFiles.Columns.Add(nameColumn);
            grdFiles.Columns.Add(sizeColumn);
            grdFiles.Columns.Add(createdColumn);
            grdFiles.Columns.Add(accessColumn);
            grdFiles.Columns.Add(changedColumn);
            grdFiles.Columns.Add(backupColumn);
            grdFiles.Columns.Add(writeColumn);
            grdFiles.Columns.Add(attributesColumn);
            grdFiles.Columns.Add(gidColumn);
            grdFiles.Columns.Add(uidColumn);
            grdFiles.Columns.Add(inodeColumn);
            grdFiles.Columns.Add(linksColumn);
            grdFiles.Columns.Add(modeColumn);

            grdFiles.AllowColumnReordering  = true;
            grdFiles.AllowDrop              = false;
            grdFiles.AllowMultipleSelection = true;
            grdFiles.ShowHeader             = true;

            foreach(KeyValuePair<string, FileEntryInfo> file in files)
                entries.Add(new EntryForGrid {Name = file.Key, Stat = file.Value, ParentPath = parentPath});

            grdFiles.DataStore         =  entries;
            sortedColumn               =  null;
            grdFiles.ColumnHeaderClick += OnGrdFilesOnColumnHeaderClick;
            ascendingSort              =  true;
        }

        void OnGrdFilesOnColumnHeaderClick(object sender, GridColumnEventArgs gridColumnEventArgs)
        {
            if(sortedColumn == gridColumnEventArgs.Column) ascendingSort = !ascendingSort;
            else ascendingSort                                           = true;

            sortedColumn = gridColumnEventArgs.Column;

            if(sortedColumn == nameColumn)
                grdFiles.DataStore =
                    ascendingSort ? entries.OrderBy(t => t.Name) : entries.OrderByDescending(t => t.Name);
            else if(sortedColumn == sizeColumn)
                grdFiles.DataStore = ascendingSort
                                         ? entries.OrderBy(t => t.Stat.Length)
                                         : entries.OrderByDescending(t => t.Stat.Length);
            else if(sortedColumn == createdColumn)
                grdFiles.DataStore = ascendingSort
                                         ? entries.OrderBy(t => t.Stat.CreationTime)
                                         : entries.OrderByDescending(t => t.Stat.CreationTime);
            else if(sortedColumn == accessColumn)
                grdFiles.DataStore = ascendingSort
                                         ? entries.OrderBy(t => t.Stat.AccessTime)
                                         : entries.OrderByDescending(t => t.Stat.AccessTime);
            else if(sortedColumn == changedColumn)
                grdFiles.DataStore = ascendingSort
                                         ? entries.OrderBy(t => t.Stat.StatusChangeTime)
                                         : entries.OrderByDescending(t => t.Stat.StatusChangeTime);
            else if(sortedColumn == backupColumn)
                grdFiles.DataStore = ascendingSort
                                         ? entries.OrderBy(t => t.Stat.BackupTime)
                                         : entries.OrderByDescending(t => t.Stat.BackupTime);
            else if(sortedColumn == writeColumn)
                grdFiles.DataStore = ascendingSort
                                         ? entries.OrderBy(t => t.Stat.LastWriteTime)
                                         : entries.OrderByDescending(t => t.Stat.LastWriteTime);
            else if(sortedColumn == attributesColumn)
                grdFiles.DataStore = ascendingSort
                                         ? entries.OrderBy(t => t.Stat.Attributes)
                                         : entries.OrderByDescending(t => t.Stat.Attributes);
            else if(sortedColumn == gidColumn)
                grdFiles.DataStore = ascendingSort
                                         ? entries.OrderBy(t => t.Stat.GID)
                                         : entries.OrderByDescending(t => t.Stat.GID);
            else if(sortedColumn == uidColumn)
                grdFiles.DataStore = ascendingSort
                                         ? entries.OrderBy(t => t.Stat.UID)
                                         : entries.OrderByDescending(t => t.Stat.UID);
            else if(sortedColumn == inodeColumn)
                grdFiles.DataStore = ascendingSort
                                         ? entries.OrderBy(t => t.Stat.Inode)
                                         : entries.OrderByDescending(t => t.Stat.Inode);
            else if(sortedColumn == linksColumn)
                grdFiles.DataStore = ascendingSort
                                         ? entries.OrderBy(t => t.Stat.Links)
                                         : entries.OrderByDescending(t => t.Stat.Links);
            else if(sortedColumn == modeColumn)
                grdFiles.DataStore = ascendingSort
                                         ? entries.OrderBy(t => t.Stat.Mode)
                                         : entries.OrderByDescending(t => t.Stat.Mode);
        }

        class EntryForGrid
        {
            public string        ParentPath;
            public FileEntryInfo Stat;
            public string        Name { get; set; }
        }
    }
}