// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : frmImageChecksum.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Image checksum calculation window.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements creating checksums of a media image.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Threading;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core;
using Eto.Forms;
using Eto.Serialization.Xaml;
using Schemas;

namespace DiscImageChef.Gui.Forms
{
    public class frmImageChecksum : Form
    {
        // How many sectors to read at once
        const uint  SECTORS_TO_READ = 256;
        bool        cancel;
        IMediaImage inputFormat;

        public frmImageChecksum(IMediaImage inputFormat)
        {
            this.inputFormat = inputFormat;
            XamlReader.Load(this);
            cancel = false;
            try { chkChecksumTracks.Visible = inputFormat.Tracks?.Count > 0; }
            catch { chkChecksumTracks.Visible = false; }

            chkChecksumTracks.Checked = chkChecksumTracks.Visible;
            chkChecksumMedia.Visible  = chkChecksumTracks.Visible;
            #if NETSTANDARD2_0
            chkRipemd160.Visible = false;
            #endif
        }

        protected void OnBtnStart(object sender, EventArgs e)
        {
            chkAdler32.Enabled        = false;
            chkChecksumMedia.Enabled  = false;
            chkChecksumTracks.Enabled = false;
            chkCrc16.Enabled          = false;
            chkCrc32.Enabled          = false;
            chkCrc64.Enabled          = false;
            chkFletcher16.Enabled     = false;
            chkFletcher32.Enabled     = false;
            chkMd5.Enabled            = false;
            chkRipemd160.Enabled      = false;
            chkSha1.Enabled           = false;
            chkSha256.Enabled         = false;
            chkSha384.Enabled         = false;
            chkSha512.Enabled         = false;
            chkSpamsum.Enabled        = false;
            btnClose.Visible          = false;
            btnStart.Visible          = false;
            btnStop.Visible           = true;
            stkProgress.Visible       = true;
            lblProgress2.Visible      = false;

            new Thread(DoWork).Start();
        }

        void DoWork()
        {
            bool formatHasTracks;

            try { formatHasTracks = inputFormat.Tracks?.Count > 0; }
            catch { formatHasTracks = false; }

            // Setup progress bars
            Application.Instance.Invoke(() =>
            {
                stkProgress.Visible   = true;
                prgProgress.MaxValue  = 1;
                prgProgress2.MaxValue = (int)(inputFormat.Info.Sectors / SECTORS_TO_READ);

                if(formatHasTracks && chkChecksumTracks.Checked == true)
                    prgProgress.MaxValue += inputFormat.Tracks.Count;
                else
                {
                    prgProgress.MaxValue = 2;
                    prgProgress2.Visible = false;
                    lblProgress2.Visible = false;
                }
            });

            EnableChecksum enabledChecksums = new EnableChecksum();

            if(chkAdler32.Checked == true) enabledChecksums |= EnableChecksum.Adler32;
            if(chkCrc16.Checked   == true) enabledChecksums |= EnableChecksum.Crc16;
            if(chkCrc32.Checked   == true) enabledChecksums |= EnableChecksum.Crc32;
            if(chkCrc64.Checked   == true) enabledChecksums |= EnableChecksum.Crc64;
            if(chkMd5.Checked     == true) enabledChecksums |= EnableChecksum.Md5;
            #if !NETSTANDARD2_0
            if(chkRipemd160.Checked == true) enabledChecksums |= EnableChecksum.Ripemd160;
            #endif
            if(chkSha1.Checked       == true) enabledChecksums |= EnableChecksum.Sha1;
            if(chkSha256.Checked     == true) enabledChecksums |= EnableChecksum.Sha256;
            if(chkSha384.Checked     == true) enabledChecksums |= EnableChecksum.Sha384;
            if(chkSha512.Checked     == true) enabledChecksums |= EnableChecksum.Sha512;
            if(chkSpamsum.Checked    == true) enabledChecksums |= EnableChecksum.SpamSum;
            if(chkFletcher16.Checked == true) enabledChecksums |= EnableChecksum.Fletcher16;
            if(chkFletcher32.Checked == true) enabledChecksums |= EnableChecksum.Fletcher32;

            Checksum mediaChecksum = null;

            TreeGridItemCollection trackHashes = new TreeGridItemCollection();
            TreeGridItemCollection mediaHashes = new TreeGridItemCollection();

            if(formatHasTracks)
                try
                {
                    Checksum trackChecksum = null;

                    if(chkChecksumMedia.Checked == true) mediaChecksum = new Checksum(enabledChecksums);

                    ulong previousTrackEnd = 0;

                    foreach(Track currentTrack in inputFormat.Tracks)
                    {
                        Application.Instance.Invoke(() =>
                        {
                            lblProgress.Text =
                                $"Hashing track {currentTrack.TrackSequence} of {inputFormat.Tracks.Count}";
                            prgProgress.Value++;
                        });

                        if(currentTrack.TrackStartSector - previousTrackEnd != 0 && chkChecksumMedia.Checked == true)
                            for(ulong i = previousTrackEnd + 1; i < currentTrack.TrackStartSector; i++)
                            {
                                ulong sector = i;
                                Application.Instance.Invoke(() =>
                                {
                                    prgProgress2.Value = (int)(sector / SECTORS_TO_READ);
                                    lblProgress2.Text  = $"Hashing track-less sector {sector}";
                                });

                                byte[] hiddenSector = inputFormat.ReadSector(i);

                                mediaChecksum?.Update(hiddenSector);
                            }

                        DicConsole.DebugWriteLine("Checksum command",
                                                  "Track {0} starts at sector {1} and ends at sector {2}",
                                                  currentTrack.TrackSequence, currentTrack.TrackStartSector,
                                                  currentTrack.TrackEndSector);

                        if(chkChecksumTracks.Checked == true) trackChecksum = new Checksum(enabledChecksums);

                        ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        ulong doneSectors = 0;

                        while(doneSectors < sectors)
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

                            byte[] sector;

                            if(sectors - doneSectors >= SECTORS_TO_READ)
                            {
                                sector = inputFormat.ReadSectors(doneSectors, SECTORS_TO_READ,
                                                                 currentTrack.TrackSequence);

                                ulong doneSectorsToInvoke = doneSectors;
                                Application.Instance.Invoke(() =>
                                {
                                    prgProgress2.Value = (int)(doneSectorsToInvoke / SECTORS_TO_READ);
                                    lblProgress2.Text =
                                        $"Hashings sectors {doneSectorsToInvoke} to {doneSectorsToInvoke + SECTORS_TO_READ} of track {currentTrack.TrackSequence}";
                                });

                                doneSectors += SECTORS_TO_READ;
                            }
                            else
                            {
                                sector = inputFormat.ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                 currentTrack.TrackSequence);

                                ulong doneSectorsToInvoke = doneSectors;
                                Application.Instance.Invoke(() =>
                                {
                                    prgProgress2.Value = (int)(doneSectorsToInvoke / SECTORS_TO_READ);
                                    lblProgress2.Text =
                                        $"Hashings sectors {doneSectorsToInvoke} to {doneSectorsToInvoke + (sectors - doneSectorsToInvoke)} of track {currentTrack.TrackSequence}";
                                });

                                doneSectors += sectors - doneSectors;
                            }

                            if(chkChecksumMedia.Checked == true) mediaChecksum?.Update(sector);

                            if(chkChecksumTracks.Checked == true) trackChecksum?.Update(sector);
                        }

                        if(chkChecksumTracks.Checked == true)
                            if(trackChecksum != null)
                                foreach(ChecksumType chk in trackChecksum.End())
                                    trackHashes.Add(new TreeGridItem
                                    {
                                        Values = new object[]
                                        {
                                            currentTrack.TrackSequence, chk.type, chk.Value
                                        }
                                    });

                        previousTrackEnd = currentTrack.TrackEndSector;
                    }

                    if(inputFormat.Info.Sectors - previousTrackEnd != 0 && chkChecksumMedia.Checked == true)
                        for(ulong i = previousTrackEnd + 1; i < inputFormat.Info.Sectors; i++)
                        {
                            ulong sector = i;
                            Application.Instance.Invoke(() =>
                            {
                                prgProgress2.Value = (int)(sector / SECTORS_TO_READ);
                                lblProgress2.Text  = $"Hashing track-less sector {sector}";
                            });

                            byte[] hiddenSector = inputFormat.ReadSector(i);
                            mediaChecksum?.Update(hiddenSector);
                        }

                    if(chkChecksumMedia.Checked == true)
                        if(mediaChecksum != null)
                            foreach(ChecksumType chk in mediaChecksum.End())
                                mediaHashes.Add(new TreeGridItem {Values = new object[] {chk.type, chk.Value}});
                }
                catch(Exception ex)
                {
                    DicConsole.DebugWriteLine("Could not get tracks because {0}", ex.Message);
                    DicConsole.WriteLine("Unable to get separate tracks, not checksumming them");
                }
            else
            {
                Application.Instance.Invoke(() => { stkProgress1.Visible = false; });
                mediaChecksum = new Checksum(enabledChecksums);

                ulong doneSectors = 0;

                while(doneSectors < inputFormat.Info.Sectors)
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

                    byte[] sector;

                    if(inputFormat.Info.Sectors - doneSectors >= SECTORS_TO_READ)
                    {
                        sector = inputFormat.ReadSectors(doneSectors, SECTORS_TO_READ);

                        ulong doneSectorsToInvoke = doneSectors;
                        Application.Instance.Invoke(() =>
                        {
                            prgProgress2.Value = (int)(doneSectorsToInvoke / SECTORS_TO_READ);
                            lblProgress2.Text =
                                $"Hashings sectors {doneSectorsToInvoke} to {doneSectorsToInvoke + SECTORS_TO_READ}";
                        });
                        doneSectors += SECTORS_TO_READ;
                    }
                    else
                    {
                        sector = inputFormat.ReadSectors(doneSectors, (uint)(inputFormat.Info.Sectors - doneSectors));
                        ulong doneSectorsToInvoke = doneSectors;
                        Application.Instance.Invoke(() =>
                        {
                            prgProgress2.Value = (int)(doneSectorsToInvoke / SECTORS_TO_READ);
                            lblProgress2.Text =
                                $"Hashings sectors {doneSectorsToInvoke} to {doneSectorsToInvoke + (inputFormat.Info.Sectors - doneSectorsToInvoke)}";
                        });
                        doneSectors += inputFormat.Info.Sectors - doneSectors;
                    }

                    mediaChecksum.Update(sector);
                }

                foreach(ChecksumType chk in mediaChecksum.End())
                    mediaHashes.Add(new TreeGridItem {Values = new object[] {chk.type, chk.Value}});
            }

            if(chkChecksumTracks.Checked == true)
                Application.Instance.Invoke(() =>
                {
                    grpTrackChecksums.Text    = "Track checksums:";
                    stkTrackChecksums.Visible = true;

                    treeTrackChecksums.Columns.Add(new GridColumn
                    {
                        HeaderText = "Track", DataCell = new TextBoxCell(0)
                    });
                    treeTrackChecksums.Columns.Add(new GridColumn
                    {
                        HeaderText = "Algorithm", DataCell = new TextBoxCell(1)
                    });
                    treeTrackChecksums.Columns.Add(new GridColumn {HeaderText = "Hash", DataCell = new TextBoxCell(2)});

                    treeTrackChecksums.AllowMultipleSelection = false;
                    treeTrackChecksums.ShowHeader             = true;
                    treeTrackChecksums.DataStore              = trackHashes;
                });

            if(chkChecksumMedia.Checked == true)
                Application.Instance.Invoke(() =>
                {
                    grpMediaChecksums.Text    = "Media checksums:";
                    stkMediaChecksums.Visible = true;

                    treeMediaChecksums.Columns.Add(new GridColumn
                    {
                        HeaderText = "Algorithm", DataCell = new TextBoxCell(0)
                    });
                    treeMediaChecksums.Columns.Add(new GridColumn {HeaderText = "Hash", DataCell = new TextBoxCell(1)});

                    treeMediaChecksums.AllowMultipleSelection = false;
                    treeMediaChecksums.ShowHeader             = true;
                    treeMediaChecksums.DataStore              = mediaHashes;
                });

            Statistics.AddCommand("checksum");

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
        CheckBox     chkChecksumMedia;
        CheckBox     chkChecksumTracks;
        CheckBox     chkAdler32;
        CheckBox     chkCrc16;
        CheckBox     chkCrc32;
        CheckBox     chkCrc64;
        CheckBox     chkFletcher16;
        CheckBox     chkFletcher32;
        CheckBox     chkMd5;
        CheckBox     chkRipemd160;
        CheckBox     chkSha1;
        CheckBox     chkSha256;
        CheckBox     chkSha384;
        CheckBox     chkSha512;
        CheckBox     chkSpamsum;
        StackLayout  stkResults;
        StackLayout  stkTrackChecksums;
        GroupBox     grpTrackChecksums;
        TreeGridView treeTrackChecksums;
        StackLayout  stkMediaChecksums;
        GroupBox     grpMediaChecksums;
        TreeGridView treeMediaChecksums;
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