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
using DiscImageChef.Console;
using DiscImageChef.Decoders.PCMCIA;
using Eto.Forms;
using Eto.Serialization.Xaml;
using Tuple = DiscImageChef.Decoders.PCMCIA.Tuple;

namespace DiscImageChef.Gui.Panels
{
    public class tabPcmciaInfo : TabPage
    {
        byte[] cis;

        public tabPcmciaInfo()
        {
            XamlReader.Load(this);
        }

        internal void LoadData(byte[] pcmciaCis)
        {
            if(pcmciaCis == null) return;

            cis     = pcmciaCis;
            Visible = true;

            TreeGridItemCollection cisList = new TreeGridItemCollection();

            treePcmcia.Columns.Add(new GridColumn {HeaderText = "CIS", DataCell = new TextBoxCell(0)});

            treePcmcia.AllowMultipleSelection = false;
            treePcmcia.ShowHeader             = false;
            treePcmcia.DataStore              = cisList;

            Tuple[] tuples = CIS.GetTuples(cis);
            if(tuples != null)
                foreach(Tuple tuple in tuples)
                {
                    string tupleCode;
                    string tupleDescription;

                    switch(tuple.Code)
                    {
                        case TupleCodes.CISTPL_NULL:
                        case TupleCodes.CISTPL_END: continue;
                        case TupleCodes.CISTPL_DEVICEGEO:
                        case TupleCodes.CISTPL_DEVICEGEO_A:
                            tupleCode        = "Device Geometry Tuples";
                            tupleDescription = CIS.PrettifyDeviceGeometryTuple(tuple);
                            break;
                        case TupleCodes.CISTPL_MANFID:
                            tupleCode        = "Manufacturer Identification Tuple";
                            tupleDescription = CIS.PrettifyManufacturerIdentificationTuple(tuple);
                            break;
                        case TupleCodes.CISTPL_VERS_1:
                            tupleCode        = "Level 1 Version / Product Information Tuple";
                            tupleDescription = CIS.PrettifyLevel1VersionTuple(tuple);
                            break;
                        case TupleCodes.CISTPL_ALTSTR:
                        case TupleCodes.CISTPL_BAR:
                        case TupleCodes.CISTPL_BATTERY:
                        case TupleCodes.CISTPL_BYTEORDER:
                        case TupleCodes.CISTPL_CFTABLE_ENTRY:
                        case TupleCodes.CISTPL_CFTABLE_ENTRY_CB:
                        case TupleCodes.CISTPL_CHECKSUM:
                        case TupleCodes.CISTPL_CONFIG:
                        case TupleCodes.CISTPL_CONFIG_CB:
                        case TupleCodes.CISTPL_DATE:
                        case TupleCodes.CISTPL_DEVICE:
                        case TupleCodes.CISTPL_DEVICE_A:
                        case TupleCodes.CISTPL_DEVICE_OA:
                        case TupleCodes.CISTPL_DEVICE_OC:
                        case TupleCodes.CISTPL_EXTDEVIC:
                        case TupleCodes.CISTPL_FORMAT:
                        case TupleCodes.CISTPL_FORMAT_A:
                        case TupleCodes.CISTPL_FUNCE:
                        case TupleCodes.CISTPL_FUNCID:
                        case TupleCodes.CISTPL_GEOMETRY:
                        case TupleCodes.CISTPL_INDIRECT:
                        case TupleCodes.CISTPL_JEDEC_A:
                        case TupleCodes.CISTPL_JEDEC_C:
                        case TupleCodes.CISTPL_LINKTARGET:
                        case TupleCodes.CISTPL_LONGLINK_A:
                        case TupleCodes.CISTPL_LONGLINK_C:
                        case TupleCodes.CISTPL_LONGLINK_CB:
                        case TupleCodes.CISTPL_LONGLINK_MFC:
                        case TupleCodes.CISTPL_NO_LINK:
                        case TupleCodes.CISTPL_ORG:
                        case TupleCodes.CISTPL_PWR_MGMNT:
                        case TupleCodes.CISTPL_SPCL:
                        case TupleCodes.CISTPL_SWIL:
                        case TupleCodes.CISTPL_VERS_2:
                            tupleCode        = $"Undecoded tuple ID {tuple.Code}";
                            tupleDescription = $"Undecoded tuple ID {tuple.Code}";
                            break;
                        default:
                            tupleCode        = $"0x{(byte)tuple.Code:X2}";
                            tupleDescription = $"Found unknown tuple ID 0x{(byte)tuple.Code:X2}";
                            break;
                    }

                    cisList.Add(new TreeGridItem {Values = new object[] {tupleCode, tupleDescription}});
                }
            else DicConsole.DebugWriteLine("Device-Info command", "PCMCIA CIS returned no tuples");
        }

        protected void OnTreePcmciaSelectedItemChanged(object sender, EventArgs e)
        {
            if(!(treePcmcia.SelectedItem is TreeGridItem item)) return;

            txtPcmciaCis.Text = item.Values[1] as string;
        }

        protected void OnBtnSavePcmciaCis(object sender, EventArgs e)
        {
            SaveFileDialog dlgSaveBinary = new SaveFileDialog();
            dlgSaveBinary.Filters.Add(new FileFilter {Extensions = new[] {"*.bin"}, Name = "Binary"});
            DialogResult result = dlgSaveBinary.ShowDialog(this);

            if(result != DialogResult.Ok) return;

            FileStream saveFs = new FileStream(dlgSaveBinary.FileName, FileMode.Create);
            saveFs.Write(cis, 0, cis.Length);

            saveFs.Close();
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        TreeGridView treePcmcia;
        TextArea     txtPcmciaCis;
        Button       btnSavePcmciaCis;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}