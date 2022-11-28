// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ECMA67.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ECMA-67 plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the ECMA-67 file system and shows information.
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
// ****************************************************************************/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the filesystem described in ECMA-67</summary>
public sealed class ECMA67 : IFilesystem
{
    const string FS_TYPE = "ecma67";
    readonly byte[] _magic =
    {
        0x56, 0x4F, 0x4C
    };

    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.ECMA67_Name;
    /// <inheritdoc />
    public Guid Id => new("62A2D44A-CBC1-4377-B4B6-28C5C92034A1");
    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start > 0)
            return false;

        if(partition.End < 8)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(6, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(sector.Length != 128)
            return false;

        VolumeLabel vol = Marshal.ByteArrayToStructureLittleEndian<VolumeLabel>(sector);

        return _magic.SequenceEqual(vol.labelIdentifier) && vol is { labelNumber: 1, recordLength: 0x31 };
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-1");
        information = "";
        ErrorNumber errno = imagePlugin.ReadSector(6, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        var sbInformation = new StringBuilder();

        VolumeLabel vol = Marshal.ByteArrayToStructureLittleEndian<VolumeLabel>(sector);

        sbInformation.AppendLine(Localization.ECMA_67);

        sbInformation.AppendFormat(Localization.Volume_name_0, Encoding.ASCII.GetString(vol.volumeIdentifier)).
                      AppendLine();

        sbInformation.AppendFormat(Localization.Volume_owner_0, Encoding.ASCII.GetString(vol.owner)).AppendLine();

        XmlFsType = new FileSystemType
        {
            Type        = FS_TYPE,
            ClusterSize = 256,
            Clusters    = partition.End - partition.Start + 1,
            VolumeName  = Encoding.ASCII.GetString(vol.volumeIdentifier)
        };

        information = sbInformation.ToString();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct VolumeLabel
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] labelIdentifier;
        public readonly byte labelNumber;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] volumeIdentifier;
        public readonly byte volumeAccessibility;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
        public readonly byte[] reserved1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public readonly byte[] owner;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] reserved2;
        public readonly byte surface;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] reserved3;
        public readonly byte recordLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] reserved4;
        public readonly byte fileLabelAllocation;
        public readonly byte labelStandardVersion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public readonly byte[] reserved5;
    }
}