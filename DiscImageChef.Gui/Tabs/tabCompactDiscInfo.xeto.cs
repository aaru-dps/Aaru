// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : tabCompactDiscInfo.xeto..cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Media information.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the Compact Disc media information.
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
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.SCSI.MMC;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Tabs
{
    public class tabCompactDiscInfo : TabPage
    {
        byte[] atipData;
        byte[] cdTextLeadInData;
        byte[] compactDiscInformationData;
        byte[] pmaData;
        byte[] rawTocData;
        byte[] sessionData;
        byte[] tocData;

        public tabCompactDiscInfo()
        {
            XamlReader.Load(this);
        }

        internal void LoadData(byte[]                                   toc,                    byte[]             atip,
                               byte[]                                   compactDiscInformation, byte[]             session, byte[] rawToc,
                               byte[]                                   pma,                    byte[]             cdTextLeadIn,
                               TOC.CDTOC?                               decodedToc,             ATIP.CDATIP?       decodedAtip,
                               Session.CDSessionInfo?                   decodedSession,         FullTOC.CDFullTOC? fullToc,
                               CDTextOnLeadIn.CDText?                   decodedCdTextLeadIn,
                               DiscInformation.StandardDiscInformation? decodedCompactDiscInformation, string mcn,
                               Dictionary<byte, string>                 isrcs)
        {
            tocData                    = toc;
            atipData                   = atip;
            compactDiscInformationData = compactDiscInformation;
            sessionData                = session;
            rawTocData                 = rawToc;
            pmaData                    = pma;
            cdTextLeadInData           = cdTextLeadIn;

            if(decodedCompactDiscInformation.HasValue)
            {
                tabCdInformation.Visible = true;
                txtCdInformation.Text    = DiscInformation.Prettify000b(decodedCompactDiscInformation);
                btnCdInformation.Visible = compactDiscInformation != null;
            }

            if(decodedToc.HasValue)
            {
                tabCdToc.Visible = true;
                txtCdToc.Text    = TOC.Prettify(decodedToc);
                btnCdToc.Visible = toc != null;
            }

            if(fullToc.HasValue)
            {
                tabCdFullToc.Visible = true;
                txtCdFullToc.Text    = FullTOC.Prettify(fullToc);
                btnCdFullToc.Visible = rawToc != null;
            }

            if(decodedSession.HasValue)
            {
                tabCdSession.Visible = true;
                txtCdSession.Text    = Session.Prettify(decodedSession);
                btnCdSession.Visible = session != null;
            }

            if(decodedCdTextLeadIn.HasValue)
            {
                tabCdText.Visible = true;
                txtCdText.Text    = CDTextOnLeadIn.Prettify(decodedCdTextLeadIn);
                btnCdText.Visible = cdTextLeadIn != null;
            }

            if(decodedAtip.HasValue)
            {
                tabCdAtip.Visible = true;
                txtCdAtip.Text    = ATIP.Prettify(atip);
                btnCdAtip.Visible = atip != null;
            }

            if(!string.IsNullOrEmpty(mcn))
            {
                stkMcn.Visible = true;
                txtMcn.Text    = mcn;
            }

            if(isrcs != null && isrcs.Count > 0)
            {
                grpIsrcs.Visible = true;

                TreeGridItemCollection isrcsItems = new TreeGridItemCollection();

                grdIsrcs.Columns.Add(new GridColumn {HeaderText = "ISRC", DataCell  = new TextBoxCell(0)});
                grdIsrcs.Columns.Add(new GridColumn {HeaderText = "Track", DataCell = new TextBoxCell(0)});

                grdIsrcs.AllowMultipleSelection = false;
                grdIsrcs.ShowHeader             = true;
                grdIsrcs.DataStore              = isrcsItems;

                foreach(KeyValuePair<byte, string> isrc in isrcs)
                    isrcsItems.Add(new TreeGridItem {Values = new object[] {isrc.Key.ToString(), isrc.Value}});
            }

            btnCdPma.Visible = pma != null;

            tabCdMisc.Visible = stkMcn.Visible || grpIsrcs.Visible || btnCdPma.Visible;

            Visible = tabCdInformation.Visible || tabCdToc.Visible  || tabCdFullToc.Visible || tabCdSession.Visible ||
                      tabCdText.Visible        || tabCdAtip.Visible || stkMcn.Visible       || grpIsrcs.Visible     ||
                      btnCdPma.Visible;
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

        protected void OnBtnCdInformationClick(object sender, EventArgs e)
        {
            SaveElement(compactDiscInformationData);
        }

        protected void OnBtnCdTocClick(object sender, EventArgs e)
        {
            SaveElement(tocData);
        }

        protected void OnBtnCdFullTocClick(object sender, EventArgs e)
        {
            SaveElement(rawTocData);
        }

        protected void OnBtnCdSessionClick(object sender, EventArgs e)
        {
            SaveElement(sessionData);
        }

        protected void OnBtnCdTextClick(object sender, EventArgs e)
        {
            SaveElement(cdTextLeadInData);
        }

        protected void OnBtnCdAtipClick(object sender, EventArgs e)
        {
            SaveElement(atipData);
        }

        protected void OnBtnCdPmaClick(object sender, EventArgs e)
        {
            SaveElement(pmaData);
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
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
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}