/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : PrintHex.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Helpers

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Prints a byte array as hexadecimal in console.
 
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

namespace DiscImageChef
{
    public static class PrintHex
    {
        public static void PrintHexArray(byte[] array, int width)
        {
            int counter = 0;
            int subcounter = 0;
            for (long i = 0; i < array.LongLength; i++)
            {
                if (counter == 0)
                {
                    Console.WriteLine();
                    Console.Write("{0:X16}   ", i);
                }
                else
                {
                    if (subcounter == 3 )
                    {
                        Console.Write("  ");
                        subcounter = 0;
                    }
                    else
                    {
                        Console.Write(" ");
                        subcounter++;
                    }
                }

                Console.Write("{0:X2}", array[i]);

                if (counter == width - 1)
                {
                    counter = 0;
                    subcounter = 0;
                }
                else
                    counter++;
            }
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}

