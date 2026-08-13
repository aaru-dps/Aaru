// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Open.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Ar plugin.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Logging;

namespace Aaru.Archives;

public sealed partial class Ar
{
    List<Entry> _entries;

    /// <summary>Parses an ASCII decimal number from a byte buffer, stopping at the first non-digit character.</summary>
    static long ParseDecimal(byte[] buffer, int offset, int length)
    {
        long val      = 0;
        var  hasDigit = false;

        for(int i = offset; i < offset + length; i++)
        {
            byte b = buffer[i];

            if(b < (byte)'0' || b > (byte)'9')
            {
                if(hasDigit) break;

                continue;
            }

            val      = val * 10 + (b - '0');
            hasDigit = true;
        }

        return val;
    }

    /// <summary>Parses an ASCII octal number from a byte buffer, stopping at the first non-octal character.</summary>
    static long ParseOctal(byte[] buffer, int offset, int length)
    {
        long val      = 0;
        var  hasDigit = false;

        for(int i = offset; i < offset + length; i++)
        {
            byte b = buffer[i];

            if(b < (byte)'0' || b > (byte)'7')
            {
                if(hasDigit) break;

                continue;
            }

            val      = val * 8 + (b - '0');
            hasDigit = true;
        }

        return val;
    }

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(filter.DataForkLength < MAGIC_LENGTH + HEADER_SIZE) return ErrorNumber.InvalidArgument;

        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;
        _encoding        = encoding ?? Encoding.UTF8;
        _entries         = [];

        // Skip archive magic
        _stream.Position = MAGIC_LENGTH;

        byte[] gnuFilenameTable = null;

        while(_stream.Position + HEADER_SIZE <= _stream.Length)
        {
            var header = new byte[HEADER_SIZE];
            int read   = _stream.Read(header, 0, HEADER_SIZE);

            if(read < HEADER_SIZE) break;

            // Validate terminator
            if(header[58] != TERMINATOR_BYTE_1 || header[59] != TERMINATOR_BYTE_2) break;

            long timestamp = ParseDecimal(header, TIMESTAMP_OFFSET, TIMESTAMP_LENGTH);
            var  uid       = (uint)ParseDecimal(header, UID_OFFSET, UID_LENGTH);
            var  gid       = (uint)ParseDecimal(header, GID_OFFSET, GID_LENGTH);
            var  mode      = (uint)ParseOctal(header, MODE_OFFSET, MODE_LENGTH);
            long size      = ParseDecimal(header, SIZE_OFFSET, SIZE_LENGTH);

            string filename;

            // BSD long filename: name field starts with "#1/"
            if(header[0] == (byte)'#' && header[1] == (byte)'1' && header[2] == (byte)'/')
            {
                var nameLen = (int)ParseDecimal(header, 3, 13);

                // The name must fit in the remaining file and inside the declared entry size
                if(nameLen < 0 || nameLen > _stream.Length - _stream.Position || nameLen > size) break;

                var nameBuf = new byte[nameLen];
                _stream.ReadExactly(nameBuf, 0, nameLen);

                size -= nameLen;

                // Trim trailing null bytes
                int trimmedLen = nameLen;

                while(trimmedLen > 0 && nameBuf[trimmedLen - 1] == 0) trimmedLen--;

                filename = _encoding.GetString(nameBuf, 0, trimmedLen);
            }

            // GNU symbol table: name field is "/ " (slash + spaces)
            else if(header[0] == (byte)'/' && header[1] == (byte)' ')
            {
                // Skip symbol table data
                _stream.Position = _stream.Position + size + 1 & ~1L;

                continue;
            }

            // GNU long filename table: name field is "// " (double slash + spaces)
            else if(header[0] == (byte)'/' && header[1] == (byte)'/' && header[2] == (byte)' ')
            {
                // The table must fit in the remaining file
                if(size < 0 || size > _stream.Length - _stream.Position) break;

                gnuFilenameTable = new byte[size];
                _stream.ReadExactly(gnuFilenameTable, 0, (int)size);

                // Align to 2-byte boundary
                if((_stream.Position & 1) != 0) _stream.Position++;

                continue;
            }

            // GNU long filename reference: name field starts with "/" followed by a digit
            else if(header[0] == (byte)'/' && header[1] >= (byte)'0' && header[1] <= (byte)'9')
            {
                var nameOffset = (int)ParseDecimal(header, 1, 15);

                if(gnuFilenameTable is null || nameOffset >= gnuFilenameTable.Length) break;

                int endOffset = nameOffset;

                while(endOffset                   < gnuFilenameTable.Length &&
                      gnuFilenameTable[endOffset] != (byte)'\n'             &&
                      gnuFilenameTable[endOffset] != (byte)'/')
                    endOffset++;

                filename = _encoding.GetString(gnuFilenameTable, nameOffset, endOffset - nameOffset);
            }

            // Regular entry: trim trailing spaces and slashes from the 16-byte name field
            else
            {
                int nameLen = NAME_LENGTH;

                while(nameLen > 0 && (header[nameLen - 1] == (byte)' ' || header[nameLen - 1] == (byte)'/')) nameLen--;

                filename = _encoding.GetString(header, NAME_OFFSET, nameLen);
            }

            long dataOffset = _stream.Position;

            AaruLogging.Debug(MODULE_NAME, "[blue]filename[/] = [green]\"{0}\"[/]",  filename);
            AaruLogging.Debug(MODULE_NAME, "[blue]size[/] = [teal]{0}[/]",           size);
            AaruLogging.Debug(MODULE_NAME, "[blue]mode[/] = [teal]0{0}[/]",          Convert.ToString(mode, 8));
            AaruLogging.Debug(MODULE_NAME, "[blue]uid[/] = [teal]{0}[/]",            uid);
            AaruLogging.Debug(MODULE_NAME, "[blue]gid[/] = [teal]{0}[/]",            gid);
            AaruLogging.Debug(MODULE_NAME, "[blue]dataOffset[/] = [teal]0x{0:X}[/]", dataOffset);

            _entries.Add(new Entry
            {
                Filename         = filename,
                Size             = size,
                DataOffset       = dataOffset,
                Mode             = mode,
                Uid              = uid,
                Gid              = gid,
                LastWriteTimeUtc = DateTimeOffset
                                  .FromUnixTimeSeconds(timestamp is < 0 or > MAX_UNIX_TIME ? 0 : timestamp)
                                  .UtcDateTime
            });

            // Seek to next entry, aligned to 2-byte boundary
            _stream.Position = dataOffset + size + 1 & ~1L;
        }

        Opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void Close()
    {
        if(!Opened) return;

        _stream?.Close();
        _stream = null;
        Opened  = false;
    }

#endregion
}