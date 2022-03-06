// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Internal.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Internal structures.
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
// Copyright © 2011-2022 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using System.Collections.Generic;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
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
        public byte[]                         AmigaComment;
        public AmigaProtection?               AmigaProtection;
        public byte?                          AppleDosType;
        public byte[]                         AppleIcon;
        public ushort?                        AppleProDosType;
        public DecodedDirectoryEntry          AssociatedFile;
        public CdiSystemArea?                 CdiSystemArea;
        public List<(uint extent, uint size)> Extents;
        public string                         Filename;
        public byte                           FileUnitSize;
        public AppleCommon.FInfo?             FinderInfo;
        public FileFlags                      Flags;
        public byte                           Interleave;
        public PosixAttributes?               PosixAttributes;
        public PosixAttributesOld?            PosixAttributesOld;
        public PosixDeviceNumber?             PosixDeviceNumber;
        public DecodedDirectoryEntry          ResourceFork;
        public byte[]                         RockRidgeAlternateName;
        public bool                           RockRidgeRelocated;
        public byte[]                         RripAccess;
        public byte[]                         RripAttributeChange;
        public byte[]                         RripBackup;
        public byte[]                         RripCreation;
        public byte[]                         RripEffective;
        public byte[]                         RripExpiration;
        public byte[]                         RripModify;
        public ulong                          Size;
        public string                         SymbolicLink;
        public DateTime?                      Timestamp;
        public ushort                         VolumeSequenceNumber;
        public CdromXa?                       XA;
        public byte                           XattrLength;

        public override string ToString() => Filename;
    }

    class PathTableEntryInternal
    {
        public uint   Extent;
        public string Name;
        public ushort Parent;
        public byte   XattrLength;

        public override string ToString() => Name;
    }
}