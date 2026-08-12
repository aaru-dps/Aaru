namespace Aaru.Archives;

public sealed partial class Lha
{
    const int MIN_HEADER_SIZE = 7;

    /// <summary>Maximum Windows FILETIME value convertible to a <see cref="System.DateTime" />.</summary>
    const long MAX_FILETIME = 2650467743999999999;
    const byte METHOD_DASH  = (byte)'-';
    const int  METHOD_LEN   = 5;

    // Extended header type IDs
    const byte EXT_HEADER_CRC        = 0x00;
    const byte EXT_FILENAME          = 0x01;
    const byte EXT_DIRECTORY         = 0x02;
    const byte EXT_COMMENT           = 0x3F;
    const byte EXT_DOS_ATTRS         = 0x40;
    const byte EXT_WINDOWS_TIMESTAMP = 0x41;
    const byte EXT_LARGE_FILE        = 0x42;
    const byte EXT_UNIX_PERMS        = 0x50;
    const byte EXT_UNIX_UIDGID       = 0x51;
    const byte EXT_UNIX_GROUP_NAME   = 0x52;
    const byte EXT_UNIX_USER_NAME    = 0x53;
    const byte EXT_UNIX_TIMESTAMP    = 0x54;
    const byte EXT_COMMENT_ALT       = 0x71;
    const byte EXT_COMBINED_UNIX     = 0x7F;
    const byte EXT_EXTENDED_UNIX     = 0xFF;
}