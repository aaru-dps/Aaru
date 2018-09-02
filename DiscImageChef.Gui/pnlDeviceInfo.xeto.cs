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

using DiscImageChef.Core.Devices.Info;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui
{
    public class pnlDeviceInfo : Panel
    {
        public pnlDeviceInfo(DeviceInfo devInfo)
        {
            XamlReader.Load(this);

            txtType.Text         = devInfo.Type.ToString();
            txtManufacturer.Text = devInfo.Manufacturer;
            txtModel.Text        = devInfo.Model;
            txtRevision.Text     = devInfo.Revision;
            txtSerial.Text       = devInfo.Serial;
            txtScsiType.Text     = devInfo.ScsiType.ToString();
            chkRemovable.Checked = devInfo.IsRemovable;
            chkUsb.Checked       = devInfo.IsUsb;
        }

        #region XAML controls
        Label      lblDeviceInfo;
        TabControl tabInfos;
        TabPage    tabGeneral;
        Label      lblType;
        TextBox    txtType;
        Label      lblManufacturer;
        TextBox    txtManufacturer;
        Label      lblModel;
        TextBox    txtModel;
        Label      lblRevision;
        TextBox    txtRevision;
        Label      lblSerial;
        TextBox    txtSerial;
        Label      lblScsiType;
        TextBox    txtScsiType;
        CheckBox   chkRemovable;
        CheckBox   chkUsb;
        #endregion
    }
}