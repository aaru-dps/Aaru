using System;
using System.Text;
using FileSystemIDandChk;
using FileSystemIDandChk.ImagePlugins;

// Information from Inside Macintosh
namespace FileSystemIDandChk.Plugins
{
    class LisaFS : Plugin
    {
        const byte LisaFSv1 = 0x0E;
        const byte LisaFSv2 = 0x0F;
        const byte LisaFSv3 = 0x11;
        const uint E_NAME = 32;
        // Maximum string size in LisaFS
        const UInt16 FILEID_FREE = 0x0000;
        const UInt16 FILEID_BOOT = 0xAAAA;
        const UInt16 FILEID_LOADER = 0xBBBB;
        const UInt16 FILEID_MDDF = 0x0001;
        const UInt16 FILEID_BITMAP = 0x0002;
        const UInt16 FILEID_SRECORD = 0x0003;
        const UInt16 FILEID_DIRECTORY = 0x0004;
        // "Catalog file"
        const UInt16 FILEID_ERASED = 0x7FFF;
        const UInt16 FILEID_MAX = FILEID_ERASED;

        public LisaFS(PluginBase Core)
        {
            Name = "Apple Lisa File System";
            PluginUUID = new Guid("7E6034D1-D823-4248-A54D-239742B28391");
        }

        public override bool Identify(ImagePlugin imagePlugin, ulong partitionOffset)
        {
            try
            {
                // LisaOS is big-endian
                BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

                // Minimal LisaOS disk is 3.5" single sided double density, 800 sectors
                if (imagePlugin.GetSectors() < 800)
                    return false;

                // LisaOS searches sectors until tag tells MDDF resides there, so we'll search 100 sectors
                for (int i = 0; i < 100; i++)
                {
                    byte[] tag = imagePlugin.ReadSectorTag((ulong)i, SectorTagType.AppleSectorTag);
                    UInt16 fileid = BigEndianBitConverter.ToUInt16(tag, 0x04);

                    if (MainClass.isDebug)
                        Console.WriteLine("DEBUG (LisaFS plugin): Sector {0}, file ID 0x{1:X4}", i, fileid);

                    if (fileid == FILEID_MDDF)
                    {
                        byte[] sector = imagePlugin.ReadSector((ulong)i);
                        Lisa_MDDF mddf = new Lisa_MDDF();

                        mddf.mddf_block = BigEndianBitConverter.ToUInt32(sector, 0x6C);
                        mddf.volsize_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x70);
                        mddf.volsize_minus_mddf_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x74);
                        mddf.vol_size = BigEndianBitConverter.ToUInt32(sector, 0x78);
                        mddf.blocksize = BigEndianBitConverter.ToUInt16(sector, 0x7C);
                        mddf.datasize = BigEndianBitConverter.ToUInt16(sector, 0x7E);

                        if (MainClass.isDebug)
                        {
                            Console.WriteLine("DEBUG (LisaFS plugin): Current sector = {0}", i);
                            Console.WriteLine("DEBUG (LisaFS plugin): mddf.mddf_block = {0}", mddf.mddf_block);
                            Console.WriteLine("DEBUG (LisaFS plugin): Disk size = {0} sectors", imagePlugin.GetSectors());
                            Console.WriteLine("DEBUG (LisaFS plugin): mddf.vol_size = {0} sectors", mddf.vol_size);
                            Console.WriteLine("DEBUG (LisaFS plugin): mddf.vol_size - 1 = {0}", mddf.volsize_minus_one);
                            Console.WriteLine("DEBUG (LisaFS plugin): mddf.vol_size - mddf.mddf_block -1 = {0}", mddf.volsize_minus_mddf_minus_one);
                            Console.WriteLine("DEBUG (LisaFS plugin): Disk sector = {0} bytes", imagePlugin.GetSectorSize());
                            Console.WriteLine("DEBUG (LisaFS plugin): mddf.blocksize = {0} bytes", mddf.blocksize);
                            Console.WriteLine("DEBUG (LisaFS plugin): mddf.datasize = {0} bytes", mddf.datasize);
                        }

                        if (mddf.mddf_block != i)
                            return false;

                        if (mddf.vol_size > imagePlugin.GetSectors())
                            return false;

                        if (mddf.vol_size - 1 != mddf.volsize_minus_one)
                            return false;

                        if (mddf.vol_size - i - 1 != mddf.volsize_minus_mddf_minus_one)
                            return false;

                        if (mddf.datasize > mddf.blocksize)
                            return false;

                        if (mddf.blocksize < imagePlugin.GetSectorSize())
                            return false;

                        if (mddf.datasize != imagePlugin.GetSectorSize())
                            return false;

                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (LisaFS plugin): Exception {0}, {1}, {2}", ex.Message, ex.InnerException, ex.StackTrace);
                return false;
            }
        }

        public override void GetInformation(ImagePlugin imagePlugin, ulong partitionOffset, out string information)
        {
            information = "";
            StringBuilder sb = new StringBuilder();

            try
            {
                // LisaOS is big-endian
                BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

                // Minimal LisaOS disk is 3.5" single sided double density, 800 sectors
                if (imagePlugin.GetSectors() < 800)
                    return;

                // LisaOS searches sectors until tag tells MDDF resides there, so we'll search 100 sectors
                for (int i = 0; i < 100; i++)
                {
                    byte[] tag = imagePlugin.ReadSectorTag((ulong)i, SectorTagType.AppleSectorTag);
                    UInt16 fileid = BigEndianBitConverter.ToUInt16(tag, 0x04);

                    if (MainClass.isDebug)
                        Console.WriteLine("DEBUG (LisaFS plugin): Sector {0}, file ID 0x{1:X4}", i, fileid);

                    if (fileid == FILEID_MDDF)
                    {
                        byte[] sector = imagePlugin.ReadSector((ulong)i);
                        Lisa_MDDF mddf = new Lisa_MDDF();
                        byte[] pString = new byte[33];
                        UInt32 lisa_time;

                        mddf.fsversion = BigEndianBitConverter.ToUInt16(sector, 0x00);
                        mddf.volid = BigEndianBitConverter.ToUInt64(sector, 0x02);
                        mddf.volnum = BigEndianBitConverter.ToUInt16(sector, 0x0A);
                        Array.Copy(sector, 0x0C, pString, 0, 33);
                        mddf.volname = StringHandlers.PascalToString(pString);
                        mddf.unknown1 = sector[0x2D];
                        Array.Copy(sector, 0x2E, pString, 0, 33);
                        // Prevent garbage
                        if (pString[0] <= 32)
                            mddf.password = StringHandlers.PascalToString(pString);
                        else
                            mddf.password = "";
                        mddf.unknown2 = sector[0x4F];
                        mddf.machine_id = BigEndianBitConverter.ToUInt32(sector, 0x50);
                        mddf.master_copy_id = BigEndianBitConverter.ToUInt32(sector, 0x54);
                        lisa_time = BigEndianBitConverter.ToUInt32(sector, 0x58);
                        mddf.dtvc = DateHandlers.LisaToDateTime(lisa_time);
                        lisa_time = BigEndianBitConverter.ToUInt32(sector, 0x5C);
                        mddf.dtcc = DateHandlers.LisaToDateTime(lisa_time);
                        lisa_time = BigEndianBitConverter.ToUInt32(sector, 0x60);
                        mddf.dtvb = DateHandlers.LisaToDateTime(lisa_time);
                        lisa_time = BigEndianBitConverter.ToUInt32(sector, 0x64);
                        mddf.dtvs = DateHandlers.LisaToDateTime(lisa_time);
                        mddf.unknown3 = BigEndianBitConverter.ToUInt32(sector, 0x68);
                        mddf.mddf_block = BigEndianBitConverter.ToUInt32(sector, 0x6C);
                        mddf.volsize_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x70);
                        mddf.volsize_minus_mddf_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x74);
                        mddf.vol_size = BigEndianBitConverter.ToUInt32(sector, 0x78);
                        mddf.blocksize = BigEndianBitConverter.ToUInt16(sector, 0x7C);
                        mddf.datasize = BigEndianBitConverter.ToUInt16(sector, 0x7E);
                        mddf.unknown4 = BigEndianBitConverter.ToUInt16(sector, 0x80);
                        mddf.unknown5 = BigEndianBitConverter.ToUInt32(sector, 0x82);
                        mddf.unknown6 = BigEndianBitConverter.ToUInt32(sector, 0x86);
                        mddf.clustersize = BigEndianBitConverter.ToUInt16(sector, 0x8A);
                        mddf.fs_size = BigEndianBitConverter.ToUInt32(sector, 0x8C);
                        mddf.unknown7 = BigEndianBitConverter.ToUInt32(sector, 0x90);
                        mddf.unknown8 = BigEndianBitConverter.ToUInt32(sector, 0x94);
                        mddf.unknown9 = BigEndianBitConverter.ToUInt32(sector, 0x98);
                        mddf.unknown10 = BigEndianBitConverter.ToUInt32(sector, 0x9C);
                        mddf.unknown11 = BigEndianBitConverter.ToUInt32(sector, 0xA0);
                        mddf.unknown12 = BigEndianBitConverter.ToUInt32(sector, 0xA4);
                        mddf.unknown13 = BigEndianBitConverter.ToUInt32(sector, 0xA8);
                        mddf.unknown14 = BigEndianBitConverter.ToUInt32(sector, 0xAC);
                        mddf.filecount = BigEndianBitConverter.ToUInt16(sector, 0xB0);
                        mddf.unknown15 = BigEndianBitConverter.ToUInt32(sector, 0xB2);
                        mddf.unknown16 = BigEndianBitConverter.ToUInt32(sector, 0xB6);
                        mddf.freecount = BigEndianBitConverter.ToUInt32(sector, 0xBA);
                        mddf.unknown17 = BigEndianBitConverter.ToUInt16(sector, 0xBE);
                        mddf.unknown18 = BigEndianBitConverter.ToUInt32(sector, 0xC0);
                        mddf.overmount_stamp = BigEndianBitConverter.ToUInt64(sector, 0xC4);
                        mddf.serialization = BigEndianBitConverter.ToUInt32(sector, 0xCC);
                        mddf.unknown19 = BigEndianBitConverter.ToUInt32(sector, 0xD0);
                        mddf.unknown_timestamp = BigEndianBitConverter.ToUInt32(sector, 0xD4);
                        mddf.unknown20 = BigEndianBitConverter.ToUInt32(sector, 0xD8);
                        mddf.unknown21 = BigEndianBitConverter.ToUInt32(sector, 0xDC);
                        mddf.unknown22 = BigEndianBitConverter.ToUInt32(sector, 0xE0);
                        mddf.unknown23 = BigEndianBitConverter.ToUInt32(sector, 0xE4);
                        mddf.unknown24 = BigEndianBitConverter.ToUInt32(sector, 0xE8);
                        mddf.unknown25 = BigEndianBitConverter.ToUInt32(sector, 0xEC);
                        mddf.unknown26 = BigEndianBitConverter.ToUInt32(sector, 0xF0);
                        mddf.unknown27 = BigEndianBitConverter.ToUInt32(sector, 0xF4);
                        mddf.unknown28 = BigEndianBitConverter.ToUInt32(sector, 0xF8);
                        mddf.unknown29 = BigEndianBitConverter.ToUInt32(sector, 0xFC);
                        mddf.unknown30 = BigEndianBitConverter.ToUInt32(sector, 0x100);
                        mddf.unknown31 = BigEndianBitConverter.ToUInt32(sector, 0x104);
                        mddf.unknown32 = BigEndianBitConverter.ToUInt32(sector, 0x108);
                        mddf.unknown33 = BigEndianBitConverter.ToUInt32(sector, 0x10C);
                        mddf.unknown34 = BigEndianBitConverter.ToUInt32(sector, 0x110);
                        mddf.unknown35 = BigEndianBitConverter.ToUInt32(sector, 0x114);
                        mddf.backup_volid = BigEndianBitConverter.ToUInt64(sector, 0x118);
                        mddf.label_size = BigEndianBitConverter.ToUInt16(sector, 0x120);
                        mddf.fs_overhead = BigEndianBitConverter.ToUInt16(sector, 0x122);
                        mddf.result_scavenge = BigEndianBitConverter.ToUInt16(sector, 0x124);
                        mddf.boot_code = BigEndianBitConverter.ToUInt16(sector, 0x126);
                        mddf.boot_environ = BigEndianBitConverter.ToUInt16(sector, 0x6C);
                        mddf.unknown36 = BigEndianBitConverter.ToUInt32(sector, 0x12A);
                        mddf.unknown37 = BigEndianBitConverter.ToUInt32(sector, 0x12E);
                        mddf.unknown38 = BigEndianBitConverter.ToUInt32(sector, 0x132);
                        mddf.vol_sequence = BigEndianBitConverter.ToUInt16(sector, 0x136);
                        mddf.vol_left_mounted = sector[0x138];

                        if (MainClass.isDebug)
                        {
                            Console.WriteLine("mddf.unknown1 = 0x{0:X2} ({0})", mddf.unknown1);
                            Console.WriteLine("mddf.unknown2 = 0x{0:X2} ({0})", mddf.unknown2);
                            Console.WriteLine("mddf.unknown3 = 0x{0:X8} ({0})", mddf.unknown3);
                            Console.WriteLine("mddf.unknown4 = 0x{0:X4} ({0})", mddf.unknown4);
                            Console.WriteLine("mddf.unknown5 = 0x{0:X8} ({0})", mddf.unknown5);
                            Console.WriteLine("mddf.unknown6 = 0x{0:X8} ({0})", mddf.unknown6);
                            Console.WriteLine("mddf.unknown7 = 0x{0:X8} ({0})", mddf.unknown7);
                            Console.WriteLine("mddf.unknown8 = 0x{0:X8} ({0})", mddf.unknown8);
                            Console.WriteLine("mddf.unknown9 = 0x{0:X8} ({0})", mddf.unknown9);
                            Console.WriteLine("mddf.unknown10 = 0x{0:X8} ({0})", mddf.unknown10);
                            Console.WriteLine("mddf.unknown11 = 0x{0:X8} ({0})", mddf.unknown11);
                            Console.WriteLine("mddf.unknown12 = 0x{0:X8} ({0})", mddf.unknown12);
                            Console.WriteLine("mddf.unknown13 = 0x{0:X8} ({0})", mddf.unknown13);
                            Console.WriteLine("mddf.unknown14 = 0x{0:X8} ({0})", mddf.unknown14);
                            Console.WriteLine("mddf.unknown15 = 0x{0:X8} ({0})", mddf.unknown15);
                            Console.WriteLine("mddf.unknown16 = 0x{0:X8} ({0})", mddf.unknown16);
                            Console.WriteLine("mddf.unknown17 = 0x{0:X4} ({0})", mddf.unknown17);
                            Console.WriteLine("mddf.unknown18 = 0x{0:X8} ({0})", mddf.unknown18);
                            Console.WriteLine("mddf.unknown19 = 0x{0:X8} ({0})", mddf.unknown19);
                            Console.WriteLine("mddf.unknown20 = 0x{0:X8} ({0})", mddf.unknown20);
                            Console.WriteLine("mddf.unknown21 = 0x{0:X8} ({0})", mddf.unknown21);
                            Console.WriteLine("mddf.unknown22 = 0x{0:X8} ({0})", mddf.unknown22);
                            Console.WriteLine("mddf.unknown23 = 0x{0:X8} ({0})", mddf.unknown23);
                            Console.WriteLine("mddf.unknown24 = 0x{0:X8} ({0})", mddf.unknown24);
                            Console.WriteLine("mddf.unknown25 = 0x{0:X8} ({0})", mddf.unknown25);
                            Console.WriteLine("mddf.unknown26 = 0x{0:X8} ({0})", mddf.unknown26);
                            Console.WriteLine("mddf.unknown27 = 0x{0:X8} ({0})", mddf.unknown27);
                            Console.WriteLine("mddf.unknown28 = 0x{0:X8} ({0})", mddf.unknown28);
                            Console.WriteLine("mddf.unknown29 = 0x{0:X8} ({0})", mddf.unknown29);
                            Console.WriteLine("mddf.unknown30 = 0x{0:X8} ({0})", mddf.unknown30);
                            Console.WriteLine("mddf.unknown31 = 0x{0:X8} ({0})", mddf.unknown31);
                            Console.WriteLine("mddf.unknown32 = 0x{0:X8} ({0})", mddf.unknown32);
                            Console.WriteLine("mddf.unknown33 = 0x{0:X8} ({0})", mddf.unknown33);
                            Console.WriteLine("mddf.unknown34 = 0x{0:X8} ({0})", mddf.unknown34);
                            Console.WriteLine("mddf.unknown35 = 0x{0:X8} ({0})", mddf.unknown35);
                            Console.WriteLine("mddf.unknown36 = 0x{0:X8} ({0})", mddf.unknown36);
                            Console.WriteLine("mddf.unknown37 = 0x{0:X8} ({0})", mddf.unknown37);
                            Console.WriteLine("mddf.unknown38 = 0x{0:X8} ({0})", mddf.unknown38);
                            Console.WriteLine("mddf.unknown_timestamp = 0x{0:X8} ({0}, {1})", mddf.unknown_timestamp, DateHandlers.LisaToDateTime(mddf.unknown_timestamp));
                        }

                        if (mddf.mddf_block != i)
                            return;

                        if (mddf.vol_size > imagePlugin.GetSectors())
                            return;

                        if (mddf.vol_size - 1 != mddf.volsize_minus_one)
                            return;

                        if (mddf.vol_size - i - 1 != mddf.volsize_minus_mddf_minus_one)
                            return;

                        if (mddf.datasize > mddf.blocksize)
                            return;

                        if (mddf.blocksize < imagePlugin.GetSectorSize())
                            return;

                        if (mddf.datasize != imagePlugin.GetSectorSize())
                            return;

                        switch (mddf.fsversion)
                        {
                            case LisaFSv1:
                                sb.AppendLine("LisaFS v1");
                                break;
                            case LisaFSv2:
                                sb.AppendLine("LisaFS v2");
                                break;
                            case LisaFSv3:
                                sb.AppendLine("LisaFS v1");
                                break;
                            default:
                                sb.AppendFormat("Uknown LisaFS version {0}", mddf.fsversion).AppendLine();
                                break;
                        }

                        sb.AppendFormat("Volume name: \"{0}\"", mddf.volname).AppendLine();
                        sb.AppendFormat("Volume password: \"{0}\"", mddf.password).AppendLine();
                        sb.AppendFormat("Volume ID: 0x{0:X16}", mddf.volid).AppendLine();
                        sb.AppendFormat("Backup volume ID: 0x{0:X16}", mddf.backup_volid).AppendLine();

                        sb.AppendFormat("Master copy ID: 0x{0:X8}", mddf.master_copy_id).AppendLine();

                        sb.AppendFormat("Volume is number {0} of {1}", mddf.volnum, mddf.vol_sequence).AppendLine();

                        sb.AppendFormat("Serial number of Lisa computer that created this volume: {0}", mddf.machine_id).AppendLine();
                        sb.AppendFormat("Serial number of Lisa computer that can use this volume's software {0}", mddf.serialization).AppendLine();

                        sb.AppendFormat("Volume created on {0}", mddf.dtvc).AppendLine();
                        sb.AppendFormat("Some timestamp, says {0}", mddf.dtcc).AppendLine();
                        sb.AppendFormat("Volume backed up on {0}", mddf.dtvb).AppendLine();
                        sb.AppendFormat("Volume scavenged on {0}", mddf.dtvs).AppendLine();
                        sb.AppendFormat("MDDF is in block {0}", mddf.mddf_block).AppendLine();
                        sb.AppendFormat("{0} blocks minus one", mddf.volsize_minus_one).AppendLine();
                        sb.AppendFormat("{0} blocks minus one minus MDDF offset", mddf.volsize_minus_mddf_minus_one).AppendLine();
                        sb.AppendFormat("{0} blocks in volume", mddf.vol_size).AppendLine();
                        sb.AppendFormat("{0} bytes per sector (uncooked)", mddf.blocksize).AppendLine();
                        sb.AppendFormat("{0} bytes per sector", mddf.datasize).AppendLine();
                        sb.AppendFormat("{0} blocks per cluster", mddf.clustersize).AppendLine();
                        sb.AppendFormat("{0} blocks in filesystem", mddf.fs_size).AppendLine();
                        sb.AppendFormat("{0} files in volume", mddf.filecount).AppendLine();
                        sb.AppendFormat("{0} blocks free", mddf.freecount).AppendLine();
                        sb.AppendFormat("{0} bytes in LisaInfo", mddf.label_size).AppendLine();
                        sb.AppendFormat("Filesystem overhead: {0}", mddf.fs_overhead).AppendLine();
                        sb.AppendFormat("Scanvenger result code: 0x{0:X8}", mddf.result_scavenge).AppendLine();
                        sb.AppendFormat("Boot code: 0x{0:X8}", mddf.boot_code).AppendLine();
                        sb.AppendFormat("Boot environment:  0x{0:X8}", mddf.boot_environ).AppendLine();
                        sb.AppendFormat("Overmount stamp: 0x{0:X16}", mddf.overmount_stamp).AppendLine();

                        if (mddf.vol_left_mounted == 0)
                            sb.AppendLine("Volume is clean");
                        else
                            sb.AppendLine("Volume is dirty");

                        information = sb.ToString();

                        return;
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (LisaFS plugin): Exception {0}, {1}, {2}", ex.Message, ex.InnerException, ex.StackTrace);
                return;
            }
        }

        struct Lisa_MDDF
        {
            // 0x00, Filesystem version
            public UInt16 fsversion;
            // 0x02, Volume ID
            public UInt64 volid;
            // 0x0A, Volume sequence number
            public UInt16 volnum;
            // 0x0C, Pascal string, 32+1 bytes, volume name
            public string volname;
            // 0x2D, unknown, possible padding
            public byte unknown1;
            // 0x2E, Pascal string, 32+1 bytes, password
            public string password;
            // 0x4F, unknown, possible padding
            public byte unknown2;
            // 0x50, Lisa serial number that init'ed this disk
            public UInt32 machine_id;
            // 0x54, ID of the master copy ? no idea really
            public UInt32 master_copy_id;
            // 0x58, Date of volume creation
            public DateTime dtvc;
            // 0x5C, Date...
            public DateTime dtcc;
            // 0x60, Date of volume backup
            public DateTime dtvb;
            // 0x64, Date of volume scavenging
            public DateTime dtvs;
            // 0x68, unknown
            public UInt32 unknown3;
            // 0x6C, block the MDDF is residing on
            public UInt32 mddf_block;
            // 0x70, volsize-1
            public UInt32 volsize_minus_one;
            // 0x74, volsize-1-mddf_block
            public UInt32 volsize_minus_mddf_minus_one;
            // 0x78, Volume size in blocks
            public UInt32 vol_size;
            // 0x7C, Blocks size of underlying drive (data+tags)
            public UInt16 blocksize;
            // 0x7E, Data only block size
            public UInt16 datasize;
            // 0x80, unknown
            public UInt16 unknown4;
            // 0x82, unknown
            public UInt32 unknown5;
            // 0x86, unknown
            public UInt32 unknown6;
            // 0x8A, Size in sectors of filesystem clusters
            public UInt16 clustersize;
            // 0x8C, Filesystem size in blocks
            public UInt32 fs_size;
            // 0x90, unknown
            public UInt32 unknown7;
            // 0x94, unknown
            public UInt32 unknown8;
            // 0x98, unknown
            public UInt32 unknown9;
            // 0x9C, unknown
            public UInt32 unknown10;
            // 0xA0, unknown
            public UInt32 unknown11;
            // 0xA4, unknown
            public UInt32 unknown12;
            // 0xA8, unknown
            public UInt32 unknown13;
            // 0xAC, unknown
            public UInt32 unknown14;
            // 0xB0, Files in volume
            public UInt16 filecount;
            // 0xB2, unknown
            public UInt32 unknown15;
            // 0xB6, unknown
            public UInt32 unknown16;
            // 0xBA, Free blocks
            public UInt32 freecount;
            // 0xBE, unknown
            public UInt16 unknown17;
            // 0xC0, unknown
            public UInt32 unknown18;
            // 0xC4, no idea
            public UInt64 overmount_stamp;
            // 0xCC, serialization, lisa serial number authorized to use blocked software on this volume
            public UInt32 serialization;
            // 0xD0, unknown
            public UInt32 unknown19;
            // 0xD4, unknown, possible timestamp
            public UInt32 unknown_timestamp;
            // 0xD8, unknown
            public UInt32 unknown20;
            // 0xDC, unknown
            public UInt32 unknown21;
            // 0xE0, unknown
            public UInt32 unknown22;
            // 0xE4, unknown
            public UInt32 unknown23;
            // 0xE8, unknown
            public UInt32 unknown24;
            // 0xEC, unknown
            public UInt32 unknown25;
            // 0xF0, unknown
            public UInt32 unknown26;
            // 0xF4, unknown
            public UInt32 unknown27;
            // 0xF8, unknown
            public UInt32 unknown28;
            // 0xFC, unknown
            public UInt32 unknown29;
            // 0x100, unknown
            public UInt32 unknown30;
            // 0x104, unknown
            public UInt32 unknown31;
            // 0x108, unknown
            public UInt32 unknown32;
            // 0x10C, unknown
            public UInt32 unknown33;
            // 0x110, unknown
            public UInt32 unknown34;
            // 0x114, unknown
            public UInt32 unknown35;
            // 0x118, ID of volume where this volume was backed up
            public UInt64 backup_volid;
            // 0x120, Size of LisaInfo label
            public UInt16 label_size;
            // 0x122, not clear
            public UInt16 fs_overhead;
            // 0x124, Return code of Scavenger
            public UInt16 result_scavenge;
            // 0x126, No idea
            public UInt16 boot_code;
            // 0x128, No idea
            public UInt16 boot_environ;
            // 0x12A, unknown
            public UInt32 unknown36;
            // 0x12E, unknown
            public UInt32 unknown37;
            // 0x132, unknown
            public UInt32 unknown38;
            // 0x136, Total volumes in sequence
            public UInt16 vol_sequence;
            // 0x138, Volume is dirty?
            public byte vol_left_mounted;
            // Is password present? (On-disk position unknown)
            public byte passwd_present;
            // Opened files (memory-only?) (On-disk position unknown)
            public UInt32 opencount;
            // No idea (On-disk position unknown)
            public UInt32 copy_thread;
            // Flags are boolean, but Pascal seems to use them as full unsigned 8 bit values
            // No idea (On-disk position unknown)
            public byte privileged;
            // Read-only volume (On-disk position unknown)
            public byte write_protected;
            // Master disk (On-disk position unknown)
            public byte master;
            // Copy disk (On-disk position unknown)
            public byte copy;
            // No idea (On-disk position unknown)
            public byte copy_flag;
            // No idea (On-disk position unknown)
            public byte scavenge_flag;
        }

        struct Lisa_Tag
        {
            // 0x00 Unknown
            public UInt32 unknown1;
            // 0x04 File ID
            public UInt16 fileID;
            // 0x06 Unknown
            public UInt16 unknown2;
            // 0x08 Unknown
            public UInt32 unknown3;
        }
    }
}
