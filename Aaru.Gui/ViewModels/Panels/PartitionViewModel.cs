// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PartitionViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the partition information panel.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes;

namespace Aaru.Gui.ViewModels.Panels;

public sealed class PartitionViewModel
{
    public PartitionViewModel(Partition partition)
    {
        NameText             = $"Partition name: {partition.Name}";
        TypeText             = $"Partition type: {partition.Type}";
        StartText            = $"Partition start: sector {partition.Start}, byte {partition.Offset}";
        LengthText           = $"Partition length: {partition.Length} sectors, {partition.Size} bytes";
        DescriptionLabelText = "Partition description:";
        DescriptionText      = partition.Description;
    }

    public string NameText             { get; }
    public string TypeText             { get; }
    public string StartText            { get; }
    public string LengthText           { get; }
    public string DescriptionLabelText { get; }
    public string DescriptionText      { get; }
}