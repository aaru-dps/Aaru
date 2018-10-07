// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Core.Media.Info;
using DiscImageChef.Decoders.Bluray;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Decoders.SCSI.SSC;
using DiscImageChef.Decoders.Xbox;
using DiscImageChef.Gui.Controls;
using DiscImageChef.Gui.Forms;
using DiscImageChef.Gui.Tabs;
using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Panels
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
                ResourceHandler.GetResourceStream($"DiscImageChef.Gui.Assets.Logos.Media.{scsiInfo.MediaType}.svg");
            /*            if(logo != null)
                        {
                            svgMediaLogo.SvgStream = logo;
                            svgMediaLogo.Visible   = true;
                        }
                        else
                        {*/
            logo = ResourceHandler.GetResourceStream($"DiscImageChef.Gui.Assets.Logos.Media.{scsiInfo.MediaType}.png");
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

            if(this.scsiInfo.XgdInfo != null)
            {
                stkXboxInformation.Visible = true;
                txtXboxL0Video.Text        = $"{this.scsiInfo.XgdInfo.L0Video} sectors";
                txtXboxL1Video.Text        = $"{this.scsiInfo.XgdInfo.L1Video} sectors";
                txtXboxMiddleZone.Text     = $"{this.scsiInfo.XgdInfo.MiddleZone} sectors";
                txtXboxGameSize.Text       = $"{this.scsiInfo.XgdInfo.GameSize} sectors";
                txtXboxTotalSize.Text      = $"{this.scsiInfo.XgdInfo.TotalSize} sectors";
                txtXboxRealBreak.Text      = this.scsiInfo.XgdInfo.LayerBreak.ToString();
            }

            if(this.scsiInfo.DvdDmi != null)
            {
                if(DMI.IsXbox(scsiInfo.DvdDmi))
                {
                    grpXboxDmi.Visible = true;
                    txtXboxDmi.Text    = DMI.PrettifyXbox(scsiInfo.DvdDmi);
                }
                else if(DMI.IsXbox360(scsiInfo.DvdDmi))
                {
                    grpXboxDmi.Visible = true;
                    txtXboxDmi.Text    = DMI.PrettifyXbox360(scsiInfo.DvdDmi);
                }
            }

            if(this.scsiInfo.DecodedXboxSecuritySector.HasValue)
            {
                grpXboxSs.Visible = true;
                txtXboxSs.Text    = SS.Prettify(this.scsiInfo.DecodedXboxSecuritySector);
            }

            btnSaveXboxSs.Visible = this.scsiInfo.XboxSecuritySector != null;
            tabXbox.Visible = stkXboxInformation.Visible || grpXboxDmi.Visible || grpXboxSs.Visible ||
                              btnSaveXboxSs.Visible;

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

            if(this.scsiInfo.BlurayDiscInformation != null)
            {
                grpBlurayDiscInformation.Visible     = true;
                btnSaveBlurayDiscInformation.Visible = true;
                txtBlurayDiscInformation.Text        = DI.Prettify(this.scsiInfo.BlurayDiscInformation);
            }

            if(this.scsiInfo.BlurayBurstCuttingArea != null)
            {
                grpBlurayBurstCuttingArea.Visible     = true;
                btnSaveBlurayBurstCuttingArea.Visible = true;
                txtBlurayBurstCuttingArea.Text        = BCA.Prettify(this.scsiInfo.BlurayBurstCuttingArea);
            }

            if(this.scsiInfo.BlurayDds != null)
            {
                grpBlurayDds.Visible     = true;
                btnSaveBlurayDds.Visible = true;
                txtBlurayDds.Text        = DDS.Prettify(this.scsiInfo.BlurayDds);
            }

            if(this.scsiInfo.BlurayCartridgeStatus != null)
            {
                grpBlurayCartridgeStatus.Visible     = true;
                btnSaveBlurayCartridgeStatus.Visible = true;
                txtBlurayCartridgeStatus.Text        = Cartridge.Prettify(this.scsiInfo.BlurayCartridgeStatus);
            }

            if(this.scsiInfo.BluraySpareAreaInformation != null)
            {
                grpBluraySpareAreaInformation.Visible     = true;
                btnSaveBluraySpareAreaInformation.Visible = true;
                txtBluraySpareAreaInformation.Text        = Spare.Prettify(this.scsiInfo.BluraySpareAreaInformation);
            }

            if(this.scsiInfo.BlurayPowResources != null)
            {
                grpBlurayPowResources.Visible     = true;
                btnSaveBlurayPowResources.Visible = true;
                txtBlurayPowResources.Text        = DiscInformation.Prettify(this.scsiInfo.BlurayPowResources);
            }

            if(this.scsiInfo.BlurayTrackResources != null)
            {
                grpBlurayTrackResources.Visible     = true;
                btnSaveBlurayTrackResources.Visible = true;
                txtBlurayTrackResources.Text        = DiscInformation.Prettify(this.scsiInfo.BlurayTrackResources);
            }

            btnSaveBlurayRawDfl.Visible = this.scsiInfo.BlurayRawDfl != null;
            btnSaveBlurayPac.Visible    = this.scsiInfo.BlurayPac    != null;

            tabBluray.Visible = grpBlurayDiscInformation.Visible      || grpBlurayBurstCuttingArea.Visible ||
                                grpBlurayDds.Visible                  || grpBlurayCartridgeStatus.Visible  ||
                                grpBluraySpareAreaInformation.Visible || grpBlurayPowResources.Visible     ||
                                grpBlurayTrackResources.Visible       || btnSaveBlurayRawDfl.Visible       ||
                                btnSaveBlurayPac.Visible;

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

        protected void OnBtnSaveBlurayDiscInformationClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.BlurayDiscInformation);
        }

        protected void OnBtnSaveBlurayBurstCuttingAreaClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.BlurayBurstCuttingArea);
        }

        protected void OnBtnSaveBlurayDdsClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.BlurayDds);
        }

        protected void OnBtnSaveBlurayCartridgeStatusClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.BlurayCartridgeStatus);
        }

        protected void OnBtnSaveBluraySpareAreaInformationClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.BluraySpareAreaInformation);
        }

        protected void OnBtnSaveBlurayPowResourcesClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.BlurayPowResources);
        }

        protected void OnBtnSaveBlurayTrackResourcesClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.BlurayTrackResources);
        }

        protected void OnBtnSaveBlurayRawDflClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.BlurayRawDfl);
        }

        protected void OnBtnSaveBlurayPacClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.BlurayPac);
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
        TabPage      tabCd;
        TabPage      tabCdInformation;
        TextArea     txtCdInformation;
        Button       btnCdInformation;
        TabPage      tabCdToc;
        TextArea     txtCdToc;
        Button       btnCdToc;
        TabPage      tabCdFullToc;
        TextArea     txtCdFullToc;
        Button       btnCdFullToc;
        TabPage      tabCdSession;
        TextArea     txtCdSession;
        Button       btnCdSession;
        TabPage      tabCdText;
        TextArea     txtCdText;
        Button       btnCdText;
        TabPage      tabCdAtip;
        TextArea     txtCdAtip;
        Button       btnCdAtip;
        TabPage      tabCdMisc;
        StackLayout  stkMcn;
        Label        lblMcn;
        TextBox      txtMcn;
        GroupBox     grpIsrcs;
        TreeGridView grdIsrcs;
        Button       btnCdPma;
        TabPage      tabDvd;
        GroupBox     grpDvdPfi;
        TextArea     txtDvdPfi;
        GroupBox     grpDvdCmi;
        TextArea     txtDvdCmi;
        GroupBox     grpHdDvdCmi;
        TextArea     txtHdDvdCmi;
        Button       btnSaveDvdPfi;
        Button       btnSaveDvdDmi;
        Button       btnSaveDvdCmi;
        Button       btnSaveHdDvdCmi;
        Button       btnSaveDvdBca;
        Button       btnSaveDvdAacs;
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
        GroupBox     grpDvdRamDds;
        TextArea     txtDvdRamDds;
        GroupBox     grpDvdRamCartridgeStatus;
        TextArea     txtDvdRamCartridgeStatus;
        GroupBox     grpDvdRamSpareAreaInformation;
        TextArea     txtDvdRamSpareAreaInformation;
        Button       btnSaveDvdRamDds;
        Button       btnSaveDvdRamCartridgeStatus;
        Button       btnSaveDvdRamSpareAreaInformation;
        TabPage      tabDvdr;
        Button       btnSaveLastBorderOutRmd;
        Button       btnSaveDvdPreRecordedInfo;
        Button       btnSaveDvdrMediaIdentifier;
        Button       btnSaveDvdrPhysicalInformation;
        Button       btnSaveHddvdrMediumStatus;
        Button       btnSaveHddvdrLastRmd;
        Button       btnSaveDvdrLayerCapacity;
        Button       btnSaveDvdrDlMiddleZoneStart;
        Button       btnSaveDvdrDlJumpIntervalSize;
        Button       btnSaveDvdrDlManualLayerJumpStartLba;
        Button       btnSaveDvdrDlRemapAnchorPoint;
        Button       btnSaveDvdPlusAdip;
        Button       btnSaveDvdPlusDcb;
        TabPage      tabBluray;
        GroupBox     grpBlurayDiscInformation;
        TextArea     txtBlurayDiscInformation;
        GroupBox     grpBlurayBurstCuttingArea;
        TextArea     txtBlurayBurstCuttingArea;
        GroupBox     grpBlurayDds;
        TextArea     txtBlurayDds;
        GroupBox     grpBlurayCartridgeStatus;
        TextArea     txtBlurayCartridgeStatus;
        GroupBox     grpBluraySpareAreaInformation;
        TextArea     txtBluraySpareAreaInformation;
        GroupBox     grpBlurayPowResources;
        TextArea     txtBlurayPowResources;
        GroupBox     grpBlurayTrackResources;
        TextArea     txtBlurayTrackResources;
        Button       btnSaveBlurayDiscInformation;
        Button       btnSaveBlurayBurstCuttingArea;
        Button       btnSaveBlurayDds;
        Button       btnSaveBlurayCartridgeStatus;
        Button       btnSaveBluraySpareAreaInformation;
        Button       btnSaveBlurayPowResources;
        Button       btnSaveBlurayTrackResources;
        Button       btnSaveBlurayRawDfl;
        Button       btnSaveBlurayPac;
        Button       btnDump;
        ImageView    imgMediaLogo;
        SvgImageView svgMediaLogo;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}