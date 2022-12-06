﻿// /***************************************************************************
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

namespace Aaru.DiscImages
{
    public sealed partial class KryoFlux
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            if(stream.Length < Marshal.SizeOf<OobBlock>())
                return false;

            byte[] hdr = new byte[Marshal.SizeOf<OobBlock>()];
            stream.Read(hdr, 0, Marshal.SizeOf<OobBlock>());

            OobBlock header = Marshal.ByteArrayToStructureLittleEndian<OobBlock>(hdr);

            stream.Seek(-Marshal.SizeOf<OobBlock>(), SeekOrigin.End);

            hdr = new byte[Marshal.SizeOf<OobBlock>()];
            stream.Read(hdr, 0, Marshal.SizeOf<OobBlock>());

            OobBlock footer = Marshal.ByteArrayToStructureLittleEndian<OobBlock>(hdr);

            if(header.blockId   != BlockIds.Oob    ||
               header.blockType != OobTypes.KFInfo ||
               footer.blockId   != BlockIds.Oob    ||
               footer.blockType != OobTypes.EOF    ||
               footer.length    != 0x0D0D)
                return false;

            // TODO: This is supposing NoFilter, shouldn't
            tracks = new SortedDictionary<byte, IFilter>();
            byte step    = 1;
            byte heads   = 2;
            bool topHead = false;

            string basename = Path.Combine(imageFilter.GetParentFolder(),
                                           imageFilter.GetFilename().
                                                       Substring(0, imageFilter.GetFilename().Length - 8));

            for(byte t = 0; t < 166; t += step)
            {
                int cylinder = t / heads;
                int head     = topHead ? 1 : t % heads;

                string trackfile = Directory.Exists(basename) ? Path.Combine(basename, $"{cylinder:D2}.{head:D1}.raw")
                                       : $"{basename}{cylinder:D2}.{head:D1}.raw";

                if(!File.Exists(trackfile))
                    if(cylinder == 0)
                    {
                        if(head == 0)
                        {
                            AaruConsole.DebugWriteLine("KryoFlux plugin",
                                                       "Cannot find cyl 0 hd 0, supposing only top head was dumped");

                            topHead = true;
                            heads   = 1;

                            continue;
                        }

                        AaruConsole.DebugWriteLine("KryoFlux plugin",
                                                   "Cannot find cyl 0 hd 1, supposing only bottom head was dumped");

                        heads = 1;

                        continue;
                    }
                    else if(cylinder == 1)
                    {
                        AaruConsole.DebugWriteLine("KryoFlux plugin", "Cannot find cyl 1, supposing double stepping");
                        step = 2;

                        continue;
                    }
                    else
                    {
                        AaruConsole.DebugWriteLine("KryoFlux plugin", "Arrived end of disk at cylinder {0}", cylinder);

                        break;
                    }

                var trackFilter = new ZZZNoFilter();
                trackFilter.Open(trackfile);

                if(!trackFilter.IsOpened())
                    throw new IOException("Could not open KryoFlux track file.");

                _imageInfo.CreationTime         = DateTime.MaxValue;
                _imageInfo.LastModificationTime = DateTime.MinValue;

                Stream trackStream = trackFilter.GetDataForkStream();

                while(trackStream.Position < trackStream.Length)
                {
                    byte blockId = (byte)trackStream.ReadByte();

                    switch(blockId)
                    {
                        case (byte)BlockIds.Oob:
                        {
                            trackStream.Position--;

                            byte[] oob = new byte[Marshal.SizeOf<OobBlock>()];
                            trackStream.Read(oob, 0, Marshal.SizeOf<OobBlock>());

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

                            byte[] kfinfo = new byte[oobBlk.length];
                            trackStream.Read(kfinfo, 0, oobBlk.length);
                            string kfinfoStr = StringHandlers.CToString(kfinfo);

                            string[] lines = kfinfoStr.Split(new[]
                            {
                                ','
                            }, StringSplitOptions.RemoveEmptyEntries);

                            DateTime blockDate = DateTime.Now;
                            DateTime blockTime = DateTime.Now;
                            bool     foundDate = false;

                            foreach(string[] kvp in lines.Select(line => line.Split('=')).Where(kvp => kvp.Length == 2))
                            {
                                kvp[0] = kvp[0].Trim();
                                kvp[1] = kvp[1].Trim();
                                AaruConsole.DebugWriteLine("KryoFlux plugin", "\"{0}\" = \"{1}\"", kvp[0], kvp[1]);

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

                                AaruConsole.DebugWriteLine("KryoFlux plugin", "Found timestamp: {0}", blockTimestamp);

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
                        default: continue;
                    }
                }

                tracks.Add(t, trackFilter);
            }

            _imageInfo.Heads     = heads;
            _imageInfo.Cylinders = (uint)(tracks.Count / heads);

            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        /// <inheritdoc />
        public byte[] ReadDiskTag(MediaTagType tag) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");
    }
}