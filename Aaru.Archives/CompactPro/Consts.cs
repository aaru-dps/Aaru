namespace Aaru.Archives;

public sealed partial class CompactPro
{
    /// <summary>Maximum accepted directory nesting depth</summary>
    const int MAX_DIRECTORY_DEPTH = 255;

    const int MIN_HEADER_SIZE = 8;

    /// <summary>First byte of a Compact Pro archive is always 0x01.</summary>
    const byte MARKER = 0x01;

    /// <summary>Encryption flag (bit 0 of entry flags).</summary>
    const ushort FLAG_ENCRYPTED = 0x01;

    /// <summary>LZH compression for resource fork (bit 1 of entry flags).</summary>
    const ushort FLAG_RESOURCE_LZH = 0x02;

    /// <summary>LZH compression for data fork (bit 2 of entry flags).</summary>
    const ushort FLAG_DATA_LZH = 0x04;

    /// <summary>Bit 7 in name length byte indicates a directory entry.</summary>
    const byte NAME_DIRECTORY_FLAG = 0x80;

    /// <summary>Mask for the actual name length (lower 7 bits).</summary>
    const byte NAME_LENGTH_MASK = 0x7F;

    /// <summary>Size of file entry metadata (excluding name) in bytes.</summary>
    const int FILE_METADATA_SIZE = 45;

    /// <summary>Size of directory entry metadata (excluding name) in bytes.</summary>
    const int DIR_METADATA_SIZE = 2;

    // XAttr name constants
    const string XATTR_COMMENT             = "comment";
    const string XATTR_APPLE_RESOURCE_FORK = "com.apple.ResourceFork";
    const string XATTR_APPLE_FINDER_INFO   = "com.apple.FinderInfo";
    const string XATTR_APPLE_HFS_TYPE      = "hfs.type";
    const string XATTR_APPLE_HFS_CREATOR   = "hfs.creator";
}