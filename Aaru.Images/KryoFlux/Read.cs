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
//     Reads KryoFlux STREAM images.
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
using System.Globalization;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Filters;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class KryoFlux
{
#region IMediaImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < Marshal.SizeOf<OobBlock>())
            return ErrorNumber.InvalidArgument;

        var hdr = new byte[Marshal.SizeOf<OobBlock>()];
        stream.EnsureRead(hdr, 0, Marshal.SizeOf<OobBlock>());

        OobBlock header = Marshal.ByteArrayToStructureLittleEndian<OobBlock>(hdr);

        stream.Seek(-Marshal.SizeOf<OobBlock>(), SeekOrigin.End);

        hdr = new byte[Marshal.SizeOf<OobBlock>()];
        stream.EnsureRead(hdr, 0, Marshal.SizeOf<OobBlock>());

        OobBlock footer = Marshal.ByteArrayToStructureLittleEndian<OobBlock>(hdr);

        if(header.blockId   != BlockIds.Oob    ||
           header.blockType != OobTypes.KFInfo ||
           footer.blockId   != BlockIds.Oob    ||
           footer.blockType != OobTypes.EOF    ||
           footer.length    != 0x0D0D)
            return ErrorNumber.InvalidArgument;

        // TODO: This is supposing NoFilter, shouldn't
        tracks = new SortedDictionary<byte, IFilter>();
        byte step    = 1;
        byte heads   = 2;
        var  topHead = false;

        string basename = Path.Combine(imageFilter.ParentFolder, imageFilter.Filename[..^8]);

        for(byte t = 0; t < 166; t += step)
        {
            int cylinder = t / heads;
            int head     = topHead ? 1 : t % heads;

            string trackfile = Directory.Exists(basename)
                                   ? Path.Combine(basename, $"{cylinder:D2}.{head:D1}.raw")
                                   : $"{basename}{cylinder:D2}.{head:D1}.raw";

            if(!File.Exists(trackfile))
            {
                if(cylinder == 0)
                {
                    if(head == 0)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.
                                                       Cannot_find_cyl_0_hd_0_supposing_only_top_head_was_dumped);

                        topHead = true;
                        heads   = 1;

                        continue;
                    }

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.
                                                   Cannot_find_cyl_0_hd_1_supposing_only_bottom_head_was_dumped);

                    heads = 1;

                    continue;
                }

                if(cylinder == 1)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Cannot_find_cyl_1_supposing_double_stepping);

                    step = 2;

                    continue;
                }

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Arrived_end_of_disk_at_cylinder_0,
                                           cylinder);

                break;
            }

            var         trackFilter = new ZZZNoFilter();
            ErrorNumber errno       = trackFilter.Open(trackfile);

            if(errno != ErrorNumber.NoError)
                return errno;

            _imageInfo.CreationTime         = DateTime.MaxValue;
            _imageInfo.LastModificationTime = DateTime.MinValue;

            Stream trackStream = trackFilter.GetDataForkStream();

            while(trackStream.Position < trackStream.Length)
            {
                var blockId = (byte)trackStream.ReadByte();

                switch(blockId)
                {
                    case (byte)BlockIds.Oob:
                    {
                        trackStream.Position--;

                        var oob = new byte[Marshal.SizeOf<OobBlock>()];
                        trackStream.EnsureRead(oob, 0, Marshal.SizeOf<OobBlock>());

                        OobBlock oobBlk = Marshal.ByteArrayToStructureLittleEndian<OobBlock>(oob);

                        if(oobBlk.blockType == OobTypes.EOF)
                        {
                            trackStream.Position = trackStream.Length;

                            break;
                        }

                        if(oobBlk.blockType != OobTypes.KFInfo)
                        {
                            trackStream.Position += oobBlk.length;

                            break;
                        }

                        var kfinfo = new byte[oobBlk.length];
                        trackStream.EnsureRead(kfinfo, 0, oobBlk.length);
                        string kfinfoStr = StringHandlers.CToString(kfinfo);

                        string[] lines = kfinfoStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        DateTime blockDate = DateTime.Now;
                        DateTime blockTime = DateTime.Now;
                        var      foundDate = false;

                        foreach(string[] kvp in lines.Select(line => line.Split('=')).Where(kvp => kvp.Length == 2))
                        {
                            kvp[0] = kvp[0].Trim();
                            kvp[1] = kvp[1].Trim();
                            AaruConsole.DebugWriteLine(MODULE_NAME, "\"{0}\" = \"{1}\"", kvp[0], kvp[1]);

                            switch(kvp[0])
                            {
                                case HOST_DATE:
                                    if(DateTime.TryParseExact(kvp[1], "yyyy.MM.dd", CultureInfo.InvariantCulture,
                                                              DateTimeStyles.AssumeLocal, out blockDate))
                                        foundDate = true;

                                    break;
                                case HOST_TIME:
                                    DateTime.TryParseExact(kvp[1], "HH:mm:ss", CultureInfo.InvariantCulture,
                                                           DateTimeStyles.AssumeLocal, out blockTime);

                                    break;
                                case KF_NAME:
                                    _imageInfo.Application = kvp[1];

                                    break;
                                case KF_VERSION:
                                    _imageInfo.ApplicationVersion = kvp[1];

                                    break;
                            }
                        }

                        if(foundDate)
                        {
                            var blockTimestamp = new DateTime(blockDate.Year, blockDate.Month, blockDate.Day,
                                                              blockTime.Hour, blockTime.Minute, blockTime.Second);

                            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_timestamp_0,
                                                       blockTimestamp);

                            if(blockTimestamp < Info.CreationTime)
                                _imageInfo.CreationTime = blockTimestamp;

                            if(blockTimestamp > Info.LastModificationTime)
                                _imageInfo.LastModificationTime = blockTimestamp;
                        }

                        break;
                    }
                    case (byte)BlockIds.Flux2:
                    case (byte)BlockIds.Flux2_1:
                    case (byte)BlockIds.Flux2_2:
                    case (byte)BlockIds.Flux2_3:
                    case (byte)BlockIds.Flux2_4:
                    case (byte)BlockIds.Flux2_5:
                    case (byte)BlockIds.Flux2_6:
                    case (byte)BlockIds.Flux2_7:
                    case (byte)BlockIds.Nop2:
                        trackStream.Position++;

                        continue;
                    case (byte)BlockIds.Nop3:
                    case (byte)BlockIds.Flux3:
                        trackStream.Position += 2;

                        continue;
                    default:
                        continue;
                }
            }

            tracks.Add(t, trackFilter);
        }

        _imageInfo.Heads     = heads;
        _imageInfo.Cylinders = (uint)(tracks.Count / heads);

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
    public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, 1, tag, out buffer);

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