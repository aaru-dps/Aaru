// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies cdrdao cuesheets (toc/bin).
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

using System;
using System.IO;
using System.Text.RegularExpressions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class Cdrdao
{
    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        try
        {
            imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
            byte[] testArray = new byte[512];
            imageFilter.GetDataForkStream().EnsureRead(testArray, 0, 512);
            imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);

            // Check for unexpected control characters that shouldn't be present in a text file and can crash this plugin
            bool twoConsecutiveNulls = false;

            for(int i = 0; i < 512; i++)
            {
                if(i >= imageFilter.GetDataForkStream().Length)
                    break;

                if(testArray[i] == 0)
                {
                    if(twoConsecutiveNulls)
                        return false;

                    twoConsecutiveNulls = true;
                }
                else
                    twoConsecutiveNulls = false;

                if(testArray[i] < 0x20  &&
                   testArray[i] != 0x0A &&
                   testArray[i] != 0x0D &&
                   testArray[i] != 0x00)
                    return false;
            }

            _tocStream = new StreamReader(imageFilter.GetDataForkStream());

            var cr = new Regex(REGEX_COMMENT);
            var dr = new Regex(REGEX_DISCTYPE);

            while(_tocStream.Peek() >= 0)
            {
                string line = _tocStream.ReadLine();

                Match dm = dr.Match(line ?? "");
                Match cm = cr.Match(line ?? "");

                // Skip comments at start of file
                if(cm.Success)
                    continue;

                return dm.Success;
            }

            return false;
        }
        catch(Exception ex)
        {
            AaruConsole.ErrorWriteLine(Localization.Exception_trying_to_identify_image_file_0, _cdrdaoFilter.Filename);
            AaruConsole.ErrorWriteLine(Localization.Exception_0, ex.Message);
            AaruConsole.ErrorWriteLine(Localization.Stack_trace_0, ex.StackTrace);

            return false;
        }
    }
}