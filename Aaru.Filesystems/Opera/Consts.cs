// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Opera filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Opera filesystem constants.
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

using System.Diagnostics.CodeAnalysis;
using Aaru.Helpers;

namespace Aaru.Filesystems
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public sealed partial class OperaFS
    {
        const string SYNC       = "ZZZZZ";
        const uint   FLAGS_MASK = 0xFF;
        const int    MAX_NAME   = 32;

        /// <summary>Directory</summary>
        const uint TYPE_DIR = 0x2A646972;
        /// <summary>Disc label</summary>
        const uint TYPE_LBL = 0x2A6C626C;
        /// <summary>Catapult</summary>
        const uint TYPE_ZAP = 0x2A7A6170;
        static readonly int _directoryEntrySize = Marshal.SizeOf<DirectoryEntry>();

        enum FileFlags : uint
        {
            File             = 2, Special            = 6, Directory = 7,
            LastEntryInBlock = 0x40000000, LastEntry = 0x80000000
        }
    }
}