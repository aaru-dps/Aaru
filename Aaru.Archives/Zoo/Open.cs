// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Zoo plugin.
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
using Aaru.Helpers;
using Aaru.Logging;
using Spectre.Console;

namespace Aaru.Archives;

public sealed partial class Zoo
{
    List<Direntry> _files;

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(filter.DataForkLength < Marshal.SizeOf<ZooHeader>()) return ErrorNumber.InvalidArgument;

        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;
        _encoding        = encoding ?? Encoding.UTF8;

        _features = ArchiveSupportedFeature.HasEntryTimestamp |
                    ArchiveSupportedFeature.SupportsFilenames |
                    ArchiveSupportedFeature.SupportsSubdirectories;

        var hdr = new byte[Marshal.SizeOf<ZooHeader>()];

        _stream.ReadExactly(hdr, 0, hdr.Length);

        ZooHeader header = Marshal.ByteArrayToStructureLittleEndian<ZooHeader>(hdr);

        AaruLogging.Debug(MODULE_NAME,
                          "[blue]header.text[/] = [green]\"{0}\"[/]",
                          Markup.Escape(Encoding.UTF8.GetString(header.text).TrimEnd('\0')));

        AaruLogging.Debug(MODULE_NAME, "[blue]header.zoo_tag[/] = [teal]0x{0:X8}[/]", header.zoo_tag);
        AaruLogging.Debug(MODULE_NAME, "[blue]header.zoo_start[/] = [teal]{0}[/]",    header.zoo_start);
        AaruLogging.Debug(MODULE_NAME, "[blue]header.zoo_minus[/] = [teal]{0}[/]",    header.zoo_minus);
        AaruLogging.Debug(MODULE_NAME, "[blue]header.major_ver[/] = [teal]{0}[/]",    header.major_ver);
        AaruLogging.Debug(MODULE_NAME, "[blue]header.minor_ver[/] = [teal]{0}[/]",    header.minor_ver);
        AaruLogging.Debug(MODULE_NAME, "[blue]header.type[/] = [teal]{0}[/]",         header.type);
        AaruLogging.Debug(MODULE_NAME, "[blue]header.acmt_pos[/] = [teal]{0}[/]",     header.acmt_pos);
        AaruLogging.Debug(MODULE_NAME, "[blue]header.acmt_len[/] = [teal]{0}[/]",     header.acmt_len);
        AaruLogging.Debug(MODULE_NAME, "[blue]header.vdata[/] = [teal]0x{0:X4}[/]",   header.vdata);

        _files = [];

        AaruLogging.Debug(MODULE_NAME, "Seeking to [teal]{0} for first file.", header.zoo_start);

        // Reject negative or out of file offsets
        if(header.zoo_start < 0 || header.zoo_start > _stream.Length) return ErrorNumber.InvalidArgument;

        _stream.Position = header.zoo_start;

        Direntry entry;

        do
        {
            long entryStart = _stream.Position;

            var buf = new byte[Marshal.SizeOf<Direntry>()];

            if(_stream.Position + buf.Length >= _stream.Length) break;

            _stream.ReadExactly(buf, 0, buf.Length);

            entry = Marshal.ByteArrayToStructureLittleEndian<Direntry>(buf);

            var pos = 56; // dir_crc

            // Variable fields plus the fixed tail cannot extend past the read buffer
            if(pos + entry.namlen + entry.dirlen + 10 > buf.Length) return ErrorNumber.InvalidArgument;

            if(entry.namlen > 0)
            {
                entry.lfname = new byte[entry.namlen];
                Array.Copy(buf, pos, entry.lfname, 0, entry.namlen);
                pos += entry.namlen;
            }
            else
                entry.lfname = null;

            if(entry.dirlen > 0)
            {
                entry.dirname = new byte[entry.dirlen];
                Array.Copy(buf, pos, entry.dirname, 0, entry.dirlen);
                pos += entry.dirlen;
            }
            else
                entry.dirname = null;

            entry.system_id  =  BitConverter.ToUInt16(buf, pos);
            pos              += 2;
            entry.fattr      =  BitConverter.ToUInt32(buf, pos);
            pos              += 4;
            entry.vflag      =  BitConverter.ToUInt16(buf, pos);
            pos              += 2;
            entry.version_no =  BitConverter.ToUInt16(buf, pos);

            AaruLogging.Debug(MODULE_NAME, "[blue]entry.zoo_tag[/] = [teal]0x{0:X8}[/]",   entry.zoo_tag);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.type[/] = [teal]{0}[/]",           entry.type);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.packing_method[/] = [teal]{0}[/]", entry.packing_method);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.next[/] = [teal]{0}[/]",           entry.next);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.offset[/] = [teal]{0}[/]",         entry.offset);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.date[/] = [teal]{0}[/]",           entry.date);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.time[/] = [teal]{0}[/]",           entry.time);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.file_crc[/] = [teal]0x{0:X4}[/]",  entry.file_crc);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.org_size[/] = [teal]{0}[/]",       entry.org_size);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.size_now[/] = [teal]{0}[/]",       entry.size_now);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.major_ver[/] = [teal]{0}[/]",      entry.major_ver);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.minor_ver[/] = [teal]{0}[/]",      entry.minor_ver);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.deleted[/] = [teal]{0}[/]",        entry.deleted);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.struc[/] = [teal]{0}[/]",          entry.struc);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.comment[/] = [teal]{0}[/]",        entry.comment);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.cmt_size[/] = [teal]{0}[/]",       entry.cmt_size);

            AaruLogging.Debug(MODULE_NAME,
                              "[blue]entry.fname[/] = [green]\"{0}\"[/]",
                              StringHandlers.CToString(entry.fname, _encoding));

            AaruLogging.Debug(MODULE_NAME, "[blue]entry.var_dir_len[/] = [teal]{0}[/]",  entry.var_dir_len);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.tz[/] = [teal]{0}[/]",           entry.tz);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.dir_crc[/] = [teal]0x{0:X4}[/]", entry.dir_crc);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.namlen[/] = [teal]{0}[/]",       entry.namlen);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.dirlen[/] = [teal]{0}[/]",       entry.dirlen);

            AaruLogging.Debug(MODULE_NAME,
                              "[blue]entry.lfname[/] = [green]\"{0}\"[/]",
                              StringHandlers.CToString(entry.lfname, _encoding));

            AaruLogging.Debug(MODULE_NAME,
                              "[blue]entry.dirname[/] = [green]\"{0}\"[/]",
                              StringHandlers.CToString(entry.dirname, _encoding));

            AaruLogging.Debug(MODULE_NAME, "[blue]entry.system_id[/] = [teal]{0}[/]",  entry.system_id);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.fattr[/] = [teal]{0}[/]",      entry.fattr);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.vflag[/] = [teal]{0}[/]",      entry.vflag);
            AaruLogging.Debug(MODULE_NAME, "[blue]entry.version_no[/] = [teal]{0}[/]", entry.version_no);

            _files.Add(entry);

            if(entry.packing_method > 0) _features |= ArchiveSupportedFeature.SupportsCompression;
            if(entry.cmt_size       > 0) _features |= ArchiveSupportedFeature.SupportsXAttrs;

            if(entry.next > entryStart && entry.next < filter.DataForkLength)
            {
                AaruLogging.Debug(MODULE_NAME, "Seeking to [teal]{0}[/] for next file.", entry.next);
                _stream.Position = entry.next;
            }
            else
                break;
        } while(entry.next > 0);

        Opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void Close()
    {
        // Already closed
        if(!Opened) return;

        _stream?.Close();

        _stream = null;
        Opened  = false;
    }

#endregion
}