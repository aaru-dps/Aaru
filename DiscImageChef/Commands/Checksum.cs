/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Checksum.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Verbs.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Implements the 'checksum' verb.
 
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
using DiscImageChef.ImagePlugins;
using DiscImageChef.Checksums;
using System.Collections.Generic;

namespace DiscImageChef.Commands
{
    public static class Checksum
    {
        public static void doChecksum(ChecksumSubOptions options)
        {
            if (MainClass.isDebug)
            {
                Console.WriteLine("--debug={0}", options.Debug);
                Console.WriteLine("--verbose={0}", options.Verbose);
                Console.WriteLine("--separated-tracks={0}", options.SeparatedTracks);
                Console.WriteLine("--whole-disc={0}", options.WholeDisc);
                Console.WriteLine("--input={0}", options.InputFile);
                Console.WriteLine("--adler32={0}", options.DoAdler32);
                Console.WriteLine("--crc16={0}", options.DoCRC16);
                Console.WriteLine("--crc32={0}", options.DoCRC32);
                Console.WriteLine("--crc64={0}", options.DoCRC64);
                Console.WriteLine("--md5={0}", options.DoMD5);
                Console.WriteLine("--ripemd160={0}", options.DoRIPEMD160);
                Console.WriteLine("--sha1={0}", options.DoSHA1);
                Console.WriteLine("--sha256={0}", options.DoSHA256);
                Console.WriteLine("--sha384={0}", options.DoSHA384);
                Console.WriteLine("--sha512={0}", options.DoSHA512);
                Console.WriteLine("--spamsum={0}", options.DoSpamSum);
            }
            //throw new NotImplementedException("Checksumming not yet implemented.");

            ImagePlugin inputFormat = ImageFormat.Detect(options.InputFile);

            if (inputFormat == null)
            {
                Console.WriteLine("Unable to recognize image format, not checksumming");
                return;
            }

            inputFormat.OpenImage(options.InputFile);

            if (options.SeparatedTracks)
            {
                try
                {
                    List<Track> inputTracks = inputFormat.GetTracks();
                    foreach (Track currentTrack in inputTracks)
                    {
                        Adler32Context adler32ctxTrack = new Adler32Context();
                        CRC16Context crc16ctxTrack = new CRC16Context();
                        CRC32Context crc32ctxTrack = new CRC32Context();
                        CRC64Context crc64ctxTrack = new CRC64Context();
                        Fletcher16Context fletcher16ctxTrack = new Fletcher16Context();
                        Fletcher32Context fletcher32ctxTrack = new Fletcher32Context();
                        MD5Context md5ctxTrack = new MD5Context();
                        RIPEMD160Context ripemd160ctxTrack = new RIPEMD160Context();
                        SHA1Context sha1ctxTrack = new SHA1Context();
                        SHA256Context sha256ctxTrack = new SHA256Context();
                        SHA384Context sha384ctxTrack = new SHA384Context();
                        SHA512Context sha512ctxTrack = new SHA512Context();
                        SpamSumContext ssctxTrack = new SpamSumContext();

                        if (options.DoAdler32)
                            adler32ctxTrack.Init();
                        if (options.DoCRC16)
                            crc16ctxTrack.Init();
                        if (options.DoCRC32)
                            crc32ctxTrack.Init();
                        if (options.DoCRC64)
                            crc64ctxTrack.Init();
                        if (options.DoFletcher16)
                            fletcher16ctxTrack.Init();
                        if (options.DoFletcher32)
                            fletcher32ctxTrack.Init();
                        if (options.DoMD5)
                            md5ctxTrack.Init();
                        if (options.DoRIPEMD160)
                            ripemd160ctxTrack.Init();
                        if (options.DoSHA1)
                            sha1ctxTrack.Init();
                        if (options.DoSHA256)
                            sha256ctxTrack.Init();
                        if (options.DoSHA384)
                            sha384ctxTrack.Init();
                        if (options.DoSHA512)
                            sha512ctxTrack.Init();
                        if (options.DoSpamSum)
                            ssctxTrack.Init();

                        ulong sectors = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        Console.WriteLine("Track {0} has {1} sectors", currentTrack.TrackSequence, sectors);

                        for (ulong i = currentTrack.TrackStartSector; i <= currentTrack.TrackEndSector; i++)
                        {
                            Console.Write("\rHashing sector {0} of track {1}", i + 1, currentTrack.TrackSequence);
                            byte[] sector = inputFormat.ReadSector(i, currentTrack.TrackSequence);
                            if (options.DoAdler32)
                                adler32ctxTrack.Update(sector);
                            if (options.DoCRC16)
                                crc16ctxTrack.Update(sector);
                            if (options.DoCRC32)
                                crc32ctxTrack.Update(sector);
                            if (options.DoCRC64)
                                crc64ctxTrack.Update(sector);
                            if (options.DoFletcher16)
                                fletcher16ctxTrack.Update(sector);
                            if (options.DoFletcher32)
                                fletcher32ctxTrack.Update(sector);
                            if (options.DoMD5)
                                md5ctxTrack.Update(sector);
                            if (options.DoRIPEMD160)
                                ripemd160ctxTrack.Update(sector);
                            if (options.DoSHA1)
                                sha1ctxTrack.Update(sector);
                            if (options.DoSHA256)
                                sha256ctxTrack.Update(sector);
                            if (options.DoSHA384)
                                sha384ctxTrack.Update(sector);
                            if (options.DoSHA512)
                                sha512ctxTrack.Update(sector);
                            if (options.DoSpamSum)
                                ssctxTrack.Update(sector);
                        }

                        Console.WriteLine();

                        if (options.DoAdler32)
                            Console.WriteLine("Track {0}'s Adler-32: 0x{1}", currentTrack.TrackSequence, adler32ctxTrack.End());
                        if (options.DoCRC16)
                            Console.WriteLine("Track {0}'s CRC16: 0x{1}", currentTrack.TrackSequence, crc16ctxTrack.End());
                        if (options.DoCRC32)
                            Console.WriteLine("Track {0}'s CRC32: 0x{1}", currentTrack.TrackSequence, crc32ctxTrack.End());
                        if (options.DoCRC64)
                            Console.WriteLine("Track {0}'s CRC64 (ECMA): 0x{1}", currentTrack.TrackSequence, crc64ctxTrack.End());
                        if (options.DoFletcher16)
                            Console.WriteLine("Track {0}'s Fletcher-16: 0x{1}", currentTrack.TrackSequence, fletcher16ctxTrack.End());
                        if (options.DoFletcher32)
                            Console.WriteLine("Track {0}'s Fletcher-32: 0x{1}", currentTrack.TrackSequence, fletcher32ctxTrack.End());
                        if (options.DoMD5)
                            Console.WriteLine("Track {0}'s MD5: {1}", currentTrack.TrackSequence, md5ctxTrack.End());
                        if (options.DoRIPEMD160)
                            Console.WriteLine("Track {0}'s RIPEMD160: {1}", currentTrack.TrackSequence, ripemd160ctxTrack.End());
                        if (options.DoSHA1)
                            Console.WriteLine("Track {0}'s SHA1: {1}", currentTrack.TrackSequence, sha1ctxTrack.End());
                        if (options.DoSHA256)
                            Console.WriteLine("Track {0}'s SHA256: {1}", currentTrack.TrackSequence, sha256ctxTrack.End());
                        if (options.DoSHA384)
                            Console.WriteLine("Track {0}'s SHA384: {1}", currentTrack.TrackSequence, sha384ctxTrack.End());
                        if (options.DoSHA512)
                            Console.WriteLine("Track {0}'s SHA512: {1}", currentTrack.TrackSequence, sha512ctxTrack.End());
                        if (options.DoSpamSum)
                            Console.WriteLine("Track {0}'s SpamSum: {1}", currentTrack.TrackSequence, ssctxTrack.End());
                    }
                }
                catch (Exception ex)
                {
                    if (options.Debug)
                        Console.WriteLine("Could not get tracks because {0}", ex.Message);
                    else
                        Console.WriteLine("Unable to get separate tracks, not checksumming them");
                }
            }


            if (options.WholeDisc)
            {
                Adler32Context adler32ctx = new Adler32Context();
                CRC16Context crc16ctx = new CRC16Context();
                CRC32Context crc32ctx = new CRC32Context();
                CRC64Context crc64ctx = new CRC64Context();
                Fletcher16Context fletcher16ctx = new Fletcher16Context();
                Fletcher32Context fletcher32ctx = new Fletcher32Context();
                MD5Context md5ctx = new MD5Context();
                RIPEMD160Context ripemd160ctx = new RIPEMD160Context();
                SHA1Context sha1ctx = new SHA1Context();
                SHA256Context sha256ctx = new SHA256Context();
                SHA384Context sha384ctx = new SHA384Context();
                SHA512Context sha512ctx = new SHA512Context();
                SpamSumContext ssctx = new SpamSumContext();

                if (options.DoAdler32)
                    adler32ctx.Init();
                if (options.DoCRC16)
                    crc16ctx.Init();
                if (options.DoCRC32)
                    crc32ctx.Init();
                if (options.DoCRC64)
                    crc64ctx.Init();
                if (options.DoFletcher16)
                    fletcher16ctx.Init();
                if (options.DoFletcher32)
                    fletcher32ctx.Init();
                if (options.DoMD5)
                    md5ctx.Init();
                if (options.DoRIPEMD160)
                    ripemd160ctx.Init();
                if (options.DoSHA1)
                    sha1ctx.Init();
                if (options.DoSHA256)
                    sha256ctx.Init();
                if (options.DoSHA384)
                    sha384ctx.Init();
                if (options.DoSHA512)
                    sha512ctx.Init();
                if (options.DoSpamSum)
                    ssctx.Init();

                ulong sectors = inputFormat.GetSectors();
                Console.WriteLine("Sectors {0}", sectors);

                for (ulong i = 0; i < sectors; i++)
                {
                    Console.Write("\rHashing sector {0}", i + 1);
                    byte[] sector = inputFormat.ReadSector(i);
                    if (options.DoAdler32)
                        adler32ctx.Update(sector);
                    if (options.DoCRC16)
                        crc16ctx.Update(sector);
                    if (options.DoCRC32)
                        crc32ctx.Update(sector);
                    if (options.DoCRC64)
                        crc64ctx.Update(sector);
                    if (options.DoFletcher16)
                        crc64ctx.Update(sector);
                    if (options.DoFletcher32)
                        crc64ctx.Update(sector);
                    if (options.DoMD5)
                        md5ctx.Update(sector);
                    if (options.DoRIPEMD160)
                        ripemd160ctx.Update(sector);
                    if (options.DoSHA1)
                        sha1ctx.Update(sector);
                    if (options.DoSHA256)
                        sha256ctx.Update(sector);
                    if (options.DoSHA384)
                        sha384ctx.Update(sector);
                    if (options.DoSHA512)
                        sha512ctx.Update(sector);
                    if (options.DoSpamSum)
                        ssctx.Update(sector);
                }

                Console.WriteLine();

                if (options.DoAdler32)
                    Console.WriteLine("Disk's Adler-32: 0x{0}", adler32ctx.End());
                if (options.DoCRC16)
                    Console.WriteLine("Disk's CRC16: 0x{0}", crc16ctx.End());
                if (options.DoCRC32)
                    Console.WriteLine("Disk's CRC32: 0x{0}", crc32ctx.End());
                if (options.DoCRC64)
                    Console.WriteLine("Disk's CRC64 (ECMA): 0x{0}", crc64ctx.End());
                if (options.DoFletcher16)
                    Console.WriteLine("Disk's Fletcher-16: 0x{0}", fletcher16ctx.End());
                if (options.DoFletcher32)
                    Console.WriteLine("Disk's Fletcher-32: 0x{0}", fletcher32ctx.End());
                if (options.DoMD5)
                    Console.WriteLine("Disk's MD5: {0}", md5ctx.End());
                if (options.DoRIPEMD160)
                    Console.WriteLine("Disk's RIPEMD160: {0}", ripemd160ctx.End());
                if (options.DoSHA1)
                    Console.WriteLine("Disk's SHA1: {0}", sha1ctx.End());
                if (options.DoSHA256)
                    Console.WriteLine("Disk's SHA256: {0}", sha256ctx.End());
                if (options.DoSHA384)
                    Console.WriteLine("Disk's SHA384: {0}", sha384ctx.End());
                if (options.DoSHA512)
                    Console.WriteLine("Disk's SHA512: {0}", sha512ctx.End());
                if (options.DoSpamSum)
                    Console.WriteLine("Disk's SpamSum: {0}", ssctx.End());
            }
        }
    }
}

