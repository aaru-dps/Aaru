// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : frmMediaScan.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Media surface scan window.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements media scan GUI window.
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
using System.Threading;
using DiscImageChef.CommonTypes;
using DiscImageChef.Core;
using DiscImageChef.Core.Devices.Scanning;
using DiscImageChef.Core.Media.Info;
using DiscImageChef.Devices;
using DiscImageChef.Gui.Controls;
using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;
using DeviceInfo = DiscImageChef.Core.Devices.Info.DeviceInfo;

namespace DiscImageChef.Gui.Forms
{
    public class frmMediaScan : Form
    {
        static readonly Color LightGreen = Color.FromRgb(0x00FF00);
        static readonly Color Green      = Color.FromRgb(0x006400);
        static readonly Color DarkGreen  = Color.FromRgb(0x003200);
        static readonly Color Yellow     = Color.FromRgb(0xFFA500);
        static readonly Color Orange     = Color.FromRgb(0xFF4500);
        static readonly Color Red        = Color.FromRgb(0x800000);
        static          Color LightRed   = Color.FromRgb(0xFF0000);
        ulong                 blocksToRead;
        string                devicePath;
        ScanResults           localResults;
        MediaScan             scanner;

        public frmMediaScan(string devicePath, DeviceInfo deviceInfo, ScsiInfo scsiInfo = null)
        {
            XamlReader.Load(this);

            this.devicePath = devicePath;
            btnStop.Visible = false;

            lineChart.AbsoluteMargins = true;
            lineChart.MarginX         = 5;
            lineChart.MarginY         = 5;
            lineChart.DrawAxes        = true;
            lineChart.AxesColor       = Colors.Black;
            lineChart.ColorX          = Colors.Gray;
            lineChart.ColorY          = Colors.Gray;
            lineChart.BackgroundColor = Color.FromRgb(0x2974c1);
            lineChart.LineColor       = Colors.Yellow;
        }

        void OnBtnCancelClick(object sender, EventArgs e)
        {
            Close();
        }

        void OnBtnStopClick(object sender, EventArgs e)
        {
            scanner.Abort();
        }

        void OnBtnScanClick(object sender, EventArgs e)
        {
            btnStop.Visible     = true;
            btnScan.Visible     = false;
            btnCancel.Visible   = false;
            stkProgress.Visible = true;
            tabResults.Visible  = true;
            new Thread(DoWork).Start();
        }

        // TODO: Allow to save MHDD and ImgBurn log files
        void DoWork()
        {
            if(devicePath.Length == 2 && devicePath[1] == ':' && devicePath[0] != '/' && char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            Device dev = new Device(devicePath);

            if(dev.Error)
            {
                MessageBox.Show($"Error {dev.LastError} opening device.", MessageBoxType.Error);
                btnStop.Visible     = false;
                btnScan.Visible     = true;
                btnCancel.Visible   = true;
                stkProgress.Visible = false;

                return;
            }

            Statistics.AddDevice(dev);

            localResults                 =  new ScanResults();
            scanner                      =  new MediaScan(null, null, devicePath, dev);
            scanner.ScanTime             += OnScanTime;
            scanner.ScanUnreadable       += OnScanUnreadable;
            scanner.UpdateStatus         += UpdateStatus;
            scanner.StoppingErrorMessage += StoppingErrorMessage;
            scanner.PulseProgress        += PulseProgress;
            scanner.InitProgress         += InitProgress;
            scanner.UpdateProgress       += UpdateProgress;
            scanner.EndProgress          += EndProgress;
            scanner.InitBlockMap         += InitBlockMap;
            scanner.ScanSpeed            += ScanSpeed;

            ScanResults results = scanner.Scan();

            Application.Instance.Invoke(() =>
            {
                lblTotalTime.Text = lblTotalTime.Text =
                                        $"Took a total of {results.TotalTime} seconds ({results.ProcessingTime} processing commands).";
                lblAvgSpeed.Text          = $"Average speed: {results.AvgSpeed:F3} MiB/sec.";
                lblMaxSpeed.Text          = $"Fastest speed burst: {results.MaxSpeed:F3} MiB/sec.";
                lblMinSpeed.Text          = $"Slowest speed burst: {results.MinSpeed:F3} MiB/sec.";
                lblA.Text                 = $"{results.A} sectors took less than 3 ms.";
                lblB.Text                 = $"{results.B} sectors took less than 10 ms but more than 3 ms.";
                lblC.Text                 = $"{results.C} sectors took less than 50 ms but more than 10 ms.";
                lblD.Text                 = $"{results.D} sectors took less than 150 ms but more than 50 ms.";
                lblE.Text                 = $"{results.E} sectors took less than 500 ms but more than 150 ms.";
                lblF.Text                 = $"{results.F} sectors took more than 500 ms.";
                lblUnreadableSectors.Text = $"{results.UnreadableSectors.Count} sectors could not be read.";
            });

            // TODO: Show list of unreadable sectors
            /*
            if(results.UnreadableSectors.Count > 0)
                foreach(ulong bad in results.UnreadableSectors)
                    string.Format("Sector {0} could not be read", bad);
*/

            // TODO: Show results
            /*
            #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if(results.SeekTotal != 0 || results.SeekMin != double.MaxValue || results.SeekMax != double.MinValue)
                #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                string.Format("Testing {0} seeks, longest seek took {1:F3} ms, fastest one took {2:F3} ms. ({3:F3} ms average)",
                                     results.SeekTimes, results.SeekMax, results.SeekMin, results.SeekTotal / 1000);
                                     */

            Statistics.AddCommand("media-scan");

            dev.Close();
            WorkFinished();
        }

        void ScanSpeed(ulong sector, double currentspeed)
        {
            Application.Instance.Invoke(() =>
            {
                if(currentspeed > lineChart.MaxY) lineChart.MaxY = (float)(currentspeed + currentspeed / 10);

                lineChart.Values.Add(new PointF(sector, (float)currentspeed));
            });
        }

        void InitBlockMap(ulong blocks, ulong blocksize, ulong blockstoread, ushort currentProfile)
        {
            Application.Instance.Invoke(() =>
            {
                blockMap.Sectors       = blocks;
                blockMap.SectorsToRead = (uint)blockstoread;
                blocksToRead           = blockstoread;
                lineChart.MinX         = 0;
                lineChart.MinY         = 0;
                switch(currentProfile)
                {
                    case 0x0005: // CD and DDCD
                    case 0x0008:
                    case 0x0009:
                    case 0x000A:
                    case 0x0020:
                    case 0x0021:
                    case 0x0022:
                        if(blocks      <= 360000) lineChart.MaxX = 360000;
                        else if(blocks <= 405000) lineChart.MaxX = 405000;
                        else if(blocks <= 445500) lineChart.MaxX = 445500;
                        else lineChart.MaxX                      = blocks;
                        lineChart.StepsX = lineChart.MaxX   / 10f;
                        lineChart.StepsY = 150              * 4;
                        lineChart.MaxY   = lineChart.StepsY * 12.5f;
                        break;
                    case 0x0010: // DVD SL
                    case 0x0011:
                    case 0x0012:
                    case 0x0013:
                    case 0x0014:
                    case 0x0018:
                    case 0x001A:
                    case 0x001B:
                        lineChart.MaxX   = 2298496;
                        lineChart.StepsX = lineChart.MaxX / 10f;
                        lineChart.StepsY = 1352.5f;
                        lineChart.MaxY   = lineChart.StepsY * 26;
                        break;
                    case 0x0015: // DVD DL
                    case 0x0016:
                    case 0x0017:
                    case 0x002A:
                    case 0x002B:
                        lineChart.MaxX   = 4173824;
                        lineChart.StepsX = lineChart.MaxX / 10f;
                        lineChart.StepsY = 1352.5f;
                        lineChart.MaxY   = lineChart.StepsY * 26;
                        break;
                    case 0x0041:
                    case 0x0042:
                    case 0x0043:
                    case 0x0040: // BD
                        if(blocks      <= 12219392) lineChart.MaxX = 12219392;
                        else if(blocks <= 24438784) lineChart.MaxX = 24438784;
                        else if(blocks <= 48878592) lineChart.MaxX = 48878592;
                        else if(blocks <= 62500864) lineChart.MaxX = 62500864;
                        else lineChart.MaxX                        = blocks;
                        lineChart.StepsX = lineChart.MaxX / 10f;
                        lineChart.StepsY = 4394.5f;
                        lineChart.MaxY   = lineChart.StepsY * 18;
                        break;
                    case 0x0050: // HD DVD
                    case 0x0051:
                    case 0x0052:
                    case 0x0053:
                    case 0x0058:
                    case 0x005A:
                        if(blocks      <= 7361599) lineChart.MaxX  = 7361599;
                        else if(blocks <= 16305407) lineChart.MaxX = 16305407;
                        else lineChart.MaxX                        = blocks;
                        lineChart.StepsX = lineChart.MaxX / 10f;
                        lineChart.StepsY = 4394.5f;
                        lineChart.MaxY   = lineChart.StepsY * 8;
                        break;
                    default:
                        lineChart.MaxX   = blocks;
                        lineChart.StepsX = lineChart.MaxX / 10f;
                        lineChart.StepsY = 625f;
                        lineChart.MaxY   = lineChart.StepsY;
                        break;
                }
            });
        }

        void WorkFinished()
        {
            Application.Instance.Invoke(() =>
            {
                btnStop.Visible      = false;
                btnScan.Visible      = true;
                btnCancel.Visible    = true;
                stkProgress.Visible  = false;
                lblTotalTime.Visible = true;
                lblAvgSpeed.Visible  = true;
                lblMaxSpeed.Visible  = true;
                lblMinSpeed.Visible  = true;
            });
        }

        void EndProgress()
        {
            Application.Instance.Invoke(() => { stkProgress1.Visible = false; });
        }

        void UpdateProgress(string text, long current, long maximum)
        {
            Application.Instance.Invoke(() =>
            {
                lblProgress.Text          = text;
                prgProgress.Indeterminate = false;
                prgProgress.MinValue      = 0;
                if(maximum > int.MaxValue)
                {
                    prgProgress.MaxValue = (int)(maximum / int.MaxValue);
                    prgProgress.Value    = (int)(current / int.MaxValue);
                }
                else
                {
                    prgProgress.MaxValue = (int)maximum;
                    prgProgress.Value    = (int)current;
                }
            });
        }

        void InitProgress()
        {
            Application.Instance.Invoke(() => { stkProgress1.Visible = true; });
        }

        void PulseProgress(string text)
        {
            Application.Instance.Invoke(() =>
            {
                lblProgress.Text          = text;
                prgProgress.Indeterminate = true;
            });
        }

        void StoppingErrorMessage(string text)
        {
            Application.Instance.Invoke(() =>
            {
                lblProgress.Text = text;
                MessageBox.Show(text, MessageBoxType.Error);
                WorkFinished();
            });
        }

        void UpdateStatus(string text)
        {
            Application.Instance.Invoke(() => { lblProgress.Text = text; });
        }

        void OnScanUnreadable(ulong sector)
        {
            Application.Instance.Invoke(() =>
            {
                localResults.Errored      += blocksToRead;
                lblUnreadableSectors.Text =  $"{localResults.Errored} sectors could not be read.";
                blockMap.ColoredSectors.Add(new ColoredBlock(sector, LightGreen));
            });
        }

        void OnScanTime(ulong sector, double duration)
        {
            Application.Instance.Invoke(() =>
            {
                if(duration < 3)
                {
                    localResults.A += blocksToRead;
                    blockMap.ColoredSectors.Add(new ColoredBlock(sector, LightGreen));
                }
                else if(duration >= 3 && duration < 10)
                {
                    localResults.B += blocksToRead;
                    blockMap.ColoredSectors.Add(new ColoredBlock(sector, Green));
                }
                else if(duration >= 10 && duration < 50)
                {
                    localResults.C += blocksToRead;
                    blockMap.ColoredSectors.Add(new ColoredBlock(sector, DarkGreen));
                }
                else if(duration >= 50 && duration < 150)
                {
                    localResults.D += blocksToRead;
                    blockMap.ColoredSectors.Add(new ColoredBlock(sector, Yellow));
                }
                else if(duration >= 150 && duration < 500)
                {
                    localResults.E += blocksToRead;
                    blockMap.ColoredSectors.Add(new ColoredBlock(sector, Orange));
                }
                else if(duration >= 500)
                {
                    localResults.F += blocksToRead;
                    blockMap.ColoredSectors.Add(new ColoredBlock(sector, Red));
                }

                lblA.Text = $"{localResults.A} sectors took less than 3 ms.";
                lblB.Text = $"{localResults.B} sectors took less than 10 ms but more than 3 ms.";
                lblC.Text = $"{localResults.C} sectors took less than 50 ms but more than 10 ms.";
                lblD.Text = $"{localResults.D} sectors took less than 150 ms but more than 50 ms.";
                lblE.Text = $"{localResults.E} sectors took less than 500 ms but more than 150 ms.";
                lblF.Text = $"{localResults.F} sectors took more than 500 ms.";
            });
        }

        #region XAML IDs
        Label       lblTotalTime;
        Label       lblAvgSpeed;
        Label       lblMaxSpeed;
        Label       lblMinSpeed;
        Label       lblA;
        Label       lblB;
        Label       lblC;
        Label       lblD;
        Label       lblE;
        Label       lblF;
        Label       lblUnreadableSectors;
        Button      btnCancel;
        Button      btnStop;
        Button      btnScan;
        StackLayout stkProgress;
        StackLayout stkProgress1;
        Label       lblProgress;
        ProgressBar prgProgress;
        StackLayout stkProgress2;
        Label       lblProgress2;
        ProgressBar prgProgress2;
        TabControl  tabResults;
        BlockMap    blockMap;
        LineChart   lineChart;
        #endregion
    }
}