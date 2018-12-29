// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : frmImageSidecar.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Image sidecar creation window.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements creating image metadata sidecar.
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
//     along with this program.  If not, see ;http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using Claunia.Encoding;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Core;
using Eto.Forms;
using Eto.Serialization.Xaml;
using Schemas;

namespace DiscImageChef.Gui.Forms
{
    public class frmImageSidecar : Form
    {
        Encoding    encoding;
        Guid        filterId;
        string      imageSource;
        IMediaImage inputFormat;

        public frmImageSidecar(IMediaImage inputFormat, string imageSource, Guid filterId, Encoding encoding)
        {
            this.inputFormat = inputFormat;
            this.imageSource = imageSource;
            this.filterId    = filterId;
            this.encoding    = encoding;
            XamlReader.Load(this);

            txtDestination.Text = Path.Combine(Path.GetDirectoryName(imageSource) ?? "",
                                               Path.GetFileNameWithoutExtension(imageSource) + ".cicm.xml");
        }

        protected void OnBtnStart(object sender, EventArgs e)
        {
            new Thread(DoWork).Start();
        }

        void DoWork()
        {
            // Prepare UI
            Application.Instance.Invoke(() =>
            {
                btnClose.Visible       = false;
                btnStart.Visible       = false;
                btnStop.Visible        = true;
                stkProgress.Visible    = false;
                btnStop.Enabled        = false;
                btnDestination.Visible = false;
            });

            CICMMetadataType sidecar = Sidecar.Create(inputFormat, imageSource, filterId, encoding);

            DicConsole.WriteLine("Writing metadata sidecar");

            FileStream xmlFs = new FileStream(txtDestination.Text, FileMode.Create);

            XmlSerializer xmlSer = new XmlSerializer(typeof(CICMMetadataType));
            xmlSer.Serialize(xmlFs, sidecar);
            xmlFs.Close();

            Application.Instance.Invoke(() =>
            {
                btnClose.Visible    = true;
                btnStop.Visible     = false;
                stkProgress.Visible = false;
            });

            Statistics.AddCommand("create-sidecar");
        }

        protected void OnBtnClose(object sender, EventArgs e)
        {
            Close();
        }

        protected void OnBtnStop(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void OnBtnDestinationClick(object sender, EventArgs e)
        {
            SaveFileDialog dlgDestination = new SaveFileDialog {Title = "Choose destination file"};
            dlgDestination.Filters.Add(new FileFilter("CICM XML metadata", "*.xml"));

            DialogResult result = dlgDestination.ShowDialog(this);

            if(result != DialogResult.Ok)
            {
                txtDestination.Text = "";
                return;
            }

            if(string.IsNullOrEmpty(Path.GetExtension(dlgDestination.FileName))) dlgDestination.FileName += ".xml";

            txtDestination.Text = dlgDestination.FileName;
        }

        #region XAML IDs
        TextBox     txtDestination;
        Button      btnDestination;
        StackLayout stkProgress;
        StackLayout stkProgress1;
        Label       lblProgress;
        ProgressBar prgProgress;
        StackLayout stkProgress2;
        Label       lblProgress2;
        ProgressBar prgProgress2;
        Button      btnStart;
        Button      btnClose;
        Button      btnStop;
        #endregion
    }
}