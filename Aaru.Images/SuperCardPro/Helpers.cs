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
//     Contains helpers for SuperCardPro flux images.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class SuperCardPro
{
    static string ReadPStringUtf8(Stream stream, uint position)
    {
        if(position == 0) return null;

        stream.Position = position;
        var lenB = new byte[2];
        stream.EnsureRead(lenB, 0, 2);
        var len = BitConverter.ToUInt16(lenB, 0);

        if(len == 0 || len + stream.Position >= stream.Length) return null;

        var str = new byte[len];
        stream.EnsureRead(str, 0, len);

        return Encoding.UTF8.GetString(str);
    }

    /// <summary>
    ///     Takes a Head, Track and Sub-Track representation and converts it to the Track representation used by SCP.
    ///     For single-sided disks: side 0 uses even entries (0,2,4...), side 1 uses odd entries (1,3,5...).
    ///     For double-sided disks: standard head + track * 2.
    /// </summary>
    /// <param name="head">The head number</param>
    /// <param name="track">The track number</param>
    /// <param name="subTrack">The sub-track number</param>
    /// <param name="heads">Per SCP spec: 0=both heads, 1=side 0 only, 2=side 1 only</param>
    /// <returns>SCP format track number</returns>
    static long HeadTrackSubToScpTrack(uint head, ushort track, byte subTrack, byte heads)
    {
        // Per SCP spec: For single-sided disks, entries are skipped
        // Side 0 only: uses even entries (0,2,4,6...)
        // Side 1 only: uses odd entries (1,3,5,7...)
        if(heads == 1) // Side 0 only
            return track * 2;

        if(heads == 2) // Side 1 only
            return track * 2 + 1;

        // Double-sided: standard head + track * 2
        return head + track * 2;
    }

    static byte[] UInt32ToFluxRepresentation(uint ticks)
    {
        uint over = ticks / 255;

        if(over == 0) return [(byte)ticks];

        var expanded = new byte[over + 1];

        Array.Fill(expanded, (byte)255, 0, (int)over);
        expanded[^1] = (byte)(ticks % 255);

        return expanded;
    }

    static byte[] UInt16ToFluxRepresentation(ushort ticks) => UInt32ToFluxRepresentation(ticks);

    static List<uint> FluxRepresentationsToUInt32List(IEnumerable<byte> flux)
    {
        List<uint> scpData = [];
        uint       tick    = 0;

        foreach(byte b in flux)
        {
            if(b == 255)
                tick += 255;
            else
            {
                tick += b;
                scpData.Add(tick);
                tick = 0;
            }
        }

        return scpData;
    }

    static List<byte> FluxRepresentationsToUInt16List(IEnumerable<byte> flux, IReadOnlyList<uint> indices,
                                                      out uint[]        trackLengths)
    {
        List<byte> scpData = [];
        ushort     tick    = 0;

        List<uint> revolutionLength = [];
        uint       revolutionTicks  = 0;
        uint       revolutionCells  = 0;
        ushort     index            = 0;

        foreach(byte b in flux)
        {
            if(b == 255)
            {
                tick            += 255;
                revolutionTicks += 255;
            }
            else
            {
                tick += b;
                scpData.AddRange(BigEndianBitConverter.GetBytes(tick));
                tick = 0;

                revolutionTicks += b;

                if(revolutionTicks > indices[index] - 1)
                {
                    revolutionLength.Add(revolutionCells);
                    revolutionTicks = 0;
                    revolutionCells = 0;
                    index++;
                }

                revolutionCells++;
            }
        }

        revolutionLength.Add(revolutionCells);

        trackLengths = revolutionLength.ToArray();

        return scpData;
    }

    static uint CalculateChecksum(Stream stream)
    {
        var  wholeFile = new byte[stream.Length];
        uint sum       = 0;

        stream.Position = 0;
        stream.EnsureRead(wholeFile, 0, wholeFile.Length);

        for(int i = HEADER_OFFSET; i < wholeFile.Length; i++) sum += wholeFile[i];

        return sum;
    }

    /// <summary>
    ///     Reads flux data with overflow handling.
    ///     When a bit cell time is 0x0000, it indicates no flux transition for at least 65536*25ns.
    ///     Multiple consecutive 0x0000 entries accumulate (each adds 65536*25ns).
    /// </summary>
    /// <summary>Reads a big-endian 16-bit flux cell, false at end of stream</summary>
    static bool TryReadFluxCell(BinaryReader reader, out ushort value)
    {
        value = 0;
        byte[] cell = reader.ReadBytes(2);

        if(cell.Length < 2) return false;

        value = BigEndianBitConverter.ToUInt16(cell, 0);

        return true;
    }

    static void ReadFluxDataWithOverflow(BinaryReader reader, ulong trackLength, List<byte> output)
    {
        for(ulong j = 0; j < trackLength; j++)
        {
            if(!TryReadFluxCell(reader, out ushort rawValue)) return;

            // Per SCP spec: 0x0000 indicates overflow (no flux transition for >= 65536*25ns)
            // When this occurs, the next bit cell time will be added to 65536 (or more if multiple 0x0000)
            // e.g., 0x0000, 0x0000, 0x7FFF = 65536 + 65536 + 32767 = 163839
            if(rawValue == 0)
            {
                // Count consecutive 0x0000 entries to accumulate overflow
                ulong overflowCount = 1;

                // Look ahead to find next non-zero value or end of track
                while(j + overflowCount < trackLength)
                {
                    long savedPosition = reader.BaseStream.Position;

                    if(!TryReadFluxCell(reader, out ushort nextValue)) return;

                    if(nextValue == 0)
                    {
                        overflowCount++;
                    }
                    else
                    {
                        // Found non-zero value - restore position so we can read it again
                        reader.BaseStream.Position = savedPosition;

                        // Per SCP spec: Next non-zero value is added to accumulated overflow
                        // overflowCount * 65536 + nextValue = total bit cell time
                        if(!TryReadFluxCell(reader, out nextValue)) return;

                        uint overflowTotal = (uint)(overflowCount * 65536) + nextValue;
                        output.AddRange(UInt32ToFluxRepresentation(overflowTotal));

                        // We've consumed overflowCount overflow entries (0x0000) plus 1 next value entry
                        // Total entries consumed: overflowCount + 1
                        // j will be incremented by loop, so advance by (overflowCount + 1 - 1) = overflowCount
                        j += overflowCount;

                        goto continueLoop;
                    }
                }

                // End of track reached while in overflow - output just the overflow
                // This shouldn't normally happen in valid SCP files, but handle it gracefully
                uint overflowTotalOnly = (uint)(overflowCount * 65536);
                output.AddRange(UInt32ToFluxRepresentation(overflowTotalOnly));

                // We've consumed overflowCount entries, j will increment, so advance by overflowCount - 1
                j += overflowCount - 1;

                if(j + 1 >= trackLength) break;

            continueLoop:

                continue;
            }

            output.AddRange(UInt16ToFluxRepresentation(rawValue));
        }
    }

    /// <summary>
    ///     Reads ASCII timestamp string between track data and footer.
    ///     Timestamp format varies by locale (e.g., "1/05/2014 5:15:21 PM").
    ///     Valid ASCII range is 0x30-0x5F. Returns null if no valid timestamp found.
    /// </summary>
    static string ReadTimestamp(Stream stream, long afterTrackDataPosition)
    {
        // Per SCP spec: Timestamp appears after track data, before footer
        // Check if we're at a valid position to read timestamp
        if(stream.Position < afterTrackDataPosition) stream.Position = afterTrackDataPosition;

        // Per SCP spec: Timestamp is ASCII, valid range 0x30-0x5F
        // Check if first byte is valid ASCII
        long savedPosition = stream.Position;

        if(stream.Position >= stream.Length) return null;

        int firstByte = stream.ReadByte();

        if(firstByte < 0) return null;

        // Per SCP spec: Valid ASCII timestamp characters are 0x30-0x5F
        // But we should allow wider range for actual timestamp strings (printable ASCII)
        if(firstByte < 0x20 || firstByte > 0x7E)
        {
            stream.Position = savedPosition;

            return null;
        }

        stream.Position = savedPosition;

        // Read until we hit invalid ASCII or null terminator or reasonable length limit
        var timestampBytes = new List<byte>();
        int maxLength      = 256; // Reasonable max timestamp length

        for(int i = 0; i < maxLength; i++)
        {
            if(stream.Position >= stream.Length) break;

            int b = stream.ReadByte();

            if(b < 0) break; // EOF

            if(b == 0) break; // Null terminator

            // Per SCP spec: ASCII characters - allow printable range
            if(b < 0x20 || b > 0x7E) // Outside printable ASCII
            {
                stream.Position--; // Unread this byte

                break;
            }

            timestampBytes.Add((byte)b);
        }

        if(timestampBytes.Count == 0) return null;

        return Encoding.ASCII.GetString(timestampBytes.ToArray()).Trim();
    }
}