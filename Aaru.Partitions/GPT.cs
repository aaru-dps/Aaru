// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : GPT.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages GUID Partition Table.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of the GUID Partition Table</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class GuidPartitionTable : IPartition
{
    const ulong  GPT_MAGIC     = 0x5452415020494645;
    const uint   GPT_REVISION1 = 0x00010000;
    const string MODULE_NAME   = "GUID Partition Table (GPT) Plugin";

#region IPartition Members

    /// <inheritdoc />
    public string Name => Localization.GuidPartitionTable_Name;

    /// <inheritdoc />
    public Guid Id => new("CBC9D281-C1D0-44E8-9038-4D66FD2678AB");

    /// <inheritdoc />
    public string Author => Authors.NATALIA_PORTILLO;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        partitions = [];

        if(sectorOffset + 2 >= imagePlugin.Info.Sectors) return false;

        ErrorNumber errno = imagePlugin.ReadSector(1 + sectorOffset, out byte[] hdrBytes);

        if(errno != ErrorNumber.NoError) return false;

        Header hdr;

        var signature  = BitConverter.ToUInt64(hdrBytes, 0);
        var misaligned = false;

        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.signature = 0x{0:X16}", signature);

        if(signature != GPT_MAGIC)
        {
            if(imagePlugin.Info.MetadataMediaType == MetadataMediaType.OpticalDisc)
            {
                errno = imagePlugin.ReadSector(sectorOffset, out hdrBytes);

                if(errno != ErrorNumber.NoError) return false;

                signature = BitConverter.ToUInt64(hdrBytes, 512);
                AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.signature @ 0x200 = 0x{0:X16}", signature);

                if(signature == GPT_MAGIC)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_unaligned_signature, signature);
                    var real = new byte[512];
                    Array.Copy(hdrBytes, 512, real, 0, 512);
                    hdrBytes   = real;
                    misaligned = true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        try
        {
            hdr = Marshal.ByteArrayToStructureLittleEndian<Header>(hdrBytes);
        }
        catch
        {
            return false;
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.revision = 0x{0:X8}",   hdr.revision);
        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.headerSize = {0}",      hdr.headerSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.headerCrc = 0x{0:X8}",  hdr.headerCrc);
        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.reserved = 0x{0:X8}",   hdr.reserved);
        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.myLBA = {0}",           hdr.myLBA);
        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.alternateLBA = {0}",    hdr.alternateLBA);
        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.firstUsableLBA = {0}",  hdr.firstUsableLBA);
        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.lastUsableLBA = {0}",   hdr.lastUsableLBA);
        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.diskGuid = {0}",        hdr.diskGuid);
        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.entryLBA = {0}",        hdr.entryLBA);
        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.entries = {0}",         hdr.entries);
        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.entriesSize = {0}",     hdr.entriesSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "hdr.entriesCrc = 0x{0:X8}", hdr.entriesCrc);

        if(hdr.signature != GPT_MAGIC) return false;

        if(hdr.myLBA != 1 + sectorOffset) return false;

        uint divisor, modulo, sectorSize;

        if(misaligned)
        {
            divisor    = 4;
            modulo     = (uint)(hdr.entryLBA % divisor);
            sectorSize = 512;
        }
        else
        {
            divisor    = 1;
            modulo     = 0;
            sectorSize = imagePlugin.Info.SectorSize;
        }

        uint totalEntriesSectors = hdr.entries * hdr.entriesSize / imagePlugin.Info.SectorSize;

        if(hdr.entries * hdr.entriesSize % imagePlugin.Info.SectorSize > 0) totalEntriesSectors++;

        errno = imagePlugin.ReadSectors(hdr.entryLBA / divisor, totalEntriesSectors + modulo, out byte[] temp);

        if(errno != ErrorNumber.NoError) return false;

        var entriesBytes = new byte[temp.Length - modulo * 512];
        Array.Copy(temp, modulo * 512, entriesBytes, 0, entriesBytes.Length);
        List<Entry> entries = [];

        for(var i = 0; i < hdr.entries; i++)
        {
            try
            {
                var entryBytes = new byte[hdr.entriesSize];
                Array.Copy(entriesBytes, hdr.entriesSize * i, entryBytes, 0, hdr.entriesSize);
                entries.Add(Marshal.ByteArrayToStructureLittleEndian<Entry>(entryBytes));
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch
            {
                // ignored
            }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
        }

        if(entries.Count == 0) return false;

        ulong pSeq = 0;

        foreach(Entry entry in entries.Where(entry => entry.partitionType != Guid.Empty &&
                                                      entry.partitionId   != Guid.Empty))
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.partitionType = {0}",    entry.partitionType);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.partitionId = {0}",      entry.partitionId);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.startLBA = {0}",         entry.startLBA);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.endLBA = {0}",           entry.endLBA);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.attributes = 0x{0:X16}", entry.attributes);
            AaruConsole.DebugWriteLine(MODULE_NAME, "entry.name = {0}",             entry.name);

            if(entry.startLBA / divisor > imagePlugin.Info.Sectors || entry.endLBA / divisor > imagePlugin.Info.Sectors)
                return false;

            var part = new Partition
            {
                Description = $"ID: {entry.partitionId}",
                Size        = (entry.endLBA - entry.startLBA + 1) * sectorSize,
                Name        = entry.name,
                Length      = (entry.endLBA - entry.startLBA + 1) / divisor,
                Sequence    = pSeq++,
                Offset      = entry.startLBA * sectorSize,
                Start       = entry.startLBA / divisor,
                Type        = GetGuidTypeName(entry.partitionType),
                Scheme      = Name
            };

            AaruConsole.DebugWriteLine(MODULE_NAME, "part.PartitionType = {0}", part.Type);
            partitions.Add(part);
        }

        return true;
    }

#endregion

    static string GetGuidTypeName(Guid type)
    {
        string strType = type.ToString().ToUpperInvariant();

        return strType switch
               {
                   "024DEE41-33E7-11D3-9D69-0008C781F39F" => Localization.MBR_scheme,
                   "C12A7328-F81F-11D2-BA4B-00A0C93EC93B" => Localization.EFI_System,
                   "21686148-6449-6E6F-744E-656564454649" => Localization.BIOS_Boot,
                   "D3BFE2DE-3DAF-11DF-BA40-E3A556D89593" => Localization.Intel_Fast_Flash_iFFS,
                   "F4019732-066E-4E12-8273-346C5641494F" => Localization.Sony_boot,
                   "BFBFAFE7-A34F-448A-9A5B-6213EB736C22" => Localization.Lenovo_boot,
                   "E3C9E316-0B5C-4DB8-817D-F92DF00215AE" => Localization.Microsoft_Reserved_MSR,
                   "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7" => Localization.Microsoft_Basic_data,
                   "5808C8AA-7E8F-42E0-85D2-E1E90434CFB3" => Localization.Logical_Disk_Manager_LDM_metadata,
                   "AF9B60A0-1431-4F62-BC68-3311714A69AD" => Localization.Logical_Disk_Manager_data,
                   "DE94BBA4-06D1-4D40-A16A-BFD50179D6AC" => Localization.Windows_Recovery_Environment,
                   "37AFFC90-EF7D-4E96-91C3-2D7AE055B174" => Localization.IBM_General_Parallel_File_System_GPFS,
                   "E75CAF8F-F680-4CEE-AFA3-B001E56EFC2D" => Localization.Windows_Storage_Spaces,
                   "75894C1E-3AEB-11D3-B7C1-7B03A0000000" => Localization.HP_UX_Data,
                   "E2A1E728-32E3-11D6-A682-7B03A0000000" => Localization.HP_UX_Service,
                   "0FC63DAF-8483-4772-8E79-3D69D8477DE4" => Localization.Linux_filesystem,
                   "A19D880F-05FC-4D3B-A006-743F0F84911E" => Localization.Linux_RAID,
                   "44479540-F297-41B2-9AF7-D131D5F0458A" => Localization.Linux_Root_x86,
                   "4F68BCE3-E8CD-4DB1-96E7-FBCAF984B709" => Localization.Linux_Root_x86_64,
                   "69DAD710-2CE4-4E3C-B16C-21A1D49ABED3" => Localization.Linux_Root_32_bit_ARM,
                   "B921B045-1DF0-41C3-AF44-4C6F280D3FAE" => Localization.Linux_Root_AArch64,
                   "0657FD6D-A4AB-43C4-84E5-0933C84B4F4F" => Localization.Linux_swap,
                   "E6D6D379-F507-44C2-A23C-238F2A3DF928" => Localization.Logical_Volume_Manager_LVM,
                   "933AC7E1-2EB4-4F13-B844-0E14E2AEF915" => Localization.Linux_home,
                   "3B8F8425-20E0-4F3B-907F-1A25A76F98E8" => Localization.Linux_srv,
                   "7FFEC5C9-2D00-49B7-8941-3EA10A5586B7" => Localization.Plain_dm_crypt,
                   "CA7D7CCB-63ED-4C53-861C-1742536059CC" => Localization.LUKS,
                   "8DA63339-0007-60C0-C436-083AC8230908" => Localization.Linux_Reserved,
                   "83BD6B9D-7F41-11DC-BE0B-001560B84F0F" => Localization.FreeBSD_Boot,
                   "516E7CB4-6ECF-11D6-8FF8-00022D09712B" => Localization.FreeBSD_Data,
                   "516E7CB5-6ECF-11D6-8FF8-00022D09712B" => Localization.FreeBSD_swap,
                   "516E7CB6-6ECF-11D6-8FF8-00022D09712B" => Localization.FreeBSD_UFS,
                   "516E7CB7-6ECF-11D6-8FF8-00022D09712B" => Localization.FreeBSD_UFS2,
                   "516E7CB8-6ECF-11D6-8FF8-00022D09712B" => Localization.FreeBSD_Vinum,
                   "516E7CBA-6ECF-11D6-8FF8-00022D09712B" => Localization.FreeBSD_ZFS,
                   "74BA7DD9-A689-11E1-BD04-00E081286ACF" => Localization.FreeBSD_nandfs,
                   "48465300-0000-11AA-AA11-00306543ECAC" => Localization.Apple_HFS,
                   "55465300-0000-11AA-AA11-00306543ECAC" => Localization.Apple_UFS,
                   "52414944-0000-11AA-AA11-00306543ECAC" => Localization.Apple_RAID,
                   "52414944-5F4F-11AA-AA11-00306543ECAC" => Localization.Apple_RAID_offline,
                   "426F6F74-0000-11AA-AA11-00306543ECAC" => Localization.Apple_Boot,
                   "4C616265-6C00-11AA-AA11-00306543ECAC" => Localization.Apple_Label,
                   "5265636F-7665-11AA-AA11-00306543ECAC" => Localization.Apple_TV_Recovery,
                   "53746F72-6167-11AA-AA11-00306543ECAC" => Localization.Apple_Core_Storage,
                   "6A82CB45-1DD2-11B2-99A6-080020736631" => Localization.Solaris_boot,
                   "6A85CF4D-1DD2-11B2-99A6-080020736631" => Localization.Solaris_Root,
                   "6A87C46F-1DD2-11B2-99A6-080020736631" => Localization.Solaris_Swap,
                   "6A8B642B-1DD2-11B2-99A6-080020736631" => Localization.Solaris_Backup,
                   "6A898CC3-1DD2-11B2-99A6-080020736631" => Localization.Solaris_usr_or_Apple_ZFS,
                   "6A8EF2E9-1DD2-11B2-99A6-080020736631" => Localization.Solaris_var,
                   "6A90BA39-1DD2-11B2-99A6-080020736631" => Localization.Solaris_home,
                   "6A9283A5-1DD2-11B2-99A6-080020736631" => Localization.Solaris_Alternate_sector,
                   "6A945A3B-1DD2-11B2-99A6-080020736631"
                    or "6A9630D1-1DD2-11B2-99A6-080020736631"
                    or "6A980767-1DD2-11B2-99A6-080020736631"
                    or "6A96237F-1DD2-11B2-99A6-080020736631"
                    or "6A8D2AC7-1DD2-11B2-99A6-080020736631" => Localization.Solaris_Reserved,
                   "49F48D32-B10E-11DC-B99B-0019D1879648" => Localization.NetBSD_Swap,
                   "49F48D5A-B10E-11DC-B99B-0019D1879648" => Localization.NetBSD_FFS,
                   "49F48D82-B10E-11DC-B99B-0019D1879648" => Localization.NetBSD_LFS,
                   "49F48DAA-B10E-11DC-B99B-0019D1879648" => Localization.NetBSD_RAID,
                   "2DB519C4-B10F-11DC-B99B-0019D1879648" => Localization.NetBSD_Concatenated,
                   "2DB519EC-B10F-11DC-B99B-0019D1879648" => Localization.NetBSD_Encrypted,
                   "FE3A2A5D-4F32-41A7-B725-ACCC3285A309" => Localization.ChromeOS_kernel,
                   "3CB8E202-3B7E-47DD-8A3C-7FF2A13CFCEC" => Localization.ChromeOS_rootfs,
                   "2E0A753D-9E48-43B0-8337-B15192CB1B5E" => Localization.ChromeOS_future_use,
                   "42465331-3BA3-10F1-802A-4861696B7521" => Localization.Haiku_BFS,
                   "85D5E45E-237C-11E1-B4B3-E89A8F7FC3A7" => Localization.MidnightBSD_Boot,
                   "85D5E45A-237C-11E1-B4B3-E89A8F7FC3A7" => Localization.MidnightBSD_Data,
                   "85D5E45B-237C-11E1-B4B3-E89A8F7FC3A7" => Localization.MidnightBSD_Swap,
                   "0394EF8B-237E-11E1-B4B3-E89A8F7FC3A7" => Localization.MidnightBSD_UFS,
                   "85D5E45C-237C-11E1-B4B3-E89A8F7FC3A7" => Localization.MidnightBSD_Vinum,
                   "85D5E45D-237C-11E1-B4B3-E89A8F7FC3A7" => Localization.MidnightBSD_ZFS,
                   "45B0969E-9B03-4F30-B4C6-B4B80CEFF106" => Localization.Ceph_Journal,
                   "45B0969E-9B03-4F30-B4C6-5EC00CEFF106" => Localization.Ceph_dm_crypt_Encrypted_Journal,
                   "4FBD7E29-9D25-41B8-AFD0-062C0CEFF05D" => Localization.Ceph_OSD,
                   "4FBD7E29-9D25-41B8-AFD0-5EC00CEFF05D" => Localization.Ceph_dm_crypt_OSD,
                   "89C57F98-2FE5-4DC0-89C1-F3AD0CEFF2BE" => Localization.Ceph_disk_in_creation,
                   "89C57F98-2FE5-4DC0-89C1-5EC00CEFF2BE" => Localization.Ceph_dm_crypt_disk_in_creation,
                   "824CC7A0-36A8-11E3-890A-952519AD3F61" => Localization.OpenBSD_Data,
                   "CEF5A9AD-73BC-4601-89F3-CDEEEEE321A1" => Localization.QNX_Power_safe_QNX6,
                   "C91818F9-8025-47AF-89D2-F030D7000C2C" => Localization.Plan_9,
                   "9D275380-40AD-11DB-BF97-000C2911D1B8" => Localization.VMware_vmkcore_coredump,
                   "AA31E02A-400F-11DB-9590-000C2911D1B8" => Localization.VMware_VMFS,
                   "9198EFFC-31C0-11DB-8F78-000C2911D1B8" => Localization.VMware_Reserved,
                   "7412F7D5-A156-4B13-81DC-867174929325" => Localization.ONIE_boot,
                   "D4E6E2CD-4469-46F3-B5CB-1BFF57AFC149" => Localization.ONIE_config,
                   "9E1A2D38-C612-4316-AA26-8B49521E5A8B" => Localization.PowerPC_PReP_boot,
                   "0311FC50-01CA-4725-AD77-9ADBB20ACE98" => Localization.Acronis_Secure_Zone,
                   "7C3457EF-0000-11AA-AA11-00306543ECAC" => Localization.Apple_File_System,
                   "9D087404-1CA5-11DC-8817-01301BB8A9F5" => Localization.DragonflyBSD_Label,
                   "9D58FDBD-1CA5-11DC-8817-01301BB8A9F5" => Localization.DragonflyBSD_Swap,
                   "9D94CE7C-1CA5-11DC-8817-01301BB8A9F5" => Localization.DragonflyBSD_UFS,
                   "9DD4478F-1CA5-11DC-8817-01301BB8A9F5" => Localization.DragonflyBSD_Vinum,
                   "DBD5211B-1CA5-11DC-8817-01301BB8A9F5" => Localization.DragonflyBSD_CCD,
                   "3D48CE54-1D16-11DC-8817-01301BB8A9F5" => Localization.DragonflyBSD_Label,
                   "BD215AB2-1D16-11DC-8696-01301BB8A9F5" => Localization.DragonflyBSD_Legacy,
                   "61DC63AC-6E38-11DC-8513-01301BB8A9F5" => Localization.DragonflyBSD_Hammer,
                   "5CBB9AD1-862D-11DC-A94D-01301BB8A9F5" => Localization.DragonflyBSD_Hammer2,
                   _                                      => ""
               };
    }

#region Nested type: Entry

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct Entry
    {
        public readonly Guid  partitionType;
        public readonly Guid  partitionId;
        public readonly ulong startLBA;
        public readonly ulong endLBA;
        public readonly ulong attributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        public readonly string name;
    }

#endregion

#region Nested type: Header

    [StructLayout(LayoutKind.Sequential)]
    struct Header
    {
        public readonly ulong signature;
        public readonly uint  revision;
        public readonly uint  headerSize;
        public readonly uint  headerCrc;
        public readonly uint  reserved;
        public readonly ulong myLBA;
        public readonly ulong alternateLBA;
        public readonly ulong firstUsableLBA;
        public readonly ulong lastUsableLBA;
        public readonly Guid  diskGuid;
        public readonly ulong entryLBA;
        public readonly uint  entries;
        public readonly uint  entriesSize;
        public readonly uint  entriesCrc;
    }

#endregion
}