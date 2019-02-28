// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using Marshal = DiscImageChef.Helpers.Marshal;

namespace DiscImageChef.Partitions
{
    public class DragonFlyBSD : IPartition
    {
        const uint DISK_MAGIC64 = 0xC4464C59;

        public string Name   => "DragonFly BSD 64-bit disklabel";
        public Guid   Id     => new Guid("D49E41A6-D952-4760-9D94-03DAE2450C5F");
        public string Author => "Natalia Portillo";

        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();
            uint nSectors = 2048 / imagePlugin.Info.SectorSize;
            if(2048 % imagePlugin.Info.SectorSize > 0) nSectors++;

            if(sectorOffset + nSectors >= imagePlugin.Info.Sectors) return false;

            byte[] sectors = imagePlugin.ReadSectors(sectorOffset, nSectors);
            if(sectors.Length < 2048) return false;

            Disklabel64 disklabel = Marshal.ByteArrayToStructureLittleEndian<Disklabel64>(sectors);

            if(disklabel.d_magic != 0xC4464C59) return false;

            ulong counter = 0;

            foreach(Partition64 entry in disklabel.d_partitions)
            {
                Partition part = new Partition
                {
                    Start = entry.p_boffset / imagePlugin.Info.SectorSize + sectorOffset,
                    Offset = entry.p_boffset +
                               sectorOffset * imagePlugin.Info.SectorSize,
                    Size     = entry.p_bsize,
                    Length   = entry.p_bsize / imagePlugin.Info.SectorSize,
                    Name     = entry.p_stor_uuid.ToString(),
                    Sequence = counter,
                    Scheme   = Name,
                    Type = (BSD.fsType)entry.p_fstype == BSD.fsType.Other
                               ? entry.p_type_uuid.ToString()
                               : BSD.fsTypeToString((BSD.fsType)entry.p_fstype)
                };

                if(entry.p_bsize % imagePlugin.Info.SectorSize > 0) part.Length++;

                if(entry.p_bsize <= 0 || entry.p_boffset <= 0) continue;

                partitions.Add(part);
                counter++;
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Disklabel64
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] d_reserved0;
            public uint  d_magic;
            public uint  d_crc;
            public uint  d_align;
            public uint  d_npartitions;
            public Guid  d_stor_uuid;
            public ulong d_total_size;
            public ulong d_bbase;
            public ulong d_pbase;
            public ulong d_pstop;
            public ulong d_abase;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] d_packname;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] d_reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public Partition64[] d_partitions;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Partition64
        {
            public ulong p_boffset;
            public ulong p_bsize;
            public byte  p_fstype;
            public byte  p_unused01;
            public byte  p_unused02;
            public byte  p_unused03;
            public uint  p_unused04;
            public uint  p_unused05;
            public uint  p_unused06;
            public Guid  p_type_uuid;
            public Guid  p_stor_uuid;
        }
    }
}