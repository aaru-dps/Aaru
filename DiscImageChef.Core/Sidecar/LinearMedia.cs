// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : LinearMedia.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains logic to create sidecar from a linear media dump.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Core
{
    public static partial class Sidecar
    {
        // TODO: Complete it
        /// <summary>
        ///     Creates a metadata sidecar for linear media (e.g. ROM chip)
        /// </summary>
        /// <param name="image">Image</param>
        /// <param name="filterId">Filter uuid</param>
        /// <param name="imagePath">Image path</param>
        /// <param name="fi">Image file information</param>
        /// <param name="plugins">Image plugins</param>
        /// <param name="imgChecksums">List of image checksums</param>
        /// <param name="sidecar">Metadata sidecar</param>
        static void LinearMedia(ImagePlugin image, Guid filterId, string imagePath, FileInfo fi, PluginBase plugins,
                                List<ChecksumType> imgChecksums, ref CICMMetadataType sidecar)
        {
            sidecar.LinearMedia = new[]
            {
                new LinearMediaType
                {
                    Checksums = imgChecksums.ToArray(),
                    Image = new ImageType
                    {
                        format = image.ImageFormat,
                        offset = 0,
                        offsetSpecified = true,
                        Value = Path.GetFileName(imagePath)
                    },
                    Size = fi.Length
                }
            };
        }
    }
}