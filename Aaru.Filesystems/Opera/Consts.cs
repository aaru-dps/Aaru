using Aaru.Helpers;

namespace Aaru.Filesystems
{
    public partial class OperaFS
    {
        const string SYNC       = "ZZZZZ";
        const uint   FLAGS_MASK = 0xFF;
        const int    MAX_NAME   = 32;

        /// <summary>
        ///     Directory
        /// </summary>
        const uint TYPE_DIR = 0x2A646972;
        /// <summary>
        ///     Disc label
        /// </summary>
        const uint TYPE_LBL = 0x2A6C626C;
        /// <summary>
        ///     Catapult
        /// </summary>
        const uint TYPE_ZAP = 0x2A7A6170;
        static readonly int DirectoryEntrySize = Marshal.SizeOf<DirectoryEntry>();

        enum FileFlags : uint
        {
            File             = 2,
            Special          = 6,
            Directory        = 7,
            LastEntryInBlock = 0x40000000,
            LastEntry        = 0x80000000
        }
    }
}