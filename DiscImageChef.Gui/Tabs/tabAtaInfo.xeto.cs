// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : tabAtaInfo.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device information.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the ATA device information.
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
using DiscImageChef.Decoders.ATA;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Tabs
{
    public class tabAtaInfo : TabPage
    {
        byte[] ata;
        byte[] atapi;

        public tabAtaInfo()
        {
            XamlReader.Load(this);
        }

        internal void LoadData(byte[] ataIdentify, byte[] atapiIdentify, AtaErrorRegistersChs? ataMcptError)
        {
            ata   = ataIdentify;
            atapi = atapiIdentify;

            if(ataIdentify == null && atapiIdentify == null) return;

            Visible = true;

            if(ataIdentify != null)
            {
                stkAtaMcpt.Visible  = false;
                chkAtaMcpt.Checked  = ataMcptError.HasValue;
                lblAtaMcpt.Visible  = ataMcptError.HasValue;
                lblAtaIdentify.Text = "ATA IDENTIFY DEVICE";

                if(ataMcptError.HasValue)
                {
                    switch(ataMcptError.Value.DeviceHead & 0x7)
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
                                $"Device contains unknown media card type {ataMcptError.Value.DeviceHead & 0x07}";
                            break;
                    }

                    chkAtaMcptWriteProtection.Checked = (ataMcptError.Value.DeviceHead & 0x08) == 0x08;

                    ushort specificData = (ushort)(ataMcptError.Value.CylinderHigh * 0x100 +
                                                   ataMcptError.Value.CylinderLow);
                    if(specificData != 0)
                    {
                        lblAtaMcptSpecificData.Visible = true;
                        lblAtaMcptSpecificData.Text    = $"Card specific data: 0x{specificData:X4}";
                    }
                }

                Text                = "ATA";
                txtAtaIdentify.Text = Identify.Prettify(ata);
            }
            else
            {
                lblAtaIdentify.Text = "ATA PACKET IDENTIFY DEVICE";
                stkAtaMcpt.Visible  = false;
                Text                = "ATAPI";
                txtAtaIdentify.Text = Identify.Prettify(atapi);
            }
        }

        protected void OnBtnSaveAtaBinary(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            if(ata        != null) saveFs.Write(ata,   0, ata.Length);
            else if(atapi != null) saveFs.Write(atapi, 0, atapi.Length);

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
        #pragma warning disable 169
        #pragma warning disable 649
        Label       lblAtaIdentify;
        TextArea    txtAtaIdentify;
        Button      btnSaveAtaBinary;
        Button      btnSaveAtaText;
        StackLayout stkAtaMcpt;
        CheckBox    chkAtaMcpt;
        Label       lblAtaMcpt;
        CheckBox    chkAtaMcptWriteProtection;
        Label       lblAtaMcptSpecificData;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}