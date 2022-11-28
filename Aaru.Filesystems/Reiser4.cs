// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Reiser4.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser4 filesystem plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Reiser4 filesystem and shows information.
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
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Reiser v4 filesystem</summary>
public sealed class Reiser4 : IFilesystem
{
    const uint REISER4_SUPER_OFFSET = 0x10000;

    const string FS_TYPE = "reiser4";

    readonly byte[] _magic =
    {
        0x52, 0x65, 0x49, 0x73, 0x45, 0x72, 0x34, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    };

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.Reiser4_Name;
    /// <inheritdoc />
    public Guid Id => new("301F2D00-E8D5-4F04-934E-81DFB21D15BA");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 512)
            return false;

        uint sbAddr = REISER4_SUPER_OFFSET / imagePlugin.Info.SectorSize;

        if(sbAddr == 0)
            sbAddr = 1;

        uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
            sbSize++;

        if(partition.Start + sbAddr + sbSize >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(sector.Length < Marshal.SizeOf<Superblock>())
            return false;

        Superblock reiserSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

        return _magic.SequenceEqual(reiserSb.magic);
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        if(imagePlugin.Info.SectorSize < 512)
            return;

        uint sbAddr = REISER4_SUPER_OFFSET / imagePlugin.Info.SectorSize;

        if(sbAddr == 0)
            sbAddr = 1;

        uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
            sbSize++;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        if(sector.Length < Marshal.SizeOf<Superblock>())
            return;

        Superblock reiserSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

        if(!_magic.SequenceEqual(reiserSb.magic))
            return;

        var sb = new StringBuilder();

        sb.AppendLine(Localization.Reiser_4_filesystem);
        sb.AppendFormat(Localization._0_bytes_per_block, reiserSb.blocksize).AppendLine();
        sb.AppendFormat(Localization.Volume_disk_format_0, reiserSb.diskformat).AppendLine();
        sb.AppendFormat(Localization.Volume_UUID_0, reiserSb.uuid).AppendLine();
        sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(reiserSb.label, Encoding)).AppendLine();

        information = sb.ToString();

        XmlFsType = new FileSystemType
        {
            Type         = FS_TYPE,
            ClusterSize  = reiserSb.blocksize,
            Clusters     = (partition.End - partition.Start) * imagePlugin.Info.SectorSize / reiserSb.blocksize,
            VolumeName   = StringHandlers.CToString(reiserSb.label, Encoding),
            VolumeSerial = reiserSb.uuid.ToString()
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Superblock
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] magic;
        public readonly ushort diskformat;
        public readonly ushort blocksize;
        public readonly Guid   uuid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] label;
    }
}