// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Macintosh File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Apple Macintosh File System and shows information.
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
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Claunia.Encoding;
using Schemas;
using Encoding = System.Text.Encoding;

// Information from Inside Macintosh Volume II
public sealed partial class AppleMFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(2 + partition.Start, out byte[] mdbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        var drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0x000);

        return drSigWord == MFS_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? new MacRoman();
        information = "";

        var sb = new StringBuilder();

        var mdb = new MasterDirectoryBlock();

        ErrorNumber errno = imagePlugin.ReadSector(2 + partition.Start, out byte[] mdbSector);

        if(errno != ErrorNumber.NoError)
            return;

        errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] bbSector);

        if(errno != ErrorNumber.NoError)
            return;

        mdb.drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0x000);

        if(mdb.drSigWord != MFS_MAGIC)
            return;

        mdb.drCrDate   = BigEndianBitConverter.ToUInt32(mdbSector, 0x002);
        mdb.drLsBkUp   = BigEndianBitConverter.ToUInt32(mdbSector, 0x006);
        mdb.drAtrb     = (AppleCommon.VolumeAttributes)BigEndianBitConverter.ToUInt16(mdbSector, 0x00A);
        mdb.drNmFls    = BigEndianBitConverter.ToUInt16(mdbSector, 0x00C);
        mdb.drDirSt    = BigEndianBitConverter.ToUInt16(mdbSector, 0x00E);
        mdb.drBlLen    = BigEndianBitConverter.ToUInt16(mdbSector, 0x010);
        mdb.drNmAlBlks = BigEndianBitConverter.ToUInt16(mdbSector, 0x012);
        mdb.drAlBlkSiz = BigEndianBitConverter.ToUInt32(mdbSector, 0x014);
        mdb.drClpSiz   = BigEndianBitConverter.ToUInt32(mdbSector, 0x018);
        mdb.drAlBlSt   = BigEndianBitConverter.ToUInt16(mdbSector, 0x01C);
        mdb.drNxtFNum  = BigEndianBitConverter.ToUInt32(mdbSector, 0x01E);
        mdb.drFreeBks  = BigEndianBitConverter.ToUInt16(mdbSector, 0x022);
        mdb.drVNSiz    = mdbSector[0x024];
        var variableSize = new byte[mdb.drVNSiz + 1];
        Array.Copy(mdbSector, 0x024, variableSize, 0, mdb.drVNSiz + 1);
        mdb.drVN = StringHandlers.PascalToString(variableSize, Encoding);

        sb.AppendLine("Apple Macintosh File System");
        sb.AppendLine();
        sb.AppendLine("Master Directory Block:");
        sb.AppendFormat("Creation date: {0}", DateHandlers.MacToDateTime(mdb.drCrDate)).AppendLine();
        sb.AppendFormat("Last backup date: {0}", DateHandlers.MacToDateTime(mdb.drLsBkUp)).AppendLine();

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.HardwareLock))
            sb.AppendLine("Volume is locked by hardware.");

        sb.AppendLine(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.Unmounted) ? "Volume was unmonted."
                          : "Volume is mounted.");

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.SparedBadBlocks))
            sb.AppendLine("Volume has spared bad blocks.");

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.DoesNotNeedCache))
            sb.AppendLine("Volume does not need cache.");

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.BootInconsistent))
            sb.AppendLine("Boot volume is inconsistent.");

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.ReusedIds))
            sb.AppendLine("There are reused CNIDs.");

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.Inconsistent))
            sb.AppendLine("Volume is seriously inconsistent.");

        if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.SoftwareLock))
            sb.AppendLine("Volume is locked by software.");

        sb.AppendFormat("{0} files on volume", mdb.drNmFls).AppendLine();
        sb.AppendFormat("First directory sector: {0}", mdb.drDirSt).AppendLine();
        sb.AppendFormat("{0} sectors in directory.", mdb.drBlLen).AppendLine();
        sb.AppendFormat("{0} volume allocation blocks.", mdb.drNmAlBlks + 1).AppendLine();
        sb.AppendFormat("Size of allocation blocks: {0} bytes", mdb.drAlBlkSiz).AppendLine();
        sb.AppendFormat("{0} bytes to allocate.", mdb.drClpSiz).AppendLine();
        sb.AppendFormat("First allocation block (#2) starts in sector {0}.", mdb.drAlBlSt).AppendLine();
        sb.AppendFormat("Next unused file number: {0}", mdb.drNxtFNum).AppendLine();
        sb.AppendFormat("{0} unused allocation blocks.", mdb.drFreeBks).AppendLine();
        sb.AppendFormat("Volume name: {0}", mdb.drVN).AppendLine();

        string bootBlockInfo = AppleCommon.GetBootBlockInformation(bbSector, Encoding);

        if(bootBlockInfo != null)
        {
            sb.AppendLine("Volume is bootable.");
            sb.AppendLine();
            sb.AppendLine(bootBlockInfo);
        }
        else
            sb.AppendLine("Volume is not bootable.");

        information = sb.ToString();

        XmlFsType = new FileSystemType();

        if(mdb.drLsBkUp > 0)
        {
            XmlFsType.BackupDate          = DateHandlers.MacToDateTime(mdb.drLsBkUp);
            XmlFsType.BackupDateSpecified = true;
        }

        XmlFsType.Bootable    = bootBlockInfo != null;
        XmlFsType.Clusters    = mdb.drNmAlBlks;
        XmlFsType.ClusterSize = mdb.drAlBlkSiz;

        if(mdb.drCrDate > 0)
        {
            XmlFsType.CreationDate          = DateHandlers.MacToDateTime(mdb.drCrDate);
            XmlFsType.CreationDateSpecified = true;
        }

        XmlFsType.Files                 = mdb.drNmFls;
        XmlFsType.FilesSpecified        = true;
        XmlFsType.FreeClusters          = mdb.drFreeBks;
        XmlFsType.FreeClustersSpecified = true;
        XmlFsType.Type                  = "MFS";
        XmlFsType.VolumeName            = mdb.drVN;
    }
}