// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.Gui.Tabs;
using Aaru.Decoders.SCSI.SSC;
using Aaru.Devices;
using Eto.Forms;
using Eto.Serialization.Xaml;
using DeviceInfo = Aaru.Core.Devices.Info.DeviceInfo;

namespace Aaru.Gui.Panels
{
    public class pnlDeviceInfo : Panel
    {
        readonly DeviceInfo devInfo;

        public pnlDeviceInfo(DeviceInfo devInfo)
        {
            XamlReader.Load(this);

            this.devInfo = devInfo;

            txtType.Text         = devInfo.Type.ToString();
            txtManufacturer.Text = devInfo.Manufacturer;
            txtModel.Text        = devInfo.Model;
            txtRevision.Text     = devInfo.FirmwareRevision;
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
                var tabPcmciaInfo = new tabPcmciaInfo();
                tabPcmciaInfo.LoadData(devInfo.Cis);
                tabInfos.Pages.Add(tabPcmciaInfo);
            }

            if(devInfo.AtaIdentify   != null ||
               devInfo.AtapiIdentify != null)
            {
                var tabAtaInfo = new tabAtaInfo();
                tabAtaInfo.LoadData(devInfo.AtaIdentify, devInfo.AtapiIdentify, devInfo.AtaMcptError);

                tabInfos.Pages.Add(tabAtaInfo);
            }

            if(devInfo.ScsiInquiryData != null)
            {
                var tabScsiInfo = new tabScsiInfo();

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
                                    ? $"{devInfo.PlextorFeatures.CdReadSpeedLimit}x" : "unlimited";

                            txtPlextorSilentModeCdWriteSpeedLimit.Text =
                                devInfo.PlextorFeatures.CdWriteSpeedLimit > 0
                                    ? $"{devInfo.PlextorFeatures.CdReadSpeedLimit}x" : "unlimited";

                            if(devInfo.PlextorFeatures.IsDvd)
                            {
                                stkPlextorSilentModeDvdReadSpeedLimit.Visible = true;

                                txtPlextorSilentModeDvdReadSpeedLimit.Text =
                                    devInfo.PlextorFeatures.DvdReadSpeedLimit > 0
                                        ? $"{devInfo.PlextorFeatures.DvdReadSpeedLimit}x" : "unlimited";
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

                if(devInfo.ScsiInquiry?.KreonPresent == true)
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

            var tabSdMmcInfo = new tabSdMmcInfo();

            tabSdMmcInfo.LoadData(devInfo.Type, devInfo.CID, devInfo.CSD, devInfo.OCR, devInfo.ExtendedCSD,
                                  devInfo.SCR);

            tabInfos.Pages.Add(tabSdMmcInfo);
        }

        protected void OnBtnSaveUsbDescriptors(object sender, EventArgs e)
        {
            var dlgSaveBinary = new SaveFileDialog();

            dlgSaveBinary.Filters.Add(new FileFilter
            {
                Extensions = new[]
                {
                    "*.bin"
                },
                Name = "Binary"
            });

            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok)
                return;

            var saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(devInfo.UsbDescriptors, 0, devInfo.UsbDescriptors.Length);

            saveFs.Close();
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        Label       lblDeviceInfo;
        TabControl  tabInfos;
        TabPage     tabGeneral;
        Label       lblType;
        TextBox     txtType;
        Label       lblManufacturer;
        TextBox     txtManufacturer;
        Label       lblModel;
        TextBox     txtModel;
        Label       lblRevision;
        TextBox     txtRevision;
        Label       lblSerial;
        TextBox     txtSerial;
        Label       lblScsiType;
        TextBox     txtScsiType;
        CheckBox    chkRemovable;
        CheckBox    chkUsb;
        TabPage     tabKreon;
        CheckBox    chkKreonChallengeResponse;
        CheckBox    chkKreonDecryptSs;
        CheckBox    chkKreonXtremeUnlock;
        CheckBox    chkKreonWxripperUnlock;
        CheckBox    chkKreonChallengeResponse360;
        CheckBox    chkKreonDecryptSs360;
        CheckBox    chkKreonXtremeUnlock360;
        CheckBox    chkKreonWxripperUnlock360;
        CheckBox    chkKreonLock;
        CheckBox    chkKreonErrorSkipping;
        TabPage     tabPlextor;
        StackLayout stkPlextorEeprom;
        Label       lblPlextorDiscs;
        TextBox     txtPlextorDiscs;
        Label       lblPlextorCdReadTime;
        TextBox     txtPlextorCdReadTime;
        Label       lblPlextorCdWriteTime;
        TextBox     txtPlextorCdWriteTime;
        StackLayout stkPlextorDvdTimes;
        Label       lblPlextorDvdReadTime;
        TextBox     txtPlextorDvdReadTime;
        Label       lblPlextorDvdWriteTime;
        TextBox     txtPlextorDvdWriteTime;
        CheckBox    chkPlextorPoweRec;
        CheckBox    chkPlextorPoweRecEnabled;
        StackLayout stkPlextorPoweRecEnabled;
        StackLayout stkPlextorPoweRecRecommended;
        Label       lblPlextorPoweRecRecommended;
        TextBox     txtPlextorPoweRecRecommended;
        StackLayout stkPlextorPoweRecSelected;
        Label       lblPlextorPoweRecSelected;
        TextBox     txtPlextorPoweRecSelected;
        StackLayout stkPlextorPoweRecMax;
        Label       lblPlextorPoweRecMax;
        TextBox     txtPlextorPoweRecMax;
        StackLayout stkPlextorPoweRecLast;
        Label       lblPlextorPoweRecLast;
        TextBox     txtPlextorPoweRecLast;
        CheckBox    chkPlextorSilentMode;
        CheckBox    chkPlextorSilentModeEnabled;
        StackLayout stkPlextorSilentModeEnabled;
        Label       lblPlextorSilentModeAccessTime;
        Label       lblPlextorSilentModeCdReadSpeedLimit;
        TextBox     txtPlextorSilentModeCdReadSpeedLimit;
        StackLayout stkPlextorSilentModeDvdReadSpeedLimit;
        Label       lblPlextorSilentModeDvdReadSpeedLimit;
        TextBox     txtPlextorSilentModeDvdReadSpeedLimit;
        Label       lblPlextorSilentModeCdWriteSpeedLimit;
        TextBox     txtPlextorSilentModeCdWriteSpeedLimit;
        CheckBox    chkPlextorGigaRec;
        CheckBox    chkPlextorSecuRec;
        CheckBox    chkPlextorSpeedRead;
        CheckBox    chkPlextorSpeedEnabled;
        CheckBox    chkPlextorHiding;
        StackLayout stkPlextorHiding;
        CheckBox    chkPlextorHidesRecordables;
        CheckBox    chkPlextorHidesSessions;
        CheckBox    chkPlextorVariRec;
        StackLayout stkPlextorDvd;
        CheckBox    chkPlextorVariRecDvd;
        CheckBox    chkPlextorBitSetting;
        CheckBox    chkPlextorBitSettingDl;
        CheckBox    chkPlextorDvdPlusWriteTest;
        TabPage     tabSsc;
        Label       lblMinBlockSize;
        Label       lblMaxBlockSize;
        Label       lblBlockSizeGranularity;
        StackLayout stkDensities;
        Label       lblDensities;
        TextArea    txtDensities;
        StackLayout stkMediaTypes;
        Label       lblMediumTypes;
        TextArea    txtMediumTypes;
        TextArea    txtMediumDensity;
        TabPage     tabUsb;
        Label       lblUsbVendorId;
        TextBox     txtUsbVendorId;
        Label       lblUsbProductId;
        TextBox     txtUsbProductId;
        Label       lblUsbManufacturer;
        TextBox     txtUsbManufacturer;
        Label       lblUsbProduct;
        TextBox     txtUsbProduct;
        Label       lblUsbSerial;
        TextBox     txtUsbSerial;
        Button      btnSaveUsbDescriptors;
        TabPage     tabFirewire;
        Label       lblFirewireVendorId;
        TextBox     txtFirewireVendorId;
        Label       lblFirewireModelId;
        TextBox     txtFirewireModelId;
        Label       lblFirewireManufacturer;
        TextBox     txtFirewireManufacturer;
        Label       lblFirewireModel;
        TextBox     txtFirewireModel;
        Label       lblFirewireGuid;
        TextBox     txtFirewireGuid;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}