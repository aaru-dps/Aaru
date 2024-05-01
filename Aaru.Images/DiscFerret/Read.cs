// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads DiscFerret flux images.
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
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class DiscFerret
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        var    magicB = new byte[4];
        Stream stream = imageFilter.GetDataForkStream();
        stream.EnsureRead(magicB, 0, 4);
        var magic = BitConverter.ToUInt32(magicB, 0);

        if(magic != DFI_MAGIC && magic != DFI_MAGIC2) return ErrorNumber.InvalidArgument;

        TrackOffsets = new SortedDictionary<int, long>();
        TrackLengths = new SortedDictionary<int, long>();
        int    t            = -1;
        ushort lastCylinder = 0, lastHead = 0;
        long   offset       = 0;

        while(stream.Position < stream.Length)
        {
            long thisOffset = stream.Position;

            var blk = new byte[Marshal.SizeOf<BlockHeader>()];
            stream.EnsureRead(blk, 0, Marshal.SizeOf<BlockHeader>());
            BlockHeader blockHeader = Marshal.ByteArrayToStructureBigEndian<BlockHeader>(blk);

            AaruConsole.DebugWriteLine(MODULE_NAME, "block@{0}.cylinder = {1}", thisOffset, blockHeader.cylinder);

            AaruConsole.DebugWriteLine(MODULE_NAME, "block@{0}.head = {1}", thisOffset, blockHeader.head);

            AaruConsole.DebugWriteLine(MODULE_NAME, "block@{0}.sector = {1}", thisOffset, blockHeader.sector);

            AaruConsole.DebugWriteLine(MODULE_NAME, "block@{0}.length = {1}", thisOffset, blockHeader.length);

            if(stream.Position + blockHeader.length > stream.Length)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Invalid_track_block_found_at_0, thisOffset);

                break;
            }

            stream.Position += blockHeader.length;

            if(blockHeader.cylinder > 0 && blockHeader.cylinder > lastCylinder)
            {
                lastCylinder = blockHeader.cylinder;
                lastHead     = 0;
                TrackOffsets.Add(t, offset);
                TrackLengths.Add(t, thisOffset - offset + 1);
                offset = thisOffset;
                t++;
            }
            else if(blockHeader.head > 0 && blockHeader.head > lastHead)
            {
                lastHead = blockHeader.head;
                TrackOffsets.Add(t, offset);
                TrackLengths.Add(t, thisOffset - offset + 1);
                offset = thisOffset;
                t++;
            }

            if(blockHeader.cylinder > _imageInfo.Cylinders) _imageInfo.Cylinders = blockHeader.cylinder;

            if(blockHeader.head > _imageInfo.Heads) _imageInfo.Heads = blockHeader.head;
        }

        _imageInfo.Heads++;
        _imageInfo.Cylinders++;

        _imageInfo.Application        = "DiscFerret";
        _imageInfo.ApplicationVersion = magic == DFI_MAGIC2 ? "2.0" : "1.0";

        AaruConsole.ErrorWriteLine(Localization.Flux_decoding_is_not_yet_implemented);

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotImplemented;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer) =>
        ReadSectorsLong(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        return ErrorNumber.NotImplemented;
    }

#endregion
}