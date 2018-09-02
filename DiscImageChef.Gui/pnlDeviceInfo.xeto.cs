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
using DiscImageChef.Core.Devices.Info;
using DiscImageChef.Decoders.ATA;
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

        #region XAML controls
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
        TabPage     tabAta;
        Label       lblAtaIdentify;
        TextArea    txtAtaIdentify;
        Button      btnSaveAtaBinary;
        Button      btnSaveAtaText;
        StackLayout stkAtaMcpt;
        CheckBox    chkAtaMcpt;
        Label       lblAtaMcpt;
        CheckBox    chkAtaMcptWriteProtection;
        Label       lblAtaMcptSpecificData;
        #endregion
    }
}