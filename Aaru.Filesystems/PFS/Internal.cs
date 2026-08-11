// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Internal.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Professional File System plugin.
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

using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class PFS
{
#region Nested type: DirEntryCacheItem

    /// <summary>Cached directory entry information</summary>
    sealed class DirEntryCacheItem
    {
        /// <summary>Anode number for this entry</summary>
        public uint           Anode          { get; init; }
        /// <summary>Entry type (file, directory, link, etc.)</summary>
        public EntryType      Type           { get; init; }
        /// <summary>File size in bytes</summary>
        public uint           Size           { get; set; }
        /// <summary>Protection bits</summary>
        public ProtectionBits Protection     { get; init; }
        /// <summary>Creation day (days since Jan 1, 1978)</summary>
        public ushort         CreationDay    { get; init; }
        /// <summary>Creation minute</summary>
        public ushort         CreationMinute { get; init; }
        /// <summary>Creation tick</summary>
        public ushort         CreationTick   { get; init; }
        /// <summary>File comment</summary>
        public string         Comment        { get; set; }
    }

#endregion

#region Nested type: PFSDirNode

    /// <summary>Directory node for enumerating directory contents</summary>
    sealed class PFSDirNode : IDirNode
    {
        /// <summary>Current position in the directory enumeration (entry index)</summary>
        internal int Position { get; set; }

        /// <summary>Array of directory entry names in this directory</summary>
        internal string[] Entries { get; set; }

        /// <inheritdoc />
        public string Path { get; init; }
    }

#endregion

#region Nested type: PFSFileNode

    /// <summary>File node for reading file contents</summary>
    sealed class PFSFileNode : IFileNode
    {
        /// <summary>Starting anode number for the file</summary>
        internal uint StartAnode { get; init; }

        /// <summary>Current anode in the chain</summary>
        internal Anode CurrentAnode { get; set; }

        /// <summary>Offset within current anode's cluster (in blocks)</summary>
        internal uint AnodeOffset { get; set; }

        /// <summary>Offset within current block (in bytes)</summary>
        internal uint BlockOffset { get; set; }

        /// <summary>File size in bytes</summary>
        internal uint FileSize { get; init; }

        /// <summary>File position the anode/block cursor corresponds to</summary>
        internal long CursorPosition { get; set; }

        /// <inheritdoc />
        public string Path { get; init; }

        /// <inheritdoc />
        public long Length { get; init; }

        /// <inheritdoc />
        public long Offset { get; set; }
    }

#endregion
}