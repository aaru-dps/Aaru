// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AODOS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commodore file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the AO-DOS file system and shows information.
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

// Information has been extracted looking at available disk images
// This may be missing fields, or not, I don't know russian so any help is appreciated
/// <inheritdoc />
/// <summary>Implements detection of the AO-DOS filesystem</summary>
public sealed class AODOS : IFilesystem
{
    readonly byte[] _identifier =
    {
        0x20, 0x41, 0x4F, 0x2D, 0x44, 0x4F, 0x53, 0x20
    };
    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public string Name => "Alexander Osipov DOS file system";
    /// <inheritdoc />
    public Guid Id => new("668E5039-9DDD-442A-BE1B-A315D6E38E26");
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Author => "Natalia Portillo";

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        // Does AO-DOS support hard disks?
        if(partition.Start > 0)
            return false;

        // How is it really?
        if(imagePlugin.Info.SectorSize != 512)
            return false;

        // Does AO-DOS support any other kind of disk?
        if(imagePlugin.Info.Sectors != 800 &&
           imagePlugin.Info.Sectors != 1600)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(0, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        BootBlock bb = Marshal.ByteArrayToStructureLittleEndian<BootBlock>(sector);

        return bb.identifier.SequenceEqual(_identifier);
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        information = "";
        Encoding    = Encoding.GetEncoding("koi8-r");
        ErrorNumber errno = imagePlugin.ReadSector(0, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        BootBlock bb = Marshal.ByteArrayToStructureLittleEndian<BootBlock>(sector);

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine("Alexander Osipov DOS file system");

        XmlFsType = new FileSystemType
        {
            Type                  = "Alexander Osipov DOS file system",
            Clusters              = imagePlugin.Info.Sectors,
            ClusterSize           = imagePlugin.Info.SectorSize,
            Files                 = bb.files,
            FilesSpecified        = true,
            FreeClusters          = imagePlugin.Info.Sectors - bb.usedSectors,
            FreeClustersSpecified = true,
            VolumeName            = StringHandlers.SpacePaddedToString(bb.volumeLabel, Encoding),
            Bootable              = true
        };

        sbInformation.AppendFormat("{0} files on volume", bb.files).AppendLine();
        sbInformation.AppendFormat("{0} used sectors on volume", bb.usedSectors).AppendLine();

        sbInformation.AppendFormat("Disk name: {0}", StringHandlers.CToString(bb.volumeLabel, Encoding)).AppendLine();

        information = sbInformation.ToString();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct BootBlock
    {
        /// <summary>A NOP opcode</summary>
        public readonly byte nop;
        /// <summary>A branch to real bootloader</summary>
        public readonly ushort branch;
        /// <summary>Unused</summary>
        public readonly byte unused;
        /// <summary>" AO-DOS "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] identifier;
        /// <summary>Volume label</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] volumeLabel;
        /// <summary>How many files are present in disk</summary>
        public readonly ushort files;
        /// <summary>How many sectors are used</summary>
        public readonly ushort usedSectors;
    }
}