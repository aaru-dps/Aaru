// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : dlgStatistics.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Statistics dialog.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the statistics dialog.
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
using System.Linq;
using DiscImageChef.Database;
using DiscImageChef.Database.Models;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Dialogs
{
    public class dlgStatistics : Dialog
    {
        public dlgStatistics()
        {
            XamlReader.Load(this);

            DicContext ctx = DicContext.Create(Settings.Settings.LocalDbPath);

            if(ctx.Commands.Any())
            {
                if(ctx.Commands.Any(c => c.Name == "analyze"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "analyze" && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "analyze" && !c.Synchronized);

                    lblAnalyze.Visible = true;
                    lblAnalyze.Text    = $"You have called the Analyze command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "benchmark"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "benchmark" && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "benchmark" && !c.Synchronized);

                    lblBenchmark.Visible = true;
                    lblBenchmark.Text    = $"You have called the Benchmark command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "checksum"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "checksum" && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "checksum" && !c.Synchronized);

                    lblChecksum.Visible = true;
                    lblChecksum.Text    = $"You have called the Checksum command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "compare"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "compare" && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "compare" && !c.Synchronized);

                    lblCompare.Visible = true;
                    lblCompare.Text    = $"You have called the Compare command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "convert-image"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "convert-image" && c.Synchronized)
                                     .Select(c => c.Count).FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "convert-image" && !c.Synchronized);

                    lblConvertImage.Visible = true;
                    lblConvertImage.Text    = $"You have called the Convert-Image command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "create-sidecar"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "create-sidecar" && c.Synchronized)
                                     .Select(c => c.Count).FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "create-sidecar" && !c.Synchronized);

                    lblCreateSidecar.Visible = true;
                    lblCreateSidecar.Text    = $"You have called the Create-Sidecar command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "decode"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "decode" && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "decode" && !c.Synchronized);

                    lblDecode.Visible = true;
                    lblDecode.Text    = $"You have called the Decode command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "device-info"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "device-info" && c.Synchronized)
                                     .Select(c => c.Count).FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "device-info" && !c.Synchronized);

                    lblDeviceInfo.Visible = true;
                    lblDeviceInfo.Text    = $"You have called the Device-Info command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "device-report"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "device-report" && c.Synchronized)
                                     .Select(c => c.Count).FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "device-report" && !c.Synchronized);

                    lblDeviceReport.Visible = true;
                    lblDeviceReport.Text    = $"You have called the Device-Report command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "dump-media"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "dump-media" && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "dump-media" && !c.Synchronized);

                    lblDumpMedia.Visible = true;
                    lblDumpMedia.Text    = $"You have called the Dump-Media command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "entropy"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "entropy" && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "entropy" && !c.Synchronized);

                    lblEntropy.Visible = true;
                    lblEntropy.Text    = $"You have called the Entropy command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "formats"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "formats" && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "formats" && !c.Synchronized);

                    lblFormats.Visible = true;
                    lblFormats.Text    = $"You have called the Formats command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "image-info"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "image-info" && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "image-info" && !c.Synchronized);

                    lblImageInfo.Visible = true;
                    lblImageInfo.Text    = $"You have called the Image-Info command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "media-info"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "media-info" && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "media-info" && !c.Synchronized);

                    lblMediaInfo.Visible = true;
                    lblMediaInfo.Text    = $"You have called the Media-Info command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "media-scan"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "media-scan" && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "media-scan" && !c.Synchronized);

                    lblMediaScan.Visible = true;
                    lblMediaScan.Text    = $"You have called the Media-Scan command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "printhex"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "printhex" && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "printhex" && !c.Synchronized);

                    lblPrintHex.Visible = true;
                    lblPrintHex.Text    = $"You have called the Print-Hex command {count} times";
                }

                if(ctx.Commands.Any(c => c.Name == "verify"))
                {
                    ulong count = ctx.Commands.Where(c => c.Name == "verify" && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Commands.LongCount(c => c.Name == "verify" && !c.Synchronized);

                    lblVerify.Visible = true;
                    lblVerify.Text    = $"You have called the Verify command {count} times";
                }

                tabCommands.Visible = lblAnalyze.Visible   || lblBenchmark.Visible    || lblChecksum.Visible      ||
                                      lblCompare.Visible   || lblConvertImage.Visible || lblCreateSidecar.Visible ||
                                      lblDecode.Visible    || lblDeviceInfo.Visible   || lblDeviceReport.Visible  ||
                                      lblDumpMedia.Visible || lblEntropy.Visible      || lblFormats.Visible       ||
                                      lblImageInfo.Visible || lblMediaInfo.Visible    || lblMediaScan.Visible     ||
                                      lblPrintHex.Visible  || lblVerify.Visible;
            }

            if(ctx.Filters.Any())
            {
                tabFilters.Visible = true;

                TreeGridItemCollection filterList = new TreeGridItemCollection();

                treeFilters.Columns.Add(new GridColumn {HeaderText = "Filter", DataCell      = new TextBoxCell(0)});
                treeFilters.Columns.Add(new GridColumn {HeaderText = "Times found", DataCell = new TextBoxCell(1)});

                treeFilters.AllowMultipleSelection = false;
                treeFilters.ShowHeader             = true;
                treeFilters.DataStore              = filterList;

                foreach(string nvs in ctx.Filters.Select(n => n.Name).Distinct())
                {
                    ulong count = ctx.Filters.Where(c => c.Name == nvs && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Filters.LongCount(c => c.Name == nvs && !c.Synchronized);

                    filterList.Add(new TreeGridItem {Values = new object[] {nvs, count}});
                }
            }

            if(ctx.MediaFormats.Any())
            {
                tabFormats.Visible = true;

                TreeGridItemCollection formatList = new TreeGridItemCollection();

                treeFormats.Columns.Add(new GridColumn {HeaderText = "Format", DataCell      = new TextBoxCell(0)});
                treeFormats.Columns.Add(new GridColumn {HeaderText = "Times found", DataCell = new TextBoxCell(1)});

                treeFormats.AllowMultipleSelection = false;
                treeFormats.ShowHeader             = true;
                treeFormats.DataStore              = formatList;

                foreach(string nvs in ctx.MediaFormats.Select(n => n.Name).Distinct())
                {
                    ulong count = ctx.MediaFormats.Where(c => c.Name == nvs && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.MediaFormats.LongCount(c => c.Name == nvs && !c.Synchronized);

                    formatList.Add(new TreeGridItem {Values = new object[] {nvs, count}});
                }
            }

            if(ctx.Partitions.Any())
            {
                tabPartitions.Visible = true;

                TreeGridItemCollection partitionList = new TreeGridItemCollection();

                treePartitions.Columns.Add(new GridColumn {HeaderText = "Filter", DataCell      = new TextBoxCell(0)});
                treePartitions.Columns.Add(new GridColumn {HeaderText = "Times found", DataCell = new TextBoxCell(1)});

                treePartitions.AllowMultipleSelection = false;
                treePartitions.ShowHeader             = true;
                treePartitions.DataStore              = partitionList;

                foreach(string nvs in ctx.Partitions.Select(n => n.Name).Distinct())
                {
                    ulong count = ctx.Partitions.Where(c => c.Name == nvs && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Partitions.LongCount(c => c.Name == nvs && !c.Synchronized);

                    partitionList.Add(new TreeGridItem {Values = new object[] {nvs, count}});
                }
            }

            if(ctx.Filesystems.Any())
            {
                tabFilesystems.Visible = true;

                TreeGridItemCollection filesystemList = new TreeGridItemCollection();

                treeFilesystems.Columns.Add(new GridColumn {HeaderText = "Filesystem", DataCell  = new TextBoxCell(0)});
                treeFilesystems.Columns.Add(new GridColumn {HeaderText = "Times found", DataCell = new TextBoxCell(1)});

                treeFilesystems.AllowMultipleSelection = false;
                treeFilesystems.ShowHeader             = true;
                treeFilesystems.DataStore              = filesystemList;

                foreach(string nvs in ctx.Filesystems.Select(n => n.Name).Distinct())
                {
                    ulong count = ctx.Filesystems.Where(c => c.Name == nvs && c.Synchronized).Select(c => c.Count)
                                     .FirstOrDefault();
                    count += (ulong)ctx.Filesystems.LongCount(c => c.Name == nvs && !c.Synchronized);

                    filesystemList.Add(new TreeGridItem {Values = new object[] {nvs, count}});
                }
            }

            if(ctx.SeenDevices.Any())
            {
                tabDevices.Visible = true;

                TreeGridItemCollection deviceList = new TreeGridItemCollection();

                treeDevices.Columns.Add(new GridColumn {HeaderText = "Device", DataCell       = new TextBoxCell(0)});
                treeDevices.Columns.Add(new GridColumn {HeaderText = "Manufacturer", DataCell = new TextBoxCell(1)});
                treeDevices.Columns.Add(new GridColumn {HeaderText = "Revision", DataCell     = new TextBoxCell(2)});
                treeDevices.Columns.Add(new GridColumn {HeaderText = "Bus", DataCell          = new TextBoxCell(3)});

                treeDevices.AllowMultipleSelection = false;
                treeDevices.ShowHeader             = true;
                treeDevices.DataStore              = deviceList;

                foreach(DeviceStat ds in ctx.SeenDevices.OrderBy(n => n.Manufacturer).ThenBy(n => n.Manufacturer)
                                            .ThenBy(n => n.Revision)
                                            .ThenBy(n => n.Bus))
                    deviceList.Add(new TreeGridItem
                    {
                        Values = new object[] {ds.Model, ds.Manufacturer, ds.Revision, ds.Bus}
                    });
            }

            if(!ctx.Medias.Any()) return;

            tabMedias.Visible = true;

            TreeGridItemCollection mediaList = new TreeGridItemCollection();

            treeMedias.Columns.Add(new GridColumn {HeaderText = "Media", DataCell       = new TextBoxCell(0)});
            treeMedias.Columns.Add(new GridColumn {HeaderText = "Times found", DataCell = new TextBoxCell(1)});
            treeMedias.Columns.Add(new GridColumn {HeaderText = "Type", DataCell        = new TextBoxCell(2)});

            treeMedias.AllowMultipleSelection = false;
            treeMedias.ShowHeader             = true;
            treeMedias.DataStore              = mediaList;

            foreach(string media in ctx.Medias.OrderBy(ms => ms.Type).Select(ms => ms.Type).Distinct())
            {
                ulong count = ctx.Medias.Where(c => c.Type == media && c.Synchronized && c.Real).Select(c => c.Count)
                                 .FirstOrDefault();
                count += (ulong)ctx.Medias.LongCount(c => c.Type == media && !c.Synchronized && c.Real);

                if(count > 0) mediaList.Add(new TreeGridItem {Values = new object[] {media, count, "real"}});

                count = ctx.Medias.Where(c => c.Type == media && c.Synchronized && !c.Real).Select(c => c.Count)
                           .FirstOrDefault();
                count += (ulong)ctx.Medias.LongCount(c => c.Type == media && !c.Synchronized && !c.Real);

                if(count == 0) continue;

                mediaList.Add(new TreeGridItem {Values = new object[] {media, count, "image"}});
            }
        }

        protected void OnBtnClose(object sender, EventArgs e)
        {
            Close();
        }

        #region XAML controls
        TabPage      tabCommands;
        Label        lblAnalyze;
        Label        lblBenchmark;
        Label        lblChecksum;
        Label        lblCompare;
        Label        lblConvertImage;
        Label        lblCreateSidecar;
        Label        lblDecode;
        Label        lblDeviceInfo;
        Label        lblDeviceReport;
        Label        lblDumpMedia;
        Label        lblEntropy;
        Label        lblFormats;
        Label        lblImageInfo;
        Label        lblMediaInfo;
        Label        lblMediaScan;
        Label        lblPrintHex;
        Label        lblVerify;
        TabPage      tabFilters;
        TreeGridView treeFilters;
        TabPage      tabFormats;
        TreeGridView treeFormats;
        TabPage      tabPartitions;
        TreeGridView treePartitions;
        TabPage      tabFilesystems;
        TreeGridView treeFilesystems;
        TabPage      tabDevices;
        TreeGridView treeDevices;
        TabPage      tabMedias;
        TreeGridView treeMedias;
        #endregion
    }
}