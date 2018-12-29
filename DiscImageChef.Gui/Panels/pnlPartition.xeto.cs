// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : pnlPartition.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitions information panel.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the partitions information panel.
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

using DiscImageChef.CommonTypes;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Panels
{
    public class pnlPartition : Panel
    {
        public pnlPartition(Partition partition)
        {
            XamlReader.Load(this);

            lblName.Text           = $"Partition name: {partition.Name}";
            lblType.Text           = $"Partition type: {partition.Type}";
            lblStart.Text          = $"Partition start: sector {partition.Start}, byte {partition.Offset}";
            lblLength.Text         = $"Partition length: {partition.Length} sectors, {partition.Size} bytes";
            grpDescription.Text    = "Partition description:";
            txtDescription.Text    = partition.Description;
            grpDescription.Visible = !string.IsNullOrEmpty(partition.Description);
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        Label    lblName;
        Label    lblType;
        Label    lblStart;
        Label    lblLength;
        GroupBox grpDescription;
        TextArea txtDescription;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}