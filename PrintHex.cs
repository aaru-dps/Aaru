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
using DiscImageChef.Console;

namespace DiscImageChef
{
    public static class PrintHex
    {
        public static void PrintHexArray(byte[] array, int width)
        {
            DicConsole.WriteLine(ByteArrayToHexArrayString(array, width));
        }

        public static string ByteArrayToHexArrayString(byte[] array, int width)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            int counter = 0;
            int subcounter = 0;
            for (long i = 0; i < array.LongLength; i++)
            {
                if (counter == 0)
                {
                    sb.AppendLine();
                    sb.AppendFormat("{0:X16}   ", i);
                }
                else
                {
                    if (subcounter == 3 )
                    {
                        sb.Append("  ");
                        subcounter = 0;
                    }
                    else
                    {
                        sb.Append(" ");
                        subcounter++;
                    }
                }

                sb.AppendFormat("{0:X2}", array[i]);

                if (counter == width - 1)
                {
                    counter = 0;
                    subcounter = 0;
                }
                else
                    counter++;
            }
            sb.AppendLine();
            sb.AppendLine();

            return sb.ToString();
        }
    }
}

