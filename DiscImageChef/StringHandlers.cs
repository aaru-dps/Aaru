/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : StringHandlers.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Program tools

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Convert byte arrays to C# strings.
 
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
using System.Text;

namespace DiscImageChef
{
    public static class StringHandlers
    {
        public static string CToString(byte[] CString)
        {
            StringBuilder sb = new StringBuilder();
			
            for (int i = 0; i < CString.Length; i++)
            {
                if (CString[i] == 0)
                    break;

                sb.Append(Encoding.ASCII.GetString(CString, i, 1));
            }
			
            return sb.ToString();
        }

        public static string PascalToString(byte[] PascalString)
        {
            StringBuilder sb = new StringBuilder();

            byte length = PascalString[0];

            for (int i = 1; i < length + 1; i++)
            {
                sb.Append(Encoding.ASCII.GetString(PascalString, i, 1));
            }

            return sb.ToString();
        }
    }
}

