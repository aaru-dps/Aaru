// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : tabDvdInfo.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Media information.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the DVD media information.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Core.Media.Info;
using DiscImageChef.Decoders.Bluray;
using DiscImageChef.Decoders.DVD;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Decoders.SCSI.SSC;
using DiscImageChef.Decoders.Xbox;
using DiscImageChef.Gui.Controls;
using DiscImageChef.Gui.Forms;
using DiscImageChef.Gui.Tabs;
using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;
using BCA = DiscImageChef.Decoders.Bluray.BCA;
using Cartridge = DiscImageChef.Decoders.DVD.Cartridge;
using DDS = DiscImageChef.Decoders.DVD.DDS;
using DMI = DiscImageChef.Decoders.Xbox.DMI;
using Spare = DiscImageChef.Decoders.DVD.Spare;

namespace DiscImageChef.Gui.Tabs
{
    public class tabDvdInfo : TabPage
    {
        byte[] DvdPfi;
        byte[] DvdDmi;
        byte[] DvdCmi;
        byte[] HddvdCopyrightInformation;
        byte[] DvdBca;
        byte[] DvdAacs;

        public tabDvdInfo()
        {
            XamlReader.Load(this);
        }

        internal void LoadData(MediaType mediaType, byte[] pfi, byte[] dmi, byte[] cmi, byte[] hdCopyrightInformation, byte[] bca, byte[] aacs, PFI.PhysicalFormatInformation? decodedPfi)
        {
            DvdPfi=pfi;
            DvdDmi=dmi;
            DvdCmi=cmi;
            HddvdCopyrightInformation=hdCopyrightInformation;
            DvdBca=bca;
            DvdAacs=aacs;

            switch(mediaType)
            {
                case MediaType.HDDVDROM:
                case MediaType.HDDVDRAM:
                case MediaType.HDDVDR:
                case MediaType.HDDVDRW:
                case MediaType.HDDVDRDL:
                case MediaType.HDDVDRWDL:
                    Text = "HD DVD";
                    break;
                default:
                    Text = "DVD";
                    break;
            }

            if(decodedPfi.HasValue)
            {
                grpDvdPfi.Visible = true;
                txtDvdPfi.Text    = PFI.Prettify(decodedPfi);
            }

            if(cmi != null)
            {
                grpDvdCmi.Visible     = true;
                txtDvdCmi.Text        = CSS_CPRM.PrettifyLeadInCopyright(cmi);
                btnSaveDvdCmi.Visible = true;
            }

            btnSaveDvdPfi.Visible   = pfi                    != null;
            btnSaveDvdDmi.Visible   = dmi                    != null;
            btnSaveDvdCmi.Visible   = cmi                    != null;
            btnSaveHdDvdCmi.Visible = hdCopyrightInformation != null;
            btnSaveDvdBca.Visible   = bca                    != null;
            btnSaveDvdAacs.Visible  = aacs                   != null;

            Visible = grpDvdPfi.Visible     || grpDvdCmi.Visible || btnSaveDvdPfi.Visible ||
                             btnSaveDvdDmi.Visible ||
                             btnSaveDvdCmi.Visible || btnSaveHdDvdCmi.Visible || btnSaveDvdBca.Visible ||
                             btnSaveDvdAacs.Visible;
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

        protected void OnBtnSaveDvdPfiClick(object sender, EventArgs e)
        {
            SaveElement(DvdPfi);
        }

        protected void OnBtnSaveDvdDmiClick(object sender, EventArgs e)
        {
            SaveElement(DvdDmi);
        }

        protected void OnBtnSaveDvdCmiClick(object sender, EventArgs e)
        {
            SaveElement(DvdCmi);
        }

        protected void OnBtnSaveHdDvdCmiClick(object sender, EventArgs e)
        {
            SaveElement(HddvdCopyrightInformation);
        }

        protected void OnBtnSaveDvdBcaClick(object sender, EventArgs e)
        {
            SaveElement(DvdBca);
        }

        protected void OnBtnSaveDvdAacsClick(object sender, EventArgs e)
        {
            SaveElement(DvdAacs);
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
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
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}