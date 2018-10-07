// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : pnlDeviceInfo.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device information.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the device information panel.
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
using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Decoders.SCSI.SSC;
using DiscImageChef.Devices;
using DiscImageChef.Gui.Tabs;
using Eto.Forms;
using Eto.Serialization.Xaml;
using DeviceInfo = DiscImageChef.Core.Devices.Info.DeviceInfo;
using Tuple = DiscImageChef.Decoders.PCMCIA.Tuple;

namespace DiscImageChef.Gui.Panels
{
    public class pnlDeviceInfo : Panel
    {
        DeviceInfo devInfo;

        public pnlDeviceInfo(DeviceInfo devInfo)
        {
            XamlReader.Load(this);

            this.devInfo = devInfo;

            txtType.Text         = devInfo.Type.ToString();
            txtManufacturer.Text = devInfo.Manufacturer;
            txtModel.Text        = devInfo.Model;
            txtRevision.Text     = devInfo.Revision;
            txtSerial.Text       = devInfo.Serial;
            txtScsiType.Text     = devInfo.ScsiType.ToString();
            chkRemovable.Checked = devInfo.IsRemovable;
            chkUsb.Checked       = devInfo.IsUsb;

            if(devInfo.IsUsb)
            {
                tabUsb.Visible                = true;
                btnSaveUsbDescriptors.Enabled = devInfo.UsbDescriptors != null;
                txtUsbVendorId.Text           = $"{devInfo.UsbVendorId:X4}";
                txtUsbProductId.Text          = $"{devInfo.UsbProductId:X4}";
                txtUsbManufacturer.Text       = devInfo.UsbManufacturerString;
                txtUsbProduct.Text            = devInfo.UsbProductString;
                txtUsbSerial.Text             = devInfo.UsbSerialString;
            }

            if(devInfo.IsFireWire)
            {
                tabFirewire.Visible          = true;
                txtFirewireVendorId.Text     = $"{devInfo.FireWireVendor:X4}";
                txtFirewireModelId.Text      = $"{devInfo.FireWireModel:X4}";
                txtFirewireManufacturer.Text = devInfo.FireWireVendorName;
                txtFirewireModel.Text        = devInfo.FireWireModelName;
                txtFirewireGuid.Text         = $"{devInfo.FireWireGuid:X16}";
            }

            if(devInfo.IsPcmcia)
            {
                tabPcmcia.Visible = true;

                TreeGridItemCollection cisList = new TreeGridItemCollection();

                treePcmcia.Columns.Add(new GridColumn {HeaderText = "CIS", DataCell = new TextBoxCell(0)});

                treePcmcia.AllowMultipleSelection = false;
                treePcmcia.ShowHeader             = false;
                treePcmcia.DataStore              = cisList;

                Tuple[] tuples = CIS.GetTuples(devInfo.Cis);
                if(tuples != null)
                    foreach(Tuple tuple in tuples)
                    {
                        string tupleCode;
                        string tupleDescription;

                        switch(tuple.Code)
                        {
                            case TupleCodes.CISTPL_NULL:
                            case TupleCodes.CISTPL_END: continue;
                            case TupleCodes.CISTPL_DEVICEGEO:
                            case TupleCodes.CISTPL_DEVICEGEO_A:
                                tupleCode        = "Device Geometry Tuples";
                                tupleDescription = CIS.PrettifyDeviceGeometryTuple(tuple);
                                break;
                            case TupleCodes.CISTPL_MANFID:
                                tupleCode        = "Manufacturer Identification Tuple";
                                tupleDescription = CIS.PrettifyManufacturerIdentificationTuple(tuple);
                                break;
                            case TupleCodes.CISTPL_VERS_1:
                                tupleCode        = "Level 1 Version / Product Information Tuple";
                                tupleDescription = CIS.PrettifyLevel1VersionTuple(tuple);
                                break;
                            case TupleCodes.CISTPL_ALTSTR:
                            case TupleCodes.CISTPL_BAR:
                            case TupleCodes.CISTPL_BATTERY:
                            case TupleCodes.CISTPL_BYTEORDER:
                            case TupleCodes.CISTPL_CFTABLE_ENTRY:
                            case TupleCodes.CISTPL_CFTABLE_ENTRY_CB:
                            case TupleCodes.CISTPL_CHECKSUM:
                            case TupleCodes.CISTPL_CONFIG:
                            case TupleCodes.CISTPL_CONFIG_CB:
                            case TupleCodes.CISTPL_DATE:
                            case TupleCodes.CISTPL_DEVICE:
                            case TupleCodes.CISTPL_DEVICE_A:
                            case TupleCodes.CISTPL_DEVICE_OA:
                            case TupleCodes.CISTPL_DEVICE_OC:
                            case TupleCodes.CISTPL_EXTDEVIC:
                            case TupleCodes.CISTPL_FORMAT:
                            case TupleCodes.CISTPL_FORMAT_A:
                            case TupleCodes.CISTPL_FUNCE:
                            case TupleCodes.CISTPL_FUNCID:
                            case TupleCodes.CISTPL_GEOMETRY:
                            case TupleCodes.CISTPL_INDIRECT:
                            case TupleCodes.CISTPL_JEDEC_A:
                            case TupleCodes.CISTPL_JEDEC_C:
                            case TupleCodes.CISTPL_LINKTARGET:
                            case TupleCodes.CISTPL_LONGLINK_A:
                            case TupleCodes.CISTPL_LONGLINK_C:
                            case TupleCodes.CISTPL_LONGLINK_CB:
                            case TupleCodes.CISTPL_LONGLINK_MFC:
                            case TupleCodes.CISTPL_NO_LINK:
                            case TupleCodes.CISTPL_ORG:
                            case TupleCodes.CISTPL_PWR_MGMNT:
                            case TupleCodes.CISTPL_SPCL:
                            case TupleCodes.CISTPL_SWIL:
                            case TupleCodes.CISTPL_VERS_2:
                                tupleCode        = $"Undecoded tuple ID {tuple.Code}";
                                tupleDescription = $"Undecoded tuple ID {tuple.Code}";
                                break;
                            default:
                                tupleCode        = $"0x{(byte)tuple.Code:X2}";
                                tupleDescription = $"Found unknown tuple ID 0x{(byte)tuple.Code:X2}";
                                break;
                        }

                        cisList.Add(new TreeGridItem {Values = new object[] {tupleCode, tupleDescription}});
                    }
                else DicConsole.DebugWriteLine("Device-Info command", "PCMCIA CIS returned no tuples");
            }

            if(devInfo.AtaIdentify != null || devInfo.AtapiIdentify != null)
            {
                tabAta.Visible = true;

                if(devInfo.AtaIdentify != null)
                {
                    stkAtaMcpt.Visible  = false;
                    chkAtaMcpt.Checked  = devInfo.AtaMcptError.HasValue;
                    lblAtaMcpt.Visible  = devInfo.AtaMcptError.HasValue;
                    lblAtaIdentify.Text = "ATA IDENTIFY DEVICE";

                    if(devInfo.AtaMcptError.HasValue)
                    {
                        switch(devInfo.AtaMcptError.Value.DeviceHead & 0x7)
                        {
                            case 0:
                                lblAtaMcpt.Text = "Device reports incorrect media card type";
                                break;
                            case 1:
                                lblAtaMcpt.Text = "Device contains a Secure Digital card";
                                break;
                            case 2:
                                lblAtaMcpt.Text = "Device contains a MultiMediaCard ";
                                break;
                            case 3:
                                lblAtaMcpt.Text = "Device contains a Secure Digital I/O card";
                                break;
                            case 4:
                                lblAtaMcpt.Text = "Device contains a Smart Media card";
                                break;
                            default:
                                lblAtaMcpt.Text =
                                    $"Device contains unknown media card type {devInfo.AtaMcptError.Value.DeviceHead & 0x07}";
                                break;
                        }

                        chkAtaMcptWriteProtection.Checked = (devInfo.AtaMcptError.Value.DeviceHead & 0x08) == 0x08;

                        ushort specificData = (ushort)(devInfo.AtaMcptError.Value.CylinderHigh * 0x100 +
                                                       devInfo.AtaMcptError.Value.CylinderLow);
                        if(specificData != 0)
                        {
                            lblAtaMcptSpecificData.Visible = true;
                            lblAtaMcptSpecificData.Text    = $"Card specific data: 0x{specificData:X4}";
                        }
                    }

                    tabAta.Text         = "ATA";
                    txtAtaIdentify.Text = Identify.Prettify(this.devInfo.AtaIdentify);
                }
                else if(devInfo.AtapiIdentify != null)
                {
                    lblAtaIdentify.Text = "ATA PACKET IDENTIFY DEVICE";
                    stkAtaMcpt.Visible  = false;
                    tabAta.Text         = "ATAPI";
                    txtAtaIdentify.Text = Identify.Prettify(this.devInfo.AtapiIdentify);
                }
            }

            if(devInfo.ScsiInquiryData != null)
            {
                tabScsiInfo tabScsiInfo = new tabScsiInfo();
                tabScsiInfo.LoadData(devInfo.ScsiInquiryData, devInfo.ScsiInquiry, devInfo.ScsiEvpdPages,
                                     devInfo.ScsiMode, devInfo.ScsiType, devInfo.ScsiModeSense6,
                                     devInfo.ScsiModeSense10, devInfo.MmcConfiguration);

                tabInfos.Pages.Add(tabScsiInfo);

                if(devInfo.PlextorFeatures != null)
                {
                    tabPlextor.Visible = true;
                    if(devInfo.PlextorFeatures.Eeprom != null)
                    {
                        stkPlextorEeprom.Visible  = true;
                        txtPlextorDiscs.Text      = $"{devInfo.PlextorFeatures.Discs}";
                        txtPlextorCdReadTime.Text = TimeSpan.FromSeconds(devInfo.PlextorFeatures.CdReadTime).ToString();
                        txtPlextorCdWriteTime.Text =
                            TimeSpan.FromSeconds(devInfo.PlextorFeatures.CdWriteTime).ToString();
                        if(devInfo.PlextorFeatures.IsDvd)
                        {
                            stkPlextorDvdTimes.Visible = true;
                            txtPlextorDvdReadTime.Text =
                                TimeSpan.FromSeconds(devInfo.PlextorFeatures.DvdReadTime).ToString();
                            txtPlextorDvdWriteTime.Text =
                                TimeSpan.FromSeconds(devInfo.PlextorFeatures.DvdWriteTime).ToString();
                        }
                    }

                    chkPlextorPoweRec.Checked = devInfo.PlextorFeatures.PoweRec;

                    if(devInfo.PlextorFeatures.PoweRec)
                    {
                        chkPlextorPoweRecEnabled.Visible = true;
                        chkPlextorPoweRecEnabled.Checked = devInfo.PlextorFeatures.PoweRecEnabled;

                        if(devInfo.PlextorFeatures.PoweRecEnabled)
                        {
                            stkPlextorPoweRecEnabled.Visible = true;

                            if(devInfo.PlextorFeatures.PoweRecRecommendedSpeed > 0)
                            {
                                stkPlextorPoweRecRecommended.Visible = true;
                                txtPlextorPoweRecRecommended.Text =
                                    $"{devInfo.PlextorFeatures.PoweRecRecommendedSpeed} Kb/sec.";
                            }

                            if(devInfo.PlextorFeatures.PoweRecSelected > 0)
                            {
                                stkPlextorPoweRecSelected.Visible = true;
                                txtPlextorPoweRecSelected.Text =
                                    $"{devInfo.PlextorFeatures.PoweRecSelected} Kb/sec.";
                            }

                            if(devInfo.PlextorFeatures.PoweRecMax > 0)
                            {
                                stkPlextorPoweRecMax.Visible = true;
                                txtPlextorPoweRecMax.Text    = $"{devInfo.PlextorFeatures.PoweRecMax} Kb/sec.";
                            }

                            if(devInfo.PlextorFeatures.PoweRecLast > 0)
                            {
                                stkPlextorPoweRecLast.Visible = true;
                                txtPlextorPoweRecLast.Text    = $"{devInfo.PlextorFeatures.PoweRecLast} Kb/sec.";
                            }
                        }
                    }

                    chkPlextorSilentMode.Checked = devInfo.PlextorFeatures.SilentMode;

                    if(devInfo.PlextorFeatures.SilentMode)
                    {
                        chkPlextorSilentModeEnabled.Visible = true;
                        chkPlextorSilentModeEnabled.Checked = devInfo.PlextorFeatures.SilentModeEnabled;

                        if(devInfo.PlextorFeatures.SilentModeEnabled)
                        {
                            lblPlextorSilentModeAccessTime.Text = devInfo.PlextorFeatures.AccessTimeLimit == 2
                                                                      ? "\tAccess time is slow"
                                                                      : "\tAccess time is fast";

                            txtPlextorSilentModeCdReadSpeedLimit.Text =
                                devInfo.PlextorFeatures.CdReadSpeedLimit > 0
                                    ? $"{devInfo.PlextorFeatures.CdReadSpeedLimit}x"
                                    : "unlimited";

                            txtPlextorSilentModeCdWriteSpeedLimit.Text =
                                devInfo.PlextorFeatures.CdWriteSpeedLimit > 0
                                    ? $"{devInfo.PlextorFeatures.CdReadSpeedLimit}x"
                                    : "unlimited";

                            if(devInfo.PlextorFeatures.IsDvd)
                            {
                                stkPlextorSilentModeDvdReadSpeedLimit.Visible = true;
                                txtPlextorSilentModeDvdReadSpeedLimit.Text =
                                    devInfo.PlextorFeatures.DvdReadSpeedLimit > 0
                                        ? $"{devInfo.PlextorFeatures.DvdReadSpeedLimit}x"
                                        : "unlimited";
                            }
                        }
                    }

                    chkPlextorGigaRec.Checked   = devInfo.PlextorFeatures.GigaRec;
                    chkPlextorSecuRec.Checked   = devInfo.PlextorFeatures.SecuRec;
                    chkPlextorSpeedRead.Checked = devInfo.PlextorFeatures.SpeedRead;

                    if(devInfo.PlextorFeatures.SpeedRead)
                    {
                        chkPlextorSpeedEnabled.Visible = true;
                        chkPlextorSpeedEnabled.Checked = devInfo.PlextorFeatures.SpeedReadEnabled;
                    }

                    chkPlextorHiding.Checked = devInfo.PlextorFeatures.Hiding;
                    if(devInfo.PlextorFeatures.Hiding)
                    {
                        stkPlextorHiding.Visible           = true;
                        chkPlextorHidesRecordables.Checked = devInfo.PlextorFeatures.HidesRecordables;
                        chkPlextorHidesSessions.Checked    = devInfo.PlextorFeatures.HidesSessions;
                    }

                    chkPlextorVariRec.Checked = devInfo.PlextorFeatures.VariRec;

                    if(devInfo.PlextorFeatures.IsDvd)
                    {
                        stkPlextorDvd.Visible              = true;
                        chkPlextorVariRecDvd.Checked       = devInfo.PlextorFeatures.VariRecDvd;
                        chkPlextorBitSetting.Checked       = devInfo.PlextorFeatures.BitSetting;
                        chkPlextorBitSettingDl.Checked     = devInfo.PlextorFeatures.BitSettingDl;
                        chkPlextorDvdPlusWriteTest.Checked = devInfo.PlextorFeatures.DvdPlusWriteTest;
                    }
                }

                if(devInfo.ScsiInquiry.Value.KreonPresent)
                {
                    tabKreon.Visible                  = true;
                    chkKreonChallengeResponse.Checked = devInfo.KreonFeatures.HasFlag(KreonFeatures.ChallengeResponse);
                    chkKreonDecryptSs.Checked         = devInfo.KreonFeatures.HasFlag(KreonFeatures.DecryptSs);
                    chkKreonXtremeUnlock.Checked      = devInfo.KreonFeatures.HasFlag(KreonFeatures.XtremeUnlock);
                    chkKreonWxripperUnlock.Checked    = devInfo.KreonFeatures.HasFlag(KreonFeatures.WxripperUnlock);
                    chkKreonChallengeResponse360.Checked =
                        devInfo.KreonFeatures.HasFlag(KreonFeatures.ChallengeResponse360);
                    chkKreonDecryptSs360.Checked      = devInfo.KreonFeatures.HasFlag(KreonFeatures.DecryptSs360);
                    chkKreonXtremeUnlock360.Checked   = devInfo.KreonFeatures.HasFlag(KreonFeatures.XtremeUnlock360);
                    chkKreonWxripperUnlock360.Checked = devInfo.KreonFeatures.HasFlag(KreonFeatures.WxripperUnlock360);
                    chkKreonLock.Checked              = devInfo.KreonFeatures.HasFlag(KreonFeatures.Lock);
                    chkKreonErrorSkipping.Checked     = devInfo.KreonFeatures.HasFlag(KreonFeatures.ErrorSkipping);
                }

                if(devInfo.BlockLimits != null)
                {
                    BlockLimits.BlockLimitsData? blockLimits = BlockLimits.Decode(devInfo.BlockLimits);

                    if(blockLimits.HasValue)
                    {
                        tabSsc.Visible = true;
                        if(blockLimits.Value.minBlockLen == blockLimits.Value.maxBlockLen)
                        {
                            lblMinBlockSize.Visible = true;
                            lblMinBlockSize.Text =
                                $"Device's block size is fixed at {blockLimits.Value.minBlockLen} bytes";
                        }
                        else
                        {
                            lblMinBlockSize.Visible = true;
                            lblMaxBlockSize.Visible = true;

                            lblMaxBlockSize.Text = blockLimits.Value.maxBlockLen > 0
                                                       ? $"Device's maximum block size is {blockLimits.Value.maxBlockLen} bytes"
                                                       : "Device does not specify a maximum block size";
                            lblMinBlockSize.Text =
                                $"Device's minimum block size is {blockLimits.Value.minBlockLen} bytes";

                            if(blockLimits.Value.granularity > 0)
                            {
                                lblBlockSizeGranularity.Visible = true;
                                lblBlockSizeGranularity.Text =
                                    $"Device's needs a block size granularity of 2^{blockLimits.Value.granularity} ({Math.Pow(2, blockLimits.Value.granularity)}) bytes";
                            }
                        }
                    }
                }

                if(devInfo.DensitySupport != null)
                    if(devInfo.DensitySupportHeader.HasValue)
                    {
                        stkDensities.Visible = true;
                        txtDensities.Text    = DensitySupport.PrettifyDensity(devInfo.DensitySupportHeader);
                    }

                if(devInfo.MediumDensitySupport != null)
                {
                    if(devInfo.MediaTypeSupportHeader.HasValue)
                    {
                        stkMediaTypes.Visible = true;
                        txtMediumTypes.Text   = DensitySupport.PrettifyMediumType(devInfo.MediaTypeSupportHeader);
                    }

                    txtMediumDensity.Visible = true;
                    txtMediumDensity.Text    = DensitySupport.PrettifyMediumType(devInfo.MediumDensitySupport);
                }
            }

            switch(devInfo.Type)
            {
                case DeviceType.MMC:
                {
                    tabSecureDigital.Text = "MultiMediaCard";
                    if(devInfo.CID != null)
                    {
                        tabCid.Visible = true;
                        txtCid.Text    = Decoders.MMC.Decoders.PrettifyCID(devInfo.CID);
                    }

                    if(devInfo.CSD != null)
                    {
                        tabCsd.Visible = true;
                        txtCid.Text    = Decoders.MMC.Decoders.PrettifyCSD(devInfo.CSD);
                    }

                    if(devInfo.OCR != null)
                    {
                        tabOcr.Visible = true;
                        txtCid.Text    = Decoders.MMC.Decoders.PrettifyOCR(devInfo.OCR);
                    }

                    if(devInfo.ExtendedCSD != null)
                    {
                        tabExtendedCsd.Visible = true;
                        txtCid.Text            = Decoders.MMC.Decoders.PrettifyExtendedCSD(devInfo.ExtendedCSD);
                    }
                }
                    break;
                case DeviceType.SecureDigital:
                {
                    tabSecureDigital.Text = "SecureDigital";
                    if(devInfo.CID != null)
                    {
                        tabCid.Visible = true;

                        txtCid.Text = Decoders.SecureDigital.Decoders.PrettifyCID(devInfo.CID);
                    }

                    if(devInfo.CSD != null)
                    {
                        tabCsd.Visible = true;

                        txtCid.Text = Decoders.SecureDigital.Decoders.PrettifyCSD(devInfo.CSD);
                    }

                    if(devInfo.OCR != null)
                    {
                        tabOcr.Visible = true;
                        txtCid.Text    = Decoders.SecureDigital.Decoders.PrettifyOCR(devInfo.OCR);
                    }

                    if(devInfo.SCR != null)
                    {
                        tabScr.Visible = true;
                        txtCid.Text    = Decoders.SecureDigital.Decoders.PrettifySCR(devInfo.SCR);
                    }
                }
                    break;
            }

            tabSecureDigital.Visible = tabCid.Visible || tabCsd.Visible || tabOcr.Visible || tabExtendedCsd.Visible ||
                                       tabScr.Visible;
        }

        protected void OnBtnSaveAtaBinary(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            if(devInfo.AtaIdentify        != null) saveFs.Write(devInfo.AtaIdentify,   0, devInfo.AtaIdentify.Length);
            else if(devInfo.AtapiIdentify != null) saveFs.Write(devInfo.AtapiIdentify, 0, devInfo.AtapiIdentify.Length);

            saveFs.Close();
        }

        protected void OnBtnSaveAtaText(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveText = new SaveFileDialog();
            dlgSaveText.Filters.Add(new FileFilter {Extensions = new[] {"*.txt"}, Name = "Text"});
            DialogResult result = dlgSaveText.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream   saveFs = new FileStream(dlgSaveText.FileName, FileMode.Create);
            StreamWriter saveSw = new StreamWriter(saveFs);
            saveSw.Write(txtAtaIdentify.Text);
            saveFs.Close();
        }

        protected void OnBtnSaveUsbDescriptors(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(devInfo.UsbDescriptors, 0, devInfo.UsbDescriptors.Length);

            saveFs.Close();
        }

        protected void OnTreePcmciaSelectedItemChanged(object sender, EventArgs e)
        {
            if(!(treePcmcia.SelectedItem is TreeGridItem item)) return;

            txtPcmciaCis.Text = item.Values[1] as string;
        }

        protected void OnBtnSavePcmciaCis(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(devInfo.Cis, 0, devInfo.Cis.Length);

            saveFs.Close();
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        Label        lblDeviceInfo;
        TabControl   tabInfos;
        TabPage      tabGeneral;
        Label        lblType;
        TextBox      txtType;
        Label        lblManufacturer;
        TextBox      txtManufacturer;
        Label        lblModel;
        TextBox      txtModel;
        Label        lblRevision;
        TextBox      txtRevision;
        Label        lblSerial;
        TextBox      txtSerial;
        Label        lblScsiType;
        TextBox      txtScsiType;
        CheckBox     chkRemovable;
        CheckBox     chkUsb;
        TabPage      tabAta;
        Label        lblAtaIdentify;
        TextArea     txtAtaIdentify;
        Button       btnSaveAtaBinary;
        Button       btnSaveAtaText;
        StackLayout  stkAtaMcpt;
        CheckBox     chkAtaMcpt;
        Label        lblAtaMcpt;
        CheckBox     chkAtaMcptWriteProtection;
        Label        lblAtaMcptSpecificData;
        TabPage      tabKreon;
        CheckBox     chkKreonChallengeResponse;
        CheckBox     chkKreonDecryptSs;
        CheckBox     chkKreonXtremeUnlock;
        CheckBox     chkKreonWxripperUnlock;
        CheckBox     chkKreonChallengeResponse360;
        CheckBox     chkKreonDecryptSs360;
        CheckBox     chkKreonXtremeUnlock360;
        CheckBox     chkKreonWxripperUnlock360;
        CheckBox     chkKreonLock;
        CheckBox     chkKreonErrorSkipping;
        TabPage      tabPlextor;
        StackLayout  stkPlextorEeprom;
        Label        lblPlextorDiscs;
        TextBox      txtPlextorDiscs;
        Label        lblPlextorCdReadTime;
        TextBox      txtPlextorCdReadTime;
        Label        lblPlextorCdWriteTime;
        TextBox      txtPlextorCdWriteTime;
        StackLayout  stkPlextorDvdTimes;
        Label        lblPlextorDvdReadTime;
        TextBox      txtPlextorDvdReadTime;
        Label        lblPlextorDvdWriteTime;
        TextBox      txtPlextorDvdWriteTime;
        CheckBox     chkPlextorPoweRec;
        CheckBox     chkPlextorPoweRecEnabled;
        StackLayout  stkPlextorPoweRecEnabled;
        StackLayout  stkPlextorPoweRecRecommended;
        Label        lblPlextorPoweRecRecommended;
        TextBox      txtPlextorPoweRecRecommended;
        StackLayout  stkPlextorPoweRecSelected;
        Label        lblPlextorPoweRecSelected;
        TextBox      txtPlextorPoweRecSelected;
        StackLayout  stkPlextorPoweRecMax;
        Label        lblPlextorPoweRecMax;
        TextBox      txtPlextorPoweRecMax;
        StackLayout  stkPlextorPoweRecLast;
        Label        lblPlextorPoweRecLast;
        TextBox      txtPlextorPoweRecLast;
        CheckBox     chkPlextorSilentMode;
        CheckBox     chkPlextorSilentModeEnabled;
        StackLayout  stkPlextorSilentModeEnabled;
        Label        lblPlextorSilentModeAccessTime;
        Label        lblPlextorSilentModeCdReadSpeedLimit;
        TextBox      txtPlextorSilentModeCdReadSpeedLimit;
        StackLayout  stkPlextorSilentModeDvdReadSpeedLimit;
        Label        lblPlextorSilentModeDvdReadSpeedLimit;
        TextBox      txtPlextorSilentModeDvdReadSpeedLimit;
        Label        lblPlextorSilentModeCdWriteSpeedLimit;
        TextBox      txtPlextorSilentModeCdWriteSpeedLimit;
        CheckBox     chkPlextorGigaRec;
        CheckBox     chkPlextorSecuRec;
        CheckBox     chkPlextorSpeedRead;
        CheckBox     chkPlextorSpeedEnabled;
        CheckBox     chkPlextorHiding;
        StackLayout  stkPlextorHiding;
        CheckBox     chkPlextorHidesRecordables;
        CheckBox     chkPlextorHidesSessions;
        CheckBox     chkPlextorVariRec;
        StackLayout  stkPlextorDvd;
        CheckBox     chkPlextorVariRecDvd;
        CheckBox     chkPlextorBitSetting;
        CheckBox     chkPlextorBitSettingDl;
        CheckBox     chkPlextorDvdPlusWriteTest;
        TabPage      tabSsc;
        StackLayout  Vertical;
        Label        lblMinBlockSize;
        Label        lblMaxBlockSize;
        Label        lblBlockSizeGranularity;
        StackLayout  stkDensities;
        Label        lblDensities;
        TextArea     txtDensities;
        StackLayout  stkMediaTypes;
        Label        lblMediumTypes;
        TextArea     txtMediumTypes;
        TextArea     txtMediumDensity;
        TabPage      tabSecureDigital;
        TabPage      tabCid;
        TextArea     txtCid;
        TabPage      tabCsd;
        TextArea     txtCsd;
        TabPage      tabOcr;
        TextArea     txtOcr;
        TabPage      tabExtendedCsd;
        TextArea     txtExtendedCsd;
        TabPage      tabScr;
        TextArea     txtScr;
        TabPage      tabUsb;
        Label        lblUsbVendorId;
        TextBox      txtUsbVendorId;
        Label        lblUsbProductId;
        TextBox      txtUsbProductId;
        Label        lblUsbManufacturer;
        TextBox      txtUsbManufacturer;
        Label        lblUsbProduct;
        TextBox      txtUsbProduct;
        Label        lblUsbSerial;
        TextBox      txtUsbSerial;
        Button       btnSaveUsbDescriptors;
        TabPage      tabFirewire;
        Label        lblFirewireVendorId;
        TextBox      txtFirewireVendorId;
        Label        lblFirewireModelId;
        TextBox      txtFirewireModelId;
        Label        lblFirewireManufacturer;
        TextBox      txtFirewireManufacturer;
        Label        lblFirewireModel;
        TextBox      txtFirewireModel;
        Label        lblFirewireGuid;
        TextBox      txtFirewireGuid;
        TabPage      tabPcmcia;
        TreeGridView treePcmcia;
        TextArea     txtPcmciaCis;
        Button       btnSavePcmciaCis;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}