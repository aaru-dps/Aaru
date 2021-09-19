// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DragonFlyBSD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages DragonFly BSD 64-bit disklabels.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions
{
    /// <inheritdoc />
    /// <summary>Implements decoding of DragonFly BSD disklabels</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public sealed class DragonFlyBSD : IPartition
    {
        const uint DISK_MAGIC64 = 0xC4464C59;

        /// <inheritdoc />
        public string Name => "DragonFly BSD 64-bit disklabel";
        /// <inheritdoc />
        public Guid Id => new("D49E41A6-D952-4760-9D94-03DAE2450C5F");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();
            uint nSectors = 2048 / imagePlugin.Info.SectorSize;

            if(2048 % imagePlugin.Info.SectorSize > 0)
                nSectors++;

            if(sectorOffset + nSectors >= imagePlugin.Info.Sectors)
                return false;

            ErrorNumber errno = imagePlugin.ReadSectors(sectorOffset, nSectors, out byte[] sectors);

            if(errno          != ErrorNumber.NoError ||
               sectors.Length < 2048)
                return false;

            Disklabel64 disklabel = Marshal.ByteArrayToStructureLittleEndian<Disklabel64>(sectors);

            if(disklabel.d_magic != 0xC4464C59)
                return false;

            ulong counter = 0;

            foreach(Partition64 entry in disklabel.d_partitions)
            {
                var part = new Partition
                {
                    Start    = (entry.p_boffset / imagePlugin.Info.SectorSize) + sectorOffset,
                    Offset   = entry.p_boffset + (sectorOffset * imagePlugin.Info.SectorSize),
                    Size     = entry.p_bsize,
                    Length   = entry.p_bsize / imagePlugin.Info.SectorSize,
                    Name     = entry.p_stor_uuid.ToString(),
                    Sequence = counter,
                    Scheme   = Name,
                    Type = (BSD.fsType)entry.p_fstype == BSD.fsType.Other ? entry.p_type_uuid.ToString()
                               : BSD.FSTypeToString((BSD.fsType)entry.p_fstype)
                };

                if(entry.p_bsize % imagePlugin.Info.SectorSize > 0)
                    part.Length++;

                if(entry.p_bsize   <= 0 ||
                   entry.p_boffset <= 0)
                    continue;

                partitions.Add(part);
                counter++;
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Disklabel64
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public readonly byte[] d_reserved0;
            public readonly uint  d_magic;
            public readonly uint  d_crc;
            public readonly uint  d_align;
            public readonly uint  d_npartitions;
            public readonly Guid  d_stor_uuid;
            public readonly ulong d_total_size;
            public readonly ulong d_bbase;
            public readonly ulong d_pbase;
            public readonly ulong d_pstop;
            public readonly ulong d_abase;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public readonly byte[] d_packname;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public readonly byte[] d_reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly Partition64[] d_partitions;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Partition64
        {
            public readonly ulong p_boffset;
            public readonly ulong p_bsize;
            public readonly byte  p_fstype;
            public readonly byte  p_unused01;
            public readonly byte  p_unused02;
            public readonly byte  p_unused03;
            public readonly uint  p_unused04;
            public readonly uint  p_unused05;
            public readonly uint  p_unused06;
            public readonly Guid  p_type_uuid;
            public readonly Guid  p_stor_uuid;
        }
    }
}