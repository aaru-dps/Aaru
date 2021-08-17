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
//     Identifies Apple DiskCopy 4.2 disk images.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Claunia.Encoding;

namespace Aaru.DiscImages
{
    public sealed partial class DiskCopy42
    {
        /// <inheritdoc />
        public bool Identify(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer  = new byte[0x58];
            byte[] pString = new byte[64];
            stream.Read(buffer, 0, 0x58);

            // Incorrect pascal string length, not DC42
            if(buffer[0] > 63)
                return false;

            var tmpHeader = new Header();

            Array.Copy(buffer, 0, pString, 0, 64);

            tmpHeader.DiskName     = StringHandlers.PascalToString(pString, Encoding.GetEncoding("macintosh"));
            tmpHeader.DataSize     = BigEndianBitConverter.ToUInt32(buffer, 0x40);
            tmpHeader.TagSize      = BigEndianBitConverter.ToUInt32(buffer, 0x44);
            tmpHeader.DataChecksum = BigEndianBitConverter.ToUInt32(buffer, 0x48);
            tmpHeader.TagChecksum  = BigEndianBitConverter.ToUInt32(buffer, 0x4C);
            tmpHeader.Format       = buffer[0x50];
            tmpHeader.FmtByte      = buffer[0x51];
            tmpHeader.Valid        = buffer[0x52];
            tmpHeader.Reserved     = buffer[0x53];

            AaruConsole.DebugWriteLine("DC42 plugin", "tmp_header.diskName = \"{0}\"", tmpHeader.DiskName);
            AaruConsole.DebugWriteLine("DC42 plugin", "tmp_header.dataSize = {0} bytes", tmpHeader.DataSize);
            AaruConsole.DebugWriteLine("DC42 plugin", "tmp_header.tagSize = {0} bytes", tmpHeader.TagSize);
            AaruConsole.DebugWriteLine("DC42 plugin", "tmp_header.dataChecksum = 0x{0:X8}", tmpHeader.DataChecksum);
            AaruConsole.DebugWriteLine("DC42 plugin", "tmp_header.tagChecksum = 0x{0:X8}", tmpHeader.TagChecksum);
            AaruConsole.DebugWriteLine("DC42 plugin", "tmp_header.format = 0x{0:X2}", tmpHeader.Format);
            AaruConsole.DebugWriteLine("DC42 plugin", "tmp_header.fmtByte = 0x{0:X2}", tmpHeader.FmtByte);
            AaruConsole.DebugWriteLine("DC42 plugin", "tmp_header.valid = {0}", tmpHeader.Valid);
            AaruConsole.DebugWriteLine("DC42 plugin", "tmp_header.reserved = {0}", tmpHeader.Reserved);

            if(tmpHeader.Valid    != 1 ||
               tmpHeader.Reserved != 0)
                return false;

            // Some versions seem to incorrectly create little endian fields
            if(tmpHeader.DataSize + tmpHeader.TagSize + 0x54 != imageFilter.GetDataForkLength() &&
               tmpHeader.Format                              != kSigmaFormatTwiggy)
            {
                tmpHeader.DataSize     = BitConverter.ToUInt32(buffer, 0x40);
                tmpHeader.TagSize      = BitConverter.ToUInt32(buffer, 0x44);
                tmpHeader.DataChecksum = BitConverter.ToUInt32(buffer, 0x48);
                tmpHeader.TagChecksum  = BitConverter.ToUInt32(buffer, 0x4C);

                if(tmpHeader.DataSize + tmpHeader.TagSize + 0x54 != imageFilter.GetDataForkLength() &&
                   tmpHeader.Format                              != kSigmaFormatTwiggy)
                    return false;
            }

            if(tmpHeader.Format != kSonyFormat400K    &&
               tmpHeader.Format != kSonyFormat800K    &&
               tmpHeader.Format != kSonyFormat720K    &&
               tmpHeader.Format != kSonyFormat1440K   &&
               tmpHeader.Format != kSonyFormat1680K   &&
               tmpHeader.Format != kSigmaFormatTwiggy &&
               tmpHeader.Format != kNotStandardFormat)
            {
                AaruConsole.DebugWriteLine("DC42 plugin", "Unknown tmp_header.format = 0x{0:X2} value",
                                           tmpHeader.Format);

                return false;
            }

            if(tmpHeader.FmtByte != kSonyFmtByte400K          &&
               tmpHeader.FmtByte != kSonyFmtByte800K          &&
               tmpHeader.FmtByte != kSonyFmtByte800KIncorrect &&
               tmpHeader.FmtByte != kSonyFmtByteProDos        &&
               tmpHeader.FmtByte != kInvalidFmtByte           &&
               tmpHeader.FmtByte != kSigmaFmtByteTwiggy       &&
               tmpHeader.FmtByte != kFmtNotStandard           &&
               tmpHeader.FmtByte != kMacOSXFmtByte)
            {
                AaruConsole.DebugWriteLine("DC42 plugin", "Unknown tmp_header.fmtByte = 0x{0:X2} value",
                                           tmpHeader.FmtByte);

                return false;
            }

            if(tmpHeader.FmtByte != kInvalidFmtByte)
                return true;

            AaruConsole.DebugWriteLine("DC42 plugin", "Image says it's unformatted");

            return false;
        }
    }
}