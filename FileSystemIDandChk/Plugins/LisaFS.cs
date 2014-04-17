using System;
using System.Text;
using FileSystemIDandChk;

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

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset)
        {
            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset, out string information)
        {
            information = "";

            return;

            /*            information = sb.ToString();

            return; */
        }

        struct Lisa_MDDF
        {
            // Lisa serial number that init'ed this disk
            public UInt32 machine_id;
            // Return code of Scavenger
            public UInt32 result_scavenge;
            // Is password present?
            public byte passwd_present;
            // Filesystem size in blocks
            public UInt32 fs_size;
            // Volume size in blocks
            public UInt32 vol_size;
            // Opened files (memory-only?(
            public UInt32 opencount;
            // Pascal string, 32+1 bytes, volume name
            public string volname;
            // Pascal string, 32+1 bytes, password
            public string password;
            // Filesystem version
            public UInt16 fsversion;
            // Volume sequence number
            public UInt16 volnum;
            // Volume ID
            public UInt64 volid;
            // ID of volume where this volume was backed up
            public UInt64 backup_volid;
            // Total volumes in sequence
            public UInt16 vol_sequence;
            // Blocks size of underlying drive (data+tags)
            public UInt16 blocksize;
            // Data only block size
            public UInt16 datasize;
            // Size of filesystem clusters
            public UInt16 clustersize;
            // Files in volume
            public UInt16 filecount;
            // Size of LisaInfo label
            public UInt16 label_size;
            // Free blocks
            public UInt32 freecount;
            // Date of volume creation
            public DateTime dtvc;
            // Date...
            public DateTime dtcc;
            // Date of volume backup
            public DateTime dtvb;
            // Date of volume scavenging
            public DateTime dtvs;
            // Lisa serial number allowed to use this disk
            public UInt32 master_copy_id;
            // No idea
            public UInt32 copy_thread;
            // No idea
            public UInt64 overmount_stamp;
            // No idea
            public UInt16 boot_code;
            // No idea
            public UInt16 boot_environ;
            // Flags are boolean, but Pascal seems to use them as full unsigned 8 bit values
            // No idea
            public byte privileged;
            // Read-only volume
            public byte write_protected;
            // Master disk
            public byte master;
            // Copy disk
            public byte copy;
            // No idea
            public byte copy_flag;
            // No idea
            public byte scavenge_flag;
            // Volume is dirty?
            public byte vol_left_mounted;
        }

        struct Lisa_Tag
        {
            public UInt32 unknown1;
            // 0x00 Unknown
            public UInt16 fileID;
            // 0x04 File ID
            public UInt16 unknown2;
            // 0x06 Unknown
            public UInt32 unknown3;
            // 0x08 Unknown
        }
    }
}
