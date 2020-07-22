// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Properties.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains properties for Quasi88 disk images.
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
    public sealed partial class D88
    {
        public string                 Name         => "D88 Disk Image";
        public Guid                   Id           => new Guid("669EDC77-EC41-4720-A88C-49C38CFFBAA0");
        public ImageInfo              Info         => _imageInfo;
        public string                 Format       => "D88 disk image";
        public string                 Author       => "Natalia Portillo";
        public List<DumpHardwareType> DumpHardware => null;
        public CICMMetadataType       CicmMetadata => null;
    }
}