// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : dlgEncodings.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Plugins dialog.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the encodings list dialog.
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Dialogs
{
    public class dlgEncodings : Dialog
    {
        ObservableCollection<CommonEncodingInfo> encodings;

        public dlgEncodings()
        {
            XamlReader.Load(this);

            DefaultButton = btnClose;
            DisplayMode   = DialogDisplayMode.Attached;

            encodings = new ObservableCollection<CommonEncodingInfo>();

            grdEncodings.DataStore = encodings;
            grdEncodings.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<CommonEncodingInfo, string>(r => r.Name)
                },
                HeaderText = "Code",
                Sortable   = true
            });
            grdEncodings.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding =
                        Binding
                           .Property<CommonEncodingInfo, string>(r => r.DisplayName)
                },
                HeaderText = "Name",
                Sortable   = true
            });
            grdEncodings.AllowMultipleSelection = false;
            grdEncodings.AllowColumnReordering  = true;
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            encodings.Clear();

            List<CommonEncodingInfo> _encodings = Encoding
                                                 .GetEncodings().Select(info => new CommonEncodingInfo
                                                  {
                                                      Name = info.Name,
                                                      DisplayName =
                                                          info.GetEncoding().EncodingName
                                                  }).ToList();
            _encodings.AddRange(Claunia.Encoding.Encoding.GetEncodings()
                                       .Select(info => new CommonEncodingInfo
                                        {
                                            Name = info.Name, DisplayName = info.DisplayName
                                        }));

            foreach(CommonEncodingInfo encoding in _encodings.OrderBy(t => t.DisplayName)) encodings.Add(encoding);
        }

        protected void OnBtnClose(object sender, EventArgs e)
        {
            Close();
        }

        class CommonEncodingInfo
        {
            public string Name        { get; set; }
            public string DisplayName { get; set; }
        }

        #region XAML controls
        GridView grdEncodings;
        Button   btnClose;
        #endregion
    }
}