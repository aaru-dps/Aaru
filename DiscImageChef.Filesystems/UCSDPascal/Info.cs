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
using DiscImageChef.Console;
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

            multiplier = (uint)(imagePlugin.Info.SectorSize == 256 ? 2 : 1);

            // Blocks 0 and 1 are boot code
            byte[] volBlock = imagePlugin.ReadSectors(multiplier * 2 + partition.Start, multiplier);

            PascalVolumeEntry volEntry = new PascalVolumeEntry();

            // On Apple II, it's little endian
            BigEndianBitConverter.IsLittleEndian =
                multiplier == 2 ? !BitConverter.IsLittleEndian : BitConverter.IsLittleEndian;

            volEntry.FirstBlock = BigEndianBitConverter.ToInt16(volBlock,                 0x00);
            volEntry.LastBlock  = BigEndianBitConverter.ToInt16(volBlock,                 0x02);
            volEntry.EntryType  = (PascalFileKind)BigEndianBitConverter.ToInt16(volBlock, 0x04);
            volEntry.VolumeName = new byte[8];
            Array.Copy(volBlock, 0x06, volEntry.VolumeName, 0, 8);
            volEntry.Blocks   = BigEndianBitConverter.ToInt16(volBlock, 0x0E);
            volEntry.Files    = BigEndianBitConverter.ToInt16(volBlock, 0x10);
            volEntry.Dummy    = BigEndianBitConverter.ToInt16(volBlock, 0x12);
            volEntry.LastBoot = BigEndianBitConverter.ToInt16(volBlock, 0x14);
            volEntry.Tail     = BigEndianBitConverter.ToInt32(volBlock, 0x16);

            DicConsole.DebugWriteLine("UCSD Pascal Plugin", "volEntry.firstBlock = {0}", volEntry.FirstBlock);
            DicConsole.DebugWriteLine("UCSD Pascal Plugin", "volEntry.lastBlock = {0}",  volEntry.LastBlock);
            DicConsole.DebugWriteLine("UCSD Pascal Plugin", "volEntry.entryType = {0}",  volEntry.EntryType);
            DicConsole.DebugWriteLine("UCSD Pascal Plugin", "volEntry.volumeName = {0}", volEntry.VolumeName);
            DicConsole.DebugWriteLine("UCSD Pascal Plugin", "volEntry.blocks = {0}",     volEntry.Blocks);
            DicConsole.DebugWriteLine("UCSD Pascal Plugin", "volEntry.files = {0}",      volEntry.Files);
            DicConsole.DebugWriteLine("UCSD Pascal Plugin", "volEntry.dummy = {0}",      volEntry.Dummy);
            DicConsole.DebugWriteLine("UCSD Pascal Plugin", "volEntry.lastBoot = {0}",   volEntry.LastBoot);
            DicConsole.DebugWriteLine("UCSD Pascal Plugin", "volEntry.tail = {0}",       volEntry.Tail);

            // First block is always 0 (even is it's sector 2)
            if(volEntry.FirstBlock != 0) return false;

            // Last volume record block must be after first block, and before end of device
            if(volEntry.LastBlock        <= volEntry.FirstBlock ||
               (ulong)volEntry.LastBlock > imagePlugin.Info.Sectors / multiplier - 2) return false;

            // Volume record entry type must be volume or secure
            if(volEntry.EntryType != PascalFileKind.Volume && volEntry.EntryType != PascalFileKind.Secure) return false;

            // Volume name is max 7 characters
            if(volEntry.VolumeName[0] > 7) return false;

            // Volume blocks is equal to volume sectors
            if(volEntry.Blocks < 0 || (ulong)volEntry.Blocks != imagePlugin.Info.Sectors / multiplier) return false;

            // There can be not less than zero files
            return volEntry.Files >= 0;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            Encoding                    = encoding ?? new Apple2();
            StringBuilder sbInformation = new StringBuilder();
            information                 = "";
            multiplier                  = (uint)(imagePlugin.Info.SectorSize == 256 ? 2 : 1);

            if(imagePlugin.Info.Sectors < 3) return;

            // Blocks 0 and 1 are boot code
            byte[] volBlock = imagePlugin.ReadSectors(multiplier * 2 + partition.Start, multiplier);

            PascalVolumeEntry volEntry = new PascalVolumeEntry();

            // On Apple //, it's little endian
            BigEndianBitConverter.IsLittleEndian =
                multiplier == 2 ? !BitConverter.IsLittleEndian : BitConverter.IsLittleEndian;

            volEntry.FirstBlock = BigEndianBitConverter.ToInt16(volBlock,                 0x00);
            volEntry.LastBlock  = BigEndianBitConverter.ToInt16(volBlock,                 0x02);
            volEntry.EntryType  = (PascalFileKind)BigEndianBitConverter.ToInt16(volBlock, 0x04);
            volEntry.VolumeName = new byte[8];
            Array.Copy(volBlock, 0x06, volEntry.VolumeName, 0, 8);
            volEntry.Blocks   = BigEndianBitConverter.ToInt16(volBlock, 0x0E);
            volEntry.Files    = BigEndianBitConverter.ToInt16(volBlock, 0x10);
            volEntry.Dummy    = BigEndianBitConverter.ToInt16(volBlock, 0x12);
            volEntry.LastBoot = BigEndianBitConverter.ToInt16(volBlock, 0x14);
            volEntry.Tail     = BigEndianBitConverter.ToInt32(volBlock, 0x16);

            // First block is always 0 (even is it's sector 2)
            if(volEntry.FirstBlock != 0) return;

            // Last volume record block must be after first block, and before end of device
            if(volEntry.LastBlock        <= volEntry.FirstBlock ||
               (ulong)volEntry.LastBlock > imagePlugin.Info.Sectors / multiplier - 2) return;

            // Volume record entry type must be volume or secure
            if(volEntry.EntryType != PascalFileKind.Volume && volEntry.EntryType != PascalFileKind.Secure) return;

            // Volume name is max 7 characters
            if(volEntry.VolumeName[0] > 7) return;

            // Volume blocks is equal to volume sectors
            if(volEntry.Blocks < 0 || (ulong)volEntry.Blocks != imagePlugin.Info.Sectors / multiplier) return;

            // There can be not less than zero files
            if(volEntry.Files < 0) return;

            sbInformation.AppendFormat("Volume record spans from block {0} to block {1}", volEntry.FirstBlock,
                                       volEntry.LastBlock).AppendLine();
            sbInformation.AppendFormat("Volume name: {0}", StringHandlers.PascalToString(volEntry.VolumeName, Encoding))
                         .AppendLine();
            sbInformation.AppendFormat("Volume has {0} blocks", volEntry.Blocks).AppendLine();
            sbInformation.AppendFormat("Volume has {0} files",  volEntry.Files).AppendLine();
            sbInformation
               .AppendFormat("Volume last booted at {0}", DateHandlers.UcsdPascalToDateTime(volEntry.LastBoot))
               .AppendLine();

            information = sbInformation.ToString();

            XmlFsType = new FileSystemType
            {
                Bootable =
                    !ArrayHelpers.ArrayIsNullOrEmpty(imagePlugin.ReadSectors(partition.Start, multiplier * 2)),
                Clusters       = volEntry.Blocks,
                ClusterSize    = (int)imagePlugin.Info.SectorSize,
                Files          = volEntry.Files,
                FilesSpecified = true,
                Type           = "UCSD Pascal",
                VolumeName     = StringHandlers.PascalToString(volEntry.VolumeName, Encoding)
            };
        }
    }
}