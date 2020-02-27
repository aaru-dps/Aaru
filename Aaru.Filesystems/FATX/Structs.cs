// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     FATX filesystem structures.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Filesystems.FATX
{
    public partial class XboxFatPlugin
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Superblock
        {
            public uint magic;
            public uint id;
            public uint sectorsPerCluster;
            public uint rootDirectoryCluster;
            // TODO: Undetermined size
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] volumeLabel;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DirectoryEntry
        {
            public byte       filenameSize;
            public Attributes attributes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_FILENAME)]
            public byte[] filename;
            public uint   firstCluster;
            public uint   length;
            public ushort lastWrittenTime;
            public ushort lastWrittenDate;
            public ushort creationTime;
            public ushort creationDate;
            public ushort lastAccessTime;
            public ushort lastAccessDate;
        }
    }
}