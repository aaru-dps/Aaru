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
using DiscImageChef.Console;
using System.Threading;

namespace DiscImageChef.Commands
{
    public static class Checksum
    {
        // How many sectors to read at once
        const uint sectorsToRead = 256;

        public static void doChecksum(ChecksumSubOptions options)
        {
            DicConsole.DebugWriteLine("Checksum command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Checksum command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Checksum command", "--separated-tracks={0}", options.SeparatedTracks);
            DicConsole.DebugWriteLine("Checksum command", "--whole-disc={0}", options.WholeDisc);
            DicConsole.DebugWriteLine("Checksum command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("Checksum command", "--adler32={0}", options.DoAdler32);
            DicConsole.DebugWriteLine("Checksum command", "--crc16={0}", options.DoCRC16);
            DicConsole.DebugWriteLine("Checksum command", "--crc32={0}", options.DoCRC32);
            DicConsole.DebugWriteLine("Checksum command", "--crc64={0}", options.DoCRC64);
            DicConsole.DebugWriteLine("Checksum command", "--md5={0}", options.DoMD5);
            DicConsole.DebugWriteLine("Checksum command", "--ripemd160={0}", options.DoRIPEMD160);
            DicConsole.DebugWriteLine("Checksum command", "--sha1={0}", options.DoSHA1);
            DicConsole.DebugWriteLine("Checksum command", "--sha256={0}", options.DoSHA256);
            DicConsole.DebugWriteLine("Checksum command", "--sha384={0}", options.DoSHA384);
            DicConsole.DebugWriteLine("Checksum command", "--sha512={0}", options.DoSHA512);
            DicConsole.DebugWriteLine("Checksum command", "--spamsum={0}", options.DoSpamSum);

            ImagePlugin inputFormat = ImageFormat.Detect(options.InputFile);

            if (inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not checksumming");
                return;
            }

            inputFormat.OpenImage(options.InputFile);
            long maxMemory = GC.GetTotalMemory(false);
            long snapMemory;

            if (inputFormat.ImageInfo.imageHasPartitions)
            {
                try
                {
                    Adler32Context adler32ctx = new Adler32Context();
                    CRC16Context crc16ctx = new CRC16Context();
                    CRC32Context crc32ctx = new CRC32Context();
                    CRC64Context crc64ctx = new CRC64Context();
                    //Fletcher16Context fletcher16ctx = new Fletcher16Context();
                    //Fletcher32Context fletcher32ctx = new Fletcher32Context();
                    MD5Context md5ctx = new MD5Context();
                    RIPEMD160Context ripemd160ctx = new RIPEMD160Context();
                    SHA1Context sha1ctx = new SHA1Context();
                    SHA256Context sha256ctx = new SHA256Context();
                    SHA384Context sha384ctx = new SHA384Context();
                    SHA512Context sha512ctx = new SHA512Context();
                    SpamSumContext ssctx = new SpamSumContext();

                    Adler32Context adler32ctxTrack = new Adler32Context();
                    CRC16Context crc16ctxTrack = new CRC16Context();
                    CRC32Context crc32ctxTrack = new CRC32Context();
                    CRC64Context crc64ctxTrack = new CRC64Context();
                    //Fletcher16Context fletcher16ctxTrack = new Fletcher16Context();
                    //Fletcher32Context fletcher32ctxTrack = new Fletcher32Context();
                    MD5Context md5ctxTrack = new MD5Context();
                    RIPEMD160Context ripemd160ctxTrack = new RIPEMD160Context();
                    SHA1Context sha1ctxTrack = new SHA1Context();
                    SHA256Context sha256ctxTrack = new SHA256Context();
                    SHA384Context sha384ctxTrack = new SHA384Context();
                    SHA512Context sha512ctxTrack = new SHA512Context();
                    SpamSumContext ssctxTrack = new SpamSumContext();

                    Thread adlerThread = new Thread(updateAdler);
                    Thread crc16Thread = new Thread(updateCRC16);
                    Thread crc32Thread = new Thread(updateCRC32);
                    Thread crc64Thread = new Thread(updateCRC64);
                    //Thread fletcher16Thread = new Thread(updateFletcher16);
                    //Thread fletcher32Thread = new Thread(updateFletcher32);
                    Thread md5Thread = new Thread(updateMD5);
                    Thread ripemd160Thread = new Thread(updateRIPEMD160);
                    Thread sha1Thread = new Thread(updateSHA1);
                    Thread sha256Thread = new Thread(updateSHA256);
                    Thread sha384Thread = new Thread(updateSHA384);
                    Thread sha512Thread = new Thread(updateSHA512);
                    Thread spamsumThread = new Thread(updateSpamSum);

                    adlerPacket adlerPkt = new adlerPacket();
                    crc16Packet crc16Pkt = new crc16Packet();
                    crc32Packet crc32Pkt = new crc32Packet();
                    crc64Packet crc64Pkt = new crc64Packet();
                    //fletcher16Packet fletcher16Pkt = new fletcher16Packet();
                    //fletcher32Packet fletcher32Pkt = new fletcher32Packet();
                    md5Packet md5Pkt = new md5Packet();
                    ripemd160Packet ripemd160Pkt = new ripemd160Packet();
                    sha1Packet sha1Pkt = new sha1Packet();
                    sha256Packet sha256Pkt = new sha256Packet();
                    sha384Packet sha384Pkt = new sha384Packet();
                    sha512Packet sha512Pkt = new sha512Packet();
                    spamsumPacket spamsumPkt = new spamsumPacket();

                    adlerPacket adlerPktTrack = new adlerPacket();
                    crc16Packet crc16PktTrack = new crc16Packet();
                    crc32Packet crc32PktTrack = new crc32Packet();
                    crc64Packet crc64PktTrack = new crc64Packet();
                    //fletcher16Packet fletcher16PktTrack = new fletcher16Packet();
                    //fletcher32Packet fletcher32PktTrack = new fletcher32Packet();
                    md5Packet md5PktTrack = new md5Packet();
                    ripemd160Packet ripemd160PktTrack = new ripemd160Packet();
                    sha1Packet sha1PktTrack = new sha1Packet();
                    sha256Packet sha256PktTrack = new sha256Packet();
                    sha384Packet sha384PktTrack = new sha384Packet();
                    sha512Packet sha512PktTrack = new sha512Packet();
                    spamsumPacket spamsumPktTrack = new spamsumPacket();

                    if (options.WholeDisc)
                    {
                        if (options.DoAdler32)
                        {
                            adler32ctx.Init();
                            adlerPkt.context = adler32ctx;
                        }
                        if (options.DoCRC16)
                        {
                            crc16ctx.Init();
                            crc16Pkt.context = crc16ctx;
                        }
                        if (options.DoCRC32)
                        {
                            crc32ctx.Init();
                            crc32Pkt.context = crc32ctx;
                        }
                        if (options.DoCRC64)
                        {
                            crc64ctx.Init();
                            crc64Pkt.context = crc64ctx;
                        }
                        /*if (options.DoFletcher16)
                        {
                            fletcher16ctx.Init();
                            fletcher16Pkt.context = fletcher16ctx;
                        }
                        if (options.DoFletcher32)
                        {
                            fletcher32ctx.Init();
                            fletcher32Pkt.context = fletcher32ctx;
                        }*/
                        if (options.DoMD5)
                        {
                            md5ctx.Init();
                            md5Pkt.context = md5ctx;
                        }
                        if (options.DoRIPEMD160)
                        {
                            ripemd160ctx.Init();
                            ripemd160Pkt.context = ripemd160ctx;
                        }
                        if (options.DoSHA1)
                        {
                            sha1ctx.Init();
                            sha1Pkt.context = sha1ctx;
                        }
                        if (options.DoSHA256)
                        {
                            sha256ctx.Init();
                            sha256Pkt.context = sha256ctx;
                        }
                        if (options.DoSHA384)
                        {
                            sha384ctx.Init();
                            sha384Pkt.context = sha384ctx;
                        }
                        if (options.DoSHA512)
                        {
                            sha512ctx.Init();
                            sha512Pkt.context = sha512ctx;
                        }
                        if (options.DoSpamSum)
                        {
                            ssctx.Init();
                            spamsumPkt.context = ssctx;
                        }
                    }

                    ulong previousTrackEnd = 0;

                    List<Track> inputTracks = inputFormat.GetTracks();
                    foreach (Track currentTrack in inputTracks)
                    {
                        if ((currentTrack.TrackStartSector - previousTrackEnd) != 0 &&
                            options.WholeDisc)
                        {
                            for (ulong i = previousTrackEnd + 1; i < currentTrack.TrackStartSector; i++)
                            {
                                DicConsole.Write("\rHashing track-less sector {0}", i);

                                byte[] hiddenSector = inputFormat.ReadSector(i);

                                if (options.DoAdler32)
                                {
                                    adlerPkt.data = hiddenSector;
                                    adlerThread.Start(adlerPkt);
                                }
                                if (options.DoCRC16)
                                {
                                    crc16Pkt.data = hiddenSector;
                                    crc16Thread.Start(crc16Pkt);
                                }
                                if (options.DoCRC32)
                                {
                                    crc32Pkt.data = hiddenSector;
                                    crc32Thread.Start(crc32Pkt);
                                }
                                if (options.DoCRC64)
                                {
                                    crc64Pkt.data = hiddenSector;
                                    crc64Thread.Start(crc64Pkt);
                                }
                                /*if (options.DoFletcher16)
                                {
                                    fletcher16Pkt.data = hiddenSector;
                                    fletcher16Thread.Start(fletcher16Pkt);
                                }
                                if (options.DoFletcher32)
                                {
                                    fletcher32Pkt.data = hiddenSector;
                                    fletcher32Thread.Start(fletcher32Pkt);
                                }*/
                                if (options.DoMD5)
                                {
                                    md5Pkt.data = hiddenSector;
                                    md5Thread.Start(md5Pkt);
                                }
                                if (options.DoRIPEMD160)
                                {
                                    ripemd160Pkt.data = hiddenSector;
                                    ripemd160Thread.Start(ripemd160Pkt);
                                }
                                if (options.DoSHA1)
                                {
                                    sha1Pkt.data = hiddenSector;
                                    sha1Thread.Start(sha1Pkt);
                                }
                                if (options.DoSHA256)
                                {
                                    sha256Pkt.data = hiddenSector;
                                    sha256Thread.Start(sha256Pkt);
                                }
                                if (options.DoSHA384)
                                {
                                    sha384Pkt.data = hiddenSector;
                                    sha384Thread.Start(sha384Pkt);
                                }
                                if (options.DoSHA512)
                                {
                                    sha512Pkt.data = hiddenSector;
                                    sha512Thread.Start(sha512Pkt);
                                }
                                if (options.DoSpamSum)
                                {
                                    spamsumPkt.data = hiddenSector;
                                    spamsumThread.Start(spamsumPkt);
                                }

                                snapMemory = GC.GetTotalMemory(false);
                                if (snapMemory > maxMemory)
                                    maxMemory = snapMemory;

                                while (adlerThread.IsAlive || crc16Thread.IsAlive ||
                                    crc32Thread.IsAlive || crc64Thread.IsAlive ||
                                    //fletcher16Thread.IsAlive || fletcher32Thread.IsAlive ||
                                    md5Thread.IsAlive || ripemd160Thread.IsAlive ||
                                    sha1Thread.IsAlive || sha256Thread.IsAlive ||
                                    sha384Thread.IsAlive || sha512Thread.IsAlive ||
                                    spamsumThread.IsAlive)
                                {
                                }

                                adlerThread = new Thread(updateAdler);
                                crc16Thread = new Thread(updateCRC16);
                                crc32Thread = new Thread(updateCRC32);
                                crc64Thread = new Thread(updateCRC64);
                                //fletcher16Thread = new Thread(updateFletcher16);
                                //fletcher32Thread = new Thread(updateFletcher32);
                                md5Thread = new Thread(updateMD5);
                                ripemd160Thread = new Thread(updateRIPEMD160);
                                sha1Thread = new Thread(updateSHA1);
                                sha256Thread = new Thread(updateSHA256);
                                sha384Thread = new Thread(updateSHA384);
                                sha512Thread = new Thread(updateSHA512);
                                spamsumThread = new Thread(updateSpamSum);

                                snapMemory = GC.GetTotalMemory(false);
                                if (snapMemory > maxMemory)
                                    maxMemory = snapMemory;
                            }
                        }

                        DicConsole.DebugWriteLine("Checksum command", "Track {0} starts at sector {1} and ends at sector {2}", currentTrack.TrackSequence,
                            currentTrack.TrackStartSector, currentTrack.TrackEndSector);

                        if (options.SeparatedTracks)
                        {
                            if (options.DoAdler32)
                            {
                                adler32ctxTrack = new Adler32Context();
                                adler32ctxTrack.Init();
                                adlerPktTrack.context = adler32ctxTrack;
                            }
                            if (options.DoCRC16)
                            {
                                crc16ctxTrack = new CRC16Context();
                                crc16ctxTrack.Init();
                                crc16PktTrack.context = crc16ctxTrack;
                            }
                            if (options.DoCRC32)
                            {
                                crc32ctxTrack = new CRC32Context();
                                crc32ctxTrack.Init();
                                crc32PktTrack.context = crc32ctxTrack;
                            }
                            if (options.DoCRC64)
                            {
                                crc64ctxTrack = new CRC64Context();
                                crc64ctxTrack.Init();
                                crc64PktTrack.context = crc64ctxTrack;
                            }
                            /*if (options.DoFletcher16)
                            {
                                fletcher16ctxTrack = new Fletcher16Context();
                                fletcher16ctxTrack.Init();
                                fletcher16PktTrack.context = fletcher16ctxTrack;
                            }
                            if (options.DoFletcher32)
                            {
                                fletcher32ctxTrack = new Fletcher32Context();
                                fletcher32ctxTrack.Init();
                                fletcher32PktTrack.context = fletcher32ctxTrack;
                            }*/
                            if (options.DoMD5)
                            {
                                md5ctxTrack = new MD5Context();
                                md5ctxTrack.Init();
                                md5PktTrack.context = md5ctxTrack;
                            }
                            if (options.DoRIPEMD160)
                            {
                                ripemd160ctxTrack = new RIPEMD160Context();
                                ripemd160ctxTrack.Init();
                                ripemd160PktTrack.context = ripemd160ctxTrack;
                            }
                            if (options.DoSHA1)
                            {
                                sha1ctxTrack = new SHA1Context();
                                sha1ctxTrack.Init();
                                sha1PktTrack.context = sha1ctxTrack;
                            }
                            if (options.DoSHA256)
                            {
                                sha256ctxTrack = new SHA256Context();
                                sha256ctxTrack.Init();
                                sha256PktTrack.context = sha256ctxTrack;
                            }
                            if (options.DoSHA384)
                            {
                                sha384ctxTrack = new SHA384Context();
                                sha384ctxTrack.Init();
                                sha384PktTrack.context = sha384ctxTrack;
                            }
                            if (options.DoSHA512)
                            {
                                sha512ctxTrack = new SHA512Context();
                                sha512ctxTrack.Init();
                                sha512PktTrack.context = sha512ctxTrack;
                            }
                            if (options.DoSpamSum)
                            {
                                ssctxTrack = new SpamSumContext();
                                ssctxTrack.Init();
                                spamsumPktTrack.context = ssctxTrack;
                            }
                        }

                        ulong sectors = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        ulong doneSectors = 0;
                        DicConsole.WriteLine("Track {0} has {1} sectors", currentTrack.TrackSequence, sectors);

                        while (doneSectors < sectors)
                        {
                            byte[] sector;

                            if ((sectors - doneSectors) >= sectorsToRead)
                            {
                                sector = inputFormat.ReadSectors(doneSectors, sectorsToRead, currentTrack.TrackSequence);
                                DicConsole.Write("\rHashings sectors {0} to {2} of track {1}", doneSectors, currentTrack.TrackSequence, doneSectors + sectorsToRead);
                                doneSectors += sectorsToRead;
                            }
                            else
                            {
                                sector = inputFormat.ReadSectors(doneSectors, (uint)(sectors - doneSectors), currentTrack.TrackSequence);
                                DicConsole.Write("\rHashings sectors {0} to {2} of track {1}", doneSectors, currentTrack.TrackSequence, doneSectors + (sectors - doneSectors));
                                doneSectors += (sectors - doneSectors);
                            }

                            if (options.WholeDisc)
                            {
                                if (options.DoAdler32)
                                {
                                    adlerPkt.data = sector;
                                    adlerThread.Start(adlerPkt);
                                }
                                if (options.DoCRC16)
                                {
                                    crc16Pkt.data = sector;
                                    crc16Thread.Start(crc16Pkt);
                                }
                                if (options.DoCRC32)
                                {
                                    crc32Pkt.data = sector;
                                    crc32Thread.Start(crc32Pkt);
                                }
                                if (options.DoCRC64)
                                {
                                    crc64Pkt.data = sector;
                                    crc64Thread.Start(crc64Pkt);
                                }
                                /*if (options.DoFletcher16)
                                {
                                    fletcher16Pkt.data = sector;
                                    fletcher16Thread.Start(fletcher16Pkt);
                                }
                                if (options.DoFletcher32)
                                {
                                    fletcher32Pkt.data = sector;
                                    fletcher32Thread.Start(fletcher32Pkt);
                                }*/
                                if (options.DoMD5)
                                {
                                    md5Pkt.data = sector;
                                    md5Thread.Start(md5Pkt);
                                }
                                if (options.DoRIPEMD160)
                                {
                                    ripemd160Pkt.data = sector;
                                    ripemd160Thread.Start(ripemd160Pkt);
                                }
                                if (options.DoSHA1)
                                {
                                    sha1Pkt.data = sector;
                                    sha1Thread.Start(sha1Pkt);
                                }
                                if (options.DoSHA256)
                                {
                                    sha256Pkt.data = sector;
                                    sha256Thread.Start(sha256Pkt);
                                }
                                if (options.DoSHA384)
                                {
                                    sha384Pkt.data = sector;
                                    sha384Thread.Start(sha384Pkt);
                                }
                                if (options.DoSHA512)
                                {
                                    sha512Pkt.data = sector;
                                    sha512Thread.Start(sha512Pkt);
                                }
                                if (options.DoSpamSum)
                                {
                                    spamsumPkt.data = sector;
                                    spamsumThread.Start(spamsumPkt);
                                }

                                snapMemory = GC.GetTotalMemory(false);
                                if (snapMemory > maxMemory)
                                    maxMemory = snapMemory;

                                while (adlerThread.IsAlive || crc16Thread.IsAlive ||
                                    crc32Thread.IsAlive || crc64Thread.IsAlive ||
                                    //fletcher16Thread.IsAlive || fletcher32Thread.IsAlive ||
                                    md5Thread.IsAlive || ripemd160Thread.IsAlive ||
                                    sha1Thread.IsAlive || sha256Thread.IsAlive ||
                                    sha384Thread.IsAlive || sha512Thread.IsAlive ||
                                    spamsumThread.IsAlive)
                                {
                                }

                                adlerThread = new Thread(updateAdler);
                                crc16Thread = new Thread(updateCRC16);
                                crc32Thread = new Thread(updateCRC32);
                                crc64Thread = new Thread(updateCRC64);
                                //fletcher16Thread = new Thread(updateFletcher16);
                                //fletcher32Thread = new Thread(updateFletcher32);
                                md5Thread = new Thread(updateMD5);
                                ripemd160Thread = new Thread(updateRIPEMD160);
                                sha1Thread = new Thread(updateSHA1);
                                sha256Thread = new Thread(updateSHA256);
                                sha384Thread = new Thread(updateSHA384);
                                sha512Thread = new Thread(updateSHA512);
                                spamsumThread = new Thread(updateSpamSum);

                                snapMemory = GC.GetTotalMemory(false);
                                if (snapMemory > maxMemory)
                                    maxMemory = snapMemory;
                            }

                            if (options.SeparatedTracks)
                            {
                                if (options.DoAdler32)
                                {
                                    adlerPktTrack.data = sector;
                                    adlerThread.Start(adlerPktTrack);
                                }
                                if (options.DoCRC16)
                                {
                                    crc16PktTrack.data = sector;
                                    crc16Thread.Start(crc16PktTrack);
                                }
                                if (options.DoCRC32)
                                {
                                    crc32PktTrack.data = sector;
                                    crc32Thread.Start(crc32PktTrack);
                                }
                                if (options.DoCRC64)
                                {
                                    crc64PktTrack.data = sector;
                                    crc64Thread.Start(crc64PktTrack);
                                }
                                /*if (options.DoFletcher16)
                                {
                                    fletcher16PktTrack.data = sector;
                                    fletcher16Thread.Start(fletcher16PktTrack);
                                }
                                if (options.DoFletcher32)
                                {
                                    fletcher32PktTrack.data = sector;
                                    fletcher32Thread.Start(fletcher32PktTrack);
                                }*/
                                if (options.DoMD5)
                                {
                                    md5PktTrack.data = sector;
                                    md5Thread.Start(md5PktTrack);
                                }
                                if (options.DoRIPEMD160)
                                {
                                    ripemd160PktTrack.data = sector;
                                    ripemd160Thread.Start(ripemd160PktTrack);
                                }
                                if (options.DoSHA1)
                                {
                                    sha1PktTrack.data = sector;
                                    sha1Thread.Start(sha1PktTrack);
                                }
                                if (options.DoSHA256)
                                {
                                    sha256PktTrack.data = sector;
                                    sha256Thread.Start(sha256PktTrack);
                                }
                                if (options.DoSHA384)
                                {
                                    sha384PktTrack.data = sector;
                                    sha384Thread.Start(sha384PktTrack);
                                }
                                if (options.DoSHA512)
                                {
                                    sha512PktTrack.data = sector;
                                    sha512Thread.Start(sha512PktTrack);
                                }
                                if (options.DoSpamSum)
                                {
                                    spamsumPktTrack.data = sector;
                                    spamsumThread.Start(spamsumPktTrack);
                                }

                                snapMemory = GC.GetTotalMemory(false);
                                if (snapMemory > maxMemory)
                                    maxMemory = snapMemory;

                                while (adlerThread.IsAlive || crc16Thread.IsAlive ||
                                    crc32Thread.IsAlive || crc64Thread.IsAlive ||
                                    //fletcher16Thread.IsAlive || fletcher32Thread.IsAlive ||
                                    md5Thread.IsAlive || ripemd160Thread.IsAlive ||
                                    sha1Thread.IsAlive || sha256Thread.IsAlive ||
                                    sha384Thread.IsAlive || sha512Thread.IsAlive ||
                                    spamsumThread.IsAlive)
                                {
                                }

                                adlerThread = new Thread(updateAdler);
                                crc16Thread = new Thread(updateCRC16);
                                crc32Thread = new Thread(updateCRC32);
                                crc64Thread = new Thread(updateCRC64);
                                //fletcher16Thread = new Thread(updateFletcher16);
                                //fletcher32Thread = new Thread(updateFletcher32);
                                md5Thread = new Thread(updateMD5);
                                ripemd160Thread = new Thread(updateRIPEMD160);
                                sha1Thread = new Thread(updateSHA1);
                                sha256Thread = new Thread(updateSHA256);
                                sha384Thread = new Thread(updateSHA384);
                                sha512Thread = new Thread(updateSHA512);
                                spamsumThread = new Thread(updateSpamSum);

                                snapMemory = GC.GetTotalMemory(false);
                                if (snapMemory > maxMemory)
                                    maxMemory = snapMemory;
                            }
                        }

                        DicConsole.WriteLine();

                        if (options.SeparatedTracks)
                        {
                            if (options.DoAdler32)
                                DicConsole.WriteLine("Track {0}'s Adler-32: 0x{1}", currentTrack.TrackSequence, adler32ctxTrack.End());
                            if (options.DoCRC16)
                                DicConsole.WriteLine("Track {0}'s CRC16: 0x{1}", currentTrack.TrackSequence, crc16ctxTrack.End());
                            if (options.DoCRC32)
                                DicConsole.WriteLine("Track {0}'s CRC32: 0x{1}", currentTrack.TrackSequence, crc32ctxTrack.End());
                            if (options.DoCRC64)
                                DicConsole.WriteLine("Track {0}'s CRC64 (ECMA): 0x{1}", currentTrack.TrackSequence, crc64ctxTrack.End());
                            /*if (options.DoFletcher16)
                                DicConsole.WriteLine("Track {0}'s Fletcher-16: 0x{1}", currentTrack.TrackSequence, fletcher16ctxTrack.End());
                            if (options.DoFletcher32)
                                DicConsole.WriteLine("Track {0}'s Fletcher-32: 0x{1}", currentTrack.TrackSequence, fletcher32ctxTrack.End());*/
                            if (options.DoMD5)
                                DicConsole.WriteLine("Track {0}'s MD5: {1}", currentTrack.TrackSequence, md5ctxTrack.End());
                            if (options.DoRIPEMD160)
                                DicConsole.WriteLine("Track {0}'s RIPEMD160: {1}", currentTrack.TrackSequence, ripemd160ctxTrack.End());
                            if (options.DoSHA1)
                                DicConsole.WriteLine("Track {0}'s SHA1: {1}", currentTrack.TrackSequence, sha1ctxTrack.End());
                            if (options.DoSHA256)
                                DicConsole.WriteLine("Track {0}'s SHA256: {1}", currentTrack.TrackSequence, sha256ctxTrack.End());
                            if (options.DoSHA384)
                                DicConsole.WriteLine("Track {0}'s SHA384: {1}", currentTrack.TrackSequence, sha384ctxTrack.End());
                            if (options.DoSHA512)
                                DicConsole.WriteLine("Track {0}'s SHA512: {1}", currentTrack.TrackSequence, sha512ctxTrack.End());
                            if (options.DoSpamSum)
                                DicConsole.WriteLine("Track {0}'s SpamSum: {1}", currentTrack.TrackSequence, ssctxTrack.End());
                        }

                        previousTrackEnd = currentTrack.TrackEndSector;
                    }

                    if ((inputFormat.GetSectors() - previousTrackEnd) != 0 &&
                        options.WholeDisc)
                    {
                        for (ulong i = previousTrackEnd + 1; i < inputFormat.GetSectors(); i++)
                        {
                            DicConsole.Write("\rHashing track-less sector {0}", i);

                            byte[] hiddenSector = inputFormat.ReadSector(i);

                            if (options.DoAdler32)
                            {
                                adlerPkt.data = hiddenSector;
                                adlerThread.Start(adlerPkt);
                            }
                            if (options.DoCRC16)
                            {
                                crc16Pkt.data = hiddenSector;
                                crc16Thread.Start(crc16Pkt);
                            }
                            if (options.DoCRC32)
                            {
                                crc32Pkt.data = hiddenSector;
                                crc32Thread.Start(crc32Pkt);
                            }
                            if (options.DoCRC64)
                            {
                                crc64Pkt.data = hiddenSector;
                                crc64Thread.Start(crc64Pkt);
                            }
                            /*if (options.DoFletcher16)
                            {
                                fletcher16Pkt.data = hiddenSector;
                                fletcher16Thread.Start(fletcher16Pkt);
                            }
                            if (options.DoFletcher32)
                            {
                                fletcher32Pkt.data = hiddenSector;
                                fletcher32Thread.Start(fletcher32Pkt);
                            }*/
                            if (options.DoMD5)
                            {
                                md5Pkt.data = hiddenSector;
                                md5Thread.Start(md5Pkt);
                            }
                            if (options.DoRIPEMD160)
                            {
                                ripemd160Pkt.data = hiddenSector;
                                ripemd160Thread.Start(ripemd160Pkt);
                            }
                            if (options.DoSHA1)
                            {
                                sha1Pkt.data = hiddenSector;
                                sha1Thread.Start(sha1Pkt);
                            }
                            if (options.DoSHA256)
                            {
                                sha256Pkt.data = hiddenSector;
                                sha256Thread.Start(sha256Pkt);
                            }
                            if (options.DoSHA384)
                            {
                                sha384Pkt.data = hiddenSector;
                                sha384Thread.Start(sha384Pkt);
                            }
                            if (options.DoSHA512)
                            {
                                sha512Pkt.data = hiddenSector;
                                sha512Thread.Start(sha512Pkt);
                            }
                            if (options.DoSpamSum)
                            {
                                spamsumPkt.data = hiddenSector;
                                spamsumThread.Start(spamsumPkt);
                            }

                            snapMemory = GC.GetTotalMemory(false);
                            if (snapMemory > maxMemory)
                                maxMemory = snapMemory;

                            while (adlerThread.IsAlive || crc16Thread.IsAlive ||
                                crc32Thread.IsAlive || crc64Thread.IsAlive ||
                                //fletcher16Thread.IsAlive || fletcher32Thread.IsAlive ||
                                md5Thread.IsAlive || ripemd160Thread.IsAlive ||
                                sha1Thread.IsAlive || sha256Thread.IsAlive ||
                                sha384Thread.IsAlive || sha512Thread.IsAlive ||
                                spamsumThread.IsAlive)
                            {
                            }

                            adlerThread = new Thread(updateAdler);
                            crc16Thread = new Thread(updateCRC16);
                            crc32Thread = new Thread(updateCRC32);
                            crc64Thread = new Thread(updateCRC64);
                            //fletcher16Thread = new Thread(updateFletcher16);
                            //fletcher32Thread = new Thread(updateFletcher32);
                            md5Thread = new Thread(updateMD5);
                            ripemd160Thread = new Thread(updateRIPEMD160);
                            sha1Thread = new Thread(updateSHA1);
                            sha256Thread = new Thread(updateSHA256);
                            sha384Thread = new Thread(updateSHA384);
                            sha512Thread = new Thread(updateSHA512);
                            spamsumThread = new Thread(updateSpamSum);

                            snapMemory = GC.GetTotalMemory(false);
                            if (snapMemory > maxMemory)
                                maxMemory = snapMemory;
                        }
                    }

                    if (options.WholeDisc)
                    {
                        if (options.DoAdler32)
                            DicConsole.WriteLine("Disk's Adler-32: 0x{0}", adler32ctx.End());
                        if (options.DoCRC16)
                            DicConsole.WriteLine("Disk's CRC16: 0x{0}", crc16ctx.End());
                        if (options.DoCRC32)
                            DicConsole.WriteLine("Disk's CRC32: 0x{0}", crc32ctx.End());
                        if (options.DoCRC64)
                            DicConsole.WriteLine("Disk's CRC64 (ECMA): 0x{0}", crc64ctx.End());
                        /*if (options.DoFletcher16)
                            DicConsole.WriteLine("Disk's Fletcher-16: 0x{0}", fletcher16ctx.End());
                        if (options.DoFletcher32)
                            DicConsole.WriteLine("Disk's Fletcher-32: 0x{0}", fletcher32ctx.End());*/
                        if (options.DoMD5)
                            DicConsole.WriteLine("Disk's MD5: {0}", md5ctx.End());
                        if (options.DoRIPEMD160)
                            DicConsole.WriteLine("Disk's RIPEMD160: {0}", ripemd160ctx.End());
                        if (options.DoSHA1)
                            DicConsole.WriteLine("Disk's SHA1: {0}", sha1ctx.End());
                        if (options.DoSHA256)
                            DicConsole.WriteLine("Disk's SHA256: {0}", sha256ctx.End());
                        if (options.DoSHA384)
                            DicConsole.WriteLine("Disk's SHA384: {0}", sha384ctx.End());
                        if (options.DoSHA512)
                            DicConsole.WriteLine("Disk's SHA512: {0}", sha512ctx.End());
                        if (options.DoSpamSum)
                            DicConsole.WriteLine("Disk's SpamSum: {0}", ssctx.End());
                    }
                }
                catch (Exception ex)
                {
                    if (options.Debug)
                        DicConsole.DebugWriteLine("Could not get tracks because {0}", ex.Message);
                    else
                        DicConsole.WriteLine("Unable to get separate tracks, not checksumming them");
                }
            }
            else
            {
                Adler32Context adler32ctx = new Adler32Context();
                CRC16Context crc16ctx = new CRC16Context();
                CRC32Context crc32ctx = new CRC32Context();
                CRC64Context crc64ctx = new CRC64Context();
                //Fletcher16Context fletcher16ctx = new Fletcher16Context();
                //Fletcher32Context fletcher32ctx = new Fletcher32Context();
                MD5Context md5ctx = new MD5Context();
                RIPEMD160Context ripemd160ctx = new RIPEMD160Context();
                SHA1Context sha1ctx = new SHA1Context();
                SHA256Context sha256ctx = new SHA256Context();
                SHA384Context sha384ctx = new SHA384Context();
                SHA512Context sha512ctx = new SHA512Context();
                SpamSumContext ssctx = new SpamSumContext();

                Thread adlerThread = new Thread(updateAdler);
                Thread crc16Thread = new Thread(updateCRC16);
                Thread crc32Thread = new Thread(updateCRC32);
                Thread crc64Thread = new Thread(updateCRC64);
                //Thread fletcher16Thread = new Thread(updateFletcher16);
                //Thread fletcher32Thread = new Thread(updateFletcher32);
                Thread md5Thread = new Thread(updateMD5);
                Thread ripemd160Thread = new Thread(updateRIPEMD160);
                Thread sha1Thread = new Thread(updateSHA1);
                Thread sha256Thread = new Thread(updateSHA256);
                Thread sha384Thread = new Thread(updateSHA384);
                Thread sha512Thread = new Thread(updateSHA512);
                Thread spamsumThread = new Thread(updateSpamSum);

                adlerPacket adlerPkt = new adlerPacket();
                crc16Packet crc16Pkt = new crc16Packet();
                crc32Packet crc32Pkt = new crc32Packet();
                crc64Packet crc64Pkt = new crc64Packet();
                //fletcher16Packet fletcher16Pkt = new fletcher16Packet();
                //fletcher32Packet fletcher32Pkt = new fletcher32Packet();
                md5Packet md5Pkt = new md5Packet();
                ripemd160Packet ripemd160Pkt = new ripemd160Packet();
                sha1Packet sha1Pkt = new sha1Packet();
                sha256Packet sha256Pkt = new sha256Packet();
                sha384Packet sha384Pkt = new sha384Packet();
                sha512Packet sha512Pkt = new sha512Packet();
                spamsumPacket spamsumPkt = new spamsumPacket();

                if (options.DoAdler32)
                {
                    adler32ctx.Init();
                    adlerPkt.context = adler32ctx;
                }
                if (options.DoCRC16)
                {
                    crc16ctx.Init();
                    crc16Pkt.context = crc16ctx;
                }
                if (options.DoCRC32)
                {
                    crc32ctx.Init();
                    crc32Pkt.context = crc32ctx;
                }
                if (options.DoCRC64)
                {
                    crc64ctx.Init();
                    crc64Pkt.context = crc64ctx;
                }
                /*if (options.DoFletcher16)
                {
                    fletcher16ctx.Init();
                    fletcher16Pkt.context = fletcher16ctx;
                }
                if (options.DoFletcher32)
                {
                    fletcher32ctx.Init();
                    fletcher32Pkt.context = fletcher32ctx;
                }*/
                if (options.DoMD5)
                {
                    md5ctx.Init();
                    md5Pkt.context = md5ctx;
                }
                if (options.DoRIPEMD160)
                {
                    ripemd160ctx.Init();
                    ripemd160Pkt.context = ripemd160ctx;
                }
                if (options.DoSHA1)
                {
                    sha1ctx.Init();
                    sha1Pkt.context = sha1ctx;
                }
                if (options.DoSHA256)
                {
                    sha256ctx.Init();
                    sha256Pkt.context = sha256ctx;
                }
                if (options.DoSHA384)
                {
                    sha384ctx.Init();
                    sha384Pkt.context = sha384ctx;
                }
                if (options.DoSHA512)
                {
                    sha512ctx.Init();
                    sha512Pkt.context = sha512ctx;
                }
                if (options.DoSpamSum)
                {
                    ssctx.Init();
                    spamsumPkt.context = ssctx;
                }

                ulong sectors = inputFormat.GetSectors();
                DicConsole.WriteLine("Sectors {0}", sectors);
                ulong doneSectors = 0;

                while (doneSectors < sectors)
                {
                    byte[] sector;

                    if ((sectors - doneSectors) >= sectorsToRead)
                    {
                        sector = inputFormat.ReadSectors(doneSectors, sectorsToRead);
                        DicConsole.Write("\rHashings sectors {0} to {1}", doneSectors, doneSectors + sectorsToRead);
                        doneSectors += sectorsToRead;
                    }
                    else
                    {
                        sector = inputFormat.ReadSectors(doneSectors, (uint)(sectors - doneSectors));
                        DicConsole.Write("\rHashings sectors {0} to {1}", doneSectors, doneSectors + (sectors - doneSectors));
                        doneSectors += (sectors - doneSectors);
                    }

                    if (options.DoAdler32)
                    {
                        adlerPkt.data = sector;
                        adlerThread.Start(adlerPkt);
                    }
                    if (options.DoCRC16)
                    {
                        crc16Pkt.data = sector;
                        crc16Thread.Start(crc16Pkt);
                    }
                    if (options.DoCRC32)
                    {
                        crc32Pkt.data = sector;
                        crc32Thread.Start(crc32Pkt);
                    }
                    if (options.DoCRC64)
                    {
                        crc64Pkt.data = sector;
                        crc64Thread.Start(crc64Pkt);
                    }
                    /*if (options.DoFletcher16)
                    {
                        fletcher16Pkt.data = sector;
                        fletcher16Thread.Start(fletcher16Pkt);
                    }
                    if (options.DoFletcher32)
                    {
                        fletcher32Pkt.data = sector;
                        fletcher32Thread.Start(fletcher32Pkt);
                    }*/
                    if (options.DoMD5)
                    {
                        md5Pkt.data = sector;
                        md5Thread.Start(md5Pkt);
                    }
                    if (options.DoRIPEMD160)
                    {
                        ripemd160Pkt.data = sector;
                        ripemd160Thread.Start(ripemd160Pkt);
                    }
                    if (options.DoSHA1)
                    {
                        sha1Pkt.data = sector;
                        sha1Thread.Start(sha1Pkt);
                    }
                    if (options.DoSHA256)
                    {
                        sha256Pkt.data = sector;
                        sha256Thread.Start(sha256Pkt);
                    }
                    if (options.DoSHA384)
                    {
                        sha384Pkt.data = sector;
                        sha384Thread.Start(sha384Pkt);
                    }
                    if (options.DoSHA512)
                    {
                        sha512Pkt.data = sector;
                        sha512Thread.Start(sha512Pkt);
                    }
                    if (options.DoSpamSum)
                    {
                        spamsumPkt.data = sector;
                        spamsumThread.Start(spamsumPkt);
                    }

                    snapMemory = GC.GetTotalMemory(false);
                    if (snapMemory > maxMemory)
                        maxMemory = snapMemory;

                    while (adlerThread.IsAlive || crc16Thread.IsAlive ||
                        crc32Thread.IsAlive || crc64Thread.IsAlive ||
                        //fletcher16Thread.IsAlive || fletcher32Thread.IsAlive ||
                        md5Thread.IsAlive || ripemd160Thread.IsAlive ||
                        sha1Thread.IsAlive || sha256Thread.IsAlive ||
                        sha384Thread.IsAlive || sha512Thread.IsAlive ||
                        spamsumThread.IsAlive)
                    {
                    }

                    adlerThread = new Thread(updateAdler);
                    crc16Thread = new Thread(updateCRC16);
                    crc32Thread = new Thread(updateCRC32);
                    crc64Thread = new Thread(updateCRC64);
                    //fletcher16Thread = new Thread(updateFletcher16);
                    //fletcher32Thread = new Thread(updateFletcher32);
                    md5Thread = new Thread(updateMD5);
                    ripemd160Thread = new Thread(updateRIPEMD160);
                    sha1Thread = new Thread(updateSHA1);
                    sha256Thread = new Thread(updateSHA256);
                    sha384Thread = new Thread(updateSHA384);
                    sha512Thread = new Thread(updateSHA512);
                    spamsumThread = new Thread(updateSpamSum);

                    snapMemory = GC.GetTotalMemory(false);
                    if (snapMemory > maxMemory)
                        maxMemory = snapMemory;
                }

                DicConsole.WriteLine();

                if (options.DoAdler32)
                    DicConsole.WriteLine("Disk's Adler-32: 0x{0}", adler32ctx.End());
                if (options.DoCRC16)
                    DicConsole.WriteLine("Disk's CRC16: 0x{0}", crc16ctx.End());
                if (options.DoCRC32)
                    DicConsole.WriteLine("Disk's CRC32: 0x{0}", crc32ctx.End());
                if (options.DoCRC64)
                    DicConsole.WriteLine("Disk's CRC64 (ECMA): 0x{0}", crc64ctx.End());
                /*if (options.DoFletcher16)
                    DicConsole.WriteLine("Disk's Fletcher-16: 0x{0}", fletcher16ctx.End());
                if (options.DoFletcher32)
                    DicConsole.WriteLine("Disk's Fletcher-32: 0x{0}", fletcher32ctx.End());*/
                if (options.DoMD5)
                    DicConsole.WriteLine("Disk's MD5: {0}", md5ctx.End());
                if (options.DoRIPEMD160)
                    DicConsole.WriteLine("Disk's RIPEMD160: {0}", ripemd160ctx.End());
                if (options.DoSHA1)
                    DicConsole.WriteLine("Disk's SHA1: {0}", sha1ctx.End());
                if (options.DoSHA256)
                    DicConsole.WriteLine("Disk's SHA256: {0}", sha256ctx.End());
                if (options.DoSHA384)
                    DicConsole.WriteLine("Disk's SHA384: {0}", sha384ctx.End());
                if (options.DoSHA512)
                    DicConsole.WriteLine("Disk's SHA512: {0}", sha512ctx.End());
                if (options.DoSpamSum)
                    DicConsole.WriteLine("Disk's SpamSum: {0}", ssctx.End());
            }

            DicConsole.DebugWriteLine("Checksum command", "Maximum memory used has been {0} bytes", maxMemory);
        }

        #region Threading helpers

        struct adlerPacket
        {
            public Adler32Context context;
            public byte[] data;
        }

        struct crc16Packet
        {
            public CRC16Context context;
            public byte[] data;
        }

        struct crc32Packet
        {
            public CRC32Context context;
            public byte[] data;
        }

        struct crc64Packet
        {
            public CRC64Context context;
            public byte[] data;
        }

        /*struct fletcher16Packet
        {
            public Fletcher16Context context;
            public byte[] data;
        }

        struct fletcher32Packet
        {
            public Fletcher32Context context;
            public byte[] data;
        }*/

        struct md5Packet
        {
            public MD5Context context;
            public byte[] data;
        }

        struct ripemd160Packet
        {
            public RIPEMD160Context context;
            public byte[] data;
        }

        struct sha1Packet
        {
            public SHA1Context context;
            public byte[] data;
        }

        struct sha256Packet
        {
            public SHA256Context context;
            public byte[] data;
        }

        struct sha384Packet
        {
            public SHA384Context context;
            public byte[] data;
        }

        struct sha512Packet
        {
            public SHA512Context context;
            public byte[] data;
        }

        struct spamsumPacket
        {
            public SpamSumContext context;
            public byte[] data;
        }

        static void updateAdler(object packet)
        {
            ((adlerPacket)packet).context.Update(((adlerPacket)packet).data);
        }

        static void updateCRC16(object packet)
        {
            ((crc16Packet)packet).context.Update(((crc16Packet)packet).data);
        }

        static void updateCRC32(object packet)
        {
            ((crc32Packet)packet).context.Update(((crc32Packet)packet).data);
        }

        static void updateCRC64(object packet)
        {
            ((crc64Packet)packet).context.Update(((crc64Packet)packet).data);
        }

        /*static void updateFletcher16(object packet)
        {
            ((fletcher16Packet)packet).context.Update(((fletcher16Packet)packet).data);
        }

        static void updateFletcher32(object packet)
        {
            ((fletcher32Packet)packet).context.Update(((fletcher32Packet)packet).data);
        }*/

        static void updateMD5(object packet)
        {
            ((md5Packet)packet).context.Update(((md5Packet)packet).data);
        }

        static void updateRIPEMD160(object packet)
        {
            ((ripemd160Packet)packet).context.Update(((ripemd160Packet)packet).data);
        }

        static void updateSHA1(object packet)
        {
            ((sha1Packet)packet).context.Update(((sha1Packet)packet).data);
        }

        static void updateSHA256(object packet)
        {
            ((sha256Packet)packet).context.Update(((sha256Packet)packet).data);
        }

        static void updateSHA384(object packet)
        {
            ((sha384Packet)packet).context.Update(((sha384Packet)packet).data);
        }

        static void updateSHA512(object packet)
        {
            ((sha512Packet)packet).context.Update(((sha512Packet)packet).data);
        }

        static void updateSpamSum(object packet)
        {
            ((spamsumPacket)packet).context.Update(((spamsumPacket)packet).data);
        }

        #endregion Threading helpers
    }
}

