// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Apple Hierarchical File System and shows information.
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
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems
{
    // Information from Inside Macintosh
    // https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
    public sealed partial class AppleHFS
    {
        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End)
                return false;

            byte[]      mdbSector;
            ushort      drSigWord;
            ErrorNumber errno;

            if(imagePlugin.Info.SectorSize == 2352 ||
               imagePlugin.Info.SectorSize == 2448 ||
               imagePlugin.Info.SectorSize == 2048)
            {
                errno = imagePlugin.ReadSectors(partition.Start, 2, out mdbSector);

                if(errno != ErrorNumber.NoError)
                    return false;

                foreach(int offset in new[]
                {
                    0, 0x200, 0x400, 0x600, 0x800, 0xA00
                }.Where(offset => mdbSector.Length >= offset + 0x7C + 2))
                {
                    drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, offset);

                    if(drSigWord != AppleCommon.HFS_MAGIC)
                        continue;

                    drSigWord =
                        BigEndianBitConverter.ToUInt16(mdbSector, offset + 0x7C); // Seek to embedded HFS+ signature

                    return drSigWord != AppleCommon.HFSP_MAGIC;
                }
            }
            else
            {
                errno = imagePlugin.ReadSector(2 + partition.Start, out mdbSector);

                if(errno != ErrorNumber.NoError)
                    return false;

                if(mdbSector.Length < 0x7C + 2)
                    return false;

                drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0);

                if(drSigWord != AppleCommon.HFS_MAGIC)
                    return false;

                drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0x7C); // Seek to embedded HFS+ signature

                return drSigWord != AppleCommon.HFSP_MAGIC;
            }

            return false;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("macintosh");
            information = "";

            var sb = new StringBuilder();

            byte[]      bbSector  = null;
            byte[]      mdbSector = null;
            ushort      drSigWord;
            ErrorNumber errno;

            bool apmFromHddOnCd = false;

            if(imagePlugin.Info.SectorSize == 2352 ||
               imagePlugin.Info.SectorSize == 2448 ||
               imagePlugin.Info.SectorSize == 2048)
            {
                errno = imagePlugin.ReadSectors(partition.Start, 2, out byte[] tmpSector);

                if(errno != ErrorNumber.NoError)
                    return;

                foreach(int offset in new[]
                {
                    0, 0x200, 0x400, 0x600, 0x800, 0xA00
                })
                {
                    drSigWord = BigEndianBitConverter.ToUInt16(tmpSector, offset);

                    if(drSigWord != AppleCommon.HFS_MAGIC)
                        continue;

                    bbSector  = new byte[1024];
                    mdbSector = new byte[512];

                    if(offset >= 0x400)
                        Array.Copy(tmpSector, offset - 0x400, bbSector, 0, 1024);

                    Array.Copy(tmpSector, offset, mdbSector, 0, 512);
                    apmFromHddOnCd = true;

                    break;
                }

                if(!apmFromHddOnCd)
                    return;
            }
            else
            {
                errno = imagePlugin.ReadSector(2 + partition.Start, out mdbSector);

                if(errno != ErrorNumber.NoError)
                    return;

                drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0);

                if(drSigWord == AppleCommon.HFS_MAGIC)
                {
                    errno = imagePlugin.ReadSector(partition.Start, out bbSector);

                    if(errno != ErrorNumber.NoError)
                        return;
                }
                else
                    return;
            }

            MasterDirectoryBlock mdb = Marshal.ByteArrayToStructureBigEndian<MasterDirectoryBlock>(mdbSector);

            sb.AppendLine("Apple Hierarchical File System");
            sb.AppendLine();

            if(apmFromHddOnCd)
                sb.AppendLine("HFS uses 512 bytes/sector while device uses 2048 bytes/sector.").AppendLine();

            sb.AppendLine("Master Directory Block:");
            sb.AppendFormat("Creation date: {0}", DateHandlers.MacToDateTime(mdb.drCrDate)).AppendLine();
            sb.AppendFormat("Last modification date: {0}", DateHandlers.MacToDateTime(mdb.drLsMod)).AppendLine();

            if(mdb.drVolBkUp > 0)
            {
                sb.AppendFormat("Last backup date: {0}", DateHandlers.MacToDateTime(mdb.drVolBkUp)).AppendLine();
                sb.AppendFormat("Backup sequence number: {0}", mdb.drVSeqNum).AppendLine();
            }
            else
                sb.AppendLine("Volume has never been backed up");

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

            if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.Journaled))
                sb.AppendLine("Volume is journaled.");

            if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.Inconsistent))
                sb.AppendLine("Volume is seriously inconsistent.");

            if(mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.SoftwareLock))
                sb.AppendLine("Volume is locked by software.");

            sb.AppendFormat("{0} files on root directory", mdb.drNmFls).AppendLine();
            sb.AppendFormat("{0} directories on root directory", mdb.drNmRtDirs).AppendLine();
            sb.AppendFormat("{0} files on volume", mdb.drFilCnt).AppendLine();
            sb.AppendFormat("{0} directories on volume", mdb.drDirCnt).AppendLine();
            sb.AppendFormat("Volume write count: {0}", mdb.drWrCnt).AppendLine();

            sb.AppendFormat("Volume bitmap starting sector (in 512-bytes): {0}", mdb.drVBMSt).AppendLine();
            sb.AppendFormat("Next allocation block: {0}.", mdb.drAllocPtr).AppendLine();
            sb.AppendFormat("{0} volume allocation blocks.", mdb.drNmAlBlks).AppendLine();
            sb.AppendFormat("{0} bytes per allocation block.", mdb.drAlBlkSiz).AppendLine();
            sb.AppendFormat("{0} bytes to allocate when extending a file.", mdb.drClpSiz).AppendLine();
            sb.AppendFormat("{0} bytes to allocate when extending a Extents B-Tree.", mdb.drXTClpSiz).AppendLine();
            sb.AppendFormat("{0} bytes to allocate when extending a Catalog B-Tree.", mdb.drCTClpSiz).AppendLine();
            sb.AppendFormat("Sector of first allocation block: {0}", mdb.drAlBlSt).AppendLine();
            sb.AppendFormat("Next unused CNID: {0}", mdb.drNxtCNID).AppendLine();
            sb.AppendFormat("{0} unused allocation blocks.", mdb.drFreeBks).AppendLine();

            sb.AppendFormat("{0} bytes in the Extents B-Tree", mdb.drXTFlSize).AppendLine();
            sb.AppendFormat("{0} bytes in the Catalog B-Tree", mdb.drCTFlSize).AppendLine();

            sb.AppendFormat("Volume name: {0}", StringHandlers.PascalToString(mdb.drVN, Encoding)).AppendLine();

            sb.AppendLine("Finder info:");
            sb.AppendFormat("CNID of bootable system's directory: {0}", mdb.drFndrInfo0).AppendLine();
            sb.AppendFormat("CNID of first-run application's directory: {0}", mdb.drFndrInfo1).AppendLine();
            sb.AppendFormat("CNID of previously opened directory: {0}", mdb.drFndrInfo2).AppendLine();
            sb.AppendFormat("CNID of bootable Mac OS 8 or 9 directory: {0}", mdb.drFndrInfo3).AppendLine();
            sb.AppendFormat("CNID of bootable Mac OS X directory: {0}", mdb.drFndrInfo5).AppendLine();

            if(mdb.drFndrInfo6 != 0 &&
               mdb.drFndrInfo7 != 0)
                sb.AppendFormat("Mac OS X Volume ID: {0:X8}{1:X8}", mdb.drFndrInfo6, mdb.drFndrInfo7).AppendLine();

            if(mdb.drEmbedSigWord == AppleCommon.HFSP_MAGIC)
            {
                sb.AppendLine("Volume wraps a HFS+ volume.");
                sb.AppendFormat("Starting block of the HFS+ volume: {0}", mdb.xdrStABNt).AppendLine();
                sb.AppendFormat("Allocations blocks of the HFS+ volume: {0}", mdb.xdrNumABlks).AppendLine();
            }
            else
            {
                sb.AppendFormat("{0} blocks in volume cache", mdb.drVCSize).AppendLine();
                sb.AppendFormat("{0} blocks in volume bitmap cache", mdb.drVBMCSize).AppendLine();
                sb.AppendFormat("{0} blocks in volume common cache", mdb.drCtlCSize).AppendLine();
            }

            string bootBlockInfo = AppleCommon.GetBootBlockInformation(bbSector, Encoding);

            if(bootBlockInfo != null)
            {
                sb.AppendLine("Volume is bootable.");
                sb.AppendLine();
                sb.AppendLine(bootBlockInfo);
            }
            else if(mdb.drFndrInfo0 != 0 ||
                    mdb.drFndrInfo3 != 0 ||
                    mdb.drFndrInfo5 != 0)
                sb.AppendLine("Volume is bootable.");
            else
                sb.AppendLine("Volume is not bootable.");

            information = sb.ToString();

            XmlFsType = new FileSystemType();

            if(mdb.drVolBkUp > 0)
            {
                XmlFsType.BackupDate          = DateHandlers.MacToDateTime(mdb.drVolBkUp);
                XmlFsType.BackupDateSpecified = true;
            }

            XmlFsType.Bootable = bootBlockInfo   != null || mdb.drFndrInfo0 != 0 || mdb.drFndrInfo3 != 0 ||
                                 mdb.drFndrInfo5 != 0;

            XmlFsType.Clusters    = mdb.drNmAlBlks;
            XmlFsType.ClusterSize = mdb.drAlBlkSiz;

            if(mdb.drCrDate > 0)
            {
                XmlFsType.CreationDate          = DateHandlers.MacToDateTime(mdb.drCrDate);
                XmlFsType.CreationDateSpecified = true;
            }

            XmlFsType.Dirty                 = !mdb.drAtrb.HasFlag(AppleCommon.VolumeAttributes.Unmounted);
            XmlFsType.Files                 = mdb.drFilCnt;
            XmlFsType.FilesSpecified        = true;
            XmlFsType.FreeClusters          = mdb.drFreeBks;
            XmlFsType.FreeClustersSpecified = true;

            if(mdb.drLsMod > 0)
            {
                XmlFsType.ModificationDate          = DateHandlers.MacToDateTime(mdb.drLsMod);
                XmlFsType.ModificationDateSpecified = true;
            }

            XmlFsType.Type       = "HFS";
            XmlFsType.VolumeName = StringHandlers.PascalToString(mdb.drVN, Encoding);

            if(mdb.drFndrInfo6 != 0 &&
               mdb.drFndrInfo7 != 0)
                XmlFsType.VolumeSerial = $"{mdb.drFndrInfo6:X8}{mdb.drFndrInfo7:X8}";
        }
    }
}