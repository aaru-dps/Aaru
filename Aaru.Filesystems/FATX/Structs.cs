// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin.
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

using System.Runtime.InteropServices;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

public sealed partial class XboxFatPlugin
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Superblock
    {
        public readonly uint magic;
        public readonly uint id;
        public readonly uint sectorsPerCluster;
        public readonly uint rootDirectoryCluster;

        // TODO: Undetermined size
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] volumeLabel;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DirectoryEntry
    {
        public readonly byte       filenameSize;
        public readonly Attributes attributes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_FILENAME)]
        public readonly byte[] filename;
        public readonly uint   firstCluster;
        public readonly uint   length;
        public readonly ushort lastWrittenTime;
        public readonly ushort lastWrittenDate;
        public readonly ushort lastAccessTime;
        public readonly ushort lastAccessDate;
        public readonly ushort creationTime;
        public readonly ushort creationDate;
    }

    sealed class FatxFileNode : IFileNode
    {
        internal uint[] _clusters;
        /// <inheritdoc />
        public string Path { get; init; }
        /// <inheritdoc />
        public long Length { get; init; }
        /// <inheritdoc />
        public long Offset { get; init; }
    }
}