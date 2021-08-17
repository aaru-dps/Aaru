// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages
{
    // TODO: Too many unknowns to make this writable
    /// <summary>
    /// Implements reading BlindWrite 5/6/7 disc images
    /// </summary>
    public sealed partial class BlindWrite5 : IOpticalMediaImage
    {
        byte[]                        _atip;
        byte[]                        _bca;
        List<SessionDescriptor>       _bwSessions;
        byte[]                        _cdtext;
        List<DataFile>                _dataFiles;
        string                        _dataPath;
        byte[]                        _discInformation;
        byte[]                        _dmi;
        byte[]                        _dpm;
        List<DataFileCharacteristics> _filePaths;
        byte[]                        _fullToc;
        Header                        _header;
        ImageInfo                     _imageInfo;
        Stream                        _imageStream;
        byte[]                        _mode2A;
        Dictionary<uint, ulong>       _offsetMap;
        byte[]                        _pfi;
        byte[]                        _pma;
        Dictionary<uint, byte>        _trackFlags;
        byte[]                        _unkBlock;

        public BlindWrite5() => _imageInfo = new ImageInfo
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