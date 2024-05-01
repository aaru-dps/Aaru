// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Verify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Verifies Apple DiskCopy 4.2 disk images.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class DiskCopy42
{
#region IVerifiableImage Members

    /// <inheritdoc />
    public bool? VerifyMediaImage()
    {
        var  data    = new byte[header.DataSize];
        var  tags    = new byte[header.TagSize];
        uint tagsChk = 0;

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_data);
        Stream dataStream = dc42ImageFilter.GetDataForkStream();
        dataStream.Seek(dataOffset, SeekOrigin.Begin);
        dataStream.EnsureRead(data, 0, (int)header.DataSize);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Calculating_data_checksum);
        uint dataChk = CheckSum(data);
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Calculated_data_checksum_equals_0_X8, dataChk);
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Stored_data_checksum_equals_0_X8,     header.DataChecksum);

        if(header.TagSize <= 0) return dataChk == header.DataChecksum && tagsChk == header.TagChecksum;

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_tags);
        Stream tagStream = dc42ImageFilter.GetDataForkStream();
        tagStream.Seek(tagOffset, SeekOrigin.Begin);
        tagStream.EnsureRead(tags, 0, (int)header.TagSize);

        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Calculating_tag_checksum);
        tagsChk = CheckSum(tags);
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Calculated_tag_checksum_equals_0_X8, tagsChk);
        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Stored_tag_checksum_equals_0_X8,     header.TagChecksum);

        return dataChk == header.DataChecksum && tagsChk == header.TagChecksum;
    }

#endregion
}