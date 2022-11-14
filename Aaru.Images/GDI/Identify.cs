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
//     Identifies Dreamcast GDI disc images.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.DiscImages;

using System;
using System.IO;
using System.Text.RegularExpressions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

public sealed partial class Gdi
{
    // Due to .gdi format, this method must parse whole file, ignoring errors (those will be returned by OpenImage()).
    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        try
        {
            imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
            var testArray = new byte[512];
            imageFilter.GetDataForkStream().EnsureRead(testArray, 0, 512);
            imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);

            // Check for unexpected control characters that shouldn't be present in a text file and can crash this plugin
            var twoConsecutiveNulls = false;

            for(var i = 0; i < 512; i++)
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

            _gdiStream = new StreamReader(imageFilter.GetDataForkStream());
            var lineNumber  = 0;
            var tracksFound = 0;
            var tracks      = 0;

            while(_gdiStream.Peek() >= 0)
            {
                lineNumber++;
                string line = _gdiStream.ReadLine();

                if(lineNumber == 1)
                {
                    if(!int.TryParse(line, out tracks))
                        return false;
                }
                else
                {
                    var regexTrack = new Regex(REGEX_TRACK);

                    Match trackMatch = regexTrack.Match(line ?? "");

                    if(!trackMatch.Success)
                        return false;

                    tracksFound++;
                }
            }

            if(tracks == 0)
                return false;

            return tracks == tracksFound;
        }
        catch(Exception ex)
        {
            AaruConsole.ErrorWriteLine("Exception trying to identify image file {0}", imageFilter.BasePath);
            AaruConsole.ErrorWriteLine("Exception: {0}", ex.Message);
            AaruConsole.ErrorWriteLine("Stack trace: {0}", ex.StackTrace);

            return false;
        }
    }
}