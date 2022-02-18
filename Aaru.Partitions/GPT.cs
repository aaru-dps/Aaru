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
// Copyright © 2011-2022 Natalia Portillo
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

namespace Aaru.Partitions
{
    /// <inheritdoc />
    /// <summary>Implements decoding of the GUID Partition Table</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public sealed class GuidPartitionTable : IPartition
    {
        const ulong GPT_MAGIC     = 0x5452415020494645;
        const uint  GPT_REVISION1 = 0x00010000;

        /// <inheritdoc />
        public string Name => "GUID Partition Table";
        /// <inheritdoc />
        public Guid Id => new("CBC9D281-C1D0-44E8-9038-4D66FD2678AB");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            if(sectorOffset + 2 >= imagePlugin.Info.Sectors)
                return false;

            ErrorNumber errno = imagePlugin.ReadSector(1 + sectorOffset, out byte[] hdrBytes);

            if(errno != ErrorNumber.NoError)
                return false;

            Header hdr;

            ulong signature  = BitConverter.ToUInt64(hdrBytes, 0);
            bool  misaligned = false;

            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.signature = 0x{0:X16}", signature);

            if(signature != GPT_MAGIC)
                if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
                {
                    errno = imagePlugin.ReadSector(sectorOffset, out hdrBytes);

                    if(errno != ErrorNumber.NoError)
                        return false;

                    signature = BitConverter.ToUInt64(hdrBytes, 512);
                    AaruConsole.DebugWriteLine("GPT Plugin", "hdr.signature @ 0x200 = 0x{0:X16}", signature);

                    if(signature == GPT_MAGIC)
                    {
                        AaruConsole.DebugWriteLine("GPT Plugin", "Found unaligned signature", signature);
                        byte[] real = new byte[512];
                        Array.Copy(hdrBytes, 512, real, 0, 512);
                        hdrBytes   = real;
                        misaligned = true;
                    }
                    else
                        return false;
                }
                else
                    return false;

            try
            {
                hdr = Marshal.ByteArrayToStructureLittleEndian<Header>(hdrBytes);
            }
            catch
            {
                return false;
            }

            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.revision = 0x{0:X8}", hdr.revision);
            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.headerSize = {0}", hdr.headerSize);
            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.headerCrc = 0x{0:X8}", hdr.headerCrc);
            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.reserved = 0x{0:X8}", hdr.reserved);
            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.myLBA = {0}", hdr.myLBA);
            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.alternateLBA = {0}", hdr.alternateLBA);
            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.firstUsableLBA = {0}", hdr.firstUsableLBA);
            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.lastUsableLBA = {0}", hdr.lastUsableLBA);
            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.diskGuid = {0}", hdr.diskGuid);
            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.entryLBA = {0}", hdr.entryLBA);
            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.entries = {0}", hdr.entries);
            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.entriesSize = {0}", hdr.entriesSize);
            AaruConsole.DebugWriteLine("GPT Plugin", "hdr.entriesCrc = 0x{0:X8}", hdr.entriesCrc);

            if(hdr.signature != GPT_MAGIC)
                return false;

            if(hdr.myLBA != 1 + sectorOffset)
                return false;

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

            if(hdr.entries * hdr.entriesSize % imagePlugin.Info.SectorSize > 0)
                totalEntriesSectors++;

            errno = imagePlugin.ReadSectors(hdr.entryLBA / divisor, totalEntriesSectors + modulo, out byte[] temp);

            if(errno != ErrorNumber.NoError)
                return false;

            byte[] entriesBytes = new byte[temp.Length - (modulo * 512)];
            Array.Copy(temp, modulo * 512, entriesBytes, 0, entriesBytes.Length);
            List<Entry> entries = new();

            for(int i = 0; i < hdr.entries; i++)
            {
                try
                {
                    byte[] entryBytes = new byte[hdr.entriesSize];
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

            if(entries.Count == 0)
                return false;

            ulong pSeq = 0;

            foreach(Entry entry in entries.Where(entry => entry.partitionType != Guid.Empty &&
                                                          entry.partitionId   != Guid.Empty))
            {
                AaruConsole.DebugWriteLine("GPT Plugin", "entry.partitionType = {0}", entry.partitionType);
                AaruConsole.DebugWriteLine("GPT Plugin", "entry.partitionId = {0}", entry.partitionId);
                AaruConsole.DebugWriteLine("GPT Plugin", "entry.startLBA = {0}", entry.startLBA);
                AaruConsole.DebugWriteLine("GPT Plugin", "entry.endLBA = {0}", entry.endLBA);
                AaruConsole.DebugWriteLine("GPT Plugin", "entry.attributes = 0x{0:X16}", entry.attributes);
                AaruConsole.DebugWriteLine("GPT Plugin", "entry.name = {0}", entry.name);

                if(entry.startLBA / divisor > imagePlugin.Info.Sectors ||
                   entry.endLBA   / divisor > imagePlugin.Info.Sectors)
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

                AaruConsole.DebugWriteLine("GPT Plugin", "part.PartitionType = {0}", part.Type);
                partitions.Add(part);
            }

            return true;
        }

        static string GetGuidTypeName(Guid type)
        {
            string strType = type.ToString().ToUpperInvariant();

            switch(strType)
            {
                case "024DEE41-33E7-11D3-9D69-0008C781F39F": return "MBR scheme";
                case "C12A7328-F81F-11D2-BA4B-00A0C93EC93B": return "EFI System";
                case "21686148-6449-6E6F-744E-656564454649": return "BIOS Boot";
                case "D3BFE2DE-3DAF-11DF-BA40-E3A556D89593": return "Intel Fast Flash (iFFS)";
                case "F4019732-066E-4E12-8273-346C5641494F": return "Sony boot";
                case "BFBFAFE7-A34F-448A-9A5B-6213EB736C22": return "Lenovo boot";
                case "E3C9E316-0B5C-4DB8-817D-F92DF00215AE": return "Microsoft Reserved (MSR)";
                case "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7": return "Microsoft Basic data";
                case "5808C8AA-7E8F-42E0-85D2-E1E90434CFB3": return "Logical Disk Manager (LDM) metadata";
                case "AF9B60A0-1431-4F62-BC68-3311714A69AD": return "Logical Disk Manager data";
                case "DE94BBA4-06D1-4D40-A16A-BFD50179D6AC": return "Windows Recovery Environment";
                case "37AFFC90-EF7D-4E96-91C3-2D7AE055B174": return "IBM General Parallel File System (GPFS)";
                case "E75CAF8F-F680-4CEE-AFA3-B001E56EFC2D": return "Windows Storage Spaces";
                case "75894C1E-3AEB-11D3-B7C1-7B03A0000000": return "HP-UX Data";
                case "E2A1E728-32E3-11D6-A682-7B03A0000000": return "HP-UX Service";
                case "0FC63DAF-8483-4772-8E79-3D69D8477DE4": return "Linux filesystem";
                case "A19D880F-05FC-4D3B-A006-743F0F84911E": return "Linux RAID";
                case "44479540-F297-41B2-9AF7-D131D5F0458A": return "Linux Root (x86)";
                case "4F68BCE3-E8CD-4DB1-96E7-FBCAF984B709": return "Linux Root (x86-64)";
                case "69DAD710-2CE4-4E3C-B16C-21A1D49ABED3": return "Linux Root (32-bit ARM)";
                case "B921B045-1DF0-41C3-AF44-4C6F280D3FAE": return "Linux Root (64-bit ARM/AArch64)";
                case "0657FD6D-A4AB-43C4-84E5-0933C84B4F4F": return "Linux Swap";
                case "E6D6D379-F507-44C2-A23C-238F2A3DF928": return "Logical Volume Manager (LVM)";
                case "933AC7E1-2EB4-4F13-B844-0E14E2AEF915": return "Linux /home";
                case "3B8F8425-20E0-4F3B-907F-1A25A76F98E8": return "Linux /srv";
                case "7FFEC5C9-2D00-49B7-8941-3EA10A5586B7": return "Plain dm-crypt";
                case "CA7D7CCB-63ED-4C53-861C-1742536059CC": return "LUKS";
                case "8DA63339-0007-60C0-C436-083AC8230908": return "Linux Reserved";
                case "83BD6B9D-7F41-11DC-BE0B-001560B84F0F": return "FreeBSD Boot";
                case "516E7CB4-6ECF-11D6-8FF8-00022D09712B": return "FreeBSD Data";
                case "516E7CB5-6ECF-11D6-8FF8-00022D09712B": return "FreeBSD Swap";
                case "516E7CB6-6ECF-11D6-8FF8-00022D09712B": return "FreeBSD UFS";
                case "516E7CB7-6ECF-11D6-8FF8-00022D09712B": return "FreeBSD UFS2";
                case "516E7CB8-6ECF-11D6-8FF8-00022D09712B": return "FreeBSD Vinum";
                case "516E7CBA-6ECF-11D6-8FF8-00022D09712B": return "FreeBSD ZFS";
                case "74BA7DD9-A689-11E1-BD04-00E081286ACF": return "FreeBSD nandfs";
                case "48465300-0000-11AA-AA11-00306543ECAC": return "Apple HFS";
                case "55465300-0000-11AA-AA11-00306543ECAC": return "Apple UFS";
                case "52414944-0000-11AA-AA11-00306543ECAC": return "Apple RAID";
                case "52414944-5F4F-11AA-AA11-00306543ECAC": return "Apple RAID, offline";
                case "426F6F74-0000-11AA-AA11-00306543ECAC": return "Apple Boot";
                case "4C616265-6C00-11AA-AA11-00306543ECAC": return "Apple Label";
                case "5265636F-7665-11AA-AA11-00306543ECAC": return "Apple TV Recovery";
                case "53746F72-6167-11AA-AA11-00306543ECAC": return "Apple Core Storage";
                case "6A82CB45-1DD2-11B2-99A6-080020736631": return "Solaris Boot";
                case "6A85CF4D-1DD2-11B2-99A6-080020736631": return "Solaris Root";
                case "6A87C46F-1DD2-11B2-99A6-080020736631": return "Solaris Swap";
                case "6A8B642B-1DD2-11B2-99A6-080020736631": return "Solaris Backup";
                case "6A898CC3-1DD2-11B2-99A6-080020736631": return "Solaris /usr or Apple ZFS";
                case "6A8EF2E9-1DD2-11B2-99A6-080020736631": return "Solaris /var";
                case "6A90BA39-1DD2-11B2-99A6-080020736631": return "Solaris /home";
                case "6A9283A5-1DD2-11B2-99A6-080020736631": return "Solaris Alternate sector";
                case "6A945A3B-1DD2-11B2-99A6-080020736631":
                case "6A9630D1-1DD2-11B2-99A6-080020736631":
                case "6A980767-1DD2-11B2-99A6-080020736631":
                case "6A96237F-1DD2-11B2-99A6-080020736631":
                case "6A8D2AC7-1DD2-11B2-99A6-080020736631": return "Solaris Reserved";
                case "49F48D32-B10E-11DC-B99B-0019D1879648": return "NetBSD Swap";
                case "49F48D5A-B10E-11DC-B99B-0019D1879648": return "NetBSD FFS";
                case "49F48D82-B10E-11DC-B99B-0019D1879648": return "NetBSD LFS";
                case "49F48DAA-B10E-11DC-B99B-0019D1879648": return "NetBSD RAID";
                case "2DB519C4-B10F-11DC-B99B-0019D1879648": return "NetBSD Concatenated";
                case "2DB519EC-B10F-11DC-B99B-0019D1879648": return "NetBSD Encrypted";
                case "FE3A2A5D-4F32-41A7-B725-ACCC3285A309": return "ChromeOS kernel";
                case "3CB8E202-3B7E-47DD-8A3C-7FF2A13CFCEC": return "ChromeOS rootfs";
                case "2E0A753D-9E48-43B0-8337-B15192CB1B5E": return "ChromeOS future use";
                case "42465331-3BA3-10F1-802A-4861696B7521": return "Haiku BFS";
                case "85D5E45E-237C-11E1-B4B3-E89A8F7FC3A7": return "MidnightBSD Boot";
                case "85D5E45A-237C-11E1-B4B3-E89A8F7FC3A7": return "MidnightBSD Data";
                case "85D5E45B-237C-11E1-B4B3-E89A8F7FC3A7": return "MidnightBSD Swap";
                case "0394EF8B-237E-11E1-B4B3-E89A8F7FC3A7": return "MidnightBSD UFS";
                case "85D5E45C-237C-11E1-B4B3-E89A8F7FC3A7": return "MidnightBSD Vinum";
                case "85D5E45D-237C-11E1-B4B3-E89A8F7FC3A7": return "MidnightBSD ZFS";
                case "45B0969E-9B03-4F30-B4C6-B4B80CEFF106": return "Ceph Journal";
                case "45B0969E-9B03-4F30-B4C6-5EC00CEFF106": return "Ceph dm-crypt Encrypted Journal";
                case "4FBD7E29-9D25-41B8-AFD0-062C0CEFF05D": return "Ceph OSD";
                case "4FBD7E29-9D25-41B8-AFD0-5EC00CEFF05D": return "Ceph dm-crypt OSD";
                case "89C57F98-2FE5-4DC0-89C1-F3AD0CEFF2BE": return "Ceph disk in creation";
                case "89C57F98-2FE5-4DC0-89C1-5EC00CEFF2BE": return "Ceph dm-crypt disk in creation";
                case "824CC7A0-36A8-11E3-890A-952519AD3F61": return "OpenBSD Data";
                case "CEF5A9AD-73BC-4601-89F3-CDEEEEE321A1": return "QNX Power-safe (QNX6)";
                case "C91818F9-8025-47AF-89D2-F030D7000C2C": return "Plan 9";
                case "9D275380-40AD-11DB-BF97-000C2911D1B8": return "VMware vmkcore (coredump)";
                case "AA31E02A-400F-11DB-9590-000C2911D1B8": return "VMware VMFS";
                case "9198EFFC-31C0-11DB-8F78-000C2911D1B8": return "VMware Reserved";
                case "7412F7D5-A156-4B13-81DC-867174929325": return "ONIE boot";
                case "D4E6E2CD-4469-46F3-B5CB-1BFF57AFC149": return "ONIE config";
                case "9E1A2D38-C612-4316-AA26-8B49521E5A8B": return "PowerPC PReP boot";
                case "0311FC50-01CA-4725-AD77-9ADBB20ACE98": return "Acronis Secure Zone";
                case "7C3457EF-0000-11AA-AA11-00306543ECAC": return "Apple File System";
                case "9D087404-1CA5-11DC-8817-01301BB8A9F5": return "DragonflyBSD Label";
                case "9D58FDBD-1CA5-11DC-8817-01301BB8A9F5": return "DragonflyBSD Swap";
                case "9D94CE7C-1CA5-11DC-8817-01301BB8A9F5": return "DragonflyBSD UFS";
                case "9DD4478F-1CA5-11DC-8817-01301BB8A9F5": return "DragonflyBSD Vinum";
                case "DBD5211B-1CA5-11DC-8817-01301BB8A9F5": return "DragonflyBSD CCD";
                case "3D48CE54-1D16-11DC-8817-01301BB8A9F5": return "DragonflyBSD Label";
                case "BD215AB2-1D16-11DC-8696-01301BB8A9F5": return "DragonflyBSD Legacy";
                case "61DC63AC-6E38-11DC-8513-01301BB8A9F5": return "DragonflyBSD Hammer";
                case "5CBB9AD1-862D-11DC-A94D-01301BB8A9F5": return "DragonflyBSD Hammer2";
                default:                                     return "";
            }
        }

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
    }
}