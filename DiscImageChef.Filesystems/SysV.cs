// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SysV.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UNIX System V filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the UNIX System V filesystem and shows information.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Filesystems
{
    // Information from the Linux kernel
    // TODO: Get documentation, test files, and rewrite this mess
    public class SysVfs : Filesystem
    {
        const uint XENIX_MAGIC = 0x002B5544;
        const uint XENIX_CIGAM = 0x44552B00;
        const uint SYSV_MAGIC = 0xFD187E20;
        const uint SYSV_CIGAM = 0x207E18FD;
        // Rest have no magic.
        // Per a Linux kernel, Coherent fs has following:
        const string COH_FNAME = "nonamexxxxx ";
        const string COH_FPACK = "nopackxxxxx\n";
        // SCO AFS
        const ushort SCO_NFREE = 0xFFFF;
        // UNIX 7th Edition has nothing to detect it, so check for a valid filesystem is a must :(
        const ushort V7_NICINOD = 100;
        const ushort V7_NICFREE = 50;
        const uint V7_MAXSIZE = 0x00FFFFFF;

        public SysVfs()
        {
            Name = "UNIX System V filesystem";
            PluginUUID = new Guid("9B8D016A-8561-400E-A12A-A198283C211D");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public SysVfs(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "UNIX System V filesystem";
            PluginUUID = new Guid("9B8D016A-8561-400E-A12A-A198283C211D");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else
                CurrentEncoding = encoding;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if((2 + partition.Start) >= partition.End)
                return false;

            uint magic;
            string s_fname, s_fpack;
            ushort s_nfree, s_ninode;
            uint s_fsize;

            /*for(int j = 0; j<=(br.BaseStream.Length/0x200); j++)
            {
                br.BaseStream.Seek(offset + j*0x200 + 0x1F8, SeekOrigin.Begin); // System V magic location
                magic = br.Readuint();

                if(magic == SYSV_MAGIC || magic == SYSV_CIGAM)
                    Console.WriteLine("0x{0:X8}: 0x{1:X8} FOUND", br.BaseStream.Position-4, magic);
                else
                    Console.WriteLine("0x{0:X8}: 0x{1:X8}", br.BaseStream.Position-4, magic);
            }*/

            /*uint number;
            br.BaseStream.Seek(offset+0x3A00, SeekOrigin.Begin);
            while((br.BaseStream.Position) <= (offset+0x3C00))
            {
                number = br.Readuint();

                Console.WriteLine("@{0:X8}: 0x{1:X8} ({1})", br.BaseStream.Position-offset-4, number);
            }*/

            byte sb_size_in_sectors;

            if(imagePlugin.GetSectorSize() <= 0x400) // Check if underlying device sector size is smaller than SuperBlock size
                sb_size_in_sectors = (byte)(0x400 / imagePlugin.GetSectorSize());
            else
                sb_size_in_sectors = 1; // If not a single sector can store it

            if(partition.End <= (partition.Start + 4 * (ulong)sb_size_in_sectors + sb_size_in_sectors)) // Device must be bigger than SB location + SB size + offset
                return false;

            // Superblock can start on 0x000, 0x200, 0x600 and 0x800, not aligned, so we assume 16 (128 bytes/sector) sectors as a safe value
            for(int i = 0; i <= 16; i++)
            {
                if(i + sb_size_in_sectors >= (int)imagePlugin.ImageInfo.sectors)
                    break;
                
                byte[] sb_sector = imagePlugin.ReadSectors((ulong)i + partition.Start, sb_size_in_sectors);

                magic = BitConverter.ToUInt32(sb_sector, 0x3F8); // XENIX magic location

                if(magic == XENIX_MAGIC || magic == XENIX_CIGAM)
                    return true;

                magic = BitConverter.ToUInt32(sb_sector, 0x1F8); // System V magic location

                if(magic == SYSV_MAGIC || magic == SYSV_CIGAM)
                    return true;

                magic = BitConverter.ToUInt32(sb_sector, 0x1F0); // XENIX 3 magic location

                if(magic == XENIX_MAGIC || magic == XENIX_CIGAM)
                    return true;

                byte[] coherent_string = new byte[6];
                Array.Copy(sb_sector, 0x1E8, coherent_string, 0, 6); // Coherent UNIX s_fname location
                s_fname = StringHandlers.CToString(coherent_string, CurrentEncoding);
                Array.Copy(sb_sector, 0x1EE, coherent_string, 0, 6); // Coherent UNIX s_fpack location
                s_fpack = StringHandlers.CToString(coherent_string, CurrentEncoding);

                if(s_fname == COH_FNAME || s_fpack == COH_FPACK)
                    return true;

                // Now try to identify 7th edition
                s_fsize = BitConverter.ToUInt32(sb_sector, 0x002); // 7th edition's s_fsize
                s_nfree = BitConverter.ToUInt16(sb_sector, 0x006); // 7th edition's s_nfree
                s_ninode = BitConverter.ToUInt16(sb_sector, 0x0D0); // 7th edition's s_ninode

                if(s_fsize > 0 && s_fsize < 0xFFFFFFFF && s_nfree > 0 && s_nfree < 0xFFFF && s_ninode > 0 && s_ninode < 0xFFFF)
                {
                    if((s_fsize & 0xFF) == 0x00 && (s_nfree & 0xFF) == 0x00 && (s_ninode & 0xFF) == 0x00)
                    {
                        // Byteswap
                        s_fsize = ((s_fsize & 0xFF) << 24) + ((s_fsize & 0xFF00) << 8) + ((s_fsize & 0xFF0000) >> 8) + ((s_fsize & 0xFF000000) >> 24);
                        s_nfree = (ushort)(s_nfree >> 8);
                        s_ninode = (ushort)(s_ninode >> 8);
                    }

                    if((s_fsize & 0xFF000000) == 0x00 && (s_nfree & 0xFF00) == 0x00 && (s_ninode & 0xFF00) == 0x00)
                    {
                        if(s_fsize < V7_MAXSIZE && s_nfree < V7_NICFREE && s_ninode < V7_NICINOD)
                        {
                            if((s_fsize * 1024) == ((partition.End - partition.Start) * imagePlugin.GetSectorSize()) || (s_fsize * 512) == ((partition.End - partition.Start) * imagePlugin.GetSectorSize()))
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();
            BigEndianBitConverter.IsLittleEndian = true; // Start in little endian until we know what are we handling here
            int start;
            uint magic;
            string s_fname, s_fpack;
            ushort s_nfree, s_ninode;
            uint s_fsize;
            bool xenix = false;
            bool sysv = false;
            bool sysvr4 = false;
            bool sys7th = false;
            bool coherent = false;
            bool xenix3 = false;
            byte[] sb_sector;
            byte sb_size_in_sectors;

            if(imagePlugin.GetSectorSize() <= 0x400) // Check if underlying device sector size is smaller than SuperBlock size
                sb_size_in_sectors = (byte)(0x400 / imagePlugin.GetSectorSize());
            else
                sb_size_in_sectors = 1; // If not a single sector can store it

            // Superblock can start on 0x000, 0x200, 0x600 and 0x800, not aligned, so we assume 16 (128 bytes/sector) sectors as a safe value
            for(start = 0; start <= 16; start++)
            {
                sb_sector = imagePlugin.ReadSectors((ulong)start + partition.Start, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(sb_sector, 0x3F8); // XENIX magic location

                if(magic == XENIX_MAGIC)
                {
                    BigEndianBitConverter.IsLittleEndian = true; // Little endian
                    xenix = true;
                    break;
                }
                if(magic == XENIX_CIGAM)
                {
                    BigEndianBitConverter.IsLittleEndian = false; // Big endian
                    xenix = true;
                    break;
                }

                magic = BigEndianBitConverter.ToUInt32(sb_sector, 0x1F0); // XENIX 3 magic location

                if(magic == XENIX_MAGIC)
                {
                    BigEndianBitConverter.IsLittleEndian = true; // Little endian
                    xenix3 = true;
                    break;
                }
                if(magic == XENIX_CIGAM)
                {
                    BigEndianBitConverter.IsLittleEndian = false; // Big endian
                    xenix3 = true;
                    break;
                }

                magic = BigEndianBitConverter.ToUInt32(sb_sector, 0x1F8); // XENIX magic location

                if(magic == SYSV_MAGIC)
                {
                    BigEndianBitConverter.IsLittleEndian = true; // Little endian
                    sysv = true;
                    break;
                }
                if(magic == SYSV_CIGAM)
                {
                    BigEndianBitConverter.IsLittleEndian = false; // Big endian
                    sysv = true;
                    break;
                }

                byte[] coherent_string = new byte[6];
                Array.Copy(sb_sector, 0x1E8, coherent_string, 0, 6); // Coherent UNIX s_fname location
                s_fname = StringHandlers.CToString(coherent_string, CurrentEncoding);
                Array.Copy(sb_sector, 0x1EE, coherent_string, 0, 6); // Coherent UNIX s_fpack location
                s_fpack = StringHandlers.CToString(coherent_string, CurrentEncoding);

                if(s_fname == COH_FNAME || s_fpack == COH_FPACK)
                {
                    BigEndianBitConverter.IsLittleEndian = true; // Coherent is in PDP endianness, use helper for that
                    coherent = true;
                    break;
                }

                // Now try to identify 7th edition
                s_fsize = BitConverter.ToUInt32(sb_sector, 0x002); // 7th edition's s_fsize
                s_nfree = BitConverter.ToUInt16(sb_sector, 0x006); // 7th edition's s_nfree
                s_ninode = BitConverter.ToUInt16(sb_sector, 0x0D0); // 7th edition's s_ninode

                if(s_fsize > 0 && s_fsize < 0xFFFFFFFF && s_nfree > 0 && s_nfree < 0xFFFF && s_ninode > 0 && s_ninode < 0xFFFF)
                {
                    if((s_fsize & 0xFF) == 0x00 && (s_nfree & 0xFF) == 0x00 && (s_ninode & 0xFF) == 0x00)
                    {
                        // Byteswap
                        s_fsize = ((s_fsize & 0xFF) << 24) + ((s_fsize & 0xFF00) << 8) + ((s_fsize & 0xFF0000) >> 8) + ((s_fsize & 0xFF000000) >> 24);
                        s_nfree = (ushort)(s_nfree >> 8);
                        s_ninode = (ushort)(s_ninode >> 8);
                    }

                    if((s_fsize & 0xFF000000) == 0x00 && (s_nfree & 0xFF00) == 0x00 && (s_ninode & 0xFF00) == 0x00)
                    {
                        if(s_fsize < V7_MAXSIZE && s_nfree < V7_NICFREE && s_ninode < V7_NICINOD)
                        {
                            if((s_fsize * 1024) == ((partition.End - partition.Start) * imagePlugin.GetSectorSize()) || (s_fsize * 512) == ((partition.End - partition.Start) * imagePlugin.GetSectorSize()))
                            {
                                sys7th = true;
                                BigEndianBitConverter.IsLittleEndian = true;
                                break;
                            }
                        }
                    }
                }
            }
            if(!sys7th && !sysv && !coherent && !xenix && !xenix3)
                return;

            xmlFSType = new Schemas.FileSystemType();

            if(xenix || xenix3)
            {
                byte[] xenix_strings = new byte[6];
                XenixSuperBlock xnx_sb = new XenixSuperBlock();
                sb_sector = imagePlugin.ReadSectors((ulong)start + partition.Start, sb_size_in_sectors);

                if(xenix3)
                {
                    xnx_sb.s_isize = BigEndianBitConverter.ToUInt16(sb_sector, 0x000);
                    xnx_sb.s_fsize = BigEndianBitConverter.ToUInt32(sb_sector, 0x002);
                    xnx_sb.s_nfree = BigEndianBitConverter.ToUInt16(sb_sector, 0x006);
                    xnx_sb.s_ninode = BigEndianBitConverter.ToUInt16(sb_sector, 0x0D0);
                    xnx_sb.s_flock = sb_sector[0x19A];
                    xnx_sb.s_ilock = sb_sector[0x19B];
                    xnx_sb.s_fmod = sb_sector[0x19C];
                    xnx_sb.s_ronly = sb_sector[0x19D];
                    xnx_sb.s_time = BigEndianBitConverter.ToInt32(sb_sector, 0x19E);
                    xnx_sb.s_tfree = BigEndianBitConverter.ToUInt32(sb_sector, 0x1A2);
                    xnx_sb.s_tinode = BigEndianBitConverter.ToUInt16(sb_sector, 0x1A6);
                    xnx_sb.s_cylblks = BigEndianBitConverter.ToUInt16(sb_sector, 0x1A8);
                    xnx_sb.s_gapblks = BigEndianBitConverter.ToUInt16(sb_sector, 0x1AA);
                    xnx_sb.s_dinfo0 = BigEndianBitConverter.ToUInt16(sb_sector, 0x1AC);
                    xnx_sb.s_dinfo1 = BigEndianBitConverter.ToUInt16(sb_sector, 0x1AE);
                    Array.Copy(sb_sector, 0x1B0, xenix_strings, 0, 6);
                    xnx_sb.s_fname = StringHandlers.CToString(xenix_strings, CurrentEncoding);
                    Array.Copy(sb_sector, 0x1B6, xenix_strings, 0, 6);
                    xnx_sb.s_fpack = StringHandlers.CToString(xenix_strings, CurrentEncoding);
                    xnx_sb.s_clean = sb_sector[0x1BC];
                    xnx_sb.s_magic = BigEndianBitConverter.ToUInt32(sb_sector, 0x1F0);
                    xnx_sb.s_type = BigEndianBitConverter.ToUInt32(sb_sector, 0x1F4);
                }
                else
                {
                    xnx_sb.s_isize = BigEndianBitConverter.ToUInt16(sb_sector, 0x000);
                    xnx_sb.s_fsize = BigEndianBitConverter.ToUInt32(sb_sector, 0x002);
                    xnx_sb.s_nfree = BigEndianBitConverter.ToUInt16(sb_sector, 0x006);
                    xnx_sb.s_ninode = BigEndianBitConverter.ToUInt16(sb_sector, 0x198);
                    xnx_sb.s_flock = sb_sector[0x262];
                    xnx_sb.s_ilock = sb_sector[0x263];
                    xnx_sb.s_fmod = sb_sector[0x264];
                    xnx_sb.s_ronly = sb_sector[0x265];
                    xnx_sb.s_time = BigEndianBitConverter.ToInt32(sb_sector, 0x266);
                    xnx_sb.s_tfree = BigEndianBitConverter.ToUInt32(sb_sector, 0x26A);
                    xnx_sb.s_tinode = BigEndianBitConverter.ToUInt16(sb_sector, 0x26E);
                    xnx_sb.s_cylblks = BigEndianBitConverter.ToUInt16(sb_sector, 0x270);
                    xnx_sb.s_gapblks = BigEndianBitConverter.ToUInt16(sb_sector, 0x272);
                    xnx_sb.s_dinfo0 = BigEndianBitConverter.ToUInt16(sb_sector, 0x274);
                    xnx_sb.s_dinfo1 = BigEndianBitConverter.ToUInt16(sb_sector, 0x276);
                    Array.Copy(sb_sector, 0x278, xenix_strings, 0, 6);
                    xnx_sb.s_fname = StringHandlers.CToString(xenix_strings, CurrentEncoding);
                    Array.Copy(sb_sector, 0x27E, xenix_strings, 0, 6);
                    xnx_sb.s_fpack = StringHandlers.CToString(xenix_strings, CurrentEncoding);
                    xnx_sb.s_clean = sb_sector[0x284];
                    xnx_sb.s_magic = BigEndianBitConverter.ToUInt32(sb_sector, 0x3F8);
                    xnx_sb.s_type = BigEndianBitConverter.ToUInt32(sb_sector, 0x3FC);
                }

                uint bs = 512;
                sb.AppendLine("XENIX filesystem");
                xmlFSType.Type = "XENIX fs";
                switch(xnx_sb.s_type)
                {
                    case 1:
                        sb.AppendLine("512 bytes per block");
                        xmlFSType.ClusterSize = 512;
                        break;
                    case 2:
                        sb.AppendLine("1024 bytes per block");
                        bs = 1024;
                        xmlFSType.ClusterSize = 1024;
                        break;
                    case 3:
                        sb.AppendLine("2048 bytes per block");
                        bs = 2048;
                        xmlFSType.ClusterSize = 2048;
                        break;
                    default:
                        sb.AppendFormat("Unknown s_type value: 0x{0:X8}", xnx_sb.s_type).AppendLine();
                        break;
                }
                if(imagePlugin.GetSectorSize() == 2336 || imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448)
                {
                    if(bs != 2048)
                        sb.AppendFormat("WARNING: Filesystem indicates {0} bytes/block while device indicates {1} bytes/sector", bs, 2048).AppendLine();
                }
                else
                {
                    if(bs != imagePlugin.GetSectorSize())
                        sb.AppendFormat("WARNING: Filesystem indicates {0} bytes/block while device indicates {1} bytes/sector", bs, imagePlugin.GetSectorSize()).AppendLine();
                }
                sb.AppendFormat("{0} zones on volume ({1} bytes)", xnx_sb.s_fsize, xnx_sb.s_fsize * bs).AppendLine();
                sb.AppendFormat("{0} free zones on volume ({1} bytes)", xnx_sb.s_tfree, xnx_sb.s_tfree * bs).AppendLine();
                sb.AppendFormat("{0} free blocks on list ({1} bytes)", xnx_sb.s_nfree, xnx_sb.s_nfree * bs).AppendLine();
                sb.AppendFormat("{0} blocks per cylinder ({1} bytes)", xnx_sb.s_cylblks, xnx_sb.s_cylblks * bs).AppendLine();
                sb.AppendFormat("{0} blocks per gap ({1} bytes)", xnx_sb.s_gapblks, xnx_sb.s_gapblks * bs).AppendLine();
                sb.AppendFormat("First data zone: {0}", xnx_sb.s_isize).AppendLine();
                sb.AppendFormat("{0} free inodes on volume", xnx_sb.s_tinode).AppendLine();
                sb.AppendFormat("{0} free inodes on list", xnx_sb.s_ninode).AppendLine();
                if(xnx_sb.s_flock > 0)
                    sb.AppendLine("Free block list is locked");
                if(xnx_sb.s_ilock > 0)
                    sb.AppendLine("inode cache is locked");
                if(xnx_sb.s_fmod > 0)
                    sb.AppendLine("Superblock is being modified");
                if(xnx_sb.s_ronly > 0)
                    sb.AppendLine("Volume is mounted read-only");
                sb.AppendFormat("Superblock last updated on {0}", DateHandlers.UNIXToDateTime(xnx_sb.s_time)).AppendLine();
                if(xnx_sb.s_time != 0)
                {
                    xmlFSType.ModificationDate = DateHandlers.UNIXToDateTime(xnx_sb.s_time);
                    xmlFSType.ModificationDateSpecified = true;
                }
                sb.AppendFormat("Volume name: {0}", xnx_sb.s_fname).AppendLine();
                xmlFSType.VolumeName = xnx_sb.s_fname;
                sb.AppendFormat("Pack name: {0}", xnx_sb.s_fpack).AppendLine();
                if(xnx_sb.s_clean == 0x46)
                    sb.AppendLine("Volume is clean");
                else
                {
                    sb.AppendLine("Volume is dirty");
                    xmlFSType.Dirty = true;
                }
            }

            if(sysv)
            {
                sb_sector = imagePlugin.ReadSectors((ulong)start + partition.Start, sb_size_in_sectors);
                ushort pad0, pad1, pad2;
                byte[] sysv_strings = new byte[6];

                pad0 = BigEndianBitConverter.ToUInt16(sb_sector, 0x002); // First padding
                pad1 = BigEndianBitConverter.ToUInt16(sb_sector, 0x00A); // Second padding
                pad2 = BigEndianBitConverter.ToUInt16(sb_sector, 0x0D6); // Third padding

                // This detection is not working as expected
                sysvr4 |= pad0 == 0 && pad1 == 0 && pad2 == 0;

                SystemVRelease4SuperBlock sysv_sb = new SystemVRelease4SuperBlock();

                sysv_sb.s_isize = BigEndianBitConverter.ToUInt16(sb_sector, 0x000);
                sysv_sb.s_state = BigEndianBitConverter.ToUInt32(sb_sector, 0x1F4);
                sysv_sb.s_magic = BigEndianBitConverter.ToUInt32(sb_sector, 0x1F8);
                sysv_sb.s_type = BigEndianBitConverter.ToUInt32(sb_sector, 0x1FC);
                sysv_sb.s_fsize = BigEndianBitConverter.ToUInt32(sb_sector, 0x004);
                sysv_sb.s_nfree = BigEndianBitConverter.ToUInt16(sb_sector, 0x008);
                sysv_sb.s_ninode = BigEndianBitConverter.ToUInt16(sb_sector, 0x0D4);
                sysv_sb.s_flock = sb_sector[0x1A0];
                sysv_sb.s_ilock = sb_sector[0x1A1];
                sysv_sb.s_fmod = sb_sector[0x1A2];
                sysv_sb.s_ronly = sb_sector[0x1A3];
                sysv_sb.s_time = BigEndianBitConverter.ToUInt32(sb_sector, 0x1A4);
                sysv_sb.s_cylblks = BigEndianBitConverter.ToUInt16(sb_sector, 0x1A8);
                sysv_sb.s_gapblks = BigEndianBitConverter.ToUInt16(sb_sector, 0x1AA);
                sysv_sb.s_dinfo0 = BigEndianBitConverter.ToUInt16(sb_sector, 0x1AC);
                sysv_sb.s_dinfo1 = BigEndianBitConverter.ToUInt16(sb_sector, 0x1AE);
                sysv_sb.s_tfree = BigEndianBitConverter.ToUInt32(sb_sector, 0x1B0);
                sysv_sb.s_tinode = BigEndianBitConverter.ToUInt16(sb_sector, 0x1B4);
                Array.Copy(sb_sector, 0x1B6, sysv_strings, 0, 6);
                sysv_sb.s_fname = StringHandlers.CToString(sysv_strings, CurrentEncoding);
                Array.Copy(sb_sector, 0x1BC, sysv_strings, 0, 6);
                sysv_sb.s_fpack = StringHandlers.CToString(sysv_strings, CurrentEncoding);

                uint bs = 512;
                if(sysvr4)
                {
                    sb.AppendLine("System V Release 4 filesystem");
                    xmlFSType.Type = "SVR4 fs";
                }
                else
                {
                    sb.AppendLine("System V Release 2 filesystem");
                    xmlFSType.Type = "SVR2 fs";
                }
                switch(sysv_sb.s_type)
                {
                    case 1:
                        sb.AppendLine("512 bytes per block");
                        xmlFSType.ClusterSize = 512;
                        break;
                    case 2:
                        sb.AppendLine("1024 bytes per block");
                        bs = 1024;
                        xmlFSType.ClusterSize = 1024;
                        break;
                    case 3:
                        sb.AppendLine("2048 bytes per block");
                        bs = 2048;
                        xmlFSType.ClusterSize = 2048;
                        break;
                    default:
                        sb.AppendFormat("Unknown s_type value: 0x{0:X8}", sysv_sb.s_type).AppendLine();
                        break;
                }
                if(imagePlugin.GetSectorSize() == 2336 || imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448)
                {
                    if(bs != 2048)
                        sb.AppendFormat("WARNING: Filesystem indicates {0} bytes/block while device indicates {1} bytes/sector", bs, 2048).AppendLine();
                }
                else
                {
                    if(bs != imagePlugin.GetSectorSize())
                        sb.AppendFormat("WARNING: Filesystem indicates {0} bytes/block while device indicates {1} bytes/sector", bs, imagePlugin.GetSectorSize()).AppendLine();
                }
                sb.AppendFormat("{0} zones on volume ({1} bytes)", sysv_sb.s_fsize, sysv_sb.s_fsize * bs).AppendLine();
                sb.AppendFormat("{0} free zones on volume ({1} bytes)", sysv_sb.s_tfree, sysv_sb.s_tfree * bs).AppendLine();
                sb.AppendFormat("{0} free blocks on list ({1} bytes)", sysv_sb.s_nfree, sysv_sb.s_nfree * bs).AppendLine();
                sb.AppendFormat("{0} blocks per cylinder ({1} bytes)", sysv_sb.s_cylblks, sysv_sb.s_cylblks * bs).AppendLine();
                sb.AppendFormat("{0} blocks per gap ({1} bytes)", sysv_sb.s_gapblks, sysv_sb.s_gapblks * bs).AppendLine();
                sb.AppendFormat("First data zone: {0}", sysv_sb.s_isize).AppendLine();
                sb.AppendFormat("{0} free inodes on volume", sysv_sb.s_tinode).AppendLine();
                sb.AppendFormat("{0} free inodes on list", sysv_sb.s_ninode).AppendLine();
                if(sysv_sb.s_flock > 0)
                    sb.AppendLine("Free block list is locked");
                if(sysv_sb.s_ilock > 0)
                    sb.AppendLine("inode cache is locked");
                if(sysv_sb.s_fmod > 0)
                    sb.AppendLine("Superblock is being modified");
                if(sysv_sb.s_ronly > 0)
                    sb.AppendLine("Volume is mounted read-only");
                sb.AppendFormat("Superblock last updated on {0}", DateHandlers.UNIXUnsignedToDateTime(sysv_sb.s_time)).AppendLine();
                if(sysv_sb.s_time != 0)
                {
                    xmlFSType.ModificationDate = DateHandlers.UNIXUnsignedToDateTime(sysv_sb.s_time);
                    xmlFSType.ModificationDateSpecified = true;
                }
                sb.AppendFormat("Volume name: {0}", sysv_sb.s_fname).AppendLine();
                xmlFSType.VolumeName = sysv_sb.s_fname;
                sb.AppendFormat("Pack name: {0}", sysv_sb.s_fpack).AppendLine();
                if(sysv_sb.s_state == (0x7C269D38 - sysv_sb.s_time))
                    sb.AppendLine("Volume is clean");
                else
                {
                    sb.AppendLine("Volume is dirty");
                    xmlFSType.Dirty = true;
                }
            }

            if(coherent)
            {
                sb_sector = imagePlugin.ReadSectors((ulong)start + partition.Start, sb_size_in_sectors);
                CoherentSuperBlock coh_sb = new CoherentSuperBlock();
                byte[] coh_strings = new byte[6];

                coh_sb.s_isize = BigEndianBitConverter.ToUInt16(sb_sector, 0x000);
                coh_sb.s_fsize = Swapping.PDPFromLittleEndian(BigEndianBitConverter.ToUInt32(sb_sector, 0x002));
                coh_sb.s_nfree = BigEndianBitConverter.ToUInt16(sb_sector, 0x006);
                coh_sb.s_ninode = BigEndianBitConverter.ToUInt16(sb_sector, 0x108);
                coh_sb.s_flock = sb_sector[0x1D2];
                coh_sb.s_ilock = sb_sector[0x1D3];
                coh_sb.s_fmod = sb_sector[0x1D4];
                coh_sb.s_ronly = sb_sector[0x1D5];
                coh_sb.s_time = Swapping.PDPFromLittleEndian(BigEndianBitConverter.ToUInt32(sb_sector, 0x1D6));
                coh_sb.s_tfree = Swapping.PDPFromLittleEndian(BigEndianBitConverter.ToUInt32(sb_sector, 0x1DE));
                coh_sb.s_tinode = BigEndianBitConverter.ToUInt16(sb_sector, 0x1E2);
                coh_sb.s_int_m = BigEndianBitConverter.ToUInt16(sb_sector, 0x1E4);
                coh_sb.s_int_n = BigEndianBitConverter.ToUInt16(sb_sector, 0x1E6);
                Array.Copy(sb_sector, 0x1E8, coh_strings, 0, 6);
                coh_sb.s_fname = StringHandlers.CToString(coh_strings, CurrentEncoding);
                Array.Copy(sb_sector, 0x1EE, coh_strings, 0, 6);
                coh_sb.s_fpack = StringHandlers.CToString(coh_strings, CurrentEncoding);

                xmlFSType.Type = "Coherent fs";
                xmlFSType.ClusterSize = 512;

                sb.AppendLine("Coherent UNIX filesystem");
                if(imagePlugin.GetSectorSize() != 512)
                    sb.AppendFormat("WARNING: Filesystem indicates {0} bytes/block while device indicates {1} bytes/sector", 512, 2048).AppendLine();
                sb.AppendFormat("{0} zones on volume ({1} bytes)", coh_sb.s_fsize, coh_sb.s_fsize * 512).AppendLine();
                sb.AppendFormat("{0} free zones on volume ({1} bytes)", coh_sb.s_tfree, coh_sb.s_tfree * 512).AppendLine();
                sb.AppendFormat("{0} free blocks on list ({1} bytes)", coh_sb.s_nfree, coh_sb.s_nfree * 512).AppendLine();
                sb.AppendFormat("First data zone: {0}", coh_sb.s_isize).AppendLine();
                sb.AppendFormat("{0} free inodes on volume", coh_sb.s_tinode).AppendLine();
                sb.AppendFormat("{0} free inodes on list", coh_sb.s_ninode).AppendLine();
                if(coh_sb.s_flock > 0)
                    sb.AppendLine("Free block list is locked");
                if(coh_sb.s_ilock > 0)
                    sb.AppendLine("inode cache is locked");
                if(coh_sb.s_fmod > 0)
                    sb.AppendLine("Superblock is being modified");
                if(coh_sb.s_ronly > 0)
                    sb.AppendLine("Volume is mounted read-only");
                sb.AppendFormat("Superblock last updated on {0}", DateHandlers.UNIXUnsignedToDateTime(coh_sb.s_time)).AppendLine();
                if(coh_sb.s_time != 0)
                {
                    xmlFSType.ModificationDate = DateHandlers.UNIXUnsignedToDateTime(coh_sb.s_time);
                    xmlFSType.ModificationDateSpecified = true;
                }
                sb.AppendFormat("Volume name: {0}", coh_sb.s_fname).AppendLine();
                xmlFSType.VolumeName = coh_sb.s_fname;
                sb.AppendFormat("Pack name: {0}", coh_sb.s_fpack).AppendLine();
            }

            if(sys7th)
            {
                sb_sector = imagePlugin.ReadSectors((ulong)start + partition.Start, sb_size_in_sectors);
                UNIX7thEditionSuperBlock v7_sb = new UNIX7thEditionSuperBlock();
                byte[] sys7_strings = new byte[6];

                v7_sb.s_isize = BigEndianBitConverter.ToUInt16(sb_sector, 0x000);
                v7_sb.s_fsize = BigEndianBitConverter.ToUInt32(sb_sector, 0x002);
                v7_sb.s_nfree = BigEndianBitConverter.ToUInt16(sb_sector, 0x006);
                v7_sb.s_ninode = BigEndianBitConverter.ToUInt16(sb_sector, 0x0D0);
                v7_sb.s_flock = sb_sector[0x19A];
                v7_sb.s_ilock = sb_sector[0x19B];
                v7_sb.s_fmod = sb_sector[0x19C];
                v7_sb.s_ronly = sb_sector[0x19D];
                v7_sb.s_time = BigEndianBitConverter.ToUInt32(sb_sector, 0x19E);
                v7_sb.s_tfree = BigEndianBitConverter.ToUInt32(sb_sector, 0x1A2);
                v7_sb.s_tinode = BigEndianBitConverter.ToUInt16(sb_sector, 0x1A6);
                v7_sb.s_int_m = BigEndianBitConverter.ToUInt16(sb_sector, 0x1A8);
                v7_sb.s_int_n = BigEndianBitConverter.ToUInt16(sb_sector, 0x1AA);
                Array.Copy(sb_sector, 0x1AC, sys7_strings, 0, 6);
                v7_sb.s_fname = StringHandlers.CToString(sys7_strings, CurrentEncoding);
                Array.Copy(sb_sector, 0x1B2, sys7_strings, 0, 6);
                v7_sb.s_fpack = StringHandlers.CToString(sys7_strings, CurrentEncoding);

                xmlFSType.Type = "UNIX 7th Edition fs";
                xmlFSType.ClusterSize = 512;
                sb.AppendLine("UNIX 7th Edition filesystem");
                if(imagePlugin.GetSectorSize() != 512)
                    sb.AppendFormat("WARNING: Filesystem indicates {0} bytes/block while device indicates {1} bytes/sector", 512, 2048).AppendLine();
                sb.AppendFormat("{0} zones on volume ({1} bytes)", v7_sb.s_fsize, v7_sb.s_fsize * 512).AppendLine();
                sb.AppendFormat("{0} free zones on volume ({1} bytes)", v7_sb.s_tfree, v7_sb.s_tfree * 512).AppendLine();
                sb.AppendFormat("{0} free blocks on list ({1} bytes)", v7_sb.s_nfree, v7_sb.s_nfree * 512).AppendLine();
                sb.AppendFormat("First data zone: {0}", v7_sb.s_isize).AppendLine();
                sb.AppendFormat("{0} free inodes on volume", v7_sb.s_tinode).AppendLine();
                sb.AppendFormat("{0} free inodes on list", v7_sb.s_ninode).AppendLine();
                if(v7_sb.s_flock > 0)
                    sb.AppendLine("Free block list is locked");
                if(v7_sb.s_ilock > 0)
                    sb.AppendLine("inode cache is locked");
                if(v7_sb.s_fmod > 0)
                    sb.AppendLine("Superblock is being modified");
                if(v7_sb.s_ronly > 0)
                    sb.AppendLine("Volume is mounted read-only");
                sb.AppendFormat("Superblock last updated on {0}", DateHandlers.UNIXUnsignedToDateTime(v7_sb.s_time)).AppendLine();
                if(v7_sb.s_time != 0)
                {
                    xmlFSType.ModificationDate = DateHandlers.UNIXUnsignedToDateTime(v7_sb.s_time);
                    xmlFSType.ModificationDateSpecified = true;
                }
                sb.AppendFormat("Volume name: {0}", v7_sb.s_fname).AppendLine();
                xmlFSType.VolumeName = v7_sb.s_fname;
                sb.AppendFormat("Pack name: {0}", v7_sb.s_fpack).AppendLine();
            }

            information = sb.ToString();

            BigEndianBitConverter.IsLittleEndian = false; // Return to default (bigendian)
        }

        // Old XENIX use different offsets
        struct XenixSuperBlock
        {
            /// <summary>0x000, index of first data zone</summary>
            public ushort s_isize;
            /// <summary>0x002, total number of zones of this volume</summary>
            public uint s_fsize;
            // the start of the free block list:
            /// <summary>0x006, blocks in s_free, &lt;=100</summary>
            public ushort s_nfree;
            /// <summary>0x008, 100 entries, 50 entries for Xenix 3, first free block list chunk</summary>
            public uint[] s_free;
            // the cache of free inodes:
            /// <summary>0x198 (0xD0), number of inodes in s_inode, &lt;= 100</summary>
            public ushort s_ninode;
            /// <summary>0x19A (0xD2), 100 entries, some free inodes</summary>
            public ushort[] s_inode;
            /// <summary>0x262 (0x19A), free block list manipulation lock</summary>
            public byte s_flock;
            /// <summary>0x263 (0x19B), inode cache manipulation lock</summary>
            public byte s_ilock;
            /// <summary>0x264 (0x19C), superblock modification flag</summary>
            public byte s_fmod;
            /// <summary>0x265 (0x19D), read-only mounted flag</summary>
            public byte s_ronly;
            /// <summary>0x266 (0x19E), time of last superblock update</summary>
            public int s_time;
            /// <summary>0x26A (0x1A2), total number of free zones</summary>
            public uint s_tfree;
            /// <summary>0x26E (0x1A6), total number of free inodes</summary>
            public ushort s_tinode;
            /// <summary>0x270 (0x1A8), blocks per cylinder</summary>
            public ushort s_cylblks;
            /// <summary>0x272 (0x1AA), blocks per gap</summary>
            public ushort s_gapblks;
            /// <summary>0x274 (0x1AC), device information ??</summary>
            public ushort s_dinfo0;
            /// <summary>0x276 (0x1AE), device information ??</summary>
            public ushort s_dinfo1;
            /// <summary>0x278 (0x1B0), 6 bytes, volume name</summary>
            public string s_fname;
            /// <summary>0x27E (0x1B6), 6 bytes, pack name</summary>
            public string s_fpack;
            /// <summary>0x284 (0x1BC), 0x46 if volume is clean</summary>
            public byte s_clean;
            /// <summary>0x285 (0x1BD), 371 bytes, 51 bytes for Xenix 3</summary>
            public byte[] s_fill;
            /// <summary>0x3F8 (0x1F0), magic</summary>
            public uint s_magic;
            /// <summary>0x3FC (0x1F4), filesystem type (1 = 512 bytes/blk, 2 = 1024 bytes/blk, 3 = 2048 bytes/blk)</summary>
            public uint s_type;
        }

        struct SystemVRelease4SuperBlock
        {
            /// <summary>0x000, index of first data zone</summary>
            public ushort s_isize;
            /// <summary>0x002, padding</summary>
            public ushort s_pad0;
            /// <summary>0x004, total number of zones of this volume</summary>
            public uint s_fsize;
            // the start of the free block list:
            /// <summary>0x008, blocks in s_free, &lt;=100</summary>
            public ushort s_nfree;
            /// <summary>0x00A, padding</summary>
            public ushort s_pad1;
            /// <summary>0x00C, 50 entries, first free block list chunk</summary>
            public uint[] s_free;
            // the cache of free inodes:
            /// <summary>0x0D4, number of inodes in s_inode, &lt;= 100</summary>
            public ushort s_ninode;
            /// <summary>0x0D6, padding</summary>
            public ushort s_pad2;
            /// <summary>0x0D8, 100 entries, some free inodes</summary>
            public ushort[] s_inode;
            /// <summary>0x1A0, free block list manipulation lock</summary>
            public byte s_flock;
            /// <summary>0x1A1, inode cache manipulation lock</summary>
            public byte s_ilock;
            /// <summary>0x1A2, superblock modification flag</summary>
            public byte s_fmod;
            /// <summary>0x1A3, read-only mounted flag</summary>
            public byte s_ronly;
            /// <summary>0x1A4, time of last superblock update</summary>
            public uint s_time;
            /// <summary>0x1A8, blocks per cylinder</summary>
            public ushort s_cylblks;
            /// <summary>0x1AA, blocks per gap</summary>
            public ushort s_gapblks;
            /// <summary>0x1AC, device information ??</summary>
            public ushort s_dinfo0;
            /// <summary>0x1AE, device information ??</summary>
            public ushort s_dinfo1;
            /// <summary>0x1B0, total number of free zones</summary>
            public uint s_tfree;
            /// <summary>0x1B4, total number of free inodes</summary>
            public ushort s_tinode;
            /// <summary>0x1B6, padding</summary>
            public ushort s_pad3;
            /// <summary>0x1B8, 6 bytes, volume name</summary>
            public string s_fname;
            /// <summary>0x1BE, 6 bytes, pack name</summary>
            public string s_fpack;
            /// <summary>0x1C4, 48 bytes</summary>
            public byte[] s_fill;
            /// <summary>0x1F4, if s_state == (0x7C269D38 - s_time) then filesystem is clean</summary>
            public uint s_state;
            /// <summary>0x1F8, magic</summary>
            public uint s_magic;
            /// <summary>0x1FC, filesystem type (1 = 512 bytes/blk, 2 = 1024 bytes/blk)</summary>
            public uint s_type;
        }

        struct SystemVRelease2SuperBlock
        {
            /// <summary>0x000, index of first data zone</summary>
            public ushort s_isize;
            /// <summary>0x002, total number of zones of this volume</summary>
            public uint s_fsize;
            // the start of the free block list:
            /// <summary>0x006, blocks in s_free, &lt;=100</summary>
            public ushort s_nfree;
            /// <summary>0x008, 50 entries, first free block list chunk</summary>
            public uint[] s_free;
            // the cache of free inodes:
            /// <summary>0x0D0, number of inodes in s_inode, &lt;= 100</summary>
            public ushort s_ninode;
            /// <summary>0x0D2, 100 entries, some free inodes</summary>
            public ushort[] s_inode;
            /// <summary>0x19A, free block list manipulation lock</summary>
            public byte s_flock;
            /// <summary>0x19B, inode cache manipulation lock</summary>
            public byte s_ilock;
            /// <summary>0x19C, superblock modification flag</summary>
            public byte s_fmod;
            /// <summary>0x19D, read-only mounted flag</summary>
            public byte s_ronly;
            /// <summary>0x19E, time of last superblock update</summary>
            public uint s_time;
            /// <summary>0x1A2, blocks per cylinder</summary>
            public ushort s_cylblks;
            /// <summary>0x1A4, blocks per gap</summary>
            public ushort s_gapblks;
            /// <summary>0x1A6, device information ??</summary>
            public ushort s_dinfo0;
            /// <summary>0x1A8, device information ??</summary>
            public ushort s_dinfo1;
            /// <summary>0x1AA, total number of free zones</summary>
            public uint s_tfree;
            /// <summary>0x1AE, total number of free inodes</summary>
            public ushort s_tinode;
            /// <summary>0x1B0, 6 bytes, volume name</summary>
            public string s_fname;
            /// <summary>0x1B6, 6 bytes, pack name</summary>
            public string s_fpack;
            /// <summary>0x1BC, 56 bytes</summary>
            public byte[] s_fill;
            /// <summary>0x1F4, if s_state == (0x7C269D38 - s_time) then filesystem is clean</summary>
            public uint s_state;
            /// <summary>0x1F8, magic</summary>
            public uint s_magic;
            /// <summary>0x1FC, filesystem type (1 = 512 bytes/blk, 2 = 1024 bytes/blk)</summary>
            public uint s_type;
        }

        struct UNIX7thEditionSuperBlock
        {
            /// <summary>0x000, index of first data zone</summary>
            public ushort s_isize;
            /// <summary>0x002, total number of zones of this volume</summary>
            public uint s_fsize;
            // the start of the free block list:
            /// <summary>0x006, blocks in s_free, &lt;=100</summary>
            public ushort s_nfree;
            /// <summary>0x008, 50 entries, first free block list chunk</summary>
            public uint[] s_free;
            // the cache of free inodes:
            /// <summary>0x0D0, number of inodes in s_inode, &lt;= 100</summary>
            public ushort s_ninode;
            /// <summary>0x0D2, 100 entries, some free inodes</summary>
            public ushort[] s_inode;
            /// <summary>0x19A, free block list manipulation lock</summary>
            public byte s_flock;
            /// <summary>0x19B, inode cache manipulation lock</summary>
            public byte s_ilock;
            /// <summary>0x19C, superblock modification flag</summary>
            public byte s_fmod;
            /// <summary>0x19D, read-only mounted flag</summary>
            public byte s_ronly;
            /// <summary>0x19E, time of last superblock update</summary>
            public uint s_time;
            /// <summary>0x1A2, total number of free zones</summary>
            public uint s_tfree;
            /// <summary>0x1A6, total number of free inodes</summary>
            public ushort s_tinode;
            /// <summary>0x1A8, interleave factor</summary>
            public ushort s_int_m;
            /// <summary>0x1AA, interleave factor</summary>
            public ushort s_int_n;
            /// <summary>0x1AC, 6 bytes, volume name</summary>
            public string s_fname;
            /// <summary>0x1B2, 6 bytes, pack name</summary>
            public string s_fpack;
        }

        struct CoherentSuperBlock
        {
            /// <summary>0x000, index of first data zone</summary>
            public ushort s_isize;
            /// <summary>0x002, total number of zones of this volume</summary>
            public uint s_fsize;
            // the start of the free block list:
            /// <summary>0x006, blocks in s_free, &lt;=100</summary>
            public ushort s_nfree;
            /// <summary>0x008, 64 entries, first free block list chunk</summary>
            public uint[] s_free;
            // the cache of free inodes:
            /// <summary>0x108, number of inodes in s_inode, &lt;= 100</summary>
            public ushort s_ninode;
            /// <summary>0x10A, 100 entries, some free inodes</summary>
            public ushort[] s_inode;
            /// <summary>0x1D2, free block list manipulation lock</summary>
            public byte s_flock;
            /// <summary>0x1D3, inode cache manipulation lock</summary>
            public byte s_ilock;
            /// <summary>0x1D4, superblock modification flag</summary>
            public byte s_fmod;
            /// <summary>0x1D5, read-only mounted flag</summary>
            public byte s_ronly;
            /// <summary>0x1D6, time of last superblock update</summary>
            public uint s_time;
            /// <summary>0x1DE, total number of free zones</summary>
            public uint s_tfree;
            /// <summary>0x1E2, total number of free inodes</summary>
            public ushort s_tinode;
            /// <summary>0x1E4, interleave factor</summary>
            public ushort s_int_m;
            /// <summary>0x1E6, interleave factor</summary>
            public ushort s_int_n;
            /// <summary>0x1E8, 6 bytes, volume name</summary>
            public string s_fname;
            /// <summary>0x1EE, 6 bytes, pack name</summary>
            public string s_fpack;
            /// <summary>0x1F4, zero-filled</summary>
            public uint s_unique;
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