// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Claunia.Encoding;
using Schemas;
using Encoding = System.Text.Encoding;

namespace Aaru.Filesystems
{
    public sealed partial class AppleDOS
    {
        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.Sectors != 455 &&
               imagePlugin.Info.Sectors != 560)
                return false;

            if(partition.Start             > 0 ||
               imagePlugin.Info.SectorSize != 256)
                return false;

            int spt = imagePlugin.Info.Sectors == 455 ? 13 : 16;

            byte[] vtocB = imagePlugin.ReadSector((ulong)(17 * spt));
            _vtoc = Marshal.ByteArrayToStructureLittleEndian<Vtoc>(vtocB);

            return _vtoc.catalogSector   < spt  && _vtoc.maxTrackSectorPairsPerSector <= 122 &&
                   _vtoc.sectorsPerTrack == spt && _vtoc.bytesPerSector               == 256;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? new Apple2();
            information = "";
            var sb = new StringBuilder();

            int spt = imagePlugin.Info.Sectors == 455 ? 13 : 16;

            byte[] vtocB = imagePlugin.ReadSector((ulong)(17 * spt));
            _vtoc = Marshal.ByteArrayToStructureLittleEndian<Vtoc>(vtocB);

            sb.AppendLine("Apple DOS File System");
            sb.AppendLine();

            sb.AppendFormat("Catalog starts at sector {0} of track {1}", _vtoc.catalogSector, _vtoc.catalogTrack).
               AppendLine();

            sb.AppendFormat("File system initialized by DOS release {0}", _vtoc.dosRelease).AppendLine();
            sb.AppendFormat("Disk volume number {0}", _vtoc.volumeNumber).AppendLine();
            sb.AppendFormat("Sectors allocated at most in track {0}", _vtoc.lastAllocatedSector).AppendLine();
            sb.AppendFormat("{0} tracks in volume", _vtoc.tracks).AppendLine();
            sb.AppendFormat("{0} sectors per track", _vtoc.sectorsPerTrack).AppendLine();
            sb.AppendFormat("{0} bytes per sector", _vtoc.bytesPerSector).AppendLine();

            sb.AppendFormat("Track allocation is {0}", _vtoc.allocationDirection > 0 ? "forward" : "reverse").
               AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Bootable    = true,
                Clusters    = imagePlugin.Info.Sectors,
                ClusterSize = imagePlugin.Info.SectorSize,
                Type        = "Apple DOS"
            };
        }
    }
}