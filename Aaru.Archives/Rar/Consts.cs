namespace Aaru.Archives;

public sealed partial class Rar
{
    /// <summary>Minimum file size to contain a valid RAR header.</summary>
    const int MIN_HEADER_SIZE = 7;

    /// <summary>Maximum Windows FILETIME value convertible to a <see cref="System.DateTime" />.</summary>
    const long MAX_FILETIME = 2650467743999999999;

    // ======================================================================
    // RAR 1.x-4.x block header constants
    // ======================================================================

    /// <summary>Flag indicating the block has a data size field (LONG_BLOCK).</summary>
    const ushort RARFLAG_LONG_BLOCK = 0x8000;

    // Archive header flags (MHD_*)
    const ushort MHD_VOLUME       = 0x0001;
    const ushort MHD_COMMENT      = 0x0002;
    const ushort MHD_LOCK         = 0x0004;
    const ushort MHD_SOLID        = 0x0008;
    const ushort MHD_NEWNUMBERING = 0x0010;
    const ushort MHD_AV           = 0x0020;
    const ushort MHD_PROTECT      = 0x0040;
    const ushort MHD_PASSWORD     = 0x0080;
    const ushort MHD_FIRSTVOLUME  = 0x0100;
    const ushort MHD_ENCRYPTVER   = 0x0200;

    // File header flags (LHD_*)
    const ushort LHD_SPLIT_BEFORE = 0x0001;
    const ushort LHD_SPLIT_AFTER  = 0x0002;
    const ushort LHD_PASSWORD     = 0x0004;
    const ushort LHD_COMMENT      = 0x0008;
    const ushort LHD_SOLID        = 0x0010;
    const ushort LHD_LARGE        = 0x0100;
    const ushort LHD_UNICODE      = 0x0200;
    const ushort LHD_SALT         = 0x0400;
    const ushort LHD_VERSION      = 0x0800;
    const ushort LHD_EXTTIME      = 0x1000;

    /// <summary>Mask for the window size / directory indicator bits in file header flags.</summary>
    const ushort LHD_WINDOWMASK = 0x00E0;

    /// <summary>Value indicating a directory entry (all window bits set).</summary>
    const ushort LHD_DIRECTORY = 0x00E0;

    /// <summary>MS-DOS directory attribute bit.</summary>
    const uint ATTR_DIRECTORY = 0x10;

    // ======================================================================
    // RAR 5.0 constants
    // ======================================================================

    // RAR 5.0 archive header flags
    const ulong RAR5_ARCHIVE_VOLUME        = 0x0001;
    const ulong RAR5_ARCHIVE_VOLUME_NUMBER = 0x0002;
    const ulong RAR5_ARCHIVE_SOLID         = 0x0004;
    const ulong RAR5_ARCHIVE_RECOVERY      = 0x0008;
    const ulong RAR5_ARCHIVE_LOCKED        = 0x0010;

    // RAR 5.0 block flags
    const ulong RAR5_BLOCK_HAS_EXTRA = 0x0001;
    const ulong RAR5_BLOCK_HAS_DATA  = 0x0002;

    // RAR 5.0 file header flags
    const ulong RAR5_FILE_IS_DIRECTORY      = 0x0001;
    const ulong RAR5_FILE_HAS_MTIME         = 0x0002;
    const ulong RAR5_FILE_HAS_CRC32         = 0x0004;
    const ulong RAR5_FILE_UNPACKED_SIZE_UNK = 0x0008;

    // RAR 5.0 block-level continuation flags
    const ulong RAR5_BLOCK_SPLIT_BEFORE = 0x0008;
    const ulong RAR5_BLOCK_SPLIT_AFTER  = 0x0010;

    // RAR 5.0 extra record types
    const ulong RAR5_EXTRA_FILE_ENCRYPTION = 0x01;
    const ulong RAR5_EXTRA_FILE_HASH       = 0x02;
    const ulong RAR5_EXTRA_FILE_TIME       = 0x03;
    const ulong RAR5_EXTRA_FILE_VERSION    = 0x04;
    const ulong RAR5_EXTRA_FILE_REDIR      = 0x05;
    const ulong RAR5_EXTRA_UNIX_OWNER      = 0x06;

    // RAR 5.0 time flags
    const ulong RAR5_TIME_IS_UNIX    = 0x0001;
    const ulong RAR5_TIME_HAS_MTIME  = 0x0002;
    const ulong RAR5_TIME_HAS_CTIME  = 0x0004;
    const ulong RAR5_TIME_HAS_ATIME  = 0x0008;
    const ulong RAR5_TIME_IS_NANOSEC = 0x0010;
    /// <summary>RAR 1.x-4.x file signature (7 bytes): Rar!\x1a\x07\x00</summary>
    static readonly byte[] RAR4_SIGNATURE =
    {
        0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00
    };

    /// <summary>RAR 5.0 file signature (8 bytes): Rar!\x1a\x07\x01\x00</summary>
    static readonly byte[] RAR5_SIGNATURE =
    {
        0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00
    };
}