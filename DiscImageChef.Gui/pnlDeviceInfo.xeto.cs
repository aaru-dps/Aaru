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
using System.Linq;
using DiscImageChef.Core.Devices.Info;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.SCSI;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui
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
                tabSCSI.Visible     = true;
                txtScsiInquiry.Text = Inquiry.Prettify(devInfo.ScsiInquiry);

                if(devInfo.ScsiMode.HasValue)
                {
                    tabScsiModeSense.Visible      = true;
                    treeModeSensePages.ShowHeader = false;

                    TreeGridItemCollection modePagesList = new TreeGridItemCollection();

                    treeModeSensePages.Columns.Add(new GridColumn {HeaderText = "Page", DataCell = new TextBoxCell(0)});

                    treeModeSensePages.AllowMultipleSelection = false;
                    treeModeSensePages.ShowHeader             = false;
                    treeModeSensePages.DataStore              = modePagesList;

                    modePagesList.Add(new TreeGridItem
                    {
                        Values = new object[]
                        {
                            "Header",
                            Modes.PrettifyModeHeader(devInfo.ScsiMode.Value.Header,
                                                     devInfo.ScsiType)
                        }
                    });

                    foreach(Modes.ModePage page in devInfo
                                                  .ScsiMode.Value.Pages.OrderBy(t => t.Page).ThenBy(t => t.Subpage))
                    {
                        string pageNumberText = page.Subpage == 0
                                                    ? $"MODE {page.Page:X2}h"
                                                    : $"MODE {page.Page:X2} Subpage {page.Subpage:X2}";
                        string decodedText;

                        switch(page.Page)
                        {
                            case 0x00:
                            {
                                if(devInfo.ScsiType == PeripheralDeviceTypes.MultiMediaDevice && page.Subpage == 0)
                                    decodedText  = Modes.PrettifyModePage_00_SFF(page.PageResponse);
                                else decodedText = "Undecoded";

                                break;
                            }
                            case 0x01:
                            {
                                if(page.Subpage == 0)
                                    decodedText = devInfo.ScsiType == PeripheralDeviceTypes.MultiMediaDevice
                                                      ? Modes.PrettifyModePage_01_MMC(page.PageResponse)
                                                      : Modes.PrettifyModePage_01(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x02:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_02(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x03:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_03(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x04:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_04(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x05:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_05(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x06:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_06(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x07:
                            {
                                if(page.Subpage == 0)
                                    decodedText = devInfo.ScsiType == PeripheralDeviceTypes.MultiMediaDevice
                                                      ? Modes.PrettifyModePage_07_MMC(page.PageResponse)
                                                      : Modes.PrettifyModePage_07(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x08:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_08(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0A:
                            {
                                if(page.Subpage      == 0) decodedText = Modes.PrettifyModePage_0A(page.PageResponse);
                                else if(page.Subpage == 1)
                                    decodedText = Modes.PrettifyModePage_0A_S01(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0B:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_0B(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0D:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_0D(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0E:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_0E(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x0F:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_0F(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x10:
                            {
                                if(page.Subpage == 0)
                                    decodedText = devInfo.ScsiType == PeripheralDeviceTypes.SequentialAccess
                                                      ? Modes.PrettifyModePage_10_SSC(page.PageResponse)
                                                      : Modes.PrettifyModePage_10(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x11:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_11(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x12:
                            case 0x13:
                            case 0x14:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_12_13_14(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x1A:
                            {
                                if(page.Subpage      == 0) decodedText = Modes.PrettifyModePage_1A(page.PageResponse);
                                else if(page.Subpage == 1)
                                    decodedText = Modes.PrettifyModePage_1A_S01(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x1B:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_1B(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x1C:
                            {
                                if(page.Subpage == 0)
                                    decodedText = devInfo.ScsiType == PeripheralDeviceTypes.MultiMediaDevice
                                                      ? Modes.PrettifyModePage_1C_SFF(page.PageResponse)
                                                      : Modes.PrettifyModePage_1C(page.PageResponse);
                                else if(page.Subpage == 1)
                                    decodedText = Modes.PrettifyModePage_1C_S01(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x1D:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_1D(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x21:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() ==
                                   "CERTANCE") decodedText = Modes.PrettifyCertanceModePage_21(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x22:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() ==
                                   "CERTANCE") decodedText = Modes.PrettifyCertanceModePage_22(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x24:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() == "IBM")
                                    decodedText = Modes.PrettifyIBMModePage_24(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x2A:
                            {
                                if(page.Subpage == 0) decodedText = Modes.PrettifyModePage_2A(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x2F:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() == "IBM")
                                    decodedText = Modes.PrettifyIBMModePage_2F(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x30:
                            {
                                if(Modes.IsAppleModePage_30(page.PageResponse))
                                    decodedText = "Drive identifies as Apple OEM drive";
                                else goto default;

                                break;
                            }
                            case 0x3B:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() == "HP")
                                    decodedText = Modes.PrettifyHPModePage_3B(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x3C:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() == "HP")
                                    decodedText = Modes.PrettifyHPModePage_3C(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x3D:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() == "IBM")
                                    decodedText = Modes.PrettifyIBMModePage_3D(page.PageResponse);
                                else if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() ==
                                        "HP") decodedText = Modes.PrettifyHPModePage_3D(page.PageResponse);
                                else goto default;

                                break;
                            }
                            case 0x3E:
                            {
                                if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() ==
                                   "FUJITSU") decodedText = Modes.PrettifyFujitsuModePage_3E(page.PageResponse);
                                else if(StringHandlers.CToString(devInfo.ScsiInquiry?.VendorIdentification).Trim() ==
                                        "HP") decodedText = Modes.PrettifyHPModePage_3E(page.PageResponse);
                                else goto default;

                                break;
                            }
                            default:
                            {
                                decodedText = "Undecoded";
                                break;
                            }
                        }

                        // TODO: Automatic error reporting
                        if(decodedText == null) decodedText = "Error decoding page, please open an issue.";
                        modePagesList.Add(new TreeGridItem {Values = new object[] {pageNumberText, decodedText}});
                    }
                }
            }
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

        protected void OnBtnSaveInquiryBinary(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(devInfo.ScsiInquiryData, 0, devInfo.ScsiInquiryData.Length);

            saveFs.Close();
        }

        protected void OnBtnSaveInquiryText(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveText = new SaveFileDialog();
            dlgSaveText.Filters.Add(new FileFilter {Extensions = new[] {"*.txt"}, Name = "Text"});
            DialogResult result = dlgSaveText.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream   saveFs = new FileStream(dlgSaveText.FileName, FileMode.Create);
            StreamWriter saveSw = new StreamWriter(saveFs);
            saveSw.Write(txtScsiInquiry.Text);
            saveFs.Close();
        }

        protected void OnBtnSaveMode6(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(devInfo.ScsiModeSense6, 0, devInfo.ScsiModeSense6.Length);

            saveFs.Close();
        }

        protected void OnBtnSaveMode10(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(devInfo.ScsiModeSense10, 0, devInfo.ScsiModeSense10.Length);

            saveFs.Close();
        }

        protected void OnTreeModePagesSelectedItemChanged(object sender, EventArgs e)
        {
            if(!(treeModeSensePages.SelectedItem is TreeGridItem item)) return;

            txtModeSensePage.Text = item.Values[1] as string;
        }

        #region XAML controls
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
        TabPage      tabSCSI;
        TabPage      tabScsiInquiry;
        Label        lblScsiInquiry;
        TextArea     txtScsiInquiry;
        Button       btnSaveInquiryBinary;
        Button       btnSaveInquiryText;
        TabPage      tabScsiModeSense;
        TreeGridView treeModeSensePages;
        TextArea     txtModeSensePage;
        Button       btnSaveMode6;
        Button       btnSaveMode10;
        #endregion
    }
}