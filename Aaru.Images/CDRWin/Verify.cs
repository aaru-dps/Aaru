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
//     Verifies CDRWin format disc images.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.Checksums;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;

namespace Aaru.DiscImages
{
    public sealed partial class CdrWin
    {
        /// <inheritdoc />
        public bool? VerifyMediaImage()
        {
            if(_discImage.DiscHashes.Count == 0)
                return null;

            // Read up to 1 MiB at a time for verification
            const int verifySize = 1024 * 1024;
            long      readBytes;
            byte[]    verifyBytes;

            IFilter[] filters = _discImage.Tracks.OrderBy(t => t.Sequence).Select(t => t.TrackFile.DataFilter).
                                           Distinct().ToArray();

            if(_discImage.DiscHashes.TryGetValue("sha1", out string sha1))
            {
                var ctx = new Sha1Context();

                foreach(Stream stream in filters.Select(filter => filter.GetDataForkStream()))
                {
                    readBytes   = 0;
                    verifyBytes = new byte[verifySize];

                    while(readBytes + verifySize < stream.Length)
                    {
                        stream.Read(verifyBytes, 0, verifyBytes.Length);
                        ctx.Update(verifyBytes);
                        readBytes += verifyBytes.LongLength;
                    }

                    verifyBytes = new byte[stream.Length - readBytes];
                    stream.Read(verifyBytes, 0, verifyBytes.Length);
                    ctx.Update(verifyBytes);
                }

                string verifySha1 = ctx.End();
                AaruConsole.DebugWriteLine("CDRWin plugin", "Calculated SHA1: {0}", verifySha1);
                AaruConsole.DebugWriteLine("CDRWin plugin", "Expected SHA1: {0}", sha1);

                return verifySha1 == sha1;
            }

            if(_discImage.DiscHashes.TryGetValue("md5", out string md5))
            {
                var ctx = new Md5Context();

                foreach(Stream stream in filters.Select(filter => filter.GetDataForkStream()))
                {
                    readBytes   = 0;
                    verifyBytes = new byte[verifySize];

                    while(readBytes + verifySize < stream.Length)
                    {
                        stream.Read(verifyBytes, 0, verifyBytes.Length);
                        ctx.Update(verifyBytes);
                        readBytes += verifyBytes.LongLength;
                    }

                    verifyBytes = new byte[stream.Length - readBytes];
                    stream.Read(verifyBytes, 0, verifyBytes.Length);
                    ctx.Update(verifyBytes);
                }

                string verifyMd5 = ctx.End();
                AaruConsole.DebugWriteLine("CDRWin plugin", "Calculated MD5: {0}", verifyMd5);
                AaruConsole.DebugWriteLine("CDRWin plugin", "Expected MD5: {0}", md5);

                return verifyMd5 == md5;
            }

            if(_discImage.DiscHashes.TryGetValue("crc32", out string crc32))
            {
                var ctx = new Crc32Context();

                foreach(Stream stream in filters.Select(filter => filter.GetDataForkStream()))
                {
                    readBytes   = 0;
                    verifyBytes = new byte[verifySize];

                    while(readBytes + verifySize < stream.Length)
                    {
                        stream.Read(verifyBytes, 0, verifyBytes.Length);
                        ctx.Update(verifyBytes);
                        readBytes += verifyBytes.LongLength;
                    }

                    verifyBytes = new byte[stream.Length - readBytes];
                    stream.Read(verifyBytes, 0, verifyBytes.Length);
                    ctx.Update(verifyBytes);
                }

                string verifyCrc = ctx.End();
                AaruConsole.DebugWriteLine("CDRWin plugin", "Calculated CRC32: {0}", verifyCrc);
                AaruConsole.DebugWriteLine("CDRWin plugin", "Expected CRC32: {0}", crc32);

                return verifyCrc == crc32;
            }

            foreach(string hash in _discImage.DiscHashes.Keys)
                AaruConsole.DebugWriteLine("CDRWin plugin", "Found unsupported hash {0}", hash);

            return null;
        }

        /// <inheritdoc />
        public bool? VerifySector(ulong sectorAddress)
        {
            ErrorNumber errno = ReadSectorLong(sectorAddress, out byte[] buffer);

            return errno != ErrorNumber.NoError ? null : CdChecksums.CheckCdSector(buffer);
        }

        /// <inheritdoc />
        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            ErrorNumber errno = ReadSectorsLong(sectorAddress, length, out byte[] buffer);

            if(errno != ErrorNumber.NoError)
                return null;

            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = CdChecksums.CheckCdSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        unknownLbas.Add((ulong)i + sectorAddress);

                        break;
                    case false:
                        failingLbas.Add((ulong)i + sectorAddress);

                        break;
                }
            }

            if(unknownLbas.Count > 0)
                return null;

            return failingLbas.Count <= 0;
        }

        /// <inheritdoc />
        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = CdChecksums.CheckCdSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        unknownLbas.Add((ulong)i + sectorAddress);

                        break;
                    case false:
                        failingLbas.Add((ulong)i + sectorAddress);

                        break;
                }
            }

            if(unknownLbas.Count > 0)
                return null;

            return failingLbas.Count <= 0;
        }
    }
}