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
//     Contains constants for partimage disk images.
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

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class Partimage
{
    const int    MAX_DESCRIPTION         = 4096;
    const int    MAX_HOSTNAMESIZE        = 128;
    const int    MAX_DEVICENAMELEN       = 512;
    const int    MAX_UNAMEINFOLEN        = 65; //SYS_NMLN
    const int    MBR_SIZE_WHOLE          = 512;
    const int    MAX_DESC_MODEL          = 128;
    const int    MAX_DESC_GEOMETRY       = 1024;
    const int    MAX_DESC_IDENTIFY       = 4096;
    const int    CHECK_FREQUENCY         = 65536;
    const string MAGIC_BEGIN_LOCALHEADER = "MAGIC-BEGIN-LOCALHEADER";
    const string MAGIC_BEGIN_DATABLOCKS  = "MAGIC-BEGIN-DATABLOCKS";
    const string MAGIC_BEGIN_BITMAP      = "MAGIC-BEGIN-BITMAP";
    const string MAGIC_BEGIN_MBRBACKUP   = "MAGIC-BEGIN-MBRBACKUP";
    const string MAGIC_BEGIN_TAIL        = "MAGIC-BEGIN-TAIL";
    const string MAGIC_BEGIN_INFO        = "MAGIC-BEGIN-INFO";
    const string MAGIC_BEGIN_EXT000      = "MAGIC-BEGIN-EXT000"; // reserved for future use
    const string MAGIC_BEGIN_EXT001      = "MAGIC-BEGIN-EXT001"; // reserved for future use
    const string MAGIC_BEGIN_EXT002      = "MAGIC-BEGIN-EXT002"; // reserved for future use
    const string MAGIC_BEGIN_EXT003      = "MAGIC-BEGIN-EXT003"; // reserved for future use
    const string MAGIC_BEGIN_EXT004      = "MAGIC-BEGIN-EXT004"; // reserved for future use
    const string MAGIC_BEGIN_EXT005      = "MAGIC-BEGIN-EXT005"; // reserved for future use
    const string MAGIC_BEGIN_EXT006      = "MAGIC-BEGIN-EXT006"; // reserved for future use
    const string MAGIC_BEGIN_EXT007      = "MAGIC-BEGIN-EXT007"; // reserved for future use
    const string MAGIC_BEGIN_EXT008      = "MAGIC-BEGIN-EXT008"; // reserved for future use
    const string MAGIC_BEGIN_EXT009      = "MAGIC-BEGIN-EXT009"; // reserved for future use
    const string MAGIC_BEGIN_VOLUME      = "PaRtImAgE-VoLuMe";
    const uint   MAX_CACHE_SIZE          = 16777216;
    const uint   MAX_CACHED_SECTORS      = MAX_CACHE_SIZE / 512;
    readonly byte[] _partimageMagic =
    {
        0x50, 0x61, 0x52, 0x74, 0x49, 0x6D, 0x41, 0x67, 0x45, 0x2D, 0x56, 0x6F, 0x4C, 0x75, 0x4D, 0x65, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    };
}