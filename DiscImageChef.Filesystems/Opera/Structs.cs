using System.Runtime.InteropServices;

namespace DiscImageChef.Filesystems
{
    public partial class OperaFS
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct OperaSuperBlock
        {
            /// <summary>0x000, Record type, must be 1</summary>
            public readonly byte record_type;
            /// <summary>0x001, 5 bytes, "ZZZZZ"</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] sync_bytes;
            /// <summary>0x006, Record version, must be 1</summary>
            public readonly byte record_version;
            /// <summary>0x007, Volume flags</summary>
            public readonly byte volume_flags;
            /// <summary>0x008, 32 bytes, volume comment</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] volume_comment;
            /// <summary>0x028, 32 bytes, volume label</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] volume_label;
            /// <summary>0x048, Volume ID</summary>
            public readonly int volume_id;
            /// <summary>0x04C, Block size in bytes</summary>
            public readonly int block_size;
            /// <summary>0x050, Blocks in volume</summary>
            public readonly int block_count;
            /// <summary>0x054, Root directory ID</summary>
            public readonly int root_dirid;
            /// <summary>0x058, Root directory blocks</summary>
            public readonly int rootdir_blocks;
            /// <summary>0x05C, Root directory block size</summary>
            public readonly int rootdir_bsize;
            /// <summary>0x060, Last root directory copy</summary>
            public readonly int last_root_copy;
        }
    }
}