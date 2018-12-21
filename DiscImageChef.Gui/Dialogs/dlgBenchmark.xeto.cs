// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : dlgBenchmark.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Benchmark dialog.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the benchmark dialog.
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
using System.Threading;
using DiscImageChef.Core;
using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Dialogs
{
    public class dlgBenchmark : Dialog
    {
        Dictionary<string, double> checksumTimes;
        int                        counter;
        BenchmarkResults           results;
        int                        step;

        public dlgBenchmark()
        {
            XamlReader.Load(this);
        }

        protected void OnBtnStart(object sender, EventArgs e)
        {
            checksumTimes                 =  new Dictionary<string, double>();
            Benchmark.UpdateProgressEvent += UpdateProgress;
            stkProgress.Visible           =  true;
            lblProgress.Text              =  "";
            btnClose.Enabled              =  false;
            btnStart.Enabled              =  false;
            nmuBufferSize.Enabled         =  false;
            nmuBlockSize.Enabled          =  false;

            Thread thread = new Thread(() =>
            {
                counter = step = (int)(nmuBufferSize.Value * 1024 * 1024 / nmuBlockSize.Value) % 3333;
                // TODO: Able to cancel!
                results = Benchmark.Do((int)(nmuBufferSize.Value * 1024 * 1024), (int)nmuBlockSize.Value);

                Application.Instance.Invoke(Finish);
            });

            thread.Start();
        }

        void Finish()
        {
            Benchmark.UpdateProgressEvent += UpdateProgress;
            StackLayout stkCalculationResults = new StackLayout();

            stkCalculationResults.Items.Add(new Label
            {
                Text =
                    $"Took {results.FillTime} seconds to fill buffer, {results.FillSpeed:F3} MiB/sec."
            });
            stkCalculationResults.Items.Add(new Label
            {
                Text =
                    $"Took {results.ReadTime} seconds to read buffer, {results.ReadSpeed:F3} MiB/sec."
            });
            stkCalculationResults.Items.Add(new Label
            {
                Text =
                    $"Took {results.EntropyTime} seconds to entropy buffer, {results.EntropySpeed:F3} MiB/sec."
            });

            foreach(KeyValuePair<string, BenchmarkEntry> entry in results.Entries)
            {
                checksumTimes.Add(entry.Key, entry.Value.TimeSpan);
                stkCalculationResults.Items.Add(new Label
                {
                    Text =
                        $"Took {entry.Value.TimeSpan} seconds to {entry.Key} buffer, {entry.Value.Speed:F3} MiB/sec."
                });
                ;
            }

            stkCalculationResults.Items.Add(new Label
            {
                Text =
                    $"Took {results.TotalTime} seconds to do all algorithms at the same time, {results.TotalSpeed:F3} MiB/sec."
            });
            stkCalculationResults.Items.Add(new Label
            {
                Text =
                    $"Took {results.SeparateTime} seconds to do all algorithms sequentially, {results.SeparateSpeed:F3} MiB/sec."
            });
            stkCalculationResults.Items.Add(new Label {Text = $"Max memory used is {results.MaxMemory} bytes"});
            stkCalculationResults.Items.Add(new Label {Text = $"Min memory used is {results.MinMemory} bytes"});

            Statistics.AddCommand("benchmark");

            stkCalculationResults.Items.Add(new StackLayoutItem(stkButtons, HorizontalAlignment.Right, true));
            stkCalculationResults.Visible = true;
            btnStart.Visible              = false;
            btnClose.Enabled              = true;
            Content                       = stkCalculationResults;
            ClientSize                    = new Size(-1, -1);
        }

        void UpdateProgress(string text, long current, long maximum)
        {
            if(counter < step)
            {
                counter++;
                return;
            }

            counter = 0;

            Application.Instance.Invoke(() =>
            {
                lblProgress.Text = text;

                if(maximum == 0)
                {
                    prgProgress.Indeterminate = true;
                    return;
                }

                if(prgProgress.Indeterminate) prgProgress.Indeterminate = false;

                if(maximum > int.MaxValue || current > int.MaxValue)
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

        protected void OnBtnClose(object sender, EventArgs e)
        {
            Close();
        }

        #region XAML controls
        NumericStepper nmuBufferSize;
        NumericStepper nmuBlockSize;
        StackLayout    stkProgress;
        Label          lblProgress;
        ProgressBar    prgProgress;
        Button         btnStart;
        Button         btnClose;
        StackLayout    stkPreCalculation;
        StackLayout    stkButtons;
        #endregion
    }
}