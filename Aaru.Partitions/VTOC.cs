// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VTOC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages UNIX VTOC and disklabels.
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
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions
{
    /// <inheritdoc />
    /// <summary>Implements decoding of UNIX VTOC partitions</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public sealed class VTOC : IPartition
    {
        const uint PD_MAGIC  = 0xCA5E600D;
        const uint VTOC_SANE = 0x600DDEEE;
        const uint PD_CIGAM  = 0x0D605ECA;
        const uint VTOC_ENAS = 0xEEDE0D60;
        const int  V_NUMPAR  = 16;
        const uint XPDVERS   = 3; /* 1st version of extended pdinfo */

        /// <inheritdoc />
        public string Name => "UNIX VTOC";
        /// <inheritdoc />
        public Guid Id => new Guid("6D35A66F-8D77-426F-A562-D88F6A1F1702");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            uint   magic      = 0;
            ulong  pdloc      = 0;
            byte[] pdsector   = null;
            bool   magicFound = false;
            bool   absolute   = false;

            foreach(ulong i in new ulong[]
            {
                0, 1, 8, 29
            }.TakeWhile(i => i + sectorOffset < imagePlugin.Info.Sectors))
            {
                pdsector = imagePlugin.ReadSector(i + sectorOffset);
                magic    = BitConverter.ToUInt32(pdsector, 4);

                AaruConsole.DebugWriteLine("VTOC plugin", "sanity at {0} is 0x{1:X8} (should be 0x{2:X8} or 0x{3:X8})",
                                           i + sectorOffset, magic, PD_MAGIC, PD_CIGAM);

                if(magic != PD_MAGIC &&
                   magic != PD_CIGAM)
                    continue;

                magicFound = true;
                pdloc      = i;

                break;
            }

            if(!magicFound)
                return false;

            PDInfo    pd;
            PDInfoOld pdold;

            if(magic == PD_MAGIC)
            {
                pd    = Marshal.ByteArrayToStructureLittleEndian<PDInfo>(pdsector);
                pdold = Marshal.ByteArrayToStructureLittleEndian<PDInfoOld>(pdsector);
            }
            else
            {
                pd    = Marshal.ByteArrayToStructureBigEndian<PDInfo>(pdsector);
                pdold = Marshal.ByteArrayToStructureBigEndian<PDInfoOld>(pdsector);
            }

            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.driveid = {0}", pd.driveid);

            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.sanity = 0x{0:X8} (should be 0x{1:X8})", pd.sanity,
                                       PD_MAGIC);

            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.version = {0}", pd.version);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.serial = \"{0}\"", StringHandlers.CToString(pd.serial));
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.cyls = {0}", pd.cyls);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.tracks = {0}", pd.tracks);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.sectors = {0}", pd.sectors);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.bytes = {0}", pd.bytes);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.logicalst = {0}", pd.logicalst);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.errlogst = {0}", pd.errlogst);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.errlogsz = {0}", pd.errlogsz);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.mfgst = {0}", pd.mfgst);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.mfgsz = {0}", pd.mfgsz);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.defectst = {0}", pd.defectst);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.defectsz = {0}", pd.defectsz);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.relno = {0}", pd.relno);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.relst = {0}", pd.relst);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.relsz = {0}", pd.relsz);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.relnext = {0}", pd.relnext);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.allcstrt = {0}", pdold.allcstrt);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.allcend = {0}", pdold.allcend);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.vtoc_ptr = {0}", pd.vtoc_ptr);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.vtoc_len = {0}", pd.vtoc_len);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.vtoc_pad = {0}", pd.vtoc_pad);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.alt_ptr = {0}", pd.alt_ptr);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.alt_len = {0}", pd.alt_len);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.pcyls = {0}", pd.pcyls);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.ptracks = {0}", pd.ptracks);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.psectors = {0}", pd.psectors);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.pbytes = {0}", pd.pbytes);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.secovhd = {0}", pd.secovhd);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.interleave = {0}", pd.interleave);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.skew = {0}", pd.skew);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.pad[0] = {0}", pd.pad[0]);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.pad[1] = {0}", pd.pad[1]);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.pad[2] = {0}", pd.pad[2]);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.pad[3] = {0}", pd.pad[3]);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.pad[4] = {0}", pd.pad[4]);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.pad[5] = {0}", pd.pad[5]);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.pad[6] = {0}", pd.pad[6]);
            AaruConsole.DebugWriteLine("VTOC plugin", "pdinfo.pad[7] = {0}", pd.pad[7]);

            magicFound = false;
            bool   useOld     = false;
            byte[] vtocsector = imagePlugin.ReadSector(pdloc + sectorOffset + 1);
            var    vtoc       = new vtoc();
            var    vtocOld    = new vtocold();
            magic = BitConverter.ToUInt32(vtocsector, 0);

            if(magic == VTOC_SANE ||
               magic == VTOC_ENAS)
            {
                magicFound = true;
                AaruConsole.DebugWriteLine("VTOC plugin", "New VTOC found at {0}", pdloc + sectorOffset + 1);

                if(magic == VTOC_SANE)
                    vtoc = Marshal.ByteArrayToStructureLittleEndian<vtoc>(vtocsector);
                else
                {
                    vtoc = Marshal.ByteArrayToStructureBigEndian<vtoc>(vtocsector);

                    for(int i = 0; i < vtoc.v_part.Length; i++)
                    {
                        vtoc.v_part[i].p_tag   = (pTag)Swapping.Swap((ushort)vtoc.v_part[i].p_tag);
                        vtoc.v_part[i].p_flag  = (pFlag)Swapping.Swap((ushort)vtoc.v_part[i].p_flag);
                        vtoc.v_part[i].p_start = Swapping.Swap(vtoc.v_part[i].p_start);
                        vtoc.v_part[i].p_size  = Swapping.Swap(vtoc.v_part[i].p_size);
                        vtoc.timestamp[i]      = Swapping.Swap(vtoc.timestamp[i]);
                    }
                }
            }

            if(!magicFound &&
               pd.version < XPDVERS)
            {
                magic = BitConverter.ToUInt32(vtocsector, 12);

                if(magic == VTOC_SANE ||
                   magic == VTOC_ENAS)
                {
                    magicFound = true;
                    useOld     = true;
                    AaruConsole.DebugWriteLine("VTOC plugin", "Old VTOC found at {0}", pdloc + sectorOffset + 1);

                    if(magic == VTOC_SANE)
                        vtocOld = Marshal.ByteArrayToStructureLittleEndian<vtocold>(vtocsector);
                    else
                    {
                        vtocOld = Marshal.ByteArrayToStructureBigEndian<vtocold>(vtocsector);

                        for(int i = 0; i < vtocOld.v_part.Length; i++)
                        {
                            vtocOld.v_part[i].p_tag   = (pTag)Swapping.Swap((ushort)vtocOld.v_part[i].p_tag);
                            vtocOld.v_part[i].p_flag  = (pFlag)Swapping.Swap((ushort)vtocOld.v_part[i].p_flag);
                            vtocOld.v_part[i].p_start = Swapping.Swap(vtocOld.v_part[i].p_start);
                            vtocOld.v_part[i].p_size  = Swapping.Swap(vtocOld.v_part[i].p_size);
                            vtocOld.timestamp[i]      = Swapping.Swap(vtocOld.timestamp[i]);
                        }
                    }
                }
            }

            if(!magicFound)
            {
                AaruConsole.DebugWriteLine("VTOC plugin", "Searching for VTOC on relative byte {0}", pd.vtoc_ptr);
                ulong relSecPtr = pd.vtoc_ptr               / imagePlugin.Info.SectorSize;
                uint  relSecOff = pd.vtoc_ptr               % imagePlugin.Info.SectorSize;
                uint  secCount  = (relSecOff + pd.vtoc_len) / imagePlugin.Info.SectorSize;

                if((relSecOff + pd.vtoc_len) % imagePlugin.Info.SectorSize > 0)
                    secCount++;

                AaruConsole.DebugWriteLine("VTOC plugin",
                                           "Going to read {0} sectors from sector {1}, getting VTOC from byte {2}",
                                           secCount, relSecPtr + sectorOffset, relSecOff);

                if(relSecPtr + sectorOffset + secCount >= imagePlugin.Info.Sectors)
                {
                    AaruConsole.DebugWriteLine("VTOC plugin", "Going to read past device size, aborting...");

                    return false;
                }

                byte[] tmp = imagePlugin.ReadSectors(relSecPtr + sectorOffset, secCount);
                vtocsector = new byte[pd.vtoc_len];
                Array.Copy(tmp, relSecOff, vtocsector, 0, pd.vtoc_len);
                magic = BitConverter.ToUInt32(vtocsector, 0);

                if(magic == VTOC_SANE ||
                   magic == VTOC_ENAS)
                {
                    magicFound = true;
                    AaruConsole.DebugWriteLine("VTOC plugin", "New VTOC found.");

                    if(magic == VTOC_SANE)
                        vtoc = Marshal.ByteArrayToStructureLittleEndian<vtoc>(vtocsector);
                    else
                    {
                        vtoc = Marshal.ByteArrayToStructureBigEndian<vtoc>(vtocsector);

                        for(int i = 0; i < vtoc.v_part.Length; i++)
                        {
                            vtoc.v_part[i].p_tag   = (pTag)Swapping.Swap((ushort)vtoc.v_part[i].p_tag);
                            vtoc.v_part[i].p_flag  = (pFlag)Swapping.Swap((ushort)vtoc.v_part[i].p_flag);
                            vtoc.v_part[i].p_start = Swapping.Swap(vtoc.v_part[i].p_start);
                            vtoc.v_part[i].p_size  = Swapping.Swap(vtoc.v_part[i].p_size);
                            vtoc.timestamp[i]      = Swapping.Swap(vtoc.timestamp[i]);
                        }
                    }
                }
            }

            if(!magicFound)
            {
                AaruConsole.DebugWriteLine("VTOC plugin", "Cannot find VTOC.");

                return false;
            }

            if(useOld)
            {
                AaruConsole.DebugWriteLine("VTOC plugin", "vtocOld.v_sanity = 0x{0:X8} (should be 0x{1:X8})",
                                           vtocOld.v_sanity, VTOC_SANE);

                AaruConsole.DebugWriteLine("VTOC plugin", "vtocOld.v_version = {0}", vtocOld.v_version);

                AaruConsole.DebugWriteLine("VTOC plugin", "vtocOld.v_volume = \"{0}\"",
                                           StringHandlers.CToString(vtocOld.v_volume));

                AaruConsole.DebugWriteLine("VTOC plugin", "vtocOld.v_sectorsz = {0}", vtocOld.v_sectorsz);
                AaruConsole.DebugWriteLine("VTOC plugin", "vtocOld.v_nparts = {0}", vtocOld.v_nparts);

                for(int i = 0; i < V_NUMPAR; i++)
                {
                    AaruConsole.DebugWriteLine("VTOC plugin", "vtocOld.v_part[{0}].p_tag = {1} ({2})", i,
                                               vtocOld.v_part[i].p_tag, (ushort)vtocOld.v_part[i].p_tag);

                    AaruConsole.DebugWriteLine("VTOC plugin", "vtocOld.v_part[{0}].p_flag = {1} ({2})", i,
                                               vtocOld.v_part[i].p_flag, (ushort)vtocOld.v_part[i].p_flag);

                    AaruConsole.DebugWriteLine("VTOC plugin", "vtocOld.v_part[{0}].p_start = {1}", i,
                                               vtocOld.v_part[i].p_start);

                    AaruConsole.DebugWriteLine("VTOC plugin", "vtocOld.v_part[{0}].p_size = {1}", i,
                                               vtocOld.v_part[i].p_size);

                    AaruConsole.DebugWriteLine("VTOC plugin", "vtocOld.timestamp[{0}] = {1}", i,
                                               DateHandlers.UnixToDateTime(vtocOld.timestamp[i]));
                }
            }
            else
            {
                AaruConsole.DebugWriteLine("VTOC plugin", "vtoc.v_sanity = 0x{0:X8} (should be 0x{1:X8})",
                                           vtoc.v_sanity, VTOC_SANE);

                AaruConsole.DebugWriteLine("VTOC plugin", "vtoc.v_version = {0}", vtoc.v_version);

                AaruConsole.DebugWriteLine("VTOC plugin", "vtoc.v_volume = \"{0}\"",
                                           StringHandlers.CToString(vtoc.v_volume));

                AaruConsole.DebugWriteLine("VTOC plugin", "vtoc.v_pad = {0}", vtoc.v_pad);
                AaruConsole.DebugWriteLine("VTOC plugin", "vtoc.v_nparts = {0}", vtoc.v_nparts);

                for(int i = 0; i < V_NUMPAR; i++)
                {
                    AaruConsole.DebugWriteLine("VTOC plugin", "vtoc.v_part[{0}].p_tag = {1} ({2})", i,
                                               vtoc.v_part[i].p_tag, (ushort)vtoc.v_part[i].p_tag);

                    AaruConsole.DebugWriteLine("VTOC plugin", "vtoc.v_part[{0}].p_flag = {1} ({2})", i,
                                               vtoc.v_part[i].p_flag, (ushort)vtoc.v_part[i].p_flag);

                    AaruConsole.DebugWriteLine("VTOC plugin", "vtoc.v_part[{0}].p_start = {1}", i,
                                               vtoc.v_part[i].p_start);

                    AaruConsole.DebugWriteLine("VTOC plugin", "vtoc.v_part[{0}].p_size = {1}", i,
                                               vtoc.v_part[i].p_size);

                    AaruConsole.DebugWriteLine("VTOC plugin", "vtoc.timestamp[{0}] = {1}", i,
                                               DateHandlers.UnixToDateTime(vtoc.timestamp[i]));
                }
            }

            uint        bps;
            partition[] parts;
            int[]       timestamps;

            if(useOld)
            {
                bps        = vtocOld.v_sectorsz;
                parts      = vtocOld.v_part;
                timestamps = vtocOld.timestamp;
            }
            else
            {
                bps        = pd.bytes;
                parts      = vtoc.v_part;
                timestamps = vtoc.timestamp;
            }

            // Check for a partition describing the VTOC whose start is the same as the start we know.
            // This means partition starts are absolute, not relative, to the VTOC position
            for(int i = 0; i < V_NUMPAR; i++)
                if(parts[i].p_tag          == pTag.V_BACKUP &&
                   (ulong)parts[i].p_start == sectorOffset)
                {
                    absolute = true;

                    break;
                }

            for(int i = 0; i < V_NUMPAR; i++)
                if(parts[i].p_tag != pTag.V_UNUSED)
                {
                    var part = new Partition
                    {
                        Start    = (ulong)(parts[i].p_start * bps) / imagePlugin.Info.SectorSize,
                        Length   = (ulong)(parts[i].p_size  * bps) / imagePlugin.Info.SectorSize,
                        Offset   = (ulong)(parts[i].p_start * bps),
                        Size     = (ulong)(parts[i].p_size  * bps),
                        Sequence = (ulong)i,
                        Type     = $"UNIX: {DecodeUnixtag(parts[i].p_tag, !useOld)}",
                        Scheme   = Name
                    };

                    string info = "";

                    // Apparently old ones are absolute :?
                    if(!useOld &&
                       !absolute)
                    {
                        part.Start  += sectorOffset;
                        part.Offset += sectorOffset * imagePlugin.Info.SectorSize;
                    }

                    if(parts[i].p_flag.HasFlag(pFlag.V_VALID))
                        info += " (valid)";

                    if(parts[i].p_flag.HasFlag(pFlag.V_UNMNT))
                        info += " (unmountable)";

                    if(parts[i].p_flag.HasFlag(pFlag.V_OPEN))
                        info += " (open)";

                    if(parts[i].p_flag.HasFlag(pFlag.V_REMAP))
                        info += " (alternate sector mapping)";

                    if(parts[i].p_flag.HasFlag(pFlag.V_RONLY))
                        info += " (read-only)";

                    if(timestamps[i] != 0)
                        info += $" created on {DateHandlers.UnixToDateTime(timestamps[i])}";

                    part.Description = "UNIX slice" + info + ".";

                    if(part.End < imagePlugin.Info.Sectors)
                        partitions.Add(part);
                }

            return partitions.Count > 0;
        }

        static string DecodeUnixtag(pTag type, bool isNew)
        {
            switch(type)
            {
                case pTag.V_UNUSED:      return "Unused";
                case pTag.V_BOOT:        return "Boot";
                case pTag.V_ROOT:        return "/";
                case pTag.V_SWAP:        return "Swap";
                case pTag.V_USER:        return "/usr";
                case pTag.V_BACKUP:      return "Whole disk";
                case pTag.V_STAND_OLD:   return isNew ? "Stand" : "Alternate sector space";
                case pTag.V_VAR_OLD:     return isNew ? "/var" : "non UNIX";
                case pTag.V_HOME_OLD:    return isNew ? "/home" : "Alternate track space";
                case pTag.V_ALTSCTR_OLD: return isNew ? "Alternate sector track" : "Stand";
                case pTag.V_CACHE:       return isNew ? "Cache" : "/var";
                case pTag.V_RESERVED:    return isNew ? "Reserved" : "/home";
                case pTag.V_DUMP:        return "dump";
                case pTag.V_ALTSCTR:     return "Alternate sector track";
                case pTag.V_VMPUBLIC:    return "volume mgt public partition";
                case pTag.V_VMPRIVATE:   return "volume mgt private partition";
                default:                 return $"Unknown TAG: 0x{type:X4}";
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PDInfo
        {
            public readonly uint driveid; /*identifies the device type*/
            public readonly uint sanity;  /*verifies device sanity*/
            public readonly uint version; /*version number*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public readonly byte[] serial;  /*serial number of the device*/
            public readonly uint cyls;      /*number of cylinders per drive*/
            public readonly uint tracks;    /*number tracks per cylinder*/
            public readonly uint sectors;   /*number sectors per track*/
            public readonly uint bytes;     /*number of bytes per sector*/
            public readonly uint logicalst; /*sector address of logical sector 0*/
            public readonly uint errlogst;  /*sector address of error log area*/
            public readonly uint errlogsz;  /*size in bytes of error log area*/
            public readonly uint mfgst;     /*sector address of mfg. defect info*/
            public readonly uint mfgsz;     /*size in bytes of mfg. defect info*/
            public readonly uint defectst;  /*sector address of the defect map*/
            public readonly uint defectsz;  /*size in bytes of defect map*/
            public readonly uint relno;     /*number of relocation areas*/
            public readonly uint relst;     /*sector address of relocation area*/
            public readonly uint relsz;     /*size in sectors of relocation area*/
            public readonly uint relnext;   /*address of next avail reloc sector*/
            /* the previous items are left intact from AT&T's 3b2 pdinfo.  Following
               are added for the 80386 port */
            public readonly uint   vtoc_ptr; /*byte offset of vtoc block*/
            public readonly ushort vtoc_len; /*byte length of vtoc block*/
            public readonly ushort vtoc_pad; /* pad for 16-bit machine alignment */
            public readonly uint   alt_ptr;  /*byte offset of alternates table*/
            public readonly ushort alt_len;  /*byte length of alternates table*/
            /* new in version 3 */
            public readonly uint   pcyls;      /*physical cylinders per drive*/
            public readonly uint   ptracks;    /*physical tracks per cylinder*/
            public readonly uint   psectors;   /*physical sectors per track*/
            public readonly uint   pbytes;     /*physical bytes per sector*/
            public readonly uint   secovhd;    /*sector overhead bytes per sector*/
            public readonly ushort interleave; /*interleave factor*/
            public readonly ushort skew;       /*skew factor*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly uint[] pad; /*space for more stuff*/
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PDInfoOld
        {
            public readonly uint driveid; /*identifies the device type*/
            public readonly uint sanity;  /*verifies device sanity*/
            public readonly uint version; /*version number*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public readonly byte[] serial;  /*serial number of the device*/
            public readonly uint cyls;      /*number of cylinders per drive*/
            public readonly uint tracks;    /*number tracks per cylinder*/
            public readonly uint sectors;   /*number sectors per track*/
            public readonly uint bytes;     /*number of bytes per sector*/
            public readonly uint logicalst; /*sector address of logical sector 0*/
            public readonly uint errlogst;  /*sector address of error log area*/
            public readonly uint errlogsz;  /*size in bytes of error log area*/
            public readonly uint mfgst;     /*sector address of mfg. defect info*/
            public readonly uint mfgsz;     /*size in bytes of mfg. defect info*/
            public readonly uint defectst;  /*sector address of the defect map*/
            public readonly uint defectsz;  /*size in bytes of defect map*/
            public readonly uint relno;     /*number of relocation areas*/
            public readonly uint relst;     /*sector address of relocation area*/
            public readonly uint relsz;     /*size in sectors of relocation area*/
            public readonly uint relnext;   /*address of next avail reloc sector*/
            public readonly uint allcstrt;  /*start of the allocatable disk*/
            public readonly uint allcend;   /*end of allocatable disk*/
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct vtocold
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly uint[] v_bootinfo; /*info needed by mboot*/
            public readonly uint v_sanity;     /*to verify vtoc sanity*/
            public readonly uint v_version;    /*layout version*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] v_volume;   /*volume name*/
            public readonly ushort v_sectorsz; /*sector size in bytes*/
            public readonly ushort v_nparts;   /*number of partitions*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public readonly uint[] v_reserved; /*free space*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = V_NUMPAR)]
            public readonly partition[] v_part; /*partition headers*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = V_NUMPAR)]
            public readonly int[] timestamp; /* SCSI time stamp */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct vtoc
        {
            public readonly uint v_sanity;  /*to verify vtoc sanity*/
            public readonly uint v_version; /*layout version*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] v_volume; /*volume name*/
            public readonly ushort v_nparts; /*number of partitions*/
            public readonly ushort v_pad;    /*pad for 286 compiler*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public readonly uint[] v_reserved; /*free space*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = V_NUMPAR)]
            public readonly partition[] v_part; /*partition headers*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = V_NUMPAR)]
            public readonly int[] timestamp; /* SCSI time stamp */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]

        // ReSharper disable once InconsistentNaming
        struct partition
        {
            public pTag  p_tag;   /*ID tag of partition*/
            public pFlag p_flag;  /*permision flags*/
            public int   p_start; /*start sector no of partition*/
            public int   p_size;  /*# of blocks in partition*/
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        enum pTag : ushort
        {
            /// <summary>empty</summary>
            V_UNUSED = 0x0000,
            /// <summary>boot</summary>
            V_BOOT = 0x0001,
            /// <summary>root</summary>
            V_ROOT = 0x0002,
            /// <summary>swap</summary>
            V_SWAP = 0x0003,
            /// <summary>/usr</summary>
            V_USER = 0x0004,
            /// <summary>whole disk</summary>
            V_BACKUP = 0x0005,
            /// <summary>stand partition ??</summary>
            V_STAND_OLD = 0x0006,
            /// <summary>alternate sector space</summary>
            V_ALTS_OLD = 0x0006,
            /// <summary>/var</summary>
            V_VAR_OLD = 0x0007,
            /// <summary>non UNIX</summary>
            V_OTHER = 0x0007,
            /// <summary>/home</summary>
            V_HOME_OLD = 0x0008,
            /// <summary>alternate track space</summary>
            V_ALTS = 0x0008,
            /// <summary>alternate sector track</summary>
            V_ALTSCTR_OLD = 0x0009,
            /// <summary>stand partition ??</summary>
            V_STAND = 0x0009,
            /// <summary>cache</summary>
            V_CACHE = 0x000A,
            /// <summary>/var</summary>
            V_VAR = 0x000A,
            /// <summary>reserved</summary>
            V_RESERVED = 0x000B,
            /// <summary>/home</summary>
            V_HOME = 0x000B,
            /// <summary>dump partition</summary>
            V_DUMP = 0x000C,
            /// <summary>alternate sector track</summary>
            V_ALTSCTR = 0x000D,
            /// <summary>volume mgt public partition</summary>
            V_VMPUBLIC = 0x000E,
            /// <summary>volume mgt private partition</summary>
            V_VMPRIVATE = 0x000F
        }

        [Flags, SuppressMessage("ReSharper", "InconsistentNaming")]
        enum pFlag : ushort
        {
            /* Partition permission flags */ V_UNMNT = 0x01, /* Unmountable partition */ V_RONLY = 0x10, /* Read only */
            V_REMAP = 0x20, /* do alternate sector mapping */ V_OPEN = 0x100, /* Partition open (for driver use) */
            V_VALID = 0x200, /* Partition is valid to use */ V_VOMASK = 0x300 /* mask for open and valid */
        }
    }
}