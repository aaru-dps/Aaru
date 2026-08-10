// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : D88.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Quasi88 disk images.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.Images;

// Information from Quasi88's FORMAT.TXT file
// Japanese comments copied from there
// TODO: Solve media types
/// <inheritdoc />
/// <summary>Implements reading Quasi88 disk images</summary>
public sealed partial class D88 : IMediaImage
{
    const string MODULE_NAME = "D88 plugin";
    ImageInfo    _imageInfo;
    List<byte[]> _sectorsData;

    // Old-style images have a 0x2A0 bytes header with 160 track table entries, new-style ones have 0x2B0 bytes
    // and 164 entries. The table is self-delimiting: it ends where the lowest track offset points, so entries
    // past that point belong to track data and must be ignored.
    static int TrackTableLength(int[] trackTable)
    {
        int entries = trackTable.Length;

        for(var i = 0; i < entries; i++)
        {
            int t = trackTable[i];

            if(t <= 0) continue;

            int implied = (t - TRACK_TABLE_OFFSET) / 4;

            if(implied < entries) entries = implied;
        }

        return entries;
    }

    public D88() => _imageInfo = new ImageInfo
    {
        ReadableSectorTags    = [],
        ReadableMediaTags     = [],
        HasPartitions         = false,
        HasSessions           = false,
        Version               = null,
        Application           = null,
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