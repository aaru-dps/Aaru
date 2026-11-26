// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Internal.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : JXFileSystem plugin.
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

/// <inheritdoc />
public sealed partial class JXFS
{
#region Nested type: JxfsDirNode

    /// <summary>An open directory</summary>
    sealed class JxfsDirNode : IDirNode
    {
        /// <summary>Next entry to return</summary>
        public int Position { get; set; }

        /// <summary>Entry names</summary>
        public string[] Entries { get; set; }

#region IDirNode Members

        /// <inheritdoc />
        public string Path { get; set; }

#endregion
    }

#endregion

#region Nested type: JxfsFileNode

    /// <summary>An open file</summary>
    sealed class JxfsFileNode : IFileNode
    {
        /// <summary>Whether the file keeps its data in a chain of data blocks in <c>:misc:</c></summary>
        public bool SmallFile { get; set; }

        /// <summary>First data block of a small file, relative to <c>:misc:</c></summary>
        public uint FirstData { get; set; }

        /// <summary>Data block of a small file read last, relative to <c>:misc:</c></summary>
        public uint ChainBlock { get; set; }

        /// <summary>Index in the file of the data block read last</summary>
        public long ChainIndex { get; set; }

        /// <summary>Contents of the data block read last</summary>
        public byte[] ChainData { get; set; }

        /// <summary>Extents of a file that is not a small file</summary>
        public List<Extent> Extents { get; set; }

#region IFileNode Members

        /// <inheritdoc />
        public string Path { get; set; }

        /// <inheritdoc />
        public long Length { get; set; }

        /// <inheritdoc />
        public long Offset { get; set; }

#endregion
    }

#endregion
}