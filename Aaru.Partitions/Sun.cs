// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Sun.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Sun disklabels.
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
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions
{
    /// <summary>
    /// Implements decoding of Sun disklabels
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class SunDisklabel : IPartition
    {
        /// <summary>Sun disklabel magic number</summary>
        const ushort DKL_MAGIC = 0xDABE;
        /// <summary>Sun VTOC magic number</summary>
        const uint VTOC_SANE = 0x600DDEEE;
        /// <summary>Sun disklabel magic number, byte-swapped</summary>
        const ushort DKL_CIGAM = 0xBEDA;
        /// <summary>Sun VTOC magic number, byte-swapped</summary>
        const uint VTOC_ENAS = 0xEEDE0D60;
        /// <summary># of logical partitions</summary>
        const int NDKMAP = 8;
        /// <summary># of logical partitions</summary>
        const int NDKMAP16 = 16;
        /// <summary>Disk label size</summary>
        const int DK_LABEL_SIZE = 512;
        /// <summary>Volume label size</summary>
        const int LEN_DKL_ASCII = 128;
        /// <summary>Length of v_volume</summary>
        const int LEN_DKL_VVOL = 8;
        /// <summary>Size of padding in SunOS disk label</summary>
        const int LEN_DKL_PAD = DK_LABEL_SIZE - (LEN_DKL_ASCII + (NDKMAP * 8) + (14 * 2));
        /// <summary>Size of padding in Solaris disk label with 8 partitions</summary>
        const int LEN_DKL_PAD8 = DK_LABEL_SIZE - (LEN_DKL_ASCII + 136      + // sizeof(dk_vtoc8)
                                                  (NDKMAP * 8)  + (14 * 2) + (2 * 2));
        const int LEN_DKL_PAD16 = DK_LABEL_SIZE - (456     + // sizeof(dk_vtoc16)
                                                   (4 * 4) + (12 * 2) + (2 * 2));

        /// <inheritdoc />
        public string Name   => "Sun Disklabel";
        /// <inheritdoc />
        public Guid   Id     => new Guid("50F35CC4-8375-4445-8DCB-1BA550C931A3");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            if(imagePlugin.Info.SectorSize < 512)
                return false;

            if(sectorOffset + 2 >= imagePlugin.Info.Sectors)
                return false;

            bool useDkl = false, useDkl8 = false, useDkl16 = false;

            byte[] sunSector = imagePlugin.ReadSector(sectorOffset);

            dk_label   dkl   = Marshal.ByteArrayToStructureLittleEndian<dk_label>(sunSector);
            dk_label8  dkl8  = Marshal.ByteArrayToStructureLittleEndian<dk_label8>(sunSector);
            dk_label16 dkl16 = Marshal.ByteArrayToStructureLittleEndian<dk_label16>(sunSector);

            AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_magic = 0x{0:X4}", dkl.dkl_magic);
            AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_vtoc.v_sanity = 0x{0:X8}", dkl8.dkl_vtoc.v_sanity);
            AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_vtoc.v_sanity = 0x{0:X8}", dkl16.dkl_vtoc.v_sanity);

            if(dkl.dkl_magic == DKL_MAGIC ||
               dkl.dkl_magic == DKL_CIGAM)
                if(dkl16.dkl_vtoc.v_sanity == VTOC_SANE ||
                   dkl16.dkl_vtoc.v_sanity == VTOC_ENAS)
                    useDkl16 = true;
                else if(dkl8.dkl_vtoc.v_sanity == VTOC_SANE ||
                        dkl8.dkl_vtoc.v_sanity == VTOC_ENAS)
                    useDkl8 = true;
                else
                    useDkl = true;

            if(!useDkl  &&
               !useDkl8 &&
               !useDkl16)
            {
                sunSector = imagePlugin.ReadSector(sectorOffset + 1);

                dkl   = Marshal.ByteArrayToStructureLittleEndian<dk_label>(sunSector);
                dkl8  = Marshal.ByteArrayToStructureLittleEndian<dk_label8>(sunSector);
                dkl16 = Marshal.ByteArrayToStructureLittleEndian<dk_label16>(sunSector);

                if(dkl.dkl_magic == DKL_MAGIC ||
                   dkl.dkl_magic == DKL_CIGAM)
                    if(dkl16.dkl_vtoc.v_sanity == VTOC_SANE ||
                       dkl16.dkl_vtoc.v_sanity == VTOC_ENAS)
                        useDkl16 = true;
                    else if(dkl8.dkl_vtoc.v_sanity == VTOC_SANE ||
                            dkl8.dkl_vtoc.v_sanity == VTOC_ENAS)
                        useDkl8 = true;
                    else
                        useDkl = true;
            }

            if(!useDkl  &&
               !useDkl8 &&
               !useDkl16)
                return false;

            if(useDkl16 && dkl16.dkl_magic == DKL_CIGAM)
                dkl16 = SwapDiskLabel(dkl16);
            else if(useDkl8 && dkl8.dkl_magic == DKL_CIGAM)
                dkl8 = SwapDiskLabel(dkl8);
            else if(useDkl && dkl.dkl_magic == DKL_CIGAM)
                dkl = SwapDiskLabel(dkl);

            if(useDkl)
            {
                ulong sectorsPerCylinder = (ulong)(dkl.dkl_nsect * dkl.dkl_nhead);

                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_asciilabel = \"{0}\"",
                                           StringHandlers.CToString(dkl.dkl_asciilabel));

                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_rpm = {0}", dkl.dkl_rpm);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_pcyl = {0}", dkl.dkl_pcyl);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_apc = {0}", dkl.dkl_apc);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_gap1 = {0}", dkl.dkl_gap1);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_gap2 = {0}", dkl.dkl_gap2);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_intrlv = {0}", dkl.dkl_intrlv);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_ncyl = {0}", dkl.dkl_ncyl);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_acyl = {0}", dkl.dkl_acyl);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_nhead = {0}", dkl.dkl_nhead);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_nsect = {0}", dkl.dkl_nsect);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_bhead = {0}", dkl.dkl_bhead);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_ppart = {0}", dkl.dkl_ppart);

                for(int i = 0; i < NDKMAP; i++)
                {
                    AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_map[{0}].dkl_cylno = {1}", i,
                                               dkl.dkl_map[i].dkl_cylno);

                    AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_map[{0}].dkl_nblk = {1}", i,
                                               dkl.dkl_map[i].dkl_nblk);
                }

                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_magic = 0x{0:X4}", dkl.dkl_magic);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl.dkl_cksum = 0x{0:X4}", dkl.dkl_cksum);
                AaruConsole.DebugWriteLine("Sun plugin", "sectorsPerCylinder = {0}", sectorsPerCylinder);

                for(int i = 0; i < NDKMAP; i++)
                    if(dkl.dkl_map[i].dkl_cylno > 0 &&
                       dkl.dkl_map[i].dkl_nblk  > 0)
                    {
                        var part = new Partition
                        {
                            Size     = (ulong)dkl.dkl_map[i].dkl_nblk * DK_LABEL_SIZE,
                            Length   = (ulong)((dkl.dkl_map[i].dkl_nblk * DK_LABEL_SIZE) / imagePlugin.Info.SectorSize),
                            Sequence = (ulong)i,
                            Offset = (((ulong)dkl.dkl_map[i].dkl_cylno * sectorsPerCylinder) + sectorOffset) *
                                     DK_LABEL_SIZE,
                            Start = ((((ulong)dkl.dkl_map[i].dkl_cylno * sectorsPerCylinder) + sectorOffset) *
                                     DK_LABEL_SIZE) / imagePlugin.Info.SectorSize,
                            Type   = "SunOS partition",
                            Scheme = Name
                        };

                        if(part.Start < imagePlugin.Info.Sectors &&
                           part.End   <= imagePlugin.Info.Sectors)
                            partitions.Add(part);
                    }
            }
            else if(useDkl8)
            {
                ulong sectorsPerCylinder = (ulong)(dkl8.dkl_nsect * dkl8.dkl_nhead);

                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_asciilabel = \"{0}\"",
                                           StringHandlers.CToString(dkl8.dkl_asciilabel));

                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_vtoc.v_version = {0}", dkl8.dkl_vtoc.v_version);

                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_vtoc.v_volume = \"{0}\"",
                                           StringHandlers.CToString(dkl8.dkl_vtoc.v_volume));

                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_vtoc.v_nparts = {0}", dkl8.dkl_vtoc.v_nparts);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_vtoc.v_sanity = 0x{0:X8}", dkl8.dkl_vtoc.v_sanity);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_write_reinstruct = {0}", dkl8.dkl_write_reinstruct);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_read_reinstruct = {0}", dkl8.dkl_read_reinstruct);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_rpm = {0}", dkl8.dkl_rpm);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_pcyl = {0}", dkl8.dkl_pcyl);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_apc = {0}", dkl8.dkl_apc);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_obs1 = {0}", dkl8.dkl_obs1);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_obs2 = {0}", dkl8.dkl_obs2);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_intrlv = {0}", dkl8.dkl_intrlv);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_ncyl = {0}", dkl8.dkl_ncyl);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_acyl = {0}", dkl8.dkl_acyl);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_nhead = {0}", dkl8.dkl_nhead);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_nsect = {0}", dkl8.dkl_nsect);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_obs3 = {0}", dkl8.dkl_obs3);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_obs4 = {0}", dkl8.dkl_obs4);

                for(int i = 0; i < NDKMAP; i++)
                {
                    AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_map[{0}].dkl_cylno = {1}", i,
                                               dkl8.dkl_map[i].dkl_cylno);

                    AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_map[{0}].dkl_nblk = {1}", i,
                                               dkl8.dkl_map[i].dkl_nblk);

                    AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_vtoc.v_part[{0}].p_tag = {1} ({2})", i,
                                               dkl8.dkl_vtoc.v_part[i].p_tag, (ushort)dkl8.dkl_vtoc.v_part[i].p_tag);

                    AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_vtoc.v_part[{0}].p_flag = {1} ({2})", i,
                                               dkl8.dkl_vtoc.v_part[i].p_flag, (ushort)dkl8.dkl_vtoc.v_part[i].p_flag);

                    AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_vtoc.v_timestamp[{0}] = {1}", i,
                                               DateHandlers.UnixToDateTime(dkl8.dkl_vtoc.v_timestamp[i]));
                }

                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_magic = 0x{0:X4}", dkl8.dkl_magic);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl8.dkl_cksum = 0x{0:X4}", dkl8.dkl_cksum);
                AaruConsole.DebugWriteLine("Sun plugin", "sectorsPerCylinder = {0}", sectorsPerCylinder);

                if(dkl8.dkl_vtoc.v_nparts > NDKMAP)
                    return false;

                for(int i = 0; i < dkl8.dkl_vtoc.v_nparts; i++)
                    if(dkl8.dkl_map[i].dkl_nblk      > 0                &&
                       dkl8.dkl_vtoc.v_part[i].p_tag != SunTag.SunEmpty &&
                       dkl8.dkl_vtoc.v_part[i].p_tag != SunTag.SunWholeDisk)
                    {
                        var part = new Partition
                        {
                            Description = SunFlagsToString(dkl8.dkl_vtoc.v_part[i].p_flag),
                            Size = (ulong)dkl8.dkl_map[i].dkl_nblk * DK_LABEL_SIZE,
                            Length = (ulong)((dkl8.dkl_map[i].dkl_nblk * DK_LABEL_SIZE) / imagePlugin.Info.SectorSize),
                            Sequence = (ulong)i,
                            Offset = (((ulong)dkl8.dkl_map[i].dkl_cylno * sectorsPerCylinder) + sectorOffset) *
                                     DK_LABEL_SIZE,
                            Start = ((((ulong)dkl8.dkl_map[i].dkl_cylno * sectorsPerCylinder) + sectorOffset) *
                                     DK_LABEL_SIZE) / imagePlugin.Info.SectorSize,
                            Type   = SunIdToString(dkl8.dkl_vtoc.v_part[i].p_tag),
                            Scheme = Name
                        };

                        if(dkl8.dkl_vtoc.v_timestamp[i] != 0)
                            part.Description +=
                                $"\nPartition timestamped on {DateHandlers.UnixToDateTime(dkl8.dkl_vtoc.v_timestamp[i])}";

                        if(part.Start < imagePlugin.Info.Sectors &&
                           part.End   <= imagePlugin.Info.Sectors)
                            partitions.Add(part);
                    }
            }
            else
            {
                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_vtoc.v_sanity = 0x{0:X8}", dkl16.dkl_vtoc.v_sanity);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_vtoc.v_version = {0}", dkl16.dkl_vtoc.v_version);

                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_vtoc.v_volume = \"{0}\"",
                                           StringHandlers.CToString(dkl16.dkl_vtoc.v_volume));

                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_vtoc.v_sectorsz = {0}", dkl16.dkl_vtoc.v_sectorsz);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_vtoc.v_nparts = {0}", dkl16.dkl_vtoc.v_nparts);

                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_vtoc.v_asciilabel = \"{0}\"",
                                           StringHandlers.CToString(dkl16.dkl_vtoc.v_asciilabel));

                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_pcyl = {0}", dkl16.dkl_pcyl);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_ncyl = {0}", dkl16.dkl_ncyl);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_acyl = {0}", dkl16.dkl_acyl);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_bcyl = {0}", dkl16.dkl_bcyl);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_nhead = {0}", dkl16.dkl_nhead);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_nsect = {0}", dkl16.dkl_nsect);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_intrlv = {0}", dkl16.dkl_intrlv);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_skew = {0}", dkl16.dkl_skew);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_apc = {0}", dkl16.dkl_apc);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_rpm = {0}", dkl16.dkl_rpm);

                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_write_reinstruct = {0}",
                                           dkl16.dkl_write_reinstruct);

                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_read_reinstruct = {0}", dkl16.dkl_read_reinstruct);

                for(int i = 0; i < NDKMAP16; i++)
                {
                    AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_vtoc.v_part[{0}].p_start = {1}", i,
                                               dkl16.dkl_vtoc.v_part[i].p_start);

                    AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_vtoc.v_part[{0}].p_size = {1}", i,
                                               dkl16.dkl_vtoc.v_part[i].p_size);

                    AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_vtoc.v_part[{0}].p_tag = {1} ({2})", i,
                                               dkl16.dkl_vtoc.v_part[i].p_tag, (ushort)dkl16.dkl_vtoc.v_part[i].p_tag);

                    AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_vtoc.v_part[{0}].p_flag = {1} ({2})", i,
                                               dkl16.dkl_vtoc.v_part[i].p_flag,
                                               (ushort)dkl16.dkl_vtoc.v_part[i].p_flag);

                    AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_vtoc.v_timestamp[{0}] = {1}", i,
                                               DateHandlers.UnixToDateTime(dkl16.dkl_vtoc.v_timestamp[i]));
                }

                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_magic = 0x{0:X4}", dkl16.dkl_magic);
                AaruConsole.DebugWriteLine("Sun plugin", "dkl16.dkl_cksum = 0x{0:X4}", dkl16.dkl_cksum);

                if(dkl16.dkl_vtoc.v_nparts > NDKMAP16)
                    return false;

                for(int i = 0; i < dkl16.dkl_vtoc.v_nparts; i++)
                    if(dkl16.dkl_vtoc.v_part[i].p_size > 0                &&
                       dkl16.dkl_vtoc.v_part[i].p_tag  != SunTag.SunEmpty &&
                       dkl16.dkl_vtoc.v_part[i].p_tag  != SunTag.SunWholeDisk)
                    {
                        var part = new Partition
                        {
                            Description = SunFlagsToString(dkl16.dkl_vtoc.v_part[i].p_flag),
                            Size        = (ulong)dkl16.dkl_vtoc.v_part[i].p_size * dkl16.dkl_vtoc.v_sectorsz,
                            Length = (ulong)((dkl16.dkl_vtoc.v_part[i].p_size * dkl16.dkl_vtoc.v_sectorsz) /
                                             imagePlugin.Info.SectorSize),
                            Sequence = (ulong)i,
                            Offset = ((ulong)dkl16.dkl_vtoc.v_part[i].p_start + sectorOffset) *
                                     dkl16.dkl_vtoc.v_sectorsz,
                            Start = (((ulong)dkl16.dkl_vtoc.v_part[i].p_start + sectorOffset) *
                                     dkl16.dkl_vtoc.v_sectorsz) / imagePlugin.Info.SectorSize,
                            Type   = SunIdToString(dkl16.dkl_vtoc.v_part[i].p_tag),
                            Scheme = Name
                        };

                        if(dkl16.dkl_vtoc.v_timestamp[i] != 0)
                            part.Description +=
                                $"\nPartition timestamped on {DateHandlers.UnixToDateTime(dkl16.dkl_vtoc.v_timestamp[i])}";

                        if(part.Start < imagePlugin.Info.Sectors &&
                           part.End   <= imagePlugin.Info.Sectors)
                            partitions.Add(part);
                    }
            }

            return partitions.Count > 0;
        }

        static dk_label SwapDiskLabel(dk_label label)
        {
            AaruConsole.DebugWriteLine("Sun plugin", "Swapping dk_label");
            label = (dk_label)Marshal.SwapStructureMembersEndian(label);

            for(int i = 0; i < label.dkl_map.Length; i++)
                label.dkl_map[i] = (dk_map)Marshal.SwapStructureMembersEndian(label.dkl_map[i]);

            return label;
        }

        static dk_label8 SwapDiskLabel(dk_label8 label)
        {
            AaruConsole.DebugWriteLine("Sun plugin", "Swapping dk_label8");
            label = (dk_label8)Marshal.SwapStructureMembersEndian(label);

            for(int i = 0; i < label.dkl_map.Length; i++)
                label.dkl_map[i] = (dk_map)Marshal.SwapStructureMembersEndian(label.dkl_map[i]);

            for(int i = 0; i < label.dkl_vtoc.v_bootinfo.Length; i++)
                label.dkl_vtoc.v_bootinfo[i] = Swapping.Swap(label.dkl_vtoc.v_bootinfo[i]);

            for(int i = 0; i < label.dkl_vtoc.v_part.Length; i++)
            {
                label.dkl_vtoc.v_part[i].p_flag = (SunFlags)Swapping.Swap((ushort)label.dkl_vtoc.v_part[i].p_flag);
                label.dkl_vtoc.v_part[i].p_tag  = (SunTag)Swapping.Swap((ushort)label.dkl_vtoc.v_part[i].p_tag);
            }

            for(int i = 0; i < label.dkl_vtoc.v_timestamp.Length; i++)
                label.dkl_vtoc.v_timestamp[i] = Swapping.Swap(label.dkl_vtoc.v_timestamp[i]);

            for(int i = 0; i < label.dkl_vtoc.v_reserved.Length; i++)
                label.dkl_vtoc.v_reserved[i] = Swapping.Swap(label.dkl_vtoc.v_reserved[i]);

            return label;
        }

        static dk_label16 SwapDiskLabel(dk_label16 label)
        {
            AaruConsole.DebugWriteLine("Sun plugin", "Swapping dk_label16");
            label = (dk_label16)Marshal.SwapStructureMembersEndian(label);

            for(int i = 0; i < label.dkl_vtoc.v_bootinfo.Length; i++)
                label.dkl_vtoc.v_bootinfo[i] = Swapping.Swap(label.dkl_vtoc.v_bootinfo[i]);

            for(int i = 0; i < label.dkl_vtoc.v_part.Length; i++)
            {
                label.dkl_vtoc.v_part[i].p_flag  = (SunFlags)Swapping.Swap((ushort)label.dkl_vtoc.v_part[i].p_flag);
                label.dkl_vtoc.v_part[i].p_tag   = (SunTag)Swapping.Swap((ushort)label.dkl_vtoc.v_part[i].p_tag);
                label.dkl_vtoc.v_part[i].p_size  = Swapping.Swap(label.dkl_vtoc.v_part[i].p_size);
                label.dkl_vtoc.v_part[i].p_start = Swapping.Swap(label.dkl_vtoc.v_part[i].p_start);
            }

            for(int i = 0; i < label.dkl_vtoc.v_timestamp.Length; i++)
                label.dkl_vtoc.v_timestamp[i] = Swapping.Swap(label.dkl_vtoc.v_timestamp[i]);

            for(int i = 0; i < label.dkl_vtoc.v_reserved.Length; i++)
                label.dkl_vtoc.v_reserved[i] = Swapping.Swap(label.dkl_vtoc.v_reserved[i]);

            return label;
        }

        static string SunFlagsToString(SunFlags flags)
        {
            var sb = new StringBuilder();

            if(flags.HasFlag(SunFlags.NoMount))
                sb.AppendLine("Unmountable");

            if(flags.HasFlag(SunFlags.ReadOnly))
                sb.AppendLine("Read-only");

            return sb.ToString();
        }

        static string SunIdToString(SunTag id)
        {
            switch(id)
            {
                case SunTag.Linux:          return "Linux";
                case SunTag.LinuxRaid:      return "Linux RAID";
                case SunTag.LinuxSwap:      return "Linux swap";
                case SunTag.LVM:            return "LVM";
                case SunTag.SunBoot:        return "Sun boot";
                case SunTag.SunEmpty:       return "Empty";
                case SunTag.SunHome:        return "Sun /home";
                case SunTag.SunRoot:        return "Sun /";
                case SunTag.SunStand:       return "Sun /stand";
                case SunTag.SunSwap:        return "Sun swap";
                case SunTag.SunUsr:         return "Sun /usr";
                case SunTag.SunVar:         return "Sun /var";
                case SunTag.SunWholeDisk:   return "Whole disk";
                case SunTag.SunAlt:         return "Replacement sectors";
                case SunTag.SunCache:       return "Sun cachefs";
                case SunTag.SunReserved:    return "Reserved for SMI";
                case SunTag.VxVmPublic:     return "Veritas public";
                case SunTag.VxVmPrivate:    return "Veritas private";
                case SunTag.NetBSD:         return "NetBSD";
                case SunTag.FreeBSD_Swap:   return "FreeBSD swap";
                case SunTag.FreeBSD_UFS:    return "FreeBSD";
                case SunTag.FreeBSD_Vinum:  return "Vinum";
                case SunTag.FreeBSD_ZFS:    return "FreeBSD ZFS";
                case SunTag.FreeBSD_NANDFS: return "FreeBSD nandfs";
                default:                    return "Unknown";
            }
        }

        enum SunTag : ushort
        {
            SunEmpty      = 0x0000, SunBoot      = 0x0001, SunRoot        = 0x0002,
            SunSwap       = 0x0003, SunUsr       = 0x0004, SunWholeDisk   = 0x0005,
            SunStand      = 0x0006, SunVar       = 0x0007, SunHome        = 0x0008,
            SunAlt        = 0x0009, SunCache     = 0x000A, SunReserved    = 0x000B,
            VxVmPublic    = 0x000E, VxVmPrivate  = 0x000F, LinuxSwap      = 0x0082,
            Linux         = 0x0083, LVM          = 0x008E, LinuxRaid      = 0x00FD,
            NetBSD        = 0x00FF, FreeBSD_Swap = 0x0901, FreeBSD_UFS    = 0x0902,
            FreeBSD_Vinum = 0x0903, FreeBSD_ZFS  = 0x0904, FreeBSD_NANDFS = 0x0905
        }

        [Flags]
        enum SunFlags : ushort
        {
            NoMount = 0x0001, ReadOnly = 0x0010
        }

        /// <summary>SunOS logical partitions</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct dk_map
        {
            /// <summary>starting cylinder</summary>
            public readonly int dkl_cylno;
            /// <summary>number of blocks</summary>
            public readonly int dkl_nblk;
        }

        /// <summary>SunOS disk label</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct dk_label
        {
            /// <summary>Informative string</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LEN_DKL_ASCII)]
            public readonly byte[] dkl_asciilabel;
            /// <summary>Padding</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LEN_DKL_PAD)]
            public readonly byte[] dkl_pad;
            /// <summary>rotations per minute</summary>
            public readonly ushort dkl_rpm;
            /// <summary># physical cylinders</summary>
            public readonly ushort dkl_pcyl;
            /// <summary>alternates per cylinder</summary>
            public readonly ushort dkl_apc;
            /// <summary>size of gap 1</summary>
            public readonly ushort dkl_gap1;
            /// <summary>size of gap 2</summary>
            public readonly ushort dkl_gap2;
            /// <summary>interleave factor</summary>
            public readonly ushort dkl_intrlv;
            /// <summary># of data cylinders</summary>
            public readonly ushort dkl_ncyl;
            /// <summary># of alternate cylinders</summary>
            public readonly ushort dkl_acyl;
            /// <summary># of heads in this partition</summary>
            public readonly ushort dkl_nhead;
            /// <summary># of 512 byte sectors per track</summary>
            public readonly ushort dkl_nsect;
            /// <summary>identifies proper label location</summary>
            public readonly ushort dkl_bhead;
            /// <summary>physical partition #</summary>
            public readonly ushort dkl_ppart;
            /// <summary>Logical partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NDKMAP)]
            public readonly dk_map[] dkl_map;
            /// <summary>identifies this label format</summary>
            public readonly ushort dkl_magic;
            /// <summary>xor checksum of sector</summary>
            public readonly ushort dkl_cksum;
        }

        /// <summary>Solaris logical partition for small disk label</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct dk_map2
        {
            /// <summary> ID tag of partition</summary>
            public SunTag p_tag;
            /// <summary> permission flag</summary>
            public SunFlags p_flag;
        }

        /// <summary>Solaris logical partition</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct dkl_partition
        {
            /// <summary>ID tag of partition</summary>
            public SunTag p_tag;
            /// <summary>permision flags</summary>
            public SunFlags p_flag;
            /// <summary>start sector no of partition</summary>
            public int p_start;
            /// <summary># of blocks in partition</summary>
            public int p_size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct dk_vtoc8
        {
            /// <summary> layout version</summary>
            public readonly uint v_version;
            /// <summary> volume name</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LEN_DKL_VVOL)]
            public readonly byte[] v_volume;
            /// <summary> number of partitions </summary>
            public readonly ushort v_nparts;
            /// <summary> partition hdrs, sec 2</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NDKMAP)]
            public readonly dk_map2[] v_part;
            /// <summary>Alignment</summary>
            public readonly ushort padding;
            /// <summary> info needed by mboot</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly uint[] v_bootinfo;
            /// <summary> to verify vtoc sanity</summary>
            public readonly uint v_sanity;
            /// <summary> free space</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public readonly uint[] v_reserved;
            /// <summary> partition timestamp</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NDKMAP)]
            public readonly int[] v_timestamp;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct dk_vtoc16
        {
            /// <summary>info needed by mboot</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly uint[] v_bootinfo;
            /// <summary>to verify vtoc sanity</summary>
            public readonly uint v_sanity;
            /// <summary>layout version</summary>
            public readonly uint v_version;
            /// <summary>volume name</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LEN_DKL_VVOL)]
            public readonly byte[] v_volume;
            /// <summary>sector size in bytes</summary>
            public readonly ushort v_sectorsz;
            /// <summary>number of partitions</summary>
            public readonly ushort v_nparts;
            /// <summary>free space</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public readonly uint[] v_reserved;
            /// <summary>partition headers</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NDKMAP16)]
            public readonly dkl_partition[] v_part;
            /// <summary>partition timestamp</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NDKMAP16)]
            public readonly int[] v_timestamp;
            /// <summary>for compatibility</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LEN_DKL_ASCII)]
            public readonly byte[] v_asciilabel;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct dk_label8
        {
            /// <summary>for compatibility</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LEN_DKL_ASCII)]
            public readonly byte[] dkl_asciilabel;
            /// <summary>vtoc inclusions from AT&amp;T SVr4</summary>
            public readonly dk_vtoc8 dkl_vtoc;
            /// <summary># sectors to skip, writes</summary>
            public readonly ushort dkl_write_reinstruct;
            /// <summary># sectors to skip, reads</summary>
            public readonly ushort dkl_read_reinstruct;
            /// <summary>unused part of 512 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LEN_DKL_PAD8)]
            public readonly byte[] dkl_pad;
            /// <summary>rotations per minute</summary>
            public readonly ushort dkl_rpm;
            /// <summary># physical cylinders</summary>
            public readonly ushort dkl_pcyl;
            /// <summary>alternates per cylinder</summary>
            public readonly ushort dkl_apc;
            /// <summary>obsolete</summary>
            public readonly ushort dkl_obs1;
            /// <summary>obsolete</summary>
            public readonly ushort dkl_obs2;
            /// <summary>interleave factor</summary>
            public readonly ushort dkl_intrlv;
            /// <summary># of data cylinders</summary>
            public readonly ushort dkl_ncyl;
            /// <summary># of alternate cylinders</summary>
            public readonly ushort dkl_acyl;
            /// <summary># of heads in this partition</summary>
            public readonly ushort dkl_nhead;
            /// <summary># of 512 byte sectors per track</summary>
            public readonly ushort dkl_nsect;
            /// <summary>obsolete</summary>
            public readonly ushort dkl_obs3;
            /// <summary>obsolete</summary>
            public readonly ushort dkl_obs4;
            /// <summary>logical partition headers</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NDKMAP)]
            public readonly dk_map[] dkl_map;
            /// <summary>identifies this label format</summary>
            public readonly ushort dkl_magic;
            /// <summary>xor checksum of sector</summary>
            public readonly ushort dkl_cksum;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct dk_label16
        {
            /// <summary>vtoc inclusions from AT&amp;T SVr4</summary>
            public readonly dk_vtoc16 dkl_vtoc;
            /// <summary># of physical cylinders</summary>
            public readonly uint dkl_pcyl;
            /// <summary># of data cylinders</summary>
            public readonly uint dkl_ncyl;
            /// <summary># of alternate cylinders</summary>
            public readonly ushort dkl_acyl;
            /// <summary>cyl offset (for fixed head area)</summary>
            public readonly ushort dkl_bcyl;
            /// <summary># of heads</summary>
            public readonly uint dkl_nhead;
            /// <summary># of data sectors per track</summary>
            public readonly uint dkl_nsect;
            /// <summary>interleave factor</summary>
            public readonly ushort dkl_intrlv;
            /// <summary>skew factor</summary>
            public readonly ushort dkl_skew;
            /// <summary>alternates per cyl (SCSI only)  </summary>
            public readonly ushort dkl_apc;
            /// <summary>revolutions per minute</summary>
            public readonly ushort dkl_rpm;
            /// <summary># sectors to skip, writes</summary>
            public readonly ushort dkl_write_reinstruct;
            /// <summary># sectors to skip, reads </summary>
            public readonly ushort dkl_read_reinstruct;
            /// <summary>for compatible expansion</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly ushort[] dkl_extra;
            /// <summary>unused part of 512 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LEN_DKL_PAD16)]
            public readonly byte[] dkl_pad;
            /// <summary>identifies this label format</summary>
            public readonly ushort dkl_magic;
            /// <summary>xor checksum of sector</summary>
            public readonly ushort dkl_cksum;
        }
    }
}