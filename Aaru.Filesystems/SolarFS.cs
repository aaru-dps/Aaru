// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SolarFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SolarOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the SolarOS filesystem and shows information.
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
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems;

// Based on FAT's BPB, cannot find a FAT or directory
/// <inheritdoc />
/// <summary>Implements detection of the Solar OS filesystem</summary>
public sealed class SolarFS : IFilesystem
{
    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => "Solar_OS filesystem";
    /// <inheritdoc />
    public Guid Id => new("EA3101C1-E777-4B4F-B5A3-8C57F50F6E65");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] bpb);

        if(errno != ErrorNumber.NoError)
            return false;

        byte[] fsTypeB = new byte[8];

        byte signature = bpb[0x25];
        Array.Copy(bpb, 0x35, fsTypeB, 0, 8);
        string fsType = StringHandlers.CToString(fsTypeB);

        return signature == 0x29 && fsType == "SOL_FS  ";
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                               Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        var         sb    = new StringBuilder();
        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] bpbSector);

        if(errno != ErrorNumber.NoError)
            return;

        var bpb = new BiosParameterBlock
        {
            bps       = BitConverter.ToUInt16(bpbSector, 0x0B),
            root_ent  = BitConverter.ToUInt16(bpbSector, 0x10),
            sectors   = BitConverter.ToUInt16(bpbSector, 0x12),
            media     = bpbSector[0x14],
            spfat     = BitConverter.ToUInt16(bpbSector, 0x15),
            sptrk     = BitConverter.ToUInt16(bpbSector, 0x17),
            heads     = BitConverter.ToUInt16(bpbSector, 0x19),
            signature = bpbSector[0x25]
        };

        byte[] bpbStrings = new byte[8];
        Array.Copy(bpbSector, 0x03, bpbStrings, 0, 8);
        bpb.OEMName = StringHandlers.CToString(bpbStrings);
        bpbStrings  = new byte[8];
        Array.Copy(bpbSector, 0x2A, bpbStrings, 0, 11);
        bpb.vol_name = StringHandlers.CToString(bpbStrings, Encoding);
        bpbStrings   = new byte[8];
        Array.Copy(bpbSector, 0x35, bpbStrings, 0, 8);
        bpb.fs_type = StringHandlers.CToString(bpbStrings, Encoding);

        bpb.x86_jump = new byte[3];
        Array.Copy(bpbSector, 0x00, bpb.x86_jump, 0, 3);
        bpb.unk1 = bpbSector[0x0D];
        bpb.unk2 = BitConverter.ToUInt16(bpbSector, 0x0E);
        bpb.unk3 = new byte[10];
        Array.Copy(bpbSector, 0x1B, bpb.unk3, 0, 10);
        bpb.unk4 = BitConverter.ToUInt32(bpbSector, 0x26);

        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.x86_jump: 0x{0:X2}{1:X2}{2:X2}", bpb.x86_jump[0],
                                   bpb.x86_jump[1], bpb.x86_jump[2]);

        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.OEMName: \"{0}\"", bpb.OEMName);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.bps: {0}", bpb.bps);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.unk1: 0x{0:X2}", bpb.unk1);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.unk2: 0x{0:X4}", bpb.unk2);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.root_ent: {0}", bpb.root_ent);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.sectors: {0}", bpb.sectors);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.media: 0x{0:X2}", bpb.media);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.spfat: {0}", bpb.spfat);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.sptrk: {0}", bpb.sptrk);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.heads: {0}", bpb.heads);

        AaruConsole.DebugWriteLine("SolarFS plugin",
                                   "BPB.unk3: 0x{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2}{9:X2}",
                                   bpb.unk3[0], bpb.unk3[1], bpb.unk3[2], bpb.unk3[3], bpb.unk3[4], bpb.unk3[5],
                                   bpb.unk3[6], bpb.unk3[7], bpb.unk3[8], bpb.unk3[9]);

        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.signature: 0x{0:X2}", bpb.signature);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.unk4: 0x{0:X8}", bpb.unk4);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.vol_name: \"{0}\"", bpb.vol_name);
        AaruConsole.DebugWriteLine("SolarFS plugin", "BPB.fs_type: \"{0}\"", bpb.fs_type);

        sb.AppendLine("Solar_OS filesystem");
        sb.AppendFormat("Media descriptor: 0x{0:X2}", bpb.media).AppendLine();
        sb.AppendFormat("{0} bytes per sector", bpb.bps).AppendLine();

        if(imagePlugin.Info.SectorSize == 2336 ||
           imagePlugin.Info.SectorSize == 2352 ||
           imagePlugin.Info.SectorSize == 2448)
        {
            if(bpb.bps != imagePlugin.Info.SectorSize)
                sb.
                    AppendFormat("WARNING: Filesystem describes a {0} bytes/sector, while device describes a {1} bytes/sector",
                                 bpb.bps, 2048).AppendLine();
        }
        else if(bpb.bps != imagePlugin.Info.SectorSize)
            sb.
                AppendFormat("WARNING: Filesystem describes a {0} bytes/sector, while device describes a {1} bytes/sector",
                             bpb.bps, imagePlugin.Info.SectorSize).AppendLine();

        sb.AppendFormat("{0} sectors on volume ({1} bytes)", bpb.sectors, bpb.sectors * bpb.bps).AppendLine();

        if(bpb.sectors > imagePlugin.Info.Sectors)
            sb.AppendFormat("WARNING: Filesystem describes a {0} sectors volume, bigger than device ({1} sectors)",
                            bpb.sectors, imagePlugin.Info.Sectors);

        sb.AppendFormat("{0} heads", bpb.heads).AppendLine();
        sb.AppendFormat("{0} sectors per track", bpb.sptrk).AppendLine();
        sb.AppendFormat("Volume name: {0}", bpb.vol_name).AppendLine();

        XmlFsType = new FileSystemType
        {
            Type        = "SolarFS",
            Clusters    = bpb.sectors,
            ClusterSize = bpb.bps,
            VolumeName  = bpb.vol_name
        };

        information = sb.ToString();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    struct BiosParameterBlock
    {
        /// <summary>0x00, x86 jump (3 bytes), jumps to 0x60</summary>
        public byte[] x86_jump;
        /// <summary>0x03, 8 bytes, "SOLAR_OS"</summary>
        public string OEMName;
        /// <summary>0x0B, Bytes per sector</summary>
        public ushort bps;
        /// <summary>0x0D, unknown, 0x01</summary>
        public byte unk1;
        /// <summary>0x0E, unknown, 0x0201</summary>
        public ushort unk2;
        /// <summary>0x10, Number of entries on root directory ? (no root directory found)</summary>
        public ushort root_ent;
        /// <summary>0x12, Sectors in volume</summary>
        public ushort sectors;
        /// <summary>0x14, Media descriptor</summary>
        public byte media;
        /// <summary>0x15, Sectors per FAT ? (no FAT found)</summary>
        public ushort spfat;
        /// <summary>0x17, Sectors per track</summary>
        public ushort sptrk;
        /// <summary>0x19, Heads</summary>
        public ushort heads;
        /// <summary>0x1B, unknown, 10 bytes, zero-filled</summary>
        public byte[] unk3;
        /// <summary>0x25, 0x29</summary>
        public byte signature;
        /// <summary>0x26, unknown, zero-filled</summary>
        public uint unk4;
        /// <summary>0x2A, 11 bytes, volume name, space-padded</summary>
        public string vol_name;
        /// <summary>0x35, 8 bytes, "SOL_FS  "</summary>
        public string fs_type;
    }
}