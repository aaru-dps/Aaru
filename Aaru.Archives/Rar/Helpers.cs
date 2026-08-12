using System;
using System.IO;
using System.Text;

namespace Aaru.Archives;

public sealed partial class Rar
{
    /// <summary>Read a RAR 5.0 variable-length integer from the stream.</summary>
    /// <remarks>
    ///     Each byte contributes 7 payload bits (bits 0-6).
    ///     Bit 7 is the continuation flag: 1 means more bytes follow, 0 means last byte.
    ///     Payload bits are accumulated LSB-first.
    /// </remarks>
    static ulong ReadVint(Stream stream)
    {
        ulong result = 0;
        var   shift  = 0;

        for(;;)
        {
            int b = stream.ReadByte();

            if(b < 0) break;

            result |= (ulong)(b & 0x7F) << shift;

            if((b & 0x80) == 0) break;

            shift += 7;

            // Prevent infinite loop on malformed data (max 10 bytes = 70 bits)
            if(shift >= 70) break;
        }

        return result;
    }

    /// <summary>Convert an MS-DOS packed date/time value to a <see cref="DateTime" />.</summary>
    static DateTime DosToDateTime(uint dosDateTime)
    {
        int second = (int)(dosDateTime & 0x1F) * 2;
        var minute = (int)(dosDateTime >> 5  & 0x3F);
        var hour   = (int)(dosDateTime >> 11 & 0x1F);
        var day    = (int)(dosDateTime >> 16 & 0x1F);
        var month  = (int)(dosDateTime >> 21 & 0x0F);
        int year   = (int)(dosDateTime >> 25 & 0x7F) + 1980;

        if(day   < 1) day    = 1;
        if(month < 1) month  = 1;
        if(month > 12) month = 12;

        int maxDay = DateTime.DaysInMonth(year, month);

        if(day    > maxDay) day = maxDay;
        if(hour   > 23) hour    = 23;
        if(minute > 59) minute  = 59;
        if(second > 59) second  = 59;

        return new DateTime(year, month, day, hour, minute, second);
    }

    /// <summary>Decode a RAR 1.x-4.x Unicode filename from the raw header data.</summary>
    /// <remarks>
    ///     The raw data contains a null-terminated ASCII name followed by a compact
    ///     encoding of Unicode high bytes using 2-bit mode flags per character.
    /// </remarks>
    static string DecodeUnicodeFilename(byte[] nameData, int nameLength, Encoding encoding)
    {
        // Find the null terminator separating ASCII part from Unicode encoding
        var asciiEnd = 0;

        while(asciiEnd < nameLength && nameData[asciiEnd] != 0) asciiEnd++;

        if(asciiEnd >= nameLength) return encoding.GetString(nameData, 0, nameLength);

        // Part 2 starts after the null terminator
        int uniPos   = asciiEnd + 1;
        var flagBits = 0;
        var flagByte = 0;
        var highByte = 0;
        var asciiPos = 0;
        var result   = new StringBuilder(asciiEnd + 16);

        if(uniPos < nameLength) highByte = nameData[uniPos++] << 8;

        while(asciiPos < asciiEnd)
        {
            if(flagBits == 0)
            {
                if(uniPos >= nameLength) break;

                flagByte = nameData[uniPos++];
                flagBits = 8;
            }

            flagBits -= 2;
            int mode = flagByte >> flagBits & 3;

            switch(mode)
            {
                case 0:
                    // Low byte from ASCII part, use stored high byte
                    result.Append((char)(highByte | nameData[asciiPos++]));

                    break;

                case 1:
                    // Read one byte, combine with high byte
                    if(uniPos >= nameLength) goto done;

                    result.Append((char)(highByte | nameData[uniPos++]));
                    asciiPos++;

                    break;

                case 2:
                    // Read two bytes as full LE character
                    if(uniPos + 1 >= nameLength) goto done;

                    result.Append((char)(nameData[uniPos] | nameData[uniPos + 1] << 8));
                    uniPos += 2;
                    asciiPos++;

                    break;

                case 3:
                    // Repetition from ASCII string
                    if(uniPos >= nameLength) goto done;

                    int len = nameData[uniPos++];

                    if((len & 0x80) != 0)
                    {
                        // Read correction byte and repeat with high byte adjustment
                        if(uniPos >= nameLength) goto done;

                        byte correction = nameData[uniPos++];
                        int  count      = (len & 0x7F) + 2;

                        for(var i = 0; i < count && asciiPos < asciiEnd; i++, asciiPos++)
                            result.Append((char)(highByte | nameData[asciiPos] + correction & 0xFF));
                    }
                    else
                    {
                        // Repeat len+2 bytes as-is from ASCII string
                        int count = (len & 0x7F) + 2;

                        for(var i = 0; i < count && asciiPos < asciiEnd; i++, asciiPos++)
                            result.Append((char)nameData[asciiPos]);
                    }

                    break;
            }
        }

    done:

        return result.ToString();
    }

    /// <summary>Parse extended time fields from a RAR 1.x-4.x file header.</summary>
    /// <remarks>
    ///     The extended time block encodes optional modification, creation, and access timestamps
    ///     with sub-second precision. Each timestamp has a 4-bit flags field:
    ///     bits 0-1 = number of extra bytes (0-3), bit 2 = has seconds count,
    ///     bit 3 = replaces DOS timestamp entirely (only for mtime).
    /// </remarks>
    static void ParseExtendedTime(byte[] headerData, int offset, int length, ref Entry entry)
    {
        if(offset + 2 > length) return;

        var flags = BitConverter.ToUInt16(headerData, offset);
        int pos   = offset + 2;

        // Modification time flags (bits 12-15). The timestamp itself comes from the base header,
        // only the odd second bit and sub-second precision bytes are stored here.
        int mtimeFlags = flags >> 12 & 0x0F;

        if((mtimeFlags & 0x08) != 0)
        {
            if((mtimeFlags & 0x04) != 0) entry.LastWriteTime = entry.LastWriteTime.AddSeconds(1);

            // Skip sub-second precision bytes
            pos += mtimeFlags & 0x03;
        }

        // Creation time flags (bits 8-11)
        int ctimeFlags = flags >> 8 & 0x0F;

        if((ctimeFlags & 0x08) != 0)
        {
            if(pos + 4 > length) return;

            entry.CreationTime =  DosToDateTime(BitConverter.ToUInt32(headerData, pos));
            pos                += 4;

            if((ctimeFlags & 0x04) != 0) entry.CreationTime = entry.CreationTime.AddSeconds(1);

            entry.HasCreationTime = true;

            // Skip sub-second precision bytes
            pos += ctimeFlags & 0x03;
        }

        // Last access time flags (bits 4-7)
        int atimeFlags = flags >> 4 & 0x0F;

        if((atimeFlags & 0x08) == 0 || pos + 4 > length) return;

        entry.LastAccessTime = DosToDateTime(BitConverter.ToUInt32(headerData, pos));

        if((atimeFlags & 0x04) != 0) entry.LastAccessTime = entry.LastAccessTime.AddSeconds(1);

        entry.HasLastAccessTime = true;
    }
}