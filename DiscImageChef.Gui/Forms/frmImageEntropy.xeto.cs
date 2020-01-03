// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : frmImageEntropy.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Image entropy calculation window.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements calculating media image entropy.
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
using System.Threading;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Core;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Forms
{
    public class frmImageEntropy : Form
    {
        EntropyResults   entropy;
        IMediaImage      inputFormat;
        EntropyResults[] tracksEntropy;

        public frmImageEntropy(IMediaImage inputFormat)
        {
            this.inputFormat = inputFormat;
            XamlReader.Load(this);

            IOpticalMediaImage inputOptical = inputFormat as IOpticalMediaImage;

            if(inputOptical?.Tracks != null && inputOptical?.Tracks.Count > 0)
            {
                chkSeparatedTracks.Visible = true;
                chkWholeDisc.Visible       = true;
            }
            else
            {
                chkSeparatedTracks.Checked = false;
                chkWholeDisc.Checked       = true;
            }
        }

        protected void OnBtnStart(object sender, EventArgs e)
        {
            Entropy entropyCalculator = new Entropy(false, false, inputFormat);
            entropyCalculator.InitProgressEvent    += InitProgress;
            entropyCalculator.InitProgress2Event   += InitProgress2;
            entropyCalculator.UpdateProgressEvent  += UpdateProgress;
            entropyCalculator.UpdateProgress2Event += UpdateProgress2;
            entropyCalculator.EndProgressEvent     += EndProgress;
            entropyCalculator.EndProgress2Event    += EndProgress2;
            chkDuplicatedSectors.Enabled           =  false;
            chkSeparatedTracks.Enabled             =  false;
            chkWholeDisc.Enabled                   =  false;
            btnClose.Visible                       =  false;
            btnStart.Visible                       =  false;
            btnStop.Visible                        =  false;
            stkProgress.Visible                    =  true;

            Thread thread = new Thread(() =>
            {
                if(chkSeparatedTracks.Checked == true)
                {
                    tracksEntropy = entropyCalculator.CalculateTracksEntropy(chkDuplicatedSectors.Checked == true);
                    foreach(EntropyResults trackEntropy in tracksEntropy)
                    {
                        DicConsole.WriteLine("Entropy for track {0} is {1:F4}.", trackEntropy.Track,
                                             trackEntropy.Entropy);
                        if(trackEntropy.UniqueSectors != null)
                            DicConsole.WriteLine("Track {0} has {1} unique sectors ({2:P3})", trackEntropy.Track,
                                                 trackEntropy.UniqueSectors,
                                                 (double)trackEntropy.UniqueSectors / (double)trackEntropy.Sectors);
                    }
                }

                if(chkWholeDisc.Checked != true) return;

                entropy = entropyCalculator.CalculateMediaEntropy(chkDuplicatedSectors.Checked == true);

                Application.Instance.Invoke(Finish);
            });

            Statistics.AddCommand("entropy");

            thread.Start();
        }

        void Finish()
        {
            stkOptions.Visible  = false;
            btnClose.Visible    = true;
            stkProgress.Visible = false;
            stkResults.Visible  = true;

            if(chkSeparatedTracks.Checked == true)
            {
                TreeGridItemCollection entropyList = new TreeGridItemCollection();

                treeTrackEntropy.Columns.Add(new GridColumn {HeaderText = "Track", DataCell   = new TextBoxCell(0)});
                treeTrackEntropy.Columns.Add(new GridColumn {HeaderText = "Entropy", DataCell = new TextBoxCell(1)});
                if(chkDuplicatedSectors.Checked == true)
                    treeTrackEntropy.Columns.Add(new GridColumn
                    {
                        HeaderText = "Unique sectors", DataCell = new TextBoxCell(2)
                    });

                treeTrackEntropy.AllowMultipleSelection = false;
                treeTrackEntropy.ShowHeader             = true;
                treeTrackEntropy.DataStore              = entropyList;

                foreach(EntropyResults trackEntropy in tracksEntropy)
                    entropyList.Add(new TreeGridItem
                    {
                        Values = new object[]
                        {
                            trackEntropy.Track, trackEntropy.Entropy,
                            $"{trackEntropy.UniqueSectors} ({(double)trackEntropy.UniqueSectors / (double)trackEntropy.Sectors:P3})"
                        }
                    });

                grpTrackEntropy.Visible = true;
            }

            if(chkWholeDisc.Checked != true) return;

            lblMediaEntropy.Text    = $"Entropy for disk is {entropy.Entropy:F4}.";
            lblMediaEntropy.Visible = true;

            if(entropy.UniqueSectors == null) return;

            lblMediaUniqueSectors.Text =
                $"Disk has {entropy.UniqueSectors} unique sectors ({(double)entropy.UniqueSectors / (double)entropy.Sectors:P3})";
            lblMediaUniqueSectors.Visible = true;
        }

        protected void OnBtnClose(object sender, EventArgs e)
        {
            Close();
        }

        protected void OnBtnStop(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void InitProgress()
        {
            stkProgress1.Visible = true;
        }

        void EndProgress()
        {
            stkProgress1.Visible = false;
        }

        void InitProgress2()
        {
            stkProgress2.Visible = true;
        }

        void EndProgress2()
        {
            stkProgress2.Visible = false;
        }

        void UpdateProgress(string text, long current, long maximum)
        {
            UpdateProgress(text, current, maximum, lblProgress, prgProgress);
        }

        void UpdateProgress2(string text, long current, long maximum)
        {
            UpdateProgress(text, current, maximum, lblProgress2, prgProgress2);
        }

        void UpdateProgress(string text, long current, long maximum, Label label, ProgressBar progressBar)
        {
            Application.Instance.Invoke(() =>
            {
                label.Text = text;

                if(maximum == 0)
                {
                    progressBar.Indeterminate = true;
                    return;
                }

                if(progressBar.Indeterminate) progressBar.Indeterminate = false;

                if(maximum > int.MaxValue || current > int.MaxValue)
                {
                    progressBar.MaxValue = (int)(maximum / int.MaxValue);
                    progressBar.Value    = (int)(current / int.MaxValue);
                }
                else
                {
                    progressBar.MaxValue = (int)maximum;
                    progressBar.Value    = (int)current;
                }
            });
        }

        #region XAML IDs
        StackLayout  stkOptions;
        CheckBox     chkDuplicatedSectors;
        CheckBox     chkSeparatedTracks;
        CheckBox     chkWholeDisc;
        StackLayout  stkResults;
        Label        lblMediaEntropy;
        Label        lblMediaUniqueSectors;
        GroupBox     grpTrackEntropy;
        TreeGridView treeTrackEntropy;
        StackLayout  stkProgress;
        StackLayout  stkProgress1;
        Label        lblProgress;
        ProgressBar  prgProgress;
        StackLayout  stkProgress2;
        Label        lblProgress2;
        ProgressBar  prgProgress2;
        Button       btnStart;
        Button       btnClose;
        Button       btnStop;
        #endregion
    }
}