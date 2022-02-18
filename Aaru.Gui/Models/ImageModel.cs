// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ImageModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI data models.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains information about an opened image.
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

using System.Collections.ObjectModel;
using Aaru.CommonTypes.Interfaces;
using Aaru.Gui.ViewModels.Panels;
using Avalonia.Media.Imaging;

namespace Aaru.Gui.Models
{
    public sealed class ImageModel
    {
        public ImageModel() => PartitionSchemesOrFileSystems = new ObservableCollection<RootModel>();

        public string                          Path                          { get; set; }
        public string                          FileName                      { get; set; }
        public Bitmap                          Icon                          { get; set; }
        public ObservableCollection<RootModel> PartitionSchemesOrFileSystems { get; }
        public IMediaImage                     Image                         { get; set; }
        public ImageInfoViewModel              ViewModel                     { get; set; }
        public IFilter                         Filter                        { get; set; }
    }
}