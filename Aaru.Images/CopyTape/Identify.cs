// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies CopyTape images.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.DiscImages.CopyTape
{
    public partial class CopyTape
    {
        public bool Identify(IFilter imageFilter)
        {
            if(imageFilter.GetDataForkLength() <= 16) return false;

            byte[] header = new byte[16];

            Stream strm = imageFilter.GetDataForkStream();
            strm.Position = 0;
            strm.Read(header, 0, 16);

            string mark = Encoding.ASCII.GetString(header);

            Regex blockRx = new Regex(BlockRegex);
            Match blockMt = blockRx.Match(mark);

            if(!blockMt.Success) return false;

            string blkSize = blockMt.Groups["blockSize"].Value;

            if(string.IsNullOrWhiteSpace(blkSize)) return false;

            if(!uint.TryParse(blkSize, out uint blockSize)) return false;

            if(blockSize == 0 || blockSize + 17 >= imageFilter.GetDataForkLength()) return false;

            strm.Position += blockSize;

            int newLine = strm.ReadByte();

            return newLine == 0x0A;
        }
    }
}