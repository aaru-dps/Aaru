// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the High Performance Optical File System and shows
//     information.
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

using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;

public sealed partial class HPOFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(16 + partition.Start >= partition.End)
            return false;

        ErrorNumber errno =
            imagePlugin.ReadSector(0 + partition.Start,
                                   out byte[] hpofsBpbSector); // Seek to BIOS parameter block, on logical sector 0

        if(errno != ErrorNumber.NoError)
            return false;

        if(hpofsBpbSector.Length < 512)
            return false;

        BiosParameterBlock bpb = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock>(hpofsBpbSector);

        return bpb.fs_type.SequenceEqual(_type);
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("ibm850");
        information = "";

        var sb = new StringBuilder();

        ErrorNumber errno =
            imagePlugin.ReadSector(0 + partition.Start,
                                   out byte[] hpofsBpbSector); // Seek to BIOS parameter block, on logical sector 0

        if(errno != ErrorNumber.NoError)
            return;

        errno = imagePlugin.ReadSector(13 + partition.Start,
                                       out byte[] medInfoSector); // Seek to media information block, on logical sector 13

        if(errno != ErrorNumber.NoError)
            return;

        errno = imagePlugin.ReadSector(14 + partition.Start,
                                       out byte[] volInfoSector); // Seek to volume information block, on logical sector 14

        if(errno != ErrorNumber.NoError)
            return;

        BiosParameterBlock     bpb = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock>(hpofsBpbSector);
        MediaInformationBlock  mib = Marshal.ByteArrayToStructureBigEndian<MediaInformationBlock>(medInfoSector);
        VolumeInformationBlock vib = Marshal.ByteArrayToStructureBigEndian<VolumeInformationBlock>(volInfoSector);

        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.oem_name = \"{0}\"", StringHandlers.CToString(bpb.oem_name));

        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.bps = {0}", bpb.bps);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.spc = {0}", bpb.spc);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.rsectors = {0}", bpb.rsectors);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.fats_no = {0}", bpb.fats_no);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.root_ent = {0}", bpb.root_ent);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.sectors = {0}", bpb.sectors);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.media = 0x{0:X2}", bpb.media);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.spfat = {0}", bpb.spfat);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.sptrk = {0}", bpb.sptrk);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.heads = {0}", bpb.heads);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.hsectors = {0}", bpb.hsectors);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.big_sectors = {0}", bpb.big_sectors);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.drive_no = 0x{0:X2}", bpb.drive_no);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.nt_flags = {0}", bpb.nt_flags);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.signature = 0x{0:X2}", bpb.signature);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.serial_no = 0x{0:X8}", bpb.serial_no);

        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.volume_label = \"{0}\"",
                                   StringHandlers.SpacePaddedToString(bpb.volume_label));

        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.fs_type = \"{0}\"", StringHandlers.CToString(bpb.fs_type));

        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.boot_code is empty? = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(bpb.boot_code));

        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.unknown = {0}", bpb.unknown);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.unknown2 = {0}", bpb.unknown2);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "bpb.signature2 = {0}", bpb.signature2);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.blockId = \"{0}\"", StringHandlers.CToString(mib.blockId));

        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.volumeLabel = \"{0}\"",
                                   StringHandlers.SpacePaddedToString(mib.volumeLabel));

        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.comment = \"{0}\"",
                                   StringHandlers.SpacePaddedToString(mib.comment));

        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.serial = 0x{0:X8}", mib.serial);

        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.creationTimestamp = {0}",
                                   DateHandlers.DosToDateTime(mib.creationDate, mib.creationTime));

        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.codepageType = {0}", mib.codepageType);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.codepage = {0}", mib.codepage);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.rps = {0}", mib.rps);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.bps = {0}", mib.bps);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.bpc = {0}", mib.bpc);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.unknown2 = {0}", mib.unknown2);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.sectors = {0}", mib.sectors);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.unknown3 = {0}", mib.unknown3);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.unknown4 = {0}", mib.unknown4);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.major = {0}", mib.major);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.minor = {0}", mib.minor);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.unknown5 = {0}", mib.unknown5);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.unknown6 = {0}", mib.unknown6);

        AaruConsole.DebugWriteLine("HPOFS Plugin", "mib.filler is empty? = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(mib.filler));

        AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.blockId = \"{0}\"", StringHandlers.CToString(vib.blockId));
        AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown = {0}", vib.unknown);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown2 = {0}", vib.unknown2);

        AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown3 is empty? = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(vib.unknown3));

        AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown4 = \"{0}\"",
                                   StringHandlers.SpacePaddedToString(vib.unknown4));

        AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.owner = \"{0}\"",
                                   StringHandlers.SpacePaddedToString(vib.owner));

        AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown5 = \"{0}\"",
                                   StringHandlers.SpacePaddedToString(vib.unknown5));

        AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown6 = {0}", vib.unknown6);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.percentFull = {0}", vib.percentFull);
        AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.unknown7 = {0}", vib.unknown7);

        AaruConsole.DebugWriteLine("HPOFS Plugin", "vib.filler is empty? = {0}",
                                   ArrayHelpers.ArrayIsNullOrEmpty(vib.filler));

        sb.AppendLine("High Performance Optical File System");
        sb.AppendFormat("OEM name: {0}", StringHandlers.SpacePaddedToString(bpb.oem_name)).AppendLine();
        sb.AppendFormat("{0} bytes per sector", bpb.bps).AppendLine();
        sb.AppendFormat("{0} sectors per cluster", bpb.spc).AppendLine();
        sb.AppendFormat("Media descriptor: 0x{0:X2}", bpb.media).AppendLine();
        sb.AppendFormat("{0} sectors per track", bpb.sptrk).AppendLine();
        sb.AppendFormat("{0} heads", bpb.heads).AppendLine();
        sb.AppendFormat("{0} sectors hidden before BPB", bpb.hsectors).AppendLine();
        sb.AppendFormat("{0} sectors on volume ({1} bytes)", mib.sectors, mib.sectors * bpb.bps).AppendLine();
        sb.AppendFormat("BIOS Drive Number: 0x{0:X2}", bpb.drive_no).AppendLine();
        sb.AppendFormat("Serial number: 0x{0:X8}", mib.serial).AppendLine();

        sb.AppendFormat("Volume label: {0}", StringHandlers.SpacePaddedToString(mib.volumeLabel, Encoding)).
           AppendLine();

        sb.AppendFormat("Volume comment: {0}", StringHandlers.SpacePaddedToString(mib.comment, Encoding)).AppendLine();

        sb.AppendFormat("Volume owner: {0}", StringHandlers.SpacePaddedToString(vib.owner, Encoding)).AppendLine();

        sb.AppendFormat("Volume created on {0}", DateHandlers.DosToDateTime(mib.creationDate, mib.creationTime)).
           AppendLine();

        sb.AppendFormat("Volume uses {0} codepage {1}", mib.codepageType > 0 && mib.codepageType < 3
                                                            ? mib.codepageType == 2
                                                                  ? "EBCDIC"
                                                                  : "ASCII" : "Unknown", mib.codepage).AppendLine();

        sb.AppendFormat("RPS level: {0}", mib.rps).AppendLine();
        sb.AppendFormat("Filesystem version: {0}.{1}", mib.major, mib.minor).AppendLine();
        sb.AppendFormat("Volume can be filled up to {0}%", vib.percentFull).AppendLine();

        XmlFsType = new FileSystemType
        {
            Clusters               = mib.sectors / bpb.spc,
            ClusterSize            = (uint)(bpb.bps * bpb.spc),
            CreationDate           = DateHandlers.DosToDateTime(mib.creationDate, mib.creationTime),
            CreationDateSpecified  = true,
            DataPreparerIdentifier = StringHandlers.SpacePaddedToString(vib.owner, Encoding),
            Type                   = "HPOFS",
            VolumeName             = StringHandlers.SpacePaddedToString(mib.volumeLabel, Encoding),
            VolumeSerial           = $"{mib.serial:X8}",
            SystemIdentifier       = StringHandlers.SpacePaddedToString(bpb.oem_name)
        };

        information = sb.ToString();
    }
}