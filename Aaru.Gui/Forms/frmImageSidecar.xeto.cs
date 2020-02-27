// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using Claunia.Encoding;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Eto.Forms;
using Eto.Serialization.Xaml;
using Schemas;

namespace Aaru.Gui.Forms
{
    public class frmImageSidecar : Form
    {
        readonly Encoding    encoding;
        readonly Guid        filterId;
        readonly string      imageSource;
        readonly IMediaImage inputFormat;

        Sidecar sidecarClass;

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
                btnStop.Enabled        = true;
                stkProgress.Visible    = true;
                btnDestination.Visible = false;
                lblStatus.Visible      = true;
            });

            sidecarClass                      =  new Sidecar(inputFormat, imageSource, filterId, encoding);
            sidecarClass.UpdateStatusEvent    += UpdateStatus;
            sidecarClass.InitProgressEvent    += InitProgress;
            sidecarClass.UpdateProgressEvent  += UpdateProgress;
            sidecarClass.EndProgressEvent     += EndProgress;
            sidecarClass.InitProgressEvent2   += InitProgress2;
            sidecarClass.UpdateProgressEvent2 += UpdateProgress2;
            sidecarClass.EndProgressEvent2    += EndProgress2;
            CICMMetadataType sidecar = sidecarClass.Create();

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
                lblStatus.Visible   = false;
            });

            Statistics.AddCommand("create-sidecar");
        }

        void EndProgress2()
        {
            Application.Instance.Invoke(() => { stkProgress2.Visible = false; });
        }

        void UpdateProgress2(string text, long current, long maximum)
        {
            Application.Instance.Invoke(() =>
            {
                lblProgress2.Text          = text;
                prgProgress2.Indeterminate = false;
                prgProgress2.MinValue      = 0;
                if(maximum > int.MaxValue)
                {
                    prgProgress2.MaxValue = (int)(maximum / int.MaxValue);
                    prgProgress2.Value    = (int)(current / int.MaxValue);
                }
                else
                {
                    prgProgress2.MaxValue = (int)maximum;
                    prgProgress2.Value    = (int)current;
                }
            });
        }

        void InitProgress2()
        {
            Application.Instance.Invoke(() => { stkProgress2.Visible = true; });
        }

        void EndProgress()
        {
            Application.Instance.Invoke(() => { stkProgress1.Visible = false; });
        }

        void UpdateProgress(string text, long current, long maximum)
        {
            Application.Instance.Invoke(() =>
            {
                lblProgress.Text          = text;
                prgProgress.Indeterminate = false;
                prgProgress.MinValue      = 0;
                if(maximum > int.MaxValue)
                {
                    prgProgress.MaxValue = (int)(maximum / int.MaxValue);
                    prgProgress.Value    = (int)(current / int.MaxValue);
                }
                else
                {
                    prgProgress.MaxValue = (int)maximum;
                    prgProgress.Value    = (int)current;
                }
            });
        }

        void InitProgress()
        {
            Application.Instance.Invoke(() => { stkProgress1.Visible = true; });
        }

        void UpdateStatus(string text)
        {
            Application.Instance.Invoke(() => { lblStatus.Text = text; });
        }

        protected void OnBtnClose(object sender, EventArgs e)
        {
            Close();
        }

        protected void OnBtnStop(object sender, EventArgs e)
        {
            lblProgress.Text = "Aborting...";
            btnStop.Enabled  = false;
            sidecarClass.Abort();
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
        Label       lblStatus;
        #endregion
    }
}