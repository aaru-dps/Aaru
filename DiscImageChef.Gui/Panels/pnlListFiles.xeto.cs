// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : pnlListFiles.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : List files panel.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Interop;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Core;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Panels
{
    // TODO: Resize columns
    // TODO: File icons?
    // TODO: Show xattrs
    public class pnlListFiles : Panel
    {
        readonly GridColumn                         accessColumn;
        bool                                        ascendingSort;
        readonly GridColumn                         attributesColumn;
        readonly GridColumn                         backupColumn;
        readonly GridColumn                         changedColumn;
        readonly GridColumn                         createdColumn;
        readonly ObservableCollection<EntryForGrid> entries;
        readonly IReadOnlyFilesystem                filesystem;
        readonly GridColumn                         gidColumn;

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        GridView grdFiles;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion

        readonly GridColumn     inodeColumn;
        readonly GridColumn     linksColumn;
        readonly GridColumn     modeColumn;
        readonly GridColumn     nameColumn;
        readonly ButtonMenuItem saveFilesMenuItem;
        readonly GridColumn     sizeColumn;
        GridColumn              sortedColumn;
        readonly GridColumn     uidColumn;
        readonly GridColumn     writeColumn;

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

            grdFiles.ContextMenu    =  new ContextMenu();
            saveFilesMenuItem       =  new ButtonMenuItem {Text = "Extract to...", Enabled = false};
            saveFilesMenuItem.Click += OnSaveFilesMenuItemClick;

            grdFiles.ContextMenu.Items.Add(saveFilesMenuItem);

            grdFiles.SelectionChanged += OnGrdFilesSelectionChanged;
        }

        void OnGrdFilesSelectionChanged(object sender, EventArgs e)
        {
            saveFilesMenuItem.Enabled = grdFiles.SelectedItems.Any();
        }

        void OnSaveFilesMenuItemClick(object sender, EventArgs e)
        {
            if(!grdFiles.SelectedItems.Any()) return;

            SelectFolderDialog saveFilesFolderDialog = new SelectFolderDialog {Title = "Choose destination folder..."};

            DialogResult result = saveFilesFolderDialog.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            Statistics.AddCommand("extract-files");

            string folder = saveFilesFolderDialog.Directory;

            foreach(EntryForGrid file in grdFiles.SelectedItems)
            {
                string filename = file.Name;

                if(DetectOS.IsWindows)
                    if(filename.Contains('<')                || filename.Contains('>') ||
                       filename.Contains(':')                ||
                       filename.Contains('\\')               || filename.Contains('/') ||
                       filename.Contains('|')                ||
                       filename.Contains('?')                || filename.Contains('*') ||
                       filename.Any(c => c < 32)             ||
                       filename.ToUpperInvariant() == "CON"  || filename.ToUpperInvariant() == "PRN"  ||
                       filename.ToUpperInvariant() == "AUX"  || filename.ToUpperInvariant() == "COM1" ||
                       filename.ToUpperInvariant() == "COM2" || filename.ToUpperInvariant() == "COM3" ||
                       filename.ToUpperInvariant() == "COM4" || filename.ToUpperInvariant() == "COM5" ||
                       filename.ToUpperInvariant() == "COM6" || filename.ToUpperInvariant() == "COM7" ||
                       filename.ToUpperInvariant() == "COM8" || filename.ToUpperInvariant() == "COM9" ||
                       filename.ToUpperInvariant() == "LPT1" || filename.ToUpperInvariant() == "LPT2" ||
                       filename.ToUpperInvariant() == "LPT3" || filename.ToUpperInvariant() == "LPT4" ||
                       filename.ToUpperInvariant() == "LPT5" || filename.ToUpperInvariant() == "LPT6" ||
                       filename.ToUpperInvariant() == "LPT7" || filename.ToUpperInvariant() == "LPT8" ||
                       filename.ToUpperInvariant() == "LPT9" || filename.Last()             == '.'    ||
                       filename.Last()             == ' ')
                    {
                        char[] chars;
                        if(filename.Last() == '.' || filename.Last() == ' ') chars = new char[filename.Length - 1];
                        else chars                                                 = new char[filename.Length];

                        for(int ci = 0; ci < chars.Length; ci++)
                            switch(filename[ci])
                            {
                                case '<':
                                case '>':
                                case ':':
                                case '\\':
                                case '/':
                                case '|':
                                case '?':
                                case '*':
                                case '\u0000':
                                case '\u0001':
                                case '\u0002':
                                case '\u0003':
                                case '\u0004':
                                case '\u0005':
                                case '\u0006':
                                case '\u0007':
                                case '\u0008':
                                case '\u0009':
                                case '\u000A':
                                case '\u000B':
                                case '\u000C':
                                case '\u000D':
                                case '\u000E':
                                case '\u000F':
                                case '\u0010':
                                case '\u0011':
                                case '\u0012':
                                case '\u0013':
                                case '\u0014':
                                case '\u0015':
                                case '\u0016':
                                case '\u0017':
                                case '\u0018':
                                case '\u0019':
                                case '\u001A':
                                case '\u001B':
                                case '\u001C':
                                case '\u001D':
                                case '\u001E':
                                case '\u001F':
                                    chars[ci] = '_';
                                    break;
                                default:
                                    chars[ci] = filename[ci];
                                    break;
                            }

                        if(filename.StartsWith("CON", StringComparison.InvariantCultureIgnoreCase) ||
                           filename.StartsWith("PRN", StringComparison.InvariantCultureIgnoreCase) ||
                           filename.StartsWith("AUX", StringComparison.InvariantCultureIgnoreCase) ||
                           filename.StartsWith("COM", StringComparison.InvariantCultureIgnoreCase) ||
                           filename.StartsWith("LPT", StringComparison.InvariantCultureIgnoreCase))
                        {
                            chars[0] = '_';
                            chars[1] = '_';
                            chars[2] = '_';
                        }

                        string corrected = new string(chars);

                        result = MessageBox.Show(this, "Unsupported filename",
                                                 $"The file name {filename} is not supported on this platform.\nDo you want to rename it to {corrected}?",
                                                 MessageBoxButtons.YesNoCancel, MessageBoxType.Warning);

                        if(result == DialogResult.Cancel) return;

                        if(result == DialogResult.No) continue;

                        filename = corrected;
                    }

                string outputPath = Path.Combine(folder, filename);

                if(File.Exists(outputPath))
                {
                    result = MessageBox.Show(this, "Existing file",
                                             $"A file named {filename} already exists on the destination folder.\nDo you want to overwrite it?",
                                             MessageBoxButtons.YesNoCancel, MessageBoxType.Question);

                    if(result == DialogResult.Cancel) return;

                    if(result == DialogResult.No) continue;

                    try { File.Delete(outputPath); }
                    catch(IOException)
                    {
                        result = MessageBox.Show(this, "Cannot delete",
                                                 "Could not delete existing file.\nDo you want to continue?",
                                                 MessageBoxButtons.YesNo, MessageBoxType.Warning);
                        if(result == DialogResult.No) return;
                    }
                }

                try
                {
                    byte[] outBuf = new byte[0];

                    Errno error = filesystem.Read(file.ParentPath + file.Name, 0, file.Stat.Length, ref outBuf);

                    if(error != Errno.NoError)
                    {
                        result = MessageBox.Show(this, "Error reading file",
                                                 $"Error {error} reading file.\nDo you want to continue?",
                                                 MessageBoxButtons.YesNo, MessageBoxType.Warning);
                        if(result == DialogResult.No) return;

                        continue;
                    }

                    FileStream fs =
                        new FileStream(outputPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);

                    fs.Write(outBuf, 0, outBuf.Length);
                    fs.Close();
                    FileInfo fi = new FileInfo(outputPath);
                    #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                    try
                    {
                        if(file.Stat.CreationTimeUtc.HasValue) fi.CreationTimeUtc = file.Stat.CreationTimeUtc.Value;
                    }
                    catch
                    {
                        // ignored
                    }

                    try
                    {
                        if(file.Stat.LastWriteTimeUtc.HasValue) fi.LastWriteTimeUtc = file.Stat.LastWriteTimeUtc.Value;
                    }
                    catch
                    {
                        // ignored
                    }

                    try
                    {
                        if(file.Stat.AccessTimeUtc.HasValue) fi.LastAccessTimeUtc = file.Stat.AccessTimeUtc.Value;
                    }
                    catch
                    {
                        // ignored
                    }
                    #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                }
                catch(IOException)
                {
                    result = MessageBox.Show(this, "Cannot create file",
                                             "Could not create destination file.\nDo you want to continue?",
                                             MessageBoxButtons.YesNo, MessageBoxType.Warning);
                    if(result == DialogResult.No) return;
                }
            }
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