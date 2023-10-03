// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : dump(8) file system plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies backups created with dump(8) shows information.
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

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;
using ufs_daddr_t = int;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements identification of a dump(8) image (virtual filesystem on a file)</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class dump
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 512)
            return false;

        // It should be start of a tape or floppy or file
        if(partition.Start != 0)
            return false;

        var sbSize = (uint)(Marshal.SizeOf<s_spcl>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<s_spcl>() % imagePlugin.Info.SectorSize != 0)
            sbSize++;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(sector.Length < Marshal.SizeOf<s_spcl>())
            return false;

        spcl16   oldHdr = Marshal.ByteArrayToStructureLittleEndian<spcl16>(sector);
        spcl_aix aixHdr = Marshal.ByteArrayToStructureLittleEndian<spcl_aix>(sector);
        s_spcl   newHdr = Marshal.ByteArrayToStructureLittleEndian<s_spcl>(sector);

        AaruConsole.DebugWriteLine(MODULE_NAME, "old magic = 0x{0:X8}", oldHdr.c_magic);
        AaruConsole.DebugWriteLine(MODULE_NAME, "aix magic = 0x{0:X8}", aixHdr.c_magic);
        AaruConsole.DebugWriteLine(MODULE_NAME, "new magic = 0x{0:X8}", newHdr.c_magic);

        return oldHdr.c_magic == OFS_MAGIC || aixHdr.c_magic is XIX_MAGIC or XIX_CIGAM || newHdr.c_magic == OFS_MAGIC ||
               newHdr.c_magic == NFS_MAGIC || newHdr.c_magic == OFS_CIGAM || newHdr.c_magic == NFS_CIGAM ||
               newHdr.c_magic == UFS2_MAGIC || newHdr.c_magic == UFS2_CIGAM;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        metadata    = new FileSystem();

        if(imagePlugin.Info.SectorSize < 512)
            return;

        if(partition.Start != 0)
            return;

        var sbSize = (uint)(Marshal.SizeOf<s_spcl>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<s_spcl>() % imagePlugin.Info.SectorSize != 0)
            sbSize++;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        if(sector.Length < Marshal.SizeOf<s_spcl>())
            return;

        spcl16   oldHdr = Marshal.ByteArrayToStructureLittleEndian<spcl16>(sector);
        spcl_aix aixHdr = Marshal.ByteArrayToStructureLittleEndian<spcl_aix>(sector);
        s_spcl   newHdr = Marshal.ByteArrayToStructureLittleEndian<s_spcl>(sector);

        var useOld = false;
        var useAix = false;

        if(newHdr.c_magic == OFS_MAGIC  ||
           newHdr.c_magic == NFS_MAGIC  ||
           newHdr.c_magic == OFS_CIGAM  ||
           newHdr.c_magic == NFS_CIGAM  ||
           newHdr.c_magic == UFS2_MAGIC ||
           newHdr.c_magic == UFS2_CIGAM)
        {
            if(newHdr.c_magic == OFS_CIGAM ||
               newHdr.c_magic == NFS_CIGAM ||
               newHdr.c_magic == UFS2_CIGAM)
                newHdr = Marshal.ByteArrayToStructureBigEndian<s_spcl>(sector);
        }
        else if(aixHdr.c_magic is XIX_MAGIC or XIX_CIGAM)
        {
            useAix = true;

            if(aixHdr.c_magic == XIX_CIGAM)
                aixHdr = Marshal.ByteArrayToStructureBigEndian<spcl_aix>(sector);
        }
        else if(oldHdr.c_magic == OFS_MAGIC)
        {
            useOld = true;

            // Swap PDP-11 endian
            oldHdr.c_date  = (int)Swapping.PDPFromLittleEndian((uint)oldHdr.c_date);
            oldHdr.c_ddate = (int)Swapping.PDPFromLittleEndian((uint)oldHdr.c_ddate);
        }
        else
        {
            information = Localization.Could_not_read_dump_8_header_block;

            return;
        }

        var sb = new StringBuilder();

        metadata = new FileSystem
        {
            ClusterSize = 1024,
            Clusters    = partition.Size / 1024
        };

        if(useOld)
        {
            metadata.Type = Localization.Old_16_bit_dump_8;
            sb.AppendLine(metadata.Type);

            if(oldHdr.c_date > 0)
            {
                metadata.CreationDate = DateHandlers.UnixToDateTime(oldHdr.c_date);
                sb.AppendFormat(Localization.Dump_created_on_0, metadata.CreationDate).AppendLine();
            }

            if(oldHdr.c_ddate > 0)
            {
                metadata.BackupDate = DateHandlers.UnixToDateTime(oldHdr.c_ddate);
                sb.AppendFormat(Localization.Previous_dump_created_on_0, metadata.BackupDate).AppendLine();
            }

            sb.AppendFormat(Localization.Dump_volume_number_0, oldHdr.c_volume).AppendLine();
        }
        else if(useAix)
        {
            metadata.Type = FS_TYPE;
            sb.AppendLine(metadata.Type);

            if(aixHdr.c_date > 0)
            {
                metadata.CreationDate = DateHandlers.UnixToDateTime(aixHdr.c_date);

                sb.AppendFormat(Localization.Dump_created_on_0, metadata.CreationDate).AppendLine();
            }

            if(aixHdr.c_ddate > 0)
            {
                metadata.BackupDate = DateHandlers.UnixToDateTime(aixHdr.c_ddate);
                sb.AppendFormat(Localization.Previous_dump_created_on_0, metadata.BackupDate).AppendLine();
            }

            sb.AppendFormat(Localization.Dump_volume_number_0, aixHdr.c_volume).AppendLine();
        }
        else
        {
            metadata.Type = FS_TYPE;
            sb.AppendLine(metadata.Type);

            if(newHdr.c_ndate > 0)
            {
                metadata.CreationDate = DateHandlers.UnixToDateTime(newHdr.c_ndate);

                sb.AppendFormat(Localization.Dump_created_on_0, metadata.CreationDate).AppendLine();
            }
            else if(newHdr.c_date > 0)
            {
                metadata.CreationDate = DateHandlers.UnixToDateTime(newHdr.c_date);

                sb.AppendFormat(Localization.Dump_created_on_0, metadata.CreationDate).AppendLine();
            }

            if(newHdr.c_nddate > 0)
            {
                metadata.BackupDate = DateHandlers.UnixToDateTime(newHdr.c_nddate);
                sb.AppendFormat(Localization.Previous_dump_created_on_0, metadata.BackupDate).AppendLine();
            }
            else if(newHdr.c_ddate > 0)
            {
                metadata.BackupDate = DateHandlers.UnixToDateTime(newHdr.c_ddate);
                sb.AppendFormat(Localization.Previous_dump_created_on_0, metadata.BackupDate).AppendLine();
            }

            sb.AppendFormat(Localization.Dump_volume_number_0, newHdr.c_volume).AppendLine();
            sb.AppendFormat(Localization.Dump_level_0,         newHdr.c_level).AppendLine();
            string dumpname = StringHandlers.CToString(newHdr.c_label);

            if(!string.IsNullOrEmpty(dumpname))
            {
                metadata.VolumeName = dumpname;
                sb.AppendFormat(Localization.Dump_label_0, dumpname).AppendLine();
            }

            string str = StringHandlers.CToString(newHdr.c_filesys);

            if(!string.IsNullOrEmpty(str))
                sb.AppendFormat(Localization.Dumped_filesystem_name_0, str).AppendLine();

            str = StringHandlers.CToString(newHdr.c_dev);

            if(!string.IsNullOrEmpty(str))
                sb.AppendFormat(Localization.Dumped_device_0, str).AppendLine();

            str = StringHandlers.CToString(newHdr.c_host);

            if(!string.IsNullOrEmpty(str))
                sb.AppendFormat(Localization.Dump_hostname_0, str).AppendLine();
        }

        information = sb.ToString();
    }

#endregion
}