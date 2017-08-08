// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : LinearMedia.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$

using System.Collections.Generic;
using System.IO;
using DiscImageChef.ImagePlugins;
using Schemas;

namespace DiscImageChef.Core
{
    public static partial class Sidecar
    {
        // TODO: Complete it
        static void LinearMedia(ImagePlugin image, System.Guid filterId, string imagePath, FileInfo fi, PluginBase plugins, List<ChecksumType> imgChecksums, ref CICMMetadataType sidecar)
        {
            sidecar.LinearMedia = new LinearMediaType[1];
            sidecar.LinearMedia[0] = new LinearMediaType();
            sidecar.LinearMedia[0].Checksums = imgChecksums.ToArray();
            sidecar.LinearMedia[0].Image = new ImageType();
            sidecar.LinearMedia[0].Image.format = image.GetImageFormat();
            sidecar.LinearMedia[0].Image.offset = 0;
            sidecar.LinearMedia[0].Image.offsetSpecified = true;
            sidecar.LinearMedia[0].Image.Value = Path.GetFileName(imagePath);
            sidecar.LinearMedia[0].Size = fi.Length;
        }
    }
}
