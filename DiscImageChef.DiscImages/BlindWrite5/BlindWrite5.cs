// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : BlindWrite5.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages BlindWrite 5 disc images.
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

using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.DiscImages
{
    // TODO: Too many unknowns to make this writable
    public partial class BlindWrite5 : IOpticalMediaImage
    {
        byte[]                        atip;
        byte[]                        bca;
        List<Bw5SessionDescriptor>    bwSessions;
        byte[]                        cdtext;
        List<Bw5DataFile>             dataFiles;
        string                        dataPath;
        byte[]                        discInformation;
        byte[]                        dmi;
        byte[]                        dpm;
        List<DataFileCharacteristics> filePaths;
        byte[]                        fullToc;

        Bw5Header               header;
        ImageInfo               imageInfo;
        Stream                  imageStream;
        byte[]                  mode2A;
        Dictionary<uint, ulong> offsetmap;
        byte[]                  pfi;
        byte[]                  pma;
        Dictionary<uint, byte>  trackFlags;
        byte[]                  unkBlock;

        public BlindWrite5()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = true,
                HasSessions           = true,
                Version               = null,
                ApplicationVersion    = null,
                MediaTitle            = null,
                Creator               = null,
                MediaManufacturer     = null,
                MediaModel            = null,
                MediaPartNumber       = null,
                MediaSequence         = 0,
                LastMediaSequence     = 0,
                DriveManufacturer     = null,
                DriveModel            = null,
                DriveSerialNumber     = null,
                DriveFirmwareRevision = null
            };
        }
    }
}