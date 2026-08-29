// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Internal.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Files-11 On-Disk Structure plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Internal structures for the Files-11 On-Disk Structure.
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

using System.Collections.Generic;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

public sealed partial class ODS
{
#region Nested type: CachedFile

    /// <summary>Cached file information from a directory.</summary>
    internal sealed class CachedFile
    {
        /// <summary>File ID (number, sequence, rvn, nmx).</summary>
        internal FileId Fid;

        /// <summary>File version number.</summary>
        internal ushort Version;
    }

#endregion

#region Nested type: OdsDirNode

    /// <summary>Directory node for ODS directory traversal.</summary>
    sealed class OdsDirNode : IDirNode
    {
        /// <summary>Array of cached directory entry information.</summary>
        internal (string Filename, CachedFile File)[] Entries;

        /// <summary>Current position in the directory contents array.</summary>
        internal int Position;

#region IDirNode Members

        /// <inheritdoc />
        public string Path { get; init; }

#endregion
    }

#endregion

#region Nested type: OdsFileNode

    /// <inheritdoc />
    /// <summary>File node for ODS file reading.</summary>
    sealed class OdsFileNode : IFileNode
    {
        /// <summary>File ID.</summary>
        internal FileId Fid;

        /// <summary>Cached file header.</summary>
        internal FileHeader FileHeader;

        /// <summary>Cached retrieval pointers (mapping information) from primary header.</summary>
        internal byte[] MapData;

        /// <summary>Cached extension file headers for multi-extent files.</summary>
        internal List<ExtensionMapInfo> ExtensionMaps;

#region IFileNode Members

        /// <inheritdoc />
        public string Path { get; init; }

        /// <inheritdoc />
        public long Length { get; set; }

        /// <inheritdoc />
        public long Offset { get; set; }

#endregion
    }

#endregion

#region Nested type: ExtensionMapInfo

    /// <summary>Cached extension file header mapping information.</summary>
    sealed class ExtensionMapInfo
    {
        /// <summary>Extension file ID.</summary>
        internal FileId ExtFid;

        /// <summary>Mapping data from this extension header.</summary>
        internal byte[] MapData;

        /// <summary>Number of map words in use.</summary>
        internal byte MapInUse;

        /// <summary>VBN sum before this extension (cumulative block count).</summary>
        internal uint VbnSum;
    }

#endregion
}