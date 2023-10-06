// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constants.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for Aaru Format disk images.
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

namespace Aaru.Images;

public sealed partial class AaruFormat
{
    /// <summary>Old magic identifier = "DICMFRMT".</summary>
    const ulong DIC_MAGIC = 0x544D52464D434944;
    /// <summary>Magic identifier = "AARUFRMT".</summary>
    const ulong AARU_MAGIC = 0x544D524655524141;
    /// <summary>
    ///     Image format version. A change in this number indicates an incompatible change to the format that prevents
    ///     older implementations from reading it correctly, if at all.
    /// </summary>
    const byte AARUFMT_VERSION_V1 = 1;
    /// <summary>Adds new index format with 64-bit entries counter</summary>
    const byte AARUFMT_VERSION = 2;
    /// <summary>Maximum read cache size, 256MiB.</summary>
    const uint MAX_CACHE_SIZE = 256 * 1024 * 1024;
    /// <summary>Size in bytes of LZMA properties.</summary>
    const int LZMA_PROPERTIES_LENGTH = 5;
    /// <summary>Maximum number of entries for the DDT cache.</summary>
    const int MAX_DDT_ENTRY_CACHE = 16000000;
    /// <summary>How many samples are contained in a RedBook sector.</summary>
    const int SAMPLES_PER_SECTOR = 588;
    /// <summary>Maximum number of samples for a FLAC block. Bigger than 4608 gives no benefit.</summary>
    const int MAX_FLAKE_BLOCK = 4608;
    /// <summary>
    ///     Minimum number of samples for a FLAC block. <see cref="CUETools.Codecs.Flake" /> does not support it to be
    ///     smaller than 256.
    /// </summary>
    const int MIN_FLAKE_BLOCK = 256;
    /// <summary>This mask is to check for flags in CompactDisc suffix/prefix DDT</summary>
    const uint CD_XFIX_MASK = 0xFF000000;
    /// <summary>This mask is to check for position in CompactDisc suffix/prefix deduplicated block</summary>
    const uint CD_DFIX_MASK = 0x00FFFFFF;
}