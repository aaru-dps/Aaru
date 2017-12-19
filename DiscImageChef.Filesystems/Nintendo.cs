// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Nintendo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Nintendo optical filesystems plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Nintendo optical filesystems and shows information.
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
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems
{
    public class NintendoPlugin : Filesystem
    {
        public NintendoPlugin()
        {
            Name = "Nintendo optical filesystems";
            PluginUUID = new Guid("4675fcb4-4418-4288-9e4a-33d6a4ac1126");
            CurrentEncoding = Encoding.GetEncoding("shift_jis");
        }

        public NintendoPlugin(Encoding encoding)
        {
            Name = "Nintendo optical filesystems";
            PluginUUID = new Guid("4675fcb4-4418-4288-9e4a-33d6a4ac1126");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("shift_jis");
            else CurrentEncoding = encoding;
        }

        public NintendoPlugin(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Nintendo optical filesystems";
            PluginUUID = new Guid("4675fcb4-4418-4288-9e4a-33d6a4ac1126");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("shift_jis");
            else CurrentEncoding = encoding;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if(partition.Start != 0) return false;

            if((imagePlugin.GetSectors() * imagePlugin.GetSectorSize()) < 0x50000) return false;

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] header = imagePlugin.ReadSectors(0, (0x50000 / imagePlugin.GetSectorSize()));

            uint magicGC = BigEndianBitConverter.ToUInt32(header, 0x1C);
            uint magicWii = BigEndianBitConverter.ToUInt32(header, 0x18);

            if(magicGC == 0xC2339F3D || magicWii == 0x5D1C9EA3) return true;

            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            StringBuilder sbInformation = new StringBuilder();
            information = "";
            xmlFSType = new Schemas.FileSystemType();

            NintendoFields fields = new NintendoFields();
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            byte[] header = imagePlugin.ReadSectors(0, (0x50000 / imagePlugin.GetSectorSize()));

            bool wii = false;

            uint magicGC = BigEndianBitConverter.ToUInt32(header, 0x1C);
            uint magicWii = BigEndianBitConverter.ToUInt32(header, 0x18);

            if(magicGC == 0xC2339F3D) wii = false;
            else if(magicWii == 0x5D1C9EA3) wii = true;
            else return;

            fields.discType = Encoding.ASCII.GetString(header, 0, 1);
            fields.gameCode = Encoding.ASCII.GetString(header, 1, 2);
            fields.regionCode = Encoding.ASCII.GetString(header, 3, 1);
            fields.publisherCode = Encoding.ASCII.GetString(header, 4, 2);
            fields.discID = Encoding.ASCII.GetString(header, 0, 6);
            fields.discNumber = header[6];
            fields.discVersion = header[7];
            fields.streaming |= header[8] > 0;
            fields.streamBufferSize = header[9];
            byte[] temp = new byte[64];
            Array.Copy(header, 0x20, temp, 0, 64);
            fields.title = StringHandlers.CToString(temp, CurrentEncoding);

            if(!wii)
            {
                fields.debugOff = BigEndianBitConverter.ToUInt32(header, 0x0400);
                fields.debugAddr = BigEndianBitConverter.ToUInt32(header, 0x0404);
                fields.dolOff = BigEndianBitConverter.ToUInt32(header, 0x0420);
                fields.fstOff = BigEndianBitConverter.ToUInt32(header, 0x0424);
                fields.fstSize = BigEndianBitConverter.ToUInt32(header, 0x0428);
                fields.fstMax = BigEndianBitConverter.ToUInt32(header, 0x042C);
            }

            if(wii)
            {
                uint offset1, offset2, offset3, offset4;
                offset1 = BigEndianBitConverter.ToUInt32(header, 0x40004) << 2;
                offset2 = BigEndianBitConverter.ToUInt32(header, 0x4000C) << 2;
                offset3 = BigEndianBitConverter.ToUInt32(header, 0x40014) << 2;
                offset4 = BigEndianBitConverter.ToUInt32(header, 0x4001C) << 2;

                fields.firstPartitions = new NintendoPartition[BigEndianBitConverter.ToUInt32(header, 0x40000)];
                fields.secondPartitions = new NintendoPartition[BigEndianBitConverter.ToUInt32(header, 0x40008)];
                fields.thirdPartitions = new NintendoPartition[BigEndianBitConverter.ToUInt32(header, 0x40010)];
                fields.fourthPartitions = new NintendoPartition[BigEndianBitConverter.ToUInt32(header, 0x40018)];

                for(int i = 0; i < fields.firstPartitions.Length; i++)
                {
                    if((offset1 + i * 8 + 8) < 0x50000)
                    {
                        fields.firstPartitions[i].offset =
                            BigEndianBitConverter.ToUInt32(header, (int)(offset1 + i * 8 + 0)) << 2;
                        fields.firstPartitions[i].type =
                            BigEndianBitConverter.ToUInt32(header, (int)(offset1 + i * 8 + 4));
                    }
                }

                for(int i = 0; i < fields.secondPartitions.Length; i++)
                {
                    if((offset1 + i * 8 + 8) < 0x50000)
                    {
                        fields.firstPartitions[i].offset =
                            BigEndianBitConverter.ToUInt32(header, (int)(offset2 + i * 8 + 0)) << 2;
                        fields.firstPartitions[i].type =
                            BigEndianBitConverter.ToUInt32(header, (int)(offset2 + i * 8 + 4));
                    }
                }

                for(int i = 0; i < fields.thirdPartitions.Length; i++)
                {
                    if((offset1 + i * 8 + 8) < 0x50000)
                    {
                        fields.firstPartitions[i].offset =
                            BigEndianBitConverter.ToUInt32(header, (int)(offset3 + i * 8 + 0)) << 2;
                        fields.firstPartitions[i].type =
                            BigEndianBitConverter.ToUInt32(header, (int)(offset3 + i * 8 + 4));
                    }
                }

                for(int i = 0; i < fields.fourthPartitions.Length; i++)
                {
                    if((offset1 + i * 8 + 8) < 0x50000)
                    {
                        fields.firstPartitions[i].offset =
                            BigEndianBitConverter.ToUInt32(header, (int)(offset4 + i * 8 + 0)) << 2;
                        fields.firstPartitions[i].type =
                            BigEndianBitConverter.ToUInt32(header, (int)(offset4 + i * 8 + 4));
                    }
                }

                fields.region = header[0x4E000];
                fields.japanAge = header[0x4E010];
                fields.usaAge = header[0x4E011];
                fields.germanAge = header[0x4E013];
                fields.pegiAge = header[0x4E014];
                fields.finlandAge = header[0x4E015];
                fields.portugalAge = header[0x4E016];
                fields.ukAge = header[0x4E017];
                fields.australiaAge = header[0x4E018];
                fields.koreaAge = header[0x4E019];
            }
            else
            {
                fields.firstPartitions = new NintendoPartition[0];
                fields.secondPartitions = new NintendoPartition[0];
                fields.thirdPartitions = new NintendoPartition[0];
                fields.fourthPartitions = new NintendoPartition[0];
            }

            DicConsole.DebugWriteLine("Nintendo plugin", "discType = {0}", fields.discType);
            DicConsole.DebugWriteLine("Nintendo plugin", "gameCode = {0}", fields.gameCode);
            DicConsole.DebugWriteLine("Nintendo plugin", "regionCode = {0}", fields.regionCode);
            DicConsole.DebugWriteLine("Nintendo plugin", "publisherCode = {0}", fields.publisherCode);
            DicConsole.DebugWriteLine("Nintendo plugin", "discID = {0}", fields.discID);
            DicConsole.DebugWriteLine("Nintendo plugin", "discNumber = {0}", fields.discNumber);
            DicConsole.DebugWriteLine("Nintendo plugin", "discVersion = {0}", fields.discVersion);
            DicConsole.DebugWriteLine("Nintendo plugin", "streaming = {0}", fields.streaming);
            DicConsole.DebugWriteLine("Nintendo plugin", "streamBufferSize = {0}", fields.streamBufferSize);
            DicConsole.DebugWriteLine("Nintendo plugin", "title = \"{0}\"", fields.title);
            DicConsole.DebugWriteLine("Nintendo plugin", "debugOff = 0x{0:X8}", fields.debugOff);
            DicConsole.DebugWriteLine("Nintendo plugin", "debugAddr = 0x{0:X8}", fields.debugAddr);
            DicConsole.DebugWriteLine("Nintendo plugin", "dolOff = 0x{0:X8}", fields.dolOff);
            DicConsole.DebugWriteLine("Nintendo plugin", "fstOff = 0x{0:X8}", fields.fstOff);
            DicConsole.DebugWriteLine("Nintendo plugin", "fstSize = {0}", fields.fstSize);
            DicConsole.DebugWriteLine("Nintendo plugin", "fstMax = {0}", fields.fstMax);
            for(int i = 0; i < fields.firstPartitions.Length; i++)
            {
                DicConsole.DebugWriteLine("Nintendo plugin", "firstPartitions[{1}].offset = {0}",
                                          fields.firstPartitions[i].offset, i);
                DicConsole.DebugWriteLine("Nintendo plugin", "firstPartitions[{1}].type = {0}",
                                          fields.firstPartitions[i].type, i);
            }
            for(int i = 0; i < fields.secondPartitions.Length; i++)
            {
                DicConsole.DebugWriteLine("Nintendo plugin", "secondPartitions[{1}].offset = {0}",
                                          fields.secondPartitions[i].offset, i);
                DicConsole.DebugWriteLine("Nintendo plugin", "secondPartitions[{1}].type = {0}",
                                          fields.secondPartitions[i].type, i);
            }
            for(int i = 0; i < fields.thirdPartitions.Length; i++)
            {
                DicConsole.DebugWriteLine("Nintendo plugin", "thirdPartitions[{1}].offset = {0}",
                                          fields.thirdPartitions[i].offset, i);
                DicConsole.DebugWriteLine("Nintendo plugin", "thirdPartitions[{1}].type = {0}",
                                          fields.thirdPartitions[i].type, i);
            }
            for(int i = 0; i < fields.fourthPartitions.Length; i++)
            {
                DicConsole.DebugWriteLine("Nintendo plugin", "fourthPartitions[{1}].offset = {0}",
                                          fields.fourthPartitions[i].offset, i);
                DicConsole.DebugWriteLine("Nintendo plugin", "fourthPartitions[{1}].type = {0}",
                                          fields.fourthPartitions[i].type, i);
            }

            DicConsole.DebugWriteLine("Nintendo plugin", "region = {0}", fields.region);
            DicConsole.DebugWriteLine("Nintendo plugin", "japanAge = {0}", fields.japanAge);
            DicConsole.DebugWriteLine("Nintendo plugin", "usaAge = {0}", fields.usaAge);
            DicConsole.DebugWriteLine("Nintendo plugin", "germanAge = {0}", fields.germanAge);
            DicConsole.DebugWriteLine("Nintendo plugin", "pegiAge = {0}", fields.pegiAge);
            DicConsole.DebugWriteLine("Nintendo plugin", "finlandAge = {0}", fields.finlandAge);
            DicConsole.DebugWriteLine("Nintendo plugin", "portugalAge = {0}", fields.portugalAge);
            DicConsole.DebugWriteLine("Nintendo plugin", "ukAge = {0}", fields.ukAge);
            DicConsole.DebugWriteLine("Nintendo plugin", "australiaAge = {0}", fields.australiaAge);
            DicConsole.DebugWriteLine("Nintendo plugin", "koreaAge = {0}", fields.koreaAge);

            sbInformation.AppendLine("Nintendo optical filesystem");
            if(wii) sbInformation.AppendLine("Nintendo Wii Optical Disc");
            else sbInformation.AppendLine("Nintendo GameCube Optical Disc");
            sbInformation.AppendFormat("Disc ID is {0}", fields.discID).AppendLine();
            sbInformation.AppendFormat("Disc is a {0} disc", DiscTypeToString(fields.discType)).AppendLine();
            sbInformation.AppendFormat("Disc region is {0}", RegionCodeToString(fields.regionCode)).AppendLine();
            sbInformation.AppendFormat("Published by {0}", PublisherCodeToString(fields.publisherCode)).AppendLine();
            if(fields.discNumber > 0)
                sbInformation.AppendFormat("Disc number {0} of a multi-disc set", fields.discNumber + 1).AppendLine();
            if(fields.streaming) sbInformation.AppendLine("Disc is prepared for audio streaming");
            if(fields.streamBufferSize > 0)
                sbInformation.AppendFormat("Audio streaming buffer size is {0} bytes", fields.streamBufferSize)
                             .AppendLine();
            sbInformation.AppendFormat("Title: {0}", fields.title).AppendLine();

            if(wii)
            {
                for(int i = 0; i < fields.firstPartitions.Length; i++)
                    sbInformation.AppendFormat("First {0} partition starts at sector {1}",
                                               PartitionTypeToString(fields.firstPartitions[i].type),
                                               fields.firstPartitions[i].offset / 2048).AppendLine();
                for(int i = 0; i < fields.secondPartitions.Length; i++)
                    sbInformation.AppendFormat("Second {0} partition starts at sector {1}",
                                               PartitionTypeToString(fields.secondPartitions[i].type),
                                               fields.secondPartitions[i].offset / 2048).AppendLine();
                for(int i = 0; i < fields.thirdPartitions.Length; i++)
                    sbInformation.AppendFormat("Third {0} partition starts at sector {1}",
                                               PartitionTypeToString(fields.thirdPartitions[i].type),
                                               fields.thirdPartitions[i].offset / 2048).AppendLine();
                for(int i = 0; i < fields.fourthPartitions.Length; i++)
                    sbInformation.AppendFormat("Fourth {0} partition starts at sector {1}",
                                               PartitionTypeToString(fields.fourthPartitions[i].type),
                                               fields.fourthPartitions[i].offset / 2048).AppendLine();

                //                sbInformation.AppendFormat("Region byte is {0}", fields.region).AppendLine();
                if((fields.japanAge & 0x80) != 0x80)
                    sbInformation.AppendFormat("Japan age rating is {0}", fields.japanAge).AppendLine();
                if((fields.usaAge & 0x80) != 0x80)
                    sbInformation.AppendFormat("ESRB age rating is {0}", fields.usaAge).AppendLine();
                if((fields.germanAge & 0x80) != 0x80)
                    sbInformation.AppendFormat("German age rating is {0}", fields.germanAge).AppendLine();
                if((fields.pegiAge & 0x80) != 0x80)
                    sbInformation.AppendFormat("PEGI age rating is {0}", fields.pegiAge).AppendLine();
                if((fields.finlandAge & 0x80) != 0x80)
                    sbInformation.AppendFormat("Finland age rating is {0}", fields.finlandAge).AppendLine();
                if((fields.portugalAge & 0x80) != 0x80)
                    sbInformation.AppendFormat("Portugal age rating is {0}", fields.portugalAge).AppendLine();
                if((fields.ukAge & 0x80) != 0x80)
                    sbInformation.AppendFormat("UK age rating is {0}", fields.ukAge).AppendLine();
                if((fields.australiaAge & 0x80) != 0x80)
                    sbInformation.AppendFormat("Australia age rating is {0}", fields.australiaAge).AppendLine();
                if((fields.koreaAge & 0x80) != 0x80)
                    sbInformation.AppendFormat("Korea age rating is {0}", fields.koreaAge).AppendLine();
            }
            else
                sbInformation.AppendFormat("FST starts at {0} and has {1} bytes", fields.fstOff, fields.fstSize)
                             .AppendLine();

            information = sbInformation.ToString();
            xmlFSType.Bootable = true;
            xmlFSType.Clusters = (long)((imagePlugin.GetSectors() * imagePlugin.GetSectorSize()) / 2048);
            xmlFSType.ClusterSize = 2048;
            if(wii) xmlFSType.Type = "Nintendo Wii filesystem";
            else xmlFSType.Type = "Nintendo Gamecube filesystem";
            xmlFSType.VolumeName = fields.title;
            xmlFSType.VolumeSerial = fields.discID;
        }

        struct NintendoFields
        {
            public string discType;
            public string gameCode;
            public string regionCode;
            public string publisherCode;
            public string discID;
            public byte discNumber;
            public byte discVersion;
            public bool streaming;
            public byte streamBufferSize;
            public string title;
            public uint debugOff;
            public uint debugAddr;
            public uint dolOff;
            public uint fstOff;
            public uint fstSize;
            public uint fstMax;
            public NintendoPartition[] firstPartitions;
            public NintendoPartition[] secondPartitions;
            public NintendoPartition[] thirdPartitions;
            public NintendoPartition[] fourthPartitions;
            public byte region;
            public byte japanAge;
            public byte usaAge;
            public byte germanAge;
            public byte pegiAge;
            public byte finlandAge;
            public byte portugalAge;
            public byte ukAge;
            public byte australiaAge;
            public byte koreaAge;
        }

        struct NintendoPartition
        {
            public uint offset;
            public uint type;
        }

        string DiscTypeToString(string discType)
        {
            switch(discType)
            {
                case "C": return "Commodore 64 Virtual Console";
                case "D": return "Demo";
                case "E": return "Neo-Geo Virtual Console";
                case "F": return "NES Virtual Console";
                case "G": return "Gamecube";
                case "H": return "Wii channel";
                case "J": return "Super Nintendo Virtual Console";
                case "L": return "Master System Virtual Console";
                case "M": return "Megadrive Virtual Console";
                case "N": return "Nintendo 64 Virtual Console";
                case "P": return "Promotional or TurboGrafx Virtual Console";
                case "Q": return "TurboGrafx CD Virtual Console";
                case "R":
                case "S": return "Wii";
                case "U": return "Utility";
                case "W": return "WiiWare";
                case "X": return "MSX Virtual Console or WiiWare demo";
                case "0":
                case "1": return "Diagnostic";
                case "4": return "Wii Backup";
                case "_": return "WiiFit";
            }

            return string.Format("unknown type '{0}'", discType);
        }

        string RegionCodeToString(string regionCode)
        {
            switch(regionCode)
            {
                case "A": return "any region";
                case "D": return "Germany";
                case "N":
                case "E": return "USA";
                case "F": return "France";
                case "I": return "Italy";
                case "J": return "Japan";
                case "K":
                case "Q": return "Korea";
                case "L":
                case "M":
                case "P": return "PAL";
                case "R": return "Russia";
                case "S": return "Spain";
                case "T": return "Taiwan";
                case "U": return "Australia";
            }

            return string.Format("unknown code '{0}'", regionCode);
        }

        string PublisherCodeToString(string publisherCode)
        {
            switch(publisherCode)
            {
                case "01": return "Nintendo";
                case "08": return "CAPCOM";
                case "41": return "Ubisoft";
                case "4F": return "Eidos";
                case "51": return "Acclaim";
                case "52": return "Activision";
                case "5D": return "Midway";
                case "5G": return "Hudson";
                case "64": return "LucasArts";
                case "69": return "Electronic Arts";
                case "6S": return "TDK Mediactive";
                case "8P": return "SEGA";
                case "A4": return "Mirage Studios";
                case "AF": return "Namco";
                case "B2": return "Bandai";
                case "DA": return "Tomy";
                case "EM": return "Konami";
                case "70": return "Atari";
                case "4Q": return "Disney Interactive";
                case "GD": return "Square Enix";
                case "7D": return "Sierra";
            }

            return string.Format("Unknown publisher '{0}'", publisherCode);
        }

        string PartitionTypeToString(uint type)
        {
            switch(type)
            {
                case 0: return "data";
                case 1: return "update";
                case 2: return "channel";
            }

            return string.Format("unknown type {0}", type);
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