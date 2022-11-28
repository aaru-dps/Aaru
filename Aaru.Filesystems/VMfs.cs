// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VMfs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : VMware file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the VMware file system and shows information.
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the VMware filesystem</summary>
[SuppressMessage("ReSharper", "UnusedType.Local"), SuppressMessage("ReSharper", "IdentifierTypo"),
 SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class VMfs : IFilesystem
{
    /// <summary>Identifier for VMfs</summary>
    const uint VMFS_MAGIC = 0xC001D00D;
    const uint VMFS_BASE = 0x00100000;

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.VMfs_Name;
    /// <inheritdoc />
    public Guid Id => new("EE52BDB8-B49C-4122-A3DA-AD21CBE79843");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start >= partition.End)
            return false;

        ulong vmfsSuperOff = VMFS_BASE / imagePlugin.Info.SectorSize;

        if(partition.Start + vmfsSuperOff > partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start + vmfsSuperOff, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        uint magic = BitConverter.ToUInt32(sector, 0x00);

        return magic == VMFS_MAGIC;
    }

    const string FS_TYPE = "vmfs";

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.UTF8;
        information = "";
        ulong       vmfsSuperOff = VMFS_BASE / imagePlugin.Info.SectorSize;
        ErrorNumber errno        = imagePlugin.ReadSector(partition.Start + vmfsSuperOff, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        VolumeInfo volInfo = Marshal.ByteArrayToStructureLittleEndian<VolumeInfo>(sector);

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine(Localization.VMware_file_system);

        uint ctimeSecs     = (uint)(volInfo.ctime / 1000000);
        uint ctimeNanoSecs = (uint)(volInfo.ctime % 1000000);
        uint mtimeSecs     = (uint)(volInfo.mtime / 1000000);
        uint mtimeNanoSecs = (uint)(volInfo.mtime % 1000000);

        sbInformation.AppendFormat(Localization.Volume_version_0, volInfo.version).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(volInfo.name, Encoding)).
                      AppendLine();

        sbInformation.AppendFormat(Localization.Volume_size_0_bytes, volInfo.size * 256).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_UUID_0, volInfo.uuid).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_created_on_0,
                                   DateHandlers.UnixUnsignedToDateTime(ctimeSecs, ctimeNanoSecs)).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_last_modified_on_0,
                                   DateHandlers.UnixUnsignedToDateTime(mtimeSecs, mtimeNanoSecs)).AppendLine();

        information = sbInformation.ToString();

        XmlFsType = new FileSystemType
        {
            Type                      = FS_TYPE,
            CreationDate              = DateHandlers.UnixUnsignedToDateTime(ctimeSecs, ctimeNanoSecs),
            CreationDateSpecified     = true,
            ModificationDate          = DateHandlers.UnixUnsignedToDateTime(mtimeSecs, mtimeNanoSecs),
            ModificationDateSpecified = true,
            Clusters                  = volInfo.size * 256 / imagePlugin.Info.SectorSize,
            ClusterSize               = imagePlugin.Info.SectorSize,
            VolumeSerial              = volInfo.uuid.ToString()
        };
    }

    [Flags]
    enum Flags : byte
    {
        RecyledFolder = 64, CaseSensitive = 128
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct VolumeInfo
    {
        public readonly uint magic;
        public readonly uint version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] unknown1;
        public readonly byte lun;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] unknown2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        public readonly byte[] name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 49)]
        public readonly byte[] unknown3;
        public readonly uint size;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31)]
        public readonly byte[] unknown4;
        public readonly Guid  uuid;
        public readonly ulong ctime;
        public readonly ulong mtime;
    }
}