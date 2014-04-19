/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : Swapping.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Program tools

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Byte-swapping methods
 
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

namespace FileSystemIDandChk
{
    static class Swapping
    {
        public static byte[] SwapTenBytes(byte[] source)
        {
            byte[] destination = new byte[8];

            destination[0] = source[9];
            destination[1] = source[8];
            destination[2] = source[7];
            destination[3] = source[6];
            destination[4] = source[5];
            destination[5] = source[4];
            destination[6] = source[3];
            destination[7] = source[2];
            destination[8] = source[1];
            destination[9] = source[0];

            return destination;
        }

        public static byte[] SwapEightBytes(byte[] source)
        {
            byte[] destination = new byte[8];

            destination[0] = source[7];
            destination[1] = source[6];
            destination[2] = source[5];
            destination[3] = source[4];
            destination[4] = source[3];
            destination[5] = source[2];
            destination[6] = source[1];
            destination[7] = source[0];

            return destination;
        }

        public static byte[] SwapFourBytes(byte[] source)
        {
            byte[] destination = new byte[4];

            destination[0] = source[3];
            destination[1] = source[2];
            destination[2] = source[1];
            destination[3] = source[0];

            return destination;
        }

        public static byte[] SwapTwoBytes(byte[] source)
        {
            byte[] destination = new byte[2];

            destination[0] = source[1];
            destination[1] = source[0];

            return destination;
        }

        public static UInt32 PDPFromLittleEndian(UInt32 x)
        {
            return ((x & 0xffff) << 16) | ((x & 0xffff0000) >> 16);
        }

        public static UInt32 PDPFromBigEndian(UInt32 x)
        {
            return ((x & 0xff00ff) << 8) | ((x & 0xff00ff00) >> 8);
        }
    }
}
