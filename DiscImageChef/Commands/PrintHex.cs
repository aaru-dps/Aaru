/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : PrintHex.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Verbs.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Implements the 'printhex' verb.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.Commands
{
    public static class PrintHex
    {
        public static void doPrintHex(PrintHexSubOptions options)
        {
            if (MainClass.isDebug)
            {
                Console.WriteLine("--debug={0}", options.Debug);
                Console.WriteLine("--verbose={0}", options.Verbose);
                Console.WriteLine("--input={0}", options.InputFile);
                Console.WriteLine("--start={0}", options.StartSector);
                Console.WriteLine("--length={0}", options.Length);
                Console.WriteLine("--long-sectors={0}", options.LongSectors);
                Console.WriteLine("--WidthBytes={0}", options.WidthBytes);
            }

            ImagePlugin inputFormat = ImageFormat.Detect(options.InputFile);

            if (inputFormat == null)
            {
                Console.WriteLine("Unable to recognize image format, not verifying");
                return;
            }

            inputFormat.OpenImage(options.InputFile);

            for(ulong i = 0; i < options.Length; i++)
            {
                Console.WriteLine("Sector {0}", options.StartSector + i);
                byte[] sector;

                if (inputFormat.ImageInfo.readableSectorTags == null)
                {
                    Console.WriteLine("Requested sectors with tags, unsupported by underlying image format, printing only user data.");
                    options.LongSectors = false;
                }
                else
                {
                    if (inputFormat.ImageInfo.readableSectorTags.Count == 0)
                    {
                        Console.WriteLine("Requested sectors with tags, unsupported by underlying image format, printing only user data.");
                        options.LongSectors = false;
                    }
                }

                if (options.LongSectors)
                    sector = inputFormat.ReadSectorLong(options.StartSector + i);
                else
                    sector = inputFormat.ReadSector(options.StartSector + i);

                DiscImageChef.PrintHex.PrintHexArray(sector, options.WidthBytes);
            }
        }
    }
}

