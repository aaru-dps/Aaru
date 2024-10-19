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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.Checksums;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Images;

public sealed partial class CdrWin
{
#region IVerifiableImage Members

    /// <inheritdoc />
    public bool? VerifyMediaImage()
    {
        if(_discImage.DiscHashes.Count == 0) return null;

        // Read up to 1 MiB at a time for verification
        const int verifySize = 1024 * 1024;
        long      readBytes;
        byte[]    verifyBytes;

        IFilter[] filters = _discImage.Tracks.OrderBy(t => t.Sequence)
                                      .Select(t => t.TrackFile.DataFilter)
                                      .Distinct()
                                      .ToArray();

        if(_discImage.DiscHashes.TryGetValue("sha1", out string sha1))
        {
            var ctx = new Sha1Context();

            foreach(Stream stream in filters.Select(filter => filter.GetDataForkStream()))
            {
                readBytes   = 0;
                verifyBytes = new byte[verifySize];

                while(readBytes + verifySize < stream.Length)
                {
                    stream.EnsureRead(verifyBytes, 0, verifyBytes.Length);
                    ctx.Update(verifyBytes);
                    readBytes += verifyBytes.LongLength;
                }

                verifyBytes = new byte[stream.Length - readBytes];
                stream.EnsureRead(verifyBytes, 0, verifyBytes.Length);
                ctx.Update(verifyBytes);
            }

            string verifySha1 = ctx.End();
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Calculated_SHA1_0, verifySha1);
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Expected_SHA1_0,   sha1);

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
                    stream.EnsureRead(verifyBytes, 0, verifyBytes.Length);
                    ctx.Update(verifyBytes);
                    readBytes += verifyBytes.LongLength;
                }

                verifyBytes = new byte[stream.Length - readBytes];
                stream.EnsureRead(verifyBytes, 0, verifyBytes.Length);
                ctx.Update(verifyBytes);
            }

            string verifyMd5 = ctx.End();
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Calculated_MD5_0, verifyMd5);
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Expected_MD5_0,   md5);

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
                    stream.EnsureRead(verifyBytes, 0, verifyBytes.Length);
                    ctx.Update(verifyBytes);
                    readBytes += verifyBytes.LongLength;
                }

                verifyBytes = new byte[stream.Length - readBytes];
                stream.EnsureRead(verifyBytes, 0, verifyBytes.Length);
                ctx.Update(verifyBytes);
            }

            string verifyCrc = ctx.End();
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Calculated_CRC32_0, verifyCrc);
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Expected_CRC32_0,   crc32);

            return verifyCrc == crc32;
        }

        foreach(string hash in _discImage.DiscHashes.Keys)
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_unsupported_hash_0, hash);

        return null;
    }

#endregion

#region IWritableOpticalImage Members

    /// <inheritdoc />
    public bool? VerifySector(ulong sectorAddress)
    {
        ErrorNumber errno = ReadSectorLong(sectorAddress, out byte[] buffer);

        return errno != ErrorNumber.NoError ? null : CdChecksums.CheckCdSector(buffer);
    }

    /// <inheritdoc />
    public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                               out List<ulong> unknownLbas)
    {
        failingLbas = [];
        unknownLbas = [];
        ErrorNumber errno = ReadSectorsLong(sectorAddress, length, out byte[] buffer);

        if(errno != ErrorNumber.NoError) return null;

        var bps    = (int)(buffer.Length / length);
        var sector = new byte[bps];

        for(var i = 0; i < length; i++)
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

        if(unknownLbas.Count > 0) return null;

        return failingLbas.Count <= 0;
    }

    /// <inheritdoc />
    public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                               out List<ulong> unknownLbas)
    {
        failingLbas = [];
        unknownLbas = [];
        ErrorNumber errno = ReadSectorsLong(sectorAddress, length, track, out byte[] buffer);

        if(errno != ErrorNumber.NoError) return null;

        var bps    = (int)(buffer.Length / length);
        var sector = new byte[bps];

        for(var i = 0; i < length; i++)
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

        if(unknownLbas.Count > 0) return null;

        return failingLbas.Count <= 0;
    }

#endregion
}