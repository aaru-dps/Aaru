// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : pnlxeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SCSI media information panel.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the SCSI media information panel.
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
using System.IO;
using DiscImageChef.Decoders.Bluray;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Panels
{
    public class tabBlurayInfo : TabPage
    {
        byte[] BurstCuttingArea;
        byte[] CartridgeStatus;
        byte[] Dds;
        byte[] DiscInformation;
        byte[] Pac;
        byte[] PowResources;
        byte[] RawDfl;
        byte[] SpareAreaInformation;
        byte[] TrackResources;

        public tabBlurayInfo()
        {
            XamlReader.Load(this);
        }

        internal void LoadData(byte[] blurayDiscInformation, byte[] blurayBurstCuttingArea, byte[] blurayDds,
                               byte[] blurayCartridgeStatus, byte[] bluraySpareAreaInformation,
                               byte[] blurayPowResources,    byte[] blurayTrackResources, byte[] blurayRawDfl,
                               byte[] blurayPac)
        {
            DiscInformation      = blurayDiscInformation;
            BurstCuttingArea     = blurayBurstCuttingArea;
            Dds                  = blurayDds;
            CartridgeStatus      = blurayCartridgeStatus;
            SpareAreaInformation = bluraySpareAreaInformation;
            PowResources         = blurayPowResources;
            TrackResources       = blurayTrackResources;
            RawDfl               = blurayRawDfl;
            Pac                  = blurayPac;

            if(blurayDiscInformation != null)
            {
                grpBlurayDiscInformation.Visible     = true;
                btnSaveBlurayDiscInformation.Visible = true;
                txtBlurayDiscInformation.Text        = DI.Prettify(blurayDiscInformation);
            }

            if(blurayBurstCuttingArea != null)
            {
                grpBlurayBurstCuttingArea.Visible     = true;
                btnSaveBlurayBurstCuttingArea.Visible = true;
                txtBlurayBurstCuttingArea.Text        = BCA.Prettify(blurayBurstCuttingArea);
            }

            if(blurayDds != null)
            {
                grpBlurayDds.Visible     = true;
                btnSaveBlurayDds.Visible = true;
                txtBlurayDds.Text        = DDS.Prettify(blurayDds);
            }

            if(blurayCartridgeStatus != null)
            {
                grpBlurayCartridgeStatus.Visible     = true;
                btnSaveBlurayCartridgeStatus.Visible = true;
                txtBlurayCartridgeStatus.Text        = Cartridge.Prettify(blurayCartridgeStatus);
            }

            if(bluraySpareAreaInformation != null)
            {
                grpBluraySpareAreaInformation.Visible     = true;
                btnSaveBluraySpareAreaInformation.Visible = true;
                txtBluraySpareAreaInformation.Text        = Spare.Prettify(bluraySpareAreaInformation);
            }

            if(blurayPowResources != null)
            {
                grpBlurayPowResources.Visible     = true;
                btnSaveBlurayPowResources.Visible = true;
                txtBlurayPowResources.Text        = Decoders.SCSI.MMC.DiscInformation.Prettify(blurayPowResources);
            }

            if(blurayTrackResources != null)
            {
                grpBlurayTrackResources.Visible     = true;
                btnSaveBlurayTrackResources.Visible = true;
                txtBlurayTrackResources.Text        = Decoders.SCSI.MMC.DiscInformation.Prettify(blurayTrackResources);
            }

            btnSaveBlurayRawDfl.Visible = blurayRawDfl != null;
            btnSaveBlurayPac.Visible    = blurayPac    != null;

            Visible = grpBlurayDiscInformation.Visible || grpBlurayBurstCuttingArea.Visible ||
                      grpBlurayDds.Visible             ||
                      grpBlurayCartridgeStatus.Visible || grpBluraySpareAreaInformation.Visible ||
                      grpBlurayPowResources.Visible    || grpBlurayTrackResources.Visible       ||
                      btnSaveBlurayRawDfl.Visible      ||
                      btnSaveBlurayPac.Visible;
        }

        void SaveElement(byte[] data)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(data, 0, data.Length);

            saveFs.Close();
        }

        protected void OnBtnSaveBlurayDiscInformationClick(object sender, EventArgs e)
        {
            SaveElement(DiscInformation);
        }

        protected void OnBtnSaveBlurayBurstCuttingAreaClick(object sender, EventArgs e)
        {
            SaveElement(BurstCuttingArea);
        }

        protected void OnBtnSaveBlurayDdsClick(object sender, EventArgs e)
        {
            SaveElement(Dds);
        }

        protected void OnBtnSaveBlurayCartridgeStatusClick(object sender, EventArgs e)
        {
            SaveElement(CartridgeStatus);
        }

        protected void OnBtnSaveBluraySpareAreaInformationClick(object sender, EventArgs e)
        {
            SaveElement(SpareAreaInformation);
        }

        protected void OnBtnSaveBlurayPowResourcesClick(object sender, EventArgs e)
        {
            SaveElement(PowResources);
        }

        protected void OnBtnSaveBlurayTrackResourcesClick(object sender, EventArgs e)
        {
            SaveElement(TrackResources);
        }

        protected void OnBtnSaveBlurayRawDflClick(object sender, EventArgs e)
        {
            SaveElement(RawDfl);
        }

        protected void OnBtnSaveBlurayPacClick(object sender, EventArgs e)
        {
            SaveElement(Pac);
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        GroupBox grpBlurayDiscInformation;
        TextArea txtBlurayDiscInformation;
        GroupBox grpBlurayBurstCuttingArea;
        TextArea txtBlurayBurstCuttingArea;
        GroupBox grpBlurayDds;
        TextArea txtBlurayDds;
        GroupBox grpBlurayCartridgeStatus;
        TextArea txtBlurayCartridgeStatus;
        GroupBox grpBluraySpareAreaInformation;
        TextArea txtBluraySpareAreaInformation;
        GroupBox grpBlurayPowResources;
        TextArea txtBlurayPowResources;
        GroupBox grpBlurayTrackResources;
        TextArea txtBlurayTrackResources;
        Button   btnSaveBlurayDiscInformation;
        Button   btnSaveBlurayBurstCuttingArea;
        Button   btnSaveBlurayDds;
        Button   btnSaveBlurayCartridgeStatus;
        Button   btnSaveBluraySpareAreaInformation;
        Button   btnSaveBlurayPowResources;
        Button   btnSaveBlurayTrackResources;
        Button   btnSaveBlurayRawDfl;
        Button   btnSaveBlurayPac;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}