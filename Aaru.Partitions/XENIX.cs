// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : XENIX.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages XENIX partitions.
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
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions
{
    // TODO: Find better documentation, this is working for XENIX 2 but not for SCO OpenServer...
    /// <inheritdoc />
    /// <summary>Implements decoding of XENIX partitions</summary>
    public sealed class XENIX : IPartition
    {
        const ushort PAMAGIC     = 0x1234;
        const int    MAXPARTS    = 16;
        const uint   XENIX_BSIZE = 1024;

        // Can't find this in any documentation but everything is aligned to this offset (in sectors)
        const uint XENIX_OFFSET = 977;

        /// <inheritdoc />
        public string Name => "XENIX";
        /// <inheritdoc />
        public Guid Id => new Guid("53BE01DE-E68B-469F-A17F-EC2E4BD61CD9");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool GetInformation(IMediaImage imagePlugin, out List<CommonTypes.Partition> partitions,
                                   ulong sectorOffset)
        {
            partitions = new List<CommonTypes.Partition>();

            if(42 + sectorOffset >= imagePlugin.Info.Sectors)
                return false;

            byte[] tblsector = imagePlugin.ReadSector(42 + sectorOffset);

            Partable xnxtbl = Marshal.ByteArrayToStructureLittleEndian<Partable>(tblsector);

            AaruConsole.DebugWriteLine("XENIX plugin", "xnxtbl.p_magic = 0x{0:X4} (should be 0x{1:X4})", xnxtbl.p_magic,
                                       PAMAGIC);

            if(xnxtbl.p_magic != PAMAGIC)
                return false;

            for(int i = 0; i < MAXPARTS; i++)
            {
                AaruConsole.DebugWriteLine("XENIX plugin", "xnxtbl.p[{0}].p_off = {1}", i, xnxtbl.p[i].p_off);
                AaruConsole.DebugWriteLine("XENIX plugin", "xnxtbl.p[{0}].p_size = {1}", i, xnxtbl.p[i].p_size);

                if(xnxtbl.p[i].p_size <= 0)
                    continue;

                var part = new CommonTypes.Partition
                {
                    Start = ((ulong)((xnxtbl.p[i].p_off + XENIX_OFFSET) * XENIX_BSIZE) / imagePlugin.Info.SectorSize) +
                            sectorOffset,
                    Length = (ulong)(xnxtbl.p[i].p_size * XENIX_BSIZE) / imagePlugin.Info.SectorSize,
                    Offset = (ulong)((xnxtbl.p[i].p_off + XENIX_OFFSET) * XENIX_BSIZE) +
                             (imagePlugin.Info.SectorSize * sectorOffset),
                    Size     = (ulong)(xnxtbl.p[i].p_size * XENIX_BSIZE),
                    Sequence = (ulong)i,
                    Type     = "XENIX",
                    Scheme   = Name
                };

                if(part.End < imagePlugin.Info.Sectors)
                    partitions.Add(part);
            }

            return partitions.Count > 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Partable
        {
            public readonly ushort p_magic; /* magic number validity indicator */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXPARTS)]
            public readonly Partition[] p; /*partition headers*/
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Partition
        {
            public readonly int p_off;  /*start 1K block no of partition*/
            public readonly int p_size; /*# of 1K blocks in partition*/
        }
    }
}