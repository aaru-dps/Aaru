// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Apple2.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Apple ][ floppy structures.
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Aaru.Console;
using Aaru.Localization;

namespace Aaru.Decoders.Floppy;

/// <summary>Methods and structures for Apple ][ floppy decoding</summary>
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class Apple2
{
    static readonly byte[] ReadTable5and3 =
    {
        // 00h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 10h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 20h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 30h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 40h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 50h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 60h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 70h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 80h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 90h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // A0h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0x01, 0x02, 0x03,

        // B0h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x04, 0x05, 0x06, 0xFF, 0xFF, 0x07, 0x08, 0xFF, 0x09, 0x0A, 0x0B,

        // C0h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // D0h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x0C, 0x0D, 0xFF, 0xFF, 0x0E, 0x0F, 0xFF, 0x10, 0x11, 0x12,

        // E0h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x13, 0x14, 0xFF, 0x15, 0x16, 0x17,

        // F0h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x18, 0x19, 0x1A, 0xFF, 0xFF, 0x1B, 0x1C, 0xFF, 0x1D, 0x1E, 0x1F
    };

    static readonly byte[] ReadTable6and2 =
    {
        // 00h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 10h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 20h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 30h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 40h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 50h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 60h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 70h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 80h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,

        // 90h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x01, 0xFF, 0xFF, 0x02, 0x03, 0xFF, 0x04, 0x05, 0x06,

        // A0h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x07, 0x08, 0xFF, 0xFF, 0xFF, 0x09, 0x0A, 0x0B, 0x0C, 0x0D,

        // B0h
        0xFF, 0xFF, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0xFF, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A,

        // C0h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x1B, 0xFF, 0x1C, 0x1D, 0x1E,

        // D0h
        0xFF, 0xFF, 0xFF, 0x1F, 0xFF, 0xFF, 0x20, 0x21, 0xFF, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28,

        // E0h
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x29, 0x2A, 0x2B, 0xFF, 0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, 0x32,

        // F0h
        0xFF, 0xFF, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0xFF, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F
    };

    /// <summary>Decodes the 5and3 encoded data</summary>
    /// <param name="data">5and3 encoded data.</param>
    public static byte[] Decode5and3(byte[] data)
    {
        if(data is not { Length: 410 })
            return null;

        byte[] buffer = new byte[data.Length];
        byte   carry  = 0;

        for(int i = 0; i < data.Length; i++)
        {
            carry     ^= ReadTable5and3[data[i]];
            buffer[i] =  carry;
        }

        byte[] output = new byte[256];

        for(int i = 0; i < 51; i++)
        {
            byte b1 = buffer[(51 * 3) - i];
            byte b2 = buffer[(51 * 2) - i];
            byte b3 = buffer[51 - i];
            byte b4 = (byte)((((b1 & 2) << 1) | (b2 & 2) | ((b3 & 2) >> 1)) & 0xFF);
            byte b5 = (byte)((((b1 & 1) << 2) | ((b2 & 1) << 1) | (b3 & 1)) & 0xFF);
            output[250 - (5 * i)] = (byte)(((buffer[i + (51 * 3) + 1] << 3) | ((b1 >> 2) & 0x7)) & 0xFF);
            output[251 - (5 * i)] = (byte)(((buffer[i + (51 * 4) + 1] << 3) | ((b2 >> 2) & 0x7)) & 0xFF);
            output[252 - (5 * i)] = (byte)(((buffer[i + (51 * 5) + 1] << 3) | ((b3 >> 2) & 0x7)) & 0xFF);
            output[253 - (5 * i)] = (byte)(((buffer[i + (51 * 6) + 1] << 3) | b4) & 0xFF);
            output[254 - (5 * i)] = (byte)(((buffer[i + (51 * 7) + 1] << 3) | b5) & 0xFF);
        }

        output[255] = (byte)(((buffer[409] << 3) | (buffer[0] & 0x7)) & 0xFF);

        return output;
    }

    /// <summary>Decodes the 6and2 encoded data</summary>
    /// <param name="data">6and2 encoded data.</param>
    public static byte[] Decode6and2(byte[] data)
    {
        if(data is not { Length: 342 })
            return null;

        byte[] buffer = new byte[data.Length];
        byte   carry  = 0;

        for(int i = 0; i < data.Length; i++)
        {
            carry     ^= ReadTable6and2[data[i]];
            buffer[i] =  carry;
        }

        byte[] output = new byte[256];

        for(uint i = 0; i < 256; i++)
        {
            output[i] = (byte)((buffer[86 + i] << 2) & 0xFF);

            switch(i)
            {
                case < 86:
                    output[i] |= (byte)(((buffer[i] & 1) << 1) & 0xFF);
                    output[i] |= (byte)(((buffer[i] & 2) >> 1) & 0xFF);

                    break;
                case < 86 * 2:
                    output[i] |= (byte)(((buffer[i - 86] & 4) >> 1) & 0xFF);
                    output[i] |= (byte)(((buffer[i - 86] & 8) >> 3) & 0xFF);

                    break;
                default:
                    output[i] |= (byte)(((buffer[i - (86 * 2)] & 0x10) >> 3) & 0xFF);
                    output[i] |= (byte)(((buffer[i - (86 * 2)] & 0x20) >> 5) & 0xFF);

                    break;
            }
        }

        return output;
    }

    public static byte[] DecodeSector(RawSector sector)
    {
        if(sector.addressField.prologue[0] != 0xD5 ||
           sector.addressField.prologue[1] != 0xAA)
            return null;

        // Pre DOS 3.3
        if(sector.addressField.prologue[2] == 0xB5)
            return Decode5and3(sector.dataField.data);

        // DOS 3.3
        return sector.addressField.prologue[2] == 0x96 ? Decode6and2(sector.dataField.data) : null;

        // Unknown

        // Not Apple ][ GCR?
    }

    public static RawSector MarshalSector(byte[] data, int offset = 0) => MarshalSector(data, out _, offset);

    public static RawSector MarshalSector(byte[] data, out int endOffset, int offset = 0)
    {
        endOffset = offset;

        // Not an Apple ][ GCR sector
        if(data        == null ||
           data.Length < 363)
            return null;

        int position = offset;

        try
        {
            while(position < data.Length)
            {
                // Prologue found
                if(data[position]     == 0xD5 &&
                   data[position + 1] == 0xAA)
                {
                    AaruConsole.DebugWriteLine("Apple ][ GCR Decoder", Localization.Prologue_found_at_0, position);

                    // Epilogue not in correct position
                    if(data[position + 11] != 0xDE ||
                       data[position + 12] != 0xAA)
                        return null;

                    var sector = new RawSector
                    {
                        addressField = new RawAddressField
                        {
                            prologue = new[]
                            {
                                data[position], data[position + 1], data[position + 2]
                            },
                            volume = new[]
                            {
                                data[position + 3], data[position + 4]
                            },
                            track = new[]
                            {
                                data[position + 5], data[position + 6]
                            },
                            sector = new[]
                            {
                                data[position + 7], data[position + 8]
                            },
                            checksum = new[]
                            {
                                data[position + 9], data[position + 10]
                            },
                            epilogue = new[]
                            {
                                data[position + 11], data[position + 12], data[position + 13]
                            }
                        }
                    };

                    AaruConsole.DebugWriteLine("Apple ][ GCR Decoder", Localization.Volume_0,
                                               (((sector.addressField.volume[0] & 0x55) << 1) |
                                                (sector.addressField.volume[1] & 0x55)) & 0xFF);

                    AaruConsole.DebugWriteLine("Apple ][ GCR Decoder", Core.Track_0,
                                               (((sector.addressField.track[0] & 0x55) << 1) |
                                                (sector.addressField.track[1] & 0x55)) & 0xFF);

                    AaruConsole.DebugWriteLine("Apple ][ GCR Decoder", Localization.Sector_0,
                                               (((sector.addressField.sector[0] & 0x55) << 1) |
                                                (sector.addressField.sector[1] & 0x55)) & 0xFF);

                    AaruConsole.DebugWriteLine("Apple ][ GCR Decoder", Localization.Checksum_0,
                                               (((sector.addressField.checksum[0] & 0x55) << 1) |
                                                (sector.addressField.checksum[1] & 0x55)) & 0xFF);

                    AaruConsole.DebugWriteLine("Apple ][ GCR Decoder", Localization.Epilogue_0_1_2,
                                               sector.addressField.epilogue[0], sector.addressField.epilogue[1],
                                               sector.addressField.epilogue[2]);

                    position += 14;
                    int  syncCount = 0;
                    bool onSync    = false;
                    var  gaps      = new MemoryStream();

                    while(data[position] == 0xFF)
                    {
                        gaps.WriteByte(data[position]);
                        syncCount++;
                        onSync = syncCount >= 5;
                        position++;
                    }

                    // Lost sync
                    if(!onSync)
                        return null;

                    // Prologue not found
                    if(data[position]     != 0xD5 ||
                       data[position + 1] != 0xAA)
                        return null;

                    sector.innerGap  = gaps.ToArray();
                    sector.dataField = new RawDataField();

                    AaruConsole.DebugWriteLine("Apple ][ GCR Decoder", Localization.Inner_gap_has_0_bytes,
                                               sector.innerGap.Length);

                    AaruConsole.DebugWriteLine("Apple ][ GCR Decoder", Localization.Prologue_found_at_0, position);
                    sector.dataField.prologue    =  new byte[3];
                    sector.dataField.prologue[0] =  data[position];
                    sector.dataField.prologue[1] =  data[position + 1];
                    sector.dataField.prologue[2] =  data[position + 2];
                    position                     += 3;

                    gaps = new MemoryStream();

                    // Read data until epilogue is found
                    while(data[position + 1] != 0xDE ||
                          data[position + 2] != 0xAA)
                    {
                        gaps.WriteByte(data[position]);
                        position++;

                        // No space left for epilogue
                        if(position + 4 > data.Length)
                            return null;
                    }

                    sector.dataField.data = gaps.ToArray();

                    AaruConsole.DebugWriteLine("Apple ][ GCR Decoder", Localization.Data_has_0_bytes,
                                               sector.dataField.data.Length);

                    sector.dataField.checksum    = data[position];
                    sector.dataField.epilogue    = new byte[3];
                    sector.dataField.epilogue[0] = data[position + 1];
                    sector.dataField.epilogue[1] = data[position + 2];
                    sector.dataField.epilogue[2] = data[position + 3];

                    position += 4;
                    gaps     =  new MemoryStream();

                    // Read gap, if any
                    while(position       < data.Length &&
                          data[position] == 0xFF)
                    {
                        gaps.WriteByte(data[position]);
                        position++;
                    }

                    // Reduces last sector gap so doesn't eat next tracks's gap
                    if(gaps.Length > 5)
                    {
                        gaps.SetLength(gaps.Length / 2);
                        position -= (int)gaps.Length;
                    }

                    sector.gap = gaps.ToArray();

                    // Return current position to be able to read separate sectors
                    endOffset = position;

                    AaruConsole.DebugWriteLine("Apple ][ GCR Decoder", Localization.Got_0_bytes_of_gap,
                                               sector.gap.Length);

                    AaruConsole.DebugWriteLine("Apple ][ GCR Decoder", Localization.Finished_sector_at_0, position);

                    return sector;
                }

                if(data[position] == 0xFF)
                    position++;

                // Found data that is not sync or a prologue
                else
                    return null;
            }
        }
        catch(IndexOutOfRangeException)
        {
            return null;
        }

        return null;
    }

    public static byte[] MarshalAddressField(RawAddressField addressField)
    {
        if(addressField == null)
            return null;

        var raw = new MemoryStream();
        raw.Write(addressField.prologue, 0, addressField.prologue.Length);
        raw.Write(addressField.volume, 0, addressField.volume.Length);
        raw.Write(addressField.track, 0, addressField.track.Length);
        raw.Write(addressField.sector, 0, addressField.sector.Length);
        raw.Write(addressField.checksum, 0, addressField.checksum.Length);
        raw.Write(addressField.epilogue, 0, addressField.epilogue.Length);

        return raw.ToArray();
    }

    public static byte[] MarshalSector(RawSector sector)
    {
        if(sector == null)
            return null;

        var raw = new MemoryStream();
        raw.Write(sector.addressField.prologue, 0, sector.addressField.prologue.Length);
        raw.Write(sector.addressField.volume, 0, sector.addressField.volume.Length);
        raw.Write(sector.addressField.track, 0, sector.addressField.track.Length);
        raw.Write(sector.addressField.sector, 0, sector.addressField.sector.Length);
        raw.Write(sector.addressField.checksum, 0, sector.addressField.checksum.Length);
        raw.Write(sector.addressField.epilogue, 0, sector.addressField.epilogue.Length);
        raw.Write(sector.innerGap, 0, sector.innerGap.Length);
        raw.Write(sector.dataField.prologue, 0, sector.dataField.prologue.Length);
        raw.Write(sector.dataField.data, 0, sector.dataField.data.Length);
        raw.WriteByte(sector.dataField.checksum);
        raw.Write(sector.dataField.epilogue, 0, sector.dataField.epilogue.Length);
        raw.Write(sector.gap, 0, sector.gap.Length);

        return raw.ToArray();
    }

    public static RawTrack MarshalTrack(byte[] data, int offset = 0) => MarshalTrack(data, out _, offset);

    public static RawTrack MarshalTrack(byte[] data, out int endOffset, int offset = 0)
    {
        int             position    = offset;
        bool            firstSector = true;
        bool            onSync      = false;
        var             gaps        = new MemoryStream();
        int             count       = 0;
        List<RawSector> sectors     = new();
        byte[]          trackNumber = new byte[2];
        endOffset = offset;

        while(position       < data.Length &&
              data[position] == 0xFF)
        {
            gaps.WriteByte(data[position]);
            count++;
            position++;
            onSync = count >= 5;
        }

        if(position >= data.Length)
            return null;

        if(!onSync)
            return null;

        while(position < data.Length)
        {
            int       oldPosition = position;
            RawSector sector      = MarshalSector(data, out position, position);

            if(sector == null)
                break;

            if(firstSector)
            {
                trackNumber[0] = sector.addressField.track[0];
                trackNumber[1] = sector.addressField.track[1];
                firstSector    = false;
            }

            if(sector.addressField.track[0] != trackNumber[0] ||
               sector.addressField.track[1] != trackNumber[1])
            {
                position = oldPosition;

                break;
            }

            AaruConsole.DebugWriteLine("Apple ][ GCR Decoder", Localization.Adding_sector_0_of_track_1,
                                       (((sector.addressField.sector[0] & 0x55) << 1) |
                                        (sector.addressField.sector[1] & 0x55)) & 0xFF,
                                       (((sector.addressField.track[0] & 0x55) << 1) |
                                        (sector.addressField.track[1] & 0x55)) & 0xFF);

            sectors.Add(sector);
        }

        if(sectors.Count == 0)
            return null;

        var track = new RawTrack
        {
            gap     = gaps.ToArray(),
            sectors = sectors.ToArray()
        };

        endOffset = position;

        return track;
    }

    public static byte[] MarshalTrack(RawTrack track)
    {
        if(track == null)
            return null;

        var raw = new MemoryStream();
        raw.Write(track.gap, 0, track.gap.Length);

        foreach(byte[] rawSector in track.sectors.Select(MarshalSector))
            raw.Write(rawSector, 0, rawSector.Length);

        return raw.ToArray();
    }

    public static List<RawTrack> MarshalDisk(byte[] data, int offset = 0) => MarshalDisk(data, out _, offset);

    public static List<RawTrack> MarshalDisk(byte[] data, out int endOffset, int offset = 0)
    {
        endOffset = offset;
        List<RawTrack> tracks   = new();
        int            position = offset;

        RawTrack track = MarshalTrack(data, out position, position);

        while(track != null)
        {
            tracks.Add(track);
            track = MarshalTrack(data, out position, position);
        }

        if(tracks.Count == 0)
            return null;

        endOffset = position;

        return tracks;
    }

    public static byte[] MarshalDisk(List<RawTrack> disk) => MarshalDisk(disk.ToArray());

    public static byte[] MarshalDisk(RawTrack[] disk)
    {
        if(disk == null)
            return null;

        var raw = new MemoryStream();

        foreach(byte[] rawTrack in disk.Select(MarshalTrack))
            raw.Write(rawTrack, 0, rawTrack.Length);

        return raw.ToArray();
    }

    public static bool IsApple2GCR(byte[] data)
    {
        RawSector sector = MarshalSector(data, out int position);

        return sector != null && position != 0;
    }

    /// <summary>GCR-encoded Apple ][ GCR floppy track</summary>
    public class RawTrack
    {
        /// <summary>Track preamble, set to self-sync 0xFF, between 40 and 95 bytes</summary>
        public byte[] gap;
        public RawSector[] sectors;
    }

    /// <summary>GCR-encoded Apple ][ GCR floppy sector</summary>
    public class RawSector
    {
        /// <summary>Address field</summary>
        public RawAddressField addressField;
        /// <summary>Data field</summary>
        public RawDataField dataField;
        /// <summary>Track preamble, set to self-sync 0xFF, between 14 and 24 bytes</summary>
        public byte[] gap;
        /// <summary>Track preamble, set to self-sync 0xFF, between 5 and 10 bytes</summary>
        public byte[] innerGap;
    }

    /// <summary>GCR-encoded Apple ][ GCR floppy sector address field</summary>
    public class RawAddressField
    {
        /// <summary>
        ///     decodedChecksum = decodedVolume ^ decodedTrack ^ decodedSector checksum[0] = (decodedChecksum >> 1) | 0xAA
        ///     checksum[1] = decodedChecksum | 0xAA
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] checksum;
        /// <summary>Always 0xDE, 0xAA, 0xEB</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] epilogue;
        /// <summary>Always 0xD5, 0xAA, 0x96</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] prologue;
        /// <summary>Sector number encoded as: sector[0] = (decodedSector >> 1) | 0xAA sector[1] = decodedSector | 0xAA</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] sector;
        /// <summary>Track number encoded as: track[0] = (decodedTrack >> 1) | 0xAA track[1] = decodedTrack | 0xAA</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] track;
        /// <summary>Volume number encoded as: volume[0] = (decodedVolume >> 1) | 0xAA volume[1] = decodedVolume | 0xAA</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] volume;
    }

    /// <summary>GCR-encoded Apple ][ GCR floppy sector data field</summary>
    public class RawDataField
    {
        public byte checksum;
        /// <summary>Encoded data bytes. 410 bytes for 5to3 (aka DOS 3.2) format 342 bytes for 6to2 (aka DOS 3.3) format</summary>
        public byte[] data;
        /// <summary>Always 0xDE, 0xAA, 0xEB</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] epilogue;
        /// <summary>Always 0xD5, 0xAA, 0xAD</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] prologue;
    }
}