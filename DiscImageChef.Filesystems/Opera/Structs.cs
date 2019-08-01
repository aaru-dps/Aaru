using System.Runtime.InteropServices;

namespace DiscImageChef.Filesystems
{
    public partial class OperaFS
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SuperBlock
        {
            /// <summary>0x000, Record type, must be 1</summary>
            public readonly byte record_type;
            /// <summary>0x001, 5 bytes, "ZZZZZ"</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public readonly byte[] sync_bytes;
            /// <summary>0x006, Record version, must be 1</summary>
            public readonly byte record_version;
            /// <summary>0x007, Volume flags</summary>
            public readonly byte volume_flags;
            /// <summary>0x008, 32 bytes, volume comment</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NAME)]
            public readonly byte[] volume_comment;
            /// <summary>0x028, 32 bytes, volume label</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NAME)]
            public readonly byte[] volume_label;
            /// <summary>0x048, Volume ID</summary>
            public readonly uint volume_id;
            /// <summary>0x04C, Block size in bytes</summary>
            public readonly uint block_size;
            /// <summary>0x050, Blocks in volume</summary>
            public readonly uint block_count;
            /// <summary>0x054, Root directory ID</summary>
            public readonly uint root_dirid;
            /// <summary>0x058, Root directory blocks</summary>
            public readonly uint rootdir_blocks;
            /// <summary>0x05C, Root directory block size</summary>
            public readonly uint rootdir_bsize;
            /// <summary>0x060, Last root directory copy</summary>
            public readonly uint last_root_copy;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DirectoryHeader
        {
            /// <summary>
            ///     Next block from this directory, -1 if last
            /// </summary>
            public readonly int next_block;
            /// <summary>
            ///     Previous block from this directory, -1 if first
            /// </summary>
            public readonly int prev_block;
            /// <summary>
            ///     Directory flags
            /// </summary>
            public readonly uint flags;
            /// <summary>
            ///     Offset to first free unused byte in the directory
            /// </summary>
            public readonly uint first_free;
            /// <summary>
            ///     Offset to first directory entry
            /// </summary>
            public readonly uint first_used;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DirectoryEntry
        {
            /// <summary>
            ///     File flags, see <see cref="FileFlags" />
            /// </summary>
            public readonly uint flags;
            /// <summary>
            ///     Unique file identifier
            /// </summary>
            public readonly uint id;
            /// <summary>
            ///     Entry type
            /// </summary>
            public readonly uint type;
            /// <summary>
            ///     Block size
            /// </summary>
            public readonly uint block_size;
            /// <summary>
            ///     Size in bytes
            /// </summary>
            public readonly uint byte_count;
            /// <summary>
            ///     Block count
            /// </summary>
            public readonly uint block_count;
            /// <summary>
            ///     Unknown
            /// </summary>
            public readonly uint burst;
            /// <summary>
            ///     Unknown
            /// </summary>
            public readonly uint gap;
            /// <summary>
            ///     Filename
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_NAME)]
            public readonly byte[] name;
            /// <summary>
            ///     Last copy
            /// </summary>
            public readonly uint last_copy;
        }

        struct DirectoryEntryWithPointers
        {
            public DirectoryEntry entry;
            public uint[]         pointers;
        }
    }
}