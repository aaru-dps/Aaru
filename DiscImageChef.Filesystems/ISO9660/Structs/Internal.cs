// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Common.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Common structures.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        struct DecodedVolumeDescriptor
        {
            public string   SystemIdentifier;
            public string   VolumeIdentifier;
            public string   VolumeSetIdentifier;
            public string   PublisherIdentifier;
            public string   DataPreparerIdentifier;
            public string   ApplicationIdentifier;
            public DateTime CreationTime;
            public bool     HasModificationTime;
            public DateTime ModificationTime;
            public bool     HasExpirationTime;
            public DateTime ExpirationTime;
            public bool     HasEffectiveTime;
            public DateTime EffectiveTime;
            public ushort   BlockSize;
            public uint     Blocks;
        }

        class DecodedDirectoryEntry
        {
            public byte?                 AppleDosType;
            public byte[]                AppleIcon;
            public ushort?               AppleProDosType;
            public DecodedDirectoryEntry AssociatedFile;
            public uint                  Extent;
            public string                Filename;
            public byte                  FileUnitSize;
            public FinderInfo            FinderInfo;
            public FileFlags             Flags;
            public byte                  Interleave;
            public DecodedDirectoryEntry ResourceFork;
            public uint                  Size;
            public DateTime?             Timestamp;
            public ushort                VolumeSequenceNumber;
            public CdromXa?              XA;

            public override string ToString() => Filename;
        }

        [Flags]
        enum FinderFlags : ushort
        {
            kIsOnDesk            = 0x0001,
            kColor               = 0x000E,
            kRequireSwitchLaunch = 0x0020,
            kIsShared            = 0x0040,
            kHasNoINITs          = 0x0080,
            kHasBeenInited       = 0x0100,
            kHasCustomIcon       = 0x0400,
            kLetter              = 0x0200,
            kChanged             = 0x0200,
            kIsStationery        = 0x0800,
            kNameLocked          = 0x1000,
            kHasBundle           = 0x2000,
            kIsInvisible         = 0x4000,
            kIsAlias             = 0x8000
        }

        struct Point
        {
            public short x;
            public short y;
        }

        class FinderInfo
        {
            public uint        fdCreator;
            public FinderFlags fdFlags;
            public short       fdFldr;
            public Point       fdLocation;
            public uint        fdType;
        }
    }
}