// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AppleHFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Apple Hierarchical File System and shows information.
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

namespace Aaru.Filesystems
{
    public partial class AppleHFS
    {
        internal enum NodeType : sbyte
        {
            /// <summary>Index node</summary>
            ndIndxNode = 0,
            /// <summary>Header node</summary>
            ndHdrNode = 1,
            /// <summary>Map node</summary>
            ndMapNode = 2,
            /// <summary>Leaf node</summary>
            ndLeafNode = -1
        }

        internal enum CatDataType : sbyte
        {
            /// <summary>Directory record</summary>
            cdrDirRec = 1,
            /// <summary>File record</summary>
            cdrFilRec = 2,
            /// <summary>Directory thread record</summary>
            cdrThdRec = 3,
            /// <summary>File thread record</summary>
            cdrFThdRec = 4
        }

        internal enum ForkType : sbyte
        {
            Data = 0, Resource = -1
        }
    }
}