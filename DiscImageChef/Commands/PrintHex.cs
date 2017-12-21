// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PrintHex.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'printhex' verb.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.DiscImages;
using DiscImageChef.Filters;

namespace DiscImageChef.Commands
{
    static class PrintHex
    {
        internal static void DoPrintHex(PrintHexOptions options)
        {
            DicConsole.DebugWriteLine("PrintHex command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("PrintHex command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("PrintHex command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("PrintHex command", "--start={0}", options.StartSector);
            DicConsole.DebugWriteLine("PrintHex command", "--length={0}", options.Length);
            DicConsole.DebugWriteLine("PrintHex command", "--long-sectors={0}", options.LongSectors);
            DicConsole.DebugWriteLine("PrintHex command", "--WidthBytes={0}", options.WidthBytes);

            FiltersList filtersList = new FiltersList();
            Filter inputFilter = filtersList.GetFilter(options.InputFile);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");
                return;
            }

            ImagePlugin inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not verifying");
                return;
            }

            inputFormat.OpenImage(inputFilter);

            for(ulong i = 0; i < options.Length; i++)
            {
                DicConsole.WriteLine("Sector {0}", options.StartSector + i);

                if(inputFormat.ImageInfo.ReadableSectorTags == null)
                {
                    DicConsole
                        .WriteLine("Requested sectors with tags, unsupported by underlying image format, printing only user data.");
                    options.LongSectors = false;
                }
                else
                {
                    if(inputFormat.ImageInfo.ReadableSectorTags.Count == 0)
                    {
                        DicConsole
                            .WriteLine("Requested sectors with tags, unsupported by underlying image format, printing only user data.");
                        options.LongSectors = false;
                    }
                }

                byte[] sector = options.LongSectors ? inputFormat.ReadSectorLong(options.StartSector + i) : inputFormat.ReadSector(options.StartSector + i);

                DiscImageChef.PrintHex.PrintHexArray(sector, options.WidthBytes);
            }

            Core.Statistics.AddCommand("print-hex");
        }
    }
}