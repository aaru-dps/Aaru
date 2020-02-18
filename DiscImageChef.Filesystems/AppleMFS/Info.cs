// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using Claunia.Encoding;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using Schemas;
using Encoding = System.Text.Encoding;

namespace DiscImageChef.Filesystems.AppleMFS
{
    // Information from Inside Macintosh Volume II
    public partial class AppleMFS
    {
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End)
                return false;

            byte[] mdbSector = imagePlugin.ReadSector(2 + partition.Start);

            ushort drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0x000);

            return drSigWord == MFS_MAGIC;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? new MacRoman();
            information = "";

            var sb = new StringBuilder();

            var mdb = new MFS_MasterDirectoryBlock();
            var bb  = new MFS_BootBlock();

            byte[] pString = new byte[16];

            byte[] mdbSector = imagePlugin.ReadSector(2 + partition.Start);
            byte[] bbSector  = imagePlugin.ReadSector(0 + partition.Start);

            mdb.drSigWord = BigEndianBitConverter.ToUInt16(mdbSector, 0x000);

            if(mdb.drSigWord != MFS_MAGIC)
                return;

            mdb.drCrDate   = BigEndianBitConverter.ToUInt32(mdbSector, 0x002);
            mdb.drLsBkUp   = BigEndianBitConverter.ToUInt32(mdbSector, 0x006);
            mdb.drAtrb     = BigEndianBitConverter.ToUInt16(mdbSector, 0x00A);
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
            byte[] variableSize = new byte[mdb.drVNSiz + 1];
            Array.Copy(mdbSector, 0x024, variableSize, 0, mdb.drVNSiz + 1);
            mdb.drVN = StringHandlers.PascalToString(variableSize, Encoding);

            bb.bbID = BigEndianBitConverter.ToUInt16(bbSector, 0x000);

            if(bb.bbID == MFSBB_MAGIC)
            {
                bb.bbEntry    = BigEndianBitConverter.ToUInt32(bbSector, 0x002);
                bb.boot_flags = bbSector[0x006];
                bb.bbVersion  = bbSector[0x007];

                bb.bbPageFlags = BigEndianBitConverter.ToInt16(bbSector, 0x008);

                Array.Copy(mdbSector, 0x00A, pString, 0, 16);
                bb.bbSysName = StringHandlers.PascalToString(pString, Encoding);
                Array.Copy(mdbSector, 0x01A, pString, 0, 16);
                bb.bbShellName = StringHandlers.PascalToString(pString, Encoding);
                Array.Copy(mdbSector, 0x02A, pString, 0, 16);
                bb.bbDbg1Name = StringHandlers.PascalToString(pString, Encoding);
                Array.Copy(mdbSector, 0x03A, pString, 0, 16);
                bb.bbDbg2Name = StringHandlers.PascalToString(pString, Encoding);
                Array.Copy(mdbSector, 0x04A, pString, 0, 16);
                bb.bbScreenName = StringHandlers.PascalToString(pString, Encoding);
                Array.Copy(mdbSector, 0x05A, pString, 0, 16);
                bb.bbHelloName = StringHandlers.PascalToString(pString, Encoding);
                Array.Copy(mdbSector, 0x06A, pString, 0, 16);
                bb.bbScrapName = StringHandlers.PascalToString(pString, Encoding);

                bb.bbCntFCBs     = BigEndianBitConverter.ToUInt16(bbSector, 0x07A);
                bb.bbCntEvts     = BigEndianBitConverter.ToUInt16(bbSector, 0x07C);
                bb.bb128KSHeap   = BigEndianBitConverter.ToUInt32(bbSector, 0x07E);
                bb.bb256KSHeap   = BigEndianBitConverter.ToUInt32(bbSector, 0x082);
                bb.bbSysHeapSize = BigEndianBitConverter.ToUInt32(bbSector, 0x086);
            }
            else
                bb.bbID = 0x0000;

            sb.AppendLine("Apple Macintosh File System");
            sb.AppendLine();
            sb.AppendLine("Master Directory Block:");
            sb.AppendFormat("Creation date: {0}", DateHandlers.MacToDateTime(mdb.drCrDate)).AppendLine();
            sb.AppendFormat("Last backup date: {0}", DateHandlers.MacToDateTime(mdb.drLsBkUp)).AppendLine();

            if((mdb.drAtrb & 0x80) == 0x80)
                sb.AppendLine("Volume is locked by hardware.");

            if((mdb.drAtrb & 0x8000) == 0x8000)
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

            if(bb.bbID == MFSBB_MAGIC)
            {
                sb.AppendLine("Volume is bootable.");
                sb.AppendLine();
                sb.AppendLine("Boot Block:");

                if((bb.boot_flags & 0x40) == 0x40)
                    sb.AppendLine("Boot block should be executed.");

                if((bb.boot_flags & 0x80) == 0x80)
                    sb.AppendLine("Boot block is in new unknown format.");
                else
                {
                    if(bb.bbPageFlags > 0)
                        sb.AppendLine("Allocate secondary sound buffer at boot.");
                    else if(bb.bbPageFlags < 0)
                        sb.AppendLine("Allocate secondary sound and video buffers at boot.");

                    sb.AppendFormat("System filename: {0}", bb.bbSysName).AppendLine();
                    sb.AppendFormat("Finder filename: {0}", bb.bbShellName).AppendLine();
                    sb.AppendFormat("Debugger filename: {0}", bb.bbDbg1Name).AppendLine();
                    sb.AppendFormat("Disassembler filename: {0}", bb.bbDbg2Name).AppendLine();
                    sb.AppendFormat("Startup screen filename: {0}", bb.bbScreenName).AppendLine();
                    sb.AppendFormat("First program to execute at boot: {0}", bb.bbHelloName).AppendLine();
                    sb.AppendFormat("Clipboard filename: {0}", bb.bbScrapName).AppendLine();
                    sb.AppendFormat("Maximum opened files: {0}", bb.bbCntFCBs * 4).AppendLine();
                    sb.AppendFormat("Event queue size: {0}", bb.bbCntEvts).AppendLine();
                    sb.AppendFormat("Heap size with 128KiB of RAM: {0} bytes", bb.bb128KSHeap).AppendLine();
                    sb.AppendFormat("Heap size with 256KiB of RAM: {0} bytes", bb.bb256KSHeap).AppendLine();
                    sb.AppendFormat("Heap size with 512KiB of RAM or more: {0} bytes", bb.bbSysHeapSize).AppendLine();
                }
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

            XmlFsType.Bootable    = bb.bbID == MFSBB_MAGIC;
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
}