// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Internal.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
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
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
#region Nested type: DecodedDirectoryEntry

    sealed class DecodedDirectoryEntry
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

        // ReSharper disable once InconsistentNaming
        public CdromXa? XA;
        public byte     XattrLength;

        public override string ToString() => Filename;
    }

#endregion

#region Nested type: DecodedVolumeDescriptor

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

#endregion

#region Nested type: Iso9660DirNode

    sealed class Iso9660DirNode : IDirNode
    {
        internal DecodedDirectoryEntry[] _entries;
        internal int                     _position;

    #region IDirNode Members

        /// <inheritdoc />
        public string Path { get; init; }

    #endregion
    }

#endregion

#region Nested type: Iso9660FileNode

    sealed class Iso9660FileNode : IFileNode
    {
        internal DecodedDirectoryEntry _dentry;

    #region IFileNode Members

        /// <inheritdoc />
        public string Path { get; init; }

        /// <inheritdoc />
        public long Length { get; init; }

        /// <inheritdoc />
        public long Offset { get; set; }

    #endregion
    }

#endregion

#region Nested type: PathTableEntryInternal

    sealed class PathTableEntryInternal
    {
        public uint   Extent;
        public string Name;
        public ushort Parent;
        public byte   XattrLength;

        public override string ToString() => Name;
    }

#endregion
}