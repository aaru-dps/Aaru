// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : pnlScsiInfo.xeto.cs
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text;
using Aaru.Gui.Controls;
using Aaru.Gui.Forms;
using Aaru.Gui.Tabs;
using Aaru.CommonTypes;
using Aaru.Core.Media.Info;
using Aaru.Decoders.SCSI.SSC;
using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace Aaru.Gui.Panels
{
    public class pnlScsiInfo : Panel
    {
        string   devicePath;
        ScsiInfo scsiInfo;

        public pnlScsiInfo(ScsiInfo scsiInfo, string devicePath)
        {
            XamlReader.Load(this);

            this.scsiInfo = scsiInfo;

            Stream logo =
                ResourceHandler.GetResourceStream($"Aaru.Gui.Assets.Logos.Media.{scsiInfo.MediaType}.svg");
            /*            if(logo != null)
                        {
                            svgMediaLogo.SvgStream = logo;
                            svgMediaLogo.Visible   = true;
                        }
                        else
                        {*/
            logo = ResourceHandler.GetResourceStream($"Aaru.Gui.Assets.Logos.Media.{scsiInfo.MediaType}.png");
            if(logo != null)
            {
                imgMediaLogo.Image   = new Bitmap(logo);
                imgMediaLogo.Visible = true;
            }
            //}

            txtType.Text = scsiInfo.MediaType.ToString();
            lblMediaSize.Text =
                $"Media has {scsiInfo.Blocks} blocks of {scsiInfo.BlockSize} bytes/each. (for a total of {scsiInfo.Blocks * scsiInfo.BlockSize} bytes)";
            lblMediaSize.Visible = scsiInfo.Blocks != 0 && scsiInfo.BlockSize != 0;

            if(scsiInfo.MediaSerialNumber != null)
            {
                stkMediaSerial.Visible = true;
                StringBuilder sbSerial = new StringBuilder();
                for(int i = 4; i < scsiInfo.MediaSerialNumber.Length; i++)
                    sbSerial.AppendFormat("{0:X2}", scsiInfo.MediaSerialNumber[i]);

                txtMediaSerial.Text = sbSerial.ToString();
            }

            btnSaveReadMediaSerial.Visible = this.scsiInfo.MediaSerialNumber != null;
            btnSaveReadCapacity.Visible    = this.scsiInfo.ReadCapacity      != null;
            btnSaveReadCapacity16.Visible  = this.scsiInfo.ReadCapacity16    != null;

            btnSaveGetConfiguration.Visible       = this.scsiInfo.MmcConfiguration       != null;
            btnSaveRecognizedFormatLayers.Visible = this.scsiInfo.RecognizedFormatLayers != null;
            btnSaveWriteProtectionStatus.Visible  = this.scsiInfo.WriteProtectionStatus  != null;
            tabMmc.Visible = btnSaveGetConfiguration.Visible || btnSaveRecognizedFormatLayers.Visible ||
                             btnSaveWriteProtectionStatus.Visible;

            if(this.scsiInfo.DensitySupportHeader.HasValue)
            {
                grpDensitySupport.Visible = true;
                txtDensitySupport.Text    = DensitySupport.PrettifyDensity(scsiInfo.DensitySupportHeader);
            }

            if(this.scsiInfo.MediaTypeSupportHeader.HasValue)
            {
                grpMediumSupport.Visible = true;
                txtMediumSupport.Text    = DensitySupport.PrettifyMediumType(scsiInfo.MediaTypeSupportHeader);
            }

            btnSaveDensitySupport.Visible = scsiInfo.DensitySupport   != null;
            btnSaveMediumSupport.Visible  = scsiInfo.MediaTypeSupport != null;
            tabSsc.Visible = grpDensitySupport.Visible || grpMediumSupport.Visible || btnSaveDensitySupport.Visible ||
                             btnSaveMediumSupport.Visible;

            tabCompactDiscInfo tabCompactDiscInfo = new tabCompactDiscInfo();
            tabCompactDiscInfo.LoadData(scsiInfo.Toc, scsiInfo.Atip, scsiInfo.CompactDiscInformation, scsiInfo.Session,
                                        scsiInfo.RawToc, this.scsiInfo.Pma, this.scsiInfo.CdTextLeadIn,
                                        this.scsiInfo.DecodedToc, this.scsiInfo.DecodedAtip,
                                        this.scsiInfo.DecodedSession, this.scsiInfo.FullToc,
                                        this.scsiInfo.DecodedCdTextLeadIn, this.scsiInfo.DecodedCompactDiscInformation,
                                        this.scsiInfo.Mcn, this.scsiInfo.Isrcs);
            tabInfos.Pages.Add(tabCompactDiscInfo);

            tabDvdInfo tabDvdInfo = new tabDvdInfo();
            tabDvdInfo.LoadData(scsiInfo.MediaType, scsiInfo.DvdPfi, scsiInfo.DvdDmi, scsiInfo.DvdCmi,
                                scsiInfo.HddvdCopyrightInformation, scsiInfo.DvdBca, scsiInfo.DvdAacs,
                                this.scsiInfo.DecodedPfi);
            tabInfos.Pages.Add(tabDvdInfo);

            tabXboxInfo tabXboxInfo = new tabXboxInfo();
            tabXboxInfo.LoadData(scsiInfo.XgdInfo, scsiInfo.DvdDmi, scsiInfo.XboxSecuritySector,
                                 scsiInfo.DecodedXboxSecuritySector);
            tabInfos.Pages.Add(tabXboxInfo);

            tabDvdWritableInfo tabDvdWritableInfo = new tabDvdWritableInfo();
            tabDvdWritableInfo.LoadData(scsiInfo.MediaType, scsiInfo.DvdRamDds, scsiInfo.DvdRamCartridgeStatus,
                                        scsiInfo.DvdRamSpareArea, scsiInfo.LastBorderOutRmd,
                                        scsiInfo.DvdPreRecordedInfo, scsiInfo.DvdrMediaIdentifier,
                                        scsiInfo.DvdrPhysicalInformation, scsiInfo.HddvdrMediumStatus,
                                        scsiInfo.HddvdrLastRmd, scsiInfo.DvdrLayerCapacity,
                                        scsiInfo.DvdrDlMiddleZoneStart, scsiInfo.DvdrDlJumpIntervalSize,
                                        scsiInfo.DvdrDlManualLayerJumpStartLba, scsiInfo.DvdrDlRemapAnchorPoint,
                                        scsiInfo.DvdPlusAdip, scsiInfo.DvdPlusDcb);
            tabInfos.Pages.Add(tabDvdWritableInfo);

            tabBlurayInfo tabBlurayInfo = new tabBlurayInfo();
            tabBlurayInfo.LoadData(scsiInfo.BlurayDiscInformation, scsiInfo.BlurayBurstCuttingArea, scsiInfo.BlurayDds,
                                   scsiInfo.BlurayCartridgeStatus, scsiInfo.BluraySpareAreaInformation,
                                   scsiInfo.BlurayPowResources, scsiInfo.BlurayTrackResources, scsiInfo.BlurayRawDfl,
                                   scsiInfo.BlurayPac);
            tabInfos.Pages.Add(tabBlurayInfo);

            this.devicePath = devicePath;
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

        protected void OnBtnSaveReadMediaSerialClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.MediaSerialNumber);
        }

        protected void OnBtnSaveReadCapacityClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.ReadCapacity);
        }

        protected void OnBtnSaveReadCapacity16Click(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.ReadCapacity16);
        }

        protected void OnBtnSaveGetConfigurationClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.MmcConfiguration);
        }

        protected void OnBtnSaveRecognizedFormatLayersClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.RecognizedFormatLayers);
        }

        protected void OnBtnSaveWriteProtectionStatusClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.WriteProtectionStatus);
        }

        protected void OnBtnSaveDensitySupportClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DensitySupport);
        }

        protected void OnBtnSaveMediumSupportClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.MediaTypeSupport);
        }

        protected void OnBtnSaveXboxSsClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.XboxSecuritySector);
        }

        protected void OnBtnDumpClick(object sender, EventArgs e)
        {
            if(scsiInfo.MediaType == MediaType.GDR || scsiInfo.MediaType == MediaType.GDROM)
            {
                MessageBox.Show("GD-ROM dump support is not yet implemented.", MessageBoxType.Error);
                return;
            }

            if((scsiInfo.MediaType == MediaType.XGD || scsiInfo.MediaType == MediaType.XGD2 ||
                scsiInfo.MediaType == MediaType.XGD3) &&
               scsiInfo.DeviceInfo.ScsiInquiry?.KreonPresent != true)
                MessageBox.Show("Dumping Xbox discs require a Kreon drive.", MessageBoxType.Error);

            frmDump dumpForm = new frmDump(devicePath, scsiInfo.DeviceInfo, scsiInfo);
            dumpForm.Show();
        }

        protected void OnBtnScanClick(object sender, EventArgs e)
        {
            if(scsiInfo.MediaType == MediaType.GDR || scsiInfo.MediaType == MediaType.GDROM)
            {
                MessageBox.Show("GD-ROM scan support is not yet implemented.", MessageBoxType.Error);
                return;
            }

            if(scsiInfo.MediaType == MediaType.XGD || scsiInfo.MediaType == MediaType.XGD2 ||
               scsiInfo.MediaType == MediaType.XGD3)
                MessageBox.Show("Scanning Xbox discs is not yet supported.", MessageBoxType.Error);

            frmMediaScan scanForm = new frmMediaScan(devicePath, scsiInfo.DeviceInfo, scsiInfo);
            scanForm.Show();
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        Label        lblMediaInfo;
        TabControl   tabInfos;
        TabPage      tabGeneral;
        Label        lblType;
        TextBox      txtType;
        Label        lblMediaSize;
        StackLayout  stkMediaSerial;
        Label        lblMediaSerial;
        TextBox      txtMediaSerial;
        Button       btnSaveReadCapacity;
        Button       btnSaveReadCapacity16;
        Button       btnSaveReadMediaSerial;
        TabPage      tabMmc;
        Button       btnSaveGetConfiguration;
        Button       btnSaveRecognizedFormatLayers;
        Button       btnSaveWriteProtectionStatus;
        TabPage      tabSsc;
        GroupBox     grpDensitySupport;
        TextArea     txtDensitySupport;
        GroupBox     grpMediumSupport;
        TextArea     txtMediumSupport;
        Button       btnSaveDensitySupport;
        Button       btnSaveMediumSupport;
        TabPage      tabXbox;
        StackLayout  stkXboxInformation;
        Label        lblXboxL0Video;
        TextBox      txtXboxL0Video;
        Label        lblXboxL1Video;
        TextBox      txtXboxL1Video;
        Label        lblXboxMiddleZone;
        TextBox      txtXboxMiddleZone;
        Label        lblXboxGameSize;
        TextBox      txtXboxGameSize;
        Label        lblXboxTotalSize;
        TextBox      txtXboxTotalSize;
        Label        lblXboxRealBreak;
        TextBox      txtXboxRealBreak;
        GroupBox     grpXboxDmi;
        TextArea     txtXboxDmi;
        GroupBox     grpXboxSs;
        TextArea     txtXboxSs;
        Button       btnSaveXboxSs;
        Button       btnDump;
        ImageView    imgMediaLogo;
        SvgImageView svgMediaLogo;
        Button       btnScan;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}