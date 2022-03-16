// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles mounting and umounting the Microsoft FAT filesystem.
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

namespace Aaru.Filesystems;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using FileSystemInfo = Aaru.CommonTypes.Structs.FileSystemInfo;
using Marshal = Aaru.Helpers.Marshal;

public sealed partial class FAT
{
    uint        _fatEntriesPerSector;
    IMediaImage _image;

    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options, string @namespace)
    {
        XmlFsType = new FileSystemType();
        ErrorNumber errno;

        options ??= GetDefaultOptions();

        if(options.TryGetValue("debug", out string debugString))
            bool.TryParse(debugString, out _debug);

        // Default namespace
        @namespace ??= "ecs";

        switch(@namespace.ToLowerInvariant())
        {
            case "dos":
                _namespace = Namespace.Dos;

                break;
            case "nt":
                _namespace = Namespace.Nt;

                break;
            case "os2":
                _namespace = Namespace.Os2;

                break;
            case "ecs":
                _namespace = Namespace.Ecs;

                break;
            case "lfn":
                _namespace = Namespace.Lfn;

                break;
            case "human":
                _namespace = Namespace.Human;

                break;
            default: return ErrorNumber.InvalidArgument;
        }

        AaruConsole.DebugWriteLine("FAT plugin", "Reading BPB");

        uint sectorsPerBpb = imagePlugin.Info.SectorSize < 512 ? 512 / imagePlugin.Info.SectorSize : 1;

        errno = imagePlugin.ReadSectors(0 + partition.Start, sectorsPerBpb, out byte[] bpbSector);

        if(errno != ErrorNumber.NoError)
            return errno;

        BpbKind bpbKind = DetectBpbKind(bpbSector, imagePlugin, partition, out BiosParameterBlockEbpb fakeBpb,
                                        out HumanParameterBlock humanBpb, out AtariParameterBlock atariBpb,
                                        out byte minBootNearJump, out bool andosOemCorrect, out bool bootable);

        _fat12             = false;
        _fat16             = false;
        _fat32             = false;
        _useFirstFat       = true;
        XmlFsType.Bootable = bootable;

        _statfs = new FileSystemInfo
        {
            FilenameLength = 11,
            Files          = 0, // Requires traversing all directories
            FreeFiles      = 0,
            PluginId       = Id,
            FreeBlocks     = 0 // Requires traversing the FAT
        };

        // This is needed because for FAT16, GEMDOS increases bytes per sector count instead of using big_sectors field.
        uint sectorsPerRealSector = 1;

        // This is needed because some OSes don't put volume label as first entry in the root directory
        uint sectorsForRootDirectory = 0;
        uint rootDirectoryCluster    = 0;

        Encoding = encoding ?? (bpbKind == BpbKind.Human ? Encoding.GetEncoding("shift_jis")
                                    : Encoding.GetEncoding("IBM437"));

        switch(bpbKind)
        {
            case BpbKind.DecRainbow:
            case BpbKind.Hardcoded:
            case BpbKind.Msx:
            case BpbKind.Apricot:
                _fat12 = true;

                break;
            case BpbKind.ShortFat32:
            case BpbKind.LongFat32:
            {
                _fat32 = true;

                Fat32ParameterBlock fat32Bpb = Marshal.ByteArrayToStructureLittleEndian<Fat32ParameterBlock>(bpbSector);

                Fat32ParameterBlockShort shortFat32Bpb =
                    Marshal.ByteArrayToStructureLittleEndian<Fat32ParameterBlockShort>(bpbSector);

                rootDirectoryCluster = fat32Bpb.root_cluster;

                // This is to support FAT partitions on hybrid ISO/USB images
                if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
                {
                    fat32Bpb.bps       *= 4;
                    fat32Bpb.spc       /= 4;
                    fat32Bpb.big_spfat /= 4;
                    fat32Bpb.hsectors  /= 4;
                    fat32Bpb.sptrk     /= 4;
                }

                XmlFsType.Type = fat32Bpb.version != 0 ? "FAT+" : "FAT32";

                if(fat32Bpb.oem_name != null &&
                   (fat32Bpb.oem_name[5] != 0x49 || fat32Bpb.oem_name[6] != 0x48 || fat32Bpb.oem_name[7] != 0x43))
                    XmlFsType.SystemIdentifier = StringHandlers.CToString(fat32Bpb.oem_name);

                _sectorsPerCluster    = fat32Bpb.spc;
                XmlFsType.ClusterSize = (uint)(fat32Bpb.bps * fat32Bpb.spc);
                _reservedSectors      = fat32Bpb.rsectors;

                if(fat32Bpb.big_sectors == 0 &&
                   fat32Bpb.signature   == 0x28)
                    XmlFsType.Clusters = shortFat32Bpb.huge_sectors / shortFat32Bpb.spc;
                else if(fat32Bpb.sectors == 0)
                    XmlFsType.Clusters = fat32Bpb.big_sectors / fat32Bpb.spc;
                else
                    XmlFsType.Clusters = (ulong)(fat32Bpb.sectors / fat32Bpb.spc);

                _sectorsPerFat         = fat32Bpb.big_spfat;
                XmlFsType.VolumeSerial = $"{fat32Bpb.serial_no:X8}";

                _statfs.Id = new FileSystemId
                {
                    IsInt    = true,
                    Serial32 = fat32Bpb.serial_no
                };

                if((fat32Bpb.flags & 0xF8) == 0x00)
                    if((fat32Bpb.flags & 0x01) == 0x01)
                        XmlFsType.Dirty = true;

                if((fat32Bpb.mirror_flags & 0x80) == 0x80)
                    _useFirstFat = (fat32Bpb.mirror_flags & 0xF) != 1;

                if(fat32Bpb.signature == 0x29)
                {
                    XmlFsType.VolumeName = StringHandlers.SpacePaddedToString(fat32Bpb.volume_label, Encoding);
                    XmlFsType.VolumeName = XmlFsType.VolumeName?.Replace("\0", "");
                }

                // Check that jumps to a correct boot code position and has boot signature set.
                // This will mean that the volume will boot, even if just to say "this is not bootable change disk"......
                XmlFsType.Bootable =
                    fat32Bpb.jump[0] == 0xEB && fat32Bpb.jump[1] >= minBootNearJump && fat32Bpb.jump[1] < 0x80 ||
                    fat32Bpb.jump[0]                        == 0xE9            && fat32Bpb.jump.Length >= 3 &&
                    BitConverter.ToUInt16(fat32Bpb.jump, 1) >= minBootNearJump &&
                    BitConverter.ToUInt16(fat32Bpb.jump, 1) <= 0x1FC;

                sectorsPerRealSector =  fat32Bpb.bps / imagePlugin.Info.SectorSize;
                _sectorsPerCluster   *= sectorsPerRealSector;

                // First root directory sector
                _firstClusterSector =
                    (ulong)(fat32Bpb.big_spfat * fat32Bpb.fats_no + fat32Bpb.rsectors) * sectorsPerRealSector -
                    2                                                                  * _sectorsPerCluster;

                if(fat32Bpb.fsinfo_sector + partition.Start <= partition.End)
                {
                    errno = imagePlugin.ReadSector(fat32Bpb.fsinfo_sector + partition.Start, out byte[] fsinfoSector);

                    if(errno != ErrorNumber.NoError)
                        return errno;

                    FsInfoSector fsInfo = Marshal.ByteArrayToStructureLittleEndian<FsInfoSector>(fsinfoSector);

                    if(fsInfo.signature1 == FSINFO_SIGNATURE1 &&
                       fsInfo.signature2 == FSINFO_SIGNATURE2 &&
                       fsInfo.signature3 == FSINFO_SIGNATURE3)
                        if(fsInfo.free_clusters < 0xFFFFFFFF)
                        {
                            XmlFsType.FreeClusters          = fsInfo.free_clusters;
                            XmlFsType.FreeClustersSpecified = true;
                        }
                }

                break;
            }

            // Some fields could overflow fake BPB, those will be handled below
            case BpbKind.Atari:
            {
                ushort sum = 0;

                for(var i = 0; i < bpbSector.Length; i += 2)
                    sum += BigEndianBitConverter.ToUInt16(bpbSector, i);

                // TODO: Check this
                if(sum == 0x1234)
                    XmlFsType.Bootable = true;

                // BGM changes the bytes per sector instead of changing the sectors per cluster. Why?! WHY!?
                uint ratio = fakeBpb.bps / imagePlugin.Info.SectorSize;
                fakeBpb.bps         = (ushort)imagePlugin.Info.SectorSize;
                fakeBpb.spc         = (byte)(fakeBpb.spc        * ratio);
                fakeBpb.rsectors    = (ushort)(fakeBpb.rsectors * ratio);
                fakeBpb.big_sectors = fakeBpb.sectors * ratio;
                fakeBpb.sectors     = 0;
                fakeBpb.spfat       = (ushort)(fakeBpb.spfat * ratio);
                fakeBpb.sptrk       = (ushort)(fakeBpb.sptrk * ratio);

                break;
            }

            case BpbKind.Human:
                // If not debug set Human68k namespace and ShiftJIS codepage as defaults
                if(!_debug)
                    _namespace = Namespace.Human;

                XmlFsType.Bootable = true;

                break;
        }

        ulong firstRootSector = 0;

        if(!_fat32)
        {
            // This is to support FAT partitions on hybrid ISO/USB images
            if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
            {
                fakeBpb.bps      *= 4;
                fakeBpb.spc      /= 4;
                fakeBpb.spfat    /= 4;
                fakeBpb.hsectors /= 4;
                fakeBpb.sptrk    /= 4;
                fakeBpb.rsectors /= 4;

                if(fakeBpb.spc == 0)
                    fakeBpb.spc = 1;
            }

            ulong clusters;

            if(bpbKind != BpbKind.Human)
            {
                int reservedSectors = fakeBpb.rsectors + fakeBpb.fats_no * fakeBpb.spfat +
                                      fakeBpb.root_ent * 32              / fakeBpb.bps;

                if(fakeBpb.sectors == 0)
                    clusters = (ulong)(fakeBpb.spc == 0 ? fakeBpb.big_sectors - reservedSectors
                                           : (fakeBpb.big_sectors - reservedSectors) / fakeBpb.spc);
                else
                    clusters = (ulong)(fakeBpb.spc == 0 ? fakeBpb.sectors - reservedSectors
                                           : (fakeBpb.sectors - reservedSectors) / fakeBpb.spc);
            }
            else
                clusters = humanBpb.clusters == 0 ? humanBpb.big_clusters : humanBpb.clusters;

            // This will walk all the FAT entries and check if they're valid FAT12 or FAT16 entries.
            // If the whole table is valid in both senses, it considers the type entry in the BPB.
            // BeOS is known to set the type as FAT16 but treat it as FAT12.
            if(!_fat12 &&
               !_fat16)
            {
                if(clusters < 4089)
                {
                    var fat12 = new ushort[clusters];

                    _reservedSectors     = fakeBpb.rsectors;
                    sectorsPerRealSector = fakeBpb.bps / imagePlugin.Info.SectorSize;
                    _fatFirstSector      = partition.Start + _reservedSectors * sectorsPerRealSector;

                    errno = imagePlugin.ReadSectors(_fatFirstSector, fakeBpb.spfat, out byte[] fatBytes);

                    if(errno != ErrorNumber.NoError)
                        return errno;

                    var pos = 0;

                    for(var i = 0; i + 3 < fatBytes.Length && pos < fat12.Length; i += 3)
                    {
                        fat12[pos++] = (ushort)(((fatBytes[i + 1] & 0xF) << 8) + fatBytes[i + 0]);

                        if(pos >= fat12.Length)
                            break;

                        fat12[pos++] = (ushort)(((fatBytes[i + 1] & 0xF0) >> 4) + (fatBytes[i + 2] << 4));
                    }

                    bool fat12Valid = fat12[0] >= FAT12_RESERVED && fat12[1] >= FAT12_RESERVED;

                    foreach(ushort entry in fat12)
                    {
                        if(entry >= FAT12_RESERVED ||
                           entry <= clusters)
                            continue;

                        fat12Valid = false;

                        break;
                    }

                    ushort[] fat16 = MemoryMarshal.Cast<byte, ushort>(fatBytes).ToArray();

                    bool fat16Valid = fat16[0] >= FAT16_RESERVED && fat16[1] >= 0x3FF0;

                    foreach(ushort entry in fat16)
                    {
                        if(entry >= FAT16_RESERVED ||
                           entry <= clusters)
                            continue;

                        fat16Valid = false;

                        break;
                    }

                    _fat12 = fat12Valid;
                    _fat16 = fat16Valid;

                    // Check BPB type
                    if(_fat12          == _fat16 &&
                       fakeBpb.fs_type != null)
                    {
                        _fat12 = Encoding.ASCII.GetString(fakeBpb.fs_type) == "FAT12   ";
                        _fat16 = Encoding.ASCII.GetString(fakeBpb.fs_type) == "FAT16   ";
                    }

                    // Still undecided (fs_type is null or is not FAT1[2|6])
                    if(_fat12 == _fat16)
                    {
                        _fat12 = true;
                        _fat16 = false;
                    }
                }
                else
                    _fat16 = true;
            }

            if(_fat12)
                XmlFsType.Type = "FAT12";
            else if(_fat16)
                XmlFsType.Type = "FAT16";

            if(bpbKind == BpbKind.Atari)
            {
                if(atariBpb.serial_no[0] != 0x49 ||
                   atariBpb.serial_no[1] != 0x48 ||
                   atariBpb.serial_no[2] != 0x43)
                {
                    XmlFsType.VolumeSerial =
                        $"{atariBpb.serial_no[0]:X2}{atariBpb.serial_no[1]:X2}{atariBpb.serial_no[2]:X2}";

                    _statfs.Id = new FileSystemId
                    {
                        IsInt = true,
                        Serial32 = (uint)((atariBpb.serial_no[0] << 16) + (atariBpb.serial_no[1] << 8) +
                                          atariBpb.serial_no[2])
                    };
                }

                XmlFsType.SystemIdentifier = StringHandlers.CToString(atariBpb.oem_name);

                if(string.IsNullOrEmpty(XmlFsType.SystemIdentifier))
                    XmlFsType.SystemIdentifier = null;
            }
            else if(fakeBpb.oem_name != null)
            {
                if(fakeBpb.oem_name[5] != 0x49 ||
                   fakeBpb.oem_name[6] != 0x48 ||
                   fakeBpb.oem_name[7] != 0x43)
                {
                    // Later versions of Windows create a DOS 3 BPB without OEM name on 8 sectors/track floppies
                    // OEM ID should be ASCII, otherwise ignore it
                    if(fakeBpb.oem_name[0] >= 0x20 &&
                       fakeBpb.oem_name[0] <= 0x7F &&
                       fakeBpb.oem_name[1] >= 0x20 &&
                       fakeBpb.oem_name[1] <= 0x7F &&
                       fakeBpb.oem_name[2] >= 0x20 &&
                       fakeBpb.oem_name[2] <= 0x7F &&
                       fakeBpb.oem_name[3] >= 0x20 &&
                       fakeBpb.oem_name[3] <= 0x7F &&
                       fakeBpb.oem_name[4] >= 0x20 &&
                       fakeBpb.oem_name[4] <= 0x7F &&
                       fakeBpb.oem_name[5] >= 0x20 &&
                       fakeBpb.oem_name[5] <= 0x7F &&
                       fakeBpb.oem_name[6] >= 0x20 &&
                       fakeBpb.oem_name[6] <= 0x7F &&
                       fakeBpb.oem_name[7] >= 0x20 &&
                       fakeBpb.oem_name[7] <= 0x7F)
                        XmlFsType.SystemIdentifier = StringHandlers.CToString(fakeBpb.oem_name);
                    else if(fakeBpb.oem_name[0] < 0x20  &&
                            fakeBpb.oem_name[1] >= 0x20 &&
                            fakeBpb.oem_name[1] <= 0x7F &&
                            fakeBpb.oem_name[2] >= 0x20 &&
                            fakeBpb.oem_name[2] <= 0x7F &&
                            fakeBpb.oem_name[3] >= 0x20 &&
                            fakeBpb.oem_name[3] <= 0x7F &&
                            fakeBpb.oem_name[4] >= 0x20 &&
                            fakeBpb.oem_name[4] <= 0x7F &&
                            fakeBpb.oem_name[5] >= 0x20 &&
                            fakeBpb.oem_name[5] <= 0x7F &&
                            fakeBpb.oem_name[6] >= 0x20 &&
                            fakeBpb.oem_name[6] <= 0x7F &&
                            fakeBpb.oem_name[7] >= 0x20 &&
                            fakeBpb.oem_name[7] <= 0x7F)
                        XmlFsType.SystemIdentifier = StringHandlers.CToString(fakeBpb.oem_name, Encoding, start: 1);
                }

                if(fakeBpb.signature is 0x28 or 0x29)
                {
                    XmlFsType.VolumeSerial = $"{fakeBpb.serial_no:X8}";

                    _statfs.Id = new FileSystemId
                    {
                        IsInt    = true,
                        Serial32 = fakeBpb.serial_no
                    };
                }
            }

            XmlFsType.Clusters    = clusters;
            _sectorsPerCluster    = fakeBpb.spc;
            XmlFsType.ClusterSize = (uint)(fakeBpb.bps * fakeBpb.spc);
            _reservedSectors      = fakeBpb.rsectors;
            _sectorsPerFat        = fakeBpb.spfat;

            if(fakeBpb.signature is 0x28 or 0x29 || andosOemCorrect)
            {
                if((fakeBpb.flags & 0xF8) == 0x00)
                    if((fakeBpb.flags & 0x01) == 0x01)
                        XmlFsType.Dirty = true;

                if(fakeBpb.signature == 0x29 || andosOemCorrect)
                {
                    XmlFsType.VolumeName = StringHandlers.SpacePaddedToString(fakeBpb.volume_label, Encoding);
                    XmlFsType.VolumeName = XmlFsType.VolumeName?.Replace("\0", "");
                }
            }

            // Workaround that PCExchange jumps into "FAT16   "...
            if(XmlFsType.SystemIdentifier == "PCX 2.0 ")
                fakeBpb.jump[1] += 8;

            // Check that jumps to a correct boot code position and has boot signature set.
            // This will mean that the volume will boot, even if just to say "this is not bootable change disk"......
            if(XmlFsType.Bootable == false &&
               fakeBpb.jump       != null)
                XmlFsType.Bootable |=
                    fakeBpb.jump[0] == 0xEB && fakeBpb.jump[1] >= minBootNearJump && fakeBpb.jump[1] < 0x80 ||
                    fakeBpb.jump[0]                        == 0xE9            && fakeBpb.jump.Length >= 3 &&
                    BitConverter.ToUInt16(fakeBpb.jump, 1) >= minBootNearJump &&
                    BitConverter.ToUInt16(fakeBpb.jump, 1) <= 0x1FC;

            // First root directory sector
            firstRootSector = (ulong)(fakeBpb.spfat * fakeBpb.fats_no + fakeBpb.rsectors) * sectorsPerRealSector +
                              partition.Start;

            sectorsForRootDirectory = (uint)(fakeBpb.root_ent * 32 / imagePlugin.Info.SectorSize);

            sectorsPerRealSector =  fakeBpb.bps / imagePlugin.Info.SectorSize;
            _sectorsPerCluster   *= sectorsPerRealSector;
        }

        _firstClusterSector += partition.Start;

        _image = imagePlugin;

        if(_fat32)
            _fatEntriesPerSector = imagePlugin.Info.SectorSize / 4;
        else if(_fat16)
            _fatEntriesPerSector = imagePlugin.Info.SectorSize / 2;
        else
            _fatEntriesPerSector = imagePlugin.Info.SectorSize * 2 / 3;

        _fatFirstSector = partition.Start + _reservedSectors * sectorsPerRealSector;

        _rootDirectoryCache = new Dictionary<string, CompleteDirectoryEntry>();
        byte[] rootDirectory;

        if(!_fat32)
        {
            _firstClusterSector = firstRootSector + sectorsForRootDirectory - _sectorsPerCluster * 2;
            errno               = imagePlugin.ReadSectors(firstRootSector, sectorsForRootDirectory, out rootDirectory);

            if(errno != ErrorNumber.NoError)
                return errno;

            if(bpbKind == BpbKind.DecRainbow)
            {
                var rootMs = new MemoryStream();

                foreach(ulong rootSector in new[]
                        {
                            0x17, 0x19, 0x1B, 0x1D, 0x1E, 0x20
                        })
                {
                    errno = imagePlugin.ReadSector(rootSector, out byte[] tmp);

                    if(errno != ErrorNumber.NoError)
                        return errno;

                    rootMs.Write(tmp, 0, tmp.Length);
                }

                rootDirectory = rootMs.ToArray();
            }
        }
        else
        {
            if(rootDirectoryCluster == 0)
                return ErrorNumber.InvalidArgument;

            var    rootMs                = new MemoryStream();
            uint[] rootDirectoryClusters = GetClusters(rootDirectoryCluster);

            foreach(uint cluster in rootDirectoryClusters)
            {
                errno = imagePlugin.ReadSectors(_firstClusterSector + cluster * _sectorsPerCluster, _sectorsPerCluster,
                                                out byte[] buffer);

                if(errno != ErrorNumber.NoError)
                    return errno;

                rootMs.Write(buffer, 0, buffer.Length);
            }

            rootDirectory = rootMs.ToArray();

            // OS/2 FAT32.IFS uses LFN instead of .LONGNAME
            if(_namespace == Namespace.Os2)
                _namespace = Namespace.Lfn;
        }

        if(rootDirectory is null)
            return ErrorNumber.InvalidArgument;

        byte[] lastLfnName     = null;
        byte   lastLfnChecksum = 0;

        for(var i = 0; i < rootDirectory.Length; i += Marshal.SizeOf<DirectoryEntry>())
        {
            DirectoryEntry entry =
                Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry>(rootDirectory, i,
                                                                         Marshal.SizeOf<DirectoryEntry>());

            if(entry.filename[0] == DIRENT_FINISHED)
                break;

            if(entry.attributes.HasFlag(FatAttributes.LFN))
            {
                if(_namespace != Namespace.Lfn &&
                   _namespace != Namespace.Ecs)
                    continue;

                LfnEntry lfnEntry =
                    Marshal.ByteArrayToStructureLittleEndian<LfnEntry>(rootDirectory, i, Marshal.SizeOf<LfnEntry>());

                int lfnSequence = lfnEntry.sequence & LFN_MASK;

                if((lfnEntry.sequence & LFN_ERASED) > 0)
                    continue;

                if((lfnEntry.sequence & LFN_LAST) > 0)
                {
                    lastLfnName     = new byte[lfnSequence * 26];
                    lastLfnChecksum = lfnEntry.checksum;
                }

                if(lastLfnName is null)
                    continue;

                if(lfnEntry.checksum != lastLfnChecksum)
                    continue;

                lfnSequence--;

                Array.Copy(lfnEntry.name1, 0, lastLfnName, lfnSequence * 26, 10);
                Array.Copy(lfnEntry.name2, 0, lastLfnName, lfnSequence * 26 + 10, 12);
                Array.Copy(lfnEntry.name3, 0, lastLfnName, lfnSequence * 26 + 22, 4);

                continue;
            }

            // Not a correct entry
            if(entry.filename[0] < DIRENT_MIN &&
               entry.filename[0] != DIRENT_E5)
                continue;

            // Self
            if(Encoding.GetString(entry.filename).TrimEnd() == ".")
                continue;

            // Parent
            if(Encoding.GetString(entry.filename).TrimEnd() == "..")
                continue;

            // Deleted
            if(entry.filename[0] == DIRENT_DELETED)
                continue;

            string filename;

            if(entry.attributes.HasFlag(FatAttributes.VolumeLabel))
            {
                var fullname = new byte[11];
                Array.Copy(entry.filename, 0, fullname, 0, 8);
                Array.Copy(entry.extension, 0, fullname, 8, 3);
                string volname = Encoding.GetString(fullname).Trim();

                if(!string.IsNullOrEmpty(volname))
                    XmlFsType.VolumeName = entry.caseinfo.HasFlag(CaseInfo.AllLowerCase) && _namespace == Namespace.Nt
                                               ? volname.ToLower() : volname;

                XmlFsType.VolumeName = XmlFsType.VolumeName?.Replace("\0", "");

                if(entry.ctime > 0 &&
                   entry.cdate > 0)
                {
                    XmlFsType.CreationDate = DateHandlers.DosToDateTime(entry.cdate, entry.ctime);

                    if(entry.ctime_ms > 0)
                        XmlFsType.CreationDate = XmlFsType.CreationDate.AddMilliseconds(entry.ctime_ms * 10);

                    XmlFsType.CreationDateSpecified = true;
                }

                if(entry.mtime > 0 &&
                   entry.mdate > 0)
                {
                    XmlFsType.ModificationDate          = DateHandlers.DosToDateTime(entry.mdate, entry.mtime);
                    XmlFsType.ModificationDateSpecified = true;
                }

                continue;
            }

            var completeEntry = new CompleteDirectoryEntry
            {
                Dirent = entry
            };

            if(_namespace is Namespace.Lfn or Namespace.Ecs &&
               lastLfnName != null)
            {
                byte calculatedLfnChecksum = LfnChecksum(entry.filename, entry.extension);

                if(calculatedLfnChecksum == lastLfnChecksum)
                {
                    filename = StringHandlers.CToString(lastLfnName, Encoding.Unicode, true);

                    completeEntry.Lfn = filename;
                    lastLfnName       = null;
                    lastLfnChecksum   = 0;
                }
            }

            if(entry.filename[0] == DIRENT_E5)
                entry.filename[0] = DIRENT_DELETED;

            string name      = Encoding.GetString(entry.filename).TrimEnd();
            string extension = Encoding.GetString(entry.extension).TrimEnd();

            if(_namespace == Namespace.Nt)
            {
                if(entry.caseinfo.HasFlag(CaseInfo.LowerCaseExtension))
                    extension = extension.ToLower(CultureInfo.CurrentCulture);

                if(entry.caseinfo.HasFlag(CaseInfo.LowerCaseBasename))
                    name = name.ToLower(CultureInfo.CurrentCulture);
            }

            if(extension != "")
                filename = name + "." + extension;
            else
                filename = name;

            if(name      == "" &&
               extension == "")
            {
                AaruConsole.DebugWriteLine("FAT filesystem", "Found empty filename in root directory");

                if(!_debug ||
                   entry.size > 0 && entry.start_cluster == 0)
                    continue; // Skip invalid name

                // If debug, add it
                name = ":{EMPTYNAME}:";

                // Try to create a unique filename with an extension from 000 to 999
                for(var uniq = 0; uniq < 1000; uniq++)
                {
                    extension = $"{uniq:D03}";

                    if(!_rootDirectoryCache.ContainsKey($"{name}.{extension}"))
                        break;
                }

                // If we couldn't find it, just skip over
                if(_rootDirectoryCache.ContainsKey($"{name}.{extension}"))
                    continue;
            }

            // Atari ST allows slash AND colon so cannot simply substitute one for the other like in Mac filesystems
            filename = filename.Replace('/', '\u2215');

            completeEntry.Shortname = filename;

            if(_namespace == Namespace.Human)
            {
                HumanDirectoryEntry humanEntry =
                    Marshal.ByteArrayToStructureLittleEndian<HumanDirectoryEntry>(rootDirectory, i,
                        Marshal.SizeOf<HumanDirectoryEntry>());

                completeEntry.HumanDirent = humanEntry;

                name      = StringHandlers.CToString(humanEntry.name1, Encoding).TrimEnd();
                extension = StringHandlers.CToString(humanEntry.extension, Encoding).TrimEnd();
                string name2 = StringHandlers.CToString(humanEntry.name2, Encoding).TrimEnd();

                if(extension != "")
                    filename = name + name2 + "." + extension;
                else
                    filename = name + name2;

                completeEntry.HumanName = filename;
            }

            if(!_fat32 &&
               filename == "EA DATA. SF")
            {
                _eaDirEntry     = entry;
                lastLfnName     = null;
                lastLfnChecksum = 0;

                if(_debug)
                    _rootDirectoryCache[completeEntry.ToString()] = completeEntry;

                continue;
            }

            _rootDirectoryCache[completeEntry.ToString()] = completeEntry;
            lastLfnName                                   = null;
            lastLfnChecksum                               = 0;
        }

        XmlFsType.VolumeName = XmlFsType.VolumeName?.Trim();
        _statfs.Blocks       = XmlFsType.Clusters;

        switch(bpbKind)
        {
            case BpbKind.Hardcoded:
                _statfs.Type = $"Microsoft FAT{(_fat16 ? "16" : "12")}";

                break;
            case BpbKind.Atari:
                _statfs.Type = $"Atari FAT{(_fat16 ? "16" : "12")}";

                break;
            case BpbKind.Msx:
                _statfs.Type = $"MSX FAT{(_fat16 ? "16" : "12")}";

                break;
            case BpbKind.Dos2:
            case BpbKind.Dos3:
            case BpbKind.Dos32:
            case BpbKind.Dos33:
            case BpbKind.ShortExtended:
            case BpbKind.Extended:
                _statfs.Type = $"Microsoft FAT{(_fat16 ? "16" : "12")}";

                break;
            case BpbKind.ShortFat32:
            case BpbKind.LongFat32:
                _statfs.Type = XmlFsType.Type == "FAT+" ? "FAT+" : "Microsoft FAT32";

                break;
            case BpbKind.Andos:
                _statfs.Type = $"ANDOS FAT{(_fat16 ? "16" : "12")}";

                break;
            case BpbKind.Apricot:
                _statfs.Type = $"Apricot FAT{(_fat16 ? "16" : "12")}";

                break;
            case BpbKind.DecRainbow:
                _statfs.Type = $"DEC FAT{(_fat16 ? "16" : "12")}";

                break;
            case BpbKind.Human:
                _statfs.Type = $"Human68k FAT{(_fat16 ? "16" : "12")}";

                break;
            default: throw new ArgumentOutOfRangeException();
        }

        _bytesPerCluster = _sectorsPerCluster * imagePlugin.Info.SectorSize;

        var firstFatEntries  = new ushort[_statfs.Blocks];
        var secondFatEntries = new ushort[_statfs.Blocks];
        var firstFatValid    = true;
        var secondFatValid   = true;

        if(_fat12)
        {
            AaruConsole.DebugWriteLine("FAT plugin", "Reading FAT12");

            errno = imagePlugin.ReadSectors(_fatFirstSector, _sectorsPerFat, out byte[] fatBytes);

            if(errno != ErrorNumber.NoError)
                return errno;

            var pos = 0;

            for(var i = 0; i + 3 < fatBytes.Length && pos < firstFatEntries.Length; i += 3)
            {
                firstFatEntries[pos++] = (ushort)(((fatBytes[i + 1] & 0xF) << 8) + fatBytes[i + 0]);

                if(pos >= firstFatEntries.Length)
                    break;

                firstFatEntries[pos++] = (ushort)(((fatBytes[i + 1] & 0xF0) >> 4) + (fatBytes[i + 2] << 4));
            }

            errno = imagePlugin.ReadSectors(_fatFirstSector + _sectorsPerFat, _sectorsPerFat, out fatBytes);

            if(errno != ErrorNumber.NoError)
                return errno;

            _fatEntries = new ushort[_statfs.Blocks];

            pos = 0;

            for(var i = 0; i + 3 < fatBytes.Length && pos < secondFatEntries.Length; i += 3)
            {
                secondFatEntries[pos++] = (ushort)(((fatBytes[i + 1] & 0xF) << 8) + fatBytes[i + 0]);

                if(pos >= secondFatEntries.Length)
                    break;

                secondFatEntries[pos++] = (ushort)(((fatBytes[i + 1] & 0xF0) >> 4) + (fatBytes[i + 2] << 4));
            }

            foreach(ushort entry in firstFatEntries)
            {
                if(entry >= FAT12_RESERVED ||
                   entry <= _statfs.Blocks)
                    continue;

                firstFatValid = false;

                break;
            }

            foreach(ushort entry in secondFatEntries)
            {
                if(entry >= FAT12_RESERVED ||
                   entry <= _statfs.Blocks)
                    continue;

                secondFatValid = false;

                break;
            }

            if(firstFatValid == secondFatValid)
                _fatEntries = _useFirstFat ? firstFatEntries : secondFatEntries;
            else if(firstFatValid)
                _fatEntries = firstFatEntries;
            else
                _fatEntries = secondFatEntries;
        }
        else if(_fat16)
        {
            AaruConsole.DebugWriteLine("FAT plugin", "Reading FAT16");

            errno = imagePlugin.ReadSectors(_fatFirstSector, _sectorsPerFat, out byte[] fatBytes);

            if(errno != ErrorNumber.NoError)
                return errno;

            AaruConsole.DebugWriteLine("FAT plugin", "Casting FAT");
            firstFatEntries = MemoryMarshal.Cast<byte, ushort>(fatBytes).ToArray();

            errno = imagePlugin.ReadSectors(_fatFirstSector + _sectorsPerFat, _sectorsPerFat, out fatBytes);

            if(errno != ErrorNumber.NoError)
                return errno;

            AaruConsole.DebugWriteLine("FAT plugin", "Casting FAT");
            secondFatEntries = MemoryMarshal.Cast<byte, ushort>(fatBytes).ToArray();

            foreach(ushort entry in firstFatEntries)
            {
                if(entry >= FAT16_RESERVED ||
                   entry <= _statfs.Blocks)
                    continue;

                firstFatValid = false;

                break;
            }

            foreach(ushort entry in secondFatEntries)
            {
                if(entry >= FAT16_RESERVED ||
                   entry <= _statfs.Blocks)
                    continue;

                secondFatValid = false;

                break;
            }

            if(firstFatValid == secondFatValid)
                _fatEntries = _useFirstFat ? firstFatEntries : secondFatEntries;
            else if(firstFatValid)
                _fatEntries = firstFatEntries;
            else
                _fatEntries = secondFatEntries;
        }

        // TODO: Check how this affects international filenames
        _cultureInfo    = new CultureInfo("en-US", false);
        _directoryCache = new Dictionary<string, Dictionary<string, CompleteDirectoryEntry>>();

        // Check it is really an OS/2 EA file
        if(_eaDirEntry.start_cluster != 0)
        {
            CacheEaData();
            var eamagic = BitConverter.ToUInt16(_cachedEaData, 0);

            if(eamagic != EADATA_MAGIC)
            {
                _eaDirEntry   = new DirectoryEntry();
                _cachedEaData = null;
            }
            else
                _eaCache = new Dictionary<string, Dictionary<string, byte[]>>();
        }
        else if(_fat32)
            _eaCache = new Dictionary<string, Dictionary<string, byte[]>>();

        // Check OS/2 .LONGNAME
        if(_eaCache != null                             &&
           _namespace is Namespace.Os2 or Namespace.Ecs &&
           !_fat32)
        {
            var rootFilesWithEas = _rootDirectoryCache.Where(t => t.Value.Dirent.ea_handle != 0).ToList();

            foreach(KeyValuePair<string, CompleteDirectoryEntry> fileWithEa in rootFilesWithEas)
            {
                Dictionary<string, byte[]> eas = GetEas(fileWithEa.Value.Dirent.ea_handle);

                if(eas is null)
                    continue;

                if(!eas.TryGetValue("com.microsoft.os2.longname", out byte[] longnameEa))
                    continue;

                if(BitConverter.ToUInt16(longnameEa, 0) != EAT_ASCII)
                    continue;

                var longnameSize = BitConverter.ToUInt16(longnameEa, 2);

                if(longnameSize + 4 > longnameEa.Length)
                    continue;

                var longnameBytes = new byte[longnameSize];

                Array.Copy(longnameEa, 4, longnameBytes, 0, longnameSize);

                string longname = StringHandlers.CToString(longnameBytes, Encoding);

                if(string.IsNullOrWhiteSpace(longname))
                    continue;

                // Forward slash is allowed in .LONGNAME, so change it to visually similar division slash
                longname = longname.Replace('/', '\u2215');

                fileWithEa.Value.Longname = longname;
                _rootDirectoryCache.Remove(fileWithEa.Key);
                _rootDirectoryCache[fileWithEa.Value.ToString()] = fileWithEa.Value;
            }
        }

        // Check FAT32.IFS EAs
        if(_fat32 || _debug)
        {
            var fat32EaSidecars = _rootDirectoryCache.Where(t => t.Key.EndsWith(FAT32_EA_TAIL, true, _cultureInfo)).
                                                      ToList();

            foreach(KeyValuePair<string, CompleteDirectoryEntry> sidecar in fat32EaSidecars)
            {
                // No real file this sidecar accompanies
                if(!_rootDirectoryCache.TryGetValue(sidecar.Key.Substring(0, sidecar.Key.Length - FAT32_EA_TAIL.Length),
                                                    out CompleteDirectoryEntry fileWithEa))
                    continue;

                // If not in debug mode we will consider the lack of EA bitflags to mean the EAs are corrupted or not real
                if(!_debug)
                    if(!fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.NormalEaOld) &&
                       !fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.CriticalEa)  &&
                       !fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.NormalEa)    &&
                       !fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.CriticalEa))
                        continue;

                fileWithEa.Fat32Ea = sidecar.Value.Dirent;

                if(!_debug)
                    _rootDirectoryCache.Remove(sidecar.Key);
            }
        }

        _mounted = true;

        if(string.IsNullOrWhiteSpace(XmlFsType.VolumeName))
            XmlFsType.VolumeName = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted)
            return ErrorNumber.AccessDenied;

        _mounted    = false;
        _fatEntries = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber StatFs(out FileSystemInfo stat)
    {
        stat = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        stat = _statfs.ShallowCopy();

        return ErrorNumber.NoError;
    }
}