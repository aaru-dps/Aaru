namespace Aaru.Archives;

public sealed partial class StuffItX
{
    const int MIN_HEADER_SIZE = 10;

    /// <summary>Maximum accepted fork index in a stream, real archives only use a handful of forks</summary>
    const int MAX_FORK_INDEX = 32;

    const string XATTR_COMMENT             = "comment";
    const string XATTR_APPLE_RESOURCE_FORK = "com.apple.ResourceFork";
    const string XATTR_APPLE_FINDER_INFO   = "com.apple.FinderInfo";
    const string XATTR_APPLE_HFS_TYPE      = "hfs.type";
    const string XATTR_APPLE_HFS_CREATOR   = "hfs.creator";

    static readonly byte[] _signature =
    {
        (byte)'S', (byte)'t', (byte)'u', (byte)'f', (byte)'f', (byte)'I', (byte)'t', (byte)'!'
    };
}