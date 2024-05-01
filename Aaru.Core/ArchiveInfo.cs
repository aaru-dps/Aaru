// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ArchiveInfo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Prints image information to console.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Spectre.Console;

namespace Aaru.Core;

/// <summary>Image information operations</summary>
public static class ArchiveInfo
{
    const string MODULE_NAME = "Archive information";

    /// <summary>Prints archive information to console</summary>
    /// <param name="imageFormat">Archive</param>
    public static void PrintArchiveInfo(IArchive imageFormat, IFilter filter, Encoding encoding)
    {
        AaruConsole.WriteLine(Localization.Core.Archive_Information_With_Markup);

        imageFormat.GetInformation(filter, encoding, out string information);

        AaruConsole.WriteLine(Markup.Escape(information));

        AaruConsole.WriteLine();
    }
}