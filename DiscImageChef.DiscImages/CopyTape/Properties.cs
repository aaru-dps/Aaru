// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Properties.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains properties for CopyTape tape images.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.DiscImages.CopyTape
{
    public partial class CopyTape
    {
        public ImageInfo              Info           => imageInfo;
        public string                 Name           => "CopyTape";
        public Guid                   Id             => new Guid("C537D41E-D6A7-4922-9AA9-8E8442D0E340");
        public string                 Author         => "Natalia Portillo";
        public string                 Format         => "CopyTape";
        public List<DumpHardwareType> DumpHardware   => null;
        public CICMMetadataType       CicmMetadata   => null;
        public List<TapeFile>         Files          { get; private set; }
        public List<TapePartition>    TapePartitions { get; set; }
        public bool                   IsTape         => true;
    }
}