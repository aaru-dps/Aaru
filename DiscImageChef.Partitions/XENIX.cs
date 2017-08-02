// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.PartPlugins
{
    // TODO: Find better documentation, this is working for XENIX 2 but not for SCO OpenServer...
    public class XENIX : PartPlugin
    {
        const ushort PAMAGIC = 0x1234;
        const int MAXPARTS = 16;
        const uint XENIX_BSIZE = 1024;
        // Can't find this in any documentation but everything is aligned to this offset (in sectors)
        const uint XENIX_OFFSET = 977;

        public XENIX()
        {
            Name = "XENIX";
            PluginUUID = new Guid("53BE01DE-E68B-469F-A17F-EC2E4BD61CD9");
        }

        public override bool GetInformation(ImagePlugin imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            if(42 + sectorOffset >= imagePlugin.GetSectors())
                return false;

            byte[] tblsector = imagePlugin.ReadSector(42 + sectorOffset);

            GCHandle handle = GCHandle.Alloc(tblsector, GCHandleType.Pinned);
            partable xnxtbl = (partable)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(partable));
            handle.Free();

            DicConsole.DebugWriteLine("XENIX plugin", "xnxtbl.p_magic = 0x{0:X4} (should be 0x{1:X4})", xnxtbl.p_magic, PAMAGIC);

            if(xnxtbl.p_magic != PAMAGIC)
                return false;

            for(int i = 0; i < MAXPARTS; i++)
            {
                DicConsole.DebugWriteLine("XENIX plugin", "xnxtbl.p[{0}].p_off = {1}", i, xnxtbl.p[i].p_off);
                DicConsole.DebugWriteLine("XENIX plugin", "xnxtbl.p[{0}].p_size = {1}", i, xnxtbl.p[i].p_size);
                if(xnxtbl.p[i].p_size >0)
                {
                    Partition part = new Partition
                    {
                        Start = ((ulong)((xnxtbl.p[i].p_off + XENIX_OFFSET) * XENIX_BSIZE) / imagePlugin.GetSectorSize()) + sectorOffset,
                        Length = (ulong)(xnxtbl.p[i].p_size * XENIX_BSIZE) / imagePlugin.GetSectorSize(),
                        Offset = (ulong)((xnxtbl.p[i].p_off + XENIX_OFFSET) * XENIX_BSIZE) + (imagePlugin.GetSectorSize() * sectorOffset),
                        Size = (ulong)(xnxtbl.p[i].p_size * XENIX_BSIZE),
                        Sequence = (ulong)i,
                        Type = "XENIX",
                        Scheme = Name
                    };

                    if(part.End < imagePlugin.GetSectors())
                        partitions.Add(part);
                }
            }

            return partitions.Count > 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct partable
        {
            public ushort p_magic;     /* magic number validity indicator */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXPARTS)]
            public partition[] p;  /*partition headers*/
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct partition
        {
            public int p_off;        /*start 1K block no of partition*/
            public int p_size;            /*# of 1K blocks in partition*/
        }
    }
}