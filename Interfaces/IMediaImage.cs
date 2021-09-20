// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IMediaImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines interface to be implemented by block addressable disk image
//     plugins.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.CommonTypes.Interfaces
{
    /// <summary>Abstract class to implement disk image reading plugins.</summary>
    public interface IMediaImage
    {
        /// <summary>Image information</summary>
        ImageInfo Info { get; }
        /// <summary>Plugin name.</summary>
        string Name { get; }
        /// <summary>Plugin UUID.</summary>
        Guid Id { get; }
        /// <summary>Plugin author</summary>
        string Author { get; }
        /// <summary>Gets the image format.</summary>
        /// <value>The image format.</value>
        string Format { get; }
        /// <summary>List of dump hardware used to create the image from real media</summary>
        List<DumpHardwareType> DumpHardware { get; }
        /// <summary>Gets the CICM XML metadata for the image</summary>
        CICMMetadataType CicmMetadata { get; }

        /// <summary>Identifies the image.</summary>
        /// <returns><c>true</c>, if image was identified, <c>false</c> otherwise.</returns>
        /// <param name="imageFilter">Image filter.</param>
        bool Identify(IFilter imageFilter);

        /// <summary>Opens the image.</summary>
        /// <returns><c>true</c>, if image was opened, <c>false</c> otherwise.</returns>
        /// <param name="imageFilter">Image filter.</param>
        ErrorNumber Open(IFilter imageFilter);

        /// <summary>Reads a disk tag.</summary>
        /// <returns></returns>
        /// <param name="tag">Tag type to read.</param>
        /// <param name="buffer">Disk tag</param>
        ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer);

        /// <summary>Reads a sector's user data.</summary>
        /// <returns>The sector's user data.</returns>
        /// <param name="sectorAddress">Sector address (LBA).</param>
        /// <param name="buffer"></param>
        ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer);

        /// <summary>Reads a sector's tag.</summary>
        /// <returns>The sector's tag.</returns>
        /// <param name="sectorAddress">Sector address (LBA).</param>
        /// <param name="tag">Tag type.</param>
        /// <param name="buffer"></param>
        ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer);

        /// <summary>Reads user data from several sectors.</summary>
        /// <returns>The sectors user data.</returns>
        /// <param name="sectorAddress">Starting sector address (LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="buffer"></param>
        ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer);

        /// <summary>Reads tag from several sectors.</summary>
        /// <returns>The sectors tag.</returns>
        /// <param name="sectorAddress">Starting sector address (LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="tag">Tag type.</param>
        /// <param name="buffer"></param>
        ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer);

        /// <summary>Reads a complete sector (user data + all tags).</summary>
        /// <returns>The complete sector. Format depends on disk type.</returns>
        /// <param name="sectorAddress">Sector address (LBA).</param>
        /// <param name="buffer"></param>
        ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer);

        /// <summary>Reads several complete sector (user data + all tags).</summary>
        /// <returns>The complete sectors. Format depends on disk type.</returns>
        /// <param name="sectorAddress">Starting sector address (LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="buffer"></param>
        ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer);
    }
}