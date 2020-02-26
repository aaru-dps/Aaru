// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using Schemas;
using FileSystemInfo = DiscImageChef.CommonTypes.Structs.FileSystemInfo;
using Marshal = DiscImageChef.Helpers.Marshal;

namespace DiscImageChef.Filesystems.FAT
{
    public partial class FAT
    {
        uint fatEntriesPerSector;

        IMediaImage image;

        /// <summary>
        ///     Mounts an Apple Lisa filesystem
        /// </summary>
        public Errno Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                           Dictionary<string, string> options,     string    @namespace)
        {
            XmlFsType = new FileSystemType();
            if(options == null) options = GetDefaultOptions();
            if(options.TryGetValue("debug", out string debugString)) bool.TryParse(debugString, out debug);

            // Default namespace
            if(@namespace is null) @namespace = "ecs";

            switch(@namespace.ToLowerInvariant())
            {
                case "dos":
                    this.@namespace = Namespace.Dos;
                    break;
                case "nt":
                    this.@namespace = Namespace.Nt;
                    break;
                case "os2":
                    this.@namespace = Namespace.Os2;
                    break;
                case "ecs":
                    this.@namespace = Namespace.Ecs;
                    break;
                case "lfn":
                    this.@namespace = Namespace.Lfn;
                    break;
                case "human":
                    this.@namespace = Namespace.Human;
                    break;
                default: return Errno.InvalidArgument;
            }

            DicConsole.DebugWriteLine("FAT plugin", "Reading BPB");

            uint sectorsPerBpb = imagePlugin.Info.SectorSize < 512 ? 512 / imagePlugin.Info.SectorSize : 1;

            byte[] bpbSector = imagePlugin.ReadSectors(0 + partition.Start, sectorsPerBpb);

            BpbKind bpbKind = DetectBpbKind(bpbSector, imagePlugin, partition,
                                            out BiosParameterBlockEbpb fakeBpb,
                                            out HumanParameterBlock humanBpb, out AtariParameterBlock atariBpb,
                                            out byte minBootNearJump,         out bool andosOemCorrect,
                                            out bool bootable);

            fat12              = false;
            fat16              = false;
            fat32              = false;
            useFirstFat        = true;
            XmlFsType.Bootable = bootable;

            statfs = new FileSystemInfo
            {
                Blocks         = XmlFsType.Clusters,
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

            switch(bpbKind)
            {
                case BpbKind.DecRainbow:
                case BpbKind.Hardcoded:
                case BpbKind.Msx:
                case BpbKind.Apricot:
                    fat12 = true;
                    break;
                case BpbKind.ShortFat32:
                case BpbKind.LongFat32:
                {
                    fat32 = true;
                    Fat32ParameterBlock fat32Bpb =
                        Marshal.ByteArrayToStructureLittleEndian<Fat32ParameterBlock>(bpbSector);
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

                    if(fat32Bpb.oem_name != null && (fat32Bpb.oem_name[5] != 0x49 || fat32Bpb.oem_name[6] != 0x48 ||
                                                     fat32Bpb.oem_name[7] != 0x43))
                        XmlFsType.SystemIdentifier = StringHandlers.CToString(fat32Bpb.oem_name);

                    sectorsPerCluster     = fat32Bpb.spc;
                    XmlFsType.ClusterSize = (uint)(fat32Bpb.bps * fat32Bpb.spc);
                    reservedSectors       = fat32Bpb.rsectors;
                    if(fat32Bpb.big_sectors == 0 && fat32Bpb.signature == 0x28)
                        XmlFsType.Clusters  = shortFat32Bpb.huge_sectors / shortFat32Bpb.spc;
                    else XmlFsType.Clusters = fat32Bpb.big_sectors       / fat32Bpb.spc;

                    sectorsPerFat          = fat32Bpb.big_spfat;
                    XmlFsType.VolumeSerial = $"{fat32Bpb.serial_no:X8}";
                    statfs.Id              = new FileSystemId {IsInt = true, Serial32 = fat32Bpb.serial_no};

                    if((fat32Bpb.flags & 0xF8) == 0x00)
                        if((fat32Bpb.flags & 0x01) == 0x01)
                            XmlFsType.Dirty = true;

                    if((fat32Bpb.mirror_flags & 0x80) == 0x80) useFirstFat = (fat32Bpb.mirror_flags & 0xF) != 1;

                    if(fat32Bpb.signature == 0x29)
                        XmlFsType.VolumeName = Encoding.ASCII.GetString(fat32Bpb.volume_label);

                    // Check that jumps to a correct boot code position and has boot signature set.
                    // This will mean that the volume will boot, even if just to say "this is not bootable change disk"......
                    XmlFsType.Bootable =
                        fat32Bpb.jump[0] == 0xEB && fat32Bpb.jump[1] >= minBootNearJump && fat32Bpb.jump[1] < 0x80 ||
                        fat32Bpb.jump[0]                        == 0xE9            && fat32Bpb.jump.Length >= 3 &&
                        BitConverter.ToUInt16(fat32Bpb.jump, 1) >= minBootNearJump &&
                        BitConverter.ToUInt16(fat32Bpb.jump, 1) <= 0x1FC;

                    sectorsPerRealSector =  fat32Bpb.bps / imagePlugin.Info.SectorSize;
                    sectorsPerCluster    *= sectorsPerRealSector;

                    // First root directory sector
                    firstClusterSector =
                        (ulong)(fat32Bpb.big_spfat * fat32Bpb.fats_no + fat32Bpb.rsectors) * sectorsPerRealSector -
                        2                                                                  * sectorsPerCluster;

                    if(fat32Bpb.fsinfo_sector + partition.Start <= partition.End)
                    {
                        byte[] fsinfoSector = imagePlugin.ReadSector(fat32Bpb.fsinfo_sector + partition.Start);
                        FsInfoSector fsInfo =
                            Marshal.ByteArrayToStructureLittleEndian<FsInfoSector>(fsinfoSector);

                        if(fsInfo.signature1 == FSINFO_SIGNATURE1 && fsInfo.signature2 == FSINFO_SIGNATURE2 &&
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
                    ushort sum                                       = 0;
                    for(int i = 0; i < bpbSector.Length; i += 2) sum += BigEndianBitConverter.ToUInt16(bpbSector, i);

                    // TODO: Check this
                    if(sum == 0x1234) XmlFsType.Bootable = true;

                    break;
                }

                case BpbKind.Human:
                    // If not debug set Human68k namespace and ShiftJIS codepage as defaults
                    if(!debug)
                    {
                        this.@namespace = Namespace.Human;
                        encoding        = Encoding.GetEncoding("shift_jis");
                    }

                    XmlFsType.Bootable = true;
                    break;
            }

            Encoding = encoding ?? (bpbKind == BpbKind.Human
                                        ? Encoding.GetEncoding("shift_jis")
                                        : Encoding.GetEncoding("IBM437"));

            ulong firstRootSector = 0;

            if(!fat32)
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

                    if(fakeBpb.spc == 0) fakeBpb.spc = 1;
                }

                // This assumes no sane implementation will violate cluster size rules
                // However nothing prevents this to happen
                // If first file on disk uses only one cluster there is absolutely no way to differentiate between FAT12 and FAT16,
                // so let's hope implementations use common sense?
                if(!fat12 && !fat16)
                {
                    ulong clusters;

                    if(fakeBpb.sectors == 0)
                        clusters  = fakeBpb.spc == 0 ? fakeBpb.big_sectors : fakeBpb.big_sectors / fakeBpb.spc;
                    else clusters = fakeBpb.spc == 0 ? fakeBpb.sectors : (ulong)fakeBpb.sectors  / fakeBpb.spc;

                    if(clusters < 4089) fat12 = true;
                    else fat16                = true;
                }

                if(fat12) XmlFsType.Type      = "FAT12";
                else if(fat16) XmlFsType.Type = "FAT16";

                if(bpbKind == BpbKind.Atari)
                {
                    if(atariBpb.serial_no[0] != 0x49 || atariBpb.serial_no[1] != 0x48 || atariBpb.serial_no[2] != 0x43)
                    {
                        XmlFsType.VolumeSerial =
                            $"{atariBpb.serial_no[0]:X2}{atariBpb.serial_no[1]:X2}{atariBpb.serial_no[2]:X2}";
                        statfs.Id = new FileSystemId
                        {
                            IsInt = true,
                            Serial32 = (uint)((atariBpb.serial_no[0] << 16) + (atariBpb.serial_no[1] << 8) +
                                              atariBpb.serial_no[2])
                        };
                    }

                    XmlFsType.SystemIdentifier = StringHandlers.CToString(atariBpb.oem_name);
                    if(string.IsNullOrEmpty(XmlFsType.SystemIdentifier)) XmlFsType.SystemIdentifier = null;
                }
                else if(fakeBpb.oem_name != null)
                {
                    if(fakeBpb.oem_name[5] != 0x49 || fakeBpb.oem_name[6] != 0x48 || fakeBpb.oem_name[7] != 0x43)
                    {
                        // Later versions of Windows create a DOS 3 BPB without OEM name on 8 sectors/track floppies
                        // OEM ID should be ASCII, otherwise ignore it
                        if(fakeBpb.oem_name[0] >= 0x20 && fakeBpb.oem_name[0] <= 0x7F && fakeBpb.oem_name[1] >= 0x20 &&
                           fakeBpb.oem_name[1] <= 0x7F && fakeBpb.oem_name[2] >= 0x20 && fakeBpb.oem_name[2] <= 0x7F &&
                           fakeBpb.oem_name[3] >= 0x20 && fakeBpb.oem_name[3] <= 0x7F && fakeBpb.oem_name[4] >= 0x20 &&
                           fakeBpb.oem_name[4] <= 0x7F && fakeBpb.oem_name[5] >= 0x20 && fakeBpb.oem_name[5] <= 0x7F &&
                           fakeBpb.oem_name[6] >= 0x20 && fakeBpb.oem_name[6] <= 0x7F && fakeBpb.oem_name[7] >= 0x20 &&
                           fakeBpb.oem_name[7] <= 0x7F)
                            XmlFsType.SystemIdentifier = StringHandlers.CToString(fakeBpb.oem_name);
                        else if(fakeBpb.oem_name[0] < 0x20  && fakeBpb.oem_name[1] >= 0x20 &&
                                fakeBpb.oem_name[1] <= 0x7F && fakeBpb.oem_name[2] >= 0x20 &&
                                fakeBpb.oem_name[2] <= 0x7F && fakeBpb.oem_name[3] >= 0x20 &&
                                fakeBpb.oem_name[3] <= 0x7F && fakeBpb.oem_name[4] >= 0x20 &&
                                fakeBpb.oem_name[4] <= 0x7F && fakeBpb.oem_name[5] >= 0x20 &&
                                fakeBpb.oem_name[5] <= 0x7F && fakeBpb.oem_name[6] >= 0x20 &&
                                fakeBpb.oem_name[6] <= 0x7F && fakeBpb.oem_name[7] >= 0x20 &&
                                fakeBpb.oem_name[7] <= 0x7F)
                            XmlFsType.SystemIdentifier = StringHandlers.CToString(fakeBpb.oem_name, Encoding, start: 1);
                    }

                    if(fakeBpb.signature == 0x28 || fakeBpb.signature == 0x29)
                    {
                        XmlFsType.VolumeSerial = $"{fakeBpb.serial_no:X8}";
                        statfs.Id              = new FileSystemId {IsInt = true, Serial32 = fakeBpb.serial_no};
                    }
                }

                if(bpbKind != BpbKind.Human)
                    if(fakeBpb.sectors == 0)
                        XmlFsType.Clusters = fakeBpb.spc == 0 ? fakeBpb.big_sectors : fakeBpb.big_sectors / fakeBpb.spc;
                    else
                        XmlFsType.Clusters =
                            (ulong)(fakeBpb.spc == 0 ? fakeBpb.sectors : fakeBpb.sectors / fakeBpb.spc);
                else XmlFsType.Clusters = humanBpb.clusters == 0 ? humanBpb.big_clusters : humanBpb.clusters;

                sectorsPerCluster     = fakeBpb.spc;
                XmlFsType.ClusterSize = (uint)(fakeBpb.bps * fakeBpb.spc);
                reservedSectors       = fakeBpb.rsectors;
                sectorsPerFat         = fakeBpb.spfat;

                if(fakeBpb.signature == 0x28 || fakeBpb.signature == 0x29 || andosOemCorrect)
                {
                    if((fakeBpb.flags & 0xF8) == 0x00)
                        if((fakeBpb.flags & 0x01) == 0x01)
                            XmlFsType.Dirty = true;

                    if(fakeBpb.signature == 0x29 || andosOemCorrect)
                        XmlFsType.VolumeName = Encoding.ASCII.GetString(fakeBpb.volume_label);
                }

                // Workaround that PCExchange jumps into "FAT16   "...
                if(XmlFsType.SystemIdentifier == "PCX 2.0 ") fakeBpb.jump[1] += 8;

                // Check that jumps to a correct boot code position and has boot signature set.
                // This will mean that the volume will boot, even if just to say "this is not bootable change disk"......
                if(XmlFsType.Bootable == false && fakeBpb.jump != null)
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
                sectorsPerCluster    *= sectorsPerRealSector;
            }

            firstClusterSector += partition.Start;

            image = imagePlugin;

            if(fat32) fatEntriesPerSector      = imagePlugin.Info.SectorSize     / 4;
            else if(fat16) fatEntriesPerSector = imagePlugin.Info.SectorSize     / 2;
            else fatEntriesPerSector           = imagePlugin.Info.SectorSize * 2 / 3;
            fatFirstSector = partition.Start + reservedSectors * sectorsPerRealSector;

            rootDirectoryCache = new Dictionary<string, CompleteDirectoryEntry>();
            byte[] rootDirectory = null;

            if(!fat32)
            {
                firstClusterSector = firstRootSector + sectorsForRootDirectory - sectorsPerCluster * 2;
                rootDirectory      = imagePlugin.ReadSectors(firstRootSector, sectorsForRootDirectory);

                if(bpbKind == BpbKind.DecRainbow)
                {
                    MemoryStream rootMs = new MemoryStream();
                    foreach(byte[] tmp in from ulong rootSector in new[] {0x17, 0x19, 0x1B, 0x1D, 0x1E, 0x20}
                                          select imagePlugin.ReadSector(rootSector)) rootMs.Write(tmp, 0, tmp.Length);

                    rootDirectory = rootMs.ToArray();
                }
            }
            else
            {
                if(rootDirectoryCluster == 0) return Errno.InvalidArgument;

                MemoryStream rootMs                = new MemoryStream();
                uint[]       rootDirectoryClusters = GetClusters(rootDirectoryCluster);

                foreach(uint cluster in rootDirectoryClusters)
                {
                    byte[] buffer =
                        imagePlugin.ReadSectors(firstClusterSector + cluster * sectorsPerCluster, sectorsPerCluster);

                    rootMs.Write(buffer, 0, buffer.Length);
                }

                rootDirectory = rootMs.ToArray();

                // OS/2 FAT32.IFS uses LFN instead of .LONGNAME
                if(this.@namespace == Namespace.Os2) this.@namespace = Namespace.Os2;
            }

            if(rootDirectory is null) return Errno.InvalidArgument;

            byte[] lastLfnName     = null;
            byte   lastLfnChecksum = 0;

            for(int i = 0; i < rootDirectory.Length; i += Marshal.SizeOf<DirectoryEntry>())
            {
                DirectoryEntry entry =
                    Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry>(rootDirectory, i,
                                                                             Marshal.SizeOf<DirectoryEntry>());

                if(entry.filename[0] == DIRENT_FINISHED) break;

                if(entry.attributes.HasFlag(FatAttributes.LFN))
                {
                    if(this.@namespace != Namespace.Lfn && this.@namespace != Namespace.Ecs) continue;

                    LfnEntry lfnEntry =
                        Marshal.ByteArrayToStructureLittleEndian<LfnEntry>(rootDirectory, i,
                                                                           Marshal.SizeOf<LfnEntry>());

                    int lfnSequence = lfnEntry.sequence & LFN_MASK;

                    if((lfnEntry.sequence & LFN_ERASED) > 0) continue;

                    if((lfnEntry.sequence & LFN_LAST) > 0)
                    {
                        lastLfnName     = new byte[lfnSequence * 26];
                        lastLfnChecksum = lfnEntry.checksum;
                    }

                    if(lastLfnName is null) continue;
                    if(lfnEntry.checksum != lastLfnChecksum) continue;

                    lfnSequence--;

                    Array.Copy(lfnEntry.name1, 0, lastLfnName, lfnSequence * 26,      10);
                    Array.Copy(lfnEntry.name2, 0, lastLfnName, lfnSequence * 26 + 10, 12);
                    Array.Copy(lfnEntry.name3, 0, lastLfnName, lfnSequence * 26 + 22, 4);

                    continue;
                }

                // Not a correct entry
                if(entry.filename[0] < DIRENT_MIN && entry.filename[0] != DIRENT_E5) continue;

                // Self
                if(Encoding.GetString(entry.filename).TrimEnd() == ".") continue;

                // Parent
                if(Encoding.GetString(entry.filename).TrimEnd() == "..") continue;

                // Deleted
                if(entry.filename[0] == DIRENT_DELETED) continue;

                string filename;

                if(entry.attributes.HasFlag(FatAttributes.VolumeLabel))
                {
                    byte[] fullname = new byte[11];
                    Array.Copy(entry.filename,  0, fullname, 0, 8);
                    Array.Copy(entry.extension, 0, fullname, 8, 3);
                    string volname = Encoding.GetString(fullname).Trim();
                    if(!string.IsNullOrEmpty(volname))
                        XmlFsType.VolumeName =
                            entry.caseinfo.HasFlag(CaseInfo.AllLowerCase) && this.@namespace == Namespace.Nt
                                ? volname.ToLower()
                                : volname;

                    if(entry.ctime > 0 && entry.cdate > 0)
                    {
                        XmlFsType.CreationDate = DateHandlers.DosToDateTime(entry.cdate, entry.ctime);
                        if(entry.ctime_ms > 0)
                            XmlFsType.CreationDate = XmlFsType.CreationDate.AddMilliseconds(entry.ctime_ms * 10);
                        XmlFsType.CreationDateSpecified = true;
                    }

                    if(entry.mtime > 0 && entry.mdate > 0)
                    {
                        XmlFsType.ModificationDate          = DateHandlers.DosToDateTime(entry.mdate, entry.mtime);
                        XmlFsType.ModificationDateSpecified = true;
                    }

                    continue;
                }

                CompleteDirectoryEntry completeEntry = new CompleteDirectoryEntry {Dirent = entry};

                if((this.@namespace == Namespace.Lfn || this.@namespace == Namespace.Ecs) && lastLfnName != null)
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

                if(entry.filename[0] == DIRENT_E5) entry.filename[0] = DIRENT_DELETED;

                string name      = Encoding.GetString(entry.filename).TrimEnd();
                string extension = Encoding.GetString(entry.extension).TrimEnd();

                if(this.@namespace == Namespace.Nt)
                {
                    if(entry.caseinfo.HasFlag(CaseInfo.LowerCaseExtension))
                        extension = extension.ToLower(CultureInfo.CurrentCulture);

                    if(entry.caseinfo.HasFlag(CaseInfo.LowerCaseBasename))
                        name = name.ToLower(CultureInfo.CurrentCulture);
                }

                if(extension != "") filename = name + "." + extension;
                else filename                = name;

                completeEntry.Shortname = filename;

                if(this.@namespace == Namespace.Human)
                {
                    HumanDirectoryEntry humanEntry =
                        Marshal.ByteArrayToStructureLittleEndian<HumanDirectoryEntry>(rootDirectory, i,
                                                                                      Marshal
                                                                                         .SizeOf<HumanDirectoryEntry
                                                                                          >());

                    completeEntry.HumanDirent = humanEntry;

                    name      = StringHandlers.CToString(humanEntry.name1, Encoding).TrimEnd();
                    extension = StringHandlers.CToString(humanEntry.extension, Encoding).TrimEnd();
                    string name2 = StringHandlers.CToString(humanEntry.name2, Encoding).TrimEnd();

                    if(extension != "") filename = name + name2 + "." + extension;
                    else filename                = name               + name2;

                    completeEntry.HumanName = filename;
                }

                if(!fat32 && filename == "EA DATA. SF")
                {
                    eaDirEntry      = entry;
                    lastLfnName     = null;
                    lastLfnChecksum = 0;

                    if(debug) rootDirectoryCache[completeEntry.ToString()] = completeEntry;

                    continue;
                }

                rootDirectoryCache[completeEntry.ToString()] = completeEntry;
                lastLfnName                                  = null;
                lastLfnChecksum                              = 0;
            }

            XmlFsType.VolumeName = XmlFsType.VolumeName?.Trim();
            statfs.Blocks        = XmlFsType.Clusters;

            switch(bpbKind)
            {
                case BpbKind.Hardcoded:
                    statfs.Type = $"Microsoft FAT{(fat16 ? "16" : "12")}";
                    break;
                case BpbKind.Atari:
                    statfs.Type = $"Atari FAT{(fat16 ? "16" : "12")}";
                    break;
                case BpbKind.Msx:
                    statfs.Type = $"MSX FAT{(fat16 ? "16" : "12")}";
                    break;
                case BpbKind.Dos2:
                case BpbKind.Dos3:
                case BpbKind.Dos32:
                case BpbKind.Dos33:
                case BpbKind.ShortExtended:
                case BpbKind.Extended:
                    statfs.Type = $"Microsoft FAT{(fat16 ? "16" : "12")}";
                    break;
                case BpbKind.ShortFat32:
                case BpbKind.LongFat32:
                    statfs.Type = XmlFsType.Type == "FAT+" ? "FAT+" : "Microsoft FAT32";
                    break;
                case BpbKind.Andos:
                    statfs.Type = $"ANDOS FAT{(fat16 ? "16" : "12")}";
                    break;
                case BpbKind.Apricot:
                    statfs.Type = $"Apricot FAT{(fat16 ? "16" : "12")}";
                    break;
                case BpbKind.DecRainbow:
                    statfs.Type = $"DEC FAT{(fat16 ? "16" : "12")}";
                    break;
                case BpbKind.Human:
                    statfs.Type = $"Human68k FAT{(fat16 ? "16" : "12")}";
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            bytesPerCluster = sectorsPerCluster * imagePlugin.Info.SectorSize;

            if(fat12)
            {
                byte[] fatBytes =
                    imagePlugin.ReadSectors(fatFirstSector + (useFirstFat ? 0 : sectorsPerFat), sectorsPerFat);

                fatEntries = new ushort[statfs.Blocks];

                int pos = 0;
                for(int i = 0; i + 3 < fatBytes.Length && pos < fatEntries.Length; i += 3)
                {
                    fatEntries[pos++] = (ushort)(((fatBytes[i + 1] & 0xF)  << 8) + fatBytes[i + 0]);
                    fatEntries[pos++] = (ushort)(((fatBytes[i + 1] & 0xF0) >> 4) + (fatBytes[i + 2] << 4));
                }
            }
            else if(fat16)
            {
                DicConsole.DebugWriteLine("FAT plugin", "Reading FAT16");

                byte[] fatBytes =
                    imagePlugin.ReadSectors(fatFirstSector + (useFirstFat ? 0 : sectorsPerFat), sectorsPerFat);

                DicConsole.DebugWriteLine("FAT plugin", "Casting FAT");
                fatEntries = MemoryMarshal.Cast<byte, ushort>(fatBytes).ToArray();
            }

            // TODO: Check how this affects international filenames
            cultureInfo    = new CultureInfo("en-US", false);
            directoryCache = new Dictionary<string, Dictionary<string, CompleteDirectoryEntry>>();

            // Check it is really an OS/2 EA file
            if(eaDirEntry.start_cluster != 0)
            {
                CacheEaData();
                ushort eamagic = BitConverter.ToUInt16(cachedEaData, 0);

                if(eamagic != EADATA_MAGIC)
                {
                    eaDirEntry   = new DirectoryEntry();
                    cachedEaData = null;
                }
                else eaCache = new Dictionary<string, Dictionary<string, byte[]>>();
            }
            else if(fat32) eaCache = new Dictionary<string, Dictionary<string, byte[]>>();

            // Check OS/2 .LONGNAME
            if(eaCache != null && (this.@namespace == Namespace.Os2 || this.@namespace == Namespace.Ecs))
            {
                List<KeyValuePair<string, CompleteDirectoryEntry>> rootFilesWithEas =
                    rootDirectoryCache.Where(t => t.Value.Dirent.ea_handle != 0).ToList();

                foreach(KeyValuePair<string, CompleteDirectoryEntry> fileWithEa in rootFilesWithEas)
                {
                    Dictionary<string, byte[]> eas = GetEas(fileWithEa.Value.Dirent.ea_handle);

                    if(eas is null) continue;

                    if(!eas.TryGetValue("com.microsoft.os2.longname", out byte[] longnameEa)) continue;

                    if(BitConverter.ToUInt16(longnameEa, 0) != EAT_ASCII) continue;

                    ushort longnameSize = BitConverter.ToUInt16(longnameEa, 2);

                    if(longnameSize + 4 > longnameEa.Length) continue;

                    byte[] longnameBytes = new byte[longnameSize];

                    Array.Copy(longnameEa, 4, longnameBytes, 0, longnameSize);

                    string longname = StringHandlers.CToString(longnameBytes, Encoding);

                    if(string.IsNullOrWhiteSpace(longname)) continue;

                    // Forward slash is allowed in .LONGNAME, so change it to visually similar division slash
                    longname = longname.Replace('/', '\u2215');

                    fileWithEa.Value.Longname = longname;
                    rootDirectoryCache.Remove(fileWithEa.Key);
                    rootDirectoryCache[fileWithEa.Value.ToString()] = fileWithEa.Value;
                }
            }

            // Check FAT32.IFS EAs
            if(fat32 || debug)
            {
                List<KeyValuePair<string, CompleteDirectoryEntry>> fat32EaSidecars = rootDirectoryCache
                                                                                    .Where(t =>
                                                                                               t.Key
                                                                                                .EndsWith(FAT32_EA_TAIL,
                                                                                                          true,
                                                                                                          cultureInfo))
                                                                                    .ToList();

                foreach(KeyValuePair<string, CompleteDirectoryEntry> sidecar in fat32EaSidecars)
                {
                    // No real file this sidecar accompanies
                    if(!rootDirectoryCache
                          .TryGetValue(sidecar.Key.Substring(0, sidecar.Key.Length - FAT32_EA_TAIL.Length),
                                       out CompleteDirectoryEntry fileWithEa)) continue;

                    // If not in debug mode we will consider the lack of EA bitflags to mean the EAs are corrupted or not real
                    if(!debug)
                        if(!fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.NormalEaOld) &&
                           !fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.CriticalEa)  &&
                           !fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.NormalEa)    &&
                           !fileWithEa.Dirent.caseinfo.HasFlag(CaseInfo.CriticalEa))
                            continue;

                    fileWithEa.Fat32Ea = sidecar.Value.Dirent;

                    if(!debug) rootDirectoryCache.Remove(sidecar.Key);
                }
            }

            mounted = true;
            return Errno.NoError;
        }

        /// <summary>
        ///     Umounts this Lisa filesystem
        /// </summary>
        public Errno Unmount()
        {
            if(!mounted) return Errno.AccessDenied;

            mounted    = false;
            fatEntries = null;
            return Errno.NoError;
        }

        /// <summary>
        ///     Gets information about the mounted volume.
        /// </summary>
        /// <param name="stat">Information about the mounted volume.</param>
        public Errno StatFs(out FileSystemInfo stat)
        {
            stat = null;
            if(!mounted) return Errno.AccessDenied;

            stat = statfs.ShallowCopy();
            return Errno.NoError;
        }
    }
}