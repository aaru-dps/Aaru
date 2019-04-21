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
using Eto.Forms;
using Eto.Serialization.Xaml;
using DeviceInfo = DiscImageChef.Core.Devices.Info.DeviceInfo;

namespace DiscImageChef.Gui.Forms
{
    public class frmMediaScan : Form
    {
        string devicePath;

        MediaScan scanner;

        public frmMediaScan(string devicePath, DeviceInfo deviceInfo, ScsiInfo scsiInfo = null)
        {
            MediaType mediaType;
            XamlReader.Load(this);

            this.devicePath = devicePath;
            btnStop.Visible = false;
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
            btnStop.Visible   = true;
            btnScan.Visible   = false;
            btnCancel.Visible = false;
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
                btnStop.Visible   = false;
                btnScan.Visible   = true;
                btnCancel.Visible = true;

                return;
            }

            Statistics.AddDevice(dev);

            scanner = new MediaScan(null, null, devicePath, dev);
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

            Application.Instance.Invoke(() =>
            {
                btnStop.Visible   = false;
                btnScan.Visible   = true;
                btnCancel.Visible = true;
            });
        }

        #region XAML IDs
        Label  lblTotalTime;
        Label  lblAvgSpeed;
        Label  lblMaxSpeed;
        Label  lblMinSpeed;
        Label  lblA;
        Label  lblB;
        Label  lblC;
        Label  lblD;
        Label  lblE;
        Label  lblF;
        Label  lblUnreadableSectors;
        Button btnCancel;
        Button btnStop;
        Button btnScan;
        #endregion
    }
}