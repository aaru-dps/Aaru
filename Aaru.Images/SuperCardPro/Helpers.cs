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
// Copyright © 2011-2023 Natalia Portillo
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
        if(position == 0)
            return null;

        stream.Position = position;
        var lenB = new byte[2];
        stream.EnsureRead(lenB, 0, 2);
        var len = BitConverter.ToUInt16(lenB, 0);

        if(len == 0 || len + stream.Position >= stream.Length)
            return null;

        var str = new byte[len];
        stream.EnsureRead(str, 0, len);

        return Encoding.UTF8.GetString(str);
    }

    /// <summary>
    ///     Takes a Head, Track and Sub-Track representation and converts it to the Track representation used by SCP.
    /// </summary>
    /// <param name="head">The head number</param>
    /// <param name="track">The track number</param>
    /// <param name="subTrack">The sub-track number</param>
    /// <returns>SCP format track number</returns>
    // ReSharper disable once UnusedParameter.Local
    static long HeadTrackSubToScpTrack(uint head, ushort track, byte subTrack) =>

        // TODO: Support single-sided disks
        head + track * 2;

    static byte[] UInt32ToFluxRepresentation(uint ticks)
    {
        uint over = ticks / 255;

        if(over == 0)
        {
            return new[]
            {
                (byte)ticks
            };
        }

        var expanded = new byte[over + 1];

        Array.Fill(expanded, (byte)255, 0, (int)over);
        expanded[^1] = (byte)(ticks % 255);

        return expanded;
    }

    static byte[] UInt16ToFluxRepresentation(ushort ticks) => UInt32ToFluxRepresentation(ticks);

    static List<uint> FluxRepresentationsToUInt32List(IEnumerable<byte> flux)
    {
        List<uint> scpData = new();
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
        List<byte> scpData = new();
        ushort     tick    = 0;

        List<uint> revolutionLength = new();
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

        for(int i = HEADER_OFFSET; i < wholeFile.Length; i++)
            sum += wholeFile[i];

        return sum;
    }
}