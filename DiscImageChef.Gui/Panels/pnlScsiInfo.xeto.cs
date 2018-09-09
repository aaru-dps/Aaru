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
using System.Collections.Generic;
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Core.Media.Info;
using DiscImageChef.Decoders.Bluray;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.DVD;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Decoders.SCSI.SSC;
using DiscImageChef.Decoders.Xbox;
using Eto.Forms;
using Eto.Serialization.Xaml;
using BCA = DiscImageChef.Decoders.Bluray.BCA;
using Cartridge = DiscImageChef.Decoders.DVD.Cartridge;
using DDS = DiscImageChef.Decoders.DVD.DDS;
using DMI = DiscImageChef.Decoders.Xbox.DMI;
using Spare = DiscImageChef.Decoders.DVD.Spare;

namespace DiscImageChef.Gui.Panels
{
    public class pnlScsiInfo : Panel
    {
        ScsiInfo scsiInfo;

        public pnlScsiInfo(ScsiInfo scsiInfo)
        {
            XamlReader.Load(this);

            this.scsiInfo = scsiInfo;

            switch(this.scsiInfo.MediaType)
            {
                case MediaType.HDDVDROM:
                case MediaType.HDDVDRAM:
                case MediaType.HDDVDR:
                case MediaType.HDDVDRW:
                case MediaType.HDDVDRDL:
                case MediaType.HDDVDRWDL:
                    tabDvd.Text = "HD DVD";
                    break;
                default:
                    tabDvd.Text = "DVD";
                    break;
            }

            switch(this.scsiInfo.MediaType)
            {
                case MediaType.DVDR:
                    tabDvdr.Text = "DVD-R";
                    break;
                case MediaType.DVDRW:
                    tabDvdr.Text = "DVD-RW";
                    break;
                case MediaType.DVDPR:
                    tabDvdr.Text = "DVD+R";
                    break;
                case MediaType.DVDPRW:
                    tabDvdr.Text = "DVD+RW";
                    break;
                case MediaType.DVDPRWDL:
                    tabDvdr.Text = "DVD+RW DL";
                    break;
                case MediaType.DVDRDL:
                    tabDvdr.Text = "DVD-R DL";
                    break;
                case MediaType.DVDPRDL:
                    tabDvdr.Text = "DVD+R DL";
                    break;
                case MediaType.DVDRAM:
                    tabDvdr.Text = "DVD-RAM";
                    break;
                case MediaType.DVDRWDL:
                    tabDvdr.Text = "DVD-RW DL";
                    break;
                case MediaType.HDDVDRAM:
                    tabDvdr.Text = "HD DVD-RAM";
                    break;
                case MediaType.HDDVDR:
                    tabDvdr.Text = "HD DVD-R";
                    break;
                case MediaType.HDDVDRW:
                    tabDvdr.Text = "HD DVD-RW";
                    break;
                case MediaType.HDDVDRDL:
                    tabDvdr.Text = "HD DVD-R DL";
                    break;
                case MediaType.HDDVDRWDL:
                    tabDvdr.Text = "HD DVD-RW DL";
                    break;
            }

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

            if(this.scsiInfo.DecodedCompactDiscInformation.HasValue)
            {
                tabCdInformation.Visible = true;
                txtCdInformation.Text    = DiscInformation.Prettify000b(scsiInfo.DecodedCompactDiscInformation);
                btnCdInformation.Visible = scsiInfo.CompactDiscInformation != null;
            }

            if(this.scsiInfo.DecodedToc.HasValue)
            {
                tabCdToc.Visible = true;
                txtCdToc.Text    = TOC.Prettify(scsiInfo.DecodedToc);
                btnCdToc.Visible = scsiInfo.Toc != null;
            }

            if(this.scsiInfo.FullToc.HasValue)
            {
                tabCdFullToc.Visible = true;
                txtCdFullToc.Text    = FullTOC.Prettify(scsiInfo.FullToc);
                btnCdFullToc.Visible = scsiInfo.RawToc != null;
            }

            if(this.scsiInfo.DecodedSession.HasValue)
            {
                tabCdSession.Visible = true;
                txtCdSession.Text    = Session.Prettify(scsiInfo.DecodedSession);
                btnCdSession.Visible = scsiInfo.Session != null;
            }

            if(this.scsiInfo.DecodedCdTextLeadIn.HasValue)
            {
                tabCdText.Visible = true;
                txtCdText.Text    = CDTextOnLeadIn.Prettify(this.scsiInfo.DecodedCdTextLeadIn);
                btnCdText.Visible = scsiInfo.CdTextLeadIn != null;
            }

            if(this.scsiInfo.DecodedAtip.HasValue)
            {
                tabCdAtip.Visible = true;
                txtCdAtip.Text    = ATIP.Prettify(this.scsiInfo.Atip);
                btnCdAtip.Visible = scsiInfo.Atip != null;
            }

            if(!string.IsNullOrEmpty(scsiInfo.Mcn))
            {
                stkMcn.Visible = true;
                txtMcn.Text    = scsiInfo.Mcn;
            }

            if(this.scsiInfo.Isrcs != null && this.scsiInfo.Isrcs.Count > 0)
            {
                grpIsrcs.Visible = true;

                TreeGridItemCollection isrcsItems = new TreeGridItemCollection();

                grdIsrcs.Columns.Add(new GridColumn {HeaderText = "ISRC", DataCell  = new TextBoxCell(0)});
                grdIsrcs.Columns.Add(new GridColumn {HeaderText = "Track", DataCell = new TextBoxCell(0)});

                grdIsrcs.AllowMultipleSelection = false;
                grdIsrcs.ShowHeader             = true;
                grdIsrcs.DataStore              = isrcsItems;

                foreach(KeyValuePair<byte, string> isrc in this.scsiInfo.Isrcs)
                    isrcsItems.Add(new TreeGridItem {Values = new object[] {isrc.Key.ToString(), isrc.Value}});
            }

            btnCdPma.Visible = this.scsiInfo.Pma != null;

            tabCdMisc.Visible = stkMcn.Visible || grpIsrcs.Visible || btnCdPma.Visible;

            tabCd.Visible = tabCdInformation.Visible || tabCdToc.Visible  || tabCdFullToc.Visible ||
                            tabCdSession.Visible     || tabCdText.Visible || tabCdAtip.Visible    || stkMcn.Visible ||
                            grpIsrcs.Visible         || btnCdPma.Visible;

            if(this.scsiInfo.DecodedPfi.HasValue)
            {
                grpDvdPfi.Visible = true;
                txtDvdPfi.Text    = PFI.Prettify(this.scsiInfo.DecodedPfi);
            }

            if(this.scsiInfo.DvdCmi != null)
            {
                grpDvdCmi.Visible     = true;
                txtDvdCmi.Text        = CSS_CPRM.PrettifyLeadInCopyright(this.scsiInfo.DvdCmi);
                btnSaveDvdCmi.Visible = true;
            }

            btnSaveDvdPfi.Visible   = this.scsiInfo.DvdPfi                    != null;
            btnSaveDvdDmi.Visible   = this.scsiInfo.DvdDmi                    != null;
            btnSaveDvdCmi.Visible   = this.scsiInfo.DvdCmi                    != null;
            btnSaveHdDvdCmi.Visible = this.scsiInfo.HddvdCopyrightInformation != null;
            btnSaveDvdBca.Visible   = this.scsiInfo.DvdBca                    != null;
            btnSaveDvdAacs.Visible  = this.scsiInfo.DvdAacs                   != null;

            tabDvd.Visible = grpDvdPfi.Visible     || grpDvdCmi.Visible || btnSaveDvdPfi.Visible ||
                             btnSaveDvdDmi.Visible ||
                             btnSaveDvdCmi.Visible || btnSaveHdDvdCmi.Visible || btnSaveDvdBca.Visible ||
                             btnSaveDvdAacs.Visible;

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

            if(this.scsiInfo.DvdRamDds != null)
            {
                grpDvdRamDds.Visible     = true;
                btnSaveDvdRamDds.Visible = true;
                txtDvdRamDds.Text        = DDS.Prettify(this.scsiInfo.DvdRamDds);
            }

            if(this.scsiInfo.DvdRamCartridgeStatus != null)
            {
                grpDvdRamCartridgeStatus.Visible     = true;
                btnSaveDvdRamCartridgeStatus.Visible = true;
                txtDvdRamCartridgeStatus.Text        = Cartridge.Prettify(this.scsiInfo.DvdRamCartridgeStatus);
            }

            if(this.scsiInfo.DvdRamSpareArea != null)
            {
                grpDvdRamSpareAreaInformation.Visible     = true;
                btnSaveDvdRamSpareAreaInformation.Visible = true;
                txtDvdRamSpareAreaInformation.Text        = Spare.Prettify(this.scsiInfo.DvdRamSpareArea);
            }

            btnSaveDvdRamDds.Visible                     = this.scsiInfo.DvdRamDds                     != null;
            btnSaveDvdRamCartridgeStatus.Visible         = this.scsiInfo.DvdRamCartridgeStatus         != null;
            btnSaveDvdRamSpareAreaInformation.Visible    = this.scsiInfo.DvdRamSpareArea               != null;
            btnSaveLastBorderOutRmd.Visible              = this.scsiInfo.LastBorderOutRmd              != null;
            btnSaveDvdPreRecordedInfo.Visible            = this.scsiInfo.DvdPreRecordedInfo            != null;
            btnSaveDvdrMediaIdentifier.Visible           = this.scsiInfo.DvdrMediaIdentifier           != null;
            btnSaveDvdrPhysicalInformation.Visible       = this.scsiInfo.DvdrPhysicalInformation       != null;
            btnSaveHddvdrMediumStatus.Visible            = this.scsiInfo.HddvdrMediumStatus            != null;
            btnSaveHddvdrLastRmd.Visible                 = this.scsiInfo.HddvdrLastRmd                 != null;
            btnSaveDvdrLayerCapacity.Visible             = this.scsiInfo.DvdrLayerCapacity             != null;
            btnSaveDvdrDlMiddleZoneStart.Visible         = this.scsiInfo.DvdrDlMiddleZoneStart         != null;
            btnSaveDvdrDlJumpIntervalSize.Visible        = this.scsiInfo.DvdrDlJumpIntervalSize        != null;
            btnSaveDvdrDlManualLayerJumpStartLba.Visible = this.scsiInfo.DvdrDlManualLayerJumpStartLba != null;
            btnSaveDvdrDlRemapAnchorPoint.Visible        = this.scsiInfo.DvdrDlRemapAnchorPoint        != null;
            btnSaveDvdPlusAdip.Visible                   = this.scsiInfo.DvdPlusAdip                   != null;
            btnSaveDvdPlusDcb.Visible                    = this.scsiInfo.DvdPlusDcb                    != null;

            tabDvdr.Visible = grpDvdRamDds.Visible                  || grpDvdRamCartridgeStatus.Visible             ||
                              grpDvdRamSpareAreaInformation.Visible || btnSaveDvdRamDds.Visible                     ||
                              btnSaveDvdRamCartridgeStatus.Visible  || btnSaveDvdRamSpareAreaInformation.Visible    ||
                              btnSaveLastBorderOutRmd.Visible       || btnSaveDvdPreRecordedInfo.Visible            ||
                              btnSaveDvdrMediaIdentifier.Visible    || btnSaveDvdrPhysicalInformation.Visible       ||
                              btnSaveHddvdrMediumStatus.Visible     || btnSaveHddvdrLastRmd.Visible                 ||
                              btnSaveDvdrLayerCapacity.Visible      || btnSaveDvdrDlMiddleZoneStart.Visible         ||
                              btnSaveDvdrDlJumpIntervalSize.Visible || btnSaveDvdrDlManualLayerJumpStartLba.Visible ||
                              btnSaveDvdrDlRemapAnchorPoint.Visible || btnSaveDvdPlusAdip.Visible                   ||
                              btnSaveDvdPlusDcb.Visible;

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
                txtBlurayDds.Text        = Decoders.Bluray.DDS.Prettify(this.scsiInfo.BlurayDds);
            }

            if(this.scsiInfo.BlurayCartridgeStatus != null)
            {
                grpBlurayCartridgeStatus.Visible     = true;
                btnSaveBlurayCartridgeStatus.Visible = true;
                txtBlurayCartridgeStatus.Text =
                    Decoders.Bluray.Cartridge.Prettify(this.scsiInfo.BlurayCartridgeStatus);
            }

            if(this.scsiInfo.BluraySpareAreaInformation != null)
            {
                grpBluraySpareAreaInformation.Visible     = true;
                btnSaveBluraySpareAreaInformation.Visible = true;
                txtBluraySpareAreaInformation.Text =
                    Decoders.Bluray.Spare.Prettify(this.scsiInfo.BluraySpareAreaInformation);
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

        protected void OnBtnCdInformationClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.CompactDiscInformation);
        }

        protected void OnBtnCdTocClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.Toc);
        }

        protected void OnBtnCdFullTocClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.RawToc);
        }

        protected void OnBtnCdSessionClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.Session);
        }

        protected void OnBtnCdTextClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.CdTextLeadIn);
        }

        protected void OnBtnCdAtipClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.Atip);
        }

        protected void OnBtnCdPmaClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.Pma);
        }

        protected void OnBtnSaveDvdPfiClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdPfi);
        }

        protected void OnBtnSaveDvdDmiClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdDmi);
        }

        protected void OnBtnSaveDvdCmiClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdCmi);
        }

        protected void OnBtnSaveHdDvdCmiClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.HddvdCopyrightInformation);
        }

        protected void OnBtnSaveDvdBcaClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdBca);
        }

        protected void OnBtnSaveDvdAacsClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdAacs);
        }

        protected void OnBtnSaveXboxSsClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.XboxSecuritySector);
        }

        protected void OnBtnSaveDvdRamDdsClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdRamDds);
        }

        protected void OnBtnSaveDvdRamCartridgeStatusClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdRamCartridgeStatus);
        }

        protected void OnBtnSaveDvdRamSpareAreaInformationClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdRamSpareArea);
        }

        protected void OnBtnSaveLastBorderOutRmdClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.LastBorderOutRmd);
        }

        protected void OnBtnSaveDvdPreRecordedInfoClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdPreRecordedInfo);
        }

        protected void OnBtnSaveDvdrMediaIdentifierClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdrMediaIdentifier);
        }

        protected void OnBtnSaveDvdrPhysicalInformationClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdrPhysicalInformation);
        }

        protected void OnBtnSaveHddvdrMediumStatusClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.HddvdrMediumStatus);
        }

        protected void OnBtnSaveHddvdrLastRmdClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.HddvdrLastRmd);
        }

        protected void OnBtnSaveDvdrLayerCapacityClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdrLayerCapacity);
        }

        protected void OnBtnSaveDvdrDlMiddleZoneStartClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdrDlMiddleZoneStart);
        }

        protected void OnBtnSaveDvdrDlJumpIntervalSizeClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdrDlJumpIntervalSize);
        }

        protected void OnBtnSaveDvdrDlManualLayerJumpStartLbaClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdrDlManualLayerJumpStartLba);
        }

        protected void OnBtnSaveDvdrDlRemapAnchorPointClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdrDlRemapAnchorPoint);
        }

        protected void OnBtnSaveDvdPlusAdipClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdPlusAdip);
        }

        protected void OnBtnSaveDvdPlusDcbClick(object sender, EventArgs e)
        {
            SaveElement(scsiInfo.DvdPlusDcb);
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
            // Not implemented
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
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}