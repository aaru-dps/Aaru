// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Audio.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains logic to create sidecar from an audio media dump.
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


using System.Collections.Generic;
using System.IO;
using DiscImageChef.ImagePlugins;
using Schemas;

namespace DiscImageChef.Core
{
    public static partial class Sidecar
    {
        // TODO: Complete it
        static void AudioMedia(ImagePlugin image, System.Guid filterId, string imagePath, FileInfo fi, PluginBase plugins, List<ChecksumType> imgChecksums, ref CICMMetadataType sidecar)
        {
            sidecar.AudioMedia = new []
            {
	            new AudioMediaType
	            {
	                Checksums = imgChecksums.ToArray(),
	                Image = new ImageType
	                {
	                    format = image.GetImageFormat(),
	                    offset = 0,
	                    offsetSpecified = true,
	                    Value = Path.GetFileName(imagePath)
	                },
	                Size = fi.Length,
	                Sequence = new SequenceType
	                {
	                    MediaTitle = image.GetImageName()
	                }
                }
            };

            if(image.GetMediaSequence() != 0 && image.GetLastDiskSequence() != 0)
            {
                sidecar.AudioMedia[0].Sequence.MediaSequence = image.GetMediaSequence();
                sidecar.AudioMedia[0].Sequence.TotalMedia = image.GetMediaSequence();
            }
            else
            {
                sidecar.AudioMedia[0].Sequence.MediaSequence = 1;
                sidecar.AudioMedia[0].Sequence.TotalMedia = 1;
            }
        }
    }
}
