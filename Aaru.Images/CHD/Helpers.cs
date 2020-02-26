// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for MAME Compressed Hunks of Data disk images.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Helpers;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

namespace DiscImageChef.DiscImages
{
    public partial class Chd
    {
        Track GetTrack(ulong sector)
        {
            Track track = new Track();
            foreach(KeyValuePair<ulong, uint> kvp in offsetmap.Where(kvp => sector >= kvp.Key))
                tracks.TryGetValue(kvp.Value, out track);

            return track;
        }

        ulong GetAbsoluteSector(ulong relativeSector, uint track)
        {
            tracks.TryGetValue(track, out Track dicTrack);
            return dicTrack.TrackStartSector + relativeSector;
        }

        byte[] GetHunk(ulong hunkNo)
        {
            if(hunkCache.TryGetValue(hunkNo, out byte[] hunk)) return hunk;

            switch(mapVersion)
            {
                case 1:
                    ulong offset = hunkTable[hunkNo] & 0x00000FFFFFFFFFFF;
                    ulong length = hunkTable[hunkNo] >> 44;

                    byte[] compHunk = new byte[length];
                    imageStream.Seek((long)offset, SeekOrigin.Begin);
                    imageStream.Read(compHunk, 0, compHunk.Length);

                    if(length                              == sectorsPerHunk * imageInfo.SectorSize) hunk = compHunk;
                    else if((ChdCompression)hdrCompression > ChdCompression.Zlib)
                        throw new
                            ImageNotSupportedException($"Unsupported compression {(ChdCompression)hdrCompression}");
                    else
                    {
                        DeflateStream zStream =
                            new DeflateStream(new MemoryStream(compHunk), CompressionMode.Decompress);
                        hunk = new byte[sectorsPerHunk * imageInfo.SectorSize];
                        int read = zStream.Read(hunk, 0, (int)(sectorsPerHunk * imageInfo.SectorSize));
                        if(read != sectorsPerHunk * imageInfo.SectorSize)
                            throw new
                                IOException($"Unable to decompress hunk correctly, got {read} bytes, expected {sectorsPerHunk * imageInfo.SectorSize}");

                        zStream.Close();
                    }

                    break;
                case 3:
                    byte[] entryBytes = new byte[16];
                    Array.Copy(hunkMap, (int)(hunkNo * 16), entryBytes, 0, 16);
                    ChdMapV3Entry entry = Marshal.ByteArrayToStructureBigEndian<ChdMapV3Entry>(entryBytes);
                    switch((Chdv3EntryFlags)(entry.flags & 0x0F))
                    {
                        case Chdv3EntryFlags.Invalid: throw new ArgumentException("Invalid hunk found.");
                        case Chdv3EntryFlags.Compressed:
                            switch((ChdCompression)hdrCompression)
                            {
                                case ChdCompression.None: goto uncompressedV3;
                                case ChdCompression.Zlib:
                                case ChdCompression.ZlibPlus:
                                    if(isHdd)
                                    {
                                        byte[] zHunk = new byte[(entry.lengthLsb << 16) + entry.lengthLsb];
                                        imageStream.Seek((long)entry.offset, SeekOrigin.Begin);
                                        imageStream.Read(zHunk, 0, zHunk.Length);
                                        DeflateStream zStream =
                                            new DeflateStream(new MemoryStream(zHunk), CompressionMode.Decompress);
                                        hunk = new byte[bytesPerHunk];
                                        int read = zStream.Read(hunk, 0, (int)bytesPerHunk);
                                        if(read != bytesPerHunk)
                                            throw new
                                                IOException($"Unable to decompress hunk correctly, got {read} bytes, expected {bytesPerHunk}");

                                        zStream.Close();
                                    }
                                    // TODO: Guess wth is MAME doing with these hunks
                                    else
                                        throw new
                                            ImageNotSupportedException("Compressed CD/GD-ROM hunks are not yet supported");

                                    break;
                                case ChdCompression.Av:
                                    throw new
                                        ImageNotSupportedException($"Unsupported compression {(ChdCompression)hdrCompression}");
                            }

                            break;
                        case Chdv3EntryFlags.Uncompressed:
                            uncompressedV3:
                            hunk = new byte[bytesPerHunk];
                            imageStream.Seek((long)entry.offset, SeekOrigin.Begin);
                            imageStream.Read(hunk, 0, hunk.Length);
                            break;
                        case Chdv3EntryFlags.Mini:
                            hunk = new byte[bytesPerHunk];
                            byte[] mini;
                            mini = BigEndianBitConverter.GetBytes(entry.offset);
                            for(int i = 0; i < bytesPerHunk; i++) hunk[i] = mini[i % 8];

                            break;
                        case Chdv3EntryFlags.SelfHunk: return GetHunk(entry.offset);
                        case Chdv3EntryFlags.ParentHunk:
                            throw new ImageNotSupportedException("Parent images are not supported");
                        case Chdv3EntryFlags.SecondCompressed:
                            throw new ImageNotSupportedException("FLAC is not supported");
                        default:
                            throw new ImageNotSupportedException($"Hunk type {entry.flags & 0xF} is not supported");
                    }

                    break;
                case 5:
                    if(hdrCompression == 0)
                    {
                        hunk = new byte[bytesPerHunk];
                        imageStream.Seek(hunkTableSmall[hunkNo] * bytesPerHunk, SeekOrigin.Begin);
                        imageStream.Read(hunk, 0, hunk.Length);
                    }
                    else throw new ImageNotSupportedException("Compressed v5 hunks not yet supported");

                    break;
                default: throw new ImageNotSupportedException($"Unsupported hunk map version {mapVersion}");
            }

            if(hunkCache.Count >= maxBlockCache) hunkCache.Clear();

            hunkCache.Add(hunkNo, hunk);

            return hunk;
        }
    }
}