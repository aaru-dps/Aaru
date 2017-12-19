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

namespace DiscImageChef.Filesystems
{
    public class PCEnginePlugin : Filesystem
    {
        public PCEnginePlugin()
        {
            Name = "PC Engine CD Plugin";
            PluginUUID = new Guid("e5ee6d7c-90fa-49bd-ac89-14ef750b8af3");
            CurrentEncoding = Encoding.GetEncoding("shift_jis");
        }

        public PCEnginePlugin(Encoding encoding)
        {
            Name = "PC Engine CD Plugin";
            PluginUUID = new Guid("e5ee6d7c-90fa-49bd-ac89-14ef750b8af3");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("shift_jis");
            else CurrentEncoding = encoding;
        }

        public PCEnginePlugin(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "PC Engine CD Plugin";
            PluginUUID = new Guid("e5ee6d7c-90fa-49bd-ac89-14ef750b8af3");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("shift_jis");
            else CurrentEncoding = encoding;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if((2 + partition.Start) >= partition.End) return false;

            byte[] system_descriptor = new byte[23];
            byte[] sector = imagePlugin.ReadSector(1 + partition.Start);

            Array.Copy(sector, 0x20, system_descriptor, 0, 23);

            return Encoding.ASCII.GetString(system_descriptor) == "PC Engine CD-ROM SYSTEM";
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            information = "";
            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Type = "PC Engine filesystem";
            xmlFSType.Clusters = (long)((partition.End - partition.Start + 1) / imagePlugin.GetSectorSize() * 2048);
            xmlFSType.ClusterSize = 2048;
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}