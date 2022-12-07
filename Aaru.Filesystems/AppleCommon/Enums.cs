// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common Apple file systems.
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

// ReSharper disable InconsistentNaming

using System;

namespace Aaru.Filesystems;

// Information from Inside Macintosh
// https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
static partial class AppleCommon
{
    [Flags]
    internal enum VolumeAttributes : ushort
    {
        HardwareLock     = 0x80, Unmounted         = 0x100, SparedBadBlocks = 0x200,
        DoesNotNeedCache = 0x400, BootInconsistent = 0x800, ReusedIds       = 0x1000,
        Journaled        = 0x2000, Inconsistent    = 0x4000, SoftwareLock   = 0x8000
    }

    [Flags]
    internal enum FinderFlags : ushort
    {
        /// <summary>Is on desktop.</summary>
        kIsOnDesk = 0x0001,
        /// <summary>Color mask.</summary>
        kColor = 0x000E, kRequireSwitchLaunch = 0x0020,
        /// <summary>If clear, the application needs to write to its resource fork, and therefore cannot be shared on a server.</summary>
        kIsShared = 0x0040,
        /// <summary>Extension or control panel with no INIT entries in resource fork.</summary>
        kHasNoINITs = 0x0080,
        /// <summary>
        ///     Clear if the file contains desktop database resources ('BNDL', 'FREF', 'open', 'kind'...) that have not been
        ///     added yet. Set only by the Finder. Reserved for folders - make sure this bit is cleared for folders.
        /// </summary>
        kHasBeenInited = 0x0100,
        /// <summary>PowerTalk</summary>
        kAOCE = 0x200, kChanged = 0x0200,
        /// <summary>Has a custom icon in the resource fork.</summary>
        kHasCustomIcon = 0x0400,
        /// <summary>Is a stationery.</summary>
        kIsStationery = 0x0800,
        /// <summary>Cannot be renamed.</summary>
        kNameLocked = 0x1000,
        /// <summary>Indicates that a file has a BNDL resource or that a folder is displayed as a package.</summary>
        kHasBundle = 0x2000,
        /// <summary>Hidden.</summary>
        kIsInvisible = 0x4000,
        /// <summary>Is an alias</summary>
        kIsAlias = 0x8000
    }

    internal enum FinderFolder : short
    {
        fTrash = -3, fDesktop = -2, fDisk = 0
    }

    [Flags]
    internal enum ExtendedFinderFlags : ushort
    {
        /// <summary>If set the other extended flags are ignored.</summary>
        kExtendedFlagsAreInvalid = 0x8000,
        /// <summary>Set if the file or folder has a badge resource.</summary>
        kExtendedFlagHasCustomBadge = 0x0100,
        /// <summary>Set if the object is marked as busy/incomplete.</summary>
        kExtendedFlagObjectIsBusy = 0x0080,
        /// <summary>Set if the file contains routing info resource.</summary>
        kExtendedFlagHasRoutingInfo = 0x0004
    }
}