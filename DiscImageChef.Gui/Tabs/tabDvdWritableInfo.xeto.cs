// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : tabDvdWritableInfo.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Media information.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the Writable DVDs media information.
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
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.Decoders.DVD;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Tabs
{
    public class tabDvdWritableInfo : TabPage
    {
        byte[] dvdLastBorderOutRmd;
        byte[] dvdPlusAdip;
        byte[] dvdPlusDcb;
        byte[] dvdPreRecordedInfo;
        byte[] dvdRamCartridgeStatus;
        byte[] dvdRamDds;
        byte[] dvdRamSpareArea;
        byte[] dvdrDlJumpIntervalSize;
        byte[] dvdrDlManualLayerJumpStartLba;
        byte[] dvdrDlMiddleZoneStart;
        byte[] dvdrDlRemapAnchorPoint;
        byte[] dvdrLayerCapacity;
        byte[] dvdrMediaIdentifier;
        byte[] dvdrPhysicalInformation;
        byte[] hddvdrLastRmd;
        byte[] hddvdrMediumStatus;

        public tabDvdWritableInfo()
        {
            XamlReader.Load(this);
        }

        internal void LoadData(MediaType mediaType, byte[] dds, byte[] cartridgeStatus,
                               byte[]    spareArea,
                               byte[]    lastBorderOutRmd,    byte[] preRecordedInfo, byte[] mediaIdentifier,
                               byte[]    physicalInformation, byte[] mediumStatus,    byte[] hdLastRmd,
                               byte[]    layerCapacity,
                               byte[]    middleZoneStart,  byte[] jumpIntervalSize, byte[] manualLayerJumpStartLba,
                               byte[]    remapAnchorPoint, byte[] adip,             byte[] dcb)
        {
            dvdRamDds                     = dds;
            dvdRamCartridgeStatus         = cartridgeStatus;
            dvdRamSpareArea               = spareArea;
            dvdLastBorderOutRmd           = lastBorderOutRmd;
            dvdPreRecordedInfo            = preRecordedInfo;
            dvdrMediaIdentifier           = mediaIdentifier;
            dvdrPhysicalInformation       = physicalInformation;
            hddvdrMediumStatus            = mediumStatus;
            hddvdrLastRmd                 = hdLastRmd;
            dvdrLayerCapacity             = layerCapacity;
            dvdrDlMiddleZoneStart         = middleZoneStart;
            dvdrDlJumpIntervalSize        = jumpIntervalSize;
            dvdrDlManualLayerJumpStartLba = manualLayerJumpStartLba;
            dvdrDlRemapAnchorPoint        = remapAnchorPoint;
            dvdPlusAdip                   = adip;
            dvdPlusDcb                    = dcb;
            switch(mediaType)
            {
                case MediaType.DVDR:
                    Text = "DVD-R";
                    break;
                case MediaType.DVDRW:
                    Text = "DVD-RW";
                    break;
                case MediaType.DVDPR:
                    Text = "DVD+R";
                    break;
                case MediaType.DVDPRW:
                    Text = "DVD+RW";
                    break;
                case MediaType.DVDPRWDL:
                    Text = "DVD+RW DL";
                    break;
                case MediaType.DVDRDL:
                    Text = "DVD-R DL";
                    break;
                case MediaType.DVDPRDL:
                    Text = "DVD+R DL";
                    break;
                case MediaType.DVDRAM:
                    Text = "DVD-RAM";
                    break;
                case MediaType.DVDRWDL:
                    Text = "DVD-RW DL";
                    break;
                case MediaType.HDDVDRAM:
                    Text = "HD DVD-RAM";
                    break;
                case MediaType.HDDVDR:
                    Text = "HD DVD-R";
                    break;
                case MediaType.HDDVDRW:
                    Text = "HD DVD-RW";
                    break;
                case MediaType.HDDVDRDL:
                    Text = "HD DVD-R DL";
                    break;
                case MediaType.HDDVDRWDL:
                    Text = "HD DVD-RW DL";
                    break;
            }

            if(dds != null)
            {
                grpDvdRamDds.Visible     = true;
                btnSaveDvdRamDds.Visible = true;
                txtDvdRamDds.Text        = DDS.Prettify(dds);
            }

            if(cartridgeStatus != null)
            {
                grpDvdRamCartridgeStatus.Visible     = true;
                btnSaveDvdRamCartridgeStatus.Visible = true;
                txtDvdRamCartridgeStatus.Text        = Cartridge.Prettify(cartridgeStatus);
            }

            if(spareArea != null)
            {
                grpDvdRamSpareAreaInformation.Visible     = true;
                btnSaveDvdRamSpareAreaInformation.Visible = true;
                txtDvdRamSpareAreaInformation.Text        = Spare.Prettify(spareArea);
            }

            btnSaveDvdRamDds.Visible                     = dds                     != null;
            btnSaveDvdRamCartridgeStatus.Visible         = cartridgeStatus         != null;
            btnSaveDvdRamSpareAreaInformation.Visible    = spareArea               != null;
            btnSaveLastBorderOutRmd.Visible              = lastBorderOutRmd        != null;
            btnSaveDvdPreRecordedInfo.Visible            = preRecordedInfo         != null;
            btnSaveDvdrMediaIdentifier.Visible           = mediaIdentifier         != null;
            btnSaveDvdrPhysicalInformation.Visible       = physicalInformation     != null;
            btnSaveHddvdrMediumStatus.Visible            = mediumStatus            != null;
            btnSaveHddvdrLastRmd.Visible                 = hdLastRmd               != null;
            btnSaveDvdrLayerCapacity.Visible             = layerCapacity           != null;
            btnSaveDvdrDlMiddleZoneStart.Visible         = middleZoneStart         != null;
            btnSaveDvdrDlJumpIntervalSize.Visible        = jumpIntervalSize        != null;
            btnSaveDvdrDlManualLayerJumpStartLba.Visible = manualLayerJumpStartLba != null;
            btnSaveDvdrDlRemapAnchorPoint.Visible        = remapAnchorPoint        != null;
            btnSaveDvdPlusAdip.Visible                   = adip                    != null;
            btnSaveDvdPlusDcb.Visible                    = dcb                     != null;

            Visible = grpDvdRamDds.Visible                  || grpDvdRamCartridgeStatus.Visible             ||
                      grpDvdRamSpareAreaInformation.Visible || btnSaveDvdRamDds.Visible                     ||
                      btnSaveDvdRamCartridgeStatus.Visible  || btnSaveDvdRamSpareAreaInformation.Visible    ||
                      btnSaveLastBorderOutRmd.Visible       || btnSaveDvdPreRecordedInfo.Visible            ||
                      btnSaveDvdrMediaIdentifier.Visible    || btnSaveDvdrPhysicalInformation.Visible       ||
                      btnSaveHddvdrMediumStatus.Visible     || btnSaveHddvdrLastRmd.Visible                 ||
                      btnSaveDvdrLayerCapacity.Visible      || btnSaveDvdrDlMiddleZoneStart.Visible         ||
                      btnSaveDvdrDlJumpIntervalSize.Visible || btnSaveDvdrDlManualLayerJumpStartLba.Visible ||
                      btnSaveDvdrDlRemapAnchorPoint.Visible || btnSaveDvdPlusAdip.Visible                   ||
                      btnSaveDvdPlusDcb.Visible;
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

        protected void OnBtnSaveDvdRamDdsClick(object sender, EventArgs e)
        {
            SaveElement(dvdRamDds);
        }

        protected void OnBtnSaveDvdRamCartridgeStatusClick(object sender, EventArgs e)
        {
            SaveElement(dvdRamCartridgeStatus);
        }

        protected void OnBtnSaveDvdRamSpareAreaInformationClick(object sender, EventArgs e)
        {
            SaveElement(dvdRamSpareArea);
        }

        protected void OnBtnSaveLastBorderOutRmdClick(object sender, EventArgs e)
        {
            SaveElement(dvdLastBorderOutRmd);
        }

        protected void OnBtnSaveDvdPreRecordedInfoClick(object sender, EventArgs e)
        {
            SaveElement(dvdPreRecordedInfo);
        }

        protected void OnBtnSaveDvdrMediaIdentifierClick(object sender, EventArgs e)
        {
            SaveElement(dvdrMediaIdentifier);
        }

        protected void OnBtnSaveDvdrPhysicalInformationClick(object sender, EventArgs e)
        {
            SaveElement(dvdrPhysicalInformation);
        }

        protected void OnBtnSaveHddvdrMediumStatusClick(object sender, EventArgs e)
        {
            SaveElement(hddvdrMediumStatus);
        }

        protected void OnBtnSaveHddvdrLastRmdClick(object sender, EventArgs e)
        {
            SaveElement(hddvdrLastRmd);
        }

        protected void OnBtnSaveDvdrLayerCapacityClick(object sender, EventArgs e)
        {
            SaveElement(dvdrLayerCapacity);
        }

        protected void OnBtnSaveDvdrDlMiddleZoneStartClick(object sender, EventArgs e)
        {
            SaveElement(dvdrDlMiddleZoneStart);
        }

        protected void OnBtnSaveDvdrDlJumpIntervalSizeClick(object sender, EventArgs e)
        {
            SaveElement(dvdrDlJumpIntervalSize);
        }

        protected void OnBtnSaveDvdrDlManualLayerJumpStartLbaClick(object sender, EventArgs e)
        {
            SaveElement(dvdrDlManualLayerJumpStartLba);
        }

        protected void OnBtnSaveDvdrDlRemapAnchorPointClick(object sender, EventArgs e)
        {
            SaveElement(dvdrDlRemapAnchorPoint);
        }

        protected void OnBtnSaveDvdPlusAdipClick(object sender, EventArgs e)
        {
            SaveElement(dvdPlusAdip);
        }

        protected void OnBtnSaveDvdPlusDcbClick(object sender, EventArgs e)
        {
            SaveElement(dvdPlusDcb);
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        GroupBox grpDvdRamDds;
        TextArea txtDvdRamDds;
        GroupBox grpDvdRamCartridgeStatus;
        TextArea txtDvdRamCartridgeStatus;
        GroupBox grpDvdRamSpareAreaInformation;
        TextArea txtDvdRamSpareAreaInformation;
        Button   btnSaveDvdRamDds;
        Button   btnSaveDvdRamCartridgeStatus;
        Button   btnSaveDvdRamSpareAreaInformation;
        Button   btnSaveLastBorderOutRmd;
        Button   btnSaveDvdPreRecordedInfo;
        Button   btnSaveDvdrMediaIdentifier;
        Button   btnSaveDvdrPhysicalInformation;
        Button   btnSaveHddvdrMediumStatus;
        Button   btnSaveHddvdrLastRmd;
        Button   btnSaveDvdrLayerCapacity;
        Button   btnSaveDvdrDlMiddleZoneStart;
        Button   btnSaveDvdrDlJumpIntervalSize;
        Button   btnSaveDvdrDlManualLayerJumpStartLba;
        Button   btnSaveDvdrDlRemapAnchorPoint;
        Button   btnSaveDvdPlusAdip;
        Button   btnSaveDvdPlusDcb;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}