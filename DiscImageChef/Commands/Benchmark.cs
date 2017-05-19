// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Benchmark.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'benchmark' verb.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using DiscImageChef.Console;
using DiscImageChef.Checksums;
using System.Threading;

namespace DiscImageChef.Commands
{
    public static class Benchmark
    {
        public static void doBenchmark(BenchmarkOptions options)
        {
            int bufferSize = options.BufferSize * 1024 * 1024;
            long minMemory = long.MaxValue;
            long maxMemory = 0;
            MemoryStream ms = new MemoryStream(bufferSize);
            Random rnd = new Random();
            DateTime start;
            DateTime end;
            long mem;
            object ctx;
            double allSeparate = 0;
            System.Collections.Generic.Dictionary<string, double> checksumTimes = new System.Collections.Generic.Dictionary<string, double>();

            DicConsole.WriteLine();

            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rWriting block {0} of {1} with random data.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                rnd.NextBytes(tmp);
                ms.Write(tmp, 0, options.BlockSize);
            }
            end = DateTime.Now;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to fill buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);

            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rReading block {0} of {1}.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
            }
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to read buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);

            #region Adler32
            ctx = new Adler32Context();
            ((Adler32Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rChecksumming block {0} of {1} with Adler32.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
                ((Adler32Context)ctx).Update(tmp);
            }
            ((Adler32Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to Adler32 buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);
            allSeparate += (end - start).TotalSeconds;
            checksumTimes.Add("Adler32", (end - start).TotalSeconds);
            #endregion Adler32

            #region CRC16
            ctx = new CRC16Context();
            ((CRC16Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rChecksumming block {0} of {1} with CRC16.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
                ((CRC16Context)ctx).Update(tmp);
            }
            ((CRC16Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to CRC16 buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);
            allSeparate += (end - start).TotalSeconds;
            checksumTimes.Add("CRC16", (end - start).TotalSeconds);
            #endregion CRC16

            #region CRC32
            ctx = new CRC32Context();
            ((CRC32Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rChecksumming block {0} of {1} with CRC32.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
                ((CRC32Context)ctx).Update(tmp);
            }
            ((CRC32Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to CRC32 buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);
            allSeparate += (end - start).TotalSeconds;
            checksumTimes.Add("CRC32", (end - start).TotalSeconds);
            #endregion CRC32

            #region CRC64
            ctx = new CRC64Context();
            ((CRC64Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rChecksumming block {0} of {1} with CRC64.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
                ((CRC64Context)ctx).Update(tmp);
            }
            ((CRC64Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to CRC64 buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);
            allSeparate += (end - start).TotalSeconds;
            checksumTimes.Add("CRC64", (end - start).TotalSeconds);
            #endregion CRC64

            #region Fletcher32
            /* ctx = new Fletcher32Context();
            ((Fletcher32Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if (mem > maxMemory)
                maxMemory = mem;
            if (mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for (int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rChecksumming block {0} of {1} with Fletcher32.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
                ((Fletcher32Context)ctx).Update(tmp);
            }
            ((Fletcher32Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if (mem > maxMemory)
                maxMemory = mem;
            if (mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to Fletcher32 buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);
			allSeparate += (end-start).TotalSeconds;
			checksumTimes.Add("Fletcher32", (end - start).TotalSeconds);*/
            #endregion Fletcher32

            #region MD5
            ctx = new MD5Context();
            ((MD5Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rChecksumming block {0} of {1} with MD5.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
                ((MD5Context)ctx).Update(tmp);
            }
            ((MD5Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to MD5 buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);
            allSeparate += (end - start).TotalSeconds;
            checksumTimes.Add("MD5", (end - start).TotalSeconds);
            #endregion MD5

            #region RIPEMD160
            ctx = new RIPEMD160Context();
            ((RIPEMD160Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rChecksumming block {0} of {1} with RIPEMD160.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
                ((RIPEMD160Context)ctx).Update(tmp);
            }
            ((RIPEMD160Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to RIPEMD160 buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);
            allSeparate += (end - start).TotalSeconds;
            checksumTimes.Add("RIPEMD160", (end - start).TotalSeconds);
            #endregion RIPEMD160

            #region SHA1
            ctx = new SHA1Context();
            ((SHA1Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rChecksumming block {0} of {1} with SHA1.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
                ((SHA1Context)ctx).Update(tmp);
            }
            ((SHA1Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to SHA1 buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);
            allSeparate += (end - start).TotalSeconds;
            checksumTimes.Add("SHA1", (end - start).TotalSeconds);
            #endregion SHA1

            #region SHA256
            ctx = new SHA256Context();
            ((SHA256Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rChecksumming block {0} of {1} with SHA256.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
                ((SHA256Context)ctx).Update(tmp);
            }
            ((SHA256Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to SHA256 buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);
            allSeparate += (end - start).TotalSeconds;
            checksumTimes.Add("SHA256", (end - start).TotalSeconds);
            #endregion SHA256

            #region SHA384
            ctx = new SHA384Context();
            ((SHA384Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rChecksumming block {0} of {1} with SHA384.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
                ((SHA384Context)ctx).Update(tmp);
            }
            ((SHA384Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to SHA384 buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);
            allSeparate += (end - start).TotalSeconds;
            checksumTimes.Add("SHA384", (end - start).TotalSeconds);
            #endregion SHA384

            #region SHA512
            ctx = new SHA512Context();
            ((SHA512Context)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rChecksumming block {0} of {1} with SHA512.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
                ((SHA512Context)ctx).Update(tmp);
            }
            ((SHA512Context)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to SHA512 buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);
            allSeparate += (end - start).TotalSeconds;
            checksumTimes.Add("SHA512", (end - start).TotalSeconds);
            #endregion SHA512

            #region SpamSum
            ctx = new SpamSumContext();
            ((SpamSumContext)ctx).Init();
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rChecksumming block {0} of {1} with SpamSum.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
                ((SpamSumContext)ctx).Update(tmp);
            }
            ((SpamSumContext)ctx).End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to SpamSum buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);
            allSeparate += (end - start).TotalSeconds;
            checksumTimes.Add("SpamSum", (end - start).TotalSeconds);
            #endregion SpamSum

            #region Entropy
            ulong[] entTable = new ulong[256];
            ms.Seek(0, SeekOrigin.Begin);
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;
            start = DateTime.Now;
            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rEntropying block {0} of {1}.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);
                foreach(byte b in tmp)
                    entTable[b]++;
            }
            double entropy = 0;
            foreach(ulong l in entTable)
            {
#pragma warning disable IDE0004 // Without this specific cast, it gives incorrect values
                double frequency = (double)l / (double)bufferSize;
#pragma warning restore IDE0004 // Without this specific cast, it gives incorrect values
                entropy += -(frequency * Math.Log(frequency, 2));
            }
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to entropy buffer, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);
            double entropyTime = (end - start).TotalSeconds;
            #endregion Entropy

            #region Multitasking
            start = DateTime.Now;
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


            adler32ctx.Init();
            adlerPkt.context = adler32ctx;
            crc16ctx.Init();
            crc16Pkt.context = crc16ctx;
            crc32ctx.Init();
            crc32Pkt.context = crc32ctx;
            crc64ctx.Init();
            crc64Pkt.context = crc64ctx;
            //fletcher16ctx.Init();
            //fletcher16Pkt.context = fletcher16ctx;
            //fletcher32ctx.Init();
            //fletcher32Pkt.context = fletcher32ctx;
            md5ctx.Init();
            md5Pkt.context = md5ctx;
            ripemd160ctx.Init();
            ripemd160Pkt.context = ripemd160ctx;
            sha1ctx.Init();
            sha1Pkt.context = sha1ctx;
            sha256ctx.Init();
            sha256Pkt.context = sha256ctx;
            sha384ctx.Init();
            sha384Pkt.context = sha384ctx;
            sha512ctx.Init();
            sha512Pkt.context = sha512ctx;
            ssctx.Init();
            spamsumPkt.context = ssctx;

            for(int i = 0; i < bufferSize / options.BlockSize; i++)
            {
                DicConsole.Write("\rChecksumming block {0} of {1} with all algorithms at the same time.", i + 1, bufferSize / options.BlockSize);
                byte[] tmp = new byte[options.BlockSize];
                ms.Read(tmp, 0, options.BlockSize);

                adlerThread = new Thread(updateAdler);
                crc16Thread = new Thread(updateCRC16);
                crc32Thread = new Thread(updateCRC32);
                crc64Thread = new Thread(updateCRC64);
                //            fletcher16Thread = new Thread(updateFletcher16);
                //            fletcher32Thread = new Thread(updateFletcher32);
                md5Thread = new Thread(updateMD5);
                ripemd160Thread = new Thread(updateRIPEMD160);
                sha1Thread = new Thread(updateSHA1);
                sha256Thread = new Thread(updateSHA256);
                sha384Thread = new Thread(updateSHA384);
                sha512Thread = new Thread(updateSHA512);
                spamsumThread = new Thread(updateSpamSum);

                adlerPkt.data = tmp;
                adlerThread.Start(adlerPkt);
                crc16Pkt.data = tmp;
                crc16Thread.Start(crc16Pkt);
                crc32Pkt.data = tmp;
                crc32Thread.Start(crc32Pkt);
                crc64Pkt.data = tmp;
                crc64Thread.Start(crc64Pkt);
                //fletcher16Pkt.data = tmp;
                //fletcher16Thread.Start(fletcher16Pkt);
                //fletcher32Pkt.data = tmp;
                //fletcher32Thread.Start(fletcher32Pkt);
                md5Pkt.data = tmp;
                md5Thread.Start(md5Pkt);
                ripemd160Pkt.data = tmp;
                ripemd160Thread.Start(ripemd160Pkt);
                sha1Pkt.data = tmp;
                sha1Thread.Start(sha1Pkt);
                sha256Pkt.data = tmp;
                sha256Thread.Start(sha256Pkt);
                sha384Pkt.data = tmp;
                sha384Thread.Start(sha384Pkt);
                sha512Pkt.data = tmp;
                sha512Thread.Start(sha512Pkt);
                spamsumPkt.data = tmp;
                spamsumThread.Start(spamsumPkt);

                /* mem = GC.GetTotalMemory(false);
            	if (mem > maxMemory)
                	maxMemory = mem;
            	if (mem < minMemory)
	                minMemory = mem;*/

                while(adlerThread.IsAlive || crc16Thread.IsAlive ||
                       crc32Thread.IsAlive || crc64Thread.IsAlive ||
                       //fletcher16Thread.IsAlive || fletcher32Thread.IsAlive ||
                       md5Thread.IsAlive || ripemd160Thread.IsAlive ||
                       sha1Thread.IsAlive || sha256Thread.IsAlive ||
                       sha384Thread.IsAlive || sha512Thread.IsAlive ||
                       spamsumThread.IsAlive)
                {
                }

            }
            adler32ctx.End();
            crc16ctx.End();
            crc32ctx.End();
            crc64ctx.End();
            //fletcher16ctx.End();
            //fletcher32ctx.End();
            md5ctx.End();
            ripemd160ctx.End();
            sha1ctx.End();
            sha256ctx.End();
            sha384ctx.End();
            sha512ctx.End();
            ssctx.End();
            end = DateTime.Now;
            mem = GC.GetTotalMemory(false);
            if(mem > maxMemory)
                maxMemory = mem;
            if(mem < minMemory)
                minMemory = mem;

            DicConsole.WriteLine();
            DicConsole.WriteLine("Took {0} seconds to do all algorithms at the same time, {1} MiB/sec.", (end - start).TotalSeconds, (bufferSize / 1048576) / (end - start).TotalSeconds);

            #endregion

            DicConsole.WriteLine("Took {0} seconds to do all algorithms sequentially, {1} MiB/sec.", allSeparate, (bufferSize / 1048576) / allSeparate);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Max memory used is {0} bytes", maxMemory);
            DicConsole.WriteLine("Min memory used is {0} bytes", minMemory);

            Core.Statistics.AddCommand("benchmark");
            Core.Statistics.AddBenchmark(checksumTimes, entropyTime, (end - start).TotalSeconds, allSeparate, maxMemory, minMemory);
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

