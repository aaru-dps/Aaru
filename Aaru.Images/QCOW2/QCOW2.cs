// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : QCOW2.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages QEMU Copy-On-Write v2 disk images.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages
{
    /// <inheritdoc />
    /// <summary>Implements reading and writing QEMU's Copy On Write v2 and v3 disk images</summary>
    public sealed partial class Qcow2 : IWritableImage
    {
        Dictionary<ulong, byte[]>  _clusterCache;
        int                        _clusterSectors;
        int                        _clusterSize;
        ImageInfo                  _imageInfo;
        Stream                     _imageStream;
        ulong                      _l1Mask;
        int                        _l1Shift;
        ulong[]                    _l1Table;
        int                        _l2Bits;
        ulong                      _l2Mask;
        int                        _l2Size;
        Dictionary<ulong, ulong[]> _l2TableCache;
        int                        _maxClusterCache;
        int                        _maxL2TableCache;
        Header                     _qHdr;
        ulong[]                    _refCountTable;
        Dictionary<ulong, byte[]>  _sectorCache;
        ulong                      _sectorMask;
        FileStream                 _writingStream;

        public Qcow2() => _imageInfo = new ImageInfo
        {
            ReadableSectorTags    = new List<SectorTagType>(),
            ReadableMediaTags     = new List<MediaTagType>(),
            HasPartitions         = false,
            HasSessions           = false,
            Version               = null,
            Application           = "QEMU",
            ApplicationVersion    = null,
            Creator               = null,
            Comments              = null,
            MediaManufacturer     = null,
            MediaModel            = null,
            MediaSerialNumber     = null,
            MediaBarcode          = null,
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