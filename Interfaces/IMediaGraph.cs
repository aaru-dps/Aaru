// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IMediaGraph.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Aaru.CommonTypes.Interfaces;

/// <summary>Defines the interface to draw the dump or verification status of a media in a picture.</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
public interface IMediaGraph
{
    /// <summary>Paints the specified sector in green</summary>
    /// <param name="sector">Sector</param>
    public void PaintSectorGood(ulong sector);

    /// <summary>Paints the specified sector in red</summary>
    /// <param name="sector">Sector</param>
    public void PaintSectorBad(ulong sector);

    /// <summary>Paints the specified sector in yellow</summary>
    /// <param name="sector">Sector</param>
    public void PaintSectorUnknown(ulong sector);

    /// <summary>Paints the specified sector in gray</summary>
    /// <param name="sector">Sector</param>
    public void PaintSectorUndumped(ulong sector);

    /// <summary>Paints a sector with the specified color</summary>
    /// <param name="sector">Sector</param>
    /// <param name="red">Red from 0 to 255</param>
    /// <param name="green">Green from 0 to 255</param>
    /// <param name="blue">Blue from 0 to 255</param>
    /// <param name="opacity">Opacity from 0 to 255</param>
    public void PaintSector(ulong sector, byte red, byte green, byte blue, byte opacity = 0xFF);

    /// <summary>Paints <see cref="length" /> sectors, staring at <see cref="startingSector" /> in gray</summary>
    /// <param name="startingSector">First sector to paint</param>
    /// <param name="length">How many sectors to paint</param>
    public void PaintSectorsUndumped(ulong startingSector, uint length);

    /// <summary>Paints <see cref="length" /> sectors, staring at <see cref="startingSector" /> in green</summary>
    /// <param name="startingSector">First sector to paint</param>
    /// <param name="length">How many sectors to paint</param>
    public void PaintSectorsGood(ulong startingSector, uint length);

    /// <summary>Paints <see cref="length" /> sectors, staring at <see cref="startingSector" /> in red</summary>
    /// <param name="startingSector">First sector to paint</param>
    /// <param name="length">How many sectors to paint</param>
    public void PaintSectorsBad(ulong startingSector, uint length);

    /// <summary>Paints <see cref="length" /> sectors, staring at <see cref="startingSector" /> in yellow</summary>
    /// <param name="startingSector">First sector to paint</param>
    /// <param name="length">How many sectors to paint</param>
    public void PaintSectorsUnknown(ulong startingSector, uint length);

    /// <summary>Paints <see cref="length" /> sectors, staring at <see cref="startingSector" /> in the specified color</summary>
    /// <param name="startingSector">First sector to paint</param>
    /// <param name="length">How many sectors to paint</param>
    /// <param name="red">Red from 0 to 255</param>
    /// <param name="green">Green from 0 to 255</param>
    /// <param name="blue">Blue from 0 to 255</param>
    /// <param name="opacity">Opacity from 0 to 255</param>
    public void PaintSectors(ulong startingSector, uint length, byte red, byte green, byte blue, byte opacity = 0xFF);

    /// <summary>Paints the specified sectors in gray</summary>
    /// <param name="sectors">List of sectors to paint</param>
    public void PaintSectorsUndumped(IEnumerable<ulong> sectors);

    /// <summary>Paints the specified sectors in green</summary>
    /// <param name="sectors">List of sectors to paint</param>
    public void PaintSectorsGood(IEnumerable<ulong> sectors);

    /// <summary>Paints the specified sectors in red</summary>
    /// <param name="sectors">List of sectors to paint</param>
    public void PaintSectorsBad(IEnumerable<ulong> sectors);

    /// <summary>Paints the specified sectors in yellow</summary>
    /// <param name="sectors">List of sectors to paint</param>
    public void PaintSectorsUnknown(IEnumerable<ulong> sectors);

    /// <summary>Paints the specified sectors in the specified color</summary>
    /// <param name="sectors">List of sectors to paint</param>
    /// <param name="red">Red from 0 to 255</param>
    /// <param name="green">Green from 0 to 255</param>
    /// <param name="blue">Blue from 0 to 255</param>
    /// <param name="opacity">Opacity from 0 to 255</param>
    public void PaintSectorsUnknown(IEnumerable<ulong> sectors, byte red, byte green, byte blue, byte opacity = 0xFF);

    /// <summary>Paints the information specific to recordable discs in green</summary>
    public void PaintRecordableInformationGood();

    /// <summary>Writes the graph bitmap as a PNG into the specified stream</summary>
    /// <param name="stream">Stream that will receive the spiral bitmap</param>
    public void WriteTo(Stream stream);

    /// <summary>Writes the graph bitmap as a PNG into the specified stream</summary>
    /// <param name="path">Path to the file to save the PNG to</param>
    public void WriteTo(string path);
}