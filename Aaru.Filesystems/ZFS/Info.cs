// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ZFS filesystem plugin.
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/*
 * The ZFS on-disk structure is quite undocumented, so this has been checked using several test images and reading the comments and headers (but not the code)
 * of ZFS-On-Linux.
 *
 * The most basic structure, the vdev label, is as follows:
 * 8KiB of blank space
 * 8KiB reserved for boot code, stored as a ZIO block with magic and checksum
 * 112KiB of nvlist, usually encoded using XDR
 * 128KiB of copies of the 1KiB uberblock
 *
 * Two vdev labels, L0 and L1 are stored at the start of the vdev.
 * Another two, L2 and L3 are stored at the end.
 *
 * The nvlist is nothing more than a double linked list of name/value pairs where name is a string and value is an arbitrary type (and can be an array of it).
 * On-disk they are stored sequentially (no pointers) and can be encoded in XDR (an old Sun serialization method that stores everything as 4 bytes chunks) or
 * natively (that is as the host natively stores that values, for example on Intel an extended float would be 10 bytes (80 bit).
 * It can also be encoded little or big endian.
 * Because of this variations, ZFS stored a header indicating the used encoding and endianess before the encoded nvlist.
 */
/// <inheritdoc />
/// <summary>Implements detection for the Zettabyte File System (ZFS)</summary>
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "UnusedType.Local"),
 SuppressMessage("ReSharper", "UnusedMember.Local"), SuppressMessage("ReSharper", "NotAccessedField.Local")]
public sealed partial class ZFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 512)
            return false;

        byte[]      sector;
        ulong       magic;
        ErrorNumber errno;

        if(partition.Start + 31 < partition.End)
        {
            errno = imagePlugin.ReadSector(partition.Start + 31, out sector);

            if(errno != ErrorNumber.NoError)
                return false;

            magic = BitConverter.ToUInt64(sector, 0x1D8);

            if(magic is ZEC_MAGIC or ZEC_CIGAM)
                return true;
        }

        if(partition.Start + 16 >= partition.End)
            return false;

        errno = imagePlugin.ReadSector(partition.Start + 16, out sector);

        if(errno != ErrorNumber.NoError)
            return false;

        magic = BitConverter.ToUInt64(sector, 0x1D8);

        return magic is ZEC_MAGIC or ZEC_CIGAM;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        // ZFS is always UTF-8
        Encoding    = Encoding.UTF8;
        information = "";
        ErrorNumber errno;

        if(imagePlugin.Info.SectorSize < 512)
            return;

        byte[] sector;
        ulong  magic;

        ulong nvlistOff = 32;
        uint  nvlistLen = 114688 / imagePlugin.Info.SectorSize;

        if(partition.Start + 31 < partition.End)
        {
            errno = imagePlugin.ReadSector(partition.Start + 31, out sector);

            if(errno != ErrorNumber.NoError)
                return;

            magic = BitConverter.ToUInt64(sector, 0x1D8);

            if(magic is ZEC_MAGIC or ZEC_CIGAM)
                nvlistOff = 32;
        }

        if(partition.Start + 16 < partition.End)
        {
            errno = imagePlugin.ReadSector(partition.Start + 16, out sector);

            if(errno != ErrorNumber.NoError)
                return;

            magic = BitConverter.ToUInt64(sector, 0x1D8);

            if(magic is ZEC_MAGIC or ZEC_CIGAM)
                nvlistOff = 17;
        }

        var sb = new StringBuilder();
        sb.AppendLine(Localization.ZFS_filesystem);

        errno = imagePlugin.ReadSectors(partition.Start + nvlistOff, nvlistLen, out byte[] nvlist);

        if(errno != ErrorNumber.NoError)
            return;

        sb.AppendLine(!DecodeNvList(nvlist, out Dictionary<string, NVS_Item> decodedNvList) ? "Could not decode nvlist"
                          : PrintNvList(decodedNvList));

        information = sb.ToString();

        Metadata = new FileSystem
        {
            Type = FS_TYPE
        };

        if(decodedNvList.TryGetValue("name", out NVS_Item tmpObj))
            Metadata.VolumeName = (string)tmpObj.value;

        if(decodedNvList.TryGetValue("guid", out tmpObj))
            Metadata.VolumeSerial = $"{(ulong)tmpObj.value}";

        if(decodedNvList.TryGetValue("pool_guid", out tmpObj))
            Metadata.VolumeSetIdentifier = $"{(ulong)tmpObj.value}";
    }
}