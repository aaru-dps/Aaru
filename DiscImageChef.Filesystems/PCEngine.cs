// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PCEngine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : NEC PC-Engine CD filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the NEC PC-Engine CD filesystem and shows information.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    public class PCEnginePlugin : IFilesystem
    {
        Encoding currentEncoding;
        FileSystemType xmlFsType;
        public virtual FileSystemType XmlFsType => xmlFsType;
        public virtual Encoding Encoding => currentEncoding;
        public virtual string Name => "PC Engine CD Plugin";
        public virtual Guid Id => new Guid("e5ee6d7c-90fa-49bd-ac89-14ef750b8af3");

        public virtual bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End) return false;

            byte[] systemDescriptor = new byte[23];
            byte[] sector = imagePlugin.ReadSector(1 + partition.Start);

            Array.Copy(sector, 0x20, systemDescriptor, 0, 23);

            return Encoding.ASCII.GetString(systemDescriptor) == "PC Engine CD-ROM SYSTEM";
        }

        public virtual void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                            Encoding encoding)
        {
            currentEncoding = encoding ?? Encoding.GetEncoding("shift_jis");
            information = "";
            xmlFsType = new FileSystemType
            {
                Type = "PC Engine filesystem",
                Clusters = (long)((partition.End - partition.Start + 1) / imagePlugin.Info.SectorSize * 2048),
                ClusterSize = 2048
            };
        }

        public virtual Errno Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding, bool debug)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public virtual Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public virtual Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}