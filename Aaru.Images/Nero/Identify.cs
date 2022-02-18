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
//     Identifies Nero Burning ROM disc images.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class Nero
    {
        /// <inheritdoc />
        public bool Identify(IFilter imageFilter)
        {
            _imageStream = imageFilter.GetDataForkStream();
            var footerV1 = new FooterV1();
            var footerV2 = new FooterV2();

            _imageStream.Seek(-8, SeekOrigin.End);
            byte[] buffer = new byte[8];
            _imageStream.Read(buffer, 0, 8);
            footerV1.ChunkId          = BigEndianBitConverter.ToUInt32(buffer, 0);
            footerV1.FirstChunkOffset = BigEndianBitConverter.ToUInt32(buffer, 4);

            _imageStream.Seek(-12, SeekOrigin.End);
            buffer = new byte[12];
            _imageStream.Read(buffer, 0, 12);
            footerV2.ChunkId          = BigEndianBitConverter.ToUInt32(buffer, 0);
            footerV2.FirstChunkOffset = BigEndianBitConverter.ToUInt64(buffer, 4);

            AaruConsole.DebugWriteLine("Nero plugin", "imageStream.Length = {0}", _imageStream.Length);
            AaruConsole.DebugWriteLine("Nero plugin", "footerV1.ChunkID = 0x{0:X8}", footerV1.ChunkId);
            AaruConsole.DebugWriteLine("Nero plugin", "footerV1.FirstChunkOffset = {0}", footerV1.FirstChunkOffset);
            AaruConsole.DebugWriteLine("Nero plugin", "footerV2.ChunkID = 0x{0:X8}", footerV2.ChunkId);
            AaruConsole.DebugWriteLine("Nero plugin", "footerV2.FirstChunkOffset = {0}", footerV2.FirstChunkOffset);

            if(footerV2.ChunkId          == NERO_FOOTER_V2 &&
               footerV2.FirstChunkOffset < (ulong)_imageStream.Length)
                return true;

            return footerV1.ChunkId == NERO_FOOTER_V1 && footerV1.FirstChunkOffset < (ulong)_imageStream.Length;
        }
    }
}