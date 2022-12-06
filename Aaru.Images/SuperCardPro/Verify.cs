// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Verify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Verifies SuperCardPro flux images.
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
using System.Collections.Generic;

namespace Aaru.DiscImages
{
    public sealed partial class SuperCardPro
    {
        /// <inheritdoc />
        public bool? VerifyMediaImage()
        {
            if(Header.flags.HasFlag(ScpFlags.Writable))
                return null;

            byte[] wholeFile = new byte[_scpStream.Length];
            uint   sum       = 0;

            _scpStream.Position = 0;
            _scpStream.Read(wholeFile, 0, wholeFile.Length);

            for(int i = 0x10; i < wholeFile.Length; i++)
                sum += wholeFile[i];

            return Header.checksum == sum;
        }

        /// <inheritdoc />
        public bool? VerifySector(ulong sectorAddress) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        /// <inheritdoc />
        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");
    }
}