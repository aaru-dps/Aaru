// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple DOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Apple DOS filesystem and shows information.
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
using System.Runtime.InteropServices;
using System.Text;
using Claunia.Encoding;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;
using Encoding = System.Text.Encoding;

namespace DiscImageChef.Filesystems.AppleDOS
{
    public partial class AppleDOS
    {
        public virtual bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.Sectors != 455 && imagePlugin.Info.Sectors != 560) return false;

            if(partition.Start > 0 || imagePlugin.Info.SectorSize != 256) return false;

            int spt;
            spt = imagePlugin.Info.Sectors == 455 ? 13 : 16;

            byte[] vtocB = imagePlugin.ReadSector((ulong)(17 * spt));
            vtoc = new Vtoc();
            IntPtr vtocPtr = Marshal.AllocHGlobal(256);
            Marshal.Copy(vtocB, 0, vtocPtr, 256);
            vtoc = (Vtoc)Marshal.PtrToStructure(vtocPtr, typeof(Vtoc));
            Marshal.FreeHGlobal(vtocPtr);

            return vtoc.catalogSector < spt && vtoc.maxTrackSectorPairsPerSector <= 122 &&
                   vtoc.sectorsPerTrack == spt && vtoc.bytesPerSector == 256;
        }

        public virtual void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
        {
            // TODO: Until Apple ][ encoding is implemented
            currentEncoding = new LisaRoman();
            information = "";
            StringBuilder sb = new StringBuilder();

            int spt;
            spt = imagePlugin.Info.Sectors == 455 ? 13 : 16;

            byte[] vtocB = imagePlugin.ReadSector((ulong)(17 * spt));
            vtoc = new Vtoc();
            IntPtr vtocPtr = Marshal.AllocHGlobal(256);
            Marshal.Copy(vtocB, 0, vtocPtr, 256);
            vtoc = (Vtoc)Marshal.PtrToStructure(vtocPtr, typeof(Vtoc));
            Marshal.FreeHGlobal(vtocPtr);

            sb.AppendLine("Apple DOS File System");
            sb.AppendLine();
            sb.AppendFormat("Catalog starts at sector {0} of track {1}", vtoc.catalogSector, vtoc.catalogTrack)
              .AppendLine();
            sb.AppendFormat("File system initialized by DOS release {0}", vtoc.dosRelease).AppendLine();
            sb.AppendFormat("Disk volume number {0}", vtoc.volumeNumber).AppendLine();
            sb.AppendFormat("Sectors allocated at most in track {0}", vtoc.lastAllocatedSector).AppendLine();
            sb.AppendFormat("{0} tracks in volume", vtoc.tracks).AppendLine();
            sb.AppendFormat("{0} sectors per track", vtoc.sectorsPerTrack).AppendLine();
            sb.AppendFormat("{0} bytes per sector", vtoc.bytesPerSector).AppendLine();
            sb.AppendFormat("Track allocation is {0}", vtoc.allocationDirection > 0 ? "forward" : "reverse")
              .AppendLine();

            information = sb.ToString();

            xmlFsType = new FileSystemType
            {
                Bootable = true,
                Clusters = (long)imagePlugin.Info.Sectors,
                ClusterSize = (int)imagePlugin.Info.SectorSize,
                Type = "Apple DOS"
            };
        }
    }
}