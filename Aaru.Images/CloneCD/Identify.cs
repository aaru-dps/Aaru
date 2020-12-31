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
//     Identifies CloneCD disc images.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text.RegularExpressions;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;

namespace Aaru.DiscImages
{
    public sealed partial class CloneCd
    {
        public bool Identify(IFilter imageFilter)
        {
            _ccdFilter = imageFilter;

            try
            {
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                byte[] testArray = new byte[512];
                imageFilter.GetDataForkStream().Read(testArray, 0, 512);
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

                _cueStream = new StreamReader(_ccdFilter.GetDataForkStream());

                string line = _cueStream.ReadLine();

                var hdr = new Regex(CCD_IDENTIFIER);

                Match hdm = hdr.Match(line ?? throw new InvalidOperationException());

                return hdm.Success;
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine("Exception trying to identify image file {0}", _ccdFilter);
                AaruConsole.ErrorWriteLine("Exception: {0}", ex.Message);
                AaruConsole.ErrorWriteLine("Stack trace: {0}", ex.StackTrace);

                return false;
            }
        }
    }
}