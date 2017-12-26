// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : U.C.S.D. Pascal filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the U.C.S.D. Pascal filesystem and shows information.
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
using System.Text;
using Claunia.Encoding;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;
using Encoding = System.Text.Encoding;

namespace DiscImageChef.Filesystems.UCSDPascal
{
    // Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
    public partial class PascalPlugin
    {
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(partition.Length < 3) return false;

            // Blocks 0 and 1 are boot code
            byte[] volBlock = imagePlugin.ReadSector(2 + partition.Start);

            PascalVolumeEntry volEntry = new PascalVolumeEntry();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            volEntry.firstBlock = BigEndianBitConverter.ToInt16(volBlock, 0x00);
            volEntry.lastBlock = BigEndianBitConverter.ToInt16(volBlock, 0x02);
            volEntry.entryType = (PascalFileKind)BigEndianBitConverter.ToInt16(volBlock, 0x04);
            volEntry.volumeName = new byte[8];
            Array.Copy(volBlock, 0x06, volEntry.volumeName, 0, 8);
            volEntry.blocks = BigEndianBitConverter.ToInt16(volBlock, 0x0E);
            volEntry.files = BigEndianBitConverter.ToInt16(volBlock, 0x10);
            volEntry.dummy = BigEndianBitConverter.ToInt16(volBlock, 0x12);
            volEntry.lastBoot = BigEndianBitConverter.ToInt16(volBlock, 0x14);
            volEntry.tail = BigEndianBitConverter.ToInt32(volBlock, 0x16);

            // First block is always 0 (even is it's sector 2)
            if(volEntry.firstBlock != 0) return false;

            // Last volume record block must be after first block, and before end of device
            if(volEntry.lastBlock <= volEntry.firstBlock ||
               (ulong)volEntry.lastBlock > imagePlugin.Info.Sectors - 2) return false;

            // Volume record entry type must be volume or secure
            if(volEntry.entryType != PascalFileKind.Volume && volEntry.entryType != PascalFileKind.Secure) return false;

            // Volume name is max 7 characters
            if(volEntry.volumeName[0] > 7) return false;

            // Volume blocks is equal to volume sectors
            if(volEntry.blocks < 0 || (ulong)volEntry.blocks != imagePlugin.Info.Sectors) return false;

            // There can be not less than zero files
            return volEntry.files >= 0;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding = encoding ?? new Apple2();
            StringBuilder sbInformation = new StringBuilder();
            information = "";

            if(imagePlugin.Info.Sectors < 3) return;

            // Blocks 0 and 1 are boot code
            byte[] volBlock = imagePlugin.ReadSector(2 + partition.Start);

            PascalVolumeEntry volEntry = new PascalVolumeEntry();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            volEntry.firstBlock = BigEndianBitConverter.ToInt16(volBlock, 0x00);
            volEntry.lastBlock = BigEndianBitConverter.ToInt16(volBlock, 0x02);
            volEntry.entryType = (PascalFileKind)BigEndianBitConverter.ToInt16(volBlock, 0x04);
            volEntry.volumeName = new byte[8];
            Array.Copy(volBlock, 0x06, volEntry.volumeName, 0, 8);
            volEntry.blocks = BigEndianBitConverter.ToInt16(volBlock, 0x0E);
            volEntry.files = BigEndianBitConverter.ToInt16(volBlock, 0x10);
            volEntry.dummy = BigEndianBitConverter.ToInt16(volBlock, 0x12);
            volEntry.lastBoot = BigEndianBitConverter.ToInt16(volBlock, 0x14);
            volEntry.tail = BigEndianBitConverter.ToInt32(volBlock, 0x16);

            // First block is always 0 (even is it's sector 2)
            if(volEntry.firstBlock != 0) return;

            // Last volume record block must be after first block, and before end of device
            if(volEntry.lastBlock <= volEntry.firstBlock ||
               (ulong)volEntry.lastBlock > imagePlugin.Info.Sectors - 2) return;

            // Volume record entry type must be volume or secure
            if(volEntry.entryType != PascalFileKind.Volume && volEntry.entryType != PascalFileKind.Secure) return;

            // Volume name is max 7 characters
            if(volEntry.volumeName[0] > 7) return;

            // Volume blocks is equal to volume sectors
            if(volEntry.blocks < 0 || (ulong)volEntry.blocks != imagePlugin.Info.Sectors) return;

            // There can be not less than zero files
            if(volEntry.files < 0) return;

            sbInformation.AppendFormat("Volume record spans from block {0} to block {1}", volEntry.firstBlock,
                                       volEntry.lastBlock).AppendLine();
            sbInformation.AppendFormat("Volume name: {0}", StringHandlers.PascalToString(volEntry.volumeName, Encoding))
                         .AppendLine();
            sbInformation.AppendFormat("Volume has {0} blocks", volEntry.blocks).AppendLine();
            sbInformation.AppendFormat("Volume has {0} files", volEntry.files).AppendLine();
            sbInformation
                .AppendFormat("Volume last booted at {0}", DateHandlers.UcsdPascalToDateTime(volEntry.lastBoot))
                .AppendLine();

            information = sbInformation.ToString();

            XmlFsType = new FileSystemType
            {
                Bootable = !ArrayHelpers.ArrayIsNullOrEmpty(imagePlugin.ReadSectors(partition.Start, 2)),
                Clusters = volEntry.blocks,
                ClusterSize = (int)imagePlugin.Info.SectorSize,
                Files = volEntry.files,
                FilesSpecified = true,
                Type = "UCSD Pascal",
                VolumeName = StringHandlers.PascalToString(volEntry.volumeName, Encoding)
            };
        }
    }
}