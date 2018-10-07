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
using DiscImageChef.Core.Media.Info;
using DiscImageChef.Decoders.Xbox;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Panels
{
    public class tabXboxInfo : TabPage
    {
        byte[] xboxSecuritySector;

        public tabXboxInfo()
        {
            XamlReader.Load(this);
        }

        internal void LoadData(XgdInfo            xgdInfo, byte[] dmi, byte[] securitySector,
                               SS.SecuritySector? decodedSecuritySector)
        {
            xboxSecuritySector = securitySector;

            if(xgdInfo != null)
            {
                stkXboxInformation.Visible = true;
                txtXboxL0Video.Text        = $"{xgdInfo.L0Video} sectors";
                txtXboxL1Video.Text        = $"{xgdInfo.L1Video} sectors";
                txtXboxMiddleZone.Text     = $"{xgdInfo.MiddleZone} sectors";
                txtXboxGameSize.Text       = $"{xgdInfo.GameSize} sectors";
                txtXboxTotalSize.Text      = $"{xgdInfo.TotalSize} sectors";
                txtXboxRealBreak.Text      = xgdInfo.LayerBreak.ToString();
            }

            if(dmi != null)
            {
                if(DMI.IsXbox(dmi))
                {
                    grpXboxDmi.Visible = true;
                    txtXboxDmi.Text    = DMI.PrettifyXbox(dmi);
                }
                else if(DMI.IsXbox360(dmi))
                {
                    grpXboxDmi.Visible = true;
                    txtXboxDmi.Text    = DMI.PrettifyXbox360(dmi);
                }
            }

            if(decodedSecuritySector.HasValue)
            {
                grpXboxSs.Visible = true;
                txtXboxSs.Text    = SS.Prettify(decodedSecuritySector);
            }

            btnSaveXboxSs.Visible = securitySector != null;
            Visible = stkXboxInformation.Visible || grpXboxDmi.Visible || grpXboxSs.Visible ||
                                    btnSaveXboxSs.Visible;
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

        protected void OnBtnSaveXboxSsClick(object sender, EventArgs e)
        {
            SaveElement(xboxSecuritySector);
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        StackLayout stkXboxInformation;
        Label       lblXboxL0Video;
        TextBox     txtXboxL0Video;
        Label       lblXboxL1Video;
        TextBox     txtXboxL1Video;
        Label       lblXboxMiddleZone;
        TextBox     txtXboxMiddleZone;
        Label       lblXboxGameSize;
        TextBox     txtXboxGameSize;
        Label       lblXboxTotalSize;
        TextBox     txtXboxTotalSize;
        Label       lblXboxRealBreak;
        TextBox     txtXboxRealBreak;
        GroupBox    grpXboxDmi;
        TextArea    txtXboxDmi;
        GroupBox    grpXboxSs;
        TextArea    txtXboxSs;
        Button      btnSaveXboxSs;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}