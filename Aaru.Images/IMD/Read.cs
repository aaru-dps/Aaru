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
//     Reads Dunfield's IMD disk images.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class Imd
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        var cmt = new MemoryStream();
        stream.Seek(0x1F, SeekOrigin.Begin);

        for(uint i = 0; i < stream.Length; i++)
        {
            var b = (byte)stream.ReadByte();

            if(b == 0x1A) break;

            cmt.WriteByte(b);
        }

        _imageInfo.Comments = StringHandlers.CToString(cmt.ToArray());
        _sectorsData        = new List<byte[]>();

        byte currentCylinder = 0;
        _imageInfo.Cylinders = 1;
        _imageInfo.Heads     = 1;
        ulong currentLba = 0;

        TransferRate mode = TransferRate.TwoHundred;

        while(stream.Position + 5 < stream.Length)
        {
            mode = (TransferRate)stream.ReadByte();
            var cylinder = (byte)stream.ReadByte();
            var head     = (byte)stream.ReadByte();
            var spt      = (byte)stream.ReadByte();
            var n        = (byte)stream.ReadByte();
            var idmap    = new byte[spt];
            var cylmap   = new byte[spt];
            var headmap  = new byte[spt];
            var bps      = new ushort[spt];

            if(cylinder != currentCylinder)
            {
                currentCylinder = cylinder;
                _imageInfo.Cylinders++;
            }

            if((head & 1) == 1) _imageInfo.Heads = 2;

            stream.EnsureRead(idmap, 0, idmap.Length);

            if((head & SECTOR_CYLINDER_MAP_MASK) == SECTOR_CYLINDER_MAP_MASK)
                stream.EnsureRead(cylmap, 0, cylmap.Length);

            if((head & SECTOR_HEAD_MAP_MASK) == SECTOR_HEAD_MAP_MASK) stream.EnsureRead(headmap, 0, headmap.Length);

            if(n == 0xFF)
            {
                var bpsbytes = new byte[spt * 2];
                stream.EnsureRead(bpsbytes, 0, bpsbytes.Length);

                for(var i = 0; i < spt; i++) bps[i] = BitConverter.ToUInt16(bpsbytes, i * 2);
            }
            else
            {
                for(var i = 0; i < spt; i++) bps[i] = (ushort)(128 << n);
            }

            if(spt > _imageInfo.SectorsPerTrack) _imageInfo.SectorsPerTrack = spt;

            SortedDictionary<byte, byte[]> track = new();

            for(var i = 0; i < spt; i++)
            {
                var type = (SectorType)stream.ReadByte();
                var data = new byte[bps[i]];

                // TODO; Handle disks with different bps in track 0
                if(bps[i] > _imageInfo.SectorSize) _imageInfo.SectorSize = bps[i];

                switch(type)
                {
                    case SectorType.Unavailable:
                        if(!track.ContainsKey(idmap[i])) track.Add(idmap[i], data);

                        break;
                    case SectorType.Normal:
                    case SectorType.Deleted:
                    case SectorType.Error:
                    case SectorType.DeletedError:
                        stream.EnsureRead(data, 0, data.Length);

                        if(!track.ContainsKey(idmap[i])) track.Add(idmap[i], data);

                        _imageInfo.ImageSize += (ulong)data.Length;

                        break;
                    case SectorType.Compressed:
                    case SectorType.CompressedDeleted:
                    case SectorType.CompressedError:
                    case SectorType.CompressedDeletedError:
                        var filling = (byte)stream.ReadByte();
                        ArrayHelpers.ArrayFill(data, filling);

                        if(!track.ContainsKey(idmap[i])) track.Add(idmap[i], data);

                        break;
                    default:
                        AaruConsole.ErrorWriteLine(string.Format(Localization.Invalid_sector_type_0, (byte)type));

                        return ErrorNumber.InvalidArgument;
                }
            }

            foreach(KeyValuePair<byte, byte[]> kvp in track)
            {
                _sectorsData.Add(kvp.Value);
                currentLba++;
            }
        }

        _imageInfo.Application = "IMD";

        // TODO: The header is the date of dump or the date of the application compilation?
        _imageInfo.CreationTime         = imageFilter.CreationTime;
        _imageInfo.LastModificationTime = imageFilter.LastWriteTime;
        _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.Filename);
        _imageInfo.Comments             = StringHandlers.CToString(cmt.ToArray());
        _imageInfo.Sectors              = currentLba;
        _imageInfo.MediaType            = MediaType.Unknown;

        MediaEncoding mediaEncoding = MediaEncoding.MFM;

        if(mode is TransferRate.TwoHundred or TransferRate.ThreeHundred or TransferRate.FiveHundred)
            mediaEncoding = MediaEncoding.FM;

        _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_imageInfo.Cylinders, (byte)_imageInfo.Heads,
                                                      (ushort)_imageInfo.SectorsPerTrack, _imageInfo.SectorSize,
                                                      mediaEncoding, false));

        switch(_imageInfo.MediaType)
        {
            case MediaType.NEC_525_HD when mode == TransferRate.FiveHundredMfm:
                _imageInfo.MediaType = MediaType.NEC_35_HD_8;

                break;
            case MediaType.DOS_525_HD when mode == TransferRate.FiveHundredMfm:
                _imageInfo.MediaType = MediaType.NEC_35_HD_15;

                break;
            case MediaType.RX50 when mode == TransferRate.FiveHundredMfm:
                _imageInfo.MediaType = MediaType.ATARI_35_SS_DD;

                break;
        }

        _imageInfo.MetadataMediaType = MetadataMediaType.BlockMedia;

        AaruConsole.VerboseWriteLine(Localization.IMD_image_contains_a_disk_of_type_0, _imageInfo.MediaType);

        if(!string.IsNullOrEmpty(_imageInfo.Comments))
            AaruConsole.VerboseWriteLine(Localization.IMD_comments_0, _imageInfo.Comments);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        if(sectorAddress > _imageInfo.Sectors - 1) return ErrorNumber.OutOfRange;

        if(sectorAddress + length > _imageInfo.Sectors) return ErrorNumber.OutOfRange;

        var ms = new MemoryStream();

        for(var i = 0; i < length; i++)
            ms.Write(_sectorsData[(int)sectorAddress + i], 0, _sectorsData[(int)sectorAddress + i].Length);

        buffer = ms.ToArray();

        return ErrorNumber.NoError;
    }

#endregion
}