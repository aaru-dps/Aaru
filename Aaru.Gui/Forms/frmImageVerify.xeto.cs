// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : frmImageVerify.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Image verification window.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements verifying media image.
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
//     along with this program.  If not, see ;http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace Aaru.Gui.Forms
{
    public class frmImageVerify : Form
    {
        bool        cancel;
        IMediaImage inputFormat;

        public frmImageVerify(IMediaImage inputFormat)
        {
            this.inputFormat = inputFormat;
            XamlReader.Load(this);
            cancel = false;
        }

        protected void OnBtnStart(object sender, EventArgs e)
        {
            chkVerifyImage.Enabled   = false;
            chkVerifySectors.Enabled = false;
            btnClose.Visible         = false;
            btnStart.Visible         = false;
            btnStop.Visible          = true;
            stkProgress.Visible      = true;
            lblProgress2.Visible     = false;

            chkVerifySectors.Visible = inputFormat as IOpticalMediaImage      != null ||
                                       inputFormat as IVerifiableSectorsImage != null;

            // TODO: Do not offer the option to use this form if the image does not support any kind of verification

            new Thread(DoWork).Start();
        }

        void DoWork()
        {
            bool? correctDisc    = null;
            long  totalSectors   = 0;
            long  errorSectors   = 0;
            long  correctSectors = 0;
            long  unknownSectors = 0;
            bool  formatHasTracks;

            IOpticalMediaImage      inputOptical           = inputFormat as IOpticalMediaImage;
            IVerifiableSectorsImage verifiableSectorsImage = inputFormat as IVerifiableSectorsImage;

            try { formatHasTracks = inputOptical?.Tracks?.Count > 0; }
            catch { formatHasTracks = false; }

            // Setup progress bars
            Application.Instance.Invoke(() =>
            {
                stkProgress.Visible  = true;
                prgProgress.MaxValue = 0;

                if(chkVerifyImage.Checked == true || chkVerifySectors.Checked == true) prgProgress.MaxValue = 1;

                if(formatHasTracks && inputOptical != null) prgProgress.MaxValue += inputOptical.Tracks.Count;
                else
                {
                    if(chkVerifySectors.Checked == true)
                    {
                        prgProgress.MaxValue = 2;
                        prgProgress2.Visible = false;
                        lblProgress2.Visible = false;
                    }
                    else
                    {
                        prgProgress2.Visible = true;
                        lblProgress2.Visible = true;
                    }
                }

                prgProgress.MaxValue++;
            });

            if(chkVerifyImage.Checked == true)
            {
                if(!(inputFormat is IVerifiableImage verifiableImage))
                    Application.Instance.Invoke(() =>
                    {
                        lblImageResult.Visible = true;
                        lblImageResult.Text    = "Disc image does not support verification.";
                    });
                else
                {
                    Application.Instance.Invoke(() =>
                    {
                        lblProgress.Text = "Checking media image...";
                        if(chkVerifySectors.Checked == true) prgProgress.Value = 1;
                        else prgProgress.Indeterminate                         = true;

                        prgProgress2.Indeterminate = true;
                    });

                    DateTime startCheck      = DateTime.UtcNow;
                    bool?    discCheckStatus = verifiableImage.VerifyMediaImage();
                    DateTime endCheck        = DateTime.UtcNow;

                    TimeSpan checkTime = endCheck - startCheck;

                    Application.Instance.Invoke(() =>
                    {
                        lblImageResult.Visible = true;
                        switch(discCheckStatus)
                        {
                            case true:
                                lblImageResult.Text = "Disc image checksums are correct";
                                break;
                            case false:
                                lblImageResult.Text = "Disc image checksums are incorrect";
                                break;
                            case null:
                                lblImageResult.Text = "Disc image does not contain checksums";
                                break;
                        }
                    });

                    correctDisc = discCheckStatus;
                    DicConsole.VerboseWriteLine("Checking disc image checksums took {0} seconds",
                                                checkTime.TotalSeconds);
                }
            }

            if(chkVerifySectors.Checked == true)
            {
                DateTime    startCheck  = DateTime.Now;
                DateTime    endCheck    = startCheck;
                List<ulong> failingLbas = new List<ulong>();
                List<ulong> unknownLbas = new List<ulong>();

                Application.Instance.Invoke(() =>
                {
                    lblProgress2.Visible       = true;
                    prgProgress2.Indeterminate = false;
                    prgProgress2.MaxValue      = (int)(inputFormat.Info.Sectors / 512);
                    btnStop.Enabled            = true;
                });

                if(formatHasTracks)
                {
                    List<Track> inputTracks      = inputOptical.Tracks;
                    ulong       currentSectorAll = 0;

                    startCheck = DateTime.UtcNow;
                    foreach(Track currentTrack in inputOptical.Tracks)
                    {
                        Application.Instance.Invoke(() =>
                        {
                            lblProgress.Text =
                                $"Verifying track {currentTrack.TrackSequence} of {inputOptical?.Tracks.Count}";
                            prgProgress.Value++;
                        });

                        ulong remainingSectors = currentTrack.TrackEndSector - currentTrack.TrackStartSector;
                        ulong currentSector    = 0;

                        while(remainingSectors > 0)
                        {
                            if(cancel)
                            {
                                Application.Instance.Invoke(() =>
                                {
                                    btnClose.Visible = true;
                                    btnStart.Visible = false;
                                    btnStop.Visible  = false;
                                });
                                return;
                            }

                            ulong all = currentSectorAll;
                            Application.Instance.Invoke(() =>
                            {
                                prgProgress2.Value = (int)(all / 512);
                                lblProgress2.Text =
                                    $"Checking sector {all} of {inputFormat.Info.Sectors}, on track {currentTrack.TrackSequence}";
                            });

                            List<ulong> tempfailingLbas;
                            List<ulong> tempunknownLbas;

                            if(remainingSectors < 512)
                                inputOptical.VerifySectors(currentSector, (uint)remainingSectors,
                                                           currentTrack.TrackSequence, out tempfailingLbas,
                                                           out tempunknownLbas);
                            else
                                inputOptical.VerifySectors(currentSector, 512, currentTrack.TrackSequence,
                                                           out tempfailingLbas, out tempunknownLbas);

                            failingLbas.AddRange(tempfailingLbas);

                            unknownLbas.AddRange(tempunknownLbas);

                            if(remainingSectors < 512)
                            {
                                currentSector    += remainingSectors;
                                currentSectorAll += remainingSectors;
                                remainingSectors =  0;
                            }
                            else
                            {
                                currentSector    += 512;
                                currentSectorAll += 512;
                                remainingSectors -= 512;
                            }
                        }
                    }

                    endCheck = DateTime.UtcNow;
                }
                else if(!(verifiableSectorsImage is null))
                {
                    ulong remainingSectors = inputFormat.Info.Sectors;
                    ulong currentSector    = 0;

                    startCheck = DateTime.UtcNow;
                    while(remainingSectors > 0)
                    {
                        if(cancel)
                        {
                            Application.Instance.Invoke(() =>
                            {
                                btnClose.Visible = true;
                                btnStart.Visible = false;
                                btnStop.Visible  = false;
                            });
                            return;
                        }

                        ulong sector = currentSector;
                        Application.Instance.Invoke(() =>
                        {
                            prgProgress2.Value = (int)(sector / 512);
                            lblProgress2.Text  = $"Checking sector {sector} of {inputFormat.Info.Sectors}";
                        });

                        List<ulong> tempfailingLbas;
                        List<ulong> tempunknownLbas;

                        if(remainingSectors < 512)
                            verifiableSectorsImage.VerifySectors(currentSector, (uint)remainingSectors,
                                                                 out tempfailingLbas, out tempunknownLbas);
                        else
                            verifiableSectorsImage.VerifySectors(currentSector, 512, out tempfailingLbas,
                                                                 out tempunknownLbas);

                        failingLbas.AddRange(tempfailingLbas);

                        unknownLbas.AddRange(tempunknownLbas);

                        if(remainingSectors < 512)
                        {
                            currentSector    += remainingSectors;
                            remainingSectors =  0;
                        }
                        else
                        {
                            currentSector    += 512;
                            remainingSectors -= 512;
                        }
                    }

                    endCheck = DateTime.UtcNow;
                }

                TimeSpan checkTime = endCheck - startCheck;

                DicConsole.VerboseWriteLine("Checking sector checksums took {0} seconds", checkTime.TotalSeconds);

                Application.Instance.Invoke(() =>
                {
                    if(failingLbas.Count > 0)
                    {
                        if(failingLbas.Count == (int)inputFormat.Info.Sectors)
                        {
                            lblSectorsErrorsAll.Visible = true;
                            lblSectorsErrorsAll.Text    = "All sectors contain errors";
                        }
                        else
                        {
                            grpSectorErrors.Text    = "LBAs with error:";
                            grpSectorErrors.Visible = true;

                            TreeGridItemCollection errorList = new TreeGridItemCollection();

                            treeSectorErrors.Columns.Add(new GridColumn
                            {
                                HeaderText = "LBA", DataCell = new TextBoxCell(0)
                            });

                            treeSectorErrors.AllowMultipleSelection = false;
                            treeSectorErrors.ShowHeader             = false;
                            treeSectorErrors.DataStore              = errorList;

                            foreach(ulong t in failingLbas) errorList.Add(new TreeGridItem {Values = new object[] {t}});
                        }
                    }

                    if(unknownLbas.Count > 0)
                    {
                        if(unknownLbas.Count == (int)inputFormat.Info.Sectors)
                        {
                            lblSectorsErrorsAll.Visible = true;
                            lblSectorsErrorsAll.Text    = "All sectors contain errors";
                        }
                        else
                        {
                            grpSectorsUnknowns.Text    = "LBAs with error:";
                            grpSectorsUnknowns.Visible = true;

                            TreeGridItemCollection unknownList = new TreeGridItemCollection();

                            treeSectorsUnknowns.Columns.Add(new GridColumn
                            {
                                HeaderText = "LBA", DataCell = new TextBoxCell(0)
                            });

                            treeSectorsUnknowns.AllowMultipleSelection = false;
                            treeSectorsUnknowns.ShowHeader             = false;
                            treeSectorsUnknowns.DataStore              = unknownList;

                            foreach(ulong t in unknownLbas)
                                unknownList.Add(new TreeGridItem {Values = new object[] {t}});
                        }
                    }

                    grpSectorSummary.Visible    = true;
                    lblTotalSectors.Text        = $"Total sectors........... {inputFormat.Info.Sectors}";
                    lblTotalSectorErrors.Text   = $"Total errors............ {failingLbas.Count}";
                    lblTotalSectorUnknowns.Text = $"Total unknowns.......... {unknownLbas.Count}";
                    lblTotalSectorErrorsUnknowns.Text =
                        $"Total errors+unknowns... {failingLbas.Count + unknownLbas.Count}";
                });

                totalSectors   = (long)inputFormat.Info.Sectors;
                errorSectors   = failingLbas.Count;
                unknownSectors = unknownLbas.Count;
                correctSectors = totalSectors - errorSectors - unknownSectors;
            }

            Statistics.AddCommand("verify");

            Application.Instance.Invoke(() =>
            {
                stkOptions.Visible  = false;
                stkResults.Visible  = true;
                stkProgress.Visible = false;
                btnStart.Visible    = false;
                btnStop.Visible     = false;
                btnClose.Visible    = true;
            });
        }

        protected void OnBtnClose(object sender, EventArgs e)
        {
            Close();
        }

        protected void OnBtnStop(object sender, EventArgs e)
        {
            cancel          = true;
            btnStop.Enabled = false;
        }

        #region XAML IDs
        StackLayout  stkOptions;
        CheckBox     chkVerifyImage;
        CheckBox     chkVerifySectors;
        StackLayout  stkResults;
        StackLayout  stkSectorResults;
        StackLayout  stkSectorErrors;
        GroupBox     grpSectorErrors;
        TreeGridView treeSectorErrors;
        StackLayout  stkSectorUnknowns;
        GroupBox     grpSectorsUnknowns;
        TreeGridView treeSectorsUnknowns;
        Label        lblImageResult;
        Label        lblSectorsErrorsAll;
        Label        lblSectorsUnknownAll;
        GroupBox     grpSectorSummary;
        Label        lblTotalSectors;
        Label        lblTotalSectorErrors;
        Label        lblTotalSectorUnknowns;
        Label        lblTotalSectorErrorsUnknowns;
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