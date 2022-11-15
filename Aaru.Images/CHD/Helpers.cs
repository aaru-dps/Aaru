// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

namespace Aaru.DiscImages;

public sealed partial class Chd
{
    Track GetTrack(ulong sector)
    {
        var track = new Track();

        foreach(KeyValuePair<ulong, uint> kvp in _offsetmap.Where(kvp => sector >= kvp.Key))
            _tracks.TryGetValue(kvp.Value, out track);

        return track;
    }

    ulong GetAbsoluteSector(ulong relativeSector, uint track)
    {
        _tracks.TryGetValue(track, out Track aaruTrack);

        return (aaruTrack?.StartSector ?? 0) + relativeSector;
    }

    ErrorNumber GetHunk(ulong hunkNo, out byte[] buffer)
    {
        if(_hunkCache.TryGetValue(hunkNo, out buffer))
            return ErrorNumber.NoError;

        switch(_mapVersion)
        {
            case 1:
                ulong offset = _hunkTable[hunkNo] & 0x00000FFFFFFFFFFF;
                ulong length = _hunkTable[hunkNo] >> 44;

                byte[] compHunk = new byte[length];
                _imageStream.Seek((long)offset, SeekOrigin.Begin);
                _imageStream.EnsureRead(compHunk, 0, compHunk.Length);

                if(length == _sectorsPerHunk * _imageInfo.SectorSize)
                    buffer = compHunk;
                else if((Compression)_hdrCompression > Compression.Zlib)
                {
                    AaruConsole.ErrorWriteLine($"Unsupported compression {(Compression)_hdrCompression}");

                    return ErrorNumber.InvalidArgument;
                }
                else
                {
                    var zStream = new DeflateStream(new MemoryStream(compHunk), CompressionMode.Decompress);
                    buffer = new byte[_sectorsPerHunk * _imageInfo.SectorSize];
                    int read = zStream.EnsureRead(buffer, 0, (int)(_sectorsPerHunk * _imageInfo.SectorSize));

                    if(read != _sectorsPerHunk * _imageInfo.SectorSize)
                    {
                        AaruConsole.ErrorWriteLine($"Unable to decompress hunk correctly, got {read} bytes, expected {
                            _sectorsPerHunk * _imageInfo.SectorSize}");

                        return ErrorNumber.InOutError;
                    }

                    zStream.Close();
                }

                break;
            case 3:
                byte[] entryBytes = new byte[16];
                Array.Copy(_hunkMap, (int)(hunkNo * 16), entryBytes, 0, 16);
                MapEntryV3 entry = Marshal.ByteArrayToStructureBigEndian<MapEntryV3>(entryBytes);

                switch((EntryFlagsV3)(entry.flags & 0x0F))
                {
                    case EntryFlagsV3.Invalid:
                        AaruConsole.ErrorWriteLine("Invalid hunk found.");

                        return ErrorNumber.InvalidArgument;
                    case EntryFlagsV3.Compressed:
                        switch((Compression)_hdrCompression)
                        {
                            case Compression.None: goto uncompressedV3;
                            case Compression.Zlib:
                            case Compression.ZlibPlus:
                                if(_isHdd)
                                {
                                    byte[] zHunk = new byte[(entry.lengthLsb << 16) + entry.lengthLsb];
                                    _imageStream.Seek((long)entry.offset, SeekOrigin.Begin);
                                    _imageStream.EnsureRead(zHunk, 0, zHunk.Length);

                                    var zStream =
                                        new DeflateStream(new MemoryStream(zHunk), CompressionMode.Decompress);

                                    buffer = new byte[_bytesPerHunk];
                                    int read = zStream.EnsureRead(buffer, 0, (int)_bytesPerHunk);

                                    if(read != _bytesPerHunk)
                                    {
                                        AaruConsole.ErrorWriteLine($"Unable to decompress hunk correctly, got {read
                                        } bytes, expected {_bytesPerHunk}");

                                        return ErrorNumber.InOutError;
                                    }

                                    zStream.Close();
                                }

                                // TODO: Guess wth is MAME doing with these hunks
                                else
                                {
                                    AaruConsole.ErrorWriteLine("Compressed CD/GD-ROM hunks are not yet supported");

                                    return ErrorNumber.NotImplemented;
                                }

                                break;
                            case Compression.Av:
                                AaruConsole.ErrorWriteLine($"Unsupported compression {(Compression)_hdrCompression}");

                                return ErrorNumber.NotImplemented;
                        }

                        break;
                    case EntryFlagsV3.Uncompressed:
                        uncompressedV3:
                        buffer = new byte[_bytesPerHunk];
                        _imageStream.Seek((long)entry.offset, SeekOrigin.Begin);
                        _imageStream.EnsureRead(buffer, 0, buffer.Length);

                        break;
                    case EntryFlagsV3.Mini:
                        buffer = new byte[_bytesPerHunk];
                        byte[] mini = BigEndianBitConverter.GetBytes(entry.offset);

                        for(int i = 0; i < _bytesPerHunk; i++)
                            buffer[i] = mini[i % 8];

                        break;
                    case EntryFlagsV3.SelfHunk: return GetHunk(entry.offset, out buffer);
                    case EntryFlagsV3.ParentHunk:
                        AaruConsole.ErrorWriteLine("Parent images are not supported");

                        return ErrorNumber.NotImplemented;
                    case EntryFlagsV3.SecondCompressed:
                        AaruConsole.ErrorWriteLine("FLAC is not supported");

                        return ErrorNumber.NotImplemented;
                    default:
                        AaruConsole.ErrorWriteLine($"Hunk type {entry.flags & 0xF} is not supported");

                        return ErrorNumber.NotSupported;
                }

                break;
            case 5:
                if(_hdrCompression == 0)
                {
                    buffer = new byte[_bytesPerHunk];
                    _imageStream.Seek(_hunkTableSmall[hunkNo] * _bytesPerHunk, SeekOrigin.Begin);
                    _imageStream.EnsureRead(buffer, 0, buffer.Length);
                }
                else
                {
                    AaruConsole.ErrorWriteLine("Compressed v5 hunks not yet supported");

                    return ErrorNumber.NotSupported;
                }

                break;
            default:
                AaruConsole.ErrorWriteLine($"Unsupported hunk map version {_mapVersion}");

                return ErrorNumber.NotSupported;
        }

        if(_hunkCache.Count >= _maxBlockCache)
            _hunkCache.Clear();

        _hunkCache.Add(hunkNo, buffer);

        return ErrorNumber.NoError;
    }
}