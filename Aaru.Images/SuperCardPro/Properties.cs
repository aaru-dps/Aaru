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
//     Contains properties for SuperCardPro flux images.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages
{
    public partial class SuperCardPro
    {
        public ImageInfo              Info         => imageInfo;
        public string                 Name         => "SuperCardPro";
        public Guid                   Id           => new Guid("C5D3182E-1D45-4767-A205-E6E5C83444DC");
        public string                 Author       => "Natalia Portillo";
        public string                 Format       => "SuperCardPro";
        public List<DumpHardwareType> DumpHardware => null;
        public CICMMetadataType       CicmMetadata => null;
    }
}