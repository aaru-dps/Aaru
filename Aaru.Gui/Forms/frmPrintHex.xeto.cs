// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : frmPrintHex.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Form to show hexadecimal dumps of sectors.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements hexadecimal sector viewer.
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
using Aaru.CommonTypes.Interfaces;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace Aaru.Gui.Forms
{
    // TODO: Decode long sector components
    // TODO: Panel with string representation of contents
    public class frmPrintHex : Form
    {
        const int   HEX_COLUMNS = 32;
        IMediaImage inputFormat;

        public frmPrintHex(IMediaImage inputFormat)
        {
            this.inputFormat = inputFormat;
            XamlReader.Load(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                inputFormat.ReadSectorLong(0);
                chkLongSector.Checked = true;
            }
            catch { chkLongSector.Visible = false; }

            lblSectors.Text = $"of {inputFormat.Info.Sectors}";
            nmuSector.Value = 0;
            OnNmuSectorValueChanged(nmuSector, e);
        }

        void OnNmuSectorValueChanged(object sender, EventArgs e)
        {
            txtPrintHex.Text =
                PrintHex
                   .ByteArrayToHexArrayString(chkLongSector.Checked == true ? inputFormat.ReadSectorLong((ulong)nmuSector.Value) : inputFormat.ReadSector((ulong)nmuSector.Value),
                                              HEX_COLUMNS);
        }

        #region XAML IDs
        Label          lblSector;
        NumericStepper nmuSector;
        Label          lblSectors;
        CheckBox       chkLongSector;
        TextArea       txtPrintHex;
        #endregion
    }
}