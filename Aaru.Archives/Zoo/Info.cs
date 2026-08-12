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

using System.IO;
using System.Globalization;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Spectre.Console;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Archives;

public sealed partial class Zoo
{
#region IArchive Members

    /// <inheritdoc />
    public bool Identify(IFilter filter)
    {
        if(filter.DataForkLength < Marshal.SizeOf<ZooHeader>()) return false;

        Stream stream = filter.GetDataForkStream();
        stream.Position = 0;

        var hdr = new byte[Marshal.SizeOf<ZooHeader>()];

        stream.ReadExactly(hdr, 0, hdr.Length);

        ZooHeader header = Marshal.ByteArrayToStructureLittleEndian<ZooHeader>(hdr);

        return header.zoo_tag == ZOO_TAG && header.zoo_start + header.zoo_minus == 0;
    }

    /// <inheritdoc />
    public void GetInformation(IFilter filter, Encoding encoding, out string information)
    {
        information = string.Empty;

        if(filter.DataForkLength < Marshal.SizeOf<ZooHeader>()) return;

        Stream stream = filter.GetDataForkStream();
        stream.Position = 0;

        var hdr = new byte[Marshal.SizeOf<ZooHeader>()];

        stream.ReadExactly(hdr, 0, hdr.Length);

        ZooHeader header = Marshal.ByteArrayToStructureLittleEndian<ZooHeader>(hdr);

        AaruLogging.Debug(MODULE_NAME,
                          "[navy]header.text[/] = [green]\"{0}\"[/]",
                          Markup.Escape(Encoding.UTF8.GetString(header.text).TrimEnd('\0')));

        AaruLogging.Debug(MODULE_NAME, "[navy]header.zoo_tag[/] = [teal]0x{0:X8}[/]", header.zoo_tag);
        AaruLogging.Debug(MODULE_NAME, "[navy]header.zoo_start[/] = [teal]{0}[/]",    header.zoo_start);
        AaruLogging.Debug(MODULE_NAME, "[navy]header.zoo_minus[/] = [teal]{0}[/]",    header.zoo_minus);
        AaruLogging.Debug(MODULE_NAME, "[navy]header.major_ver[/] = [teal]{0}[/]",    header.major_ver);
        AaruLogging.Debug(MODULE_NAME, "[navy]header.minor_ver[/] = [teal]{0}[/]",    header.minor_ver);
        AaruLogging.Debug(MODULE_NAME, "[navy]header.type[/] = [teal]{0}[/]",         header.type);
        AaruLogging.Debug(MODULE_NAME, "[navy]header.acmt_pos[/] = [teal]{0}[/]",     header.acmt_pos);
        AaruLogging.Debug(MODULE_NAME, "[navy]header.acmt_len[/] = [teal]{0}[/]",     header.acmt_len);
        AaruLogging.Debug(MODULE_NAME, "[navy]header.vdata[/] = [teal]0x{0:X4}[/]",   header.vdata);

        var sb = new StringBuilder();
        sb.AppendLine("[bold][blue]Zoo archive:[/][/]");

        sb.Append(CultureInfo.InvariantCulture,
                  $"[slateblue1]Header text:[/] [green]\"{Markup.Escape(Encoding.UTF8.GetString(header.text).TrimEnd('\0'))}\"[/]")
          .AppendLine();

        sb.Append(CultureInfo.InvariantCulture, $"[slateblue1]Start of archive:[/] [teal]{header.zoo_start}[/]")
          .AppendLine();

        sb.Append(CultureInfo.InvariantCulture,
                  $"[slateblue1]Version required to extract all files:[/] [teal]{header.major_ver}.{header.minor_ver}[/]")
          .AppendLine();

        sb.Append(CultureInfo.InvariantCulture, $"[slateblue1]Archive type:[/] [teal]{header.type}[/]").AppendLine();

        if(header is { acmt_len: > 0, acmt_pos: >= 0 } && header.acmt_pos + header.acmt_len <= stream.Length)
        {
            var buffer = new byte[header.acmt_len];
            stream.Position =   header.acmt_pos;
            encoding        ??= Encoding.UTF8;
            stream.ReadExactly(buffer, 0, buffer.Length);
            sb.AppendLine("[slateblue1]Archive comment:[/]");

            sb.Append(CultureInfo.InvariantCulture,
                      $"[rosybrown]{Markup.Escape(StringHandlers.CToString(buffer, encoding))}[/]")
              .AppendLine();
        }

        information = sb.ToString();
    }

#endregion
}