// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Acorn filesystem plugin.
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

using System.Collections.Generic;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of Acorn's Advanced Data Filing System (ADFS)</summary>
public sealed partial class AcornADFS
{
    static byte AcornMapChecksum(byte[] data, int length)
    {
        int sum   = 0;
        int carry = 0;

        if(length > data.Length)
            length = data.Length;

        // ADC r0, r0, r1
        // MOVS r0, r0, LSL #24
        // MOV r0, r0, LSR #24
        for(int i = length - 1; i >= 0; i--)
        {
            sum += data[i] + carry;

            if(sum > 0xFF)
            {
                carry =  1;
                sum   &= 0xFF;
            }
            else
                carry = 0;
        }

        return (byte)(sum & 0xFF);
    }

    static byte NewMapChecksum(byte[] mapBase)
    {
        uint rover;
        uint sumVector0 = 0;
        uint sumVector1 = 0;
        uint sumVector2 = 0;
        uint sumVector3 = 0;

        for(rover = (uint)(mapBase.Length - 4); rover > 0; rover -= 4)
        {
            sumVector0 += mapBase[rover + 0] + (sumVector3 >> 8);
            sumVector3 &= 0xff;
            sumVector1 += mapBase[rover + 1] + (sumVector0 >> 8);
            sumVector0 &= 0xff;
            sumVector2 += mapBase[rover + 2] + (sumVector1 >> 8);
            sumVector1 &= 0xff;
            sumVector3 += mapBase[rover + 3] + (sumVector2 >> 8);
            sumVector2 &= 0xff;
        }

        /*
                Don't add the check byte when calculating its value
        */
        sumVector0 += sumVector3 >> 8;
        sumVector1 += mapBase[1] + (sumVector0 >> 8);
        sumVector2 += mapBase[2] + (sumVector1 >> 8);
        sumVector3 += mapBase[3] + (sumVector2 >> 8);

        return (byte)((sumVector0 ^ sumVector1 ^ sumVector2 ^ sumVector3) & 0xff);
    }

    // TODO: This is not correct...
    static byte AcornDirectoryChecksum(IList<byte> data, int length)
    {
        uint sum = 0;

        if(length > data.Count)
            length = data.Count;

        // EOR r0, r1, r0, ROR #13
        for(int i = 0; i < length; i++)
        {
            uint carry = sum & 0x1FFF;
            sum >>= 13;
            sum ^=  data[i];
            sum +=  carry << 19;
        }

        return (byte)(((sum & 0xFF000000) >> 24) ^ ((sum & 0xFF0000) >> 16) ^ ((sum & 0xFF00) >> 8) ^ (sum & 0xFF));
    }
}