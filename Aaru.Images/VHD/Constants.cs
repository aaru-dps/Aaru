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
//     Contains constants for Connectix and Microsoft Virtual PC disk images.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class Vhd
{
    /// <summary>File magic number, "conectix"</summary>
    const ulong IMAGE_COOKIE = 0x636F6E6563746978;
    /// <summary>Dynamic disk header magic, "cxsparse"</summary>
    const ulong DYNAMIC_COOKIE = 0x6378737061727365;

    /// <summary>Disk image is candidate for deletion on shutdown</summary>
    const uint FEATURES_TEMPORARY = 0x00000001;
    /// <summary>Unknown, set from Virtual PC for Mac 7 onwards</summary>
    const uint FEATURES_RESERVED = 0x00000002;
    /// <summary>Unknown</summary>
    const uint FEATURES_UNKNOWN = 0x00000100;

    /// <summary>Only known version</summary>
    const uint VERSION1 = 0x00010000;

    /// <summary>Created by Virtual PC, "vpc "</summary>
    const uint CREATOR_VIRTUAL_PC = 0x76706320;
    /// <summary>Created by Virtual Server, "vs  "</summary>
    const uint CREATOR_VIRTUAL_SERVER = 0x76732020;
    /// <summary>Created by QEMU, "qemu"</summary>
    const uint CREATOR_QEMU = 0x71656D75;
    /// <summary>Created by VirtualBox, "vbox"</summary>
    const uint CREATOR_VIRTUAL_BOX = 0x76626F78;
    /// <summary>Created by DiscImageChef, "dic "</summary>
    const uint CREATOR_DISCIMAGECHEF = 0x64696320;
    /// <summary>Created by Aaru, "aaru"</summary>
    const uint CREATOR_AARU = 0x61617275;
    /// <summary>Disk image created by Virtual Server 2004</summary>
    const uint VERSION_VIRTUAL_SERVER2004 = 0x00010000;
    /// <summary>Disk image created by Virtual PC 2004</summary>
    const uint VERSION_VIRTUAL_PC2004 = 0x00050000;
    /// <summary>Disk image created by Virtual PC 2007</summary>
    const uint VERSION_VIRTUAL_PC2007 = 0x00050003;
    /// <summary>Disk image created by Virtual PC for Mac 5, 6 or 7</summary>
    const uint VERSION_VIRTUAL_PC_MAC = 0x00040000;

    /// <summary>Disk image created in Windows, "Wi2k"</summary>
    const uint CREATOR_WINDOWS = 0x5769326B;
    /// <summary>Disk image created in Macintosh, "Mac "</summary>
    const uint CREATOR_MACINTOSH = 0x4D616320;
    /// <summary>Seen in Virtual PC for Mac for dynamic and fixed images</summary>
    const uint CREATOR_MACINTOSH_OLD = 0x00000000;

    /// <summary>Disk image type is none, useless?</summary>
    const uint TYPE_NONE = 0;
    /// <summary>Deprecated disk image type</summary>
    const uint TYPE_DEPRECATED1 = 1;
    /// <summary>Fixed disk image type</summary>
    const uint TYPE_FIXED = 2;
    /// <summary>Dynamic disk image type</summary>
    const uint TYPE_DYNAMIC = 3;
    /// <summary>Differencing disk image type</summary>
    const uint TYPE_DIFFERENCING = 4;
    /// <summary>Deprecated disk image type</summary>
    const uint TYPE_DEPRECATED2 = 5;
    /// <summary>Deprecated disk image type</summary>
    const uint TYPE_DEPRECATED3 = 6;

    /// <summary>Means platform locator is unused</summary>
    const uint PLATFORM_CODE_UNUSED = 0x00000000;
    /// <summary>Stores a relative path string for Windows, unknown locale used, deprecated, "Wi2r"</summary>
    const uint PLATFORM_CODE_WINDOWS_RELATIVE = 0x57693272;
    /// <summary>Stores an absolute path string for Windows, unknown locale used, deprecated, "Wi2k"</summary>
    const uint PLATFORM_CODE_WINDOWS_ABSOLUTE = 0x5769326B;
    /// <summary>Stores a relative path string for Windows in UTF-16, "W2ru"</summary>
    const uint PLATFORM_CODE_WINDOWS_RELATIVE_U = 0x57327275;
    /// <summary>Stores an absolute path string for Windows in UTF-16, "W2ku"</summary>
    const uint PLATFORM_CODE_WINDOWS_ABSOLUTE_U = 0x57326B75;
    /// <summary>Stores a Mac OS alias as a blob, "Mac "</summary>
    const uint PLATFORM_CODE_MACINTOSH_ALIAS = 0x4D616320;
    /// <summary>Stores a Mac OS X URI (RFC-2396) absolute path in UTF-8, "MacX"</summary>
    const uint PLATFORM_CODE_MACINTOSH_URI = 0x4D616358;
}