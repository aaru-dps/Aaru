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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Core;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Dialogs
{
    public class dlgStatistics : Dialog
    {
        public dlgStatistics()
        {
            XamlReader.Load(this);

            if(Statistics.AllStats.Commands != null)
            {
                if(Statistics.AllStats.Commands.Analyze > 0)
                {
                    lblAnalyze.Visible = true;
                    lblAnalyze.Text =
                        $"You have called the Analyze command {Statistics.AllStats.Commands.Analyze} times";
                }

                if(Statistics.AllStats.Commands.Benchmark > 0)
                {
                    lblBenchmark.Visible = true;
                    lblBenchmark.Text =
                        $"You have called the Benchmark command {Statistics.AllStats.Commands.Benchmark} times";
                }

                if(Statistics.AllStats.Commands.Checksum > 0)
                {
                    lblChecksum.Visible = true;
                    lblChecksum.Text =
                        $"You have called the Checksum command {Statistics.AllStats.Commands.Checksum} times";
                }

                if(Statistics.AllStats.Commands.Compare > 0)
                {
                    lblCompare.Visible = true;
                    lblCompare.Text =
                        $"You have called the Compare command {Statistics.AllStats.Commands.Compare} times";
                }

                if(Statistics.AllStats.Commands.ConvertImage > 0)
                {
                    lblConvertImage.Visible = true;
                    lblConvertImage.Text =
                        $"You have called the Convert-Image command {Statistics.AllStats.Commands.ConvertImage} times";
                }

                if(Statistics.AllStats.Commands.CreateSidecar > 0)
                {
                    lblCreateSidecar.Visible = true;
                    lblCreateSidecar.Text =
                        $"You have called the Create-Sidecar command {Statistics.AllStats.Commands.CreateSidecar} times";
                }

                if(Statistics.AllStats.Commands.Decode > 0)
                {
                    lblDecode.Visible = true;
                    lblDecode.Text =
                        $"You have called the Decode command {Statistics.AllStats.Commands.Decode} times";
                }

                if(Statistics.AllStats.Commands.DeviceInfo > 0)
                {
                    lblDeviceInfo.Visible = true;
                    lblDeviceInfo.Text =
                        $"You have called the Device-Info command {Statistics.AllStats.Commands.DeviceInfo} times";
                }

                if(Statistics.AllStats.Commands.DeviceReport > 0)
                {
                    lblDeviceReport.Visible = true;
                    lblDeviceReport.Text =
                        $"You have called the Device-Report command {Statistics.AllStats.Commands.DeviceReport} times";
                }

                if(Statistics.AllStats.Commands.DumpMedia > 0)
                {
                    lblDumpMedia.Visible = true;
                    lblDumpMedia.Text =
                        $"You have called the Dump-Media command {Statistics.AllStats.Commands.DumpMedia} times";
                }

                if(Statistics.AllStats.Commands.Entropy > 0)
                {
                    lblEntropy.Visible = true;
                    lblEntropy.Text =
                        $"You have called the Entropy command {Statistics.AllStats.Commands.Entropy} times";
                }

                if(Statistics.AllStats.Commands.Formats > 0)
                {
                    lblFormats.Visible = true;
                    lblFormats.Text =
                        $"You have called the Formats command {Statistics.AllStats.Commands.Formats} times";
                }

                if(Statistics.AllStats.Commands.ImageInfo > 0)
                {
                    lblImageInfo.Visible = true;
                    lblImageInfo.Text =
                        $"You have called the Image-Info command {Statistics.AllStats.Commands.ImageInfo} times";
                }

                if(Statistics.AllStats.Commands.MediaInfo > 0)
                {
                    lblMediaInfo.Visible = true;
                    lblMediaInfo.Text =
                        $"You have called the Media-Info command {Statistics.AllStats.Commands.MediaInfo} times";
                }

                if(Statistics.AllStats.Commands.MediaScan > 0)
                {
                    lblMediaScan.Visible = true;
                    lblMediaScan.Text =
                        $"You have called the Media-Scan command {Statistics.AllStats.Commands.MediaScan} times";
                }

                if(Statistics.AllStats.Commands.PrintHex > 0)
                {
                    lblPrintHex.Visible = true;
                    lblPrintHex.Text =
                        $"You have called the Print-Hex command {Statistics.AllStats.Commands.PrintHex} times";
                }

                if(Statistics.AllStats.Commands.Verify > 0)
                {
                    lblVerify.Visible = true;
                    lblVerify.Text =
                        $"You have called the Verify command {Statistics.AllStats.Commands.Verify} times";
                }

                tabCommands.Visible = lblAnalyze.Visible   || lblBenchmark.Visible    || lblChecksum.Visible      ||
                                      lblCompare.Visible   || lblConvertImage.Visible || lblCreateSidecar.Visible ||
                                      lblDecode.Visible    || lblDeviceInfo.Visible   || lblDeviceReport.Visible  ||
                                      lblDumpMedia.Visible || lblEntropy.Visible      || lblFormats.Visible       ||
                                      lblImageInfo.Visible || lblMediaInfo.Visible    || lblMediaScan.Visible     ||
                                      lblPrintHex.Visible  || lblVerify.Visible;
            }

            if(Statistics.AllStats.Benchmark != null)
            {
                StackLayout stkBenchmarks = new StackLayout();

                foreach(ChecksumStats chk in Statistics.AllStats.Benchmark.Checksum)
                    stkBenchmarks.Items.Add(new Label
                    {
                        Text =
                            $"Took {chk.Value} seconds to calculate {chk.algorithm} algorithm"
                    });

                stkBenchmarks.Items.Add(new Label
                {
                    Text =
                        $"Took {Statistics.AllStats.Benchmark.Sequential} seconds to calculate all algorithms sequentially"
                });
                stkBenchmarks.Items.Add(new Label
                {
                    Text =
                        $"Took {Statistics.AllStats.Benchmark.All} seconds to calculate all algorithms at the same time"
                });
                stkBenchmarks.Items.Add(new Label
                {
                    Text =
                        $"Took {Statistics.AllStats.Benchmark.Entropy} seconds to calculate entropy"
                });

                stkBenchmarks.Items.Add(new Label
                {
                    Text =
                        $"Used a maximum of {Statistics.AllStats.Benchmark.MaxMemory} bytes of memory"
                });
                stkBenchmarks.Items.Add(new Label
                {
                    Text =
                        $"Used a minimum of {Statistics.AllStats.Benchmark.MinMemory} bytes of memory"
                });
                tabBenchmark.Content = stkBenchmarks;
                tabBenchmark.Visible = true;
            }

            if(Statistics.AllStats.Filters != null && Statistics.AllStats.Filters.Count > 0)
            {
                tabFilters.Visible = true;

                TreeGridItemCollection filterList = new TreeGridItemCollection();

                treeFilters.Columns.Add(new GridColumn {HeaderText = "Filter", DataCell      = new TextBoxCell(0)});
                treeFilters.Columns.Add(new GridColumn {HeaderText = "Times found", DataCell = new TextBoxCell(1)});

                treeFilters.AllowMultipleSelection = false;
                treeFilters.ShowHeader             = true;
                treeFilters.DataStore              = filterList;

                foreach(NameValueStats nvs in Statistics.AllStats.Filters.OrderBy(n => n.name))
                    filterList.Add(new TreeGridItem {Values = new object[] {nvs.name, nvs.Value}});
            }

            if(Statistics.AllStats.MediaImages != null && Statistics.AllStats.MediaImages.Count > 0)
            {
                tabFormats.Visible = true;

                TreeGridItemCollection formatList = new TreeGridItemCollection();

                treeFormats.Columns.Add(new GridColumn {HeaderText = "Format", DataCell      = new TextBoxCell(0)});
                treeFormats.Columns.Add(new GridColumn {HeaderText = "Times found", DataCell = new TextBoxCell(1)});

                treeFormats.AllowMultipleSelection = false;
                treeFormats.ShowHeader             = true;
                treeFormats.DataStore              = formatList;

                foreach(NameValueStats nvs in Statistics.AllStats.MediaImages.OrderBy(n => n.name))
                    formatList.Add(new TreeGridItem {Values = new object[] {nvs.name, nvs.Value}});
            }

            if(Statistics.AllStats.Partitions != null && Statistics.AllStats.Partitions.Count > 0)
            {
                tabPartitions.Visible = true;

                TreeGridItemCollection partitionList = new TreeGridItemCollection();

                treePartitions.Columns.Add(new GridColumn {HeaderText = "Filter", DataCell      = new TextBoxCell(0)});
                treePartitions.Columns.Add(new GridColumn {HeaderText = "Times found", DataCell = new TextBoxCell(1)});

                treePartitions.AllowMultipleSelection = false;
                treePartitions.ShowHeader             = true;
                treePartitions.DataStore              = partitionList;

                foreach(NameValueStats nvs in Statistics.AllStats.Partitions.OrderBy(n => n.name))
                    partitionList.Add(new TreeGridItem {Values = new object[] {nvs.name, nvs.Value}});
            }

            if(Statistics.AllStats.Filesystems != null && Statistics.AllStats.Filesystems.Count > 0)
            {
                tabFilesystems.Visible = true;

                TreeGridItemCollection filesystemList = new TreeGridItemCollection();

                treeFilesystems.Columns.Add(new GridColumn {HeaderText = "Filesystem", DataCell  = new TextBoxCell(0)});
                treeFilesystems.Columns.Add(new GridColumn {HeaderText = "Times found", DataCell = new TextBoxCell(1)});

                treeFilesystems.AllowMultipleSelection = false;
                treeFilesystems.ShowHeader             = true;
                treeFilesystems.DataStore              = filesystemList;

                foreach(NameValueStats nvs in Statistics.AllStats.Filesystems.OrderBy(n => n.name))
                    filesystemList.Add(new TreeGridItem {Values = new object[] {nvs.name, nvs.Value}});
            }

            if(Statistics.AllStats.Devices != null && Statistics.AllStats.Devices.Count > 0)
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

                foreach(DeviceStats ds in Statistics.AllStats.Devices.OrderBy(n => n.Manufacturer)
                                                    .ThenBy(n => n.Manufacturer).ThenBy(n => n.Revision)
                                                    .ThenBy(n => n.Bus))
                    deviceList.Add(new TreeGridItem
                    {
                        Values = new object[] {ds.Model, ds.Manufacturer, ds.Revision, ds.Bus}
                    });
            }

            if(Statistics.AllStats.Medias != null && Statistics.AllStats.Medias.Count > 0)
            {
                tabMedias.Visible = true;

                TreeGridItemCollection mediaList = new TreeGridItemCollection();

                treeMedias.Columns.Add(new GridColumn {HeaderText = "Media", DataCell       = new TextBoxCell(0)});
                treeMedias.Columns.Add(new GridColumn {HeaderText = "Times found", DataCell = new TextBoxCell(1)});
                treeMedias.Columns.Add(new GridColumn {HeaderText = "Type", DataCell        = new TextBoxCell(2)});

                treeMedias.AllowMultipleSelection = false;
                treeMedias.ShowHeader             = true;
                treeMedias.DataStore              = mediaList;

                foreach(MediaStats ms in Statistics.AllStats.Medias.OrderBy(m => m.type).ThenBy(m => m.real))
                    mediaList.Add(new TreeGridItem
                    {
                        Values = new object[] {ms.type, ms.Value, ms.real ? "real" : "image"}
                    });
            }

            if(Statistics.AllStats.MediaScan != null)
            {
                tabMediaScan.Visible   = true;
                lblSectorsTotal.Text   = $"Scanned a total of {Statistics.AllStats.MediaScan.Sectors.Total} sectors";
                lblSectorsCorrect.Text = $"{Statistics.AllStats.MediaScan.Sectors.Correct} of them correctly";
                lblSectorsError.Text   = $"{Statistics.AllStats.MediaScan.Sectors.Error} of them had errors";
                lblLessThan3ms.Text =
                    $"{Statistics.AllStats.MediaScan.Times.LessThan3ms} of them took less than 3 ms";
                lblLessThan10ms.Text =
                    $"{Statistics.AllStats.MediaScan.Times.LessThan10ms} of them took less than 10 ms but more than 3 ms";
                lblLessThan50ms.Text =
                    $"{Statistics.AllStats.MediaScan.Times.LessThan50ms} of them took less than 50 ms but more than 10 ms";
                lblLessThan150ms.Text =
                    $"{Statistics.AllStats.MediaScan.Times.LessThan150ms} of them took less than 150 ms but more than 50 ms";
                lblLessThan500ms.Text =
                    $"{Statistics.AllStats.MediaScan.Times.LessThan500ms} of them took less than 500 ms but more than 150 ms";
                lblMoreThan500ms.Text =
                    $"{Statistics.AllStats.MediaScan.Times.MoreThan500ms} of them took less than more than 500 ms";
            }

            if(Statistics.AllStats.Verify == null) return;

            tabVerify.Visible = true;
            lblCorrectImages.Text =
                $"{Statistics.AllStats.Verify.MediaImages.Correct} media images has been correctly verified";
            lblFailedImages.Text =
                $"{Statistics.AllStats.Verify.MediaImages.Failed} media images has been determined as containing errors";
            lblVerifiedSectors.Text = $"{Statistics.AllStats.Verify.Sectors.Total} sectors has been verified";
            lblCorrectSectors.Text =
                $"{Statistics.AllStats.Verify.Sectors.Correct} sectors has been determined correct";
            lblFailedSectors.Text =
                $"{Statistics.AllStats.Verify.Sectors.Error} sectors has been determined to contain errors";
            lblUnknownSectors.Text =
                $"{Statistics.AllStats.Verify.Sectors.Unverifiable} sectors could not be determined as correct or not";
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
        TabPage      tabBenchmark;
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
        TabPage      tabMediaScan;
        Label        lblSectorsTotal;
        Label        lblSectorsCorrect;
        Label        lblSectorsError;
        Label        lblLessThan3ms;
        Label        lblLessThan10ms;
        Label        lblLessThan50ms;
        Label        lblLessThan150ms;
        Label        lblLessThan500ms;
        Label        lblMoreThan500ms;
        TabPage      tabVerify;
        Label        lblCorrectImages;
        Label        lblFailedImages;
        Label        lblVerifiedSectors;
        Label        lblCorrectSectors;
        Label        lblFailedSectors;
        Label        lblUnknownSectors;
        #endregion
    }
}